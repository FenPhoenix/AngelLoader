/*
FMScanner - A fast, thorough, accurate scanner for Thief 1 and Thief 2 fan missions.

Written in 2017-2020 by FenPhoenix.

To the extent possible under law, the author(s) have dedicated all copyright and related and neighboring rights
to this software to the public domain worldwide. This software is distributed without any warranty.

You should have received a copy of the CC0 Public Domain Dedication along with this software.
If not, see <http://creativecommons.org/publicdomain/zero/1.0/>.
*/

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

    internal static class Constants
    {
        internal const char uldq = '\u201C'; // Unicode left double-quote
        internal const char urdq = '\u201D'; // Unicode right double-quote
    }

    internal static class FMDirs
    {
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

        internal const string T3FMExtras1 = "Fan Mission Extras";
        internal const string T3FMExtras2 = "FanMissionExtras";

        internal const string T3DetectS = "Content/T3/Maps/";
        internal const int T3DetectSLen = 16; // workaround for .NET 4.7.2 not inlining const string lengths
        internal const string T3FMExtras1S = "Fan Mission Extras/";
        internal const string T3FMExtras2S = "FanMissionExtras/";

        internal const string BooksS = "books/";
        internal const string FamS = "fam/";
        internal const string IntrfaceS = "intrface/";
        internal const int IntrfaceSLen = 9; // workaround for .NET 4.7.2 not inlining const string lengths
        internal const string MeshS = "mesh/";
        internal const string MotionsS = "motions/";
        internal const string MoviesS = "movies/";
        internal const string CutscenesS = "cutscenes/"; // SS2 only
        internal const string ObjS = "obj/";
        internal const string ScriptsS = "scripts/";
        internal const string SndS = "snd/";
        internal const string Snd2S = "snd2/"; // SS2 only
        internal const string StringsS = "strings/";
        internal const string SubtitlesS = "subtitles/";
    }

    internal static class FMFiles
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

        internal static readonly string[]
        TitlesStrLocations =
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

        // Telliamed's fminfo.xml file, used in a grand total of three missions
        internal const string FMInfoXml = "fminfo.xml";

        // fm.ini, a NewDark (or just FMSel?) file
        internal const string FMIni = "fm.ini";

        // System Shock 2 file
        internal const string ModIni = "mod.ini";

        // Used for SS2 fingerprinting for the game type scan fallback
        internal static readonly string[]
        SS2MisFiles =
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
    }

    // Used for stripping RTF files of embedded images before scanning (performance and memory optimization)
    [SuppressMessage("ReSharper", "IdentifierTypo")]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [SuppressMessage("ReSharper", "StringLiteralTypo")]
    internal static class RtfTags
    {
        internal static readonly byte[] shppict = Encoding.ASCII.GetBytes(@"\*\shppict");
        internal static readonly byte[] objdata = Encoding.ASCII.GetBytes(@"\*\objdata");
        internal static readonly byte[] nonshppict = Encoding.ASCII.GetBytes(@"\nonshppict");
        internal static readonly byte[] pict = Encoding.ASCII.GetBytes(@"\pict");
        internal static readonly byte[] Bytes11 = new byte[11];
    }

    internal static class FMConstants
    {
        #region Preallocated arrays

        // Perf, for passing to params[]-taking methods so we don't allocate all the time

        internal static readonly char[] CA_Period = { '.' };
        internal static readonly char[] CA_Asterisk = { '*' };
        internal static readonly char[] CA_Hyphen = { '-' };
        internal static readonly char[] CA_CommaSemicolon = { ',', ';' };
        internal static readonly char[] CA_Backslash = { '\\' };
        internal static readonly char[] CA_DoubleQuote = { '\"' };
        internal static readonly char[] CA_UnicodeQuotes = { Constants.uldq, Constants.urdq };
        internal static readonly string[] SA_DoubleSpaces = { "  " };
        internal static readonly string[] SA_T3DetectExtensions = { "*.ibt", "*.cbt", "*.gmp", "*.ned", "*.unr" };
        internal static readonly string[] SA_AllFiles = { "*" };
        internal static readonly string[] SA_AllBinFiles = { "*.bin" };
        internal static readonly string[] SA_AllSubFiles = { "*.sub" };

        #region Field detect strings

        internal static readonly string[] SA_TitleDetect =
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

        internal static readonly string[] SA_AuthorDetect =
        {
            "Author", "Authors", "Autor",
            "Created by", "Devised by", "Designed by", "Author=", "Made by",
            "FM Author", "Mission Author", "Mission author", "Mission Creator", "Mission creator",
            "The author:", "author:",
            // TODO: @TEMP_HACK: See above
            "Fan Mission/Map Author"
        };

        internal static readonly string[] SA_ReleaseDateDetect =
        {
            "Date Of Release", "Date of Release",
            "Date of release", "Release Date", "Release date"
        };

        internal static readonly string[] SA_VersionDetect = { "Version" };

        #endregion

        #endregion

        // Ordered by number of actual total occurrences across all FMs:
        // gif: 153,294
        // pcx: 74,786
        // tga: 12,622
        // dds: 11,647
        // png: 11,290
        // bmp: 657
        internal static readonly string[] ImageFileExtensions = { ".gif", ".pcx", ".tga", ".dds", ".png", ".bmp" };
        internal static readonly string[] ImageFilePatterns = { "*.gif", "*.pcx", "*.tga", "*.dds", "*.png", "*.bmp" };

        internal static readonly string[] MotionFilePatterns = { "*.mc", "*.mi" };
        internal static readonly string[] MotionFileExtensions = { ".mc", ".mi" };

        // .osm for the classic scripts; .nut for Squirrel scripts for NewDark >= 1.25
        internal static readonly string[] ScriptFileExtensions = { ".osm", ".nut" };

        #region Languages

        // NOTE: I think this was for GetLanguages() for the planned accuracy update?
        //internal static string[] LanguageDirs { get; } = { FMDirs.Books, FMDirs.Intrface, FMDirs.Strings };

        // Perf micro-optimization: don't create a new list if we're only returning English
        internal static readonly List<string> EnglishOnly = new List<string> { "english" };

        internal static readonly string[]
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

        internal static readonly string[]
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

        internal static readonly string[] Languages_FS_Lang_Language_FS =
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
        internal static readonly Dictionary<string, string>
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

        internal static readonly string[]
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
    }

    internal static class MisFileStrings
    {
        internal static readonly byte[] OBJ_MAP = Encoding.ASCII.GetBytes("OBJ_MAP");

        internal static readonly byte[] Thief2UniqueString = Encoding.ASCII.GetBytes("RopeyArrow");

        // SS2-only detection string
        internal static readonly byte[] MAPPARAM = Encoding.ASCII.GetBytes("MAPPARAM");
    }

    // Putting regexes in here is a perf optimization: static (initialized only once) and Compiled increases
    // their performance by a huge amount. And as we know, regexes need all the performance help they can get.
    internal static class Regexes
    {
        internal static readonly Regex GLMLTagRegex =
            new Regex(@"\[/?GL[A-Z]+\]", RegexOptions.Compiled);

        internal static readonly Regex OpenParenSpacesRegex =
            new Regex(@"\(\s+", RegexOptions.Compiled);

        internal static readonly Regex CloseParenSpacesRegex =
            new Regex(@"\s+\)", RegexOptions.Compiled);

        internal static readonly Regex DaySuffixesRegex =
            new Regex(@"\d(?<Suffix>(st|nd|rd|th)).+",
                RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.ExplicitCapture);

        internal static readonly Regex VersionExclude1Regex =
            new Regex(@"\d\.\d+\+", RegexOptions.Compiled);

        internal static readonly Regex TitleAnyConsecutiveLettersRegex =
            new Regex(@"\w\w", RegexOptions.Compiled);

        // TODO: [a-z] is only ASCII letters, so it won't catch lowercase other stuff I guess
        internal static readonly Regex TitleContainsLowerCaseCharsRegex =
            new Regex(@"[a-z]", RegexOptions.Compiled);

        internal static readonly Regex AuthorEmailRegex =
            new Regex(@"\(?\S+@\S+\.\S{2,5}\)?", RegexOptions.Compiled);

        // This doesn't need to be a regex really, but it takes like 5.4 microseconds per FM, so, yeah
        internal static readonly Regex NewGameStrTitleRegex =
            new Regex(@"^skip_training\:\s*""(?<Title>.+)""", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        // TODO: This one looks iffy though
        internal static readonly Regex VersionFirstNumberRegex =
            new Regex(@"[0123456789\.]+", RegexOptions.Compiled);

        // Much, much faster to iterate through possible regex matches, common ones first
        // TODO: These are still kinda slow comparatively. Profile to see if any are bottlenecks
        internal static readonly Regex[] NewDarkVersionRegexes =
        {
            new Regex(@"NewDark (?<Version>\d\.\d+)",
                RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.ExplicitCapture),
            new Regex(@"(New ?Dark|""New ?Dark"").? v?(\.| )?(?<Version>\d\.\d+)",
                RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.ExplicitCapture),
            new Regex(@"(New ?Dark|""New ?Dark"").? .?(Version|Patch) .?(?<Version>\d\.\d+)",
                RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.ExplicitCapture),
            new Regex(@"(Dark ?Engine) (Version.?|v)?(\.| )?(?<Version>\d\.\d+)",
                RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.ExplicitCapture),
            new Regex(
                @"((?<!(Love |Being |Penitent |Counter-|Requiem for a |Space ))Thief|(?<!Being )Thief ?(2|II)|The Metal Age) v?(\.| )?(?<Version>\d\.\d+)",
                RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.ExplicitCapture),
            new Regex(
                @"\D(?<Version>\d\.\d+) (version of |.?)New ?Dark(?! ?\d\.\d+)|Thief Gold( Patch)? (?<Version>(?!1\.33|1\.37)\d\.\d+)",
                RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.ExplicitCapture),
            new Regex(@"Version (?<Version>\d\.\d+) of (Thief ?(2|II))",
                RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.ExplicitCapture),
            new Regex(@"(New ?Dark|""New ?Dark"") (is )?required (.? )v?(\.| )?(?<Version>\d\.\d+)",
                RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.ExplicitCapture),
            new Regex(@"(?<Version>(?!1\.3(3|7))\d\.\d+) Patch",
                RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.ExplicitCapture)

            // Original regex for reference - slow!
            // @"((?<Name>(""*New *Dark""*( Version| Patch)*|Dark *Engine|(?<!(Love |Being |Penitent |Counter-|Requiem for a |Space ))Thief|(?<!Being )Thief *2|Thief *II|The Metal Age)) *V?(\.| )*(?<Version>\d\.\d+)|\D(?<Version>\d\.\d+) +(version of |(?!\r\n).?)New *Dark(?! *\d\.\d+)|Thief Gold( Patch)* (?<Version>(?!1\.33|1\.37)\d\.\d+))",
        };

        internal static readonly Regex[] AuthorRegexes =
        {
            new Regex(
                @"(FM|mis(si|is|i)on|campaign|series) for Thief( Gold|: The Dark Project|\s*2(: The Metal Age)?)\s+by\s*(?<Author>.+)",
                RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.ExplicitCapture),
            new Regex(
                @"(A )?Thief( Gold|: The Dark Project|\s*2(: The Metal Age)?) (fan(-| ?)mis((si|is|i)on)|FM|campaign)\s+by (?<Author>.+)",
                RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.ExplicitCapture),
            new Regex(
                @"A(n)? (fan(-| ?)mis((si|is|i)on)|FM|campaign)\s+(made\s+)?by\s+(?<Author>.+)",
                RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.ExplicitCapture),
            new Regex(
                @"A(n)? .+(-| )part\s+Thief( Gold |: The Dark Project |\s*2(: The Metal Age )?)\s+(fan(-| ?)mis((si|is|i)on)|FM|campaign)\s+((made\s+by)|by|from)\s+(?<Author>.+)",
                RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.ExplicitCapture)
        };

        private const string CopyrightSecondPart =
            //language=regexp
            @"(?<Months>( (Jan|Febr)uary| Ma(rch|y)| A(pril|ugust)| Ju(ne|ly)| (((Sept|Nov|Dec)em)|Octo)ber))?" +
            //language=regexp
            @"(?(Months)(, ?| ))\d*( by| to)? (?<Author>.+)";

        // Unicode 00A9 = copyright symbol

        internal static readonly Regex[] AuthorMissionCopyrightRegexes =
        {
            new Regex(
                //language=regexp
                @"^This (level|(fan(-| |))?mis(si|is|i)on|FM) is( made)? (\(c\)|\u00A9) ?" + CopyrightSecondPart,
                RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.ExplicitCapture),
            new Regex(
                //language=regexp
                @"^The (levels?|(fan(-| |))?mis(si|is|i)ons?|FMs?)( in this (zip|archive( file)?))? (is|are)( made)? (\(c\)|\u00A9) ?" + CopyrightSecondPart,
                RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.ExplicitCapture),
            new Regex(
                //language=regexp
                @"^These (levels|(fan(-| |))?mis(si|is|i)ons|FMs) are( made)? (\(c\)|\u00A9) ?" + CopyrightSecondPart,
                RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.ExplicitCapture)
        };

        // This one is only to be used if we know the above line says "Copyright" or something, because it has
        // an @ as an option for a copyright symbol (used by some Theker missions) and we want to be sure it
        // means what we think it means.
        internal static readonly Regex AuthorGeneralCopyrightIncludeAtSymbolRegex =
            new Regex(
                //language=regexp
                @"^(Copyright )?(\(c\)|\u00A9|@) ?" + CopyrightSecondPart,
                RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.ExplicitCapture);

        internal static readonly Regex AuthorGeneralCopyrightRegex =
            new Regex(
                //language=regexp
                @"^(Copyright )?(\(c\)|\u00A9) ?" + CopyrightSecondPart,
                RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.ExplicitCapture);

        internal static readonly Regex CopyrightAuthorYearRegex = new Regex(@" \d+.*$", RegexOptions.Compiled);

        internal static readonly Regex TitleByAuthorRegex =
            new Regex(@"(\s+|\s*(:|-|\u2013|,)\s*)by(\s+|\s*(:|-|\u2013)\s*)(?<Author>.+)",
                RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.ExplicitCapture);
    }

    /// <summary>
    /// Specialized (therefore fast) sort for titles.str lines only. Anything else is likely to throw an
    /// IndexOutOfRangeException.
    /// </summary>
    internal sealed class TitlesStrNaturalNumericSort : IComparer<string>
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
