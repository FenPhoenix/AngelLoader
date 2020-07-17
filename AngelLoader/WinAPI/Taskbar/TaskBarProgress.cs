using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace AngelLoader.WinAPI.Taskbar
{
    internal enum TaskbarStates
    {
        NoProgress = 0,
        Indeterminate = 1,
        //Normal = 2,
        //Error = 4,
        //Paused = 8
    }

    internal static class TaskBarProgress
    {
        [ComImport]
        [Guid("56fdf344-fd6d-11d0-958a-006097c9a090")]
        [ClassInterface(ClassInterfaceType.None)]
        private class TaskbarInstance { }

        [SuppressMessage("ReSharper", "SuspiciousTypeConversion.Global")]
        private static readonly ITaskbarList3 _instance = (ITaskbarList3)new TaskbarInstance();

        // Windows 7 (version 6.1) is the minimum required version for this
        private static readonly bool _taskbarSupported =
            Environment.OSVersion.Platform == PlatformID.Win32NT &&
            Environment.OSVersion.Version >= new Version(6, 1);

        internal static void SetState(IntPtr windowHandle, TaskbarStates taskbarState)
        {
            if (!_taskbarSupported || windowHandle == IntPtr.Zero) return;
            _instance.SetProgressState(windowHandle, taskbarState);
        }

        internal static void SetValue(IntPtr windowHandle, int progressValue, int progressMax)
        {
            if (!_taskbarSupported || windowHandle == IntPtr.Zero) return;
            _instance.SetProgressValue(windowHandle, (ulong)progressValue, (ulong)progressMax);
        }
    }
}
