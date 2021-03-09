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
        internal static IMouseEvents? MouseHook;

        #region Suspend/resume drawing

        internal static void SuspendDrawing_Native(this ISuspendResumable control)
        {
            if (!control.IsHandleCreated || !control.Visible) return;
            SendMessage(control.Handle, WM_SETREDRAW, false, 0);
            control.Suspended = true;
        }

        internal static void ResumeDrawing_Native(this ISuspendResumable control)
        {
            if (!control.IsHandleCreated || !control.Visible) return;
            SendMessage(control.Handle, WM_SETREDRAW, true, 0);
            control.Suspended = false;
            control.Refresh();
        }

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

            #region Add native dark scroll bars to their closest addable parents

            // This prevents other controls in the collection from having their size/location bumped around if we
            // were to just call this just-in-time while the user is dragging. We want to do it while everything
            // is stationary.
            // We could just add these scroll bars to each control manually at init-component time, but we want
            // to avoid doing that as it's error-prone and easy to forget.

            // TODO: @DarkMode(Add native dark scroll bars to their parents):
            // Lazy-loaded controls will be a problem here. We should probably just convert any lazy-loaded
            // scrollable controls back to immediately-loaded again.
            // Menus and buttons are fine to stay lazy-loaded, but check list boxes and panels etc.

            foreach (Control c in controlColors.Keys)
            {
                if (c is IDarkableScrollableNative ids)
                {
                    ids.VerticalVisualScrollBar?.AddToParent();
                    ids.HorizontalVisualScrollBar?.AddToParent();
                    ids.VisualScrollBarCorner?.AddToParent();
                }
            }

            #endregion

            foreach (var item in controlColors)
            {
                Control control = item.Key;

                // Visual scroll bars are always themed by definition of how they work, so just always exclude
                // them here.
                if (control is ScrollBarVisualOnly_Base ||
                    excludePredicate?.Invoke(control) == true)
                {
                    continue;
                }

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
    }
}
