using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using AngelLoader.Common.DataClasses;
using AngelLoader.Common.Utility;
using AngelLoader.Importing;
using static AngelLoader.Common.Utility.Methods;

namespace AngelLoader.Forms
{
    public partial class ImportFromMultipleInisForm : Form
    {
        internal List<string> IniFiles;
        private readonly ImportType ImportType;

        public ImportFromMultipleInisForm(ImportType importType)
        {
            ImportType = importType;
            InitializeComponent();
            Localize();

            AutodetectGameIni(Game.Thief1, Thief1IniTextBox);
            AutodetectGameIni(Game.Thief2, Thief2IniTextBox);
            AutodetectGameIni(Game.Thief3, Thief3IniTextBox);
        }

        private void Localize()
        {
            Text = ImportType == ImportType.NewDarkLoader
                ? LText.Importing.ImportFromNewDarkLoader_TitleText
                : LText.Importing.ImportFromFMSel_TitleText;

            ChooseIniFilesLabel.Text = ImportType == ImportType.NewDarkLoader
                ? LText.Importing.ChooseNewDarkLoaderIniFiles
                : LText.Importing.ChooseFMSelIniFiles;

            Thief1GroupBox.Text = LText.Importing.Thief1;
            Thief2GroupBox.Text = LText.Importing.Thief2;
            Thief3GroupBox.Text = LText.Importing.Thief3;

            Thief1AutodetectCheckBox.Text = LText.Global.Autodetect;
            Thief2AutodetectCheckBox.Text = LText.Global.Autodetect;
            Thief3AutodetectCheckBox.Text = LText.Global.Autodetect;

            Thief1IniBrowseButton.SetTextAutoSize(Thief1IniTextBox, LText.Global.BrowseEllipses);
            Thief2IniBrowseButton.SetTextAutoSize(Thief2IniTextBox, LText.Global.BrowseEllipses);
            Thief3IniBrowseButton.SetTextAutoSize(Thief3IniTextBox, LText.Global.BrowseEllipses);

            OKButton.SetTextAutoSize(LText.Global.OK, OKButton.Width);
            Cancel_Button.SetTextAutoSize(LText.Global.Cancel, Cancel_Button.Width);
        }

        private void ImportFromMultipleInisForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (DialogResult != DialogResult.OK) return;

            IniFiles = new List<string>
            {
                Thief1IniTextBox.Text.Trim(),
                Thief2IniTextBox.Text.Trim(),
                Thief3IniTextBox.Text.Trim()
            };
        }

        private void ThiefIniBrowseButtons_Click(object sender, EventArgs e)
        {
            var s = (Button)sender;
            var tb =
                s == Thief1IniBrowseButton ? Thief1IniTextBox :
                s == Thief2IniBrowseButton ? Thief2IniTextBox :
                Thief3IniTextBox;

            using (var d = new OpenFileDialog())
            {
                d.Filter = LText.BrowseDialogs.IniFiles + @"|*.ini|" + LText.BrowseDialogs.AllFiles + @"|*.*";
                if (d.ShowDialog() != DialogResult.OK) return;

                tb.Text = d.FileName;
            }
        }

        private void AutodetectCheckBoxes_CheckedChanged(object sender, EventArgs e)
        {
            var s = (CheckBox)sender;
            var textBox =
                s == Thief1AutodetectCheckBox ? Thief1IniTextBox :
                s == Thief2AutodetectCheckBox ? Thief2IniTextBox :
                Thief3IniTextBox;
            var button =
                s == Thief1AutodetectCheckBox ? Thief1IniBrowseButton :
                s == Thief2AutodetectCheckBox ? Thief2IniBrowseButton :
                Thief3IniBrowseButton;
            var game =
                s == Thief1AutodetectCheckBox ? Game.Thief1 :
                s == Thief2AutodetectCheckBox ? Game.Thief2 :
                Game.Thief3;

            textBox.ReadOnly = s.Checked;
            button.Enabled = !s.Checked;

            if (s.Checked) AutodetectGameIni(game, textBox);
        }

        private void AutodetectGameIni(Game game, TextBox textBox)
        {
            var iniFile = ImportType == ImportType.NewDarkLoader ? "NewDarkLoader.ini" : "fmsel.ini";

            var fmsPath = GetFMInstallsBasePath(game);
            if (fmsPath.IsWhiteSpace())
            {
                textBox.Text = "";
            }
            else
            {
                var iniFileFull = Path.Combine(fmsPath, iniFile);
                textBox.Text = File.Exists(iniFileFull) ? iniFileFull : "";
            }
        }

        #region NDL Research notes

        /* NewDarkLoader:

        -Allow the user to select multiple NDL installs (because one is needed for each game)
        -We can take the ArchiveRoot path (below) and add it to our archive paths

        -For archive dirs, NDL picks up FMs from all subdirectories as well. Just that, nothing fancy going on.

        -Unused keys:
        -------------
        const string kGame_type = "Type";
        const string kArchive_name = "Archive";
        const string kWindowPos = "WindowPos";
        -------------

        Config section:
        ---------------
        const string secOptions = "Config";
        
        -Supported archive extensions. Default zip,7z,rar. Ignore this, we have our own internal list.
        const string kExtensions = "ValidExtensions";

        -Ignore, we don't use this
        const string kUseRelativePaths = "UseRelativePaths";

        -Archive path (will be only one per install)
        const string kArchive_root = "ArchiveRoot";

        const string kLanguage = "Language";

        -int with two possible values
         1 = dd/MM/yyyy
         2 = MM/dd/yyyy
        const string kDate_format = "DateFormat";
        
        -Don't ask for confirmation to play when you double-click an FM
        const string kAlwaysPlay = "DbClDontAsk";

        -The value "Ask" or "Always"
        const string kBackup_type = "BackupType";

        -One of these will be written out. This is "run NDL after game/editor" so we don't need it anyway.
        const string kReturn_type = "DebriefFM";
        const string kReturn_type_ed = "DebriefFMEd";

        -We use internal 7z stuff only, so ignore these
        const string k7zipG = "sevenZipG";
        const string kUse7zNoWin = "Use7zNoWin";

        -This is what goes after google's "site:" bit, it'll be eg. "ttlg.com"
        -UI says enter 0 to disable, check if that gets written out literally to the file
        "WebSearchSite"

        -Space separated - "the a an"
        -UI says enter 0 to disable, check if that gets written out literally to the file
        "ArticleWords"
        "SortIgnoreArticles"

        const string kSplitDist = "SplitDistance";
        const string kCWidths = "ColumnWidths";
        const string kWindowState = "WindowState";

        -Whether the top-right section is expanded or collapsed
        const string kShowTags = "ShowTags";

        const string kSortCol = "SortColumn";
        const string kSortOrder = "SortOrder";
        
        -Last played (not last selected) FM. Ignore it I guess.
        const string kLast_fm = "LastFM";

        -Ignore these, it's easy enough for the user to set them back again
        const string kNameFilter = "FilterName";
        const string kUnfinishedFilter = "FilterUnfinished";
        const string kStartDateFilter = "FilterStart";
        const string kEndDateFilter = "FilterEnd";
        const string kIncTagsFilter = "FilterTagsOR";
        const string kExcTagsFilter = "FilterTagsNOT";
        ---------------

        -FM section:
        ------------
        const string kFm_title = "NiceName";
        
        -Both written out in Unix time hex just like us, so no conversion needed
        -But scan for dates ourselves, and then only replace dates that are invalid (<1999)
        const string kRelease_date = "ReleaseDate";

        -This actually does mean "last played", not "last completed"
        const string kLast_played = "LastCompleted";

        -Same as always, just an int 0-15
        const string kFinished = "Finished";

        -If empty or no key, then None (unrated), otherwise 0-10
        const string kRating = "Rating";

        -Single line. Just read it verbatim.
        const string kComment = "Comment";

        -Disabled mods string. Read it verbatim, but if it's "*" then blank it and set DisableAllMods to true
        const string kNo_mods = "ModExclude";

        -"[none]" means none. Otherwise, a string with the same format as we use.
        const string kTags = "Tags";

        -Selected readme.
        const string kInfoFile = "InfoFile";

        -Size in bytes. Same as our SizeBytes key.
        -Appears to work just like ours; it's the archive size if at all possible, otherwise the folder size.
        const string kSizeBytes = "FMSize";
        ------------
        */

        #endregion
    }
}
