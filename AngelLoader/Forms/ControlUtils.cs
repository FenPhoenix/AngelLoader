using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using AL_Common;
using AngelLoader.DataClasses;
using AngelLoader.Forms.CustomControls;
using Gma.System.MouseKeyHook;
using JetBrains.Annotations;
using static AngelLoader.Misc;
using static AngelLoader.WinAPI.Native;

namespace AngelLoader.Forms
{
    internal static class ControlUtils
    {
        // Only one copy of the hook
        internal static readonly IMouseEvents MouseHook = Hook.AppEvents();

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

        internal static bool EqualsIfNotNull(this object? sender, object? equals) => sender != null && equals != null && sender == equals;

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

        private static void FillControlDict(
            Control control,
            Dictionary<Control, (Color ForeColor, Color BackColor)> controlColors,
            bool alsoCreateControlHandles,
            int stackCounter = 0)
        {
            const int maxStackCount = 100;

            if (!controlColors.ContainsKey(control))
            {
                controlColors[control] = (control.ForeColor, control.BackColor);
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
            Dictionary<Control, (Color ForeColor, Color BackColor)> controlColors,
            Func<Component, bool>? excludePredicate = null,
            bool alsoCreateControlHandles = false
            )
        {
            // TODO: @DarkMode(SetTheme): Eventually just codegen the set of all darkable controls
            // So we don't have to have this awkward dictionary fill/loop/manual-set system.

            bool darkMode = theme == VisualTheme.Dark;

            // TODO: @DarkMode(FillControlDict): Controls might change their colors after construct
            // We fixed the Settings window case, but keep this in mind until we're sure we're done.
            if (controlColors.Count == 0) FillControlDict(form, controlColors, alsoCreateControlHandles);

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

        // TODO: @DarkMode: Add this to all controls with alignable text
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

        #region Messageboxes

        public static bool AskToContinue(string message, string title, bool noIcon = false)
        {
            if (Config.VisualTheme == VisualTheme.Dark)
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

            //var yesButton = yes != null ? new TaskDialogButton(yes) : new TaskDialogButton(ButtonType.Yes);
            //var noButton = no != null ? new TaskDialogButton(no) : new TaskDialogButton(ButtonType.No);

            //using var d = new TaskDialog(
            //    title: title,
            //    message: message,
            //    buttons: new[] { yesButton, noButton },
            //    defaultButton: defaultButton == ButtonType.No ? noButton : yesButton,
            //    verificationText: showDontAskAgain ? LText.AlertMessages.DontAskAgain : null,
            //    mainIcon: icon);

            //bool cancel = d.ShowDialog() != yesButton;
            //bool dontAskAgain = d.IsVerificationChecked;
            //return (cancel, dontAskAgain);
        }

        public static void ShowAlert(string message, string title, MessageBoxIcon icon = MessageBoxIcon.Warning)
        {
            if (Config.VisualTheme == VisualTheme.Dark)
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
    }
}
