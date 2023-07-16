//#define ENABLE_UNUSED

using System;
using System.Collections.Generic;
using System.IO;
#if ENABLE_UNUSED
using System.Runtime.InteropServices;
#endif
using System.Text;

namespace AL_Common;

/// <summary>
/// Meant to be instantiated once and reused via <see cref="T:AL_Common.StreamReaderCustom.SRC_Wrapper"/>.
/// </summary>
public sealed class StreamReaderCustom
{
    /// <summary>
    /// For convenience of using-semantics, but without actually constructing or disposing the underlying
    /// <see cref="T:StreamReaderCustom"/> object (allocation avoidance).
    /// </summary>
    public readonly ref struct SRC_Wrapper
    {
        public readonly StreamReaderCustom Reader;

        /// <summary>
        /// UTF8 version to avoid the encoding allocation batch stuff when we just want the cached StringBuilder.
        /// The stream will be disposed when this struct is disposed.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="sr"></param>
        public SRC_Wrapper(Stream stream, StreamReaderCustom sr)
        {
            Reader = sr;
            sr.Init(stream, Encoding.UTF8, true, encodingCruftEnabled: false);
        }

        /// <summary>
        /// The stream will be disposed when this struct is disposed.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="encoding"></param>
        /// <param name="detectEncodingFromByteOrderMarks"></param>
        /// <param name="sr"></param>
        public SRC_Wrapper(Stream stream, Encoding encoding, bool detectEncodingFromByteOrderMarks, StreamReaderCustom sr)
        {
            Reader = sr;
            sr.Init(stream, encoding, detectEncodingFromByteOrderMarks, encodingCruftEnabled: true);
        }

        public void Dispose() => Reader.DeInit();
    }

    private const int _knownEncodingCount = 140;

    private readonly byte[] _byteBuffer = new byte[_defaultBufferSize];

    private Stream _stream = null!;

    // Allocated stuff
    private Encoding _encoding = null!;

    private Dictionary<Encoding, Decoder>? _decoders;
    private Decoder _decoder = null!;

    private int _maxCharsPerBuffer;
    private Dictionary<int, char[]>? _charBuffers;
    private char[] _charBuffer = Array.Empty<char>();

    private int _charPos;
    private int _charLen;
    private int _bytePos;
    private int _byteLen;

    private bool _detectEncoding;

    private Dictionary<Encoding, byte[]>? _perEncodingPreambles;
    private byte[] _preamble = Array.Empty<byte>();

    private bool _checkPreamble;

#if ENABLE_UNUSED
    private bool _isBlocked;
#endif

    private const int _defaultBufferSize = 1024;

    private void Init(
      Stream stream,
      Encoding encoding,
      bool detectEncodingFromByteOrderMarks,
      bool encodingCruftEnabled)
    {
        _stream = stream;

        _encoding = encoding;

        _maxCharsPerBuffer = encoding.GetMaxCharCount(_defaultBufferSize);

        if (encodingCruftEnabled)
        {
            _decoders ??= new Dictionary<Encoding, Decoder>(_knownEncodingCount);
            if (_decoders.TryGetValue(encoding, out Decoder decoder))
            {
                _decoder = decoder;
            }
            else
            {
                _decoder = encoding.GetDecoder();
                _decoders[encoding] = _decoder;
            }

            _charBuffers ??= new Dictionary<int, char[]>(10)
            {
                { 1024, new char[1024] },
                { 1025, new char[1025] },
                { 513, new char[513] },
                { 514, new char[514] },
                { 1027, new char[1027] }
            };
            if (_charBuffers.TryGetValue(_maxCharsPerBuffer, out char[] maxCharsBuffer))
            {
                _charBuffer = maxCharsBuffer;
            }
            else
            {
                _charBuffer = new char[_maxCharsPerBuffer];
                _charBuffers[_maxCharsPerBuffer] = _charBuffer;
            }

            _perEncodingPreambles ??= new Dictionary<Encoding, byte[]>(_knownEncodingCount);
            if (_perEncodingPreambles.TryGetValue(encoding, out byte[] preamble))
            {
                _preamble = preamble;
            }
            else
            {
                _preamble = encoding.GetPreamble();
                _perEncodingPreambles[encoding] = _preamble;
            }
        }
        else
        {
            _decoder = encoding.GetDecoder();
            _charBuffer = new char[_maxCharsPerBuffer];
            _preamble = encoding.GetPreamble();
        }

        _charPos = 0;
        _charLen = 0;
        _bytePos = 0;
        _byteLen = 0;

        _detectEncoding = detectEncodingFromByteOrderMarks;

        _checkPreamble = _preamble.Length != 0;
#if ENABLE_UNUSED
        _isBlocked = false;
#endif
    }

    public void DeInit()
    {
        if (_stream != null!) _stream.Dispose();
        _stream = null!;
        _encoding = null!;
        _decoder = null!;
        _preamble = Array.Empty<byte>();
        _charBuffer = Array.Empty<char>();
    }

#if ENABLE_UNUSED

    /// <summary>Gets the current character encoding that the current <see cref="T:AL_Common.StreamReaderCustom" /> object is using.</summary>
    /// /// <returns>The current character encoding used by the current reader. The value can be different after the first call to any <see cref="Read()" /> method of <see cref="T:AL_Common.StreamReaderCustom" />, since encoding autodetection is not done until the first call to a <see cref="Read()" /> method.</returns>
    public Encoding CurrentEncoding => _encoding;

    /// <summary>Returns the underlying stream.</summary>
    /// <returns>The underlying stream.</returns>
    public Stream BaseStream => _stream;

    /// <summary>Clears the internal buffer.</summary>
    public void DiscardBufferedData()
    {
        _byteLen = 0;
        _charLen = 0;
        _charPos = 0;
        if (_encoding != null!)
            _decoder = _encoding.GetDecoder();
        _isBlocked = false;
    }

    /// <summary>Gets a value that indicates whether the current stream position is at the end of the stream.</summary>
    /// <returns>
    /// <see langword="true" /> if the current stream position is at the end of the stream; otherwise <see langword="false" />.</returns>
    /// <exception cref="T:System.ObjectDisposedException">The underlying stream has been disposed.</exception>
    public bool EndOfStream
    {
        get
        {
            if (_stream == null!)
                ThrowHelper.ReaderClosed();
            return _charPos >= _charLen && ReadBuffer() == 0;
        }
    }


    /// <summary>Returns the next available character but does not consume it.</summary>
    /// <returns>An integer representing the next character to be read, or -1 if there are no characters to be read or if the stream does not support seeking.</returns>
    /// <exception cref="T:System.IO.IOException">An I/O error occurs.</exception>
    public int Peek()
    {
        if (_stream == null!)
            ThrowHelper.ReaderClosed();
        return _charPos == _charLen && (_isBlocked || ReadBuffer() == 0) ? -1 : (int)_charBuffer[_charPos];
    }

    /// <summary>Reads the next character from the input stream and advances the character position by one character.</summary>
    /// <returns>The next character from the input stream represented as an <see cref="T:System.Int32" /> object, or -1 if no more characters are available.</returns>
    /// <exception cref="T:System.IO.IOException">An I/O error occurs.</exception>
    public int Read()
    {
        if (_stream == null!)
            ThrowHelper.ReaderClosed();
        if (_charPos == _charLen && ReadBuffer() == 0)
            return -1;
        int num = (int)_charBuffer[_charPos];
        ++_charPos;
        return num;
    }


    /// <summary>Reads a specified maximum of characters from the current stream into a buffer, beginning at the specified index.</summary>
    /// <param name="buffer">When this method returns, contains the specified character array with the values between <paramref name="index" /> and (index + count - 1) replaced by the characters read from the current source.</param>
    /// <param name="index">The index of <paramref name="buffer" /> at which to begin writing.</param>
    /// <param name="count">The maximum number of characters to read.</param>
    /// <returns>The number of characters that have been read, or 0 if at the end of the stream and no data was read. The number will be less than or equal to the <paramref name="count" /> parameter, depending on whether the data is available within the stream.</returns>
    /// <exception cref="T:System.ArgumentException">The buffer length minus <paramref name="index" /> is less than <paramref name="count" />.</exception>
    /// <exception cref="T:System.ArgumentNullException">
    /// <paramref name="buffer" /> is <see langword="null" />.</exception>
    /// <exception cref="T:System.ArgumentOutOfRangeException">
    /// <paramref name="index" /> or <paramref name="count" /> is negative.</exception>
    /// <exception cref="T:System.IO.IOException">An I/O error occurs, such as the stream is closed.</exception>
    public int Read([In, Out] char[] buffer, int index, int count)
    {
        if (buffer == null)
            throw new ArgumentNullException(nameof(buffer), ("ArgumentNull_Buffer"));
        if (index < 0 || count < 0)
            throw new ArgumentOutOfRangeException(index < 0 ? nameof(index) : nameof(count), ("ArgumentOutOfRange_NeedNonNegNum"));
        if (buffer.Length - index < count)
            throw new ArgumentException(("Argument_InvalidOffLen"));
        if (_stream == null!)
            ThrowHelper.ReaderClosed();
        int num1 = 0;
        bool readToUserBuffer = false;
        while (count > 0)
        {
            int num2 = _charLen - _charPos;
            if (num2 == 0)
                num2 = ReadBuffer(buffer, index + num1, count, out readToUserBuffer);
            if (num2 != 0)
            {
                if (num2 > count)
                    num2 = count;
                if (!readToUserBuffer)
                {
                    Buffer.BlockCopy(_charBuffer, _charPos * 2, buffer, (index + num1) * 2, num2 * 2);
                    _charPos += num2;
                }
                num1 += num2;
                count -= num2;
                if (_isBlocked)
                    break;
            }
            else
                break;
        }
        return num1;
    }

#endif

    private readonly StringBuilder _readToEndSB = new();

    /// <summary>Reads all characters from the current position to the end of the stream.</summary>
    /// <returns>The rest of the stream as a string, from the current position to the end. If the current position is at the end of the stream, returns an empty string ("").</returns>
    /// <exception cref="T:System.OutOfMemoryException">There is insufficient memory to allocate a buffer for the returned string.</exception>
    /// <exception cref="T:System.IO.IOException">An I/O error occurs.</exception>
    public string ReadToEnd()
    {
        if (_stream == null!)
        {
            ThrowHelper.ReaderClosed();
        }

        _readToEndSB.EnsureCapacity(_charLen - _charPos);
        _readToEndSB.Clear();
        do
        {
            _readToEndSB.Append(_charBuffer, _charPos, _charLen - _charPos);
            _charPos = _charLen;
            ReadBuffer();
        }
        while (_charLen > 0);
        return _readToEndSB.ToString();
    }

    private void CompressBuffer(int n)
    {
        Buffer.BlockCopy(_byteBuffer, n, _byteBuffer, 0, _byteLen - n);
        _byteLen -= n;
    }

    // @MEM(StreamReaderCustom.DetectEncoding()): This modifies the cached encoding stuff, so it causes extra allocations
    // It also creates new encodings...
    private void DetectEncoding()
    {
        if (_byteLen < 2)
        {
            return;
        }
        _detectEncoding = false;
        bool changedEncoding = false;
        if (_byteBuffer[0] == 0xFE && _byteBuffer[1] == 0xFF)
        {
            _encoding = new UnicodeEncoding(true, true);
            CompressBuffer(2);
            changedEncoding = true;
        }
        else if (_byteBuffer[0] == 0xFF && _byteBuffer[1] == 0xFE)
        {
            if (_byteLen < 4 || _byteBuffer[2] != 0 || _byteBuffer[3] != 0)
            {
                _encoding = new UnicodeEncoding(false, true);
                CompressBuffer(2);
                changedEncoding = true;
            }
            else
            {
                _encoding = new UTF32Encoding(false, true);
                CompressBuffer(4);
                changedEncoding = true;
            }
        }
        else if (_byteLen >= 3 && _byteBuffer[0] == 0xEF && _byteBuffer[1] == 0xBB && _byteBuffer[2] == 0xBF)
        {
            _encoding = Encoding.UTF8;
            CompressBuffer(3);
            changedEncoding = true;
        }
        else if (_byteLen >= 4 && _byteBuffer[0] == 0 && _byteBuffer[1] == 0 && _byteBuffer[2] == 0xFE && _byteBuffer[3] == 0xFF)
        {
            _encoding = new UTF32Encoding(true, true);
            CompressBuffer(4);
            changedEncoding = true;
        }
        else if (_byteLen == 2)
        {
            _detectEncoding = true;
        }
        if (!changedEncoding)
        {
            return;
        }
        _decoder = _encoding.GetDecoder();
        _maxCharsPerBuffer = _encoding.GetMaxCharCount(_byteBuffer.Length);
        _charBuffer = new char[_maxCharsPerBuffer];
    }

    private bool IsPreamble()
    {
        if (!_checkPreamble) return _checkPreamble;

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

        if (_checkPreamble && _bytePos == _preamble.Length)
        {
            CompressBuffer(_preamble.Length);
            _bytePos = 0;
            _checkPreamble = false;
            _detectEncoding = false;
        }

        return _checkPreamble;
    }

    private int ReadBuffer()
    {
        _charLen = 0;
        _charPos = 0;
        if (!_checkPreamble)
        {
            _byteLen = 0;
        }
        do
        {
            if (_checkPreamble)
            {
                int len = _stream.Read(_byteBuffer, _bytePos, _byteBuffer.Length - _bytePos);
                if (len == 0)
                {
                    if (_byteLen > 0)
                    {
                        _charLen += _decoder.GetChars(_byteBuffer, 0, _byteLen, _charBuffer, _charLen);
                        _bytePos = _byteLen = 0;
                    }
                    return _charLen;
                }
                _byteLen += len;
            }
            else
            {
                _byteLen = _stream.Read(_byteBuffer, 0, _byteBuffer.Length);
                if (_byteLen == 0)
                {
                    return _charLen;
                }
            }
#if ENABLE_UNUSED
            _isBlocked = _byteLen < _byteBuffer.Length;
#endif
            if (IsPreamble()) continue;

            if (_detectEncoding && _byteLen >= 2)
            {
                DetectEncoding();
            }
            _charLen += _decoder.GetChars(_byteBuffer, 0, _byteLen, _charBuffer, _charLen);
        }
        while (_charLen == 0);
        return _charLen;
    }

#if ENABLE_UNUSED

    private int ReadBuffer(
      char[] userBuffer,
      int userOffset,
      int desiredChars,
      out bool readToUserBuffer)
    {
        _charLen = 0;
        _charPos = 0;
        if (!_checkPreamble)
            _byteLen = 0;
        int charIndex = 0;
        readToUserBuffer = desiredChars >= _maxCharsPerBuffer;
        do
        {
            if (_checkPreamble)
            {
                int num = _stream.Read(_byteBuffer, _bytePos, _byteBuffer.Length - _bytePos);
                if (num == 0)
                {
                    if (_byteLen > 0)
                    {
                        if (readToUserBuffer)
                        {
                            charIndex = _decoder.GetChars(_byteBuffer, 0, _byteLen, userBuffer, userOffset + charIndex);
                            _charLen = 0;
                        }
                        else
                        {
                            charIndex = _decoder.GetChars(_byteBuffer, 0, _byteLen, _charBuffer, charIndex);
                            _charLen += charIndex;
                        }
                    }
                    return charIndex;
                }
                _byteLen += num;
            }
            else
            {
                _byteLen = _stream.Read(_byteBuffer, 0, _byteBuffer.Length);
                if (_byteLen == 0)
                    break;
            }
            _isBlocked = _byteLen < _byteBuffer.Length;
            if (!IsPreamble())
            {
                if (_detectEncoding && _byteLen >= 2)
                {
                    DetectEncoding();
                    readToUserBuffer = desiredChars >= _maxCharsPerBuffer;
                }
                _charPos = 0;
                if (readToUserBuffer)
                {
                    charIndex += _decoder.GetChars(_byteBuffer, 0, _byteLen, userBuffer, userOffset + charIndex);
                    _charLen = 0;
                }
                else
                {
                    charIndex = _decoder.GetChars(_byteBuffer, 0, _byteLen, _charBuffer, charIndex);
                    _charLen += charIndex;
                }
            }
        }
        while (charIndex == 0);
        _isBlocked &= charIndex < desiredChars;
        return charIndex;
    }

#endif

    private readonly StringBuilder _readLineSB = new();

    /// <summary>Reads a line of characters from the current stream and returns the data as a string.</summary>
    /// <returns>The next line from the input stream, or <see langword="null" /> if the end of the input stream is reached.</returns>
    /// <exception cref="T:System.OutOfMemoryException">There is insufficient memory to allocate a buffer for the returned string.</exception>
    /// <exception cref="T:System.IO.IOException">An I/O error occurs.</exception>
    public string? ReadLine()
    {
        if (_stream == null!)
        {
            ThrowHelper.ReaderClosed();
        }
        if (_charPos == _charLen && ReadBuffer() == 0)
        {
            return null;
        }
        bool sbCreated = false;
        _readLineSB.Clear();
        do
        {
            int charPos = _charPos;
            do
            {
                char ch = _charBuffer[charPos];
                switch (ch)
                {
                    case '\n':
                    case '\r':
                        string str;
                        if (sbCreated)
                        {
                            _readLineSB.Append(_charBuffer, _charPos, charPos - _charPos);
                            str = _readLineSB.ToString();
                        }
                        else
                        {
                            str = new string(_charBuffer, _charPos, charPos - _charPos);
                        }
                        _charPos = charPos + 1;
                        if (ch == '\r' && (_charPos < _charLen || ReadBuffer() > 0) && _charBuffer[_charPos] == '\n')
                        {
                            ++_charPos;
                        }
                        return str;
                    default:
                        ++charPos;
                        continue;
                }
            }
            while (charPos < _charLen);
            int charCount = _charLen - _charPos;
            if (!sbCreated)
            {
                _readLineSB.EnsureCapacity(charCount + 80);
                sbCreated = true;
            }
            _readLineSB.Append(_charBuffer, _charPos, charCount);
        }
        while (ReadBuffer() > 0);
        return _readLineSB.ToString();
    }
}
