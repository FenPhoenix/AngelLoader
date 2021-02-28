using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using JetBrains.Annotations;

namespace AngelLoader.Forms.CustomControls
{
    // TODO: @DarkMode(DarkCheckBox):
    // -Add support for putting the checkbox on the right-hand side
    public sealed class DarkCheckBox : CheckBox, IDarkable
    {
        #region Field Region

        private DarkControlState _controlState = DarkControlState.Normal;

        private bool _spacePressed;

        #endregion

        private bool _darkModeEnabled;
        [PublicAPI]
        [Browsable(false)]
        public bool DarkModeEnabled
        {
            get => _darkModeEnabled;
            set
            {
                if (_darkModeEnabled == value) return;
                _darkModeEnabled = value;
                Invalidate();
            }
        }

        #region Property Region

        //[Browsable(false)]
        //[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        //public new Appearance Appearance
        //{
        //    get { return base.Appearance; }
        //}

        //[Browsable(false)]
        //[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        //public new bool AutoEllipsis
        //{
        //    get { return base.AutoEllipsis; }
        //}

        //[Browsable(false)]
        //[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        //public new Image BackgroundImage
        //{
        //    get { return base.BackgroundImage; }
        //}

        //[Browsable(false)]
        //[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        //public new ImageLayout BackgroundImageLayout
        //{
        //    get { return base.BackgroundImageLayout; }
        //}

        //[Browsable(false)]
        //[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        //public new bool FlatAppearance
        //{
        //    get { return false; }
        //}

        //[Browsable(false)]
        //[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        //public new FlatStyle FlatStyle
        //{
        //    get { return base.FlatStyle; }
        //}

        //[Browsable(false)]
        //[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        //public new Image Image
        //{
        //    get { return base.Image; }
        //}

        //[Browsable(false)]
        //[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        //public new ContentAlignment ImageAlign
        //{
        //    get { return base.ImageAlign; }
        //}

        //[Browsable(false)]
        //[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        //public new int ImageIndex
        //{
        //    get { return base.ImageIndex; }
        //}

        //[Browsable(false)]
        //[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        //public new string ImageKey
        //{
        //    get { return base.ImageKey; }
        //}

        //[Browsable(false)]
        //[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        //public new ImageList ImageList
        //{
        //    get { return base.ImageList; }
        //}

        //[Browsable(false)]
        //[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        //public new ContentAlignment TextAlign
        //{
        //    get { return base.TextAlign; }
        //}

        //[Browsable(false)]
        //[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        //public new TextImageRelation TextImageRelation
        //{
        //    get { return base.TextImageRelation; }
        //}

        //[Browsable(false)]
        //[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        //public new bool ThreeState
        //{
        //    get { return base.ThreeState; }
        //}

        //[Browsable(false)]
        //[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        //public new bool UseCompatibleTextRendering
        //{
        //    get { return false; }
        //}

        //[Browsable(false)]
        //[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        //public new bool UseVisualStyleBackColor
        //{
        //    get { return false; }
        //}

        #endregion

        #region Constructor Region

        public DarkCheckBox()
        {
            // Always true
            SetStyle(ControlStyles.SupportsTransparentBackColor |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.UserPaint, true);
            UseMnemonic = false;
        }

        #endregion

        private void InvalidateIfDark()
        {
            if (_darkModeEnabled) Invalidate();
        }

        #region Method Region

        private void SetControlState(DarkControlState controlState)
        {
            if (_controlState != controlState)
            {
                _controlState = controlState;
                InvalidateIfDark();
            }
        }

        #endregion

        #region Event Handler Region

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (!_darkModeEnabled) return;

            if (_spacePressed) return;

            SetControlState(e.Button == MouseButtons.Left && ClientRectangle.Contains(e.Location)
                ? DarkControlState.Pressed
                : DarkControlState.Hover);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (!_darkModeEnabled) return;

            if (e.Button == MouseButtons.Left && ClientRectangle.Contains(e.Location))
            {
                SetControlState(DarkControlState.Pressed);
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            if (!_darkModeEnabled) return;

            if (_spacePressed) return;

            SetControlState(DarkControlState.Normal);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);

            if (!_darkModeEnabled) return;

            if (_spacePressed) return;

            SetControlState(DarkControlState.Normal);
        }

        protected override void OnMouseCaptureChanged(EventArgs e)
        {
            base.OnMouseCaptureChanged(e);

            if (!_darkModeEnabled) return;

            if (_spacePressed) return;

            if (!ClientRectangle.Contains(Cursor.Position))
            {
                SetControlState(DarkControlState.Normal);
            }
        }

        protected override void OnGotFocus(EventArgs e)
        {
            base.OnGotFocus(e);

            if (!_darkModeEnabled) return;

            InvalidateIfDark();
        }

        protected override void OnLostFocus(EventArgs e)
        {
            base.OnLostFocus(e);

            if (!_darkModeEnabled) return;

            _spacePressed = false;

            SetControlState(ClientRectangle.Contains(Cursor.Position)
                ? DarkControlState.Hover
                : DarkControlState.Normal);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (!_darkModeEnabled) return;

            if (e.KeyCode == Keys.Space)
            {
                _spacePressed = true;
                SetControlState(DarkControlState.Pressed);
            }
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);

            if (!_darkModeEnabled) return;

            if (e.KeyCode == Keys.Space)
            {
                _spacePressed = false;

                SetControlState(ClientRectangle.Contains(Cursor.Position)
                    ? DarkControlState.Hover
                    : DarkControlState.Normal);
            }
        }

        #endregion

        #region Paint Region

        protected override void OnPaint(PaintEventArgs e)
        {
            if (!_darkModeEnabled)
            {
                base.OnPaint(e);
                return;
            }

            var g = e.Graphics;
            var rect = new Rectangle(0, 0, ClientSize.Width, ClientSize.Height);

            var size = Consts.CheckBoxSize;

            var textColorPen = DarkColors.LightTextPen;
            var borderPen = DarkColors.LightTextPen;
            var fillBrush = DarkColors.LightTextBrush;

            if (Enabled)
            {
                if (AutoCheck && Focused)
                {
                    borderPen = DarkColors.BlueHighlightPen;
                    fillBrush = DarkColors.BlueHighlightBrush;
                }

                if (_controlState == DarkControlState.Hover)
                {
                    borderPen = DarkColors.BlueHighlightPen;
                    fillBrush = DarkColors.BlueSelectionBrush;
                }
                else if (_controlState == DarkControlState.Pressed)
                {
                    borderPen = DarkColors.GreyHighlightPen;
                    fillBrush = DarkColors.GreySelectionBrush;
                }
            }
            else
            {
                textColorPen = DarkColors.DisabledTextPen;
                borderPen = DarkColors.GreyHighlightPen;
                fillBrush = DarkColors.GreySelectionBrush;
            }

            Color? parentBackColor = Parent?.BackColor;
            if (parentBackColor != null)
            {
                using var b = new SolidBrush((Color)parentBackColor);
                g.FillRectangle(b, rect);
            }
            else
            {
                g.FillRectangle(DarkColors.GreyBackgroundBrush, rect);
            }

            var outlineBoxRect = new Rectangle(0, (rect.Height / 2) - (size / 2), size, size);
            g.DrawRectangle(borderPen, outlineBoxRect);

            if (CheckState == CheckState.Checked)
            {
                using var p = new Pen(fillBrush, 1.6f);
                SmoothingMode oldSmoothingMode = g.SmoothingMode;

                g.SmoothingMode = SmoothingMode.HighQuality;

                // First half of checkmark
                g.DrawLine(p,
                    outlineBoxRect.Left + 1.5f,
                    outlineBoxRect.Top + 6,
                    outlineBoxRect.Left + 4.5f,
                    outlineBoxRect.Top + 9);

                // Second half of checkmark
                g.DrawLine(p,
                    outlineBoxRect.Left + 4.5f,
                    outlineBoxRect.Top + 9,
                    outlineBoxRect.Left + 10.5f,
                    outlineBoxRect.Top + 3);

                g.SmoothingMode = oldSmoothingMode;
            }
            else if (CheckState == CheckState.Indeterminate)
            {
                var boxRect = new Rectangle(3, ((rect.Height / 2) - ((size - 4) / 2)) + 1, size - 5, size - 5);
                g.FillRectangle(fillBrush, boxRect);
            }

            TextFormatFlags textFormatFlags =
                ControlUtils.GetTextAlignmentFlags(TextAlign) |
                TextFormatFlags.NoPrefix |
                TextFormatFlags.NoClipping|
                TextFormatFlags.WordBreak;

            var textRect = new Rectangle(size + 4, 0, rect.Width - size, rect.Height);
            TextRenderer.DrawText(g, Text, Font, textRect, textColorPen.Color, textFormatFlags);
        }

        #endregion
    }
}
