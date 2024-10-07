// @MT_TASK: Comment this out for final release
//#define TIMING_TEST

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using AL_Common;
using AngelLoader.DataClasses;
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
        internal readonly BusType BusType;

        public PhysicalDisk(string deviceId, MediaType mediaType, BusType busType)
        {
            DeviceId = deviceId;
            MediaType = mediaType;
            BusType = busType;
        }
    }

    internal static async Task<AL_DriveType> GetAllDrivesTypeAsync(List<string> paths)
    {
        return await Task.Run(() => GetAllDrivesType(paths));
    }

    // @MT_TASK: We should put a time limit on this in case something weird happens and it goes forever or an objectionably long time
    // This can return Unspecified for all if you've messed around with Disk Management (I guess?!)
    // That's okay in that case, we'll just fall back to HDD 1-threaded version...
    internal static AL_DriveType GetAllDrivesType(List<string> paths)
    {
#if TIMING_TEST
        var sw = System.Diagnostics.Stopwatch.StartNew();
#endif
        try
        {
            /*
            Some of these WMI things are Windows 8+ ("MSFT_PhysicalDisk" for instance).
            Let's just always return false for Windows 7. Win7 is old enough that hardly anyone should be using
            it, and if they do, well then they get a single-threaded scan by default. If they want threaded,
            they'll have to set it manually.
            */
            if (!Utils.WinVersionIs8OrAbove()) return AL_DriveType.Other;

            List<string> letters = new(paths.Count);

            for (int i = 0; i < paths.Count; i++)
            {
                if (paths[i].IsWhiteSpace()) continue;

                string letter = Path.GetPathRoot(paths[i]).TrimEnd(Common.CA_BS_FS);
                if (letter.IsEmpty())
                {
                    return AL_DriveType.Other;
                }
                if (letter.Length is not 2 || !letter[0].IsAsciiAlpha())
                {
                    return AL_DriveType.Other;
                }

                letters.Add(letter);
            }

            letters = letters.Distinct(StringComparer.OrdinalIgnoreCase).ToList();

            if (letters.Count == 0) return AL_DriveType.Other;

            List<PhysicalDisk> physDisks = GetPhysicalDisks();

            AL_DriveType[] driveTypes = new AL_DriveType[letters.Count];

            for (int i = 0; i < letters.Count; i++)
            {
                string letter = letters[i];

                AL_DriveType driveType = GetDriveType(letter, physDisks);
#if TIMING_TEST
                System.Diagnostics.Trace.WriteLine(letter + " " + driveType);
#endif
                if (driveType == AL_DriveType.Other)
                {
#if TIMING_TEST
                    System.Diagnostics.Trace.WriteLine("All drives are considered: " + AL_DriveType.Other);
#endif
                    return AL_DriveType.Other;
                }

                driveTypes[i] = driveType;
            }

            AL_DriveType ret =
                driveTypes.All(static x => x == AL_DriveType.NVMe_SSD) ? AL_DriveType.NVMe_SSD :
                driveTypes.All(static x => x == AL_DriveType.SATA_SSD) ? AL_DriveType.SATA_SSD :
                AL_DriveType.Other;

#if TIMING_TEST
            System.Diagnostics.Trace.WriteLine("All drives are considered: " + ret);
#endif

            return ret;
        }
        catch
        {
            return AL_DriveType.Other;
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
        using ManagementObjectSearcher physicalSearcher = new(
            @"\\.\root\microsoft\windows\storage",
            "SELECT DeviceId, MediaType, BusType FROM MSFT_PhysicalDisk");

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

    /*
    @MT_TASK: Timeouts can be passed to MOS ctors only, and details are slightly complicated ("return immediately" / "semi-threaded" blah blah)
    We need to have a global timeout, which means we might need to just have a TimeSpan that we pass through to
    each call and then subtract however long it took from it, to make sure we don't exceed our max total...
    That sounds horrendous, but what else can we do? Hopefully we can figure something else out I guess?
    Or like the inverse, just have a global stopwatch, pass say 1 second to each call, and check total elapsed
    time after each call.
    We can forget about Thread.Abort() - aside from being risky, it's not supported on modern .NET anyway.
    */
    private static AL_DriveType GetDriveType(string driveLetter, List<PhysicalDisk> physDisks)
    {
        try
        {
            using ManagementObjectSearcher queryResults = new(
                "ASSOCIATORS OF {Win32_LogicalDisk.DeviceID='" + driveLetter +
                "'} WHERE AssocClass = Win32_LogicalDiskToPartition");
            ManagementObjectCollection partitions = queryResults.Get();

            foreach (ManagementBaseObject partition in partitions)
            {
                if (partition["DeviceID"] is not string partitionDeviceID) continue;

                using ManagementObjectSearcher queryResults2 = new(
                    "ASSOCIATORS OF {Win32_DiskPartition.DeviceID='" + partitionDeviceID +
                    "'} WHERE AssocClass = Win32_DiskDriveToDiskPartition");
                ManagementObjectCollection drives = queryResults2.Get();

#if TIMING_TEST
                var sw = System.Diagnostics.Stopwatch.StartNew();
#endif

                AL_DriveType returnDriveType = AL_DriveType.Other;

                foreach (ManagementBaseObject drive in drives)
                {
                    string deviceId = drive["DeviceID"].ToString();
                    string idStr = deviceId.Substring(@"\\.\PHYSICALDRIVE".Length);
                    AL_DriveType thisDriveType = GetDriveTypeForId(physDisks, idStr);

                    // RAID drives are reported as however many drives there are in the RAID, with each one having
                    // its respective type. So quit once we've found spinning rust or something unknown, for perf.
                    if (thisDriveType == AL_DriveType.Other)
                    {
                        return AL_DriveType.Other;
                    }
                    else if (thisDriveType == AL_DriveType.NVMe_SSD)
                    {
                        // Don't override SATA with NVMe; the lowest speed drive in the array is what we should
                        // report the whole drive as.
                        if (returnDriveType == AL_DriveType.Other)
                        {
                            returnDriveType = AL_DriveType.NVMe_SSD;
                        }
                    }
                    else
                    {
                        returnDriveType = thisDriveType;
                    }
                }

#if TIMING_TEST
                sw.Stop();
                System.Diagnostics.Trace.WriteLine("LOOP TIME: " + sw.Elapsed);
#endif

                return returnDriveType;
            }

            return AL_DriveType.Other;
        }
        catch
        {
            return AL_DriveType.Other;
        }

        // @MT_TASK: If physDisks list can be made to be an async enumerable or something... or at least overlapped until this point and then waited on
        static AL_DriveType GetDriveTypeForId(List<PhysicalDisk> physDisks, string id)
        {
            foreach (PhysicalDisk physDisk in physDisks)
            {
                if (physDisk.DeviceId == id)
                {
                    return IsSolidState(physDisk.MediaType)
                        ? physDisk.BusType == BusType.NVMe
                            ? AL_DriveType.NVMe_SSD
                            : physDisk.BusType == BusType.SATA
                                ? AL_DriveType.SATA_SSD
                                : AL_DriveType.Other
                        : AL_DriveType.Other;
                }
            }

            return AL_DriveType.Other;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsSolidState(MediaType mediaType) => mediaType is MediaType.SSD or MediaType.SCM;
}
