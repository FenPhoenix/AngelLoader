using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using AL_Common;
using AngelLoader.DataClasses;
using AngelLoader.Forms.CustomControls;
using AngelLoader.WinAPI;
using JetBrains.Annotations;
using static AngelLoader.Misc;
using static AngelLoader.WinAPI.Native;

namespace AngelLoader.Forms
{
    internal static class ControlUtils
    {
        #region Suspend/resume drawing

        internal static void SuspendDrawing(this Control control)
        {
            if (!control.IsHandleCreated || !control.Visible) return;
            SendMessage(control.Handle, WM_SETREDRAW, false, 0);
        }

        internal static void ResumeDrawing(this Control control)
        {
            if (!control.IsHandleCreated || !control.Visible) return;
            SendMessage(control.Handle, WM_SETREDRAW, true, 0);
            control.Refresh();
        }

        #endregion

        /// <summary>
        /// Sets the progress bar's value instantly. Avoids the la-dee-dah catch-up-when-I-feel-like-it nature of
        /// the progress bar that makes it look annoying and unprofessional.
        /// </summary>
        /// <param name="pb"></param>
        /// <param name="value"></param>
        public static void SetValueInstant(this ProgressBar pb, int value)
        {
            value = value.Clamp(pb.Minimum, pb.Maximum);

            if (value == pb.Maximum)
            {
                pb.Value = pb.Maximum;
            }
            else
            {
                pb.Value = (value + 1).Clamp(pb.Minimum, pb.Maximum);
                pb.Value = value;
            }
        }

        #region Centering

        [PublicAPI]
        internal static void CenterH(this Control control, Control parent)
        {
            control.Location = new Point((parent.Width / 2) - (control.Width / 2), control.Location.Y);
        }

        /*
        [PublicAPI]
        internal static void CenterV(this Control control, Control parent)
        {
            control.Location = new Point(control.Location.X, (parent.Height / 2) - (control.Height / 2));
        }
        */

        [PublicAPI]
        internal static void CenterHV(this Control control, Control parent, bool clientSize = false)
        {
            int pWidth = clientSize ? parent.ClientSize.Width : parent.Width;
            int pHeight = clientSize ? parent.ClientSize.Height : parent.Height;
            control.Location = new Point((pWidth / 2) - (control.Width / 2), (pHeight / 2) - (control.Height / 2));
        }

        #endregion

        #region Autosizing

        // PERF_TODO: These are relatively expensive operations (10ms to make 3 calls from SettingsForm)
        // See if we can manually calculate some or all of this and end up with the same result as if we let the
        // layout do the work as we do now.

        /// <summary>
        /// Special case for buttons that need to be autosized but then have their width adjusted after the fact,
        /// and so are unable to have GrowAndShrink set.
        /// </summary>
        /// <param name="button"></param>
        /// <param name="text"></param>
        internal static void SetTextAutoSize(this Button button, string text)
        {
            button.Text = "";
            button.Width = 2;
            button.Text = text;
        }

        /// <summary>
        /// Sets a <see cref="Button"/>'s text, and autosizes and repositions the <see cref="Button"/> and a
        /// <see cref="TextBox"/> horizontally together to accommodate it.
        /// </summary>
        /// <param name="button"></param>
        /// <param name="textBox"></param>
        /// <param name="text"></param>
        internal static void SetTextForTextBoxButtonCombo(this Button button, TextBox textBox, string text)
        {
            // Quick fix for this not working if layout is suspended.
            // This will then cause any other controls within the same parent to do their full layout.
            // If this becomes a problem, come up with a better solution here.
            button.Parent.ResumeLayout();

            AnchorStyles oldAnchor = button.Anchor;
            button.Anchor = AnchorStyles.Top | AnchorStyles.Left;

            int oldWidth = button.Width;

            button.Text = text;

            int diff =
                button.Width > oldWidth ? -(button.Width - oldWidth) :
                button.Width < oldWidth ? oldWidth - button.Width : 0;

            button.Left += diff;
            // For some reason the diff doesn't work when scaling is > 100% so, yeah
            textBox.Width = button.Left > textBox.Left ? (button.Left - textBox.Left) - 1 : 0;

            button.Anchor = oldAnchor;
        }

        #endregion

        internal static void RemoveAndSelectNearest(this DarkListBox listBox)
        {
            if (listBox.SelectedIndex == -1) return;

            int oldSelectedIndex = listBox.SelectedIndex;

            listBox.Items.RemoveAt(listBox.SelectedIndex);

            if (oldSelectedIndex < listBox.Items.Count && listBox.Items.Count > 1)
            {
                listBox.SelectedIndex = oldSelectedIndex;
            }
            else if (listBox.Items.Count > 1)
            {
                listBox.SelectedIndex = oldSelectedIndex - 1;
            }
            else if (listBox.Items.Count == 1)
            {
                listBox.SelectedIndex = 0;
            }
        }

        internal static void HideFocusRectangle(this Control control) => SendMessage(
            control.Handle,
            WM_CHANGEUISTATE,
            new IntPtr(SetControlFocusToHidden),
            new IntPtr(0));

        internal static void MakeColumnVisible(DataGridViewColumn column, bool visible)
        {
            column.Visible = visible;
            // Fix for zero-height glitch when Rating column gets swapped out when all columns are hidden
            try
            {
                column.Width++;
                column.Width--;
            }
            // stupid OCD check in case adding 1 would take us over 65536
            catch (ArgumentOutOfRangeException)
            {
                column.Width--;
                column.Width++;
            }
        }

        #region Theming

        private static void FillControlDict(
            Control control,
            List<KeyValuePair<Control, (Color ForeColor, Color BackColor)>> controlColors,
            bool alsoCreateControlHandles,
            int stackCounter = 0)
        {
            const int maxStackCount = 100;

            if (control.Tag is not LazyLoaded tag || tag != LazyLoaded.True)
            {
                controlColors.Add(new KeyValuePair<Control, (Color ForeColor, Color BackColor)>(control, (control.ForeColor, control.BackColor)));
            }

            if (alsoCreateControlHandles && !control.IsHandleCreated)
            {
                var dummy = control.Handle;
            }

            stackCounter++;

            AssertR(
                stackCounter <= maxStackCount,
                nameof(FillControlDict) + "(): stack overflow (" + nameof(stackCounter) + " == " + stackCounter + ", should be <= " + maxStackCount + ")");

            for (int i = 0; i < control.Controls.Count; i++)
            {
                FillControlDict(control.Controls[i], controlColors, alsoCreateControlHandles, stackCounter);
            }
        }

        internal static void CreateAllControlsHandles(
            Control control,
            int stackCounter = 0
            )
        {
            const int maxStackCount = 100;

            if (!control.IsHandleCreated)
            {
                var dummy = control.Handle;
            }

            stackCounter++;

            AssertR(
                stackCounter <= maxStackCount,
                nameof(CreateAllControlsHandles) + "(): stack overflow (" + nameof(stackCounter) + " == " + stackCounter + ", should be <= " + maxStackCount + ")");

            for (int i = 0; i < control.Controls.Count; i++)
            {
                CreateAllControlsHandles(control.Controls[i], stackCounter);
            }
        }

        internal static void ChangeFormThemeMode(
            VisualTheme theme,
            Form form,
            List<KeyValuePair<Control, (Color ForeColor, Color BackColor)>> controlColors,
            Func<Component, bool>? excludePredicate = null,
            bool alsoCreateControlHandles = false,
            int capacity = -1
        )
        {
            bool darkMode = theme == VisualTheme.Dark;

            // @DarkModeNote(FillControlDict): Controls might change their colors after construct
            // Remember to handle this if new controls are added that this applies to.
            if (controlColors.Count == 0)
            {
                if (capacity >= 0) controlColors.Capacity = capacity;
                FillControlDict(form, controlColors, alsoCreateControlHandles);
            }

            foreach (var item in controlColors)
            {
                Control control = item.Key;

                // Separate if because a control could be IDarkable AND be a ToolStrip
                if (control is ToolStrip ts)
                {
                    foreach (ToolStripItem tsItem in ts.Items)
                    {
                        if (tsItem is IDarkable darkableTSItem && !excludePredicate?.Invoke(tsItem) == true)
                        {
                            darkableTSItem.DarkModeEnabled = darkMode;
                        }
                    }
                }

                if (control is IDarkable darkableControl)
                {
                    darkableControl.DarkModeEnabled = darkMode;
                }
                else
                {
                    if (darkMode)
                    {
                        control.ForeColor = DarkColors.LightText;
                        control.BackColor = DarkColors.Fen_ControlBackground;
                    }
                    else
                    {
                        control.ForeColor = item.Value.ForeColor;
                        control.BackColor = item.Value.BackColor;
                    }
                }
            }
        }

        #endregion

        #region Text alignment flags

        internal static TextFormatFlags GetTextAlignmentFlags(ContentAlignment textAlign) => textAlign switch
        {
            ContentAlignment.TopLeft => TextFormatFlags.Top | TextFormatFlags.Left,
            ContentAlignment.TopCenter => TextFormatFlags.Top | TextFormatFlags.HorizontalCenter,
            ContentAlignment.TopRight => TextFormatFlags.Top | TextFormatFlags.Right,
            ContentAlignment.MiddleLeft => TextFormatFlags.VerticalCenter | TextFormatFlags.Left,
            ContentAlignment.MiddleCenter => TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter,
            ContentAlignment.MiddleRight => TextFormatFlags.VerticalCenter | TextFormatFlags.Right,
            ContentAlignment.BottomLeft => TextFormatFlags.Bottom | TextFormatFlags.Left,
            ContentAlignment.BottomCenter => TextFormatFlags.Bottom | TextFormatFlags.HorizontalCenter,
            ContentAlignment.BottomRight => TextFormatFlags.Bottom | TextFormatFlags.Right,
            _ => TextFormatFlags.Top | TextFormatFlags.Left
        };

        internal static TextFormatFlags GetTextAlignmentFlags(DataGridViewContentAlignment align) => align switch
        {
            DataGridViewContentAlignment.TopLeft => TextFormatFlags.Top | TextFormatFlags.Left,
            DataGridViewContentAlignment.TopCenter => TextFormatFlags.Top | TextFormatFlags.HorizontalCenter,
            DataGridViewContentAlignment.TopRight => TextFormatFlags.Top | TextFormatFlags.Right,
            DataGridViewContentAlignment.MiddleLeft => TextFormatFlags.VerticalCenter | TextFormatFlags.Left,
            DataGridViewContentAlignment.MiddleCenter => TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter,
            DataGridViewContentAlignment.MiddleRight => TextFormatFlags.VerticalCenter | TextFormatFlags.Right,
            DataGridViewContentAlignment.BottomLeft => TextFormatFlags.Bottom | TextFormatFlags.Left,
            DataGridViewContentAlignment.BottomCenter => TextFormatFlags.Bottom | TextFormatFlags.HorizontalCenter,
            DataGridViewContentAlignment.BottomRight => TextFormatFlags.Bottom | TextFormatFlags.Right,
            _ => TextFormatFlags.Top | TextFormatFlags.Left
        };

        #endregion

        #region Scrolling

        internal static SCROLLINFO GetCurrentScrollInfo(IntPtr handle, int direction)
        {
            var si = new SCROLLINFO();
            si.cbSize = (uint)Marshal.SizeOf(si);
            si.fMask = (uint)ScrollInfoMask.SIF_ALL;
            GetScrollInfo(handle, direction, ref si);
            return si;
        }

        internal static void RepositionScroll(IntPtr handle, SCROLLINFO si, int direction)
        {
            // Reposition scroll
            SetScrollInfo(handle, direction, ref si, true);

            // Send a WM_*SCROLL scroll message using SB_THUMBTRACK as wParam
            // SB_THUMBTRACK: low-order word of wParam, si.nPos high-order word of wParam
            IntPtr ptrWParam = new IntPtr(SB_THUMBTRACK + (0x10000 * si.nPos));
            IntPtr ptrLParam = new IntPtr(0);

            IntPtr wp = (long)ptrWParam >= 0 ? ptrWParam : (IntPtr)SB_THUMBTRACK;
            SendMessage(handle, direction == SB_VERT ? WM_VSCROLL : WM_HSCROLL, wp, ptrLParam);
        }

        #endregion

        #region Messageboxes

        internal static void SetMessageBoxIcon(PictureBox pictureBox, MessageBoxIcon icon)
        {
            var sii = new SHSTOCKICONINFO();
            try
            {
                // ReSharper disable ConditionIsAlwaysTrueOrFalse
                SHSTOCKICONID sysIcon =
                    icon == MessageBoxIcon.Error ||
                    icon == MessageBoxIcon.Hand ||
                    icon == MessageBoxIcon.Stop
                  ? SHSTOCKICONID.SIID_ERROR
                  : icon == MessageBoxIcon.Question
                  ? SHSTOCKICONID.SIID_HELP
                  : icon == MessageBoxIcon.Exclamation ||
                    icon == MessageBoxIcon.Warning
                  ? SHSTOCKICONID.SIID_WARNING
                  : icon == MessageBoxIcon.Asterisk ||
                    icon == MessageBoxIcon.Information
                  ? SHSTOCKICONID.SIID_INFO
                  : throw new ArgumentOutOfRangeException();
                // ReSharper restore ConditionIsAlwaysTrueOrFalse

                sii.cbSize = (uint)Marshal.SizeOf(typeof(SHSTOCKICONINFO));

                int result = SHGetStockIconInfo(sysIcon, SHGSI_ICON, ref sii);
                Marshal.ThrowExceptionForHR(result, new IntPtr(-1));

                pictureBox.Image = Icon.FromHandle(sii.hIcon).ToBitmap();
            }
            catch
            {
                // "Wrong style" image (different style from the MessageBox one) but better than nothing if the
                // above fails
                pictureBox.Image = SystemIcons.Warning.ToBitmap();
            }
            finally
            {
                DestroyIcon(sii.hIcon);
            }
        }

        internal static int GetFlowLayoutPanelControlsWidthAll(FlowLayoutPanel flp)
        {
            int ret = 0;
            for (int i = 0; i < flp.Controls.Count; i++)
            {
                Control c = flp.Controls[i];
                ret += c.Margin.Horizontal + c.Width;
            }
            ret += flp.Padding.Horizontal;

            return ret;
        }

        public static bool AskToContinue(string message, string title, bool noIcon = false)
        {
            if (Config.DarkMode)
            {
                using var d = new DarkTaskDialog(
                    message: message,
                    title: title,
                    icon: noIcon ? MessageBoxIcon.None : MessageBoxIcon.Warning,
                    yesText: LText.Global.Yes,
                    noText: LText.Global.No,
                    defaultButton: DarkTaskDialog.Button.No);
                return d.ShowDialog() == DialogResult.Yes;
            }
            else
            {
                return MessageBox.Show(
                    message,
                    title,
                    MessageBoxButtons.YesNo,
                    noIcon ? MessageBoxIcon.None : MessageBoxIcon.Warning) == DialogResult.Yes;
            }
        }

        public static (bool Cancel, bool Continue, bool DontAskAgain)
        AskToContinueWithCancelCustomStrings(string message, string title, MessageBoxIcon icon, bool showDontAskAgain,
                                             string yes, string no, string cancel, DarkTaskDialog.Button? defaultButton = null)
        {
            using var d = new DarkTaskDialog(
                title: title,
                message: message,
                yesText: yes,
                noText: no,
                cancelText: cancel,
                defaultButton: defaultButton ?? DarkTaskDialog.Button.Cancel,
                checkBoxText: showDontAskAgain ? LText.AlertMessages.DontAskAgain : null,
                icon: icon);

            var result = d.ShowDialog();

            bool canceled = result == DialogResult.Cancel;
            bool cont = result == DialogResult.Yes;
            bool dontAskAgain = d.IsVerificationChecked;
            return (canceled, cont, dontAskAgain);
        }

        public static (bool Cancel, bool DontAskAgain)
        AskToContinueYesNoCustomStrings(string message, string title, MessageBoxIcon icon, bool showDontAskAgain,
                                        string? yes, string? no, DarkTaskDialog.Button? defaultButton = null)
        {
            using var d = new DarkTaskDialog(
                title: title,
                message: message,
                yesText: yes,
                noText: no,
                defaultButton: defaultButton ?? DarkTaskDialog.Button.No,
                checkBoxText: showDontAskAgain ? LText.AlertMessages.DontAskAgain : null,
                icon: icon);

            var result = d.ShowDialog();

            bool cancel = result != DialogResult.Yes;
            bool dontAskAgain = d.IsVerificationChecked;
            return (cancel, dontAskAgain);
        }

        public static void ShowAlert(string message, string title, MessageBoxIcon icon = MessageBoxIcon.Warning)
        {
            if (Config.DarkMode)
            {
                using var d = new DarkTaskDialog(
                    message: message,
                    title: title,
                    icon: icon,
                    yesText: LText.Global.OK,
                    defaultButton: DarkTaskDialog.Button.Yes);
                d.ShowDialog();
            }
            else
            {
                MessageBox.Show(message, title, MessageBoxButtons.OK, icon);
            }
        }

        #endregion

        #region ToolTips

        // Make ABSOLUTELY SURE we're able to do every single reflection thing we need to do to make this work.
        // Any failure whatsoever should result in falling back to classic-mode tooltips so we don't end up with
        // unreadable text!

        private static MethodInfo? _toolTipRecreateHandleMethod;
        private static FieldInfo? _toolTipNativeWindowControlField;
        private static Type? _toolTipNativeWindowClass;

        private static bool? _toolTipsReflectable;

        [MemberNotNullWhen(true,
            nameof(_toolTipRecreateHandleMethod),
            nameof(_toolTipNativeWindowControlField),
            nameof(_toolTipNativeWindowClass))]
        internal static bool ToolTipsReflectable
        {
            get
            {
                static bool SetFalse()
                {
                    _toolTipRecreateHandleMethod = null;
                    _toolTipNativeWindowControlField = null;
                    _toolTipNativeWindowClass = null;
                    _toolTipsReflectable = false;
                    return false;
                }

                if (_toolTipsReflectable == null)
                {
                    using var testToolTip = new ToolTip();

                    const BindingFlags bindingFlags =
                        BindingFlags.Public |
                        BindingFlags.NonPublic |
                        BindingFlags.Instance;

                    #region Check for RecreateHandle method

                    try
                    {
                        _toolTipRecreateHandleMethod = typeof(ToolTip)
                            .GetMethod(
                                "RecreateHandle",
                                bindingFlags);

                        if (_toolTipRecreateHandleMethod == null)
                        {
                            return SetFalse();
                        }

                        // Make sure we can invoke the method and it has the right param count (none in this case)
                        _toolTipRecreateHandleMethod.Invoke(testToolTip, null);
                    }
                    catch
                    {
                        return SetFalse();
                    }

                    #endregion

                    #region Check for nested ToolTipNativeWindow class

                    try
                    {
                        _toolTipNativeWindowClass = typeof(ToolTip)
                            .GetNestedType(
                                "ToolTipNativeWindow",
                                bindingFlags);

                        if (_toolTipNativeWindowClass == null)
                        {
                            return SetFalse();
                        }
                    }
                    catch
                    {
                        return SetFalse();
                    }

                    #endregion

                    #region Check for ToolTip field in ToolTipNativeWindow class

                    try
                    {
                        _toolTipNativeWindowControlField = _toolTipNativeWindowClass
                            .GetField("control",
                                bindingFlags);

                        if (_toolTipNativeWindowControlField == null ||
                            _toolTipNativeWindowControlField.FieldType != typeof(ToolTip))
                        {
                            return SetFalse();
                        }
                    }
                    catch
                    {
                        return SetFalse();
                    }

                    #endregion

                    #region Check for the ToolTip field to be initialized

                    object? tsNativeWindow = null;
                    try
                    {
                        ConstructorInfo[] constructors = _toolTipNativeWindowClass
                            .GetConstructors(
                                bindingFlags);

                        if (constructors.Length != 1)
                        {
                            return SetFalse();
                        }

                        ConstructorInfo? cons = constructors[0];

                        if (cons == null)
                        {
                            return SetFalse();
                        }

                        tsNativeWindow = Activator.CreateInstance(
                            type: _toolTipNativeWindowClass,
                            bindingAttr: bindingFlags,
                            binder: null,
                            args: new object[] { testToolTip },
                            culture: CultureInfo.InvariantCulture);

                        if (tsNativeWindow == null)
                        {
                            return SetFalse();
                        }

                        // At this point, we know we have only one instance constructor, that it takes one param
                        // of type ToolTip, and that there is a ToolTip field in the class. If getting the ToolTip
                        // field's value succeeds, then we know this field is guaranteed to be initialized by the
                        // time the ToolTipNativeWindow class is constructed.
                        _toolTipNativeWindowControlField!.GetValue(tsNativeWindow);

                        _toolTipsReflectable = true;
                    }
                    catch
                    {
                        return SetFalse();
                    }
                    finally
                    {
                        // Ultra paranoid cleanup - this isn't disposable in .NET Framework 4.7.2 at the very
                        // least, but in theory it could be, so dispose it if so!
                        if (tsNativeWindow is IDisposable tsNativeWindowDisposable)
                        {
                            tsNativeWindowDisposable.Dispose();
                        }
                    }

                    #endregion
                }

                return _toolTipsReflectable == true;
            }
        }

        // Tooltips cache their text color (though not their other colors), so they only call the GetThemeColor()
        // method once. That means we can't change their text color at will. Recreating their handles causes them
        // to call GetThemeColor() again, so we can set their text color again.
        // We also can't try to do clever subclassing stuff, because some controls have internal tooltips that we
        // can't set to our subclassed version.
        internal static void RecreateAllToolTipHandles()
        {
            if (!ToolTipsReflectable) return;

            try
            {
                var handles = Native.GetProcessWindowHandles();
                foreach (IntPtr handle in handles)
                {
                    NativeWindow? nw = NativeWindow.FromHandle(handle);
                    if (nw == null || nw.GetType() != _toolTipNativeWindowClass) continue;

                    _toolTipRecreateHandleMethod.Invoke((ToolTip)_toolTipNativeWindowControlField.GetValue(nw), null);
                }
            }
            catch (Exception ex)
            {
                // Because of our exhaustive reflection checks, there should be no exceptions here unless something
                // very, very weird happens. If we do get an exception here, then we'll have wrong-colored (and
                // possibly unreadable) text in tooltips until a restart.
                Logger.Log(
                    "Unable to recreate tooltip handles, meaning their text color has not changed with dark/light theme.",
                    ex);
            }
        }

        #endregion
    }
}
