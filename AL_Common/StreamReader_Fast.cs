using System;
using System.Diagnostics.Contracts;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

// @MEM: This thing has #if FEATURE_UTF32 in here, we need to check if it's supposed to be enabled
// before using this reader for general purposes! (FMData.ini is our file so it's always UTF8, so it's fine there)
namespace AL_Common
{
    [Serializable]
    [ComVisible(true)]
    public class StreamReader_Fast : TextReader
    {
        // Using a 1K byte buffer and a 4K FileStream buffer works out pretty well
        // perf-wise.  On even a 40 MB text file, any perf loss by using a 4K
        // buffer is negated by the win of allocating a smaller byte[], which 
        // saves construction time.  This does break adaptive buffering,
        // but this is slightly faster.
        internal static int DefaultBufferSize => 1024;

        private const int MinBufferSize = 128;

        private Stream _stream;
        private Encoding _encoding;
        private Decoder _decoder;
        private byte[] _byteBuffer;
        private char[] _charBuffer;
        private readonly StringBuilder _sb = new();
        private readonly byte[] _preamble;   // Encoding's preamble, which identifies this encoding.
        private int _charPos;
        private int _charLen;
        // Record the number of valid bytes in the byteBuffer, for a few checks.
        private int _byteLen;
        // This is used only for preamble detection
        private int _bytePos;

        // This is the maximum number of chars we can get from one call to 
        // ReadBuffer.  Used so ReadBuffer can tell when to copy data into
        // a user's char[] directly, instead of our internal char[].
        private int _maxCharsPerBuffer;

        // We will support looking for byte order marks in the stream and trying
        // to decide what the encoding might be from the byte order marks, IF they
        // exist.  But that's all we'll do.  
        private bool _detectEncoding;

        // Whether we must still check for the encoding's given preamble at the
        // beginning of this file.
        private bool _checkPreamble;

        // The intent of this field is to leave open the underlying stream when 
        // disposing of this StreamReader.  A name like _leaveOpen is better, 
        // but this type is serializable, and this field's name was _closable.
        private readonly bool _closable;  // Whether to close the underlying stream.

        public StreamReader_Fast(Stream stream)
            : this(stream, true)
        {
        }

        public StreamReader_Fast(Stream stream, bool detectEncodingFromByteOrderMarks)
            : this(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks, DefaultBufferSize, false)
        {
        }

        public StreamReader_Fast(Stream stream, Encoding encoding)
            : this(stream, encoding, true, DefaultBufferSize, false)
        {
        }

        public StreamReader_Fast(Stream stream, Encoding encoding, bool detectEncodingFromByteOrderMarks)
            : this(stream, encoding, detectEncodingFromByteOrderMarks, DefaultBufferSize, false)
        {
        }

        // Creates a new StreamReader for the given stream.  The 
        // character encoding is set by encoding and the buffer size, 
        // in number of 16-bit characters, is set by bufferSize.  
        // 
        // Note that detectEncodingFromByteOrderMarks is a very
        // loose attempt at detecting the encoding by looking at the first
        // 3 bytes of the stream.  It will recognize UTF-8, little endian
        // unicode, and big endian unicode text, but that's it.  If neither
        // of those three match, it will use the Encoding you provided.
        // 
        public StreamReader_Fast(Stream stream, Encoding encoding, bool detectEncodingFromByteOrderMarks, int bufferSize)
            : this(stream, encoding, detectEncodingFromByteOrderMarks, bufferSize, false)
        {
        }

        public StreamReader_Fast(Stream stream, Encoding encoding, bool detectEncodingFromByteOrderMarks, int bufferSize, bool leaveOpen)
        {
            if (!stream.CanRead)
            {
                throw new ArgumentException("Stream not readable");
            }
            if (bufferSize <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(bufferSize), "Argument out of range");
            }

            this._stream = stream;
            this._encoding = encoding;
            _decoder = encoding.GetDecoder();
            if (bufferSize < MinBufferSize) bufferSize = MinBufferSize;
            _byteBuffer = new byte[bufferSize];
            _maxCharsPerBuffer = encoding.GetMaxCharCount(bufferSize);
            _charBuffer = new char[_maxCharsPerBuffer];
            _byteLen = 0;
            _bytePos = 0;
            _detectEncoding = detectEncodingFromByteOrderMarks;
            _preamble = encoding.GetPreamble();
            _checkPreamble = _preamble.Length > 0;
            _closable = !leaveOpen;
        }

        public override void Close() => Dispose(true);

        protected override void Dispose(bool disposing)
        {
            // Dispose of our resources if this StreamReader is closable.
            // Note that Console.In should be left open.
            try
            {
                // Note that Stream.Close() can potentially throw here. So we need to 
                // ensure cleaning up internal resources, inside the finally block.  
                if (!LeaveOpen && disposing && (_stream != null!))
                {
                    _stream.Close();
                }
            }
            finally
            {
                if (!LeaveOpen && (_stream != null))
                {
                    _stream = null!;
                    _encoding = null!;
                    _decoder = null!;
                    _byteBuffer = null!;
                    _charBuffer = null!;
                    _charPos = 0;
                    _charLen = 0;
                    base.Dispose(disposing);
                }
            }
        }

        public virtual Encoding CurrentEncoding => _encoding;

        public virtual Stream BaseStream => _stream;

        internal bool LeaveOpen => !_closable;

        // Trims n bytes from the front of the buffer.
        private void CompressBuffer(int n)
        {
            Contract.Assert(_byteLen >= n, "CompressBuffer was called with a number of bytes greater than the current buffer length.  Are two threads using this StreamReader at the same time?");
            Buffer.BlockCopy(_byteBuffer, n, _byteBuffer, 0, _byteLen - n);
            _byteLen -= n;
        }

        private void DetectEncoding()
        {
            if (_byteLen < 2) return;
            _detectEncoding = false;
            bool changedEncoding = false;
            if (_byteBuffer[0] == 0xFE && _byteBuffer[1] == 0xFF)
            {
                // Big Endian Unicode

                _encoding = new UnicodeEncoding(true, true);
                CompressBuffer(2);
                changedEncoding = true;
            }
            else if (_byteBuffer[0] == 0xFF && _byteBuffer[1] == 0xFE)
            {
                // Little Endian Unicode, or possibly little endian UTF32
                if (_byteLen < 4 || _byteBuffer[2] != 0 || _byteBuffer[3] != 0)
                {
                    _encoding = new UnicodeEncoding(false, true);
                    CompressBuffer(2);
                    changedEncoding = true;
                }
#if FEATURE_UTF32   
                else {
                    encoding = new UTF32Encoding(false, true);
                    CompressBuffer(4);
                changedEncoding = true;
            }
#endif            
            }
            else if (_byteLen >= 3 && _byteBuffer[0] == 0xEF && _byteBuffer[1] == 0xBB && _byteBuffer[2] == 0xBF)
            {
                // UTF-8
                _encoding = Encoding.UTF8;
                CompressBuffer(3);
                changedEncoding = true;
            }
#if FEATURE_UTF32            
            else if (byteLen >= 4 && byteBuffer[0] == 0 && byteBuffer[1] == 0 &&
                     byteBuffer[2] == 0xFE && byteBuffer[3] == 0xFF) {
                // Big Endian UTF32
                encoding = new UTF32Encoding(true, true);
                CompressBuffer(4);
                changedEncoding = true;
            }
#endif            
            else if (_byteLen == 2)
            {
                _detectEncoding = true;
            }
            // Note: in the future, if we change this algorithm significantly,
            // we can support checking for the preamble of the given encoding.

            if (changedEncoding)
            {
                _decoder = _encoding.GetDecoder();
                _maxCharsPerBuffer = _encoding.GetMaxCharCount(_byteBuffer.Length);
                _charBuffer = new char[_maxCharsPerBuffer];
            }
        }

        // Trims the preamble bytes from the byteBuffer. This routine can be called multiple times
        // and we will buffer the bytes read until the preamble is matched or we determine that
        // there is no match. If there is no match, every byte read previously will be available 
        // for further consumption. If there is a match, we will compress the buffer for the 
        // leading preamble bytes
        private bool IsPreamble()
        {
            if (!_checkPreamble) return _checkPreamble;

            Contract.Assert(_bytePos <= _preamble.Length, "_compressPreamble was called with the current bytePos greater than the preamble buffer length.  Are two threads using this StreamReader at the same time?");
            int len = _byteLen >= _preamble.Length ? _preamble.Length - _bytePos : _byteLen - _bytePos;

            for (int i = 0; i < len; i++, _bytePos++)
            {
                if (_byteBuffer[_bytePos] != _preamble[_bytePos])
                {
                    _bytePos = 0;
                    _checkPreamble = false;
                    break;
                }
            }

            Contract.Assert(_bytePos <= _preamble.Length, "possible bug in _compressPreamble.  Are two threads using this StreamReader at the same time?");

            if (_checkPreamble)
            {
                if (_bytePos == _preamble.Length)
                {
                    // We have a match
                    CompressBuffer(_preamble.Length);
                    _bytePos = 0;
                    _checkPreamble = false;
                    _detectEncoding = false;
                }
            }

            return _checkPreamble;
        }

        internal virtual int ReadBuffer()
        {
            _charLen = 0;
            _charPos = 0;

            if (!_checkPreamble) _byteLen = 0;

            do
            {
                if (_checkPreamble)
                {
                    int len = _stream.Read(_byteBuffer, _bytePos, _byteBuffer.Length - _bytePos);

                    if (len == 0)
                    {
                        // EOF but we might have buffered bytes from previous 
                        // attempt to detect preamble that needs to be decoded now
                        if (_byteLen > 0)
                        {
                            _charLen += _decoder.GetChars(_byteBuffer, 0, _byteLen, _charBuffer, _charLen);
                            // Need to zero out the byteLen after we consume these bytes so that we don't keep infinitely hitting this code path
                            _bytePos = _byteLen = 0;
                        }

                        return _charLen;
                    }

                    _byteLen += len;
                }
                else
                {
                    _byteLen = _stream.Read(_byteBuffer, 0, _byteBuffer.Length);

                    // We're at EOF
                    if (_byteLen == 0) return _charLen;
                }

                // Check for preamble before detect encoding. This is not to override the
                // user supplied Encoding for the one we implicitly detect. The user could
                // customize the encoding which we will loose, such as ThrowOnError on UTF8
                if (IsPreamble()) continue;

                // If we're supposed to detect the encoding and haven't done so yet,
                // do it.  Note this may need to be called more than once.
                if (_detectEncoding && _byteLen >= 2) DetectEncoding();

                _charLen += _decoder.GetChars(_byteBuffer, 0, _byteLen, _charBuffer, _charLen);
            } while (_charLen == 0);

            return _charLen;
        }

        // Reads a line. A line is defined as a sequence of characters followed by
        // a carriage return ('\r'), a line feed ('\n'), or a carriage return
        // immediately followed by a line feed. The resulting string does not
        // contain the terminating carriage return and/or line feed. The returned
        // value is null if the end of the input stream has been reached.
        //
        public StringBuilder? ReadLine_Custom()
        {
            if (_stream == null!) __Error.ReaderClosed();

            if (_charPos == _charLen)
            {
                if (ReadBuffer() == 0) return null;
            }

            //StringBuilder sb = null;
            _sb.Clear();
            do
            {
                int i = _charPos;
                do
                {
                    char ch = _charBuffer[i];
                    // Note the following common line feed chars:
                    // \n - UNIX   \r\n - DOS   \r - Mac
                    if (ch is '\r' or '\n')
                    {
                        //String s;
                        //if (sb != null)
                        if (_sb.Length > 0)
                        {
                            _sb.Append(_charBuffer, _charPos, i - _charPos);
                            //s = sb.ToString();
                        }
                        else
                        {
                            //s = new String(charBuffer, _charPos, i - _charPos);
                            _sb.Clear();
                            _sb.Append(_charBuffer, _charPos, i - _charPos);
                        }
                        _charPos = i + 1;
                        if (ch == '\r' && (_charPos < _charLen || ReadBuffer() > 0))
                        {
                            if (_charBuffer[_charPos] == '\n') _charPos++;
                        }
                        return _sb;
                    }
                    i++;
                } while (i < _charLen);
                i = _charLen - _charPos;
                //if (sb == null) sb = new StringBuilder(i + 80);
                if (_sb.Length == 0) _sb.Capacity = i + 80;
                _sb.Append(_charBuffer, _charPos, i);
            } while (ReadBuffer() > 0);
            return _sb;
        }
    }

    internal static class __Error
    {
        internal static void ReaderClosed() => throw new ObjectDisposedException(null, "Reader closed");
    }
}
