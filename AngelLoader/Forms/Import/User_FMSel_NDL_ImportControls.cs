using System;
using System.IO;
using System.Windows.Forms;
using AngelLoader.Common.DataClasses;
using AngelLoader.Common.Utility;
using AngelLoader.Importing;
using static AngelLoader.Common.GameSupport;
using static AngelLoader.Common.Misc;

namespace AngelLoader.Forms.Import
{
    public partial class User_FMSel_NDL_ImportControls : UserControl
    {
        internal string Thief1IniFile => Thief1IniTextBox.Text;
        internal string Thief2IniFile => Thief2IniTextBox.Text;
        internal string Thief3IniFile => Thief3IniTextBox.Text;
        internal string SS2IniFile => SS2IniTextBox.Text;

        private ImportType ImportType;

        // Designer use
        public User_FMSel_NDL_ImportControls() => InitializeComponent();

        internal void Init(ImportType importType)
        {
            ImportType = importType;

            Localize();

            AutodetectGameIni(GameIndex.Thief1, Thief1IniTextBox);
            AutodetectGameIni(GameIndex.Thief2, Thief2IniTextBox);
            AutodetectGameIni(GameIndex.Thief3, Thief3IniTextBox);
            AutodetectGameIni(GameIndex.SS2, SS2IniTextBox);
        }

        private void Localize()
        {
            ChooseIniFilesLabel.Text = ImportType == ImportType.NewDarkLoader
                ? LText.Importing.ChooseNewDarkLoaderIniFiles
                : LText.Importing.ChooseFMSelIniFiles;

            Thief1GroupBox.Text = LText.Global.Thief1;
            Thief2GroupBox.Text = LText.Global.Thief2;
            Thief3GroupBox.Text = LText.Global.Thief3;
            SS2GroupBox.Text = LText.Global.SystemShock2;

            Thief1AutodetectCheckBox.Text = LText.Global.Autodetect;
            Thief2AutodetectCheckBox.Text = LText.Global.Autodetect;
            Thief3AutodetectCheckBox.Text = LText.Global.Autodetect;
            SS2AutodetectCheckBox.Text = LText.Global.Autodetect;

            Thief1IniBrowseButton.SetTextAutoSize(Thief1IniTextBox, LText.Global.BrowseEllipses);
            Thief2IniBrowseButton.SetTextAutoSize(Thief2IniTextBox, LText.Global.BrowseEllipses);
            Thief3IniBrowseButton.SetTextAutoSize(Thief3IniTextBox, LText.Global.BrowseEllipses);
            SS2IniBrowseButton.SetTextAutoSize(SS2IniTextBox, LText.Global.BrowseEllipses);
        }

        private void AutodetectGameIni(GameIndex game, TextBox textBox)
        {
            var iniFile = ImportType == ImportType.NewDarkLoader ? "NewDarkLoader.ini" : "fmsel.ini";

            var fmsPath = Config.GetFMInstallPath(game);
            if (fmsPath.IsWhiteSpace())
            {
                textBox.Text = "";
            }
            else
            {
                var iniFileFull = Path.Combine(fmsPath, iniFile);
                textBox.Text = File.Exists(iniFileFull) ? iniFileFull : "";
            }
        }

        private void ThiefIniBrowseButtons_Click(object sender, EventArgs e)
        {
            var s = (Button)sender;
            var tb =
                s == Thief1IniBrowseButton ? Thief1IniTextBox :
                s == Thief2IniBrowseButton ? Thief2IniTextBox :
                s == Thief3IniBrowseButton ? Thief3IniTextBox :
                SS2IniTextBox;

            using var d = new OpenFileDialog
            {
                Filter = LText.BrowseDialogs.IniFiles + @"|*.ini|" + LText.BrowseDialogs.AllFiles + @"|*.*"
            };
            if (d.ShowDialog() != DialogResult.OK) return;

            tb.Text = d.FileName;
        }

        private void AutodetectCheckBoxes_CheckedChanged(object sender, EventArgs e)
        {
            var s = (CheckBox)sender;
            var textBox =
                s == Thief1AutodetectCheckBox ? Thief1IniTextBox :
                s == Thief2AutodetectCheckBox ? Thief2IniTextBox :
                s == Thief3AutodetectCheckBox ? Thief3IniTextBox :
                SS2IniTextBox;
            var button =
                s == Thief1AutodetectCheckBox ? Thief1IniBrowseButton :
                s == Thief2AutodetectCheckBox ? Thief2IniBrowseButton :
                s == Thief3AutodetectCheckBox ? Thief3IniBrowseButton :
                SS2IniBrowseButton;
            var game =
                s == Thief1AutodetectCheckBox ? GameIndex.Thief1 :
                s == Thief2AutodetectCheckBox ? GameIndex.Thief2 :
                s == Thief3AutodetectCheckBox ? GameIndex.Thief3 :
                GameIndex.SS2;

            textBox.ReadOnly = s.Checked;
            button.Enabled = !s.Checked;

            if (s.Checked) AutodetectGameIni(game, textBox);
        }
    }
}
