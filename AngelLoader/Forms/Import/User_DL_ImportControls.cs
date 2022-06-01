using System;
using System.Windows.Forms;
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
            DarkLoaderIniTextBox.Text = Import.AutodetectDarkLoaderIni();
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

            if (AutodetectCheckBox.Checked) DarkLoaderIniTextBox.Text = Import.AutodetectDarkLoaderIni();
        }

        internal void Localize()
        {
            ChooseDarkLoaderIniLabel.Text = LText.Importing.DarkLoader_ChooseIni;
            AutodetectCheckBox.Text = LText.Global.Autodetect;
            DarkLoaderIniBrowseButton.SetTextForTextBoxButtonCombo(DarkLoaderIniTextBox, LText.Global.BrowseEllipses);
        }
    }
}
