using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security;
using System.Text;
using AngelLoader.Common.DataClasses;
using FMScanner;
using JetBrains.Annotations;
using SevenZip;
using static AngelLoader.Common.Common;
using static AngelLoader.Common.Games;
using static AngelLoader.Common.Games.GameIndex;
using static AngelLoader.Common.Logger;
using static AngelLoader.WinAPI.InteropMisc;

namespace AngelLoader.Common.Utility
{
    internal static class Methods
    {
        #region Process.Start with UseShellExecute on

        /// <summary>
        /// Starts a process resource by specifying the name of a document or application file and associates the resource with a new <see cref="T:System.Diagnostics.Process" /> component.
        /// <para>
        /// *Use this for Framework and Core compatibility: Core has UseShellExecute off by default (but we want it on).
        /// </para>
        /// </summary>
        /// <param name="fileName">The name of a document or application file to run in the process.</param>
        /// <returns>A new <see cref="T:System.Diagnostics.Process" /> that is associated with the process resource, or <see langword="null" /> if no process resource is started. Note that a new process that's started alongside already running instances of the same process will be independent from the others. In addition, Start may return a non-null Process with its <see cref="P:System.Diagnostics.Process.HasExited" /> property already set to <see langword="true" />. In this case, the started process may have activated an existing instance of itself and then exited.</returns>
        /// <exception cref="T:System.ComponentModel.Win32Exception">An error occurred when opening the associated file.</exception>
        /// <exception cref="T:System.ObjectDisposedException">The process object has already been disposed.</exception>
        /// <exception cref="T:System.IO.FileNotFoundException">The PATH environment variable has a string containing quotes.</exception>
        [PublicAPI]
        internal static Process ProcessStart_UseShellExecute(string fileName)
        {
            return Process.Start(new ProcessStartInfo { FileName = fileName, UseShellExecute = true });
        }

        /// <summary>
        /// Starts a process resource by specifying the name of an application and a set of command-line arguments, and associates the resource with a new <see cref="T:System.Diagnostics.Process" /> component.
        /// <para>
        /// *Use this for Framework and Core compatibility: Core has UseShellExecute off by default (but we want it on).
        /// </para>
        /// </summary>
        /// <param name="fileName">The name of an application file to run in the process.</param>
        /// <param name="arguments">Command-line arguments to pass when starting the process.</param>
        /// <returns>A new <see cref="T:System.Diagnostics.Process" /> that is associated with the process resource, or <see langword="null" /> if no process resource is started. Note that a new process that's started alongside already running instances of the same process will be independent from the others. In addition, Start may return a non-null Process with its <see cref="P:System.Diagnostics.Process.HasExited" /> property already set to <see langword="true" />. In this case, the started process may have activated an existing instance of itself and then exited.</returns>
        /// <exception cref="T:System.InvalidOperationException">The <paramref name="fileName" /> or <paramref name="arguments" /> parameter is <see langword="null" />.</exception>
        /// <exception cref="T:System.ComponentModel.Win32Exception">An error occurred when opening the associated file.
        /// -or-
        /// The sum of the length of the arguments and the length of the full path to the process exceeds 2080. The error message associated with this exception can be one of the following: "The data area passed to a system call is too small." or "Access is denied."</exception>
        /// <exception cref="T:System.ObjectDisposedException">The process object has already been disposed.</exception>
        /// <exception cref="T:System.IO.FileNotFoundException">The PATH environment variable has a string containing quotes.</exception>
        [PublicAPI]
        internal static Process ProcessStart_UseShellExecute(string fileName, string arguments)
        {
            return Process.Start(new ProcessStartInfo { FileName = fileName, Arguments = arguments, UseShellExecute = true });
        }

        /// <summary>
        /// Starts the process resource that is specified by the parameter containing process start information (for example, the file name of the process to start) and associates the resource with a new <see cref="T:System.Diagnostics.Process" /> component.
        /// <para>
        /// *Use this for Framework and Core compatibility: Core has UseShellExecute off by default (but we want it on).
        /// </para>
        /// </summary>
        /// <param name="startInfo">The <see cref="T:System.Diagnostics.ProcessStartInfo" /> that contains the information that is used to start the process, including the file name and any command-line arguments.</param>
        /// <returns>A new <see cref="T:System.Diagnostics.Process" /> that is associated with the process resource, or <see langword="null" /> if no process resource is started. Note that a new process that's started alongside already running instances of the same process will be independent from the others. In addition, Start may return a non-null Process with its <see cref="P:System.Diagnostics.Process.HasExited" /> property already set to <see langword="true" />. In this case, the started process may have activated an existing instance of itself and then exited.</returns>
        /// <exception cref="T:System.InvalidOperationException">No file name was specified in the <paramref name="startInfo" /> parameter's <see cref="P:System.Diagnostics.ProcessStartInfo.FileName" /> property.
        /// -or-
        /// The <see cref="P:System.Diagnostics.ProcessStartInfo.UseShellExecute" /> property of the <paramref name="startInfo" /> parameter is <see langword="true" /> and the <see cref="P:System.Diagnostics.ProcessStartInfo.RedirectStandardInput" />, <see cref="P:System.Diagnostics.ProcessStartInfo.RedirectStandardOutput" />, or <see cref="P:System.Diagnostics.ProcessStartInfo.RedirectStandardError" /> property is also <see langword="true" />.
        /// -or-
        /// The <see cref="P:System.Diagnostics.ProcessStartInfo.UseShellExecute" /> property of the <paramref name="startInfo" /> parameter is <see langword="true" /> and the <see cref="P:System.Diagnostics.ProcessStartInfo.UserName" /> property is not <see langword="null" /> or empty or the <see cref="P:System.Diagnostics.ProcessStartInfo.Password" /> property is not <see langword="null" />.</exception>
        /// <exception cref="T:System.ArgumentNullException">The <paramref name="startInfo" /> parameter is <see langword="null" />.</exception>
        /// <exception cref="T:System.ObjectDisposedException">The process object has already been disposed.</exception>
        /// <exception cref="T:System.IO.FileNotFoundException">The file specified in the <paramref name="startInfo" /> parameter's <see cref="P:System.Diagnostics.ProcessStartInfo.FileName" /> property could not be found.</exception>
        /// <exception cref="T:System.ComponentModel.Win32Exception">An error occurred when opening the associated file.
        /// -or-
        /// The sum of the length of the arguments and the length of the full path to the process exceeds 2080. The error message associated with this exception can be one of the following: "The data area passed to a system call is too small." or "Access is denied."</exception>
        /// <exception cref="T:System.PlatformNotSupportedException">Method not supported on operating systems without shell support such as Nano Server (.NET Core only).</exception>
        [PublicAPI]
        internal static Process ProcessStart_UseShellExecute(ProcessStartInfo startInfo, bool overrideUseShellExecuteToOn = true)
        {
            if (overrideUseShellExecuteToOn) startInfo.UseShellExecute = true;
            return Process.Start(startInfo);
        }

        /// <summary>
        /// Starts a process resource by specifying the name of an application, a set of command-line arguments, a user name, a password, and a domain and associates the resource with a new <see cref="T:System.Diagnostics.Process" /> component.
        /// <para>
        /// *Use this for Framework and Core compatibility: Core has UseShellExecute off by default (but we want it on).
        /// </para>
        /// </summary>
        /// <param name="fileName">The name of an application file to run in the process.</param>
        /// <param name="arguments">Command-line arguments to pass when starting the process.</param>
        /// <param name="userName">The user name to use when starting the process.</param>
        /// <param name="password">A <see cref="T:System.Security.SecureString" /> that contains the password to use when starting the process.</param>
        /// <param name="domain">The domain to use when starting the process.</param>
        /// <returns>A new <see cref="T:System.Diagnostics.Process" /> that is associated with the process resource, or <see langword="null" /> if no process resource is started. Note that a new process that's started alongside already running instances of the same process will be independent from the others. In addition, Start may return a non-null Process with its <see cref="P:System.Diagnostics.Process.HasExited" /> property already set to <see langword="true" />. In this case, the started process may have activated an existing instance of itself and then exited.</returns>
        /// <exception cref="T:System.InvalidOperationException">No file name was specified.</exception>
        /// <exception cref="T:System.ComponentModel.Win32Exception">An error occurred when opening the associated file.
        /// -or-
        /// The sum of the length of the arguments and the length of the full path to the associated file exceeds 2080. The error message associated with this exception can be one of the following: "The data area passed to a system call is too small." or "Access is denied."</exception>
        /// <exception cref="T:System.ObjectDisposedException">The process object has already been disposed.</exception>
        /// <exception cref="T:System.PlatformNotSupportedException">Method not supported on Linux or macOS (.NET Core only).</exception>
        [PublicAPI]
        internal static Process ProcessStart_UseShellExecute(string fileName, string arguments, string userName, SecureString password, string domain)
        {
            return Process.Start(new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                UserName = userName,
                Password = password,
                Domain = domain,
                UseShellExecute = true
            });
        }

        /// <summary>
        /// Starts a process resource by specifying the name of an application, a user name, a password, and a domain and associates the resource with a new <see cref="T:System.Diagnostics.Process" /> component.
        /// <para>
        /// *Use this for Framework and Core compatibility: Core has UseShellExecute off by default (but we want it on).
        /// </para>
        /// </summary>
        /// <param name="fileName">The name of an application file to run in the process.</param>
        /// <param name="userName">The user name to use when starting the process.</param>
        /// <param name="password">A <see cref="T:System.Security.SecureString" /> that contains the password to use when starting the process.</param>
        /// <param name="domain">The domain to use when starting the process.</param>
        /// <returns>A new <see cref="T:System.Diagnostics.Process" /> that is associated with the process resource, or <see langword="null" /> if no process resource is started. Note that a new process that's started alongside already running instances of the same process will be independent from the others. In addition, Start may return a non-null Process with its <see cref="P:System.Diagnostics.Process.HasExited" /> property already set to <see langword="true" />. In this case, the started process may have activated an existing instance of itself and then exited.</returns>
        /// <exception cref="T:System.InvalidOperationException">No file name was specified.</exception>
        /// <exception cref="T:System.ComponentModel.Win32Exception">There was an error in opening the associated file.</exception>
        /// <exception cref="T:System.ObjectDisposedException">The process object has already been disposed.</exception>
        /// <exception cref="T:System.PlatformNotSupportedException">Method not supported on Linux or macOS (.NET Core only).</exception>
        [PublicAPI]
        internal static Process ProcessStart_UseShellExecute(string fileName, string userName, SecureString password, string domain)
        {
            return Process.Start(new ProcessStartInfo
            {
                FileName = fileName,
                UserName = userName,
                Password = password,
                Domain = domain,
                UseShellExecute = true
            });
        }

        #endregion

        internal static string GetEditorExe(GameIndex game)
        {
            var gameExe = Config.GetGameExe(game);
            if (gameExe.IsEmpty()) return "";

            var gamePath = Path.GetDirectoryName(gameExe);
            if (gamePath.IsEmpty()) return "";

            var edExe = Path.Combine(gamePath, game == SS2 ? Paths.ShockEdExe : Paths.DromEdExe);
            return File.Exists(edExe) ? edExe : "";
        }

        internal static string GetT2MultiplayerExe()
        {
            if (Config.GetGameExe(Thief2).IsEmpty()) return "";

            var gamePath = Path.GetDirectoryName(Config.GetGameExe(Thief2));
            if (gamePath.IsEmpty()) return "";

            var t2MPExe = Path.Combine(gamePath, Paths.T2MPExe);
            return File.Exists(t2MPExe) ? t2MPExe : "";
        }

        internal static ScanOptions GetDefaultScanOptions()
        {
            return ScanOptions.FalseDefault(
                scanTitle: true,
                scanAuthor: true,
                scanGameType: true,
                scanCustomResources: true,
                scanSize: true,
                scanReleaseDate: true,
                scanTags: true);
        }

        internal static bool FMCustomResourcesScanned(FanMission fm)
        {
            return fm.HasMap != null &&
                   fm.HasAutomap != null &&
                   fm.HasScripts != null &&
                   fm.HasTextures != null &&
                   fm.HasSounds != null &&
                   fm.HasObjects != null &&
                   fm.HasCreatures != null &&
                   fm.HasMotions != null &&
                   fm.HasMovies != null &&
                   fm.HasSubtitles != null;
        }

        internal static bool FMIsReallyInstalled(FanMission fm)
        {
            if (!GameIsKnownAndSupported(fm.Game)) return false;

            if (fm.Installed)
            {
                var instPath = Config.GetFMInstallPathUnsafe(fm.Game);
                if (instPath.IsEmpty()) return false;

                string path;
                try
                {
                    path = Path.Combine(instPath, fm.InstalledDir);
                }
                catch (Exception)
                {
                    return false;
                }

                return Directory.Exists(path);
            }

            return false;
        }

        internal static bool GameIsRunning(string gameExe, bool checkAllGames = false)
        {
            //Log("Checking if " + gameExe + " is running. Listing processes...");
            Log("Checking if " + gameExe + " is running.");

            string t2MPExe = "";
            string T2MPExe()
            {
                if (!t2MPExe.IsEmpty()) return t2MPExe;
                if (Config.GetGameExe(Thief2).IsEmpty()) return "";
                var t2Path = Path.GetDirectoryName(Config.GetGameExe(Thief2));
                return t2MPExe = (t2Path.IsEmpty() ? "" : Path.Combine(t2Path, Paths.T2MPExe));
            }

            static bool AnyGameRunning(string fnb)
            {
                for (int i = 0; i < Config.GameExes.Length; i++)
                {
                    var exe = Config.GetGameExe((GameIndex)i);
                    if (!exe.IsEmpty() && fnb.EqualsI(exe.ToBackSlashes())) return true;
                }

                return false;
            }

            // We're doing this whole rigamarole because the game might have been started by someone other than
            // us. Otherwise, we could just persist our process object and then we wouldn't have to do this check.
            foreach (var proc in Process.GetProcesses())
            {
                try
                {
                    var fn = GetProcessPath(proc.Id);
                    //Log.Info("Process filename: " + fn);
                    if (!fn.IsEmpty())
                    {
                        var fnb = fn.ToBackSlashes();
                        if ((checkAllGames &&
                             (AnyGameRunning(fnb) ||
                              (!T2MPExe().IsEmpty() && fnb.EqualsI(T2MPExe().ToBackSlashes())))) ||
                            (!checkAllGames &&
                             (!gameExe.IsEmpty() && fnb.EqualsI(gameExe.ToBackSlashes()))))
                        {
                            var logExe = checkAllGames ? "a game exe" : gameExe;

                            Log("Found " + logExe + " running: " + fn +
                                "\r\nReturning true, game should be blocked from starting");
                            return true;
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Even if this were to be one of our games, if .NET won't let us find out then all we can do
                    // is shrug and move on.
                    Log("Exception caught in GameIsRunning", ex);
                }
            }

            return false;
        }

        [PublicAPI]
        internal static string GetProcessPath(int procId)
        {
            using (var hProc = OpenProcess(ProcessAccessFlags.QueryLimitedInformation, false, procId))
            {
                if (!hProc.IsInvalid)
                {
                    var buffer = new StringBuilder(1024);
                    int size = buffer.Capacity;
                    if (QueryFullProcessImageName(hProc, 0, buffer, ref size)) return buffer.ToString();
                }
            }
            return "";
        }

        /// <summary>
        /// Returns the list of FM archive paths, returning subfolders as well if that option is enabled.
        /// </summary>
        /// <returns></returns>
        internal static List<string> GetFMArchivePaths()
        {
            var paths = new List<string>();
            foreach (var path in Config.FMArchivePaths)
            {
                paths.Add(path);
                if (Config.FMArchivePathsIncludeSubfolders)
                {
                    try
                    {
                        foreach (var dir in Directory.GetDirectories(path, "*", SearchOption.AllDirectories))
                        {
                            if (!dir.GetDirNameFast().EqualsI(".fix") &&
                                !dir.ContainsI(Path.DirectorySeparatorChar + ".fix" + Path.DirectorySeparatorChar))
                            {
                                paths.Add(dir);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Log(nameof(GetFMArchivePaths) + " : subfolders=true, Exception in GetDirectories", ex);
                    }
                }
            }

            return paths;
        }

        internal static string FindFMArchive(FanMission fm, List<string> archivePaths = null)
        {
            if (fm.Archive.IsEmpty()) return null;

            foreach (var path in (archivePaths != null && archivePaths.Count > 0 ? archivePaths : GetFMArchivePaths()))
            {
                var f = Path.Combine(path, fm.Archive);
                if (File.Exists(f)) return f;
            }

            return null;
        }

        internal static void UnSetReadOnly(string fileOnDiskFullPath)
        {
            try
            {
                new FileInfo(fileOnDiskFullPath).IsReadOnly = false;
            }
            catch (Exception ex)
            {
                Log("Unable to set file attributes for " + fileOnDiskFullPath, ex);
            }
        }

        internal static void SetFileAttributesFromSevenZipEntry(ArchiveFileInfo archiveFileInfo, string fileOnDiskFullPath)
        {
            // ExtractFile() doesn't set these, so we have to set them ourselves.
            // ExtractArchive() sets them though, so we don't need to call this when using that.
            try
            {
                _ = new FileInfo(fileOnDiskFullPath)
                {
                    IsReadOnly = false,
                    LastWriteTime = archiveFileInfo.LastWriteTime,
                    CreationTime = archiveFileInfo.CreationTime,
                    LastAccessTime = archiveFileInfo.LastAccessTime
                };
            }
            catch (Exception ex)
            {
                Log("Unable to set file attributes for " + fileOnDiskFullPath, ex);
            }
        }

        #region Tags

        // Update fm.TagsString here. We keep TagsString around because when we're reading, writing, and merging
        // FMs, we don't want to spend time converting back and forth. So Tags is session-only, and only gets
        // filled out for FMs that will be displayed. TagsString is the one that gets saved and loaded, and must
        // be kept in sync with Tags. This should ONLY be called when a tag is added or removed. Keep it simple
        // so we can see and follow the logic.
        internal static void UpdateFMTagsString(FanMission fm)
        {
            var intermediateList = new List<string>();
            foreach (var item in fm.Tags)
            {
                if (item.Tags.Count == 0)
                {
                    intermediateList.Add(item.Category);
                }
                else
                {
                    foreach (var tag in item.Tags)
                    {
                        intermediateList.Add(item.Category + ":" + tag);
                    }
                }
            }

            var sb = new StringBuilder();
            for (int i = 0; i < intermediateList.Count; i++)
            {
                if (i > 0) sb.Append(',');
                sb.Append(intermediateList[i]);
            }

            fm.TagsString = sb.ToString();
        }

        // Very awkward procedure that accesses global state in the name of only doing one iteration
        // TODO: Test perf when 1000+ FMs each have a bunch of tags
        internal static void AddTagsToFMAndGlobalList(string tagsToAdd, CatAndTagsList existingFMTags)
        {
            if (tagsToAdd.IsEmpty()) return;

            var tagsArray = tagsToAdd.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var item in tagsArray)
            {
                string cat, tag;

                var colonCount = item.CountChars(':');

                // No way josé
                if (colonCount > 1) continue;

                if (colonCount == 1)
                {
                    var index = item.IndexOf(':');
                    cat = item.Substring(0, index).Trim().ToLowerInvariant();
                    tag = item.Substring(index + 1).Trim();
                    if (cat.IsEmpty() || tag.IsEmpty()) continue;
                }
                else
                {
                    cat = "misc";
                    tag = item.Trim();
                }

                // Note: We've already converted cat to lowercase, so we just do straight == to shave time off

                #region FM tags

                CatAndTags match = null;
                for (int i = 0; i < existingFMTags.Count; i++)
                {
                    if (existingFMTags[i].Category == cat) match = existingFMTags[i];
                }
                if (match == null)
                {
                    existingFMTags.Add(new CatAndTags { Category = cat });
                    existingFMTags[existingFMTags.Count - 1].Tags.Add(tag);
                }
                else
                {
                    if (!match.Tags.ContainsI(tag)) match.Tags.Add(tag);
                }

                #endregion

                #region Global tags

                GlobalCatAndTags globalMatch = null;
                for (int i = 0; i < GlobalTags.Count; i++)
                {
                    if (GlobalTags[i].Category.Name == cat) globalMatch = GlobalTags[i];
                }
                if (globalMatch == null)
                {
                    GlobalTags.Add(new GlobalCatAndTags { Category = new GlobalCatOrTag { Name = cat, UsedCount = 1 } });
                    GlobalTags[GlobalTags.Count - 1].Tags.Add(new GlobalCatOrTag { Name = tag, UsedCount = 1 });
                }
                else
                {
                    globalMatch.Category.UsedCount++;

                    var ft = FirstTagOrNull(globalMatch.Tags, tag);
                    if (ft == null)
                    {
                        globalMatch.Tags.Add(new GlobalCatOrTag { Name = tag, UsedCount = 1 });
                    }
                    else
                    {
                        ft.UsedCount++;
                    }
                }

                #endregion
            }
        }

        // Avoid the overhead of FirstOrDefault()
        private static GlobalCatOrTag FirstTagOrNull(List<GlobalCatOrTag> tagsList, string tag)
        {
            for (int i = 0; i < tagsList.Count; i++)
            {
                if (tagsList[i].Name.EqualsI(tag)) return tagsList[i];
            }

            return null;
        }

        #endregion
    }
}
