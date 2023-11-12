#if !NETFRAMEWORK
using System.Runtime.CompilerServices;
#endif

namespace AL_Common;

public static partial class RTFParserCommon
{
    public sealed partial class SymbolDict
    {
#if !NETFRAMEWORK
        /* ANSI-C code produced by gperf version 3.1 */
        /* Command-line: 'C:\\gperf\\tools\\gperf.exe' --output-file='C:\\_al_rtf_table_gen\\gperfOutputFile.txt' -t 'C:\\_al_rtf_table_gen\\gperfFormatFile.txt'  */
        /* Computed positions: -k'1-3,$' */

        //private const int TOTAL_KEYWORDS = 86;
        //private const int MIN_WORD_LENGTH = 1;
        private const int MAX_WORD_LENGTH = 18;
        //private const int MIN_HASH_VALUE = 1;
        private const int MAX_HASH_VALUE = 261;
        /* maximum key range = 261, duplicates = 0 */

        private readonly ushort[] asso_values =
        {
            262, 262, 262, 262, 262, 262, 262, 262, 262, 262,
            262, 262, 262, 262, 262, 262, 262, 262, 262, 262,
            262, 262, 262, 262, 262, 262, 262, 262, 262, 262,
            262, 262, 262, 262, 262, 262, 262, 262, 262,  35,
            262, 262,  30, 262, 262, 262, 262, 262, 262,   0,
            262, 262, 262, 262, 262, 262, 262, 262, 262, 262,
            262, 262, 262, 262, 262, 262, 262, 262, 262, 262,
            262, 262, 262, 262, 262, 262, 262, 262, 262, 262,
            262, 262, 262, 262, 262, 262, 262, 262, 262, 262,
            262, 262,  25, 262, 262, 262, 262,   0,  30,   0,
            35,   0,  10,  60,  50,  10,  85,  10,   0,  65,
            25,   0,  50,  25,  70,   5,  20, 110,   5,   0,
            17,   5, 262,  20, 262,  15,   0, 262, 262, 262,
            262, 262, 262, 262, 262, 262, 262, 262, 262, 262,
            262, 262, 262, 262, 262, 262, 262, 262, 262, 262,
            262, 262, 262, 262, 262, 262, 262, 262, 262, 262,
            262, 262, 262, 262, 262, 262, 262, 262, 262, 262,
            262, 262, 262, 262, 262, 262, 262, 262, 262, 262,
            262, 262, 262, 262, 262, 262, 262, 262, 262, 262,
            262, 262, 262, 262, 262, 262, 262, 262, 262, 262,
            262, 262, 262, 262, 262, 262, 262, 262, 262, 262,
            262, 262, 262, 262, 262, 262, 262, 262, 262, 262,
            262, 262, 262, 262, 262, 262, 262, 262, 262, 262,
            262, 262, 262, 262, 262, 262, 262, 262, 262, 262,
            262, 262, 262, 262, 262, 262, 262, 262, 262, 262,
            262, 262, 262, 262, 262, 262
        };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int Hash(char[] keyword, int len)
        {
            int key = len;

            // Original C code does a stupid thing where it puts default at the top and falls through and junk,
            // but we can't do that in C#, so have something clearer/clunkier
            switch (len)
            {
                // Most common case first - we get a measurable speedup from this
                case > 2:
                    key += asso_values[keyword[2]];
                    key += asso_values[keyword[1]];
                    key += asso_values[keyword[0]];
                    break;
                case 1:
                    key += asso_values[keyword[0]];
                    break;
                case 2:
                    key += asso_values[keyword[1]];
                    key += asso_values[keyword[0]];
                    break;
            }
            key += asso_values[keyword[len - 1]];

            return key;
        }

        // NOTE: This is generated. Values can be modified, but not keys (keys are the first string params).
        // Also no reordering. Adding, removing, reordering, or modifying keys requires generating a new version.
        // See RTF_SymbolListGenSource.cs for how to generate a new version (it also contains the original
        // Symbol list which must be used as the source to generate this one).
        private readonly Symbol?[] _symbolTable =
        {
            null,
// Entry 23
            new Symbol("~", 0, false, KeywordType.Character, ' '),
            null, null,
// Entry 79
            new Symbol("cell", 0, false, KeywordType.Character, ' '),
            null, null, null,
// Entry 40
            new Symbol("colortbl", 0, false, KeywordType.Special, (int)SpecialType.ColorTable),
            null, null,
// Entry 14
            new Symbol("v", 1, false, KeywordType.Property, (int)Property.Hidden),
// Entry 33
            new Symbol("cs", 0, false, KeywordType.Destination, (int)DestinationType.CanBeDestOrNotDest),
            null, null, null, null,
// Entry 46
            new Symbol("footerl", 0, false, KeywordType.Destination, (int)DestinationType.Skip),
// Entry 48
            new Symbol("footnote", 0, false, KeywordType.Destination, (int)DestinationType.Skip),
// Entry 68
            new Symbol("xe", 0, false, KeywordType.Destination, (int)DestinationType.Skip),
            null,
// Entry 7
            new Symbol("f", 0, false, KeywordType.Property, (int)Property.FontNum),
// Entry 65
            new Symbol("tc", 0, false, KeywordType.Destination, (int)DestinationType.Skip),
// Entry 17
            new Symbol("softline", 0, false, KeywordType.Character, '\n'),
            null, null, null,
// Entry 45
            new Symbol("footerf", 0, false, KeywordType.Destination, (int)DestinationType.Skip),
// Entry 57
            new Symbol("keywords", 0, false, KeywordType.Destination, (int)DestinationType.Skip),
            null, null,
// Entry 81
            new Symbol("}", 0, false, KeywordType.Character, '}'),
// Entry 35
            new Symbol("ts", 0, false, KeywordType.Destination, (int)DestinationType.CanBeDestOrNotDest),
            null, null, null, null,
// Entry 21
            new Symbol("enspace", 0, false, KeywordType.Character, ' '),
            null,
// Entry 16
            new Symbol("line", 0, false, KeywordType.Character, '\n'),
// Entry 67
            new Symbol("txe", 0, false, KeywordType.Destination, (int)DestinationType.Skip),
// Entry 80
            new Symbol("{", 0, false, KeywordType.Character, '{'),
// Entry 6
            new Symbol("fonttbl", 0, false, KeywordType.Special, (int)SpecialType.FontTable),
// Entry 36
            new Symbol("listtext", 0, false, KeywordType.Destination, (int)DestinationType.Skip),
// Entry 0
            new Symbol("ansi", 1252, true, KeywordType.Special, (int)SpecialType.HeaderCodePage),
// Entry 84
            new Symbol("sectd", 20, true, KeywordType.Special, (int)SpecialType.ForegroundColorReset),
            null,
// Entry 34
            new Symbol("ds", 0, false, KeywordType.Destination, (int)DestinationType.CanBeDestOrNotDest),
            null,
// Entry 56
            new Symbol("info", 0, false, KeywordType.Destination, (int)DestinationType.Skip),
            null,
// Entry 82
            new Symbol("\\", 0, false, KeywordType.Character, '\\'),
// Entry 1
            new Symbol("pc", 437, true, KeywordType.Special, (int)SpecialType.HeaderCodePage),
// Entry 3
            new Symbol("pca", 850, true, KeywordType.Special, (int)SpecialType.HeaderCodePage),
            null,
// Entry 66
            new Symbol("title", 0, false, KeywordType.Destination, (int)DestinationType.Skip),
            null,
// Entry 54
            new Symbol("headerl", 0, false, KeywordType.Destination, (int)DestinationType.Skip),
            null,
// Entry 5
            new Symbol("deff", 0, false, KeywordType.Special, (int)SpecialType.DefaultFont),
// Entry 63
            new Symbol("stylesheet", 0, false, KeywordType.Destination, (int)DestinationType.Skip),
// Entry 31
            new Symbol("*", 0, false, KeywordType.Special, (int)SpecialType.SkipDest),
// Entry 51
            new Symbol("ftnsepc", 0, false, KeywordType.Destination, (int)DestinationType.Skip),
            null,
// Entry 73
            new Symbol("datastore", 0, false, KeywordType.Destination, (int)DestinationType.SkippableHex),
            null, null,
// Entry 53
            new Symbol("headerf", 0, false, KeywordType.Destination, (int)DestinationType.Skip),
// Entry 2
            new Symbol("mac", 10000, true, KeywordType.Special, (int)SpecialType.HeaderCodePage),
            null, null,
// Entry 12
            new Symbol("'", 0, false, KeywordType.Special, (int)SpecialType.HexEncodedChar),
// Entry 32
            new Symbol("fldinst", 0, false, KeywordType.Destination, (int)DestinationType.FieldInstruction),
// Entry 78
            new Symbol("row", 0, false, KeywordType.Character, '\n'),
// Entry 28
            new Symbol("ldblquote", 0, false, KeywordType.Character, '\x201C'),
            null, null,
// Entry 20
            new Symbol("emspace", 0, false, KeywordType.Character, ' '),
// Entry 71
            new Symbol("colorschememapping", 0, false, KeywordType.Destination, (int)DestinationType.SkippableHex),
// Entry 70
            new Symbol("themedata", 0, false, KeywordType.Destination, (int)DestinationType.SkippableHex),
// Entry 85
            new Symbol("plain", 20, true, KeywordType.Special, (int)SpecialType.ForegroundColorReset),
// Entry 77
            new Symbol("panose", 20, true, KeywordType.Special, (int)SpecialType.SkipNumberOfBytes),
// Entry 76
            new Symbol("blipuid", 32, true, KeywordType.Special, (int)SpecialType.SkipNumberOfBytes),
// Entry 18
            new Symbol("tab", 0, false, KeywordType.Character, '\t'),
// Entry 69
            new Symbol("pict", 0, false, KeywordType.Destination, (int)DestinationType.SkippableHex),
// Entry 49
            new Symbol("ftncn", 0, false, KeywordType.Destination, (int)DestinationType.Skip),
// Entry 44
            new Symbol("footer", 0, false, KeywordType.Destination, (int)DestinationType.Skip),
// Entry 47
            new Symbol("footerr", 0, false, KeywordType.Destination, (int)DestinationType.Skip),
// Entry 8
            new Symbol("fcharset", -1, false, KeywordType.Special, (int)SpecialType.Charset),
// Entry 10
            new Symbol("lang", 0, false, KeywordType.Property, (int)Property.Lang),
// Entry 62
            new Symbol("rxe", 0, false, KeywordType.Destination, (int)DestinationType.Skip),
            null,
// Entry 41
            new Symbol("comment", 0, false, KeywordType.Destination, (int)DestinationType.Skip),
// Entry 30
            new Symbol("bin", 0, false, KeywordType.Special, (int)SpecialType.SkipNumberOfBytes),
            null, null, null,
// Entry 4
            new Symbol("ansicpg", 1252, false, KeywordType.Special, (int)SpecialType.HeaderCodePage),
            null,
// Entry 74
            new Symbol("datafield", 0, false, KeywordType.Destination, (int)DestinationType.SkippableHex),
            null, null,
// Entry 22
            new Symbol("qmspace", 0, false, KeywordType.Character, ' '),
            null, null, null, null,
// Entry 43
            new Symbol("doccomm", 0, false, KeywordType.Destination, (int)DestinationType.Skip),
            null, null, null,
// Entry 50
            new Symbol("ftnsep", 0, false, KeywordType.Destination, (int)DestinationType.Skip),
// Entry 11
            new Symbol("uc", 1, false, KeywordType.Property, (int)Property.UnicodeCharSkipCount),
            null, null, null,
// Entry 25
            new Symbol("endash", 0, false, KeywordType.Character, '\x2013'),
// Entry 72
            new Symbol("passwordhash", 0, false, KeywordType.Destination, (int)DestinationType.SkippableHex),
            null, null, null,
// Entry 37
            new Symbol("pntext", 0, false, KeywordType.Destination, (int)DestinationType.Skip),
// Entry 75
            new Symbol("objdata", 0, false, KeywordType.Destination, (int)DestinationType.SkippableHex),
            null, null, null,
// Entry 52
            new Symbol("header", 0, false, KeywordType.Destination, (int)DestinationType.Skip),
// Entry 55
            new Symbol("headerr", 0, false, KeywordType.Destination, (int)DestinationType.Skip),
// Entry 58
            new Symbol("operator", 0, false, KeywordType.Destination, (int)DestinationType.Skip),
            null, null, null, null, null, null, null, null, null,
// Entry 60
            new Symbol("private1", 0, false, KeywordType.Destination, (int)DestinationType.Skip),
            null, null,
// Entry 26
            new Symbol("lquote", 0, false, KeywordType.Character, '\x2018'),
// Entry 42
            new Symbol("creatim", 0, false, KeywordType.Destination, (int)DestinationType.Skip),
            null,
// Entry 29
            new Symbol("rdblquote", 0, false, KeywordType.Character, '\x201D'),
            null,
// Entry 61
            new Symbol("revtim", 0, false, KeywordType.Destination, (int)DestinationType.Skip),
            null, null, null, null, null, null, null, null, null,
// Entry 24
            new Symbol("emdash", 0, false, KeywordType.Character, '\x2014'),
            null, null,
// Entry 83
            new Symbol("pard", 20, true, KeywordType.Special, (int)SpecialType.ForegroundColorReset),
            null, null, null, null, null, null,
// Entry 19
            new Symbol("bullet", 0, false, KeywordType.Character, '\x2022'),
            null, null, null, null, null,
// Entry 64
            new Symbol("subject", 0, false, KeywordType.Destination, (int)DestinationType.Skip),
// Entry 9
            new Symbol("cpg", -1, false, KeywordType.Special, (int)SpecialType.CodePage),
            null, null, null, null, null, null, null, null, null,
            null, null, null, null, null, null, null, null, null,
            null,
// Entry 15
            new Symbol("par", 0, false, KeywordType.Character, '\n'),
            null, null, null, null, null, null, null, null,
// Entry 59
            new Symbol("printim", 0, false, KeywordType.Destination, (int)DestinationType.Skip),
            null, null, null,
// Entry 38
            new Symbol("author", 0, false, KeywordType.Destination, (int)DestinationType.Skip),
            null, null, null, null,
// Entry 27
            new Symbol("rquote", 0, false, KeywordType.Character, '\x2019'),
            null, null, null, null, null, null, null, null, null,
// Entry 13
            new Symbol("u", 0, false, KeywordType.Special, (int)SpecialType.UnicodeChar),
            null, null, null, null, null, null, null, null, null,
            null, null, null, null, null, null, null, null, null,
            null, null, null, null, null, null, null, null, null,
            null, null, null, null, null, null, null, null, null,
            null, null, null,
// Entry 39
            new Symbol("buptim", 0, false, KeywordType.Destination, (int)DestinationType.Skip),
        };
#endif
    }
}
