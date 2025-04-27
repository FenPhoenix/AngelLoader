using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using AngelLoader.DataClasses;
using static AL_Common.FastIO_Native;

namespace AngelLoader;

internal static class FastIO
{
    private enum FileType
    {
        Files,
        Directories,
        FilesAndDirectories,
    }

    // This is meant to be industrial-strength, so just call the params nullable and check them.
    // No screwing around.
    private static void ThrowException(string? searchPattern, int err, string? path)
    {
        searchPattern ??= "<null>";
        path ??= "<null>";

        Win32Exception ex = new(err);
        throw new Win32Exception(err,
            "System error code: " + err.ToStrInv() + $"{NL}" +
            ex.Message + $"{NL}" +
            "path: '" + path + $"'{NL}" +
            "search pattern: " + searchPattern + $"{NL}");
    }

    internal static List<string> GetDirsTopOnly(
        string path,
        string searchPattern,
        bool ignoreReparsePoints = false,
        bool pathIsKnownValid = false,
        bool returnFullPaths = true,
        int preallocate = 16)
    {
        List<string> ret = new(preallocate);
        GetFilesTopOnlyInternal(
            path,
            searchPattern,
            FileType.Directories,
            ignoreReparsePoints,
            pathIsKnownValid,
            returnFullPaths,
            ret,
            null);
        return ret;
    }

    internal static List<string> GetFilesTopOnly(
        string path,
        string searchPattern,
        bool pathIsKnownValid = false,
        bool returnFullPaths = true,
        int preallocate = 16)
    {
        List<string> ret = new(preallocate);
        GetFilesTopOnlyInternal(
            path,
            searchPattern,
            FileType.Files,
            ignoreReparsePoints: false,
            pathIsKnownValid,
            returnFullPaths,
            ret,
            null);
        return ret;
    }

    internal static void GetDirsTopOnly_FMs(
        string path,
        string searchPattern,
        List<string> dirNames,
        List<ExpandableDate_FromTicks> dateTimes)
    {
        dirNames.Clear();
        dateTimes.Clear();
        GetFilesTopOnlyInternal(
            path,
            searchPattern,
            FileType.Directories,
            ignoreReparsePoints: false,
            pathIsKnownValid: false,
            returnFullPaths: false,
            dirNames,
            dateTimes);
    }

    internal static void GetFilesTopOnly_FMs(
        string path,
        string searchPattern,
        List<string> fileNames,
        List<ExpandableDate_FromTicks> dateTimes)
    {
        fileNames.Clear();
        dateTimes.Clear();
        GetFilesTopOnlyInternal(
            path,
            searchPattern,
            FileType.Files,
            ignoreReparsePoints: false,
            pathIsKnownValid: false,
            returnFullPaths: false,
            fileNames,
            dateTimes);
    }

    // ~2.4x faster than GetFiles() - huge boost to cold startup time
    private static void GetFilesTopOnlyInternal(
        string path,
        string searchPattern,
        FileType fileType,
        bool ignoreReparsePoints,
        bool pathIsKnownValid,
        bool returnFullPaths,
        List<string> filesOrDirs,
        List<ExpandableDate_FromTicks>? dateTimes)
    {
        if (searchPattern.IsEmpty())
        {
            ThrowHelper.ArgumentException(nameof(searchPattern) + " was null or empty", nameof(searchPattern));
        }

        path = NormalizeAndCheckPath(path, pathIsKnownValid);
        bool searchPatternHas3CharExt = SearchPatternHas3CharExt(searchPattern);

        // Other relevant errors (though we don't use them specifically at the moment)
        //const int ERROR_PATH_NOT_FOUND = 0x3;
        //const int ERROR_REM_NOT_LIST = 0x33;
        //const int ERROR_BAD_NETPATH = 0x35;

        string searchPath = MakeUNCPath(path) + "\\" + searchPattern;

        using FileFinder fileFinder = FileFinder.Create(
            searchPath,
            FIND_FIRST_EX_LARGE_FETCH,
            out FindData findData);

        if (fileFinder.IsInvalid)
        {
            int err = Marshal.GetLastPInvokeError();
            if (err is ERROR_FILE_NOT_FOUND or ERROR_NO_MORE_FILES) return;

            // Since the framework isn't here to save us, we should blanket-catch and throw on every
            // possible error other than file-not-found (as that's an intended scenario, obviously).
            // This isn't as nice as you'd get from a framework method call, but it gets the job done.
            ThrowException(searchPattern, err, path);
        }
        do
        {
            if ((
                    (fileType == FileType.Files && IsFile(findData)) ||
                    (fileType == FileType.Directories && IsDirectory(findData, ignoreReparsePoints)) ||
                    (fileType == FileType.FilesAndDirectories && (IsFile(findData) || IsDirectory(findData, ignoreReparsePoints)))
                ) &&
                (
                    findData.cFileName != "." && findData.cFileName != ".." &&
                    !(searchPatternHas3CharExt && FileNameExtTooLong(findData.cFileName))
                )
               )
            {
                string fullName = returnFullPaths
                    // Exception could occur here
                    // @DIRSEP: Matching behavior of GetFiles()? Is it? Or does it just return whatever it gets from Windows?
                    ? Path.Combine(path, findData.cFileName).ToSystemDirSeps_Net()
                    : findData.cFileName;

                filesOrDirs.Add(fullName);
                // PERF: 0.67ms over 1099 dirs (Ryzen 3950x)
                // Very cheap operation all things considered, but it never hurts to skip it when we don't
                // need it.
                dateTimes?.Add(new ExpandableDate_FromTicks(findData.ftCreationTime.ToTicks()));
            }
        } while (fileFinder.TryFindNextFile(out findData));
    }

    public static bool OneOrMoreTdmPk4FMFilesExist(string path, bool pathIsKnownValid)
    {
        const string searchPattern = "*.pk4";

        path = NormalizeAndCheckPath(path, pathIsKnownValid);

        // Other relevant errors (though we don't use them specifically at the moment)
        //const int ERROR_PATH_NOT_FOUND = 0x3;
        //const int ERROR_REM_NOT_LIST = 0x33;
        //const int ERROR_BAD_NETPATH = 0x35;

        string searchPath = MakeUNCPath(path) + "\\" + searchPattern;

        using FileFinder fileFinder = FileFinder.Create(
            searchPath,
            FIND_FIRST_EX_LARGE_FETCH,
            out FindData findData);

        if (fileFinder.IsInvalid)
        {
            int err = Marshal.GetLastPInvokeError();
            if (err is ERROR_FILE_NOT_FOUND or ERROR_NO_MORE_FILES) return false;

            // Since the framework isn't here to save us, we should blanket-catch and throw on every
            // possible error other than file-not-found (as that's an intended scenario, obviously).
            // This isn't as nice as you'd get from a framework method call, but it gets the job done.
            ThrowException(searchPattern, err, path);
        }
        do
        {
            if (IsFile(findData) &&
                findData.cFileName != "." && findData.cFileName != ".." &&
                !FileNameExtTooLong(findData.cFileName) &&
                !findData.cFileName.EndsWithI("_l10n.pk4")
               )
            {
                return true;
            }
        } while (fileFinder.TryFindNextFile(out findData));

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsFile(FindData findData)
    {
        return (findData.dwFileAttributes & FILE_ATTRIBUTE_DIRECTORY) != FILE_ATTRIBUTE_DIRECTORY;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsDirectory(FindData findData, bool ignoreReparsePoints)
    {
        return (findData.dwFileAttributes & FILE_ATTRIBUTE_DIRECTORY) == FILE_ATTRIBUTE_DIRECTORY &&
               (!ignoreReparsePoints || (findData.dwFileAttributes & FILE_ATTRIBUTE_REPARSE_POINT) != FILE_ATTRIBUTE_REPARSE_POINT);
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
        using FileFinder fileFinder = FileFinder.Create(
            searchPath,
            FIND_FIRST_EX_LARGE_FETCH,
            out FindData findData);

        if (fileFinder.IsInvalid)
        {
            int err = Marshal.GetLastPInvokeError();
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
        } while (fileFinder.TryFindNextFile(out findData));

        return false;
    }
}
