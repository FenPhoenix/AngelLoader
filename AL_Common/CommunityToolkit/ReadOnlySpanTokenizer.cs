// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace AL_Common.CommunityToolkit;

/// <summary>
/// A <see langword="ref"/> <see langword="struct"/> that tokenizes a given <see cref="ReadOnlySpan{T}"/> instance.
/// </summary>
/// <typeparam name="T">The type of items to enumerate.</typeparam>
[EditorBrowsable(EditorBrowsableState.Never)]
public ref struct ReadOnlySpanTokenizer<T> where T : IEquatable<T>
{
    /// <summary>
    /// The source <see cref="ReadOnlySpan{T}"/> instance.
    /// </summary>
    private readonly ReadOnlySpan<T> _span;

    /// <summary>
    /// The separator item to use.
    /// </summary>
    private readonly T _separator;

    /// <summary>
    /// The current initial offset.
    /// </summary>
    private int _start;

    /// <summary>
    /// The current final offset.
    /// </summary>
    private int _end;

    /// <summary>
    /// Initializes a new instance of the <see cref="ReadOnlySpanTokenizer{T}"/> struct.
    /// </summary>
    /// <param name="span">The source <see cref="ReadOnlySpan{T}"/> instance.</param>
    /// <param name="separator">The separator item to use.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReadOnlySpanTokenizer(ReadOnlySpan<T> span, T separator)
    {
        _span = span;
        _separator = separator;
        _start = 0;
        _end = -1;
    }

    /// <summary>
    /// Implements the duck-typed <see cref="IEnumerable{T}.GetEnumerator"/> method.
    /// </summary>
    /// <returns>An <see cref="ReadOnlySpanTokenizer{T}"/> instance targeting the current <see cref="ReadOnlySpan{T}"/> value.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly ReadOnlySpanTokenizer<T> GetEnumerator() => this;

    /// <summary>
    /// Implements the duck-typed <see cref="System.Collections.IEnumerator.MoveNext"/> method.
    /// </summary>
    /// <returns><see langword="true"/> whether a new element is available, <see langword="false"/> otherwise</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool MoveNext()
    {
        int newEnd = _end + 1;
        int length = _span.Length;

        // Additional check if the separator is not the last character
        if (newEnd <= length)
        {
            _start = newEnd;

            // We need to call this extension explicitly or the extension method resolution rules for the C# compiler
            // will end up picking CommunityToolkit.HighPerformance.ReadOnlySpanExtensions.IndexOf instead, even
            // though the latter takes the parameter via a readonly reference. This is because the "in" modifier is
            // implicit, which makes the signature compatible, and because extension methods are matched in such a
            // way that methods "closest" to where they're used are preferred. Since this type shares the same root
            // namespace, this makes that extension a better match, so that it overrides the MemoryExtensions one.
            // This is not a problem for consumers of this package, as their code would be outside of the
            // CommunityToolkit.HighPerformance namespace, so both extensions would be "equally distant", so that
            // when they're both in scope it will be possible to choose which one to use by adding an explicit "in".
            int index = MemoryExtensions.IndexOf(_span.Slice(newEnd), _separator);

            // Extract the current subsequence
            if (index >= 0)
            {
                _end = newEnd + index;

                return true;
            }

            _end = length;

            return true;
        }

        return false;
    }

    /// <summary>
    /// Gets the duck-typed <see cref="IEnumerator{T}.Current"/> property.
    /// </summary>
    public readonly ReadOnlySpan<T> Current
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _span.Slice(_start, _end - _start);
    }
}
