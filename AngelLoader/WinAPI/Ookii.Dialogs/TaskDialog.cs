// Copyright (c) Sven Groot (Ookii.org) 2009
// BSD license; see LICENSE for details.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing.Design;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using AngelLoader.Forms.CustomControls.Static_LazyLoaded;
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
    [DefaultProperty("MainInstruction"), DefaultEvent("ButtonClicked"), Description("Displays a task dialog.")]
    public sealed class TaskDialog : Component, IWin32Window
    {
        #region Events

        /// <summary>
        /// Event raised when the user presses F1 while the dialog has focus.
        /// </summary>
        [Category("Action"), Description("Event raised when the user presses F1 while the dialog has focus.")]
        public event EventHandler? HelpRequested;

        #endregion

        #region Fields

        private List<TaskDialogButton>? _buttons;
        private NativeMethods.TASKDIALOGCONFIG _config;
        private TaskDialogIcon _mainIcon;
        private System.Drawing.Icon? _customMainIcon;
        private Dictionary<int, TaskDialogButton>? _buttonsById;
        private IntPtr _handle;
        private int _inEventHandler;
        private bool _updatePending;

        #endregion

        #region Constructors

        public TaskDialog(IEnumerable<TaskDialogButton> buttons)
        {
            //int highestId = 10;
            foreach (var button in buttons)
            {
                //button.Owner = this;
                Buttons.Add(button);
                //if (button.ButtonType == ButtonType.Custom)
                //{
                //    button.Id = highestId;
                //    highestId++;
                //}
                //else
                //{
                //    button.Id = (int)button.ButtonType;
                //}
                if (button.ButtonType == ButtonType.Custom)
                {
                    //if (ItemCollection == null) return;

                    int highestId = 9;
                    foreach (TaskDialogButton item in Buttons)
                    {
                        if (item.Id > highestId) highestId = item.Id;
                    }
                    button.Id = highestId + 1;
                }
            }


            TraceWriteButtonIds();
        }

        public void TraceWriteButtonIds()
        {
            foreach (var b in Buttons)
            {
                Trace.WriteLine("b.Id: " + b.Id);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TaskDialog"/> class.
        /// </summary>
        public TaskDialog()
        {
            _config.cbSize = (uint)Marshal.SizeOf(_config);
            _config.pfCallback = TaskDialogCallback;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TaskDialog"/> class with the specified container.
        /// </summary>
        /// <param name="container">The <see cref="IContainer"/> to add the <see cref="TaskDialog"/> to.</param>
        public TaskDialog(IContainer? container)
        {
            container?.Add(this);

            _config.cbSize = (uint)Marshal.SizeOf(_config);
            _config.pfCallback = TaskDialogCallback;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets a list of the buttons on the Task Dialog.
        /// </summary>
        /// <value>
        /// A list of the buttons on the Task Dialog.
        /// </value>
        /// <remarks>
        /// Custom buttons are displayed in the order they have in the collection. Standard buttons will always be displayed
        /// in the Windows-defined order, regardless of the order of the buttons in the collection.
        /// </remarks>
        [Localizable(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
         Category("Appearance"), Description("A list of the buttons on the Task Dialog.")]
        //public TaskDialogItemCollection<TaskDialogButton> Buttons => _buttons ??= new TaskDialogItemCollection<TaskDialogButton>(this);
        public List<TaskDialogButton> Buttons => _buttons ??= new List<TaskDialogButton>();

        /// <summary>
        /// Gets or sets the window title of the task dialog.
        /// </summary>
        /// <value>
        /// The window title of the task dialog. The default is an empty string ("").
        /// </value>
        [Localizable(true), Category("Appearance"), Description("The window title of the task dialog."), DefaultValue("")]
        public string WindowTitle
        {
            get => _config.pszWindowTitle ?? string.Empty;
            set
            {
                _config.pszWindowTitle = string.IsNullOrEmpty(value) ? null : value;
                //UpdateDialog();
            }
        }

        /// <summary>
        /// Gets or sets the dialog's primary content.
        /// </summary>
        /// <value>
        /// The dialog's primary content. The default is an empty string ("").
        /// </value>
        [Localizable(true), Category("Appearance"), Description("The dialog's primary content."), DefaultValue(""), Editor(typeof(System.ComponentModel.Design.MultilineStringEditor), typeof(UITypeEditor))]
        public string Content
        {
            get => _config.pszContent ?? string.Empty;
            set
            {
                _config.pszContent = string.IsNullOrEmpty(value) ? null : value;
                SetElementText(NativeMethods.TaskDialogElements.Content, Content);
            }
        }


        /// <summary>
        /// Gets or sets the icon to display in the task dialog.
        /// </summary>
        /// <value>
        /// A <see cref="TaskDialogIcon"/> that indicates the icon to display in the main content area of the task dialog.
        /// The default is <see cref="TaskDialogIcon.Custom"/>.
        /// </value>
        /// <remarks>
        /// When this property is set to <see cref="TaskDialogIcon.Custom"/>, use the <see cref="CustomMainIcon"/> property to
        /// specify the icon to use.
        /// </remarks>
        [Localizable(true), Category("Appearance"), Description("The icon to display in the task dialog."), DefaultValue(TaskDialogIcon.Custom)]
        public TaskDialogIcon MainIcon
        {
            get => _mainIcon;
            set
            {
                if (_mainIcon != value)
                {
                    _mainIcon = value;
                    //UpdateDialog();
                }
            }
        }

        /// <summary>
        /// Gets or sets a custom icon to display in the dialog.
        /// </summary>
        /// <value>
        /// An <see cref="System.Drawing.Icon"/> that represents the icon to display in the main content area of the task dialog,
        /// or <see langword="null" /> if no custom icon is used. The default value is <see langword="null"/>.
        /// </value>
        /// <remarks>
        /// This property is ignored if the <see cref="MainIcon"/> property has a value other than <see cref="TaskDialogIcon.Custom"/>.
        /// </remarks>
        [Localizable(true), Category("Appearance"), Description("A custom icon to display in the dialog."), DefaultValue(null)]
        public System.Drawing.Icon? CustomMainIcon
        {
            get => _customMainIcon;
            set
            {
                if (_customMainIcon != value)
                {
                    _customMainIcon = value;
                    //UpdateDialog();
                }
            }
        }

        /// <summary>
        /// Gets or sets a value that indicates whether custom buttons should be displayed as normal buttons or command links.
        /// </summary>
        /// <value>
        /// A <see cref="TaskDialogButtonStyle"/> that indicates the display style of custom buttons on the dialog.
        /// The default value is <see cref="TaskDialogButtonStyle.Standard"/>.
        /// </value>
        /// <remarks>
        /// <para>
        ///   This property affects only custom buttons, not standard ones.
        /// </para>
        /// <para>
        ///   If a custom button is being displayed on a task dialog
        ///   with <see cref="TaskDialog.ButtonStyle"/> set to <see cref="AngelLoader.WinAPI.Ookii.Dialogs.TaskDialogButtonStyle.CommandLinks"/>
        ///   or <see cref="AngelLoader.WinAPI.Ookii.Dialogs.TaskDialogButtonStyle.CommandLinksNoIcon"/>, you delineate the command from the 
        ///   note by placing a line break in the string specified by <see cref="TaskDialogItem.Text"/> property.
        /// </para>
        /// </remarks>
        [Category("Behavior"), Description("Indicates whether custom buttons should be displayed as normal buttons or command links."), DefaultValue(TaskDialogButtonStyle.Standard)]
        public TaskDialogButtonStyle ButtonStyle
        {
            get =>
                GetFlag(NativeMethods.TaskDialogFlags.UseCommandLinksNoIcon) ? TaskDialogButtonStyle.CommandLinksNoIcon :
                GetFlag(NativeMethods.TaskDialogFlags.UseCommandLinks) ? TaskDialogButtonStyle.CommandLinks :
                TaskDialogButtonStyle.Standard;
            set
            {
                SetFlag(NativeMethods.TaskDialogFlags.UseCommandLinks, value == TaskDialogButtonStyle.CommandLinks);
                SetFlag(NativeMethods.TaskDialogFlags.UseCommandLinksNoIcon, value == TaskDialogButtonStyle.CommandLinksNoIcon);
                //UpdateDialog();
            }
        }

        /// <summary>
        /// Gets or sets the label for the verification checkbox.
        /// </summary>
        /// <value>
        /// The label for the verification checkbox, or an empty string ("") if no verification checkbox
        /// is shown. The default value is an empty string ("").
        /// </value>
        /// <remarks>
        /// If no text is set, the verification checkbox will not be shown.
        /// </remarks>
        [Localizable(true), Category("Appearance"), Description("The label for the verification checkbox."), DefaultValue("")]
        public string VerificationText
        {
            get => _config.pszVerificationText ?? string.Empty;
            set
            {
                string? realValue = string.IsNullOrEmpty(value) ? null : value;
                if (_config.pszVerificationText != realValue)
                {
                    _config.pszVerificationText = realValue;
                    //UpdateDialog();
                }
            }
        }

        /// <summary>
        /// Gets or sets a value that indicates whether the verification checkbox is checked ot not.
        /// </summary>
        /// <value>
        /// <see langword="true" /> if the verification checkbox is checked; otherwise, <see langword="false" />.
        /// </value>
        /// <remarks>
        /// <para>
        ///   Set this property before displaying the dialog to determine the initial state of the check box.
        ///   Use this property after displaying the dialog to determine whether the check box was checked when
        ///   the user closed the dialog.
        /// </para>
        /// <note>
        ///   This property is only used if <see cref="VerificationText"/> is not an empty string ("").
        /// </note>
        /// </remarks>
        [Category("Behavior"), Description("Indicates whether the verification checkbox is checked ot not."), DefaultValue(false)]
        public bool IsVerificationChecked
        {
            get => GetFlag(NativeMethods.TaskDialogFlags.VerificationFlagChecked);
            set
            {
                if (value != IsVerificationChecked)
                {
                    SetFlag(NativeMethods.TaskDialogFlags.VerificationFlagChecked, value);
                    if (IsDialogRunning) ClickVerification(value, false);
                }
            }
        }

        /// <summary>
        /// Gets or sets a value that indicates that the dialog should be able to be closed using Alt-F4, Escape and the title
        /// bar's close button even if no cancel button is specified.
        /// </summary>
        /// <value>
        /// <see langword="true" /> if the dialog can be closed using Alt-F4, Escape and the title
        /// bar's close button even if no cancel button is specified; otherwise, <see langword="false" />.
        /// The default value is <see langword="false" />.
        /// </value>
        [Category("Behavior"), Description("Indicates that the dialog should be able to be closed using Alt-F4, Escape and the title bar's close button even if no cancel button is specified."), DefaultValue(false)]
        public bool AllowDialogCancellation
        {
            get => GetFlag(NativeMethods.TaskDialogFlags.AllowDialogCancellation);
            set
            {
                if (AllowDialogCancellation != value)
                {
                    SetFlag(NativeMethods.TaskDialogFlags.AllowDialogCancellation, value);
                    //UpdateDialog();
                }
            }
        }

        /// <summary>
        /// Gets or sets a value that indicates whether the dialog is centered in the parent window instead of the screen.
        /// </summary>
        /// <value>
        /// <see langword="true" /> when the dialog is centered relative to the parent window; <see langword="false" /> when it is centered on the screen.
        /// The default value is <see langword="false" />.
        /// </value>
        [Category("Layout"), Description("Indicates whether the dialog is centered in the parent window instead of the screen."), DefaultValue(false)]
        public bool CenterParent
        {
            get => GetFlag(NativeMethods.TaskDialogFlags.PositionRelativeToWindow);
            set
            {
                if (CenterParent != value)
                {
                    SetFlag(NativeMethods.TaskDialogFlags.PositionRelativeToWindow, value);
                    //UpdateDialog();
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
        /// The dialog will use the active window as its owner. If the current process has no active window,
        /// the dialog will be displayed as a modeless dialog (identical to calling <see cref="Show"/>).
        /// </remarks>
        /// <exception cref="InvalidOperationException">
        /// <para>
        ///   One of the properties or a combination of properties is not valid.
        /// </para>
        /// <para>
        ///   -or-
        /// </para>
        /// <para>
        ///   The dialog is already running.
        /// </para>
        /// </exception>
        /// <exception cref="NotSupportedException">Task dialogs are not supported on the current operating system.</exception>
        public TaskDialogButton? ShowDialog() => ShowDialog(null);

        /// <summary>
        /// This method is for internal AngelLoader.WinAPI.Ookii.Dialogs use and should not be called from your code.
        /// </summary>
        /// <param name="owner"></param>
        /// <returns></returns>
        public TaskDialogButton? ShowDialog(IWin32Window? owner)
        {
            //UpdateDialog();

            IntPtr ownerHandle = owner?.Handle ?? NativeMethods.GetActiveWindow();
            return ShowDialog(ownerHandle);
        }

        /// <summary>
        /// Simulates a click on the verification checkbox of the <see cref="TaskDialog"/>, if it exists.
        /// </summary>
        /// <param name="checkState"><see langword="true" /> to set the state of the checkbox to be checked; <see langword="false" /> to set it to be unchecked.</param>
        /// <param name="setFocus"><see langword="true" /> to set the keyboard focus to the checkbox; otherwise <see langword="false" />.</param>
        /// <exception cref="InvalidOperationException">The task dialog is not being displayed.</exception>
        public void ClickVerification(bool checkState, bool setFocus)
        {
            if (!IsDialogRunning)
            {
                throw new InvalidOperationException(OokiiResources.TaskDialogNotRunningError);
            }

            NativeMethods.SendMessage(Handle, (int)NativeMethods.TaskDialogMessages.ClickVerification, new IntPtr(checkState ? 1 : 0), new IntPtr(setFocus ? 1 : 0));
        }

        #endregion

        #region Protected methods

        /// <summary>
        /// Raises the <see cref="HelpRequested"/> event.
        /// </summary>
        /// <param name="e">The <see cref="EventArgs"/> containing the data for the event.</param>
        private void OnHelpRequested(EventArgs e) => HelpRequested?.Invoke(this, e);

        #endregion

        #region Internal Members

        #endregion

        #region Private members

        private TaskDialogButton? ShowDialog(IntPtr owner)
        {
            if (IsDialogRunning)
            {
                throw new InvalidOperationException(OokiiResources.TaskDialogRunningError);
            }

            if (_buttons == null || _buttons.Count == 0)
            {
                throw new InvalidOperationException(OokiiResources.TaskDialogNoButtonsError);
            }

            _config.hwndParent = owner;
            _config.dwCommonButtons = 0;
            _config.pButtons = IntPtr.Zero;
            _config.cButtons = 0;
            List<NativeMethods.TASKDIALOG_BUTTON> buttons = SetupButtons();

            SetupIcon();

            try
            {
                MarshalButtons(buttons, out _config.pButtons, out _config.cButtons);
                int buttonId;
                bool verificationFlagChecked;
                using (new ComCtlv6ActivationContext(true))
                {
                    var tl = new List<string>
                    {
                        nameof(_config.cbSize) + " = " + _config.cbSize,
                        nameof(_config.hwndParent) + " = " + _config.hwndParent,
                        nameof(_config.hInstance) + " = " + _config.hInstance,
                        nameof(_config.dwFlags) + " = " + _config.dwFlags,
                        nameof(_config.dwCommonButtons) + " = " + _config.dwCommonButtons,
                        nameof(_config.pszWindowTitle) + " = " + (_config.pszWindowTitle ?? ""),
                        nameof(_config.hMainIcon) + " = " + _config.hMainIcon,
                        nameof(_config.pszMainInstruction) + " = " + (_config.pszMainInstruction ?? ""),
                        nameof(_config.pszContent) + " = " + (_config.pszContent ?? ""),
                        nameof(_config.cButtons) + " = " + _config.cButtons,
                        nameof(_config.pButtons) + " = " + _config.pButtons,
                        nameof(_config.nDefaultButton) + " = " + _config.nDefaultButton,
                        nameof(_config.cRadioButtons) + " = " + _config.cRadioButtons,
                        nameof(_config.pRadioButtons) + " = " + _config.pRadioButtons,
                        nameof(_config.nDefaultRadioButton) + " = " + _config.nDefaultRadioButton,
                        nameof(_config.pszVerificationText) + " = " + (_config.pszVerificationText ?? ""),
                        nameof(_config.pszExpandedInformation) + " = " + (_config.pszExpandedInformation ?? ""),
                        nameof(_config.pszExpandedControlText) + " = " + (_config.pszExpandedControlText ?? ""),
                        nameof(_config.pszCollapsedControlText) + " = " +
                        (_config.pszCollapsedControlText ?? ""),
                        nameof(_config.hFooterIcon) + " = " + _config.hFooterIcon,
                        nameof(_config.pszFooterText) + " = " + (_config.pszFooterText ?? ""),
                        nameof(_config.pfCallback) + " = " + _config.pfCallback,
                        nameof(_config.lpCallbackData) + " = " + _config.lpCallbackData,
                        nameof(_config.cxWidth) + " = " + _config.cxWidth
                    };

                    using (var sw = new StreamWriter(@"C:\TaskDialog_dump_1.txt"))
                    {
                        foreach (string item in tl) sw.WriteLine(item);
                    }

                    NativeMethods.TaskDialogIndirect(ref _config, out buttonId, out _,
                        out verificationFlagChecked);
                }
                IsVerificationChecked = verificationFlagChecked;

                return _buttonsById!.TryGetValue(buttonId, out var selectedButton) ? selectedButton : null;
            }
            catch (Exception ex)
            {
                Logger.Log("TaskDialog: " + ex);
                throw;
                return null;
            }
            finally
            {
                CleanUpButtons(ref _config.pButtons, ref _config.cButtons);
                CleanUpButtons(ref _config.pRadioButtons, ref _config.cRadioButtons);
            }
        }

        internal void UpdateDialog()
        {
            if (!IsDialogRunning) return;

            // If the navigate page message is sent from within the callback, the navigation won't
            // take place until the callback returns. Any further messages sent after the navigate
            // page message before the end of the callback will then be lost as the navigation occurs.
            // For that reason, we defer it all the way until the end.
            if (_inEventHandler > 0)
            {
                _updatePending = true;
            }
            else
            {
                _updatePending = false;
                CleanUpButtons(ref _config.pButtons, ref _config.cButtons);
                CleanUpButtons(ref _config.pRadioButtons, ref _config.cRadioButtons);
                _config.dwCommonButtons = 0;

                List<NativeMethods.TASKDIALOG_BUTTON> buttons = SetupButtons();

                SetupIcon();

                MarshalButtons(buttons, out _config.pButtons, out _config.cButtons);

                int size = Marshal.SizeOf(_config);
                IntPtr memory = Marshal.AllocHGlobal(size);
                try
                {
                    Marshal.StructureToPtr(_config, memory, false);
                    NativeMethods.SendMessage(Handle, (int)NativeMethods.TaskDialogMessages.NavigatePage,
                        IntPtr.Zero, memory);
                }
                catch (Exception ex)
                {
                    Logger.Log("TaskDialog: ", ex);
                }
                finally
                {
                    Marshal.DestroyStructure(memory, typeof(NativeMethods.TASKDIALOGCONFIG));
                    Marshal.FreeHGlobal(memory);
                }
            }
        }

        // Intentionally not using the Handle property, since the cross-thread call check should not be performed here.
        private bool IsDialogRunning => _handle != IntPtr.Zero;

        private void SetElementText(NativeMethods.TaskDialogElements element, string text)
        {
            if (!IsDialogRunning) return;

            IntPtr newTextPtr = Marshal.StringToHGlobalUni(text);
            try
            {
                NativeMethods.SendMessage(Handle, (int)NativeMethods.TaskDialogMessages.SetElementText, new IntPtr((int)element), newTextPtr);
            }
            finally
            {
                if (newTextPtr != IntPtr.Zero) Marshal.FreeHGlobal(newTextPtr);
            }
        }

        private void SetupIcon() => SetupIcon(MainIcon, CustomMainIcon, NativeMethods.TaskDialogFlags.UseHIconMain);

        private void SetupIcon(TaskDialogIcon icon, System.Drawing.Icon? customIcon, NativeMethods.TaskDialogFlags flag)
        {
            SetFlag(flag, false);
            if (icon == TaskDialogIcon.Custom)
            {
                if (customIcon != null)
                {
                    SetFlag(flag, true);
                    if (flag == NativeMethods.TaskDialogFlags.UseHIconMain)
                    {
                        _config.hMainIcon = customIcon.Handle;
                    }
                    else
                    {
                        _config.hFooterIcon = customIcon.Handle;
                    }
                }
            }
            else
            {
                if (flag == NativeMethods.TaskDialogFlags.UseHIconMain)
                {
                    _config.hMainIcon = new IntPtr((int)icon);
                }
                else
                {
                    _config.hFooterIcon = new IntPtr((int)icon);
                }
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

        private List<NativeMethods.TASKDIALOG_BUTTON> SetupButtons()
        {
            _buttonsById = new Dictionary<int, TaskDialogButton>();
            var buttons = new List<NativeMethods.TASKDIALOG_BUTTON>();
            _config.nDefaultButton = 0;
            foreach (TaskDialogButton button in Buttons)
            {
                if (button.Id < 1)
                {
                    throw new InvalidOperationException(OokiiResources.InvalidTaskDialogItemIdError);
                }

                _buttonsById.Add(button.Id, button);

                if (button.Default) _config.nDefaultButton = button.Id;

                if (button.ButtonType == ButtonType.Custom)
                {
                    if (string.IsNullOrEmpty(button.Text))
                    {
                        throw new InvalidOperationException(OokiiResources.TaskDialogEmptyButtonLabelError);
                    }
                    var taskDialogButton = new NativeMethods.TASKDIALOG_BUTTON
                    {
                        nButtonID = button.Id,
                        pszButtonText = button.Text
                    };
                    if (ButtonStyle == TaskDialogButtonStyle.CommandLinks || ButtonStyle == TaskDialogButtonStyle.CommandLinksNoIcon && !string.IsNullOrEmpty(button.CommandLinkNote))
                    {
                        taskDialogButton.pszButtonText += "\n" + button.CommandLinkNote;
                    }
                    buttons.Add(taskDialogButton);
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
            Interlocked.Increment(ref _inEventHandler);
            try
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
            finally
            {
                Interlocked.Decrement(ref _inEventHandler);
                if (_updatePending) UpdateDialog();
            }
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

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing"><see langword="true" /> if managed resources should be disposed; otherwise, <see langword="false" />.</param>
        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing && _buttons != null)
                {
                    foreach (TaskDialogButton button in _buttons) button.Dispose();
                    _buttons.Clear();
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }
    }
}
