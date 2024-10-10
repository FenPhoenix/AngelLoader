// @MT_TASK: Comment this out for final release
//#define TIMING_TEST

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using AL_Common;
using AL_Common.DeviceIoControlLib.Objects.Storage;
using AL_Common.DeviceIoControlLib.Wrapper;
using AngelLoader.DataClasses;
using Microsoft.Win32.SafeHandles;

namespace AngelLoader;

internal static class DetectDriveTypes
{
    /*
    @MT_TASK: Different paths are "relevant" depending on what operation we're doing. We need to account for this.
    For example, we want to consider the backup path for install/uninstall, but it should be ignored for scans -
    otherwise if the backup path is on an HDD, the scan will then fall back to single-threaded even though it
    doesn't even touch the backup path.
    We should keep a list of drive types alongside all of our paths in the config object, so that different
    operations can decide which paths they care about.
    */
    internal static List<AL_DriveType> GetAllDrivesType(List<string> paths)
    {
#if TIMING_TEST
        var sw = System.Diagnostics.Stopwatch.StartNew();
#endif

        // This stuff all works on Windows 7 which is the oldest we support, so no need to check.

        List<string> roots = new(paths.Count);

        for (int i = 0; i < paths.Count; i++)
        {
            if (paths[i].IsWhiteSpace()) continue;

            try
            {
                string? root = Path.GetPathRoot(paths[i])?.TrimEnd(Common.CA_BS_FS);
                if (!root.IsEmpty())
                {
                    roots.Add(root);
                }
            }
            catch
            {
                // ignore and don't add
            }
        }

        List<AL_DriveType> ret = new(roots.Count);

        roots = roots.Distinct(StringComparer.OrdinalIgnoreCase).ToList();

        if (roots.Count == 0)
        {
#if TIMING_TEST
            sw.Stop();
            System.Diagnostics.Trace.WriteLine(sw.Elapsed);
#endif
            return ret;
        }

        /*
        Whereas the WMI-based method could take upwards of 500ms _per drive_ for HDDs - and still ~50ms per
        drive for SSDs - this DeviceIoControl-based method gets the whole set done in under 10ms.
        Now that's more like it.
        */

        foreach (string root in roots)
        {
            AL_DriveType driveType;

            // We've got a network drive or something else, and we don't know what it is.
            if (!RootIsLetter(root))
            {
                driveType = AL_DriveType.Other;
            }
            else
            {
                SafeFileHandle? safeHandle = null;
                try
                {
                    // I can't really test to see if this is even needed in this situation, but let's just use it
                    // for safety. We REALLY don't want some prompt window popping up when we're running this;
                    // users won't know wtf's going on.
                    using (DisableMediaInsertionPrompt.Create())
                    {
                        string dummyFileName = @"\\.\" + root;
                        safeHandle = CreateFileW(
                            lpFileName: dummyFileName,
                            /*
                            IMPORTANT(Drive type detect non-administrator bullet dodge):
                            Access ***MUST*** be set to 0! If we set any other access at all, then the operation
                            will require administrator privileges. The ONLY way we can run on non-admin is to set
                            0 here!
                            */
                            dwDesiredAccess: 0,
                            dwShareMode: FileShare.ReadWrite,
                            lpSecurityAttributes: IntPtr.Zero,
                            dwCreationDisposition: FileMode.Open,
                            dwFlagsAndAttributes: FileAttributes.Normal,
                            hTemplateFile: IntPtr.Zero);
                    }

                    StorageDeviceWrapper device = new(safeHandle);

                    DEVICE_SEEK_PENALTY_DESCRIPTOR seekPenaltyDescriptor = device.StorageGetSeekPenaltyDescriptor();
                    if (seekPenaltyDescriptor.IncursSeekPenalty)
                    {
                        driveType = AL_DriveType.Other;
                    }
                    else
                    {
                        STORAGE_DEVICE_DESCRIPTOR_PARSED deviceProperty = device.StorageGetDeviceProperty();

                        driveType = deviceProperty.BusType switch
                        {
                            STORAGE_BUS_TYPE.BusTypeNvme or STORAGE_BUS_TYPE.BusTypeSCM => AL_DriveType.NVMe_SSD,
                            STORAGE_BUS_TYPE.BusTypeSata => AL_DriveType.SATA_SSD,
                            // We know we have no seek penalty, so "SATA SSD" should be a safe minimum level for
                            // exotic bus types.
                            _ => AL_DriveType.SATA_SSD,
                        };
                    }
                }
                catch
                {
                    driveType = AL_DriveType.Other;
                }
                finally
                {
                    safeHandle?.Dispose();
                }
            }

            ret.Add(driveType);
        }

#if TIMING_TEST
        sw.Stop();
        System.Diagnostics.Trace.WriteLine(sw.Elapsed);
#endif

        return ret;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool RootIsLetter(string root)
    {
        return !root.IsEmpty() && root.Length == 2 && root[0].IsAsciiAlpha() && root[1] == ':';
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
