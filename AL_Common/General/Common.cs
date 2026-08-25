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

    public const int MaxArrayLength = 2146435071;

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
        /// <summary>
        /// Used for solid archives only.
        /// </summary>
        public readonly long TotalExtractionCost;

        public NameAndIndex(string name, int index, long totalExtractionCost)
        {
            Name = name;
            Index = index;
            TotalExtractionCost = totalExtractionCost;
        }

        public NameAndIndex(string name, int index)
        {
            Name = name;
            Index = index;
            TotalExtractionCost = 0;
        }

        public NameAndIndex(string name)
        {
            Name = name;
            Index = -1;
            TotalExtractionCost = 0;
        }
    }

    public readonly struct RtfColor
    {
        public readonly byte R;
        public readonly byte G;
        public readonly byte B;
        public readonly bool IsDefaultColor;

        public RtfColor(byte r, byte g, byte b)
        {
            R = r;
            G = g;
            B = b;
            IsDefaultColor = false;
        }

        public RtfColor(byte r, byte g, byte b, bool isDefaultColor)
        {
            R = r;
            G = g;
            B = b;
            IsDefaultColor = isDefaultColor;
        }
    }

    #endregion

    #region Methods

    public static bool EqualsIfNotNull(this object? obj1, object? obj2) => obj1 != null && obj2 != null && obj1 == obj2;

    public static string GetPlainInnerText(this XmlNode? node) => node == null ? "" : WebUtility.HtmlDecode(node.InnerText);

    /// <summary>
    /// Use this to run a function to initialize a field without having to create a standalone function.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="initFunc"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T RunFunc<T>(Func<T> initFunc) => initFunc.Invoke();

    /// <summary>Indicates whether a <see langword="char"/> is within the specified inclusive range.</summary>
    /// <param name="value">The <see langword="char"/> to evaluate.</param>
    /// <param name="minInclusive">The lower bound, inclusive.</param>
    /// <param name="maxInclusive">The upper bound, inclusive.</param>
    /// <returns>true if <paramref name="value"/> is within the specified range; otherwise, false.</returns>
    /// <remarks>
    /// The method does not validate that <paramref name="maxInclusive"/> is greater than or equal
    /// to <paramref name="minInclusive"/>.  If <paramref name="maxInclusive"/> is less than
    /// <paramref name="minInclusive"/>, the behavior is undefined.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsBetween(char value, char minInclusive, char maxInclusive) =>
        (uint)(value - minInclusive) <= (uint)(maxInclusive - minInclusive);

    /// <summary>Indicates whether an <see langword="int"/> is within the specified inclusive range.</summary>
    /// <param name="value">The <see langword="int"/> to evaluate.</param>
    /// <param name="minInclusive">The lower bound, inclusive.</param>
    /// <param name="maxInclusive">The upper bound, inclusive.</param>
    /// <returns>true if <paramref name="value"/> is within the specified range; otherwise, false.</returns>
    /// <remarks>
    /// The method does not validate that <paramref name="maxInclusive"/> is greater than or equal
    /// to <paramref name="minInclusive"/>.  If <paramref name="maxInclusive"/> is less than
    /// <paramref name="minInclusive"/>, the behavior is undefined.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsBetween(this int value, int minInclusive, int maxInclusive) =>
        (uint)(value - minInclusive) <= (uint)(maxInclusive - minInclusive);

    /// <summary>Indicates whether a <see langword="uint"/> is within the specified inclusive range.</summary>
    /// <param name="value">The <see langword="uint"/> to evaluate.</param>
    /// <param name="minInclusive">The lower bound, inclusive.</param>
    /// <param name="maxInclusive">The upper bound, inclusive.</param>
    /// <returns>true if <paramref name="value"/> is within the specified range; otherwise, false.</returns>
    /// <remarks>
    /// The method does not validate that <paramref name="maxInclusive"/> is greater than or equal
    /// to <paramref name="minInclusive"/>.  If <paramref name="maxInclusive"/> is less than
    /// <paramref name="minInclusive"/>, the behavior is undefined.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsBetween(this uint value, int minInclusive, int maxInclusive) =>
        (uint)(value - minInclusive) <= (uint)(maxInclusive - minInclusive);

    /// <summary>Indicates whether a <see langword="ushort"/> is within the specified inclusive range.</summary>
    /// <param name="value">The <see langword="ushort"/> to evaluate.</param>
    /// <param name="minInclusive">The lower bound, inclusive.</param>
    /// <param name="maxInclusive">The upper bound, inclusive.</param>
    /// <returns>true if <paramref name="value"/> is within the specified range; otherwise, false.</returns>
    /// <remarks>
    /// The method does not validate that <paramref name="maxInclusive"/> is greater than or equal
    /// to <paramref name="minInclusive"/>.  If <paramref name="maxInclusive"/> is less than
    /// <paramref name="minInclusive"/>, the behavior is undefined.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsBetween(this ushort value, int minInclusive, int maxInclusive) =>
        (uint)(value - minInclusive) <= (uint)(maxInclusive - minInclusive);

    #endregion
}
