﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
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

    internal static bool TryWriteAllLines(string file, List<string> lines)
    {
        try
        {
            File.WriteAllLines(file, lines);
            return true;
        }
        catch (Exception ex)
        {
            Log(ErrorText.ExWrite + file, ex);
            return false;
        }
    }

    #endregion
}
