using System;
using System.Runtime.InteropServices;

namespace AL_Common.DeviceIoControlLib.Utilities;

internal static class MarshalHelper
{
    public static T ToStructure<T>(this nint ptr)
    {
        return Marshal.PtrToStructure<T>(ptr)!;
    }

    public static uint SizeOf<T>()
    {
        return (uint)Marshal.SizeOf<T>();
    }
}
