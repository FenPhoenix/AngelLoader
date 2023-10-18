/*
@TDM(Downloader): Handle every possible failure you can think of in here.

@TDM(Downloader): Presumably ids can change out from under us
Given that id order seems to match alphabetical order of FMs, presumably the ids change whenever a new FM is
added to the server list.
While unlikely, we'll need to verify the FM we're about to download is the one we actually wanted (check
internal name). If it doesn't match, we'll reload the list, re-match the ids and so on and try again.

@TDM(Downloader): We need to make sure all asynchronous server loads are cancelable and play nice with form closing etc.
And like if we're loading a screenshot on the details page and then we click Back and select another mission before
the screenshot is done, etc. etc. argh

@TDM(Downloader): Disallow download when game is running
*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using AL_Common;
using static AL_Common.Common;
using static AngelLoader.GameSupport;
using static AngelLoader.Global;
using static AngelLoader.Misc;

namespace AngelLoader.Forms;

public sealed partial class TDMDownloadForm : DarkFormBase
{
    private List<TDM_ServerFMData> _serverFMDataList = new();

    private CancellationTokenSource _serverFMDataCTS = new();
    private CancellationTokenSource _serverFMDetailsCTS = new();
    private CancellationTokenSource _screenshotCTS = new();

    private readonly TDM_Download_Main MainPage;
    private readonly TDM_Download_Details DetailsPage;

    private enum Page
    {
        Main,
        Details
    }

    public TDMDownloadForm()
    {
#if DEBUG
        InitializeComponent();
#else
        InitSlim();
#endif

        MainPage = new TDM_Download_Main { Visible = false };
        DetailsPage = new TDM_Download_Details { Visible = false };

        for (int i = 0; i < 2; i++)
        {
            UserControl page = i == 0 ? MainPage : DetailsPage;

            page.Location = Point.Empty;
            page.Size = new Size(ClientSize.Width, ClientSize.Height - 32);
            page.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom;
            Controls.Add(page);
        }

        MainPage.Show();

        if (Config.DarkMode)
        {
            SetThemeBase(Config.VisualTheme);
        }

        Localize();

        ShowMissionInfo(false);

        MainPage.ProgressLabel.Text = "";

        MainPage.ServerListBox.SelectedIndexChanged += ServerListBox_SelectedIndexChanged;
        MainPage.SelectForDownloadButton.Click += SelectForDownloadButton_Click;
        MainPage.DownloadButton.Click += DownloadButton_Click;
    }

    private void Localize()
    {
        // implement
        // @TDM: Add localized strings
        CloseButton.Text = "Close";
        MainPage.DownloadButton.Text = "Download";
        MoreDetailsButton.Text = "More...";
    }

    // @TDM(Download): Localizable text in here
    private void SwapPage()
    {
        if (MainPage.Visible)
        {
            MoreDetailsButton.Text = "Back";
            DetailsPage.Show();
            MainPage.Hide();
        }
        else
        {
            MainPage.Show();
            DetailsPage.Hide();
            MoreDetailsButton.Text = "More...";
        }
    }

    private void ShowMissionInfo(bool visible)
    {
        MainPage.MissionBasicInfoKeysLabel.Visible = visible;
        MainPage.MissionBasicInfoValuesLabel.Visible = visible;
        MoreDetailsButton.Visible = visible;
    }

    private void TDMDownloadForm_Load(object sender, EventArgs e)
    {

    }

    private async void TDMDownloadForm_Shown(object sender, EventArgs e)
    {
        _serverFMDataCTS = _serverFMDataCTS.Recreate();
        (bool success, _, _, _serverFMDataList) = await TDM_Downloader.TryGetMissionsFromServer(_serverFMDataCTS.Token);
        if (success)
        {
            var comparer = Comparers.TDMServerFMTitle;
            comparer.SortDirection = SortDirection.Ascending;

            _serverFMDataList.Sort(comparer);

            try
            {
                MainPage.ServerListBox.BeginUpdate();
                foreach (TDM_ServerFMData item in _serverFMDataList)
                {
                    MainPage.ServerListBox.AddFullItem(item.Id, item.Title);
                }
            }
            finally
            {
                MainPage.ServerListBox.EndUpdate();
            }
        }
        else
        {
            // @TDM: Put this on an error label or whatever
            Trace.WriteLine("Unable to fetch missions list from the server");
        }
    }

    private void SelectForDownloadButton_Click(object sender, EventArgs e)
    {
        ListView.SelectedIndexCollection selectedIndices = MainPage.ServerListBox.SelectedIndices;
        string[] items = MainPage.ServerListBox.ItemsAsStrings;

        using (new UpdateRegion(MainPage.DownloadListBox))
        {
            foreach (int index in selectedIndices)
            {
                if (!MainPage.DownloadListBox.BackingItems.Contains(MainPage.ServerListBox.BackingItems[index]))
                {
                    MainPage.DownloadListBox.AddFullItem(MainPage.ServerListBox.BackingItems[index], items[index]);
                }
            }
        }
    }

    private async void DownloadButton_Click(object sender, EventArgs e)
    {
        Dictionary<string, TDM_ServerFMData> serverFMDataDict;
        try
        {
            serverFMDataDict = _serverFMDataList.ToDictionary(static x => x.Id, static x => x);
        }
        catch (ArgumentException ex)
        {
            Trace.WriteLine("Duplicate ids?!");
            Core.Dialogs.ShowAlert(
                "There were duplicate ids in the server list. That's not supposed to happen?!",
                LText.AlertMessages.Alert);
            return;
        }

        Random random = new();

        byte[] buffer = new byte[FileStreamBufferSize];

        int downloadsCount = MainPage.DownloadListBox.Items.Count;
        for (int i = 0; i < downloadsCount; i++)
        {
            string id = MainPage.DownloadListBox.BackingItems[i];
            if (serverFMDataDict.TryGetValue(id, out TDM_ServerFMData data))
            {
                _serverFMDetailsCTS = _serverFMDetailsCTS.Recreate();
                var fmDetailsResult = await TDM_Downloader.GetMissionDetails(data, _serverFMDetailsCTS.Token);
                if (fmDetailsResult.Success)
                {
                    bool checksumsValid = CheckSumsValid(fmDetailsResult.ServerFMDetails.DownloadLocations);

                    Trace.WriteLine("Checksums valid: " + checksumsValid);

                    if (!checksumsValid) continue;

                    Trace.WriteLine("Original download location order:");
                    foreach (TDM_FMDownloadLocation item in fmDetailsResult.ServerFMDetails.DownloadLocations)
                    {
                        Trace.WriteLine(item.Url);
                    }

                    // @TDM: We probably want to make a copy of this and leave the original as-is
                    ShuffleUrlsByWeights(fmDetailsResult.ServerFMDetails.DownloadLocations, random);

                    Trace.WriteLine("Shuffled-by-weight download location order:");
                    foreach (TDM_FMDownloadLocation item in fmDetailsResult.ServerFMDetails.DownloadLocations)
                    {
                        Trace.WriteLine(item.Url);
                    }

                    try
                    {
                        await DownloadMission(
                            listIndex: i,
                            fmDetailsResult.ServerFMDetails.InternalName,
                            fmDetailsResult.ServerFMDetails.DownloadLocations[0].Url,
                            buffer);

                        // @TDM: This doesn't work for some reason
                        //MainPage.DownloadListBox.RemoveFullItemAtIndex(0);
                        //i--;

                        MainPage.ProgressLabel.Text = "";
                    }
                    catch (Exception ex)
                    {
                        Core.Dialogs.ShowAlert("Download failed.", "Download", MBoxIcon.Error);
                    }
                }
                else
                {
                    Trace.WriteLine("Failed to download " + MainPage.DownloadListBox.Items[i] + " (id " + id + ")");
                }
            }
        }
        Core.Dialogs.ShowAlert("Done!", "Download", MBoxIcon.Information);
    }

    private sealed class DownloadProgress
    {
        public string TitleText = "";
        public int ListIndex;
        public float Percent;
    }

    private static async Task StreamCopyToAsync_Progress(
        string titleText,
        int listIndex,
        Stream source,
        long sourceLength,
        Stream destination,
        byte[] buffer,
        CancellationToken cancellationToken,
        IProgress<DownloadProgress> progress)
    {
        var progressReport = new DownloadProgress { TitleText = titleText, ListIndex = listIndex };

        int bytesRead;
        long totalBytesRead = 0;
        float previousPercent = 0;
        while ((bytesRead = await source.ReadAsync(buffer, 0, buffer.Length, cancellationToken).ConfigureAwait(false)) != 0)
        {
            await destination.WriteAsync(buffer, 0, bytesRead, cancellationToken).ConfigureAwait(false);

            totalBytesRead += bytesRead;

            float percent = (float)Math.Round(GetPercentFromValue_Float(totalBytesRead, sourceLength), 1);

            // Only report if the percent has changed by an amount that can be displayed, to keep report frequency
            // from going crazy.
            if (Math.Abs(percent - previousPercent) > 0.1)
            {
                previousPercent = percent;
                progressReport.Percent = percent;
                progress.Report(progressReport);
            }
        }
        progressReport.Percent = 100f;
        progress.Report(progressReport);
    }

    // @TDM(DownloadMission): Robustify this
    private async Task DownloadMission(int listIndex, string internalName, string url, byte[] buffer)
    {
        var progress = new Progress<DownloadProgress>(ReportProgress);

        string tempPath = Paths.TDMDownloadTemp;
        Paths.CreateOrClearTempPath(tempPath);
        string fileName = internalName + ".pk4";
        string filePathInTemp = Path.Combine(tempPath, fileName);

        var request = await GlobalHttpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);

        using (Stream stream = await GlobalHttpClient.GetStreamAsync(url))
        using (FileStream fs = File.Create(filePathInTemp))
        {
            await StreamCopyToAsync_Progress(
                MainPage.DownloadListBox.Items[listIndex].Text,
                listIndex,
                stream,
                request.Content.Headers.ContentLength ?? 0,
                fs,
                buffer,
                CancellationToken.None,
                progress
            );
        }

        string finalPath = Path.Combine(Config.GetFMInstallPath(GameIndex.TDM), fileName);
        File.Copy(filePathInTemp, finalPath, overwrite: true);
        File.Delete(filePathInTemp);

        return;

        void ReportProgress(DownloadProgress pr)
        {
            // @TDM(ListView text length):
            // "The text of the ListViewItem should not exceed 259 characters or unexpected behavior could occur."
            // ARGH. This thing sucks. We need something altogether better...
            // Could we maybe really truly shoehorn the downloader into the main list? That'd be glorious.
            MainPage.DownloadListBox.Items[pr.ListIndex].Text =
                pr.TitleText + " " +
                pr.Percent.ToString("F1", CultureInfo.CurrentCulture) + "%";
        }
    }

    private static bool CheckSumsValid<T>(List<T> urls) where T : IChecksum
    {
        int urlsCount = urls.Count;
        if (urlsCount == 0) return true;

        for (int i = 0; i < urlsCount; i++)
        {
            string sha256 = urls[i].SHA256;

            foreach (char c in sha256)
            {
                if (!c.IsAsciiNumeric() && !c.IsAsciiLower())
                {
                    return false;
                }
            }

            if (sha256 != urls[0].SHA256)
            {
                return false;
            }
        }

        return true;
    }

    private static void ShuffleUrlsByWeights<T>(List<T> urls, Random random) where T : IWeighted
    {
        /*
        This is the algo the game uses to choose a download location.
        Let's match it to prevent any problems.
        
        Dark Mod source code reference:
        MissionManager.cpp
        Search "//stgatilov #5349: shuffle the URL lists according to weights"
        */
        int urlsCount = urls.Count;
        for (int i = 0; i < urlsCount; i++)
        {
            double sum = 0.0d;
            for (int j = i; j < urlsCount; j++)
            {
                sum += urls[j].Weight;
            }
            double value = random.NextDouble() * sum;
            int choice = i;
            for (int j = i; j < urlsCount; j++)
            {
                value -= urls[j].Weight;
                if (value < 0.0d)
                {
                    choice = j;
                    break;
                }
            }
            (urls[choice], urls[i]) = (urls[i], urls[choice]);
        }
    }

    private void ServerListBox_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (MainPage.ServerListBox.SelectedIndex == -1)
        {
            ShowMissionInfo(false);
        }
        else
        {
            TDM_ServerFMData item = _serverFMDataList[MainPage.ServerListBox.SelectedIndex];

            // @TDM(Download): Localizable text here
            MainPage.MissionBasicInfoKeysLabel.Text =
                "Title: " + "\r\n" +
                "Author: " + "\r\n" +
                "Release date: " + "\r\n" +
                "Size: ";

            MainPage.MissionBasicInfoValuesLabel.Text =
                item.Title + "\r\n" +
                item.Author + "\r\n" +
                item.ReleaseDate + "\r\n" +
                item.Size + " " + LText.Global.MegabyteShort;

            MainPage.MissionBasicInfoValuesLabel.Location = MainPage.MissionBasicInfoValuesLabel.Location with
            {
                X = MainPage.MissionBasicInfoKeysLabel.Right + 16
            };

            ShowMissionInfo(true);
        }
    }

    /*
    @TDM(Screenshot display):
    Screenshots are cached locally in fms\_missionshots with the name previewshot_[screenshotname]
    So for example, for "screenshots/0_ac1_0.jpg" it would be "previewshot_0_ac1_0.jpg"
    We should use the cached version if it exists, to save time and bandwidth (the game does this).
    @TDM(More details): We could use Settings-like swapping of panels
    Click "More..." and it swaps to the details panel, and it has a "Back" button that swaps back
    */
    private void MoreDetailsButton_Click(object sender, EventArgs e)
    {
        SwapPage();
    }
}
