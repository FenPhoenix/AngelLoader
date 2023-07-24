using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using JetBrains.Annotations;
using static AL_Common.Common;

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
        Unknown = int.MaxValue
    }

    [PublicAPI]
    public sealed class Result
    {
        public bool ErrorOccurred =>
            !Canceled &&
            (Exception != null
             || (ExitCode != SevenZipExitCode.NoError && ExitCode != SevenZipExitCode.UserStopped)
             || (ExitCodeInt != null && ExitCodeInt != 0 && ExitCodeInt != 255));

        public string ErrorText = "";
        public Exception? Exception;
        public SevenZipExitCode ExitCode = SevenZipExitCode.NoError;
        public int? ExitCodeInt;
        public bool Canceled;

        public override string ToString() =>
            ErrorOccurred
                ? "Error in 7z.exe extraction:\r\n"
                  + ErrorText + "\r\n"
                  + (Exception?.ToString() ?? "") + "\r\n"
                  + "ExitCode: " + ExitCode + "\r\n"
                  + "ExitCodeInt: " + (ExitCodeInt?.ToString() ?? "")
                : "No error.\r\n"
                  + "Canceled: " + Canceled + "\r\n"
                  + "ExitCode: " + ExitCode + "\r\n"
                  + "ExitCodeInt: " + (ExitCodeInt?.ToString() ?? "");
    }

    /// <summary>
    /// Extract a .7z file wholly or partially, using the official 7z.exe command-line utility, version 19.00
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
    /// <param name="cancellationToken"></param>
    /// <param name="progress"></param>
    /// <returns></returns>
    public static Result Extract(
        string sevenZipWorkingPath,
        string sevenZipPathAndExe,
        string archivePath,
        string outputPath,
        int entriesCount = 0,
        string listFile = "",
        List<string>? fileNamesList = null,
        CancellationToken? cancellationToken = null,
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
                {
                    Exception = ex,
                    ErrorText = "Exception trying to write the 7z.exe list file: " + listFile
                };
            }
        }

        bool canceled = false;

        string errorText = "";

        var report = new ProgressReport();

        var p = new Process { EnableRaisingEvents = true };
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
                var proc = (Process)sender;
                if (!canceled && cancellationToken != null && ((CancellationToken)cancellationToken).IsCancellationRequested)
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
                    string lineT = e.Data.Trim();

                    int pi = lineT.IndexOf('%');
                    if (pi > -1)
                    {
                        int di;
                        if (entriesCount > 0 &&
                            int.TryParse((di = lineT.IndexOf('-', pi + 1)) > -1
                                ? lineT.Substring(pi + 1, di)
                                : lineT.Substring(pi + 1), out int entriesDone))
                        {
                            report.PercentOfEntries = GetPercentFromValue_Int(entriesDone, entriesCount).Clamp(0, 100);
                        }

                        if (int.TryParse(lineT.Substring(0, pi), out int bytesPercent))
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
                if (!e.Data.IsWhiteSpace()) errorText += "\r\n---" + e.Data;
            };

            p.Start();
            p.BeginOutputReadLine();
            p.BeginErrorReadLine();

            p.WaitForExit();

            var result = new Result();

            (result.ExitCode, result.ExitCodeInt, result.Exception) = GetExitCode(p);
            result.Canceled = canceled || result.ExitCode == SevenZipExitCode.UserStopped;
            result.ErrorText = errorText;

            return result;
        }
        catch (Exception ex)
        {
            return new Result
            {
                Exception = ex,
                Canceled = canceled,
                ErrorText = errorText,
                ExitCode = SevenZipExitCode.Unknown
            };
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
                _ => SevenZipExitCode.Unknown
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
