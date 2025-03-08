using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Microsoft.WindowsAPICodePack.Dialogs;
using static FMInfoGen.Misc;

namespace FMInfoGen;

internal static class Core
{
    internal static MainForm View = null!;

    internal static void Init()
    {
        if (File.Exists(Paths.ConfigFile))
        {
            Ini.ReadConfigIni(Config);
        }

        View = new MainForm();
        View.Show();

        View.SetTempPath(Config.TempPath);

        SetFMsFolder(Config.FMsPath);
        UpdateExtractedFMsListBox();
    }

    #region SetFMsFolder

    internal static void SetFMsFolder()
    {
        string path;
        using (CommonOpenFileDialog d = new())
        {
            d.IsFolderPicker = true;
            if (Directory.Exists(Config.FMsPath))
            {
                d.InitialDirectory = Config.FMsPath;
            }

            if (d.ShowDialog() != CommonFileDialogResult.Ok) return;
            path = d.FileName;
        }

        SetFMsFolder(path);
    }

    internal static void SetFMsFolder(string path)
    {
        if (path.IsEmpty()) return;

        Config.FMsPath = path;

        List<string> files = new();
        foreach (string f in Directory.GetFiles(Config.FMsPath))
        {
            if (f.ExtIsArchive() && !f.ContainsI(".FMSelBak."))
            {
                files.Add(Path.GetFileName(f));
            }
        }

        files.Sort();

        View.SetFMArchives(files);

        SaveConfig();
    }

    #endregion

    internal static void ChangeTempPath()
    {
        string path;
        using (CommonOpenFileDialog d = new())
        {
            d.IsFolderPicker = true;
            if (Directory.Exists(Config.FMsPath))
            {
                d.InitialDirectory = Config.FMsPath;
            }

            if (d.ShowDialog() != CommonFileDialogResult.Ok) return;
            path = d.FileName;
        }

        if (!path.IsEmpty())
        {
            View.SetTempPath(path);
            UpdateExtractedFMsListBox();
        }

        SaveConfig();
    }

    internal static void UpdateExtractedFMsListBox()
    {
        string selectedFM;
        if (Config.FMsPath.IsEmpty() ||
            Config.TempPath.IsEmpty() ||
            (selectedFM = View.GetSelectedFM()).IsEmpty() ||
            !Directory.Exists(Path.Combine(Config.TempPath, selectedFM.FN_NoExt())))
        {
            return;
        }

        IEnumerable<string> items =
            from dir in Directory.GetDirectories(Config.TempPath)
            select Path.GetFileName(dir);

        View.SetExtractedPathList(items);
    }

    internal static void SaveConfig() => Ini.WriteConfigIni(Config);

    internal static void Shutdown()
    {
        SaveConfig();
        Application.Exit();
    }
}
