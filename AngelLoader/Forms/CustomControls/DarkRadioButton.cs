using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using AngelLoader.Forms.WinFormsNative;
using JetBrains.Annotations;

namespace AngelLoader.Forms.CustomControls
{
    public sealed class DarkRadioButton : RadioButton, IDarkable
    {
        #region Field Region

        private DarkControlState _controlState = DarkControlState.Normal;

        private bool _origUseVisualStyleBackColor;

        private bool _spacePressed;

        private const int _radioButtonSize = 12;

        #endregion

        #region Property Region

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new Appearance Appearance => base.Appearance;

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new bool AutoEllipsis => base.AutoEllipsis;

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new Image BackgroundImage => base.BackgroundImage;

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new ImageLayout BackgroundImageLayout => base.BackgroundImageLayout;

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new bool FlatAppearance => false;

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new FlatStyle FlatStyle => base.FlatStyle;

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new Image Image => base.Image;

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new ContentAlignment ImageAlign => base.ImageAlign;

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new int ImageIndex => base.ImageIndex;

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new string ImageKey => base.ImageKey;

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new ImageList ImageList => base.ImageList;

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new ContentAlignment TextAlign => base.TextAlign;

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new TextImageRelation TextImageRelation => base.TextImageRelation;

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new bool UseCompatibleTextRendering => false;

        #endregion

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

                if (_darkModeEnabled)
                {
                    _origUseVisualStyleBackColor = UseVisualStyleBackColor;
                    UseVisualStyleBackColor = false;
                }
                else
                {
                    UseVisualStyleBackColor = _origUseVisualStyleBackColor;
                }

                Invalidate();
            }
        }

        #region Constructor Region

        public DarkRadioButton()
        {
            // Always true in both modes
            SetStyle(ControlStyles.SupportsTransparentBackColor |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.UserPaint, true);
        }

        #endregion

        #region Method Region

        private void SetControlState(DarkControlState controlState)
        {
            if (_controlState != controlState)
            {
                _controlState = controlState;
                if (DarkModeEnabled) Invalidate();
            }
        }

        #endregion

        #region Event Handler Region

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (!DarkModeEnabled) return;

            if (_spacePressed) return;

            SetControlState(e.Button == MouseButtons.Left && ClientRectangle.Contains(e.Location)
            ? DarkControlState.Pressed
            : DarkControlState.Hover);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (!DarkModeEnabled) return;

            if (e.Button == MouseButtons.Left && ClientRectangle.Contains(e.Location))
            {
                SetControlState(DarkControlState.Pressed);
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            if (!DarkModeEnabled) return;

            if (_spacePressed) return;

            SetControlState(DarkControlState.Normal);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);

            if (!DarkModeEnabled) return;

            if (_spacePressed) return;

            SetControlState(DarkControlState.Normal);
        }

        protected override void OnMouseCaptureChanged(EventArgs e)
        {
            base.OnMouseCaptureChanged(e);

            if (!DarkModeEnabled) return;

            if (_spacePressed) return;

            if (!ClientRectangle.Contains(this.PointToClient_Fast(Native.GetCursorPosition_Fast())))
            {
                SetControlState(DarkControlState.Normal);
            }
        }

        protected override void OnGotFocus(EventArgs e)
        {
            base.OnGotFocus(e);

            if (!DarkModeEnabled) return;

            Invalidate();
        }

        protected override void OnLostFocus(EventArgs e)
        {
            base.OnLostFocus(e);

            if (!DarkModeEnabled) return;

            _spacePressed = false;

            SetControlState(!ClientRectangle.Contains(this.PointToClient_Fast(Native.GetCursorPosition_Fast()))
                ? DarkControlState.Normal
                : DarkControlState.Hover);
        }

        #endregion

        #region Paint Region

        protected override void OnPaint(PaintEventArgs e)
        {
            if (!DarkModeEnabled)
            {
                base.OnPaint(e);
                return;
            }

            var g = e.Graphics;

            var textColor = DarkColors.LightText;
            var borderColorPen = DarkColors.LightTextPen;
            var fillColorBrush = DarkColors.LightTextBrush;

            if (Enabled)
            {
                if (Focused)
                {
                    borderColorPen = DarkColors.BlueHighlightPen;
                    fillColorBrush = DarkColors.BlueSelectionBrush;
                }

                if (_controlState == DarkControlState.Hover)
                {
                    borderColorPen = DarkColors.BlueHighlightPen;
                    fillColorBrush = DarkColors.BlueSelectionBrush;
                }
                else if (_controlState == DarkControlState.Pressed)
                {
                    borderColorPen = DarkColors.GreyHighlightPen;
                    fillColorBrush = DarkColors.GreySelectionBrush;
                }
            }
            else
            {
                textColor = DarkColors.DisabledText;
                borderColorPen = DarkColors.GreyHighlightPen;
                fillColorBrush = DarkColors.GreySelectionBrush;
            }

            g.FillRectangle(DarkColors.Fen_ControlBackgroundBrush, ClientRectangle);

            g.SmoothingMode = SmoothingMode.HighQuality;

            var boxRect = new Rectangle(0, (ClientRectangle.Height / 2) - (_radioButtonSize / 2), _radioButtonSize, _radioButtonSize);
            g.DrawEllipse(borderColorPen, boxRect);

            if (Checked)
            {
                var checkRect = new Rectangle(3, (ClientRectangle.Height / 2) - ((_radioButtonSize - 7) / 2) - 1, _radioButtonSize - 6, _radioButtonSize - 6);
                g.FillEllipse(fillColorBrush, checkRect);
            }

            g.SmoothingMode = SmoothingMode.Default;

            TextFormatFlags textFormatFlags =
                ControlUtils.GetTextAlignmentFlags(TextAlign) |
                TextFormatFlags.NoClipping |
                TextFormatFlags.NoPrefix |
                TextFormatFlags.WordBreak;

            var textRect = new Rectangle(_radioButtonSize + 4, 0, ClientRectangle.Width - _radioButtonSize, ClientRectangle.Height);
            TextRenderer.DrawText(g, Text, Font, textRect, textColor, textFormatFlags);
        }

        #endregion
    }
}
