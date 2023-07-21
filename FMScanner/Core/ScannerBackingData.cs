// Uncomment this define in all files it appears in to get all features (we use it for testing)
//#define FMScanner_FullCode

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using AL_Common;
using static AL_Common.Common;

namespace FMScanner;

public sealed partial class Scanner
{
    private readonly byte[] _rtfHeaderBuffer = new byte[RTFHeaderBytes.Length];

    private readonly byte[] _misChunkHeaderBuffer = new byte[12];

    private readonly BinaryBuffer _binaryReadBuffer = new();

    private const char LeftDoubleQuote = '\u201C';
    private const char RightDoubleQuote = '\u201D';

    [SuppressMessage("ReSharper", "IdentifierTypo")]
    private static class FMDirs
    {
        // PERF: const string concatenation is free (const concats are done at compile time), so do it to lessen
        // the chance of error.

        // We only need BooksS
        internal const string Fam = "fam";
        // We only need IntrfaceS
        internal const string Mesh = "mesh";
        internal const string Motions = "motions";
        internal const string Movies = "movies";
        internal const string Cutscenes = "cutscenes"; // SS2 only
        internal const string Obj = "obj";
        internal const string Scripts = "scripts";
        internal const string Snd = "snd";
        internal const string Snd2 = "snd2"; // SS2 only
        // We only need StringsS
        internal const string Subtitles = "subtitles";

        internal const string BooksS = "books/";
        internal const string FamS = Fam + "/";
        internal const string IntrfaceS = "intrface/";
        internal const int IntrfaceSLen = 9; // workaround for .NET 4.7.2 not inlining const string lengths
        internal const string MeshS = Mesh + "/";
        internal const string MotionsS = Motions + "/";
        internal const string MoviesS = Movies + "/";
        internal const string CutscenesS = Cutscenes + "/"; // SS2 only
        internal const string ObjS = Obj + "/";
        internal const string ScriptsS = Scripts + "/";
        internal const string SndS = Snd + "/";
        internal const string Snd2S = Snd2 + "/"; // SS2 only
        internal const string StringsS = "strings/";
        internal const string SubtitlesS = Subtitles + "/";

        internal const string T3FMExtras1S = "Fan Mission Extras/";
        internal const string T3FMExtras2S = "FanMissionExtras/";

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

        internal const string IntrfaceEnglishNewGameStr = "intrface/english/newgame.str";
        internal const string IntrfaceNewGameStr = "intrface/newgame.str";
        internal const string SNewGameStr = "/newgame.str";

        internal const string StringsMissFlag = "strings/missflag.str";
        internal const string StringsEnglishMissFlag = "strings/english/missflag.str";
        internal const string SMissFlag = "/missflag.str";

        // Telliamed's fminfo.xml file, used in a grand total of three missions
        internal const string FMInfoXml = "fminfo.xml";

        // fm.ini, a NewDark (or just FMSel?) file
        internal const string FMIni = "fm.ini";

        // System Shock 2 file
        internal const string ModIni = "mod.ini";

        // For Thief 3 missions, all of them have this file, and then any other .gmp files are the actual missions
        internal const string EntryGmp = "Entry.gmp";
    }

    #region Non-const FM Files

    private readonly string[] FMFiles_TitlesStrLocations = new string[24];

    // Used for SS2 fingerprinting for the game type scan fallback
    [SuppressMessage("ReSharper", "StringLiteralTypo")]
    private readonly HashSetI FMFiles_SS2MisFiles = new(23)
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

    #region Preallocated arrays

    // Perf, for passing to params[]-taking methods so we don't allocate all the time
    private readonly char[] CA_Period = { '.' };
    private readonly char[] CA_Asterisk = { '*' };
    private readonly char[] CA_Hyphen = { '-' };
    private readonly char[] CA_DoubleQuote = { '\"' };
    private readonly char[] CA_UnicodeQuotes = { LeftDoubleQuote, RightDoubleQuote };
    private readonly char[] CA_DateSeparators = { ' ', '-', '/' };
    private readonly char[] CA_Parens = { '(', ')' };
    private readonly string[] CRLF_CR_LF = { "\r\n", "\r", "\n" };
    private readonly string[] SA_DoubleSpaces = { "  " };
    private readonly string[] SA_T3DetectExtensions = { "*.ibt", "*.cbt", "*.gmp", "*.ned", "*.unr" };
    private readonly string[] SA_AllFiles = { "*" };
    private readonly string[] SA_AllBinFiles = { "*.bin" };
    private readonly string[] SA_AllSubFiles = { "*.sub" };

    #region Field detect strings

    // IMPORTANT(Field detect strings): Always use lowercase for letters where you want case insensitivity!
    // This gets matched with "given case or upper case" to prevent false positives from lowercase first letters.

    private readonly string[] SA_TitleDetect =
    {
        "Title of the mission",
        "Title",
        "Mission title",
        "Mission name",
        "Level name",
        "Mission:",
        "Mission ",
        "Campaign title",
        "The name of Mission:",
        // TODO: @TEMP_HACK: This works for the one mission that has it in this casing
        // Rewrite this code in here so we can have more detailed detection options than just
        // these silly strings and the default case check
        "Fan Mission/Map Name"
    };

    private readonly string[] SA_AuthorDetect =
    {
        "Author",
        "Authors",
        "Autor",
        "Created by",
        "Devised by",
        "Designed by",
        "Author=",
        "Made by",
        "FM Author",
        "Mission author",
        "Mission creator",
        "The author:",
        "author:",
        // TODO: @TEMP_HACK: See above
        "Fan Mission/Map Author"
    };

    private readonly string[] SA_LatestUpdateDateDetect =
    {
        "Update date",
        "Updated date",
        "Update",
        "Updated",
        "Last updated",
        "Last update",

        "Latest update",
        "Date of update",
        "Version date",
        "Rereleased",
        "Re-release",
        "Re-released",
        "Date of rerelease",
        "Date of re-release",
        "Date rereleased",
        "Date re-released",
        "Revision date",
        "Release of latest revision",
        "Release date of latest revision"
    };

    private readonly string[] SA_ReleaseDateDetect =
    {
        "Date of release",
        "Release date",

        "Date of completion",
        "Date finished",
        "Released",
        "Originally released",
        "Date:",
        "Original date of release",
        "Official release date",
        "Released on",
        "Release",
        "Releasedate",
        "Date released",
        "Finished",
        "Completion date",
        // "Date of Release"
        "DOR:",
        "First release",

        #region French

        // "Release date"
        "Date de sortie",
        // "Launch date"
        "Date de lancement",
        // "Date of completion"
        "Date de réalisation",
        "Date de realisation",
        // "Release date"
        "Date de parution",

        #endregion

        #region German

        // "Publication date"
        "Erscheinungsdatum",
        "Datum der erscheinung",
        // "Release date"
        "Releasedatum",
        // "Issue date"
        "Ausgabedatum",
        // "Released on"
        "Erschienen am"

        #endregion
    };

#if FMScanner_FullCode
    private readonly string[] SA_VersionDetect = { "Version" };
#endif

    #endregion

    #endregion

    #region Extension and file pattern arrays

    /*
    Ordered by number of actual total occurrences across all FMs (in 1098 set):
    gif: 153,294
    pcx: 74,786
    tga: 12,622
    dds: 11,647
    png: 11,290
    bmp: 657
    */
    private readonly string[] ImageFileExtensions = { ".gif", ".pcx", ".tga", ".dds", ".png", ".bmp" };
    private readonly string[] ImageFilePatterns = { "*.gif", "*.pcx", "*.tga", "*.dds", "*.png", "*.bmp" };

    private readonly string[] MotionFilePatterns = { "*.mc", "*.mi" };
    private readonly string[] MotionFileExtensions = { ".mc", ".mi" };

    // .osm for the classic scripts; .nut for Squirrel scripts for NewDark >= 1.25
    private readonly string[] ScriptFileExtensions = { ".osm", ".nut" };

    #endregion

    #region Languages

    private readonly string[] Languages_FS_Lang_FS;
    private readonly string[] Languages_FS_Lang_Language_FS;
    private readonly string[] LanguagesC;

    #endregion

    #region Dates

    private readonly string[]
    _dateFormatsEuropean =
    {
        "d.M.yyyy",
        "dd.M.yyyy",

        "d.MM.yyyy",
        "dd.MM.yyyy"
    };

    /*
    Fields we use in here:
    d - The day of the month, from 1 through 31.
    dd - The day of the month, from 01 through 31.
    M - The month, from 1 through 12.
    MM - The month, from 01 through 12.
    MMM - abbreviated name (ie. Sep)
    MMMM - full name (ie. September)
    y - The year, from 0 to 99.
    yy - The year, from 00 to 99.
    yyyy - The year as a four-digit number.
    */
    private readonly (string Format, bool CanBeAmbiguous)[]
    _dateFormats =
    {
        ("MMM d yy", false),
        ("MMM dd yy", false),

        ("MMM d yyyy", false),
        ("MMM dd yyyy", false),

        ("MMMM d yy", false),
        ("MMMM dd yy", false),

        ("MMMM d yyyy", false),
        ("MMMM dd yyyy", false),

        ("d MMM yy", true),
        ("dd MMM yy", true),

        ("d MMM yyyy", false),
        ("dd MMM yyyy", false),

        ("d MMMM yy", true),
        ("dd MMMM yy", true),
        ("d MMMM yyyy", false),
        ("dd MMMM yyyy", false),

        ("yyyy M d", true),
        ("yyyy M dd", true),
        ("yyyy MM d", true),
        ("yyyy MM dd", true),
        ("yyyy d M", true),
        ("yyyy dd M", true),
        ("yyyy d MM", true),
        ("yyyy dd MM", true),

        ("yyyy MMM d", false),
        ("yyyy MMM dd", false),
        ("yyyy MMMM d", false),
        ("yyyy MMMM dd", false),

        ("MM dd yyyy", true),
        ("dd MM yyyy", true),
        ("MM dd yy", true),
        ("dd MM yy", true),

        ("M d yyyy", true),
        ("d M yyyy", true),
        ("M d yy", true),
        ("d M yy", true)
    };

    private readonly string[]
    _monthNamesEnglish =
    {
        // January and February are matched by German "Januar" / "Februar"
        "March",
        "April",
        "May",
        "June",
        "July",
        "August",
        "September",
        "October",
        "November",
        "December",

        "Jan",
        "Feb",
        "Febr",
        "Mar",
        "Apr",
        // "May" left out because it's already three letters and thus already exists in the full name set
        "Jun",
        "Jul",
        "Aug",
        "Sep",
        "Sept",
        "Oct",
        "Nov",
        "Dec",

        "Feburary",
        "Martch",
        "Jully",

        // French
        "janvier",
        "février",
        "fevrier",
        "mars",
        "avril",
        "mai",
        "juin",
        "juillet",
        "juiller",
        "août",
        "aout",
        "septembre",
        "octobre",
        "novembre",
        "décembre",
        "decembre",

        // German
        "Januar",
        "Februar",
        "März",
        "Marz",
        "Juni",
        "Juli",
        "Oktober",
        "Okt",
        "Dezember"
    };

    #endregion

    #region Game detection

    // ReSharper disable IdentifierTypo

    private readonly byte[] OBJ_MAP =
    {
        (byte)'O',
        (byte)'B',
        (byte)'J',
        (byte)'_',
        (byte)'M',
        (byte)'A',
        (byte)'P'
    };

    /*
    In theory, someone could make a Thief 1 mission with a RopeyArrow archetype. It's never happened and is
    vanishingly unlikely to happen, but hey. Before RopeyArrow, there will be an int32 id number followed by
    the bytes 0B 00 00 00 (for .mis files) or 0F 00 00 00 0B 00 00 00 (for .gam files). 99% of the time this
    id number is D7 F3 FF FF (presumably the T2 default), so we could almost prepend this to our string for
    extra strength defense against a custom RopeyArrow archetype... except that a handful of legit T2 missions
    have different ids. So unfortunately if we want to stay accurate we have to stay with just "RopeyArrow".
    */
    private readonly byte[] RopeyArrow =
    {
        (byte)'R',
        (byte)'o',
        (byte)'p',
        (byte)'e',
        (byte)'y',
        (byte)'A',
        (byte)'r',
        (byte)'r',
        (byte)'o',
        (byte)'w'
    };

    private const int _gameTypeBufferSize = 81_920;

    private byte[]? _gameTypeBuffer_ChunkPlusRopeyArrow;
    private byte[] GameTypeBuffer_ChunkPlusRopeyArrow => _gameTypeBuffer_ChunkPlusRopeyArrow ??= new byte[_gameTypeBufferSize + RopeyArrow.Length];

    private byte[]? _gameTypeBuffer_ChunkPlusMAPPARAM;
    private byte[] GameTypeBuffer_ChunkPlusMAPPARAM => _gameTypeBuffer_ChunkPlusMAPPARAM ??= new byte[_gameTypeBufferSize + MAPPARAM.Length];

    private const int _ss2MapParamNewDarkLoc = 696;
    private const int _oldDarkT2Loc = 772;
    private const int _ss2MapParamOldDarkLoc = 916;
    // Neither of these clash with SS2's SKYOBJVAR locations (3168, 7292).
    private const int _newDarkLoc1 = 7217;
    private const int _newDarkLoc2 = 3093;

    private readonly int[] _locations = { _ss2MapParamNewDarkLoc, _oldDarkT2Loc, _ss2MapParamOldDarkLoc, _newDarkLoc1, _newDarkLoc2 };

    private const int _ss2NewDarkOffset = 705; // 696+9 = 705
    private const int _t2OldDarkOffset = 76;   // (772+9)-705 = 76
    private const int _ss2OldDarkOffset = 144; // ((916+9)-76)-705 = 144
    private const int _newDarkOffset1 = 2177;  // (((3093+9)-144)-76)-705 = 2177
    private const int _newDarkOffset2 = 4124;  // ((((7217+9)-2177)-144)-76)-705 = 4124

    private readonly int[] _zipOffsets = { _ss2NewDarkOffset, _t2OldDarkOffset, _ss2OldDarkOffset, _newDarkOffset1, _newDarkOffset2 };

    private readonly byte[][] _zipOffsetBuffers =
    {
        new byte[_ss2NewDarkOffset],
        new byte[_t2OldDarkOffset],
        new byte[_ss2OldDarkOffset],
        new byte[_newDarkOffset1],
        new byte[_newDarkOffset2]
    };

    // MAPPARAM is 8 bytes, so for that we just check the first 8 bytes and ignore the last, rather than
    // complicating things any further than they already are.
    private const int _gameDetectStringBufferLength = 9;
    private readonly byte[] _gameDetectStringBuffer = new byte[_gameDetectStringBufferLength];

    // ReSharper restore IdentifierTypo

    #endregion

    #region Regexes

    // PERF: Making regexes compiled increases their performance by a huge amount.
    // And as we know, regexes need all the performance help they can get.
    private readonly Regex AThief3Mission =
        new Regex(@"^A\s+Thief(\s+|\s+:\s+|\s+-\s+)Deadly",
            RegexOptions.ExplicitCapture | IgnoreCaseInvariant | RegexOptions.Compiled);

    private readonly Regex OpenParenSpacesRegex =
        new Regex(@"\(\s+", RegexOptions.Compiled);

    private readonly Regex CloseParenSpacesRegex =
        new Regex(@"\s+\)", RegexOptions.Compiled);

    private readonly Regex DaySuffixesRegex =
        new Regex(@"\d(?<Suffix>(st|nd|rd|th)).+",
            RegexOptions.ExplicitCapture | IgnoreCaseInvariant | RegexOptions.Compiled);

    private readonly Regex AuthorEmailRegex =
        new Regex(@"\(?\S+@\S+\.\S{2,5}\)?", RegexOptions.Compiled);

#if FMScanner_FullCode
    private readonly Regex VersionExclude1Regex =
        new Regex(@"\d\.\d+\+", RegexOptions.Compiled);

    // TODO: This one looks iffy though
    private readonly Regex VersionFirstNumberRegex =
        new Regex(@"[0123456789\.]+", RegexOptions.Compiled);

    // Much, much faster to iterate through possible regex matches, common ones first
    // TODO: These are still kinda slow comparatively. Profile to see if any are bottlenecks
    private readonly Regex[] NewDarkVersionRegexes =
    {
        new Regex(@"NewDark (?<Version>\d\.\d+)",
            IgnoreCaseInvariant | RegexOptions.Compiled |
            RegexOptions.ExplicitCapture),
        new Regex(@"(New ?Dark|""New ?Dark"").? v?(\.| )?(?<Version>\d\.\d+)",
            IgnoreCaseInvariant | RegexOptions.Compiled |
            RegexOptions.ExplicitCapture),
        new Regex(@"(New ?Dark|""New ?Dark"").? .?(Version|Patch) .?(?<Version>\d\.\d+)",
            IgnoreCaseInvariant | RegexOptions.Compiled |
            RegexOptions.ExplicitCapture),
        new Regex(@"(Dark ?Engine) (Version.?|v)?(\.| )?(?<Version>\d\.\d+)",
            IgnoreCaseInvariant | RegexOptions.Compiled |
            RegexOptions.ExplicitCapture),
        new Regex(
            @"((?<!(Love |Being |Penitent |Counter-|Requiem for a |Space ))Thief|(?<!Being )Thief ?(2|II)|The Metal Age) v?(\.| )?(?<Version>\d\.\d+)",
            IgnoreCaseInvariant | RegexOptions.Compiled |
            RegexOptions.ExplicitCapture),
        new Regex(
            @"\D(?<Version>\d\.\d+) (version of |.?)New ?Dark(?! ?\d\.\d+)|Thief Gold( Patch)? (?<Version>(?!1\.33|1\.37)\d\.\d+)",
            IgnoreCaseInvariant | RegexOptions.Compiled |
            RegexOptions.ExplicitCapture),
        new Regex(@"Version (?<Version>\d\.\d+) of (Thief ?(2|II))",
            IgnoreCaseInvariant | RegexOptions.Compiled |
            RegexOptions.ExplicitCapture),
        new Regex(@"(New ?Dark|""New ?Dark"") (is )?required (.? )v?(\.| )?(?<Version>\d\.\d+)",
            IgnoreCaseInvariant | RegexOptions.Compiled |
            RegexOptions.ExplicitCapture),
        new Regex(@"(?<Version>(?!1\.3(3|7))\d\.\d+) Patch",
            IgnoreCaseInvariant | RegexOptions.Compiled |
            RegexOptions.ExplicitCapture)
    };
#endif

    private readonly Regex[] AuthorRegexes =
    {
        new Regex(
            @"(FM|mis(si|is|i)on|campaign|series) for Thief( Gold|: The Dark Project|\s*2(: The Metal Age)?)\s+by\s*(?<Author>.+)",
            IgnoreCaseInvariant | RegexOptions.Compiled |
            RegexOptions.ExplicitCapture),
        new Regex(
            @"(A )?Thief( Gold|: The Dark Project|\s*2(: The Metal Age)?) (fan(-| ?)mis((si|is|i)on)|FM|campaign)\s+by (?<Author>.+)",
            IgnoreCaseInvariant | RegexOptions.Compiled |
            RegexOptions.ExplicitCapture),
        new Regex(
            @"A(n)? (fan(-| ?)mis((si|is|i)on)|FM|campaign)\s+(made\s+)?by\s+(?<Author>.+)",
            IgnoreCaseInvariant | RegexOptions.Compiled |
            RegexOptions.ExplicitCapture),
        new Regex(
            @"A(n)? .+(-| )part\s+Thief( Gold |: The Dark Project |\s*2(: The Metal Age )?)\s+(fan(-| ?)mis((si|is|i)on)|FM|campaign)\s+((made\s+by)|by|from)\s+(?<Author>.+)",
            IgnoreCaseInvariant | RegexOptions.Compiled |
            RegexOptions.ExplicitCapture)
    };

    private const string _copyrightSecondPart =
        //language=regexp
        "(?<Months>( (Jan|Febr)uary| Ma(rch|y)| A(pril|ugust)| Ju(ne|ly)| (((Sept|Nov|Dec)em)|Octo)ber))?" +
        //language=regexp
        @"(?(Months)(, ?| ))\d*( by| to)? (?<Author>.+)";

    // Unicode 00A9 = copyright symbol

    private readonly Regex[] AuthorMissionCopyrightRegexes =
    {
        new Regex(
            //language=regexp
            @"^This (level|(fan(-| |))?mis(si|is|i)on|FM) is( made)? (\(c\)|\u00A9) ?" + _copyrightSecondPart,
            IgnoreCaseInvariant | RegexOptions.Compiled |
            RegexOptions.ExplicitCapture),
        new Regex(
            //language=regexp
            @"^The (levels?|(fan(-| |))?mis(si|is|i)ons?|FMs?)( in this (zip|archive( file)?))? (is|are)( made)? (\(c\)|\u00A9) ?" +
            _copyrightSecondPart,
            IgnoreCaseInvariant | RegexOptions.Compiled |
            RegexOptions.ExplicitCapture),
        new Regex(
            //language=regexp
            @"^These (levels|(fan(-| |))?mis(si|is|i)ons|FMs) are( made)? (\(c\)|\u00A9) ?" +
            _copyrightSecondPart,
            IgnoreCaseInvariant | RegexOptions.Compiled |
            RegexOptions.ExplicitCapture)
    };

    // This one is only to be used if we know the above line says "Copyright" or something, because it has
    // an @ as an option for a copyright symbol (used by some Theker missions) and we want to be sure it
    // means what we think it means.
    private readonly Regex AuthorGeneralCopyrightIncludeAtSymbolRegex =
        new Regex(
            //language=regexp
            @"^(Copyright )?(\(c\)|\u00A9|@) ?" + _copyrightSecondPart,
            IgnoreCaseInvariant | RegexOptions.Compiled | RegexOptions.ExplicitCapture);

    private readonly Regex AuthorGeneralCopyrightRegex =
        new Regex(
            //language=regexp
            @"^(Copyright )?(\(c\)|\u00A9) ?" + _copyrightSecondPart,
            IgnoreCaseInvariant | RegexOptions.Compiled | RegexOptions.ExplicitCapture);

    private readonly Regex CopyrightAuthorYearRegex = new Regex(@" \d+.*$", RegexOptions.Compiled);

    private readonly Regex TitleByAuthorRegex =
        new Regex(@"(\s+|\s*(:|-|\u2013|,)\s*)by(\s+|\s*(:|-|\u2013)\s*)(?<Author>.+)",
            IgnoreCaseInvariant | RegexOptions.Compiled | RegexOptions.ExplicitCapture);

    #region Release date detection

    private readonly Regex MultipleColonsRegex =
        new Regex(@"(:\s*)+",
            RegexOptions.Compiled | RegexOptions.ExplicitCapture);

    private readonly Regex MultipleDashesRegex =
        new Regex("-{2,}",
            RegexOptions.Compiled | RegexOptions.ExplicitCapture);

    private readonly Regex MultipleUnicodeDashesRegex =
        new Regex(@"\u2013{2,}",
            RegexOptions.Compiled | RegexOptions.ExplicitCapture);

    private readonly Regex AnyDateNumberRTLRegex =
        new Regex("(Y2K|[0123456789])",
            IgnoreCaseInvariant | RegexOptions.Compiled | RegexOptions.ExplicitCapture | RegexOptions.RightToLeft);

    private readonly Regex NewDarkAndNumberRegex =
        new Regex(@"New ?Dark [0123456789]\.[0123456789]{1,2}",
            IgnoreCaseInvariant | RegexOptions.Compiled | RegexOptions.ExplicitCapture);

    private readonly Regex EuropeanDateRegex =
        new Regex(@"\.*[0123456789]{1,2}\s*\.\s*[0123456789]{1,2}\s*\.\s*([0123456789]{4}|[0123456789]{2})\.*",
            RegexOptions.Compiled | RegexOptions.ExplicitCapture);

    private readonly Regex PeriodWithOptionalSurroundingSpacesRegex =
        new Regex(@"\s*\.\s*",
            RegexOptions.Compiled | RegexOptions.ExplicitCapture);

    private readonly Regex DateSeparatorsRegex =
        // Tilde: Auldale Chess Tournament saying "March ~8, 2006"
        new Regex(@"\s*(,|~|-|/|\.)\s*",
            RegexOptions.Compiled | RegexOptions.ExplicitCapture);

    private readonly Regex DateOfSeparatorRegex =
        new Regex(@"\s*of\s*",
            IgnoreCaseInvariant | RegexOptions.Compiled);

    private readonly Regex OneOrMoreWhiteSpaceCharsRegex =
        new Regex(@"\s+", RegexOptions.Compiled);

    private readonly Regex FebrRegex =
        new Regex("Febr ",
            IgnoreCaseInvariant | RegexOptions.Compiled);

    private readonly Regex SeptRegex =
        new Regex("Sept ",
            IgnoreCaseInvariant | RegexOptions.Compiled);

    private readonly Regex OktRegex =
        new Regex("Okt ",
            IgnoreCaseInvariant | RegexOptions.Compiled);

    private readonly Regex Y2KRegex =
        new Regex("Y2K",
            IgnoreCaseInvariant | RegexOptions.Compiled);

    private readonly Regex JanuaryVariationsRegex =
        new Regex("Jan(vier|uar )",
            IgnoreCaseInvariant | RegexOptions.Compiled | RegexOptions.ExplicitCapture);

    private readonly Regex FebruaryVariationsRegex =
        new Regex("F(eburar(y| )|(é|e)vrier)",
            IgnoreCaseInvariant | RegexOptions.Compiled | RegexOptions.ExplicitCapture);

    private readonly Regex MarchVariationsRegex =
        new Regex("M(ar(tch|s|z)|ärz)",
            IgnoreCaseInvariant | RegexOptions.Compiled | RegexOptions.ExplicitCapture);

    private readonly Regex AprilVariationsRegex =
        new Regex("avril",
            IgnoreCaseInvariant | RegexOptions.Compiled);

    private readonly Regex MayVariationsRegex =
        new Regex("mai",
            IgnoreCaseInvariant | RegexOptions.Compiled);

    private readonly Regex JuneVariationsRegex =
        new Regex("Ju(in|ni)",
            IgnoreCaseInvariant | RegexOptions.Compiled | RegexOptions.ExplicitCapture);

    private readonly Regex JulyVariationsRegex =
        new Regex("Ju(l(ly|i)|ille(t|r))",
            IgnoreCaseInvariant | RegexOptions.Compiled | RegexOptions.ExplicitCapture);

    private readonly Regex AugustVariationsRegex =
        new Regex("ao(u|û)t",
            IgnoreCaseInvariant | RegexOptions.Compiled | RegexOptions.ExplicitCapture);

    private readonly Regex SeptemberVariationsRegex =
        new Regex("septembre",
            IgnoreCaseInvariant | RegexOptions.Compiled);

    private readonly Regex OctoberVariationsRegex =
        new Regex("O(ctobre|ktober)",
            IgnoreCaseInvariant | RegexOptions.Compiled | RegexOptions.ExplicitCapture);

    private readonly Regex HalloweenRegex =
        new Regex("Halloween",
            IgnoreCaseInvariant | RegexOptions.Compiled | RegexOptions.ExplicitCapture);

    private readonly Regex ChristmasRegex =
        new Regex("Christmas",
            IgnoreCaseInvariant | RegexOptions.Compiled | RegexOptions.ExplicitCapture);

    private readonly Regex NovemberVariationsRegex =
        new Regex("novembre",
            IgnoreCaseInvariant | RegexOptions.Compiled | RegexOptions.ExplicitCapture);

    private readonly Regex DecemberVariationsRegex =
        new Regex("D((é|e)cembre|ezember)",
            IgnoreCaseInvariant | RegexOptions.Compiled | RegexOptions.ExplicitCapture);

    #endregion

    private readonly Regex MultipleWhiteSpaceRegex =
        new Regex(@"\s{2,}", RegexOptions.Compiled);

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

            // int32 max digits minus 1 (to avoid having to check for overflow)
            const int maxDigits = 9;

            int xIndex1 = x.IndexOf('_');
            int xIndex2 = x.IndexOf(':', xIndex1);

            int xNum = 0;
            int xEnd = Math.Min(xIndex2, xIndex1 + maxDigits);
            for (int i = xIndex1 + 1; i < xEnd; i++)
            {
                char c = x[i];
                if (c.IsAsciiNumeric())
                {
                    xNum *= 10;
                    xNum += c - '0';
                }
                else
                {
                    return 0;
                }
            }

            int yIndex1 = y.IndexOf('_');
            int yIndex2 = y.IndexOf(':', yIndex1);

            int yNum = 0;
            int yEnd = Math.Min(yIndex2, yIndex1 + maxDigits);
            for (int i = yIndex1 + 1; i < yEnd; i++)
            {
                char c = y[i];
                if (c.IsAsciiNumeric())
                {
                    yNum *= 10;
                    yNum += c - '0';
                }
                else
                {
                    return 0;
                }
            }

            return xNum - yNum;
        }
    }
}
