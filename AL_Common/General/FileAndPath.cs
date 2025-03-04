using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using static System.StringComparison;

namespace AL_Common;

public static partial class Common
{
    #region Forward/backslash conversion

    public static string ToForwardSlashes(this string value) => value.Replace('\\', '/');

    public static string ToForwardSlashes_Net(this string value)
    {
        return value.StartsWithO(@"\\") ? @"\\" + value.Substring(2).ToForwardSlashes() : value.ToForwardSlashes();
    }

    public static string ToBackSlashes(this string value) => value.Replace('/', '\\');

    public static string ToSystemDirSeps(this string value) => value.Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);

    public static string ToSystemDirSeps_Net(this string value)
    {
        return value.StartsWithO(@"\\") ? @"\\" + value.Substring(2).ToSystemDirSeps() : value.ToSystemDirSeps();
    }

    public static string MakeUNCPath(string path) => path.StartsWithO(@"\\") ? string.Concat(@"\\?\UNC\", path.AsSpan(2)) : @"\\?\" + path;

    #endregion

    #region ReadAllLines

    // Return the original lists to avoid the wasteful and useless allocation of the array conversion that
    // you get with the built-in methods
    public static List<string> File_ReadAllLines_List(string path)
    {
        List<string> ret = new();
        using FileStream_Read_WithRentedBuffer fs = new(path);
        using StreamReaderCustom.SRC_Wrapper sr = new(fs.FileStream, new StreamReaderCustom());
        while (sr.Reader.ReadLine() is { } str)
        {
            ret.Add(str);
        }
        return ret;
    }

    public static List<string> File_ReadAllLines_List(string path, Encoding encoding, bool detectEncodingFromByteOrderMarks)
    {
        List<string> ret = new();
        using FileStream_Read_WithRentedBuffer fs = new(path);
        using StreamReaderCustom.SRC_Wrapper sr = new(fs.FileStream, encoding, detectEncodingFromByteOrderMarks, new StreamReaderCustom());
        while (sr.Reader.ReadLine() is { } str)
        {
            ret.Add(str);
        }
        return ret;
    }

    public static StreamReaderCustom.SRC_Wrapper File_OpenTextFast(string path, int bufferSize)
    {
        return new StreamReaderCustom.SRC_Wrapper(File_OpenReadFast(path, bufferSize), new StreamReaderCustom());
    }

    [StructLayout(LayoutKind.Auto)]
    public readonly ref struct FileStream_Read_WithRentedBuffer
    {
        public readonly FileStream FileStream;
        private readonly byte[] Buffer;

        public FileStream_Read_WithRentedBuffer(string path, int bufferSize = FileStreamBufferSize)
        {
            Buffer = ArrayPool<byte>.Shared.Rent(bufferSize);
            FileStream = GetReadModeFileStreamWithCachedBuffer(path, Buffer, bufferSize);
        }

        public void Dispose()
        {
            FileStream.Dispose();
            ArrayPool<byte>.Shared.Return(Buffer);
        }
    }

    [StructLayout(LayoutKind.Auto)]
    public readonly ref struct FileStream_Write_WithRentedBuffer
    {
        public readonly FileStream FileStream;
        private readonly byte[] Buffer;

        public FileStream_Write_WithRentedBuffer(string path, bool overwrite = true, int bufferSize = FileStreamBufferSize)
        {
            Buffer = ArrayPool<byte>.Shared.Rent(bufferSize);
            FileStream = GetWriteModeFileStreamWithCachedBuffer(path, overwrite, Buffer, bufferSize);
        }

        public void Dispose()
        {
            FileStream.Dispose();
            ArrayPool<byte>.Shared.Return(Buffer);
        }
    }

    public sealed class FileStream_Read_WithRentedBuffer_Ref : IDisposable
    {
        public readonly FileStream FileStream;
        private readonly byte[] Buffer;

        public FileStream_Read_WithRentedBuffer_Ref(string path)
        {
            Buffer = ArrayPool<byte>.Shared.Rent(FileStreamBufferSize);
            FileStream = GetReadModeFileStreamWithCachedBuffer(path, Buffer);
        }

        public void Dispose()
        {
            FileStream.Dispose();
            ArrayPool<byte>.Shared.Return(Buffer);
        }
    }

    public static FileStream File_OpenReadFast(string path, int bufferSize)
    {
        return new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize);
    }

    #endregion

    #region Path-specific string queries (separator-agnostic)

    public static bool PathContainsI(this List<string> value, string substring)
    {
        for (int i = 0; i < value.Count; i++)
        {
            if (value[i].PathEqualsI(substring)) return true;
        }
        return false;
    }

    #region Disabled until needed

#if false
    public static bool PathContainsI(this string[] value, string substring)
    {
        for (int i = 0; i < value.Length; i++)
        {
            if (value[i].PathEqualsI(substring)) return true;
        }
        return false;
    }
#endif

    #endregion

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsDirSep(this char character) => character is '/' or '\\';

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool StartsWithDirSep(this string value) => value.Length > 0 && value[0].IsDirSep();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool EndsWithDirSep(this string value) => value.Length > 0 && value[^1].IsDirSep();

    // Note: We hardcode '/' and '\' for now because we can get paths from archive files too, where the dir
    // sep chars are in no way guaranteed to match those of the OS.
    // Not like any OS is likely to use anything other than '/' or '\' anyway.

    // We hope not to have to call this too often, but it's here as a fallback.
    public static string CanonicalizePath(string value) => value.ToBackSlashes();

    /// <summary>
    /// Returns true if <paramref name="value"/> contains either directory separator character.
    /// <para>Do NOT use for full (non-relative) paths as it will count the "\\" at the start of UNC paths! </para>
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static bool Rel_ContainsDirSep(this string value)
    {
        for (int i = 0; i < value.Length; i++)
        {
            if (value[i].IsDirSep()) return true;
        }
        return false;
    }

    /// <summary>
    /// Counts the total occurrences of both directory separator characters in <paramref name="value"/>.
    /// <para>Do NOT use for full (non-relative) paths as it will count the "\\" at the start of UNC paths! </para>
    /// </summary>
    /// <param name="value"></param>
    /// <param name="start"></param>
    /// <returns></returns>
    public static int Rel_CountDirSeps(this string value, int start = 0)
    {
        int count = 0;
        for (int i = start; i < value.Length; i++)
        {
            if (value[i].IsDirSep()) count++;
        }
        return count;
    }

    /// <summary>
    /// <para>Do NOT use for full (non-relative) paths as it will count the "\\" at the start of UNC paths! </para>
    /// </summary>
    /// <param name="value"></param>
    /// <param name="count"></param>
    /// <param name="start"></param>
    /// <returns></returns>
    public static bool Rel_DirSepCountIsAtLeast(this string value, int count, int start = 0)
    {
        int foundCount = 0;
        for (int i = start; i < value.Length; i++)
        {
            if (value[i].IsDirSep()) foundCount++;
            if (foundCount == count) return true;
        }

        return false;
    }

    /// <summary>
    /// Returns the number of directory separators in a string, earlying-out once it's counted <paramref name="maxToCount"/>
    /// occurrences.
    /// <para>Do NOT use for full (non-relative) paths as it will count the "\\" at the start of UNC paths! </para>
    /// </summary>
    /// <param name="value"></param>
    /// <param name="maxToCount">The maximum number of occurrences to count before earlying-out.</param>
    /// <returns></returns>
    public static int Rel_CountDirSepsUpToAmount(this string value, int maxToCount)
    {
        int foundCount = 0;
        for (int i = 0; i < value.Length; i++)
        {
            if (value[i].IsDirSep())
            {
                foundCount++;
                if (foundCount == maxToCount) break;
            }
        }

        return foundCount;
    }

    /// <summary>
    /// Returns the last index of either directory separator character in <paramref name="value"/>.
    /// <para>Do NOT use for full (non-relative) paths as it will count the "\\" at the start of UNC paths! </para>
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static int Rel_LastIndexOfDirSep(this string value) => value.AsSpan().LastIndexOfAny('/', '\\');

    /// <summary>
    /// Path equality check ignoring case and directory separator differences.
    /// </summary>
    /// <param name="first"></param>
    /// <param name="second"></param>
    /// <returns></returns>
    public static bool PathEqualsI(this string first, string second)
    {
        if (first == second) return true;

        int firstLen = first.Length;
        if (firstLen != second.Length) return false;

        for (int i = 0; i < firstLen; i++)
        {
            char fc = first[i];
            char sc = second[i];

            if (!BothAreAscii(fc, sc))
            {
                return first.Equals(second, OrdinalIgnoreCase) ||
                       CanonicalizePath(first).Equals(CanonicalizePath(second), OrdinalIgnoreCase);
            }

            if (!AsciiPathCharsConsideredEqual_Win(fc, sc)) return false;
        }

        return true;
    }

    /// <summary>
    /// Path equality check ignoring case and directory separator differences. Only use if <paramref name="second"/> is ASCII.
    /// </summary>
    /// <param name="first"></param>
    /// <param name="second"></param>
    /// <returns></returns>
    public static bool PathEqualsI_AsciiSecond(this string first, string second)
    {
        if (first == second) return true;

        int firstLen = first.Length;
        if (firstLen != second.Length) return false;

        for (int i = 0; i < firstLen; i++)
        {
            char fc = first[i];
            char sc = second[i];

            if (!AsciiPathCharsConsideredEqual_Win(fc, sc)) return false;
        }

        return true;
    }

    /// <summary>
    /// Path starts-with check ignoring case and directory separator differences.
    /// </summary>
    /// <param name="first"></param>
    /// <param name="second"></param>
    /// <returns></returns>
    public static bool PathStartsWithI(this string first, string second)
    {
        if (first.Length < second.Length) return false;

        for (int i = 0; i < second.Length; i++)
        {
            char fc = first[i];
            char sc = second[i];

            if (!BothAreAscii(fc, sc))
            {
                return first.StartsWith(second, OrdinalIgnoreCase) ||
                       CanonicalizePath(first).StartsWith(CanonicalizePath(second), OrdinalIgnoreCase);
            }

            if (!AsciiPathCharsConsideredEqual_Win(fc, sc)) return false;
        }

        return true;
    }

    /// <summary>
    /// Path starts-with check ignoring case and directory separator differences. Only use if <paramref name="second"/> is ASCII.
    /// </summary>
    /// <param name="first"></param>
    /// <param name="second"></param>
    /// <returns></returns>
    public static bool PathStartsWithI_AsciiSecond(this string first, string second)
    {
        if (first.Length < second.Length) return false;

        for (int i = 0; i < second.Length; i++)
        {
            char fc = first[i];
            char sc = second[i];

            if (!AsciiPathCharsConsideredEqual_Win(fc, sc)) return false;
        }

        return true;
    }

    /// <summary>
    /// Path ends-with check ignoring case and directory separator differences.
    /// </summary>
    /// <param name="first"></param>
    /// <param name="second"></param>
    /// <returns></returns>
    public static bool PathEndsWithI(this string first, string second)
    {
        if (first.Length < second.Length) return false;

        for (int fi = first.Length - second.Length, si = 0; fi < first.Length; fi++, si++)
        {
            char fc = first[fi];
            char sc = second[si];

            if (!BothAreAscii(fc, sc))
            {
                return first.EndsWith(second, OrdinalIgnoreCase) ||
                       CanonicalizePath(first).EndsWith(CanonicalizePath(second), OrdinalIgnoreCase);
            }

            if (!AsciiPathCharsConsideredEqual_Win(fc, sc)) return false;
        }

        return true;
    }

    /// <summary>
    /// Path ends-with check ignoring case and directory separator differences. Only use if <paramref name="second"/> is ASCII.
    /// </summary>
    /// <param name="first"></param>
    /// <param name="second"></param>
    /// <returns></returns>
    public static bool PathEndsWithI_AsciiSecond(this string first, string second)
    {
        if (first.Length < second.Length) return false;

        for (int fi = first.Length - second.Length, si = 0; fi < first.Length; fi++, si++)
        {
            char fc = first[fi];
            char sc = second[si];

            if (!AsciiPathCharsConsideredEqual_Win(fc, sc)) return false;
        }

        return true;
    }

    #region Disabled until needed

#if false
    public static bool PathContainsI_Dir(this List<string> value, string substring)
    {
        for (int i = 0; i < value.Count; i++)
        {
            if (value[i].PathEqualsI_Dir(substring)) return true;
        }
        return false;
    }

    public static bool PathContainsI_Dir(this string[] value, string substring)
    {
        for (int i = 0; i < value.Length; i++)
        {
            if (value[i].PathEqualsI_Dir(substring)) return true;
        }
        return false;
    }

    /// <summary>
    /// Counts the total occurrences of both directory separator characters in <paramref name="value"/>.
    /// </summary>
    /// <param name="value"></param>
    /// <param name="start"></param>
    /// <returns></returns>
    public static int CountDirSeps(this string value, int start = 0)
    {
        int count = 0;
        for (int i = start; i < value.Length; i++)
        {
            if (value[i].IsDirSep()) count++;
        }
        return count;
    }

    /// <summary>
    /// Counts dir seps up to <paramref name="count"/> occurrences and then returns, skipping further counting.
    /// </summary>
    /// <param name="value"></param>
    /// <param name="count"></param>
    /// <param name="start"></param>
    /// <returns></returns>
    internal static bool DirSepCountIsAtLeast(this string value, int count, int start = 0)
    {
        int foundCount = 0;
        for (int i = start; i < value.Length; i++)
        {
            if (value[i].IsDirSep()) foundCount++;
            if (foundCount == count) return true;
        }

        return false;
    }
#endif

    #endregion

    #region Equality / StartsWith / EndsWith

    public static bool PathSequenceEqualI_Dir(this IList<string> first, IList<string> second)
    {
        int firstCount;
        if ((firstCount = first.Count) != second.Count) return false;

        for (int i = 0; i < firstCount; i++)
        {
            if (!first[i].PathEqualsI_Dir(second[i])) return false;
        }
        return true;
    }

    public static bool PathSequenceEqualI(this IList<string> first, IList<string> second)
    {
        int firstCount;
        if ((firstCount = first.Count) != second.Count) return false;

        for (int i = 0; i < firstCount; i++)
        {
            if (!first[i].PathEqualsI(second[i])) return false;
        }
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool AsciiPathCharsConsideredEqual_Win(char char1, char char2) =>
        char1.EqualsIAscii(char2) ||
        (char1.IsDirSep() && char2.IsDirSep());

    /// <summary>
    /// Path equality check ignoring case and directory separator differences. Directory version: Ignores
    /// trailing path separators.
    /// </summary>
    /// <param name="first"></param>
    /// <param name="second"></param>
    /// <returns></returns>
    public static bool PathEqualsI_Dir(this string first, string second) => first.TrimEnd(CA_BS_FS).PathEqualsI(second.TrimEnd(CA_BS_FS));

    #endregion

    #endregion

    #region Extensions

    /// <summary>
    /// Just removes the extension from a filename, without the rather large overhead of
    /// Path.GetFileNameWithoutExtension().
    /// </summary>
    /// <param name="fileName"></param>
    /// <returns></returns>
    public static string RemoveExtension(this string fileName)
    {
        int i;
        return (i = fileName.LastIndexOf('.')) == -1 ? fileName : fileName.Substring(0, i);
    }

    /// <summary>
    /// Determines whether this string ends with a file extension. Obviously only makes sense for strings
    /// that are supposed to be file names.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static bool HasFileExtension(this string value)
    {
        // Exact copy of code in .NET modern Path.HasFileExtension, but with explicit dir seps because we're
        // dealing with zip files, and we don't want "whatever the framework decides"
        for (int i = value.Length - 1; i >= 0; i--)
        {
            char ch = value[i];
            if (ch == '.')
            {
                return i != value.Length - 1;
            }
            if (ch.IsDirSep())
            {
                break;
            }
        }
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool ExtIsTxt(this string value) => value.EndsWithI(".txt");

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool ExtIsRtf(this string value) => value.EndsWithI(".rtf");

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool ExtIsWri(this string value) => value.EndsWithI(".wri");

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool ExtIsHtml(this string value) => value.EndsWithI(".html") || value.EndsWithI(".htm");

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool ExtIsGlml(this string value) => value.EndsWithI(".glml");

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool ExtIsZip(this string value) => value.EndsWithI(".zip");

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool ExtIs7z(this string value) => value.EndsWithI(".7z");

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool ExtIsRar(this string value) => value.EndsWithI(".rar");

    #endregion

    #region Set file attributes

    public static void File_UnSetReadOnly(string fileOnDiskFullPath, bool throwException = false)
    {
        try
        {
            new FileInfo(fileOnDiskFullPath).IsReadOnly = false;
        }
        catch (Exception ex)
        {
            Logger.Log("Unable to set file attributes for " + fileOnDiskFullPath, ex);
            if (throwException) throw;
        }
    }

    public static void Dir_UnSetReadOnly(string dirOnDiskFullPath, bool throwException = false)
    {
        try
        {
            // IMPORTANT: ReadOnly is NOT ignored for directories despite what it says here, as I learned to my cost:
            // https://support.microsoft.com/en-us/topic/you-cannot-view-or-change-the-read-only-or-the-system-attributes-of-folders-in-windows-server-2003-in-windows-xp-in-windows-vista-or-in-windows-7-55bd5ec5-d19e-6173-0df1-8f5b49247165
            _ = new DirectoryInfo(dirOnDiskFullPath).Attributes &= ~FileAttributes.ReadOnly;
        }
        catch (Exception ex)
        {
            Logger.Log("Unable to set directory attributes for " + dirOnDiskFullPath, ex);
            if (throwException) throw;
        }
    }

    public static void DirAndFileTree_UnSetReadOnly(string path, bool throwException = false)
    {
        Dir_UnSetReadOnly(path, throwException);

        foreach (string f in Directory.GetFiles(path, "*", SearchOption.AllDirectories))
        {
            File_UnSetReadOnly(f, throwException);
        }

        foreach (string d in Directory.GetDirectories(path, "*", SearchOption.AllDirectories))
        {
            Dir_UnSetReadOnly(d, throwException);
        }
    }

    #endregion
}
