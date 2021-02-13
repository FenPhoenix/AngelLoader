﻿using System;
using System.IO;
using System.Security;
using AngelLoader.WinAPI;
using Microsoft.Win32;
using static AngelLoader.Logger;

namespace AngelLoader
{
    internal static class Paths
    {
        // Fields that will, or will most likely, be used pretty much right away are initialized normally here.
        // Fields that are likely not to be used right away are lazy-loaded.

        #region Startup path

#if Release_Testing
        internal const string Startup = @"C:\AngelLoader";
#elif Release
        internal static readonly string Startup = System.Windows.Forms.Application.StartupPath;
#else
        internal const string Startup = @"C:\AngelLoader";
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
            string baseTemp = _baseTemp.TrimEnd(Misc.CA_BS_FS_Space);

            // @DIRSEP: getting rid of this concat is more trouble than it's worth
            // This method is called rarely and only once in a row
            if (!path.PathStartsWithI(baseTemp + "\\")) return;

            #endregion

            if (Directory.Exists(path))
            {
                try
                {
                    foreach (string f in FastIO.GetFilesTopOnly(path, "*")) File.Delete(f);
                    foreach (string d in FastIO.GetDirsTopOnly(path, "*")) Directory.Delete(d, recursive: true);
                }
                catch (Exception ex)
                {
                    Log("Exception clearing temp path " + path, ex);
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
                    Log("Exception creating temp path " + path, ex);
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

        // Use a 64-bit version if possible for even more out-of-memory prevention...
        internal static readonly string SevenZipPath = Environment.Is64BitOperatingSystem
            ? PathCombineFast_NoChecks(Startup, "7z64")
            : PathCombineFast_NoChecks(Startup, "7z32");

        internal static string SevenZipExe => PathCombineFast_NoChecks(SevenZipPath, "7z.exe");

        #region Log files

        internal static readonly string LogFile = PathCombineFast_NoChecks(Startup, "AngelLoader_log.txt");
        internal static readonly string ScannerLogFile = PathCombineFast_NoChecks(Startup, "FMScanner_log.txt");

        #endregion

        #endregion

        #region Other loaders' files

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

        internal const string DromEdExe = "DromEd.exe";
        internal const string ShockEdExe = "ShockEd.exe";

        internal const string T2MPExe = "Thief2MP.exe";

        #endregion

        // Sneaky Upgrade file. We scan this for the SU version number.
        internal const string SneakyDll = "Sneaky.dll";

        #region Game config files

        internal const string CamCfg = "cam.cfg";
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
                    !Environment.Is64BitProcess
                        // If we're x86 Win/x86 app OR x64 Win/x86 app, then this is the right path. On x86 Win,
                        // this is the actual registry path, and on x64 Win/x86 app, this will redirect to the
                        // actual path (which is the same except "Wow6432Node\" is inserted after "Software\")
                        ? @"HKEY_LOCAL_MACHINE\Software\Ion Storm\Thief - Deadly Shadows"
                        // If we're x64 Win/x64 app, then \Software WON'T redirect to Software\Wow6432Node, so we
                        // have to do it ourselves.
                        : @"HKEY_LOCAL_MACHINE\Software\Wow6432Node\Ion Storm\Thief - Deadly Shadows", "SaveGamePath", -1);

                // Must check for null, because a null return means "path not found", while a default value return
                // means "key name not found". Jank.
                if (regKey == null || (regKey is int regKeyDefault && regKeyDefault == -1) || !(regKey is string))
                {
                    Log("Couldn't find the registry key that points to Thief: Deadly Shadows options directory (SaveGamePath key)");
                    return "";
                }
                else
                {
                    // @NULL_TODO: Test with like !(regKey is string regKeyStr) up there
                    string regKeyStr = regKey.ToString()!;
                    string soIni = "";
                    try
                    {
                        soIni = Path.Combine(regKeyStr, "Options", "SneakyOptions.ini");
                    }
                    catch (Exception ex)
                    {
                        Log("Found the registry key but couldn't find SneakyOptions.ini.\r\n" +
                            "Additionally, it seems the registry key's value contained invalid path characters " +
                            "or was otherwise not a valid path (Path.Combine() failed).\r\n" +
                            "Registry key path was: " + regKeyStr + "\r\n" +
                            "Full path was: " + soIni, ex);
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
                Log("Unexpected exception occurred in " + nameof(GetSneakyOptionsIni), ex);
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
}
