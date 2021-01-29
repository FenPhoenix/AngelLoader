using DarkUI.Config;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace DarkUI.Controls
{
    [ToolboxBitmap(typeof(Button))]
    [DefaultEvent("Click")]
    public class DarkButton : Button
    {
        #region Field Region

        private DarkButtonStyle _style = DarkButtonStyle.Normal;
        private DarkControlState _buttonState = DarkControlState.Normal;

        private bool _isDefault;
        private bool _spacePressed;

        private int _padding = Consts.Padding / 2;
        private int _imagePadding = 5; // Consts.Padding / 2

        private FlatStyle? _originalFlatStyle;
        private int? _originalBorderSize;

        private Size? _originalSize;

        #endregion

        #region Designer Property Region

        public new string Text
        {
            get { return base.Text; }
            set
            {
                base.Text = value;
                InvalidateIfDark();
            }
        }

        public new bool Enabled
        {
            get { return base.Enabled; }
            set
            {
                base.Enabled = value;
                InvalidateIfDark();
            }
        }

        [Category("Appearance")]
        [Description("Determines the style of the button.")]
        [DefaultValue(DarkButtonStyle.Normal)]
        public DarkButtonStyle ButtonStyle
        {
            get { return _style; }
            set
            {
                _style = value;
                InvalidateIfDark();
            }
        }

        [Category("Appearance")]
        [Description("Determines the amount of padding between the image and text.")]
        [DefaultValue(5)]
        public int ImagePadding
        {
            get { return _imagePadding; }
            set
            {
                _imagePadding = value;
                InvalidateIfDark();
            }
        }

        [Category("Appearance")]
        [Description("Performs a small size and position adjustment in dark mode to attempt to keep precise UI layouts the same as in classic mode.")]
        [DefaultValue(false)]
        public bool AdjustSizeAndPosForDarkMode { get; set; }

        #endregion

        #region Code Property Region

        //[Browsable(false)]
        //[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        //public new bool AutoEllipsis
        //{
        //    get { return false; }
        //}

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public DarkControlState ButtonState
        {
            get { return _buttonState; }
        }

        //[Browsable(false)]
        //[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        //public new ContentAlignment ImageAlign
        //{
        //    get { return base.ImageAlign; }
        //}

        //[Browsable(false)]
        //[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        //public new bool FlatAppearance
        //{
        //    get { return false; }
        //}

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
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

        //[Browsable(false)]
        //[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        //public new ContentAlignment TextAlign
        //{
        //    get { return base.TextAlign; }
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

        private bool _darkModeEnabled;
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

            base.UseCompatibleTextRendering = false;

            if (_darkModeEnabled)
            {
                // BUG: @DarkMode: DarkButton - position/size bug
                // AddTagButton is inching along to the left with every toggle due to it being anchored right
                // (doesn't happen when anchored left). We need to account for all possible anchorings here.
                // Also:
                // If a number is 0 and we subtract and then add again, we might end up with different final
                // values(?) We should just store all the values as previous and restore them back exactly.

                if (AdjustSizeAndPosForDarkMode)
                {
                    if (_originalSize == null) _originalSize = Size;
                    MinimumSize = Size.Subtract(MinimumSize, new Size(2, 2));
                    Size = Size.Subtract((Size)_originalSize, new Size(2, 2));
                    Location = new Point(Location.X + 1, Location.Y + 1);
                }

                _originalFlatStyle = base.FlatStyle;
                _originalBorderSize = FlatAppearance.BorderSize;
                base.UseVisualStyleBackColor = !_darkModeEnabled;
                SetButtonState(DarkControlState.Normal);

                Invalidate();
            }
            else
            {
                // Need to set these explicitly because in some cases (not all) they don't get set back automatically
                ForeColor = SystemColors.ControlText;
                BackColor = SystemColors.Control;
                base.UseVisualStyleBackColor = true;
                base.FlatStyle = _originalFlatStyle ?? base.FlatStyle;
                FlatAppearance.BorderSize = _originalBorderSize ?? FlatAppearance.BorderSize;

                if (AdjustSizeAndPosForDarkMode && _originalSize != null)
                {
                    MinimumSize = Size.Add(MinimumSize, new Size(2, 2));
                    Size = (Size)_originalSize;
                    Location = new Point(Location.X - 1, Location.Y - 1);
                }
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
            if (_buttonState != buttonState)
            {
                _buttonState = buttonState;
                InvalidateIfDark();
            }
        }

        #endregion

        #region Event Handler Region

        protected override void OnCreateControl()
        {
            base.OnCreateControl();

            if (!_darkModeEnabled) return;

            var form = FindForm();
            if (form != null)
            {
                if (form.AcceptButton == this)
                    _isDefault = true;
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (!_darkModeEnabled) return;

            if (_spacePressed)
                return;

            if (e.Button == MouseButtons.Left)
            {
                if (ClientRectangle.Contains(e.Location))
                    SetButtonState(DarkControlState.Pressed);
                else
                    SetButtonState(DarkControlState.Hover);
            }
            else
            {
                SetButtonState(DarkControlState.Hover);
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (!_darkModeEnabled) return;

            if (e.Button != MouseButtons.Left) return;

            if (!ClientRectangle.Contains(e.Location))
                return;

            SetButtonState(DarkControlState.Pressed);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            if (!_darkModeEnabled) return;

            if (e.Button != MouseButtons.Left) return;

            if (_spacePressed)
                return;

            SetButtonState(DarkControlState.Normal);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);

            if (!_darkModeEnabled) return;

            if (_spacePressed)
                return;

            SetButtonState(DarkControlState.Normal);
        }

        protected override void OnMouseCaptureChanged(EventArgs e)
        {
            base.OnMouseCaptureChanged(e);

            if (!_darkModeEnabled) return;

            if (_spacePressed)
                return;

            var location = Cursor.Position;

            if (!ClientRectangle.Contains(location))
                SetButtonState(DarkControlState.Normal);
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

            var location = Cursor.Position;

            if (!ClientRectangle.Contains(location))
                SetButtonState(DarkControlState.Normal);
            else
                SetButtonState(DarkControlState.Hover);
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

                var location = Cursor.Position;

                if (!ClientRectangle.Contains(location))
                    SetButtonState(DarkControlState.Normal);
                else
                    SetButtonState(DarkControlState.Hover);
            }
        }

        public override void NotifyDefault(bool value)
        {
            base.NotifyDefault(value);

            if (!_darkModeEnabled) return;

            if (!DesignMode)
                return;

            _isDefault = value;
            InvalidateIfDark();
        }

        #endregion

        #region Paint Region

        // Need our own event because we can't fire base.OnPaint() or it overrides our own painting
        public PaintEventHandler PaintCustom;

        protected override void OnPaint(PaintEventArgs e)
        {
            if (!_darkModeEnabled)
            {
                base.OnPaint(e);
                PaintCustom?.Invoke(this, e);
                return;
            }

            var g = e.Graphics;
            var rect = new Rectangle(0, 0, ClientSize.Width, ClientSize.Height);

            var textColor = Colors.LightText;
            var borderColor = Colors.GreySelection;
            var fillColor = _isDefault ? Colors.DarkBlueBackground : Colors.LightBackground;

            if (Enabled)
            {
                if (ButtonStyle == DarkButtonStyle.Normal)
                {
                    if (Focused && TabStop)
                        borderColor = Colors.BlueHighlight;

                    switch (ButtonState)
                    {
                        case DarkControlState.Hover:
                            fillColor = _isDefault ? Colors.BlueBackground : Colors.LighterBackground;
                            break;
                        case DarkControlState.Pressed:
                            fillColor = _isDefault ? Colors.DarkBackground : Colors.DarkBackground;
                            break;
                    }
                }
                else if (ButtonStyle == DarkButtonStyle.Flat)
                {
                    switch (ButtonState)
                    {
                        case DarkControlState.Normal:
                            fillColor = Colors.GreyBackground;
                            break;
                        case DarkControlState.Hover:
                            fillColor = Colors.MediumBackground;
                            break;
                        case DarkControlState.Pressed:
                            fillColor = Colors.DarkBackground;
                            break;
                    }
                }
            }
            else
            {
                textColor = Colors.DisabledText;
                fillColor = Colors.DarkGreySelection;
            }

            using (var b = new SolidBrush(fillColor))
            {
                g.FillRectangle(b, rect);
            }

            if (ButtonStyle == DarkButtonStyle.Normal)
            {
                using (var p = new Pen(borderColor, 1))
                {
                    var modRect = new Rectangle(rect.Left, rect.Top, rect.Width - 1, rect.Height - 1);

                    g.DrawRectangle(p, modRect);
                }
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
                        y = y - ((int)(stringSize.Height / 2) + (ImagePadding / 2));
                        break;
                    case TextImageRelation.TextAboveImage:
                        textOffsetY = ((Image.Size.Height / 2) + (ImagePadding / 2)) * -1;
                        y = y + ((int)(stringSize.Height / 2) + (ImagePadding / 2));
                        break;
                    case TextImageRelation.ImageBeforeText:
                        textOffsetX = Image.Size.Width + (ImagePadding * 2);
                        x = ImagePadding;
                        break;
                    case TextImageRelation.TextBeforeImage:
                        x = x + (int)stringSize.Width;
                        break;
                }

                g.DrawImageUnscaled(Image, x, y);
            }

            using (var b = new SolidBrush(textColor))
            {
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

                var modRect = new Rectangle(rect.Left + textOffsetX + padding.Left,
                                            rect.Top + textOffsetY + padding.Top, rect.Width - padding.Horizontal,
                                            rect.Height - padding.Vertical);

                const TextFormatFlags textFormat =
                    TextFormatFlags.HorizontalCenter |
                    TextFormatFlags.VerticalCenter |
                    TextFormatFlags.EndEllipsis |
                    TextFormatFlags.NoPrefix;

                // Use TextRenderer.DrawText() rather than g.DrawString() to match default text look exactly
                TextRenderer.DrawText(g, Text, Font, modRect, b.Color, textFormat);
            }

            PaintCustom?.Invoke(this, e);
        }

        #endregion
    }
}
