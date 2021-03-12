using System.Drawing;
using System.Windows.Forms;
using AngelLoader.DataClasses;
using AngelLoader.Forms.CustomControls;
using AngelLoader.Properties;
using AngelLoader.WinAPI;

namespace AngelLoader.Forms
{
    public sealed partial class SplashScreenForm : Form
    {
        private readonly Native.DeviceContext _deviceContext;
        private readonly Graphics _graphics;
        private bool _themeSet;
        private bool _closingAllowed;
        private readonly Font _messageFont = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
        private readonly Rectangle _messageRect = new Rectangle(0, 120, 648, 64);
        private const TextFormatFlags _messageTextFormatFlags =
            TextFormatFlags.HorizontalCenter |
            TextFormatFlags.Top |
            TextFormatFlags.NoPrefix |
            TextFormatFlags.NoClipping |
            TextFormatFlags.WordBreak;
        private VisualTheme _theme;
        private readonly Bitmap _logoBitmap = new Icon(Images.AngelLoader, 48, 48).ToBitmap();

        public SplashScreenForm()
        {
            InitializeComponent();

            Text = "AngelLoader " + Application.ProductVersion;

            CreateHandle();
            _deviceContext = new Native.DeviceContext(Handle);
            _graphics = Graphics.FromHdc(_deviceContext.DC);
        }

        public void Show(VisualTheme theme)
        {
            if (Visible && Opacity != 0d) return;

            if (!_themeSet)
            {
                _theme = theme;

                // Ultra slim, because a splash screen should come up as quick as possible
                if (theme == VisualTheme.Dark)
                {
                    BackColor = DarkColors.Fen_ControlBackground;
                    ForeColor = DarkColors.LightText;
                }

                _themeSet = true;
            }

            if (Misc.WinVersionIs8OrAbove()) Opacity = 0d;

            base.Show();

            // Must draw these after Show(), or they don't show up
            _graphics.DrawImage(_logoBitmap, 152, 48);
            _graphics.DrawImage(theme == VisualTheme.Dark ? Resources.About_DarkMode : Resources.About, 200, 48);

            if (Misc.WinVersionIs8OrAbove()) Opacity = 1.0d;
        }

        public void SetMessage(string message)
        {
            var bgColorBrush = _theme == VisualTheme.Dark
                ? DarkColors.Fen_ControlBackgroundBrush
                : SystemBrushes.Control;

            _graphics.FillRectangle(bgColorBrush, _messageRect);
            TextRenderer.DrawText(_graphics, message, _messageFont, _messageRect, ForeColor, BackColor, _messageTextFormatFlags);
        }

        public void ProgrammaticClose()
        {
            _closingAllowed = true;
            Close();
            Dispose();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (!_closingAllowed)
            {
                e.Cancel = true;
            }
            else
            {
                base.OnFormClosing(e);
            }
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                components?.Dispose();
                _messageFont.Dispose();
                _logoBitmap.Dispose();
                _graphics.Dispose();
                _deviceContext.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
