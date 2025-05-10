//#define ALWAYS_USE_APP_STARTUP_PATH

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using AngelLoader.DataClasses;
using Microsoft.Win32;
using static AL_Common.Logger;
using static AngelLoader.GameSupport;
using static AngelLoader.Global;
using static AngelLoader.Utils;

namespace AngelLoader;

internal sealed class TempPath(Func<string> pathGetter)
{
    internal readonly Func<string> PathGetter = pathGetter;
}

internal static class TempPaths
{
    internal static readonly TempPath Update = new(static () => Paths.UpdateTemp);
    internal static readonly TempPath UpdateBak = new(static () => Paths.UpdateBakTemp);
    internal static readonly TempPath UpdateAppDownload = new(static () => Paths.UpdateAppDownloadTemp);
    internal static readonly TempPath Help = new(static () => Paths.HelpTemp);
    internal static readonly TempPath SevenZipList = new(static () => Paths.SevenZipListTemp);
    internal static readonly TempPath StubComm = new(static () => Paths.StubCommTemp);
    internal static readonly TempPath FMScanner = new(static () => Paths.FMScannerTemp);
    internal static readonly TempPath FMCache = new(static () => Paths.FMCacheTemp);
}

internal static class Paths
{
    #region Startup path

    // We use this pulled-out Application.StartupPath code, so we don't rely on the WinForms Application class

    [DllImport("kernel32.dll", ExactSpelling = true, CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern int GetModuleFileNameW(HandleRef hModule, StringBuilder buffer, int length);
    private static string GetStartupPath()
    {
        HandleRef nullHandleRef = new(null, IntPtr.Zero);
        const int MAX_UNICODESTRING_LEN = short.MaxValue;
        const int ERROR_INSUFFICIENT_BUFFER = 122;

        StringBuilder buffer = new(MAX_PATH);
        int noOfTimes = 1;
        int length;
        while (((length = GetModuleFileNameW(nullHandleRef, buffer, buffer.Capacity)) == buffer.Capacity) &&
               Marshal.GetLastWin32Error() == ERROR_INSUFFICIENT_BUFFER &&
               buffer.Capacity < MAX_UNICODESTRING_LEN)
        {
            noOfTimes += 2;
            int capacity = noOfTimes * MAX_PATH < MAX_UNICODESTRING_LEN ? noOfTimes * MAX_PATH : MAX_UNICODESTRING_LEN;
            buffer.EnsureCapacity(capacity);
        }
        buffer.Length = length;
        return Path.GetDirectoryName(buffer.ToString())!;
    }

#if DEBUG || Release_Testing
    internal static readonly string Startup = GetStartupPath_Private();

    private static string GetStartupPath_Private()
    {
        try
        {
            /*
            I like to dev with my normal copy of AngelLoader in C:\AngelLoader (because it has all my data there),
            hence I want it running from there when I debug and run from the IDE. But we don't want other people
            to have that hardcoded path in here, because if they have a non-dev AL install in C:\AngelLoader then
            it will very probably get messed with if they run or compile the code. So only do the hardcoded path
            if my personal environment var is set. Obviously don't add this var yourself.
            */
#if ALWAYS_USE_APP_STARTUP_PATH
            return GetStartupPath();
#endif

            string? val = Environment.GetEnvironmentVariable("AL_FEN_PERSONAL_DEV_3053BA21", EnvironmentVariableTarget.Machine);
            return val?.EqualsTrue() == true ? @"C:\AngelLoader" : GetStartupPath();
        }
        catch
        {
            return GetStartupPath();
        }
    }
#else
    internal static readonly string Startup = GetStartupPath();
#endif

    #endregion

    #region Temp

    // @Robustness: I guess this could throw if GetTempPath() returns something with invalid chars(?)
    // Probably can be considered practically impossible... But check if doing cross-platform?
    // -Keep Path.Combine() so we at least throw and exit if that happens though
    // -Also keep this immediately-loaded for the above reason
    internal static readonly string BaseTemp = Path.Combine(Path.GetTempPath(), "AngelLoader");

    #region Help

    internal static readonly string HelpTemp = PathCombineFast_NoChecks(BaseTemp, "Help");

    internal static readonly string HelpRedirectFilePath = PathCombineFast_NoChecks(HelpTemp, "redir.html");

    #endregion

    #region Scan

    internal static readonly string FMScannerTemp = PathCombineFast_NoChecks(BaseTemp, "FMScan");

    internal static readonly string SevenZipListTemp = PathCombineFast_NoChecks(BaseTemp, "7zl");

    #endregion

    internal static readonly string FMCacheTemp = PathCombineFast_NoChecks(BaseTemp, "FMCache");

    #region Stub

    internal static readonly string StubCommTemp = PathCombineFast_NoChecks(BaseTemp, "Stub");

    /// <summary>
    /// Tells the stub dll what to do.
    /// </summary>
    internal static readonly string StubCommFilePath = PathCombineFast_NoChecks(StubCommTemp, "al_stub_args.tmp");

    #endregion

    internal static readonly string UpdateTemp = PathCombineFast_NoChecks(BaseTemp, "Update");

    internal static readonly string UpdateBakTemp = PathCombineFast_NoChecks(BaseTemp, "UpdateBak");

    internal static readonly string UpdateAppDownloadTemp = PathCombineFast_NoChecks(BaseTemp, "UpdateDL");

    internal static void CreateOrClearTempPath(TempPath pathType)
    {
        string path = pathType.PathGetter.Invoke();

        if (Directory.Exists(path))
        {
            try
            {
                DirAndFileTree_UnSetReadOnly(path, throwException: true);
            }
            catch (Exception ex)
            {
                Log(ErrorText.Ex + $"setting temp path subtree to all non-readonly.{NL}" +
                    "path was: " + path, ex);
            }

            try
            {
                foreach (string f in FastIO.GetFilesTopOnly(path, "*"))
                {
                    File.Delete(f);
                }
                foreach (string d in FastIO.GetDirsTopOnly(path, "*"))
                {
                    Directory.Delete(d, recursive: true);
                }
            }
            catch (Exception ex)
            {
                Log(ErrorText.Ex + "clearing temp path " + path, ex);
            }
        }
        else
        {
            try
            {
                Directory.CreateDirectory(path);
            }
            catch (Exception ex)
            {
                Log(ErrorText.ExCreate + "temp path " + path, ex);
            }
        }
    }

    #endregion

    #region AngelLoader files

    #region Data folder

    internal static readonly string Data = PathCombineFast_NoChecks(Startup, "Data");

    internal static readonly string Languages = PathCombineFast_NoChecks(Data, "Languages");

    /// <summary>For caching readmes and whatever else we want from non-installed FM archives</summary>
    internal static readonly string FMsCache = PathCombineFast_NoChecks(Data, "FMsCache");

    internal static readonly string ConfigIni = PathCombineFast_NoChecks(Data, "Config.ini");
    internal const string FMDataBakBase = "FMData.bak";
    internal const string FMDataBakNumberedRegexString = "^" + FMDataBakBase + "[0-9]+$";
    internal static readonly string FMDataIni = PathCombineFast_NoChecks(Data, "FMData.ini");

    #endregion

    internal static readonly string UpdateExe = PathCombineFast_NoChecks(Startup, "Update.exe");

    internal static readonly string UpdateExeBak = PathCombineFast_NoChecks(Startup, "Update.exe.bak");

    #region Docs

    internal static readonly string DocFile = PathCombineFast_NoChecks(Startup, "doc", "AngelLoader documentation.html");

    #endregion

    #region Stub

    internal const string StubFileName = "AngelLoader_Stub.dll";

    internal static readonly string StubPath = PathCombineFast_NoChecks(Startup, StubFileName);

    #endregion

    #region FFmpeg

    internal static readonly string FFmpegExe = PathCombineFast_NoChecks(Startup, "ffmpeg", "ffmpeg.exe");

    internal static readonly string FFprobeExe = PathCombineFast_NoChecks(Startup, "ffmpeg", "ffprobe.exe");

    #endregion

    // Use a 64-bit version if possible for even more out-of-memory prevention...
#if X64
    internal static readonly string SevenZipPath = PathCombineFast_NoChecks(Startup, "7z64");
#else
    internal static readonly string SevenZipPath = Environment.Is64BitOperatingSystem
        ? PathCombineFast_NoChecks(Startup, "7z64")
        : PathCombineFast_NoChecks(Startup, "7z32");
#endif

    internal static readonly string SevenZipExe = PathCombineFast_NoChecks(SevenZipPath, "7z.exe");

    #region Log files

    internal static readonly string LogFile = PathCombineFast_NoChecks(Startup, "AngelLoader_log.txt");
    // We only use this to delete it in new versions for cleanliness
    internal static readonly string ScannerLogFile_Old = PathCombineFast_NoChecks(Startup, "FMScanner_log.txt");

    #endregion

    #endregion

    #region Other loaders' files

    internal const string DarkLoaderExe = "DarkLoader.exe";

    // DarkLoader uses this to say whether an FM is installed, and we use it to detect this situation so we
    // can tell the user to go uninstall it in DarkLoader before trying to play
    internal const string DarkLoaderDotCurrent = "DarkLoader.Current";

    internal const string FMSelDll = "fmsel.dll";

    internal const string DarkLoaderIni = "DarkLoader.ini";
    internal const string NewDarkLoaderIni = "NewDarkLoader.ini";
    internal const string FMSelIni = "fmsel.ini";

    // Dirs that go in the installed FMs dir and aren't FMs, so we have to ignore them when finding FMs
    internal const string FMSelCache = ".fmsel.cache";
    internal const string TDMMissionShots = "_missionshots";

    #endregion

    #region FM backup

    internal const string FMBackupSuffix = ".FMSelBak.zip";

    // This is used for excluding save/screenshot backup archives when scanning dirs. Just in case these ever
    // get different extensions, we want to just match the phrase. Probably a YAGNI violation. Meh.
    internal const string FMSelBak = ".FMSelBak.";

    internal const string DarkLoaderSaveBakDir = "DarkLoader";

    internal static readonly string DarkLoaderSaveOrigBakDir = PathCombineFast_NoChecks(DarkLoaderSaveBakDir, "Original");

    internal const string FMSelInf = "fmsel.inf";

    #endregion

    #region Game exes

    internal const string T2MPExe = "Thief2MP.exe";

    #endregion

    internal const string SneakyDll = "Sneaky.dll";

    internal const string MissFlagStr = "missflag.str";

    #region Thief Buddy

    private static readonly object _thiefBuddyLock = new();
    /// <summary>
    /// Constructs and returns the Thief Buddy executable full path from the registry, or returns the empty string
    /// if the Thief Buddy registry entry was not found.
    /// </summary>
    /// <returns></returns>
    internal static string GetThiefBuddyExePath()
    {
        lock (_thiefBuddyLock)
        {
            try
            {
                object? tbInstallLocationKey = Registry.GetValue(
                    keyName: @"HKEY_CURRENT_USER\SOFTWARE\VoiceActorWare\Thief Buddy",
                    valueName: "InstallLocation",
                    defaultValue: -1);

                if (tbInstallLocationKey is string installLocation)
                {
                    return Path.Combine(installLocation, "Thief Buddy", "Thief Buddy.exe");
                }
            }
            catch
            {
                return "";
            }

            return "";
        }
    }

    #endregion

    #region Game config files

    #region NewDark

    internal const string DarkCfg = "dark.cfg";

    internal const string CamCfg = "cam.cfg";
    internal const string CamExtCfg = "cam_ext.cfg";
    internal const string CamModIni = "cam_mod.ini";
#if !ReleaseBeta && !ReleasePublic
    internal const string UserCfg = "user.cfg";
    internal const string UserBnd = "user.bnd";
#endif

    #endregion

    #region Thief 3

    private const string _sneakyOptionsIni = "SneakyOptions.ini";
    // First release version to support portable is 1.1.11, but there was a 1.1.10.519 beta that supported it too
    private static readonly Version _sneakyUpgradeMinimumPortableVersion = new(1, 1, 10, 519);
    // Similar situation with this
    private static readonly Version _sneakyUpgradeMinimumNewStylePortableVersion = new(1, 1, 11, 506);

    internal static (string SoIni, bool IsPortable, bool IsNewStylePortable)
    GetSneakyOptionsIni()
    {
        if (TryGetSneakyOptionsIniFromGameDir(out string soIni, out bool isNewStylePortable))
        {
            return (soIni, true, isNewStylePortable);
        }
        else
        {
            return (GetSneakyOptionsIniFromRegistry(), false, false);
        }
    }

    internal static bool SneakyUpgradeIsPortable() => TryGetSneakyOptionsIniFromGameDir(out _, out _);

    private static bool TryGetSneakyOptionsIniFromGameDir(out string soIni, out bool isNewStylePortable)
    {
        try
        {
            soIni = "";
            isNewStylePortable = false;

            (_, Version? version, _) = Core.GetGameVersion(GameIndex.Thief3);
            if (version != null && version < _sneakyUpgradeMinimumPortableVersion)
            {
                return false;
            }

            // If no version found, carry on under the assumption that something has changed in a new SU version,
            // and assume that if a valid Sneaky.ini is found then we're on a supported version.

            string gamePath = Config.GetGamePath(GameIndex.Thief3);
            if (gamePath.IsWhiteSpace()) return false;

            string expectedSystemDirName = new DirectoryInfo(gamePath).Name;
            if (!expectedSystemDirName.EqualsI("System"))
            {
                Log($"Specified Thief 3 executable is not located in a folder named 'System'. This is unexpected, but continuing.{NL}" +
                    "Thief 3 executable: " + Config.GetGameExe(GameIndex.Thief3) + $"{NL}" +
                    "Thief 3 executable directory full path: " + gamePath + $"{NL}" +
                    "Thief 3 executable directory name: " + expectedSystemDirName);
            }

            string sneakyIni = Path.Combine(gamePath, "Sneaky.ini");

            if (!File.Exists(sneakyIni)) return false;

            if (!TryReadAllLines(sneakyIni, out List<string>? lines))
            {
                return false;
            }

            bool ignoreSaveGameKey = false;

            for (int i = 0; i < lines.Count; i++)
            {
                string lineT = lines[i].Trim();
                if (lineT.EqualsI("[Install]"))
                {
                    while (i < lines.Count - 1)
                    {
                        string lt = lines[i + 1].Trim();
                        if (lt.TryGetValueI("SaveGamePath=", out string saveGamePath))
                        {
                            if (!saveGamePath.IsWhiteSpace())
                            {
                                saveGamePath = RelativeToAbsolute(gamePath, saveGamePath);
                                string si_SoIni = Path.Combine(saveGamePath, "Options", _sneakyOptionsIni);
                                if (File.Exists(si_SoIni))
                                {
                                    soIni = si_SoIni;
                                    isNewStylePortable = true;
                                    return true;
                                }
                            }
                        }
                        else if (lt.TryGetValueI("IgnoreSaveGamePath=", out string ignoreSaveGamePath))
                        {
                            ignoreSaveGameKey = ignoreSaveGamePath.EqualsTrue();
                            break;
                        }
                        else if (lt.IsIniHeader())
                        {
                            break;
                        }
                        i++;
                    }
                    break;
                }
            }

            if (!ignoreSaveGameKey) return false;

            // We just have to assume that one level up is the game root
            string? gameRootPath = Path.GetDirectoryName(gamePath);
            if (gameRootPath.IsWhiteSpace()) return false;

            string finalSoIni = Path.Combine(gameRootPath, "Options", _sneakyOptionsIni);
            if (!File.Exists(finalSoIni)) return false;

            isNewStylePortable = false;
            soIni = finalSoIni;
            return true;
        }
        catch
        {
            soIni = "";
            isNewStylePortable = false;
            return false;
        }
    }

    private static string GetSneakyOptionsIniFromRegistry()
    {
        try
        {
            // Tested on Win7 Ultimate 64 and Win10 Pro 64:
            // Admin and non-Admin accounts can both read this key

            using RegistryKey? hklm = (RegistryKey?)RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32);
            using RegistryKey? tdsKey = hklm?.OpenSubKey(@"Software\Ion Storm\Thief - Deadly Shadows", writable: false);

            if (tdsKey?.GetValue("SaveGamePath", defaultValue: -1) is string regKeyStr)
            {
                string soIni;
                try
                {
                    soIni = Path.Combine(regKeyStr, "Options", _sneakyOptionsIni);
                }
                catch (Exception ex)
                {
                    Log(ErrorText.FoundRegKey + $"it appears to be an invalid path (Path.Combine() failed).{NL}" +
                        ErrorText.RegKeyPath + regKeyStr, ex);
                    return "";
                }

                if (!File.Exists(soIni))
                {
                    Log(ErrorText.FoundRegKey + "couldn't find " + _sneakyOptionsIni + $"{NL}" +
                        ErrorText.RegKeyPath + regKeyStr + $"{NL}" +
                        "Full path was: " + soIni);
                    return "";
                }

                return soIni;
            }
            else
            {
                Log("Couldn't find the registry key that points to Thief: Deadly Shadows options directory (SaveGamePath key)");
                return "";
            }
        }
        catch (SecurityException ex)
        {
            Log(ex.Message, ex);
        }
        catch (IOException ex)
        {
            Log(ex.Message, ex);
        }
        catch (Exception ex)
        {
            Log("Unexpected exception occurred.", ex);
        }

        return "";
    }

    #endregion

    #region Dark Mod

    internal const string TDMCurrentFMFile = "currentfm.txt";
    internal const string MissionsTdmInfo = "missions.tdminfo";

    #endregion

    #endregion

    #region Private methods

    private static string PathCombineFast_NoChecks(string path1, string path2, string path3)
    {
        return PathCombineFast_NoChecks(PathCombineFast_NoChecks(path1, path2), path3);
    }

    private static string PathCombineFast_NoChecks(string path1, string path2)
    {
        int path1Length = path1.Length;
        if (path1Length == 0) return path2;
        char c = path1[path1Length - 1];
        return c == Path.DirectorySeparatorChar ||
               c == Path.AltDirectorySeparatorChar ||
               c == Path.VolumeSeparatorChar
            ? path1 + path2
            : path1 + "\\" + path2;
    }

    #endregion
}
