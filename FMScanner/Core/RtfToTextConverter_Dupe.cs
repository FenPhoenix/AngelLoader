using System.Runtime.CompilerServices;
using AL_Common;
using static AL_Common.Common;
using static AL_Common.RTFParserCommon;

namespace FMScanner;

// @RTF: Large amount of code duplication for performance. See how much we can dedupe without losing perf.

public sealed partial class RtfToTextConverter
{
    private readonly Context _ctx = new();

    #region Resettables

    private ArrayWithLength<byte> _rtfBytes = ArrayWithLength<byte>.Empty();

    private bool _skipDestinationIfUnknown;

    // For whatever reason it's faster to have this
    private int _groupCount;

    #endregion

    private void ResetBase()
    {
        _ctx.Reset();

        _groupCount = 0;
        _skipDestinationIfUnknown = false;
    }

    #region Stream

    private int CurrentPos;

    /// <summary>
    /// Returns false if the end of the stream has been reached.
    /// </summary>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool GetNextChar(out char ch)
    {
        if (CurrentPos == _rtfBytes.Length)
        {
            ch = '\0';
            return false;
        }

        ch = (char)_rtfBytes[CurrentPos++];

        return true;
    }

    #endregion

    private RtfError ParseKeyword()
    {
        bool hasParam = false;
        int negateParam = 0;
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
            negateParam = 1;
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

            param = BranchlessConditionalNegate(param, negateParam);
        }

        /* From the spec:
         "As with all RTF keywords, a keyword-terminating space may be present (before the ANSI characters)
         that is not counted in the characters to skip."
         This implements the spec for regular control words and \uN alike. Nothing extra needed for removing
         the space from the skip-chars to count.
        */
        // Current position will be > 0 at this point, so a decrement is always safe
        CurrentPos += MinusOneIfNotSpace_8Bits(ch);

        return DispatchKeyword(param, hasParam);
    }

    private RtfError HandlePict()
    {
        int pictScopeLevel = _ctx.ScopeStack.Count;

        while (CurrentPos < _rtfBytes.Length)
        {
            char ch = (char)_rtfBytes[CurrentPos++];

            switch (ch)
            {
                // Push/pop scopes inline to avoid having one branch to check the actual error condition and then
                // a second branch to check the return error code from the push/pop method.
                case '{':
                    if (_ctx.ScopeStack.Count >= ScopeStack.MaxScopes) return RtfError.StackOverflow;
                    _ctx.ScopeStack.DeepCopyToNext();
                    _groupCount++;
                    break;
                case '}':
                    if (_ctx.ScopeStack.Count == 0) return RtfError.StackUnderflow;
                    --_ctx.ScopeStack.Count;
                    _groupCount--;
                    if (_groupCount < pictScopeLevel)
                    {
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
                    if (_groupCount == pictScopeLevel)
                    {
                        while (CurrentPos < _rtfBytes.Length)
                        {
                            /*
                            Since plaintext hex is written as 2 chars per byte (eg. FF), we can skip two chars
                            at a time and still be correct.
                            We get a HUGE speedup from this.
                            But plaintext hex can be broken up with linebreaks, so we need to check for those too
                            unfortunately. This second loop that ONLY checks what needs checking for a hex run
                            speeds us up even more.
                            */
                            if (ch is '\r' or '\n')
                            {
                                CurrentPos++;
                            }
                            else if (ch != '}')
                            {
                                CurrentPos += 2;
                            }
                            else
                            {
                                break;
                            }
                            ch = (char)_rtfBytes[CurrentPos];
                        }
                    }
                    break;
            }
        }

        return RtfError.OK;
    }
}
