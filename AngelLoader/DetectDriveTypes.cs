//#define TIMING_TEST

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using AL_Common.DeviceIoControlLib.Objects.Storage;
using AL_Common.DeviceIoControlLib.Wrapper;
using AngelLoader.DataClasses;
using Microsoft.Win32.SafeHandles;
using static AngelLoader.Misc;

namespace AngelLoader;

internal static class DetectDriveData
{
    internal static List<SettingsDriveData> GetSettingsDriveData()
    {
#if TIMING_TEST
        var sw = System.Diagnostics.Stopwatch.StartNew();
#endif
        string[] drives;
        try
        {
            drives = Directory.GetLogicalDrives();
        }
        catch
        {
            drives = Array.Empty<string>();
        }

        List<SettingsDriveData> ret = new(drives.Length);

        DriveLetterDictionary rootsDict = new(drives.Length);

        for (int i = 0; i < drives.Length; i++)
        {
            string drive = drives[i];
            SettingsDriveData path = new(drive);

            (path.Root, bool isLink) = GetRoot(drive, IOPathType.Directory);

            if (!path.Root.IsEmpty() && !isLink)
            {
                char rootLetter = path.Root[0];
                if (rootsDict.TryGetValue(rootLetter, out DriveMultithreadingLevel driveThreadability))
                {
                    path.MultithreadingLevel = driveThreadability;
                }
                else
                {
                    (driveThreadability, string modelName) = GetDriveThreadability(path.Root);
                    rootsDict[rootLetter] = driveThreadability;
                    path.MultithreadingLevel = driveThreadability;
                    path.ModelName = modelName;
                }

                ret.Add(path);
            }
        }

#if TIMING_TEST
        sw.Stop();
        System.Diagnostics.Trace.WriteLine(nameof(GetSettingsDriveData) + ": " + sw.Elapsed);
#endif

        return ret;
    }

    internal static void GetAllDriveThreadabilities(List<ThreadablePath> paths, DriveLetterDictionary drivesDict)
    {
#if TIMING_TEST
        var sw = System.Diagnostics.Stopwatch.StartNew();
#endif

        DriveLetterDictionary rootsDict = new(paths.Count);

        foreach (ThreadablePath path in paths)
        {
            (path.Root, _) = GetRoot(path.OriginalPath, path.IOPathType);

            if (!path.Root.IsEmpty())
            {
                char rootLetter = path.Root[0];

                if (drivesDict.TryGetValue(rootLetter, out DriveMultithreadingLevel result) && result != DriveMultithreadingLevel.Auto)
                {
                    path.DriveMultithreadingLevel = result;
                }
                else
                {
                    if (rootsDict.TryGetValue(rootLetter, out DriveMultithreadingLevel driveThreadability))
                    {
                        path.DriveMultithreadingLevel = driveThreadability;
                    }
                    else
                    {
                        (driveThreadability, _) = GetDriveThreadability(path.Root);
                        rootsDict[rootLetter] = driveThreadability;
                        path.DriveMultithreadingLevel = driveThreadability;
                    }
                }
            }
        }

#if TIMING_TEST
        sw.Stop();
        System.Diagnostics.Trace.WriteLine(nameof(GetAllDriveThreadabilities) + ": " + sw.Elapsed);
#endif
    }

    [DllImport("kernel32.dll", ExactSpelling = true, SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern SafeFileHandle CreateFileW(
        string lpFileName,
        [MarshalAs(UnmanagedType.U4)] FileAccess dwDesiredAccess,
        [MarshalAs(UnmanagedType.U4)] FileShare dwShareMode,
        IntPtr lpSecurityAttributes,
        [MarshalAs(UnmanagedType.U4)] FileMode dwCreationDisposition,
        [MarshalAs(UnmanagedType.U4)] FileAttributes dwFlagsAndAttributes,
        IntPtr hTemplateFile);

    private static (string Root, bool IsLink)
    GetRoot(string path, IOPathType ioPathType)
    {
        if (!path.IsWhiteSpace())
        {
            try
            {
                if (PathStartsWithLetterAndVolumeSeparator(path))
                {
                    (string realPath, bool isLink) = GetRealDirectory(path, ioPathType);
                    return (Path.GetPathRoot(realPath)?.TrimEnd(CA_BS_FS) ?? "", isLink);
                }
            }
            catch
            {
                // ignore
            }
        }

        return ("", false);
    }

    private static (DriveMultithreadingLevel Threadability, string DriveModelName)
    GetDriveThreadability(string root)
    {
        string modelName = "";

        /*
        Whereas the WMI-based method could take upwards of 500ms _per drive_ for HDDs - and still ~50ms per
        drive for SSDs - this DeviceIoControl-based method gets the whole set done in under 10ms.
        Now that's more like it.
        */

        // This stuff all works on Windows 7 which is the oldest we support, so no need to check.

        // We've got a network drive or something else, and we don't know what it is.
        if (!RootIsLetter(root))
        {
            return (DriveMultithreadingLevel.Single, modelName);
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
                @MT_TASK_NOTE: If the drive is Windows software RAID, this call fails with Win32 error "Incorrect
                 function", so we get no drive type and no model name either.
                 Hardware RAID would presumably return the actual RAID bus type in which case we'd get Read threading
                 level, which is fine enough. But I can't just go buying a bunch of random RAID hardware to try
                 to test that or I'll go broke working on this free program. Meh...
                */
                STORAGE_DEVICE_DESCRIPTOR_PARSED deviceProperty = device.StorageGetDeviceProperty();

                modelName = deviceProperty.ProductId;

                /*
                @MT_TASK_NOTE(Threading levels and drive types)
                Whether write-heavy threading gives a benefit seems to depend mostly on how "high end" the drive
                is, not necessarily whether it's SATA or NVMe.

                -My Samsung 870 Evo 2TB SATA SSD responds well even to write threading, and even while extracting
                 to itself.

                -My WD Blue SA510 1TB SATA SSD tanks hard when hit with write threading or even single-threaded
                 writing when a lot of it is done in sequence. There's nothing to be done about the latter, but
                 it's still less slow than any parallel option.

                -My Gigabyte Aorus GP-ASM2NE6200TTTD 2TB NVMe SSD of course has zero trouble with any sort of
                 threading whatsoever, but it's almost ludicrously high-end with 3600TBW and enough sustained
                 sequential for uncompressed 1440p video footage and all.

                To test if it's safe to detect NVMe SSDs as write-threadable, I'd have to start buying cheap
                drives just to test them, and that's a questionable decision for a free app.

                So, we can't feasibly detect write threading support. We just have to leave read/write threading
                as a manual option.
                */

                /*
                @MT_TASK_NOTE: This can fail (for USB/Optical (virtual mounted)/SD (through USB dongle)/etc.)
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

                DriveMultithreadingLevel multithreadingLevel =
                    seekPenaltyDescriptor.IncursSeekPenalty
                        ? DriveMultithreadingLevel.Single
                        : deviceProperty.BusType switch
                        {
                            /*
                            @MT_TASK_NOTE: We never autodetect ReadWrite now, because we can't know if the drive
                             is capable of it even if it's NVMe...
                            */
                            STORAGE_BUS_TYPE.BusTypeNvme => DriveMultithreadingLevel.Read,
                            STORAGE_BUS_TYPE.BusTypeSCM => DriveMultithreadingLevel.Read,
                            STORAGE_BUS_TYPE.BusTypeRAID => DriveMultithreadingLevel.Read,
                            STORAGE_BUS_TYPE.BusTypeSata => DriveMultithreadingLevel.Read,
                            // We know we have no seek penalty, so Read should be a safe minimum level for exotic
                            // bus types.
                            _ => DriveMultithreadingLevel.Read,
                        };

                return (multithreadingLevel, modelName);
            }
            catch
            {
                return (DriveMultithreadingLevel.Single, modelName);
            }
            finally
            {
                safeHandle?.Dispose();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static bool RootIsLetter(string root)
        {
            return !root.IsEmpty() && root.Length == 2 && root[0].IsAsciiAlpha() && root[1] == ':';
        }
    }

    private static bool TryGetSubstedPath(string path, [NotNullWhen(true)] out string? realPath)
    {
        if (!path.EndsWithDirSep())
        {
            path += "\\";
        }

        realPath = null;

        string? driveLetter;
        try
        {
            driveLetter = Path.GetPathRoot(path)?.TrimEnd(CA_BS_FS);
            if (driveLetter == null)
            {
                return false;
            }
        }
        catch
        {
            return false;
        }

        string result = CallQueryDosDevice(driveLetter);
        if (result.IsEmpty())
        {
            return false;
        }

        if (result.StartsWithO(@"\??\"))
        {
            string realRoot = result.Substring(4);

            if (!driveLetter.EndsWithDirSep())
            {
                driveLetter += "\\";
            }

            realPath = Path.Combine(realRoot, path.Substring(driveLetter.Length));

            if (!PathStartsWithLetterAndVolumeSeparator(realPath))
            {
                realPath = null;
                return false;
            }

            return true;
        }

        realPath = path;

        return false;
    }

    private static string CallQueryDosDevice(string name)
    {
        const int ERROR_INSUFFICIENT_BUFFER = 122;

        StringBuilder buffer = new(MAX_PATH);

        uint dataSize = QueryDosDeviceW(name, buffer, buffer.Capacity);
        while (dataSize <= 0)
        {
            int lastError = Marshal.GetLastWin32Error();
            if (lastError == ERROR_INSUFFICIENT_BUFFER)
            {
                buffer.EnsureCapacity(buffer.Capacity * 2);
                dataSize = QueryDosDeviceW(name, buffer, buffer.Capacity);
            }
            else
            {
                throw new Exception($"Error {lastError} calling QueryDosDevice for '{name}' with buffer size {buffer.Length}. {new Win32Exception(lastError).Message}");
            }
        }

        return buffer.ToString();

        [DllImport("kernel32.dll", ExactSpelling = true, SetLastError = true, CharSet = CharSet.Unicode)]
        static extern uint QueryDosDeviceW(string lpDeviceName, StringBuilder lpTargetPath, int ucchMax);
    }

    private static bool PathStartsWithLetterAndVolumeSeparator(string path) =>
        path.Length >= 2 && path[0].IsAsciiAlpha() && path[1] == Path.VolumeSeparatorChar;


    // In case the directory is a symlink pointing at some other drive
    private static (string Path, bool IsLink) GetRealDirectory(string path, IOPathType ioPathType)
    {
        try
        {
            if (ioPathType == IOPathType.File)
            {
                string? dir = Path.GetDirectoryName(path);
                if (dir.IsEmpty()) return (path, false);
                path = dir;
            }

            DirectoryInfo di = new(path);
            if (!di.Exists)
            {
                return (path, false);
            }

            string? realPath;

            bool finalIsLink = false;

            // Perf: Checking for symbolic link is expensive (double-digit milliseconds for one check), so just
            // do a reparse point check first, which is effectively instantaneous.
            if (!IsReparsePoint(di))
            {
                if (TryGetSubstedPath(path, out realPath))
                {
                    return (realPath, true);
                }
                return (path, false);
            }
            else
            {
                if (TryGetSubstedPath(path, out realPath))
                {
                    if (!IsReparsePoint(new DirectoryInfo(path)))
                    {
                        return (realPath, true);
                    }
                    finalIsLink = true;
                    path = realPath;
                }
            }

            FileSystemInfo? fsi = Directory_ResolveLinkTarget(path, returnFinalTarget: true);
            if (fsi != null)
            {
                path = fsi.FullName;
            }

            return (path, finalIsLink);

            static bool IsReparsePoint(DirectoryInfo di) => (di.Attributes & FileAttributes.ReparsePoint) != 0;
        }
        catch
        {
            return (path, false);
        }
    }
}
