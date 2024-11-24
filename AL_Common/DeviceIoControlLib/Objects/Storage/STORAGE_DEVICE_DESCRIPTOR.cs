using System.Runtime.InteropServices;

namespace AL_Common.DeviceIoControlLib.Objects.Storage;

[StructLayout(LayoutKind.Sequential)]
public struct STORAGE_DEVICE_DESCRIPTOR
{
    public uint Version;
    public uint Size;
    public byte DeviceType;
    public byte DeviceTypeModifier;
    [MarshalAs(UnmanagedType.U1)]
    public bool RemovableMedia;
    [MarshalAs(UnmanagedType.U1)]
    public bool CommandQueueing;
    public uint VendorIdOffset;
    public uint ProductIdOffset;
    public uint ProductRevisionOffset;
    public uint SerialNumberOffset;
    public STORAGE_BUS_TYPE BusType;
    public uint RawPropertiesLength;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x16)]
    public byte[] RawDeviceProperties;
}
