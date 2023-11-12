using System.Runtime.CompilerServices;

namespace AL_Common;

public static partial class RTFParserCommon
{
    public sealed partial class SymbolDict
    {
#if NETFRAMEWORK
        /* ANSI-C code produced by gperf version 3.1 */
        /* Command-line: gperf --output-file='c:\\gperfOutputFile.txt' -t 'c:\\gperfFormatFile.txt'  */
        /* Computed positions: -k'1-3,$' */

        //private const int TOTAL_KEYWORDS = 83;
        //private const int MIN_WORD_LENGTH = 1;
        private const int MAX_WORD_LENGTH = 18;
        //private const int MIN_HASH_VALUE = 1;
        private const int MAX_HASH_VALUE = 241;
        /* maximum key range = 241, duplicates = 0 */

        private readonly byte[] asso_values =
        {
            242, 242, 242, 242, 242, 242, 242, 242, 242, 242,
            242, 242, 242, 242, 242, 242, 242, 242, 242, 242,
            242, 242, 242, 242, 242, 242, 242, 242, 242, 242,
            242, 242, 242, 242, 242, 242, 242, 242, 242, 80,
            242, 242, 75, 242, 242, 242, 242, 242, 242, 20,
            242, 242, 242, 242, 242, 242, 242, 242, 242, 242,
            242, 242, 242, 242, 242, 242, 242, 242, 242, 242,
            242, 242, 242, 242, 242, 242, 242, 242, 242, 242,
            242, 242, 242, 242, 242, 242, 242, 242, 242, 242,
            242, 242, 65, 242, 242, 242, 242, 0, 50, 5,
            15, 5, 10, 80, 35, 15, 5, 5, 55, 95,
            0, 5, 30, 15, 5, 45, 0, 60, 0, 0,
            5, 0, 242, 50, 242, 40, 5, 242, 242, 242,
            242, 242, 242, 242, 242, 242, 242, 242, 242, 242,
            242, 242, 242, 242, 242, 242, 242, 242, 242, 242,
            242, 242, 242, 242, 242, 242, 242, 242, 242, 242,
            242, 242, 242, 242, 242, 242, 242, 242, 242, 242,
            242, 242, 242, 242, 242, 242, 242, 242, 242, 242,
            242, 242, 242, 242, 242, 242, 242, 242, 242, 242,
            242, 242, 242, 242, 242, 242, 242, 242, 242, 242,
            242, 242, 242, 242, 242, 242, 242, 242, 242, 242,
            242, 242, 242, 242, 242, 242, 242, 242, 242, 242,
            242, 242, 242, 242, 242, 242, 242, 242, 242, 242,
            242, 242, 242, 242, 242, 242, 242, 242, 242, 242,
            242, 242, 242, 242, 242, 242, 242, 242, 242, 242,
            242, 242, 242, 242, 242, 242
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
// Entry 14
            new Symbol("v", 1, false, KeywordType.Property, (int)Property.Hidden),
            null, null, null, null, null, null, null, null, null,
// Entry 23
            new Symbol("~", 0, false, KeywordType.Character, ' '),
// Entry 65
            new Symbol("tc", 0, false, KeywordType.Destination, (int)DestinationType.Skip),
// Entry 78
            new Symbol("row", 0, false, KeywordType.Character, '\n'),
            null,
// Entry 49
            new Symbol("ftncn", 0, false, KeywordType.Destination, (int)DestinationType.Skip),
            null,
// Entry 68
            new Symbol("xe", 0, false, KeywordType.Destination, (int)DestinationType.Skip),
// Entry 67
            new Symbol("txe", 0, false, KeywordType.Destination, (int)DestinationType.Skip),
            null, null,
// Entry 7
            new Symbol("f", 0, false, KeywordType.Property, (int)Property.FontNum),
// Entry 51
            new Symbol("ftnsepc", 0, false, KeywordType.Destination, (int)DestinationType.Skip),
// Entry 62
            new Symbol("rxe", 0, false, KeywordType.Destination, (int)DestinationType.Skip),
            null,
// Entry 66
            new Symbol("title", 0, false, KeywordType.Destination, (int)DestinationType.Skip),
            null, null, null,
// Entry 73
            new Symbol("datastore", 0, false, KeywordType.Destination, (int)DestinationType.SkippableHex),
            null,
// Entry 44
            new Symbol("footer", 0, false, KeywordType.Destination, (int)DestinationType.Skip),
// Entry 47
            new Symbol("footerr", 0, false, KeywordType.Destination, (int)DestinationType.Skip),
// Entry 48
            new Symbol("footnote", 0, false, KeywordType.Destination, (int)DestinationType.Skip),
// Entry 56
            new Symbol("info", 0, false, KeywordType.Destination, (int)DestinationType.Skip),
            null,
// Entry 37
            new Symbol("pntext", 0, false, KeywordType.Destination, (int)DestinationType.Skip),
// Entry 45
            new Symbol("footerf", 0, false, KeywordType.Destination, (int)DestinationType.Skip),
// Entry 3
            new Symbol("pca", 850, true, KeywordType.Special, (int)SpecialType.HeaderCodePage),
// Entry 74
            new Symbol("datafield", 0, false, KeywordType.Destination, (int)DestinationType.SkippableHex),
            null,
// Entry 77
            new Symbol("panose", 20, true, KeywordType.Special, (int)SpecialType.SkipNumberOfBytes),
// Entry 1
            new Symbol("pc", 437, true, KeywordType.Special, (int)SpecialType.HeaderCodePage),
// Entry 15
            new Symbol("par", 0, false, KeywordType.Character, '\n'),
// Entry 5
            new Symbol("deff", 0, false, KeywordType.Special, (int)SpecialType.DefaultFont),
            null,
// Entry 50
            new Symbol("ftnsep", 0, false, KeywordType.Destination, (int)DestinationType.Skip),
            null, null,
// Entry 70
            new Symbol("themedata", 0, false, KeywordType.Destination, (int)DestinationType.SkippableHex),
            null,
// Entry 52
            new Symbol("header", 0, false, KeywordType.Destination, (int)DestinationType.Skip),
// Entry 55
            new Symbol("headerr", 0, false, KeywordType.Destination, (int)DestinationType.Skip),
// Entry 58
            new Symbol("operator", 0, false, KeywordType.Destination, (int)DestinationType.Skip),
// Entry 69
            new Symbol("pict", 0, false, KeywordType.Destination, (int)DestinationType.SkippableHex),
// Entry 63
            new Symbol("stylesheet", 0, false, KeywordType.Destination, (int)DestinationType.Skip),
            null,
// Entry 53
            new Symbol("headerf", 0, false, KeywordType.Destination, (int)DestinationType.Skip),
// Entry 8
            new Symbol("fcharset", -1, false, KeywordType.Special, (int)SpecialType.Charset),
            null, null,
// Entry 25
            new Symbol("endash", 0, false, KeywordType.Character, '\x2013'),
// Entry 21
            new Symbol("enspace", 0, false, KeywordType.Character, ' '),
// Entry 57
            new Symbol("keywords", 0, false, KeywordType.Destination, (int)DestinationType.Skip),
// Entry 0
            new Symbol("ansi", 1252, true, KeywordType.Special, (int)SpecialType.HeaderCodePage),
            null, null,
// Entry 75
            new Symbol("objdata", 0, false, KeywordType.Destination, (int)DestinationType.SkippableHex),
// Entry 30
            new Symbol("bin", 0, false, KeywordType.Special, (int)SpecialType.SkipNumberOfBytes),
            null, null,
// Entry 38
            new Symbol("author", 0, false, KeywordType.Destination, (int)DestinationType.Skip),
// Entry 11
            new Symbol("uc", 1, false, KeywordType.Property, (int)Property.UnicodeCharSkipCount),
// Entry 17
            new Symbol("softline", 0, false, KeywordType.Character, '\n'),
            null, null, null,
// Entry 6
            new Symbol("fonttbl", 0, false, KeywordType.Special, (int)SpecialType.FontTable),
// Entry 60
            new Symbol("private1", 0, false, KeywordType.Destination, (int)DestinationType.Skip),
// Entry 16
            new Symbol("line", 0, false, KeywordType.Character, '\n'),
            null,
// Entry 81
            new Symbol("}", 0, false, KeywordType.Character, '}'),
// Entry 46
            new Symbol("footerl", 0, false, KeywordType.Destination, (int)DestinationType.Skip),
            null,
// Entry 29
            new Symbol("rdblquote", 0, false, KeywordType.Character, '\x201D'),
            null, null,
// Entry 32
            new Symbol("fldinst", 0, false, KeywordType.Destination, (int)DestinationType.FieldInstruction),
            null, null, null,
// Entry 27
            new Symbol("rquote", 0, false, KeywordType.Character, '\x2019'),
// Entry 35
            new Symbol("ts", 0, false, KeywordType.Destination, (int)DestinationType.CanBeDestOrNotDest),
            null, null, null, null,
// Entry 33
            new Symbol("cs", 0, false, KeywordType.Destination, (int)DestinationType.CanBeDestOrNotDest),
            null, null, null,
// Entry 80
            new Symbol("{", 0, false, KeywordType.Character, '{'),
// Entry 54
            new Symbol("headerl", 0, false, KeywordType.Destination, (int)DestinationType.Skip),
// Entry 18
            new Symbol("tab", 0, false, KeywordType.Character, '\t'),
            null, null, null,
// Entry 34
            new Symbol("ds", 0, false, KeywordType.Destination, (int)DestinationType.CanBeDestOrNotDest),
// Entry 2
            new Symbol("mac", 10000, true, KeywordType.Special, (int)SpecialType.HeaderCodePage),
            null, null,
// Entry 61
            new Symbol("revtim", 0, false, KeywordType.Destination, (int)DestinationType.Skip),
// Entry 41
            new Symbol("comment", 0, false, KeywordType.Destination, (int)DestinationType.Skip),
            null, null, null, null,
// Entry 42
            new Symbol("creatim", 0, false, KeywordType.Destination, (int)DestinationType.Skip),
            null, null, null,
// Entry 13
            new Symbol("u", 0, false, KeywordType.Special, (int)SpecialType.UnicodeChar),
// Entry 72
            new Symbol("passwordhash", 0, false, KeywordType.Destination, (int)DestinationType.SkippableHex),
// Entry 36
            new Symbol("listtext", 0, false, KeywordType.Destination, (int)DestinationType.Skip),
// Entry 79
            new Symbol("cell", 0, false, KeywordType.Character, ' '),
            null, null,
// Entry 43
            new Symbol("doccomm", 0, false, KeywordType.Destination, (int)DestinationType.Skip),
// Entry 40
            new Symbol("colortbl", 0, false, KeywordType.Special, (int)SpecialType.ColorTable),
            null, null,
// Entry 82
            new Symbol("\\", 0, false, KeywordType.Character, '\\'),
// Entry 4
            new Symbol("ansicpg", 1252, false, KeywordType.Special, (int)SpecialType.HeaderCodePage),
            null,
// Entry 28
            new Symbol("ldblquote", 0, false, KeywordType.Character, '\x201C'),
            null, null, null, null,
// Entry 10
            new Symbol("lang", 0, false, KeywordType.Property, (int)Property.Lang),
            null,
// Entry 26
            new Symbol("lquote", 0, false, KeywordType.Character, '\x2018'),
// Entry 76
            new Symbol("blipuid", 32, true, KeywordType.Special, (int)SpecialType.SkipNumberOfBytes),
            null, null, null, null, null, null, null, null,
// Entry 31
            new Symbol("*", 0, false, KeywordType.Special, (int)SpecialType.SkipDest),
// Entry 59
            new Symbol("printim", 0, false, KeywordType.Destination, (int)DestinationType.Skip),
            null, null, null,
// Entry 24
            new Symbol("emdash", 0, false, KeywordType.Character, '\x2014'),
// Entry 20
            new Symbol("emspace", 0, false, KeywordType.Character, ' '),
            null, null, null,
// Entry 12
            new Symbol("'", 0, false, KeywordType.Special, (int)SpecialType.HexEncodedChar),
// Entry 64
            new Symbol("subject", 0, false, KeywordType.Destination, (int)DestinationType.Skip),
// Entry 71
            new Symbol("colorschememapping", 0, false, KeywordType.Destination, (int)DestinationType.SkippableHex),
            null, null, null,
// Entry 22
            new Symbol("qmspace", 0, false, KeywordType.Character, ' '),
            null, null, null,
// Entry 19
            new Symbol("bullet", 0, false, KeywordType.Character, '\x2022'),
            null, null, null, null, null, null, null, null, null,
            null, null, null, null, null, null, null, null, null,
            null, null, null, null, null, null, null, null,
// Entry 9
            new Symbol("cpg", -1, false, KeywordType.Special, (int)SpecialType.CodePage),
            null, null, null, null, null, null, null, null, null,
            null, null, null, null, null, null, null, null, null,
            null, null, null, null, null, null, null, null, null,
            null, null, null, null, null, null, null, null, null,
            null, null, null, null, null, null,
// Entry 39
            new Symbol("buptim", 0, false, KeywordType.Destination, (int)DestinationType.Skip)
        };
#endif
    }
}
