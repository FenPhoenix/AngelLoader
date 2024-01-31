using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using AngelLoader.DataClasses;
using static AngelLoader.Global;

namespace AngelLoader.Forms;

public sealed partial class UpdateForm : DarkFormBase, IWaitCursorSettable
{
    private readonly AutoResetEvent _downloadARE = new(false);

    private bool _downloadingUpdateInfo;

    internal CheckUpdates.UpdateInfo? UpdateInfo;

    public UpdateForm()
    {
#if DEBUG
        InitializeComponent();
#else
        InitSlim();
#endif

        ReleaseNotesRichTextBox.SetOwner(this);

        UpdateButton.Enabled = false;

        if (Config.DarkMode) SetThemeBase(Config.VisualTheme);

        Localize();
    }

    public override void RespondToSystemThemeChange() => SetThemeBase(Config.VisualTheme);

    private void Localize()
    {
        Text = LText.Update.UpdateDialog_Title;

        ReleaseNotesRichTextBox.Localize();

        UpdateButton.Text = LText.Update.UpdateDialog_UpdateAndRestartButtonText;
        Cancel_Button.Text = LText.Global.Cancel;
    }

    private void SetReleaseNotes(string releaseNotes)
    {
        // @Update: Implement headings/bulleted lists etc.
        ReleaseNotesRichTextBox.SetText(releaseNotes);
    }

    protected override async void OnShown(EventArgs e)
    {
        base.OnShown(e);
        Cancel_Button.Focus();
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

    protected override void OnKeyDown(KeyEventArgs e)
    {
        switch (e.KeyCode)
        {
            case Keys.Escape:
                Close();
                break;
            case Keys.Add or Keys.Oemplus:
                ReleaseNotesRichTextBox.Zoom(Zoom.In);
                break;
            case Keys.Subtract or Keys.OemMinus:
                ReleaseNotesRichTextBox.Zoom(Zoom.Out);
                break;
            case Keys.D0 or Keys.NumPad0:
                ReleaseNotesRichTextBox.Zoom(Zoom.Reset);
                break;
        }

        base.OnKeyDown(e);
    }

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

    public void SetWaitCursor(bool value) => Cursor = value ? Cursors.WaitCursor : Cursors.Default;

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        var tb = ReleaseNotesRichTextBox;

        e.Graphics.DrawRectangle(
            pen: Config.DarkMode ? DarkColors.GreySelectionPen : SystemPens.ControlLight,
            rect: new Rectangle(tb.Left - 2, tb.Top - 2, tb.Width + 3, tb.Height + 3)
        );
    }
}
