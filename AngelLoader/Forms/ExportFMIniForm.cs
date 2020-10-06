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
        public ExportFMIniForm()
        {
            InitializeComponent();
        }

        public ExportFMIniForm(FanMission fm)
        {
            NiceNameTextBox.Text = fm.Title;
            InfoFileTextBox.Text = fm.SelectedReadme;
            ReleaseDateTextBox.Text = fm.ReleaseDate.UnixDateString;
            TagsTextBox.Text = fm.TagsString;
        }

        private void ExportButton_Click(object sender, EventArgs e)
        {

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
            }
        }
    }
}
