using System.Runtime.CompilerServices;
using AL_Common;
using static AL_Common.Common;
using static AL_Common.RTFParserBase;

namespace AngelLoader;

// @RTF: Large amount of code duplication for performance. See how much we can dedupe without losing perf.

public sealed partial class RtfDisplayedReadmeParser
{
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
}
