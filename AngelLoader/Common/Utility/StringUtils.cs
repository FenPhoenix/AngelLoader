using System;
using AL_Common;
using static System.StringComparison;

namespace AngelLoader;

public static partial class Utils
{
    #region StartsWith and EndsWith

    internal static bool StartsWithFast(this string str, string value)
    {
        int valueLength = value.Length;
        if (str.Length < valueLength) return false;

        for (int i = 0; i < valueLength; i++)
        {
            if (str[i] != value[i]) return false;
        }

        return true;
    }

    #region Starts with plus whitespace

    internal static bool StartsWithPlusWhiteSpace(this string str, string value)
    {
        int valLen;
        return str.StartsWithO(value) &&
               str.Length > (valLen = value.Length) &&
               char.IsWhiteSpace(str[valLen]);
    }

    #region Disabled until needed

#if false
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

    internal static bool TryGetValueO(this string line, string key, out string value)
    {
        if (line.StartsWithFast(key))
        {
            value = line.Substring(key.Length);
            return true;
        }
        else
        {
            value = "";
            return false;
        }
    }

    internal static bool TryGetValueI(this string line, string key, out string value)
    {
        if (line.StartsWithI(key))
        {
            value = line.Substring(key.Length);
            return true;
        }
        else
        {
            value = "";
            return false;
        }
    }

    internal static string ToSingleLineComment(this string value)
    {
        const int maxLength = 100;

        if (value.IsEmpty()) return "";

        int linebreakIndex = value.IndexOf("\r\n", InvariantCulture);

        return linebreakIndex is > -1 and <= maxLength
            ? value.Substring(0, linebreakIndex)
            : value.Substring(0, Math.Min(value.Length, maxLength));
    }

    #region Escaping

    internal static string FromRNEscapes(this string value) => value.Replace(@"\r\n", "\r\n").Replace(@"\\", "\\");

    internal static string ToRNEscapes(this string value) => value.Replace("\\", @"\\").Replace("\r\n", @"\r\n");

    /// <summary>
    /// Just puts a \ in front of each character in the string.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    internal static string EscapeAllChars(this string value)
    {
        if (value.IsEmpty()) return "";

        string ret = "";
        foreach (char c in value)
        {
            ret += '\\' + c.ToString();
        }

        return ret;
    }

    #endregion

    internal static string NormalizeToCRLF(this string text)
    {
        if (!text.Contains('\r'))
        {
            text = text.Replace("\n", "\r\n");
        }

        return text;
    }
}
