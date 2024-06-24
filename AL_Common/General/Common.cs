using System;
using System.Globalization;
using System.Net;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using JetBrains.Annotations;

namespace AL_Common;

[PublicAPI]
public static partial class Common
{
    public const int MAX_PATH = 260;

    #region Fields / classes

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

    // It's supposed to always be "{\rtf1", but some files have no number or some other number...
    // RichTextBox also only checks for "{\rtf", so we're fine here.
    public static readonly byte[] RTFHeaderBytes = @"{\rtf"u8.ToArray();

    public static readonly byte[] MAPPARAM = "MAPPARAM"u8.ToArray();

    #region Preset char arrays

    // Perf, for passing to Split(), Trim() etc. so we don't allocate all the time
    public static readonly string[] SA_CRLF = { "\r\n" };
    public static readonly char[] CA_CommaSemicolon = { ',', ';' };
    public static readonly char[] CA_CommaSpace = { ',', ' ' };
    public static readonly char[] CA_BS_FS = { '\\', '/' };
    public static readonly char[] CA_BS_FS_Space = { '\\', '/', ' ' };

    #endregion

    public const RegexOptions IgnoreCaseInvariant = RegexOptions.IgnoreCase | RegexOptions.CultureInvariant;

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

    /// <summary>
    /// Shorthand for <paramref name="value"/>.ToString(<see cref="NumberFormatInfo.InvariantInfo"/>)
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ToStrInv(this int value) => value.ToString(NumberFormatInfo.InvariantInfo);

    /// <summary>
    /// Shorthand for <paramref name="value"/>.ToString(<see cref="NumberFormatInfo.InvariantInfo"/>)
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ToStrInv(this byte value) => value.ToString(NumberFormatInfo.InvariantInfo);

    /// <summary>
    /// Shorthand for <paramref name="value"/>.ToString(<see cref="CultureInfo.CurrentCulture"/>)
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ToStrCur(this int value) => value.ToString(CultureInfo.CurrentCulture);

    /// <summary>
    /// Shorthand for <paramref name="value"/>.ToString(<see cref="CultureInfo.CurrentCulture"/>)
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ToStrCur(this double value) => value.ToString(CultureInfo.CurrentCulture);

    public static string GetPlainInnerText(this XmlNode? node) => node == null ? "" : WebUtility.HtmlDecode(node.InnerText);

    #endregion

    // .NET Core changed the Encoding.Default return value from legacy ANSI to UTF8. This is disastrous for us
    // because we need to read and write certain files that are written in Framework Encoding.Default (ANSI).
    // So implement a manual version here...

    [LibraryImport("kernel32.dll")]
    internal static partial int GetACP();

    public static Encoding GetLegacyDefaultEncoding()
    {
        try
        {
            // .NET Framework simply calls GetACP() for Encoding.Default, so let's try that first.
            return Encoding.GetEncoding(GetACP());
        }
        catch
        {
            try
            {
                // @NET5(Encoding.Default): I think InstalledUICulture is the actual one we want?
                // CurrentCulture will normally match it I think, but to be safe we should be explicit...
                // Unless I'm wrong, but hopefully someone will report the bug again if I am?
                return Encoding.GetEncoding(CultureInfo.InstalledUICulture.TextInfo.ANSICodePage);
            }
            catch (Exception ex)
            {
                // @NET5(Encoding.Default): If we get here, we should maybe put up a dialog, because this is a bad situation.
                // It will lead to a recurrence of the bug where cam_mod.ini is written with the wrong encoding
                // and non-ASCII game paths won't be read and FMs can't be played.
                Logger.Log(
                    "Unable to get the system default ANSI encoding, which is required for reading and writing certain files correctly.\r\n" +
                    "Returning .NET default encoding, which will probably be UTF8 and may cause issues for locales outside North America.",
                    ex);
                return Encoding.Default;
            }
        }
    }
}
