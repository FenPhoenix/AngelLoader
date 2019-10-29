using System;
using System.Windows.Forms;

namespace FenGen
{
    internal partial class MainForm : Form
    {
        internal MainForm()
        {
            InitializeComponent();
        }

        private async void GenerateButton_Click(object sender, EventArgs e)
        {
            //var sourceFile = Path.Combine(Core.ALProjectPath, @"Common\DataClasses\FanMissionData.cs");
            //var destFile = Path.Combine(Core.ALProjectPath, @"Ini\FMData.cs");
            //Core.GenerateFMData(sourceFile, destFile);
            await Roslyn_Test.RunRoslynTest();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Environment.Exit(0);
        }
    }
}
