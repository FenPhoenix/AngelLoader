using System.Collections.Generic;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Text;
using JetBrains.Annotations;
using static AL_Common.Common;

namespace AngelLoader
{
    // @DarkModeNote(RtfColorTableParser):
    // We use a full parser here because rather than simply replacing all byte sequences with another, here we
    // need to parse and return the first and ONLY the first {\colortbl} group. In theory that could be in a
    // comment or invalidly in the middle of another group or something. I mean it won't, let's be honest, but
    // the color table is important enough to take the perf hit and the small amount of code duplication.

    public sealed class RtfColorTableParser : AL_Common.RTFParserBase
    {
        #region Private fields

        private byte[] _stream = null!;

        #endregion

        #region Stream

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ResetStream(byte[] stream)
        {
            _stream = stream;
            base.ResetStreamBase(stream.Length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override byte StreamReadByte() => _stream[(int)CurrentPos];

        #endregion

        #region Resettables

        private List<Color>? _colorTable;
        private int _colorTableStartIndex;
        private int _colorTableEndIndex;

        #endregion

        #region Public API

        [PublicAPI]
        public (bool Success, List<Color>? ColorTable, int ColorTableStartIndex, int ColorTableEndIndex)
        GetColorTable(byte[] stream)
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

        private void Reset(byte[] stream)
        {
            base.ResetBase();

            #region Fixed-size fields

            _colorTableStartIndex = 0;
            _colorTableEndIndex = 0;

            #endregion

            _colorTable = null;

            // This one has the seek-back buffer (a Stack<char>) which is technically eligible for deallocation,
            // even though in practice I think it's guaranteed never to have more than like 5 chars in it maybe?
            // Again, it's a stack so we can't check its capacity. But... meh. See above.
            // Not way into the idea of making another custom type where the only difference is we can access a
            // frigging internal variable, gonna be honest.
            ResetStream(stream);
        }

        private bool _exiting;

        private Error ParseRtf()
        {
            while (CurrentPos < Length)
            {
                if (_exiting) return Error.OK;

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
                        if ((ec = ParseKeyword()) != Error.OK) return ec;
                        break;
                    case '\r':
                    case '\n':
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
                    if (_currentScope.RtfDestinationState == RtfDestinationState.Normal)
                    {
                        _currentScope.Properties[symbol.Index] = param;
                    }
                    return Error.OK;
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
        private Error ChangeDestination(DestinationType destinationType)
        {
            switch (destinationType)
            {
                // IgnoreButDontSkipGroup is only relevant for plaintext extraction. As we're only parsing color
                // tables, we can just skip groups so marked.
                case DestinationType.IgnoreButDontSkipGroup:
                case DestinationType.Skip:
                    _currentScope.RtfDestinationState = RtfDestinationState.Skip;
                    return Error.OK;
                default:
                    //return Error.InvalidSymbolTableEntry;
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

            string ct = _colorTableSB.ToString();
            string[] entries = ct.Split(CA_Semicolon);

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
}
