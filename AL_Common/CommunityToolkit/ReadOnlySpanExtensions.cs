// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Runtime.CompilerServices;

namespace AL_Common.CommunityToolkit;

/// <summary>
/// Helpers for working with the <see cref="ReadOnlySpan{T}"/> type.
/// </summary>
public static class ReadOnlySpanExtensions
{
    /// <summary>
    /// Tokenizes the values in the input <see cref="ReadOnlySpan{T}"/> instance using a specified separator.
    /// This extension should be used directly within a <see langword="foreach"/> loop:
    /// <code>
    /// ReadOnlySpan&lt;char&gt; text = "Hello, world!";
    ///
    /// foreach (var token in text.Tokenize(','))
    /// {
    ///     // Access the tokens here...
    /// }
    /// </code>
    /// The compiler will take care of properly setting up the <see langword="foreach"/> loop with the type returned from this method.
    /// </summary>
    /// <typeparam name="T">The type of items in the <see cref="ReadOnlySpan{T}"/> to tokenize.</typeparam>
    /// <param name="span">The source <see cref="ReadOnlySpan{T}"/> to tokenize.</param>
    /// <param name="separator">The separator <typeparamref name="T"/> item to use.</param>
    /// <returns>A wrapper type that will handle the tokenization for <paramref name="span"/>.</returns>
    /// <remarks>The returned <see cref="ReadOnlySpanTokenizer{T}"/> value shouldn't be used directly: use this extension in a <see langword="foreach"/> loop.</remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ReadOnlySpanTokenizer<T> Tokenize<T>(this ReadOnlySpan<T> span, T separator) where T : IEquatable<T>
    {
        return new(span, separator);
    }
}
