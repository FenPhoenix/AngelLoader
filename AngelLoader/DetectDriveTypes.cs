//#define TIMING_TEST

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using AL_Common;
using AL_Common.DeviceIoControlLib.Objects.Storage;
using AL_Common.DeviceIoControlLib.Wrapper;
using AngelLoader.DataClasses;
using Microsoft.Win32.SafeHandles;
using static AL_Common.Common;
using static AngelLoader.Misc;

namespace AngelLoader;

internal static class DetectDriveData
{
    internal static void FillSettingsDriveData(SettingsDriveData[] paths)
    {
#if TIMING_TEST
        var sw = System.Diagnostics.Stopwatch.StartNew();
#endif

        DriveLetterDictionary rootsDict = new(paths.Length);

        foreach (SettingsDriveData path in paths)
        {
            path.Root = GetRoot(path.OriginalPath, IOPathType.Directory);

            if (!path.Root.IsEmpty())
            {
                char rootLetter = path.Root[0];
                if (rootsDict.TryGetValue(rootLetter, out DriveThreadability driveThreadability))
                {
                    path.Threadability = driveThreadability;
                }
                else
                {
                    (driveThreadability, string modelName) = GetDriveThreadability(path.Root);
                    rootsDict[rootLetter] = driveThreadability;
                    path.Threadability = driveThreadability;
                    path.ModelName = modelName;
                }
            }
        }

#if TIMING_TEST
        sw.Stop();
        System.Diagnostics.Trace.WriteLine(nameof(FillSettingsDriveData) + ": " + sw.Elapsed);
#endif
    }

    internal static void GetAllDriveThreadabilities(List<ThreadablePath> paths, DriveLetterDictionary drivesDict)
    {
#if TIMING_TEST
        var sw = System.Diagnostics.Stopwatch.StartNew();
#endif

        DriveLetterDictionary rootsDict = new(paths.Count);

        foreach (ThreadablePath path in paths)
        {
            path.Root = GetRoot(path.OriginalPath, path.IOPathType);

            if (!path.Root.IsEmpty())
            {
                char rootLetter = path.Root[0];

                if (drivesDict.TryGetValue(rootLetter, out DriveThreadability result) && result != DriveThreadability.Auto)
                {
                    path.DriveThreadability = result;
                }
                else
                {
                    if (rootsDict.TryGetValue(rootLetter, out DriveThreadability driveThreadability))
                    {
                        path.DriveThreadability = driveThreadability;
                    }
                    else
                    {
                        (driveThreadability, _) = GetDriveThreadability(path.Root);
                        rootsDict[rootLetter] = driveThreadability;
                        path.DriveThreadability = driveThreadability;
                    }
                }
            }
        }

#if TIMING_TEST
        sw.Stop();
        System.Diagnostics.Trace.WriteLine(nameof(GetAllDriveThreadabilities) + ": " + sw.Elapsed);
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

    private static string GetRoot(string path, IOPathType ioPathType)
    {
        if (!path.IsWhiteSpace())
        {
            try
            {
                if (PathStartsWithLetterAndVolumeSeparator(path))
                {
                    string realPath = GetRealDirectory(path, ioPathType);
                    return Path.GetPathRoot(realPath)?.TrimEnd(CA_BS_FS) ?? "";
                }
            }
            catch
            {
                // ignore
            }
        }

        return "";
    }

    private static (DriveThreadability Threadability, string DriveModelName)
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
            return (DriveThreadability.Single, modelName);
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

                STORAGE_DEVICE_DESCRIPTOR_PARSED deviceProperty = device.StorageGetDeviceProperty();

                modelName = deviceProperty.ProductId;

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

                DriveThreadability threadability =
                    seekPenaltyDescriptor.IncursSeekPenalty
                        ? DriveThreadability.Single
                        : deviceProperty.BusType switch
                        {
                            STORAGE_BUS_TYPE.BusTypeNvme
                                or STORAGE_BUS_TYPE.BusTypeSCM
                                /*
                                @MT_TASK: What happens if the drive is RAID?
                                @MT_TASK: What's the model name on RAID?

                                @MT_TASK: We never autodetect aggressive now, because we can't know if the drive
                                 is capable of it even if it's NVMe...
                                */
                                //or STORAGE_BUS_TYPE.BusTypeRAID
                                => DriveThreadability.Standard,
                            STORAGE_BUS_TYPE.BusTypeSata => DriveThreadability.Standard,
                            // We know we have no seek penalty, so "SATA SSD" should be a safe minimum level for
                            // exotic bus types.
                            _ => DriveThreadability.Standard,
                        };

                return (threadability, modelName);
            }
            catch
            {
                return (DriveThreadability.Single, modelName);
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

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        static extern uint QueryDosDeviceW(string lpDeviceName, StringBuilder lpTargetPath, int ucchMax);
    }

    private static bool PathStartsWithLetterAndVolumeSeparator(string path) =>
        path.Length >= 2 && path[0].IsAsciiAlpha() && path[1] == Path.VolumeSeparatorChar;


    // In case the directory is a symlink pointing at some other drive
    private static string GetRealDirectory(string path, IOPathType ioPathType)
    {
        try
        {
            if (ioPathType == IOPathType.File)
            {
                string? dir = Path.GetDirectoryName(path);
                if (dir.IsEmpty()) return path;
                path = dir;
            }

            DirectoryInfo di = new(path);
            if (!di.Exists)
            {
                return path;
            }

            string? realPath;

            // Perf: Checking for symbolic link is expensive (double-digit milliseconds for one check), so just
            // do a reparse point check first, which is effectively instantaneous.
            if (!IsReparsePoint(di))
            {
                if (TryGetSubstedPath(path, out realPath))
                {
                    return realPath;
                }
                return path;
            }
            else
            {
                if (TryGetSubstedPath(path, out realPath))
                {
                    if (!IsReparsePoint(new DirectoryInfo(path)))
                    {
                        return realPath;
                    }
                    path = realPath;
                }
            }

            FileSystemInfo? fsi = Directory_ResolveLinkTarget(path, returnFinalTarget: true);
            if (fsi != null)
            {
                path = fsi.FullName;
            }

            return path;

            static bool IsReparsePoint(DirectoryInfo di) => (di.Attributes & FileAttributes.ReparsePoint) != 0;
        }
        catch
        {
            return path;
        }
    }
}
