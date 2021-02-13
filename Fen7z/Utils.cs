using System;
using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;

namespace Fen7z
{
    internal static class Utils
    {
        #region Empty / whitespace checks

        /// <summary>
        /// Returns true if <paramref name="value"/> is null or empty.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        [ContractAnnotation("null => true")]
        internal static bool IsEmpty([NotNullWhen(false)] this string? value) => string.IsNullOrEmpty(value);

        /// <summary>
        /// Returns true if <paramref name="value"/> is null, empty, or whitespace.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        [ContractAnnotation("null => true")]
        internal static bool IsWhiteSpace([NotNullWhen(false)] this string? value) => string.IsNullOrWhiteSpace(value);

        #endregion

        internal static int GetPercentFromValue(int current, int total) => (100 * current) / total;

        internal static T Clamp<T>(this T value, T min, T max) where T : IComparable<T> =>
            value.CompareTo(min) < 0 ? min : value.CompareTo(max) > 0 ? max : value;
    }
}
