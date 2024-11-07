/*
Unmodified, as-is local testing app. There's a bunch of hardcoded paths that won't work on your machine, so you'll
have to modify them. Just adding it for completeness.

Public domain/CC0. It's just a test frontend.
*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using FMScanner;
using WK.Libraries.SharpClipboardNS;
using static FMInfoGen.Misc;

namespace FMInfoGen;

internal sealed partial class MainForm : Form
{
    private bool LoadingAccuracySettings { get; set; }

    private readonly SharpClipboard _sharpClipboard;
    private bool _clipboardMonitoringEnabled;

    #region Interface

    #region Get

    internal string GetTempPath() => TempPathTextBox.Text;

    internal IEnumerable<string> GetFMArchives() => FMsListBox.Items.OfType<string>();

    internal string GetSelectedFM() => FMsListBox.SelectedIndex == -1 ? "" : FMsListBox.SelectedItem.ToString();

    internal string GetSelectedFMYamlFileName() => FMInfoFilesListView.SelectedItems[0].SubItems[1].Text;

    internal ScanOptions GetSelectedScanOptions() => new()
    {
        ScanTitle = ScanTitleCheckBox.Checked,
        ScanCampaignMissionNames = ScanIncludedMissionsCheckBox.Checked,
        ScanAuthor = ScanAuthorCheckBox.Checked,
        ScanVersion = ScanVersionCheckBox.Checked,
        ScanLanguages = ScanLanguagesCheckBox.Checked,
        ScanGameType = ScanGameTypeCheckBox.Checked,
        ScanNewDarkRequired = ScanNewDarkRequiredCheckBox.Checked,
        ScanNewDarkMinimumVersion = ScanNDMinVerCheckBox.Checked,
        ScanCustomResources = ScanCustomResourcesCheckBox.Checked,
        ScanSize = ScanSizeCheckBox.Checked,
        ScanReleaseDate = ScanReleaseDateCheckBox.Checked,
        ScanTags = ScanTagsCheckBox.Checked,
        ScanDescription = ScanDescriptionCheckBox.Checked,
    };

    internal bool GetOverwriteFoldersChecked() => OverwriteFoldersCheckBox.Checked;

    #endregion

    #region Set

    internal void SetFMArchives(IEnumerable<string> items) => SetListBoxItems(FMsListBox, items);

    internal void SetTempPath(string text) => TempPathTextBox.Text = text;

    internal void SetExtractedPathList(IEnumerable<string> items) => SetListBoxItems(ExtractedPathListBox, items);

    internal void SetFMScanProgressBarValue(int value) => FMScanProgressBar.SetValueClamped(value);

    internal void SetExtractProgressBarValue(int value) => ExtractProgressBar.SetValueClamped(value);

    internal void SetDebugLabelText(string text) => DebugLabel.Text = text;

    #endregion

    #region Begin/end modes

    internal void BeginExtractArchiveMode()
    {
        ExtractProgressBar.Show();
        CancelExtractAllButton.Show();
        SetUnsafeControlsEnabled(false);
    }

    internal void EndExtractArchiveMode()
    {
        CancelExtractAllButton.Hide();
        ExtractProgressBar.Hide();
        ExtractProgressBar.Value = 0;
        SetUnsafeControlsEnabled(true);
    }

    internal void BeginScanMode()
    {
        SetUnsafeControlsEnabled(false);
        // Visual: to prevent the log textbox from being focused
        MainTabPage.Focus();
    }

    internal void EndScanMode() => SetUnsafeControlsEnabled(true);

    #endregion

    #endregion

    internal MainForm()
    {
        InitializeComponent();

        _sharpClipboard = new SharpClipboard();
        _sharpClipboard.ClipboardChanged += _sharpClipboard_ClipboardChanged;

        FMInfoFilesListView.GetType()
            .GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic)
            !.SetValue(FMInfoFilesListView, true);

        foreach (var c in AccuracyCheckPanel.Controls) ((CheckBox)c).Tag = null;
    }

    #region Private methods

    private async Task ScanAllFMsIfAny(bool zips)
    {
        if (FMsListBox.Items.Count == 0)
        {
            MessageBox.Show("No FMs in the list!");
            return;
        }
        await FMScan.ScanAllFMs(zips);
    }

    private void SetUnsafeControlsEnabled(bool enabled)
    {
        MainTab_ListsPanel.Enabled = enabled;
        ScanControlsGroupBox.Enabled = enabled;
        ScanOptionsGroupBox.Enabled = enabled;
        TestButton.Enabled = enabled;
        Test2Button.Enabled = enabled;
        AccuracyTabPage.Enabled = enabled;
    }

    private static void SetListBoxItems(ListBox listBox, IEnumerable<string> items)
    {
        listBox.Items.Clear();
        listBox.ColumnWidth = 0;

        listBox.BeginUpdate();
        foreach (string item in items) listBox.Items.Add(item);
        listBox.EndUpdate();

        listBox.AutoSizeColumns();
    }

    private static string MakeAccuracyFilePath(ListViewItem listViewItem) =>
        Path.Combine(Paths.AccuracyDataPath, listViewItem.SubItems[1].Text.FN_NoExt() + ".txt");

    private void PopulateFMInfoList()
    {
        FMInfoFilesListView.ListViewItemSorter = null;

        FMInfoFilesListView.Items.Clear();

        AccuracyCheckPanel.Hide();

        OpenFMFolderButton.Enabled = false;
        OpenYamlButton.Enabled = false;
        CountButton.Enabled = false;

        var mainTimer = new Stopwatch();
        var readTimer = new Stopwatch();
        var populateTimer = new Stopwatch();

        mainTimer.Start();

        var items = new List<ListViewItem>();

        readTimer.Start();

        static string ConvertSize(ulong? size)
        {
            return size == null
                ? ""
                : size < ByteSize.MB
                    ? Math.Round((ulong)size / 1024f).ToString(CultureInfo.CurrentCulture) + " KB"
                    : size is >= ByteSize.MB and < ByteSize.GB
                        ? Math.Round((ulong)size / 1024f / 1024f).ToString(CultureInfo.CurrentCulture) + " MB"
                        : Math.Round((ulong)size / 1024f / 1024f / 1024f, 2).ToString(CultureInfo.CurrentCulture) + " GB";
        }

        foreach (string f in Directory.GetFiles(Paths.LocalDataPath, "*.yaml"))
        {
            var fmDataTemp = Core.FMDataFromYamlFile(f);

            // All because array elements can be null
            string languages = "";
            if (fmDataTemp.Languages.Length > 0)
            {
                foreach (string lang in fmDataTemp.Languages)
                {
                    if (lang.IsEmpty()) continue;

                    if (!languages.IsEmpty()) languages += ", ";
                    languages += lang;
                }
            }

            items.Add(
                new ListViewItem(
                        new[]
                        {
                            fmDataTemp.Title,
                            Path.GetFileName(f),
                            fmDataTemp.Author,
                            fmDataTemp.Version,
                            languages,
                            fmDataTemp.Game.ToString(),
                            fmDataTemp.NewDarkRequired.ToString(),
                            fmDataTemp.NewDarkMinRequiredVersion,
                            "", //fmDataTemp.OriginalReleaseDate,
                            "", //fmDataTemp.LastUpdateDate,
                            fmDataTemp.Type.ToString(),
                            fmDataTemp.HasCustomScripts.ToString(),
                            fmDataTemp.HasCustomTextures.ToString(),
                            fmDataTemp.HasCustomSounds.ToString(),
                            fmDataTemp.HasCustomObjects.ToString(),
                            fmDataTemp.HasCustomCreatures.ToString(),
                            fmDataTemp.HasCustomMotions.ToString(),
                            fmDataTemp.HasAutomap.ToString(),
                            fmDataTemp.HasMap.ToString(),
                            fmDataTemp.HasMovies.ToString(),
                            fmDataTemp.HasCustomSubtitles.ToString(),
                            ConvertSize(fmDataTemp.Size),
                        })
                    { UseItemStyleForSubItems = false });
        }

        readTimer.Stop();
        Trace.WriteLine("reading yaml files + filling ListViewItemsList items() took:\n" + readTimer.Elapsed);

        ListViewItem[] itemsArray = items.ToArray();

        populateTimer.Start();

        FMInfoFilesListView.BeginUpdate();
        FMInfoFilesListView.Items.AddRange(itemsArray);
        FMInfoFilesListView.EndUpdate();

        populateTimer.Stop();
        Trace.WriteLine("populating listview took:\n" + populateTimer.Elapsed);

        foreach (ListViewItem item in FMInfoFilesListView.Items)
        {
            string accFile = MakeAccuracyFilePath(item);
            if (File.Exists(accFile))
            {
                var acc = Ini.ReadAccuracyData(accFile);

                ColorItem(item.SubItems[0], acc.Title);
                ColorItem(item.SubItems[2], acc.Author);
                ColorItem(item.SubItems[3], acc.Version);
                ColorItem(item.SubItems[4], acc.Languages);
                ColorItem(item.SubItems[5], acc.Game);
                ColorItem(item.SubItems[6], acc.NewDarkRequired);
                ColorItem(item.SubItems[7], acc.NewDarkMinRequiredVersion);
                ColorItem(item.SubItems[10], acc.Type);
                ColorItem(item.SubItems[11], acc.HasCustomScripts);
                ColorItem(item.SubItems[12], acc.HasCustomTextures);
                ColorItem(item.SubItems[13], acc.HasCustomSounds);
                ColorItem(item.SubItems[14], acc.HasCustomObjects);
                ColorItem(item.SubItems[15], acc.HasCustomCreatures);
                ColorItem(item.SubItems[16], acc.HasCustomMotions);
                ColorItem(item.SubItems[17], acc.HasAutomap);
                ColorItem(item.SubItems[18], acc.HasMap);
                ColorItem(item.SubItems[19], acc.HasMovies);
                ColorItem(item.SubItems[20], acc.HasCustomSubtitles);
            }
            else
            {
                for (int i = 0; i < item.SubItems.Count; i++)
                {
                    if (i is 1 or 8 or 9) continue;

                    var si = item.SubItems[i];
                    ColorItem(si, null);
                }
            }
        }

        FMInfoFilesListView.EndUpdate();

        mainTimer.Stop();

        Trace.WriteLine("total:\n" + mainTimer.Elapsed);
    }

    private static void ColorItem(ListViewItem.ListViewSubItem item, bool? value)
    {
        // Tags are for sorting purposes
        switch (value)
        {
            case true:
            {
                if (!item.Text.IsEmpty())
                {
                    item.ForeColor = Color.Green;
                }
                else
                {
                    item.BackColor = Color.Green;
                }

                item.Font = new Font(item.Font.FontFamily, item.Font.Size, FontStyle.Bold);
                item.Tag = "2";
                break;
            }
            case false:
            {
                if (!item.Text.IsEmpty())
                {
                    item.ForeColor = Color.Red;
                }
                else
                {
                    item.BackColor = Color.Red;
                }

                item.Font = new Font(item.Font.FontFamily, item.Font.Size, FontStyle.Bold);
                item.Tag = "1";
                break;
            }
            case null:
            {
                item.ForeColor = Color.Black;
                item.BackColor = Color.White;

                item.Font = new Font(item.Font.FontFamily, item.Font.Size, FontStyle.Regular);
                item.Tag = "0";
                break;
            }
        }
    }

    #endregion

    #region Event handlers

    private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
    {
        FormClosing -= MainForm_FormClosing;
        Core.Shutdown();
    }

    private void AccuracyCheckBoxes_CheckStateChanged(object sender, EventArgs e)
    {
        CheckBox s = (CheckBox)sender;
        s.Tag = s.CheckState == CheckState.Indeterminate ? null : s.Checked;

        if (LoadingAccuracySettings) return;

        int itemNum =
            s == TitleCheckBox ? 0 :
            s == AuthorCheckBox ? 2 :
            s == VersionCheckBox ? 3 :
            s == LanguagesCheckBox ? 4 :
            s == GameCheckBox ? 5 :
            s == NewDarkCheckBox ? 6 :
            s == NewDarkVerReqCheckBox ? 7 :
            s == TypeCheckBox ? 10 :
            s == ScriptsCheckBox ? 11 :
            s == TexturesCheckBox ? 12 :
            s == SoundsCheckBox ? 13 :
            s == ObjectsCheckBox ? 14 :
            s == CreaturesCheckBox ? 15 :
            s == MotionsCheckBox ? 16 :
            s == AutomapCheckBox ? 17 :
            s == MapCheckBox ? 18 :
            s == MoviesCheckBox ? 19 :
            s == SubtitlesCheckBox ? 20 :
            0;

        foreach (ListViewItem item in FMInfoFilesListView.SelectedItems)
        {
            ColorItem(item.SubItems[itemNum], (bool?)s.Tag);

            var acc = new AccuracyData
            {
                Title = (bool?)TitleCheckBox.Tag,
                Author = (bool?)AuthorCheckBox.Tag,
                Version = (bool?)VersionCheckBox.Tag,
                Languages = (bool?)LanguagesCheckBox.Tag,
                Game = (bool?)GameCheckBox.Tag,
                NewDarkRequired = (bool?)NewDarkCheckBox.Tag,
                NewDarkMinRequiredVersion = (bool?)NewDarkVerReqCheckBox.Tag,
                Type = (bool?)TypeCheckBox.Tag,
                HasCustomScripts = (bool?)ScriptsCheckBox.Tag,
                HasCustomTextures = (bool?)TexturesCheckBox.Tag,
                HasCustomSounds = (bool?)SoundsCheckBox.Tag,
                HasCustomObjects = (bool?)ObjectsCheckBox.Tag,
                HasCustomCreatures = (bool?)CreaturesCheckBox.Tag,
                HasCustomMotions = (bool?)MotionsCheckBox.Tag,
                HasAutomap = (bool?)AutomapCheckBox.Tag,
                HasMap = (bool?)MapCheckBox.Tag,
                HasMovies = (bool?)MoviesCheckBox.Tag,
                HasCustomSubtitles = (bool?)SubtitlesCheckBox.Tag,
            };

            Ini.WriteAccuracyData(acc, MakeAccuracyFilePath(item));
        }
    }

    private void AccuracyCheckBoxes_Click(object sender, EventArgs e)
    {
        CheckBox s = (CheckBox)sender;

        s.CheckState = s.CheckState switch
        {
            CheckState.Indeterminate => CheckState.Checked,
            CheckState.Checked => CheckState.Unchecked,
            _ => CheckState.Indeterminate,
        };

        FMInfoFilesListView.Focus();
    }

    private void FMInfoFilesListView_KeyPress(object sender, KeyPressEventArgs e)
    {
        if (e.KeyChar == (char)Keys.Enter && FMInfoFilesListView.SelectedItems.Count == 1)
        {
            Core.OpenFMFolder();
        }
    }

    private void SetFMsFolderButton_Click(object sender, EventArgs e) => Core.SetFMsFolder();

    private void SetTempPathButton_Click(object sender, EventArgs e) => Core.ChangeTempPath();

    private void TempPathTextBox_TextChanged(object sender, EventArgs e) => Config.TempPath = TempPathTextBox.Text;

    private void ExtractFMArchiveButton_Click(object sender, EventArgs e) => FMExtract.ExtractFMArchive();

    private async void ExtractAllFMArchivesButton_Click(object sender, EventArgs e) => await FMExtract.ExtractAllFMArchives();

    private void CancelExtractAllButton_Click(object sender, EventArgs e) => FMExtract.Cancel();

    private async void GetFromZipsButton_Click(object sender, EventArgs e) => await ScanAllFMsIfAny(zips: true);

    private async void GetFromFoldersButton_Click(object sender, EventArgs e) => await ScanAllFMsIfAny(zips: false);

    private void GetOneFromZipButton_Click(object sender, EventArgs e)
    {
        if (FMsListBox.SelectedIndex == -1) return;
        FMScan.ScanFM(FMsListBox.SelectedItem.ToString(), zip: true);
    }

    private void GetOneFromFolderButton_Click(object sender, EventArgs e)
    {
        if (FMsListBox.SelectedIndex > -1)
        {
            string item = Path.Combine(Paths.CurrentExtractedDir,
                FMsListBox.SelectedItem.ToString().FN_NoExt().Trim());
            FMScan.ScanFM(item, zip: false, Paths.LocalDataFolderVerPath);
        }
    }

    private void PopulateFMInfoListButton_Click(object sender, EventArgs e) => PopulateFMInfoList();

    private void OpenYamlButton_Click(object sender, EventArgs e) => Core.OpenYamlFile();

    private void OpenFMFolderButton_Click(object sender, EventArgs e) => Core.OpenFMFolder();

    private void FMInfoFilesListView_ColumnClick(object sender, ColumnClickEventArgs e)
    {
        if (FMInfoFilesListView.ListViewItemSorter is not Comparers.ItemComparer sorter)
        {
            sorter = new Comparers.ItemComparer(e.Column) { Order = SortOrder.Ascending };
            FMInfoFilesListView.ListViewItemSorter = sorter;
        }

        if (e.Column == sorter.Column)
        {
            sorter.Order = sorter.Order == SortOrder.Ascending ? SortOrder.Descending : SortOrder.Ascending;
        }
        else
        {
            sorter.Column = e.Column;
            sorter.Order = SortOrder.Ascending;
        }

        FMInfoFilesListView.Sort();
    }

    private void FMInfoFilesListView_SelectedIndexChanged(object sender, EventArgs e)
    {
        // Not only clears the textbox but also prevents a silent exception from occurring
        if (FMInfoFilesListView.SelectedItems.Count == 0)
        {
            FMInfoTextBox.Clear();
            AccuracyCheckPanel.Hide();
            OpenFMFolderButton.Enabled = false;
            OpenYamlButton.Enabled = false;
            CountButton.Enabled = false;
            return;
        }

        bool oneSelected = FMInfoFilesListView.SelectedItems.Count == 1;

        OpenFMFolderButton.Enabled = oneSelected;
        OpenYamlButton.Enabled = oneSelected;
        CountButton.Enabled = true;

        AccuracyCheckPanel.Show();

        var fmData = Core.FMDataFromYamlFile(
            Path.Combine(
                Paths.LocalDataPath,
                FMInfoFilesListView.SelectedItems[0].SubItems[1].Text));

        FMInfoTextBox.Clear();
        if (fmData.Type == FMType.Campaign)
        {
            FMInfoTextBox.Text += "Missions in this campaign:\r\n";
            foreach (string mission in fmData.IncludedMissions)
            {
                FMInfoTextBox.Text += mission + "\r\n";
            }
        }

        if (!fmData.Description.IsWhiteSpace())
        {
            if (!FMInfoTextBox.Text.IsEmpty()) FMInfoTextBox.Text += "\r\n";

            FMInfoTextBox.Text += fmData.Description.Replace("\n", "\r\n");
        }

        string accFile = Path.Combine(Paths.AccuracyDataPath,
            FMInfoFilesListView.SelectedItems[0].SubItems[1].Text.FN_NoExt() +
            ".txt");
        if (!File.Exists(accFile))
        {
            LoadingAccuracySettings = true;

            foreach (var c in AccuracyCheckPanel.Controls)
            {
                ((CheckBox)c).CheckState = CheckState.Indeterminate;
            }

            LoadingAccuracySettings = false;
        }
        else
        {
            var acc = Ini.ReadAccuracyData(accFile);

            LoadingAccuracySettings = true;

            var fields = new (CheckBox CheckBox, bool? AccField)[]
            {
                (TitleCheckBox, acc.Title),
                (AuthorCheckBox, acc.Author),
                (VersionCheckBox, acc.Version),
                (LanguagesCheckBox, acc.Languages),
                (GameCheckBox, acc.Game),
                (NewDarkCheckBox, acc.NewDarkRequired),
                (NewDarkVerReqCheckBox, acc.NewDarkMinRequiredVersion),
                (TypeCheckBox, acc.Type),
                (ScriptsCheckBox, acc.HasCustomScripts),
                (TexturesCheckBox, acc.HasCustomTextures),
                (SoundsCheckBox, acc.HasCustomSounds),
                (ObjectsCheckBox, acc.HasCustomObjects),
                (CreaturesCheckBox, acc.HasCustomCreatures),
                (MotionsCheckBox, acc.HasCustomMotions),
                (AutomapCheckBox, acc.HasAutomap),
                (MapCheckBox, acc.HasMap),
                (MoviesCheckBox, acc.HasMovies),
                (SubtitlesCheckBox, acc.HasCustomSubtitles),
            };

            for (int i = 0; i < fields.Length; i++)
            {
                var field = fields[i];

                field.CheckBox.CheckState = field.AccField switch
                {
                    null => CheckState.Indeterminate,
                    true => CheckState.Checked,
                    _ => CheckState.Unchecked,
                };
            }

            LoadingAccuracySettings = false;
        }
    }

    private void CountButton_Click(object sender, EventArgs e) => MessageBox.Show(FMInfoFilesListView.SelectedItems.Count.ToString());

    private void CancelFMScanButton_Click(object sender, EventArgs e) => FMScan.CancelScan();

    private void SetFMsFolderTo1098Button_Click(object sender, EventArgs e) => Core.SetFMsFolder(Paths.T1T2ArchivePath);

    private void SetFMsFolderToSS2Button_Click(object sender, EventArgs e) => Core.SetFMsFolder(Paths.SS2ArchivePath);

    private void SetFMsFolderToT3Button_Click(object sender, EventArgs e) => Core.SetFMsFolder(Paths.T3ArchivePath);

    private void SetFMsFolderTo7zTestButton_Click(object sender, EventArgs e) => Core.SetFMsFolder(Paths.SevenZipTestPath);

    #region Testing

    private void TestButton_Click(object sender, EventArgs e)
    {
    }

    private void Test2Button_Click(object sender, EventArgs e)
    {
        const string item = @"F:\FM packs\SS2 FM Pack\ss2 fms\Extracted\Blind Disposition v1.3";
        FMScan.ScanFM(item, false, Path.Combine(Paths.LocalTestPath, "localdata-ss2-folder-ver"));
    }

    #endregion

    #endregion

    private void _sharpClipboard_ClipboardChanged(object sender, SharpClipboard.ClipboardChangedEventArgs e)
    {
        if (!_clipboardMonitoringEnabled) return;

        Trace.WriteLine(e.ContentType);

        if (e.ContentType == SharpClipboard.ContentTypes.Text
            && _sharpClipboard.ClipboardText.ContainsI(".yaml")
           )
        {
            //Trace.WriteLine(_sharpClipboard.ClipboardText);
            //return;

            string fmDir = _sharpClipboard.ClipboardText.Substring(0,
                _sharpClipboard.ClipboardText.IndexOf(".yaml", StringComparison.InvariantCultureIgnoreCase));

            //Trace.WriteLine(fmDir);

            try
            {
                Process.Start("explorer.exe",
                    "\"" + Path.Combine(@"F:\FM packs\FM pack\All\ExtractPermanent", fmDir) + "\"");
            }
            catch
            {
                // ignore
            }
        }
    }

    private void FastManualDiffButton_Click(object sender, EventArgs e)
    {
        _clipboardMonitoringEnabled = !_clipboardMonitoringEnabled;

        FastManualDiffButton.Text = _clipboardMonitoringEnabled ? "Fast manual diff (*)" : "Fast manual diff";

        //string[] lines = File.ReadAllLines(@"C:\report.txt");
        //for (int i = 0; i < lines.Length; i++)
        //{
        //    string line = lines[i];

        //    string fmDir;
        //    if (line.StartsWith("\""))
        //    {
        //        fmDir = line.Substring(1, line.IndexOf(".yaml", StringComparison.InvariantCulture) - 1);
        //    }
        //    else
        //    {
        //        fmDir = line.Substring(0, line.IndexOf(".yaml", StringComparison.InvariantCulture));
        //    }

        //    if (line.Contains("Text files are different"))
        //    {
        //        Process.Start(Path.Combine(Paths.LocalDataPath, fmDir + ".yaml"));
        //        Process.Start(@"explorer.exe",
        //            "\"" + Path.Combine(@"F:\FM packs\FM pack\All\ExtractPermanent", fmDir) + "\"");
        //    }
        //    Trace.WriteLine("");
        //}
    }
}
