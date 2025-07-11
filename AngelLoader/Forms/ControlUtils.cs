﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using AngelLoader.DataClasses;
using AngelLoader.Forms.CustomControls;
using AngelLoader.Forms.WinFormsNative;
using AngelLoader.Forms.WinFormsNative.Taskbar;
using static AngelLoader.GameSupport;
using static AngelLoader.Global;
using static AngelLoader.Misc;
using static AngelLoader.Utils;

namespace AngelLoader.Forms;

internal static class ControlUtils
{
    #region Suspend/resume drawing

    internal static void SuspendDrawing(this Control control)
    {
        if (!control.IsHandleCreated || !control.Visible) return;
        Native.SendMessageW(control.Handle, Native.WM_SETREDRAW, false, 0);
    }

    internal static void ResumeDrawing(this Control control, bool invalidateInsteadOfRefresh = false)
    {
        if (!control.IsHandleCreated || !control.Visible) return;
        Native.SendMessageW(control.Handle, Native.WM_SETREDRAW, true, 0);
        if (invalidateInsteadOfRefresh)
        {
            control.Invalidate();
        }
        else
        {
            control.Refresh();
        }
    }

    internal static void ResumeDrawingAndFocusControl(
        this Control control,
        Control?[]? controlsToFocus,
        bool invalidateInsteadOfRefresh = false)
    {
        if (!control.IsHandleCreated || !control.Visible) return;
        Native.SendMessageW(control.Handle, Native.WM_SETREDRAW, true, 0);

        /*
        Focus after the enable-redraw message but before the refresh.
        If we put it before the enable-redraw message, the focus will be lost; if we put it after, there's a
        short visible blip of the default focus before we change it.

        Designed originally for the tab control, but use it for whatever...
        */
        if (controlsToFocus != null)
        {
            foreach (Control? c in controlsToFocus)
            {
                c?.Focus();
            }
        }

        if (invalidateInsteadOfRefresh)
        {
            control.Invalidate();
        }
        else
        {
            control.Refresh();
        }
    }

    #endregion

    #region Centering

    internal static void CenterH(this Control control, Control parent, bool clientSize = false)
    {
        int pWidth = clientSize ? parent.ClientSize.Width : parent.Width;
        control.Location = control.Location with { X = (pWidth / 2) - (control.Width / 2) };
    }

#if false
    internal static void CenterV(this Control control, Control parent)
    {
        control.Location = control.Location with { Y = (parent.Height / 2) - (control.Height / 2) };
    }
#endif

    internal static void CenterHV(this Control control, Control parent, bool clientSize = false)
    {
        int pWidth = clientSize ? parent.ClientSize.Width : parent.Width;
        int pHeight = clientSize ? parent.ClientSize.Height : parent.Height;
        control.Location = new Point((pWidth / 2) - (control.Width / 2), (pHeight / 2) - (control.Height / 2));
    }

    #endregion

    #region Autosizing

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

    internal static void HideFocusRectangle(this Control control) => Native.SendMessageW(
        control.Handle,
        Native.WM_CHANGEUISTATE,
        Native.SetControlFocusToHidden,
        0);

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
        _ => TextFormatFlags.Top | TextFormatFlags.Left,
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
        _ => TextFormatFlags.Top | TextFormatFlags.Left,
    };

    #endregion

    #region Scrolling

    internal static Native.SCROLLINFO GetCurrentScrollInfo(nint handle, int direction)
    {
        Native.SCROLLINFO si = new();
        si.cbSize = (uint)Marshal.SizeOf(si);
        si.fMask = (uint)Native.ScrollInfoMask.SIF_ALL;
        Native.GetScrollInfo(handle, direction, ref si);
        return si;
    }

    internal static void RepositionScroll(nint handle, Native.SCROLLINFO si, int direction)
    {
        Native.SetScrollInfo(handle, direction, ref si, true);

        // Send a WM_*SCROLL scroll message using SB_THUMBTRACK as wParam
        // SB_THUMBTRACK: low-order word of wParam, si.nPos high-order word of wParam
        nint ptrWParam = Native.MAKELPARAM((int)Native.SB_THUMBTRACK, si.nPos);
        nint wp = ptrWParam >= 0 ? ptrWParam : (nint)Native.SB_THUMBTRACK;
        Native.SendMessageW(handle, direction == Native.SB_VERT ? Native.WM_VSCROLL : Native.WM_HSCROLL, wp, 0);
    }

    #endregion

    #region Dialogs

    internal static MessageBoxIcon GetIcon(MBoxIcon icon) => icon switch
    {
        MBoxIcon.Information => MessageBoxIcon.Information,
        MBoxIcon.Warning => MessageBoxIcon.Warning,
        MBoxIcon.Error => MessageBoxIcon.Error,
        _ => MessageBoxIcon.None,
    };

    internal static MessageBoxButtons GetButtons(MBoxButtons buttons) => buttons switch
    {
        MBoxButtons.OKCancel => MessageBoxButtons.OKCancel,
        MBoxButtons.YesNo => MessageBoxButtons.YesNo,
        MBoxButtons.YesNoCancel => MessageBoxButtons.YesNoCancel,
        _ => MessageBoxButtons.OK,
    };

    internal static MBoxButton DialogResultToMBoxButton(DialogResult dialogResult) => dialogResult switch
    {
        DialogResult.Yes => MBoxButton.Yes,
        DialogResult.No => MBoxButton.No,
        _ => MBoxButton.Cancel,
    };

    internal static void SetMessageBoxIcon(PictureBox pictureBox, MessageBoxIcon icon)
    {
        Native.SHSTOCKICONINFO sii = new();
        try
        {
            Native.SHSTOCKICONID sysIcon = icon switch
            {
                MessageBoxIcon.Error or
                MessageBoxIcon.Hand or
                MessageBoxIcon.Stop
                    => Native.SHSTOCKICONID.SIID_ERROR,
                MessageBoxIcon.Question
                    => Native.SHSTOCKICONID.SIID_HELP,
                MessageBoxIcon.Exclamation or
                MessageBoxIcon.Warning
                    => Native.SHSTOCKICONID.SIID_WARNING,
                MessageBoxIcon.Asterisk or
                MessageBoxIcon.Information
                    => Native.SHSTOCKICONID.SIID_INFO,
                _
                    => throw new ArgumentOutOfRangeException(),
            };

            sii.cbSize = (uint)Marshal.SizeOf(typeof(Native.SHSTOCKICONINFO));

            int result = Native.SHGetStockIconInfo(sysIcon, Native.SHGSI_ICON, ref sii);
            Marshal.ThrowExceptionForHR(result, new IntPtr(-1));

            pictureBox.Image = Icon.FromHandle(sii.hIcon).ToBitmap();
        }
        catch
        {
            // "Wrong style" image (different style from the MessageBox one) but better than nothing if the
            // above fails
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
                _
                    => null,
            };
        }
        finally
        {
            Native.DestroyIcon(sii.hIcon);
        }
    }

    internal static int GetFlowLayoutPanelControlsWidthAll(FlowLayoutPanelCustom flp)
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
    /// <param name="owner"></param>
    /// <returns></returns>
    internal static DialogResult ShowDialogDark(this CommonDialog dialog, IWin32Window? owner)
    {
        using (new Win32ThemeHooks.DialogScope(active: Config.DarkMode))
        {
            return dialog.ShowDialog(owner);
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
    internal static DialogResult ShowDialogDark(this Form dialog, IWin32Window? owner) => dialog.ShowDialog(owner);

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
            if (_toolTipsReflectable == null)
            {
                using ToolTip testToolTip = new();

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
                        .GetField(WinFormsReflection.ToolTipNativeWindow_ToolTipFieldName,
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

                    if (cons == null!)
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
                    // Ultra paranoid cleanup - this isn't disposable in .NET Framework 4.7.2 at the very least,
                    // but in theory it could be, so dispose it if so!
                    if (tsNativeWindow is IDisposable tsNativeWindowDisposable)
                    {
                        tsNativeWindowDisposable.Dispose();
                    }
                }

                #endregion
            }

            return _toolTipsReflectable == true;

            static bool SetFalse()
            {
                _toolTipRecreateHandleMethod = null;
                _toolTipNativeWindowControlField = null;
                _toolTipNativeWindowClass = null;
                _toolTipsReflectable = false;
                return false;
            }
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
            List<nint> handles = Native.GetProcessWindowHandles();
            foreach (nint handle in handles)
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
                ErrorText.Un + "recreate tooltip handles, meaning their text color has not changed with dark/light theme.",
                ex);
        }
    }

    #endregion

    internal static void DisposeAndClear(this Control.ControlCollection controlCollection)
    {
        foreach (Control? control in controlCollection)
        {
            control?.Dispose();
        }
        controlCollection.Clear();
    }

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

    internal static void FillTreeViewFromTags(
        DarkTreeView treeView,
        FMCategoriesCollection categories,
        bool sort,
        bool selectFirst = false)
    {
        using (new UpdateRegion(treeView))
        {
            treeView.Nodes.Clear();

            if (sort) categories.SortAndMoveMiscToEnd();

            foreach (CatAndTagsList item in categories)
            {
                TreeNode categoryNode = new(item.Category);
                foreach (string tag in item.Tags)
                {
                    categoryNode.Nodes.Add(tag);
                }
                treeView.Nodes.Add(categoryNode);
            }

            treeView.ExpandAll();
            if (selectFirst && treeView.Nodes.Count > 0)
            {
                treeView.SelectedNode = treeView.Nodes[0];
            }
        }
    }

    internal static void AutoSizeFilterWindow(Form form, Button okButton, Button cancelButton)
    {
        Native.NONCLIENTMETRICSW metrics = Native.GetNonClientMetrics();

        int totalButtonsWidth = okButton.Width + okButton.Padding.Horizontal + okButton.Margin.Horizontal +
                                cancelButton.Width + cancelButton.Padding.Horizontal + cancelButton.Margin.Horizontal;

        form.Size = form.Size with
        {
            Width =
            Math.Min(
                MathMax3(
                    num1: form.Width,
                    num2: totalButtonsWidth,
                    num3: metrics.iSMCaptionWidth +
                          TextRenderer.MeasureText(form.Text, SystemFonts.SmallCaptionFont).Width +
                          metrics.iMenuWidth +
                          (metrics.iBorderWidth * 2) +
                          (metrics.iPaddedBorderWidth * 2)),
                800),
        };
    }

    internal static Point ClampFormToScreenBounds(Form parent, Form form, Point desiredLocation)
    {
        Rectangle screenBounds = Screen.FromControl(parent).WorkingArea;
        return new Point(
            desiredLocation.X - ((desiredLocation.X + form.Width) - (screenBounds.X + screenBounds.Width)).ClampToZero(),
            desiredLocation.Y - ((desiredLocation.Y + form.Height) - (screenBounds.Y + screenBounds.Height)).ClampToZero()
        );
    }

    #region Aero Snap window restore hack

    private static bool? _restoredHackReflectable;

    private static FieldInfo? _restoredWindowBoundsField;
    private static FieldInfo? _restoredWindowBoundsSpecifiedField;

    // This is part of the Aero Snap restore hack; it just stops the form from growing by Size - ClientSize
    // every time it gets restored.
    internal static void SetAeroSnapRestoreHackValues(Form form, Point nominalWindowLocation, Size nominalWindowSize)
    {
        if (_restoredHackReflectable == false) return;

        try
        {
            if (_restoredHackReflectable == null)
            {
                const BindingFlags bFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

                _restoredWindowBoundsField = typeof(Form).GetField(WinFormsReflection.Form_RestoredWindowBounds, bFlags);
                _restoredWindowBoundsSpecifiedField = typeof(Form).GetField(WinFormsReflection.Form_RestoredWindowBoundsSpecified, bFlags);

                _restoredHackReflectable = _restoredWindowBoundsField != null && _restoredWindowBoundsSpecifiedField != null;
            }

            if (_restoredHackReflectable == true)
            {
                _restoredWindowBoundsField!.SetValue(form, new Rectangle(nominalWindowLocation, nominalWindowSize));
                _restoredWindowBoundsSpecifiedField!.SetValue(form, BoundsSpecified.None);
            }
        }
        catch
        {
            _restoredHackReflectable = false;
        }
    }

    #endregion

    internal static bool IsMenuKey(KeyEventArgs e) => e.KeyCode == Keys.Apps || (e.Shift && e.KeyCode == Keys.F10);

    #region Show menu

    internal static void ShowMenu(
        DarkContextMenu menu,
        Control control,
        MenuPos pos,
        int xOffset = 0,
        int yOffset = 0,
        bool unstickMenu = false)
    {
        int x = pos is MenuPos.LeftUp or MenuPos.LeftDown or MenuPos.TopRight or MenuPos.BottomRight
            ? 0
            : control.Width;

        int y = pos is MenuPos.LeftDown or MenuPos.TopLeft or MenuPos.TopRight or MenuPos.RightDown
            ? 0
            : control.Height;

        ToolStripDropDownDirection direction = pos switch
        {
            MenuPos.LeftUp or MenuPos.TopLeft => ToolStripDropDownDirection.AboveLeft,
            MenuPos.RightUp or MenuPos.TopRight => ToolStripDropDownDirection.AboveRight,
            MenuPos.LeftDown or MenuPos.BottomLeft => ToolStripDropDownDirection.BelowLeft,
            _ => ToolStripDropDownDirection.BelowRight,
        };

        if (unstickMenu)
        {
            // If menu is stuck to a submenu or something, we need to show and hide it once to get it unstuck,
            // then carry on with the final show below
            menu.Show();
            menu.Hide();
        }

        menu.Show(control, new Point(x + xOffset, y + yOffset), direction);
    }

    #endregion

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

    private static void FillControlColorList(
        Control control,
        List<KeyValuePair<Control, ControlOriginalColors?>>? controlColors,
        bool createControlHandles,
        Func<Control, bool>? createHandlePredicate = null
#if !ReleasePublic && !NoAsserts
        , int stackCounter = 0
#endif
    )
    {
#if !ReleasePublic && !NoAsserts
        const int maxStackCount = 100;
#endif

        if (controlColors != null && control.Tag is not LoadType.Lazy)
        {
            ControlOriginalColors? origColors = control is IDarkable
                ? null
                : new ControlOriginalColors(control.ForeColor, control.BackColor);
            controlColors.Add(new KeyValuePair<Control, ControlOriginalColors?>(control, origColors));
        }

        if (createControlHandles &&
            createHandlePredicate?.Invoke(control) != false &&
            !control.IsHandleCreated)
        {
            _ = control.Handle;
        }

#if !ReleasePublic && !NoAsserts
        stackCounter++;

        AssertR(
            stackCounter <= maxStackCount,
            nameof(FillControlColorList) + "(): stack overflow (" + nameof(stackCounter) + " == " + stackCounter + ", should be <= " + maxStackCount + ")");
#endif

        /*
        Our custom tab control is a special case in that we have the ability to show/hide tabs, which is
        implemented by actually adding and removing the tab pages from the control and keeping them in a
        backing list (that's the only way to do it). So we can run into problems where if a tab page is
        not part of the control (because it's hidden), it will not be hit by this method and therefore
        will never be themed correctly. So handle custom tab controls separately and go through their
        backing lists rather than their Controls collection.
        */
        if (control is DarkTabControl dtc)
        {
            Control[] backingPages = dtc.BackingTabPagesAsControls;
            foreach (Control backingPage in backingPages)
            {
                FillControlColorList(
                    control: backingPage,
                    controlColors: controlColors,
                    createControlHandles: createControlHandles,
                    createHandlePredicate: createHandlePredicate
#if !ReleasePublic && !NoAsserts
                    , stackCounter: stackCounter
#endif
                );
            }
        }
        else
        {
            Control.ControlCollection controls = control.Controls;
            int count = controls.Count;
            for (int i = 0; i < count; i++)
            {
                FillControlColorList(
                    control: controls[i],
                    controlColors: controlColors,
                    createControlHandles: createControlHandles,
                    createHandlePredicate: createHandlePredicate
#if !ReleasePublic && !NoAsserts
                    , stackCounter: stackCounter
#endif
                );
            }
        }
    }

    internal static void CreateAllControlsHandles(Control control, Func<Control, bool> createHandlePredicate)
    {
        FillControlColorList(
            control: control,
            controlColors: null,
            createControlHandles: true,
            createHandlePredicate: createHandlePredicate);
    }

    internal static void SetTheme(
        Control baseControl,
        List<KeyValuePair<Control, ControlOriginalColors?>> controlColors,
        VisualTheme theme,
        Func<Component, bool>? excludePredicate = null,
        bool createControlHandles = false,
        Func<Control, bool>? createHandlePredicate = null,
        int capacity = -1)
    {
        bool darkMode = theme == VisualTheme.Dark;

        // @DarkModeNote(FillControlColorList): Controls might change their colors after construct
        // Remember to handle this if new controls are added that this applies to.
        if (controlColors.Count == 0)
        {
            if (capacity >= 0) controlColors.Capacity = capacity;
            FillControlColorList(
                control: baseControl,
                controlColors: (List<KeyValuePair<Control, ControlOriginalColors?>>?)controlColors,
                createControlHandles: createControlHandles,
                createHandlePredicate: createHandlePredicate);
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

    internal static void DrawCheckMark(Graphics g, Pen pen, RectangleF rect)
    {
        SmoothingMode oldSmoothingMode = g.SmoothingMode;
        g.SmoothingMode = SmoothingMode.HighQuality;

        // First half of checkmark
        g.DrawLine(
            pen,
            rect.Left + 1.5f,
            rect.Top + 6,
            rect.Left + 4.5f,
            rect.Top + 9);

        // Second half of checkmark
        g.DrawLine(
            pen,
            rect.Left + 4.5f,
            rect.Top + 9,
            rect.Left + 10.5f,
            rect.Top + 3);

        g.SmoothingMode = oldSmoothingMode;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static string GetRatingString(int rating, RatingDisplayStyle style)
    {
        return (style == RatingDisplayStyle.FMSel ? rating / 2.0 : rating).ToStrCur();
    }

    /// <summary>
    /// For text that goes in menus: "&" is a reserved character, so escape "&" to "&&"
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    internal static string EscapeAmpersands(this string value) => value.Replace("&", "&&");

    internal static string GetInstallStateText(Game game, bool value, bool multiSelected)
    {
        return game == Game.TDM
            ? value && !multiSelected
                ? LText.Global.SelectFM_DarkMod
                : LText.Global.DeselectFM_DarkMod
            : value
                ? multiSelected
                    ? LText.Global.InstallFMs
                    : LText.Global.InstallFM
                : multiSelected
                    ? LText.Global.UninstallFMs
                    : LText.Global.UninstallFM;
    }

    internal static string ToStringOrEmpty(this object? obj) => obj?.ToString() ?? "";

    internal static bool SystemThemeHasChanged(ref Message m, out VisualTheme newTheme)
    {
        if (m.Msg == Native.WM_SETTINGCHANGE &&
            Config.FollowSystemTheme &&
            (string?)Marshal.PtrToStringUni(m.LParam) is "ImmersiveColorSet")
        {
            newTheme = Core.GetSystemTheme();
            if (newTheme != Config.VisualTheme)
            {
                return true;
            }
        }

        newTheme = default;
        return false;
    }

    internal static string GetWindowTitleString()
    {
#if RELEASE_BETA
        const string betaVer = "4";
        string title = "AngelLoader " + Application.ProductVersion + " beta " + betaVer;
#else
        string title = "AngelLoader " + Application.ProductVersion;
#endif
#if X64
        title += " (64-bit)";
#endif
        return title;
    }

    /*
    The docs are confusing about this. They mention the value "5000" several times, and then they say this:
    "The maximum time you can delay a popup is 5000 milliseconds." If you're just breezing over that line, you
    might read that as "the maximum time you can display a popup for is 5000 milliseconds". But that's false,
    the maximum display time is 32767 milliseconds. But it doesn't say that anywhere on the page, and the fact
    that this is the help page for AutoPopDelay and NOT InitialDelay means you're expecting any line vaguely
    saying "maximum" and then a number will be the maximum value for the property the page is about. But nope.
    */
    internal static void SetMaxDelay(this ToolTip toolTip)
    {
        if (!WinVersion.SupportsPersistentToolTips)
        {
            toolTip.AutoPopDelay = 32767;
        }
    }

    #region Cursor

    public static bool TryCreateCursor(Bitmap bitmap, int xHotspot, int yHotspot, [NotNullWhen(true)] out Cursor? cursor)
    {
        cursor = null;

        Native.ICONINFO iconInfo = new();
        nint iconHandle = 0;
        nint cursorPtr = 0;
        try
        {
            iconHandle = bitmap.GetHicon();

            if (!Native.GetIconInfo(iconHandle, ref iconInfo) || iconHandle == 0)
            {
                return false;
            }

            iconInfo.xHotspot = xHotspot;
            iconInfo.yHotspot = yHotspot;

            // false means it's a cursor (not an icon)
            iconInfo.fIcon = false;

            cursorPtr = Native.CreateIconIndirect(ref iconInfo);
            if (cursorPtr == 0)
            {
                Native.DeleteObject(cursorPtr);
                return false;
            }

            cursor = new Cursor(cursorPtr);
            return true;
        }
        catch
        {
            Native.DeleteObject(cursorPtr);
            return false;
        }
        finally
        {
            Native.DeleteObject(iconInfo.hbmMask);
            Native.DeleteObject(iconInfo.hbmColor);
            Native.DestroyIcon(iconHandle);
        }
    }

    public static Bitmap? CloneWithOpacity(this Bitmap bitmap, float opacity)
    {
        Bitmap? retBmp = null;
        try
        {
            retBmp = new Bitmap(bitmap.Width, bitmap.Height);

            using Graphics g = Graphics.FromImage(retBmp);

            using ImageAttributes imgAttrib = new();

            ColorMatrix opacityMatrix = new() { Matrix33 = opacity };
            imgAttrib.SetColorMatrix(opacityMatrix);

            g.DrawImage(
                image: bitmap,
                destRect: new Rectangle(0, 0, retBmp.Width, retBmp.Height),
                srcX: 0,
                srcY: 0,
                srcWidth: bitmap.Width,
                srcHeight: bitmap.Height,
                srcUnit: GraphicsUnit.Pixel,
                imageAttr: imgAttrib);

            return retBmp;
        }
        catch
        {
            retBmp?.Dispose();
            return null;
        }
    }

    #endregion

    internal static void SetTaskBarState(this Form form, TaskbarStates states)
    {
        if (form.IsHandleCreated)
        {
            TaskBarProgress.SetState(form.Handle, states);
        }
    }

    internal static void SetTaskBarValue(this Form form, int progressValue, int progressMax)
    {
        if (form.IsHandleCreated)
        {
            TaskBarProgress.SetValue(form.Handle, progressValue, progressMax);
        }
    }

    internal static void DrawFocusRectangle(Control c, Graphics g, Rectangle rect, Color color)
    {
        // This draws around the entire control rather than just around the text, but meh. It's good enough.
        // I don't know if there's any way to get the text rectangle reliably, so let's just leave it like
        // this.
        ControlPaint.DrawFocusRectangle(
            g,
            rect,
            // This method doesn't even use the foreColor value. Straight up it just ignores it and passes
            // only backColor. Meh?
            c.ForeColor,
            color);
    }

    /*
    @Wine: We have to call this from every single control that inherits from ScrollableControl, because we have
    to act on the WM_NCPAINT message, and messages ONLY get sent to the exact control that they're meant for.
    So we can't catch it in a main form WndProc() override nor in a message filter.
    */
    internal static void Wine_DrawScrollBarCorner(ScrollableControl control)
    {
        if (!Config.DarkMode ||
            !WinVersion.IsWine ||
            !control.VerticalScroll.Visible ||
            !control.HorizontalScroll.Visible)
        {
            return;
        }

        nint controlHandle = control.Handle;

        Native.SCROLLBARINFO sbi_v = new() { cbSize = Marshal.SizeOf(typeof(Native.SCROLLBARINFO)) };
        Native.GetScrollBarInfo(controlHandle, Native.OBJID_VSCROLL, ref sbi_v);

        Rectangle sbr = sbi_v.rcScrollBar.ToRectangle();

        Point vertSB_BottomPoint = control.PointToClient_Fast(new Point(sbr.Right, sbr.Bottom));

        using Native.GraphicsContext gc = new(controlHandle);
        gc.G.FillRectangle(
            DarkColors.DarkBackgroundBrush,
            vertSB_BottomPoint.X - sbr.Width,
            vertSB_BottomPoint.Y,
            sbr.Width,
            SystemInformation.VerticalScrollBarArrowHeight
        );
    }
}
