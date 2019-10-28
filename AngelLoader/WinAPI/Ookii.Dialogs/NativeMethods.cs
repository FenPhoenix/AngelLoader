// NULL_TODO
#nullable disable

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace AngelLoader.WinAPI.Ookii.Dialogs
{
    [SuppressMessage("ReSharper", "IdentifierTypo")]
    internal static class NativeMethods
    {
        public static bool IsWindowsVistaOrLater => Environment.OSVersion.Platform == PlatformID.Win32NT && Environment.OSVersion.Version >= new Version(6, 0, 6000);

        public static bool IsWindowsXPOrLater => Environment.OSVersion.Platform == PlatformID.Win32NT && Environment.OSVersion.Version >= new Version(5, 1, 2600);

        #region Task Dialogs

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern IntPtr GetActiveWindow();

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern int GetWindowThreadProcessId(IntPtr hWnd, out int lpdwProcessId);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern int GetCurrentThreadId();

        [SuppressMessage("Microsoft.Interoperability", "CA1400:PInvokeEntryPointsShouldExist"), DllImport("comctl32.dll", PreserveSig = false)]
        public static extern void TaskDialogIndirect([In] ref TASKDIALOGCONFIG pTaskConfig, out int pnButton, out int pnRadioButton, [MarshalAs(UnmanagedType.Bool)] out bool pfVerificationFlagChecked);

        public delegate uint TaskDialogCallback(IntPtr hwnd, uint uNotification, IntPtr wParam, IntPtr lParam, IntPtr dwRefData);

        public const int WM_GETICON = 0x007F;
        public const int WM_SETICON = 0x0080;
        public const int ICON_SMALL = 0;

        [PublicAPI]
        public enum TaskDialogNotifications
        {
            Created = 0,
            Navigated = 1,
            ButtonClicked = 2,            // wParam = Button ID
            HyperlinkClicked = 3,            // lParam = (LPCWSTR)pszHREF
            Timer = 4,            // wParam = Milliseconds since dialog created or timer reset
            Destroyed = 5,
            RadioButtonClicked = 6,            // wParam = Radio Button ID
            DialogConstructed = 7,
            VerificationClicked = 8,             // wParam = 1 if checkbox checked, 0 if not, lParam is unused and always 0
            Help = 9,
            ExpandoButtonClicked = 10            // wParam = 0 (dialog is now collapsed), wParam != 0 (dialog is now expanded)
        }

        [Flags]
        public enum TaskDialogCommonButtonFlags
        {
            OkButton = 0x0001, // selected control return value IDOK
            YesButton = 0x0002, // selected control return value IDYES
            NoButton = 0x0004, // selected control return value IDNO
            CancelButton = 0x0008, // selected control return value IDCANCEL
            RetryButton = 0x0010, // selected control return value IDRETRY
            CloseButton = 0x0020  // selected control return value IDCLOSE
        }

        [Flags]
        public enum TaskDialogFlags
        {
            EnableHyperLinks = 0x0001,
            UseHIconMain = 0x0002,
            UseHIconFooter = 0x0004,
            AllowDialogCancellation = 0x0008,
            UseCommandLinks = 0x0010,
            UseCommandLinksNoIcon = 0x0020,
            ExpandFooterArea = 0x0040,
            ExpandedByDefault = 0x0080,
            VerificationFlagChecked = 0x0100,
            ShowProgressBar = 0x0200,
            ShowMarqueeProgressBar = 0x0400,
            CallbackTimer = 0x0800,
            PositionRelativeToWindow = 0x1000,
            RtlLayout = 0x2000,
            NoDefaultRadioButton = 0x4000,
            CanBeMinimized = 0x8000
        }

        [PublicAPI]
        public enum TaskDialogMessages
        {
            NavigatePage = InteropMisc.WM_USER + 101,
            ClickButton = InteropMisc.WM_USER + 102, // wParam = Button ID
            SetMarqueeProgressBar = InteropMisc.WM_USER + 103, // wParam = 0 (nonMarque) wParam != 0 (Marquee)
            SetProgressBarState = InteropMisc.WM_USER + 104, // wParam = new progress state
            SetProgressBarRange = InteropMisc.WM_USER + 105, // lParam = MAKELPARAM(nMinRange, nMaxRange)
            SetProgressBarPos = InteropMisc.WM_USER + 106, // wParam = new position
            SetProgressBarMarquee = InteropMisc.WM_USER + 107, // wParam = 0 (stop marquee), wParam != 0 (start marquee), lparam = speed (milliseconds between repaints)
            SetElementText = InteropMisc.WM_USER + 108, // wParam = element (TASKDIALOG_ELEMENTS), lParam = new element text (LPCWSTR)
            ClickRadioButton = InteropMisc.WM_USER + 110, // wParam = Radio Button ID
            EnableButton = InteropMisc.WM_USER + 111, // lParam = 0 (disable), lParam != 0 (enable), wParam = Button ID
            EnableRadioButton = InteropMisc.WM_USER + 112, // lParam = 0 (disable), lParam != 0 (enable), wParam = Radio Button ID
            ClickVerification = InteropMisc.WM_USER + 113, // wParam = 0 (unchecked), 1 (checked), lParam = 1 (set key focus)
            UpdateElementText = InteropMisc.WM_USER + 114, // wParam = element (TASKDIALOG_ELEMENTS), lParam = new element text (LPCWSTR)
            SetButtonElevationRequiredState = InteropMisc.WM_USER + 115, // wParam = Button ID, lParam = 0 (elevation not required), lParam != 0 (elevation required)
            UpdateIcon = InteropMisc.WM_USER + 116  // wParam = icon element (TASKDIALOG_ICON_ELEMENTS), lParam = new icon (hIcon if TDF_USE_HICON_* was set, PCWSTR otherwise)
        }

        public enum TaskDialogElements
        {
            Content,
            ExpandedInformation,
            Footer,
            MainInstruction
        }

        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        public struct TASKDIALOG_BUTTON
        {
            public int nButtonID;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string pszButtonText;
        }

        [SuppressMessage("Microsoft.Design", "CA1049:TypesThatOwnNativeResourcesShouldBeDisposable"), StructLayout(LayoutKind.Sequential, Pack = 4)]
        [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
        internal struct TASKDIALOGCONFIG
        {
            internal uint cbSize;
            internal IntPtr hwndParent;
            internal IntPtr hInstance;
            internal TaskDialogFlags dwFlags;
            internal TaskDialogCommonButtonFlags dwCommonButtons;
            [MarshalAs(UnmanagedType.LPWStr)]
            internal string pszWindowTitle;
            internal IntPtr hMainIcon;
            [MarshalAs(UnmanagedType.LPWStr)]
            internal string pszMainInstruction;
            [MarshalAs(UnmanagedType.LPWStr)]
            internal string pszContent;
            internal uint cButtons;
            //[MarshalAs(UnmanagedType.LPArray)]
            [SuppressMessage("Microsoft.Reliability", "CA2006:UseSafeHandleToEncapsulateNativeResources")]
            internal IntPtr pButtons;
            internal int nDefaultButton;
            internal uint cRadioButtons;
            //[MarshalAs(UnmanagedType.LPArray)]
            internal IntPtr pRadioButtons;
            internal int nDefaultRadioButton;
            [MarshalAs(UnmanagedType.LPWStr)]
            internal string pszVerificationText;
            [MarshalAs(UnmanagedType.LPWStr)]
            internal string pszExpandedInformation;
            [MarshalAs(UnmanagedType.LPWStr)]
            internal string pszExpandedControlText;
            [MarshalAs(UnmanagedType.LPWStr)]
            internal string pszCollapsedControlText;
            internal IntPtr hFooterIcon;
            [MarshalAs(UnmanagedType.LPWStr)]
            internal string pszFooterText;
            [MarshalAs(UnmanagedType.FunctionPtr)]
            internal TaskDialogCallback pfCallback;
            internal IntPtr lpCallbackData;
            internal uint cxWidth;
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        internal static extern IntPtr SendMessage(IntPtr hwnd, int wMsg, IntPtr wParam, IntPtr lParam);

        #endregion

        #region Activation Context

        [DllImport("Kernel32.dll", SetLastError = true)]
        internal static extern ActivationContextSafeHandle CreateActCtx(ref ACTCTX actctx);
        [DllImport("kernel32.dll"), ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        internal static extern void ReleaseActCtx(IntPtr hActCtx);
        [DllImport("Kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool ActivateActCtx(ActivationContextSafeHandle hActCtx, out IntPtr lpCookie);
        [DllImport("Kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool DeactivateActCtx(uint dwFlags, IntPtr lpCookie);

        internal const int ACTCTX_FLAG_ASSEMBLY_DIRECTORY_VALID = 0x004;

        [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
        internal struct ACTCTX
        {
            public int cbSize;
            public uint dwFlags;
            public string lpSource;
            public ushort wProcessorArchitecture;
            public ushort wLangId;
            public string lpAssemblyDirectory;
            public string lpResourceName;
            public string lpApplicationName;
        }

        #endregion
    }
}
