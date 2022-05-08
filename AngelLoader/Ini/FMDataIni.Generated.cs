#define FenGen_FMDataDest

//#define write_old_resources_style

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AL_Common;
using AngelLoader.DataClasses;
using static AL_Common.Common;
using static AngelLoader.FenGenAttributes;
using static AngelLoader.GameSupport;
using static AngelLoader.LanguageSupport;
using static AngelLoader.Misc;

namespace AngelLoader
{
    [FenGenFMDataDestClass]
    internal static partial class Ini
    {
        #region Generated code for reader

        // This nonsense is to allow for keys to be looked up in a dictionary rather than running ten thousand
        // if statements on every line.

        private static void FMData_NoArchive_Set(FanMission fm, string valTrimmed, string valRaw)
        {
            fm.NoArchive = valTrimmed.EqualsTrue();
        }

        private static void FMData_MarkedScanned_Set(FanMission fm, string valTrimmed, string valRaw)
        {
            fm.MarkedScanned = valTrimmed.EqualsTrue();
        }

        private static void FMData_Pinned_Set(FanMission fm, string valTrimmed, string valRaw)
        {
            fm.Pinned = valTrimmed.EqualsTrue();
        }

        private static void FMData_Archive_Set(FanMission fm, string valTrimmed, string valRaw)
        {
            fm.Archive = valTrimmed;
        }

        private static void FMData_InstalledDir_Set(FanMission fm, string valTrimmed, string valRaw)
        {
            fm.InstalledDir = valTrimmed;
        }

        private static void FMData_Title_Set(FanMission fm, string valTrimmed, string valRaw)
        {
            fm.Title = valTrimmed;
        }

        private static void FMData_AltTitles_Set(FanMission fm, string valTrimmed, string valRaw)
        {
            if (!string.IsNullOrEmpty(valTrimmed))
            {
                fm.AltTitles.Add(valTrimmed);
            }
        }

        private static void FMData_Author_Set(FanMission fm, string valTrimmed, string valRaw)
        {
            fm.Author = valTrimmed;
        }

        private static void FMData_Game_Set(FanMission fm, string valTrimmed, string valRaw)
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

        private static void FMData_Installed_Set(FanMission fm, string valTrimmed, string valRaw)
        {
            fm.Installed = valTrimmed.EqualsTrue();
        }

        private static void FMData_NoReadmes_Set(FanMission fm, string valTrimmed, string valRaw)
        {
            fm.NoReadmes = valTrimmed.EqualsTrue();
        }

        private static void FMData_SelectedReadme_Set(FanMission fm, string valTrimmed, string valRaw)
        {
            fm.SelectedReadme = valTrimmed;
        }

        private static void FMData_ReadmeEncoding_Set(FanMission fm, string valTrimmed, string valRaw)
        {
            if (!string.IsNullOrEmpty(valTrimmed))
            {
                fm.ReadmeAndCodePageEntries.Add(valTrimmed);
            }
        }

        private static void FMData_SizeBytes_Set(FanMission fm, string valTrimmed, string valRaw)
        {
            ulong.TryParse(valTrimmed, out ulong result);
            fm.SizeBytes = result;
        }

        private static void FMData_Rating_Set(FanMission fm, string valTrimmed, string valRaw)
        {
            bool success = int.TryParse(valTrimmed, out int result);
            fm.Rating = success ? result : -1;
        }

        private static void FMData_ReleaseDate_Set(FanMission fm, string valTrimmed, string valRaw)
        {
            fm.ReleaseDate.UnixDateString = valTrimmed;
        }

        private static void FMData_LastPlayed_Set(FanMission fm, string valTrimmed, string valRaw)
        {
            fm.LastPlayed.UnixDateString = valTrimmed;
        }

        private static void FMData_DateAdded_Set(FanMission fm, string valTrimmed, string valRaw)
        {
            // PERF: Don't convert to local here; do it at display-time
            fm.DateAdded = ConvertHexUnixDateToDateTime(valTrimmed, convertToLocal: false);
        }

        private static void FMData_FinishedOn_Set(FanMission fm, string valTrimmed, string valRaw)
        {
            uint.TryParse(valTrimmed, out uint result);
            fm.FinishedOn = result;
        }

        private static void FMData_FinishedOnUnknown_Set(FanMission fm, string valTrimmed, string valRaw)
        {
            fm.FinishedOnUnknown = valTrimmed.EqualsTrue();
        }

        private static void FMData_Comment_Set(FanMission fm, string valTrimmed, string valRaw)
        {
            fm.Comment = valRaw;
        }

        private static void FMData_DisabledMods_Set(FanMission fm, string valTrimmed, string valRaw)
        {
            fm.DisabledMods = valTrimmed;
        }

        private static void FMData_DisableAllMods_Set(FanMission fm, string valTrimmed, string valRaw)
        {
            fm.DisableAllMods = valTrimmed.EqualsTrue();
        }

        private static void FMData_HasResources_Set(FanMission fm, string valTrimmed, string valRaw)
        {
            fm.ResourcesScanned = !valTrimmed.EqualsI("NotScanned");
            FillFMHasXFields(fm, valTrimmed);
        }

        private static void FMData_LangsScanned_Set(FanMission fm, string valTrimmed, string valRaw)
        {
            fm.LangsScanned = valTrimmed.EqualsTrue();
        }

        private static void FMData_Langs_Set(FanMission fm, string valTrimmed, string valRaw)
        {
            SetFMLanguages(fm, valTrimmed);
        }

        private static void FMData_SelectedLang_Set(FanMission fm, string valTrimmed, string valRaw)
        {
            if (LangStringsToEnums.TryGetValue(valTrimmed, out var result))
            {
                fm.SelectedLang = result;
            }
        }

        private static void FMData_TagsString_Set(FanMission fm, string valTrimmed, string valRaw)
        {
            fm.TagsString = valTrimmed;
        }

        #region Old resource format - backward compatibility, we still have to be able to read it

        private static void FMData_HasMap_Set(FanMission fm, string valTrimmed, string valRaw)
        {
            SetFMResource(fm, CustomResources.Map, valTrimmed.EqualsTrue());
            fm.ResourcesScanned = true;
        }

        private static void FMData_HasAutomap_Set(FanMission fm, string valTrimmed, string valRaw)
        {
            SetFMResource(fm, CustomResources.Automap, valTrimmed.EqualsTrue());
            fm.ResourcesScanned = true;
        }

        private static void FMData_HasScripts_Set(FanMission fm, string valTrimmed, string valRaw)
        {
            SetFMResource(fm, CustomResources.Scripts, valTrimmed.EqualsTrue());
            fm.ResourcesScanned = true;
        }

        private static void FMData_HasTextures_Set(FanMission fm, string valTrimmed, string valRaw)
        {
            SetFMResource(fm, CustomResources.Textures, valTrimmed.EqualsTrue());
            fm.ResourcesScanned = true;
        }

        private static void FMData_HasSounds_Set(FanMission fm, string valTrimmed, string valRaw)
        {
            SetFMResource(fm, CustomResources.Sounds, valTrimmed.EqualsTrue());
            fm.ResourcesScanned = true;
        }

        private static void FMData_HasObjects_Set(FanMission fm, string valTrimmed, string valRaw)
        {
            SetFMResource(fm, CustomResources.Objects, valTrimmed.EqualsTrue());
            fm.ResourcesScanned = true;
        }

        private static void FMData_HasCreatures_Set(FanMission fm, string valTrimmed, string valRaw)
        {
            SetFMResource(fm, CustomResources.Creatures, valTrimmed.EqualsTrue());
            fm.ResourcesScanned = true;
        }

        private static void FMData_HasMotions_Set(FanMission fm, string valTrimmed, string valRaw)
        {
            SetFMResource(fm, CustomResources.Motions, valTrimmed.EqualsTrue());
            fm.ResourcesScanned = true;
        }

        private static void FMData_HasMovies_Set(FanMission fm, string valTrimmed, string valRaw)
        {
            SetFMResource(fm, CustomResources.Movies, valTrimmed.EqualsTrue());
            fm.ResourcesScanned = true;
        }

        private static void FMData_HasSubtitles_Set(FanMission fm, string valTrimmed, string valRaw)
        {
            SetFMResource(fm, CustomResources.Subtitles, valTrimmed.EqualsTrue());
            fm.ResourcesScanned = true;
        }

        #endregion

        private static readonly Dictionary<string, Action<FanMission, string, string>> _actionDict_FMData = new()
        {
            { "NoArchive", FMData_NoArchive_Set },
            { "MarkedScanned", FMData_MarkedScanned_Set },
            { "Pinned", FMData_Pinned_Set },
            { "Archive", FMData_Archive_Set },
            { "InstalledDir", FMData_InstalledDir_Set },
            { "Title", FMData_Title_Set },
            { "AltTitles", FMData_AltTitles_Set },
            { "Author", FMData_Author_Set },
            { "Game", FMData_Game_Set },
            { "Installed", FMData_Installed_Set },
            { "NoReadmes", FMData_NoReadmes_Set },
            { "SelectedReadme", FMData_SelectedReadme_Set },
            { "ReadmeEncoding", FMData_ReadmeEncoding_Set },
            { "SizeBytes", FMData_SizeBytes_Set },
            { "Rating", FMData_Rating_Set },
            { "ReleaseDate", FMData_ReleaseDate_Set },
            { "LastPlayed", FMData_LastPlayed_Set },
            { "DateAdded", FMData_DateAdded_Set },
            { "FinishedOn", FMData_FinishedOn_Set },
            { "FinishedOnUnknown", FMData_FinishedOnUnknown_Set },
            { "Comment", FMData_Comment_Set },
            { "DisabledMods", FMData_DisabledMods_Set },
            { "DisableAllMods", FMData_DisableAllMods_Set },
            { "HasResources", FMData_HasResources_Set },
            { "LangsScanned", FMData_LangsScanned_Set },
            { "Langs", FMData_Langs_Set },
            { "SelectedLang", FMData_SelectedLang_Set },
            { "TagsString", FMData_TagsString_Set },

            #region Old resource format - backward compatibility, we still have to be able to read it

            { "HasMap", FMData_HasMap_Set },
            { "HasAutomap", FMData_HasAutomap_Set },
            { "HasScripts", FMData_HasScripts_Set },
            { "HasTextures", FMData_HasTextures_Set },
            { "HasSounds", FMData_HasSounds_Set },
            { "HasObjects", FMData_HasObjects_Set },
            { "HasCreatures", FMData_HasCreatures_Set },
            { "HasMotions", FMData_HasMotions_Set },
            { "HasMovies", FMData_HasMovies_Set },
            { "HasSubtitles", FMData_HasSubtitles_Set }

            #endregion
        };

        #endregion

        #region Generated code for writer

        private static void WriteFMDataIni(List<FanMission> fmDataList, string fileName)
        {
            var sb = new StringBuilder();

            foreach (FanMission fm in fmDataList)
            {
                sb.AppendLine("[FM]");

                if (fm.NoArchive)
                {
                    sb.AppendLine("NoArchive=True");
                }
                if (fm.MarkedScanned)
                {
                    sb.AppendLine("MarkedScanned=True");
                }
                if (fm.Pinned)
                {
                    sb.AppendLine("Pinned=True");
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
                    sb.AppendLine("Installed=True");
                }
                if (fm.NoReadmes)
                {
                    sb.AppendLine("NoReadmes=True");
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
                    sb.AppendLine("FinishedOnUnknown=True");
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
                    sb.AppendLine("LangsScanned=True");
                }
                if (fm.Langs != 0)
                {
                    sb.Append("Langs=");
                    CommaCombineLanguageFlags(sb, fm.Langs);
                }
                switch (fm.SelectedLang)
                {
                    // Much faster to do this than Enum.ToString()
                    case Language.English:
                        sb.AppendLine("SelectedLang=english");
                        break;
                    case Language.Czech:
                        sb.AppendLine("SelectedLang=czech");
                        break;
                    case Language.Dutch:
                        sb.AppendLine("SelectedLang=dutch");
                        break;
                    case Language.French:
                        sb.AppendLine("SelectedLang=french");
                        break;
                    case Language.German:
                        sb.AppendLine("SelectedLang=german");
                        break;
                    case Language.Hungarian:
                        sb.AppendLine("SelectedLang=hungarian");
                        break;
                    case Language.Italian:
                        sb.AppendLine("SelectedLang=italian");
                        break;
                    case Language.Japanese:
                        sb.AppendLine("SelectedLang=japanese");
                        break;
                    case Language.Polish:
                        sb.AppendLine("SelectedLang=polish");
                        break;
                    case Language.Russian:
                        sb.AppendLine("SelectedLang=russian");
                        break;
                    case Language.Spanish:
                        sb.AppendLine("SelectedLang=spanish");
                        break;
                        // Don't handle Language.Default because we don't want to write out defaults
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

        #endregion
    }
}
