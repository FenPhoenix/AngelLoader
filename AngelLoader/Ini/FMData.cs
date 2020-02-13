#define FenGen_FMDataDest

//#define write_old_resources_style

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AngelLoader.DataClasses;
using static AngelLoader.GameSupport;
using static AngelLoader.Misc;

namespace AngelLoader
{
    internal static partial class Ini
    {
        // This method was autogenerated for maximum performance at runtime.
        internal static void ReadFMDataIni(string fileName, List<FanMission> fmsList)
        {
            string[] iniLines = File.ReadAllLines(fileName, Encoding.UTF8);

            if (fmsList.Count > 0) fmsList.Clear();

            bool fmsListIsEmpty = true;

            foreach (string line in iniLines)
            {
                string lineT = line.Trim();

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

                FanMission fm = fmsList[fmsList.Count - 1];

                if (lineT.StartsWithFast_NoNullChecks("NoArchive="))
                {
                    string val = lineT.Substring(10);
                    fm.NoArchive = val.EqualsTrue();
                }
                else if (lineT.StartsWithFast_NoNullChecks("MarkedScanned="))
                {
                    string val = lineT.Substring(14);
                    fm.MarkedScanned = val.EqualsTrue();
                }
                else if (lineT.StartsWithFast_NoNullChecks("Archive="))
                {
                    string val = lineT.Substring(8);
                    fm.Archive = val;
                }
                else if (lineT.StartsWithFast_NoNullChecks("InstalledDir="))
                {
                    string val = lineT.Substring(13);
                    fm.InstalledDir = val;
                }
                else if (lineT.StartsWithFast_NoNullChecks("Title="))
                {
                    string val = lineT.Substring(6);
                    fm.Title = val;
                }
                else if (lineT.StartsWithFast_NoNullChecks("AltTitles="))
                {
                    string val = lineT.Substring(10);
                    if (!string.IsNullOrEmpty(val))
                    {
                        fm.AltTitles.Add(val);
                    }
                }
                else if (lineT.StartsWithFast_NoNullChecks("Author="))
                {
                    string val = lineT.Substring(7);
                    fm.Author = val;
                }
                else if (lineT.StartsWithFast_NoNullChecks("Game="))
                {
                    string val = lineT.Substring(5);
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
                    string val = lineT.Substring(10);
                    fm.Installed = val.EqualsTrue();
                }
                else if (lineT.StartsWithFast_NoNullChecks("NoReadmes="))
                {
                    string val = lineT.Substring(10);
                    fm.NoReadmes = val.EqualsTrue();
                }
                else if (lineT.StartsWithFast_NoNullChecks("SelectedReadme="))
                {
                    string val = lineT.Substring(15);
                    fm.SelectedReadme = val;
                }
                else if (lineT.StartsWithFast_NoNullChecks("SizeBytes="))
                {
                    string val = lineT.Substring(10);
                    ulong.TryParse(val, out ulong result);
                    fm.SizeBytes = result;
                }
                else if (lineT.StartsWithFast_NoNullChecks("Rating="))
                {
                    string val = lineT.Substring(7);
                    bool success = int.TryParse(val, out int result);
                    fm.Rating = success ? result : -1;
                }
                else if (lineT.StartsWithFast_NoNullChecks("ReleaseDate="))
                {
                    string val = lineT.Substring(12);
                    fm.ReleaseDate.UnixDateString = val;
                }
                else if (lineT.StartsWithFast_NoNullChecks("LastPlayed="))
                {
                    string val = lineT.Substring(11);
                    fm.LastPlayed.UnixDateString = val;
                }
                else if (lineT.StartsWithFast_NoNullChecks("Created="))
                {
                    string val = lineT.Substring(8);
                    fm.Created = ConvertHexUnixDateToDateTime(val);
                }
                else if (lineT.StartsWithFast_NoNullChecks("FinishedOn="))
                {
                    string val = lineT.Substring(11);
                    uint.TryParse(val, out uint result);
                    fm.FinishedOn = result;
                }
                else if (lineT.StartsWithFast_NoNullChecks("FinishedOnUnknown="))
                {
                    string val = lineT.Substring(18);
                    fm.FinishedOnUnknown = val.EqualsTrue();
                }
                else if (lineT.StartsWithFast_NoNullChecks("Comment="))
                {
                    string val = lineT.Substring(8);
                    fm.Comment = val;
                }
                else if (lineT.StartsWithFast_NoNullChecks("DisabledMods="))
                {
                    string val = lineT.Substring(13);
                    fm.DisabledMods = val;
                }
                else if (lineT.StartsWithFast_NoNullChecks("DisableAllMods="))
                {
                    string val = lineT.Substring(15);
                    fm.DisableAllMods = val.EqualsTrue();
                }
                else if (lineT.StartsWithFast_NoNullChecks("HasResources="))
                {
                    string val = lineT.Substring(13);
                    fm.ResourcesScanned = !val.EqualsI("NotScanned");
                    FillFMHasXFields(fm, val);
                }
                #region Old resource format - backward compatibility, we still have to be able to read it
                else if (lineT.StartsWithFast_NoNullChecks("HasMap="))
                {
                    string val = lineT.Substring(7);
                    SetFMResource(fm, CustomResources.Map, val.EqualsTrue());
                    resourcesFound = true;
                }
                else if (lineT.StartsWithFast_NoNullChecks("HasAutomap="))
                {
                    string val = lineT.Substring(11);
                    SetFMResource(fm, CustomResources.Automap, val.EqualsTrue());
                    resourcesFound = true;
                }
                else if (lineT.StartsWithFast_NoNullChecks("HasScripts="))
                {
                    string val = lineT.Substring(11);
                    SetFMResource(fm, CustomResources.Scripts, val.EqualsTrue());
                    resourcesFound = true;
                }
                else if (lineT.StartsWithFast_NoNullChecks("HasTextures="))
                {
                    string val = lineT.Substring(12);
                    SetFMResource(fm, CustomResources.Textures, val.EqualsTrue());
                    resourcesFound = true;
                }
                else if (lineT.StartsWithFast_NoNullChecks("HasSounds="))
                {
                    string val = lineT.Substring(10);
                    SetFMResource(fm, CustomResources.Sounds, val.EqualsTrue());
                    resourcesFound = true;
                }
                else if (lineT.StartsWithFast_NoNullChecks("HasObjects="))
                {
                    string val = lineT.Substring(11);
                    SetFMResource(fm, CustomResources.Objects, val.EqualsTrue());
                    resourcesFound = true;
                }
                else if (lineT.StartsWithFast_NoNullChecks("HasCreatures="))
                {
                    string val = lineT.Substring(13);
                    SetFMResource(fm, CustomResources.Creatures, val.EqualsTrue());
                    resourcesFound = true;
                }
                else if (lineT.StartsWithFast_NoNullChecks("HasMotions="))
                {
                    string val = lineT.Substring(11);
                    SetFMResource(fm, CustomResources.Motions, val.EqualsTrue());
                    resourcesFound = true;
                }
                else if (lineT.StartsWithFast_NoNullChecks("HasMovies="))
                {
                    string val = lineT.Substring(10);
                    SetFMResource(fm, CustomResources.Movies, val.EqualsTrue());
                    resourcesFound = true;
                }
                else if (lineT.StartsWithFast_NoNullChecks("HasSubtitles="))
                {
                    string val = lineT.Substring(13);
                    SetFMResource(fm, CustomResources.Subtitles, val.EqualsTrue());
                    resourcesFound = true;
                }
                #endregion
                else if (lineT.StartsWithFast_NoNullChecks("LanguagesString="))
                {
                    string val = lineT.Substring(16);
                    fm.LanguagesString = val;
                }
                else if (lineT.StartsWithFast_NoNullChecks("TagsString="))
                {
                    string val = lineT.Substring(11);
                    fm.TagsString = val;
                }
                if (resourcesFound) fm.ResourcesScanned = true;
            }
        }

        // This method was autogenerated for maximum performance at runtime.
        private static void WriteFMDataIni(List<FanMission> fmDataList, string fileName)
        {
            // Averaged over the 1573 FMs in my FMData.ini file (in new HasResources format)
            const int averageFMEntryCharCount = 378;
            var sb = new StringBuilder(averageFMEntryCharCount * fmDataList.Count);

            foreach (FanMission fm in fmDataList)
            {
                sb.AppendLine("[FM]");

                if (fm.NoArchive)
                {
                    sb.Append("NoArchive=");
                    sb.AppendLine(fm.NoArchive.ToString());
                }
                if (fm.MarkedScanned)
                {
                    sb.Append("MarkedScanned=");
                    sb.AppendLine(fm.MarkedScanned.ToString());
                }
                if (!string.IsNullOrEmpty(fm.Archive))
                {
                    sb.Append("Archive=");
                    sb.AppendLine(fm.Archive);
                }
                if (!string.IsNullOrEmpty(fm.InstalledDir))
                {
                    sb.Append("InstalledDir=");
                    sb.AppendLine(fm.InstalledDir);
                }
                if (!string.IsNullOrEmpty(fm.Title))
                {
                    sb.Append("Title=");
                    sb.AppendLine(fm.Title);
                }
                foreach (string s in fm.AltTitles)
                {
                    sb.Append("AltTitles=");
                    sb.AppendLine(s);
                }
                if (!string.IsNullOrEmpty(fm.Author))
                {
                    sb.Append("Author=");
                    sb.AppendLine(fm.Author);
                }
                switch (fm.Game)
                {
                    // Much faster to do this than Enum.ToString()
                    case Game.Thief1:
                        sb.AppendLine("Game=Thief1");
                        break;
                    case Game.Thief2:
                        sb.AppendLine("Game=Thief2");
                        break;
                    case Game.Thief3:
                        sb.AppendLine("Game=Thief3");
                        break;
                    case Game.SS2:
                        sb.AppendLine("Game=SS2");
                        break;
                    case Game.Unsupported:
                        sb.AppendLine("Game=Unsupported");
                        break;
                        // Don't handle Game.Null because we don't want to write out defaults
                }
                if (fm.Installed)
                {
                    sb.Append("Installed=");
                    sb.AppendLine(fm.Installed.ToString());
                }
                if (fm.NoReadmes)
                {
                    sb.Append("NoReadmes=");
                    sb.AppendLine(fm.NoReadmes.ToString());
                }
                if (!string.IsNullOrEmpty(fm.SelectedReadme))
                {
                    sb.Append("SelectedReadme=");
                    sb.AppendLine(fm.SelectedReadme);
                }
                if (fm.SizeBytes != 0)
                {
                    sb.Append("SizeBytes=");
                    sb.AppendLine(fm.SizeBytes.ToString());
                }
                if (fm.Rating != -1)
                {
                    sb.Append("Rating=");
                    sb.AppendLine(fm.Rating.ToString());
                }
                if (!string.IsNullOrEmpty(fm.ReleaseDate.UnixDateString))
                {
                    sb.Append("ReleaseDate=");
                    sb.AppendLine(fm.ReleaseDate.UnixDateString);
                }
                if (!string.IsNullOrEmpty(fm.LastPlayed.UnixDateString))
                {
                    sb.Append("LastPlayed=");
                    sb.AppendLine(fm.LastPlayed.UnixDateString);
                }
                if (fm.Created != null)
                {
                    sb.Append("Created=");
                    sb.AppendLine(new DateTimeOffset((DateTime)fm.Created).ToUnixTimeSeconds().ToString("X"));
                }
                // NOTE: This is not in itself an enum, it's a uint, so it's fast. We just cast it to an enum
                // later on, but no worries here.
                if (fm.FinishedOn != 0)
                {
                    sb.Append("FinishedOn=");
                    sb.AppendLine(fm.FinishedOn.ToString());
                }
                if (fm.FinishedOnUnknown)
                {
                    sb.Append("FinishedOnUnknown=");
                    sb.AppendLine(fm.FinishedOnUnknown.ToString());
                }
                if (!string.IsNullOrEmpty(fm.Comment))
                {
                    sb.Append("Comment=");
                    sb.AppendLine(fm.Comment);
                }
                if (!string.IsNullOrEmpty(fm.DisabledMods))
                {
                    sb.Append("DisabledMods=");
                    sb.AppendLine(fm.DisabledMods);
                }
                if (fm.DisableAllMods)
                {
                    sb.Append("DisableAllMods=");
                    sb.AppendLine(fm.DisableAllMods.ToString());
                }
#if write_old_resources_style
                if (fm.ResourcesScanned)
                {
                    sb.AppendLine("HasMap=" + FMHasResource(fm, CustomResources.Map).ToString());
                    sb.AppendLine("HasAutomap=" + FMHasResource(fm, CustomResources.Automap).ToString());
                    sb.AppendLine("HasScripts=" + FMHasResource(fm, CustomResources.Scripts).ToString());
                    sb.AppendLine("HasTextures=" + FMHasResource(fm, CustomResources.Textures).ToString());
                    sb.AppendLine("HasSounds=" + FMHasResource(fm, CustomResources.Sounds).ToString());
                    sb.AppendLine("HasObjects=" + FMHasResource(fm, CustomResources.Objects).ToString());
                    sb.AppendLine("HasCreatures=" + FMHasResource(fm, CustomResources.Creatures).ToString());
                    sb.AppendLine("HasMotions=" + FMHasResource(fm, CustomResources.Motions).ToString());
                    sb.AppendLine("HasMovies=" + FMHasResource(fm, CustomResources.Movies).ToString());
                    sb.AppendLine("HasSubtitles=" + FMHasResource(fm, CustomResources.Subtitles).ToString());
                }
#else
                sb.Append("HasResources=");
                if (fm.ResourcesScanned)
                {
                    CommaCombineHasXFields(fm, sb);
                }
                else
                {
                    sb.AppendLine("NotScanned");
                }
#endif
                if (!string.IsNullOrEmpty(fm.LanguagesString))
                {
                    sb.Append("LanguagesString=");
                    sb.AppendLine(fm.LanguagesString);
                }
                if (!string.IsNullOrEmpty(fm.TagsString))
                {
                    sb.Append("TagsString=");
                    sb.AppendLine(fm.TagsString);
                }
            }

            using var sw = new StreamWriter(fileName, false, Encoding.UTF8);
            sw.Write(sb.ToString());
        }
    }
}
