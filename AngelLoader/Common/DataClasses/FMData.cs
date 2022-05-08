﻿#define FenGen_FMDataSource

using System;
using System.Collections.Generic;
using static AL_Common.Common;
using static AngelLoader.FenGenAttributes;
using static AngelLoader.GameSupport;
using static AngelLoader.LanguageSupport;

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

        internal bool Pinned;

        // For FMs that have metadata but don't exist on disk
        [FenGenIgnore]
        internal bool MarkedUnavailable;

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

        [FenGenIgnore]
        internal readonly DictionaryI<int> ReadmeCodePages = new DictionaryI<int>();
        [FenGenIniName("ReadmeEncoding")]
        [FenGenListType("MultipleLines")]
        internal readonly List<string> ReadmeAndCodePageEntries = new List<string>();

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
        // ReSharper disable once RedundantDefaultMemberInitializer
        private uint _finishedOn = 0;
        [FenGenNumericEmpty(0)]
        internal uint FinishedOn { get => _finishedOn; set => _finishedOn = value.Clamp(0u, 15u); }
        internal bool FinishedOnUnknown;

        [FenGenIgnore]
        internal string CommentSingleLine = "";
        [FenGenDoNotTrimValue]
        internal string Comment = "";

        internal string DisabledMods = "";

        /// <summary>
        /// This is for backward compatibility only. Use only for that purpose.
        /// </summary>
        [FenGenDoNotWrite]
        internal bool DisableAllMods;

        [FenGenIgnore]
        internal bool ResourcesScanned;
        [FenGenIniName("HasResources")]
        internal CustomResources Resources = CustomResources.None;

        // TODO(FMData): Langs could be made into enums and flags, because we only support specific ones.
        // This would save a lot of memory from not having them all be strings, in exchange for having to convert
        // them on ini read/write.

        internal bool LangsScanned;
        //[FenGenIgnore]
        //internal string Langs = "";
        //[FenGenIgnore]
        //private string _selectedLang = "";
        //internal string SelectedLang
        //{
        //    get => _selectedLang;
        //    set => _selectedLang = value.ToLowerInvariant();
        //}

        /*
        The plan is to do like this...

        string[] langs = valTrimmed.Split(AL_Common.Common.CA_Comma, StringSplitOptions.RemoveEmptyEntries);
        for (int i = 0; i < langs.Length; i++)
        {
            langs[i] = langs[i].Trim().ToLowerInvariant();
            if (LanguageSupport.LangStringsToEnums.TryGetValue(langs[i], out LanguageSupport.Language index))
            {
                fm.LangsE |= index;
            }
        }

        ... except in a no-alloc version. The plan:

        -Generate a perfect hash for the keyword set

        Then in the FMData.ini reader:
                
        -Loop:
         -Get start and end indexes of next comma-separated item (whitespace ignored)
         -Iterate the section of the string, replacing-in-place all ASCII uppercase chars to lowercase, and if
          any non-ASCII are found, reject the item and move on to the next
         -Pass line with start and end indexes to perfect hash lookup function, where since it's guaranteed to be
          lowercase and the Hash function can just take the start and length, it will work just like normal
         -If we get a value back, fm.LangsE |= value

        EDIT 2022-05-07:
        No that in-place thing is dumb, we can just check the string is ascii lowercase, and if isn't, we can
        just fall back to taking a ToLowerInvariant() allocation once, and then when we write the langs out
        they WILL be ascii lowercase only, so we won't take the allocation again.
        */

        [FenGenIniName("Langs")]
        internal Language LangsE = Language.Default;

        [FenGenIniName("SelectedLang")]
        [FenGenFlagsSingleAssignment]
        internal Language SelectedLangE = Language.Default;

        [FenGenIgnore]
        internal readonly FMCategoriesCollection Tags = new FMCategoriesCollection();
        internal string TagsString = "";
    }
}
