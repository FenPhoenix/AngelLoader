using System.Runtime.CompilerServices;
using AL_Common;
using static AL_Common.RTFParserCommon;

namespace AngelLoader;

// @RTF: Large amount of code duplication for performance. See how much we can dedupe without losing perf.

public sealed partial class RtfDisplayedReadmeParser
{
    private readonly Context _ctx = new();

    #region Resettables

    private int _binaryCharsLeftToSkip;

    private bool _skipDestinationIfUnknown;

    // We really do need this tracking var, as the scope stack could be empty but we're still valid (I think)
    private int _groupCount;

    #endregion

    private void ResetBase()
    {
        _ctx.Reset();

        _groupCount = 0;
        _binaryCharsLeftToSkip = 0;
        _skipDestinationIfUnknown = false;
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

        _ctx.UnGetBuffer.Push(c);
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
        if (_ctx.UnGetBuffer.Count > 0)
        {
            ch = _ctx.UnGetBuffer.Pop();
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
        if (_ctx.UnGetBuffer.Count > 0)
        {
            ch = _ctx.UnGetBuffer.Pop();
        }
        else
        {
            ch = (char)StreamReadByte();
        }
#pragma warning restore IDE0045 // Convert to conditional expression
        CurrentPos++;

        return ch;
    }

    #endregion

    private RtfError ParseKeyword()
    {
        bool hasParam = false;
        bool negateParam = false;
        int param = 0;

        if (!GetNextChar(out char ch)) return RtfError.EndOfFile;

        _ctx.Keyword.ClearFast();

        if (!ch.IsAsciiAlpha())
        {
            /* From the spec:
             "A control symbol consists of a backslash followed by a single, non-alphabetical character.
             For example, \~ (backslash tilde) represents a non-breaking space. Control symbols do not have
             delimiters, i.e., a space following a control symbol is treated as text, not a delimiter."

             So just go straight to dispatching without looking for a param and without eating the space.
            */
            _ctx.Keyword.AddFast(ch);
            return DispatchKeyword(0, false);
        }

        int i;
        bool eof = false;
        for (i = 0; i < KeywordMaxLen && ch.IsAsciiAlpha(); i++, eof = !GetNextChar(out ch))
        {
            if (eof) return RtfError.EndOfFile;
            _ctx.Keyword.AddFast(ch);
        }
        if (i > KeywordMaxLen) return RtfError.KeywordTooLong;

        if (ch == '-')
        {
            negateParam = true;
            if (!GetNextChar(out ch)) return RtfError.EndOfFile;
        }

        if (ch.IsAsciiNumeric())
        {
            hasParam = true;

            // Parse param in real-time to avoid doing a second loop over
            for (i = 0; i < ParamMaxLen && ch.IsAsciiNumeric(); i++, eof = !GetNextChar(out ch))
            {
                if (eof) return RtfError.EndOfFile;
                param += ch - '0';
                param *= 10;
            }
            // Undo the last multiply just one time to avoid checking if we should do it every time through
            // the loop
            param /= 10;
            if (i > ParamMaxLen) return RtfError.ParameterTooLong;

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
