#define FenGen_FMDataSource

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using AL_Common;
using JetBrains.Annotations;
using static AL_Common.FenGenAttributes;
using static AL_Common.LanguageSupport;
using static AL_Common.Logger;
using static AngelLoader.GameSupport;
using static AngelLoader.Global;

namespace AngelLoader.DataClasses;

/*
FenGen reads this and outputs fast ini read and write methods.

Notes to self:
-Keep names shortish for more performance when reading
-I told myself to version-header ini files right from the start, but I didn't. Meh.
*/

[FenGenFMDataSourceClass]
[StructLayout(LayoutKind.Auto)]
public sealed class FanMission
{
    #region FM Flags

    // For compactness of the object - bools now take 1 bit instead of 1 byte.
    // This is at the cost of branching in the property accesses, but meh. It's way more than fast enough still.

    [Flags]
    private enum FMFlag : ushort
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
        ShownInFilter = 1 << 12,
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool GetFMFlag(FMFlag fmFlag) => (_fmFlags & fmFlag) != 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void SetFMFlag(FMFlag fmFlag, bool value)
    {
        if (value) { _fmFlags |= fmFlag; } else { _fmFlags &= ~fmFlag; }
    }

    [FenGenIgnore]
    private FMFlag _fmFlags = FMFlag.None;

    #endregion

    [FenGenIgnore]
    internal bool ShownInFilter
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => GetFMFlag(FMFlag.ShownInFilter);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => SetFMFlag(FMFlag.ShownInFilter, value);
    }

    // Cached value to avoid doing the expensive check every startup. If a matching archive is found in the
    // normal archive list combine, this will be set to false again. Results in a nice perf gain if there are
    // archive-less FMs in the list.
    internal bool NoArchive
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => GetFMFlag(FMFlag.NoArchive);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => SetFMFlag(FMFlag.NoArchive, value);
    }

    // Since our scanned values are very complex due to having the option to choose what to scan for as well
    // as being able to import from three other loaders, we need a simple way to say "scan on select or not".
    internal bool MarkedScanned
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => GetFMFlag(FMFlag.MarkedScanned);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => SetFMFlag(FMFlag.MarkedScanned, value);
    }

    [FenGenIgnore]
    internal bool MarkedRecent
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => GetFMFlag(FMFlag.MarkedRecent);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => SetFMFlag(FMFlag.MarkedRecent, value);
    }

    internal bool Pinned
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => GetFMFlag(FMFlag.Pinned);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => SetFMFlag(FMFlag.Pinned, value);
    }

    // For FMs that have metadata but don't exist on disk
    [FenGenIgnore]
    internal bool MarkedUnavailable
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => GetFMFlag(FMFlag.MarkedUnavailable);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => SetFMFlag(FMFlag.MarkedUnavailable, value);
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
    [FenGenTreatAsList("string")]
    [FenGenListType("MultipleLines")]
    internal readonly AltTitlesList AltTitles = new();

    internal string Author = "";

    internal Game Game = Game.Null;

    internal bool Installed
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => GetFMFlag(FMFlag.Installed);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => SetFMFlag(FMFlag.Installed, value);
    }

    internal bool NoReadmes
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => GetFMFlag(FMFlag.NoReadmes);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => SetFMFlag(FMFlag.NoReadmes, value);
    }

    // Lazy value to say that we should re-cache readmes on next select.
    internal bool ForceReadmeReCache
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => GetFMFlag(FMFlag.ForceReadmeReCache);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => SetFMFlag(FMFlag.ForceReadmeReCache, value);
    }

    [FenGenIgnore]
    private string _selectedReadme = "";
    // @DIRSEP: Always backslashes for backward compatibility and prevention of find misses in readme chooser box
    internal string SelectedReadme { get => _selectedReadme; set => _selectedReadme = value.ToBackSlashes(); }

    [FenGenReadmeEncoding]
    [FenGenIniName("ReadmeEncoding")]
    internal readonly ReadmeCodePagesCollection ReadmeCodePages = new();

    [FenGenNumericEmpty(0)]
    internal ulong SizeBytes = 0;

    [FenGenIgnore]
    private sbyte _rating = -1;
    [FenGenNumericEmpty(-1)]
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
    internal uint FinishedOn
    {
        get => _finishedOn;
        set
        {
            _finishedOn = (byte)value.Clamp((byte)0, (byte)15);
            if (_finishedOn > 0) SetFMFlag(FMFlag.FinishedOnUnknown, false);
        }
    }

    internal bool FinishedOnUnknown
    {
        get => GetFMFlag(FMFlag.FinishedOnUnknown);
        set
        {
            SetFMFlag(FMFlag.FinishedOnUnknown, value);
            if (value) _finishedOn = 0;
        }
    }

    [FenGenIgnore]
    private string? _commentSingleLine;
    [FenGenIgnore]
    internal string CommentSingleLine
    {
        get => _commentSingleLine ??= Comment.FromRNEscapes().ToSingleLineComment(100);
        set => _commentSingleLine = value;
    }

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
        get => GameSupportsMods(Game) && GetFMFlag(FMFlag.DisableAllMods);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => SetFMFlag(FMFlag.DisableAllMods, value);
    }

    [FenGenIgnore]
    internal bool ResourcesScanned
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => GetFMFlag(FMFlag.ResourcesScanned);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => SetFMFlag(FMFlag.ResourcesScanned, value);
    }
    [FenGenIniName("HasResources")]
    internal CustomResources Resources = CustomResources.None;

    internal bool LangsScanned
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => GetFMFlag(FMFlag.LangsScanned);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => SetFMFlag(FMFlag.LangsScanned, value);
    }

    internal Language Langs = Language.Default;

    [FenGenFlagsSingleAssignment]
    internal Language SelectedLang = Language.Default;

    /*
    @FMDataCompact(Cat/tags):
    Could we just fill out the global list with objects and then put those objects into each FMs' list, like the
    FMs will just reference the global collection? That way we don't have to keep them in sync, and we wouldn't
    duplicate a ton of strings either.
    */
    [FenGenIgnore]
    internal readonly FMCategoriesCollection Tags = new();
    /*
    @FMDataCompact(TagsString):
    We can get rid of this and just construct it from Tags at serialization time. For ~1900 FMs we can do this in
    ~1.1ms (first run). But this scales sub-linearly; for 10x the FMs we only go up to ~3ms (first run).
    This is probably okay, but carrying TagsString makes the serialization completely free (and alloc-free too).
    */
    internal string TagsString = "";

    internal bool? NewMantle;
    internal bool? PostProc;
    internal bool? NDSubs;

    [FenGenNumericEmpty(-1)]
    internal int MisCount = -1;

    [FenGenIgnore]
    private TimeSpan _playTime = TimeSpan.Zero;
    [FenGenNumericEmpty(0)]
    internal TimeSpan PlayTime
    {
        get => _playTime;
        // Negative playtime makes no sense, so just clamp it to 0
        set => _playTime = value.Ticks < 0 ? TimeSpan.Zero : value;
    }

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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool IsTopped() => MarkedRecent || Pinned;

    internal void LogInfo(
        string topMessage,
        Exception? ex = null,
        bool stackTrace = false,
        [CallerMemberName] string callerMemberName = "")
    {
        Log("Caller: " + callerMemberName + "\r\n\r\n" +
            topMessage + "\r\n" +
            "" + nameof(Game) + ": " + Game + "\r\n" +
            "" + nameof(Archive) + ": " + Archive + "\r\n" +
            "" + nameof(InstalledDir) + ": " + InstalledDir + "\r\n" +
            "" + nameof(TDMInstalledDir) + " (if applicable): " + TDMInstalledDir + "\r\n" +
            "" + nameof(Installed) + ": " + Installed + "\r\n" +
            (Game.ConvertsToKnownAndSupported(out GameIndex gameIndex)
                ? "Base directory for installed FMs: " + Config.GetFMInstallPath(gameIndex)
                : "Game type is not known or not supported.") +
            (ex != null ? "\r\nException:\r\n" + ex : ""), stackTrace: stackTrace);
    }

    #endregion
}

public readonly struct ValidAudioConvertibleFM
{
    private readonly FanMission InternalFM;

    public readonly GameIndex GameIndex;

    public string InstalledDir
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => InternalFM.InstalledDir;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string GetId() => InternalFM.GetId();

    private ValidAudioConvertibleFM(FanMission fm, GameIndex gameIndex)
    {
        InternalFM = fm;
        GameIndex = gameIndex;
    }

    public static bool TryCreateFrom(FanMission inFM, out ValidAudioConvertibleFM outFM)
    {
        if (inFM.Game.ConvertsToDark(out GameIndex gameIndex) && inFM is { Installed: true, MarkedUnavailable: false })
        {
            outFM = new ValidAudioConvertibleFM(inFM, gameIndex);
            return true;
        }
        else
        {
            outFM = default;
            return false;
        }
    }

    public static List<ValidAudioConvertibleFM> CreateListFrom(List<FanMission> fms)
    {
        List<ValidAudioConvertibleFM> ret = new(fms.Count);
        for (int i = 0; i < fms.Count; i++)
        {
            if (TryCreateFrom(fms[i], out ValidAudioConvertibleFM validDarkFM))
            {
                ret.Add(validDarkFM);
            }
        }
        return ret;
    }

    internal void LogInfo(
        string topMessage,
        Exception? ex = null,
        bool stackTrace = false,
        [CallerMemberName] string callerMemberName = "")
    {
        InternalFM.LogInfo(topMessage, ex, stackTrace, callerMemberName);
    }

    [PublicAPI]
    public override bool Equals(object? obj) => obj is ValidAudioConvertibleFM fm && Equals(fm);

    [PublicAPI]
    public bool Equals(ValidAudioConvertibleFM fm) => InternalFM.Equals(fm.InternalFM);

    [PublicAPI]
    public static bool operator ==(ValidAudioConvertibleFM lhs, ValidAudioConvertibleFM rhs) => lhs.Equals(rhs);

    [PublicAPI]
    public static bool operator !=(ValidAudioConvertibleFM lhs, ValidAudioConvertibleFM rhs) => !(lhs == rhs);

    [PublicAPI]
    public override int GetHashCode() => InternalFM.GetHashCode();

    [PublicAPI]
    public override string? ToString() => InternalFM.ToString();
}
