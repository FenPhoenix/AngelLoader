﻿using System;
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

    private static readonly byte[] _riff = "RIFF"u8.ToArray();
    private static readonly byte[] _wave = "WAVE"u8.ToArray();
    private static readonly byte[] _fmt = "fmt "u8.ToArray();

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
        using var dsw = new DisableScreenshotWatchers();

        List<ValidAudioConvertibleFM> validFMs;
        {
            List<FanMission> rawFMs = Core.View.GetSelectedFMs_InOrder_List();
            validFMs = ValidAudioConvertibleFM.CreateListFrom(rawFMs);
            if (validFMs.Count == 0) return;

            if (validFMs.Count != rawFMs.Count)
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
        }

        if (!ChecksPassed(validFMs)) return;

        try
        {
            bool single = validFMs.Count == 1;

            _conversionCts = _conversionCts.Recreate();

            Core.View.ShowProgressBox_Single(
                message1: LText.ProgressBox.ConvertingAudioFiles,
                progressType: single ? ProgressType.Indeterminate : ProgressType.Determinate,
                cancelMessage: single ? null : LText.Global.Stop,
                cancelAction: single ? null : CancelToken
            );

            BinaryBuffer buffer = new();
            byte[] fileStreamBuffer = new byte[FileStreamBufferSize];

            for (int i = 0; i < validFMs.Count; i++)
            {
                ValidAudioConvertibleFM fm = validFMs[i];

                Core.View.SetProgressBoxState_Single(
                    message2: fm.GetId(),
                    percent: single ? null : GetPercentFromValue_Int(i + 1, validFMs.Count)
                );

                await ConvertToWAVs(fm, convertType, buffer, fileStreamBuffer, CancellationToken.None);

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

        static bool ChecksPassed(List<ValidAudioConvertibleFM> fms)
        {
            bool[] gamesChecked = new bool[SupportedGameCount];

            for (int i = 0; i < fms.Count; i++)
            {
                ValidAudioConvertibleFM fm = fms[i];

                int intGameIndex = (int)fm.GameIndex;

                if (!gamesChecked[intGameIndex])
                {
                    string gameExe = Config.GetGameExe(fm.GameIndex);
                    string gameName = GetLocalizedGameName(fm.GameIndex);

                    if (GameIsRunning(gameExe))
                    {
                        Core.Dialogs.ShowAlert(
                            gameName + $":{NL}" +
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

    internal static Task ConvertAsPartOfInstall(ValidAudioConvertibleFM fm, AudioConvert type, BinaryBuffer buffer, byte[] fileStreamBuffer, CancellationToken ct)
    {
        return ConvertToWAVs(fm, type, buffer, fileStreamBuffer, ct);
    }

    private static Task ConvertToWAVs(ValidAudioConvertibleFM fm, AudioConvert type, BinaryBuffer buffer, byte[] fileStreamBuffer, CancellationToken ct)
    {
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
                        if (line.TryGetValueO("bits_per_sample=", out string value) &&
                            Int_TryParseInv(value, out int result))
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
                    string message = $"The following executables could not be found:{NL}{NL}" +
                                     (ffmpegNotFound ? Paths.FFmpegExe + $"{NL}" : "") +
                                     (ffProbeNotFound ? Paths.FFprobeExe + $"{NL}" : "") + $"{NL}" +
                                     ErrorText.Un + "convert audio files.";

                    Log(message, stackTrace: true);
                    Core.Dialogs.ShowError(message);
                    return;
                }

                if (ct.IsCancellationRequested) return;

                try
                {
                    string[] fmSndPaths = GetFMSoundPathsByGame(fm);
                    foreach (string fmSndPath in fmSndPaths)
                    {
                        if (!Directory.Exists(fmSndPath)) return;

                        if (ct.IsCancellationRequested) return;

                        Dir_UnSetReadOnly(fmSndPath);

                        if (ct.IsCancellationRequested) return;

                        string[] wavFiles = Directory.GetFiles(fmSndPath, "*.wav", SearchOption.AllDirectories);

                        if (ct.IsCancellationRequested) return;

                        foreach (string f in wavFiles)
                        {
                            // Workaround https://fenphoenix.github.io/AngelLoader/file_ext_note.html
                            if (!f.EndsWithI(".wav")) continue;

                            File_UnSetReadOnly(f);

                            if (ct.IsCancellationRequested) return;

                            int bits = GetBitDepthFast(f, buffer, fileStreamBuffer);

                            if (ct.IsCancellationRequested) return;

                            // Header wasn't wav, so skip this one
                            if (bits == -1) continue;

                            if (bits == 0) bits = GetBitDepthSlow(f);

                            if (ct.IsCancellationRequested) return;

                            if (bits is >= 1 and <= 16) continue;

                            string tempFile = f.RemoveExtension() + ".al_16bit_.wav";
                            await FFmpeg.NET.Engine.ConvertAsync(f, tempFile, FFmpeg.NET.ConvertType.AudioBitRateTo16Bit);
                            File.Delete(f);
                            File.Move(tempFile, f);

                            if (ct.IsCancellationRequested) return;
                        }
                    }
                }
                catch (Exception ex)
                {
                    fm.LogInfo(ErrorText.Ex + "in file conversion (" + type + ")", ex);
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

                        if (ct.IsCancellationRequested) return;

                        Dir_UnSetReadOnly(fmSndPath);

                        if (ct.IsCancellationRequested) return;

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

                        if (ct.IsCancellationRequested) return;

                        foreach (string f in files)
                        {
                            // Workaround https://fenphoenix.github.io/AngelLoader/file_ext_note.html
                            if (!f.EndsWithI(ext)) continue;

                            File_UnSetReadOnly(f);

                            if (ct.IsCancellationRequested) return;

                            try
                            {
                                await FFmpeg.NET.Engine.ConvertAsync(f, f.RemoveExtension() + ".wav", FFmpeg.NET.ConvertType.FormatConvert);
                            }
                            catch (Exception ex)
                            {
                                fm.LogInfo(ErrorText.Ex + "in FFmpeg convert (" + type + ")", ex);
                            }

                            try
                            {
                                File.Delete(f);
                            }
                            catch (Exception ex)
                            {
                                Log(ErrorText.Ex + "deleting file " + f, ex);
                            }

                            if (ct.IsCancellationRequested) return;
                        }
                    }
                }
                catch (Exception ex)
                {
                    fm.LogInfo(ErrorText.Ex + "in file conversion (" + type + ")", ex);
                }
            }
        });
    }

    #endregion

    #region Helpers

    // Only Dark games can have audio converted for now, because it looks like SU's FMSel pointedly
    // doesn't do any conversion whatsoever, neither automatically nor even with a menu option. I'll
    // assume Thief 3 doesn't need it and leave it at that.
    private static string[] GetFMSoundPathsByGame(ValidAudioConvertibleFM fm)
    {
        string instPath = Path.Combine(Config.GetFMInstallPath(fm.GameIndex), fm.InstalledDir);
        string sndPath = Path.Combine(instPath, "snd");
        return fm.GameIndex == GameIndex.SS2
            ? new[] { sndPath, Path.Combine(instPath, "snd2") }
            : new[] { sndPath };
    }

    #endregion
}
