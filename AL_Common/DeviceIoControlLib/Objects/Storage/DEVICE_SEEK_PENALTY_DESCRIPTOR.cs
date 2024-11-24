using System.Runtime.InteropServices;

namespace AL_Common.DeviceIoControlLib.Objects.Storage;

[StructLayout(LayoutKind.Sequential)]
public struct DEVICE_SEEK_PENALTY_DESCRIPTOR
{
    public uint Version;
    public uint Size;
    [MarshalAs(UnmanagedType.U1)]
    public bool IncursSeekPenalty;
}
