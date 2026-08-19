/*
 * MIT License
 * 
 * Copyright (c) 2024-2026 Brian Tobin
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
*/

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
using ReasonableRTF_Displayed.Enums;
using ReasonableRTF_Displayed.Extensions;
using ReasonableRTF_Displayed.Models.Fonts;
using ReasonableRTF_Displayed.Models.Symbols;

namespace ReasonableRTF_Displayed;

public sealed partial class RRTF_RtfDisplayedReadmeParser
{
    private List<RtfColor>? _colorTable;
    private bool _foundColorTable;
    private bool _getColorTable;
    private bool _getLangs;

    public sealed class RRTF_LangItem
    {
        private static int GetDigitsUpTo5(int number) =>
            number <= 9 ? 1 :
            number <= 99 ? 2 :
            number <= 999 ? 3 :
            number <= 9999 ? 4 :
            5;

        public int Index;
        public readonly int CodePage;
        public readonly int DigitsCount;

        public RRTF_LangItem(int index, int codePage)
        {
            Index = index;
            CodePage = codePage;
            DigitsCount = GetDigitsUpTo5(codePage);
        }
    }

    private List<RRTF_LangItem>? _langItems;

    #region Private fields

    // +1 to allow reading one beyond the max and then checking for it to return an error
    private readonly byte[] _keyword = new byte[_keywordMaxLen + 1];

    private const int _keywordParseMaxRequiredBytes =
        _keywordMaxLen + 1 + // +1 to read one beyond for length checking purposes
        1 + // Minus sign
        _paramMaxLen + 1 + // +1 to read one beyond for length checking purposes
        1; // Space at end

    // "\bin"
    private const int _binLength = 4;
    private readonly uint _binUInt = BitConverter.IsLittleEndian ? 0x6E69625Cu : 0x5C62696Eu;

    private const int _internalBufferDefaultCapacity = 32;

    /// <summary>
    /// Since font numbers can be negative, let's just use a slightly less likely value than the already unlikely
    /// enough -1...
    /// </summary>
    private const int NoFontNumber = int.MinValue;
    private const ushort NoLang = ushort.MaxValue;
    private const ushort NoCodePage = ushort.MaxValue;

    private const int _keywordMaxLen = 32;
    // Most are signed int16 (5 chars), but a few can be signed int32 (10 chars)
    private const int _paramMaxLen = 10;

    private const int _undefinedLanguage = 1024;

    #region Tables

    #region Conversion tables

    #region Charset to code page

    private const int _charSetToCodePageLength = 256;

    private static readonly ushort[] _charSetToCodePage =
    [
        1252, // 0 - "ANSI" (1252) (Yes, this is specified as _explicitly_ 1252, so this isn't a straggling 1252-default)
        0, // 1 - Default
        42, // 2 - Symbol
        NoCodePage, // 3
        NoCodePage, // 4
        NoCodePage, // 5
        NoCodePage, // 6
        NoCodePage, // 7
        NoCodePage, // 8
        NoCodePage, // 9
        NoCodePage, // 10
        NoCodePage, // 11
        NoCodePage, // 12
        NoCodePage, // 13
        NoCodePage, // 14
        NoCodePage, // 15
        NoCodePage, // 16
        NoCodePage, // 17
        NoCodePage, // 18
        NoCodePage, // 19
        NoCodePage, // 20
        NoCodePage, // 21
        NoCodePage, // 22
        NoCodePage, // 23
        NoCodePage, // 24
        NoCodePage, // 25
        NoCodePage, // 26
        NoCodePage, // 27
        NoCodePage, // 28
        NoCodePage, // 29
        NoCodePage, // 30
        NoCodePage, // 31
        NoCodePage, // 32
        NoCodePage, // 33
        NoCodePage, // 34
        NoCodePage, // 35
        NoCodePage, // 36
        NoCodePage, // 37
        NoCodePage, // 38
        NoCodePage, // 39
        NoCodePage, // 40
        NoCodePage, // 41
        NoCodePage, // 42
        NoCodePage, // 43
        NoCodePage, // 44
        NoCodePage, // 45
        NoCodePage, // 46
        NoCodePage, // 47
        NoCodePage, // 48
        NoCodePage, // 49
        NoCodePage, // 50
        NoCodePage, // 51
        NoCodePage, // 52
        NoCodePage, // 53
        NoCodePage, // 54
        NoCodePage, // 55
        NoCodePage, // 56
        NoCodePage, // 57
        NoCodePage, // 58
        NoCodePage, // 59
        NoCodePage, // 60
        NoCodePage, // 61
        NoCodePage, // 62
        NoCodePage, // 63
        NoCodePage, // 64
        NoCodePage, // 65
        NoCodePage, // 66
        NoCodePage, // 67
        NoCodePage, // 68
        NoCodePage, // 69
        NoCodePage, // 70
        NoCodePage, // 71
        NoCodePage, // 72
        NoCodePage, // 73
        NoCodePage, // 74
        NoCodePage, // 75
        NoCodePage, // 76
        10000, // 77 - Mac Roman
        10001, // 78 - Mac Shift Jis
        10003, // 79 - Mac Hangul
        10008, // 80 - Mac GB2312
        10002, // 81 - Mac Big5
        NoCodePage, // 82 - Mac Johab (old) (codepage unknown)
        10005, // 83 - Mac Hebrew
        10004, // 84 - Mac Arabic
        10006, // 85 - Mac Greek
        10081, // 86 - Mac Turkish
        10021, // 87 - Mac Thai
        10029, // 88 - Mac East Europe
        10007, // 89 - Mac Russian
        NoCodePage, // 90
        NoCodePage, // 91
        NoCodePage, // 92
        NoCodePage, // 93
        NoCodePage, // 94
        NoCodePage, // 95
        NoCodePage, // 96
        NoCodePage, // 97
        NoCodePage, // 98
        NoCodePage, // 99
        NoCodePage, // 100
        NoCodePage, // 101
        NoCodePage, // 102
        NoCodePage, // 103
        NoCodePage, // 104
        NoCodePage, // 105
        NoCodePage, // 106
        NoCodePage, // 107
        NoCodePage, // 108
        NoCodePage, // 109
        NoCodePage, // 110
        NoCodePage, // 111
        NoCodePage, // 112
        NoCodePage, // 113
        NoCodePage, // 114
        NoCodePage, // 115
        NoCodePage, // 116
        NoCodePage, // 117
        NoCodePage, // 118
        NoCodePage, // 119
        NoCodePage, // 120
        NoCodePage, // 121
        NoCodePage, // 122
        NoCodePage, // 123
        NoCodePage, // 124
        NoCodePage, // 125
        NoCodePage, // 126
        NoCodePage, // 127
        932, // 128 - Shift JIS (Windows-31J) (932)
        949, // 129 - Hangul
        1361, // 130 - Johab
        NoCodePage, // 131
        NoCodePage, // 132
        NoCodePage, // 133
        936, // 134 - GB2312
        NoCodePage, // 135
        950, // 136 - Big5
        NoCodePage, // 137
        NoCodePage, // 138
        NoCodePage, // 139
        NoCodePage, // 140
        NoCodePage, // 141
        NoCodePage, // 142
        NoCodePage, // 143
        NoCodePage, // 144
        NoCodePage, // 145
        NoCodePage, // 146
        NoCodePage, // 147
        NoCodePage, // 148
        NoCodePage, // 149
        NoCodePage, // 150
        NoCodePage, // 151
        NoCodePage, // 152
        NoCodePage, // 153
        NoCodePage, // 154
        NoCodePage, // 155
        NoCodePage, // 156
        NoCodePage, // 157
        NoCodePage, // 158
        NoCodePage, // 159
        NoCodePage, // 160
        1253, // 161 - Greek
        1254, // 162 - Turkish
        1258, // 163 - Vietnamese
        NoCodePage, // 164
        NoCodePage, // 165
        NoCodePage, // 166
        NoCodePage, // 167
        NoCodePage, // 168
        NoCodePage, // 169
        NoCodePage, // 170
        NoCodePage, // 171
        NoCodePage, // 172
        NoCodePage, // 173
        NoCodePage, // 174
        NoCodePage, // 175
        NoCodePage, // 176
        1255, // 177 - Hebrew
        1256, // 178 - Arabic
        NoCodePage, // 179 - Arabic Traditional (old) (codepage unknown)
        NoCodePage, // 180 - Arabic user (old) (codepage unknown)
        NoCodePage, // 181 - Hebrew user (old) (codepage unknown)
        NoCodePage, // 182
        NoCodePage, // 183
        NoCodePage, // 184
        NoCodePage, // 185
        1257, // 186 - Baltic
        NoCodePage, // 187
        NoCodePage, // 188
        NoCodePage, // 189
        NoCodePage, // 190
        NoCodePage, // 191
        NoCodePage, // 192
        NoCodePage, // 193
        NoCodePage, // 194
        NoCodePage, // 195
        NoCodePage, // 196
        NoCodePage, // 197
        NoCodePage, // 198
        NoCodePage, // 199
        NoCodePage, // 200
        NoCodePage, // 201
        NoCodePage, // 202
        NoCodePage, // 203
        1251, // 204 - Russian
        NoCodePage, // 205
        NoCodePage, // 206
        NoCodePage, // 207
        NoCodePage, // 208
        NoCodePage, // 209
        NoCodePage, // 210
        NoCodePage, // 211
        NoCodePage, // 212
        NoCodePage, // 213
        NoCodePage, // 214
        NoCodePage, // 215
        NoCodePage, // 216
        NoCodePage, // 217
        NoCodePage, // 218
        NoCodePage, // 219
        NoCodePage, // 220
        NoCodePage, // 221
        874, // 222 - Thai
        NoCodePage, // 223
        NoCodePage, // 224
        NoCodePage, // 225
        NoCodePage, // 226
        NoCodePage, // 227
        NoCodePage, // 228
        NoCodePage, // 229
        NoCodePage, // 230
        NoCodePage, // 231
        NoCodePage, // 232
        NoCodePage, // 233
        NoCodePage, // 234
        NoCodePage, // 235
        NoCodePage, // 236
        NoCodePage, // 237
        1250, // 238 - Eastern European
        NoCodePage, // 239
        NoCodePage, // 240
        NoCodePage, // 241
        NoCodePage, // 242
        NoCodePage, // 243
        NoCodePage, // 244
        NoCodePage, // 245
        NoCodePage, // 246
        NoCodePage, // 247
        NoCodePage, // 248
        NoCodePage, // 249
        NoCodePage, // 250
        NoCodePage, // 251
        NoCodePage, // 252
        NoCodePage, // 253
        437, // 254 - PC 437
        850, // 255 - OEM
    ];

    #endregion

    #region Lang to code page

    private const int _maxLangNumber = 16385;
    private static readonly ushort[] _langToCodePage = InitializeLangToCodePage();

    private static ushort[] InitializeLangToCodePage()
    {
        ushort[] langToCodePage = InitializedArray(_maxLangNumber + 1, NoCodePage);

        /*
        There's a ton more languages than this, but it's not clear what code page they all translate to.
        This should be enough to get on with for now though...

        Note: 1024 is implicitly rejected by simply not being in the list, so we're all good there.
        */

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

        // Cyrillic
        langToCodePage[1049] = 1251;
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

        // Western European
        langToCodePage[1033] = 1252;

        return langToCodePage;
    }

    #endregion

    #endregion

    // Perf: On modern .NET, the "ReadOnlySpan<> x =>" pattern removes bounds checking (assuming you index with a
    // numeric type that's <= the length of the span), and generates only a tiny amount of asm. But on Framework,
    // the JIT doesn't recognize the pattern, and performance is catastrophic. So ugly ifdefs everywhere it is...
    private static readonly bool[] _isNonPlainText =
    [
        true, // '\0' (0)
        false, false, false, false, false, false, false, false, false,
        true, // '\n' (10)
        false, false,
        true, // '\r' (13)
        false, false, false, false, false, false, false, false, false, false,
        false, false, false, false, false, false, false, false, false, false,
        false, false, false, false, false, false, false, false, false, false,
        false, false, false, false, false, false, false, false, false, false,
        false, false, false, false, false, false, false, false, false, false,
        false, false, false, false, false, false, false, false, false, false,
        false, false, false, false, false, false, false, false, false, false,
        false, false, false, false, false, false, false, false,
        true, // '\\' (92)
        false, false, false, false, false, false, false, false, false, false,
        false, false, false, false, false, false, false, false, false, false,
        false, false, false, false, false, false, false, false, false, false,
        true, // '{' (123)
        false,
        true, // '}' (125)
        false, false, false, false, false, false, false, false, false, false,
        false, false, false, false, false, false, false, false, false, false,
        false, false, false, false, false, false, false, false, false, false,
        false, false, false, false, false, false, false, false, false, false,
        false, false, false, false, false, false, false, false, false, false,
        false, false, false, false, false, false, false, false, false, false,
        false, false, false, false, false, false, false, false, false, false,
        false, false, false, false, false, false, false, false, false, false,
        false, false, false, false, false, false, false, false, false, false,
        false, false, false, false, false, false, false, false, false, false,
        false, false, false, false, false, false, false, false, false, false,
        false, false, false, false, false, false, false, false, false, false,
        false, false, false, false, false, false, false, false, false, false,
    ];

    private static ReadOnlySpan<bool> _isIgnoreChar =>
    [
        true, // '\0' (0)
        false, false, false, false, false, false, false, false, false,
        true, // '\n' (10)
        false, false,
        true, // '\r' (13)
        false, false, false, false, false, false, false, false, false, false,
        false, false, false, false, false, false, false, false, false, false,
        false, false, false, false, false, false, false, false, false, false,
        false, false, false, false, false, false, false, false, false, false,
        false, false, false, false, false, false, false, false, false, false,
        false, false, false, false, false, false, false, false, false, false,
        false, false, false, false, false, false, false, false, false, false,
        false, false, false, false, false, false, false, false, false, false,
        false, false, false, false, false, false, false, false, false, false,
        false, false, false, false, false, false, false, false, false, false,
        false, false, false, false, false, false, false, false, false, false,
        false, false, false, false, false, false, false, false, false, false,
        false, false, false, false, false, false, false, false, false, false,
        false, false, false, false, false, false, false, false, false, false,
        false, false, false, false, false, false, false, false, false, false,
        false, false, false, false, false, false, false, false, false, false,
        false, false, false, false, false, false, false, false, false, false,
        false, false, false, false, false, false, false, false, false, false,
        false, false, false, false, false, false, false, false, false, false,
        false, false, false, false, false, false, false, false, false, false,
        false, false, false, false, false, false, false, false, false, false,
        false, false, false, false, false, false, false, false, false, false,
        false, false, false, false, false, false, false, false, false, false,
        false, false, false, false, false, false, false, false, false, false,
        false, false,
    ];

    #endregion

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
    /// Initializes a new instance of the <see cref="RRTF_RtfDisplayedReadmeParser"/> class.
    /// </summary>
    public RRTF_RtfDisplayedReadmeParser()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        _fontDictionary = new Dictionary<int, FontEntry>(_internalBufferDefaultCapacity);

        InitGroupStack();
    }

    [PublicAPI]
    public (bool Success, List<RtfColor>? ColorTable, List<RRTF_LangItem>? LangItems)
    GetData(in ArrayWithLength<byte> rtfBytes, bool getColorTable, bool getLangs)
    {
        try
        {
            _rtfBytes = rtfBytes.Array;
            _rtfBytesLength = rtfBytes.Length;

            #region Reset

            GroupStack_Reset();
            _fontDictionary.Clear();

            _headerCodePage = 0;
            _headerDefaultFontSet = false;
            _headerDefaultFontNum = 0;

            _skipDestinationIfUnknown = false;

            _currentPos = 0;

            _inHandleSkippableHexData = false;
            _inFontTable = false;


            _foundColorTable = false;
            _getColorTable = false;
            _getLangs = false;

            _colorTable = null;
            _langItems = null;

            _getColorTable = getColorTable;
            _getLangs = getLangs;

            #endregion

            if (!getLangs && !getColorTable)
            {
                return (false, ColorTable: _colorTable, LangItems: _langItems);
            }

            RtfError error = ParseRtf();
            if (error == RtfError.OK)
            {
                return (true, ColorTable: _colorTable, LangItems: _langItems);
            }
            else
            {
                return (false, ColorTable: _colorTable, LangItems: _langItems);
            }
        }
        catch
        {
            return (false, _colorTable, _langItems);
        }
        finally
        {
            // Reset after so we don't carry around any waste after running
            GroupStack_ResetCapacityToDefault();
            if (_fontDictionary.Count > _internalBufferDefaultCapacity)
            {
                _fontDictionary = new Dictionary<int, FontEntry>(_internalBufferDefaultCapacity);
            }

            _rtfBytes = Array.Empty<byte>();
            _rtfBytesLength = 0;
        }
    }

    #endregion

    #region Parse

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private RtfError ParseKeyword(ref byte bufferRef)
    {
        // The keyword parsers are JIT inlined now, so make sure to have only one call to each!
        if (_currentPos < _rtfBytesLength - _keywordParseMaxRequiredBytes)
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
        if (_currentPos < _rtfBytesLength - _keywordParseMaxRequiredBytes)
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
                                currentFontCodePage = param.IsBetween(0, _charSetToCodePageLength - 1)
                                    ? _charSetToCodePage[param]
                                    : _headerCodePage;
                                break;
                            }
                            case KeywordType.CPG:
                                currentFontCodePage = IsNonEmptyUShortParam(param)
                                    ? (ushort)param
                                    : _headerCodePage;
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
                        if (ShouldUseSimdFontNameCodePath())
                        {
                            _ = SIMD_SkipPlainText(ref bufferRef);
                        }

                        if (currentFontCodePage == NoCodePage)
                        {
                            currentFontCodePage = _headerCodePage;
                        }

                        _fontDictionary[currentFontNumber] = new FontEntry(currentFontCodePage);
                        currentFontNumber = NoFontNumber;
                        currentFontCodePage = NoCodePage;
                    }
                    break;
                }
            }
        }

        _inFontTable = false;
        return RtfError.OK;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool ShouldUseSimdFontNameCodePath()
    {
        return System.Numerics.Vector.IsHardwareAccelerated && _vectorLengthFitsInAByte;
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
                // @RTF: Enable later once we've de-crufted
#if false
                    if (_getLangs && !GroupStack_CurrentSkipDest)
#endif
                {
                    if (symbol.UseDefaultParam || !hasParam) param = symbol.DefaultParam;
                    ChangeProperty((Property)symbol.Index, param);
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
                return ParseFontTable(ref bufferRef);
            }
            case SpecialType.ColorTable:
                // Spec is to ignore any further color tables after the first one
                if (_getColorTable && !_foundColorTable)
                {
                    _foundColorTable = true;
                    return ParseAndBuildColorTable();
                }
                else
                {
                    _currentPos = IndexOfNextClosingBrace_ChunkAware();
                }
                break;
        }

        return RtfError.OK;
    }

    private RtfError ParseAndBuildColorTable()
    {
        ClearColorTable(RtfError.OK);

        int closingBraceIndex = Array_IndexOfByte_Fast(_rtfBytes, (byte)'}', _currentPos, _rtfBytesLength - _currentPos);
        if (closingBraceIndex == -1) return ClearColorTable(RtfError.OK);

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

        return first ? ClearColorTable(RtfError.OK) : RtfError.OK;

        RtfError ClearColorTable(RtfError error)
        {
            _colorTable = null;
            return error;
        }

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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ChangeProperty(Property propertyTableIndex, int param)
    {
        switch (propertyTableIndex)
        {
            case Property.FontNum:
            {
                // \fN supersedes \langN
                GroupStack_CurrentPropertyLang = NoLang;
                GroupStack_CurrentPropertyFontNum = param;
                break;
            }
            case Property.Lang:
            {
                int currentLang = GroupStack_CurrentPropertyLang;
                int groupFontNum = HeaderDefaultIfNotSet(GroupStack_CurrentPropertyFontNum);

                _fontDictionary.TryGetValue(groupFontNum, out FontEntry fontEntry);

                ushort headerCodePage = _headerCodePage == 0 ? (ushort)1252 : _headerCodePage;

                int currentCodePage = fontEntry.IsSet && fontEntry.CodePage != NoCodePage ? fontEntry.CodePage : headerCodePage;

                if (currentLang != NoLang && currentLang != _undefinedLanguage && param != _undefinedLanguage)
                {
                    if (param.IsBetween(0, _maxLangNumber))
                    {
                        int langCodePage = _langToCodePage[param];
                        if (langCodePage == NoCodePage)
                        {
                            _langItems ??= new List<RRTF_LangItem>();
                            _langItems.Add(new RRTF_LangItem(_currentPos, currentCodePage));
                        }
                    }
                }
                else
                {
                    if (param.IsBetween(0, _maxLangNumber))
                    {
                        int langCodePage = _langToCodePage[param];
                        if (langCodePage != NoCodePage && langCodePage != currentCodePage)
                        {
                            _langItems ??= new List<RRTF_LangItem>();
                            _langItems.Add(new RRTF_LangItem(_currentPos, langCodePage));
                        }
                    }
                }

                if (param != _undefinedLanguage)
                {
                    GroupStack_CurrentPropertyLang = IsNonEmptyUShortParam(param) ? (ushort)param : NoLang;
                }
                break;
            }
        }
    }

    #endregion

    #region Helpers

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsNonEmptyUShortParam(int value)
    {
        /*
        The whole ushort range except 0xFFFF - that's our value for "not set" (-1 equivalent). As 0xFFFF (65535)
        is not a valid codepage or lang in either the RTF spec or .NET (any version), we can hijack it for this
        purpose without issue.
        */
        return (uint)(value - ushort.MinValue) <= (ushort.MaxValue - 1) - ushort.MinValue;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int HeaderDefaultIfNotSet(int fontNum) => fontNum > NoFontNumber ? fontNum : _headerDefaultFontNum;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static int Array_IndexOfByte_Fast(byte[] array, byte value, int startIndex, int count)
    {
        /*
        On .NET, Array.IndexOf() uses crazy fast SIMD. On Framework, it normally doesn't.
        However, on Framework 64-bit only, we can make it use SIMD by using span.IndexOf(), if we reference the
        appropriate package (directly or indirectly), System.Memory or whatever it is.
        If we're 32-bit, though, SIMD is not supported, so we just stick to the regular Array.IndexOf(), which
        while substantially slower than the SIMD version, is still reasonably fast.

        But instead of checking for 64-bit vs. 32-bit, we can just check directly if SIMD is supported.
        */
        if (System.Numerics.Vector.IsHardwareAccelerated)
        {
            int index = array.AsSpan(startIndex, count).IndexOf(value);
            if (index > -1) index += startIndex;
            return index;
        }
        else
        {
            return Array.IndexOf(array, value, startIndex, count);
        }
    }

    #endregion

    #region Fast skipping

    private int IndexOfNextClosingBrace_ChunkAware()
    {
        int foundIndex = Array_IndexOfByte_Fast(_rtfBytes, (byte)'}', _currentPos, _rtfBytesLength - _currentPos);
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
                    if (index > _rtfBytesLength - _binLength ||
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

    // Pulling previously separate classes into the main class increases performance: the fewer separate classes
    // we have to reference in hot loops, the better.

    #region Buffer

    private byte[] _rtfBytes = Array.Empty<byte>();
    private int _rtfBytesLength;

    /// <summary>
    /// Manually bounds-checked past <see cref="T:_rtfBytesLength"/>.
    /// Now that we have stream support, this method should always be called for array accesses to ensure the
    /// chunks are loaded when needed. Only access <see cref="T:Array"/> directly in cases where you know for
    /// sure you don't need the chunk load triggering in your particular scenario.
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private byte GetByte(int index)
    {
        // Very unfortunately, we have to manually bounds-check here, because our array could be longer than
        // Length (such as when it comes from a pool).
        if (index > _rtfBytesLength - 1)
        {
            /*
            Putting the ThrowHelper call here makes us full speed (on the byte array path). Putting this here
            instead loses us like 6-10% again. Even though HandleOutOfBounds() has the no inlining attribute!
            Argh!
            But, this system does make us a little faster than before (especially on the streaming path), so hey.
            */
            index = HandleOutOfBounds();
        }
        return _rtfBytes[index];
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private int HandleOutOfBounds()
    {
        if (_groupStackTopIndex > 0)
        {
            ThrowHelper.UnmatchedBraceException();
        }
        else
        {
            ThrowHelper.IndexOutOfRange();
        }
        return 0;
    }

    #endregion

    #region GroupStack

    [StructLayout(LayoutKind.Auto)]
    private struct GroupStackFrame
    {
        internal bool SkipDestination;

        internal int PropFontNum;
        internal ushort PropLang;
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

    private ushort GroupStack_CurrentPropertyLang
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _groupStackFrames[_groupStackTopIndex].PropLang;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => _groupStackFrames[_groupStackTopIndex].PropLang = value;
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
            PropLang = NoLang,
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

    //private const int TOTAL_KEYWORDS = 82;
    //private const int MIN_WORD_LENGTH = 1;
    private const int MAX_WORD_LENGTH = 18;
    //private const int MIN_HASH_VALUE = 31;
    private const int MAX_HASH_VALUE = 407;
    /* maximum key range = 377, duplicates = 0 */

    private static readonly ushort[] asso_values =
    [
        408, 408, 408, 408, 408, 408, 408, 408, 408, 408,
        408, 408, 408, 408, 408, 408, 408, 408, 408, 408,
        408, 408, 408, 408, 408, 408, 408, 408, 408, 408,
        408, 408, 408, 408, 408, 408, 408, 408, 408, 408,
        408, 408, 408, 408, 408, 408, 408, 408, 408, 408,
        408, 408, 408, 408, 408, 408, 408, 408, 408, 408,
        408, 408, 408, 408, 408, 408, 408, 408, 408, 408,
        408, 408, 408, 408, 408, 408, 408, 408, 408, 408,
        408, 408, 408, 408, 408, 408, 408, 408, 408, 408,
        408, 408, 408, 408, 408, 408, 408, 25, 90, 24,
        20, 41, 116, 31, 8, 53, 90, 9, 25, 111,
        48, 84, 11, 40, 87, 79, 13, 15, 120, 97,
        19, 111, 27, 408, 408, 408, 408, 408, 408, 408,
        408, 408, 408, 408, 408, 408, 408, 408, 408, 408,
        408, 408, 408, 408, 408, 408, 408, 408, 408, 408,
        408, 408, 408, 408, 408, 408, 408, 408, 408, 408,
        408, 408, 408, 408, 408, 408, 408, 408, 408, 408,
        408, 408, 408, 408, 408, 408, 408, 408, 408, 408,
        408, 408, 408, 408, 408, 408, 408, 408, 408, 408,
        408, 408, 408, 408, 408, 408, 408, 408, 408, 408,
        408, 408, 408, 408, 408, 408, 408, 408, 408, 408,
        408, 408, 408, 408, 408, 408, 408, 408, 408, 408,
        408, 408, 408, 408, 408, 408, 408, 408, 408, 408,
        408, 408, 408, 408, 408, 408, 408, 408, 408, 408,
        408, 408, 408, 408, 408, 408, 408, 408, 408, 408,
        408, 408, 408, 408, 408, 408,
    ];

    private static readonly Symbol _fontSymbol = new("f", 0, false, KeywordType.Property, (ushort)Property.FontNum);

    private static readonly ushort[] _symbolFirstCharTable =
    [
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0x7501, 0,
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0x7002, 0, 0x7402, 0,
        0x7502, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0x6409, 0x7003, 0, 0, 0x7006, 0,
        0, 0, 0, 0x7409, 0, 0, 0, 0x6303, 0, 0, 0x7802, 0, 0x7004, 0x6807, 0, 0x6409, 0, 0, 0, 0, 0, 0, 0, 0,
        0x7403, 0, 0x6304, 0, 0, 0, 0x6506, 0, 0x7405, 0, 0x6C06, 0, 0, 0, 0x7006, 0, 0x6C04, 0, 0x700C, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0x6106, 0, 0, 0x6206, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0x7304, 0, 0, 0, 0, 0x6308,
        0x6806, 0x6807, 0x6608, 0, 0x6C04, 0, 0x7402, 0, 0, 0, 0, 0x6C08, 0, 0x6402, 0x6607, 0x6312, 0, 0x6302,
        0x6C09, 0x6506, 0x6D03, 0, 0x7206, 0x6107, 0x7203, 0, 0, 0x6606, 0x6207, 0, 0x6807, 0, 0x7007, 0, 0x6E08,
        0, 0, 0x7307, 0, 0, 0, 0x6607, 0x6104, 0, 0, 0, 0x7003, 0, 0, 0x6507, 0, 0, 0, 0, 0x7403, 0, 0, 0, 0,
        0x730A, 0, 0, 0, 0x6605, 0x6F08, 0, 0x6206, 0, 0, 0, 0, 0, 0x6307, 0, 0x7601, 0x6203, 0, 0, 0, 0x6407,
        0x7209, 0x6B08, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0x7A05, 0, 0, 0, 0, 0x7A04, 0, 0, 0x7007, 0x6307, 0,
        0x6E07, 0, 0, 0, 0, 0, 0x7107, 0x6507, 0x6607, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0x6F07,
        0x6404, 0, 0, 0, 0, 0x7A04, 0, 0, 0x6904, 0, 0x7A03, 0, 0, 0, 0, 0, 0, 0, 0, 0x6607, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0x6608, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0, 0x7206, 0, 0, 0x7203, 0, 0, 0, 0, 0, 0, 0, 0, 0x6606, 0x6607, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0x6607,
    ];

    /*
    For "listtext", "pntext"
    TODO(listtext/pntext): Temporarily disabled with a hack, but decide what we want to do here

    For "v"
    \v to make all plain text hidden (not output to the conversion stream), \v0 to make it shown again

    For "mac"
    The spec calls this "Apple Macintosh" but again says nothing about what codepage that is. I'll
    assume 10000 ("Mac Roman")

    NOTE: This is generated. Values can be modified, but not keys (keys are the first string params).
    Also no reordering. Adding, removing, reordering, or modifying keys requires generating a new version.
    See RTF_SymbolListGenSource.cs for how to generate a new version (it also contains the original
    Symbol list which must be used as the source to generate this one).
    */
    private static readonly Symbol?[] _symbolTable =
    [
        null, null, null, null, null, null, null, null, null,
        null, null, null, null, null, null, null, null, null,
        null, null, null, null, null, null, null, null, null,
        null, null, null, null,
// Entry 11
        new Symbol("u", 0, false, KeywordType.Special, (ushort)SpecialType.UnicodeChar),
        null, null, null, null, null, null, null, null, null,
        null, null, null, null, null, null, null, null, null,
        null, null, null, null, null, null, null, null, null,
        null, null,
// Entry 1
        new Symbol("pc", 437, true, KeywordType.Special, (ushort)SpecialType.HeaderCodePage),
        null,
// Entry 65
        new Symbol("tc", 0, false, KeywordType.Destination, (ushort)DestinationType.Skip),
        null,
// Entry 10
        new Symbol("uc", 1, false, KeywordType.Property, (ushort)Property.UnicodeCharSkipCount),
        null, null, null, null, null, null, null, null, null,
        null, null, null, null, null, null, null, null, null,
        null, null, null,
// Entry 74
        new Symbol("datafield", 0, false, KeywordType.Destination, (ushort)DestinationType.SkippableHex),
// Entry 3
        new Symbol("pca", 850, true, KeywordType.Special, (ushort)SpecialType.HeaderCodePage),
        null, null,
// Entry 37
        new Symbol("pntext", 0, false, KeywordType.Destination, 255),
        null, null, null, null,
// Entry 70
        new Symbol("themedata", 0, false, KeywordType.Destination, (ushort)DestinationType.SkippableHex),
        null, null, null,
// Entry 8
        new Symbol("cpg", ushort.MaxValue, false, KeywordType.CPG, 0),
        null, null,
// Entry 68
        new Symbol("xe", 0, false, KeywordType.Destination, (ushort)DestinationType.Skip),
        null,
// Entry 69
        new Symbol("pict", 1, false, KeywordType.Destination, (ushort)DestinationType.SkippableHex),
// Entry 54
        new Symbol("headerl", 0, false, KeywordType.Destination, (ushort)DestinationType.Skip),
        null,
// Entry 73
        new Symbol("datastore", 0, false, KeywordType.Destination, (ushort)DestinationType.SkippableHex),
        null, null, null, null, null, null, null, null,
// Entry 67
        new Symbol("txe", 0, false, KeywordType.Destination, (ushort)DestinationType.Skip),
        null,
// Entry 79
        new Symbol("cell", 0, false, KeywordType.Character, '\t'),
        null, null, null,
// Entry 22
        new Symbol("endash", 0, false, KeywordType.Character, '\x2013'),
        null,
// Entry 66
        new Symbol("title", 0, false, KeywordType.Destination, (ushort)DestinationType.Skip),
        null,
// Entry 23
        new Symbol("lquote", 0, false, KeywordType.Character, '\x2018'),
        null, null, null,
// Entry 77
        new Symbol("panose", 20, true, KeywordType.Special, (ushort)SpecialType.SkipNumberOfBytes),
        null,
// Entry 9
        new Symbol("lang", 0, false, KeywordType.Property, (ushort)Property.Lang),
        null,
// Entry 72
        new Symbol("passwordhash", 0, false, KeywordType.Destination, (ushort)DestinationType.SkippableHex),
        null, null, null, null, null, null, null, null, null,
        null,
// Entry 38
        new Symbol("author", 0, false, KeywordType.Destination, (ushort)DestinationType.Skip),
        null, null,
// Entry 17
        new Symbol("bullet", 0, false, KeywordType.Character, '\x2022'),
        null, null, null, null, null, null, null, null, null,
        null, null,
// Entry 15
        new Symbol("sect", 0, false, KeywordType.Character, '\n'),
        null, null, null, null,
// Entry 40
        new Symbol("colortbl", 0, false, KeywordType.Special, (ushort)SpecialType.ColorTable),
// Entry 52
        new Symbol("header", 0, false, KeywordType.Destination, (ushort)DestinationType.Skip),
// Entry 55
        new Symbol("headerr", 0, false, KeywordType.Destination, (ushort)DestinationType.Skip),
// Entry 7
        new Symbol("fcharset", ushort.MaxValue, false, KeywordType.FCharset, 0),
        null,
// Entry 14
        new Symbol("line", 0, false, KeywordType.Character, '\n'),
        null,
// Entry 35
        new Symbol("ts", 0, false, KeywordType.Destination, (ushort)DestinationType.CanBeDestOrNotDest),
        null, null, null, null,
// Entry 36
        new Symbol("listtext", 0, false, KeywordType.Destination, 255),
        null,
// Entry 34
        new Symbol("ds", 0, false, KeywordType.Destination, (ushort)DestinationType.CanBeDestOrNotDest),
// Entry 32
        new Symbol("fldinst", 0, false, KeywordType.Destination, (ushort)DestinationType.FieldInstruction),
// Entry 71
        new Symbol("colorschememapping", 0, false, KeywordType.Destination, (ushort)DestinationType.SkippableHex),
        null,
// Entry 33
        new Symbol("cs", 0, false, KeywordType.Destination, (ushort)DestinationType.CanBeDestOrNotDest),
// Entry 25
        new Symbol("ldblquote", 0, false, KeywordType.Character, '\x201C'),
// Entry 21
        new Symbol("emdash", 0, false, KeywordType.Character, '\x2014'),
// Entry 2
        new Symbol("mac", 10000, true, KeywordType.Special, (ushort)SpecialType.HeaderCodePage),
        null,
// Entry 24
        new Symbol("rquote", 0, false, KeywordType.Character, '\x2019'),
// Entry 4
        new Symbol("ansicpg", 0, false, KeywordType.Special, (ushort)SpecialType.HeaderCodePage),
// Entry 62
        new Symbol("rxe", 0, false, KeywordType.Destination, (ushort)DestinationType.Skip),
        null, null,
// Entry 50
        new Symbol("ftnsep", 0, false, KeywordType.Destination, (ushort)DestinationType.Skip),
// Entry 76
        new Symbol("blipuid", 32, true, KeywordType.Special, (ushort)SpecialType.SkipNumberOfBytes),
        null,
// Entry 53
        new Symbol("headerf", 0, false, KeywordType.Destination, (ushort)DestinationType.Skip),
        null,
// Entry 60
        new Symbol("private", 0, false, KeywordType.Destination, (ushort)DestinationType.Skip),
        null,
// Entry 81
        new Symbol("nestcell", 0, false, KeywordType.Character, '\t'),
        null, null,
// Entry 64
        new Symbol("subject", 0, false, KeywordType.Destination, (ushort)DestinationType.Skip),
        null, null, null,
// Entry 51
        new Symbol("ftnsepc", 0, false, KeywordType.Destination, (ushort)DestinationType.Skip),
// Entry 0
        new Symbol("ansi", 0, true, KeywordType.Special, (ushort)SpecialType.HeaderCodePage),
        null, null, null,
// Entry 13
        new Symbol("par", 0, false, KeywordType.Character, '\n'),
        null, null,
// Entry 19
        new Symbol("enspace", 0, false, KeywordType.Character, '\x2002'),
        null, null, null, null,
// Entry 16
        new Symbol("tab", 0, false, KeywordType.Character, '\t'),
        null, null, null, null,
// Entry 63
        new Symbol("stylesheet", 0, false, KeywordType.Destination, (ushort)DestinationType.Skip),
        null, null, null,
// Entry 49
        new Symbol("ftncn", 0, false, KeywordType.Destination, (ushort)DestinationType.Skip),
// Entry 58
        new Symbol("operator", 0, false, KeywordType.Destination, (ushort)DestinationType.Skip),
        null,
// Entry 39
        new Symbol("buptim", 0, false, KeywordType.Destination, (ushort)DestinationType.Skip),
        null, null, null, null, null,
// Entry 41
        new Symbol("comment", 0, false, KeywordType.Destination, (ushort)DestinationType.Skip),
        null,
// Entry 12
        new Symbol("v", 1, false, KeywordType.Property, (ushort)Property.Hidden),
// Entry 31
        new Symbol("bin", 0, false, KeywordType.Special, (ushort)SpecialType.SkipNumberOfBytes),
        null, null, null,
// Entry 43
        new Symbol("doccomm", 0, false, KeywordType.Destination, (ushort)DestinationType.Skip),
// Entry 26
        new Symbol("rdblquote", 0, false, KeywordType.Character, '\x201D'),
// Entry 57
        new Symbol("keywords", 0, false, KeywordType.Destination, (ushort)DestinationType.Skip),
        null, null, null, null, null, null, null, null, null,
        null, null, null,
// Entry 28
        new Symbol("zwnbo", 0, false, KeywordType.Character, '\xFEFF'),
        null, null, null, null,
// Entry 30
        new Symbol("zwnj", 0, false, KeywordType.Character, '\x200C'),
        null, null,
// Entry 59
        new Symbol("printim", 0, false, KeywordType.Destination, (ushort)DestinationType.Skip),
// Entry 42
        new Symbol("creatim", 0, false, KeywordType.Destination, (ushort)DestinationType.Skip),
        null,
// Entry 80
        new Symbol("nestrow", 0, false, KeywordType.Special, (ushort)SpecialType.CellRowEnd),
        null, null, null, null, null,
// Entry 20
        new Symbol("qmspace", 0, false, KeywordType.Character, '\x2005'),
// Entry 18
        new Symbol("emspace", 0, false, KeywordType.Character, '\x2003'),
// Entry 6
        new Symbol("fonttbl", 0, false, KeywordType.Special, (ushort)SpecialType.FontTable),
        null, null, null, null, null, null, null, null, null,
        null, null, null, null, null, null,
// Entry 75
        new Symbol("objdata", 1, false, KeywordType.Destination, (ushort)DestinationType.SkippableHex),
// Entry 5
        new Symbol("deff", 0, false, KeywordType.Special, (ushort)SpecialType.DefaultFont),
        null, null, null, null,
// Entry 27
        new Symbol("zwbo", 0, false, KeywordType.Character, '\x200B'),
        null, null,
// Entry 56
        new Symbol("info", 0, false, KeywordType.Destination, (ushort)DestinationType.Skip),
        null,
// Entry 29
        new Symbol("zwj", 0, false, KeywordType.Character, '\x200D'),
        null, null, null, null, null, null, null, null,
// Entry 46
        new Symbol("footerl", 0, false, KeywordType.Destination, (ushort)DestinationType.Skip),
        null, null, null, null, null, null, null, null, null,
        null, null, null, null, null, null, null,
// Entry 48
        new Symbol("footnote", 0, false, KeywordType.Destination, (ushort)DestinationType.Skip),
        null, null, null, null, null, null, null, null, null,
        null, null, null, null, null, null, null, null, null,
        null, null, null, null, null, null, null, null, null,
        null, null, null, null,
// Entry 61
        new Symbol("revtim", 0, false, KeywordType.Destination, (ushort)DestinationType.Skip),
        null, null,
// Entry 78
        new Symbol("row", 0, false, KeywordType.Special, (ushort)SpecialType.CellRowEnd),
        null, null, null, null, null, null, null, null,
// Entry 44
        new Symbol("footer", 0, false, KeywordType.Destination, (ushort)DestinationType.Skip),
// Entry 47
        new Symbol("footerr", 0, false, KeywordType.Destination, (ushort)DestinationType.Skip),
        null, null, null, null, null, null, null, null, null,
        null, null, null, null, null, null, null, null, null,
        null, null, null, null, null, null, null, null, null,
        null,
// Entry 45
        new Symbol("footerf", 0, false, KeywordType.Destination, (ushort)DestinationType.Skip),
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
            key += asso_values[Unsafe.AddByteOffset(ref keywordRef, 1)];
            key += asso_values[keywordRef];

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

    private static Symbol?[] InitSingleCharSymbolTable()
    {
        Symbol?[] ret = new Symbol?[256];

        ret['u'] = new Symbol("u", 0, false, KeywordType.Special, (ushort)SpecialType.UnicodeChar);
        ret['v'] = new Symbol("v", 1, false, KeywordType.Property, (ushort)Property.Hidden);

        return ret;
    }

    private static readonly Symbol?[] _singleCharSymbolTable = InitSingleCharSymbolTable();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Symbol? LookUpControlWord_LengthOne(byte firstChar)
    {
        return _singleCharSymbolTable[firstChar];
    }

    #endregion
}
