// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace AL_Common.RTF;

public sealed partial class RtfDisplayedReadmeParser
{
    internal static class PerTypeValues<T>
    {
        internal static readonly nint ArrayAdjustment = MeasureArrayAdjustment();

        // Array header sizes are a runtime implementation detail and aren't the same across all runtimes.
        // (The CLR made a tweak after 4.5, and Mono has an extra Bounds pointer.)
        private static nint MeasureArrayAdjustment()
        {
            T[] sampleArray = new T[1];
            return Unsafe.ByteOffset(ref Unsafe.As<Pinnable<T>>(sampleArray).Data, ref sampleArray[0]);
        }
    }

    //
    // This class exists solely so that arbitrary objects can be Unsafe-casted to it to get a ref to the start of
    // the user data.
    //
    [StructLayout(LayoutKind.Sequential)]
    internal sealed class Pinnable<T>
    {
        public T Data = default!;
    }

    private static readonly nint _groupStackFrame_ByteOffset = PerTypeValues<GroupStackFrame>.ArrayAdjustment;
    private static readonly nint _byte_ByteOffset = PerTypeValues<byte>.ArrayAdjustment;
    private static readonly nint _char_ByteOffset = PerTypeValues<char>.ArrayAdjustment;
    private static readonly nint _bool_ByteOffset = PerTypeValues<bool>.ArrayAdjustment;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ref GroupStackFrame GetArrayDataReference(GroupStackFrame[] array)
    {
        return ref Unsafe.AddByteOffset(ref Unsafe.As<Pinnable<GroupStackFrame>>(array).Data, _groupStackFrame_ByteOffset);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ref byte GetArrayDataReference(byte[] array)
    {
        return ref Unsafe.AddByteOffset(ref Unsafe.As<Pinnable<byte>>(array).Data, _byte_ByteOffset);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ref bool GetArrayDataReference(bool[] array)
    {
        return ref Unsafe.AddByteOffset(ref Unsafe.As<Pinnable<bool>>(array).Data, _bool_ByteOffset);
    }
}
