using System;
using System.Drawing;
using System.Windows.Forms;

namespace FenGen
{
    public partial class ExceptionBox : Form
    {
        public ExceptionBox(string exContent)
        {
            InitializeComponent();

            IconPictureBox.Image = SystemIcons.Error.ToBitmap();

            ExceptionTextBox.Text = exContent;
            ExceptionTextBox.SelectionStart = 0;
            ExceptionTextBox.SelectionLength = 0;
        }

        private void CopyButton_Click(object sender, EventArgs e) => ExceptionTextBox.Copy();
    }
}
