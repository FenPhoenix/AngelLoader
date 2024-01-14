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
 -Deletes its renamed exe if it exists
 -Renames its exe while running
 -Copies the update from the temp folder (including the updater exe from there, which will copy because we've
  renamed ourselves)
 -If successful, call AL, and close
 -AL will delete our renamed exe (possibly on next close, so it doesn't have to wait for us to close?)
 -If failed, rename our exe back to normal

TODO: Remove debug command line in properties!
*/

public sealed partial class MainForm : Form
{
    public MainForm()
    {
#if DEBUG
        InitializeComponent();
#else
        InitSlim();
#endif

        Text = "AngelLoader Update";
        CopyingLabel.Text = "Copying...";

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
        string exePath = Application.ExecutablePath;

        try
        {
            File.Delete(exePath + ".bak");
        }
        catch
        {
            // didn't exist or whatever
        }

        File.Move(exePath, exePath + ".bak");

        List<string> files = Directory.GetFiles(Program.UpdateTempPath, "*", SearchOption.AllDirectories).ToList();

        for (int i = 0; i < files.Count; i++)
        {
            string fileName = Path.GetFileName(files[i]);

            if (fileName.EqualsI("FMData.ini") ||
                fileName.StartsWithI("FMData.bak") ||
                fileName.EqualsI("Config.ini"))
            {
                files.RemoveAt(i);
                i--;
            }
        }

        string updateDirWithTrailingDirSep = Program.UpdateTempPath.TrimEnd('\\', '/') + "\\";

        for (int i = 0; i < files.Count; i++)
        {
            string file = files[i];
            string fileName = file.Substring(updateDirWithTrailingDirSep.Length);

            CopyingLabel.Text = "Copying..." + Environment.NewLine + fileName;
            CopyingLabel.CenterHOnForm(this);

            // TODO: Handle errors robustly
            string finalFileName = Path.Combine(startupPath, fileName);
            Directory.CreateDirectory(Path.GetDirectoryName(finalFileName)!);
            File.Copy(file, finalFileName, overwrite: true);

            //for (int t = 0; t < 100; t++)
            //{
            //    Thread.Sleep(1);
            //    Application.DoEvents();
            //}

            int percent = Utils.GetPercentFromValue_Int(i + 1, files.Count);
            CopyingProgressBar.SetProgress(percent);
        }

        Utils.ClearUpdateTempPath();

        // TODO: Handle errors robustly
        using (Process.Start(Path.Combine(startupPath, "AngelLoader.exe"))) { }
        Application.Exit();
    }
}
