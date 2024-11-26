global using static AL_Common.FullyGlobal;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
    public static readonly char[] CA_CommaSemicolon = { ',', ';' };
    public static readonly char[] CA_CommaSpace = { ',', ' ' };
    public static readonly char[] CA_BS_FS = { '\\', '/' };

    #endregion

    public const RegexOptions IgnoreCaseInvariant = RegexOptions.IgnoreCase | RegexOptions.CultureInvariant;

    // @MT_TASK_NOTE: This is rented/returned in tight loops a lot, we should back it out to reduce call count
    public sealed class FixedLengthByteArrayPool
    {
        private readonly int _length;
        private readonly ConcurrentBag<byte[]> _items = new();

        public FixedLengthByteArrayPool(int length) => _length = length;

        public byte[] Rent() => _items.TryTake(out byte[]? item) ? item : new byte[_length];

        public void Return(byte[] item) => _items.Add(item);
    }

    public sealed class IOBufferPools
    {
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

    // .NET Core changed the Encoding.Default return value from legacy ANSI to UTF8. This is disastrous for us
    // because we need to read and write certain files that are written in Framework Encoding.Default (ANSI).
    // So implement a manual version here...

    [LibraryImport("kernel32.dll")]
    private static partial int GetACP();

    public static Encoding GetLegacyDefaultEncoding()
    {
        try
        {
            // .NET Framework simply calls GetACP() for Encoding.Default, so let's try that first.
            return Encoding.GetEncoding(GetACP());
        }
        catch
        {
            try
            {
                // @NET5(Encoding.Default): I think InstalledUICulture is the actual one we want?
                // CurrentCulture will normally match it I think, but to be safe we should be explicit...
                // Unless I'm wrong, but hopefully someone will report the bug again if I am?
                return Encoding.GetEncoding(CultureInfo.InstalledUICulture.TextInfo.ANSICodePage);
            }
            catch (Exception ex)
            {
                // @NET5(Encoding.Default): If we get here, we should maybe put up a dialog, because this is a bad situation.
                // It will lead to a recurrence of the bug where cam_mod.ini is written with the wrong encoding
                // and non-ASCII game paths won't be read and FMs can't be played.
                Logger.Log(
                    $"Unable to get the system default ANSI encoding, which is required for reading and writing certain files correctly.{NL}" +
                    "Returning .NET default encoding, which will probably be UTF8 and may cause issues for locales outside North America.",
                    ex);
                return Encoding.Default;
            }
        }
    }

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

    #endregion
}
