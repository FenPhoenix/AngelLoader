using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using AngelLoader.DataClasses;
using AngelLoader.Forms.CustomControls;
using AngelLoader.WinAPI;
using JetBrains.Annotations;
using static AngelLoader.Misc;

namespace AngelLoader.Forms
{
    internal static class ControlUtils
    {
        #region Suspend/resume drawing

        internal static void SuspendDrawing(this Control control)
        {
            if (!control.IsHandleCreated || !control.Visible) return;
            Native.SendMessage(control.Handle, Native.WM_SETREDRAW, false, 0);
        }

        internal static void ResumeDrawing(this Control control)
        {
            if (!control.IsHandleCreated || !control.Visible) return;
            Native.SendMessage(control.Handle, Native.WM_SETREDRAW, true, 0);
            control.Refresh();
        }

        #endregion

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

        internal static void HideFocusRectangle(this Control control) => Native.SendMessage(
            control.Handle,
            Native.WM_CHANGEUISTATE,
            new IntPtr(Native.SetControlFocusToHidden),
            new IntPtr(0));

        #region Theming

        internal sealed class ControlOriginalColors
        {
            internal readonly Color ForeColor;
            internal readonly Color BackColor;

            internal ControlOriginalColors(Color foreColor, Color backColor)
            {
                ForeColor = foreColor;
                BackColor = backColor;
            }
        }

        private static void FillControlDict(
            Control control,
            List<KeyValuePair<Control, ControlOriginalColors?>> controlColors,
            bool alsoCreateControlHandles,
            int stackCounter = 0)
        {
            const int maxStackCount = 100;

            if (control.Tag is not LoadType.Lazy)
            {
                ControlOriginalColors? origColors = control is IDarkable
                    ? null
                    : new ControlOriginalColors(control.ForeColor, control.BackColor);
                controlColors.Add(new KeyValuePair<Control, ControlOriginalColors?>(control, origColors));
            }

            if (alsoCreateControlHandles && !control.IsHandleCreated)
            {
                IntPtr dummy = control.Handle;
            }

            stackCounter++;

            AssertR(
                stackCounter <= maxStackCount,
                nameof(FillControlDict) + "(): stack overflow (" + nameof(stackCounter) + " == " + stackCounter + ", should be <= " + maxStackCount + ")");

            // Our custom tab control is a special case in that we have the ability to show/hide tabs, which is
            // implemented by actually adding and removing the tab pages from the control and keeping them in a
            // backing list (that's the only way to do it). So we can run into problems where if a tab page is
            // not part of the control (because it's hidden), it will not be hit by this method and therefore
            // will never be themed correctly. So handle custom tab controls separately and go through their
            // backing lists rather than their Controls collection.
            if (control is DarkTabControl dtc)
            {
                var backingPages = dtc.BackingTabPages;
                for (int i = 0; i < backingPages.Length; i++)
                {
                    FillControlDict(backingPages[i], controlColors, alsoCreateControlHandles, stackCounter);
                }
            }
            else
            {
                for (int i = 0; i < control.Controls.Count; i++)
                {
                    FillControlDict(control.Controls[i], controlColors, alsoCreateControlHandles, stackCounter);
                }
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

            // Special-case the custom tab control, see control dictionary filler method for explanation
            if (control is DarkTabControl dtc)
            {
                var backingPages = dtc.BackingTabPages;
                for (int i = 0; i < backingPages.Length; i++)
                {
                    CreateAllControlsHandles(backingPages[i], stackCounter);
                }
            }
            else
            {
                for (int i = 0; i < control.Controls.Count; i++)
                {
                    CreateAllControlsHandles(control.Controls[i], stackCounter);
                }
            }
        }

        internal static void ChangeFormThemeMode(
            VisualTheme theme,
            Form form,
            List<KeyValuePair<Control, ControlOriginalColors?>> controlColors,
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
                        if (tsItem is IDarkable darkableTSItem && (excludePredicate == null || !excludePredicate(tsItem)))
                        {
                            darkableTSItem.DarkModeEnabled = darkMode;
                        }
                    }
                }

                // We might want to exclude a ToolStrip but not its subcomponents, so we put this check after the
                // ToolStrip component check
                if (excludePredicate?.Invoke(control) == true) continue;

                if (control is IDarkable darkableControl)
                {
                    darkableControl.DarkModeEnabled = darkMode;
                }
                else
                {
                    (control.ForeColor, control.BackColor) =
                        darkMode
                            ? (DarkColors.LightText, DarkColors.Fen_ControlBackground)
                            : (item.Value!.ForeColor, item.Value!.BackColor);
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

        internal static Native.SCROLLINFO GetCurrentScrollInfo(IntPtr handle, int direction)
        {
            var si = new Native.SCROLLINFO();
            si.cbSize = (uint)Marshal.SizeOf(si);
            si.fMask = (uint)Native.ScrollInfoMask.SIF_ALL;
            Native.GetScrollInfo(handle, direction, ref si);
            return si;
        }

        internal static void RepositionScroll(IntPtr handle, Native.SCROLLINFO si, int direction)
        {
            // Reposition scroll
            Native.SetScrollInfo(handle, direction, ref si, true);

            // Send a WM_*SCROLL scroll message using SB_THUMBTRACK as wParam
            // SB_THUMBTRACK: low-order word of wParam, si.nPos high-order word of wParam
            IntPtr ptrWParam = new IntPtr(Native.SB_THUMBTRACK + (0x10000 * si.nPos));
            IntPtr ptrLParam = new IntPtr(0);

            IntPtr wp = (long)ptrWParam >= 0 ? ptrWParam : (IntPtr)Native.SB_THUMBTRACK;
            Native.SendMessage(handle, direction == Native.SB_VERT ? Native.WM_VSCROLL : Native.WM_HSCROLL, wp, ptrLParam);
        }

        #endregion

        #region Dialogs

        internal static void SetMessageBoxIcon(PictureBox pictureBox, MessageBoxIcon icon)
        {
            var sii = new Native.SHSTOCKICONINFO();
            try
            {
                // ReSharper disable ConditionIsAlwaysTrueOrFalse
                Native.SHSTOCKICONID sysIcon =
                    icon is MessageBoxIcon.Error or
                            MessageBoxIcon.Hand or
                            MessageBoxIcon.Stop
                  ? Native.SHSTOCKICONID.SIID_ERROR
                  : icon == MessageBoxIcon.Question
                  ? Native.SHSTOCKICONID.SIID_HELP
                  : icon is MessageBoxIcon.Exclamation or
                            MessageBoxIcon.Warning
                  ? Native.SHSTOCKICONID.SIID_WARNING
                  : icon is MessageBoxIcon.Asterisk or
                            MessageBoxIcon.Information
                  ? Native.SHSTOCKICONID.SIID_INFO
                  : throw new ArgumentOutOfRangeException();
                // ReSharper restore ConditionIsAlwaysTrueOrFalse

                sii.cbSize = (uint)Marshal.SizeOf(typeof(Native.SHSTOCKICONINFO));

                int result = Native.SHGetStockIconInfo(sysIcon, Native.SHGSI_ICON, ref sii);
                Marshal.ThrowExceptionForHR(result, new IntPtr(-1));

                pictureBox.Image = Icon.FromHandle(sii.hIcon).ToBitmap();
            }
            catch
            {
                // "Wrong style" image (different style from the MessageBox one) but better than nothing if the
                // above fails
                // ReSharper disable ConditionIsAlwaysTrueOrFalse
                pictureBox.Image = icon switch
                {
                    MessageBoxIcon.Error or
                    MessageBoxIcon.Hand or
                    MessageBoxIcon.Stop
                        => SystemIcons.Error.ToBitmap(),
                    MessageBoxIcon.Question
                        => SystemIcons.Question.ToBitmap(),
                    MessageBoxIcon.Exclamation or
                    MessageBoxIcon.Warning
                        => SystemIcons.Warning.ToBitmap(),
                    MessageBoxIcon.Asterisk or
                    MessageBoxIcon.Information
                        => SystemIcons.Information.ToBitmap(),
                    _ => null
                };
                // ReSharper restore ConditionIsAlwaysTrueOrFalse
            }
            finally
            {
                Native.DestroyIcon(sii.hIcon);
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

        /// <summary>
        /// Shows a dialog with hooked theming disabled, to prevent themed elements being drawn in the unthemed
        /// dialog (half-dark tooltips, dark scrollbars, etc.)
        /// </summary>
        /// <param name="dialog"></param>
        /// <returns></returns>
        internal static DialogResult ShowDialogDark(this CommonDialog dialog)
        {
            using (Config.DarkMode ? new NativeHooks.DialogScope() : null)
            {
                return dialog.ShowDialog();
            }
        }

        /// <summary>
        /// Just redirects to ShowDialog(), but it's so I can make every call ShowDialogDark() so if I find any
        /// ShowDialog() call I'll know it could be a bug (if it's calling a built-in dialog).
        /// </summary>
        /// <param name="dialog"></param>
        /// <returns></returns>
        internal static DialogResult ShowDialogDark(this Form dialog) => dialog.ShowDialog();

        /// <summary>
        /// Just redirects to ShowDialog(), but it's so I can make every call ShowDialogDark() so if I find any
        /// ShowDialog() call I'll know it could be a bug (if it's calling a built-in dialog).
        /// </summary>
        /// <param name="dialog"></param>
        /// <param name="owner"></param>
        /// <returns></returns>
        internal static DialogResult ShowDialogDark(this Form dialog, IWin32Window owner) => dialog.ShowDialog(owner);

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
                        // field's value succeeds and its value is not null, then we know this field is guaranteed
                        // to be initialized by the time the ToolTipNativeWindow class is constructed.
                        if (_toolTipNativeWindowControlField.GetValue(tsNativeWindow) == null)
                        {
                            return SetFalse();
                        }

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

        internal static void DisposeAndClear(this Control.ControlCollection controlCollection)
        {
            foreach (Control? control in controlCollection) control?.Dispose();
            controlCollection.Clear();
        }

        internal static Font GetMicrosoftSansSerifDefault() => new("Microsoft Sans Serif", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 0);

        internal static void SetFontStyle(this Control control, FontStyle fontStyle)
        {
            Font f = control.Font;
            control.Font = new Font(
                f.FontFamily,
                f.Size,
                fontStyle,
                f.Unit,
                f.GdiCharSet,
                f.GdiVerticalFont);
        }

        internal static void FillTreeViewFromTags_Sorted(TreeView treeView, FMCategoriesCollection categories)
        {
            categories.SortAndMoveMiscToEnd();

            foreach (CatAndTagsList item in categories)
            {
                var categoryNode = new TreeNode(item.Category);
                foreach (string tag in item.Tags) categoryNode.Nodes.Add(tag);
                treeView.Nodes.Add(categoryNode);
            }
        }
    }
}
