using System;
using System.Text;
using AL_Common;
using static System.StringComparison;

namespace AngelLoader;

public static partial class Utils
{
    #region StartsWith and EndsWith

    internal static bool StartsWithFast_NoNullChecks(this string str, string value)
    {
        if (str.Length < value.Length) return false;

        for (int i = 0; i < value.Length; i++)
        {
            if (str[i] != value[i]) return false;
        }

        return true;
    }

    /// <summary>
    /// StartsWith (case-insensitive). Uses a fast ASCII compare where possible.
    /// </summary>
    /// <param name="str"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    internal static bool StartsWithI(this string str, string value) => StartsWithOrEndsWithIFast(str, value, start: true);

    /// <summary>
    /// EndsWith (case-insensitive). Uses a fast ASCII compare where possible.
    /// </summary>
    /// <param name="str"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    internal static bool EndsWithI(this string str, string value) => StartsWithOrEndsWithIFast(str, value, start: false);

    private static bool StartsWithOrEndsWithIFast(this string str, string value, bool start)
    {
        if (str.IsEmpty() || str.Length < value.Length) return false;

        int siStart = start ? 0 : str.Length - value.Length;
        int siEnd = start ? value.Length : str.Length;

        for (int si = siStart, vi = 0; si < siEnd; si++, vi++)
        {
            if (value[vi] > 127)
            {
                return start
                    ? str.StartsWith(value, OrdinalIgnoreCase)
                    : str.EndsWith(value, OrdinalIgnoreCase);
            }

            if (!str[si].EqualsIAscii(value[vi])) return false;
        }

        return true;
    }

    #region Starts with plus whitespace

    #region Disabled until needed

#if false
    internal static bool StartsWithPlusWhiteSpace(this string str, string value)
    {
        int valLen;
        return str.StartsWithO(value) &&
               str.Length > (valLen = value.Length) &&
               char.IsWhiteSpace(str[valLen]);
    }

    internal static bool StartsWithPlusWhiteSpace(this string str, string value, int valueLength) =>
        str.StartsWithO(value) &&
        str.Length > valueLength &&
        char.IsWhiteSpace(str[valueLength]);
#endif

    #endregion

    internal static bool StartsWithIPlusWhiteSpace(this string str, string value)
    {
        int valLen;
        return str.StartsWithI(value) &&
               str.Length > (valLen = value.Length) &&
               char.IsWhiteSpace(str[valLen]);
    }

    internal static bool StartsWithIPlusWhiteSpace(this string str, string value, int valueLength) =>
        str.StartsWithI(value) &&
        str.Length > valueLength &&
        char.IsWhiteSpace(str[valueLength]);

    #endregion

    #endregion

    #region FM installed name conversion

    /// <summary>
    /// Format an FM archive name to conform to NewDarkLoader's FM install directory name requirements.
    /// </summary>
    /// <param name="archiveName">Filename without path or extension.</param>
    /// <param name="truncate">Whether to truncate the name to 30 characters or less.</param>
    /// <returns></returns>
    internal static string ToInstDirNameNDL(this string archiveName, bool truncate = true) => ToInstDirName(archiveName, "+.~ ", truncate);

    /// <summary>
    /// Format an FM archive name to conform to FMSel's FM install directory name requirements.
    /// </summary>
    /// <param name="archiveName">Filename without path or extension.</param>
    /// <param name="truncate">Whether to truncate the name to 30 characters or less.</param>
    /// <returns></returns>
    internal static string ToInstDirNameFMSel(this string archiveName, bool truncate) => ToInstDirName(archiveName, "+;:.,<>?*~| ", truncate);

    // @THREADING(_toInstDirNameSB): Not thread-safe
    private static readonly StringBuilder _toInstDirNameSB = new(30);
    private static string ToInstDirName(string archiveName, string illegalChars, bool truncate)
    {
        int count = archiveName.LastIndexOf('.');
        if (truncate)
        {
            if (count is -1 or > 30) count = Math.Min(archiveName.Length, 30);
        }
        else
        {
            if (count == -1) count = archiveName.Length;
        }

        _toInstDirNameSB.Clear();
        _toInstDirNameSB.Append(archiveName, 0, count);
        for (int i = 0; i < illegalChars.Length; i++) _toInstDirNameSB.Replace(illegalChars[i], '_');

        return _toInstDirNameSB.ToString();
    }

    #endregion

    internal static string ToSingleLineComment(this string value, int maxLength)
    {
        if (value.IsEmpty()) return "";

        int linebreakIndex = value.IndexOf("\r\n", InvariantCulture);

        return linebreakIndex > -1 && linebreakIndex <= maxLength
            ? value.Substring(0, linebreakIndex)
            : value.Substring(0, Math.Min(value.Length, maxLength));
    }

    #region Escaping

    internal static string FromRNEscapes(this string value) => value.Replace(@"\r\n", "\r\n").Replace(@"\\", "\\");

    internal static string ToRNEscapes(this string value) => value.Replace("\\", @"\\").Replace("\r\n", @"\r\n");

    /// <summary>
    /// For text that goes in menus: "&" is a reserved character, so escape "&" to "&&"
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    internal static string EscapeAmpersands(this string value) => value.Replace("&", "&&");

    /// <summary>
    /// Just puts a \ in front of each character in the string.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    internal static string EscapeAllChars(this string value)
    {
        if (value.IsEmpty()) return "";

        string ret = "";
        foreach (char c in value) ret += '\\' + c.ToString();

        return ret;
    }

    #endregion
}
