using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using AngelLoader.DataClasses;
using FFmpeg.NET;
using static AngelLoader.GameSupport;
using static AngelLoader.Logger;
using static AngelLoader.Misc;

namespace AngelLoader
{
    internal static class FMAudio
    {
        #region API methods

        // PERF_TODO: ffmpeg can do multiple files in one run. Switch to that, and see if ffprobe can do it too.

        // OpenAL doesn't play nice with anything over 16 bits, blasting out white noise when it tries to play
        // such. Converting all >16bit wavs to 16 bit fixes this.
        internal static async Task ConvertWAVsTo16Bit(FanMission fm, bool doChecksAndProgressBox)
        {
            if (doChecksAndProgressBox)
            {
                var (success, refreshFM) = ChecksPassed(fm);
                if (!success)
                {
                    if (refreshFM) await Core.View.RefreshSelectedFM(refreshReadme: false);
                    return;
                }
            }

            #region Local functions

            static int GetBitDepthFast(string file)
            {
                // In case we read past the end of the file or can't open the file or whatever. We're trying to
                // be fast, so don't check explicitly. If there's a more serious IO problem, we'll catch it in a
                // minute.
                try
                {
                    using var fs = new FileStream(file, FileMode.Open, FileAccess.Read);
                    using var br = new BinaryReader(fs, Encoding.ASCII);

                    string riff = Encoding.ASCII.GetString(br.ReadBytes(4));
                    if (riff != "RIFF") return -1;
                    br.ReadBytes(4);
                    string wave = Encoding.ASCII.GetString(br.ReadBytes(4));
                    if (wave != "WAVE") return 0;
                    string fmt = Encoding.ASCII.GetString(br.ReadBytes(4));
                    if (fmt != "fmt ") return 0;
                    br.ReadBytes(18);
                    ushort bits = br.ReadUInt16();
                    return bits;
                }
                catch (Exception)
                {
                    return 0;
                }
            }

            // PERF_TODO: I could maybe speed this up by having the process not be recreated all the time?
            // I suspect it may be just the fact that it's a separate program that's constantly being started and
            // stopped. If that's the case, MEH. :\
            static int GetBitDepthSlow(string file)
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

                        using var sr = new StringReader(e.Data);

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
                    };

                    p.Start();
                    p.BeginOutputReadLine();

                    p.WaitForExit();
                }
                return ret;
            }

            #endregion

            try
            {
                if (doChecksAndProgressBox) Core.View.ShowProgressBox(ProgressTasks.ConvertFiles);

                if (!File.Exists(Paths.FFprobeExe) || !File.Exists(Paths.FFmpegExe))
                {
                    Log("FFmpeg.exe or FFProbe.exe don't exist", stackTrace: true);
                    return;
                }

                await Task.Run(async () =>
                {
                    try
                    {
                        var fmSndPaths = GetFMSoundPathsByGame(fm);

                        var engine = new Engine(Paths.FFmpegExe);
                        var options = new ConversionOptions { AudioBitRate = 16 };

                        foreach (string fmSndPath in fmSndPaths)
                        {
                            if (!Directory.Exists(fmSndPath)) return;

                            Dir_UnSetReadOnly(fmSndPath);

                            var wavFiles = Directory.EnumerateFiles(fmSndPath, "*.wav", SearchOption.AllDirectories);
                            foreach (string f in wavFiles)
                            {
                                File_UnSetReadOnly(f);

                                int bits = GetBitDepthFast(f);

                                // Header wasn't wav, so skip this one
                                if (bits == -1) continue;

                                if (bits == 0) bits = GetBitDepthSlow(f);
                                if (bits >= 1 && bits <= 16) continue;

                                string tempFile = f.RemoveExtension() + ".al_16bit_.wav";

                                var inFile = new MediaFile(f);
                                var outFile = new MediaFile(tempFile);
                                await engine.ConvertAsync(inFile, outFile, options);

                                File.Delete(f);
                                File.Move(tempFile, f);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Log("Exception in file conversion", ex);
                    }
                });
            }
            finally
            {
                if (doChecksAndProgressBox) Core.View.HideProgressBox();
            }
        }

        // Dark engine games can't play MP3s, so they must be converted in all cases.
        // This one won't be called anywhere except during install, because it always runs during install so
        // there's no need to make it optional elsewhere. So we don't need to have a check bool or anything.
        internal static async Task ConvertMP3sToWAVs(FanMission fm) => await ConvertToWAVs(fm, "*.mp3");

        // From the FMSel manual:
        // "The game _can_ play OGG files but it can under some circumstance cause short hiccups, on less powerful
        // computers, performance heavy missions or with large OGG files. In such cases it might help to convert
        // them to WAV files during installation."
        internal static async Task ConvertOGGsToWAVs(FanMission fm, bool doChecksAndProgressBox)
        {
            if (doChecksAndProgressBox)
            {
                var (success, refreshFM) = ChecksPassed(fm);
                if (!success)
                {
                    if (refreshFM) await Core.View.RefreshSelectedFM(refreshReadme: false);
                    return;
                }
            }

            try
            {
                if (doChecksAndProgressBox) Core.View.ShowProgressBox(ProgressTasks.ConvertFiles);
                await ConvertToWAVs(fm, "*.ogg");
            }
            finally
            {
                if (doChecksAndProgressBox) Core.View.HideProgressBox();
            }
        }

        #endregion

        #region Helpers

        private static (bool Success, bool RefreshSelectedFM)
        ChecksPassed(FanMission fm)
        {
            if (!fm.Installed || !GameIsDark(fm.Game)) return (false, false);

            string gameExe = Config.GetGameExeUnsafe(fm.Game);
            string gameName = GetLocalizedGameName(fm.Game);
            if (GameIsRunning(gameExe))
            {
                Core.View.ShowAlert(
                    gameName + ":\r\n" + LText.AlertMessages.FileConversion_GameIsRunning,
                    LText.AlertMessages.Alert);

                return (false, false);
            }

            if (!FMIsReallyInstalled(fm))
            {
                bool yes = Core.View.AskToContinue(LText.AlertMessages.Misc_FMMarkedInstalledButNotInstalled,
                    LText.AlertMessages.Alert);
                if (yes) fm.Installed = false;

                return (false, true);
            }

            return (true, false);
        }

        #endregion

        #region Private methods

        private static async Task ConvertToWAVs(FanMission fm, string pattern)
        {
            if (!GameIsDark(fm.Game)) return;

            await Task.Run(async () =>
            {
                try
                {
                    var fmSndPaths = GetFMSoundPathsByGame(fm);
                    foreach (string fmSndPath in fmSndPaths)
                    {
                        if (!Directory.Exists(fmSndPath)) return;

                        try
                        {
                            Dir_UnSetReadOnly(fmSndPath);
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

                        foreach (string f in files)
                        {
                            File_UnSetReadOnly(f);

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
                }
                catch (Exception ex)
                {
                    Log("Exception in file conversion", ex);
                }
            });
        }

        private static List<string> GetFMSoundPathsByGame(FanMission fm)
        {
            // Guard for the below unsafe Game conversion, and:
            // Only Dark games can have audio converted for now, because it looks like SU's FMSel pointedly
            // doesn't do any conversion whatsoever, neither automatically nor even with a menu option. I'll
            // assume Thief 3 doesn't need it and leave it at that.
            if (!GameIsDark(fm.Game)) return new List<string>();

            string instPath = Path.Combine(Config.GetFMInstallPathUnsafe(fm.Game), fm.InstalledDir);
            string sndPath = Path.Combine(instPath, "snd");
            return fm.Game == Game.SS2
                ? new List<string> { sndPath, Path.Combine(instPath, "snd2") }
                : new List<string> { sndPath };
        }

        #endregion
    }
}
