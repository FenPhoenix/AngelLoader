using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
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
                    var lineTS = line.TrimStart();
                    var lineTB = lineTS.TrimEnd();

                    #region Disabled

                    //if (!archiveRootRead)
                    //{
                    //    if (lineTB != "[Config]") continue;

                    //    while (i < lines.Length - 1)
                    //    {
                    //        var lt = lines[i + 1].Trim();
                    //        if (lt.StartsWithFast_NoNullChecks("ArchiveRoot="))
                    //        {
                    //            archiveRoot = lt.Substring(12);
                    //            archiveRootRead = true;
                    //            i++;
                    //            break;
                    //        }
                    //        else if (!lt.IsEmpty() && lt[0] == '[' && lt[lt.Length - 1] == ']')
                    //        {
                    //            break;
                    //        }
                    //        i++;
                    //    }
                    //    if (archiveRootRead)
                    //    {
                    //        i = -1;
                    //        break;
                    //    }
                    //}

                    #endregion

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
                                // Handle tags reading here
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
            var importedFMIndexes = new List<int>();

            // placeholder
            return importedFMIndexes;
        }
    }
}
