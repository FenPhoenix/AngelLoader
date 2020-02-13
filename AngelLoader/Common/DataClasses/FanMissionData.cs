#define FenGen_FMDataSource

using System;
using System.Collections.Generic;
using static AngelLoader.GameSupport;

namespace AngelLoader.DataClasses
{
    // FenGen reads this and outputs fast ini read and write methods.
    // TODO: Version-header ini files right from the start, in case I have to change the format after release
    /*
     Notes to self:
        -Keep properties to one line, cause FenGen doesn't handle multiline ones
        -Keep names shortish for more performance when reading
    */

    // [FenGen:WriteEmptyValues=false]
    internal sealed class FanMission
    {
        // Cached value to avoid doing the expensive check every startup. If a matching archive is found in the
        // normal archive list combine, this will be set to false again. Results in a nice perf gain if there are
        // archive-less FMs in the list.
        internal bool NoArchive;

        // Since our scanned values are very complex due to having the option to choose what to scan for as well
        // as being able to import from three other loaders, we need a simple way to say "scan on select or not".
        internal bool MarkedScanned;

        internal string Archive = "";
        internal string InstalledDir = "";

        internal string Title = "";
        // [FenGen:ListType=MultipleLines]
        internal readonly List<string> AltTitles = new List<string>();

        internal string Author = "";

        internal Game Game = Game.Null;

        internal bool Installed;

        internal bool NoReadmes;
        internal string SelectedReadme = "";

        // [FenGen:DoNotSerialize]
        private ulong _sizeBytes = 0;
        // [FenGen:NumericEmpty=0]
        internal ulong SizeBytes { get => _sizeBytes; set => _sizeBytes = value.Clamp(ulong.MinValue, ulong.MaxValue); }

        // [FenGen:DoNotSerialize]
        private int _rating = -1;
        // [FenGen:NumericEmpty=-1]
        internal int Rating { get => _rating; set => _rating = value.Clamp(-1, 10); }

        internal readonly ExpandableDate ReleaseDate = new ExpandableDate();
        internal readonly ExpandableDate LastPlayed = new ExpandableDate();
        // TODO: Keep and store this value here still
        // We get this value for free when we get the FM archives and dirs on startup, but this value is fragile:
        // it updates whenever the user so much as moves the file or folder. We should store it here to keep it
        // permanent even across moves, new PCs or Windows installs with file restores, etc.
        // NOTE: What we should be doing is marking down the date when we added a given FM to the list, but we
        // would need to have done that from the start for it to work, because now we don't know the add date for
        // anything in the list previously. What we could do is just have a filter button "Show new FMs only" and
        // start tracking added-dates from here on out, and just say anything that doesn't have a date is just
        // ignored and considered not new.
        internal DateTime? DateAdded = null;

        // [FenGen:DoNotSerialize]
        private uint _finishedOn = 0;
        // [FenGen:NumericEmpty=0]
        internal uint FinishedOn { get => _finishedOn; set => _finishedOn = value.Clamp(0u, 15u); }
        internal bool FinishedOnUnknown;

        // [FenGen:DoNotSerialize]
        internal string CommentSingleLine = "";
        internal string Comment = "";

        internal string DisabledMods = "";
        internal bool DisableAllMods;

        // [FenGen:DoNotSerialize]
        internal bool ResourcesScanned;
        internal CustomResources Resources = CustomResources.None;

        // not using for now, but add DoNotSerialize attribute here if I ever do
        // internal string[] Languages;
        internal string LanguagesString = "";

        // [FenGen:DoNotSerialize]
        internal readonly CatAndTagsList Tags = new CatAndTagsList();
        internal string TagsString = "";
    }
}
