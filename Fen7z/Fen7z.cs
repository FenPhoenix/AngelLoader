using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using static Fen7z.Utils;

namespace Fen7z
{
    public sealed class Fen7z
    {
        public sealed class ProgressReport
        {
            public string EntryFileName = "";
            public int EntryNumber;
            public int EntriesCount;
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
            public string ErrorText = "";
            public Exception? Exception = null;
            public SevenZipExitCode ExitCode = SevenZipExitCode.NoError;
            public int? ExitCodeInt;
            public bool Canceled;
        }

        public async Task<Result> Extract(
            string sevenZipWorkingPath,
            string sevenZipPathAndExe,
            string archivePath,
            string outputPath,
            int entriesCount,
            string listFile,
            List<string> fileNamesList,
            CancellationToken cancellationToken,
            IProgress<ProgressReport> progress)
        {
            return await Task.Run(() =>
            {
                bool selectiveFiles = !listFile.IsEmpty() && fileNamesList.Count > 0;

                if (selectiveFiles)
                {
                    File.WriteAllLines(listFile, fileNamesList);
                }

                bool canceled = false;

                string error = "";

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
                                            + "@\"" + listFile + "\" "
                                            + "-aoa -y -bsp1 -bb1";
                    p.StartInfo.CreateNoWindow = true;
                    p.StartInfo.UseShellExecute = false;

                    p.OutputDataReceived += (sender, e) =>
                    {
                        var proc = (Process)sender;
                        if (!canceled && cancellationToken.IsCancellationRequested)
                        {
                            canceled = true;

                            report.Canceling = true;
                            progress.Report(report);
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

                        if (e.Data.IsEmpty()) return;

                        using var sr = new StringReader(e.Data);

                        string? line;
                        while ((line = sr.ReadLine()) != null)
                        {
                            string lineT = line.Trim();

                            #region Get percent of entries extracted

                            int pi = lineT.IndexOf('%');
                            if (pi > -1)
                            {
                                int di;
                                if (int.TryParse((di = lineT.IndexOf('-', pi + 1)) > -1
                                    ? lineT.Substring(pi + 1, di)
                                    : lineT.Substring(pi + 1), out int entriesDone))
                                {
                                    int filesPercent = GetPercentFromValue(entriesDone, entriesCount)
                                        .Clamp(0, 100);
                                    report.PercentOfEntries = filesPercent;
                                }
                                if (int.TryParse(lineT.Substring(0, pi), out int bytesPercent))
                                {
                                    report.PercentOfBytes = bytesPercent;
                                }
                                progress.Report(report);
                            }

                            #endregion
                        }
                    };

                    p.ErrorDataReceived += (_, e) =>
                    {
                        if (!e.Data.IsWhiteSpace()) error += "\r\n---" + e.Data;
                    };

                    p.Start();
                    p.BeginOutputReadLine();
                    p.BeginErrorReadLine();

                    p.WaitForExit();

                    var result = new Result();

                    (result.ExitCode, result.ExitCodeInt, result.Exception) = GetExitCode(p);
                    result.Canceled = canceled;
                    result.ErrorText = error;

                    return result;
                }
                catch (Exception ex)
                {
                    return new Result()
                    {
                        Exception = ex,
                        Canceled = canceled,
                        ErrorText = error,
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
                }
            }, cancellationToken);
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
