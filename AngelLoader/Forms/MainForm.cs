/* NOTE: MainForm notes:
NOTE: Don't lazy load the filter bar scroll buttons, as they screw the whole thing up (FMsDGV doesn't anchor
in its panel correctly, etc.). If we figure out how to solve this later, we can lazy load them then.

Things to lazy load:
-Top-right section in its entirety, and then individual tab pages (in case some are hidden), and then individual
 controls on each tab page (in case the tabs are visible but not selected on startup)
-Game buttons and game tabs (one or the other will be invisible on startup)

@NET5: Fonts will change and control sizes will all change too.
-.NET 6 seems to have an option to set the font to the old MS Sans Serif 8.25pt app-wide.
-If converting the whole app to Segoe UI, remember to change all MinimumSize (button min size "75,23" etc.)
IMPORTANT: Remember to change font-size-dependent DGV zoom feature to work correctly with the new font!

NOTE(MainForm Designer): The controls move positions because they're accounting for the scroll bar
but then when the scroll bar isn't there at runtime, their positions are wrong (too much margin on whatever side
the scroll bar was).

@X64: IntPtr will be 64-bit, so search for all places where we deal with them and make sure they all still work

@MULTISEL: Test game tabs mode.

@MULTISEL: When switching game tabs, multi-selections are not saved. Do we want this behavior or no?
This is part of the decision of "how temporary" do we want multi-selections to be.

@MULTISEL(Theory about the Stupid Hack for selection point syncing / @SEL_SYNC_HACK):
Is it because we go like Row 5 -> select, Row 0 -> deselect, but "deselect" is counted as a selection "thing",
so that the last point where a selection "thing" happened is counted as the point from where a new keyboard
selection will start? So we're selected on only Row 5 but "last selection thing" is Row 0 so we start from Row 0?
If that's the case, we could remove the stupid hack from everywhere and just change the selection in a way that
makes DGV's idea of things match the ACTUAL blatantly displayed user-facing idea.
@MULTISEL(@SEL_SYNC_HACK enlightenment):
There's the concept of "current row" (CurrentRow) and "current cell" (CurrentCell). CurrentRow is read-only (of
bloody course), but it can be set in a roundabout way by setting CurrentCell to a cell in the current row. BUT,
setting this causes the row to scroll into view (as documented) and possibly also some other goddamn visual garbage,
visible selection change/flickering etc.
It's conceptually much cleaner to use this method, but we would then have to hack around this infuriating unwanted
behavior that comes part and parcel with what should be a simple #$@!ing property flip.
Our current hack is nasty, but it does do what we want, is performant enough, and looks good to the user.
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
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using AngelLoader.DataClasses;
using AngelLoader.Forms.CustomControls;
using AngelLoader.Forms.CustomControls.LazyLoaded;
using AngelLoader.Forms.WinFormsNative;
using static AL_Common.Common;
using static AngelLoader.GameSupport;
using static AngelLoader.Misc;

namespace AngelLoader.Forms
{
    public sealed partial class MainForm : DarkFormBase, IView
    {
        #region Private fields

        /// <summary>
        /// Any control that might need to know this can check it.
        /// </summary>
        internal bool AboutToClose;

        #region Window size/location

        private FormWindowState _nominalWindowState;
        private Size _nominalWindowSize;
        private Point _nominalWindowLocation;

        #endregion

        #region FMs list

        private float _fmsListDefaultFontSizeInPoints;
        private int _fmsListDefaultRowHeight;

        // Set these beforehand and don't set autosize on any column! Or else it explodes everything because
        // FMsDGV tries to refresh when it shouldn't and all kinds of crap. Phew.
        private const int _ratingImageColumnWidth = 73;
        private const int _finishedColumnWidth = 91;

        #endregion

        #region Control arrays

        private readonly TabPage[] _gameTabsInOrder;
        private readonly ToolStripButtonCustom[] _filterByGameButtonsInOrder;
        private readonly TabPage[] _topRightTabsInOrder;

        private readonly Control[] _filterLabels;
        private readonly ToolStripItem[] _filtersToolStripSeparatedItems;
        private readonly Control[] _bottomAreaSeparatedItems;

        private readonly Component[][] _hideableFilterControls;

        private readonly DarkButton[] _readmeControlButtons;

        #endregion

        #region Enums

        private enum KeepSel { False, True, TrueNearest }

        private enum ZoomFMsDGVType
        {
            ZoomIn,
            ZoomOut,
            ResetZoom,
            ZoomTo,
            ZoomToHeightOnly
        }

        #endregion

        #region Disablers

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool EventsDisabled { get; set; }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool KeyPressesDisabled { get; set; }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool ZeroSelectCodeDisabled { get; set; }

        // Needed for Rating column swap to prevent a possible exception when CellValueNeeded is called in the
        // middle of the operation
        internal bool CellValueNeededDisabled;

        private TransparentPanel? ViewBlockingPanel;
        internal bool ViewBlocked { get; private set; }

        #endregion

        #region Lazy-loaded controls

        private readonly AddTagLLDropDown AddTagLLDropDown;
        private readonly AddTagLLMenu AddTagLLMenu;
        private readonly AltTitlesLLMenu AltTitlesLLMenu;
        private readonly ChooseReadmeLLPanel ChooseReadmeLLPanel;
        private readonly EncodingsLLMenu EncodingsLLMenu;
        private readonly ExitLLButton ExitLLButton;
        private readonly FilterControlsLLMenu FilterControlsLLMenu;
        private readonly FMsDGV_ColumnHeaderLLMenu FMsDGV_ColumnHeaderLLMenu;
        private readonly FMsDGV_FM_LLMenu FMsDGV_FM_LLMenu;
        private readonly GameFilterControlsLLMenu GameFilterControlsLLMenu;
        private readonly InstallUninstallFMLLButton InstallUninstallFMLLButton;
        private readonly Lazy_FMsListZoomButtons Lazy_FMsListZoomButtons;
        private readonly Lazy_PlayOriginalControls Lazy_PlayOriginalControls;
        private readonly Lazy_ToolStripLabels Lazy_ToolStripLabels;
        private readonly MainLLMenu MainLLMenu;
        private readonly PlayOriginalGameLLMenu PlayOriginalGameLLMenu;
        private readonly PlayOriginalT2InMultiplayerLLMenu PlayOriginalT2InMultiplayerLLMenu;
        private readonly TopRightLLMenu TopRightLLMenu;
        private readonly ViewHTMLReadmeLLButton ViewHTMLReadmeLLButton;

        #endregion

        #endregion

        #region Non-public-release methods

#if !ReleaseBeta && !ReleasePublic
        private void ForceWindowedCheckBox_CheckedChanged(object sender, EventArgs e) => Config.ForceWindowed = ForceWindowedCheckBox.Checked;

        private void T1ScreenShotModeCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (EventsDisabled) return;
            GameConfigFiles.SetScreenShotMode(GameIndex.Thief1, T1ScreenShotModeCheckBox.Checked);
        }

        private void T2ScreenShotModeCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (EventsDisabled) return;
            GameConfigFiles.SetScreenShotMode(GameIndex.Thief2, T2ScreenShotModeCheckBox.Checked);
        }

        public void UpdateGameScreenShotModes()
        {
            using (new DisableEvents(this))
            {
                bool? t1 = GameConfigFiles.GetScreenShotMode(GameIndex.Thief1);
                bool? t2 = GameConfigFiles.GetScreenShotMode(GameIndex.Thief2);

                T1ScreenShotModeCheckBox.Visible = t1 != null;
                T2ScreenShotModeCheckBox.Visible = t2 != null;

                if (t1 != null) T1ScreenShotModeCheckBox.Checked = (bool)t1;
                if (t2 != null) T2ScreenShotModeCheckBox.Checked = (bool)t2;
            }
        }
#endif

        #endregion

        #region Test / debug

#if DEBUG || (Release_Testing && !RT_StartupOnly)

        private void TestButton_Click(object sender, EventArgs e)
        {
            Config.VisualTheme = Config.DarkMode ? VisualTheme.Classic : VisualTheme.Dark;
            SetTheme(Config.VisualTheme);
        }

        private void Test2Button_Click(object sender, EventArgs e)
        {
            //return;

            //Width = 1305;
            //Height = 750;

            Width = 1458;
            Height = 872;
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
            if (m.Msg == Native.WM_THEMECHANGED)
            {
                Win32ThemeHooks.ReloadTheme();
            }
            base.WndProc(ref m);
        }

        public bool PreFilterMessage(ref Message m)
        {
            const bool BlockMessage = true;
            const bool PassMessageOn = false;

            static bool TryGetHWndFromMousePos(Message msg, out IntPtr result)
            {
                Point pos = new Point(Native.SignedLOWORD(msg.LParam), Native.SignedHIWORD(msg.LParam));
                result = Native.WindowFromPoint(pos);
                return result != IntPtr.Zero && Control.FromHandle(result) != null;
            }

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

                if (ViewBlocked || CursorOutsideAddTagsDropDownArea()) return BlockMessage;

                int delta = Native.SignedHIWORD(m.WParam);
                if (CanFocus && CursorOverControl(FilterBarFLP) && !CursorOverControl(FMsDGV))
                {
                    // Allow the filter bar to be mousewheel-scrolled with the buttons properly appearing and
                    // disappearing as appropriate
                    if (delta != 0)
                    {
                        int direction = delta > 0 ? Native.SB_LINELEFT : Native.SB_LINERIGHT;
                        int origSmallChange = FilterBarFLP.HorizontalScroll.SmallChange;

                        FilterBarFLP.HorizontalScroll.SmallChange = 45;

                        Native.SendMessage(FilterBarFLP.Handle, Native.WM_SCROLL, (IntPtr)direction, IntPtr.Zero);

                        FilterBarFLP.HorizontalScroll.SmallChange = origSmallChange;
                    }
                }
                else if (CanFocus && CursorOverControl(FMsDGV) && Native.LOWORD(m.WParam) == Native.MK_CONTROL)
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

                if (ViewBlocked) return BlockMessage;

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
            // NC = Non-Client, ie. the mouse was in a non-client area of the control
            else if (m.Msg is Native.WM_MOUSEMOVE or Native.WM_NCMOUSEMOVE)
            {
                if (!CanFocus) return PassMessageOn;

                if (CursorOutsideAddTagsDropDownArea() || ViewBlocked) return BlockMessage;

                ShowReadmeControls(CursorOverReadmeArea());
            }
            else if (m.Msg is
                Native.WM_LBUTTONDOWN or Native.WM_NCLBUTTONDOWN or
                Native.WM_MBUTTONDOWN or Native.WM_NCMBUTTONDOWN or
                Native.WM_RBUTTONDOWN or Native.WM_NCRBUTTONDOWN or
                Native.WM_LBUTTONDBLCLK or Native.WM_NCLBUTTONDBLCLK or
                Native.WM_MBUTTONDBLCLK or Native.WM_NCMBUTTONDBLCLK or
                Native.WM_RBUTTONDBLCLK or Native.WM_NCRBUTTONDBLCLK or
                Native.WM_LBUTTONUP or Native.WM_NCLBUTTONUP or
                Native.WM_MBUTTONUP or Native.WM_NCMBUTTONUP or
                Native.WM_RBUTTONUP or Native.WM_NCRBUTTONUP)
            {
                if (!CanFocus) return PassMessageOn;

                if (ViewBlocked &&
                    // Fix multi-select after view-blocking scan
                    // (so it doesn't throw out the mouseup and leave us selecting lines with mouse not down)
                    (m.Msg is
                        Native.WM_LBUTTONDOWN or Native.WM_NCLBUTTONDOWN or
                        Native.WM_MBUTTONDOWN or Native.WM_NCMBUTTONDOWN or
                        Native.WM_RBUTTONDOWN or Native.WM_NCRBUTTONDOWN))
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
                    if (FMsDGV.RowSelected() && !FMsDGV.MainSelectedRow!.Displayed)
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
                if (KeyPressesDisabled || ViewBlocked) return BlockMessage;

                int wParam = (int)m.WParam;
                if (wParam == (int)Keys.F1 && CanFocus)
                {
                    static bool AnyControlFocusedIn(Control control, int stackCounter = 0)
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

                    bool AnyControlFocusedInTabPage(TabPage tabPage) =>
                        (TopRightTabControl.Focused && TopRightTabControl.SelectedTab == tabPage) ||
                        AnyControlFocusedIn(tabPage);

                    bool mainMenuWasOpen = MainLLMenu.Visible;

                    string section =
                        !EverythingPanel.Enabled ? HelpSections.MainWindow :
                        mainMenuWasOpen ? HelpSections.MainMenu :
                        FMsDGV_FM_LLMenu.Visible ? HelpSections.FMContextMenu :
                        FMsDGV_ColumnHeaderLLMenu.Visible ? HelpSections.ColumnHeaderContextMenu :
                        AnyControlFocusedIn(TopSplitContainer.Panel1) ? HelpSections.MissionList :
                        TopRightMenuButton.Focused || TopRightLLMenu.Focused || AnyControlFocusedInTabPage(StatisticsTabPage) ? HelpSections.StatsTab :
                        AnyControlFocusedInTabPage(EditFMTabPage) ? HelpSections.EditFMTab :
                        AnyControlFocusedInTabPage(CommentTabPage) ? HelpSections.CommentTab :
                        // Add tag dropdown is in EverythingPanel, not tags tab page
                        AnyControlFocusedInTabPage(TagsTabPage) || AddTagLLDropDown.Focused ? HelpSections.TagsTab :
                        AnyControlFocusedInTabPage(PatchTabPage) ? HelpSections.PatchTab :
                        AnyControlFocusedInTabPage(ModsTabPage) ? HelpSections.ModsTab :
                        AnyControlFocusedIn(MainSplitContainer.Panel2) ? HelpSections.ReadmeArea :
                        HelpSections.MainWindow;

                    Core.OpenHelpFile(section);

                    // Otherwise, F1 activates the menu item marked F1, but we want to handle it manually
                    if (mainMenuWasOpen) return BlockMessage;
                }
            }
            else if (m.Msg == Native.WM_KEYUP)
            {
                if (KeyPressesDisabled || ViewBlocked) return BlockMessage;
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
            /*
            Font loading speed:
            We can't try to be clever and set the form's font to a loaded-from-disk one in order to make it never
            load the built-in default one (super slow if you have a ton of fonts installed like I do), because
            the Font property setter checks the default font anyway, meaning it loads the damn thing anyway so
            it's pointless.
            */
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

            Win32ThemeHooks.InstallHooks();

            #region Manual control construct + init

            #region Lazy-loaded controls

            AddTagLLDropDown = new AddTagLLDropDown(this);
            AddTagLLMenu = new AddTagLLMenu(this);
            AltTitlesLLMenu = new AltTitlesLLMenu(this);
            ChooseReadmeLLPanel = new ChooseReadmeLLPanel(this);
            EncodingsLLMenu = new EncodingsLLMenu(this);
            ExitLLButton = new ExitLLButton(this);
            FilterControlsLLMenu = new FilterControlsLLMenu(this);
            FMsDGV_ColumnHeaderLLMenu = new FMsDGV_ColumnHeaderLLMenu(this);
            FMsDGV_FM_LLMenu = new FMsDGV_FM_LLMenu(this);
            GameFilterControlsLLMenu = new GameFilterControlsLLMenu(this);
            InstallUninstallFMLLButton = new InstallUninstallFMLLButton(this);
            Lazy_FMsListZoomButtons = new Lazy_FMsListZoomButtons(this);
            Lazy_PlayOriginalControls = new Lazy_PlayOriginalControls(this);
            Lazy_ToolStripLabels = new Lazy_ToolStripLabels(this);
            MainLLMenu = new MainLLMenu(this);
            PlayOriginalGameLLMenu = new PlayOriginalGameLLMenu(this);
            PlayOriginalT2InMultiplayerLLMenu = new PlayOriginalT2InMultiplayerLLMenu(this);
            TopRightLLMenu = new TopRightLLMenu(this);
            ViewHTMLReadmeLLButton = new ViewHTMLReadmeLLButton(this);

            #endregion

            // The other Rating column, there has to be two, one for text and one for images
            RatingImageColumn = new DataGridViewImageColumn
            {
                // IMPORTANT: Set this explicitly, otherwise we can end up with the following situation:
                // -We start up, rating column is set to text so this one hasn't been added yet, then we change
                //  to image rating column. This gets added and has its header cell replaced with a custom one,
                //  and does NOT have its text transferred over. It ends up with blank text.
                //  NOTE! The text column avoids this issue solely because it gets added in the component init
                //  method (therefore the OnColumnAdded() handler is run and it gets its header cell replaced
                //  immediately). If we changed that, we would have to add this to the rating text column too!
                HeaderCell = new DataGridViewColumnHeaderCellCustom(),
                ImageLayout = DataGridViewImageCellLayout.Zoom,
                ReadOnly = true,
                Width = _ratingImageColumnWidth,
                Resizable = DataGridViewTriState.False,
                SortMode = DataGridViewColumnSortMode.Programmatic
            };

            TopRightMultiSelectBlockerPanel = new DrawnPanel
            {
                Visible = false,
                Location = new Point(0, 0),
                Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom,
                Size = new Size(533, 310),
                DarkModeDrawnBackColor = DarkColors.Fen_ControlBackground
            };
            TopSplitContainer.Panel2.Controls.Add(TopRightMultiSelectBlockerPanel);
            TopRightMultiSelectBlockerPanel.BringToFront();

            TopRightMultiSelectBlockerLabel = new DarkLabel
            {
                AutoSize = false,
                DarkModeBackColor = DarkColors.Fen_ControlBackground,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter
            };
            TopRightMultiSelectBlockerPanel.Controls.Add(TopRightMultiSelectBlockerLabel);

            #endregion

            #region Construct + init non-public-release controls

#if DEBUG || (Release_Testing && !RT_StartupOnly)
            #region Construct + init debug-only controls

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
            _gameTabsInOrder = new TabPage[]
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

            _topRightTabsInOrder = new TabPage[]
            {
                StatisticsTabPage,
                EditFMTabPage,
                CommentTabPage,
                TagsTabPage,
                PatchTabPage,
                ModsTabPage
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
                new Component[] { FilterShowUnavailableButton },
                new Component[] { FilterShowRecentAtTopButton }
            };

            _readmeControlButtons = new[]
            {
                ReadmeEncodingButton,
                ReadmeZoomInButton,
                ReadmeZoomOutButton,
                ReadmeResetZoomButton,
                ReadmeFullScreenButton
            };

            #endregion

            foreach (DarkButton button in _readmeControlButtons)
            {
                button.DarkModeBackColor = DarkColors.Fen_DarkBackground;
            }

            MainMenuButton.HideFocusRectangle();
        }

        // In early development, I had some problems with putting init stuff in the constructor, where all manner
        // of nasty random behavior would happen. Not sure if that was because of something specific I was doing
        // wrong or what, but I have this init method now that comfortably runs after the ctor. Shrug.
        // MT: On startup only, this is run in parallel with FindFMs.Find()
        // So don't touch anything the other touches: anything affecting preset tags or the FMs list.
        public void InitThreadable()
        {
#if RELEASE_BETA
            const string betaVer = "4";
            Text = "AngelLoader " + Application.ProductVersion + " beta " + betaVer;
#else
            Text = "AngelLoader " + Application.ProductVersion;
#endif

            #region Set up form and control state

            // Set here in init method so as to avoid the changes being visible.
            // Set here specifically (before anything else) so that splitter positioning etc. works right.
            SetWindowStateAndSize();

            #region Top-right tabs

            AssertR(_topRightTabsInOrder.Length == TopRightTabsData.Count, nameof(_topRightTabsInOrder) + " length is different than enum length");

            var sortedTabPages = new SortedDictionary<int, TabPage>();
            for (int i = 0; i < TopRightTabsData.Count; i++)
            {
                sortedTabPages.Add(Config.TopRightTabsData.Tabs[i].DisplayIndex, _topRightTabsInOrder[i]);
            }

            var topRightTabs = new List<TabPage>(sortedTabPages.Count);
            foreach (var item in sortedTabPages) topRightTabs.Add(item.Value);

            // This removes any existing tabs so it works even though we always add all tabs in component init now
            TopRightTabControl.SetTabsFull(topRightTabs);
            var gameTabs = new List<TabPage>(SupportedGameCount);
            foreach (TabPage item in _gameTabsInOrder) gameTabs.Add(item);
            GamesTabControl.SetTabsFull(gameTabs);

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

            FMsDGV.InjectOwner(this);

            _fmsListDefaultFontSizeInPoints = FMsDGV.DefaultCellStyle.Font.SizeInPoints;
            _fmsListDefaultRowHeight = FMsDGV.RowTemplate.Height;

            #region Columns

            FinishedColumn.Width = _finishedColumnWidth;

            UpdateRatingListsAndColumn(Config.RatingDisplayStyle == RatingDisplayStyle.FMSel, startup: true);

            FMsDGV.SetColumnData(FMsDGV_ColumnHeaderLLMenu, Config.Columns);

            #endregion

            #endregion

            #region Readme area

            ReadmeRichTextBox.InjectOwner(this);

            // Set both at once to avoid an elusive bug that happens when you start up, the readme is blank, then
            // you shut down without loading a readme, whereupon it will save out ZoomFactor which is still 1.0.
            // You can't just save out StoredZoomFactor either because it doesn't change when the user zooms, as
            // there's no event for that. Fun.
            ReadmeRichTextBox.SetAndStoreZoomFactor(Config.ReadmeZoomFactor);

            #endregion

            #region Filters

            GameFilterControlsLLMenu.SetCheckedStates(Config.GameFilterControlVisibilities);

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

            #region Pseudofilters

            FilterShowUnsupportedButton.Checked = Config.ShowUnsupported;
            FilterShowUnavailableButton.Checked = Config.ShowUnavailableFMs;
            FilterShowRecentAtTopButton.Checked = Config.ShowRecentAtTop;

            #endregion

            // EnsureValidity() guarantees selected tab will not be invisible
            for (int i = 0; i < TopRightTabsData.Count; i++)
            {
                if ((int)Config.TopRightTabsData.SelectedTab == i)
                {
                    TopRightTabControl.SelectedTab = _topRightTabsInOrder[i];
                    break;
                }
            }

            SetPlayOriginalGameControlsState();

            // This button is a weird special case (see its class) so we just construct it here and it will be
            // shown when localized.
            // TODO (inst/uninst button): We might be able to wrangle this into something cleaner nonetheless.
            if (!Config.HideUninstallButton) InstallUninstallFMLLButton.Construct();
            if (!Config.HideExitButton) ExitLLButton.SetVisible(true);

            TopSplitContainer.CollapsedSize = TopRightCollapseButton.Width;
            if (Config.TopRightPanelCollapsed)
            {
                TopSplitContainer.SetFullScreen(true, suspendResume: false);
                SetTopRightCollapsedState();
            }

            ModsCheckList.Inject(() => ModsShowUberCheckBox.Checked);

            #endregion

            // Set these here because they depend on the splitter positions
            Localize(startup: true);

            if (Math.Abs(Config.FMsListFontSizeInPoints - FMsDGV.DefaultCellStyle.Font.SizeInPoints) >= 0.001)
            {
                ZoomFMsDGV(ZoomFMsDGVType.ZoomToHeightOnly, Config.FMsListFontSizeInPoints);
            }

            ChangeGameOrganization(startup: true);

            // Do this here to prevent double-loading of RTF/GLML readmes
            SetTheme(Config.VisualTheme, startup: true, alsoCreateControlHandles: true);

#if !ReleaseBeta && !ReleasePublic
            UpdateGameScreenShotModes();
#endif
        }

        private ISplashScreen_Safe? _splashScreen;

        // This one can't be multithreaded because it depends on the FMs list
        public async Task FinishInitAndShow(List<int>? fmsViewListUnscanned, ISplashScreen_Safe splashScreen)
        {
            _splashScreen = splashScreen;

            if (Visible) return;

            // Sort the list here because InitThreadable() is run in parallel to FindFMs.Find() but sorting needs
            // Find() to have been run first.
            SortFMsDGV(Config.SortedColumn, Config.SortDirection);

            if (fmsViewListUnscanned?.Count > 0)
            {
                Show();
                await FMScan.ScanNewFMs(fmsViewListUnscanned);
            }

            Core.SetFilter();
            if (RefreshFMsList(FMsDGV.CurrentSelFM, startup: true, KeepSel.TrueNearest))
            {
                _displayedFM = await Core.DisplayFM();
            }

            if (!Visible) Show();

            // Must come after Show() I guess or it doesn't work?!
            FMsDGV.Focus();

#if !ReleasePublic
            //if (Config.CheckForUpdatesOnStartup) await CheckUpdates.Check();
#endif
        }

        private static FormWindowState WindowStateToFormWindowState(WindowState windowState) => windowState switch
        {
            Misc.WindowState.Normal => FormWindowState.Normal,
            Misc.WindowState.Minimized => FormWindowState.Minimized,
            _ => FormWindowState.Maximized
        };

        private static WindowState FormWindowStateToWindowState(FormWindowState formWindowState) => formWindowState switch
        {
            FormWindowState.Normal => Misc.WindowState.Normal,
            FormWindowState.Minimized => Misc.WindowState.Minimized,
            _ => Misc.WindowState.Maximized
        };

        private void SetWindowStateAndSize()
        {
            // Size MUST come first, otherwise it doesn't take (and then you have to put it in _Load, where it
            // can potentially be seen being changed)
            Size = Config.MainWindowSize;
            WindowState = WindowStateToFormWindowState(Config.MainWindowState);

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

            _nominalWindowState = WindowStateToFormWindowState(Config.MainWindowState);
            _nominalWindowSize = Config.MainWindowSize;
            _nominalWindowLocation = new Point(loc.X, loc.Y);
        }

        private new void Show()
        {
            base.Show();
            _splashScreen?.Hide();
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

        private void MainForm_Deactivate(object sender, EventArgs e)
        {
#if false
            SetReadmeControlZPosition(true);
#endif
            CancelResizables();
        }

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

#if false
        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);
            if ((ModifierKeys & Keys.Control) != Keys.Control)
            {
                SetReadmeControlZPosition(true);
            }
        }
#endif

        private async void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
#if false
            if ((ModifierKeys & Keys.Control) == Keys.Control)
            {
                SetReadmeControlZPosition(false);
            }
#endif

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

            // Let user use Home+End keys to navigate a filter textbox if it's focused, even if the mouse is over
            // the FMs list
            if ((FilterTitleTextBox.Focused || FilterAuthorTextBox.Focused) &&
                (e.KeyCode is Keys.Home or Keys.End))
            {
                return;
            }

            #region FMsDGV nav

            // NIGHTMARE REALM
            // This code is absurd. Boy howdy I hope I never have to touch it again.

            void DoSelectionSyncHack(int index, bool suspendResume = true)
            {
                try
                {
                    if (suspendResume) EverythingPanel.SuspendDrawing();

                    // @SEL_SYNC_HACK
                    FMsDGV.MultiSelect = false;
                    FMsDGV.SelectSingle(index, suppressSelectionChangedEvent: true);
                    FMsDGV.SelectProperly();
                    FMsDGV.MultiSelect = true;
                    FMsDGV.SelectSingle(index);
                }
                finally
                {
                    if (suspendResume) EverythingPanel.ResumeDrawing();
                }
            }

            void SelectAndSuppress(int index, bool singleSelect = false, bool selectionSyncHack = false)
            {
                bool fmsDifferent = GetMainSelectedFMOrNull() != _displayedFM;

                if (singleSelect)
                {
                    if (selectionSyncHack)
                    {
                        DoSelectionSyncHack(index, suspendResume: fmsDifferent);
                    }
                    else
                    {
                        FMsDGV.SelectSingle(index);
                    }
                }
                else
                {
                    FMsDGV.Rows[index].Selected = true;
                }
                FMsDGV.SelectProperly();
                e.SuppressKeyPress = true;
            }

            async Task HandleHomeOrEnd(bool home, bool selectionSyncHack = true)
            {
                if (!FMsDGV.RowSelected() || (!FMsDGV.Focused && !CursorOverControl(FMsDGV))) return;

                var edgeRow = FMsDGV.Rows[home ? 0 : FMsDGV.RowCount - 1];
                try
                {
                    FMsDGV.FirstDisplayedScrollingRowIndex = edgeRow.Index;
                }
                catch
                {
                    // no room is available to display rows
                }
                if (FMsDGV.MainSelectedRow == edgeRow)
                {
                    e.SuppressKeyPress = true;
                    if (!e.Shift)
                    {
                        using (new DisableEvents(this))
                        {
                            for (int i = 0; i < FMsDGV.RowCount; i++)
                            {
                                var row = FMsDGV.Rows[i];
                                if (row == edgeRow) continue;
                                FMsDGV.SetRowSelected(row.Index, selected: false, suppressEvent: true);
                            }
                        }
                        bool fmsDifferent = FMsDGV.GetMainSelectedFM() != _displayedFM;
                        if (fmsDifferent)
                        {
                            _displayedFM = await Core.DisplayFM();
                        }
                        SetTopRightBlockerVisible();

                        if (selectionSyncHack)
                        {
                            using (new DisableEvents(this))
                            {
                                DoSelectionSyncHack(edgeRow.Index, suspendResume: fmsDifferent);
                            }
                        }
                    }
                }
                else
                {
                    using (new DisableEvents(this))
                    {
                        if (e.Shift)
                        {
                            int mainSelectedRowIndex = FMsDGV.MainSelectedRow!.Index;
                            // We can set CurrentCell here because we've already scrolled the row into view, so
                            // the fact that setting CurrentCell force-scrolls it into view is irrelevant
                            try
                            {
                                FMsDGV.CurrentCell = edgeRow.Cells[FMsDGV.FirstDisplayedCell?.ColumnIndex ?? 0];
                            }
                            catch
                            {
                                // ignore
                            }
                            for (int i = 0; i < FMsDGV.RowCount; i++)
                            {
                                var row = FMsDGV.Rows[i];
                                bool sel = home
                                    ? row.Index <= mainSelectedRowIndex
                                    : row.Index >= mainSelectedRowIndex;
                                FMsDGV.SetRowSelected(row.Index, selected: sel, suppressEvent: true);
                            }
                        }
                        else
                        {
                            FMsDGV.ClearSelection();
                        }
                    }
                    SelectAndSuppress(edgeRow.Index, singleSelect: !e.Shift, selectionSyncHack: true);
                    // Have to do these manually because we're suppressing the normal chain of selection logic
                    SetTopRightBlockerVisible();
                    UpdateUIControlsForMultiSelectState(FMsDGV.GetMainSelectedFM());
                }
            }

            // @MULTISEL(FMsDGV nav): Shift-selecting "backwards" (so items deselect back toward main selection)
            // doesn't work with Home/End (but now works with arrows/page keys)
            if (e.KeyCode == Keys.Home || (e.Control && e.KeyCode == Keys.Up))
            {
                await HandleHomeOrEnd(home: true);
            }
            else if (e.KeyCode == Keys.End || (e.Control && e.KeyCode == Keys.Down))
            {
                await HandleHomeOrEnd(home: false);
            }
            // The key suppression is to stop FMs being reloaded when the selection hasn't changed (perf)
            else if (e.KeyCode is Keys.PageUp or Keys.Up)
            {
                if (FMsDGV.RowSelected() && (FMsDGV.Focused || CursorOverControl(FMsDGV)))
                {
                    var firstRow = FMsDGV.Rows[0];
                    if (firstRow.Selected)
                    {
                        if (FMsDGV.MainSelectedRow != firstRow)
                        {
                            using (!e.Shift ? new DisableEvents(this) : null)
                            {
                                SelectAndSuppress(0, singleSelect: !e.Shift, selectionSyncHack: !e.Shift);
                            }
                        }
                        if (!e.Shift) await HandleHomeOrEnd(home: true, selectionSyncHack: e.Shift);
                    }
                    else
                    {
                        FMsDGV.SendKeyDown(e);
                        e.SuppressKeyPress = true;
                    }
                }
            }
            else if (e.KeyCode is Keys.PageDown or Keys.Down)
            {
                if (FMsDGV.RowSelected() && (FMsDGV.Focused || CursorOverControl(FMsDGV)))
                {
                    var lastRow = FMsDGV.Rows[FMsDGV.RowCount - 1];
                    if (lastRow.Selected)
                    {
                        if (FMsDGV.MainSelectedRow != lastRow)
                        {
                            using (!e.Shift ? new DisableEvents(this) : null)
                            {
                                SelectAndSuppress(FMsDGV.RowCount - 1, singleSelect: !e.Shift, selectionSyncHack: !e.Shift);
                            }
                        }
                        if (!e.Shift) await HandleHomeOrEnd(home: false, selectionSyncHack: e.Shift);
                    }
                    else
                    {
                        FMsDGV.SendKeyDown(e);
                        e.SuppressKeyPress = true;
                    }
                }
            }

            #endregion

            else if (e.KeyCode == Keys.Enter)
            {
                FanMission fm;
                if (FMsDGV.Focused && FMsDGV.RowSelected() && GameIsKnownAndSupported((fm = FMsDGV.GetMainSelectedFM()).Game))
                {
                    e.SuppressKeyPress = true;
                    await FMInstallAndPlay.InstallIfNeededAndPlay(fm, askConfIfRequired: true);
                }
            }
            else if (e.KeyCode == Keys.Delete)
            {
                if (FMsDGV.Focused && FMsDGV.RowSelected())
                {
                    await FMArchives.Delete(FMsDGV.GetMainSelectedFM());
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
                        await SortAndSetFilter(keepSelection: true);
                        e.SuppressKeyPress = true;
                    }
                }
            }
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
                else if (e.KeyCode is Keys.Add or Keys.Oemplus)
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
                else if (e.KeyCode is Keys.Subtract or Keys.OemMinus)
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
                else if (e.KeyCode is Keys.D0 or Keys.NumPad0)
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
            if (!EverythingPanel.Enabled || ViewBlocked)
            {
                Dialogs.ShowAlert(
                    LText.AlertMessages.AppClosing_OperationInProgress,
                    LText.AlertMessages.Alert);
                e.Cancel = true;
                return;
            }

            Application.RemoveMessageFilter(this);

            // Argh, stupid hack to get this to not run TWICE on Application.Exit()
            // Application.Exit() is the worst thing ever. Before closing it just does whatever the hell it wants.
            FormClosing -= MainForm_FormClosing;

            AboutToClose = true;

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
            FanMission? selFM = GetMainSelectedFMOrNull();

            try
            {
                if (!startup) EverythingPanel.SuspendDrawing();

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

                SetGameFilterShowHideMenuText();
                GameFilterControlsLLMenu.Localize();

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

                Lazy_ToolStripLabels.Localize(Lazy_ToolStripLabel.FilterByRating);
                // This one is tricky - it could have LText.Global.None as part of its text. Finally caught!
                if (!startup) UpdateRatingLabel();

                FilterShowUnsupportedButton.ToolTipText = LText.FilterBar.ShowUnsupported;
                FilterShowUnavailableButton.ToolTipText = LText.FilterBar.ShowUnavailable;
                FilterShowRecentAtTopButton.ToolTipText = LText.FilterBar.ShowRecentAtTop;

                FilterControlsShowHideButton.ToolTipText = LText.FilterBar.ShowHideMenuToolTip;
                FilterControlsLLMenu.Localize();

                #endregion

                #region Clear/refresh/reset area

                RefreshFromDiskButton.ToolTipText = LText.FilterBar.RefreshFMsListButtonToolTip;
                RefreshFiltersButton.ToolTipText = LText.FilterBar.RefreshFiltersButtonToolTip;
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
                RatingImageColumn.HeaderText = LText.FMsList.RatingColumn;
                FinishedColumn.HeaderText = LText.FMsList.FinishedColumn;
                ReleaseDateColumn.HeaderText = LText.FMsList.ReleaseDateColumn;
                LastPlayedColumn.HeaderText = LText.FMsList.LastPlayedColumn;
                DateAddedColumn.HeaderText = LText.FMsList.DateAddedColumn;
                DisabledModsColumn.HeaderText = LText.FMsList.DisabledModsColumn;
                CommentColumn.HeaderText = LText.FMsList.CommentColumn;

                #endregion

                #endregion

                #region Top-right tabs area

                TopRightMultiSelectBlockerLabel.Text = LText.FMsList.TopRight_MultipleFMsSelected;

                TopRightLLMenu.Localize();

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

                #region Mods tab

                ModsTabPage.Text = LText.ModsTab.TabText;

                ModsHeaderLabel.Text = LText.ModsTab.Header;

                ModsCheckList.RefreshCautionLabelText(LText.ModsTab.ImportantModsCaution);

                ModsShowUberCheckBox.Text = LText.ModsTab.ShowImportantMods;
                ModsResetButton.Text = LText.ModsTab.EnableAll;

                ModsDisabledModsLabel.Text = LText.ModsTab.DisabledMods;

                #endregion

                #endregion

                #region Readme area

                MainToolTip.SetToolTip(ReadmeZoomInButton, LText.Global.ZoomIn);
                MainToolTip.SetToolTip(ReadmeZoomOutButton, LText.Global.ZoomOut);
                MainToolTip.SetToolTip(ReadmeResetZoomButton, LText.Global.ResetZoom);
                MainToolTip.SetToolTip(ReadmeFullScreenButton, LText.ReadmeArea.FullScreenToolTip);
                MainToolTip.SetToolTip(ReadmeEncodingButton, LText.ReadmeArea.CharacterEncoding);

                EncodingsLLMenu.Localize();

                ViewHTMLReadmeLLButton.Localize();

                ChooseReadmeLLPanel.Localize();

                #endregion

                #region Bottom area

                PlayFMButton.Text = LText.MainButtons.PlayFM;

                Lazy_PlayOriginalControls.LocalizeSingle();
                Lazy_PlayOriginalControls.LocalizeMulti();
                PlayOriginalGameLLMenu.Localize();
                PlayOriginalT2InMultiplayerLLMenu.Localize();

                InstallUninstallFMLLButton.Localize(startup);

                WebSearchButton.Text = LText.MainButtons.WebSearch;

                SettingsButton.Text = LText.MainButtons.Settings;
                ExitLLButton.Localize();

                #endregion

                LocalizeProgressBox();
            }
            finally
            {
                if (!startup) EverythingPanel.ResumeDrawing();

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
                if (!startup) EverythingPanel.SuspendDrawing();

                if (startup && !darkMode)
                {
                    ControlUtils.CreateAllControlsHandles(this);
                }
                else
                {
                    ControlUtils.ChangeFormThemeMode(
                        theme,
                        this,
                        _controlColors,
                        x => x.EqualsIfNotNull(ProgressBox)
                             || (_progressBoxConstructed && x is Control xControl &&
                                 ProgressBox!.Controls.Contains(xControl))
                             || x is SplitterPanel,
                        alsoCreateControlHandles: alsoCreateControlHandles,
                        capacity: 150
                    );
                }

                SetReadmeButtonsBackColor(ReadmeRichTextBox.Visible, theme);

                // Set these first so other controls get the right data when they reference them
                Images.DarkModeEnabled = darkMode;
                Images.ReloadImageArrays();

                if (!startup) ControlUtils.RecreateAllToolTipHandles();

                if (!startup || darkMode)
                {
                    MainLLMenu.DarkModeEnabled = darkMode;
                    FMsDGV_FM_LLMenu.DarkModeEnabled = darkMode;
                    FMsDGV_ColumnHeaderLLMenu.DarkModeEnabled = darkMode;
                    TopRightLLMenu.DarkModeEnabled = darkMode;
                    AddTagLLMenu.DarkModeEnabled = darkMode;
                    AddTagLLDropDown.DarkModeEnabled = darkMode;
                    AltTitlesLLMenu.DarkModeEnabled = darkMode;
                    GameFilterControlsLLMenu.DarkModeEnabled = darkMode;
                    FilterControlsLLMenu.DarkModeEnabled = darkMode;
                    PlayOriginalGameLLMenu.DarkModeEnabled = darkMode;
                    PlayOriginalT2InMultiplayerLLMenu.DarkModeEnabled = darkMode;
                    InstallUninstallFMLLButton.DarkModeEnabled = darkMode;
                    ExitLLButton.DarkModeEnabled = darkMode;
                    ViewHTMLReadmeLLButton.DarkModeEnabled = darkMode;
                    ProgressBoxDarkModeEnabled = darkMode;
                    Lazy_FMsListZoomButtons.DarkModeEnabled = darkMode;
                    ChooseReadmeLLPanel.DarkModeEnabled = darkMode;
                    EncodingsLLMenu.DarkModeEnabled = darkMode;
                    Lazy_ToolStripLabels.DarkModeEnabled = darkMode;
                    Lazy_PlayOriginalControls.DarkModeEnabled = darkMode;
                }

                FilterByReleaseDateButton.Image = Images.FilterByReleaseDate;
                FilterByLastPlayedButton.Image = Images.FilterByLastPlayed;
                FilterByTagsButton.Image = Images.FilterByTags;
                FilterByFinishedButton.Image = Images.FilterByFinished;
                FilterByUnfinishedButton.Image = Images.FilterByUnfinished;
                FilterByRatingButton.Image = Images.FilterByRating;
                FilterShowRecentAtTopButton.Image = Images.FilterShowRecentAtTop;

                RefreshFromDiskButton.Image = Images.Refresh;
                RefreshFiltersButton.Image = Images.RefreshFilters;
                ClearFiltersButton.Image = Images.ClearFilters;

                for (int i = 0; i < SupportedGameCount; i++)
                {
                    _filterByGameButtonsInOrder[i].Image = Images.GetPerGameImage(i).Primary.Large();
                }

                var gameTabImages = new Image[SupportedGameCount];
                for (int i = 0; i < SupportedGameCount; i++)
                {
                    gameTabImages[i] = Images.GetPerGameImage(i).Primary.Small();
                }

                GamesTabControl.SetImages(gameTabImages);

                // Have to do this or else they don't show up if we start in dark mode, but they do if we switch
                // while running(?) meh, whatever.
                // UPDATE: Just always set these. Why not. We don't want any other problems with them in the future.
                SetReadmeControlZPosition(true);
            }
            finally
            {
                if (!startup) EverythingPanel.ResumeDrawing();
            }
        }

        #endregion

        #region Helpers & misc

        // TODO: This is a crappy way to do it, make a proper "logical visibility" layer for these
        private void SetReadmeControlZPosition(bool front)
        {
            if (front)
            {
                ChooseReadmeComboBox.BringToFront();
                foreach (DarkButton button in _readmeControlButtons)
                {
                    button.BringToFront();
                }
            }
            else
            {
                ChooseReadmeComboBox.SendToBack();
                ChooseReadmeComboBox.DroppedDown = false;
                foreach (DarkButton button in _readmeControlButtons)
                {
                    button.SendToBack();
                }
            }
        }

        #region Invoke

        public object InvokeSync(Delegate method) => Invoke(method);
        //public object InvokeSync(Delegate method, params object[] args) => Invoke(method, args);

        #endregion

        #region Show menu

        private enum MenuPos { LeftUp, LeftDown, TopLeft, TopRight, RightUp, RightDown, BottomLeft, BottomRight }

        private static void ShowMenu(
            ContextMenuStrip menu,
            Control control,
            MenuPos pos,
            int xOffset = 0,
            int yOffset = 0,
            bool unstickMenu = false)
        {
            int x = pos is MenuPos.LeftUp or MenuPos.LeftDown or MenuPos.TopRight or MenuPos.BottomRight
                ? 0
                : control.Width;

            int y = pos is MenuPos.LeftDown or MenuPos.TopLeft or MenuPos.TopRight or MenuPos.RightDown
                ? 0
                : control.Height;

            var direction = pos switch
            {
                MenuPos.LeftUp or MenuPos.TopLeft => ToolStripDropDownDirection.AboveLeft,
                MenuPos.RightUp or MenuPos.TopRight => ToolStripDropDownDirection.AboveRight,
                MenuPos.LeftDown or MenuPos.BottomLeft => ToolStripDropDownDirection.BelowLeft,
                _ => ToolStripDropDownDirection.BelowRight
            };

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
                if (block) Cursor = Cursors.WaitCursor;

                // Doesn't help the RichTextBox, it happily flickers like it always does. Oh well.
                EverythingPanel.SuspendDrawing();
                ViewBlocked = block;
                ViewBlockingPanel.Visible = block;
                ViewBlockingPanel.BringToFront();
            }
            finally
            {
                EverythingPanel.ResumeDrawing();

                if (!block) Cursor = Cursors.Default;
            }
        }

        private void UpdateConfig()
        {
            GameIndex gameTab = GameIndex.Thief1;
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

            SelectedFM selectedFM = FMsDGV.GetMainSelectedFMPosInfo();

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
                FormWindowStateToWindowState(_nominalWindowState),
                _nominalWindowSize,
                _nominalWindowLocation,
                mainSplitterPercent,
                topSplitterPercent,
                FMsDGV.GetColumnData(),
                FMsDGV.CurrentSortedColumn,
                FMsDGV.CurrentSortDirection,
                FMsDGV.DefaultCellStyle.Font.SizeInPoints,
                FMsDGV.Filter,
                GameFilterControlsLLMenu.GetCheckedStates(),
                FilterControlsLLMenu.GetCheckedStates(),
                selectedFM,
                FMsDGV.GameTabsState,
                gameTab,
                topRightTabs,
                TopSplitContainer.FullScreen,
                ReadmeRichTextBox.ZoomFactor);
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

            Point rpt = this.PointToClient_Fast(control.PointToScreen_Fast(new Point(0, 0)));
            Size rcs = fullArea ? control.Size : control.ClientSize;

            Point ptc = this.PointToClient_Fast(Native.GetCursorPosition_Fast());

            // Don't create eleventy billion Rectangle objects per second
            return ptc.X >= rpt.X && ptc.X < rpt.X + rcs.Width &&
                   ptc.Y >= rpt.Y && ptc.Y < rpt.Y + rcs.Height;
        }

        #endregion

        #endregion

        #region Main menu

        #region Menu button

        private void MainMenuButton_Click(object sender, EventArgs e)
        {
            ShowMenu(MainLLMenu.Menu, MainMenuButton, MenuPos.BottomRight, xOffset: 0, yOffset: 2);
        }

        private void MainMenuButton_Enter(object sender, EventArgs e) => MainMenuButton.HideFocusRectangle();

        #endregion

        #region Menu items

        internal void MainMenu_GameVersionsMenuItem_Click(object sender, EventArgs e)
        {
            using var f = new GameVersionsForm();
            f.ShowDialogDark();
        }

        internal async void ImportMenuItems_Click(object sender, EventArgs e)
        {
            ImportType importType =
                  sender == MainLLMenu.ImportFromDarkLoaderMenuItem
                ? ImportType.DarkLoader
                : sender == MainLLMenu.ImportFromFMSelMenuItem
                ? ImportType.FMSel
                : ImportType.NewDarkLoader;

            await Import.ImportFrom(importType);
        }

        internal async void ScanAllFMsMenuItem_Click(object sender, EventArgs e) => await FMScan.ScanAllFMs();

#if false
        internal void GlobalFMStatsMenuItem_Click(object sender, EventArgs e)
        {
            using var f = new GlobalFMStatsForm();
            f.ShowDialogDark();
        }
#endif

        internal void ViewHelpFileMenuItem_Click(object sender, EventArgs e) => Core.OpenHelpFile();

        internal void AboutMenuItem_Click(object sender, EventArgs e)
        {
            using var f = new AboutForm();
            f.ShowDialogDark();
        }

        internal void Exit_Click(object sender, EventArgs e) => Close();

        #endregion

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
                    GamesTabControl.SelectedTab = _gameTabsInOrder[(int)Config.GameTab];
                }
            }

            for (int i = 0; i < SupportedGameCount; i++)
            {
                GameIndex gameIndex = (GameIndex)i;
                Game game = GameIndexToGame(gameIndex);
                ToolStripButtonCustom button = _filterByGameButtonsInOrder[i];
                button.Checked = Config.Filter.Games.HasFlagFast(game);
            }

            if (!startup) ChangeFilterControlsForGameType();

            AutosizeGameTabsWidth();
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

            AutosizeGameTabsWidth();

            if (refreshFilterBarPositionIfNeeded && Config.GameOrganization == GameOrganization.ByTab)
            {
                PositionFilterBarAfterTabs();
            }
        }

        // Prevents the couple-pixel-high tab page from extending out too far and becoming visible
        private void AutosizeGameTabsWidth()
        {
            if (GamesTabControl.TabCount == 0)
            {
                GamesTabControl.Width = 0;
            }
            else
            {
                var lastGameTabsRect = GamesTabControl.GetTabRect(GamesTabControl.TabCount - 1);
                GamesTabControl.Width = lastGameTabsRect.X + lastGameTabsRect.Width + 5;
            }
        }

        private (SelectedFM GameSelFM, Filter GameFilter, GameIndex GameIndex)
        GetGameSelFMAndFilter(TabPage tabPage)
        {
            SelectedFM? gameSelFM = null;
            Filter? gameFilter = null;
            GameIndex? gameIndex = null;
            for (int i = 0; i < SupportedGameCount; i++)
            {
                if (_gameTabsInOrder[i] == tabPage)
                {
                    gameSelFM = FMsDGV.GameTabsState.GetSelectedFM((GameIndex)i);
                    gameFilter = FMsDGV.GameTabsState.GetFilter((GameIndex)i);
                    gameIndex = (GameIndex)i;
                    break;
                }
            }

            AssertR(gameSelFM != null, "gameSelFM is null: Selected tab is not being handled");
            AssertR(gameFilter != null, "gameFilter is null: Selected tab is not being handled");
            AssertR(gameIndex != null, "gameIndex is null: Selected tab is not being handled");

            return (gameSelFM!, gameFilter!, (GameIndex)gameIndex!);
        }

        private void SaveCurrentTabSelectedFM(TabPage? tabPage)
        {
            if (tabPage == null) return;

            var (gameSelFM, gameFilter, _) = GetGameSelFMAndFilter(tabPage);
            SelectedFM selFM = FMsDGV.GetMainSelectedFMPosInfo();
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

            if (GamesTabControl.SelectedTab == null)
            {
                Config.GameTab = GameIndex.Thief1;
                return;
            }

            var (gameSelFM, gameFilter, gameIndex) = GetGameSelFMAndFilter(GamesTabControl.SelectedTab);

            Config.GameTab = gameIndex;

            for (int i = 0; i < SupportedGameCount; i++)
            {
                _filterByGameButtonsInOrder[i].Checked = gameSelFM == FMsDGV.GameTabsState.GetSelectedFM((GameIndex)i);
            }

            gameSelFM.DeepCopyTo(FMsDGV.CurrentSelFM);
            gameFilter.DeepCopyTo(FMsDGV.Filter);

            SetUIFilterValues(gameFilter);

            await SortAndSetFilter(keepSelection: true, gameTabSwitch: true);
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
        public bool GetShowUnavailableFMsFilter() => FilterShowUnavailableButton.Checked;
        public bool GetShowRecentAtTop() => FilterShowRecentAtTopButton.Checked;

        public List<int> GetFilterShownIndexList() => FMsDGV.FilterShownIndexList;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int TopBarXZero() => MainMenuButton.Left + MainMenuButton.Width + 8;

        // Separate so we can call it from _Load on startup (because it needs the form to be loaded to layout the
        // controls properly) but keep the rest of the work before load
        private void ChangeFilterControlsForGameType()
        {
            if (Config.GameOrganization == GameOrganization.ByTab)
            {
                #region Select target tab in advance

                // Perf optimization: We select what will be our target tab in advance, because otherwise we might
                // cause a chain reaction where one tab gets hidden and the next gets selected, triggering a refresh,
                // and then that tab gets hidden and the next gets selected, triggering a refresh, etc.

                bool[] checkedStates = GameFilterControlsLLMenu.GetCheckedStates();

                int selectedTabOrderIndex = 0;
                for (int i = 0; i < SupportedGameCount; i++)
                {
                    if (GamesTabControl.SelectedTab == _gameTabsInOrder[i])
                    {
                        selectedTabOrderIndex = i;
                        break;
                    }
                }

                if (!checkedStates[selectedTabOrderIndex])
                {
                    int index = 0;
                    for (int i = 0; i < checkedStates.Length; i++)
                    {
                        if (checkedStates[i])
                        {
                            index = i;
                            break;
                        }
                    }

                    GamesTabControl.SelectedTab = _gameTabsInOrder[index];
                }

                // Twice through, show first and then hide, to prevent the possibility of a temporary state of no
                // tabs in the list, thus setting selection to none and screwing us up
                for (int i = 0; i < SupportedGameCount; i++)
                {
                    bool visible = GameFilterControlsLLMenu.GetCheckedStates()[i];
                    if (visible) GamesTabControl.ShowTab(_gameTabsInOrder[i], true);
                }
                for (int i = 0; i < SupportedGameCount; i++)
                {
                    bool visible = GameFilterControlsLLMenu.GetCheckedStates()[i];
                    if (!visible) GamesTabControl.ShowTab(_gameTabsInOrder[i], false);
                }

                #endregion
            }
            else // OneList
            {
                for (int i = 0; i < SupportedGameCount; i++)
                {
                    bool visible = GameFilterControlsLLMenu.GetCheckedStates()[i];
                    var button = _filterByGameButtonsInOrder[i];
                    button.Visible = visible;
                    if (button.Checked && !visible) button.Checked = false;
                }
            }

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

            SetGameFilterShowHideMenuText();

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
            // This is not allowed to happen with the current system (we prevent the last tab from being closed)
            if (GamesTabControl.TabCount == 0)
            {
                filterBarAfterTabsX = TopBarXZero();
            }
            else
            {
                var lastRect = GamesTabControl.GetTabRect(GamesTabControl.TabCount - 1);
                filterBarAfterTabsX = TopBarXZero() + lastRect.X + lastRect.Width + 5;
            }

            FilterBarFLP.Location = FilterBarFLP.Location with { X = filterBarAfterTabsX };
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

                    if (f.ShowDialogDark() != DialogResult.OK) return;

                    FMsDGV.Filter.SetDateFromAndTo(lastPlayed, f.DateFrom, f.DateTo);

                    button.Checked = f.DateFrom != null || f.DateTo != null;
                }

                UpdateDateLabel(lastPlayed);
            }
            else if (sender == FilterByTagsButton)
            {
                using var tf = new FilterTagsForm(GlobalTags, FMsDGV.Filter.Tags);
                if (tf.ShowDialogDark() != DialogResult.OK) return;

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

                    if (f.ShowDialogDark() != DialogResult.OK) return;
                    FMsDGV.Filter.SetRatingFromAndTo(f.RatingFrom, f.RatingTo);
                    FilterByRatingButton.Checked =
                        !(FMsDGV.Filter.RatingFrom == -1 && FMsDGV.Filter.RatingTo == 10);
                }

                UpdateRatingLabel();
            }

            await SortAndSetFilter(keepSelection: true);
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

                    Lazy_ToolStripLabels.Show(
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

                if (sender == ClearFiltersButton)
                {
                    ClearUIAndCurrentInternalFilter();
                }
                else if (sender == FilterShowUnsupportedButton)
                {
                    Config.ShowUnsupported = FilterShowUnsupportedButton.Checked;
                }
                else if (sender == FilterShowUnavailableButton)
                {
                    Config.ShowUnavailableFMs = FilterShowUnavailableButton.Checked;
                }

                // Don't keep selection for title/author, cause you want to end up on the FM you typed as soon as
                // possible
                bool keepSel = sender != FilterShowRecentAtTopButton && !senderIsTextBox;
                await SortAndSetFilter(
                    keepSelection: keepSel,
                    landImmediate: senderIsTextBox &&
                                   (!FilterTitleTextBox.Text.IsWhiteSpace() ||
                                   !FilterAuthorTextBox.Text.IsWhiteSpace()));
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
                FilterBarScrollLeftButton.Location = FilterBarFLP.Location with { Y = FilterBarFLP.Location.Y + 1 };
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

        private void GameFilterControlsShowHideButton_Click(object sender, EventArgs e)
        {
            ShowMenu(GameFilterControlsLLMenu.Menu,
                GameFilterControlsShowHideButtonToolStrip,
                MenuPos.RightDown,
                -GameFilterControlsShowHideButton.Width,
                GameFilterControlsShowHideButton.Height);
        }

        [SuppressMessage("ReSharper", "MemberCanBeMadeStatic.Global")]
        internal async void GameFilterControlsMenuItems_Click(object sender, EventArgs e)
        {
            var s = (ToolStripMenuItemCustom)sender;

            if (!s.Checked && GameFilterControlsLLMenu.GetCheckedStates().All(x => !x))
            {
                s.Checked = true;
                return;
            }

            if (Config.GameOrganization == GameOrganization.OneList)
            {
                ToolStripButtonCustom? button = null;
                for (int i = 0; i < SupportedGameCount; i++)
                {
                    if (s == (ToolStripMenuItemCustom)GameFilterControlsLLMenu.Menu.Items[i])
                    {
                        button = _filterByGameButtonsInOrder[i];
                        break;
                    }
                }

                AssertR(button != null, nameof(button) + " is null - button does not have a corresponding menu item");

                button!.Visible = s.Checked;
                if (button!.Checked && !s.Checked) button.Checked = false;

                // We have to refresh manually because Checked change doesn't trigger the refresh, only Click
                await SortAndSetFilter(keepSelection: true);
            }
            else // ByTab
            {
                TabPage? tab = null;
                for (int i = 0; i < SupportedGameCount; i++)
                {
                    if (s == (ToolStripMenuItemCustom)GameFilterControlsLLMenu.Menu.Items[i])
                    {
                        tab = _gameTabsInOrder[i];
                        break;
                    }
                }

                AssertR(tab != null, nameof(tab) + " is null - tab does not have a corresponding menu item");

                // We don't need to do a manual refresh here because ShowTab will end up resulting in one
                GamesTabControl.ShowTab(tab!, s.Checked);
                AutosizeGameTabsWidth();
                PositionFilterBarAfterTabs();
            }
        }

        private void SetGameFilterShowHideMenuText() =>
            GameFilterControlsShowHideButton.ToolTipText =
                Config.GameOrganization == GameOrganization.OneList
                    ? LText.FilterBar.ShowHideGameFilterMenu_Filters_ToolTip
                    : LText.FilterBar.ShowHideGameFilterMenu_Tabs_ToolTip;

        #endregion

        #region Filter controls visibility menu

        private void FilterControlsShowHideButton_Click(object sender, EventArgs e)
        {
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

        public void RefreshFM(FanMission fm, bool rowOnly = false)
        {
            FanMission? selectedFM = GetMainSelectedFMOrNull();
            if (selectedFM == null) return;

            if (selectedFM == fm)
            {
                FMsDGV.InvalidateRow(FMsDGV.MainSelectedRow!.Index);
                if (!rowOnly) UpdateAllFMUIDataExceptReadme(selectedFM);
            }
            else
            {
                RefreshFMsListKeepSelection();
            }
        }

        public void RefreshCurrentFM_IncludeInstalledState()
        {
            FanMission? selectedFM = GetMainSelectedFMOrNull();
            if (selectedFM == null) return;

            UpdateUIControlsForMultiSelectState(selectedFM);
        }

        public void RefreshAllSelectedFMs(bool rowOnly = false)
        {
            if (!rowOnly)
            {
                FanMission? selectedFM = GetMainSelectedFMOrNull();
                if (selectedFM != null)
                {
                    UpdateAllFMUIDataExceptReadme(selectedFM);
                }
            }
            RefreshFMsListKeepSelection();
        }

        public void RefreshAllSelectedFMRows()
        {
            var selRows = FMsDGV.SelectedRows;
            foreach (DataGridViewRow row in selRows)
            {
                FMsDGV.InvalidateRow(row.Index);
            }
        }

        /// <summary>
        /// Returns false if the list is empty and ClearShownData() has been called, otherwise true
        /// </summary>
        /// <param name="selectedFM"></param>
        /// <param name="startup"></param>
        /// <param name="keepSelection"></param>
        /// <param name="fromColumnClick"></param>
        /// <param name="multiSelectedFMs"></param>
        /// <returns></returns>
        private bool RefreshFMsList(SelectedFM? selectedFM, bool startup = false, KeepSel keepSelection = KeepSel.False,
                                    bool fromColumnClick = false, FanMission[]? multiSelectedFMs = null)
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
                                FMsDGV.FirstDisplayedScrollingRowIndex =
                                    FMsDGV.CurrentSortDirection == SortDirection.Ascending
                                        ? row.ClampToZero()
                                        : (row - FMsDGV.DisplayedRowCount(true)).ClampToZero();
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

                    bool selectDoneAtLeastOnce = false;
                    void DoSelect()
                    {
                        if (keepSelection != KeepSel.False) EventsDisabled = true;
                        FMsDGV.SelectSingle(row, suppressSelectionChangedEvent: !selectDoneAtLeastOnce);
                        FMsDGV.SelectProperly(suspendResume: startup);
                        selectDoneAtLeastOnce = true;
                    }

                    // @SEL_SYNC_HACK
                    // Stupid hack to attempt to prevent multiselect-set-popping-back-to-starting-at-list-top
                    FMsDGV.MultiSelect = false;
                    DoSelect();
                    FMsDGV.MultiSelect = true;
                    DoSelect();

                    if (multiSelectedFMs != null)
                    {
                        foreach (FanMission fm in multiSelectedFMs)
                        {
                            int index = FMsDGV.GetIndexFromFM(fm);
                            if (index > -1)
                            {
                                FMsDGV.Rows[index].Selected = true;
                            }
                        }
                    }

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

        public void RefreshFMsListKeepSelection(bool keepMulti = true)
        {
            if (FMsDGV.RowCount == 0) return;

            int selectedRow = FMsDGV.MainSelectedRow!.Index;
            var selRows = FMsDGV.SelectedRows;

            try
            {
                TopSplitContainer.Panel1.SuspendDrawing();
                using (new DisableEvents(this))
                {
                    FMsDGV.SelectSingle(selectedRow);
                    // TODO: This pops our position back to put selected FM in view - but do we really need to run this here?
                    // Alternatively, maybe SelectProperly() should pop us back to where we were after it's done?
                    FMsDGV.SelectProperly();
                    if (keepMulti)
                    {
                        foreach (DataGridViewRow row in selRows)
                        {
                            FMsDGV.Rows[row.Index].Selected = true;
                        }
                    }
                }
            }
            finally
            {
                TopSplitContainer.Panel1.ResumeDrawing();
            }
        }

        #endregion

        #region Top-right area

        // @VBL (technically)
        // Hook them all up to one event handler to avoid extraneous async/awaits
        private async void FieldScanButtons_Click(object sender, EventArgs e)
        {
            if (sender == EditFMScanForReadmesButton)
            {
                Ini.WriteFullFMDataIni();
                _displayedFM = await Core.DisplayFM(refreshCache: true);
            }
            else
            {
                var scanOptions =
                    sender == EditFMScanTitleButton ? FMScanner.ScanOptions.FalseDefault(scanTitle: true) :
                    sender == EditFMScanAuthorButton ? FMScanner.ScanOptions.FalseDefault(scanAuthor: true) :
                    sender == EditFMScanReleaseDateButton ? FMScanner.ScanOptions.FalseDefault(scanReleaseDate: true) :
                    //sender == StatsScanCustomResourcesButton
                    FMScanner.ScanOptions.FalseDefault(scanCustomResources: true);

                FanMission fm = FMsDGV.GetMainSelectedFM();
                if (await FMScan.ScanFMs(new List<FanMission> { fm }, scanOptions, hideBoxIfZip: true))
                {
                    RefreshFM(fm);
                }
            }
        }

        #region Edit FM tab

        private void EditFMAltTitlesArrowButton_Click(object sender, EventArgs e)
        {
            FillAltTitlesMenu(FMsDGV.GetMainSelectedFM().AltTitles);
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
            FanMission fm = FMsDGV.GetMainSelectedFM();
            fm.Title = EditFMTitleTextBox.Text;
            RefreshFM(fm, rowOnly: true);
        }

        private void EditFMTitleTextBox_Leave(object sender, EventArgs e)
        {
            if (EventsDisabled) return;
            Ini.WriteFullFMDataIni();
        }

        private void EditFMAuthorTextBox_TextChanged(object sender, EventArgs e)
        {
            if (EventsDisabled) return;
            FanMission fm = FMsDGV.GetMainSelectedFM();
            fm.Author = EditFMAuthorTextBox.Text;
            RefreshFM(fm, rowOnly: true);
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

            FanMission fm = FMsDGV.GetMainSelectedFM();
            fm.ReleaseDate.DateTime = EditFMReleaseDateCheckBox.Checked
                ? EditFMReleaseDateDateTimePicker.Value
                : null;

            RefreshFM(fm, rowOnly: true);
            Ini.WriteFullFMDataIni();
        }

        private void EditFMReleaseDateDateTimePicker_ValueChanged(object sender, EventArgs e)
        {
            if (EventsDisabled) return;
            FanMission fm = FMsDGV.GetMainSelectedFM();
            fm.ReleaseDate.DateTime = EditFMReleaseDateDateTimePicker.Value;
            RefreshFM(fm, rowOnly: true);
            Ini.WriteFullFMDataIni();
        }

        private void EditFMLastPlayedCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (EventsDisabled) return;
            EditFMLastPlayedDateTimePicker.Visible = EditFMLastPlayedCheckBox.Checked;

            FanMission fm = FMsDGV.GetMainSelectedFM();
            fm.LastPlayed.DateTime = EditFMLastPlayedCheckBox.Checked
                ? EditFMLastPlayedDateTimePicker.Value
                : null;

            RefreshFM(fm, rowOnly: true);
            Ini.WriteFullFMDataIni();
        }

        private void EditFMLastPlayedDateTimePicker_ValueChanged(object sender, EventArgs e)
        {
            if (EventsDisabled) return;
            FanMission fm = FMsDGV.GetMainSelectedFM();
            fm.LastPlayed.DateTime = EditFMLastPlayedDateTimePicker.Value;
            RefreshFM(fm, rowOnly: true);
            Ini.WriteFullFMDataIni();
        }

        private void EditFMRatingComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (EventsDisabled) return;
            UpdateRatingForSelectedFMs(EditFMRatingComboBox.SelectedIndex - 1);
        }

        internal void UpdateRatingForSelectedFMs(int rating)
        {
            FanMission fm = FMsDGV.GetMainSelectedFM();
            fm.Rating = rating;
            RefreshFM(fm, rowOnly: true);

            UpdateRatingMenus(rating, disableEvents: true);

            FanMission[] sFMs = FMsDGV.GetSelectedFMs();
            if (sFMs.Length > 1)
            {
                foreach (FanMission sFM in sFMs)
                {
                    sFM.Rating = rating;
                }
                RefreshAllSelectedFMs(rowOnly: true);
            }
            Ini.WriteFullFMDataIni();
        }

        private void EditFMLanguageComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (EventsDisabled) return;
            Core.UpdateFMSelectedLanguage();
        }

        private void EditFMFinishedOnButton_Click(object sender, EventArgs e)
        {
            ShowMenu(FMsDGV_FM_LLMenu.GetFinishedOnMenu(), EditFMFinishedOnButton, MenuPos.BottomRight, unstickMenu: true);
        }

        private void EditFMScanLanguagesButton_Click(object sender, EventArgs e)
        {
            using (new DisableEvents(this))
            {
                Core.ScanAndFillLanguagesList(forceScan: true);
            }
            Ini.WriteFullFMDataIni();
        }

        #endregion

        #region Comment tab

        public string GetFMCommentText() => CommentTextBox.Text;

        private void CommentTextBox_TextChanged(object sender, EventArgs e)
        {
            if (EventsDisabled) return;
            Core.UpdateFMComment();
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
                AddTagLLDropDown.SetItemsAndShow(list);
            }
        }

        internal void AddTagTextBoxOrListBox_KeyDown(object sender, KeyEventArgs e)
        {
            DarkListBox box = AddTagLLDropDown.ListBox;

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
                    string catAndTag = box.SelectedIndex == -1 ? AddTagTextBox.Text : box.SelectedItem;
                    FMTags.AddTagOperation(FMsDGV.GetMainSelectedFM(), catAndTag);
                    break;
                default:
                    if (sender == box) AddTagTextBox.Focus();
                    break;
            }
        }

        internal void AddTagListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (AddTagLLDropDown.ListBox.SelectedIndex == -1) return;

            using (new DisableEvents(this))
            {
                AddTagTextBox.Text = AddTagLLDropDown.ListBox.SelectedItem;
            }

            if (AddTagTextBox.Text.Length > 0)
            {
                AddTagTextBox.SelectionStart = AddTagTextBox.Text.Length;
            }
        }

        public FanMission? GetMainSelectedFMOrNull() => FMsDGV.RowSelected() ? FMsDGV.GetMainSelectedFM() : null;

        /// <summary>
        /// Order is not guaranteed. Seems to be in reverse order currently but who knows. Use <see cref="GetSelectedFMs_InOrder"/>
        /// if you need them in visual order.
        /// </summary>
        /// <returns></returns>
        public FanMission[] GetSelectedFMs() => FMsDGV.GetSelectedFMs();

        /// <summary>
        /// Use this if you need the FMs in visual order, but take a (probably minor-ish) perf/mem hit.
        /// </summary>
        /// <returns></returns>
        public FanMission[] GetSelectedFMs_InOrder() => FMsDGV.GetSelectedFMs_InOrder();

        public FanMission? GetFMFromIndex(int index) => FMsDGV.RowSelected() ? FMsDGV.GetFMFromIndex(index) : null;

        public (string Category, string Tag)
        SelectedCategoryAndTag()
        {
            TreeNode selNode = TagsTreeView.SelectedNode;
            TreeNode parent;

            return selNode == null
                ? ("", "")
                : (parent = selNode.Parent) == null
                ? (selNode.Text, "")
                : (parent.Text, selNode.Text);
        }

        private void RemoveTagButton_Click(object sender, EventArgs e) => FMTags.RemoveTagOperation();

        internal void AddTagListBox_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;

            if (AddTagLLDropDown.ListBox.SelectedIndex > -1)
            {
                FMTags.AddTagOperation(FMsDGV.GetMainSelectedFM(), AddTagLLDropDown.ListBox.SelectedItem);
            }
        }

        public void ClearTagsSearchBox()
        {
            AddTagTextBox.Clear();
            AddTagLLDropDown.HideAndClear();
        }

        private void AddTagButton_Click(object sender, EventArgs e)
        {
            FMTags.AddTagOperation(FMsDGV.GetMainSelectedFM(), AddTagTextBox.Text);
        }

        // @VBL (AddTagFromListButton_Click - lots of menu items and event hookups)
        private void AddTagFromListButton_Click(object sender, EventArgs e)
        {
            GlobalTags.SortAndMoveMiscToEnd();

            AddTagLLMenu.Menu.Items.Clear();

            var addTagMenuItems = new List<ToolStripItem>(GlobalTags.Count);
            foreach (CatAndTagsList item in GlobalTags)
            {
                if (item.Tags.Count == 0)
                {
                    var catItem = new ToolStripMenuItemWithBackingText(item.Category + ":") { Tag = LoadType.Lazy };
                    catItem.Click += AddTagMenuEmptyItem_Click;
                    addTagMenuItems.Add(catItem);
                }
                else
                {
                    var catItem = new ToolStripMenuItemWithBackingText(item.Category) { Tag = LoadType.Lazy };
                    addTagMenuItems.Add(catItem);

                    var last = addTagMenuItems[addTagMenuItems.Count - 1];

                    if (item.Category != PresetTags.MiscCategory)
                    {
                        var customItem = new ToolStripMenuItemWithBackingText(LText.TagsTab.CustomTagInCategory) { Tag = LoadType.Lazy };
                        customItem.Click += AddTagMenuCustomItem_Click;
                        ((ToolStripMenuItemWithBackingText)last).DropDownItems.Add(customItem);
                        ((ToolStripMenuItemWithBackingText)last).DropDownItems.Add(new ToolStripSeparator { Tag = LoadType.Lazy });
                    }

                    foreach (string tag in item.Tags)
                    {
                        var tagItem = new ToolStripMenuItemWithBackingText(tag) { Tag = LoadType.Lazy };

                        tagItem.Click += item.Category == PresetTags.MiscCategory
                            ? AddTagMenuMiscItem_Click
                            : AddTagMenuItem_Click;

                        ((ToolStripMenuItemWithBackingText)last).DropDownItems.Add(tagItem);
                    }
                }
            }

            AddTagLLMenu.AddRange(addTagMenuItems.ToArray());

            ShowMenu(AddTagLLMenu.Menu, AddTagFromListButton, MenuPos.LeftDown);
        }

        private void AddTagMenuItem_Click(object sender, EventArgs e)
        {
            var item = (ToolStripMenuItemWithBackingText)sender;
            if (item.HasDropDownItems) return;

            var cat = (ToolStripMenuItemWithBackingText?)item.OwnerItem;
            if (cat == null) return;

            FMTags.AddTagOperation(FMsDGV.GetMainSelectedFM(), cat.BackingText + ": " + item.BackingText);
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
            if (PatchDMLsListBox.SelectedIndex == -1) return;

            bool success = Core.RemoveDML(FMsDGV.GetMainSelectedFM(), PatchDMLsListBox.SelectedItem);
            if (!success) return;

            PatchDMLsListBox.RemoveAndSelectNearest();
        }

        // @VBL
        private void PatchAddDMLButton_Click(object sender, EventArgs e)
        {
            var dmlFiles = new List<string>();

            using (var d = new OpenFileDialog())
            {
                d.Multiselect = true;
                d.Filter = LText.BrowseDialogs.DMLFiles + "|*.dml";
                if (d.ShowDialogDark() != DialogResult.OK || d.FileNames.Length == 0) return;
                dmlFiles.AddRange(d.FileNames);
            }

            var itemsHashSet = PatchDMLsListBox.ItemsAsStrings.ToHashSetI();

            try
            {
                PatchDMLsListBox.BeginUpdate();
                foreach (string f in dmlFiles)
                {
                    if (f.IsEmpty()) continue;

                    bool success = Core.AddDML(FMsDGV.GetMainSelectedFM(), f);
                    if (!success) return;

                    string dmlFileName = Path.GetFileName(f);
                    if (!itemsHashSet.Contains(dmlFileName))
                    {
                        PatchDMLsListBox.Items.Add(dmlFileName);
                    }
                }
            }
            finally
            {
                PatchDMLsListBox.EndUpdate();
            }
        }

        private void PatchOpenFMFolderButton_Click(object sender, EventArgs e) => Core.OpenFMFolder(FMsDGV.GetMainSelectedFM());

        #endregion

        #region Mods tab

        private void ModsDisabledModsTextBox_TextChanged(object sender, EventArgs e)
        {
            if (EventsDisabled) return;
            FanMission fm = FMsDGV.GetMainSelectedFM();
            fm.DisabledMods = ModsDisabledModsTextBox.Text;
            RefreshFM(fm, rowOnly: true);
        }

        private void ModsDisabledModsTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                ModsDisabledModsTextBoxCommit();
            }
        }

        private void ModsDisabledModsTextBox_Leave(object sender, EventArgs e)
        {
            ModsDisabledModsTextBoxCommit();
        }

        // @VBL(ModsDisabledModsTextBoxCommit()): But actually maybe not?
        // This looks business-logic-ish, but really it's kind of a UI detail still? Directly updating a list of
        // checkboxes from a textbox. Meh.
        private void ModsDisabledModsTextBoxCommit()
        {
            if (EventsDisabled) return;

            if (!FMsDGV.RowSelected()) return;

            string[] disabledMods = FMsDGV.GetMainSelectedFM().DisabledMods.Split(CA_Plus, StringSplitOptions.RemoveEmptyEntries);

            var modNames = new DictionaryI<int>(ModsCheckList.CheckItems.Length);

            for (int i = 0; i < ModsCheckList.CheckItems.Length; i++)
            {
                var checkItem = ModsCheckList.CheckItems[i];
                modNames[checkItem.Text] = i;
            }

            bool[] checkedStates = InitializedArray(ModsCheckList.CheckItems.Length, true);

            foreach (string mod in disabledMods)
            {
                if (modNames.TryGetValue(mod, out int index))
                {
                    checkedStates[index] = false;
                }
            }

            ModsCheckList.SetItemCheckedStates(checkedStates);

            Ini.WriteFullFMDataIni();
        }

        // @VBL
        private void ModsCheckList_ItemCheckedChanged(object sender, DarkCheckList.DarkCheckListEventArgs e)
        {
            if (EventsDisabled) return;

            if (!FMsDGV.RowSelected()) return;

            var fm = FMsDGV.GetMainSelectedFM();

            fm.DisabledMods = "";

            foreach (DarkCheckList.CheckItem item in ModsCheckList.CheckItems)
            {
                if (!item.Checked)
                {
                    if (!fm.DisabledMods.IsEmpty()) fm.DisabledMods += "+";
                    fm.DisabledMods += item.Text;
                }
            }

            using (new DisableEvents(this))
            {
                ModsDisabledModsTextBox.Text = fm.DisabledMods;
            }

            fm.DisabledMods = ModsDisabledModsTextBox.Text;
            RefreshFM(fm, rowOnly: true);

            Ini.WriteFullFMDataIni();
        }

        private void ModsShowUberCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            ModsCheckList.ShowCautionSection(ModsShowUberCheckBox.Checked);
        }

        private void ModsResetButton_Click(object sender, EventArgs e)
        {
            var fm = FMsDGV.GetMainSelectedFM();

            using (new DisableEvents(this))
            {
                foreach (Control control in ModsCheckList.Controls)
                {
                    if (control is CheckBox checkBox)
                    {
                        checkBox.Checked = true;
                    }
                }

                fm.DisabledMods = "";
                ModsDisabledModsTextBox.Text = "";
            }

            RefreshFM(fm, rowOnly: true);
        }

        #endregion

        private void TopRightCollapseButton_Click(object sender, EventArgs e)
        {
            TopSplitContainer.ToggleFullScreen();
            SetTopRightCollapsedState();
        }

        private void SetTopRightCollapsedState()
        {
            bool collapsed = TopSplitContainer.FullScreen;

            if (collapsed)
            {
                TopRightTabControl.Enabled = false;
            }
            else
            {
                if (!TopRightMultiSelectBlockerPanel.Visible)
                {
                    TopRightTabControl.Enabled = true;
                }
            }

            TopRightCollapseButton.ArrowDirection = collapsed ? Direction.Left : Direction.Right;
        }

        private void TopRightMenuButton_Click(object sender, EventArgs e)
        {
            ShowMenu(TopRightLLMenu.Menu, TopRightMenuButton, MenuPos.BottomLeft);
        }

        internal void TopRightMenu_MenuItems_Click(object sender, EventArgs e)
        {
            var s = (ToolStripMenuItemCustom)sender;

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

        private FanMission? _displayedFM;

        public int GetMainSelectedRowIndex() => FMsDGV.RowSelected() ? FMsDGV.MainSelectedRow!.Index : -1;

        public SelectedFM? GetFMPosInfoFromIndex(int index) =>
            FMsDGV.RowCount == 0 || index < 0 || index >= FMsDGV.RowCount
                ? null
                : FMsDGV.GetFMPosInfoFromIndex(index);

        public bool RowSelected(int index) => FMsDGV.RowCount > 0 && index > -1 && index < FMsDGV.RowCount &&
                                              FMsDGV.Rows[index].Selected;

        public SelectedFM? GetMainSelectedFMPosInfo() => FMsDGV.RowSelected() ? FMsDGV.GetMainSelectedFMPosInfo() : null;

        public int GetRowCount() => FMsDGV.RowCount;

        public void SetRowCount(int count) => FMsDGV.RowCount = count;

        public void ShowFMsListZoomButtons(bool visible)
        {
            Lazy_FMsListZoomButtons.SetVisible(visible);
            SetFilterBarWidth();
        }

        private void ZoomFMsDGV(ZoomFMsDGVType type, float? zoomFontSize = null)
        {
            try
            {
                // If we suspend the form, our mouse events go THROUGH it to any underlying window and affect that.
                // If we suspend FMsDGV, it straight-up doesn't work and still flickers.
                // So suspend EverythingPanel instead and it works fine.
                TopSplitContainer.Panel1.SuspendDrawing();

                SelectedFM? selFM = FMsDGV.RowSelected() ? FMsDGV.GetMainSelectedFMPosInfo() : null;

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
                var selRows = FMsDGV.SelectedRows;
                int[] selIndices = new int[selRows.Count];
                for (int i = 0; i < selRows.Count; i++)
                {
                    selIndices[i] = selRows[i].Index;
                }
                using (new DisableEvents(this))
                {
                    // Force a regeneration of rows (height will take effect here)
                    int rowCount = FMsDGV.RowCount;
                    FMsDGV.RowCount = 0;
                    FMsDGV.RowCount = rowCount;
                    // Setting RowCount to > 0 causes the first row to be auto-selected, so de-select it.
                    FMsDGV.ClearSelection();

                    // Restore previous selection (no events will be fired, due to being in a DisableEvents block)
                    if (selIndices.Length > 0)
                    {
                        // Sort and select in reverse order so the topmost selection is always the "main" selection
                        Array.Sort(selIndices);
                        for (int i = selIndices.Length - 1; i >= 0; i--)
                        {
                            FMsDGV.Rows[selIndices[i]].Selected = true;
                        }

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
                if (selIndices.Length > 0 && selFM != null) CenterSelectedFM();
            }
            finally
            {
                TopSplitContainer.Panel1.ResumeDrawing();
            }
        }

        private void CenterSelectedFM()
        {
            try
            {
                FMsDGV.FirstDisplayedScrollingRowIndex =
                    (FMsDGV.MainSelectedRow!.Index - (FMsDGV.DisplayedRowCount(true) / 2))
                    .Clamp(0, FMsDGV.RowCount - 1);
            }
            catch
            {
                // no room is available to display rows
            }
        }

        public void SetPinnedMenuState(bool pinned)
        {
            FMsDGV_FM_LLMenu.SetPinOrUnpinMenuItemState(!pinned);
        }

        #region FMs list sorting

        public Column GetCurrentSortedColumnIndex() => FMsDGV.CurrentSortedColumn;

        public SortDirection GetCurrentSortDirection() => FMsDGV.CurrentSortDirection;

        private void SortFMsDGV(Column column, SortDirection sortDirection)
        {
            FMsDGV.CurrentSortedColumn = column;
            FMsDGV.CurrentSortDirection = sortDirection;

            Core.SortFMsViewList(column, sortDirection);

            int intCol = (int)column;
            for (int i = 0; i < FMsDGV.Columns.Count; i++)
            {
                DataGridViewColumn c = FMsDGV.Columns[i];
                if (i == intCol)
                {
                    c.HeaderCell.SortGlyphDirection =
                        FMsDGV.CurrentSortDirection == SortDirection.Ascending
                            ? SortOrder.Ascending
                            : SortOrder.Descending;
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
        /// <param name="landImmediate"></param>
        /// <param name="keepMultiSelection"></param>
        /// <returns></returns>
        public async Task SortAndSetFilter(SelectedFM? selectedFM = null, bool forceDisplayFM = false,
                                           bool keepSelection = false, bool gameTabSwitch = false,
                                           bool landImmediate = false, bool keepMultiSelection = false)
        {
            bool selFMWasPassedIn = selectedFM != null;

            FanMission? oldSelectedFM = GetMainSelectedFMOrNull();

            selectedFM ??= keepSelection && !gameTabSwitch && FMsDGV.RowSelected()
                ? FMsDGV.GetMainSelectedFMPosInfo()
                : null;

            // Do this before any changes to the list (set filter, sort, etc.) because otherwise it will be wrong
            FanMission[]? multiSelectedFMs = keepMultiSelection ? FMsDGV.GetSelectedFMs() : null;

            KeepSel keepSel =
                selectedFM != null ? KeepSel.TrueNearest :
                keepSelection || gameTabSwitch ? KeepSel.True : KeepSel.False;

            // Fix: in RefreshFMsList, CurrentSelFM was being used when coming from no FMs listed to some FMs listed
            if (!gameTabSwitch && !selFMWasPassedIn && oldSelectedFM == null) keepSel = KeepSel.False;

            if (gameTabSwitch) forceDisplayFM = true;

            SortFMsDGV(FMsDGV.CurrentSortedColumn, FMsDGV.CurrentSortDirection);

            Core.SetFilter();

            if (landImmediate && FMsDGV.FilterShownIndexList.Count > 0)
            {
                bool foundUnTopped = false;
                for (int i = 0; i < FMsDGV.FilterShownIndexList.Count; i++)
                {
                    var fm = FMsDGV.GetFMFromIndex(i);
                    if (!fm.MarkedRecent && !fm.Pinned)
                    {
                        selectedFM = FMsDGV.GetFMPosInfoFromIndex(i);
                        keepSel = KeepSel.True;
                        foundUnTopped = true;
                        break;
                    }
                }

                if (!foundUnTopped)
                {
                    bool titleIsWhiteSpace = FilterTitleTextBox.Text.IsWhiteSpace();
                    bool authorIsWhiteSpace = FilterAuthorTextBox.Text.IsWhiteSpace();

                    for (int i = 0; i < FMsDGV.FilterShownIndexList.Count; i++)
                    {
                        var fm = FMsDGV.GetFMFromIndex(i);

                        if (!fm.MarkedRecent && !fm.Pinned)
                        {
                            break;
                        }

                        if ((!titleIsWhiteSpace && Core.FMTitleContains_AllTests(fm, FilterTitleTextBox.Text, FilterTitleTextBox.Text.Trim())) ||
                            (!authorIsWhiteSpace && fm.Author.ContainsI(FilterAuthorTextBox.Text)))
                        {
                            selectedFM = FMsDGV.GetFMPosInfoFromIndex(i);
                            keepSel = KeepSel.True;
                            break;
                        }
                    }
                }
            }

            if (RefreshFMsList(selectedFM, keepSelection: keepSel, multiSelectedFMs: multiSelectedFMs))
            {
                // DEBUG: Keep this in for testing this because the whole thing is irrepressibly finicky
                //Trace.WriteLine(nameof(keepSelection) + ": " + keepSelection);
                //Trace.WriteLine("selectedFM != null: " + (selectedFM != null));
                //Trace.WriteLine("!selectedFM.InstalledName.IsEmpty(): " + (selectedFM != null && !selectedFM.InstalledName.IsEmpty()));
                //Trace.WriteLine("selectedFM.InstalledName != FMsDGV.GetMainSelectedFM().InstalledDir: " + (selectedFM != null && selectedFM.InstalledName != FMsDGV.GetMainSelectedFM().InstalledDir));

                // Optimization in case we land on the same FM as before, don't reload it
                // And whaddaya know, I still ended up having to have this eyes-glazing-over stuff here.
                if (forceDisplayFM ||
                    (keepSelection &&
                     selectedFM != null && !selectedFM.InstalledName.IsEmpty() &&
                     selectedFM.InstalledName != FMsDGV.GetMainSelectedFM().InstalledDir) ||
                    (!keepSelection &&
                     (oldSelectedFM == null ||
                      (FMsDGV.RowSelected() && !oldSelectedFM.Equals(FMsDGV.GetMainSelectedFM())))) ||
                    // Fix: when resetting release date filter the readme wouldn't load for the selected FM
                    oldSelectedFM == null)
                {
                    _displayedFM = await Core.DisplayFM();
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
                : size is >= ByteSize.MB and < ByteSize.GB
                ? Math.Round(size / 1024f / 1024f).ToString(CultureInfo.CurrentCulture) + " " + LText.Global.MegabyteShort
                : Math.Round(size / 1024f / 1024f / 1024f, 2).ToString(CultureInfo.CurrentCulture) + " " + LText.Global.GigabyteShort;

            const string pinChar = "\U0001F4CC ";

            switch ((Column)e.ColumnIndex)
            {
                case Column.Game:
                    e.Value =
                        GameIsKnownAndSupported(fm.Game) ? Images.FMsList_GameIcons[(int)GameToGameIndex(fm.Game)] :
                        fm.Game == Game.Unsupported ? Images.RedQuestionMarkCircle :
                        // Can't say null, or else it sets an ugly red-x image
                        Images.Blank;
                    break;

                case Column.Installed:
                    e.Value = fm.Installed ? Images.GreenCheckCircle : Images.Blank;
                    break;

                case Column.Title:
                    string finalTitle;
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
                        finalTitle = title;
                    }
                    else
                    {
                        finalTitle = fm.Title;
                    }

                    if (TitleColumn.Visible && fm.Pinned)
                    {
                        finalTitle = pinChar + finalTitle;
                    }

                    e.Value = finalTitle;

                    break;

                case Column.Archive:

                    e.Value = !TitleColumn.Visible && ArchiveColumn.Visible && fm.Pinned
                        ? pinChar + fm.Archive
                        : fm.Archive;
                    break;

                case Column.Author:
                    e.Value = !TitleColumn.Visible && !ArchiveColumn.Visible && AuthorColumn.Visible && fm.Pinned
                        ? pinChar + fm.Author
                        : fm.Author;
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
                            e.Value = fm.Rating == -1 ? Images.Blank : Images.FMsList_StarIcons[fm.Rating];
                        }
                        else
                        {
                            e.Value = fm.Rating == -1 ? "" : (fm.Rating / 2.0).ToString(CultureInfo.CurrentCulture);
                        }
                    }
                    break;

                case Column.Finished:
                    e.Value = fm.FinishedOnUnknown ? Images.FinishedOnUnknown : Images.FMsList_FinishedOnIcons[fm.FinishedOn];
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

            SelectedFM? selFM = FMsDGV.RowSelected() ? FMsDGV.GetMainSelectedFMPosInfo() : null;

            var newSortDirection =
                e.ColumnIndex == (int)FMsDGV.CurrentSortedColumn && FMsDGV.CurrentSortDirection == SortDirection.Ascending
                    ? SortDirection.Descending
                    : SortDirection.Ascending;

            SortFMsDGV((Column)e.ColumnIndex, newSortDirection);

            Core.SetFilter();
            if (RefreshFMsList(selFM, keepSelection: KeepSel.TrueNearest, fromColumnClick: true))
            {
                if (selFM != null && FMsDGV.RowSelected() &&
                    selFM.InstalledName != FMsDGV.GetMainSelectedFM().InstalledDir)
                {
                    _displayedFM = await Core.DisplayFM();
                }
            }
        }

        private void FMsDGV_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right) return;

            var ht = FMsDGV.HitTest(e.X, e.Y);

            #region Right-click menu

            if (ht.Type is DataGridViewHitTestType.ColumnHeader or DataGridViewHitTestType.None)
            {
                FMsDGV.ContextMenuStrip = FMsDGV_ColumnHeaderLLMenu.Menu;
            }
            else if (ht.Type == DataGridViewHitTestType.Cell && ht.ColumnIndex > -1 && ht.RowIndex > -1)
            {
                FMsDGV.ContextMenuStrip = FMsDGV_FM_LLMenu.Menu;
                if (!FMsDGV.Rows[ht.RowIndex].Selected)
                {
                    FMsDGV.SelectSingle(ht.RowIndex);
                }
                // We don't need to call SelectProperly() here because the mousedown will select it properly
            }
            else
            {
                FMsDGV.ContextMenuStrip = null;
            }

            #endregion
        }

        private void SetTopRightTabsMultiSelectBlockerPanel(bool visible)
        {
            // Always make sure the blocker is covering up the enabled changed work, to prevent flicker of it
            if (visible)
            {
                TopRightMultiSelectBlockerPanel.Visible = true;
                if (!TopSplitContainer.FullScreen) TopRightTabControl.Enabled = false;
            }
            else
            {
                if (!TopSplitContainer.FullScreen) TopRightTabControl.Enabled = true;
                TopRightMultiSelectBlockerPanel.Visible = false;
            }
        }

        private bool SetTopRightBlockerVisible()
        {
            bool val = FMsDGV.MultipleFMsSelected();
            SetTopRightTabsMultiSelectBlockerPanel(val);
            return val;
        }

        private async void FMsDGV_SelectionChanged(object sender, EventArgs e)
        {
            // We don't need this because there's another check in ChangeSelection(), but we can avoid running
            // the async machinery with this.
            if (EventsDisabled) return;

            // Don't run selection logic for extra selected rows, to prevent a possible cascade of heavy operations
            // from being run during multi-select (scanning, caching, who knows what)
            if (SetTopRightBlockerVisible()) return;

            await ChangeSelection(FMsDGV.MainSelectedRow?.Index ?? -1);
        }

        private async Task ChangeSelection(int index = -1)
        {
            if (EventsDisabled) return;

            if (index > -1 && _displayedFM == GetFMFromIndex(index))
            {
                return;
            }

            if (!FMsDGV.RowSelected())
            {
                ClearShownData();
            }
            else
            {
                // @SEL_SYNC_HACK
                // Stupid hack to attempt to prevent multiselect-set-popping-back-to-starting-at-list-top
                if (index > -1)
                {
                    using (new DisableEvents(this))
                    {
                        FMsDGV.SelectSingle(index);
                        FMsDGV.SelectProperly();
                    }
                }

                FMsDGV.SelectProperly();

                _displayedFM = await Core.DisplayFM(index: index);
            }
        }

        private void FMsDGV_KeyDown(object sender, KeyEventArgs e)
        {
            // This is in here because it doesn't really work right if we put it in MainForm_KeyDown anyway
            if (e.KeyCode == Keys.Apps)
            {
                FMsDGV.ContextMenuStrip = FMsDGV_FM_LLMenu.Menu;
            }
        }

        private async void FMsDGV_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            FanMission fm;
            if (e.RowIndex < 0 || !FMsDGV.RowSelected() || !GameIsKnownAndSupported((fm = FMsDGV.GetMainSelectedFM()).Game))
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
                    ? (DataGridViewColumn)RatingImageColumn
                    : RatingTextColumn;

            if (!startup)
            {
                var oldRatingColumn = FMsDGV.Columns[(int)Column.Rating];
                newRatingColumn.Width = newRatingColumn == RatingTextColumn
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
                        if (newRatingColumn!.HeaderCell is DataGridViewColumnHeaderCellCustom cell)
                        {
                            cell.DarkModeEnabled = Config.DarkMode;
                        }
                    }
                    finally
                    {
                        CellValueNeededDisabled = false;
                    }
                }
                if (FMsDGV.CurrentSortedColumn == Column.Rating)
                {
                    FMsDGV.Columns[(int)Column.Rating].HeaderCell.SortGlyphDirection =
                        FMsDGV.CurrentSortDirection == SortDirection.Ascending
                            ? SortOrder.Ascending
                            : SortOrder.Descending;
                }
            }

            if (!startup)
            {
                FMsDGV.SetColumnData(FMsDGV_ColumnHeaderLLMenu, FMsDGV.GetColumnData());
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

                    Lazy_ToolStripLabels.Show(Lazy_ToolStripLabel.FilterByRating, from + " - " + to);
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

            var fm = FMsDGV.GetMainSelectedFM();
            fm.SelectedReadme = ChooseReadmeLLPanel.ListBox.SelectedBackingItem();
            ChooseReadmeLLPanel.ShowPanel(false);

            if (fm.SelectedReadme.ExtIsHtml())
            {
                ViewHTMLReadmeLLButton.Show();
            }
            else
            {
                SetReadmeVisible(true);
            }

            if (ChooseReadmeLLPanel.ListBox.Items.Count > 1)
            {
                ReadmeListFillAndSelect(ChooseReadmeLLPanel.ListBox.BackingItems, fm.SelectedReadme);
                ShowReadmeControls(CursorOverReadmeArea());
            }
            else
            {
                using (new DisableEvents(this))
                {
                    ChooseReadmeComboBox.ClearFullItems();
                }
                ChooseReadmeComboBox.Hide();
            }

            Core.LoadReadme(fm);
        }

        private void ChooseReadmeComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (EventsDisabled) return;

            var fm = FMsDGV.GetMainSelectedFM();
            fm.SelectedReadme = ChooseReadmeComboBox.SelectedBackingItem();
            Core.LoadReadme(fm);
        }

        private void ChooseReadmeComboBox_DropDownClosed(object sender, EventArgs e)
        {
            if (!CursorOverReadmeArea()) ShowReadmeControls(false);
        }

        #endregion

        internal void ReadmeEncodingMenuItems_Click(object sender, EventArgs e)
        {
            if (sender is not ToolStripMenuItemWithBackingField<int> menuItem) return;

            Core.ChangeEncodingForFMSelectedReadme(FMsDGV.GetMainSelectedFM(), menuItem.Field);
        }

        // Allows the readme controls to hide when the mouse moves directly from the readme area onto another
        // window. General-case showing and hiding is still handled by PreFilterMessage() for reliability.
        // Note: ChooseReadmePanel doesn't need this, because the readme controls aren't shown when it's visible.
        internal void ReadmeArea_MouseLeave(object sender, EventArgs e)
        {
            IntPtr hWnd = Native.WindowFromPoint(Cursor.Position);
            if (hWnd == IntPtr.Zero || Control.FromHandle(hWnd) == null) ShowReadmeControls(false);
        }

        [SuppressMessage("ReSharper", "MemberCanBeMadeStatic.Local")]
        private void ReadmeRichTextBox_LinkClicked(object sender, LinkClickedEventArgs e) => Core.OpenLink(e.LinkText, fixUpEmailLinks: true);

        private void ReadmeEncodingButton_Click(object sender, EventArgs e)
        {
            ShowMenu(EncodingsLLMenu.Menu, ReadmeEncodingButton, MenuPos.LeftDown);
        }

        public Encoding? ChangeReadmeEncoding(Encoding? encoding)
        {
            return ReadmeRichTextBox.ChangeEncoding(encoding);
        }

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
            foreach (DarkButton button in _readmeControlButtons)
            {
                button.BackColor = backColor;
            }
        }

        private void ShowReadmeControls(bool enabled)
        {
            ChooseReadmeComboBox.Visible = enabled && ChooseReadmeComboBox.Items.Count > 0;
            foreach (DarkButton button in _readmeControlButtons)
            {
                button.Visible = enabled;
            }
        }

        internal void ViewHTMLReadmeButton_Click(object sender, EventArgs e) => Core.ViewHTMLReadme(FMsDGV.GetMainSelectedFM());

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
                    InstallUninstallFMLLButton.Construct();
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
                await FMInstallAndPlay.InstallOrUninstall(GetSelectedFMs_InOrder());
            }
            else if (sender == PlayFMButton)
            {
                await FMInstallAndPlay.InstallIfNeededAndPlay(FMsDGV.GetMainSelectedFM());
            }
        }

        #region Play original game

        // @GENGAMES (Play original game controls): Begin

        public void SetPlayOriginalGameControlsState()
        {
            static bool AnyControlVisible()
            {
                for (int i = 0; i < SupportedGameCount; i++)
                {
                    // Check the backing data states rather than the controls' Visible properties, because those
                    // will be false if they're _physically_ not shown, even if the _logical_ state is set to
                    // "Visible = true"
                    if (!Config.GetGameExe((GameIndex)i).IsEmpty())
                    {
                        return true;
                    }
                }

                return false;
            }

            // We hide the separate-buttons flow layout panel when empty, because although its _interior_ has
            // zero width when empty, it has outside margin spacing that we want to get rid of when we're not
            // showing it.

            try
            {
                BottomPanel.SuspendDrawing();

                if (Config.PlayOriginalSeparateButtons)
                {
                    Lazy_PlayOriginalControls.SetMode(singleButton: false);
                    PlayOriginalFLP.Visible = AnyControlVisible();
                }
                else
                {
                    Lazy_PlayOriginalControls.SetMode(singleButton: true);
                    PlayOriginalFLP.Show();
                }
            }
            finally
            {
                BottomPanel.ResumeDrawing();
            }
        }

        // Because of the T2MP menu item breaking up the middle there, we can't array/index these menu items.
        // Just gonna have to leave this part as-is.
        internal void PlayOriginalGameButton_Click(object sender, EventArgs e)
        {
            PlayOriginalGameLLMenu.Construct();

            for (int i = 0; i < SupportedGameCount; i++)
            {
                PlayOriginalGameLLMenu.GameMenuItems[i].Enabled = !Config.GetGameExe((GameIndex)i).IsEmpty();
            }
            PlayOriginalGameLLMenu.Thief2MPMenuItem.Visible = Config.T2MPDetected;

            ShowMenu(PlayOriginalGameLLMenu.Menu, Lazy_PlayOriginalControls.ButtonSingle, MenuPos.TopRight);
        }

        [SuppressMessage("ReSharper", "MemberCanBeMadeStatic.Global")]
        internal void PlayOriginalGameMenuItem_Click(object sender, EventArgs e)
        {
            GameIndex gameIndex = ((ToolStripMenuItemCustom)sender).GameIndex;

            bool playMP = sender == PlayOriginalGameLLMenu.Thief2MPMenuItem;

            FMInstallAndPlay.PlayOriginalGame(gameIndex, playMP);
        }

        internal void PlayOriginalGameButtons_Click(object sender, EventArgs e)
        {
            FMInstallAndPlay.PlayOriginalGame(((DarkButton)sender).GameIndex);
        }

        internal void PlayOriginalT2MPButton_Click(object sender, EventArgs e)
        {
            ShowMenu(PlayOriginalT2InMultiplayerLLMenu.Menu, Lazy_PlayOriginalControls.T2MPMenuButton, MenuPos.TopRight);
        }

        internal void PlayT2InMultiplayerMenuItem_Click(object sender, EventArgs e)
        {
            FMInstallAndPlay.PlayOriginalGame(GameIndex.Thief2, playMP: true);
        }

        // @GENGAMES (Play original game controls): End

        #endregion

        #endregion

        private void WebSearchButton_Click(object sender, EventArgs e) => Core.OpenWebSearchUrl(FMsDGV.GetMainSelectedFM().Title);

        #endregion

        #region Right side

        internal async void Settings_Click(object sender, EventArgs e) => await Core.OpenSettings();

        public void ShowExitButton(bool enabled) => ExitLLButton.SetVisible(enabled);

        #endregion

        #endregion

        #region FM display

        // Perpetual TODO: Make sure this clears everything including the top right tab stuff
        private void ClearShownData()
        {
            // Hack to stop this being run when we clear selection for the purpose of selecting just one immediately
            // after.
            if (ZeroSelectCodeDisabled) return;

            #region Menus

            MainLLMenu.SetScanAllFMsMenuItemEnabled(FMsViewList.Count > 0);

            FMsDGV_FM_LLMenu.SetPlayFMInMPMenuItemVisible(false);
            FMsDGV_FM_LLMenu.SetPlayFMInMPMenuItemEnabled(false);
            FMsDGV_FM_LLMenu.SetPlayFMMenuItemEnabled(false);

            FMsDGV_FM_LLMenu.SetInstallUninstallMenuItemText(sayInstall: true, multiSelected: false);
            FMsDGV_FM_LLMenu.SetInstallUninstallMenuItemEnabled(false);

            FMsDGV_FM_LLMenu.SetPinOrUnpinMenuItemState(sayPin: true);

            FMsDGV_FM_LLMenu.SetDeleteFMMenuItemEnabled(false);

            FMsDGV_FM_LLMenu.SetOpenInDromEdMenuItemText(sayShockEd: false);
            FMsDGV_FM_LLMenu.SetOpenInDromEdVisible(false);
            FMsDGV_FM_LLMenu.SetOpenInDromedEnabled(false);

            FMsDGV_FM_LLMenu.SetOpenFMFolderVisible(false);

            FMsDGV_FM_LLMenu.SetScanFMMenuItemEnabled(false);

            FMsDGV_FM_LLMenu.SetConvertAudioRCSubMenuEnabled(false);

            #endregion

            #region Bottom bar

            InstallUninstallFMLLButton.SetSayInstall(true);
            InstallUninstallFMLLButton.SetEnabled(false);

            PlayFMButton.Enabled = false;

            WebSearchButton.Enabled = false;

            #endregion

            #region Readme area

            SetReadmeVisible(false);
            ReadmeRichTextBox.SetText("");

            ChooseReadmeLLPanel.ShowPanel(false);
            ViewHTMLReadmeLLButton.Hide();

            #endregion

            #region Stats tab

            BlankStatsPanelWithMessage(LText.StatisticsTab.NoFMSelected);
            StatsScanCustomResourcesButton.Hide();

            #endregion

            using (new DisableEvents(this))
            {
                #region Edit FM tab

                AltTitlesLLMenu.ClearItems();

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

                #endregion

                #region Comment tab

                CommentTextBox.Text = "";
                CommentTextBox.Enabled = false;

                #endregion

                #region Tags tab

                AddTagTextBox.Text = "";

                TagsTreeView.Nodes.Clear();

                foreach (Control c in TagsTabPage.Controls) c.Enabled = false;

                #endregion

                #region Patch tab

                ShowPatchSection(enable: false);

                #endregion

                #region Mods tab

                ModsCheckList.ClearList();
                ModsCheckList.Enabled = false;

                foreach (Control c in ModsTabPage.Controls)
                {
                    switch (c)
                    {
                        case TextBox tb:
                            tb.Text = "";
                            break;
                        case CheckBox chk:
                            chk.Checked = false;
                            break;
                    }

                    c.Enabled = false;
                }

                #endregion
            }

            _displayedFM = null;
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

        private void UpdateRatingMenus(int rating, bool disableEvents = false)
        {
            using (disableEvents ? new DisableEvents(this) : null)
            {
                FMsDGV_FM_LLMenu.SetRatingMenuItemChecked(rating);
                EditFMRatingComboBox.SelectedIndex = rating + 1;
            }
        }

        // @MULTISEL(Context menu sel state update): Since this runs always on selection change...
        // ... we might not need to call it on FM load.
        // NOTE(Context menu sel state update):
        // Keep this light and fast, because it gets called like 3 times every selection due to the @SEL_SYNC_HACK
        // for preventing "multi-select starts from top row even though our selection is not actually at the top
        // row"
        internal void UpdateUIControlsForMultiSelectState(FanMission fm)
        {
            var selRows = FMsDGV.SelectedRows;

            #region Get attributes that apply to all items

            // Crap-garbage code to loop through only once in case we have a large selection set

            bool allAreInstalled,
                noneAreInstalled,
                allAreDark,
                allAreAvailable,
                noneAreAvailable,
                allAreKnownAndSupported,
                allSelectedAreSameInstalledState,
                allAreSupportedAndAvailable;
            {
                int installedCount = 0,
                    markedUnavailableCount = 0,
                    gameIsDarkCount = 0,
                    knownAndSupportedCount = 0;

                int selRowsCount = selRows.Count;
                for (int i = 0; i < selRowsCount; i++)
                {
                    FanMission sFM = FMsDGV.GetFMFromIndex(selRows[i].Index);
                    if (sFM.Installed) installedCount++;
                    if (sFM.MarkedUnavailable) markedUnavailableCount++;
                    if (GameIsDark(sFM.Game)) gameIsDarkCount++;
                    if (GameIsKnownAndSupported(sFM.Game)) knownAndSupportedCount++;
                }

                allAreInstalled = installedCount == selRowsCount;
                noneAreInstalled = installedCount == 0;
                allAreDark = gameIsDarkCount == selRowsCount;
                allAreKnownAndSupported = knownAndSupportedCount == selRowsCount;
                allAreAvailable = markedUnavailableCount == 0;
                noneAreAvailable = markedUnavailableCount == selRowsCount;
                allSelectedAreSameInstalledState = allAreInstalled || noneAreInstalled;
                allAreSupportedAndAvailable = allAreKnownAndSupported && allAreAvailable;
            }

            #endregion

            bool multiSelected = selRows.Count > 1;

            // @MULTISEL(FM menu item toggles): Maybe we want to hide unsupported menu items rather than disable?
            FMsDGV_FM_LLMenu.SetPlayFMMenuItemEnabled(!multiSelected && allAreSupportedAndAvailable);

            FMsDGV_FM_LLMenu.SetPlayFMInMPMenuItemVisible(!multiSelected && fm.Game == Game.Thief2 && Config.T2MPDetected);
            FMsDGV_FM_LLMenu.SetPlayFMInMPMenuItemEnabled(!multiSelected && !fm.MarkedUnavailable);

            FMsDGV_FM_LLMenu.SetInstallUninstallMenuItemEnabled(allSelectedAreSameInstalledState && allAreSupportedAndAvailable);
            FMsDGV_FM_LLMenu.SetInstallUninstallMenuItemText(!fm.Installed, multiSelected);

            FMsDGV_FM_LLMenu.SetPinOrUnpinMenuItemState(!fm.Pinned);
            FMsDGV_FM_LLMenu.SetPinItemsMode();

            FMsDGV_FM_LLMenu.SetDeleteFMMenuItemEnabled(allAreAvailable);

            FMsDGV_FM_LLMenu.SetOpenInDromEdMenuItemText(sayShockEd: fm.Game == Game.SS2);
            FMsDGV_FM_LLMenu.SetOpenInDromEdVisible(!multiSelected && GameIsDark(fm.Game) && Config.GetGameEditorDetectedUnsafe(fm.Game));
            FMsDGV_FM_LLMenu.SetOpenInDromedEnabled(!multiSelected && !fm.MarkedUnavailable);

            FMsDGV_FM_LLMenu.SetOpenFMFolderVisible(!multiSelected && fm.Installed);

            FMsDGV_FM_LLMenu.SetScanFMMenuItemEnabled(!noneAreAvailable);
            FMsDGV_FM_LLMenu.SetScanFMText();

            FMsDGV_FM_LLMenu.SetConvertAudioRCSubMenuEnabled(allAreInstalled && allAreDark && allAreAvailable);

            FMsDGV_FM_LLMenu.SetGameSpecificFinishedOnMenuItemsText(fm.Game);

            FMsDGV_FM_LLMenu.SetWebSearchEnabled(!multiSelected);

            InstallUninstallFMLLButton.SetEnabled(allSelectedAreSameInstalledState && allAreSupportedAndAvailable);
            InstallUninstallFMLLButton.SetSayInstall(!fm.Installed);

            PlayFMButton.Enabled = !multiSelected && allAreSupportedAndAvailable;

            WebSearchButton.Enabled = !multiSelected;
        }

        // @GENGAMES: Lots of game-specific code in here, but I don't see much to be done about it.
        public void UpdateAllFMUIDataExceptReadme(FanMission fm)
        {
            bool fmIsT3 = fm.Game == Game.Thief3;

            #region Toggles

            // We should never get here when FMsList.Count == 0, but hey
            MainLLMenu.SetScanAllFMsMenuItemEnabled(FMsViewList.Count > 0);

            UpdateUIControlsForMultiSelectState(fm);

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

            FMsDGV_FM_LLMenu.SetFinishedOnMenuItemsChecked((Difficulty)fm.FinishedOn, fm.FinishedOnUnknown);

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

            using (new DisableEvents(this))
            {
                #region Edit FM tab

                EditFMTitleTextBox.Text = fm.Title;

                EditFMAltTitlesArrowButton.Enabled = fm.AltTitles.Count > 0;

                EditFMAuthorTextBox.Text = fm.Author;

                EditFMReleaseDateCheckBox.Checked = fm.ReleaseDate.DateTime != null;
                EditFMReleaseDateDateTimePicker.Value = fm.ReleaseDate.DateTime ?? DateTime.Now;
                EditFMReleaseDateDateTimePicker.Visible = fm.ReleaseDate.DateTime != null;

                EditFMLastPlayedCheckBox.Checked = fm.LastPlayed.DateTime != null;
                EditFMLastPlayedDateTimePicker.Value = fm.LastPlayed.DateTime ?? DateTime.Now;
                EditFMLastPlayedDateTimePicker.Visible = fm.LastPlayed.DateTime != null;

                UpdateRatingMenus(fm.Rating, disableEvents: false);

                Core.ScanAndFillLanguagesList(forceScan: false);

                #endregion

                #region Comment tab

                CommentTextBox.Text = fm.Comment.FromRNEscapes();

                #endregion

                #region Tags tab

                AddTagTextBox.Text = "";
                DisplayFMTags(fm.Tags);

                #endregion

                #region Patch tab

                if (GameIsDark(fm.Game) && fm.Installed)
                {
                    PatchMainPanel.Show();
                    PatchFMNotInstalledLabel.Hide();
                    try
                    {
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
                    }
                    finally
                    {
                        PatchDMLsListBox.EndUpdate();
                    }
                }

                #endregion

                // @VBL
                #region Mods tab

                ModsDisabledModsTextBox.Text = fm.DisabledMods;

                foreach (Control c in ModsTabPage.Controls)
                {
                    c.Enabled = true;
                }

                try
                {
                    ModsCheckList.SuspendDrawing();

                    // @Mods(Mods panel checkbox list): Make a control to handle the recycling/dark mode syncing of these
                    ModsCheckList.ClearList();

                    if (GameIsDark(fm.Game))
                    {
                        (Error error, List<Mod> mods) = GameConfigFiles.GetGameMods(fm);

                        if (error == Error.None)
                        {
                            var disabledModsList = fm.DisabledMods
                                .Split(CA_Plus, StringSplitOptions.RemoveEmptyEntries)
                                .ToHashSetI();

                            bool allDisabled = fm.DisableAllMods;

                            if (allDisabled) fm.DisabledMods = "";

                            for (int i = 0; i < mods.Count; i++)
                            {
                                Mod mod = mods[i];
                                if (mod.IsUber)
                                {
                                    mods.RemoveAt(i);
                                    mods.Add(mod);
                                }
                            }

                            var checkItems = new DarkCheckList.CheckItem[mods.Count];

                            for (int i = 0; i < mods.Count; i++)
                            {
                                Mod mod = mods[i];
                                checkItems[i] = new DarkCheckList.CheckItem(
                                    @checked: allDisabled ? mod.IsUber : !disabledModsList.Contains(mod.InternalName),
                                    text: mod.InternalName,
                                    caution: mod.IsUber);

                                if (allDisabled && !mod.IsUber)
                                {
                                    if (!fm.DisabledMods.IsEmpty()) fm.DisabledMods += "+";
                                    fm.DisabledMods += mod.InternalName;
                                }
                            }

                            if (allDisabled)
                            {
                                ModsDisabledModsTextBox.Text = fm.DisabledMods;
                                fm.DisableAllMods = false;
                            }

                            ModsCheckList.FillList(checkItems, LText.ModsTab.ImportantModsCaution);
                        }
                    }
                }
                finally
                {
                    ModsCheckList.ResumeDrawing();
                }

                #endregion
            }
        }

        public void ClearReadmesList()
        {
            using (new DisableEvents(this))
            {
                ChooseReadmeComboBox.ClearFullItems();
            }
        }

        public void ClearLanguagesList() => EditFMLanguageComboBox.ClearFullItems();

        public void AddLanguageToList(string backingItem, string item) => EditFMLanguageComboBox.AddFullItem(backingItem, item);

        public string? GetMainSelectedLanguage()
        {
            return EditFMLanguageComboBox.SelectedIndex == -1 ? null : EditFMLanguageComboBox.SelectedBackingItem();
        }

        /// <summary>
        /// Sets the selected item in the language list.
        /// </summary>
        /// <param name="language"></param>
        /// <returns>The selected backing string, or null if a match was not found.</returns>
        public string? SetSelectedLanguage(string language)
        {
            if (EditFMLanguageComboBox.Items.Count == 0)
            {
                return null;
            }
            else
            {
                int index = EditFMLanguageComboBox.BackingItems.FindIndex(x => x.EqualsI(language));
                EditFMLanguageComboBox.SelectedIndex = index == -1 ? 0 : index;
                return EditFMLanguageComboBox.SelectedBackingItem();
            }
        }

        public void ReadmeListFillAndSelect(List<string> readmeFiles, string readme)
        {
            using (new DisableEvents(this))
            {
                FillReadmeList(readmeFiles);
                ChooseReadmeComboBox.SelectBackingIndexOf(readme);
            }
        }

        public void SetReadmeState(ReadmeState state, List<string>? readmeFilesForChooser = null)
        {
            AssertR(state != ReadmeState.InitialReadmeChooser || readmeFilesForChooser != null,
                "tried to set readme state to " + nameof(ReadmeState.InitialReadmeChooser) + " but provided a null readme list");

            switch (state)
            {
                case ReadmeState.HTML:
                    ViewHTMLReadmeLLButton.Show();
                    SetReadmeVisible(false);
                    ReadmeEncodingButton.Enabled = false;
                    // In case the cursor is over the scroll bar area
                    if (CursorOverReadmeArea()) ShowReadmeControls(true);
                    break;
                case ReadmeState.PlainText:
                case ReadmeState.OtherSupported:
                    SetReadmeVisible(true);
                    ViewHTMLReadmeLLButton.Hide();
                    ReadmeEncodingButton.Enabled = state == ReadmeState.PlainText;
                    break;
                case ReadmeState.LoadError:
                    ChooseReadmeLLPanel.ShowPanel(false);
                    ChooseReadmeComboBox.Hide();
                    ViewHTMLReadmeLLButton.Hide();
                    SetReadmeVisible(true);
                    ReadmeEncodingButton.Enabled = false;
                    break;
                case ReadmeState.InitialReadmeChooser when readmeFilesForChooser != null:
                    SetReadmeVisible(false);
                    ViewHTMLReadmeLLButton.Hide();
                    FillReadmeList(readmeFilesForChooser, initialChooser: true);
                    ShowReadmeControls(false);
                    ChooseReadmeLLPanel.ShowPanel(true);
                    break;
            }
        }

        private void FillReadmeList(List<string> readmes, bool initialChooser = false)
        {
            try
            {
                IListControlWithBackingItems readmeListControl;
                if (initialChooser)
                {
                    ChooseReadmeLLPanel.ListBox.BeginUpdate();
                    ChooseReadmeLLPanel.ListBox.ClearFullItems();
                    readmeListControl = ChooseReadmeLLPanel.ListBox;
                }
                else
                {
                    readmeListControl = ChooseReadmeComboBox;
                }

                foreach (string f in readmes)
                {
                    // @DIRSEP: To backslashes for each file, to prevent selection misses.
                    // I thought I accounted for this with backslashing the selected readme, but they all need to be.
                    readmeListControl.AddFullItem(f.ToBackSlashes(), f.GetFileNameFast());
                }
            }
            finally
            {
                if (initialChooser)
                {
                    ChooseReadmeLLPanel.ListBox.EndUpdate();
                }
            }
        }

        public void ShowReadmeChooser(bool visible) => ChooseReadmeComboBox.Visible = visible;

        public void ShowInitialReadmeChooser(bool visible) => ChooseReadmeLLPanel.ShowPanel(visible);

        public Encoding? LoadReadmeContent(string path, ReadmeType fileType, Encoding? encoding)
        {
            return ReadmeRichTextBox.LoadContent(path, fileType, encoding);
        }

        public void SetReadmeText(string text) => ReadmeRichTextBox.SetText(text);

        public void SetSelectedEncoding(Encoding encoding) => EncodingsLLMenu.SetEncodingMenuItemChecked(encoding);

        private void FillAltTitlesMenu(List<string> fmAltTitles)
        {
            AltTitlesLLMenu.ClearItems();

            if (fmAltTitles.Count > 0)
            {
                var altTitlesMenuItems = new ToolStripItem[fmAltTitles.Count];
                for (int i = 0; i < fmAltTitles.Count; i++)
                {
                    var item = new ToolStripMenuItemCustom(fmAltTitles[i]);
                    item.Click += EditFMAltTitlesMenuItems_Click;
                    altTitlesMenuItems[i] = item;
                }

                AltTitlesLLMenu.Menu.Items.AddRange(altTitlesMenuItems);

                EditFMAltTitlesArrowButton.Enabled = true;
            }
        }

        public void DisplayFMTags(FMCategoriesCollection fmTags)
        {
            try
            {
                TagsTreeView.SuspendDrawing();
                TagsTreeView.Nodes.Clear();

                if (fmTags.Count == 0) return;

                ControlUtils.FillTreeViewFromTags_Sorted(TagsTreeView, fmTags);

                TagsTreeView.ExpandAll();
            }
            finally
            {
                TagsTreeView.ResumeDrawing();
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
            Pen s1Pen = Images.Sep1Pen;
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

        internal void PlayOriginalGameButton_Paint(object sender, PaintEventArgs e) => Images.PaintPlayOriginalButton(Lazy_PlayOriginalControls.ButtonSingle, e);

        internal void PlayOriginalGamesButtons_Paint(object sender, PaintEventArgs e)
        {
            DarkButton button = (DarkButton)sender;

            Image image = Images.GetPerGameImage(button.GameIndex).Primary.Large(button.Enabled);

            Images.PaintBitmapButton(button, e, image, x: 8);
        }

        [SuppressMessage("ReSharper", "MemberCanBeMadeStatic.Global")]
        internal void InstallUninstall_Play_Buttons_Paint(object sender, PaintEventArgs e)
        {
            DarkButton button = InstallUninstallFMLLButton.Button;
            bool enabled = button.Enabled;

            Images.PaintBitmapButton(
                button,
                e,
                InstallUninstallFMLLButton.SayInstallState
                    ? enabled ? Images.Install_24 : Images.GetDisabledImage(Images.Install_24)
                    : enabled ? Images.Uninstall_24 : Images.GetDisabledImage(Images.Uninstall_24),
                10);
        }

        private void SettingsButton_Paint(object sender, PaintEventArgs e) => Images.PaintBitmapButton(
            SettingsButton,
            e,
            SettingsButton.Enabled ? Images.Settings : Images.GetDisabledImage(Images.Settings),
            x: 10);

        private void PatchAddDMLButton_Paint(object sender, PaintEventArgs e) => Images.PaintPlusButton(PatchAddDMLButton, e);

        private void PatchRemoveDMLButton_Paint(object sender, PaintEventArgs e) => Images.PaintMinusButton(PatchRemoveDMLButton, e);

        private void TopRightMenuButton_Paint(object sender, PaintEventArgs e) => Images.PaintHamburgerMenuButton_TopRight(TopRightMenuButton, e);

        private void MainMenuButton_Paint(object sender, PaintEventArgs e) => Images.PaintHamburgerMenuButton24(MainMenuButton, e);

        private void WebSearchButton_Paint(object sender, PaintEventArgs e) => Images.PaintWebSearchButton(WebSearchButton, e);

        private void ReadmeFullScreenButton_Paint(object sender, PaintEventArgs e) => Images.PaintReadmeFullScreenButton(ReadmeFullScreenButton, e);

        private void ReadmeEncodingButton_Paint(object sender, PaintEventArgs e) => Images.PaintReadmeEncodingButton(ReadmeEncodingButton, e);

        private void ResetLayoutButton_Paint(object sender, PaintEventArgs e) => Images.PaintResetLayoutButton(ResetLayoutButton, e);

        private void ScanIconButtons_Paint(object sender, PaintEventArgs e) => Images.PaintScanSmallButtons((Button)sender, e);

        private void ZoomInButtons_Paint(object sender, PaintEventArgs e) => Images.PaintZoomButtons((Button)sender, e, Zoom.In);

        private void ZoomOutButtons_Paint(object sender, PaintEventArgs e) => Images.PaintZoomButtons((Button)sender, e, Zoom.Out);

        private void ZoomResetButtons_Paint(object sender, PaintEventArgs e) => Images.PaintZoomButtons((Button)sender, e, Zoom.Reset);

        #endregion

        private void EverythingPanel_DragEnter(object sender, DragEventArgs e)
        {
            if (EverythingPanel.Enabled &&
                e.Data.GetData(DataFormats.FileDrop) is string[] droppedItems &&
                Core.AtLeastOneDroppedFileValid(droppedItems))
            {
                e.Effect = DragDropEffects.Copy;
            }
        }

        private async void EverythingPanel_DragDrop(object sender, DragEventArgs e)
        {
            if (EverythingPanel.Enabled &&
                e.Data.GetData(DataFormats.FileDrop) is string[] droppedItems &&
                Core.AtLeastOneDroppedFileValid(droppedItems))
            {
                await AddFMs(droppedItems);
            }
        }

        public async Task<bool> AddFMs(string[] fmArchiveNames)
        {
            if (!EverythingPanel.Enabled) return false;

            try
            {
                EnableEverything(false);
                return await FMArchives.Add(this, fmArchiveNames.ToList());
            }
            finally
            {
                EnableEverything(true);
            }
        }

        public void ActivateThisInstance()
        {
            if (WindowState == FormWindowState.Minimized) WindowState = _nominalWindowState;
            Activate();
        }
    }
}
