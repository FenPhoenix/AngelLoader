using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using AL_Common;
using static AngelLoader.Global;

namespace AngelLoader.Forms;

public sealed partial class ImportFromDarkLoaderForm : DarkFormBase
{
    internal string DarkLoaderIniFile = "";
    internal bool ImportFMData;
    internal bool ImportTitle;
    internal bool ImportSize;
    internal bool ImportComment;
    internal bool ImportReleaseDate;
    internal bool ImportLastPlayed;
    internal bool ImportFinishedOn;

    internal bool ImportSaves;

    internal bool BackupPathSetRequested;

    internal ImportFromDarkLoaderForm()
    {
#if DEBUG
        InitializeComponent();
#else
        InitSlim();
#endif

        if (Config.DarkMode) SetThemeBase(Config.VisualTheme);

        if (Directory.Exists(Config.FMsBackupPath))
        {
            Size = new Size(Size.Width, Size.Height - 40);
            BackupPathRequiredLabel.Hide();
            SetBackupPathLinkLabel.Hide();
        }
        else
        {
            ImportSavesCheckBox.Checked = false;
            ImportSavesCheckBox.Enabled = false;
        }
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        Localize();
    }

    private void Localize()
    {
        Text = LText.Importing.ImportFromDarkLoader_TitleText;

        ImportControls.Localize();

        ImportFMDataCheckBox.Text = LText.Importing.DarkLoader_ImportFMData;
        ImportSavesCheckBox.Text = LText.Importing.DarkLoader_ImportSaves;

        ImportTitleCheckBox.Text = LText.Importing.ImportData_Title;
        ImportReleaseDateCheckBox.Text = LText.Importing.ImportData_ReleaseDate;
        ImportLastPlayedCheckBox.Text = LText.Importing.ImportData_LastPlayed;
        ImportFinishedOnCheckBox.Text = LText.Importing.ImportData_Finished;
        ImportCommentCheckBox.Text = LText.Importing.ImportData_Comment;
        ImportSizeCheckBox.Text = LText.Importing.ImportData_Size;

        OKButton.Text = LText.Global.OK;
        Cancel_Button.Text = LText.Global.Cancel;

        BackupPathRequiredLabel.Text = LText.Importing.DarkLoader_BackupPathRequired;
        SetBackupPathLinkLabel.Text = LText.Importing.DarkLoader_SetBackupPath;

        int backupControlsWidth = Math.Max(BackupPathRequiredLabel.Right, SetBackupPathLinkLabel.Right);
        if (backupControlsWidth > ClientSize.Width)
        {
            int extra = backupControlsWidth - ClientSize.Width;
            Width += extra;
        }
    }

    private void ImportFMDataCheckBox_CheckedChanged(object sender, EventArgs e)
    {
        bool value = ImportFMDataCheckBox.Checked;
        ImportTitleCheckBox.Enabled = value;
        ImportSizeCheckBox.Enabled = value;
        ImportCommentCheckBox.Enabled = value;
        ImportReleaseDateCheckBox.Enabled = value;
        ImportLastPlayedCheckBox.Enabled = value;
        ImportFinishedOnCheckBox.Enabled = value;
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        base.OnFormClosing(e);

        if (BackupPathSetRequested) SuppressRefreshOnClose = true;

        if (DialogResult != DialogResult.OK) return;

        string file = ImportControls.DarkLoaderIniText;

        bool fileNameIsDLIni;
        try
        {
            fileNameIsDLIni = Path.GetFileName(file).EqualsI(Paths.DarkLoaderIni);
        }
        catch (ArgumentException)
        {
            Core.Dialogs.ShowAlert(LText.Importing.SelectedFileIsNotAValidPath, LText.AlertMessages.Alert);
            e.Cancel = true;
            return;
        }

        if (!fileNameIsDLIni)
        {
            Core.Dialogs.ShowAlert(LText.Importing.DarkLoader_SelectedFileIsNotDarkLoaderIni, LText.AlertMessages.Alert);
            e.Cancel = true;
            return;
        }

        bool iniFileExists = File.Exists(file);
        if (!iniFileExists)
        {
            Core.Dialogs.ShowAlert(LText.Importing.DarkLoader_SelectedDarkLoaderIniWasNotFound, LText.AlertMessages.Alert);
            e.Cancel = true;
            return;
        }

        DarkLoaderIniFile = file;
        ImportTitle = ImportTitleCheckBox.Checked;
        ImportSize = ImportSizeCheckBox.Checked;
        ImportComment = ImportCommentCheckBox.Checked;
        ImportReleaseDate = ImportReleaseDateCheckBox.Checked;
        ImportLastPlayed = ImportLastPlayedCheckBox.Checked;
        ImportFinishedOn = ImportFinishedOnCheckBox.Checked;

        ImportFMData = ImportFMDataCheckBox.Checked &&
                       (ImportTitle || ImportSize || ImportComment || ImportReleaseDate ||
                        ImportLastPlayed || ImportFinishedOn);

        ImportSaves = ImportSavesCheckBox.Checked;
    }

    #region Research notes

    /* DarkLoader:

    Saves:
    -When you uninstall a mission, it puts the saves in [GameExePath]\allsaves
        ex. C:\Thief2\allsaves
    -Saves are put into a zip, in its base directory (no "saves" folder within)
    -The zip is named [archive]_saves.zip
        ex. 2002-02-19_Justforshow_saves.zip
    -This name is NOT run through the badchar remover, but it does appear to have whitespace trimmed off both
     ends.

    ---

    Non-FM headers:
    [options]
    [window]
    [mission directories]
    [Thief 1]
    [Thief 2]
    [Thief2x]
    [SShock 2]
    (or anything that doesn't have a .number at the end)
     
    FM headers look like this:
    
    [1999-06-11_DeceptiveSceptre,The.538256]
    
    First an opening bracket ('[') then the archive name minus the extension (which is always '.zip'), in
    full (not truncated), with the following characters removed:
    ], Chr(9) (TAB), Chr(10) (LF), Chr(13) (CR)
     
    or, put another way:
    badchars=[']',#9,#10,#13];

    Then comes a dot (.) followed by the size, in bytes, of the archive, then a closing bracket (']').

    FM key-value pairs:
    type=(int)
    
    Will be one of these values (numeric, not named):
    darkGameUnknown = 0; <- if it hasn't been scanned, it will be this
    darkGameThief   = 1;
    darkGameThief2  = 2;
    darkGameT2x     = 3;
    darkGameSS2     = 4;

    but we should ignore this and scan for the type ourselves, because:
    a) DarkLoader gets the type wrong with NewDark (marks Thief1 as Thief2), and
    b) we don't want to pollute our own list with archive types we don't support (T2X? we support SS2 now)

    comment=(string)
    Looks like this:
    comment="This is a comment"
    The string is always surrounded with double-quotes (").
    Escapes are handled like this:
    #9  -> \t
    #10 -> \n
    #13 -> \r
    "   -> \"
    \   -> \\

    title=(string)
    Handled the exact same way as comment= above.

    misdate=(int)
    Mission release date in number of days since December 30, 1899.

    date=(int)
    Last played date in number of days since December 30, 1899.

    finished=(int)
    A 4-bit flags field, exactly the same as AngelLoader uses, so no conversion needed at all.
    */

    #endregion

    private void SetBackupPathLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
        BackupPathSetRequested = true;
        DialogResult = DialogResult.Cancel;
        Close();
    }
}
