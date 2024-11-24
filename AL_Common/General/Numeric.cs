using System;
using System.Globalization;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace AL_Common;

public static partial class Common
{
    #region Clamping

    /// <summary>
    /// Clamps a number to between <paramref name="min"/> and <paramref name="max"/>.
    /// </summary>
    /// <param name="value"></param>
    /// <param name="min"></param>
    /// <param name="max"></param>
    /// <returns></returns>
    public static T Clamp<T>(this T value, T min, T max) where T : INumber<T> => T.Clamp(value, min, max);

    /// <summary>
    /// If <paramref name="value"/> is less than zero, returns zero. Otherwise, returns <paramref name="value"/>
    /// unchanged.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T ClampToZero<T>(this T value) where T : INumber<T> => T.Max(value, T.Zero);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float ClampZeroToOne(this float value) => value.Clamp(0, 1.0f);

    /// <summary>
    /// Clamps a number to <paramref name="min"/>.
    /// </summary>
    /// <param name="value"></param>
    /// <param name="min"></param>
    /// <returns></returns>
    public static T ClampToMin<T>(this T value, T min) where T : INumber<T> => T.Max(value, min);

    #endregion

    #region Percent

    public static int GetPercentFromValue_Int(int current, int total) => total == 0 ? 0 : (100 * current) / total;
    public static int GetPercentFromValue_ULong(ulong current, ulong total) => (int)(total == 0 ? 0 : (100 * current) / total);
    public static float GetValueFromPercent_Float(float percent, int total) => (percent / 100f) * total;
    public static int GetValueFromPercent_Int(double percent, int total) => (int)((percent / 100d) * total);
#if false
    public static float GetPercentFromValue_Float(long current, long total) => total == 0 ? 0 : (float)(100 * current) / total;
    public static double GetPercentFromValue_Double(int current, int total) => total == 0 ? 0 : (double)(100 * current) / total;
    public static long GetValueFromPercent(double percent, long total) => (long)((percent / 100) * total);
    public static int GetValueFromPercent(double percent, int total) => (int)((percent / 100d) * total);
    public static int GetValueFromPercent_Rounded(double percent, int total) => (int)Math.Round((percent / 100d) * total, 1, MidpointRounding.AwayFromZero);
    public static double GetValueFromPercent_Double(double percent, int total) => (percent / 100d) * total;
#endif

    #endregion

    #region TryParse Invariant

    /// <summary>
    /// Calls <see langword="float"/>.TryParse(<paramref name="s"/>, <see cref="NumberStyles.Float"/>, <see cref="NumberFormatInfo.InvariantInfo"/>, out <see langword="float"/> <paramref name="result"/>);
    /// </summary>
    /// <param name="s">A string representing a number to convert.</param>
    /// <param name="result"></param>
    /// <exception cref="ArgumentException"></exception>
    /// <returns><see langword="true"/> if <paramref name="s"/> was converted successfully; otherwise, <see langword="false"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Float_TryParseInv(ReadOnlySpan<char> s, out float result)
    {
        return float.TryParse(s, NumberStyles.Float, NumberFormatInfo.InvariantInfo, out result);
    }

    /// <summary>
    /// Calls <see langword="int"/>.TryParse(<paramref name="s"/>, <see cref="NumberStyles.Integer"/>, <see cref="NumberFormatInfo.InvariantInfo"/>, out <see langword="int"/> <paramref name="result"/>);
    /// </summary>
    /// <param name="s">A string representing a number to convert.</param>
    /// <param name="result"></param>
    /// <exception cref="ArgumentException"></exception>
    /// <returns><see langword="true"/> if <paramref name="s"/> was converted successfully; otherwise, <see langword="false"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Int_TryParseInv(string s, out int result)
    {
        return int.TryParse(s, NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out result);
    }

    /// <summary>
    /// Calls <see langword="int"/>.TryParse(<paramref name="s"/>, <see cref="NumberStyles.Integer"/>, <see cref="NumberFormatInfo.InvariantInfo"/>, out <see langword="int"/> <paramref name="result"/>);
    /// </summary>
    /// <param name="s">A string representing a number to convert.</param>
    /// <param name="result"></param>
    /// <exception cref="ArgumentException"></exception>
    /// <returns><see langword="true"/> if <paramref name="s"/> was converted successfully; otherwise, <see langword="false"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Int_TryParseInv(ReadOnlySpan<char> s, out int result)
    {
        return int.TryParse(s, NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out result);
    }

    /// <summary>
    /// Calls <see langword="uint"/>.TryParse(<paramref name="s"/>, <see cref="NumberStyles.Integer"/>, <see cref="NumberFormatInfo.InvariantInfo"/>, out <see langword="uint"/> <paramref name="result"/>);
    /// </summary>
    /// <param name="s">A string representing a number to convert.</param>
    /// <param name="result"></param>
    /// <exception cref="ArgumentException"></exception>
    /// <returns><see langword="true"/> if <paramref name="s"/> was converted successfully; otherwise, <see langword="false"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool UInt_TryParseInv(string s, out uint result)
    {
        return uint.TryParse(s, NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out result);
    }

    /// <summary>
    /// Calls <see langword="uint"/>.TryParse(<paramref name="s"/>, <see cref="NumberStyles.Integer"/>, <see cref="NumberFormatInfo.InvariantInfo"/>, out <see langword="uint"/> <paramref name="result"/>);
    /// </summary>
    /// <param name="s">A string representing a number to convert.</param>
    /// <param name="result"></param>
    /// <exception cref="ArgumentException"></exception>
    /// <returns><see langword="true"/> if <paramref name="s"/> was converted successfully; otherwise, <see langword="false"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool UInt_TryParseInv(ReadOnlySpan<char> s, out uint result)
    {
        return uint.TryParse(s, NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out result);
    }

    #endregion
}
