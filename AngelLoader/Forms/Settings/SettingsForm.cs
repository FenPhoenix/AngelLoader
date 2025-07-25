﻿// TODO: @IO_SAFETY: @Robustness: Check paths and exes for conflicts, duplicates, disallowed locations, etc.

// Idea I had:
// Change out the left buttons for a TreeView that can have subcategories. That way, we can divide up the settings
// into small enough pages that no page has an unreasonable loading delay.
// But...
// UPDATE 2021-04-27: We have severe flickering issues with the TreeView. Reverting to old style for now.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using AngelLoader.DataClasses;
using AngelLoader.Forms.CustomControls;
using AngelLoader.Forms.WinFormsNative;
using AngelLoader.Forms.WinFormsNative.Dialogs;
using static AL_Common.Logger;
using static AngelLoader.Forms.Interfaces;
using static AngelLoader.GameSupport;
using static AngelLoader.Global;
using static AngelLoader.Misc;
using static AngelLoader.SettingsWindowData;

namespace AngelLoader.Forms;

internal sealed partial class SettingsForm : DarkFormBase, IEventDisabler
{
    #region Private fields

    private readonly Dictionary<Type, IDisposable> _dummyDisposables = new();

    private readonly ISettingsChangeableView? _ownerForm;

    private readonly Timer _thiefBuddyExistenceCheckTimer;
    private bool _thiefBuddyConsideredToExist;

    private readonly SettingsWindowState _state;

    #region Copies of passed-in data

    private readonly string _inLanguage;
    private readonly LText_Class _inLText;

    private readonly VisualTheme _inTheme;
    private readonly bool _inFollowSystemTheme;

    #endregion

    private VisualTheme _selfTheme;

    private sealed class PageButtonAndPage
    {
        internal readonly DarkRadioButtonCustom Button;
        internal readonly ISettingsPage Page;

        public PageButtonAndPage(DarkRadioButtonCustom button, ISettingsPage page)
        {
            Button = button;
            Page = page;
        }
    }

    private readonly PageButtonAndPage[] PageControls;
    private readonly int?[] _pageVScrollValues = new int?[SettingsTabCount];

    // @TDM(Settings/Paths page organization):
    // It's high time we rearrange this page. We should put FM Archives up near the games that need it, and change
    // Other to Backup Path and put that with the games that need it too (or even just have it as fields in the
    // requiring games group), and maybe put TDM separate even.

    private readonly DarkTextBox[] ExePathTextBoxes;
    private readonly Control[] ErrorableControls;

    private readonly DarkLabel[] GameExeLabels;
    private readonly DarkTextBox[] GameExeTextBoxes;
    private readonly StandardButton[] GameExeBrowseButtons;
    private readonly DarkCheckBox[] GameUseSteamCheckBoxes;

    private readonly DarkLabel[] GameWebSearchUrlLabels;
    private readonly DarkTextBox[] GameWebSearchUrlTextBoxes;
    private readonly DarkButton[] GameWebSearchUrlResetButtons;

    #region I/O threading

    private sealed class DriveDataSection
    {
        internal readonly DarkLabel DriveLabel;
        internal readonly DriveMultithreadingLevel AutoMultithreadingLevel;
        internal DriveMultithreadingLevel MultithreadingLevel;
        internal readonly string Drive;
        internal readonly string ModelName;
        internal readonly DarkComboBox ComboBox;

        public DriveDataSection(
            DarkLabel driveLabel,
            DriveMultithreadingLevel autoMultithreadingLevel,
            DriveMultithreadingLevel multithreadingLevel,
            string drive,
            string modelName,
            DarkComboBox comboBox)
        {
            DriveLabel = driveLabel;
            AutoMultithreadingLevel = autoMultithreadingLevel;
            MultithreadingLevel = multithreadingLevel;
            Drive = drive;
            ModelName = modelName;
            ComboBox = comboBox;
        }
    }

    private readonly DriveDataSection[] IOThreadingLevelDriveDataSections;

    #endregion

    // August 4 is chosen more-or-less randomly, but both its name and its number are different short vs. long
    // (Aug vs. August; 8 vs. 08), and the same thing with 4 (4 vs. 04).
    private readonly DateTime _exampleDate = new(DateTime.Now.Year, 8, 4);

    private readonly DarkComboBoxWithBackingItems LangComboBox;
    private readonly DarkGroupBox LangGroupBox;

    private readonly PathsPage PathsPage;
    private readonly AppearancePage AppearancePage;
    private readonly AudioFilesPage AudioFilesPage;
    private readonly OtherPage OtherPage;
    private readonly ThiefBuddyPage ThiefBuddyPage;
    private readonly UpdatePage UpdatePage;
    private readonly IOThreadingPage IOThreadingPage;

    private enum PathError { True, False }

    #region Disablers

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public int EventsDisabled { get; set; }

    #endregion

    #endregion

    public readonly ConfigData OutConfig;
    public bool AskForImport;

    private void CreateButtonAndPage<T>(SettingsTab tab, out T Page) where T : Control, ISettingsPage, new()
    {
        DarkRadioButtonCustom button = new();

        MainSplitContainer.Panel1.Controls.Add(button);

        button.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        button.Location = new Point(8, 8 + (24 * (int)tab));
        button.Size = new Size(136, 23);
        button.TabIndex = (int)tab;
        button.CheckedChanged += PageRadioButtons_CheckedChanged;

        Page = new T { Visible = false };

        PageControls[(int)tab] = new PageButtonAndPage(button, Page);
    }

    private DarkRadioButtonCustom GetPageButton(SettingsTab tab) => PageControls[(int)tab].Button;

    protected override void WndProc(ref Message m)
    {
        if (_state.IsStartup())
        {
            if (m.Msg == Native.WM_THEMECHANGED)
            {
                Win32ThemeHooks.ReloadTheme();
            }
            else if (ControlUtils.SystemThemeHasChanged(ref m, out VisualTheme newTheme))
            {
                Config.VisualTheme = newTheme;
                SetTheme(Config.VisualTheme, Config.FollowSystemTheme, startup: false);
                m.Result = IntPtr.Zero;
            }
        }
        base.WndProc(ref m);
    }

    // @CAN_RUN_BEFORE_VIEW_INIT
    internal SettingsForm(ISettingsChangeableView? ownerForm, ConfigData config, SettingsWindowState state)
    {
        // Simply opening the language files (not even reading them) can take >50ms for the 11 file set on my
        // system, inexplicably. So overlap the read.
        IOrderedEnumerable<KeyValuePair<string, string>>? langsList = null;
        using Task readLanguagesTask = Task.Run(() =>
        {
            langsList = ReadLanguages().OrderBy(static x => x.Key, StringComparer.Ordinal);
        });

#if DEBUG
        InitializeComponent();
#else
        InitSlim();
#endif

        // We have to set this manually for reasons involving the view environment and us potentially being shown
        // without a main form under us
        try
        {
            if (Owner is not MainForm && ownerForm is MainForm mainForm)
            {
                Owner = mainForm;
            }
        }
        catch
        {
            // we're already owned by the main form I guess
        }

        _thiefBuddyExistenceCheckTimer = new Timer();

        _selfTheme = config.VisualTheme;

        _state = state;
        _ownerForm = ownerForm;

        #region Init copies of passed-in data

        _inLanguage = config.Language;
        // Even though this looks like it should be a reference and therefore not work for being a separate
        // object, it somehow does, because I guess we new up LText on read and break the reference and then
        // this copy becomes its own copy...? I don't like that I didn't know that...
        _inLText = LText;

        _inTheme = config.VisualTheme;
        _inFollowSystemTheme = config.FollowSystemTheme;

        #endregion

        OutConfig = new ConfigData();

        PageControls = new PageButtonAndPage[SettingsTabCount];

        CreateButtonAndPage(SettingsTab.Paths, out PathsPage);
        CreateButtonAndPage(SettingsTab.Appearance, out AppearancePage);
        CreateButtonAndPage(SettingsTab.AudioFiles, out AudioFilesPage);
        CreateButtonAndPage(SettingsTab.Other, out OtherPage);
        CreateButtonAndPage(SettingsTab.ThiefBuddy, out ThiefBuddyPage);
        CreateButtonAndPage(SettingsTab.Update, out UpdatePage);
        CreateButtonAndPage(SettingsTab.IOThreading, out IOThreadingPage);

        // IMPORTANT: If there exists a page field that isn't in the array, we'll get a non-compiling null error,
        // but if we have a SettingsTab item but no field yet, then we need this to catch that (at runtime, unfortunately).
        Utils.AssertR(PageControls.All(static x => x != null), nameof(PageControls) + " is missing at least one item");

        LangGroupBox = AppearancePage.LanguageGroupBox;
        LangComboBox = AppearancePage.LanguageComboBox;

        // @GENGAMES (Settings): Begin

        #region Instantiate control arrays

        GameExeLabels = new[]
        {
            PathsPage.Thief1ExePathLabel,
            PathsPage.Thief2ExePathLabel,
            PathsPage.Thief3ExePathLabel,
            PathsPage.SS2ExePathLabel,
            PathsPage.TDMExePathLabel,
        };
        GameExeTextBoxes = new[]
        {
            PathsPage.Thief1ExePathTextBox,
            PathsPage.Thief2ExePathTextBox,
            PathsPage.Thief3ExePathTextBox,
            PathsPage.SS2ExePathTextBox,
            PathsPage.TDMExePathTextBox,
        };
        GameExeBrowseButtons = new[]
        {
            PathsPage.Thief1ExePathBrowseButton,
            PathsPage.Thief2ExePathBrowseButton,
            PathsPage.Thief3ExePathBrowseButton,
            PathsPage.SS2ExePathBrowseButton,
            PathsPage.TDMExePathBrowseButton,
        };
        GameUseSteamCheckBoxes = new[]
        {
            PathsPage.Thief1UseSteamCheckBox,
            PathsPage.Thief2UseSteamCheckBox,
            PathsPage.Thief3UseSteamCheckBox,
            PathsPage.SS2UseSteamCheckBox,
            GetDummy<DarkCheckBox>(),
        };

        GameWebSearchUrlLabels = new[]
        {
            OtherPage.T1WebSearchUrlLabel,
            OtherPage.T2WebSearchUrlLabel,
            OtherPage.T3WebSearchUrlLabel,
            OtherPage.SS2WebSearchUrlLabel,
            OtherPage.TDMWebSearchUrlLabel,
        };
        GameWebSearchUrlTextBoxes = new[]
        {
            OtherPage.T1WebSearchUrlTextBox,
            OtherPage.T2WebSearchUrlTextBox,
            OtherPage.T3WebSearchUrlTextBox,
            OtherPage.SS2WebSearchUrlTextBox,
            OtherPage.TDMWebSearchUrlTextBox,
        };
        GameWebSearchUrlResetButtons = new[]
        {
            OtherPage.T1WebSearchUrlResetButton,
            OtherPage.T2WebSearchUrlResetButton,
            OtherPage.T3WebSearchUrlResetButton,
            OtherPage.SS2WebSearchUrlResetButton,
            OtherPage.TDMWebSearchUrlResetButton,
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

        #endregion

        // These are nullable because null values get put INTO them later. So not a mistake to fill them with
        // non-nullable ints right off the bat.
        for (int i = 0; i < SettingsTabCount; i++)
        {
            SettingsTab tab = (SettingsTab)i;
            _pageVScrollValues[i] = config.GetSettingsTabVScrollPos(tab);
        }

        #region Add pages

        // We set DockStyle here so that it isn't set when we use the designer!
        for (int i = 0; i < SettingsTabCount; i++)
        {
            PageControls[i].Page.Dock = DockStyle.Fill;
        }

        if (_state.IsStartup())
        {
            PagePanel.Controls.Add(PathsPage);

            PathsPage.PagePanel.Controls.Add(LangGroupBox);
            AppearancePage.PagePanel.Controls.Remove(LangGroupBox);
            LangGroupBox.Location = new Point(8, 8);
            LangGroupBox.Width = PathsPage.Width - 16;
            LangGroupBox.MinimumSize = LangGroupBox.MinimumSize with { Width = LangGroupBox.Width };
            PathsPage.ActualPathsPanel.Location = new Point(0, LangGroupBox.Height + 8);
        }
        else
        {
            for (int i = 0; i < SettingsTabCount; i++)
            {
                PagePanel.Controls.Add((Control)PageControls[i].Page);
            }
        }

        #endregion

        #region Set non-page UI state

        // This DisableEvents block is still required because it involves non-page events
        using (new DisableEvents(this))
        {
            if (_state.IsStartup())
            {
                // _Load is too late for some of this stuff, so might as well put everything here
                StartPosition = FormStartPosition.CenterScreen;
                ShowInTaskbar = true;
                DarkRadioButtonCustom pathsButton = GetPageButton(SettingsTab.Paths);
                pathsButton.Checked = true;
                for (int i = 0; i < SettingsTabCount; i++)
                {
                    DarkRadioButtonCustom button = PageControls[i].Button;
                    if (button != pathsButton) button.Hide();
                }
            }
            else
            {
                if (state == SettingsWindowState.BackupPathSet)
                {
                    GetPageButton(SettingsTab.Paths).Checked = true;
                    for (int i = 0; i < PageControls.Length; i++)
                    {
                        if (i != (int)SettingsTab.Paths)
                        {
                            PageControls[i].Button.Hide();
                        }
                    }
                }
                else
                {
                    PageControls[(int)config.SettingsTab].Button.Checked = true;
                }
            }
        }

        #endregion

        Rectangle screenBounds = Screen.FromControl(this).WorkingArea;
        Size = new Size(
            Math.Min(config.SettingsWindowSize.Width, screenBounds.Width),
            Math.Min(config.SettingsWindowSize.Height, screenBounds.Height));
        MainSplitContainer.SplitterDistance = config.SettingsWindowSplitterDistance;

        #region Set page UI state

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

        using (new UpdateRegion(PathsPage.FMArchivePathsListBox))
        {
            PathsPage.FMArchivePathsListBox.Items.Clear();
            foreach (string path in config.FMArchivePaths)
            {
                PathsPage.FMArchivePathsListBox.Items.Add(path);
            }
        }

        PathsPage.IncludeSubfoldersCheckBox.Checked = config.FMArchivePathsIncludeSubfolders;

        #endregion

        if (!_state.IsStartup())
        {
            #region Appearance page

            if (config.FollowSystemTheme)
            {
                AppearancePage.FollowSystemThemeRadioButton.Checked = true;
            }
            else
            {
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

            object[] dateFormatList = ValidDateFormats.Cast<object>().ToArray();
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

            AppearancePage.ShowUninstallButtonCheckBox.Checked = !config.HideUninstallButton;
            AppearancePage.ShowFMListZoomButtonsCheckBox.Checked = !config.HideFMListZoomButtons;
            AppearancePage.ShowExitButtonCheckBox.Checked = !config.HideExitButton;
            AppearancePage.ShowWebSearchButtonCheckBox.Checked = !config.HideWebSearchButton;

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

            #region Audio files page

            AudioFilesPage.ConvertOGGsToWAVsOnInstallCheckBox.Checked = config.ConvertOGGsToWAVsOnInstall;
            AudioFilesPage.ND127_ConvertWAVsTo16BitOnInstallCheckBox.Checked = config.ConvertWAVsTo16BitOnInstall;
            AudioFilesPage.ND128_ConvertMP3sToWAVsOnInstallCheckBox.Checked = config.ConvertMP3sToWAVsOnInstall_ND128;

            #endregion

            #region Thief Buddy page

            switch (config.RunThiefBuddyOnFMPlay)
            {
                case RunThiefBuddyOnFMPlay.Always:
                    ThiefBuddyPage.RunTBAlwaysRadioButton.Checked = true;
                    break;
                case RunThiefBuddyOnFMPlay.Never:
                    ThiefBuddyPage.RunTBNeverRadioButton.Checked = true;
                    break;
                case RunThiefBuddyOnFMPlay.Ask:
                default:
                    ThiefBuddyPage.RunTBAskRadioButton.Checked = true;
                    break;
            }

            CheckThiefBuddyExistence();
            UpdateThiefBuddyExistenceOnUI();

            #endregion

            #region Update page

            UpdatePage.CheckForUpdatesOnStartupCheckBox.Checked = config.CheckForUpdates == CheckForUpdates.True;

            #endregion

            #region I/O Threading page

            DriveLetterDictionary driveLettersAndTypes = new(config.DriveLettersAndTypes.Count);
            config.DriveLettersAndTypes.CopyTo_NoClearDest(driveLettersAndTypes);

            switch (config.IOThreadsMode)
            {
                case IOThreadsMode.Custom:
                    IOThreadingPage.CustomModeRadioButton.Checked = true;
                    break;
                case IOThreadsMode.Auto:
                default:
                    IOThreadingPage.AutoModeRadioButton.Checked = true;
                    break;
            }
            SetIOThreadsState();

            IOThreadingPage.CustomThreadsNumericUpDown.Value = config.CustomIOThreadCount;

            List<SettingsDriveData> settingsDriveData = DetectDriveData.GetSettingsDriveData();
            /*
            If count is 0, then the I/O threading levels group box will end up with min height, which will at
            least provide a sign that something's wrong to the user. For that reason we shouldn't do anything
            slick like hide the group box, because that would just cause confusion if the user is told to do
            something with a group box that isn't there.
            */
            IOThreadingLevelDriveDataSections = new DriveDataSection[settingsDriveData.Count];

            const int driveTypePanelHeight = 64;
            int y = 24;
            int tabIndex = 2;
            for (int i = 0; i < settingsDriveData.Count; i++, y += driveTypePanelHeight, tabIndex++)
            {
                SettingsDriveData driveData = settingsDriveData[i];

                PanelCustom panel = new()
                {
                    Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                    Location = new Point(8, y),
                    Size = new Size(IOThreadingPage.IOThreadingLevelGroupBox.Width - 16, driveTypePanelHeight - 20),
                    TabIndex = tabIndex,
                };
                DarkLabel driveLabel = new()
                {
                    AutoSize = true,
                    TabIndex = tabIndex + 1,
                };
                DarkComboBox comboBox = new()
                {
                    Location = new Point(8, 20),
                    MinimumSize = new Size(160, 0),
                    TabIndex = tabIndex + 2,
                    TabStop = true,

                    Tag = i,
                };
                comboBox.Items.AddRange(new object[] { "", "", "", "" });
                comboBox.SelectedIndex = 0;

                panel.Controls.Add(driveLabel);
                panel.Controls.Add(comboBox);

                DriveMultithreadingLevel driveMultithreadingLevel =
                    ConfigData.GetDriveThreadability(
                        driveLettersAndTypes,
                        driveData.Root);

                IOThreadingLevelDriveDataSections[i] = new DriveDataSection(
                    driveLabel: driveLabel,
                    autoMultithreadingLevel: driveData.MultithreadingLevel,
                    multithreadingLevel: driveMultithreadingLevel,
                    drive: driveData.Root,
                    modelName: driveData.ModelName,
                    comboBox: comboBox
                );

                IOThreadingPage.IOThreadingLevelGroupBox.Controls.Add(panel);

                if (i < settingsDriveData.Count - 1)
                {
                    IOThreadingPage.HorizDivYPositions.Add(y + (driveTypePanelHeight - 20));
                }

                comboBox.SelectedIndex = driveMultithreadingLevel switch
                {
                    DriveMultithreadingLevel.Single => 1,
                    DriveMultithreadingLevel.Read => 2,
                    DriveMultithreadingLevel.ReadWrite => 3,
                    _ => 0,
                };
                comboBox.SelectedIndexChanged += ComboBox_SelectedIndexChanged;
            }
            IOThreadingPage.IOThreadingLevelGroupBox.Size = IOThreadingPage.IOThreadingLevelGroupBox.Size with
            {
                Height = y - 8,
            };

            #endregion

            #region Other page

            OtherPage.OldMantleForOldDarkFMsCheckBox.Checked = config.UseOldMantlingForOldDarkFMs;

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

            for (int i = 0; i < SupportedGameCount; i++)
            {
                GameWebSearchUrlTextBoxes[i].Text = config.WebSearchUrls[i];
            }

            OtherPage.ConfirmPlayOnDCOrEnterCheckBox.Checked = config.ConfirmPlayOnDCOrEnter;

            OtherPage.EnableFuzzySearchCheckBox.Checked = config.EnableFuzzySearch;

            OtherPage.AlwaysShowPresetTagsCheckBox.Checked = config.ShowPresetTags;

            #endregion
        }
        else
        {
            IOThreadingLevelDriveDataSections = Array.Empty<DriveDataSection>();
        }

        #endregion

        if (_inTheme != VisualTheme.Classic)
        {
            SetTheme(_selfTheme, config.FollowSystemTheme, startup: true);
        }
        else
        {
            ErrorIconPictureBox.Image = Images.RedExclCircle;
        }

        // Do this last so that language reading can have the longest overlap window possible
        #region Load languages

        const string engLang = "English";

        readLanguagesTask.Wait();

        using (new UpdateRegion(LangComboBox))
        {
            LangComboBox.AddFullItem(engLang, engLang);
            if (langsList != null)
            {
                foreach (var item in langsList)
                {
                    if (!item.Key.EqualsI(engLang))
                    {
                        LangComboBox.AddFullItem(item.Key, item.Value);
                    }
                }
            }
        }

        LangComboBox.SelectBackingIndexOf(LangComboBox.BackingItems.Contains(config.Language, StringComparer.Ordinal)
            ? config.Language
            : engLang);

        #endregion

        // Comes last so we don't have to use any DisableEvents blocks
        #region Hook up page events

        #region Paths page

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

        #endregion

        LangComboBox.SelectedIndexChanged += LanguageComboBox_SelectedIndexChanged;

        if (!_state.IsStartup())
        {
            #region Appearance page

            AppearancePage.ClassicThemeRadioButton.CheckedChanged += VisualThemeRadioButtons_CheckedChanged;
            AppearancePage.DarkThemeRadioButton.CheckedChanged += VisualThemeRadioButtons_CheckedChanged;
            AppearancePage.FollowSystemThemeRadioButton.CheckedChanged += VisualThemeRadioButtons_CheckedChanged;

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

            #endregion

            #region Thief Buddy page

            _thiefBuddyExistenceCheckTimer.Tick += ThiefBuddyExistenceCheckTimer_Tick;
            _thiefBuddyExistenceCheckTimer.Interval = 1000;
            _thiefBuddyExistenceCheckTimer.Start();

            ThiefBuddyPage.GetTBLinkLabel.LinkClicked += ThiefBuddyPage_GetTBLinkLabel_LinkClicked;

            #endregion

            #region I/O Threading page

            IOThreadingPage.AutoModeRadioButton.CheckedChanged += IOThreadsRadioButtons_CheckedChanged;
            IOThreadingPage.CustomModeRadioButton.CheckedChanged += IOThreadsRadioButtons_CheckedChanged;
            IOThreadingPage.IOThreadsResetButton.Click += IOThreadsResetButton_Click;

            #endregion

            #region Other page

            foreach (DarkButton button in GameWebSearchUrlResetButtons)
            {
                button.Click += WebSearchURLResetButtons_Click;
            }

            #endregion
        }

        #endregion
    }

    private static DictionaryI<string> ReadLanguages()
    {
        DictionaryI<string> ret = new();

        List<string> langFiles;
        try
        {
            langFiles = FastIO.GetFilesTopOnly(Paths.Languages, "*.ini");
        }
        catch (Exception ex)
        {
            ResetLanguages();
            Log(ErrorText.ExTry + "get language .ini files from " + Paths.Languages, ex);
            return ret;
        }

        ret.Clear();

        try
        {
            for (int i = 0; i < langFiles.Count; i++)
            {
                string fileName = langFiles[i];
                string fileNameWithoutExtension = fileName.GetFileNameFast().RemoveExtension();

                Ini.AddLanguageFromFile(fileName, fileNameWithoutExtension, ret);
            }
        }
        catch (Exception ex)
        {
            ResetLanguages();
            Log(ErrorText.Ex + "in language files read", ex);
        }

        return ret;

        void ResetLanguages() => ret.Clear();
    }

    private void ComboBox_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (sender is DarkComboBox { Tag: int index } comboBox)
        {
            IOThreadingLevelDriveDataSections[index].MultithreadingLevel = comboBox.SelectedIndex switch
            {
                1 => DriveMultithreadingLevel.Single,
                2 => DriveMultithreadingLevel.Read,
                3 => DriveMultithreadingLevel.ReadWrite,
                _ => DriveMultithreadingLevel.Auto,
            };
        }
    }

    protected override void OnLoad(EventArgs e)
    {
        foreach (var pageControl in PageControls)
        {
            if (pageControl.Button.Checked)
            {
                ShowPage(Array.IndexOf(PageControls, pageControl), initialCall: true);
                break;
            }
        }

        Localize(suspendResume: false);

        /*
        This could maybe feel intrusive if we do it every time we open, so only do it on startup. It's
        much more important / useful to do it on startup, because we're likely only to open on startup
        if there's already an error. But, we also want to suppress errors if we're starting for the first
        time ever. In that case, invalid fields aren't conceptually "errors", but rather the user just
        hasn't filled them in yet. We'll error on OK click if we have to, but present a pleasanter UX
        prior to then.
        */

        switch (_state)
        {
            case SettingsWindowState.BackupPathSet:
                ShowPathError(PathsPage.BackupPathTextBox, true);
                EnsureControlIsInView(PathsPage.PagePanel, PathsPage.BackupPathTextBox);
                break;
            case SettingsWindowState.Startup:
                CheckForErrors();
                break;
        }

        base.OnLoad(e);
    }

    protected override void OnShown(EventArgs e)
    {
        // We have to do this here, in _Shown, otherwise it doesn't do its initial layout and might miss if
        // there's supposed to be scroll bars or whatever else... this makes it visually correct. Don't ask
        // questions.
        PathsPage.DoLayout = true;
        PathsPage.LayoutFLP.PerformLayout();
        IOThreadingPage.DoLayout = true;
        IOThreadingPage.LayoutFLP.PerformLayout();

        base.OnShown(e);
    }

    public override void RespondToSystemThemeChange()
    {
        SetTheme(Config.VisualTheme, Config.FollowSystemTheme, startup: false);
    }

    private void SetTheme(VisualTheme theme, bool followSystemTheme, bool startup)
    {
        _selfTheme = theme;

        // Some parts of the code check this (eg. theme renderers) so we need to set it. We'll revert it
        // back to the passed-in theme if we cancel.
        Config.VisualTheme = theme;
        Config.FollowSystemTheme = followSystemTheme;

        try
        {
            if (!startup) MainSplitContainer.SuspendDrawing();

            SetThemeBase(
                theme,
                static x => x is SplitterPanel,
                capacity: 150
            );

            if (!startup) SetRatingImage();
            foreach (Control c in ErrorableControls)
            {
                ShowPathError(c, PathErrorIsSet(c));
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

    private void Localize(bool suspendResume = true)
    {
        if (suspendResume) MainSplitContainer.SuspendDrawing();
        try
        {
            Text = _state.IsStartup() ? LText.SettingsWindow.StartupTitleText : LText.SettingsWindow.TitleText;

            OKButton.Text = LText.Global.OK;
            Cancel_Button.Text = LText.Global.Cancel;

            #region Paths page

            GetPageButton(SettingsTab.Paths).Text = _state.IsStartup()
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

            PathsPage.BackupGroupBox.Text = LText.SettingsWindow.Paths_BackupPath;
            PathsPage.BackupPathBrowseButton.SetTextForTextBoxButtonCombo(PathsPage.BackupPathTextBox, LText.Global.BrowseEllipses);

            PathsPage.BackupPathHelpLabel.Text = LText.SettingsWindow.Paths_BackupPath_Info;
            PathsPage.BackupPathTDMHelpLabel.Text = LText.SettingsWindow.Paths_BackupPath_Required;
            // Required for the startup version where the lang box is on the same page as paths!
            PathsPage.LayoutFLP.PerformLayout();

            PathsPage.FMArchivePathsGroupBox.Text = LText.SettingsWindow.Paths_FMArchivePaths;
            PathsPage.IncludeSubfoldersCheckBox.Text = LText.SettingsWindow.Paths_IncludeSubfolders;
            MainToolTip.SetToolTip(PathsPage.AddFMArchivePathButton, LText.SettingsWindow.Paths_AddArchivePathToolTip);
            MainToolTip.SetToolTip(PathsPage.RemoveFMArchivePathButton, LText.SettingsWindow.Paths_RemoveArchivePathToolTip);

            ErrorLabel.Text = LText.SettingsWindow.Paths_ErrorSomePathsAreInvalid;

            #endregion

            if (_state.IsStartup())
            {
                LangGroupBox.Text = LText.SettingsWindow.Appearance_Language;
            }
            else
            {
                #region Appearance page

                GetPageButton(SettingsTab.Appearance).Text = LText.SettingsWindow.Appearance_TabText;

                AppearancePage.LanguageGroupBox.Text = LText.SettingsWindow.Appearance_Language;

                AppearancePage.VisualThemeGroupBox.Text = LText.SettingsWindow.Appearance_Theme;
                AppearancePage.ClassicThemeRadioButton.Text = LText.SettingsWindow.Appearance_Theme_Classic;
                AppearancePage.DarkThemeRadioButton.Text = LText.SettingsWindow.Appearance_Theme_Dark;
                AppearancePage.FollowSystemThemeRadioButton.Text = LText.SettingsWindow.Appearance_Theme_FollowSystem;

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
                AppearancePage.ShowUninstallButtonCheckBox.Text = LText.SettingsWindow.Appearance_ShowUninstallButton;
                AppearancePage.ShowFMListZoomButtonsCheckBox.Text = LText.SettingsWindow.Appearance_ShowFMListZoomButtons;
                AppearancePage.ShowExitButtonCheckBox.Text = LText.SettingsWindow.Appearance_ShowExitButton;
                AppearancePage.ShowWebSearchButtonCheckBox.Text = LText.SettingsWindow.Appearance_ShowWebSearchButton;

                AppearancePage.ReadmeGroupBox.Text = LText.SettingsWindow.Appearance_ReadmeBox;
                AppearancePage.ReadmeFixedWidthFontCheckBox.Text = LText.SettingsWindow.Appearance_ReadmeUseFixedWidthFont;

                AppearancePage.PlayWithoutFMGroupBox.Text = LText.SettingsWindow.Appearance_PlayWithoutFM;
                AppearancePage.PlayWithoutFM_SingleButtonRadioButton.Text = LText.SettingsWindow.Appearance_PlayWithoutFM_SingleButton;
                AppearancePage.PlayWithoutFM_MultipleButtonsRadioButton.Text = LText.SettingsWindow.Appearance_PlayWithoutFM_MultiButton;

                #endregion

                #region Audio Files page

                GetPageButton(SettingsTab.AudioFiles).Text = LText.SettingsWindow.AudioFiles_TabText;

                AudioFilesPage.GlobalGroupBox.Text = LText.SettingsWindow.AudioFiles_AllNewDarkVersions;

                AudioFilesPage.ConvertOGGsToWAVsOnInstallCheckBox.Text = LText.SettingsWindow.AudioFiles_ConvertOGGsToWAVsOnInstall;
                MainToolTip.SetToolTip(AudioFilesPage.ConvertOGGsToWAVsOnInstallCheckBox, LText.SettingsWindow.AudioFiles_ConvertOGGsToWAVsOnInstall_ToolTip);

                AudioFilesPage.ND127GroupBox.Text = LText.SettingsWindow.AudioFiles_NewDark_127;

                AudioFilesPage.ND127_ConvertWAVsTo16BitOnInstallCheckBox.Text = LText.SettingsWindow.AudioFiles_ConvertWAVsTo16BitOnInstall_NewDark_127;
                MainToolTip.SetToolTip(AudioFilesPage.ND127_ConvertWAVsTo16BitOnInstallCheckBox, LText.SettingsWindow.AudioFiles_ConvertWAVsTo16BitOnInstall_ToolTip);

                AudioFilesPage.ND127_ConvertMP3sToWAVsOnInstallCheckBox.Text = LText.SettingsWindow.AudioFiles_ConvertMP3sToWavOnInstall_NewDark_127;

                AudioFilesPage.ND128GroupBox.Text = LText.SettingsWindow.AudioFiles_NewDark_128;

                AudioFilesPage.ND128_ConvertWAVsTo16BitOnInstallCheckBox.Text = LText.SettingsWindow.AudioFiles_ConvertWAVsTo16BitOnInstall_NewDark_128;
                AudioFilesPage.ND128_ConvertMP3sToWAVsOnInstallCheckBox.Text = LText.SettingsWindow.AudioFiles_ConvertMP3sToWavOnInstall_NewDark_128;

                // @ND128: We may want a tooltip for mp3 conversion and whatever else

                #endregion

                #region Thief Buddy page

                GetPageButton(SettingsTab.ThiefBuddy).Text = NonLocalizableText.ThiefBuddy;

                ThiefBuddyPage.ThiefBuddyOptionsGroupBox.Text = LText.SettingsWindow.ThiefBuddy_ThiefBuddyOptions;

                UpdateThiefBuddyStatusLabel();

                ThiefBuddyPage.RunThiefBuddyWhenPlayingFMsLabel.Text = LText.SettingsWindow.ThiefBuddy_RunThiefBuddyWhenPlayingFMs;
                ThiefBuddyPage.RunTBAlwaysRadioButton.Text = LText.SettingsWindow.ThiefBuddy_RunAlways;
                ThiefBuddyPage.RunTBAskRadioButton.Text = LText.SettingsWindow.ThiefBuddy_RunAskEveryTime;
                ThiefBuddyPage.RunTBNeverRadioButton.Text = LText.SettingsWindow.ThiefBuddy_RunNever;

                ThiefBuddyPage.WhatIsTBHelpLabel.Text = LText.SettingsWindow.ThiefBuddy_Help;
                ThiefBuddyPage.GetTBLinkLabel.Text = LText.SettingsWindow.ThiefBuddy_Get;

                #endregion

                #region Update page

                GetPageButton(SettingsTab.Update).Text = LText.SettingsWindow.Update_TabText;
                UpdatePage.UpdateOptionsGroupBox.Text = LText.SettingsWindow.Update_UpdateOptions;
                UpdatePage.CheckForUpdatesOnStartupCheckBox.Text = LText.SettingsWindow.Update_CheckForUpdatesOnStartup;

                #endregion

                #region I/O Threading page

                GetPageButton(SettingsTab.IOThreading).Text = LText.SettingsWindow.IOThreading_TabText;

                IOThreadingPage.IOThreadCountGroupBox.Text = LText.SettingsWindow.IOThreading_IOThreads;

                IOThreadingPage.AutoModeRadioButton.Text = LText.SettingsWindow.IOThreading_IOThreads_Auto;

                IOThreadingPage.CustomModeRadioButton.Text = LText.SettingsWindow.IOThreading_IOThreads_Custom;
                IOThreadingPage.CustomThreadsLabel.Text = LText.SettingsWindow.IOThreading_IOThreads_Threads;

                IOThreadingPage.IOThreadingLevelGroupBox.Text = LText.SettingsWindow.IOThreading_IOThreadingLevels;

                IOThreadingPage.HelpLabel.Text = LText.SettingsWindow.IOThreading_HelpMessage;

                foreach (DriveDataSection section in IOThreadingLevelDriveDataSections)
                {
                    section.DriveLabel.Text = section.Drive + " " + section.ModelName;

                    section.ComboBox.Items[0] = GetAutodetectedThreadingLevelString(section.AutoMultithreadingLevel);
                    section.ComboBox.Items[1] = GetThreadingLevelString(DriveMultithreadingLevel.Single);
                    section.ComboBox.Items[2] = GetThreadingLevelString(DriveMultithreadingLevel.Read);
                    section.ComboBox.Items[3] = GetThreadingLevelString(DriveMultithreadingLevel.ReadWrite);
                }

                DarkComboBox[] threadingLevelComboBoxes = new DarkComboBox[IOThreadingLevelDriveDataSections.Length];
                for (int i = 0; i < IOThreadingLevelDriveDataSections.Length; i++)
                {
                    threadingLevelComboBoxes[i] = IOThreadingLevelDriveDataSections[i].ComboBox;
                }
                DarkComboBox.DoAutoSizeForSet(threadingLevelComboBoxes, rightPadding: 16);

                static string GetAutodetectedThreadingLevelString(DriveMultithreadingLevel threadability)
                {
                    return threadability switch
                    {
                        DriveMultithreadingLevel.Read => LText.SettingsWindow.IOThreading_IOThreadingLevels_Auto_MultithreadedRead,
                        _ => LText.SettingsWindow.IOThreading_IOThreadingLevels_Auto_SingleThread,
                    };
                }

                static string GetThreadingLevelString(DriveMultithreadingLevel threadability)
                {
                    return threadability switch
                    {
                        DriveMultithreadingLevel.ReadWrite => LText.SettingsWindow.IOThreading_IOThreadingLevels_MultithreadedReadAndWrite,
                        DriveMultithreadingLevel.Read => LText.SettingsWindow.IOThreading_IOThreadingLevels_MultithreadedRead,
                        _ => LText.SettingsWindow.IOThreading_IOThreadingLevels_SingleThread,
                    };
                }

                #endregion

                #region Other page

                GetPageButton(SettingsTab.Other).Text = LText.SettingsWindow.Other_TabText;

                OtherPage.FMSettingsGroupBox.Text = LText.SettingsWindow.Other_FMSettings;

                OtherPage.OldMantleForOldDarkFMsCheckBox.Text = LText.SettingsWindow.Other_UseOldMantlingForOldDarkFMs;

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

                for (int i = 0; i < SupportedGameCount; i++)
                {
                    GameWebSearchUrlLabels[i].Text = GetLocalizedGameNameColon((GameIndex)i);
                    MainToolTip.SetToolTip(GameWebSearchUrlResetButtons[i], LText.SettingsWindow.Other_WebSearchResetToolTip);
                }

                OtherPage.PlayFMOnDCOrEnterGroupBox.Text = LText.SettingsWindow.Other_ConfirmPlayOnDCOrEnter;
                OtherPage.ConfirmPlayOnDCOrEnterCheckBox.Text = LText.SettingsWindow.Other_ConfirmPlayOnDCOrEnter_Ask;

                OtherPage.FilteringGroupBox.Text = LText.SettingsWindow.Other_Filtering;
                OtherPage.EnableFuzzySearchCheckBox.Text = LText.SettingsWindow.Other_EnableFuzzySearch;

                OtherPage.TagsGroupBox.Text = LText.SettingsWindow.Other_Tags;
                OtherPage.AlwaysShowPresetTagsCheckBox.Text = LText.SettingsWindow.Other_AlwaysShowPresetTags;
                MainToolTip.SetToolTip(OtherPage.AlwaysShowPresetTagsCheckBox, LText.SettingsWindow.Other_AlwaysShowPresetTags_ToolTip);

                #endregion
            }
        }
        finally
        {
            if (suspendResume) MainSplitContainer.ResumeDrawing();
        }
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
#if DEBUG || (Release_Testing && !RT_StartupOnly)
        if (e.Control && e.KeyCode == Keys.E)
        {
            PagePanel.Enabled = !PagePanel.Enabled;
            return;
        }
#endif

        if (e.KeyCode == Keys.Escape)
        {
            if (MainSplitContainer.Resizing)
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
            int pageIndex = Array.FindIndex(PageControls, static x => x.Page.Visible);
            if (pageIndex > -1)
            {
                string section =
                    _state.IsStartup()
                        ? HelpSections.InitialSettings
                        : HelpSections.SettingsPages[pageIndex];

                if (!section.IsEmpty()) Core.OpenHelpFile(section);
            }
        }

        base.OnKeyDown(e);
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        base.OnFormClosing(e);

        #region Save window state

        // Special case: these are meta, so they should always be set even if the user clicked Cancel
        OutConfig.SettingsTab = SettingsTab.Paths;
        for (int i = 0; i < SettingsTabCount; i++)
        {
            if (PageControls[i].Button.Checked)
            {
                OutConfig.SettingsTab = (SettingsTab)i;
                break;
            }
        }

        OutConfig.SettingsWindowSize = Size;
        OutConfig.SettingsWindowSplitterDistance = MainSplitContainer.SplitterDistance;

        // If some pages haven't had their vertical scroll value loaded, just take the value from the backing
        // store
        for (int i = 0; i < SettingsTabCount; i++)
        {
            SettingsTab tab = (SettingsTab)i;
            OutConfig.SetSettingsTabVScrollPos(tab, _pageVScrollValues[i] ?? PageControls[i].Page.GetVScrollPos());
        }

        #endregion

        #region Cancel

        if (DialogResult != DialogResult.OK)
        {
            if (!_state.IsStartup())
            {
                bool langsDifferent = !LangComboBox.SelectedBackingItem().EqualsI(_inLanguage);
                bool newFollowSystemTheme = AppearancePage.FollowSystemThemeRadioButton.Checked;
                VisualTheme newTheme = Core.GetSystemTheme();
                bool themesDifferent = _inTheme != _selfTheme || newFollowSystemTheme != _inFollowSystemTheme || newTheme != _inTheme;

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

                    if (_inFollowSystemTheme)
                    {
                        Config.VisualTheme = Core.GetSystemTheme();
                        Config.FollowSystemTheme = true;
                        _ownerForm?.SetTheme(Config.VisualTheme);
                    }
                    else if (themesDifferent)
                    {
                        Config.VisualTheme = _inTheme;
                        Config.FollowSystemTheme = _inFollowSystemTheme;
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

        if (!_state.IsStartup()) FormatArticles();

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
        foreach (string path in PathsPage.FMArchivePathsListBox.ItemsAsStrings)
        {
            OutConfig.FMArchivePaths.Add(path.Trim());
        }

        OutConfig.FMArchivePathsIncludeSubfolders = PathsPage.IncludeSubfoldersCheckBox.Checked;

        #endregion

        if (_state.IsStartup())
        {
            OutConfig.Language = LangComboBox.SelectedBackingItem();
        }
        else
        {
            #region Appearance page

            OutConfig.Language = LangComboBox.SelectedBackingItem();

            if (AppearancePage.FollowSystemThemeRadioButton.Checked)
            {
                OutConfig.VisualTheme = Core.GetSystemTheme();
                OutConfig.FollowSystemTheme = true;
            }
            else
            {
                OutConfig.VisualTheme =
                    AppearancePage.DarkThemeRadioButton.Checked
                        ? VisualTheme.Dark
                        : VisualTheme.Classic;
                OutConfig.FollowSystemTheme = false;
            }

            #region Game organization

            OutConfig.GameOrganization = AppearancePage.OrganizeGamesByTabRadioButton.Checked
                ? GameOrganization.ByTab
                : GameOrganization.OneList;

            OutConfig.UseShortGameTabNames = AppearancePage.UseShortGameTabNamesCheckBox.Checked;

            #endregion

            #region Articles

            OutConfig.EnableArticles = AppearancePage.EnableIgnoreArticlesCheckBox.Checked;

            List<string> retArticles = AppearancePage.ArticlesTextBox.Text
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

            OutConfig.Articles.ClearAndAdd_Small(retArticles);

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

            OutConfig.DateCustomFormat1 = AppearancePage.Date1ComboBox.SelectedItem.ToStringOrEmpty();
            OutConfig.DateCustomSeparator1 = AppearancePage.DateSeparator1TextBox.Text;
            OutConfig.DateCustomFormat2 = AppearancePage.Date2ComboBox.SelectedItem.ToStringOrEmpty();
            OutConfig.DateCustomSeparator2 = AppearancePage.DateSeparator2TextBox.Text;
            OutConfig.DateCustomFormat3 = AppearancePage.Date3ComboBox.SelectedItem.ToStringOrEmpty();
            OutConfig.DateCustomSeparator3 = AppearancePage.DateSeparator3TextBox.Text;
            OutConfig.DateCustomFormat4 = AppearancePage.Date4ComboBox.SelectedItem.ToStringOrEmpty();

            #endregion

            OutConfig.DaysRecent = (uint)AppearancePage.RecentFMsNumericUpDown.Value;

            #region Show/hide UI elements

            OutConfig.HideUninstallButton = !AppearancePage.ShowUninstallButtonCheckBox.Checked;
            OutConfig.HideFMListZoomButtons = !AppearancePage.ShowFMListZoomButtonsCheckBox.Checked;
            OutConfig.HideExitButton = !AppearancePage.ShowExitButtonCheckBox.Checked;
            OutConfig.HideWebSearchButton = !AppearancePage.ShowWebSearchButtonCheckBox.Checked;

            #endregion

            OutConfig.ReadmeUseFixedWidthFont = AppearancePage.ReadmeFixedWidthFontCheckBox.Checked;

            OutConfig.PlayOriginalSeparateButtons = AppearancePage.PlayWithoutFM_MultipleButtonsRadioButton.Checked;

            #endregion

            #region Audio files page

            OutConfig.ConvertOGGsToWAVsOnInstall = AudioFilesPage.ConvertOGGsToWAVsOnInstallCheckBox.Checked;
            OutConfig.ConvertWAVsTo16BitOnInstall = AudioFilesPage.ND127_ConvertWAVsTo16BitOnInstallCheckBox.Checked;
            OutConfig.ConvertMP3sToWAVsOnInstall_ND128 = AudioFilesPage.ND128_ConvertMP3sToWAVsOnInstallCheckBox.Checked;

            #endregion

            #region Thief Buddy page

            OutConfig.RunThiefBuddyOnFMPlay =
                ThiefBuddyPage.RunTBAlwaysRadioButton.Checked ? RunThiefBuddyOnFMPlay.Always :
                ThiefBuddyPage.RunTBNeverRadioButton.Checked ? RunThiefBuddyOnFMPlay.Never :
                RunThiefBuddyOnFMPlay.Ask;

            #endregion

            #region Update page

            OutConfig.CheckForUpdates = UpdatePage.CheckForUpdatesOnStartupCheckBox.Checked
                ? CheckForUpdates.True
                : CheckForUpdates.False;

            #endregion

            #region I/O Threading page

            OutConfig.IOThreadsMode =
                IOThreadingPage.CustomModeRadioButton.Checked ? IOThreadsMode.Custom :
                IOThreadsMode.Auto;

            OutConfig.CustomIOThreadCount = (int)IOThreadingPage.CustomThreadsNumericUpDown.Value;

            foreach (DriveDataSection section in IOThreadingLevelDriveDataSections)
            {
                OutConfig.DriveLettersAndTypes[section.Drive[0]] = section.MultithreadingLevel;
            }

            #endregion

            #region Other page

            OutConfig.UseOldMantlingForOldDarkFMs = OtherPage.OldMantleForOldDarkFMsCheckBox.Checked;

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

            for (int i = 0; i < SupportedGameCount; i++)
            {
                OutConfig.WebSearchUrls[i] = GameWebSearchUrlTextBoxes[i].Text;
            }

            OutConfig.ConfirmPlayOnDCOrEnter = OtherPage.ConfirmPlayOnDCOrEnterCheckBox.Checked;

            OutConfig.EnableFuzzySearch = OtherPage.EnableFuzzySearchCheckBox.Checked;

            OutConfig.ShowPresetTags = OtherPage.AlwaysShowPresetTagsCheckBox.Checked;

            #endregion
        }

        AskForImport = _state == SettingsWindowState.StartupClean;
    }

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

    #region Page selection handler

    private void PageRadioButtons_CheckedChanged(object sender, EventArgs e)
    {
        if (EventsDisabled > 0) return;

        DarkRadioButtonCustom s = (DarkRadioButtonCustom)sender;
        if (!s.Checked) return;

        using (new DisableEvents(this))
        {
            foreach (var pageControl in PageControls)
            {
                if (s != pageControl.Button) pageControl.Button.Checked = false;
            }
        }

        ShowPage(Array.FindIndex(PageControls, x => x.Button == s));
    }

    private void ShowPage(int index, bool initialCall = false)
    {
        if (PageControls[index].Page.Visible) return;

        if (_state.IsStartup())
        {
            // Don't bother with position saving if this is the Initial Settings window
            PathsPage.Show();
        }
        else
        {
            int pagesLength = PageControls.Length;
            if (index < 0 || index > pagesLength - 1) return;

            bool pagePosWasStored = _pageVScrollValues[index] != null;
            try
            {
                if (!initialCall && pagePosWasStored) MainSplitContainer.SuspendDrawing();

                PageControls[index].Page.Show();
                for (int i = 0; i < pagesLength; i++)
                {
                    if (i != index) PageControls[i].Page.Hide();
                }

                // Lazy-load for faster initial startup
                if (pagePosWasStored)
                {
                    PageControls[index].Page.SetVScrollPos((int)_pageVScrollValues[index]!);
                    if (!initialCall)
                    {
                        // Infuriating hack to get the scroll bar to show up in the right position (the content
                        // already does)
                        PageControls[index].Page.Hide();
                        PageControls[index].Page.Show();
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
        DarkTextBox exePathTextBox = (DarkTextBox)sender;
        ShowPathError(exePathTextBox, !exePathTextBox.Text.IsEmpty() && !File.Exists(exePathTextBox.Text));
        ShowPathError(PathsPage.BackupPathTextBox, BackupPathInvalid_Settings(PathsPage.BackupPathTextBox.Text, GameExeTextBoxes));
    }

    private void ExePathBrowseButtons_Click(object sender, EventArgs e)
    {
        string? dialogTitle = null;
        DarkTextBox? tb = null;
        for (int i = 0; i < SupportedGameCount; i++)
        {
            GameIndex gameIndex = (GameIndex)i;
            if (sender == GameExeBrowseButtons[i])
            {
                dialogTitle = GetLocalizedSelectGameExecutableMessage(gameIndex);
                tb = GameExeTextBoxes[i];
                break;
            }
        }
        tb ??= PathsPage.SteamExeTextBox;
        dialogTitle ??= LText.SettingsWindow.Paths_ChooseSteamExe_DialogTitle;

        string initialPath = "";
        try
        {
            initialPath = Path.GetDirectoryName(tb.Text) ?? "";
        }
        catch
        {
            // ignore
        }

        (DialogResult result, string fileName) = BrowseForExeFile(initialPath, dialogTitle);
        if (result == DialogResult.OK) tb.Text = fileName;

        ShowPathError(tb, !tb.Text.IsEmpty() && !File.Exists(tb.Text));
    }

    private static bool BackupPathInvalid_Settings(string backupPath, DarkTextBox[] gameExeTextBoxes)
    {
        for (int i = 0; i < SupportedGameCount; i++)
        {
            GameIndex gameIndex = (GameIndex)i;
            if (GameRequiresBackupPath(gameIndex) &&
                !gameExeTextBoxes[i].Text.IsEmpty() &&
                !Directory.Exists(backupPath))
            {
                return true;
            }
        }

        return false;
    }

    private void BackupPathTextBox_Leave(object sender, EventArgs e)
    {
        DarkTextBox s = (DarkTextBox)sender;
        ShowPathError(s, BackupPathInvalid_Settings(s.Text, GameExeTextBoxes));
    }

    private static string SanitizePathForDialog(string path)
    {
        if (!path.IsWhiteSpace() && !Directory.Exists(path))
        {
            try
            {
                // C:\Folder\File.exe becomes C:\Folder
                path = Path.GetDirectoryName(path) ?? "";
                return Directory.Exists(path) ? path : "";
            }
            catch
            {
                return "";
            }
        }

        return path;
    }

    private void BackupPathBrowseButton_Click(object sender, EventArgs e)
    {
        DarkTextBox tb = PathsPage.BackupPathTextBox;

        using (VistaFolderBrowserDialog d = new())
        {
            d.Title = LText.SettingsWindow.Paths_ChooseBackupPath_DialogTitle;
            d.InitialDirectory = SanitizePathForDialog(tb.Text);
            d.MultiSelect = false;
            if (d.ShowDialogDark(this) == DialogResult.OK) tb.Text = d.DirectoryName;
        }

        ShowPathError(tb, !Directory.Exists(tb.Text));
    }

    private (DialogResult Result, string FileName)
    BrowseForExeFile(string initialPath, string title)
    {
        using OpenFileDialog dialog = new();
        dialog.Title = title;
        dialog.InitialDirectory = initialPath;
        dialog.Filter = LText.BrowseDialogs.ExeFiles + "|*.exe";
        return (dialog.ShowDialogDark(this), dialog.FileName);
    }

    private void SteamExeTextBox_TextChanged(object sender, EventArgs e)
    {
        PathsPage.LaunchTheseGamesThroughSteamPanel.Enabled = !PathsPage.SteamExeTextBox.Text.IsWhiteSpace();
    }

    private void SetUseSteamGameCheckBoxesEnabled(bool enabled)
    {
        foreach (DarkCheckBox checkBox in GameUseSteamCheckBoxes)
        {
            checkBox.Enabled = enabled;
        }
    }

    private void LaunchTheseGamesThroughSteamCheckBox_CheckedChanged(object sender, EventArgs e)
    {
        SetUseSteamGameCheckBoxesEnabled(PathsPage.LaunchTheseGamesThroughSteamCheckBox.Checked);
    }

    #region Archive paths

    private void AddFMArchivePathButton_Click(object sender, EventArgs e)
    {
        using VistaFolderBrowserDialog d = new();

        d.Title = LText.SettingsWindow.Paths_AddFMArchivePath_DialogTitle;

        DarkListBox lb = PathsPage.FMArchivePathsListBox;
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
            using (new UpdateRegion(PathsPage.FMArchivePathsListBox))
            {
                HashSetPathI hash = PathsPage.FMArchivePathsListBox.ItemsAsStrings.ToHashSetPathI();

                foreach (string dir in d.DirectoryNames)
                {
                    if (!hash.Contains(dir)) PathsPage.FMArchivePathsListBox.Items.Add(dir);
                }
            }
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

    private void LanguageComboBox_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (EventsDisabled > 0) return;

        string lang = LangComboBox.SelectedBackingItem();
        try
        {
            LText = new LText_Class();
            Ini.ReadLocalizationIni(Path.Combine(Paths.Languages, lang + ".ini"), LText);
        }
        catch (Exception ex)
        {
            if (!lang.EqualsI("English"))
            {
                string msg = ex is FileNotFoundException
                    ? "Language file not found."
                    : ErrorText.Un + "read language file.";

                Core.Dialogs.ShowAlert(msg + " Falling back to English.", LText.AlertMessages.Alert);

                using (new DisableEvents(this))
                {
                    LangComboBox.SelectedIndex = 0;
                }

                Log(ErrorText.Ex + "in language reading", ex);
            }

            // If we wanted to be really fancy we could fall back to the previously selected language, by
            // keeping it in memory and only switching if we know the select succeeded.
            LText = new LText_Class();
        }

        try
        {
            SetCursors(wait: true);

            Localize();
            if (!_state.IsStartup()) _ownerForm?.Localize();
        }
        finally
        {
            SetCursors(wait: false);
        }
    }

    private void VisualThemeRadioButtons_CheckedChanged(object sender, EventArgs e)
    {
        if (EventsDisabled > 0) return;
        if (sender is not DarkRadioButton button) return;
        if (!button.Checked) return;

        VisualTheme theme =
            button == AppearancePage.DarkThemeRadioButton ? VisualTheme.Dark :
            button == AppearancePage.ClassicThemeRadioButton ? VisualTheme.Classic :
            Core.GetSystemTheme();

        try
        {
            SetCursors(wait: true);

            SetTheme(theme, button == AppearancePage.FollowSystemThemeRadioButton, startup: false);
            _ownerForm?.SetTheme(theme);
        }
        finally
        {
            SetCursors(wait: false);
        }
    }

    private void GameOrganizationRadioButtons_CheckedChanged(object sender, EventArgs e)
    {
        if (sender is not DarkRadioButton { Checked: true }) return;
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

    #region Rating display

    private void RatingOutOfTenRadioButton_CheckedChanged(object sender, EventArgs e)
    {
        if (EventsDisabled > 0) return;
        if (AppearancePage.RatingNDLDisplayStyleRadioButton.Checked)
        {
            AppearancePage.RatingUseStarsCheckBox.Enabled = false;
            SetRatingImage();
        }
    }

    private void RatingOutOfFiveRadioButton_CheckedChanged(object sender, EventArgs e)
    {
        if (EventsDisabled > 0) return;
        if (AppearancePage.RatingFMSelDisplayStyleRadioButton.Checked)
        {
            AppearancePage.RatingUseStarsCheckBox.Enabled = true;
            SetRatingImage();
        }
    }

    private void RatingUseStarsCheckBox_CheckedChanged(object sender, EventArgs e)
    {
        if (EventsDisabled > 0) return;
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

    #region Date format

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
        AppearancePage.Date1ComboBox.SelectedItem.ToStringOrEmpty() +
        AppearancePage.DateSeparator1TextBox.Text.EscapeAllChars() +
        AppearancePage.Date2ComboBox.SelectedItem.ToStringOrEmpty() +
        AppearancePage.DateSeparator2TextBox.Text.EscapeAllChars() +
        AppearancePage.Date3ComboBox.SelectedItem.ToStringOrEmpty() +
        AppearancePage.DateSeparator3TextBox.Text.EscapeAllChars() +
        AppearancePage.Date4ComboBox.SelectedItem.ToStringOrEmpty();

    private bool FormatAndTestDate(out string formatString, out string exampleDateString)
    {
        formatString = GetFormattedCustomDateString();

        // It's impossible to get an ArgumentOutOfRangeException as long as our readonly example date is valid.
        // It's probably impossible to get a FormatException too (because we handle invalid formats in the
        // config reader and reset to default if they aren't valid) but not 100% certain.
        try
        {
            exampleDateString = _exampleDate.ToString(formatString, CultureInfo.CurrentCulture);
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

    private void UpdateCustomExampleDate()
    {
        if (FormatAndTestDate(out _, out string formattedExampleDate))
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
        if (sender is not DarkRadioButton { Checked: true }) return;
        AppearancePage.DateCustomFormatPanel.Enabled = false;
        AppearancePage.PreviewDateLabel.Text = sender == AppearancePage.DateCurrentCultureShortRadioButton
            ? _exampleDate.ToShortDateString()
            : _exampleDate.ToLongDateString();
    }

    private void DateCustomRadioButton_CheckedChanged(object sender, EventArgs e)
    {
        DarkRadioButton s = (DarkRadioButton)sender;
        AppearancePage.DateCustomFormatPanel.Enabled = s.Checked;
        if (s.Checked) UpdateCustomExampleDate();
    }

    private void DateCustomValue_Changed(object sender, EventArgs e)
    {
        if (AppearancePage.DateCustomFormatPanel.Enabled) UpdateCustomExampleDate();
    }

    #endregion

    #endregion

    #region Thief Buddy page

    private async void ThiefBuddyExistenceCheckTimer_Tick(object sender, EventArgs e)
    {
        try
        {
            await Task.Run(CheckThiefBuddyExistence);
            UpdateThiefBuddyExistenceOnUI();
        }
        catch
        {
            // Ignore - in case the controls are disposed? I think that can happen...
        }
    }

    private void CheckThiefBuddyExistence()
    {
        string thiefBuddyExe = Paths.GetThiefBuddyExePath();
        _thiefBuddyConsideredToExist = !thiefBuddyExe.IsWhiteSpace() && File.Exists(thiefBuddyExe);
    }

    private void UpdateThiefBuddyExistenceOnUI()
    {
        UpdateThiefBuddyStatusLabel();
        ThiefBuddyPage.RunTBPanel.Enabled = _thiefBuddyConsideredToExist;
    }

    private void UpdateThiefBuddyStatusLabel()
    {
        ThiefBuddyPage.TBInstallStatusLabel.Text = _thiefBuddyConsideredToExist
            ? LText.SettingsWindow.ThiefBuddy_StatusInstalled
            : LText.SettingsWindow.ThiefBuddy_StatusNotInstalled;
    }

    private static void ThiefBuddyPage_GetTBLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
        Core.OpenLink(NonLocalizableText.ThiefBuddyLink);
    }

    #endregion

    #region I/O Threading page

    private void IOThreadsRadioButtons_CheckedChanged(object sender, EventArgs e)
    {
        SetIOThreadsState();
    }

    private void SetIOThreadsState()
    {
        IOThreadingPage.CustomThreadsLabel.Enabled = IOThreadingPage.CustomModeRadioButton.Checked;
        IOThreadingPage.CustomThreadsNumericUpDown.Enabled = IOThreadingPage.CustomModeRadioButton.Checked;
        IOThreadingPage.IOThreadsResetButton.Enabled = IOThreadingPage.CustomModeRadioButton.Checked;
    }

    private void IOThreadsResetButton_Click(object sender, EventArgs e)
    {
        IOThreadingPage.CustomThreadsNumericUpDown.Value = CoreCount;
    }

    #endregion

    #region Other page

    private void WebSearchURLResetButtons_Click(object sender, EventArgs e)
    {
        int index = Array.FindIndex(GameWebSearchUrlResetButtons, x => x == sender);
        GameWebSearchUrlTextBoxes[index].Text = Defaults.WebSearchUrls[index];
    }

    #endregion

    #region Errors

    private void EnsureControlIsInView(ScrollableControl parent, Control control)
    {
        if (control == PathsPage.BackupPathTextBox)
        {
            PathsPage.PagePanel.ScrollControlIntoView(PathsPage.BackupGroupBox);
            PathsPage.PagePanel.ScrollControlIntoView(PathsPage.BackupPathHelpLabel);
        }
        else
        {
            parent.ScrollControlIntoView(control);
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

        foreach (DarkTextBox tb in ExePathTextBoxes)
        {
            if (!tb.Text.IsWhiteSpace() && !File.Exists(tb.Text))
            {
                error = true;
                ShowPathError(tb, true);
            }
        }

        if (BackupPathInvalid_Settings(PathsPage.BackupPathTextBox.Text, GameExeTextBoxes))
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
            GetPageButton(SettingsTab.Paths).Checked = true;

            // One user missed the error highlight on a textbox because it was scrolled offscreen, and was
            // confused as to why there was an error. So scroll the first error-highlighted textbox onscreen
            // to make it clear.
            foreach (Control control in ErrorableControls)
            {
                if (PathErrorIsSet(control))
                {
                    EnsureControlIsInView(PathsPage.PagePanel, control);
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
            foreach (Control c in ErrorableControls)
            {
                if (PathErrorIsSet(c)) return;
            }
        }

        ErrorLabel.Text = shown ? LText.SettingsWindow.Paths_ErrorSomePathsAreInvalid : "";
        ErrorIconPictureBox.Visible = shown;
        ErrorLabel.Visible = shown;
    }

    private static bool PathErrorIsSet(Control control) => control.Tag is PathError.True;

    #endregion

    private T GetDummy<T>() where T : IDisposable, new()
    {
        return (T)(_dummyDisposables.TryGetValue(typeof(T), out IDisposable item)
            ? item
            : _dummyDisposables.AddAndReturn(typeof(T), new T()));
    }

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _thiefBuddyExistenceCheckTimer.Dispose();

            components?.Dispose();
            // If we're on startup, only PathsPage will have been added, so others must be manually disposed.
            // Just dispose them all if they need it, to be thorough.
            for (int i = 0; i < SettingsTabCount; i++)
            {
                PageControls[i].Page.Dispose();
            }
            foreach (KeyValuePair<Type, IDisposable> item in _dummyDisposables)
            {
                item.Value.Dispose();
            }
        }
        base.Dispose(disposing);
    }
}
