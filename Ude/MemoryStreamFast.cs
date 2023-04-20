using System;
using AL_Common;

namespace Ude.NetStandard;

public sealed class MemoryStreamFast
{
    private byte[] _buffer;
    private readonly int _origin;
    private int _position;
    private int _length;
    private int _capacity;

    public MemoryStreamFast(int capacity)
    {
        if (capacity < 0)
        {
            ThrowHelper.ArgumentOutOfRange(nameof(capacity), "NegativeCapacity");
        }

        _buffer = new byte[capacity];

        _capacity = capacity;
        _origin = 0;
    }

    private bool EnsureCapacity(int value)
    {
        if (value < 0)
        {
            ThrowHelper.IOException("StreamTooLong");
        }
        if (value <= _capacity)
        {
            return false;
        }
        int num = value;
        if (num < 256)
        {
            num = 256;
        }
        if (num < _capacity * 2)
        {
            num = _capacity * 2;
        }
        if ((uint)(_capacity * 2) > 2147483591U)
        {
            num = value > 2147483591 ? value : 2147483591;
        }
        Capacity = num;
        return true;
    }

#if false

    /// <summary>Returns the array of unsigned bytes from which this stream was created.</summary>
    /// <returns>The byte array from which this stream was created, or the underlying array if a byte array was not provided to the <see cref="T:System.IO.MemoryStreamFast" /> constructor during construction of the current instance.</returns>
    /// <exception cref="T:System.UnauthorizedAccessException">The <see langword="MemoryStreamFast" /> instance was not created with a publicly visible buffer.</exception>
    public byte[] GetBuffer() => _buffer;

    /// <summary>Returns the array of unsigned bytes from which this stream was created. The return value indicates whether the conversion succeeded.</summary>
    /// <param name="buffer">The byte array segment from which this stream was created.</param>
    /// <returns>
    /// <see langword="true" /> if the conversion was successful; otherwise, <see langword="false" />.</returns>
    public bool TryGetBuffer(out ArraySegment<byte> buffer)
    {
        buffer = new ArraySegment<byte>(_buffer, _origin, _length - _origin);
        return true;
    }

#endif

    /// <summary>Gets or sets the number of bytes allocated for this stream.</summary>
    /// <returns>The length of the usable portion of the buffer for the stream.</returns>
    /// <exception cref="T:System.ArgumentOutOfRangeException">A capacity is set that is negative or less than the current length of the stream.</exception>
    /// <exception cref="T:System.ObjectDisposedException">The current stream is closed.</exception>
    /// <exception cref="T:System.NotSupportedException">
    /// <see langword="set" /> is invoked on a stream whose capacity cannot be modified.</exception>
    public int Capacity
    {
#if false
        get => _capacity - _origin;
#endif
        set
        {
            if (value < Length)
            {
                ThrowHelper.ArgumentOutOfRange(nameof(value), "SmallCapacity");
            }
            if (value == _capacity)
            {
                return;
            }
            if (value > 0)
            {
                byte[] dst = new byte[value];
                if (_length > 0)
                {
                    Buffer.BlockCopy(_buffer, 0, dst, 0, _length);
                }
                _buffer = dst;
            }
            else
            {
                _buffer = Array.Empty<byte>();
            }
            _capacity = value;
        }
    }

    /// <summary>Gets the length of the stream in bytes.</summary>
    /// <returns>The length of the stream in bytes.</returns>
    /// <exception cref="T:System.ObjectDisposedException">The stream is closed.</exception>
    internal long Length => _length - _origin;

    /// <summary>Gets or sets the current position within the stream.</summary>
    /// <returns>The current position within the stream.</returns>
    /// <exception cref="T:System.ArgumentOutOfRangeException">The position is set to a negative value or a value greater than <see cref="F:System.Int32.MaxValue" />.</exception>
    /// <exception cref="T:System.ObjectDisposedException">The stream is closed.</exception>
    internal long Position
    {
        get => _position - _origin;
#if false
        set
        {
            if (value < 0L)
                throw new ArgumentOutOfRangeException(nameof(value), "ArgumentOutOfRange_NeedNonNegNum");
            if (value > int.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(value), "ArgumentOutOfRange_StreamLength");
            _position = _origin + (int)value;
        }
#endif
    }

    /// <summary>Sets the length of the current stream to the specified value.</summary>
    /// <param name="value">The value at which to set the length.</param>
    /// <exception cref="T:System.NotSupportedException">The current stream is not resizable and <paramref name="value" /> is larger than the current capacity.
    /// -or-
    /// The current stream does not support writing.</exception>
    /// <exception cref="T:System.ArgumentOutOfRangeException">
    /// <paramref name="value" /> is negative or is greater than the maximum length of the <see cref="T:System.IO.MemoryStreamFast" />, where the maximum length is(<see cref="F:System.Int32.MaxValue" /> - origin), and origin is the index into the underlying buffer at which the stream starts.</exception>
    internal void SetLength(long value)
    {
        if (value < 0L || value > int.MaxValue || value > int.MaxValue - _origin)
        {
            ThrowHelper.ArgumentOutOfRange(nameof(value), "StreamLength");
        }
        int num = _origin + (int)value;
        if (!EnsureCapacity(num) && num > _length)
        {
            Array.Clear(_buffer, _length, num - _length);
        }
        _length = num;
        if (_position <= num)
        {
            return;
        }
        _position = num;
    }

    /// <summary>Writes the stream contents to a byte array, regardless of the <see cref="P:System.IO.MemoryStreamFast.Position" /> property.</summary>
    /// <returns>A new byte array.</returns>
    internal byte[] ToArray()
    {
        byte[] dst = new byte[_length - _origin];
        Buffer.BlockCopy(_buffer, _origin, dst, 0, _length - _origin);
        return dst;
    }

    /// <summary>Writes a block of bytes to the current stream using data read from a buffer.</summary>
    /// <param name="buffer">The buffer to write data from.</param>
    /// <param name="offset">The zero-based byte offset in <paramref name="buffer" /> at which to begin copying bytes to the current stream.</param>
    /// <param name="count">The maximum number of bytes to write.</param>
    /// <exception cref="T:System.NotSupportedException">The stream does not support writing. For additional information see <see cref="P:System.IO.Stream.CanWrite" />.
    /// -or-
    /// The current position is closer than <paramref name="count" /> bytes to the end of the stream, and the capacity cannot be modified.</exception>
    /// <exception cref="T:System.ArgumentException">
    /// <paramref name="offset" /> subtracted from the buffer length is less than <paramref name="count" />.</exception>
    /// <exception cref="T:System.ArgumentOutOfRangeException">
    /// <paramref name="offset" /> or <paramref name="count" /> are negative.</exception>
    /// <exception cref="T:System.IO.IOException">An I/O error occurs.</exception>
    /// <exception cref="T:System.ObjectDisposedException">The current stream instance is closed.</exception>
    internal void Write(byte[] buffer, int offset, int count)
    {
        if (offset < 0)
        {
            ThrowHelper.ArgumentOutOfRange(nameof(offset), "NeedNonNegNum");
        }
        if (count < 0)
        {
            ThrowHelper.ArgumentOutOfRange(nameof(count), "NeedNonNegNum");
        }
        if (buffer.Length - offset < count)
        {
            ThrowHelper.ArgumentException("Argument_InvalidOffLen");
        }
        int num1 = _position + count;
        if (num1 < 0)
        {
            ThrowHelper.IOException("StreamTooLong");
        }
        if (num1 > _length)
        {
            bool flag = _position > _length;
            if (num1 > _capacity && EnsureCapacity(num1))
            {
                flag = false;
            }
            if (flag)
            {
                Array.Clear(_buffer, _length, num1 - _length);
            }
            _length = num1;
        }
        if (count <= 8 && buffer != _buffer)
        {
            int num2 = count;
            while (--num2 >= 0)
                _buffer[_position + num2] = buffer[offset + num2];
        }
        else
        {
            Buffer.BlockCopy(buffer, offset, _buffer, _position, count);
        }
        _position = num1;
    }

    /// <summary>Writes a byte to the current stream at the current position.</summary>
    /// <param name="value">The byte to write.</param>
    /// <exception cref="T:System.NotSupportedException">The stream does not support writing. For additional information see <see cref="P:System.IO.Stream.CanWrite" />.
    /// -or-
    /// The current position is at the end of the stream, and the capacity cannot be modified.</exception>
    /// <exception cref="T:System.ObjectDisposedException">The current stream is closed.</exception>
    internal void WriteByte(byte value)
    {
        if (_position >= _length)
        {
            int num = _position + 1;
            bool flag = _position > _length;
            if (num >= _capacity && EnsureCapacity(num))
            {
                flag = false;
            }
            if (flag)
            {
                Array.Clear(_buffer, _length, _position - _length);
            }
            _length = num;
        }
        _buffer[_position++] = value;
    }

    internal byte this[int index] => _buffer[index];
}
