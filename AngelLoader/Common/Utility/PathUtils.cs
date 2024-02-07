using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using AngelLoader.DataClasses;
using static AL_Common.Common;
using static AL_Common.Logger;

namespace AngelLoader;

public static partial class Utils
{
    internal static bool PathIsRelative(string path) =>
        path.Length > 1 && path[0] == '.' &&
        (path[1].IsDirSep() || (path[1] == '.' && path.Length > 2 && path[2].IsDirSep()));

    internal static string RelativeToAbsolute(string basePath, string relativePath)
    {
        AssertR(!basePath.IsEmpty(), "basePath is null or empty");

        return relativePath.IsEmpty() ? basePath : Path.GetFullPath(Path.Combine(basePath, relativePath));
    }

    #region Get file / dir names

    /// <summary>
    /// Strips the leading path from the filename, taking into account both / and \ chars.
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    internal static string GetFileNameFast(this string path)
    {
        int index = path.Rel_LastIndexOfDirSep();
        return index == -1 ? path : path[(index + 1)..];
    }

    internal static string GetDirNameFast(this string path) => GetFileNameFast(path.TrimEnd(CA_BS_FS));

    #endregion

    #region File extensions

    #region Filename extension checks

    // General version; might need to be different than the scanner version
    internal static bool IsValidReadme(this string readme) =>
        readme.ExtIsTxt() ||
        readme.ExtIsRtf() ||
        readme.ExtIsWri() ||
        readme.ExtIsGlml() ||
        readme.ExtIsHtml();

    #region Baked-in extension checks

    // @RAR: We have too much duplicated code now, clean it up.
    internal static bool ExtIsArchive(this string value) =>
        value.ExtIsZip() ||
        value.ExtIs7z() ||
        value.ExtIsRar();

    #endregion

    #endregion

    #endregion

    #region Try combine path and check existence

    internal static bool TryCombineDirectoryPathAndCheckExistence(
        string pathPart1,
        string pathPart2,
        out string combinedPath)
    {
        try
        {
            string ret = Path.Combine(pathPart1, pathPart2);
            if (Directory.Exists(ret))
            {
                combinedPath = ret;
                return true;
            }
            else
            {
                combinedPath = "";
                return false;
            }
        }
        catch
        {
            combinedPath = "";
            return false;
        }
    }

    internal static bool TryCombineFilePathAndCheckExistence(
        string pathPart1,
        string pathPart2,
        out string combinedPath)
    {
        try
        {
            string ret = Path.Combine(pathPart1, pathPart2);
            if (File.Exists(ret))
            {
                combinedPath = ret;
                return true;
            }
            else
            {
                combinedPath = "";
                return false;
            }
        }
        catch
        {
            combinedPath = "";
            return false;
        }
    }

    internal static bool TryCombineFilePathAndCheckExistence(
        string pathPart1,
        string pathPart2,
        string pathPart3,
        out string combinedPath)
    {
        try
        {
            string ret = Path.Combine(pathPart1, pathPart2, pathPart3);
            if (File.Exists(ret))
            {
                combinedPath = ret;
                return true;
            }
            else
            {
                combinedPath = "";
                return false;
            }
        }
        catch
        {
            combinedPath = "";
            return false;
        }
    }

    #region Disabled until needed

#if false
    internal static bool TryCombineFilePathAndCheckExistence(string[] pathParts, out string combinedPath)
    {
        try
        {
            string ret = Path.Combine(pathParts);
            if (File.Exists(ret))
            {
                combinedPath = ret;
                return true;
            }
            else
            {
                combinedPath = "";
                return false;
            }
        }
        catch
        {
            combinedPath = "";
            return false;
        }
    }
#endif

    #endregion

    #endregion

    #region Try read and write lines

    internal static bool TryReadAllLines(string file, [NotNullWhen(true)] out List<string>? lines)
    {
        try
        {
            lines = File_ReadAllLines_List(file);
            return true;
        }
        catch (Exception ex)
        {
            Log(ErrorText.ExRead + file, ex);
            lines = null;
            return false;
        }
    }

    internal static bool TryWriteAllLines(string file, List<string> lines, [NotNullWhen(false)] out Exception? exception)
    {
        try
        {
            File.WriteAllLines(file, lines);
            exception = null;
            return true;
        }
        catch (Exception ex)
        {
            Log(ErrorText.ExWrite + file, ex);
            exception = ex;
            return false;
        }
    }

    #region Default encoding

    /*
    NewDark wants paths to be in system default (ANSI) encoding. Note that it appears to only be cam_mod.ini that
    it wants in this encoding, either that or it just wants the paths specifically to be in this encoding.

    FMSel says this:

    UTF-8 USAGE
    -----------
    All file and directory names (including the raw FM name since that boils down to a directory) are NOT in UTF-8
    format, they're in "raw" unconverted format to keep things simple, all other strings (nice names, tags, notes
    etc. etc.) are in UTF-8 format
    */

    internal static bool TryReadAllLines_DefaultEncoding(string file, [NotNullWhen(true)] out List<string>? lines)
    {
        try
        {
            lines = File_ReadAllLines_List(file, Encoding.Default, true);
            return true;
        }
        catch (Exception ex)
        {
            Log(ErrorText.ExRead + file, ex);
            lines = null;
            return false;
        }
    }

    internal static bool TryWriteAllLines_DefaultEncoding(string file, List<string> lines, [NotNullWhen(false)] out Exception? exception)
    {
        try
        {
            File.WriteAllLines(file, lines, Encoding.Default);
            exception = null;
            return true;
        }
        catch (Exception ex)
        {
            Log(ErrorText.ExWrite + file, ex);
            exception = ex;
            return false;
        }
    }

    #endregion

    #endregion

    internal static bool DirectoryHasWritePermission(string path)
    {
        try
        {
            if (path.IsWhiteSpace()) return true;

            using FileStream fs = File.Create(
                Path.Combine(path, Path.GetRandomFileName()),
                1,
                FileOptions.DeleteOnClose);
            return true;
        }
        catch (UnauthorizedAccessException)
        {
            return false;
        }
        catch (Exception)
        {
            // The write may still fail for other reasons, but we still have write permission in theory
            return true;
        }
    }
}
