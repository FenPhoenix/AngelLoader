#define FenGen_FMDataDest

//#define write_old_resources_style

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AL_Common;
using AngelLoader.DataClasses;
using static AngelLoader.FenGenAttributes;
using static AngelLoader.GameSupport;
using static AngelLoader.LanguageSupport;
using static AngelLoader.Utils;

namespace AngelLoader
{
    [FenGenFMDataDestClass]
    internal static partial class Ini
    {
        #region Generated code for reader

        // This nonsense is to allow for keys to be looked up in a dictionary rather than running ten thousand
        // if statements on every line.

        private static void FMData_NoArchive_Set(FanMission fm, string val, int eqIndex)
        {
            val = val.Trim();
            fm.NoArchive = val.EndEqualsTrue(eqIndex + 1);
        }

        private static void FMData_MarkedScanned_Set(FanMission fm, string val, int eqIndex)
        {
            val = val.Trim();
            fm.MarkedScanned = val.EndEqualsTrue(eqIndex + 1);
        }

        private static void FMData_Pinned_Set(FanMission fm, string val, int eqIndex)
        {
            val = val.Trim();
            fm.Pinned = val.EndEqualsTrue(eqIndex + 1);
        }

        private static void FMData_Archive_Set(FanMission fm, string val, int eqIndex)
        {
            val = val.Substring(eqIndex + 1);
            val = val.Trim();
            fm.Archive = val;
        }

        private static void FMData_InstalledDir_Set(FanMission fm, string val, int eqIndex)
        {
            val = val.Substring(eqIndex + 1);
            val = val.Trim();
            fm.InstalledDir = val;
        }

        private static void FMData_Title_Set(FanMission fm, string val, int eqIndex)
        {
            val = val.Substring(eqIndex + 1);
            val = val.Trim();
            fm.Title = val;
        }

        private static void FMData_AltTitles_Set(FanMission fm, string val, int eqIndex)
        {
            val = val.Substring(eqIndex + 1);
            val = val.Trim();
            if (!string.IsNullOrEmpty(val))
            {
                fm.AltTitles.Add(val);
            }
        }

        private static void FMData_Author_Set(FanMission fm, string val, int eqIndex)
        {
            val = val.Substring(eqIndex + 1);
            val = val.Trim();
            fm.Author = val;
        }

        private static void FMData_Game_Set(FanMission fm, string val, int eqIndex)
        {
            val = val.Trim();
            if (val.ValueEqualsI("Thief1", eqIndex + 1))
            {
                fm.Game = Game.Thief1;
            }
            else if (val.ValueEqualsI("Thief2", eqIndex + 1))
            {
                fm.Game = Game.Thief2;
            }
            else if (val.ValueEqualsI("Thief3", eqIndex + 1))
            {
                fm.Game = Game.Thief3;
            }
            else if (val.ValueEqualsI("SS2", eqIndex + 1))
            {
                fm.Game = Game.SS2;
            }
            else if (val.ValueEqualsI("Unsupported", eqIndex + 1))
            {
                fm.Game = Game.Unsupported;
            }
            else
            {
                fm.Game = Game.Null;
            }
        }

        private static void FMData_Installed_Set(FanMission fm, string val, int eqIndex)
        {
            val = val.Trim();
            fm.Installed = val.EndEqualsTrue(eqIndex + 1);
        }

        private static void FMData_NoReadmes_Set(FanMission fm, string val, int eqIndex)
        {
            val = val.Trim();
            fm.NoReadmes = val.EndEqualsTrue(eqIndex + 1);
        }

        private static void FMData_SelectedReadme_Set(FanMission fm, string val, int eqIndex)
        {
            val = val.Substring(eqIndex + 1);
            val = val.Trim();
            fm.SelectedReadme = val;
        }

        private static void FMData_ReadmeEncoding_Set(FanMission fm, string val, int eqIndex)
        {
            val = val.Trim();
            AddReadmeEncoding(fm, val, eqIndex + 1);
        }

        private static void FMData_SizeBytes_Set(FanMission fm, string val, int eqIndex)
        {
            val = val.Trim();
            TryParseULongFromEnd(val, eqIndex + 1, 20, out ulong result);
            fm.SizeBytes = result;
        }

        private static void FMData_Rating_Set(FanMission fm, string val, int eqIndex)
        {
            val = val.Trim();
            bool success = TryParseIntFromEnd(val, eqIndex + 1, 2, out int result);
            fm.Rating = success ? result : -1;
        }

        private static void FMData_ReleaseDate_Set(FanMission fm, string val, int eqIndex)
        {
            val = val.Substring(eqIndex + 1);
            val = val.Trim();
            fm.ReleaseDate.UnixDateString = val;
        }

        private static void FMData_LastPlayed_Set(FanMission fm, string val, int eqIndex)
        {
            val = val.Substring(eqIndex + 1);
            val = val.Trim();
            fm.LastPlayed.UnixDateString = val;
        }

        private static void FMData_DateAdded_Set(FanMission fm, string val, int eqIndex)
        {
            val = val.Substring(eqIndex + 1);
            val = val.Trim();
            // PERF: Don't convert to local here; do it at display-time
            fm.DateAdded = ConvertHexUnixDateToDateTime(val, convertToLocal: false);
        }

        private static void FMData_FinishedOn_Set(FanMission fm, string val, int eqIndex)
        {
            val = val.Trim();
            TryParseUIntFromEnd(val, eqIndex + 1, 2, out uint result);
            fm.FinishedOn = result;
        }

        private static void FMData_FinishedOnUnknown_Set(FanMission fm, string val, int eqIndex)
        {
            val = val.Trim();
            fm.FinishedOnUnknown = val.EndEqualsTrue(eqIndex + 1);
        }

        private static void FMData_Comment_Set(FanMission fm, string val, int eqIndex)
        {
            val = val.Substring(eqIndex + 1);
            // We require this value to be untrimmed
            fm.Comment = val;
        }

        private static void FMData_DisabledMods_Set(FanMission fm, string val, int eqIndex)
        {
            val = val.Substring(eqIndex + 1);
            val = val.Trim();
            fm.DisabledMods = val;
        }

        private static void FMData_DisableAllMods_Set(FanMission fm, string val, int eqIndex)
        {
            val = val.Trim();
            fm.DisableAllMods = val.EndEqualsTrue(eqIndex + 1);
        }

        private static void FMData_HasResources_Set(FanMission fm, string val, int eqIndex)
        {
            val = val.Trim();
            fm.ResourcesScanned = !val.ValueEqualsI("NotScanned", eqIndex + 1);
            FillFMHasXFields(fm, val, eqIndex + 1);
        }

        private static void FMData_LangsScanned_Set(FanMission fm, string val, int eqIndex)
        {
            val = val.Trim();
            fm.LangsScanned = val.EndEqualsTrue(eqIndex + 1);
        }

        private static void FMData_Langs_Set(FanMission fm, string val, int eqIndex)
        {
            val = val.Trim();
            SetFMLanguages(fm, val, eqIndex + 1);
        }

        private static void FMData_SelectedLang_Set(FanMission fm, string val, int eqIndex)
        {
            val = val.Trim();
            if (Langs_TryGetValue(val, eqIndex + 1, val.Length, out var result))
            {
                fm.SelectedLang = result;
            }
        }

        private static void FMData_TagsString_Set(FanMission fm, string val, int eqIndex)
        {
            val = val.Substring(eqIndex + 1);
            val = val.Trim();
            fm.TagsString = val;
        }

        private static void FMData_NewMantle_Set(FanMission fm, string val, int eqIndex)
        {
            val = val.Trim();
            fm.NewMantle = val.EndEqualsTrue(eqIndex + 1) ? true : val.EndEqualsFalse(eqIndex + 1) ? false : (bool?)null;
        }

        private static void FMData_MisCount_Set(FanMission fm, string val, int eqIndex)
        {
            val = val.Trim();
            bool success = TryParseIntFromEnd(val, eqIndex + 1, 10, out int result);
            fm.MisCount = success ? result : -1;
        }

        #region Old resource format - backward compatibility, we still have to be able to read it

        private static void FMData_HasMap_Set(FanMission fm, string val, int eqIndex)
        {
            SetFMResource(fm, CustomResources.Map, val.EndEqualsTrue(eqIndex + 1));
            fm.ResourcesScanned = true;
        }

        private static void FMData_HasAutomap_Set(FanMission fm, string val, int eqIndex)
        {
            SetFMResource(fm, CustomResources.Automap, val.EndEqualsTrue(eqIndex + 1));
            fm.ResourcesScanned = true;
        }

        private static void FMData_HasScripts_Set(FanMission fm, string val, int eqIndex)
        {
            SetFMResource(fm, CustomResources.Scripts, val.EndEqualsTrue(eqIndex + 1));
            fm.ResourcesScanned = true;
        }

        private static void FMData_HasTextures_Set(FanMission fm, string val, int eqIndex)
        {
            SetFMResource(fm, CustomResources.Textures, val.EndEqualsTrue(eqIndex + 1));
            fm.ResourcesScanned = true;
        }

        private static void FMData_HasSounds_Set(FanMission fm, string val, int eqIndex)
        {
            SetFMResource(fm, CustomResources.Sounds, val.EndEqualsTrue(eqIndex + 1));
            fm.ResourcesScanned = true;
        }

        private static void FMData_HasObjects_Set(FanMission fm, string val, int eqIndex)
        {
            SetFMResource(fm, CustomResources.Objects, val.EndEqualsTrue(eqIndex + 1));
            fm.ResourcesScanned = true;
        }

        private static void FMData_HasCreatures_Set(FanMission fm, string val, int eqIndex)
        {
            SetFMResource(fm, CustomResources.Creatures, val.EndEqualsTrue(eqIndex + 1));
            fm.ResourcesScanned = true;
        }

        private static void FMData_HasMotions_Set(FanMission fm, string val, int eqIndex)
        {
            SetFMResource(fm, CustomResources.Motions, val.EndEqualsTrue(eqIndex + 1));
            fm.ResourcesScanned = true;
        }

        private static void FMData_HasMovies_Set(FanMission fm, string val, int eqIndex)
        {
            SetFMResource(fm, CustomResources.Movies, val.EndEqualsTrue(eqIndex + 1));
            fm.ResourcesScanned = true;
        }

        private static void FMData_HasSubtitles_Set(FanMission fm, string val, int eqIndex)
        {
            SetFMResource(fm, CustomResources.Subtitles, val.EndEqualsTrue(eqIndex + 1));
            fm.ResourcesScanned = true;
        }

        #endregion

        private sealed unsafe class FMData_DelegatePointerWrapper
        {
            internal readonly delegate*<FanMission, string, int, void> Action;

            internal FMData_DelegatePointerWrapper(delegate*<FanMission, string, int, void> action)
            {
                Action = action;
            }
        }

        private static readonly unsafe Dictionary<string, FMData_DelegatePointerWrapper> _actionDict_FMData = new(new KeyComparer())
        {
            { "NoArchive", new FMData_DelegatePointerWrapper(&FMData_NoArchive_Set) },
            { "MarkedScanned", new FMData_DelegatePointerWrapper(&FMData_MarkedScanned_Set) },
            { "Pinned", new FMData_DelegatePointerWrapper(&FMData_Pinned_Set) },
            { "Archive", new FMData_DelegatePointerWrapper(&FMData_Archive_Set) },
            { "InstalledDir", new FMData_DelegatePointerWrapper(&FMData_InstalledDir_Set) },
            { "Title", new FMData_DelegatePointerWrapper(&FMData_Title_Set) },
            { "AltTitles", new FMData_DelegatePointerWrapper(&FMData_AltTitles_Set) },
            { "Author", new FMData_DelegatePointerWrapper(&FMData_Author_Set) },
            { "Game", new FMData_DelegatePointerWrapper(&FMData_Game_Set) },
            { "Installed", new FMData_DelegatePointerWrapper(&FMData_Installed_Set) },
            { "NoReadmes", new FMData_DelegatePointerWrapper(&FMData_NoReadmes_Set) },
            { "SelectedReadme", new FMData_DelegatePointerWrapper(&FMData_SelectedReadme_Set) },
            { "ReadmeEncoding", new FMData_DelegatePointerWrapper(&FMData_ReadmeEncoding_Set) },
            { "SizeBytes", new FMData_DelegatePointerWrapper(&FMData_SizeBytes_Set) },
            { "Rating", new FMData_DelegatePointerWrapper(&FMData_Rating_Set) },
            { "ReleaseDate", new FMData_DelegatePointerWrapper(&FMData_ReleaseDate_Set) },
            { "LastPlayed", new FMData_DelegatePointerWrapper(&FMData_LastPlayed_Set) },
            { "DateAdded", new FMData_DelegatePointerWrapper(&FMData_DateAdded_Set) },
            { "FinishedOn", new FMData_DelegatePointerWrapper(&FMData_FinishedOn_Set) },
            { "FinishedOnUnknown", new FMData_DelegatePointerWrapper(&FMData_FinishedOnUnknown_Set) },
            { "Comment", new FMData_DelegatePointerWrapper(&FMData_Comment_Set) },
            { "DisabledMods", new FMData_DelegatePointerWrapper(&FMData_DisabledMods_Set) },
            { "DisableAllMods", new FMData_DelegatePointerWrapper(&FMData_DisableAllMods_Set) },
            { "HasResources", new FMData_DelegatePointerWrapper(&FMData_HasResources_Set) },
            { "LangsScanned", new FMData_DelegatePointerWrapper(&FMData_LangsScanned_Set) },
            { "Langs", new FMData_DelegatePointerWrapper(&FMData_Langs_Set) },
            { "SelectedLang", new FMData_DelegatePointerWrapper(&FMData_SelectedLang_Set) },
            { "TagsString", new FMData_DelegatePointerWrapper(&FMData_TagsString_Set) },
            { "NewMantle", new FMData_DelegatePointerWrapper(&FMData_NewMantle_Set) },
            { "MisCount", new FMData_DelegatePointerWrapper(&FMData_MisCount_Set) },

            #region Old resource format - backward compatibility, we still have to be able to read it

            { "HasMap", new FMData_DelegatePointerWrapper(&FMData_HasMap_Set) },
            { "HasAutomap", new FMData_DelegatePointerWrapper(&FMData_HasAutomap_Set) },
            { "HasScripts", new FMData_DelegatePointerWrapper(&FMData_HasScripts_Set) },
            { "HasTextures", new FMData_DelegatePointerWrapper(&FMData_HasTextures_Set) },
            { "HasSounds", new FMData_DelegatePointerWrapper(&FMData_HasSounds_Set) },
            { "HasObjects", new FMData_DelegatePointerWrapper(&FMData_HasObjects_Set) },
            { "HasCreatures", new FMData_DelegatePointerWrapper(&FMData_HasCreatures_Set) },
            { "HasMotions", new FMData_DelegatePointerWrapper(&FMData_HasMotions_Set) },
            { "HasMovies", new FMData_DelegatePointerWrapper(&FMData_HasMovies_Set) },
            { "HasSubtitles", new FMData_DelegatePointerWrapper(&FMData_HasSubtitles_Set) }

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
                foreach (var item in fm.ReadmeCodePages)
                {
                    sb.Append("ReadmeEncoding=");
                    sb.Append(item.Key).Append(',').AppendLine(item.Value.ToString());
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
                if (fm.NewMantle != null)
                {
                    sb.Append("NewMantle=");
                    sb.AppendLine(fm.NewMantle.ToString());
                }
                if (fm.MisCount != -1)
                {
                    sb.Append("MisCount=");
                    sb.AppendLine(fm.MisCount.ToString());
                }
            }

            using var sw = new StreamWriter(fileName, false, Encoding.UTF8);
            sw.Write(sb.ToString());
        }

        #endregion
    }
}
