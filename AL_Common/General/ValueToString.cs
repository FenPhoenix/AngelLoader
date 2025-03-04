using System;
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
    public static string ToStrInv<T>(this T value) where T : IFormattable
    {
        return value.ToString(null, NumberFormatInfo.InvariantInfo);
    }

    /// <summary>
    /// Shorthand for <paramref name="value"/>.ToString(<see cref="NumberFormatInfo.CurrentInfo"/>)
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ToStrCur<T>(this T value) where T : IFormattable
    {
        return value.ToString(null, NumberFormatInfo.CurrentInfo);
    }
}
