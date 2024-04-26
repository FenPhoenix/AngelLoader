#nullable enable // Required for generated files
#define FenGen_RtfDuplicateDest

using System;
using static AL_Common.Common;
using static AL_Common.FenGenAttributes;
using static AL_Common.RTFParserCommon;

namespace AL_Common;

[FenGenRtfDuplicateDestClass]
public sealed partial class RtfDisplayedReadmeParser
{
    /*
    The functions in here are very difficult to de-duplicate because of their call chains. Any attempt results
    in a performance loss from extra branching at the call-site and/or passing too many params in and out or
    something. So we auto-duplicate this code into every parser to ensure maximum possible performance.

    This really is a last-resort kind of thing. It may be possible to redo the whole parser design to avoid
    duplication and still keep perf, but for now at least this works fine.
    */

    private readonly Context _ctx = new();

    #region Resettables

    private ArrayWithLength<byte> _rtfBytes = ArrayWithLength<byte>.Empty();

    private bool _skipDestinationIfUnknown;

    // For whatever reason it's faster to have this
    private int _groupCount;

    private int CurrentPos;

    private bool _inHandleSkippableHexData;

    #endregion

    private void ResetBase(in ArrayWithLength<byte> rtfBytes)
    {
        _ctx.Reset();

        _groupCount = 0;
        _skipDestinationIfUnknown = false;

        _rtfBytes = rtfBytes;
        CurrentPos = 0;

        _inHandleSkippableHexData = false;
    }

    private RtfError ParseKeyword()
    {
        bool hasParam = false;
        int param = 0;
        Symbol? symbol;

        char ch = (char)_rtfBytes[CurrentPos++];

        char[] keyword = _ctx.Keyword;

        if (!char.IsAsciiLetter(ch))
        {
            /* From the spec:
             "A control symbol consists of a backslash followed by a single, non-alphabetical character.
             For example, \~ (backslash tilde) represents a non-breaking space. Control symbols do not have
             delimiters, i.e., a space following a control symbol is treated as text, not a delimiter."

             So just go straight to dispatching without looking for a param and without eating the space.
            */

            // Fast path for destination marker - claws us back a small amount of perf
            if (ch == '*')
            {
                _skipDestinationIfUnknown = true;
                return RtfError.OK;
            }

            symbol = Symbols.LookUpControlSymbol(ch);
        }
        else
        {
            int keywordCount;
            for (keywordCount = 0;
                 keywordCount < KeywordMaxLen && char.IsAsciiLetter(ch);
                 keywordCount++, ch = (char)_rtfBytes[CurrentPos++])
            {
                keyword[keywordCount] = ch;
            }

            int negateParam = 0;
            if (ch == '-')
            {
                negateParam = 1;
                ch = (char)_rtfBytes[CurrentPos++];
            }
            if (char.IsAsciiDigit(ch))
            {
                hasParam = true;

                // Parse param in real-time to avoid doing a second loop over
                for (int i = 0;
                     i < ParamMaxLen && char.IsAsciiDigit(ch);
                     i++, ch = (char)_rtfBytes[CurrentPos++])
                {
                    param = (param * 10) + (ch - '0');
                }
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

            symbol = Symbols.LookUpControlWord(keyword, keywordCount);
        }

        if (symbol == null)
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

        return DispatchKeyword(symbol, param, hasParam);
    }

    private RtfError HandleSkippableHexData()
    {
        // Prevent stack overflow from maliciously-crafted rtf files - we should never recurse back into here in
        // a spec-conforming file.
        if (_inHandleSkippableHexData) return RtfError.AbortedForSafety;
        _inHandleSkippableHexData = true;

        int startGroupLevel = _ctx.GroupStack.Count;

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
                    if (_groupCount < startGroupLevel)
                    {
                        _inHandleSkippableHexData = false;
                        return RtfError.OK;
                    }
                    break;
                case '\\':
                    // This implicitly also handles the case where the data is \binN instead of hex
                    RtfError ec = ParseKeyword();
                    if (ec != RtfError.OK) return ec;
                    break;
                case '\r':
                case '\n':
                    break;
                default:
                    if (_groupCount == startGroupLevel)
                    {
                        // We were doing a clever skip-two-char-at-a-time for the hex data, but turns out that
                        // Array.IndexOf() is the fastest thing by light-years once again. Hey, no complaints here.
                        int closingBraceIndex = Array.IndexOf(_rtfBytes.Array, (byte)'}', CurrentPos, _rtfBytes.Length - CurrentPos);
                        CurrentPos = closingBraceIndex == -1 ? _rtfBytes.Length : closingBraceIndex;
                    }
                    break;
            }
        }

        _inHandleSkippableHexData = false;
        return RtfError.OK;
    }
}
