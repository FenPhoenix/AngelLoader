using System;

namespace AL_Common;

public static partial class Common
{
    public static ArraySegment<T> Slice<T>(this ArraySegment<T> arraySegment, int index)
    {
        arraySegment.ArraySegment_ThrowInvalidOperationIfDefault();

        if ((uint)index > (uint)arraySegment.Count)
        {
            ThrowHelper.ThrowArgumentOutOfRange_IndexMustBeLessOrEqualException(nameof(index));
        }

        return new ArraySegment<T>(arraySegment.Array!, arraySegment.Offset + index, arraySegment.Count - index);
    }

    public static ArraySegment<T> Slice<T>(this ArraySegment<T> arraySegment, int index, int count)
    {
        arraySegment.ArraySegment_ThrowInvalidOperationIfDefault();

        if ((uint)index > (uint)arraySegment.Count || (uint)count > (uint)(arraySegment.Count - index))
        {
            ThrowHelper.ThrowArgumentOutOfRange_IndexMustBeLessOrEqualException(nameof(index));
        }

        return new ArraySegment<T>(arraySegment.Array!, arraySegment.Offset + index, count);
    }

    private static void ArraySegment_ThrowInvalidOperationIfDefault<T>(this ArraySegment<T> arraySegment)
    {
        if (arraySegment.Array == null)
        {
            ThrowHelper.ThrowInvalidOperationException(SR.InvalidOperation_NullArray);
        }
    }
}
