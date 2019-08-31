using System;
using System.IO;
using System.Windows.Forms;
using AngelLoader.Common.DataClasses;
using AngelLoader.Common.Utility;
using AngelLoader.Importing;
using static AngelLoader.Common.Utility.Methods;

namespace AngelLoader.Forms.Import
{
    public partial class User_FMSel_NDL_ImportControls : UserControl
    {
        internal string Thief1IniFile => Thief1IniTextBox.Text;
        internal string Thief2IniFile => Thief2IniTextBox.Text;
        internal string Thief3IniFile => Thief3IniTextBox.Text;

        private ImportType ImportType;

        // Designer use
        public User_FMSel_NDL_ImportControls() => InitializeComponent();

        internal void Init(ImportType importType)
        {
            ImportType = importType;

            Localize();

            AutodetectGameIni(Game.Thief1, Thief1IniTextBox);
            AutodetectGameIni(Game.Thief2, Thief2IniTextBox);
            AutodetectGameIni(Game.Thief3, Thief3IniTextBox);
        }

        internal void Localize()
        {
            ChooseIniFilesLabel.Text = ImportType == ImportType.NewDarkLoader
                ? LText.Importing.ChooseNewDarkLoaderIniFiles
                : LText.Importing.ChooseFMSelIniFiles;

            Thief1GroupBox.Text = LText.Importing.Thief1;
            Thief2GroupBox.Text = LText.Importing.Thief2;
            Thief3GroupBox.Text = LText.Importing.Thief3;

            Thief1AutodetectCheckBox.Text = LText.Global.Autodetect;
            Thief2AutodetectCheckBox.Text = LText.Global.Autodetect;
            Thief3AutodetectCheckBox.Text = LText.Global.Autodetect;

            Thief1IniBrowseButton.SetTextAutoSize(Thief1IniTextBox, LText.Global.BrowseEllipses);
            Thief2IniBrowseButton.SetTextAutoSize(Thief2IniTextBox, LText.Global.BrowseEllipses);
            Thief3IniBrowseButton.SetTextAutoSize(Thief3IniTextBox, LText.Global.BrowseEllipses);
        }

        private void AutodetectGameIni(Game game, TextBox textBox)
        {
            var iniFile = ImportType == ImportType.NewDarkLoader ? "NewDarkLoader.ini" : "fmsel.ini";

            var fmsPath = GetFMInstallsBasePath(game);
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
                Thief3IniTextBox;

            using (var d = new OpenFileDialog())
            {
                d.Filter = LText.BrowseDialogs.IniFiles + @"|*.ini|" + LText.BrowseDialogs.AllFiles + @"|*.*";
                if (d.ShowDialog() != DialogResult.OK) return;

                tb.Text = d.FileName;
            }
        }

        private void AutodetectCheckBoxes_CheckedChanged(object sender, EventArgs e)
        {
            var s = (CheckBox)sender;
            var textBox =
                s == Thief1AutodetectCheckBox ? Thief1IniTextBox :
                s == Thief2AutodetectCheckBox ? Thief2IniTextBox :
                Thief3IniTextBox;
            var button =
                s == Thief1AutodetectCheckBox ? Thief1IniBrowseButton :
                s == Thief2AutodetectCheckBox ? Thief2IniBrowseButton :
                Thief3IniBrowseButton;
            var game =
                s == Thief1AutodetectCheckBox ? Game.Thief1 :
                s == Thief2AutodetectCheckBox ? Game.Thief2 :
                Game.Thief3;

            textBox.ReadOnly = s.Checked;
            button.Enabled = !s.Checked;

            if (s.Checked) AutodetectGameIni(game, textBox);
        }
    }
}
