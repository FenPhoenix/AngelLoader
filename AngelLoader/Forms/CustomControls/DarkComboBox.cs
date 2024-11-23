using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using AngelLoader.DataClasses;
using AngelLoader.Forms.WinFormsNative;
using JetBrains.Annotations;

namespace AngelLoader.Forms.CustomControls;

public class DarkComboBox : ComboBox, IDarkable, IUpdateRegion
{
    private const int _padding = 10;

    private DarkControlState _buttonState = DarkControlState.Normal;

    // No TextAlign property, so leave constant
    // The Win10 classic-mode combobox doesn't use EndEllipses, it just cuts right off.
    // Good or bad, that's the stock behavior, so let's match it.
    private const TextFormatFlags _textFormat =
        TextFormatFlags.Default |
        TextFormatFlags.VerticalCenter |
        TextFormatFlags.NoPrefix |
        TextFormatFlags.SingleLine;

    private Bitmap? _buffer;

    private bool _darkModeEnabled;
    [PublicAPI]
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool DarkModeEnabled
    {
        set
        {
            if (_darkModeEnabled == value) return;
            _darkModeEnabled = value;

            SetStyle(ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.UserPaint, _darkModeEnabled);

            if (_darkModeEnabled)
            {
                DrawMode = DrawMode.OwnerDrawFixed;
                base.FlatStyle = FlatStyle.Flat;
            }
            else
            {
                DrawMode = DrawMode.Normal;
                base.FlatStyle = FlatStyle.Standard;
            }
        }
    }

    [DefaultValue(false)]
    [PublicAPI]
    public bool FireMouseLeaveOnLeaveWindow { get; set; }

    [DefaultValue(false)]
    [PublicAPI]
    public bool SuppressScrollWheelValueChange { get; set; }

#if DEBUG

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [PublicAPI]
    public new Color ForeColor { get; set; }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [PublicAPI]
    public new Color BackColor { get; set; }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [PublicAPI]
    public new FlatStyle FlatStyle { get; set; }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [PublicAPI]
    public new ComboBoxStyle DropDownStyle { get; set; }

#endif

    public DarkComboBox()
    {
        base.DropDownStyle = ComboBoxStyle.DropDownList;
    }

    protected override Size DefaultSize => new(121, 21);

    private void SetButtonState(DarkControlState buttonState, bool invalidate = true)
    {
        if (_buttonState != buttonState)
        {
            _buttonState = buttonState;
            if (invalidate) InvalidateIfDark();
        }
    }

    private void InvalidateIfDark()
    {
        if (_darkModeEnabled) Invalidate();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _buffer?.Dispose();
            _buffer = null;
        }

        base.Dispose(disposing);
    }

    protected override void OnTabStopChanged(EventArgs e)
    {
        base.OnTabStopChanged(e);
        InvalidateIfDark();
    }

    protected override void OnTabIndexChanged(EventArgs e)
    {
        base.OnTabIndexChanged(e);
        InvalidateIfDark();
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);

        if (!_darkModeEnabled) return;

        SetButtonState((e.Button & MouseButtons.Left) != 0 &&
                       ClientRectangle.Contains(e.Location) &&
                       DroppedDown
            ? DarkControlState.Pressed
            : DarkControlState.Hover);
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);

        if (!_darkModeEnabled) return;

        if (e.Button != MouseButtons.Left) return;

        if (!ClientRectangle.Contains(e.Location)) return;

        SetButtonState(DarkControlState.Pressed);
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);

        if (!_darkModeEnabled) return;

        if (e.Button != MouseButtons.Left || DroppedDown) return;

        SetButtonState(DarkControlState.Normal);
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);

        if (!_darkModeEnabled) return;

        SetButtonState(DarkControlState.Normal);
    }

    protected override void OnMouseCaptureChanged(EventArgs e)
    {
        base.OnMouseCaptureChanged(e);

        if (!_darkModeEnabled) return;

        if (!ClientRectangle.Contains(this.ClientCursorPos()))
        {
            // Don't invalidate here, fixes the issue where if you click and move the mouse quickly onto any
            // item in the dropdown, that item's text will become the main text somehow.
            SetButtonState(DarkControlState.Normal, invalidate: false);
        }
    }

    protected override void OnGotFocus(EventArgs e)
    {
        base.OnGotFocus(e);
        InvalidateIfDark();
    }

    protected override void OnLostFocus(EventArgs e)
    {
        base.OnLostFocus(e);

        if (!_darkModeEnabled) return;

        SetButtonState(ClientRectangle.Contains(this.ClientCursorPos())
            ? DarkControlState.Hover
            : DarkControlState.Normal);
    }

    protected override void OnDropDownClosed(EventArgs e)
    {
        base.OnDropDownClosed(e);

        if (!_darkModeEnabled) return;

        SetButtonState(ClientRectangle.Contains(this.ClientCursorPos())
            ? DarkControlState.Hover
            : DarkControlState.Normal);
    }

    protected override void OnTextChanged(EventArgs e)
    {
        base.OnTextChanged(e);
        InvalidateIfDark();
    }

    protected override void OnTextUpdate(EventArgs e)
    {
        base.OnTextUpdate(e);
        InvalidateIfDark();
    }

    protected override void OnSelectedValueChanged(EventArgs e)
    {
        base.OnSelectedValueChanged(e);
        InvalidateIfDark();
    }

    protected override void OnInvalidated(InvalidateEventArgs e)
    {
        base.OnInvalidated(e);
        PaintCombobox();
    }

    protected override void OnResize(EventArgs e)
    {
        _buffer?.Dispose();
        // Explicitly set null, because we only reinstantiate if null
        _buffer = null;

        base.OnResize(e);

        InvalidateIfDark();
    }

    private void PaintCombobox()
    {
        if (!_darkModeEnabled) return;

        _buffer ??= new Bitmap(ClientRectangle.Width, ClientRectangle.Height);

        using Graphics g = Graphics.FromImage(_buffer);
        Rectangle rect = ClientRectangle;

        SolidBrush textColorBrush = Enabled ? DarkColors.LightTextBrush : DarkColors.DisabledTextBrush;
        Pen borderPen = DarkColors.GreySelectionPen;
        SolidBrush fillColorBrush = DarkColors.LightBackgroundBrush;

        if (Enabled)
        {
            switch (_buttonState)
            {
                case DarkControlState.Hover:
                    fillColorBrush = DarkColors.BlueBackgroundBrush;
                    borderPen = DarkColors.BlueHighlightPen;
                    break;
                case DarkControlState.Pressed:
                    fillColorBrush = DarkColors.DarkBackgroundBrush;
                    break;
            }
        }

        if (Focused && TabStop)
        {
            borderPen = DarkColors.BlueHighlightPen;
        }

        g.FillRectangle(fillColorBrush, rect);

        var borderRect = new Rectangle(rect.Left, rect.Top, rect.Width - 1, rect.Height - 1);
        g.DrawRectangle(borderPen, borderRect);

        const int arrowWidth = 9;
        const int arrowHeight = 4;

        var arrowRect = new Rectangle(
            rect.Width - arrowWidth - (_padding / 2),
            (rect.Height / 2) - (arrowHeight / 2),
            arrowWidth,
            arrowHeight
        );

        Images.PaintArrow9x5(
            g: g,
            direction: Direction.Down,
            area: arrowRect,
            pen: DarkColors.GreyHighlightPen
        );

        string text = SelectedItem?.ToString() ?? Text;

        const int padding = 1;

        var textRect = new Rectangle(rect.Left + padding,
            rect.Top + padding,
            rect.Width - (arrowWidth + 4) - (_padding / 2) - (padding * 2),
            rect.Height - (padding * 2));

        // Explicitly set the fill color so that the antialiasing/ClearType looks right
        TextRenderer.DrawText(g, text, Font, textRect, textColorBrush.Color, fillColorBrush.Color, _textFormat);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        if (!_darkModeEnabled)
        {
            base.OnPaint(e);
            return;
        }

        //if (_buffer == null) PaintCombobox();
        // Just paint it always, because we already check for a null buffer and initialize it in PaintComboBox()
        // and while we exit before then if !_darkModeEnabled, we already will have exited this method anyway
        // if that's the case, so we know dark mode is enabled by the time we get here.
        // Fixes the glitchy drawing bug.
        PaintCombobox();

        e.Graphics.DrawImageUnscaled(_buffer!, Point.Empty);
    }

    protected override void OnDrawItem(DrawItemEventArgs e)
    {
        if (!_darkModeEnabled)
        {
            base.OnDrawItem(e);
            return;
        }

        // Otherwise, we draw the text that's supposed to be in the drop-down overtop of the non-dropped-down
        // item, briefly overwriting the text already there but we're bumped over slightly.
        if (!DroppedDown) return;

        Graphics g = e.Graphics;
        Rectangle rect = e.Bounds;

        bool itemIsHighlighted = (e.State & DrawItemState.Selected) != 0 ||
                                 (e.State & DrawItemState.Focus) != 0;

        Color textColor;
        SolidBrush fillColorBrush;
        if (itemIsHighlighted)
        {
            textColor = DarkColors.Fen_HighlightText;
            fillColorBrush = DarkColors.BlueSelectionBrush;
        }
        else
        {
            textColor = DarkColors.LightText;
            fillColorBrush = DarkColors.LightBackgroundBrush;
        }

        g.FillRectangle(fillColorBrush, rect);

        if (e.Index >= 0 && e.Index < Items.Count)
        {
            string text = Items[e.Index].ToStringOrEmpty();

            const int padding = -1;

            var textRect = new Rectangle(rect.Left + padding,
                rect.Top + padding,
                rect.Width - (padding * 2),
                rect.Height - (padding * 2));

            // Explicitly set the fill color so that the antialiasing/ClearType looks right
            TextRenderer.DrawText(g, text, Font, textRect, textColor, fillColorBrush.Color, _textFormat);
        }
    }

    #region Dropdown fixes/behavior improvements etc.

    protected override void WndProc(ref Message m)
    {
        // If the dropdown is going to go off the right side of the screen, try to reposition it so it always
        // appears fully on-screen
        // TODO(combobox clamp): fix other sides: left, top, bottom
        if (m.Msg == Native.WM_CTLCOLORLISTBOX)
        {
            Point p = this.PointToScreen_Fast(new Point(0, Height));

            Rectangle screenBounds = Screen.FromControl(this).WorkingArea;
            int screenWidth = screenBounds.X + screenBounds.Width;
            bool alignRight = p.X + DropDownWidth > screenWidth;

            int x = alignRight ? p.X - (DropDownWidth - Math.Min(Width, screenWidth - p.X)) : p.X;
            Native.SetWindowPos(m.LParam, IntPtr.Zero, x, p.Y, 0, 0, Native.SWP_NOSIZE);
        }
        // Needed to make the MouseLeave event fire when the mouse moves off the control directly onto another
        // window (other controls work like that automatically, ComboBox doesn't)
        else if (FireMouseLeaveOnLeaveWindow && m.Msg == Native.WM_MOUSELEAVE)
        {
            OnMouseLeave(EventArgs.Empty);
            m.Result = (IntPtr)1;
            // If we return here, the ComboBox remains highlighted even when the mouse leaves.
            // If we don't return here, the OnMouseLeave event gets fired twice. That's irritating, but in
            // this particular case it's fine, it just hides the readme controls twice. But remember in case
            // you want to do anything more complicated...
        }

        base.WndProc(ref m);
    }

    protected override void OnDropDown(EventArgs e)
    {
        DropDownWidth = Math.Max(GetLongestItemTextWidth(), Width);

        base.OnDropDown(e);
    }

    private int GetLongestItemTextWidth()
    {
        int finalWidth = 0;
        foreach (object item in Items)
        {
            if (item is not string itemStr) continue;

            int currentItemWidth = TextRenderer.MeasureText(itemStr, Font, Size.Empty).Width;
            if (finalWidth < currentItemWidth) finalWidth = currentItemWidth;
        }
        return finalWidth;
    }

    [PublicAPI]
    public void DoAutoSize(int rightPadding = 0)
    {
        COMBOBOXINFO cbInfo = new() { cbSize = _comboboxInfoSize };
        GetComboBoxInfo(Handle, ref cbInfo);
        Size = Size with { Width = cbInfo.rcButton.Width + GetLongestItemTextWidth() + rightPadding };
    }

    [PublicAPI]
    public static void DoAutoSizeForSet(DarkComboBox[] comboBoxes, int rightPadding = 0)
    {
        int finalWidth = 0;
        foreach (DarkComboBox comboBox in comboBoxes)
        {
            int currentWidth = comboBox.GetLongestItemTextWidth();
            if (finalWidth < currentWidth) finalWidth = currentWidth;
        }
        foreach (DarkComboBox comboBox in comboBoxes)
        {
            COMBOBOXINFO cbInfo = new() { cbSize = _comboboxInfoSize };
            GetComboBoxInfo(comboBox.Handle, ref cbInfo);
            comboBox.Size = comboBox.Size with { Width = cbInfo.rcButton.Width + finalWidth + rightPadding };
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct COMBOBOXINFO
    {
        internal int cbSize;
        internal Native.RECT rcItem;
        internal Native.RECT rcButton;
        internal int stateButton;
        internal IntPtr hwndCombo;
        internal IntPtr hwndItem;
        internal IntPtr hwndList;

        internal COMBOBOXINFO(int size)
        {
            cbSize = size;
            rcItem = Native.RECT.Empty;
            rcButton = Native.RECT.Empty;
            stateButton = 0;
            hwndCombo = IntPtr.Zero;
            hwndItem = IntPtr.Zero;
            hwndList = IntPtr.Zero;
        }
    };

    private static readonly int _comboboxInfoSize = Marshal.SizeOf(typeof(COMBOBOXINFO));

    [DllImport("user32.dll", CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = true)]
    private static extern bool GetComboBoxInfo(IntPtr hwnd, [In, Out] ref COMBOBOXINFO cbInfo);

    #endregion
}
