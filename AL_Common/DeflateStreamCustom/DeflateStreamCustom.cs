using System;
using System.IO;

namespace AL_Common.DeflateStreamCustom;

public sealed class DeflateStreamCustom : Stream
{
    private Stream _stream;
    private readonly bool _leaveOpen;
    private InflaterZlib _inflater;
    private readonly byte[] buffer;

    /// <summary>Initializes a new instance of the <see cref="T:System.IO.Compression.DeflateStreamCustom" /> class by using the specified stream and compression mode, and optionally leaves the stream open.</summary>
    /// <param name="stream">The stream to compress or decompress.</param>
    /// <param name="buffer"></param>
    /// <param name="leaveOpen">
    /// <see langword="true" /> to leave the stream open after disposing the <see cref="T:System.IO.Compression.DeflateStreamCustom" /> object; otherwise, <see langword="false" />.</param>
    /// <exception cref="T:System.ArgumentNullException">
    /// <paramref name="stream" /> is <see langword="null" />.</exception>
    /// <exception cref="T:System.ArgumentException">
    /// -or-
    /// <see cref="T:System.IO.Compression.CompressionMode" /> is <see cref="F:System.IO.Compression.CompressionMode.Compress" /> and <see cref="P:System.IO.Stream.CanWrite" /> is <see langword="false" />.
    /// -or-
    /// <see cref="T:System.IO.Compression.CompressionMode" /> is <see cref="F:System.IO.Compression.CompressionMode.Decompress" /> and <see cref="P:System.IO.Stream.CanRead" /> is <see langword="false" />.</exception>
    public DeflateStreamCustom(Stream stream, byte[] buffer, bool leaveOpen)
    {
        _stream = stream;
        _leaveOpen = leaveOpen;
        if (!_stream.CanRead)
        {
            throw new ArgumentException("NotReadableStream", nameof(stream));
        }
        _inflater = new InflaterZlib();
        this.buffer = buffer;
    }

    /// <summary>Gets a reference to the underlying stream.</summary>
    /// <returns>A stream object that represents the underlying stream.</returns>
    /// <exception cref="T:System.ObjectDisposedException">The underlying stream is closed.</exception>
    public Stream BaseStream => _stream;

    /// <summary>Gets a value indicating whether the stream supports reading while decompressing a file.</summary>
    /// <returns>
    /// <see langword="true" /> if the <see cref="T:System.IO.Compression.CompressionMode" /> value is <see langword="Decompress" />, and the underlying stream is opened and supports reading; otherwise, <see langword="false" />.</returns>
    public override bool CanRead => _stream.CanRead;

    /// <summary>Gets a value indicating whether the stream supports writing.</summary>
    /// <returns>
    /// <see langword="true" /> if the <see cref="T:System.IO.Compression.CompressionMode" /> value is <see langword="Compress" />, and the underlying stream supports writing and is not closed; otherwise, <see langword="false" />.</returns>
    public override bool CanWrite => false;

    /// <summary>Gets a value indicating whether the stream supports seeking.</summary>
    /// <returns>
    /// <see langword="false" /> in all cases.</returns>
    public override bool CanSeek => false;

    /// <summary>This property is not supported and always throws a <see cref="T:System.NotSupportedException" />.</summary>
    /// <returns>A long value.</returns>
    /// <exception cref="T:System.NotSupportedException">This property is not supported on this stream.</exception>
    public override long Length => throw new NotSupportedException("NotSupported");

    /// <summary>This property is not supported and always throws a <see cref="T:System.NotSupportedException" />.</summary>
    /// <returns>A long value.</returns>
    /// <exception cref="T:System.NotSupportedException">This property is not supported on this stream.</exception>
    public override long Position
    {
        get => throw new NotSupportedException("NotSupported");
        set => throw new NotSupportedException("NotSupported");
    }

    /// <summary>The current implementation of this method has no functionality.</summary>
    /// <exception cref="T:System.ObjectDisposedException">The stream is closed.</exception>
    public override void Flush() => EnsureNotDisposed();

    /// <summary>This operation is not supported and always throws a <see cref="T:System.NotSupportedException" />.</summary>
    /// <param name="offset">The location in the stream.</param>
    /// <param name="origin">One of the <see cref="T:System.IO.SeekOrigin" /> values.</param>
    /// <returns>A long value.</returns>
    /// <exception cref="T:System.NotSupportedException">This property is not supported on this stream.</exception>
    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException("NotSupported");

    /// <summary>This operation is not supported and always throws a <see cref="T:System.NotSupportedException" />.</summary>
    /// <param name="value">The length of the stream.</param>
    /// <exception cref="T:System.NotSupportedException">This property is not supported on this stream.</exception>
    public override void SetLength(long value) => throw new NotSupportedException("NotSupported");

    /// <summary>Reads a number of decompressed bytes into the specified byte array.</summary>
    /// <param name="array">The array to store decompressed bytes.</param>
    /// <param name="offset">The byte offset in <paramref name="array" /> at which the read bytes will be placed.</param>
    /// <param name="count">The maximum number of decompressed bytes to read.</param>
    /// <returns>The number of bytes that were read into the byte array.</returns>
    /// <exception cref="T:System.ArgumentNullException">
    /// <paramref name="array" /> is <see langword="null" />.</exception>
    /// <exception cref="T:System.InvalidOperationException">The <see cref="T:System.IO.Compression.CompressionMode" /> value was <see langword="Compress" /> when the object was created.
    /// -or-
    ///  The underlying stream does not support reading.</exception>
    /// <exception cref="T:System.ArgumentOutOfRangeException">
    ///         <paramref name="offset" /> or <paramref name="count" /> is less than zero.
    /// -or-
    /// <paramref name="array" /> length minus the index starting point is less than <paramref name="count" />.</exception>
    /// <exception cref="T:System.IO.InvalidDataException">The data is in an invalid format.</exception>
    /// <exception cref="T:System.ObjectDisposedException">The stream is closed.</exception>
    public override int Read(byte[] array, int offset, int count)
    {
        ValidateParameters(array, offset, count);
        EnsureNotDisposed();
        int offset1 = offset;
        int length1 = count;
        while (true)
        {
            int num = _inflater.Inflate(array, offset1, length1);
            offset1 += num;
            length1 -= num;
            if (length1 != 0 && !_inflater.Finished())
            {
                int length2 = _stream.Read(buffer, 0, buffer.Length);
                if (length2 != 0)
                {
                    _inflater.SetInput(buffer, 0, length2);
                }
                else
                {
                    break;
                }
            }
            else
            {
                break;
            }
        }
        return count - length1;
    }

    private static void ValidateParameters(byte[] array, int offset, int count)
    {
        if (offset < 0) throw new ArgumentOutOfRangeException(nameof(offset));
        if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));
        if (array.Length - offset < count) throw new ArgumentException("InvalidArgumentOffsetCount");
    }

    private void EnsureNotDisposed()
    {
        if (_stream == null)
        {
            throw new ObjectDisposedException(null, "ObjectDisposed_StreamClosed");
        }
    }

    public override void Write(byte[] array, int offset, int count)
    {
    }

    /// <summary>Releases the unmanaged resources used by the <see cref="T:System.IO.Compression.DeflateStreamCustom" /> and optionally releases the managed resources.</summary>
    /// <param name="disposing">
    /// <see langword="true" /> to release both managed and unmanaged resources; <see langword="false" /> to release only unmanaged resources.</param>
    protected override void Dispose(bool disposing)
    {
        try
        {
            if (disposing)
            {
                if (!_leaveOpen)
                {
                    if (_stream != null!)
                    {
                        _stream.Dispose();
                    }
                }
            }
        }
        finally
        {
            _stream = null!;
            try
            {
                if (_inflater != null!)
                {
                    _inflater.Dispose();
                }
            }
            finally
            {
                _inflater = null!;
                base.Dispose(disposing);
            }
        }
    }
}
