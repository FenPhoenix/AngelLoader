// TODO: @IO_SAFETY: @Robustness: Check paths and exes for conflicts, duplicates, disallowed locations, etc.

using System;
using System.Collections;
using System.Collections.Specialized;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using AngelLoader.DataClasses;
using AngelLoader.Forms.CustomControls;
using AngelLoader.Forms.CustomControls.SettingsPages;
using AngelLoader.WinAPI.Dialogs;
using static AngelLoader.Forms.CustomControls.SettingsPages.Interfaces;
using static AngelLoader.GameSupport;
using static AngelLoader.Logger;
using static AngelLoader.Misc;

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

        private readonly ILocalizable? _ownerForm;

        private readonly bool _startup;
        private readonly bool _cleanStart;

        #region Copies of passed-in data

        private readonly string _inLanguage;
        private readonly LText_Class _inLText;

        private readonly int _inPathsVScrollPos;
        private readonly int _inFMDisplayVScrollPos;
        private readonly int _inOtherVScrollPos;

        #endregion

        private readonly RadioButtonCustom[] PageRadioButtons;
        private readonly ISettingsPage[] Pages;
        private readonly int?[] _pageVScrollValues;

        private readonly TextBox[] ExePathTextBoxes;
        private readonly TextBox[] ErrorableTextBoxes;

        private readonly Label[] GameExeLabels;
        private readonly TextBox[] GameExeTextBoxes;
        private readonly Button[] GameExeBrowseButtons;
        private readonly CheckBox[] GameUseSteamCheckBoxes;

        // August 4 is chosen more-or-less randomly, but both its name and its number are different short vs. long
        // (Aug vs. August; 8 vs. 08), and the same thing with 4 (4 vs. 04).
        private readonly DateTime _exampleDate = new DateTime(DateTime.Now.Year, 8, 4);

        private readonly ComboBoxCustom LangComboBox;
        private readonly GroupBox LangGroupBox;

        private readonly PathsPage PathsPage;
        private readonly FMDisplayPage FMDisplayPage;
        private readonly OtherPage OtherPage;

        private enum PathError { True, False }

        public bool EventsDisabled { get; set; }

        #endregion

        public readonly ConfigData OutConfig;

        // @CAN_RUN_BEFORE_VIEW_INIT
        internal SettingsForm(ILocalizable? ownerForm, ConfigData config, bool startup, bool cleanStart)
        {
#if DEBUG
            InitializeComponent();
#else
            InitComponentManual();
#endif

            // Needed for Esc-to-cancel-drag and stuff
            KeyPreview = true;

            _startup = startup;
            _cleanStart = cleanStart;
            _ownerForm = ownerForm;

            #region Init copies of passed-in data

            _inLanguage = config.Language;
            // Even though this looks like it should be a reference and therefore not work for being a separate
            // object, it somehow does, because I guess we new up LText on read and break the reference and then
            // this copy becomes its own copy...? I don't like that I didn't know that...
            _inLText = LText;

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

            // @GENGAMES (Settings): Begin

            GameExeLabels = new[]
            {
                PathsPage.Thief1ExePathLabel,
                PathsPage.Thief2ExePathLabel,
                PathsPage.Thief3ExePathLabel,
                PathsPage.SS2ExePathLabel
            };
            GameExeTextBoxes = new[]
            {
                PathsPage.Thief1ExePathTextBox,
                PathsPage.Thief2ExePathTextBox,
                PathsPage.Thief3ExePathTextBox,
                PathsPage.SS2ExePathTextBox
            };
            GameExeBrowseButtons = new[]
            {
                PathsPage.Thief1ExePathBrowseButton,
                PathsPage.Thief2ExePathBrowseButton,
                PathsPage.Thief3ExePathBrowseButton,
                PathsPage.SS2ExePathBrowseButton
            };
            GameUseSteamCheckBoxes = new[]
            {
                PathsPage.Thief1UseSteamCheckBox,
                PathsPage.Thief2UseSteamCheckBox,
                PathsPage.Thief3UseSteamCheckBox,
                PathsPage.SS2UseSteamCheckBox
            };

            // TODO: @GENGAMES (Settings): We've traded one form of jank for another
            // In our quest to be fast and lean, we're using arrays instead of lists here. That means we have to
            // do this hideous SupportedGameCount + n thing instead of just being able to say AddRange(games)
            // then Add(whatever else) afterwards.
            // Still, this jank is now at least localized to this one small area and we'll know immediately if we
            // get it wrong (we'll crash on OOB).

            #region Exe path textboxes

            ExePathTextBoxes = new TextBox[SupportedGameCount + 1];
            Array.Copy(GameExeTextBoxes, 0, ExePathTextBoxes, 0, SupportedGameCount);

            ExePathTextBoxes[SupportedGameCount] = PathsPage.SteamExeTextBox;

            #endregion

            #region Errorable textboxes

            ErrorableTextBoxes = new TextBox[SupportedGameCount + 2];
            Array.Copy(GameExeTextBoxes, 0, ErrorableTextBoxes, 0, SupportedGameCount);

            ErrorableTextBoxes[SupportedGameCount] = PathsPage.SteamExeTextBox;
            ErrorableTextBoxes[SupportedGameCount + 1] = PathsPage.BackupPathTextBox;

            #endregion

            // @GENGAMES (Settings): End

            PageRadioButtons = new[] { PathsRadioButton, FMDisplayRadioButton, OtherRadioButton };

            // These are nullable because null values get put INTO them later. So not a mistake to fill them with
            // non-nullable ints right off the bat.
            _pageVScrollValues = new int?[]
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
                        case SettingsTab.Paths:
                        default:
                            PathsRadioButton.Checked = true;
                            break;
                    }
                }
            }

            #endregion

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

            for (int i = 0; i < SupportedGameCount; i++)
            {
                GameIndex gameIndex = (GameIndex)i;
                GameExeTextBoxes[i].Text = config.GetGameExe(gameIndex);
                GameUseSteamCheckBoxes[i].Checked = config.GetUseSteamSwitch(gameIndex);
            }

            PathsPage.SteamExeTextBox.Text = config.SteamExe;
            PathsPage.LaunchTheseGamesThroughSteamPanel.Enabled = !PathsPage.SteamExeTextBox.Text.IsWhiteSpace();
            PathsPage.LaunchTheseGamesThroughSteamCheckBox.Checked = config.LaunchGamesWithSteam;
            SetUseSteamGameCheckBoxesEnabled(config.LaunchGamesWithSteam);

            PathsPage.BackupPathTextBox.Text = config.FMsBackupPath;

            PathsPage.FMArchivePathsListBox.Items.Clear();
            foreach (string path in config.FMArchivePaths) PathsPage.FMArchivePathsListBox.Items.Add(path);

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
                        FMDisplayPage.UseShortGameTabNamesCheckBox.Enabled = true;
                        break;
                    case GameOrganization.OneList:
                    default:
                        FMDisplayPage.OrganizeGamesInOneListRadioButton.Checked = true;
                        FMDisplayPage.UseShortGameTabNamesCheckBox.Enabled = false;
                        break;
                }

                FMDisplayPage.UseShortGameTabNamesCheckBox.Checked = config.UseShortGameTabNames;

                #endregion

                #region Articles

                FMDisplayPage.EnableIgnoreArticlesCheckBox.Checked = config.EnableArticles;

                for (int i = 0; i < config.Articles.Count; i++)
                {
                    string article = config.Articles[i];
                    if (i > 0) FMDisplayPage.ArticlesTextBox.Text += ", ";
                    FMDisplayPage.ArticlesTextBox.Text += article;
                }

                FMDisplayPage.MoveArticlesToEndCheckBox.Checked = config.MoveArticlesToEnd;

                SetArticlesEnabledState();

                #endregion

                #region Date format

                object[] dateFormatList = ValidDateFormatList.Cast<object>().ToArray();
                FMDisplayPage.Date1ComboBox.Items.AddRange(dateFormatList);
                FMDisplayPage.Date2ComboBox.Items.AddRange(dateFormatList);
                FMDisplayPage.Date3ComboBox.Items.AddRange(dateFormatList);
                FMDisplayPage.Date4ComboBox.Items.AddRange(dateFormatList);

                string d1 = config.DateCustomFormat1;
                string s1 = config.DateCustomSeparator1;
                string d2 = config.DateCustomFormat2;
                string s2 = config.DateCustomSeparator2;
                string d3 = config.DateCustomFormat3;
                string s3 = config.DateCustomSeparator3;
                string d4 = config.DateCustomFormat4;

                FMDisplayPage.Date1ComboBox.SelectedItem = FMDisplayPage.Date1ComboBox.Items.Contains(d1) ? d1 : Defaults.DateCustomFormat1;
                FMDisplayPage.DateSeparator1TextBox.Text = s1;
                FMDisplayPage.Date2ComboBox.SelectedItem = FMDisplayPage.Date2ComboBox.Items.Contains(d2) ? d2 : Defaults.DateCustomFormat2;
                FMDisplayPage.DateSeparator2TextBox.Text = s2;
                FMDisplayPage.Date3ComboBox.SelectedItem = FMDisplayPage.Date3ComboBox.Items.Contains(d3) ? d3 : Defaults.DateCustomFormat3;
                FMDisplayPage.DateSeparator3TextBox.Text = s3;
                FMDisplayPage.Date4ComboBox.SelectedItem = FMDisplayPage.Date4ComboBox.Items.Contains(d4) ? d4 : Defaults.DateCustomFormat4;

                // This comes last so that all the custom data is in place for the preview date to use
                switch (config.DateFormat)
                {
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
                    case DateFormat.CurrentCultureShort:
                    default:
                        FMDisplayPage.DateCurrentCultureShortRadioButton.Checked = true;
                        FMDisplayPage.DateCustomFormatPanel.Enabled = false;
                        FMDisplayPage.PreviewDateLabel.Text = _exampleDate.ToShortDateString();
                        break;
                }

                #endregion

                #region Recent FMs

                FMDisplayPage.RecentFMsNumericUpDown.Maximum = Defaults.MaxDaysRecent;
                FMDisplayPage.RecentFMsNumericUpDown.Value = config.DaysRecent;

                #endregion

                #region Rating display style

                switch (config.RatingDisplayStyle)
                {
                    case RatingDisplayStyle.NewDarkLoader:
                        FMDisplayPage.RatingNDLDisplayStyleRadioButton.Checked = true;
                        break;
                    case RatingDisplayStyle.FMSel:
                    default:
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
                    default:
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

            for (int i = 0; i < SupportedGameCount; i++)
            {
                GameExeTextBoxes[i].Leave += ExePathTextBoxes_Leave;
                GameExeBrowseButtons[i].Click += ExePathBrowseButtons_Click;
            }

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
                FMDisplayPage.OrganizeGamesByTabRadioButton.CheckedChanged += GameOrganizationRadioButtons_CheckedChanged;
                FMDisplayPage.OrganizeGamesInOneListRadioButton.CheckedChanged += GameOrganizationRadioButtons_CheckedChanged;

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

            // This could maybe feel intrusive if we do it every time we open, so only do it on startup. It's
            // much more important / useful to do it on startup, because we're likely only to open on startup
            // if there's already an error. But, we also want to suppress errors if we're starting for the first
            // time ever. In that case, invalid fields aren't conceptually "errors", but rather the user just
            // hasn't filled them in yet. We'll error on OK click if we have to, but present a pleasanter UX
            // prior to then.
            if (_startup && !_cleanStart) CheckForErrors();
        }

        private void SettingsForm_Shown(object sender, EventArgs e)
        {
            // We have to do this here, in _Shown, otherwise it doesn't do its initial layout and might miss if
            // there's supposed to be scroll bars or whatever else... this makes it visually correct. Don't ask
            // questions.
            PathsPage.DoLayout = true;
            PathsPage.FlowLayoutPanel1.PerformLayout();
        }

        private void SetUseSteamGameCheckBoxesEnabled(bool enabled)
        {
            for (int i = 0; i < SupportedGameCount; i++)
            {
                GameUseSteamCheckBoxes[i].Enabled = enabled;
            }
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
                Text = _startup ? LText.SettingsWindow.StartupTitleText : LText.SettingsWindow.TitleText;

                OKButton.Text = LText.Global.OK;
                Cancel_Button.Text = LText.Global.Cancel;

                #region Paths tab

                PathsRadioButton.Text = _startup
                    ? LText.SettingsWindow.InitialSettings_TabText
                    : LText.SettingsWindow.Paths_TabText;

                for (int i = 0; i < SupportedGameCount; i++)
                {
                    GameIndex gameIndex = (GameIndex)i;
                    GameExeLabels[i].Text = GetLocalizedGameNameColon(gameIndex);
                    GameUseSteamCheckBoxes[i].Text = GetLocalizedGameName(gameIndex);
                    GameExeBrowseButtons[i].SetTextForTextBoxButtonCombo(GameExeTextBoxes[i], LText.Global.BrowseEllipses);
                }

                PathsPage.PathsToGameExesGroupBox.Text = LText.SettingsWindow.Paths_PathsToGameExes;

                PathsPage.SteamOptionsGroupBox.Text = LText.SettingsWindow.Paths_SteamOptions;
                PathsPage.SteamExeLabel.Text = LText.SettingsWindow.Paths_PathToSteamExecutable;
                PathsPage.LaunchTheseGamesThroughSteamCheckBox.Text = LText.SettingsWindow.Paths_LaunchTheseGamesThroughSteam;

                PathsPage.OtherGroupBox.Text = LText.SettingsWindow.Paths_Other;
                PathsPage.BackupPathLabel.Text = LText.SettingsWindow.Paths_BackupPath;

                PathsPage.BackupPathHelpLabel.Text = LText.SettingsWindow.Paths_BackupPath_Info;
                // Required for the startup version where the lang box is on the same page as paths!
                PathsPage.FlowLayoutPanel1.PerformLayout();

                PathsPage.BackupPathBrowseButton.SetTextForTextBoxButtonCombo(PathsPage.BackupPathTextBox, LText.Global.BrowseEllipses);
                PathsPage.SteamExeBrowseButton.SetTextForTextBoxButtonCombo(PathsPage.SteamExeTextBox, LText.Global.BrowseEllipses);

                PathsPage.GameRequirementsLabel.Text =
                    LText.SettingsWindow.Paths_DarkEngineGamesRequireNewDark + Environment.NewLine +
                    LText.SettingsWindow.Paths_Thief3RequiresSneakyUpgrade;

                PathsPage.FMArchivePathsGroupBox.Text = LText.SettingsWindow.Paths_FMArchivePaths;
                PathsPage.IncludeSubfoldersCheckBox.Text = LText.SettingsWindow.Paths_IncludeSubfolders;
                MainToolTip.SetToolTip(PathsPage.AddFMArchivePathButton, LText.SettingsWindow.Paths_AddArchivePathToolTip);
                MainToolTip.SetToolTip(PathsPage.RemoveFMArchivePathButton, LText.SettingsWindow.Paths_RemoveArchivePathToolTip);

                #endregion

                if (_startup)
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
                    FMDisplayPage.OrganizeGamesInOneListRadioButton.Text = LText.SettingsWindow.FMDisplay_GameOrganizationOneList;

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

                    FMDisplayPage.RecentFMsGroupBox.Text = LText.SettingsWindow.FMDisplay_RecentFMs;
                    FMDisplayPage.RecentFMsLabel.Text = LText.SettingsWindow.FMDisplay_RecentFMs_MaxDays;

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

                ErrorLabel.Text = LText.SettingsWindow.Paths_ErrorSomePathsAreInvalid;
                MainErrorProvider.SetError(ErrorLabel, LText.AlertMessages.Error);
            }
            finally
            {
                if (suspendResume) this.ResumeDrawing();
            }
        }

        /// <summary>
        /// Sets or removes error visuals as necessary, and returns a bool for whether there were errors or not.
        /// </summary>
        /// <returns><see langword="true"/> if there were errors, <see langword="false"/> otherwise.</returns>
        private bool CheckForErrors()
        {
            bool error = false;
            //bool backupPathIsArchivePathError = false;

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

            // Disabled for now... as it's a restriction tightening thing...
            //foreach (string item in PathsPage.FMArchivePathsListBox.Items)
            //{
            //    if (PathsPage.BackupPathTextBox.Text.PathEqualsI_Dir(item))
            //    {
            //        error = true;
            //        backupPathIsArchivePathError = true;
            //        ShowPathError(PathsPage.BackupPathTextBox, true);
            //    }
            //}

            if (error)
            {
                // Currently, all errors happen on the Paths page, so go to that page automatically.
                PathsRadioButton.Checked = true;

                // One user missed the error highlight on a textbox because it was scrolled offscreen, and was
                // confused as to why there was an error. So scroll the first error-highlighted textbox onscreen
                // to make it clear.
                foreach (TextBox tb in ErrorableTextBoxes)
                {
                    if (PathErrorIsSet(tb))
                    {
                        PathsPage.PagePanel.ScrollControlIntoView(tb);
                        break;
                    }
                }

                // See above
                //if (backupPathIsArchivePathError)
                //{
                //    MessageBox.Show(
                //        LText.AlertMessages.Settings_Paths_BackupPathIsAnArchivePath,
                //        LText.AlertMessages.Alert,
                //        MessageBoxButtons.OK,
                //        MessageBoxIcon.Error);
                //}

                return true;
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

            return false;
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
            OutConfig.SettingsPathsVScrollPos = _pageVScrollValues[0] ?? PathsPage.GetVScrollPos();
            OutConfig.SettingsFMDisplayVScrollPos = _pageVScrollValues[1] ?? FMDisplayPage.GetVScrollPos();
            OutConfig.SettingsOtherVScrollPos = _pageVScrollValues[2] ?? OtherPage.GetVScrollPos();

            #endregion

            #region Cancel

            if (DialogResult != DialogResult.OK)
            {
                if (!_startup && !LangComboBox.SelectedBackingItem().EqualsI(_inLanguage))
                {
                    try
                    {
                        // It's actually totally fine that this one is a reference.
                        LText = _inLText;
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

            if (!_startup) FormatArticles();

            if (CheckForErrors())
            {
                e.Cancel = true;
                return;
            }

            #region Paths page

            for (int i = 0; i < SupportedGameCount; i++)
            {
                GameIndex gameIndex = (GameIndex)i;
                OutConfig.SetGameExe(gameIndex, GameExeTextBoxes[i].Text.Trim());
                OutConfig.SetUseSteamSwitch(gameIndex, GameUseSteamCheckBoxes[i].Checked);
            }

            OutConfig.SteamExe = PathsPage.SteamExeTextBox.Text.Trim();
            OutConfig.LaunchGamesWithSteam = PathsPage.LaunchTheseGamesThroughSteamCheckBox.Checked;

            OutConfig.FMsBackupPath = PathsPage.BackupPathTextBox.Text.Trim();

            // Manual so we can use Trim() on each
            OutConfig.FMArchivePaths.Clear();
            foreach (string path in PathsPage.FMArchivePathsListBox.Items) OutConfig.FMArchivePaths.Add(path.Trim());

            OutConfig.FMArchivePathsIncludeSubfolders = PathsPage.IncludeSubfoldersCheckBox.Checked;

            #endregion

            if (_startup)
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
                    .Split(CA_Comma, StringSplitOptions.RemoveEmptyEntries)
                    .Distinct(StringComparer.InvariantCultureIgnoreCase)
                    .ToList();

                // Just in case
                for (int i = 0; i < retArticles.Count; i++)
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

                #region Rating display style

                OutConfig.RatingDisplayStyle = FMDisplayPage.RatingNDLDisplayStyleRadioButton.Checked
                    ? RatingDisplayStyle.NewDarkLoader
                    : RatingDisplayStyle.FMSel;
                OutConfig.RatingUseStars = FMDisplayPage.RatingUseStarsCheckBox.Checked;

                #endregion

                #region Date format

                OutConfig.DateFormat =
                    FMDisplayPage.DateCurrentCultureShortRadioButton.Checked ? DateFormat.CurrentCultureShort :
                    FMDisplayPage.DateCurrentCultureLongRadioButton.Checked ? DateFormat.CurrentCultureLong :
                    DateFormat.Custom;

                bool customDateSuccess = FormatAndTestDate(out string customDateString, out _);
                if (customDateSuccess)
                {
                    OutConfig.DateCustomFormatString = customDateString;
                }
                else
                {
                    SetCustomDateFormatFieldsToDefault();
                    OutConfig.DateCustomFormatString = GetFormattedCustomDateString();
                }

                OutConfig.DateCustomFormat1 = FMDisplayPage.Date1ComboBox.SelectedItem.ToString();
                OutConfig.DateCustomSeparator1 = FMDisplayPage.DateSeparator1TextBox.Text;
                OutConfig.DateCustomFormat2 = FMDisplayPage.Date2ComboBox.SelectedItem.ToString();
                OutConfig.DateCustomSeparator2 = FMDisplayPage.DateSeparator2TextBox.Text;
                OutConfig.DateCustomFormat3 = FMDisplayPage.Date3ComboBox.SelectedItem.ToString();
                OutConfig.DateCustomSeparator3 = FMDisplayPage.DateSeparator3TextBox.Text;
                OutConfig.DateCustomFormat4 = FMDisplayPage.Date4ComboBox.SelectedItem.ToString();

                #endregion

                OutConfig.DaysRecent = (uint)FMDisplayPage.RecentFMsNumericUpDown.Value;

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
        private
#if !DEBUG
        static
#endif
        void PageRadioButtons_Click(object sender, EventArgs e) => ((RadioButtonCustom)sender).Checked = true;

        // This is for mouse use, to give a snappier experience, we change on MouseDown
        private
#if !DEBUG
        static
#endif
        void Paths_RadioButton_MouseDown(object sender, MouseEventArgs e)
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
            int? pos =
                page == PathsPage ? _inPathsVScrollPos :
                page == FMDisplayPage ? _inFMDisplayVScrollPos :
                page == OtherPage ? _inOtherVScrollPos :
                (int?)null;

            AssertR(pos != null, nameof(pos) + " is null: settings page is not being handled in " + nameof(SetPageScrollPos));

            page.SetVScrollPos((int)pos!);
        }

        private void ShowPage(int index, bool initialCall = false)
        {
            if (Pages[index].IsVisible) return;

            if (_startup)
            {
                // Don't bother with position saving if this is the Initial Settings window
                PathsPage.Show();
            }
            else
            {
                int pagesLength = Pages.Length;
                if (index < 0 || index > pagesLength - 1) return;

                bool pagePosWasStored = _pageVScrollValues[index] != null;
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

                        _pageVScrollValues[index] = null;
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
            TextBox? tb = null;
            for (int i = 0; i < SupportedGameCount; i++)
            {
                if (sender == GameExeBrowseButtons[i])
                {
                    tb = GameExeTextBoxes[i];
                    break;
                }
            }
            tb ??= PathsPage.SteamExeTextBox;

            string initialPath = "";
            try
            {
                initialPath = Path.GetDirectoryName(tb.Text) ?? "";
            }
            catch
            {
                // ignore
            }

            (DialogResult result, string fileName) = BrowseForExeFile(initialPath);
            if (result == DialogResult.OK) tb.Text = fileName;

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

            using (var d = new VistaFolderBrowserDialog())
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
                Filter = LText.BrowseDialogs.ExeFiles + "|*.exe"
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
            foreach (object item in PathsPage.FMArchivePathsListBox.Items)
            {
                if (item.ToString().PathEqualsI(path)) return true;
            }

            return false;
        }

        private void AddFMArchivePathButton_Click(object sender, EventArgs e)
        {
            using var d = new VistaFolderBrowserDialog();

            var lb = PathsPage.FMArchivePathsListBox;
            string initDir =
                lb.SelectedIndex > -1 ? lb.SelectedItem.ToString() :
                lb.Items.Count > 0 ? lb.Items[lb.Items.Count - 1].ToString() :
                "";
            if (!initDir.IsWhiteSpace())
            {
                try
                {
                    d.InitialDirectory = Path.GetDirectoryName(initDir) ?? "";
                }
                catch
                {
                    // ignore
                }
            }
            d.MultiSelect = true;
            if (d.ShowDialog() == DialogResult.OK)
            {
                foreach (string dir in d.DirectoryNames)
                {
                    if (!FMArchivePathExistsInBox(dir)) PathsPage.FMArchivePathsListBox.Items.Add(dir);
                }
            }
        }

        private void RemoveFMArchivePathButton_Click(object sender, EventArgs e) => PathsPage.FMArchivePathsListBox.RemoveAndSelectNearest();

        #endregion

        #endregion

        #region FM Display page

        private void GameOrganizationRadioButtons_CheckedChanged(object sender, EventArgs e)
        {
            FMDisplayPage.UseShortGameTabNamesCheckBox.Enabled = FMDisplayPage.OrganizeGamesByTabRadioButton.Checked;
        }

        #region Articles

        private void ArticlesCheckBox_CheckedChanged(object sender, EventArgs e) => SetArticlesEnabledState();

        private void SetArticlesEnabledState()
        {
            FMDisplayPage.ArticlesTextBox.Enabled = FMDisplayPage.EnableIgnoreArticlesCheckBox.Checked;
            FMDisplayPage.MoveArticlesToEndCheckBox.Enabled = FMDisplayPage.EnableIgnoreArticlesCheckBox.Checked;
        }

        private void ArticlesTextBox_Leave(object sender, EventArgs e) => FormatArticles();

        private void FormatArticles()
        {
            string articles = FMDisplayPage.ArticlesTextBox.Text;

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
            articles = articles.Trim(CA_Comma);

            string[] articlesArray = articles.Split(CA_CommaSpace).Distinct(StringComparer.InvariantCultureIgnoreCase).ToArray();

            articles = string.Join(", ", articlesArray);

            FMDisplayPage.ArticlesTextBox.Text = articles;
        }

        #endregion

        private void SetCustomDateFormatFieldsToDefault()
        {
            using (new DisableEvents(this))
            {
                FMDisplayPage.Date1ComboBox.SelectedItem = Defaults.DateCustomFormat1;
                FMDisplayPage.DateSeparator1TextBox.Text = Defaults.DateCustomSeparator1;
                FMDisplayPage.Date2ComboBox.SelectedItem = Defaults.DateCustomFormat2;
                FMDisplayPage.DateSeparator2TextBox.Text = Defaults.DateCustomSeparator2;
                FMDisplayPage.Date3ComboBox.SelectedItem = Defaults.DateCustomFormat3;
                FMDisplayPage.DateSeparator3TextBox.Text = Defaults.DateCustomSeparator3;
                FMDisplayPage.Date4ComboBox.SelectedItem = Defaults.DateCustomFormat4;
            }
            UpdateCustomExampleDate();
        }

        private string GetFormattedCustomDateString() =>
            FMDisplayPage.Date1ComboBox.SelectedItem +
            FMDisplayPage.DateSeparator1TextBox.Text.EscapeAllChars() +
            FMDisplayPage.Date2ComboBox.SelectedItem +
            FMDisplayPage.DateSeparator2TextBox.Text.EscapeAllChars() +
            FMDisplayPage.Date3ComboBox.SelectedItem +
            FMDisplayPage.DateSeparator3TextBox.Text.EscapeAllChars() +
            FMDisplayPage.Date4ComboBox.SelectedItem;

        private bool FormatAndTestDate(out string formatString, out string exampleDateString)
        {
            formatString = GetFormattedCustomDateString();

            // It's impossible to get an ArgumentOutOfRangeException as long as our readonly example date is valid.
            // It's probably impossible to get a FormatException too (because we handle invalid formats in the
            // config reader and reset to default if they aren't valid) but not 100% certain.
            try
            {
                exampleDateString = _exampleDate.ToString(formatString);
                return true;
            }
            catch (FormatException)
            {
                formatString = "";
                exampleDateString = "";
                return false;
            }
            catch (ArgumentOutOfRangeException)
            {
                formatString = "";
                exampleDateString = "";
                return false;
            }
        }

        #region Date display

        private void UpdateCustomExampleDate()
        {
            bool success = FormatAndTestDate(out _, out string formattedExampleDate);
            if (success)
            {
                FMDisplayPage.PreviewDateLabel.Text = formattedExampleDate;
            }
            else
            {
                SetCustomDateFormatFieldsToDefault();
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
            try
            {
                LText = Ini.ReadLocalizationIni(Path.Combine(Paths.Languages, LangComboBox.SelectedBackingItem() + ".ini"));
                Localize();
                if (!_startup) LocalizeOwnerForm();
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
                    if (PathErrorIsSet(tb)) return;
                }
            }

            ErrorLabel.Text = shown ? LText.SettingsWindow.Paths_ErrorSomePathsAreInvalid : "";
            ErrorLabel.Visible = shown;
        }

        private static bool PathErrorIsSet(Control control) => control.Tag is PathError pathError && pathError == PathError.True;

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
            else if (e.KeyCode == Keys.F1)
            {
                string section =
                    _startup ? HelpSections.InitialSettings :
                    PathsPage.IsVisible ? HelpSections.PathsSettings :
                    FMDisplayPage.IsVisible ? HelpSections.FMDisplaySettings :
                    OtherPage.IsVisible ? HelpSections.OtherSettings :
                    "";

                if (!section.IsEmpty()) Core.OpenHelpFile(section);
            }
        }

        private void LocalizeOwnerForm()
        {
            try
            {
                _ownerForm!.Localize();
            }
            catch (Exception ex)
            {
                Log(nameof(_ownerForm) + " was null or some other exotic exception occurred - not supposed to happen", ex);
            }
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                components?.Dispose();
                // If we're on startup, only PathsPage will have been added, so others must be manually disposed.
                // Just dispose them all if they need it, to be thorough.
                PathsPage?.Dispose();
                FMDisplayPage?.Dispose();
                OtherPage?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
