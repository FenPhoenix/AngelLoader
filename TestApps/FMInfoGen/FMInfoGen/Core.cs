using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using FMScanner;
using JetBrains.Annotations;
using Microsoft.WindowsAPICodePack.Dialogs;
using YamlDotNet.Serialization;
using static FMInfoGen.Misc;

namespace FMInfoGen;

internal static class Core
{
    [PublicAPI]
    internal static MainForm View = null!;

    internal static void Init()
    {
        if (File.Exists(Paths.ConfigFile)) Ini.ReadConfigIni(Config);

        View = new MainForm();
        View.Show();

        View.SetTempPath(Config.TempPath);

        SetFMsFolder(Config.FMsPath);
        UpdateExtractedFMsListBox();
    }

    internal static void OpenYamlFile()
    {
        Process.Start(Path.Combine(Paths.LocalDataPath, View.GetSelectedFMYamlFileName()));
    }

    internal static void OpenFMFolder()
    {
        Process.Start("explorer.exe",
            "\"" +
            Path.Combine(Paths.CurrentExtractedDir, View.GetSelectedFMYamlFileName().FN_NoExt()) +
            "\"");
    }

    #region SetFMsFolder

    internal static void SetFMsFolder()
    {
        CommonFileDialogResult result;
        string path = "";
        using (var d = new CommonOpenFileDialog { IsFolderPicker = true })
        {
            if (Directory.Exists(Config.FMsPath)) d.InitialDirectory = Config.FMsPath;
            result = d.ShowDialog();
            if (result == CommonFileDialogResult.Ok) path = d.FileName;
        }

        if (result == CommonFileDialogResult.Ok && !path.IsEmpty())
        {
            SetFMsFolder(path);
        }
    }

    internal static void SetFMsFolder(string path)
    {
        if (path.IsEmpty()) return;

        Config.FMsPath = path;

        var files = new List<string>();
        foreach (var f in Directory.GetFiles(Config.FMsPath))
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
        CommonFileDialogResult result;
        string path = "";
        using (var d = new CommonOpenFileDialog { IsFolderPicker = true })
        {
            if (Directory.Exists(Config.FMsPath)) d.InitialDirectory = Config.FMsPath;
            result = d.ShowDialog();
            if (result == CommonFileDialogResult.Ok) path = d.FileName;
        }
        if (result == CommonFileDialogResult.Ok && !path.IsEmpty())
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

        var items =
            from dir in Directory.GetDirectories(Config.TempPath)
            select Path.GetFileName(dir);

        View.SetExtractedPathList(items);
    }

    internal static ScannedFMData FMDataFromYamlFile(string file)
    {
        string yaml = File.ReadAllText(file);

        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(new LowerCaseNamingConvention())
            .Build();

        return deserializer.Deserialize<ScannedFMData>(yaml);
    }

    [PublicAPI]
    internal static void SaveConfig() => Ini.WriteConfigIni(Config);

    internal static void Shutdown()
    {
        SaveConfig();
        Application.Exit();
    }
}
