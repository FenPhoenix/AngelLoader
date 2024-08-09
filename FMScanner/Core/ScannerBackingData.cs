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
    private readonly byte[] _misChunkHeaderBuffer = new byte[12];

    private ListFast<char>? _utf32CharBuffer;
    private ListFast<char> Utf32CharBuffer => _utf32CharBuffer ??= new ListFast<char>(2);

    private readonly BinaryBuffer _binaryReadBuffer = new();

    private const char LeftDoubleQuote = '\u201C';
    private const char RightDoubleQuote = '\u201D';

    private readonly struct AsciiCharWithNonAsciiEquivalent(char original, char ascii)
    {
        internal readonly char Original = original;
        internal readonly char Ascii = ascii;
    }

    private readonly AsciiCharWithNonAsciiEquivalent[] _nonAsciiCharsWithAsciiEquivalents =
    {
        new('\x2003', ' '),
        new('\x2002', ' '),
        new('\x2005', ' '),
        new('\xA0', ' '),
        new('\x2014', '-'),
        new('\x2013', '-'),
        new('\x2018', '\''),
        new('\x2019', '\''),
        new('\x201C', '"'),
        new('\x201D', '"'),
    };

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

        internal const string TDM_DarkModTxt = "darkmod.txt";
        internal const string TDM_ReadmeTxt = "readme.txt";
        internal const string TDM_MapSequence = "tdm_mapsequence.txt";
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
        "station.mis",
    };

    #endregion

    #region Preallocated arrays

    // Perf, for passing to params[]-taking methods so we don't allocate all the time
    private readonly char[] CA_AsteriskHyphen = { '*', '-' };
    private readonly char[] CA_UnicodeQuotes = { LeftDoubleQuote, RightDoubleQuote };
    private readonly char[] CA_DateSeparators = { ' ', '-', '/' };
    private readonly char[] CA_Parens = { '(', ')' };
    private readonly string[] SA_Linebreaks = { "\r\n", "\r", "\n" };
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
        "Fan Mission/Map Name",
        // @Scanner: We need more robust language heuristics / readme filename lang detection to use these
#if false
        "Titre Mission",
        "Titre de la mission",
        "Titre",
#endif
    };

    private readonly string[] SA_AuthorDetect =
    {
        "Author",
        "Authors",
        "Autor",
        // @Scanner: We need more robust language heuristics / readme filename lang detection to use these
#if false
        "Auteur",
        "Auteur de la mission",
#endif
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
        "Fan Mission/Map Author",
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
        "Release date of latest revision",
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
        "Erschienen am",

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
        "dd.MM.yyyy",
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
        ("d M yy", true),
    };

    private readonly string[]
    _monthNames =
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
        // "May" left out because its 3-letter name is also its full name and thus it already occurs above
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
        "Dezember",
    };

    #endregion

    #region Game detection

    // ReSharper disable IdentifierTypo

    private readonly byte[] OBJ_MAP = "OBJ_MAP"u8.ToArray();

    /*
    In theory, someone could make a Thief 1 mission with a RopeyArrow archetype. It's never happened and is
    vanishingly unlikely to happen, but hey. Before RopeyArrow, there will be an int32 id number followed by
    the bytes 0B 00 00 00 (for .mis files) or 0F 00 00 00 0B 00 00 00 (for .gam files). 99% of the time this
    id number is D7 F3 FF FF (presumably the T2 default), so we could almost prepend this to our string for
    extra strength defense against a custom RopeyArrow archetype... except that a handful of legit T2 missions
    have different ids. So unfortunately if we want to stay accurate we have to stay with just "RopeyArrow".
    */
    private readonly byte[] RopeyArrow = "RopeyArrow"u8.ToArray();

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
        new byte[_newDarkOffset2],
    };

    // MAPPARAM is 8 bytes, so for that we just check the first 8 bytes and ignore the last, rather than
    // complicating things any further than they already are.
    private const int _gameDetectStringBufferLength = 9;
    private readonly byte[] _gameDetectStringBuffer = new byte[_gameDetectStringBufferLength];

    // ReSharper restore IdentifierTypo

    #endregion

    // @NET5(Regexes): Having these be generated bloats us up by 70K, but we get a sizable perf increase.
    // GetAuthor() is like 4x faster, GetReleaseDate() is 2x.
    #region Regexes

    [GeneratedRegex("^A Thief( 1| 2| Gold)? (fan|campaign)", RegexOptions.ExplicitCapture | IgnoreCaseInvariant)]
    private static partial Regex AThiefMissionRegex();

    [GeneratedRegex(@"^A\s+Thief(\s+|\s+:\s+|\s+-\s+)Deadly", RegexOptions.ExplicitCapture | IgnoreCaseInvariant)]
    private static partial Regex AThief3MissionRegex();

    [GeneratedRegex(@"\(\s+")]
    private static partial Regex OpenParenSpacesRegex();

    [GeneratedRegex(@"\s+\)")]
    private static partial Regex CloseParenSpacesRegex();

    [GeneratedRegex("[0-9](?<Suffix>(st|nd|rd|th)).+", RegexOptions.ExplicitCapture | IgnoreCaseInvariant)]
    private static partial Regex DaySuffixesRegex();

    [GeneratedRegex(@"\(?\S+@\S+\.\S{2,5}\)?")]
    private static partial Regex AuthorEmailRegex();

#if FMScanner_FullCode
    [GeneratedRegex(@"\d\.\d+\+")]
    private static partial Regex VersionExclude1Regex();

    // TODO: This one looks iffy though
    [GeneratedRegex(@"[0123456789\.]+")]
    private static partial Regex VersionFirstNumberRegex();

    [GeneratedRegex(@"NewDark (?<Version>\d\.\d+)", IgnoreCaseInvariant | RegexOptions.ExplicitCapture)]
    private static partial Regex NewDarkVersion1();

    [GeneratedRegex(@"(New ?Dark|""New ?Dark"").? v?(\.| )?(?<Version>\d\.\d+)", IgnoreCaseInvariant | RegexOptions.ExplicitCapture)]
    private static partial Regex NewDarkVersion2();

    [GeneratedRegex(@"(New ?Dark|""New ?Dark"").? .?(Version|Patch) .?(?<Version>\d\.\d+)", IgnoreCaseInvariant | RegexOptions.ExplicitCapture)]
    private static partial Regex NewDarkVersion3();

    [GeneratedRegex(@"(Dark ?Engine) (Version.?|v)?(\.| )?(?<Version>\d\.\d+)", IgnoreCaseInvariant | RegexOptions.ExplicitCapture)]
    private static partial Regex NewDarkVersion4();

    [GeneratedRegex(@"((?<!(Love |Being |Penitent |Counter-|Requiem for a |Space ))Thief|(?<!Being )Thief ?(2|II)|The Metal Age) v?(\.| )?(?<Version>\d\.\d+)", IgnoreCaseInvariant | RegexOptions.ExplicitCapture)]
    private static partial Regex NewDarkVersion5();

    [GeneratedRegex(@"\D(?<Version>\d\.\d+) (version of |.?)New ?Dark(?! ?\d\.\d+)|Thief Gold( Patch)? (?<Version>(?!1\.33|1\.37)\d\.\d+)", IgnoreCaseInvariant | RegexOptions.ExplicitCapture)]
    private static partial Regex NewDarkVersion6();

    [GeneratedRegex(@"Version (?<Version>\d\.\d+) of (Thief ?(2|II))", IgnoreCaseInvariant | RegexOptions.ExplicitCapture)]
    private static partial Regex NewDarkVersion7();

    [GeneratedRegex(@"(New ?Dark|""New ?Dark"") (is )?required (.? )v?(\.| )?(?<Version>\d\.\d+)", IgnoreCaseInvariant | RegexOptions.ExplicitCapture)]
    private static partial Regex NewDarkVersion8();

    [GeneratedRegex(@"(?<Version>(?!1\.3(3|7))\d\.\d+) Patch", IgnoreCaseInvariant | RegexOptions.ExplicitCapture)]
    private static partial Regex NewDarkVersion9();

    // Much, much faster to iterate through possible regex matches, common ones first
    // TODO: These are still kinda slow comparatively. Profile to see if any are bottlenecks
    private readonly Regex[] NewDarkVersionRegexes =
    {
        NewDarkVersion1(),
        NewDarkVersion2(),
        NewDarkVersion3(),
        NewDarkVersion4(),
        NewDarkVersion5(),
        NewDarkVersion6(),
        NewDarkVersion7(),
        NewDarkVersion8(),
        NewDarkVersion9(),
    };
#endif

    #region Author

    [GeneratedRegex(
        @"(FM|mis(si|is|i)on|campaign|series) for Thief( Gold|: The Dark Project|\s*2(: The Metal Age)?)\s+by\s*(?<Author>.+)",
        IgnoreCaseInvariant | RegexOptions.ExplicitCapture)]
    private static partial Regex AuthorRegex1();

    [GeneratedRegex(
        @"(A )?Thief( Gold|: The Dark Project|\s*2(: The Metal Age)?) (fan(-| ?)mis((si|is|i)on)|FM|campaign)\s+by (?<Author>.+)",
        IgnoreCaseInvariant | RegexOptions.ExplicitCapture)]
    private static partial Regex AuthorRegex2();

    [GeneratedRegex(
        @"A(n)? (fan(-| ?)mis((si|is|i)on)|FM|campaign)\s+(made\s+)?by\s+(?<Author>.+)",
        IgnoreCaseInvariant | RegexOptions.ExplicitCapture)]
    private static partial Regex AuthorRegex3();

    [GeneratedRegex(
        @"A(n)? .+(-| )part\s+Thief( Gold |: The Dark Project |\s*2(: The Metal Age )?)\s+(fan(-| ?)mis((si|is|i)on)|FM|campaign)\s+((made\s+by)|by|from)\s+(?<Author>.+)",
        IgnoreCaseInvariant | RegexOptions.ExplicitCapture)]
    private static partial Regex AuthorRegex4();

    private readonly Regex[] AuthorRegexes =
    {
        AuthorRegex1(),
        AuthorRegex2(),
        AuthorRegex3(),
        AuthorRegex4(),
    };

    #endregion

    #region Author copyright

    private const string _copyrightSecondPart =
        //language=regexp
        "(?<Months>( (Jan|Febr)uary| Ma(rch|y)| A(pril|ugust)| Ju(ne|ly)| (((Sept|Nov|Dec)em)|Octo)ber))?" +
        //language=regexp
        "(?(Months)(, ?| ))[0-9]*( by| to)? (?<Author>.+)";

    // Unicode 00A9 = copyright symbol

    [GeneratedRegex(
        //language=regexp
        @"^This (level|(fan(-| |))?mis(si|is|i)on|FM) is( made)? (\(c\)|\u00A9) ?" + _copyrightSecondPart,
        IgnoreCaseInvariant | RegexOptions.ExplicitCapture)]
    private static partial Regex AuthorMissionCopyrightRegex1();

    [GeneratedRegex(
        //language=regexp
        @"^The (levels?|(fan(-| |))?mis(si|is|i)ons?|FMs?)( in this (zip|archive( file)?))? (is|are)( made)? (\(c\)|\u00A9) ?" +
        _copyrightSecondPart,
        IgnoreCaseInvariant | RegexOptions.ExplicitCapture)]
    private static partial Regex AuthorMissionCopyrightRegex2();

    [GeneratedRegex(
        //language=regexp
        @"^These (levels|(fan(-| |))?mis(si|is|i)ons|FMs) are( made)? (\(c\)|\u00A9) ?" +
        _copyrightSecondPart,
        IgnoreCaseInvariant | RegexOptions.ExplicitCapture)]
    private static partial Regex AuthorMissionCopyrightRegex3();

    private readonly Regex[] AuthorMissionCopyrightRegexes =
    {
        AuthorMissionCopyrightRegex1(),
        AuthorMissionCopyrightRegex2(),
        AuthorMissionCopyrightRegex3(),
    };

    // This one is only to be used if we know the above line says "Copyright" or something, because it has
    // an @ as an option for a copyright symbol (used by some Theker missions) and we want to be sure it
    // means what we think it means.
    [GeneratedRegex(@"^(Copyright )?(\(c\)|\u00A9|@) ?" + _copyrightSecondPart,
        IgnoreCaseInvariant | RegexOptions.ExplicitCapture)]
    private static partial Regex AuthorGeneralCopyrightIncludeAtSymbolRegex();

    [GeneratedRegex(@"^(Copyright )?(\(c\)|\u00A9) ?" + _copyrightSecondPart,
        IgnoreCaseInvariant | RegexOptions.ExplicitCapture)]
    private static partial Regex AuthorGeneralCopyrightRegex();

    [GeneratedRegex(" [0-9]+.*$")]
    private static partial Regex CopyrightAuthorYearRegex();

    #endregion

    [GeneratedRegex(@"(\s+|\s*(:|-|\u2013|,)\s*)by(\s+|\s*(:|-|\u2013)\s*)(?<Author>.+)",
        IgnoreCaseInvariant | RegexOptions.ExplicitCapture)]
    private static partial Regex TitleByAuthorRegex();

    #region Release date detection

    [GeneratedRegex(@"(:\s*)+", RegexOptions.ExplicitCapture)]
    private static partial Regex MultipleColonsRegex();

    [GeneratedRegex("-{2,}", RegexOptions.ExplicitCapture)]
    private static partial Regex MultipleDashesRegex();

    [GeneratedRegex(@"\u2013{2,}", RegexOptions.ExplicitCapture)]
    private static partial Regex MultipleUnicodeDashesRegex();

    [GeneratedRegex("(Y2K|[0-9])",
        IgnoreCaseInvariant | RegexOptions.ExplicitCapture | RegexOptions.RightToLeft)]
    private static partial Regex AnyDateNumberRTLRegex();

    [GeneratedRegex(@"New ?Dark [0-9]\.[0-9]{1,2}",
        IgnoreCaseInvariant | RegexOptions.ExplicitCapture)]
    private static partial Regex NewDarkAndNumberRegex();

    [GeneratedRegex(@"\.*[0-9]{1,2}\s*\.\s*[0-9]{1,2}\s*\.\s*([0-9]{4}|[0-9]{2})\.*",
        RegexOptions.ExplicitCapture)]
    private static partial Regex EuropeanDateRegex();

    [GeneratedRegex(@"\s*\.\s*", RegexOptions.ExplicitCapture)]
    private static partial Regex PeriodWithOptionalSurroundingSpacesRegex();

    // Tilde: Auldale Chess Tournament saying "March ~8, 2006"
    [GeneratedRegex(@"\s*(,|~|-|/|\.)\s*", RegexOptions.ExplicitCapture)]
    private static partial Regex DateSeparatorsRegex();

    [GeneratedRegex(@"\s*of\s*", IgnoreCaseInvariant)]
    private static partial Regex DateOfSeparatorRegex();

    [GeneratedRegex(@"\s+")]
    private static partial Regex OneOrMoreWhiteSpaceCharsRegex();

    [GeneratedRegex("Febr ", IgnoreCaseInvariant)]
    private static partial Regex FebrRegex();

    [GeneratedRegex("Sept ", IgnoreCaseInvariant)]
    private static partial Regex SeptRegex();

    [GeneratedRegex("Okt ", IgnoreCaseInvariant)]
    private static partial Regex OktRegex();

    [GeneratedRegex("Y2K", IgnoreCaseInvariant)]
    private static partial Regex Y2KRegex();

    [GeneratedRegex("Jan(vier|uar )", IgnoreCaseInvariant | RegexOptions.ExplicitCapture)]
    private static partial Regex JanuaryVariationsRegex();

    [GeneratedRegex("F(eburar(y| )|(é|e)vrier)", IgnoreCaseInvariant | RegexOptions.ExplicitCapture)]
    private static partial Regex FebruaryVariationsRegex();

    [GeneratedRegex("M(ar(tch|s|z)|ärz)", IgnoreCaseInvariant | RegexOptions.ExplicitCapture)]
    private static partial Regex MarchVariationsRegex();

    [GeneratedRegex("avril", IgnoreCaseInvariant)]
    private static partial Regex AprilVariationsRegex();

    [GeneratedRegex("mai", IgnoreCaseInvariant)]
    private static partial Regex MayVariationsRegex();

    [GeneratedRegex("Ju(in|ni)", IgnoreCaseInvariant | RegexOptions.ExplicitCapture)]
    private static partial Regex JuneVariationsRegex();

    [GeneratedRegex("Ju(l(ly|i)|ille(t|r))",
        IgnoreCaseInvariant | RegexOptions.ExplicitCapture)]
    private static partial Regex JulyVariationsRegex();

    [GeneratedRegex("ao(u|û)t",
        IgnoreCaseInvariant | RegexOptions.ExplicitCapture)]
    private static partial Regex AugustVariationsRegex();

    [GeneratedRegex("septembre", IgnoreCaseInvariant)]
    private static partial Regex SeptemberVariationsRegex();

    [GeneratedRegex("\"O(ctobre|ktober)",
        IgnoreCaseInvariant | RegexOptions.ExplicitCapture)]
    private static partial Regex OctoberVariationsRegex();

    [GeneratedRegex("Halloween", IgnoreCaseInvariant)]
    private static partial Regex HalloweenRegex();

    [GeneratedRegex("Christmas", IgnoreCaseInvariant)]
    private static partial Regex ChristmasRegex();

    [GeneratedRegex("novembre", IgnoreCaseInvariant)]
    private static partial Regex NovemberVariationsRegex();

    [GeneratedRegex("D((é|e)cembre|ezember)", IgnoreCaseInvariant | RegexOptions.ExplicitCapture)]
    private static partial Regex DecemberVariationsRegex();

    #endregion

    [GeneratedRegex(@"\s{2,}")]
    private static partial Regex MultipleWhiteSpaceRegex();

    [GeneratedRegex(@"^Mission [0-9]+\:\s*.+")]
    private static partial Regex DarkMod_TDM_MapSequence_MissionLine_Regex();

    [GeneratedRegex("(Title:|Author:|Description:|Version:|Required TDM Version:)",
        RegexOptions.ExplicitCapture)]
    private static partial Regex DarkModTxtFieldsRegex();

    /*
    Catches stuff like "PD" but also "CoS"
    Also catches stuff like "FM" and also roman numerals. We could get clever if we wanted, but that would just
    be a perf tweak, as everything works out fine as is in terms of accuracy.
    */
    [GeneratedRegex(@"(\s+|^)[A-Z]+[a-z]*[A-Z]+([^A-Za-z]|$)",
        RegexOptions.ExplicitCapture)]
    private static partial Regex AcronymRegex();

    #endregion

    #region Titles.str encoding detection

    /*
    Titles.str files are often in OEM850, but we can't detect this with the general purpose charset detector.
    So we detect by looking for byte sequences that represent non-ASCII stock mission names in OEM850.

    NOTE: Do NOT add the surrounding quotes to the byte sequences, or performance will tank!
    This is because hits are way more expensive than misses, and quotes will cause a zillion hits for the first
    char of each keyphrase due to all the quotes in the file. Not having quotes is fine because we're only
    detecting the phrases in OEM850 encoding, so if they show up in some other encoding we won't match them.
    Even the otherwise worryingly short/common-sounding "Mörder" is fine because of this.
    */

    private readonly byte[][] TitlesStrOEM850KeyPhrases =
    {
        // Das Hüter-Training
        new byte[]
        {
            0x44, 0x61, 0x73, 0x20, 0x48, 0x81, 0x74, 0x65, 0x72, 0x2D, 0x54, 0x72, 0x61, 0x69, 0x6E, 0x69, 0x6E,
            0x67,
        },

        // Ausbruch aus dem Cragscleft-Gefängnis
        new byte[]
        {
            0x41, 0x75, 0x73, 0x62, 0x72, 0x75, 0x63, 0x68, 0x20, 0x61, 0x75, 0x73, 0x20, 0x64, 0x65, 0x6D, 0x20,
            0x43, 0x72, 0x61, 0x67, 0x73, 0x63, 0x6C, 0x65, 0x66, 0x74, 0x2D, 0x47, 0x65, 0x66, 0x84, 0x6E, 0x67,
            0x6E, 0x69, 0x73,
        },

        // Mörder
        new byte[] { 0x4D, 0x94, 0x72, 0x64, 0x65, 0x72 },

        // Zurück zur Kathedrale
        new byte[]
        {
            0x5A, 0x75, 0x72, 0x81, 0x63, 0x6B, 0x20, 0x7A, 0x75, 0x72, 0x20, 0x4B, 0x61, 0x74, 0x68, 0x65, 0x64,
            0x72, 0x61, 0x6C, 0x65,
        },

        // Seltsame Gefährten
        new byte[]
        {
            0x53, 0x65, 0x6C, 0x74, 0x73, 0x61, 0x6D, 0x65, 0x20, 0x47, 0x65, 0x66, 0x84, 0x68, 0x72, 0x74, 0x65,
            0x6E,
        },

        // Entraînement d'un gardien
        new byte[]
        {
            0x45, 0x6E, 0x74, 0x72, 0x61, 0x8C, 0x6E, 0x65, 0x6D, 0x65, 0x6E, 0x74, 0x20, 0x64, 0x27, 0x75, 0x6E,
            0x20, 0x67, 0x61, 0x72, 0x64, 0x69, 0x65, 0x6E,
        },

        // L'Epée
        new byte[] { 0x4C, 0x27, 0x45, 0x70, 0x82, 0x65 },

        // La Cathédrale hantée
        new byte[]
        {
            0x4C, 0x61, 0x20, 0x43, 0x61, 0x74, 0x68, 0x82, 0x64, 0x72, 0x61, 0x6C, 0x65, 0x20, 0x68, 0x61, 0x6E,
            0x74, 0x82, 0x65,
        },

        // La Cité Perdue
        new byte[] { 0x4C, 0x61, 0x20, 0x43, 0x69, 0x74, 0x82, 0x20, 0x50, 0x65, 0x72, 0x64, 0x75, 0x65 },

        // Retour à la cathédrale
        new byte[]
        {
            0x52, 0x65, 0x74, 0x6F, 0x75, 0x72, 0x20, 0x85, 0x20, 0x6C, 0x61, 0x20, 0x63, 0x61, 0x74, 0x68, 0x82,
            0x64, 0x72, 0x61, 0x6C, 0x65,
        },

        // Drôles d'acolytes
        new byte[]
        {
            0x44, 0x72, 0x93, 0x6C, 0x65, 0x73, 0x20, 0x64, 0x27, 0x61, 0x63, 0x6F, 0x6C, 0x79, 0x74, 0x65, 0x73,
        },

        // Städtische Spar- und Kreditanstalt
        new byte[]
        {
            0x53, 0x74, 0x84, 0x64, 0x74, 0x69, 0x73, 0x63, 0x68, 0x65, 0x20, 0x53, 0x70, 0x61, 0x72, 0x2D, 0x20,
            0x75, 0x6E, 0x64, 0x20, 0x4B, 0x72, 0x65, 0x64, 0x69, 0x74, 0x61, 0x6E, 0x73, 0x74, 0x61, 0x6C, 0x74,
        },

        // Entführung
        new byte[] { 0x45, 0x6E, 0x74, 0x66, 0x81, 0x68, 0x72, 0x75, 0x6E, 0x67 },

        // Une ingérence romanesque
        new byte[]
        {
            0x55, 0x6E, 0x65, 0x20, 0x69, 0x6E, 0x67, 0x82, 0x72, 0x65, 0x6E, 0x63, 0x65, 0x20, 0x72, 0x6F, 0x6D,
            0x61, 0x6E, 0x65, 0x73, 0x71, 0x75, 0x65,
        },

        // Expédition ... et encaissement
        new byte[]
        {
            0x45, 0x78, 0x70, 0x82, 0x64, 0x69, 0x74, 0x69, 0x6F, 0x6E, 0x20, 0x2E, 0x2E, 0x2E, 0x20, 0x65, 0x74,
            0x20, 0x65, 0x6E, 0x63, 0x61, 0x69, 0x73, 0x73, 0x65, 0x6D, 0x65, 0x6E, 0x74,
        },

        // Une oreille indiscrète
        new byte[]
        {
            0x55, 0x6E, 0x65, 0x20, 0x6F, 0x72, 0x65, 0x69, 0x6C, 0x6C, 0x65, 0x20, 0x69, 0x6E, 0x64, 0x69, 0x73,
            0x63, 0x72, 0x8A, 0x74, 0x65,
        },

        // La Première Banque Urbaine
        new byte[]
        {
            0x55, 0x6E, 0x65, 0x20, 0x6F, 0x72, 0x65, 0x69, 0x6C, 0x6C, 0x65, 0x20, 0x69, 0x6E, 0x64, 0x69, 0x73,
            0x63, 0x72, 0x8A, 0x74, 0x65,
        },

        // Le maître chanteur
        new byte[]
        {
            0x4C, 0x65, 0x20, 0x6D, 0x61, 0x8C, 0x74, 0x72, 0x65, 0x20, 0x63, 0x68, 0x61, 0x6E, 0x74, 0x65, 0x75,
            0x72,
        },

        // Une soirée délicieuse
        new byte[]
        {
            0x55, 0x6E, 0x65, 0x20, 0x73, 0x6F, 0x69, 0x72, 0x82, 0x65, 0x20, 0x64, 0x82, 0x6C, 0x69, 0x63, 0x69,
            0x65, 0x75, 0x73, 0x65,
        },

        // Un repérage périlleux
        new byte[]
        {
            0x55, 0x6E, 0x20, 0x72, 0x65, 0x70, 0x82, 0x72, 0x61, 0x67, 0x65, 0x20, 0x70, 0x82, 0x72, 0x69, 0x6C,
            0x6C, 0x65, 0x75, 0x78,
        },

        // L'enlèvement
        new byte[] { 0x4C, 0x27, 0x65, 0x6E, 0x6C, 0x8A, 0x76, 0x65, 0x6D, 0x65, 0x6E, 0x74 },

        // Sabotage à la Manufacture Fondière
        new byte[]
        {
            0x53, 0x61, 0x62, 0x6F, 0x74, 0x61, 0x67, 0x65, 0x20, 0x85, 0x20, 0x6C, 0x61, 0x20, 0x4D, 0x61, 0x6E,
            0x75, 0x66, 0x61, 0x63, 0x74, 0x75, 0x72, 0x65, 0x20, 0x46, 0x6F, 0x6E, 0x64, 0x69, 0x8A, 0x72, 0x65,
        },
    };

    #endregion

    /// <summary>
    /// Specialized (therefore fast) sort for titles.str lines only. Anything else is likely to throw an
    /// IndexOutOfRangeException.
    /// </summary>
    private sealed class TitlesStrNaturalNumericSort : IComparer<string>
    {
        public int Compare(string? x, string? y)
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
                if (char.IsAsciiDigit(c))
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
                if (char.IsAsciiDigit(c))
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
