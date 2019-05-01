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
using AngelLoader.Common.DataClasses;
using AngelLoader.Common.Utility;
using AngelLoader.Importing;
using AngelLoader.Properties;
using AngelLoader.WinAPI;
using FMScanner;
using Gma.System.MouseKeyHook;
using Ookii.Dialogs.WinForms;
using static AngelLoader.Common.Common;
using static AngelLoader.Common.Logger;
using static AngelLoader.Common.Utility.Methods;

namespace AngelLoader.Forms
{
    public partial class MainForm : Form, IView, IEventDisabler, IKeyPressDisabler, IMessageFilter
    {
        public object InvokeSync(Delegate method) => Invoke(method);
        public object InvokeSync(Delegate method, params object[] args) => Invoke(method, args);
        public object InvokeAsync(Delegate method) => BeginInvoke(method);
        public object InvokeAsync(Delegate method, params object[] args) => BeginInvoke(method, args);

        #region Private fields

        private FormWindowState NominalWindowState;
        private Size NominalWindowSize;
        private Point NominalWindowLocation;

        private float FMsListDefaultFontSizeInPoints;
        private int FMsListDefaultRowHeight;

        private enum ZoomFMsDGVType
        {
            ZoomIn,
            ZoomOut,
            ResetZoom,
            ZoomTo,
            ZoomToHeightOnly
        }

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
        private Bitmap FinishedOnUnknownIcon;

        #endregion

        private DataGridViewImageColumn RatingImageColumn;

        private bool InitialSelectedFMHasBeenSet;

        public bool EventsDisabled { get; set; }

        // Needed for Rating column swap to prevent a possible exception when CellValueNeeded is called in the
        // middle of the operation
        private bool CellValueNeededDisabled;
        // Needed for zooming to prevent Config column widths from being set in the zoom methods
        private bool ColumnWidthSaveDisabled;

        public bool KeyPressesDisabled { get; set; }

        #endregion

        public int CurrentSortedColumnIndex => FMsDGV.CurrentSortedColumn;
        public SortOrder CurrentSortDirection => FMsDGV.CurrentSortDirection;

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
                int wParam = (int)m.WParam;
                if (hWnd != IntPtr.Zero && Control.FromHandle(hWnd) != null)
                {
                    if (CursorOverControl(FiltersFlowLayoutPanel) && !CursorOverControl(FMsDGV))
                    {
                        // Allow the filter bar to be mousewheel-scrolled with the buttons properly appearing
                        // and disappearing as appropriate
                        if (wParam != 0)
                        {
                            int direction = wParam > 0 ? InteropMisc.SB_LINELEFT : InteropMisc.SB_LINERIGHT;
                            int origSmallChange = FiltersFlowLayoutPanel.HorizontalScroll.SmallChange;

                            FiltersFlowLayoutPanel.HorizontalScroll.SmallChange = 45;

                            InteropMisc.SendMessage(FiltersFlowLayoutPanel.Handle, InteropMisc.WM_SCROLL, (IntPtr)direction, IntPtr.Zero);

                            FiltersFlowLayoutPanel.HorizontalScroll.SmallChange = origSmallChange;
                        }
                    }
                    else if (CursorOverControl(FMsDGV) && (wParam & 0xFFFF) == InteropMisc.MK_CONTROL)
                    {
                        var delta = wParam >> 16;
                        if (delta > 0)
                        {
                            ZoomFMsDGV(ZoomFMsDGVType.ZoomIn);
                        }
                        else if (delta < 0)
                        {
                            ZoomFMsDGV(ZoomFMsDGVType.ZoomOut);
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
            // CanFocus will be false if there are modal windows open
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

            ShowReadmeControls(CursorOverReadmeArea());
        }

        #endregion

        public void Block(bool block) => this.BlockWindow(block);

        // Put anything that does anything in here, not in the constructor. Otherwise it's a world of pain and
        // screwy behavior cascading outwards and messing with everything it touches. Don't do it.
        public void Init()
        {
#if ReleaseBeta
            var ver = typeof(MainForm).Assembly.GetName().Version;
            var verThird = ver.Build > 0 ? @"." + ver.Build : "";
            Text = @"AngelLoader " + ver.Major + @"." + ver.Minor + verThird;
            base.Text += " " + Application.ProductVersion;
#else
            Text = @"AngelLoader " + Application.ProductVersion;
#endif

#if Release && !Release_Testing
            DebugLabel.Hide();
            DebugLabel2.Hide();
            TestButton.Hide();
            Test2Button.Hide();
#endif

            AppMouseHook = Hook.AppEvents();
            AppMouseHook.MouseDownExt += HookMouseDown;
            AppMouseHook.MouseUpExt += HookMouseUp;
            AppMouseHook.MouseMoveExt += HookMouseMove;
            Application.AddMessageFilter(this);

            // Aside from a possible OpenSettings() call in Model.Init() if it needs to throw up the Settings
            // window (which doesn't show the view, so the startup process is still left intact), this code is
            // now a nice straight line with no back-and-forth spaghetti method calls.

            Core.ProgressBox = ProgressBox;

            #region Set up form and control state

            // TODO: Stop hiding this when we get the feature in fully; this is for release
            PlayFMAdvancedMenuItem.Visible = false;

            FMsListDefaultFontSizeInPoints = FMsDGV.DefaultCellStyle.Font.SizeInPoints;
            FMsListDefaultRowHeight = FMsDGV.RowTemplate.Height;

            // Allows shortcut keys to be detected globally (selected control doesn't affect them)
            KeyPreview = true;

            var indexes = new SortedDictionary<int, TabPage>
            {
                { Config.TopRightTabOrder.StatsTabPosition, StatisticsTabPage },
                { Config.TopRightTabOrder.EditFMTabPosition, EditFMTabPage },
                { Config.TopRightTabOrder.CommentTabPosition, CommentTabPage },
                { Config.TopRightTabOrder.TagsTabPosition, TagsTabPage },
                { Config.TopRightTabOrder.PatchTabPosition, PatchTabPage }
            };

            TopRightTabControl.TabPages.Clear();
            foreach (var item in indexes) TopRightTabControl.TabPages.Add(item.Value);

            #region SplitContainers

            MainSplitContainer.SetSplitterPercent(Config.MainSplitterPercent, suspendResume: false);
            TopSplitContainer.SetSplitterPercent(Config.TopSplitterPercent, suspendResume: false);

            MainSplitContainer.InjectSibling(TopSplitContainer);
            TopSplitContainer.InjectSibling(MainSplitContainer);

            #endregion

            #region Columns

            FinishedColumn.Width = FinishedColumnWidth;

            // The other Rating column, there has to be two, one for text and one for images
            RatingImageColumn = new DataGridViewImageColumn
            {
                HeaderText = LText.FMsList.RatingColumn,
                ImageLayout = DataGridViewImageCellLayout.Zoom,
                MinimumWidth = Defaults.MinColumnWidth,
                Name = "RatingImageColumn",
                ReadOnly = true,
                Width = RatingImageColumnWidth,
                Resizable = DataGridViewTriState.False,
                SortMode = DataGridViewColumnSortMode.Programmatic
            };

            UpdateRatingLists(Config.RatingDisplayStyle == RatingDisplayStyle.FMSel);
            UpdateRatingColumn(startup: true);

            FMsDGV.FillColumns(Config.Columns);

            #endregion

            #region Readme

            ReadmeRichTextBox.InjectParent(this);

            // Set both at once to avoid an elusive bug that happens when you start up, the readme is blank, then
            // you shut down without loading a readme, whereupon it will save out ZoomFactor which is still 1.0.
            // You can't just save out StoredZoomFactor either because it doesn't change when the user zooms, as
            // there's no event for that. Fun.
            ReadmeRichTextBox.StoredZoomFactor = Config.ReadmeZoomFactor;
            ReadmeRichTextBox.ZoomFactor = ReadmeRichTextBox.StoredZoomFactor;

            #endregion

            ProgressBox.Inject(this);

            #region Filters

            FiltersFlowLayoutPanel.HorizontalScroll.SmallChange = 20;

            // Set these to invisible here, because if you do it on the UI, it constantly marks the form file as
            // unsaved every time you open it even though no changes were made. Aggravating as hell. ToolStrips
            // march to the beat of their own drum, that's for sure. :\
            FilterByReleaseDateLabel.Visible = false;
            FilterByLastPlayedLabel.Visible = false;
            FilterByRatingLabel.Visible = false;

            Config.Filter.DeepCopyTo(FMsDGV.Filter);
            SetUIFilterValues(FMsDGV.Filter);

            #endregion

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

            FinishedOnMenu.SetPreventCloseOnClickItems(FinishedOnNormalMenuItem, FinishedOnHardMenuItem,
                FinishedOnExpertMenuItem, FinishedOnExtremeMenuItem, FinishedOnUnknownMenuItem);

            // Cheap 'n cheesy storage of initial size for minimum-width setting later
            EditFMFinishedOnButton.Tag = EditFMFinishedOnButton.Size;
            ChooseReadmeButton.Tag = ChooseReadmeButton.Size;

            TopRightTabControl.SelectedTab =
                Config.TopRightTab == TopRightTab.EditFM ? EditFMTabPage :
                Config.TopRightTab == TopRightTab.Comment ? CommentTabPage :
                Config.TopRightTab == TopRightTab.Tags ? TagsTabPage :
                Config.TopRightTab == TopRightTab.Patch ? PatchTabPage :
                StatisticsTabPage;

            InstallUninstallFMButton.Visible = !Config.HideUninstallButton;
            ShowFMsListZoomButtons(!Config.HideFMListZoomButtons);

            ChangeGameOrganization();

            #endregion

            // Set here so as to avoid the changes being visible
            SetWindowStateAndSize();

            TopSplitContainer.CollapsedSize = TopRightCollapseButton.Width;
            if (Config.TopRightPanelCollapsed)
            {
                TopSplitContainer.SetFullScreen(true, suspendResume: false);
                SetTopRightCollapsedState();
            }

            // Set these here because they depend on the splitter positions
            SetUITextToLocalized(suspendResume: false);
            ChooseReadmePanel.CenterHV(MainSplitContainer.Panel2);

            SortFMsDGV(Config.SortedColumn, Config.SortDirection);
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            ZoomFMsDGV(ZoomFMsDGVType.ZoomToHeightOnly, Config.FMsListFontSizeInPoints);
        }

        private async void MainForm_Shown(object sender, EventArgs e)
        {
            // This must certainly need to come after Show() as well, right?!
            // This await call takes 15ms just to make the call alone(?!) so don't do it unless we have to
            if (Core.ViewListGamesNull.Count > 0) await Core.ScanNewFMsForGameType(useViewListGamesNull: true);

            // This must come after Show() because of possible FM caching needing to put up ProgressBox... etc.
            // Don't do Suspend/ResumeDrawing on startup because resume is slowish (having a refresh and all)
            // TODO: Put this before Show() again and just have the cacher show the form if needed
            await SetFilter(suppressSuspendResume: true);
        }

        private void SetWindowStateAndSize()
        {
            // Size MUST come first, otherwise it doesn't take (and then you have to put it in _Load, where it
            // can potentially be seen being changed)
            Size = Config.MainWindowSize;
            WindowState = Config.MainWindowState;

            const int minVisible = 200;

            var loc = new Point(Config.MainWindowLocation.X, Config.MainWindowLocation.Y);
            var bounds = Screen.FromControl(this).Bounds;

            if (loc.X < bounds.Left - (Width - minVisible) || loc.X > bounds.Right - minVisible)
            {
                loc.X = Defaults.MainWindowX;
            }
            if (loc.Y < bounds.Top - (Height - minVisible) || loc.Y > bounds.Bottom - minVisible)
            {
                loc.Y = Defaults.MainWindowY;
            }

            Location = new Point(loc.X, loc.Y);

            NominalWindowState = Config.MainWindowState;
            NominalWindowSize = Config.MainWindowSize;
            NominalWindowLocation = new Point(loc.X, loc.Y);
        }

        private void SetUIFilterValues(Filter filter)
        {
            using (new DisableEvents(this))
            {
                FiltersFlowLayoutPanel.SuspendDrawing();
                try
                {
                    FilterTitleTextBox.Text = filter.Title;
                    FilterAuthorTextBox.Text = filter.Author;
                    FilterShowJunkCheckBox.Checked = filter.ShowJunk;

                    FilterByTagsButton.Checked = !filter.Tags.Empty();

                    FilterByFinishedButton.Checked = filter.Finished.Contains(FinishedState.Finished);
                    FilterByUnfinishedButton.Checked = filter.Finished.Contains(FinishedState.Unfinished);

                    FilterByRatingButton.Checked = !(filter.RatingFrom == -1 && filter.RatingTo == 10);
                    UpdateRatingLabel(suspendResume: false);

                    FilterByReleaseDateButton.Checked = filter.ReleaseDateFrom != null || filter.ReleaseDateTo != null;
                    UpdateDateLabel(lastPlayed: false, suspendResume: false);

                    FilterByLastPlayedButton.Checked = filter.LastPlayedFrom != null || filter.LastPlayedTo != null;
                    UpdateDateLabel(lastPlayed: true, suspendResume: false);
                }
                finally
                {
                    FiltersFlowLayoutPanel.ResumeDrawing();
                }
            }
        }

        public void SetUITextToLocalized(bool suspendResume = true)
        {
            // Certain controls' text depends on FM state. Because this could be run after startup, we need to
            // make sure those controls' text is set correctly.
            var selFM = FMsDGV.SelectedRows.Count > 0 ? GetSelectedFM() : null;
            bool sayInstall = selFM == null || !selFM.Installed;

            if (suspendResume)
            {
                this.SuspendDrawing();
            }
            else
            {
                BottomLeftButtonsFlowLayoutPanel.SuspendLayout();
                BottomRightButtonsFlowLayoutPanel.SuspendLayout();
                StatsCheckBoxesPanel.SuspendLayout();
                EditFMTabPage.SuspendLayout();
                CommentTabPage.SuspendLayout();
                TagsTabPage.SuspendLayout();
                MainSplitContainer.Panel2.SuspendLayout();
                ChooseReadmePanel.SuspendLayout();
            }
            try
            {
                #region Game tabs

                Thief1TabPage.Text = LText.GameTabs.Thief1;
                Thief2TabPage.Text = LText.GameTabs.Thief2;
                Thief3TabPage.Text = LText.GameTabs.Thief3;

                // Prevents the couple-pixel-high tab page from extending out too far and becoming visible
                var lastGameTabsRect = GamesTabControl.GetTabRect(GamesTabControl.TabCount - 1);
                GamesTabControl.Width = lastGameTabsRect.X + lastGameTabsRect.Width + 5;

                #endregion

                #region Filter bar

                if (Config.GameOrganization == GameOrganization.ByTab) PositionFilterBarAfterTabs();

                FilterByThief1Button.ToolTipText = LText.FilterBar.Thief1ToolTip;
                FilterByThief2Button.ToolTipText = LText.FilterBar.Thief2ToolTip;
                FilterByThief3Button.ToolTipText = LText.FilterBar.Thief3ToolTip;

                FilterTitleLabel.Text = LText.FilterBar.Title;
                FilterAuthorLabel.Text = LText.FilterBar.Author;

                FilterByReleaseDateButton.ToolTipText = LText.FilterBar.ReleaseDateToolTip;
                FilterByReleaseDateLabel.ToolTipText = LText.FilterBar.ReleaseDateToolTip;

                FilterByLastPlayedButton.ToolTipText = LText.FilterBar.LastPlayedToolTip;
                FilterByLastPlayedLabel.ToolTipText = LText.FilterBar.LastPlayedToolTip;

                FilterByTagsButton.ToolTipText = LText.FilterBar.TagsToolTip;
                FilterByFinishedButton.ToolTipText = LText.FilterBar.FinishedToolTip;
                FilterByUnfinishedButton.ToolTipText = LText.FilterBar.UnfinishedToolTip;

                FilterByRatingButton.ToolTipText = LText.FilterBar.RatingToolTip;
                FilterByRatingLabel.ToolTipText = LText.FilterBar.RatingToolTip;

                FilterShowJunkCheckBox.Text = LText.FilterBar.ShowJunk;

                #endregion

                #region Clear/refresh/reset area

                RefreshFromDiskButton.ToolTipText = LText.FilterBar.RefreshFromDiskButtonToolTip;
                RefreshFiltersButton.ToolTipText = LText.FilterBar.RefreshFilteredListButtonToolTip;
                ClearFiltersButton.ToolTipText = LText.FilterBar.ClearFiltersButtonToolTip;
                MainToolTip.SetToolTip(ResetLayoutButton, LText.FilterBar.ResetLayoutButtonToolTip);

                #endregion

                #region FMs list

                FMsDGV.SetUITextToLocalized();

                FMsListZoomInButton.ToolTipText = LText.FMsList.ZoomInToolTip;
                FMsListZoomOutButton.ToolTipText = LText.FMsList.ZoomOutToolTip;
                FMsListResetZoomButton.ToolTipText = LText.FMsList.ResetZoomToolTip;

                #region Columns

                GameTypeColumn.HeaderText = LText.FMsList.GameColumn;
                InstalledColumn.HeaderText = LText.FMsList.InstalledColumn;
                TitleColumn.HeaderText = LText.FMsList.TitleColumn;
                ArchiveColumn.HeaderText = LText.FMsList.ArchiveColumn;
                AuthorColumn.HeaderText = LText.FMsList.AuthorColumn;
                SizeColumn.HeaderText = LText.FMsList.SizeColumn;
                RatingTextColumn.HeaderText = LText.FMsList.RatingColumn;
                RatingImageColumn.HeaderText = LText.FMsList.RatingColumn;
                FinishedColumn.HeaderText = LText.FMsList.FinishedColumn;
                ReleaseDateColumn.HeaderText = LText.FMsList.ReleaseDateColumn;
                LastPlayedColumn.HeaderText = LText.FMsList.LastPlayedColumn;
                DisabledModsColumn.HeaderText = LText.FMsList.DisabledModsColumn;
                CommentColumn.HeaderText = LText.FMsList.CommentColumn;

                #endregion

                #region FM context menu

                PlayFMMenuItem.Text = LText.FMsList.FMMenu_PlayFM;

                //PlayFMAdvancedMenuItem.Text = LText.FMsList.FMMenu_PlayFMAdvanced;
                Core.SetDefaultConfigVarNamesToLocalized();

                InstallUninstallMenuItem.Text = sayInstall
                    ? LText.FMsList.FMMenu_InstallFM
                    : LText.FMsList.FMMenu_UninstallFM;

                OpenInDromEdMenuItem.Text = LText.FMsList.FMMenu_OpenInDromEd;

                ScanFMMenuItem.Text = LText.FMsList.FMMenu_ScanFM;

                ConvertAudioRCSubMenu.Text = LText.FMsList.FMMenu_ConvertAudio;
                ConvertWAVsTo16BitMenuItem.Text = LText.FMsList.ConvertAudioMenu_ConvertWAVsTo16Bit;
                ConvertOGGsToWAVsToolStripMenuItem.Text = LText.FMsList.ConvertAudioMenu_ConvertOGGsToWAVs;

                RatingRCSubMenu.Text = LText.FMsList.FMMenu_Rating;
                RatingRCMenuUnrated.Text = LText.Global.Unrated;

                FinishedOnRCSubMenu.Text = LText.FMsList.FMMenu_FinishedOn;

                WebSearchMenuItem.Text = LText.FMsList.FMMenu_WebSearch;

                #endregion

                #region Finished On menu

                var fmIsT3 = selFM != null && selFM.Game == Game.Thief3;

                FinishedOnNormalMenuItem.Text = fmIsT3 ? LText.Difficulties.Easy : LText.Difficulties.Normal;
                FinishedOnHardMenuItem.Text = fmIsT3 ? LText.Difficulties.Normal : LText.Difficulties.Hard;
                FinishedOnExpertMenuItem.Text = fmIsT3 ? LText.Difficulties.Hard : LText.Difficulties.Expert;
                FinishedOnExtremeMenuItem.Text = fmIsT3 ? LText.Difficulties.Expert : LText.Difficulties.Extreme;
                FinishedOnUnknownMenuItem.Text = LText.Difficulties.Unknown;

                #endregion

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

                StatsScanCustomResourcesButton.SetTextAutoSize(LText.StatisticsTab.RescanCustomResources);

                #endregion

                #region Edit FM tab

                EditFMTabPage.Text = LText.EditFMTab.TabText;
                EditFMTitleLabel.Text = LText.EditFMTab.Title;
                EditFMAuthorLabel.Text = LText.EditFMTab.Author;
                EditFMReleaseDateCheckBox.Text = LText.EditFMTab.ReleaseDate;
                EditFMLastPlayedCheckBox.Text = LText.EditFMTab.LastPlayed;
                EditFMRatingLabel.Text = LText.EditFMTab.Rating;

                // For some reason this counts as a selected index change?!
                using (new DisableEvents(this)) EditFMRatingComboBox.Items[0] = LText.Global.Unrated;

                EditFMFinishedOnButton.SetTextAutoSize(LText.EditFMTab.FinishedOn, ((Size)EditFMFinishedOnButton.Tag).Width);
                EditFMDisabledModsLabel.Text = LText.EditFMTab.DisabledMods;
                EditFMDisableAllModsCheckBox.Text = LText.EditFMTab.DisableAllMods;

                MainToolTip.SetToolTip(EditFMScanTitleButton, LText.EditFMTab.RescanTitleToolTip);
                MainToolTip.SetToolTip(EditFMScanAuthorButton, LText.EditFMTab.RescanAuthorToolTip);
                MainToolTip.SetToolTip(EditFMScanReleaseDateButton, LText.EditFMTab.RescanReleaseDateToolTip);

                EditFMScanForReadmesButton.SetTextAutoSize(LText.EditFMTab.RescanForReadmes);

                #endregion

                #region Comment tab

                CommentTabPage.Text = LText.CommentTab.TabText;

                #endregion

                #region Tags tab

                TagsTabPage.Text = LText.TagsTab.TabText;
                AddTagButton.SetTextAutoSize(AddTagTextBox, LText.TagsTab.AddTag);
                AddTagFromListButton.SetTextAutoSize(LText.TagsTab.AddFromList);
                RemoveTagButton.SetTextAutoSize(LText.TagsTab.RemoveTag);

                #endregion

                #region Patch tab

                PatchTabPage.Text = LText.PatchTab.TabText;
                PatchDMLPatchesLabel.Text = LText.PatchTab.DMLPatchesApplied;
                MainToolTip.SetToolTip(PatchAddDMLButton, LText.PatchTab.AddDMLPatchToolTip);
                MainToolTip.SetToolTip(PatchRemoveDMLButton, LText.PatchTab.RemoveDMLPatchToolTip);
                PatchFMNotInstalledLabel.Text = LText.PatchTab.FMNotInstalled;
                PatchFMNotInstalledLabel.CenterHV(PatchTabPage);
                PatchOpenFMFolderButton.SetTextAutoSize(LText.PatchTab.OpenFMFolder, PatchOpenFMFolderButton.Width);

                #endregion

                #region Readme area

                MainToolTip.SetToolTip(ZoomInButton, LText.ReadmeArea.ZoomInToolTip);
                MainToolTip.SetToolTip(ZoomOutButton, LText.ReadmeArea.ZoomOutToolTip);
                MainToolTip.SetToolTip(ResetZoomButton, LText.ReadmeArea.ResetZoomToolTip);
                MainToolTip.SetToolTip(ReadmeFullScreenButton, LText.ReadmeArea.FullScreenToolTip);

                ViewHTMLReadmeButton.SetTextAutoSize(LText.ReadmeArea.ViewHTMLReadme);

                ChooseReadmeButton.SetTextAutoSize(LText.Global.OK, ((Size)ChooseReadmeButton.Tag).Width);

                #endregion

                #region Bottom area

                PlayFMButton.SetTextAutoSize(LText.MainButtons.PlayFM, preserveHeight: true);

                #region Install / Uninstall FM button

                // Special-case this button to always be the width of the longer of the two localized strings for
                // "Install" and "Uninstall" so it doesn't resize when its text changes. (visual nicety)
                InstallUninstallFMButton.SuspendDrawing();

                var instString = LText.MainButtons.InstallFM;
                var uninstString = LText.MainButtons.UninstallFM;
                var instButtonFont = InstallUninstallFMButton.Font;
                var instStringWidth = TextRenderer.MeasureText(instString, instButtonFont).Width;
                var uninstStringWidth = TextRenderer.MeasureText(uninstString, instButtonFont).Width;
                var longestString = instStringWidth > uninstStringWidth ? instString : uninstString;

                InstallUninstallFMButton.SetTextAutoSize(longestString, preserveHeight: true);

                InstallUninstallFMButton.Text = sayInstall
                    ? LText.MainButtons.InstallFM
                    : LText.MainButtons.UninstallFM;
                InstallUninstallFMButton.Image = sayInstall
                    ? Resources.Install_24
                    : Resources.Uninstall_24;

                InstallUninstallFMButton.ResumeDrawing();

                #endregion

                PlayOriginalGameButton.SetTextAutoSize(LText.MainButtons.PlayOriginalGame, preserveHeight: true);
                WebSearchButton.SetTextAutoSize(LText.MainButtons.WebSearch, preserveHeight: true);
                ScanAllFMsButton.SetTextAutoSize(LText.MainButtons.ScanAllFMs, preserveHeight: true);
                ImportButton.SetTextAutoSize(LText.MainButtons.Import, preserveHeight: true);
                SettingsButton.SetTextAutoSize(LText.MainButtons.Settings, preserveHeight: true);

                #endregion

                ProgressBox.SetUITextToLocalized();

                SetFMSizesToLocalized();
            }
            finally
            {
                if (suspendResume)
                {
                    this.ResumeDrawing();
                }
                else
                {
                    BottomLeftButtonsFlowLayoutPanel.ResumeLayout();
                    BottomRightButtonsFlowLayoutPanel.ResumeLayout();
                    StatsCheckBoxesPanel.ResumeLayout();
                    EditFMTabPage.ResumeLayout();
                    CommentTabPage.ResumeLayout();
                    TagsTabPage.ResumeLayout();
                    MainSplitContainer.Panel2.ResumeLayout();
                    ChooseReadmePanel.ResumeLayout();
                }

                // We can't do this while the layout is suspended, because then it won't have the right dimensions
                // for centering
                ViewHTMLReadmeButton.CenterHV(MainSplitContainer.Panel2);
            }

            // To refresh the FM size column strings to localized
            // Quick hack: the only time we pass suspendResume = false is on startup, and we don't need to refresh
            // then because we already will later
            if (suspendResume) RefreshFMsListKeepSelection();
        }

        public void ShowInstallUninstallButton(bool enabled) => InstallUninstallFMButton.Visible = enabled;

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
                if (WindowState != FormWindowState.Maximized)
                {
                    NominalWindowSize = Size;
                    NominalWindowLocation = new Point(Location.X, Location.Y);
                }
            }

            if (ProgressBox.Visible) ProgressBox.Center();
            if (AddTagListBox.Visible) HideAddTagDropDown();

            SetFilterBarScrollButtons();
        }

        private void MainForm_LocationChanged(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Normal)
            {
                NominalWindowLocation = new Point(Location.X, Location.Y);
            }
        }

        private async void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (KeyPressesDisabled)
            {
                e.SuppressKeyPress = true;
                return;
            }

            if (e.KeyCode == Keys.Enter)
            {
                if (FMsDGV.Focused && FMsDGV.SelectedRows.Count > 0 && GameIsKnownAndSupported(GetSelectedFM()))
                {
                    e.SuppressKeyPress = true;
                    await InstallAndPlay.InstallOrPlay(GetSelectedFM(), askConfIfRequired: true);
                }
            }
            else if (e.KeyCode == Keys.Escape)
            {
                FMsDGV.CancelColumnResize();
                MainSplitContainer.CancelResize();
                TopSplitContainer.CancelResize();

                HideAddTagDropDown();
            }

            else if (e.Control)
            {
                if (e.KeyCode == Keys.F)
                {
                    FilterTitleTextBox.Focus();
                    FilterTitleTextBox.SelectAll();
                }
                else if (e.KeyCode == Keys.Add || e.KeyCode == Keys.Oemplus)
                {
                    if ((ReadmeRichTextBox.Focused && !CursorOverControl(FMsDGV)) || CursorOverReadmeArea())
                    {
                        ReadmeRichTextBox.ZoomIn();
                    }
                    else if ((FMsDGV.Focused && !CursorOverReadmeArea()) || CursorOverControl(FMsDGV))
                    {
                        ZoomFMsDGV(ZoomFMsDGVType.ZoomIn);
                    }
                }
                else if (e.KeyCode == Keys.Subtract || e.KeyCode == Keys.OemMinus)
                {
                    if ((ReadmeRichTextBox.Focused && !CursorOverControl(FMsDGV)) || CursorOverReadmeArea())
                    {
                        ReadmeRichTextBox.ZoomOut();
                    }
                    else if ((FMsDGV.Focused && !CursorOverReadmeArea()) || CursorOverControl(FMsDGV))
                    {
                        ZoomFMsDGV(ZoomFMsDGVType.ZoomOut);
                    }
                }
                else if (e.KeyCode == Keys.D0 || e.KeyCode == Keys.NumPad0)
                {
                    if ((ReadmeRichTextBox.Focused && !CursorOverControl(FMsDGV)) || CursorOverReadmeArea())
                    {
                        ReadmeRichTextBox.ResetZoomFactor();
                    }
                    else if ((FMsDGV.Focused && !CursorOverReadmeArea()) || CursorOverControl(FMsDGV))
                    {
                        ZoomFMsDGV(ZoomFMsDGVType.ResetZoom);
                    }
                }
            }
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Extremely cheap and cheesy, but otherwise I have to figure out how to wait for a completely
            // separate and detached thread to complete. Argh. Threading sucks.
            if (!EverythingPanel.Enabled)
            {
                MessageBox.Show(LText.AlertMessages.AppClosing_OperationInProgress, LText.AlertMessages.Alert,
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                e.Cancel = true;
                return;
            }

            Application.RemoveMessageFilter(this);
            // Mouse hook will dispose along with the form

            // Argh, stupid hack to get this to not run TWICE on Application.Exit()
            // Application.Exit() is the worst thing ever. Before closing it just does whatever the hell it wants.
            FormClosing -= MainForm_FormClosing;

            UpdateConfig();
            Core.Shutdown();
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

        public void ChangeGameOrganization()
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
                Config.Filter.Games.ClearAndAdd(Config.GameTab);

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
                selTopRightTab == PatchTabPage ? TopRightTab.Patch :
                TopRightTab.Statistics;

            var selectedFM = GetSelectedFMPosInfo();

            var topRightTabOrder = new TopRightTabOrder
            {
                StatsTabPosition = TopRightTabControl.TabPages.IndexOf(StatisticsTabPage),
                EditFMTabPosition = TopRightTabControl.TabPages.IndexOf(EditFMTabPage),
                CommentTabPosition = TopRightTabControl.TabPages.IndexOf(CommentTabPage),
                TagsTabPosition = TopRightTabControl.TabPages.IndexOf(TagsTabPage),
                PatchTabPosition = TopRightTabControl.TabPages.IndexOf(PatchTabPage),
            };

            #region Quick hack to prevent splitter distances from freaking out if we're closing while minimized

            var nominalState = NominalWindowState;

            var minimized = false;
            if (WindowState == FormWindowState.Minimized)
            {
                minimized = true;
                WindowState = FormWindowState.Maximized;
            }

            float mainSplitterPercent = MainSplitContainer.SplitterPercentReal;
            float topSplitterPercent = TopSplitContainer.SplitterPercentReal;

            if (minimized) WindowState = nominalState;

            #endregion

            Core.UpdateConfig(
                NominalWindowState,
                NominalWindowSize,
                NominalWindowLocation,
                mainSplitterPercent,
                topSplitterPercent,
                FMsDGV.ColumnsToColumnData(),
                FMsDGV.CurrentSortedColumn,
                FMsDGV.CurrentSortDirection,
                FMsDGV.DefaultCellStyle.Font.SizeInPoints,
                FMsDGV.Filter,
                selectedFM,
                FMsDGV.GameTabsState,
                gameTab,
                topRightTab,
                topRightTabOrder,
                TopSplitContainer.FullScreen,
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
            return FMsDGV.Filtered ? Core.FMsViewList[FMsDGV.FilterShownIndexList[index]] : Core.FMsViewList[index];
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

            for (int i = 0; i < (FMsDGV.Filtered ? FMsDGV.FilterShownIndexList.Count : Core.FMsViewList.Count); i++)
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

        internal void SetDebugMessageText(string text)
        {
#if !ReleasePublic
            DebugLabel.Text = text;
#endif
        }

        #endregion

        #region FMsDGV-related

        public int GetRowCount() => FMsDGV.RowCount;

        public void SetRowCount(int count) => FMsDGV.RowCount = count;

        private void ZoomFMsDGV(ZoomFMsDGVType type, float? zoomFontSize = null)
        {
            // We'll be changing widths all over the place here, so don't save them out while we do this
            ColumnWidthSaveDisabled = true;
            try
            {
                // No goal escapes me, mate

                SelectedFM selFM = FMsDGV.SelectedRows.Count > 0 ? GetSelectedFMPosInfo() : null;

                var f = FMsDGV.DefaultCellStyle.Font;

                // Set zoom level
                var fontSize =
                    type == ZoomFMsDGVType.ZoomIn ? f.SizeInPoints + 1.0f :
                    type == ZoomFMsDGVType.ZoomOut ? f.SizeInPoints - 1.0f :
                    type == ZoomFMsDGVType.ZoomTo && zoomFontSize != null ? (float)zoomFontSize :
                    type == ZoomFMsDGVType.ZoomToHeightOnly && zoomFontSize != null ? (float)zoomFontSize :
                    FMsListDefaultFontSizeInPoints;

                // Clamp zoom level
                if (fontSize < Math.Round(1.00f, 2)) fontSize = 1.00f;
                if (fontSize > Math.Round(41.25f, 2)) fontSize = 41.25f;
                fontSize = (float)Math.Round(fontSize, 2);

                // Set new font size
                var newF = new Font(f.FontFamily, fontSize, f.Style, f.Unit, f.GdiCharSet, f.GdiVerticalFont);

                // Set row height based on font plus some padding
                var rowHeight = type == ZoomFMsDGVType.ResetZoom ? FMsListDefaultRowHeight : newF.Height + 9;

                // If we're on startup, then the widths will already have been restored (to zoomed size) from the
                // config
                var heightOnly = type == ZoomFMsDGVType.ZoomToHeightOnly;

                // Must be done first, else we get wrong values
                List<double> widthMul = new List<double>();
                foreach (DataGridViewColumn c in FMsDGV.Columns)
                {
                    var size = c.HeaderCell.Size;
                    widthMul.Add((double)size.Width / size.Height);
                }

                // Set font on cells
                FMsDGV.DefaultCellStyle.Font = newF;

                // Set font on headers
                FMsDGV.ColumnHeadersDefaultCellStyle.Font = newF;

                // Set height on all rows (but it won't take effect yet)
                FMsDGV.RowTemplate.Height = rowHeight;

                // Save previous selection
                int selIndex = FMsDGV.SelectedRows.Count > 0 ? FMsDGV.SelectedRows[0].Index : -1;
                using (new DisableEvents(this))
                {
                    // Force a regeneration of rows (height will take effect here)
                    int rowCount = FMsDGV.RowCount;
                    FMsDGV.RowCount = 0;
                    FMsDGV.RowCount = rowCount;

                    // Restore previous selection (no events will be fired, due to being in a DisableEvents block)
                    if (selIndex > -1) FMsDGV.Rows[selIndex].Selected = true;

                    // Set column widths (keeping ratio to height)
                    for (var i = 0; i < FMsDGV.Columns.Count; i++)
                    {
                        DataGridViewColumn c = FMsDGV.Columns[i];

                        // Complicated gobbledegook for handling different options and also special-casing the
                        // non-resizable columns
                        var reset = type == ZoomFMsDGVType.ResetZoom;
                        if (c != RatingImageColumn && c != FinishedColumn)
                        {
                            c.MinimumWidth = reset ? Defaults.MinColumnWidth : rowHeight + 3;
                        }

                        if (heightOnly)
                        {
                            if (c == RatingImageColumn || c == FinishedColumn)
                            {
                                c.Width = (int)Math.Round(c.HeaderCell.Size.Height * widthMul[i]);
                            }
                        }
                        else
                        {
                            if (reset && c == RatingImageColumn)
                            {
                                c.Width = RatingImageColumnWidth;
                            }
                            else if (reset && c == FinishedColumn)
                            {
                                c.Width = FinishedColumnWidth;
                            }
                            else
                            {
                                // The ever-present rounding errors creep in here, but meh. I should figure out
                                // how to not have those - ensure scaling always happens in integral pixel counts
                                // somehow?
                                c.Width = reset && Math.Abs(Config.FMsListFontSizeInPoints - FMsListDefaultFontSizeInPoints) < 0.1
                                    ? Config.Columns[i].Width
                                    : (int)Math.Ceiling(c.HeaderCell.Size.Height * widthMul[i]);
                            }
                        }
                    }
                }

                // Keep selected FM in the center of the list vertically where possible (UX nicety)
                if (selIndex > -1 && selFM != null)
                {
                    try
                    {
                        FMsDGV.FirstDisplayedScrollingRowIndex =
                            (FMsDGV.SelectedRows[0].Index - (FMsDGV.DisplayedRowCount(true) / 2))
                            .Clamp(0, FMsDGV.RowCount - 1);
                    }
                    catch (Exception)
                    {
                        // no room is available to display rows
                    }
                }
            }
            finally
            {
                ColumnWidthSaveDisabled = false;
            }

            // And that's how you do it
        }

        public async Task SortAndSetFilter(bool suppressRefresh = false, bool forceRefreshReadme = false,
            bool forceSuppressSelectionChangedEvent = false, bool suppressSuspendResume = false)
        {
            SortByCurrentColumn();
            await SetFilter(suppressRefresh, forceRefreshReadme, forceSuppressSelectionChangedEvent, suppressSuspendResume);
        }

        // PERF: 0.7~2.2ms with every filter set (including a bunch of tag filters), over 1098 set. But note that
        //       the majority had no tags for this test.
        //       This was tested with the Release_Testing (optimized) profile.
        //       All in all, I'd say performance is looking really good. Certainly better than I was expecting,
        //       given this is a reasonably naive implementation with no real attempt to be clever.
        private async Task SetFilter(bool suppressRefresh = false, bool forceRefreshReadme = false,
            bool forceSuppressSelectionChangedEvent = false, bool suppressSuspendResume = false)
        {
#if !ReleasePublic
            DebugLabel2.Text = int.TryParse(DebugLabel2.Text, out var result) ? (result + 1).ToString() : "1";
#endif

            #region Set filters that are stored in control state

            FMsDGV.Filter.Title = FilterTitleTextBox.Text;
            FMsDGV.Filter.Author = FilterAuthorTextBox.Text;

            FMsDGV.Filter.Games.Clear();
            if (FilterByThief1Button.Checked) FMsDGV.Filter.Games.Add(Game.Thief1);
            if (FilterByThief2Button.Checked) FMsDGV.Filter.Games.Add(Game.Thief2);
            if (FilterByThief3Button.Checked) FMsDGV.Filter.Games.Add(Game.Thief3);

            FMsDGV.Filter.Finished.Clear();
            if (FilterByFinishedButton.Checked) FMsDGV.Filter.Finished.Add(FinishedState.Finished);
            if (FilterByUnfinishedButton.Checked) FMsDGV.Filter.Finished.Add(FinishedState.Unfinished);

            FMsDGV.Filter.ShowJunk = FilterShowJunkCheckBox.Checked;

            #endregion

            // Skip the GetSelectedFM() check during a state where it may fail.
            // We also don't need it if we're forcing both things.
            var oldSelectedFM =
                forceRefreshReadme && forceSuppressSelectionChangedEvent ? null :
                FMsDGV.SelectedRows.Count > 0 ? GetSelectedFM() : null;

            FMsDGV.FilterShownIndexList.Clear();

            // This one gets checked in a loop, so cache it. Others are only checked twice at most, so leave them
            // be.
            var titleIsWhitespace = FMsDGV.Filter.Title.IsWhiteSpace();

            #region Early out

            if (titleIsWhitespace &&
                FMsDGV.Filter.Author.IsWhiteSpace() &&
                FMsDGV.Filter.Games.Count == 0 &&
                FMsDGV.Filter.Tags.Empty() &&
                FMsDGV.Filter.ReleaseDateFrom == null &&
                FMsDGV.Filter.ReleaseDateTo == null &&
                FMsDGV.Filter.LastPlayedFrom == null &&
                FMsDGV.Filter.LastPlayedTo == null &&
                FMsDGV.Filter.RatingFrom == -1 &&
                FMsDGV.Filter.RatingTo == 10 &&
                (FMsDGV.Filter.Finished.Count == 0 ||
                 (FMsDGV.Filter.Finished.Contains(FinishedState.Finished)
                  && FMsDGV.Filter.Finished.Contains(FinishedState.Unfinished))) &&
                FMsDGV.Filter.ShowJunk)
            {
                FMsDGV.Filtered = false;

                if (!suppressRefresh)
                {
                    await RefreshFMsList(
                        refreshReadme: forceRefreshReadme || (oldSelectedFM != null && !oldSelectedFM.Equals(GetFMFromIndex(0))),
                        suppressSelectionChangedEvent: forceSuppressSelectionChangedEvent || oldSelectedFM != null,
                        suppressSuspendResume);
                }
                return;
            }

            #endregion

            #region Title / initial

            for (int i = 0; i < Core.FMsViewList.Count; i++)
            {
                var fm = Core.FMsViewList[i];

                if (titleIsWhitespace ||
                    fm.Archive.ContainsI(FMsDGV.Filter.Title) ||
                    fm.Title.ContainsI(FMsDGV.Filter.Title) ||
                    fm.InstalledDir.ContainsI(FMsDGV.Filter.Title))
                {
                    FMsDGV.FilterShownIndexList.Add(i);
                }
            }

            #endregion

            #region Author

            if (!FMsDGV.Filter.Author.IsWhiteSpace())
            {
                for (int i = 0; i < FMsDGV.FilterShownIndexList.Count; i++)
                {
                    var fmAuthor = Core.FMsViewList[FMsDGV.FilterShownIndexList[i]].Author;

                    if (!fmAuthor.ContainsI(FMsDGV.Filter.Author))
                    {
                        FMsDGV.FilterShownIndexList.RemoveAt(i);
                        i--;
                    }
                }
            }

            #endregion

            #region Show junk

            if (!FMsDGV.Filter.ShowJunk)
            {
                for (int i = 0; i < FMsDGV.FilterShownIndexList.Count; i++)
                {
                    var fm = Core.FMsViewList[FMsDGV.FilterShownIndexList[i]];
                    if (fm.Game == Game.Unsupported && !FilterShowJunkCheckBox.Checked)
                    {
                        FMsDGV.FilterShownIndexList.RemoveAt(i);
                        i--;
                    }
                }
            }

            #endregion

            #region Games

            if (FMsDGV.Filter.Games.Count > 0)
            {
                for (int i = 0; i < FMsDGV.FilterShownIndexList.Count; i++)
                {
                    var fm = Core.FMsViewList[FMsDGV.FilterShownIndexList[i]];
                    if (GameIsKnownAndSupported(fm) && !FMsDGV.Filter.Games.Contains((Game)fm.Game))
                    {
                        FMsDGV.FilterShownIndexList.RemoveAt(i);
                        i--;
                    }
                }
            }

            #endregion

            #region Tags

            if (FMsDGV.Filter.Tags.AndTags.Count > 0 ||
                FMsDGV.Filter.Tags.OrTags.Count > 0 ||
                FMsDGV.Filter.Tags.NotTags.Count > 0)
            {
                var andTags = FMsDGV.Filter.Tags.AndTags;
                var orTags = FMsDGV.Filter.Tags.OrTags;
                var notTags = FMsDGV.Filter.Tags.NotTags;

                for (int i = 0; i < FMsDGV.FilterShownIndexList.Count; i++)
                {
                    var fmTags = Core.FMsViewList[FMsDGV.FilterShownIndexList[i]].Tags;
                    if (fmTags.Count == 0 && notTags.Count == 0)
                    {
                        FMsDGV.FilterShownIndexList.RemoveAt(i);
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
                            FMsDGV.FilterShownIndexList.RemoveAt(i);
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
                            FMsDGV.FilterShownIndexList.RemoveAt(i);
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
                            FMsDGV.FilterShownIndexList.RemoveAt(i);
                            i--;
                            continue;
                        }
                    }

                    #endregion
                }
            }

            #endregion

            #region Rating

            if (!(FMsDGV.Filter.RatingFrom == -1 && FMsDGV.Filter.RatingTo == 10))
            {
                var rf = FMsDGV.Filter.RatingFrom;
                var rt = FMsDGV.Filter.RatingTo;

                for (int i = 0; i < FMsDGV.FilterShownIndexList.Count; i++)
                {
                    var fmRating = Core.FMsViewList[FMsDGV.FilterShownIndexList[i]].Rating;

                    if (fmRating < rf || fmRating > rt)
                    {
                        FMsDGV.FilterShownIndexList.RemoveAt(i);
                        i--;
                    }
                }
            }

            #endregion

            #region Release date

            if (FMsDGV.Filter.ReleaseDateFrom != null || FMsDGV.Filter.ReleaseDateTo != null)
            {
                var rdf = FMsDGV.Filter.ReleaseDateFrom;
                var rdt = FMsDGV.Filter.ReleaseDateTo;

                for (int i = 0; i < FMsDGV.FilterShownIndexList.Count; i++)
                {
                    var fmRelDate = Core.FMsViewList[FMsDGV.FilterShownIndexList[i]].ReleaseDate;

                    if (fmRelDate == null ||
                        (rdf != null &&
                         fmRelDate.Value.Date.CompareTo(rdf.Value.Date) < 0) ||
                        (rdt != null &&
                         fmRelDate.Value.Date.CompareTo(rdt.Value.Date) > 0))
                    {
                        FMsDGV.FilterShownIndexList.RemoveAt(i);
                        i--;
                    }
                }
            }

            #endregion

            #region Last played

            if (FMsDGV.Filter.LastPlayedFrom != null || FMsDGV.Filter.LastPlayedTo != null)
            {
                var lpdf = FMsDGV.Filter.LastPlayedFrom;
                var lpdt = FMsDGV.Filter.LastPlayedTo;

                for (int i = 0; i < FMsDGV.FilterShownIndexList.Count; i++)
                {
                    var fmLastPlayed = Core.FMsViewList[FMsDGV.FilterShownIndexList[i]].LastPlayed;

                    if (fmLastPlayed == null ||
                        (lpdf != null &&
                         fmLastPlayed.Value.Date.CompareTo(lpdf.Value.Date) < 0) ||
                        (lpdt != null &&
                         fmLastPlayed.Value.Date.CompareTo(lpdt.Value.Date) > 0))
                    {
                        FMsDGV.FilterShownIndexList.RemoveAt(i);
                        i--;
                    }
                }
            }

            #endregion

            #region Finished

            if (FMsDGV.Filter.Finished.Count > 0)
            {
                for (int i = 0; i < FMsDGV.FilterShownIndexList.Count; i++)
                {
                    var fm = Core.FMsViewList[FMsDGV.FilterShownIndexList[i]];
                    var fmFinished = fm.FinishedOn;
                    var fmFinishedOnUnknown = fm.FinishedOnUnknown;

                    if (((fmFinished > 0 || fmFinishedOnUnknown) && !FMsDGV.Filter.Finished.Contains(FinishedState.Finished)) ||
                       ((fmFinished <= 0 && !fmFinishedOnUnknown) && !FMsDGV.Filter.Finished.Contains(FinishedState.Unfinished)))
                    {
                        FMsDGV.FilterShownIndexList.RemoveAt(i);
                        i--;
                    }
                }
            }

            #endregion

            FMsDGV.Filtered = true;

            // If the actual selected FM hasn't changed, don't reload its readme. While this can't eliminate the
            // lag when a filter selection first lands on a heavy readme, it at least prevents said readme from
            // being reloaded over and over every time you type a letter if the selected FM hasn't changed.

            if (suppressRefresh) return;

            // NOTE: GetFMFromIndex(0) takes 0 because RefreshFMsList() sets the selection to 0.
            // Remember this if you ever change that.
            await RefreshFMsList(
                refreshReadme: forceRefreshReadme || FMsDGV.FilterShownIndexList.Count == 0 ||
                               (oldSelectedFM != null && !oldSelectedFM.Equals(GetFMFromIndex(0))),
                suppressSelectionChangedEvent: forceSuppressSelectionChangedEvent || oldSelectedFM != null,
                suppressSuspendResume);
        }

        private void FMsDGV_CellValueNeeded_Initial(object sender, DataGridViewCellValueEventArgs e)
        {
            if (CellValueNeededDisabled) return;

            // Lazy-load these in an attempt to save some kind of startup time
            Thief1Icon = Resources.Thief1_21;
            Thief2Icon = Resources.Thief2_21;
            Thief3Icon = Resources.Thief3_21;
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
            FinishedOnUnknownIcon = Resources.Finished_Unknown;

            // Prevents having to check the bool again forevermore even after we've already set the images.
            // Taking an extremely minor technique from a data-oriented design talk, heck yeah!
            FMsDGV.CellValueNeeded -= FMsDGV_CellValueNeeded_Initial;
            FMsDGV.CellValueNeeded += FMsDGV_CellValueNeeded;
            FMsDGV_CellValueNeeded(sender, e);
        }

        private void FMsDGV_CellValueNeeded(object sender, DataGridViewCellValueEventArgs e)
        {
            if (CellValueNeededDisabled) return;

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

            switch ((Column)e.ColumnIndex)
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
                    e.Value = fm.FinishedOnUnknown ? FinishedOnUnknownIcon : FinishedOnIcons[(FinishedOn)fm.FinishedOn];
                    break;

                case Column.ReleaseDate:
                    e.Value = fm.ReleaseDate != null ? FormattedDate((DateTime)fm.ReleaseDate) : "";
                    break;

                case Column.LastPlayed:
                    e.Value = fm.LastPlayed != null ? FormattedDate((DateTime)fm.LastPlayed) : "";
                    break;

                case Column.DisabledMods:
                    e.Value = fm.DisableAllMods ? LText.FMsList.AllModsDisabledMessage : fm.DisabledMods;
                    break;

                case Column.Comment:
                    e.Value = fm.CommentSingleLine;
                    break;
            }
        }

        private async void FMsDGV_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;

            var newSortDirection =
                e.ColumnIndex == FMsDGV.CurrentSortedColumn && FMsDGV.CurrentSortDirection == SortOrder.Ascending
                    ? SortOrder.Descending
                    : SortOrder.Ascending;

            SortFMsDGV((Column)e.ColumnIndex, newSortDirection);
            if (FMsDGV.Filtered)
            {
                // SetFilter() calls a refresh on its own
                await SetFilter(forceRefreshReadme: true);
            }
            else
            {
                await RefreshFMsList(refreshReadme: true, suppressSelectionChangedEvent: true);
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

                ConvertAudioRCSubMenu.Enabled = GameIsDark(fm) && fm.Installed;
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
            for (int i = 0; i < FMsDGV.RowCount; i++)
            {
                if (FMsDGV.Rows[i].Cells[(int)Column.Title].Value.ToString().StartsWithI(firstChar.ToString()))
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
                var rowIndex = FindRowIndex(e.KeyChar);

                if (rowIndex > -1)
                {
                    FMsDGV.Rows[rowIndex].Selected = true;
                    FMsDGV.FirstDisplayedScrollingRowIndex = FMsDGV.SelectedRows[0].Index;
                }
            }
        }

        private async void FMsDGV_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F5)
            {
                if (e.Shift && !e.Control && !e.Alt)
                {
                    await Core.RefreshFromDisk();
                }
                else if (!e.Shift)
                {
                    await SortAndSetFilter();
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

        private async void PlayFMMenuItem_Click(object sender, EventArgs e) => await InstallAndPlay.InstallOrPlay(GetSelectedFM());

        private async void InstallUninstallMenuItem_Click(object sender, EventArgs e)
        {
            var fm = GetSelectedFM();

            await InstallAndPlay.InstallOrUninstall(fm);
        }

        private async void ConvertWAVsTo16BitMenuItem_Click(object sender, EventArgs e)
        {
            var fm = GetSelectedFM();
            if (!fm.Installed) return;

            await Core.ConvertWAVsTo16Bit(fm);
        }

        private async void ConvertOGGsToWAVsMenuItem_Click(object sender, EventArgs e)
        {
            var fm = GetSelectedFM();
            if (!fm.Installed) return;

            await Core.ConvertOGGsToWAVs(fm);
        }

        #endregion

        #endregion

        #region Install/Play buttons

        private async void InstallUninstallFMButton_Click(object sender, EventArgs e)
        {
            var fm = GetSelectedFM();

            await InstallAndPlay.InstallOrUninstall(fm);
        }

        private async void PlayFMButton_Click(object sender, EventArgs e) => await InstallAndPlay.InstallOrPlay(GetSelectedFM());

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
            var item = (ToolStripMenuItem)sender;

            var game =
                item == PlayOriginalThief1MenuItem ? Game.Thief1 :
                item == PlayOriginalThief2MenuItem ? Game.Thief2 :
                Game.Thief3;

            InstallAndPlay.PlayOriginalGame(game);
        }

        #endregion

        #endregion

        private async void ScanAllFMsButton_Click(object sender, EventArgs e)
        {
            if (Core.FMsViewList.Count == 0) return;

            ScanOptions scanOptions = null;
            bool noneSelected;
            using (var f = new ScanAllFMsForm())
            {
                if (f.ShowDialog() != DialogResult.OK) return;
                noneSelected = f.NoneSelected;
                if (!noneSelected)
                {
                    scanOptions = ScanOptions.FalseDefault(
                        scanTitle: f.ScanOptions.ScanTitle,
                        scanAuthor: f.ScanOptions.ScanAuthor,
                        scanGameType: f.ScanOptions.ScanGameType,
                        scanCustomResources: f.ScanOptions.ScanCustomResources,
                        scanSize: f.ScanOptions.ScanSize,
                        scanReleaseDate: f.ScanOptions.ScanReleaseDate,
                        scanTags: f.ScanOptions.ScanTags);
                }
            }

            if (noneSelected)
            {
                MessageBox.Show(LText.ScanAllFMsBox.NothingWasScanned, LText.AlertMessages.Alert);
                return;
            }

            var success = await Core.ScanFMs(Core.FMsViewList, scanOptions);
            if (success) await SortAndSetFilter(forceRefreshReadme: true);
        }

        private async void SettingsButton_Click(object sender, EventArgs e) => await Core.OpenSettings();

        public void ShowFMsListZoomButtons(bool visible)
        {
            FMsListZoomInButton.Visible = visible;
            FMsListZoomOutButton.Visible = visible;
            FMsListResetZoomButton.Visible = visible;
            FiltersFlowLayoutPanel.Width = (RefreshClearToolStripCustom.Location.X - 4) - FiltersFlowLayoutPanel.Location.X;
        }

        public void UpdateRatingDisplayStyle(RatingDisplayStyle style, bool startup)
        {
            UpdateRatingLists(style == RatingDisplayStyle.FMSel);
            UpdateRatingColumn(startup);
            UpdateRatingLabel();
        }

        private void UpdateRatingLists(bool fmSelStyle)
        {
            // Just in case, since changing a ComboBox item's text counts as a selected index change maybe? Argh!
            using (new DisableEvents(this))
            {
                for (int i = 0; i <= 10; i++)
                {
                    string num = (fmSelStyle ? i / 2.0 : i).ToString(CultureInfo.CurrentCulture);
                    RatingRCSubMenu.DropDownItems[i + 1].Text = num;
                    EditFMRatingComboBox.Items[i + 1] = num;
                }
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
                newRatingColumn.Width = newRatingColumn == RatingTextColumn
                    ? oldRatingColumn.Width
                    // To set the ratio back to exact on zoom reset
                    : FMsDGV.RowTemplate.Height == 22
                    ? RatingImageColumnWidth
                    : (FMsDGV.DefaultCellStyle.Font.Height + 9) * (RatingImageColumnWidth / 22);
                newRatingColumn.Visible = oldRatingColumn.Visible;
                newRatingColumn.DisplayIndex = oldRatingColumn.DisplayIndex;
            }

            if (!startup || newRatingColumn != RatingTextColumn)
            {
                using (new DisableEvents(this))
                {
                    CellValueNeededDisabled = true;
                    try
                    {
                        FMsDGV.Columns.RemoveAt((int)Column.Rating);
                        FMsDGV.Columns.Insert((int)Column.Rating, newRatingColumn);
                    }
                    finally
                    {
                        CellValueNeededDisabled = false;
                    }
                }
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

        private async Task OpenFilterTags()
        {
            using (var tf = new FilterTagsForm(GlobalTags, FMsDGV.Filter.Tags))
            {
                if (tf.ShowDialog() != DialogResult.OK) return;

                DeepCopyTagsFilter(tf.TagsFilter, FMsDGV.Filter.Tags);
                FilterByTagsButton.Checked = !FMsDGV.Filter.Tags.Empty();
            }

            await SortAndSetFilter();
        }

        #region Refresh FMs list

        internal async Task RefreshSelectedFMRowOnly() => await RefreshSelectedFM(false, true);

        public async Task RefreshSelectedFM(bool refreshReadme, bool refreshGridRowOnly = false)
        {
            FMsDGV.InvalidateRow(FMsDGV.SelectedRows[0].Index);

            if (refreshGridRowOnly) return;

            await DisplaySelectedFM(refreshReadme);
        }

        public async Task RefreshFMsList(bool refreshReadme, bool suppressSelectionChangedEvent = false,
            bool suppressSuspendResume = false)
        {
            using (suppressSelectionChangedEvent ? new DisableEvents(this) : null)
            {
                // A small but measurable perf increase from this. Also prevents flickering when switching game
                // tabs.
                if (!suppressSuspendResume) FMsDGV.SuspendDrawing();
                FMsDGV.RowCount = FMsDGV.Filtered ? FMsDGV.FilterShownIndexList.Count : Core.FMsViewList.Count;

                if (FMsDGV.RowCount == 0)
                {
                    if (!suppressSuspendResume) FMsDGV.ResumeDrawing();
                    ClearShownData();
                    InitialSelectedFMHasBeenSet = true;
                }
                else
                {
                    int row;
                    if (InitialSelectedFMHasBeenSet)
                    {
                        row = 0;
                        FMsDGV.FirstDisplayedScrollingRowIndex = 0;
                    }
                    else
                    {
                        row = GetIndexFromInstalledName(FMsDGV.CurrentSelFM.InstalledName).ClampToZero();
                        try
                        {
                            FMsDGV.FirstDisplayedScrollingRowIndex = (row - FMsDGV.CurrentSelFM.IndexFromTop).ClampToZero();
                        }
                        catch (Exception)
                        {
                            // no room is available to display rows
                        }
                        refreshReadme = true;
                    }

                    using (!InitialSelectedFMHasBeenSet ? new DisableEvents(this) : null)
                    {
                        FMsDGV.Rows[row].Selected = true;
                    }

                    // Resume drawing before loading the readme; that way the list will update instantly even
                    // if the readme doesn't. The user will see delays in the "right place" (the readme box)
                    // and understand why it takes a sec. Otherwise, it looks like merely changing tabs brings
                    // a significant delay, and that's annoying because it doesn't seem like it should happen.
                    if (!suppressSuspendResume) FMsDGV.ResumeDrawing();

                    await DisplaySelectedFM(refreshReadme);

                    InitialSelectedFMHasBeenSet = true;
                }
            }
        }

        public void RefreshFMsListKeepSelection()
        {
            if (FMsDGV.RowCount == 0) return;

            var selectedRow = FMsDGV.SelectedRows[0].Index;

            using (new DisableEvents(this))
            {
                FMsDGV.Refresh();
                FMsDGV.Rows[selectedRow].Selected = true;
            }
        }

        #endregion

        private void SortByCurrentColumn() => SortFMsDGV((Column)FMsDGV.CurrentSortedColumn, FMsDGV.CurrentSortDirection);

        public void SortFMsDGV(Column column, SortOrder sortDirection)
        {
            FMsDGV.CurrentSortedColumn = (int)column;
            FMsDGV.CurrentSortDirection = sortDirection;

            Core.SortFMsViewList(column, sortDirection);

            foreach (DataGridViewColumn c in FMsDGV.Columns) c.HeaderCell.SortGlyphDirection = SortOrder.None;
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

        private async void FMsDGV_SelectionChanged(object sender, EventArgs e)
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

                await DisplaySelectedFM(refreshReadme: true);
            }
        }

        // Perpetual TODO: Make sure this clears everything including the top right tab stuff
        private void ClearShownData()
        {
            if (Core.FMsViewList.Count == 0) ScanAllFMsButton.Enabled = false;

            InstallUninstallMenuItem.Text = LText.FMsList.FMMenu_InstallFM;
            InstallUninstallMenuItem.Enabled = false;
            // Special-cased; don't autosize this one
            InstallUninstallFMButton.Text = LText.MainButtons.InstallFM;
            InstallUninstallFMButton.Image = Resources.Install_24;
            InstallUninstallFMButton.Enabled = false;
            PlayFMMenuItem.Enabled = false;
            PlayFMButton.Enabled = false;

            OpenInDromedSep.Visible = false;
            OpenInDromEdMenuItem.Visible = false;

            ScanFMMenuItem.Enabled = false;

            // Hide instead of clear to avoid zoom factor pain
            ShowReadme(false);

            ChooseReadmePanel.Hide();
            ViewHTMLReadmeButton.Hide();
            WebSearchButton.Enabled = false;

            BlankStatsPanelWithMessage(LText.StatisticsTab.NoFMSelected);
            StatsScanCustomResourcesButton.Hide();

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

                UncheckFinishedOnMenuItemsExceptUnknown();
                FinishedOnUnknownMenuItem.Checked = false;

                CommentTextBox.Text = "";
                CommentTextBox.Enabled = false;
                AddTagTextBox.Text = "";

                TagsTreeView.Nodes.Clear();

                foreach (Control c in TagsTabPage.Controls) c.Enabled = false;

                PatchDMLsListBox.Items.Clear();
                PatchMainPanel.Show();
                PatchFMNotInstalledLabel.CenterHV(PatchTabPage);
                PatchFMNotInstalledLabel.Hide();
                PatchMainPanel.Enabled = false;
            }
        }

        private void HidePatchSectionWithMessage(string message)
        {
            PatchDMLsListBox.Items.Clear();
            PatchMainPanel.Hide();
            PatchFMNotInstalledLabel.Text = message;
            PatchFMNotInstalledLabel.CenterHV(PatchTabPage);
            PatchFMNotInstalledLabel.Show();
        }

        private void BlankStatsPanelWithMessage(string message)
        {
            CustomResourcesLabel.Text = message;
            foreach (CheckBox cb in StatsCheckBoxesPanel.Controls) cb.Checked = false;
            StatsCheckBoxesPanel.Hide();
        }

        // It's really hard to come up with a succinct name that makes it clear what this does and doesn't do
        private async Task DisplaySelectedFM(bool refreshReadme = false)
        {
            var fm = GetSelectedFM();

            if (fm.Game == null || (GameIsKnownAndSupported(fm) && !fm.MarkedScanned))
            {
                using (new DisableKeyPresses(this))
                {
                    // If successful, this method will be called again, so exit to avoid running it twice
                    if (await ScanSelectedFM(GetDefaultScanOptions())) return;
                }
            }

            bool fmIsT3 = fm.Game == Game.Thief3;

            #region Toggles

            // We should never get here when FMsList.Count == 0, but hey
            if (Core.FMsViewList.Count > 0) ScanAllFMsButton.Enabled = true;

            FinishedOnNormalMenuItem.Text = fmIsT3 ? LText.Difficulties.Easy : LText.Difficulties.Normal;
            FinishedOnHardMenuItem.Text = fmIsT3 ? LText.Difficulties.Normal : LText.Difficulties.Hard;
            FinishedOnExpertMenuItem.Text = fmIsT3 ? LText.Difficulties.Hard : LText.Difficulties.Expert;
            FinishedOnExtremeMenuItem.Text = fmIsT3 ? LText.Difficulties.Expert : LText.Difficulties.Extreme;
            // FinishedOnUnknownMenuItem text stays the same

            var installable = GameIsKnownAndSupported(fm);

            InstallUninstallMenuItem.Enabled = installable;
            InstallUninstallMenuItem.Text = fm.Installed
                ? LText.FMsList.FMMenu_UninstallFM
                : LText.FMsList.FMMenu_InstallFM;

            if ((fm.Game == Game.Thief1 && Config.T1DromEdDetected) ||
                (fm.Game == Game.Thief2 && Config.T2DromEdDetected))
            {
                OpenInDromedSep.Visible = true;
                OpenInDromEdMenuItem.Visible = true;
            }
            else
            {
                OpenInDromedSep.Visible = false;
                OpenInDromEdMenuItem.Visible = false;
            }

            InstallUninstallFMButton.Enabled = installable;
            // Special-cased; don't autosize this one
            InstallUninstallFMButton.Text = fm.Installed
                ? LText.MainButtons.UninstallFM
                : LText.MainButtons.InstallFM;
            InstallUninstallFMButton.Image = fm.Installed
                ? Resources.Uninstall_24
                : Resources.Install_24;

            PlayFMMenuItem.Enabled = installable;
            PlayFMButton.Enabled = installable;

            ScanFMMenuItem.Enabled = true;

            WebSearchButton.Enabled = true;

            foreach (Control c in EditFMTabPage.Controls) c.Enabled = true;

            CommentTextBox.Enabled = true;
            foreach (Control c in TagsTabPage.Controls) c.Enabled = true;

            PatchMainPanel.Enabled = true;

            if (!fm.Installed)
            {
                HidePatchSectionWithMessage(LText.PatchTab.FMNotInstalled);
            }

            PatchDMLsPanel.Enabled = fm.Game != Game.Thief3;

            #endregion

            #region FinishedOn

            if (fm.FinishedOnUnknown)
            {
                FinishedOnUnknownMenuItem.Checked = true;
                UncheckFinishedOnMenuItemsExceptUnknown();
            }
            else
            {
                var val = (FinishedOn)fm.FinishedOn;
                // I don't have to disable events because I'm only wired up to Click, not Checked
                FinishedOnNormalMenuItem.Checked = (val & FinishedOn.Normal) == FinishedOn.Normal;
                FinishedOnHardMenuItem.Checked = (val & FinishedOn.Hard) == FinishedOn.Hard;
                FinishedOnExpertMenuItem.Checked = (val & FinishedOn.Expert) == FinishedOn.Expert;
                FinishedOnExtremeMenuItem.Checked = (val & FinishedOn.Extreme) == FinishedOn.Extreme;
                FinishedOnUnknownMenuItem.Checked = false;
            }

            #endregion

            #region Custom resources

            if (fmIsT3)
            {
                BlankStatsPanelWithMessage(LText.StatisticsTab.CustomResourcesNotSupportedForThief3);
                StatsScanCustomResourcesButton.Hide();
            }
            else if (!FMCustomResourcesScanned(fm))
            {
                BlankStatsPanelWithMessage(LText.StatisticsTab.CustomResourcesNotScanned);
                StatsScanCustomResourcesButton.Show();
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
                StatsScanCustomResourcesButton.Show();
            }

            #endregion

            #region Other tabs

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

                if (fm.Installed)
                {
                    PatchMainPanel.Show();
                    PatchFMNotInstalledLabel.Hide();
                    PatchDMLsListBox.Items.Clear();
                    var (success, dmlFiles) = Core.GetDMLFiles(fm);
                    if (success)
                    {
                        foreach (var f in dmlFiles)
                        {
                            if (f.IsEmpty()) continue;
                            PatchDMLsListBox.Items.Add(f);
                        }
                    }
                }
            }

            DisplayFMTags(fm);

            #endregion

            if (!refreshReadme) return;

            var cacheData = await FMCache.GetCacheableData(fm, ProgressBox);

            #region Readme

            var readmeFiles = cacheData.Readmes;

            if (!readmeFiles.ContainsI(fm.SelectedReadme)) fm.SelectedReadme = null;

            using (new DisableEvents(this)) ChooseReadmeComboBox.ClearFullItems();

            void FillAndSelectReadmeFromMulti(string readme)
            {
                using (new DisableEvents(this))
                {
                    ChooseReadmeComboBox.AddRangeFull(readmeFiles);
                    ChooseReadmeComboBox.SelectBackingIndexOf(readme);
                }
            }

            if (!fm.SelectedReadme.IsEmpty())
            {
                if (readmeFiles.Count > 1)
                {
                    FillAndSelectReadmeFromMulti(fm.SelectedReadme);
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
                    var safeReadme = Core.DetectSafeReadme(readmeFiles, fm.Title);

                    if (!safeReadme.IsEmpty())
                    {
                        fm.SelectedReadme = safeReadme;
                        FillAndSelectReadmeFromMulti(safeReadme);
                    }
                    else
                    {
                        ShowReadme(false);
                        ViewHTMLReadmeButton.Hide();

                        ChooseReadmeListBox.ClearFullItems();
                        ChooseReadmeListBox.AddRangeFull(readmeFiles);

                        ShowReadmeControls(false);

                        ChooseReadmePanel.Show();

                        return;
                    }
                }
                else if (readmeFiles.Count == 1)
                {
                    fm.SelectedReadme = readmeFiles[0];

                    ChooseReadmeComboBox.Hide();
                }
            }

            ChooseReadmePanel.Hide();

            try
            {
                var (path, type) = Core.GetReadmeFileAndType(fm);
                ReadmeLoad(path, type);
            }
            catch (Exception ex)
            {
                Log(nameof(DisplaySelectedFM) + ": " + nameof(ReadmeLoad) + " failed.", ex);

                ViewHTMLReadmeButton.Hide();
                ShowReadme(true);
                ReadmeRichTextBox.SetText(LText.ReadmeArea.UnableToLoadReadme);
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

        internal void CancelScan() => Core.CancelScan();

        internal void CancelInstallFM() => InstallAndPlay.CancelInstallFM();

        #endregion

        #endregion

        #region Messageboxes

        public bool AskToContinue(string message, string title, bool noIcon = false)
        {
            var result = MessageBox.Show(message, title, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            return result == DialogResult.Yes;
        }

        public (bool Cancel, bool Continue)
        AskToContinueWithCancel(string message, string title)
        {
            var result = MessageBox.Show(message, title, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);
            if (result == DialogResult.Cancel) return (true, false);
            return (false, result == DialogResult.Yes);
        }

        public (bool Cancel, bool Continue, bool DontAskAgain)
        AskToContinueWithCancel_TD(string message, string title)
        {
            using (var d = new TaskDialog())
            using (var yesButton = new TaskDialogButton(ButtonType.Yes))
            using (var noButton = new TaskDialogButton(ButtonType.No))
            using (var cancelButton = new TaskDialogButton(ButtonType.Cancel))
            {
                d.AllowDialogCancellation = true;
                d.ButtonStyle = TaskDialogButtonStyle.Standard;
                d.WindowTitle = title;
                d.Content = message;
                d.VerificationText = LText.AlertMessages.DontAskAgain;
                d.Buttons.Add(yesButton);
                d.Buttons.Add(noButton);
                d.Buttons.Add(cancelButton);
                var buttonClicked = d.ShowDialog();
                var cancel = buttonClicked == null || buttonClicked == cancelButton;
                var cont = buttonClicked == yesButton;
                var dontAskAgain = d.IsVerificationChecked;
                return (cancel, cont, dontAskAgain);
            }
        }

        public (bool Cancel, bool Continue, bool DontAskAgain)
        AskToContinueWithCancelCustomStrings(string message, string title, TaskDialogIcon? icon,
            bool showDontAskAgain, string yes, string no, string cancel)
        {
            using (var d = new TaskDialog())
            using (var yesButton = new TaskDialogButton(yes))
            using (var noButton = new TaskDialogButton(no))
            using (var cancelButton = new TaskDialogButton(cancel))
            {
                d.AllowDialogCancellation = true;
                if (icon != null) d.MainIcon = (TaskDialogIcon)icon;
                d.ButtonStyle = TaskDialogButtonStyle.Standard;
                d.WindowTitle = title;
                d.Content = message;
                if (showDontAskAgain) d.VerificationText = LText.AlertMessages.DontAskAgain;
                d.Buttons.Add(yesButton);
                d.Buttons.Add(noButton);
                d.Buttons.Add(cancelButton);
                var buttonClicked = d.ShowDialog();
                var canceled = buttonClicked == null || buttonClicked == cancelButton;
                var cont = buttonClicked == yesButton;
                var dontAskAgain = d.IsVerificationChecked;
                return (canceled, cont, dontAskAgain);
            }
        }

        public (bool Cancel, bool DontAskAgain)
        AskToContinueYesNoCustomStrings(string message, string title, TaskDialogIcon? icon, bool showDontAskAgain,
            string yes, string no)
        {
            using (var d = new TaskDialog())
            using (var yesButton = yes != null ? new TaskDialogButton(yes) : new TaskDialogButton(ButtonType.Yes))
            using (var noButton = no != null ? new TaskDialogButton(no) : new TaskDialogButton(ButtonType.No))
            {
                d.AllowDialogCancellation = true;
                if (icon != null) d.MainIcon = (TaskDialogIcon)icon;
                d.ButtonStyle = TaskDialogButtonStyle.Standard;
                d.WindowTitle = title;
                d.Content = message;
                d.VerificationText = showDontAskAgain ? LText.AlertMessages.DontAskAgain : null;
                d.Buttons.Add(yesButton);
                d.Buttons.Add(noButton);
                var buttonClicked = d.ShowDialog();
                var cancel = buttonClicked != yesButton;
                var dontAskAgain = d.IsVerificationChecked;
                return (cancel, dontAskAgain);
            }
        }

        public void ShowAlert(string message, string title)
        {
            MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        #endregion

        private async void FilterTextBoxes_TextChanged(object sender, EventArgs e)
        {
            if (EventsDisabled) return;
            await SortAndSetFilter();
        }

        private async void FilterByGameCheckButtons_Click(object sender, EventArgs e)
        {
            if (EventsDisabled) return;
            await SortAndSetFilter();
        }

        private (SelectedFM GameSelFM, Filter GameFilter) GetGameSelFMAndFilter(TabPage tabPage)
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

            return (gameSelFM, gameFilter);
        }

        private void SaveCurrentTabSelectedFM(TabPage tabPage)
        {
            var (gameSelFM, gameFilter) = GetGameSelFMAndFilter(tabPage);
            var selFM = GetSelectedFMPosInfo();
            selFM.DeepCopyTo(gameSelFM);
            FMsDGV.Filter.DeepCopyTo(gameFilter);
        }

        private void GamesTabControl_Deselecting(object sender, TabControlCancelEventArgs e)
        {
            if (EventsDisabled) return;
            if (GamesTabControl.Visible) SaveCurrentTabSelectedFM(e.TabPage);
        }

        private async void GamesTabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (EventsDisabled) return;

            var (gameSelFM, gameFilter) = GetGameSelFMAndFilter(GamesTabControl.SelectedTab);

            FilterByThief1Button.Checked = gameSelFM == FMsDGV.GameTabsState.T1SelFM;
            FilterByThief2Button.Checked = gameSelFM == FMsDGV.GameTabsState.T2SelFM;
            FilterByThief3Button.Checked = gameSelFM == FMsDGV.GameTabsState.T3SelFM;

            gameSelFM.DeepCopyTo(FMsDGV.CurrentSelFM);
            gameFilter.DeepCopyTo(FMsDGV.Filter);

            SetUIFilterValues(gameFilter);

            InitialSelectedFMHasBeenSet = false;
            await SortAndSetFilter();
        }

        private async void CommentTextBox_TextChanged(object sender, EventArgs e)
        {
            if (EventsDisabled) return;

            if (FMsDGV.SelectedRows.Count == 0) return;

            var fm = GetSelectedFM();

            // Converting a multiline comment to single line:
            // DarkLoader copies up to the first linebreak or the 40 char mark, whichever comes first.
            // I'm doing the same, but bumping the cutoff point to 100 chars, which is still plenty fast.
            // fm.Comment.ToEscapes() is unbounded, but I measure tenths to hundredths of a millisecond even for
            // 25,000+ character strings with nothing but slashes and linebreaks in them.
            fm.Comment = CommentTextBox.Text.ToEscapes();
            fm.CommentSingleLine = CommentTextBox.Text.ToSingleLineComment(100);

            await RefreshSelectedFMRowOnly();
        }

        private void CommentTextBox_Leave(object sender, EventArgs e)
        {
            if (EventsDisabled) return;
            Core.WriteFullFMDataIni();
        }

        private void ResetLayoutButton_Click(object sender, EventArgs e)
        {
            MainSplitContainer.ResetSplitterPercent();
            TopSplitContainer.ResetSplitterPercent();
            if (FilterBarScrollRightButton.Visible) SetFilterBarScrollButtons();
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

            var list = Core.ListMatchingTags(AddTagTextBox.Text);
            if (list == null)
            {
                HideAddTagDropDown();
            }
            else
            {
                AddTagListBox.Items.Clear();
                foreach (var item in list) AddTagListBox.Items.Add(item);

                ShowAddTagDropDown();
            }
        }

        private void AddTagTextBoxOrListBox_KeyDown(object sender, KeyEventArgs e)
        {
            var box = AddTagListBox;

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
                    AddTagOperation(GetSelectedFM(), catAndTag);
                    break;
                default:
                    if (sender == AddTagListBox) AddTagTextBox.Focus();
                    break;
            }
        }

        private void AddTagListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            var lb = AddTagListBox;
            if (lb.SelectedIndex == -1) return;

            var tb = AddTagTextBox;

            using (new DisableEvents(this)) tb.Text = lb.SelectedItem.ToString();

            if (tb.Text.Length > 0) tb.SelectionStart = tb.Text.Length;
        }

        private void RemoveTagButton_Click(object sender, EventArgs e)
        {
            if (FMsDGV.SelectedRows.Count == 0) return;

            var fm = GetSelectedFM();
            var tv = TagsTreeView;

            var success = Core.RemoveTagFromFM(fm, tv.SelectedNode.Parent?.Text, tv.SelectedNode?.Text);
            if (!success) return;

            DisplayFMTags(fm);
            Core.WriteFullFMDataIni();
        }

        private void AddTagListBox_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;

            var lb = (ListBox)sender;

            if (lb.SelectedIndex > -1) AddTagOperation(GetSelectedFM(), lb.SelectedItem.ToString());
        }

        private void AddTagOperation(FanMission fm, string catAndTag)
        {
            // Cock-blocked here too
            if (AddTagTextBox.Text.CountChars(':') <= 1 && !AddTagTextBox.Text.IsWhiteSpace())
            {
                AddTagsToFMAndGlobalList(catAndTag, fm.Tags);
                UpdateFMTagsString(fm);
                DisplayFMTags(fm);
                Core.WriteFullFMDataIni();
            }

            AddTagTextBox.Clear();
            HideAddTagDropDown();
        }

        private void AddTagButton_Click(object sender, EventArgs e) => AddTagOperation(GetSelectedFM(), AddTagTextBox.Text);

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
            var item = (ToolStripMenuItem)sender;
            if (item.HasDropDownItems) return;

            var cat = item.OwnerItem;
            if (cat == null) return;

            AddTagOperation(GetSelectedFM(), cat.Text + @": " + item.Text);
        }

        private void AddTagMenuCustomItem_Click(object sender, EventArgs e)
        {
            var item = (ToolStripMenuItem)sender;

            var cat = item.OwnerItem;
            if (cat == null) return;

            AddTagTextBox.SetTextAndMoveCursorToEnd(cat.Text + @": ");
        }

        private void AddTagMenuMiscItem_Click(object sender, EventArgs e) => AddTagTextBox.SetTextAndMoveCursorToEnd(((ToolStripMenuItem)sender).Text);

        private void AddTagMenuEmptyItem_Click(object sender, EventArgs e) => AddTagTextBox.SetTextAndMoveCursorToEnd(((ToolStripMenuItem)sender).Text + ' ');

        // Just to keep things in a known state (clearing items also removes their event hookups, which is
        // convenient)
        private void AddTagMenu_Closed(object sender, ToolStripDropDownClosedEventArgs e) => AddTagMenu.Items.Clear();

        #endregion

        #region Readme

        #region Choose readme

        private async void ChooseReadmeButton_Click(object sender, EventArgs e)
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

            await RefreshSelectedFM(refreshReadme: true);
        }

        private async void ChooseReadmeComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (EventsDisabled) return;
            var fm = GetSelectedFM();
            fm.SelectedReadme = ChooseReadmeComboBox.SelectedBackingItem();
            await RefreshSelectedFM(refreshReadme: true);
        }

        private void ChooseReadmeComboBox_DropDownClosed(object sender, EventArgs e)
        {
            if (!CursorOverReadmeArea()) ShowReadmeControls(false);
        }

        #endregion

        private void ReadmeRichTextBox_LinkClicked(object sender, LinkClickedEventArgs e) => Core.OpenLink(e.LinkText);

        private void ZoomInButton_Click(object sender, EventArgs e) => ReadmeRichTextBox.ZoomIn();

        private void ZoomOutButton_Click(object sender, EventArgs e) => ReadmeRichTextBox.ZoomOut();

        private void ResetZoomButton_Click(object sender, EventArgs e) => ReadmeRichTextBox.ResetZoomFactor();

        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="IOException"></exception>
        private void ReadmeLoad(string path, ReadmeType readmeType)
        {
            #region Debug

            // Tells me whether a readme got reloaded more than once, which should never be allowed to happen
            // due to performance concerns.
#if !ReleasePublic
            DebugLabel.Text = int.TryParse(DebugLabel.Text, out var result) ? (result + 1).ToString() : "1";
#endif

            #endregion

            if (readmeType == ReadmeType.HTML)
            {
                ViewHTMLReadmeButton.Show();
                ShowReadme(false);
                // In case the cursor is over the scroll bar area
                if (CursorOverReadmeArea()) ShowReadmeControls(true);
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
            ZoomInButton.BackColor = enabled ? SystemColors.Window : SystemColors.Control;
            ZoomOutButton.BackColor = enabled ? SystemColors.Window : SystemColors.Control;
            ResetZoomButton.BackColor = enabled ? SystemColors.Window : SystemColors.Control;
            ReadmeFullScreenButton.BackColor = enabled ? SystemColors.Window : SystemColors.Control;

            // In case the cursor is already over the readme when we do this
            // (cause it won't show automatically if it is)
            ShowReadmeControls(enabled && CursorOverReadmeArea());
        }

        private void ShowReadmeControls(bool enabled)
        {
            ZoomInButton.Visible = enabled;
            ZoomOutButton.Visible = enabled;
            ResetZoomButton.Visible = enabled;
            ReadmeFullScreenButton.Visible = enabled;
            ChooseReadmeComboBox.Visible = enabled && ChooseReadmeComboBox.Items.Count > 0;
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
            Core.WriteFullFMDataIni();
        }

        private async void EditFMTitleTextBox_TextChanged(object sender, EventArgs e)
        {
            if (EventsDisabled) return;
            var fm = GetSelectedFM();
            fm.Title = EditFMTitleTextBox.Text;
            await RefreshSelectedFMRowOnly();
        }

        private async void EditFMAuthorTextBox_TextChanged(object sender, EventArgs e)
        {
            if (EventsDisabled) return;
            var fm = GetSelectedFM();
            fm.Author = EditFMAuthorTextBox.Text;
            await RefreshSelectedFMRowOnly();
        }

        private void EditFMAuthorTextBox_Leave(object sender, EventArgs e)
        {
            if (EventsDisabled) return;
            Core.WriteFullFMDataIni();
        }

        private async void EditFMReleaseDateCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (EventsDisabled) return;
            EditFMReleaseDateDateTimePicker.Visible = EditFMReleaseDateCheckBox.Checked;

            var fm = GetSelectedFM();

            fm.ReleaseDate = EditFMReleaseDateCheckBox.Checked
                ? EditFMReleaseDateDateTimePicker.Value
                : (DateTime?)null;

            await RefreshSelectedFMRowOnly();
            Core.WriteFullFMDataIni();
        }

        private async void EditFMReleaseDateDateTimePicker_ValueChanged(object sender, EventArgs e)
        {
            if (EventsDisabled) return;
            var fm = GetSelectedFM();
            fm.ReleaseDate = EditFMReleaseDateDateTimePicker.Value;
            await RefreshSelectedFMRowOnly();
            Core.WriteFullFMDataIni();
        }

        private async void EditFMLastPlayedCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (EventsDisabled) return;
            EditFMLastPlayedDateTimePicker.Visible = EditFMLastPlayedCheckBox.Checked;

            var fm = GetSelectedFM();

            fm.LastPlayed = EditFMLastPlayedCheckBox.Checked
                ? EditFMLastPlayedDateTimePicker.Value
                : (DateTime?)null;

            await RefreshSelectedFMRowOnly();
            Core.WriteFullFMDataIni();
        }

        private async void EditFMLastPlayedDateTimePicker_ValueChanged(object sender, EventArgs e)
        {
            if (EventsDisabled) return;
            var fm = GetSelectedFM();
            fm.LastPlayed = EditFMLastPlayedDateTimePicker.Value;
            await RefreshSelectedFMRowOnly();
            Core.WriteFullFMDataIni();
        }

        private async void EditFMDisabledModsTextBox_TextChanged(object sender, EventArgs e)
        {
            if (EventsDisabled) return;
            var fm = GetSelectedFM();
            fm.DisabledMods = EditFMDisabledModsTextBox.Text;
            await RefreshSelectedFMRowOnly();
        }

        private void EditFMDisabledModsTextBox_Leave(object sender, EventArgs e)
        {
            if (EventsDisabled) return;
            Core.WriteFullFMDataIni();
        }

        private async void EditFMDisableAllModsCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (EventsDisabled) return;
            EditFMDisabledModsTextBox.Enabled = !EditFMDisableAllModsCheckBox.Checked;

            var fm = GetSelectedFM();
            fm.DisableAllMods = EditFMDisableAllModsCheckBox.Checked;
            await RefreshSelectedFMRowOnly();
            Core.WriteFullFMDataIni();
        }

        private async void EditFMRatingComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (EventsDisabled) return;
            var fm = GetSelectedFM();
            fm.Rating = EditFMRatingComboBox.SelectedIndex - 1;
            await RefreshSelectedFMRowOnly();
            Core.WriteFullFMDataIni();
        }

        #endregion

        private async void RatingRCMenuItems_Click(object sender, EventArgs e)
        {
            var fm = GetSelectedFM();

            for (int i = 0; i < RatingRCSubMenu.DropDownItems.Count; i++)
            {
                if (RatingRCSubMenu.DropDownItems[i] != sender) continue;

                fm.Rating = i - 1;
                await RefreshSelectedFM(refreshReadme: false);
                Core.WriteFullFMDataIni();
                break;
            }
        }

        private void EditFMFinishedOnButton_Click(object sender, EventArgs e)
        {
            var button = EditFMFinishedOnButton;
            var menu = FinishedOnMenu;

            // Menu could be stuck to a submenu, so show, hide, show to get it unstuck and showing in the right
            // place
            menu.Show();
            menu.Hide();
            menu.Show(button, 0, button.Height);
        }

        private async void FinishedOnMenuItems_Click(object sender, EventArgs e)
        {
            var senderItem = (ToolStripMenuItem)sender;

            var fm = GetSelectedFM();

            fm.FinishedOn = 0;
            fm.FinishedOnUnknown = false;

            if (senderItem == FinishedOnUnknownMenuItem)
            {
                fm.FinishedOnUnknown = senderItem.Checked;
            }
            else
            {
                int at = 1;
                foreach (ToolStripMenuItem item in FinishedOnMenu.Items)
                {
                    if (item == FinishedOnUnknownMenuItem) continue;

                    if (item.Checked) fm.FinishedOn |= at;
                    at <<= 1;
                }
                if (fm.FinishedOn > 0)
                {
                    FinishedOnUnknownMenuItem.Checked = false;
                    fm.FinishedOnUnknown = false;
                }
            }

            await RefreshSelectedFMRowOnly();
            Core.WriteFullFMDataIni();
        }

        private void FinishedOnUnknownMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            if (FinishedOnUnknownMenuItem.Checked)
            {
                UncheckFinishedOnMenuItemsExceptUnknown();
            }
        }

        private void UncheckFinishedOnMenuItemsExceptUnknown()
        {
            FinishedOnNormalMenuItem.Checked = false;
            FinishedOnHardMenuItem.Checked = false;
            FinishedOnExpertMenuItem.Checked = false;
            FinishedOnExtremeMenuItem.Checked = false;
        }

        private void ReadmeFullScreenButton_Click(object sender, EventArgs e)
        {
            MainSplitContainer.ToggleFullScreen();
            ShowReadmeControls(CursorOverReadmeArea());
        }

        private void WebSearchButton_Click(object sender, EventArgs e) => SearchWeb();

        private void SearchWeb() => Core.OpenWebSearchUrl(GetSelectedFM());

        private void FiltersFlowLayoutPanel_SizeChanged(object sender, EventArgs e) => SetFilterBarScrollButtons();

        private void FiltersFlowLayoutPanel_Scroll(object sender, ScrollEventArgs e) => SetFilterBarScrollButtons();

        private void SetFilterBarScrollButtons()
        {
            if (EventsDisabled) return;

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
                using (new DisableEvents(this))
                {
                    // Disgusting! But necessary to patch up heisenbuggy behavior with this crap. This is really
                    // bad in general anyway, but how else am I supposed to have show-and-hide scroll buttons with
                    // WinForms? Argh!
                    for (int i = 0; i < 8; i++)
                    {
                        InteropMisc.SendMessage(FiltersFlowLayoutPanel.Handle, InteropMisc.WM_SCROLL, (IntPtr)InteropMisc.SB_LINELEFT, IntPtr.Zero);
                    }
                }
            }
            else if (hs.Value >= (hs.Maximum + 1) - hs.LargeChange)
            {
                ShowLeft();
                FilterBarScrollRightButton.Hide();
                using (new DisableEvents(this))
                {
                    // Ditto the above
                    for (int i = 0; i < 8; i++)
                    {
                        InteropMisc.SendMessage(FiltersFlowLayoutPanel.Handle, InteropMisc.WM_SCROLL, (IntPtr)InteropMisc.SB_LINERIGHT, IntPtr.Zero);
                    }
                }
            }
            else
            {
                ShowLeft();
                ShowRight();
            }
        }

        private async void FilterShowJunkCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (EventsDisabled) return;
            await SortAndSetFilter();
        }

        private async void FilterByFinishedButton_Click(object sender, EventArgs e) => await SortAndSetFilter();

        private async void FilterByUnfinishedButton_Click(object sender, EventArgs e) => await SortAndSetFilter();

        private async void FilterByRatingButton_Click(object sender, EventArgs e) => await OpenRatingFilter();

        private async void FilterByTagsButton_Click(object sender, EventArgs e) => await OpenFilterTags();

        private async void FilterByReleaseDateButton_Click(object sender, EventArgs e) => await OpenDateFilter(lastPlayed: false);

        private async void FilterByLastPlayedButton_Click(object sender, EventArgs e) => await OpenDateFilter(lastPlayed: true);

        private async Task OpenDateFilter(bool lastPlayed)
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
            await SortAndSetFilter();
        }

        private void UpdateDateLabel(bool lastPlayed, bool suspendResume = true)
        {
            var button = lastPlayed ? FilterByLastPlayedButton : FilterByReleaseDateButton;
            var fromDate = lastPlayed ? FMsDGV.Filter.LastPlayedFrom : FMsDGV.Filter.ReleaseDateFrom;
            var toDate = lastPlayed ? FMsDGV.Filter.LastPlayedTo : FMsDGV.Filter.ReleaseDateTo;
            var label = lastPlayed ? FilterByLastPlayedLabel : FilterByReleaseDateLabel;

            // Normally you can see the re-layout kind of "sequentially happen", this stops that and makes it
            // snappy
            if (suspendResume) FiltersFlowLayoutPanel.SuspendDrawing();
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
                if (suspendResume) FiltersFlowLayoutPanel.ResumeDrawing();
            }
        }

        private async Task OpenRatingFilter()
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
            await SortAndSetFilter();
        }

        private void UpdateRatingLabel(bool suspendResume = true)
        {
            // For snappy visual layout performance
            if (suspendResume) FiltersFlowLayoutPanel.SuspendDrawing();
            try
            {
                if (FilterByRatingButton.Checked)
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
                if (suspendResume) FiltersFlowLayoutPanel.ResumeDrawing();
            }
        }

        private async void ClearFiltersButton_Click(object sender, EventArgs e) => await ClearAllUIAndInternalFilters();

        public async Task ClearAllUIAndInternalFilters()
        {
            using (new DisableEvents(this))
            {
                FiltersFlowLayoutPanel.SuspendDrawing();
                try
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
                finally
                {
                    FiltersFlowLayoutPanel.ResumeDrawing();
                }
            }

            await SortAndSetFilter();
        }

        private async void RefreshFiltersButton_Click(object sender, EventArgs e) => await SortAndSetFilter();

        private void ViewHTMLReadmeButton_Click(object sender, EventArgs e) => Core.ViewHTMLReadme(GetSelectedFM());

        private void ImportButton_Click(object sender, EventArgs e)
        {
            var menu = ImportFromMenu;
            var button = ImportButton;
            menu.Show(button, -(menu.Width - button.Width), -menu.Height);
        }

        private async void ImportFromDarkLoaderMenuItem_Click(object sender, EventArgs e)
        {
            string iniFile;
            bool importFMData;
            bool importSaves;
            using (var f = new ImportFromDarkLoaderForm())
            {
                if (f.ShowDialog() != DialogResult.OK) return;
                iniFile = f.DarkLoaderIniFile;
                importFMData = f.ImportFMData;
                importSaves = f.ImportSaves;
            }

            if (!importFMData && !importSaves)
            {
                MessageBox.Show(LText.Importing.NothingWasImported, LText.AlertMessages.Alert);
                return;
            }

            // Do this every time we modify FMsViewList in realtime, to prevent FMsDGV from redrawing from the
            // list when it's in an indeterminate state (which can cause a selection change (bad) and/or a visible
            // change of the list (not really bad but unprofessional looking))
            SetRowCount(0);

            bool success = await Core.ImportFromDarkLoader(iniFile, importFMData, importSaves);

            // Do this no matter what; because we set the row count to 0 the list MUST be refreshed
            await SortAndSetFilter(forceRefreshReadme: true, forceSuppressSelectionChangedEvent: true);
        }

        private async void ImportFromFMSelMenuItem_Click(object sender, EventArgs e) => await ImportFromNDLOrFMSel(ImportType.FMSel);

        private async void ImportFromNewDarkLoaderMenuItem_Click(object sender, EventArgs e) => await ImportFromNDLOrFMSel(ImportType.NewDarkLoader);

        private async Task ImportFromNDLOrFMSel(ImportType importType)
        {
            List<string> iniFiles = new List<string>();
            using (var f = new ImportFromMultipleInisForm(importType))
            {
                if (f.ShowDialog() != DialogResult.OK) return;
                foreach (var file in f.IniFiles) iniFiles.Add(file);
            }

            if (iniFiles.All(x => x.IsWhiteSpace()))
            {
                MessageBox.Show(LText.Importing.NothingWasImported, LText.AlertMessages.Alert);
                return;
            }

            // Do this every time we modify FMsViewList in realtime, to prevent FMsDGV from redrawing from the
            // list when it's in an indeterminate state (which can cause a selection change (bad) and/or a visible
            // change of the list (not really bad but unprofessional looking))
            SetRowCount(0);

            foreach (var file in iniFiles)
            {
                if (file.IsWhiteSpace()) continue;

                // We're modifying the data that FMsDGV pulls from when it redraws. This will at least prevent a
                // selection changed event from firing while we do it, as that could be really bad potentially.
                bool success = await (importType == ImportType.FMSel
                    ? Core.ImportFromFMSel(file)
                    : Core.ImportFromNDL(file));
            }

            // Do this no matter what; because we set the row count to 0 the list MUST be refreshed
            await SortAndSetFilter(forceRefreshReadme: true, forceSuppressSelectionChangedEvent: true);
        }

        private void WebSearchMenuItem_Click(object sender, EventArgs e) => SearchWeb();

        private async void EditFMScanTitleButton_Click(object sender, EventArgs e) => await ScanSelectedFM(ScanOptions.FalseDefault(scanTitle: true));

        private async void EditFMScanAuthorButton_Click(object sender, EventArgs e) => await ScanSelectedFM(ScanOptions.FalseDefault(scanAuthor: true));

        private async void EditFMScanReleaseDateButton_Click(object sender, EventArgs e) => await ScanSelectedFM(ScanOptions.FalseDefault(scanReleaseDate: true));

        private async void RescanCustomResourcesButton_Click(object sender, EventArgs e) => await ScanSelectedFM(ScanOptions.FalseDefault(scanCustomResources: true));

        private async Task<bool> ScanSelectedFM(ScanOptions scanOptions)
        {
            bool success = await Core.ScanFM(GetSelectedFM(), scanOptions);
            if (success) await RefreshSelectedFM(refreshReadme: true);
            return success;
        }

        private async void EditFMScanForReadmesButton_Click(object sender, EventArgs e)
        {
            GetSelectedFM().RefreshCache = true;
            await DisplaySelectedFM(refreshReadme: true);
        }

        // Hack for when the textbox is smaller than the button or overtop of it or whatever... anchoring...
        private void TopSplitContainer_Panel2_SizeChanged(object sender, EventArgs e)
        {
            AddTagTextBox.Width = AddTagButton.Left > AddTagTextBox.Left
                ? (AddTagButton.Left - AddTagTextBox.Left) - 1
                : 0;
        }

        private async void ScanFMMenuItem_Click(object sender, EventArgs e) => await ScanSelectedFM(GetDefaultScanOptions());

        private void PatchRemoveDMLButton_Click(object sender, EventArgs e)
        {
            var lb = PatchDMLsListBox;
            if (lb.SelectedIndex == -1) return;

            bool success = Core.RemoveDML(GetSelectedFM(), lb.SelectedItem.ToString());
            if (!success) return;

            lb.RemoveAndSelectNearest();
        }

        private void PatchAddDMLButton_Click(object sender, EventArgs e)
        {
            var lb = PatchDMLsListBox;

            var dmlFiles = new List<string>();

            using (var d = new OpenFileDialog())
            {
                d.Multiselect = true;
                d.Filter = LText.BrowseDialogs.DMLFiles + @"|*.dml";
                if (d.ShowDialog() != DialogResult.OK || d.FileNames.Length == 0) return;
                dmlFiles.AddRange(d.FileNames);
            }

            foreach (var f in dmlFiles)
            {
                if (f.IsEmpty()) continue;

                bool success = Core.AddDML(GetSelectedFM(), f);
                if (!success) return;

                var dml = Path.GetFileName(f);
                if (!lb.Items.Cast<string>().ToArray().ContainsI(dml))
                {
                    lb.Items.Add(Path.GetFileName(f));
                }
            }
        }

        private void PatchOpenFMFolderButton_Click(object sender, EventArgs e) => Core.OpenFMFolder(GetSelectedFM());

        private async void OpenInDromEdMenuItem_Click(object sender, EventArgs e)
        {
            var fm = GetSelectedFM();

            if (!fm.Installed && !await InstallAndPlay.InstallFM(fm)) return;

            InstallAndPlay.OpenFMInDromEd(fm);
        }

        private void TopRightCollapseButton_Click(object sender, EventArgs e)
        {
            TopSplitContainer.ToggleFullScreen();
            SetTopRightCollapsedState();
        }

        private void SetTopRightCollapsedState()
        {
            var collapsed = TopSplitContainer.FullScreen;
            TopRightTabControl.Enabled = !collapsed;
            TopRightCollapseButton.Image = collapsed ? Resources.ArrowLeftSmall : Resources.ArrowRightSmall;
        }

        private async void FMsDGV_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            FanMission fm;
            if (e.RowIndex < 0 || FMsDGV.SelectedRows.Count == 0 || !GameIsKnownAndSupported(fm = GetSelectedFM()))
            {
                return;
            }

            await InstallAndPlay.InstallOrPlay(fm, askConfIfRequired: true);
        }

        private async void RefreshFromDiskButton_Click(object sender, EventArgs e) => await Core.RefreshFromDisk();

        // TODO: This isn't hooked up to anything, but things seem to work fine. Do I need this?!
        private void FMsDGV_ColumnWidthChanged(object sender, DataGridViewColumnEventArgs e)
        {
            if (ColumnWidthSaveDisabled) return;

            if (Config.Columns.Count == 0) return;
            //Trace.WriteLine(e.Column.Index.ToString());
            Config.Columns[e.Column.Index].Width = e.Column.Width;
        }

        private void FMsListZoomInButton_Click(object sender, EventArgs e) => ZoomFMsDGV(ZoomFMsDGVType.ZoomIn);

        private void FMsListZoomOutButton_Click(object sender, EventArgs e) => ZoomFMsDGV(ZoomFMsDGVType.ZoomOut);

        private void FMsListResetZoomButton_Click(object sender, EventArgs e) => ZoomFMsDGV(ZoomFMsDGVType.ResetZoom);

        private void PlayFMAdvancedMenuItem_Click(object sender, EventArgs e)
        {
            // Throw up dialog with config var options
        }
    }
}
