﻿using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace AL_Common;

public static class FastIO_Native
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

    [StructLayout(LayoutKind.Auto)]
    public readonly ref struct FileFinder
    {
        private const int FindExInfoBasic = 1;
        private const int FindExSearchNameMatch = 0;

        private readonly nint _handle;

        private FileFinder(nint handle) => _handle = handle;

        public static FileFinder Create(string fileName, int additionalFlags, out WIN32_FIND_DATAW findData)
        {
            return new FileFinder(FindFirstFileExW(
                fileName,
                FindExInfoBasic,
                out findData,
                FindExSearchNameMatch,
                0,
                additionalFlags));
        }

        public bool TryFindNextFile(out WIN32_FIND_DATAW findData) => FindNextFileW(_handle, out findData);

        public bool IsInvalid => _handle == 0 || _handle == -1;

        public void Dispose() => FindClose(_handle);

        #region P/Invoke

        [DllImport("kernel32.dll", ExactSpelling = true, CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern nint FindFirstFileExW(
            string lpFileName,
            int fInfoLevelId,
            out WIN32_FIND_DATAW lpFindFileData,
            int fSearchOp,
            nint lpSearchFilter,
            int dwAdditionalFlags);

        [DllImport("kernel32.dll", ExactSpelling = true, CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool FindNextFileW(nint hFindFile, out WIN32_FIND_DATAW lpFindFileData);

        [DllImport("kernel32.dll", ExactSpelling = true, SetLastError = true)]
        private static extern bool FindClose(nint hFindFile);

        #endregion
    }

    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    [SuppressMessage("ReSharper", "FieldCanBeMadeReadOnly.Global")]
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct WIN32_FIND_DATAW
    {
        public uint dwFileAttributes;
        public FILE_TIME ftCreationTime;
        public FILE_TIME ftLastAccessTime;
        public FILE_TIME ftLastWriteTime;
        public uint nFileSizeHigh;
        public uint nFileSizeLow;
        public uint dwReserved0;
        public uint dwReserved1;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_PATH)]
        public string cFileName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 14)]
        public string cAlternateFileName;
    }

    #endregion

    private static readonly char[] _invalidPathChars = Path.GetInvalidPathChars();

    public static string NormalizeAndCheckPath(string path, bool pathIsKnownValid)
    {
        // Vital, path must not have a trailing separator
        // We also normalize it manually because we use \?\\ which skips normalization
        path = path.ToBackSlashes().TrimEnd(CA_Backslash);

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
