using System;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using AL_Common;
using AngelLoader.DataClasses;
using JetBrains.Annotations;
using static AngelLoader.GameSupport;
using static AngelLoader.GameSupport.GameIndex;
using static AngelLoader.Logger;
using static AngelLoader.WinAPI.Native;

namespace AngelLoader
{
    public static partial class Misc
    {
        // Depend on non-defined symbol if we're in public release profile, to prevent the bloat of calls to this
#if ReleasePublic || NoAsserts
        [Conditional("_AngelLoader_In_Release_Public_Mode")]
#endif
        [PublicAPI]
        [AssertionMethod]
        public static void AssertR(
            [AssertionCondition(AssertionConditionType.IS_TRUE)]
            bool condition,
            string message,
            string detailedMessage = "")
            => Trace.Assert(condition, message, detailedMessage);

        #region Numeric

        #region Clamping

        internal static float ClampToRichTextBoxZoomMinMax(this float value) => value.Clamp(0.1f, 5.0f);

        internal static float ClampToFMsDGVFontSizeMinMax(this float value)
        {
            if (value < Math.Round(1.00f, 2)) value = 1.00f;
            if (value > Math.Round(41.25f, 2)) value = 41.25f;
            return (float)Math.Round(value, 2);
        }

        #endregion

        internal static float CubicRoot(float x) => (float)Math.Pow(x, 1f / 3f);

        #endregion

        #region Set file attributes

        internal static void File_UnSetReadOnly(string fileOnDiskFullPath)
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

        internal static void Dir_UnSetReadOnly(string dirOnDiskFullPath)
        {
            try
            {
                _ = new DirectoryInfo(dirOnDiskFullPath) { Attributes = FileAttributes.Normal };
                // TODO: Dir_UnSetReadOnly: More correct but possibly breaking change
                //new DirectoryInfo(dirOnDiskFullPath).Attributes &= ~FileAttributes.ReadOnly;
            }
            catch (Exception ex)
            {
                Log("Unable to set directory attributes for " + dirOnDiskFullPath, ex);
            }
        }

        #endregion

        #region FM utils

        internal static void SetFMResource(FanMission fm, CustomResources resource, bool value)
        {
            if (value) { fm.Resources |= resource; } else { fm.Resources &= ~resource; }
        }

        internal static bool FMHasResource(FanMission fm, CustomResources resource) => (fm.Resources & resource) == resource;

        internal static bool FMNeedsScan(FanMission fm) => !fm.MarkedUnavailable && (fm.Game == Game.Null ||
                                                           (fm.Game != Game.Unsupported && !fm.MarkedScanned));

        /// <summary>
        /// Returns true if <paramref name="fm"/> is actually installed on disk. Specifically, it checks only if
        /// the FM's installed folder exists in the expected place, and not whether the folder contains a complete
        /// and validly structured FM or even if it contains anything at all. Validity is assumed.
        /// </summary>
        /// <param name="fm"></param>
        /// <returns></returns>
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
            catch
            {
                return false;
            }

            return Directory.Exists(path);
        }

        #endregion

        #region Enum HasFlagFast

        internal static bool HasFlagFast(this FinishedState @enum, FinishedState flag) => (@enum & flag) == flag;

        internal static bool HasFlagFast(this Game @enum, Game flag) => (@enum & flag) == flag;

        internal static bool HasFlagFast(this Difficulty @enum, Difficulty flag) => (@enum & flag) == flag;

        #endregion

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

                using var hProc = OpenProcess(QUERY_LIMITED_INFORMATION, false, procId);
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
                    Log(ex: ex);
                }
            }

            return false;
        }

        internal static void CancelIfNotDisposed(this CancellationTokenSource value)
        {
            try { value.Cancel(); } catch (ObjectDisposedException) { }
        }

        internal static bool WinVersionIs7OrAbove()
        {
            try
            {
                return Environment.OSVersion.Platform == PlatformID.Win32NT &&
                       Environment.OSVersion.Version >= new Version(6, 1);

                // Win8 check: same but version is 6, 2
            }
            catch
            {
                return false;
            }
        }

        internal static ZipArchive GetZipArchiveCharEnc(string fileName)
        {
            // One user was getting "1 is not a supported code page" with this(?!) so fall back in that case...
            Encoding enc;
            try
            {
                enc = Encoding.GetEncoding(CultureInfo.CurrentCulture.TextInfo.OEMCodePage);
            }
            catch
            {
                enc = Encoding.UTF8;
            }

            return new ZipArchive(File.OpenRead(fileName), ZipArchiveMode.Read, leaveOpen: false, enc);
        }
    }
}
