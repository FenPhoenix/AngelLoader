using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using AL_Common;
using AngelLoader.DataClasses;
using AngelLoader.Forms.CustomControls;
using static AngelLoader.GameSupport;
using static AngelLoader.Misc;

namespace AngelLoader.Forms
{
    public sealed partial class GameVersionsForm : DarkFormBase
    {
        private readonly (DarkLabel Label, DarkTextBox TextBox)[] GameVersionItems;

        private readonly List<KeyValuePair<Control, (Color ForeColor, Color BackColor)>> _controlColors = new();

        public GameVersionsForm()
        {
#if DEBUG
            InitializeComponent();
#else
            InitializeComponentSlim();
#endif

            // @GENGAMES (GameVersionsForm): Begin
            GameVersionItems = new[]
            {
                (T1VersionLabel, T1VersionTextBox),
                (T2VersionLabel, T2VersionTextBox),
                (T3VersionLabel, T3VersionTextBox),
                (SS2VersionLabel, SS2VersionTextBox)
            };

            for (int i = 0; i < SupportedGameCount; i++)
            {
                GameIndex gameIndex = (GameIndex)i;
                var (label, textBox) = GameVersionItems[i];

                if (!Config.GetGameExe(gameIndex).IsEmpty())
                {
                    (Error error, string version) = Core.GetGameVersion((GameIndex)i);
                    textBox.Text =
                        error == Error.GameExeNotFound ? LText.GameVersionsWindow.Error_GameExeNotFound :
                        error == Error.SneakyDllNotFound ? LText.GameVersionsWindow.Error_SneakyDllNotFound :
                        error == Error.GameVersionNotFound ? LText.GameVersionsWindow.Error_GameVersionNotFound :
                        GameIsDark(gameIndex) ? version : "Sneaky Upgrade " + version;
                }
                else
                {
                    textBox.Enabled = false;
                    label.Enabled = false;
                }
            }
            // @GENGAMES (GameVersionsForm): End

            if (Config.DarkMode) SetTheme(Config.VisualTheme);

            Localize();
        }

        private void SetTheme(VisualTheme theme) => ControlUtils.ChangeFormThemeMode(theme, this, _controlColors);

        private void Localize()
        {
            Text = LText.GameVersionsWindow.TitleText;

            T1VersionLabel.Text = LText.Global.Thief1_Colon;
            T2VersionLabel.Text = LText.Global.Thief2_Colon;
            T3VersionLabel.Text = LText.Global.Thief3_Colon;
            SS2VersionLabel.Text = LText.Global.SystemShock2_Colon;

            OKButton.Text = LText.Global.OK;

            #region Manual layout

            int maxLabelRightSidePos = 0;
            for (int i = 0; i < SupportedGameCount; i++)
            {
                var label = GameVersionItems[i].Label;
                int labelRightSidePos = label.Left + label.Width;
                if (labelRightSidePos > maxLabelRightSidePos) maxLabelRightSidePos = labelRightSidePos;
            }

            for (int i = 0; i < SupportedGameCount; i++)
            {
                var textBox = GameVersionItems[i].TextBox;
                if (maxLabelRightSidePos > textBox.Left)
                {
                    int amount = maxLabelRightSidePos - textBox.Left;
                    textBox.Width -= amount;
                    textBox.Left += amount;
                    if (textBox.Right > ClientSize.Width)
                    {
                        textBox.Left = (ClientSize.Width - textBox.Width) - 9;
                    }
                }
            }

            #endregion
        }

        private void GameVersionsForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F1)
            {
                Core.OpenHelpFile(HelpSections.GameVersions);
            }
        }
    }
}
