﻿// Uncomment this define in all files it appears in to get all features (we use it for testing)
//#define FMScanner_FullCode

using System.Collections.Generic;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using static AL_Common.Common;
using static AL_Common.LanguageSupport;

namespace FMScanner;

// IMPORTANT: No lazy-loading allowed in here. Everything should be immediate-initialized for thread safety.
public sealed class ReadOnlyDataContext
{
    [StructLayout(LayoutKind.Auto)]
    internal readonly struct AsciiCharWithNonAsciiEquivalent(char original, char ascii)
    {
        internal readonly char Original = original;
        internal readonly char Ascii = ascii;
    }

    internal readonly string[] FMFiles_TitlesStrLocations;

    internal readonly string[] Languages_FS_Lang_FS;
    internal readonly string[] Languages_FS_Lang_Language_FS;
    internal readonly string[] LanguagesC;

    internal readonly byte[] RomanNumeralToDecimalTable;

    // Used for SS2 fingerprinting for the game type scan fallback
    [SuppressMessage("ReSharper", "StringLiteralTypo")]
    internal readonly HashSetI FMFiles_SS2MisFiles = new(23)
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

    public ReadOnlyDataContext()
    {
        Languages_FS_Lang_FS = new string[SupportedLanguageCount];
        Languages_FS_Lang_Language_FS = new string[SupportedLanguageCount];
        LanguagesC = new string[SupportedLanguageCount];

        #region FMFiles_TitlesStrLocations

        // 2 entries per language, plus an additional 2 for the no-language-dir titles files
        FMFiles_TitlesStrLocations = new string[(SupportedLanguageCount * 2) + 2];

        // Do not change search order: strings/english, strings, strings/[any other language]
        FMFiles_TitlesStrLocations[0] = "strings/english/titles.str";
        FMFiles_TitlesStrLocations[1] = "strings/english/title.str";
        FMFiles_TitlesStrLocations[2] = "strings/titles.str";
        FMFiles_TitlesStrLocations[3] = "strings/title.str";

        for (int i = 1; i < SupportedLanguageCount; i++)
        {
            string lang = SupportedLanguages[i];
            FMFiles_TitlesStrLocations[(i - 1) + 4] = "strings/" + lang + "/titles.str";
            FMFiles_TitlesStrLocations[(i - 1) + 4 + (SupportedLanguageCount - 1)] = "strings/" + lang + "/title.str";
        }

        #endregion

        #region Languages

        for (int i = 0; i < SupportedLanguageCount; i++)
        {
            string lang = SupportedLanguages[i];
            Languages_FS_Lang_FS[i] = "/" + lang + "/";
            Languages_FS_Lang_Language_FS[i] = "/" + lang + " Language/";

            // Lowercase to first-char-uppercase: Cheesy hack because it wasn't designed this way.
            LanguagesC[i] = (char)(lang[0] - 32) + lang.Substring(1);
        }

        #endregion

        #region Roman numeral table

        RomanNumeralToDecimalTable = new byte['X' + 1];
        RomanNumeralToDecimalTable['I'] = 1;
        RomanNumeralToDecimalTable['V'] = 5;
        RomanNumeralToDecimalTable['X'] = 10;

        #endregion
    }

    internal readonly TitlesStrNaturalNumericSortComparer TitlesStrNaturalNumericSort = new();

    internal readonly AsciiCharWithNonAsciiEquivalent[] NonAsciiCharsWithAsciiEquivalents =
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

    #region Misc preallocated char and string arrays

    // Perf, for passing to params[]-taking methods so we don't allocate all the time
    internal readonly char[] CA_Period = { '.' };
    internal readonly char[] CA_Asterisk = { '*' };
    internal readonly char[] CA_AsteriskHyphen = { '*', '-' };
    internal readonly char[] CA_UnicodeQuotes = { LeftDoubleQuote, RightDoubleQuote };
    internal readonly char[] CA_DateSeparators = { ' ', '-', '/' };
    internal readonly char[] CA_Parens = { '(', ')' };
    internal readonly string[] SA_Linebreaks = { "\r\n", "\r", "\n" };
    internal readonly string[] SA_DoubleSpaces = { "  " };
    internal readonly string[] SA_T3DetectExtensions = { "*.ibt", "*.cbt", "*.gmp", "*.ned", "*.unr" };
    internal readonly string[] SA_AllFiles = { "*" };
    internal readonly string[] SA_AllBinFiles = { "*.bin" };
    internal readonly string[] SA_AllSubFiles = { "*.sub" };

    #endregion

    #region Field detect strings

    // IMPORTANT(Field detect strings): Always use lowercase for letters where you want case insensitivity!
    // This gets matched with "given case or upper case" to prevent false positives from lowercase first letters.

    internal readonly string[] SA_TitleDetect =
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

    internal readonly string[] SA_AuthorDetect =
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

    internal readonly string[] SA_LatestUpdateDateDetect =
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

    internal readonly string[] SA_ReleaseDateDetect =
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
    internal readonly string[] SA_VersionDetect = { "Version" };
#endif

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
    internal readonly string[] ImageFileExtensions = { ".gif", ".pcx", ".tga", ".dds", ".png", ".bmp" };
    internal readonly string[] ImageFilePatterns = { "*.gif", "*.pcx", "*.tga", "*.dds", "*.png", "*.bmp" };

    internal readonly string[] MotionFilePatterns = { "*.mc", "*.mi" };
    internal readonly string[] MotionFileExtensions = { ".mc", ".mi" };

    // .osm for the classic scripts; .nut for Squirrel scripts for NewDark >= 1.25
    internal readonly string[] ScriptFileExtensions = { ".osm", ".nut" };

    #endregion

    #region Dates

    internal readonly string[]
    DateFormatsEuropean =
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
    internal readonly (string Format, bool CanBeAmbiguous)[]
    DateFormats =
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

    internal readonly string[]
    MonthNames =
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

    internal const int SS2_NewDark_MAPPARAM_Location = 696;
    internal const int T2_OldDark_SKYOBJVAR_Location = 772;
    internal const int SS2_OldDark_MAPPARAM_Location = 916;
    // Neither of these clash with SS2's SKYOBJVAR locations (3168, 7292).
    internal const int NewDark_SKYOBJVAR_Location1 = 7217;
    internal const int NewDark_SKYOBJVAR_Location2 = 3093;

    internal const int SS2_NewDark_MAPPARAM_Offset = 705; // 696+9 = 705
    internal const int T2_OldDark_SKYOBJVAR_Offset = 76;  // (772+9)-705 = 76
    internal const int SS2_OldDark_MAPPARAM_Offset = 144; // ((916+9)-76)-705 = 144
    internal const int NewDark_SKYOBJVAR_Offset1 = 2177;  // (((3093+9)-144)-76)-705 = 2177
    internal const int NewDark_SKYOBJVAR_Offset2 = 4124;  // ((((7217+9)-2177)-144)-76)-705 = 4124

    internal readonly byte[] OBJ_MAP = "OBJ_MAP"u8.ToArray();

    /*
    In theory, someone could make a Thief 1 mission with a RopeyArrow archetype. It's never happened and is
    vanishingly unlikely to happen, but hey. Before RopeyArrow, there will be an int32 id number followed by
    the bytes 0B 00 00 00 (for .mis files) or 0F 00 00 00 0B 00 00 00 (for .gam files). 99% of the time this
    id number is D7 F3 FF FF (presumably the T2 default), so we could almost prepend this to our string for
    extra strength defense against a custom RopeyArrow archetype... except that a handful of legit T2 missions
    have different ids. So unfortunately if we want to stay accurate we have to stay with just "RopeyArrow".
    */
    internal readonly byte[] RopeyArrow = "RopeyArrow"u8.ToArray();

    internal readonly int[] GameDetect_KeyPhraseLocations =
    {
        SS2_NewDark_MAPPARAM_Location,
        T2_OldDark_SKYOBJVAR_Location,
        SS2_OldDark_MAPPARAM_Location,
        NewDark_SKYOBJVAR_Location1,
        NewDark_SKYOBJVAR_Location2,
    };

    internal readonly int[] GameDetect_KeyPhraseZipOffsets =
    {
        SS2_NewDark_MAPPARAM_Offset,
        T2_OldDark_SKYOBJVAR_Offset,
        SS2_OldDark_MAPPARAM_Offset,
        NewDark_SKYOBJVAR_Offset1,
        NewDark_SKYOBJVAR_Offset2,
    };

    // ReSharper restore IdentifierTypo

    #endregion

    #region Regexes

    // PERF: Making regexes compiled increases their performance by a huge amount.
    // And as we know, regexes need all the performance help they can get.

    internal readonly Regex AThiefMissionRegex =
        new Regex("^A Thief( 1| 2| Gold)? (fan|campaign)",
            RegexOptions.ExplicitCapture | IgnoreCaseInvariant | RegexOptions.Compiled);

    internal readonly Regex AThief3MissionRegex =
        new Regex(@"^A\s+Thief(\s+|\s+:\s+|\s+-\s+)Deadly",
            RegexOptions.ExplicitCapture | IgnoreCaseInvariant | RegexOptions.Compiled);

    internal readonly Regex OpenParenSpacesRegex =
        new Regex(@"\(\s+", RegexOptions.Compiled);

    internal readonly Regex CloseParenSpacesRegex =
        new Regex(@"\s+\)", RegexOptions.Compiled);

    internal readonly Regex DaySuffixesRegex =
        new Regex("[0-9](?<Suffix>(st|nd|rd|th)).+",
            RegexOptions.ExplicitCapture | IgnoreCaseInvariant | RegexOptions.Compiled);

    internal readonly Regex AuthorEmailRegex =
        new Regex(@"\(?\S+@\S+\.\S{2,5}\)?", RegexOptions.Compiled);

#if FMScanner_FullCode
    internal readonly Regex VersionExclude1Regex =
        new Regex(@"\d\.\d+\+", RegexOptions.Compiled);

    // TODO: This one looks iffy though
    internal readonly Regex VersionFirstNumberRegex =
        new Regex(@"[0123456789\.]+", RegexOptions.Compiled);

    // Much, much faster to iterate through possible regex matches, common ones first
    // TODO: These are still kinda slow comparatively. Profile to see if any are bottlenecks
    internal readonly Regex[] NewDarkVersionRegexes =
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
            RegexOptions.ExplicitCapture),
    };
#endif

    internal readonly Regex[] AuthorRegexes =
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
            RegexOptions.ExplicitCapture),
    };

    private const string _copyrightSecondPart =
        //language=regexp
        "(?<Months>( (Jan|Febr)uary| Ma(rch|y)| A(pril|ugust)| Ju(ne|ly)| (((Sept|Nov|Dec)em)|Octo)ber))?" +
        //language=regexp
        "(?(Months)(, ?| ))[0-9]*( by| to)? (?<Author>.+)";

    // Unicode 00A9 = copyright symbol

    internal readonly Regex[] AuthorMissionCopyrightRegexes =
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
            RegexOptions.ExplicitCapture),
    };

    // This one is only to be used if we know the above line says "Copyright" or something, because it has
    // an @ as an option for a copyright symbol (used by some Theker missions) and we want to be sure it
    // means what we think it means.
    internal readonly Regex AuthorGeneralCopyrightIncludeAtSymbolRegex =
        new Regex(
            //language=regexp
            @"^(Copyright )?(\(c\)|\u00A9|@) ?" + _copyrightSecondPart,
            IgnoreCaseInvariant | RegexOptions.Compiled | RegexOptions.ExplicitCapture);

    internal readonly Regex AuthorGeneralCopyrightRegex =
        new Regex(
            //language=regexp
            @"^(Copyright )?(\(c\)|\u00A9) ?" + _copyrightSecondPart,
            IgnoreCaseInvariant | RegexOptions.Compiled | RegexOptions.ExplicitCapture);

    internal readonly Regex CopyrightAuthorYearRegex = new Regex(" [0-9]+.*$", RegexOptions.Compiled);

    internal readonly Regex TitleByAuthorRegex =
        new Regex(@"(\s+|\s*(:|-|\u2013|,)\s*)by(\s+|\s*(:|-|\u2013)\s*)(?<Author>.+)",
            IgnoreCaseInvariant | RegexOptions.Compiled | RegexOptions.ExplicitCapture);

    #region Release date detection

    internal readonly Regex MultipleColonsRegex =
        new Regex(@"(:\s*)+",
            RegexOptions.Compiled | RegexOptions.ExplicitCapture);

    internal readonly Regex MultipleDashesRegex =
        new Regex("-{2,}",
            RegexOptions.Compiled | RegexOptions.ExplicitCapture);

    internal readonly Regex MultipleUnicodeDashesRegex =
        new Regex(@"\u2013{2,}",
            RegexOptions.Compiled | RegexOptions.ExplicitCapture);

    internal readonly Regex AnyDateNumberRTLRegex =
        new Regex("(Y2K|[0-9])",
            IgnoreCaseInvariant | RegexOptions.Compiled | RegexOptions.ExplicitCapture | RegexOptions.RightToLeft);

    internal readonly Regex NewDarkAndNumberRegex =
        new Regex(@"New ?Dark [0-9]\.[0-9]{1,2}",
            IgnoreCaseInvariant | RegexOptions.Compiled | RegexOptions.ExplicitCapture);

    internal readonly Regex EuropeanDateRegex =
        new Regex(@"\.*[0-9]{1,2}\s*\.\s*[0-9]{1,2}\s*\.\s*([0-9]{4}|[0-9]{2})\.*",
            RegexOptions.Compiled | RegexOptions.ExplicitCapture);

    internal readonly Regex PeriodWithOptionalSurroundingSpacesRegex =
        new Regex(@"\s*\.\s*",
            RegexOptions.Compiled | RegexOptions.ExplicitCapture);

    internal readonly Regex DateSeparatorsRegex =
        // Tilde: Auldale Chess Tournament saying "March ~8, 2006"
        new Regex(@"\s*(,|~|-|/|\.)\s*",
            RegexOptions.Compiled | RegexOptions.ExplicitCapture);

    internal readonly Regex DateOfSeparatorRegex =
        new Regex(@"\s*of\s*",
            IgnoreCaseInvariant | RegexOptions.Compiled);

    internal readonly Regex OneOrMoreWhiteSpaceCharsRegex =
        new Regex(@"\s+", RegexOptions.Compiled);

    internal readonly Regex FebrRegex =
        new Regex("Febr ",
            IgnoreCaseInvariant | RegexOptions.Compiled);

    internal readonly Regex SeptRegex =
        new Regex("Sept ",
            IgnoreCaseInvariant | RegexOptions.Compiled);

    internal readonly Regex OktRegex =
        new Regex("Okt ",
            IgnoreCaseInvariant | RegexOptions.Compiled);

    internal readonly Regex Y2KRegex =
        new Regex("Y2K",
            IgnoreCaseInvariant | RegexOptions.Compiled);

    internal readonly Regex JanuaryVariationsRegex =
        new Regex("Jan(vier|uar )",
            IgnoreCaseInvariant | RegexOptions.Compiled | RegexOptions.ExplicitCapture);

    internal readonly Regex FebruaryVariationsRegex =
        new Regex("F(eburar(y| )|(é|e)vrier)",
            IgnoreCaseInvariant | RegexOptions.Compiled | RegexOptions.ExplicitCapture);

    internal readonly Regex MarchVariationsRegex =
        new Regex("M(ar(tch|s|z)|ärz)",
            IgnoreCaseInvariant | RegexOptions.Compiled | RegexOptions.ExplicitCapture);

    internal readonly Regex AprilVariationsRegex =
        new Regex("avril",
            IgnoreCaseInvariant | RegexOptions.Compiled);

    internal readonly Regex MayVariationsRegex =
        new Regex("mai",
            IgnoreCaseInvariant | RegexOptions.Compiled);

    internal readonly Regex JuneVariationsRegex =
        new Regex("Ju(in|ni)",
            IgnoreCaseInvariant | RegexOptions.Compiled | RegexOptions.ExplicitCapture);

    internal readonly Regex JulyVariationsRegex =
        new Regex("Ju(l(ly|i)|ille(t|r))",
            IgnoreCaseInvariant | RegexOptions.Compiled | RegexOptions.ExplicitCapture);

    internal readonly Regex AugustVariationsRegex =
        new Regex("ao(u|û)t",
            IgnoreCaseInvariant | RegexOptions.Compiled | RegexOptions.ExplicitCapture);

    internal readonly Regex SeptemberVariationsRegex =
        new Regex("septembre",
            IgnoreCaseInvariant | RegexOptions.Compiled);

    internal readonly Regex OctoberVariationsRegex =
        new Regex("O(ctobre|ktober)",
            IgnoreCaseInvariant | RegexOptions.Compiled | RegexOptions.ExplicitCapture);

    internal readonly Regex HalloweenRegex =
        new Regex("Halloween",
            IgnoreCaseInvariant | RegexOptions.Compiled | RegexOptions.ExplicitCapture);

    internal readonly Regex ChristmasRegex =
        new Regex("Christmas",
            IgnoreCaseInvariant | RegexOptions.Compiled | RegexOptions.ExplicitCapture);

    internal readonly Regex NovemberVariationsRegex =
        new Regex("novembre",
            IgnoreCaseInvariant | RegexOptions.Compiled | RegexOptions.ExplicitCapture);

    internal readonly Regex DecemberVariationsRegex =
        new Regex("D((é|e)cembre|ezember)",
            IgnoreCaseInvariant | RegexOptions.Compiled | RegexOptions.ExplicitCapture);

    #endregion

    internal readonly Regex MultipleWhiteSpaceRegex =
        new Regex(@"\s{2,}", RegexOptions.Compiled);

    internal readonly Regex DarkMod_TDM_MapSequence_MissionLine_Regex =
        new Regex(@"^Mission [0-9]+\:\s*.+", RegexOptions.Compiled);

    internal readonly Regex DarkModTxtFieldsRegex =
        new Regex("(Title:|Author:|Description:|Version:|Required TDM Version:)",
            RegexOptions.Compiled | RegexOptions.ExplicitCapture);

    /*
    Catches stuff like "PD" but also "CoS"
    Also catches stuff like "FM" and also roman numerals. We could get clever if we wanted, but that would just
    be a perf tweak, as everything works out fine as is in terms of accuracy.
    */
    internal readonly Regex AcronymRegex =
        new Regex(@"(\s+|^)[A-Z]+[a-z]*[A-Z]+([^A-Za-z]|$)",
            RegexOptions.Compiled | RegexOptions.ExplicitCapture);

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

    // Bytes which, in one encoding, would be unexpected characters (symbols, box-drawing chars, etc.), but which
    // in the other encoding would be commonly expected characters.

    // 1/0 for true/false, to enable branchless occurrence counting

    private static byte[] InitSuspected1252Bytes()
    {
        byte[] ret = new byte[256];

        ret[0xB0] = 1;
        ret[0xB1] = 1;
        ret[0xB2] = 1;
        ret[0xB3] = 1;
        ret[0xB4] = 1;
        ret[0xB9] = 1;
        ret[0xBA] = 1;
        ret[0xBB] = 1;
        ret[0xBC] = 1;
        ret[0xBF] = 1;

        ret[0xC0] = 1;
        ret[0xC1] = 1;
        ret[0xC2] = 1;
        ret[0xC3] = 1;
        ret[0xC4] = 1;
        ret[0xC5] = 1;
        ret[0xC8] = 1;
        ret[0xC9] = 1;
        ret[0xCA] = 1;
        ret[0xCB] = 1;
        ret[0xCC] = 1;
        ret[0xCD] = 1;
        ret[0xCE] = 1;

        ret[0xD9] = 1;
        ret[0xDA] = 1;
        ret[0xDB] = 1;
        ret[0xDC] = 1;
        ret[0xDD] = 1;
        ret[0xDF] = 1;

        ret[0xEE] = 1;

        ret[0xF0] = 1;
        ret[0xF1] = 1;
        ret[0xF2] = 1;
        ret[0xF3] = 1;
        ret[0xF4] = 1;
        ret[0xF5] = 1;
        ret[0xF6] = 1;
        ret[0xF8] = 1;
        ret[0xF9] = 1;
        ret[0xFA] = 1;
        ret[0xFB] = 1;
        ret[0xFC] = 1;
        ret[0xFD] = 1;
        ret[0xFE] = 1;
        ret[0xFF] = 1;

        return ret;
    }

    private static byte[] InitSuspected850Bytes()
    {
        byte[] ret = new byte[256];

        ret[0x80] = 1;
        ret[0x81] = 1;
        ret[0x82] = 1;
        ret[0x83] = 1;
        ret[0x84] = 1;
        ret[0x85] = 1;
        ret[0x86] = 1;
        ret[0x87] = 1;
        ret[0x88] = 1;
        ret[0x89] = 1;
        ret[0x8B] = 1;
        ret[0x8D] = 1;
        ret[0x8F] = 1;

        ret[0xA0] = 1;
        // A1 is either ¡ (1252) or í (850) - this is sort of a toss-up...
        // Let's leave out A1 for now.
        ret[0xA2] = 1;
        ret[0xA3] = 1;
        ret[0xA4] = 1;
        ret[0xA5] = 1;

        ret[0xB5] = 1;
        ret[0xB6] = 1;
        ret[0xB7] = 1;

        // C6: Æ in 1252; ã in 850
        // C7: Ç in 1252; Ã in 850

        ret[0xD7] = 1;

        return ret;
    }

    internal readonly byte[] Suspected1252Bytes = InitSuspected1252Bytes();

    internal readonly byte[] Suspected850Bytes = InitSuspected850Bytes();

    internal readonly byte[][] TitlesStrOEM850KeyPhrases =
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

    internal const char LeftDoubleQuote = '\u201C';
    internal const char RightDoubleQuote = '\u201D';

    [SuppressMessage("ReSharper", "IdentifierTypo")]
    internal static class FMDirs
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
    internal static class FMFiles
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

    #region Comparer classes

    /// <summary>
    /// Specialized (therefore fast) sort for titles.str lines only. Anything else is likely to throw an
    /// IndexOutOfRangeException.
    /// </summary>
    internal sealed class TitlesStrNaturalNumericSortComparer : IComparer<string>
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

    internal sealed class FMScanOriginalIndexComparer : IComparer<ScannedFMDataAndError>
    {
        public int Compare(ScannedFMDataAndError x, ScannedFMDataAndError y)
        {
            return x.OriginalIndex.CompareTo(y.OriginalIndex);
        }
    }

    #endregion
}