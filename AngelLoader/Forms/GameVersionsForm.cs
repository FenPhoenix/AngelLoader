using System.Drawing;
using System.Windows.Forms;
using AL_Common;
using AngelLoader.Forms.CustomControls;
using static AngelLoader.GameSupport;
using static AngelLoader.Global;
using static AngelLoader.Misc;

namespace AngelLoader.Forms;

public sealed class GameVersionsForm : DarkFormBase
{
    private readonly DarkButton OKButton;

    private readonly (DarkLabel Label, DarkTextBox TextBox)[] GameVersionItems =
        new (DarkLabel Label, DarkTextBox TextBox)[SupportedGameCount];

    public GameVersionsForm()
    {
        #region Init

        OKButton = new DarkButton();
        var okFLP = new FlowLayoutPanel();
        okFLP.SuspendLayout();

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
        // 
        // okFLP
        // 
        okFLP.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        okFLP.Controls.Add(OKButton);
        okFLP.FlowDirection = FlowDirection.RightToLeft;
        okFLP.Location = new Point(0, 106);
        okFLP.Size = new Size(438, 40);
        okFLP.TabIndex = 0;

        // 
        // GameVersionsForm
        // 
        AutoScaleDimensions = new SizeF(6F, 13F);
        AutoScaleMode = AutoScaleMode.Font;
        CancelButton = OKButton;
        ClientSize = new Size(438, 146);
        Controls.Add(okFLP);
        FormBorderStyle = FormBorderStyle.FixedSingle;
        KeyPreview = true;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterParent;
        // Hack to prevent slow first render on some forms if Text is blank
        Text = " ";

        for (
            int i = 0, lblY = 11, tbY = 8;
            i < SupportedGameCount;
            i++, lblY += 24, tbY += 24)
        {
            var label = new DarkLabel();
            var textBox = new DarkTextBox();

            label.AutoSize = true;
            label.Location = new Point(11, lblY);

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

        okFLP.ResumeLayout(false);
        okFLP.PerformLayout();

        ResumeLayout(false);
        PerformLayout();

        #endregion

        for (int i = 0; i < SupportedGameCount; i++)
        {
            GameIndex gameIndex = (GameIndex)i;
            (DarkLabel label, DarkTextBox textBox) = GameVersionItems[i];

            if (!Config.GetGameExe(gameIndex).IsEmpty())
            {
                (Error error, string version) = Core.GetGameVersion((GameIndex)i);
                textBox.Text =
                    error == Error.GameExeNotFound ? LText.GameVersionsWindow.Error_GameExeNotFound :
                    error == Error.SneakyDllNotFound ? LText.GameVersionsWindow.Error_SneakyDllNotFound :
                    error == Error.GameVersionNotFound ? LText.GameVersionsWindow.Error_GameVersionNotFound :
                    gameIndex == GameIndex.Thief3 ? "Sneaky Upgrade " + version : version;
            }
            else
            {
                textBox.Enabled = false;
                label.Enabled = false;
            }
        }

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
            DarkLabel label = GameVersionItems[i].Label;
            int labelRightSidePos = label.Left + label.Width;
            if (labelRightSidePos > maxLabelRightSidePos) maxLabelRightSidePos = labelRightSidePos;
        }

        for (int i = 0; i < SupportedGameCount; i++)
        {
            DarkTextBox textBox = GameVersionItems[i].TextBox;
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

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.KeyCode == Keys.F1)
        {
            Core.OpenHelpFile(HelpSections.GameVersions);
        }
        base.OnKeyDown(e);
    }
}
