using System;
using AL_Common;

namespace Ude.NetStandard;

public sealed class MemoryStreamFast
{
    public byte[] Buffer;

    /// <summary>Do not modify!</summary>
    public int Position;

    /// <summary>Do not modify!</summary>
    public int Length;

    private int _capacity;

    public MemoryStreamFast(int capacity)
    {
        if (capacity < 0)
        {
            ThrowHelper.ArgumentOutOfRange(nameof(capacity), "NegativeCapacity");
        }

        Buffer = new byte[capacity];

        _capacity = capacity;
    }

    public bool EnsureCapacity(int value)
    {
        if (value < 0) ThrowHelper.IOException("StreamTooLong");

        if (value > _capacity)
        {
            int newCapacity = value;
            if (newCapacity < 256) newCapacity = 256;
            if (newCapacity < _capacity * 2) newCapacity = _capacity * 2;

            if ((uint)(_capacity * 2) > 2147483591U)
            {
                newCapacity = value > 2147483591 ? value : 2147483591;
            }

            Capacity = newCapacity;

            return true;
        }

        return false;
    }

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
                byte[] newBuffer = new byte[value];
                if (Length > 0)
                {
                    System.Buffer.BlockCopy(Buffer, 0, newBuffer, 0, Length);
                }
                Buffer = newBuffer;
            }
            else
            {
                Buffer = Array.Empty<byte>();
            }
            _capacity = value;
        }
    }

    /// <summary>Sets the length of the current stream to the specified value.</summary>
    /// <param name="value">The value at which to set the length.</param>
    /// <exception cref="T:System.NotSupportedException">The current stream is not resizable and <paramref name="value" /> is larger than the current capacity.
    /// -or-
    /// The current stream does not support writing.</exception>
    /// <exception cref="T:System.ArgumentOutOfRangeException">
    /// <paramref name="value" /> is negative or is greater than the maximum length of the <see cref="T:System.IO.MemoryStreamFast" />, where the maximum length is(<see cref="F:System.Int32.MaxValue" /> - origin), and origin is the index into the underlying buffer at which the stream starts.</exception>
    internal void SetLength(int value)
    {
        if (value < 0)
        {
            ThrowHelper.ArgumentOutOfRange(nameof(value), "StreamLength");
        }
        if (!EnsureCapacity(value) && value > Length)
        {
            Array.Clear(Buffer, Length, value - Length);
        }
        Length = value;
        if (Position <= value)
        {
            return;
        }
        Position = value;
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

        int i = Position + count;
        if (i < 0)
        {
            ThrowHelper.IOException("StreamTooLong");
        }

        if (i > Length)
        {
            bool mustZero = Position > Length;
            if (i > _capacity && EnsureCapacity(i))
            {
                mustZero = false;
            }
            if (mustZero)
            {
                Array.Clear(Buffer, Length, i - Length);
            }
            Length = i;
        }
        if (count <= 8 && buffer != Buffer)
        {
            int byteCount = count;
            while (--byteCount >= 0)
            {
                Buffer[Position + byteCount] = buffer[offset + byteCount];
            }
        }
        else
        {
            System.Buffer.BlockCopy(buffer, offset, Buffer, Position, count);
        }
        Position = i;
    }

    /// <summary>Writes a byte to the current stream at the current position.</summary>
    /// <param name="value">The byte to write.</param>
    /// <exception cref="T:System.NotSupportedException">The stream does not support writing. For additional information see <see cref="P:System.IO.Stream.CanWrite" />.
    /// -or-
    /// The current position is at the end of the stream, and the capacity cannot be modified.</exception>
    /// <exception cref="T:System.ObjectDisposedException">The current stream is closed.</exception>
    internal void WriteByte(byte value)
    {
        if (Position >= Length)
        {
            int newLength = Position + 1;
            bool mustZero = Position > Length;
            if (newLength >= _capacity && EnsureCapacity(newLength))
            {
                mustZero = false;
            }
            if (mustZero)
            {
                Array.Clear(Buffer, Length, Position - Length);
            }
            Length = newLength;
        }
        Buffer[Position++] = value;
    }

    internal byte this[int index] => Buffer[index];
}
