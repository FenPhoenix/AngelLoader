using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static AL_Common.Common;

namespace AL_Common;

public static partial class RTFParserCommon
{
    // Perf: A readonly struct is required to retain full performance, and therefore we can only put readonly
    // things in here (no mutable value types like the unicode skip counter etc.)
    [StructLayout(LayoutKind.Auto)]
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
        private const int DefaultCapacity = 100;
        private int Capacity;

        private bool[] _skipDestinations;
        private bool[] _inFontTables;
        public byte[] SymbolFonts;
        public int[][] Properties;

        /// <summary>Do not modify!</summary>
        public int Count;

        internal GroupStack() => Init();

        [MemberNotNull(
            nameof(_skipDestinations),
            nameof(_inFontTables),
            nameof(SymbolFonts),
            nameof(Properties))]
        private void Init()
        {
            Count = 0;
            Capacity = DefaultCapacity;

            _skipDestinations = new bool[Capacity];
            _inFontTables = new bool[Capacity];
            SymbolFonts = new byte[Capacity];
            Properties = new int[Capacity][];

            for (int i = 0; i < Capacity; i++)
            {
                Properties[i] = new int[_propertiesLen];
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DeepCopyToNext()
        {
            // We don't really take a speed hit from this at all, but we support files with a stupid amount of
            // nested groups now.
            if (Count >= Capacity - 1)
            {
                int oldMaxGroups = Capacity;

                int newCapacity;
                checked
                {
                    try
                    {
                        // Don't let it go all the way up to array max. 262,144 is absurdly high but not so high
                        // as to take too much memory.
                        newCapacity = (Capacity * 2).Clamp(0, ByteSize.KB * 256);
                    }
                    catch (OverflowException)
                    {
                        newCapacity = ByteSize.KB * 256;
                    }
                }

                Capacity = newCapacity;
                Array.Resize(ref _skipDestinations, Capacity);
                Array.Resize(ref _inFontTables, Capacity);
                Array.Resize(ref SymbolFonts, Capacity);
                Array.Resize(ref Properties, Capacity);

                for (int i = oldMaxGroups; i < Capacity; i++)
                {
                    Properties[i] = new int[_propertiesLen];
                }
            }

            _skipDestinations[Count + 1] = _skipDestinations[Count];
            _inFontTables[Count + 1] = _inFontTables[Count];
            SymbolFonts[Count + 1] = SymbolFonts[Count];
            for (int i = 0; i < _propertiesLen; i++)
            {
                Properties[Count + 1][i] = Properties[Count][i];
            }
            ++Count;
        }

        #region Current group

        public bool CurrentSkipDest
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _skipDestinations[Count];
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => _skipDestinations[Count] = value;
        }

        public bool CurrentInFontTable
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _inFontTables[Count];
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => _inFontTables[Count] = value;
        }

        public SymbolFont CurrentSymbolFont
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (SymbolFont)SymbolFonts[Count];
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => SymbolFonts[Count] = (byte)value;
        }

        public int[] CurrentProperties
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Properties[Count];
        }

        // Current group always begins at group 0, so reset just that one
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ResetFirst()
        {
            _skipDestinations[0] = false;
            _inFontTables[0] = false;
            SymbolFonts[0] = (int)SymbolFont.None;

            Properties[0][(int)Property.Hidden] = 0;
            Properties[0][(int)Property.UnicodeCharSkipCount] = 1;
            Properties[0][(int)Property.FontNum] = NoFontNumber;
            Properties[0][(int)Property.Lang] = -1;
        }

        #endregion

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void ClearFast() => Count = 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void ResetCapacityIfTooHigh()
        {
            if (Capacity > DefaultCapacity)
            {
                Init();
            }
        }
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
        public readonly ushort Index;

        public Symbol(string keyword, int defaultParam, bool useDefaultParam, KeywordType keywordType, ushort index)
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
        /* Command-line: 'C:\\gperf\\tools\\gperf.exe' --output-file='C:\\_al_rtf_table_gen\\gperfOutputFile.txt' -t 'C:\\_al_rtf_table_gen\\gperfFormatFile.txt'  */
        /* Computed positions: -k'1-3,$' */

        //private const int TOTAL_KEYWORDS = 79;
        //private const int MIN_WORD_LENGTH = 1;
        private const int MAX_WORD_LENGTH = 18;
        //private const int MIN_HASH_VALUE = 11;
        private const int MAX_HASH_VALUE = 263;
        /* maximum key range = 253, duplicates = 0 */

        private readonly ushort[] asso_values =
        {
            264, 264, 264, 264, 264, 264, 264, 264, 264, 264,
            264, 264, 264, 264, 264, 264, 264, 264, 264, 264,
            264, 264, 264, 264, 264, 264, 264, 264, 264, 264,
            264, 264, 264, 264, 264, 264, 264, 264, 264, 264,
            264, 264, 264, 264, 264, 264, 264, 264, 264, 264,
            264, 264, 264, 264, 264, 264, 264, 264, 264, 264,
            264, 264, 264, 264, 264, 264, 264, 264, 264, 264,
            264, 264, 264, 264, 264, 264, 264, 264, 264, 264,
            264, 264, 264, 264, 264, 264, 264, 264, 264, 264,
            264, 264, 264, 264, 264, 264, 264, 10, 45, 10,
            75, 10, 5, 95, 40, 5, 95, 0, 0, 35,
            15, 0, 25, 15, 50, 15, 0, 70, 10, 105,
            125, 0, 264, 264, 264, 264, 264, 264, 264, 264,
            264, 264, 264, 264, 264, 264, 264, 264, 264, 264,
            264, 264, 264, 264, 264, 264, 264, 264, 264, 264,
            264, 264, 264, 264, 264, 264, 264, 264, 264, 264,
            264, 264, 264, 264, 264, 264, 264, 264, 264, 264,
            264, 264, 264, 264, 264, 264, 264, 264, 264, 264,
            264, 264, 264, 264, 264, 264, 264, 264, 264, 264,
            264, 264, 264, 264, 264, 264, 264, 264, 264, 264,
            264, 264, 264, 264, 264, 264, 264, 264, 264, 264,
            264, 264, 264, 264, 264, 264, 264, 264, 264, 264,
            264, 264, 264, 264, 264, 264, 264, 264, 264, 264,
            264, 264, 264, 264, 264, 264, 264, 264, 264, 264,
            264, 264, 264, 264, 264, 264, 264, 264, 264, 264,
            264, 264, 264, 264, 264, 264
        };

        /*
        For "emspace", "enspace", "qmspace", "~", "emdash", "endash", "lquote", "rquote", "ldblquote", "rdblquote"
        Convert these to ascii equivalents because for our use case we don't really want weird versions of
        spaces and dashes etc.
        Original (proper) values:
        emspace   '\x2003'
        enspace   '\x2002'
        qmspace   '\x2005'
        ~         '\xa0'
        emdash    '\x2014'
        endash    '\x2013'
        lquote    '\x2018'
        rquote    '\x2019'
        ldblquote '\x201C'
        rdblquote '\x201D'

        For "cs", "ds", "ts"
        Hack to make sure we extract the \fldrslt text from Thief Trinity in that one place.

        For "listtext", "pntext"
        Ignore list item bullets and numeric prefixes etc. We don't need them.

        For "v"
        v to make all plain text hidden (not output to the conversion stream), \v0 to make it shown again

        For "ansi"
        The spec calls this "ANSI (the default)" but says nothing about what codepage that actually means.
        "ANSI" is often misused to mean one of the Windows codepages, so I'll assume it's Windows-1252.

        For "mac"
        The spec calls this "Apple Macintosh" but again says nothing about what codepage that is. I'll
        assume 10000 ("Mac Roman")

        For "fldinst"
        We need to do stuff with this (SYMBOL instruction)

        NOTE: This is generated. Values can be modified, but not keys (keys are the first string params).
        Also no reordering. Adding, removing, reordering, or modifying keys requires generating a new version.
        See RTF_SymbolListGenSource.cs for how to generate a new version (it also contains the original
        Symbol list which must be used as the source to generate this one).
        */
        private readonly Symbol?[] _symbolTable =
        {
            null, null, null, null, null, null, null, null, null,
            null, null,
// Entry 7
            new Symbol("f", 0, false, KeywordType.Property, (ushort)Property.FontNum),
// Entry 43
            new Symbol("footerl", 0, false, KeywordType.Destination, (ushort)DestinationType.Skip),
            null, null, null, null,
// Entry 42
            new Symbol("footerf", 0, false, KeywordType.Destination, (ushort)DestinationType.Skip),
// Entry 37
            new Symbol("colortbl", 0, false, KeywordType.Special, (ushort)SpecialType.ColorTable),
            null,
// Entry 63
            new Symbol("title", 0, false, KeywordType.Destination, (ushort)DestinationType.Skip),
// Entry 13
            new Symbol("v", 1, false, KeywordType.Property, (ushort)Property.Hidden),
// Entry 62
            new Symbol("tc", 0, false, KeywordType.Destination, (ushort)DestinationType.Skip),
// Entry 45
            new Symbol("footnote", 0, false, KeywordType.Destination, (ushort)DestinationType.Skip),
// Entry 76
            new Symbol("cell", 0, false, KeywordType.Character, ' '),
// Entry 60
            new Symbol("stylesheet", 0, false, KeywordType.Destination, (ushort)DestinationType.Skip),
            null,
// Entry 6
            new Symbol("fonttbl", 0, false, KeywordType.Special, (ushort)SpecialType.FontTable),
// Entry 33
            new Symbol("listtext", 0, false, KeywordType.Destination, (ushort)DestinationType.Skip),
// Entry 53
            new Symbol("info", 0, false, KeywordType.Destination, (ushort)DestinationType.Skip),
            null, null,
// Entry 32
            new Symbol("ts", 0, false, KeywordType.Destination, (ushort)DestinationType.CanBeDestOrNotDest),
// Entry 54
            new Symbol("keywords", 0, false, KeywordType.Destination, (ushort)DestinationType.Skip),
// Entry 15
            new Symbol("line", 0, false, KeywordType.Character, '\n'),
            null, null,
// Entry 48
            new Symbol("ftnsepc", 0, false, KeywordType.Destination, (ushort)DestinationType.Skip),
            null,
// Entry 16
            new Symbol("sect", 0, false, KeywordType.Character, '\n'),
// Entry 46
            new Symbol("ftncn", 0, false, KeywordType.Destination, (ushort)DestinationType.Skip),
            null,
// Entry 30
            new Symbol("cs", 0, false, KeywordType.Destination, (ushort)DestinationType.CanBeDestOrNotDest),
            null,
// Entry 66
            new Symbol("pict", 1, false, KeywordType.Destination, (ushort)DestinationType.SkippableHex),
            null,
// Entry 34
            new Symbol("pntext", 0, false, KeywordType.Destination, (ushort)DestinationType.Skip),
// Entry 1
            new Symbol("pc", 437, true, KeywordType.Special, (ushort)SpecialType.HeaderCodePage),
// Entry 78
            new Symbol("nestcell", 0, false, KeywordType.Character, ' '),
// Entry 0
            new Symbol("ansi", 1252, true, KeywordType.Special, (ushort)SpecialType.HeaderCodePage),
            null,
// Entry 47
            new Symbol("ftnsep", 0, false, KeywordType.Destination, (ushort)DestinationType.Skip),
// Entry 38
            new Symbol("comment", 0, false, KeywordType.Destination, (ushort)DestinationType.Skip),
            null, null, null, null,
// Entry 20
            new Symbol("enspace", 0, false, KeywordType.Character, ' '),
// Entry 3
            new Symbol("pca", 850, true, KeywordType.Special, (ushort)SpecialType.HeaderCodePage),
            null, null,
// Entry 41
            new Symbol("footer", 0, false, KeywordType.Destination, (ushort)DestinationType.Skip),
// Entry 44
            new Symbol("footerr", 0, false, KeywordType.Destination, (ushort)DestinationType.Skip),
// Entry 8
            new Symbol("fcharset", -1, false, KeywordType.Special, (ushort)SpecialType.Charset),
            null, null,
// Entry 74
            new Symbol("panose", 20, true, KeywordType.Special, (ushort)SpecialType.SkipNumberOfBytes),
// Entry 51
            new Symbol("headerl", 0, false, KeywordType.Destination, (ushort)DestinationType.Skip),
// Entry 2
            new Symbol("mac", 10000, true, KeywordType.Special, (ushort)SpecialType.HeaderCodePage),
// Entry 67
            new Symbol("themedata", 0, false, KeywordType.Destination, (ushort)DestinationType.SkippableHex),
            null, null,
// Entry 50
            new Symbol("headerf", 0, false, KeywordType.Destination, (ushort)DestinationType.Skip),
            null, null, null, null,
// Entry 19
            new Symbol("emspace", 0, false, KeywordType.Character, ' '),
            null, null, null, null,
// Entry 21
            new Symbol("qmspace", 0, false, KeywordType.Character, ' '),
// Entry 28
            new Symbol("bin", 0, false, KeywordType.Special, (ushort)SpecialType.SkipNumberOfBytes),
            null, null, null,
// Entry 29
            new Symbol("fldinst", 0, false, KeywordType.Destination, (ushort)DestinationType.FieldInstruction),
            null, null, null, null,
// Entry 11
            new Symbol("uc", 1, false, KeywordType.Property, (ushort)Property.UnicodeCharSkipCount),
// Entry 55
            new Symbol("operator", 0, false, KeywordType.Destination, (ushort)DestinationType.Skip),
            null, null, null,
// Entry 57
            new Symbol("private", 0, false, KeywordType.Destination, (ushort)DestinationType.Skip),
            null,
// Entry 5
            new Symbol("deff", 0, false, KeywordType.Special, (ushort)SpecialType.DefaultFont),
            null,
// Entry 24
            new Symbol("lquote", 0, false, KeywordType.Character, '\''),
// Entry 69
            new Symbol("passwordhash", 0, false, KeywordType.Destination, (ushort)DestinationType.SkippableHex),
// Entry 17
            new Symbol("tab", 0, false, KeywordType.Character, '\t'),
// Entry 70
            new Symbol("datastore", 0, false, KeywordType.Destination, (ushort)DestinationType.SkippableHex),
            null, null,
// Entry 31
            new Symbol("ds", 0, false, KeywordType.Destination, (ushort)DestinationType.CanBeDestOrNotDest),
            null, null, null,
// Entry 58
            new Symbol("revtim", 0, false, KeywordType.Destination, (ushort)DestinationType.Skip),
// Entry 39
            new Symbol("creatim", 0, false, KeywordType.Destination, (ushort)DestinationType.Skip),
            null, null, null,
// Entry 49
            new Symbol("header", 0, false, KeywordType.Destination, (ushort)DestinationType.Skip),
// Entry 52
            new Symbol("headerr", 0, false, KeywordType.Destination, (ushort)DestinationType.Skip),
            null, null, null,
// Entry 18
            new Symbol("bullet", 0, false, KeywordType.Character, '\x2022'),
// Entry 56
            new Symbol("printim", 0, false, KeywordType.Destination, (ushort)DestinationType.Skip),
// Entry 68
            new Symbol("colorschememapping", 0, false, KeywordType.Destination, (ushort)DestinationType.SkippableHex),
// Entry 10
            new Symbol("lang", 0, false, KeywordType.Property, (ushort)Property.Lang),
            null, null,
// Entry 40
            new Symbol("doccomm", 0, false, KeywordType.Destination, (ushort)DestinationType.Skip),
            null, null, null, null,
// Entry 73
            new Symbol("blipuid", 32, true, KeywordType.Special, (ushort)SpecialType.SkipNumberOfBytes),
            null, null, null,
// Entry 35
            new Symbol("author", 0, false, KeywordType.Destination, (ushort)DestinationType.Skip),
// Entry 61
            new Symbol("subject", 0, false, KeywordType.Destination, (ushort)DestinationType.Skip),
// Entry 14
            new Symbol("par", 0, false, KeywordType.Character, '\n'),
// Entry 26
            new Symbol("ldblquote", 0, false, KeywordType.Character, '"'),
            null,
// Entry 12
            new Symbol("u", 0, false, KeywordType.Special, (ushort)SpecialType.UnicodeChar),
// Entry 4
            new Symbol("ansicpg", 1252, false, KeywordType.Special, (ushort)SpecialType.HeaderCodePage),
            null, null, null,
// Entry 23
            new Symbol("endash", 0, false, KeywordType.Character, '-'),
// Entry 65
            new Symbol("xe", 0, false, KeywordType.Destination, (ushort)DestinationType.Skip),
// Entry 64
            new Symbol("txe", 0, false, KeywordType.Destination, (ushort)DestinationType.Skip),
            null, null,
// Entry 25
            new Symbol("rquote", 0, false, KeywordType.Character, '\''),
// Entry 77
            new Symbol("nestrow", 0, false, KeywordType.Character, '\n'),
            null, null, null, null,
// Entry 72
            new Symbol("objdata", 1, false, KeywordType.Destination, (ushort)DestinationType.SkippableHex),
            null, null, null, null, null, null, null, null,
// Entry 22
            new Symbol("emdash", 0, false, KeywordType.Character, '-'),
            null, null,
// Entry 71
            new Symbol("datafield", 0, false, KeywordType.Destination, (ushort)DestinationType.SkippableHex),
            null, null, null, null, null, null, null, null, null,
            null, null,
// Entry 36
            new Symbol("buptim", 0, false, KeywordType.Destination, (ushort)DestinationType.Skip),
            null, null, null, null, null, null, null,
// Entry 27
            new Symbol("rdblquote", 0, false, KeywordType.Character, '"'),
            null, null, null, null, null, null, null, null,
// Entry 59
            new Symbol("rxe", 0, false, KeywordType.Destination, (ushort)DestinationType.Skip),
            null, null, null, null, null, null, null, null, null,
            null, null, null, null, null, null, null, null, null,
            null, null, null, null, null, null, null, null, null,
            null, null,
// Entry 9
            new Symbol("cpg", -1, false, KeywordType.Special, (ushort)SpecialType.CodePage),
            null, null, null, null, null, null, null, null, null,
            null, null, null, null, null, null, null, null, null,
            null, null, null, null, null, null, null, null, null,
            null, null, null, null, null, null, null,
// Entry 75
            new Symbol("row", 0, false, KeywordType.Character, '\n'),
        };

        private static Symbol?[] InitControlSymbolArray()
        {
            Symbol?[] ret = new Symbol?[256];
            ret['\''] = new Symbol("'", 0, false, KeywordType.Special, (int)SpecialType.HexEncodedChar);
            /*
            @RTF(KeywordType.Character and symbol fonts):
            \, {, and } are the only KeywordType.Character chars that can be in a symbol font. Everything else is
            either below 0x20 or more than one byte, which in either case means they can't be symbol font chars.
            ~ is nominally a non-breaking space, and in RichEdit is displayed as such (or at least whitespace of
            some kind), but in LibreOffice is displayed as a square dot when set to Wingdings (as expected).
            Since RichEdit doesn't treat it as a symbol font character we should in theory match its behavior,
            but we convert it to an ASCII space anyway so the whole thing is moot currently. But just in case we
            decide to change it, there's the info.

            We could maybe figure out a way to not have to do the symbol font check/conversion in the common case
            where we don't need to, is the point of this whole soliloquy.
            */
            ret['\\'] = new Symbol("\\", 0, false, KeywordType.Character, '\\');
            ret['{'] = new Symbol("{", 0, false, KeywordType.Character, '{');
            ret['}'] = new Symbol("}", 0, false, KeywordType.Character, '}');
            // Nominally Non-Breaking Space (0xA0)
            ret['~'] = new Symbol("~", 0, false, KeywordType.Character, ' ');
            // Nominally Non-Breaking Hyphen (0x2011)
            ret['_'] = new Symbol("_", 0, false, KeywordType.Character, '-');
            // There's also \- which is Optional Hyphen (the scanner is only producing single-line values, so no
            // need for this), and \: which "specifies a subentry in an index entry" (it's not clear even from
            // the spec what exactly an "index entry" is).
            return ret;
        }

        private readonly Symbol?[] ControlSymbols = InitControlSymbolArray();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Symbol? LookUpControlSymbol(char ch) => ControlSymbols[ch];

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
        public int CodePage = -1;

        // We need to store names in case we get codepage 42 nonsense, we need to know which font to translate
        // to Unicode (Wingdings, Webdings, or Symbol)
        public SymbolFont SymbolFont = SymbolFont.Unset;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Reset()
        {
            CodePage = -1;
            SymbolFont = SymbolFont.Unset;
        }
    }

    public sealed class FontDictionary
    {
        private int _capacity;
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
        private const int _switchPoint = 32768;

        public FontEntry? Top;

        private int _highestKey;

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
            if (key > _highestKey) _highestKey = key;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            Top = null;
            _dict?.Reset();
            /*
            Clear only the required portion of the array to shave some time off.

            We can elide the clear and that works correctly as long as we don't have \f keywords that reference
            undefined font numbers, but if we did then it would index into the array and possibly get a font
            entry when it should have gotten null. That's unlikely, but we should probably not just ignore it.

            Aside from eliding the clear and reducing edge-case safety, this up-to-highest-key clear is probably
            about the best we can do.
            */

            // If the dictionary is not null, that means we had an out-of-array-range key, so all bets are off.
            // Just clear the entire thing in that case.
            if (_dict == null && (_highestKey is > 0 and < _switchPoint))
            {
                Array.Clear(_array, 0, _highestKey + 1);
            }
            else
            {
                _array.Clear();
            }
            _highestKey = 0;
            _fontEntryPoolVirtualCount = _fontEntryPool.Count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ClearFull(int capacity)
        {
            _capacity = capacity;
            _highestKey = 0;
            _fontEntryPool.HardReset(capacity);
            _dict = null;
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

    public enum SpecialType : byte
    {
        HeaderCodePage,
        DefaultFont,
        FontTable,
        Charset,
        CodePage,
        UnicodeChar,
        HexEncodedChar,
        SkipNumberOfBytes,
        ColorTable
    }

    public enum KeywordType : byte
    {
        Character,
        Property,
        Destination,
        Special
    }

    public enum DestinationType : byte
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
        /// Despite the fact that the \* prefixed versions are only supposed to appear at the start of a group,<br/>
        /// there's one readme (Thief Trinity) where \*\csN is written in the middle of a group. If we treated<br/>
        /// the \* prefixed version as being a skip-group trigger, then we would skip the rest of the group if it<br/>
        /// was in the middle of one, missing whatever text was after it.
        /// <para/>
        /// However, we actually don't have to treat any version of the word as a skip-group trigger, because the<br/>
        /// only time we want that is when they're in the \stylesheet group, which is already being skipped. So<br/>
        /// ignoring the word is a no-op in \stylesheet, and also a no-op in a regular group, which is what we want<br/>
        /// in both cases.
        /// </summary>
        CanBeDestOrNotDest,
        Skip,
        SkippableHex
    }

    private const int _propertiesLen = 4;
    public enum Property : byte
    {
        Hidden,
        UnicodeCharSkipCount,
        FontNum,
        Lang
    }

    public enum SymbolFont : byte
    {
        // Non-font values at the start, to avoid having to check the top bounds
        None,
        Unset,
        Symbol,
        Wingdings,
        Webdings
    }

    public enum RtfError : byte
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
        /// There were over <inheritdoc cref="GroupStack.MaxGroups" path="//summary"/> nested groups.
        /// </summary>
        StackOverflow,
        /// <summary>
        /// RTF ended during an open group.
        /// </summary>
        UnmatchedBrace,
        /// <summary>
        /// The rtf is malformed in such a way that it might be unsafe to continue parsing it (infinite loops, stack overflows, etc.)
        /// </summary>
        AbortedForSafety
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
        // We only use 8 bits of a char's 16
        const int bits = 8;
        // 7 instructions on Framework x86
        // 8 instructions on Framework x64
        // 7 instructions on .NET 8 x64
        return ((character - ' ') | (' ' - character)) >> (bits - 1);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int BranchlessConditionalNegate(int value, int negate) => (value ^ -negate) + negate;
}
