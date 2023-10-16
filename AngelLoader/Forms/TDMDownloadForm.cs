﻿/*
@TDM(Downloader): Handle every possible failure you can think of in here.

@TDM(Downloader): Presumably ids can change out from under us
Given that id order seems to match alphabetical order of FMs, presumably the ids change whenever a new FM is
added to the server list.
While unlikely, we'll need to verify the FM we're about to download is the one we actually wanted (check
internal name). If it doesn't match, we'll reload the list, re-match the ids and so on and try again.
*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using AL_Common;
using static AngelLoader.Global;

namespace AngelLoader.Forms;

public sealed partial class TDMDownloadForm : DarkFormBase
{
    private List<TDM_ServerFMData> _serverFMDataList = new();

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
        (bool success, _, _serverFMDataList) = await TDM_Downloader.TryGetMissionsFromServer();
        if (success)
        {
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
    }

    private void SelectForDownloadButton_Click(object sender, EventArgs e)
    {
        ListView.SelectedIndexCollection selectedIndices = MainPage.ServerListBox.SelectedIndices;
        string[] items = MainPage.ServerListBox.ItemsAsStrings;

        using (new UpdateRegion(MainPage.ServerListBox))
        {
            foreach (int index in selectedIndices)
            {
                MainPage.DownloadListBox.AddFullItem(MainPage.ServerListBox.BackingItems[index], items[index]);
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

        int downloadsCount = MainPage.DownloadListBox.Items.Count;
        for (int i = 0; i < downloadsCount; i++)
        {
            string id = MainPage.DownloadListBox.BackingItems[i];
            if (serverFMDataDict.TryGetValue(id, out TDM_ServerFMData data))
            {
                var fmDetailsResult = await TDM_Downloader.GetMissionDetails(data);
                if (fmDetailsResult.Success)
                {
                    foreach (TDM_FMDownloadLocation item in fmDetailsResult.ServerFMDetails.DownloadLocations)
                    {
                        /*
                        @TDM(Download): We should match the TDM code here
                        Match the weighting to decide which one to use, and also whatever is done with the sha256
                        and language and whatever else.
                        */
                        Trace.WriteLine(item.Url);
                    }
                }
            }
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
