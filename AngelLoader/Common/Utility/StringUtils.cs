using System;
using AL_Common;
using static System.StringComparison;

namespace AngelLoader;

public static partial class Utils
{
    #region StartsWith and EndsWith

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

    /// <summary>
    /// If you know the existing single line comment is empty, call this one for less work.
    /// </summary>
    /// <param name="value"></param>
    /// <param name="maxLength"></param>
    /// <returns></returns>
    internal static string ToSingleLineComment(this string value, int maxLength)
    {
        if (value.IsEmpty()) return "";

        int linebreakIndex = value.IndexOf("\r\n", InvariantCulture);

        return linebreakIndex > -1 && linebreakIndex <= maxLength
            ? value.Substring(0, linebreakIndex)
            : value.Substring(0, Math.Min(value.Length, maxLength));
    }

    /// <summary>
    /// If the existing single line comment might not be empty, call this one to ensure no extra allocations happen
    /// when the existing single line comment doesn't need updating.
    /// </summary>
    /// <param name="value"></param>
    /// <param name="oldSingleLine"></param>
    /// <param name="maxLength"></param>
    /// <returns></returns>
    internal static string ToSingleLineComment_AllocOnlyIfNeeded(this string value, string oldSingleLine, int maxLength)
    {
        if (value.IsEmpty()) return "";

        int linebreakIndex = value.IndexOf("\r\n", InvariantCulture);

        bool cutoffIsLineBreak = linebreakIndex > -1 && linebreakIndex <= maxLength;

        ReadOnlySpan<char> valueSpan =
            cutoffIsLineBreak
                ? value.AsSpan(0, linebreakIndex)
                : value.AsSpan(0, Math.Min(value.Length, maxLength));

        if (!valueSpan.SequenceEqual(oldSingleLine))
        {
            return cutoffIsLineBreak
                ? value.Substring(0, linebreakIndex)
                : value.Substring(0, Math.Min(value.Length, maxLength));
        }
        else
        {
            return oldSingleLine;
        }
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
