using System;
using System.ComponentModel;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using AngelLoader.DataClasses;
using AngelLoader.Forms.WinFormsNative;
using JetBrains.Annotations;

namespace AngelLoader.Forms.CustomControls;

public sealed class DarkNumericUpDown : NumericUpDown, IDarkable
{
#if false
    private Color? _origForeColor;
    private Color? _origBackColor;
#endif

    private static bool? _setStyleMethodReflectable;
    private static MethodInfo? _setStyleMethod;

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

            // Classic mode original values:

            // This:
            // ControlStyles.OptimizedDoubleBuffer == false
            // ControlStyles.ResizeRedraw == true
            // ControlStyles.UserPaint == true

            // Controls[0]:
            // ControlStyles.AllPaintingInWmPaint == true
            // ControlStyles.DoubleBuffer == false

            if (_darkModeEnabled)
            {
                SetStyle(ControlStyles.OptimizedDoubleBuffer |
                         ControlStyles.ResizeRedraw |
                         ControlStyles.UserPaint, true);

#if false
                _origForeColor ??= base.ForeColor;
                _origBackColor ??= base.BackColor;
#endif

                // @DarkModeNote(NumericUpDown): Fore/back colors don't take when we set them, but they end up correct anyway(?!)
                // My only guess is it's taking the parent's fore/back colors?
#if false
                base.ForeColor = DarkColors.LightText;
                base.BackColor = DarkColors.Fen_DarkBackground;
#endif
            }
            else
            {
                SetStyle(ControlStyles.OptimizedDoubleBuffer, false);
                SetStyle(ControlStyles.ResizeRedraw | ControlStyles.UserPaint, true);

#if false
                if (_origForeColor != null) base.ForeColor = (Color)_origForeColor;
                if (_origBackColor != null) base.BackColor = (Color)_origBackColor;
#endif
            }

            try
            {
                // Prevent flickering, only if our assembly has reflection permission
                if (_setStyleMethodReflectable == null)
                {
                    _setStyleMethod = Controls[0].GetType().GetMethod("SetStyle", BindingFlags.NonPublic | BindingFlags.Instance);
                    _setStyleMethodReflectable = _setStyleMethod != null;
                }
                if (_setStyleMethodReflectable == true)
                {
                    if (_darkModeEnabled)
                    {
                        _setStyleMethod!.Invoke(Controls[0], new object[] { ControlStyles.AllPaintingInWmPaint | ControlStyles.DoubleBuffer, true });
                    }
                    else
                    {
                        _setStyleMethod!.Invoke(Controls[0], new object[] { ControlStyles.AllPaintingInWmPaint, true });
                        _setStyleMethod!.Invoke(Controls[0], new object[] { ControlStyles.DoubleBuffer, false });
                    }
                }
            }
            catch
            {
                // Oh well...
                _setStyleMethodReflectable = false;
            }
        }
    }

#if DEBUG

    [PublicAPI]
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public new Color ForeColor { get; set; }

    [PublicAPI]
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public new Color BackColor { get; set; }

#endif

    private bool _mouseDown;

    public DarkNumericUpDown()
    {
        Controls[0].Paint += DarkNumericUpDown_Paint;
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        if (!_darkModeEnabled)
        {
            base.OnMouseMove(e);
            return;
        }

        Invalidate();
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        if (!_darkModeEnabled)
        {
            base.OnMouseDown(e);
            return;
        }

        _mouseDown = true;
        Invalidate();
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        if (!_darkModeEnabled)
        {
            base.OnMouseUp(e);
            return;
        }

        _mouseDown = false;
        Invalidate();
    }

    protected override void OnMouseEnter(EventArgs e)
    {
        base.OnMouseEnter(e);
        if (_darkModeEnabled) Invalidate();
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);
        if (_darkModeEnabled) Invalidate();
    }

    protected override void OnGotFocus(EventArgs e)
    {
        base.OnGotFocus(e);
        if (_darkModeEnabled) Invalidate();
    }

    protected override void OnLostFocus(EventArgs e)
    {
        base.OnLostFocus(e);
        if (_darkModeEnabled) Invalidate();
    }

    protected override void OnTextBoxLostFocus(object source, EventArgs e)
    {
        base.OnTextBoxLostFocus(source, e);
        if (_darkModeEnabled) Invalidate();
    }

    private void DarkNumericUpDown_Paint(object sender, PaintEventArgs e)
    {
        if (!_darkModeEnabled) return;

        Graphics g = e.Graphics;
        Rectangle clipRect = Controls[0].ClientRectangle;

        g.FillRectangle(DarkColors.HeaderBackgroundBrush, clipRect);

        Point mousePos = Controls[0].ClientCursorPos();

        var upArea = new Rectangle(0, 0, clipRect.Width, clipRect.Height / 2);
        bool upHot = upArea.Contains(mousePos);

        Pen upPen = upHot
            ? _mouseDown
                ? DarkColors.ActiveControlPen
                : DarkColors.GreyHighlightPen
            : DarkColors.GreySelectionPen;

        Images.PaintArrow7x4(
            g: g,
            direction: Direction.Up,
            area: upArea,
            controlEnabled: Enabled,
            pen: upPen
        );

        var downArea = new Rectangle(0, clipRect.Height / 2, clipRect.Width, clipRect.Height / 2);
        bool downHot = downArea.Contains(mousePos);

        Pen downPen = downHot
            ? _mouseDown
                ? DarkColors.ActiveControlPen
                : DarkColors.GreyHighlightPen
            : DarkColors.GreySelectionPen;

        Images.PaintArrow7x4(
            g: g,
            direction: Direction.Down,
            area: downArea,
            controlEnabled: Enabled,
            pen: downPen
        );
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        if (!_darkModeEnabled) return;

        Pen borderPen = Focused && TabStop ? DarkColors.BlueHighlightPen : DarkColors.GreySelectionPen;
        var borderRect = new Rectangle(ClientRectangle.Left, ClientRectangle.Top, ClientRectangle.Width - 1, ClientRectangle.Height - 1);

        e.Graphics.DrawRectangle(borderPen, borderRect);
    }
}
