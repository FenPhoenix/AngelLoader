using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using AngelLoader.Common;
using AngelLoader.Common.DataClasses;
using AngelLoader.Common.Utility;
using AngelLoader.Ini;
using FFmpeg.NET;
using static AngelLoader.Common.Logger;
using static AngelLoader.Common.Utility.Methods;

namespace AngelLoader
{//
    internal class AudioConverter
    {
        private readonly FanMission FM;
        private readonly string InstalledFMsBasePath;

        internal AudioConverter(FanMission fm, string installedFMsBasePath)
        {
            FM = fm;
            InstalledFMsBasePath = installedFMsBasePath;
        }

        // TODO: ffmpeg can do multiple files in one run. Switch to that, and see if ffprobe can do it too.

        // OpenAL doesn't play nice with anything over 16 bits, blasting out white noise when it tries to play
        // such. Converting all >16bit wavs to 16 bit fixes this.
        internal async Task ConvertWAVsTo16Bit()
        {
            if (!GameIsDark(FM)) return;

            if (!File.Exists(Paths.FFprobeExe) || !File.Exists(Paths.FFmpegExe))
            {
                Log("FFmpeg.exe or FFProbe.exe don't exist", stackTrace: true);
                return;
            }

            int GetBitDepthFast(string file)
            {
                // In case we read past the end of the file or can't open the file or whatever. We're trying to
                // be fast, so don't check explicitly. If there's a more serious IO problem, we'll catch it in a
                // minute.
                try
                {
                    using (var fs = new FileStream(file, FileMode.Open, FileAccess.Read))
                    using (var br = new BinaryReader(fs, Encoding.ASCII))
                    {
                        var riff = Encoding.ASCII.GetString(br.ReadBytes(4));
                        if (riff != "RIFF") return -1;
                        br.ReadBytes(4);
                        var wave = Encoding.ASCII.GetString(br.ReadBytes(4));
                        if (wave != "WAVE") return 0;
                        var fmt = Encoding.ASCII.GetString(br.ReadBytes(4));
                        if (fmt != "fmt ") return 0;
                        br.ReadBytes(18);
                        ushort bits = br.ReadUInt16();
                        return bits;
                    }
                }
                catch (Exception)
                {
                    return 0;
                }
            }

            // TODO: I could maybe speed this up by having the process not be recreated all the time?
            // I suspect it may be just the fact that it's a separate program that's constantly being started and
            // stopped. If that's the case, MEH. :\
            int GetBitDepthSlow(string file)
            {
                int ret = 0;

                using (var p = new Process { EnableRaisingEvents = true })
                {
                    p.StartInfo.FileName = Paths.FFprobeExe;
                    p.StartInfo.RedirectStandardOutput = true;
                    p.StartInfo.Arguments = "-show_format -show_streams -hide_banner \"" + file + "\"";
                    p.StartInfo.CreateNoWindow = true;
                    p.StartInfo.UseShellExecute = false;

                    p.OutputDataReceived += (sender, e) =>
                    {
                        if (e.Data.IsEmpty()) return;

                        using (var sr = new StringReader(e.Data))
                        {
                            string line;
                            while ((line = sr.ReadLine()) != null)
                            {
                                if (line.StartsWithFast_NoNullChecks("bits_per_sample=") &&
                                    int.TryParse(line.Substring(line.IndexOf('=') + 1), out int result))
                                {
                                    ret = result;
                                    break;
                                }
                            }
                        }
                    };

                    p.Start();
                    p.BeginOutputReadLine();

                    p.WaitForExit();
                }
                return ret;
            }

            await Task.Run(async () =>
            {
                try
                {
                    var fmSndPath = GetFMSoundPathByGame();
                    if (!Directory.Exists(fmSndPath)) return;

                    _ = new DirectoryInfo(fmSndPath) { Attributes = FileAttributes.Normal };

                    var wavFiles = Directory.EnumerateFiles(fmSndPath, "*.wav", SearchOption.AllDirectories);
                    foreach (var f in wavFiles)
                    {
                        UnSetReadOnly(f);

                        int bits = GetBitDepthFast(f);

                        // Header wasn't wav, so skip this one
                        if (bits == -1) continue;

                        if (bits == 0) bits = GetBitDepthSlow(f);
                        if (bits >= 1 && bits <= 16) continue;

                        var engine = new Engine(Paths.FFmpegExe);
                        var options = new ConversionOptions { AudioBitRate = 16 };
                        var inFile = new MediaFile(f);
                        var outFile = new MediaFile(f.RemoveExtension() + ".al_16bit_.wav");
                        await engine.ConvertAsync(inFile, outFile, options);

                        File.Delete(f);
                        File.Move(f.RemoveExtension() + ".al_16bit_.wav", f);
                    }
                }
                catch (Exception ex)
                {
                    Log("Exception in file conversion", ex);
                }
            });
        }

        // Dark engine games can't play MP3s, so they must be converted in all cases.
        internal async Task ConvertMP3sToWAVs() => await ConvertToWAVs("*.mp3");

        // From the FMSel manual:
        // "The game _can_ play OGG files but it can under some circumstance cause short hiccups, on less powerful
        // computers, performance heavy missions or with large OGG files. In such cases it might help to convert
        // them to WAV files during installation."
        internal async Task ConvertOGGsToWAVs() => await ConvertToWAVs("*.ogg");

        private async Task ConvertToWAVs(string pattern)
        {
            if (!GameIsDark(FM)) return;

            await Task.Run(async () =>
            {
                try
                {
                    var fmSndPath = GetFMSoundPathByGame();
                    if (!Directory.Exists(fmSndPath)) return;

                    try
                    {
                        _ = new DirectoryInfo(fmSndPath) { Attributes = FileAttributes.Normal };
                    }
                    catch (Exception ex)
                    {
                        Log("Unable to set directory attributes on " + fmSndPath, ex);
                    }

                    string[] files;
                    try
                    {
                        files = Directory.GetFiles(fmSndPath, pattern, SearchOption.AllDirectories);
                    }
                    catch (Exception ex)
                    {
                        Log("Exception during file enumeration of " + fmSndPath, ex);
                        return;
                    }

                    foreach (var f in files)
                    {
                        UnSetReadOnly(f);

                        try
                        {
                            var engine = new Engine(Paths.FFmpegExe);
                            await engine.ConvertAsync(new MediaFile(f), new MediaFile(f.RemoveExtension() + ".wav"));
                        }
                        catch (Exception ex)
                        {
                            Log("Exception in FFmpeg convert", ex);
                        }

                        try
                        {
                            File.Delete(f);
                        }
                        catch (Exception ex)
                        {
                            Log("Exception in deleting file " + f, ex);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log("Exception in file conversion", ex);
                }
            });
        }

        private string GetFMSoundPathByGame()
        {
            // Only T1/T2 can have audio converted for now, because it looks like SU's FMSel pointedly doesn't do
            // any conversion whatsoever, neither automatically nor even with a menu option. I'll assume Thief 3
            // doesn't need it and leave it at that.
            Debug.Assert(GameIsDark(FM), !FM.Archive.IsEmpty() ? FM.Archive : FM.InstalledDir + " is not T1/T2");

            return Path.Combine(InstalledFMsBasePath, FM.InstalledDir, "snd");
        }
    }
}
