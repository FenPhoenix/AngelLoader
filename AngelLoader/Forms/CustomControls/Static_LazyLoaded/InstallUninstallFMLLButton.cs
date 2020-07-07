using System.Drawing;
using System.Windows.Forms;
using static AngelLoader.Misc;

namespace AngelLoader.Forms.CustomControls.Static_LazyLoaded
{
    internal static class InstallUninstallFMLLButton
    {
        internal static bool Constructed { get; private set; }

        private static bool _sayInstall;
        private static bool _enabled;
        
        private static Button? Button;

        internal static void SetSayInstall(bool value)
        {
            if (Constructed) SetSayInstallState(value);
            _sayInstall = value;
        }

        internal static void SetEnabled(bool value)
        {
            if (Constructed) Button!.Enabled = value;
            _enabled = value;
        }

        internal static void Construct(MainForm owner)
        {
            if (Constructed) return;
            
            var container = owner.BottomLeftButtonsFLP;

            Button = new Button { Visible = false };

            container.Controls.Add(Button);
            container.Controls.SetChildIndex(Button, 2);

            Button.AutoSize = true;
            Button.AutoSizeMode = AutoSizeMode.GrowOnly;
            Button.ImageAlign = ContentAlignment.MiddleLeft;
            Button.Padding = new Padding(6, 0, 6, 0);
            Button.Height = 36;
            Button.TabIndex = 58;
            Button.TextImageRelation = TextImageRelation.ImageBeforeText;
            Button.UseVisualStyleBackColor = true;
            Button.Click += owner.InstallUninstallFMButton_Click;

            Button.Enabled = _enabled;
            SetSayInstallState(_sayInstall);

            Constructed = true;
        }

        internal static void Localize(bool startup)
        {
            if (!Constructed) return;

            #region Install / Uninstall FM button

            // Special-case this button to always be the width of the longer of the two localized strings for
            // "Install" and "Uninstall" so it doesn't resize when its text changes. (visual nicety)
            Button!.SuspendDrawing();

            // Have to call this to get its layout working
            Button!.Show();

            string instString = LText.MainButtons.InstallFM;
            string uninstString = LText.MainButtons.UninstallFM;
            int instStringWidth = TextRenderer.MeasureText(instString, Button.Font).Width;
            int uninstStringWidth = TextRenderer.MeasureText(uninstString, Button.Font).Width;
            string longestString = instStringWidth > uninstStringWidth ? instString : uninstString;

            Button.SetTextAutoSize(longestString, preserveHeight: true);

            if (!startup) Button.Text = _sayInstall ? LText.MainButtons.InstallFM : LText.MainButtons.UninstallFM;

            if (Button.Visible && Config.HideUninstallButton) Button.Hide();

            Button.ResumeDrawing();

            #endregion
        }

        internal static void Show() => Button!.Show();

        internal static void Hide()
        {
            if (Constructed) Button!.Hide();
        }

        private static void SetSayInstallState(bool value)
        {
            Button!.Text = value ? LText.MainButtons.InstallFM : LText.MainButtons.UninstallFM;
            Button.Image = value ? Images.Install_24 : Images.Uninstall_24;
        }
    }
}
