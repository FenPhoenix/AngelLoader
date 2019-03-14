using System;
using System.Runtime.InteropServices;

namespace AngelLoader.WinAPI.Taskbar
{
    internal enum TaskbarStates
    {
        NoProgress = 0,
        Indeterminate = 0x1,
        Normal = 0x2,
        Error = 0x4,
        Paused = 0x8
    }

    internal class TaskBarProgress
    {
        [ComImport]
        [Guid("56fdf344-fd6d-11d0-958a-006097c9a090")]
        [ClassInterface(ClassInterfaceType.None)]
        private class TaskbarInstance { }

        private static readonly ITaskbarList3 Instance = (ITaskbarList3)new TaskbarInstance();

        private static readonly bool TaskbarSupported =
            Environment.OSVersion.Platform == PlatformID.Win32NT &&
            Environment.OSVersion.Version >= new Version(6, 1);

        internal static void SetState(IntPtr windowHandle, TaskbarStates taskbarState)
        {
            if (TaskbarSupported)
            {
                Instance.SetProgressState(windowHandle, taskbarState);
            }
        }

        internal static void SetValue(IntPtr windowHandle, int progressValue, int progressMax)
        {
            if (TaskbarSupported)
            {
                Instance.SetProgressValue(windowHandle, (ulong)progressValue, (ulong)progressMax);
            }
        }
    }
}
