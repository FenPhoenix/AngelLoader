#define FenGen_FMDataDest

//#define write_old_resources_style

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using AL_Common;
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
        val = val.Substring(eqIndex + 1);
        val = val.Trim();
        int.TryParse(val, NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out int result);
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
        fm.DateAdded = ConvertHexUnixDateToDateTime(val);
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
        fm.ResourcesScanned = !val.ValueEqualsIAscii("NotScanned", eqIndex + 1);
        SetFMCustomResources(fm, val, eqIndex + 1);
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
        bool success = TryParseIntFromEnd(val, eqIndex + 1, 10, out int result);
        fm.MisCount = success ? result : -1;
    }

    private static void FMData_PlayTime_Set(FanMission fm, string val, int eqIndex)
    {
        val = val.Trim();
        TryParseLongFromEnd(val, eqIndex + 1, 19, out long result);
        fm.PlayTime = TimeSpan.FromTicks(result);
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

    [StructLayout(LayoutKind.Auto)]
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
        { "PlayTime", new FMData_DelegatePointerWrapper(&FMData_PlayTime_Set) },

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
        { "HasSubtitles", new FMData_DelegatePointerWrapper(&FMData_HasSubtitles_Set) },

        #endregion
    };

    #endregion

    #region Generated code for writer

    private static void WriteFMDataIni(List<FanMission> fmDataList, List<FanMission> fmDataListTDM, string fileName)
    {
        // Larger buffer size helps with perf for larger file sizes.
        using var fs = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.Read, ByteSize.KB * 256);
        using var sw = new StreamWriter(fs, Encoding.UTF8, ByteSize.KB * 256);

        static void AddFMToSW(FanMission fm, StreamWriter sw)
        {
            sw.WriteLine("[FM]");

            if (fm.NoArchive)
            {
                sw.WriteLine("NoArchive=True");
            }
            if (fm.MarkedScanned)
            {
                sw.WriteLine("MarkedScanned=True");
            }
            if (fm.Pinned)
            {
                sw.WriteLine("Pinned=True");
            }
            if (!string.IsNullOrEmpty(fm.Archive))
            {
                sw.Write("Archive=");
                sw.WriteLine(fm.Archive);
            }
            if (!string.IsNullOrEmpty(fm.InstalledDir))
            {
                sw.Write("InstalledDir=");
                sw.WriteLine(fm.InstalledDir);
            }
            if (!string.IsNullOrEmpty(fm.TDMInstalledDir))
            {
                sw.Write("TDMInstalledDir=");
                sw.WriteLine(fm.TDMInstalledDir);
            }
            if (fm.TDMVersion != 0)
            {
                sw.Write("TDMVersion=");
                sw.WriteLine(fm.TDMVersion.ToString(NumberFormatInfo.InvariantInfo));
            }
            if (!string.IsNullOrEmpty(fm.Title))
            {
                sw.Write("Title=");
                sw.WriteLine(fm.Title);
            }
            var list = fm.AltTitles;
            for (int i = 0; i < list.Count; i++)
            {
                var item = list[i];
                sw.Write("AltTitles=");
                sw.WriteLine(item);
            }
            if (!string.IsNullOrEmpty(fm.Author))
            {
                sw.Write("Author=");
                sw.WriteLine(fm.Author);
            }
            switch (fm.Game)
            {
                // Much faster to do this than Enum.ToString()
                case Game.Thief1:
                    sw.WriteLine("Game=Thief1");
                    break;
                case Game.Thief2:
                    sw.WriteLine("Game=Thief2");
                    break;
                case Game.Thief3:
                    sw.WriteLine("Game=Thief3");
                    break;
                case Game.SS2:
                    sw.WriteLine("Game=SS2");
                    break;
                case Game.TDM:
                    sw.WriteLine("Game=TDM");
                    break;
                case Game.Unsupported:
                    sw.WriteLine("Game=Unsupported");
                    break;
                    // Don't handle Game.Null because we don't want to write out defaults
            }
            if (fm.Installed)
            {
                sw.WriteLine("Installed=True");
            }
            if (fm.NoReadmes)
            {
                sw.WriteLine("NoReadmes=True");
            }
            if (fm.ForceReadmeReCache)
            {
                sw.WriteLine("ForceReadmeReCache=True");
            }
            if (!string.IsNullOrEmpty(fm.SelectedReadme))
            {
                sw.Write("SelectedReadme=");
                sw.WriteLine(fm.SelectedReadme);
            }
            if (fm.ReadmeCodePages.TryGetSingle(out var single))
            {
                sw.Write("ReadmeEncoding=");
                sw.Write(single.Key);
                sw.Write(',');
                sw.WriteLine(single.Value.ToString(NumberFormatInfo.InvariantInfo));
            }
            else if (fm.ReadmeCodePages.TryGetDictionary(out var dict))
            {
                foreach (var item in dict)
                {
                    sw.Write("ReadmeEncoding=");
                    sw.Write(item.Key);
                    sw.Write(',');
                    sw.WriteLine(item.Value.ToString(NumberFormatInfo.InvariantInfo));
                }
            }
            if (fm.SizeBytes != 0)
            {
                sw.Write("SizeBytes=");
                sw.WriteLine(fm.SizeBytes.ToString(NumberFormatInfo.InvariantInfo));
            }
            if (fm.Rating != -1)
            {
                sw.Write("Rating=");
                sw.WriteLine(fm.Rating.ToString(NumberFormatInfo.InvariantInfo));
            }
            if (!string.IsNullOrEmpty(fm.ReleaseDate.UnixDateString))
            {
                sw.Write("ReleaseDate=");
                sw.WriteLine(fm.ReleaseDate.UnixDateString);
            }
            if (!string.IsNullOrEmpty(fm.LastPlayed.UnixDateString))
            {
                sw.Write("LastPlayed=");
                sw.WriteLine(fm.LastPlayed.UnixDateString);
            }
            if (fm.DateAdded != null)
            {
                sw.Write("DateAdded=");
                sw.WriteLine(new DateTimeOffset((DateTime)fm.DateAdded).ToUnixTimeSeconds().ToString("X"));
            }
            if (fm.FinishedOn != 0)
            {
                sw.Write("FinishedOn=");
                sw.WriteLine(fm.FinishedOn.ToString(NumberFormatInfo.InvariantInfo));
            }
            if (fm.FinishedOnUnknown)
            {
                sw.WriteLine("FinishedOnUnknown=True");
            }
            if (!string.IsNullOrEmpty(fm.Comment))
            {
                sw.Write("Comment=");
                sw.WriteLine(fm.Comment);
            }
            if (!string.IsNullOrEmpty(fm.DisabledMods))
            {
                sw.Write("DisabledMods=");
                sw.WriteLine(fm.DisabledMods);
            }
            if (fm.DisableAllMods)
            {
                sw.WriteLine("DisableAllMods=True");
            }
#if write_old_resources_style
            if (fm.ResourcesScanned)
            {
                sw.WriteLine("HasMap=" + fm.HasResource(CustomResources.Map).ToString());
                sw.WriteLine("HasAutomap=" + fm.HasResource(CustomResources.Automap).ToString());
                sw.WriteLine("HasScripts=" + fm.HasResource(CustomResources.Scripts).ToString());
                sw.WriteLine("HasTextures=" + fm.HasResource(CustomResources.Textures).ToString());
                sw.WriteLine("HasSounds=" + fm.HasResource(CustomResources.Sounds).ToString());
                sw.WriteLine("HasObjects=" + fm.HasResource(CustomResources.Objects).ToString());
                sw.WriteLine("HasCreatures=" + fm.HasResource(CustomResources.Creatures).ToString());
                sw.WriteLine("HasMotions=" + fm.HasResource(CustomResources.Motions).ToString());
                sw.WriteLine("HasMovies=" + fm.HasResource(CustomResources.Movies).ToString());
                sw.WriteLine("HasSubtitles=" + fm.HasResource(CustomResources.Subtitles).ToString());
            }
#else
            sw.Write("HasResources=");
            if (fm.ResourcesScanned)
            {
                CommaCombineCustomResources(fm.Resources, sw);
            }
            else
            {
                sw.WriteLine("NotScanned");
            }
#endif
            if (fm.LangsScanned)
            {
                sw.WriteLine("LangsScanned=True");
            }
            if (fm.Langs != 0)
            {
                sw.Write("Langs=");
                CommaCombineLanguageFlags(sw, fm.Langs);
            }
            switch (fm.SelectedLang)
            {
                // Much faster to do this than Enum.ToString()
                case Language.English:
                    sw.WriteLine("SelectedLang=english");
                    break;
                case Language.Czech:
                    sw.WriteLine("SelectedLang=czech");
                    break;
                case Language.Dutch:
                    sw.WriteLine("SelectedLang=dutch");
                    break;
                case Language.French:
                    sw.WriteLine("SelectedLang=french");
                    break;
                case Language.German:
                    sw.WriteLine("SelectedLang=german");
                    break;
                case Language.Hungarian:
                    sw.WriteLine("SelectedLang=hungarian");
                    break;
                case Language.Italian:
                    sw.WriteLine("SelectedLang=italian");
                    break;
                case Language.Japanese:
                    sw.WriteLine("SelectedLang=japanese");
                    break;
                case Language.Polish:
                    sw.WriteLine("SelectedLang=polish");
                    break;
                case Language.Russian:
                    sw.WriteLine("SelectedLang=russian");
                    break;
                case Language.Spanish:
                    sw.WriteLine("SelectedLang=spanish");
                    break;
                    // Don't handle Language.Default because we don't want to write out defaults
            }
            if (!string.IsNullOrEmpty(fm.TagsString))
            {
                sw.Write("TagsString=");
                sw.WriteLine(fm.TagsString);
            }
            if (fm.NewMantle != null)
            {
                sw.Write("NewMantle=");
                sw.WriteLine(fm.NewMantle == true ? bool.TrueString : bool.FalseString);
            }
            if (fm.PostProc != null)
            {
                sw.Write("PostProc=");
                sw.WriteLine(fm.PostProc == true ? bool.TrueString : bool.FalseString);
            }
            if (fm.NDSubs != null)
            {
                sw.Write("NDSubs=");
                sw.WriteLine(fm.NDSubs == true ? bool.TrueString : bool.FalseString);
            }
            if (fm.MisCount != -1)
            {
                sw.Write("MisCount=");
                sw.WriteLine(fm.MisCount.ToString(NumberFormatInfo.InvariantInfo));
            }
            if (fm.PlayTime.Ticks != 0)
            {
                sw.Write("PlayTime=");
                sw.WriteLine(fm.PlayTime.Ticks.ToString(NumberFormatInfo.InvariantInfo));
            }
        }

        foreach (FanMission fm in fmDataList)
        {
            AddFMToSW(fm, sw);
        }

        foreach (FanMission fm in fmDataListTDM)
        {
            AddFMToSW(fm, sw);
        }
    }

    #endregion
}
