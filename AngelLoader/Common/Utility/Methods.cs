using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Security;
using System.Text;
using AngelLoader.DataClasses;
using JetBrains.Annotations;
using SevenZip;
using static AngelLoader.GameSupport;
using static AngelLoader.GameSupport.GameIndex;
using static AngelLoader.Logger;
using static AngelLoader.WinAPI.InteropMisc;

namespace AngelLoader
{
    public static partial class Misc
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
        /// <param name="overrideUseShellExecuteToOn">Force UseShellExecute to be <see langword="true"/></param>
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

        #region Get exes

        internal static string GetEditorExe(GameIndex game)
        {
            if (!GameIsDark(game)) return "";

            string gamePath = Config.GetGamePath(game);
            if (gamePath.IsEmpty()) return "";

            string edExe = Path.Combine(gamePath, game == SS2 ? Paths.ShockEdExe : Paths.DromEdExe);
            return File.Exists(edExe) ? edExe : "";
        }

        internal static string GetT2MultiplayerExe()
        {
            string gamePath = Config.GetGamePath(Thief2);
            if (gamePath.IsEmpty()) return "";

            string t2MPExe = Path.Combine(gamePath, Paths.T2MPExe);
            return File.Exists(t2MPExe) ? t2MPExe : "";
        }

        #endregion

#if false
        
        internal static string CreateTitle()
        {
            string ret = "";
            for (int i = 0; i < SupportedGameCount; i++)
            {
                string gameExe = Config.GetGameExe((GameIndex)i);
                if (!gameExe.IsWhiteSpace() && File.Exists(gameExe))
                {
                    if (ret.Length > 0) ret += ", ";
                    ret += i switch
                    {
                        0 => "T1: ",
                        1 => "T2: ",
                        2 => "T3: ",
                        _ => "SS2: "
                    };
                    Error error = TryGetGameVersion((GameIndex)i, out string version);
                    ret += error == Error.None ? version : "unknown";
                }
            }

            return ret;
        }

        internal static Error TryGetGameVersion(GameIndex game, out string version)
        {
            version = "";

            string gameExe = GameIsDark(game) ? Config.GetGameExe(game) : Path.Combine(Config.GetGamePath(Thief3), "Sneaky.dll");

            if (gameExe.IsWhiteSpace()) return Error.GameExeNotSpecified;
            if (!File.Exists(gameExe)) return Error.GameExeNotFound;

            FileStream? fs = null;
            BinaryReader? br = null;
            try
            {
                fs = new FileStream(gameExe, FileMode.Open, FileAccess.Read);
                br = new BinaryReader(fs, Encoding.ASCII, leaveOpen: false);

                long streamLen = br.BaseStream.Length;

                if (streamLen > int.MaxValue) return Error.ExeIsLargerThanInt;

                // Search starting at 88% through the file: 91% (average location) plus some wiggle room (fastest)
                long pos = (long)((88.0d / 100) * streamLen);
                long byteCount = streamLen - pos;
                br.BaseStream.Position = pos;
                byte[] bytes = new byte[byteCount];
                br.Read(bytes, 0, (int)byteCount);
                int verIndex = bytes.IndexOfByteSequence(ProductVersionBytes);

                // Fallback: search the whole file - still fast, but not as fast
                if (verIndex == -1)
                {
                    br.BaseStream.Position = 0;
                    bytes = new byte[streamLen];
                    br.Read(bytes, 0, (int)streamLen);
                    verIndex = bytes.IndexOfByteSequence(ProductVersionBytes);
                    if (verIndex == -1) return Error.GameVersionNotFound;
                }

                // Init with non-null values so we don't start out with two nulls and early-out before we do anything
                byte[] null2 = { 255, 255 };
                for (int i = verIndex + ProductVersionBytes.Length; i < bytes.Length; i++)
                {
                    if (null2[0] == '\0' && null2[1] == '\0') break;
                    null2[0] = null2[1];
                    null2[1] = bytes[i];
                    if (bytes[i] > 0) version += ((char)bytes[i]).ToString();
                }
            }
            catch (Exception ex)
            {
                Log("Exception reading/searching game exe for version string", ex);
                version = "";
                return Error.GameExeReadFailed;
            }
            finally
            {
                br?.Dispose();
                fs?.Dispose();
            }

            return Error.None;
        }

#endif

        internal static bool PathIsRelative(string path) =>
            path.Length > 1 && path[0] == '.' &&
            (path[1].IsDirSep() || (path[1] == '.' && path.Length > 2 && path[2].IsDirSep()));

        internal static void SetFMResource(FanMission fm, CustomResources resource, bool value)
        {
            if (value) { fm.Resources |= resource; } else { fm.Resources &= ~resource; }
        }

        internal static bool FMHasResource(FanMission fm, CustomResources resource) => (fm.Resources & resource) == resource;

        internal static bool FMNeedsScan(FanMission fm) => fm.Game == Game.Null || (fm.Game != Game.Unsupported && !fm.MarkedScanned);

        /// <summary>
        /// Converts a 32-bit or 64-bit Unix date string in hex format to a nullable DateTime object.
        /// </summary>
        /// <param name="unixDate"></param>
        /// <param name="convertToLocal"></param>
        /// <returns>A DateTime object, or null if the string couldn't be converted to a valid date for any reason.</returns>
        internal static DateTime? ConvertHexUnixDateToDateTime(string unixDate, bool convertToLocal = true)
        {
            bool success = long.TryParse(
                unixDate,
                NumberStyles.HexNumber,
                DateTimeFormatInfo.InvariantInfo,
                out long result);

            if (success)
            {
                try
                {
                    if (convertToLocal)
                    {
                        return DateTimeOffset
                            .FromUnixTimeSeconds(result)
                            .DateTime
                            .ToLocalTime();
                    }
                    else
                    {
                        return DateTimeOffset
                            .FromUnixTimeSeconds(result)
                            .DateTime;
                    }
                }
                catch (ArgumentOutOfRangeException)
                {
                    return null;
                }
            }

            return null;
        }

        internal static FMScanner.ScanOptions GetDefaultScanOptions() => FMScanner.ScanOptions.FalseDefault(
            scanTitle: true,
            scanAuthor: true,
            scanGameType: true,
            scanCustomResources: true,
            scanSize: true,
            scanReleaseDate: true,
            scanTags: true);

        internal static bool FMIsReallyInstalled(FanMission fm)
        {
            if (!GameIsKnownAndSupported(fm.Game)) return false;

            if (fm.Installed)
            {
                string instPath = Config.GetFMInstallPathUnsafe(fm.Game);
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

            #region Local functions

            string t2MPExe = "";
            string T2MPExe()
            {
                if (!t2MPExe.IsEmpty()) return t2MPExe;
                if (Config.GetGameExe(Thief2).IsEmpty()) return "";
                string t2Path = Config.GetGamePath(Thief2);
                return t2MPExe = t2Path.IsEmpty() ? "" : Path.Combine(t2Path, Paths.T2MPExe);
            }

            static bool AnyGameRunning(string fnb)
            {
                for (int i = 0; i < Config.GameExes.Length; i++)
                {
                    string exe = Config.GetGameExe((GameIndex)i);
                    if (!exe.IsEmpty() && fnb.PathEqualsI(exe)) return true;
                }

                return false;
            }

            static string GetProcessPath(int procId, StringBuilder _buffer)
            {
                // Recycle the buffer - avoids GC house party
                _buffer.Clear();

                using (var hProc = OpenProcess(ProcessAccessFlags.QueryLimitedInformation, false, procId))
                {
                    if (!hProc.IsInvalid)
                    {
                        int size = _buffer.Capacity;
                        if (QueryFullProcessImageName(hProc, 0, _buffer, ref size)) return _buffer.ToString();
                    }
                }
                return "";
            }

            #endregion

            var buffer = new StringBuilder(1024);

            // We're doing this whole rigamarole because the game might have been started by someone other than
            // us. Otherwise, we could just persist our process object and then we wouldn't have to do this check.
            foreach (Process proc in Process.GetProcesses())
            {
                try
                {
                    string fn = GetProcessPath(proc.Id, buffer);
                    //Log.Info("Process filename: " + fn);
                    if (!fn.IsEmpty())
                    {
                        if ((checkAllGames &&
                             (AnyGameRunning(fn) ||
                              (!T2MPExe().IsEmpty() && fn.PathEqualsI(T2MPExe())))) ||
                            (!checkAllGames &&
                             (!gameExe.IsEmpty() && fn.PathEqualsI(gameExe))))
                        {
                            string logExe = checkAllGames ? "a game exe" : gameExe;

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

        #region FM archives

        /// <summary>
        /// Returns the list of FM archive paths, returning subfolders as well if that option is enabled.
        /// </summary>
        /// <returns></returns>
        internal static List<string> GetFMArchivePaths()
        {
            var paths = new List<string>();
            foreach (string path in Config.FMArchivePaths)
            {
                paths.Add(path);
                if (Config.FMArchivePathsIncludeSubfolders)
                {
                    try
                    {
                        string[] dirs = Directory.GetDirectories(path, "*", SearchOption.AllDirectories);
                        for (int di = 0; di < dirs.Length; di++)
                        {
                            string dir = dirs[di];
                            if (!dir.GetDirNameFast().EqualsI(".fix") &&
                                // @DIRSEP: '/' conversion due to string.ContainsI()
                                !dir.ToForwardSlashes().ContainsI("/.fix/"))
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

        internal static string FindFMArchive(string fmArchive, List<string>? archivePaths = null)
        {
            if (fmArchive.IsEmpty()) return "";

            foreach (string path in (archivePaths != null && archivePaths.Count > 0 ? archivePaths : GetFMArchivePaths()))
            {
                string f = Path.Combine(path, fmArchive);
                if (File.Exists(f)) return f;
            }

            return "";
        }

        internal static List<string> FindFMArchive_Multiple(string fmArchive)
        {
            if (fmArchive.IsEmpty()) return new List<string>();

            var list = new List<string>();

            foreach (string path in GetFMArchivePaths())
            {
                string f = Path.Combine(path, fmArchive);
                if (File.Exists(f)) list.Add(f);
            }

            return list;
        }

        #endregion

        #region Set file attributes

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

        #endregion
    }
}
