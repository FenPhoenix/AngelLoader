using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using AL_Common;
using static AL_Common.Common;
using static AngelLoader.GameSupport;
using static AngelLoader.Global;

namespace AngelLoader;

internal static class TDMParser
{
    internal static List<TDM_LocalFMData> ParseMissionsInfoFile()
    {
        const string missionInfoId = "tdm_missioninfo";
        const string downloaded_version = "\"downloaded_version\"";
        const string last_play_date = "\"last_play_date\"";
        const string mission_completed_0 = "\"mission_completed_0\"";
        const string mission_completed_1 = "\"mission_completed_1\"";
        const string mission_completed_2 = "\"mission_completed_2\"";
        const string mission_loot_collected_0 = "\"mission_loot_collected_0\"";
        const string mission_loot_collected_1 = "\"mission_loot_collected_1\"";
        const string mission_loot_collected_2 = "\"mission_loot_collected_2\"";

        try
        {
            string fmsPath = Config.GetFMInstallPath(GameIndex.TDM);
            string missionsFile = Path.Combine(fmsPath, Paths.MissionsTdmInfo);

            List<TDM_LocalFMData> ret = new();

            List<string> lines = File_ReadAllLines_List(missionsFile);

            for (int i = 0; i < lines.Count; i++)
            {
                string lineT = lines[i].Trim();
                /*
                @TDM(Case-sensitivity/ParseMissionsInfoFile())
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
                        localFMData.MissionCompleted0 = GetValueForKey(entryLineT, mission_completed_0);
                    }
                    else if (entryLineT.StartsWithO(mission_completed_1))
                    {
                        localFMData.MissionCompleted1 = GetValueForKey(entryLineT, mission_completed_1);
                    }
                    else if (entryLineT.StartsWithO(mission_completed_2))
                    {
                        localFMData.MissionCompleted2 = GetValueForKey(entryLineT, mission_completed_2);
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

    internal static async Task<ScannerTDMContext> GetScannerTDMContext()
    {
        List<TDM_LocalFMData> localFMData = ParseMissionsInfoFile();
        (bool success, _, _, List<TDM_ServerFMData> serverFMData) = await TDM_Downloader.TryGetMissionsFromServer();
        return success
            ? new ScannerTDMContext(localFMData, serverFMData)
            : new ScannerTDMContext();
    }
}
