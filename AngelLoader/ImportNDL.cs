using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using AngelLoader.Common.DataClasses;
using AngelLoader.Common.Utility;
using AngelLoader.Ini;
using static AngelLoader.Ini.Ini;

namespace AngelLoader
{
    internal static class ImportNDL
    {
        internal static async Task<(bool Success, List<FanMission> FMs)>
        Import(string iniFile)
        {
            var lines = await Task.Run(() => File.ReadAllLines(iniFile));
            var fms = new List<FanMission>();

            await Task.Run(() =>
            {
                for (int i = 0; i < lines.Length; i++)
                {
                    var line = lines[i];

                    if (line.Length >= 5 && line[0] == '[' && line[1] == 'F' && line[2] == 'M' && line[3] == '=')
                    {
                        // NOTE: There can be a problem like:
                        // installed name is CoolMission[1]
                        // it gets written like [FM=CoolMission[1]]
                        // it gets read and all [ and ] chars are removed
                        // it gets written back out like [FM=CoolMission1]
                        // Rare I guess, so just ignore?
                        var instName = line.Substring(1, line.Length - 2);

                        var fm = new FanMission { InstalledDir = instName };

                        while (i < lines.Length - 1)
                        {
                            var lineFM = lines[i + 1];
                            if (lineFM.StartsWithFast_NoNullChecks("NiceName="))
                            {
                                fm.Title = lineFM.Substring(9);
                            }
                            else if (lineFM.StartsWithFast_NoNullChecks("ReleaseDate="))
                            {
                                fm.ReleaseDate = ReadNullableDate(lineFM.Substring(12));
                            }
                            else if (lineFM.StartsWithFast_NoNullChecks("LastCompleted="))
                            {
                                fm.LastPlayed = ReadNullableDate(lineFM.Substring(14));
                            }
                            else if (lineFM.StartsWithFast_NoNullChecks("Finished="))
                            {
                                int.TryParse(lineFM.Substring(9), out int result);
                                // result will be 0 on fail, which is the empty value so it's fine
                                fm.FinishedOn = result;
                            }
                            else if (lineFM.StartsWithFast_NoNullChecks("Rating="))
                            {
                                fm.Rating = int.TryParse(lineFM.Substring(7), out int result) ? result : -1;
                            }
                            else if (lineFM.StartsWithFast_NoNullChecks("Comment="))
                            {
                                fm.Comment = lineFM.Substring(8);
                            }
                            else if (lineFM.StartsWithFast_NoNullChecks("ModExclude="))
                            {
                                var val = lineFM.Substring(11);
                                if (val == "*")
                                {
                                    fm.DisableAllMods = true;
                                }
                                else
                                {
                                    fm.DisabledMods = val;
                                }
                            }
                            else if (lineFM.StartsWithFast_NoNullChecks("Tags="))
                            {
                                fm.TagsString = lineFM.Substring(5);
                            }
                            else if (lineFM.StartsWithFast_NoNullChecks("InfoFile="))
                            {
                                fm.SelectedReadme = lineFM.Substring(9);
                            }
                            else if (lineFM.StartsWithFast_NoNullChecks("FMSize="))
                            {
                                ulong.TryParse(lineFM.Substring(7), out ulong result);
                                fm.SizeBytes = result;
                            }
                        }
                        fms.Add(fm);
                    }
                }
            });

            return (true, fms);
        }

        internal static List<int> MergeNDLFMData(List<FanMission> importedFMs, List<FanMission> mainList)
        {
            var checkedList = new List<FanMission>();
            var importedFMIndexes = new List<int>();
            int initCount = mainList.Count;
            int indexPastEnd = initCount - 1;

            for (int impFMi = 0; impFMi < importedFMs.Count; impFMi++)
            {
                var importedFM = importedFMs[impFMi];

                bool existingFound = false;
                for (int mainFMi = 0; mainFMi < initCount; mainFMi++)
                {
                    var mainFM = mainList[mainFMi];

                    if (!mainFM.Checked &&
                        mainFM.Archive.EqualsI(importedFM.Archive))
                    {
                        if (!importedFM.Title.IsEmpty()) mainFM.Title = importedFM.Title;
                        if (importedFM.ReleaseDate != null) mainFM.ReleaseDate = importedFM.ReleaseDate;
                        mainFM.LastPlayed = importedFM.LastPlayed;
                        mainFM.FinishedOn = importedFM.FinishedOn;
                        mainFM.Rating = importedFM.Rating;
                        mainFM.Comment = importedFM.Comment;
                        mainFM.DisabledMods = importedFM.DisabledMods;
                        mainFM.DisableAllMods = importedFM.DisableAllMods;
                        mainFM.TagsString = importedFM.TagsString;
                        mainFM.SelectedReadme = importedFM.SelectedReadme;
                        if (mainFM.SizeBytes == 0) mainFM.SizeBytes = importedFM.SizeBytes;

                        mainFM.Checked = true;

                        // So we only loop through checked FMs when we reset them
                        checkedList.Add(mainFM);

                        importedFMIndexes.Add(mainFMi);

                        existingFound = true;
                        break;
                    }
                }
                if (!existingFound)
                {
                    mainList.Add(new FanMission
                    {
                        Archive = importedFM.Archive,
                        InstalledDir = importedFM.InstalledDir,
                        Title = !importedFM.Title.IsEmpty() ? importedFM.Title : importedFM.Archive,
                        ReleaseDate = importedFM.ReleaseDate,
                        LastPlayed = importedFM.LastPlayed,
                        FinishedOn = importedFM.FinishedOn,
                        Rating = importedFM.Rating,
                        Comment = importedFM.Comment,
                        DisabledMods = importedFM.DisabledMods,
                        DisableAllMods = importedFM.DisableAllMods,
                        TagsString = importedFM.TagsString,
                        SelectedReadme = importedFM.SelectedReadme,
                        SizeBytes = importedFM.SizeBytes
                    });
                    indexPastEnd++;
                    importedFMIndexes.Add(indexPastEnd);
                }
            }

            // Reset temp bool
            for (int i = 0; i < checkedList.Count; i++) checkedList[i].Checked = false;

            return importedFMIndexes;
        }
    }
}
