using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace AL_UpdateCopy;

public sealed partial class MainForm : Form
{
    public MainForm()
    {
        InitializeComponent();

        CopyingLabel.CenterHOnForm(this);
        CopyingProgressBar.CenterHOnForm(this);
    }

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);
        DoCopy();
    }

    private void DoCopy()
    {
        string startupPath = Application.StartupPath;
        string selfExe = Path.GetFileName(Application.ExecutablePath);

        List<string> files = Directory.GetFiles(startupPath, "*", SearchOption.AllDirectories).ToList();

        for (int i = 0; i < files.Count; i++)
        {
            string fileName = Path.GetFileName(files[i]);

            if (fileName.EqualsI("FMData.ini") ||
                fileName.StartsWithI("FMData.bak") ||
                fileName.EqualsI("Config.ini") ||
                fileName.EqualsI(selfExe))
            {
                files.RemoveAt(i);
                i--;
            }
        }

        string selfDir = startupPath;
        if (!selfDir.EndsWithDirSep()) selfDir += "\\";

        for (int i = 0; i < files.Count; i++)
        {
            string file = files[i];
            Trace.WriteLine(file);
            string fileName = file.Substring(selfDir.Length);

            CopyingLabel.Text = "Copying..." + Environment.NewLine + fileName;
            CopyingLabel.CenterHOnForm(this);

            // TODO: Handle errors robustly
            string finalFileName = Path.Combine(Program.DestDir, fileName);
            Directory.CreateDirectory(Path.GetDirectoryName(finalFileName)!);
            File.Copy(file, Path.Combine(Program.DestDir, fileName), overwrite: true);

            //for (int t = 0; t < 100; t++)
            //{
            //    Thread.Sleep(1);
            //    Application.DoEvents();
            //}

            int percent = Utils.GetPercentFromValue_Int(i + 1, files.Count);
            CopyingProgressBar.SetProgress(percent);
        }
    }
}
