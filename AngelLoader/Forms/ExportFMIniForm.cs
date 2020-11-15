using System;
using System.IO;
using System.Windows.Forms;
using AngelLoader.DataClasses;
using static AngelLoader.Logger;
using static AngelLoader.Misc;

namespace AngelLoader.Forms
{
    public sealed partial class ExportFMIniForm : Form
    {
        public ExportFMIniForm() => InitializeComponent();

        public ExportFMIniForm(FanMission fm)
        {
            InitializeComponent();

            NiceNameTextBox.Text = fm.Title;
            // @DIRSEP: ExportFMIniForm: SelectedReadme: Probably both will work here, but we aren't sure.
            // Only Thief 3 FMs normally have readmes in subfolders, and SU's FMSel is closed-source so we don't
            // know if it's writing system dirseps or just explicit backslashes with its fm.ini export feature.
            // We're just going to go with system dirseps for now.
            InfoFileTextBox.Text = fm.SelectedReadme.ToSystemDirSeps();
            ReleaseDateTextBox.Text = fm.ReleaseDate.UnixDateString;
            TagsTextBox.Text = fm.TagsString;

            Localize();
        }

        private void Localize()
        {
            Text = LText.ExportFMIni.TitleText;
            ExportButton.Text = LText.Global.Export;
            Cancel_Button.Text = LText.Global.Cancel;
        }

        private void ExportFMIniForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (DialogResult != DialogResult.OK) return;

            string fileName;
            using (var d = new SaveFileDialog())
            {
                d.Filter = LText.BrowseDialogs.FMIniFile + "|" + Paths.FMIni;
                d.FileName = Paths.FMIni;
                if (d.ShowDialog() != DialogResult.OK)
                {
                    e.Cancel = true;
                    DialogResult = DialogResult.None;
                    return;
                }
                fileName = d.FileName;
            }

            try
            {
                using var sw = new StreamWriter(fileName);
                sw.WriteLine("NiceName=" + NiceNameTextBox.Text);
                sw.WriteLine("InfoFile=" + InfoFileTextBox.Text);
                sw.WriteLine("ReleaseDate=" + ReleaseDateTextBox.Text);
                sw.WriteLine("Tags=" + TagsTextBox.Text);
                sw.WriteLine("Descr=" + DescrTextBox.Text
                    .Replace("\r\n", @"\n")
                    .Replace("\r", @"\n")
                    .Replace("\n", @"\n"));
            }
            catch (Exception ex)
            {
                Log("Exception writing fm.ini", ex);
                MessageBox.Show(ex.Message);
                e.Cancel = true;
                DialogResult = DialogResult.None;
                // ReSharper disable once RedundantJumpStatement
                return;
            }
        }
    }
}
