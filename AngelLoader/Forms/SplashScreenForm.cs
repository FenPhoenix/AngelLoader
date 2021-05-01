using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.IO;
using System.Windows.Forms;
using AngelLoader.DataClasses;
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

        #region Private fields

        private VisualTheme _theme;
        private bool _themeSet;
        private bool _closingAllowed;

        private readonly Rectangle _messageRect = new Rectangle(1, 120, 646, 63);

        // Paranoid to make absolutely sure we're not accessing any cross-thread-disallowed Control properties in
        // SetMessage() (thread safety for FM finder)
        private Color _foreColorCached;
        private Color _backColorCached;

        private int _checkMessageWidth;

        #region Disposables

        private readonly Native.GraphicsContext _graphicsContext;

        private readonly Font _messageFont;
        private readonly Bitmap _logoBitmap = new Icon(AL_Icon.AngelLoader, 48, 48).ToBitmap();

        // Separate copy in here, so we don't cause an instantiation cascade for all the fields in DarkColors.
        // Special case to be AS FAST as possible.
        private readonly SolidBrush _fen_ControlBackgroundBrush = new SolidBrush(Color.FromArgb(48, 48, 48));

        #endregion

        #endregion

        // Perf
        internal bool VisibleCached;

        public SplashScreenForm()
        {
#if DEBUG
            InitializeComponent();
#else
            InitializeComponentSlim();
#endif

            // For some reason getting a built-in font is godawful slow (270+ ms), so we literally just fricking
            // bundle Open Sans and use that. It takes like 6ms. Sheesh.
            try
            {
                using var Collection = new PrivateFontCollection();
                Collection.AddFontFile(Path.Combine(Paths.Startup, "OpenSans-Regular.ttf"));
                _messageFont = new Font(Collection.Families[0], 12.0f, FontStyle.Regular);
            }
            catch
            {
                // Godawful slow as stated, but if we don't find our font, then we have to fall back to something.
                _messageFont = new Font(FontFamily.GenericSansSerif, 12.0f, FontStyle.Regular);
            }

            Text = "AngelLoader " + Application.ProductVersion;

            _graphicsContext = new Native.GraphicsContext(Handle);
        }

        public void Show(VisualTheme theme)
        {
            if (Visible || _themeSet) return;

            _theme = theme;

            // Ultra slim, because a splash screen should come up as quick as possible
            if (theme == VisualTheme.Dark)
            {
                // Again, manual copies of colors so we don't statically initialize the whole of DarkColors.
                BackColor = Color.FromArgb(48, 48, 48); // Fen_ControlBackground
                ForeColor = Color.FromArgb(220, 220, 220); // LightText
            }

            _foreColorCached = ForeColor;
            _backColorCached = BackColor;

            base.Show();

            // Must draw these after Show(), or they don't show up.
            // These will stay visible for the life of the form, due to our setup.
            _graphicsContext.G.DrawImage(_logoBitmap, 152, 48);
            _graphicsContext.G.DrawImage(theme == VisualTheme.Dark ? Resources.About_Dark : Resources.About, 200, 48);
            using var pen = new Pen(
                theme == VisualTheme.Dark
                    ? Color.FromArgb(81, 81, 81) // LightBorder
                    : SystemColors.ControlDark);
            _graphicsContext.G.DrawRectangle(pen, 0, 0, 647, 183);

            _themeSet = true;
        }

        public void SetMessage(string message)
        {
            Brush bgColorBrush = _theme == VisualTheme.Dark
                ? _fen_ControlBackgroundBrush
                : SystemBrushes.Control;

            _graphicsContext.G.FillRectangle(bgColorBrush, _messageRect);

            const TextFormatFlags _messageTextFormatFlags =
                TextFormatFlags.HorizontalCenter |
                TextFormatFlags.Top |
                TextFormatFlags.NoPrefix |
                TextFormatFlags.NoClipping |
                TextFormatFlags.WordBreak;

            TextRenderer.DrawText(_graphicsContext.G, message, _messageFont, _messageRect, _foreColorCached, _backColorCached, _messageTextFormatFlags);
        }

        public void SetCheckMessageWidth(string message)
        {
            const TextFormatFlags _messageTextFormatFlags =
                TextFormatFlags.HorizontalCenter |
                TextFormatFlags.Top |
                TextFormatFlags.NoPrefix |
                TextFormatFlags.NoClipping;

            _checkMessageWidth = TextRenderer.MeasureText(message, _messageFont, Size.Empty, _messageTextFormatFlags).Width;
        }

        public void SetCheckAtStoredMessageWidth()
        {
            if (_checkMessageWidth > Width) return;
            int checkPos = (Width / 2) + (_checkMessageWidth / 2);

            using var checkMarkPen = new Pen(_foreColorCached, 1.6f);

            SmoothingMode oldSmoothingMode = _graphicsContext.G.SmoothingMode;

            _graphicsContext.G.SmoothingMode = SmoothingMode.HighQuality;

            var outlineBoxRect = new Rectangle(checkPos, _messageRect.Y + 4, 12, 12);

            // First half of checkmark
            _graphicsContext.G.DrawLine(checkMarkPen,
                outlineBoxRect.Left + 1.5f,
                outlineBoxRect.Top + 6,
                outlineBoxRect.Left + 4.5f,
                outlineBoxRect.Top + 9);

            // Second half of checkmark
            _graphicsContext.G.DrawLine(checkMarkPen,
                outlineBoxRect.Left + 4.5f,
                outlineBoxRect.Top + 9,
                outlineBoxRect.Left + 10.5f,
                outlineBoxRect.Top + 3);

            _graphicsContext.G.SmoothingMode = oldSmoothingMode;
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            VisibleCached = Visible;
            base.OnVisibleChanged(e);
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
                _graphicsContext.Dispose();
                _fen_ControlBackgroundBrush.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
