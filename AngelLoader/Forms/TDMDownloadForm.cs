/*
@TDM(Downloader): Handle every possible failure you can think of in here.

@TDM(Downloader): Presumably ids can change out from under us
Given that id order seems to match alphabetical order of FMs, presumably the ids change whenever a new FM is
added to the server list.
While unlikely, we'll need to verify the FM we're about to download is the one we actually wanted (check
internal name). If it doesn't match, we'll reload the list, re-match the ids and so on and try again.
*/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.Remoting.MetadataServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AL_Common;
using static AngelLoader.Global;

namespace AngelLoader.Forms;

public sealed partial class TDMDownloadForm : DarkFormBase
{
    private List<TDM_ServerFMData> _serverFMDataList = new();

    public TDMDownloadForm()
    {
#if DEBUG
        InitializeComponent();
#else
        InitSlim();
#endif

        SetThemeBase(Config.VisualTheme);

        Localize();

        ShowMissionInfo(false);
    }

    private void Localize()
    {
        // implement
        // @TDM: Add localized strings
        CloseButton.Text = "Close";
        DownloadButton.Text = "Download";
        MoreDetailsButton.Text = "More...";
    }

    private void ShowMissionInfo(bool visible)
    {
        MissionBasicInfoKeysLabel.Visible = visible;
        MissionBasicInfoValuesLabel.Visible = visible;
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
                ServerListBox.BeginUpdate();
                foreach (TDM_ServerFMData item in _serverFMDataList)
                {
                    ServerListBox.AddFullItem(item.Id, item.Title);
                }
            }
            finally
            {
                ServerListBox.EndUpdate();
            }
        }
    }

    private void SelectForDownloadButton_Click(object sender, EventArgs e)
    {
        ListView.SelectedIndexCollection selectedIndices = ServerListBox.SelectedIndices;
        string[] items = ServerListBox.ItemsAsStrings;

        using (new UpdateRegion(ServerListBox))
        {
            foreach (int index in selectedIndices)
            {
                DownloadListBox.AddFullItem(ServerListBox.BackingItems[index], items[index]);
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

        int downloadsCount = DownloadListBox.Items.Count;
        for (int i = 0; i < downloadsCount; i++)
        {
            string id = DownloadListBox.BackingItems[i];
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
        if (ServerListBox.SelectedIndex == -1)
        {
            ShowMissionInfo(false);
        }
        else
        {
            TDM_ServerFMData item = _serverFMDataList[ServerListBox.SelectedIndex];

            // @TDM(Download): Localizable text here
            MissionBasicInfoKeysLabel.Text =
                "Title: " + "\r\n" +
                "Author: " + "\r\n" +
                "Release date: " + "\r\n" +
                "Size: ";

            MissionBasicInfoValuesLabel.Text =
                item.Title + "\r\n" +
                item.Author + "\r\n" +
                item.ReleaseDate + "\r\n" +
                item.Size + " " + LText.Global.MegabyteShort;

            MissionBasicInfoValuesLabel.Location = MissionBasicInfoValuesLabel.Location with
            {
                X = MissionBasicInfoKeysLabel.Right + 16
            };

            ShowMissionInfo(true);
        }
    }
}
