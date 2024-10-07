using System;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using AL_Common;
using AngelLoader.DataClasses;
using JetBrains.Annotations;
using static AL_Common.Common;
using static AL_Common.Logger;
using static AngelLoader.GameSupport;
using static AngelLoader.Global;
using static AngelLoader.Misc;
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

        #endregion

        // We're doing this whole rigamarole because the game might have been started by someone other than
        // us. Otherwise, we could just persist our process object and then we wouldn't have to do this check.

        Process[] processes = Process.GetProcesses();
        try
        {
            foreach (Process proc in processes)
            {
                try
                {
                    string? fn = GetProcessPath(proc.Id);
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
        }
        catch
        {
            return false;
        }
    }

    internal static bool WinVersionIs8OrAbove()
    {
        try
        {
            OperatingSystem osVersion = Environment.OSVersion;
            return osVersion.Platform == PlatformID.Win32NT &&
                   osVersion.Version >= new Version(6, 2);
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
                   osVersion.Version >= new Version(10, 0, 17763);
        }
        catch
        {
            return false;
        }
    }

    internal static Font GetMicrosoftSansSerifDefault() => new("Microsoft Sans Serif", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 0);

    internal static string AutodetectDarkLoaderFile(string fileName)
    {
        // Common locations. Don't go overboard and search the whole filesystem; that would take forever.
        string[] dlLocations =
        {
            "DarkLoader",
            @"Games\DarkLoader",
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

    internal static void ResetColumnDisplayIndexes(ColumnDataArray columns)
    {
        for (int i = 0; i < ColumnCount; i++)
        {
            columns[i].DisplayIndex = i;
        }
    }

    // PERF: ~0.14ms per FM for en-US Long Date format
    // @PERF_TODO: Test with custom - dt.ToString() might be slow?
    internal static string FormatDate(DateTime dt) => Config.DateFormat switch
    {
        DateFormat.CurrentCultureShort => dt.ToShortDateString(),
        DateFormat.CurrentCultureLong => dt.ToLongDateString(),
        _ => dt.ToString(Config.DateCustomFormatString, CultureInfo.CurrentCulture),
    };

    internal static string FormatSize(ulong size) => size switch
    {
        0 => "",
        < ByteSize.MB => Math.Round(size / 1024f).ToStrCur() + " " + LText.Global.KilobyteShort,
        < ByteSize.GB => Math.Round(size / 1024f / 1024f).ToStrCur() + " " + LText.Global.MegabyteShort,
        _ => Math.Round(size / 1024f / 1024f / 1024f, 2).ToStrCur() + " " + LText.Global.GigabyteShort,
    };

    internal static int GetThreadCountForParallelOperation(int maxWorkItemsCount)
    {
        int threads = Config.IOThreadingLevel switch
        {
            IOThreadingLevel.Custom => Config.CustomIOThreads,
            IOThreadingLevel.HDD => 1,
            IOThreadingLevel.SATA_SSD => CoreCount,
            IOThreadingLevel.NVMe_SSD => CoreCount,
            _ => Config.AllDrivesType != AL_DriveType.Other
                ? CoreCount
                : 1,
        };

        return Math.Min(threads, maxWorkItemsCount);
    }

#if DateAccTest
    internal static string DateAccuracy_Serialize(DateAccuracy da) => da switch
    {
        DateAccuracy.Green => "Green",
        DateAccuracy.Yellow => "Yellow",
        DateAccuracy.Red => "Red",
        _ => "Null",
    };

    internal static DateAccuracy DateAccuracy_Deserialize(string str) => str switch
    {
        "Green" => DateAccuracy.Green,
        "Yellow" => DateAccuracy.Yellow,
        "Red" => DateAccuracy.Red,
        _ => DateAccuracy.Null,
    };
#endif
}
