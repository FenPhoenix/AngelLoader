using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using JetBrains.Annotations;

namespace AL_Common;

public static class Fen7z
{
    [PublicAPI]
    public sealed class ProgressReport
    {
        public int PercentOfBytes;
        public int PercentOfEntries;
        public bool Canceling;
    }

    public enum SevenZipExitCode
    {
        NoError = 0,
        /// <summary>
        /// Warning (Non fatal error(s)). For example, one or more files were locked by some other application,
        /// so they were not compressed.
        /// </summary>
        Warning = 1,
        FatalError = 2,
        CommandLineError = 7,
        /// <summary>
        /// Not enough memory for the operation.
        /// </summary>
        NotEnoughMemory = 8,
        /// <summary>
        /// User stopped the process.
        /// </summary>
        UserStopped = 255,
        Unknown = int.MaxValue,
    }

    [PublicAPI]
    public sealed class Result
    {
        public bool ErrorOccurred =>
            !Canceled &&
            (Exception != null
             || (ExitCode != SevenZipExitCode.NoError && ExitCode != SevenZipExitCode.UserStopped)
             || (ExitCodeInt != null && ExitCodeInt != 0 && ExitCodeInt != 255));

        public readonly string ErrorText;
        public readonly Exception? Exception;
        public readonly SevenZipExitCode ExitCode;
        public readonly int? ExitCodeInt;
        public readonly bool Canceled;

        public Result(Exception exception, string errorText)
        {
            Exception = exception;
            ErrorText = errorText;
            ExitCode = SevenZipExitCode.NoError;
        }

        public Result(Exception exception, string errorText, SevenZipExitCode exitCode, bool canceled)
        {
            Exception = exception;
            ErrorText = errorText;
            ExitCode = exitCode;
            Canceled = canceled;
        }

        public Result(Exception? exception, string errorText, SevenZipExitCode exitCode, int? exitCodeInt, bool canceled)
        {
            Exception = exception;
            ErrorText = errorText;
            ExitCode = exitCode;
            ExitCodeInt = exitCodeInt;
            Canceled = canceled;
        }

        public override string ToString() =>
            ErrorOccurred
                ? $"Error in 7z.exe extraction:{NL}"
                  + ErrorText + $"{NL}"
                  + (Exception?.ToString() ?? "") + $"{NL}"
                  + "ExitCode: " + ExitCode + $"{NL}"
                  + "ExitCodeInt: " + (ExitCodeInt?.ToString() ?? "")
                : $"No error.{NL}"
                  + "Canceled: " + Canceled + $"{NL}"
                  + "ExitCode: " + ExitCode + $"{NL}"
                  + "ExitCodeInt: " + (ExitCodeInt?.ToString() ?? "");
    }

    /// <summary>
    /// Extract a .7z file wholly or partially, using the official 7z.exe command-line utility
    /// for speed, but without the out-of-memory exceptions you get with SevenZipSharp when using that version.
    /// Hooray!
    /// </summary>
    /// <param name="sevenZipWorkingPath"></param>
    /// <param name="sevenZipPathAndExe"></param>
    /// <param name="archivePath"></param>
    /// <param name="outputPath"></param>
    /// <param name="entriesCount">Only used if <paramref name="progress"/> is provided (non-null).</param>
    /// <param name="listFile"></param>
    /// <param name="fileNamesList"></param>
    /// <param name="progress"></param>
    /// <returns></returns>
    public static Result Extract(
        string sevenZipWorkingPath,
        string sevenZipPathAndExe,
        string archivePath,
        string outputPath,
        int entriesCount = 0,
        string listFile = "",
        ListFast<string>? fileNamesList = null,
        IProgress<ProgressReport>? progress = null)
    {
        return Extract(
            sevenZipWorkingPath: sevenZipWorkingPath,
            sevenZipPathAndExe: sevenZipPathAndExe,
            archivePath: archivePath,
            outputPath: outputPath,
            cancellationToken: CancellationToken.None,
            entriesCount: entriesCount,
            listFile: listFile,
            fileNamesList: fileNamesList,
            progress: progress);
    }

    /// <summary>
    /// Extract a .7z file wholly or partially, using the official 7z.exe command-line utility
    /// for speed, but without the out-of-memory exceptions you get with SevenZipSharp when using that version.
    /// Hooray!
    /// </summary>
    /// <param name="sevenZipWorkingPath"></param>
    /// <param name="sevenZipPathAndExe"></param>
    /// <param name="archivePath"></param>
    /// <param name="outputPath"></param>
    /// <param name="cancellationToken"></param>
    /// <param name="entriesCount">Only used if <paramref name="progress"/> is provided (non-null).</param>
    /// <param name="listFile"></param>
    /// <param name="fileNamesList"></param>
    /// <param name="progress"></param>
    /// <returns></returns>
    public static Result Extract(
        string sevenZipWorkingPath,
        string sevenZipPathAndExe,
        string archivePath,
        string outputPath,
        CancellationToken cancellationToken,
        int entriesCount = 0,
        string listFile = "",
        ListFast<string>? fileNamesList = null,
        IProgress<ProgressReport>? progress = null)
    {
        bool selectiveFiles = !listFile.IsWhiteSpace() && fileNamesList?.Count > 0;

        if (selectiveFiles && fileNamesList != null)
        {
            try
            {
                // @MEM(Fen7z): Allow passing a buffer that gets set on the write stream
                // Just like the buffer-taking reflection-based read stream we have.
                // Scan, byte[4096]: 49 / 201,292
                File.WriteAllLines(listFile, fileNamesList);
            }
            catch (Exception ex)
            {
                return new Result
                (
                    exception: ex,
                    errorText: "Exception trying to write the 7z.exe list file: " + listFile
                );
            }
        }

        bool canceled = false;

        string errorText = "";

        ProgressReport report = new();

        Process p = new() { EnableRaisingEvents = true };
        try
        {
            p.StartInfo.FileName = sevenZipPathAndExe;
            p.StartInfo.WorkingDirectory = sevenZipWorkingPath;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardError = true;
            // x     = Extract with full paths
            // -aoa  = Overwrite all existing files without prompt
            // -y    = Say yes to all prompts automatically
            // -bsp1 = Redirect progress information to stdout stream
            // -bb1  = Show names of processed files in log (needed for smooth reporting of entries done count)
            p.StartInfo.Arguments =
                "x \"" + archivePath + "\" -o\"" + outputPath + "\" "
                + (selectiveFiles ? "@\"" + listFile + "\" " : "")
                + "-aoa -y -bsp1 -bb1";
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.UseShellExecute = false;

            p.OutputDataReceived += (sender, e) =>
            {
                Process proc = (Process)sender;
                if (!canceled && cancellationToken.IsCancellationRequested)
                {
                    canceled = true;

                    report.Canceling = true;
                    progress?.Report(report);
                    try
                    {
                        proc.CancelErrorRead();
                        proc.CancelOutputRead();
                        /*
                        We should be sending Ctrl+C to it, but since that's apparently deep-level black
                        magic on Windows, we just kill it. We expect the caller to understand that the
                        extracted files will be in an indeterminate state, and to delete them or do whatever
                        it deems fit.
                        TODO: If we can find a reliable way to send Ctrl+C to a process, we should switch to that.
                        */
                        proc.Kill();
                    }
                    catch
                    {
                        // Ignore, it's going to throw but work anyway (even on non-admin, tested)
                    }
                    return;
                }

                if (e.Data.IsEmpty() || report.Canceling || progress == null) return;
                try
                {
                    ReadOnlySpan<char> lineT = e.Data.AsSpan().Trim();

                    int pi = lineT.IndexOf('%');
                    if (pi > -1)
                    {
                        if (entriesCount > 0)
                        {
                            ReadOnlySpan<char> lineTPastPercent = lineT[(pi + 1)..];
                            int di;
                            if (Int_TryParseInv((di = lineTPastPercent.IndexOf('-')) > -1
                                    ? lineTPastPercent[..di]
                                    : lineTPastPercent, out int entriesDone))
                            {
                                report.PercentOfEntries = GetPercentFromValue_Int(entriesDone, entriesCount).Clamp(0, 100);
                            }
                        }

                        if (Int_TryParseInv(lineT[..pi], out int bytesPercent))
                        {
                            report.PercentOfBytes = bytesPercent;
                        }

                        progress.Report(report);
                    }
                }
                catch
                {
                    // ignore, it just means we won't report progress... meh
                }
            };

            p.ErrorDataReceived += (_, e) =>
            {
                if (!e.Data.IsWhiteSpace()) errorText += $"{NL}---" + e.Data;
            };

            p.Start();
            p.BeginOutputReadLine();
            p.BeginErrorReadLine();

            /*
            If we only check for cancellation in the output callback, we can only cancel on callback granularity.
            This is fine for a single FM (alone or in sequence), but for multithreading, the small delays add up
            and come at different offsets, thus creating the possibility of an objectionably long delay before
            all tasks get canceled.

            We can attempt to solve this by checking in a loop, however:
            
            - We still need to call WaitForExit() after checking for cancellation, to prevent race conditions with
            writing/clearing the temp dirs. And that brings back the long delay potential anyway, so it doesn't
            really help us.

            We could easily put a "Canceling..." message on the progress box to at least let the user know. This
            isn't ideal, but it's something.
            
            -Old notes (these would apply if we didn't wait for exit):-
            HasExited is heavy and calling it every 50ms adds time to the scan, about 269ms over the 43 7z FM
            set. That's about 6.3ms per FM. We could reduce that by raising the sleep interval, but then we would
            also increase the cancel delay. 6.3ms per FM is not that bad, though, given that 7z scans often take
            a second or more.

            However, what we could do is have a parameter saying whether to use the high or low frequency cancel
            check, and use low/efficient for single-thread or single FM, and high/inefficient for multithread.
            -end old notes-
            */
            p.WaitForExit();

            (SevenZipExitCode exitCode, int? exitCodeInt, Exception? exception) = GetExitCode(p);

            return new Result(exception, errorText, exitCode, exitCodeInt, canceled);
        }
        catch (Exception ex)
        {
            return new Result(ex, errorText, SevenZipExitCode.Unknown, canceled);
        }
        finally
        {
            try
            {
                if (!p.HasExited)
                {
                    p.Kill();
                }
            }
            catch
            {
                // ignore
            }
            finally
            {
                try
                {
                    if (!p.HasExited)
                    {
                        p.WaitForExit();
                    }
                }
                catch
                {
                    // ignore...
                }

                p.Dispose();
            }
            if (selectiveFiles && !listFile.IsEmpty())
            {
                try
                {
                    File.Delete(listFile);
                }
                catch
                {
                    // ignore
                }
            }
        }
    }

    private static (SevenZipExitCode ExitCode, int? ExitCodeInt, Exception? ex)
    GetExitCode(Process p)
    {
        try
        {
            int exitCode = p.ExitCode;
            SevenZipExitCode sevenZipExitCode = exitCode switch
            {
                (int)SevenZipExitCode.NoError => SevenZipExitCode.NoError,
                (int)SevenZipExitCode.Warning => SevenZipExitCode.Warning,
                (int)SevenZipExitCode.FatalError => SevenZipExitCode.FatalError,
                (int)SevenZipExitCode.CommandLineError => SevenZipExitCode.CommandLineError,
                (int)SevenZipExitCode.NotEnoughMemory => SevenZipExitCode.NotEnoughMemory,
                (int)SevenZipExitCode.UserStopped => SevenZipExitCode.UserStopped,
                _ => SevenZipExitCode.Unknown,
            };
            return (sevenZipExitCode, exitCode, null);
        }
        catch (InvalidOperationException ex)
        {
            return (SevenZipExitCode.Unknown, null, ex);
        }
        catch (NotSupportedException ex)
        {
            return (SevenZipExitCode.Unknown, null, ex);
        }
    }
}
