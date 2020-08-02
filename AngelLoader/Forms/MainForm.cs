﻿/* TODO: MainForm notes:
 PERF_TODO: Removing ALL asyncs and awaits saves 13ms on startup and 48k off the exe.
 If it comes down to it, I can probably figure out how to do the old way of async programming that doesn't add
 state machines a bazillion levels down, so as to only have the overhead right where it's actually used. But
 the time savings isn't as much as I thought it might be, so meh.

 NOTE: Don't lazy load the filter bar scroll buttons, as they screw the whole thing up (FMsDGV doesn't anchor
 in its panel correctly, etc.). If we figure out how to solve this later, we can lazy load them then.

 Images to switch to drawing:
 -Zoom images
 -Install / uninstall
 -Green CheckCircle
 -See if we can draw and/or just make these more efficient:
  -Rating and finished images (they're a bunch of the same pic just pasted together differently, we should maybe
   have just one of each unique pic and copy them into bitmaps as needed. That also ties into lazy loading below)
 -Settings (can we do gradients and curved paths?)
 -Import
 -Calendars (can we get detailed enough? The play arrow icon cutting the lines at a diagonal might be an obstacle)
 -Anything else not listed in "definitely won't draw" is at least a possibility

 Images we definitely won't draw (iow that really need to be rasters):
 -Thief logos
 -Zip logo (Show_Unsupported)
 -Rating examples (two of them have text)

 Things to lazy load:
 -Top-right section in its entirety, and then individual tab pages (in case some are hidden), and then individual
  controls on each tab page (in case the tabs are visible but not selected on startup)
 -Game buttons and game tabs (one or the other will be invisible on startup)
 -DataGridView images at a more granular level (right now they're all loaded at once as soon as any are needed)
*/

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using AngelLoader.DataClasses;
using AngelLoader.Forms.CustomControls;
using AngelLoader.Forms.CustomControls.Static_LazyLoaded;
using AngelLoader.Importing;
using AngelLoader.Properties;
using AngelLoader.WinAPI;
using AngelLoader.WinAPI.Ookii.Dialogs;
using static AngelLoader.GameSupport;
using static AngelLoader.GameSupport.GameIndex;
using static AngelLoader.Logger;
using static AngelLoader.Misc;

namespace AngelLoader.Forms
{
    public sealed partial class MainForm : Form, IView, IKeyPressDisabler, IMessageFilter
    {
        // We don't need to dispose anything on here really, because the app closes when the form closes, so
        // Windows will dispose it all anyway
#pragma warning disable IDE0069 // Disposable fields should be disposed

        #region Test / debug

#if !ReleaseBeta && !ReleasePublic
        private readonly CheckBox ForceWindowedCheckBox;
        private void ForceWindowedCheckBox_CheckedChanged(object sender, EventArgs e) => Config.ForceWindowed = ForceWindowedCheckBox.Checked;
#endif

#if DEBUG || (Release_Testing && !RT_StartupOnly)

        private void TestButton_Click(object sender, EventArgs e)
        {
        }

        private void Test2Button_Click(object sender, EventArgs e)
        {
            Width = 1305;
            Height = 750;
        }

#endif

        #endregion

        #region IView implementations

        #region Invoke

        public object InvokeSync(Delegate method) => Invoke(method);
        public object InvokeSync(Delegate method, params object[] args) => Invoke(method, args);

        #endregion

        public Column GetCurrentSortedColumnIndex() => FMsDGV.CurrentSortedColumn;
        public SortOrder GetCurrentSortDirection() => FMsDGV.CurrentSortDirection;
        public bool GetShowRecentAtTop() => FilterShowRecentAtTopButton.Checked;

        public void Block(bool block)
        {
            if (ViewBlockingPanel == null)
            {
                ViewBlockingPanel = new TransparentPanel { Visible = false };
                Controls.Add(ViewBlockingPanel);
                ViewBlockingPanel.Dock = DockStyle.Fill;
            }

            try
            {
                // Doesn't help the RichTextBox, it happily flickers like it always does. Oh well.
                this.SuspendDrawing();
                _viewBlocked = block;
                ViewBlockingPanel.Visible = block;
                ViewBlockingPanel.BringToFront();
            }
            finally
            {
                this.ResumeDrawing();
            }
        }

        public SelectedFM? GetSelectedFMPosInfo() => FMsDGV.RowSelected() ? FMsDGV.GetSelectedFMPosInfo() : null;

        public Filter GetFilter() => FMsDGV.Filter;
        public string GetTitleFilter() => FilterTitleTextBox.Text;
        public string GetAuthorFilter() => FilterAuthorTextBox.Text;

        public bool[] GetGameFiltersEnabledStates()
        {
            bool[] gamesChecked = new bool[SupportedGameCount];

            for (int i = 0; i < SupportedGameCount; i++)
            {
                gamesChecked[i] = _filterByGameButtonsInOrder[i].Checked;
            }

            return gamesChecked;
        }

        public bool GetFinishedFilter() => FilterByFinishedButton.Checked;
        public bool GetUnfinishedFilter() => FilterByUnfinishedButton.Checked;
        public bool GetShowUnsupportedFilter() => FilterShowUnsupportedButton.Checked;
        public List<int> GetFilterShownIndexList() => FMsDGV.FilterShownIndexList;

#if DEBUG || (Release_Testing && !RT_StartupOnly)
        public string GetDebug1Text() => DebugLabel.Text;
        public string GetDebug2Text() => DebugLabel2.Text;
        public void SetDebug1Text(string value) => DebugLabel.Text = value;
        public void SetDebug2Text(string value) => DebugLabel2.Text = value;
#endif

        public void Localize() => Localize(startup: false);

        public void ChangeReadmeBoxFont(bool useFixed) => ReadmeRichTextBox.SetFontType(useFixed);

        public void ShowInstallUninstallButton(bool enabled)
        {
            if (enabled)
            {
                if (!InstallUninstallFMLLButton.Constructed)
                {
                    InstallUninstallFMLLButton.Construct(this);
                    InstallUninstallFMLLButton.Localize(false);
                }
                InstallUninstallFMLLButton.Show();
            }
            else
            {
                InstallUninstallFMLLButton.Hide();
            }
        }

        public void ChangeGameOrganization(bool startup = false)
        {
            if (Config.GameOrganization == GameOrganization.OneList)
            {
                Config.SelFM.DeepCopyTo(FMsDGV.CurrentSelFM);
            }
            else // ByTab
            {
                // In case they don't match
                Config.Filter.Games = GameIndexToGame(Config.GameTab);

                Config.GameTabsState.DeepCopyTo(FMsDGV.GameTabsState);

                FMsDGV.GameTabsState.GetSelectedFM(Config.GameTab).DeepCopyTo(FMsDGV.CurrentSelFM);
                FMsDGV.GameTabsState.GetFilter(Config.GameTab).DeepCopyTo(FMsDGV.Filter);

                using (new DisableEvents(this))
                {
                    GamesTabControl.SelectedIndex = (int)Config.GameTab;
                }
            }

            // Do these even if we're not in startup, because we may have changed the game organization mode
            for (int i = 0; i < SupportedGameCount; i++)
            {
                var game = GameIndexToGame((GameIndex)i);
                _filterByGameButtonsInOrder[i].Checked = Config.Filter.Games.HasFlagFast(game);
            }

            if (!startup) ChangeFilterControlsForGameType();
        }

        public void ShowFMsListZoomButtons(bool visible)
        {
            Lazy_FMsListZoomButtons.SetVisible(this, visible);
            SetFilterBarWidth();
        }

        public void ClearUIAndCurrentInternalFilter()
        {
            using (new DisableEvents(this))
            {
                FilterBarFLP.SuspendDrawing();
                try
                {
                    bool oneList = Config.GameOrganization == GameOrganization.OneList;
                    if (oneList)
                    {
                        for (int i = 0; i < SupportedGameCount; i++)
                        {
                            _filterByGameButtonsInOrder[i].Checked = false;
                        }
                    }
                    FilterTitleTextBox.Text = "";
                    FilterAuthorTextBox.Text = "";

                    FilterByReleaseDateButton.Checked = false;
                    Lazy_ToolStripLabels.Hide(Lazy_ToolStripLabel.FilterByReleaseDate);

                    FilterByLastPlayedButton.Checked = false;
                    Lazy_ToolStripLabels.Hide(Lazy_ToolStripLabel.FilterByLastPlayed);

                    FilterByTagsButton.Checked = false;
                    FilterByFinishedButton.Checked = false;
                    FilterByUnfinishedButton.Checked = false;

                    FilterByRatingButton.Checked = false;
                    Lazy_ToolStripLabels.Hide(Lazy_ToolStripLabel.FilterByRating);

                    FilterShowUnsupportedButton.Checked = false;

                    // NOTE: Here is the line where the internal filter is cleared. It does in fact happen!
                    FMsDGV.Filter.Clear(oneList);
                }
                finally
                {
                    FilterBarFLP.ResumeDrawing();
                }
            }
        }

        #region Messageboxes

        public bool AskToContinue(string message, string title, bool noIcon = false) =>
            MessageBox.Show(
                message,
                title,
                MessageBoxButtons.YesNo,
                noIcon ? MessageBoxIcon.None : MessageBoxIcon.Warning) == DialogResult.Yes;

        public (bool Cancel, bool Continue, bool DontAskAgain)
        AskToContinueWithCancelCustomStrings(string message, string title, TaskDialogIcon? icon, bool showDontAskAgain,
                                             string yes, string no, string cancel, ButtonType? defaultButton = null)
        {
            var yesButton = new TaskDialogButton(yes);
            var noButton = new TaskDialogButton(no);
            var cancelButton = new TaskDialogButton(cancel);

            using var d = new TaskDialog(
                title: title,
                message: message,
                buttons: new[] { yesButton, noButton, cancelButton },
                defaultButton: defaultButton switch
                {
                    ButtonType.No => noButton,
                    ButtonType.Cancel => cancelButton,
                    _ => yesButton
                },
                verificationText: showDontAskAgain ? LText.AlertMessages.DontAskAgain : null,
                mainIcon: icon);

            TaskDialogButton? buttonClicked = d.ShowDialog();
            bool canceled = buttonClicked == null || buttonClicked == cancelButton;
            bool cont = buttonClicked == yesButton;
            bool dontAskAgain = d.IsVerificationChecked;
            return (canceled, cont, dontAskAgain);
        }

        public (bool Cancel, bool DontAskAgain)
        AskToContinueYesNoCustomStrings(string message, string title, TaskDialogIcon? icon, bool showDontAskAgain,
                                        string? yes, string? no, ButtonType? defaultButton = null)
        {
            var yesButton = yes != null ? new TaskDialogButton(yes) : new TaskDialogButton(ButtonType.Yes);
            var noButton = no != null ? new TaskDialogButton(no) : new TaskDialogButton(ButtonType.No);

            using var d = new TaskDialog(
                title: title,
                message: message,
                buttons: new[] { yesButton, noButton },
                defaultButton: defaultButton == ButtonType.No ? noButton : yesButton,
                verificationText: showDontAskAgain ? LText.AlertMessages.DontAskAgain : null,
                mainIcon: icon);

            bool cancel = d.ShowDialog() != yesButton;
            bool dontAskAgain = d.IsVerificationChecked;
            return (cancel, dontAskAgain);
        }

        public void ShowAlert(string message, string title) => MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Warning);

        #endregion

        public void ChangeGameTabNameShortness(bool useShort, bool refreshFilterBarPositionIfNeeded)
        {
            for (int i = 0; i < SupportedGameCount; i++)
            {
                _gameTabsInOrder[i].Text = useShort
                    ? GetShortLocalizedGameName((GameIndex)i)
                    : GetLocalizedGameName((GameIndex)i);
            }

            // Prevents the couple-pixel-high tab page from extending out too far and becoming visible
            var lastGameTabsRect = GamesTabControl.GetTabRect(GamesTabControl.TabCount - 1);
            GamesTabControl.Width = lastGameTabsRect.X + lastGameTabsRect.Width + 5;

            if (refreshFilterBarPositionIfNeeded && Config.GameOrganization == GameOrganization.ByTab)
            {
                PositionFilterBarAfterTabs();
            }
        }

        #endregion

        #region Private fields

        private FormWindowState _nominalWindowState;
        private Size _nominalWindowSize;
        private Point _nominalWindowLocation;

        private float _fMsListDefaultFontSizeInPoints;
        private int _rMsListDefaultRowHeight;

        // To order them such that we can just look them up with an index
        private readonly TabPage[] _gameTabsInOrder;
        private readonly ToolStripButtonCustom[] _filterByGameButtonsInOrder;
        private readonly TabPage[] _topRightTabsInOrder;

        private readonly Control[] _filterLabels;
        private readonly ToolStripItem[] _filtersToolStripSeparatedItems;
        private readonly Control[] _bottomAreaSeparatedItems;

        private enum KeepSel { False, True, TrueNearest }

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
        private const int _ratingImageColumnWidth = 73;
        private const int _finishedColumnWidth = 91;

        #region Bitmaps

        // We need to grab these images every time a cell is shown on the DataGridView, and pulling them from
        // Resources every time is enormously expensive, causing laggy scrolling and just generally wasting good
        // cycles. So we copy them only once to these local bitmaps, and voila, instant scrolling performance.
        private readonly Bitmap?[] GameIcons = new Bitmap?[SupportedGameCount];

        private Bitmap? BlankIcon;
        private Bitmap? CheckIcon;
        private Bitmap? RedQuestionMarkIcon;

        private Bitmap[]? StarIcons;
        private Bitmap[]? FinishedOnIcons;
        private Bitmap? FinishedOnUnknownIcon;

        #endregion

        private DataGridViewImageColumn? RatingImageColumn;

        public bool EventsDisabled { get; set; }
        public bool KeyPressesDisabled { get; set; }

        // Needed for Rating column swap to prevent a possible exception when CellValueNeeded is called in the
        // middle of the operation
        private bool _cellValueNeededDisabled;

        private TransparentPanel? ViewBlockingPanel;
        private bool _viewBlocked;

        #endregion

        #region Message handling

        protected override void WndProc(ref Message m)
        {
            // A second instance has been started and told us to show ourselves, so do it here (nicer UX).
            // This has to be in WndProc, not PreFilterMessage(). Shrug.
            if (m.Msg == InteropMisc.WM_SHOWFIRSTINSTANCE)
            {
                if (WindowState == FormWindowState.Minimized) WindowState = _nominalWindowState;
                Activate();
            }
            base.WndProc(ref m);
        }

        public bool PreFilterMessage(ref Message m)
        {
            // So I don't forget what the return values do
            const bool BlockMessage = true;
            const bool PassMessageOn = false;

            static bool TryGetHWndFromMousePos(Message msg, out IntPtr result)
            {
                Point pos = new Point(msg.LParam.ToInt32() & 0xffff, msg.LParam.ToInt32() >> 16);
                result = InteropMisc.WindowFromPoint(pos);
                return result != IntPtr.Zero && Control.FromHandle(result) != null;
            }

            // Note: CanFocus will be false if there are modal windows open

            // This allows controls to be scrolled with the mousewheel when the mouse is over them, without
            // needing to actually be focused. Vital for a good user experience.
            #region Mouse
            if (m.Msg == InteropMisc.WM_MOUSEWHEEL)
            {
                // IMPORTANT! Do this check inside each if block rather than above, because the message may not
                // be a mousemove message, and in that case we'd be trying to get a window point from a random
                // value, and that causes the min,max,close button flickering.
                if (!TryGetHWndFromMousePos(m, out IntPtr hWnd)) return PassMessageOn;

                if (_viewBlocked || CursorOutsideAddTagsDropDownArea()) return BlockMessage;

                int wParam = (int)m.WParam;
                int delta = wParam >> 16;
                if (CanFocus && CursorOverControl(FilterBarFLP) && !CursorOverControl(FMsDGV))
                {
                    // Allow the filter bar to be mousewheel-scrolled with the buttons properly appearing
                    // and disappearing as appropriate
                    if (delta != 0)
                    {
                        int direction = delta > 0 ? InteropMisc.SB_LINELEFT : InteropMisc.SB_LINERIGHT;
                        int origSmallChange = FilterBarFLP.HorizontalScroll.SmallChange;

                        FilterBarFLP.HorizontalScroll.SmallChange = 45;

                        InteropMisc.SendMessage(FilterBarFLP.Handle, InteropMisc.WM_SCROLL, (IntPtr)direction, IntPtr.Zero);

                        FilterBarFLP.HorizontalScroll.SmallChange = origSmallChange;
                    }
                }
                else if (CanFocus && CursorOverControl(FMsDGV) && (wParam & 0xFFFF) == InteropMisc.MK_CONTROL)
                {
                    if (delta != 0) ZoomFMsDGV(delta > 0 ? ZoomFMsDGVType.ZoomIn : ZoomFMsDGVType.ZoomOut);
                }
                else
                {
                    // Stupid hack to fix "send mousewheel to underlying control and block further messages"
                    // functionality still not being fully reliable. We need to focus the parent control sometimes
                    // inexplicably. Sure. Whole point is to avoid having to do that, but sure.
                    if (CursorOverControl(TopSplitContainer.Panel2))
                    {
                        TopSplitContainer.Panel2.Focus();
                    }
                    else if (CursorOverControl(MainSplitContainer.Panel2))
                    {
                        MainSplitContainer.Panel2.Focus();
                    }
                    InteropMisc.SendMessage(hWnd, m.Msg, m.WParam, m.LParam);
                }
                return BlockMessage;
            }
            else if (m.Msg == InteropMisc.WM_MOUSEHWHEEL)
            {
                if (!TryGetHWndFromMousePos(m, out _)) return PassMessageOn;

                if (_viewBlocked) return BlockMessage;

                if (CanFocus && CursorOverControl(FMsDGV))
                {
                    int delta = (int)m.WParam >> 16;
                    if (delta != 0)
                    {
                        int offset = FMsDGV.HorizontalScrollingOffset;
                        offset = delta < 0 ? (offset - 15).ClampToZero() : offset + 15;
                        FMsDGV.HorizontalScrollingOffset = offset;
                        return BlockMessage;
                    }
                }
            }
            // Just handle the NC* messages and presto, we don't even need the mouse hook anymore!
            // NC = Non-Client, ie. the mouse was in a non-client area of the control
            else if (m.Msg == InteropMisc.WM_MOUSEMOVE || m.Msg == InteropMisc.WM_NCMOUSEMOVE)
            {
                if (!CanFocus) return PassMessageOn;

                if (CursorOutsideAddTagsDropDownArea() || _viewBlocked) return BlockMessage;

                ShowReadmeControls(CursorOverReadmeArea());
            }
            else if (m.Msg == InteropMisc.WM_LBUTTONDOWN || m.Msg == InteropMisc.WM_NCLBUTTONDOWN ||
                     m.Msg == InteropMisc.WM_MBUTTONDOWN || m.Msg == InteropMisc.WM_NCMBUTTONDOWN ||
                     m.Msg == InteropMisc.WM_RBUTTONDOWN || m.Msg == InteropMisc.WM_NCRBUTTONDOWN ||
                     m.Msg == InteropMisc.WM_LBUTTONDBLCLK || m.Msg == InteropMisc.WM_NCLBUTTONDBLCLK ||
                     m.Msg == InteropMisc.WM_MBUTTONDBLCLK || m.Msg == InteropMisc.WM_NCMBUTTONDBLCLK ||
                     m.Msg == InteropMisc.WM_RBUTTONDBLCLK || m.Msg == InteropMisc.WM_NCRBUTTONDBLCLK ||
                     m.Msg == InteropMisc.WM_LBUTTONUP || m.Msg == InteropMisc.WM_NCLBUTTONUP ||
                     m.Msg == InteropMisc.WM_MBUTTONUP || m.Msg == InteropMisc.WM_NCMBUTTONUP ||
                     m.Msg == InteropMisc.WM_RBUTTONUP || m.Msg == InteropMisc.WM_NCRBUTTONUP)
            {
                if (!CanFocus) return PassMessageOn;

                if (_viewBlocked)
                {
                    return BlockMessage;
                }
                else if (CursorOutsideAddTagsDropDownArea())
                {
                    AddTagLLDropDown.HideAndClear();
                    return BlockMessage;
                }
                else if (m.Msg == InteropMisc.WM_MBUTTONDOWN && CursorOverControl(FMsDGV))
                {
                    FMsDGV.Focus();
                    if (FMsDGV.RowSelected() && !FMsDGV.SelectedRows[0].Displayed)
                    {
                        CenterSelectedFM();
                    }
                }
            }
            #endregion
            #region Keys
            // To handle alt presses, we have to handle WM_SYSKEYDOWN, which handles alt and F10. Sure why not.
            else if (m.Msg == InteropMisc.WM_SYSKEYDOWN)
            {
                int wParam = (int)m.WParam;
                if (ModifierKeys == Keys.Alt && wParam == (int)Keys.F4) return PassMessageOn;
            }
            // Any other keys have to use this.
            else if (m.Msg == InteropMisc.WM_KEYDOWN)
            {
                if (KeyPressesDisabled || _viewBlocked) return BlockMessage;

                int wParam = (int)m.WParam;
                if (wParam == (int)Keys.F1 && CanFocus)
                {
                    int stackCounter = 0;
                    bool AnyControlFocusedIn(Control control)
                    {
                        stackCounter++;
                        if (stackCounter > 100) return false;

                        if (control.Focused) return true;

                        for (int i = 0; i < control.Controls.Count; i++)
                        {
                            if (AnyControlFocusedIn(control.Controls[i])) return true;
                        }

                        return false;
                    }

                    bool AnyControlFocusedInTabPage(TabPage tabPage) =>
                        (TopRightTabControl.Focused && TopRightTabControl.SelectedTab == tabPage) ||
                        AnyControlFocusedIn(tabPage);

                    string section =
                        !EverythingPanel.Enabled ? HelpSections.MainWindow :
                        FMsDGV.FMContextMenuVisible ? HelpSections.FMContextMenu :
                        FMsDGV.ColumnHeaderMenuVisible ? HelpSections.ColumnHeaderContextMenu :
                        // TODO: We could try to be clever and take mouse position into account in some cases?
                        AnyControlFocusedIn(TopSplitContainer.Panel1) ? HelpSections.MissionList :
                        TopRightMenuButton.Focused || TopRightLLMenu.Focused || AnyControlFocusedInTabPage(StatisticsTabPage) ? HelpSections.StatsTab :
                        AnyControlFocusedInTabPage(EditFMTabPage) ? HelpSections.EditFMTab :
                        AnyControlFocusedInTabPage(CommentTabPage) ? HelpSections.CommentTab :
                        // Add tag dropdown is in EverythingPanel, not tags tab page
                        AnyControlFocusedInTabPage(TagsTabPage) || AddTagLLDropDown.Focused ? HelpSections.TagsTab :
                        AnyControlFocusedInTabPage(PatchTabPage) ? HelpSections.PatchTab :
                        AnyControlFocusedIn(MainSplitContainer.Panel2) ? HelpSections.ReadmeArea :
                        // TODO: Handle bottom area controls (we need a whole other section delimiter in the help file)
                        HelpSections.MainWindow;

                    Core.OpenHelpFile(section);
                }
            }
            else if (m.Msg == InteropMisc.WM_KEYUP)
            {
                if (KeyPressesDisabled || _viewBlocked) return BlockMessage;
            }
            #endregion

            return PassMessageOn;
        }

        #endregion

        #region Init / load / show

        // InitializeComponent() (and stuff that doesn't do anything) only - for everything else use the init
        // method(s) below
        public MainForm()
        {
#if DEBUG
            // The debug path - the standard designer-generated method with tons of bloat and redundant value
            // setting, immediate initialization, etc.
            // This path supports working with the designer.
            InitializeComponent();
#else
            // The fast path - a custom method with all or most cruft stripped out, copied by hand from the
            // designer-generated method and tweaked as I see fit for speed and lazy-loading support.
            // This path doesn't support working with the designer, or at least shouldn't be trusted to do so.
            InitComponentManual();
#endif

#if DEBUG || (Release_Testing && !RT_StartupOnly)
            #region Init debug-only controls

            TestButton = new Button();
            Test2Button = new Button();
            DebugLabel = new Label();
            DebugLabel2 = new Label();

            BottomPanel.Controls.Add(TestButton);
            BottomPanel.Controls.Add(Test2Button);
            BottomPanel.Controls.Add(DebugLabel);
            BottomPanel.Controls.Add(DebugLabel2);

            TestButton.Location = new Point(640, 0);
            TestButton.Size = new Size(75, 22);
            TestButton.TabIndex = 999;
            TestButton.Text = @"Test";
            TestButton.UseVisualStyleBackColor = true;
            TestButton.Click += TestButton_Click;

            Test2Button.Location = new Point(640, 21);
            Test2Button.Size = new Size(75, 22);
            Test2Button.TabIndex = 999;
            Test2Button.Text = @"Test2";
            Test2Button.UseVisualStyleBackColor = true;
            Test2Button.Click += Test2Button_Click;

            DebugLabel.AutoSize = true;
            DebugLabel.Location = new Point(720, 8);
            DebugLabel.Size = new Size(71, 13);
            DebugLabel.TabIndex = 29;
            DebugLabel.Text = @"[DebugLabel]";

            DebugLabel2.AutoSize = true;
            DebugLabel2.Location = new Point(720, 24);
            DebugLabel2.Size = new Size(77, 13);
            DebugLabel2.TabIndex = 32;
            DebugLabel2.Text = @"[DebugLabel2]";

            #endregion
#endif

#if !ReleaseBeta && !ReleasePublic
            ForceWindowedCheckBox = new CheckBox();
            BottomRightButtonsFLP.Controls.Add(ForceWindowedCheckBox);
            ForceWindowedCheckBox.Dock = DockStyle.Fill;
            ForceWindowedCheckBox.Text = @"Force windowed";
            ForceWindowedCheckBox.CheckedChanged += ForceWindowedCheckBox_CheckedChanged;
#endif

            // -------- New games go here!
            // @GENGAMES (tabs and filter buttons): Begin
            _gameTabsInOrder = new[]
            {
                Thief1TabPage,
                Thief2TabPage,
                Thief3TabPage,
                SS2TabPage
            };
            _filterByGameButtonsInOrder = new[]
            {
                FilterByThief1Button,
                FilterByThief2Button,
                FilterByThief3Button,
                FilterBySS2Button
            };
            // @GENGAMES (tabs and filter buttons): End

            // Putting these into a list whose order matches the enum allows us to just iterate the list without
            // naming any specific tab page. This greatly minimizes the number of places we'll need to add code
            // when we add new tab pages.
            _topRightTabsInOrder = new[]
            {
                StatisticsTabPage,
                EditFMTabPage,
                CommentTabPage,
                TagsTabPage,
                PatchTabPage
            };

            #region Separated items

            _filterLabels = new Control[]
            {
                FilterTitleLabel,
                FilterAuthorLabel
            };

            _filtersToolStripSeparatedItems = new ToolStripItem[]
            {
                FilterByReleaseDateButton,
                FilterByLastPlayedButton,
                FilterByTagsButton,
                FilterByFinishedButton,
                FilterByRatingButton,
                FilterShowUnsupportedButton,
                FilterShowRecentAtTopButton
            };

            _bottomAreaSeparatedItems = new Control[]
            {
                ScanAllFMsButton,
                WebSearchButton
            };

            #endregion
        }

        // In early development, I had some problems with putting init stuff in the constructor, where all manner
        // of nasty random behavior would happen. Not sure if that was because of something specific I was doing
        // wrong or what, but I have this init method now that comfortably runs after the ctor. Shrug.
        // MT: On startup only, this is run in parallel with FindFMs.Find()
        // So don't touch anything the other touches: anything affecting preset tags or the FMs list.
        public void InitThreadable()
        {
            Text = @"AngelLoader " + Application.ProductVersion;

            FMsDGV.InjectOwner(this);

            #region Set up form and control state

            // Set here in init method so as to avoid the changes being visible.
            // Set here specifically (before anything else) so that splitter positioning etc. works right.
            SetWindowStateAndSize();

            // Allows shortcut keys to be detected globally (selected control doesn't affect them)
            KeyPreview = true;

            #region Top-right tabs

            var sortedTabPages = new SortedDictionary<int, TabPage>();
            for (int i = 0; i < TopRightTabsData.Count; i++)
            {
                sortedTabPages.Add(Config.TopRightTabsData.Tabs[i].DisplayIndex, _topRightTabsInOrder[i]);
            }

            var tabs = new List<TabPage>(sortedTabPages.Count);
            foreach (var item in sortedTabPages) tabs.Add(item.Value);

            // This removes any existing tabs so it's fine even for debug
            TopRightTabControl.SetTabsFull(tabs);

            for (int i = 0; i < TopRightTabsData.Count; i++)
            {
                TopRightTabControl.ShowTab(_topRightTabsInOrder[i], Config.TopRightTabsData.Tabs[i].Visible);
                TopRightLLMenu.SetItemChecked(i, Config.TopRightTabsData.Tabs[i].Visible);
            }

            #endregion

            #region SplitContainers

            MainSplitContainer.SetSplitterPercent(Config.MainSplitterPercent, suspendResume: false);
            TopSplitContainer.SetSplitterPercent(Config.TopSplitterPercent, suspendResume: false);

            MainSplitContainer.InjectSibling(TopSplitContainer);
            TopSplitContainer.InjectSibling(MainSplitContainer);

            #endregion

            #region FMs DataGridView

            _fMsListDefaultFontSizeInPoints = FMsDGV.DefaultCellStyle.Font.SizeInPoints;
            _rMsListDefaultRowHeight = FMsDGV.RowTemplate.Height;

            #region Columns

            FinishedColumn.Width = _finishedColumnWidth;

            // The other Rating column, there has to be two, one for text and one for images
            RatingImageColumn = new DataGridViewImageColumn
            {
                ImageLayout = DataGridViewImageCellLayout.Zoom,
                ReadOnly = true,
                Width = _ratingImageColumnWidth,
                Resizable = DataGridViewTriState.False,
                SortMode = DataGridViewColumnSortMode.Programmatic
            };

            UpdateRatingListsAndColumn(Config.RatingDisplayStyle == RatingDisplayStyle.FMSel, startup: true);

            FMsDGV.SetColumnData(Config.Columns);

            #endregion

            #endregion

            #region Readme area

            // Set both at once to avoid an elusive bug that happens when you start up, the readme is blank, then
            // you shut down without loading a readme, whereupon it will save out ZoomFactor which is still 1.0.
            // You can't just save out StoredZoomFactor either because it doesn't change when the user zooms, as
            // there's no event for that. Fun.
            ReadmeRichTextBox.SetAndStoreZoomFactor(Config.ReadmeZoomFactor);

            #endregion

            #region Filters

            FilterBarFLP.HorizontalScroll.SmallChange = 20;

            Config.Filter.DeepCopyTo(FMsDGV.Filter);
            SetUIFilterValues(FMsDGV.Filter);

            #endregion

            FilterShowRecentAtTopButton.Checked = Config.ShowRecentAtTop;

            // EnsureValidity() guarantees selected tab will not be invisible
            for (int i = 0; i < TopRightTabsData.Count; i++)
            {
                if ((int)Config.TopRightTabsData.SelectedTab == i)
                {
                    TopRightTabControl.SelectedTab = _topRightTabsInOrder[i];
                    break;
                }
            }

            if (!Config.HideUninstallButton) InstallUninstallFMLLButton.Construct(this);

            TopSplitContainer.CollapsedSize = TopRightCollapseButton.Width;
            if (Config.TopRightPanelCollapsed)
            {
                TopSplitContainer.SetFullScreen(true, suspendResume: false);
                SetTopRightCollapsedState();
            }

            #endregion

            // Set these here because they depend on the splitter positions
            Localize(startup: true);

            if (Math.Abs(Config.FMsListFontSizeInPoints - FMsDGV.DefaultCellStyle.Font.SizeInPoints) >= 0.001)
            {
                ZoomFMsDGV(ZoomFMsDGVType.ZoomToHeightOnly, Config.FMsListFontSizeInPoints);
            }

            ChangeGameOrganization(startup: true);
        }

        // This one can't be multithreaded because it depends on the FMs list
        public async Task FinishInitAndShow(List<int>? fmsViewListUnscanned)
        {
            if (Visible) return;

            // Sort the list here because InitThreadable() is run in parallel to FindFMs.Find() but sorting needs
            // Find() to have been run first.
            SortFMsDGV(Config.SortedColumn, Config.SortDirection);

            // This await call takes 15ms just to make the call alone(?!) so don't do it unless we have to
            if (fmsViewListUnscanned?.Count > 0)
            {
                Show();
                await FMScan.ScanNewFMs(fmsViewListUnscanned);
            }

            Core.SetFilter();
            if (RefreshFMsList(FMsDGV.CurrentSelFM, startup: true, KeepSel.TrueNearest))
            {
                await DisplaySelectedFM(refreshReadme: true);
            }

            FMsDGV.Focus();

            if (!Visible) Show();

#if !ReleasePublic
            //if (Config.CheckForUpdatesOnStartup) await CheckUpdates.Check();
#endif
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

            _nominalWindowState = Config.MainWindowState;
            _nominalWindowSize = Config.MainWindowSize;
            _nominalWindowLocation = new Point(loc.X, loc.Y);
        }

        public void ShowOnly()
        {
            if (!Visible) Show();
        }

        #endregion

        #region Form events

        private void MainForm_Load(object sender, EventArgs e)
        {
            // These have to go here because they depend on and/or affect the width of other controls, and we
            // need to be in a state where layout is happening
            ChangeFilterControlsForGameType();
            ShowFMsListZoomButtons(!Config.HideFMListZoomButtons);

            Application.AddMessageFilter(this);
        }

        [SuppressMessage("ReSharper", "MemberCanBeMadeStatic.Local")]
        private void MainForm_Shown(object sender, EventArgs e)
        {
            // debug - end of startup - to make sure when we profile, we're measuring only startup time
#if RT_StartupOnly
            // Regular Environment.Exit() because we're testing speed
            Environment.Exit(1);
            return;
#endif
        }

        private void MainForm_Deactivate(object sender, EventArgs e) => CancelResizables();

        private void MainForm_SizeChanged(object sender, EventArgs e)
        {
            // TODO: Make it so window docking doesn't count as changing the normal window dimensions
            if (WindowState != FormWindowState.Minimized)
            {
                _nominalWindowState = WindowState;
                if (WindowState != FormWindowState.Maximized)
                {
                    _nominalWindowSize = Size;
                    _nominalWindowLocation = new Point(Location.X, Location.Y);
                }
            }

            if (AddTagLLDropDown.Visible) AddTagLLDropDown.HideAndClear();
        }

        private void MainForm_LocationChanged(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Normal) _nominalWindowLocation = new Point(Location.X, Location.Y);
        }

        private void CancelResizables()
        {
            FMsDGV.CancelColumnResize();
            MainSplitContainer.CancelResize();
            TopSplitContainer.CancelResize();
        }

        private async void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (KeyPressesDisabled) return;

            void SelectAndSuppress(int index)
            {
                FMsDGV.Rows[index].Selected = true;
                FMsDGV.SelectProperly();
                e.SuppressKeyPress = true;
            }

            // Let user use Home+End keys to navigate a filter textbox if it's focused, even if the mouse is over
            // the FMs list
            if ((FilterTitleTextBox.Focused || FilterAuthorTextBox.Focused) &&
                (e.KeyCode == Keys.Home || e.KeyCode == Keys.End))
            {
                return;
            }

            if (e.KeyCode == Keys.Enter)
            {
                if (FMsDGV.Focused && FMsDGV.RowSelected() && GameIsKnownAndSupported(FMsDGV.GetSelectedFM().Game))
                {
                    e.SuppressKeyPress = true;
                    await FMInstallAndPlay.InstallIfNeededAndPlay(FMsDGV.GetSelectedFM(), askConfIfRequired: true);
                }
            }
            else if (e.KeyCode == Keys.Delete)
            {
                if (FMsDGV.Focused && FMsDGV.RowSelected())
                {
                    await FMArchives.Delete(FMsDGV.GetSelectedFM());
                }
            }
            else if (e.KeyCode == Keys.Escape)
            {
                CancelResizables();

                AddTagLLDropDown.HideAndClear();

                // Easy way to "get out" of the filter if you want to use Home and End again
                if (FilterTitleTextBox.Focused || FilterAuthorTextBox.Focused)
                {
                    FMsDGV.Focus();
                }
            }
            #region FMsDGV nav
            else if (e.KeyCode == Keys.Home || (e.Control && e.KeyCode == Keys.Up))
            {
                if (FMsDGV.RowSelected() && (FMsDGV.Focused || CursorOverControl(FMsDGV)))
                {
                    SelectAndSuppress(0);
                }
            }
            else if (e.KeyCode == Keys.End || (e.Control && e.KeyCode == Keys.Down))
            {
                if (FMsDGV.RowSelected() && (FMsDGV.Focused || CursorOverControl(FMsDGV)))
                {
                    SelectAndSuppress(FMsDGV.RowCount - 1);
                }
            }
            // The key suppression is to stop FMs being reloaded when the selection hasn't changed (perf)
            else if (e.KeyCode == Keys.PageUp || e.KeyCode == Keys.Up)
            {
                if (FMsDGV.RowSelected() && (FMsDGV.Focused || CursorOverControl(FMsDGV)))
                {
                    if (FMsDGV.SelectedRows[0].Index == 0)
                    {
                        SelectAndSuppress(0);
                    }
                    else
                    {
                        FMsDGV.SendKeyDown(e);
                        e.SuppressKeyPress = true;
                    }
                }
            }
            else if (e.KeyCode == Keys.PageDown || e.KeyCode == Keys.Down)
            {
                if (FMsDGV.RowSelected() && (FMsDGV.Focused || CursorOverControl(FMsDGV)))
                {
                    if (FMsDGV.SelectedRows[0].Index == FMsDGV.RowCount - 1)
                    {
                        SelectAndSuppress(FMsDGV.RowCount - 1);
                    }
                    else
                    {
                        FMsDGV.SendKeyDown(e);
                        e.SuppressKeyPress = true;
                    }
                }
            }
            else if (e.KeyCode == Keys.F5)
            {
                if (FMsDGV.Focused || CursorOverControl(FMsDGV))
                {
                    if (e.Shift && !e.Control && !e.Alt)
                    {
                        await Core.RefreshFMsListFromDisk();
                        e.SuppressKeyPress = true;
                    }
                    else if (!e.Shift)
                    {
                        await SortAndSetFilter();
                        e.SuppressKeyPress = true;
                    }
                }
            }
            #endregion
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
            // TODO: I only block the view during zip extracts, which are pretty quick.
            // Do I really want to put up this dialog during that situation?
            if (!EverythingPanel.Enabled || _viewBlocked)
            {
                MessageBox.Show(LText.AlertMessages.AppClosing_OperationInProgress, LText.AlertMessages.Alert,
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                e.Cancel = true;
                return;
            }

            Application.RemoveMessageFilter(this);

            // Argh, stupid hack to get this to not run TWICE on Application.Exit()
            // Application.Exit() is the worst thing ever. Before closing it just does whatever the hell it wants.
            FormClosing -= MainForm_FormClosing;

            UpdateConfig();
            Core.Shutdown();
        }

        #endregion

        private void Localize(bool startup)
        {
            // Certain controls' text depends on FM state. Because this could be run after startup, we need to
            // make sure those controls' text is set correctly.
            FanMission? selFM = FMsDGV.RowSelected() ? FMsDGV.GetSelectedFM() : null;

            if (!startup)
            {
                this.SuspendDrawing();
            }
            else
            {
                // PERF: These will already have been suspended in InitComponentManual(), and we're going to
                // resume them in the finally block
#if DEBUG
                BottomLeftButtonsFLP.SuspendLayout();
                BottomRightButtonsFLP.SuspendLayout();
                StatisticsTabPage.SuspendLayout();
                StatsCheckBoxesPanel.SuspendLayout();
                EditFMTabPage.SuspendLayout();
                CommentTabPage.SuspendLayout();
                TagsTabPage.SuspendLayout();
                AddRemoveTagFLP.SuspendLayout();
                PatchMainPanel.SuspendLayout();
                MainSplitContainer.Panel2.SuspendLayout();
                ChooseReadmeLLPanel.SuspendPanelLayout();
#endif
            }
            try
            {
                #region Game tabs

                ChangeGameTabNameShortness(Config.UseShortGameTabNames, false);

                #endregion

                #region Filter bar

                // Don't do this on startup, cause we're already going to do it afterward
                if (!startup && Config.GameOrganization == GameOrganization.ByTab)
                {
                    PositionFilterBarAfterTabs();
                }

                for (int i = 0; i < SupportedGameCount; i++)
                {
                    _filterByGameButtonsInOrder[i].ToolTipText = GetLocalizedGameName((GameIndex)i);
                }

                FilterTitleLabel.Text = LText.FilterBar.Title;
                FilterAuthorLabel.Text = LText.FilterBar.Author;

                FilterByReleaseDateButton.ToolTipText = LText.FilterBar.ReleaseDateToolTip;
                Lazy_ToolStripLabels.Localize(Lazy_ToolStripLabel.FilterByReleaseDate);

                FilterByLastPlayedButton.ToolTipText = LText.FilterBar.LastPlayedToolTip;
                Lazy_ToolStripLabels.Localize(Lazy_ToolStripLabel.FilterByLastPlayed);

                FilterByTagsButton.ToolTipText = LText.FilterBar.TagsToolTip;
                FilterByFinishedButton.ToolTipText = LText.FilterBar.FinishedToolTip;
                FilterByUnfinishedButton.ToolTipText = LText.FilterBar.UnfinishedToolTip;

                FilterByRatingButton.ToolTipText = LText.FilterBar.RatingToolTip;

                // This one is tricky - it could have LText.Global.None as part of its text. Finally caught!
                if (startup)
                {
                    Lazy_ToolStripLabels.Localize(Lazy_ToolStripLabel.FilterByRating);
                }
                else
                {
                    UpdateRatingLabel();
                }

                FilterShowUnsupportedButton.ToolTipText = LText.FilterBar.ShowUnsupported;
                FilterShowRecentAtTopButton.ToolTipText = LText.FilterBar.ShowRecentAtTop;

                #endregion

                #region Clear/refresh/reset area

                RefreshFromDiskButton.ToolTipText = LText.FilterBar.RefreshFromDiskButtonToolTip;
                RefreshFiltersButton.ToolTipText = LText.FilterBar.RefreshFilteredListButtonToolTip;
                ClearFiltersButton.ToolTipText = LText.FilterBar.ClearFiltersButtonToolTip;
                MainToolTip.SetToolTip(ResetLayoutButton, LText.FilterBar.ResetLayoutButtonToolTip);

                #endregion

                #region FMs list

                FMsDGV.Localize();

                Lazy_FMsListZoomButtons.Localize();

                #region Columns

                GameTypeColumn.HeaderText = LText.FMsList.GameColumn;
                InstalledColumn.HeaderText = LText.FMsList.InstalledColumn;
                TitleColumn.HeaderText = LText.FMsList.TitleColumn;
                ArchiveColumn.HeaderText = LText.FMsList.ArchiveColumn;
                AuthorColumn.HeaderText = LText.FMsList.AuthorColumn;
                SizeColumn.HeaderText = LText.FMsList.SizeColumn;
                RatingTextColumn.HeaderText = LText.FMsList.RatingColumn;
                RatingImageColumn!.HeaderText = LText.FMsList.RatingColumn;
                FinishedColumn.HeaderText = LText.FMsList.FinishedColumn;
                ReleaseDateColumn.HeaderText = LText.FMsList.ReleaseDateColumn;
                LastPlayedColumn.HeaderText = LText.FMsList.LastPlayedColumn;
                DateAddedColumn.HeaderText = LText.FMsList.DateAddedColumn;
                DisabledModsColumn.HeaderText = LText.FMsList.DisabledModsColumn;
                CommentColumn.HeaderText = LText.FMsList.CommentColumn;

                #endregion

                #endregion

                PlayOriginalGameLLMenu.Localize();

                #region Top-right tabs area

                #region Show/hide tabs menu

                TopRightLLMenu.Localize();

                #endregion

                #region Statistics tab

                StatisticsTabPage.Text = LText.StatisticsTab.TabText;

                CustomResourcesLabel.Text =
                    selFM == null ? LText.StatisticsTab.NoFMSelected :
                    selFM.Game == Game.Thief3 ? LText.StatisticsTab.CustomResourcesNotSupportedForThief3 :
                    selFM.ResourcesScanned ? LText.StatisticsTab.CustomResources :
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

                StatsScanCustomResourcesButton.Text = LText.StatisticsTab.RescanCustomResources;

                #endregion

                #region Edit FM tab

                EditFMTabPage.Text = LText.EditFMTab.TabText;
                EditFMTitleLabel.Text = LText.EditFMTab.Title;
                EditFMAuthorLabel.Text = LText.EditFMTab.Author;
                EditFMReleaseDateCheckBox.Text = LText.EditFMTab.ReleaseDate;
                EditFMLastPlayedCheckBox.Text = LText.EditFMTab.LastPlayed;
                EditFMRatingLabel.Text = LText.EditFMTab.Rating;

                // For some reason this counts as a selected index change?!
                using (new DisableEvents(this))
                {
                    EditFMRatingComboBox.Items[0] = LText.Global.Unrated;
                    if (EditFMLanguageComboBox.Items.Count > 0 &&
                        EditFMLanguageComboBox.BackingItems[0].EqualsI(FMLanguages.DefaultLangKey))
                    {
                        EditFMLanguageComboBox.Items[0] = LText.EditFMTab.DefaultLanguage;
                    }
                }

                EditFMFinishedOnButton.Text = LText.EditFMTab.FinishedOn;
                EditFMDisabledModsLabel.Text = LText.EditFMTab.DisabledMods;
                EditFMDisableAllModsCheckBox.Text = LText.EditFMTab.DisableAllMods;

                MainToolTip.SetToolTip(EditFMScanTitleButton, LText.EditFMTab.RescanTitleToolTip);
                MainToolTip.SetToolTip(EditFMScanAuthorButton, LText.EditFMTab.RescanAuthorToolTip);
                MainToolTip.SetToolTip(EditFMScanReleaseDateButton, LText.EditFMTab.RescanReleaseDateToolTip);
                MainToolTip.SetToolTip(EditFMScanLanguagesButton, LText.EditFMTab.RescanLanguages);

                EditFMLanguageLabel.Text = LText.EditFMTab.PlayFMInThisLanguage;

                EditFMScanForReadmesButton.Text = LText.EditFMTab.RescanForReadmes;

                #endregion

                #region Comment tab

                CommentTabPage.Text = LText.CommentTab.TabText;

                #endregion

                #region Tags tab

                TagsTabPage.Text = LText.TagsTab.TabText;
                AddTagButton.SetTextForTextBoxButtonCombo(AddTagTextBox, LText.TagsTab.AddTag);
                AddTagFromListButton.Text = LText.TagsTab.AddFromList;
                RemoveTagButton.Text = LText.TagsTab.RemoveTag;

                #endregion

                #region Patch tab

                PatchTabPage.Text = LText.PatchTab.TabText;
                PatchDMLPatchesLabel.Text = LText.PatchTab.DMLPatchesApplied;
                MainToolTip.SetToolTip(PatchAddDMLButton, LText.PatchTab.AddDMLPatchToolTip);
                MainToolTip.SetToolTip(PatchRemoveDMLButton, LText.PatchTab.RemoveDMLPatchToolTip);
                PatchFMNotInstalledLabel.Text = LText.PatchTab.FMNotInstalled;
                PatchFMNotInstalledLabel.CenterHV(PatchTabPage);
                PatchOpenFMFolderButton.Text = LText.PatchTab.OpenFMFolder;

                #endregion

                #endregion

                #region Readme area

                MainToolTip.SetToolTip(ReadmeZoomInButton, LText.ReadmeArea.ZoomInToolTip);
                MainToolTip.SetToolTip(ReadmeZoomOutButton, LText.ReadmeArea.ZoomOutToolTip);
                MainToolTip.SetToolTip(ReadmeResetZoomButton, LText.ReadmeArea.ResetZoomToolTip);
                MainToolTip.SetToolTip(ReadmeFullScreenButton, LText.ReadmeArea.FullScreenToolTip);

                ViewHTMLReadmeLLButton.Localize();

                ChooseReadmeLLPanel.Localize();

                #endregion

                #region Bottom area

                PlayFMButton.Text = LText.MainButtons.PlayFM;

                // Allow button to do its max-string-length layout thing
                if (startup && !Config.HideUninstallButton) BottomLeftButtonsFLP.ResumeLayout();
                InstallUninstallFMLLButton.Localize(startup);
                if (startup && !Config.HideUninstallButton) BottomLeftButtonsFLP.SuspendLayout();

                PlayOriginalGameButton.Text = LText.MainButtons.PlayOriginalGame;
                WebSearchButton.Text = LText.MainButtons.WebSearch;
                ScanAllFMsButton.Text = LText.MainButtons.ScanAllFMs;
                ImportButton.Text = LText.MainButtons.Import;
                SettingsButton.Text = LText.MainButtons.Settings;

                #endregion

                LocalizeProgressBox();
            }
            finally
            {
                if (!startup)
                {
                    this.ResumeDrawing();
                }
                else
                {
                    BottomLeftButtonsFLP.ResumeLayout();
                    BottomRightButtonsFLP.ResumeLayout();
                    StatisticsTabPage.ResumeLayout();
                    StatsCheckBoxesPanel.ResumeLayout();
                    EditFMTabPage.ResumeLayout();
                    CommentTabPage.ResumeLayout();
                    TagsTabPage.ResumeLayout();
                    AddRemoveTagFLP.ResumeLayout();
                    PatchMainPanel.ResumeLayout();
                    MainSplitContainer.Panel2.ResumeLayout();
                    ChooseReadmeLLPanel.ResumePanelLayout();
                }

                // We can't do this while the layout is suspended, because then it won't have the right dimensions
                // for centering
                ViewHTMLReadmeLLButton.Center(MainSplitContainer.Panel2);
            }

            // To refresh the FM size column strings to localized
            // We don't need to refresh on startup because we already will later
            if (!startup) RefreshFMsListKeepSelection();
        }

        // Separate so we can call it from _Load on startup (because it needs the form to be loaded to layout the
        // controls properly) but keep the rest of the work before load
        private void ChangeFilterControlsForGameType()
        {
            if (Config.GameOrganization == GameOrganization.OneList)
            {
                GamesTabControl.Hide();
                // Don't inline this var - it stores the X value to persist it through a change
                int plusWidth = FilterBarFLP.Location.X;
                FilterBarFLP.Location = new Point(0, FilterBarFLP.Location.Y);
                FilterBarFLP.Width += plusWidth;
                FilterGameButtonsToolStrip.Show();
            }
            else // ByTab
            {
                PositionFilterBarAfterTabs();

                FilterGameButtonsToolStrip.Hide();
                GamesTabControl.Show();
            }

            SetFilterBarScrollButtons();
        }

        private void UpdateConfig()
        {
            GameIndex gameTab = Thief1;
            if (Config.GameOrganization == GameOrganization.ByTab)
            {
                SaveCurrentTabSelectedFM(GamesTabControl.SelectedTab);
                var selGameTab = GamesTabControl.SelectedTab;
                for (int i = 0; i < SupportedGameCount; i++)
                {
                    if (_gameTabsInOrder[i] == selGameTab)
                    {
                        gameTab = (GameIndex)i;
                        break;
                    }
                }
            }

            SelectedFM selectedFM = FMsDGV.GetSelectedFMPosInfo();

            var topRightTabs = new TopRightTabsData
            {
                SelectedTab = (TopRightTab)Array.IndexOf(_topRightTabsInOrder, TopRightTabControl.SelectedTab)
            };

            for (int i = 0; i < TopRightTabsData.Count; i++)
            {
                topRightTabs.Tabs[i].DisplayIndex = TopRightTabControl.GetTabDisplayIndex(_topRightTabsInOrder[i]);
                topRightTabs.Tabs[i].Visible = TopRightTabControl.Contains(_topRightTabsInOrder[i]);
            }

            #region Quick hack to prevent splitter distances from freaking out if we're closing while minimized

            FormWindowState nominalState = _nominalWindowState;

            bool minimized = false;
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
                _nominalWindowState,
                _nominalWindowSize,
                _nominalWindowLocation,
                mainSplitterPercent,
                topSplitterPercent,
                FMsDGV.GetColumnData(),
                FMsDGV.CurrentSortedColumn,
                FMsDGV.CurrentSortDirection,
                FMsDGV.DefaultCellStyle.Font.SizeInPoints,
                FMsDGV.Filter,
                selectedFM,
                FMsDGV.GameTabsState,
                gameTab,
                topRightTabs,
                TopSplitContainer.FullScreen,
                ReadmeRichTextBox.ZoomFactor);
        }

        #region Cursor over area detection

        private bool CursorOverReadmeArea()
        {
            return ReadmeRichTextBox.Visible ? CursorOverControl(ReadmeRichTextBox) :
                ViewHTMLReadmeLLButton.Visible && CursorOverControl(MainSplitContainer.Panel2);
        }

        // Standard Windows drop-down behavior: nothing else responds until the drop-down closes
        private bool CursorOutsideAddTagsDropDownArea()
        {
            // Check Visible first, otherwise we might be passing a null ref!
            return AddTagLLDropDown.Visible &&
                   // Check Size instead of ClientSize in order to support clicking the scroll bar
                   !CursorOverControl(AddTagLLDropDown.ListBox, fullArea: true) &&
                   !CursorOverControl(AddTagTextBox) &&
                   !CursorOverControl(AddTagButton);
        }

        private bool CursorOverControl(Control control, bool fullArea = false)
        {
            if (!control.Visible || !control.Enabled) return false;

            // Don't create eleventy billion Rectangle objects per second
            Point rpt = PointToClient(control.PointToScreen(new Point(0, 0)));
            Size rcs = fullArea ? control.Size : control.ClientSize;
            Point ptc = PointToClient(Cursor.Position);
            return ptc.X >= rpt.X && ptc.X < rpt.X + rcs.Width &&
                   ptc.Y >= rpt.Y && ptc.Y < rpt.Y + rcs.Height;
        }

        #endregion

        #region FMsDGV-related

        public void SetRowCount(int count) => FMsDGV.RowCount = count;

        private void ZoomFMsDGV(ZoomFMsDGVType type, float? zoomFontSize = null)
        {
            // No goal escapes me, mate

            SelectedFM? selFM = FMsDGV.RowSelected() ? FMsDGV.GetSelectedFMPosInfo() : null;

            Font f = FMsDGV.DefaultCellStyle.Font;

            // Set zoom level
            float fontSize =
                type == ZoomFMsDGVType.ZoomIn ? f.SizeInPoints + 1.0f :
                type == ZoomFMsDGVType.ZoomOut ? f.SizeInPoints - 1.0f :
                type == ZoomFMsDGVType.ZoomTo && zoomFontSize != null ? (float)zoomFontSize :
                type == ZoomFMsDGVType.ZoomToHeightOnly && zoomFontSize != null ? (float)zoomFontSize :
                _fMsListDefaultFontSizeInPoints;

            // Clamp zoom level
            if (fontSize < Math.Round(1.00f, 2)) fontSize = 1.00f;
            if (fontSize > Math.Round(41.25f, 2)) fontSize = 41.25f;
            fontSize = (float)Math.Round(fontSize, 2);

            // Set new font size
            Font newF = new Font(f.FontFamily, fontSize, f.Style, f.Unit, f.GdiCharSet, f.GdiVerticalFont);

            // Set row height based on font plus some padding
            int rowHeight = type == ZoomFMsDGVType.ResetZoom ? _rMsListDefaultRowHeight : newF.Height + 9;

            // If we're on startup, then the widths will already have been restored (to zoomed size) from the
            // config
            bool heightOnly = type == ZoomFMsDGVType.ZoomToHeightOnly;

            // Must be done first, else we get wrong values
            List<double> widthMul = new List<double>();
            foreach (DataGridViewColumn c in FMsDGV.Columns)
            {
                Size size = c.HeaderCell.Size;
                widthMul.Add((double)size.Width / size.Height);
            }

            // Set font on cells
            FMsDGV.DefaultCellStyle.Font = newF;

            // Set font on headers
            FMsDGV.ColumnHeadersDefaultCellStyle.Font = newF;

            // Set height on all rows (but it won't take effect yet)
            FMsDGV.RowTemplate.Height = rowHeight;

            // Save previous selection
            int selIndex = FMsDGV.RowSelected() ? FMsDGV.SelectedRows[0].Index : -1;
            using (new DisableEvents(this))
            {
                // Force a regeneration of rows (height will take effect here)
                int rowCount = FMsDGV.RowCount;
                FMsDGV.RowCount = 0;
                FMsDGV.RowCount = rowCount;

                // Restore previous selection (no events will be fired, due to being in a DisableEvents block)
                if (selIndex > -1)
                {
                    FMsDGV.Rows[selIndex].Selected = true;
                    FMsDGV.SelectProperly();
                }

                // Set column widths (keeping ratio to height)
                for (int i = 0; i < FMsDGV.Columns.Count; i++)
                {
                    DataGridViewColumn c = FMsDGV.Columns[i];

                    // Complicated gobbledegook for handling different options and also special-casing the
                    // non-resizable columns
                    bool reset = type == ZoomFMsDGVType.ResetZoom;
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
                            c.Width = _ratingImageColumnWidth;
                        }
                        else if (reset && c == FinishedColumn)
                        {
                            c.Width = _finishedColumnWidth;
                        }
                        else
                        {
                            // The ever-present rounding errors creep in here, but meh. I should figure out
                            // how to not have those - ensure scaling always happens in integral pixel counts
                            // somehow?
                            c.Width = reset && Math.Abs(Config.FMsListFontSizeInPoints - _fMsListDefaultFontSizeInPoints) < 0.1
                                ? Config.Columns[i].Width
                                : (int)Math.Ceiling(c.HeaderCell.Size.Height * widthMul[i]);
                        }
                    }
                }
            }

            // Keep selected FM in the center of the list vertically where possible (UX nicety)
            if (selIndex > -1 && selFM != null) CenterSelectedFM();

            // And that's how you do it
        }

        private void CenterSelectedFM()
        {
            try
            {
                FMsDGV.FirstDisplayedScrollingRowIndex =
                    (FMsDGV.SelectedRows[0].Index - (FMsDGV.DisplayedRowCount(true) / 2))
                    .Clamp(0, FMsDGV.RowCount - 1);
            }
            catch
            {
                // no room is available to display rows
            }
        }

        private void SortFMsDGV(Column column, SortOrder sortDirection)
        {
            FMsDGV.CurrentSortedColumn = column;
            FMsDGV.CurrentSortDirection = sortDirection;

            Core.SortFMsViewList(column, sortDirection);

            // Perf: doing it this way is significantly faster than the old method of indiscriminately setting
            // all columns to None and then setting the current one back to the CurrentSortDirection glyph again
            int intCol = (int)column;
            for (int i = 0; i < FMsDGV.Columns.Count; i++)
            {
                DataGridViewColumn c = FMsDGV.Columns[i];
                if (i == intCol && c.HeaderCell.SortGlyphDirection != FMsDGV.CurrentSortDirection)
                {
                    c.HeaderCell.SortGlyphDirection = FMsDGV.CurrentSortDirection;
                }
                else if (i != intCol && c.HeaderCell.SortGlyphDirection != SortOrder.None)
                {
                    c.HeaderCell.SortGlyphDirection = SortOrder.None;
                }
            }
        }

        /// <summary>
        /// Pass selectedFM only if you need to store it BEFORE this method runs, like for RefreshFromDisk()
        /// </summary>
        /// <param name="selectedFM"></param>
        /// <param name="forceDisplayFM"></param>
        /// <param name="keepSelection"></param>
        /// <param name="gameTabSwitch"></param>
        /// <returns></returns>
        public async Task SortAndSetFilter(SelectedFM? selectedFM = null, bool forceDisplayFM = false,
            bool keepSelection = true, bool gameTabSwitch = false)
        {
            bool selFMWasPassedIn = selectedFM != null;

            FanMission? oldSelectedFM = FMsDGV.RowSelected() ? FMsDGV.GetSelectedFM() : null;

            selectedFM ??= keepSelection && !gameTabSwitch && FMsDGV.RowSelected()
                ? FMsDGV.GetSelectedFMPosInfo()
                : null;

            KeepSel keepSel =
                selectedFM != null ? KeepSel.TrueNearest :
                keepSelection || gameTabSwitch ? KeepSel.True : KeepSel.False;

            // Fix: in RefreshFMsList, CurrentSelFM was being used when coming from no FMs listed to some FMs listed
            if (!gameTabSwitch && !selFMWasPassedIn && oldSelectedFM == null) keepSel = KeepSel.False;

            if (gameTabSwitch) forceDisplayFM = true;

            SortFMsDGV(FMsDGV.CurrentSortedColumn, FMsDGV.CurrentSortDirection);

            Core.SetFilter();
            if (RefreshFMsList(selectedFM, keepSelection: keepSel))
            {
                // DEBUG: Keep this in for testing this because the whole thing is irrepressibly finicky
                //Trace.WriteLine(nameof(keepSelection) + ": " + keepSelection);
                //Trace.WriteLine("selectedFM != null: " + (selectedFM != null));
                //Trace.WriteLine("!selectedFM.InstalledName.IsEmpty(): " + (selectedFM != null && !selectedFM.InstalledName.IsEmpty()));
                //Trace.WriteLine("selectedFM.InstalledName != FMsDGV.GetSelectedFM().InstalledDir: " + (selectedFM != null && selectedFM.InstalledName != FMsDGV.GetSelectedFM().InstalledDir));

                // Optimization in case we land on the same as FM as before, don't reload it
                // And whaddaya know, I still ended up having to have this eyes-glazing-over stuff here.
                if (forceDisplayFM ||
                    (keepSelection &&
                     selectedFM != null && !selectedFM.InstalledName.IsEmpty() &&
                     selectedFM.InstalledName != FMsDGV.GetSelectedFM().InstalledDir) ||
                    (!keepSelection &&
                     (oldSelectedFM == null ||
                      (FMsDGV.RowSelected() && !oldSelectedFM.Equals(FMsDGV.GetSelectedFM())))) ||
                    // Fix: when resetting release date filter the readme wouldn't load for the selected FM
                    oldSelectedFM == null)
                {
                    await DisplaySelectedFM(refreshReadme: true);
                }
            }
        }

        #region FMsDGV event handlers

        // Coloring the recent rows here because if we do it in _CellValueNeeded, we get a brief flash of the
        // default while-background cell color before it changes.
        private void FMsDGV_RowPrePaint(object sender, DataGridViewRowPrePaintEventArgs e)
        {
            if (_cellValueNeededDisabled) return;

            if (FMsDGV.FilterShownIndexList.Count == 0) return;

            var fm = FMsDGV.GetFMFromIndex(e.RowIndex);

            FMsDGV.Rows[e.RowIndex].DefaultCellStyle.BackColor = fm.MarkedRecent ? Color.LightGoldenrodYellow : SystemColors.Window;
        }

        private void FMsDGV_CellValueNeeded_Initial(object sender, DataGridViewCellValueEventArgs e)
        {
            if (_cellValueNeededDisabled) return;

            // @LAZYLOAD notes for DGV images:
            // -If we're going to lazy-load, we should go full-bore and draw the icons onto bitmaps as needed,
            //  so we can get rid of all the duplicate pre-baked images we have now.
            // -Until we do that, we should leave this alone. It also might be okay to just keep these like they
            //  are, being loaded in advance, because then the scrolling is guaranteed not to have any hiccups.

            // Lazy-load these in an attempt to save some kind of startup time
            // @LAZYLOAD: Try lazy-loading these at a more granular level
            // The arrays are obstacles to lazy-loading, but see if we still get good scrolling perf when we look
            // them up and load the individual images as needed, rather than all at once here

            // @GENGAMES (Game icons for FMs list): Begin
            // We would prefer to put these in an array, but see Images class for why we can't really do that
            GameIcons[(int)Thief1] = Images.Thief1_21;
            GameIcons[(int)Thief2] = Images.Thief2_21;
            GameIcons[(int)Thief3] = Images.Thief3_21;
            GameIcons[(int)SS2] = Images.Shock2_21;
            // @GENGAMES (Game icons for FMs list): End

            BlankIcon = new Bitmap(1, 1, PixelFormat.Format32bppPArgb);
            CheckIcon = Resources.CheckCircle;
            RedQuestionMarkIcon = Resources.QuestionMarkCircleRed;
            // @LAZYLOAD: Have these be wrapper objects so we can put them in the list without them loading
            // Then grab the internal object down below when we go to display them
            StarIcons = Images.GetRatingImages();

            FinishedOnIcons = Images.GetFinishedOnImages(BlankIcon);
            FinishedOnUnknownIcon = Images.FinishedOnUnknown;

            // Prevents having to check the bool again forevermore even after we've already set the images.
            // Taking an extremely minor technique from a data-oriented design talk, heck yeah!
            FMsDGV.CellValueNeeded -= FMsDGV_CellValueNeeded_Initial;
            FMsDGV.CellValueNeeded += FMsDGV_CellValueNeeded;
            FMsDGV_CellValueNeeded(sender, e);
        }

        private void FMsDGV_CellValueNeeded(object sender, DataGridViewCellValueEventArgs e)
        {
            if (_cellValueNeededDisabled) return;

            if (FMsDGV.FilterShownIndexList.Count == 0) return;

            var fm = FMsDGV.GetFMFromIndex(e.RowIndex);

            // PERF: ~0.14ms per FM for en-US Long Date format
            // PERF_TODO: Test with custom - dt.ToString() might be slow?
            static string FormatDate(DateTime dt) => Config.DateFormat switch
            {
                DateFormat.CurrentCultureShort => dt.ToShortDateString(),
                DateFormat.CurrentCultureLong => dt.ToLongDateString(),
                _ => dt.ToString(Config.DateCustomFormatString),
            };

            static string FormatSize(ulong size) =>
                size == 0
                ? ""
                : size < ByteSize.MB
                ? Math.Round(size / 1024f).ToString(CultureInfo.CurrentCulture) + " " + LText.Global.KilobyteShort
                : size >= ByteSize.MB && size < ByteSize.GB
                ? Math.Round(size / 1024f / 1024f).ToString(CultureInfo.CurrentCulture) + " " + LText.Global.MegabyteShort
                : Math.Round(size / 1024f / 1024f / 1024f, 2).ToString(CultureInfo.CurrentCulture) + " " + LText.Global.GigabyteShort;

            switch ((Column)e.ColumnIndex)
            {
                case Column.Game:
                    e.Value =
                        GameIsKnownAndSupported(fm.Game) ? GameIcons[(int)GameToGameIndex(fm.Game)] :
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
                            string a = Config.Articles[i];
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
                    // This conversion takes like 1ms over the entire 1545 set, so no problem
                    e.Value = FormatSize(fm.SizeBytes);
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
                            e.Value = fm.Rating == -1 ? BlankIcon : StarIcons![fm.Rating];
                        }
                        else
                        {
                            e.Value = fm.Rating == -1 ? "" : (fm.Rating / 2.0).ToString(CultureInfo.CurrentCulture);
                        }
                    }
                    break;

                case Column.Finished:
                    e.Value = fm.FinishedOnUnknown ? FinishedOnUnknownIcon : FinishedOnIcons![fm.FinishedOn];
                    break;

                case Column.ReleaseDate:
                    e.Value = fm.ReleaseDate.DateTime != null ? FormatDate((DateTime)fm.ReleaseDate.DateTime) : "";
                    break;

                case Column.LastPlayed:
                    e.Value = fm.LastPlayed.DateTime != null ? FormatDate((DateTime)fm.LastPlayed.DateTime) : "";
                    break;

                case Column.DateAdded:
                    // Convert to local time: very important. We don't do it earlier for startup perf reasons.
                    e.Value = fm.DateAdded != null ? FormatDate(((DateTime)fm.DateAdded).ToLocalTime()) : "";
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

            SelectedFM? selFM = FMsDGV.RowSelected() ? FMsDGV.GetSelectedFMPosInfo() : null;

            var newSortDirection =
                e.ColumnIndex == (int)FMsDGV.CurrentSortedColumn && FMsDGV.CurrentSortDirection == SortOrder.Ascending
                    ? SortOrder.Descending
                    : SortOrder.Ascending;

            SortFMsDGV((Column)e.ColumnIndex, newSortDirection);

            Core.SetFilter();
            if (RefreshFMsList(selFM, keepSelection: KeepSel.TrueNearest, fromColumnClick: true))
            {
                if (selFM != null && FMsDGV.RowSelected() &&
                    selFM.InstalledName != FMsDGV.GetSelectedFM().InstalledDir)
                {
                    await DisplaySelectedFM(refreshReadme: true);
                }
            }
        }

        private void FMsDGV_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right) return;

            var ht = FMsDGV.HitTest(e.X, e.Y);

            #region Right-click menu

            if (ht.Type == DataGridViewHitTestType.ColumnHeader || ht.Type == DataGridViewHitTestType.None)
            {
                FMsDGV.SetContextMenuToColumnHeader();
            }
            else if (ht.Type == DataGridViewHitTestType.Cell && ht.ColumnIndex > -1 && ht.RowIndex > -1)
            {
                FMsDGV.SetContextMenuToFM();
                FMsDGV.Rows[ht.RowIndex].Selected = true;
                // We don't need to call SelectProperly() here because the mousedown will select it properly
            }
            else
            {
                FMsDGV.SetContextMenuToNone();
            }

            #endregion
        }

        // Okay, boys and girls. We get the glitched last row on keyboard-scroll if we don't do this idiot thing.
        // No, we can't do any of the normal things you'd think would work in RefreshFMsList() itself. I tried.
        // Everything is stupid. Whatever.
        private bool _fmsListOneTimeHackRefreshDone;
        private async void FMsDGV_SelectionChanged(object sender, EventArgs e)
        {
            if (EventsDisabled) return;

            if (!FMsDGV.RowSelected())
            {
                ClearShownData();
            }
            else
            {
                FMsDGV.SelectProperly();

                if (!_fmsListOneTimeHackRefreshDone)
                {
                    RefreshFMsList(FMsDGV.GetSelectedFMPosInfo(), startup: false, KeepSel.TrueNearest);
                    _fmsListOneTimeHackRefreshDone = true;
                }

                await DisplaySelectedFM(refreshReadme: true);
            }
        }

        #region Crappy hack for basic go-to-first-typed-letter

        // TODO: Make this into a working, polished, documented feature

        private void FMsDGV_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar.IsAsciiAlpha())
            {
                int rowIndex = -1;

                for (int i = 0; i < FMsDGV.RowCount; i++)
                {
                    if (FMsDGV.Rows[i].Cells[(int)Column.Title].Value.ToString().StartsWithI(e.KeyChar.ToString()))
                    {
                        rowIndex = i;
                        break;
                    }
                }

                if (rowIndex > -1)
                {
                    FMsDGV.Rows[rowIndex].Selected = true;
                    FMsDGV.SelectProperly();
                    FMsDGV.FirstDisplayedScrollingRowIndex = FMsDGV.SelectedRows[0].Index;
                }
            }
        }

        #endregion

        private void FMsDGV_KeyDown(object sender, KeyEventArgs e)
        {
            // This is in here because it doesn't really work right if we put it in MainForm_KeyDown anyway
            if (e.KeyCode == Keys.Apps)
            {
                FMsDGV.SetContextMenuToFM();
            }
        }

        private async void FMsDGV_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            FanMission fm;
            if (e.RowIndex < 0 || !FMsDGV.RowSelected() || !GameIsKnownAndSupported((fm = FMsDGV.GetSelectedFM()).Game))
            {
                return;
            }

            await FMInstallAndPlay.InstallIfNeededAndPlay(fm, askConfIfRequired: true);
        }

        #endregion

        #endregion

        #region Bottom bar

        #region Left side

        #region Install/Play buttons

        internal async void InstallUninstallFMButton_Click(object sender, EventArgs e) => await FMInstallAndPlay.InstallOrUninstall(FMsDGV.GetSelectedFM());

        private async void PlayFMButton_Click(object sender, EventArgs e) => await FMInstallAndPlay.InstallIfNeededAndPlay(FMsDGV.GetSelectedFM());

        #region Play original game

        // @GENGAMES (Play original game menu event handlers): Begin
        // Because of the T2MP menu item breaking up the middle there, we can't array/index these menu items.
        // Just gonna have to leave this part as-is.
        private void PlayOriginalGameButton_Click(object sender, EventArgs e)
        {
            PlayOriginalGameLLMenu.Construct(this, components);

            PlayOriginalGameLLMenu.Thief1MenuItem.Enabled = !Config.GetGameExe(Thief1).IsEmpty();
            PlayOriginalGameLLMenu.Thief2MenuItem.Enabled = !Config.GetGameExe(Thief2).IsEmpty();
            PlayOriginalGameLLMenu.Thief2MPMenuItem.Visible = Config.T2MPDetected;
            PlayOriginalGameLLMenu.Thief3MenuItem.Enabled = !Config.GetGameExe(Thief3).IsEmpty();
            PlayOriginalGameLLMenu.SS2MenuItem.Enabled = !Config.GetGameExe(SS2).IsEmpty();

            ShowMenu(PlayOriginalGameLLMenu.Menu, PlayOriginalGameButton, MenuPos.TopRight);
        }

        [SuppressMessage("ReSharper", "MemberCanBeMadeStatic.Global")]
        internal void PlayOriginalGameMenuItem_Click(object sender, EventArgs e)
        {
            var item = (ToolStripMenuItem)sender;

            GameIndex game =
                item == PlayOriginalGameLLMenu.Thief1MenuItem ? Thief1 :
                item == PlayOriginalGameLLMenu.Thief2MenuItem || item == PlayOriginalGameLLMenu.Thief2MPMenuItem ? Thief2 :
                item == PlayOriginalGameLLMenu.Thief3MenuItem ? Thief3 :
                SS2;

            bool playMP = item == PlayOriginalGameLLMenu.Thief2MPMenuItem;

            FMInstallAndPlay.PlayOriginalGame(game, playMP);
        }
        // @GENGAMES (Play original game menu event handlers): End

        #endregion

        #endregion

        private async void ScanAllFMsButton_Click(object sender, EventArgs e)
        {
            if (FMsViewList.Count == 0) return;

            FMScanner.ScanOptions? scanOptions = null;
            bool noneSelected;
            using (var f = new ScanAllFMsForm())
            {
                if (f.ShowDialog() != DialogResult.OK) return;
                noneSelected = f.NoneSelected;
                if (!noneSelected)
                {
                    scanOptions = FMScanner.ScanOptions.FalseDefault(
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

            bool success = await FMScan.ScanFMs(FMsViewList, scanOptions!);
            if (success) await SortAndSetFilter(forceDisplayFM: true);
        }

        private void WebSearchButton_Click(object sender, EventArgs e) => Core.OpenWebSearchUrl(FMsDGV.GetSelectedFM().Title);

        #endregion

        #region Right side

        private void ImportButton_Click(object sender, EventArgs e)
        {
            ImportFromLLMenu.Construct(this, components);
            ShowMenu(ImportFromLLMenu.ImportFromMenu, ImportButton, MenuPos.TopLeft);
        }

        [SuppressMessage("ReSharper", "MemberCanBeMadeStatic.Global")]
        internal async void ImportMenuItems_Click(object sender, EventArgs e)
        {
            // This is only hooked up after construct, so we know these menu items are initialized
            if (sender == ImportFromLLMenu.ImportFromDarkLoaderMenuItem)
            {
                await ImportCommon.ImportFromDarkLoader();
            }
            else
            {
                await ImportCommon.ImportFromNDLOrFMSel(sender == ImportFromLLMenu.ImportFromFMSelMenuItem
                    ? ImportType.FMSel
                    : ImportType.NewDarkLoader);
            }
        }

        private async void SettingsButton_Click(object sender, EventArgs e)
        {
            var ret = Core.OpenSettings();
            if (ret.Canceled) return;

            if (ret.FMsViewListUnscanned?.Count > 0) await FMScan.ScanNewFMs(ret.FMsViewListUnscanned);
            // TODO: forceDisplayFM is always true so that this always works, but it could be smarter
            // If I store the selected FM up above the Find(), I can make the FM not have to reload if
            // it's still selected
            if (ret.SortAndSetFilter) await SortAndSetFilter(keepSelection: ret.KeepSel, forceDisplayFM: true);
        }

        #endregion

        #endregion

        #region Update displayed rating

        public void UpdateRatingDisplayStyle(RatingDisplayStyle style, bool startup)
        {
            UpdateRatingListsAndColumn(style == RatingDisplayStyle.FMSel, startup);
            UpdateRatingLabel();
        }

        private void UpdateRatingListsAndColumn(bool fmSelStyle, bool startup)
        {
            #region Update rating lists

            // Just in case, since changing a ComboBox item's text counts as a selected index change maybe? Argh!
            using (new DisableEvents(this))
            {
                for (int i = 0; i <= 10; i++)
                {
                    string num = (fmSelStyle ? i / 2.0 : i).ToString(CultureInfo.CurrentCulture);
                    EditFMRatingComboBox.Items[i + 1] = num;
                }
            }

            FMsDGV.UpdateRatingList(fmSelStyle);

            #endregion

            #region Update rating column

            var newRatingColumn =
                Config.RatingDisplayStyle == RatingDisplayStyle.FMSel && Config.RatingUseStars
                    ? (DataGridViewColumn)RatingImageColumn!
                    : RatingTextColumn;

            if (!startup)
            {
                var oldRatingColumn = FMsDGV.Columns[(int)Column.Rating];
                newRatingColumn!.Width = newRatingColumn == RatingTextColumn
                    ? oldRatingColumn.Width
                    // To set the ratio back to exact on zoom reset
                    : FMsDGV.RowTemplate.Height == 22
                        ? _ratingImageColumnWidth
                        : (FMsDGV.DefaultCellStyle.Font.Height + 9) * (_ratingImageColumnWidth / 22);
                newRatingColumn.Visible = oldRatingColumn.Visible;
                newRatingColumn.DisplayIndex = oldRatingColumn.DisplayIndex;
            }

            if (!startup || newRatingColumn != RatingTextColumn)
            {
                using (new DisableEvents(this))
                {
                    _cellValueNeededDisabled = true;
                    try
                    {
                        FMsDGV.Columns.RemoveAt((int)Column.Rating);
                        FMsDGV.Columns.Insert((int)Column.Rating, newRatingColumn!);
                    }
                    finally
                    {
                        _cellValueNeededDisabled = false;
                    }
                }
                if (FMsDGV.CurrentSortedColumn == Column.Rating)
                {
                    FMsDGV.Columns[(int)Column.Rating].HeaderCell.SortGlyphDirection = FMsDGV.CurrentSortDirection;
                }
            }

            if (!startup)
            {
                FMsDGV.SetColumnData(FMsDGV.GetColumnData());
                RefreshFMsListKeepSelection();
            }

            #endregion
        }

        private void UpdateRatingLabel(bool suspendResume = true)
        {
            // For snappy visual layout performance
            if (suspendResume) FilterBarFLP.SuspendDrawing();
            try
            {
                if (FilterByRatingButton.Checked)
                {
                    bool ndl = Config.RatingDisplayStyle == RatingDisplayStyle.NewDarkLoader;
                    int rFrom = FMsDGV.Filter.RatingFrom;
                    int rTo = FMsDGV.Filter.RatingTo;
                    var curCulture = CultureInfo.CurrentCulture;

                    string from = rFrom == -1 ? LText.Global.None : (ndl ? rFrom : rFrom / 2.0).ToString(curCulture);
                    string to = rTo == -1 ? LText.Global.None : (ndl ? rTo : rTo / 2.0).ToString(curCulture);

                    Lazy_ToolStripLabels.Show(this, Lazy_ToolStripLabel.FilterByRating, from + @" - " + to);
                }
                else
                {
                    Lazy_ToolStripLabels.Hide(Lazy_ToolStripLabel.FilterByRating);
                }
            }
            finally
            {
                if (suspendResume) FilterBarFLP.ResumeDrawing();
            }
        }

        #endregion

        #region Refresh FMs list

        public void RefreshSelectedFMRowOnly() => FMsDGV.InvalidateRow(FMsDGV.SelectedRows[0].Index);

        public Task RefreshSelectedFM(bool refreshReadme)
        {
            FMsDGV.InvalidateRow(FMsDGV.SelectedRows[0].Index);

            return DisplaySelectedFM(refreshReadme);
        }

        /// <summary>
        /// Returns false if the list is empty and ClearShownData() has been called, otherwise true
        /// </summary>
        /// <param name="selectedFM"></param>
        /// <param name="startup"></param>
        /// <param name="keepSelection"></param>
        /// <param name="fromColumnClick"></param>
        /// <returns></returns>
        private bool RefreshFMsList(SelectedFM? selectedFM, bool startup = false, KeepSel keepSelection = KeepSel.False,
            bool fromColumnClick = false)
        {
            using (new DisableEvents(this))
            {
                // A small but measurable perf increase from this. Also prevents flickering when switching game
                // tabs.
                if (!startup)
                {
                    FMsDGV.SuspendDrawing();
                    // So, I'm sorry, I thought the line directly above this one said to suspend drawing. I just
                    // thought I saw a suspend drawing command, and since drawing cells constitutes drawing, I
                    // just assumed you would understand that to suspend drawing means not to draw cells. I must
                    // be mistaken. No no, please.
                    _cellValueNeededDisabled = true;
                }

                // Prevents:
                // -a glitched row from being drawn at the end in certain situations
                // -the subsequent row count set from being really slow
                FMsDGV.Rows.Clear();

                FMsDGV.RowCount = FMsDGV.FilterShownIndexList.Count;

                if (FMsDGV.RowCount == 0)
                {
                    if (!startup) FMsDGV.ResumeDrawing();
                    ClearShownData();
                    return false;
                }
                else
                {
                    int row;
                    if (keepSelection == KeepSel.False)
                    {
                        row = 0;
                        FMsDGV.FirstDisplayedScrollingRowIndex = 0;
                    }
                    else
                    {
                        SelectedFM selFM = selectedFM ?? FMsDGV.CurrentSelFM;
                        bool findNearest = keepSelection == KeepSel.TrueNearest && selectedFM != null;
                        row = FMsDGV.GetIndexFromInstalledName(selFM.InstalledName, findNearest).ClampToZero();
                        try
                        {
                            if (fromColumnClick)
                            {
                                if (FMsDGV.CurrentSortDirection == SortOrder.Ascending)
                                {
                                    FMsDGV.FirstDisplayedScrollingRowIndex = row.ClampToZero();
                                }
                                else if (FMsDGV.CurrentSortDirection == SortOrder.Descending)
                                {
                                    FMsDGV.FirstDisplayedScrollingRowIndex = (row - FMsDGV.DisplayedRowCount(true)).ClampToZero();
                                }
                            }
                            else
                            {
                                FMsDGV.FirstDisplayedScrollingRowIndex = (row - selFM.IndexFromTop).ClampToZero();
                            }
                        }
                        catch
                        {
                            // no room is available to display rows
                        }
                    }

                    // Events will be re-enabled at the end of the enclosing using block
                    if (keepSelection != KeepSel.False) EventsDisabled = true;
                    FMsDGV.Rows[row].Selected = true;
                    FMsDGV.SelectProperly(suspendResume: startup);

                    // Resume drawing before loading the readme; that way the list will update instantly even
                    // if the readme doesn't. The user will see delays in the "right place" (the readme box)
                    // and understand why it takes a sec. Otherwise, it looks like merely changing tabs brings
                    // a significant delay, and that's annoying because it doesn't seem like it should happen.
                    if (!startup)
                    {
                        _cellValueNeededDisabled = false;
                        FMsDGV.ResumeDrawing();
                    }
                }
            }

            return true;
        }

        public void RefreshFMsListKeepSelection()
        {
            if (FMsDGV.RowCount == 0) return;

            int selectedRow = FMsDGV.SelectedRows[0].Index;

            using (new DisableEvents(this))
            {
                FMsDGV.Refresh();
                FMsDGV.Rows[selectedRow].Selected = true;
                FMsDGV.SelectProperly();
            }
        }

        #endregion

        #region FM display

        // Perpetual TODO: Make sure this clears everything including the top right tab stuff
        private void ClearShownData()
        {
            if (FMsViewList.Count == 0) ScanAllFMsButton.Enabled = false;

            FMsDGV.SetInstallUninstallMenuItemText(true);
            FMsDGV.SetInstallUninstallMenuItemEnabled(false);
            FMsDGV.SetDeleteFMMenuItemEnabled(false);
            FMsDGV.SetOpenInDromEdMenuItemText(false);

            // Special-cased; don't autosize this one
            InstallUninstallFMLLButton.SetSayInstall(true);
            InstallUninstallFMLLButton.SetEnabled(false);

            FMsDGV.SetPlayFMMenuItemEnabled(false);
            PlayFMButton.Enabled = false;

            FMsDGV.SetPlayFMInMPMenuItemVisible(false);

            FMsDGV.SetOpenInDromEdVisible(false);

            FMsDGV.SetScanFMMenuItemEnabled(false);

            FMsDGV.SetConvertAudioRCSubMenuEnabled(false);

            // Hide instead of clear to avoid zoom factor pain
            SetReadmeVisible(false);

            ChooseReadmeLLPanel.ShowPanel(false);
            ViewHTMLReadmeLLButton.Hide();
            WebSearchButton.Enabled = false;

            BlankStatsPanelWithMessage(LText.StatisticsTab.NoFMSelected);
            StatsScanCustomResourcesButton.Hide();

            AltTitlesLLMenu.ClearItems();

            using (new DisableEvents(this))
            {
                EditFMRatingComboBox.SelectedIndex = 0;

                EditFMLanguageComboBox.ClearFullItems();
                EditFMLanguageComboBox.AddFullItem(FMLanguages.DefaultLangKey, LText.EditFMTab.DefaultLanguage);
                EditFMLanguageComboBox.SelectedIndex = 0;

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

                FMsDGV.ClearFinishedOnMenuItemChecks();

                CommentTextBox.Text = "";
                CommentTextBox.Enabled = false;
                AddTagTextBox.Text = "";

                TagsTreeView.Nodes.Clear();

                foreach (Control c in TagsTabPage.Controls) c.Enabled = false;

                ShowPatchSection(enable: false);
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

        private void ShowPatchSection(bool enable)
        {
            PatchDMLsListBox.Items.Clear();
            PatchMainPanel.Show();
            PatchFMNotInstalledLabel.CenterHV(PatchTabPage);
            PatchFMNotInstalledLabel.Hide();
            PatchMainPanel.Enabled = enable;
        }

        private void BlankStatsPanelWithMessage(string message)
        {
            CustomResourcesLabel.Text = message;
            foreach (CheckBox cb in StatsCheckBoxesPanel.Controls) cb.Checked = false;
            StatsCheckBoxesPanel.Hide();
        }

        // @GENGAMES: Lots of game-specific code in here, but I don't see much to be done about it.
        private async Task DisplaySelectedFM(bool refreshReadme, bool refreshCache = false)
        {
            FanMission fm = FMsDGV.GetSelectedFM();

            if (fm.Game == Game.Null || (GameIsKnownAndSupported(fm.Game) && !fm.MarkedScanned))
            {
                using (new DisableKeyPresses(this))
                {
                    if (await FMScan.ScanFMs(new List<FanMission> { fm }, hideBoxIfZip: true))
                    {
                        RefreshSelectedFMRowOnly();
                    }
                }
            }

            bool fmIsT3 = fm.Game == Game.Thief3;
            bool fmIsSS2 = fm.Game == Game.SS2;

            #region Toggles

            // We should never get here when FMsList.Count == 0, but hey
            if (FMsViewList.Count > 0) ScanAllFMsButton.Enabled = true;

            FMsDGV.SetGameSpecificFinishedOnMenuItemsText(fm.Game);
            // FinishedOnUnknownMenuItem text stays the same

            bool gameIsSupported = GameIsKnownAndSupported(fm.Game);

            FMsDGV.SetInstallUninstallMenuItemEnabled(gameIsSupported);
            FMsDGV.SetInstallUninstallMenuItemText(!fm.Installed);
            FMsDGV.SetDeleteFMMenuItemEnabled(true);
            FMsDGV.SetOpenInDromEdMenuItemText(fmIsSS2);

            FMsDGV.SetOpenInDromEdVisible(GameIsDark(fm.Game) && Config.GetGameEditorDetectedUnsafe(fm.Game));

            FMsDGV.SetPlayFMInMPMenuItemVisible(fm.Game == Game.Thief2 && Config.T2MPDetected);

            InstallUninstallFMLLButton.SetEnabled(gameIsSupported);
            // Special-cased; don't autosize this one
            InstallUninstallFMLLButton.SetSayInstall(!fm.Installed);

            FMsDGV.SetPlayFMMenuItemEnabled(gameIsSupported);
            PlayFMButton.Enabled = gameIsSupported;

            FMsDGV.SetScanFMMenuItemEnabled(true);

            FMsDGV.SetConvertAudioRCSubMenuEnabled(GameIsDark(fm.Game) && fm.Installed);

            WebSearchButton.Enabled = true;

            foreach (Control c in EditFMTabPage.Controls)
            {
                if (c == EditFMLanguageLabel ||
                    c == EditFMLanguageComboBox ||
                    c == EditFMScanLanguagesButton)
                {
                    c.Enabled = !fmIsT3;
                }
                else
                {
                    c.Enabled = true;
                }
            }

            CommentTextBox.Enabled = true;
            foreach (Control c in TagsTabPage.Controls) c.Enabled = true;

            PatchMainPanel.Enabled = true;

            if (fm.Installed)
            {
                ShowPatchSection(enable: true);
            }
            else
            {
                HidePatchSectionWithMessage(LText.PatchTab.FMNotInstalled);
            }

            PatchDMLsPanel.Enabled = fm.Game != Game.Thief3;

            #endregion

            #region FinishedOn

            FMsDGV.SetFinishedOnMenuItemsChecked((Difficulty)fm.FinishedOn, fm.FinishedOnUnknown);

            #endregion

            #region Custom resources

            if (fmIsT3)
            {
                BlankStatsPanelWithMessage(LText.StatisticsTab.CustomResourcesNotSupportedForThief3);
                StatsScanCustomResourcesButton.Hide();
            }
            else if (!fm.ResourcesScanned)
            {
                BlankStatsPanelWithMessage(LText.StatisticsTab.CustomResourcesNotScanned);
                StatsScanCustomResourcesButton.Show();
            }
            else
            {
                CustomResourcesLabel.Text = LText.StatisticsTab.CustomResources;

                CR_MapCheckBox.Checked = FMHasResource(fm, CustomResources.Map);
                CR_AutomapCheckBox.Checked = FMHasResource(fm, CustomResources.Automap);
                CR_ScriptsCheckBox.Checked = FMHasResource(fm, CustomResources.Scripts);
                CR_TexturesCheckBox.Checked = FMHasResource(fm, CustomResources.Textures);
                CR_SoundsCheckBox.Checked = FMHasResource(fm, CustomResources.Sounds);
                CR_ObjectsCheckBox.Checked = FMHasResource(fm, CustomResources.Objects);
                CR_CreaturesCheckBox.Checked = FMHasResource(fm, CustomResources.Creatures);
                CR_MotionsCheckBox.Checked = FMHasResource(fm, CustomResources.Motions);
                CR_MoviesCheckBox.Checked = FMHasResource(fm, CustomResources.Movies);
                CR_SubtitlesCheckBox.Checked = FMHasResource(fm, CustomResources.Subtitles);

                StatsCheckBoxesPanel.Show();
                StatsScanCustomResourcesButton.Show();
            }

            #endregion

            #region Other tabs

            using (new DisableEvents(this))
            {
                EditFMTitleTextBox.Text = fm.Title;

                FillAltTitlesMenu(fm.AltTitles);

                EditFMAuthorTextBox.Text = fm.Author;

                EditFMReleaseDateCheckBox.Checked = fm.ReleaseDate.DateTime != null;
                EditFMReleaseDateDateTimePicker.Value = fm.ReleaseDate.DateTime ?? DateTime.Now;
                EditFMReleaseDateDateTimePicker.Visible = fm.ReleaseDate.DateTime != null;

                EditFMLastPlayedCheckBox.Checked = fm.LastPlayed.DateTime != null;
                EditFMLastPlayedDateTimePicker.Value = fm.LastPlayed.DateTime ?? DateTime.Now;
                EditFMLastPlayedDateTimePicker.Visible = fm.LastPlayed.DateTime != null;

                EditFMDisableAllModsCheckBox.Checked = fm.DisableAllMods;
                EditFMDisabledModsTextBox.Text = fm.DisabledMods;
                EditFMDisabledModsTextBox.Enabled = !fm.DisableAllMods;

                FMsDGV.SetRatingMenuItemChecked(fm.Rating);
                EditFMRatingComboBox.SelectedIndex = fm.Rating + 1;

                ScanAndFillLanguagesBox(fm, disableEvents: false);

                CommentTextBox.Text = fm.Comment.FromRNEscapes();

                AddTagTextBox.Text = "";

                if (GameIsDark(fm.Game) && fm.Installed)
                {
                    PatchMainPanel.Show();
                    PatchFMNotInstalledLabel.Hide();
                    PatchDMLsListBox.Items.Clear();
                    var (success, dmlFiles) = Core.GetDMLFiles(fm);
                    if (success)
                    {
                        foreach (string f in dmlFiles)
                        {
                            if (!f.IsEmpty()) PatchDMLsListBox.Items.Add(f);
                        }
                    }
                }
            }

            DisplayFMTags(fm.Tags);

            #endregion

            if (!refreshReadme) return;

            var cacheData = await FMCache.GetCacheableData(fm, refreshCache);

            #region Readme

            var readmeFiles = cacheData.Readmes;
            readmeFiles.Sort();

            if (!readmeFiles.PathContainsI(fm.SelectedReadme)) fm.SelectedReadme = "";

            using (new DisableEvents(this)) ChooseReadmeComboBox.ClearFullItems();

            if (!fm.SelectedReadme.IsEmpty())
            {
                if (readmeFiles.Count > 1)
                {
                    ReadmeComboBoxFillAndSelect(readmeFiles, fm.SelectedReadme);
                }
                else
                {
                    ChooseReadmeComboBox.Hide();
                }
            }
            else // if fm.SelectedReadme is empty
            {
                if (readmeFiles.Count == 0)
                {
                    ReadmeRichTextBox.SetText(LText.ReadmeArea.NoReadmeFound);

                    ChooseReadmeLLPanel.ShowPanel(false);
                    ChooseReadmeComboBox.Hide();
                    ViewHTMLReadmeLLButton.Hide();
                    SetReadmeVisible(true);

                    return;
                }
                else if (readmeFiles.Count > 1)
                {
                    string safeReadme = Core.DetectSafeReadme(readmeFiles, fm.Title);

                    if (!safeReadme.IsEmpty())
                    {
                        fm.SelectedReadme = safeReadme;
                        // @DIRSEP: Pass only fm.SelectedReadme, otherwise we might end up with un-normalized dirseps
                        ReadmeComboBoxFillAndSelect(readmeFiles, fm.SelectedReadme);
                    }
                    else
                    {
                        SetReadmeVisible(false);
                        ViewHTMLReadmeLLButton.Hide();

                        ChooseReadmeLLPanel.Construct(this, MainSplitContainer.Panel2);

                        ChooseReadmeLLPanel.ListBox.ClearFullItems();
                        foreach (string f in readmeFiles) ChooseReadmeLLPanel.ListBox.AddFullItem(f, f.GetFileNameFast());

                        ShowReadmeControls(false);

                        ChooseReadmeLLPanel.ShowPanel(true);

                        return;
                    }
                }
                else if (readmeFiles.Count == 1)
                {
                    fm.SelectedReadme = readmeFiles[0];

                    ChooseReadmeComboBox.Hide();
                }
            }

            ChooseReadmeLLPanel.ShowPanel(false);

            LoadReadme(fm);

            #endregion
        }

        private void ScanAndFillLanguagesBox(FanMission fm, bool forceScan = false, bool disableEvents = true)
        {
            using (disableEvents ? new DisableEvents(this) : null)
            {
                EditFMLanguageComboBox.ClearFullItems();
                EditFMLanguageComboBox.AddFullItem(FMLanguages.DefaultLangKey, LText.EditFMTab.DefaultLanguage);

                if (!GameIsDark(fm.Game))
                {
                    EditFMLanguageComboBox.SelectedIndex = 0;
                    fm.SelectedLang = FMLanguages.DefaultLangKey;
                    return;
                }

                bool doScan = forceScan || !fm.LangsScanned;

                if (doScan) FMLanguages.FillFMSupportedLangs(fm);

                var langs = fm.Langs.Split(CA_Comma, StringSplitOptions.RemoveEmptyEntries).ToList();
                var sortedLangs = doScan ? langs : FMLanguages.SortLangsToSpec(langs);
                fm.Langs = "";
                for (int i = 0; i < sortedLangs.Count; i++)
                {
                    string langLower = sortedLangs[i].ToLowerInvariant();
                    EditFMLanguageComboBox.AddFullItem(langLower, FMLanguages.Translated[langLower]);

                    // Rewrite the FM's lang string for cleanliness, in case it contains unsupported langs or
                    // other nonsense
                    if (!langLower.EqualsI(FMLanguages.DefaultLangKey))
                    {
                        if (!fm.Langs.IsEmpty()) fm.Langs += ",";
                        fm.Langs += langLower;
                    }
                }

                if (fm.SelectedLang.EqualsI(FMLanguages.DefaultLangKey))
                {
                    EditFMLanguageComboBox.SelectedIndex = 0;
                    fm.SelectedLang = FMLanguages.DefaultLangKey;
                }
                else
                {
                    int index = EditFMLanguageComboBox.BackingItems.FindIndex(x => x.EqualsI(fm.SelectedLang));
                    EditFMLanguageComboBox.SelectedIndex = index == -1 ? 0 : index;

                    fm.SelectedLang = EditFMLanguageComboBox.SelectedIndex > -1
                        ? EditFMLanguageComboBox.SelectedBackingItem()
                        : FMLanguages.DefaultLangKey;
                }
            }
        }

        private void ReadmeComboBoxFillAndSelect(List<string> readmeFiles, string readme)
        {
            using (new DisableEvents(this))
            {
                // @DIRSEP: To backslashes for each file, to prevent selection misses.
                // I thought I accounted for this with backslashing the selected readme, but they all need to be.
                foreach (string f in readmeFiles) ChooseReadmeComboBox.AddFullItem(f.ToBackSlashes(), f.GetFileNameFast());
                ChooseReadmeComboBox.SelectBackingIndexOf(readme);
            }
        }

        private void LoadReadme(FanMission fm)
        {
            try
            {
                var (path, fileType) = Core.GetReadmeFileAndType(fm);
                #region Debug

                // Tells me whether a readme got reloaded more than once, which should never be allowed to happen
                // due to performance concerns.
#if DEBUG || (Release_Testing && !RT_StartupOnly)
                DebugLabel.Text = int.TryParse(DebugLabel.Text, out int result) ? (result + 1).ToString() : "1";
#endif

                #endregion

                if (fileType == ReadmeType.HTML)
                {
                    ViewHTMLReadmeLLButton.Show(this);
                    SetReadmeVisible(false);
                    // In case the cursor is over the scroll bar area
                    if (CursorOverReadmeArea()) ShowReadmeControls(true);
                }
                else
                {
                    SetReadmeVisible(true);
                    ViewHTMLReadmeLLButton.Hide();

                    ReadmeRichTextBox.LoadContent(path, fileType);
                }
            }
            catch (Exception ex)
            {
                Log(nameof(LoadReadme) + " failed.", ex);

                ViewHTMLReadmeLLButton.Hide();
                SetReadmeVisible(true);
                ReadmeRichTextBox.SetText(LText.ReadmeArea.UnableToLoadReadme);
            }
        }

        private void FillAltTitlesMenu(List<string> fmAltTitles)
        {
            if (!AltTitlesLLMenu.Constructed) return;

            AltTitlesLLMenu.ClearItems();

            if (fmAltTitles.Count == 0)
            {
                EditFMAltTitlesArrowButton.Enabled = false;
            }
            else
            {
                List<ToolStripItem> altTitlesMenuItems = new List<ToolStripItem>(fmAltTitles.Count);
                foreach (string altTitle in fmAltTitles)
                {
                    var item = new ToolStripMenuItem { Text = altTitle };
                    item.Click += EditFMAltTitlesMenuItems_Click;
                    altTitlesMenuItems.Add(item);
                }
                AltTitlesLLMenu.AddRange(altTitlesMenuItems);

                EditFMAltTitlesArrowButton.Enabled = true;
            }
        }

        private void DisplayFMTags(CatAndTagsList fmTags)
        {
            var tv = TagsTreeView;

            try
            {
                tv.SuspendDrawing();
                tv.Nodes.Clear();

                if (fmTags.Count == 0) return;

                fmTags.SortAndMoveMiscToEnd();

                foreach (CatAndTags item in fmTags)
                {
                    tv.Nodes.Add(item.Category);
                    var last = tv.Nodes[tv.Nodes.Count - 1];
                    foreach (string tag in item.Tags) last.Nodes.Add(tag);
                }

                tv.ExpandAll();
            }
            finally
            {
                tv.ResumeDrawing();
            }
        }

        #endregion

        #region Game tabs

        private (SelectedFM GameSelFM, Filter GameFilter)
        GetGameSelFMAndFilter(TabPage tabPage)
        {
            // NULL_TODO: Null so I can assert
            SelectedFM? gameSelFM = null;
            Filter? gameFilter = null;
            for (int i = 0; i < SupportedGameCount; i++)
            {
                if (_gameTabsInOrder[i] == tabPage)
                {
                    gameSelFM = FMsDGV.GameTabsState.GetSelectedFM((GameIndex)i);
                    gameFilter = FMsDGV.GameTabsState.GetFilter((GameIndex)i);
                    break;
                }
            }

            AssertR(gameSelFM != null, "gameSelFM is null: Selected tab is not being handled");
            AssertR(gameFilter != null, "gameFilter is null: Selected tab is not being handled");

            return (gameSelFM!, gameFilter!);
        }

        private void SaveCurrentTabSelectedFM(TabPage tabPage)
        {
            var (gameSelFM, gameFilter) = GetGameSelFMAndFilter(tabPage);
            SelectedFM selFM = FMsDGV.GetSelectedFMPosInfo();
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

            for (int i = 0; i < SupportedGameCount; i++)
            {
                _filterByGameButtonsInOrder[i].Checked = gameSelFM == FMsDGV.GameTabsState.GetSelectedFM((GameIndex)i);
            }

            gameSelFM.DeepCopyTo(FMsDGV.CurrentSelFM);
            gameFilter.DeepCopyTo(FMsDGV.Filter);

            SetUIFilterValues(gameFilter);

            await SortAndSetFilter(gameTabSwitch: true);
        }

        #endregion

        #region Top-right area

        // Hook them all up to one event handler to avoid extraneous async/awaits
        private async void FieldScanButtons_Click(object sender, EventArgs e)
        {
            if (sender == EditFMScanForReadmesButton)
            {
                Ini.WriteFullFMDataIni();
                await DisplaySelectedFM(refreshReadme: true, refreshCache: true);
            }
            else
            {
                var scanOptions =
                    sender == EditFMScanTitleButton ? FMScanner.ScanOptions.FalseDefault(scanTitle: true) :
                    sender == EditFMScanAuthorButton ? FMScanner.ScanOptions.FalseDefault(scanAuthor: true) :
                    sender == EditFMScanReleaseDateButton ? FMScanner.ScanOptions.FalseDefault(scanReleaseDate: true) :
                    //sender == StatsScanCustomResourcesButton
                    FMScanner.ScanOptions.FalseDefault(scanCustomResources: true);

                await FMScan.ScanFMAndRefresh(FMsDGV.GetSelectedFM(), scanOptions);
            }
        }

        #region Edit FM tab

        private void EditFMAltTitlesArrowButtonClick(object sender, EventArgs e)
        {
            AltTitlesLLMenu.Construct(components);
            FillAltTitlesMenu(FMsDGV.GetSelectedFM().AltTitles);
            ShowMenu(AltTitlesLLMenu.Menu, EditFMAltTitlesArrowButton, MenuPos.BottomLeft);
        }

        private void EditFMAltTitlesMenuItems_Click(object sender, EventArgs e)
        {
            EditFMTitleTextBox.Text = ((ToolStripMenuItem)sender).Text;
            Ini.WriteFullFMDataIni();
        }

        private void EditFMTitleTextBox_TextChanged(object sender, EventArgs e)
        {
            if (EventsDisabled) return;
            FMsDGV.GetSelectedFM().Title = EditFMTitleTextBox.Text;
            RefreshSelectedFMRowOnly();
        }

        private void EditFMTitleTextBox_Leave(object sender, EventArgs e)
        {
            if (EventsDisabled) return;
            Ini.WriteFullFMDataIni();
        }

        private void EditFMAuthorTextBox_TextChanged(object sender, EventArgs e)
        {
            if (EventsDisabled) return;
            FMsDGV.GetSelectedFM().Author = EditFMAuthorTextBox.Text;
            RefreshSelectedFMRowOnly();
        }

        private void EditFMAuthorTextBox_Leave(object sender, EventArgs e)
        {
            if (EventsDisabled) return;
            Ini.WriteFullFMDataIni();
        }

        private void EditFMReleaseDateCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (EventsDisabled) return;
            EditFMReleaseDateDateTimePicker.Visible = EditFMReleaseDateCheckBox.Checked;

            FMsDGV.GetSelectedFM().ReleaseDate.DateTime = EditFMReleaseDateCheckBox.Checked
                ? EditFMReleaseDateDateTimePicker.Value
                : (DateTime?)null;

            RefreshSelectedFMRowOnly();
            Ini.WriteFullFMDataIni();
        }

        private void EditFMReleaseDateDateTimePicker_ValueChanged(object sender, EventArgs e)
        {
            if (EventsDisabled) return;
            FMsDGV.GetSelectedFM().ReleaseDate.DateTime = EditFMReleaseDateDateTimePicker.Value;
            RefreshSelectedFMRowOnly();
            Ini.WriteFullFMDataIni();
        }

        private void EditFMLastPlayedCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (EventsDisabled) return;
            EditFMLastPlayedDateTimePicker.Visible = EditFMLastPlayedCheckBox.Checked;

            FMsDGV.GetSelectedFM().LastPlayed.DateTime = EditFMLastPlayedCheckBox.Checked
                ? EditFMLastPlayedDateTimePicker.Value
                : (DateTime?)null;

            RefreshSelectedFMRowOnly();
            Ini.WriteFullFMDataIni();
        }

        private void EditFMLastPlayedDateTimePicker_ValueChanged(object sender, EventArgs e)
        {
            if (EventsDisabled) return;
            FMsDGV.GetSelectedFM().LastPlayed.DateTime = EditFMLastPlayedDateTimePicker.Value;
            RefreshSelectedFMRowOnly();
            Ini.WriteFullFMDataIni();
        }

        private void EditFMDisabledModsTextBox_TextChanged(object sender, EventArgs e)
        {
            if (EventsDisabled) return;
            FMsDGV.GetSelectedFM().DisabledMods = EditFMDisabledModsTextBox.Text;
            RefreshSelectedFMRowOnly();
        }

        private void EditFMDisabledModsTextBox_Leave(object sender, EventArgs e)
        {
            if (EventsDisabled) return;
            Ini.WriteFullFMDataIni();
        }

        private void EditFMDisableAllModsCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (EventsDisabled) return;
            EditFMDisabledModsTextBox.Enabled = !EditFMDisableAllModsCheckBox.Checked;

            FMsDGV.GetSelectedFM().DisableAllMods = EditFMDisableAllModsCheckBox.Checked;
            RefreshSelectedFMRowOnly();
            Ini.WriteFullFMDataIni();
        }

        private void EditFMRatingComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (EventsDisabled) return;
            int rating = EditFMRatingComboBox.SelectedIndex - 1;
            FMsDGV.GetSelectedFM().Rating = rating;
            FMsDGV.SetRatingMenuItemChecked(rating);
            RefreshSelectedFMRowOnly();
            Ini.WriteFullFMDataIni();
        }

        private void EditFMLanguageComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (EventsDisabled || !FMsDGV.RowSelected()) return;

            FMsDGV.GetSelectedFM().SelectedLang = EditFMLanguageComboBox.SelectedIndex > -1
                ? EditFMLanguageComboBox.SelectedBackingItem()
                : FMLanguages.DefaultLangKey;
            Ini.WriteFullFMDataIni();
        }

        private void EditFMFinishedOnButton_Click(object sender, EventArgs e)
        {
            ShowMenu(FMsDGV.GetFinishedOnMenu(), EditFMFinishedOnButton, MenuPos.BottomRight, unstickMenu: true);
        }

        private void EditFMScanLanguagesButton_Click(object sender, EventArgs e)
        {
            ScanAndFillLanguagesBox(FMsDGV.GetSelectedFM(), forceScan: true);
            Ini.WriteFullFMDataIni();
        }

        #endregion

        #region Comment tab

        private void CommentTextBox_TextChanged(object sender, EventArgs e)
        {
            if (EventsDisabled) return;

            if (!FMsDGV.RowSelected()) return;

            var fm = FMsDGV.GetSelectedFM();

            // Converting a multiline comment to single line:
            // DarkLoader copies up to the first linebreak or the 40 char mark, whichever comes first.
            // I'm doing the same, but bumping the cutoff point to 100 chars, which is still plenty fast.
            // fm.Comment.ToEscapes() is unbounded, but I measure tenths to hundredths of a millisecond even for
            // 25,000+ character strings with nothing but slashes and linebreaks in them.
            fm.Comment = CommentTextBox.Text.ToRNEscapes();
            fm.CommentSingleLine = CommentTextBox.Text.ToSingleLineComment(100);

            RefreshSelectedFMRowOnly();
        }

        private void CommentTextBox_Leave(object sender, EventArgs e)
        {
            if (EventsDisabled) return;
            Ini.WriteFullFMDataIni();
        }

        #endregion

        #region Tags tab

        // Robustness for if the user presses tab to get away, rather than clicking
        internal void AddTagTextBoxOrListBox_Leave(object sender, EventArgs e)
        {
            if ((sender == AddTagTextBox && !AddTagLLDropDown.Focused) ||
                (AddTagLLDropDown.Constructed &&
                 sender == AddTagLLDropDown.ListBox && !AddTagTextBox.Focused))
            {
                AddTagLLDropDown.HideAndClear();
            }
        }

        private void AddTagTextBox_TextChanged(object sender, EventArgs e)
        {
            if (EventsDisabled) return;

            var list = FMTags.GetMatchingTagsList(AddTagTextBox.Text);
            if (list.Count == 0)
            {
                AddTagLLDropDown.HideAndClear();
            }
            else
            {
                AddTagLLDropDown.SetItemsAndShow(this, list);
            }
        }

        internal void AddTagTextBoxOrListBox_KeyDown(object sender, KeyEventArgs e)
        {
            AddTagLLDropDown.Construct(this);
            var box = AddTagLLDropDown.ListBox;

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
                    string catAndTag = box.SelectedIndex == -1 ? AddTagTextBox.Text : box.SelectedItem.ToString();
                    AddTagOperation(FMsDGV.GetSelectedFM(), catAndTag);
                    break;
                default:
                    if (sender == AddTagLLDropDown.ListBox) AddTagTextBox.Focus();
                    break;
            }
        }

        internal void AddTagListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            var lb = AddTagLLDropDown.ListBox;
            if (lb.SelectedIndex == -1) return;

            var tb = AddTagTextBox;

            using (new DisableEvents(this)) tb.Text = lb.SelectedItem.ToString();

            if (tb.Text.Length > 0) tb.SelectionStart = tb.Text.Length;
        }

        private void RemoveTagButton_Click(object sender, EventArgs e)
        {
            if (!FMsDGV.RowSelected()) return;

            var fm = FMsDGV.GetSelectedFM();
            var tv = TagsTreeView;

            bool success = FMTags.RemoveTagFromFM(fm, tv.SelectedNode?.Parent?.Text ?? "", tv.SelectedNode?.Text ?? "");
            if (!success) return;

            DisplayFMTags(fm.Tags);
        }

        internal void AddTagListBox_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;

            if (AddTagLLDropDown.ListBox.SelectedIndex > -1)
            {
                AddTagOperation(FMsDGV.GetSelectedFM(), AddTagLLDropDown.ListBox.SelectedItem.ToString());
            }
        }

        private void AddTagOperation(FanMission fm, string catAndTag)
        {
            if (!catAndTag.CharCountIsAtLeast(':', 2) && !catAndTag.IsWhiteSpace())
            {
                FMTags.AddTagToFM(fm, catAndTag);
                DisplayFMTags(fm.Tags);
            }

            AddTagTextBox.Clear();
            AddTagLLDropDown.HideAndClear();
        }

        private void AddTagButton_Click(object sender, EventArgs e) => AddTagOperation(FMsDGV.GetSelectedFM(), AddTagTextBox.Text);

        private void AddTagFromListButton_Click(object sender, EventArgs e)
        {
            GlobalTags.SortAndMoveMiscToEnd();

            AddTagLLMenu.Construct(this, components);
            AddTagLLMenu.Menu.Items.Clear();

            var addTagMenuItems = new List<ToolStripItem>(GlobalTags.Count);
            foreach (GlobalCatAndTags catAndTag in GlobalTags)
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

                    foreach (GlobalCatOrTag tag in catAndTag.Tags)
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

            AddTagLLMenu.Menu.Items.AddRange(addTagMenuItems.ToArray());

            ShowMenu(AddTagLLMenu.Menu, AddTagFromListButton, MenuPos.LeftDown);
        }

        private void AddTagMenuItem_Click(object sender, EventArgs e)
        {
            var item = (ToolStripMenuItem)sender;
            if (item.HasDropDownItems) return;

            var cat = item.OwnerItem;
            if (cat == null) return;

            AddTagOperation(FMsDGV.GetSelectedFM(), cat.Text + @": " + item.Text);
        }

        private void AddTagMenuCustomItem_Click(object sender, EventArgs e)
        {
            var item = (ToolStripMenuItem)sender;

            var cat = item.OwnerItem;
            if (cat == null) return;

            AddTagTextBox.SetTextAndMoveCursorToEnd(cat.Text + @": ");
        }

        private void AddTagMenuMiscItem_Click(object sender, EventArgs e) => AddTagTextBox.SetTextAndMoveCursorToEnd(((ToolStripMenuItem)sender).Text);

        private void AddTagMenuEmptyItem_Click(object sender, EventArgs e) => AddTagTextBox.SetTextAndMoveCursorToEnd(((ToolStripMenuItem)sender).Text + " ");

        // Just to keep things in a known state (clearing items also removes their event hookups, which is
        // convenient)
        [SuppressMessage("ReSharper", "MemberCanBeMadeStatic.Global")]
        internal void AddTagMenu_Closed(object sender, ToolStripDropDownClosedEventArgs e)
        {
            // This handler will only be hooked up after construction, so we don't need to call Construct()
            AddTagLLMenu.Menu.Items.Clear();
        }

        #endregion

        #region Patch tab

        private void PatchRemoveDMLButton_Click(object sender, EventArgs e)
        {
            var lb = PatchDMLsListBox;
            if (lb.SelectedIndex == -1) return;

            bool success = Core.RemoveDML(FMsDGV.GetSelectedFM(), lb.SelectedItem.ToString());
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

            foreach (string f in dmlFiles)
            {
                if (f.IsEmpty()) continue;

                bool success = Core.AddDML(FMsDGV.GetSelectedFM(), f);
                if (!success) return;

                string dmlFileName = Path.GetFileName(f);
                if (!lb.Items.Cast<string>().ToArray().ContainsI(dmlFileName))
                {
                    lb.Items.Add(dmlFileName);
                }
            }
        }

        private void PatchOpenFMFolderButton_Click(object sender, EventArgs e) => Core.OpenFMFolder(FMsDGV.GetSelectedFM());

        #endregion

        private void TopRightCollapseButton_Click(object sender, EventArgs e)
        {
            TopSplitContainer.ToggleFullScreen();
            SetTopRightCollapsedState();
        }

        private void SetTopRightCollapsedState()
        {
            bool collapsed = TopSplitContainer.FullScreen;
            TopRightTabControl.Enabled = !collapsed;
            TopRightCollapseButton.ArrowDirection = collapsed ? Direction.Left : Direction.Right;
        }

        private void TopRightMenuButton_Click(object sender, EventArgs e)
        {
            TopRightLLMenu.Construct(this, components);
            ShowMenu(TopRightLLMenu.Menu, TopRightMenuButton, MenuPos.BottomLeft);
        }

        internal void TopRightMenu_MenuItems_Click(object sender, EventArgs e)
        {
            var s = (ToolStripMenuItem)sender;

            // NULL_TODO: Null so I can assert
            TabPage? tab = null;
            for (int i = 0; i < TopRightTabsData.Count; i++)
            {
                if (s == (ToolStripMenuItem)TopRightLLMenu.Menu.Items[i])
                {
                    tab = _topRightTabsInOrder[i];
                    break;
                }
            }

            AssertR(tab != null, nameof(tab) + " is null - tab does not have a corresponding menu item");

            if (!s.Checked && TopRightTabControl.TabCount == 1)
            {
                s.Checked = true;
                return;
            }

            TopRightTabControl.ShowTab(tab!, s.Checked);
        }

        #endregion

        #region Readme

        #region Choose readme

        internal void ChooseReadmeButton_Click(object sender, EventArgs e)
        {
            // This is only hooked up after construction, so no Construct() call needed

            if (ChooseReadmeLLPanel.ListBox.Items.Count == 0 || ChooseReadmeLLPanel.ListBox.SelectedIndex == -1)
            {
                return;
            }

            var fm = FMsDGV.GetSelectedFM();
            fm.SelectedReadme = ChooseReadmeLLPanel.ListBox.SelectedBackingItem();
            ChooseReadmeLLPanel.ShowPanel(false);

            if (fm.SelectedReadme.ExtIsHtml())
            {
                ViewHTMLReadmeLLButton.Show(this);
            }
            else
            {
                SetReadmeVisible(true);
            }

            if (ChooseReadmeLLPanel.ListBox.Items.Count > 1)
            {
                ReadmeComboBoxFillAndSelect(ChooseReadmeLLPanel.ListBox.BackingItems, fm.SelectedReadme);
                ShowReadmeControls(CursorOverReadmeArea());
            }
            else
            {
                using (new DisableEvents(this)) ChooseReadmeComboBox.ClearFullItems();
                ChooseReadmeComboBox.Hide();
            }

            LoadReadme(fm);
        }

        private void ChooseReadmeComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (EventsDisabled) return;

            var fm = FMsDGV.GetSelectedFM();
            fm.SelectedReadme = ChooseReadmeComboBox.SelectedBackingItem();
            // Just load the readme; don't call DisplaySelectedFM() because that will re-get readmes and screw
            // things up
            LoadReadme(fm);
        }

        private void ChooseReadmeComboBox_DropDownClosed(object sender, EventArgs e)
        {
            if (!CursorOverReadmeArea()) ShowReadmeControls(false);
        }

        #endregion

        // Allows the readme controls to hide when the mouse moves directly from the readme area onto another
        // window. General-case showing and hiding is still handled by PreFilterMessage() for reliability.
        // Note: ChooseReadmePanel doesn't need this, because the readme controls aren't shown when it's visible.
        internal void ReadmeArea_MouseLeave(object sender, EventArgs e)
        {
            IntPtr hWnd = InteropMisc.WindowFromPoint(Cursor.Position);
            if (hWnd == IntPtr.Zero || Control.FromHandle(hWnd) == null) ShowReadmeControls(false);
        }

        [SuppressMessage("ReSharper", "MemberCanBeMadeStatic.Local")]
        private void ReadmeRichTextBox_LinkClicked(object sender, LinkClickedEventArgs e) => Core.OpenLink(e.LinkText);

        private void ReadmeZoomInButton_Click(object sender, EventArgs e) => ReadmeRichTextBox.ZoomIn();

        private void ReadmeZoomOutButton_Click(object sender, EventArgs e) => ReadmeRichTextBox.ZoomOut();

        private void ReadmeResetZoomButton_Click(object sender, EventArgs e) => ReadmeRichTextBox.ResetZoomFactor();

        private void ReadmeFullScreenButton_Click(object sender, EventArgs e)
        {
            MainSplitContainer.ToggleFullScreen();
            ShowReadmeControls(CursorOverReadmeArea());
        }

        private void SetReadmeVisible(bool enabled)
        {
            ReadmeRichTextBox.Visible = enabled;
            ReadmeZoomInButton.BackColor = enabled ? SystemColors.Window : SystemColors.Control;
            ReadmeZoomOutButton.BackColor = enabled ? SystemColors.Window : SystemColors.Control;
            ReadmeResetZoomButton.BackColor = enabled ? SystemColors.Window : SystemColors.Control;
            ReadmeFullScreenButton.BackColor = enabled ? SystemColors.Window : SystemColors.Control;

            // In case the cursor is already over the readme when we do this
            // (cause it won't show automatically if it is)
            ShowReadmeControls(enabled && CursorOverReadmeArea());
        }

        private void ShowReadmeControls(bool enabled)
        {
            ReadmeZoomInButton.Visible = enabled;
            ReadmeZoomOutButton.Visible = enabled;
            ReadmeResetZoomButton.Visible = enabled;
            ReadmeFullScreenButton.Visible = enabled;
            ChooseReadmeComboBox.Visible = enabled && ChooseReadmeComboBox.Items.Count > 0;
        }

        internal void ViewHTMLReadmeButton_Click(object sender, EventArgs e) => Core.ViewHTMLReadme(FMsDGV.GetSelectedFM());

        #endregion

        private void FiltersFlowLayoutPanel_SizeChanged(object sender, EventArgs e) => SetFilterBarScrollButtons();

        private void FiltersFlowLayoutPanel_Scroll(object sender, ScrollEventArgs e) => SetFilterBarScrollButtons();

        // PERF_TODO: This is still called too many times on startup.
        // Even though it has checks to prevent any real work from being done if not needed, I should still take
        // a look at this and see if I can't make it be called only once max on startup.
        // TODO: Something about the Construct() calls in this method causes the anchoring issue (when we lazy-load).
        // If we just construct once at the top, it works fine. But we can't do that because then it would always
        // load right away, defeating the purpose of lazy loading. Look into this. If we can solve it, that's a
        // bit more time shaved off of startup.
        // 2019-07-17: Lazy loading these is disabled for the moment.
        private void SetFilterBarScrollButtons()
        {
            // Don't run this a zillion gatrillion times during init
            if (EventsDisabled || !Visible) return;

            void ShowLeft()
            {
                FilterBarScrollLeftButton.Location = new Point(FilterBarFLP.Location.X, FilterBarFLP.Location.Y + 1);
                FilterBarScrollLeftButton.Show();
            }

            void ShowRight()
            {
                // Don't set it based on the filter bar width and location, otherwise it gets it slightly wrong
                // the first time
                FilterBarScrollRightButton.Location = new Point(
                    RefreshAreaToolStrip.Location.X - FilterBarScrollRightButton.Width - 4,
                    FilterBarFLP.Location.Y + 1);
                FilterBarScrollRightButton.Show();
            }

            var hs = FilterBarFLP.HorizontalScroll;
            if (!hs.Visible)
            {
                if (FilterBarScrollLeftButton.Visible || FilterBarScrollRightButton.Visible)
                {
                    FilterBarScrollLeftButton.Hide();
                    FilterBarScrollRightButton.Hide();
                }
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
                        InteropMisc.SendMessage(FilterBarFLP.Handle, InteropMisc.WM_SCROLL, (IntPtr)InteropMisc.SB_LINELEFT, IntPtr.Zero);
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
                        InteropMisc.SendMessage(FilterBarFLP.Handle, InteropMisc.WM_SCROLL, (IntPtr)InteropMisc.SB_LINERIGHT, IntPtr.Zero);
                    }
                }
            }
            else
            {
                ShowLeft();
                ShowRight();
            }
        }

        private void SetUIFilterValues(Filter filter)
        {
            using (new DisableEvents(this))
            {
                FilterBarFLP.SuspendDrawing();
                try
                {
                    FilterTitleTextBox.Text = filter.Title;
                    FilterAuthorTextBox.Text = filter.Author;
                    FilterShowUnsupportedButton.Checked = filter.ShowUnsupported;

                    FilterByTagsButton.Checked = !filter.Tags.IsEmpty();

                    FilterByFinishedButton.Checked = filter.Finished.HasFlagFast(FinishedState.Finished);
                    FilterByUnfinishedButton.Checked = filter.Finished.HasFlagFast(FinishedState.Unfinished);

                    FilterByRatingButton.Checked = !(filter.RatingFrom == -1 && filter.RatingTo == 10);
                    UpdateRatingLabel(suspendResume: false);

                    FilterByReleaseDateButton.Checked = filter.ReleaseDateFrom != null || filter.ReleaseDateTo != null;
                    UpdateDateLabel(lastPlayed: false, suspendResume: false);

                    FilterByLastPlayedButton.Checked = filter.LastPlayedFrom != null || filter.LastPlayedTo != null;
                    UpdateDateLabel(lastPlayed: true, suspendResume: false);
                }
                finally
                {
                    FilterBarFLP.ResumeDrawing();
                }
            }
        }

        private void PositionFilterBarAfterTabs()
        {
            int filterBarAfterTabsX;
            // In case I decide to allow a variable number of tabs based on which games are defined
            if (GamesTabControl.TabCount == 0)
            {
                filterBarAfterTabsX = 0;
            }
            else
            {
                var lastRect = GamesTabControl.GetTabRect(GamesTabControl.TabCount - 1);
                filterBarAfterTabsX = lastRect.X + lastRect.Width + 5;
            }

            FilterBarFLP.Location = new Point(filterBarAfterTabsX, FilterBarFLP.Location.Y);
            SetFilterBarWidth();
        }

        private void SetFilterBarWidth() => FilterBarFLP.Width = (RefreshAreaToolStrip.Location.X - 4) - FilterBarFLP.Location.X;

        #region Filter bar controls

        // A ton of things in one event handler to cut down on async/awaits
        private async void FilterWindowOpenButtons_Click(object sender, EventArgs e)
        {
            if (sender == FilterByReleaseDateButton || sender == FilterByLastPlayedButton)
            {
                var button = (ToolStripButtonCustom)sender;

                bool lastPlayed = button == FilterByLastPlayedButton;
                DateTime? fromDate = lastPlayed ? FMsDGV.Filter.LastPlayedFrom : FMsDGV.Filter.ReleaseDateFrom;
                DateTime? toDate = lastPlayed ? FMsDGV.Filter.LastPlayedTo : FMsDGV.Filter.ReleaseDateTo;
                string title = lastPlayed ? LText.DateFilterBox.LastPlayedTitleText : LText.DateFilterBox.ReleaseDateTitleText;

                using (var f = new FilterDateForm(title, fromDate, toDate))
                {
                    f.Location = FilterBarFLP.PointToScreen(new Point(
                        FilterIconButtonsToolStrip.Location.X + button.Bounds.X,
                        FilterIconButtonsToolStrip.Location.Y + button.Bounds.Y + button.Height));

                    if (f.ShowDialog() != DialogResult.OK) return;

                    FMsDGV.Filter.SetDateFromAndTo(lastPlayed, f.DateFrom, f.DateTo);

                    button.Checked = f.DateFrom != null || f.DateTo != null;
                }

                UpdateDateLabel(lastPlayed);
            }
            else if (sender == FilterByTagsButton)
            {
                using var tf = new FilterTagsForm(GlobalTags, FMsDGV.Filter.Tags);
                if (tf.ShowDialog() != DialogResult.OK) return;

                tf.TagsFilter.DeepCopyTo(FMsDGV.Filter.Tags);
                FilterByTagsButton.Checked = !FMsDGV.Filter.Tags.IsEmpty();
            }
            else if (sender == FilterByRatingButton)
            {
                bool outOfFive = Config.RatingDisplayStyle == RatingDisplayStyle.FMSel;
                using (var f = new FilterRatingForm(FMsDGV.Filter.RatingFrom, FMsDGV.Filter.RatingTo, outOfFive))
                {
                    f.Location =
                        FilterBarFLP.PointToScreen(new Point(
                            FilterIconButtonsToolStrip.Location.X +
                            FilterByRatingButton.Bounds.X,
                            FilterIconButtonsToolStrip.Location.Y +
                            FilterByRatingButton.Bounds.Y +
                            FilterByRatingButton.Height));

                    if (f.ShowDialog() != DialogResult.OK) return;
                    FMsDGV.Filter.SetRatingFromAndTo(f.RatingFrom, f.RatingTo);
                    FilterByRatingButton.Checked =
                        !(FMsDGV.Filter.RatingFrom == -1 && FMsDGV.Filter.RatingTo == 10);
                }

                UpdateRatingLabel();
            }

            await SortAndSetFilter();
        }

        private void UpdateDateLabel(bool lastPlayed, bool suspendResume = true)
        {
            var button = lastPlayed ? FilterByLastPlayedButton : FilterByReleaseDateButton;
            DateTime? fromDate = lastPlayed ? FMsDGV.Filter.LastPlayedFrom : FMsDGV.Filter.ReleaseDateFrom;
            DateTime? toDate = lastPlayed ? FMsDGV.Filter.LastPlayedTo : FMsDGV.Filter.ReleaseDateTo;

            // Normally you can see the re-layout kind of "sequentially happen", this stops that and makes it
            // snappy
            if (suspendResume) FilterBarFLP.SuspendDrawing();
            try
            {
                if (button.Checked)
                {
                    string from = fromDate == null ? "" : fromDate.Value.ToShortDateString();
                    string to = toDate == null ? "" : toDate.Value.ToShortDateString();

                    Lazy_ToolStripLabels.Show(this,
                        lastPlayed
                            ? Lazy_ToolStripLabel.FilterByLastPlayed
                            : Lazy_ToolStripLabel.FilterByReleaseDate, from + @" - " + to);
                }
                else
                {
                    Lazy_ToolStripLabels.Hide(lastPlayed
                        ? Lazy_ToolStripLabel.FilterByLastPlayed
                        : Lazy_ToolStripLabel.FilterByReleaseDate);
                }
            }
            finally
            {
                if (suspendResume) FilterBarFLP.ResumeDrawing();
            }
        }

        #region Filter bar right-hand controls

        internal void FMsListZoomInButton_Click(object sender, EventArgs e) => ZoomFMsDGV(ZoomFMsDGVType.ZoomIn);

        internal void FMsListZoomOutButton_Click(object sender, EventArgs e) => ZoomFMsDGV(ZoomFMsDGVType.ZoomOut);

        internal void FMsListResetZoomButton_Click(object sender, EventArgs e) => ZoomFMsDGV(ZoomFMsDGVType.ResetZoom);

        // A ton of things in one event handler to cut down on async/awaits
        private async void SortAndSetFiltersButtons_Click(object sender, EventArgs e)
        {
            if (sender == RefreshFromDiskButton)
            {
                await Core.RefreshFMsListFromDisk();
            }
            else
            {
                bool senderIsTextBox = sender == FilterTitleTextBox ||
                                       sender == FilterAuthorTextBox;
                bool senderIsGameButton = _filterByGameButtonsInOrder.Contains(sender);

                if ((senderIsTextBox || senderIsGameButton) && EventsDisabled)
                {
                    return;
                }

                if (sender == ClearFiltersButton) ClearUIAndCurrentInternalFilter();

                // Don't keep selection for these ones, cause you want to end up on the FM you typed as soon as possible
                bool keepSel = sender != FilterShowRecentAtTopButton && !senderIsTextBox;
                await SortAndSetFilter(keepSelection: keepSel);
            }
        }

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
            InteropMisc.SendMessage(FilterBarFLP.Handle, InteropMisc.WM_SCROLL, (IntPtr)direction, IntPtr.Zero);
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
                        InteropMisc.SendMessage(FilterBarFLP.Handle, InteropMisc.WM_SCROLL, (IntPtr)direction, IntPtr.Zero);
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
            var otherButton = senderButton == FilterBarScrollLeftButton ? FilterBarScrollRightButton : FilterBarScrollLeftButton;
            if (!senderButton.Visible && otherButton.Visible) _repeatButtonRunning = false;
        }

        #endregion

        #endregion

        private void ResetLayoutButton_Click(object sender, EventArgs e)
        {
            MainSplitContainer.ResetSplitterPercent();
            TopSplitContainer.ResetSplitterPercent();
            if (FilterBarScrollRightButton.Visible) SetFilterBarScrollButtons();
        }

        #region Show menu

        private enum MenuPos { LeftUp, LeftDown, TopLeft, TopRight, RightUp, RightDown, BottomLeft, BottomRight }

        private static void ShowMenu(ContextMenuStrip menu, Control control, MenuPos pos, bool unstickMenu = false)
        {
            int x = pos == MenuPos.LeftUp || pos == MenuPos.LeftDown || pos == MenuPos.TopRight || pos == MenuPos.BottomRight
                ? 0
                : control.Width;

            int y = pos == MenuPos.LeftDown || pos == MenuPos.TopLeft || pos == MenuPos.TopRight || pos == MenuPos.RightDown
                ? 0
                : control.Height;

            var direction =
                pos == MenuPos.LeftUp || pos == MenuPos.TopLeft ? ToolStripDropDownDirection.AboveLeft :
                pos == MenuPos.RightUp || pos == MenuPos.TopRight ? ToolStripDropDownDirection.AboveRight :
                pos == MenuPos.LeftDown || pos == MenuPos.BottomLeft ? ToolStripDropDownDirection.BelowLeft :
                ToolStripDropDownDirection.BelowRight;

            if (unstickMenu)
            {
                // If menu is stuck to a submenu or something, we need to show and hide it once to get it unstuck,
                // then carry on with the final show below
                menu.Show();
                menu.Hide();
            }

            menu.Show(control, new Point(x, y), direction);
        }

        #endregion

        #region Control painting

        // Perf: Where feasible, it's way faster to simply draw images vector-style on-the-fly, rather than
        // pulling rasters from Resources, because Resources is a fat bloated hog with five headcrabs on it

        private void BottomLeftButtonsFLP_Paint(object sender, PaintEventArgs e) => ControlPainter.PaintControlSeparators(e, 2, items: _bottomAreaSeparatedItems);

        private void FilterIconButtonsToolStrip_Paint(object sender, PaintEventArgs e) => ControlPainter.PaintToolStripSeparators(e, 5, _filtersToolStripSeparatedItems);

        private void RefreshAreaToolStrip_Paint(object sender, PaintEventArgs e)
        {
            // This one is a special case, so draw it explicitly here
            Pen s1Pen = ControlPainter.GetSeparatorPenForCurrentVisualStyleMode();
            const int y1 = 5;
            const int y2 = 20;

            ControlPainter.DrawSeparator(
                e: e,
                line1Pen: s1Pen,
                line1DistanceBackFromLoc: 3,
                line1Top: y1,
                line1Bottom: y2,
                x: RefreshFromDiskButton.Bounds.Location.X);

            ControlPainter.DrawSeparator(
                e: e,
                line1Pen: s1Pen,
                // Right side hack
                line1DistanceBackFromLoc: 0,
                line1Top: y1,
                line1Bottom: y2,
                // Right side hack
                x: ClearFiltersButton.Bounds.Right + 6);
        }

        private void FilterBarFLP_Paint(object sender, PaintEventArgs e) => ControlPainter.PaintControlSeparators(e, -1, 5, 20, _filterLabels);

        private void PlayFMButton_Paint(object sender, PaintEventArgs e) => ControlPainter.PaintPlayFMButton(PlayFMButton, e);

        private void PatchAddDMLButton_Paint(object sender, PaintEventArgs e) => ControlPainter.PaintPlusButton(PatchAddDMLButton, e);

        private void PatchRemoveDMLButton_Paint(object sender, PaintEventArgs e) => ControlPainter.PaintMinusButton(PatchRemoveDMLButton, e);

        private void TopRightMenuButton_Paint(object sender, PaintEventArgs e) => ControlPainter.PaintTopRightMenuButton(TopRightMenuButton, e);

        private void WebSearchButton_Paint(object sender, PaintEventArgs e) => ControlPainter.PaintWebSearchButton(WebSearchButton, e);

        private void ReadmeFullScreenButton_Paint(object sender, PaintEventArgs e) => ControlPainter.PaintReadmeFullScreenButton(ReadmeFullScreenButton, e);

        private void ResetLayoutButton_Paint(object sender, PaintEventArgs e) => ControlPainter.PaintResetLayoutButton(ResetLayoutButton, e);

        private void ScanAllFMsButton_Paint(object sender, PaintEventArgs e) => ControlPainter.PaintScanAllFMsButton(ScanAllFMsButton, e);

        // Keep this one static because it calls out to the internal ButtonPainter rather than external Core, so
        // it's fine even if we modularize the view
        // TODO: MainForm static event handler that calls out to the static control painter: Is this really fine?
        private
#if !DEBUG
        static
#endif
        void ScanIconButtons_Paint(object sender, PaintEventArgs e) => ControlPainter.PaintScanSmallButtons((Button)sender, e);

        private
#if !DEBUG
        static
#endif
        void ZoomInButtons_Paint(object sender, PaintEventArgs e) => ControlPainter.PaintZoomButtons((Button)sender, e, Zoom.In);

        private
#if !DEBUG
        static
#endif
        void ZoomOutButtons_Paint(object sender, PaintEventArgs e) => ControlPainter.PaintZoomButtons((Button)sender, e, Zoom.Out);

        private
#if !DEBUG
        static
#endif
        void ZoomResetButtons_Paint(object sender, PaintEventArgs e) => ControlPainter.PaintZoomButtons((Button)sender, e, Zoom.Reset);

        #endregion
    }
}
