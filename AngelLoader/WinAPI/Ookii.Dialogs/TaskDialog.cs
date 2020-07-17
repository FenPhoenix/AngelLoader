// Copyright (c) Sven Groot (Ookii.org) 2009
// BSD license; see LICENSE for details.
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using JetBrains.Annotations;

namespace AngelLoader.WinAPI.Ookii.Dialogs
{
    /// <summary>
    /// Displays a Task Dialog.
    /// </summary>
    /// <remarks>
    /// The task dialog contains an application-defined message text and title, icons, and any combination of predefined push buttons.
    /// </remarks>
    /// <threadsafety static="true" instance="false" />
    [PublicAPI]
    public sealed class TaskDialog : Component, IWin32Window
    {
        #region Events

        /// <summary>
        /// Event raised when the user presses F1 while the dialog has focus.
        /// </summary>
        public event EventHandler? HelpRequested; // I might use this, so keeping it

        #endregion

        #region Fields

        private readonly List<TaskDialogButton> _buttons = new List<TaskDialogButton>();
        private NativeMethods.TASKDIALOGCONFIG _config;
        private Dictionary<int, TaskDialogButton>? _buttonsById;
        private IntPtr _handle;

        private readonly TaskDialogButton _defaultButton;

        private readonly TaskDialogIcon _mainIcon;
        private readonly Icon? _customMainIcon;

        #endregion

        #region Constructors

        /// <summary>
        /// 
        /// </summary>
        /// <param name="windowTitle">
        /// The window title of the task dialog. The default is an empty string ("").
        /// </param>
        /// <param name="content">
        /// The dialog's primary content. The default is an empty string ("").
        /// </param>
        /// <param name="buttons"></param>
        /// <param name="defaultButton"></param>
        /// <param name="verificationText">
        /// <para>
        /// The label for the verification checkbox, or an empty string ("") if no verification checkbox
        /// should be shown. The default value is an empty string ("").
        /// </para>
        /// </param>
        /// <param name="verificationChecked">
        /// <para>
        /// Sets whether the verification checkbox should start out checked.
        /// </para>
        /// <para>
        /// This parameter is only meaningful if <paramref name="verificationText"/> is a non-empty string.
        /// </para>
        /// </param>
        /// <param name="mainIcon">
        /// <para>
        /// A <see cref="TaskDialogIcon"/> that indicates the icon to display in the main content area of the task dialog.
        /// The default is <see cref="TaskDialogIcon.Custom"/>.
        /// </para>
        /// <para>
        /// When this property is set to <see cref="TaskDialogIcon.Custom"/>, use the <paramref name="customMainIcon"/> parameter to
        /// specify the icon to use.
        /// </para>
        /// </param>
        /// <param name="customMainIcon">
        /// 
        /// An <see cref="System.Drawing.Icon"/> that represents the icon to display in the main content area of the task dialog,
        /// or <see langword="null" /> if no custom icon should be used. The default value is <see langword="null"/>.
        /// <para>
        /// This property is ignored if the <see cref="_mainIcon"/> property has a value other than <see cref="TaskDialogIcon.Custom"/>.
        /// </para>
        /// </param>
        /// <param name="allowDialogCancellation">
        /// <para>
        /// Sets whether the dialog should be able to be closed using Alt-F4, Escape and the title/ bar's close
        /// button even if no cancel button is specified.
        /// </para>
        /// </param>
        /// <param name="centerParent">
        /// Sets whether the dialog should be centered in the parent window instead of the screen.
        /// </param>
        public TaskDialog(
            string windowTitle,
            string content,
            TaskDialogButton[] buttons,
            TaskDialogButton defaultButton,
            string? verificationText,
            bool verificationChecked = false,
            TaskDialogIcon? mainIcon = null,
            Icon? customMainIcon = null,
            bool allowDialogCancellation = true,
            bool centerParent = false)
        {
            #region Init config

            _config.cbSize = (uint)Marshal.SizeOf(_config);
            _config.pfCallback = TaskDialogCallback;

            #endregion

            if (buttons == null || buttons.Length == 0)
            {
                throw new InvalidOperationException(OokiiResources.TaskDialogNoButtonsError);
            }

            _config.pszWindowTitle = windowTitle;

            _config.pszContent = content;

            #region Set button ids

            _defaultButton = defaultButton;

            int highestId = 9;
            foreach (TaskDialogButton button in buttons)
            {
                _buttons.Add(button);
                if (button.ButtonType == ButtonType.Custom)
                {
                    button.Id = highestId;
                    highestId++;
                }
                else
                {
                    button.Id = (int)button.ButtonType;
                }
            }

            #endregion

            _config.pszVerificationText = verificationText ?? "";

            IsVerificationChecked = verificationChecked;

            SetFlag(NativeMethods.TaskDialogFlags.AllowDialogCancellation, allowDialogCancellation);

            SetFlag(NativeMethods.TaskDialogFlags.PositionRelativeToWindow, centerParent);

            if (mainIcon != null) _mainIcon = (TaskDialogIcon)mainIcon;
            if (customMainIcon != null && mainIcon == TaskDialogIcon.Custom) _customMainIcon = customMainIcon;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets a value that indicates whether the verification checkbox is checked ot not.
        /// </summary>
        /// <value>
        /// <see langword="true" /> if the verification checkbox is checked; otherwise, <see langword="false" />.
        /// </value>
        /// <remarks>
        /// <para>
        ///   Use this property after displaying the dialog to determine whether the check box was checked when
        ///   the user closed the dialog.
        /// </para>
        /// <note>
        ///   This property is only meaningful if a non-empty verification text string was passed into the constructor.
        /// </note>
        /// </remarks>
        public bool IsVerificationChecked
        {
            get => GetFlag(NativeMethods.TaskDialogFlags.VerificationFlagChecked);
            private set
            {
                if (value != IsVerificationChecked)
                {
                    SetFlag(NativeMethods.TaskDialogFlags.VerificationFlagChecked, value);
                    if (IsDialogRunning)
                    {
                        NativeMethods.SendMessage(Handle, (int)NativeMethods.TaskDialogMessages.ClickVerification, new IntPtr(value ? 1 : 0), new IntPtr(0));
                    }
                }
            }
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Shows the task dialog as a modal dialog.
        /// </summary>
        /// <returns>The button that the user clicked. Can be <see langword="null" /> if the user cancelled the dialog using the
        /// title bar close button.</returns>
        /// <remarks>
        /// The dialog will use the active window as its owner.
        /// </remarks>
        /// <exception cref="InvalidOperationException">
        /// <para>
        /// One of the properties or a combination of properties is not valid.
        /// </para>
        /// <para>
        /// -or-
        /// </para>
        /// <para>
        /// The dialog is already running.
        /// </para>
        /// </exception>
        public TaskDialogButton? ShowDialog() => ShowDialog(null);

        /// <summary>
        /// This method is for internal AngelLoader.WinAPI.Ookii.Dialogs use and should not be called from your code.
        /// </summary>
        /// <param name="owner"></param>
        /// <returns></returns>
        public TaskDialogButton? ShowDialog(IWin32Window? owner)
        {
            IntPtr ownerHandle = owner?.Handle ?? NativeMethods.GetActiveWindow();
            return ShowDialog(ownerHandle);
        }

        #endregion

        #region Event methods

        /// <summary>
        /// Raises the <see cref="HelpRequested"/> event.
        /// </summary>
        /// <param name="e">The <see cref="EventArgs"/> containing the data for the event.</param>
        private void OnHelpRequested(EventArgs e) => HelpRequested?.Invoke(this, e);

        #endregion

        #region Private members

        private TaskDialogButton? ShowDialog(IntPtr owner)
        {
            if (IsDialogRunning)
            {
                throw new InvalidOperationException(OokiiResources.TaskDialogRunningError);
            }

            _config.hwndParent = owner;
            _config.dwCommonButtons = 0;
            _config.pButtons = IntPtr.Zero;
            _config.cButtons = 0;
            List<NativeMethods.TASKDIALOG_BUTTON> buttons = SetupButtons();

            SetupIcon();

            try
            {
                #region Set content

                IntPtr newTextPtr = Marshal.StringToHGlobalUni(_config.pszContent ?? "");
                try
                {
                    NativeMethods.SendMessage(Handle, (int)NativeMethods.TaskDialogMessages.SetElementText, new IntPtr((int)NativeMethods.TaskDialogElements.Content), newTextPtr);
                }
                finally
                {
                    if (newTextPtr != IntPtr.Zero) Marshal.FreeHGlobal(newTextPtr);
                }

                #endregion

                #region Set buttons and verification checkbox

                MarshalButtons(buttons, out _config.pButtons, out _config.cButtons);
                int buttonId;
                bool verificationFlagChecked;
                using (new ComCtlv6ActivationContext(true))
                {
                    NativeMethods.TaskDialogIndirect(ref _config, out buttonId, out _, out verificationFlagChecked);
                }

                IsVerificationChecked = verificationFlagChecked;

                return _buttonsById!.TryGetValue(buttonId, out var selectedButton) ? selectedButton : null;

                #endregion
            }
            finally
            {
                CleanUpButtons(ref _config.pButtons, ref _config.cButtons);
            }
        }

        // Intentionally not using the Handle property, since the cross-thread call check should not be performed here.
        private bool IsDialogRunning => _handle != IntPtr.Zero;

        private void SetupIcon()
        {
            const NativeMethods.TaskDialogFlags flag = NativeMethods.TaskDialogFlags.UseHIconMain;

            SetFlag(flag, false);
            if (_mainIcon == TaskDialogIcon.Custom)
            {
                if (_customMainIcon != null)
                {
                    SetFlag(flag, true);
                    _config.hMainIcon = _customMainIcon.Handle;
                }
            }
            else
            {
                _config.hMainIcon = new IntPtr((int)_mainIcon);
            }
        }

        private static void CleanUpButtons(ref IntPtr buttons, ref uint count)
        {
            if (buttons == IntPtr.Zero) return;

            int elementSize = Marshal.SizeOf(typeof(NativeMethods.TASKDIALOG_BUTTON));
            for (int x = 0; x < count; ++x)
            {
                // This'll be safe until they introduce 128 bit machines. :)
                // It's the only way to do it without unsafe code.
                IntPtr offset = new IntPtr(buttons.ToInt64() + x * elementSize);
                Marshal.DestroyStructure(offset, typeof(NativeMethods.TASKDIALOG_BUTTON));
            }
            Marshal.FreeHGlobal(buttons);
            buttons = IntPtr.Zero;
            count = 0;
        }

        private static void MarshalButtons(List<NativeMethods.TASKDIALOG_BUTTON> buttons, out IntPtr buttonsPtr, out uint count)
        {
            buttonsPtr = IntPtr.Zero;
            count = 0;
            if (buttons.Count == 0) return;

            int elementSize = Marshal.SizeOf(typeof(NativeMethods.TASKDIALOG_BUTTON));
            buttonsPtr = Marshal.AllocHGlobal(elementSize * buttons.Count);
            for (int x = 0; x < buttons.Count; ++x)
            {
                // This'll be safe until they introduce 128 bit machines. :)
                // It's the only way to do it without unsafe code.
                IntPtr offset = new IntPtr(buttonsPtr.ToInt64() + x * elementSize);
                Marshal.StructureToPtr(buttons[x], offset, false);
            }
            count = (uint)buttons.Count;
        }

        private List<NativeMethods.TASKDIALOG_BUTTON>
        SetupButtons()
        {
            _buttonsById = new Dictionary<int, TaskDialogButton>();
            var buttons = new List<NativeMethods.TASKDIALOG_BUTTON>();
            _config.nDefaultButton = 0;
            foreach (TaskDialogButton button in _buttons)
            {
                _buttonsById.Add(button.Id, button);

                if (_defaultButton == button) _config.nDefaultButton = button.Id;

                if (button.ButtonType == ButtonType.Custom)
                {
                    if (button.Text.IsEmpty())
                    {
                        throw new InvalidOperationException(OokiiResources.TaskDialogEmptyButtonLabelError);
                    }
                    buttons.Add(new NativeMethods.TASKDIALOG_BUTTON
                    {
                        nButtonID = button.Id,
                        pszButtonText = button.Text
                    });
                }
                else
                {
                    _config.dwCommonButtons |= button.ButtonFlag;
                }
            }
            return buttons;
        }

        private void SetFlag(NativeMethods.TaskDialogFlags flag, bool value)
        {
            if (value) { _config.dwFlags |= flag; } else { _config.dwFlags &= ~flag; }
        }

        private bool GetFlag(NativeMethods.TaskDialogFlags flag) => (_config.dwFlags & flag) != 0;

        private uint TaskDialogCallback(IntPtr hwnd, uint uNotification, IntPtr wParam, IntPtr lParam, IntPtr dwRefData)
        {
            switch ((NativeMethods.TaskDialogNotifications)uNotification)
            {
                case NativeMethods.TaskDialogNotifications.Created:
                    _handle = hwnd;
                    break;
                case NativeMethods.TaskDialogNotifications.Destroyed:
                    _handle = IntPtr.Zero;
                    break;
                case NativeMethods.TaskDialogNotifications.VerificationClicked:
                    IsVerificationChecked = (int)wParam == 1;
                    break;
                case NativeMethods.TaskDialogNotifications.Help:
                    OnHelpRequested(EventArgs.Empty);
                    break;
            }
            return 0;
        }

        private void CheckCrossThreadCall()
        {
            IntPtr handle = _handle;
            if (handle != IntPtr.Zero)
            {
                int windowThreadId = NativeMethods.GetWindowThreadProcessId(handle, out _);
                int threadId = NativeMethods.GetCurrentThreadId();
                if (windowThreadId != threadId)
                {
                    throw new InvalidOperationException(OokiiResources.TaskDialogIllegalCrossThreadCallError);
                }
            }
        }

        #endregion

        #region IWin32Window Members

        /// <summary>
        /// Gets the window handle of the task dialog.
        /// </summary>
        /// <value>
        /// The window handle of the task dialog when it is being displayed, or <see cref="IntPtr.Zero"/> when the dialog
        /// is not being displayed.
        /// </value>
        [Browsable(false)]
        public IntPtr Handle
        {
            get
            {
                CheckCrossThreadCall();
                return _handle;
            }
        }

        #endregion
    }
}
