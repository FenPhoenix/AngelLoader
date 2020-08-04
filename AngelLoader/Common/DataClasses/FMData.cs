#define FenGen_FMDataSource

using System;
using System.Collections.Generic;
using static AngelLoader.Attributes;
using static AngelLoader.GameSupport;

namespace AngelLoader.DataClasses
{
    // FenGen reads this and outputs fast ini read and write methods.
    /*
     Notes to self:
        -Keep names shortish for more performance when reading
        -I told myself to version-header ini files right from the start, but I didn't. Meh.
    */

    [FenGenFMDataSourceClass(writeEmptyValues: false)]
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

        // Disgusting hack to let an FM disappear from the list after being deleted. It will only be filtered out,
        // but on next run of the FM finder, it will be properly removed if the archive is no longer there.
        [FenGenIgnore]
        internal bool MarkedDeleted;

        internal string Archive = "";
        internal string InstalledDir = "";

        internal string Title = "";
        [FenGenListType("MultipleLines")]
        internal readonly List<string> AltTitles = new List<string>();

        internal string Author = "";

        internal Game Game = Game.Null;

        internal bool Installed;

        internal bool NoReadmes;

        [FenGenIgnore]
        private string _selectedReadme = "";
        // @DIRSEP: Always backslashes for backward compatibility and prevention of find misses in readme chooser box
        internal string SelectedReadme { get => _selectedReadme; set => _selectedReadme = value.ToBackSlashes(); }

        [FenGenNumericEmpty(0)]
        internal ulong SizeBytes = 0;
        
        [FenGenIgnore]
        private int _rating = -1;
        [FenGenNumericEmpty(-1)]
        internal int Rating { get => _rating; set => _rating = value.Clamp(-1, 10); }

        internal readonly ExpandableDate ReleaseDate = new ExpandableDate();
        internal readonly ExpandableDate LastPlayed = new ExpandableDate();

        // We get this value for free when we get the FM archives and dirs on startup, but this value is fragile:
        // it updates whenever the user so much as moves the file or folder. We store it here to keep it permanent
        // even across moves, new PCs or Windows installs with file restores, etc.
        // NOTE: This is not an ExpandableDate, because the way we get the date value is not in unix hex string
        // format, and it's expensive to convert it to such. With a regular nullable DateTime we're only paying
        // like 3-5ms extra on startup (for 1574 FMs), so it's good enough for now.
        [FenGenDoNotConvertDateTimeToLocal]
        internal DateTime? DateAdded = null;

        [FenGenIgnore]
        private uint _finishedOn = 0;
        [FenGenNumericEmpty(0)]
        internal uint FinishedOn { get => _finishedOn; set => _finishedOn = value.Clamp(0u, 15u); }
        internal bool FinishedOnUnknown;

        [FenGenIgnore]
        internal string CommentSingleLine = "";
        [FenGenDoNotTrimValue]
        internal string Comment = "";

        internal string DisabledMods = "";
        internal bool DisableAllMods;

        [FenGenIgnore]
        internal bool ResourcesScanned;
        [FenGenIniName("HasResources")]
        [FenGenInsertAfter("LegacyCustomResources")]
        internal CustomResources Resources = CustomResources.None;

        internal bool LangsScanned;
        internal string Langs = "";
        internal string SelectedLang = "";

        [FenGenIgnore]
        internal readonly CatAndTagsList Tags = new CatAndTagsList();
        internal string TagsString = "";
    }
}
