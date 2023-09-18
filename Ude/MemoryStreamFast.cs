using System;
using System.Runtime.CompilerServices;
using AL_Common;

namespace Ude.NetStandard;

public sealed class MemoryStreamFast
{
    public byte[] Buffer;

    /// <summary>Do not modify!</summary>
    private int _position;

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

    private bool EnsureCapacity(int value)
    {
        if (value > _capacity)
        {
            int newCapacity = value;
            if (newCapacity < 256) newCapacity = 256;
            if (newCapacity < _capacity * 2) newCapacity = _capacity * 2;

            if ((uint)(_capacity * 2) > 2147483591U)
            {
                newCapacity = value > 2147483591 ? value : 2147483591;
            }

            byte[] newBuffer = new byte[newCapacity];
            if (Length > 0)
            {
                System.Buffer.BlockCopy(Buffer, 0, newBuffer, 0, Length);
            }
            Buffer = newBuffer;
            _capacity = newCapacity;

            return true;
        }

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void ResetToCapacity(int capacity)
    {
        EnsureCapacity(capacity);
        Length = 0;
        _position = 0;
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

        int i = _position + count;
        if (i < 0)
        {
            ThrowHelper.IOException("StreamTooLong");
        }

        if (i > Length)
        {
            bool mustZero = _position > Length;
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
                Buffer[_position + byteCount] = buffer[offset + byteCount];
            }
        }
        else
        {
            System.Buffer.BlockCopy(buffer, offset, Buffer, _position, count);
        }
        _position = i;
    }

    /// <summary>Writes a byte to the current stream at the current position.</summary>
    /// <param name="value">The byte to write.</param>
    /// <exception cref="T:System.NotSupportedException">The stream does not support writing. For additional information see <see cref="P:System.IO.Stream.CanWrite" />.
    /// -or-
    /// The current position is at the end of the stream, and the capacity cannot be modified.</exception>
    /// <exception cref="T:System.ObjectDisposedException">The current stream is closed.</exception>
    internal void WriteByte(byte value)
    {
        if (_position >= Length)
        {
            int newLength = _position + 1;
            bool mustZero = _position > Length;
            if (newLength >= _capacity && EnsureCapacity(newLength))
            {
                mustZero = false;
            }
            if (mustZero)
            {
                Array.Clear(Buffer, Length, _position - Length);
            }
            Length = newLength;
        }
        Buffer[_position++] = value;
    }

    internal byte this[int index] => Buffer[index];
}
