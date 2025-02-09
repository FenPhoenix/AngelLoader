using System;
using System.Net;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Xml;
using JetBrains.Annotations;

namespace AL_Common;

[PublicAPI]
public static partial class Common
{
    #region Fields / classes

    public const int MAX_PATH = 260;

    public const RegexOptions Regex_IgnoreCaseInvariant = RegexOptions.IgnoreCase | RegexOptions.CultureInvariant;

    /// <summary>
    /// Shorthand for <see cref="Environment.NewLine"/>
    /// </summary>
    public static readonly string NL = Environment.NewLine;

    public const int StreamCopyBufferSize = 81920;
    public const int FileStreamBufferSize = 4096;

    public sealed class ProgressPercents
    {
        public int MainPercent;
        public int SubPercent;
    }

    // Class instead of enum so we don't have to keep casting its fields
    public static class ByteSize
    {
        public const int KB = 1024;
        public const int MB = KB * 1024;
        public const int GB = MB * 1024;
    }

    public static class ByteLengths
    {
        public const int Byte = 1;
        public const int Int16 = 2;
        public const int Int32 = 4;
        public const int Int64 = 8;
    }

    /// <summary>
    /// Stores a filename/index pair for quick lookups into a zip file.
    /// </summary>
    [StructLayout(LayoutKind.Auto)]
    public readonly struct NameAndIndex
    {
        public readonly string Name;
        public readonly int Index;

        public NameAndIndex(string name, int index)
        {
            Name = name;
            Index = index;
        }

        public NameAndIndex(string name)
        {
            Name = name;
            Index = -1;
        }
    }

    #endregion

    #region Methods

    public static bool EqualsIfNotNull(this object? obj1, object? obj2) => obj1 != null && obj2 != null && obj1 == obj2;

    /// <summary>
    /// Copy of .NET 7 version (fewer branches than Framework) but with a fast null return on fail instead of the infernal exception-throwing.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ListFast<char>? ConvertFromUtf32(uint utf32u, ListFast<char> charBuffer)
    {
        if (((utf32u - 0x110000u) ^ 0xD800u) < 0xFFEF0800u)
        {
            return null;
        }

        if (utf32u <= char.MaxValue)
        {
            charBuffer.ItemsArray[0] = (char)utf32u;
            charBuffer.Count = 1;
            return charBuffer;
        }

        charBuffer.ItemsArray[0] = (char)((utf32u + ((0xD800u - 0x40u) << 10)) >> 10);
        charBuffer.ItemsArray[1] = (char)((utf32u & 0x3FFu) + 0xDC00u);
        charBuffer.Count = 2;

        return charBuffer;
    }

    public static string GetPlainInnerText(this XmlNode? node) => node == null ? "" : WebUtility.HtmlDecode(node.InnerText);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T RunFunc<T>(Func<T> initFunc) => initFunc.Invoke();

    #endregion
}
