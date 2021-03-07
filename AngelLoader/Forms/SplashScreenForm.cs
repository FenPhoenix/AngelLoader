using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using AngelLoader.DataClasses;
using AngelLoader.Forms.CustomControls;
using AngelLoader.Properties;

namespace AngelLoader.Forms
{
    public sealed partial class SplashScreenForm : Form
    {
        private readonly EventWaitHandle _disposeWaitHandle;
        private bool _themeSet;
        private bool _closingAllowed;

        public SplashScreenForm(EventWaitHandle waitHandle, EventWaitHandle disposeWaitHandle)
        {
#if DEBUG
            InitializeComponent();
#else
            InitializeComponentSlim();
#endif
            _disposeWaitHandle = disposeWaitHandle;

            // IMPORTANT: This icon MUST be set directly (not generated) or else we silently crash when we're standalone (not running in VS)!
            Icon = Images.AngelLoader;

            LogoPictureBox.Image = new Icon(Images.AngelLoader, 48, 48).ToBitmap();

            CreateHandle();
            waitHandle.Set();
        }

        public void Show(VisualTheme theme)
        {
            if (Visible && Opacity != 0d) return;

            if (!_themeSet)
            {
                // Ultra slim, because a splash screen should come up as quick as possible
                if (theme == VisualTheme.Dark)
                {
                    BackColor = DarkColors.Fen_ControlBackground;
                    SplashScreenMessageLabel.DarkModeEnabled = true;
                    LogoTextPictureBox.Image = Resources.About_DarkMode;
                }
                else
                {
                    LogoTextPictureBox.Image = Resources.About;
                }
                _themeSet = true;
            }

            if (Misc.WinVersionIs8OrAbove())
            {
                Opacity = 0d;
            }

            base.Show();

            if (Misc.WinVersionIs8OrAbove())
            {
                Opacity = 1.0d;
            }
        }

        public void SetMessageText(string text) => SplashScreenMessageLabel.Text = text;


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
            if (disposing) components?.Dispose();

            base.Dispose(disposing);

            _disposeWaitHandle.Set();
        }
    }
}
