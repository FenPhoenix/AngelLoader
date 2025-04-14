using System.Runtime.InteropServices;

namespace AngelLoader.Forms.WinFormsNative.Taskbar;

[ComImport]
[Guid("ea1afb91-9e28-4b86-90e9-9e9f8a5eefaf")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface ITaskbarList3
{
    // ITaskbarList
    [PreserveSig]
    void HrInit();
    [PreserveSig]
    void AddTab(nint hwnd);
    [PreserveSig]
    void DeleteTab(nint hwnd);
    [PreserveSig]
    void ActivateTab(nint hwnd);
    [PreserveSig]
    void SetActiveAlt(nint hwnd);

    // ITaskbarList2
    [PreserveSig]
    void MarkFullscreenWindow(nint hwnd, [MarshalAs(UnmanagedType.Bool)] bool fFullscreen);

    // ITaskbarList3
    [PreserveSig]
    void SetProgressValue(nint hwnd, ulong ullCompleted, ulong ullTotal);
    [PreserveSig]
    void SetProgressState(nint hwnd, TaskbarStates state);
}
