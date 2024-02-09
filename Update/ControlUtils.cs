using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Update;

internal static class ControlUtils
{
    internal static void CenterHOnForm(this Control control, Control parent)
    {
        control.Location = control.Location with { X = (parent.ClientSize.Width / 2) - (control.Width / 2) };
    }

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
    List<KeyValuePair<Control, ControlOriginalColors?>>? controlColors)
    {
        if (controlColors != null)
        {
            ControlOriginalColors? origColors = control is IDarkable
                ? null
                : new ControlOriginalColors(control.ForeColor, control.BackColor);
            controlColors.Add(new KeyValuePair<Control, ControlOriginalColors?>(control, origColors));
        }

        Control.ControlCollection controls = control.Controls;
        int count = controls.Count;
        for (int i = 0; i < count; i++)
        {
            FillControlColorList(
                control: controls[i],
                controlColors: controlColors
            );
        }
    }

    internal static void SetTheme(
        Control baseControl,
        List<KeyValuePair<Control, ControlOriginalColors?>> controlColors,
        VisualTheme theme,
        Func<Component, bool>? excludePredicate = null)
    {
        bool darkMode = theme == VisualTheme.Dark;

        // @DarkModeNote(FillControlColorList): Controls might change their colors after construct
        // Remember to handle this if new controls are added that this applies to.
        if (controlColors.Count == 0)
        {
            FillControlColorList(
                control: baseControl,
                controlColors: (List<KeyValuePair<Control, ControlOriginalColors?>>?)controlColors);
        }

        foreach (var item in controlColors)
        {
            Control control = item.Key;

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

    internal static void SetMessageBoxIcon(PictureBox pictureBox, MessageBoxIcon icon)
    {
        var sii = new Native.SHSTOCKICONINFO();
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
                    => throw new ArgumentOutOfRangeException()
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
                    => null
            };
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
}
