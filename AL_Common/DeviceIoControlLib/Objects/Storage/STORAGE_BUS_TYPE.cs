using JetBrains.Annotations;

namespace AL_Common.DeviceIoControlLib.Objects.Storage;

[PublicAPI]
public enum STORAGE_BUS_TYPE
{
    BusTypeUnknown = 0x00,
    BusTypeScsi = 0x1,
    BusTypeAtapi = 0x2,
    BusTypeAta = 0x3,
    BusType1394 = 0x4,
    BusTypeSsa = 0x5,
    BusTypeFibre = 0x6,
    BusTypeUsb = 0x7,
    BusTypeRAID = 0x8,
    BusTypeiScsi = 0x9,
    BusTypeSas = 0xA,
    BusTypeSata = 0xB,
    BusTypeSd = 0xC,
    BusTypeMmc = 0xD,
    BusTypeVirtual = 0xE,
    BusTypeFileBackedVirtual = 0xF,
    BusTypeSpaces = 0x10,
    BusTypeNvme = 0x11,
    BusTypeSCM = 0x12,
    BusTypeUfs = 0x13,
    BusTypeMax = 0x11,
    BusTypeMaxReserved = 0x7F,
}
