﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using AngelLoader.Common;
using static AngelLoader.Common.Common;
using static AngelLoader.Common.Utility.Methods;
using AngelLoader.Common.DataClasses;
using AngelLoader.Common.Utility;
using AngelLoader.Properties;
using AngelLoader.WinAPI;
using FMScanner;
using Gma.System.MouseKeyHook;

namespace AngelLoader.Forms
{
    public partial class MainForm : Form, IEventDisabler, IMessageFilter
    {
        #region Private fields

        private BusinessLogic Model;

        private FormWindowState NominalWindowState;
        private Size NominalWindowSize;

        private List<FanMission> FMsList;

        // Set these beforehand and don't set autosize on any column! Or else it explodes everything because
        // FMsDGV tries to refresh when it shouldn't and all kinds of crap. Phew.
        private const int RatingImageColumnWidth = 73;
        private const int FinishedColumnWidth = 91;

        #region Bitmaps

        // We need to grab these images every time a cell is shown on the DataGridView, and pulling them from
        // Resources every time is enormously expensive, causing laggy scrolling and just generally wasting good
        // cycles. So we copy them only once to these local bitmaps, and voila, instant scrolling performance.
        private Bitmap Thief1Icon;
        private Bitmap Thief2Icon;
        private Bitmap Thief3Icon;
        private Bitmap BlankIcon;
        private Bitmap CheckIcon;
        private Bitmap RedQuestionMarkIcon;

        private Bitmap[] StarIcons;
        private Dictionary<FinishedOn, Bitmap> FinishedOnIcons;

        #endregion

        private DataGridViewImageColumn RatingImageColumn;

        private bool InitialSelectedFMHasBeenSet;

        public bool EventsDisabled { get; set; }

        #endregion

        #region Filter bar scroll RepeatButtons

        // TODO: Make this use a timer or something?
        // The thread is fine but the speed accumulates if you click a bunch. Not a big deal I guess but hey.
        // Single-threading it would also allow it to be packed away in a custom control.
        private bool _repeatButtonRunning;

        private void FilterBarScrollButtons_Click(object sender, EventArgs e)
        {
            if (_repeatButtonRunning) return;
            int direction = sender == FilterBarScrollLeftButton ? InteropMisc.SB_LINELEFT : InteropMisc.SB_LINERIGHT;
            InteropMisc.SendMessage(FiltersFlowLayoutPanel.Handle, InteropMisc.WM_SCROLL, (IntPtr)direction, IntPtr.Zero);
        }

        private void FilterBarScrollButtons_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            RunRepeatButton(sender == FilterBarScrollLeftButton ? InteropMisc.SB_LINELEFT : InteropMisc.SB_LINERIGHT);
        }

        private void RunRepeatButton(int direction)
        {
            if (_repeatButtonRunning) return;
            _repeatButtonRunning = true;
            Task.Run(() =>
            {
                while (_repeatButtonRunning)
                {
                    Invoke(new Action(() =>
                    {
                        InteropMisc.SendMessage(FiltersFlowLayoutPanel.Handle, InteropMisc.WM_SCROLL, (IntPtr)direction, IntPtr.Zero);
                    }));
                    Thread.Sleep(150);
                }
            });
        }

        private void FilterBarScrollButtons_EnabledChanged(object sender, EventArgs e) => _repeatButtonRunning = false;

        private void FilterBarScrollLeftButton_MouseUp(object sender, MouseEventArgs e) => _repeatButtonRunning = false;

        private void FilterBarScrollButtons_VisibleChanged(object sender, EventArgs e)
        {
            var senderButton = (Button)sender;
            var otherButton = senderButton == FilterBarScrollLeftButton
                ? FilterBarScrollRightButton
                : FilterBarScrollLeftButton;
            if (!senderButton.Visible && otherButton.Visible) _repeatButtonRunning = false;
        }

        #endregion

        // Keeping this for the mousewheel functionality because it passes on the message directly and so allows
        // any pressed keys to also be passed along to the control (allows Ctrl+Mousewheel for rtfbox zoom f.ex.)
        public bool PreFilterMessage(ref Message m)
        {
            // So I don't forget what the return values do
            const bool BlockMessage = true;
            const bool PassMessageOn = false;

            if (m.Msg == InteropMisc.WM_MOUSEWHEEL)
            {
                if (CursorOutsideAddTagsDropDownArea()) return BlockMessage;

                // This allows controls to be scrolled with the mousewheel when the mouse is over them, without
                // needing to actually be focused. Vital for a good user experience.
                var pos = new Point(m.LParam.ToInt32() & 0xffff, m.LParam.ToInt32() >> 16);
                var hWnd = InteropMisc.WindowFromPoint(pos);
                if (hWnd != IntPtr.Zero && Control.FromHandle(hWnd) != null)
                {
                    if (CursorOverControl(FiltersFlowLayoutPanel) && !CursorOverControl(FMsDGV))
                    {
                        // Allow the filter bar to be mousewheel-scrolled with the buttons properly appearing
                        // and disappearing as appropriate
                        int wParam = (int)m.WParam;
                        if (wParam != 0)
                        {
                            int direction = wParam > 0 ? InteropMisc.SB_LINELEFT : InteropMisc.SB_LINERIGHT;
                            int origSmallChange = FiltersFlowLayoutPanel.HorizontalScroll.SmallChange;

                            FiltersFlowLayoutPanel.HorizontalScroll.SmallChange = 45;

                            InteropMisc.SendMessage(FiltersFlowLayoutPanel.Handle, InteropMisc.WM_SCROLL, (IntPtr)direction, IntPtr.Zero);

                            FiltersFlowLayoutPanel.HorizontalScroll.SmallChange = origSmallChange;
                        }
                    }
                    else
                    {
                        InteropMisc.SendMessage(hWnd, m.Msg, m.WParam, m.LParam);
                    }
                    return BlockMessage;
                }
            }

            return PassMessageOn;
        }

        private IMouseEvents AppMouseHook;

        #region Form (init, events, close)

        // InitializeComponent() only - for everything else use Init() below
        public MainForm() => InitializeComponent();

        #region Mouse hook

        // Standard Windows drop-down behavior: nothing else responds until the drop-down closes
        private bool CursorOutsideAddTagsDropDownArea()
        {
            return AddTagListBox.Visible &&
                   // Check Size instead of ClientSize in order to support clicking the scroll bar
                   !CursorOverControl(AddTagListBox, fullArea: true) &&
                   !CursorOverControl(AddTagTextBox) &&
                   !CursorOverControl(AddTagButton);
        }

        private void HookMouseDown(object sender, MouseEventExtArgs e)
        {
            // !CanFocus will be false if there are modal windows open
            if (!CanFocus) return;

            if (CursorOutsideAddTagsDropDownArea())
            {
                HideAddTagDropDown();
                e.Handled = true;
            }
        }

        private void HookMouseUp(object sender, MouseEventExtArgs e)
        {
        }

        private void HookMouseMove(object sender, MouseEventExtArgs e)
        {
            if (!CanFocus) return;

            if (CursorOverReadmeArea())
            {
                ShowReadmeControls();
            }
            else
            {
                HideReadmeControls();
            }
        }

        #endregion

        // Put anything that does anything in here, not in the constructor. Otherwise it's a world of pain and
        // screwy behavior cascading outwards and messing with everything it touches. Don't do it.
        internal async Task Init()
        {
            AppMouseHook = Hook.AppEvents();
            AppMouseHook.MouseDownExt += HookMouseDown;
            AppMouseHook.MouseUpExt += HookMouseUp;
            AppMouseHook.MouseMoveExt += HookMouseMove;
            Application.AddMessageFilter(this);

            // Aside from a possible OpenSettings() call in Model.Init() if it needs to throw up the Settings
            // window (which doesn't show the view, so the startup process is still left intact), this code is
            // now a nice straight line with no back-and-forth spaghetti method calls.

            Model = new BusinessLogic(this, ProgressBox);

            await Model.Init();

            // Model.Init() success means Config is now populated

            Model.FindFMs();
            FMsList = Model.FMsList;

            #region Set up form and control state

            KeyPreview = true;

            // Fine-tuning defaults without having to mess with the UI, because with all the anchoring, changing
            // the position of anything will mess it all up.
            TopSplitContainer.SplitterDistance = 1238;

            float mainPercent = (float)(MainSplitContainer.SplitterDistance * 100) /
                                MainSplitContainer.Height;
            float topPercent = (float)(TopSplitContainer.SplitterDistance * 100) /
                               TopSplitContainer.Width;

            MainSplitContainer.SplitterDistancePercent = mainPercent;
            TopSplitContainer.SplitterDistancePercent = topPercent;

            // The other Rating column, there has to be two, one for text and one for images
            RatingImageColumn = new DataGridViewImageColumn
            {
                HeaderText = LText.FMsListColumns.Rating,
                ImageLayout = DataGridViewImageCellLayout.Zoom,
                MinimumWidth = 25,
                Name = "RatingImageColumn",
                ReadOnly = true,
                Width = RatingImageColumnWidth,
                Resizable = DataGridViewTriState.False,
                SortMode = DataGridViewColumnSortMode.Programmatic
            };

            FinishedColumn.Width = FinishedColumnWidth;

            UpdateRatingLists(Config.RatingDisplayStyle == RatingDisplayStyle.FMSel);
            UpdateRatingColumn(startup: true);

            FMsDGV.FillColumns(Config.Columns);

            // Set both at once to avoid an elusive bug that happens when you start up, the readme is blank, then
            // you shut down without loading a readme, whereupon it will save out ZoomFactor which is still 1.0.
            // You can't just save out StoredZoomFactor either because it doesn't change when the user zooms, as
            // there's no event for that. Fun.
            ReadmeRichTextBox.StoredZoomFactor = Config.ReadmeZoomFactor;
            ReadmeRichTextBox.ZoomFactor = ReadmeRichTextBox.StoredZoomFactor;

            MainSplitContainer.InjectSibling(TopSplitContainer);
            TopSplitContainer.InjectSibling(MainSplitContainer);

            ProgressBox.Inject(this);

            FiltersFlowLayoutPanel.HorizontalScroll.SmallChange = 20;

            // Set these to invisible here, because if you do it on the UI, it constantly marks the form file as
            // unsaved every time you open it even though no changes were made. Aggravating as hell. ToolStrips
            // march to the beat of their own drum, that's for sure. :\
            FilterByReleaseDateLabel.Visible = false;
            FilterByLastPlayedLabel.Visible = false;
            FilterByRatingLabel.Visible = false;

            ViewHTMLReadmeButton.CenterHV(MainSplitContainer.Panel2);

            #region Autosize menus

            // This is another hack to fix behavior caused by the UI designer. When you select a menu, it appears
            // and adds an extra "Type Here" item to the bottom. This item counts as part of the height, and so
            // the height ends up including an item that only actually appears in the designer, causing the menu
            // to be shown in the wrong location when you call Show() with the height as a parameter. Setting a
            // menu's size to empty causes it to autosize back to its actual proper size. I swear, this stuff.

            FMRightClickMenu.Size = Size.Empty;
            AltTitlesMenu.Size = Size.Empty;
            PlayOriginalGameMenu.Size = Size.Empty;
            FinishedOnMenu.Size = Size.Empty;
            AddTagMenu.Size = Size.Empty;
            ImportFromMenu.Size = Size.Empty;

            #endregion

            #endregion

            // Set values before the form is shown so that we don't see them being set
            SetInitialValues();

            Show();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            // This needs to be set here and nowhere else. Before _Load and it doesn't take; after _Load and the
            // changing will be visible.
            SetWindowStateAndSize();

            // These also need to be set here for similar reasons.
            MainSplitContainer.SetSplitterDistance(Config.MainHorizontalSplitterDistance);
            TopSplitContainer.SetSplitterDistance(Config.TopVerticalSplitterDistance);

            // Set this here because it might need to know the size of things after splitter adjustment
            SetUITextToLocalized();
        }

        private void SetWindowStateAndSize()
        {
            // TODO: Save and restore the position, and robustly handle the window being offscreen, too large etc.
            WindowState = Config.MainWindowState;
            NominalWindowState = Config.MainWindowState;

            Size = Config.MainWindowSize;
            NominalWindowSize = Config.MainWindowSize;
        }

        internal async void SetInitialValues()
        {
            #region Filters

            Config.Filter.DeepCopyTo(FMsDGV.Filter);
            SetUIFilterValues(FMsDGV.Filter);

            #endregion

            #region Top right tabs

            TopRightTabControl.SelectedTab =
                Config.TopRightTab == TopRightTab.EditFM ? EditFMTabPage :
                Config.TopRightTab == TopRightTab.Comment ? CommentTabPage :
                Config.TopRightTab == TopRightTab.Tags ? TagsTabPage :
                StatisticsTabPage;

            #endregion

            ChangeGameOrganization();

            await ScanNewFMsForGameType();

            SortFMTable(Config.SortedColumn, Config.SortDirection);
            // Even if the filter is empty, do this anyway to cause a refresh.
            // It'll early-out on an empty filter anyway.
            SetFilter();
        }

        private void SetUIFilterValues(Filter filter)
        {
            using (new DisableEvents(this))
            {
                FilterTitleTextBox.Text = filter.Title;
                FilterAuthorTextBox.Text = filter.Author;
                FilterShowJunkCheckBox.Checked = filter.ShowJunk;

                FilterByTagsButton.Checked = !filter.Tags.Empty();

                FilterByFinishedButton.Checked = filter.Finished.Contains(FinishedState.Finished);
                FilterByUnfinishedButton.Checked = filter.Finished.Contains(FinishedState.Unfinished);

                FilterByRatingButton.Checked = !(filter.RatingFrom == -1 && filter.RatingTo == 10);
                UpdateRatingLabel();

                FilterByReleaseDateButton.Checked = filter.ReleaseDateFrom != null || filter.ReleaseDateTo != null;
                UpdateDateLabel(lastPlayed: false);

                FilterByLastPlayedButton.Checked = filter.LastPlayedFrom != null || filter.LastPlayedTo != null;
                UpdateDateLabel(lastPlayed: true);
            }
        }

        private void SetUITextToLocalized()
        {
            // Certain controls' text depends on FM state. Because this could be run after startup, we need to
            // make sure those controls' text is set correctly.
            var selFM = FMsDGV.SelectedRows.Count > 0 ? GetSelectedFM() : null;

            this.SuspendDrawing();
            try
            {
                // TODO: Replace all magic numbers in here with saved default widths
                // This could be run not on startup, so we can't just save the actual button widths here

                #region Game tabs

                Thief1TabPage.Text = LText.GameTabs.Thief1;
                Thief2TabPage.Text = LText.GameTabs.Thief2;
                Thief3TabPage.Text = LText.GameTabs.Thief3;

                #endregion

                #region Filter bar

                if (Config.GameOrganization == GameOrganization.ByTab) PositionFilterBarAfterTabs();

                FilterByThief1Button.ToolTipText = LText.FilterBar.Thief1ToolTip;
                FilterByThief2Button.ToolTipText = LText.FilterBar.Thief2ToolTip;
                FilterByThief3Button.ToolTipText = LText.FilterBar.Thief3ToolTip;

                FilterTitleLabel.Text = LText.FilterBar.Title;
                FilterAuthorLabel.Text = LText.FilterBar.Author;

                FilterByReleaseDateButton.ToolTipText = LText.FilterBar.ReleaseDateToolTip;
                FilterByLastPlayedButton.ToolTipText = LText.FilterBar.LastPlayedToolTip;
                FilterByTagsButton.ToolTipText = LText.FilterBar.TagsToolTip;
                FilterByFinishedButton.ToolTipText = LText.FilterBar.FinishedToolTip;
                FilterByUnfinishedButton.ToolTipText = LText.FilterBar.UnfinishedToolTip;
                FilterByRatingButton.ToolTipText = LText.FilterBar.RatingToolTip;

                FilterShowJunkCheckBox.Text = LText.FilterBar.ShowJunk;

                #endregion

                #region Clear/refresh/reset area

                RefreshFiltersButton.ToolTipText = LText.RefreshFilteredListButtonToolTip;
                ClearFiltersButton.ToolTipText = LText.ClearFiltersButtonToolTip;
                MainToolTip.SetToolTip(ResetLayoutButton, LText.ResetLayoutButtonToolTip);

                #endregion

                #region Columns

                GameTypeColumn.HeaderText = LText.FMsListColumns.Game;
                InstalledColumn.HeaderText = LText.FMsListColumns.Installed;
                TitleColumn.HeaderText = LText.FMsListColumns.Title;
                ArchiveColumn.HeaderText = LText.FMsListColumns.Archive;
                AuthorColumn.HeaderText = LText.FMsListColumns.Author;
                SizeColumn.HeaderText = LText.FMsListColumns.Size;
                RatingTextColumn.HeaderText = LText.FMsListColumns.Rating;
                RatingImageColumn.HeaderText = LText.FMsListColumns.Rating;
                FinishedColumn.HeaderText = LText.FMsListColumns.Finished;
                ReleaseDateColumn.HeaderText = LText.FMsListColumns.ReleaseDate;
                LastPlayedColumn.HeaderText = LText.FMsListColumns.LastPlayed;
                DisabledModsColumn.HeaderText = LText.FMsListColumns.DisabledMods;
                CommentColumn.HeaderText = LText.FMsListColumns.Comment;

                #endregion

                #region Play / install / uninstall

                PlayFMMenuItem.Text = LText.FMContextMenu.PlayFM;
                PlayFMButton.SetL10nText(LText.BottomArea.PlayFM);

                var sayInstall = selFM == null || !selFM.Installed;

                InstallUninstallMenuItem.Text = sayInstall
                    ? LText.FMContextMenu.InstallFM
                    : LText.FMContextMenu.UninstallFM;

                InstallUninstallFMButton.SetL10nText(sayInstall
                    ? LText.BottomArea.InstallFM
                    : LText.BottomArea.UninstallFM);

                #endregion

                #region FM context menu

                RatingRCSubMenu.Text = LText.FMContextMenu.Rating;
                FinishedOnRCSubMenu.Text = LText.FMContextMenu.FinishedOn;
                TasksRCSubMenu.Text = LText.FMContextMenu.Tasks;

                RatingRCMenuUnrated.Text = LText.Global.Unrated;

                ConvertWAVsTo16BitMenuItem.Text = LText.TasksMenu.ConvertWAVsTo16Bit;
                ConvertOGGsToWAVsToolStripMenuItem.Text = LText.TasksMenu.ConvertOGGsToWAVs;

                #endregion

                #region Play original games menu

                PlayOriginalThief1MenuItem.Text = LText.PlayOriginalGameMenu.Thief1;
                PlayOriginalThief2MenuItem.Text = LText.PlayOriginalGameMenu.Thief2;
                PlayOriginalThief3MenuItem.Text = LText.PlayOriginalGameMenu.Thief3;

                #endregion

                #region Statistics tab

                StatisticsTabPage.Text = LText.StatisticsTab.TabText;

                CustomResourcesLabel.Text =
                    selFM == null ? LText.StatisticsTab.NoFMSelected :
                    selFM.Game == Game.Thief3 ? LText.StatisticsTab.CustomResourcesNotSupportedForThief3 :
                    FMCustomResourcesScanned(selFM) ? LText.StatisticsTab.CustomResources :
                    LText.StatisticsTab.CustomResourcesNotScanned;

                CR_MapCheckBox.Text = LText.StatisticsTab.Map;
                CR_AutomapCheckBox.Text = LText.StatisticsTab.Automap;
                CR_TexturesCheckBox.Text = LText.StatisticsTab.Textures;
                CR_SoundsCheckBox.Text = LText.StatisticsTab.Sounds;
                CR_MoviesCheckBox.Text = LText.StatisticsTab.Movies;
                CR_ObjectsCheckBox.Text = LText.StatisticsTab.Objects;
                CR_CreaturesCheckBox.Text = LText.StatisticsTab.Creatures;
                CR_MotionsCheckBox.Text = LText.StatisticsTab.Motions;
                CR_ScriptsCheckBox.Text = LText.StatisticsTab.Scripts;
                CR_SubtitlesCheckBox.Text = LText.StatisticsTab.Subtitles;

                #endregion

                #region Edit FM tab

                EditFMTabPage.Text = LText.EditFMTab.TabText;
                EditFMTitleLabel.Text = LText.EditFMTab.Title;
                EditFMAuthorLabel.Text = LText.EditFMTab.Author;
                EditFMReleaseDateCheckBox.Text = LText.EditFMTab.ReleaseDate;
                EditFMLastPlayedCheckBox.Text = LText.EditFMTab.LastPlayed;
                EditFMRatingLabel.Text = LText.EditFMTab.Rating;
                EditFMRatingComboBox.Items[0] = LText.Global.Unrated;
                EditFMFinishedOnButton.SetL10nText(LText.EditFMTab.FinishedOn, 138);
                EditFMDisabledModsLabel.Text = LText.EditFMTab.DisabledMods;
                EditFMDisableAllModsCheckBox.Text = LText.EditFMTab.DisableAllMods;

                #endregion

                #region Comment tab

                CommentTabPage.Text = LText.CommentTab.TabText;

                #endregion

                #region Tags tab

                TagsTabPage.Text = LText.TagsTab.TabText;
                // Set these directly, because they shouldn't be resizing themselves or the layout will break
                AddTagButton.Text = LText.TagsTab.AddTag;
                AddTagFromListButton.Text = LText.TagsTab.AddFromList;
                RemoveTagButton.Text = LText.TagsTab.RemoveTag;

                #endregion

                #region Readme area

                ViewHTMLReadmeButton.SetL10nText(LText.ReadmeArea.ViewHTMLReadme);
                ViewHTMLReadmeButton.CenterHV(MainSplitContainer.Panel2);

                #endregion

                #region Bottom area

                PlayOriginalGameButton.SetL10nText(LText.BottomArea.PlayOriginalGame);
                WebSearchButton.SetL10nText(LText.BottomArea.WebSearch);
                ScanAllFMsButton.SetL10nText(LText.BottomArea.ScanAllFMs);
                ImportButton.SetL10nText(LText.BottomArea.Import);
                SettingsButton.SetL10nText(LText.BottomArea.Settings);

                #endregion

                ProgressBox.SetUITextToLocalized();
            }
            finally
            {
                this.ResumeDrawing();
            }
        }

        private void MainForm_Deactivate(object sender, EventArgs e)
        {
            FMsDGV.CancelColumnResize();
            MainSplitContainer.CancelResize();
            TopSplitContainer.CancelResize();
        }

        private void MainForm_SizeChanged(object sender, EventArgs e)
        {
            if (WindowState != FormWindowState.Minimized)
            {
                NominalWindowState = WindowState;
                NominalWindowSize = Size;
            }

            if (ProgressBox.Visible) ProgressBox.Center();
            if (AddTagListBox.Visible) HideAddTagDropDown();
        }

        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                FMsDGV.CancelColumnResize();
                MainSplitContainer.CancelResize();
                TopSplitContainer.CancelResize();

                HideAddTagDropDown();
            }
            else if (e.Control && e.KeyCode == Keys.F)
            {
                FilterTitleTextBox.Focus();
                FilterTitleTextBox.SelectAll();
            }
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Application.RemoveMessageFilter(this);
            // Mouse hook will dispose along with the form

            // Argh, stupid hack to get this to not run TWICE on Application.Exit()
            // Application.Exit() is the worst thing ever. Before closing it just does whatever the hell it wants.
            FormClosing -= MainForm_FormClosing;

            UpdateConfig();
            Model.Shutdown();
        }

        #endregion

        private void PositionFilterBarAfterTabs()
        {
            int FilterBarAfterTabsX()
            {
                // In case I decide to allow a variable number of tabs based on which games are defined
                if (GamesTabControl.TabCount == 0) return 0;

                var lastRect = GamesTabControl.GetTabRect(GamesTabControl.TabCount - 1);
                return lastRect.X + lastRect.Width + 5;
            }

            FiltersFlowLayoutPanel.Location =
                new Point(FilterBarAfterTabsX(), FiltersFlowLayoutPanel.Location.Y);
            FiltersFlowLayoutPanel.Width =
                (RefreshClearToolStripCustom.Location.X - 4) - FiltersFlowLayoutPanel.Location.X;
        }

        private void ChangeGameOrganization()
        {
            if (Config.GameOrganization == GameOrganization.OneList)
            {
                FilterGamesLeftSepToolStripCustom.Hide();
                GamesTabControl.Hide();
                var plusWidth = FiltersFlowLayoutPanel.Location.X;
                FiltersFlowLayoutPanel.Location = new Point(0, FiltersFlowLayoutPanel.Location.Y);
                FiltersFlowLayoutPanel.Width += plusWidth;
                FilterGameButtonsToolStrip.Show();

                Config.SelFM.DeepCopyTo(FMsDGV.CurrentSelFM);
            }
            else // ByTab
            {
                // In case they don't match
                Config.Filter.Games.Clear();
                Config.Filter.Games.Add(Config.GameTab);

                PositionFilterBarAfterTabs();

                FilterGameButtonsToolStrip.Hide();
                FilterGamesLeftSepToolStripCustom.Show();
                GamesTabControl.Show();

                Config.GameTabsState.DeepCopyTo(FMsDGV.GameTabsState);

                switch (Config.GameTab)
                {
                    case Game.Thief1:
                        FMsDGV.GameTabsState.T1SelFM.DeepCopyTo(FMsDGV.CurrentSelFM);
                        FMsDGV.GameTabsState.T1Filter.DeepCopyTo(FMsDGV.Filter);
                        break;
                    case Game.Thief2:
                        FMsDGV.GameTabsState.T2SelFM.DeepCopyTo(FMsDGV.CurrentSelFM);
                        FMsDGV.GameTabsState.T2Filter.DeepCopyTo(FMsDGV.Filter);
                        break;
                    case Game.Thief3:
                        FMsDGV.GameTabsState.T3SelFM.DeepCopyTo(FMsDGV.CurrentSelFM);
                        FMsDGV.GameTabsState.T3Filter.DeepCopyTo(FMsDGV.Filter);
                        break;
                }

                using (new DisableEvents(this))
                {
                    GamesTabControl.SelectedTab =
                        Config.GameTab == Game.Thief2 ? Thief2TabPage :
                        Config.GameTab == Game.Thief3 ? Thief3TabPage :
                        Thief1TabPage;
                }
            }

            // Do these even if we're not in startup, because we may have changed the game organization mode
            FilterByThief1Button.Checked = Config.Filter.Games.Contains(Game.Thief1);
            FilterByThief2Button.Checked = Config.Filter.Games.Contains(Game.Thief2);
            FilterByThief3Button.Checked = Config.Filter.Games.Contains(Game.Thief3);
        }

        private async Task ScanNewFMsForGameType()
        {
            // TODO: This can be canceled, so make sure the world won't explode if the user cancels
            // and leaves some FMs in an un-game-type-scanned state.
            var fmsToScan = new List<FanMission>();
            foreach (var fm in FMsList)
            {
                if (fm.Game == null) fmsToScan.Add(fm);
            }
            if (fmsToScan.Count > 0)
            {
                var scanOptions = new ScanOptions
                {
                    ScanTitle = false,
                    ScanCampaignMissionNames = false,
                    ScanAuthor = false,
                    ScanVersion = false,
                    ScanLanguages = false,
                    ScanGameType = true,
                    ScanNewDarkRequired = false,
                    ScanNewDarkMinimumVersion = false,
                    ScanCustomResources = false,
                    ScanSize = false
                };

                await Model.ScanFMs(fmsToScan, scanOptions);
            }
        }

        private void UpdateConfig()
        {
            Game gameTab;
            if (Config.GameOrganization == GameOrganization.ByTab)
            {
                SaveCurrentTabSelectedFM(GamesTabControl.SelectedTab);
                var selGameTab = GamesTabControl.SelectedTab;
                gameTab =
                    selGameTab == Thief2TabPage ? Game.Thief2 :
                    selGameTab == Thief3TabPage ? Game.Thief3 :
                    Game.Thief1;
            }
            else
            {
                gameTab = Game.Thief1;
            }

            var selTopRightTab = TopRightTabControl.SelectedTab;
            var topRightTab =
                selTopRightTab == EditFMTabPage ? TopRightTab.EditFM :
                selTopRightTab == CommentTabPage ? TopRightTab.Comment :
                selTopRightTab == TagsTabPage ? TopRightTab.Tags :
                TopRightTab.Statistics;

            var s = FMsDGV;
            var selectedFM = GetSelectedFMPosInfo();

            Model.UpdateConfig(
                NominalWindowState,
                NominalWindowSize,
                MainSplitContainer.SplitterDistanceReal,
                TopSplitContainer.SplitterDistance,
                s.ColumnsToColumnData(), s.CurrentSortedColumn, s.CurrentSortDirection,
                s.Filter,
                selectedFM,
                s.GameTabsState,
                gameTab,
                topRightTab,
                ReadmeRichTextBox.ZoomFactor);
        }

        private bool CursorOverReadmeArea()
        {
            return ReadmeRichTextBox.Visible ? CursorOverControl(ReadmeRichTextBox) :
                ViewHTMLReadmeButton.Visible && CursorOverControl(MainSplitContainer.Panel2);
        }

        private bool CursorOverControl(Control control, bool fullArea = false)
        {
            if (!control.Visible || !control.Enabled) return false;

            // Don't create eleventy billion Rectangle objects per second
            var rpt = PointToClient(control.PointToScreen(new Point(0, 0)));
            var rcs = fullArea ? control.Size : control.ClientSize;
            var ptc = PointToClient(Cursor.Position);
            return ptc.X >= rpt.X && ptc.X < rpt.X + rcs.Width &&
                   ptc.Y >= rpt.Y && ptc.Y < rpt.Y + rcs.Height;
        }

        #region Get FM selected/index etc.

        /// <summary>
        /// Gets the FM at <paramref name="index"/>, taking the currently set filters into account.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        private FanMission GetFMFromIndex(int index)
        {
            return FMsDGV.Filtered ? FMsList[FMsDGV.FilterShownIndexList[index]] : FMsList[index];
        }

        /// <summary>
        /// Gets the currently selected FM, taking the currently set filters into account.
        /// </summary>
        /// <returns></returns>
        private FanMission GetSelectedFM()
        {
            Debug.Assert(FMsDGV.SelectedRows.Count > 0, "GetSelectedFM: no rows selected!");

            return GetFMFromIndex(FMsDGV.SelectedRows[0].Index);
        }

        private int GetIndexFromInstalledName(string installedName)
        {
            // Graceful default if a value is missing
            if (installedName.IsEmpty()) return 0;

            for (int i = 0; i < (FMsDGV.Filtered ? FMsDGV.FilterShownIndexList.Count : FMsList.Count); i++)
            {
                var fm = GetFMFromIndex(i);
                if (fm.InstalledDir.EqualsI(installedName)) return i;
            }

            return 0;
        }

        private SelectedFM GetSelectedFMPosInfo()
        {
            var ret = new SelectedFM { InstalledName = null, IndexFromTop = 0 };

            if (FMsDGV.SelectedRows.Count == 0) return ret;

            int sel = FMsDGV.SelectedRows[0].Index;
            int firstDisplayed = FMsDGV.FirstDisplayedScrollingRowIndex;
            int lastDisplayed = firstDisplayed + FMsDGV.DisplayedRowCount(false);

            int indexFromTop = sel >= firstDisplayed && sel <= lastDisplayed
                ? sel - firstDisplayed
                : FMsDGV.DisplayedRowCount(true) / 2;

            ret.InstalledName = GetFMFromIndex(sel).InstalledDir;
            ret.IndexFromTop = indexFromTop;
            return ret;
        }

        #endregion

        #region Test / debug

        private void TestButton_Click(object sender, EventArgs e)
        {
        }

        private void Test2Button_Click(object sender, EventArgs e)
        {
        }

        #endregion

        #region FMsDGV-related

        // TODO: Run perf testing when every FM has a bunch of tags
        // PERF: 0.7~2.2ms with every filter set (including a bunch of tag filters), over 1098 set. But note that
        //       majority have no tags, so test again when all of them do.
        //       This was tested with the Release_Testing (optimized) profile.
        //       All in all, I'd say performance is looking really good. Certainly better than I was expecting,
        //       given this is a reasonably naive implementation with no real attempt to be clever.
        private void SetFilter(bool suppressRefresh = false, bool forceRefreshReadme = false, bool forceSuppressSelectionChangedEvent = false)
        {
            DebugLabel2.Text = int.TryParse(DebugLabel2.Text, out var result) ? (result + 1).ToString() : "1";

            var t = new Stopwatch();
            t.Start();

            var s = FMsDGV;

            #region Set filters that are stored in control state

            s.Filter.Title = FilterTitleTextBox.Text;
            s.Filter.Author = FilterAuthorTextBox.Text;

            s.Filter.Games.Clear();
            if (FilterByThief1Button.Checked) s.Filter.Games.Add(Game.Thief1);
            if (FilterByThief2Button.Checked) s.Filter.Games.Add(Game.Thief2);
            if (FilterByThief3Button.Checked) s.Filter.Games.Add(Game.Thief3);

            s.Filter.Finished.Clear();
            if (FilterByFinishedButton.Checked) s.Filter.Finished.Add(FinishedState.Finished);
            if (FilterByUnfinishedButton.Checked) s.Filter.Finished.Add(FinishedState.Unfinished);

            s.Filter.ShowJunk = FilterShowJunkCheckBox.Checked;

            #endregion

            // Skip the GetSelectedFM() check during a state where it may fail.
            // We also don't need it if we're forcing both things.
            var oldSelectedFM =
                forceRefreshReadme && forceSuppressSelectionChangedEvent ? null :
                s.SelectedRows.Count > 0 ? GetSelectedFM() : null;

            s.FilterShownIndexList.Clear();

            // This one gets checked in a loop, so cache it. Others are only checked twice at most, so leave them
            // be.
            var titleIsWhitespace = s.Filter.Title.IsWhiteSpace();

            #region Early out

            if (titleIsWhitespace &&
                s.Filter.Author.IsWhiteSpace() &&
                s.Filter.Games.Count == 0 &&
                s.Filter.Tags.Empty() &&
                s.Filter.ReleaseDateFrom == null &&
                s.Filter.ReleaseDateTo == null &&
                s.Filter.LastPlayedFrom == null &&
                s.Filter.LastPlayedTo == null &&
                s.Filter.RatingFrom == -1 &&
                s.Filter.RatingTo == 10 &&
                (s.Filter.Finished.Count == 0 ||
                 (s.Filter.Finished.Contains(FinishedState.Finished)
                  && s.Filter.Finished.Contains(FinishedState.Unfinished))) &&
                s.Filter.ShowJunk)
            {
                s.Filtered = false;

                if (!suppressRefresh)
                {
                    RefreshFMsList(
                        refreshReadme: forceRefreshReadme || (oldSelectedFM != null && !oldSelectedFM.Equals(GetFMFromIndex(0))),
                        suppressSelectionChangedEvent: forceSuppressSelectionChangedEvent || oldSelectedFM != null);
                }
                return;
            }

            #endregion

            #region Title / initial

            for (int i = 0; i < FMsList.Count; i++)
            {
                var fm = FMsList[i];

                if (titleIsWhitespace ||
                    fm.Archive.ContainsI(s.Filter.Title) ||
                    fm.Title.ContainsI(s.Filter.Title) ||
                    fm.InstalledDir.ContainsI(s.Filter.Title))
                {
                    s.FilterShownIndexList.Add(i);
                }
            }

            #endregion

            #region Author

            if (!s.Filter.Author.IsWhiteSpace())
            {
                for (int i = 0; i < s.FilterShownIndexList.Count; i++)
                {
                    var fmAuthor = FMsList[s.FilterShownIndexList[i]].Author;

                    if (!fmAuthor.ContainsI(s.Filter.Author))
                    {
                        s.FilterShownIndexList.RemoveAt(i);
                        i--;
                    }
                }
            }

            #endregion

            #region Show junk

            if (!s.Filter.ShowJunk)
            {
                for (int i = 0; i < s.FilterShownIndexList.Count; i++)
                {
                    var fm = FMsList[s.FilterShownIndexList[i]];
                    if (fm.Game == Game.Unsupported && !FilterShowJunkCheckBox.Checked)
                    {
                        s.FilterShownIndexList.RemoveAt(i);
                        i--;
                    }
                }
            }

            #endregion

            #region Games

            if (s.Filter.Games.Count > 0)
            {
                for (int i = 0; i < s.FilterShownIndexList.Count; i++)
                {
                    var fmGame = FMsList[s.FilterShownIndexList[i]].Game;
                    if (fmGame != Game.Unsupported && (fmGame == null || !s.Filter.Games.Contains((Game)fmGame)))
                    {
                        s.FilterShownIndexList.RemoveAt(i);
                        i--;
                    }
                }
            }

            #endregion

            #region Tags

            if (s.Filter.Tags.AndTags.Count > 0 ||
                s.Filter.Tags.OrTags.Count > 0 ||
                s.Filter.Tags.NotTags.Count > 0)
            {
                var andTags = s.Filter.Tags.AndTags;
                var orTags = s.Filter.Tags.OrTags;
                var notTags = s.Filter.Tags.NotTags;

                for (int i = 0; i < s.FilterShownIndexList.Count; i++)
                {
                    var fmTags = FMsList[s.FilterShownIndexList[i]].Tags;
                    if (fmTags.Count == 0 && notTags.Count == 0)
                    {
                        s.FilterShownIndexList.RemoveAt(i);
                        i--;
                        continue;
                    }

                    // I don't ever want to see these damn things again

                    #region And

                    if (andTags.Count > 0)
                    {
                        bool andPass = true;
                        foreach (var andTag in andTags)
                        {
                            var match = fmTags.FirstOrDefault(x => x.Category == andTag.Category);
                            if (match == null)
                            {
                                andPass = false;
                                break;
                            }

                            if (andTag.Tags.Count > 0)
                            {
                                foreach (var andTagTag in andTag.Tags)
                                {
                                    if (match.Tags.FirstOrDefault(x => x == andTagTag) == null)
                                    {
                                        andPass = false;
                                        break;
                                    }
                                }

                                if (!andPass) break;
                            }
                        }

                        if (!andPass)
                        {
                            s.FilterShownIndexList.RemoveAt(i);
                            i--;
                            continue;
                        }
                    }

                    #endregion

                    #region Or

                    if (orTags.Count > 0)
                    {
                        bool orPass = false;
                        foreach (var orTag in orTags)
                        {
                            var match = fmTags.FirstOrDefault(x => x.Category == orTag.Category);
                            if (match == null) continue;

                            if (orTag.Tags.Count > 0)
                            {
                                foreach (var orTagTag in orTag.Tags)
                                {
                                    if (match.Tags.FirstOrDefault(x => x == orTagTag) != null)
                                    {
                                        orPass = true;
                                        break;
                                    }
                                }

                                if (orPass) break;
                            }
                            else
                            {
                                orPass = true;
                            }
                        }

                        if (!orPass)
                        {
                            s.FilterShownIndexList.RemoveAt(i);
                            i--;
                            continue;
                        }
                    }

                    #endregion

                    #region Not

                    if (notTags.Count > 0)
                    {
                        bool notPass = true;
                        foreach (var notTag in notTags)
                        {
                            var match = fmTags.FirstOrDefault(x => x.Category == notTag.Category);
                            if (match == null) continue;

                            if (notTag.Tags.Count == 0)
                            {
                                notPass = false;
                                continue;
                            }

                            if (notTag.Tags.Count > 0)
                            {
                                foreach (var notTagTag in notTag.Tags)
                                {
                                    if (match.Tags.FirstOrDefault(x => x == notTagTag) != null)
                                    {
                                        notPass = false;
                                        break;
                                    }
                                }

                                if (!notPass) break;
                            }
                        }

                        if (!notPass)
                        {
                            s.FilterShownIndexList.RemoveAt(i);
                            i--;
                            continue;
                        }
                    }

                    #endregion
                }
            }

            #endregion

            #region Rating

            if (!(s.Filter.RatingFrom == -1 && s.Filter.RatingTo == 10))
            {
                var rf = s.Filter.RatingFrom;
                var rt = s.Filter.RatingTo;

                for (int i = 0; i < s.FilterShownIndexList.Count; i++)
                {
                    var fmRating = FMsList[s.FilterShownIndexList[i]].Rating;

                    if (fmRating < rf || fmRating > rt)
                    {
                        s.FilterShownIndexList.RemoveAt(i);
                        i--;
                    }
                }
            }

            #endregion

            #region Release date

            if (s.Filter.ReleaseDateFrom != null || s.Filter.ReleaseDateTo != null)
            {
                var rdf = s.Filter.ReleaseDateFrom;
                var rdt = s.Filter.ReleaseDateTo;

                for (int i = 0; i < s.FilterShownIndexList.Count; i++)
                {
                    var fmRelDate = FMsList[s.FilterShownIndexList[i]].ReleaseDate;

                    if (fmRelDate == null ||
                        (rdf != null &&
                         fmRelDate.Value.Date.CompareTo(rdf.Value.Date) < 0) ||
                        (rdt != null &&
                         fmRelDate.Value.Date.CompareTo(rdt.Value.Date) > 0))
                    {
                        s.FilterShownIndexList.RemoveAt(i);
                        i--;
                    }
                }
            }

            #endregion

            #region Last played

            if (s.Filter.LastPlayedFrom != null || s.Filter.LastPlayedTo != null)
            {
                var lpdf = s.Filter.LastPlayedFrom;
                var lpdt = s.Filter.LastPlayedTo;

                for (int i = 0; i < s.FilterShownIndexList.Count; i++)
                {
                    var fmLastPlayed = FMsList[s.FilterShownIndexList[i]].LastPlayed;

                    if (fmLastPlayed == null ||
                        (lpdf != null &&
                         fmLastPlayed.Value.Date.CompareTo(lpdf.Value.Date) < 0) ||
                        (lpdt != null &&
                         fmLastPlayed.Value.Date.CompareTo(lpdt.Value.Date) > 0))
                    {
                        s.FilterShownIndexList.RemoveAt(i);
                        i--;
                    }
                }
            }

            #endregion

            #region Finished

            if (s.Filter.Finished.Count > 0)
            {
                for (int i = 0; i < s.FilterShownIndexList.Count; i++)
                {
                    var fmFinished = FMsList[s.FilterShownIndexList[i]].FinishedOn;

                    if (fmFinished > 0 && !s.Filter.Finished.Contains(FinishedState.Finished) ||
                       fmFinished <= 0 && !s.Filter.Finished.Contains(FinishedState.Unfinished))
                    {
                        s.FilterShownIndexList.RemoveAt(i);
                        i--;
                    }
                }
            }

            #endregion

            s.Filtered = true;

            t.Stop();
            Trace.WriteLine("SetFilter() (filter work only):\r\n" + t.Elapsed);

            // If the actual selected FM hasn't changed, don't reload its readme. While this can't eliminate the
            // lag when a filter selection first lands on a heavy readme, it at least prevents said readme from
            // being reloaded over and over every time you type a letter if the selected FM hasn't changed.

            if (suppressRefresh) return;

            // NOTE: GetFMFromIndex(0) takes 0 because RefreshFMsList() sets the selection to 0.
            // Remember this if you ever change that.
            RefreshFMsList(
                refreshReadme: forceRefreshReadme || s.FilterShownIndexList.Count == 0 ||
                               (oldSelectedFM != null && !oldSelectedFM.Equals(GetFMFromIndex(0))),
                suppressSelectionChangedEvent: forceSuppressSelectionChangedEvent || oldSelectedFM != null);
        }

        private void FMsDGV_CellValueNeeded_Initial(object sender, DataGridViewCellValueEventArgs e)
        {
            // Lazy-load these in an attempt to save some kind of startup time
            Thief1Icon = Resources.Thief1;
            Thief2Icon = Resources.Thief2;
            Thief3Icon = Resources.Thief3;
            BlankIcon = Resources.Blank;
            CheckIcon = Resources.CheckCircle;
            RedQuestionMarkIcon = Resources.QuestionMarkCircleRed;
            StarIcons = new[]
            {
                Resources.Stars0,
                Resources.Stars0_5,
                Resources.Stars1,
                Resources.Stars1_5,
                Resources.Stars2,
                Resources.Stars2_5,
                Resources.Stars3,
                Resources.Stars3_5,
                Resources.Stars4,
                Resources.Stars4_5,
                Resources.Stars5
            };

            FinishedOnIcons = new Dictionary<FinishedOn, Bitmap>
            {
                {
                    FinishedOn.None,
                    Resources.Finished_None
                },
                {
                    FinishedOn.Normal,
                    Resources.Finished_Normal
                },
                {
                    FinishedOn.Normal | FinishedOn.Hard,
                    Resources.Finished_Normal_Hard
                },
                {
                    FinishedOn.Normal | FinishedOn.Hard | FinishedOn.Expert,
                    Resources.Finished_Normal_Hard_Expert
                },
                {
                    FinishedOn.Normal | FinishedOn.Hard | FinishedOn.Extreme,
                    Resources.Finished_Normal_Hard_Extreme
                },
                {
                    FinishedOn.Normal | FinishedOn.Hard | FinishedOn.Expert | FinishedOn.Extreme,
                    Resources.Finished_Normal_Hard_Expert_Extreme
                },
                {
                    FinishedOn.Normal | FinishedOn.Expert,
                    Resources.Finished_Normal_Expert
                },
                {
                    FinishedOn.Normal | FinishedOn.Expert | FinishedOn.Extreme,
                    Resources.Finished_Normal_Expert_Extreme
                },
                {
                    FinishedOn.Normal | FinishedOn.Extreme,
                    Resources.Finished_Normal_Extreme
                },
                {
                    FinishedOn.Hard,
                    Resources.Finished_Hard
                },
                {
                    FinishedOn.Hard | FinishedOn.Expert,
                    Resources.Finished_Hard_Expert
                },
                {
                    FinishedOn.Hard | FinishedOn.Expert | FinishedOn.Extreme,
                    Resources.Finished_Hard_Expert_Extreme
                },
                {
                    FinishedOn.Hard | FinishedOn.Extreme,
                    Resources.Finished_Hard_Extreme
                },
                {
                    FinishedOn.Expert,
                    Resources.Finished_Expert
                },
                {
                    FinishedOn.Expert | FinishedOn.Extreme,
                    Resources.Finished_Expert_Extreme
                },
                {
                    FinishedOn.Extreme,
                    Resources.Finished_Extreme
                }
            };

            // Prevents having to check the bool again forevermore even after we've already set the images.
            // Taking an extremely minor technique from a data-oriented design talk, heck yeah!
            FMsDGV.CellValueNeeded -= FMsDGV_CellValueNeeded_Initial;
            FMsDGV.CellValueNeeded += FMsDGV_CellValueNeeded;
            FMsDGV_CellValueNeeded(sender, e);
        }

        private void FMsDGV_CellValueNeeded(object sender, DataGridViewCellValueEventArgs e)
        {
            if (FMsDGV.Filtered && FMsDGV.FilterShownIndexList.Count == 0) return;

            var fm = GetFMFromIndex(e.RowIndex);

            // PERF: ~0.14ms per FM for en-US Long Date format
            string FormattedDate(DateTime dt)
            {
                return
                    Config.DateFormat == DateFormat.CurrentCultureShort ? dt.ToShortDateString() :
                    Config.DateFormat == DateFormat.CurrentCultureLong ? dt.ToLongDateString() :
                    Config.DateFormat == DateFormat.Custom ? dt.ToString(Config.DateCustomFormatString) :
                    throw new Exception("Config.DateFormat is not what it should be!");
            }

            switch ((Column)FMsDGV.Columns[e.ColumnIndex].Index)
            {
                case Column.Game:
                    e.Value =
                        fm.Game == Game.Thief1 ? Thief1Icon :
                        fm.Game == Game.Thief2 ? Thief2Icon :
                        fm.Game == Game.Thief3 ? Thief3Icon :
                        fm.Game == Game.Unsupported ? RedQuestionMarkIcon :
                        // Can't say null, or else it sets an ugly red-x image
                        BlankIcon;
                    break;

                case Column.Installed:
                    e.Value = fm.Installed ? CheckIcon : BlankIcon;
                    break;

                case Column.Title:
                    if (Config.EnableArticles && Config.MoveArticlesToEnd)
                    {
                        string title = fm.Title;
                        for (int i = 0; i < Config.Articles.Count; i++)
                        {
                            var a = Config.Articles[i];
                            if (fm.Title.StartsWithI(a + " "))
                            {
                                // Take the actual article from the name so as to preserve casing
                                title = fm.Title.Substring(a.Length + 1) + ", " + fm.Title.Substring(0, a.Length);
                                break;
                            }
                        }
                        e.Value = title;
                    }
                    else
                    {
                        e.Value = fm.Title;
                    }
                    break;

                case Column.Archive:
                    e.Value = fm.Archive;
                    break;

                case Column.Author:
                    e.Value = fm.Author;
                    break;

                case Column.Size:
                    // This one gets changed for every FM on language change, so it's good
                    e.Value = fm.SizeString;
                    break;

                case Column.Rating:
                    if (Config.RatingDisplayStyle == RatingDisplayStyle.NewDarkLoader)
                    {
                        e.Value = fm.Rating == -1 ? "" : fm.Rating.ToString();
                    }
                    else
                    {
                        if (Config.RatingUseStars)
                        {
                            e.Value = fm.Rating == -1 ? BlankIcon : StarIcons[fm.Rating];
                        }
                        else
                        {
                            e.Value = fm.Rating == -1 ? "" : (fm.Rating / 2.0).ToString(CultureInfo.CurrentCulture);
                        }
                    }
                    break;

                case Column.Finished:
                    e.Value = FinishedOnIcons[(FinishedOn)fm.FinishedOn];
                    break;

                case Column.ReleaseDate:
                    e.Value = fm.ReleaseDate != null ? FormattedDate((DateTime)fm.ReleaseDate) : "";
                    break;

                case Column.LastPlayed:
                    e.Value = fm.LastPlayed != null ? FormattedDate((DateTime)fm.LastPlayed) : "";
                    break;

                case Column.DisabledMods:
                    e.Value = fm.DisableAllMods ? LText.FMsListAllModsDisabledMessage : fm.DisabledMods;
                    break;

                case Column.Comment:
                    e.Value = fm.CommentSingleLine;
                    break;
            }
        }

        private void FMsDGV_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;

            var newSortDirection =
                e.ColumnIndex == FMsDGV.CurrentSortedColumn && FMsDGV.CurrentSortDirection == SortOrder.Ascending
                    ? SortOrder.Descending
                    : SortOrder.Ascending;

            SortFMTable((Column)e.ColumnIndex, newSortDirection);
            if (FMsDGV.Filtered)
            {
                // SetFilter() calls a refresh on its own
                SetFilter();
            }
            else
            {
                RefreshFMsList(refreshReadme: true, suppressSelectionChangedEvent: true);
            }
        }

        private void FMsDGV_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right) return;

            var ht = FMsDGV.HitTest(e.X, e.Y);

            #region Right-click menu

            if (ht.Type == DataGridViewHitTestType.ColumnHeader || ht.Type == DataGridViewHitTestType.None)
            {
                FMsDGV.ContextMenuStrip = FMsDGV.FMColumnHeaderRightClickMenu;
            }
            else if (ht.Type == DataGridViewHitTestType.Cell && ht.ColumnIndex > -1 && ht.RowIndex > -1)
            {
                FMsDGV.ContextMenuStrip = FMRightClickMenu;

                var fm = GetFMFromIndex(ht.RowIndex);

                TasksRCSubMenu.Enabled = GameIsDark(fm) && fm.Installed;
                FMsDGV.Rows[ht.RowIndex].Selected = true;
            }
            else
            {
                FMsDGV.ContextMenuStrip = null;
            }

            #endregion
        }

        #region Crappy hack for basic go-to-first-typed-letter

        private int FindRowIndex(char firstChar)
        {
            var s = FMsDGV;

            for (int i = 0; i < s.RowCount; i++)
            {
                if (s.Rows[i].Cells[(int)Column.Title].Value.ToString().StartsWithI(firstChar.ToString()))
                {
                    return i;
                }
            }

            return -1;
        }

        private void FMsDGV_KeyPress(object sender, KeyPressEventArgs e)
        {
            if ((e.KeyChar >= 65 && e.KeyChar <= 90) || (e.KeyChar >= 97 && e.KeyChar <= 122))
            {
                var s = FMsDGV;

                var rowIndex = FindRowIndex(e.KeyChar);

                if (rowIndex > -1)
                {
                    s.Rows[rowIndex].Selected = true;
                    s.FirstDisplayedScrollingRowIndex = s.SelectedRows[0].Index;
                }
            }
        }

        #endregion

        #region FM context menu

        private void FMRightClickMenu_Opening(object sender, CancelEventArgs e)
        {
            // Fix for a corner case where the user could press the right mouse button, hold it, keyboard-switch
            // to an empty tab, then let up the mouse and a menu would come up even though no FM was selected.
            if (FMsDGV.RowCount == 0 || FMsDGV.SelectedRows.Count == 0) e.Cancel = true;
        }

        private async void PlayFMMenuItem_Click(object sender, EventArgs e) => await CallInstallOrPlay();

        private async void InstallUninstallMenuItem_Click(object sender, EventArgs e)
        {
            var fm = GetSelectedFM();

            await Model.InstallOrUninstall(fm);
        }

        private async void ConvertWAVsTo16BitMenuItem_Click(object sender, EventArgs e)
        {
            var fm = GetSelectedFM();
            if (!fm.Installed) return;

            await Model.ConvertWAVsTo16Bit(fm);
        }

        private async void ConvertOGGsToWAVsMenuItem_Click(object sender, EventArgs e)
        {
            var fm = GetSelectedFM();
            if (!fm.Installed) return;

            await Model.ConvertOGGsToWAVs(fm);
        }

        #endregion

        #endregion

        private async Task CallInstallOrPlay()
        {
            var fm = GetSelectedFM();

            if (!fm.Installed && !await Model.InstallFM(fm)) return;

            if (Model.PlayFM(fm))
            {
                fm.LastPlayed = DateTime.Now;
                RefreshSelectedFM(refreshReadme: false);
            }
        }

        #region Install/Play buttons

        private async void InstallUninstallFMButton_Click(object sender, EventArgs e)
        {
            var fm = GetSelectedFM();

            await Model.InstallOrUninstall(fm);
        }

        private async void PlayFMButton_Click(object sender, EventArgs e) => await CallInstallOrPlay();

        #region Play original game

        private void PlayOriginalGameButton_Click(object sender, EventArgs e)
        {
            var button = PlayOriginalGameButton;
            var menu = PlayOriginalGameMenu;

            PlayOriginalThief1MenuItem.Enabled = !Config.T1Exe.IsEmpty();
            PlayOriginalThief2MenuItem.Enabled = !Config.T2Exe.IsEmpty();
            PlayOriginalThief3MenuItem.Enabled = !Config.T3Exe.IsEmpty();

            menu.Show(button, 0, -menu.Height);
        }

        private void PlayOriginalGameMenuItem_Click(object sender, EventArgs e)
        {
            var s = (ToolStripMenuItem)sender;

            var game =
                s == PlayOriginalThief1MenuItem ? Game.Thief1 :
                s == PlayOriginalThief2MenuItem ? Game.Thief2 :
                Game.Thief3;

            Model.PlayOriginalGame(game);
        }

        #endregion

        #endregion

        private async void ScanAllFMsButton_Click(object sender, EventArgs e)
        {
            var scanOptions = new ScanOptions
            {
                ScanTitle = true,
                ScanCampaignMissionNames = false,
                ScanAuthor = true,
                ScanVersion = false,
                ScanLanguages = true,
                ScanGameType = true,
                ScanNewDarkRequired = false,
                ScanNewDarkMinimumVersion = false,
                ScanCustomResources = true,
                ScanSize = true
            };

            var success = await Model.ScanAllFMs(scanOptions);
            if (success) SetFilter(forceRefreshReadme: true);
        }

        private async void SettingsButton_Click(object sender, EventArgs e) => await OpenSettings();

        internal async Task<bool> OpenSettings(bool startup = false)
        {
            using (var sf = new SettingsForm(Config, startup))
            {
                // This needs to be separate so the below line can work
                var result = sf.ShowDialog();

                // Special case: this is meta, so it should always be set even if the user clicked Cancel
                Config.SettingsTab = sf.OutConfig.SettingsTab;

                if (result != DialogResult.OK) return false;

                #region Set changed bools

                bool archivePathsChanged =
                    !startup &&
                    (!Config.FMArchivePaths.SequenceEqual(sf.OutConfig.FMArchivePaths, StringComparer.OrdinalIgnoreCase) ||
                    Config.FMArchivePathsIncludeSubfolders != sf.OutConfig.FMArchivePathsIncludeSubfolders);

                bool gamePathsChanged =
                    !startup &&
                    (!Config.T1Exe.EqualsI(sf.OutConfig.T1Exe) ||
                    !Config.T2Exe.EqualsI(sf.OutConfig.T2Exe) ||
                    !Config.T3Exe.EqualsI(sf.OutConfig.T3Exe));

                bool gameOrganizationChanged =
                    !startup && (Config.GameOrganization != sf.OutConfig.GameOrganization);

                bool articlesChanged =
                    !startup &&
                    (Config.EnableArticles != sf.OutConfig.EnableArticles ||
                    !Config.Articles.SequenceEqual(sf.OutConfig.Articles, StringComparer.InvariantCultureIgnoreCase) ||
                    Config.MoveArticlesToEnd != sf.OutConfig.MoveArticlesToEnd);

                bool dateFormatChanged =
                    !startup &&
                    (Config.DateFormat != sf.OutConfig.DateFormat ||
                    Config.DateCustomFormatString != sf.OutConfig.DateCustomFormatString);

                bool ratingDisplayStyleChanged =
                    !startup &&
                    (Config.RatingDisplayStyle != sf.OutConfig.RatingDisplayStyle ||
                    Config.RatingUseStars != sf.OutConfig.RatingUseStars);

                bool languageChanged =
                    !startup && (!Config.Language.EqualsI(sf.OutConfig.Language));

                #endregion

                #region Set config data

                // Set values individually (rather than deep-copying) so that non-Settings values don't get
                // overwritten.

                #region Paths tab

                #region Game exes

                Config.T1Exe = sf.OutConfig.T1Exe;
                Config.T2Exe = sf.OutConfig.T2Exe;
                Config.T3Exe = sf.OutConfig.T3Exe;

                // TODO: These should probably go in the Settings form along with the cam_mod.ini check
                // Note: SettingsForm is supposed to check these for validity, so we shouldn't have any exceptions
                //       being thrown here.
                Config.T1FMInstallPath = !Config.T1Exe.IsWhiteSpace()
                    ? Model.GetInstFMsPathFromCamModIni(Path.GetDirectoryName(Config.T1Exe), out Error error1)
                    : "";

                Config.T2FMInstallPath = !Config.T2Exe.IsWhiteSpace()
                    ? Model.GetInstFMsPathFromCamModIni(Path.GetDirectoryName(Config.T2Exe), out Error error2)
                    : "";

                if (!Config.T3Exe.IsWhiteSpace())
                {
                    var (success, useCentralSaves, t3FMInstPath) = Model.GetInstFMsPathFromT3();
                    if (success)
                    {
                        Config.T3FMInstallPath = t3FMInstPath;
                        Config.T3UseCentralSaves = useCentralSaves;
                    }
                }
                else
                {
                    Config.T3FMInstallPath = "";
                }

                #endregion

                Config.FMsBackupPath = sf.OutConfig.FMsBackupPath;

                Config.FMArchivePaths = sf.OutConfig.FMArchivePaths;
                Config.FMArchivePathsIncludeSubfolders = sf.OutConfig.FMArchivePathsIncludeSubfolders;

                #endregion

                if (startup) return true;

                // From this point on, we're not in startup mode.

                // For clarity, don't copy the other tabs' data on startup, because their tabs won't be shown and
                // so they won't have been changed

                #region FM Display tab

                Config.GameOrganization = sf.OutConfig.GameOrganization;

                Config.EnableArticles = sf.OutConfig.EnableArticles;
                Config.Articles.Clear();
                Config.Articles.AddRange(sf.OutConfig.Articles);
                Config.MoveArticlesToEnd = sf.OutConfig.MoveArticlesToEnd;

                Config.RatingDisplayStyle = sf.OutConfig.RatingDisplayStyle;
                Config.RatingUseStars = sf.OutConfig.RatingUseStars;

                Config.DateFormat = sf.OutConfig.DateFormat;
                Config.DateCustomFormat1 = sf.OutConfig.DateCustomFormat1;
                Config.DateCustomSeparator1 = sf.OutConfig.DateCustomSeparator1;
                Config.DateCustomFormat2 = sf.OutConfig.DateCustomFormat2;
                Config.DateCustomSeparator2 = sf.OutConfig.DateCustomSeparator2;
                Config.DateCustomFormat3 = sf.OutConfig.DateCustomFormat3;
                Config.DateCustomSeparator3 = sf.OutConfig.DateCustomSeparator3;
                Config.DateCustomFormat4 = sf.OutConfig.DateCustomFormat4;
                Config.DateCustomFormatString = sf.OutConfig.DateCustomFormatString;

                #endregion

                #region Other tab

                Config.ConvertWAVsTo16BitOnInstall = sf.OutConfig.ConvertWAVsTo16BitOnInstall;
                Config.ConvertOGGsToWAVsOnInstall = sf.OutConfig.ConvertOGGsToWAVsOnInstall;

                Config.BackupSaves = sf.OutConfig.BackupSaves;

                Config.WebSearchUrl = sf.OutConfig.WebSearchUrl;

                #endregion

                // These ones MUST NOT to be set on startup, because the source values won't be valid
                Config.SortedColumn = (Column)FMsDGV.CurrentSortedColumn;
                Config.SortDirection = FMsDGV.CurrentSortDirection;

                #endregion

                #region Change-specific actions (pre-refresh)

                if (archivePathsChanged || gamePathsChanged)
                {
                    Model.FindFMs(startup: false);

                    // The reference gets broken somehow, even though it's only FMsList.Clear() and not
                    // FMsList = new List<FanMission>() (could be the Union()s?)
                    FMsList = Model.FMsList;
                }
                if (gameOrganizationChanged)
                {
                    // Clear everything to defaults so we don't have any leftover state screwing things all up
                    Config.ClearAllSelectedFMs();
                    Config.ClearAllFilters();
                    Config.GameTab = Game.Thief1;
                    ClearAllUIAndInternalFilters();
                    if (Config.GameOrganization == GameOrganization.ByTab)
                        Config.Filter.Games.Add(Game.Thief1);
                    ChangeGameOrganization();
                }
                if (ratingDisplayStyleChanged)
                {
                    UpdateRatingLists(Config.RatingDisplayStyle == RatingDisplayStyle.FMSel);
                    UpdateRatingColumn(startup: false);
                    UpdateRatingLabel();
                }
                if (languageChanged)
                {
                    // This will set "KB" / "MB" / "GB" to localized, and decimal separator to current culture
                    foreach (var fm in FMsList) fm.SizeString = ((long?)fm.SizeBytes).ConvertSize();
                }

                #endregion

                #region Call appropriate refresh method (if applicable)

                // Game paths should have been checked and verified before OK was clicked, so assume they're good
                // here
                if (gamePathsChanged || archivePathsChanged || gameOrganizationChanged || articlesChanged)
                {
                    if (gamePathsChanged || archivePathsChanged) await ScanNewFMsForGameType();

                    SortFMTable(Config.SortedColumn, Config.SortDirection);
                    SetFilter(forceRefreshReadme: true, forceSuppressSelectionChangedEvent: true);
                }
                else if (dateFormatChanged || languageChanged)
                {
                    RefreshFMsListKeepSelection();
                }

                #endregion
            }

            return true;
        }

        private void UpdateRatingLists(bool fmSelStyle)
        {
            for (int i = 0; i <= 10; i++)
            {
                string num = (fmSelStyle ? i / 2.0 : i).ToString(CultureInfo.CurrentCulture);
                RatingRCSubMenu.DropDownItems[i + 1].Text = num;
                EditFMRatingComboBox.Items[i + 1] = num;
            }
        }

        private void UpdateRatingColumn(bool startup)
        {
            var newRatingColumn =
                Config.RatingDisplayStyle == RatingDisplayStyle.FMSel && Config.RatingUseStars
                    ? (DataGridViewColumn)RatingImageColumn
                    : RatingTextColumn;

            if (!startup)
            {
                var oldRatingColumn = FMsDGV.Columns[(int)Column.Rating];
                newRatingColumn.Width = newRatingColumn == RatingTextColumn ? oldRatingColumn.Width : RatingImageColumnWidth;
                newRatingColumn.Visible = oldRatingColumn.Visible;
                newRatingColumn.DisplayIndex = oldRatingColumn.DisplayIndex;
            }

            if (!startup || newRatingColumn != RatingTextColumn)
            {
                FMsDGV.Columns.RemoveAt((int)Column.Rating);
                FMsDGV.Columns.Insert((int)Column.Rating, newRatingColumn);
                if (FMsDGV.CurrentSortedColumn == (int)Column.Rating)
                {
                    FMsDGV.Columns[(int)Column.Rating].HeaderCell.SortGlyphDirection = FMsDGV.CurrentSortDirection;
                }
            }

            if (!startup)
            {
                FMsDGV.FillColumns(FMsDGV.ColumnsToColumnData());
                RefreshFMsListKeepSelection();
            }
        }

        private void OpenFilterTags()
        {
            using (var tf = new FilterTagsForm2(GlobalTags, FMsDGV.Filter.Tags))
            {
                if (tf.ShowDialog() != DialogResult.OK) return;

                DeepCopyTagsFilter(tf.TagsFilter, FMsDGV.Filter.Tags);
                FilterByTagsButton.Checked = !FMsDGV.Filter.Tags.Empty();
            }

            SetFilter();
        }

        internal void SetDebugMessageText(string text) => DebugLabel.Text = text;

        #region Refresh FMs list

        internal void RefreshSelectedFMRowOnly() => RefreshSelectedFM(false, true);

        internal void RefreshSelectedFM(bool refreshReadme, bool refreshGridRowOnly = false)
        {
            var currentRow = FMsDGV.SelectedRows[0].Index;

            FMsDGV.InvalidateRow(currentRow);

            if (refreshGridRowOnly) return;

            DisplaySelectedFM(GetFMFromIndex(currentRow), refreshReadme);
        }

        internal void RefreshFMsList(bool refreshReadme, bool suppressSelectionChangedEvent = false)
        {
            var s = FMsDGV;

            using (suppressSelectionChangedEvent ? new DisableEvents(this) : null)
            {
                // A small but measurable perf increase from this. Also prevents flickering when switching game
                // tabs.
                s.SuspendDrawing();
                s.RowCount = s.Filtered ? s.FilterShownIndexList.Count : FMsList.Count;

                if (s.RowCount == 0)
                {
                    s.ResumeDrawing();
                    ClearShownData();
                    InitialSelectedFMHasBeenSet = true;
                }
                else
                {
                    int row;
                    if (InitialSelectedFMHasBeenSet)
                    {
                        row = 0;
                        s.FirstDisplayedScrollingRowIndex = 0;
                    }
                    else
                    {
                        row = GetIndexFromInstalledName(s.CurrentSelFM.InstalledName).ClampToZero();
                        s.FirstDisplayedScrollingRowIndex = (row - s.CurrentSelFM.IndexFromTop).ClampToZero();
                        refreshReadme = true;
                    }

                    using (!InitialSelectedFMHasBeenSet ? new DisableEvents(this) : null)
                    {
                        s.Rows[row].Selected = true;
                    }

                    // Resume drawing before loading the readme; that way the list will update instantly even
                    // if the readme doesn't. The user will see delays in the "right place" (the readme box)
                    // and understand why it takes a sec. Otherwise, it looks like merely changing tabs brings
                    // a significant delay, and that's annoying because it doesn't seem like it should happen.
                    s.ResumeDrawing();

                    DisplaySelectedFM(GetFMFromIndex(row), refreshReadme);

                    InitialSelectedFMHasBeenSet = true;
                }
            }
        }

        private void RefreshFMsListKeepSelection()
        {
            var s = FMsDGV;

            if (s.RowCount == 0) return;

            var selectedRow = s.SelectedRows[0].Index;

            using (new DisableEvents(this))
            {
                s.Refresh();
                s.Rows[selectedRow].Selected = true;
            }
        }

        #endregion

        internal void SortFMTable(Column column, SortOrder sortDirection)
        {
            FMsDGV.CurrentSortedColumn = (int)column;
            FMsDGV.CurrentSortDirection = sortDirection;

            var articles = Config.EnableArticles ? Config.Articles : new List<string>();

            void SortByTitle(bool reverse = false)
            {
                var ascending = reverse ? SortOrder.Descending : SortOrder.Ascending;

                FMsList = sortDirection == ascending
                    ? FMsList.OrderBy(x => x.Title, new FMTitleComparer(articles)).ToList()
                    : FMsList.OrderByDescending(x => x.Title, new FMTitleComparer(articles)).ToList();
            }

            // For any column which could have empty entries, sort by title first in order to maintain a
            // consistent order

            switch (column)
            {
                case Column.Game:
                    SortByTitle();
                    FMsList = sortDirection == SortOrder.Ascending
                        ? FMsList.OrderBy(x => x.Game).ToList()
                        : FMsList.OrderByDescending(x => x.Game).ToList();
                    break;

                case Column.Installed:
                    SortByTitle();
                    // Reverse this because "Installed" should go on top and blanks should go on bottom
                    FMsList = sortDirection == SortOrder.Descending
                        ? FMsList.OrderBy(x => x.Installed).ToList()
                        : FMsList.OrderByDescending(x => x.Installed).ToList();
                    break;

                case Column.Title:
                    SortByTitle();
                    break;

                case Column.Archive:
                    FMsList = sortDirection == SortOrder.Ascending
                        ? FMsList.OrderBy(x => x.Archive).ToList()
                        : FMsList.OrderByDescending(x => x.Archive).ToList();
                    break;

                case Column.Author:
                    SortByTitle();
                    FMsList = sortDirection == SortOrder.Ascending
                        ? FMsList.OrderBy(x => x.Author).ToList()
                        : FMsList.OrderByDescending(x => x.Author).ToList();
                    break;

                case Column.Size:
                    SortByTitle();
                    FMsList = sortDirection == SortOrder.Ascending
                        ? FMsList.OrderBy(x => x.SizeBytes).ToList()
                        : FMsList.OrderByDescending(x => x.SizeBytes).ToList();
                    break;

                case Column.Rating:
                    SortByTitle();
                    FMsList = sortDirection == SortOrder.Ascending
                        ? FMsList.OrderBy(x => x.Rating).ToList()
                        : FMsList.OrderByDescending(x => x.Rating).ToList();
                    break;

                case Column.Finished:
                    SortByTitle();
                    FMsList = sortDirection == SortOrder.Ascending
                        ? FMsList.OrderBy(x => x.FinishedOn).ToList()
                        : FMsList.OrderByDescending(x => x.FinishedOn).ToList();
                    break;

                case Column.ReleaseDate:
                    SortByTitle();

                    FMsList = sortDirection == SortOrder.Ascending
                        ? FMsList.OrderBy(x => x.ReleaseDate?.Date ?? x.ReleaseDate).ToList()
                        : FMsList.OrderByDescending(x => x.ReleaseDate?.Date ?? x.ReleaseDate).ToList();
                    break;

                case Column.LastPlayed:
                    SortByTitle();
                    FMsList = sortDirection == SortOrder.Ascending
                        ? FMsList.OrderBy(x => x.LastPlayed?.Date ?? x.LastPlayed).ToList()
                        : FMsList.OrderByDescending(x => x.LastPlayed?.Date ?? x.LastPlayed).ToList();
                    break;

                case Column.DisabledMods:
                    SortByTitle();
                    FMsList = sortDirection == SortOrder.Ascending
                        ? FMsList.OrderBy(x => x.DisabledMods).ToList()
                        : FMsList.OrderByDescending(x => x.DisabledMods).ToList();
                    break;

                case Column.Comment:
                    SortByTitle();
                    FMsList = sortDirection == SortOrder.Ascending
                        ? FMsList.OrderBy(x => x.CommentSingleLine).ToList()
                        : FMsList.OrderByDescending(x => x.CommentSingleLine).ToList();
                    break;
            }

            foreach (DataGridViewColumn x in FMsDGV.Columns)
            {
                x.HeaderCell.SortGlyphDirection = SortOrder.None;
            }

            FMsDGV.Columns[(int)column].HeaderCell.SortGlyphDirection = FMsDGV.CurrentSortDirection;
        }

        private void DisplayFMTags(FanMission fm)
        {
            var tv = TagsTreeView;

            try
            {
                tv.SuspendDrawing();
                tv.Nodes.Clear();

                if (fm.Tags.Count == 0) return;

                fm.Tags.SortCat();

                foreach (var item in fm.Tags)
                {
                    tv.Nodes.Add(item.Category);
                    var last = tv.Nodes[tv.Nodes.Count - 1];
                    foreach (var tag in item.Tags) last.Nodes.Add(tag);
                }

                tv.ExpandAll();
            }
            finally
            {
                tv.ResumeDrawing();
            }
        }

        private void FMsDGV_SelectionChanged(object sender, EventArgs e)
        {
            if (EventsDisabled) return;
            if (FMsDGV.SelectedRows.Count == 0)
            {
                ClearShownData();
            }
            else
            {
                // Working with an event-driven GUI in general is okay, but initializing one is HELL. IN. A. CAN.
                // Constantly babysitting the damn thing with global flags all over the place to tell it to STOP
                // DOING SHIT WHILE I'M TRYING TO SET FIRST VALUES. Argh.
                if (!InitialSelectedFMHasBeenSet) return;

                var fm = GetSelectedFM();
                DisplaySelectedFM(fm, refreshReadme: true);
            }
        }

        // Perpetual TODO: Make sure this clears everything including the top right tab stuff
        private void ClearShownData()
        {
            InstallUninstallMenuItem.Text = LText.FMContextMenu.InstallFM;
            InstallUninstallMenuItem.Enabled = false;
            InstallUninstallFMButton.Enabled = false;
            PlayFMMenuItem.Enabled = false;
            PlayFMButton.Enabled = false;
            // Hide instead of clear to avoid zoom factor pain
            ShowReadme(false);
            ChooseReadmePanel.Hide();
            ViewHTMLReadmeButton.Hide();
            WebSearchButton.Enabled = false;

            BlankStatsPanelWithMessage(LText.StatisticsTab.NoFMSelected);

            AltTitlesMenu.Items.Clear();

            using (new DisableEvents(this))
            {
                EditFMRatingComboBox.SelectedIndex = 0;

                foreach (Control c in EditFMTabPage.Controls)
                {
                    switch (c)
                    {
                        case TextBox tb:
                            tb.Text = "";
                            break;
                        case DateTimePicker dtp:
                            dtp.Value = DateTime.Now;
                            dtp.Hide();
                            break;
                        case CheckBox chk:
                            chk.Checked = false;
                            break;
                    }

                    c.Enabled = false;
                }

                FinishedOnNormalMenuItem.Checked = false;
                FinishedOnHardMenuItem.Checked = false;
                FinishedOnExpertMenuItem.Checked = false;
                FinishedOnExtremeMenuItem.Checked = false;

                CommentTextBox.Text = "";
                CommentTextBox.Enabled = false;
                AddTagTextBox.Text = "";
            }

            TagsTreeView.Nodes.Clear();

            foreach (Control c in TagsTabPage.Controls) c.Enabled = false;
        }

        private void BlankStatsPanelWithMessage(string message)
        {
            CustomResourcesLabel.Text = message;
            foreach (CheckBox cb in StatsCheckBoxesPanel.Controls) cb.Checked = false;
            StatsCheckBoxesPanel.Hide();
        }

        private static bool FMCustomResourcesScanned(FanMission fm)
        {
            return fm.HasMap != null &&
                   fm.HasAutomap != null &&
                   fm.HasScripts != null &&
                   fm.HasTextures != null &&
                   fm.HasSounds != null &&
                   fm.HasObjects != null &&
                   fm.HasCreatures != null &&
                   fm.HasMotions != null &&
                   fm.HasMovies != null &&
                   fm.HasSubtitles != null;
        }

        // It's really hard to come up with a succinct name that makes it clear what this does and doesn't do
        private void DisplaySelectedFM(FanMission fm, bool refreshReadme = false)
        {
            bool fmIsT3 = fm.Game == Game.Thief3;

            #region Toggles

            FinishedOnNormalMenuItem.Text = fmIsT3 ? LText.Difficulties.Easy : LText.Difficulties.Normal;
            FinishedOnHardMenuItem.Text = fmIsT3 ? LText.Difficulties.Normal : LText.Difficulties.Hard;
            FinishedOnExpertMenuItem.Text = fmIsT3 ? LText.Difficulties.Hard : LText.Difficulties.Expert;
            FinishedOnExtremeMenuItem.Text = fmIsT3 ? LText.Difficulties.Expert : LText.Difficulties.Extreme;

            var installable = GameIsKnownAndSupported(fm);
            InstallUninstallMenuItem.Enabled = installable;
            InstallUninstallFMButton.Enabled = installable;
            PlayFMMenuItem.Enabled = installable;
            PlayFMButton.Enabled = installable;
            InstallUninstallMenuItem.Text = fm.Installed ? LText.FMContextMenu.UninstallFM : LText.FMContextMenu.InstallFM;
            InstallUninstallFMButton.SetL10nText(fm.Installed ? LText.BottomArea.UninstallFM : LText.BottomArea.InstallFM);

            #endregion

            WebSearchButton.Enabled = true;

            foreach (Control c in EditFMTabPage.Controls) c.Enabled = true;

            CommentTextBox.Enabled = true;
            foreach (Control c in TagsTabPage.Controls) c.Enabled = true;

            #region FinishedOn

            var val = (FinishedOn)fm.FinishedOn;
            // I don't have to disable events because I'm only wired up to Click, not Checked
            FinishedOnNormalMenuItem.Checked = (val & FinishedOn.Normal) == FinishedOn.Normal;
            FinishedOnHardMenuItem.Checked = (val & FinishedOn.Hard) == FinishedOn.Hard;
            FinishedOnExpertMenuItem.Checked = (val & FinishedOn.Expert) == FinishedOn.Expert;
            FinishedOnExtremeMenuItem.Checked = (val & FinishedOn.Extreme) == FinishedOn.Extreme;

            #endregion

            #region Custom resources

            if (fmIsT3)
            {
                BlankStatsPanelWithMessage(LText.StatisticsTab.CustomResourcesNotSupportedForThief3);
            }
            else if (!FMCustomResourcesScanned(fm))
            {
                BlankStatsPanelWithMessage(LText.StatisticsTab.CustomResourcesNotScanned);
            }
            else
            {
                CustomResourcesLabel.Text = LText.StatisticsTab.CustomResources;

                CR_MapCheckBox.Checked = fm.HasMap == true;
                CR_AutomapCheckBox.Checked = fm.HasAutomap == true;
                CR_ScriptsCheckBox.Checked = fm.HasScripts == true;
                CR_TexturesCheckBox.Checked = fm.HasTextures == true;
                CR_SoundsCheckBox.Checked = fm.HasSounds == true;
                CR_ObjectsCheckBox.Checked = fm.HasObjects == true;
                CR_CreaturesCheckBox.Checked = fm.HasCreatures == true;
                CR_MotionsCheckBox.Checked = fm.HasMotions == true;
                CR_MoviesCheckBox.Checked = fm.HasMovies == true;
                CR_SubtitlesCheckBox.Checked = fm.HasSubtitles == true;

                StatsCheckBoxesPanel.Show();
            }

            #endregion

            using (new DisableEvents(this))
            {
                EditFMTitleTextBox.Text = fm.Title;

                AltTitlesMenu.Items.Clear();

                if (fm.AltTitles.Count == 0)
                {
                    EditFMAltTitlesDropDownButton.Enabled = false;
                }
                else
                {
                    List<ToolStripItem> altTitlesMenuItems = new List<ToolStripItem>();
                    foreach (var t in fm.AltTitles)
                    {
                        var item = new ToolStripMenuItem { Text = t };
                        item.Click += EditFMAltTitlesMenuItems_Click;
                        altTitlesMenuItems.Add(item);
                    }
                    AltTitlesMenu.Items.AddRange(altTitlesMenuItems.ToArray());
                    EditFMAltTitlesDropDownButton.Enabled = true;
                }

                EditFMAuthorTextBox.Text = fm.Author;

                EditFMReleaseDateCheckBox.Checked = fm.ReleaseDate != null;
                EditFMReleaseDateDateTimePicker.Value = fm.ReleaseDate ?? DateTime.Now;
                EditFMReleaseDateDateTimePicker.Visible = fm.ReleaseDate != null;

                EditFMLastPlayedCheckBox.Checked = fm.LastPlayed != null;
                EditFMLastPlayedDateTimePicker.Value = fm.LastPlayed ?? DateTime.Now;
                EditFMLastPlayedDateTimePicker.Visible = fm.LastPlayed != null;

                EditFMDisableAllModsCheckBox.Checked = fm.DisableAllMods;
                EditFMDisabledModsTextBox.Text = fm.DisabledMods;
                EditFMDisabledModsTextBox.Enabled = !fm.DisableAllMods;

                EditFMRatingComboBox.SelectedIndex = fm.Rating + 1;

                CommentTextBox.Text = fm.Comment.FromEscapes();

                AddTagTextBox.Text = "";
            }

            DisplayFMTags(fm);

            if (!refreshReadme) return;

            var cacheData = Model.GetCacheableData(fm);

            #region Readme

            var readmeFiles = cacheData.Readmes;

            if (!readmeFiles.ContainsI(fm.SelectedReadme)) fm.SelectedReadme = null;

            using (new DisableEvents(this)) ChooseReadmeComboBox.ClearFullItems();

            if (!fm.SelectedReadme.IsEmpty())
            {
                ShowReadme(true);

                if (readmeFiles.Count > 1)
                {
                    ChooseReadmeComboBox.AddRangeFull(readmeFiles);

                    using (new DisableEvents(this)) ChooseReadmeComboBox.SelectBackingIndexOf(fm.SelectedReadme);

                    // In case the cursor is already over the readme when we do this
                    // (cause it won't show automatically if it is)
                    if (CursorOverReadmeArea()) ShowReadmeControls();
                }
                else
                {
                    ChooseReadmeComboBox.Hide();
                }
            }
            else // if fm.SelectedReadme is null or empty
            {
                if (readmeFiles.Count == 0)
                {
                    ReadmeRichTextBox.SetText(LText.ReadmeArea.NoReadmeFound);

                    ChooseReadmePanel.Hide();
                    ChooseReadmeComboBox.Hide();
                    ViewHTMLReadmeButton.Hide();
                    ShowReadme(true);

                    return;
                }
                else if (readmeFiles.Count > 1)
                {
                    ShowReadme(false);
                    ViewHTMLReadmeButton.Hide();

                    ChooseReadmeListBox.ClearFullItems();
                    ChooseReadmeListBox.AddRangeFull(readmeFiles);

                    HideReadmeControls();

                    ChooseReadmePanel.Show();

                    return;
                }
                else if (readmeFiles.Count == 1)
                {
                    fm.SelectedReadme = readmeFiles[0];

                    ChooseReadmeComboBox.Hide();
                    ShowReadme(true);
                }
            }

            ChooseReadmePanel.Hide();
            ViewHTMLReadmeButton.Hide();

            try
            {
                var (path, type) = Model.GetReadmeFileAndType(fm);
                ReadmeLoad(path, type);
            }
            catch (Exception e)
            {
                ViewHTMLReadmeButton.Hide();
                ShowReadme(true);
                ReadmeRichTextBox.SetText(LText.ReadmeArea.UnableToLoadReadme);

                Debug.WriteLine("--------" + fm.Archive);
                Debug.WriteLine("ReadmeRichTextBox.Load() failure:");
                Debug.WriteLine(e);
            }

            #endregion
        }

        #region Progress window

        internal void EnableEverything(bool enabled)
        {
            bool doFocus = !EverythingPanel.Enabled && enabled;

            EverythingPanel.Enabled = enabled;

            if (!doFocus) return;

            // The "mouse wheel scroll without needing to focus" thing stops working when no control is focused
            // (this happens when we disable and enable EverythingPanel). Therefore, we need to give focus to a
            // control here. One is as good as the next, but FMsDGV seems like a sensible choice.
            FMsDGV.Focus();
        }

        #region In

        internal void CancelScan() => Model.CancelScan();

        internal void CancelInstallFM() => Model.CancelInstallFM(GetSelectedFM());

        #endregion

        #endregion

        #region Messageboxes

        public bool AskToContinue(string message, string title)
        {
            var result = MessageBox.Show(message, title, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            return result == DialogResult.Yes;
        }

        public void ShowAlert(string message, string title)
        {
            MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        #endregion

        private void FilterTextBoxes_TextChanged(object sender, EventArgs e)
        {
            if (EventsDisabled) return;
            SetFilter();
        }

        private void FilterByGameCheckButtons_Click(object sender, EventArgs e)
        {
            if (EventsDisabled) return;
            SetFilter();
        }

        private void SaveCurrentTabSelectedFM(TabPage tabPage)
        {
            var gameSelFM =
                tabPage == Thief1TabPage ? FMsDGV.GameTabsState.T1SelFM :
                tabPage == Thief2TabPage ? FMsDGV.GameTabsState.T2SelFM :
                tabPage == Thief3TabPage ? FMsDGV.GameTabsState.T3SelFM :
                null;

            var gameFilter =
                tabPage == Thief1TabPage ? FMsDGV.GameTabsState.T1Filter :
                tabPage == Thief2TabPage ? FMsDGV.GameTabsState.T2Filter :
                tabPage == Thief3TabPage ? FMsDGV.GameTabsState.T3Filter :
                null;

            Debug.Assert(gameSelFM != null, "gameSelFM is null: Selected tab is not being handled");
            Debug.Assert(gameFilter != null, "gameFilter is null: Selected tab is not being handled");

            var selFM = GetSelectedFMPosInfo();

            selFM.DeepCopyTo(gameSelFM);
            FMsDGV.Filter.DeepCopyTo(gameFilter);
        }

        private void GamesTabControl_Deselecting(object sender, TabControlCancelEventArgs e)
        {
            if (EventsDisabled) return;
            if (GamesTabControl.Visible) SaveCurrentTabSelectedFM(e.TabPage);
        }

        private void GamesTabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (EventsDisabled) return;

            var s = GamesTabControl;

            var gameSelFM =
                s.SelectedTab == Thief1TabPage ? FMsDGV.GameTabsState.T1SelFM :
                s.SelectedTab == Thief2TabPage ? FMsDGV.GameTabsState.T2SelFM :
                s.SelectedTab == Thief3TabPage ? FMsDGV.GameTabsState.T3SelFM :
                null;

            var gameFilter =
                s.SelectedTab == Thief1TabPage ? FMsDGV.GameTabsState.T1Filter :
                s.SelectedTab == Thief2TabPage ? FMsDGV.GameTabsState.T2Filter :
                s.SelectedTab == Thief3TabPage ? FMsDGV.GameTabsState.T3Filter :
                null;

            Debug.Assert(gameSelFM != null, "gameSelFM is null: Selected tab is not being handled");
            Debug.Assert(gameFilter != null, "gameFilter is null: Selected tab is not being handled");

            FilterByThief1Button.Checked = gameSelFM == FMsDGV.GameTabsState.T1SelFM;
            FilterByThief2Button.Checked = gameSelFM == FMsDGV.GameTabsState.T2SelFM;
            FilterByThief3Button.Checked = gameSelFM == FMsDGV.GameTabsState.T3SelFM;

            gameSelFM.DeepCopyTo(FMsDGV.CurrentSelFM);
            gameFilter.DeepCopyTo(FMsDGV.Filter);

            SetUIFilterValues(gameFilter);

            InitialSelectedFMHasBeenSet = false;
            SetFilter();
        }

        private void CommentTextBox_TextChanged(object sender, EventArgs e)
        {
            if (EventsDisabled) return;
            var s = (TextBox)sender;

            if (FMsDGV.SelectedRows.Count == 0) return;

            var fm = GetSelectedFM();

            // Converting a multiline comment to single line:
            // DarkLoader copies up to the first linebreak or the 40 char mark, whichever comes first.
            // I'm doing the same, but bumping the cutoff point to 100 chars, which is still plenty fast.
            // fm.Comment.ToEscapes() is unbounded, but I measure tenths to hundredths of a millisecond even for
            // 25,000+ character strings with nothing but slashes and linebreaks in them.
            fm.Comment = s.Text.ToEscapes();
            fm.CommentSingleLine = s.Text.ToSingleLineComment(100);

            RefreshSelectedFMRowOnly();
        }

        private void CommentTextBox_Leave(object sender, EventArgs e)
        {
            if (EventsDisabled) return;
            Model.WriteFullFMDataIni();
        }

        private void ResetLayoutButton_Click(object sender, EventArgs e)
        {
            MainSplitContainer.ResetSplitterDistance();
            TopSplitContainer.ResetSplitterDistance();
        }

        #region Tags tab

        private void ShowAddTagDropDown()
        {
            var p = PointToClient(AddTagTextBox.PointToScreen(new Point(0, 0)));
            AddTagListBox.Location = new Point(p.X, p.Y + AddTagTextBox.Height);
            AddTagListBox.Size = new Size(Math.Max(AddTagTextBox.Width, 256), 225);

            AddTagListBox.BringToFront();
            AddTagListBox.Show();
        }

        private void HideAddTagDropDown()
        {
            AddTagListBox.Hide();
            AddTagListBox.Items.Clear();
        }

        // Robustness for if the user presses tab to get away, rather than clicking
        private void AddTagTextBoxOrListBox_Leave(object sender, EventArgs e)
        {
            if ((sender == AddTagTextBox && !AddTagListBox.Focused) ||
                (sender == AddTagListBox && !AddTagTextBox.Focused))
            {
                HideAddTagDropDown();
            }
        }

        private void AddTagTextBox_TextChanged(object sender, EventArgs e)
        {
            if (EventsDisabled) return;
            var s = (TextBox)sender;

            // Smartasses who try to break it get nothing
            if (s.Text.CountChars(':') > 1)
            {
                HideAddTagDropDown();
                return;
            }

            (string First, string Second) text;

            var index = s.Text.IndexOf(':');
            if (index > -1)
            {
                text.First = s.Text.Substring(0, index).Trim();
                text.Second = s.Text.Substring(index + 1).Trim();
            }
            else
            {
                text.First = s.Text.Trim();
                text.Second = "";
            }

            // Shut up, it works
            if (s.Text.IsWhiteSpace())
            {
                HideAddTagDropDown();
            }
            else
            {
                var list = new List<string>();

                foreach (var gCat in GlobalTags)
                {
                    if (gCat.Category.Name.ContainsI(text.First))
                    {
                        if (gCat.Tags.Count == 0)
                        {
                            if (gCat.Category.Name != "misc") list.Add(gCat.Category.Name + ":");
                        }
                        else
                        {
                            foreach (var gTag in gCat.Tags)
                            {
                                if (!text.Second.IsWhiteSpace() && !gTag.Name.ContainsI(text.Second)) continue;
                                if (gCat.Category.Name == "misc")
                                {
                                    if (text.Second.IsWhiteSpace() && !gCat.Category.Name.ContainsI(text.First))
                                    {
                                        list.Add(gTag.Name);
                                    }
                                }
                                else
                                {
                                    list.Add(gCat.Category.Name + ": " + gTag.Name);
                                }
                            }
                        }
                    }
                    // if, not else if - we want to display found tags both categorized and uncategorized
                    if (gCat.Category.Name == "misc")
                    {
                        foreach (var gTag in gCat.Tags)
                        {
                            if (gTag.Name.ContainsI(s.Text)) list.Add(gTag.Name);
                        }
                    }
                }

                list.Sort(StringComparer.OrdinalIgnoreCase);

                AddTagListBox.Items.Clear();
                foreach (var item in list) AddTagListBox.Items.Add(item);

                ShowAddTagDropDown();
            }
        }

        private void AddTagTextBoxOrListBox_KeyDown(object sender, KeyEventArgs e)
        {
            var box = AddTagListBox;

            var fm = GetSelectedFM();

            switch (e.KeyCode)
            {
                case Keys.Up when box.Items.Count > 0:
                    box.SelectedIndex =
                        box.SelectedIndex == -1 ? box.Items.Count - 1 :
                        box.SelectedIndex == 0 ? -1 :
                        box.SelectedIndex - 1;
                    e.Handled = true;
                    break;
                case Keys.Down when box.Items.Count > 0:
                    box.SelectedIndex =
                        box.SelectedIndex == -1 ? 0 :
                        box.SelectedIndex == box.Items.Count - 1 ? -1 :
                        box.SelectedIndex + 1;
                    e.Handled = true;
                    break;
                case Keys.Enter:
                    var catAndTag = box.SelectedIndex == -1 ? AddTagTextBox.Text : box.SelectedItem.ToString();
                    AddTagOperation(fm, catAndTag);
                    break;
                default:
                    if (sender == AddTagListBox) AddTagTextBox.Focus();
                    break;
            }
        }

        private void AddTagListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            var s = (ListBox)sender;
            if (s.SelectedIndex == -1) return;

            var tb = AddTagTextBox;

            using (new DisableEvents(this)) tb.Text = s.SelectedItem.ToString();

            if (tb.Text.Length > 0) tb.SelectionStart = tb.Text.Length;
        }

        private void RemoveTagFromFM()
        {
            var s = TagsTreeView;

            if (s.SelectedNode == null) return;

            var fm = GetSelectedFM();

            // Parent node (category)
            if (s.SelectedNode.Parent == null)
            {
                // TODO: Make this less annoying (unlocalized for now so it stands out)
                var result =
                    MessageBox.Show("Remove category?", "Tags", MessageBoxButtons.YesNo);
                if (result == DialogResult.No) return;

                var cat = fm.Tags.FirstOrDefault(x => x.Category == s.SelectedNode.Text);
                if (cat != null)
                {
                    fm.Tags.Remove(cat);
                    UpdateFMTagsString(fm);

                    // TODO: Profile the FirstOrDefaults and see if I should make them for loops
                    var globalCat = GlobalTags.FirstOrDefault(x => x.Category.Name == cat.Category);
                    if (globalCat != null && !globalCat.Category.IsPreset)
                    {
                        if (globalCat.Category.UsedCount > 0) globalCat.Category.UsedCount--;
                        if (globalCat.Category.UsedCount == 0) GlobalTags.Remove(globalCat);
                    }
                }
            }
            // Child node (tag)
            else
            {
                // TODO: See above
                var result =
                    MessageBox.Show("Remove tag?", "Tags", MessageBoxButtons.YesNo);
                if (result == DialogResult.No) return;

                var cat = fm.Tags.FirstOrDefault(x => x.Category == s.SelectedNode.Parent.Text);
                var tag = cat?.Tags.FirstOrDefault(x => x == s.SelectedNode.Text);
                if (tag != null)
                {
                    cat.Tags.Remove(tag);
                    if (cat.Tags.Count == 0) fm.Tags.Remove(cat);
                    UpdateFMTagsString(fm);

                    var globalCat = GlobalTags.FirstOrDefault(x => x.Category.Name == cat.Category);
                    var globalTag = globalCat?.Tags.FirstOrDefault(x => x.Name == s.SelectedNode.Text);
                    if (globalTag != null && !globalTag.IsPreset)
                    {
                        if (globalTag.UsedCount > 0) globalTag.UsedCount--;
                        if (globalTag.UsedCount == 0) globalCat.Tags.Remove(globalTag);
                    }
                }
            }

            DisplayFMTags(fm);
            Model.WriteFullFMDataIni();
        }

        private void RemoveTagButton_Click(object sender, EventArgs e)
        {
            RemoveTagFromFM();
        }

        private void AddTagListBox_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;

            var box = (ListBox)sender;

            if (box.SelectedIndex > -1) AddTagOperation(GetSelectedFM(), box.SelectedItem.ToString());
        }

        private void AddTagOperation(FanMission fm, string catAndTag)
        {
            // Cock-blocked here too
            if (AddTagTextBox.Text.CountChars(':') <= 1)
            {
                Model.AddTagsToFMAndGlobalList(catAndTag, fm.Tags);
                UpdateFMTagsString(fm);
                DisplayFMTags(fm);
                Model.WriteFullFMDataIni();
            }

            AddTagTextBox.Clear();
            HideAddTagDropDown();
        }

        private void AddTagButton_Click(object sender, EventArgs e)
        {
            var fm = GetSelectedFM();
            AddTagOperation(fm, AddTagTextBox.Text);
        }

        private void TagPresetsButton_Click(object sender, EventArgs e)
        {
            var button = (Button)sender;
            var menu = AddTagMenu;

            GlobalTags.SortCat();

            menu.Items.Clear();

            var addTagMenuItems = new List<ToolStripItem>();
            foreach (var catAndTag in GlobalTags)
            {
                if (catAndTag.Tags.Count == 0)
                {
                    var catItem = new ToolStripMenuItem(catAndTag.Category + ":");
                    catItem.Click += AddTagMenuEmptyItem_Click;
                    addTagMenuItems.Add(catItem);
                }
                else
                {
                    var catItem = new ToolStripMenuItem(catAndTag.Category.Name);
                    addTagMenuItems.Add(catItem);

                    var last = addTagMenuItems[addTagMenuItems.Count - 1];

                    if (catAndTag.Category.Name != "misc")
                    {
                        var customItem = new ToolStripMenuItem(LText.Global.CustomTagInCategory);
                        customItem.Click += AddTagMenuCustomItem_Click;
                        ((ToolStripMenuItem)last).DropDownItems.Add(customItem);
                        ((ToolStripMenuItem)last).DropDownItems.Add(new ToolStripSeparator());
                    }

                    foreach (var tag in catAndTag.Tags)
                    {
                        var tagItem = new ToolStripMenuItem(tag.Name);

                        if (catAndTag.Category.Name == "misc")
                        {
                            tagItem.Click += AddTagMenuMiscItem_Click;
                        }
                        else
                        {
                            tagItem.Click += AddTagMenuItem_Click;
                        }

                        ((ToolStripMenuItem)last).DropDownItems.Add(tagItem);
                    }
                }
            }

            AddTagMenu.Items.AddRange(addTagMenuItems.ToArray());

            menu.Show(button, -menu.Width, 0);
        }

        private void AddTagMenuItem_Click(object sender, EventArgs e)
        {
            var s = (ToolStripMenuItem)sender;
            if (s.HasDropDownItems) return;

            var cat = s.OwnerItem;
            if (cat == null) return;

            var catAndTag = cat.Text + @": " + s.Text;

            var fm = GetSelectedFM();
            AddTagOperation(fm, catAndTag);
        }

        private void AddTagMenuCustomItem_Click(object sender, EventArgs e)
        {
            var s = (ToolStripMenuItem)sender;

            var cat = s.OwnerItem;
            if (cat == null) return;

            AddTagTextBox.SetTextAndMoveCursorToEnd(cat.Text + @": ");
        }

        private void AddTagMenuMiscItem_Click(object sender, EventArgs e)
        {
            var s = (ToolStripMenuItem)sender;

            AddTagTextBox.SetTextAndMoveCursorToEnd(s.Text);
        }

        private void AddTagMenuEmptyItem_Click(object sender, EventArgs e)
        {
            var s = (ToolStripMenuItem)sender;

            AddTagTextBox.SetTextAndMoveCursorToEnd(s.Text + ' ');
        }

        // Just to keep things in a known state (clearing items also removes their event hookups, which is
        // convenient)
        private void AddTagMenu_Closed(object sender, ToolStripDropDownClosedEventArgs e) => AddTagMenu.Items.Clear();

        #endregion

        #region Readme

        #region Choose readme

        private void ChooseReadmeButton_Click(object sender, EventArgs e)
        {
            if (ChooseReadmeListBox.Items.Count == 0) return;

            var fm = GetSelectedFM();
            fm.SelectedReadme = ChooseReadmeListBox.SelectedBackingItem();
            ChooseReadmePanel.Hide();

            if (fm.SelectedReadme.ExtIsHtml())
            {
                ViewHTMLReadmeButton.Show();
            }
            else
            {
                ShowReadme(true);
            }

            RefreshSelectedFM(refreshReadme: true);
        }

        private void ChooseReadmeComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (EventsDisabled) return;
            var fm = GetSelectedFM();
            fm.SelectedReadme = ChooseReadmeComboBox.SelectedBackingItem();
            RefreshSelectedFM(refreshReadme: true);
        }

        private void ChooseReadmeComboBox_DropDownClosed(object sender, EventArgs e)
        {
            if (!CursorOverReadmeArea()) HideReadmeControls();
        }

        #endregion

        private void ReadmeRichTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyValue == '0') ReadmeRichTextBox.ResetZoomFactor();
        }

        private void ResetZoomButton_Click(object sender, EventArgs e) => ReadmeRichTextBox.ResetZoomFactor();

        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="IOException"></exception>
        private void ReadmeLoad(string path, ReadmeType readmeType)
        {
            #region Debug

            // Tells me whether a readme got reloaded more than once, which should never be allowed to happen
            // due to performance concerns.
            DebugLabel.Text = int.TryParse(DebugLabel.Text, out var result) ? (result + 1).ToString() : "1";

            #endregion

            if (readmeType == ReadmeType.HTML)
            {
                ViewHTMLReadmeButton.Show();
                ShowReadme(false);
                // In case the cursor is over the scroll bar area
                if (CursorOverReadmeArea()) ShowReadmeControls();
            }
            else
            {
                ShowReadme(true);
                ViewHTMLReadmeButton.Hide();
                var fileType = readmeType == ReadmeType.PlainText
                    ? RichTextBoxStreamType.PlainText
                    : RichTextBoxStreamType.RichText;

                ReadmeRichTextBox.LoadContent(path, fileType);
            }
        }

        private void ShowReadme(bool enabled)
        {
            ReadmeRichTextBox.Visible = enabled;
            ResetZoomButton.BackColor = enabled ? SystemColors.Window : SystemColors.Control;
            ReadmeFullScreenButton.BackColor = enabled ? SystemColors.Window : SystemColors.Control;
        }

        private void ShowReadmeControls()
        {
            ResetZoomButton.Show();
            ResetZoomButton.BringToFront();
            ReadmeFullScreenButton.Show();
            ReadmeFullScreenButton.BringToFront();
            // Visible check needed so items don't get selected with a mere mouseover
            if (ChooseReadmeComboBox.Items.Count > 0 && !ChooseReadmeComboBox.Visible)
            {
                ChooseReadmeComboBox.Show();
                ChooseReadmeComboBox.BringToFront();
            }
        }

        private void HideReadmeControls()
        {
            ResetZoomButton.Hide();
            ReadmeFullScreenButton.Hide();
            ChooseReadmeComboBox.Hide();
        }

        #endregion

        #region Edit FM tab

        private void EditFMAltTitlesDropDownButton_Click(object sender, EventArgs e)
        {
            var button = EditFMAltTitlesDropDownButton;
            var menu = AltTitlesMenu;

            menu.Show(button, -(menu.Width - button.Width), button.Height);
        }

        private void EditFMAltTitlesMenuItems_Click(object sender, EventArgs e)
        {
            EditFMTitleTextBox.Text = ((ToolStripMenuItem)sender).Text;
            Model.WriteFullFMDataIni();
        }

        private void EditFMTitleTextBox_TextChanged(object sender, EventArgs e)
        {
            if (EventsDisabled) return;
            var fm = GetSelectedFM();
            fm.Title = EditFMTitleTextBox.Text;
            RefreshSelectedFMRowOnly();
        }

        private void EditFMAuthorTextBox_TextChanged(object sender, EventArgs e)
        {
            if (EventsDisabled) return;
            var fm = GetSelectedFM();
            fm.Author = EditFMAuthorTextBox.Text;
            RefreshSelectedFMRowOnly();
        }

        private void EditFMAuthorTextBox_Leave(object sender, EventArgs e)
        {
            if (EventsDisabled) return;
            Model.WriteFullFMDataIni();
        }

        private void EditFMReleaseDateCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (EventsDisabled) return;
            EditFMReleaseDateDateTimePicker.Visible = EditFMReleaseDateCheckBox.Checked;

            var fm = GetSelectedFM();

            fm.ReleaseDate = EditFMReleaseDateCheckBox.Checked
                ? EditFMReleaseDateDateTimePicker.Value
                : (DateTime?)null;

            RefreshSelectedFMRowOnly();
            Model.WriteFullFMDataIni();
        }

        private void EditFMReleaseDateDateTimePicker_ValueChanged(object sender, EventArgs e)
        {
            if (EventsDisabled) return;
            var fm = GetSelectedFM();
            fm.ReleaseDate = EditFMReleaseDateDateTimePicker.Value;
            RefreshSelectedFMRowOnly();
            Model.WriteFullFMDataIni();
        }

        private void EditFMLastPlayedCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (EventsDisabled) return;
            EditFMLastPlayedDateTimePicker.Visible = EditFMLastPlayedCheckBox.Checked;

            var fm = GetSelectedFM();

            fm.LastPlayed = EditFMLastPlayedCheckBox.Checked
                ? EditFMLastPlayedDateTimePicker.Value
                : (DateTime?)null;

            RefreshSelectedFMRowOnly();
            Model.WriteFullFMDataIni();
        }

        private void EditFMLastPlayedDateTimePicker_ValueChanged(object sender, EventArgs e)
        {
            if (EventsDisabled) return;
            var fm = GetSelectedFM();
            fm.LastPlayed = EditFMLastPlayedDateTimePicker.Value;
            RefreshSelectedFMRowOnly();
            Model.WriteFullFMDataIni();
        }

        private void EditFMDisabledModsTextBox_TextChanged(object sender, EventArgs e)
        {
            if (EventsDisabled) return;
            var fm = GetSelectedFM();
            fm.DisabledMods = EditFMDisabledModsTextBox.Text;
            RefreshSelectedFMRowOnly();
        }

        private void EditFMDisabledModsTextBox_Leave(object sender, EventArgs e)
        {
            if (EventsDisabled) return;
            Model.WriteFullFMDataIni();
        }

        private void EditFMDisableAllModsCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (EventsDisabled) return;
            EditFMDisabledModsTextBox.Enabled = !EditFMDisableAllModsCheckBox.Checked;

            var fm = GetSelectedFM();
            fm.DisableAllMods = EditFMDisableAllModsCheckBox.Checked;
            RefreshSelectedFMRowOnly();
            Model.WriteFullFMDataIni();
        }

        private void EditFMRatingComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (EventsDisabled) return;
            var fm = GetSelectedFM();
            fm.Rating = EditFMRatingComboBox.SelectedIndex - 1;
            RefreshSelectedFMRowOnly();
            Model.WriteFullFMDataIni();
        }

        #endregion

        private void RatingRCMenuItems_Click(object sender, EventArgs e)
        {
            var fm = GetSelectedFM();

            for (int i = 0; i < RatingRCSubMenu.DropDownItems.Count; i++)
            {
                if (RatingRCSubMenu.DropDownItems[i] != sender) continue;

                fm.Rating = i - 1;
                RefreshSelectedFM(refreshReadme: false);
                Model.WriteFullFMDataIni();
                break;
            }
        }

        private void EditFMFinishedOnButton_Click(object sender, EventArgs e)
        {
            var button = EditFMFinishedOnButton;
            var menu = FinishedOnMenu;

            menu.Show(button, 0, button.Height);
        }

        private void FinishedOnMenuItems_Click(object sender, EventArgs e)
        {
            var fm = GetSelectedFM();

            fm.FinishedOn = 0;
            int at = 1;
            foreach (ToolStripMenuItem item in FinishedOnMenu.Items)
            {
                if (item.Checked) fm.FinishedOn |= at;
                at <<= 1;
            }

            RefreshSelectedFMRowOnly();
            Model.WriteFullFMDataIni();
        }

        private void ReadmeFullScreenButton_Click(object sender, EventArgs e) => MainSplitContainer.ToggleFullScreen();

        private void WebSearchButton_Click(object sender, EventArgs e)
        {
            var fm = GetSelectedFM();

            var url = Config.WebSearchUrl;
            var index = url.IndexOf("$TITLE$", StringComparison.OrdinalIgnoreCase);
            var finalUrl = Uri.EscapeUriString(index == -1
                ? url
                : url.Substring(0, index) + fm.Title + url.Substring(index + "$TITLE$".Length));

            if (!Uri.IsWellFormedUriString(finalUrl, UriKind.Absolute))
            {
                MessageBox.Show(LText.AlertMessages.WebSearchURLIsInvalid, LText.AlertMessages.Alert);
                return;
            }

            try
            {
                Process.Start(finalUrl);
            }
            catch (FileNotFoundException ex)
            {
                MessageBox.Show(LText.AlertMessages.WebSearchURLIsInvalid, LText.AlertMessages.Alert);
            }
            catch (Win32Exception ex)
            {
                // log it
            }
        }

        private void FiltersFlowLayoutPanel_SizeChanged(object sender, EventArgs e) => SetFilterBarScrollButtons();

        private void FiltersFlowLayoutPanel_Scroll(object sender, ScrollEventArgs e) => SetFilterBarScrollButtons();

        private void SetFilterBarScrollButtons()
        {
            var flp = FiltersFlowLayoutPanel;
            void ShowLeft()
            {
                FilterBarScrollLeftButton.Location = new Point(flp.Location.X, flp.Location.Y + 1);
                FilterBarScrollLeftButton.Show();
            }

            void ShowRight()
            {
                // Don't set it based on the filter bar width and location, otherwise it gets it slightly wrong
                // the first time
                FilterBarScrollRightButton.Location = new Point(
                    RefreshClearToolStripCustom.Location.X - FilterBarScrollRightButton.Width - 4,
                    flp.Location.Y + 1);
                FilterBarScrollRightButton.Show();
            }

            var hs = FiltersFlowLayoutPanel.HorizontalScroll;
            if (!hs.Visible)
            {
                FilterBarScrollLeftButton.Hide();
                FilterBarScrollRightButton.Hide();
            }
            // Keep order: Show, Hide
            // Otherwise there's a small hiccup with the buttons
            else if (hs.Value == 0)
            {
                ShowRight();
                FilterBarScrollLeftButton.Hide();
            }
            else if (hs.Value >= (hs.Maximum + 1) - hs.LargeChange)
            {
                ShowLeft();
                FilterBarScrollRightButton.Hide();
            }
            else
            {
                ShowLeft();
                ShowRight();
            }
        }

        private void FilterShowJunkCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (EventsDisabled) return;
            SetFilter();
        }

        private void FilterByFinishedButton_Click(object sender, EventArgs e) => SetFilter();

        private void FilterByUnfinishedButton_Click(object sender, EventArgs e) => SetFilter();

        private void FilterByRatingButton_Click(object sender, EventArgs e) => OpenFilterRating();

        private void FilterByTagsButton_Click(object sender, EventArgs e) => OpenFilterTags();

        private void FilterByReleaseDateButton_Click(object sender, EventArgs e) => OpenDateFilter(lastPlayed: false);

        private void FilterByLastPlayedButton_Click(object sender, EventArgs e) => OpenDateFilter(lastPlayed: true);

        private void OpenDateFilter(bool lastPlayed)
        {
            var button = lastPlayed ? FilterByLastPlayedButton : FilterByReleaseDateButton;
            var fromDate = lastPlayed ? FMsDGV.Filter.LastPlayedFrom : FMsDGV.Filter.ReleaseDateFrom;
            var toDate = lastPlayed ? FMsDGV.Filter.LastPlayedTo : FMsDGV.Filter.ReleaseDateTo;
            var title = lastPlayed ? LText.DateFilterBox.LastPlayedTitleText : LText.DateFilterBox.ReleaseDateTitleText;

            using (var f = new FilterDateForm(title, fromDate, toDate))
            {
                f.Location = FiltersFlowLayoutPanel.PointToScreen(new Point(
                    FilterIconButtonsToolStripCustom.Location.X + button.Bounds.X,
                    FilterIconButtonsToolStripCustom.Location.Y + button.Bounds.Y + button.Height));

                if (f.ShowDialog() != DialogResult.OK) return;

                if (lastPlayed)
                {
                    FMsDGV.Filter.SetLastPlayedFromAndTo(f.DateFrom, f.DateTo);
                }
                else
                {
                    FMsDGV.Filter.SetReleaseDateFromAndTo(f.DateFrom, f.DateTo);
                }

                button.Checked = f.DateFrom != null || f.DateTo != null;
            }

            UpdateDateLabel(lastPlayed);
            SetFilter();
        }

        private void UpdateDateLabel(bool lastPlayed)
        {
            var button = lastPlayed ? FilterByLastPlayedButton : FilterByReleaseDateButton;
            var fromDate = lastPlayed ? FMsDGV.Filter.LastPlayedFrom : FMsDGV.Filter.ReleaseDateFrom;
            var toDate = lastPlayed ? FMsDGV.Filter.LastPlayedTo : FMsDGV.Filter.ReleaseDateTo;
            var label = lastPlayed ? FilterByLastPlayedLabel : FilterByReleaseDateLabel;

            // Normally you can see the re-layout kind of "sequentially happen", this stops that and makes it
            // snappy
            FiltersFlowLayoutPanel.SuspendDrawing();
            try
            {
                if (button.Checked)
                {
                    var from = fromDate == null ? "" : fromDate.Value.ToShortDateString();
                    var to = toDate == null ? "" : toDate.Value.ToShortDateString();
                    label.Text = from + @" - " + to;
                    label.Visible = true;
                }
                else
                {
                    label.Visible = false;
                }
            }
            finally
            {
                FiltersFlowLayoutPanel.ResumeDrawing();
            }
        }

        private void OpenFilterRating()
        {
            var outOfFive = Config.RatingDisplayStyle == RatingDisplayStyle.FMSel;
            using (var f = new FilterRatingForm(FMsDGV.Filter.RatingFrom, FMsDGV.Filter.RatingTo, outOfFive))
            {
                f.Location =
                    FiltersFlowLayoutPanel.PointToScreen(new Point(
                        FilterIconButtonsToolStripCustom.Location.X +
                        FilterByRatingButton.Bounds.X,
                        FilterIconButtonsToolStripCustom.Location.Y +
                        FilterByRatingButton.Bounds.Y +
                        FilterByRatingButton.Height));

                if (f.ShowDialog() != DialogResult.OK) return;
                FMsDGV.Filter.SetRatingFromAndTo(f.RatingFrom, f.RatingTo);
                FilterByRatingButton.Checked =
                    !(FMsDGV.Filter.RatingFrom == -1 && FMsDGV.Filter.RatingTo == 10);
            }

            UpdateRatingLabel();
            SetFilter();
        }

        private void UpdateRatingLabel()
        {
            var s = FilterByRatingButton;

            // For snappy visual layout performance
            FiltersFlowLayoutPanel.SuspendDrawing();
            try
            {
                if (s.Checked)
                {
                    var ndl = Config.RatingDisplayStyle == RatingDisplayStyle.NewDarkLoader;
                    var rFrom = FMsDGV.Filter.RatingFrom;
                    var rTo = FMsDGV.Filter.RatingTo;
                    var curCulture = CultureInfo.CurrentCulture;

                    var from = rFrom == -1 ? LText.Global.None : (ndl ? rFrom : rFrom / 2.0).ToString(curCulture);
                    var to = rTo == -1 ? LText.Global.None : (ndl ? rTo : rTo / 2.0).ToString(curCulture);

                    FilterByRatingLabel.Text = from + @" - " + to;

                    FilterByRatingLabel.Visible = true;
                }
                else
                {
                    FilterByRatingLabel.Visible = false;
                }
            }
            finally
            {
                FiltersFlowLayoutPanel.ResumeDrawing();
            }
        }

        private void ClearFiltersButton_Click(object sender, EventArgs e) => ClearAllUIAndInternalFilters();

        private void ClearAllUIAndInternalFilters()
        {
            using (new DisableEvents(this))
            {
                bool oneList = Config.GameOrganization == GameOrganization.OneList;
                if (oneList)
                {
                    FilterByThief1Button.Checked = false;
                    FilterByThief2Button.Checked = false;
                    FilterByThief3Button.Checked = false;
                }
                FilterTitleTextBox.Text = "";
                FilterAuthorTextBox.Text = "";

                FilterByReleaseDateButton.Checked = false;
                FilterByReleaseDateLabel.Visible = false;

                FilterByLastPlayedButton.Checked = false;
                FilterByLastPlayedLabel.Visible = false;

                FilterByTagsButton.Checked = false;
                FilterByFinishedButton.Checked = false;
                FilterByUnfinishedButton.Checked = false;

                FilterByRatingButton.Checked = false;
                FilterByRatingLabel.Visible = false;

                FilterShowJunkCheckBox.Checked = false;
                FMsDGV.Filter.Clear(oneList);
            }

            SetFilter();
        }

        private void RefreshFiltersButton_Click(object sender, EventArgs e) => SetFilter();

        private void ViewHTMLReadmeButton_Click(object sender, EventArgs e)
        {
            string path;
            try
            {
                (path, _) = Model.GetReadmeFileAndType(GetSelectedFM());
            }
            catch (Exception ex)
            {
                // log it
                return;
            }

            if (File.Exists(path))
            {
                Process.Start(path);
            }
            else
            {
                // log it
            }
        }

        private void ImportButton_Click(object sender, EventArgs e)
        {
            var menu = ImportFromMenu;
            var button = ImportButton;
            menu.Show(button, -(menu.Width - button.Width), -menu.Height);
        }

        private async void ImportFromDarkLoaderMenuItem_Click(object sender, EventArgs e)
        {
            using (var f = new ImportFromDarkLoaderForm(Model))
            {
                if (f.ShowDialog() != DialogResult.OK) return;

                if (f.ImportedFMs.Count == 0) return;

                var fms = new List<FanMission>();
                fms.AddRange(f.ImportedFMs);

                // Don't bother deep-copying fms so we can carry it out of the using block, because the merge
                // constitutes a deep copy anyway
                Model.MergeDarkLoaderFMData(fms);
            }

            // DarkLoader might have the wrong game type or no game type, so scan for that
            await ScanNewFMsForGameType();
            Model.WriteFullFMDataIni();
        }

        private void ImportFromFMSelMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void ImportFromNewDarkLoaderMenuItem_Click(object sender, EventArgs e)
        {

        }
    }
}
