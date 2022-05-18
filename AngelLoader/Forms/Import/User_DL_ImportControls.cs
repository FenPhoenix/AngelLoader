using System;
using System.IO;
using System.Windows.Forms;
using static AngelLoader.Logger;
using static AngelLoader.Misc;

namespace AngelLoader.Forms
{
    public sealed partial class User_DL_ImportControls : UserControl
    {
        public User_DL_ImportControls()
        {
#if DEBUG
            InitializeComponent();
#else
            InitializeComponentSlim();
#endif
            DarkLoaderIniTextBox.Text = AutodetectDarkLoaderIni();
        }

        internal string DarkLoaderIniText => DarkLoaderIniTextBox.Text;

        private void DarkLoaderIniBrowseButton_Click(object sender, EventArgs e)
        {
            using var d = new OpenFileDialog
            {
                Filter = LText.BrowseDialogs.IniFiles + "|*.ini|" + LText.BrowseDialogs.AllFiles + "|*.*"
            };
            if (d.ShowDialogDark(FindForm()) != DialogResult.OK) return;

            DarkLoaderIniTextBox.Text = d.FileName;
        }

        private void AutodetectCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            DarkLoaderIniTextBox.ReadOnly = AutodetectCheckBox.Checked;
            DarkLoaderIniBrowseButton.Enabled = !AutodetectCheckBox.Checked;

            if (AutodetectCheckBox.Checked) DarkLoaderIniTextBox.Text = AutodetectDarkLoaderIni();
        }

        internal void Localize()
        {
            ChooseDarkLoaderIniLabel.Text = LText.Importing.DarkLoader_ChooseIni;
            AutodetectCheckBox.Text = LText.Global.Autodetect;
            DarkLoaderIniBrowseButton.SetTextForTextBoxButtonCombo(DarkLoaderIniTextBox, LText.Global.BrowseEllipses);
        }

        private static string AutodetectDarkLoaderIni()
        {
            // Common locations. Don't go overboard and search the whole filesystem; that would take forever.
            string[] dlLocations =
            {
                "DarkLoader",
                @"Games\DarkLoader"
            };

            DriveInfo[] drives;
            try
            {
                drives = DriveInfo.GetDrives();
            }
            catch (Exception ex)
            {
                Log("Exception in GetDrives()", ex);
                return "";
            }

            foreach (DriveInfo drive in drives)
            {
                if (!drive.IsReady || drive.DriveType != DriveType.Fixed) continue;

                try
                {
                    foreach (string loc in dlLocations)
                    {
                        if (TryCombineFilePathAndCheckExistence(drive.Name, loc, Paths.DarkLoaderIni, out string dlIni))
                        {
                            return dlIni;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log("Exception in DarkLoader multi-drive search", ex);
                }
            }

            return "";
        }
    }
}
