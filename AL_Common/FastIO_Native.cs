using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static AL_Common.Common;

namespace AL_Common;

// @NET5(FastIO): This whole thing needs reconsidering
// .NET modern does the don't-ask-for-8.3-thing already, so it should be as fast as this at least.
// It also fixes the 3-character extension quirk.
public static partial class FastIO_Native
{
    #region Fields

    public const int FILE_ATTRIBUTE_DIRECTORY = 0x10;
    public const int FIND_FIRST_EX_LARGE_FETCH = 0x2;
    public const int ERROR_FILE_NOT_FOUND = 0x2;
    public const int FILE_ATTRIBUTE_REPARSE_POINT = 0x400;
    // The docs specify this as something FindNextFile* can return, but say nothing about it regarding
    // FindFirstFile*. But the .NET Framework reference source checks for this along with ERROR_FILE_NOT_FOUND
    // so I guess I will too, though it seems never to have been a problem before(?)
    public const int ERROR_NO_MORE_FILES = 0x12;

    #endregion

    #region Classes / structs / enums

    public readonly ref struct FileFinder
    {
        private const int FindExInfoBasic = 1;
        private const int FindExSearchNameMatch = 0;

        private readonly IntPtr _handle;

        private FileFinder(IntPtr handle) => _handle = handle;

        public static FileFinder Create(string fileName, int additionalFlags, out FindData findData)
        {
            IntPtr handle = FindFirstFileExW(
                fileName,
                FindExInfoBasic,
                out WIN32_FIND_DATAW findDataInternal,
                FindExSearchNameMatch,
                IntPtr.Zero,
                additionalFlags);

            findData = new FindData(findDataInternal);
            return new FileFinder(handle);
        }

        public bool TryFindNextFile(out FindData findData)
        {
            bool success = FindNextFileW(_handle, out var findDataInternal);
            findData = new FindData(findDataInternal);
            return success;
        }

        public bool IsInvalid => _handle == IntPtr.Zero || _handle == new IntPtr(-1);

        public void Dispose() => FindClose(_handle);
    }

    #region P/Invoke

    [LibraryImport("kernel32.dll", SetLastError = true, StringMarshalling = StringMarshalling.Utf16)]
    private static partial IntPtr FindFirstFileExW(
        string lpFileName,
        int fInfoLevelId,
        out WIN32_FIND_DATAW lpFindFileData,
        int fSearchOp,
        IntPtr lpSearchFilter,
        int dwAdditionalFlags);

    [LibraryImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool FindNextFileW(IntPtr hFindFile, out WIN32_FIND_DATAW lpFindFileData);

    [LibraryImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool FindClose(IntPtr hFindFile);

    #endregion

    public readonly ref struct FindData
    {
        public readonly uint dwFileAttributes;
        public readonly FILE_TIME ftCreationTime;
#if false
        public readonly FILE_TIME ftLastAccessTime;
        public readonly FILE_TIME ftLastWriteTime;
#endif
        public readonly string cFileName;

        internal FindData(WIN32_FIND_DATAW findDataInternal)
        {
            dwFileAttributes = findDataInternal.dwFileAttributes;
            ftCreationTime = findDataInternal.ftCreationTime;
#if false
            ftLastAccessTime = findDataInternal.ftLastAccessTime;
            ftLastWriteTime = findDataInternal.ftLastWriteTime;
#endif
            cFileName = findDataInternal.ConvertFileNameToString();
        }
    }

    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    [SuppressMessage("ReSharper", "FieldCanBeMadeReadOnly.Global")]
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    internal unsafe struct WIN32_FIND_DATAW
    {
        internal uint dwFileAttributes;
        internal FILE_TIME ftCreationTime;
        internal FILE_TIME ftLastAccessTime;
        internal FILE_TIME ftLastWriteTime;
        internal uint nFileSizeHigh;
        internal uint nFileSizeLow;
        internal uint dwReserved0;
        internal uint dwReserved1;
        private fixed char _cFileName[MAX_PATH];
        private fixed char _cAlternateFileName[14];

        /// <summary>
        /// Gets the null-terminated string length of the given span.
        /// </summary>
        private static int GetFixedBufferStringLength(ReadOnlySpan<char> span)
        {
            int length = span.IndexOf('\0');
            return length < 0 ? span.Length : length;
        }

        /// <summary>
        /// Returns a string from the given span, terminating the string at null if present.
        /// </summary>
        private static string GetStringFromFixedBuffer(ReadOnlySpan<char> span)
        {
            fixed (char* c = &MemoryMarshal.GetReference(span))
            {
                return new string(c, 0, GetFixedBufferStringLength(span));
            }
        }

        public string ConvertFileNameToString() =>
            GetStringFromFixedBuffer(MemoryMarshal.CreateReadOnlySpan(ref _cFileName[0], MAX_PATH));
    }

    #endregion

    private static readonly char[] _invalidPathChars = Path.GetInvalidPathChars();

    public static string NormalizeAndCheckPath(string path, bool pathIsKnownValid)
    {
        // Vital, path must not have a trailing separator
        // We also normalize it manually because we use \?\\ which skips normalization
        path = path.ToBackSlashes().TrimEnd('\\');

        if (!pathIsKnownValid)
        {
            bool pathContainsInvalidChars = false;

            foreach (char c in _invalidPathChars)
            {
                if (path.Contains(c))
                {
                    pathContainsInvalidChars = true;
                    break;
                }
            }

            if (path.IsWhiteSpace() || pathContainsInvalidChars)
            {
                ThrowHelper.ArgumentException("The path '" + path + "' is empty, consists only of whitespace, or contains invalid characters.");
            }
        }

        return path;
    }

    #region Workaround https://fenphoenix.github.io/AngelLoader/file_ext_note.html

    public static bool SearchPatternHas3CharExt(string searchPattern)
    {
        if (searchPattern.Length > 4 && searchPattern[^4] == '.')
        {
            for (int i = 1; i <= 3; i++)
            {
                // This logic isn't quite correct; the problem still occurs if our pattern is like "*.t*t"
                // for example, but checking for that starts to get complicated and we don't ever use patterns
                // like that ourselves, so meh.
                char c = searchPattern[^i];
                if (c is '*' or '?') return false;
            }
            return true;
        }

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool FileNameExtTooLong(string fileName)
    {
        int len = fileName.Length;
        return len > 4 && fileName[len - 4] != '.';
    }

    #endregion
}
