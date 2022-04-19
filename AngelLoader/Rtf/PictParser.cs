using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace AngelLoader
{
    public sealed class ImageFixer : AL_Common.RTFParserBase
    {
        // Static - we do want to keep this around because this will be run very frequently
        private static readonly HashSet<string> _keywordHashSet = new()
        {
            "emfblip",
            "pngblip",
            "jpegblip",
            "macpict",
            "pmmetafileN",
            "wmetafileN",
            "dibitmapN",
            "wbitmapN"
        };

        private byte[] _replaceArray = Array.Empty<byte>();

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
                Error error = ParseRtf();
                return error == Error.OK;
            }
            catch
            {
                return false;
            }
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

        private static ListFast<char> CreateListFastChar(string source)
        {
            var ret = new ListFast<char>(source.Length);
            for (int i = 0; i < source.Length; i++)
            {
                ret.AddFast(source[i]);
            }
            return ret;
        }

        private static readonly StringBuilder _tempSB = new(_keywordMaxLen);

        private static string ListFastToString(ListFast<char> listFast)
        {
            _tempSB.Clear();
            for (int i = 0; i < listFast.Count; i++)
            {
                _tempSB.Append(listFast.ItemsArray[i]);
            }
            return _tempSB.ToString();
        }

        protected override Error DispatchKeyword(int param, bool hasParam)
        {
            // TODO: We want to get rid of the ToString call if we can...
            if (_keywordHashSet.Contains(ListFastToString(_keyword)))
            {
                if (SeqEqual(_keyword, "pngblip"))
                {
                    for (int i = 0; i < _replaceArray.Length; i++)
                    {
                        _stream[i] = _replaceArray[i];
                    }
                }
                _exiting = true;
            }
            return Error.OK;
        }
    }
}
