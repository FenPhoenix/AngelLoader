// @MT_TASK: Test identicality for wav bits detection between previous and current versions before release

//#define SATA_SINGLE_THREAD

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
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

    private static CancellationTokenSource _conversionCts = new();
    private static void CancelToken() => _conversionCts.CancelIfNotDisposed();

    #region Public methods

    // @PERF_TODO: ffmpeg can do multiple files in one run. Switch to that, and see if ffprobe can do it too.

    // OpenAL doesn't play nice with anything over 16 bits, blasting out white noise when it tries to play such.
    // Converting all >16bit wavs to 16 bit fixes this.

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

            ThreadingData threadingData = GetLowestCommonThreadingData(GetAudioConversionRelevantPaths(validFMs));

            await Task.Run(() =>
            {
                for (int i = 0; i < validFMs.Count; i++)
                {
                    ValidAudioConvertibleFM fm = validFMs[i];

                    Core.View.SetProgressBoxState_Single(
                        message2: fm.GetId(),
                        percent: single ? null : GetPercentFromValue_Int(i + 1, validFMs.Count)
                    );

                    ConvertToWAVs(fm, convertType, threadingData, CancellationToken.None);

                    if (!single && _conversionCts.IsCancellationRequested) return;
                }
            });
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

        static List<IOPath> GetAudioConversionRelevantPaths(List<ValidAudioConvertibleFM> fms)
        {
            List<IOPath> ret = new(SupportedGameCount);

            bool[] fmInstalledDirsRequired = new bool[SupportedGameCount];
            for (int i = 0; i < fms.Count; i++)
            {
                fmInstalledDirsRequired[(int)fms[i].GameIndex] = true;
            }

            for (int i = 0; i < SupportedGameCount; i++)
            {
                if (fmInstalledDirsRequired[i])
                {
                    ret.Add(new IOPath(Config.GetFMInstallPath((GameIndex)i), IOPathType.Directory));
                }
            }

            return ret;
        }

        #endregion
    }

    /*
    @MT_TASK(Audio convert/threading):
    We pass install-relevant here, but we really want audio-convert-relevant.
    But we would need per-FM relevant paths, so we'd need to store a drive type value in the FMData class I guess.
    */
    internal static ConvertAudioError ConvertAsPartOfInstall(
        ValidAudioConvertibleFM fm,
        AudioConvert type,
        ThreadingData threadingData,
        CancellationToken ct)
    {
        return ConvertToWAVs(fm, type, threadingData, ct);
    }

    private static ConvertAudioError ConvertToWAVs(
        ValidAudioConvertibleFM fm,
        AudioConvert type,
        ThreadingData threadingData,
        CancellationToken ct)
    {
        if (type == AudioConvert.WAVToWAV16)
        {
            bool ffmpegNotFound = !File.Exists(Paths.FFmpegExe);
            bool ffProbeNotFound = !File.Exists(Paths.FFprobeExe);
            if (ffmpegNotFound || ffProbeNotFound)
            {
                string message = $"The following executables could not be found:{NL}{NL}" +
                                 (ffmpegNotFound ? Paths.FFmpegExe + $"{NL}" : "") +
                                 (ffProbeNotFound ? Paths.FFprobeExe + $"{NL}" : "") + $"{NL}" +
                                 ErrorText.Un + "convert audio files.";

                Log(message, stackTrace: true);
                return ConvertAudioError.FFmpegNotFound;
            }

            if (ct.IsCancellationRequested) return ConvertAudioError.None;

            try
            {
                string[] fmSndPaths = GetFMSoundPathsByGame(fm);
                foreach (string fmSndPath in fmSndPaths)
                {
                    if (!Directory.Exists(fmSndPath)) return ConvertAudioError.None;

                    if (ct.IsCancellationRequested) return ConvertAudioError.None;

                    Dir_UnSetReadOnly(fmSndPath);

                    if (ct.IsCancellationRequested) return ConvertAudioError.None;

                    string[] wavFiles = Directory.GetFiles(fmSndPath, "*.wav", SearchOption.AllDirectories);

                    if (ct.IsCancellationRequested) return ConvertAudioError.None;

                    int threadCount = GetThreadCount(wavFiles.Length, threadingData);

                    if (!TryGetParallelForData(threadCount, wavFiles, ct, out var pd))
                    {
                        return ConvertAudioError.None;
                    }

                    Parallel.For(0, threadCount, pd.PO, _ =>
                    {
                        /*
                        36 bytes encompasses the beginning of a wav file (RIFF/WAVE/fmt ) and up to the end of
                        the "wBitsPerSample" field in the case where there's no chunk in between WAVE and fmt.
                        It's also long enough that we can read the JUNK header out of the same buffer we initially
                        used, and then after skipping the JUNK chunk we can reuse the buffer to read the forward-
                        displaced fmt chunk in that case.
                        */
                        Span<byte> buffer = stackalloc byte[36];

                        while (pd.CQ.TryDequeue(out string f))
                        {
                            // Workaround https://fenphoenix.github.io/AngelLoader/file_ext_note.html
                            if (!f.EndsWithI(".wav")) continue;

                            File_UnSetReadOnly(f);

                            pd.PO.CancellationToken.ThrowIfCancellationRequested();

                            int bits = GetBitDepthFast(f, buffer);

                            pd.PO.CancellationToken.ThrowIfCancellationRequested();

                            // Header wasn't wav, so skip this one
                            if (bits == -1) continue;

                            if (bits == 0) bits = GetBitDepthSlow(f);

                            pd.PO.CancellationToken.ThrowIfCancellationRequested();

                            if (bits is >= 1 and <= 16) continue;

                            string tempFile = f.RemoveExtension() + ".al_16bit_.wav";
                            FFmpeg.NET.Engine.Convert(f, tempFile, FFmpeg.NET.ConvertType.AudioBitRateTo16Bit);
                            File.Delete(f);
                            File.Move(tempFile, f);

                            pd.PO.CancellationToken.ThrowIfCancellationRequested();
                        }
                    });
                }
            }
            catch (OperationCanceledException)
            {
                return ConvertAudioError.None;
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
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
                    if (!Directory.Exists(fmSndPath)) return ConvertAudioError.None;

                    if (ct.IsCancellationRequested) return ConvertAudioError.None;

                    Dir_UnSetReadOnly(fmSndPath);

                    if (ct.IsCancellationRequested) return ConvertAudioError.None;

                    string[] files;
                    try
                    {
                        files = Directory.GetFiles(fmSndPath, pattern, SearchOption.AllDirectories);
                    }
                    catch (Exception ex)
                    {
                        Log(ErrorText.ExGet + "files in " + fmSndPath, ex);
                        return ConvertAudioError.None;
                    }

                    if (ct.IsCancellationRequested) return ConvertAudioError.None;

                    int threadCount = GetThreadCount(files.Length, threadingData);

                    if (!TryGetParallelForData(threadCount, files, ct, out var pd))
                    {
                        return ConvertAudioError.None;
                    }

                    Parallel.For(0, threadCount, pd.PO, _ =>
                    {
                        while (pd.CQ.TryDequeue(out string f))
                        {
                            // Workaround https://fenphoenix.github.io/AngelLoader/file_ext_note.html
                            if (!f.EndsWithI(ext)) continue;

                            File_UnSetReadOnly(f);

                            pd.PO.CancellationToken.ThrowIfCancellationRequested();

                            try
                            {
                                FFmpeg.NET.Engine.Convert(f, f.RemoveExtension() + ".wav",
                                    FFmpeg.NET.ConvertType.FormatConvert);
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

                            pd.PO.CancellationToken.ThrowIfCancellationRequested();
                        }
                    });
                }
            }
            catch (OperationCanceledException)
            {
                return ConvertAudioError.None;
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                fm.LogInfo(ErrorText.Ex + "in file conversion (" + type + ")", ex);
            }
        }

        return ConvertAudioError.None;

        #region Local functions

        static int GetBitDepthFast(string file, Span<byte> buffer)
        {
            try
            {
                const uint RIFF = 0x46464952; // "RIFF"
                const uint WAVE = 0x45564157; // "WAVE"
                const uint JUNK = 0x4B4E554A; // "JUNK"
                const uint fmt_ = 0x20746D66; // "fmt "

                using AL_SafeFileHandle fileHandle = AL_SafeFileHandle.Open(
                    file,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.Read,
                    FileOptions.None);
                long fileLength = RandomAccess.GetLength(fileHandle);
                if (fileLength < buffer.Length) return -1;

                int bytesRead = RandomAccess.Read(fileHandle, buffer, 0);
                if (bytesRead < buffer.Length) return -1;

                if (Unsafe.ReadUnaligned<uint>(ref buffer[0]) != RIFF) return -1;

                if (Unsafe.ReadUnaligned<uint>(ref buffer[8]) != WAVE) return 0;

                uint afterWaveHeader = Unsafe.ReadUnaligned<uint>(ref buffer[12]);

                /*
                Spec of the "JUNK" chunk:
                https://www.daubnet.com/en/file-format-riff

                Name   | Size       | Description
                ------------------------------------------------------------
                ID     | 4 byte     | four ASCII character identifier 'JUNK'
                Size   | 4 byte     | size of Data
                Data   | Size bytes | nothing
                unused | 1 byte     | present if Size is odd 
                ------------------------------------------------------------

                Maybe it's possible for multiple JUNK chunks to exist in a row (not sure), and there's apparently
                a PAD chunk that can exist too, but whatever, if we see those, we'll just fall back for now...
                */
                if (afterWaveHeader == JUNK)
                {
                    uint junkSize = Unsafe.ReadUnaligned<uint>(ref buffer[16]);
                    if (junkSize % 2 != 0) junkSize++;
                    if (fileLength < 20 + junkSize + 4 + 20) return 0;
                    bytesRead = RandomAccess.Read(fileHandle, buffer, 20 + junkSize);
                    if (bytesRead < buffer.Length) return 0;
                    uint expectedFmtHeader = Unsafe.ReadUnaligned<uint>(ref buffer[0]);
                    if (expectedFmtHeader != fmt_) return 0;

                    ushort bits = Unsafe.ReadUnaligned<ushort>(ref buffer[22]);
                    return bits;
                }
                else if (afterWaveHeader == fmt_)
                {
                    ushort bits = Unsafe.ReadUnaligned<ushort>(ref buffer[34]);
                    return bits;
                }
                else
                {
                    return 0;
                }
            }
            catch
            {
                return 0;
            }
        }

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

        // Only Dark games can have audio converted for now, because it looks like SU's FMSel pointedly
        // doesn't do any conversion whatsoever, neither automatically nor even with a menu option. I'll
        // assume Thief 3 doesn't need it and leave it at that.
        static string[] GetFMSoundPathsByGame(ValidAudioConvertibleFM fm)
        {
            string instPath = Path.Combine(Config.GetFMInstallPath(fm.GameIndex), fm.InstalledDir);
            string sndPath = Path.Combine(instPath, "snd");
            return fm.GameIndex == GameIndex.SS2
                ? new[] { sndPath, Path.Combine(instPath, "snd2") }
                : new[] { sndPath };
        }

        /*
        @MT_TASK_NOTE(ConvertToWAVs aggressive threading perf on SATA):
        In the test set, SATA aggressive threading was slower (occasionally much slower) on some individual FMs,
        but the total time for the set was still slightly faster than with normal threading. Given that, and the
        fact that SATA aggressive threading is much faster when only a single FM is in the set, let's just allow
        aggressive threading for SATA.

        We could put the audio conversion for all FMs at the end of the entire install process, so that the parallel
        loop here doesn't fight for resources with the already-going parallel loop for the install.
        This would complicate things for the SATA case though.
        */
        static int GetThreadCount(int maxWorkItemsCount, ThreadingData threadingData) =>
#if SATA_SINGLE_THREAD
            threadingData.Mode == IOThreadingMode.Aggressive
                ? GetThreadCountForParallelOperation(maxWorkItemsCount, threadingData)
                : 1;
#else
            GetThreadCountForParallelOperation(maxWorkItemsCount, threadingData);
#endif

        #endregion
    }

    #endregion
}
