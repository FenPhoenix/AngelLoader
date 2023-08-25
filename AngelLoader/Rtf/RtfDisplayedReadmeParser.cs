using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Text;
using AL_Common;
using JetBrains.Annotations;
using static AL_Common.Common;
using static AL_Common.RTFParserBase;

namespace AngelLoader;

// @DarkModeNote(RtfColorTableParser):
// We use a full parser here because rather than simply replacing all byte sequences with another, here we
// need to parse and return the first and ONLY the first {\colortbl} group. In theory that could be in a
// comment or invalidly in the middle of another group or something. I mean it won't, let's be honest, but
// the color table is important enough to take the perf hit and the small amount of code duplication.

public sealed partial class RtfDisplayedReadmeParser
{
    #region Private fields

    private byte[] _rtfBytes = Array.Empty<byte>();

    #endregion

    #region Stream

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private byte StreamReadByte() => _rtfBytes[(int)CurrentPos];

    #endregion

    #region Resettables

    private List<Color>? _colorTable;
    private bool _foundColorTable;
    private bool _getColorTable;
    private bool _getLangs;

    internal sealed class LangItem
    {
        internal int Index;
        internal readonly int CodePage;
        internal int DigitsCount;

        internal LangItem(int index, int codePage)
        {
            Index = index;
            CodePage = codePage;
        }
    }

    private List<LangItem>? _langItems;

    #endregion

    #region Public API

    [PublicAPI]
    internal (bool Success, List<Color>? ColorTable, List<LangItem>? LangItems)
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
            Reset(Array.Empty<byte>());
        }
    }

    #endregion

    private void Reset(byte[] rtfBytes)
    {
        ResetBase();
        // Don't carry around the font entry pool for the entire app lifetime
        _ctx._fontEntries.ClearFull(0);

        #region Fixed-size fields

        _foundColorTable = false;
        _getColorTable = false;
        _getLangs = false;

        #endregion

        _colorTable = null;
        _langItems = null;

        _rtfBytes = rtfBytes;
        ResetStreamBase(rtfBytes.Length);
    }

    private RtfError ParseRtf()
    {
        while (CurrentPos < Length)
        {
            if (!_getLangs && _getColorTable && _foundColorTable) return RtfError.OK;

            char ch = GetNextCharFast();

            if (_groupCount < 0) return RtfError.StackUnderflow;

            if (_ctx._currentScope.RtfInternalState == RtfInternalState.Binary)
            {
                if (--_binaryCharsLeftToSkip <= 0)
                {
                    _ctx._currentScope.RtfInternalState = RtfInternalState.Normal;
                    _binaryCharsLeftToSkip = 0;
                }
                continue;
            }

            RtfError ec;
            switch (ch)
            {
                case '{':
                    if ((ec = _ctx._scopeStack.Push(_ctx._currentScope, ref _groupCount)) != RtfError.OK) return ec;
                    break;
                case '}':
                    if ((ec = _ctx._scopeStack.Pop(_ctx._currentScope, ref _groupCount)) != RtfError.OK) return ec;
                    break;
                case '\\':
                    if ((ec = ParseKeyword()) != RtfError.OK) return ec;
                    break;
                case '\r':
                case '\n':
                    break;
            }
        }

        return _groupCount < 0 ? RtfError.StackUnderflow : _groupCount > 0 ? RtfError.UnmatchedBrace : RtfError.OK;
    }

    #region Act on keywords

    private RtfError DispatchKeyword(int param, bool hasParam)
    {
        if (!Symbols.TryGetValue(_ctx._keyword, out Symbol? symbol))
        {
            // If this is a new destination
            if (_skipDestinationIfUnknown)
            {
                _ctx._currentScope.RtfDestinationState = RtfDestinationState.Skip;
            }
            _skipDestinationIfUnknown = false;
            return RtfError.OK;
        }

        _skipDestinationIfUnknown = false;
        switch (symbol.KeywordType)
        {
            case KeywordType.Property:
                if (symbol.UseDefaultParam || !hasParam) param = symbol.DefaultParam;
                return _getLangs && _ctx._currentScope.RtfDestinationState == RtfDestinationState.Normal
                    ? ChangeProperty((Property)symbol.Index, param)
                    : RtfError.OK;
            case KeywordType.Destination:
                return _ctx._currentScope.RtfDestinationState == RtfDestinationState.Normal
                    ? ChangeDestination((DestinationType)symbol.Index)
                    : RtfError.OK;
            case KeywordType.Special:
                var specialType = (SpecialType)symbol.Index;
                return _ctx._currentScope.RtfDestinationState == RtfDestinationState.Normal ||
                       specialType == SpecialType.Bin
                    ? DispatchSpecialKeyword(specialType, param)
                    : RtfError.OK;
            default:
                //return Error.InvalidSymbolTableEntry;
                return RtfError.OK;
        }
    }

    private RtfError DispatchSpecialKeyword(SpecialType specialType, int param)
    {
        switch (specialType)
        {
            case SpecialType.Bin:
                if (param > 0)
                {
                    _ctx._currentScope.RtfInternalState = RtfInternalState.Binary;
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
                    return RtfError.OK;
                }
            default:
                HandleSpecialTypeFont(specialType, param);
                return RtfError.OK;
        }

        return RtfError.OK;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private RtfError ChangeProperty(Property propertyTableIndex, int val)
    {
        if (propertyTableIndex == Property.FontNum)
        {
            if (_ctx._currentScope.InFontTable)
            {
                _ctx._fontEntries.Add(val);
                return RtfError.OK;
            }

            // \fN supersedes \langN
            _ctx._currentScope.Properties[(int)Property.Lang] = -1;
        }
        else if (propertyTableIndex == Property.Lang)
        {
            int currentLang = _ctx._currentScope.Properties[(int)Property.Lang];

            int scopeFontNum = _ctx._currentScope.Properties[(int)Property.FontNum];
            if (scopeFontNum == -1) scopeFontNum = _ctx._header.DefaultFontNum;

            _ctx._fontEntries.TryGetValue(scopeFontNum, out FontEntry? fontEntry);

            int currentCodePage = fontEntry?.CodePage >= 0 ? fontEntry.CodePage : _ctx._header.CodePage;

            if (currentLang > -1 && currentLang != _undefinedLanguage && val != _undefinedLanguage)
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

                if (val == _undefinedLanguage) return RtfError.OK;
            }
        }

        _ctx._currentScope.Properties[(int)propertyTableIndex] = val;

        return RtfError.OK;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private RtfError ChangeDestination(DestinationType destinationType)
    {
        switch (destinationType)
        {
            // CanBeDestOrNotDest is only relevant for plaintext extraction. As we're only parsing color
            // tables, we can just skip groups so marked.
            // TODO: Update and diff-test this with our new knowledge: we should skip the group only if it was a destination!
            case DestinationType.CanBeDestOrNotDest:
            case DestinationType.Skip:
                _ctx._currentScope.RtfDestinationState = RtfDestinationState.Skip;
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
                UnGetChar('}');
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
