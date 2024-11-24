//#define TESTING

using System.Runtime.InteropServices;

namespace AL_Common.DeviceIoControlLib.Objects.Storage;

[StructLayout(LayoutKind.Sequential)]
public struct STORAGE_DEVICE_DESCRIPTOR_PARSED
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
    public string SerialNumber;
    public string VendorId;
    public string ProductId;
    public string ProductRevision;

#if TESTING
    public override string ToString()
    {
        string rawDeviceProperties = "";
        uint length = RawPropertiesLength.Clamp(0u, 0x16u);
        for (int i = 0; i < length; i++)
        {
            rawDeviceProperties += RawDeviceProperties[i].ToString(System.Globalization.NumberFormatInfo.InvariantInfo) + ",";
        }

        return "----------------" + $"{NL}" +
               nameof(Version) + ": " + Version.ToString(System.Globalization.NumberFormatInfo.InvariantInfo) + $"{NL}" +
               nameof(Size) + ": " + Size.ToString(System.Globalization.NumberFormatInfo.InvariantInfo) + $"{NL}" +
               nameof(DeviceType) + ": " + DeviceType.ToString(System.Globalization.NumberFormatInfo.InvariantInfo) + $"{NL}" +
               nameof(DeviceTypeModifier) + ": " + DeviceTypeModifier.ToString(System.Globalization.NumberFormatInfo.InvariantInfo) + $"{NL}" +
               nameof(RemovableMedia) + ": " + RemovableMedia.ToString(System.Globalization.NumberFormatInfo.InvariantInfo) + $"{NL}" +
               nameof(CommandQueueing) + ": " + CommandQueueing.ToString(System.Globalization.NumberFormatInfo.InvariantInfo) + $"{NL}" +
               nameof(VendorIdOffset) + ": " + VendorIdOffset.ToString(System.Globalization.NumberFormatInfo.InvariantInfo) + $"{NL}" +
               nameof(ProductIdOffset) + ": " + ProductIdOffset.ToString(System.Globalization.NumberFormatInfo.InvariantInfo) + $"{NL}" +
               nameof(ProductRevisionOffset) + ": " + ProductRevisionOffset.ToString(System.Globalization.NumberFormatInfo.InvariantInfo) + $"{NL}" +
               nameof(SerialNumberOffset) + ": " + SerialNumberOffset.ToString(System.Globalization.NumberFormatInfo.InvariantInfo) + $"{NL}" +
               nameof(BusType) + ": " + BusType + $"{NL}" +
               nameof(RawPropertiesLength) + ": " + RawPropertiesLength.ToString(System.Globalization.NumberFormatInfo.InvariantInfo) + $"{NL}" +
               nameof(RawDeviceProperties) + ": " + rawDeviceProperties + $"{NL}" +
               nameof(SerialNumber) + ": " + SerialNumber + $"{NL}" +
               nameof(VendorId) + ": " + VendorId + $"{NL}" +
               nameof(ProductId) + ": " + ProductId + $"{NL}" +
               nameof(ProductRevision) + ": " + ProductRevision + $"{NL}";
    }
#endif
}
