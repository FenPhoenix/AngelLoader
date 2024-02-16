#define FenGen_FMDataSource

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static AL_Common.Common;
using static AL_Common.FenGenAttributes;
using static AL_Common.LanguageSupport;
using static AngelLoader.GameSupport;

namespace AngelLoader.DataClasses;

/*
FenGen reads this and outputs fast ini read and write methods.

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
[StructLayout(LayoutKind.Auto)]
public sealed class FanMission
{
    [Flags]
    private enum BoolBit : ushort
    {
        None = 0,
        NoArchive = 1 << 0,
        MarkedScanned = 1 << 1,
        MarkedRecent = 1 << 2,
        Pinned = 1 << 3,
        MarkedUnavailable = 1 << 4,
        Installed = 1 << 5,
        NoReadmes = 1 << 6,
        ForceReadmeReCache = 1 << 7,
        FinishedOnUnknown = 1 << 8,
        DisableAllMods = 1 << 9,
        ResourcesScanned = 1 << 10,
        LangsScanned = 1 << 11,
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool GetBoolBit(BoolBit boolBit) => (_boolBitset & boolBit) != 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void SetBoolBit(BoolBit boolBit, bool value)
    {
        if (value) { _boolBitset |= boolBit; } else { _boolBitset &= ~boolBit; }
    }

    [FenGenIgnore]
    private BoolBit _boolBitset = BoolBit.None;

    // Cached value to avoid doing the expensive check every startup. If a matching archive is found in the
    // normal archive list combine, this will be set to false again. Results in a nice perf gain if there are
    // archive-less FMs in the list.
    internal bool NoArchive
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => GetBoolBit(BoolBit.NoArchive);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => SetBoolBit(BoolBit.NoArchive, value);
    }

    // Since our scanned values are very complex due to having the option to choose what to scan for as well
    // as being able to import from three other loaders, we need a simple way to say "scan on select or not".
    internal bool MarkedScanned
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => GetBoolBit(BoolBit.MarkedScanned);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => SetBoolBit(BoolBit.MarkedScanned, value);
    }

    [FenGenIgnore]
    internal bool MarkedRecent
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => GetBoolBit(BoolBit.MarkedRecent);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => SetBoolBit(BoolBit.MarkedRecent, value);
    }

    internal bool Pinned
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => GetBoolBit(BoolBit.Pinned);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => SetBoolBit(BoolBit.Pinned, value);
    }

    // For FMs that have metadata but don't exist on disk
    [FenGenIgnore]
    internal bool MarkedUnavailable
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => GetBoolBit(BoolBit.MarkedUnavailable);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => SetBoolBit(BoolBit.MarkedUnavailable, value);
    }

    internal string Archive = "";

    [FenGenIgnore]
    internal string DisplayArchive
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Game == Game.TDM ? TDMInstalledDir : Archive;
    }

    /*
    InstalledDir doubles as a unique identifier for an FM, but that use requires us to be able to change it in
    case of a naming clash. But for TDM, FMs are always in folders to begin with, so we can't change the name or
    we wouldn't be able to find our folder. So we add the TDMInstalledDir field for TDM FMs which is the real
    folder name, and then InstalledDir is either the same or a number-appended version if there was a clash.
    For TDM FMs, we use InstalledDir only as a unique id and nothing else, so it can be whatever.
    Janky, but we're stuck with it for backward compatibility.
    */
    /// <summary>
    /// For TDM FMs, a unique identifier only. For other games, also the actual installed folder name.
    /// </summary>
    internal string InstalledDir = "";
    /// <summary>
    /// For TDM FMs, the actual installed folder name. For other games, blank and not used.
    /// </summary>
    internal string TDMInstalledDir = "";

    [FenGenIgnore]
    internal string RealInstalledDir
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Game == Game.TDM ? TDMInstalledDir : InstalledDir;
    }

    [FenGenNumericEmpty(0)]
    internal int TDMVersion;

    internal string Title = "";
    [FenGenListType("MultipleLines")]
    internal readonly List<string> AltTitles = new();

    internal string Author = "";

    [FenGenDoNotSubstring]
    internal Game Game = Game.Null;

    internal bool Installed
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => GetBoolBit(BoolBit.Installed);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => SetBoolBit(BoolBit.Installed, value);
    }

    internal bool NoReadmes
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => GetBoolBit(BoolBit.NoReadmes);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => SetBoolBit(BoolBit.NoReadmes, value);
    }

    // Lazy value to say that we should re-cache readmes on next select.
    internal bool ForceReadmeReCache
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => GetBoolBit(BoolBit.ForceReadmeReCache);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => SetBoolBit(BoolBit.ForceReadmeReCache, value);
    }

    [FenGenIgnore]
    private string _selectedReadme = "";
    // @DIRSEP: Always backslashes for backward compatibility and prevention of find misses in readme chooser box
    internal string SelectedReadme { get => _selectedReadme; set => _selectedReadme = value.ToBackSlashes(); }

    /*
    @FMDataCompact(ReadmeCodePages): We can make this more efficient.
    A List is half the size of a Dictionary (40 bytes vs. 80 bytes), and we could make another custom list
    ("CompactList") with even less in it than ListFast, and just use a linear search. The most readmes any known
    FM has is 8, and it's almost unimaginable that any non-troll FM would have enough for a linear search to
    become a problem.
    We could also do a frugal object - 96% of FMs have 0, 1, or 2 readmes; 77% have 0 or 1.
    */
    [FenGenReadmeEncoding]
    [FenGenDoNotSubstring]
    [FenGenIniName("ReadmeEncoding")]
    internal readonly DictionaryI<int> ReadmeCodePages = new();

    [FenGenNumericEmpty(0)]
    [FenGenMaxDigits(20)]
    internal ulong SizeBytes = 0;

    [FenGenIgnore]
    private sbyte _rating = -1;
    [FenGenNumericEmpty(-1)]
    [FenGenMaxDigits(2)]
    internal int Rating { get => _rating; set => _rating = (sbyte)value.SetRatingClamped(); }

    internal readonly ExpandableDate ReleaseDate = new();
    internal readonly ExpandableDate LastPlayed = new();

    /*
    We get this value for free when we get the FM archives and dirs on startup, but this value is fragile:
    it updates whenever the user so much as moves the file or folder. We store it here to keep it permanent
    even across moves, new PCs or Windows installs with file restores, etc.
    This is not an ExpandableDate, because the way we get the date value is not in unix hex string format,
    and it's expensive to convert it to such. With a regular nullable DateTime we're only paying like 3-5ms
    extra on startup (for 1574 FMs), so it's good enough for now.
    */
    internal DateTime? DateAdded = null;

    // IMPORTANT: FinishedOnUnknown MUST come AFTER FinishedOn to maintain its override priority!

    [FenGenIgnore]
    private byte _finishedOn;
    [FenGenNumericEmpty(0)]
    [FenGenMaxDigits(2)]
    internal uint FinishedOn
    {
        get => _finishedOn;
        set
        {
            _finishedOn = (byte)value.Clamp((byte)0, (byte)15);
            if (_finishedOn > 0) SetBoolBit(BoolBit.FinishedOnUnknown, false);
        }
    }

    internal bool FinishedOnUnknown
    {
        get => GetBoolBit(BoolBit.FinishedOnUnknown);
        set
        {
            SetBoolBit(BoolBit.FinishedOnUnknown, value);
            if (value) _finishedOn = 0;
        }
    }

    [FenGenIgnore]
    internal string CommentSingleLine = "";
    [FenGenDoNotTrimValue]
    internal string Comment = "";

    [FenGenIgnore]
    private string _disabledMods = "";
    internal string DisabledMods
    {
        get => GameSupportsMods(Game) ? _disabledMods : "";
        set => _disabledMods = value;
    }

    /// <summary>
    /// This is for backward compatibility only. Use only for that purpose.
    /// </summary>
    internal bool DisableAllMods
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => GameSupportsMods(Game) && GetBoolBit(BoolBit.DisableAllMods);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => SetBoolBit(BoolBit.DisableAllMods, value);
    }

    [FenGenIgnore]
    internal bool ResourcesScanned
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => GetBoolBit(BoolBit.ResourcesScanned);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => SetBoolBit(BoolBit.ResourcesScanned, value);
    }
    [FenGenIniName("HasResources")]
    [FenGenDoNotSubstring]
    internal CustomResources Resources = CustomResources.None;

    internal bool LangsScanned
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => GetBoolBit(BoolBit.LangsScanned);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => SetBoolBit(BoolBit.LangsScanned, value);
    }

    [FenGenDoNotSubstring]
    internal Language Langs = Language.Default;

    [FenGenFlagsSingleAssignment]
    [FenGenDoNotSubstring]
    internal Language SelectedLang = Language.Default;

    /*
    @FMDataCompact(Cat/tags):
    Could we just fill out the global list with objects and then put those objects into each FMs' list, like the
    FMs will just reference the global collection? That way we don't have to keep them in sync, and we wouldn't
    duplicate a ton of strings either.
    */
    internal readonly FMCategoriesCollection Tags = new();
    /*
    @FMDataCompact(TagsString):
    We can get rid of this and just construct it from Tags at serialization time. For ~1900 FMs we can do this in
    ~1.1ms (first run). But this scales sub-linearly; for 10x the FMs we only go up to ~3ms (first run).
    This is probably okay, but carrying TagsString makes the serialization completely free (and alloc-free too).
    */
    [FenGenIgnore]
    internal string TagsString = "";

    internal bool? NewMantle;
    internal bool? PostProc;
    internal bool? NDSubs;

    [FenGenNumericEmpty(-1)]
    [FenGenMaxDigits(10)]
    internal int MisCount = -1;

#if DateAccTest
    [FenGenIgnore]
    internal DateAccuracy DateAccuracy = DateAccuracy.Null;
#endif

    #region Utility methods

    /// <summary>
    /// If <see cref="T:Archive"/> is non-blank, returns it; otherwise, returns <see cref="T:InstalledDir"/>.
    /// </summary>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal string GetId() => !Archive.IsEmpty() ? Archive : InstalledDir;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void SetResource(CustomResources resource, bool value)
    {
        if (value) { Resources |= resource; } else { Resources &= ~resource; }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool HasResource(CustomResources resource) => (Resources & resource) != 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool NeedsScan() => !MarkedUnavailable && (Game == Game.Null ||
        (Game != Game.Unsupported && !MarkedScanned));

    // Rar is only "slow to scan" if it's solid, but since there's no quick way to tell, let's just always call
    // it "slow to scan".
    internal bool IsFastToScan() => Game != Game.TDM && !Archive.ExtIs7z() && !Archive.ExtIsRar();

    internal bool NeedsReadmesCachedDuringScan() => Archive.ExtIs7z() || Archive.ExtIsRar();

    #endregion
}
