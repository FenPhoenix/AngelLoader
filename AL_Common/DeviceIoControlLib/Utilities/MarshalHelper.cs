using System;
using System.Runtime.InteropServices;

namespace AL_Common.DeviceIoControlLib.Utilities;

internal static class MarshalHelper
{
    public static T ToStructure<T>(this IntPtr ptr)
    {
#if !NETFRAMEWORK
        return Marshal.PtrToStructure<T>(ptr);
#else
        return (T)Marshal.PtrToStructure(ptr, typeof(T));
#endif
    }

    public static uint SizeOf<T>()
    {
#if !NETFRAMEWORK
        return (uint)Marshal.SizeOf<T>();
#else
        return (uint)Marshal.SizeOf(typeof(T));
#endif
    }
}
