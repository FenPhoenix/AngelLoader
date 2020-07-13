using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Security;
using System.Text;
using System.Threading;
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
        internal static int GetPercentFromValue(int current, int total) => (100 * current) / total;
        internal static long GetValueFromPercent(double percent, long total) => (long)((percent / 100) * total);

        #region Clamping

        /// <summary>
        /// Clamps a number to between min and max.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        internal static T Clamp<T>(this T value, T min, T max) where T : IComparable<T>
        {
            return value.CompareTo(min) < 0 ? min : value.CompareTo(max) > 0 ? max : value;
        }

        /// <summary>
        /// If <paramref name="value"/> is less than zero, returns zero. Otherwise, returns <paramref name="value"/>
        /// unchanged.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        internal static int ClampToZero(this int value) => Math.Max(value, 0);

        #endregion

        // Depend on non-defined symbol if we're in public release profile, to prevent the bloat of calls to this
#if ReleasePublic || NoAsserts
        [Conditional("_AngelLoader_In_Release_Public_Mode")]
#endif
        [PublicAPI]
        [AssertionMethod]
        public static void AssertR([AssertionCondition(AssertionConditionType.IS_TRUE)] bool condition, string message, string detailedMessage = "")
            => Trace.Assert(condition, message, detailedMessage);

        #region Get exes

        internal static string GetEditorExe(GameIndex game)
        {
            if (!GameIsDark(game)) return "";

            string gamePath = Config.GetGamePath(game);
            if (gamePath.IsEmpty()) return "";

            string exe = game == SS2 ? Paths.ShockEdExe : Paths.DromEdExe;
            return TryCombineFilePathAndCheckExistence(gamePath, exe, out string fullPathExe)
                ? fullPathExe
                : "";
        }

        internal static string GetT2MultiplayerExe()
        {
            string gamePath = Config.GetGamePath(Thief2);
            if (gamePath.IsEmpty()) return "";

            return TryCombineFilePathAndCheckExistence(gamePath, Paths.T2MPExe, out string fullPathExe)
                ? fullPathExe
                : "";
        }

        #endregion

#if false
        
        internal static int IndexOfByteSequence(this byte[] input, byte[] pattern)
        {
            var firstByte = pattern[0];
            int index = Array.IndexOf(input, firstByte);

            while (index > -1)
            {
                for (int i = 0; i < pattern.Length; i++)
                {
                    if (index + i >= input.Length) return -1;
                    if (pattern[i] != input[index + i])
                    {
                        if ((index = Array.IndexOf(input, firstByte, index + i)) == -1) return -1;
                        break;
                    }

                    if (i == pattern.Length - 1) return index;
                }
            }

            return index;
        }

        // Used for detecting NewDark version of NewDark executables
        private static readonly byte[] ProductVersionBytes = Encoding.ASCII.GetBytes(new[]
        {
            'P', '\0', 'r', '\0', 'o', '\0', 'd', '\0', 'u', '\0', 'c', '\0', 't', '\0',
            'V', '\0', 'e', '\0', 'r', '\0', 's', '\0', 'i', '\0', 'o', '\0', 'n', '\0', '\0', '\0'
        });

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
                long pos = GetValueFromPercent(88.0d, streamLen);
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
                    DateTime dt = DateTimeOffset.FromUnixTimeSeconds(result).DateTime;
                    return convertToLocal ? dt.ToLocalTime() : dt;
                }
                catch (ArgumentOutOfRangeException)
                {
                    return null;
                }
            }

            return null;
        }

        internal static bool FMIsReallyInstalled(FanMission fm)
        {
            if (!GameIsKnownAndSupported(fm.Game) || !fm.Installed) return false;

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

        internal static bool GameIsRunning(string gameExe, bool checkAllGames = false)
        {
            #region Local functions

            string t2MPExe = "";
            string T2MPExe()
            {
                if (!t2MPExe.IsEmpty()) return t2MPExe;
                if (Config.GetGameExe(Thief2).IsEmpty()) return "";
                string t2Path = Config.GetGamePath(Thief2);
                return t2MPExe = t2Path.IsEmpty() ? "" : Path.Combine(t2Path, Paths.T2MPExe);
            }

            static bool AnyGameRunning(string path)
            {
                for (int i = 0; i < SupportedGameCount; i++)
                {
                    string exe = Config.GetGameExe((GameIndex)i);
                    if (!exe.IsEmpty() && path.PathEqualsI(exe)) return true;
                }

                return false;
            }

            static string GetProcessPath(int procId, StringBuilder buffer)
            {
                // Recycle the buffer - avoids GC house party
                buffer.Clear();

                using var hProc = OpenProcess(ProcessAccessFlags.QueryLimitedInformation, false, procId);
                if (!hProc.IsInvalid)
                {
                    int size = buffer.Capacity;
                    if (QueryFullProcessImageName(hProc, 0, buffer, ref size)) return buffer.ToString();
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
                    if (!fn.IsEmpty() &&
                        ((checkAllGames &&
                          (AnyGameRunning(fn) ||
                           (!T2MPExe().IsEmpty() && fn.PathEqualsI(T2MPExe())))) ||
                         (!checkAllGames &&
                          (!gameExe.IsEmpty() && fn.PathEqualsI(gameExe)))))
                    {
                        string logExe = checkAllGames ? "a game exe" : gameExe;

                        Log("Found " + logExe + " running: " + fn +
                            "\r\nReturning true, game should be blocked from starting");
                        return true;
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

            var paths = archivePaths?.Count > 0 ? archivePaths : GetFMArchivePaths();
            foreach (string path in paths)
            {
                if (TryCombineFilePathAndCheckExistence(path, fmArchive, out string f))
                {
                    return f;
                }
            }

            return "";
        }

        internal static List<string> FindFMArchive_Multiple(string fmArchive)
        {
            if (fmArchive.IsEmpty()) return new List<string>();

            var list = new List<string>();

            foreach (string path in GetFMArchivePaths())
            {
                if (TryCombineFilePathAndCheckExistence(path, fmArchive, out string f))
                {
                    list.Add(f);
                }
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

        internal static void CancelIfNotDisposed(this CancellationTokenSource value)
        {
            try { value.Cancel(); } catch (ObjectDisposedException) { }
        }
    }
}
