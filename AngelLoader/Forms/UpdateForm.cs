using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using AngelLoader.DataClasses;
using static AngelLoader.Global;

namespace AngelLoader.Forms;

public sealed partial class UpdateForm : DarkFormBase, IWaitCursorSettable, IDarkContextMenuOwner
{
    private readonly AutoResetEvent _downloadARE = new(false);

    private bool _downloadingUpdateInfo;

    internal AppUpdate.UpdateInfo? UpdateInfo;
    internal bool NoUpdatesFound;

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

    private void SetText(string text) => ReleaseNotesRichTextBox.SetText(text);

    private void SetReleaseNotes(Stream stream) => ReleaseNotesRichTextBox.LoadControlledRtf(stream);

    protected override async void OnShown(EventArgs e)
    {
        base.OnShown(e);
        Cancel_Button.Focus();
        try
        {
            await LoadUpdateInfo();
        }
        catch
        {
            // Weird control-access-after-disposed thing even though we should be synchronous for that...
        }
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        if (_downloadingUpdateInfo)
        {
            try
            {
                AppUpdate.CancelDetailsDownload();
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
        SetText(LText.Update.DownloadingUpdateInfo);

        AppUpdate.UpdateDetailsDownloadResult result;
        List<AppUpdate.UpdateInfo> updateInfos;
        try
        {
            _downloadingUpdateInfo = true;
            try
            {
                (result, updateInfos) = await AppUpdate.GetUpdateDetails(_downloadARE);
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

        if (result == AppUpdate.UpdateDetailsDownloadResult.Success && updateInfos.Count > 0)
        {
#if false
            updateInfos.Add(new AppUpdate.UpdateInfo(new Version(1, 0), "This is test text!", new Uri("https://www.google.com")));
#endif

            UpdateInfo = updateInfos[0];

            string changelogFullText =
                @"{\rtf1" +
                @"\ansi\ansicpg1252" +
                @"\deff0{\fonttbl{\f0\fnull\fcharset0 Segoe UI;}}";
            for (int i = 0; i < updateInfos.Count; i++)
            {
                if (i > 0) changelogFullText += @"\line\line ---\line\line ";
                AppUpdate.UpdateInfo? item = updateInfos[i];
                changelogFullText += @"\b1 " + item.Version + @":\b0\line\line " +
                                     ChangelogBodyToRtf(item.ChangelogText);
            }
            changelogFullText += "}";

            using (var ms = new MemoryStream(Encoding.ASCII.GetBytes(changelogFullText)))
            {
                SetReleaseNotes(ms);
            }

            UpdateButton.Enabled = true;
        }
        else if (result == AppUpdate.UpdateDetailsDownloadResult.NoUpdatesFound)
        {
            SetText(LText.Update.NoUpdatesAvailable);
            UpdateButton.Enabled = false;
            NoUpdatesFound = true;
        }
        else
        {
            SetText(LText.Update.FailedToDownloadUpdateInfo);
            UpdateButton.Enabled = false;
        }
    }

    // In with the UI code because RTF is UI-specific
    private static string ChangelogBodyToRtf(string text)
    {
        string[] lines = AppUpdate.GetFormattedPlainTextReleaseNotesLines(text);

        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i];

            lines[i] = line
                .Replace(@"\", @"\\")
                .Replace("{", @"\{")
                .Replace("}", @"\}");

            if (Regex.Match(line.TrimEnd(), ":$").Success)
            {
                lines[i] = @"\b1 " + line + @"\b0 ";
            }
        }

        text = string.Join(@"\line ", lines);

        StringBuilder sb = new();

        for (int i = 0; i < text.Length; i++)
        {
            char c = text[i];
            // 2-char codepoints are just 2 consecutive \uN keywords, so this is correct
            if (c > 127)
            {
                sb.Append(@"\u");
                sb.Append((int)c);
                sb.Append('?');
            }
            else
            {
                sb.Append(c);
            }
        }

        return sb.ToString();
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

    public bool ViewBlocked => false;

    public IContainer GetComponents() => components ??= new Container();
}
