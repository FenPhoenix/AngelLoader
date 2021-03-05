using System;
using System.ComponentModel;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using JetBrains.Annotations;

namespace AngelLoader.Forms.CustomControls
{
    public sealed class DarkNumericUpDown : NumericUpDown, IDarkable
    {
        private Color? _origForeColor;
        private Color? _origBackColor;

        private readonly Point[] _arrowPoints = new Point[3];

        private bool _darkModeEnabled;
        [PublicAPI]
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool DarkModeEnabled
        {
            get => _darkModeEnabled;
            set
            {
                if (_darkModeEnabled == value) return;
                _darkModeEnabled = value;
                SetUpTheme();
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new Color ForeColor { get; set; }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new Color BackColor { get; set; }

        private bool _mouseDown;

        public DarkNumericUpDown()
        {
            Controls[0].Paint += DarkNumericUpDown_Paint;
        }

        private void SetUpTheme()
        {
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

                _origForeColor ??= base.ForeColor;
                _origBackColor ??= base.BackColor;

                base.ForeColor = Color.Gainsboro;
                base.BackColor = DarkColors.LightBackground;
            }
            else
            {
                SetStyle(ControlStyles.OptimizedDoubleBuffer, false);
                SetStyle(ControlStyles.ResizeRedraw | ControlStyles.UserPaint, true);

                if (_origForeColor != null) base.ForeColor = (Color)_origForeColor;
                if (_origBackColor != null) base.BackColor = (Color)_origBackColor;
            }

            try
            {
                // Prevent flickering, only if our assembly has reflection permission
                MethodInfo? method = Controls[0].GetType().GetMethod("SetStyle", BindingFlags.NonPublic | BindingFlags.Instance);
                if (method != null)
                {
                    if (_darkModeEnabled)
                    {
                        method.Invoke(Controls[0], new object[] { ControlStyles.AllPaintingInWmPaint | ControlStyles.DoubleBuffer, true });
                    }
                    else
                    {
                        method.Invoke(Controls[0], new object[] { ControlStyles.AllPaintingInWmPaint, true });
                        method.Invoke(Controls[0], new object[] { ControlStyles.DoubleBuffer, false });
                    }
                }
            }
            catch
            {
                // Oh well...
            }
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

            var g = e.Graphics;
            var clipRect = Controls[0].ClientRectangle;

            g.FillRectangle(DarkColors.HeaderBackgroundBrush, clipRect);

            Point mousePos = Controls[0].PointToClient(Cursor.Position);

            var upArea = new Rectangle(0, 0, clipRect.Width, clipRect.Height / 2);
            bool upHot = upArea.Contains(mousePos);

            Brush upBrush = upHot
                ? _mouseDown
                    ? DarkColors.ActiveControlBrush
                    : DarkColors.GreyHighlightBrush
                : DarkColors.GreySelectionBrush;

            ControlPainter.PaintArrow(
                g: g,
                arrowPolygon: _arrowPoints,
                direction: Misc.Direction.Up,
                area: upArea,
                controlEnabled: Enabled,
                brush: upBrush
            );

            var downArea = new Rectangle(0, clipRect.Height / 2, clipRect.Width, clipRect.Height / 2);
            bool downHot = downArea.Contains(mousePos);

            Brush downBrush = downHot
                ? _mouseDown
                    ? DarkColors.ActiveControlBrush
                    : DarkColors.GreyHighlightBrush
                : DarkColors.GreySelectionBrush;

            ControlPainter.PaintArrow(
                g: g,
                arrowPolygon: _arrowPoints,
                direction: Misc.Direction.Down,
                area: downArea,
                controlEnabled: Enabled,
                brush: downBrush
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
}
