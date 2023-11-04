#define FenGen_RtfDuplicateDest

using System;
using System.Runtime.CompilerServices;
using static AL_Common.Common;
using static AL_Common.FenGenAttributes;
using static AL_Common.RTFParserCommon;

namespace AngelLoader;

[FenGenRtfDuplicateDestClass]
public sealed partial class RtfDisplayedReadmeParser
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
        int pictGroupLevel = _ctx.GroupStack.Count;

        while (CurrentPos < _rtfBytes.Length)
        {
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
                    if (_groupCount < pictGroupLevel)
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
                    if (_groupCount == pictGroupLevel)
                    {
                        // We were doing a clever skip-two-char-at-a-time for the hex data, but turns out that
                        // Array.IndexOf() is the fastest thing by light-years once again. Hey, no complaints here.
                        int closingBraceIndex = Array.IndexOf(_rtfBytes.Array, (byte)'}', CurrentPos);
                        if (closingBraceIndex == -1) return RtfError.EndOfFile;
                        CurrentPos = closingBraceIndex;
                    }
                    break;
            }
        }

        return RtfError.OK;
    }
}
