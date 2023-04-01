using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Text;
using JetBrains.Annotations;
using static AL_Common.Common;

namespace AngelLoader;

// @DarkModeNote(RtfColorTableParser):
// We use a full parser here because rather than simply replacing all byte sequences with another, here we
// need to parse and return the first and ONLY the first {\colortbl} group. In theory that could be in a
// comment or invalidly in the middle of another group or something. I mean it won't, let's be honest, but
// the color table is important enough to take the perf hit and the small amount of code duplication.

public sealed class RtfDisplayedReadmeParser : AL_Common.RTFParserBase
{
    #region Private fields

    private byte[] _rtfBytes = Array.Empty<byte>();

    #endregion

    #region Stream

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected override byte StreamReadByte() => _rtfBytes[(int)CurrentPos];

    #endregion

    #region Resettables

    private List<Color>? _colorTable;
    private int _colorTableStartIndex;
    private int _colorTableEndIndex;
    private bool _foundColorTable;
    private bool _getColorTable;
    private bool _getLangs;

    public sealed class LangItem
    {
        public int Index;
        public readonly int CodePage;
        public int DigitsCount;

        public LangItem(int index, int codePage)
        {
            Index = index;
            CodePage = codePage;
        }
    }

    private List<LangItem>? _langItems;

    #endregion

    #region Public API

    [PublicAPI]
    public (bool Success, List<Color>? ColorTable, int ColorTableStartIndex, int ColorTableEndIndex, List<LangItem>? LangItems)
    GetData(byte[] rtfBytes, bool getColorTable, bool getLangs)
    {
        try
        {
            // Reset before because at least one thing (current scope) needs it in order to be in a valid
            // state to start with
            Reset(rtfBytes);

            _getColorTable = getColorTable;
            _getLangs = getLangs;

            if (!getLangs && !getColorTable)
            {
                return (false, ColorTable: _colorTable, ColorTableStartIndex: _colorTableStartIndex,
                    ColorTableEndIndex: _colorTableEndIndex,
                    LangItems: _langItems);
            }

            Error error = ParseRtf();
            return error == Error.OK
                ? (true, ColorTable: _colorTable, ColorTableStartIndex: _colorTableStartIndex,
                    ColorTableEndIndex: _colorTableEndIndex,
                    LangItems: _langItems)
                : (false, ColorTable: _colorTable, ColorTableStartIndex: _colorTableStartIndex,
                    ColorTableEndIndex: _colorTableEndIndex,
                    LangItems: _langItems);
        }
        catch
        {
            return (false, _colorTable, _colorTableStartIndex, _colorTableEndIndex, _langItems);
        }
        finally
        {
            // Reset after so we don't carry around any waste after running
            Reset(Array.Empty<byte>());
        }
    }

    #endregion

    private void Reset(byte[] rtfBytes)
    {
        base.ResetBase();

        #region Fixed-size fields

        _colorTableStartIndex = 0;
        _colorTableEndIndex = 0;
        _foundColorTable = false;
        _getColorTable = false;
        _getLangs = false;

        #endregion

        _colorTable = null;
        _langItems = null;

        _rtfBytes = rtfBytes;
        base.ResetStreamBase(rtfBytes.Length);
    }

    private Error ParseRtf()
    {
        while (CurrentPos < Length)
        {
            if (!_getLangs && _getColorTable && _foundColorTable) return Error.OK;

            char ch = GetNextCharFast();

            if (_groupCount < 0) return Error.StackUnderflow;

            if (_currentScope.RtfInternalState == RtfInternalState.Binary)
            {
                if (--_binaryCharsLeftToSkip <= 0)
                {
                    _currentScope.RtfInternalState = RtfInternalState.Normal;
                    _binaryCharsLeftToSkip = 0;
                }
                continue;
            }

            Error ec;
            switch (ch)
            {
                case '{':
                    if ((ec = PushScope()) != Error.OK) return ec;
                    break;
                case '}':
                    if ((ec = PopScope()) != Error.OK) return ec;
                    break;
                case '\\':
                    if ((ec = ParseKeyword()) != Error.OK) return ec;
                    break;
                case '\r':
                case '\n':
                    break;
            }
        }

        return _groupCount < 0 ? Error.StackUnderflow : _groupCount > 0 ? Error.UnmatchedBrace : Error.OK;
    }

    #region Act on keywords

    protected override Error DispatchKeyword(int param, bool hasParam)
    {
        if (!Symbols.TryGetValue(_keyword, out Symbol? symbol))
        {
            // If this is a new destination
            if (_skipDestinationIfUnknown)
            {
                _currentScope.RtfDestinationState = RtfDestinationState.Skip;
            }
            _skipDestinationIfUnknown = false;
            return Error.OK;
        }

        _skipDestinationIfUnknown = false;
        switch (symbol.KeywordType)
        {
            case KeywordType.Property:
                if (symbol.UseDefaultParam || !hasParam) param = symbol.DefaultParam;
                return _getLangs && _currentScope.RtfDestinationState == RtfDestinationState.Normal
                    ? ChangeProperty((Property)symbol.Index, param)
                    : Error.OK;
            case KeywordType.Destination:
                return _currentScope.RtfDestinationState == RtfDestinationState.Normal
                    ? ChangeDestination((DestinationType)symbol.Index)
                    : Error.OK;
            case KeywordType.Special:
                var specialType = (SpecialType)symbol.Index;
                return _currentScope.RtfDestinationState == RtfDestinationState.Normal ||
                       specialType == SpecialType.Bin
                    ? DispatchSpecialKeyword(specialType, param)
                    : Error.OK;
            default:
                //return Error.InvalidSymbolTableEntry;
                return Error.OK;
        }
    }

    private Error DispatchSpecialKeyword(SpecialType specialType, int param)
    {
        switch (specialType)
        {
            case SpecialType.Bin:
                if (param > 0)
                {
                    _currentScope.RtfInternalState = RtfInternalState.Binary;
                    _binaryCharsLeftToSkip = param;
                }
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
                    return Error.OK;
                }
            default:
                return Error.OK;
        }

        return Error.OK;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Error ChangeProperty(Property propertyTableIndex, int val)
    {
        if (propertyTableIndex == Property.FontNum)
        {
            if (_currentScope.InFontTable)
            {
                _fontEntries.Add(val, new FontEntry());
                return Error.OK;
            }

            // \fN supersedes \langN
            _currentScope.Properties[(int)Property.Lang] = -1;
        }
        else if (propertyTableIndex == Property.Lang)
        {
            int currentLang = _currentScope.Properties[(int)Property.Lang];

            int scopeFontNum = _currentScope.Properties[(int)Property.FontNum];
            if (scopeFontNum == -1) scopeFontNum = _header.DefaultFontNum;

            _fontEntries.TryGetValue(scopeFontNum, out FontEntry? fontEntry);

            int currentCodePage = fontEntry?.CodePage ?? _header.CodePage;

            if (currentLang > -1 && currentLang != 1024 && val != 1024)
            {
                if (val is > -1 and <= MaxLangNumIndex)
                {
                    int langCodePage = LangToCodePage[val];
                    if (langCodePage == -1)
                    {
                        _langItems ??= new List<LangItem>();
                        _langItems.Add(new LangItem((int)CurrentPos, currentCodePage));
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
                        _langItems.Add(new LangItem((int)CurrentPos, langCodePage));
                    }
                }

                // 1024 = "undefined language", ignore it
                if (val == 1024) return Error.OK;
            }
        }

        _currentScope.Properties[(int)propertyTableIndex] = val;

        return Error.OK;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Error ChangeDestination(DestinationType destinationType)
    {
        switch (destinationType)
        {
            // CanBeDestOrNotDest is only relevant for plaintext extraction. As we're only parsing color
            // tables, we can just skip groups so marked.
            // @vNext: Update and diff-test this with our new knowledge: we should skip the group only if it was a destination!
            case DestinationType.CanBeDestOrNotDest:
            case DestinationType.Skip:
                _currentScope.RtfDestinationState = RtfDestinationState.Skip;
                return Error.OK;
            default:
                return Error.OK;
        }
    }

    #endregion

    // @MEM(Color table parser): We could still reduce allocations in here a bit more (but by making the code even more terrible)
    private Error ParseAndBuildColorTable()
    {
        var _colorTableSB = new StringBuilder(4096);

        Error ClearReturnFields(Error error)
        {
            _colorTable = null;
            _colorTableStartIndex = 0;
            _colorTableEndIndex = 0;
            return error;
        }

        ClearReturnFields(Error.OK);

        _colorTableStartIndex = (int)CurrentPos;

        while (true)
        {
            if (!GetNextChar(out char ch)) return ClearReturnFields(Error.EndOfFile);
            if (ch == '}')
            {
                UnGetChar('}');
                _colorTableEndIndex = (int)CurrentPos;
                break;
            }
            _colorTableSB.Append(ch);
        }

        string[] entries = _colorTableSB.ToString().Split(CA_Semicolon);

        int realEntryCount = entries.Length;
        if (entries.Length == 0)
        {
            return ClearReturnFields(Error.OK);
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

        return Error.OK;
    }
}
