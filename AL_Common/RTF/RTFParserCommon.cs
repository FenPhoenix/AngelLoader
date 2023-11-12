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

    public enum InsertItemKind
    {
        Lang,
        ForeColorReset
    }

    public sealed class UIntParamInsertItem
    {
        private static int GetParamLength(uint number) =>
            number <= 9 ? 1 :
            number <= 99 ? 2 :
            number <= 999 ? 3 :
            number <= 9999 ? 4 :
            number <= 99999 ? 5 :
            number <= 999999 ? 6 :
            number <= 9999999 ? 7 :
            number <= 99999999 ? 8 :
            number <= 999999999 ? 9 :
            10;

        public readonly InsertItemKind Kind;
        public int Index;
        public readonly uint Param;
        public readonly int ParamLength;


        public UIntParamInsertItem(int index, uint param, InsertItemKind kind)
        {
            Index = index;
            Param = param;
            Kind = kind;
            ParamLength = GetParamLength(param);
        }
    }

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
        None,
        Symbol,
        Wingdings,
        Webdings,
        Unset
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
