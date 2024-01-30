using System;
using System.Collections.Generic;
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

    private async Task LoadUpdateInfo()
    {
        bool success;
        List<CheckUpdates.UpdateInfo> updateInfos;
        try
        {
            Cursor = Cursors.WaitCursor;
            (success, updateInfos) = await CheckUpdates.GetUpdateDetails();
        }
        finally
        {
            Cursor = Cursors.Default;
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
        }
        else
        {
            // @Update: If we couldn't access the internet, we need to say something different than if it's some other error
            Core.Dialogs.ShowAlert("Update error description goes here", "Update");
        }
    }
}
