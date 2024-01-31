using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static AngelLoader.Global;

namespace AngelLoader.Forms;

public sealed partial class UpdateForm : DarkFormBase
{
    public UpdateForm()
    {
#if DEBUG
        InitializeComponent();
#else
        InitSlim();
#endif

        UpdateButton.Enabled = false;

        if (Config.DarkMode) SetThemeBase(Config.VisualTheme);

        Localize();
    }

    internal CheckUpdates.UpdateInfo? UpdateInfo;

    public override void RespondToSystemThemeChange() => SetThemeBase(Config.VisualTheme);

    private void Localize()
    {
        Text = LText.Update.UpdateDialog_Title;

        UpdateButton.Text = LText.Update.UpdateDialog_UpdateAndRestartButtonText;
        Cancel_Button.Text = LText.Global.Cancel;
    }

    // @Update: Should we make this a RichTextBox so we can show bold/italic and zoom text and stuff?
    private void SetReleaseNotes(string releaseNotes) => ReleaseNotesTextBox.Text = releaseNotes;

    protected override async void OnShown(EventArgs e)
    {
        base.OnShown(e);
        await LoadUpdateInfo();
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        if (_downloadingUpdateInfo)
        {
            try
            {
                CheckUpdates.CancelDetailsDownload();
                _downloadARE.WaitOne();
            }
            catch
            {
                // ignore
            }
        }

        base.OnFormClosing(e);
    }

    private readonly AutoResetEvent _downloadARE = new(false);

    private bool _downloadingUpdateInfo;

    private async Task LoadUpdateInfo()
    {
        SetReleaseNotes("Downloading update information...");

        bool success;
        List<CheckUpdates.UpdateInfo> updateInfos;
        try
        {
            _downloadingUpdateInfo = true;
            try
            {
                (success, updateInfos) = await CheckUpdates.GetUpdateDetails(_downloadARE);
            }
            catch (OperationCanceledException)
            {
                return;
            }
        }
        finally
        {
            _downloadingUpdateInfo = false;
        }

        if (success && updateInfos.Count > 0)
        {
            UpdateInfo = updateInfos[0];

            // @Update: Test with multiple versions/changelogs
            string changelogFullText = "";
            for (int i = 0; i < updateInfos.Count; i++)
            {
                if (i > 0) changelogFullText += "\r\n\r\n\r\n";
                CheckUpdates.UpdateInfo? item = updateInfos[i];
                changelogFullText += item.Version + ":\r\n" + item.ChangelogText;
            }

            SetReleaseNotes(changelogFullText);

            UpdateButton.Enabled = true;
        }
        else
        {
            SetReleaseNotes("Failed to download update information.");

            // @Update: If we couldn't access the internet, we need to say something different than if it's some other error
            Core.Dialogs.ShowAlert("Update error description goes here", "Update");
        }
    }
}
