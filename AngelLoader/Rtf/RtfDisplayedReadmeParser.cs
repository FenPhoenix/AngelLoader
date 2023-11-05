using System.Collections.Generic;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Text;
using AL_Common;
using JetBrains.Annotations;
using static AL_Common.Common;
using static AL_Common.RTFParserCommon;

namespace AngelLoader;

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

    internal sealed class LangItem
    {
        private static int GetDigitsUpTo5(int number) =>
            number <= 9 ? 1 :
            number <= 99 ? 2 :
            number <= 999 ? 3 :
            number <= 9999 ? 4 :
            5;

        internal int Index;
        internal readonly int CodePage;
        internal readonly int DigitsCount;

        internal LangItem(int index, int codePage)
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
    internal (bool Success, List<Color>? ColorTable, List<LangItem>? LangItems)
    GetData(ArrayWithLength<byte> rtfBytes, bool getColorTable, bool getLangs)
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

    private void Reset(ArrayWithLength<byte> rtfBytes)
    {
        ResetBase();
        // Don't carry around the font entry pool for the entire app lifetime
        _ctx.FontEntries.ClearFull(0);

        #region Fixed-size fields

        _foundColorTable = false;
        _getColorTable = false;
        _getLangs = false;

        #endregion

        _colorTable = null;
        _langItems = null;

        _rtfBytes = rtfBytes;

        CurrentPos = 0;
    }

    private RtfError ParseRtf()
    {
        while (CurrentPos < _rtfBytes.Length)
        {
            if (!_getLangs && _getColorTable && _foundColorTable) return RtfError.OK;

            char ch = (char)_rtfBytes[CurrentPos++];

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
                    break;
                case '\\':
                    RtfError ec = ParseKeyword();
                    if (ec != RtfError.OK) return ec;
                    break;
                case '\r':
                case '\n':
                    break;
            }
        }

        return _groupCount > 0 ? RtfError.UnmatchedBrace : RtfError.OK;
    }

    #region Act on keywords

    private RtfError DispatchKeyword(int param, bool hasParam)
    {
        if (!Symbols.TryGetValue(_ctx.Keyword, out Symbol? symbol))
        {
            // If this is a new destination
            if (_skipDestinationIfUnknown)
            {
                _ctx.GroupStack.CurrentRtfDestinationState = RtfDestinationState.Skip;
            }
            _skipDestinationIfUnknown = false;
            return RtfError.OK;
        }

        _skipDestinationIfUnknown = false;
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
                CurrentPos += param;
                if (CurrentPos >= _rtfBytes.Length) return RtfError.EndOfFile;
                break;
            case SpecialType.SkipDest:
                _skipDestinationIfUnknown = true;
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
            if (groupFontNum == -1) groupFontNum = _ctx.Header.DefaultFontNum;

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

    // @MEM(Color table parser): We could still reduce allocations in here a bit more (but by making the code even more terrible)
    private RtfError ParseAndBuildColorTable()
    {
        var _colorTableSB = new StringBuilder(4096);

        RtfError ClearReturnFields(RtfError error)
        {
            _colorTable = null;
            return error;
        }

        ClearReturnFields(RtfError.OK);

        while (true)
        {
            if (!GetNextChar(out char ch)) return ClearReturnFields(RtfError.EndOfFile);
            if (ch == '}')
            {
                CurrentPos--;
                break;
            }
            _colorTableSB.Append(ch);
        }

        string[] entries = _colorTableSB.ToString().Split(CA_Semicolon);

        int realEntryCount = entries.Length;
        if (entries.Length == 0)
        {
            return ClearReturnFields(RtfError.OK);
        }
        // Remove the last blank entry so we don't count it as the auto/default one by hitting a blank entry
        // in the loop below
        else if (entries.Length > 1 && entries[entries.Length - 1].IsWhiteSpace())
        {
            realEntryCount--;
        }

        _colorTable = new List<Color>(realEntryCount);

        for (int i = 0; i < realEntryCount; i++)
        {
            string entry = entries[i].Trim();

            if (entry.IsEmpty())
            {
                // 0 alpha will be the flag for "this is the omitted default/auto color"
                _colorTable.Add(Color.FromArgb(0, 0, 0, 0));
            }
            else
            {
                const string redString = "\\red";
                const int redStringLen = 4;
                const string greenString = "\\green";
                const int greenStringLen = 6;
                const string blueString = "\\blue";
                const int blueStringLen = 5;

                static bool GetColorByte(string entry, string hueString, int hueStringLen, out byte result)
                {
                    int hueIndex = FindIndexOfCharSequence(entry, hueString);
                    if (hueIndex > -1)
                    {
                        int indexPastHue = hueIndex + hueStringLen;
                        if (indexPastHue < entry.Length)
                        {
                            char firstDigit = entry[indexPastHue];
                            if (firstDigit.IsAsciiNumeric())
                            {
                                int colorValue = firstDigit - '0';
                                for (int colorI = indexPastHue + 1; colorI < entry.Length; colorI++)
                                {
                                    char c = entry[colorI];
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

                if (GetColorByte(entry, redString, redStringLen, out byte red) &&
                    GetColorByte(entry, greenString, greenStringLen, out byte green) &&
                    GetColorByte(entry, blueString, blueStringLen, out byte blue))
                {
                    _colorTable.Add(Color.FromArgb(red, green, blue));
                }
            }
        }

        return RtfError.OK;
    }
}
