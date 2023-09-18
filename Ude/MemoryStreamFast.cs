using System.Runtime.CompilerServices;

namespace Ude.NetStandard;

public sealed class MemoryStreamFast
{
    public byte[] Buffer;

    private int _position;

    /// <summary>Do not modify!</summary>
    public int Length;

    private int _capacity;

    public MemoryStreamFast(int capacity)
    {
        Buffer = new byte[capacity];
        _capacity = capacity;
    }

    private void EnsureCapacity(int value)
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
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void ResetToCapacity(int capacity)
    {
        EnsureCapacity(capacity);
        Length = 0;
        _position = 0;
    }

    internal void Write(byte[] buffer, int offset, int count)
    {
        int i = _position + count;
        if (i > Length)
        {
            if (i > _capacity) EnsureCapacity(i);
            Length = i;
        }
        if (count <= 8 && buffer != Buffer)
        {
            while (--count >= 0)
            {
                Buffer[_position + count] = buffer[offset + count];
            }
        }
        else
        {
            System.Buffer.BlockCopy(buffer, offset, Buffer, _position, count);
        }
        _position = i;
    }

    internal void WriteByte(byte value)
    {
        if (_position >= Length)
        {
            int newLength = _position + 1;
            if (newLength >= _capacity) EnsureCapacity(newLength);
            Length = newLength;
        }
        Buffer[_position++] = value;
    }
}
