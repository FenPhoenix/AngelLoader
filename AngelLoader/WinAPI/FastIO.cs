using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using AngelLoader.Common.Utility;
using JetBrains.Annotations;
using Microsoft.Win32.SafeHandles;
using static AngelLoader.Common.Common;

namespace AngelLoader.WinAPI
{
    internal static class FastIO
    {
        #region Fields

        private const int FILE_ATTRIBUTE_DIRECTORY = 0x10;
        private const int FIND_FIRST_EX_LARGE_FETCH = 0x2;
        private const int ERROR_FILE_NOT_FOUND = 0x2;
        private const int FILE_ATTRIBUTE_REPARSE_POINT = 0x400;
        // The docs specify this as something FindNextFile* can return, but say nothing about it regarding
        // FindFirstFile*. But the .NET Framework reference source checks for this along with ERROR_FILE_NOT_FOUND
        // so I guess I will too, though it seems never to have been a problem before(?)
        private const int ERROR_NO_MORE_FILES = 0x12;

        private enum FileType
        {
            Files,
            Directories
        }

        #endregion

        #region Classes / structs / enums

        // So we don't have to remember to call FindClose()
        [UsedImplicitly]
        internal class SafeSearchHandle : SafeHandleZeroOrMinusOneIsInvalid
        {
            internal SafeSearchHandle() : base(true) { }
            protected override bool ReleaseHandle() => FindClose(handle);

            [DllImport("kernel32.dll", SetLastError = true)]
            private static extern bool FindClose(IntPtr hFindFile);
        }

        [PublicAPI]
        private enum FINDEX_INFO_LEVELS
        {
            FindExInfoStandard = 0,
            FindExInfoBasic = 1
        }

        [PublicAPI]
        private enum FINDEX_SEARCH_OPS
        {
            FindExSearchNameMatch = 0,
            FindExSearchLimitToDirectories = 1,
            FindExSearchLimitToDevices = 2
        }

        [SuppressMessage("ReSharper", "InconsistentNaming")]
        [SuppressMessage("ReSharper", "MemberCanBePrivate.Local")]
        [SuppressMessage("ReSharper", "FieldCanBeMadeReadOnly.Local")]
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct WIN32_FIND_DATA
        {
            internal uint dwFileAttributes;
            internal System.Runtime.InteropServices.ComTypes.FILETIME ftCreationTime;
            internal System.Runtime.InteropServices.ComTypes.FILETIME ftLastAccessTime;
            internal System.Runtime.InteropServices.ComTypes.FILETIME ftLastWriteTime;
            internal uint nFileSizeHigh;
            internal uint nFileSizeLow;
            internal uint dwReserved0;
            internal uint dwReserved1;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            internal string cFileName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 14)]
            internal string cAlternateFileName;
        }

        #endregion

        #region P/Invoke definitions

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern SafeSearchHandle FindFirstFileEx(
            string lpFileName,
            FINDEX_INFO_LEVELS fInfoLevelId,
            out WIN32_FIND_DATA lpFindFileData,
            FINDEX_SEARCH_OPS fSearchOp,
            IntPtr lpSearchFilter,
            int dwAdditionalFlags);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool FindNextFileW(SafeSearchHandle hFindFile, out WIN32_FIND_DATA lpFindFileData);

        #endregion

        private static void ThrowException(string searchPattern, int err, string path)
        {
            if (searchPattern == null) searchPattern = "<null>";
            if (path == null) path = "<null>";

            var ex = new Win32Exception(err);
            throw new Win32Exception(err,
                "System error code: " + err + "\r\n" +
                ex.Message + "\r\n" +
                "path: '" + path + "'\r\n" +
                "search pattern: " + searchPattern + "\r\n");
        }

        internal static List<string> GetDirsTopOnly(string path, string searchPattern, bool initListCapacityLarge = false,
            bool ignoreReparsePoints = false)
        {
            return GetFilesTopOnlyInternal(path, searchPattern, initListCapacityLarge, FileType.Directories, ignoreReparsePoints);
        }

        internal static List<string> GetFilesTopOnly(string path, string searchPattern, bool initListCapacityLarge = false)
        {
            return GetFilesTopOnlyInternal(path, searchPattern, initListCapacityLarge, FileType.Files, false);
        }

        // ~2.4x faster than GetFiles() - huge boost to cold startup time
        private static List<string> GetFilesTopOnlyInternal(string path, string searchPattern,
            bool initListCapacityLarge, FileType fileType, bool ignoreReparsePoints)
        {
            if (string.IsNullOrEmpty(searchPattern))
            {
                throw new ArgumentException(nameof(searchPattern) + @" was null or empty", nameof(searchPattern));
            }

            // Vital, path must not have a trailing separator
            path = path.TrimEnd('\\', '/');

            if (string.IsNullOrWhiteSpace(path) || Path.GetInvalidPathChars().Any(path.Contains<char>))
            {
                throw new ArgumentException("The path '" + path + "' is empty, consists only of whitespace, or contains invalid characters.");
            }

            // PERF: We can't know how many files we're going to find, so make the initial list capacity large
            // enough that we're unlikely to have it bump its size up repeatedly. Shaves some time off.
            var ret = initListCapacityLarge ? new List<string>(2000) : new List<string>(16);

            // Other relevant errors (though we don't use them specifically at the moment)
            //const int ERROR_PATH_NOT_FOUND = 0x3;
            //const int ERROR_REM_NOT_LIST = 0x33;
            //const int ERROR_BAD_NETPATH = 0x35;

            using var findHandle = FindFirstFileEx(@"\\?\" + path + "\\" + searchPattern,
                FINDEX_INFO_LEVELS.FindExInfoBasic, out WIN32_FIND_DATA findData,
                FINDEX_SEARCH_OPS.FindExSearchNameMatch, IntPtr.Zero, FIND_FIRST_EX_LARGE_FETCH);

            if (findHandle.IsInvalid)
            {
                var err = Marshal.GetLastWin32Error();
                if (err == ERROR_FILE_NOT_FOUND || err == ERROR_NO_MORE_FILES) return ret;

                // Since the framework isn't here to save us, we should blanket-catch and throw on every
                // possible error other than file-not-found (as that's an intended scenario, obviously).
                // This isn't as nice as you'd get from a framework method call, but it gets the job done.
                ThrowException(searchPattern, err, path);
            }
            do
            {
                if (((fileType == FileType.Files &&
                      (findData.dwFileAttributes & FILE_ATTRIBUTE_DIRECTORY) != FILE_ATTRIBUTE_DIRECTORY) ||
                     (fileType == FileType.Directories &&
                      (findData.dwFileAttributes & FILE_ATTRIBUTE_DIRECTORY) == FILE_ATTRIBUTE_DIRECTORY &&
                     (!ignoreReparsePoints || (findData.dwFileAttributes & FILE_ATTRIBUTE_REPARSE_POINT) != FILE_ATTRIBUTE_REPARSE_POINT))) &&
                    findData.cFileName != "." && findData.cFileName != "..")
                {
                    // Exception could occur here
                    var fullName = Path.Combine(path, findData.cFileName);

                    ret.Add(fullName);
                }
            } while (FindNextFileW(findHandle, out findData));

            return ret;
        }

        /// <summary>
        /// Helper for finding language-named subdirectories in an installed FM directory.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="searchList"></param>
        /// <param name="retList"></param>
        /// <param name="earlyOutOnEnglish"></param>
        /// <returns><see langword="true"/> if English was found and we quit the search early</returns>
        internal static bool SearchDirForLanguages(string path, List<string> searchList, List<string> retList,
            bool earlyOutOnEnglish)
        {
            // Always do this
            path = path.TrimEnd('\\', '/');

            using var findHandle = FindFirstFileEx(@"\\?\" + path + "\\*",
                FINDEX_INFO_LEVELS.FindExInfoBasic, out WIN32_FIND_DATA findData,
                FINDEX_SEARCH_OPS.FindExSearchNameMatch, IntPtr.Zero, FIND_FIRST_EX_LARGE_FETCH);

            if (findHandle.IsInvalid)
            {
                int err = Marshal.GetLastWin32Error();
                if (err == ERROR_FILE_NOT_FOUND || err == ERROR_NO_MORE_FILES) return false;
                ThrowException("*", err, path);
            }
            do
            {
                if ((findData.dwFileAttributes & FILE_ATTRIBUTE_DIRECTORY) == FILE_ATTRIBUTE_DIRECTORY &&
                    // Just ignore reparse points and sidestep any problems
                    (findData.dwFileAttributes & FILE_ATTRIBUTE_REPARSE_POINT) != FILE_ATTRIBUTE_REPARSE_POINT &&
                    findData.cFileName != "." && findData.cFileName != "..")
                {
                    if (FMSupportedLanguages.ContainsI(findData.cFileName))
                    {
                        // Add lang dir to found langs list, but not to search list - don't search within lang
                        // dirs (matching FMSel behavior)
                        if (!retList.ContainsI(findData.cFileName)) retList.Add(findData.cFileName);
                        // Matching FMSel behavior: early-out on English
                        if (earlyOutOnEnglish && findData.cFileName.EqualsI("english")) return true;
                    }
                    else
                    {
                        searchList.Add(Path.Combine(path, findData.cFileName));
                    }
                }
            } while (FindNextFileW(findHandle, out findData));

            return false;
        }
    }
}
