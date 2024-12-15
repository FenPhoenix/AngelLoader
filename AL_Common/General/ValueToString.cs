using System.Globalization;
using System.Runtime.CompilerServices;

namespace AL_Common;

public static partial class Common
{
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
    /// Shorthand for <paramref name="value"/>.ToString(<see cref="NumberFormatInfo.CurrentInfo"/>)
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ToStrCur(this int value) => value.ToString(NumberFormatInfo.CurrentInfo);

    /// <summary>
    /// Shorthand for <paramref name="value"/>.ToString(<see cref="NumberFormatInfo.CurrentInfo"/>)
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ToStrCur(this double value) => value.ToString(NumberFormatInfo.CurrentInfo);
}
