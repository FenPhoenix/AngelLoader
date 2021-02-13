﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using static Fen7z.Utils;

namespace Fen7z
{
    public static class Fen7z
    {
        public sealed class ProgressReport
        {
            public string EntryFileName = "";
            public int EntryNumber;
            public int TotalEntriesCount;
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

        public sealed class Result
        {
            public bool ErrorOccurred =>
                !Canceled &&
                (/*!ErrorText.IsWhiteSpace()
                ||*/ Exception != null
                || (ExitCode != SevenZipExitCode.NoError && ExitCode != SevenZipExitCode.UserStopped)
                || (ExitCodeInt != null && ExitCodeInt != 0 && ExitCodeInt != 255));

            public string ErrorText = "";
            public Exception? Exception;
            public SevenZipExitCode ExitCode = SevenZipExitCode.NoError;
            public int? ExitCodeInt;
            public bool Canceled;
        }

        public static Result Extract(
            string sevenZipWorkingPath,
            string sevenZipPathAndExe,
            string archivePath,
            string outputPath,
            int entriesCount,
            string listFile,
            List<string> fileNamesList,
            CancellationToken? cancellationToken = null,
            IProgress<ProgressReport>? progress = null)
        {
            bool selectiveFiles = !listFile.IsWhiteSpace() && fileNamesList.Count > 0;

            if (selectiveFiles)
            {
                try
                {
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
                p.StartInfo.Arguments = "x \"" + archivePath + "\" -o\"" + outputPath + "\" "
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
                            // We should be sending Ctrl+C to it, but since that's deep-level black magic, we
                            // just kill it. We're going to delete all its extracted files immediately afterward
                            // anyway, so file corruption isn't an issue.
                            proc.Kill();
                        }
                        catch
                        {
                            // Ignore, it's going to throw but work anyway (even on non-admin, tested)
                        }
                        return;
                    }

                    if (e.Data.IsEmpty() || report.Canceling || progress == null) return;

                    using var sr = new StringReader(e.Data);

                    string? line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        string lineT = line.Trim();

                        int pi = lineT.IndexOf('%');
                        if (pi > -1)
                        {
                            int di;
                            if (int.TryParse((di = lineT.IndexOf('-', pi + 1)) > -1
                                ? lineT.Substring(pi + 1, di)
                                : lineT.Substring(pi + 1), out int entriesDone))
                            {
                                int filesPercent = GetPercentFromValue(entriesDone, entriesCount).Clamp(0, 100);
                                report.PercentOfEntries = filesPercent;
                            }

                            if (int.TryParse(lineT.Substring(0, pi), out int bytesPercent))
                            {
                                report.PercentOfBytes = bytesPercent;
                            }

                            progress?.Report(report);
                        }
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
                return new Result()
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
                if (selectiveFiles)
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
                int? exitCode = p.ExitCode;
                SevenZipExitCode sevenZipExitCode;
                if (exitCode == (int?)SevenZipExitCode.NoError)
                {
                    sevenZipExitCode = SevenZipExitCode.NoError;
                }
                else if (exitCode == (int?)SevenZipExitCode.Warning)
                {
                    sevenZipExitCode = SevenZipExitCode.Warning;
                }
                else if (exitCode == (int?)SevenZipExitCode.FatalError)
                {
                    sevenZipExitCode = SevenZipExitCode.FatalError;
                }
                else if (exitCode == (int?)SevenZipExitCode.CommandLineError)
                {
                    sevenZipExitCode = SevenZipExitCode.CommandLineError;
                }
                else if (exitCode == (int?)SevenZipExitCode.NotEnoughMemory)
                {
                    sevenZipExitCode = SevenZipExitCode.NotEnoughMemory;
                }
                else if (exitCode == (int?)SevenZipExitCode.UserStopped)
                {
                    sevenZipExitCode = SevenZipExitCode.UserStopped;
                }
                else
                {
                    sevenZipExitCode = SevenZipExitCode.Unknown;
                }
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
}
