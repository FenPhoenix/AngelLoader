﻿/*
Perf log:

             FMInfoGen | RTF_ToPlainTextTest
2023-11-07   ?           1631MB/s (x86) / 2009MB/s (x64)
2023-11-06   ?           1539MB/s (x86) / 1872MB/s (x64)
2023-11-06   ?           1471MB/s (x86) / 1716MB/s (x64)
2023-11-06   ?           1248MB/s (x86) / 1615MB/s (x64)
2023-11-05   ?           1185MB/s (x86) / 1525MB/s (x64)
2023-11-04   ?           1109MB/s (x86) / 1360MB/s (x64)
2023-11-04   ?           906MB/s (x86) / 1101MB/s (x64)
2023-11-03   ?           828MB/s (x86) / 1012MB/s (x64)
2023-11-02   ?           643MB/s (x86) / 713MB/s (x64)
2023-11-02   ?           567MB/s (x86) / 663MB/s (x64)
2023-10-05:  ?           521MB/s (x64)
2023-10-03:  ?           512MB/s (x64)
2023-10-03:  ?           492MB/s (x86)
2023-09-30:  ?           363MB/s (x86)
2023-09-29:  ?           335MB/s (x86)
2020-08-24:  179MB/s     254MB/s
2020-08-20:  157MB/s     211MB/s
2020-08-19:  88.2 MB/s   97MB/s

---

Note to self:
RTFs in 1098 set, base dirs only:
count: 280
total size: 88,714,908
84.6 MB

---

This is a fast RTF-to-plaintext converter designed to provide scannable text for FMScanner.

Goals:
1. Be platform-agnostic (no RichTextBox dependency).
2. Achieve faster performance than RichTextBox, and ideally be as fast as possible beyond that.
3. Accurately convert all "general text" characters as the user would be intended to see them, even symbol font
   glyphs.

Non-Goals:
1. Perfectly match RichTextBox in output. The output is not meant to be displayed to the user, so we don't care
   about extra whitespace lines, indenting, bulleted/numbered list characters, etc. We also convert certain
   exotic characters to their ASCII equivalents (dashes, spaces etc.) to give easier data to the scanner.
2. Extremely strict validation or enforcement of spec. We assume the stream has been checked for an rtf header
   already. We also allow for what RichTextBox allows - or has allowed - for, even if it's not quite to spec.
3. Forward-only stream support. We used to have this, but it entailed a fairly sizable performance loss. The
   need for an un-get buffer caused constant branching inside a tight loop, and also precluded optimizations
   afforded by being able to move the stream index freely.

---

Notes and miscellaneous:
-Hex that combines into an actual valid character: \'81\'63
 (it's supposed to be an ellipsis - __MSG_final__FMInfo-De - Copy.rtf has an instance of it)
-Tiger face (>2 byte Unicode test): \u-9169?\u-10179?

@RTF(Every-char checks):
-RichTextBox respects \v0 (hidden text) when it converts, but LibreOffice doesn't.
-RichTextBox and LibreOffice both remove nulls.

@RTF(RTF to plaintext converter):
-Consider being extremely forgiving about errors - we want as much plaintext as we can get out of a file, and
 even imperfect text may be useful. FMScanner extracts a relatively very small portion of text from the file,
 so statistically it's likely it may not even hit broken text even if it exists.
*/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using JetBrains.Annotations;
using static AL_Common.Common;
using static AL_Common.RTFParserCommon;

namespace AL_Common;

public sealed partial class RtfToTextConverter
{
    #region Constants

    private const int _windows1252 = 1252;
    private const int _shiftJisWin = 932;
    private const char _unicodeUnknown_Char = '\u25A1';

    // 20 bytes * 4 for up to 4 bytes per char. Chars are 2 bytes but like whatever, why do math when you can
    // over-provision.
    private readonly ListFast<char> _charGeneralBuffer = new(20 * 4);

    #endregion

    #region Tables

    #region Font to Unicode conversion tables

    /*
    Many RTF files put emoji-like glyphs into text not with a Unicode character, but by just putting in a
    regular-ass single-byte char and then setting the font to Wingdings or whatever. So the letter "J"
    would show as "☺" in the Wingdings font. If we want to support this lunacy, we need conversion tables
    from known fonts to their closest Unicode mappings. So here we have them.

    These arrays MUST be of length 224, with entries starting at the codepoint for 0x20 and ending at the
    codepoint for 0xFF. That way, they can be arrays instead of dictionaries, making us smaller and faster.
    */

    // ReSharper disable RedundantExplicitArraySize
    private readonly uint[] _symbolFontToUnicode = new uint[224]
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

        // Lowercase phi, but capital phi in Windows Symbol
        0x03C6,

        0x03B3,
        0x03B7,
        0x03B9,

        // Capital phi, but lowercase phi in Windows Symbol
        0x03D5,

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

        // 7F - 9F are undefined
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

        // Undefined
        _unicodeUnknown_Char
    };

    private readonly uint[] _wingdingsFontToUnicode = new uint[224]
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
        0x25AF,
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
        0x1FA9F
    };

    private readonly uint[] _webdingsFontToUnicode = new uint[224]
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

        // 7F is undefined
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
        0x1F54A
    };
    // ReSharper restore RedundantExplicitArraySize

    private readonly uint[][] _symbolFontTables;

    public RtfToTextConverter()
    {
        _symbolFontTables = new uint[5][];
        _symbolFontTables[(int)SymbolFont.Symbol] = _symbolFontToUnicode;
        _symbolFontTables[(int)SymbolFont.Wingdings] = _wingdingsFontToUnicode;
        _symbolFontTables[(int)SymbolFont.Webdings] = _webdingsFontToUnicode;
    }

    #endregion

    // From .NET 8. Maps ascii characters to hex (ie. "A" -> 0xA). 0xFF means not a hex character.
    private static readonly byte[] _charToHex =
    {
        0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
        0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
        0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
        0x0, 0x1, 0x2, 0x3, 0x4, 0x5, 0x6, 0x7, 0x8, 0x9, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
        0xFF, 0xA, 0xB, 0xC, 0xD, 0xE, 0xF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
        0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
        0xFF, 0xa, 0xb, 0xc, 0xd, 0xe, 0xf, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
        0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
        0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
        0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
        0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
        0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
        0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
        0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
        0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
        0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF
    };

    #endregion

    #region Preallocated char arrays

    // This "SYMBOL" and the below "Symbol" are unrelated. "SYMBOL" is a fldinst keyword, while "Symbol" is
    // the name of a font.
    private readonly char[] _SYMBOLChars = { 'S', 'Y', 'M', 'B', 'O', 'L' };

    private readonly char[] _wingdingsChars = { 'W', 'i', 'n', 'g', 'd', 'i', 'n', 'g', 's' };

    private readonly char[] _webdingsChars = { 'W', 'e', 'b', 'd', 'i', 'n', 'g', 's' };

    private readonly char[] _symbolChars = { 'S', 'y', 'm', 'b', 'o', 'l' };

    #endregion

    #region Resettables

    /*
    Per spec, if we see a \uN keyword whose N falls within the range of 0xF020 to 0xF0FF, we're supposed to
    subtract 0xF000 and then find the last used font whose charset is 2 (codepage 42) and use its symbol font
    to convert the char. However, when the spec says "last used" it REALLY means last used. Period. Regardless
    of group. Even if the font was used in a group above us that we should have no knowledge of, it still
    counts as the last used one. Also, we need the last used font WHOSE CODEPAGE IS 42, not the last used font
    period. So we have to track only the charset 2/codepage 42 ones. Globally. Truly bizarre.
    */
    private int _lastUsedFontWithCodePage42 = NoFontNumber;

    // Highest measured was 56192
    private readonly ListFast<char> _plainText = new(ByteSize.KB * 60);

    private const int _fldinstSymbolNumberMaxLen = 10;
    private readonly ListFast<char> _fldinstSymbolNumber = new(_fldinstSymbolNumberMaxLen);

    private const int _fldinstSymbolFontNameMaxLen = 9;
    private readonly ListFast<char> _fldinstSymbolFontName = new(_fldinstSymbolFontNameMaxLen);

    // Highest measured was 17
    private readonly ListFast<byte> _hexBuffer = new(20);

    // Highest measured was 13
    private readonly ListFast<char> _unicodeBuffer = new(20);

    private bool _inHandleFontTable;

    #endregion

    #region Reusable buffers

    private readonly byte[] _byteBuffer1 = new byte[1];
    private readonly byte[] _byteBuffer4 = new byte[4];

    #endregion

    #region Cached encodings

    // DON'T reset this. We want to build up a dictionary of encodings and amortize it over the entire list
    // of RTF files.
    private readonly Dictionary<int, Encoding> _encodings = new(31);

    // Common ones explicitly stored to avoid even a dictionary lookup. Don't reset these either.
    private readonly Encoding _windows1252Encoding = Encoding.GetEncoding(_windows1252);

    private readonly Encoding _windows1250Encoding = Encoding.GetEncoding(1250);

    private readonly Encoding _windows1251Encoding = Encoding.GetEncoding(1251);

    private readonly Encoding _shiftJisWinEncoding = Encoding.GetEncoding(_shiftJisWin);

    #endregion

    #region Public API

    [PublicAPI]
    public (bool Success, string Text)
    Convert(in ArrayWithLength<byte> rtfBytes)
    {
        Reset(rtfBytes);

#if ReleaseRTFTest || DebugRTFTest
        RtfError error = ParseRtf();
        return error == RtfError.OK ? (true, CreateStringFromChars(_plainText)) : throw new System.Exception("RTF converter error: " + error);
        //return error == Error.OK ? (true, "") : throw new Exception("RTF converter error: " + error);
#else
        try
        {
            RtfError error = ParseRtf();
            return error == RtfError.OK ? (true, CreateStringFromChars(_plainText)) : (false, "");
        }
        catch
        {
            return (false, "");
        }
        finally
        {
            _rtfBytes = ArrayWithLength<byte>.Empty();
        }
#endif
    }

    #endregion

    private void Reset(in ArrayWithLength<byte> rtfBytes)
    {
        ResetBase(rtfBytes);

        #region Fixed-size fields

        // Specific capacity and won't grow; no need to deallocate
        _fldinstSymbolNumber.ClearFast();
        _fldinstSymbolFontName.ClearFast();

        _lastUsedFontWithCodePage42 = NoFontNumber;

        #endregion

        _hexBuffer.ClearFast();
        _unicodeBuffer.ClearFast();
        _plainText.ClearFast();

        // Extremely unlikely we'll hit any of these, but just for safety
        if (_hexBuffer.Capacity > ByteSize.MB) _hexBuffer.Capacity = 0;
        if (_unicodeBuffer.Capacity > ByteSize.MB) _unicodeBuffer.Capacity = 0;
        // For the font entries, we can't check a Dictionary's capacity nor set it, so... oh well.
        // @NET5: I think .NET modern has dictionary clear methods?
        if (_plainText.Capacity > ByteSize.MB) _plainText.Capacity = 0;

        _inHandleFontTable = false;
    }

    private RtfError ParseRtf()
    {
        while (CurrentPos < _rtfBytes.Length)
        {
            char ch = (char)_rtfBytes.Array[CurrentPos++];

            // Ordered by most frequently appearing first
            switch (ch)
            {
                case '\\':
                    RtfError ec = ParseKeyword();
                    if (ec != RtfError.OK) return ec;
                    break;
                // Push/pop groups inline to avoid having one branch to check the actual error condition and then
                // a second branch to check the return error code from the push/pop method.
                case '{':
                    if (_ctx.GroupStack.Count >= GroupStack.MaxGroups) return RtfError.StackOverflow;
                    _ctx.GroupStack.DeepCopyToNext();
                    _groupCount++;
                    break;
                case '}':
                    if (_ctx.GroupStack.Count == 0) return RtfError.StackUnderflow;
                    --_ctx.GroupStack.Count;
                    _groupCount--;
                    break;
                case '\r':
                case '\n':
                    break;
                default:
                    if (_ctx.GroupStack.CurrentRtfDestinationState == RtfDestinationState.Normal)
                    {
                        ParseChar_Byte(ch);
                    }
                    break;
            }
        }

        return _groupCount > 0 ? RtfError.UnmatchedBrace : RtfError.OK;
    }

    #region Act on keywords

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private RtfError DispatchKeyword(Symbol symbol, int param, bool hasParam)
    {
        if (_ctx.GroupStack.CurrentRtfDestinationState == RtfDestinationState.Normal)
        {
            switch (symbol.KeywordType)
            {
                case KeywordType.Property:
                    if (symbol.UseDefaultParam || !hasParam) param = symbol.DefaultParam;
                    ChangeProperty((Property)symbol.Index, param);
                    return RtfError.OK;
                case KeywordType.Character:
                    ParseChar((char)symbol.Index);
                    return RtfError.OK;
                case KeywordType.Destination:
                    return symbol.Index == (int)DestinationType.SkippableHex
                        ? HandleSkippableHexData()
                        : ChangeDestination((DestinationType)symbol.Index);
                case KeywordType.Special:
                    var specialType = (SpecialType)symbol.Index;
                    return DispatchSpecialKeyword(specialType, symbol, param);
                default:
                    return RtfError.InvalidSymbolTableEntry;
            }
        }
        else
        {
            switch (symbol.KeywordType)
            {
                case KeywordType.Destination:
                    return symbol.Index == (int)DestinationType.SkippableHex
                        ? HandleSkippableHexData()
                        : RtfError.OK;
                case KeywordType.Special:
                    var specialType = (SpecialType)symbol.Index;
                    return specialType == SpecialType.SkipNumberOfBytes
                        ? DispatchSpecialKeyword(specialType, symbol, param)
                        : RtfError.OK;
                default:
                    return RtfError.OK;
            }
        }
    }

    private RtfError DispatchSpecialKeyword(SpecialType specialType, Symbol symbol, int param)
    {
        switch (specialType)
        {
            case SpecialType.SkipNumberOfBytes:
                if (symbol.UseDefaultParam) param = symbol.DefaultParam;
                if (param < 0) return RtfError.AbortedForSafety;
                CurrentPos += param;
                break;
            case SpecialType.HexEncodedChar:
                HandleHexRun();
                break;
            case SpecialType.SkipDest:
                _skipDestinationIfUnknown = true;
                break;
            case SpecialType.UnicodeChar:
                SkipUnicodeFallbackChars();
                UnicodeBufferAdd(param);
                HandleUnicodeRun();
                break;
            case SpecialType.ColorTable:
                int closingBraceIndex = Array.IndexOf(_rtfBytes.Array, (byte)'}', CurrentPos, _rtfBytes.Length - CurrentPos);
                CurrentPos = closingBraceIndex == -1 ? _rtfBytes.Length : closingBraceIndex;
                break;
            case SpecialType.FontTable:
                _ctx.GroupStack.CurrentInFontTable = true;
                RtfError error = HandleFontTable();
                if (error != RtfError.OK) return error;
                break;
            default:
                HandleSpecialTypeFont(_ctx, specialType, param);
                return RtfError.OK;
        }

        return RtfError.OK;
    }

    private unsafe RtfError HandleFontTable()
    {
        // Prevent stack overflow from maliciously-crafted rtf files - we should never recurse back into here in
        // a spec-conforming file.
        if (_inHandleFontTable) return RtfError.AbortedForSafety;
        _inHandleFontTable = true;

        int fontTableGroupLevel = _ctx.GroupStack.Count;

        while (CurrentPos < _rtfBytes.Length)
        {
            char ch = (char)_rtfBytes.Array[CurrentPos++];

            switch (ch)
            {
                // Push/pop groups inline to avoid having one branch to check the actual error condition and then
                // a second branch to check the return error code from the push/pop method.
                case '{':
                    if (_ctx.GroupStack.Count >= GroupStack.MaxGroups) return RtfError.StackOverflow;
                    _ctx.GroupStack.DeepCopyToNext();
                    _groupCount++;
                    break;
                case '}':
                    if (_ctx.GroupStack.Count == 0) return RtfError.StackUnderflow;
                    --_ctx.GroupStack.Count;
                    _groupCount--;
                    if (_groupCount < fontTableGroupLevel)
                    {
                        // We can't actually set the symbol font as soon as we see \deffN, because we won't have
                        // any font entry objects yet. Now that we do, we can retroactively set all previous
                        // groups' fonts as appropriate, as if they had propagated up automatically.
                        int defaultFontNum = _ctx.Header.DefaultFontNum;
                        if (_ctx.FontEntries.TryGetValue(defaultFontNum, out FontEntry? fontEntry))
                        {
                            SymbolFont symbolFont = GetSymbolFontTypeFromFontEntry(fontEntry);
                            // Start at 1 because the "base" group is still inside an opening { so it's
                            // really group 1.
                            for (int i = 1; i < _groupCount + 1; i++)
                            {
                                int[] properties = _ctx.GroupStack.Properties[i];
                                int fontNum = properties[(int)Property.FontNum];
                                if (fontNum == NoFontNumber)
                                {
                                    properties[(int)Property.FontNum] = defaultFontNum;
                                    _ctx.GroupStack.SymbolFonts.Array[i] = (int)symbolFont;
                                }
                                else
                                {
                                    break;
                                }
                            }
                        }

                        _inHandleFontTable = false;
                        return RtfError.OK;
                    }
                    break;
                case '\\':
                    RtfError ec = ParseKeyword();
                    if (ec != RtfError.OK) return ec;
                    break;
                case '\r':
                case '\n':
                    break;
                default:
                    if (_ctx.GroupStack.CurrentRtfDestinationState == RtfDestinationState.Normal)
                    {
                        _ctx.FontEntries.Top?.AppendNameChar(ch);
                    }
                    break;
            }
        }

        _inHandleFontTable = false;
        return RtfError.OK;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ChangeProperty(Property propertyTableIndex, int val)
    {
        if (propertyTableIndex == Property.FontNum)
        {
            if (_ctx.GroupStack.CurrentInFontTable)
            {
                _ctx.FontEntries.Add(val);
                return;
            }
            else if (_ctx.FontEntries.TryGetValue(val, out FontEntry? fontEntry))
            {
                if (fontEntry.CodePage == 42)
                {
                    // We have to track this globally, per behavior of RichEdit and implied by the spec.
                    _lastUsedFontWithCodePage42 = val;
                }

                // Support bare characters that are supposed to be displayed in a symbol font. We use a simple
                // enum so that we don't have to do a dictionary lookup on every single character, but only
                // once per font change.
                _ctx.GroupStack.CurrentSymbolFont = GetSymbolFontTypeFromFontEntry(fontEntry);
            }
            // \fN supersedes \langN
            _ctx.GroupStack.CurrentProperties[(int)Property.Lang] = -1;
        }
        else if (propertyTableIndex == Property.Lang)
        {
            if (val == UndefinedLanguage) return;
        }

        _ctx.GroupStack.CurrentProperties[(int)propertyTableIndex] = val;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private RtfError ChangeDestination(DestinationType destinationType)
    {
        switch (destinationType)
        {
            case DestinationType.CanBeDestOrNotDest:
                // Stupid crazy type of control word, see description for enum field
                return RtfError.OK;
            case DestinationType.FieldInstruction:
                return HandleFieldInstruction();
            case DestinationType.Skip:
                _ctx.GroupStack.CurrentRtfDestinationState = RtfDestinationState.Skip;
                return RtfError.OK;
            default:
                return RtfError.InvalidSymbolTableEntry;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ParseChar(char ch)
    {
        if (ch != '\0' &&
            _ctx.GroupStack.CurrentProperties[(int)Property.Hidden] == 0)
        {
            // Support bare characters that are supposed to be displayed in a symbol font.
            GroupStack groupStack = _ctx.GroupStack;
            SymbolFont symbolFont = groupStack.CurrentSymbolFont;
            if (symbolFont > SymbolFont.Unset)
            {
                uint[] fontTable = _symbolFontTables[(int)symbolFont];
                if (GetCharFromConversionList_UInt(ch, fontTable, out ListFast<char> result))
                {
                    _plainText.AddRange(result, result.Count);
                }
            }
            else
            {
                _plainText.Add(ch);
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ParseChar_Byte(char ch)
    {
        if (ch != '\0' &&
            _ctx.GroupStack.CurrentProperties[(int)Property.Hidden] == 0)
        {
            // Support bare characters that are supposed to be displayed in a symbol font.
            GroupStack groupStack = _ctx.GroupStack;
            SymbolFont symbolFont = groupStack.CurrentSymbolFont;
            if (symbolFont > SymbolFont.Unset)
            {
                GetCharFromConversionList_Byte((byte)ch, _symbolFontTables[(int)symbolFont], out ListFast<char> result);
                _plainText.AddRange(result, result.Count);
            }
            else
            {
                _plainText.Add(ch);
            }
        }
    }

    #endregion

    #region Handle specially encoded characters

    #region Hex

    private void AddHexBuffer(bool success, bool codePageWas42, Encoding? enc, FontEntry? fontEntry)
    {
        // If multiple hex chars are directly after another (eg. \'81\'63) then they may be representing one
        // multibyte character (or not, they may also just be two single-byte chars in a row). To deal with
        // this, we have to put all contiguous hex chars into a buffer and when the run ends, we just pass
        // the buffer to the current encoding's byte-to-char decoder and get our correct result.

        ListFast<char> finalChars = _charGeneralBuffer;
        if (!success)
        {
            SetListFastToUnknownChar(finalChars);
            PutChars(finalChars, finalChars.Count);
        }
        else
        {
            // DON'T try to combine this byte with the next one if we're on code page 42 (symbol font translation) -
            // then we're guaranteed to be single-byte, and combining won't give a correct result
            if (codePageWas42)
            {
                if (fontEntry == null)
                {
                    for (int i = 0; i < _hexBuffer.Count; i++)
                    {
                        byte codePoint = _hexBuffer.ItemsArray[i];
                        GetCharFromConversionList_Byte(codePoint, _symbolFontTables[(int)SymbolFont.Symbol], out finalChars);
                        if (finalChars.Count == 0)
                        {
                            SetListFastToUnknownChar(finalChars);
                        }
                        PutChars(finalChars, finalChars.Count);
                    }
                }
                else
                {
                    SymbolFont symbolFont = GetSymbolFontTypeFromFontEntry(fontEntry);
                    if (symbolFont > SymbolFont.Unset)
                    {
                        for (int i = 0; i < _hexBuffer.Count; i++)
                        {
                            byte codePoint = _hexBuffer.ItemsArray[i];
                            GetCharFromConversionList_Byte(codePoint, _symbolFontTables[(int)symbolFont], out finalChars);
                            PutChars(finalChars, finalChars.Count);
                        }
                    }
                    else
                    {
                        for (int i = 0; i < _hexBuffer.Count; i++)
                        {
                            try
                            {
                                if (enc != null)
                                {
                                    int sourceBufferCount = _hexBuffer.Count;
                                    finalChars.EnsureCapacity(sourceBufferCount);
                                    finalChars.Count = enc
                                        .GetChars(_hexBuffer.ItemsArray, 0, sourceBufferCount,
                                            finalChars.ItemsArray, 0);
                                }
                                else
                                {
                                    SetListFastToUnknownChar(finalChars);
                                }
                            }
                            catch
                            {
                                SetListFastToUnknownChar(finalChars);
                            }
                            PutChars(finalChars, finalChars.Count);
                        }
                    }
                }
            }
            else
            {
                try
                {
                    if (enc != null)
                    {
                        int sourceBufferCount = _hexBuffer.Count;
                        finalChars.EnsureCapacity(sourceBufferCount);
                        finalChars.Count = enc
                            .GetChars(_hexBuffer.ItemsArray, 0, sourceBufferCount, finalChars.ItemsArray, 0);
                    }
                    else
                    {
                        SetListFastToUnknownChar(finalChars);
                    }
                }
                catch
                {
                    SetListFastToUnknownChar(finalChars);
                }
                PutChars(finalChars, finalChars.Count);
            }
        }
    }

    private void HandleHexRun()
    {
        _hexBuffer.ClearFast();

        (bool success, bool codePageWas42, Encoding? enc, FontEntry? fontEntry) = GetCurrentEncoding();

        /*
        If the hex is invalid, oh well, we'll just get a junk character. No point in adding more branches
        to check for what's basically never going to happen.
        Other readers' behavior:
        -RichTextBox fails the whole read on invalid hex.
        -LibreOffice just skips invalid hex chars.
        If we wanted to match LibreOffice, we'd just only add to the buffer if neither nibble was 0xFF.
        Doing so makes us a little slower. Not catastrophically so, but meh.
        */
        byte b = _rtfBytes[CurrentPos++];
        byte hexNibble1 = _charToHex[b];
        b = _rtfBytes[CurrentPos++];
        byte hexNibble2 = _charToHex[b];
        byte finalHexByte = (byte)((hexNibble1 << 4) + hexNibble2);
        _hexBuffer.Add(finalHexByte);

        while (CurrentPos < _rtfBytes.Length)
        {
            b = _rtfBytes.Array[CurrentPos++];
            if (b == '\\')
            {
                b = _rtfBytes[CurrentPos++];
                if (b == '\'')
                {
                    b = _rtfBytes[CurrentPos++];
                    hexNibble1 = _charToHex[b];
                    b = _rtfBytes[CurrentPos++];
                    hexNibble2 = _charToHex[b];
                    finalHexByte = (byte)((hexNibble1 << 4) + hexNibble2);
                    _hexBuffer.Add(finalHexByte);
                }
                else
                {
                    CurrentPos -= 2;
                    AddHexBuffer(success, codePageWas42, enc, fontEntry);
                    return;
                }
            }
            // Spaces end a hex run, but linebreaks don't.
            else if (b is not (byte)'\r' and not (byte)'\n')
            {
                CurrentPos--;
                AddHexBuffer(success, codePageWas42, enc, fontEntry);
                return;
            }
        }
    }

    #endregion

    #region Unicode

    private void HandleUnicodeRun()
    {
        int rtfLength = _rtfBytes.Length;
        while (CurrentPos < rtfLength)
        {
            char ch = (char)_rtfBytes.Array[CurrentPos++];
            if (ch == '\\')
            {
                ch = (char)_rtfBytes[CurrentPos++];

                if (ch == 'u')
                {
                    ch = (char)_rtfBytes[CurrentPos++];

                    int negateParam = 0;
                    if (ch == '-')
                    {
                        negateParam = 1;
                        ch = (char)_rtfBytes[CurrentPos++];
                    }
                    if (ch.IsAsciiNumeric())
                    {
                        int param = 0;

                        // Parse param in real-time to avoid doing a second loop over
                        for (int i = 0;
                             i < ParamMaxLen && ch.IsAsciiNumeric();
                             i++, ch = (char)_rtfBytes[CurrentPos++])
                        {
                            param = (param * 10) + (ch - '0');
                        }
                        param = BranchlessConditionalNegate(param, negateParam);

                        UnicodeBufferAdd(param);
                        CurrentPos += MinusOneIfNotSpace_8Bits(ch);
                        SkipUnicodeFallbackChars();
                    }
                    else
                    {
                        CurrentPos -= (3 + negateParam);
                        CurrentPos += MinusOneIfNotSpace_8Bits(ch);
                        ParseUnicode();
                        return;
                    }
                }
                else
                {
                    CurrentPos -= 2;
                    ParseUnicode();
                    return;
                }
            }
            else
            {
                CurrentPos--;
                ParseUnicode();
                return;
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void UnicodeBufferAdd(int param)
    {
        // Make sure the code point is normalized before adding it to the buffer!
        NormalizeUnicodePoint_HandleSymbolCharRange(param, out uint codePoint);

        // If our code point has been through a font translation table, it may be longer than 2 bytes.
        if (codePoint > char.MaxValue)
        {
            ListFast<char>? chars = ConvertFromUtf32(codePoint, _charGeneralBuffer);
            if (chars == null)
            {
                _unicodeBuffer.Add(_unicodeUnknown_Char);
            }
            else
            {
                _unicodeBuffer.AddRange(chars, 2);
            }
        }
        else
        {
            _unicodeBuffer.Add((char)codePoint);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void SkipUnicodeFallbackChars()
    {
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
        int numToSkip = _ctx.GroupStack.CurrentProperties[(int)Property.UnicodeCharSkipCount];
        while (numToSkip > 0 && CurrentPos < _rtfBytes.Length)
        {
            char c = (char)_rtfBytes.Array[CurrentPos++];
            switch (c)
            {
                case '\\':
                    if (CurrentPos < _rtfBytes.Length - 4 &&
                        _rtfBytes.Array[CurrentPos] == '\'' &&
                        _rtfBytes.Array[CurrentPos + 1].IsAsciiHex() &&
                        _rtfBytes.Array[CurrentPos + 2].IsAsciiHex())
                    {
                        CurrentPos += 3;
                        numToSkip--;
                    }
                    else if (CurrentPos < _rtfBytes.Length - 2 &&
                             _rtfBytes.Array[CurrentPos] is (byte)'{' or (byte)'}' or (byte)'\\')
                    {
                        CurrentPos++;
                        numToSkip--;
                    }
                    else
                    {
                        numToSkip--;
                    }
                    break;
                case '?':
                    numToSkip--;
                    break;
                // Per spec, if we encounter a group delimiter during Unicode skipping, we end skipping early
                case '{' or '}':
                    CurrentPos--;
                    return;
                default:
                    numToSkip--;
                    break;
            }
        }
    }

    private void ParseUnicode()
    {
        #region Handle surrogate pairs and fix up bad Unicode

        for (int i = 0; i < _unicodeBuffer.Count; i++)
        {
            char c = _unicodeBuffer.ItemsArray[i];

            if (char.IsHighSurrogate(c))
            {
                if (i < _unicodeBuffer.Count - 1 && char.IsLowSurrogate(_unicodeBuffer.ItemsArray[i + 1]))
                {
                    i++;
                }
                else
                {
                    _unicodeBuffer.ItemsArray[i] = _unicodeUnknown_Char;
                }
            }
            else if (char.IsLowSurrogate(c) || char.GetUnicodeCategory(c) is UnicodeCategory.OtherNotAssigned)
            {
                _unicodeBuffer.ItemsArray[i] = _unicodeUnknown_Char;
            }
        }

        #endregion

        PutChars(_unicodeBuffer, _unicodeBuffer.Count);

        _unicodeBuffer.ClearFast();
    }

    #endregion

    #region Field instructions

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
    */
    private RtfError HandleFieldInstruction()
    {
        _fldinstSymbolNumber.ClearFast();
        _fldinstSymbolFontName.ClearFast();

        int param;

        char ch = (char)_rtfBytes[CurrentPos++];

        #region Check for SYMBOL instruction

        // Straight-up just check for S, because SYMBOL is the only word we care about.
        if (ch != 'S') return RewindAndSkipGroup();

        int i;
        for (i = 0; i < 6; i++, ch = (char)_rtfBytes[CurrentPos++])
        {
            if (ch != _SYMBOLChars[i]) return RewindAndSkipGroup();
        }

        #endregion

        ch = (char)_rtfBytes[CurrentPos++];

        bool numIsHex = false;
        int negateNum = 0;

        #region Parse numeric field parameter

        if (ch == '-')
        {
            ch = (char)_rtfBytes[CurrentPos++];
            negateNum = 1;
        }

        #region Handle if the param is hex

        if (ch == '0')
        {
            ch = (char)_rtfBytes[CurrentPos++];
            if (ch is 'x' or 'X')
            {
                ch = (char)_rtfBytes[CurrentPos++];
                if (ch == '-')
                {
                    ch = (char)_rtfBytes[CurrentPos++];
                    if (ch != ' ') return RewindAndSkipGroup();
                    negateNum = 1;
                }
                numIsHex = true;
            }
        }

        #endregion

        #region Read parameter

        bool alphaCharsFound = false;
        bool alphaFound;
        for (i = 0;
             i < _fldinstSymbolNumberMaxLen && ((alphaFound = ch.IsAsciiAlpha()) || ch.IsAsciiNumeric());
             i++, ch = (char)_rtfBytes[CurrentPos++])
        {
            if (alphaFound) alphaCharsFound = true;

            _fldinstSymbolNumber.Add(ch);
        }

        if (_fldinstSymbolNumber.Count == 0 ||
            i >= _fldinstSymbolNumberMaxLen ||
            (!numIsHex && alphaCharsFound))
        {
            return RewindAndSkipGroup();
        }

        #endregion

        #region Parse parameter

        if (numIsHex)
        {
            // @ALLOC: ToString(): int.TryParse(hex)
            // We could implement our own hex parser, but this is called so infrequently (actually not at all
            // in the test set) that it doesn't really matter.
            // TODO: Make our own parser anyway, because speed in all things
            if (!int.TryParse(CreateStringFromChars(_fldinstSymbolNumber),
                    NumberStyles.HexNumber,
                    NumberFormatInfo.InvariantInfo,
                    out param))
            {
                return RewindAndSkipGroup();
            }
        }
        else
        {
            param = ParseIntFast(_fldinstSymbolNumber);
        }

        #endregion

        #endregion

        param = BranchlessConditionalNegate(param, negateNum);

        // TODO: Do we need to handle 0xF020-0xF0FF type stuff and negative values for field instructions?
        NormalizeUnicodePoint(param, out uint codePoint);

        if (ch != ' ') return RewindAndSkipGroup();

        const int maxParams = 6;
        const int useCurrentGroupCodePage = -1;

        ListFast<char> finalChars = _charGeneralBuffer;
        finalChars.Count = 0;

        #region Parse params

        for (i = 0; i < maxParams; i++)
        {
            ch = (char)_rtfBytes[CurrentPos++];
            if (ch != '\\') continue;
            ch = (char)_rtfBytes[CurrentPos++];
            if (ch != '\\') continue;

            ch = (char)_rtfBytes[CurrentPos++];

            // From the spec:
            // "Interprets text in field-argument as the value of an ANSI character."
            if (ch == 'a')
            {
                finalChars = GetCharFromCodePage(_windows1252, codePoint);
                break;
            }
            /*
            From the spec:
            "Interprets text in field-argument as the value of a SHIFT-JIS character."

            Note that "SHIFT-JIS" in RTF land means specifically Windows-31J or whatever you want to call it.
            */
            else if (ch == 'j')
            {
                finalChars = GetCharFromCodePage(_shiftJisWin, codePoint);
                break;
            }
            else if (ch == 'u')
            {
                ListFast<char>? chars = ConvertFromUtf32(codePoint, _charGeneralBuffer);
                if (chars != null)
                {
                    finalChars = chars;
                }
                else
                {
                    SetListFastToUnknownChar(finalChars);
                }
                break;
            }
            /*
            From the spec:
            "Interprets text in the switch's field-argument as the name of the font from which the character
            whose value is specified by text in the field's field-argument. By default, the font used is that
            for the current text run."

            In other words:
            If it's \\f, we use the current group's font's codepage, and if it's \\f "FontName" then we use
            code page 1252 and convert from FontName to Unicode using manual conversion tables, that is
            assuming FontName is "Symbol", "Wingdings" or "Webdings". We don't really want to go down the
            rabbit hole of Wingdings 2, Wingdings 3, or whatever else have you...

            Note that RichEdit doesn't go this far. It reads the fldinst parts and all, and displays the
            characters in rich text if you have the appropriate fonts, but it doesn't convert from the fonts
            to equivalent Unicode chars when it converts to plain text. Which is reasonable really, but we
            want to do it because it's some cool ninja shit and also it helps us keep the odd copyright symbol
            intact and stuff.
            */
            else if (ch == 'f')
            {
                ch = (char)_rtfBytes[CurrentPos++];
                if (IsSeparatorChar(ch))
                {
                    finalChars = GetCharFromCodePage(useCurrentGroupCodePage, codePoint);
                    break;
                }
                else if (ch == ' ')
                {
                    ch = (char)_rtfBytes[CurrentPos++];
                    if (ch != '\"')
                    {
                        finalChars = GetCharFromCodePage(useCurrentGroupCodePage, codePoint);
                        break;
                    }

                    int fontNameCharCount = 0;

                    while ((ch = (char)_rtfBytes[CurrentPos++]) != '\"')
                    {
                        if (fontNameCharCount >= _fldinstSymbolFontNameMaxLen || IsSeparatorChar(ch))
                        {
                            return RewindAndSkipGroup();
                        }
                        _fldinstSymbolFontName.Add(ch);
                        fontNameCharCount++;
                    }

                    // Just hardcoding the three most common fonts here, because there's only so far you
                    // really want to go down this path.
                    if (SeqEqual(_fldinstSymbolFontName, _symbolChars) &&
                        !GetCharFromConversionList_UInt(codePoint, _symbolFontToUnicode, out finalChars))
                    {
                        return RewindAndSkipGroup();
                    }
                    else if (SeqEqual(_fldinstSymbolFontName, _wingdingsChars) &&
                             !GetCharFromConversionList_UInt(codePoint, _wingdingsFontToUnicode, out finalChars))
                    {
                        return RewindAndSkipGroup();
                    }
                    else if (SeqEqual(_fldinstSymbolFontName, _webdingsChars) &&
                             !GetCharFromConversionList_UInt(codePoint, _webdingsFontToUnicode, out finalChars))
                    {
                        return RewindAndSkipGroup();
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
                ch = (char)_rtfBytes[CurrentPos++];
                if (IsSeparatorChar(ch)) break;
            }
            /*
            From the spec:
            "Interprets text in the switch's field-argument as the integral font size in points."

            This one takes an argument (hence the extra logic to ignore it), but, yeah, we ignore it.
            */
            else if (ch == 's')
            {
                ch = (char)_rtfBytes[CurrentPos++];
                if (ch != ' ') return RewindAndSkipGroup();

                int numDigitCount = 0;
                while ((ch = (char)_rtfBytes[CurrentPos++]).IsAsciiNumeric())
                {
                    if (numDigitCount > _fldinstSymbolNumberMaxLen) goto breakout;
                    numDigitCount++;
                }

                if (IsSeparatorChar(ch)) break;
            }
        }

        breakout:

        #endregion

        if (finalChars.Count > 0) PutChars(finalChars, finalChars.Count);

        return RewindAndSkipGroup();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsSeparatorChar(char ch) => ch is '\\' or '{' or '}';

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private unsafe ListFast<char> GetCharFromCodePage(int codePage, uint codePoint)
    {
        // BitConverter.GetBytes() does this, but it allocates a temp array every time.
        // I think I understand the general idea here but like yeah
        fixed (byte* b = _byteBuffer4) *(uint*)b = codePoint;

        try
        {
            if (codePage > -1)
            {
                _charGeneralBuffer.Count = GetEncodingFromCachedList(codePage)
                    .GetChars(_byteBuffer4, 0, 4, _charGeneralBuffer.ItemsArray, 0);
                return _charGeneralBuffer;
            }
            else
            {
                (bool success, _, Encoding? enc, _) = GetCurrentEncoding();
                if (success && enc != null)
                {
                    _charGeneralBuffer.Count = enc
                        .GetChars(_byteBuffer4, 0, 4, _charGeneralBuffer.ItemsArray, 0);
                    return _charGeneralBuffer;
                }
                else
                {
                    SetListFastToUnknownChar(_charGeneralBuffer);
                    return _charGeneralBuffer;
                }
            }
        }
        catch
        {
            SetListFastToUnknownChar(_charGeneralBuffer);
            return _charGeneralBuffer;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private RtfError RewindAndSkipGroup()
    {
        CurrentPos--;
        _ctx.GroupStack.CurrentRtfDestinationState = RtfDestinationState.Skip;
        return RtfError.OK;
    }

    #endregion

    #endregion

    #region PutChar

    private void PutChars(ListFast<char> ch, int count)
    {
        // This is only ever called from encoded-char handlers (hex, Unicode, field instructions), so we don't
        // need to duplicate any of the bare-char symbol font stuff here.

        if (!(count == 1 && ch[0] == '\0') &&
            _ctx.GroupStack.CurrentProperties[(int)Property.Hidden] == 0 &&
            !_ctx.GroupStack.CurrentInFontTable)
        {
            _plainText.AddRange(ch, count);
        }
    }

    #endregion

    #region Helpers

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string CreateStringFromChars(ListFast<char> chars) => new(chars.ItemsArray, 0, chars.Count);

    /// <summary>
    /// If <paramref name="codePage"/> is in the cached list, returns the Encoding associated with it;
    /// otherwise, gets the Encoding for <paramref name="codePage"/> and places it in the cached list
    /// for next time.
    /// </summary>
    /// <param name="codePage"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Encoding GetEncodingFromCachedList(int codePage)
    {
        switch (codePage)
        {
            case _windows1252:
                return _windows1252Encoding;
            case 1250:
                return _windows1250Encoding;
            case 1251:
                return _windows1251Encoding;
            case _shiftJisWin:
                return _shiftJisWinEncoding;
            default:
                if (_encodings.TryGetValue(codePage, out Encoding result))
                {
                    return result;
                }
                else
                {
                    // NOTE: This can throw, but all calls to this are wrapped in try-catch blocks.
                    // TODO: But weird that we don't put the try-catch here and just return null...?
                    Encoding enc = Encoding.GetEncoding(codePage);
                    _encodings[codePage] = enc;
                    return enc;
                }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private (bool Success, bool CodePageWas42, Encoding? Encoding, FontEntry? FontEntry)
    GetCurrentEncoding()
    {
        int groupFontNum = _ctx.GroupStack.CurrentProperties[(int)Property.FontNum];
        int groupLang = _ctx.GroupStack.CurrentProperties[(int)Property.Lang];

        if (groupFontNum == NoFontNumber) groupFontNum = _ctx.Header.DefaultFontNum;

        _ctx.FontEntries.TryGetValue(groupFontNum, out FontEntry? fontEntry);

        int codePage;
        if (groupLang is > -1 and <= MaxLangNumIndex)
        {
            int translatedCodePage = LangToCodePage[groupLang];
            codePage = translatedCodePage > -1 ? translatedCodePage : fontEntry?.CodePage >= 0 ? fontEntry.CodePage : _ctx.Header.CodePage;
        }
        else
        {
            codePage = fontEntry?.CodePage >= 0 ? fontEntry.CodePage : _ctx.Header.CodePage;
        }

        if (codePage == 42) return (true, true, null, fontEntry);

        // Awful, but we're based on nice, relaxing error returns, so we don't want to throw exceptions. Ever.
        Encoding enc;
        try
        {
            enc = GetEncodingFromCachedList(codePage);
        }
        catch
        {
            try
            {
                enc = GetEncodingFromCachedList(_windows1252);
            }
            catch
            {
                return (false, false, null, null);
            }
        }

        return (true, false, enc, fontEntry);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private SymbolFont GetSymbolFontTypeFromFontEntry(FontEntry fontEntry)
    {
        if (fontEntry.SymbolFont == SymbolFont.Unset)
        {
            fontEntry.SymbolFont =
                fontEntry.NameEquals(_symbolChars) ? SymbolFont.Symbol :
                fontEntry.NameEquals(_wingdingsChars) ? SymbolFont.Wingdings :
                fontEntry.NameEquals(_webdingsChars) ? SymbolFont.Webdings :
                SymbolFont.None;
        }

        return fontEntry.SymbolFont;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool GetCharFromConversionList_UInt(uint codePoint, uint[] fontTable, out ListFast<char> finalChars)
    {
        finalChars = _charGeneralBuffer;

        if (codePoint - 0x20 <= 0xFF - 0x20)
        {
            ListFast<char>? chars = ConvertFromUtf32(fontTable[codePoint - 0x20], _charGeneralBuffer);
            if (chars != null)
            {
                finalChars = chars;
            }
            else
            {
                SetListFastToUnknownChar(finalChars);
            }
        }
        else
        {
            if (codePoint > 255)
            {
                finalChars.Count = 0;
                return false;
            }
            try
            {
                _byteBuffer1[0] = (byte)codePoint;
                finalChars.Count = GetEncodingFromCachedList(_windows1252)
                    .GetChars(_byteBuffer1, 0, 1, finalChars.ItemsArray, 0);
            }
            catch
            {
                SetListFastToUnknownChar(finalChars);
            }
        }

        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void GetCharFromConversionList_Byte(byte codePoint, uint[] fontTable, out ListFast<char> finalChars)
    {
        finalChars = _charGeneralBuffer;

        if (codePoint >= 0x20)
        {
            ListFast<char>? chars = ConvertFromUtf32(fontTable[codePoint - 0x20], _charGeneralBuffer);
            if (chars != null)
            {
                finalChars = chars;
            }
            else
            {
                SetListFastToUnknownChar(finalChars);
            }
        }
        else
        {
            try
            {
                _byteBuffer1[0] = codePoint;
                finalChars.Count = GetEncodingFromCachedList(_windows1252)
                    .GetChars(_byteBuffer1, 0, 1, finalChars.ItemsArray, 0);
            }
            catch
            {
                SetListFastToUnknownChar(finalChars);
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void NormalizeUnicodePoint_HandleSymbolCharRange(int codePoint, out uint returnCodePoint)
    {
        // Per spec, values >32767 are expressed as negative numbers, and we must add 65536 to get the
        // correct value.
        if (codePoint < 0)
        {
            codePoint += 65536;
            if (codePoint < 0)
            {
                returnCodePoint = _unicodeUnknown_Char;
                return;
            }
        }

        returnCodePoint = (uint)codePoint;

        /*
        From the spec:
        "Occasionally Word writes SYMBOL_CHARSET (nonUnicode) characters in the range U+F020..U+F0FF instead
        of U+0020..U+00FF. Internally Word uses the values U+F020..U+F0FF for these characters so that plain-
        text searches don't mistakenly match SYMBOL_CHARSET characters when searching for Unicode characters
        in the range U+0020..U+00FF. To find out the correct symbol font to use, e.g., Wingdings, Symbol,
        etc., find the last SYMBOL_CHARSET font control word \fN used, look up font N in the font table and
        find the face name. The charset is specified by the \fcharsetN control word and SYMBOL_CHARSET is for
        N = 2. This corresponds to codepage 42."

        Verified, this does in fact mean "find the last used font that specifically has \fcharset2" (or \cpg42).
        And, yes, that's last used, period, regardless of group. So we track it globally. That's the official
        behavior, don't ask me.

        Verified, these 0xF020-0xF0FF chars can be represented either as negatives or as >32767 positives
        (despite the spec saying that \uN must be signed int16). So we need to fall through to this section
        even if we did the above, because by adding 65536 we might now be in the 0xF020-0xF0FF range.
        */
        if (returnCodePoint - 0xF020 <= 0xF0FF - 0xF020)
        {
            returnCodePoint -= 0xF000;

            int fontNum = _lastUsedFontWithCodePage42 > NoFontNumber
                ? _lastUsedFontWithCodePage42
                : _ctx.Header.DefaultFontNum;

            if (!_ctx.FontEntries.TryGetValue(fontNum, out FontEntry? fontEntry) || fontEntry.CodePage != 42)
            {
                return;
            }

            // We already know our code point is within bounds of the array, because the arrays also go from
            // 0x20 - 0xFF, so no need to check.
            SymbolFont symbolFont = GetSymbolFontTypeFromFontEntry(fontEntry);
            if (symbolFont > SymbolFont.Unset)
            {
                returnCodePoint = _symbolFontTables[(int)symbolFont][returnCodePoint - 0x20];
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void NormalizeUnicodePoint(int codePoint, out uint returnCodePoint)
    {
        // Per spec, values >32767 are expressed as negative numbers, and we must add 65536 to get the
        // correct value.
        if (codePoint < 0)
        {
            codePoint += 65536;
            if (codePoint < 0)
            {
                returnCodePoint = _unicodeUnknown_Char;
                return;
            }
        }

        returnCodePoint = (uint)codePoint;
    }

    /// <summary>
    /// Only call this if <paramref name="chars"/>'s length is > 0 and consists solely of the characters '0' through '9'.
    /// It does no checks at all and will throw if either of these things is false.
    /// </summary>
    /// <param name="chars"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int ParseIntFast(ListFast<char> chars)
    {
        int result = chars.ItemsArray[0] - '0';

        for (int i = 1; i < chars.Count; i++)
        {
            result *= 10;
            result += chars.ItemsArray[i] - '0';
        }
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool SeqEqual(ListFast<char> seq1, char[] seq2)
    {
        int seq1Count = seq1.Count;
        if (seq1Count != seq2.Length) return false;

        for (int ci = 0; ci < seq1Count; ci++)
        {
            if (seq1.ItemsArray[ci] != seq2[ci]) return false;
        }
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void SetListFastToUnknownChar(ListFast<char> list)
    {
        list.ItemsArray[0] = _unicodeUnknown_Char;
        list.Count = 1;
    }

    #endregion
}
