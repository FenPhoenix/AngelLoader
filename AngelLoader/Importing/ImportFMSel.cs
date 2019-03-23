using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using AngelLoader.Common;
using AngelLoader.Common.DataClasses;
using static AngelLoader.Ini.Ini;

namespace AngelLoader.Importing
{
    internal static class ImportFMSel
    {
        internal static async Task<(ImportError Error, List<FanMission> FMs)>
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
                        var instName = line.Substring(4, line.Length - 5);

                        var fm = new FanMission { InstalledDir = instName };

                        while (i < lines.Length - 1)
                        {
                            var lineFM = lines[i + 1];
                            if (lineFM.StartsWithFast_NoNullChecks("NiceName="))
                            {
                                fm.Title = lineFM.Substring(9);
                            }
                            else if (lineFM.StartsWithFast_NoNullChecks("Archive="))
                            {
                                fm.Archive = lineFM.Substring(8);
                            }
                            else if (lineFM.StartsWithFast_NoNullChecks("ReleaseDate="))
                            {
                                fm.ReleaseDate = ReadNullableHexDate(lineFM.Substring(12));
                            }
                            else if (lineFM.StartsWithFast_NoNullChecks("LastStarted="))
                            {
                                fm.LastPlayed = ReadNullableHexDate(lineFM.Substring(12));
                            }
                            else if (lineFM.StartsWithFast_NoNullChecks("Completed="))
                            {
                                int.TryParse(lineFM.Substring(10), out int result);
                                // Unfortunately FMSel doesn't let you choose the difficulty you finished on, so
                                // we have to have this fallback value as a best-effort thing.
                                if (result > 0) fm.FinishedOnUnknown = true;
                            }
                            else if (lineFM.StartsWithFast_NoNullChecks("Rating="))
                            {
                                fm.Rating = int.TryParse(lineFM.Substring(7), out int result) ? result : -1;
                            }
                            else if (lineFM.StartsWithFast_NoNullChecks("Notes="))
                            {
                                fm.Comment = lineFM.Substring(6).Replace(@"\n", "\r\n");
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
                        }
                        fms.Add(fm);
                    }
                }
            });

            return (ImportError.None, fms);
        }
    }
}
