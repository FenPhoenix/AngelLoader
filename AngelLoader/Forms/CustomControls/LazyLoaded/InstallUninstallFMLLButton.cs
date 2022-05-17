using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using JetBrains.Annotations;
using static AngelLoader.Misc;

namespace AngelLoader.Forms.CustomControls.LazyLoaded
{
    internal sealed class InstallUninstallFMLLButton
    {
        private readonly MainForm _owner;

        internal bool Constructed { get; private set; }

        private bool _enabled;

        internal bool SayInstallState { get; private set; }

        internal DarkButton Button = null!;

        private bool _darkModeEnabled;
        [PublicAPI]
        public bool DarkModeEnabled
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

        internal InstallUninstallFMLLButton(MainForm owner) => _owner = owner;

        internal void Construct()
        {
            if (Constructed) return;

            var container = _owner.BottomLeftButtonsFLP;

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
            Button.Click += _owner.InstallUninstall_Play_Buttons_Click;
            Button.PaintCustom += _owner.InstallUninstall_Play_Buttons_Paint;

            Button.Enabled = _enabled;
            SetSayInstallState(SayInstallState);

            Constructed = true;
        }

        internal void Localize(bool startup)
        {
            if (!Constructed) return;

            #region Install / Uninstall FM button

            // Special-case this button to always be the width of the longer of the two localized strings for
            // "Install" and "Uninstall" so it doesn't resize when its text changes. (visual nicety)
            Button.SuspendDrawing();

            // Have to call this to get its layout working
            Button.Show();

            (string Text, int Length)[] stringsAndLengths =
            {
                (LText.InstallAndPlayFMGlobal.UninstallFMs, TextRenderer.MeasureText(LText.InstallAndPlayFMGlobal.UninstallFMs, Button.Font).Width),
                (LText.InstallAndPlayFMGlobal.UninstallFM, TextRenderer.MeasureText(LText.InstallAndPlayFMGlobal.UninstallFM, Button.Font).Width),
                (LText.InstallAndPlayFMGlobal.InstallFMs, TextRenderer.MeasureText(LText.InstallAndPlayFMGlobal.InstallFMs, Button.Font).Width),
                (LText.InstallAndPlayFMGlobal.InstallFM, TextRenderer.MeasureText(LText.InstallAndPlayFMGlobal.InstallFM, Button.Font).Width)
            };

            stringsAndLengths = stringsAndLengths.OrderByDescending(x => x.Length).ToArray();

            string longestString = stringsAndLengths[0].Text;

            // Special case autosize text-set: can't be GrowAndShrink
            Button.SetTextAutoSize(longestString);

            if (!startup) SetButtonText(SayInstallState);

            if (Button.Visible && Config.HideUninstallButton) Button.Hide();

            Button.ResumeDrawing();

            #endregion
        }

        internal void SetSayInstall(bool value)
        {
            if (Constructed) SetSayInstallState(value);
            SayInstallState = value;
        }

        internal void SetEnabled(bool value)
        {
            if (Constructed) Button.Enabled = value;
            _enabled = value;
        }

        internal void Show() => Button.Show();

        internal void Hide()
        {
            if (Constructed) Button.Hide();
        }

        private void SetButtonText(bool installState)
        {
            bool multiSelected = _owner.FMsDGV.MultipleFMsSelected();

            // Special-cased; don't autosize this one
            Button.Text =
                installState
                    ? multiSelected
                        ? LText.InstallAndPlayFMGlobal.InstallFMs
                        : LText.InstallAndPlayFMGlobal.InstallFM
                    : multiSelected
                        ? LText.InstallAndPlayFMGlobal.UninstallFMs
                        : LText.InstallAndPlayFMGlobal.UninstallFM;
        }

        private void SetSayInstallState(bool value)
        {
            SetButtonText(value);
            Button.Invalidate();
        }
    }
}
