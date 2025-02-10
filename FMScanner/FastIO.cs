using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.InteropServices;
using static AL_Common.FastIO_Native;

namespace FMScanner;

internal static class FastIO
{
    private enum FastIOSearchOption
    {
        TopDirectoryOnly,
        AllDirectories,
    }

    internal static bool FilesExistSearchTop(string path, string[] searchPatterns)
    {
        return FirstFileExists(FastIOSearchOption.TopDirectoryOnly, path, searchPatterns);
    }

    internal static bool FilesExistSearchAll(string path, string[] searchPatterns)
    {
        return FirstFileExists(FastIOSearchOption.AllDirectories, path, searchPatterns);
    }

    private static void ThrowException(string[] searchPatterns, int err, string path, string pattern, int loop)
    {
        string spString = "";
        for (int i = 0; i < searchPatterns.Length; i++)
        {
            if (i > 0) spString += ",";
            spString += searchPatterns[i];
        }

        string whichLoop = loop == 0 ? "First loop" : "Second loop";

        Win32Exception ex = new(err);
        throw new Win32Exception(err,
            whichLoop + $"{NL}" +
            "System error code: " + err.ToStrInv() + $"{NL}" +
            ex.Message + $"{NL}" +
            "path: '" + path + $"'{NL}" +
            "search patterns: " + spString + $"{NL}" +
            "current search pattern: '" + pattern + "'");
    }

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    private static bool FirstFileExists(FastIOSearchOption searchOption, string path, string[] searchPatterns)
    {
        path = NormalizeAndCheckPath(path, pathIsKnownValid: false);

        // Other relevant errors (though we don't use them specifically at the moment)
        //const int ERROR_PATH_NOT_FOUND = 0x3;
        //const int ERROR_REM_NOT_LIST = 0x33;
        //const int ERROR_BAD_NETPATH = 0x35;

        // Search the base directory first, and only then search subdirectories.

        string searchPath = MakeUNCPath(path) + "\\";

        foreach (string p in searchPatterns)
        {
            bool searchPatternHas3CharExt = SearchPatternHas3CharExt(p);

            using FileFinder fileFinder = FileFinder.Create(
                searchPath + p,
                0,
                out WIN32_FIND_DATAW findData);

            if (fileFinder.IsInvalid)
            {
                int err = Marshal.GetLastWin32Error();
                if (err == ERROR_FILE_NOT_FOUND) continue;

                // Since the framework isn't here to save us, we should blanket-catch and throw on every
                // possible error other than file-not-found (as that's an intended scenario, obviously).
                // This isn't as nice as you'd get from a framework method call, but it gets the job done.
                ThrowException(searchPatterns, err, path, p, 0);
            }
            do
            {
                if ((findData.dwFileAttributes & FILE_ATTRIBUTE_DIRECTORY) != FILE_ATTRIBUTE_DIRECTORY &&
                    findData.cFileName != "." && findData.cFileName != ".." &&
                    !(searchPatternHas3CharExt && FileNameExtTooLong(findData.cFileName)))
                {
                    return true;
                }
            } while (fileFinder.TryFindNextFile(out findData));

            if (searchOption == FastIOSearchOption.TopDirectoryOnly) return false;
        }

        using (FileFinder fileFinder = FileFinder.Create(
                   searchPath + "*",
                   0,
                   out WIN32_FIND_DATAW findData))
        {
            if (fileFinder.IsInvalid)
            {
                int err = Marshal.GetLastWin32Error();
                if (err != ERROR_FILE_NOT_FOUND)
                {
                    ThrowException(searchPatterns, err, path, @"\* [looking for all directories]", 1);
                }
            }
            do
            {
                if ((findData.dwFileAttributes & FILE_ATTRIBUTE_DIRECTORY) == FILE_ATTRIBUTE_DIRECTORY &&
                    findData.cFileName != "." && findData.cFileName != ".." &&
                    FirstFileExists(FastIOSearchOption.AllDirectories, Path.Combine(path, findData.cFileName),
                        searchPatterns))
                {
                    return true;
                }
            } while (fileFinder.TryFindNextFile(out findData));

            return false;
        }
    }
}
