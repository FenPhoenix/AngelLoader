﻿using System;
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

    /// <summary>
    /// StartsWith (case-insensitive). Uses a fast ASCII compare where possible.
    /// </summary>
    /// <param name="str"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    internal static bool StartsWithI(this string str, string value)
    {
        if (str.IsEmpty()) return false;
        int valueLength = value.Length;
        if (str.Length < valueLength) return false;

        for (int si = 0, vi = 0; si < valueLength; si++, vi++)
        {
            char vc = value[vi];

            if (vc > 127)
            {
                return str.StartsWith(value, OrdinalIgnoreCase);
            }

            if (!str[si].EqualsIAscii(vc)) return false;
        }

        return true;
    }

    /// <summary>
    /// EndsWith (case-insensitive). Uses a fast ASCII compare where possible.
    /// </summary>
    /// <param name="str"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    internal static bool EndsWithI(this string str, string value)
    {
        if (str.IsEmpty()) return false;
        int strLength = str.Length;
        int valueLength = value.Length;
        if (strLength < valueLength) return false;

        int start = strLength - valueLength;

        for (int si = start, vi = 0; si < strLength; si++, vi++)
        {
            char vc = value[vi];

            if (vc > 127)
            {
                return str.EndsWith(value, OrdinalIgnoreCase);
            }

            if (!str[si].EqualsIAscii(vc)) return false;
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
