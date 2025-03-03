using System;
using JetBrains.Annotations;

namespace FMScanner;

[Flags]
internal enum ScanOptionsEnum
{
    None,
    Title,
    Author,
    GameType,
    CustomResources,
    Size,
    ReleaseDate,
    Tags,
    MissionCount,
}

[PublicAPI]
public sealed class ScanOptions
{
    // IMPORTANT: We have the temporary "scan title for author purposes" that could make this wrong in that case
    internal ScanOptionsEnum GetOptionsEnum()
    {
        ScanOptionsEnum ret = ScanOptionsEnum.None;

        if (ScanTitle) ret |= ScanOptionsEnum.Title;
        if (ScanAuthor) ret |= ScanOptionsEnum.Author;
        if (ScanGameType) ret |= ScanOptionsEnum.GameType;
        if (ScanCustomResources) ret |= ScanOptionsEnum.CustomResources;
        if (ScanSize) ret |= ScanOptionsEnum.Size;
        if (ScanReleaseDate) ret |= ScanOptionsEnum.ReleaseDate;
        if (ScanTags) ret |= ScanOptionsEnum.Tags;
        if (ScanMissionCount) ret |= ScanOptionsEnum.MissionCount;

        return ret;
    }

    // Dumb looking on this side, but extremely nice and convenient on the calling side.
    // Pretty sure there must be a better way to be able to have two sets of defaults for one object...
    /// <summary>
    /// Returns a <see cref="ScanOptions"/> object where all fields are set to false except the ones you specify to be true.
    /// </summary>
    public static ScanOptions FalseDefault(
        bool scanTitle = false,
        bool scanAuthor = false,
        bool scanGameType = false,
        bool scanCustomResources = false,
        bool scanSize = false,
        bool scanReleaseDate = false,
        bool scanTags = false,
        bool scanMissionCount = false
    ) =>
        new()
        {
            ScanTitle = scanTitle,
            ScanAuthor = scanAuthor,
            ScanGameType = scanGameType,
            ScanCustomResources = scanCustomResources,
            ScanSize = scanSize,
            ScanReleaseDate = scanReleaseDate,
            ScanTags = scanTags,
            ScanMissionCount = scanMissionCount,
        };

    internal ScanOptions DeepCopy() => new()
    {
        ScanTitle = ScanTitle,
        ScanAuthor = ScanAuthor,
        ScanGameType = ScanGameType,
        ScanCustomResources = ScanCustomResources,
        ScanSize = ScanSize,
        ScanReleaseDate = ScanReleaseDate,
        ScanTags = ScanTags,
        ScanMissionCount = ScanMissionCount,
    };

    /// <summary>
    /// <see langword="true"/> to detect the mission's title.
    /// </summary>
    public bool ScanTitle = true;
    /// <summary>
    /// <see langword="true"/> to detect the mission's author.
    /// </summary>
    public bool ScanAuthor = true;
    /// <summary>
    /// <see langword="true"/> to detect which game the mission is for (Thief 1, Thief 2, Thief 3, or System Shock 2).
    /// </summary>
    public bool ScanGameType = true;
    /// <summary>
    /// <see langword="true"/> to detect whether the mission contains custom resources.
    /// If the mission is for Thief: Deadly Shadows, this option has no effect.
    /// </summary>
    public bool ScanCustomResources = true;
    /// <summary>
    /// <see langword="true"/> to detect the size of the mission. This will differ depending on whether the
    /// mission is a compressed archive or an uncompressed directory.
    /// </summary>
    public bool ScanSize = true;
    /// <summary>
    /// <see langword="true"/> to detect the mission's release date.
    /// </summary>
    public bool ScanReleaseDate = true;
    /// <summary>
    /// <see langword="true"/> to detect the mission's tags.
    /// </summary>
    public bool ScanTags = true;
    /// <summary>
    /// <see langword="true"/> to detect the mission count.
    /// </summary>
    public bool ScanMissionCount = true;
}

[PublicAPI]
public sealed class ProgressReport
{
    public string FMName = "";
    public int FMNumber;
    public int FMsCount;
}

[PublicAPI]
public enum Game
{
    /// <summary>Not scanned.</summary>
    Null,
    /// <summary>Thief: The Dark Project or Thief Gold.</summary>
    Thief1,
    /// <summary>Thief II: The Metal Age.</summary>
    Thief2,
    /// <summary>Thief: Deadly Shadows.</summary>
    Thief3,
    /// <summary>System Shock 2.</summary>
    SS2,
    /// <summary>The Dark Mod.</summary>
    TDM,
    /// <summary>Invalid or corrupt mission, not a mission, or mission for an unsupported game.</summary>
    Unsupported,
}

[PublicAPI]
public sealed class FMToScan
{
    public readonly int OriginalIndex;

    public readonly string Path;
    public readonly bool ForceFullScan;
    /// <summary>
    /// Optional cache path to place extracted readme files for solid archives, for performance.
    /// Ignored for all other FM package types.
    /// </summary>
    public readonly string CachePath;

    public readonly string DisplayName;

    public readonly bool IsTDM;
    public readonly bool IsArchive;

    public FMToScan(string path, bool forceFullScan, string cachePath, bool isTDM, string displayName, bool isArchive, int originalIndex)
    {
        Path = path;
        ForceFullScan = forceFullScan;
        CachePath = cachePath;
        DisplayName = displayName;
        IsTDM = isTDM;
        IsArchive = isArchive;
        OriginalIndex = originalIndex;
    }

    public FMToScan(string path, bool forceFullScan, bool isTDM, string displayName, bool isArchive, int originalIndex)
    {
        Path = path;
        ForceFullScan = forceFullScan;
        CachePath = "";
        DisplayName = displayName;
        IsTDM = isTDM;
        IsArchive = isArchive;
        OriginalIndex = originalIndex;
    }
}

// Fields with types that don't have a simple "unknown" state are nullable to represent "not scanned" or "unknown".
// Numeric types, bools, DateTime etc.

public sealed class ScannedFMDataAndError
{
    public ScannedFMData? ScannedFMData;
    public Exception? Exception;
    public Fen7z.Result? Fen7zResult;
    public string ErrorInfo = "";
    public bool NeedsHtmlRefExtract;
    public readonly int OriginalIndex;

    public ScannedFMDataAndError(int originalIndex)
    {
        OriginalIndex = originalIndex;
    }
}

[PublicAPI]
// These properties are kept in this exact order because the test diff writeout depends on it
public sealed class ScannedFMData
{
    public string ArchiveName = "";
    public ulong? Size;

    private string _title = "";
    public string Title { get => _title; set => _title = value.Trim(); }

    public string[] AlternateTitles = Array.Empty<string>();

    private string _author = "";
    public string Author { get => _author; set => _author = value.Trim(); }

    public Game Game;

    private DateTime? _lastUpdateDate;
    public DateTime? LastUpdateDate
    {
        get => _lastUpdateDate;
        // Future years will eventually stop being rejected once the current date passes them, but eh
        internal set => _lastUpdateDate = value != null && ((DateTime)value).Year > DateTime.Now.Year ? null : value;
    }

    public string TagsString = "";

    public int? MissionCount;

    public bool? HasMap;

    private bool? _hasAutomap;
    public bool? HasAutomap
    {
        get => _hasAutomap;
        internal set
        {
            _hasAutomap = value;
            /*
            Definitely a clever deduction, definitely not a sneaky hack for GatB-T2.
            More details:
            Map files are supposed to be named "pagexxx.pcx" ("page001.pcx" etc.).
            Some missions have a file in the intrface base dir called "map.pcx".
            From what I can tell, this looks like it's supposed to be the background (or "surrounding")
            image for the map screen. But GatB-T2 puts the actual map image itself in this file, and has
            no proper pagexxx.pcx file. But it does have an automap file (although it appears not to work
            in-game, go figure). I have no reasonable way to detect this situation; I just have to trust
            that files are what they're supposed to be. But since an automap requires a map, it makes
            sense to set HasMap to true if HasAutomap is true, and that just coincidentally makes GatB-T2
            map detection accurate. So result achieved and no harm done.
            */
            if (value == true) HasMap = true;
        }
    }

    public bool? HasCustomCreatures;
    public bool? HasCustomMotions;
    public bool? HasMovies;
    public bool? HasCustomObjects;
    public bool? HasCustomScripts;
    public bool? HasCustomSounds;
    public bool? HasCustomSubtitles;
    public bool? HasCustomTextures;
}
