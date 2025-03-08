/*
Unmodified, as-is local testing app. There's a bunch of hardcoded paths that won't work on your machine, so you'll
have to modify them. Just adding it for completeness.

Public domain/CC0. It's just a test frontend.
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using FMScanner;
using static FMInfoGen.Misc;

namespace FMInfoGen;

internal sealed partial class MainForm : Form
{
    internal MainForm() => InitializeComponent();

    #region Interface

    #region Get

    internal string GetTempPath() => (string)Invoke(() => TempPathTextBox.Text);

    internal IEnumerable<string> GetFMArchives() => (IEnumerable<string>)Invoke(() => FMsListBox.Items.OfType<string>());

    internal string GetSelectedFM() => (string)Invoke(() => FMsListBox.SelectedIndex == -1 ? "" : FMsListBox.SelectedItem.ToString());

    internal ScanOptions GetSelectedScanOptions() => (ScanOptions)Invoke(() => new ScanOptions
    {
        ScanTitle = ScanTitleCheckBox.Checked,
        ScanAuthor = ScanAuthorCheckBox.Checked,
        ScanGameType = ScanGameTypeCheckBox.Checked,
        ScanCustomResources = ScanCustomResourcesCheckBox.Checked,
        ScanSize = ScanSizeCheckBox.Checked,
        ScanReleaseDate = ScanReleaseDateCheckBox.Checked,
        ScanTags = ScanTagsCheckBox.Checked,
        ScanMissionCount = ScanMissionCountCheckBox.Checked,
    });

    internal bool GetOverwriteFoldersChecked() => (bool)Invoke(() => OverwriteFoldersCheckBox.Checked);

    #endregion

    #region Set

    internal void SetFMArchives(IEnumerable<string> items) => Invoke(() => SetListBoxItems(FMsListBox, items));

    internal void SetTempPath(string text) => Invoke(() => TempPathTextBox.Text = text);

    internal void SetExtractedPathList(IEnumerable<string> items) => Invoke(() => SetListBoxItems(ExtractedPathListBox, items));

    internal void SetFMScanProgressBarValue(int value) => Invoke(() => FMScanProgressBar.SetValueClamped(value));

    internal void SetExtractProgressBarValue(int value) => Invoke(() => ExtractProgressBar.SetValueClamped(value));

    internal void SetDebugLabelText(string text) => Invoke(() => DebugLabel.Text = text);

    #endregion

    #region Begin/end modes

    internal void BeginExtractArchiveMode() => Invoke(() =>
    {
        ExtractProgressBar.Show();
        CancelExtractAllButton.Show();
        SetUnsafeControlsEnabled(false);
    });

    internal void EndExtractArchiveMode() => Invoke(() =>
    {
        CancelExtractAllButton.Hide();
        ExtractProgressBar.Hide();
        ExtractProgressBar.Value = 0;
        SetUnsafeControlsEnabled(true);
    });

    internal void BeginScanMode() => Invoke(() =>
    {
        SetUnsafeControlsEnabled(false);
        MainTabPage.Focus();
    });

    internal void EndScanMode() => Invoke(() => SetUnsafeControlsEnabled(true));

    #endregion

    #endregion

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
    }

    private static void SetListBoxItems(ListBox listBox, IEnumerable<string> items)
    {
        listBox.Items.Clear();
        listBox.ColumnWidth = 0;

        try
        {
            listBox.BeginUpdate();

            foreach (string item in items)
            {
                listBox.Items.Add(item);
            }
        }
        finally
        {
            listBox.EndUpdate();
            listBox.AutoSizeColumns();
        }
    }

    #endregion

    #region Event handlers

    private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
    {
        Core.Shutdown();
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
        const string item = @"J:\FM packs\SS2 FM Pack\ss2 fms\Extracted\Blind Disposition v1.3";
        FMScan.ScanFM(item, false, Path.Combine(Paths.LocalTestPath, "localdata-ss2-folder-ver"));
    }

    #endregion

    #endregion
}
