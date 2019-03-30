using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using AngelLoader.Common;
using AngelLoader.Common.DataClasses;
using AngelLoader.Common.Utility;
using AngelLoader.Properties;
using AngelLoader.WinAPI.Dialogs;

namespace AngelLoader.Forms
{
    internal sealed partial class SettingsForm : Form, IEventDisabler, ILocalizable
    {
        private readonly ILocalizable OwnerForm;

        private readonly bool Startup;

        private readonly ConfigData InConfig;
        internal readonly ConfigData OutConfig = new ConfigData();

        private readonly TextBox[] GameExePathTextBoxes;

        private enum PathError
        {
            True,
            False
        }

        // August 4 is chosen more-or-less randomly, but both its name and its number are different short vs. long
        // (Aug vs. August; 8 vs. 08), and the same thing with 4 (4 vs. 04).
        private readonly DateTime exampleDate = new DateTime(DateTime.Now.Year, 8, 4);

        public bool EventsDisabled { get; set; }

        #region Constructor / closing

        internal SettingsForm(ILocalizable ownerForm, ConfigData config, bool startup)
        {
            InitializeComponent();

            Startup = startup;

            OwnerForm = ownerForm;

            GameExePathTextBoxes = new[]
            {
                Thief1ExePathTextBox,
                Thief2ExePathTextBox,
                Thief3ExePathTextBox
            };

            InConfig = config;

            if (startup)
            {
                // NOTE: Make sure MainToolTip doesn't have anything set on any controls that are in a tab page
                // that has been removed, or it won't show for other completely unrelated controls that are in
                // the visible tab page (?!)

                Text = LText.SettingsWindow.StartupTitleText;
                // _Load is too late for some of this stuff, so might as well put everything here
                StartPosition = FormStartPosition.CenterScreen;
                ShowInTaskbar = true;
                MainTabControl.TabPages.Remove(FMDisplayTabPage);
                MainTabControl.TabPages.Remove(OtherTabPage);
                PathsTabPage.Controls.Add(LanguageGroupBox);
                LanguageGroupBox.BringToFront();
                var lx = FMArchivePathsGroupBox.Left;
                var ly = FMArchivePathsGroupBox.Top + FMArchivePathsGroupBox.Height + 8;
                LanguageGroupBox.Location = new Point(lx, ly);
                LanguageGroupBox.Width = FMArchivePathsGroupBox.Width;
            }
            else
            {
                Text = LText.SettingsWindow.TitleText;
                MainTabControl.SelectedTab =
                    InConfig.SettingsTab == SettingsTab.FMDisplay ? FMDisplayTabPage :
                    InConfig.SettingsTab == SettingsTab.Other ? OtherTabPage :
                    PathsTabPage;
            }

            // Language can change while the form is open, so store original sizes for later use as minimums
            OKButton.Tag = OKButton.Size;
            Cancel_Button.Tag = Cancel_Button.Size;
        }

        private void SettingsForm_Load(object sender, EventArgs e)
        {
            #region Paths

            Thief1ExePathTextBox.Text = InConfig.T1Exe;
            Thief2ExePathTextBox.Text = InConfig.T2Exe;
            Thief3ExePathTextBox.Text = InConfig.T3Exe;

            BackupPathTextBox.Text = InConfig.FMsBackupPath;

            FMArchivePathsListBox.Items.Clear();
            foreach (var path in InConfig.FMArchivePaths)
            {
                FMArchivePathsListBox.Items.Add(path);
            }

            IncludeSubfoldersCheckBox.Checked = InConfig.FMArchivePathsIncludeSubfolders;

            #endregion

            #region Game organization

            switch (InConfig.GameOrganization)
            {
                case GameOrganization.ByTab:
                    OrganizeGamesByTabRadioButton.Checked = true;
                    break;
                case GameOrganization.OneList:
                    SortGamesInOneListRadioButton.Checked = true;
                    break;
            }

            #endregion

            #region File conversion

            ConvertWAVsTo16BitOnInstallCheckBox.Checked = InConfig.ConvertWAVsTo16BitOnInstall;
            ConvertOGGsToWAVsOnInstallCheckBox.Checked = InConfig.ConvertOGGsToWAVsOnInstall;

            #endregion

            #region Articles

            EnableIgnoreArticlesCheckBox.Checked = InConfig.EnableArticles;

            for (var i = 0; i < InConfig.Articles.Count; i++)
            {
                var article = InConfig.Articles[i];
                if (i > 0) ArticlesTextBox.Text += @", ";
                ArticlesTextBox.Text += article;
            }

            MoveArticlesToEndCheckBox.Checked = InConfig.MoveArticlesToEnd;

            #endregion

            #region Date format

            // NOTE: This section actually depends on the events in order to work. Also it appears to depend on
            // none of the date-related checkboxes being checked by default. Absolutely don't make any of them
            // checked by default!

            switch (InConfig.DateFormat)
            {
                case DateFormat.CurrentCultureShort:
                    DateCurrentCultureShortRadioButton.Checked = true;
                    break;
                case DateFormat.CurrentCultureLong:
                    DateCurrentCultureLongRadioButton.Checked = true;
                    break;
                case DateFormat.Custom:
                    DateCustomRadioButton.Checked = true;
                    break;
            }

            // Do it yourself...
            var pdp = PreviewDatePanel;
            var srb = DateCurrentCultureShortRadioButton;
            pdp.Location = new Point(srb.Location.X + srb.Width, 16);
            pdp.Width = DateFormatGroupBox.Width - pdp.Location.X - 8;

            object[] dateFormatList = { "", "d", "dd", "ddd", "dddd", "M", "MM", "MMM", "MMMM", "yy", "yyyy" };
            Date1ComboBox.Items.AddRange(dateFormatList);
            Date2ComboBox.Items.AddRange(dateFormatList);
            Date3ComboBox.Items.AddRange(dateFormatList);
            Date4ComboBox.Items.AddRange(dateFormatList);

            var d1 = InConfig.DateCustomFormat1;
            var s1 = InConfig.DateCustomSeparator1;
            var d2 = InConfig.DateCustomFormat2;
            var s2 = InConfig.DateCustomSeparator2;
            var d3 = InConfig.DateCustomFormat3;
            var s3 = InConfig.DateCustomSeparator3;
            var d4 = InConfig.DateCustomFormat4;

            Date1ComboBox.SelectedItem = !d1.IsEmpty() && Date1ComboBox.Items.Contains(d1) ? d1 : "dd";
            DateSeparator1TextBox.Text = !s1.IsEmpty() ? s1 : "/";
            Date2ComboBox.SelectedItem = !d2.IsEmpty() && Date2ComboBox.Items.Contains(d2) ? d2 : "MM";
            DateSeparator2TextBox.Text = !s2.IsEmpty() ? s2 : "/";
            Date3ComboBox.SelectedItem = !d3.IsEmpty() && Date3ComboBox.Items.Contains(d3) ? d3 : "yyyy";
            DateSeparator3TextBox.Text = !s3.IsEmpty() ? s3 : "";
            Date4ComboBox.SelectedItem = !d4.IsEmpty() && Date4ComboBox.Items.Contains(d4) ? d4 : "";

            #endregion

            #region Rating display style

            using (new DisableEvents(this))
            {
                switch (InConfig.RatingDisplayStyle)
                {
                    case RatingDisplayStyle.NewDarkLoader:
                        RatingNDLDisplayStyleRadioButton.Checked = true;
                        break;
                    case RatingDisplayStyle.FMSel:
                        RatingFMSelDisplayStyleRadioButton.Checked = true;
                        break;
                }

                RatingUseStarsCheckBox.Checked = InConfig.RatingUseStars;

                RatingExamplePictureBox.Image = RatingNDLDisplayStyleRadioButton.Checked
                    ? Resources.RatingExample_NDL
                    : RatingFMSelDisplayStyleRadioButton.Checked && RatingUseStarsCheckBox.Checked
                    ? Resources.RatingExample_FMSel_Stars
                    : Resources.RatingExample_FMSel_Number;

                RatingUseStarsCheckBox.Enabled = RatingFMSelDisplayStyleRadioButton.Checked;
            }

            #endregion

            #region Backup FM

            switch (InConfig.BackupFMData)
            {
                case BackupFMData.SavesAndScreensOnly:
                    BackupSavesAndScreensOnlyRadioButton.Checked = true;
                    break;
                case BackupFMData.AllChangedFiles:
                    BackupAllChangedDataRadioButton.Checked = true;
                    break;
            }

            BackupAlwaysAskCheckBox.Checked = InConfig.BackupAlwaysAsk;

            #endregion

            #region Languages

            using (new DisableEvents(this))
            {
                foreach (var item in InConfig.LanguageNames)
                {
                    LanguageComboBox.BackingItems.Add(item.Key);
                    LanguageComboBox.Items.Add(item.Value);
                }

                const string engLang = "English";

                if (LanguageComboBox.BackingItems.ContainsI(engLang))
                {
                    LanguageComboBox.BackingItems.Remove(engLang);
                    LanguageComboBox.BackingItems.Insert(0, engLang);
                    LanguageComboBox.Items.Remove(engLang);
                    LanguageComboBox.Items.Insert(0, engLang);
                }
                else
                {
                    LanguageComboBox.BackingItems.Insert(0, engLang);
                    LanguageComboBox.Items.Insert(0, engLang);
                }

                LanguageComboBox.SelectBackingIndexOf(LanguageComboBox.BackingItems.Contains(InConfig.Language)
                    ? InConfig.Language
                    : engLang);
            }

            #endregion

            WebSearchUrlTextBox.Text = InConfig.WebSearchUrl;

            SetUITextToLocalized();
        }

        public void SetUITextToLocalized(bool suspendResume = true)
        {
            if (suspendResume) this.SuspendDrawing();
            try
            {
                Text = LText.SettingsWindow.TitleText;

                OKButton.SetTextAutoSize(LText.Global.OK, ((Size)OKButton.Tag).Width);
                Cancel_Button.SetTextAutoSize(LText.Global.Cancel, ((Size)Cancel_Button.Tag).Width);

                #region Paths tab

                PathsTabPage.Text = Startup ? LText.SettingsWindow.InitialSettings_TabText : LText.SettingsWindow.Paths_TabText;

                PathsToGameExesGroupBox.Text = LText.SettingsWindow.Paths_PathsToGameExes;
                Thief1ExePathLabel.Text = LText.SettingsWindow.Paths_Thief1;
                Thief2ExePathLabel.Text = LText.SettingsWindow.Paths_Thief2;
                Thief3ExePathLabel.Text = LText.SettingsWindow.Paths_Thief3;

                OtherGroupBox.Text = LText.SettingsWindow.Paths_Other;
                BackupPathLabel.Text = LText.SettingsWindow.Paths_BackupPath;

                // Manual "flow layout" for textbox/browse button combos
                for (int i = 0; i < 4; i++)
                {
                    var button =
                        i == 0 ? Thief1ExePathBrowseButton :
                        i == 1 ? Thief2ExePathBrowseButton :
                        i == 2 ? Thief3ExePathBrowseButton :
                        BackupPathBrowseButton;

                    var textBox =
                        i == 0 ? Thief1ExePathTextBox :
                        i == 1 ? Thief2ExePathTextBox :
                        i == 2 ? Thief3ExePathTextBox :
                        BackupPathTextBox;

                    button.SetTextAutoSize(textBox, LText.Global.BrowseEllipses);
                }

                T1T2NeedNewDarkLabel.Text = LText.SettingsWindow.Paths_Thief1AndThief2RequireNewDark;
                T3NeedsSULabel.Text = LText.SettingsWindow.Paths_Thief3RequiresSneakyUpgrade;

                FMArchivePathsGroupBox.Text = LText.SettingsWindow.Paths_FMArchivePaths;
                IncludeSubfoldersCheckBox.Text = LText.SettingsWindow.Paths_IncludeSubfolders;
                MainToolTip.SetToolTip(AddFMArchivePathButton, LText.SettingsWindow.Paths_AddArchivePathToolTip);
                MainToolTip.SetToolTip(RemoveFMArchivePathButton, LText.SettingsWindow.Paths_RemoveArchivePathToolTip);

                #endregion

                #region FM Display tab

                FMDisplayTabPage.Text = LText.SettingsWindow.FMDisplay_TabText;

                GameOrganizationGroupBox.Text = LText.SettingsWindow.FMDisplay_GameOrganization;
                OrganizeGamesByTabRadioButton.Text = LText.SettingsWindow.FMDisplay_GameOrganizationByTab;
                SortGamesInOneListRadioButton.Text = LText.SettingsWindow.FMDisplay_GameOrganizationOneList;

                SortingGroupBox.Text = LText.SettingsWindow.FMDisplay_Sorting;
                EnableIgnoreArticlesCheckBox.Text = LText.SettingsWindow.FMDisplay_IgnoreArticles;
                MoveArticlesToEndCheckBox.Text = LText.SettingsWindow.FMDisplay_MoveArticlesToEnd;

                RatingDisplayStyleGroupBox.Text = LText.SettingsWindow.FMDisplay_RatingDisplayStyle;
                RatingNDLDisplayStyleRadioButton.Text = LText.SettingsWindow.FMDisplay_RatingDisplayStyleNDL;
                RatingFMSelDisplayStyleRadioButton.Text = LText.SettingsWindow.FMDisplay_RatingDisplayStyleFMSel;
                RatingUseStarsCheckBox.Text = LText.SettingsWindow.FMDisplay_RatingDisplayStyleUseStars;

                DateFormatGroupBox.Text = LText.SettingsWindow.FMDisplay_DateFormat;
                DateCurrentCultureShortRadioButton.Text = LText.SettingsWindow.FMDisplay_CurrentCultureShort;
                DateCurrentCultureLongRadioButton.Text = LText.SettingsWindow.FMDisplay_CurrentCultureLong;
                DateCustomRadioButton.Text = LText.SettingsWindow.FMDisplay_Custom;

                #endregion

                #region Other tab

                OtherTabPage.Text = LText.SettingsWindow.Other_TabText;

                FMFileConversionGroupBox.Text = LText.SettingsWindow.Other_FMFileConversion;
                ConvertWAVsTo16BitOnInstallCheckBox.Text = LText.SettingsWindow.Other_ConvertWAVsTo16BitOnInstall;
                ConvertOGGsToWAVsOnInstallCheckBox.Text = LText.SettingsWindow.Other_ConvertOGGsToWAVsOnInstall;

                BackupSavesGroupBox.Text = LText.SettingsWindow.Other_BackUpSaves;
                BackupSavesAndScreensOnlyRadioButton.Text = LText.SettingsWindow.Other_BackUpSavesAndScreenshotsOnly;
                BackupAllChangedDataRadioButton.Text = LText.SettingsWindow.Other_BackUpAllChangedFiles;
                BackupAlwaysAskCheckBox.Text = LText.SettingsWindow.Other_BackUpAlwaysAsk;

                LanguageGroupBox.Text = LText.SettingsWindow.Other_Language;

                WebSearchGroupBox.Text = LText.SettingsWindow.Other_WebSearch;
                WebSearchUrlLabel.Text = LText.SettingsWindow.Other_WebSearchURL;
                WebSearchTitleExplanationLabel.Text = LText.SettingsWindow.Other_WebSearchTitleVar;
                MainToolTip.SetToolTip(WebSearchUrlResetButton, LText.SettingsWindow.Other_WebSearchResetToolTip);

                #endregion
            }
            finally
            {
                if (suspendResume) this.ResumeDrawing();
            }
        }

        private void SettingsForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Special case: this is meta, so it should always be set even if the user clicked Cancel
            OutConfig.SettingsTab =
                MainTabControl.SelectedTab == FMDisplayTabPage ? SettingsTab.FMDisplay :
                MainTabControl.SelectedTab == OtherTabPage ? SettingsTab.Other :
                SettingsTab.Paths;

            if (DialogResult != DialogResult.OK)
            {
                if (!Startup)
                {
                    try
                    {
                        if (!LanguageComboBox.SelectedBackingItem().EqualsI(InConfig.Language))
                        {
                            Ini.Ini.ReadLocalizationIni(Path.Combine(Paths.Languages, InConfig.Language + ".ini"));
                            if (!Startup) OwnerForm.SetUITextToLocalized();
                        }
                    }
                    catch (Exception ex)
                    {
                        // log it
                    }
                }
                return;
            }

            FormatArticles();

            #region Checks

            bool error = false;

            // TODO: Run a similar thing to Model.CheckPaths() to check for cam_mod.ini etc. to be thorough

            foreach (var tb in GameExePathTextBoxes)
            {
                if (!tb.Text.IsWhiteSpace() && !File.Exists(tb.Text))
                {
                    error = true;
                    ShowPathError(tb, true);
                }
            }

            if (!Directory.Exists(BackupPathTextBox.Text))
            {
                error = true;
                ShowPathError(BackupPathTextBox, true);
            }

            if (error)
            {
                e.Cancel = true;
                MainTabControl.SelectedTab = PathsTabPage;
                return;
            }
            else
            {
                foreach (var tb in GameExePathTextBoxes)
                {
                    tb.BackColor = SystemColors.Window;
                    tb.Tag = PathError.False;
                }
                BackupPathTextBox.BackColor = SystemColors.Window;
                BackupPathTextBox.Tag = PathError.False;
                ErrorLabel.Hide();

                // Extremely petty visual nicety - makes the error stuff go away before the form closes
                Refresh();
            }

            #endregion

            #region Paths

            OutConfig.T1Exe = Thief1ExePathTextBox.Text.Trim();
            OutConfig.T2Exe = Thief2ExePathTextBox.Text.Trim();
            OutConfig.T3Exe = Thief3ExePathTextBox.Text.Trim();

            OutConfig.FMsBackupPath = BackupPathTextBox.Text.Trim();

            OutConfig.FMArchivePaths.Clear();
            foreach (string path in FMArchivePathsListBox.Items) OutConfig.FMArchivePaths.Add(path.Trim());

            OutConfig.FMArchivePathsIncludeSubfolders = IncludeSubfoldersCheckBox.Checked;

            #endregion

            #region Game organization

            OutConfig.GameOrganization = OrganizeGamesByTabRadioButton.Checked
                    ? GameOrganization.ByTab
                    : GameOrganization.OneList;

            #endregion

            #region File conversion

            OutConfig.ConvertWAVsTo16BitOnInstall = ConvertWAVsTo16BitOnInstallCheckBox.Checked;
            OutConfig.ConvertOGGsToWAVsOnInstall = ConvertOGGsToWAVsOnInstallCheckBox.Checked;

            #endregion

            #region Articles

            OutConfig.EnableArticles = EnableIgnoreArticlesCheckBox.Checked;

            var retArticles = ArticlesTextBox.Text
                .Replace(", ", ",")
                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Distinct(StringComparer.InvariantCultureIgnoreCase)
                .ToList();

            // Just in case
            for (var i = 0; i < retArticles.Count; i++)
            {
                if (retArticles[i].IsWhiteSpace())
                {
                    retArticles.RemoveAt(i);
                    i--;
                }
            }

            OutConfig.Articles.Clear();
            OutConfig.Articles.AddRange(retArticles);

            OutConfig.MoveArticlesToEnd = MoveArticlesToEndCheckBox.Checked;

            #endregion

            #region Date format

            OutConfig.DateFormat =
                DateCurrentCultureShortRadioButton.Checked ? DateFormat.CurrentCultureShort :
                DateCurrentCultureLongRadioButton.Checked ? DateFormat.CurrentCultureLong :
                DateFormat.Custom;

            OutConfig.DateCustomFormat1 = Date1ComboBox.SelectedItem.ToString();
            OutConfig.DateCustomSeparator1 = DateSeparator1TextBox.Text;
            OutConfig.DateCustomFormat2 = Date2ComboBox.SelectedItem.ToString();
            OutConfig.DateCustomSeparator2 = DateSeparator2TextBox.Text;
            OutConfig.DateCustomFormat3 = Date3ComboBox.SelectedItem.ToString();
            OutConfig.DateCustomSeparator3 = DateSeparator3TextBox.Text;
            OutConfig.DateCustomFormat4 = Date4ComboBox.SelectedItem.ToString();


            var formatString = Date1ComboBox.SelectedItem +
                               DateSeparator1TextBox.Text.EscapeAllChars() +
                               Date2ComboBox.SelectedItem +
                               DateSeparator2TextBox.Text.EscapeAllChars() +
                               Date3ComboBox.SelectedItem +
                               DateSeparator3TextBox.Text.EscapeAllChars() +
                               Date4ComboBox.SelectedItem;

            try
            {
                var testDate = exampleDate.ToString(formatString);
                OutConfig.DateCustomFormatString = formatString;
            }
            catch (FormatException)
            {
                MessageBox.Show(LText.SettingsWindow.FMDisplay_ErrorInvalidDateFormat, LText.AlertMessages.Error,
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                e.Cancel = true;
                return;
            }
            catch (ArgumentOutOfRangeException)
            {
                MessageBox.Show(LText.SettingsWindow.FMDisplay_ErrorDateOutOfRange, LText.AlertMessages.Error,
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                e.Cancel = true;
                return;
            }

            #endregion

            #region Rating display style

            OutConfig.RatingDisplayStyle = RatingNDLDisplayStyleRadioButton.Checked
                ? RatingDisplayStyle.NewDarkLoader
                : RatingDisplayStyle.FMSel;
            OutConfig.RatingUseStars = RatingUseStarsCheckBox.Checked;

            #endregion

            #region Backup saves

            OutConfig.BackupFMData = BackupSavesAndScreensOnlyRadioButton.Checked
                ? BackupFMData.SavesAndScreensOnly
                : BackupFMData.AllChangedFiles;

            OutConfig.BackupAlwaysAsk = BackupAlwaysAskCheckBox.Checked;

            #endregion

            OutConfig.Language = LanguageComboBox.SelectedBackingItem();

            OutConfig.WebSearchUrl = WebSearchUrlTextBox.Text;
        }

        #endregion

        private void GameExePathBrowseButtons_Click(object sender, EventArgs e)
        {
            var tb =
                sender == Thief1ExePathBrowseButton ? Thief1ExePathTextBox :
                sender == Thief2ExePathBrowseButton ? Thief2ExePathTextBox :
                Thief3ExePathTextBox;

            var (result, fileName) = BrowseForExeFile();
            if (result == DialogResult.OK) tb.Text = fileName ?? "";
        }

        private void BackupPathBrowseButton_Click(object sender, EventArgs e)
        {
            using (var d = new AutoFolderBrowserDialog())
            {
                d.InitialDirectory = BackupPathTextBox.Text;
                d.MultiSelect = false;
                if (d.ShowDialog() == DialogResult.OK) BackupPathTextBox.Text = d.DirectoryName;
            }
        }

        #region FMArchivePaths-related

        private bool FMArchivePathExistsInBox(string path)
        {
            foreach (var item in FMArchivePathsListBox.Items)
            {
                if (item.ToString().EqualsI(path)) return true;
            }

            return false;
        }

        private void AddFMArchivePathButton_Click(object sender, EventArgs e)
        {
            using (var d = new AutoFolderBrowserDialog())
            {
                d.InitialDirectory = FMArchivePathsListBox.SelectedIndex > -1
                    ? Path.GetDirectoryName(FMArchivePathsListBox.SelectedItem.ToString())
                    : "";
                d.MultiSelect = true;
                if (d.ShowDialog() == DialogResult.OK)
                {
                    foreach (var dir in d.DirectoryNames)
                    {
                        if (!FMArchivePathExistsInBox(dir)) FMArchivePathsListBox.Items.Add(dir);
                    }
                }
            }
        }

        private void RemoveFMArchivePathButton_Click(object sender, EventArgs e)
        {
            FMArchivePathsListBox.RemoveAndSelectNearest();
        }

        #endregion

        #region Methods

        private static (DialogResult Result, string FileName) BrowseForExeFile()
        {
            using (var dialog = new OpenFileDialog())
            {
                dialog.Filter = LText.BrowseDialogs.ExeFiles + @"|*.exe";
                return (dialog.ShowDialog(), dialog.FileName);
            }
        }

        private void ShowPathError(TextBox textBox, bool shown)
        {
            if (shown)
            {
                if (textBox != null)
                {
                    textBox.BackColor = Color.MistyRose;
                    textBox.Tag = PathError.True;
                }
                ErrorLabel.Text = LText.SettingsWindow.Paths_ErrorSomePathsAreInvalid;
                ErrorLabel.Show();
            }
            else
            {
                if (textBox != null)
                {
                    textBox.BackColor = SystemColors.Window;
                    textBox.Tag = PathError.False;
                }

                bool errorsRemaining = BackupPathTextBox.Tag is PathError bError && bError == PathError.True;
                foreach (var tb in GameExePathTextBoxes)
                {
                    if (tb.Tag is PathError gError && gError == PathError.True) errorsRemaining = true;
                }
                if (errorsRemaining) return;

                ErrorLabel.Text = "";
                ErrorLabel.Hide();
            }
        }

        #endregion

        private void ArticlesCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            ArticlesTextBox.Enabled = EnableIgnoreArticlesCheckBox.Checked;
            MoveArticlesToEndCheckBox.Enabled = EnableIgnoreArticlesCheckBox.Checked;
        }

        private void ArticlesTextBox_Leave(object sender, EventArgs e)
        {
            FormatArticles();
        }

        private void FormatArticles()
        {
            var articles = ArticlesTextBox.Text;

            // Copied wholesale from Autovid, ridiculous looking, but works

            if (articles.IsWhiteSpace())
            {
                ArticlesTextBox.Text = "";
                return;
            }

            // Remove duplicate consecutive spaces
            articles = Regex.Replace(articles, @"\s{2,}", " ");

            // Remove spaces surrounding commas
            articles = Regex.Replace(articles, @"\s?\,\s?", ",");

            // Remove duplicate consecutive commas
            articles = Regex.Replace(articles, @"\,{2,}", ",");

            // Remove commas from start and end
            articles = articles.Trim(',');

            var articlesArray = articles.Split(',', ' ').Distinct(StringComparer.InvariantCultureIgnoreCase).ToArray();

            articles = "";
            for (var i = 0; i < articlesArray.Length; i++)
            {
                if (i > 0) articles += ", ";
                articles += articlesArray[i];
            }

            ArticlesTextBox.Text = articles;
        }

        private void UpdateExampleDate()
        {

            var formatString = Date1ComboBox.SelectedItem +
                               DateSeparator1TextBox.Text.EscapeAllChars() +
                               Date2ComboBox.SelectedItem +
                               DateSeparator2TextBox.Text.EscapeAllChars() +
                               Date3ComboBox.SelectedItem +
                               DateSeparator3TextBox.Text.EscapeAllChars() +
                               Date4ComboBox.SelectedItem;

            try
            {
                var date = exampleDate.ToString(formatString);
                PreviewDateLabel.Text = date;
            }
            catch (FormatException)
            {
                MessageBox.Show(LText.SettingsWindow.FMDisplay_ErrorInvalidDateFormat, LText.AlertMessages.Error,
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);

            }
            catch (ArgumentOutOfRangeException)
            {
                MessageBox.Show(LText.SettingsWindow.FMDisplay_ErrorDateOutOfRange, LText.AlertMessages.Error,
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void DateCurrentCultureShortRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            DateCustomFormatPanel.Enabled = false;
            PreviewDateLabel.Text = exampleDate.ToShortDateString();
        }

        private void DateCurrentCultureLongRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            DateCustomFormatPanel.Enabled = false;
            PreviewDateLabel.Text = exampleDate.ToLongDateString();
        }

        private void DateCustomRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            var s = (RadioButton)sender;
            DateCustomFormatPanel.Enabled = s.Checked;

            if (s.Checked) UpdateExampleDate();
        }

        private void DateComboBoxes_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (DateCustomFormatPanel.Enabled) UpdateExampleDate();
        }

        private void DateSeparatorTextBoxes_TextChanged(object sender, EventArgs e)
        {
            if (DateCustomFormatPanel.Enabled) UpdateExampleDate();
        }

        private void RatingOutOfTenRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (EventsDisabled) return;
            if (RatingNDLDisplayStyleRadioButton.Checked)
            {
                RatingUseStarsCheckBox.Enabled = false;
                RatingExamplePictureBox.Image = Resources.RatingExample_NDL;
            }
        }

        private void RatingOutOfFiveRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (EventsDisabled) return;
            if (RatingFMSelDisplayStyleRadioButton.Checked)
            {
                RatingUseStarsCheckBox.Enabled = true;
                RatingExamplePictureBox.Image = RatingUseStarsCheckBox.Checked
                    ? Resources.RatingExample_FMSel_Stars
                    : Resources.RatingExample_FMSel_Number;
            }
        }

        private void RatingUseStarsCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (EventsDisabled) return;
            RatingExamplePictureBox.Image = RatingUseStarsCheckBox.Checked
                ? Resources.RatingExample_FMSel_Stars
                : Resources.RatingExample_FMSel_Number;
        }

        private void GameExePathTextBoxes_Leave(object sender, EventArgs e)
        {
            var s = (TextBox)sender;
            ShowPathError(s, !s.Text.IsEmpty() && !File.Exists(s.Text));
        }

        private void BackupPathTextBox_Leave(object sender, EventArgs e)
        {
            var s = (TextBox)sender;
            ShowPathError(s, !Directory.Exists(s.Text));
        }

        private void WebSearchURLResetButton_Click(object sender, EventArgs e)
        {
            WebSearchUrlTextBox.Text = Defaults.WebSearchUrl;
        }

        private void LanguageComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (EventsDisabled) return;
            var s = LanguageComboBox;
            Ini.Ini.ReadLocalizationIni(Path.Combine(Paths.Languages, s.SelectedBackingItem() + ".ini"));
            SetUITextToLocalized();
            if (!Startup) OwnerForm.SetUITextToLocalized();
        }
    }
}
