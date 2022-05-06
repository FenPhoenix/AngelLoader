// @MEM: Change the RTF converter stuff from inheritance-based to some other thing
// Because calling base methods is quite a _lot_ slower than not, it turns out.

#if true
using System.Runtime.CompilerServices;
using AL_Common;
using static AL_Common.RTFParserBase;

namespace FMScanner
{
    public sealed partial class RtfToTextConverter
    {
        #region Resettables

        private int _binaryCharsLeftToSkip;
        private int _unicodeCharsLeftToSkip;

        private bool _skipDestinationIfUnknown;

        // We really do need this tracking var, as the scope stack could be empty but we're still valid (I think)
        private int _groupCount;

        #endregion

        private void ResetBase()
        {
            #region Fixed-size fields

            // Specific capacity and won't grow; no need to deallocate
            _keyword.ClearFast();

            // Fixed-size value types
            _groupCount = 0;
            _binaryCharsLeftToSkip = 0;
            _unicodeCharsLeftToSkip = 0;
            _skipDestinationIfUnknown = false;

            // Types that contain only fixed-size value types
            _currentScope.Reset();

            #endregion

            _scopeStack.ClearFast();
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

        //private const int _bufferLen = Common.ByteSize.MB * 100;
        private const int _bufferLen = 81920;

        private readonly byte[] _buffer = new byte[_bufferLen];
        // Start it ready to roll over to 0 so we don't need extra logic for the first get
        private int _bufferPos = _bufferLen - 1;

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
        private readonly StackFast<char> _unGetBuffer = new StackFast<char>(100);
        private bool _unGetBufferEmpty = true;

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
            _unGetBufferEmpty = false;
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

            if (!_unGetBufferEmpty)
            {
                ch = _unGetBuffer.Pop();
                _unGetBufferEmpty = _unGetBuffer.Count == 0;
            }
            else
            {
                ch = (char)StreamReadByte();
            }
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
            if (!_unGetBufferEmpty)
            {
                ch = _unGetBuffer.Pop();
                _unGetBufferEmpty = _unGetBuffer.Count == 0;
            }
            else
            {
                ch = (char)StreamReadByte();
            }
            CurrentPos++;

            return ch;
        }

        private void ResetStreamBase(long streamLength)
        {
            Length = streamLength;

            CurrentPos = 0;

            // Don't clear the buffer; we don't need to and it wastes time
            _bufferPos = _bufferLen - 1;

            _unGetBuffer.ClearFast();
            _unGetBufferEmpty = true;
        }

        #endregion

        #region Scope push/pop

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Error PushScope()
        {
            // Don't wait for out-of-memory; just put a sane cap on it.
            if (_scopeStack.Count >= MaxScopes) return Error.StackOverflow;

            _scopeStack.Push(_currentScope);

            _currentScope.RtfInternalState = RtfInternalState.Normal;

            _groupCount++;

            return Error.OK;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Error PopScope()
        {
            if (_scopeStack.Count == 0) return Error.StackUnderflow;

            _scopeStack.Pop().DeepCopyTo(_currentScope);
            _groupCount--;

            return Error.OK;
        }

        #endregion

        private Error ParseKeyword()
        {
            bool hasParam = false;
            bool negateParam = false;
            int param = 0;

            if (!GetNextChar(out char ch)) return Error.EndOfFile;

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
            for (i = 0; i < KeywordMaxLen && ch.IsAsciiAlpha(); i++, eof = !GetNextChar(out ch))
            {
                if (eof) return Error.EndOfFile;
                _keyword.AddFast(ch);
            }
            if (i > KeywordMaxLen) return Error.KeywordTooLong;

            if (ch == '-')
            {
                negateParam = true;
                if (!GetNextChar(out ch)) return Error.EndOfFile;
            }

            if (ch.IsAsciiNumeric())
            {
                hasParam = true;

                // Parse param in real-time to avoid doing a second loop over
                for (i = 0; i < ParamMaxLen && ch.IsAsciiNumeric(); i++, eof = !GetNextChar(out ch))
                {
                    if (eof) return Error.EndOfFile;
                    param += ch - '0';
                    param *= 10;
                }
                // Undo the last multiply just one time to avoid checking if we should do it every time through
                // the loop
                param /= 10;
                if (i > ParamMaxLen) return Error.ParameterTooLong;

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
}
#endif