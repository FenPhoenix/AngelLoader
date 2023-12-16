using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using static AL_Common.Common;

namespace AL_Common;

public static partial class RTFParserCommon
{
    // Perf: A readonly struct is required to retain full performance, and therefore we can only put readonly
    // things in here (no mutable value types like the unicode skip counter etc.)
    public readonly struct Context
    {
        public readonly char[] Keyword;
        public readonly GroupStack GroupStack;
        public readonly FontDictionary FontEntries;
        public readonly Header Header;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Reset()
        {
            GroupStack.ClearFast();
            GroupStack.ResetFirst();
            FontEntries.Clear();
            Header.Reset();
        }

        public Context()
        {
            Keyword = new char[KeywordMaxLen];
            GroupStack = new GroupStack();

            /*
            FMs can have 100+ of these...
            Highest measured was 131
            Fonts can specify themselves as whatever number they want, so we can't just count by index
            eg. you could have \f1 \f2 \f3 but you could also have \f1 \f14 \f45
            */
            FontEntries = new FontDictionary(150);

            Header = new Header();
        }
    }

    #region Constants

    public const int KeywordMaxLen = 32;
    // Most are signed int16 (5 chars), but a few can be signed int32 (10 chars)
    public const int ParamMaxLen = 10;

    public const int UndefinedLanguage = 1024;

    /// <summary>
    /// Since font numbers can be negative, let's just use a slightly less likely value than the already unlikely
    /// enough -1...
    /// </summary>
    public const int NoFontNumber = int.MinValue;

    #endregion

    #region Conversion tables

    #region Charset to code page

    private const int _charSetToCodePageLength = 256;
    private static readonly int[] _charSetToCodePage = InitializeCharSetToCodePage();

    private static int[] InitializeCharSetToCodePage()
    {
        int[] charSetToCodePage = InitializedArray(_charSetToCodePageLength, -1);

        charSetToCodePage[0] = 1252;   // "ANSI" (1252)

        // TODO: Code page 0 ("Default") is variable... should we force it to 1252?
        // "The system default Windows ANSI code page" says the doc page.
        // Terrible. Fortunately only two known FMs define it in a font entry, and neither one actually uses
        // said font entry. Still, maybe this should be 1252 as well, since we're rolling dice anyway we may
        // as well go with the statistically likeliest?
        charSetToCodePage[1] = 0;      // Default

        charSetToCodePage[2] = 42;     // Symbol
        charSetToCodePage[77] = 10000; // Mac Roman
        charSetToCodePage[78] = 10001; // Mac Shift Jis
        charSetToCodePage[79] = 10003; // Mac Hangul
        charSetToCodePage[80] = 10008; // Mac GB2312
        charSetToCodePage[81] = 10002; // Mac Big5
        //charSetToCodePage[82] = ?    // Mac Johab (old)
        charSetToCodePage[83] = 10005; // Mac Hebrew
        charSetToCodePage[84] = 10004; // Mac Arabic
        charSetToCodePage[85] = 10006; // Mac Greek
        charSetToCodePage[86] = 10081; // Mac Turkish
        charSetToCodePage[87] = 10021; // Mac Thai
        charSetToCodePage[88] = 10029; // Mac East Europe
        charSetToCodePage[89] = 10007; // Mac Russian
        charSetToCodePage[128] = 932;  // Shift JIS (Windows-31J) (932)
        charSetToCodePage[129] = 949;  // Hangul
        charSetToCodePage[130] = 1361; // Johab
        charSetToCodePage[134] = 936;  // GB2312
        charSetToCodePage[136] = 950;  // Big5
        charSetToCodePage[161] = 1253; // Greek
        charSetToCodePage[162] = 1254; // Turkish
        charSetToCodePage[163] = 1258; // Vietnamese
        charSetToCodePage[177] = 1255; // Hebrew
        charSetToCodePage[178] = 1256; // Arabic
        //charSetToCodePage[179] = ?   // Arabic Traditional (old)
        //charSetToCodePage[180] = ?   // Arabic user (old)
        //charSetToCodePage[181] = ?   // Hebrew user (old)
        charSetToCodePage[186] = 1257; // Baltic
        charSetToCodePage[204] = 1251; // Russian
        charSetToCodePage[222] = 874;  // Thai
        charSetToCodePage[238] = 1250; // Eastern European
        charSetToCodePage[254] = 437;  // PC 437
        charSetToCodePage[255] = 850;  // OEM

        return charSetToCodePage;
    }

    #endregion

    #region Lang to code page

    public const int MaxLangNumDigits = 5;
    public const int MaxLangNumIndex = 16385;
    public static readonly int[] LangToCodePage = InitializeLangToCodePage();

    private static int[] InitializeLangToCodePage()
    {
        int[] langToCodePage = InitializedArray(MaxLangNumIndex + 1, -1);

        /*
        There's a ton more languages than this, but it's not clear what code page they all translate to.
        This should be enough to get on with for now though...

        Note: 1024 is implicitly rejected by simply not being in the list, so we're all good there.

        2023-03-31: Only handle 1049 for now (and leave in 1033 for the plaintext converter).
        */
#if false
        // Arabic
        langToCodePage[1065] = 1256;
        langToCodePage[1025] = 1256;
        langToCodePage[2049] = 1256;
        langToCodePage[3073] = 1256;
        langToCodePage[4097] = 1256;
        langToCodePage[5121] = 1256;
        langToCodePage[6145] = 1256;
        langToCodePage[7169] = 1256;
        langToCodePage[8193] = 1256;
        langToCodePage[9217] = 1256;
        langToCodePage[10241] = 1256;
        langToCodePage[11265] = 1256;
        langToCodePage[12289] = 1256;
        langToCodePage[13313] = 1256;
        langToCodePage[14337] = 1256;
        langToCodePage[15361] = 1256;
        langToCodePage[16385] = 1256;
        langToCodePage[1056] = 1256;
        langToCodePage[2118] = 1256;
        langToCodePage[2137] = 1256;
        langToCodePage[1119] = 1256;
        langToCodePage[1120] = 1256;
        langToCodePage[1123] = 1256;
        langToCodePage[1164] = 1256;
#endif

        // Cyrillic
        langToCodePage[1049] = 1251;
#if false
        langToCodePage[1026] = 1251;
        langToCodePage[10266] = 1251;
        langToCodePage[1058] = 1251;
        langToCodePage[2073] = 1251;
        langToCodePage[3098] = 1251;
        langToCodePage[7194] = 1251;
        langToCodePage[8218] = 1251;
        langToCodePage[12314] = 1251;
        langToCodePage[1059] = 1251;
        langToCodePage[1064] = 1251;
        langToCodePage[2092] = 1251;
        langToCodePage[1071] = 1251;
        langToCodePage[1087] = 1251;
        langToCodePage[1088] = 1251;
        langToCodePage[2115] = 1251;
        langToCodePage[1092] = 1251;
        langToCodePage[1104] = 1251;
        langToCodePage[1133] = 1251;
        langToCodePage[1157] = 1251;

        // Greek
        langToCodePage[1032] = 1253;

        // Hebrew
        langToCodePage[1037] = 1255;
        langToCodePage[1085] = 1255;

        // Vietnamese
        langToCodePage[1066] = 1258;
#endif

        // Western European
        langToCodePage[1033] = 1252;

        return langToCodePage;
    }

    #endregion

    #endregion

    #region Classes

    public sealed class GroupStack
    {
        // SOA and removal of bounds checking through fixed-sized buffers improves perf

        public unsafe struct IntArrayWrapper
        {
            internal fixed int Array[MaxGroups];
        }

        public unsafe struct BoolArrayWrapper
        {
            internal fixed bool Array[MaxGroups];
        }

        // Highest measured was 10
        public const int MaxGroups = 100;

        private IntArrayWrapper RtfDestinationStates;
        private BoolArrayWrapper InFontTables;
        public IntArrayWrapper SymbolFonts;
        public readonly int[][] Properties = new int[MaxGroups][];

        /// <summary>Do not modify!</summary>
        public int Count;

        public GroupStack()
        {
            for (int i = 0; i < MaxGroups; i++)
            {
                Properties[i] = new int[_propertiesLen];
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void DeepCopyToNext()
        {
            RtfDestinationStates.Array[Count + 1] = RtfDestinationStates.Array[Count];
            InFontTables.Array[Count + 1] = InFontTables.Array[Count];
            SymbolFonts.Array[Count + 1] = SymbolFonts.Array[Count];
            for (int i = 0; i < _propertiesLen; i++)
            {
                Properties[Count + 1][i] = Properties[Count][i];
            }
            ++Count;
        }

        #region Current group

        public unsafe RtfDestinationState CurrentRtfDestinationState
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (RtfDestinationState)RtfDestinationStates.Array[Count];
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => RtfDestinationStates.Array[Count] = (int)value;
        }

        public unsafe bool CurrentInFontTable
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => InFontTables.Array[Count];
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => InFontTables.Array[Count] = value;
        }

        public unsafe SymbolFont CurrentSymbolFont
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (SymbolFont)SymbolFonts.Array[Count];
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => SymbolFonts.Array[Count] = (int)value;
        }

        public int[] CurrentProperties
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Properties[Count];
        }

        // Current group always begins at group 0, so reset just that one
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void ResetFirst()
        {
            RtfDestinationStates.Array[0] = 0;
            InFontTables.Array[0] = false;
            SymbolFonts.Array[0] = (int)SymbolFont.None;

            Properties[0][(int)Property.Hidden] = 0;
            Properties[0][(int)Property.UnicodeCharSkipCount] = 1;
            Properties[0][(int)Property.FontNum] = NoFontNumber;
            Properties[0][(int)Property.Lang] = -1;
        }

        #endregion

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ClearFast() => Count = 0;
    }

    public sealed class Symbol
    {
        public readonly string Keyword;
        public readonly int DefaultParam;
        public readonly bool UseDefaultParam;
        public readonly KeywordType KeywordType;
        /// <summary>
        /// Index into the property table, or a regular enum member, or a character literal, depending on <see cref="KeywordType"/>.
        /// </summary>
        public readonly int Index;

        public Symbol(string keyword, int defaultParam, bool useDefaultParam, KeywordType keywordType, int index)
        {
            Keyword = keyword;
            DefaultParam = defaultParam;
            UseDefaultParam = useDefaultParam;
            KeywordType = keywordType;
            Index = index;
        }
    }

    public sealed class Header
    {
        public int CodePage;
        public bool DefaultFontSet;
        public int DefaultFontNum;

        public Header() => Reset();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Reset()
        {
            CodePage = 1252;
            DefaultFontSet = false;
            DefaultFontNum = 0;
        }
    }

    public sealed class SymbolDict
    {
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

        // For "emspace", "enspace", "qmspace", "~"
        // Just convert these to regular spaces because we're just trying to scan for strings in readmes
        // without weird crap tripping us up
        // emspace  '\x2003'
        // enspace  '\x2002'
        // qmspace  '\x2005'
        // ~        '\xa0'

        // For "emdash", "endash", "lquote", "rquote", "ldblquote", "rdblquote"
        // NOTE: Maybe just convert these all to ASCII equivalents as well?

        // For "cs", "ds", "ts"
        // Hack to make sure we extract the \fldrslt text from Thief Trinity in that one place.

        // For "listtext", "pntext"
        // Ignore list item bullets and numeric prefixes etc. We don't need them.

        // For "v"
        // \v to make all plain text hidden (not output to the conversion stream), \v0 to make it shown again

        // For "ansi"
        // The spec calls this "ANSI (the default)" but says nothing about what codepage that actually means.
        // "ANSI" is often misused to mean one of the Windows codepages, so I'll assume it's Windows-1252.

        // For "mac"
        // The spec calls this "Apple Macintosh" but again says nothing about what codepage that is. I'll
        // assume 10000 ("Mac Roman")

        // For "fldinst"
        // We need to do stuff with this (SYMBOL instruction)

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

        // It seems like directly looking up a char in this table would be faster, but it measures the same to
        // slightly slower. My random guess is that the main table is always in cache, and this one often isn't?
#if false
        private static Symbol?[] InitControlSymbolArray()
        {
            Symbol?[] ret = new Symbol?[256];
            ret['\''] = new Symbol("'", 0, false, KeywordType.Special, (int)SpecialType.HexEncodedChar);
            ret['*'] = new Symbol("*", 0, false, KeywordType.Special, (int)SpecialType.SkipDest);
            ret['\\'] = new Symbol("\\", 0, false, KeywordType.Character, '\\');
            ret['{'] = new Symbol("{", 0, false, KeywordType.Character, '{');
            ret['}'] = new Symbol("}", 0, false, KeywordType.Character, '}');
            ret['~'] = new Symbol("~", 0, false, KeywordType.Character, ' ');
            return ret;
        }

        public readonly Symbol?[] ControlSymbols = InitControlSymbolArray();
#endif

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Symbol? LookUpControlSymbol(char ch)
        {
            byte val = asso_values[ch];
            int key = 1 + val + val;

            if (key <= MAX_HASH_VALUE)
            {
                Symbol? symbol = _symbolTable[key];
                if (symbol == null)
                {
                    return null;
                }

                string seq2 = symbol.Keyword;
                if (seq2.Length != 1)
                {
                    return null;
                }

                if (ch != seq2[0])
                {
                    return null;
                }

                return symbol;
            }

            return null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Symbol? LookUpControlWord(char[] keyword, int len)
        {
            // Min word length is 1, and we're guaranteed to always be at least 1, so no need to check for >= min
            if (len <= MAX_WORD_LENGTH)
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

                if (key <= MAX_HASH_VALUE)
                {
                    Symbol? symbol = _symbolTable[key];
                    if (symbol == null)
                    {
                        return null;
                    }

                    string seq2 = symbol.Keyword;
                    if (len != seq2.Length)
                    {
                        return null;
                    }

                    for (int ci = 0; ci < len; ci++)
                    {
                        if (keyword[ci] != seq2[ci])
                        {
                            return null;
                        }
                    }

                    return symbol;
                }
            }

            return null;
        }
    }

    public sealed class FontEntry
    {
        // Use only as many chars as we need - "Wingdings" is 9 chars and is the longest we need
        private const int _nameMaxLength = 9;

        // We need to store names in case we get codepage 42 nonsense, we need to know which font to translate
        // to Unicode (Wingdings, Webdings, or Symbol)
        private readonly char[] _name = new char[_nameMaxLength];
        private int _nameCharPos;

        private bool _nameDone;

        public int CodePage = -1;

        public SymbolFont SymbolFont = SymbolFont.Unset;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Reset()
        {
            _nameCharPos = 0;
            _nameDone = false;
            CodePage = -1;
            SymbolFont = SymbolFont.Unset;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool NameEquals(char[] array2)
        {
            if (_nameCharPos != array2.Length) return false;

            for (int i = 0; i < _nameCharPos; i++)
            {
                if (_name[i] != array2[i]) return false;
            }

            return true;
        }

        /*
        @MEM(Rtf FontEntry name char[9]):
        We could use just one char buffer and fill it with the name, then when we're done we convert it to a
        SymbolFont enum and set it on the top font entry. Maybe we could even detect as we fill it out if it
        can't be a supported symbol font name and early-out with setting SymbolFont.None right there.
        */
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AppendNameChar(char c)
        {
            if (!_nameDone && _nameCharPos < _nameMaxLength)
            {
                if (c == ';')
                {
                    _nameDone = true;
                    return;
                }
                _name[_nameCharPos++] = c;
            }
        }
    }

    public sealed class FontDictionary
    {
        private readonly int _capacity;
        private Dictionary<int, FontEntry>? _dict;

        private readonly ListFast<FontEntry> _fontEntryPool;
        private int _fontEntryPoolVirtualCount;

        private readonly FontEntry?[] _array = new FontEntry?[_switchPoint];

        /*
        \fN params are normally in the signed int16 range, but the Windows RichEdit control supports them in the
        -30064771071 - 30064771070 (-0x6ffffffff - 0x6fffffffe) range (yes, bizarre numbers, but I tested and
        there they are). So we're going to use the array for the expected "normal" range, and fall back to the
        dictionary for weird crap that probably won't happen.
        */
        private const int _switchPoint = 32767;

        public FontEntry? Top;

        public FontDictionary(int capacity)
        {
            _capacity = capacity;
            _fontEntryPool = new ListFast<FontEntry>(capacity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(int key)
        {
            FontEntry fontEntry;
            if (_fontEntryPoolVirtualCount > 0)
            {
                --_fontEntryPoolVirtualCount;
                fontEntry = _fontEntryPool[_fontEntryPoolVirtualCount];
                fontEntry.Reset();
            }
            else
            {
                fontEntry = new FontEntry();
                _fontEntryPool.Add(fontEntry);
            }

            Top = fontEntry;
            if (key is < 0 or >= _switchPoint)
            {
                _dict ??= new Dictionary<int, FontEntry>(_capacity);
                _dict[key] = fontEntry;
            }
            else
            {
                _array[key] = fontEntry;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            Top = null;
            _dict?.Clear();
            _array.Clear();
            _fontEntryPoolVirtualCount = _fontEntryPool.Count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ClearFull(int capacity)
        {
            _fontEntryPool.Capacity = capacity;
            Clear();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetValue(int key, [NotNullWhen(true)] out FontEntry? value)
        {
            if (key is < 0 or >= _switchPoint)
            {
                if (_dict == null)
                {
                    value = null;
                    return false;
                }
                else
                {
                    return _dict.TryGetValue(key, out value);
                }
            }
            else
            {
                value = _array[key];
                return value != null;
            }
        }
    }

    #endregion

    #region Enums

    public enum SpecialType
    {
        HeaderCodePage,
        DefaultFont,
        FontTable,
        Charset,
        CodePage,
        UnicodeChar,
        HexEncodedChar,
        SkipNumberOfBytes,
        SkipDest,
        ColorTable
    }

    public enum KeywordType
    {
        Character,
        Property,
        Destination,
        Special
    }

    public enum DestinationType
    {
        FieldInstruction,
        /// <summary>
        /// This is for \csN, \dsN, and \tsN.
        /// <para/>
        /// These are weird hybrids that can either be written as destinations (eg. "\*\cs15") or not (eg. "\cs15").
        /// <para/>
        /// The spec explains:<br/>
        /// "\csN:<br/>
        /// Designates character style with a style handle N. Like \sN, \csN is not a destination control word.<br/>
        /// However, it is important to treat it like one inside the style sheet; that is, \csN must be prefixed<br/>
        /// with \* and must appear as the first item inside a group. Doing so ensures that readers that do not<br/>
        /// understand character styles will skip the character style information correctly. When used in body<br/>
        /// text to indicate that a character style was applied, do not include the \* prefix."
        /// <para/>
        /// We don't really have a way to handle this table-side, so just call it a destination and give it a<br/>
        /// special-cased type where it just skips the control word always, even if it was a destination and<br/>
        /// would normally have caused its entire group to be skipped.
        /// <para/>
        /// You might think we could just elide these from the table entirely and accomplish the same thing,<br/>
        /// but there's one readme (Thief Trinity) where \*\csN is written in the middle of a group, rather<br/>
        /// than the start of a group as destinations are supposed to be written, causing us to skip its group<br/>
        /// and miss some text.
        /// <para/>
        /// The correct way to handle this would be to track whether we're at the start of a group when we hit<br/>
        /// one of these, but that's a bunch of crufty garbage so whatever, let's just stick with this, as it<br/>
        /// seems to work fine...
        /// </summary>
        CanBeDestOrNotDest,
        Skip,
        SkippableHex
    }

    private const int _propertiesLen = 4;
    public enum Property
    {
        Hidden,
        UnicodeCharSkipCount,
        FontNum,
        Lang
    }

    public enum SymbolFont
    {
        // Non-font values at the start, to avoid having to check the top bounds
        None,
        Unset,
        Symbol,
        Wingdings,
        Webdings
    }

    public enum RtfDestinationState
    {
        Normal,
        Skip
    }

    public enum RtfError
    {
        /// <summary>
        /// No error.
        /// </summary>
        OK,
        /// <summary>
        /// Unmatched '}'.
        /// </summary>
        StackUnderflow,
        /// <summary>
        /// Too many subgroups (we cap it at 100).
        /// </summary>
        StackOverflow,
        /// <summary>
        /// RTF ended during an open group.
        /// </summary>
        UnmatchedBrace,
        /// <summary>
        /// A symbol table entry was malformed. Possibly one of its enum values was out of range.
        /// </summary>
        InvalidSymbolTableEntry
    }

    #endregion

    // Static - otherwise the color table parser instantiates this huge thing every RTF readme in dark mode!
    // Also it's readonly so it's thread-safe anyway.
    public static readonly SymbolDict Symbols = new();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    // @RTF(in Context ctx): Test perf with in vs. without
    public static void HandleSpecialTypeFont(Context ctx, SpecialType specialType, int param)
    {
        switch (specialType)
        {
            case SpecialType.HeaderCodePage:
                ctx.Header.CodePage = param >= 0 ? param : 1252;
                break;
            case SpecialType.FontTable:
                ctx.GroupStack.CurrentInFontTable = true;
                break;
            case SpecialType.DefaultFont:
                if (!ctx.Header.DefaultFontSet)
                {
                    ctx.Header.DefaultFontNum = param;
                    ctx.Header.DefaultFontSet = true;
                }
                break;
            case SpecialType.Charset:
                // Reject negative codepage values as invalid and just use the header default in that case
                // (which is guaranteed not to be negative)
                if (ctx.FontEntries.Top != null && ctx.GroupStack.CurrentInFontTable)
                {
                    if (param is >= 0 and < _charSetToCodePageLength)
                    {
                        int codePage = _charSetToCodePage[param];
                        ctx.FontEntries.Top.CodePage = codePage >= 0 ? codePage : ctx.Header.CodePage;
                    }
                    else
                    {
                        ctx.FontEntries.Top.CodePage = ctx.Header.CodePage;
                    }
                }
                break;
            case SpecialType.CodePage:
                if (ctx.FontEntries.Top != null && ctx.GroupStack.CurrentInFontTable)
                {
                    ctx.FontEntries.Top.CodePage = param >= 0 ? param : ctx.Header.CodePage;
                }
                break;
        }
    }

    /// <summary>
    /// Specialized thing for branchless handling of optional spaces after rtf control words.
    /// Char values must be no higher than a byte (0-255) for the logic to work (perf).
    /// </summary>
    /// <param name="character"></param>
    /// <returns>-1 if <paramref name="character"/> is not equal to the ascii space character (0x20), otherwise, 0.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int MinusOneIfNotSpace_8Bits(char character)
    {
        int ret = character ^ ' ';
        // We only use 8 bits of a char's 16, so we can skip a couple shifts (tested)
        ret |= (ret >> 4);
        ret |= (ret >> 2);
        ret |= (ret >> 1);
        return -(ret & 1);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int BranchlessConditionalNegate(int value, int negate) => (value ^ -negate) + negate;
}
