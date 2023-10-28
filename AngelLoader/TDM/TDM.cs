using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
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
                List<string> pk4Files = FastIO.GetFilesTopOnly(tdmFMsPath, "*.pk4", returnFullPaths: false);
                foreach (string pk4File in pk4Files)
                {
                    pk4ConvertedNamesDict[pk4File.ConvertToValidTDMInternalName()] = pk4File;
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
        Core.View.RefreshFMsListRowsOnlyKeepSelection();
        FanMission? fm = Core.View.GetMainSelectedFMOrNull();
        if (fm != null)
        {
            Core.View.UpdateAllFMUIDataExceptReadme(fm);
        }
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
            while (!TryGetLines(file, out lines))
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
                // @TDM(Case-sensitivity/UpdateTDMInstalledFMStatus): Case-sensitive compare
                // Case-sensitive compare of the dir name from currentfm.txt and the dir name from our
                // list.
                fm.Installed = fmName != null && !fm.MarkedUnavailable && fm.TDMInstalledDir == fmName;
            }
        }

        return;

        static bool TryGetLines(string file, [NotNullWhen(true)] out List<string>? lines)
        {
            try
            {
                lines = File_ReadAllLines_List(file);
                return true;
            }
            catch
            {
                lines = null;
                return false;
            }
        }
    }

    internal static void SetTDMMissionInfoData()
    {
        if (Config.GetFMInstallPath(GameIndex.TDM).IsEmpty()) return;

        List<TDM_LocalFMData> localFMDataList = TDM.ParseMissionsInfoFile();
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
                    fm.FinishedOn |= (int)Difficulty.Normal;
                }
                if (localData.MissionCompletedOnHard)
                {
                    fm.FinishedOn |= (int)Difficulty.Hard;
                }
                if (localData.MissionCompletedOnExpert)
                {
                    fm.FinishedOn |= (int)Difficulty.Expert;
                }

                // Only add last played date if there is none (we set date on play, and ours is more granular)
                if (!localData.LastPlayDate.IsEmpty() &&
                    fm.LastPlayed.DateTime == null &&
                    TryParseTDMDate(localData.LastPlayDate, out DateTime result))
                {
                    fm.LastPlayed.DateTime = result;
                }

                if (int.TryParse(localData.DownloadedVersion, out int version))
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
            HashSetI dirsHash = fileTdmFMIds_Dirs.ToHashSetI();

            var finalFilesList = new List<string>(fileTdmFMIds_Dirs.Count + fileTdmFMIds_PK4s.Count);

            finalFilesList.AddRange(fileTdmFMIds_Dirs);

            for (int i = 0; i < fileTdmFMIds_PK4s.Count; i++)
            {
                string nameWithoutExt = fileTdmFMIds_PK4s[i].ConvertToValidTDMInternalName();
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
            if (!internalTdmFMIds.SequenceEqual(finalFilesList))
            {
                return true;
            }

            List<TDM_LocalFMData> localDataList = TDM.ParseMissionsInfoFile();
            // @TDM: Case-sensitive dictionary
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
                    int.TryParse(localData.DownloadedVersion, out int version) &&
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
}
