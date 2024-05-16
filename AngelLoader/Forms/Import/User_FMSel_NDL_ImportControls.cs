using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using AL_Common;
using AngelLoader.Forms.CustomControls;
using static AngelLoader.GameSupport;
using static AngelLoader.Global;
using static AngelLoader.Misc;
using static AngelLoader.Utils;

namespace AngelLoader.Forms;

public sealed class User_FMSel_NDL_ImportControls : UserControl
{
    private ImportType _importType;

    private readonly
        (DarkGroupBox GroupBox,
        DarkCheckBox AutodetectCheckBox,
        DarkTextBox TextBox,
        StandardButton BrowseButton)[]
        GameIniItems = new
            (DarkGroupBox GroupBox,
            DarkCheckBox AutodetectCheckBox,
            DarkTextBox TextBox,
            StandardButton BrowseButton)[ImportSupportingGameCount];

    private readonly DarkLabel ChooseIniFilesLabel;

    internal string GetIniFile(int index) => GameIniItems[index].TextBox.Text;

    public User_FMSel_NDL_ImportControls()
    {
        SuspendLayout();

        ChooseIniFilesLabel = new DarkLabel
        {
            AutoSize = true,
            Location = new Point(16, 8),
        };

        AutoScaleDimensions = new SizeF(6F, 13F);
        AutoScaleMode = AutoScaleMode.Font;
        Size = new Size(551, 410);
        Controls.Add(ChooseIniFilesLabel);

        for (int i = 0, y = 32; i < ImportSupportingGameCount; i++, y += 88)
        {
            var checkBox = new DarkCheckBox();
            var textBox = new DarkTextBox();
            var button = new StandardButton();
            var groupBox = new DarkGroupBox();

            groupBox.SuspendLayout();

            checkBox.AutoSize = true;
            checkBox.Checked = true;
            checkBox.Location = new Point(16, 24);
            checkBox.TabIndex = 0;
            checkBox.CheckedChanged += AutodetectCheckBoxes_CheckedChanged;

            textBox.Location = new Point(16, 48);
            textBox.ReadOnly = true;
            textBox.Size = new Size(432, 20);
            textBox.TabIndex = 1;

            button.Enabled = false;
            button.Location = new Point(448, 47);
            button.TabIndex = 1;
            button.Click += ThiefIniBrowseButtons_Click;

            groupBox.Controls.Add(checkBox);
            groupBox.Controls.Add(textBox);
            groupBox.Controls.Add(button);

            groupBox.Location = new Point(8, y);
            groupBox.Size = new Size(536, 80);
            groupBox.TabIndex = i + 1;

            GameIniItems[i].GroupBox = groupBox;
            GameIniItems[i].AutodetectCheckBox = checkBox;
            GameIniItems[i].TextBox = textBox;
            GameIniItems[i].BrowseButton = button;

            Controls.Add(groupBox);

            groupBox.ResumeLayout(false);
        }

        ResumeLayout(false);
        PerformLayout();
    }

    internal void Init(ImportType importType)
    {
        _importType = importType;

        Localize();

        for (int i = 0, importI = 0; i < SupportedGameCount; i++)
        {
            GameIndex gameIndex = (GameIndex)i;
            if (GameSupportsImport(gameIndex))
            {
                AutodetectGameIni(gameIndex, GameIniItems[importI].TextBox);
                importI++;
            }
        }
    }

    private void Localize()
    {
        ChooseIniFilesLabel.Text = _importType == ImportType.NewDarkLoader
            ? LText.Importing.ChooseNewDarkLoaderIniFiles
            : LText.Importing.ChooseFMSelIniFiles;

        for (int i = 0, importI = 0; i < SupportedGameCount; i++)
        {
            GameIndex gameIndex = (GameIndex)i;
            if (GameSupportsImport(gameIndex))
            {
                GameIniItems[importI].GroupBox.Text = GetLocalizedGameName(gameIndex);
                GameIniItems[importI].AutodetectCheckBox.Text = LText.Global.Autodetect;
                GameIniItems[importI].BrowseButton.SetTextForTextBoxButtonCombo(GameIniItems[importI].TextBox, LText.Global.BrowseEllipses);
                importI++;
            }
        }
    }

    private void AutodetectGameIni(GameIndex game, TextBox textBox)
    {
        string iniFile = _importType == ImportType.NewDarkLoader ? Paths.NewDarkLoaderIni : Paths.FMSelIni;

        string fmsPath = Config.GetFMInstallPath(game);
        textBox.Text = !fmsPath.IsWhiteSpace() && TryCombineFilePathAndCheckExistence(fmsPath, iniFile, out string iniFileFull)
            ? iniFileFull
            : "";
    }

    private void ThiefIniBrowseButtons_Click(object? sender, EventArgs e)
    {
        using var d = new OpenFileDialog();
        d.Filter = LText.BrowseDialogs.IniFiles + "|*.ini|" + LText.BrowseDialogs.AllFiles + "|*.*";
        if (d.ShowDialogDark(FindForm()) != DialogResult.OK) return;

        DarkTextBox tb = GameIniItems.First(x => x.BrowseButton == sender).TextBox;
        tb.Text = d.FileName;
    }

    private void AutodetectCheckBoxes_CheckedChanged(object? sender, EventArgs e)
    {
        if (sender is not CheckBox checkBox) return;

        var gameIniItem = GameIniItems.First(x => x.AutodetectCheckBox == sender);

        gameIniItem.TextBox.ReadOnly = checkBox.Checked;
        gameIniItem.BrowseButton.Enabled = !checkBox.Checked;

        if (checkBox.Checked) AutodetectGameIni((GameIndex)Array.IndexOf(GameIniItems, gameIniItem), gameIniItem.TextBox);
    }
}
