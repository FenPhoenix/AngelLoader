using System;
using System.Runtime.CompilerServices;

namespace AngelLoader
{
    public sealed class ImageFixer : AL_Common.RTFParserBase
    {
        private static readonly KeywordHashSet _keywordHashSet = new();

        private sealed class KeywordHashSet
        {
            /* ANSI-C code produced by gperf version 3.1 */
            /* Command-line: gperf --output-file='c:\\gperf\\tools\\gperf_out.txt' -t 'c:\\gperf\\tools\\gperf_rtf.txt'  */
            /* Computed positions: -k'1' */

            //private const int TOTAL_KEYWORDS = 8;
            private const int MIN_WORD_LENGTH = 7;
            private const int MAX_WORD_LENGTH = 10;
            //private const int MIN_HASH_VALUE = 7;
            private const int MAX_HASH_VALUE = 22;
            /* maximum key range = 16, duplicates = 0 */

            private readonly byte[] asso_values =
            {
                23, 23, 23, 23, 23, 23, 23, 23, 23, 23,
                23, 23, 23, 23, 23, 23, 23, 23, 23, 23,
                23, 23, 23, 23, 23, 23, 23, 23, 23, 23,
                23, 23, 23, 23, 23, 23, 23, 23, 23, 23,
                23, 23, 23, 23, 23, 23, 23, 23, 23, 23,
                23, 23, 23, 23, 23, 23, 23, 23, 23, 23,
                23, 23, 23, 23, 23, 23, 23, 23, 23, 23,
                23, 23, 23, 23, 23, 23, 23, 23, 23, 23,
                23, 23, 23, 23, 23, 23, 23, 23, 23, 23,
                23, 23, 23, 23, 23, 23, 23, 23, 23, 23,
                5, 15, 23, 23, 23, 23, 0, 23, 23, 10,
                23, 23, 5, 23, 23, 23, 23, 23, 23, 0,
                23, 23, 23, 23, 23, 23, 23, 23, 23, 23,
                23, 23, 23, 23, 23, 23, 23, 23, 23, 23,
                23, 23, 23, 23, 23, 23, 23, 23, 23, 23,
                23, 23, 23, 23, 23, 23, 23, 23, 23, 23,
                23, 23, 23, 23, 23, 23, 23, 23, 23, 23,
                23, 23, 23, 23, 23, 23, 23, 23, 23, 23,
                23, 23, 23, 23, 23, 23, 23, 23, 23, 23,
                23, 23, 23, 23, 23, 23, 23, 23, 23, 23,
                23, 23, 23, 23, 23, 23, 23, 23, 23, 23,
                23, 23, 23, 23, 23, 23, 23, 23, 23, 23,
                23, 23, 23, 23, 23, 23, 23, 23, 23, 23,
                23, 23, 23, 23, 23, 23, 23, 23, 23, 23,
                23, 23, 23, 23, 23, 23, 23, 23, 23, 23,
                23, 23, 23, 23, 23, 23
            };

            private readonly string?[] _symbolTable =
            {
                null, null, null, null, null, null, null,
                "wbitmap", // can have param
                "jpegblip",
                "wmetafile", // can have param
                null, null,
                "pngblip",
                "dibitmap", // can have param
                null,
                "pmmetafile", // can have param
                null,
                "macpict",
                null, null, null, null,
                "emfblip"
            };

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private uint Hash(ListFast<char> str, int len)
            {
                return (uint)(len + asso_values[str.ItemsArray[0]]);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool Contains(ListFast<char> str)
            {
                int len = str.Count;
                if (len is <= MAX_WORD_LENGTH and >= MIN_WORD_LENGTH)
                {
                    uint key = Hash(str, len);

                    if (key <= MAX_HASH_VALUE)
                    {
                        string? keyword = _symbolTable[key];
                        return keyword != null && SeqEqual(str, keyword);
                    }
                }

                return false;
            }
        }

        private byte[] _replaceArray = Array.Empty<byte>();

        private bool _exiting;
        private bool _replaced;

        private ByteArraySegmentSlim _stream;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ResetStream(ByteArraySegmentSlim stream)
        {
            _stream = stream;
            base.ResetStreamBase(stream.Count);
        }

        private void Reset(ByteArraySegmentSlim stream, byte[] replaceArray)
        {
            base.ResetBase();

            _exiting = false;
            _replaced = false;
            _replaceArray = replaceArray;

            ResetStream(stream);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override byte StreamReadByte() => _stream[(int)CurrentPos];

        public bool Run(ByteArraySegmentSlim stream, byte[] replaceArray)
        {
            Reset(stream, replaceArray);

            try
            {
                ParseRtf();
                return _replaced;
            }
            catch
            {
                return false;
            }
        }

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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool SeqEqual(ListFast<char> seq1, string seq2)
        {
            int seq1Count = seq1.Count;
            if (seq1Count != seq2.Length) return false;

            for (int ci = 0; ci < seq1Count; ci++)
            {
                if (seq1.ItemsArray[ci] != seq2[ci]) return false;
            }
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Replace(ByteArraySegmentSlim original, byte[] replace)
        {
            for (int i = 0; i < replace.Length; i++)
            {
                original[i] = replace[i];
            }
        }

        protected override Error DispatchKeyword(int param, bool hasParam)
        {
            if (_keywordHashSet.Contains(_keyword))
            {
                if (SeqEqual(_keyword, "pngblip"))
                {
                    Replace(_stream, _replaceArray);
                    _replaced = true;
                }
                _exiting = true;
            }
            return Error.OK;
        }
    }
}
