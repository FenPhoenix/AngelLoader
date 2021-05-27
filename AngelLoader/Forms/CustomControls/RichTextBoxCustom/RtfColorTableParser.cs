using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using AL_Common;
using JetBrains.Annotations;
using static AL_Common.RTF;

namespace AngelLoader.Forms.CustomControls
{
    // @DarkModeNote(RtfColorTableParser):
    // We use a full parser here because rather than simply replacing all byte sequences with another, here we
    // need to parse and return the first and ONLY the first {\colortbl} group. In theory that could be in a
    // comment or invalidly in the middle of another group or something. I mean it won't, let's be honest, but
    // the color table is important enough to take the perf hit and the code duplication.

    public sealed class RtfColorTableParser : AL_Common.RTF
    {
        #region Classes

        // Keeping these as entirely separate classes (with attendant code duplication) for now, because of the
        // stream being a Stream in one and List<byte> (for avoidance of conversion to an array and then to a
        // MemoryStream) in the other.
        private sealed class RTFStream
        {
            #region Private fields

            private List<byte> _stream = null!;

            // We can't actually get the length of some kinds of streams (zip entry streams), so we take the
            // length as a param and store it.
            /// <summary>
            /// Do not modify!
            /// </summary>
            internal long Length;

            /// <summary>
            /// Do not modify!
            /// </summary>
            internal int CurrentPos;

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
            private readonly Stack<char> _unGetBuffer = new Stack<char>(5);
            private bool _unGetBufferEmpty = true;

            #endregion

            // PERF: Everything in here is inlined. This gives a shockingly massive speedup.

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal void Reset(List<byte> stream)
            {
                _stream = stream;
                Length = stream.Count;

                CurrentPos = 0;

                _unGetBuffer.Clear();
                _unGetBufferEmpty = true;
            }

            /// <summary>
            /// Puts a char back into the stream and decrements the read position. Actually doesn't really do that
            /// but uses an internal seek-back buffer to allow it work with forward-only streams. But don't worry
            /// about that. Just use it as normal.
            /// </summary>
            /// <param name="c"></param>
            /// <returns></returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal void UnGetChar(char c)
            {
                if (CurrentPos < 0) return;

                _unGetBuffer.Push(c);
                _unGetBufferEmpty = false;
                if (CurrentPos > 0) CurrentPos--;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private byte StreamReadByte() => _stream[CurrentPos];

            /// <summary>
            /// Returns false if the end of the stream has been reached.
            /// </summary>
            /// <returns></returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal bool GetNextChar(out char ch)
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
            internal char GetNextCharFast()
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
        }

        #endregion

        #region Resettables

        private readonly RTFStream _rtfStream = new RTFStream();

        private readonly StringBuilder _colorTableSB = new StringBuilder(4096);

        private readonly List<Color> _colorTable = new List<Color>(32);
        private int _colorTableStartIndex;
        private int _colorTableEndIndex;

        #endregion

        #region Public API

        [PublicAPI]
        public (bool Success, List<Color> ColorTable, int ColorTableStartIndex, int ColorTableEndIndex)
            GetColorTable(List<byte> stream)
        {
            Reset(stream);

            try
            {
                Error error = ParseRtf();
                return error == Error.OK
                    ? (true, ColorTable: _colorTable, ColorTableStartIndex: _colorTableStartIndex,
                        ColorTableEndIndex: _colorTableEndIndex)
                    : (false, ColorTable: _colorTable, ColorTableStartIndex: _colorTableStartIndex,
                        ColorTableEndIndex: _colorTableEndIndex);
            }
            catch
            {
                return (false, _colorTable, _colorTableStartIndex, _colorTableEndIndex);
            }
        }

        #endregion

        private void Reset(List<byte> stream)
        {
            base.ResetBase();

            #region Fixed-size fields

            _colorTableStartIndex = 0;
            _colorTableEndIndex = 0;

            #endregion

            _colorTableSB.Clear();
            _colorTable.Clear();

            // This one has the seek-back buffer (a Stack<char>) which is technically eligible for deallocation,
            // even though in practice I think it's guaranteed never to have more than like 5 chars in it maybe?
            // Again, it's a stack so we can't check its capacity. But... meh. See above.
            // Not way into the idea of making another custom type where the only difference is we can access a
            // frigging internal variable, gonna be honest.
            _rtfStream.Reset(stream);
        }

        private bool _exiting;

        protected override bool GetNextChar(out char ch) => _rtfStream.GetNextChar(out ch);
        protected override void UnGetChar(char ch) => _rtfStream.UnGetChar(ch);

        private Error ParseRtf()
        {
            while (_rtfStream.CurrentPos < _rtfStream.Length)
            {
                if (_exiting) return Error.OK;

                char ch = _rtfStream.GetNextCharFast();

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
                        // Per spec, if we encounter a group delimiter during Unicode skipping, we end skipping early
                        if (_unicodeCharsLeftToSkip > 0) _unicodeCharsLeftToSkip = 0;
                        if ((ec = PushScope()) != Error.OK) return ec;
                        break;
                    case '}':
                        // ditto the above
                        if (_unicodeCharsLeftToSkip > 0) _unicodeCharsLeftToSkip = 0;
                        if ((ec = PopScope()) != Error.OK) return ec;
                        break;
                    case '\\':
                        // We have to check what the keyword is before deciding whether to parse the Unicode.
                        // If it's another \uN keyword, then obviously we don't want to parse yet because the
                        // run isn't finished.
                        if ((ec = ParseKeyword()) != Error.OK) return ec;
                        break;
                    case '\r':
                    case '\n':
                        // These DON'T count as Unicode barriers, so don't parse the Unicode here!
                        break;
                    default:
                        if (_currentScope.RtfInternalState == RtfInternalState.Normal &&
                            _currentScope.RtfDestinationState == RtfDestinationState.Normal &&
                            --_unicodeCharsLeftToSkip <= 0)
                        {
                            _unicodeCharsLeftToSkip = 0;
                        }
                        break;
                }
            }

            return _groupCount < 0 ? Error.StackUnderflow : _groupCount > 0 ? Error.UnmatchedBrace : Error.OK;
        }

        #region Act on keywords

        protected override Error DispatchKeyword(int param, bool hasParam)
        {
            if (!_symbolTable.TryGetValue(_keyword, out Symbol? symbol))
            {
                // If this is a new destination
                if (_skipDestinationIfUnknown)
                {
                    _currentScope.RtfDestinationState = RtfDestinationState.Skip;
                }
                _skipDestinationIfUnknown = false;
                return Error.OK;
            }

            // From the spec:
            // "While this is not likely to occur (or recommended), a \binN keyword, its argument, and the binary
            // data that follows are considered one character for skipping purposes."
            if (symbol.Index == (int)SpecialType.Bin && _unicodeCharsLeftToSkip > 0)
            {
                // Rather than literally counting it as one character for skipping purposes, we just increment
                // the chars left to skip count by the specified length of the binary run, which accomplishes
                // the same thing and is the easiest option.
                // Note: It seems like we should have to add 1 for the space after \binN, but it looks like the
                // numbers somehow work out that we don't have to and it's already implicitly counted. Shrug.
                if (param >= 0) _unicodeCharsLeftToSkip += param;
            }

            // From the spec:
            // "Any RTF control word or symbol is considered a single character for the purposes of counting
            // skippable characters."
            // But don't do it if it's a hex char, because we handle it elsewhere in that case.
            if (symbol.Index != (int)SpecialType.HexEncodedChar &&
                _currentScope.RtfInternalState != RtfInternalState.Binary &&
                _unicodeCharsLeftToSkip > 0)
            {
                if (--_unicodeCharsLeftToSkip <= 0) _unicodeCharsLeftToSkip = 0;
                return Error.OK;
            }

            _skipDestinationIfUnknown = false;
            switch (symbol.KeywordType)
            {
                case KeywordType.Property:
                    if (symbol.UseDefaultParam || !hasParam) param = symbol.DefaultParam;
                    return _currentScope.RtfDestinationState == RtfDestinationState.Normal
                        ? ChangeProperty((Property)symbol.Index, param)
                        : Error.OK;
                case KeywordType.Character:
                    if (_currentScope.RtfDestinationState == RtfDestinationState.Normal &&
                        --_unicodeCharsLeftToSkip <= 0)
                    {
                        _unicodeCharsLeftToSkip = 0;
                    }
                    return Error.OK;
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
                case SpecialType.HexEncodedChar:
                    _currentScope.RtfInternalState = RtfInternalState.HexEncodedChar;
                    break;
                case SpecialType.SkipDest:
                    _skipDestinationIfUnknown = true;
                    break;
                case SpecialType.UnicodeChar:
                    _unicodeCharsLeftToSkip = _currentScope.Properties[(int)Property.UnicodeCharSkipCount];
                    break;
                case SpecialType.ColorTable:
                    // Spec is to ignore any further color tables after the first one, which is fortunate for us
                    // because it makes us way faster (we quit as soon as we've parsed the color table, which is
                    // usually very close to the top of the file).
                    _exiting = true;
                    return ParseAndBuildColorTable();
                default:
                    return Error.OK;
            }

            return Error.OK;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Error ChangeProperty(Property propertyTableIndex, int val)
        {
            _currentScope.Properties[(int)propertyTableIndex] = val;

            return Error.OK;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Error ChangeDestination(DestinationType destinationType)
        {
            switch (destinationType)
            {
                case DestinationType.IgnoreButDontSkipGroup:
                    // The group this destination is in may contain text we want to extract, so parse it as normal.
                    // We will still skip over the next nested destination group we find, if any, unless it too is
                    // marked as ignore-but-don't-skip.
                    return Error.OK;
                case DestinationType.Skip:
                    _currentScope.RtfDestinationState = RtfDestinationState.Skip;
                    return Error.OK;
                default:
                    //return Error.InvalidSymbolTableEntry;
                    return Error.OK;
            }
        }

        #endregion

        private Error ParseAndBuildColorTable()
        {
            Error ClearReturnFields(Error error)
            {
                _colorTableSB.Clear();
                _colorTable.Clear();
                _colorTableStartIndex = 0;
                _colorTableEndIndex = 0;
                return error;
            }

            ClearReturnFields(Error.OK);

            _colorTableStartIndex = _rtfStream.CurrentPos;

            while (true)
            {
                if (!_rtfStream.GetNextChar(out char ch)) return ClearReturnFields(Error.EndOfFile);
                if (ch == '}')
                {
                    _rtfStream.UnGetChar('}');
                    _colorTableEndIndex = _rtfStream.CurrentPos;
                    break;
                }
                _colorTableSB.Append(ch);
            }

            string ct = _colorTableSB.ToString();
            List<string> entries = ct.Split(';').ToList();

            if (entries.Count == 0)
            {
                return ClearReturnFields(Error.OK);
            }
            // Remove the last blank entry so we don't count it as the auto/default one by hitting a blank entry
            // in the loop below
            else if (entries.Count > 1 && entries[entries.Count - 1].IsWhiteSpace())
            {
                entries.RemoveAt(entries.Count - 1);
            }

            for (int i = 0; i < entries.Count; i++)
            {
                string entry = entries[i].Trim();

                if (entry.IsEmpty())
                {
                    // 0 alpha will be the flag for "this is the omitted default/auto color"
                    _colorTable.Add(Color.FromArgb(0, 0, 0, 0));
                }
                else
                {
                    // Horrible but functional just to get it going
                    // NOTE: In theory this could throw, but if so it'll be caught by the try-catch wrapping the
                    // entire parse operation, and we'll return false, which is what we want. No need to add a
                    // second try-catch here.
                    Match redMatch = Regex.Match(entry, @"\\red(?<Value>[0123456789]{1,3})");
                    Match greenMatch = Regex.Match(entry, @"\\green(?<Value>[0123456789]{1,3})");
                    Match blueMatch = Regex.Match(entry, @"\\blue(?<Value>[0123456789]{1,3})");

                    if (redMatch.Success &&
                        blueMatch.Success &&
                        greenMatch.Success &&
                        byte.TryParse(redMatch.Groups["Value"].Value, out byte red) &&
                        byte.TryParse(greenMatch.Groups["Value"].Value, out byte green) &&
                        byte.TryParse(blueMatch.Groups["Value"].Value, out byte blue))
                    {
                        _colorTable.Add(Color.FromArgb(red, green, blue));
                    }
                }
            }

            return Error.OK;
        }
    }
}
