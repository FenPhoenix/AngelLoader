using System;
using System.Collections.Generic;

namespace AL_Common;

public static partial class Common
{
    #region Classes

    private static readonly PathComparer _pathComparer = new();
    public sealed class PathComparer : StringComparer
    {
        // Allocations here, but this doesn't ever seem to get hit for Add() or Contains() calls
        public override int Compare(string? x, string? y)
        {
            return x == y ? 0 :
                x == null ? -1 :
                y == null ? 1 :
                string.Compare(x.ToBackSlashes(), y.ToBackSlashes(), StringComparison.OrdinalIgnoreCase);
        }

        public override bool Equals(string? x, string? y)
        {
            if (x == y) return true;
            if (x == null || y == null) return false;

            return x.PathEqualsI(y);
        }

        // @MEM(PathComparer/GetHashCode): We allocate if the string is not already backslash separators
        public override int GetHashCode(string obj) => obj == null
            ? throw new ArgumentNullException(nameof(obj))
            : OrdinalIgnoreCase.GetHashCode(obj.ToBackSlashes());
    }

    /// <summary>
    /// A HashSet&lt;<see cref="string"/>&gt; where lookups are case-insensitive and directory separator-insensitive
    /// </summary>
    public sealed class HashSetPathI : HashSet<string>
    {
        public HashSetPathI() : base(_pathComparer) { }

        public HashSetPathI(int capacity) : base(capacity, _pathComparer) { }

        public HashSetPathI(IEnumerable<string> collection) : base(collection, _pathComparer) { }

        /// <inheritdoc cref="HashSet{T}.Add"/>
        public new bool Add(string value) => base.Add(value.ToBackSlashes());
    }

    /// <summary>
    /// HashSet&lt;<see langword="string"/>&gt; that uses <see cref="StringComparer.OrdinalIgnoreCase"/> for equality comparison.
    /// </summary>
    public sealed class HashSetI : HashSet<string>
    {
        public HashSetI() : base(StringComparer.OrdinalIgnoreCase) { }

        public HashSetI(int capacity) : base(capacity, StringComparer.OrdinalIgnoreCase) { }

        public HashSetI(IEnumerable<string> collection) : base(collection, StringComparer.OrdinalIgnoreCase) { }
    }

    /// <summary>
    /// Dictionary&lt;<see langword="string"/>, TValue&gt; that uses <see cref="StringComparer.OrdinalIgnoreCase"/> for equality comparison.
    /// Since the key type will always be <see langword="string"/>, only the value type is specifiable.
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    public sealed class DictionaryI<TValue> : Dictionary<string, TValue>
    {
        public DictionaryI() : base(StringComparer.OrdinalIgnoreCase) { }

        public DictionaryI(int capacity) : base(capacity, StringComparer.OrdinalIgnoreCase) { }

#if false
        public DictionaryI(IDictionary<string, TValue> collection) : base(collection, StringComparer.OrdinalIgnoreCase) { }
#endif
    }

    /// <summary>
    /// Clears the dictionary and sets its internal storage to zero-length.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="dictionary"></param>
    /// <param name="capacity"></param>
    public static void Reset<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, int capacity) where TKey : notnull
    {
        dictionary.Clear();
        dictionary.TrimExcess(capacity);
    }

    #endregion
}
