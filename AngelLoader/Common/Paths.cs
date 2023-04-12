using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using AngelLoader.DataClasses;
using Microsoft.Win32;
using static AL_Common.Common;
using static AL_Common.Logger;

namespace AngelLoader;

internal static class Paths
{
    // Fields that will, or will most likely, be used pretty much right away are initialized normally here.
    // Fields that are likely not to be used right away are lazy-loaded.

    #region Startup path

#if false
    internal const string AppFileName = "AngelLoader.exe";
#endif

    // We use this pulled-out Application.StartupPath code, so we don't rely on the WinForms Application class

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern int GetModuleFileName(HandleRef hModule, StringBuilder buffer, int length);

    private static string GetStartupPath()
    {
        var hModule = new HandleRef(null, IntPtr.Zero);
        var buffer = new StringBuilder(260);
        int num = 1;
        int moduleFileName;
        while ((moduleFileName = GetModuleFileName(hModule, buffer, buffer.Capacity)) == buffer.Capacity &&
               Marshal.GetLastWin32Error() == 122 &&
               buffer.Capacity < (int)short.MaxValue)
        {
            num += 2;
            int capacity = num * 260 < (int)short.MaxValue ? num * 260 : (int)short.MaxValue;
            buffer.EnsureCapacity(capacity);
        }
        buffer.Length = moduleFileName;
        return Path.GetDirectoryName(buffer.ToString())!;
    }

#if DEBUG || Release_Testing
    private static string? _startup;
    internal static string Startup
    {
        get
        {
            if (_startup.IsEmpty())
            {
                try
                {
                    // I like to dev with my normal copy of AngelLoader in C:\AngelLoader (because it has all
                    // my data there), hence I want it running from there when I debug and run from the IDE.
                    // But we don't want other people to have that hardcoded path in here, because if they
                    // have a non-dev AL install in C:\AngelLoader then it will very probably get messed with
                    // if they run or compile the code. So only do the hardcoded path if my personal environment
                    // var is set. Obviously don't add this var yourself.
                    string? val = Environment.GetEnvironmentVariable("AL_FEN_PERSONAL_DEV_3053BA21", EnvironmentVariableTarget.Machine);
                    _startup = val?.EqualsTrue() == true ? @"C:\AngelLoader" : GetStartupPath();
                }
                catch
                {
                    _startup = GetStartupPath();
                }
            }

            return _startup;
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
    private static readonly string _baseTemp = Path.Combine(Path.GetTempPath(), "AngelLoader");

    #region Help

    private static string? _helpTemp;
    internal static string HelpTemp => _helpTemp ??= PathCombineFast_NoChecks(_baseTemp, "Help");

    private static string? _helpRedirectFilePath;
    internal static string HelpRedirectFilePath => _helpRedirectFilePath ??= PathCombineFast_NoChecks(HelpTemp, "redir.html");

    #endregion

    #region Scan

    private static string? _fmScannerTemp;
    internal static string FMScannerTemp => _fmScannerTemp ??= PathCombineFast_NoChecks(_baseTemp, "FMScan");

    private static string? _sevenZipListTemp;
    internal static string SevenZipListTemp => _sevenZipListTemp ??= PathCombineFast_NoChecks(_baseTemp, "7zl");

    #endregion

    #region Stub

    private static string? _stubCommTemp;
    internal static string StubCommTemp => _stubCommTemp ??= PathCombineFast_NoChecks(_baseTemp, "Stub");

    private static string? _stubCommFilePath;
    /// <summary>
    /// Tells the stub dll what to do.
    /// </summary>
    internal static string StubCommFilePath => _stubCommFilePath ??= PathCombineFast_NoChecks(StubCommTemp, "al_stub_args.tmp");

    #endregion

    internal static void CreateOrClearTempPath(string path)
    {
        #region Safety check

        // Make sure we never delete any paths that are not safely tucked in our temp folder
        string baseTemp = _baseTemp.TrimEnd(CA_BS_FS_Space);

        // @DIRSEP: getting rid of this concat is more trouble than it's worth
        // This method is called rarely and only once in a row
        bool pathIsInTempDir = path.PathStartsWithI(baseTemp + "\\");

        Utils.AssertR(pathIsInTempDir, "Path '" + path + "' is not in temp dir '" + baseTemp + "'");

        if (!pathIsInTempDir) return;

        #endregion

        if (Directory.Exists(path))
        {
            try
            {
                DirAndFileTree_UnSetReadOnly(path, throwException: true);
            }
            catch (Exception ex)
            {
                Log(ErrorText.Ex + "setting temp path subtree to all non-readonly.\r\n" +
                    "path was: " + path, ex);
            }

            try
            {
                foreach (string f in FastIO.GetFilesTopOnly(path, "*")) File.Delete(f);
                foreach (string d in FastIO.GetDirsTopOnly(path, "*")) Directory.Delete(d, recursive: true);
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

    private static string? _fmsCache;
    /// <summary>For caching readmes and whatever else we want from non-installed FM archives</summary>
    internal static string FMsCache => _fmsCache ??= PathCombineFast_NoChecks(Data, "FMsCache");

    internal static readonly string ConfigIni = PathCombineFast_NoChecks(Data, "Config.ini");
    internal const string FMDataBakBase = "FMData.bak";
    internal const string FMDataBakNumberedRegexString = "^" + FMDataBakBase + "[0123456789]+$";
    internal static readonly string FMDataIni = PathCombineFast_NoChecks(Data, "FMData.ini");

    #endregion

    #region Docs

    private static string? _docFile;
    internal static string DocFile => _docFile ??= PathCombineFast_NoChecks(Startup, "doc", "AngelLoader documentation.html");

    #endregion

    #region Stub

    internal const string StubFileName = "AngelLoader_Stub.dll";

    private static string? _stubPath;
    internal static string StubPath => _stubPath ??= PathCombineFast_NoChecks(Startup, StubFileName);

    #endregion

    #region FFmpeg

    private static string? _ffmpegExe;
    internal static string FFmpegExe => _ffmpegExe ??= PathCombineFast_NoChecks(Startup, "ffmpeg", "ffmpeg.exe");

    private static string? _ffprobeExe;
    internal static string FFprobeExe => _ffprobeExe ??= PathCombineFast_NoChecks(Startup, "ffmpeg", "ffprobe.exe");

    #endregion

    private static string? _sevenZipPath;
    internal static string SevenZipPath => _sevenZipPath ??=
        // Use a 64-bit version if possible for even more out-of-memory prevention...
        // @X64: If we go x64-only, we can remove the 32-bit 7z and then we need to update this
        Environment.Is64BitOperatingSystem
            ? PathCombineFast_NoChecks(Startup, "7z64")
            : PathCombineFast_NoChecks(Startup, "7z32");

    private static string? _sevenZipExe;
    internal static string SevenZipExe => _sevenZipExe ??= PathCombineFast_NoChecks(SevenZipPath, "7z.exe");

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

    // A dir that goes in the installed FMs dir and isn't an FM, so we have to ignore it when finding FMs
    internal const string FMSelCache = ".fmsel.cache";

    #endregion

    #region FM backup

    internal const string FMBackupSuffix = ".FMSelBak.zip";

    // This is used for excluding save/screenshot backup archives when scanning dirs. Just in case these ever
    // get different extensions, we want to just match the phrase. Probably a YAGNI violation. Meh.
    internal const string FMSelBak = ".FMSelBak.";

    internal const string DarkLoaderSaveBakDir = "DarkLoader";

    private static string? _darkLoaderSaveOrigBakDir;
    internal static string DarkLoaderSaveOrigBakDir => _darkLoaderSaveOrigBakDir ??= PathCombineFast_NoChecks(DarkLoaderSaveBakDir, "Original");

    internal const string FMSelInf = "fmsel.inf";

    #endregion

    #region Game exes

    internal const string T2MPExe = "Thief2MP.exe";

    #endregion

    // Sneaky Upgrade file. We scan this for the SU version number.
    internal const string SneakyDll = "Sneaky.dll";

    internal const string MissFlagStr = "missflag.str";

    #region Thief Buddy

    private static readonly object _thiefBuddyLock = new();
    internal static string ThiefBuddyDefaultExePath
    {
        get
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
    }

    #endregion

    #region Game config files

    internal const string DarkCfg = "dark.cfg";

    internal const string CamCfg = "cam.cfg";
    internal const string CamExtCfg = "cam_ext.cfg";
    internal const string CamModIni = "cam_mod.ini";
    internal const string UserCfg = "user.cfg";

    internal static string GetSneakyOptionsIni()
    {
        try
        {
            // Tested on Win7 Ultimate 64 and Win10 Pro 64:
            // Admin and non-Admin accounts can both read this key

            // @X64 (SneakyOptions.ini reg key)
            // We're not x64 currently, but this check lets us be compatible for an easy switch if we decide
            // to do so in the future.
            object? regKey = Registry.GetValue(
                keyName: !Environment.Is64BitProcess
                    // If we're x86 Win/x86 app OR x64 Win/x86 app, then this is the right path. On x86 Win,
                    // this is the actual registry path, and on x64 Win/x86 app, this will redirect to the
                    // actual path (which is the same except "Wow6432Node\" is inserted after "Software\")
                    ? @"HKEY_LOCAL_MACHINE\Software\Ion Storm\Thief - Deadly Shadows"
                    // If we're x64 Win/x64 app, then \Software WON'T redirect to Software\Wow6432Node, so we
                    // have to do it ourselves.
                    : @"HKEY_LOCAL_MACHINE\Software\Wow6432Node\Ion Storm\Thief - Deadly Shadows",
                valueName: "SaveGamePath",
                defaultValue: -1);

            // Must check for null, because a null return means "path not found", while a default value return
            // means "key name not found". Jank.
            if (regKey is not (null or -1) && regKey is string regKeyStr)
            {
                string soIni;
                try
                {
                    soIni = Path.Combine(regKeyStr, "Options", "SneakyOptions.ini");
                }
                catch (Exception ex)
                {
                    Log("Found the registry key but it appears to be an invalid path (Path.Combine() failed).\r\n" +
                        "Registry key path was: " + regKeyStr, ex);
                    return "";
                }

                if (!File.Exists(soIni))
                {
                    Log("Found the registry key but couldn't find SneakyOptions.ini.\r\n" +
                        "Registry key path was: " + regKeyStr + "\r\n" +
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
            Log("The user does not have the permissions required to read from the registry key.", ex);
        }
        catch (IOException ex)
        {
            Log("The RegistryKey that contains the specified value has been marked for deletion.", ex);
        }
        catch (Exception ex)
        {
            // Shouldn't happen, but it was because of the lack of this catch-all that an unlikely but serious
            // bug used to be possible here.
            Log("Unexpected exception occurred.", ex);
        }

        return "";
    }

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
