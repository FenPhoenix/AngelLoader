using System;
using AL_Common.DeviceIoControlLib.Objects.Enums;
using AL_Common.DeviceIoControlLib.Objects.Storage;
using AL_Common.DeviceIoControlLib.Utilities;
using Microsoft.Win32.SafeHandles;

namespace AL_Common.DeviceIoControlLib.Wrapper;

public sealed class StorageDeviceWrapper
{
    private readonly SafeFileHandle _handle;

    public StorageDeviceWrapper(SafeFileHandle handle)
    {
        if (handle.IsInvalid)
        {
            throw new ArgumentException("Handle is invalid");
        }

        _handle = handle;
    }

    public DEVICE_SEEK_PENALTY_DESCRIPTOR StorageGetSeekPenaltyDescriptor()
    {
        STORAGE_PROPERTY_QUERY query = new()
        {
            QueryType = STORAGE_QUERY_TYPE.PropertyStandardQuery,
            PropertyId = STORAGE_PROPERTY_ID.StorageDeviceSeekPenaltyProperty,
        };

        byte[] res = DeviceIoControlHelper.InvokeIoControlUnknownSize(_handle, IOControlCode.StorageQueryProperty, query);
        DEVICE_SEEK_PENALTY_DESCRIPTOR descriptor = Utils.ByteArrayToStruct<DEVICE_SEEK_PENALTY_DESCRIPTOR>(res, 0);

        return descriptor;
    }

    public STORAGE_DEVICE_DESCRIPTOR_PARSED StorageGetDeviceProperty()
    {
        STORAGE_PROPERTY_QUERY query = new()
        {
            QueryType = STORAGE_QUERY_TYPE.PropertyStandardQuery,
            PropertyId = STORAGE_PROPERTY_ID.StorageDeviceProperty,
        };

        byte[] res = DeviceIoControlHelper.InvokeIoControlUnknownSize(_handle, IOControlCode.StorageQueryProperty, query);
        STORAGE_DEVICE_DESCRIPTOR descriptor = Utils.ByteArrayToStruct<STORAGE_DEVICE_DESCRIPTOR>(res, 0);

        STORAGE_DEVICE_DESCRIPTOR_PARSED returnValue = new()
        {
            Version = descriptor.Version,
            Size = descriptor.Size,
            DeviceType = descriptor.DeviceType,
            DeviceTypeModifier = descriptor.DeviceTypeModifier,
            RemovableMedia = descriptor.RemovableMedia,
            CommandQueueing = descriptor.CommandQueueing,
            VendorIdOffset = descriptor.VendorIdOffset,
            ProductIdOffset = descriptor.ProductIdOffset,
            ProductRevisionOffset = descriptor.ProductRevisionOffset,
            SerialNumberOffset = descriptor.SerialNumberOffset,
            BusType = descriptor.BusType,
            RawPropertiesLength = descriptor.RawPropertiesLength,
            RawDeviceProperties = descriptor.RawDeviceProperties,
        };

        if (descriptor.SerialNumberOffset > 0)
        {
            returnValue.SerialNumber = Utils.ReadNullTerminatedAsciiString(res, (int)descriptor.SerialNumberOffset);
        }

        if (descriptor.VendorIdOffset > 0)
        {
            returnValue.VendorId = Utils.ReadNullTerminatedAsciiString(res, (int)descriptor.VendorIdOffset);
        }

        if (descriptor.ProductIdOffset > 0)
        {
            returnValue.ProductId = Utils.ReadNullTerminatedAsciiString(res, (int)descriptor.ProductIdOffset);
        }

        if (descriptor.ProductRevisionOffset > 0)
        {
            returnValue.ProductRevision = Utils.ReadNullTerminatedAsciiString(res, (int)descriptor.ProductRevisionOffset);
        }

        return returnValue;
    }
}
