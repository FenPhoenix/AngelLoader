﻿// Enable this (and the one in Scanner.cs) to get all features (we use it for testing)
//#define FMScanner_FullCode

using System;
using JetBrains.Annotations;

namespace FMScanner
{
    [PublicAPI]
    public sealed class ScanOptions
    {
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
            bool scanTags = false

#if FMScanner_FullCode
            ,
            bool scanCampaignMissionNames = false,
            bool scanVersion = false,
            bool scanLanguages = false,
            bool scanNewDarkRequired = false,
            bool scanNewDarkMinimumVersion = false,
            bool scanDescription = false
#endif
            ) =>
            new ScanOptions
            {
                ScanTitle = scanTitle,
                ScanAuthor = scanAuthor,
                ScanGameType = scanGameType,
                ScanCustomResources = scanCustomResources,
                ScanSize = scanSize,
                ScanReleaseDate = scanReleaseDate,
                ScanTags = scanTags,
#if FMScanner_FullCode
                ScanCampaignMissionNames = scanCampaignMissionNames,
                ScanVersion = scanVersion,
                ScanLanguages = scanLanguages,
                ScanNewDarkRequired = scanNewDarkRequired,
                ScanNewDarkMinimumVersion = scanNewDarkMinimumVersion,
                ScanDescription = scanDescription
#endif
            };

        internal ScanOptions DeepCopy() => new ScanOptions
        {
            ScanTitle = ScanTitle,
            ScanAuthor = ScanAuthor,
            ScanGameType = ScanGameType,
            ScanCustomResources = ScanCustomResources,
            ScanSize = ScanSize,
            ScanReleaseDate = ScanReleaseDate,
            ScanTags = ScanTags,
#if FMScanner_FullCode
            ScanCampaignMissionNames = ScanCampaignMissionNames,
            ScanVersion = ScanVersion,
            ScanLanguages = ScanLanguages,
            ScanNewDarkRequired = ScanNewDarkRequired,
            ScanNewDarkMinimumVersion = ScanNewDarkMinimumVersion,
            ScanDescription = ScanDescription
#endif
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
#if FMScanner_FullCode
        /// <summary>
        /// <see langword="true"/> to detect the titles of individual campaign missions.
        /// If the mission is not a campaign, this option has no effect.
        /// If the mission is for Thief: Deadly Shadows, this option has no effect.
        /// </summary>
        public bool ScanCampaignMissionNames = true;
        /// <summary>
        /// <see langword="true"/> to detect the mission's version.
        /// </summary>
        public bool ScanVersion = true;
        /// <summary>
        /// <see langword="true"/> to detect the languages the mission supports.
        /// If the mission is for Thief: Deadly Shadows, this option has no effect.
        /// </summary>
        public bool ScanLanguages = true;
        /// <summary>
        /// <see langword="true"/> to detect whether the mission requires NewDark.
        /// If the mission is for Thief: Deadly Shadows, this option has no effect.
        /// </summary>
        public bool ScanNewDarkRequired = true;
        /// <summary>
        /// <see langword="true"/> to detect the minimum NewDark version the mission requires.
        /// If ScanNewDarkRequired is false, this option has no effect.
        /// If the mission is for Thief: Deadly Shadows, this option has no effect.
        /// </summary>
        public bool ScanNewDarkMinimumVersion = true;
        /// <summary>
        /// <see langword="true"/> to detect the mission's fm.ini description field.
        /// </summary>
        public bool ScanDescription = true;
#endif
    }

    [PublicAPI]
    public sealed class ProgressReport
    {
        public string FMName = "";
        public int FMNumber;
        public int FMsTotal;
        public int Percent;
        public bool Finished;
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
        /// <summary>Invalid or corrupt mission, not a mission, or mission for an unsupported game.</summary>
        Unsupported
    }

    /// <summary>
    /// Whether the FM is a single mission or a campaign.
    /// </summary>
    [PublicAPI]
    public enum FMType
    {
        FanMission,
        Campaign
    }

    [PublicAPI]
    public class FMToScan
    {
        public string Path = "";
        public bool ForceFullScan;
        /// <summary>
        /// Optional cache path to place extracted readme files for .7z archives, for performance.
        /// Ignored for all other FM package types.
        /// </summary>
        public string CachePath = "";
    }

    // NULL_TODO (Scanner - FMData)
    // Fields with types that don't have a simple "unknown" state are nullable to represent "not scanned" or "unknown".
    // Numeric types, bools, DateTime etc.

    public sealed class ScannedFMDataAndError
    {
        public ScannedFMData? ScannedFMData;
        public Exception? Exception;
        public Fen7z.Fen7z.Result? Fen7zResult;
        public string ErrorInfo = "";
    }

    [PublicAPI]
    // These properties are kept in this exact order because the test diff writeout depends on it
    public sealed class ScannedFMData
    {
        public string ArchiveName = "";
        public ulong? Size;
        public string Title = "";
        public string[] AlternateTitles = Array.Empty<string>();
        public string Author = "";
        public FMType Type;
#if FMScanner_FullCode
        public string[] IncludedMissions = Array.Empty<string>();
#endif
        public Game Game;
        public string[] Languages = Array.Empty<string>();
#if FMScanner_FullCode
        public string Version = "";
        public bool? NewDarkRequired;
        public string NewDarkMinRequiredVersion = "";
#endif

#if FMScanner_FullCode
        /// <summary>
        /// Deprecated and will always be blank. Use <see cref="LastUpdateDate"/> instead.
        /// </summary>
        public DateTime? OriginalReleaseDate;
#endif

        private DateTime? _lastUpdateDate;
        public DateTime? LastUpdateDate
        {
            get => _lastUpdateDate;
            // Future years will eventually stop being rejected once the current date passes them, but eh
            internal set => _lastUpdateDate = value != null && ((DateTime)value).Year > DateTime.Now.Year ? null : value;
        }

#if FMScanner_FullCode
        public string Description = "";
#endif
        public string TagsString = "";

        public bool? HasMap;

        private bool? _hasAutomap;
        public bool? HasAutomap
        {
            get => _hasAutomap;
            internal set
            {
                _hasAutomap = value;
                // Definitely a clever deduction, definitely not a sneaky hack for GatB-T2.
                // More details:
                // Map files are supposed to be named "pagexxx.pcx" ("page001.pcx" etc.).
                // Some missions have a file in the intrface base dir called "map.pcx".
                // From what I can tell, this looks like it's supposed to be the background (or "surrounding")
                // image for the map screen. But GatB-T2 puts the actual map image itself in this file, and has
                // no proper pagexxx.pcx file. But it does have an automap file (although it appears not to work
                // in-game, go figure). I have no reasonable way to detect this situation; I just have to trust
                // that files are what they're supposed to be. But since an automap requires a map, it makes
                // sense to set HasMap to true if HasAutomap is true, and that just coincidentally makes GatB-T2
                // map detection accurate. So result achieved and no harm done.
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
}
