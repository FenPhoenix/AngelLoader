using System;

namespace SharpCompress.Common;

internal abstract class FlagUtility
{
    /// <summary>
    /// Returns true if the flag is set on the specified bit field.
    /// Currently only works with 32-bit bitfields.
    /// </summary>
    /// <typeparam name="T">Enumeration with Flags attribute</typeparam>
    /// <param name="bitField">Flagged variable</param>
    /// <param name="flag">Flag to test</param>
    /// <returns></returns>
    public static bool HasFlag<T>(T bitField, T flag)
        where T : struct => HasFlag(Convert.ToInt64(bitField), Convert.ToInt64(flag));

    /// <summary>
    /// Returns true if the flag is set on the specified bit field.
    /// Currently only works with 32-bit bitfields.
    /// </summary>
    /// <param name="bitField">Flagged variable</param>
    /// <param name="flag">Flag to test</param>
    /// <returns></returns>
    public static bool HasFlag(long bitField, long flag) => ((bitField & flag) == flag);
}
