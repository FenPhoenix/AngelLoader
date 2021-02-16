using System;
using Gma.System.MouseKeyHook;

namespace DarkUI
{
    internal static class Global
    {
        // Only one copy of the hook
        internal static IMouseEvents MouseHook;

        internal static double GetPercentFromValue(int current, int total) => (double)(100 * current) / total;
        internal static int GetValueFromPercent(double percent, int total) => (int)((percent / 100) * total);

        internal static int SignedHIWORD(int n) => (int)(short)HIWORD(n);

        internal static int SignedLOWORD(int n) => (int)(short)LOWORD(n);

        internal static int SignedHIWORD(IntPtr n) => SignedHIWORD(unchecked((int)(long)n));

        internal static int SignedLOWORD(IntPtr n) => SignedLOWORD(unchecked((int)(long)n));

        internal static int HIWORD(int n) => (n >> 16) & 0xffff;

        internal static int LOWORD(int n) => n & 0xffff;

        internal static T Clamp<T>(this T value, T min, T max) where T : IComparable<T> =>
            value.CompareTo(min) < 0 ? min : value.CompareTo(max) > 0 ? max : value;
    }
}
