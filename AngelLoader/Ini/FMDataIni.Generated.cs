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

    private static void FMData_NoArchive_Set(FanMission fm, ReadOnlySpan<char> val)
    {
        val = val.Trim();
        fm.NoArchive = val.EqualsTrue();
    }

    private static void FMData_MarkedScanned_Set(FanMission fm, ReadOnlySpan<char> val)
    {
        val = val.Trim();
        fm.MarkedScanned = val.EqualsTrue();
    }

    private static void FMData_Pinned_Set(FanMission fm, ReadOnlySpan<char> val)
    {
        val = val.Trim();
        fm.Pinned = val.EqualsTrue();
    }

    private static void FMData_Archive_Set(FanMission fm, ReadOnlySpan<char> val)
    {
        val = val.Trim();
        fm.Archive = val.ToString();
    }

    private static void FMData_InstalledDir_Set(FanMission fm, ReadOnlySpan<char> val)
    {
        val = val.Trim();
        fm.InstalledDir = val.ToString();
    }

    private static void FMData_TDMInstalledDir_Set(FanMission fm, ReadOnlySpan<char> val)
    {
        val = val.Trim();
        fm.TDMInstalledDir = val.ToString();
    }

    private static void FMData_TDMVersion_Set(FanMission fm, ReadOnlySpan<char> val)
    {
        val = val.Trim();
        int.TryParse(val, NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out int result);
        fm.TDMVersion = result;
    }

    private static void FMData_Title_Set(FanMission fm, ReadOnlySpan<char> val)
    {
        val = val.Trim();
        fm.Title = val.ToString();
    }

    private static void FMData_AltTitles_Set(FanMission fm, ReadOnlySpan<char> val)
    {
        val = val.Trim();
        if (!val.IsEmpty)
        {
            fm.AltTitles.Add(val.ToString());
        }
    }

    private static void FMData_Author_Set(FanMission fm, ReadOnlySpan<char> val)
    {
        val = val.Trim();
        fm.Author = val.ToString();
    }

    private static void FMData_Game_Set(FanMission fm, ReadOnlySpan<char> val)
    {
        val = val.Trim();
        if (val.EqualsI("Thief1"))
        {
            fm.Game = Game.Thief1;
        }
        else if (val.EqualsI("Thief2"))
        {
            fm.Game = Game.Thief2;
        }
        else if (val.EqualsI("Thief3"))
        {
            fm.Game = Game.Thief3;
        }
        else if (val.EqualsI("SS2"))
        {
            fm.Game = Game.SS2;
        }
        else if (val.EqualsI("TDM"))
        {
            fm.Game = Game.TDM;
        }
        else if (val.EqualsI("Unsupported"))
        {
            fm.Game = Game.Unsupported;
        }
        else
        {
            fm.Game = Game.Null;
        }
    }

    private static void FMData_Installed_Set(FanMission fm, ReadOnlySpan<char> val)
    {
        val = val.Trim();
        fm.Installed = val.EqualsTrue();
    }

    private static void FMData_NoReadmes_Set(FanMission fm, ReadOnlySpan<char> val)
    {
        val = val.Trim();
        fm.NoReadmes = val.EqualsTrue();
    }

    private static void FMData_ForceReadmeReCache_Set(FanMission fm, ReadOnlySpan<char> val)
    {
        val = val.Trim();
        fm.ForceReadmeReCache = val.EqualsTrue();
    }

    private static void FMData_SelectedReadme_Set(FanMission fm, ReadOnlySpan<char> val)
    {
        val = val.Trim();
        fm.SelectedReadme = val.ToString();
    }

    private static void FMData_ReadmeEncoding_Set(FanMission fm, ReadOnlySpan<char> val)
    {
        val = val.Trim();
        AddReadmeEncoding(fm, val);
    }

    private static void FMData_SizeBytes_Set(FanMission fm, ReadOnlySpan<char> val)
    {
        val = val.Trim();
        ulong.TryParse(val, NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out ulong result);
        fm.SizeBytes = result;
    }

    private static void FMData_Rating_Set(FanMission fm, ReadOnlySpan<char> val)
    {
        val = val.Trim();
        bool success = int.TryParse(val, NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out int result);
        fm.Rating = success ? result : -1;
    }

    private static void FMData_ReleaseDate_Set(FanMission fm, ReadOnlySpan<char> val)
    {
        val = val.Trim();
        fm.ReleaseDate.UnixDateString = val.ToString();
    }

    private static void FMData_LastPlayed_Set(FanMission fm, ReadOnlySpan<char> val)
    {
        val = val.Trim();
        fm.LastPlayed.UnixDateString = val.ToString();
    }

    private static void FMData_DateAdded_Set(FanMission fm, ReadOnlySpan<char> val)
    {
        val = val.Trim();
        fm.DateAdded = ConvertHexUnixDateToDateTime(val);
    }

    private static void FMData_FinishedOn_Set(FanMission fm, ReadOnlySpan<char> val)
    {
        val = val.Trim();
        uint.TryParse(val, NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out uint result);
        fm.FinishedOn = result;
    }

    private static void FMData_FinishedOnUnknown_Set(FanMission fm, ReadOnlySpan<char> val)
    {
        val = val.Trim();
        fm.FinishedOnUnknown = val.EqualsTrue();
    }

    private static void FMData_Comment_Set(FanMission fm, ReadOnlySpan<char> val)
    {
        // We require this value to be untrimmed
        fm.Comment = val.ToString();
    }

    private static void FMData_DisabledMods_Set(FanMission fm, ReadOnlySpan<char> val)
    {
        val = val.Trim();
        fm.DisabledMods = val.ToString();
    }

    private static void FMData_DisableAllMods_Set(FanMission fm, ReadOnlySpan<char> val)
    {
        val = val.Trim();
        fm.DisableAllMods = val.EqualsTrue();
    }

    private static void FMData_HasResources_Set(FanMission fm, ReadOnlySpan<char> val)
    {
        val = val.Trim();
        fm.ResourcesScanned = !val.EqualsI("NotScanned");
        SetFMCustomResources(fm, val);
    }

    private static void FMData_LangsScanned_Set(FanMission fm, ReadOnlySpan<char> val)
    {
        val = val.Trim();
        fm.LangsScanned = val.EqualsTrue();
    }

    private static void FMData_Langs_Set(FanMission fm, ReadOnlySpan<char> val)
    {
        val = val.Trim();
        SetFMLanguages(fm, val);
    }

    private static void FMData_SelectedLang_Set(FanMission fm, ReadOnlySpan<char> val)
    {
        val = val.Trim();
        if (Langs_TryGetValue(val, 0, val.Length, out var result))
        {
            fm.SelectedLang = result;
        }
    }

    private static void FMData_TagsString_Set(FanMission fm, ReadOnlySpan<char> val)
    {
        val = val.Trim();
        fm.TagsString = val.ToString();
    }

    private static void FMData_NewMantle_Set(FanMission fm, ReadOnlySpan<char> val)
    {
        val = val.Trim();
        fm.NewMantle = val.EqualsTrue() ? true : val.EqualsFalse() ? false : (bool?)null;
    }

    private static void FMData_PostProc_Set(FanMission fm, ReadOnlySpan<char> val)
    {
        val = val.Trim();
        fm.PostProc = val.EqualsTrue() ? true : val.EqualsFalse() ? false : (bool?)null;
    }

    private static void FMData_NDSubs_Set(FanMission fm, ReadOnlySpan<char> val)
    {
        val = val.Trim();
        fm.NDSubs = val.EqualsTrue() ? true : val.EqualsFalse() ? false : (bool?)null;
    }

    private static void FMData_MisCount_Set(FanMission fm, ReadOnlySpan<char> val)
    {
        val = val.Trim();
        bool success = int.TryParse(val, NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out int result);
        fm.MisCount = success ? result : -1;
    }

    private static void FMData_PlayTime_Set(FanMission fm, ReadOnlySpan<char> val)
    {
        val = val.Trim();
        long.TryParse(val, NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out long result);
        fm.PlayTime = TimeSpan.FromTicks(result);
    }

    #region Old resource format - backward compatibility, we still have to be able to read it

    private static void FMData_HasMap_Set(FanMission fm, ReadOnlySpan<char> val)
    {
        fm.SetResource(CustomResources.Map, val.EqualsTrue());
        fm.ResourcesScanned = true;
    }

    private static void FMData_HasAutomap_Set(FanMission fm, ReadOnlySpan<char> val)
    {
        fm.SetResource(CustomResources.Automap, val.EqualsTrue());
        fm.ResourcesScanned = true;
    }

    private static void FMData_HasScripts_Set(FanMission fm, ReadOnlySpan<char> val)
    {
        fm.SetResource(CustomResources.Scripts, val.EqualsTrue());
        fm.ResourcesScanned = true;
    }

    private static void FMData_HasTextures_Set(FanMission fm, ReadOnlySpan<char> val)
    {
        fm.SetResource(CustomResources.Textures, val.EqualsTrue());
        fm.ResourcesScanned = true;
    }

    private static void FMData_HasSounds_Set(FanMission fm, ReadOnlySpan<char> val)
    {
        fm.SetResource(CustomResources.Sounds, val.EqualsTrue());
        fm.ResourcesScanned = true;
    }

    private static void FMData_HasObjects_Set(FanMission fm, ReadOnlySpan<char> val)
    {
        fm.SetResource(CustomResources.Objects, val.EqualsTrue());
        fm.ResourcesScanned = true;
    }

    private static void FMData_HasCreatures_Set(FanMission fm, ReadOnlySpan<char> val)
    {
        fm.SetResource(CustomResources.Creatures, val.EqualsTrue());
        fm.ResourcesScanned = true;
    }

    private static void FMData_HasMotions_Set(FanMission fm, ReadOnlySpan<char> val)
    {
        fm.SetResource(CustomResources.Motions, val.EqualsTrue());
        fm.ResourcesScanned = true;
    }

    private static void FMData_HasMovies_Set(FanMission fm, ReadOnlySpan<char> val)
    {
        fm.SetResource(CustomResources.Movies, val.EqualsTrue());
        fm.ResourcesScanned = true;
    }

    private static void FMData_HasSubtitles_Set(FanMission fm, ReadOnlySpan<char> val)
    {
        fm.SetResource(CustomResources.Subtitles, val.EqualsTrue());
        fm.ResourcesScanned = true;
    }

    #endregion

    private readonly unsafe struct FMData_DelegatePointerWrapper
    {
        internal readonly delegate*<FanMission, ReadOnlySpan<char>, void> Action;

        internal FMData_DelegatePointerWrapper(delegate*<FanMission, ReadOnlySpan<char>, void> action)
        {
            Action = action;
        }
    }

    private static readonly unsafe Dictionary<ReadOnlyMemory<char>, FMData_DelegatePointerWrapper> _actionDict_FMData = new(new MemoryStringComparer())
    {
        { "NoArchive".AsMemory(), new FMData_DelegatePointerWrapper(&FMData_NoArchive_Set) },
        { "MarkedScanned".AsMemory(), new FMData_DelegatePointerWrapper(&FMData_MarkedScanned_Set) },
        { "Pinned".AsMemory(), new FMData_DelegatePointerWrapper(&FMData_Pinned_Set) },
        { "Archive".AsMemory(), new FMData_DelegatePointerWrapper(&FMData_Archive_Set) },
        { "InstalledDir".AsMemory(), new FMData_DelegatePointerWrapper(&FMData_InstalledDir_Set) },
        { "TDMInstalledDir".AsMemory(), new FMData_DelegatePointerWrapper(&FMData_TDMInstalledDir_Set) },
        { "TDMVersion".AsMemory(), new FMData_DelegatePointerWrapper(&FMData_TDMVersion_Set) },
        { "Title".AsMemory(), new FMData_DelegatePointerWrapper(&FMData_Title_Set) },
        { "AltTitles".AsMemory(), new FMData_DelegatePointerWrapper(&FMData_AltTitles_Set) },
        { "Author".AsMemory(), new FMData_DelegatePointerWrapper(&FMData_Author_Set) },
        { "Game".AsMemory(), new FMData_DelegatePointerWrapper(&FMData_Game_Set) },
        { "Installed".AsMemory(), new FMData_DelegatePointerWrapper(&FMData_Installed_Set) },
        { "NoReadmes".AsMemory(), new FMData_DelegatePointerWrapper(&FMData_NoReadmes_Set) },
        { "ForceReadmeReCache".AsMemory(), new FMData_DelegatePointerWrapper(&FMData_ForceReadmeReCache_Set) },
        { "SelectedReadme".AsMemory(), new FMData_DelegatePointerWrapper(&FMData_SelectedReadme_Set) },
        { "ReadmeEncoding".AsMemory(), new FMData_DelegatePointerWrapper(&FMData_ReadmeEncoding_Set) },
        { "SizeBytes".AsMemory(), new FMData_DelegatePointerWrapper(&FMData_SizeBytes_Set) },
        { "Rating".AsMemory(), new FMData_DelegatePointerWrapper(&FMData_Rating_Set) },
        { "ReleaseDate".AsMemory(), new FMData_DelegatePointerWrapper(&FMData_ReleaseDate_Set) },
        { "LastPlayed".AsMemory(), new FMData_DelegatePointerWrapper(&FMData_LastPlayed_Set) },
        { "DateAdded".AsMemory(), new FMData_DelegatePointerWrapper(&FMData_DateAdded_Set) },
        { "FinishedOn".AsMemory(), new FMData_DelegatePointerWrapper(&FMData_FinishedOn_Set) },
        { "FinishedOnUnknown".AsMemory(), new FMData_DelegatePointerWrapper(&FMData_FinishedOnUnknown_Set) },
        { "Comment".AsMemory(), new FMData_DelegatePointerWrapper(&FMData_Comment_Set) },
        { "DisabledMods".AsMemory(), new FMData_DelegatePointerWrapper(&FMData_DisabledMods_Set) },
        { "DisableAllMods".AsMemory(), new FMData_DelegatePointerWrapper(&FMData_DisableAllMods_Set) },
        { "HasResources".AsMemory(), new FMData_DelegatePointerWrapper(&FMData_HasResources_Set) },
        { "LangsScanned".AsMemory(), new FMData_DelegatePointerWrapper(&FMData_LangsScanned_Set) },
        { "Langs".AsMemory(), new FMData_DelegatePointerWrapper(&FMData_Langs_Set) },
        { "SelectedLang".AsMemory(), new FMData_DelegatePointerWrapper(&FMData_SelectedLang_Set) },
        { "TagsString".AsMemory(), new FMData_DelegatePointerWrapper(&FMData_TagsString_Set) },
        { "NewMantle".AsMemory(), new FMData_DelegatePointerWrapper(&FMData_NewMantle_Set) },
        { "PostProc".AsMemory(), new FMData_DelegatePointerWrapper(&FMData_PostProc_Set) },
        { "NDSubs".AsMemory(), new FMData_DelegatePointerWrapper(&FMData_NDSubs_Set) },
        { "MisCount".AsMemory(), new FMData_DelegatePointerWrapper(&FMData_MisCount_Set) },
        { "PlayTime".AsMemory(), new FMData_DelegatePointerWrapper(&FMData_PlayTime_Set) },

        #region Old resource format - backward compatibility, we still have to be able to read it

        { "HasMap".AsMemory(), new FMData_DelegatePointerWrapper(&FMData_HasMap_Set) },
        { "HasAutomap".AsMemory(), new FMData_DelegatePointerWrapper(&FMData_HasAutomap_Set) },
        { "HasScripts".AsMemory(), new FMData_DelegatePointerWrapper(&FMData_HasScripts_Set) },
        { "HasTextures".AsMemory(), new FMData_DelegatePointerWrapper(&FMData_HasTextures_Set) },
        { "HasSounds".AsMemory(), new FMData_DelegatePointerWrapper(&FMData_HasSounds_Set) },
        { "HasObjects".AsMemory(), new FMData_DelegatePointerWrapper(&FMData_HasObjects_Set) },
        { "HasCreatures".AsMemory(), new FMData_DelegatePointerWrapper(&FMData_HasCreatures_Set) },
        { "HasMotions".AsMemory(), new FMData_DelegatePointerWrapper(&FMData_HasMotions_Set) },
        { "HasMovies".AsMemory(), new FMData_DelegatePointerWrapper(&FMData_HasMovies_Set) },
        { "HasSubtitles".AsMemory(), new FMData_DelegatePointerWrapper(&FMData_HasSubtitles_Set) },

        #endregion
    };

    #endregion

    #region Generated code for writer

    private static void WriteFMDataIni(List<FanMission> fmDataList, List<FanMission> fmDataListTDM, string fileName)
    {
        // Larger buffer size helps with perf for larger file sizes.
        using var fs = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.Read, ByteSize.KB * 256);
        using var sw = new StreamWriter(fs, Encoding.UTF8, ByteSize.KB * 256);

        Span<char> numberSpan = stackalloc char[20];

        static void AddFMToSW(FanMission fm, StreamWriter sw, Span<char> numberSpan)
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
                fm.TDMVersion.TryFormat(numberSpan, out int written, provider: NumberFormatInfo.InvariantInfo);
                sw.Write("TDMVersion=");
                sw.WriteLine(numberSpan[..written]);
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
                sw.Write(",");
                if (single.Value.TryFormat(numberSpan, out int written, provider: NumberFormatInfo.InvariantInfo))
                {
                    sw.WriteLine(numberSpan[..written]);
                }
            }
            else if (fm.ReadmeCodePages.TryGetDictionary(out var dict))
            {
                foreach (var item in dict)
                {
                    sw.Write("ReadmeEncoding=");
                    sw.Write(item.Key);
                    sw.Write(",");
                    if (item.Value.TryFormat(numberSpan, out int written, provider: NumberFormatInfo.InvariantInfo))
                    {
                        sw.WriteLine(numberSpan[..written]);
                    }
                }
            }
            if (fm.SizeBytes != 0)
            {
                fm.SizeBytes.TryFormat(numberSpan, out int written, provider: NumberFormatInfo.InvariantInfo);
                sw.Write("SizeBytes=");
                sw.WriteLine(numberSpan[..written]);
            }
            if (fm.Rating != -1)
            {
                fm.Rating.TryFormat(numberSpan, out int written, provider: NumberFormatInfo.InvariantInfo);
                sw.Write("Rating=");
                sw.WriteLine(numberSpan[..written]);
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
                long seconds = new DateTimeOffset((DateTime)fm.DateAdded).ToUnixTimeSeconds();
                seconds.TryFormat(numberSpan, out int written, "X", provider: NumberFormatInfo.InvariantInfo);
                sw.Write("DateAdded=");
                sw.WriteLine(numberSpan[..written]);
            }
            if (fm.FinishedOn != 0)
            {
                fm.FinishedOn.TryFormat(numberSpan, out int written, provider: NumberFormatInfo.InvariantInfo);
                sw.Write("FinishedOn=");
                sw.WriteLine(numberSpan[..written]);
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
                sw.WriteLine("HasMap=" + FMHasResource(fm, CustomResources.Map).ToString());
                sw.WriteLine("HasAutomap=" + FMHasResource(fm, CustomResources.Automap).ToString());
                sw.WriteLine("HasScripts=" + FMHasResource(fm, CustomResources.Scripts).ToString());
                sw.WriteLine("HasTextures=" + FMHasResource(fm, CustomResources.Textures).ToString());
                sw.WriteLine("HasSounds=" + FMHasResource(fm, CustomResources.Sounds).ToString());
                sw.WriteLine("HasObjects=" + FMHasResource(fm, CustomResources.Objects).ToString());
                sw.WriteLine("HasCreatures=" + FMHasResource(fm, CustomResources.Creatures).ToString());
                sw.WriteLine("HasMotions=" + FMHasResource(fm, CustomResources.Motions).ToString());
                sw.WriteLine("HasMovies=" + FMHasResource(fm, CustomResources.Movies).ToString());
                sw.WriteLine("HasSubtitles=" + FMHasResource(fm, CustomResources.Subtitles).ToString());
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
                fm.MisCount.TryFormat(numberSpan, out int written, provider: NumberFormatInfo.InvariantInfo);
                sw.Write("MisCount=");
                sw.WriteLine(numberSpan[..written]);
            }
            if (fm.PlayTime.Ticks != 0)
            {
                fm.PlayTime.Ticks.TryFormat(numberSpan, out int written, provider: NumberFormatInfo.InvariantInfo);
                sw.Write("PlayTime=");
                sw.WriteLine(numberSpan[..written]);
            }
        }

        foreach (FanMission fm in fmDataList)
        {
            AddFMToSW(fm, sw, numberSpan);
        }

        foreach (FanMission fm in fmDataListTDM)
        {
            AddFMToSW(fm, sw, numberSpan);
        }
    }

    #endregion
}
