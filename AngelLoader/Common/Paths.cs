﻿using System;
using System.Diagnostics;
using System.IO;
using System.Security;
using AngelLoader.Common.Utility;
using AngelLoader.WinAPI;
using Microsoft.Win32;
using static AngelLoader.Common.Logger;

namespace AngelLoader.Common
{
    internal static class Paths
    {
#if Release_Testing
        internal const string Startup = @"C:\AngelLoader";
#elif Release
        internal static readonly string Startup = System.Windows.Forms.Application.StartupPath;
#else
        internal const string Startup = @"C:\AngelLoader";
#endif

        internal static readonly string LogFile = Path.Combine(Startup, "AngelLoader_log.txt");
        internal static readonly string ScannerLogFile = Path.Combine(Startup, "FMScanner_log.txt");

        #region Temp

        private static readonly string _baseTemp = Path.Combine(Path.GetTempPath(), "AngelLoader");

        internal static readonly string FMScannerTemp = Path.Combine(_baseTemp, "FMScan");

        internal static readonly string StubCommTemp = Path.Combine(_baseTemp, "Stub");

        /// <summary>
        /// Tells the stub dll what to do.
        /// </summary>
        internal static readonly string StubCommFilePath = Path.Combine(StubCommTemp, "al_stub_args.tmp");

        #endregion

        internal static string GetSneakyOptionsIni()
        {
            try
            {
                // Tested on Win7 Ultimate 64: Admin and non-Admin accounts can both read this key
                // TODO: Test on Win10
                var regKey = Registry.GetValue(
                    @"HKEY_LOCAL_MACHINE\Software\Ion Storm\Thief - Deadly Shadows", "SaveGamePath", -1);

                // Must check for null, because a null return means "path not found", while a default value return
                // means "key name not found". Jank.
                if (regKey == null || regKey is int regKeyDefault && regKeyDefault == -1 || !(regKey is string))
                {
                    Log("Couldn't find the registry key that points to Thief: Deadly Shadows options directory (SaveGamePath key)");
                    return null;
                }
                else
                {
                    var regKeyStr = regKey.ToString();
                    string soIni = null;
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
                        return null;
                    }
                    if (!File.Exists(soIni))
                    {
                        Log("Found the registry key but couldn't find SneakyOptions.ini.\r\n" +
                            "Registry key path was: " + regKeyStr + "\r\n" +
                            "Full path was: " + soIni);
                        return null;
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

            return null;
        }

        internal const string StubFileName = "AngelLoader_Stub.dll";

        internal const string FMBackupSuffix = ".FMSelBak.zip";

        // This is used for excluding save/screenshot backup archives when scanning dirs. Just in case these ever
        // get different extensions, we want to just match the phrase. Probably a YAGNI violation. Meh.
        internal const string FMSelBak = ".FMSelBak.";

        internal const string FMSelInf = "fmsel.inf";

        internal const string DromEdExe = "DromEd.exe";

        internal const string T2MPExe = "Thief2MP.exe";

        internal const string DarkLoaderSaveBakDir = "DarkLoader";

        internal const string DarkLoaderIni = "DarkLoader.ini";

        internal static readonly string DarkLoaderSaveOrigBakDir = Path.Combine(DarkLoaderSaveBakDir, "Original");

        internal static readonly string Data = Path.Combine(Startup, "Data");

        internal static readonly string Languages = Path.Combine(Data, "Languages");

        /// <summary>
        /// For caching readmes and whatever else we want from non-installed FM archives
        /// </summary>
        internal static readonly string FMsCache = Path.Combine(Data, "FMsCache");

        internal static readonly string ConfigIni = Path.Combine(Data, "Config.ini");
        internal static readonly string FMDataIni = Path.Combine(Data, "FMData.ini");

        internal static readonly string FFmpegExe = Path.Combine(Startup, "ffmpeg", "ffmpeg.exe");
        internal static readonly string FFprobeExe = Path.Combine(Startup, "ffmpeg", "ffprobe.exe");

        internal const string T3ReadmeDir1 = "Fan Mission Extras";
        internal const string T3ReadmeDir2 = "FanMissionExtras";

        #region Methods

        internal static void PrepareTempPath(string path)
        {
            #region Safety check

            // Make sure we never delete any paths that are not safely tucked in our temp folder
            var baseTemp = _baseTemp.TrimEnd('\\', '/', ' ');

            if (!path.StartsWithI(baseTemp + '\\') && !path.StartsWithI(baseTemp + '/')) return;

            #endregion

            try
            {
                if (Directory.Exists(path))
                {
                    foreach (var f in FastIO.GetFilesTopOnly(path, "*"))
                    {
                        File.Delete(f);
                    }

                    foreach (var d in Directory.EnumerateDirectories(path, "*", SearchOption.TopDirectoryOnly))
                    {
                        Directory.Delete(d, recursive: true);
                    }
                }
                else
                {
                    Directory.CreateDirectory(path);
                }
            }
            catch (Exception ex)
            {
                Log("Exception clearing temp path " + path, ex);
            }
        }

        internal static string RelativeToAbsolute(string basePath, string relativePath)
        {
            Debug.Assert(!basePath.IsEmpty(), "basePath is null or empty");

            return relativePath.IsEmpty() ? basePath : Path.GetFullPath(Path.Combine(basePath, relativePath));
        }

        #endregion
    }
}
