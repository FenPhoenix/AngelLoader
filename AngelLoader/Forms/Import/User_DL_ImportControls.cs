using System;
using System.Windows.Forms;
using AngelLoader.Common.DataClasses;
using AngelLoader.Common.Utility;

namespace AngelLoader.Forms.Import
{
    public partial class User_DL_ImportControls : UserControl
    {
        public User_DL_ImportControls() => InitializeComponent();

        internal string DarkLoaderIniText
        {
            get => DarkLoaderIniTextBox.Text;
            set => DarkLoaderIniTextBox.Text = value;
        }

        internal bool ImportFMData => ImportFMDataCheckBox.Checked;
        internal bool ImportSaves => ImportSavesCheckBox.Checked;

        private void DarkLoaderIniBrowseButton_Click(object sender, EventArgs e)
        {
            using (var d = new OpenFileDialog())
            {
                d.Filter = LText.BrowseDialogs.IniFiles + @"|*.ini|" + LText.BrowseDialogs.AllFiles + @"|*.*";
                if (d.ShowDialog() != DialogResult.OK) return;

                DarkLoaderIniTextBox.Text = d.FileName;
            }
        }

        private void AutodetectCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            var s = AutodetectCheckBox;
            DarkLoaderIniTextBox.ReadOnly = s.Checked;
            DarkLoaderIniBrowseButton.Enabled = !s.Checked;

            if (s.Checked) DarkLoaderIniTextBox.Text = ImportCommon.AutodetectDarkLoaderIni();
        }

        internal void Localize()
        {
            ChooseDarkLoaderIniLabel.Text = LText.Importing.DarkLoader_ChooseIni;
            AutodetectCheckBox.Text = LText.Global.Autodetect;
            DarkLoaderIniBrowseButton.SetTextAutoSize(DarkLoaderIniTextBox, LText.Global.BrowseEllipses);
            ImportFMDataCheckBox.Text = LText.Importing.DarkLoader_ImportFMData;
            ImportSavesCheckBox.Text = LText.Importing.DarkLoader_ImportSaves;
        }
    }
}
