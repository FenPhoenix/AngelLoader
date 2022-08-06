using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using JetBrains.Annotations;
using Microsoft.Win32.SafeHandles;

namespace AL_Common
{
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

        // Reimplementing this internal struct for output parity with DirectoryInfo.Get*
        // Screw it, not touching this one at all and just shutting up all warnings.
        [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
        [SuppressMessage("ReSharper", "FieldCanBeMadeReadOnly.Global")]
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        [SuppressMessage("ReSharper", "RedundantCast")]
#pragma warning disable IDE0003, IDE0004
        public struct FILE_TIME
        {
            public uint ftTimeLow;
            public uint ftTimeHigh;

            public FILE_TIME(long fileTime)
            {
                this.ftTimeLow = (uint)fileTime;
                this.ftTimeHigh = (uint)(fileTime >> 32);
            }

            public readonly long ToTicks()
            {
                return ((long)this.ftTimeHigh << 32) + (long)this.ftTimeLow;
            }
        }
#pragma warning restore IDE0004, IDE0003

        // So we don't have to remember to call FindClose()
        [UsedImplicitly]
        public class SafeSearchHandle : SafeHandleZeroOrMinusOneIsInvalid
        {
            public SafeSearchHandle() : base(true) { }
            protected override bool ReleaseHandle() => FindClose(handle);

            [DllImport("kernel32.dll", SetLastError = true)]
            private static extern bool FindClose(IntPtr hFindFile);
        }

        /*
        public enum FINDEX_INFO_LEVELS
        {
            //FindExInfoStandard = 0,
            FindExInfoBasic = 1
        }
        */

        public const int FindExInfoBasic = 1;

        /*
        public enum FINDEX_SEARCH_OPS
        {
            FindExSearchNameMatch = 0,
            //FindExSearchLimitToDirectories = 1,
            //FindExSearchLimitToDevices = 2
        }
        */

        public const int FindExSearchNameMatch = 0;

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
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string cFileName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 14)]
            public string cAlternateFileName;
        }

        #endregion

        #region P/Invoke definitions

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern SafeSearchHandle FindFirstFileExW(
            string lpFileName,
            int fInfoLevelId,
            out WIN32_FIND_DATAW lpFindFileData,
            int fSearchOp,
            IntPtr lpSearchFilter,
            int dwAdditionalFlags);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern bool FindNextFileW(SafeSearchHandle hFindFile, out WIN32_FIND_DATAW lpFindFileData);

        #endregion

        private static readonly char[] _invalidPathChars = Path.GetInvalidPathChars();

        public static string NormalizeAndCheckPath(string path, bool pathIsKnownValid)
        {
            // Vital, path must not have a trailing separator
            // We also normalize it manually because we use \?\\ which skips normalization
            path = path.ToBackSlashes().TrimEnd(Common.CA_Backslash);

            if (!pathIsKnownValid)
            {
                bool pathContainsInvalidChars = false;

                // Dumb loop to avoid LINQ.
                for (int i = 0; i < _invalidPathChars.Length; i++)
                {
                    if (path.Contains(_invalidPathChars[i]))
                    {
                        pathContainsInvalidChars = true;
                        break;
                    }
                }

                if (string.IsNullOrWhiteSpace(path) || pathContainsInvalidChars)
                {
                    throw new ArgumentException("The path '" + path + "' is empty, consists only of whitespace, or contains invalid characters.");
                }
            }

            return path;
        }

        #region Workaround https://fenphoenix.github.io/AngelLoader/file_ext_note.html

        public static bool SearchPatternHas3CharExt(string searchPattern)
        {
            if (searchPattern.Length > 4 && searchPattern[searchPattern.Length - 4] == '.')
            {
                for (int i = 1; i <= 3; i++)
                {
                    // This logic isn't quite correct; the problem still occurs if our pattern is like "*.t*t"
                    // for example, but checking for that starts to get complicated and we don't ever use patterns
                    // like that ourselves, so meh.
                    char c = searchPattern[searchPattern.Length - i];
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
}
