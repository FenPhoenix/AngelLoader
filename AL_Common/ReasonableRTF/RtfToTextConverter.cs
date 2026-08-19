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

/*
Notes and miscellaneous:
-Hex that combines into an actual valid character: \'81\'63
 (it's supposed to be an ellipsis - __MSG_final__FMInfo-De - Copy.rtf has an instance of it)
-Tiger face (>2 byte Unicode test): \u-9169?\u-10179?
-RichTextBox respects \v0 (hidden text) when it converts, but LibreOffice doesn't.
-RichTextBox and LibreOffice both remove nulls.

TODO: Add an option to copy HYPERLINK field instructions to output like RichTextBox does?
HYPERLINK in a fldinst can - and usually does - occur after a bunch of random cruft, unlike SYMBOL.
So we'd have to make another special parse method that when it gets to plain text it checks for HYPERLINK and
parses from there. Not a big deal but yeah. In fact we could also handle SYMBOL that way just in case.
The Framework RichTextBox doesn't seem to copy HYPERLINK text to the plaintext output. Just the .NET 8 one does
I guess. So we could just leave this out...

TODO: Handle bulleted/numbered lists properly

NOTE(Footnotes):
RichTextBox doesn't convert footnotes at all.
LibreOffice adds citation numbers but doesn't add the footnotes themselves.
*/

using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using AL_Common;
using ReasonableRTF.Enums;
using ReasonableRTF.Extensions;
using ReasonableRTF.Helper;
using ReasonableRTF.Models;
using ReasonableRTF.Models.Fonts;
using ReasonableRTF.Models.Symbols;
using static AL_Common.RTFParserCommon;

namespace ReasonableRTF;

public sealed partial class RtfToTextConverter
{
    #region Private fields

    #region Options

    private LineBreakStyle _lineBreakStyle;
    private bool _convertHiddenText;
    private ushort _defaultCodePage;

    #endregion

    // Cache it for perf
    private static readonly char[] LineBreakString = Environment.NewLine.ToCharArray();
    private static readonly int LineBreakStringLength = LineBreakString.Length;

    // +1 to allow reading one beyond the max and then checking for it to return an error
    private readonly byte[] _keyword = new byte[KeywordMaxLen + 1];

    private const int _internalBufferDefaultCapacity = 32;

    private const char _unicodeUnknown_Char = '\u25A1';

    private const int _defaultStreamBufferSize = 81920;
    private const int _maxSeekBackBytes = 8;
    private const int _minimumBufferSize = _maxSeekBackBytes * 2;

    private const int _plainTextRunFastPathAmountBackFromBufferEnd = 512;

    private readonly byte[] _SYMBOLName = "SYMBOL "u8.ToArray();
    private readonly ulong _SYMBOLKeywordAsULong = BitConverter.IsLittleEndian
        ? 0x00_20_4C_4F_42_4D_59_53ul
        : 0x53_59_4D_42_4F_4C_20_00ul;
    private readonly ulong _SYMBOLKeywordAsULong_Mask = BitConverter.IsLittleEndian
        ? 0x00_FF_FF_FF_FF_FF_FF_FFul
        : 0xFF_FF_FF_FF_FF_FF_FF_00ul;

    // Set to a length that no reasonable font name would be above, to minimize the chance of having to do a slow
    // bounds-checked read-and-throw-away of the rest of the bytes.
    private const int _maxSymbolFontNameLength = 64;

    private const int _fldinstSymbolNumberMaxLen = 10;
    private readonly char[] _fldinstSymbolNumber = new char[_fldinstSymbolNumberMaxLen + 1];

    private readonly char[] _fldinstSymbolFontName = new char[_maxSymbolFontNameLength + 1];

    private readonly byte[] _symbolFontNameBuffer = new byte[_maxSymbolFontNameLength];

    #region Tables

    #region Conversion tables

    #region Font to Unicode

    /*
    NOTE: _DON'T_ move this stuff to a separate file, or else Fw64 gets slower again. I swear this crap is making
    less and less sense.

    Many RTF files put emoji-like glyphs into text not with a Unicode character, but by just putting in a
    regular-ass single-byte char and then setting the font to Wingdings or whatever. So the letter "J"
    would show as "☺" in the Wingdings font. If we want to support this lunacy, we need conversion tables
    from known fonts to their closest Unicode mappings. So here we have them.

    These arrays MUST be of length 224, with entries starting at the codepoint for 0x20 and ending at the
    codepoint for 0xFF. That way, they can be arrays instead of dictionaries, making us smaller and faster.
    */

    private const int _symbolArraysStartingIndex = 2;
    private const int _symbolArraysLength = 9;

    private readonly uint[][] _symbolFontTables = new uint[_symbolArraysLength][];
    private readonly byte[][] _symbolFontCharsArrays = new byte[_symbolArraysLength][];
    private const int _minSupportedSymbolFontNameLength = 6;
    private const int _maxSupportedSymbolFontNameLength = 17;

    private static readonly bool[] _symbolFontNameLengths =
    [
        false, // 0
        false, // 1
        false, // 2
        false, // 3
        false, // 4
        false, // 5
        true,  // 6
        false, // 7
        true,  // 8
        true,  // 9
        false, // 10
        true,  // 11
        false, // 12
        true,  // 13
        false, // 14
        false, // 15
        false, // 16
        true,  // 17
    ];

    private void InitSymbolFontData()
    {
        // ReSharper disable RedundantExplicitArraySize
#pragma warning disable IDE0300
        _symbolFontTables[(int)SymbolFont.Symbol] = new uint[224]
        {
            ' ',
            0x0021,
            0x2200,
            0x0023,
            0x2203,
            0x0025,
            0x0026,
            0x220D,
            0x0028,
            0x0029,
            0x2217,
            0x002B,
            0x002C,
            0x2212,
            0x002E,
            0x002F,
            0x0030,
            0x0031,
            0x0032,
            0x0033,
            0x0034,
            0x0035,
            0x0036,
            0x0037,
            0x0038,
            0x0039,
            0x003A,
            0x003B,
            0x003C,
            0x003D,
            0x003E,
            0x003F,
            0x2245,
            0x0391,
            0x0392,
            0x03A7,
            0x0394,
            0x0395,
            0x03A6,
            0x0393,
            0x0397,
            0x0399,
            0x03D1,
            0x039A,
            0x039B,
            0x039C,
            0x039D,
            0x039F,
            0x03A0,
            0x0398,
            0x03A1,
            0x03A3,
            0x03A4,
            0x03A5,
            0x03C2,
            0x03A9,
            0x039E,
            0x03A8,
            0x0396,
            0x005B,
            0x2234,
            0x005D,
            0x22A5,
            0x005F,

            // Supposed to be " ‾" but closest Unicode char is "‾" (0x203E)
            0x203E,

            0x03B1,
            0x03B2,
            0x03C7,
            0x03B4,
            0x03B5,

            // Nominally lowercase phi (0x3C6), but is uppercase phi in Windows Symbol
            0x03D5,

            0x03B3,
            0x03B7,
            0x03B9,

            // Nominally uppercase phi (0x3D5), but is lowercase phi in Windows Symbol
            0x03C6,

            0x03BA,
            0x03BB,
            0x03BC,
            0x03BD,
            0x03BF,
            0x03C0,
            0x03B8,
            0x03C1,
            0x03C3,
            0x03C4,
            0x03C5,
            0x03D6,
            0x03C9,
            0x03BE,
            0x03C8,
            0x03B6,
            0x007B,
            0x007C,
            0x007D,
            0x223C,
            _unicodeUnknown_Char,
            _unicodeUnknown_Char,
            _unicodeUnknown_Char,
            _unicodeUnknown_Char,
            _unicodeUnknown_Char,
            _unicodeUnknown_Char,
            _unicodeUnknown_Char,
            _unicodeUnknown_Char,
            _unicodeUnknown_Char,
            _unicodeUnknown_Char,
            _unicodeUnknown_Char,
            _unicodeUnknown_Char,
            _unicodeUnknown_Char,
            _unicodeUnknown_Char,
            _unicodeUnknown_Char,
            _unicodeUnknown_Char,
            _unicodeUnknown_Char,
            _unicodeUnknown_Char,
            _unicodeUnknown_Char,
            _unicodeUnknown_Char,
            _unicodeUnknown_Char,
            _unicodeUnknown_Char,
            _unicodeUnknown_Char,
            _unicodeUnknown_Char,
            _unicodeUnknown_Char,
            _unicodeUnknown_Char,
            _unicodeUnknown_Char,
            _unicodeUnknown_Char,
            _unicodeUnknown_Char,
            _unicodeUnknown_Char,
            _unicodeUnknown_Char,
            _unicodeUnknown_Char,
            _unicodeUnknown_Char,

            // Euro sign, but undefined in Win10 Symbol font at least
            0x20AC,

            0x03D2,
            0x2032,
            0x2264,
            0x2044,
            0x221E,
            0x0192,
            0x2663,
            0x2666,
            0x2665,
            0x2660,
            0x2194,
            0x2190,
            0x2191,
            0x2192,
            0x2193,
            0x00B0,
            0x00B1,
            0x2033,
            0x2265,
            0x00D7,
            0x221D,
            0x2202,
            0x2022,
            0x00F7,
            0x2260,
            0x2261,
            0x2248,
            0x2026,
            0x23D0,
            0x23AF,
            0x21B5,
            0x2135,
            0x2111,
            0x211C,
            0x2118,
            0x2297,
            0x2295,
            0x2205,
            0x2229,
            0x222A,
            0x2283,
            0x2287,
            0x2284,
            0x2282,
            0x2286,
            0x2208,
            0x2209,
            0x2220,
            0x2207,

            // First set of (R), (TM), (C) (nominally serif)
            0x00AE,
            0x00A9,
            0x2122,

            0x220F,
            0x221A,
            0x22C5,
            0x00AC,
            0x2227,
            0x2228,
            0x21D4,
            0x21D0,
            0x21D1,
            0x21D2,
            0x21D3,
            0x25CA,
            0x2329,

            // Second set of (R), (TM), (C) (nominally sans-serif)
            0x00AE,
            0x00A9,
            0x2122,

            0x2211,
            0x239B,
            0x239C,
            0x239D,
            0x23A1,
            0x23A2,
            0x23A3,
            0x23A7,
            0x23A8,
            0x23A9,
            0x23AA,

            // Apple logo. Using "RED APPLE".
            0x1F34E,

            0x232A,
            0x222B,
            0x2320,
            0x23AE,
            0x2321,
            0x239E,
            0x239F,
            0x23A0,
            0x23A4,
            0x23A5,
            0x23A6,
            0x23AB,
            0x23AC,
            0x23AD,
            _unicodeUnknown_Char,
        };

        _symbolFontTables[(int)SymbolFont.Wingdings] = new uint[224]
        {
            ' ',
            0x1F589,
            0x2702,
            0x2701,
            0x1F453,
            0x1F56D,
            0x1F56E,
            0x1F56F,
            0x1F57F,
            0x2706,
            0x1F582,
            0x1F583,
            0x1F4EA,
            0x1F4EB,
            0x1F4EC,
            0x1F4ED,
            0x1F5C0,
            0x1F5C1,
            0x1F5CE,
            0x1F5CF,
            0x1F5D0,
            0x1F5C4,
            0x231B,
            0x1F5AE,
            0x1F5B0,
            0x1F5B2,
            0x1F5B3,
            0x1F5B4,
            0x1F5AB,
            0x1F5AC,
            0x2707,
            0x270D,
            0x1F58E,
            0x270C,
            0x1F58F,
            0x1F44D,
            0x1F44E,
            0x261C,
            0x261E,
            0x261D,
            0x261F,
            0x1F590,
            0x263A,
            0x1F610,
            0x2639,
            0x1F4A3,
            0x1F571,
            0x1F3F3,
            0x1F3F1,
            0x2708,
            0x263C,
            0x1F322,
            0x2744,
            0x1F546,
            0x271E,
            0x1F548,
            0x2720,
            0x2721,
            0x262A,
            0x262F,
            0x1F549,
            0x2638,
            0x2648,
            0x2649,
            0x264A,
            0x264B,
            0x264C,
            0x264D,
            0x264E,
            0x264F,
            0x2650,
            0x2651,
            0x2652,
            0x2653,
            0x1F670,
            0x1F675,
            0x26AB,
            0x1F53E,
            0x25FC,
            0x1F78F,
            0x1F790,
            0x2751,
            0x2752,
            0x1F79F,
            0x29EB,
            0x25C6,
            0x2756,
            0x2B29,
            0x2327,
            0x2BB9,
            0x2318,
            0x1F3F5,
            0x1F3F6,
            0x1F676,
            0x1F677,
            _unicodeUnknown_Char,
            0x1F10B,
            0x2780,
            0x2781,
            0x2782,
            0x2783,
            0x2784,
            0x2785,
            0x2786,
            0x2787,
            0x2788,
            0x2789,
            0x1F10C,
            0x278A,
            0x278B,
            0x278C,
            0x278D,
            0x278E,
            0x278F,
            0x2790,
            0x2791,
            0x2792,
            0x2793,
            0x1F662,
            0x1F660,
            0x1F661,
            0x1F663,
            0x1F65E,
            0x1F65C,
            0x1F65D,
            0x1F65F,
            0x2219,
            0x2022,
            0x2B1D,
            0x2B58,
            0x1F786,
            0x1F788,
            0x1F78A,
            0x1F78B,
            0x1F53F,
            0x25AA,
            0x1F78E,
            0x1F7C1,
            0x1F7C5,
            0x2605,
            0x1F7CB,
            0x1F7CF,
            0x1F7D3,
            0x1F7D1,
            0x2BD0,
            0x2316,
            0x2BCE,
            0x2BCF,
            0x2BD1,
            0x272A,
            0x2730,
            0x1F550,
            0x1F551,
            0x1F552,
            0x1F553,
            0x1F554,
            0x1F555,
            0x1F556,
            0x1F557,
            0x1F558,
            0x1F559,
            0x1F55A,
            0x1F55B,
            0x2BB0,
            0x2BB1,
            0x2BB2,
            0x2BB3,
            0x2BB4,
            0x2BB5,
            0x2BB6,
            0x2BB7,
            0x1F66A,
            0x1F66B,
            0x1F655,
            0x1F654,
            0x1F657,
            0x1F656,
            0x1F650,
            0x1F651,
            0x1F652,
            0x1F653,
            0x232B,
            0x2326,
            0x2B98,
            0x2B9A,
            0x2B99,
            0x2B9B,
            0x2B88,
            0x2B8A,
            0x2B89,
            0x2B8B,
            0x1F868,
            0x1F86A,
            0x1F869,
            0x1F86B,
            0x1F86C,
            0x1F86D,
            0x1F86F,
            0x1F86E,
            0x1F878,
            0x1F87A,
            0x1F879,
            0x1F87B,
            0x1F87C,
            0x1F87D,
            0x1F87F,
            0x1F87E,
            0x21E6,
            0x21E8,
            0x21E7,
            0x21E9,
            0x2B04,
            0x21F3,
            0x2B01,
            0x2B00,
            0x2B03,
            0x2B02,
            0x1F8AC,
            0x1F8AD,
            0x1F5F6,
            0x2713,
            0x1F5F7,
            0x1F5F9,

            // Windows logo. Using "WINDOW".
            0x1FA9F,
        };

        _symbolFontTables[(int)SymbolFont.Wingdings2] = new uint[224]
        {
            ' ',
            0x1F58A,
            0x1F58B,
            0x1F58C,
            0x1F58D,
            0x2704,
            0x2700,
            0x1F57E,
            0x1F57D,
            0x1F5C5,
            0x1F5C6,
            0x1F5C7,
            0x1F5C8,
            0x1F5C9,
            0x1F5CA,
            0x1F5CB,
            0x1F5CC,
            0x1F5CD,
            0x1F4CB,
            0x1F5D1,
            0x1F5D4,
            0x1F5B5,
            0x1F5B6,
            0x1F5B7,
            0x1F5B8,
            0x1F5AD,
            0x1F5AF,
            0x1F5B1,
            0x1F592,
            0x1F593,
            0x1F598,
            0x1F599,
            0x1F59A,
            0x1F59B,
            0x1F448,
            0x1F449,
            0x1F59C,
            0x1F59D,
            0x1F59E,
            0x1F59F,
            0x1F5A0,
            0x1F5A1,
            0x1F446,
            0x1F447,
            0x1F5A2,
            0x1F5A3,
            0x1F591,
            0x1F5F4,
            0x1F5F8,
            0x1F5F5,
            0x2611,
            0x2BBD,
            0x2612,
            0x2BBE,
            0x2BBF,
            0x1F6C7,
            0x29B8,
            0x1F671,
            0x1F674,
            0x1F672,
            0x1F673,
            0x203D,
            0x1F679,
            0x1F67A,
            0x1F67B,
            0x1F666,
            0x1F664,
            0x1F665,
            0x1F667,
            0x1F65A,
            0x1F658,
            0x1F659,
            0x1F65B,
            0x24EA,
            0x2460,
            0x2461,
            0x2462,
            0x2463,
            0x2464,
            0x2465,
            0x2466,
            0x2467,
            0x2468,
            0x2469,
            0x24FF,
            0x2776,
            0x2777,
            0x2778,
            0x2779,
            0x277A,
            0x277B,
            0x277C,
            0x277D,
            0x277E,
            0x277F,
            _unicodeUnknown_Char,
            0x2609,
            0x1F315,
            0x263D,
            0x263E,
            0x2E3F,
            0x271D,
            0x1F547,
            0x1F55C,
            0x1F55D,
            0x1F55E,
            0x1F55F,
            0x1F560,
            0x1F561,
            0x1F562,
            0x1F563,
            0x1F564,
            0x1F565,
            0x1F566,
            0x1F567,
            0x1F668,
            0x1F669,
            0x22C5,
            0x1F784,
            0x2981,
            0x25CF,
            0x25CB,
            0x1F785,
            0x1F787,
            0x1F789,
            0x2299,
            0x29BF,
            0x1F78C,
            0x1F78D,
            0x25FE,
            0x25A0,
            0x25A1,
            0x1F791,
            0x1F792,
            0x1F793,
            0x1F794,
            0x25A3,
            0x1F795,
            0x1F796,
            0x1F797,
            0x1F798,
            0x2B29,
            0x2B25,
            0x25C7,
            0x1F79A,
            0x25C8,
            0x1F79B,
            0x1F79C,
            0x1F79D,
            0x1F79E,
            0x2B2A,
            0x2B27,
            0x25CA,
            0x1F7A0,
            0x25D6,
            0x25D7,
            0x2BCA,
            0x2BCB,
            0x2BC0,
            0x2BC1,
            0x2B1F,
            0x2BC2,
            0x2B23,
            0x2B22,
            0x2BC3,
            0x2BC4,
            0x1F7A1,
            0x1F7A2,
            0x1F7A3,
            0x1F7A4,
            0x1F7A5,
            0x1F7A6,
            0x1F7A7,
            0x1F7A8,
            0x1F7A9,
            0x1F7AA,
            0x1F7AB,
            0x1F7AC,
            0x1F7AD,
            0x1F7AE,
            0x1F7AF,
            0x1F7B0,
            0x1F7B1,
            0x1F7B2,
            0x1F7B3,
            0x1F7B4,
            0x1F7B5,
            0x1F7B6,
            0x1F7B7,
            0x1F7B8,
            0x1F7B9,
            0x1F7BA,
            0x1F7BB,
            0x1F7BC,
            0x1F7BD,
            0x1F7BE,
            0x1F7BF,
            0x1F7C0,
            0x1F7C2,
            0x1F7C4,
            0x1F7C6,
            0x1F7C9,
            0x1F7CA,
            0x2736,
            0x1F7CC,
            0x1F7CE,
            0x1F7D0,
            0x1F7D2,
            0x2739,
            0x1F7C3,
            0x1F7C7,
            0x272F,
            0x1F7CD,
            0x1F7D4,
            0x2BCC,
            0x2BCD,
            0x203B,
            0x2042,
            _unicodeUnknown_Char,
            _unicodeUnknown_Char,
            _unicodeUnknown_Char,
            _unicodeUnknown_Char,
            _unicodeUnknown_Char,
            _unicodeUnknown_Char,
        };

        _symbolFontTables[(int)SymbolFont.Wingdings3] = new uint[224]
        {
            ' ',
            0x2B60,
            0x2B62,
            0x2B61,
            0x2B63,
            0x2B66,
            0x2B67,
            0x2B69,
            0x2B68,
            0x2B70,
            0x2B72,
            0x2B71,
            0x2B73,
            0x2B76,
            0x2B78,
            0x2B7B,
            0x2B7D,
            0x2B64,
            0x2B65,
            0x2B6A,
            0x2B6C,
            0x2B6B,
            0x2B6D,
            0x2B4D,
            0x2BA0,
            0x2BA1,
            0x2BA2,
            0x2BA3,
            0x2BA4,
            0x2BA5,
            0x2BA6,
            0x2BA7,
            0x2B90,
            0x2B91,
            0x2B92,
            0x2B93,
            0x2B80,
            0x2B83,
            0x2B7E,
            0x2B7F,
            0x2B84,
            0x2B86,
            0x2B85,
            0x2B87,
            0x2B8F,
            0x2B8D,
            0x2B8E,
            0x2B8C,
            0x2B6E,
            0x2B6F,
            0x238B,
            0x2324,
            0x2303,
            0x2325,
            0x2423,
            0x237D,
            0x21EA,
            0x2BB8,
            0x1F8A0,
            0x1F8A1,
            0x1F8A2,
            0x1F8A3,
            0x1F8A4,
            0x1F8A5,
            0x1F8A6,
            0x1F8A7,
            0x1F8A8,
            0x1F8A9,
            0x1F8AA,
            0x1F8AB,
            0x1F850,
            0x1F852,
            0x1F851,
            0x1F853,
            0x1F854,
            0x1F855,
            0x1F857,
            0x1F856,
            0x1F858,
            0x1F859,
            0x25B2,
            0x25BC,
            0x25B3,
            0x25BD,
            0x25C0,
            0x25B6,
            0x25C1,
            0x25B7,
            0x25E3,
            0x25E2,
            0x25E4,
            0x25E5,
            0x1F780,
            0x1F782,
            0x1F781,
            _unicodeUnknown_Char,
            0x1F783,
            0x2BC5,
            0x2BC6,
            0x2BC7,
            0x2BC8,
            0x2B9C,
            0x2B9E,
            0x2B9D,
            0x2B9F,
            0x1F810,
            0x1F812,
            0x1F811,
            0x1F813,
            0x1F814,
            0x1F816,
            0x1F815,
            0x1F817,
            0x1F818,
            0x1F81A,
            0x1F819,
            0x1F81B,
            0x1F81C,
            0x1F81E,
            0x1F81D,
            0x1F81F,
            0x1F800,
            0x1F802,
            0x1F801,
            0x1F803,
            0x1F804,
            0x1F806,
            0x1F805,
            0x1F807,
            0x1F808,
            0x1F80A,
            0x1F809,
            0x1F80B,
            0x1F820,
            0x1F822,
            0x1F824,
            0x1F826,
            0x1F828,
            0x1F82A,
            0x1F82C,
            0x1F89C,
            0x1F89D,
            0x1F89E,
            0x1F89F,
            0x1F82E,
            0x1F830,
            0x1F832,
            0x1F834,
            0x1F836,
            0x1F838,
            0x1F83A,
            0x1F839,
            0x1F83B,
            0x1F898,
            0x1F89A,
            0x1F899,
            0x1F89B,
            0x1F83C,
            0x1F83E,
            0x1F83D,
            0x1F83F,
            0x1F840,
            0x1F842,
            0x1F841,
            0x1F843,
            0x1F844,
            0x1F846,
            0x1F845,
            0x1F847,
            0x2BA8,
            0x2BA9,
            0x2BAA,
            0x2BAB,
            0x2BAC,
            0x2BAD,
            0x2BAE,
            0x2BAF,
            0x1F860,
            0x1F862,
            0x1F861,
            0x1F863,
            0x1F864,
            0x1F865,
            0x1F867,
            0x1F866,
            0x1F870,
            0x1F872,
            0x1F871,
            0x1F873,
            0x1F874,
            0x1F875,
            0x1F877,
            0x1F876,
            0x1F880,
            0x1F882,
            0x1F881,
            0x1F883,
            0x1F884,
            0x1F885,
            0x1F887,
            0x1F886,
            0x1F890,
            0x1F892,
            0x1F891,
            0x1F893,
            0x1F894,
            0x1F896,
            0x1F895,
            0x1F897,
            _unicodeUnknown_Char,
            _unicodeUnknown_Char,
            _unicodeUnknown_Char,
            _unicodeUnknown_Char,
            _unicodeUnknown_Char,
            _unicodeUnknown_Char,
            _unicodeUnknown_Char,
            _unicodeUnknown_Char,
            _unicodeUnknown_Char,
            _unicodeUnknown_Char,
            _unicodeUnknown_Char,
            _unicodeUnknown_Char,
            _unicodeUnknown_Char,
            _unicodeUnknown_Char,
            _unicodeUnknown_Char,
        };

        _symbolFontTables[(int)SymbolFont.Webdings] = new uint[224]
        {
            ' ',
            0x1F577,
            0x1F578,
            0x1F572,
            0x1F576,
            0x1F3C6,
            0x1F396,
            0x1F587,
            0x1F5E8,
            0x1F5E9,
            0x1F5F0,
            0x1F5F1,
            0x1F336,
            0x1F397,
            0x1F67E,
            0x1F67C,
            0x1F5D5,
            0x1F5D6,
            0x1F5D7,
            0x23F4,
            0x23F5,
            0x23F6,
            0x23F7,
            0x23EA,
            0x23E9,
            0x23EE,
            0x23ED,
            0x23F8,
            0x23F9,
            0x23FA,
            0x1F5DA,
            0x1F5F3,
            0x1F6E0,
            0x1F3D7,
            0x1F3D8,
            0x1F3D9,
            0x1F3DA,
            0x1F3DC,
            0x1F3ED,
            0x1F3DB,
            0x1F3E0,
            0x1F3D6,
            0x1F3DD,
            0x1F6E3,
            0x1F50D,
            0x1F3D4,
            0x1F441,
            0x1F442,
            0x1F3DE,
            0x1F3D5,
            0x1F6E4,
            0x1F3DF,
            0x1F6F3,
            0x1F56C,
            0x1F56B,
            0x1F568,
            0x1F508,
            0x1F394,
            0x1F395,
            0x1F5EC,
            0x1F67D,
            0x1F5ED,
            0x1F5EA,
            0x1F5EB,
            0x2B94,
            0x2714,
            0x1F6B2,
            0x2B1C,
            0x1F6E1,
            0x1F381,
            0x1F6F1,
            0x2B1B,
            0x1F691,
            0x1F6C8,
            0x1F6E9,
            0x1F6F0,
            0x1F7C8,
            0x1F574,
            0x2B24,
            0x1F6E5,
            0x1F694,
            0x1F5D8,
            0x1F5D9,
            0x2753,
            0x1F6F2,
            0x1F687,
            0x1F68D,
            0x1F6A9,
            0x29B8,
            0x2296,
            0x1F6AD,
            0x1F5EE,
            0x23D0,
            0x1F5EF,
            0x1F5F2,

            _unicodeUnknown_Char,

            0x1F6B9,
            0x1F6BA,
            0x1F6C9,
            0x1F6CA,
            0x1F6BC,
            0x1F47D,
            0x1F3CB,
            0x26F7,
            0x1F3C2,
            0x1F3CC,
            0x1F3CA,
            0x1F3C4,
            0x1F3CD,
            0x1F3CE,
            0x1F698,
            0x1F4C8,
            0x1F6E2,
            0x1F4B0,
            0x1F3F7,
            0x1F4B3,
            0x1F46A,
            0x1F5E1,
            0x1F5E2,
            0x1F5E3,
            0x272F,
            0x1F584,
            0x1F585,
            0x1F583,
            0x1F586,
            0x1F5B9,
            0x1F5BA,
            0x1F5BB,
            0x1F575,
            0x1F570,
            0x1F5BD,
            0x1F5BE,
            0x1F4CB,
            0x1F5D2,
            0x1F5D3,
            0x1F56E,
            0x1F4DA,
            0x1F5DE,
            0x1F5DF,
            0x1F5C3,
            0x1F4C7,
            0x1F5BC,
            0x1F3AD,
            0x1F39C,
            0x1F398,
            0x1F399,
            0x1F3A7,
            0x1F4BF,
            0x1F39E,
            0x1F4F7,
            0x1F39F,
            0x1F3AC,
            0x1F4FD,
            0x1F4F9,
            0x1F4FE,
            0x1F4FB,
            0x1F39A,
            0x1F39B,
            0x1F4FA,
            0x1F4BB,
            0x1F5A5,
            0x1F5A6,
            0x1F5A7,
            0x1F579,
            0x1F3AE,
            0x1F57B,
            0x1F57C,
            0x1F4DF,
            0x1F581,
            0x1F580,
            0x1F5A8,
            0x1F5A9,
            0x1F5BF,
            0x1F5AA,
            0x1F5DC,
            0x1F512,
            0x1F513,
            0x1F5DD,
            0x1F4E5,
            0x1F4E4,
            0x1F573,
            0x1F323,
            0x1F324,
            0x1F325,
            0x1F326,
            0x2601,
            0x1F328,
            0x1F327,
            0x1F329,
            0x1F32A,
            0x1F32C,
            0x1F32B,
            0x1F31C,
            0x1F321,
            0x1F6CB,
            0x1F6CF,
            0x1F37D,
            0x1F378,
            0x1F6CE,
            0x1F6CD,
            0x24C5,
            0x267F,
            0x1F6C6,
            0x1F588,
            0x1F393,
            0x1F5E4,
            0x1F5E5,
            0x1F5E6,
            0x1F5E7,
            0x1F6EA,
            0x1F43F,
            0x1F426,
            0x1F41F,
            0x1F415,
            0x1F408,
            0x1F66C,
            0x1F66E,
            0x1F66D,
            0x1F66F,
            0x1F5FA,
            0x1F30D,
            0x1F30F,
            0x1F30E,
            0x1F54A,
        };

        _symbolFontTables[(int)SymbolFont.ITCZapfDingbats] = new uint[224]
        {
            ' ',
            0x2701,
            0x2702,
            0x2703,
            0x2704,
            0x260E,
            0x2706,
            0x2707,
            0x2708,
            0x2709,
            0x261B,
            0x261E,
            0x270C,
            0x270D,
            0x270E,
            0x270F,
            0x2710,
            0x2711,
            0x2712,
            0x2713,
            0x2714,
            0x2715,
            0x2716,
            0x2717,
            0x2718,
            0x2719,
            0x271A,
            0x271B,
            0x271C,
            0x271D,
            0x271E,
            0x271F,
            0x2720,
            0x2721,
            0x2722,
            0x2723,
            0x2724,
            0x2725,
            0x2726,
            0x2727,
            0x2605,
            0x2729,
            0x272A,
            0x272B,
            0x272C,
            0x272D,
            0x272E,
            0x272F,
            0x2730,
            0x2731,
            0x2732,
            0x2733,
            0x2734,
            0x2735,
            0x2736,
            0x2737,
            0x2738,
            0x2739,
            0x273A,
            0x273B,
            0x273C,
            0x273D,
            0x273E,
            0x273F,
            0x2740,
            0x2741,
            0x2742,
            0x2743,
            0x2744,
            0x2745,
            0x2746,
            0x2747,
            0x2748,
            0x2749,
            0x274A,
            0x274B,
            0x25CF,
            0x274D,
            0x25A0,
            0x274F,
            0x2750,
            0x2751,
            0x2752,
            0x25B2,
            0x25BC,
            0x25C6,
            0x2756,
            0x25D7,
            0x2758,
            0x2759,
            0x275A,
            0x275B,
            0x275C,
            0x275D,
            0x275E,
            _unicodeUnknown_Char,
            0x2768,
            0x2769,
            0x276A,
            0x276B,
            0x276C,
            0x276D,
            0x276E,
            0x276F,
            0x2770,
            0x2771,
            0x2772,
            0x2773,
            0x2774,
            0x2775,
            _unicodeUnknown_Char,
            _unicodeUnknown_Char,
            _unicodeUnknown_Char,
            _unicodeUnknown_Char,
            _unicodeUnknown_Char,
            _unicodeUnknown_Char,
            _unicodeUnknown_Char,
            _unicodeUnknown_Char,
            _unicodeUnknown_Char,
            _unicodeUnknown_Char,
            _unicodeUnknown_Char,
            _unicodeUnknown_Char,
            _unicodeUnknown_Char,
            _unicodeUnknown_Char,
            _unicodeUnknown_Char,
            _unicodeUnknown_Char,
            _unicodeUnknown_Char,
            _unicodeUnknown_Char,
            _unicodeUnknown_Char,
            0x2761,
            0x2762,
            0x2763,
            0x2764,
            0x2765,
            0x2766,
            0x2767,
            0x2663,
            0x2666,
            0x2665,
            0x2660,
            0x2460,
            0x2461,
            0x2462,
            0x2463,
            0x2464,
            0x2465,
            0x2466,
            0x2467,
            0x2468,
            0x2469,
            0x2776,
            0x2777,
            0x2778,
            0x2779,
            0x277A,
            0x277B,
            0x277C,
            0x277D,
            0x277E,
            0x277F,
            0x2780,
            0x2781,
            0x2782,
            0x2783,
            0x2784,
            0x2785,
            0x2786,
            0x2787,
            0x2788,
            0x2789,
            0x278A,
            0x278B,
            0x278C,
            0x278D,
            0x278E,
            0x278F,
            0x2790,
            0x2791,
            0x2792,
            0x2793,
            0x2794,
            0x2192,
            0x2194,
            0x2195,
            0x2798,
            0x2799,
            0x279A,
            0x279B,
            0x279C,
            0x279D,
            0x279E,
            0x279F,
            0x27A0,
            0x27A1,
            0x27A2,
            0x27A3,
            0x27A4,
            0x27A5,
            0x27A6,
            0x27A7,
            0x27A8,
            0x27A9,
            0x27AA,
            0x27AB,
            0x27AC,
            0x27AD,
            0x27AE,
            0x27AF,
            _unicodeUnknown_Char,
            0x27B1,
            0x27B2,
            0x27B3,
            0x27B4,
            0x27B5,
            0x27B6,
            0x27B7,
            0x27B8,
            0x27B9,
            0x27BA,
            0x27BB,
            0x27BC,
            0x27BD,
            0x27BE,
            _unicodeUnknown_Char,
        };

        _symbolFontTables[(int)SymbolFont.ZapfDingbats] = _symbolFontTables[(int)SymbolFont.ITCZapfDingbats];

#pragma warning restore IDE0300
        // ReSharper restore RedundantExplicitArraySize

        _symbolFontCharsArrays[(int)SymbolFont.Symbol] = "Symbol"u8.ToArray();
        _symbolFontCharsArrays[(int)SymbolFont.Wingdings] = "Wingdings"u8.ToArray();
        _symbolFontCharsArrays[(int)SymbolFont.Wingdings2] = "Wingdings 2"u8.ToArray();
        _symbolFontCharsArrays[(int)SymbolFont.Wingdings3] = "Wingdings 3"u8.ToArray();
        _symbolFontCharsArrays[(int)SymbolFont.Webdings] = "Webdings"u8.ToArray();
        _symbolFontCharsArrays[(int)SymbolFont.ITCZapfDingbats] = "ITC Zapf Dingbats"u8.ToArray();
        _symbolFontCharsArrays[(int)SymbolFont.ZapfDingbats] = "Zapf Dingbats"u8.ToArray();

        InitSymbolFontNameVectors();
    }

    #endregion

    #endregion

    private static readonly bool[] _isSeparatorChar =
    [
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

    private Stream? _bufferedStream;

    private bool _reachedEndOfStream;

    private int _leadingBufferByteCount;

    private bool _skipDestinationIfUnknown;

    private int _currentPos;
    private int _chunksRead;

    private bool _inHandleSkippableHexData;

    /*
    From the spec:
    "Occasionally Word writes SYMBOL_CHARSET (nonUnicode) characters in the range U+F020..U+F0FF instead
    of U+0020..U+00FF. Internally Word uses the values U+F020..U+F0FF for these characters so that plain-
    text searches don't mistakenly match SYMBOL_CHARSET characters when searching for Unicode characters
    in the range U+0020..U+00FF. To find out the correct symbol font to use, e.g., Wingdings, Symbol,
    etc., find the last SYMBOL_CHARSET font control word \fN used, look up font N in the font table and
    find the face name. The charset is specified by the \fcharsetN control word and SYMBOL_CHARSET is for
    N = 2. This corresponds to codepage 42."

    However, there's also a weird quirk with the "RichEdit50W" version of the Windows RichEdit control, which is
    that fonts that were set in a non-destination group above us ALSO count as potentially "last used". In other
    words, these fonts leak right out of their stack frames. So that means we have to globally track the last set
    font whose codepage is 42.

    However, this quirk appears to ONLY happen with the "RichEdit50W" version of the Windows RichEdit control
    (it doesn't happen with LibreOffice or Microsoft Word 2010 or "RichEdit20W"). So we just have to decide whose
    expectations we're going to match. We're going with RichEdit's behavior for now.

    TODO: If we wanted to support this "properly", we would actually need another field in the group stack to
    track codepage 42 fonts, because it says "last SYMBOL_CHARSET font control \fN used", which I take to mean
    not "the last font used and if it's not codepage 42 then quit", but rather "the last codepage 42 font used
    even if it's not the last font used". Which means we either keep track of codepage 42 fonts in the stack, or
    we just search backward in the stack for the last used font when we need it.
    The group stack frame is currently 13 bytes, so another 4 bytes for another font number puts us one byte over
    the 16-byte boundary and we'd be up to 20. We would have to see if that's worse or if a linear stack search
    is worse.
    Unless the spec doesn't mean what I interpret it as. I'd have to test that too.
    * Update: Nope, it does mean "the last font used and if it's not codepage 42 then quit", apparently. At least
      that's how Word 2010 and LibreOffice both treat it. So, we wouldn't have to search the stack or add a field
      or anything.
    */
    private int _lastUsedFontWithCodePage42 = NoFontNumber;

    private bool _inFontTable;

    #endregion

    #region Reusable buffers

    private readonly byte[] _byteBuffer1 = new byte[1];
    private readonly byte[] _byteBuffer4 = new byte[4];

    #endregion

    #region Cached encodings

    // DON'T reset this. We want to build up a dictionary of encodings and amortize it over the entire list
    // of RTF files.
    private Dictionary<ushort, Encoding> _encodings;

    #endregion

    private readonly RtfToTextConverterOptions _defaultOptions;

    #endregion

    #region Public API

    /// <summary>
    /// Initializes a new instance of the <see cref="RtfToTextConverter"/> class.
    /// </summary>
    public RtfToTextConverter()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        _defaultOptions = new RtfToTextConverterOptions();

        InitSymbolFontData();

        _fontDictionary = new Dictionary<int, FontEntry>(_internalBufferDefaultCapacity);

        _encodings = new Dictionary<ushort, Encoding>(_internalBufferDefaultCapacity);

        InitGroupStack();
    }

    #region Byte array

    /// <summary>
    /// Converts a byte array of RTF data into plain text.
    /// </summary>
    /// <param name="source">The byte array containing the RTF to convert.</param>
    /// <returns>An <see cref="RtfResult"/> containing the converted plain text, or error information if the conversion was not successful.</returns>
    public RtfResult Convert(byte[] source)
    {
        return ConvertInternal(source, source.Length, _defaultOptions, null, _defaultStreamBufferSize);
    }

    /// <summary>
    /// Converts a byte array of RTF data into plain text.
    /// </summary>
    /// <param name="source">The byte array containing the RTF to convert.</param>
    /// <param name="options">A set of options.</param>
    /// <returns>An <see cref="RtfResult"/> containing the converted plain text, or error information if the conversion was not successful.</returns>
    public RtfResult Convert(byte[] source, RtfToTextConverterOptions options)
    {
        return ConvertInternal(source, source.Length, options, null, _defaultStreamBufferSize);
    }

    /// <summary>
    /// Converts a byte array of RTF data into plain text.
    /// </summary>
    /// <param name="source">The byte array containing the RTF to convert.</param>
    /// <param name="length">The maximum number of bytes to read from the RTF byte array.</param>
    /// <returns>An <see cref="RtfResult"/> containing the converted plain text, or error information if the conversion was not successful.</returns>
    /// <exception cref="ArgumentException"></exception>
    public RtfResult Convert(byte[] source, int length)
    {
        UtilHelper.ValidateArgs(source, length);
        return ConvertInternal(source, length, _defaultOptions, null, _defaultStreamBufferSize);
    }

    /// <summary>
    /// Converts a byte array of RTF data into plain text.
    /// </summary>
    /// <param name="source">The byte array containing the RTF to convert.</param>
    /// <param name="length">The maximum number of bytes to read from the RTF byte array.</param>
    /// <param name="options">A set of options.</param>
    /// <returns>An <see cref="RtfResult"/> containing the converted plain text, or error information if the conversion was not successful.</returns>
    /// <exception cref="ArgumentException"></exception>
    public RtfResult Convert(byte[] source, int length, RtfToTextConverterOptions options)
    {
        UtilHelper.ValidateArgs(source, length);
        return ConvertInternal(source, length, options, null, _defaultStreamBufferSize);
    }

    #endregion

    #region Filename

    /// <summary>
    /// Converts an RTF file into plain text.
    /// </summary>
    /// <param name="fileName">The path to the RTF file to convert.</param>
    /// <returns>An <see cref="RtfResult"/> containing the converted plain text, or error information if the conversion was not successful.</returns>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="PathTooLongException"></exception>
    /// <exception cref="DirectoryNotFoundException"></exception>
    /// <exception cref="UnauthorizedAccessException"></exception>
    /// <exception cref="FileNotFoundException"></exception>
    /// <exception cref="NotSupportedException"></exception>
    /// <exception cref="IOException"></exception>
    public RtfResult Convert(string fileName)
    {
        using FileStream stream = File.OpenRead(fileName);
        return ConvertInternal(Array.Empty<byte>(), 0, _defaultOptions, stream, _defaultStreamBufferSize);
    }

    /// <summary>
    /// Converts an RTF file into plain text.
    /// </summary>
    /// <param name="fileName">The path to the RTF file to convert.</param>
    /// <param name="bufferSize">The size of the buffer to use during streaming. The default value is 81920.</param>
    /// <returns>An <see cref="RtfResult"/> containing the converted plain text, or error information if the conversion was not successful.</returns>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="PathTooLongException"></exception>
    /// <exception cref="DirectoryNotFoundException"></exception>
    /// <exception cref="UnauthorizedAccessException"></exception>
    /// <exception cref="FileNotFoundException"></exception>
    /// <exception cref="NotSupportedException"></exception>
    /// <exception cref="IOException"></exception>
    public RtfResult Convert(string fileName, int bufferSize)
    {
        using FileStream stream = File.OpenRead(fileName);
        return ConvertInternal(Array.Empty<byte>(), 0, _defaultOptions, stream, bufferSize);
    }

    /// <summary>
    /// Converts an RTF file into plain text.
    /// </summary>
    /// <param name="fileName">The path to the RTF file to convert.</param>
    /// <param name="options">A set of options.</param>
    /// <returns>An <see cref="RtfResult"/> containing the converted plain text, or error information if the conversion was not successful.</returns>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="PathTooLongException"></exception>
    /// <exception cref="DirectoryNotFoundException"></exception>
    /// <exception cref="UnauthorizedAccessException"></exception>
    /// <exception cref="FileNotFoundException"></exception>
    /// <exception cref="NotSupportedException"></exception>
    /// <exception cref="IOException"></exception>
    public RtfResult Convert(string fileName, RtfToTextConverterOptions options)
    {
        using FileStream stream = File.OpenRead(fileName);
        return ConvertInternal(Array.Empty<byte>(), 0, options, stream, _defaultStreamBufferSize);
    }

    /// <summary>
    /// Converts an RTF file into plain text.
    /// </summary>
    /// <param name="fileName">The path to the RTF file to convert.</param>
    /// <param name="bufferSize">The size of the buffer to use during streaming. The default value is 81920.</param>
    /// <param name="options">A set of options.</param>
    /// <returns>An <see cref="RtfResult"/> containing the converted plain text, or error information if the conversion was not successful.</returns>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="PathTooLongException"></exception>
    /// <exception cref="DirectoryNotFoundException"></exception>
    /// <exception cref="UnauthorizedAccessException"></exception>
    /// <exception cref="FileNotFoundException"></exception>
    /// <exception cref="NotSupportedException"></exception>
    /// <exception cref="IOException"></exception>
    public RtfResult Convert(string fileName, int bufferSize, RtfToTextConverterOptions options)
    {
        using FileStream stream = File.OpenRead(fileName);
        return ConvertInternal(Array.Empty<byte>(), 0, options, stream, bufferSize);
    }

    #endregion

    #region Stream

    /// <summary>
    /// Converts a stream of RTF data into plain text.
    /// </summary>
    /// <param name="stream">A stream containing the RTF to convert.</param>
    /// <returns>An <see cref="RtfResult"/> containing the converted plain text, or error information if the conversion was not successful.</returns>
    public RtfResult Convert(Stream stream)
    {
        return ConvertInternal(Array.Empty<byte>(), 0, _defaultOptions, stream, _defaultStreamBufferSize);
    }

    /// <summary>
    /// Converts a stream of RTF data into plain text.
    /// </summary>
    /// <param name="stream">A stream containing the RTF to convert.</param>
    /// <param name="bufferSize">The size of the buffer to use during streaming. The default value is 81920.</param>
    /// <returns>An <see cref="RtfResult"/> containing the converted plain text, or error information if the conversion was not successful.</returns>
    public RtfResult Convert(Stream stream, int bufferSize)
    {
        return ConvertInternal(Array.Empty<byte>(), 0, _defaultOptions, stream, bufferSize);
    }

    /// <summary>
    /// Converts a stream of RTF data into plain text.
    /// </summary>
    /// <param name="stream">A stream containing the RTF to convert.</param>
    /// <param name="options">A set of options.</param>
    /// <returns>An <see cref="RtfResult"/> containing the converted plain text, or error information if the conversion was not successful.</returns>
    public RtfResult Convert(Stream stream, RtfToTextConverterOptions options)
    {
        return ConvertInternal(Array.Empty<byte>(), 0, options, stream, _defaultStreamBufferSize);
    }

    /// <summary>
    /// Converts a stream of RTF data into plain text.
    /// </summary>
    /// <param name="stream">A stream containing the RTF to convert.</param>
    /// <param name="bufferSize">The size of the buffer to use during streaming. The default value is 81920.</param>
    /// <param name="options">A set of options.</param>
    /// <returns>An <see cref="RtfResult"/> containing the converted plain text, or error information if the conversion was not successful.</returns>
    public RtfResult Convert(Stream stream, int bufferSize, RtfToTextConverterOptions options)
    {
        return ConvertInternal(Array.Empty<byte>(), 0, options, stream, bufferSize);
    }

    #endregion

    /// <summary>
    /// Resets all buffers back to default capacity, releasing excess memory.
    /// </summary>
    public void ResetMemory()
    {
        GroupStack_ResetCapacityToDefault();
        PlainText_ResetCapacityToDefault();
        HexBuffer_ResetCapacityToDefault();
        UnicodeBuffer_ResetCapacityToDefault();
        _fontDictionary = new Dictionary<int, FontEntry>(_internalBufferDefaultCapacity);
        _encodings = new Dictionary<ushort, Encoding>(_internalBufferDefaultCapacity);
    }

    #endregion

    private RtfResult ConvertInternal(byte[] bytes, int bytesLength, RtfToTextConverterOptions options, Stream? bufferedStream, int bufferSize)
    {
        try
        {
            if (bufferedStream == null)
            {
                _buffer = bytes;
                SetBufferLength(bytesLength);
                _leadingBufferByteCount = 0;
            }
            else
            {
                _bufferedStream = bufferedStream;
                bufferSize = Math.Max(bufferSize, _minimumBufferSize);
                _buffer = ArrayPool<byte>.Shared.Rent(bufferSize);
                SetBufferLength(bufferSize);
                _leadingBufferByteCount = _maxSeekBackBytes;
            }

            SetOptions(options);

            #region Reset

            _reachedEndOfStream = false;

            GroupStack_Reset();
            _fontDictionary.Clear();

            _headerCodePage = 0;
            _headerDefaultFontSet = false;
            _headerDefaultFontNum = 0;

            _skipDestinationIfUnknown = false;

            _chunksRead = 0;
            _currentPos = _leadingBufferByteCount;

            _inHandleSkippableHexData = false;
            _inFontTable = false;

            _lastUsedFontWithCodePage42 = NoFontNumber;

            _hexBuffer_Count = 0;
            _unicodeBuffer_Count = 0;
            _plainText_Count = 0;

            #endregion

            if (_bufferedStream != null)
            {
                LoadNextChunkIntoBuffer();
            }
            else
            {
                _chunksRead = 1;
            }

            RtfError error = ParseRtf();
            if (error == RtfError.OK)
            {
                return new RtfResult(new string(_plainText, 0, _plainText_Count));
            }
            else
            {
                RtfResult ret = new(error, GetCurrentOverallPos(), null);
#if DEBUG
                System.Diagnostics.Trace.WriteLine(ret);
#endif
                return ret;
            }
        }
        catch (IndexOutOfRangeException ex)
        {
            return new RtfResult(RtfError.UnexpectedEndOfFile, GetCurrentOverallPos(), ex);
        }
        catch (EndOfStreamException ex)
        {
            return new RtfResult(RtfError.UnexpectedEndOfFile, GetCurrentOverallPos(), ex);
        }
        catch (UnmatchedBraceException ex)
        {
            return new RtfResult(RtfError.UnmatchedBrace, GetCurrentOverallPos(), ex);
        }
        catch (Exception ex)
        {
            return new RtfResult(RtfError.UnexpectedError, GetCurrentOverallPos(), ex);
        }
        finally
        {
            if (_bufferedStream != null)
            {
                ArrayPool<byte>.Shared.Return(_buffer);
            }
            _buffer = Array.Empty<byte>();
            _bufferedStream = null;
        }
    }

    #region Parse

    private RtfError ParseRtf()
    {
        // Avoid bounds checks by passing a buffer reference everywhere. We do our own bounds checking.
        ref byte bufferRef = ref GetArrayDataReference(_buffer);
        ref bool isNonPlainTextCharRef = ref GetArrayDataReference(IsNonPlainText);
        ref bool isIgnoreCharRef = ref MemoryMarshal.GetReference(IsIgnoreChar);

        while (!_reachedEndOfStream)
        {
            while (_currentPos < _currentBufferChunkLength)
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
                    default:
                    {
                        if (!Unsafe.AddByteOffset(ref isIgnoreCharRef, (nint)ch) &&
                            !GroupStack_CurrentSkipDest &&
                            !GroupStack_CurrentPropertyHidden)
                        {
                            // No measurable perf loss from this, and it lets us avoid duplicating the loop body.
                            char currentChar = (char)(_currentPos < _currentBufferChunkLength
                                ? GetByteAtPos(ref bufferRef, _currentPos)
                                : GetByte(_currentPos));

                            if (Unsafe.AddByteOffset(ref isNonPlainTextCharRef, (nint)currentChar))
                            {
                                SymbolFont symbolFont = GroupStack_CurrentSymbolFont;
                                if (symbolFont > SymbolFont.None)
                                {
                                    AddCharFromConversionList((byte)ch, _symbolFontTables[(int)symbolFont]);
                                }
                                else
                                {
                                    PlainText_Add(ch);
                                }
                            }
                            else
                            {
                                HandlePlainTextRun(ref bufferRef);
                            }
                        }
                        break;
                    }
                }
            }

            if (_bufferedStream != null) { HandleOutOfBounds(); } else { break; }
        }

        return _groupStackTopIndex > 0 ? RtfError.UnmatchedBrace : RtfError.OK;
    }

    private void HandlePlainTextRun(ref byte bufferRef)
    {
        _currentPos--;

        SymbolFont symbolFont = GroupStack_CurrentSymbolFont;
        if (symbolFont > SymbolFont.None)
        {
            uint[] table = _symbolFontTables[(int)symbolFont];
            while (!_reachedEndOfStream)
            {
                while (_currentPos < _currentBufferChunkLength)
                {
                    char ch = (char)GetByteAtCurrentPosAndIncrement(ref bufferRef);
                    if (!IsNonPlainText[(byte)ch])
                    {
                        AddCharFromConversionList((byte)ch, table);
                    }
                    else
                    {
                        _currentPos--;
                        return;
                    }
                }

                if (_bufferedStream != null) { HandleOutOfBounds(); } else { break; }
            }
        }
        else
        {
            if (System.Numerics.Vector.IsHardwareAccelerated)
            {
                bool finishedOnNonPlainTextChar = SIMD_CopyPlainText(ref bufferRef);

                if (finishedOnNonPlainTextChar)
                {
                    return;
                }
            }

            if (_currentPos < (_currentBufferChunkLength - 1) - _plainTextRunFastPathAmountBackFromBufferEnd &&
                _plainText_Count < (_plainText_Capacity - _plainTextRunFastPathAmountBackFromBufferEnd) - 1)
            {
                char[] plainText = _plainText;
                for (int i = 0; i < _plainTextRunFastPathAmountBackFromBufferEnd; i++)
                {
                    char ch = (char)GetByteAtCurrentPosAndIncrement(ref bufferRef);
                    if (!IsNonPlainText[(byte)ch])
                    {
                        plainText[_plainText_Count++] = ch;
                    }
                    else
                    {
                        _currentPos--;
                        return;
                    }
                }
            }

            if (System.Numerics.Vector.IsHardwareAccelerated)
            {
                // Break out of the scalar loop at the buffer boundary, so that if the plaintext run continues
                // after the next buffer load, we'll be able to jump back into a SIMD parse.
                while (_currentPos < _currentBufferChunkLength)
                {
                    char ch = (char)GetByteAtCurrentPosAndIncrement(ref bufferRef);
                    if (!IsNonPlainText[(byte)ch])
                    {
                        PlainText_Add(ch);
                    }
                    else
                    {
                        _currentPos--;
                        return;
                    }
                }
            }
            else
            {
                while (!_reachedEndOfStream)
                {
                    while (_currentPos < _currentBufferChunkLength)
                    {
                        char ch = (char)GetByteAtCurrentPosAndIncrement(ref bufferRef);
                        if (!IsNonPlainText[(byte)ch])
                        {
                            PlainText_Add(ch);
                        }
                        else
                        {
                            _currentPos--;
                            return;
                        }
                    }

                    if (_bufferedStream != null) { HandleOutOfBounds(); } else { break; }
                }
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private RtfError ParseKeyword(ref byte bufferRef)
    {
        // The keyword parsers are JIT inlined now, so make sure to have only one call to each!

        if (_currentPos < _currentBufferChunkLength - KeywordParseMaxRequiredBytes)
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
        if (_currentPos < _currentBufferChunkLength - KeywordParseMaxRequiredBytes)
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

        while (!_reachedEndOfStream)
        {
            while (_currentPos < _currentBufferChunkLength)
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
                            if (_fontDictionary.TryGetValue(defaultFontNum, out FontEntry fontEntry))
                            {
                                SymbolFont symbolFont = fontEntry.SymbolFont;
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
                                        _groupStackFrames[i].SymbolFont = symbolFont;
                                    }
                                    else
                                    {
                                        break;
                                    }
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
                            SymbolFont currentFontSymbolFont = ShouldUseSimdFontNameCodePath()
                                ? SIMD_TryGetFontName(ref bufferRef, ch)
                                : GetSymbolFont_Scalar(ref bufferRef, ch);

                            if (currentFontCodePage == NoCodePage)
                            {
                                currentFontCodePage = _headerCodePage;
                            }

                            _fontDictionary[currentFontNumber] = new FontEntry(currentFontCodePage, currentFontSymbolFont);
                            currentFontNumber = NoFontNumber;
                            currentFontCodePage = NoCodePage;
                        }
                        break;
                    }
                }
            }

            if (_bufferedStream != null) { HandleOutOfBounds(); } else { break; }
        }

        _inFontTable = false;
        return RtfError.OK;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool ShouldUseSimdFontNameCodePath()
    {
        return System.Numerics.Vector.IsHardwareAccelerated && VectorLengthFitsInAByte;
    }

    private SymbolFont GetSymbolFont_Scalar(ref byte bufferRef, char ch, int symbolFontNameCountStart = 0)
    {
        int symbolFontNameCount;
        bool isNonSemicolonSeparatorChar = false;
        if (_currentPos < _currentBufferChunkLength - (_maxSymbolFontNameLength + 1))
        {
            for (symbolFontNameCount = symbolFontNameCountStart;
                 symbolFontNameCount < _maxSymbolFontNameLength &&
                 ch != ';' &&
                 !(isNonSemicolonSeparatorChar = IsNonPlainText[(byte)ch]);
                 symbolFontNameCount++, ch = (char)GetByteAtCurrentPosAndIncrement(ref bufferRef))
            {
                _symbolFontNameBuffer[symbolFontNameCount] = (byte)ch;
            }
        }
        else
        {
            for (symbolFontNameCount = symbolFontNameCountStart;
                 symbolFontNameCount < _maxSymbolFontNameLength &&
                 ch != ';' &&
                 !(isNonSemicolonSeparatorChar = IsNonPlainText[(byte)ch]);
                 symbolFontNameCount++, ch = (char)GetByte(IncrementCurrentPos()))
            {
                _symbolFontNameBuffer[symbolFontNameCount] = (byte)ch;
            }
        }

        if (symbolFontNameCount == _maxSymbolFontNameLength)
        {
            while (ch != ';' && !(isNonSemicolonSeparatorChar = IsNonPlainText[(byte)ch]))
            {
                ch = (char)GetByte(IncrementCurrentPos());
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

        for (int i = _symbolArraysStartingIndex; i < _symbolArraysLength; i++)
        {
            byte[] nameBytes = _symbolFontCharsArrays[i];
            if (FontName_SeqEqual(_symbolFontNameBuffer, nameBytes, symbolFontNameCount))
            {
                return (SymbolFont)i;
            }
        }

        return SymbolFont.None;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static bool FontName_SeqEqual(byte[] first, byte[] second, int firstLength)
        {
            if (firstLength != second.Length) return false;

            for (int i = 0; i < firstLength; i++)
            {
                if (first[i] != second[i]) return false;
            }

            return true;
        }
    }

    #endregion

    #region Act on keywords

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private RtfError DispatchControlSymbol(ref byte bufferRef, char symbol)
    {
        if (GroupStack_CurrentSkipDest || GroupStack_CurrentPropertyHidden || _inFontTable)
        {
            return RtfError.OK;
        }

        if (symbol == '\'')
        {
            HandleHexRun(ref bufferRef);
        }
        else
        {
            AddChar_Explicit(symbol);
        }

        return RtfError.OK;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private RtfError DispatchKeyword(ref byte bufferRef, Symbol symbol, int param, bool hasParam)
    {
        if (!GroupStack_CurrentSkipDest)
        {
            switch (symbol.KeywordType)
            {
                case KeywordType.Property:
                    if (symbol.UseDefaultParam || !hasParam) param = symbol.DefaultParam;
                    ChangeProperty((Property)symbol.Index, param);
                    return RtfError.OK;
                case KeywordType.Character:
                    if (!GroupStack_CurrentPropertyHidden)
                    {
                        AddChar_Explicit((char)symbol.Index);
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
                        case DestinationType.FieldInstruction:
                            return HandleFieldInstruction(ref bufferRef);
                        // Stupid crazy type of control word, see description for enum field
                        case DestinationType.CanBeDestOrNotDest:
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
            case SpecialType.HexEncodedChar:
                if (!GroupStack_CurrentSkipDest && !GroupStack_CurrentPropertyHidden && !_inFontTable)
                {
                    HandleHexRun(ref bufferRef);
                }
                break;
            case SpecialType.SkipNumberOfBytes:
                if (symbol.UseDefaultParam) param = symbol.DefaultParam;
                if (param < 0) return RtfError.AbortedForSafety;
                IncrementCurrentPos_ArbitraryAmountForward(param);
                break;
            case SpecialType.UnicodeChar:
            {
                HandleUnicodeParamAndSkipFallbackChars(ref bufferRef, param);
                return HandleUnicodeRun(ref bufferRef);
            }
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
                _currentPos = IndexOfNextClosingBrace_ChunkAware();
                break;
            case SpecialType.CellRowEnd:
                // Quick and dirty hack - remove trailing cell separator char from the end of the last cell in a row
                if (!GroupStack_CurrentPropertyHidden &&
                    _plainText_Count > 0 &&
                    _plainText[_plainText_Count - 1] == '\t')
                {
                    _plainText_Count--;
                    AddLineBreak();
                }
                break;
        }

        return RtfError.OK;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ChangeProperty(Property propertyTableIndex, int param)
    {
        switch (propertyTableIndex)
        {
            case Property.FontNum:
            {
                if (_fontDictionary.TryGetValue(param, out FontEntry fontEntry))
                {
                    if (fontEntry.CodePage == 42)
                    {
                        _lastUsedFontWithCodePage42 = param;
                    }

                    GroupStack_CurrentSymbolFont = fontEntry.SymbolFont;
                }
                // \fN supersedes \langN
                GroupStack_CurrentPropertyLang = NoLang;
                GroupStack_CurrentPropertyFontNum = param;
                break;
            }
            case Property.Lang:
            {
                if (param != UndefinedLanguage)
                {
                    GroupStack_CurrentPropertyLang = IsNonEmptyUShortParam(param) ? (ushort)param : NoLang;
                }
                break;
            }
            case Property.Hidden:
            {
                if (!_convertHiddenText)
                {
                    GroupStack_CurrentPropertyHidden = param > 0;
                }
                break;
            }
            default:
                GroupStack_CurrentPropertyUnicodeCharSkipCount = param;
                break;
        }
    }

    #endregion

    #region Handle specially encoded characters

    #region Hex

    private void HandleHexRun(ref byte bufferRef)
    {
        _hexBuffer_Count = 0;

        ushort codePage = GetCurrentCodePage();

        byte byte1;
        byte byte2;

        if (_currentPos < _currentBufferChunkLength - 1)
        {
            byte1 = GetByteAtCurrentPosAndIncrement(ref bufferRef);
            byte2 = GetByteAtCurrentPosAndIncrement(ref bufferRef);
        }
        else
        {
            byte1 = GetByte(IncrementCurrentPos());
            byte2 = GetByte(IncrementCurrentPos());
        }

        if (codePage == 42)
        {
            SymbolFont symbolFont = GroupStack_CurrentSymbolFont;
            if (symbolFont == SymbolFont.None) symbolFont = SymbolFont.Symbol;
            uint[] symbolFontTable = _symbolFontTables[(int)symbolFont];

            AddHexByteToPlainText_SymbolFont(byte1, byte2, symbolFontTable);

            // TODO: Manually duplicated code for performance - should be automated if possible
            while (_currentPos < _currentBufferChunkLength - 3)
            {
                byte b = GetByteAtCurrentPosAndIncrement(ref bufferRef);
                if (b == (byte)'\\')
                {
                    b = GetByteAtCurrentPosAndIncrement(ref bufferRef);
                    if (b == (byte)'\'')
                    {
                        byte1 = GetByteAtCurrentPosAndIncrement(ref bufferRef);
                        byte2 = GetByteAtCurrentPosAndIncrement(ref bufferRef);
                        AddHexByteToPlainText_SymbolFont(byte1, byte2, symbolFontTable);
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

            while (!_reachedEndOfStream)
            {
                byte b = GetByte(IncrementCurrentPos());
                if (b == (byte)'\\')
                {
                    b = GetByte(IncrementCurrentPos());
                    if (b == (byte)'\'')
                    {
                        byte1 = GetByte(IncrementCurrentPos());
                        byte2 = GetByte(IncrementCurrentPos());
                        AddHexByteToPlainText_SymbolFont(byte1, byte2, symbolFontTable);
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
        else if (_sbcsToUtf16Dict.TryGetValue(codePage, out char[]? sbcsMappingTable))
        {
            AddHexByteToPlainText_SBCS(byte1, byte2, sbcsMappingTable);

            // TODO: Manually duplicated code for performance - should be automated if possible
            while (_currentPos < _currentBufferChunkLength - 3)
            {
                byte b = GetByteAtCurrentPosAndIncrement(ref bufferRef);
                if (b == (byte)'\\')
                {
                    b = GetByteAtCurrentPosAndIncrement(ref bufferRef);
                    if (b == (byte)'\'')
                    {
                        byte1 = GetByteAtCurrentPosAndIncrement(ref bufferRef);
                        byte2 = GetByteAtCurrentPosAndIncrement(ref bufferRef);
                        AddHexByteToPlainText_SBCS(byte1, byte2, sbcsMappingTable);
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

            while (!_reachedEndOfStream)
            {
                byte b = GetByte(IncrementCurrentPos());
                if (b == (byte)'\\')
                {
                    b = GetByte(IncrementCurrentPos());
                    if (b == (byte)'\'')
                    {
                        byte1 = GetByte(IncrementCurrentPos());
                        byte2 = GetByte(IncrementCurrentPos());
                        AddHexByteToPlainText_SBCS(byte1, byte2, sbcsMappingTable);
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
        else
        {
            AddByteToHexBuffer(byte1, byte2);

            // TODO: Manually duplicated code for performance - should be automated if possible
            while (_currentPos < _currentBufferChunkLength - 3)
            {
                byte b = GetByteAtCurrentPosAndIncrement(ref bufferRef);
                if (b == (byte)'\\')
                {
                    b = GetByteAtCurrentPosAndIncrement(ref bufferRef);
                    if (b == (byte)'\'')
                    {
                        byte1 = GetByteAtCurrentPosAndIncrement(ref bufferRef);
                        byte2 = GetByteAtCurrentPosAndIncrement(ref bufferRef);
                        AddByteToHexBuffer(byte1, byte2);
                    }
                    else
                    {
                        _currentPos -= 2;
                        AddHexBuffer(codePage);
                        return;
                    }
                }
                // Spaces end a hex run, but linebreaks don't.
                else if (b is not (byte)'\r' and not (byte)'\n')
                {
                    _currentPos--;
                    AddHexBuffer(codePage);
                    return;
                }
            }

            while (!_reachedEndOfStream)
            {
                byte b = GetByte(IncrementCurrentPos());
                if (b == (byte)'\\')
                {
                    b = GetByte(IncrementCurrentPos());
                    if (b == (byte)'\'')
                    {
                        byte1 = GetByte(IncrementCurrentPos());
                        byte2 = GetByte(IncrementCurrentPos());
                        AddByteToHexBuffer(byte1, byte2);
                    }
                    else
                    {
                        _currentPos -= 2;
                        AddHexBuffer(codePage);
                        return;
                    }
                }
                // Spaces end a hex run, but linebreaks don't.
                else if (b is not (byte)'\r' and not (byte)'\n')
                {
                    _currentPos--;
                    AddHexBuffer(codePage);
                    return;
                }
            }
        }
    }

    private void AddHexBuffer(ushort codePage)
    {
        // If multiple hex chars are directly after another (eg. \'81\'63) then they may be representing one
        // multibyte character (or not, they may also just be two single-byte chars in a row). To deal with
        // this, we have to put all contiguous hex chars into a buffer and when the run ends, we just pass
        // the buffer to the current encoding's byte-to-char decoder and get our correct result.

        // DON'T try to combine this byte with the next one if we're on code page 42 (symbol font translation) -
        // then we're guaranteed to be single-byte, and combining won't give a correct result
        if (codePage == 42)
        {
            SymbolFont symbolFont = GroupStack_CurrentSymbolFont;
            if (symbolFont == SymbolFont.None) symbolFont = SymbolFont.Symbol;

            uint[] symbolFontTable = _symbolFontTables[(int)symbolFont];
            for (int i = 0; i < _hexBuffer_Count; i++)
            {
                AddCharFromConversionList(_hexBuffer[i], symbolFontTable);
            }
        }
        else
        {
            DecodeAndCopyBytesIntoPlainText(codePage, _hexBuffer, _hexBuffer_Count);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void AddByteToHexBuffer(byte byte1, byte byte2)
    {
        /*
        Other readers' behavior:
        -RichTextBox fails the whole read on invalid hex.
        -LibreOffice just skips invalid hex chars.

        We're going to match LibreOffice here.
        */
        byte hexNibble1 = CharExtension.CharToHexLookup[byte1];
        byte hexNibble2 = CharExtension.CharToHexLookup[byte2];
        // Reject null bytes
        if ((hexNibble1 | hexNibble2).IsBetween(1, 0xFE))
        {
            byte finalHexByte = (byte)((hexNibble1 << 4) + hexNibble2);
            HexBuffer_Add(finalHexByte);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void AddHexByteToPlainText_SymbolFont(byte byte1, byte byte2, uint[] symbolFontTable)
    {
        /*
        Other readers' behavior:
        -RichTextBox fails the whole read on invalid hex.
        -LibreOffice just skips invalid hex chars.

        We're going to match LibreOffice here.
        */
        byte hexNibble1 = CharExtension.CharToHexLookup[byte1];
        byte hexNibble2 = CharExtension.CharToHexLookup[byte2];
        // Reject null bytes
        if ((hexNibble1 | hexNibble2).IsBetween(1, 0xFE))
        {
            byte finalHexByte = (byte)((hexNibble1 << 4) + hexNibble2);
            AddCharFromConversionList(finalHexByte, symbolFontTable);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void AddHexByteToPlainText_SBCS(byte byte1, byte byte2, char[] sbcsMappingTable)
    {
        /*
        Other readers' behavior:
        -RichTextBox fails the whole read on invalid hex.
        -LibreOffice just skips invalid hex chars.

        We're going to match LibreOffice here.
        */
        byte hexNibble1 = CharExtension.CharToHexLookup[byte1];
        byte hexNibble2 = CharExtension.CharToHexLookup[byte2];
        // Reject null bytes
        if ((hexNibble1 | hexNibble2).IsBetween(1, 0xFE))
        {
            byte finalHexByte = (byte)((hexNibble1 << 4) + hexNibble2);

            ref char mappingsRef = ref GetArrayDataReference(sbcsMappingTable);
            ref char plainTextRef = ref PlainText_EnsureCapacityAndGetRef(_plainText_Count + 1);

            char c = Unsafe.Add(ref mappingsRef, (nint)finalHexByte);
            Unsafe.Add(ref plainTextRef, (nint)_plainText_Count) = c;
            _plainText_Count += 1;
        }
    }

    #endregion

    #region Unicode

    private RtfError HandleUnicodeRun(ref byte bufferRef)
    {
        // TODO: Manually duplicated code for performance - should be automated if possible
        while (_currentPos < (_currentBufferChunkLength - (3 + ParamMaxLen + 1 + 1)))
        {
            char ch = (char)GetByteAtCurrentPosAndIncrement(ref bufferRef);
            if (ch == '\\')
            {
                ch = (char)GetByteAtCurrentPosAndIncrement(ref bufferRef);

                if (ch == 'u')
                {
                    ch = (char)GetByteAtCurrentPosAndIncrement(ref bufferRef);

                    int negateParam = 0;
                    if (ch == '-')
                    {
                        negateParam = 1;
                        ch = (char)GetByteAtCurrentPosAndIncrement(ref bufferRef);
                    }
                    if (ch.IsAsciiNumeric())
                    {
                        int param = 0;

                        checked
                        {
                            try
                            {
                                int i;
                                for (i = 0;
                                     i < ParamMaxLen + 1 && ch.IsAsciiNumeric();
                                     i++, ch = (char)GetByteAtCurrentPosAndIncrement(ref bufferRef))
                                {
                                    param = (param * 10) + (ch - '0');
                                }
                                if (i > ParamMaxLen)
                                {
                                    return RtfError.ParameterOutOfRange;
                                }
                            }
                            catch (OverflowException)
                            {
                                return RtfError.ParameterOutOfRange;
                            }
                        }
                        if (negateParam == 1) param = -param;

                        /*
                        From the spec:
                        "As with all RTF keywords, a keyword-terminating space may be present (before the ANSI
                        characters) that is not counted in the characters to skip."
                        */
                        if (ch != ' ') --_currentPos;
                        HandleUnicodeParamAndSkipFallbackChars(ref bufferRef, param);
                    }
                    else
                    {
                        _currentPos -= (3 + negateParam);
                        if (ch != ' ') --_currentPos;
                        ParseUnicode();
                        return RtfError.OK;
                    }
                }
                else
                {
                    _currentPos -= 2;
                    ParseUnicode();
                    return RtfError.OK;
                }
            }
            else
            {
                _currentPos--;
                ParseUnicode();
                return RtfError.OK;
            }
        }

        while (!_reachedEndOfStream)
        {
            char ch = (char)GetByte(IncrementCurrentPos());
            if (ch == '\\')
            {
                ch = (char)GetByte(IncrementCurrentPos());

                if (ch == 'u')
                {
                    ch = (char)GetByte(IncrementCurrentPos());

                    int negateParam = 0;
                    if (ch == '-')
                    {
                        negateParam = 1;
                        ch = (char)GetByte(IncrementCurrentPos());
                    }
                    if (ch.IsAsciiNumeric())
                    {
                        int param = 0;

                        checked
                        {
                            try
                            {
                                int i;
                                for (i = 0;
                                     i < ParamMaxLen + 1 && ch.IsAsciiNumeric();
                                     i++, ch = (char)GetByte(IncrementCurrentPos()))
                                {
                                    param = (param * 10) + (ch - '0');
                                }
                                if (i > ParamMaxLen)
                                {
                                    return RtfError.ParameterOutOfRange;
                                }
                            }
                            catch (OverflowException)
                            {
                                return RtfError.ParameterOutOfRange;
                            }
                        }
                        if (negateParam == 1) param = -param;

                        /*
                        From the spec:
                        "As with all RTF keywords, a keyword-terminating space may be present (before the ANSI
                        characters) that is not counted in the characters to skip."
                        */
                        if (ch != ' ') --_currentPos;
                        HandleUnicodeParamAndSkipFallbackChars(ref bufferRef, param);
                    }
                    else
                    {
                        _currentPos -= (3 + negateParam);
                        if (ch != ' ') --_currentPos;
                        ParseUnicode();
                        return RtfError.OK;
                    }
                }
                else
                {
                    _currentPos -= 2;
                    ParseUnicode();
                    return RtfError.OK;
                }
            }
            else
            {
                _currentPos--;
                ParseUnicode();
                return RtfError.OK;
            }
        }

        return RtfError.OK;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void AddCodePointToUnicodeBuffer(uint codePoint)
    {
        // If our code point has been through a font translation table, it may be longer than 2 bytes.
        if (codePoint > char.MaxValue)
        {
            if (((codePoint - 0x110000u) ^ 0xD800u) < 0xFFEF0800u)
            {
                UnicodeBuffer_Add(_unicodeUnknown_Char);
            }
            else
            {
                UnicodeBuffer_EnsureCapacity(_unicodeBuffer_Count + 2);
                _unicodeBuffer[_unicodeBuffer_Count] = (char)((codePoint + ((0xD800u - 0x40u) << 10)) >> 10);
                _unicodeBuffer[_unicodeBuffer_Count + 1] = (char)((codePoint & 0x3FFu) + 0xDC00u);
                _unicodeBuffer_Count += 2;
            }
        }
        else
        {
            UnicodeBuffer_Add((char)codePoint);
        }
    }

    private void HandleUnicodeParamAndSkipFallbackChars(ref byte bufferRef, int param)
    {
        // Make sure the code point is normalized before adding it to the buffer!
        uint codePoint = NormalizeUnicodeCodePoint(param);
        AddCodePointToUnicodeBuffer(codePoint);

        /*
        The spec states that, for the purposes of Unicode fallback character skipping, a "character" could mean
        any of the following:
        -An actual single character
        -A hex-encoded character (\'hh)
        -An entire control word
        -A \binN word, the space after it, and all of its binary data

        However, the Windows RichEdit control only counts raw chars and hex-encoded chars, so it doesn't conform
        to spec fully here. This is actually really fortunate, because ignoring the thorny "entire control word
        including bin and its data" thing means we get simpler and faster.
        */
        int numToSkip = GroupStack_CurrentPropertyUnicodeCharSkipCount;

        // TODO: Manually duplicated code for performance - should be automated if possible
        while (numToSkip > 0 && _currentPos < _currentBufferChunkLength - 5)
        {
            char c = (char)GetByteAtCurrentPosAndIncrement(ref bufferRef);
            switch (c)
            {
                case '\\':
                    if (GetByteAtCurrentPosAndIncrement(ref bufferRef) == '\'')
                    {
                        byte b = GetByteAtCurrentPosAndIncrement(ref bufferRef);
                        if (!CharExtension.IsAsciiHexDigit((char)b))
                        {
                            _currentPos--;
                            break;
                        }
                        b = GetByteAtCurrentPosAndIncrement(ref bufferRef);
                        if (!CharExtension.IsAsciiHexDigit((char)b))
                        {
                            _currentPos -= 2;
                            break;
                        }
                        numToSkip--;
                    }
                    else if (_isSeparatorChar[GetByteAtCurrentPosAndIncrement(ref bufferRef)])
                    {
                        _ = GetByteAtCurrentPosAndIncrement(ref bufferRef);
                        numToSkip--;
                    }
                    else
                    {
                        numToSkip--;
                    }
                    break;
                case '\r' or '\n':
                    break;
                // Per spec, if we encounter a group delimiter during Unicode skipping, we end skipping early
                case '{' or '}':
                    _currentPos--;
                    return;
                default:
                    numToSkip--;
                    break;
            }
        }

        while (numToSkip > 0 && !_reachedEndOfStream)
        {
            char c = (char)GetByte(IncrementCurrentPos());
            switch (c)
            {
                case '\\':
                    if (GetByte(IncrementCurrentPos()) == '\'')
                    {
                        byte b = GetByte(IncrementCurrentPos());
                        if (!CharExtension.IsAsciiHexDigit((char)b))
                        {
                            _currentPos--;
                            break;
                        }
                        b = GetByte(IncrementCurrentPos());
                        if (!CharExtension.IsAsciiHexDigit((char)b))
                        {
                            _currentPos -= 2;
                            break;
                        }
                        numToSkip--;
                    }
                    else if (_isSeparatorChar[GetByte(_currentPos)])
                    {
                        _ = GetByte(IncrementCurrentPos());
                        numToSkip--;
                    }
                    else
                    {
                        numToSkip--;
                    }
                    break;
                case '\r' or '\n':
                    break;
                // Per spec, if we encounter a group delimiter during Unicode skipping, we end skipping early
                case '{' or '}':
                    _currentPos--;
                    return;
                default:
                    numToSkip--;
                    break;
            }
        }
    }

    private void ParseUnicode()
    {
        char[] unicodeBuffer = _unicodeBuffer;
        int unicodeBufferCount = _unicodeBuffer_Count;

        #region Handle surrogate pairs and fix up bad Unicode

        for (int i = 0; i < unicodeBufferCount; i++)
        {
            char c = unicodeBuffer[i];

            if (char.IsHighSurrogate(c))
            {
                if (i < unicodeBufferCount - 1 && char.IsLowSurrogate(unicodeBuffer[i + 1]))
                {
                    i++;
                }
                else
                {
                    unicodeBuffer[i] = _unicodeUnknown_Char;
                }
            }
            else if (char.IsLowSurrogate(c) || char.GetUnicodeCategory(c) is UnicodeCategory.OtherNotAssigned)
            {
                unicodeBuffer[i] = _unicodeUnknown_Char;
            }
        }

        #endregion

        if (!GroupStack_CurrentPropertyHidden && !GroupStack_CurrentSkipDest && !_inFontTable)
        {
            if (unicodeBufferCount == 1)
            {
                char ch = unicodeBuffer[0];
                if (ch != '\0')
                {
                    PlainText_Add(ch);
                }
            }
            else
            {
                PlainText_EnsureCapacity(_plainText_Count + unicodeBufferCount);
                int plainTextIndex = _plainText_Count;
                for (int unicodeBufferIndex = 0; unicodeBufferIndex < unicodeBufferCount; unicodeBufferIndex++)
                {
                    char ch = unicodeBuffer[unicodeBufferIndex];
                    if (ch != '\0')
                    {
                        _plainText[plainTextIndex] = ch;
                        plainTextIndex++;
                    }
                }

                _plainText_Count = plainTextIndex;
            }
        }

        _unicodeBuffer_Count = 0;
    }

    #endregion

    #region Field instructions

    // TODO: Add unknown char when appropriate, instead of nothing

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void FieldInst_AddChar(in FontEntry fontEntry, ushort param)
    {
        SymbolFont symbolFont = fontEntry.SymbolFont;
        if (symbolFont > SymbolFont.None)
        {
            uint codePoint = _symbolFontTables[(int)symbolFont][param - 0x20];
            AddCodePointToUnicodeBuffer(codePoint);
            ParseUnicode();
        }
        else
        {
            FieldInst_AddCharFromCodePage(NoCodePage, param);
        }
    }

    private void FieldInst_Handle_A(ushort param)
    {
        if (param is < 0x20 or > byte.MaxValue) return;
        FieldInst_Handle_AddCharWithEncoding(param);
    }

    private void FieldInst_Handle_Unicode(ushort param)
    {
        if (param.IsBetween(0xF020, 0xF0FF))
        {
            param -= 0xF000;
        }

        // TODO: Should this be an IsBetween(), to allow <0x20 chars like tab and whatever to be output in the else block?
        if (param <= byte.MaxValue)
        {
            if (param >= 0x20)
            {
                int fontNum = HeaderDefaultIfNotSet(GroupStack_CurrentPropertyFontNum);

                if (!_fontDictionary.TryGetValue(fontNum, out FontEntry fontEntry) || fontEntry.CodePage != 42)
                {
                    PlainText_Add((char)param);
                    return;
                }

                FieldInst_AddChar(in fontEntry, param);
            }
        }
        else
        {
            UnicodeBuffer_Add((char)param);
            ParseUnicode();
        }
    }

    private void FieldInst_Handle_J(ushort param) => FieldInst_Handle_Unicode(param);

    private void FieldInst_Handle_U(ushort param) => FieldInst_Handle_Unicode(param);

    private void FieldInst_Handle_AddCharWithEncoding(ushort param)
    {
        int fontNum = HeaderDefaultIfNotSet(GroupStack_CurrentPropertyFontNum);

        if (!_fontDictionary.TryGetValue(fontNum, out FontEntry fontEntry))
        {
            FieldInst_AddCharFromCodePage(_headerCodePage, param);
        }
        else if (fontEntry.CodePage != 42)
        {
            FieldInst_AddCharFromCodePage(fontEntry.CodePage, param);
        }
        else
        {
            FieldInst_AddChar(in fontEntry, param);
        }
    }

    private void FieldInst_Handle_F_Bare(ushort param)
    {
        if (param.IsBetween(0xF020, 0xF0FF))
        {
            param -= 0xF000;
        }

        // TODO: Should this be an IsBetween(), to allow <0x20 chars like tab and whatever to be output in the else block?
        if (param <= byte.MaxValue)
        {
            if (param >= 0x20)
            {
                FieldInst_Handle_AddCharWithEncoding(param);
            }
        }
        else
        {
            UnicodeBuffer_Add((char)param);
            ParseUnicode();
        }
    }

    private void FieldInst_Handle_F_WithSymbolFontName(ushort codePoint, uint[] symbolFontTable)
    {
        if (codePoint.IsBetween(0xF020, 0xF0FF))
        {
            codePoint -= 0xF000;
        }
        if (codePoint <= byte.MaxValue)
        {
            AddCharFromConversionList((byte)codePoint, symbolFontTable);
        }
    }

    /*
    Field instructions are completely out to lunch with a totally unique syntax and even escaped control
    words (\\f etc.), so rather than try to shoehorn that into the regular parser and pollute it all up
    with incomprehensible nonsense, I'm just cordoning the whole thing off here. It's still incomprehensible
    nonsense, but hey, at least it stays in the loony bin and doesn't hurt anyone else.

    Also we only need one field instruction - SYMBOL. The syntax should go strictly like this:

    {\*\fldinst SYMBOL [\\f ["FontName"]] [\\a] [\\j] [\\u] [\\h] [\\s n] <arbitrary amounts of other junk>}

    With [] denoting optional parameters. I guess one of them must be present (at least we would fail if we
    didn't find any).

    Anyway, the spec is clear and simple and so we can just try to parse it exactly and quit on anything
    unexpected. Otherwise, we only want either \\f, \\a, \\j, or \\u. The others we ignore. Once we've found
    what we need, looped through six params and not found what we need, or reached a separator char, we quit
    and skip the rest of the group.

    Field instruction research:

    -Unicode character numbers can only be up to 2 bytes (0-65535), in other words "Unicode" here means "UTF-16".
     Field instructions can't be combined to produce 4 bytes the way \uN keywords can.
    -None support negative-and-add-65536.
    -None use "last used font even in a scope above"

    SYMBOL (num) \\f
    -------------------
    -Supports 0xF000-0xF0FF stuff

    SYMBOL (num) \\f "Symbol font name"
    -------------------
    -Ignores current font and "last used" font - it uses only the one specified in quotes
    -Supports 0xF000-0xF0FF stuff

    SYMBOL (num) \\a:
    -------------------
    -Doesn't support 0xF000-0xF0FF stuff
    -Supports dec or hex (single-byte)
    -If the current font is a symbol font, it displays in a symbol font and so still needs converting

    SYMBOL (num) \\j:
    -------------------
    -This is a character in Windows-31J (932), but it's specified with a Unicode codepoint, in either dec or hex.
    -Supports 0xF000-0xF0FF stuff (but maybe by accident of weird multi-byte behavior?)
    -Supports symbol fonts, even though it's a multibyte encoding...

    -These symbol-font-supporting things do a weird thing where if there's two bytes they ignore the second byte
     when they're in a symbol font, as far as I can tell? Like 0xF929 == 0x0929 == 0x0029.
     Except sometimes it isn't just the second byte, sometimes it's random and I don't know how it's arriving at
     the character. We should probably just say this is undefined behavior and not try to support it.

    SYMBOL (num) \\u:
    -------------------
    -Supports 0xF000-0xF0FF stuff (but maybe by accident of weird multi-byte behavior?)
     We should probably just say it doesn't support it because it doesn't make sense for Unicode.
    */
    private RtfError HandleFieldInstruction(ref byte bufferRef)
    {
        if (GroupStack_CurrentPropertyHidden)
        {
            SkipDest(ref bufferRef);
            return RtfError.OK;
        }

        ushort param;
        int i;

        #region Check for SYMBOL instruction

        if (_currentPos < _currentBufferChunkLength - sizeof(ulong))
        {
            ulong SYMBOLKeyword = Unsafe.ReadUnaligned<ulong>(ref GetRefAtPos(ref bufferRef, _currentPos));

            if ((SYMBOLKeyword & _SYMBOLKeywordAsULong_Mask) != _SYMBOLKeywordAsULong)
            {
                SkipDest(ref bufferRef);
                return RtfError.OK;
            }
            _currentPos += _SYMBOLName.Length;
        }
        else
        {
            for (i = 0; i < _SYMBOLName.Length; i++)
            {
                byte b = GetByte(IncrementCurrentPos());
                if (b != _SYMBOLName[i])
                {
                    return FieldInst_RewindAndSkipGroup(ref bufferRef);
                }
            }
        }

        #endregion

        char ch = (char)GetByte(IncrementCurrentPos());

        bool numIsHex = false;

        #region Parse numeric field parameter

        if (ch == '-')
        {
            return FieldInst_RewindAndSkipGroup(ref bufferRef);
        }

        #region Handle if the param is hex

        if (ch == '0')
        {
            ch = (char)GetByte(IncrementCurrentPos());
            if (ch is 'x' or 'X')
            {
                ch = (char)GetByte(IncrementCurrentPos());
                if (ch == '-')
                {
                    return FieldInst_RewindAndSkipGroup(ref bufferRef);
                }
                numIsHex = true;
            }
        }

        #endregion

        #region Read parameter

        bool alphaCharsFound = false;
        bool alphaFound;
        int fldinstSymbolNumberCount;
        for (fldinstSymbolNumberCount = 0;
             fldinstSymbolNumberCount < _fldinstSymbolNumberMaxLen + 1 &&
             ((alphaFound = ch.IsAsciiAlpha()) || ch.IsAsciiNumeric());
             fldinstSymbolNumberCount++, ch = (char)GetByte(IncrementCurrentPos()))
        {
            if (alphaFound) alphaCharsFound = true;

            _fldinstSymbolNumber[fldinstSymbolNumberCount] = ch;
        }

        if (fldinstSymbolNumberCount == 0 ||
            fldinstSymbolNumberCount >= _fldinstSymbolNumberMaxLen ||
            (!numIsHex && alphaCharsFound))
        {
            return FieldInst_RewindAndSkipGroup(ref bufferRef);
        }

        #endregion

        #region Parse parameter

        if (numIsHex)
        {
            ReadOnlySpan<char> span = new(_fldinstSymbolNumber, 0, fldinstSymbolNumberCount);
            if (!ushort.TryParse(span.ToString(),
                    NumberStyles.HexNumber,
                    NumberFormatInfo.InvariantInfo,
                    out param))
            {
                return FieldInst_RewindAndSkipGroup(ref bufferRef);
            }
        }
        else
        {
            int parsed = ParseIntFast(_fldinstSymbolNumber, fldinstSymbolNumberCount);
            if (parsed <= ushort.MaxValue)
            {
                param = (ushort)parsed;
            }
            else
            {
                return FieldInst_RewindAndSkipGroup(ref bufferRef);
            }
        }

        #endregion

        #endregion

        if (ch != ' ') return FieldInst_RewindAndSkipGroup(ref bufferRef);

        const int maxParams = 6;

        #region Parse params

        for (i = 0; i < maxParams; i++)
        {
            ch = (char)GetByte(IncrementCurrentPos());
            if (ch != '\\') continue;
            ch = (char)GetByte(IncrementCurrentPos());
            if (ch != '\\') continue;

            ch = (char)GetByte(IncrementCurrentPos());

            // From the spec:
            // "Interprets text in field-argument as the value of an ANSI character."
            if (ch == 'a')
            {
                FieldInst_Handle_A(param);
                return FieldInst_RewindAndSkipGroup(ref bufferRef);
            }
            /*
            From the spec:
            "Interprets text in field-argument as the value of a SHIFT-JIS character."

            Note that "SHIFT-JIS" in RTF land means specifically Windows-31J or whatever you want to call it.
            Also, the params here are just UTF-16 code points, the SHIFT-JIS encoding isn't really anything to
            do with it...
            */
            else if (ch == 'j')
            {
                FieldInst_Handle_J(param);
                return FieldInst_RewindAndSkipGroup(ref bufferRef);
            }
            else if (ch == 'u')
            {
                FieldInst_Handle_U(param);
                return FieldInst_RewindAndSkipGroup(ref bufferRef);
            }
            /*
            From the spec:
            "Interprets text in the switch's field-argument as the name of the font from which the character
            whose value is specified by text in the field's field-argument. By default, the font used is that
            for the current text run."

            In other words:
            If it's \\f, we use the current group's font's codepage, and if it's \\f "FontName" then we use
            code page 1252 and convert from FontName to Unicode using manual conversion tables.

            Note that RichEdit doesn't go this far. It reads the fldinst parts and all, and displays the
            characters in rich text if you have the appropriate fonts, but it doesn't convert from the fonts
            to equivalent Unicode chars when it converts to plain text. Which is reasonable really, but we
            want to do it because it's some cool ninja shit and also it helps us keep the odd copyright symbol
            intact and stuff.
            */
            else if (ch == 'f')
            {
                ch = (char)GetByte(IncrementCurrentPos());
                if (_isSeparatorChar[(byte)ch])
                {
                    FieldInst_Handle_F_Bare(param);
                    return FieldInst_RewindAndSkipGroup(ref bufferRef);
                }
                else if (ch == ' ')
                {
                    ch = (char)GetByte(IncrementCurrentPos());
                    if (ch != '\"')
                    {
                        FieldInst_Handle_F_Bare(param);
                        return FieldInst_RewindAndSkipGroup(ref bufferRef);
                    }

                    int fontNameCharCount = 0;

                    while ((ch = (char)GetByte(IncrementCurrentPos())) != '\"')
                    {
                        if (fontNameCharCount >= _maxSymbolFontNameLength || _isSeparatorChar[(byte)ch])
                        {
                            return FieldInst_RewindAndSkipGroup(ref bufferRef);
                        }
                        _fldinstSymbolFontName[fontNameCharCount] = ch;
                        fontNameCharCount++;
                    }

                    for (int symbolFontI = _symbolArraysStartingIndex; symbolFontI < _symbolArraysLength; symbolFontI++)
                    {
                        byte[] symbolChars = _symbolFontCharsArrays[symbolFontI];
                        uint[] symbolFontTable = _symbolFontTables[symbolFontI];

                        if (SeqEqual(_fldinstSymbolFontName, fontNameCharCount, symbolChars))
                        {
                            FieldInst_Handle_F_WithSymbolFontName(param, symbolFontTable);
                            return FieldInst_RewindAndSkipGroup(ref bufferRef);
                        }
                    }
                }
            }
            /*
            From the spec:
            "Inserts the symbol without affecting the line spacing of the paragraph. If large symbols are
            inserted with this switch, text above the symbol may be overwritten."

            This doesn't concern us, so ignore it.
            */
            else if (ch == 'h')
            {
                ch = (char)GetByte(IncrementCurrentPos());
                if (_isSeparatorChar[(byte)ch]) break;
            }
            /*
            From the spec:
            "Interprets text in the switch's field-argument as the integral font size in points."

            This one takes an argument (hence the extra logic to ignore it), but, yeah, we ignore it.
            */
            else if (ch == 's')
            {
                ch = (char)GetByte(IncrementCurrentPos());
                if (ch != ' ') return FieldInst_RewindAndSkipGroup(ref bufferRef);

                int numDigitCount = 0;
                while ((ch = (char)GetByte(IncrementCurrentPos())).IsAsciiNumeric())
                {
                    if (numDigitCount > _fldinstSymbolNumberMaxLen)
                    {
                        return FieldInst_RewindAndSkipGroup(ref bufferRef);
                    }
                    numDigitCount++;
                }

                if (_isSeparatorChar[(byte)ch]) break;
            }
        }

        #endregion

        return FieldInst_RewindAndSkipGroup(ref bufferRef);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private RtfError FieldInst_RewindAndSkipGroup(ref byte bufferRef)
    {
        _currentPos--;
        SkipDest(ref bufferRef);
        return RtfError.OK;
    }

    #endregion

    #region Encoding helpers

    // All callers reject nulls or won't send nulls.
    private void DecodeAndCopyBytesIntoPlainText(ushort codePage, byte[] bytes, int byteCount)
    {
        if (codePage == 0) codePage = _defaultCodePage;

        if (_sbcsToUtf16Dict.TryGetValue(codePage, out char[]? mappingTable))
        {
            ref byte bytesRef = ref GetArrayDataReference(bytes);
            ref char mappingsRef = ref GetArrayDataReference(mappingTable);
            ref char plainTextRef = ref PlainText_EnsureCapacityAndGetRef(_plainText_Count + byteCount);

            for (int i = 0, charsI = _plainText_Count; i < byteCount; i++, charsI++)
            {
                byte b = Unsafe.AddByteOffset(ref bytesRef, (nint)i);
                char c = Unsafe.Add(ref mappingsRef, (nint)b);
                Unsafe.Add(ref plainTextRef, (nint)charsI) = c;
            }
            _plainText_Count += byteCount;
        }
        else
        {
            try
            {
                PlainText_EnsureCapacity(_plainText_Count + (byteCount * 4));
                _plainText_Count += GetEncodingFromCachedList(codePage).GetChars(bytes, 0, byteCount, _plainText, _plainText_Count);
            }
            catch
            {
                PlainText_Add(_unicodeUnknown_Char);
            }
        }
    }

    private void DecodeAndCopyByteIntoPlainText(ushort codePage, byte b)
    {
        if (codePage == 0)
        {
            codePage = _defaultCodePage;
        }

        if (_sbcsToUtf16Dict.TryGetValue(codePage, out char[]? mappingTable))
        {
            ref char mappingsRef = ref GetArrayDataReference(mappingTable);
            ref char plainTextRef = ref PlainText_EnsureCapacityAndGetRef(_plainText_Count + 1);

            char c = Unsafe.Add(ref mappingsRef, (nint)b);
            Unsafe.Add(ref plainTextRef, (nint)_plainText_Count) = c;
            _plainText_Count += 1;
        }
        else
        {
            try
            {
                _byteBuffer1[0] = b;
                PlainText_EnsureCapacity(_plainText_Count + 4);
                _plainText_Count += GetEncodingFromCachedList(codePage).GetChars(_byteBuffer1, 0, 1, _plainText, _plainText_Count);
            }
            catch
            {
                PlainText_Add(_unicodeUnknown_Char);
            }
        }
    }

    private Encoding GetEncodingFromCachedList(ushort codePage)
    {
        if (_encodings.TryGetValue(codePage, out Encoding? result))
        {
            return result;
        }
        else
        {
            Encoding enc = Encoding.GetEncoding(codePage);
            _encodings[codePage] = enc;
            return enc;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ushort GetCurrentCodePage()
    {
        ushort groupLang = GroupStack_CurrentPropertyLang;

        ushort codePage;
        ushort translatedCodePage;
        if (groupLang <= MaxLangNumIndex && (translatedCodePage = LangToCodePage[groupLang]) < NoCodePage)
        {
            codePage = translatedCodePage;
        }
        else
        {
            int groupFontNum = HeaderDefaultIfNotSet(GroupStack_CurrentPropertyFontNum);
            _fontDictionary.TryGetValue(groupFontNum, out FontEntry fontEntry);
            codePage = fontEntry.IsSet ? fontEntry.CodePage : _headerCodePage;
        }

        return codePage > 0 ? codePage : _defaultCodePage;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private uint NormalizeUnicodeCodePoint(int codePoint)
    {
        // Per spec, values >32767 are expressed as negative numbers, and we must add 65536 to get the correct
        // value.
        if (codePoint < 0)
        {
            codePoint += 65536;
            if (codePoint < 0)
            {
                return _unicodeUnknown_Char;
            }
        }

        uint returnCodePoint = (uint)codePoint;

        /*
        Verified, these 0xF020-0xF0FF chars can be represented either as negatives or as >32767 positives
        (despite the spec saying that \uN must be signed int16). So we need to fall through to this section
        even if we did the above, because by adding 65536 we might now be in the 0xF020-0xF0FF range.
        */
        if (returnCodePoint.IsBetween(0xF020, 0xF0FF))
        {
            returnCodePoint -= 0xF000;

            int fontNum = HeaderDefaultIfNotSet(_lastUsedFontWithCodePage42);

            if (_fontDictionary.TryGetValue(fontNum, out FontEntry fontEntry) && fontEntry.CodePage == 42)
            {
                SymbolFont symbolFont = fontEntry.SymbolFont;
                if (symbolFont > SymbolFont.None)
                {
                    return _symbolFontTables[(int)symbolFont][returnCodePoint - 0x20];
                }
            }
        }

        return returnCodePoint;
    }

    #endregion

    #endregion

    #region Add chars

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void AddChar_Explicit(char ch)
    {
        // No need to check for null, because only explicit chars will be passed (not unknown ones) and we know
        // none of them are null.

        // If this byte is at the start of a stream it's going to be interpreted as a BOM; only if it's past
        // the start should we actually write it.
        if (ch == '\xFEFF' && _plainText_Count == 0)
        {
            return;
        }

        if (ch == '\n')
        {
            AddLineBreak();
            return;
        }

        SymbolFont symbolFont = GroupStack_CurrentSymbolFont;
        if (symbolFont > SymbolFont.None && ch <= byte.MaxValue)
        {
            AddCharFromConversionList((byte)ch, _symbolFontTables[(int)symbolFont]);
        }
        else
        {
            PlainText_Add(ch);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void AddCharFromConversionList(byte codePoint, uint[] fontTable)
    {
        if (codePoint >= 0x20)
        {
            AddConvertedFromUtf32(fontTable[codePoint - 0x20]);
        }
        else
        {
            DecodeAndCopyByteIntoPlainText(_defaultCodePage, codePoint);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void FieldInst_AddCharFromCodePage(ushort codePage, ushort codePoint)
    {
        // Reject null chars
        if (codePoint == 0) return;

        // We're a field instruction code path, so we don't have to be in tip top performance shape necessarily.
        int byteCount = codePoint <= byte.MaxValue ? 1 : 2;

        // BitConverter.GetBytes() does this, but it allocates a temp array every time.
        // Use Unsafe.WriteUnaligned() like .NET 10+: https://github.com/dotnet/runtime/pull/91639
        Unsafe.WriteUnaligned(ref GetArrayDataReference(_byteBuffer4), codePoint);

        if (codePage < NoCodePage)
        {
            DecodeAndCopyBytesIntoPlainText(codePage, _byteBuffer4, byteCount);
        }
        else
        {
            codePage = GetCurrentCodePage();
            if (codePage != 42)
            {
                DecodeAndCopyBytesIntoPlainText(codePage, _byteBuffer4, byteCount);
            }
            else
            {
                PlainText_Add(_unicodeUnknown_Char);
            }
        }
    }

    /// <summary>
    /// Copy of .NET 7 version (fewer branches than Framework) but with a fast null return on fail instead of the infernal exception-throwing.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void AddConvertedFromUtf32(uint utf32u)
    {
        if (((utf32u - 0x110000u) ^ 0xD800u) < 0xFFEF0800u)
        {
            PlainText_Add(_unicodeUnknown_Char);
            return;
        }

        if (utf32u <= char.MaxValue)
        {
            PlainText_Add((char)utf32u);
            return;
        }

        PlainText_Add((char)((utf32u + ((0xD800u - 0x40u) << 10)) >> 10));
        PlainText_Add((char)((utf32u & 0x3FFu) + 0xDC00u));
    }

    private void AddLineBreak()
    {
        switch (_lineBreakStyle)
        {
            case LineBreakStyle.EnvironmentDefault:
                // Try to be efficient - should be branch predictor friendly with no loop overhead in the expected
                // cases.
                switch (LineBreakStringLength)
                {
                    case 2:
                        PlainText_EnsureCapacity(_plainText_Count + 2);
                        char[] plainText = _plainText;
                        plainText[_plainText_Count] = LineBreakString[0];
                        plainText[_plainText_Count + 1] = LineBreakString[1];
                        _plainText_Count += 2;
                        break;
                    case 1:
                        PlainText_Add(LineBreakString[0]);
                        break;
                    default:
                    {
                        // Shouldn't ever hit this, but maybe Microsoft vibe-codes a patch into Windows that makes
                        // the line break be three characters. Just kidding. Probably.
                        for (int i = 0; i < LineBreakString.Length; i++)
                        {
                            PlainText_Add(LineBreakString[i]);
                        }
                        break;
                    }
                }
                break;
            case LineBreakStyle.CRLF:
            {
                PlainText_EnsureCapacity(_plainText_Count + 2);
                char[] plainText = _plainText;
                plainText[_plainText_Count] = '\r';
                plainText[_plainText_Count + 1] = '\n';
                _plainText_Count += 2;
                break;
            }
            default:
                PlainText_Add('\n');
                break;
        }
    }

    #endregion

    #region Helpers

    /*
    This MUST have AggressiveInlining on it, or else .NET Framework x64 gets significantly slower. Also if we
    pull the code inline physically, .NET Framework x64 is ALSO slower. No, we have to make this method separate
    BUT THEN ALSO tell the JIT to inline it, and only then do we keep performance. Yeah okay sure.
    */
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void SetOptions(RtfToTextConverterOptions src)
    {
        _lineBreakStyle = src._lineBreakStyle;
        _convertHiddenText = src._convertHiddenText;
        _defaultCodePage = src._defaultCodePage;

        if (src._swapUppercaseAndLowercasePhiSymbols)
        {
            _symbolFontTables[(int)SymbolFont.Symbol][0x66 - 0x20] = 0x03D5;
            _symbolFontTables[(int)SymbolFont.Symbol][0x6A - 0x20] = 0x03C6;
        }
        else
        {
            _symbolFontTables[(int)SymbolFont.Symbol][0x66 - 0x20] = 0x03C6;
            _symbolFontTables[(int)SymbolFont.Symbol][0x6A - 0x20] = 0x03D5;
        }

        _symbolFontTables[(int)SymbolFont.Symbol][0xA0 - 0x20] = src._symbolFontA0Char switch
        {
            SymbolFontA0Char.EuroSign => '\x20AC',
            SymbolFontA0Char.NumericSpace => '\x2007',
            _ => _unicodeUnknown_Char,
        };
    }

    /// <summary>
    /// Only call this if <paramref name="chars"/>'s length is > 0 and consists solely of the characters '0' through '9'.
    /// It does no checks at all and will throw if either of these things is false.
    /// </summary>
    /// <param name="chars"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int ParseIntFast(char[] chars, int length)
    {
        int result = 0;
        for (int i = 0; i < length; i++)
        {
            result = (result * 10) + (chars[i] - '0');
        }
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool SeqEqual(char[] seq1, int seq1Length, byte[] seq2)
    {
        if (seq1Length != seq2.Length) return false;

        for (int ci = 0; ci < seq1Length; ci++)
        {
            if (seq1[ci] != seq2[ci]) return false;
        }
        return true;
    }

    // Calculate it at the end from values we already have, rather than changing an additional value in hot loops
    private int GetCurrentOverallPos()
    {
        return _bufferedStream == null
            ? _currentPos
            : ((_chunksRead - 1) * (_bufferLength - _leadingBufferByteCount)) + _currentPos - _leadingBufferByteCount;
    }

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

    #endregion

    #region Fast skipping

    private int IndexOfNextClosingBrace_ChunkAware()
    {
        while (!_reachedEndOfStream)
        {
            int foundIndex = UtilHelper.Array_IndexOfByte_Fast(_buffer, (byte)'}', _currentPos, _currentBufferChunkLength - _currentPos);
            if (foundIndex > -1)
            {
                return foundIndex;
            }
            else
            {
                if (_bufferedStream != null)
                {
                    LoadNextChunkIntoBuffer();
                }
                else
                {
                    return _currentBufferChunkLength;
                }
            }
        }

        return _currentBufferChunkLength;
    }

    private RtfError HandleSkippableHexData(ref byte bufferRef)
    {
        // Prevent stack overflow from maliciously-crafted rtf files - we should never recurse back into here in
        // a spec-conforming file.
        if (_inHandleSkippableHexData) return RtfError.AbortedForSafety;
        _inHandleSkippableHexData = true;

        int startGroupLevel = _groupStackTopIndex;

        while (!_reachedEndOfStream)
        {
            while (_currentPos < _currentBufferChunkLength)
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
                            InsertSpaceIfNecessary();
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

            if (_bufferedStream != null) { HandleOutOfBounds(); } else { break; }
        }

        InsertSpaceIfNecessary();
        _inHandleSkippableHexData = false;
        return RtfError.OK;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void InsertSpaceIfNecessary()
    {
        if (_plainText_Count > 0 &&
            !char.IsWhiteSpace(_plainText[_plainText_Count - 1]) &&
            !GroupStack_CurrentPropertyHidden &&
            !GroupStack_CurrentSkipDest &&
            !_inFontTable)
        {
            PlainText_Add(' ');
        }
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
        while (!_reachedEndOfStream)
        {
            index = SIMD_SkipDest(ref bufferRef, index, _currentBufferChunkLength - index);

            /*
            Curly braces can be escaped like \{ and \}. But there can be an arbitrary amount of backslashes
            before a curly brace, because it could be a series of escaped backslashes and then an escaped
            curly brace: \\\\\\\}. Which means if we encountered one, we'd have to read an arbitrary amount
            back in the stream, which we can't do. So if we don't find the end of our subgroup stack in the
            current buffer chunk, just give up and take the slow path that properly parses escapes.
            */
            if (index <= 0 ||
                index >= _currentBufferChunkLength ||
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
                    if (index > _currentBufferChunkLength - BinLength ||
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
        Debug.Assert(_currentPos < _currentBufferChunkLength);
        return Unsafe.AddByteOffset(ref bufferRef, (nint)IncrementCurrentPos());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private byte GetByteAtPos(ref byte bufferRef, int pos)
    {
        Debug.Assert(pos < _currentBufferChunkLength);
        return Unsafe.AddByteOffset(ref bufferRef, (nint)pos);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ref byte GetRefAtPos(ref byte bufferRef, int pos)
    {
        Debug.Assert(pos < _currentBufferChunkLength);
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
        if (_bufferedStream == null)
        {
            _currentPos += amount;
        }
        else
        {
            IncrementCurrentPos_Stream_PossiblySkippingMultipleChunks(amount);
        }
    }

    private int IncrementCurrentPos_Stream(int originalPos)
    {
        if (_currentPos + 1 > _currentBufferChunkLength - 1)
        {
            int difference = (_currentPos + 1) - _currentBufferChunkLength;

            LoadNextChunkIntoBuffer();
            _currentPos += difference;
            originalPos = _currentPos - 1;
        }
        else
        {
            _currentPos += 1;
        }

        return originalPos;
    }

    private void IncrementCurrentPos_Stream_PossiblySkippingMultipleChunks(int amount)
    {
        if (_currentPos + amount > _currentBufferChunkLength - 1)
        {
            bool skippingMultipleChunks;
            do
            {
                int savedAmount = amount;

                skippingMultipleChunks = amount > _currentBufferChunkLength;

                if (skippingMultipleChunks)
                {
                    amount = _currentBufferChunkLength - _currentPos;
                    savedAmount -= amount;
                }

                int difference = (_currentPos + amount) - _currentBufferChunkLength;

                LoadNextChunkIntoBuffer();
                _currentPos += difference;

                amount = savedAmount;

            } while (skippingMultipleChunks);
        }
        else
        {
            _currentPos += amount;
        }
    }

    private void LoadNextChunkIntoBuffer()
    {
        Debug.Assert(_bufferedStream != null);

        // This path should only be hit when in streaming mode, and when therefore the buffer size is supposed
        // to have an enforced minimum.
        Debug.Assert(_buffer.Length >= _maxSeekBackBytes);

        byte[] buffer = _buffer;

        ulong endChunk = Unsafe.ReadUnaligned<ulong>(ref buffer[_currentBufferChunkLength - _maxSeekBackBytes]);
        Unsafe.WriteUnaligned(ref buffer[0], endChunk);

        int bytesRead = _bufferedStream!.ReadAll(buffer, _maxSeekBackBytes, _bufferLength - _maxSeekBackBytes);

        if (bytesRead == 0)
        {
            // Drop-in that loops can check to achieve the same effect as checking the length the way we used to
            _reachedEndOfStream = true;
        }
        else
        {
            _chunksRead++;
            _currentPos = _maxSeekBackBytes;
            _currentBufferChunkLength = bytesRead + _maxSeekBackBytes;
        }
    }

    #endregion

    // Pulling previously separate classes into the main class increases performance: the fewer separate classes
    // we have to reference in hot loops, the better.

    #region Buffer

    private byte[] _buffer = Array.Empty<byte>();
    private int _bufferLength;
    private int _currentBufferChunkLength;

    private void SetBufferLength(int length)
    {
        _bufferLength = length;
        _currentBufferChunkLength = length;
    }

    /// <summary>
    /// Manually bounds-checked past <see cref="T:_currentBufferChunkLength"/>.
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
        if (index > _currentBufferChunkLength - 1)
        {
            /*
            Putting the ThrowHelper call here makes us full speed (on the byte array path). Putting this here
            instead loses us like 6-10% again. Even though HandleOutOfBounds() has the no inlining attribute!
            Argh!
            But, this system does make us a little faster than before (especially on the streaming path), so hey.
            */
            index = HandleOutOfBounds();
        }
        return _buffer[index];
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private int HandleOutOfBounds()
    {
        if (_bufferedStream != null)
        {
            _currentPos--;
            int originalPos = _currentPos;
            int ret = IncrementCurrentPos_Stream(_currentPos);
            if (_currentPos > _currentBufferChunkLength)
            {
                if (_groupStackTopIndex > 0)
                {
                    _currentPos = originalPos;
                    ThrowHelper.UnmatchedBraceException();
                }
                else
                {
                    _currentPos = originalPos;
                    ThrowHelper.IndexOutOfRange();
                }
                return 0;
            }
            else
            {
                return ret;
            }
        }
        else
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
    }

    #endregion

    #region GroupStack

    [StructLayout(LayoutKind.Auto)]
    private struct GroupStackFrame
    {
        internal bool SkipDestination;
        internal SymbolFont SymbolFont;

        internal bool PropHidden;
        internal int PropUnicodeSkipCharCount;
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

    private SymbolFont GroupStack_CurrentSymbolFont
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _groupStackFrames[_groupStackTopIndex].SymbolFont;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => _groupStackFrames[_groupStackTopIndex].SymbolFont = value;
    }

    private bool GroupStack_CurrentPropertyHidden
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _groupStackFrames[_groupStackTopIndex].PropHidden;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => _groupStackFrames[_groupStackTopIndex].PropHidden = value;
    }

    private int GroupStack_CurrentPropertyUnicodeCharSkipCount
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _groupStackFrames[_groupStackTopIndex].PropUnicodeSkipCharCount;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => _groupStackFrames[_groupStackTopIndex].PropUnicodeSkipCharCount = value;
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
            SymbolFont = SymbolFont.None,
            PropHidden = false,
            PropUnicodeSkipCharCount = 1,
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
    Ignore list item bullets and numeric prefixes etc. We don't need them.

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
        new Symbol("pntext", 0, false, KeywordType.Destination, (ushort)DestinationType.Skip),
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
        new Symbol("endash", 0, false, KeywordType.Character, '-'),
        null,
// Entry 66
        new Symbol("title", 0, false, KeywordType.Destination, (ushort)DestinationType.Skip),
        null,
// Entry 23
        new Symbol("lquote", 0, false, KeywordType.Character, '\''),
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
        new Symbol("listtext", 0, false, KeywordType.Destination, (ushort)DestinationType.Skip),
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
        new Symbol("ldblquote", 0, false, KeywordType.Character, '"'),
// Entry 21
        new Symbol("emdash", 0, false, KeywordType.Character, '-'),
// Entry 2
        new Symbol("mac", 10000, true, KeywordType.Special, (ushort)SpecialType.HeaderCodePage),
        null,
// Entry 24
        new Symbol("rquote", 0, false, KeywordType.Character, '\''),
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
        new Symbol("enspace", 0, false, KeywordType.Character, ' '),
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
        new Symbol("rdblquote", 0, false, KeywordType.Character, '"'),
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
        new Symbol("qmspace", 0, false, KeywordType.Character, ' '),
// Entry 18
        new Symbol("emspace", 0, false, KeywordType.Character, ' '),
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

    private static readonly char[] _controlSymbols = RunFunc(static () =>
    {
        char[] ret = new char[256];
        ret['\''] = '\'';
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
        ret['\\'] = '\\';
        ret['{'] = '{';
        ret['}'] = '}';

        // Nominally Non-Breaking Space (0xA0)
        ret['~'] = ' ';

        // Nominally Non-Breaking Hyphen (0x2011)
        ret['_'] = '-';

        // There's also \- which is Optional Hyphen (the scanner is only producing single-line values, so no
        // need for this), and \: which "specifies a subentry in an index entry" (it's not clear even from
        // the spec what exactly an "index entry" is).

        /*
        Spec:
        "A carriage return (character value 13) or line feed (character value 10) is treated as a \par
        control if the character is preceded by a backslash. You must include the backslash; otherwise,
        RTF ignores the control word."
        */
        ret['\r'] = '\n';
        ret['\n'] = '\n';
        return ret;
    });

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static char LookUpControlSymbol(byte ch)
    {
        return Unsafe.Add(ref GetArrayDataReference(_controlSymbols), (nint)ch);
    }

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
