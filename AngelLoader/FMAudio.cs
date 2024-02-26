using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using AL_Common;
using AngelLoader.DataClasses;
using static AL_Common.Common;
using static AL_Common.Logger;
using static AngelLoader.GameSupport;
using static AngelLoader.Global;
using static AngelLoader.Misc;
using static AngelLoader.Utils;

namespace AngelLoader;

internal static class FMAudio
{
    // @BetterErrors(FMAudio):
    // Lots of exceptions possible here... we need to decide which to actually bother the user about...
    // Maybe do similar to scanner, write all errors to log and then tell user there were errors during the
    // conversion.

    private static readonly byte[] _riff = { (byte)'R', (byte)'I', (byte)'F', (byte)'F' };
    private static readonly byte[] _wave = { (byte)'W', (byte)'A', (byte)'V', (byte)'E' };
    private static readonly byte[] _fmt = { (byte)'f', (byte)'m', (byte)'t', (byte)' ' };

    private static CancellationTokenSource _conversionCts = new();
    private static void CancelToken() => _conversionCts.CancelIfNotDisposed();

    #region Public methods

    // @PERF_TODO: ffmpeg can do multiple files in one run. Switch to that, and see if ffprobe can do it too.

    // OpenAL doesn't play nice with anything over 16 bits, blasting out white noise when it tries to play
    // such. Converting all >16bit wavs to 16 bit fixes this.

    // From the FMSel manual:
    // "The game _can_ play OGG files but it can under some circumstance cause short hiccups, on less powerful
    // computers, performance heavy missions or with large OGG files. In such cases it might help to convert
    // them to WAV files during installation."

    internal static async Task ConvertSelected(AudioConvert convertType)
    {
        using var fmInstDirModScope = new FMInstalledDirModificationScope();

        List<FanMission> fms = Core.View.GetSelectedFMs_InOrder_List();
        if (fms.Count == 0) return;

        bool anyInapplicable = false;
        for (int i = 0; i < fms.Count; i++)
        {
            FanMission fm = fms[i];
            if (!GameIsDark(fm.Game) || !fm.Installed || fm.MarkedUnavailable)
            {
                anyInapplicable = true;
                fms.RemoveAt(i);
                i--;
            }
        }

        if (anyInapplicable)
        {
            (MBoxButton result, _) = Core.Dialogs.ShowMultiChoiceDialog(
                message: LText.AlertMessages.AudioConversion_SomeSelectedFilesDoNotSupportConversion,
                title: LText.AlertMessages.Alert,
                icon: MBoxIcon.None,
                yes: LText.Global.Continue,
                no: LText.Global.Cancel,
                defaultButton: MBoxButton.Yes
            );
            if (result == MBoxButton.No) return;
        }

        if (!ChecksPassed(fms)) return;

        try
        {
            bool single = fms.Count == 1;

            _conversionCts = _conversionCts.Recreate();

            Core.View.ShowProgressBox_Single(
                message1: LText.ProgressBox.ConvertingAudioFiles,
                progressType: single ? ProgressType.Indeterminate : ProgressType.Determinate,
                cancelMessage: single ? null : LText.Global.Stop,
                cancelAction: single ? null : CancelToken
            );

            BinaryBuffer buffer = new();
            byte[] fileStreamBuffer = new byte[FileStreamBufferSize];

            for (int i = 0; i < fms.Count; i++)
            {
                FanMission fm = fms[i];

                Core.View.SetProgressBoxState_Single(
                    message2: fm.GetId(),
                    percent: single ? null : GetPercentFromValue_Int(i + 1, fms.Count)
                );

                await ConvertToWAVs(fm, convertType, buffer, fileStreamBuffer);

                if (!single && _conversionCts.IsCancellationRequested) return;
            }
        }
        finally
        {
            Core.View.HideProgressBox();
            _conversionCts.Dispose();
        }

        return;

        #region Local functions

        static bool ChecksPassed(List<FanMission> fms)
        {
            bool[] gamesChecked = new bool[SupportedGameCount];

            for (int i = 0; i < fms.Count; i++)
            {
                FanMission fm = fms[i];

                AssertR(GameIsKnownAndSupported(fm.Game), nameof(fm) + "." + nameof(fm.Game) + " is not known or supported (not convertible to GameIndex).");

                GameIndex gameIndex = GameToGameIndex(fm.Game);
                int intGameIndex = (int)gameIndex;

                if (!gamesChecked[intGameIndex])
                {
                    string gameExe = Config.GetGameExe(gameIndex);
                    string gameName = GetLocalizedGameName(gameIndex);

                    if (GameIsRunning(gameExe))
                    {
                        Core.Dialogs.ShowAlert(
                            gameName + ":\r\n" +
                            LText.AlertMessages.AudioConversion_GameIsRunning,
                            LText.AlertMessages.Alert);

                        return false;
                    }

                    gamesChecked[intGameIndex] = true;
                }
            }

            return true;
        }

        #endregion
    }

    internal static Task ConvertAsPartOfInstall(FanMission fm, AudioConvert type, BinaryBuffer buffer, byte[] fileStreamBuffer, CancellationToken? ct = null)
    {
        return ConvertToWAVs(fm, type, buffer, fileStreamBuffer, ct);
    }

    private static Task ConvertToWAVs(FanMission fm, AudioConvert type, BinaryBuffer buffer, byte[] fileStreamBuffer, CancellationToken? ct = null)
    {
        if (!GameIsDark(fm.Game)) return VoidTask;

        return Task.Run(async () =>
        {
            if (type == AudioConvert.WAVToWAV16)
            {
                #region Local functions

                static int GetBitDepthFast(string file, BinaryBuffer buffer, byte[] fileStreamBuffer)
                {
                    // In case we read past the end of the file or can't open the file or whatever. We're trying
                    // to be fast, so don't check explicitly. If there's a more serious IO problem, we'll catch
                    // it in a minute.
                    try
                    {
                        using var fs = GetReadModeFileStreamWithCachedBuffer(file, fileStreamBuffer);

                        _ = fs.ReadAll(buffer.Buffer.Cleared(), 0, 4);
                        if (!buffer.Buffer.StartsWith(_riff)) return -1;

                        fs.Seek(4, SeekOrigin.Current);

                        _ = fs.ReadAll(buffer.Buffer.Cleared(), 0, 4);
                        if (!buffer.Buffer.StartsWith(_wave)) return 0;

                        _ = fs.ReadAll(buffer.Buffer.Cleared(), 0, 4);
                        if (!buffer.Buffer.StartsWith(_fmt)) return 0;

                        fs.Seek(18, SeekOrigin.Current);

                        ushort bits = BinaryRead.ReadUInt16(fs, buffer);
                        return bits;
                    }
                    catch
                    {
                        return 0;
                    }
                }

                // @PERF_TODO: I could maybe speed this up by having the process not be recreated all the time?
                // I suspect it may be just the fact that it's a separate program that's constantly being started
                // and stopped. If that's the case, MEH. :\
                static int GetBitDepthSlow(string file)
                {
                    int ret = 0;

                    using var p = new Process();
                    p.EnableRaisingEvents = true;
                    p.StartInfo.FileName = Paths.FFprobeExe;
                    p.StartInfo.RedirectStandardOutput = true;
                    p.StartInfo.Arguments = "-show_format -show_streams -hide_banner \"" + file + "\"";
                    p.StartInfo.CreateNoWindow = true;
                    p.StartInfo.UseShellExecute = false;

                    p.OutputDataReceived += (_, e) =>
                    {
                        if (e.Data.IsEmpty()) return;

                        string line = e.Data;
                        if (line.StartsWithFast("bits_per_sample=") &&
                            Int_TryParseInv(line.Substring(line.IndexOf('=') + 1), out int result))
                        {
                            ret = result;
                        }
                    };

                    p.Start();
                    p.BeginOutputReadLine();

                    p.WaitForExit();
                    return ret;
                }

                #endregion

                bool ffmpegNotFound = !File.Exists(Paths.FFmpegExe);
                bool ffProbeNotFound = !File.Exists(Paths.FFprobeExe);
                if (ffmpegNotFound || ffProbeNotFound)
                {
                    string message = "The following executables could not be found:\r\n\r\n" +
                                     (ffmpegNotFound ? Paths.FFmpegExe + "\r\n" : "") +
                                     (ffProbeNotFound ? Paths.FFprobeExe + "\r\n" : "") + "\r\n" +
                                     ErrorText.Un + "convert audio files.";

                    Log(message, stackTrace: true);
                    Core.Dialogs.ShowError(message);
                    return;
                }

                if (Canceled(ct)) return;

                try
                {
                    string[] fmSndPaths = GetFMSoundPathsByGame(fm);
                    foreach (string fmSndPath in fmSndPaths)
                    {
                        if (!Directory.Exists(fmSndPath)) return;

                        if (Canceled(ct)) return;

                        Dir_UnSetReadOnly(fmSndPath);

                        if (Canceled(ct)) return;

                        string[] wavFiles = Directory.GetFiles(fmSndPath, "*.wav", SearchOption.AllDirectories);

                        if (Canceled(ct)) return;

                        foreach (string f in wavFiles)
                        {
                            // Workaround https://fenphoenix.github.io/AngelLoader/file_ext_note.html
                            if (!f.EndsWithI(".wav")) continue;

                            File_UnSetReadOnly(f);

                            if (Canceled(ct)) return;

                            int bits = GetBitDepthFast(f, buffer, fileStreamBuffer);

                            if (Canceled(ct)) return;

                            // Header wasn't wav, so skip this one
                            if (bits == -1) continue;

                            if (bits == 0) bits = GetBitDepthSlow(f);

                            if (Canceled(ct)) return;

                            if (bits is >= 1 and <= 16) continue;

                            string tempFile = f.RemoveExtension() + ".al_16bit_.wav";
                            await FFmpeg.NET.Engine.ConvertAsync(f, tempFile, FFmpeg.NET.ConvertType.AudioBitRateTo16Bit);
                            File.Delete(f);
                            File.Move(tempFile, f);

                            if (Canceled(ct)) return;
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogFMInfo(fm, ErrorText.Ex + "in file conversion (" + type + ")", ex);
                }
            }
            else
            {
                try
                {
                    string pattern = type == AudioConvert.MP3ToWAV ? "*.mp3" : "*.ogg";
                    string ext = type == AudioConvert.MP3ToWAV ? ".mp3" : ".ogg";

                    string[] fmSndPaths = GetFMSoundPathsByGame(fm);
                    foreach (string fmSndPath in fmSndPaths)
                    {
                        if (!Directory.Exists(fmSndPath)) return;

                        if (Canceled(ct)) return;

                        Dir_UnSetReadOnly(fmSndPath);

                        if (Canceled(ct)) return;

                        string[] files;
                        try
                        {
                            files = Directory.GetFiles(fmSndPath, pattern, SearchOption.AllDirectories);
                        }
                        catch (Exception ex)
                        {
                            Log(ErrorText.ExGet + "files in " + fmSndPath, ex);
                            return;
                        }

                        if (Canceled(ct)) return;

                        foreach (string f in files)
                        {
                            // Workaround https://fenphoenix.github.io/AngelLoader/file_ext_note.html
                            if (!f.EndsWithI(ext)) continue;

                            File_UnSetReadOnly(f);

                            if (Canceled(ct)) return;

                            try
                            {
                                await FFmpeg.NET.Engine.ConvertAsync(f, f.RemoveExtension() + ".wav", FFmpeg.NET.ConvertType.FormatConvert);
                            }
                            catch (Exception ex)
                            {
                                LogFMInfo(fm, ErrorText.Ex + "in FFmpeg convert (" + type + ")", ex);
                            }

                            try
                            {
                                File.Delete(f);
                            }
                            catch (Exception ex)
                            {
                                Log(ErrorText.Ex + "deleting file " + f, ex);
                            }

                            if (Canceled(ct)) return;
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogFMInfo(fm, ErrorText.Ex + "in file conversion (" + type + ")", ex);
                }
            }
        });

        static bool Canceled(CancellationToken? ct) => ct != null && ((CancellationToken)ct).IsCancellationRequested;
    }

    #endregion

    #region Helpers

    private static string[] GetFMSoundPathsByGame(FanMission fm)
    {
        // Guard for the below unsafe Game conversion, and:
        // Only Dark games can have audio converted for now, because it looks like SU's FMSel pointedly
        // doesn't do any conversion whatsoever, neither automatically nor even with a menu option. I'll
        // assume Thief 3 doesn't need it and leave it at that.
        if (!fm.Game.ConvertsToDark(out GameIndex gameIndex)) return Array.Empty<string>();

        string instPath = Path.Combine(Config.GetFMInstallPath(gameIndex), fm.InstalledDir);
        string sndPath = Path.Combine(instPath, "snd");
        return fm.Game == Game.SS2
            ? new[] { sndPath, Path.Combine(instPath, "snd2") }
            : new[] { sndPath };
    }

    #endregion
}
