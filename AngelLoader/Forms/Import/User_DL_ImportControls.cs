using System;
using System.IO;
using System.Windows.Forms;
using AngelLoader.DataClasses;
using static AngelLoader.Logger;

namespace AngelLoader.Forms.Import
{
    public partial class User_DL_ImportControls : UserControl
    {
        public User_DL_ImportControls()
        {
            InitializeComponent();
            DarkLoaderIniTextBox.Text = AutodetectDarkLoaderIni();
        }

        internal string DarkLoaderIniText => DarkLoaderIniTextBox.Text;

        private void DarkLoaderIniBrowseButton_Click(object sender, EventArgs e)
        {
            using var d = new OpenFileDialog
            {
                Filter = LText.BrowseDialogs.IniFiles + @"|*.ini|" + LText.BrowseDialogs.AllFiles + @"|*.*"
            };
            if (d.ShowDialog() != DialogResult.OK) return;

            DarkLoaderIniTextBox.Text = d.FileName;
        }

        private void AutodetectCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            var s = AutodetectCheckBox;
            DarkLoaderIniTextBox.ReadOnly = s.Checked;
            DarkLoaderIniBrowseButton.Enabled = !s.Checked;

            if (s.Checked) DarkLoaderIniTextBox.Text = AutodetectDarkLoaderIni();
        }

        internal void Localize()
        {
            ChooseDarkLoaderIniLabel.Text = LText.Importing.DarkLoader_ChooseIni;
            AutodetectCheckBox.Text = LText.Global.Autodetect;
            DarkLoaderIniBrowseButton.SetTextAutoSize(DarkLoaderIniTextBox, LText.Global.BrowseEllipses);
        }

        private static string AutodetectDarkLoaderIni()
        {
            // Common locations. Don't go overboard and search the whole filesystem; that would take forever.
            var dlLocations = new[]
            {
                @"DarkLoader",
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

            foreach (var drive in drives)
            {
                if (!drive.IsReady || drive.DriveType != DriveType.Fixed) continue;

                try
                {
                    foreach (var loc in dlLocations)
                    {
                        var dlIni = Path.Combine(drive.Name, loc, Paths.DarkLoaderIni);
                        if (File.Exists(dlIni)) return dlIni;
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
