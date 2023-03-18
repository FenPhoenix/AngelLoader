using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace AL_Common;

public class StreamReaderCustom
{
    private const int DefaultFileStreamBufferSize = 4096;
    private const int MinBufferSize = 128;
    private Stream _stream;

    // Allocated stuff
    private Encoding _encoding;
    private Decoder _decoder;
    private byte[] _byteBuffer;
    private char[] _charBuffer;
    private byte[] _preamble;

    private int _charPos;
    private int _charLen;
    private int _byteLen;
    private int _bytePos;
    private int _maxCharsPerBuffer;
    private bool _detectEncoding;
    private bool _checkPreamble;
    private bool _isBlocked;
    private bool _closable;

    internal static int DefaultBufferSize => 1024;

    /// <summary>Initializes a new instance of the <see cref="T:System.IO.StreamReaderCustom" /> class for the specified stream, with the specified character encoding, byte order mark detection option, and buffer size.</summary>
    /// <param name="stream">The stream to be read.</param>
    /// <param name="encoding">The character encoding to use.</param>
    /// <param name="detectEncodingFromByteOrderMarks">Indicates whether to look for byte order marks at the beginning of the file.</param>
    /// <param name="bufferSize">The minimum buffer size.</param>
    /// <exception cref="T:System.ArgumentException">The stream does not support reading.</exception>
    /// <exception cref="T:System.ArgumentNullException">
    /// <paramref name="stream" /> or <paramref name="encoding" /> is <see langword="null" />.</exception>
    /// <exception cref="T:System.ArgumentOutOfRangeException">
    /// <paramref name="bufferSize" /> is less than or equal to zero.</exception>
    public StreamReaderCustom(
      Stream stream,
      Encoding encoding,
      bool detectEncodingFromByteOrderMarks,
      byte[] buffer)
      : this(stream, encoding, detectEncodingFromByteOrderMarks, buffer, false)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="T:System.IO.StreamReaderCustom" /> class for the specified stream based on the specified character encoding, byte order mark detection option, and buffer size, and optionally leaves the stream open.</summary>
    /// <param name="stream">The stream to read.</param>
    /// <param name="encoding">The character encoding to use.</param>
    /// <param name="detectEncodingFromByteOrderMarks">
    /// <see langword="true" /> to look for byte order marks at the beginning of the file; otherwise, <see langword="false" />.</param>
    /// <param name="bufferSize">The minimum buffer size.</param>
    /// <param name="leaveOpen">
    /// <see langword="true" /> to leave the stream open after the <see cref="T:System.IO.StreamReaderCustom" /> object is disposed; otherwise, <see langword="false" />.</param>
    public StreamReaderCustom(
      Stream stream,
      Encoding encoding,
      bool detectEncodingFromByteOrderMarks,
      byte[] buffer,
      bool leaveOpen)
    {
        if (stream == null || encoding == null)
            throw new ArgumentNullException(stream == null ? nameof(stream) : nameof(encoding));
        if (!stream.CanRead)
            throw new ArgumentException(("Argument_StreamNotReadable"));
        Init(stream, encoding, detectEncodingFromByteOrderMarks, buffer, leaveOpen);
    }

    private void Init(
      Stream stream,
      Encoding encoding,
      bool detectEncodingFromByteOrderMarks,
      byte[] buffer,
      bool leaveOpen)
    {
        _stream = stream;
        _encoding = encoding;
        _decoder = encoding.GetDecoder();

        int bufferSize = buffer.Length;
        if (bufferSize < 128) bufferSize = 128;
        _byteBuffer = new byte[bufferSize];

        _maxCharsPerBuffer = encoding.GetMaxCharCount(bufferSize);
        _charBuffer = new char[_maxCharsPerBuffer];
        _byteLen = 0;
        _bytePos = 0;
        _detectEncoding = detectEncodingFromByteOrderMarks;
        _preamble = encoding.GetPreamble();
        _checkPreamble = _preamble.Length != 0;
        _isBlocked = false;
        _closable = !leaveOpen;
    }

    /// <summary>Closes the <see cref="T:System.IO.StreamReaderCustom" /> object and the underlying stream, and releases any system resources associated with the reader.</summary>
    public void Close() => Dispose(true);

    /// <summary>Closes the underlying stream, releases the unmanaged resources used by the <see cref="T:System.IO.StreamReaderCustom" />, and optionally releases the managed resources.</summary>
    /// <param name="disposing">
    /// <see langword="true" /> to release both managed and unmanaged resources; <see langword="false" /> to release only unmanaged resources.</param>
    protected void Dispose(bool disposing)
    {
        try
        {
            if (!(!LeaveOpen & disposing) || _stream == null)
                return;
            _stream.Close();
        }
        finally
        {
            if (!LeaveOpen && _stream != null)
            {
                _stream = (Stream)null;
                _encoding = (Encoding)null;
                _decoder = (Decoder)null;
                _byteBuffer = (byte[])null;
                _charBuffer = (char[])null;
                _charPos = 0;
                _charLen = 0;
            }
        }
    }

    /// <summary>Gets the current character encoding that the current <see cref="T:System.IO.StreamReaderCustom" /> object is using.</summary>
    /// <returns>The current character encoding used by the current reader. The value can be different after the first call to any <see cref="Overload:System.IO.StreamReaderCustom.Read" /> method of <see cref="T:System.IO.StreamReaderCustom" />, since encoding autodetection is not done until the first call to a <see cref="Overload:System.IO.StreamReaderCustom.Read" /> method.</returns>
    public Encoding CurrentEncoding
    {
        get => _encoding;
    }

    /// <summary>Returns the underlying stream.</summary>
    /// <returns>The underlying stream.</returns>
    public Stream BaseStream
    {
        get => _stream;
    }

    internal bool LeaveOpen => !_closable;

    /// <summary>Clears the internal buffer.</summary>
    public void DiscardBufferedData()
    {
        _byteLen = 0;
        _charLen = 0;
        _charPos = 0;
        if (_encoding != null)
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
            if (_stream == null)
                __Error.ReaderClosed();
            return _charPos >= _charLen && ReadBuffer() == 0;
        }
    }

    /// <summary>Returns the next available character but does not consume it.</summary>
    /// <returns>An integer representing the next character to be read, or -1 if there are no characters to be read or if the stream does not support seeking.</returns>
    /// <exception cref="T:System.IO.IOException">An I/O error occurs.</exception>
    public int Peek()
    {
        if (_stream == null)
            __Error.ReaderClosed();
        return _charPos == _charLen && (_isBlocked || ReadBuffer() == 0) ? -1 : (int)_charBuffer[_charPos];
    }

    /// <summary>Reads the next character from the input stream and advances the character position by one character.</summary>
    /// <returns>The next character from the input stream represented as an <see cref="T:System.Int32" /> object, or -1 if no more characters are available.</returns>
    /// <exception cref="T:System.IO.IOException">An I/O error occurs.</exception>
    public int Read()
    {
        if (_stream == null)
            __Error.ReaderClosed();
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
        if (_stream == null)
            __Error.ReaderClosed();
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
                    Buffer.BlockCopy((Array)_charBuffer, _charPos * 2, (Array)buffer, (index + num1) * 2, num2 * 2);
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

    /// <summary>Reads all characters from the current position to the end of the stream.</summary>
    /// <returns>The rest of the stream as a string, from the current position to the end. If the current position is at the end of the stream, returns an empty string ("").</returns>
    /// <exception cref="T:System.OutOfMemoryException">There is insufficient memory to allocate a buffer for the returned string.</exception>
    /// <exception cref="T:System.IO.IOException">An I/O error occurs.</exception>
    public string ReadToEnd()
    {
        if (_stream == null)
            __Error.ReaderClosed();
        StringBuilder stringBuilder = new StringBuilder(_charLen - _charPos);
        do
        {
            stringBuilder.Append(_charBuffer, _charPos, _charLen - _charPos);
            _charPos = _charLen;
            ReadBuffer();
        }
        while (_charLen > 0);
        return stringBuilder.ToString();
    }

    /// <summary>Reads a specified maximum number of characters from the current stream and writes the data to a buffer, beginning at the specified index.</summary>
    /// <param name="buffer">When this method returns, contains the specified character array with the values between <paramref name="index" /> and (index + count - 1) replaced by the characters read from the current source.</param>
    /// <param name="index">The position in <paramref name="buffer" /> at which to begin writing.</param>
    /// <param name="count">The maximum number of characters to read.</param>
    /// <returns>The number of characters that have been read. The number will be less than or equal to <paramref name="count" />, depending on whether all input characters have been read.</returns>
    /// <exception cref="T:System.ArgumentNullException">
    /// <paramref name="buffer" /> is <see langword="null" />.</exception>
    /// <exception cref="T:System.ArgumentException">The buffer length minus <paramref name="index" /> is less than <paramref name="count" />.</exception>
    /// <exception cref="T:System.ArgumentOutOfRangeException">
    /// <paramref name="index" /> or <paramref name="count" /> is negative.</exception>
    /// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.IO.StreamReaderCustom" /> is closed.</exception>
    /// <exception cref="T:System.IO.IOException">An I/O error occurred.</exception>
    public int ReadBlock([In, Out] char[] buffer, int index, int count)
    {
        if (buffer == null)
            throw new ArgumentNullException(nameof(buffer), ("ArgumentNull_Buffer"));
        if (index < 0 || count < 0)
            throw new ArgumentOutOfRangeException(index < 0 ? nameof(index) : nameof(count), ("ArgumentOutOfRange_NeedNonNegNum"));
        if (buffer.Length - index < count)
            throw new ArgumentException(("Argument_InvalidOffLen"));
        if (_stream == null)
            __Error.ReaderClosed();

        int num1 = 0;
        int num2;
        do
        {
            num1 += num2 = Read(buffer, index + num1, count - num1);
        }
        while (num2 > 0 && num1 < count);
        return num1;
    }

    private void CompressBuffer(int n)
    {
        Buffer.BlockCopy((Array)_byteBuffer, n, (Array)_byteBuffer, 0, _byteLen - n);
        _byteLen -= n;
    }

    private void DetectEncoding()
    {
        if (_byteLen < 2)
            return;
        _detectEncoding = false;
        bool flag = false;
        if (_byteBuffer[0] == (byte)254 && _byteBuffer[1] == byte.MaxValue)
        {
            _encoding = (Encoding)new UnicodeEncoding(true, true);
            CompressBuffer(2);
            flag = true;
        }
        else if (_byteBuffer[0] == byte.MaxValue && _byteBuffer[1] == (byte)254)
        {
            if (_byteLen < 4 || _byteBuffer[2] != (byte)0 || _byteBuffer[3] != (byte)0)
            {
                _encoding = (Encoding)new UnicodeEncoding(false, true);
                CompressBuffer(2);
                flag = true;
            }
            else
            {
                _encoding = (Encoding)new UTF32Encoding(false, true);
                CompressBuffer(4);
                flag = true;
            }
        }
        else if (_byteLen >= 3 && _byteBuffer[0] == (byte)239 && _byteBuffer[1] == (byte)187 && _byteBuffer[2] == (byte)191)
        {
            _encoding = Encoding.UTF8;
            CompressBuffer(3);
            flag = true;
        }
        else if (_byteLen >= 4 && _byteBuffer[0] == (byte)0 && _byteBuffer[1] == (byte)0 && _byteBuffer[2] == (byte)254 && _byteBuffer[3] == byte.MaxValue)
        {
            _encoding = (Encoding)new UTF32Encoding(true, true);
            CompressBuffer(4);
            flag = true;
        }
        else if (_byteLen == 2)
            _detectEncoding = true;
        if (!flag)
            return;
        _decoder = _encoding.GetDecoder();
        _maxCharsPerBuffer = _encoding.GetMaxCharCount(_byteBuffer.Length);
        _charBuffer = new char[_maxCharsPerBuffer];
    }

    private bool IsPreamble()
    {
        if (!_checkPreamble)
            return _checkPreamble;
        int num1 = _byteLen >= _preamble.Length ? _preamble.Length - _bytePos : _byteLen - _bytePos;
        int num2 = 0;
        while (num2 < num1)
        {
            if ((int)_byteBuffer[_bytePos] != (int)_preamble[_bytePos])
            {
                _bytePos = 0;
                _checkPreamble = false;
                break;
            }
            ++num2;
            ++_bytePos;
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

    internal int ReadBuffer()
    {
        _charLen = 0;
        _charPos = 0;
        if (!_checkPreamble)
            _byteLen = 0;
        do
        {
            if (_checkPreamble)
            {
                int num = _stream.Read(_byteBuffer, _bytePos, _byteBuffer.Length - _bytePos);
                if (num == 0)
                {
                    if (_byteLen > 0)
                    {
                        _charLen += _decoder.GetChars(_byteBuffer, 0, _byteLen, _charBuffer, _charLen);
                        _bytePos = _byteLen = 0;
                    }
                    return _charLen;
                }
                _byteLen += num;
            }
            else
            {
                _byteLen = _stream.Read(_byteBuffer, 0, _byteBuffer.Length);
                if (_byteLen == 0)
                    return _charLen;
            }
            _isBlocked = _byteLen < _byteBuffer.Length;
            if (!IsPreamble())
            {
                if (_detectEncoding && _byteLen >= 2)
                    DetectEncoding();
                _charLen += _decoder.GetChars(_byteBuffer, 0, _byteLen, _charBuffer, _charLen);
            }
        }
        while (_charLen == 0);
        return _charLen;
    }

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

    /// <summary>Reads a line of characters from the current stream and returns the data as a string.</summary>
    /// <returns>The next line from the input stream, or <see langword="null" /> if the end of the input stream is reached.</returns>
    /// <exception cref="T:System.OutOfMemoryException">There is insufficient memory to allocate a buffer for the returned string.</exception>
    /// <exception cref="T:System.IO.IOException">An I/O error occurs.</exception>
    public string ReadLine()
    {
        if (_stream == null)
            __Error.ReaderClosed();
        if (_charPos == _charLen && ReadBuffer() == 0)
            return (string)null;
        StringBuilder stringBuilder = (StringBuilder)null;
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
                        if (stringBuilder != null)
                        {
                            stringBuilder.Append(_charBuffer, _charPos, charPos - _charPos);
                            str = stringBuilder.ToString();
                        }
                        else
                            str = new string(_charBuffer, _charPos, charPos - _charPos);
                        _charPos = charPos + 1;
                        if (ch == '\r' && (_charPos < _charLen || ReadBuffer() > 0) && _charBuffer[_charPos] == '\n')
                            ++_charPos;
                        return str;
                    default:
                        ++charPos;
                        continue;
                }
            }
            while (charPos < _charLen);
            int charCount = _charLen - _charPos;
            if (stringBuilder == null)
                stringBuilder = new StringBuilder(charCount + 80);
            stringBuilder.Append(_charBuffer, _charPos, charCount);
        }
        while (ReadBuffer() > 0);
        return stringBuilder.ToString();
    }

    internal static class __Error
    {
        internal static void ReaderClosed() => throw new ObjectDisposedException(null, ("ObjectDisposed_ReaderClosed"));
    }
}
