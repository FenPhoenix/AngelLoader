using System;
using System.Runtime.InteropServices;

namespace AL_Common.DeviceIoControlLib.Objects.Storage;

[StructLayout(LayoutKind.Sequential)]
public struct STORAGE_PROPERTY_QUERY
{
    public STORAGE_PROPERTY_ID PropertyId;
    public STORAGE_QUERY_TYPE QueryType;
    // @NET5/IMPORTANT/BUG: This is part of a setup that may be incorrect and cause crashes on some systems!
    //                      The Framework version is a byte[].
    public IntPtr AdditionalParameters;
}
