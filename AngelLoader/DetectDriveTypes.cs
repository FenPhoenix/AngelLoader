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
                string realPath = GetRealDirectory(paths[i]);
                string? root = Path.GetPathRoot(realPath)?.TrimEnd(Common.CA_BS_FS);
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

                    /*
                    @MT_TASK: This can fail (for USB/Optical (virtual mounted)/SD (through USB dongle)/etc.)
                    If it fails, we fall back to Other which is probably fine. But, we could also say if this
                    fails then we should continue on below and get the bus type, from which we may be able to
                    make a more intelligent decision.

                    It's possible to get the nominal rotation rate, but this requires admin privileges. The docs
                    make it sound like it doesn't:

                    https://learn.microsoft.com/en-us/windows-hardware/drivers/ddi/ntddscsi/ni-ntddscsi-ioctl_ata_pass_through
                    "Applications do not require administrative privileges to send a pass-through request to a
                    device, but they must have read/write access to the device."

                    But of course, having read/write access to the device itself requires admin privileges, so
                    the fact that something beyond it doesn't is completely useless. Oh well.
                    */
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
                            STORAGE_BUS_TYPE.BusTypeNvme
                                or STORAGE_BUS_TYPE.BusTypeSCM
                                /*
                                @MT_TASK: The question of RAID
                                We could have SATA RAID or NVMe RAID, and we don't know which... we also don't
                                know if it's striped or mirrored. Striped SATA RAID is probably fast enough for
                                aggressive threading, but mirrored RAID is the same speed as non-RAID. So we're
                                in a bit of a conundrum here.
                                */
                                //or STORAGE_BUS_TYPE.BusTypeRAID
                                => AL_DriveType.NVMe_SSD,
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static bool RootIsLetter(string root)
        {
            return !root.IsEmpty() && root.Length == 2 && root[0].IsAsciiAlpha() && root[1] == ':';
        }

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        static extern SafeFileHandle CreateFileW(
            string lpFileName,
            [MarshalAs(UnmanagedType.U4)] FileAccess dwDesiredAccess,
            [MarshalAs(UnmanagedType.U4)] FileShare dwShareMode,
            IntPtr lpSecurityAttributes,
            [MarshalAs(UnmanagedType.U4)] FileMode dwCreationDisposition,
            [MarshalAs(UnmanagedType.U4)] FileAttributes dwFlagsAndAttributes,
            IntPtr hTemplateFile);

        // In case the directory is a symlink pointing at some other drive
        static string GetRealDirectory(string path)
        {
            try
            {
                // @MT_TASK: Cheap, inefficient check; make this smarter later
                // By having a path object with a file-or-directory field?
                if (File.Exists(path))
                {
                    string? dir = Path.GetDirectoryName(path);
                    if (dir.IsEmpty()) return path;
                    path = dir;
                }

                // Perf: Checking for symbolic link is expensive (double-digit milliseconds for one check), so just
                // do a reparse point check first, which is effectively instantaneous.
                if ((new DirectoryInfo(path).Attributes & FileAttributes.ReparsePoint) == 0)
                {
                    return path;
                }

                FileSystemInfo? fsi = Common.Directory_ResolveLinkTarget(path, returnFinalTarget: true);
                if (fsi != null)
                {
                    path = fsi.FullName;
                }

                return path;
            }
            catch
            {
                return path;
            }
        }
    }
}
