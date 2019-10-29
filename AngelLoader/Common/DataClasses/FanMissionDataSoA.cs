using System;
using System.Collections.Generic;
using static AngelLoader.Common.GameSupport;

namespace AngelLoader.Common.DataClasses
{
    internal static class FanMissionDataSoA
    {
        internal enum FMField
        {
            NoArchive,
            MarkedScanned,
            Archive,
            InstalledDir,
            Title,
            AltTitles,
            Author,
            Game,
            Installed,
            NoReadmes,
            SelectedReadme,
            SizeBytes,
            Rating,
            ReleaseDate,
            LastPlayed,
            FinishedOn,
            FinishedOnUnknown,
            CommentSingleLine,
            Comment,
            DisabledMods,
            DisableAllMods,
            HasMap,
            HasAutomap,
            HasScripts,
            HasTextures,
            HasSounds,
            HasObjects,
            HasCreatures,
            HasMotions,
            HasMovies,
            HasSubtitles,
            LanguagesString,
            Tags,
            TagsString
        }

        internal static void AddNewFMWithDefaultValues()
        {
            NoArchiveList.Add(default);
            MarkedScannedList.Add(default);
            ArchiveList.Add("");
            InstalledDirList.Add("");
            TitleList.Add("");
            AltTitlesList.Add(new List<string>());
            AuthorList.Add("");
            GameList.Add(default);
            InstalledList.Add(default);
            NoReadmesList.Add(default);
            SelectedReadmeList.Add("");
            SizeBytesList.Add(default);
            RatingList.Add(default);
            ReleaseDateList.Add(default);
            LastPlayedList.Add(default);
            FinishedOnList.Add(default);
            FinishedOnUnknownList.Add(default);
            CommentSingleLineList.Add("");
            CommentList.Add("");
            DisabledModsList.Add("");
            DisableAllModsList.Add(default);
            HasMapList.Add(default);
            HasAutomapList.Add(default);
            HasScriptsList.Add(default);
            HasTexturesList.Add(default);
            HasSoundsList.Add(default);
            HasObjectsList.Add(default);
            HasCreaturesList.Add(default);
            HasMotionsList.Add(default);
            HasMoviesList.Add(default);
            HasSubtitlesList.Add(default);
            LanguagesStringList.Add("");
            TagsList.Add(new CatAndTagsList());
            TagsStringList.Add("");
        }

        internal static readonly List<bool> NoArchiveList = new List<bool>();
        internal static readonly List<bool> MarkedScannedList = new List<bool>();
        internal static readonly List<string> ArchiveList = new List<string>();
        internal static readonly List<string> InstalledDirList = new List<string>();
        internal static readonly List<string> TitleList = new List<string>();
        internal static readonly List<List<string>> AltTitlesList = new List<List<string>>();
        internal static readonly List<string> AuthorList = new List<string>();
        internal static readonly List<Game> GameList = new List<Game>();
        internal static readonly List<bool> InstalledList = new List<bool>();
        internal static readonly List<bool> NoReadmesList = new List<bool>();
        internal static readonly List<string> SelectedReadmeList = new List<string>();
        internal static readonly List<ulong> SizeBytesList = new List<ulong>();
        internal static readonly List<int> RatingList = new List<int>();
        internal static readonly List<DateTime?> ReleaseDateList = new List<DateTime?>();
        internal static readonly List<DateTime?> LastPlayedList = new List<DateTime?>();
        internal static readonly List<uint> FinishedOnList = new List<uint>();
        internal static readonly List<bool> FinishedOnUnknownList = new List<bool>();
        internal static readonly List<string> CommentSingleLineList = new List<string>();
        internal static readonly List<string> CommentList = new List<string>();
        internal static readonly List<string> DisabledModsList = new List<string>();
        internal static readonly List<bool> DisableAllModsList = new List<bool>();
        internal static readonly List<bool?> HasMapList = new List<bool?>();
        internal static readonly List<bool?> HasAutomapList = new List<bool?>();
        internal static readonly List<bool?> HasScriptsList = new List<bool?>();
        internal static readonly List<bool?> HasTexturesList = new List<bool?>();
        internal static readonly List<bool?> HasSoundsList = new List<bool?>();
        internal static readonly List<bool?> HasObjectsList = new List<bool?>();
        internal static readonly List<bool?> HasCreaturesList = new List<bool?>();
        internal static readonly List<bool?> HasMotionsList = new List<bool?>();
        internal static readonly List<bool?> HasMoviesList = new List<bool?>();
        internal static readonly List<bool?> HasSubtitlesList = new List<bool?>();
        internal static readonly List<string> LanguagesStringList = new List<string>();
        internal static readonly List<CatAndTagsList> TagsList = new List<CatAndTagsList>();
        internal static readonly List<string> TagsStringList = new List<string>();
    }
}
