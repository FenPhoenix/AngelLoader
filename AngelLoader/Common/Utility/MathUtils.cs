using System;

namespace AngelLoader;

public static partial class Utils
{
    #region Clamping

    internal static float ClampToRichTextBoxZoomMinMax(this float value) => value.Clamp(0.1f, 5.0f);

    internal static float ClampToFMsDGVFontSizeMinMax(this float value)
    {
        if (value < Math.Round(1.00f, 2)) value = 1.00f;
        if (value > Math.Round(41.25f, 2)) value = 41.25f;
        return (float)Math.Round(value, 2);
    }

    internal static int SetRatingClamped(this int rating) => rating.Clamp(-1, 10);

    #endregion

    internal static int MathMax3(int num1, int num2, int num3) => Math.Max(Math.Max(num1, num2), num3);

    internal static int MathMax4(int num1, int num2, int num3, int num4) => Math.Max(Math.Max(Math.Max(num1, num2), num3), num4);
}
