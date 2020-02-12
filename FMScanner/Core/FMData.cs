/*
FMScanner - A fast, thorough, accurate scanner for Thief 1 and Thief 2 fan missions.

Written in 2017-2020 by FenPhoenix.

To the extent possible under law, the author(s) have dedicated all copyright and related and neighboring rights
to this software to the public domain worldwide. This software is distributed without any warranty.

You should have received a copy of the CC0 Public Domain Dedication along with this software.
If not, see <http://creativecommons.org/publicdomain/zero/1.0/>.
*/

using System;
using System.Collections.Generic;
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
        /// <param name="scanTitle"></param>
        /// <param name="scanCampaignMissionNames"></param>
        /// <param name="scanAuthor"></param>
        /// <param name="scanVersion"></param>
        /// <param name="scanLanguages"></param>
        /// <param name="scanGameType"></param>
        /// <param name="scanNewDarkRequired"></param>
        /// <param name="scanNewDarkMinimumVersion"></param>
        /// <param name="scanCustomResources"></param>
        /// <param name="scanSize"></param>
        /// <param name="scanReleaseDate"></param>
        /// <param name="scanTags"></param>
        /// <returns></returns>
        public static ScanOptions FalseDefault(bool scanTitle = false, bool scanCampaignMissionNames = false,
            bool scanAuthor = false, bool scanVersion = false, bool scanLanguages = false,
            bool scanGameType = false, bool scanNewDarkRequired = false, bool scanNewDarkMinimumVersion = false,
            bool scanCustomResources = false, bool scanSize = false, bool scanReleaseDate = false,
            bool scanTags = false) =>
            new ScanOptions
            {
                ScanTitle = scanTitle,
                ScanCampaignMissionNames = scanCampaignMissionNames,
                ScanAuthor = scanAuthor,
                ScanVersion = scanVersion,
                ScanLanguages = scanLanguages,
                ScanGameType = scanGameType,
                ScanNewDarkRequired = scanNewDarkRequired,
                ScanNewDarkMinimumVersion = scanNewDarkMinimumVersion,
                ScanCustomResources = scanCustomResources,
                ScanSize = scanSize,
                ScanReleaseDate = scanReleaseDate,
                ScanTags = scanTags
            };

        internal ScanOptions DeepCopy() =>
            new ScanOptions
            {
                ScanTitle = ScanTitle,
                ScanCampaignMissionNames = ScanCampaignMissionNames,
                ScanAuthor = ScanAuthor,
                ScanVersion = ScanVersion,
                ScanLanguages = ScanLanguages,
                ScanGameType = ScanGameType,
                ScanNewDarkRequired = ScanNewDarkRequired,
                ScanNewDarkMinimumVersion = ScanNewDarkMinimumVersion,
                ScanCustomResources = ScanCustomResources,
                ScanSize = ScanSize,
                ScanReleaseDate = ScanReleaseDate,
                ScanTags = ScanTags
            };

        /// <summary>
        /// <see langword="true"/> to detect the mission's title.
        /// </summary>
        public bool ScanTitle { get; set; } = true;
        /// <summary>
        /// <see langword="true"/> to detect the titles of individual campaign missions.
        /// If the mission is not a campaign, this option has no effect.
        /// If the mission is for Thief: Deadly Shadows, this option has no effect.
        /// </summary>
        public bool ScanCampaignMissionNames { get; set; } = true;
        /// <summary>
        /// <see langword="true"/> to detect the mission's author.
        /// </summary>
        public bool ScanAuthor { get; set; } = true;
        /// <summary>
        /// <see langword="true"/> to detect the mission's version.
        /// </summary>
        public bool ScanVersion { get; set; } = true;
        /// <summary>
        /// <see langword="true"/> to detect the languages the mission supports.
        /// If the mission is for Thief: Deadly Shadows, this option has no effect.
        /// </summary>
        public bool ScanLanguages { get; set; } = true;
        /// <summary>
        /// <see langword="true"/> to detect which game the mission is for (Thief 1, Thief 2, Thief 3, or System Shock 2).
        /// </summary>
        public bool ScanGameType { get; set; } = true;
        /// <summary>
        /// <see langword="true"/> to detect whether the mission requires NewDark.
        /// If the mission is for Thief: Deadly Shadows, this option has no effect.
        /// </summary>
        public bool ScanNewDarkRequired { get; set; } = true;
        /// <summary>
        /// <see langword="true"/> to detect the minimum NewDark version the mission requires.
        /// If ScanNewDarkRequired is false, this option has no effect.
        /// If the mission is for Thief: Deadly Shadows, this option has no effect.
        /// </summary>
        public bool ScanNewDarkMinimumVersion { get; set; } = true;
        /// <summary>
        /// <see langword="true"/> to detect whether the mission contains custom resources.
        /// If the mission is for Thief: Deadly Shadows, this option has no effect.
        /// </summary>
        public bool ScanCustomResources { get; set; } = true;
        /// <summary>
        /// <see langword="true"/> to detect the size of the mission. This will differ depending on whether the
        /// mission is a compressed archive or an uncompressed directory.
        /// </summary>
        public bool ScanSize { get; set; } = true;
        /// <summary>
        /// <see langword="true"/> to detect the mission's release date.
        /// </summary>
        public bool ScanReleaseDate { get; set; } = true;
        /// <summary>
        /// <see langword="true"/> to detect the mission's tags.
        /// </summary>
        public bool ScanTags { get; set; } = true;
    }

    [PublicAPI]
    public sealed class ProgressReport
    {
        public string FMName = ""; // non-null for safety
        public int FMNumber;
        public int FMsTotal;
        public int Percent;
        public bool Finished;
    }

    [PublicAPI]
    public enum Game
    {
        Null,
        Thief1,
        Thief2,
        Thief3,
        SS2,
        Unsupported
    }

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
    }

    [PublicAPI]
    public sealed class ScannedFMData
    {
        public string ArchiveName { get; internal set; }
        public long? Size { get; internal set; }
        public string Title { get; internal set; }
        public List<string> AlternateTitles { get; internal set; } = new List<string>();
        public string Author { get; internal set; }
        public FMType Type { get; internal set; }
        public string[] IncludedMissions { get; internal set; }
        public Game Game { get; internal set; }
        public string[] Languages { get; internal set; }
        public string Version { get; internal set; }
        public bool? NewDarkRequired { get; internal set; }
        public string NewDarkMinRequiredVersion { get; internal set; }
        /// <summary>
        /// Deprecated and will always be blank. Use <see cref="LastUpdateDate"/> instead.
        /// </summary>
        public DateTime? OriginalReleaseDate { get; internal set; }

        private DateTime? _lastUpdateDate;
        public DateTime? LastUpdateDate
        {
            get => _lastUpdateDate;
            // Future years will eventually stop being rejected once the current date passes them, but eh
            internal set => _lastUpdateDate = value != null && ((DateTime)value).Year > DateTime.Now.Year ? null : value;
        }

        public bool? HasCustomScripts { get; internal set; }
        public bool? HasCustomTextures { get; internal set; }
        public bool? HasCustomSounds { get; internal set; }
        public bool? HasCustomObjects { get; internal set; }
        public bool? HasCustomCreatures { get; internal set; }
        public bool? HasCustomMotions { get; internal set; }
        public bool? HasAutomap { get; internal set; }
        public bool? HasMovies { get; internal set; }
        public bool? HasMap { get; internal set; }
        public bool? HasCustomSubtitles { get; internal set; }
        public string Description { get; internal set; }
        public string TagsString { get; internal set; }
    }
}
