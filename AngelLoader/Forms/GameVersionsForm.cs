using System.Drawing;
using System.Windows.Forms;
using AL_Common;
using AngelLoader.Forms.CustomControls;
using static AngelLoader.GameSupport;
using static AngelLoader.Misc;

namespace AngelLoader.Forms
{
    public sealed partial class GameVersionsForm : DarkFormBase
    {
        private DarkButton OKButton;
        private FlowLayoutPanel OKFlowLayoutPanel;

        private readonly
            (DarkLabel Label,
            DarkTextBox TextBox)[]
            GameVersionItems = new
                (DarkLabel Label,
                DarkTextBox TextBox)[SupportedGameCount];

        public GameVersionsForm()
        {
            #region Init

            OKButton = new DarkButton();
            OKFlowLayoutPanel = new FlowLayoutPanel();
            OKFlowLayoutPanel.SuspendLayout();

            SuspendLayout();

            // 
            // OKButton
            // 
            OKButton.AutoSize = true;
            OKButton.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            OKButton.DialogResult = DialogResult.Cancel;
            OKButton.Margin = new Padding(3, 8, 9, 3);
            OKButton.MinimumSize = new Size(75, 23);
            OKButton.TabIndex = 0;
            OKButton.UseVisualStyleBackColor = true;
            // 
            // OKFlowLayoutPanel
            // 
            OKFlowLayoutPanel.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            OKFlowLayoutPanel.Controls.Add(OKButton);
            OKFlowLayoutPanel.FlowDirection = FlowDirection.RightToLeft;
            OKFlowLayoutPanel.Location = new Point(0, 106);
            OKFlowLayoutPanel.Size = new Size(438, 40);
            OKFlowLayoutPanel.TabIndex = 0;

            // 
            // GameVersionsForm
            // 
            AutoScaleDimensions = new SizeF(6F, 13F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = OKButton;
            ClientSize = new Size(438, 146);
            Controls.Add(OKFlowLayoutPanel);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Icon = AL_Icon.AngelLoader;
            KeyPreview = true;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            // Hack to prevent slow first render on some forms if Text is blank
            Text = " ";
            KeyDown += GameVersionsForm_KeyDown;

            for (
                int i = 0, lblY = 11, tbY = 8;
                i < SupportedGameCount;
                i++, lblY += 24, tbY += 24)
            {
                var label = new DarkLabel();
                var textBox = new DarkTextBox();

                label.AutoSize = true;
                label.Location = new Point(11, lblY);
                label.TabIndex = i + 1;

                textBox.Anchor = AnchorStyles.Top | AnchorStyles.Right;
                textBox.Location = new Point(205, tbY);
                textBox.MaximumSize = new Size(224, 32767);
                textBox.MinimumSize = new Size(80, 4);
                textBox.ReadOnly = true;
                textBox.Size = new Size(224, 20);
                textBox.TabIndex = label.TabIndex + 1;

                GameVersionItems[i].Label = label;
                GameVersionItems[i].TextBox = textBox;

                Controls.Add(label);
                Controls.Add(textBox);
            }

            OKFlowLayoutPanel.ResumeLayout(false);
            OKFlowLayoutPanel.PerformLayout();

            ResumeLayout(false);
            PerformLayout();

            #endregion

            // @GENGAMES (GameVersionsForm): Begin

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

            if (Config.DarkMode) SetThemeBase(Config.VisualTheme);

            Localize();
        }

        private void Localize()
        {
            Text = LText.GameVersionsWindow.TitleText;

            for (int i = 0; i < SupportedGameCount; i++)
            {
                GameIndex gameIndex = (GameIndex)i;
                GameVersionItems[i].Label.Text = GetLocalizedGameNameColon(gameIndex);
            }

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
