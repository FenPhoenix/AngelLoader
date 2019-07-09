using System;
using System.IO;
using System.Windows.Forms;

namespace FenGen
{
    internal partial class MainForm : Form
    {
        internal Model Model { get; set; }

        internal MainForm()
        {
            InitializeComponent();
        }

        private void GenerateButton_Click(object sender, EventArgs e)
        {
            var sourceFile = Path.Combine(Model.ALProjectPath, @"Common\DataClasses\FanMissionData.cs");
            var destFile = Path.Combine(Model.ALProjectPath, @"Ini\FMData.cs");
            Model.GenerateFMData(sourceFile, destFile);
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Environment.Exit(0);
        }
    }
}
