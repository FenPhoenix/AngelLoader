// @MT_TASK: Remove for final release
//#define TIMING_TEST

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management;
using System.Runtime.CompilerServices;
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

    [PublicAPI]
    private enum BusType : ushort
    {
        Unknown = 0,
        SCSI = 1,
        ATAPI = 2,
        ATA = 3,
        _1394 = 4,
        SSA = 5,
        FibreChannel = 6,
        USB = 7,
        RAID = 8,
        iSCSI = 9,
        SAS = 10,
        SATA = 11,
        SD = 12,
        MMC = 13,
        MAX = 14,
        FileBackedVirtual = 15,
        StorageSpaces = 16,
        NVMe = 17,
        MicrosoftReserved = 18,
    }

    private sealed class PhysicalDisk
    {
        internal readonly string DeviceId;
        internal readonly MediaType MediaType;
        // @MT_TASK: Use the BusType to detect NVMe to enable ultra-threaded mode
        internal readonly BusType BusType;

        public PhysicalDisk(string deviceId, MediaType mediaType, BusType busType)
        {
            DeviceId = deviceId;
            MediaType = mediaType;
            BusType = busType;
        }
    }

    internal static async Task<bool> AllDrivesAreSolidStateAsync(List<string> paths)
    {
        return await Task.Run(() => AllDrivesAreSolidState(paths));
    }

    // @MT_TASK: We should put a time limit on this in case something weird happens and it goes forever or an objectionably long time
    // This can return Unspecified for all if you've messed around with Disk Management (I guess?!)
    // That's okay in that case, we'll just fall back to HDD 1-threaded version...
    internal static bool AllDrivesAreSolidState(List<string> paths)
    {
#if TIMING_TEST
        var sw = new System.Diagnostics.Stopwatch();
        sw.Start();
#endif
        try
        {
            /*
            Some of these WMI things are Windows 8+ ("MSFT_PhysicalDisk" for instance).
            Let's just always return false for Windows 7. Win7 is old enough that hardly anyone should be using
            it, and if they do, well then they get a single-threaded scan by default. If they want threaded,
            they'll have to set it manually.
            */
            if (!Utils.WinVersionIs8OrAbove()) return false;

            List<string> letters = new(paths.Count);

            for (int i = 0; i < paths.Count; i++)
            {
                string letter = Path.GetPathRoot(paths[i]).TrimEnd(Common.CA_BS_FS);
                if (letter.IsEmpty())
                {
                    return false;
                }
                if (letter.Length is not 2 || !letter[0].IsAsciiAlpha())
                {
                    return false;
                }

                letters.Add(letter);
            }

            letters = letters.Distinct(StringComparer.OrdinalIgnoreCase).ToList();

            if (letters.Count == 0) return false;

            List<PhysicalDisk> physDisks = GetPhysicalDisks();

            for (int i = 0; i < letters.Count; i++)
            {
                string letter = letters[i];

                MediaType mediaType = GetMediaType(letter, physDisks);
#if TIMING_TEST
                System.Diagnostics.Trace.WriteLine(letter + " " + mediaType);
#endif
                if (!IsSolidState(mediaType))
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
#if TIMING_TEST
        finally
        {
            sw.Stop();
            System.Diagnostics.Trace.WriteLine(sw.Elapsed);
        }
#endif
    }

    private static List<PhysicalDisk> GetPhysicalDisks()
    {
        using ManagementObjectSearcher physicalSearcher = new(@"\\.\root\microsoft\windows\storage", "SELECT DeviceId, MediaType FROM MSFT_PhysicalDisk");

        ManagementObjectCollection physResults = physicalSearcher.Get();
        List<PhysicalDisk> physDisks = new(physResults.Count);
        foreach (ManagementBaseObject physQueryObj in physResults)
        {
            string deviceId = "";
            MediaType mediaType = default;
            BusType busType = default;

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

            object? busTypeObj = physQueryObj["BusType"];
            if (busTypeObj is ushort busTypeValue)
            {
                busType = (BusType)busTypeValue;
            }

            physDisks.Add(new PhysicalDisk(deviceId, mediaType, busType));
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

#if TIMING_TEST
                var sw = new System.Diagnostics.Stopwatch();
                sw.Start();
#endif

                MediaType returnMediaType = MediaType.Unspecified;

                foreach (ManagementBaseObject drive in drives)
                {
                    string deviceId = drive["DeviceID"].ToString();
                    string idStr = deviceId.Substring(@"\\.\PHYSICALDRIVE".Length);
                    returnMediaType = GetMediaTypeForId(physDisks, idStr);

                    // RAID drives are reported as however many drives there are in the RAID, with each one having
                    // its respective type. So quit once we've found spinning rust or something unknown, for perf.
                    if (!IsSolidState(returnMediaType))
                    {
                        return returnMediaType;
                    }
                }

#if TIMING_TEST
                sw.Stop();
                System.Diagnostics.Trace.WriteLine("LOOP TIME: " + sw.Elapsed);
#endif

                return returnMediaType;
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsSolidState(MediaType mediaType) => mediaType is MediaType.SSD or MediaType.SCM;
}
