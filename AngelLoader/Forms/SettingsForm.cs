using System;
using System.Collections;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using AngelLoader.Common;
using AngelLoader.Common.DataClasses;
using AngelLoader.Common.Utility;
using AngelLoader.CustomControls;
using AngelLoader.CustomControls.SettingsPages;
using AngelLoader.WinAPI.Dialogs;
using static AngelLoader.Common.Logger;
using static AngelLoader.CustomControls.SettingsPages.Interfaces;

namespace AngelLoader.Forms
{
    /*
    PERF (2019-06-24):
    _Load: ~31ms
    ctor: ~17ms
    
    Major bottlenecks are:
    -Localize() - SetTextAutoSize() calls being the main contributors
    -ShowPage() (for Other page) - nothing we can do about this
    -Page ctors - lazy-loading these would be a giant headache - not really worth it
    */

    internal sealed partial class SettingsForm : Form, IEventDisabler
    {
        #region Private fields

        private readonly ILocalizable OwnerForm;

        private readonly bool Startup;

        #region Copies of passed-in data

        private readonly string _inLanguage;

        private readonly int _inPathsVScrollPos;
        private readonly int _inFMDisplayVScrollPos;
        private readonly int _inOtherVScrollPos;

        #endregion

        private readonly RadioButtonCustom[] PageRadioButtons;
        private readonly ISettingsPage[] Pages;
        private readonly int?[] PageVScrollValues;

        private readonly TextBox[] ExePathTextBoxes;

        #endregion

        public readonly ConfigData OutConfig;

        private enum PathError { True, False }

        // August 4 is chosen more-or-less randomly, but both its name and its number are different short vs. long
        // (Aug vs. August; 8 vs. 08), and the same thing with 4 (4 vs. 04).
        private readonly DateTime _exampleDate = new DateTime(DateTime.Now.Year, 8, 4);

        public bool EventsDisabled { get; set; }

#pragma warning disable IDE0069 // Disposable fields should be disposed
        // They'll be disposed automatically like any other control
        private readonly ComboBoxCustom LangComboBox;

        private readonly GroupBox LangGroupBox;
        private readonly PathsPage PathsPage;
        private readonly FMDisplayPage FMDisplayPage;
        private readonly OtherPage OtherPage;
#pragma warning restore IDE0069 // Disposable fields should be disposed

        internal SettingsForm(ILocalizable ownerForm, ConfigData config, bool startup)
        {
            InitializeComponent();

            // Needed for Esc-to-cancel-drag and stuff
            KeyPreview = true;

            Startup = startup;
            OwnerForm = ownerForm;

            #region Init copies of passed-in data

            _inLanguage = config.Language;

            _inPathsVScrollPos = config.SettingsPathsVScrollPos;
            _inFMDisplayVScrollPos = config.SettingsFMDisplayVScrollPos;
            _inOtherVScrollPos = config.SettingsOtherVScrollPos;

            #endregion

            OutConfig = new ConfigData();

            PathsPage = new PathsPage { Visible = false };
            FMDisplayPage = new FMDisplayPage { Visible = false };
            OtherPage = new OtherPage { Visible = false };

            LangGroupBox = OtherPage.LanguageGroupBox;
            LangComboBox = OtherPage.LanguageComboBox;

            ExePathTextBoxes = new[]
            {
                PathsPage.Thief1ExePathTextBox,
                PathsPage.Thief2ExePathTextBox,
                PathsPage.Thief3ExePathTextBox,
                PathsPage.SS2ExePathTextBox,
                PathsPage.SteamExeTextBox
            };

            PageRadioButtons = new[] { PathsRadioButton, FMDisplayRadioButton, OtherRadioButton };

            // These are nullable because null values get put INTO them later. So not a mistake to fill them with
            // non-nullable ints right off the bat.
            PageVScrollValues = new int?[]
            {
                _inPathsVScrollPos,
                _inFMDisplayVScrollPos,
                _inOtherVScrollPos
            };

            #region Add pages

            PagePanel.Controls.Add(PathsPage);
            PathsPage.Dock = DockStyle.Fill;

            if (startup)
            {
                Pages = new ISettingsPage[] { PathsPage };

                PathsPage.PagePanel.Controls.Add(LangGroupBox);
                OtherPage.PagePanel.Controls.Remove(LangGroupBox);
                LangGroupBox.Location = new Point(8, 8);
                LangGroupBox.Width = PathsPage.Width - 16;
                LangGroupBox.MinimumSize = new Size(LangGroupBox.Width, LangGroupBox.MinimumSize.Height);
                PathsPage.ActualPathsPanel.Location = new Point(0, LangGroupBox.Height + 8);
            }
            else
            {
                Pages = new ISettingsPage[] { PathsPage, FMDisplayPage, OtherPage };

                PagePanel.Controls.Add(FMDisplayPage);
                PagePanel.Controls.Add(OtherPage);

                FMDisplayPage.Dock = DockStyle.Fill;
                OtherPage.Dock = DockStyle.Fill;
            }

            #endregion

            #region Set non-page UI state

            // This DisableEvents block is still required because it involves non-page events
            using (new DisableEvents(this))
            {
                if (startup)
                {
                    // _Load is too late for some of this stuff, so might as well put everything here
                    StartPosition = FormStartPosition.CenterScreen;
                    ShowInTaskbar = true;
                    PathsRadioButton.Checked = true;
                    FMDisplayRadioButton.Hide();
                    OtherRadioButton.Hide();
                }
                else
                {
                    switch (config.SettingsTab)
                    {
                        case SettingsTab.FMDisplay:
                            FMDisplayRadioButton.Checked = true;
                            break;
                        case SettingsTab.Other:
                            OtherRadioButton.Checked = true;
                            break;
                        default:
                            PathsRadioButton.Checked = true;
                            break;
                    }
                }
            }

            #endregion

            // Language can change while the form is open, so store original sizes for later use as minimums
            OKButton.Tag = OKButton.Size;
            Cancel_Button.Tag = Cancel_Button.Size;

            Width = Math.Min(config.SettingsWindowSize.Width, Screen.PrimaryScreen.WorkingArea.Width);
            Height = Math.Min(config.SettingsWindowSize.Height, Screen.PrimaryScreen.WorkingArea.Height);
            MainSplitContainer.SplitterDistance = config.SettingsWindowSplitterDistance;

            #region Set page UI state

            #region Load languages

            var tempLangDict = new OrderedDictionary();

            foreach (var item in config.LanguageNames) tempLangDict[item.Key] = item.Value;

            const string engLang = "English";

            if (tempLangDict.Contains(engLang)) tempLangDict.Remove(engLang);
            tempLangDict.Insert(0, engLang, engLang);

            foreach (DictionaryEntry item in tempLangDict)
            {
                LangComboBox.AddFullItem(item.Key.ToString(), item.Value.ToString());
            }

            LangComboBox.SelectBackingIndexOf(LangComboBox.BackingItems.Contains(config.Language)
                ? config.Language
                : engLang);

            #endregion

            #region Paths page

            PathsPage.Thief1ExePathTextBox.Text = config.T1Exe;
            PathsPage.Thief2ExePathTextBox.Text = config.T2Exe;
            PathsPage.Thief3ExePathTextBox.Text = config.T3Exe;
            PathsPage.SS2ExePathTextBox.Text = config.SS2Exe;

            PathsPage.SteamExeTextBox.Text = config.SteamExe;
            PathsPage.LaunchTheseGamesThroughSteamPanel.Enabled = !PathsPage.SteamExeTextBox.Text.IsWhiteSpace();
            PathsPage.LaunchTheseGamesThroughSteamCheckBox.Checked = config.LaunchGamesWithSteam;
            PathsPage.T1UseSteamCheckBox.Checked = config.T1UseSteam;
            PathsPage.T2UseSteamCheckBox.Checked = config.T2UseSteam;
            PathsPage.T3UseSteamCheckBox.Checked = config.T3UseSteam;
            PathsPage.SS2UseSteamCheckBox.Checked = config.SS2UseSteam;
            SetUseSteamGameCheckBoxesEnabled(config.LaunchGamesWithSteam);

            PathsPage.BackupPathTextBox.Text = config.FMsBackupPath;

            PathsPage.FMArchivePathsListBox.Items.Clear();
            foreach (var path in config.FMArchivePaths) PathsPage.FMArchivePathsListBox.Items.Add(path);

            PathsPage.IncludeSubfoldersCheckBox.Checked = config.FMArchivePathsIncludeSubfolders;

            #endregion

            if (!startup)
            {
                #region FM Display page

                #region Game organization

                switch (config.GameOrganization)
                {
                    case GameOrganization.ByTab:
                        FMDisplayPage.OrganizeGamesByTabRadioButton.Checked = true;
                        break;
                    case GameOrganization.OneList:
                        FMDisplayPage.SortGamesInOneListRadioButton.Checked = true;
                        break;
                }

                FMDisplayPage.UseShortGameTabNamesCheckBox.Checked = config.UseShortGameTabNames;

                #endregion

                #region Articles

                FMDisplayPage.EnableIgnoreArticlesCheckBox.Checked = config.EnableArticles;

                for (var i = 0; i < config.Articles.Count; i++)
                {
                    var article = config.Articles[i];
                    if (i > 0) FMDisplayPage.ArticlesTextBox.Text += @", ";
                    FMDisplayPage.ArticlesTextBox.Text += article;
                }

                FMDisplayPage.MoveArticlesToEndCheckBox.Checked = config.MoveArticlesToEnd;

                #endregion

                #region Date format

                object[] dateFormatList = { "", "d", "dd", "ddd", "dddd", "M", "MM", "MMM", "MMMM", "yy", "yyyy" };
                FMDisplayPage.Date1ComboBox.Items.AddRange(dateFormatList);
                FMDisplayPage.Date2ComboBox.Items.AddRange(dateFormatList);
                FMDisplayPage.Date3ComboBox.Items.AddRange(dateFormatList);
                FMDisplayPage.Date4ComboBox.Items.AddRange(dateFormatList);

                var d1 = config.DateCustomFormat1;
                var s1 = config.DateCustomSeparator1;
                var d2 = config.DateCustomFormat2;
                var s2 = config.DateCustomSeparator2;
                var d3 = config.DateCustomFormat3;
                var s3 = config.DateCustomSeparator3;
                var d4 = config.DateCustomFormat4;

                FMDisplayPage.Date1ComboBox.SelectedItem = !d1.IsEmpty() && FMDisplayPage.Date1ComboBox.Items.Contains(d1) ? d1 : "dd";
                FMDisplayPage.DateSeparator1TextBox.Text = !s1.IsEmpty() ? s1 : "/";
                FMDisplayPage.Date2ComboBox.SelectedItem = !d2.IsEmpty() && FMDisplayPage.Date2ComboBox.Items.Contains(d2) ? d2 : "MM";
                FMDisplayPage.DateSeparator2TextBox.Text = !s2.IsEmpty() ? s2 : "/";
                FMDisplayPage.Date3ComboBox.SelectedItem = !d3.IsEmpty() && FMDisplayPage.Date3ComboBox.Items.Contains(d3) ? d3 : "yyyy";
                FMDisplayPage.DateSeparator3TextBox.Text = !s3.IsEmpty() ? s3 : "";
                FMDisplayPage.Date4ComboBox.SelectedItem = !d4.IsEmpty() && FMDisplayPage.Date4ComboBox.Items.Contains(d4) ? d4 : "";

                // This comes last so that all the custom data is in place for the preview date to use
                switch (config.DateFormat)
                {
                    case DateFormat.CurrentCultureShort:
                        FMDisplayPage.DateCurrentCultureShortRadioButton.Checked = true;
                        FMDisplayPage.DateCustomFormatPanel.Enabled = false;
                        FMDisplayPage.PreviewDateLabel.Text = _exampleDate.ToShortDateString();
                        break;
                    case DateFormat.CurrentCultureLong:
                        FMDisplayPage.DateCurrentCultureLongRadioButton.Checked = true;
                        FMDisplayPage.DateCustomFormatPanel.Enabled = false;
                        FMDisplayPage.PreviewDateLabel.Text = _exampleDate.ToLongDateString();
                        break;
                    case DateFormat.Custom:
                        FMDisplayPage.DateCustomRadioButton.Checked = true;
                        FMDisplayPage.DateCustomFormatPanel.Enabled = true;
                        UpdateCustomExampleDate();
                        break;
                }

                #endregion

                #region Rating display style

                switch (config.RatingDisplayStyle)
                {
                    case RatingDisplayStyle.NewDarkLoader:
                        FMDisplayPage.RatingNDLDisplayStyleRadioButton.Checked = true;
                        break;
                    case RatingDisplayStyle.FMSel:
                        FMDisplayPage.RatingFMSelDisplayStyleRadioButton.Checked = true;
                        break;
                }

                FMDisplayPage.RatingUseStarsCheckBox.Checked = config.RatingUseStars;

                FMDisplayPage.RatingExamplePictureBox.Image = FMDisplayPage.RatingNDLDisplayStyleRadioButton.Checked
                    ? Images.RatingExample_NDL
                    : FMDisplayPage.RatingFMSelDisplayStyleRadioButton.Checked && FMDisplayPage.RatingUseStarsCheckBox.Checked
                    ? Images.RatingExample_FMSel_Stars
                    : Images.RatingExample_FMSel_Number;

                FMDisplayPage.RatingUseStarsCheckBox.Enabled = FMDisplayPage.RatingFMSelDisplayStyleRadioButton.Checked;

                #endregion

                #endregion

                #region Other page

                #region File conversion

                OtherPage.ConvertWAVsTo16BitOnInstallCheckBox.Checked = config.ConvertWAVsTo16BitOnInstall;
                OtherPage.ConvertOGGsToWAVsOnInstallCheckBox.Checked = config.ConvertOGGsToWAVsOnInstall;

                #endregion

                #region Uninstalling FMs

                OtherPage.ConfirmUninstallCheckBox.Checked = config.ConfirmUninstall;

                switch (config.BackupFMData)
                {
                    case BackupFMData.SavesAndScreensOnly:
                        OtherPage.BackupSavesAndScreensOnlyRadioButton.Checked = true;
                        break;
                    case BackupFMData.AllChangedFiles:
                        OtherPage.BackupAllChangedDataRadioButton.Checked = true;
                        break;
                }

                OtherPage.BackupAlwaysAskCheckBox.Checked = config.BackupAlwaysAsk;

                #endregion

                OtherPage.WebSearchUrlTextBox.Text = config.WebSearchUrl;

                OtherPage.ConfirmPlayOnDCOrEnterCheckBox.Checked = config.ConfirmPlayOnDCOrEnter;

                #region Show/hide UI elements

                OtherPage.HideUninstallButtonCheckBox.Checked = config.HideUninstallButton;
                OtherPage.HideFMListZoomButtonsCheckBox.Checked = config.HideFMListZoomButtons;

                #endregion

                OtherPage.ReadmeFixedWidthFontCheckBox.Checked = config.ReadmeUseFixedWidthFont;

                #endregion
            }

            #endregion

            // Comes last so we don't have to use any DisableEvents blocks
            #region Hook up page events

            PathsPage.Thief1ExePathTextBox.Leave += ExePathTextBoxes_Leave;
            PathsPage.Thief2ExePathTextBox.Leave += ExePathTextBoxes_Leave;
            PathsPage.Thief3ExePathTextBox.Leave += ExePathTextBoxes_Leave;
            PathsPage.SS2ExePathTextBox.Leave += ExePathTextBoxes_Leave;

            PathsPage.Thief1ExePathBrowseButton.Click += ExePathBrowseButtons_Click;
            PathsPage.Thief2ExePathBrowseButton.Click += ExePathBrowseButtons_Click;
            PathsPage.Thief3ExePathBrowseButton.Click += ExePathBrowseButtons_Click;
            PathsPage.SS2ExePathBrowseButton.Click += ExePathBrowseButtons_Click;

            PathsPage.SteamExeTextBox.Leave += ExePathTextBoxes_Leave;
            PathsPage.LaunchTheseGamesThroughSteamCheckBox.CheckedChanged += LaunchTheseGamesThroughSteamCheckBox_CheckedChanged;
            PathsPage.SteamExeTextBox.TextChanged += SteamExeTextBox_TextChanged;

            PathsPage.SteamExeBrowseButton.Click += ExePathBrowseButtons_Click;

            PathsPage.BackupPathTextBox.Leave += BackupPathTextBox_Leave;
            PathsPage.BackupPathBrowseButton.Click += BackupPathBrowseButton_Click;

            PathsPage.AddFMArchivePathButton.Click += AddFMArchivePathButton_Click;
            PathsPage.RemoveFMArchivePathButton.Click += RemoveFMArchivePathButton_Click;

            LangComboBox.SelectedIndexChanged += LanguageComboBox_SelectedIndexChanged;

            if (!startup)
            {
                FMDisplayPage.EnableIgnoreArticlesCheckBox.CheckedChanged += ArticlesCheckBox_CheckedChanged;
                FMDisplayPage.ArticlesTextBox.Leave += ArticlesTextBox_Leave;

                FMDisplayPage.RatingNDLDisplayStyleRadioButton.CheckedChanged += RatingOutOfTenRadioButton_CheckedChanged;
                FMDisplayPage.RatingFMSelDisplayStyleRadioButton.CheckedChanged += RatingOutOfFiveRadioButton_CheckedChanged;
                FMDisplayPage.RatingUseStarsCheckBox.CheckedChanged += RatingUseStarsCheckBox_CheckedChanged;

                FMDisplayPage.DateCurrentCultureShortRadioButton.CheckedChanged += DateShortAndLongRadioButtons_CheckedChanged;
                FMDisplayPage.DateCurrentCultureLongRadioButton.CheckedChanged += DateShortAndLongRadioButtons_CheckedChanged;
                FMDisplayPage.DateCustomRadioButton.CheckedChanged += DateCustomRadioButton_CheckedChanged;

                FMDisplayPage.Date1ComboBox.SelectedIndexChanged += DateCustomValue_Changed;
                FMDisplayPage.Date2ComboBox.SelectedIndexChanged += DateCustomValue_Changed;
                FMDisplayPage.Date3ComboBox.SelectedIndexChanged += DateCustomValue_Changed;
                FMDisplayPage.Date4ComboBox.SelectedIndexChanged += DateCustomValue_Changed;

                FMDisplayPage.DateSeparator1TextBox.TextChanged += DateCustomValue_Changed;
                FMDisplayPage.DateSeparator2TextBox.TextChanged += DateCustomValue_Changed;
                FMDisplayPage.DateSeparator3TextBox.TextChanged += DateCustomValue_Changed;

                OtherPage.WebSearchUrlResetButton.Click += WebSearchURLResetButton_Click;
            }

            #endregion
        }

        private void SettingsForm_Load(object sender, EventArgs e)
        {
            foreach (var button in PageRadioButtons)
            {
                if (button.Checked)
                {
                    ShowPage(Array.IndexOf(PageRadioButtons, button), initialCall: true);
                    break;
                }
            }

            Localize(suspendResume: false);
        }

        private void SetUseSteamGameCheckBoxesEnabled(bool enabled)
        {
            PathsPage.T1UseSteamCheckBox.Enabled = enabled;
            PathsPage.T2UseSteamCheckBox.Enabled = enabled;
            PathsPage.T3UseSteamCheckBox.Enabled = enabled;
            PathsPage.SS2UseSteamCheckBox.Enabled = enabled;
        }

        private void LaunchTheseGamesThroughSteamCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            SetUseSteamGameCheckBoxesEnabled(PathsPage.LaunchTheseGamesThroughSteamCheckBox.Checked);
        }

        private void Localize(bool suspendResume = true)
        {
            if (suspendResume) this.SuspendDrawing();
            try
            {
                Text = Startup ? LText.SettingsWindow.StartupTitleText : LText.SettingsWindow.TitleText;

                OKButton.SetTextAutoSize(LText.Global.OK, ((Size)OKButton.Tag).Width);
                Cancel_Button.SetTextAutoSize(LText.Global.Cancel, ((Size)Cancel_Button.Tag).Width);

                #region Paths tab

                PathsRadioButton.Text = Startup
                    ? LText.SettingsWindow.InitialSettings_TabText
                    : LText.SettingsWindow.Paths_TabText;

                PathsPage.PathsToGameExesGroupBox.Text = LText.SettingsWindow.Paths_PathsToGameExes;
                PathsPage.Thief1ExePathLabel.Text = LText.Global.Thief1_Colon;
                PathsPage.Thief2ExePathLabel.Text = LText.Global.Thief2_Colon;
                PathsPage.Thief3ExePathLabel.Text = LText.Global.Thief3_Colon;
                PathsPage.SS2ExePathLabel.Text = LText.Global.SystemShock2_Colon;

                PathsPage.SteamOptionsGroupBox.Text = LText.SettingsWindow.Paths_SteamOptions;
                PathsPage.SteamExeLabel.Text = LText.SettingsWindow.Paths_PathToSteamExecutable;
                PathsPage.LaunchTheseGamesThroughSteamCheckBox.Text = LText.SettingsWindow.Paths_LaunchTheseGamesThroughSteam;
                PathsPage.T1UseSteamCheckBox.Text = LText.Global.Thief1;
                PathsPage.T2UseSteamCheckBox.Text = LText.Global.Thief2;
                PathsPage.T3UseSteamCheckBox.Text = LText.Global.Thief3;
                PathsPage.SS2UseSteamCheckBox.Text = LText.Global.SystemShock2;

                PathsPage.OtherGroupBox.Text = LText.SettingsWindow.Paths_Other;
                PathsPage.BackupPathLabel.Text = LText.SettingsWindow.Paths_BackupPath;

                // Manual "flow layout" for textbox/browse button combos
                PathsPage.Thief1ExePathBrowseButton.SetTextAutoSize(PathsPage.Thief1ExePathTextBox, LText.Global.BrowseEllipses);
                PathsPage.Thief2ExePathBrowseButton.SetTextAutoSize(PathsPage.Thief2ExePathTextBox, LText.Global.BrowseEllipses);
                PathsPage.Thief3ExePathBrowseButton.SetTextAutoSize(PathsPage.Thief3ExePathTextBox, LText.Global.BrowseEllipses);
                PathsPage.SS2ExePathBrowseButton.SetTextAutoSize(PathsPage.SS2ExePathTextBox, LText.Global.BrowseEllipses);
                PathsPage.BackupPathBrowseButton.SetTextAutoSize(PathsPage.BackupPathTextBox, LText.Global.BrowseEllipses);
                PathsPage.SteamExeBrowseButton.SetTextAutoSize(PathsPage.SteamExeTextBox, LText.Global.BrowseEllipses);

                PathsPage.GameRequirementsLabel.Text =
                    LText.SettingsWindow.Paths_DarkEngineGamesRequireNewDark + Environment.NewLine +
                    LText.SettingsWindow.Paths_Thief3RequiresSneakyUpgrade;

                PathsPage.FMArchivePathsGroupBox.Text = LText.SettingsWindow.Paths_FMArchivePaths;
                PathsPage.IncludeSubfoldersCheckBox.Text = LText.SettingsWindow.Paths_IncludeSubfolders;
                MainToolTip.SetToolTip(PathsPage.AddFMArchivePathButton, LText.SettingsWindow.Paths_AddArchivePathToolTip);
                MainToolTip.SetToolTip(PathsPage.RemoveFMArchivePathButton, LText.SettingsWindow.Paths_RemoveArchivePathToolTip);

                #endregion

                if (Startup)
                {
                    LangGroupBox.Text = LText.SettingsWindow.Other_Language;
                }
                else
                {
                    #region FM Display tab

                    FMDisplayRadioButton.Text = LText.SettingsWindow.FMDisplay_TabText;

                    FMDisplayPage.GameOrganizationGroupBox.Text = LText.SettingsWindow.FMDisplay_GameOrganization;
                    FMDisplayPage.OrganizeGamesByTabRadioButton.Text = LText.SettingsWindow.FMDisplay_GameOrganizationByTab;
                    FMDisplayPage.UseShortGameTabNamesCheckBox.Text = LText.SettingsWindow.FMDisplay_UseShortGameTabNames;
                    FMDisplayPage.SortGamesInOneListRadioButton.Text = LText.SettingsWindow.FMDisplay_GameOrganizationOneList;

                    FMDisplayPage.SortingGroupBox.Text = LText.SettingsWindow.FMDisplay_Sorting;
                    FMDisplayPage.EnableIgnoreArticlesCheckBox.Text = LText.SettingsWindow.FMDisplay_IgnoreArticles;
                    FMDisplayPage.MoveArticlesToEndCheckBox.Text = LText.SettingsWindow.FMDisplay_MoveArticlesToEnd;

                    FMDisplayPage.RatingDisplayStyleGroupBox.Text = LText.SettingsWindow.FMDisplay_RatingDisplayStyle;
                    FMDisplayPage.RatingNDLDisplayStyleRadioButton.Text = LText.SettingsWindow.FMDisplay_RatingDisplayStyleNDL;
                    FMDisplayPage.RatingFMSelDisplayStyleRadioButton.Text = LText.SettingsWindow.FMDisplay_RatingDisplayStyleFMSel;
                    FMDisplayPage.RatingUseStarsCheckBox.Text = LText.SettingsWindow.FMDisplay_RatingDisplayStyleUseStars;

                    FMDisplayPage.DateFormatGroupBox.Text = LText.SettingsWindow.FMDisplay_DateFormat;
                    FMDisplayPage.DateCurrentCultureShortRadioButton.Text = LText.SettingsWindow.FMDisplay_CurrentCultureShort;
                    FMDisplayPage.DateCurrentCultureLongRadioButton.Text = LText.SettingsWindow.FMDisplay_CurrentCultureLong;
                    FMDisplayPage.DateCustomRadioButton.Text = LText.SettingsWindow.FMDisplay_Custom;

                    #endregion

                    #region Other tab

                    OtherRadioButton.Text = LText.SettingsWindow.Other_TabText;

                    OtherPage.FMFileConversionGroupBox.Text = LText.SettingsWindow.Other_FMFileConversion;
                    OtherPage.ConvertWAVsTo16BitOnInstallCheckBox.Text = LText.SettingsWindow.Other_ConvertWAVsTo16BitOnInstall;
                    OtherPage.ConvertOGGsToWAVsOnInstallCheckBox.Text = LText.SettingsWindow.Other_ConvertOGGsToWAVsOnInstall;

                    OtherPage.UninstallingFMsGroupBox.Text = LText.SettingsWindow.Other_UninstallingFMs;
                    OtherPage.ConfirmUninstallCheckBox.Text = LText.SettingsWindow.Other_ConfirmBeforeUninstalling;
                    OtherPage.WhatToBackUpLabel.Text = LText.SettingsWindow.Other_WhenUninstallingBackUp;
                    OtherPage.BackupSavesAndScreensOnlyRadioButton.Text = LText.SettingsWindow.Other_BackUpSavesAndScreenshotsOnly;
                    OtherPage.BackupAllChangedDataRadioButton.Text = LText.SettingsWindow.Other_BackUpAllChangedFiles;
                    OtherPage.BackupAlwaysAskCheckBox.Text = LText.SettingsWindow.Other_BackUpAlwaysAsk;

                    OtherPage.LanguageGroupBox.Text = LText.SettingsWindow.Other_Language;

                    OtherPage.WebSearchGroupBox.Text = LText.SettingsWindow.Other_WebSearch;
                    OtherPage.WebSearchUrlLabel.Text = LText.SettingsWindow.Other_WebSearchURL;
                    OtherPage.WebSearchTitleExplanationLabel.Text = LText.SettingsWindow.Other_WebSearchTitleVar;
                    MainToolTip.SetToolTip(OtherPage.WebSearchUrlResetButton, LText.SettingsWindow.Other_WebSearchResetToolTip);

                    OtherPage.PlayFMOnDCOrEnterGroupBox.Text = LText.SettingsWindow.Other_ConfirmPlayOnDCOrEnter;
                    OtherPage.ConfirmPlayOnDCOrEnterCheckBox.Text = LText.SettingsWindow.Other_ConfirmPlayOnDCOrEnter_Ask;

                    OtherPage.ShowOrHideUIElementsGroupBox.Text = LText.SettingsWindow.Other_ShowOrHideInterfaceElements;
                    OtherPage.HideUninstallButtonCheckBox.Text = LText.SettingsWindow.Other_HideUninstallButton;
                    OtherPage.HideFMListZoomButtonsCheckBox.Text = LText.SettingsWindow.Other_HideFMListZoomButtons;

                    OtherPage.ReadmeGroupBox.Text = LText.SettingsWindow.Other_ReadmeBox;
                    OtherPage.ReadmeFixedWidthFontCheckBox.Text = LText.SettingsWindow.Other_ReadmeUseFixedWidthFont;

                    #endregion
                }
            }
            finally
            {
                if (suspendResume) this.ResumeDrawing();
            }
        }

        private void SettingsForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            #region Save window state

            // Special case: these are meta, so they should always be set even if the user clicked Cancel
            OutConfig.SettingsTab =
                FMDisplayRadioButton.Checked ? SettingsTab.FMDisplay :
                OtherRadioButton.Checked ? SettingsTab.Other :
                SettingsTab.Paths;
            OutConfig.SettingsWindowSize = Size;
            OutConfig.SettingsWindowSplitterDistance = MainSplitContainer.SplitterDistance;

            // If some pages haven't had their vertical scroll value loaded, just take the value from the backing
            // store
            OutConfig.SettingsPathsVScrollPos = PageVScrollValues[0] ?? PathsPage.GetVScrollPos();
            OutConfig.SettingsFMDisplayVScrollPos = PageVScrollValues[1] ?? FMDisplayPage.GetVScrollPos();
            OutConfig.SettingsOtherVScrollPos = PageVScrollValues[2] ?? OtherPage.GetVScrollPos();

            #endregion

            #region Cancel

            if (DialogResult != DialogResult.OK)
            {
                if (!Startup && !LangComboBox.SelectedBackingItem().EqualsI(_inLanguage))
                {
                    try
                    {
                        Ini.Ini.ReadLocalizationIni(Path.Combine(Paths.Languages, _inLanguage + ".ini"));
                        LocalizeOwnerForm();
                    }
                    catch (Exception ex)
                    {
                        Log("Exception in language reading", ex);
                    }
                }

                return;
            }

            #endregion

            if (!Startup) FormatArticles();

            #region Checks

            bool error = false;

            // TODO: Check for cam_mod.ini etc. to be thorough

            foreach (var tb in ExePathTextBoxes)
            {
                if (!tb.Text.IsWhiteSpace() && !File.Exists(tb.Text))
                {
                    error = true;
                    ShowPathError(tb, true);
                }
            }

            if (!Directory.Exists(PathsPage.BackupPathTextBox.Text))
            {
                error = true;
                ShowPathError(PathsPage.BackupPathTextBox, true);
            }

            if (error)
            {
                e.Cancel = true;
                PathsRadioButton.Checked = true;
                return;
            }
            else
            {
                foreach (var tb in ExePathTextBoxes)
                {
                    tb.BackColor = SystemColors.Window;
                    tb.Tag = PathError.False;
                }
                PathsPage.BackupPathTextBox.BackColor = SystemColors.Window;
                PathsPage.BackupPathTextBox.Tag = PathError.False;
                ErrorLabel.Hide();

                // Extremely petty visual nicety - makes the error stuff go away before the form closes
                Refresh();
                PathsPage.Refresh();
            }

            #endregion

            #region Paths page

            OutConfig.T1Exe = PathsPage.Thief1ExePathTextBox.Text.Trim();
            OutConfig.T2Exe = PathsPage.Thief2ExePathTextBox.Text.Trim();
            OutConfig.T3Exe = PathsPage.Thief3ExePathTextBox.Text.Trim();
            OutConfig.SS2Exe = PathsPage.SS2ExePathTextBox.Text.Trim();

            OutConfig.SteamExe = PathsPage.SteamExeTextBox.Text.Trim();
            OutConfig.LaunchGamesWithSteam = PathsPage.LaunchTheseGamesThroughSteamCheckBox.Checked;
            OutConfig.T1UseSteam = PathsPage.T1UseSteamCheckBox.Checked;
            OutConfig.T2UseSteam = PathsPage.T2UseSteamCheckBox.Checked;
            OutConfig.T3UseSteam = PathsPage.T3UseSteamCheckBox.Checked;
            OutConfig.SS2UseSteam = PathsPage.SS2UseSteamCheckBox.Checked;

            OutConfig.FMsBackupPath = PathsPage.BackupPathTextBox.Text.Trim();

            // Manual so we can use Trim() on each
            OutConfig.FMArchivePaths.Clear();
            foreach (string path in PathsPage.FMArchivePathsListBox.Items) OutConfig.FMArchivePaths.Add(path.Trim());

            OutConfig.FMArchivePathsIncludeSubfolders = PathsPage.IncludeSubfoldersCheckBox.Checked;

            #endregion

            if (Startup)
            {
                OutConfig.Language = LangComboBox.SelectedBackingItem();
            }
            else
            {
                #region FM Display page

                #region Game organization

                OutConfig.GameOrganization = FMDisplayPage.OrganizeGamesByTabRadioButton.Checked
                        ? GameOrganization.ByTab
                        : GameOrganization.OneList;

                OutConfig.UseShortGameTabNames = FMDisplayPage.UseShortGameTabNamesCheckBox.Checked;

                #endregion

                #region Articles

                OutConfig.EnableArticles = FMDisplayPage.EnableIgnoreArticlesCheckBox.Checked;

                var retArticles = FMDisplayPage.ArticlesTextBox.Text
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

                OutConfig.Articles.ClearAndAdd(retArticles);

                OutConfig.MoveArticlesToEnd = FMDisplayPage.MoveArticlesToEndCheckBox.Checked;

                #endregion

                #region Date format

                OutConfig.DateFormat =
                    FMDisplayPage.DateCurrentCultureShortRadioButton.Checked ? DateFormat.CurrentCultureShort :
                    FMDisplayPage.DateCurrentCultureLongRadioButton.Checked ? DateFormat.CurrentCultureLong :
                    DateFormat.Custom;

                OutConfig.DateCustomFormat1 = FMDisplayPage.Date1ComboBox.SelectedItem.ToString();
                OutConfig.DateCustomSeparator1 = FMDisplayPage.DateSeparator1TextBox.Text;
                OutConfig.DateCustomFormat2 = FMDisplayPage.Date2ComboBox.SelectedItem.ToString();
                OutConfig.DateCustomSeparator2 = FMDisplayPage.DateSeparator2TextBox.Text;
                OutConfig.DateCustomFormat3 = FMDisplayPage.Date3ComboBox.SelectedItem.ToString();
                OutConfig.DateCustomSeparator3 = FMDisplayPage.DateSeparator3TextBox.Text;
                OutConfig.DateCustomFormat4 = FMDisplayPage.Date4ComboBox.SelectedItem.ToString();

                var formatString = FMDisplayPage.Date1ComboBox.SelectedItem +
                                   FMDisplayPage.DateSeparator1TextBox.Text.EscapeAllChars() +
                                   FMDisplayPage.Date2ComboBox.SelectedItem +
                                   FMDisplayPage.DateSeparator2TextBox.Text.EscapeAllChars() +
                                   FMDisplayPage.Date3ComboBox.SelectedItem +
                                   FMDisplayPage.DateSeparator3TextBox.Text.EscapeAllChars() +
                                   FMDisplayPage.Date4ComboBox.SelectedItem;

                try
                {
                    _ = _exampleDate.ToString(formatString);
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

                OutConfig.RatingDisplayStyle = FMDisplayPage.RatingNDLDisplayStyleRadioButton.Checked
                    ? RatingDisplayStyle.NewDarkLoader
                    : RatingDisplayStyle.FMSel;
                OutConfig.RatingUseStars = FMDisplayPage.RatingUseStarsCheckBox.Checked;

                #endregion

                #endregion

                #region Other page

                #region File conversion

                OutConfig.ConvertWAVsTo16BitOnInstall = OtherPage.ConvertWAVsTo16BitOnInstallCheckBox.Checked;
                OutConfig.ConvertOGGsToWAVsOnInstall = OtherPage.ConvertOGGsToWAVsOnInstallCheckBox.Checked;

                #endregion

                #region Uninstalling FMs

                OutConfig.ConfirmUninstall = OtherPage.ConfirmUninstallCheckBox.Checked;

                OutConfig.BackupFMData = OtherPage.BackupSavesAndScreensOnlyRadioButton.Checked
                    ? BackupFMData.SavesAndScreensOnly
                    : BackupFMData.AllChangedFiles;

                OutConfig.BackupAlwaysAsk = OtherPage.BackupAlwaysAskCheckBox.Checked;

                #endregion

                OutConfig.Language = LangComboBox.SelectedBackingItem();

                OutConfig.WebSearchUrl = OtherPage.WebSearchUrlTextBox.Text;

                OutConfig.ConfirmPlayOnDCOrEnter = OtherPage.ConfirmPlayOnDCOrEnterCheckBox.Checked;

                #region Show/hide UI elements

                OutConfig.HideUninstallButton = OtherPage.HideUninstallButtonCheckBox.Checked;
                OutConfig.HideFMListZoomButtons = OtherPage.HideFMListZoomButtonsCheckBox.Checked;

                #endregion

                OutConfig.ReadmeUseFixedWidthFont = OtherPage.ReadmeFixedWidthFontCheckBox.Checked;

                #endregion
            }
        }

        #region Page selection handler

        // This is to handle keyboard "clicks"
        private void PageRadioButtons_Click(object sender, EventArgs e) => ((RadioButtonCustom)sender).Checked = true;

        // This is for mouse use, to give a snappier experience, we change on MouseDown
        private void Paths_RadioButton_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left) ((RadioButtonCustom)sender).Checked = true;
        }

        private void PathsRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (EventsDisabled) return;

            var s = (RadioButtonCustom)sender;
            if (!s.Checked) return;

            using (new DisableEvents(this)) foreach (var b in PageRadioButtons) if (s != b) b.Checked = false;

            ShowPage(Array.IndexOf(PageRadioButtons, s));
        }

        private void SetPageScrollPos(ISettingsPage page)
        {
            // TODO: This is the only place these globals are used other than the ctor and I think they can be removed
            int? pos =
                page == PathsPage ? _inPathsVScrollPos :
                page == FMDisplayPage ? _inFMDisplayVScrollPos :
                page == OtherPage ? _inOtherVScrollPos :
                (int?)null;

            Debug.Assert(pos != null, nameof(pos) + " is null: settings page is not being handled in " + nameof(SetPageScrollPos));

            page.SetVScrollPos((int)pos);
        }

        private void ShowPage(int index, bool initialCall = false)
        {
            if (Pages[index].IsVisible) return;

            if (Startup)
            {
                // Don't bother with position saving if this is the Initial Settings window
                PathsPage.Show();
            }
            else
            {
                int pagesLength = Pages.Length;
                if (index < 0 || index > pagesLength - 1) return;

                bool pagePosWasStored = PageVScrollValues[index] != null;
                try
                {
                    if (!initialCall && pagePosWasStored) this.SuspendDrawing();

                    Pages[index].ShowPage();
                    for (int i = 0; i < pagesLength; i++) if (i != index) Pages[i].HidePage();

                    // Lazy-load for faster initial startup
                    if (pagePosWasStored)
                    {
                        SetPageScrollPos(Pages[index]);
                        if (!initialCall)
                        {
                            // Infuriating hack to get the scroll bar to show up in the right position (the content
                            // already does)
                            Pages[index].HidePage();
                            Pages[index].ShowPage();
                        }

                        PageVScrollValues[index] = null;
                    }
                }
                finally
                {
                    if (!initialCall && pagePosWasStored) this.ResumeDrawing();
                }
            }
        }

        #endregion

        #region Paths page

        private void ExePathTextBoxes_Leave(object sender, EventArgs e)
        {
            var s = (TextBox)sender;
            ShowPathError(s, !s.Text.IsEmpty() && !File.Exists(s.Text));
        }

        private void ExePathBrowseButtons_Click(object sender, EventArgs e)
        {
            var tb =
                sender == PathsPage.Thief1ExePathBrowseButton ? PathsPage.Thief1ExePathTextBox :
                sender == PathsPage.Thief2ExePathBrowseButton ? PathsPage.Thief2ExePathTextBox :
                sender == PathsPage.Thief3ExePathBrowseButton ? PathsPage.Thief3ExePathTextBox :
                sender == PathsPage.SS2ExePathBrowseButton ? PathsPage.SS2ExePathTextBox :
                PathsPage.SteamExeTextBox;

            string initialPath = "";
            try
            {
                initialPath = Path.GetDirectoryName(tb.Text);
            }
            catch
            {
                // ignore
            }

            var (result, fileName) = BrowseForExeFile(initialPath);
            if (result == DialogResult.OK) tb.Text = fileName ?? "";

            ShowPathError(tb, !tb.Text.IsEmpty() && !File.Exists(tb.Text));
        }

        private void BackupPathTextBox_Leave(object sender, EventArgs e)
        {
            var s = (TextBox)sender;
            ShowPathError(s, !Directory.Exists(s.Text));
        }

        private void BackupPathBrowseButton_Click(object sender, EventArgs e)
        {
            var tb = PathsPage.BackupPathTextBox;

            using (var d = new AutoFolderBrowserDialog())
            {
                d.InitialDirectory = tb.Text;
                d.MultiSelect = false;
                if (d.ShowDialog() == DialogResult.OK) tb.Text = d.DirectoryName;
            }

            ShowPathError(tb, !Directory.Exists(tb.Text));
        }

        private static (DialogResult Result, string FileName)
        BrowseForExeFile(string initialPath)
        {
            using var dialog = new OpenFileDialog
            {
                InitialDirectory = initialPath,
                Filter = LText.BrowseDialogs.ExeFiles + @"|*.exe"
            };
            return (dialog.ShowDialog(), dialog.FileName);
        }

        private void SteamExeTextBox_TextChanged(object sender, EventArgs e)
        {
            PathsPage.LaunchTheseGamesThroughSteamPanel.Enabled = !PathsPage.SteamExeTextBox.Text.IsWhiteSpace();
        }


        #region Archive paths

        private bool FMArchivePathExistsInBox(string path)
        {
            foreach (var item in PathsPage.FMArchivePathsListBox.Items)
            {
                if (item.ToString().EqualsI(path)) return true;
            }

            return false;
        }

        private void AddFMArchivePathButton_Click(object sender, EventArgs e)
        {
            using var d = new AutoFolderBrowserDialog();

            var lb = PathsPage.FMArchivePathsListBox;
            var initDir =
                lb.SelectedIndex > -1 ? lb.SelectedItem.ToString() :
                lb.Items.Count > 0 ? lb.Items[lb.Items.Count - 1].ToString() :
                "";
            if (!initDir.IsWhiteSpace())
            {
                try
                {
                    d.InitialDirectory = Path.GetDirectoryName(initDir);
                }
                catch
                {
                    // ignore
                }
            }
            d.MultiSelect = true;
            if (d.ShowDialog() == DialogResult.OK)
            {
                foreach (var dir in d.DirectoryNames)
                {
                    if (!FMArchivePathExistsInBox(dir)) PathsPage.FMArchivePathsListBox.Items.Add(dir);
                }
            }
        }

        private void RemoveFMArchivePathButton_Click(object sender, EventArgs e) => PathsPage.FMArchivePathsListBox.RemoveAndSelectNearest();

        #endregion

        #endregion

        #region FM Display page

        #region Articles

        private void ArticlesCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            FMDisplayPage.ArticlesTextBox.Enabled = FMDisplayPage.EnableIgnoreArticlesCheckBox.Checked;
            FMDisplayPage.MoveArticlesToEndCheckBox.Enabled = FMDisplayPage.EnableIgnoreArticlesCheckBox.Checked;
        }

        private void ArticlesTextBox_Leave(object sender, EventArgs e) => FormatArticles();

        private void FormatArticles()
        {
            var articles = FMDisplayPage.ArticlesTextBox.Text;

            // Copied wholesale from Autovid, ridiculous looking, but works

            if (articles.IsWhiteSpace())
            {
                FMDisplayPage.ArticlesTextBox.Text = "";
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

            FMDisplayPage.ArticlesTextBox.Text = articles;
        }

        #endregion

        #region Date display

        private void UpdateCustomExampleDate()
        {

            var formatString = FMDisplayPage.Date1ComboBox.SelectedItem +
                               FMDisplayPage.DateSeparator1TextBox.Text.EscapeAllChars() +
                               FMDisplayPage.Date2ComboBox.SelectedItem +
                               FMDisplayPage.DateSeparator2TextBox.Text.EscapeAllChars() +
                               FMDisplayPage.Date3ComboBox.SelectedItem +
                               FMDisplayPage.DateSeparator3TextBox.Text.EscapeAllChars() +
                               FMDisplayPage.Date4ComboBox.SelectedItem;

            try
            {
                FMDisplayPage.PreviewDateLabel.Text = _exampleDate.ToString(formatString);
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

        private void DateShortAndLongRadioButtons_CheckedChanged(object sender, EventArgs e)
        {
            FMDisplayPage.DateCustomFormatPanel.Enabled = false;
            FMDisplayPage.PreviewDateLabel.Text = sender == FMDisplayPage.DateCurrentCultureShortRadioButton
                ? _exampleDate.ToShortDateString()
                : _exampleDate.ToLongDateString();
        }

        private void DateCustomRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            var s = (RadioButton)sender;
            FMDisplayPage.DateCustomFormatPanel.Enabled = s.Checked;
            if (s.Checked) UpdateCustomExampleDate();
        }

        private void DateCustomValue_Changed(object sender, EventArgs e)
        {
            if (FMDisplayPage.DateCustomFormatPanel.Enabled) UpdateCustomExampleDate();
        }

        #endregion

        #region Rating display

        private void RatingOutOfTenRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (EventsDisabled) return;
            if (FMDisplayPage.RatingNDLDisplayStyleRadioButton.Checked)
            {
                FMDisplayPage.RatingUseStarsCheckBox.Enabled = false;
                FMDisplayPage.RatingExamplePictureBox.Image = Images.RatingExample_NDL;
            }
        }

        private void RatingOutOfFiveRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (EventsDisabled) return;
            if (FMDisplayPage.RatingFMSelDisplayStyleRadioButton.Checked)
            {
                FMDisplayPage.RatingUseStarsCheckBox.Enabled = true;
                FMDisplayPage.RatingExamplePictureBox.Image = FMDisplayPage.RatingUseStarsCheckBox.Checked
                    ? Images.RatingExample_FMSel_Stars
                    : Images.RatingExample_FMSel_Number;
            }
        }

        private void RatingUseStarsCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (EventsDisabled) return;
            FMDisplayPage.RatingExamplePictureBox.Image = FMDisplayPage.RatingUseStarsCheckBox.Checked
                ? Images.RatingExample_FMSel_Stars
                : Images.RatingExample_FMSel_Number;
        }

        #endregion

        #endregion

        #region Other page

        private void WebSearchURLResetButton_Click(object sender, EventArgs e) => OtherPage.WebSearchUrlTextBox.Text = Defaults.WebSearchUrl;

        private void LanguageComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (EventsDisabled) return;
            var s = LangComboBox;
            try
            {
                Ini.Ini.ReadLocalizationIni(Path.Combine(Paths.Languages, s.SelectedBackingItem() + ".ini"));
                Localize();
                if (!Startup) LocalizeOwnerForm();
            }
            catch (Exception ex)
            {
                Log("Exception in language reading", ex);
            }
        }

        #endregion

        private void ShowPathError(TextBox textBox, bool shown)
        {
            textBox.BackColor = shown ? Color.MistyRose : SystemColors.Window;
            textBox.Tag = shown ? PathError.True : PathError.False;

            if (!shown)
            {
                foreach (var tb in ExePathTextBoxes)
                {
                    if (tb.Tag is PathError gError && gError == PathError.True) return;
                }
            }

            ErrorLabel.Text = shown ? LText.SettingsWindow.Paths_ErrorSomePathsAreInvalid : "";
            ErrorLabel.Visible = shown;
        }

        private void SettingsForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                if (MainSplitContainer.IsSplitterFixed)
                {
                    MainSplitContainer.CancelResize();
                    e.SuppressKeyPress = true;
                }
                else
                {
                    DialogResult = DialogResult.Cancel;
                    Close();
                }
            }
        }

        private void LocalizeOwnerForm()
        {
            try { OwnerForm.Localize(); }
            catch (Exception ex) { Log("OwnerForm might be uninitialized or somethin' again - not supposed to happen", ex); }
        }
    }
}
