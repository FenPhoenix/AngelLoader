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

public sealed class RtfDisplayedReadmeParser //: AL_Common.RTFParserBase
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
        _fontEntries.ClearFull(0);

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

            if (_currentScope.RtfInternalState == RtfInternalState.Binary)
            {
                if (--_binaryCharsLeftToSkip <= 0)
                {
                    _currentScope.RtfInternalState = RtfInternalState.Normal;
                    _binaryCharsLeftToSkip = 0;
                }
                continue;
            }

            RtfError ec;
            switch (ch)
            {
                case '{':
                    if ((ec = _scopeStack.Push(_currentScope, ref _groupCount)) != RtfError.OK) return ec;
                    break;
                case '}':
                    if ((ec = _scopeStack.Pop(_currentScope, ref _groupCount)) != RtfError.OK) return ec;
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
        if (!Symbols.TryGetValue(_keyword, out Symbol? symbol))
        {
            // If this is a new destination
            if (_skipDestinationIfUnknown)
            {
                _currentScope.RtfDestinationState = RtfDestinationState.Skip;
            }
            _skipDestinationIfUnknown = false;
            return RtfError.OK;
        }

        _skipDestinationIfUnknown = false;
        switch (symbol.KeywordType)
        {
            case KeywordType.Property:
                if (symbol.UseDefaultParam || !hasParam) param = symbol.DefaultParam;
                return _getLangs && _currentScope.RtfDestinationState == RtfDestinationState.Normal
                    ? ChangeProperty((Property)symbol.Index, param)
                    : RtfError.OK;
            case KeywordType.Destination:
                return _currentScope.RtfDestinationState == RtfDestinationState.Normal
                    ? ChangeDestination((DestinationType)symbol.Index)
                    : RtfError.OK;
            case KeywordType.Special:
                var specialType = (SpecialType)symbol.Index;
                return _currentScope.RtfDestinationState == RtfDestinationState.Normal ||
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
            if (_currentScope.InFontTable)
            {
                _fontEntries.Add(val);
                return RtfError.OK;
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

            int currentCodePage = fontEntry?.CodePage >= 0 ? fontEntry.CodePage : _header.CodePage;

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

        _currentScope.Properties[(int)propertyTableIndex] = val;

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
                _currentScope.RtfDestinationState = RtfDestinationState.Skip;
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

    #region Base

    #region Stream

    // We can't actually get the length of some kinds of streams (zip entry streams), so we take the
    // length as a param and store it.
    /// <summary>
    /// Do not modify!
    /// </summary>
    private long Length;

    /// <summary>
    /// Do not modify!
    /// </summary>
    private long CurrentPos;

    /*
    We use this as a "seek-back" buffer for when we want to move back in the stream. We put chars back
    "into the stream", but they actually go in here and then when we go to read, we read from this first
    and so on until it's empty, then go back to reading from the main stream again. In this way, we
    support a rudimentary form of peek-and-rewind without ever actually seeking backwards in the stream.
    This is required to support zip entry streams which are unseekable. If we required a seekable stream,
    we would have to copy the entire, potentially very large, zip entry stream to memory first and then
    read it, which is possibly unnecessarily memory-hungry.

    2020-08-15:
    We now have a buffered stream so in theory we could check if we're > 0 in the buffer and just actually
    rewind if we are, but our seek-back buffer is fast enough already so we're just keeping that for now.
    */
    private readonly UnGetStack _unGetBuffer = new();

    /// <summary>
    /// Puts a char back into the stream and decrements the read position. Actually doesn't really do that
    /// but uses an internal seek-back buffer to allow it work with forward-only streams. But don't worry
    /// about that. Just use it as normal.
    /// </summary>
    /// <param name="c"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void UnGetChar(char c)
    {
        if (CurrentPos < 0) return;

        _unGetBuffer.Push(c);
        if (CurrentPos > 0) CurrentPos--;
    }

    /// <summary>
    /// Returns false if the end of the stream has been reached.
    /// </summary>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool GetNextChar(out char ch)
    {
        if (CurrentPos == Length)
        {
            ch = '\0';
            return false;
        }

        // For some reason leaving this as a full if makes us fast but changing it to a ternary makes us slow?!
#pragma warning disable IDE0045 // Convert to conditional expression
        if (_unGetBuffer.Count > 0)
        {
            ch = _unGetBuffer.Pop();
        }
        else
        {
            ch = (char)StreamReadByte();
        }
#pragma warning restore IDE0045 // Convert to conditional expression
        CurrentPos++;

        return true;
    }

    /// <summary>
    /// For use in loops that already check the stream position against the end as a loop condition
    /// </summary>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private char GetNextCharFast()
    {
        char ch;
        // Ditto above
#pragma warning disable IDE0045 // Convert to conditional expression
        if (_unGetBuffer.Count > 0)
        {
            ch = _unGetBuffer.Pop();
        }
        else
        {
            ch = (char)StreamReadByte();
        }
#pragma warning restore IDE0045 // Convert to conditional expression
        CurrentPos++;

        return ch;
    }

    private void ResetStreamBase(long streamLength)
    {
        Length = streamLength;

        CurrentPos = 0;

        _unGetBuffer.Clear();
    }

    #endregion

    #region Resettables

    private readonly ListFast<char> _keyword = new(_keywordMaxLen);

    private int _binaryCharsLeftToSkip;

    private bool _skipDestinationIfUnknown;

    // Highest measured was 10
    private readonly ScopeStack _scopeStack = new();

    private readonly Scope _currentScope = new();

    // We really do need this tracking var, as the scope stack could be empty but we're still valid (I think)
    private int _groupCount;

    /*
    FMs can have 100+ of these...
    Highest measured was 131
    Fonts can specify themselves as whatever number they want, so we can't just count by index
    eg. you could have \f1 \f2 \f3 but you could also have \f1 \f14 \f45
    */
    private readonly FontDictionary _fontEntries = new(150);

    private readonly Header _header = new();

    #endregion

    private void ResetBase()
    {
        #region Fixed-size fields

        // Specific capacity and won't grow; no need to deallocate
        _keyword.ClearFast();

        _groupCount = 0;
        _binaryCharsLeftToSkip = 0;
        _skipDestinationIfUnknown = false;

        _currentScope.Reset();

        #endregion

        _scopeStack.ClearFast();

        _header.Reset();

        _fontEntries.Clear();
    }

    private RtfError ParseKeyword()
    {
        bool hasParam = false;
        bool negateParam = false;
        int param = 0;

        if (!GetNextChar(out char ch)) return RtfError.EndOfFile;

        _keyword.ClearFast();

        if (!ch.IsAsciiAlpha())
        {
            /* From the spec:
             "A control symbol consists of a backslash followed by a single, non-alphabetical character.
             For example, \~ (backslash tilde) represents a non-breaking space. Control symbols do not have
             delimiters, i.e., a space following a control symbol is treated as text, not a delimiter."

             So just go straight to dispatching without looking for a param and without eating the space.
            */
            _keyword.AddFast(ch);
            return DispatchKeyword(0, false);
        }

        int i;
        bool eof = false;
        for (i = 0; i < _keywordMaxLen && ch.IsAsciiAlpha(); i++, eof = !GetNextChar(out ch))
        {
            if (eof) return RtfError.EndOfFile;
            _keyword.AddFast(ch);
        }
        if (i > _keywordMaxLen) return RtfError.KeywordTooLong;

        if (ch == '-')
        {
            negateParam = true;
            if (!GetNextChar(out ch)) return RtfError.EndOfFile;
        }

        if (ch.IsAsciiNumeric())
        {
            hasParam = true;

            // Parse param in real-time to avoid doing a second loop over
            for (i = 0; i < _paramMaxLen && ch.IsAsciiNumeric(); i++, eof = !GetNextChar(out ch))
            {
                if (eof) return RtfError.EndOfFile;
                param += ch - '0';
                param *= 10;
            }
            // Undo the last multiply just one time to avoid checking if we should do it every time through
            // the loop
            param /= 10;
            if (i > _paramMaxLen) return RtfError.ParameterTooLong;

            if (negateParam) param = -param;
        }

        /* From the spec:
         "As with all RTF keywords, a keyword-terminating space may be present (before the ANSI characters)
         that is not counted in the characters to skip."
         This implements the spec for regular control words and \uN alike. Nothing extra needed for removing
         the space from the skip-chars to count.
        */
        if (ch != ' ') UnGetChar(ch);

        return DispatchKeyword(param, hasParam);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private RtfError HandleSpecialTypeFont(SpecialType specialType, int param)
    {
        switch (specialType)
        {
            case SpecialType.HeaderCodePage:
                _header.CodePage = param >= 0 ? param : 1252;
                break;
            case SpecialType.FontTable:
                _currentScope.InFontTable = true;
                break;
            case SpecialType.DefaultFont:
                if (!_header.DefaultFontSet)
                {
                    _header.DefaultFontNum = param;
                    _header.DefaultFontSet = true;
                }
                break;
            case SpecialType.Charset:
                // Reject negative codepage values as invalid and just use the header default in that case
                // (which is guaranteed not to be negative)
                if (_fontEntries.Count > 0 && _currentScope.InFontTable)
                {
                    if (param is >= 0 and < _charSetToCodePageLength)
                    {
                        int codePage = _charSetToCodePage[param];
                        _fontEntries.Top.CodePage = codePage >= 0 ? codePage : _header.CodePage;
                    }
                    else
                    {
                        _fontEntries.Top.CodePage = _header.CodePage;
                    }
                }
                break;
            case SpecialType.CodePage:
                if (_fontEntries.Count > 0 && _currentScope.InFontTable)
                {
                    _fontEntries.Top.CodePage = param >= 0 ? param : _header.CodePage;
                }
                break;
            default:
                return RtfError.InvalidSymbolTableEntry;
        }

        return RtfError.OK;
    }

    #endregion
}
