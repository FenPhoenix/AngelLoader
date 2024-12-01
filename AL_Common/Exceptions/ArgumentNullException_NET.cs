using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace AL_Common;

public static class ArgumentNullException_NET
{
    /// <summary>Throws an <see cref="ArgumentNullException"/> if <paramref name="argument"/> is null.</summary>
    /// <param name="argument">The reference type argument to validate as non-null.</param>
    /// <param name="paramName">The name of the parameter with which <paramref name="argument"/> corresponds.</param>
    public static void ThrowIfNull([NotNull] object? argument,
        [CallerArgumentExpression(nameof(argument))] string? paramName = null)
    {
        if (argument is null)
        {
            Throw(paramName);
        }
    }

    internal static void ThrowIfNull([NotNull] object? argument, ExceptionArgument_NET paramName)
    {
        if (argument is null)
        {
            ThrowHelper.ThrowArgumentNullException(paramName);
        }
    }

    /// <summary>Throws an <see cref="ArgumentNullException"/> if <paramref name="argument"/> is null.</summary>
    /// <param name="argument">The pointer argument to validate as non-null.</param>
    /// <param name="paramName">The name of the parameter with which <paramref name="argument"/> corresponds.</param>
    [CLSCompliant(false)]
    public static unsafe void ThrowIfNull([NotNull] void* argument, [CallerArgumentExpression(nameof(argument))] string? paramName = null)
    {
        if (argument is null)
        {
            Throw(paramName);
        }
    }

    /// <summary>Throws an <see cref="ArgumentNullException"/> if <paramref name="argument"/> is null.</summary>
    /// <param name="argument">The pointer argument to validate as non-null.</param>
    /// <param name="paramName">The name of the parameter with which <paramref name="argument"/> corresponds.</param>
    internal static unsafe void ThrowIfNull(IntPtr argument, [CallerArgumentExpression(nameof(argument))] string? paramName = null)
    {
        if (argument == IntPtr.Zero)
        {
            Throw(paramName);
        }
    }

    [DoesNotReturn]
    internal static void Throw(string? paramName) =>
        throw new ArgumentNullException(paramName);
}
