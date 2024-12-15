// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.IO;

namespace AL_Common.FastZipReader;

public static class ZipHelpers
{
    private const int ValidZipDate_YearMin = 1980;

    private static readonly DateTime _invalidDateIndicator = new(ValidZipDate_YearMin, 1, 1, 0, 0, 0);

    /// <summary>
    /// Converts a Zip timestamp to a DateTime object. If <paramref name="zipDateTime"/> is not a
    /// valid Zip timestamp, an indicator value of 1980 January 1 at midnight will be returned.
    /// </summary>
    /// <param name="zipDateTime"></param>
    /// <returns></returns>
    public static DateTime ZipTimeToDateTime(uint zipDateTime)
    {
        if (zipDateTime == 0)
        {
            return _invalidDateIndicator;
        }

        // DosTime format 32 bits
        // Year: 7 bits, 0 is 1980
        // Month: 4 bits
        // Day: 5 bits
        // Hour: 5 bits
        // Minute: 6 bits
        // Second: 5 bits

        // do the bit shift as unsigned because the fields are unsigned, but
        // we can safely convert to int, because they won't be too big
        int year = (int)(ValidZipDate_YearMin + (zipDateTime >> 25));
        int month = (int)((zipDateTime >> 21) & 0xF);
        int day = (int)((zipDateTime >> 16) & 0x1F);
        int hour = (int)((zipDateTime >> 11) & 0x1F);
        int minute = (int)((zipDateTime >> 5) & 0x3F);
        int second = (int)((zipDateTime & 0x001F) * 2); // only 5 bits for second, so we only have a granularity of 2 sec.

        try
        {
            return new DateTime(year, month, day, hour, minute, second);
        }
        // Note: This is where my mischievous little "'System.ArgumentOutOfRangeException' in mscorlib.dll"
        // is coming from. Turns out the perf penalty was inconsequential even when this was being done for
        // every single entry, but now that I'm only doing it for a handful per FM, it's not worth worrying
        // about at all.
        catch (ArgumentOutOfRangeException)
        {
            return _invalidDateIndicator;
        }
    }

    #region Zip safety

    private static string GetZipSafetyFailMessage(string fileName, string full) =>
        $"Extracting this file would result in it being outside the intended folder (malformed/malicious filename?).{NL}" +
        "Entry full file name: " + fileName + $"{NL}" +
        "Path where it wanted to end up: " + full;

    // @ZipSafety: Make sure all calls to this method are handling the possible exception here! (looking at you, FMBackupAndRestore)
    // @ZipSafety: The possibility of forgetting to call this method is a problem. Architect it to reduce the likelihood somehow?
    /// <summary>
    /// Zip Slip prevention.
    /// </summary>
    /// <param name="path"></param>
    /// <param name="fileName"></param>
    /// <returns></returns>
    /// <exception cref="IOException"></exception>
    public static string GetExtractedNameOrThrowIfMalicious(string path, string fileName)
    {
        // Path.GetFullPath() incurs a very small perf hit (60ms on a 26 second extract), so don't worry about it.
        // This is basically what ZipFileExtensions.ExtractToDirectory() does.

        if (path.Length > 0 && !path[^1].IsDirSep())
        {
            path += "\\";
        }

        string extractedName = Path.Combine(path, fileName);
        string full = Path.GetFullPath(extractedName);

        if (full.PathStartsWithI(path))
        {
            return full;
        }
        else
        {
            ThrowHelper.IOException(GetZipSafetyFailMessage(fileName, full));
            return "";
        }
    }

    /// <summary>
    /// Zip Slip prevention. For when you just want to ignore it and not extract the file, rather than fail the
    /// whole operation.
    /// </summary>
    /// <param name="path"></param>
    /// <param name="fileName"></param>
    /// <param name="result"></param>
    /// <returns></returns>
    public static bool TryGetExtractedNameOrFailIfMalicious(string path, string fileName, out string result)
    {
        // Path.GetFullPath() incurs a very small perf hit (60ms on a 26 second extract), so don't worry about it.
        // This is basically what ZipFileExtensions.ExtractToDirectory() does.

        try
        {
            if (path.Length > 0 && !path[^1].IsDirSep())
            {
                path += "\\";
            }

            string extractedName = Path.Combine(path, fileName);
            string full = Path.GetFullPath(extractedName);

            if (full.PathStartsWithI(path))
            {
                result = extractedName;
                return true;
            }
            else
            {
                Logger.Log(GetZipSafetyFailMessage(fileName, full), stackTrace: true);
                result = "";
                return false;
            }
        }
        catch
        {
            result = "";
            return false;
        }
    }

    #endregion
}
