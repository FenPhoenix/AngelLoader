// @DarkModeNote(RtfColorTableParser):
// We use a full parser here because rather than simply replacing all byte sequences with another, here we
// need to parse and return the first and ONLY the first {\colortbl} group. In theory that could be in a
// comment or invalidly in the middle of another group or something. I mean it won't, let's be honest, but
// the color table is important enough to take the perf hit and the small amount of code duplication.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using AL_Common.CommunityToolkit;
using JetBrains.Annotations;
using static AL_Common.RTF.RtfCommon;

namespace AL_Common.RTF;

[StructLayout(LayoutKind.Sequential)]
public readonly struct CodePageItem
{
    public readonly int Index;
    public readonly byte[] CodePageBytes;

    public CodePageItem(int index, byte[] codePageBytes)
    {
        Index = index;
        CodePageBytes = codePageBytes;
    }
}

public sealed partial class RtfDisplayedReadmeParser
{
    #region Private fields

    private readonly struct FontNameData
    {
        internal readonly byte[] Bytes;
        internal readonly ushort CodePage;
        internal readonly byte[] CodePageInsertBytes;

        internal FontNameData(byte[] bytes, ushort codePage, byte[] codePageInsertBytes)
        {
            Bytes = bytes;
            CodePage = codePage;
            CodePageInsertBytes = codePageInsertBytes;
        }
    }

    // All code pages are 4 in length plus 1 for the keyword-ending space, and since these are old-style RTF and
    // the RTF spec hasn't been updated since 2008 in any case, it's basically impossible that this ever changes.
    // So let's be efficient and use a constant: 4 for the keyword, 4 for the param, 1 for the space = 9.
    public const int FontNameSuffixCodePageLength = 9;

    private static readonly FontNameData[] FontNameSuffixes =
    [
        // IMPORTANT: Spaces at the end serve as the keyword-ending spaces for safety. Do not remove!
        new FontNameData(" Baltic"u8.ToArray(), 1257, @"\cpg1257 "u8.ToArray()),
        new FontNameData(" CE"u8.ToArray(), 1250, @"\cpg1250 "u8.ToArray()),
        new FontNameData(" Cyr"u8.ToArray(), 1251, @"\cpg1251 "u8.ToArray()),
        new FontNameData(" Greek"u8.ToArray(), 1253, @"\cpg1253 "u8.ToArray()),
        new FontNameData(" Tur"u8.ToArray(), 1254, @"\cpg1254 "u8.ToArray()),
        new FontNameData(" (Hebrew)"u8.ToArray(), 1255, @"\cpg1255 "u8.ToArray()),
        new FontNameData(" (Arabic)"u8.ToArray(), 1256, @"\cpg1256 "u8.ToArray()),
        new FontNameData(" (Vietnamese)"u8.ToArray(), 1258, @"\cpg1258 "u8.ToArray()),
    ];

    private bool _parsedFontTable;

    private List<RtfColor>? _colorTable;
    private bool _parsedColorTable;
    private bool _getColorTable;
    private List<CodePageItem>? _codePageItems;

    private byte[] _rtfBytes = Array.Empty<byte>();
    private int _rtfBytesLength;

    // +1 to allow reading one beyond the max and then checking for it to return an error
    private readonly byte[] _keyword = new byte[KeywordMaxLen + 1];

    #region Resettables

    #region Header

    private ushort _headerCodePage;
    private bool _headerDefaultFontSet;
    private int _headerDefaultFontNum;

    #endregion

    /*
    \fN params are normally in the signed int16 range, but the Windows RichEdit control supports them in the
    -30064771071 - 30064771070 (-0x6ffffffff - 0x6fffffffe) range (yes, bizarre numbers, but I tested and there
    they are). So let's just make them int32.
    */

    private Dictionary<int, FontEntry> _fontDictionary;

    private bool _skipDestinationIfUnknown;

    private int _currentPos;

    private bool _inHandleSkippableHexData;

    private bool _inFontTable;

    #endregion

    #endregion

    #region Public API

    /// <summary>
    /// Initializes a new instance of the <see cref="RtfDisplayedReadmeParser"/> class.
    /// </summary>
    public RtfDisplayedReadmeParser()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        _fontDictionary = new Dictionary<int, FontEntry>(FontTableDefaultCapacity);

        InitGroupStack();
    }

    [PublicAPI]
    public (bool Success, List<RtfColor>? ColorTable, List<CodePageItem>? CodePageItems)
    GetData(in byte[] rtfBytes, bool getColorTable)
    {
        try
        {
            _rtfBytes = rtfBytes;
            _rtfBytesLength = rtfBytes.Length;

            #region Reset

            GroupStack_Reset();
            _fontDictionary.Clear();

            _headerCodePage = 0;
            _headerDefaultFontSet = false;
            _headerDefaultFontNum = 0;

            _fontNameBuffer_Count = 0;

            _skipDestinationIfUnknown = false;

            _currentPos = 0;

            _inHandleSkippableHexData = false;
            _inFontTable = false;

            _parsedFontTable = false;
            _parsedColorTable = false;
            _getColorTable = getColorTable;

            _colorTable = null;
            _codePageItems = null;

            #endregion

            RtfError error = ParseRtf();
            if (error == RtfError.OK)
            {
                return (true, ColorTable: _colorTable, CodePageItems: _codePageItems);
            }
            else
            {
                return (false, ColorTable: _colorTable, CodePageItems: _codePageItems);
            }
        }
        catch
        {
            return (false, _colorTable, _codePageItems);
        }
        finally
        {
            // Reset after so we don't carry around any waste after running
            _colorTable = null;
            _codePageItems = null;
            GroupStack_ResetCapacityToDefault();
            if (_fontDictionary.Count > FontTableDefaultCapacity)
            {
                _fontDictionary = new Dictionary<int, FontEntry>(FontTableDefaultCapacity);
            }
            FontNameBuffer_ResetCapacityToDefault();

            _rtfBytes = Array.Empty<byte>();
            _rtfBytesLength = 0;
        }
    }

    #endregion

    #region Parse

    private RtfError ParseRtf()
    {
        // Avoid bounds checks by passing a buffer reference everywhere. We do our own bounds checking.
        ref byte bufferRef = ref GetArrayDataReference(_rtfBytes);
        ref bool isNonPlainTextCharRef = ref GetArrayDataReference(IsNonPlainText);
        ref bool isIgnoreCharRef = ref GetArrayDataReference(IsIgnoreChar);

        while (_currentPos < _rtfBytesLength)
        {
            char ch = (char)GetByteAtCurrentPosAndIncrement(ref bufferRef);

            // Ordered by most frequently appearing first
            switch (ch)
            {
                case '\\':
                    RtfError ec = ParseKeyword(ref bufferRef);
                    if (ec != RtfError.OK) return ec;
                    break;
                case '{':
                    GroupStack_DeepCopyToNext();
                    break;
                case '}':
                    if (_groupStackTopIndex == 0) return RtfError.StackUnderflow;
                    --_groupStackTopIndex;
                    if (_groupStackTopIndex == 0) return RtfError.OK;
                    break;
                // Although we don't convert plain text, we still want to know about it so we can efficiently
                // skip it.
                default:
                {
                    if (!Unsafe.AddByteOffset(ref isIgnoreCharRef, (nint)ch) &&
                        !GroupStack_CurrentSkipDest)
                    {
                        // No measurable perf loss from this, and it lets us avoid duplicating the loop body.
                        char currentChar = (char)(_currentPos < _rtfBytesLength
                            ? GetByteAtPos(ref bufferRef, _currentPos)
                            : GetByte(_currentPos));

                        if (!Unsafe.AddByteOffset(ref isNonPlainTextCharRef, (nint)currentChar))
                        {
                            HandlePlainTextRun(ref bufferRef);
                        }
                    }
                    break;
                }
            }
        }

        return _groupStackTopIndex > 0 ? RtfError.UnmatchedBrace : RtfError.OK;
    }

    private void HandlePlainTextRun(ref byte bufferRef)
    {
        _currentPos--;

        if (System.Numerics.Vector.IsHardwareAccelerated)
        {
            bool finishedOnNonPlainTextChar = SIMD_SkipPlainText(ref bufferRef);
            if (finishedOnNonPlainTextChar)
            {
                return;
            }
        }

        while (_currentPos < _rtfBytesLength)
        {
            char ch = (char)GetByteAtCurrentPosAndIncrement(ref bufferRef);
            if (IsNonPlainText[(byte)ch])
            {
                _currentPos--;
                return;
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private RtfError ParseKeyword(ref byte bufferRef)
    {
        // The keyword parsers are JIT inlined now, so make sure to have only one call to each!
        if (_currentPos < _rtfBytesLength - KeywordParseMaxRequiredBytes)
        {
            return ParseKeyword_Fast(ref bufferRef);
        }
        else
        {
            return ParseKeyword_Slow(ref bufferRef);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private RtfError ParseKeyword_FontTable(ref byte bufferRef, out KeywordType fontTableKeyword, out int param)
    {
        if (_currentPos < _rtfBytesLength - KeywordParseMaxRequiredBytes)
        {
            return ParseKeyword_FontTable_Fast(ref bufferRef, out fontTableKeyword, out param);
        }
        else
        {
            return ParseKeyword_FontTable_Slow(ref bufferRef, out fontTableKeyword, out param);
        }
    }

    private RtfError ParseFontTable(ref byte bufferRef)
    {
        // Prevent stack overflow from maliciously-crafted rtf files - we should never recurse back into here in
        // a spec-conforming file.
        if (_inFontTable) return RtfError.AbortedForSafety;
        _inFontTable = true;

        int fontTableGroupLevel = _groupStackTopIndex;

        int currentFontNumber = NoFontNumber;
        ushort currentFontCodePage = NoCodePage;
        int lastCodePageIndex = -1;

        while (_currentPos < _rtfBytesLength)
        {
            char ch = (char)GetByteAtCurrentPosAndIncrement(ref bufferRef);

            switch (ch)
            {
                case '{':
                    GroupStack_DeepCopyToNext();
                    break;
                case '}':
                    if (_groupStackTopIndex == 0) return RtfError.StackUnderflow;
                    --_groupStackTopIndex;
                    if (_groupStackTopIndex < fontTableGroupLevel)
                    {
                        // We can't actually set the symbol font as soon as we see \deffN, because we won't
                        // have any font entry objects yet. Now that we do, we can retroactively set all
                        // previous groups' fonts as appropriate, as if they had propagated up automatically.
                        int defaultFontNum = _headerDefaultFontNum;
                        /*
                        Start at 1 because the "base" group is still inside an opening { so it's really
                        group 1.
                        NOTE: The <= is correct. It's an index, not a length.
                        */
                        for (int i = 1; i <= _groupStackTopIndex; i++)
                        {
                            if (_groupStackFrames[i].PropFontNum == NoFontNumber)
                            {
                                _groupStackFrames[i].PropFontNum = defaultFontNum;
                            }
                            else
                            {
                                break;
                            }
                        }

                        _inFontTable = false;
                        return RtfError.OK;
                    }
                    break;
                case '\\':
                    RtfError error = ParseKeyword_FontTable(
                        ref bufferRef,
                        out KeywordType fontTableKeyword,
                        out int param);
                    if (error != RtfError.OK) return error;

                    if (fontTableKeyword == KeywordType.F)
                    {
                        currentFontNumber = param;
                    }
                    else if (currentFontNumber > NoFontNumber)
                    {
                        switch (fontTableKeyword)
                        {
                            case KeywordType.FCharset:
                            {
                                currentFontCodePage = param.IsBetween(0, CharSetToCodePageLength - 1)
                                    ? CharSetToCodePage[param]
                                    : _headerCodePage;
                                lastCodePageIndex = _currentPos;
                                break;
                            }
                            case KeywordType.CPG:
                                currentFontCodePage = IsNonEmptyUShortParam(param)
                                    ? (ushort)param
                                    : _headerCodePage;
                                lastCodePageIndex = _currentPos;
                                break;
                        }
                    }
                    break;
                case '\r':
                case '\n':
                    break;
                default:
                {
                    if (!GroupStack_CurrentSkipDest &&
                        // We can't check for codepage 42, because symbol fonts can have other codepages
                        // (although that may be a quirk/bug or whatever, but it can happen). Too bad,
                        // otherwise we could save time here...
                        currentFontNumber > NoFontNumber)
                    {
                        FontNameData fontNameData = GetFontNameData_Scalar(ref bufferRef, ch);

                        ushort codePageOverride = fontNameData.CodePage;

                        if (codePageOverride != NoCodePage)
                        {
                            currentFontCodePage = codePageOverride;
                            if (lastCodePageIndex > -1)
                            {
                                _codePageItems ??= new List<CodePageItem>();
                                _codePageItems.Add(new CodePageItem(lastCodePageIndex, fontNameData.CodePageInsertBytes));
                            }
                        }
                        else if (currentFontCodePage == NoCodePage)
                        {
                            currentFontCodePage = _headerCodePage;
                        }

                        _fontDictionary[currentFontNumber] = new FontEntry(currentFontCodePage, SymbolFont.None);
                        currentFontNumber = NoFontNumber;
                        currentFontCodePage = NoCodePage;
                        lastCodePageIndex = -1;
                    }
                    break;
                }
            }
        }

        _inFontTable = false;
        return RtfError.OK;
    }

    private FontNameData GetFontNameData_Scalar(ref byte bufferRef, char ch, int symbolFontNameCountStart = 0)
    {
        int symbolFontNameCount;
        _fontNameBuffer_Count = 0;
        bool isNonSemicolonSeparatorChar = false;
        if (_currentPos < _rtfBytesLength - (MaxSymbolFontNameLength + 1))
        {
            ref byte fontNameBufferRef = ref FontNameBuffer_EnsureCapacityAndGetRef(MaxSymbolFontNameLength + 1);

            for (symbolFontNameCount = symbolFontNameCountStart;
                 symbolFontNameCount < MaxSymbolFontNameLength &&
                 ch != ';' &&
                 !(isNonSemicolonSeparatorChar = IsNonPlainText[(byte)ch]);
                 symbolFontNameCount++, ch = (char)GetByteAtCurrentPosAndIncrement(ref bufferRef))
            {
                Unsafe.Add(ref fontNameBufferRef, (nint)_fontNameBuffer_Count) = (byte)ch;
                _fontNameBuffer_Count += 1;
            }
        }
        else
        {
            for (symbolFontNameCount = symbolFontNameCountStart;
                 symbolFontNameCount < MaxSymbolFontNameLength &&
                 ch != ';' &&
                 !(isNonSemicolonSeparatorChar = IsNonPlainText[(byte)ch]);
                 symbolFontNameCount++, ch = (char)GetByte(IncrementCurrentPos()))
            {
                FontNameBuffer_Add((byte)ch);
            }
        }

        if (symbolFontNameCount == MaxSymbolFontNameLength)
        {
            while (ch != ';' && !(isNonSemicolonSeparatorChar = IsNonPlainText[(byte)ch]))
            {
                ch = (char)GetByte(IncrementCurrentPos());
                FontNameBuffer_Add((byte)ch);
            }
        }

        /*
        Support weird nonsense in the font table like:

        {Zapf Dingbats{\*\falt Monotype Sorts};}

        where we should stop at the { instead of the ; so we get the name right.

        Also whatever nonsense is going on in some of those RtfPipe test files.
        */
        if (isNonSemicolonSeparatorChar)
        {
            _currentPos--;
        }

        ReadOnlySpan<byte> fontNameBufferSpan = _fontNameBuffer.AsSpan(0, _fontNameBuffer_Count);

        foreach (FontNameData fontNameSuffix in FontNameSuffixes)
        {
            if (fontNameBufferSpan.EndsWith(fontNameSuffix.Bytes))
            {
                return fontNameSuffix;
            }
        }

        return new FontNameData(Array.Empty<byte>(), NoCodePage, Array.Empty<byte>());
    }

    /*
    Handle hex run separately in scalar so as to avoid the degenerate case of \'xx being read like this:
    - \' is control char, so skip
    - Read 'x', it's plaintext, read next 'x', it's also plaintext, so go into SIMD
    - SIMD does a huge setup and overhead, reads one char (the second 'x'), and returns
    */
    private void HandleHexRun(ref byte bufferRef)
    {
        if (_currentPos < _rtfBytesLength - 1)
        {
            _currentPos += 2;
        }
        else
        {
            _ = GetByte(IncrementCurrentPos());
            _ = GetByte(IncrementCurrentPos());
        }

        // TODO: Manually duplicated code for performance - should be automated if possible
        while (_currentPos < _rtfBytesLength - 3)
        {
            byte b = GetByteAtCurrentPosAndIncrement(ref bufferRef);
            if (b == (byte)'\\')
            {
                b = GetByteAtCurrentPosAndIncrement(ref bufferRef);
                if (b == (byte)'\'')
                {
                    _currentPos += 2;
                }
                else
                {
                    _currentPos -= 2;
                    return;
                }
            }
            // Spaces end a hex run, but linebreaks don't.
            else if (b is not (byte)'\r' and not (byte)'\n')
            {
                _currentPos--;
                return;
            }
        }

        while (_currentPos < _rtfBytesLength)
        {
            byte b = GetByte(IncrementCurrentPos());
            if (b == (byte)'\\')
            {
                b = GetByte(IncrementCurrentPos());
                if (b == (byte)'\'')
                {
                    _ = GetByte(IncrementCurrentPos());
                    _ = GetByte(IncrementCurrentPos());
                }
                else
                {
                    _currentPos -= 2;
                    return;
                }
            }
            // Spaces end a hex run, but linebreaks don't.
            else if (b is not (byte)'\r' and not (byte)'\n')
            {
                _currentPos--;
                return;
            }
        }
    }

    private void ParseAndBuildColorTable()
    {
        _colorTable = null;

        int closingBraceIndex = Array_IndexOfByte_Fast(_rtfBytes, (byte)'}', _currentPos, _rtfBytesLength - _currentPos);
        if (closingBraceIndex == -1) return;

        ReadOnlySpan<byte> colorTableSpan = _rtfBytes.AsSpan(_currentPos, closingBraceIndex - _currentPos);

        // 64 x 4 bytes == 256 bytes, totally fine and unlikely to need to grow (largest known is 23), and saves
        // counting all ';' chars
        _colorTable = new List<RtfColor>(64);

        ReadOnlySpan<byte> redString = "\\red"u8;
        ReadOnlySpan<byte> greenString = "\\green"u8;
        ReadOnlySpan<byte> blueString = "\\blue"u8;

        bool first = true;
        foreach (ReadOnlySpan<byte> entry in colorTableSpan.Tokenize((byte)';'))
        {
            if (entry.IsWhiteSpace())
            {
                if (first)
                {
                    _colorTable.Add(new RtfColor(0, 0, 0, isDefaultColor: true));
                }
            }
            else
            {
                if (GetColorByte(entry, redString, out byte red) &&
                    GetColorByte(entry, greenString, out byte green) &&
                    GetColorByte(entry, blueString, out byte blue))
                {
                    _colorTable.Add(new RtfColor(red, green, blue));
                }
            }
            first = false;
        }

        if (first) _colorTable = null;
        return;

        static bool GetColorByte(ReadOnlySpan<byte> entry, ReadOnlySpan<byte> hueWord, out byte result)
        {
            int hueIndex = entry.IndexOf(hueWord);
            if (hueIndex > -1)
            {
                int indexPastHue = hueIndex + hueWord.Length;
                if (indexPastHue < entry.Length)
                {
                    byte firstDigit = entry[indexPastHue];
                    if (firstDigit.IsAsciiNumeric())
                    {
                        int colorValue = firstDigit - '0';
                        for (int colorI = indexPastHue + 1; colorI < entry.Length; colorI++)
                        {
                            byte c = entry[colorI];
                            if (!c.IsAsciiNumeric()) break;
                            // Color value too long, must be 1-3 digits
                            if (colorI >= indexPastHue + 3)
                            {
                                result = 0;
                                return false;
                            }
                            colorValue *= 10;
                            colorValue += c - '0';
                        }
                        if (colorValue is >= 0 and <= 255)
                        {
                            result = (byte)colorValue;
                            return true;
                        }
                    }
                }
            }

            result = 0;
            return false;
        }
    }

    #endregion

    #region Act on keywords

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private RtfError DispatchKeyword(ref byte bufferRef, Symbol symbol, int param, bool hasParam)
    {
        if (!GroupStack_CurrentSkipDest)
        {
            switch (symbol.KeywordType)
            {
                case KeywordType.Property:
                {
                    if (symbol.UseDefaultParam || !hasParam) param = symbol.DefaultParam;
                    if ((Property)symbol.Index == Property.FontNum)
                    {
                        GroupStack_CurrentPropertyFontNum = param;
                        return RtfError.OK;
                    }
                }
                return RtfError.OK;
                case KeywordType.Special:
                    SpecialType specialType = (SpecialType)symbol.Index;
                    return DispatchSpecialKeyword(ref bufferRef, specialType, symbol, param);
                case KeywordType.Destination:
                    DestinationType destType = (DestinationType)symbol.Index;
                    switch (destType)
                    {
                        case DestinationType.SkippableHex:
                            if (symbol.DefaultParam == 1)
                            {
                                return HandleSkippableHexData(ref bufferRef);
                            }
                            else
                            {
                                _currentPos = IndexOfNextClosingBrace_ChunkAware();
                                return RtfError.OK;
                            }
                        case DestinationType.Skip:
                            SkipDest(ref bufferRef);
                            return RtfError.OK;
                        default:
                            return RtfError.OK;
                    }
                default:
                    return RtfError.OK;
            }
        }
        else
        {
            switch (symbol.KeywordType)
            {
                case KeywordType.Destination:
                {
                    if ((DestinationType)symbol.Index == DestinationType.SkippableHex)
                    {
                        if (symbol.DefaultParam == 1)
                        {
                            return HandleSkippableHexData(ref bufferRef);
                        }
                        else
                        {
                            _currentPos = IndexOfNextClosingBrace_ChunkAware();
                        }
                    }
                    return RtfError.OK;
                }
                case KeywordType.Special:
                    SpecialType specialType = (SpecialType)symbol.Index;
                    return specialType == SpecialType.SkipNumberOfBytes
                        ? DispatchSpecialKeyword(ref bufferRef, specialType, symbol, param)
                        : RtfError.OK;
                default:
                    return RtfError.OK;
            }
        }
    }

    private RtfError DispatchSpecialKeyword(ref byte bufferRef, SpecialType specialType, Symbol symbol, int param)
    {
        switch (specialType)
        {
            case SpecialType.SkipNumberOfBytes:
                if (symbol.UseDefaultParam) param = symbol.DefaultParam;
                if (param < 0) return RtfError.AbortedForSafety;
                IncrementCurrentPos_ArbitraryAmountForward(param);
                break;
            case SpecialType.HeaderCodePage:
                _headerCodePage = IsNonEmptyUShortParam(param) ? (ushort)param : (ushort)0;
                break;
            case SpecialType.DefaultFont:
                if (!_headerDefaultFontSet)
                {
                    _headerDefaultFontNum = param;
                    _headerDefaultFontSet = true;
                }
                break;
            case SpecialType.FontTable:
            {
                RtfError error = ParseFontTable(ref bufferRef);
                _parsedFontTable = true;

                if (!_getColorTable || _parsedColorTable)
                {
                    EndParse();
                }
                return error;
            }
            case SpecialType.ColorTable:
                // Spec is to ignore any further color tables after the first one
                if (_getColorTable && !_parsedColorTable)
                {
                    _parsedColorTable = true;
                    ParseAndBuildColorTable();
                    if (_parsedFontTable)
                    {
                        EndParse();
                        return RtfError.OK;
                    }
                }
                else
                {
                    _currentPos = IndexOfNextClosingBrace_ChunkAware();
                }
                break;
        }

        return RtfError.OK;
    }

    #endregion

    #region Helpers

    private void EndParse()
    {
        _currentPos = _rtfBytesLength;
        _groupStackTopIndex = 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int HeaderDefaultIfNotSet(int fontNum) => fontNum > NoFontNumber ? fontNum : _headerDefaultFontNum;

    #endregion

    #region Fast skipping

    private int IndexOfNextClosingBrace_ChunkAware()
    {
        int foundIndex = RTF_Array_IndexOfByte_Fast(_rtfBytes, (byte)'}', _currentPos, _rtfBytesLength - _currentPos);
        return foundIndex > -1 ? foundIndex : _rtfBytesLength;
    }

    private RtfError HandleSkippableHexData(ref byte bufferRef)
    {
        // Prevent stack overflow from maliciously-crafted rtf files - we should never recurse back into here in
        // a spec-conforming file.
        if (_inHandleSkippableHexData) return RtfError.AbortedForSafety;
        _inHandleSkippableHexData = true;

        int startGroupLevel = _groupStackTopIndex;

        while (_currentPos < _rtfBytesLength)
        {
            char ch = (char)GetByteAtCurrentPosAndIncrement(ref bufferRef);

            switch (ch)
            {
                case '{':
                    GroupStack_DeepCopyToNext();
                    break;
                case '}':
                    if (_groupStackTopIndex == 0) return RtfError.StackUnderflow;
                    --_groupStackTopIndex;
                    if (_groupStackTopIndex < startGroupLevel)
                    {
                        _inHandleSkippableHexData = false;
                        return RtfError.OK;
                    }
                    break;
                case '\\':
                    // This implicitly also handles the case where the data is \binN instead of hex
                    RtfError ec = ParseKeyword(ref bufferRef);
                    if (ec != RtfError.OK) return ec;
                    break;
                case '\r':
                case '\n':
                    break;
                default:
                    if (_groupStackTopIndex == startGroupLevel)
                    {
                        _currentPos = IndexOfNextClosingBrace_ChunkAware();
                    }
                    break;
            }
        }

        _inHandleSkippableHexData = false;
        return RtfError.OK;
    }

    private void SkipDest(ref byte bufferRef)
    {
        // This method should either skip the entire destination in one go, or else bail and use the slow path
        // for the rest of the destination.
        if (GroupStack_CurrentSkipDest)
        {
            return;
        }

        GroupStack_CurrentSkipDest = true;

        if (!System.Numerics.Vector.IsHardwareAccelerated)
        {
            return;
        }

        int startGroupLevel = _groupStackTopIndex;

        int index = _currentPos;
        while (_currentPos < _rtfBytesLength)
        {
            index = SIMD_SkipDest(ref bufferRef, index, _rtfBytesLength - index);

            /*
            Curly braces can be escaped like \{ and \}. But there can be an arbitrary amount of backslashes
            before a curly brace, because it could be a series of escaped backslashes and then an escaped
            curly brace: \\\\\\\}. Which means if we encountered one, we'd have to read an arbitrary amount
            back in the stream, which we can't do. So if we don't find the end of our subgroup stack in the
            current buffer chunk, just give up and take the slow path that properly parses escapes.
            */
            if (index <= 0 ||
                index >= _rtfBytesLength ||
                GetByteAtPos(ref bufferRef, index - 1) == '\\')
            {
                _groupStackTopIndex = startGroupLevel;
                return;
            }
            switch (GetByteAtPos(ref bufferRef, index))
            {
                case (byte)'{':
                    ++_groupStackTopIndex;
                    break;
                case (byte)'}':
                    --_groupStackTopIndex;
                    if (_groupStackTopIndex < startGroupLevel)
                    {
                        _currentPos = index + 1;
                        return;
                    }
                    break;
                // If we find \bin, run away: it could contain unescaped curly braces that are just part of
                // the raw binary.
                case (byte)'\\':
                    if (index > _rtfBytesLength - BinLength ||
                        (GetByteAtPos(ref bufferRef, index + 1) == 'b' &&
                         GetByteAtPos(ref bufferRef, index + 2) == 'i' &&
                         GetByteAtPos(ref bufferRef, index + 3) == 'n'))
                    {
                        _groupStackTopIndex = startGroupLevel;
                        return;
                    }
                    break;
            }
            ++index;
        }
    }

    #endregion

    #region Read and seek wrappers

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private byte GetByteAtCurrentPosAndIncrement(ref byte bufferRef)
    {
        Debug.Assert(_currentPos < _rtfBytesLength);
        return Unsafe.AddByteOffset(ref bufferRef, (nint)IncrementCurrentPos());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private byte GetByteAtPos(ref byte bufferRef, int pos)
    {
        Debug.Assert(pos < _rtfBytesLength);
        return Unsafe.AddByteOffset(ref bufferRef, (nint)pos);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ref byte GetRefAtPos(ref byte bufferRef, int pos)
    {
        Debug.Assert(pos < _rtfBytesLength);
        return ref Unsafe.AddByteOffset(ref bufferRef, (nint)pos);
    }

    // Let the internal bounds-checker handle it - we don't need a soft bounds-check because we don't use streams
    // and don't support arrays with virtual lengths.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private byte GetByte(int index)
    {
        return _rtfBytes[index];
    }

    /// <summary>
    /// Increment _currentPos. Behaves like _currentPos++, returning the value before it was modified.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int IncrementCurrentPos()
    {
        // This performs better than raw _currentPos++, inexplicably. Sure?
        return _currentPos++;
    }

    private void IncrementCurrentPos_ArbitraryAmountForward(int amount)
    {
        _currentPos += amount;
    }

    #endregion

    #region GroupStack

    [StructLayout(LayoutKind.Auto)]
    private struct GroupStackFrame
    {
        internal bool SkipDestination;

        internal int PropFontNum;
    }

    private const int _groupStackDefaultCapacity = 100;
    private int _groupStackCapacity;
    private int _groupStackTopIndex;

    private GroupStackFrame[] _groupStackFrames;

    [MemberNotNull(nameof(_groupStackFrames))]
    private void InitGroupStack()
    {
        _groupStackTopIndex = 0;
        _groupStackCapacity = _groupStackDefaultCapacity;

        _groupStackFrames = new GroupStackFrame[_groupStackCapacity];
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void GroupStack_Grow()
    {
        int newCapacity = _groupStackCapacity * 2;
        if ((uint)newCapacity > Array.MaxLength) newCapacity = Array.MaxLength;

        _groupStackCapacity = newCapacity;
        Array.Resize(ref _groupStackFrames, _groupStackCapacity);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void GroupStack_DeepCopyToNext()
    {
        // We don't really take a speed hit from this at all, but we support files with a stupid amount of
        // nested groups now.
        if (_groupStackTopIndex >= _groupStackCapacity - 1)
        {
            GroupStack_Grow();
        }

        ref GroupStackFrame groupStackRef = ref GetArrayDataReference(_groupStackFrames);
        // .NET itself does this (ArraySortHelper.cs for example), so I'm just going to say it's safe.
        // ARM users yell at me if it isn't I guess.
        Unsafe.Add(ref groupStackRef, _groupStackTopIndex + 1) = Unsafe.Add(ref groupStackRef, _groupStackTopIndex);

        ++_groupStackTopIndex;
    }

    #region Current group

    private bool GroupStack_CurrentSkipDest
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _groupStackFrames[_groupStackTopIndex].SkipDestination;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => _groupStackFrames[_groupStackTopIndex].SkipDestination = value;
    }

    private int GroupStack_CurrentPropertyFontNum
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _groupStackFrames[_groupStackTopIndex].PropFontNum;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => _groupStackFrames[_groupStackTopIndex].PropFontNum = value;
    }

    #endregion

    // Current group always begins at group 0, so reset just that one
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void GroupStack_Reset()
    {
        _groupStackTopIndex = 0;

        _groupStackFrames[0] = new GroupStackFrame
        {
            SkipDestination = false,
            PropFontNum = NoFontNumber,
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void GroupStack_ResetCapacityToDefault()
    {
        if (_groupStackCapacity > _groupStackDefaultCapacity)
        {
            InitGroupStack();
        }
    }

    #endregion

    #region SymbolDict

    /* ANSI-C code produced by gperf version 3.1 */
    /* Command-line: 'C:\\gperf\\tools\\gperf.exe' --output-file='C:\\_al_rtf_table_gen\\gperfOutputFile.txt' -r -t 'C:\\_al_rtf_table_gen\\gperfFormatFile.txt'  */
    /* Computed positions: -k'1-3,$' */

    //private const int TOTAL_KEYWORDS = 56;
    //private const int MIN_WORD_LENGTH = 2;
    private const int MAX_WORD_LENGTH = 18;
    //private const int MIN_HASH_VALUE = 19;
    private const int MAX_HASH_VALUE = 170;
    /* maximum key range = 152, duplicates = 0 */

    private static readonly byte[] asso_values =
    [
        171, 171, 171, 171, 171, 171, 171, 171, 171, 171,
        171, 171, 171, 171, 171, 171, 171, 171, 171, 171,
        171, 171, 171, 171, 171, 171, 171, 171, 171, 171,
        171, 171, 171, 171, 171, 171, 171, 171, 171, 171,
        171, 171, 171, 171, 171, 171, 171, 171, 171, 171,
        171, 171, 171, 171, 171, 171, 171, 171, 171, 171,
        171, 171, 171, 171, 171, 171, 171, 171, 171, 171,
        171, 171, 171, 171, 171, 171, 171, 171, 171, 171,
        171, 171, 171, 171, 171, 171, 171, 171, 171, 171,
        171, 171, 171, 171, 171, 171, 171, 58, 44, 10,
        50, 13, 45, 53, 55, 10, 3, 56, 28, 45,
        54, 62, 18, 171, 50, 32, 27, 171, 61, 171,
        4, 0, 171, 171, 171, 171, 171, 171, 171, 171,
        171, 171, 171, 171, 171, 171, 171, 171, 171, 171,
        171, 171, 171, 171, 171, 171, 171, 171, 171, 171,
        171, 171, 171, 171, 171, 171, 171, 171, 171, 171,
        171, 171, 171, 171, 171, 171, 171, 171, 171, 171,
        171, 171, 171, 171, 171, 171, 171, 171, 171, 171,
        171, 171, 171, 171, 171, 171, 171, 171, 171, 171,
        171, 171, 171, 171, 171, 171, 171, 171, 171, 171,
        171, 171, 171, 171, 171, 171, 171, 171, 171, 171,
        171, 171, 171, 171, 171, 171, 171, 171, 171, 171,
        171, 171, 171, 171, 171, 171, 171, 171, 171, 171,
        171, 171, 171, 171, 171, 171, 171, 171, 171, 171,
        171, 171, 171, 171, 171, 171, 171, 171, 171, 171,
        171, 171, 171, 171, 171, 171,
    ];

    private static readonly Symbol _fontSymbol = new("f", 0, false, KeywordType.Property, (ushort)Property.FontNum);

    private static readonly ushort[] _symbolFirstCharTable =
    [
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0x7802, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0x7002, 0,
        0, 0, 0, 0, 0, 0, 0, 0x7402, 0, 0, 0, 0, 0x6302, 0, 0, 0, 0x7007, 0, 0, 0, 0, 0, 0, 0, 0x7403, 0, 0,
        0x7004, 0, 0x7402, 0, 0, 0, 0, 0, 0, 0x6D03, 0x730A, 0, 0, 0x7405, 0, 0x6308, 0x6307, 0, 0, 0x7006,
        0x7203, 0x7007, 0, 0, 0, 0x6402, 0, 0, 0, 0, 0x6307, 0, 0x7006, 0, 0, 0, 0x6C08, 0x6B08, 0, 0, 0x6409, 0,
        0, 0, 0, 0x6104, 0, 0, 0x7409, 0, 0x6312, 0x7307, 0x6207, 0x6407, 0x6206, 0, 0, 0x6607, 0x700C, 0,
        0x6303, 0, 0x6904, 0, 0x6606, 0, 0, 0, 0, 0x6608, 0x6607, 0x6F07, 0, 0, 0x6F08, 0x6607, 0x6608, 0x6409,
        0x7003, 0, 0, 0, 0x6106, 0x6607, 0, 0x6404, 0, 0, 0, 0x6807, 0, 0x6107, 0, 0, 0, 0, 0x6203, 0, 0, 0x6605,
        0x6607, 0, 0, 0x7206, 0x6606, 0x6607, 0x6807, 0, 0, 0, 0x6806, 0x6807,
    ];

    /*
    NOTE: This is generated. Values can be modified, but not keys (keys are the first string params).
    Also no reordering. Adding, removing, reordering, or modifying keys requires generating a new version.
    See RTF_SymbolListGenSource.cs for how to generate a new version (it also contains the original
    Symbol list which must be used as the source to generate this one).

    See RTF_SymbolListGenSource.cs for comments on the keywords.
    */
    private static readonly Symbol?[] _symbolTable =
    [
        null, null, null, null, null, null, null, null, null,
        null, null, null, null, null, null, null, null, null,
        null,
// Entry 46
        new Symbol("xe", 0, false, KeywordType.Destination, (ushort)DestinationType.Skip),
        null, null, null, null, null, null, null, null, null,
        null,
// Entry 1
        new Symbol("pc", 437, true, KeywordType.Special, (ushort)SpecialType.HeaderCodePage),
        null, null, null, null, null, null, null, null,
// Entry 43
        new Symbol("tc", 0, false, KeywordType.Destination, (ushort)DestinationType.Skip),
        null, null, null, null,
// Entry 11
        new Symbol("cs", 0, false, KeywordType.Destination, (ushort)DestinationType.SkipKeywordButNotGroup),
        null, null, null,
// Entry 38
        new Symbol("private", 0, false, KeywordType.Destination, (ushort)DestinationType.Skip),
        null, null, null, null, null, null, null,
// Entry 45
        new Symbol("txe", 0, false, KeywordType.Destination, (ushort)DestinationType.Skip),
        null, null,
// Entry 47
        new Symbol("pict", 1, false, KeywordType.Destination, (ushort)DestinationType.SkippableHex),
        null,
// Entry 13
        new Symbol("ts", 0, false, KeywordType.Destination, (ushort)DestinationType.SkipKeywordButNotGroup),
        null, null, null, null, null, null,
// Entry 2
        new Symbol("mac", 10000, true, KeywordType.Special, (ushort)SpecialType.HeaderCodePage),
// Entry 41
        new Symbol("stylesheet", 0, false, KeywordType.Destination, (ushort)DestinationType.Skip),
        null, null,
// Entry 44
        new Symbol("title", 0, false, KeywordType.Destination, (ushort)DestinationType.Skip),
        null,
// Entry 18
        new Symbol("colortbl", 0, false, KeywordType.Special, (ushort)SpecialType.ColorTable),
// Entry 20
        new Symbol("creatim", 0, false, KeywordType.Destination, (ushort)DestinationType.Skip),
        null, null,
// Entry 15
        new Symbol("pntext", 0, false, KeywordType.Destination, (ushort)DestinationType.Skip),
// Entry 40
        new Symbol("rxe", 0, false, KeywordType.Destination, (ushort)DestinationType.Skip),
// Entry 37
        new Symbol("printim", 0, false, KeywordType.Destination, (ushort)DestinationType.Skip),
        null, null, null,
// Entry 12
        new Symbol("ds", 0, false, KeywordType.Destination, (ushort)DestinationType.SkipKeywordButNotGroup),
        null, null, null, null,
// Entry 19
        new Symbol("comment", 0, false, KeywordType.Destination, (ushort)DestinationType.Skip),
        null,
// Entry 55
        new Symbol("panose", 20, true, KeywordType.Special, (ushort)SpecialType.SkipNumberOfBytes),
        null, null, null,
// Entry 14
        new Symbol("listtext", 0, false, KeywordType.Destination, (ushort)DestinationType.Skip),
// Entry 35
        new Symbol("keywords", 0, false, KeywordType.Destination, (ushort)DestinationType.Skip),
        null, null,
// Entry 51
        new Symbol("datastore", 0, false, KeywordType.Destination, (ushort)DestinationType.SkippableHex),
        null, null, null, null,
// Entry 0
        new Symbol("ansi", 0, true, KeywordType.Special, (ushort)SpecialType.HeaderCodePage),
        null, null,
// Entry 48
        new Symbol("themedata", 0, false, KeywordType.Destination, (ushort)DestinationType.SkippableHex),
        null,
// Entry 49
        new Symbol("colorschememapping", 0, false, KeywordType.Destination, (ushort)DestinationType.SkippableHex),
// Entry 42
        new Symbol("subject", 0, false, KeywordType.Destination, (ushort)DestinationType.Skip),
// Entry 54
        new Symbol("blipuid", 32, true, KeywordType.Special, (ushort)SpecialType.SkipNumberOfBytes),
// Entry 21
        new Symbol("doccomm", 0, false, KeywordType.Destination, (ushort)DestinationType.Skip),
// Entry 17
        new Symbol("buptim", 0, false, KeywordType.Destination, (ushort)DestinationType.Skip),
        null, null,
// Entry 29
        new Symbol("ftnsepc", 0, false, KeywordType.Destination, (ushort)DestinationType.Skip),
// Entry 50
        new Symbol("passwordhash", 0, false, KeywordType.Destination, (ushort)DestinationType.SkippableHex),
        null,
// Entry 8
        new Symbol("cpg", ushort.MaxValue, false, KeywordType.CPG, 0),
        null,
// Entry 34
        new Symbol("info", 0, false, KeywordType.Destination, (ushort)DestinationType.Skip),
        null,
// Entry 28
        new Symbol("ftnsep", 0, false, KeywordType.Destination, (ushort)DestinationType.Skip),
        null, null, null, null,
// Entry 26
        new Symbol("footnote", 0, false, KeywordType.Destination, (ushort)DestinationType.Skip),
// Entry 10
        new Symbol("fldinst", 0, false, KeywordType.Destination, (ushort)DestinationType.SkipKeywordButNotGroup),
// Entry 53
        new Symbol("objdata", 1, false, KeywordType.Destination, (ushort)DestinationType.SkippableHex),
        null, null,
// Entry 36
        new Symbol("operator", 0, false, KeywordType.Destination, (ushort)DestinationType.Skip),
// Entry 6
        new Symbol("fonttbl", 0, false, KeywordType.Special, (ushort)SpecialType.FontTable),
// Entry 7
        new Symbol("fcharset", ushort.MaxValue, false, KeywordType.FCharset, 0),
// Entry 52
        new Symbol("datafield", 0, false, KeywordType.Destination, (ushort)DestinationType.SkippableHex),
// Entry 3
        new Symbol("pca", 850, true, KeywordType.Special, (ushort)SpecialType.HeaderCodePage),
        null, null, null,
// Entry 16
        new Symbol("author", 0, false, KeywordType.Destination, (ushort)DestinationType.Skip),
// Entry 24
        new Symbol("footerl", 0, false, KeywordType.Destination, (ushort)DestinationType.Skip),
        null,
// Entry 5
        new Symbol("deff", 0, false, KeywordType.Special, (ushort)SpecialType.DefaultFont),
        null, null, null,
// Entry 32
        new Symbol("headerl", 0, false, KeywordType.Destination, (ushort)DestinationType.Skip),
        null,
// Entry 4
        new Symbol("ansicpg", 0, false, KeywordType.Special, (ushort)SpecialType.HeaderCodePage),
        null, null, null, null,
// Entry 9
        new Symbol("bin", 0, false, KeywordType.Special, (ushort)SpecialType.SkipNumberOfBytes),
        null, null,
// Entry 27
        new Symbol("ftncn", 0, false, KeywordType.Destination, (ushort)DestinationType.Skip),
// Entry 23
        new Symbol("footerf", 0, false, KeywordType.Destination, (ushort)DestinationType.Skip),
        null, null,
// Entry 39
        new Symbol("revtim", 0, false, KeywordType.Destination, (ushort)DestinationType.Skip),
// Entry 22
        new Symbol("footer", 0, false, KeywordType.Destination, (ushort)DestinationType.Skip),
// Entry 25
        new Symbol("footerr", 0, false, KeywordType.Destination, (ushort)DestinationType.Skip),
// Entry 31
        new Symbol("headerf", 0, false, KeywordType.Destination, (ushort)DestinationType.Skip),
        null, null, null,
// Entry 30
        new Symbol("header", 0, false, KeywordType.Destination, (ushort)DestinationType.Skip),
// Entry 33
        new Symbol("headerr", 0, false, KeywordType.Destination, (ushort)DestinationType.Skip),
    ];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Symbol? LookUpControlWord(ref byte keywordRef, byte len)
    {
        // Min word length is 1, and we're guaranteed to always be at least 1, so no need to check for >= min
        if (len <= MAX_WORD_LENGTH)
        {
            int key = len;

            // We handle 1-length before we get here, so know we're at least 2.
            // NOTE: This logic is optimized to do the same thing as the gperf generated code, but more efficiently.
            key += asso_values[Unsafe.AddByteOffset(ref keywordRef, (nint)len - 1)];
            if (len > 2) key += asso_values[Unsafe.AddByteOffset(ref keywordRef, 2)];
            key += asso_values[Unsafe.AddByteOffset(ref keywordRef, 0)];

            if (key <= MAX_HASH_VALUE)
            {
                ushort firstCharAndLength = _symbolFirstCharTable[key];
                ushort incomingFirstCharAndLength = (ushort)((ushort)(keywordRef << 8) + len);
                if (incomingFirstCharAndLength != firstCharAndLength)
                {
                    return null;
                }

                Symbol symbol = _symbolTable[key]!;

                string symbolKeyword = symbol.Keyword;
                for (byte ci = 1; ci < len; ci++)
                {
                    if (GetByteAtPos_KeywordLookup(ref keywordRef, ci) != symbolKeyword[ci])
                    {
                        return null;
                    }
                }

                return symbol;
            }
        }

        return null;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static byte GetByteAtPos_KeywordLookup(ref byte keywordRef, int pos)
    {
        return Unsafe.AddByteOffset(ref keywordRef, (nint)pos);
    }

    #endregion
}
