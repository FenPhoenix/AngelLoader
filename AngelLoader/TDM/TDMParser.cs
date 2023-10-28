using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using AL_Common;
using static AL_Common.Common;
using static AngelLoader.GameSupport;
using static AngelLoader.Global;

namespace AngelLoader;

internal static class TDMParser
{
    // Works fine when missions.tdminfo doesn't exist, just returns an empty list.
    internal static List<TDM_LocalFMData> ParseMissionsInfoFile()
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
}
