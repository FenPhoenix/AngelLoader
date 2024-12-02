// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace AL_Common.NETM_IO.Strategies
{
    internal abstract class FileStreamStrategy : Stream
    {
        /// <summary>Validates arguments provided to reading and writing methods on <see cref="Stream"/>.</summary>
        /// <param name="buffer">The array "buffer" argument passed to the reading or writing method.</param>
        /// <param name="offset">The integer "offset" argument passed to the reading or writing method.</param>
        /// <param name="count">The integer "count" argument passed to the reading or writing method.</param>
        /// <exception cref="ArgumentNullException"><paramref name="buffer"/> was null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="offset"/> was outside the bounds of <paramref name="buffer"/>, or
        /// <paramref name="count"/> was negative, or the range specified by the combination of
        /// <paramref name="offset"/> and <paramref name="count"/> exceed the length of <paramref name="buffer"/>.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected static void ValidateBufferArguments(byte[] buffer, int offset, int count)
        {
            if (buffer is null)
            {
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument_NET.buffer);
            }

            if (offset < 0)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument_NET.offset, ExceptionResource_NET.ArgumentOutOfRange_NeedNonNegNum);
            }

            if ((uint)count > buffer.Length - offset)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument_NET.count, ExceptionResource_NET.Argument_InvalidOffLen);
            }
        }

        internal abstract bool IsAsync { get; }

        internal bool IsDerived { get; init; }

        internal abstract string Name { get; }

        internal abstract AL_SafeFileHandle AL_SafeFileHandle { get; }

        internal IntPtr Handle => AL_SafeFileHandle.DangerousGetHandle();

        internal abstract bool IsClosed { get; }

        internal abstract void Lock(long position, long length);

        internal abstract void Unlock(long position, long length);

        internal abstract void Flush(bool flushToDisk);

        internal void DisposeInternal(bool disposing) => Dispose(disposing);
    }
}
