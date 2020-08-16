using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.RegularExpressions;
using JetBrains.Annotations;

namespace FMScanner
{
    [PublicAPI]
    internal static class ByteSize
    {
        internal const int KB = 1024;
        internal const int MB = KB * 1024;
        internal const int GB = MB * 1024;
    }

    /*
     2020-07-31: This stuff is now part of the non-static Scanner class so it can be garbage-collected, rather
     than being static and sticking around forever. This stuff will only be instantiated once per scan (whether
     for single or multiple FMs) so allocations and GC pressure aren't really a problem. The only time we would
     do a bunch of single-FM scans in a row would be if the user just went through the list scanning everything
     manually one after another, which would take forever and no one would do. Or if they cancelled the all-FMs
     scan and then ran down the list selecting every FM one after another, thus auto-scanning them. If they do
     that, then meh. It'll be slow anyway in that case.
    */
    public sealed partial class Scanner
    {

        [SuppressMessage("ReSharper", "IdentifierTypo")]
        internal static class Constants
        {
            internal const char uldq = '\u201C'; // Unicode left double-quote
            internal const char urdq = '\u201D'; // Unicode right double-quote
        }

        [SuppressMessage("ReSharper", "IdentifierTypo")]
        private static class FMDirs
        {
            // PERF: const string concatenation is free (const concats are done at compile time), so do it to lessen
            // the chance of error.

            internal const string Books = "books";
            internal const string Fam = "fam";
            internal const string Intrface = "intrface";
            internal const string Mesh = "mesh";
            internal const string Motions = "motions";
            internal const string Movies = "movies";
            internal const string Cutscenes = "cutscenes"; // SS2 only
            internal const string Obj = "obj";
            internal const string Scripts = "scripts";
            internal const string Snd = "snd";
            internal const string Snd2 = "snd2"; // SS2 only
            internal const string Strings = "strings";
            internal const string Subtitles = "subtitles";

            internal const string BooksS = Books + "/";
            internal const string FamS = Fam + "/";
            internal const string IntrfaceS = Intrface + "/";
            internal const int IntrfaceSLen = 9; // workaround for .NET 4.7.2 not inlining const string lengths
            internal const string MeshS = Mesh + "/";
            internal const string MotionsS = Motions + "/";
            internal const string MoviesS = Movies + "/";
            internal const string CutscenesS = Cutscenes + "/"; // SS2 only
            internal const string ObjS = Obj + "/";
            internal const string ScriptsS = Scripts + "/";
            internal const string SndS = Snd + "/";
            internal const string Snd2S = Snd2 + "/"; // SS2 only
            internal const string StringsS = Strings + "/";
            internal const string SubtitlesS = Subtitles + "/";

            internal const string T3FMExtras1 = "Fan Mission Extras";
            internal const string T3FMExtras2 = "FanMissionExtras";
            internal const string T3FMExtras1S = T3FMExtras1 + "/";
            internal const string T3FMExtras2S = T3FMExtras2 + "/";

            internal const string T3DetectS = "Content/T3/Maps/";
            internal const int T3DetectSLen = 16; // workaround for .NET 4.7.2 not inlining const string lengths
        }

        [SuppressMessage("ReSharper", "IdentifierTypo")]
        [SuppressMessage("ReSharper", "CommentTypo")]
        private static class FMFiles
        {
            internal const string SS2Fingerprint1 = "/usemsg.str";
            internal const string SS2Fingerprint2 = "/savename.str";
            internal const string SS2Fingerprint3 = "/objlooks.str";
            internal const string SS2Fingerprint4 = "/OBJSHORT.str";

            internal const string IntrfaceEnglishNewGameStrS = "intrface/english/newgame.str";
            internal const string IntrfaceNewGameStrS = "intrface/newgame.str";
            internal const string DscNewGameStrS = "/newgame.str";

            internal const string StringsMissFlag = "strings/missflag.str";
            internal const string StringsEnglishMissFlag = "strings/english/missflag.str";
            internal const string SMissFlag = "/missflag.str";

            // Telliamed's fminfo.xml file, used in a grand total of three missions
            internal const string FMInfoXml = "fminfo.xml";

            // fm.ini, a NewDark (or just FMSel?) file
            internal const string FMIni = "fm.ini";

            // System Shock 2 file
            internal const string ModIni = "mod.ini";
        }

        #region Non-const FM Files

        private readonly string[]
        FMFiles_TitlesStrLocations =
        {
            // Do not change search order: strings/english, strings, strings/[any other language]
            "strings/english/titles.str",
            "strings/english/title.str",
            "strings/titles.str",
            "strings/title.str",

            "strings/czech/titles.str",
            "strings/dutch/titles.str",
            "strings/french/titles.str",
            "strings/german/titles.str",
            "strings/hungarian/titles.str",
            "strings/italian/titles.str",
            "strings/japanese/titles.str",
            "strings/polish/titles.str",
            "strings/russian/titles.str",
            "strings/spanish/titles.str",

            "strings/czech/title.str",
            "strings/dutch/title.str",
            "strings/french/title.str",
            "strings/german/title.str",
            "strings/hungarian/title.str",
            "strings/italian/title.str",
            "strings/japanese/title.str",
            "strings/polish/title.str",
            "strings/russian/title.str",
            "strings/spanish/title.str"
        };

        // Used for SS2 fingerprinting for the game type scan fallback
        [SuppressMessage("ReSharper", "StringLiteralTypo")]
        private readonly string[]
        FMFiles_SS2MisFiles =
        {
            "command1.mis",
            "command2.mis",
            "earth.mis",
            "eng1.mis",
            "eng2.mis",
            "hydro1.mis",
            "hydro2.mis",
            "hydro3.mis",
            "many.mis",
            "medsci1.mis",
            "medsci2.mis",
            "ops1.mis",
            "ops2.mis",
            "ops3.mis",
            "ops4.mis",
            "rec1.mis",
            "rec2.mis",
            "rec3.mis",
            "rick1.mis",
            "rick2.mis",
            "rick3.mis",
            "shodan.mis",
            "station.mis"
        };

        #endregion

        #region RTF

        private readonly byte[] RtfTags_HeaderBytes = Encoding.ASCII.GetBytes(@"{\rtf1");
        private const int RtfTags_HeaderBytesLength = 6; // stupid micro-optimization

        #endregion

        #region Preallocated arrays

        // Perf, for passing to params[]-taking methods so we don't allocate all the time

        private readonly char[] CA_Period = { '.' };
        private readonly char[] CA_Asterisk = { '*' };
        private readonly char[] CA_Hyphen = { '-' };
        private readonly char[] CA_CommaSemicolon = { ',', ';' };
        private readonly char[] CA_DoubleQuote = { '\"' };
        private readonly char[] CA_UnicodeQuotes = { Constants.uldq, Constants.urdq };
        private readonly string[] SA_CRLF = { "\r\n" };
        private readonly string[] SA_DoubleSpaces = { "  " };
        private readonly string[] SA_T3DetectExtensions = { "*.ibt", "*.cbt", "*.gmp", "*.ned", "*.unr" };
        private readonly string[] SA_AllFiles = { "*" };
        private readonly string[] SA_AllBinFiles = { "*.bin" };
        private readonly string[] SA_AllSubFiles = { "*.sub" };

        #region Field detect strings

        private readonly string[] SA_TitleDetect =
        {
            "Title of the Mission", "Title of the mission",
            "Title", "Mission Title", "Mission title", "Mission Name", "Mission name", "Level Name",
            "Level name", "Mission:", "Mission ", "Campaign Title", "Campaign title",
            "The name of Mission:",
            // TODO: @TEMP_HACK: This works for the one mission that has it in this casing
            // Rewrite this code in here so we can have more detailed detection options than just
            // these silly strings and the default case check
            "Fan Mission/Map Name"
        };

        private readonly string[] SA_AuthorDetect =
        {
            "Author", "Authors", "Autor",
            "Created by", "Devised by", "Designed by", "Author=", "Made by",
            "FM Author", "Mission Author", "Mission author", "Mission Creator", "Mission creator",
            "The author:", "author:",
            // TODO: @TEMP_HACK: See above
            "Fan Mission/Map Author"
        };

        private readonly string[] SA_ReleaseDateDetect =
        {
            "Date Of Release", "Date of Release",
            "Date of release", "Release Date", "Release date"
        };

        private readonly string[] SA_VersionDetect = { "Version" };

        #endregion

        #endregion

        #region Extension and file pattern arrays

        // Ordered by number of actual total occurrences across all FMs:
        // gif: 153,294
        // pcx: 74,786
        // tga: 12,622
        // dds: 11,647
        // png: 11,290
        // bmp: 657
        private readonly string[] ImageFileExtensions = { ".gif", ".pcx", ".tga", ".dds", ".png", ".bmp" };
        private readonly string[] ImageFilePatterns = { "*.gif", "*.pcx", "*.tga", "*.dds", "*.png", "*.bmp" };

        private readonly string[] MotionFilePatterns = { "*.mc", "*.mi" };
        private readonly string[] MotionFileExtensions = { ".mc", ".mi" };

        // .osm for the classic scripts; .nut for Squirrel scripts for NewDark >= 1.25
        private readonly string[] ScriptFileExtensions = { ".osm", ".nut" };

        #endregion

        #region Languages

        // NOTE: I think this was for GetLanguages() for the planned accuracy update?
        //private string[] LanguageDirs { get; } = { FMDirs.Books, FMDirs.Intrface, FMDirs.Strings };

        // Perf micro-optimization: don't create a new list if we're only returning English
        private readonly List<string> EnglishOnly = new List<string> { "english" };

        private readonly string[]
        Languages =
        {
            "english",
            "czech",
            "dutch",
            "french",
            "german",
            "hungarian",
            "italian",
            "japanese",
            "polish",
            "russian",
            "spanish"
        };

        // Perf: avoids concats

        private readonly string[]
        Languages_FS_Lang_FS =
        {
            "/english/",
            "/czech/",
            "/dutch/",
            "/french/",
            "/german/",
            "/hungarian/",
            "/italian/",
            "/japanese/",
            "/polish/",
            "/russian/",
            "/spanish/"
        };

        private readonly string[]
        Languages_FS_Lang_Language_FS =
        {
            "/english Language/",
            "/czech Language/",
            "/dutch Language/",
            "/french Language/",
            "/german Language/",
            "/hungarian Language/",
            "/italian Language/",
            "/japanese Language/",
            "/polish Language/",
            "/russian Language/",
            "/spanish Language/"
        };

        // Cheesy hack because it wasn't designed this way
        private readonly Dictionary<string, string>
        LanguagesC = new Dictionary<string, string>
        {
            { "english", "English" },
            { "czech", "Czech" },
            { "dutch", "Dutch" },
            { "french", "French" },
            { "german", "German" },
            { "hungarian", "Hungarian" },
            { "italian", "Italian" },
            { "japanese", "Japanese" },
            { "polish", "Polish" },
            { "russian", "Russian" },
            { "spanish", "Spanish" }
        };

        #endregion

        private readonly string[]
        DateFormats =
        {
            "MMM d yy",
            "MMM d, yy",
            "MMM dd yy",
            "MMM dd, yy",

            "MMM d yyyy",
            "MMM d, yyyy",
            "MMM dd yyyy",
            "MMM dd, yyyy",

            "MMMM d yy",
            "MMMM d, yy",
            "MMMM dd yy",
            "MMMM dd, yy",

            "MMMM d yyyy",
            "MMMM d, yyyy",
            "MMMM dd yyyy",
            "MMMM dd, yyyy",

            "d MMM yy",
            "d MMM, yy",
            "dd MMM yy",
            "dd MMM, yy",

            "d MMM yyyy",
            "d MMM, yyyy",
            "dd MMM yyyy",
            "dd MMM, yyyy",

            "d MMMM yy",
            "d MMMM, yy",
            "dd MMMM yy",
            "dd MMMM, yy",

            "d MMMM yyyy",
            "d MMMM, yyyy",
            "dd MMMM yyyy",
            "dd MMMM, yyyy",

            "yyyy MMM d",
            "yyyy MMM dd",
            "yyyy MMMM d",
            "yyyy MMMM dd",

            "MM/dd/yyyy",
            "dd/MM/yyyy",
            "MM/dd/yy",
            "dd/MM/yy",

            "M/d/yyyy",
            "d/M/yyyy",
            "M/d/yy",
            "d/M/yy",

            "MM-dd-yyyy",
            "dd-MM-yyyy",
            "MM-dd-yy",
            "dd-MM-yy",

            "M-d-yyyy",
            "d-M-yyyy",
            "M-d-yy",
            "d-M-yy"

            // TODO: Ambiguous months and days might pose a problem?
        };

        #region Game detect strings

        // ReSharper disable StringLiteralTypo
        // ReSharper disable IdentifierTypo

        private readonly byte[] OBJ_MAP = Encoding.ASCII.GetBytes("OBJ_MAP");

        private readonly byte[] Thief2UniqueString = Encoding.ASCII.GetBytes("RopeyArrow");

        // SS2-only detection string
        private readonly byte[] MAPPARAM = Encoding.ASCII.GetBytes("MAPPARAM");

        // ReSharper restore IdentifierTypo
        // ReSharper restore StringLiteralTypo

        #endregion

        #region Regexes

        // PERF: Making regexes compiled increases their performance by a huge amount. And as we know, regexes
        // need all the performance help they can get.
        private readonly Regex GLMLTagRegex =
            new Regex(@"\[/?GL[A-Z]+\]", RegexOptions.Compiled);

        private readonly Regex AThief3Mission =
            new Regex(@"^A\s+Thief(\s+|\s+:\s+|\s+-\s+)Deadly",
                RegexOptions.ExplicitCapture | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

        private readonly Regex OpenParenSpacesRegex =
            new Regex(@"\(\s+", RegexOptions.Compiled);

        private readonly Regex CloseParenSpacesRegex =
            new Regex(@"\s+\)", RegexOptions.Compiled);

        private readonly Regex DaySuffixesRegex =
            new Regex(@"\d(?<Suffix>(st|nd|rd|th)).+",
                RegexOptions.ExplicitCapture | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

        private readonly Regex VersionExclude1Regex =
            new Regex(@"\d\.\d+\+", RegexOptions.Compiled);

        private readonly Regex TitleAnyConsecutiveLettersRegex =
            new Regex(@"\w\w", RegexOptions.Compiled);

        // TODO: [a-z] is only ASCII letters, so it won't catch lowercase other stuff I guess
        private readonly Regex TitleContainsLowerCaseCharsRegex =
            new Regex(@"[a-z]", RegexOptions.Compiled);

        private readonly Regex AuthorEmailRegex =
            new Regex(@"\(?\S+@\S+\.\S{2,5}\)?", RegexOptions.Compiled);

        // This doesn't need to be a regex really, but it takes like 5.4 microseconds per FM, so, yeah
        private readonly Regex NewGameStrTitleRegex =
            new Regex(@"^skip_training\:\s*""(?<Title>.+)""",
                RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

        // TODO: This one looks iffy though
        private readonly Regex VersionFirstNumberRegex =
            new Regex(@"[0123456789\.]+", RegexOptions.Compiled);

        // Much, much faster to iterate through possible regex matches, common ones first
        // TODO: These are still kinda slow comparatively. Profile to see if any are bottlenecks
        private readonly Regex[] NewDarkVersionRegexes =
        {
            new Regex(@"NewDark (?<Version>\d\.\d+)",
                RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled |
                RegexOptions.ExplicitCapture),
            new Regex(@"(New ?Dark|""New ?Dark"").? v?(\.| )?(?<Version>\d\.\d+)",
                RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled |
                RegexOptions.ExplicitCapture),
            new Regex(@"(New ?Dark|""New ?Dark"").? .?(Version|Patch) .?(?<Version>\d\.\d+)",
                RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled |
                RegexOptions.ExplicitCapture),
            new Regex(@"(Dark ?Engine) (Version.?|v)?(\.| )?(?<Version>\d\.\d+)",
                RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled |
                RegexOptions.ExplicitCapture),
            new Regex(
                @"((?<!(Love |Being |Penitent |Counter-|Requiem for a |Space ))Thief|(?<!Being )Thief ?(2|II)|The Metal Age) v?(\.| )?(?<Version>\d\.\d+)",
                RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled |
                RegexOptions.ExplicitCapture),
            new Regex(
                @"\D(?<Version>\d\.\d+) (version of |.?)New ?Dark(?! ?\d\.\d+)|Thief Gold( Patch)? (?<Version>(?!1\.33|1\.37)\d\.\d+)",
                RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled |
                RegexOptions.ExplicitCapture),
            new Regex(@"Version (?<Version>\d\.\d+) of (Thief ?(2|II))",
                RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled |
                RegexOptions.ExplicitCapture),
            new Regex(@"(New ?Dark|""New ?Dark"") (is )?required (.? )v?(\.| )?(?<Version>\d\.\d+)",
                RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled |
                RegexOptions.ExplicitCapture),
            new Regex(@"(?<Version>(?!1\.3(3|7))\d\.\d+) Patch",
                RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled |
                RegexOptions.ExplicitCapture)

            // Original regex for reference - slow!
            // @"((?<Name>(""*New *Dark""*( Version| Patch)*|Dark *Engine|(?<!(Love |Being |Penitent |Counter-|Requiem for a |Space ))Thief|(?<!Being )Thief *2|Thief *II|The Metal Age)) *V?(\.| )*(?<Version>\d\.\d+)|\D(?<Version>\d\.\d+) +(version of |(?!\r\n).?)New *Dark(?! *\d\.\d+)|Thief Gold( Patch)* (?<Version>(?!1\.33|1\.37)\d\.\d+))",
        };

        private readonly Regex[] AuthorRegexes =
        {
            new Regex(
                @"(FM|mis(si|is|i)on|campaign|series) for Thief( Gold|: The Dark Project|\s*2(: The Metal Age)?)\s+by\s*(?<Author>.+)",
                RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled |
                RegexOptions.ExplicitCapture),
            new Regex(
                @"(A )?Thief( Gold|: The Dark Project|\s*2(: The Metal Age)?) (fan(-| ?)mis((si|is|i)on)|FM|campaign)\s+by (?<Author>.+)",
                RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled |
                RegexOptions.ExplicitCapture),
            new Regex(
                @"A(n)? (fan(-| ?)mis((si|is|i)on)|FM|campaign)\s+(made\s+)?by\s+(?<Author>.+)",
                RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled |
                RegexOptions.ExplicitCapture),
            new Regex(
                @"A(n)? .+(-| )part\s+Thief( Gold |: The Dark Project |\s*2(: The Metal Age )?)\s+(fan(-| ?)mis((si|is|i)on)|FM|campaign)\s+((made\s+by)|by|from)\s+(?<Author>.+)",
                RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled |
                RegexOptions.ExplicitCapture)
        };

        private const string _copyrightSecondPart =
            //language=regexp
            @"(?<Months>( (Jan|Febr)uary| Ma(rch|y)| A(pril|ugust)| Ju(ne|ly)| (((Sept|Nov|Dec)em)|Octo)ber))?" +
            //language=regexp
            @"(?(Months)(, ?| ))\d*( by| to)? (?<Author>.+)";

        // Unicode 00A9 = copyright symbol

        private readonly Regex[] AuthorMissionCopyrightRegexes =
        {
            new Regex(
                //language=regexp
                @"^This (level|(fan(-| |))?mis(si|is|i)on|FM) is( made)? (\(c\)|\u00A9) ?" + _copyrightSecondPart,
                RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled |
                RegexOptions.ExplicitCapture),
            new Regex(
                //language=regexp
                @"^The (levels?|(fan(-| |))?mis(si|is|i)ons?|FMs?)( in this (zip|archive( file)?))? (is|are)( made)? (\(c\)|\u00A9) ?" +
                _copyrightSecondPart,
                RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled |
                RegexOptions.ExplicitCapture),
            new Regex(
                //language=regexp
                @"^These (levels|(fan(-| |))?mis(si|is|i)ons|FMs) are( made)? (\(c\)|\u00A9) ?" +
                _copyrightSecondPart,
                RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled |
                RegexOptions.ExplicitCapture)
        };

        // This one is only to be used if we know the above line says "Copyright" or something, because it has
        // an @ as an option for a copyright symbol (used by some Theker missions) and we want to be sure it
        // means what we think it means.
        private readonly Regex AuthorGeneralCopyrightIncludeAtSymbolRegex =
            new Regex(
                //language=regexp
                @"^(Copyright )?(\(c\)|\u00A9|@) ?" + _copyrightSecondPart,
                RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled | RegexOptions.ExplicitCapture);

        private readonly Regex AuthorGeneralCopyrightRegex =
            new Regex(
                //language=regexp
                @"^(Copyright )?(\(c\)|\u00A9) ?" + _copyrightSecondPart,
                RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled | RegexOptions.ExplicitCapture);

        private readonly Regex CopyrightAuthorYearRegex = new Regex(@" \d+.*$", RegexOptions.Compiled);

        private readonly Regex TitleByAuthorRegex =
            new Regex(@"(\s+|\s*(:|-|\u2013|,)\s*)by(\s+|\s*(:|-|\u2013)\s*)(?<Author>.+)",
                RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled | RegexOptions.ExplicitCapture);

        #endregion

        /// <summary>
        /// Specialized (therefore fast) sort for titles.str lines only. Anything else is likely to throw an
        /// IndexOutOfRangeException.
        /// </summary>
        private sealed class TitlesStrNaturalNumericSort : IComparer<string>
        {
            public int Compare(string x, string y)
            {
                if (x == y) return 0;
                if (x.IsEmpty()) return -1;
                if (y.IsEmpty()) return 1;

                int xIndex1;
                string xNum = x.Substring(xIndex1 = x.IndexOf('_') + 1, x.IndexOf(':') - xIndex1);
                int yIndex1;
                string yNum = y.Substring(yIndex1 = y.IndexOf('_') + 1, y.IndexOf(':') - yIndex1);

                while (xNum.Length < yNum.Length) xNum = '0' + xNum;
                while (yNum.Length < xNum.Length) yNum = '0' + yNum;

                return string.CompareOrdinal(xNum, yNum);
            }
        }
    }
}
