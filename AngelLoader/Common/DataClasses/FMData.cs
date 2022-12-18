﻿#define FenGen_FMDataSource

using System;
using System.Collections.Generic;
using static AL_Common.Common;
using static AngelLoader.FenGenAttributes;
using static AngelLoader.GameSupport;
using static AngelLoader.LanguageSupport;

namespace AngelLoader.DataClasses;

// FenGen reads this and outputs fast ini read and write methods.
/*
 Notes to self:
    -Keep names shortish for more performance when reading
    -I told myself to version-header ini files right from the start, but I didn't. Meh.


@MEM(FMData): We could get rid of some stuff in here, like TagsString is an easy candidate
-We could also, if we're clever, get rid of some other stuff we nominally need, like MarkedRecent. We could
 have like an internal hashset where we put all "recent" FMs into and remove them from it as appropriate.
 Same with MarkedUnavailable. In fact, we could do that for all fields expected to _usually_ be false.
-We could squeeze all value types and enums down to their smallest possible representation, sbyte/byte/ushort
 etc.
*/

[FenGenFMDataSourceClass]
public sealed class FanMission
{
    // Cached value to avoid doing the expensive check every startup. If a matching archive is found in the
    // normal archive list combine, this will be set to false again. Results in a nice perf gain if there are
    // archive-less FMs in the list.
    internal bool NoArchive;

    // Since our scanned values are very complex due to having the option to choose what to scan for as well
    // as being able to import from three other loaders, we need a simple way to say "scan on select or not".
    internal bool MarkedScanned;

    // For drawing rows in "Recently-Added Yellow" color
    [FenGenIgnore]
    internal bool MarkedRecent;

    internal bool Pinned;

    // For FMs that have metadata but don't exist on disk
    [FenGenIgnore]
    internal bool MarkedUnavailable;

    internal string Archive = "";
    internal string InstalledDir = "";

    internal string Title = "";
    [FenGenListType("MultipleLines")]
    internal readonly List<string> AltTitles = new();

    internal string Author = "";

    [FenGenDoNotSubstring]
    internal Game Game = Game.Null;

    internal bool Installed;

    internal bool NoReadmes;

    [FenGenIgnore]
    private string _selectedReadme = "";
    // @DIRSEP: Always backslashes for backward compatibility and prevention of find misses in readme chooser box
    internal string SelectedReadme { get => _selectedReadme; set => _selectedReadme = value.ToBackSlashes(); }

    [FenGenReadmeEncoding]
    [FenGenDoNotSubstring]
    [FenGenIniName("ReadmeEncoding")]
    internal readonly DictionaryI<int> ReadmeCodePages = new();

    [FenGenNumericEmpty(0)]
    [FenGenMaxDigits(20)]
    internal ulong SizeBytes = 0;

    [FenGenIgnore]
    private int _rating = -1;
    [FenGenNumericEmpty(-1)]
    [FenGenMaxDigits(2)]
    internal int Rating { get => _rating; set => _rating = value.SetRatingClamped(); }

    internal readonly ExpandableDate ReleaseDate = new();
    internal readonly ExpandableDate LastPlayed = new();

    // We get this value for free when we get the FM archives and dirs on startup, but this value is fragile:
    // it updates whenever the user so much as moves the file or folder. We store it here to keep it permanent
    // even across moves, new PCs or Windows installs with file restores, etc.
    // This is not an ExpandableDate, because the way we get the date value is not in unix hex string format,
    // and it's expensive to convert it to such. With a regular nullable DateTime we're only paying like 3-5ms
    // extra on startup (for 1574 FMs), so it's good enough for now.
    [FenGenDoNotConvertDateTimeToLocal]
    internal DateTime? DateAdded = null;

    [FenGenIgnore]
    // ReSharper disable once RedundantDefaultMemberInitializer
    private uint _finishedOn = 0;
    [FenGenNumericEmpty(0)]
    [FenGenMaxDigits(2)]
    internal uint FinishedOn { get => _finishedOn; set => _finishedOn = value.Clamp(0u, 15u); }
    internal bool FinishedOnUnknown;

    [FenGenIgnore]
    internal string CommentSingleLine = "";
    [FenGenDoNotTrimValue]
    internal string Comment = "";

    [FenGenIgnore]
    private string _disabledMods = "";
    internal string DisabledMods
    {
        get => Game == Game.Thief3 ? "" : _disabledMods;
        set => _disabledMods = value;
    }

    [FenGenIgnore]
    private bool _disableAllMods;
    /// <summary>
    /// This is for backward compatibility only. Use only for that purpose.
    /// </summary>
    internal bool DisableAllMods
    {
        get => Game != Game.Thief3 && _disableAllMods;
        set => _disableAllMods = value;
    }

    [FenGenIgnore]
    internal bool ResourcesScanned;
    [FenGenIniName("HasResources")]
    [FenGenDoNotSubstring]
    internal CustomResources Resources = CustomResources.None;

    internal bool LangsScanned;

    [FenGenDoNotSubstring]
    internal Language Langs = Language.Default;

    [FenGenFlagsSingleAssignment]
    [FenGenDoNotSubstring]
    internal Language SelectedLang = Language.Default;

    [FenGenIgnore]
    internal readonly FMCategoriesCollection Tags = new();
    internal string TagsString = "";

    internal bool? NewMantle;
    internal bool? PostProc;
    internal bool? NDSubs;

    [FenGenNumericEmpty(-1)]
    [FenGenMaxDigits(10)]
    internal int MisCount = -1;
}