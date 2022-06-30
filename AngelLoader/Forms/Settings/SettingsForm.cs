// TODO: @IO_SAFETY: @Robustness: Check paths and exes for conflicts, duplicates, disallowed locations, etc.

// Idea I had:
// Change out the left buttons for a TreeView that can have subcategories. That way, we can divide up the settings
// into small enough pages that no page has an unreasonable loading delay.
// But...
// UPDATE 2021-04-27: We have severe flickering issues with the TreeView. Reverting to old style for now.

using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using AngelLoader.DataClasses;
using AngelLoader.Forms.CustomControls;
using AngelLoader.Forms.WinFormsNative;
using AngelLoader.Forms.WinFormsNative.Dialogs;
using static AL_Common.Common;
using static AL_Common.Logger;
using static AngelLoader.Forms.Interfaces;
using static AngelLoader.GameSupport;
using static AngelLoader.Misc;

namespace AngelLoader.Forms
{
    internal sealed partial class SettingsForm : DarkFormBase, IEventDisabler
    {
        #region Private fields

        private readonly ISettingsChangeableWindow? _ownerForm;

        private readonly bool _startup;
        private readonly bool _cleanStart;

        #region Copies of passed-in data

        private readonly string _inLanguage;
        private readonly LText_Class _inLText;

        private readonly int _inPathsVScrollPos;
        private readonly int _inAppearanceVScrollPos;
        private readonly int _inOtherVScrollPos;

        private readonly VisualTheme _inTheme;

        #endregion

        private VisualTheme _selfTheme;

        private readonly DarkRadioButtonCustom[] PageRadioButtons;
        private readonly ISettingsPage[] Pages;
        private readonly int?[] _pageVScrollValues;

        private readonly DarkTextBox[] ExePathTextBoxes;
        private readonly Control[] ErrorableControls;

        private readonly DarkLabel[] GameExeLabels;
        private readonly DarkTextBox[] GameExeTextBoxes;
        private readonly DarkButton[] GameExeBrowseButtons;
        private readonly DarkCheckBox[] GameUseSteamCheckBoxes;

        // August 4 is chosen more-or-less randomly, but both its name and its number are different short vs. long
        // (Aug vs. August; 8 vs. 08), and the same thing with 4 (4 vs. 04).
        private readonly DateTime _exampleDate = new DateTime(DateTime.Now.Year, 8, 4);

        private readonly DarkComboBoxWithBackingItems LangComboBox;
        private readonly DarkGroupBox LangGroupBox;

        private readonly PathsPage PathsPage;
        private readonly AppearancePage AppearancePage;
        private readonly OtherPage OtherPage;

        private enum PathError { True, False }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool EventsDisabled { get; set; }

        #endregion

        public readonly ConfigData OutConfig;

        protected override void WndProc(ref Message m)
        {
            if (_startup && m.Msg == Native.WM_THEMECHANGED)
            {
                Win32ThemeHooks.ReloadTheme();
            }
            base.WndProc(ref m);
        }

        // @CAN_RUN_BEFORE_VIEW_INIT
        internal SettingsForm(ISettingsChangeableWindow? ownerForm, ConfigData config, bool startup, bool cleanStart)
        {
#if DEBUG
            InitializeComponent();
#else
            InitSlim();
#endif

            if (startup) Win32ThemeHooks.InstallHooks();

            _selfTheme = config.VisualTheme;

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
            _inAppearanceVScrollPos = config.SettingsAppearanceVScrollPos;
            _inOtherVScrollPos = config.SettingsOtherVScrollPos;

            _inTheme = config.VisualTheme;

            #endregion

            OutConfig = new ConfigData();

            PathsPage = new PathsPage { Visible = false };
            AppearancePage = new AppearancePage { Visible = false };
            OtherPage = new OtherPage { Visible = false };

            LangGroupBox = AppearancePage.LanguageGroupBox;
            LangComboBox = AppearancePage.LanguageComboBox;

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

            // @GENGAMES (Settings): We've traded one form of jank for another
            // In our quest to be fast and lean, we're using arrays instead of lists here. That means we have to
            // do this hideous SupportedGameCount + n thing instead of just being able to say AddRange(games)
            // then Add(whatever else) afterwards.
            // Still, this jank is now at least localized to this one small area and we'll know immediately if we
            // get it wrong (we'll crash on OOB).

            #region Exe path textboxes

            ExePathTextBoxes = new DarkTextBox[SupportedGameCount + 1];
            Array.Copy(GameExeTextBoxes, 0, ExePathTextBoxes, 0, SupportedGameCount);

            ExePathTextBoxes[SupportedGameCount] = PathsPage.SteamExeTextBox;

            #endregion

            #region Errorable textboxes

            ErrorableControls = new Control[SupportedGameCount + 3];
            Array.Copy(GameExeTextBoxes, 0, ErrorableControls, 0, SupportedGameCount);

            ErrorableControls[SupportedGameCount] = PathsPage.SteamExeTextBox;
            ErrorableControls[SupportedGameCount + 1] = PathsPage.BackupPathTextBox;
            ErrorableControls[SupportedGameCount + 2] = PathsPage.FMArchivePathsListBox;

            #endregion

            // @GENGAMES (Settings): End

            PageRadioButtons = new[] { PathsRadioButton, AppearanceRadioButton, OtherRadioButton };

            // These are nullable because null values get put INTO them later. So not a mistake to fill them with
            // non-nullable ints right off the bat.
            _pageVScrollValues = new int?[]
            {
                _inPathsVScrollPos,
                _inAppearanceVScrollPos,
                _inOtherVScrollPos
            };

            #region Add pages

            PagePanel.Controls.Add(PathsPage);
            // We set DockStyle here so that it isn't set when we use the designer!
            PathsPage.Dock = DockStyle.Fill;

            if (startup)
            {
                Pages = new ISettingsPage[] { PathsPage };

                PathsPage.PagePanel.Controls.Add(LangGroupBox);
                AppearancePage.PagePanel.Controls.Remove(LangGroupBox);
                LangGroupBox.Location = new Point(8, 8);
                LangGroupBox.Width = PathsPage.Width - 16;
                LangGroupBox.MinimumSize = LangGroupBox.MinimumSize with { Width = LangGroupBox.Width };
                PathsPage.ActualPathsPanel.Location = new Point(0, LangGroupBox.Height + 8);
            }
            else
            {
                Pages = new ISettingsPage[] { PathsPage, AppearancePage, OtherPage };

                PagePanel.Controls.Add(AppearancePage);
                PagePanel.Controls.Add(OtherPage);

                AppearancePage.Dock = DockStyle.Fill;
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
                    AppearanceRadioButton.Hide();
                    OtherRadioButton.Hide();
                }
                else
                {
                    switch (config.SettingsTab)
                    {
                        case SettingsTab.Appearance:
                            AppearanceRadioButton.Checked = true;
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

            Width = Math.Min(config.SettingsWindowSize.Width, Screen.FromControl(this).WorkingArea.Width);
            Height = Math.Min(config.SettingsWindowSize.Height, Screen.FromControl(this).WorkingArea.Height);
            MainSplitContainer.SplitterDistance = config.SettingsWindowSplitterDistance;

            #region Set page UI state

            #region Load languages

            const string engLang = "English";

            var langsList = config.LanguageNames.ToList().OrderBy(x => x.Key);

            try
            {
                LangComboBox.BeginUpdate();

                LangComboBox.AddFullItem(engLang, engLang);
                foreach (var item in langsList)
                {
                    if (!item.Key.EqualsI(engLang))
                    {
                        LangComboBox.AddFullItem(item.Key, item.Value);
                    }
                }
            }
            finally
            {
                LangComboBox.EndUpdate();
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

            PathsPage.FMArchivePathsListBox.BeginUpdate();
            PathsPage.FMArchivePathsListBox.Items.Clear();
            foreach (string path in config.FMArchivePaths) PathsPage.FMArchivePathsListBox.Items.Add(path);
            PathsPage.FMArchivePathsListBox.EndUpdate();

            PathsPage.IncludeSubfoldersCheckBox.Checked = config.FMArchivePathsIncludeSubfolders;

            #endregion

            if (!startup)
            {
                #region Appearance page

                switch (_selfTheme)
                {
                    case VisualTheme.Classic:
                        AppearancePage.ClassicThemeRadioButton.Checked = true;
                        break;
                    case VisualTheme.Dark:
                    default:
                        AppearancePage.DarkThemeRadioButton.Checked = true;
                        break;
                }

                #region Game organization

                switch (config.GameOrganization)
                {
                    case GameOrganization.ByTab:
                        AppearancePage.OrganizeGamesByTabRadioButton.Checked = true;
                        AppearancePage.UseShortGameTabNamesCheckBox.Enabled = true;
                        break;
                    case GameOrganization.OneList:
                    default:
                        AppearancePage.OrganizeGamesInOneListRadioButton.Checked = true;
                        AppearancePage.UseShortGameTabNamesCheckBox.Enabled = false;
                        break;
                }

                AppearancePage.UseShortGameTabNamesCheckBox.Checked = config.UseShortGameTabNames;

                #endregion

                #region Articles

                AppearancePage.EnableIgnoreArticlesCheckBox.Checked = config.EnableArticles;

                for (int i = 0; i < config.Articles.Count; i++)
                {
                    string article = config.Articles[i];
                    if (i > 0) AppearancePage.ArticlesTextBox.Text += ", ";
                    AppearancePage.ArticlesTextBox.Text += article;
                }

                AppearancePage.MoveArticlesToEndCheckBox.Checked = config.MoveArticlesToEnd;

                SetArticlesEnabledState();

                #endregion

                #region Rating display style

                switch (config.RatingDisplayStyle)
                {
                    case RatingDisplayStyle.NewDarkLoader:
                        AppearancePage.RatingNDLDisplayStyleRadioButton.Checked = true;
                        break;
                    case RatingDisplayStyle.FMSel:
                    default:
                        AppearancePage.RatingFMSelDisplayStyleRadioButton.Checked = true;
                        break;
                }

                AppearancePage.RatingUseStarsCheckBox.Checked = config.RatingUseStars;

                SetRatingImage();

                AppearancePage.RatingUseStarsCheckBox.Enabled = AppearancePage.RatingFMSelDisplayStyleRadioButton.Checked;

                #endregion

                #region Date format

                object[] dateFormatList = ValidDateFormatList.Cast<object>().ToArray();
                AppearancePage.Date1ComboBox.Items.AddRange(dateFormatList);
                AppearancePage.Date2ComboBox.Items.AddRange(dateFormatList);
                AppearancePage.Date3ComboBox.Items.AddRange(dateFormatList);
                AppearancePage.Date4ComboBox.Items.AddRange(dateFormatList);

                string d1 = config.DateCustomFormat1;
                string s1 = config.DateCustomSeparator1;
                string d2 = config.DateCustomFormat2;
                string s2 = config.DateCustomSeparator2;
                string d3 = config.DateCustomFormat3;
                string s3 = config.DateCustomSeparator3;
                string d4 = config.DateCustomFormat4;

                AppearancePage.Date1ComboBox.SelectedItem = AppearancePage.Date1ComboBox.Items.Contains(d1) ? d1 : Defaults.DateCustomFormat1;
                AppearancePage.DateSeparator1TextBox.Text = s1;
                AppearancePage.Date2ComboBox.SelectedItem = AppearancePage.Date2ComboBox.Items.Contains(d2) ? d2 : Defaults.DateCustomFormat2;
                AppearancePage.DateSeparator2TextBox.Text = s2;
                AppearancePage.Date3ComboBox.SelectedItem = AppearancePage.Date3ComboBox.Items.Contains(d3) ? d3 : Defaults.DateCustomFormat3;
                AppearancePage.DateSeparator3TextBox.Text = s3;
                AppearancePage.Date4ComboBox.SelectedItem = AppearancePage.Date4ComboBox.Items.Contains(d4) ? d4 : Defaults.DateCustomFormat4;

                // This comes last so that all the custom data is in place for the preview date to use
                switch (config.DateFormat)
                {
                    case DateFormat.CurrentCultureLong:
                        AppearancePage.DateCurrentCultureLongRadioButton.Checked = true;
                        AppearancePage.DateCustomFormatPanel.Enabled = false;
                        AppearancePage.PreviewDateLabel.Text = _exampleDate.ToLongDateString();
                        break;
                    case DateFormat.Custom:
                        AppearancePage.DateCustomRadioButton.Checked = true;
                        AppearancePage.DateCustomFormatPanel.Enabled = true;
                        UpdateCustomExampleDate();
                        break;
                    case DateFormat.CurrentCultureShort:
                    default:
                        AppearancePage.DateCurrentCultureShortRadioButton.Checked = true;
                        AppearancePage.DateCustomFormatPanel.Enabled = false;
                        AppearancePage.PreviewDateLabel.Text = _exampleDate.ToShortDateString();
                        break;
                }

                #endregion

                #region Recent FMs

                AppearancePage.RecentFMsNumericUpDown.Maximum = Defaults.MaxDaysRecent;
                AppearancePage.RecentFMsNumericUpDown.Value = config.DaysRecent;

                #endregion

                #region Show/hide UI elements

                AppearancePage.HideUninstallButtonCheckBox.Checked = config.HideUninstallButton;
                AppearancePage.HideFMListZoomButtonsCheckBox.Checked = config.HideFMListZoomButtons;
                AppearancePage.HideExitButtonCheckBox.Checked = config.HideExitButton;

                #endregion

                AppearancePage.ReadmeFixedWidthFontCheckBox.Checked = config.ReadmeUseFixedWidthFont;

                #region Play without FM

                if (config.PlayOriginalSeparateButtons)
                {
                    AppearancePage.PlayWithoutFM_MultipleButtonsRadioButton.Checked = true;
                }
                else
                {
                    AppearancePage.PlayWithoutFM_SingleButtonRadioButton.Checked = true;
                }

                #endregion

                #endregion

                #region Other page

                #region File conversion

                OtherPage.ConvertWAVsTo16BitOnInstallCheckBox.Checked = config.ConvertWAVsTo16BitOnInstall;
                OtherPage.ConvertOGGsToWAVsOnInstallCheckBox.Checked = config.ConvertOGGsToWAVsOnInstall;

                #endregion

                #region Installing FMs

                switch (config.ConfirmBeforeInstall)
                {
                    case ConfirmBeforeInstall.Always:
                        OtherPage.Install_ConfirmAlwaysRadioButton.Checked = true;
                        break;
                    case ConfirmBeforeInstall.OnlyForMultiple:
                        OtherPage.Install_ConfirmMultipleOnlyRadioButton.Checked = true;
                        break;
                    case ConfirmBeforeInstall.Never:
                    default:
                        OtherPage.Install_ConfirmNeverRadioButton.Checked = true;
                        break;
                }

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

                #endregion
            }

            #endregion

            if (_inTheme != VisualTheme.Classic)
            {
                SetTheme(_selfTheme, startup: true);
            }
            else
            {
                ErrorIconPictureBox.Image = Images.RedExclCircle;
            }

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
                AppearancePage.ClassicThemeRadioButton.CheckedChanged += VisualThemeRadioButtons_CheckedChanged;
                AppearancePage.DarkThemeRadioButton.CheckedChanged += VisualThemeRadioButtons_CheckedChanged;

                AppearancePage.OrganizeGamesByTabRadioButton.CheckedChanged += GameOrganizationRadioButtons_CheckedChanged;
                AppearancePage.OrganizeGamesInOneListRadioButton.CheckedChanged += GameOrganizationRadioButtons_CheckedChanged;

                AppearancePage.EnableIgnoreArticlesCheckBox.CheckedChanged += ArticlesCheckBox_CheckedChanged;
                AppearancePage.ArticlesTextBox.Leave += ArticlesTextBox_Leave;

                AppearancePage.RatingNDLDisplayStyleRadioButton.CheckedChanged += RatingOutOfTenRadioButton_CheckedChanged;
                AppearancePage.RatingFMSelDisplayStyleRadioButton.CheckedChanged += RatingOutOfFiveRadioButton_CheckedChanged;
                AppearancePage.RatingUseStarsCheckBox.CheckedChanged += RatingUseStarsCheckBox_CheckedChanged;

                AppearancePage.DateCurrentCultureShortRadioButton.CheckedChanged += DateShortAndLongRadioButtons_CheckedChanged;
                AppearancePage.DateCurrentCultureLongRadioButton.CheckedChanged += DateShortAndLongRadioButtons_CheckedChanged;
                AppearancePage.DateCustomRadioButton.CheckedChanged += DateCustomRadioButton_CheckedChanged;

                AppearancePage.Date1ComboBox.SelectedIndexChanged += DateCustomValue_Changed;
                AppearancePage.Date2ComboBox.SelectedIndexChanged += DateCustomValue_Changed;
                AppearancePage.Date3ComboBox.SelectedIndexChanged += DateCustomValue_Changed;
                AppearancePage.Date4ComboBox.SelectedIndexChanged += DateCustomValue_Changed;

                AppearancePage.DateSeparator1TextBox.TextChanged += DateCustomValue_Changed;
                AppearancePage.DateSeparator2TextBox.TextChanged += DateCustomValue_Changed;
                AppearancePage.DateSeparator3TextBox.TextChanged += DateCustomValue_Changed;

                OtherPage.WebSearchUrlResetButton.Click += WebSearchURLResetButton_Click;
            }

            #endregion
        }

        private void SetTheme(VisualTheme theme, bool startup)
        {
            _selfTheme = theme;

            // Some parts of the code check this (eg. theme renderers) so we need to set it. We'll revert it
            // back to the passed-in theme if we cancel.
            Config.VisualTheme = theme;

            bool darkMode = theme == VisualTheme.Dark;

            try
            {
                if (!startup) MainSplitContainer.SuspendDrawing();

                SetThemeBase(
                    theme,
                    x => x is SplitterPanel,
                    capacity: 150
                );

                Images.DarkModeEnabled = darkMode;
                SetRatingImage();
                for (int i = 0; i < ErrorableControls.Length; i++)
                {
                    ShowPathError(ErrorableControls[i], PathErrorIsSet(ErrorableControls[i]));
                }
                // Just use an error image instead of an ErrorProvider, because ErrorProvider's tooltip is even
                // stupider than usual and REALLY resists being themed properly (we can't even recreate its handle
                // even if we DID want to do more reflection crap!)
                ErrorIconPictureBox.Image = Images.RedExclCircle;
            }
            finally
            {
                if (!startup) MainSplitContainer.ResumeDrawing();
            }
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
            PathsPage.LayoutFLP.PerformLayout();
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
            if (suspendResume) MainSplitContainer.SuspendDrawing();
            try
            {
                Text = _startup ? LText.SettingsWindow.StartupTitleText : LText.SettingsWindow.TitleText;

                OKButton.Text = LText.Global.OK;
                Cancel_Button.Text = LText.Global.Cancel;

                #region Paths page

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
                PathsPage.GameRequirementsLabel.Text =
                    LText.SettingsWindow.Paths_DarkEngineGamesRequireNewDark + Environment.NewLine +
                    LText.SettingsWindow.Paths_Thief3RequiresSneakyUpgrade;

                PathsPage.SteamOptionsGroupBox.Text = LText.SettingsWindow.Paths_SteamOptions;
                PathsPage.SteamExeLabel.Text = LText.SettingsWindow.Paths_PathToSteamExecutable;
                PathsPage.SteamExeBrowseButton.SetTextForTextBoxButtonCombo(PathsPage.SteamExeTextBox, LText.Global.BrowseEllipses);
                PathsPage.LaunchTheseGamesThroughSteamCheckBox.Text = LText.SettingsWindow.Paths_LaunchTheseGamesThroughSteam;

                PathsPage.OtherGroupBox.Text = LText.SettingsWindow.Paths_Other;
                PathsPage.BackupPathLabel.Text = LText.SettingsWindow.Paths_BackupPath;
                PathsPage.BackupPathBrowseButton.SetTextForTextBoxButtonCombo(PathsPage.BackupPathTextBox, LText.Global.BrowseEllipses);

                PathsPage.BackupPathHelpLabel.Text = LText.SettingsWindow.Paths_BackupPath_Info;
                // Required for the startup version where the lang box is on the same page as paths!
                PathsPage.LayoutFLP.PerformLayout();

                PathsPage.FMArchivePathsGroupBox.Text = LText.SettingsWindow.Paths_FMArchivePaths;
                PathsPage.IncludeSubfoldersCheckBox.Text = LText.SettingsWindow.Paths_IncludeSubfolders;
                MainToolTip.SetToolTip(PathsPage.AddFMArchivePathButton, LText.SettingsWindow.Paths_AddArchivePathToolTip);
                MainToolTip.SetToolTip(PathsPage.RemoveFMArchivePathButton, LText.SettingsWindow.Paths_RemoveArchivePathToolTip);

                ErrorLabel.Text = LText.SettingsWindow.Paths_ErrorSomePathsAreInvalid;

                #endregion

                if (_startup)
                {
                    LangGroupBox.Text = LText.SettingsWindow.Appearance_Language;
                }
                else
                {
                    #region Appearance page

                    AppearanceRadioButton.Text = LText.SettingsWindow.Appearance_TabText;

                    AppearancePage.LanguageGroupBox.Text = LText.SettingsWindow.Appearance_Language;

                    AppearancePage.VisualThemeGroupBox.Text = LText.SettingsWindow.Appearance_Theme;
                    AppearancePage.ClassicThemeRadioButton.Text = LText.SettingsWindow.Appearance_Theme_Classic;
                    AppearancePage.DarkThemeRadioButton.Text = LText.SettingsWindow.Appearance_Theme_Dark;

                    AppearancePage.FMsListGroupBox.Text = LText.SettingsWindow.Appearance_FMsList;
                    AppearancePage.GameOrganizationLabel.Text = LText.SettingsWindow.Appearance_GameOrganization;
                    AppearancePage.OrganizeGamesByTabRadioButton.Text = LText.SettingsWindow.Appearance_GameOrganizationByTab;
                    AppearancePage.UseShortGameTabNamesCheckBox.Text = LText.SettingsWindow.Appearance_UseShortGameTabNames;
                    AppearancePage.OrganizeGamesInOneListRadioButton.Text = LText.SettingsWindow.Appearance_GameOrganizationOneList;

                    AppearancePage.SortingLabel.Text = LText.SettingsWindow.Appearance_Sorting;
                    AppearancePage.EnableIgnoreArticlesCheckBox.Text = LText.SettingsWindow.Appearance_IgnoreArticles;
                    AppearancePage.MoveArticlesToEndCheckBox.Text = LText.SettingsWindow.Appearance_MoveArticlesToEnd;

                    AppearancePage.RatingDisplayStyleLabel.Text = LText.SettingsWindow.Appearance_RatingDisplayStyle;
                    AppearancePage.RatingNDLDisplayStyleRadioButton.Text = LText.SettingsWindow.Appearance_RatingDisplayStyleNDL;
                    AppearancePage.RatingFMSelDisplayStyleRadioButton.Text = LText.SettingsWindow.Appearance_RatingDisplayStyleFMSel;
                    AppearancePage.RatingUseStarsCheckBox.Text = LText.SettingsWindow.Appearance_RatingDisplayStyleUseStars;

                    AppearancePage.DateFormatLabel.Text = LText.SettingsWindow.Appearance_DateFormat;
                    AppearancePage.DateCurrentCultureShortRadioButton.Text = LText.SettingsWindow.Appearance_CurrentCultureShort;
                    AppearancePage.DateCurrentCultureLongRadioButton.Text = LText.SettingsWindow.Appearance_CurrentCultureLong;
                    AppearancePage.DateCustomRadioButton.Text = LText.SettingsWindow.Appearance_Custom;

                    AppearancePage.RecentFMsHeaderLabel.Text = LText.SettingsWindow.Appearance_RecentFMs;
                    AppearancePage.RecentFMsLabel.Text = LText.SettingsWindow.Appearance_RecentFMs_MaxDays;

                    AppearancePage.ShowOrHideUIElementsGroupBox.Text = LText.SettingsWindow.Appearance_ShowOrHideInterfaceElements;
                    AppearancePage.HideUninstallButtonCheckBox.Text = LText.SettingsWindow.Appearance_HideUninstallButton;
                    AppearancePage.HideFMListZoomButtonsCheckBox.Text = LText.SettingsWindow.Appearance_HideFMListZoomButtons;
                    AppearancePage.HideExitButtonCheckBox.Text = LText.SettingsWindow.Appearance_HideExitButton;

                    AppearancePage.ReadmeGroupBox.Text = LText.SettingsWindow.Appearance_ReadmeBox;
                    AppearancePage.ReadmeFixedWidthFontCheckBox.Text = LText.SettingsWindow.Appearance_ReadmeUseFixedWidthFont;

                    AppearancePage.PlayWithoutFMGroupBox.Text = LText.SettingsWindow.Appearance_PlayWithoutFM;
                    AppearancePage.PlayWithoutFM_SingleButtonRadioButton.Text = LText.SettingsWindow.Appearance_PlayWithoutFM_SingleButton;
                    AppearancePage.PlayWithoutFM_MultipleButtonsRadioButton.Text = LText.SettingsWindow.Appearance_PlayWithoutFM_MultiButton;

                    #endregion

                    #region Other page

                    OtherRadioButton.Text = LText.SettingsWindow.Other_TabText;

                    OtherPage.FMFileConversionGroupBox.Text = LText.SettingsWindow.Other_FMFileConversion;
                    OtherPage.ConvertWAVsTo16BitOnInstallCheckBox.Text = LText.SettingsWindow.Other_ConvertWAVsTo16BitOnInstall;
                    OtherPage.ConvertOGGsToWAVsOnInstallCheckBox.Text = LText.SettingsWindow.Other_ConvertOGGsToWAVsOnInstall;

                    OtherPage.InstallingFMsGroupBox.Text = LText.SettingsWindow.Other_InstallingFMs;
                    OtherPage.ConfirmBeforeInstallLabel.Text = LText.SettingsWindow.Other_ConfirmBeforeInstallingFM;
                    OtherPage.Install_ConfirmAlwaysRadioButton.Text = LText.SettingsWindow.Other_InstallConfirm_Always;
                    OtherPage.Install_ConfirmMultipleOnlyRadioButton.Text = LText.SettingsWindow.Other_InstallConfirm_OnlyForMultipleFMs;
                    OtherPage.Install_ConfirmNeverRadioButton.Text = LText.SettingsWindow.Other_InstallConfirm_Never;

                    OtherPage.UninstallingFMsGroupBox.Text = LText.SettingsWindow.Other_UninstallingFMs;
                    OtherPage.ConfirmUninstallCheckBox.Text = LText.SettingsWindow.Other_ConfirmBeforeUninstalling;
                    OtherPage.WhatToBackUpLabel.Text = LText.SettingsWindow.Other_WhenUninstallingBackUp;
                    OtherPage.BackupSavesAndScreensOnlyRadioButton.Text = LText.SettingsWindow.Other_BackUpSavesAndScreenshotsOnly;
                    OtherPage.BackupAllChangedDataRadioButton.Text = LText.SettingsWindow.Other_BackUpAllChangedFiles;
                    OtherPage.BackupAlwaysAskCheckBox.Text = LText.SettingsWindow.Other_BackUpAlwaysAsk;

                    OtherPage.WebSearchGroupBox.Text = LText.SettingsWindow.Other_WebSearch;
                    OtherPage.WebSearchUrlLabel.Text = LText.SettingsWindow.Other_WebSearchURL;
                    OtherPage.WebSearchTitleExplanationLabel.Text = LText.SettingsWindow.Other_WebSearchTitleVar;
                    MainToolTip.SetToolTip(OtherPage.WebSearchUrlResetButton, LText.SettingsWindow.Other_WebSearchResetToolTip);

                    OtherPage.PlayFMOnDCOrEnterGroupBox.Text = LText.SettingsWindow.Other_ConfirmPlayOnDCOrEnter;
                    OtherPage.ConfirmPlayOnDCOrEnterCheckBox.Text = LText.SettingsWindow.Other_ConfirmPlayOnDCOrEnter_Ask;

                    #endregion
                }
            }
            finally
            {
                if (suspendResume) MainSplitContainer.ResumeDrawing();
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

            foreach (string path in PathsPage.FMArchivePathsListBox.ItemsAsStrings)
            {
                if (!Directory.Exists(path))
                {
                    error = true;
                    ShowPathError(PathsPage.FMArchivePathsListBox, true);
                    break;
                }
            }

            if (error)
            {
                // Currently, all errors happen on the Paths page, so go to that page automatically.
                PathsRadioButton.Checked = true;

                // One user missed the error highlight on a textbox because it was scrolled offscreen, and was
                // confused as to why there was an error. So scroll the first error-highlighted textbox onscreen
                // to make it clear.
                foreach (Control control in ErrorableControls)
                {
                    if (PathErrorIsSet(control))
                    {
                        PathsPage.PagePanel.ScrollControlIntoView(control);
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
                foreach (Control control in ErrorableControls)
                {
                    if (control is DarkTextBox tb)
                    {
                        tb.BackColor = _selfTheme == VisualTheme.Dark
                            ? tb.DarkModeBackColor
                            : SystemColors.Window;
                    }
                    else if (control is DarkListBox lb)
                    {
                        Color color = _selfTheme == VisualTheme.Dark
                            ? DarkColors.LightBackground
                            : SystemColors.Window;

                        lb.BackColor = color;

                        foreach (ListViewItem item in lb.Items)
                        {
                            item.BackColor = color;
                        }
                    }

                    control.Tag = PathError.False;
                }
                ErrorIconPictureBox.Hide();
                ErrorLabel.Hide();
            }

            return false;
        }

        private void SettingsForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            #region Save window state

            // Special case: these are meta, so they should always be set even if the user clicked Cancel
            OutConfig.SettingsTab =
                AppearanceRadioButton.Checked ? SettingsTab.Appearance :
                OtherRadioButton.Checked ? SettingsTab.Other :
                SettingsTab.Paths;
            OutConfig.SettingsWindowSize = Size;
            OutConfig.SettingsWindowSplitterDistance = MainSplitContainer.SplitterDistance;

            // If some pages haven't had their vertical scroll value loaded, just take the value from the backing
            // store
            OutConfig.SettingsPathsVScrollPos = _pageVScrollValues[0] ?? PathsPage.GetVScrollPos();
            OutConfig.SettingsAppearanceVScrollPos = _pageVScrollValues[1] ?? AppearancePage.GetVScrollPos();
            OutConfig.SettingsOtherVScrollPos = _pageVScrollValues[2] ?? OtherPage.GetVScrollPos();

            #endregion

            #region Cancel

            if (DialogResult != DialogResult.OK)
            {
                if (!_startup)
                {
                    bool langsDifferent = !LangComboBox.SelectedBackingItem().EqualsI(_inLanguage);
                    bool themesDifferent = _inTheme != _selfTheme;

                    try
                    {
                        if (langsDifferent || themesDifferent)
                        {
                            SetCursors(wait: true);
                        }

                        if (langsDifferent)
                        {
                            // It's actually totally fine that this one is a reference.
                            LText = _inLText;
                            _ownerForm?.Localize();
                        }

                        if (themesDifferent)
                        {
                            Config.VisualTheme = _inTheme;
                            _ownerForm?.SetTheme(_inTheme);
                        }
                    }
                    finally
                    {
                        if (langsDifferent || themesDifferent)
                        {
                            SetCursors(wait: false);
                        }
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
            foreach (string path in PathsPage.FMArchivePathsListBox.ItemsAsStrings) OutConfig.FMArchivePaths.Add(path.Trim());

            OutConfig.FMArchivePathsIncludeSubfolders = PathsPage.IncludeSubfoldersCheckBox.Checked;

            #endregion

            if (_startup)
            {
                OutConfig.Language = LangComboBox.SelectedBackingItem();
            }
            else
            {
                #region Appearance page

                OutConfig.Language = LangComboBox.SelectedBackingItem();

                OutConfig.VisualTheme = AppearancePage.DarkThemeRadioButton.Checked
                    ? VisualTheme.Dark
                    : VisualTheme.Classic;

                #region Game organization

                OutConfig.GameOrganization = AppearancePage.OrganizeGamesByTabRadioButton.Checked
                        ? GameOrganization.ByTab
                        : GameOrganization.OneList;

                OutConfig.UseShortGameTabNames = AppearancePage.UseShortGameTabNamesCheckBox.Checked;

                #endregion

                #region Articles

                OutConfig.EnableArticles = AppearancePage.EnableIgnoreArticlesCheckBox.Checked;

                var retArticles = AppearancePage.ArticlesTextBox.Text
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

                OutConfig.MoveArticlesToEnd = AppearancePage.MoveArticlesToEndCheckBox.Checked;

                #endregion

                #region Rating display style

                OutConfig.RatingDisplayStyle = AppearancePage.RatingNDLDisplayStyleRadioButton.Checked
                    ? RatingDisplayStyle.NewDarkLoader
                    : RatingDisplayStyle.FMSel;
                OutConfig.RatingUseStars = AppearancePage.RatingUseStarsCheckBox.Checked;

                #endregion

                #region Date format

                OutConfig.DateFormat =
                    AppearancePage.DateCurrentCultureShortRadioButton.Checked ? DateFormat.CurrentCultureShort :
                    AppearancePage.DateCurrentCultureLongRadioButton.Checked ? DateFormat.CurrentCultureLong :
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

                OutConfig.DateCustomFormat1 = AppearancePage.Date1ComboBox.SelectedItem.ToString();
                OutConfig.DateCustomSeparator1 = AppearancePage.DateSeparator1TextBox.Text;
                OutConfig.DateCustomFormat2 = AppearancePage.Date2ComboBox.SelectedItem.ToString();
                OutConfig.DateCustomSeparator2 = AppearancePage.DateSeparator2TextBox.Text;
                OutConfig.DateCustomFormat3 = AppearancePage.Date3ComboBox.SelectedItem.ToString();
                OutConfig.DateCustomSeparator3 = AppearancePage.DateSeparator3TextBox.Text;
                OutConfig.DateCustomFormat4 = AppearancePage.Date4ComboBox.SelectedItem.ToString();

                #endregion

                OutConfig.DaysRecent = (uint)AppearancePage.RecentFMsNumericUpDown.Value;

                #region Show/hide UI elements

                OutConfig.HideUninstallButton = AppearancePage.HideUninstallButtonCheckBox.Checked;
                OutConfig.HideFMListZoomButtons = AppearancePage.HideFMListZoomButtonsCheckBox.Checked;
                OutConfig.HideExitButton = AppearancePage.HideExitButtonCheckBox.Checked;

                #endregion

                OutConfig.ReadmeUseFixedWidthFont = AppearancePage.ReadmeFixedWidthFontCheckBox.Checked;

                OutConfig.PlayOriginalSeparateButtons = AppearancePage.PlayWithoutFM_MultipleButtonsRadioButton.Checked;

                #endregion

                #region Other page

                #region File conversion

                OutConfig.ConvertWAVsTo16BitOnInstall = OtherPage.ConvertWAVsTo16BitOnInstallCheckBox.Checked;
                OutConfig.ConvertOGGsToWAVsOnInstall = OtherPage.ConvertOGGsToWAVsOnInstallCheckBox.Checked;

                #endregion

                #region Installing FMs

                OutConfig.ConfirmBeforeInstall =
                    OtherPage.Install_ConfirmAlwaysRadioButton.Checked ? ConfirmBeforeInstall.Always :
                    OtherPage.Install_ConfirmMultipleOnlyRadioButton.Checked ? ConfirmBeforeInstall.OnlyForMultiple :
                    ConfirmBeforeInstall.Never;

                #endregion

                #region Uninstalling FMs

                OutConfig.ConfirmUninstall = OtherPage.ConfirmUninstallCheckBox.Checked;

                OutConfig.BackupFMData =
                    OtherPage.BackupSavesAndScreensOnlyRadioButton.Checked
                        ? BackupFMData.SavesAndScreensOnly
                        : BackupFMData.AllChangedFiles;

                OutConfig.BackupAlwaysAsk = OtherPage.BackupAlwaysAskCheckBox.Checked;

                #endregion

                OutConfig.WebSearchUrl = OtherPage.WebSearchUrlTextBox.Text;

                OutConfig.ConfirmPlayOnDCOrEnter = OtherPage.ConfirmPlayOnDCOrEnterCheckBox.Checked;

                #endregion
            }
        }

        #region Page selection handler

        private void PathsRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (EventsDisabled) return;

            var s = (DarkRadioButtonCustom)sender;
            if (!s.Checked) return;

            using (new DisableEvents(this)) foreach (var b in PageRadioButtons) if (s != b) b.Checked = false;

            ShowPage(Array.IndexOf(PageRadioButtons, s));
        }

        private void SetPageScrollPos(ISettingsPage page)
        {
            int? pos =
                page == PathsPage ? _inPathsVScrollPos :
                page == AppearancePage ? _inAppearanceVScrollPos :
                page == OtherPage ? _inOtherVScrollPos :
                null;

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
                    if (!initialCall && pagePosWasStored) MainSplitContainer.SuspendDrawing();

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
                    if (!initialCall && pagePosWasStored) MainSplitContainer.ResumeDrawing();
                }
            }
        }

        #endregion

        #region Paths page

        private void ExePathTextBoxes_Leave(object sender, EventArgs e)
        {
            var s = (DarkTextBox)sender;
            ShowPathError(s, !s.Text.IsEmpty() && !File.Exists(s.Text));
        }

        private void ExePathBrowseButtons_Click(object sender, EventArgs e)
        {
            DarkTextBox? tb = null;
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
            var s = (DarkTextBox)sender;
            ShowPathError(s, !Directory.Exists(s.Text));
        }

        // @NET5: Do it on this side of the boundary now because we'll want to use the built-in Vista dialog that comes with .NET 5
        private static string SanitizePathForDialog(string path)
        {
            if (!string.IsNullOrWhiteSpace(path) && !Directory.Exists(path))
            {
                try
                {
                    // C:\Folder\File.exe becomes C:\Folder
                    path = Path.GetDirectoryName(path) ?? "";
                    return Directory.Exists(path) ? path : "";
                }
                // Fix: we weren't checking for invalid path names
                catch
                {
                    return "";
                }
            }

            return path;
        }

        private void BackupPathBrowseButton_Click(object sender, EventArgs e)
        {
            var tb = PathsPage.BackupPathTextBox;

            using (var d = new VistaFolderBrowserDialog())
            {
                d.InitialDirectory = SanitizePathForDialog(tb.Text);
                d.MultiSelect = false;
                if (d.ShowDialogDark(this) == DialogResult.OK) tb.Text = d.DirectoryName;
            }

            ShowPathError(tb, !Directory.Exists(tb.Text));
        }

        private (DialogResult Result, string FileName)
        BrowseForExeFile(string initialPath)
        {
            using var dialog = new OpenFileDialog
            {
                InitialDirectory = initialPath,
                Filter = LText.BrowseDialogs.ExeFiles + "|*.exe"
            };
            return (dialog.ShowDialogDark(this), dialog.FileName);
        }

        private void SteamExeTextBox_TextChanged(object sender, EventArgs e)
        {
            PathsPage.LaunchTheseGamesThroughSteamPanel.Enabled = !PathsPage.SteamExeTextBox.Text.IsWhiteSpace();
        }

        #region Archive paths

        private void AddFMArchivePathButton_Click(object sender, EventArgs e)
        {
            using var d = new VistaFolderBrowserDialog();

            var lb = PathsPage.FMArchivePathsListBox;
            string initDir =
                lb.SelectedIndex > -1 ? lb.SelectedItem :
                lb.Items.Count > 0 ? lb.ItemsAsStrings[lb.Items.Count - 1] :
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
            if (d.ShowDialogDark(this) == DialogResult.OK)
            {
                PathsPage.FMArchivePathsListBox.BeginUpdate();

                var hash = PathsPage.FMArchivePathsListBox.ItemsAsStrings.ToHashSetI();

                foreach (string dir in d.DirectoryNames)
                {
                    if (!hash.Contains(dir)) PathsPage.FMArchivePathsListBox.Items.Add(dir);
                }
                PathsPage.FMArchivePathsListBox.EndUpdate();
            }

            CheckForErrors();
        }

        private void RemoveFMArchivePathButton_Click(object sender, EventArgs e)
        {
            PathsPage.FMArchivePathsListBox.RemoveAndSelectNearest();
            CheckForErrors();
        }

        #endregion

        #endregion

        #region Appearance page

        private void VisualThemeRadioButtons_CheckedChanged(object sender, EventArgs e)
        {
            if (EventsDisabled) return;

            VisualTheme theme = sender == AppearancePage.DarkThemeRadioButton
                ? VisualTheme.Dark
                : VisualTheme.Classic;

            try
            {
                SetCursors(wait: true);

                SetTheme(theme, startup: false);
                _ownerForm?.SetTheme(theme);
            }
            finally
            {
                SetCursors(wait: false);
            }
        }

        private void GameOrganizationRadioButtons_CheckedChanged(object sender, EventArgs e)
        {
            AppearancePage.UseShortGameTabNamesCheckBox.Enabled = AppearancePage.OrganizeGamesByTabRadioButton.Checked;
        }

        #region Articles

        private void ArticlesCheckBox_CheckedChanged(object sender, EventArgs e) => SetArticlesEnabledState();

        private void SetArticlesEnabledState()
        {
            AppearancePage.ArticlesTextBox.Enabled = AppearancePage.EnableIgnoreArticlesCheckBox.Checked;
            AppearancePage.MoveArticlesToEndCheckBox.Enabled = AppearancePage.EnableIgnoreArticlesCheckBox.Checked;
        }

        private void ArticlesTextBox_Leave(object sender, EventArgs e) => FormatArticles();

        private void FormatArticles()
        {
            string articles = AppearancePage.ArticlesTextBox.Text;

            // Copied wholesale from Autovid, ridiculous looking, but works

            if (articles.IsWhiteSpace())
            {
                AppearancePage.ArticlesTextBox.Text = "";
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

            AppearancePage.ArticlesTextBox.Text = articles;
        }

        #endregion

        private void SetCustomDateFormatFieldsToDefault()
        {
            using (new DisableEvents(this))
            {
                AppearancePage.Date1ComboBox.SelectedItem = Defaults.DateCustomFormat1;
                AppearancePage.DateSeparator1TextBox.Text = Defaults.DateCustomSeparator1;
                AppearancePage.Date2ComboBox.SelectedItem = Defaults.DateCustomFormat2;
                AppearancePage.DateSeparator2TextBox.Text = Defaults.DateCustomSeparator2;
                AppearancePage.Date3ComboBox.SelectedItem = Defaults.DateCustomFormat3;
                AppearancePage.DateSeparator3TextBox.Text = Defaults.DateCustomSeparator3;
                AppearancePage.Date4ComboBox.SelectedItem = Defaults.DateCustomFormat4;
            }
            UpdateCustomExampleDate();
        }

        private string GetFormattedCustomDateString() =>
            AppearancePage.Date1ComboBox.SelectedItem +
            AppearancePage.DateSeparator1TextBox.Text.EscapeAllChars() +
            AppearancePage.Date2ComboBox.SelectedItem +
            AppearancePage.DateSeparator2TextBox.Text.EscapeAllChars() +
            AppearancePage.Date3ComboBox.SelectedItem +
            AppearancePage.DateSeparator3TextBox.Text.EscapeAllChars() +
            AppearancePage.Date4ComboBox.SelectedItem;

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
                AppearancePage.PreviewDateLabel.Text = formattedExampleDate;
            }
            else
            {
                SetCustomDateFormatFieldsToDefault();
            }
        }

        private void DateShortAndLongRadioButtons_CheckedChanged(object sender, EventArgs e)
        {
            AppearancePage.DateCustomFormatPanel.Enabled = false;
            AppearancePage.PreviewDateLabel.Text = sender == AppearancePage.DateCurrentCultureShortRadioButton
                ? _exampleDate.ToShortDateString()
                : _exampleDate.ToLongDateString();
        }

        private void DateCustomRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            var s = (RadioButton)sender;
            AppearancePage.DateCustomFormatPanel.Enabled = s.Checked;
            if (s.Checked) UpdateCustomExampleDate();
        }

        private void DateCustomValue_Changed(object sender, EventArgs e)
        {
            if (AppearancePage.DateCustomFormatPanel.Enabled) UpdateCustomExampleDate();
        }

        #endregion

        #region Rating display

        private void RatingOutOfTenRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (EventsDisabled) return;
            if (AppearancePage.RatingNDLDisplayStyleRadioButton.Checked)
            {
                AppearancePage.RatingUseStarsCheckBox.Enabled = false;
                SetRatingImage();
            }
        }

        private void RatingOutOfFiveRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (EventsDisabled) return;
            if (AppearancePage.RatingFMSelDisplayStyleRadioButton.Checked)
            {
                AppearancePage.RatingUseStarsCheckBox.Enabled = true;
                SetRatingImage();
            }
        }

        private void RatingUseStarsCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (EventsDisabled) return;
            SetRatingImage();
        }

        private void SetRatingImage()
        {
            AppearancePage.RatingExamplePictureBox.Image = AppearancePage.RatingNDLDisplayStyleRadioButton.Checked
                ? Images.RatingExample_NDL
                : AppearancePage.RatingFMSelDisplayStyleRadioButton.Checked && AppearancePage.RatingUseStarsCheckBox.Checked
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

            // @BetterErrors(Settings lang combobox select)
            // If we fail, we should fall back to internal default English set, and switch the combobox to English
            try
            {
                LText = Ini.ReadLocalizationIni(Path.Combine(Paths.Languages, LangComboBox.SelectedBackingItem() + ".ini"));
            }
            catch (Exception ex)
            {
                Log(ErrorText.Ex + "in language reading", ex);
            }

            try
            {
                SetCursors(wait: true);

                Localize();
                if (!_startup) _ownerForm?.Localize();
            }
            finally
            {
                SetCursors(wait: false);
            }
        }

        #endregion

        private void SetCursors(bool wait)
        {
            if (wait)
            {
                _ownerForm?.SetWaitCursor(true);
                Cursor = Cursors.WaitCursor;
            }
            else
            {
                _ownerForm?.SetWaitCursor(false);
                Cursor = Cursors.Default;
            }
        }

        private void ShowPathError(Control control, bool shown)
        {
            if (control is DarkTextBox textBox)
            {
                textBox.BackColor =
                    _selfTheme == VisualTheme.Dark
                        ? shown
                            ? DarkColors.Fen_RedHighlight
                            : textBox.DarkModeBackColor
                        : shown
                            ? Color.MistyRose
                            : SystemColors.Window;
            }
            else if (control is DarkListBox listBox)
            {
                Color color =
                    _selfTheme == VisualTheme.Dark
                        ? shown
                            ? DarkColors.Fen_RedHighlight
                            : DarkColors.LightBackground
                        : shown
                            ? Color.MistyRose
                            : SystemColors.Window;

                listBox.BackColor = color;

                foreach (ListViewItem item in listBox.Items)
                {
                    item.BackColor = color;
                }
            }

            control.Tag = shown ? PathError.True : PathError.False;

            if (!shown)
            {
                foreach (var tb in ErrorableControls)
                {
                    if (PathErrorIsSet(tb)) return;
                }
            }

            ErrorLabel.Text = shown ? LText.SettingsWindow.Paths_ErrorSomePathsAreInvalid : "";
            ErrorIconPictureBox.Visible = shown;
            ErrorLabel.Visible = shown;
        }

        private static bool PathErrorIsSet(Control control) => control.Tag is PathError.True;

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
                    AppearancePage.IsVisible ? HelpSections.AppearanceSettings :
                    OtherPage.IsVisible ? HelpSections.OtherSettings :
                    "";

                if (!section.IsEmpty()) Core.OpenHelpFile(section);
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
                PathsPage.Dispose();
                AppearancePage.Dispose();
                OtherPage.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
