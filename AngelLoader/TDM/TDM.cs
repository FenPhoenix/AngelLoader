using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using AL_Common;
using AngelLoader.DataClasses;
using static AL_Common.Common;
using static AngelLoader.GameSupport;
using static AngelLoader.Global;
using static AngelLoader.Utils;

namespace AngelLoader;

internal static class TDM
{
    // Works fine when missions.tdminfo doesn't exist, just returns an empty list.
    private static List<TDM_LocalFMData> ParseMissionsInfoFile()
    {
        const string missionInfoId = "tdm_missioninfo";
        const string downloaded_version = "\"downloaded_version\"";
        const string last_play_date = "\"last_play_date\"";
        const string mission_completed_0 = "\"mission_completed_0\"";
        const string mission_completed_1 = "\"mission_completed_1\"";
        const string mission_completed_2 = "\"mission_completed_2\"";

        try
        {
            string fmsPath = Config.GetFMInstallPath(GameIndex.TDM);

            if (fmsPath.IsEmpty())
            {
                return new List<TDM_LocalFMData>();
            }

            string missionsFile = Path.Combine(fmsPath, Paths.MissionsTdmInfo);

            List<string>? lines = null;
            using (var cts = new CancellationTokenSource(5000))
            {
                bool timedOut;
                while (!(timedOut = cts.IsCancellationRequested))
                {
                    try
                    {
                        lines = File_ReadAllLines_List(missionsFile);
                        break;
                    }
                    catch (DirectoryNotFoundException)
                    {
                        return new List<TDM_LocalFMData>();
                    }
                    catch (FileNotFoundException)
                    {
                        return new List<TDM_LocalFMData>();
                    }
                    catch (IOException)
                    {
                        Thread.Sleep(50);
                    }
                    catch (Exception)
                    {
                        return new List<TDM_LocalFMData>();
                    }
                }

                if (timedOut)
                {
                    return new List<TDM_LocalFMData>();
                }
            }

            if (lines == null) return new List<TDM_LocalFMData>();

            List<TDM_LocalFMData> ret = new();

            for (int i = 0; i < lines.Count; i++)
            {
                string lineT = lines[i].Trim();
                /*
                @TDM_CASE(ParseMissionsInfoFile())
                TDM is case-sensitive with these. If you rename say "iris" to "iriS", it will add another
                entry for "iriS" even if "iris" already exists, and "iriS" will be empty to start with.
                And when it reads the file, it's case-sensitive too, so if the dir "iriS" exists and "iriS"
                exists in the file, it will take that entry and not display the "iris" stats. Otherwise if
                the "iris" dir exists then it will skip "iriS" in the file and take "iris".
                */
                if (!lineT.StartsWithPlusWhiteSpace(missionInfoId)) continue;

                string fmName = lineT.Substring(missionInfoId.Length).Trim();
                if (fmName.IsEmpty())
                {
                    SkipEntry(lines, ref i);
                    continue;
                }

                SkipToEntryData(lines, ref i);

                TDM_LocalFMData localFMData = new(fmName);

                while (i < lines.Count - 1)
                {
                    string entryLineT = lines[i + 1].Trim();
                    if (entryLineT.StartsWithO(downloaded_version))
                    {
                        localFMData.DownloadedVersion = GetValueForKey(entryLineT, downloaded_version);
                    }
                    else if (entryLineT.StartsWithO(last_play_date))
                    {
                        localFMData.LastPlayDate = GetValueForKey(entryLineT, last_play_date);
                    }

                    else if (entryLineT.StartsWithO(mission_completed_0))
                    {
                        localFMData.MissionCompletedOnNormal = GetValueForKey(entryLineT, mission_completed_0) == "1";
                    }
                    else if (entryLineT.StartsWithO(mission_completed_1))
                    {
                        localFMData.MissionCompletedOnHard = GetValueForKey(entryLineT, mission_completed_1) == "1";
                    }
                    else if (entryLineT.StartsWithO(mission_completed_2))
                    {
                        localFMData.MissionCompletedOnExpert = GetValueForKey(entryLineT, mission_completed_2) == "1";
                    }

                    else if (entryLineT == "}")
                    {
                        break;
                    }
                    i++;
                }

                ret.Add(localFMData);
            }

            return ret;
        }
        catch
        {
            return new List<TDM_LocalFMData>();
        }

        static string GetValueForKey(string line, string key)
        {
            return line.Substring(key.Length).Trim().Trim(CA_DoubleQuote);
        }

        static void SkipEntry(List<string> lines, ref int i)
        {
            for (; i < lines.Count; i++)
            {
                if (lines[i].Trim() == "}")
                {
                    return;
                }
            }
        }

        static void SkipToEntryData(List<string> lines, ref int i)
        {
            for (; i < lines.Count; i++)
            {
                if (lines[i].Trim() == "{")
                {
                    return;
                }
            }
        }
    }

    internal static async Task<ScannerTDMContext> GetScannerTDMContext(CancellationToken cancellationToken)
    {
        List<TDM_LocalFMData> localFMData = ParseMissionsInfoFile();
        string fmsPath = Config.GetFMInstallPath(GameIndex.TDM);
        DictionaryI<string> pk4ConvertedNamesDict = GetTDMBaseFMsDirPK4sConverted();
        (bool success, _, _, List<TDM_ServerFMData> serverFMData) = await TDM_Downloader.TryGetMissionsFromServer(cancellationToken);
        return success
            ? new ScannerTDMContext(fmsPath, pk4ConvertedNamesDict, localFMData, serverFMData)
            : new ScannerTDMContext(fmsPath);
    }

    internal static DictionaryI<string> GetTDMBaseFMsDirPK4sConverted()
    {
        try
        {
            string tdmFMsPath = Config.GetFMInstallPath(GameIndex.TDM);
            // @TDM_CASE: Case-insensitive dictionary
            DictionaryI<string> pk4ConvertedNamesDict = new();
            if (!tdmFMsPath.IsEmpty())
            {
                // Matching game behavior - it supports zips in the base FMs dir, but renames them to pk4 once it
                // moves them to a individual folders, and doesn't support zips inside those. Zips override pk4s.
                List<string> pk4Files = FastIO.GetFilesTopOnly(tdmFMsPath, "*.pk4", returnFullPaths: false);
                List<string> zipFiles = FastIO.GetFilesTopOnly(tdmFMsPath, "*.zip", returnFullPaths: false);
                foreach (string pk4File in pk4Files)
                {
                    pk4ConvertedNamesDict[pk4File.ConvertToValidTDMInternalName(extension: ".pk4")] = pk4File;
                }
                foreach (string zipFile in zipFiles)
                {
                    pk4ConvertedNamesDict[zipFile.ConvertToValidTDMInternalName(extension: ".zip")] = zipFile;
                }
            }
            return pk4ConvertedNamesDict;
        }
        catch
        {
            return new DictionaryI<string>();
        }
    }

    internal static void ViewRefreshAfterAutoUpdate()
    {
        /*
        @TDM(currentfm.txt refresh minimal required work):
        Technically current fm only needs the FMs list refresh and multiselect state update. We could technically
        improve perf by just doing those for current fm change. But, we measure basically no difference whatsoever
        between this full refresh vs. just an FMs list refresh, even with all tabs loaded and updating for real.
        The FMs DataGridView takes like 13ms to draw the un-maximized window's worth of cells, even with no logic
        and most cells just blank or constant strings. Doesn't seem to be anything we can do about that, so meh.
        The rest of the time beyond that is just noise.
        */
        Core.View.RefreshAllSelectedFMs_Full();
        Core.View.SetAvailableAndFinishedFMCount();
    }

    internal static void UpdateTDMDataFromDisk(bool refresh)
    {
        SetTDMMissionInfoData();
        UpdateTDMInstalledFMStatus();
        if (refresh)
        {
            ViewRefreshAfterAutoUpdate();
        }
    }

    internal static void UpdateTDMInstalledFMStatus()
    {
        string? fmName = null;
        string file;
        string fmInstallPath = Config.GetGamePath(GameIndex.TDM);
        if (!fmInstallPath.IsEmpty())
        {
            try
            {
                file = Path.Combine(fmInstallPath, Paths.TDMCurrentFMFile);
            }
            catch
            {
                file = "";
                fmName = null;
            }
        }
        else
        {
            file = "";
            fmName = null;
        }

        if (file.IsEmpty())
        {
            foreach (FanMission fm in FMDataIniListTDM)
            {
                fm.Installed = false;
            }
        }
        else if (File.Exists(file))
        {
            using var cts = new CancellationTokenSource(5000);

            List<string>? lines;
            while (!TryReadAllLines(file, out lines, log: false))
            {
                Thread.Sleep(50);

                if (cts.IsCancellationRequested)
                {
                    return;
                }
            }

            if (lines.Count > 0)
            {
                fmName = lines[0];
            }

            foreach (FanMission fm in FMDataIniListTDM)
            {
                // @TDM_CASE(Case-sensitivity/UpdateTDMInstalledFMStatus): Case-sensitive compare
                // Case-sensitive compare of the dir name from currentfm.txt and the dir name from our
                // list.
                fm.Installed = fmName != null && !fm.MarkedUnavailable && fm.TDMInstalledDir == fmName;
            }
        }
    }

    internal static void SetTDMMissionInfoData()
    {
        if (Config.GetFMInstallPath(GameIndex.TDM).IsEmpty()) return;

        List<TDM_LocalFMData> localFMDataList = ParseMissionsInfoFile();
        // @TDM_CASE: Case sensitive dictionary
        var tdmFMsDict = new Dictionary<string, FanMission>(FMDataIniListTDM.Count);
        foreach (FanMission fm in FMDataIniListTDM)
        {
            tdmFMsDict[fm.TDMInstalledDir] = fm;
        }

        foreach (TDM_LocalFMData localData in localFMDataList)
        {
            if (tdmFMsDict.TryGetValue(localData.InternalName, out FanMission fm))
            {
                // Only add, don't remove any the user has set manually
                if (localData.MissionCompletedOnNormal)
                {
                    fm.FinishedOn |= (uint)Difficulty.Normal;
                }
                if (localData.MissionCompletedOnHard)
                {
                    fm.FinishedOn |= (uint)Difficulty.Hard;
                }
                if (localData.MissionCompletedOnExpert)
                {
                    fm.FinishedOn |= (uint)Difficulty.Expert;
                }

                // Only add last played date if there is none (we set date on play, and ours is more granular)
                if (!localData.LastPlayDate.IsEmpty() &&
                    fm.LastPlayed.DateTime == null &&
                    TryParseTDMDate(localData.LastPlayDate, out DateTime result))
                {
                    fm.LastPlayed.DateTime = result;
                }

                if (Int_TryParseInv(localData.DownloadedVersion, out int version))
                {
                    fm.TDMVersion = version;
                }
            }
        }
    }

    internal static bool TdmFMSetChanged()
    {
        string fmsPath = Config.GetFMInstallPath(GameIndex.TDM);
        if (fmsPath.IsEmpty()) return false;

        try
        {
            List<string> internalTdmFMIds = new(FMDataIniListTDM.Count);
            foreach (FanMission fm in FMDataIniListTDM)
            {
                if (!fm.MarkedUnavailable)
                {
                    internalTdmFMIds.Add(fm.TDMInstalledDir);
                }
            }

            List<string> fileTdmFMIds_Dirs = FastIO.GetDirsTopOnly(fmsPath, "*", returnFullPaths: false);
            List<string> fileTdmFMIds_PK4s = FastIO.GetFilesTopOnly(fmsPath, "*.pk4", returnFullPaths: false);
            List<string> fileTdmFMIds_Zips = FastIO.GetFilesTopOnly(fmsPath, "*.zip", returnFullPaths: false);
            HashSetI dirsHash = fileTdmFMIds_Dirs.ToHashSetI();

            var finalFilesList = new List<string>(fileTdmFMIds_Dirs.Count + fileTdmFMIds_PK4s.Count + fileTdmFMIds_Zips.Count);

            finalFilesList.AddRange(fileTdmFMIds_Dirs);

            for (int i = 0; i < fileTdmFMIds_PK4s.Count; i++)
            {
                string nameWithoutExt = fileTdmFMIds_PK4s[i].ConvertToValidTDMInternalName(".pk4");
                if (dirsHash.Add(nameWithoutExt))
                {
                    finalFilesList.Add(nameWithoutExt);
                }
            }

            for (int i = 0; i < fileTdmFMIds_Zips.Count; i++)
            {
                string nameWithoutExt = fileTdmFMIds_Zips[i].ConvertToValidTDMInternalName(".zip");
                if (dirsHash.Add(nameWithoutExt))
                {
                    finalFilesList.Add(nameWithoutExt);
                }
            }

            for (int i = 0; i < finalFilesList.Count; i++)
            {
                if (!IsValidTdmFM(fmsPath, finalFilesList[i]))
                {
                    finalFilesList.RemoveAt(i);
                    break;
                }
            }

            internalTdmFMIds.Sort();
            finalFilesList.Sort();

            // @TDM_CASE: Case-sensitive comparison
            if (!internalTdmFMIds.SequenceEqual(finalFilesList, StringComparer.Ordinal))
            {
                return true;
            }

            List<TDM_LocalFMData> localDataList = ParseMissionsInfoFile();
            // @TDM_CASE: Case-sensitive dictionary
            var internalTDMDict = new Dictionary<string, FanMission>(FMDataIniListTDM.Count);
            foreach (FanMission fm in FMDataIniListTDM)
            {
                if (!fm.MarkedUnavailable)
                {
                    internalTDMDict[fm.TDMInstalledDir] = fm;
                }
            }

            foreach (TDM_LocalFMData localData in localDataList)
            {
                if (internalTDMDict.TryGetValue(localData.InternalName, out FanMission fm) &&
                    Int_TryParseInv(localData.DownloadedVersion, out int version) &&
                    fm.TDMVersion != version)
                {
                    return true;
                }
            }

            return false;
        }
        catch
        {
            return false;
        }
    }

    internal static bool IsValidTdmFM(string fmsPath, string fileOrDirName)
    {
        /*
        The Dark Mod Wiki also mentions _i18n.pk4 files, but the game (as of v2.11 anyway) doesn't seem to do
        anything with these, not putting them automatically into the matching game dir or anything. So it could
        be these are deprecated file names, but regardless, it looks like we can ignore them.
        */
        // @TDM_CASE
        if (fileOrDirName.EqualsI(Paths.TDMMissionShots) ||
            fileOrDirName.EndsWithI("_l10n") ||
            fileOrDirName.EndsWithI("_l10n.pk4"))
        {
            return false;
        }

        try
        {
            string fullDir = Path.Combine(fmsPath, fileOrDirName);
            if (Directory.Exists(fullDir))
            {
                List<string> pk4Files = FastIO.GetFilesTopOnly(fullDir, "*.pk4", preallocate: 1);
                if (pk4Files.Count == 0)
                {
                    return false;
                }
            }
        }
        catch
        {
            return false;
        }

        return true;
    }

    /*
    @TDM_NOTE(exe version not as consistent/reliable as you might necessarily want)
    For some reason the TDM file version ends up being "DM_VERSION_COMPLETE" (?!)
    The TDM version is like 2.0.11.0 for 2.11, but 2.0.7.0 for 2.07
    Also, TDM <2.07 has just 1.0.0.1 as both file and product version in the exe.
    So do some whatever heuristics and try to get something reasonable. If we can't, oh well.
    But we can get the version from lastinstall.ini in the form of like "release211".
    */
    internal static string GetTDMVersion(FileVersionInfo vi, Version? version)
    {
        string gamePath = Config.GetGamePath(GameIndex.TDM);
        if (gamePath.IsEmpty()) return GetFileVersion(vi, version);

        try
        {
            string lastInstallIni = Path.Combine(gamePath, ".zipsync", "lastinstall.ini");
            using var sr = new StreamReader(lastInstallIni, Encoding.UTF8);

            bool inVersion = false;
            while (sr.ReadLine() is { } line)
            {
                string lineT = line.Trim();
                if (inVersion &&
                    lineT.StartsWithFast("version="))
                {
                    string versionString = lineT.Substring(lineT.IndexOf('=') + 1).Trim();
                    Match match = Regex.Match(versionString, "(?<VersionRaw>[0-9]+)");
                    if (match.Success)
                    {
                        string versionRaw = match.Groups["VersionRaw"].Value;
                        if (versionRaw.Length >= 2)
                        {
                            versionRaw = versionRaw.Insert(1, ".");
                            return versionRaw;
                        }
                    }
                    else
                    {
                        return GetFileVersion(vi, version);
                    }
                }
                else if (lineT == "[Version]")
                {
                    inVersion = true;
                }
                else if (!lineT.IsEmpty() && lineT[0] == '[')
                {
                    inVersion = false;
                }
            }
        }
        catch
        {
            return GetFileVersion(vi, version);
        }

        return GetFileVersion(vi, version);

        static string GetFileVersion(FileVersionInfo vi, Version? version)
        {
            int majorPart = vi.FileMajorPart;
            int minorPart = vi.FileMinorPart;
            int buildPart = vi.FileBuildPart;
            int privatePart = vi.FilePrivatePart;

            return majorPart > 0 && minorPart == 0 && privatePart == 0
                ? majorPart.ToStrInv() + "." + (buildPart > 9 ? buildPart.ToStrInv() : ".0" + buildPart.ToStrInv())
                : version?.ToString() ?? "";
        }
    }
}
