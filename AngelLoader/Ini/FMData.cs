#define FenGen_FMDataDest

using System.Collections.Generic;
using System.IO;
using System.Text;
using AngelLoader.DataClasses;
using static AngelLoader.GameSupport;

// PERF_TODO: Notes for the writer:
// -Enum.ToString() is very expensive. Probably faster to do it manually with an if statement and nameof(value)
// -All the WriteLine()s take up a lot of time (most of which is GC). Using a StringBuilder would probably be
//  much faster.
// -The reader is lightning fast still. No need to do anything to it.

namespace AngelLoader.Ini
{
    internal static partial class Ini
    {
        // This method was autogenerated for maximum performance at runtime.
        internal static void ReadFMDataIni(string fileName, List<FanMission> fmsList)
        {
            var iniLines = File.ReadAllLines(fileName, Encoding.UTF8);

            if (fmsList.Count > 0) fmsList.Clear();

            bool fmsListIsEmpty = true;

            foreach (var line in iniLines)
            {
                var lineT = line.TrimStart();

                if (lineT.Length > 0 && lineT[0] == '[')
                {
                    if (lineT.Length >= 4 && lineT[1] == 'F' && lineT[2] == 'M' && lineT[3] == ']')
                    {
                        fmsList.Add(new FanMission());
                        if (fmsListIsEmpty) fmsListIsEmpty = false;
                    }

                    continue;
                }

                if (fmsListIsEmpty) continue;

                bool resourcesFound = false;

                // Comment chars (;) and blank lines will be rejected implicitly.
                // Since they're rare cases, checking for them would only slow us down.

                var fm = fmsList[fmsList.Count - 1];

                if (lineT.StartsWithFast_NoNullChecks("NoArchive="))
                {
                    var val = lineT.Substring(10);
                    fm.NoArchive = val.EqualsTrue();
                }
                else if (lineT.StartsWithFast_NoNullChecks("MarkedScanned="))
                {
                    var val = lineT.Substring(14);
                    fm.MarkedScanned = val.EqualsTrue();
                }
                else if (lineT.StartsWithFast_NoNullChecks("Archive="))
                {
                    var val = lineT.Substring(8);
                    fm.Archive = val;
                }
                else if (lineT.StartsWithFast_NoNullChecks("InstalledDir="))
                {
                    var val = lineT.Substring(13);
                    fm.InstalledDir = val;
                }
                else if (lineT.StartsWithFast_NoNullChecks("Title="))
                {
                    var val = lineT.Substring(6);
                    fm.Title = val;
                }
                else if (lineT.StartsWithFast_NoNullChecks("AltTitles="))
                {
                    var val = lineT.Substring(10);
                    if (!string.IsNullOrEmpty(val))
                    {
                        fm.AltTitles.Add(val);
                    }
                }
                else if (lineT.StartsWithFast_NoNullChecks("Author="))
                {
                    var val = lineT.Substring(7);
                    fm.Author = val;
                }
                else if (lineT.StartsWithFast_NoNullChecks("Game="))
                {
                    var val = lineT.Substring(5);
                    val = val.Trim();
                    if (val.EqualsI(nameof(Game.Thief1)))
                    {
                        fm.Game = Game.Thief1;
                    }
                    else if (val.EqualsI(nameof(Game.Thief2)))
                    {
                        fm.Game = Game.Thief2;
                    }
                    else if (val.EqualsI(nameof(Game.Thief3)))
                    {
                        fm.Game = Game.Thief3;
                    }
                    else if (val.EqualsI(nameof(Game.SS2)))
                    {
                        fm.Game = Game.SS2;
                    }
                    else if (val.EqualsI(nameof(Game.Unsupported)))
                    {
                        fm.Game = Game.Unsupported;
                    }
                    else
                    {
                        fm.Game = Game.Null;
                    }
                }
                else if (lineT.StartsWithFast_NoNullChecks("Installed="))
                {
                    var val = lineT.Substring(10);
                    fm.Installed = val.EqualsTrue();
                }
                else if (lineT.StartsWithFast_NoNullChecks("NoReadmes="))
                {
                    var val = lineT.Substring(10);
                    fm.NoReadmes = val.EqualsTrue();
                }
                else if (lineT.StartsWithFast_NoNullChecks("SelectedReadme="))
                {
                    var val = lineT.Substring(15);
                    fm.SelectedReadme = val;
                }
                else if (lineT.StartsWithFast_NoNullChecks("SizeBytes="))
                {
                    var val = lineT.Substring(10);
                    ulong.TryParse(val, out ulong result);
                    fm.SizeBytes = result;
                }
                else if (lineT.StartsWithFast_NoNullChecks("Rating="))
                {
                    var val = lineT.Substring(7);
                    bool success = int.TryParse(val, out int result);
                    fm.Rating = success ? result : -1;
                }
                else if (lineT.StartsWithFast_NoNullChecks("ReleaseDate="))
                {
                    var val = lineT.Substring(12);
                    fm.ReleaseDate.UnixDateString = val;
                }
                else if (lineT.StartsWithFast_NoNullChecks("LastPlayed="))
                {
                    var val = lineT.Substring(11);
                    fm.LastPlayed.UnixDateString = val;
                }
                else if (lineT.StartsWithFast_NoNullChecks("FinishedOn="))
                {
                    var val = lineT.Substring(11);
                    uint.TryParse(val, out uint result);
                    fm.FinishedOn = result;
                }
                else if (lineT.StartsWithFast_NoNullChecks("FinishedOnUnknown="))
                {
                    var val = lineT.Substring(18);
                    fm.FinishedOnUnknown = val.EqualsTrue();
                }
                else if (lineT.StartsWithFast_NoNullChecks("Comment="))
                {
                    var val = lineT.Substring(8);
                    fm.Comment = val;
                }
                else if (lineT.StartsWithFast_NoNullChecks("DisabledMods="))
                {
                    var val = lineT.Substring(13);
                    fm.DisabledMods = val;
                }
                else if (lineT.StartsWithFast_NoNullChecks("DisableAllMods="))
                {
                    var val = lineT.Substring(15);
                    fm.DisableAllMods = val.EqualsTrue();
                }
                else if (lineT.StartsWithFast_NoNullChecks("ResourcesScanned="))
                {
                    var val = lineT.Substring(17);
                    fm.ResourcesScanned = val.EqualsTrue();
                }
                else if (lineT.StartsWithFast_NoNullChecks("HasMap="))
                {
                    var val = lineT.Substring(7);
                    fm.HasMap = val.EqualsTrue();
                    resourcesFound = true;
                }
                else if (lineT.StartsWithFast_NoNullChecks("HasAutomap="))
                {
                    var val = lineT.Substring(11);
                    fm.HasAutomap = val.EqualsTrue();
                    resourcesFound = true;
                }
                else if (lineT.StartsWithFast_NoNullChecks("HasScripts="))
                {
                    var val = lineT.Substring(11);
                    fm.HasScripts = val.EqualsTrue();
                    resourcesFound = true;
                }
                else if (lineT.StartsWithFast_NoNullChecks("HasTextures="))
                {
                    var val = lineT.Substring(12);
                    fm.HasTextures = val.EqualsTrue();
                    resourcesFound = true;
                }
                else if (lineT.StartsWithFast_NoNullChecks("HasSounds="))
                {
                    var val = lineT.Substring(10);
                    fm.HasSounds = val.EqualsTrue();
                    resourcesFound = true;
                }
                else if (lineT.StartsWithFast_NoNullChecks("HasObjects="))
                {
                    var val = lineT.Substring(11);
                    fm.HasObjects = val.EqualsTrue();
                    resourcesFound = true;
                }
                else if (lineT.StartsWithFast_NoNullChecks("HasCreatures="))
                {
                    var val = lineT.Substring(13);
                    fm.HasCreatures = val.EqualsTrue();
                    resourcesFound = true;
                }
                else if (lineT.StartsWithFast_NoNullChecks("HasMotions="))
                {
                    var val = lineT.Substring(11);
                    fm.HasMotions = val.EqualsTrue();
                    resourcesFound = true;
                }
                else if (lineT.StartsWithFast_NoNullChecks("HasMovies="))
                {
                    var val = lineT.Substring(10);
                    fm.HasMovies = val.EqualsTrue();
                    resourcesFound = true;
                }
                else if (lineT.StartsWithFast_NoNullChecks("HasSubtitles="))
                {
                    var val = lineT.Substring(13);
                    fm.HasSubtitles = val.EqualsTrue();
                    resourcesFound = true;
                }
                else if (lineT.StartsWithFast_NoNullChecks("LanguagesString="))
                {
                    var val = lineT.Substring(16);
                    fm.LanguagesString = val;
                }
                else if (lineT.StartsWithFast_NoNullChecks("TagsString="))
                {
                    var val = lineT.Substring(11);
                    fm.TagsString = val;
                }
                if (resourcesFound) fm.ResourcesScanned = true;
            }
        }

        // This method was autogenerated for maximum performance at runtime.
        internal static void WriteFMDataIni(List<FanMission> fmDataList, string fileName)
        {
            using (var sw = new StreamWriter(fileName, false, Encoding.UTF8))
            {
                foreach (var fm in fmDataList)
                {
                    sw.WriteLine("[FM]");

                    if (fm.NoArchive)
                    {
                        sw.WriteLine("NoArchive=" + fm.NoArchive.ToString());
                    }
                    if (fm.MarkedScanned)
                    {
                        sw.WriteLine("MarkedScanned=" + fm.MarkedScanned.ToString());
                    }
                    if (!string.IsNullOrEmpty(fm.Archive))
                    {
                        sw.WriteLine("Archive=" + fm.Archive);
                    }
                    if (!string.IsNullOrEmpty(fm.InstalledDir))
                    {
                        sw.WriteLine("InstalledDir=" + fm.InstalledDir);
                    }
                    if (!string.IsNullOrEmpty(fm.Title))
                    {
                        sw.WriteLine("Title=" + fm.Title);
                    }
                    foreach (var s in fm.AltTitles)
                    {
                        sw.WriteLine("AltTitles=" + s);
                    }
                    if (!string.IsNullOrEmpty(fm.Author))
                    {
                        sw.WriteLine("Author=" + fm.Author);
                    }
                    if (fm.Game != Game.Null)
                    {
                        sw.WriteLine("Game=" + fm.Game.ToString());
                    }
                    if (fm.Installed)
                    {
                        sw.WriteLine("Installed=" + fm.Installed.ToString());
                    }
                    if (fm.NoReadmes)
                    {
                        sw.WriteLine("NoReadmes=" + fm.NoReadmes.ToString());
                    }
                    if (!string.IsNullOrEmpty(fm.SelectedReadme))
                    {
                        sw.WriteLine("SelectedReadme=" + fm.SelectedReadme);
                    }
                    if (fm.SizeBytes != 0)
                    {
                        sw.WriteLine("SizeBytes=" + fm.SizeBytes.ToString());
                    }
                    if (fm.Rating != -1)
                    {
                        sw.WriteLine("Rating=" + fm.Rating.ToString());
                    }
                    if (!string.IsNullOrEmpty(fm.ReleaseDate.UnixDateString))
                    {
                        sw.WriteLine("ReleaseDate=" + fm.ReleaseDate.UnixDateString);
                    }
                    if (!string.IsNullOrEmpty(fm.LastPlayed.UnixDateString))
                    {
                        sw.WriteLine("LastPlayed=" + fm.LastPlayed.UnixDateString);
                    }
                    if (fm.FinishedOn != 0)
                    {
                        sw.WriteLine("FinishedOn=" + fm.FinishedOn.ToString());
                    }
                    if (fm.FinishedOnUnknown)
                    {
                        sw.WriteLine("FinishedOnUnknown=" + fm.FinishedOnUnknown.ToString());
                    }
                    if (!string.IsNullOrEmpty(fm.Comment))
                    {
                        sw.WriteLine("Comment=" + fm.Comment);
                    }
                    if (!string.IsNullOrEmpty(fm.DisabledMods))
                    {
                        sw.WriteLine("DisabledMods=" + fm.DisabledMods);
                    }
                    if (fm.DisableAllMods)
                    {
                        sw.WriteLine("DisableAllMods=" + fm.DisableAllMods.ToString());
                    }
                    if (true)
                    {
                        sw.WriteLine("ResourcesScanned=" + fm.ResourcesScanned.ToString());
                        if (fm.ResourcesScanned)
                        {
                            if (fm.HasMap)
                            {
                                sw.WriteLine("HasMap=" + fm.HasMap.ToString());
                            }
                            if (fm.HasAutomap)
                            {
                                sw.WriteLine("HasAutomap=" + fm.HasAutomap.ToString());
                            }
                            if (fm.HasScripts)
                            {
                                sw.WriteLine("HasScripts=" + fm.HasScripts.ToString());
                            }
                            if (fm.HasTextures)
                            {
                                sw.WriteLine("HasTextures=" + fm.HasTextures.ToString());
                            }
                            if (fm.HasSounds)
                            {
                                sw.WriteLine("HasSounds=" + fm.HasSounds.ToString());
                            }
                            if (fm.HasObjects)
                            {
                                sw.WriteLine("HasObjects=" + fm.HasObjects.ToString());
                            }
                            if (fm.HasCreatures)
                            {
                                sw.WriteLine("HasCreatures=" + fm.HasCreatures.ToString());
                            }
                            if (fm.HasMotions)
                            {
                                sw.WriteLine("HasMotions=" + fm.HasMotions.ToString());
                            }
                            if (fm.HasMovies)
                            {
                                sw.WriteLine("HasMovies=" + fm.HasMovies.ToString());
                            }
                            if (fm.HasSubtitles)
                            {
                                sw.WriteLine("HasSubtitles=" + fm.HasSubtitles.ToString());
                            }
                        }
                    }
                    else
                    {
                        if (fm.ResourcesScanned)
                        {
                            {
                                sw.WriteLine("HasMap=" + fm.HasMap.ToString());
                            }
                            {
                                sw.WriteLine("HasAutomap=" + fm.HasAutomap.ToString());
                            }
                            {
                                sw.WriteLine("HasScripts=" + fm.HasScripts.ToString());
                            }
                            {
                                sw.WriteLine("HasTextures=" + fm.HasTextures.ToString());
                            }
                            {
                                sw.WriteLine("HasSounds=" + fm.HasSounds.ToString());
                            }
                            {
                                sw.WriteLine("HasObjects=" + fm.HasObjects.ToString());
                            }
                            {
                                sw.WriteLine("HasCreatures=" + fm.HasCreatures.ToString());
                            }
                            {
                                sw.WriteLine("HasMotions=" + fm.HasMotions.ToString());
                            }
                            {
                                sw.WriteLine("HasMovies=" + fm.HasMovies.ToString());
                            }
                            {
                                sw.WriteLine("HasSubtitles=" + fm.HasSubtitles.ToString());
                            }
                        }
                    }
                    if (!string.IsNullOrEmpty(fm.LanguagesString))
                    {
                        sw.WriteLine("LanguagesString=" + fm.LanguagesString);
                    }
                    if (!string.IsNullOrEmpty(fm.TagsString))
                    {
                        sw.WriteLine("TagsString=" + fm.TagsString);
                    }
                }
            }
        }
    }
}
