using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace AL_UpdateCopy;

public sealed partial class MainForm : Form
{
    public MainForm()
    {
        InitializeComponent();

        CopyingLabel.CenterH(this, clientSize: true);
        CopyingProgressBar.CenterH(this, clientSize: true);
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
            string file = files[i];
            string fileName = Path.GetFileName(file);
            if (fileName.EqualsI("FMData.ini") ||
                fileName.StartsWithI("FMData.bak") ||
                fileName.EqualsI("Config.ini") ||
                fileName.EqualsI(selfExe))
            {
                files.RemoveAt(i);
                i--;
            }
        }

        for (int i = 0; i < files.Count; i++)
        {
            int percent = Utils.GetPercentFromValue_Int(i + 1, files.Count);
            CopyingProgressBar.SetProgress(percent.Clamp(0, 100));

            string file = files[i];
            string fileName = Path.GetFileName(file);
            // TODO: Handle errors robustly
            File.Copy(file, Path.Combine(Program.DestDir, fileName), overwrite: true);
        }
    }
}
