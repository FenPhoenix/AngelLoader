using System;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using AL_Common;
using AngelLoader.DataClasses;
using JetBrains.Annotations;
using static AL_Common.Logger;
using static AngelLoader.GameSupport;
using static AngelLoader.Global;
using static AngelLoader.NativeCommon;

namespace AngelLoader;

public static partial class Utils
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

    #region FM utils

    /// <summary>
    /// Returns true if <paramref name="fm"/> is actually installed on disk. Specifically, it checks only if
    /// the FM's installed folder exists in the expected place, and not whether the folder contains a complete
    /// and validly structured FM or even if it contains anything at all. Validity is assumed.
    /// </summary>
    /// <param name="fm"></param>
    /// <param name="fmInstalledPath">If the FM exists on disk, the FM's installed path; otherwise, the empty string.</param>
    /// <returns></returns>
    internal static bool FMIsReallyInstalled(FanMission fm, out string fmInstalledPath)
    {
        fmInstalledPath = "";

        if (!fm.Game.ConvertsToKnownAndSupported(out GameIndex gameIndex))
        {
            return false;
        }

        string instPath = Config.GetFMInstallPath(gameIndex);
        if (gameIndex == GameIndex.TDM)
        {
            return !instPath.IsEmpty() &&
                   TryCombineDirectoryPathAndCheckExistence(instPath, fm.TDMInstalledDir, out fmInstalledPath);
        }

        if (!fm.Installed)
        {
            return false;
        }

        return !instPath.IsEmpty() &&
               TryCombineDirectoryPathAndCheckExistence(instPath, fm.RealInstalledDir, out fmInstalledPath);
    }

    #endregion

    /// <summary>
    /// Converts a 32-bit or 64-bit Unix date string in hex format to a nullable DateTime object.
    /// </summary>
    /// <param name="unixDate"></param>
    /// <returns>A DateTime object, or null if the string couldn't be converted to a valid date for any reason.</returns>
    internal static DateTime? ConvertHexUnixDateToDateTime(string unixDate)
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
                return DateTimeOffset.FromUnixTimeSeconds(result).DateTime.ToLocalTime();
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
            if (Config.GetGameExe(GameIndex.Thief2).IsEmpty()) return "";
            string t2Path = Config.GetGamePath(GameIndex.Thief2);
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
            buffer.Clear();

            using var hProc = OpenProcess(QUERY_LIMITED_INFORMATION, false, procId);
            if (!hProc.IsInvalid)
            {
                int size = buffer.Capacity;
                if (QueryFullProcessImageNameW(hProc, 0, buffer, ref size)) return buffer.ToString();
            }
            return "";
        }

        #endregion

        var buffer = new StringBuilder(1024);

        // We're doing this whole rigamarole because the game might have been started by someone other than
        // us. Otherwise, we could just persist our process object and then we wouldn't have to do this check.

        Process[] processes = Process.GetProcesses();
        try
        {
            foreach (Process proc in processes)
            {
                try
                {
                    string fn = GetProcessPath(proc.Id, buffer);
                    if (!fn.IsEmpty() &&
                        ((checkAllGames &&
                          (AnyGameRunning(fn) ||
                           (!T2MPExe().IsEmpty() && fn.PathEqualsI(T2MPExe())))) ||
                         (!checkAllGames &&
                          !gameExe.IsEmpty() && fn.PathEqualsI(gameExe))))
                    {
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    Log(ex: ex);
                }
            }
        }
        finally
        {
            processes.DisposeAll();
        }

        return false;
    }

    internal static bool WinVersionIs7OrAbove()
    {
        try
        {
            OperatingSystem osVersion = Environment.OSVersion;
            return osVersion.Platform == PlatformID.Win32NT &&
                   osVersion.Version >= new Version(6, 1);

            // Win8 check: same but version is 6, 2
        }
        catch
        {
            return false;
        }
    }

    internal static bool WinVersionSupportsDarkMode()
    {
        try
        {
            OperatingSystem osVersion = Environment.OSVersion;
            return osVersion.Platform == PlatformID.Win32NT &&
                   osVersion.Version.Major >= 10 &&
                   osVersion.Version.Build >= 17763;
        }
        catch
        {
            return false;
        }
    }

    internal static void LogFMInfo(
        FanMission fm,
        string topMessage,
        Exception? ex = null,
        bool stackTrace = false,
        [CallerMemberName] string callerMemberName = "")
    {
        Log("Caller: " + callerMemberName + "\r\n\r\n" +
            topMessage + "\r\n" +
            "fm." + nameof(fm.Game) + ": " + fm.Game + "\r\n" +
            "fm." + nameof(fm.Archive) + ": " + fm.Archive + "\r\n" +
            "fm." + nameof(fm.InstalledDir) + ": " + fm.InstalledDir + "\r\n" +
            "fm." + nameof(fm.TDMInstalledDir) + " (if applicable): " + fm.TDMInstalledDir + "\r\n" +
            "fm." + nameof(fm.Installed) + ": " + fm.Installed + "\r\n" +
            (fm.Game.ConvertsToKnownAndSupported(out GameIndex gameIndex)
                ? "Base directory for installed FMs: " + Config.GetFMInstallPath(gameIndex)
                : "Game type is not known or not supported.") +
            (ex != null ? "\r\nException:\r\n" + ex : ""), stackTrace: stackTrace);
    }

    internal static Font GetMicrosoftSansSerifDefault() => new("Microsoft Sans Serif", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 0);

    internal static string AutodetectDarkLoaderFile(string fileName)
    {
        // Common locations. Don't go overboard and search the whole filesystem; that would take forever.
        string[] dlLocations =
        {
            "DarkLoader",
            @"Games\DarkLoader"
        };

        DriveInfo[] drives;
        try
        {
            drives = DriveInfo.GetDrives();
        }
        catch (Exception ex)
        {
            Log(ex: ex);
            return "";
        }

        try
        {
            string dlIni;
            foreach (DriveInfo drive in drives)
            {
                if (!drive.IsReady || drive.DriveType != DriveType.Fixed) continue;

                foreach (string loc in dlLocations)
                {
                    if (TryCombineFilePathAndCheckExistence(drive.Name, loc, fileName, out dlIni))
                    {
                        return dlIni;
                    }
                }
            }

            if (TryCombineFilePathAndCheckExistence(
                    Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
                    "DarkLoader",
                    fileName,
                    out dlIni))
            {
                return dlIni;
            }
        }
        catch (Exception ex)
        {
            Log(ErrorText.Ex + "in DarkLoader multi-drive search", ex);
        }

        return "";
    }

    internal static bool BackupPathInvalid(string backupPath)
    {
        for (int i = 0; i < SupportedGameCount; i++)
        {
            GameIndex gameIndex = (GameIndex)i;
            if (GameRequiresBackupPath(gameIndex) &&
                !Config.GetGameExe(gameIndex).IsEmpty() &&
                !Directory.Exists(backupPath))
            {
                return true;
            }
        }

        return false;
    }

    internal static void Reset(this System.Timers.Timer timer)
    {
        timer.Stop();
        timer.Start();
    }

    internal static Encoding GetOEMCodePageOrFallback(Encoding fallback)
    {
        Encoding enc;
        try
        {
            enc = Encoding.GetEncoding(CultureInfo.CurrentCulture.TextInfo.OEMCodePage);
        }
        catch
        {
            enc = fallback;
        }

        return enc;
    }

    internal static void ResetColumnDisplayIndexes(ColumnData[] columns)
    {
        for (int i = 0; i < columns.Length; i++)
        {
            columns[i].DisplayIndex = i;
        }
    }

#if DateAccTest
    internal static string DateAccuracy_Serialize(DateAccuracy da) => da switch
    {
        DateAccuracy.Green => "Green",
        DateAccuracy.Yellow => "Yellow",
        DateAccuracy.Red => "Red",
        _ => "Null"
    };

    internal static DateAccuracy DateAccuracy_Deserialize(string str) => str switch
    {
        "Green" => DateAccuracy.Green,
        "Yellow" => DateAccuracy.Yellow,
        "Red" => DateAccuracy.Red,
        _ => DateAccuracy.Null
    };
#endif
}
