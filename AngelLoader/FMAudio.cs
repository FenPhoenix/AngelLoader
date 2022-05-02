using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using AL_Common;
using AngelLoader.DataClasses;
using AngelLoader.Forms;
using static AngelLoader.GameSupport;
using static AngelLoader.Logger;
using static AngelLoader.Misc;

namespace AngelLoader
{
    internal static class FMAudio
    {
        // @BetterErrors(FMAudio):
        // Lots of exceptions possible here... we need to decide which to actually bother the user about...

        private static readonly byte[] _riff = { (byte)'R', (byte)'I', (byte)'F', (byte)'F' };
        private static readonly byte[] _wave = { (byte)'W', (byte)'A', (byte)'V', (byte)'E' };
        private static readonly byte[] _fmt = { (byte)'f', (byte)'m', (byte)'t', (byte)' ' };

        private static CancellationTokenSource _conversionCTS = new();
        private static void CancelToken() => _conversionCTS.CancelIfNotDisposed();

        #region Public methods

        // PERF_TODO: ffmpeg can do multiple files in one run. Switch to that, and see if ffprobe can do it too.

        // OpenAL doesn't play nice with anything over 16 bits, blasting out white noise when it tries to play
        // such. Converting all >16bit wavs to 16 bit fixes this.

        // From the FMSel manual:
        // "The game _can_ play OGG files but it can under some circumstance cause short hiccups, on less powerful
        // computers, performance heavy missions or with large OGG files. In such cases it might help to convert
        // them to WAV files during installation."

        internal static async Task ConvertSelected(AudioConvert convertType)
        {
            FanMission[] fms = Core.View.GetSelectedFMs_InOrder();
            if (fms.Length == 0) return;

            foreach (FanMission fm in fms)
            {
                if (!ChecksPassed(fm)) return;
            }

            try
            {
                _conversionCTS = _conversionCTS.Recreate();

                Core.View.ShowProgressBox_Single(
                    message1: LText.ProgressBox.ConvertingFiles,
                    progressType: ProgressType.Indeterminate,
                    cancelAction: CancelToken
                );

                foreach (FanMission fm in fms)
                {
                    // @MULTISEL(Convert manual): We can add progress percent for this
                    Core.View.SetProgressBoxState_Single(message2: GetFMId(fm));

                    await ConvertToWAVs(fm, convertType, false);

                    // @MULTISEL(Audio convert manual): Notes on cancel/stop:
                    // We should make it so we can stop in the middle of a conversion, not just after each one.
                    // We'll need to rewrite the loop so instead of a bunch of single convert-and-copy operations
                    // in a row, we should convert all at once into a temp folder structure, during which we allow
                    // stopping, and then only at the very end copy them all into the FM's folder (during which we
                    // don't allow stopping, disable the button I guess.)
                    if (_conversionCTS.IsCancellationRequested)
                    {
                        // @MULTISEL(Audio convert manual, "stop" dialog message) - make this final or get rid of it or whatever
                        (bool cancel, _) = Dialogs.AskToContinueYesNoCustomStrings(
                            "Really stop converting audio for the selected FM(s)?\r\n" +
                            "Whatever has been done to this point won't (can't) be undone, but no more FMs' audio files will be converted.",
                            LText.AlertMessages.Alert,
                            MessageBoxIcon.None,
                            false,
                            "Continue",
                            "Stop"
                        );
                        if (cancel)
                        {
                            return;
                        }
                        else
                        {
                            _conversionCTS = _conversionCTS.Recreate();
                        }
                    }
                }
            }
            finally
            {
                Core.View.HideProgressBox();
            }
        }

        // @MULTISEL(Audio conversion):
        // Since we can now convert audio in bulk, we REALLY need a Cancel button. Otherwise the user could be
        // staring at the indeterminate convert progress for centuries if they select enough FMs, with no way
        // to stop the operation. We can't actually "cancel" (ie. roll back) the operation because we'll be
        // replacing files... so "stop" operation is the best we can do.
        internal static async Task ConvertToWAVs(FanMission fm, AudioConvert type, bool doChecksAndProgressBox)
        {
            // @MULTISEL(Audio convert): This bool is not used anymore, because we do this check in the manual method
            if (doChecksAndProgressBox)
            {
                if (!ChecksPassed(fm)) return;
            }

            if (!GameIsDark(fm.Game)) return;

            try
            {
                if (doChecksAndProgressBox)
                {
                    Core.View.ShowProgressBox_Single(
                        message1: LText.ProgressBox.ConvertingFiles,
                        progressType: ProgressType.Indeterminate);
                }

                await Task.Run(async () =>
                {
                    if (type == AudioConvert.WAVToWAV16)
                    {
                        #region Local functions

                        static int GetBitDepthFast(string file)
                        {
                            // In case we read past the end of the file or can't open the file or whatever. We're trying to
                            // be fast, so don't check explicitly. If there's a more serious IO problem, we'll catch it in a
                            // minute.
                            try
                            {
                                using var fs = File.OpenRead(file);
                                using var br = new BinaryReader(fs, Encoding.ASCII);

                                byte[] riff = br.ReadBytes(4);
                                if (!riff.SequenceEqual(_riff)) return -1;
                                br.ReadBytes(4);
                                byte[] wave = br.ReadBytes(4);
                                if (!wave.SequenceEqual(_wave)) return 0;
                                byte[] fmt = br.ReadBytes(4);
                                if (!fmt.SequenceEqual(_fmt)) return 0;
                                br.ReadBytes(18);
                                ushort bits = br.ReadUInt16();
                                return bits;
                            }
                            catch
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

                                p.OutputDataReceived += (_, e) =>
                                {
                                    if (e.Data.IsEmpty()) return;

                                    using var sr = new StringReader(e.Data);

                                    string? line;
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

                        if (!File.Exists(Paths.FFprobeExe) || !File.Exists(Paths.FFmpegExe))
                        {
                            Log("FFmpeg.exe or FFProbe.exe don't exist", stackTrace: true);
                            return;
                        }

                        try
                        {
                            var fmSndPaths = GetFMSoundPathsByGame(fm);

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
                                    if (bits is >= 1 and <= 16) continue;

                                    string tempFile = f.RemoveExtension() + ".al_16bit_.wav";

                                    await FFmpeg.NET.Engine.ConvertAsync(f, tempFile, FFmpeg.NET.ConvertType.AudioBitRateTo16Bit);

                                    File.Delete(f);
                                    File.Move(tempFile, f);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Log("Exception in file conversion", ex);
                        }
                    }
                    else
                    {
                        try
                        {
                            string pattern = type == AudioConvert.MP3ToWAV ? "*.mp3" : "*.ogg";

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
                                        await FFmpeg.NET.Engine.ConvertAsync(f, f.RemoveExtension() + ".wav", FFmpeg.NET.ConvertType.FormatConvert);
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
                    }
                });
            }
            finally
            {
                if (doChecksAndProgressBox) Core.View.HideProgressBox();
            }
        }

        #endregion

        #region Helpers

        private static bool
        ChecksPassed(FanMission fm)
        {
            if (!fm.Installed || !GameIsDark(fm.Game)) return false;

            GameIndex gameIndex = GameToGameIndex(fm.Game);

            string gameExe = Config.GetGameExe(gameIndex);
            string gameName = GetLocalizedGameName(gameIndex);
            if (GameIsRunning(gameExe))
            {
                Dialogs.ShowAlert(
                    gameName + ":\r\n" + LText.AlertMessages.FileConversion_GameIsRunning,
                    LText.AlertMessages.Alert);

                return false;
            }

            if (!FMIsReallyInstalled(fm))
            {
                bool yes = Dialogs.AskToContinue(
                    LText.AlertMessages.Misc_FMMarkedInstalledButNotInstalled,
                    LText.AlertMessages.Alert);
                if (yes)
                {
                    fm.Installed = false;
                    Core.View.RefreshFM(fm);
                }

                return false;
            }

            return true;
        }

        private static string[] GetFMSoundPathsByGame(FanMission fm)
        {
            // Guard for the below unsafe Game conversion, and:
            // Only Dark games can have audio converted for now, because it looks like SU's FMSel pointedly
            // doesn't do any conversion whatsoever, neither automatically nor even with a menu option. I'll
            // assume Thief 3 doesn't need it and leave it at that.
            if (!GameIsDark(fm.Game)) return Array.Empty<string>();

            string instPath = Path.Combine(Config.GetFMInstallPathUnsafe(fm.Game), fm.InstalledDir);
            string sndPath = Path.Combine(instPath, "snd");
            return fm.Game == Game.SS2
                ? new[] { sndPath, Path.Combine(instPath, "snd2") }
                : new[] { sndPath };
        }

        #endregion
    }
}
