using System;
using System.Collections.Generic;
using AngelLoader.Common.Utility;

namespace AngelLoader.Common.DataClasses
{
    // FenGen reads this and outputs fast ini read and write methods.
    // TODO: Document FenGen options somewhere
    // TODO: Version-header ini files right from the start, in case I have to change the format after release
    /*
     Notes to self:
        -Keep properties to one line, cause FenGen doesn't handle multiline ones
        -Keep names shortish for more performance when reading
    */

    // [FenGen:WriteEmptyValues=false]
    internal sealed class FanMission
    {
        // Used as a performance optimization for merging
        // [FenGen:DoNotSerialize]
        internal bool Checked = false;

        internal string Archive = "";
        internal string InstalledDir = "";

        internal string Title = "";
        // [FenGen:ListType=MultipleLines]
        internal List<string> AltTitles = new List<string>();

        internal string Author = "";

        internal Game? Game;

        internal bool Installed;

        internal bool RefreshCache;

        internal bool NoReadmes;
        internal string SelectedReadme = "";

        // [FenGen:DoNotSerialize]
        internal string SizeString = "";
        // [FenGen:DoNotSerialize]
        private ulong _sizeBytes = 0;
        // [FenGen:NumericEmpty=0]
        internal ulong SizeBytes { get => _sizeBytes; set => _sizeBytes = value.Clamp(ulong.MinValue, ulong.MaxValue); }

        // [FenGen:DoNotSerialize]
        private int _rating = -1;
        // [FenGen:NumericEmpty=-1]
        internal int Rating { get => _rating; set => _rating = value.Clamp(-1, 10); }

        internal DateTime? ReleaseDate = null;
        internal DateTime? LastPlayed = null;

        // [FenGen:DoNotSerialize]
        private int _finishedOn = 0;
        // [FenGen:NumericEmpty=0]
        internal int FinishedOn { get => _finishedOn; set => _finishedOn = value.Clamp(0, 15); }
        internal bool FinishedOnUnknown;

        // [FenGen:DoNotSerialize]
        internal string CommentSingleLine = "";
        internal string Comment = "";

        internal string DisabledMods = "";
        internal bool DisableAllMods;

        internal bool? HasMap;
        internal bool? HasAutomap;
        internal bool? HasScripts;
        internal bool? HasTextures;
        internal bool? HasSounds;
        internal bool? HasObjects;
        internal bool? HasCreatures;
        internal bool? HasMotions;
        internal bool? HasMovies;
        internal bool? HasSubtitles;

        // [FenGen:DoNotSerialize]
        internal string[] Languages;
        internal string LanguagesString;

        // [FenGen:DoNotSerialize]
        internal List<CatAndTags> Tags = new List<CatAndTags>();
        internal string TagsString;
    }
}
