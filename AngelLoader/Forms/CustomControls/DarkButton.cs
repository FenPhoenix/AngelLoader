using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using JetBrains.Annotations;

namespace AngelLoader.Forms.CustomControls
{
    [ToolboxBitmap(typeof(Button))]
    [DefaultEvent("Click")]
    public class DarkButton : Button, IDarkable
    {
        #region Field Region

        private DarkButtonStyle _style = DarkButtonStyle.Normal;

        private bool _isDefault;
        private bool _spacePressed;

        private int _imagePadding = 5; // Consts.Padding / 2

        private FlatStyle? _originalFlatStyle;
        private int? _originalBorderSize;

        #endregion

        #region Designer Property Region

        [PublicAPI]
        public Color? DarkModeBackColor { get; set; }

        [PublicAPI]
        public new string Text
        {
            get => base.Text;
            set
            {
                base.Text = value;
                InvalidateIfDark();
            }
        }
        [PublicAPI]

        public new bool Enabled
        {
            get => base.Enabled;
            set
            {
                base.Enabled = value;
                InvalidateIfDark();
            }
        }

        [Category("Appearance")]
        [Description("Determines the style of the button.")]
        [DefaultValue(DarkButtonStyle.Normal)]
        [PublicAPI]
        public DarkButtonStyle ButtonStyle
        {
            get => _style;
            set
            {
                _style = value;
                InvalidateIfDark();
            }
        }

        [Category("Appearance")]
        [Description("Determines the amount of padding between the image and text.")]
        [DefaultValue(5)]
        [PublicAPI]
        public int ImagePadding
        {
            get => _imagePadding;
            set
            {
                _imagePadding = value;
                InvalidateIfDark();
            }
        }

        #endregion

        #region Code Property Region

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [PublicAPI]
        public DarkControlState ButtonState { get; private set; } = DarkControlState.Normal;

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [PublicAPI]
        public new FlatStyle FlatStyle
        {
            get => base.FlatStyle;
            set
            {
                base.FlatStyle = value;
                ButtonStyle = value == FlatStyle.Flat
                    ? DarkButtonStyle.Flat
                    : DarkButtonStyle.Normal;
            }
        }

        #endregion

        private bool _darkModeEnabled;
        [PublicAPI]
        public bool DarkModeEnabled
        {
            get => _darkModeEnabled;
            set
            {
                _darkModeEnabled = value;
                SetUpTheme();
            }
        }

        private void SetUpTheme()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.UserPaint, true);

            // Everything needs to be just like this, or else there are cases where the appearance is wrong

            UseCompatibleTextRendering = false;

            if (_darkModeEnabled)
            {
                _originalFlatStyle = base.FlatStyle;
                _originalBorderSize = FlatAppearance.BorderSize;
                UseVisualStyleBackColor = !_darkModeEnabled;
                SetButtonState(DarkControlState.Normal);

                Invalidate();
            }
            else
            {
                // Need to set these explicitly because in some cases (not all) they don't get set back automatically
                ForeColor = SystemColors.ControlText;
                BackColor = SystemColors.Control;
                UseVisualStyleBackColor = true;
                base.FlatStyle = _originalFlatStyle ?? base.FlatStyle;
                FlatAppearance.BorderSize = _originalBorderSize ?? FlatAppearance.BorderSize;
            }
        }

        #region Constructor Region

        public DarkButton()
        {
            UseMnemonic = false;
            SetUpTheme();
        }

        #endregion

        private void InvalidateIfDark()
        {
            if (_darkModeEnabled) Invalidate();
        }

        #region Method Region

        private void SetButtonState(DarkControlState buttonState)
        {
            if (ButtonState != buttonState)
            {
                ButtonState = buttonState;
                InvalidateIfDark();
            }
        }

        #endregion

        #region Event Handler Region

        // Need our own event because we can't fire base.OnPaint() or it overrides our own painting
        public event PaintEventHandler? PaintCustom;

        protected override void OnCreateControl()
        {
            base.OnCreateControl();

            if (!_darkModeEnabled) return;

            Form? form = FindForm();
            if (form != null && form.AcceptButton == this) _isDefault = true;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (!_darkModeEnabled) return;

            if (_spacePressed) return;

            SetButtonState(e.Button == MouseButtons.Left && ClientRectangle.Contains(e.Location)
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

            if (e.Button != MouseButtons.Left) return;

            if (_spacePressed) return;

            SetButtonState(DarkControlState.Normal);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);

            if (!_darkModeEnabled) return;

            if (_spacePressed) return;

            SetButtonState(DarkControlState.Normal);
        }

        protected override void OnMouseCaptureChanged(EventArgs e)
        {
            base.OnMouseCaptureChanged(e);

            if (!_darkModeEnabled) return;

            if (_spacePressed) return;

            if (!ClientRectangle.Contains(Cursor.Position)) SetButtonState(DarkControlState.Normal);
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

            SetButtonState(!ClientRectangle.Contains(Cursor.Position)
                ? DarkControlState.Normal
                : DarkControlState.Hover);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (!_darkModeEnabled) return;

            if (e.KeyCode == Keys.Space)
            {
                _spacePressed = true;
                SetButtonState(DarkControlState.Pressed);
            }
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);

            if (!_darkModeEnabled) return;

            if (e.KeyCode == Keys.Space)
            {
                _spacePressed = false;

                SetButtonState(!ClientRectangle.Contains(Cursor.Position)
                    ? DarkControlState.Normal
                    : DarkControlState.Hover);
            }
        }

        public override void NotifyDefault(bool value)
        {
            base.NotifyDefault(value);

            if (!_darkModeEnabled) return;

            if (!DesignMode) return;

            _isDefault = value;
            InvalidateIfDark();
        }

        #endregion

        #region Paint Region

        protected override void OnPaint(PaintEventArgs e)
        {
            if (!_darkModeEnabled)
            {
                base.OnPaint(e);
                PaintCustom?.Invoke(this, e);
                return;
            }

            var g = e.Graphics;

            var rect = ButtonStyle == DarkButtonStyle.Normal
                // Slightly modified rectangle to account for Flat style being slightly larger than classic mode,
                // this matches us visually in size and position to classic mode
                ? new Rectangle(1, 1, ClientSize.Width - 2, ClientSize.Height - 3)
                : new Rectangle(0, 0, ClientSize.Width, ClientSize.Height);

            var textColorBrush = DarkColors.LightTextBrush;
            var borderPen = DarkColors.GreySelectionPen;
            Color? fillColor = null;
            if (DarkModeBackColor != null) fillColor = DarkModeBackColor;

            if (Enabled)
            {
                if (ButtonStyle == DarkButtonStyle.Normal)
                {
                    if (Focused && TabStop) borderPen = DarkColors.BlueHighlightPen;

                    switch (ButtonState)
                    {
                        case DarkControlState.Hover:
                            fillColor = _isDefault ? DarkColors.BlueBackground : DarkColors.LighterBackground;
                            break;
                        case DarkControlState.Pressed:
                            fillColor = _isDefault ? DarkColors.DarkBackground : DarkColors.DarkBackground;
                            break;
                    }
                }
                else if (ButtonStyle == DarkButtonStyle.Flat)
                {
                    switch (ButtonState)
                    {
                        case DarkControlState.Normal:
                            fillColor = DarkModeBackColor ?? DarkColors.GreyBackground;
                            break;
                        case DarkControlState.Hover:
                            fillColor = DarkColors.MediumBackground;
                            break;
                        case DarkControlState.Pressed:
                            fillColor = DarkColors.DarkBackground;
                            break;
                    }
                }
            }
            else
            {
                textColorBrush = DarkColors.DisabledTextBrush;
                fillColor = DarkModeBackColor ?? DarkColors.DarkGreySelection;
            }

            if (fillColor != null)
            {
                using var b = new SolidBrush((Color)fillColor);
                g.FillRectangle(b, rect);
            }
            else
            {
                var fillBrush = _isDefault ? DarkColors.DarkBlueBackgroundBrush : DarkColors.LightBackgroundBrush;
                g.FillRectangle(fillBrush, rect);
            }

            if (ButtonStyle == DarkButtonStyle.Normal)
            {
                // Again, match us visually to size and position of classic mode
                var borderRect = new Rectangle(rect.Left, rect.Top, rect.Width - 1, rect.Height);

                g.DrawRectangle(borderPen, borderRect);
            }

            var textOffsetX = 0;
            var textOffsetY = 0;

            if (Image != null)
            {
                var stringSize = g.MeasureString(Text, Font, rect.Size);

                var x = (ClientSize.Width / 2) - (Image.Size.Width / 2);
                var y = (ClientSize.Height / 2) - (Image.Size.Height / 2);

                switch (TextImageRelation)
                {
                    case TextImageRelation.ImageAboveText:
                        textOffsetY = (Image.Size.Height / 2) + (ImagePadding / 2);
                        y -= (int)(stringSize.Height / 2) + (ImagePadding / 2);
                        break;
                    case TextImageRelation.TextAboveImage:
                        textOffsetY = ((Image.Size.Height / 2) + (ImagePadding / 2)) * -1;
                        y += (int)(stringSize.Height / 2) + (ImagePadding / 2);
                        break;
                    case TextImageRelation.ImageBeforeText:
                        textOffsetX = Image.Size.Width + (ImagePadding * 2);
                        x = ImagePadding;
                        break;
                    case TextImageRelation.TextBeforeImage:
                        x += (int)stringSize.Width;
                        break;
                }

                g.DrawImageUnscaled(Image, x, y);
            }

            var padding = Padding;
            /*
            TODO: Remove this hack entirely and just make all image buttons be manually painted
            We can just draw a bitmap instead of a vector and be perfectly fine then.
            Remember to cache all needed bitmaps so we don't pull from Resources every time.

            Create a greyed-out version of any bitmap:
            Bitmap c = new Bitmap("filename");
            Image d = ToolStripRenderer.CreateDisabledImage(c);
            */
            if (Image != null)
            {
                //padding.Left = -42;
                padding.Left -= Image.Width * 2;
                //padding.Right = -32;
            }

            var textRect = new Rectangle(rect.Left + textOffsetX + padding.Left,
                rect.Top + textOffsetY + padding.Top, rect.Width - padding.Horizontal,
                rect.Height - padding.Vertical);

            const TextFormatFlags textFormat =
                TextFormatFlags.HorizontalCenter |
                TextFormatFlags.VerticalCenter |
                TextFormatFlags.EndEllipsis |
                TextFormatFlags.NoPrefix |
                TextFormatFlags.NoClipping;

            // Use TextRenderer.DrawText() rather than g.DrawString() to match default text look exactly
            TextRenderer.DrawText(g, Text, Font, textRect, textColorBrush.Color, textFormat);

            #region Draw "transparent" (parent-control-backcolor-matching) border

            // This gets rid of the surrounding garbage from us modifying our draw position slightly to match
            // the visual size and positioning of the classic theme.
            // Draw this AFTER everything else, so that we draw on top so everything looks right.

            if (ButtonStyle == DarkButtonStyle.Normal)
            {
                Control parent = Parent;

                if (parent != null)
                {
                    using var pen = new Pen(parent.BackColor);
                    var bgRect = new Rectangle(0, 0, ClientSize.Width - 1, ClientSize.Height - 1);
                    g.DrawRectangle(pen, bgRect);
                }
            }

            #endregion

            PaintCustom?.Invoke(this, e);
        }

        #endregion
    }
}
