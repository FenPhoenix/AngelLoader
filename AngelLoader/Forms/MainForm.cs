/* TODO: MainForm notes:
 NOTE: Don't lazy load the filter bar scroll buttons, as they screw the whole thing up (FMsDGV doesn't anchor
 in its panel correctly, etc.). If we figure out how to solve this later, we can lazy load them then.

 Images to switch to drawing:
 -Install / uninstall
 -Green CheckCircle
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

 @NET5: Fonts will change and control sizes will all change too.
 -We could go through and set font to MS Sans Serif 8.25pt everywhere. This would get us up and running quickly
  with no other changes, but we would have to remember to set it for every single control manually (including
  ones we lazy-load manually - that would preclude us from simply running a loop through all controls on the form
  and setting the font on them all that way!)
 -We could bite the bullet and go through the entire UI fixing and adjusting the layout and layout logic (including
  our "75,23" button min sizes etc!). This would give us a nicer font and a UI layout that supports it, but now
  we would have two versions to maintain (old Framework (perf on Windows), new .NET 5 (Wine support on Linux)).
 IMPORTANT: Remember to change font-size-dependent DGV zoom feature to work correctly with the new font!

 TODO: @DarkMode: Make sure all controls' disabled colors are working!
 TODO: @DarkMode: Test all parts of the app with high DPI!
 TODO: @DarkMode: Test on Win7 to make sure the dark theme still looks as it should.

 @X64: IntPtr will be 64-bit, so search for all places where we deal with them and make sure they all still work
*/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using AL_Common;
using AngelLoader.DataClasses;
using AngelLoader.Forms.CustomControls;
using AngelLoader.Forms.CustomControls.Static_LazyLoaded;
using AngelLoader.WinAPI;
using static AL_Common.CommonUtils;
using static AngelLoader.GameSupport;
using static AngelLoader.GameSupport.GameIndex;
using static AngelLoader.Logger;
using static AngelLoader.Misc;

namespace AngelLoader.Forms
{
    public sealed partial class MainForm : DarkForm, IView
    {
        #region Private fields

        private FormWindowState _nominalWindowState;
        private Size _nominalWindowSize;
        private Point _nominalWindowLocation;

        private float _fmsListDefaultFontSizeInPoints;
        private int _fmsListDefaultRowHeight;

        // To order them such that we can just look them up with an index
        private readonly TabPage[] _gameTabsInOrder;
        private readonly ToolStripButtonCustom[] _filterByGameButtonsInOrder;
        private readonly TabPage[] _topRightTabsInOrder;

        private readonly Control[] _filterLabels;
        private readonly ToolStripItem[] _filtersToolStripSeparatedItems;
        private readonly Control[] _bottomAreaSeparatedItems;

        private readonly Component[][] _hideableFilterControls;

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

        public bool EventsDisabled { get; set; }
        public bool KeyPressesDisabled { get; set; }

        // Needed for Rating column swap to prevent a possible exception when CellValueNeeded is called in the
        // middle of the operation
        internal bool CellValueNeededDisabled;

        private TransparentPanel? ViewBlockingPanel;
        private bool _viewBlocked;

        #endregion

        #region Test / debug

#if !ReleaseBeta && !ReleasePublic
        private void ForceWindowedCheckBox_CheckedChanged(object sender, EventArgs e) => Config.ForceWindowed = ForceWindowedCheckBox.Checked;

        private void T1ScreenShotModeCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (EventsDisabled) return;
            GameConfigFiles.SetScreenShotMode(Thief1, T1ScreenShotModeCheckBox.Checked);
        }

        private void T2ScreenShotModeCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (EventsDisabled) return;
            GameConfigFiles.SetScreenShotMode(Thief2, T2ScreenShotModeCheckBox.Checked);
        }

        public void UpdateGameScreenShotModes()
        {
            using (new DisableEvents(this))
            {
                bool? t1 = GameConfigFiles.GetScreenShotMode(Thief1);
                bool? t2 = GameConfigFiles.GetScreenShotMode(Thief2);

                T1ScreenShotModeCheckBox.Visible = t1 != null;
                T2ScreenShotModeCheckBox.Visible = t2 != null;

                if (t1 != null) T1ScreenShotModeCheckBox.Checked = (bool)t1;
                if (t2 != null) T2ScreenShotModeCheckBox.Checked = (bool)t2;
            }
        }
#endif

        private readonly List<KeyValuePair<Control, (Color ForeColor, Color BackColor)>> _controlColors = new();

#if DEBUG || (Release_Testing && !RT_StartupOnly)

        private void TestButton_Click(object sender, EventArgs e)
        {
            Config.VisualTheme = Config.DarkMode ? VisualTheme.Classic : VisualTheme.Dark;
            SetTheme(Config.VisualTheme);
        }

        private void Test2Button_Click(object sender, EventArgs e)
        {
            var cs = ReadmeRichTextBox.GetRTFColorStyle() switch
            {
                RTFColorStyle.Original => RTFColorStyle.Auto,
                RTFColorStyle.Auto => RTFColorStyle.Monochrome,
                _ => RTFColorStyle.Original,
            };

            ReadmeRichTextBox.SetRTFColorStyle(cs);

            return;

            Width = 1305;
            Height = 750;
        }

        private void Test3Button_Click(object sender, EventArgs e)
        {

        }

        private void Test4Button_Click(object sender, EventArgs e)
        {

        }

#endif

#if DEBUG || (Release_Testing && !RT_StartupOnly)
        public string GetDebug1Text() => DebugLabel.Text;
        public string GetDebug2Text() => DebugLabel2.Text;
        public void SetDebug1Text(string value) => DebugLabel.Text = value;
        public void SetDebug2Text(string value) => DebugLabel2.Text = value;
#endif

        #endregion

        #region Message handling

        protected override void WndProc(ref Message m)
        {
            // A second instance has been started and told us to show ourselves, so do it here (nicer UX).
            // This has to be in WndProc, not PreFilterMessage(). Shrug.
            if (m.Msg == Native.WM_SHOWFIRSTINSTANCE)
            {
                if (WindowState == FormWindowState.Minimized) WindowState = _nominalWindowState;
                Activate();
            }
            else if (m.Msg == Native.WM_THEMECHANGED)
            {
                NativeHooks.ReloadTheme();
            }
            base.WndProc(ref m);
        }

        public bool PreFilterMessage(ref Message m)
        {
            // So I don't forget what the return values do
            const bool BlockMessage = true;
            const bool PassMessageOn = false;

            // Note: CanFocus will be false if there are modal windows open

            // This allows controls to be scrolled with the mousewheel when the mouse is over them, without
            // needing to actually be focused. Vital for a good user experience.

            #region Mouse

            if (m.Msg == Native.WM_MOUSEWHEEL)
            {
                // IMPORTANT (PreFilterMessage):
                // Do this check inside each if block rather than above, because the message may not
                // be a mousemove message, and in that case we'd be trying to get a window point from a random
                // value, and that causes the min,max,close button flickering.
                if (!TryGetHWndFromMousePos(m, out IntPtr hWnd)) return PassMessageOn;

                if (_viewBlocked || CursorOutsideAddTagsDropDownArea()) return BlockMessage;

                int delta = Native.SignedHIWORD(m.WParam);
                if (CanFocus && CursorOverControl(FilterBarFLP) && !CursorOverControl(FMsDGV))
                {
                    // Allow the filter bar to be mousewheel-scrolled with the buttons properly appearing
                    // and disappearing as appropriate
                    if (delta != 0)
                    {
                        int direction = delta > 0 ? Native.SB_LINELEFT : Native.SB_LINERIGHT;
                        int origSmallChange = FilterBarFLP.HorizontalScroll.SmallChange;

                        FilterBarFLP.HorizontalScroll.SmallChange = 45;

                        Native.SendMessage(FilterBarFLP.Handle, Native.WM_SCROLL, (IntPtr)direction, IntPtr.Zero);

                        FilterBarFLP.HorizontalScroll.SmallChange = origSmallChange;
                    }
                }
                else if (CanFocus && CursorOverControl(FMsDGV) && (Native.LOWORD(m.WParam)) == Native.MK_CONTROL)
                {
                    if (delta != 0) ZoomFMsDGV(delta > 0 ? ZoomFMsDGVType.ZoomIn : ZoomFMsDGVType.ZoomOut);
                }
                else
                {
                    // Stupid hack to fix "send mousewheel to underlying control and block further messages"
                    // functionality still not being fully reliable. We need to focus the parent control sometimes
                    // inexplicably. Sure. Whole point is to avoid having to do that, but sure.
                    if (!(AddTagLLDropDown.Constructed && CursorOverControl(AddTagLLDropDown.ListBox, fullArea: true)))
                    {
                        if (CursorOverControl(TopSplitContainer.Panel2))
                        {
                            TopSplitContainer.Panel2.Focus();
                        }
                        else if (CursorOverControl(MainSplitContainer.Panel2) &&
                                 !ReadmeRichTextBox.Focused)
                        {
                            MainSplitContainer.Panel2.Focus();
                        }
                    }
                    Native.SendMessage(hWnd, m.Msg, m.WParam, m.LParam);
                }
                return BlockMessage;
            }
            else if (m.Msg == Native.WM_MOUSEHWHEEL)
            {
                if (!TryGetHWndFromMousePos(m, out _)) return PassMessageOn;

                if (_viewBlocked) return BlockMessage;

                if (CanFocus && CursorOverControl(FMsDGV))
                {
                    int delta = Native.SignedHIWORD(m.WParam);
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
            else if (m.Msg == Native.WM_MOUSEMOVE || m.Msg == Native.WM_NCMOUSEMOVE)
            {
                if (!CanFocus) return PassMessageOn;

                if (CursorOutsideAddTagsDropDownArea() || _viewBlocked) return BlockMessage;

                ShowReadmeControls(CursorOverReadmeArea());
            }
            else if (m.Msg == Native.WM_LBUTTONDOWN || m.Msg == Native.WM_NCLBUTTONDOWN ||
                     m.Msg == Native.WM_MBUTTONDOWN || m.Msg == Native.WM_NCMBUTTONDOWN ||
                     m.Msg == Native.WM_RBUTTONDOWN || m.Msg == Native.WM_NCRBUTTONDOWN ||
                     m.Msg == Native.WM_LBUTTONDBLCLK || m.Msg == Native.WM_NCLBUTTONDBLCLK ||
                     m.Msg == Native.WM_MBUTTONDBLCLK || m.Msg == Native.WM_NCMBUTTONDBLCLK ||
                     m.Msg == Native.WM_RBUTTONDBLCLK || m.Msg == Native.WM_NCRBUTTONDBLCLK ||
                     m.Msg == Native.WM_LBUTTONUP || m.Msg == Native.WM_NCLBUTTONUP ||
                     m.Msg == Native.WM_MBUTTONUP || m.Msg == Native.WM_NCMBUTTONUP ||
                     m.Msg == Native.WM_RBUTTONUP || m.Msg == Native.WM_NCRBUTTONUP)
            {
                if (!CanFocus) return PassMessageOn;

                if (_viewBlocked)
                {
                    return BlockMessage;
                }
                else if (CursorOutsideAddTagsDropDownArea())
                {
                    AddTagLLDropDown.HideAndClear();
                    if (m.Msg != Native.WM_LBUTTONUP &&
                        m.Msg != Native.WM_MBUTTONUP &&
                        m.Msg != Native.WM_RBUTTONUP &&

                        m.Msg != Native.WM_NCLBUTTONUP &&
                        m.Msg != Native.WM_NCMBUTTONUP &&
                        m.Msg != Native.WM_NCRBUTTONUP &&

                        m.Msg != Native.WM_NCLBUTTONDOWN &&
                        m.Msg != Native.WM_NCMBUTTONDOWN &&
                        m.Msg != Native.WM_NCRBUTTONDOWN &&

                        m.Msg != Native.WM_NCLBUTTONDBLCLK &&
                        m.Msg != Native.WM_NCMBUTTONDBLCLK &&
                        m.Msg != Native.WM_NCRBUTTONDBLCLK)
                    {
                        return BlockMessage;
                    }
                }
                else if (m.Msg == Native.WM_MBUTTONDOWN && CursorOverControl(FMsDGV))
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
            else if (m.Msg == Native.WM_SYSKEYDOWN)
            {
                int wParam = (int)m.WParam;
                if (ModifierKeys == Keys.Alt && wParam == (int)Keys.F4) return PassMessageOn;
            }
            // Any other keys have to use this.
            else if (m.Msg == Native.WM_KEYDOWN)
            {
                if (KeyPressesDisabled || _viewBlocked) return BlockMessage;

                int wParam = (int)m.WParam;
                if (wParam == (int)Keys.F1 && CanFocus)
                {
                    static bool AnyControlFocusedIn(Control control, int stackCounter = 0)
                    {
                        try
                        {
                            stackCounter++;
                            if (stackCounter > 100) return false;

                            if (control.Focused) return true;

                            for (int i = 0; i < control.Controls.Count; i++)
                            {
                                if (AnyControlFocusedIn(control.Controls[i], stackCounter)) return true;
                            }

                            return false;
                        }
                        finally
                        {
                            stackCounter--;
                        }
                    }

                    bool AnyControlFocusedInTabPage(TabPage tabPage) =>
                        (TopRightTabControl.Focused && TopRightTabControl.SelectedTab == tabPage) ||
                        AnyControlFocusedIn(tabPage);

                    bool mainMenuWasOpen = MainLLMenu.Visible;

                    string section =
                        !EverythingPanel.Enabled ? HelpSections.MainWindow :
                        mainMenuWasOpen ? HelpSections.MainMenu :
                        FMsDGV_FM_LLMenu.Visible ? HelpSections.FMContextMenu :
                        FMsDGV_ColumnHeaderLLMenu.Visible ? HelpSections.ColumnHeaderContextMenu :
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

                    // Otherwise, F1 activates the menu item marked F1, but we want to handle it manually
                    if (mainMenuWasOpen) return BlockMessage;
                }
            }
            else if (m.Msg == Native.WM_KEYUP)
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

            NativeHooks.InstallHooks();

            #region Manual control init

            ReadmeFullScreenButton.DarkModeBackColor = DarkColors.Fen_DarkBackground;
            ReadmeZoomInButton.DarkModeBackColor = DarkColors.Fen_DarkBackground;
            ReadmeZoomOutButton.DarkModeBackColor = DarkColors.Fen_DarkBackground;
            ReadmeResetZoomButton.DarkModeBackColor = DarkColors.Fen_DarkBackground;

            // The other Rating column, there has to be two, one for text and one for images
            RatingImageColumn = new DataGridViewImageColumn
            {
                ImageLayout = DataGridViewImageCellLayout.Zoom,
                ReadOnly = true,
                Width = _ratingImageColumnWidth,
                Resizable = DataGridViewTriState.False,
                SortMode = DataGridViewColumnSortMode.Programmatic
            };

            #endregion

            MainMenuButton.HideFocusRectangle();

            //Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);

            #region Init non-public-release controls

#if DEBUG || (Release_Testing && !RT_StartupOnly)
            #region Init debug-only controls

            TestButton = new DarkButton();
            Test2Button = new DarkButton();
            Test3Button = new DarkButton();
            Test4Button = new DarkButton();
            DebugLabel = new DarkLabel();
            DebugLabel2 = new DarkLabel();

            BottomPanel.Controls.Add(TestButton);
            BottomPanel.Controls.Add(Test2Button);
            BottomPanel.Controls.Add(Test3Button);
            BottomPanel.Controls.Add(Test4Button);
            BottomPanel.Controls.Add(DebugLabel);
            BottomPanel.Controls.Add(DebugLabel2);

            TestButton.Location = new Point(650, 0);
            TestButton.Size = new Size(75, 22);
            TestButton.TabIndex = 999;
            TestButton.Text = "Test";
            TestButton.UseVisualStyleBackColor = true;
            TestButton.Click += TestButton_Click;

            Test2Button.Location = new Point(650, 21);
            Test2Button.Size = new Size(75, 22);
            Test2Button.TabIndex = 999;
            Test2Button.Text = "Test2";
            Test2Button.UseVisualStyleBackColor = true;
            Test2Button.Click += Test2Button_Click;

            Test3Button.Location = new Point(725, 0);
            Test3Button.Size = new Size(75, 22);
            Test3Button.TabIndex = 999;
            Test3Button.Text = "Test3";
            Test3Button.UseVisualStyleBackColor = true;
            Test3Button.Click += Test3Button_Click;

            Test4Button.Location = new Point(725, 21);
            Test4Button.Size = new Size(75, 22);
            Test4Button.TabIndex = 999;
            Test4Button.Text = "Test4";
            Test4Button.UseVisualStyleBackColor = true;
            Test4Button.Click += Test4Button_Click;

            DebugLabel.AutoSize = true;
            DebugLabel.Location = new Point(804, 8);
            DebugLabel.Size = new Size(71, 13);
            DebugLabel.TabIndex = 29;
            DebugLabel.Text = "[DebugLabel]";

            DebugLabel2.AutoSize = true;
            DebugLabel2.Location = new Point(804, 24);
            DebugLabel2.Size = new Size(77, 13);
            DebugLabel2.TabIndex = 32;
            DebugLabel2.Text = "[DebugLabel2]";

            #endregion
#endif

#if !ReleaseBeta && !ReleasePublic
            ForceWindowedCheckBox = new DarkCheckBox { AutoSize = true, Dock = DockStyle.Fill, Text = "Force windowed" };
            BottomRightButtonsFLP.Controls.Add(ForceWindowedCheckBox);
            ForceWindowedCheckBox.CheckedChanged += ForceWindowedCheckBox_CheckedChanged;

            T1ScreenShotModeCheckBox = new DarkCheckBox { AutoSize = true, Dock = DockStyle.Fill, Text = "T1 SSM" };
            T2ScreenShotModeCheckBox = new DarkCheckBox { AutoSize = true, Dock = DockStyle.Fill, Text = "T2 SSM" };
            // Add in reverse order because the flow layout panel is right-to-left I guess?
            BottomRightButtonsFLP.Controls.Add(T2ScreenShotModeCheckBox);
            BottomRightButtonsFLP.Controls.Add(T1ScreenShotModeCheckBox);
            T1ScreenShotModeCheckBox.CheckedChanged += T1ScreenShotModeCheckBox_CheckedChanged;
            T2ScreenShotModeCheckBox.CheckedChanged += T2ScreenShotModeCheckBox_CheckedChanged;
#endif

            #endregion

            #region Control arrays

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
            _topRightTabsInOrder = new TabPage[]
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

            _hideableFilterControls = new[]
            {
                new Component[] { FilterTitleLabel, FilterTitleTextBox },
                new Component[] { FilterAuthorLabel, FilterAuthorTextBox },
                new Component[] { FilterByReleaseDateButton },
                new Component[] { FilterByLastPlayedButton },
                new Component[] { FilterByTagsButton },
                new Component[] { FilterByFinishedButton, FilterByUnfinishedButton },
                new Component[] { FilterByRatingButton },
                new Component[] { FilterShowUnsupportedButton },
                new Component[] { FilterShowRecentAtTopButton }
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
            Text = "AngelLoader " + Application.ProductVersion;

            FMsDGV.InjectOwner(this);

            #region Set up form and control state

            // Set here in init method so as to avoid the changes being visible.
            // Set here specifically (before anything else) so that splitter positioning etc. works right.
            SetWindowStateAndSize();

            #region Top-right tabs

            var sortedTabPages = new SortedDictionary<int, TabPage>();
            for (int i = 0; i < TopRightTabsData.Count; i++)
            {
                sortedTabPages.Add(Config.TopRightTabsData.Tabs[i].DisplayIndex, _topRightTabsInOrder[i]);
            }

            var tabs = new List<TabPage>(sortedTabPages.Count);
            foreach (var item in sortedTabPages) tabs.Add(item.Value);

            // This removes any existing tabs so it works even though we always add all tabs in component init now
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
            MainSplitContainer.Panel1DarkBackColor = DarkColors.Fen_ControlBackground;
            MainSplitContainer.Panel2DarkBackColor = DarkColors.Fen_DarkBackground;
            TopSplitContainer.InjectSibling(MainSplitContainer);
            TopSplitContainer.Panel1DarkBackColor = DarkColors.Fen_ControlBackground;
            TopSplitContainer.Panel2DarkBackColor = DarkColors.Fen_DarkBackground;

            #endregion

            #region FMs DataGridView

            _fmsListDefaultFontSizeInPoints = FMsDGV.DefaultCellStyle.Font.SizeInPoints;
            _fmsListDefaultRowHeight = FMsDGV.RowTemplate.Height;

            #region Columns

            FinishedColumn.Width = _finishedColumnWidth;

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
            ReadmeRichTextBox.SetRTFColorStyle(Config.RTFThemedColorStyle, startup: true);

            #endregion

            #region Filters

            #region Set filter control visibilities

            FilterControlsLLMenu.SetCheckedStates(Config.FilterControlVisibilities);

            for (int fiI = 0; fiI < HideableFilterControlsCount; fiI++)
            {
                Component[] filterItems = _hideableFilterControls[fiI];
                for (int i = 0; i < filterItems.Length; i++)
                {
                    switch (filterItems[i])
                    {
                        case Control control:
                            control.Visible = Config.FilterControlVisibilities[fiI];
                            break;
                        case ToolStripItem toolStripItem:
                            toolStripItem.Visible = Config.FilterControlVisibilities[fiI];
                            break;
                    }
                }
            }

            #endregion

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

            // This button is a weird special case (see its class) so we just construct it here and it will be
            // shown when localized.
            // TODO (inst/uninst button): We might be able to wrangle this into something cleaner nonetheless.
            if (!Config.HideUninstallButton) InstallUninstallFMLLButton.Construct(this);
            if (!Config.HideExitButton) ExitLLButton.SetVisible(this, true);

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

#if !ReleaseBeta && !ReleasePublic
            UpdateGameScreenShotModes();
#endif

            // PERF: If we're in the classic theme, we don't need to do anything
            // Do this here to prevent double-loading of RTF/GLML readmes
            if (Config.DarkMode)
            {
                SetTheme(Config.VisualTheme, startup: true, alsoCreateControlHandles: true);
            }
            else
            {
                ControlUtils.CreateAllControlsHandles(this);
                Images.ReloadImages();
            }

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
                await DisplaySelectedFM();
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
#if DEBUG || (Release_Testing && !RT_StartupOnly)
            if (e.Control && e.KeyCode == Keys.E)
            {
                EnableEverything(!EverythingPanel.Enabled);
                return;
            }
            // For separating log spam
            else if (e.Control && e.KeyCode == Keys.T)
            {
                System.Diagnostics.Trace.WriteLine("");
            }
#endif

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
                    TextBox? textBox =
                        FilterTitleTextBox.Visible ? FilterTitleTextBox :
                        FilterAuthorTextBox.Visible ? FilterAuthorTextBox :
                        null;

                    if (textBox != null)
                    {
                        textBox.Focus();
                        textBox.SelectAll();
                    }
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
                ControlUtils.ShowAlert(
                    LText.AlertMessages.AppClosing_OperationInProgress,
                    LText.AlertMessages.Alert);
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

        #region ISettingsChangeableWindow

        public void Localize() => Localize(startup: false);

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
                MainToolTip.SetToolTip(MainMenuButton, LText.MainMenu.MainMenuToolTip);
                MainLLMenu.Localize();

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

                FilterControlsShowHideButton.ToolTipText = LText.FilterBar.ShowHideMenuToolTip;

                #endregion

                #region Clear/refresh/reset area

                RefreshFromDiskButton.ToolTipText = LText.FilterBar.RefreshFromDiskButtonToolTip;
                RefreshFiltersButton.ToolTipText = LText.FilterBar.RefreshFilteredListButtonToolTip;
                ClearFiltersButton.ToolTipText = LText.FilterBar.ClearFiltersButtonToolTip;
                MainToolTip.SetToolTip(ResetLayoutButton, LText.FilterBar.ResetLayoutButtonToolTip);

                #endregion

                #region FMs list

                FMsDGV_ColumnHeaderLLMenu.Localize();
                FMsDGV_FM_LLMenu.Localize();

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

                MainToolTip.SetToolTip(ReadmeZoomInButton, LText.Global.ZoomIn);
                MainToolTip.SetToolTip(ReadmeZoomOutButton, LText.Global.ZoomOut);
                MainToolTip.SetToolTip(ReadmeResetZoomButton, LText.Global.ResetZoom);
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

                SettingsButton.Text = LText.MainButtons.Settings;
                ExitLLButton.Localize();

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

        public void SetTheme(VisualTheme theme) => SetTheme(theme, startup: false);

        private void SetTheme(VisualTheme theme, bool startup, bool alsoCreateControlHandles = false)
        {
            bool darkMode = theme == VisualTheme.Dark;

            try
            {
                if (!startup) this.SuspendDrawing();

                // TODO: @DarkMode(SetTheme excludes): We need to exclude lazy-loaded controls also.
                // Figure out some way to just say "if a control is part of a lazy-loaded class" so we don't
                // have to write them out manually here again and keep both places in sync.
                // 2021-03-25:
                // Normally a duplicate-set situation would never happen, because if nothing has been lazy-loaded
                // yet by the time we get here, we'll only fill our dictionary with controls that are loaded
                // already, and then we won't ever update the dictionary again.
                // HOWEVER! At least two things (install/uninstall button, exit button) may well have been
                // constructed already by the time we get here, so duplicate setting of those would happen
                // in that case.
                // This is only a perf/efficiency issue, not a bug, we can release with this and we're fine, just
                // suboptimally efficient.
                // We can try to put the startup call to this even earlier to put it before the "setup form and
                // control state" section to preempt any loading of lazy-loaded controls, but that would put it
                // in InitThreadable() rather than FinishInitAndShow() (probably not a problem?) and also we're
                // not 100% sure if there's anything in the form/control state setup that needs to get done before
                // us. We should go through it with a fine-tooth comb and test it if we want to do this optimization.

                ControlUtils.ChangeFormThemeMode(
                    theme,
                    this,
                    _controlColors,
                    x => x.EqualsIfNotNull(ProgressBox)
                         || (_progressBoxConstructed && x is Control xControl && ProgressBox!.Controls.Contains(xControl))
                         || x is SplitterPanel,
                    alsoCreateControlHandles: alsoCreateControlHandles,
                    capacity: 150
                );

                SetReadmeButtonsBackColor(ReadmeRichTextBox.Visible, theme);

                // Set these first so other controls get the right data when they reference them
                Images.DarkModeEnabled = darkMode;
                Images.ReloadImages();

                ControlUtils.RecreateAllToolTipHandles();

                MainLLMenu.DarkModeEnabled = darkMode;
                FMsDGV_FM_LLMenu.DarkModeEnabled = darkMode;
                FMsDGV_ColumnHeaderLLMenu.DarkModeEnabled = darkMode;
                TopRightLLMenu.DarkModeEnabled = darkMode;
                AddTagLLMenu.DarkModeEnabled = darkMode;
                AddTagLLDropDown.DarkModeEnabled = darkMode;
                AltTitlesLLMenu.DarkModeEnabled = darkMode;
                FilterControlsLLMenu.DarkModeEnabled = darkMode;
                PlayOriginalGameLLMenu.DarkModeEnabled = darkMode;
                InstallUninstallFMLLButton.DarkModeEnabled = darkMode;
                ExitLLButton.DarkModeEnabled = darkMode;
                ViewHTMLReadmeLLButton.DarkModeEnabled = darkMode;
                ProgressBoxDarkModeEnabled = darkMode;
                Lazy_FMsListZoomButtons.DarkModeEnabled = darkMode;
                ChooseReadmeLLPanel.DarkModeEnabled = darkMode;
                RefreshFiltersButton.Image = Images.Refresh;
                Lazy_ToolStripLabels.DarkModeEnabled = darkMode;

                FilterByThief1Button.Image = Images.Thief1_21;
                FilterByThief2Button.Image = Images.Thief2_21;
                FilterByThief3Button.Image = Images.Thief3_21;
                FilterBySS2Button.Image = Images.Shock2_21;

                GameTabsImageList.Images[0] = Images.Thief1_16;
                GameTabsImageList.Images[1] = Images.Thief2_16;
                GameTabsImageList.Images[2] = Images.Thief3_16;
                GameTabsImageList.Images[3] = Images.Shock2_16;

                // Have to do this or else they don't show up if we start in dark mode, but they do if we switch
                // while running(?) meh, whatever
                ReadmeZoomInButton.BringToFront();
                ReadmeZoomOutButton.BringToFront();
                ReadmeResetZoomButton.BringToFront();
                ReadmeFullScreenButton.BringToFront();
                ChooseReadmeComboBox.BringToFront();
            }
            finally
            {
                if (!startup) this.ResumeDrawing();
            }
        }

        #endregion

        #region Helpers & misc

        #region Invoke

        public object InvokeSync(Delegate method) => Invoke(method);
        //public object InvokeSync(Delegate method, params object[] args) => Invoke(method, args);

        #endregion

        #region Show menu

        private enum MenuPos { LeftUp, LeftDown, TopLeft, TopRight, RightUp, RightDown, BottomLeft, BottomRight }

        private static void ShowMenu(ContextMenuStrip menu, Control control, MenuPos pos,
                                     int xOffset = 0, int yOffset = 0, bool unstickMenu = false)
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

            menu.Show(control, new Point(x + xOffset, y + yOffset), direction);
        }

        #endregion

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
                FilterControlsLLMenu.GetCheckedStates(),
                selectedFM,
                FMsDGV.GameTabsState,
                gameTab,
                topRightTabs,
                TopSplitContainer.FullScreen,
                ReadmeRichTextBox.ZoomFactor,
                ReadmeRichTextBox.GetRTFColorStyle());
        }

        internal IContainer GetComponents() => components;

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

        private static bool TryGetHWndFromMousePos(Message msg, out IntPtr result)
        {
            Point pos = new Point(Native.SignedLOWORD(msg.LParam), Native.SignedHIWORD(msg.LParam));
            result = Native.WindowFromPoint(pos);
            return result != IntPtr.Zero && Control.FromHandle(result) != null;
        }

        #endregion

        #region Main menu

        private void MainMenuButton_Click(object sender, EventArgs e)
        {
            MainLLMenu.Construct(this, components);
            ShowMenu(MainLLMenu.Menu, MainMenuButton, MenuPos.BottomRight, xOffset: -2, yOffset: 2);
        }

        private void MainMenuButton_Enter(object sender, EventArgs e) => MainMenuButton.HideFocusRectangle();

        #endregion

        #region Filter bar

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

        #region Game tabs

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
        public bool GetShowRecentAtTop() => FilterShowRecentAtTopButton.Checked;

        public List<int> GetFilterShownIndexList() => FMsDGV.FilterShownIndexList;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int TopBarXZero() => MainMenuButton.Left + MainMenuButton.Width + 8;

        // Separate so we can call it from _Load on startup (because it needs the form to be loaded to layout the
        // controls properly) but keep the rest of the work before load
        private void ChangeFilterControlsForGameType()
        {
            if (Config.GameOrganization == GameOrganization.OneList)
            {
                GamesTabControl.Hide();
                // Don't inline this var - it stores the X value to persist it through a change
                int plusWidth = FilterBarFLP.Location.X - TopBarXZero();
                FilterBarFLP.Location = new Point(TopBarXZero(), FilterBarFLP.Location.Y);
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
                    FilterTitleTextBox.Clear();
                    FilterAuthorTextBox.Clear();

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
                    FMsDGV.Filter.ClearAll(oneList);
                }
                finally
                {
                    FilterBarFLP.ResumeDrawing();
                }
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
                filterBarAfterTabsX = TopBarXZero();
            }
            else
            {
                var lastRect = GamesTabControl.GetTabRect(GamesTabControl.TabCount - 1);
                filterBarAfterTabsX = TopBarXZero() + lastRect.X + lastRect.Width + 5;
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
                            : Lazy_ToolStripLabel.FilterByReleaseDate, from + " - " + to);
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
        private async void Filters_Changed(object sender, EventArgs e)
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

                // Don't keep selection for title/author, cause you want to end up on the FM you typed as soon as
                // possible
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
            int direction = sender == FilterBarScrollLeftButton ? Native.SB_LINELEFT : Native.SB_LINERIGHT;
            Native.SendMessage(FilterBarFLP.Handle, Native.WM_SCROLL, (IntPtr)direction, IntPtr.Zero);
        }

        private void FilterBarScrollButtons_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            RunRepeatButton(sender == FilterBarScrollLeftButton ? Native.SB_LINELEFT : Native.SB_LINERIGHT);
        }

        private void RunRepeatButton(int direction)
        {
            if (_repeatButtonRunning) return;
            _repeatButtonRunning = true;
            Task.Run(() =>
            {
                while (_repeatButtonRunning)
                {
                    Invoke(new Action(() => Native.SendMessage(FilterBarFLP.Handle, Native.WM_SCROLL, (IntPtr)direction, IntPtr.Zero)));
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

        private void FilterBarFLP_SizeChanged(object sender, EventArgs e) => SetFilterBarScrollButtons();

        private void FilterBarFLP_Scroll(object sender, ScrollEventArgs e) => SetFilterBarScrollButtons();

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
                        Native.SendMessage(FilterBarFLP.Handle, Native.WM_SCROLL, (IntPtr)Native.SB_LINELEFT, IntPtr.Zero);
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
                        Native.SendMessage(FilterBarFLP.Handle, Native.WM_SCROLL, (IntPtr)Native.SB_LINERIGHT, IntPtr.Zero);
                    }
                }
            }
            else
            {
                ShowLeft();
                ShowRight();
            }
        }

        #endregion

        #endregion

        private void ResetLayoutButton_Click(object sender, EventArgs e)
        {
            MainSplitContainer.ResetSplitterPercent();
            TopSplitContainer.ResetSplitterPercent();
            if (FilterBarScrollRightButton.Visible) SetFilterBarScrollButtons();
        }

        #endregion

        #region Filter controls visibility menu

        private void FilterControlsShowHideButton_Click(object sender, EventArgs e)
        {
            FilterControlsLLMenu.Construct(this, components);
            ShowMenu(FilterControlsLLMenu.Menu,
                FilterIconButtonsToolStrip,
                MenuPos.RightDown,
                -FilterControlsShowHideButton.Width,
                FilterIconButtonsToolStrip.Height);
        }

        internal async void FilterControlsMenuItems_Click(object sender, EventArgs e)
        {
            var s = (ToolStripMenuItemCustom)sender;

            try
            {
                FilterBarFLP.SuspendDrawing();

                var filterItems = _hideableFilterControls[(int)s.Tag];
                for (int i = 0; i < filterItems.Length; i++)
                {
                    switch (filterItems[i])
                    {
                        case Control control:
                            control.Visible = s.Checked;
                            if (control is TextBox textBox && !s.Checked)
                            {
                                // Just let the normal event handle it
                                textBox.Clear();
                            }
                            break;
                        case ToolStripItem toolStripItem:
                            toolStripItem.Visible = s.Checked;
                            if (toolStripItem is ToolStripButton toolStripButton && !s.Checked)
                            {
                                // Some buttons aren't just toggles but bring up windows, so we can't just call
                                // PerformClick() on them. So let's do like in the clear filters method but just
                                // for this one filter.
                                bool buttonWasChecked = toolStripButton.Checked;
                                toolStripButton.Checked = false;
                                var filterControl = (HideableFilterControls)s.Tag;
                                switch (filterControl)
                                {
                                    case HideableFilterControls.ReleaseDate:
                                        Lazy_ToolStripLabels.Hide(Lazy_ToolStripLabel.FilterByReleaseDate);
                                        break;
                                    case HideableFilterControls.LastPlayed:
                                        Lazy_ToolStripLabels.Hide(Lazy_ToolStripLabel.FilterByLastPlayed);
                                        break;
                                    case HideableFilterControls.Rating:
                                        Lazy_ToolStripLabels.Hide(Lazy_ToolStripLabel.FilterByRating);
                                        break;
                                }

                                FMsDGV.Filter.ClearHideableFilter(filterControl);

                                bool keepSel = toolStripButton != FilterShowRecentAtTopButton ||
                                               !buttonWasChecked;
                                // Note: It's fine that we call this in the middle of a suspend/resume block,
                                // because we're suspend/resuming the filter bar, whereas this is suspend/resuming
                                // the FMs list.
                                await SortAndSetFilter(keepSelection: keepSel);
                            }
                            break;
                    }
                }
            }
            finally
            {
                FilterBarFLP.ResumeDrawing();
            }
        }

        #endregion

        #region Refresh FMs list

        public void RefreshSelectedFM(bool rowOnly = false)
        {
            FMsDGV.InvalidateRow(FMsDGV.SelectedRows[0].Index);

            if (rowOnly) return;

            UpdateAllFMUIDataExceptReadme(FMsDGV.GetSelectedFM());
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
                    CellValueNeededDisabled = true;
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
                        CellValueNeededDisabled = false;
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
                // TODO: This pops our position back to put selected FM in view - but do we really need to run this here?
                // Alternatively, maybe SelectProperly() should pop us back to where we were after it's done?
                FMsDGV.SelectProperly();
            }
        }

        #endregion

        #region Top-right area

        // Hook them all up to one event handler to avoid extraneous async/awaits
        private async void FieldScanButtons_Click(object sender, EventArgs e)
        {
            if (sender == EditFMScanForReadmesButton)
            {
                Ini.WriteFullFMDataIni();
                await DisplaySelectedFM(refreshCache: true);
            }
            else
            {
                var scanOptions =
                    sender == EditFMScanTitleButton ? FMScanner.ScanOptions.FalseDefault(scanTitle: true) :
                    sender == EditFMScanAuthorButton ? FMScanner.ScanOptions.FalseDefault(scanAuthor: true) :
                    sender == EditFMScanReleaseDateButton ? FMScanner.ScanOptions.FalseDefault(scanReleaseDate: true) :
                    //sender == StatsScanCustomResourcesButton
                    FMScanner.ScanOptions.FalseDefault(scanCustomResources: true);

                if (await FMScan.ScanFMs(new List<FanMission> { FMsDGV.GetSelectedFM() }, scanOptions, hideBoxIfZip: true))
                {
                    RefreshSelectedFM();
                }
            }
        }

        #region Edit FM tab

        private void EditFMAltTitlesArrowButton_Click(object sender, EventArgs e)
        {
            AltTitlesLLMenu.Construct(components);
            FillAltTitlesMenu(FMsDGV.GetSelectedFM().AltTitles);
            ShowMenu(AltTitlesLLMenu.Menu, EditFMAltTitlesArrowButton, MenuPos.BottomLeft);
        }

        private void EditFMAltTitlesMenuItems_Click(object sender, EventArgs e)
        {
            EditFMTitleTextBox.Text = ((ToolStripMenuItemCustom)sender).Text;
            Ini.WriteFullFMDataIni();
        }

        private void EditFMTitleTextBox_TextChanged(object sender, EventArgs e)
        {
            if (EventsDisabled) return;
            FMsDGV.GetSelectedFM().Title = EditFMTitleTextBox.Text;
            RefreshSelectedFM(rowOnly: true);
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
            RefreshSelectedFM(rowOnly: true);
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

            RefreshSelectedFM(rowOnly: true);
            Ini.WriteFullFMDataIni();
        }

        private void EditFMReleaseDateDateTimePicker_ValueChanged(object sender, EventArgs e)
        {
            if (EventsDisabled) return;
            FMsDGV.GetSelectedFM().ReleaseDate.DateTime = EditFMReleaseDateDateTimePicker.Value;
            RefreshSelectedFM(rowOnly: true);
            Ini.WriteFullFMDataIni();
        }

        private void EditFMLastPlayedCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (EventsDisabled) return;
            EditFMLastPlayedDateTimePicker.Visible = EditFMLastPlayedCheckBox.Checked;

            FMsDGV.GetSelectedFM().LastPlayed.DateTime = EditFMLastPlayedCheckBox.Checked
                ? EditFMLastPlayedDateTimePicker.Value
                : (DateTime?)null;

            RefreshSelectedFM(rowOnly: true);
            Ini.WriteFullFMDataIni();
        }

        private void EditFMLastPlayedDateTimePicker_ValueChanged(object sender, EventArgs e)
        {
            if (EventsDisabled) return;
            FMsDGV.GetSelectedFM().LastPlayed.DateTime = EditFMLastPlayedDateTimePicker.Value;
            RefreshSelectedFM(rowOnly: true);
            Ini.WriteFullFMDataIni();
        }

        private void EditFMDisabledModsTextBox_TextChanged(object sender, EventArgs e)
        {
            if (EventsDisabled) return;
            FMsDGV.GetSelectedFM().DisabledMods = EditFMDisabledModsTextBox.Text;
            RefreshSelectedFM(rowOnly: true);
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
            RefreshSelectedFM(rowOnly: true);
            Ini.WriteFullFMDataIni();
        }

        private void EditFMRatingComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (EventsDisabled) return;
            int rating = EditFMRatingComboBox.SelectedIndex - 1;
            FMsDGV.GetSelectedFM().Rating = rating;
            FMsDGV_FM_LLMenu.SetRatingMenuItemChecked(rating);
            RefreshSelectedFM(rowOnly: true);
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
            ShowMenu(FMsDGV_FM_LLMenu.GetFinishedOnMenu(this), EditFMFinishedOnButton, MenuPos.BottomRight, unstickMenu: true);
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

            RefreshSelectedFM(rowOnly: true);
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
                    // We can't do a switch expression on the second one, so keep them both the same for consistency
                    // ReSharper disable once ConvertConditionalTernaryExpressionToSwitchExpression
                    box.SelectedIndex =
                        box.SelectedIndex == -1 ? box.Items.Count - 1 :
                        box.SelectedIndex == 0 ? -1 :
                        box.SelectedIndex - 1;
                    // We need this call to make the thing scroll...
                    if (box.SelectedIndex > -1) box.EnsureVisible(box.SelectedIndex);
                    e.Handled = true;
                    break;
                case Keys.Down when box.Items.Count > 0:
                    box.SelectedIndex =
                        box.SelectedIndex == -1 ? 0 :
                        box.SelectedIndex == box.Items.Count - 1 ? -1 :
                        box.SelectedIndex + 1;
                    if (box.SelectedIndex > -1) box.EnsureVisible(box.SelectedIndex);
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
            if (!FMsDGV.RowSelected() || TagsTreeView.SelectedNode == null) return;

            var fm = FMsDGV.GetSelectedFM();

            string catText, tagText;
            bool isCategory;
            if (TagsTreeView.SelectedNode.Parent != null)
            {
                isCategory = false;
                catText = TagsTreeView.SelectedNode.Parent.Text;
                tagText = TagsTreeView.SelectedNode.Text;
            }
            else
            {
                isCategory = true;
                catText = TagsTreeView.SelectedNode.Text;
                tagText = "";
            }

            bool success = FMTags.RemoveTagFromFM(fm, catText, tagText, isCategory);
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
                Ini.WriteFullFMDataIni();
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
            foreach (CatAndTags catAndTag in GlobalTags)
            {
                if (catAndTag.Tags.Count == 0)
                {
                    var catItem = new ToolStripMenuItemWithBackingText(catAndTag.Category + ":") { Tag = LazyLoaded.True };
                    catItem.Click += AddTagMenuEmptyItem_Click;
                    addTagMenuItems.Add(catItem);
                }
                else
                {
                    var catItem = new ToolStripMenuItemWithBackingText(catAndTag.Category) { Tag = LazyLoaded.True };
                    addTagMenuItems.Add(catItem);

                    var last = addTagMenuItems[addTagMenuItems.Count - 1];

                    if (catAndTag.Category != "misc")
                    {
                        var customItem = new ToolStripMenuItemWithBackingText(LText.TagsTab.CustomTagInCategory) { Tag = LazyLoaded.True };
                        customItem.Click += AddTagMenuCustomItem_Click;
                        ((ToolStripMenuItemWithBackingText)last).DropDownItems.Add(customItem);
                        ((ToolStripMenuItemWithBackingText)last).DropDownItems.Add(new ToolStripSeparator { Tag = LazyLoaded.True });
                    }

                    foreach (string tag in catAndTag.Tags)
                    {
                        var tagItem = new ToolStripMenuItemWithBackingText(tag) { Tag = LazyLoaded.True };

                        if (catAndTag.Category == "misc")
                        {
                            tagItem.Click += AddTagMenuMiscItem_Click;
                        }
                        else
                        {
                            tagItem.Click += AddTagMenuItem_Click;
                        }

                        ((ToolStripMenuItemWithBackingText)last).DropDownItems.Add(tagItem);
                    }
                }
            }

            AddTagLLMenu.Menu.Items.AddRange(addTagMenuItems.ToArray());

            // TODO: @DarkMode: Make this less dumb
            // Like maybe override add methods of DarkContextMenu to just always re-setup the theme afterward
            // Special case because we add items dynamically
            AddTagLLMenu.Menu.RefreshDarkModeState();

            ShowMenu(AddTagLLMenu.Menu, AddTagFromListButton, MenuPos.LeftDown);
        }

        private void AddTagMenuItem_Click(object sender, EventArgs e)
        {
            var item = (ToolStripMenuItemWithBackingText)sender;
            if (item.HasDropDownItems) return;

            var cat = (ToolStripMenuItemWithBackingText?)item.OwnerItem;
            if (cat == null) return;

            AddTagOperation(FMsDGV.GetSelectedFM(), cat.BackingText + ": " + item.BackingText);
        }

        private void AddTagMenuCustomItem_Click(object sender, EventArgs e)
        {
            var item = (ToolStripMenuItemWithBackingText)sender;

            var cat = (ToolStripMenuItemWithBackingText?)item.OwnerItem;
            if (cat == null) return;

            AddTagTextBox.SetTextAndMoveCursorToEnd(cat.BackingText + ": ");
        }

        private void AddTagMenuMiscItem_Click(object sender, EventArgs e) => AddTagTextBox.SetTextAndMoveCursorToEnd(((ToolStripMenuItemWithBackingText)sender).BackingText);

        private void AddTagMenuEmptyItem_Click(object sender, EventArgs e) => AddTagTextBox.SetTextAndMoveCursorToEnd(((ToolStripMenuItemWithBackingText)sender).BackingText + " ");

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

            bool success = Core.RemoveDML(FMsDGV.GetSelectedFM(), lb.SelectedItem);
            if (!success) return;

            lb.RemoveAndSelectNearest();
        }

        private void PatchAddDMLButton_Click(object sender, EventArgs e)
        {
            var dmlFiles = new List<string>();

            using (var d = new OpenFileDialog())
            {
                d.Multiselect = true;
                d.Filter = LText.BrowseDialogs.DMLFiles + "|*.dml";
                if (d.ShowDialog() != DialogResult.OK || d.FileNames.Length == 0) return;
                dmlFiles.AddRange(d.FileNames);
            }
            PatchDMLsListBox.BeginUpdate();
            foreach (string f in dmlFiles)
            {
                if (f.IsEmpty()) continue;

                bool success = Core.AddDML(FMsDGV.GetSelectedFM(), f);
                if (!success) return;

                string dmlFileName = Path.GetFileName(f);
                if (!PatchDMLsListBox.ItemsAsStrings.ContainsI(dmlFileName))
                {
                    PatchDMLsListBox.Items.Add(dmlFileName);
                }
            }
            PatchDMLsListBox.EndUpdate();
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
            var s = (ToolStripMenuItemCustom)sender;

            // NULL_TODO: Null so I can assert
            TabPage? tab = null;
            for (int i = 0; i < TopRightTabsData.Count; i++)
            {
                if (s == (ToolStripMenuItemCustom)TopRightLLMenu.Menu.Items[i])
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

        #region FMs list

        public SelectedFM? GetSelectedFMPosInfo() => FMsDGV.RowSelected() ? FMsDGV.GetSelectedFMPosInfo() : null;

        public void SetRowCount(int count) => FMsDGV.RowCount = count;

        public void ShowFMsListZoomButtons(bool visible)
        {
            Lazy_FMsListZoomButtons.SetVisible(this, visible);
            SetFilterBarWidth();
        }

        private void ZoomFMsDGV(ZoomFMsDGVType type, float? zoomFontSize = null)
        {
            try
            {
                this.SuspendDrawing();

                // No goal escapes me, mate

                SelectedFM? selFM = FMsDGV.RowSelected() ? FMsDGV.GetSelectedFMPosInfo() : null;

                Font f = FMsDGV.DefaultCellStyle.Font;

                // Set zoom level
                float fontSize =
                    (type == ZoomFMsDGVType.ZoomIn ? f.SizeInPoints + 1.0f :
                    type == ZoomFMsDGVType.ZoomOut ? f.SizeInPoints - 1.0f :
                    type == ZoomFMsDGVType.ZoomTo && zoomFontSize != null ? (float)zoomFontSize :
                    type == ZoomFMsDGVType.ZoomToHeightOnly && zoomFontSize != null ? (float)zoomFontSize :
                    _fmsListDefaultFontSizeInPoints).ClampToFMsDGVFontSizeMinMax();

                // Set new font size
                Font newF = new Font(f.FontFamily, fontSize, f.Style, f.Unit, f.GdiCharSet, f.GdiVerticalFont);

                // Set row height based on font plus some padding
                int rowHeight = type == ZoomFMsDGVType.ResetZoom ? _fmsListDefaultRowHeight : newF.Height + 9;

                // If we're on startup, then the widths will already have been restored (to zoomed size) from the
                // config
                bool heightOnly = type == ZoomFMsDGVType.ZoomToHeightOnly;

                // Must be done first, else we get wrong values
                var widthMul = new List<double>();
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
                                c.Width = reset && Math.Abs(Config.FMsListFontSizeInPoints - _fmsListDefaultFontSizeInPoints) < 0.1
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
            finally
            {
                this.ResumeDrawing();
            }
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

        #region FMs list sorting

        public Column GetCurrentSortedColumnIndex() => FMsDGV.CurrentSortedColumn;

        public SortOrder GetCurrentSortDirection() => FMsDGV.CurrentSortDirection;

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
                    await DisplaySelectedFM();
                }
            }
        }

        #endregion

        #region FMsDGV event handlers

        private void FMsDGV_CellValueNeeded(object sender, DataGridViewCellValueEventArgs e)
        {
            if (CellValueNeededDisabled) return;

            if (FMsDGV.FilterShownIndexList.Count == 0) return;

            var fm = FMsDGV.GetFMFromIndex(e.RowIndex);

            // PERF: ~0.14ms per FM for en-US Long Date format
            // PERF_TODO: Test with custom - dt.ToString() might be slow?
            static string FormatDate(DateTime dt) => Config.DateFormat switch
            {
                DateFormat.CurrentCultureShort => dt.ToShortDateString(),
                DateFormat.CurrentCultureLong => dt.ToLongDateString(),
                _ => dt.ToString(Config.DateCustomFormatString)
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
                        GameIsKnownAndSupported(fm.Game) ? Images.GameIcons[(int)GameToGameIndex(fm.Game)] :
                        fm.Game == Game.Unsupported ? Images.RedQuestionMarkCircle :
                        // Can't say null, or else it sets an ugly red-x image
                        Images.Blank;
                    break;

                case Column.Installed:
                    e.Value = fm.Installed ? Images.GreenCheckCircle : Images.Blank;
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
                            e.Value = fm.Rating == -1 ? Images.Blank : Images.StarIcons![fm.Rating];
                        }
                        else
                        {
                            e.Value = fm.Rating == -1 ? "" : (fm.Rating / 2.0).ToString(CultureInfo.CurrentCulture);
                        }
                    }
                    break;

                case Column.Finished:
                    e.Value = fm.FinishedOnUnknown ? Images.FinishedOnUnknown : Images.FinishedOnIcons![fm.FinishedOn];
                    break;

                case Column.ReleaseDate:
                    e.Value = fm.ReleaseDate.DateTime != null ? FormatDate((DateTime)fm.ReleaseDate.DateTime) : "";
                    break;

                case Column.LastPlayed:
                    e.Value = fm.LastPlayed.DateTime != null ? FormatDate((DateTime)fm.LastPlayed.DateTime) : "";
                    break;

                case Column.DateAdded:
                    // IMPORTANT (Convert to local time): We don't do it earlier for startup perf reasons.
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
                    await DisplaySelectedFM();
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

                await DisplaySelectedFM();
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

            FMsDGV_FM_LLMenu.UpdateRatingList(fmSelStyle);

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
                    CellValueNeededDisabled = true;
                    try
                    {
                        FMsDGV.Columns.RemoveAt((int)Column.Rating);
                        FMsDGV.Columns.Insert((int)Column.Rating, newRatingColumn!);
                    }
                    finally
                    {
                        CellValueNeededDisabled = false;
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

                    Lazy_ToolStripLabels.Show(this, Lazy_ToolStripLabel.FilterByRating, from + " - " + to);
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
            IntPtr hWnd = Native.WindowFromPoint(Cursor.Position);
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

            SetReadmeButtonsBackColor(enabled, Config.VisualTheme);

            // In case the cursor is already over the readme when we do this
            // (cause it won't show automatically if it is)
            ShowReadmeControls(enabled && CursorOverReadmeArea());
        }

        private void SetReadmeButtonsBackColor(bool enabled, VisualTheme theme)
        {
            if (theme == VisualTheme.Dark) return;

            Color backColor = enabled ? SystemColors.Window : SystemColors.Control;
            ReadmeZoomInButton.BackColor = backColor;
            ReadmeZoomOutButton.BackColor = backColor;
            ReadmeResetZoomButton.BackColor = backColor;
            ReadmeFullScreenButton.BackColor = backColor;
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

        public void ChangeReadmeBoxFont(bool useFixed) => ReadmeRichTextBox.SetFontType(useFixed);

        #endregion

        #region Bottom bar

        #region Left side

        #region Install/Play buttons

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

        internal async void InstallUninstall_Play_Buttons_Click(object sender, EventArgs e)
        {
            if (sender.EqualsIfNotNull(InstallUninstallFMLLButton.Button))
            {
                await FMInstallAndPlay.InstallOrUninstall(FMsDGV.GetSelectedFM());
            }
            else if (sender == PlayFMButton)
            {
                await FMInstallAndPlay.InstallIfNeededAndPlay(FMsDGV.GetSelectedFM());
            }
        }

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
            var item = (ToolStripMenuItemCustom)sender;

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
                if (!noneSelected) scanOptions = f.ScanOptions;
            }

            if (noneSelected)
            {
                ControlUtils.ShowAlert(LText.ScanAllFMsBox.NothingWasScanned, LText.AlertMessages.Alert);
                return;
            }

            if (await FMScan.ScanFMs(FMsViewList, scanOptions!)) await SortAndSetFilter(forceDisplayFM: true);
        }

        private void WebSearchButton_Click(object sender, EventArgs e) => Core.OpenWebSearchUrl(FMsDGV.GetSelectedFM().Title);

        #endregion

        #region Right side

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

        public void ShowExitButton(bool enabled) => ExitLLButton.SetVisible(this, enabled);

        #endregion

        #endregion

        #region FM display

        // Perpetual TODO: Make sure this clears everything including the top right tab stuff
        private void ClearShownData()
        {
            if (FMsViewList.Count == 0) ScanAllFMsButton.Enabled = false;

            FMsDGV_FM_LLMenu.SetPlayFMInMPMenuItemVisible(false);
            FMsDGV_FM_LLMenu.SetPlayFMInMPMenuItemEnabled(false);
            FMsDGV_FM_LLMenu.SetPlayFMMenuItemEnabled(false);

            FMsDGV_FM_LLMenu.SetInstallUninstallMenuItemText(true);
            FMsDGV_FM_LLMenu.SetInstallUninstallMenuItemEnabled(false);

            FMsDGV_FM_LLMenu.SetDeleteFMMenuItemEnabled(false);

            FMsDGV_FM_LLMenu.SetOpenInDromEdMenuItemText(false);
            FMsDGV_FM_LLMenu.SetOpenInDromEdVisible(false);
            FMsDGV_FM_LLMenu.SetOpenInDromedEnabled(false);

            FMsDGV_FM_LLMenu.SetOpenFMFolderVisible(false);

            FMsDGV_FM_LLMenu.SetScanFMMenuItemEnabled(false);

            FMsDGV_FM_LLMenu.SetConvertAudioRCSubMenuEnabled(false);

            InstallUninstallFMLLButton.SetSayInstall(true);
            InstallUninstallFMLLButton.SetEnabled(false);

            PlayFMButton.Enabled = false;

            SetReadmeVisible(false);
            // Save memory
            ReadmeRichTextBox.SetText("");

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

                FMsDGV_FM_LLMenu.ClearFinishedOnMenuItemChecks();

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

        public void UpdateRatingMenus(int rating, bool disableEvents = false)
        {
            using (disableEvents ? new DisableEvents(this) : null)
            {
                FMsDGV_FM_LLMenu.SetRatingMenuItemChecked(rating);
                EditFMRatingComboBox.SelectedIndex = rating + 1;
            }
        }

        // @GENGAMES: Lots of game-specific code in here, but I don't see much to be done about it.
        private void UpdateAllFMUIDataExceptReadme(FanMission fm)
        {
            bool fmIsT3 = fm.Game == Game.Thief3;
            bool fmIsSS2 = fm.Game == Game.SS2;

            #region Toggles

            // We should never get here when FMsList.Count == 0, but hey
            if (FMsViewList.Count > 0) ScanAllFMsButton.Enabled = true;

            FMsDGV_FM_LLMenu.SetGameSpecificFinishedOnMenuItemsText(fm.Game);
            // FinishedOnUnknownMenuItem text stays the same

            bool gameIsSupported = GameIsKnownAndSupported(fm.Game);

            FMsDGV_FM_LLMenu.SetPlayFMMenuItemEnabled(gameIsSupported && !fm.MarkedUnavailable);

            FMsDGV_FM_LLMenu.SetPlayFMInMPMenuItemVisible(fm.Game == Game.Thief2 && Config.T2MPDetected);
            FMsDGV_FM_LLMenu.SetPlayFMInMPMenuItemEnabled(!fm.MarkedUnavailable);

            FMsDGV_FM_LLMenu.SetInstallUninstallMenuItemEnabled(gameIsSupported && !fm.MarkedUnavailable);
            FMsDGV_FM_LLMenu.SetInstallUninstallMenuItemText(!fm.Installed);

            FMsDGV_FM_LLMenu.SetDeleteFMMenuItemEnabled(!fm.MarkedUnavailable);

            FMsDGV_FM_LLMenu.SetOpenInDromEdMenuItemText(fmIsSS2);
            FMsDGV_FM_LLMenu.SetOpenInDromEdVisible(GameIsDark(fm.Game) && Config.GetGameEditorDetectedUnsafe(fm.Game));
            FMsDGV_FM_LLMenu.SetOpenInDromedEnabled(!fm.MarkedUnavailable);

            FMsDGV_FM_LLMenu.SetOpenFMFolderVisible(fm.Installed);

            FMsDGV_FM_LLMenu.SetScanFMMenuItemEnabled(!fm.MarkedUnavailable);

            FMsDGV_FM_LLMenu.SetConvertAudioRCSubMenuEnabled(GameIsDark(fm.Game) && fm.Installed && !fm.MarkedUnavailable);

            InstallUninstallFMLLButton.SetEnabled(gameIsSupported && !fm.MarkedUnavailable);
            InstallUninstallFMLLButton.SetSayInstall(!fm.Installed);

            PlayFMButton.Enabled = gameIsSupported && !fm.MarkedUnavailable;

            WebSearchButton.Enabled = true;

            StatsScanCustomResourcesButton.Enabled = !fm.MarkedUnavailable;

            foreach (Control c in EditFMTabPage.Controls)
            {
                if (c == EditFMLanguageLabel ||
                    c == EditFMLanguageComboBox)
                {
                    c.Enabled = !fmIsT3;
                }
                else
                {
                    c.Enabled = true;
                }
            }

            EditFMScanTitleButton.Enabled = !fm.MarkedUnavailable;
            EditFMScanAuthorButton.Enabled = !fm.MarkedUnavailable;
            EditFMScanReleaseDateButton.Enabled = !fm.MarkedUnavailable;
            EditFMScanLanguagesButton.Enabled = !fmIsT3 && !fm.MarkedUnavailable;
            EditFMScanForReadmesButton.Enabled = !fm.MarkedUnavailable;

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

            PatchDMLsPanel.Enabled = GameIsDark(fm.Game);

            #endregion

            #region FinishedOn

            FMsDGV_FM_LLMenu.SetFinishedOnMenuItemsChecked((Difficulty)fm.FinishedOn, fm.FinishedOnUnknown);

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

                UpdateRatingMenus(fm.Rating, disableEvents: false);

                ScanAndFillLanguagesBox(fm, disableEvents: false);

                CommentTextBox.Text = fm.Comment.FromRNEscapes();

                AddTagTextBox.Text = "";

                if (GameIsDark(fm.Game) && fm.Installed)
                {
                    PatchMainPanel.Show();
                    PatchFMNotInstalledLabel.Hide();
                    PatchDMLsListBox.BeginUpdate();
                    PatchDMLsListBox.Items.Clear();
                    (bool success, List<string> dmlFiles) = Core.GetDMLFiles(fm);
                    if (success)
                    {
                        foreach (string f in dmlFiles)
                        {
                            if (!f.IsEmpty()) PatchDMLsListBox.Items.Add(f);
                        }
                    }
                    PatchDMLsListBox.EndUpdate();
                }
            }

            DisplayFMTags(fm.Tags);

            #endregion
        }

        private async Task DisplaySelectedFM(bool refreshCache = false)
        {
            FanMission fm = FMsDGV.GetSelectedFM();

            if (fm.Game == Game.Null || (GameIsKnownAndSupported(fm.Game) && !fm.MarkedScanned))
            {
                using (new DisableKeyPresses(this))
                {
                    if (await FMScan.ScanFMs(new List<FanMission> { fm }, hideBoxIfZip: true))
                    {
                        RefreshSelectedFM(rowOnly: true);
                    }
                }
            }

            UpdateAllFMUIDataExceptReadme(fm);

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

                        ChooseReadmeLLPanel.ListBox.BeginUpdate();
                        ChooseReadmeLLPanel.ListBox.ClearFullItems();
                        foreach (string f in readmeFiles)
                        {
                            ChooseReadmeLLPanel.ListBox.AddFullItem(f, f.GetFileNameFast());
                        }
                        ChooseReadmeLLPanel.ListBox.EndUpdate();

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
                (string path, ReadmeType fileType) = Core.GetReadmeFileAndType(fm);
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
                var altTitlesMenuItems = new List<ToolStripItem>(fmAltTitles.Count);
                foreach (string altTitle in fmAltTitles)
                {
                    var item = new ToolStripMenuItemCustom(altTitle);
                    item.Click += EditFMAltTitlesMenuItems_Click;
                    altTitlesMenuItems.Add(item);
                }
                AltTitlesLLMenu.AddRange(altTitlesMenuItems);

                // TODO: @DarkMode: Make this less dumb
                // Like maybe override add methods of DarkContextMenu to just always re-setup the theme afterward
                // Special case because we add items dynamically
                AltTitlesLLMenu.Menu.RefreshDarkModeState();

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

        #region Control painting

        // Perf: Where feasible, it's way faster to simply draw images vector-style on-the-fly, rather than
        // pulling rasters from Resources, because Resources is a fat bloated hog with five headcrabs on it

        // Draw a nice separator between the bottom of the readme and the bottom bar. Every other side is already
        // visually separated enough.
        private void MainSplitContainer_Panel2_Paint(object sender, PaintEventArgs e)
        {
            SplitterPanel panel2 = MainSplitContainer.Panel2;

            if (MainSplitContainer.DarkModeEnabled)
            {
                e.Graphics.DrawLine(DarkColors.GreySelectionPen, panel2.Left, panel2.Height - 2, panel2.Right, panel2.Height - 2);
                e.Graphics.DrawLine(DarkColors.Fen_ControlBackgroundPen, panel2.Left, panel2.Height - 1, panel2.Right, panel2.Height - 1);
            }
            else
            {
                e.Graphics.DrawLine(SystemPens.ControlLight, panel2.Left, panel2.Height - 2, panel2.Right, panel2.Height - 2);
            }
        }

        private void BottomLeftButtonsFLP_Paint(object sender, PaintEventArgs e) => Images.PaintControlSeparators(e, 2, items: _bottomAreaSeparatedItems);

        private void FilterIconButtonsToolStrip_Paint(object sender, PaintEventArgs e) => Images.PaintToolStripSeparators(e, 5, _filtersToolStripSeparatedItems);

        private void RefreshAreaToolStrip_Paint(object sender, PaintEventArgs e)
        {
            // This one is a special case, so draw it explicitly here
            Pen s1Pen = Images.GetSeparatorPenForCurrentVisualStyleMode();
            const int y1 = 5;
            const int y2 = 20;

            Images.DrawSeparator(
                e: e,
                line1Pen: s1Pen,
                line1DistanceBackFromLoc: 3,
                line1Top: y1,
                line1Bottom: y2,
                x: RefreshFromDiskButton.Bounds.Location.X);

            Images.DrawSeparator(
                e: e,
                line1Pen: s1Pen,
                // Right side hack
                line1DistanceBackFromLoc: 0,
                line1Top: y1,
                line1Bottom: y2,
                // Right side hack
                x: ClearFiltersButton.Bounds.Right + 6);
        }

        private void FilterBarFLP_Paint(object sender, PaintEventArgs e) => Images.PaintControlSeparators(e, -1, 5, 20, _filterLabels);

        private void PlayFMButton_Paint(object sender, PaintEventArgs e) => Images.PaintPlayFMButton(PlayFMButton, e);

        private void PlayOriginalGameButton_Paint(object sender, PaintEventArgs e) => Images.PaintBitmapButton(
            PlayOriginalGameButton,
            e,
            PlayOriginalGameButton.Enabled ? Images.PlayOriginalGame : Images.PlayOriginalGame_Disabled,
            x: 10);

        [SuppressMessage("ReSharper", "MemberCanBeMadeStatic.Global")]
        internal void InstallUninstall_Play_Buttons_Paint(object sender, PaintEventArgs e)
        {
            DarkButton button = InstallUninstallFMLLButton.Button;
            bool enabled = button.Enabled;

            Images.PaintBitmapButton(
                button,
                e,
                InstallUninstallFMLLButton.SayInstallState
                    ? enabled ? Images.Install_24 : Images.Install_24_Disabled
                    : enabled ? Images.Uninstall_24 : Images.Uninstall_24_Disabled,
                10);
        }

        private void SettingsButton_Paint(object sender, PaintEventArgs e) => Images.PaintBitmapButton(
            SettingsButton,
            e,
            SettingsButton.Enabled ? Images.Settings : Images.Settings_Disabled,
            x: 10);

        private void PatchAddDMLButton_Paint(object sender, PaintEventArgs e) => Images.PaintPlusButton(PatchAddDMLButton, e);

        private void PatchRemoveDMLButton_Paint(object sender, PaintEventArgs e) => Images.PaintMinusButton(PatchRemoveDMLButton, e);

        private void TopRightMenuButton_Paint(object sender, PaintEventArgs e) => Images.PaintHamburgerMenuButton_TopRight(TopRightMenuButton, e);

        private void MainMenuButton_Paint(object sender, PaintEventArgs e) => Images.PaintHamburgerMenuButton24(MainMenuButton, e);

        private void WebSearchButton_Paint(object sender, PaintEventArgs e) => Images.PaintWebSearchButton(WebSearchButton, e);

        private void ReadmeFullScreenButton_Paint(object sender, PaintEventArgs e) => Images.PaintReadmeFullScreenButton(ReadmeFullScreenButton, e);

        private void ResetLayoutButton_Paint(object sender, PaintEventArgs e) => Images.PaintResetLayoutButton(ResetLayoutButton, e);

        private void ScanAllFMsButton_Paint(object sender, PaintEventArgs e) => Images.PaintScanAllFMsButton(ScanAllFMsButton, e);

        // Keep this one static because it calls out to the internal ButtonPainter rather than external Core, so
        // it's fine even if we modularize the view
        // TODO: MainForm static event handler that calls out to the static control painter: Is this really fine?
        private
#if !DEBUG
        static
#endif
        void ScanIconButtons_Paint(object sender, PaintEventArgs e) => Images.PaintScanSmallButtons((Button)sender, e);

        private
#if !DEBUG
        static
#endif
        void ZoomInButtons_Paint(object sender, PaintEventArgs e) => Images.PaintZoomButtons((Button)sender, e, Zoom.In);

        private
#if !DEBUG
        static
#endif
        void ZoomOutButtons_Paint(object sender, PaintEventArgs e) => Images.PaintZoomButtons((Button)sender, e, Zoom.Out);

        private
#if !DEBUG
        static
#endif
        void ZoomResetButtons_Paint(object sender, PaintEventArgs e) => Images.PaintZoomButtons((Button)sender, e, Zoom.Reset);

        #endregion
    }
}
