using JetBrains.Annotations;

namespace AL_Common.DeviceIoControlLib.Objects.Storage;

[PublicAPI]
public enum STORAGE_PROPERTY_ID
{
    StorageDeviceProperty = 0,
    StorageAdapterProperty = 1,
    StorageDeviceIdProperty = 2,
    StorageDeviceUniqueIdProperty = 3,
    StorageDeviceWriteCacheProperty = 4,
    StorageMiniportProperty = 5,
    StorageAccessAlignmentProperty = 6,
    StorageDeviceSeekPenaltyProperty = 7,
    StorageDeviceTrimProperty = 8,
    StorageDeviceWriteAggregationProperty = 9,
    StorageDeviceDeviceTelemetryProperty = 10, // 0xA
    StorageDeviceLBProvisioningProperty = 11, // 0xB
    StorageDevicePowerProperty = 12, // 0xC
    StorageDeviceCopyOffloadProperty = 13, // 0xD
    StorageDeviceResiliencyProperty = 14, // 0xE
}
