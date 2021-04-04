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
        /*
        Here's the deal with this:
        We want this form to be fully and properly painted at all times. Normally, we would achieve this by simply
        running all init tasks in the background so that this form's thread is always free. However, part of our
        init time includes initializing the main form (which is heavy, and even more so if we're setting dark mode).
        Since all UI stuff has to run on the same thread, this form would end up in a blocked, and quite likely
        partially-painted, state. We can put this form into another thread via a separate ApplicationContext,
        and that works, except then we have insurmountable focus issues (the main form _usually_, but not always,
        takes focus as it should).
        The other option we have is to bypass the normal way to display a form entirely, and just paint everything
        directly onto its device context. That way, we paint it _once_ and never change it until we repaint the
        message text, and it never gets into an ugly half-painted state, even when its thread is blocked, as it
        will be during main form init. But it does mean we have some very unorthodox code in here. Don't worry
        about it.
        */

        // TODO: @DarkMode(SplashScreen): Maybe draw a border and make the classic-mode one Window (white) instead of Control

        #region Private fields

        private VisualTheme _theme;
        private bool _themeSet;
        private bool _closingAllowed;

        private readonly Rectangle _messageRect = new Rectangle(0, 120, 648, 64);

        #region Disposables

        private readonly Native.DeviceContext _deviceContext;
        private readonly Graphics _graphics;

        private readonly Font _messageFont = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
        private readonly Bitmap _logoBitmap = new Icon(Images.AngelLoader, 48, 48).ToBitmap();

        #endregion

        #endregion

        public SplashScreenForm()
        {
#if DEBUG
            InitializeComponent();
#else
            InitializeComponentSlim();
#endif

            Text = "AngelLoader " + Application.ProductVersion;

            _deviceContext = new Native.DeviceContext(Handle);
            _graphics = Graphics.FromHdc(_deviceContext.DC);
        }

        public void Show(VisualTheme theme)
        {
            if (Visible) return;

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

            base.Show();

            // Must draw these after Show(), or they don't show up.
            // These will stay visible for the life of the form, due to our setup.
            _graphics.DrawImage(_logoBitmap, 152, 48);
            _graphics.DrawImage(theme == VisualTheme.Dark ? Resources.About_Dark : Resources.About, 200, 48);
        }

        public void SetMessage(string message)
        {
            Brush bgColorBrush = _theme == VisualTheme.Dark
                ? DarkColors.Fen_ControlBackgroundBrush
                : SystemBrushes.Control;

            _graphics.FillRectangle(bgColorBrush, _messageRect);

            const TextFormatFlags _messageTextFormatFlags =
                TextFormatFlags.HorizontalCenter |
                TextFormatFlags.Top |
                TextFormatFlags.NoPrefix |
                TextFormatFlags.NoClipping |
                TextFormatFlags.WordBreak;

            TextRenderer.DrawText(_graphics, message, _messageFont, _messageRect, ForeColor, BackColor, _messageTextFormatFlags);
        }

        public void ProgrammaticClose()
        {
            _closingAllowed = true;
            Close();
        }

        // Don't let the user close the splash screen; that would put us in an unexpected/undefined state.
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
