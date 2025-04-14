using System;
using System.Runtime.InteropServices;

namespace AL_Common.DeviceIoControlLib.Objects.Storage;

[StructLayout(LayoutKind.Sequential)]
public struct STORAGE_PROPERTY_QUERY
{
    public STORAGE_PROPERTY_ID PropertyId;
    public STORAGE_QUERY_TYPE QueryType;
    public nint AdditionalParameters;
}
