﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using SpanExtensions;
using static AL_Common.Common;
using static AL_Common.RTFParserCommon;

namespace AL_Common;

// @DarkModeNote(RtfColorTableParser):
// We use a full parser here because rather than simply replacing all byte sequences with another, here we
// need to parse and return the first and ONLY the first {\colortbl} group. In theory that could be in a
// comment or invalidly in the middle of another group or something. I mean it won't, let's be honest, but
// the color table is important enough to take the perf hit and the small amount of code duplication.

public sealed partial class RtfDisplayedReadmeParser
{
    #region Resettables

    private List<Color>? _colorTable;
    private bool _foundColorTable;
    private bool _getColorTable;
    private bool _getLangs;

    public sealed class LangItem
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

        public LangItem(int index, int codePage)
        {
            Index = index;
            CodePage = codePage;
            DigitsCount = GetDigitsUpTo5(codePage);
        }
    }

    private List<LangItem>? _langItems;

    #endregion

    #region Public API

    [PublicAPI]
    public (bool Success, List<Color>? ColorTable, List<LangItem>? LangItems)
    GetData(in ArrayWithLength<byte> rtfBytes, bool getColorTable, bool getLangs)
    {
        try
        {
            // Reset before because at least one thing (current group) needs it in order to be in a valid
            // state to start with
            Reset(rtfBytes);

            _getColorTable = getColorTable;
            _getLangs = getLangs;

            if (!getLangs && !getColorTable)
            {
                return (false, ColorTable: _colorTable, LangItems: _langItems);
            }

            RtfError error = ParseRtf();
            return error == RtfError.OK
                ? (true, ColorTable: _colorTable, LangItems: _langItems)
                : (false, ColorTable: _colorTable, LangItems: _langItems);
        }
        catch
        {
            return (false, _colorTable, _langItems);
        }
        finally
        {
            // Reset after so we don't carry around any waste after running
            Reset(ArrayWithLength<byte>.Empty());
        }
    }

    #endregion

    private void Reset(in ArrayWithLength<byte> rtfBytes)
    {
        ResetBase(rtfBytes);
        // Don't carry around the font entry pool for the entire app lifetime
        _ctx.FontEntries.ClearFull(0);

        #region Fixed-size fields

        _foundColorTable = false;
        _getColorTable = false;
        _getLangs = false;

        #endregion

        _colorTable = null;
        _langItems = null;
    }

    private RtfError ParseRtf()
    {
        while (CurrentPos < _rtfBytes.Length)
        {
            if (!_getLangs && _getColorTable && _foundColorTable) return RtfError.OK;

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
                    if (_groupCount == 0) return RtfError.OK;
                    break;
                case '\r':
                case '\n':
                    break;
            }
        }

        return _groupCount > 0 ? RtfError.UnmatchedBrace : RtfError.OK;
    }

    #region Act on keywords

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private RtfError DispatchKeyword(Symbol symbol, int param, bool hasParam)
    {
        switch (symbol.KeywordType)
        {
            case KeywordType.Property:
                if (_getLangs && _ctx.GroupStack.CurrentRtfDestinationState == RtfDestinationState.Normal)
                {
                    if (symbol.UseDefaultParam || !hasParam) param = symbol.DefaultParam;
                    ChangeProperty((Property)symbol.Index, param);
                }
                return RtfError.OK;
            case KeywordType.Destination:
                return symbol.Index == (int)DestinationType.SkippableHex
                    ? HandleSkippableHexData()
                    : _ctx.GroupStack.CurrentRtfDestinationState == RtfDestinationState.Normal
                        ? ChangeDestination((DestinationType)symbol.Index)
                        : RtfError.OK;
            case KeywordType.Special:
                var specialType = (SpecialType)symbol.Index;
                return _ctx.GroupStack.CurrentRtfDestinationState == RtfDestinationState.Normal ||
                       specialType == SpecialType.SkipNumberOfBytes
                    ? DispatchSpecialKeyword(specialType, symbol, param)
                    : RtfError.OK;
            default:
                //return RtfError.InvalidSymbolTableEntry;
                return RtfError.OK;
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
            case SpecialType.ColorTable:
                // Spec is to ignore any further color tables after the first one
                if (_getColorTable && !_foundColorTable)
                {
                    _foundColorTable = true;
                    return ParseAndBuildColorTable();
                }
                else
                {
                    return RtfError.OK;
                }
            default:
                HandleSpecialTypeFont(_ctx, specialType, param);
                return RtfError.OK;
        }

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

            // \fN supersedes \langN
            _ctx.GroupStack.CurrentProperties[(int)Property.Lang] = -1;
        }
        else if (propertyTableIndex == Property.Lang)
        {
            int currentLang = _ctx.GroupStack.CurrentProperties[(int)Property.Lang];

            int groupFontNum = _ctx.GroupStack.CurrentProperties[(int)Property.FontNum];
            if (groupFontNum == NoFontNumber) groupFontNum = _ctx.Header.DefaultFontNum;

            _ctx.FontEntries.TryGetValue(groupFontNum, out FontEntry? fontEntry);

            int currentCodePage = fontEntry?.CodePage >= 0 ? fontEntry.CodePage : _ctx.Header.CodePage;

            if (currentLang > -1 && currentLang != UndefinedLanguage && val != UndefinedLanguage)
            {
                if (val is > -1 and <= MaxLangNumIndex)
                {
                    int langCodePage = LangToCodePage[val];
                    if (langCodePage == -1)
                    {
                        _langItems ??= new List<LangItem>();
                        _langItems.Add(new LangItem(CurrentPos, currentCodePage));
                    }
                }
            }
            else
            {
                if (val is > -1 and <= MaxLangNumIndex)
                {
                    int langCodePage = LangToCodePage[val];
                    if (langCodePage > -1 && langCodePage != currentCodePage)
                    {
                        _langItems ??= new List<LangItem>();
                        _langItems.Add(new LangItem(CurrentPos, langCodePage));
                    }
                }

                if (val == UndefinedLanguage) return;
            }
        }

        _ctx.GroupStack.CurrentProperties[(int)propertyTableIndex] = val;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private RtfError ChangeDestination(DestinationType destinationType)
    {
        switch (destinationType)
        {
            // CanBeDestOrNotDest is only relevant for plaintext extraction. As we're only parsing color tables,
            // we can just skip groups so marked.
            // TODO: Update and diff-test this with our new knowledge: we should skip the group only if it was a destination!
            case DestinationType.CanBeDestOrNotDest:
            case DestinationType.Skip:
                _ctx.GroupStack.CurrentRtfDestinationState = RtfDestinationState.Skip;
                return RtfError.OK;
            default:
                return RtfError.OK;
        }
    }

    #endregion

    private RtfError ParseAndBuildColorTable()
    {
        ClearReturnFields(RtfError.OK);

        int closingBraceIndex = Array.IndexOf(_rtfBytes.Array, (byte)'}', CurrentPos, _rtfBytes.Length - CurrentPos);
        if (closingBraceIndex == -1) return ClearReturnFields(RtfError.OK);

        ReadOnlySpan<byte> colorTableSpan = _rtfBytes.Array.AsSpan(CurrentPos, closingBraceIndex - CurrentPos);

        _colorTable = new List<Color>(colorTableSpan.Count((byte)';'));

        bool first = true;
        foreach (ReadOnlySpan<byte> entry in colorTableSpan.Split((byte)';'))
        {
            if (entry.IsWhiteSpace())
            {
                if (first)
                {
                    // 0 alpha will be the flag for "this is the omitted default/auto color"
                    _colorTable.Add(Color.FromArgb(0, 0, 0, 0));
                }
            }
            else
            {
                ReadOnlySpan<byte> redString = "\\red"u8;
                ReadOnlySpan<byte> greenString = "\\green"u8;
                ReadOnlySpan<byte> blueString = "\\blue"u8;

                if (GetColorByte(entry, redString, out byte red) &&
                    GetColorByte(entry, greenString, out byte green) &&
                    GetColorByte(entry, blueString, out byte blue))
                {
                    _colorTable.Add(Color.FromArgb(red, green, blue));
                }
            }
            first = false;
        }

        return first ? ClearReturnFields(RtfError.OK) : RtfError.OK;

        RtfError ClearReturnFields(RtfError error)
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
}
