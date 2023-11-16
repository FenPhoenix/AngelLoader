using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;

namespace SharpCompress;

[CLSCompliant(false)]
public static class Utility
{
    public static ReadOnlyCollection<T> ToReadOnly<T>(this ICollection<T> items) => new(items);

    public static void SetSize(this List<byte> list, int count)
    {
        if (count > list.Count)
        {
            // Ensure the list only needs to grow once
            list.Capacity = count;
            for (var i = list.Count; i < count; i++)
            {
                list.Add(0x0);
            }
        }
        else
        {
            list.RemoveRange(count, list.Count - count);
        }
    }

    public static void ForEach<T>(this IEnumerable<T> items, Action<T> action)
    {
        foreach (var item in items)
        {
            action(item);
        }
    }

    public static void Copy(
        Array sourceArray,
        long sourceIndex,
        Array destinationArray,
        long destinationIndex,
        long length
    )
    {
        if (sourceIndex > int.MaxValue || sourceIndex < int.MinValue)
        {
            throw new ArgumentOutOfRangeException(nameof(sourceIndex));
        }

        if (destinationIndex > int.MaxValue || destinationIndex < int.MinValue)
        {
            throw new ArgumentOutOfRangeException(nameof(destinationIndex));
        }

        if (length > int.MaxValue || length < int.MinValue)
        {
            throw new ArgumentOutOfRangeException(nameof(length));
        }

        Array.Copy(
            sourceArray,
            (int)sourceIndex,
            destinationArray,
            (int)destinationIndex,
            (int)length
        );
    }

    public static IEnumerable<T> AsEnumerable<T>(this T item)
    {
        yield return item;
    }

    public static void CheckNotNull(this object obj, string name)
    {
        if (obj is null)
        {
            throw new ArgumentNullException(name);
        }
    }

    public static void CheckNotNullOrEmpty(this string obj, string name)
    {
        obj.CheckNotNull(name);
        if (obj.Length == 0)
        {
            throw new ArgumentException("String is empty.", name);
        }
    }

    public static void Skip(this Stream source, long advanceAmount)
    {
        if (source.CanSeek)
        {
            source.Position += advanceAmount;
            return;
        }

        var buffer = GetTransferByteArray();
        try
        {
            var read = 0;
            var readCount = 0;
            do
            {
                readCount = buffer.Length;
                if (readCount > advanceAmount)
                {
                    readCount = (int)advanceAmount;
                }
                read = source.Read(buffer, 0, readCount);
                if (read <= 0)
                {
                    break;
                }
                advanceAmount -= read;
                if (advanceAmount == 0)
                {
                    break;
                }
            } while (true);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    public static void Skip(this Stream source)
    {
        var buffer = GetTransferByteArray();
        try
        {
            do { } while (source.Read(buffer, 0, buffer.Length) == buffer.Length);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    public static DateTime DosDateToDateTime(ushort iDate, ushort iTime)
    {
        var year = (iDate / 512) + 1980;
        var month = iDate % 512 / 32;
        var day = iDate % 512 % 32;
        var hour = iTime / 2048;
        var minute = iTime % 2048 / 32;
        var second = iTime % 2048 % 32 * 2;

        if (iDate == ushort.MaxValue || month == 0 || day == 0)
        {
            year = 1980;
            month = 1;
            day = 1;
        }

        if (iTime == ushort.MaxValue)
        {
            hour = minute = second = 0;
        }

        DateTime dt;
        try
        {
            dt = new DateTime(year, month, day, hour, minute, second, DateTimeKind.Local);
        }
        catch
        {
            dt = new DateTime();
        }
        return dt;
    }

    public static DateTime DosDateToDateTime(uint iTime) =>
        DosDateToDateTime((ushort)(iTime / 65536), (ushort)(iTime % 65536));

    /// <summary>
    /// Convert Unix time value to a DateTime object.
    /// </summary>
    /// <param name="unixtime">The Unix time stamp you want to convert to DateTime.</param>
    /// <returns>Returns a DateTime object that represents value of the Unix time.</returns>
    public static DateTime UnixTimeToDateTime(long unixtime)
    {
        var sTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        return sTime.AddSeconds(unixtime);
    }

    public static long TransferTo(
        this Stream source,
        Stream destination,
        Common.Entry entry
    )
    {
        var array = GetTransferByteArray();
        try
        {
            long total = 0;
            while (ReadTransferBlock(source, array, out var count))
            {
                total += count;
                destination.Write(array, 0, count);
            }
            return total;
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(array);
        }
    }

    private static bool ReadTransferBlock(Stream source, byte[] array, out int count) =>
        (count = source.Read(array, 0, array.Length)) != 0;

    private static byte[] GetTransferByteArray() => ArrayPool<byte>.Shared.Rent(81920);
}
