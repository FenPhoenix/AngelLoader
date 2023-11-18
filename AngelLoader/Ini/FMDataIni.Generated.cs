#define FenGen_FMDataDest

//#define write_old_resources_style

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using AngelLoader.DataClasses;
using static AL_Common.Common;
using static AL_Common.FenGenAttributes;
using static AL_Common.LanguageSupport;
using static AngelLoader.GameSupport;
using static AngelLoader.Utils;

namespace AngelLoader;

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

    private static void FMData_TDMInstalledDir_Set(FanMission fm, string val, int eqIndex)
    {
        val = val.Substring(eqIndex + 1);
        val = val.Trim();
        fm.TDMInstalledDir = val;
    }

    private static void FMData_TDMVersion_Set(FanMission fm, string val, int eqIndex)
    {
        val = val.Trim();
        int.TryParse(val.AsSpan()[(eqIndex + 1)..], NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out int result);
        fm.TDMVersion = result;
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
        if (val.ValueEqualsIAscii("Thief1", eqIndex + 1))
        {
            fm.Game = Game.Thief1;
        }
        else if (val.ValueEqualsIAscii("Thief2", eqIndex + 1))
        {
            fm.Game = Game.Thief2;
        }
        else if (val.ValueEqualsIAscii("Thief3", eqIndex + 1))
        {
            fm.Game = Game.Thief3;
        }
        else if (val.ValueEqualsIAscii("SS2", eqIndex + 1))
        {
            fm.Game = Game.SS2;
        }
        else if (val.ValueEqualsIAscii("TDM", eqIndex + 1))
        {
            fm.Game = Game.TDM;
        }
        else if (val.ValueEqualsIAscii("Unsupported", eqIndex + 1))
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

    private static void FMData_ForceReadmeReCache_Set(FanMission fm, string val, int eqIndex)
    {
        val = val.Trim();
        fm.ForceReadmeReCache = val.EndEqualsTrue(eqIndex + 1);
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
        ulong.TryParse(val.AsSpan()[(eqIndex + 1)..], NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out ulong result);
        fm.SizeBytes = result;
    }

    private static void FMData_Rating_Set(FanMission fm, string val, int eqIndex)
    {
        val = val.Trim();
        bool success = int.TryParse(val.AsSpan()[(eqIndex + 1)..], NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out int result);
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
        fm.DateAdded = ConvertHexUnixDateToDateTime(val);
    }

    private static void FMData_FinishedOn_Set(FanMission fm, string val, int eqIndex)
    {
        val = val.Trim();
        uint.TryParse(val.AsSpan()[(eqIndex + 1)..], NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out uint result);
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
        fm.ResourcesScanned = !val.ValueEqualsIAscii("NotScanned", eqIndex + 1);
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

    private static void FMData_PostProc_Set(FanMission fm, string val, int eqIndex)
    {
        val = val.Trim();
        fm.PostProc = val.EndEqualsTrue(eqIndex + 1) ? true : val.EndEqualsFalse(eqIndex + 1) ? false : (bool?)null;
    }

    private static void FMData_NDSubs_Set(FanMission fm, string val, int eqIndex)
    {
        val = val.Trim();
        fm.NDSubs = val.EndEqualsTrue(eqIndex + 1) ? true : val.EndEqualsFalse(eqIndex + 1) ? false : (bool?)null;
    }

    private static void FMData_MisCount_Set(FanMission fm, string val, int eqIndex)
    {
        val = val.Trim();
        bool success = int.TryParse(val.AsSpan()[(eqIndex + 1)..], NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out int result);
        fm.MisCount = success ? result : -1;
    }

    #region Old resource format - backward compatibility, we still have to be able to read it

    private static void FMData_HasMap_Set(FanMission fm, string val, int eqIndex)
    {
        fm.SetResource(CustomResources.Map, val.EndEqualsTrue(eqIndex + 1));
        fm.ResourcesScanned = true;
    }

    private static void FMData_HasAutomap_Set(FanMission fm, string val, int eqIndex)
    {
        fm.SetResource(CustomResources.Automap, val.EndEqualsTrue(eqIndex + 1));
        fm.ResourcesScanned = true;
    }

    private static void FMData_HasScripts_Set(FanMission fm, string val, int eqIndex)
    {
        fm.SetResource(CustomResources.Scripts, val.EndEqualsTrue(eqIndex + 1));
        fm.ResourcesScanned = true;
    }

    private static void FMData_HasTextures_Set(FanMission fm, string val, int eqIndex)
    {
        fm.SetResource(CustomResources.Textures, val.EndEqualsTrue(eqIndex + 1));
        fm.ResourcesScanned = true;
    }

    private static void FMData_HasSounds_Set(FanMission fm, string val, int eqIndex)
    {
        fm.SetResource(CustomResources.Sounds, val.EndEqualsTrue(eqIndex + 1));
        fm.ResourcesScanned = true;
    }

    private static void FMData_HasObjects_Set(FanMission fm, string val, int eqIndex)
    {
        fm.SetResource(CustomResources.Objects, val.EndEqualsTrue(eqIndex + 1));
        fm.ResourcesScanned = true;
    }

    private static void FMData_HasCreatures_Set(FanMission fm, string val, int eqIndex)
    {
        fm.SetResource(CustomResources.Creatures, val.EndEqualsTrue(eqIndex + 1));
        fm.ResourcesScanned = true;
    }

    private static void FMData_HasMotions_Set(FanMission fm, string val, int eqIndex)
    {
        fm.SetResource(CustomResources.Motions, val.EndEqualsTrue(eqIndex + 1));
        fm.ResourcesScanned = true;
    }

    private static void FMData_HasMovies_Set(FanMission fm, string val, int eqIndex)
    {
        fm.SetResource(CustomResources.Movies, val.EndEqualsTrue(eqIndex + 1));
        fm.ResourcesScanned = true;
    }

    private static void FMData_HasSubtitles_Set(FanMission fm, string val, int eqIndex)
    {
        fm.SetResource(CustomResources.Subtitles, val.EndEqualsTrue(eqIndex + 1));
        fm.ResourcesScanned = true;
    }

    #endregion

    private readonly unsafe struct FMData_DelegatePointerWrapper
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
        { "TDMInstalledDir", new FMData_DelegatePointerWrapper(&FMData_TDMInstalledDir_Set) },
        { "TDMVersion", new FMData_DelegatePointerWrapper(&FMData_TDMVersion_Set) },
        { "Title", new FMData_DelegatePointerWrapper(&FMData_Title_Set) },
        { "AltTitles", new FMData_DelegatePointerWrapper(&FMData_AltTitles_Set) },
        { "Author", new FMData_DelegatePointerWrapper(&FMData_Author_Set) },
        { "Game", new FMData_DelegatePointerWrapper(&FMData_Game_Set) },
        { "Installed", new FMData_DelegatePointerWrapper(&FMData_Installed_Set) },
        { "NoReadmes", new FMData_DelegatePointerWrapper(&FMData_NoReadmes_Set) },
        { "ForceReadmeReCache", new FMData_DelegatePointerWrapper(&FMData_ForceReadmeReCache_Set) },
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
        { "PostProc", new FMData_DelegatePointerWrapper(&FMData_PostProc_Set) },
        { "NDSubs", new FMData_DelegatePointerWrapper(&FMData_NDSubs_Set) },
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

    private static void WriteFMDataIni(List<FanMission> fmDataList, List<FanMission> fmDataListTDM, string fileName)
    {
        var sb = new StringBuilder();

        static void AddFMToSB(FanMission fm, StringBuilder sb)
        {
            sb.AppendLine("[FM]");

            if (fm.NoArchive)
            {
                sb.Append("NoArchive").AppendLine("=True");
            }
            if (fm.MarkedScanned)
            {
                sb.Append("MarkedScanned").AppendLine("=True");
            }
            if (fm.Pinned)
            {
                sb.Append("Pinned").AppendLine("=True");
            }
            if (!string.IsNullOrEmpty(fm.Archive))
            {
                sb.Append("Archive").Append('=');
                sb.AppendLine(fm.Archive);
            }
            if (!string.IsNullOrEmpty(fm.InstalledDir))
            {
                sb.Append("InstalledDir").Append('=');
                sb.AppendLine(fm.InstalledDir);
            }
            if (!string.IsNullOrEmpty(fm.TDMInstalledDir))
            {
                sb.Append("TDMInstalledDir").Append('=');
                sb.AppendLine(fm.TDMInstalledDir);
            }
            if (fm.TDMVersion != 0)
            {
                sb.Append("TDMVersion").Append('=');
                sb.AppendLine(fm.TDMVersion.ToString());
            }
            if (!string.IsNullOrEmpty(fm.Title))
            {
                sb.Append("Title").Append('=');
                sb.AppendLine(fm.Title);
            }
            foreach (string s in fm.AltTitles)
            {
                sb.Append("AltTitles").Append('=');
                sb.AppendLine(s);
            }
            if (!string.IsNullOrEmpty(fm.Author))
            {
                sb.Append("Author").Append('=');
                sb.AppendLine(fm.Author);
            }
            switch (fm.Game)
            {
                // Much faster to do this than Enum.ToString()
                case Game.Thief1:
                    sb.Append("Game").Append('=').AppendLine("Thief1");
                    break;
                case Game.Thief2:
                    sb.Append("Game").Append('=').AppendLine("Thief2");
                    break;
                case Game.Thief3:
                    sb.Append("Game").Append('=').AppendLine("Thief3");
                    break;
                case Game.SS2:
                    sb.Append("Game").Append('=').AppendLine("SS2");
                    break;
                case Game.TDM:
                    sb.Append("Game").Append('=').AppendLine("TDM");
                    break;
                case Game.Unsupported:
                    sb.Append("Game").Append('=').AppendLine("Unsupported");
                    break;
                    // Don't handle Game.Null because we don't want to write out defaults
            }
            if (fm.Installed)
            {
                sb.Append("Installed").AppendLine("=True");
            }
            if (fm.NoReadmes)
            {
                sb.Append("NoReadmes").AppendLine("=True");
            }
            if (fm.ForceReadmeReCache)
            {
                sb.Append("ForceReadmeReCache").AppendLine("=True");
            }
            if (!string.IsNullOrEmpty(fm.SelectedReadme))
            {
                sb.Append("SelectedReadme").Append('=');
                sb.AppendLine(fm.SelectedReadme);
            }
            foreach (var item in fm.ReadmeCodePages)
            {
                sb.Append("ReadmeEncoding").Append('=');
                sb.Append(item.Key).Append(',').AppendLine(item.Value.ToString());
            }
            if (fm.SizeBytes != 0)
            {
                sb.Append("SizeBytes").Append('=');
                sb.AppendLine(fm.SizeBytes.ToString());
            }
            if (fm.Rating != -1)
            {
                sb.Append("Rating").Append('=');
                sb.AppendLine(fm.Rating.ToString());
            }
            if (!string.IsNullOrEmpty(fm.ReleaseDate.UnixDateString))
            {
                sb.Append("ReleaseDate").Append('=');
                sb.AppendLine(fm.ReleaseDate.UnixDateString);
            }
            if (!string.IsNullOrEmpty(fm.LastPlayed.UnixDateString))
            {
                sb.Append("LastPlayed").Append('=');
                sb.AppendLine(fm.LastPlayed.UnixDateString);
            }
            if (fm.DateAdded != null)
            {
                sb.Append("DateAdded").Append('=');
                sb.AppendLine(new DateTimeOffset((DateTime)fm.DateAdded).ToUnixTimeSeconds().ToString("X"));
            }
            if (fm.FinishedOn != 0)
            {
                sb.Append("FinishedOn").Append('=');
                sb.AppendLine(fm.FinishedOn.ToString());
            }
            if (fm.FinishedOnUnknown)
            {
                sb.Append("FinishedOnUnknown").AppendLine("=True");
            }
            if (!string.IsNullOrEmpty(fm.Comment))
            {
                sb.Append("Comment").Append('=');
                sb.AppendLine(fm.Comment);
            }
            if (!string.IsNullOrEmpty(fm.DisabledMods))
            {
                sb.Append("DisabledMods").Append('=');
                sb.AppendLine(fm.DisabledMods);
            }
            if (fm.DisableAllMods)
            {
                sb.Append("DisableAllMods").AppendLine("=True");
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
            sb.Append("HasResources").Append('=');
            if (fm.ResourcesScanned)
            {
                CommaCombineHasXFields(fm.Resources, sb);
            }
            else
            {
                sb.AppendLine("NotScanned");
            }
#endif
            if (fm.LangsScanned)
            {
                sb.Append("LangsScanned").AppendLine("=True");
            }
            if (fm.Langs != 0)
            {
                sb.Append("Langs").Append('=');
                CommaCombineLanguageFlags(sb, fm.Langs);
            }
            switch (fm.SelectedLang)
            {
                // Much faster to do this than Enum.ToString()
                case Language.English:
                    sb.Append("SelectedLang").Append('=').AppendLine("english");
                    break;
                case Language.Czech:
                    sb.Append("SelectedLang").Append('=').AppendLine("czech");
                    break;
                case Language.Dutch:
                    sb.Append("SelectedLang").Append('=').AppendLine("dutch");
                    break;
                case Language.French:
                    sb.Append("SelectedLang").Append('=').AppendLine("french");
                    break;
                case Language.German:
                    sb.Append("SelectedLang").Append('=').AppendLine("german");
                    break;
                case Language.Hungarian:
                    sb.Append("SelectedLang").Append('=').AppendLine("hungarian");
                    break;
                case Language.Italian:
                    sb.Append("SelectedLang").Append('=').AppendLine("italian");
                    break;
                case Language.Japanese:
                    sb.Append("SelectedLang").Append('=').AppendLine("japanese");
                    break;
                case Language.Polish:
                    sb.Append("SelectedLang").Append('=').AppendLine("polish");
                    break;
                case Language.Russian:
                    sb.Append("SelectedLang").Append('=').AppendLine("russian");
                    break;
                case Language.Spanish:
                    sb.Append("SelectedLang").Append('=').AppendLine("spanish");
                    break;
                    // Don't handle Language.Default because we don't want to write out defaults
            }
            if (!string.IsNullOrEmpty(fm.TagsString))
            {
                sb.Append("TagsString").Append('=');
                sb.AppendLine(fm.TagsString);
            }
            if (fm.NewMantle != null)
            {
                sb.Append("NewMantle").Append('=');
                sb.AppendLine(fm.NewMantle.ToString());
            }
            if (fm.PostProc != null)
            {
                sb.Append("PostProc").Append('=');
                sb.AppendLine(fm.PostProc.ToString());
            }
            if (fm.NDSubs != null)
            {
                sb.Append("NDSubs").Append('=');
                sb.AppendLine(fm.NDSubs.ToString());
            }
            if (fm.MisCount != -1)
            {
                sb.Append("MisCount").Append('=');
                sb.AppendLine(fm.MisCount.ToString());
            }
        }

        foreach (FanMission fm in fmDataList)
        {
            AddFMToSB(fm, sb);
        }

        foreach (FanMission fm in fmDataListTDM)
        {
            AddFMToSB(fm, sb);
        }

        using var sw = new StreamWriter(fileName, false, Encoding.UTF8);
        sw.Write(sb.ToString());
    }

    #endregion
}
