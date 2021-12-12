#define FenGen_FMDataDest

//#define write_old_resources_style

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AL_Common;
using AngelLoader.DataClasses;
using static AngelLoader.GameSupport;
using static AngelLoader.Misc;

namespace AngelLoader
{
    internal static partial class Ini
    {
        private static void NoArchive_Set(FanMission fm, string valTrimmed, string valRaw)
        {
            fm.NoArchive = valTrimmed.EqualsTrue();
        }

        private static void MarkedScanned_Set(FanMission fm, string valTrimmed, string valRaw)
        {
            fm.MarkedScanned = valTrimmed.EqualsTrue();
        }

        private static void Pinned_Set(FanMission fm, string valTrimmed, string valRaw)
        {
            fm.Pinned = valTrimmed.EqualsTrue();
        }

        private static void Archive_Set(FanMission fm, string valTrimmed, string valRaw)
        {
            fm.Archive = valTrimmed;
        }

        private static void InstalledDir_Set(FanMission fm, string valTrimmed, string valRaw)
        {
            fm.InstalledDir = valTrimmed;
        }

        private static void Title_Set(FanMission fm, string valTrimmed, string valRaw)
        {
            fm.Title = valTrimmed;
        }

        private static void AltTitles_Set(FanMission fm, string valTrimmed, string valRaw)
        {
            if (!string.IsNullOrEmpty(valTrimmed))
            {
                fm.AltTitles.Add(valTrimmed);
            }
        }

        private static void Author_Set(FanMission fm, string valTrimmed, string valRaw)
        {
            fm.Author = valTrimmed;
        }

        private static void Game_Set(FanMission fm, string valTrimmed, string valRaw)
        {
            if (valTrimmed.EqualsI("Thief1"))
            {
                fm.Game = Game.Thief1;
            }
            else if (valTrimmed.EqualsI("Thief2"))
            {
                fm.Game = Game.Thief2;
            }
            else if (valTrimmed.EqualsI("Thief3"))
            {
                fm.Game = Game.Thief3;
            }
            else if (valTrimmed.EqualsI("SS2"))
            {
                fm.Game = Game.SS2;
            }
            else if (valTrimmed.EqualsI("Unsupported"))
            {
                fm.Game = Game.Unsupported;
            }
            else
            {
                fm.Game = Game.Null;
            }
        }

        private static void Installed_Set(FanMission fm, string valTrimmed, string valRaw)
        {
            fm.Installed = valTrimmed.EqualsTrue();
        }

        private static void NoReadmes_Set(FanMission fm, string valTrimmed, string valRaw)
        {
            fm.NoReadmes = valTrimmed.EqualsTrue();
        }

        private static void SelectedReadme_Set(FanMission fm, string valTrimmed, string valRaw)
        {
            fm.SelectedReadme = valTrimmed;
        }

        private static void ReadmeEncoding_Set(FanMission fm, string valTrimmed, string valRaw)
        {
            if (!string.IsNullOrEmpty(valTrimmed))
            {
                fm.ReadmeAndCodePageEntries.Add(valTrimmed);
            }
        }

        private static void SizeBytes_Set(FanMission fm, string valTrimmed, string valRaw)
        {
            ulong.TryParse(valTrimmed, out ulong result);
            fm.SizeBytes = result;
        }

        private static void Rating_Set(FanMission fm, string valTrimmed, string valRaw)
        {
            bool success = int.TryParse(valTrimmed, out int result);
            fm.Rating = success ? result : -1;
        }

        private static void ReleaseDate_Set(FanMission fm, string valTrimmed, string valRaw)
        {
            fm.ReleaseDate.UnixDateString = valTrimmed;
        }

        private static void LastPlayed_Set(FanMission fm, string valTrimmed, string valRaw)
        {
            fm.LastPlayed.UnixDateString = valTrimmed;
        }

        private static void DateAdded_Set(FanMission fm, string valTrimmed, string valRaw)
        {
            // PERF: Don't convert to local here; do it at display-time
            fm.DateAdded = ConvertHexUnixDateToDateTime(valTrimmed, convertToLocal: false);
        }

        private static void FinishedOn_Set(FanMission fm, string valTrimmed, string valRaw)
        {
            uint.TryParse(valTrimmed, out uint result);
            fm.FinishedOn = result;
        }

        private static void FinishedOnUnknown_Set(FanMission fm, string valTrimmed, string valRaw)
        {
            fm.FinishedOnUnknown = valTrimmed.EqualsTrue();
        }

        private static void Comment_Set(FanMission fm, string valTrimmed, string valRaw)
        {
            fm.Comment = valRaw;
        }

        private static void DisabledMods_Set(FanMission fm, string valTrimmed, string valRaw)
        {
            fm.DisabledMods = valTrimmed;
        }

        private static void DisableAllMods_Set(FanMission fm, string valTrimmed, string valRaw)
        {
            fm.DisableAllMods = valTrimmed.EqualsTrue();
        }

        private static void HasResources_Set(FanMission fm, string valTrimmed, string valRaw)
        {
            fm.ResourcesScanned = !valTrimmed.EqualsI("NotScanned");
            FillFMHasXFields(fm, valTrimmed);
        }

        private static void HasMap_Set(FanMission fm, string valTrimmed, string valRaw)
        {
            SetFMResource(fm, CustomResources.Map, valTrimmed.EqualsTrue());
            fm.ResourcesScanned = true;
        }

        private static void HasAutomap_Set(FanMission fm, string valTrimmed, string valRaw)
        {
            SetFMResource(fm, CustomResources.Automap, valTrimmed.EqualsTrue());
            fm.ResourcesScanned = true;
        }

        private static void HasScripts_Set(FanMission fm, string valTrimmed, string valRaw)
        {
            SetFMResource(fm, CustomResources.Scripts, valTrimmed.EqualsTrue());
            fm.ResourcesScanned = true;
        }

        private static void HasTextures_Set(FanMission fm, string valTrimmed, string valRaw)
        {
            SetFMResource(fm, CustomResources.Textures, valTrimmed.EqualsTrue());
            fm.ResourcesScanned = true;
        }

        private static void HasSounds_Set(FanMission fm, string valTrimmed, string valRaw)
        {
            SetFMResource(fm, CustomResources.Sounds, valTrimmed.EqualsTrue());
            fm.ResourcesScanned = true;
        }

        private static void HasObjects_Set(FanMission fm, string valTrimmed, string valRaw)
        {
            SetFMResource(fm, CustomResources.Objects, valTrimmed.EqualsTrue());
            fm.ResourcesScanned = true;
        }

        private static void HasCreatures_Set(FanMission fm, string valTrimmed, string valRaw)
        {
            SetFMResource(fm, CustomResources.Creatures, valTrimmed.EqualsTrue());
            fm.ResourcesScanned = true;
        }

        private static void HasMotions_Set(FanMission fm, string valTrimmed, string valRaw)
        {
            SetFMResource(fm, CustomResources.Motions, valTrimmed.EqualsTrue());
            fm.ResourcesScanned = true;
        }

        private static void HasMovies_Set(FanMission fm, string valTrimmed, string valRaw)
        {
            SetFMResource(fm, CustomResources.Movies, valTrimmed.EqualsTrue());
            fm.ResourcesScanned = true;
        }

        private static void HasSubtitles_Set(FanMission fm, string valTrimmed, string valRaw)
        {
            SetFMResource(fm, CustomResources.Subtitles, valTrimmed.EqualsTrue());
            fm.ResourcesScanned = true;
        }

        private static void LangsScanned_Set(FanMission fm, string valTrimmed, string valRaw)
        {
            fm.LangsScanned = valTrimmed.EqualsTrue();
        }

        private static void Langs_Set(FanMission fm, string valTrimmed, string valRaw)
        {
            fm.Langs = valTrimmed;
        }

        private static void SelectedLang_Set(FanMission fm, string valTrimmed, string valRaw)
        {
            fm.SelectedLang = valTrimmed;
        }

        private static void TagsString_Set(FanMission fm, string valTrimmed, string valRaw)
        {
            fm.TagsString = valTrimmed;
        }

        private static readonly Dictionary<string, Action<FanMission, string, string>> _actionDict = new()
        {
            { "NoArchive", NoArchive_Set },
            { "MarkedScanned", MarkedScanned_Set },
            { "Pinned", Pinned_Set },
            { "Archive", Archive_Set },
            { "InstalledDir", InstalledDir_Set },
            { "Title", Title_Set },
            { "AltTitles", AltTitles_Set },
            { "Author", Author_Set },
            { "Game", Game_Set },
            { "Installed", Installed_Set },
            { "NoReadmes", NoReadmes_Set },
            { "SelectedReadme", SelectedReadme_Set },
            { "ReadmeEncoding", ReadmeEncoding_Set },
            { "SizeBytes", SizeBytes_Set },
            { "Rating", Rating_Set },
            { "ReleaseDate", ReleaseDate_Set },
            { "LastPlayed", LastPlayed_Set },
            { "DateAdded", DateAdded_Set },
            { "FinishedOn", FinishedOn_Set },
            { "FinishedOnUnknown", FinishedOnUnknown_Set },
            { "Comment", Comment_Set },
            { "DisabledMods", DisabledMods_Set },
            { "DisableAllMods", DisableAllMods_Set },
            { "HasResources", HasResources_Set },

            { "HasMap", HasMap_Set },
            { "HasAutomap", HasAutomap_Set },
            { "HasScripts", HasScripts_Set },
            { "HasTextures", HasTextures_Set },
            { "HasSounds", HasSounds_Set },
            { "HasObjects", HasObjects_Set },
            { "HasCreatures", HasCreatures_Set },
            { "HasMotions", HasMotions_Set },
            { "HasMovies", HasMovies_Set },
            { "HasSubtitles", HasSubtitles_Set },

            { "LangsScanned", LangsScanned_Set },
            { "Langs", Langs_Set },
            { "SelectedLang", SelectedLang_Set },
            { "TagsString", TagsString_Set },
        };

        // This method was autogenerated for maximum performance at runtime.
        internal static void ReadFMDataIni(string fileName, List<FanMission> fmsList)
        {
            string[] iniLines = File.ReadAllLines(fileName, Encoding.UTF8);

            if (fmsList.Count > 0) fmsList.Clear();

            bool fmsListIsEmpty = true;

            foreach (string line in iniLines)
            {
                string lineTS = line.TrimStart();

                if (lineTS.Length > 0 && lineTS[0] == '[')
                {
                    if (lineTS.Length >= 4 && lineTS[1] == 'F' && lineTS[2] == 'M' && lineTS[3] == ']')
                    {
                        fmsList.Add(new FanMission());
                        if (fmsListIsEmpty) fmsListIsEmpty = false;
                    }

                    continue;
                }

                if (fmsListIsEmpty) continue;

                // Comment chars (;) and blank lines will be rejected implicitly.
                // Since they're rare cases, checking for them would only slow us down.

                FanMission fm = fmsList[fmsList.Count - 1];

                int eqIndex = lineTS.IndexOf('=');
                if (eqIndex > -1)
                {
                    string key = lineTS.Substring(0, eqIndex);
                    string valRaw = lineTS.Substring(eqIndex + 1);
                    string valTrimmed = valRaw.TrimEnd();
                    if (_actionDict.TryGetValue(key, out var action))
                    {
                        action.Invoke(fm, valTrimmed, valRaw);
                    }
                }
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
                if (fm.Pinned)
                {
                    sb.Append("Pinned=");
                    sb.AppendLine(fm.Pinned.ToString());
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
                foreach (string s in fm.ReadmeAndCodePageEntries)
                {
                    sb.Append("ReadmeEncoding=");
                    sb.AppendLine(s);
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
                if (fm.DateAdded != null)
                {
                    sb.Append("DateAdded=");
                    // Again, important to convert to local time here because we don't do it on startup.
                    sb.AppendLine(new DateTimeOffset(((DateTime)fm.DateAdded).ToLocalTime()).ToUnixTimeSeconds().ToString("X"));
                }
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
                if (fm.LangsScanned)
                {
                    sb.Append("LangsScanned=");
                    sb.AppendLine(fm.LangsScanned.ToString());
                }
                if (!string.IsNullOrEmpty(fm.Langs))
                {
                    sb.Append("Langs=");
                    sb.AppendLine(fm.Langs);
                }
                if (!string.IsNullOrEmpty(fm.SelectedLang))
                {
                    sb.Append("SelectedLang=");
                    sb.AppendLine(fm.SelectedLang);
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
