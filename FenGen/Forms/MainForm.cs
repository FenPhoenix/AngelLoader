﻿using System;
using System.Windows.Forms;

namespace FenGen.Forms
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private async void GenerateButton_Click(object sender, EventArgs e)
        {
            //var sourceFile = Path.Combine(Core.ALProjectPath, @"Common\DataClasses\FanMissionData.cs");
            //var destFile = Path.Combine(Core.ALProjectPath, @"Ini\FMData.cs");
            //Core.GenerateFMData(sourceFile, destFile);
            //await Roslyn_Test.RunRoslynTest();

#if false
            var langFile = Path.Combine(Core.ALProjectPath, @"Languages\English.ini");
            StateVars.WriteTestLangFile = true;
            StateVars.TestFile = @"C:\AngelLoader\Data\Languages\TestLang.ini";
            LanguageGen.Generate(langFile);
#endif
            //VisLoc.Generate();

            //string sourceFile = Path.Combine(Core.ALProjectPath, @"Common\DataClasses\FanMissionData.cs");
            //string destFile = Path.Combine(Core.ALProjectPath, @"Ini\FMData.cs");
            //FMData.Generate(sourceFile, destFile);

            Core.ReadArgsAndDoTasks();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Environment.Exit(0);
        }
    }
}
