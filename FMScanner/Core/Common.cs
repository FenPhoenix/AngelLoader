﻿/*
FMScanner - A fast, thorough, accurate scanner for Thief 1 and Thief 2 fan missions.

Written in 2017-2019 by FenPhoenix.

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

        internal const string T3Detect = @"Content\T3\Maps\";
        internal const string T3FMExtras1 = "Fan Mission Extras";
        internal const string T3FMExtras2 = "FanMissionExtras";

        // Perf, so directory separator char doesn't have to converted to a string and concatenated
        // NOTE: This is dubious, did I even profile this? Still, the scanner is lightning fast anyway, so whatever
        internal static string T3DetectS(char dsc) => dsc == '/' ? "Content/T3/Maps/" : @"Content\T3\Maps\";
        internal static string T3FMExtras1S(char dsc) => dsc == '/' ? "Fan Mission Extras/" : @"Fan Mission Extras\";
        internal static string T3FMExtras2S(char dsc) => dsc == '/' ? "FanMissionExtras/" : @"FanMissionExtras\";

        internal static string BooksS(char dsc) => dsc == '/' ? "books/" : @"books\";
        internal static string FamS(char dsc) => dsc == '/' ? "fam/" : @"fam\";
        internal static string IntrfaceS(char dsc) => dsc == '/' ? "intrface/" : @"intrface\";
        internal static string MeshS(char dsc) => dsc == '/' ? "mesh/" : @"mesh\";
        internal static string MotionsS(char dsc) => dsc == '/' ? "motions/" : @"motions\";
        internal static string MoviesS(char dsc) => dsc == '/' ? "movies/" : @"movies\";
        internal static string CutscenesS(char dsc) => dsc == '/' ? "cutscenes/" : @"cutscenes\"; // SS2 only
        internal static string ObjS(char dsc) => dsc == '/' ? "obj/" : @"obj\";
        internal static string ScriptsS(char dsc) => dsc == '/' ? "scripts/" : @"scripts\";
        internal static string SndS(char dsc) => dsc == '/' ? "snd/" : @"snd\";
        internal static string Snd2S(char dsc) => dsc == '/' ? "snd2/" : @"snd2\"; // SS2 only
        internal static string StringsS(char dsc) => dsc == '/' ? "strings/" : @"strings\";
        internal static string SubtitlesS(char dsc) => dsc == '/' ? "subtitles/" : @"subtitles\";

    }

    internal static class FMFiles
    {
        internal const string MissFlag = "missflag.str";
        internal const string TitlesStr = "titles.str";
        internal const string TitleStr = "title.str";
        internal const string NewGameStr = "newgame.str";

        // Telliamed's fminfo.xml file, used in a grand total of three missions
        internal const string FMInfoXml = "fminfo.xml";

        // fm.ini, a NewDark (or just FMSel?) file
        internal const string FMIni = "fm.ini";

        // System Shock 2 file
        internal const string ModIni = "mod.ini";
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
        internal static readonly byte[] Bytes10 = new byte[10];
        internal static readonly byte[] Bytes5 = new byte[5];
    }

    internal static class FMConstants
    {
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

        // NOTE: I think this was for GetLanguages() for the planned accuracy update?
        //internal static string[] LanguageDirs { get; } = { FMDirs.Books, FMDirs.Intrface, FMDirs.Strings };

        internal static readonly string[] Languages =
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

        // Cheesy hack because it wasn't designed this way
        internal static readonly Dictionary<string, string> LanguagesC = new Dictionary<string, string>
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

        internal static readonly string[] DateFormats =
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
        internal static readonly char[] SkyObjVar = { 'S', 'K', 'Y', 'O', 'B', 'J', 'V', 'A', 'R' };
        internal static readonly char[] ObjMap = { 'O', 'B', 'J', '_', 'M', 'A', 'P' };
        internal static readonly byte[] Thief2UniqueStringMis = Encoding.ASCII.GetBytes("RopeyArrow");
        internal static readonly byte[] Thief2UniqueStringGam = Encoding.ASCII.GetBytes("RopeyArrow");

        // SS2-only detection string
        internal static readonly char[] MapParam = { 'M', 'A', 'P', 'P', 'A', 'R', 'A', 'M' };
    }

    [PublicAPI] // Not public, but whatever
    internal sealed class FMIniData
    {
        internal string NiceName { get; set; }
        internal string ReleaseDate { get; set; }
        internal string InfoFile { get; set; }
        internal string Tags { get; set; }
        internal string Descr { get; set; }
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
                @"(FM|mission|campaign|series) for Thief( Gold|: The Dark Project|\s*2(: The Metal Age)?)\s+by\s*(?<Author>.+)",
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
                @"^This (level|(fan(-| |))?mission|FM) is( made)? (\(c\)|\u00A9) ?" + CopyrightSecondPart,
                RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.ExplicitCapture),
            new Regex(
                //language=regexp
                @"^The (levels?|(fan(-| |))?missions?|FMs?)( in this (zip|archive( file)?))? (is|are)( made)? (\(c\)|\u00A9) ?" + CopyrightSecondPart,
                RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.ExplicitCapture),
            new Regex(
                //language=regexp
                @"^These (levels|(fan(-| |))?missions|FMs) are( made)? (\(c\)|\u00A9) ?" + CopyrightSecondPart,
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
            if (string.IsNullOrEmpty(x)) return -1;
            if (string.IsNullOrEmpty(y)) return 1;

            int xIndex1;
            var xNum = x.Substring(xIndex1 = x.IndexOf('_') + 1, x.IndexOf(':') - xIndex1);
            int yIndex1;
            var yNum = y.Substring(yIndex1 = y.IndexOf('_') + 1, y.IndexOf(':') - yIndex1);

            while (xNum.Length < yNum.Length) xNum = '0' + xNum;
            while (yNum.Length < xNum.Length) yNum = '0' + yNum;

            return string.CompareOrdinal(xNum, yNum);
        }
    }
}
