using System;
using System.Linq;
using System.Windows.Forms;
using AngelLoader.DataClasses;
using AngelLoader.Importing;
using static AngelLoader.GameSupport;
using static AngelLoader.Misc;

namespace AngelLoader.Forms.Import
{
    public partial class User_FMSel_NDL_ImportControls : UserControl
    {
        private ImportType ImportType;

        private readonly (GroupBox GroupBox, CheckBox AutodetectCheckBox, TextBox TextBox, Button BrowseButton)[]
        GameIniItems;

        internal string GetIniFile(GameIndex game) => GameIniItems[(int)game].TextBox.Text;

        public User_FMSel_NDL_ImportControls()
        {
            InitializeComponent();

            GameIniItems = new[]
            {
                (
                    Thief1GroupBox,
                    Thief1AutodetectCheckBox,
                    Thief1IniTextBox,
                    Thief1IniBrowseButton
                ),
                (
                    Thief2GroupBox,
                    Thief2AutodetectCheckBox,
                    Thief2IniTextBox,
                    Thief2IniBrowseButton
                ),
                (
                    Thief3GroupBox,
                    Thief3AutodetectCheckBox,
                    Thief3IniTextBox,
                    Thief3IniBrowseButton
                ),
                (
                    SS2GroupBox,
                    SS2AutodetectCheckBox,
                    SS2IniTextBox,
                    SS2IniBrowseButton
                )
            };
        }

        internal void Init(ImportType importType)
        {
            ImportType = importType;

            Localize();

            for (int i = 0; i < SupportedGameCount; i++)
            {
                AutodetectGameIni((GameIndex)i, GameIniItems[i].TextBox);
            }
        }

        private void Localize()
        {
            ChooseIniFilesLabel.Text = ImportType == ImportType.NewDarkLoader
                ? LText.Importing.ChooseNewDarkLoaderIniFiles
                : LText.Importing.ChooseFMSelIniFiles;

            for (int i = 0; i < SupportedGameCount; i++)
            {
                GameIniItems[i].GroupBox.Text = GetLocalizedGameName((GameIndex)i);
                GameIniItems[i].AutodetectCheckBox.Text = LText.Global.Autodetect;
                GameIniItems[i].BrowseButton.SetTextAutoSize(GameIniItems[i].TextBox, LText.Global.BrowseEllipses);
            }
        }

        private void AutodetectGameIni(GameIndex game, TextBox textBox)
        {
            string iniFile = ImportType == ImportType.NewDarkLoader ? Paths.NewDarkLoaderIni : Paths.FMSelIni;

            string fmsPath = Config.GetFMInstallPath(game);
            textBox.Text = !fmsPath.IsWhiteSpace() && TryCombineFilePathAndCheckExistence(fmsPath, iniFile, out string iniFileFull)
                ? iniFileFull
                : "";
        }

        private void ThiefIniBrowseButtons_Click(object sender, EventArgs e)
        {
            using var d = new OpenFileDialog
            {
                Filter = LText.BrowseDialogs.IniFiles + @"|*.ini|" + LText.BrowseDialogs.AllFiles + @"|*.*"
            };
            if (d.ShowDialog() != DialogResult.OK) return;

            var tb = GameIniItems.First(x => x.BrowseButton == sender).TextBox;
            tb.Text = d.FileName;
        }

        private void AutodetectCheckBoxes_CheckedChanged(object sender, EventArgs e)
        {
            var s = (CheckBox)sender;

            var gameIniItem = GameIniItems.First(x => x.AutodetectCheckBox == sender);

            gameIniItem.TextBox.ReadOnly = s.Checked;
            gameIniItem.BrowseButton.Enabled = !s.Checked;

            if (s.Checked) AutodetectGameIni((GameIndex)Array.IndexOf(GameIniItems, gameIniItem), gameIniItem.TextBox);
        }
    }
}
