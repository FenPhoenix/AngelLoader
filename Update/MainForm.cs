using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Update;

/*
The plan:
-Ship this executable with AL distribution
-AL downloads the update and puts it in the temp folder, then calls this exe
-This exe then:
 -Waits for AL to close
 -Renames its exe while running
 -Copies the update from the temp folder (including the updater exe from there, which will copy because we've
  renamed ourselves)
 -If successful, delete our renamed exe, call AL, and close
 -If failed, rename our exe back to normal

TODO: Remove debug command line in properties!
*/

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

        // TODO: Handle errors robustly
        using (Process.Start(Path.Combine(Program.DestDir, Program.DestExe))) { }
        Application.Exit();
    }
}
