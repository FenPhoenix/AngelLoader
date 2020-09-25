using System;
using System.Windows.Forms;
using static AngelLoader.GameSupport;
using static AngelLoader.Misc;

namespace AngelLoader.Forms
{
    public partial class GameInfoForm : Form
    {
        private readonly (Label Label, TextBox TextBox)[] GameVersionItems;

        public GameInfoForm()
        {
            InitializeComponent();

            // @GENGAMES (GameInfoForm): Begin
            GameVersionItems = new[]
            {
                (T1VersionLabel, T1VersionTextBox),
                (T2VersionLabel, T2VersionTextBox),
                (T3VersionLabel, T3VersionTextBox),
                (SS2VersionLabel, SS2VersionTextBox)
            };
            // @GENGAMES (GameInfoForm): End

            Localize();
        }

        private void GameInfoForm_Load(object sender, EventArgs e)
        {
            for (int i = 0; i < SupportedGameCount; i++)
            {
                GameIndex gameIndex = (GameIndex)i;

                string ver = "";

                if (!Config.GetGameExe(gameIndex).IsEmpty())
                {
                    Error error = GameVersions.TryGetGameVersion((GameIndex)i, out ver);
                    if (error == Error.GameVersionNotFound)
                    {
                        ver = "not found";
                    }
                    else if (error != Error.None)
                    {
                        ver = "error getting version";
                    }

                    if (!GameIsDark(gameIndex))
                    {
                        ver = "Sneaky Upgrade " + ver;
                    }
                }
                else
                {
                    ver = "Game not set";
                }

                GameVersionItems[i].TextBox.Text = ver;
            }
        }

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
                    int amount = (maxLabelRightSidePos - textBox.Left);
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
    }
}
