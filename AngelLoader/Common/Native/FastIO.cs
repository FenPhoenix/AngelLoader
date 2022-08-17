using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using static AL_Common.Common;
using static AL_Common.FastIO_Native;

namespace AngelLoader
{
    internal static class FastIO
    {
        private enum FileType
        {
            Files,
            Directories
        }

        // This is meant to be industrial-strength, so just call the params nullable and check them.
        // No screwing around.
        private static void ThrowException(string? searchPattern, int err, string? path)
        {
            searchPattern ??= "<null>";
            path ??= "<null>";

            var ex = new Win32Exception(err);
            throw new Win32Exception(err,
                "System error code: " + err + "\r\n" +
                ex.Message + "\r\n" +
                "path: '" + path + "'\r\n" +
                "search pattern: " + searchPattern + "\r\n");
        }

        internal static List<string> GetDirsTopOnly(
            string path,
            string searchPattern,
            bool initListCapacityLarge = false,
            bool ignoreReparsePoints = false,
            bool pathIsKnownValid = false,
            bool returnFullPaths = true)
        {
            return GetFilesTopOnlyInternal(
                path,
                searchPattern,
                initListCapacityLarge,
                FileType.Directories,
                ignoreReparsePoints,
                pathIsKnownValid,
                returnFullPaths,
                returnDateTimes: false,
                out _);
        }

        internal static List<string> GetFilesTopOnly(
            string path,
            string searchPattern,
            bool initListCapacityLarge = false,
            bool pathIsKnownValid = false,
            bool returnFullPaths = true)
        {
            return GetFilesTopOnlyInternal(
                path,
                searchPattern,
                initListCapacityLarge,
                FileType.Files,
                ignoreReparsePoints: false,
                pathIsKnownValid,
                returnFullPaths,
                returnDateTimes: false,
                out _);
        }

        internal static List<string> GetDirsTopOnly_FMs(
            string path,
            string searchPattern,
            out List<DateTime> dateTimes)
        {
            return GetFilesTopOnlyInternal(
                path,
                searchPattern,
                initListCapacityLarge: true,
                FileType.Directories,
                ignoreReparsePoints: false,
                pathIsKnownValid: false,
                returnFullPaths: false,
                returnDateTimes: true,
                out dateTimes);
        }

        internal static List<string> GetFilesTopOnly_FMs(
            string path,
            string searchPattern,
            out List<DateTime> dateTimes)
        {
            return GetFilesTopOnlyInternal(
                path,
                searchPattern,
                initListCapacityLarge: true,
                FileType.Files,
                ignoreReparsePoints: false,
                pathIsKnownValid: false,
                returnFullPaths: false,
                returnDateTimes: true,
                out dateTimes);
        }

        // ~2.4x faster than GetFiles() - huge boost to cold startup time
        private static List<string> GetFilesTopOnlyInternal(
            string path,
            string searchPattern,
            bool initListCapacityLarge,
            FileType fileType,
            bool ignoreReparsePoints,
            bool pathIsKnownValid,
            bool returnFullPaths,
            bool returnDateTimes,
            out List<DateTime> dateTimes)
        {
            if (searchPattern.IsEmpty())
            {
                throw new ArgumentException(nameof(searchPattern) + " was null or empty", nameof(searchPattern));
            }

            path = NormalizeAndCheckPath(path, pathIsKnownValid);
            bool searchPatternHas3CharExt = SearchPatternHas3CharExt(searchPattern);

            // PERF: We can't know how many files we're going to find, so make the initial list capacity large
            // enough that we're unlikely to have it bump its size up repeatedly. Shaves some time off.
            var ret = initListCapacityLarge ? new List<string>(2000) : new List<string>(16);
            dateTimes =
                !returnDateTimes ? new List<DateTime>() :
                initListCapacityLarge ? new List<DateTime>(2000) : new List<DateTime>(16);

            // Other relevant errors (though we don't use them specifically at the moment)
            //const int ERROR_PATH_NOT_FOUND = 0x3;
            //const int ERROR_REM_NOT_LIST = 0x33;
            //const int ERROR_BAD_NETPATH = 0x35;

            string searchPath = MakeUNCPath(path) + "\\" + searchPattern;

            using var findHandle = FindFirstFileExW(
                searchPath,
                FindExInfoBasic,
                out WIN32_FIND_DATAW findData,
                FindExSearchNameMatch,
                IntPtr.Zero,
                FIND_FIRST_EX_LARGE_FETCH);

            if (findHandle.IsInvalid)
            {
                int err = Marshal.GetLastWin32Error();
                if (err is ERROR_FILE_NOT_FOUND or ERROR_NO_MORE_FILES) return ret;

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
                    findData.cFileName != "." && findData.cFileName != ".." &&
                    !(searchPatternHas3CharExt && FileNameExtTooLong(findData.cFileName)))
                {
                    string fullName = returnFullPaths
                        // Exception could occur here
                        // @DIRSEP: Matching behavior of GetFiles()? Is it? Or does it just return whatever it gets from Windows?
                        ? Path.Combine(path, findData.cFileName).ToSystemDirSeps_Net()
                        : findData.cFileName;

                    ret.Add(fullName);
                    // PERF: 0.67ms over 1099 dirs (Ryzen 3950x)
                    // Very cheap operation all things considered, but it never hurts to skip it when we don't
                    // need it.
                    if (returnDateTimes)
                    {
                        dateTimes.Add(DateTime.FromFileTimeUtc(findData.ftCreationTime.ToTicks()).ToLocalTime());
                    }
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
        internal static bool SearchDirForLanguages(
            string path,
            List<string> searchList,
            HashSetI retList,
            bool earlyOutOnEnglish)
        {
            path = NormalizeAndCheckPath(path, pathIsKnownValid: true);

            string searchPath = MakeUNCPath(path) + "\\*";
            using var findHandle = FindFirstFileExW(searchPath,
                FindExInfoBasic, out WIN32_FIND_DATAW findData,
                FindExSearchNameMatch, IntPtr.Zero, FIND_FIRST_EX_LARGE_FETCH);

            if (findHandle.IsInvalid)
            {
                int err = Marshal.GetLastWin32Error();
                if (err is ERROR_FILE_NOT_FOUND or ERROR_NO_MORE_FILES) return false;
                ThrowException("*", err, path);
            }
            do
            {
                if ((findData.dwFileAttributes & FILE_ATTRIBUTE_DIRECTORY) == FILE_ATTRIBUTE_DIRECTORY &&
                    // Just ignore reparse points and sidestep any problems
                    (findData.dwFileAttributes & FILE_ATTRIBUTE_REPARSE_POINT) != FILE_ATTRIBUTE_REPARSE_POINT &&
                    findData.cFileName != "." && findData.cFileName != "..")
                {
                    if (LanguageSupport.LanguageIsSupported(findData.cFileName))
                    {
                        // Add lang dir to found langs list, but not to search list - don't search within lang
                        // dirs (matching FMSel behavior)
                        retList.Add(findData.cFileName);
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
