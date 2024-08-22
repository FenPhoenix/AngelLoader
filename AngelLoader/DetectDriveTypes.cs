using System.Collections.Generic;
using System.IO;
using System.Management;
using System.Threading.Tasks;
using AL_Common;
using JetBrains.Annotations;

namespace AngelLoader;

internal static class DetectDriveTypes
{
    [PublicAPI]
    private enum MediaType : ushort
    {
        Unspecified = 0,
        HDD = 3,
        SSD = 4,
        // "Storage Class Memory": https://www.techtarget.com/searchstorage/definition/storage-class-memory
        // Consumer PCs presumably wouldn't have this, but it would have the same performance characteristics as
        // a consumer-level SSD or better, so we can allow it if we want.
        SCM = 5,
    }

    private sealed class PhysicalDisk
    {
        internal readonly string DeviceId;
        internal readonly MediaType MediaType;

        public PhysicalDisk(string deviceId, MediaType mediaType)
        {
            DeviceId = deviceId;
            MediaType = mediaType;
        }
    }

    internal static async Task<bool> AllDrivesAreSolidStateAsync(List<string> paths)
    {
        return await Task.Run(() => AllDrivesAreSolidState(paths));
    }

    internal static bool AllDrivesAreSolidState(List<string> paths)
    {
        try
        {
            List<PhysicalDisk> physDisks = GetPhysicalDisks();

            for (int i = 0; i < paths.Count; i++)
            {
                string letter = Path.GetPathRoot(paths[i]).TrimEnd(Common.CA_BS_FS);
                if (letter.IsEmpty()) return false;

                MediaType mediaType = GetMediaType(letter, physDisks);
                if (mediaType is not MediaType.SSD and not MediaType.SCM)
                {
                    return false;
                }
            }

            return true;
        }
        catch
        {
            return false;
        }
    }

    private static List<PhysicalDisk> GetPhysicalDisks()
    {
        using ManagementObjectSearcher physicalSearcher = new(@"\\.\root\microsoft\windows\storage", "SELECT * FROM MSFT_PhysicalDisk");

        ManagementObjectCollection physResults = physicalSearcher.Get();
        List<PhysicalDisk> physDisks = new(physResults.Count);
        foreach (ManagementBaseObject physQueryObj in physResults)
        {
            string deviceId = "";
            MediaType mediaType = default;

            object? deviceIdObj = physQueryObj["DeviceId"];
            if (deviceIdObj is string deviceIdValue)
            {
                deviceId = deviceIdValue;
            }

            object? mediaTypeObj = physQueryObj["MediaType"];
            if (mediaTypeObj is ushort mediaTypeValue)
            {
                mediaType = (MediaType)mediaTypeValue;
            }

            physDisks.Add(new PhysicalDisk(deviceId, mediaType));
        }

        return physDisks;
    }

    private static MediaType GetMediaType(string driveLetter, List<PhysicalDisk> physDisks)
    {
        try
        {
            string query = "ASSOCIATORS OF {Win32_LogicalDisk.DeviceID='" + driveLetter +
                           "'} WHERE AssocClass = Win32_LogicalDiskToPartition";
            using ManagementObjectSearcher queryResults = new(query);
            ManagementObjectCollection partitions = queryResults.Get();

            foreach (ManagementBaseObject partition in partitions)
            {
                query = "ASSOCIATORS OF {Win32_DiskPartition.DeviceID='" + (string)partition["DeviceID"] +
                        "'} WHERE AssocClass = Win32_DiskDriveToDiskPartition";
                using ManagementObjectSearcher queryResults2 = new(query);
                ManagementObjectCollection drives = queryResults2.Get();

                // @MT_TASK(Drive type detect): Test with RAID and see what result it gives
                // Can there be more than one drive per partition...? Is it like if you have a RAID then there can be?
                foreach (ManagementBaseObject drive in drives)
                {
                    string deviceId = drive["DeviceID"].ToString();
                    string idStr = deviceId.Substring(@"\\.\PHYSICALDRIVE".Length);
                    return GetMediaTypeForId(physDisks, idStr);
                }
            }

            return MediaType.Unspecified;
        }
        catch
        {
            return MediaType.Unspecified;
        }

        static MediaType GetMediaTypeForId(List<PhysicalDisk> physDisks, string id)
        {
            foreach (PhysicalDisk physDisk in physDisks)
            {
                if (physDisk.DeviceId == id)
                {
                    return physDisk.MediaType;
                }
            }

            return MediaType.Unspecified;
        }
    }
}
