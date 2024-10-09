// @MT_TASK: Comment this out for final release
//#define TIMING_TEST

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using AL_Common;
using AL_Common.DeviceIoControlLib.Objects.Storage;
using AL_Common.DeviceIoControlLib.Wrapper;
using AngelLoader.DataClasses;
using Microsoft.Win32.SafeHandles;

namespace AngelLoader;

internal static class DetectDriveTypes
{
    internal static AL_DriveType GetAllDrivesType(List<string> paths)
    {
#if TIMING_TEST
        var sw = System.Diagnostics.Stopwatch.StartNew();
#endif
        try
        {
            // This stuff all works on Windows 7 which is the oldest we support, so no need to check.

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

            List<AL_DriveType> driveTypes = new(letters.Count);

            /*
            Whereas the WMI-based method could take upwards of 500ms _per drive_ for HDDs - and still ~50ms per
            drive for SSDs - this DeviceIoControl-based method gets the whole set done in under 10ms.
            Now that's more like it.
            */

            foreach (string letter in letters)
            {
                string dummyFileName = @"\\.\" + letter;

                using SafeFileHandle safeHandle = CreateFileW(
                    lpFileName: dummyFileName,
                    /*
                    IMPORTANT(Drive type detect non-administrator bullet dodge):
                    Access ***MUST*** be set to 0! If we set any other access at all, then the operation will
                    require administrator privileges. The ONLY way we can run on non-admin is to set 0 here!
                    Extremely well played, Microsoft... you really had me for a minute there...
                    */
                    dwDesiredAccess: 0,
                    dwShareMode: FileShare.ReadWrite,
                    lpSecurityAttributes: IntPtr.Zero,
                    dwCreationDisposition: FileMode.Open,
                    dwFlagsAndAttributes: FileAttributes.Normal,
                    hTemplateFile: IntPtr.Zero);

                StorageDeviceWrapper device = new(safeHandle);

                DEVICE_SEEK_PENALTY_DESCRIPTOR seekPenaltyDescriptor = device.StorageGetSeekPenaltyDescriptor();
                if (seekPenaltyDescriptor.IncursSeekPenalty)
                {
#if TIMING_TEST
                    System.Diagnostics.Trace.WriteLine("All drives are considered: " + AL_DriveType.Other);
#endif
                    return AL_DriveType.Other;
                }

                STORAGE_DEVICE_DESCRIPTOR_PARSED deviceProperty = device.StorageGetDeviceProperty();

                AL_DriveType driveType = deviceProperty.BusType switch
                {
                    STORAGE_BUS_TYPE.BusTypeNvme => AL_DriveType.NVMe_SSD,
                    STORAGE_BUS_TYPE.BusTypeSata => AL_DriveType.SATA_SSD,
                    /*
                    @MT_TASK(Bus types):
                    We know we have no seek penalty, so "SATA SSD" should be a safe minimum level for exotic
                    bus types. We could also decide to fall back to "NVMe SSD" level after rejecting ones that
                    look iffy like SAS, SCSI, etc. SCM is almost certainly going to benefit from "NVMe SSD" level.
                    */
                    _ => AL_DriveType.SATA_SSD,
                };
                driveTypes.Add(driveType);
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

    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern SafeFileHandle CreateFileW(
        string lpFileName,
        [MarshalAs(UnmanagedType.U4)] FileAccess dwDesiredAccess,
        [MarshalAs(UnmanagedType.U4)] FileShare dwShareMode,
        IntPtr lpSecurityAttributes,
        [MarshalAs(UnmanagedType.U4)] FileMode dwCreationDisposition,
        [MarshalAs(UnmanagedType.U4)] FileAttributes dwFlagsAndAttributes,
        IntPtr hTemplateFile);
}
