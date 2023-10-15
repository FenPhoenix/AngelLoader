using System.Collections.Generic;
using System.IO;
using AL_Common;
using static AL_Common.Common;
using static AngelLoader.GameSupport;

namespace AngelLoader;

internal static class TDMParser
{
    internal static List<MissionInfoEntry> ParseMissionsInfoFile()
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
            string fmsPath = Global.Config.GetFMInstallPath(GameIndex.TDM);
            string missionsFile = Path.Combine(fmsPath, Paths.MissionsTdmInfo);

            List<MissionInfoEntry> ret = new();

            List<string> lines = File_ReadAllLines_List(missionsFile);

            for (int i = 0; i < lines.Count; i++)
            {
                string lineT = lines[i].Trim();
                if (!lineT.StartsWithPlusWhiteSpace(missionInfoId)) continue;

                string fmName = lineT.Substring(missionInfoId.Length).Trim();
                if (fmName.IsEmpty())
                {
                    SkipEntry(lines, ref i);
                    continue;
                }

                SkipToEntryData(lines, ref i);

                var missionInfo = new MissionInfoEntry(fmName);

                while (i < lines.Count - 1)
                {
                    string entryLineT = lines[i + 1].Trim();
                    if (entryLineT.StartsWithO(downloaded_version))
                    {
                        missionInfo.DownloadedVersion = GetValueForKey(entryLineT, downloaded_version);
                    }
                    else if (entryLineT.StartsWithO(last_play_date))
                    {
                        missionInfo.LastPlayDate = GetValueForKey(entryLineT, last_play_date);
                    }

                    else if (entryLineT.StartsWithO(mission_completed_0))
                    {
                        missionInfo.MissionCompleted0 = GetValueForKey(entryLineT, mission_completed_0);
                    }
                    else if (entryLineT.StartsWithO(mission_completed_1))
                    {
                        missionInfo.MissionCompleted1 = GetValueForKey(entryLineT, mission_completed_1);
                    }
                    else if (entryLineT.StartsWithO(mission_completed_2))
                    {
                        missionInfo.MissionCompleted2 = GetValueForKey(entryLineT, mission_completed_2);
                    }

                    else if (entryLineT.StartsWithO(mission_loot_collected_0))
                    {
                        missionInfo.MissionLootCollected0 = GetValueForKey(entryLineT, mission_loot_collected_0);
                    }
                    else if (entryLineT.StartsWithO(mission_loot_collected_1))
                    {
                        missionInfo.MissionLootCollected1 = GetValueForKey(entryLineT, mission_loot_collected_1);
                    }
                    else if (entryLineT.StartsWithO(mission_loot_collected_2))
                    {
                        missionInfo.MissionLootCollected2 = GetValueForKey(entryLineT, mission_loot_collected_2);
                    }

                    else if (entryLineT == "}")
                    {
                        break;
                    }
                    i++;
                }

                ret.Add(missionInfo);
            }

            return ret;
        }
        catch
        {
            return new List<MissionInfoEntry>();
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
}
