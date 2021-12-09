using System.Drawing;
using System.Windows.Forms;
using JetBrains.Annotations;
using static AngelLoader.Misc;

namespace AngelLoader.Forms.CustomControls.Static_LazyLoaded
{
    internal static class InstallUninstallFMLLButton
    {
        internal static bool Constructed { get; private set; }

        private static bool _enabled;

        internal static bool SayInstallState { get; private set; }

        internal static DarkButton Button = null!;

        private static bool _darkModeEnabled;
        [PublicAPI]
        public static bool DarkModeEnabled
        {
            get => _darkModeEnabled;
            set
            {
                if (_darkModeEnabled == value) return;
                _darkModeEnabled = value;
                if (!Constructed) return;

                Button.DarkModeEnabled = value;
            }
        }

        internal static void Construct(MainForm owner)
        {
            if (Constructed) return;

            var container = owner.BottomLeftButtonsFLP;

            // This Visible = false must be being ignored?
            // Otherwise, it's impossible that this would work, because we construct but only explicitly call
            // Show() in OpenSettings()...
            // 2020-12-30: No, it's because we call Localize() and that calls Show().
            Button = new DarkButton { Visible = false, Tag = LoadType.Lazy };

            container.Controls.Add(Button);
            container.Controls.SetChildIndex(Button, 2);

            Button.AutoSize = true;
            Button.AutoSizeMode = AutoSizeMode.GrowOnly;
            Button.DarkModeEnabled = _darkModeEnabled;
            Button.Margin = new Padding(6, 3, 0, 3);
            Button.Padding = new Padding(30, 0, 6, 0);
            Button.MinimumSize = new Size(0, 36);
            Button.TabIndex = 58;
            Button.UseVisualStyleBackColor = true;
            Button.Click += owner.InstallUninstall_Play_Buttons_Click;
            Button.PaintCustom += owner.InstallUninstall_Play_Buttons_Paint;

            Button.Enabled = _enabled;
            SetSayInstallState(SayInstallState);

            Constructed = true;
        }

        internal static void Localize(bool startup)
        {
            if (!Constructed) return;

            #region Install / Uninstall FM button

            // Special-case this button to always be the width of the longer of the two localized strings for
            // "Install" and "Uninstall" so it doesn't resize when its text changes. (visual nicety)
            Button.SuspendDrawing();

            // Have to call this to get its layout working
            Button.Show();

            string instString = LText.MainButtons.InstallFM;
            string uninstString = LText.MainButtons.UninstallFM;
            int instStringWidth = TextRenderer.MeasureText(instString, Button.Font).Width;
            int uninstStringWidth = TextRenderer.MeasureText(uninstString, Button.Font).Width;
            string longestString = instStringWidth > uninstStringWidth ? instString : uninstString;

            // Special case autosize text-set: can't be GrowAndShrink
            Button.SetTextAutoSize(longestString);

            if (!startup) Button.Text = SayInstallState ? LText.MainButtons.InstallFM : LText.MainButtons.UninstallFM;

            if (Button.Visible && Config.HideUninstallButton) Button.Hide();

            Button.ResumeDrawing();

            #endregion
        }

        internal static void SetSayInstall(bool value)
        {
            if (Constructed) SetSayInstallState(value);
            SayInstallState = value;
        }

        internal static void SetEnabled(bool value)
        {
            if (Constructed) Button.Enabled = value;
            _enabled = value;
        }

        internal static void Show() => Button.Show();

        internal static void Hide()
        {
            if (Constructed) Button.Hide();
        }

        private static void SetSayInstallState(bool value)
        {
            // Special-cased; don't autosize this one
            Button.Text = value ? LText.MainButtons.InstallFM : LText.MainButtons.UninstallFM;
            Button.Invalidate();
        }
    }
}
