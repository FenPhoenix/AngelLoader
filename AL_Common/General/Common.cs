﻿global using static AL_Common.FullyGlobal;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using JetBrains.Annotations;

namespace AL_Common;

[PublicAPI]
public static partial class Common
{
    public const int MAX_PATH = 260;

    #region Fields / classes

    public sealed class ProgressPercents
    {
        public int MainPercent;
        public int SubPercent;
    }

    // Class instead of enum so we don't have to keep casting its fields
    public static class ByteSize
    {
        public const int KB = 1024;
        public const int MB = KB * 1024;
        public const int GB = MB * 1024;
    }

    public static class ByteLengths
    {
        public const int Byte = 1;
        public const int Int16 = 2;
        public const int Int32 = 4;
        public const int Int64 = 8;
    }

    /// <summary>
    /// Stores a filename/index pair for quick lookups into a zip file.
    /// </summary>
    [StructLayout(LayoutKind.Auto)]
    public readonly struct NameAndIndex
    {
        public readonly string Name;
        public readonly int Index;

        public NameAndIndex(string name, int index)
        {
            Name = name;
            Index = index;
        }

        public NameAndIndex(string name)
        {
            Name = name;
            Index = -1;
        }
    }

    // It's supposed to always be "{\rtf1", but some files have no number or some other number...
    // RichTextBox also only checks for "{\rtf", so we're fine here.
    public static readonly byte[] RTFHeaderBytes = @"{\rtf"u8.ToArray();

    public static readonly byte[] MAPPARAM = "MAPPARAM"u8.ToArray();

    #region Preset char arrays

    // Perf, for passing to Split(), Trim() etc. so we don't allocate all the time
    public static readonly string[] SA_CRLF = { "\r\n" };
    public static readonly char[] CA_Comma = { ',' };
    public static readonly char[] CA_Colon = { ':' };
    public static readonly char[] CA_Semicolon = { ';' };
    public static readonly char[] CA_CommaSemicolon = { ',', ';' };
    public static readonly char[] CA_CommaSpace = { ',', ' ' };
    public static readonly char[] CA_Backslash = { '\\' };
    public static readonly char[] CA_BS_FS = { '\\', '/' };
    public static readonly char[] CA_Plus = { '+' };
    public static readonly char[] CA_DoubleQuote = { '\"' };

    #endregion

    public const RegexOptions IgnoreCaseInvariant = RegexOptions.IgnoreCase | RegexOptions.CultureInvariant;

    // @MT_TASK_NOTE: This is rented/returned in tight loops a lot, we should back it out to reduce call count
    public sealed class FixedLengthByteArrayPool
    {
        private readonly int _length;
        private readonly ConcurrentBag<byte[]> _items = new();

        public FixedLengthByteArrayPool(int length) => _length = length;

        public byte[] Rent() => _items.TryTake(out byte[] item) ? item : new byte[_length];

        public void Return(byte[] item) => _items.Add(item);
    }

    public sealed class IOBufferPools
    {
        public readonly FixedLengthByteArrayPool StreamCopy = new(StreamCopyBufferSize);
        public readonly FixedLengthByteArrayPool FileStream = new(FileStreamBufferSize);
    }

    #endregion

    #region Methods

    public static bool EqualsIfNotNull(this object? obj1, object? obj2) => obj1 != null && obj2 != null && obj1 == obj2;

    /// <summary>
    /// Copy of .NET 7 version (fewer branches than Framework) but with a fast null return on fail instead of the infernal exception-throwing.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ListFast<char>? ConvertFromUtf32(uint utf32u, ListFast<char> charBuffer)
    {
        if (((utf32u - 0x110000u) ^ 0xD800u) < 0xFFEF0800u)
        {
            return null;
        }

        if (utf32u <= char.MaxValue)
        {
            charBuffer.ItemsArray[0] = (char)utf32u;
            charBuffer.Count = 1;
            return charBuffer;
        }

        charBuffer.ItemsArray[0] = (char)((utf32u + ((0xD800u - 0x40u) << 10)) >> 10);
        charBuffer.ItemsArray[1] = (char)((utf32u & 0x3FFu) + 0xDC00u);
        charBuffer.Count = 2;

        return charBuffer;
    }

    /// <summary>
    /// Shorthand for <paramref name="value"/>.ToString(<see cref="NumberFormatInfo.InvariantInfo"/>)
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ToStrInv(this int value) => value.ToString(NumberFormatInfo.InvariantInfo);

    /// <summary>
    /// Shorthand for <paramref name="value"/>.ToString(<see cref="NumberFormatInfo.InvariantInfo"/>)
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ToStrInv(this byte value) => value.ToString(NumberFormatInfo.InvariantInfo);

    /// <summary>
    /// Shorthand for <paramref name="value"/>.ToString(<see cref="NumberFormatInfo.CurrentInfo"/>)
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ToStrCur(this int value) => value.ToString(NumberFormatInfo.CurrentInfo);

    /// <summary>
    /// Shorthand for <paramref name="value"/>.ToString(<see cref="NumberFormatInfo.CurrentInfo"/>)
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ToStrCur(this double value) => value.ToString(NumberFormatInfo.CurrentInfo);

    public static string GetPlainInnerText(this XmlNode? node) => node == null ? "" : WebUtility.HtmlDecode(node.InnerText);

    #region Zip safety

    private static string GetZipSafetyFailMessage(string fileName, string full) =>
        $"Extracting this file would result in it being outside the intended folder (malformed/malicious filename?).{NL}" +
        "Entry full file name: " + fileName + $"{NL}" +
        "Path where it wanted to end up: " + full;

    // @ZipSafety: Make sure all calls to this method are handling the possible exception here! (looking at you, FMBackupAndRestore)
    // @ZipSafety: The possibility of forgetting to call this method is a problem. Architect it to reduce the likelihood somehow?
    /// <summary>
    /// Zip Slip prevention.
    /// </summary>
    /// <param name="path"></param>
    /// <param name="fileName"></param>
    /// <returns></returns>
    /// <exception cref="IOException"></exception>
    public static string GetExtractedNameOrThrowIfMalicious(string path, string fileName)
    {
        // Path.GetFullPath() incurs a very small perf hit (60ms on a 26 second extract), so don't worry about it.
        // This is basically what ZipFileExtensions.ExtractToDirectory() does.

        if (path.Length > 0 && !path[^1].IsDirSep())
        {
            path += "\\";
        }

        string extractedName = Path.Combine(path, fileName);
        string full = Path.GetFullPath(extractedName);

        if (full.PathStartsWithI(path))
        {
            return full;
        }
        else
        {
            ThrowHelper.IOException(GetZipSafetyFailMessage(fileName, full));
            return "";
        }
    }

    /// <summary>
    /// Zip Slip prevention. For when you just want to ignore it and not extract the file, rather than fail the
    /// whole operation.
    /// </summary>
    /// <param name="path"></param>
    /// <param name="fileName"></param>
    /// <param name="result"></param>
    /// <returns></returns>
    public static bool TryGetExtractedNameOrFailIfMalicious(string path, string fileName, out string result)
    {
        // Path.GetFullPath() incurs a very small perf hit (60ms on a 26 second extract), so don't worry about it.
        // This is basically what ZipFileExtensions.ExtractToDirectory() does.

        try
        {
            if (path.Length > 0 && !path[^1].IsDirSep())
            {
                path += "\\";
            }

            string extractedName = Path.Combine(path, fileName);
            string full = Path.GetFullPath(extractedName);

            if (full.PathStartsWithI(path))
            {
                result = extractedName;
                return true;
            }
            else
            {
                Logger.Log(GetZipSafetyFailMessage(fileName, full), stackTrace: true);
                result = "";
                return false;
            }
        }
        catch
        {
            result = "";
            return false;
        }
    }

    public static Encoding GetOEMCodePageOrFallback(Encoding fallback)
    {
        Encoding enc;
        try
        {
            enc = Encoding.GetEncoding(CultureInfo.CurrentCulture.TextInfo.OEMCodePage);
        }
        catch
        {
            enc = fallback;
        }

        return enc;
    }

    #endregion

    public readonly struct ParallelForData<T>
    {
        public readonly ConcurrentQueue<T> CQ;
        public readonly ParallelOptions PO;

        public ParallelForData(ConcurrentQueue<T> cq, ParallelOptions po)
        {
            CQ = cq;
            PO = po;
        }

        public ParallelForData()
        {
            CQ = new ConcurrentQueue<T>();
            PO = new ParallelOptions();
        }
    }

    public static bool TryGetParallelForData<T>(
        int threadCount,
        IEnumerable<T> items,
        CancellationToken cancellationToken,
        out ParallelForData<T> pd)
    {
        if (threadCount is 0 or < -1)
        {
            pd = new ParallelForData<T>();
            return false;
        }

        try
        {
            pd = new ParallelForData<T>(
                cq: new ConcurrentQueue<T>(items),
                po: new ParallelOptions
                {
                    CancellationToken = cancellationToken,
                    MaxDegreeOfParallelism = threadCount,
                }
            );

            return true;
        }
        catch
        {
            pd = new ParallelForData<T>();
            return false;
        }
    }

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

    #region Path.GetRandomFileName() from modern .NET

    private static ReadOnlySpan<byte> Base32Char => "abcdefghijklmnopqrstuvwxyz012345"u8;

    /// <summary>
    /// Returns a cryptographically strong random 8.3 string that can be
    /// used as either a folder name or a file name.
    /// </summary>
    public static unsafe string Path_GetRandomFileName()
    {
        // For generating random file names
        // 8 random bytes provides 12 chars in our encoding for the 8.3 name.
        const int KeyLength = 8;

        byte* pKey = stackalloc byte[KeyLength];
        Interop.GetRandomBytes(pKey, KeyLength);

        Span<char> chars = stackalloc char[12];

        byte b0 = pKey[0];
        byte b1 = pKey[1];
        byte b2 = pKey[2];
        byte b3 = pKey[3];
        byte b4 = pKey[4];

        // write to chars[11] first in order to eliminate redundant bounds checks
        chars[11] = (char)Base32Char[pKey[7] & 0x1F];

        // Consume the 5 Least significant bits of the first 5 bytes
        chars[0] = (char)Base32Char[b0 & 0x1F];
        chars[1] = (char)Base32Char[b1 & 0x1F];
        chars[2] = (char)Base32Char[b2 & 0x1F];
        chars[3] = (char)Base32Char[b3 & 0x1F];
        chars[4] = (char)Base32Char[b4 & 0x1F];

        // Consume 3 MSB of b0, b1, MSB bits 6, 7 of b3, b4
        chars[5] = (char)Base32Char[
            ((b0 & 0xE0) >> 5) |
            ((b3 & 0x60) >> 2)];

        chars[6] = (char)Base32Char[
            ((b1 & 0xE0) >> 5) |
            ((b4 & 0x60) >> 2)];

        // Consume 3 MSB bits of b2, 1 MSB bit of b3, b4
        b2 >>= 5;

        Debug.Assert((b2 & 0xF8) == 0, "Unexpected set bits");

        if ((b3 & 0x80) != 0)
            b2 |= 0x08;
        if ((b4 & 0x80) != 0)
            b2 |= 0x10;

        chars[7] = (char)Base32Char[b2];

        // Set the file extension separator
        chars[8] = '.';

        // Consume the 5 Least significant bits of the remaining 3 bytes
        chars[9] = (char)Base32Char[pKey[5] & 0x1F];
        chars[10] = (char)Base32Char[pKey[6] & 0x1F];

        return chars.ToString();
    }

    #endregion

    #endregion
}
