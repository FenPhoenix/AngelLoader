﻿/* NOTE: MainForm notes:
-Don't lazy load the filter bar scroll buttons, as they screw the whole thing up (FMsDGV doesn't anchor
in its panel correctly, etc.). If we figure out how to solve this later, we can lazy load them then.

Things to lazy load:
-Top-right section in its entirety, and then individual tab pages (in case some are hidden), and then individual
 controls on each tab page (in case the tabs are visible but not selected on startup)
-Game buttons and game tabs (one or the other will be invisible on startup)

@NET5: Fonts will change and control sizes will all change too.
-.NET 6 seems to have an option to set the font to the old MS Sans Serif 8.25pt app-wide.
-If converting the whole app to Segoe UI, remember to change all MinimumSize (button min size "75,23" etc.)
IMPORTANT: Remember to change font-size-dependent DGV zoom feature to work correctly with the new font!

-The controls move positions because they're accounting for the scroll bar
but then when the scroll bar isn't there at runtime, their positions are wrong (too much margin on whatever side
the scroll bar was).

@X64: IntPtr will be 64-bit, so search for all places where we deal with them and make sure they all still work

@SEL_SYNC_HACK enlightenment:
There's the concept of "current row" (CurrentRow) and "current cell" (CurrentCell). CurrentRow is read-only (of
bloody course), but it can be set in a roundabout way by setting CurrentCell to a cell in the current row. BUT,
setting this causes the row to scroll into view (as documented) and possibly also some other goddamn visual garbage,
visible selection change/flickering etc.
It's conceptually much cleaner to use this method, but we would then have to hack around this infuriating unwanted
behavior that comes part and parcel with what should be a simple #$@!ing property flip.
Our current hack is nasty, but it does do what we want, is performant enough, and looks good to the user.

@MEM(WindowText): We can use control.SetStyle(ControlStyles.CacheText, true) to slash StringBuilder allocations
But this method is protected, so we either have to subclass every damn control and set it for each, or use
reflection during the control dictionary filler loop.
Despite greatly lowering StringBuilder allocations, this does not appear to improve performance to any noticeable
degree.
*/

//#define SAVE_NON_AERO_SNAPPED_BOUNDS
//#define HIDE_PERSONAL_CONTROLS

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using AL_Common;
using AngelLoader.DataClasses;
using AngelLoader.Forms.CustomControls;
using AngelLoader.Forms.CustomControls.LazyLoaded;
using AngelLoader.Forms.WinFormsNative;
using static AL_Common.Common;
using static AngelLoader.GameSupport;
using static AngelLoader.Global;
using static AngelLoader.Misc;
using static AngelLoader.Utils;

namespace AngelLoader.Forms
{
    public sealed partial class MainForm : DarkFormBase, IView, IMessageFilter
    {
        #region Private fields

        // Stupid hack for if event handlers need to know
        private bool _startupState = true;

        private ISplashScreen_Safe? _splashScreen;

        /// <summary>
        /// Any control that might need to know this can check it.
        /// </summary>
        internal bool AboutToClose;

        private FanMission? _displayedFM;

        #region Window size/location

        private FormWindowState _nominalWindowState;
        private Size _nominalWindowSize;
        private Point _nominalWindowLocation;

        #endregion

        #region FMs list

        private readonly float _fmsListDefaultFontSizeInPoints;
        private readonly int _fmsListDefaultRowHeight;

        // Set these beforehand and don't set autosize on any column! Or else it explodes everything because
        // FMsDGV tries to refresh when it shouldn't and all kinds of crap. Phew.
        private const int _ratingImageColumnWidth = 73;
        private const int _finishedColumnWidth = 91;

        #endregion

        #region Control arrays

        private readonly TabPage[] _gameTabs;
        private readonly ToolStripButtonCustom[] _filterByGameButtons;
        private readonly Lazy_TabsBase[] _topRightTabs;

        private readonly Control[] _filterLabels;
        private readonly ToolStripItem[] _filtersToolStripSeparatedItems;
        internal readonly Control?[] _bottomLeftAreaSeparatedItems;
        private readonly Control[] _bottomRightAreaSeparatedItems;

        private readonly Component[][] _hideableFilterControls;

        private readonly DarkButton[] _readmeControlButtons;

        #endregion

        #region Enums

        private enum KeepSel { False, True, TrueNearest }

        // IMPORTANT: Don't change the order of the first three, they're used as indices!
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
        public int EventsDisabledCount { get; set; }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool EventsDisabled => EventsDisabledCount > 0;

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool ZeroSelectCodeDisabled => ZeroSelectCodeDisabledCount > 0;

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int ZeroSelectCodeDisabledCount { get; set; }

        // Needed for Rating column swap to prevent a possible exception when CellValueNeeded is called in the
        // middle of the operation
        internal bool CellValueNeededDisabled;

        private TransparentPanel? ViewBlockingPanel;
        public bool ViewBlocked { get; private set; }

        #endregion

        #region Lazy-loaded controls

        private readonly IDarkable[] _lazyLoadedControls;

        private readonly ChooseReadmeLLPanel ChooseReadmeLLPanel;
        private readonly EncodingsLLMenu EncodingsLLMenu;
        private readonly ExitLLButton ExitLLButton;
        private readonly FilterControlsLLMenu FilterControlsLLMenu;
        private readonly FMsDGV_ColumnHeaderLLMenu FMsDGV_ColumnHeaderLLMenu;
        internal readonly FMsDGV_FM_LLMenu FMsDGV_FM_LLMenu;
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
        private readonly Lazy_RTFBoxMenu Lazy_RTFBoxMenu;
        private readonly Lazy_WebSearchButton Lazy_WebSearchButton;
        private readonly Lazy_TopRightBlocker Lazy_TopRightBlocker;

        #endregion

        // Cache visible state because calling Visible redoes the work even if the value is the same
        private bool _readmeControlsOtherThanComboBoxVisible;

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

            static bool TryGetHWndFromMousePos(Message msg, out IntPtr result, [NotNullWhen(true)] out Control? control)
            {
                var pos = new Point(Native.SignedLOWORD(msg.LParam), Native.SignedHIWORD(msg.LParam));
                result = Native.WindowFromPoint(pos);
                control = Control.FromHandle(result);
                return control != null;
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
                if (!TryGetHWndFromMousePos(m, out IntPtr hWnd, out Control? controlOver)) return PassMessageOn;

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
                    if (!(TagsTabPage.AddTagLLDropDownVisible() && TagsTabPage.CursorOverAddTagLLDropDown(fullArea: true)))
                    {
                        if (controlOver is DarkTextBox { Multiline: true })
                        {
                            Native.SendMessage(controlOver.Handle, m.Msg, m.WParam, m.LParam);
                        }
                        else if (CursorOverControl(TopSplitContainer.Panel2))
                        {
                            TopSplitContainer.Panel2.Focus();
                        }
                        else if (CursorOverControl(MainSplitContainer.Panel2) &&
                                 !ReadmeRichTextBox.Focused)
                        {
                            MainSplitContainer.Panel2.Focus();
                        }
                    }
                    if (controlOver is DarkComboBox { SuppressScrollWheelValueChange: true, Focused: false } cb)
                    {
                        if (cb.Parent is { IsHandleCreated: true })
                        {
                            Native.SendMessage(cb.Parent.Handle, m.Msg, m.WParam, m.LParam);
                        }
                        else
                        {
                            return BlockMessage;
                        }
                    }
                    else
                    {
                        Native.SendMessage(hWnd, m.Msg, m.WParam, m.LParam);
                    }
                }
                return BlockMessage;
            }
            else if (m.Msg == Native.WM_MOUSEHWHEEL)
            {
                if (!TryGetHWndFromMousePos(m, out _, out _)) return PassMessageOn;

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

                Control? control = Control.FromHandle(Native.WindowFromPoint(Native.GetCursorPosition_Fast()));
                if (control is ToolStripDropDown) return PassMessageOn;

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
                        Native.WM_LBUTTONDOWN or
                        Native.WM_MBUTTONDOWN or
                        Native.WM_RBUTTONDOWN))
                {
                    return BlockMessage;
                }
                else if (CursorOutsideAddTagsDropDownArea())
                {
                    TagsTabPage.HideAndClearAddTagLLDropDown();
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
            else if (m.Msg is Native.WM_SYSKEYDOWN or Native.WM_SYSKEYUP)
            {
                int wParam = (int)m.WParam;
                if (ModifierKeys == Keys.Alt && wParam == (int)Keys.F4)
                {
                    return PassMessageOn;
                }
                else if (ViewBlocked)
                {
                    return BlockMessage;
                }
            }
            // Any other keys have to use this.
            else if (m.Msg == Native.WM_KEYDOWN)
            {
                if (ViewBlocked && CanFocus) return BlockMessage;

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
                        AnyControlFocusedInTabPage(TagsTabPage) || TagsTabPage.AddTagLLDropDownFocused() ? HelpSections.TagsTab :
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
                if (ViewBlocked && CanFocus) return BlockMessage;
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
            // IMPORTANT! Init manual controls BEFORE component init!
            // Otherwise, we might get event handlers firing (looking at you, SizeChanged) right after the resume
            // layout calls in the component init methods, and referencing the manual controls. Never happened to
            // me, but one other user had this happen. I don't know why it doesn't happen for me on Win10 _or_ 7,
            // but whatever...
            #region Manual control construct + init

            #region Lazy-loaded controls

            _lazyLoadedControls = new IDarkable[]
            {
                ChooseReadmeLLPanel = new ChooseReadmeLLPanel(this),
                EncodingsLLMenu = new EncodingsLLMenu(this),
                ExitLLButton = new ExitLLButton(this),
                FilterControlsLLMenu = new FilterControlsLLMenu(this),
                FMsDGV_ColumnHeaderLLMenu = new FMsDGV_ColumnHeaderLLMenu(this),
                FMsDGV_FM_LLMenu = new FMsDGV_FM_LLMenu(this),
                GameFilterControlsLLMenu = new GameFilterControlsLLMenu(this),
                InstallUninstallFMLLButton = new InstallUninstallFMLLButton(this),
                Lazy_FMsListZoomButtons = new Lazy_FMsListZoomButtons(this),
                Lazy_PlayOriginalControls = new Lazy_PlayOriginalControls(this),
                Lazy_ToolStripLabels = new Lazy_ToolStripLabels(this),
                MainLLMenu = new MainLLMenu(this),
                PlayOriginalGameLLMenu = new PlayOriginalGameLLMenu(this),
                PlayOriginalT2InMultiplayerLLMenu = new PlayOriginalT2InMultiplayerLLMenu(this),
                TopRightLLMenu = new TopRightLLMenu(this),
                ViewHTMLReadmeLLButton = new ViewHTMLReadmeLLButton(this),
                Lazy_RTFBoxMenu = new Lazy_RTFBoxMenu(this),
                Lazy_WebSearchButton = new Lazy_WebSearchButton(this),
                Lazy_TopRightBlocker = new Lazy_TopRightBlocker(this)
            };

            #endregion

            // The other Rating column, there has to be two, one for text and one for images
            RatingImageColumn = new DataGridViewImageColumn
            {
                // IMPORTANT: Set this explicitly, otherwise we can end up with the following situation:
                // -We start up, rating column is set to text so this one hasn't been added yet, then we change
                //  to image rating column. This gets added and has its header cell replaced with a custom one,
                //  and does NOT have its text transferred over. It ends up with blank text.
                //  Note! The text column avoids this issue solely because it gets added in the component init
                //  method (therefore the OnColumnAdded() handler is run and it gets its header cell replaced
                //  immediately). If we changed that, we would have to add this to the rating text column too!
                HeaderCell = new DataGridViewColumnHeaderCellCustom(),
                ImageLayout = DataGridViewImageCellLayout.Zoom,
                ReadOnly = true,
                Width = _ratingImageColumnWidth,
                Resizable = DataGridViewTriState.False,
                SortMode = DataGridViewColumnSortMode.Programmatic
            };

            #endregion

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

            _fmsListDefaultFontSizeInPoints = FMsDGV.DefaultCellStyle.Font.SizeInPoints;
            _fmsListDefaultRowHeight = FMsDGV.RowTemplate.Height;

            _topRightTabs = new Lazy_TabsBase[]
            {
                StatisticsTabPage,
                EditFMTabPage,
                CommentTabPage,
                TagsTabPage,
                PatchTabPage,
                ModsTabPage
            };

            for (int i = 0; i < _topRightTabs.Length; i++)
            {
                _topRightTabs[i].SetOwner(this);
            }

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
#if !HIDE_PERSONAL_CONTROLS
            BottomRightFLP.Controls.Add(ForceWindowedCheckBox);
#endif
            ForceWindowedCheckBox.CheckedChanged += ForceWindowedCheckBox_CheckedChanged;

            T1ScreenShotModeCheckBox = new DarkCheckBox { AutoSize = true, Dock = DockStyle.Fill, Text = "T1 SSM" };
            T2ScreenShotModeCheckBox = new DarkCheckBox { AutoSize = true, Dock = DockStyle.Fill, Text = "T2 SSM" };
            // Add in reverse order because the flow layout panel is right-to-left I guess?
#if !HIDE_PERSONAL_CONTROLS
            BottomRightFLP.Controls.Add(T2ScreenShotModeCheckBox);
            BottomRightFLP.Controls.Add(T1ScreenShotModeCheckBox);
#endif
            T1ScreenShotModeCheckBox.CheckedChanged += T1ScreenShotModeCheckBox_CheckedChanged;
            T2ScreenShotModeCheckBox.CheckedChanged += T2ScreenShotModeCheckBox_CheckedChanged;
#endif

            #endregion

            #region Control arrays

            _gameTabs = new TabPage[SupportedGameCount];
            _filterByGameButtons = new ToolStripButtonCustom[SupportedGameCount];
            for (int i = 0; i < SupportedGameCount; i++)
            {
                #region Game tabs

                var tab = new DarkTabPageCustom
                {
                    TabIndex = i,
                    ImageIndex = i
                };
                _gameTabs[i] = tab;
                // Still add the tabs to the collection like before, even though we're going to clear and add
                // again, because I remember we had some kind of issue with the fonts if we didn't do that.
                // Quick testing doesn't show the issue now, but whatever, maybe it was only on Win 7?
                // Let's just keep it like before for safety.
                GamesTabControl.Controls.Add(tab);

                #endregion

                #region Game filter buttons

                var button = new ToolStripButtonCustom
                {
                    AutoSize = false,
                    CheckOnClick = true,
                    DisplayStyle = ToolStripItemDisplayStyle.Image,
                    Margin =
                        i == 0 ? new Padding(1, 0, 0, 0) :
                        i == SupportedGameCount - 1 ? new Padding(0, 0, 2, 0) :
                        new Padding(0),
                    Size = new Size(25, 25)
                };
                _filterByGameButtons[i] = button;
                button.Click += Async_EventHandler_Main;

                #endregion
            }

            FilterGameButtonsToolStrip.Items.AddRange(_filterByGameButtons.Cast<ToolStripItem>().ToArray());

            // Do this only after adding so they don't fire from the adds
            GamesTabControl.SelectedIndexChanged += GamesTabControl_SelectedIndexChanged;
            GamesTabControl.Deselecting += GamesTabControl_Deselecting;

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

            _bottomLeftAreaSeparatedItems = new Control?[]
            {
                // Lazy-loaded web search button will go into this on construct (terrible hack)
                null
            };

            _bottomRightAreaSeparatedItems = new Control[]
            {
                SettingsButton
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
        // @THREADING: On startup only, this is run in parallel with FindFMs.Find()
        // So don't touch anything the other touches: anything affecting preset tags or the FMs list.
        public void InitThreadable()
        {
#if RELEASE_BETA
            const string betaVer = "4";
            string title = "AngelLoader " + Application.ProductVersion + " beta " + betaVer;
#else
            string title = "AngelLoader " + Application.ProductVersion;
#endif
            if (Environment.Is64BitProcess) title += " (64-bit)";

            Text = title;

            #region Set up form and control state

            // Set here in init method so as to avoid the changes being visible.
            // Set here specifically (before anything else) so that splitter positioning etc. works right.
            SetWindowStateAndSize();

            GamesTabControl.SetTabsFull(_gameTabs);

            #region Top-right tabs

            AssertR(_topRightTabs.Length == TopRightTabsData.Count, nameof(_topRightTabs) + " length is different than enum length");

            var topRightTabsDict = new Dictionary<int, TabPage>();
            for (int i = 0; i < TopRightTabsData.Count; i++)
            {
                topRightTabsDict.Add(Config.TopRightTabsData.Tabs[i].DisplayIndex, _topRightTabs[i]);
            }

            var topRightTabs = new TabPage[TopRightTabsData.Count];
            for (int i = 0; i < TopRightTabsData.Count; i++)
            {
                topRightTabs[i] = topRightTabsDict[i];
            }

            TopRightTabControl.SetTabsFull(topRightTabs);

            for (int i = 0; i < TopRightTabsData.Count; i++)
            {
                TopRightTabControl.ShowTab(_topRightTabs[i], Config.TopRightTabsData.Tabs[i].Visible);
                TopRightLLMenu.SetItemChecked(i, Config.TopRightTabsData.Tabs[i].Visible);
            }

            // EnsureValidity() guarantees selected tab will not be invisible
            for (int i = 0; i < TopRightTabsData.Count; i++)
            {
                if ((int)Config.TopRightTabsData.SelectedTab == i)
                {
                    TopRightTabControl.SelectedTab = _topRightTabs[i];
                    break;
                }
            }

            #endregion

            #region SplitContainers

            MainSplitContainer.SetSplitterPercent(Config.MainSplitterPercent, setIfFullScreen: true, suspendResume: false);
            TopSplitContainer.SetSplitterPercent(Config.TopSplitterPercent, setIfFullScreen: false, suspendResume: false);

            MainSplitContainer.SetSibling(TopSplitContainer);
            MainSplitContainer.Panel1DarkBackColor = DarkColors.Fen_ControlBackground;
            MainSplitContainer.Panel2DarkBackColor = DarkColors.Fen_DarkBackground;
            TopSplitContainer.SetSibling(MainSplitContainer);
            TopSplitContainer.Panel1DarkBackColor = DarkColors.Fen_ControlBackground;
            TopSplitContainer.Panel2DarkBackColor = DarkColors.Fen_DarkBackground;

            #endregion

            #region FMs DataGridView

            FMsDGV.SetOwner(this);

            #region Columns

            FinishedColumn.Width = _finishedColumnWidth;

            UpdateRatingListsAndColumn(Config.RatingDisplayStyle == RatingDisplayStyle.FMSel, startup: true);

            FMsDGV.SetColumnData(FMsDGV_ColumnHeaderLLMenu, Config.Columns);

            #endregion

            #endregion

            #region Readme area

            ReadmeRichTextBox.SetOwner(this);

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

            SetPlayOriginalGameControlsState();

            if (!Config.HideUninstallButton) ShowInstallUninstallButton(true, startup: true);
            if (!Config.HideExitButton) ShowExitButton(true, startup: true);
            if (!Config.HideWebSearchButton) ShowWebSearchButton(true, startup: true);

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

            // Do this here to prevent double-loading of RTF/GLML readmes
            SetTheme(Config.VisualTheme, startup: true, createControlHandles: true);

#if !ReleaseBeta && !ReleasePublic
            UpdateGameScreenShotModes();
#endif
        }

        // This one can't be multithreaded because it depends on the FMs list
        // @VBL
        public async Task FinishInitAndShow(List<FanMission>? fmsViewListUnscanned, ISplashScreen_Safe splashScreen)
        {
            _splashScreen = splashScreen;

            if (Visible) return;

            // Sort the list here because InitThreadable() is run in parallel to FindFMs.Find() but sorting needs
            // Find() to have been run first.
            SortFMsDGV(Config.SortedColumn, Config.SortDirection);

            if (fmsViewListUnscanned?.Count > 0)
            {
                if (!Visible) Show();
                await FMScan.ScanNewFMs(fmsViewListUnscanned);
            }

            if (!Config.AskedToScanForMisCounts)
            {
                var fmsNeedingMisCountScan = new List<FanMission>();

                for (int i = 0; i < FMsViewList.Count; i++)
                {
                    FanMission fm = FMsViewList[i];
                    if (GameIsKnownAndSupported(fm.Game) && !fm.MarkedUnavailable && fm.MisCount == -1)
                    {
                        fmsNeedingMisCountScan.Add(fm);
                    }
                }

                if (fmsNeedingMisCountScan.Count > 0)
                {
                    if (!Visible) Show();
                    await FMScan.ScanFMs(
                        fmsNeedingMisCountScan,
                        FMScanner.ScanOptions.FalseDefault(scanMissionCount: true),
                        scanMessage: LText.ProgressBox.ScanningForMissionCounts
                    );
                }

                Config.AskedToScanForMisCounts = true;
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

        /*
        TODO: Fix multi-monitor stuff here
        -Should save/restore position no matter what monitor it's on.
        -If off the edge of the screen and there is NOT another monitor there, should reposition to be onscreen
         as usual.
        -If partly off one screen and partly on another, reposition to be on the screen its greatest part was on.
        -Handle vertical monitors.
        -UPDATE: It seems it comes up on whatever monitor you start its exe from. Don't know if this is
         DisplayFusion or stock Win10 behavior. But that's a semi-reasonable behavior although Notepad++
         overrides it because it's slick af.
        */
        private void SetWindowStateAndSize()
        {
            // Size MUST come first, otherwise it doesn't take (and then you have to put it in _Load, where it
            // can potentially be seen being changed)
            Size = Config.MainWindowSize;
            WindowState = WindowStateToFormWindowState(Config.MainWindowState);

            const int minVisible = 200;

            Point loc = Config.MainWindowLocation;
            Rectangle bounds = Screen.FromControl(this).Bounds;

            if (loc.X < bounds.Left - (Width - minVisible) || loc.X > bounds.Right - minVisible)
            {
                loc.X = Defaults.MainWindowLocation.X;
            }
            // Don't let it go any amount past the top of the screen, because that's where the title bar is
            if (loc.Y < bounds.Top || loc.Y > bounds.Bottom - minVisible)
            {
                loc.Y = Defaults.MainWindowLocation.Y;
            }

            Location = loc;

            _nominalWindowState = WindowStateToFormWindowState(Config.MainWindowState);
            _nominalWindowSize = Config.MainWindowSize;
            _nominalWindowLocation = loc;
        }

        public new void Show()
        {
            if (TopRightTabControl.SelectedTab is Lazy_TabsBase lazyTab && !Config.TopRightPanelCollapsed)
            {
                lazyTab.Construct();
            }
            TopRightTabControl.Selected += TopRightTabControl_Selected;

            base.Show();
            _splashScreen?.Hide();

            _startupState = false;
        }

        #endregion

        #region Form events

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // These have to go here because they depend on and/or affect the width of other controls, and we
            // need to be in a state where layout is happening
            ChangeFilterControlsForGameType();
            ShowFMsListZoomButtons(!Config.HideFMListZoomButtons);

            Application.AddMessageFilter(this);
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            // debug - end of startup - to make sure when we profile, we're measuring only startup time
#if RT_StartupOnly
            // Regular Environment.Exit() because we're testing speed
            Environment.Exit(1);
            return;
#endif
        }

        protected override void OnDeactivate(EventArgs e)
        {
            CancelResizables();
            base.OnDeactivate(e);
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            // Prevent unpleasant visual garbage drawing in the window on startup if we do the full thing
            if (_startupState)
            {
                if (WindowState != FormWindowState.Minimized)
                {
                    _nominalWindowState = WindowState;
                    if (WindowState != FormWindowState.Maximized)
                    {
                        _nominalWindowSize = Size;
                        _nominalWindowLocation = Location;
                    }
                }
            }
            else
            {
                if (WindowState != FormWindowState.Minimized)
                {
                    bool nominalWasMaximized = _nominalWindowState == FormWindowState.Maximized;

                    _nominalWindowState = WindowState;

                    if (WindowState != FormWindowState.Maximized)
                    {
                        /*
                        Native Win32 apps restore their un-Aero-Snapped size/position automatically, but WinForms
                        decides it's way better to save the snapped bounds as the new bounds. It does it completely
                        and entirely on purpose too. In fact, when you snap a window, maximize it, then restore it,
                        the window DOES go back to its un-snapped bounds briefly, but then WinForms puts it right
                        back to its snapped position in blatant defiance of the way every other kind of app works.
                        So... force it not to with some p/invoke and reflection crap.

                        This fixes:
                        -When snapping, maximizing, then restoring, we would end up with snapped bounds instead of
                         the last un-snapped bounds.
                        -Snapped position was saved to config file so on the next startup we'd end up with snapped
                         bounds. (but see notes below)

                        Some apps save the un-snapped bounds (and thus restore to un-snapped bounds on next start).
                        Notepad and Notepad++ both do; Discord, Firefox, Reaper, Agent Ransack, FileSeek don't...
                        It seems most apps don't.
                        We might conceivably have people who like AL to always come up snapped, and not doing so
                        might annoy them.
                        So let's disable it for now, to behave like most other apps.
                        */

#if !SAVE_NON_AERO_SNAPPED_BOUNDS
                        _nominalWindowSize = Size;
                        _nominalWindowLocation = Location;
#endif
                        if (Native.TryGetRealWindowBounds(this, out Rectangle rect))
                        {
                            var unsnappedLocation = new Point(rect.Left, rect.Top);
                            var unsnappedSize = new Size(rect.Width, rect.Height);

#if SAVE_NON_AERO_SNAPPED_BOUNDS
                            _nominalWindowLocation = unsnappedLocation;
                            _nominalWindowSize = unsnappedSize;
#endif

                            if (nominalWasMaximized)
                            {
                                ControlUtils.SetAeroSnapRestoreHackValues(this, unsnappedLocation, unsnappedSize);
                            }
                        }
#if SAVE_NON_AERO_SNAPPED_BOUNDS
                        else
                        {
                            _nominalWindowSize = Size;
                            _nominalWindowLocation = Location;
                        }
#endif
                    }
                }
            }

            if (TagsTabPage.AddTagLLDropDownVisible())
            {
                TagsTabPage.HideAndClearAddTagLLDropDown();
            }

            base.OnSizeChanged(e);
        }

        protected override void OnLocationChanged(EventArgs e)
        {
            if (WindowState == FormWindowState.Normal) _nominalWindowLocation = Location;
            base.OnLocationChanged(e);
        }

        private async void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
#if DEBUG || (Release_Testing && !RT_StartupOnly)
            if (e.Control && e.KeyCode == Keys.E)
            {
                SetUIEnabled(!EverythingPanel.Enabled);
                return;
            }
            // For separating log spam
            else if (e.Control && e.KeyCode == Keys.T)
            {
                System.Diagnostics.Trace.WriteLine("");
            }
#endif

            if (ViewBlocked) return;

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
                    FMsDGV.SelectProperly(suspendResume: !suspendResume);
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

                DataGridViewRow edgeRow = FMsDGV.Rows[home ? 0 : FMsDGV.RowCount - 1];
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
                                DataGridViewRow row = FMsDGV.Rows[i];
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
                                DataGridViewRow row = FMsDGV.Rows[i];
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

            // NOTE(FMsDGV nav): wontfix: Shift-selecting "backwards" (so items deselect back toward main selection)
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
                    DataGridViewRow firstRow = FMsDGV.Rows[0];
                    if (firstRow.Selected)
                    {
                        if (FMsDGV.MainSelectedRow != firstRow)
                        {
                            using (new DisableEvents(this, !e.Shift))
                            {
                                SelectAndSuppress(0, singleSelect: !e.Shift, selectionSyncHack: !e.Shift);
                            }
                        }
                        if (!e.Shift) await HandleHomeOrEnd(home: true, selectionSyncHack: true);
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
                    DataGridViewRow lastRow = FMsDGV.Rows[FMsDGV.RowCount - 1];
                    if (lastRow.Selected)
                    {
                        if (FMsDGV.MainSelectedRow != lastRow)
                        {
                            using (new DisableEvents(this, !e.Shift))
                            {
                                SelectAndSuppress(FMsDGV.RowCount - 1, singleSelect: !e.Shift, selectionSyncHack: !e.Shift);
                            }
                        }
                        if (!e.Shift) await HandleHomeOrEnd(home: false, selectionSyncHack: true);
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
                if (FMsDGV.Focused)
                {
                    if (Core.SelectedFMIsPlayable(out FanMission? fm))
                    {
                        await FMInstallAndPlay.InstallIfNeededAndPlay(fm, askConfIfRequired: true);
                    }
                    // Only suppress if FMsDGV focused otherwise it doesn't pass on to like textboxes or whatever
                    e.SuppressKeyPress = true;
                }
            }
            else if (e.KeyCode == Keys.Delete)
            {
                if (FMsDGV.Focused && FMsDGV.RowSelected())
                {
                    await FMDelete.HandleDelete();
                }
            }
            else if (e.KeyCode == Keys.Escape)
            {
                CancelResizables();

                TagsTabPage.HideAndClearAddTagLLDropDown();

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

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            // Extremely cheap and cheesy, but otherwise I have to figure out how to wait for a completely
            // separate and detached thread to complete. Argh. Threading sucks.
            if (!EverythingPanel.Enabled || ViewBlocked)
            {
                Core.Dialogs.ShowAlert(
                    LText.AlertMessages.AppClosing_OperationInProgress,
                    LText.AlertMessages.Alert);
                e.Cancel = true;
                return;
            }

            AboutToClose = true;

            Application.RemoveMessageFilter(this);

            UpdateConfig();

            base.OnFormClosing(e);
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            base.OnFormClosed(e);
            Core.Shutdown();
        }

        #endregion

        #region ISettingsChangeableWindow

        public void Localize() => Localize(startup: false);

        private void Localize(bool startup)
        {
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
                    _filterByGameButtons[i].ToolTipText = GetLocalizedGameName((GameIndex)i);
                }

                SetGameFilterShowHideMenuText();
                GameFilterControlsLLMenu.Localize();

                FilterTitleLabel.Text = LText.FilterBar.Title;
                FilterAuthorLabel.Text = LText.FilterBar.Author;

                FilterByReleaseDateButton.ToolTipText = LText.FilterBar.ReleaseDateToolTip;
                Lazy_ToolStripLabels.Localize(Lazy_FilterLabel.ReleaseDate);

                FilterByLastPlayedButton.ToolTipText = LText.FilterBar.LastPlayedToolTip;
                Lazy_ToolStripLabels.Localize(Lazy_FilterLabel.LastPlayed);

                FilterByTagsButton.ToolTipText = LText.FilterBar.TagsToolTip;
                FilterByFinishedButton.ToolTipText = LText.FilterBar.FinishedToolTip;
                FilterByUnfinishedButton.ToolTipText = LText.FilterBar.UnfinishedToolTip;

                FilterByRatingButton.ToolTipText = LText.FilterBar.RatingToolTip;
                Lazy_ToolStripLabels.Localize(Lazy_FilterLabel.Rating);
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
                MisCountColumn.HeaderText = LText.FMsList.MissionCountColumn;
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

                SetFMSelectedCountMessage(FMsDGV.GetRowSelectedCount());

                TopRightLLMenu.Localize();

                StatisticsTabPage.Text = LText.StatisticsTab.TabText;
                EditFMTabPage.Text = LText.EditFMTab.TabText;
                CommentTabPage.Text = LText.CommentTab.TabText;
                TagsTabPage.Text = LText.TagsTab.TabText;
                PatchTabPage.Text = LText.PatchTab.TabText;
                ModsTabPage.Text = LText.ModsTab.TabText;

                for (int i = 0; i < _topRightTabs.Length; i++)
                {
                    _topRightTabs[i].Localize();
                }

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

                if (ReadmeRichTextBox.LocalizableMessageType != ReadmeLocalizableMessage.None)
                {
                    SetReadmeLocalizableMessage(ReadmeRichTextBox.LocalizableMessageType);
                }

                Lazy_RTFBoxMenu.Localize();

                #endregion

                #region Bottom area

                PlayFMButton.Text = LText.Global.PlayFM;

                Lazy_PlayOriginalControls.LocalizeSingle();
                Lazy_PlayOriginalControls.LocalizeMulti();
                PlayOriginalGameLLMenu.Localize();
                PlayOriginalT2InMultiplayerLLMenu.Localize();

                InstallUninstallFMLLButton.Localize();

                Lazy_WebSearchButton.Localize();

                SetAvailableFMCount();
                SettingsButton.Text = LText.MainButtons.Settings;
                ExitLLButton.Localize();

                #endregion
            }
            finally
            {
                // This causes a refresh of everything and in turn updates the size column strings
                // We don't need to refresh on startup because we already will later
                if (!startup) EverythingPanel.ResumeDrawing();
            }
        }

        public void SetTheme(VisualTheme theme) => SetTheme(theme, startup: false);

        private void SetTheme(VisualTheme theme, bool startup, bool createControlHandles = false)
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
                    SetThemeBase(
                        theme,
                        x => x.EqualsIfNotNull(ProgressBox)
                             || (_progressBoxConstructed && x is Control xControl &&
                                 ProgressBox!.Controls.Contains(xControl))
                             || x is SplitterPanel,
                        createControlHandles: createControlHandles,
                        capacity: 150
                    );
                }

                SetReadmeButtonsBackColor(ReadmeRichTextBox.Visible, theme);

                // Set these first so other controls get the right data when they reference them
                Images.ReloadImageArrays();

                if (!startup) ControlUtils.RecreateAllToolTipHandles();

                if (!startup || darkMode)
                {
                    for (int i = 0; i < _lazyLoadedControls.Length; i++)
                    {
                        _lazyLoadedControls[i].DarkModeEnabled = darkMode;
                    }
                    SetProgressBoxDarkModeEnabled(darkMode);
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

                var gameTabImages = new Image[SupportedGameCount];
                for (int i = 0; i < SupportedGameCount; i++)
                {
                    GameIndex gameIndex = (GameIndex)i;
                    _filterByGameButtons[i].Image = Images.GetPerGameImage(gameIndex).Primary.Large();
                    gameTabImages[i] = Images.GetPerGameImage(gameIndex).Primary.Small();
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

        #region Main menu

        #region Menu button

        private void MainMenuButton_Click(object sender, EventArgs e)
        {
            ControlUtils.ShowMenu(MainLLMenu.Menu, MainMenuButton, ControlUtils.MenuPos.BottomRight, xOffset: 0, yOffset: 2);
        }

        private void MainMenuButton_Enter(object sender, EventArgs e) => MainMenuButton.HideFocusRectangle();

        #endregion

        #region Menu items

        internal void MainMenu_GameVersionsMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                Cursor = Cursors.WaitCursor;

                using var f = new GameVersionsForm();
                f.ShowDialogDark(this);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        internal void ViewHelpFileMenuItem_Click(object sender, EventArgs e) => Core.OpenHelpFile();

        internal void AboutMenuItem_Click(object sender, EventArgs e)
        {
            using var f = new AboutForm();
            f.ShowDialogDark(this);
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
                    GamesTabControl.SelectedTab = _gameTabs[(int)Config.GameTab];
                }
            }

            for (int i = 0; i < SupportedGameCount; i++)
            {
                GameIndex gameIndex = (GameIndex)i;
                Game game = GameIndexToGame(gameIndex);
                ToolStripButtonCustom button = _filterByGameButtons[i];
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
                _gameTabs[i].Text = useShort
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
                Rectangle lastGameTabsRect = GamesTabControl.GetTabRect(GamesTabControl.TabCount - 1);
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
                if (_gameTabs[i] == tabPage)
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
                _filterByGameButtons[i].Checked = gameSelFM == FMsDGV.GameTabsState.GetSelectedFM((GameIndex)i);
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

        public Game GetGameFiltersEnabled()
        {
            Game games = Game.Null;
            for (int i = 0; i < SupportedGameCount; i++)
            {
                if (_filterByGameButtons[i].Checked) games |= GameIndexToGame((GameIndex)i);
            }

            return games;
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
                    if (GamesTabControl.SelectedTab == _gameTabs[i])
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

                    GamesTabControl.SelectedTab = _gameTabs[index];
                }

                // Twice through, show first and then hide, to prevent the possibility of a temporary state of no
                // tabs in the list, thus setting selection to none and screwing us up
                for (int i = 0; i < SupportedGameCount; i++)
                {
                    bool visible = GameFilterControlsLLMenu.GetCheckedStates()[i];
                    if (visible) GamesTabControl.ShowTab(_gameTabs[i], true);
                }
                for (int i = 0; i < SupportedGameCount; i++)
                {
                    bool visible = GameFilterControlsLLMenu.GetCheckedStates()[i];
                    if (!visible) GamesTabControl.ShowTab(_gameTabs[i], false);
                }

                #endregion

                PositionFilterBarAfterTabs();

                FilterGameButtonsToolStrip.Hide();
                GamesTabControl.Show();
            }
            else // OneList
            {
                for (int i = 0; i < SupportedGameCount; i++)
                {
                    bool visible = GameFilterControlsLLMenu.GetCheckedStates()[i];
                    ToolStripButtonCustom button = _filterByGameButtons[i];
                    button.Visible = visible;
                    if (button.Checked && !visible) button.Checked = false;
                }

                GamesTabControl.Hide();
                // Don't inline this var - it stores the X value to persist it through a change
                int plusWidth = FilterBarFLP.Location.X - TopBarXZero();
                FilterBarFLP.Location = new Point(TopBarXZero(), FilterBarFLP.Location.Y);
                FilterBarFLP.Width += plusWidth;
                FilterGameButtonsToolStrip.Show();
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
                            _filterByGameButtons[i].Checked = false;
                        }
                    }

                    FilterTitleTextBox.Clear();
                    FilterAuthorTextBox.Clear();

                    FilterByReleaseDateButton.Checked = false;
                    Lazy_ToolStripLabels.Hide(Lazy_FilterLabel.ReleaseDate);

                    FilterByLastPlayedButton.Checked = false;
                    Lazy_ToolStripLabels.Hide(Lazy_FilterLabel.LastPlayed);

                    FilterByTagsButton.Checked = false;

                    FilterByFinishedButton.Checked = false;
                    FilterByUnfinishedButton.Checked = false;

                    FilterByRatingButton.Checked = false;
                    Lazy_ToolStripLabels.Hide(Lazy_FilterLabel.Rating);

                    // Here is the line where the internal filter is cleared. It does in fact happen!
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

                    FilterByReleaseDateButton.Checked = filter.ReleaseDateFrom != null || filter.ReleaseDateTo != null;
                    UpdateDateLabel(lastPlayed: false, suspendResume: false);

                    FilterByLastPlayedButton.Checked = filter.LastPlayedFrom != null || filter.LastPlayedTo != null;
                    UpdateDateLabel(lastPlayed: true, suspendResume: false);

                    FilterByTagsButton.Checked = !filter.Tags.IsEmpty();

                    FilterByFinishedButton.Checked = filter.Finished.HasFlagFast(FinishedState.Finished);
                    FilterByUnfinishedButton.Checked = filter.Finished.HasFlagFast(FinishedState.Unfinished);

                    FilterByRatingButton.Checked = !(filter.RatingFrom == -1 && filter.RatingTo == 10);
                    UpdateRatingLabel(suspendResume: false);
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
                Rectangle lastRect = GamesTabControl.GetTabRect(GamesTabControl.TabCount - 1);
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
                    f.Location = ControlUtils.ClampFormToScreenBounds(
                        parent: this,
                        form: f,
                        desiredLocation:
                        FilterBarFLP.PointToScreen_Fast(new Point(
                            FilterIconButtonsToolStrip.Location.X + button.Bounds.X,
                            FilterIconButtonsToolStrip.Location.Y + button.Bounds.Y + button.Height)));

                    if (f.ShowDialogDark(this) != DialogResult.OK) return;

                    FMsDGV.Filter.SetDateFromAndTo(lastPlayed, f.DateFrom, f.DateTo);

                    button.Checked = f.DateFrom != null || f.DateTo != null;
                }

                UpdateDateLabel(lastPlayed);
            }
            else if (sender == FilterByTagsButton)
            {
                using var tf = new FilterTagsForm(GlobalTags, FMsDGV.Filter.Tags);
                if (tf.ShowDialogDark(this) != DialogResult.OK) return;

                tf.TagsFilter.DeepCopyTo(FMsDGV.Filter.Tags);
                FilterByTagsButton.Checked = !FMsDGV.Filter.Tags.IsEmpty();
            }
            else if (sender == FilterByRatingButton)
            {
                bool outOfFive = Config.RatingDisplayStyle == RatingDisplayStyle.FMSel;
                using (var f = new FilterRatingForm(FMsDGV.Filter.RatingFrom, FMsDGV.Filter.RatingTo, outOfFive))
                {
                    f.Location = ControlUtils.ClampFormToScreenBounds(
                        parent: this,
                        form: f,
                        desiredLocation:
                        FilterBarFLP.PointToScreen_Fast(new Point(
                            FilterIconButtonsToolStrip.Location.X +
                            FilterByRatingButton.Bounds.X,
                            FilterIconButtonsToolStrip.Location.Y +
                            FilterByRatingButton.Bounds.Y +
                            FilterByRatingButton.Height)));

                    if (f.ShowDialogDark(this) != DialogResult.OK) return;
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
            ToolStripButtonCustom button = lastPlayed ? FilterByLastPlayedButton : FilterByReleaseDateButton;
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
                            ? Lazy_FilterLabel.LastPlayed
                            : Lazy_FilterLabel.ReleaseDate, from + " - " + to);
                }
                else
                {
                    Lazy_ToolStripLabels.Hide(
                        lastPlayed
                            ? Lazy_FilterLabel.LastPlayed
                            : Lazy_FilterLabel.ReleaseDate);
                }
            }
            finally
            {
                if (suspendResume) FilterBarFLP.ResumeDrawing();
            }
        }

        #region Filter bar right-hand controls

        internal void FMsListZoomButtons_Click(object sender, EventArgs e)
        {
            ZoomFMsDGVType zoomType = (ZoomFMsDGVType)Array.IndexOf(Lazy_FMsListZoomButtons.Buttons, (ToolStripButtonCustom)sender);
            ZoomFMsDGV(zoomType);
        }

        // A ton of things in one event handler to cut down on async/awaits
        internal async void Async_EventHandler_Main(object sender, EventArgs e)
        {
            if (sender == RefreshFromDiskButton)
            {
                await Core.RefreshFMsListFromDisk();
            }
            else if (sender == SettingsButton || sender.EqualsIfNotNull(MainLLMenu.SettingsMenuItem))
            {
                try
                {
                    Cursor = Cursors.WaitCursor;

                    await Core.OpenSettings();
                }
                finally
                {
                    Cursor = Cursors.Default;
                }
            }
            else if (sender.EqualsIfNotNull(InstallUninstallFMLLButton.Button))
            {
                await FMInstallAndPlay.InstallOrUninstall(GetSelectedFMs_InOrder());
            }
            else if (sender == PlayFMButton)
            {
                await FMInstallAndPlay.InstallIfNeededAndPlay(FMsDGV.GetMainSelectedFM());
            }
            else if (sender.EqualsIfNotNull(MainLLMenu.ScanAllFMsMenuItem))
            {
                await FMScan.ScanAllFMs();
            }
            else if (sender.EqualsIfNotNull(MainLLMenu.ImportFromDarkLoaderMenuItem) ||
                     sender.EqualsIfNotNull(MainLLMenu.ImportFromFMSelMenuItem) ||
                     sender.EqualsIfNotNull(MainLLMenu.ImportFromNewDarkLoaderMenuItem))
            {
                ImportType importType =
                    sender.EqualsIfNotNull(MainLLMenu.ImportFromDarkLoaderMenuItem) ? ImportType.DarkLoader :
                    sender.EqualsIfNotNull(MainLLMenu.ImportFromFMSelMenuItem) ? ImportType.FMSel :
                    ImportType.NewDarkLoader;

                await Import.ImportFrom(importType);
            }
            else if (sender.EqualsIfNotNull(EditFMTabPage.Sender_ScanForReadmes) ||
                     sender.EqualsIfNotNull(EditFMTabPage.Sender_ScanTitle) ||
                     sender.EqualsIfNotNull(EditFMTabPage.Sender_ScanAuthor) ||
                     sender.EqualsIfNotNull(EditFMTabPage.Sender_ScanReleaseDate) ||
                     sender.EqualsIfNotNull(StatisticsTabPage.Sender_ScanCustomResources))
            {
                if (sender.EqualsIfNotNull(EditFMTabPage.Sender_ScanForReadmes))
                {
                    try
                    {
                        Cursor = Cursors.WaitCursor;

                        Ini.WriteFullFMDataIni();
                        _displayedFM = await Core.DisplayFM(refreshCache: true);
                    }
                    finally
                    {
                        Cursor = Cursors.Default;
                    }
                }
                else
                {
                    try
                    {
                        FanMission fm = FMsDGV.GetMainSelectedFM();

                        // .7z FMs will have a progress box instead
                        if (!fm.Archive.ExtIs7z())
                        {
                            Cursor = Cursors.WaitCursor;
                        }

                        var scanOptions =
                            sender.EqualsIfNotNull(EditFMTabPage.Sender_ScanTitle) ? FMScanner.ScanOptions.FalseDefault(scanTitle: true) :
                            sender.EqualsIfNotNull(EditFMTabPage.Sender_ScanAuthor) ? FMScanner.ScanOptions.FalseDefault(scanAuthor: true) :
                            sender.EqualsIfNotNull(EditFMTabPage.Sender_ScanReleaseDate) ? FMScanner.ScanOptions.FalseDefault(scanReleaseDate: true) :
                            //sender.EqualsIfNotNull(StatisticsTabPage.Sender_ScanCustomResources)
                            FMScanner.ScanOptions.FalseDefault(scanCustomResources: true, scanMissionCount: true);

                        if (await FMScan.ScanFMs(new List<FanMission> { fm }, scanOptions, hideBoxIfZip: true))
                        {
                            RefreshFM(fm);
                        }
                    }
                    finally
                    {
                        Cursor = Cursors.Default;
                    }
                }
            }
            else
            {
                bool senderIsTextBox = sender == FilterTitleTextBox ||
                                       sender == FilterAuthorTextBox;
                bool senderIsGameButton = _filterByGameButtons.Contains(sender);

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
                bool keepSel = sender != FilterShowRecentAtTopButton &&
                               sender != FilterShowUnavailableButton &&
                               !senderIsTextBox;
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
            DarkArrowButton otherButton = senderButton == FilterBarScrollLeftButton
                ? FilterBarScrollRightButton
                : FilterBarScrollLeftButton;
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

            HScrollProperties hs = FilterBarFLP.HorizontalScroll;
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
            MainSplitContainer.ResetSplitterPercent(Defaults.MainSplitterPercent, setIfFullScreen: true);
            TopSplitContainer.ResetSplitterPercent(Defaults.TopSplitterPercent, setIfFullScreen: false);
            if (FilterBarScrollRightButton.Visible) SetFilterBarScrollButtons();
        }

        private void GameFilterControlsShowHideButton_Click(object sender, EventArgs e)
        {
            ControlUtils.ShowMenu(GameFilterControlsLLMenu.Menu,
                GameFilterControlsShowHideButtonToolStrip,
                ControlUtils.MenuPos.RightDown,
                -GameFilterControlsShowHideButton.Width,
                GameFilterControlsShowHideButton.Height);
        }

        [SuppressMessage("ReSharper", "MemberCanBeMadeStatic.Global")]
        internal async void GameFilterControlsMenuItems_Click(object sender, EventArgs e)
        {
            var s = (ToolStripMenuItemCustom)sender;

            if (!s.Checked && GameFilterControlsLLMenu.GetCheckedStates().All(static x => !x))
            {
                s.Checked = true;
                return;
            }

            if (Config.GameOrganization == GameOrganization.OneList)
            {
                ToolStripButtonCustom button = GetObjectFromMenuItem(
                    GameFilterControlsLLMenu.Menu,
                    s,
                    _filterByGameButtons,
                    SupportedGameCount);

                button.Visible = s.Checked;
                if (button.Checked && !s.Checked) button.Checked = false;

                // We have to refresh manually because Checked change doesn't trigger the refresh, only Click
                await SortAndSetFilter(keepSelection: true);
            }
            else // ByTab
            {
                TabPage tab = GetObjectFromMenuItem(
                    GameFilterControlsLLMenu.Menu,
                    s,
                    _gameTabs,
                    SupportedGameCount);

                // We don't need to do a manual refresh here because ShowTab will end up resulting in one
                GamesTabControl.ShowTab(tab, s.Checked);
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
            ControlUtils.ShowMenu(FilterControlsLLMenu.Menu,
                FilterIconButtonsToolStrip,
                ControlUtils.MenuPos.RightDown,
                -FilterControlsShowHideButton.Width,
                FilterIconButtonsToolStrip.Height);
        }

        internal async void FilterControlsMenuItems_Click(object sender, EventArgs e)
        {
            var s = (ToolStripMenuItemCustom)sender;

            try
            {
                FilterBarFLP.SuspendDrawing();

                Component[] filterItems = _hideableFilterControls[(int)s.Tag];
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
                                        Lazy_ToolStripLabels.Hide(Lazy_FilterLabel.ReleaseDate);
                                        break;
                                    case HideableFilterControls.LastPlayed:
                                        Lazy_ToolStripLabels.Hide(Lazy_FilterLabel.LastPlayed);
                                        break;
                                    case HideableFilterControls.Rating:
                                        Lazy_ToolStripLabels.Hide(Lazy_FilterLabel.Rating);
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
                RefreshFMsListRowsOnlyKeepSelection();
            }
        }

        public void RefreshMainSelectedFMRow_Fast()
        {
            if (FMsDGV.MainSelectedRow != null)
            {
                FMsDGV.InvalidateRow(FMsDGV.MainSelectedRow.Index);
            }
        }

        public void RefreshAllSelectedFMs_Full()
        {
            FanMission? selectedFM = GetMainSelectedFMOrNull();
            if (selectedFM != null) UpdateAllFMUIDataExceptReadme(selectedFM);
            RefreshFMsListRowsOnlyKeepSelection();
        }

        // Convenience method so I don't forget why I'm calling the full update method again
        public void RefreshAllSelectedFMs_UpdateInstallState()
        {
            // We need to update the Patch tab too, and if we ever have any other tab in the future that might
            // need updating
            RefreshAllSelectedFMs_Full();
        }

        public void RefreshFMsListRowsOnlyKeepSelection() => FMsDGV.Refresh();

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
                    // TODO(RefreshFMsList): We should put this in a try-finally
                    // But note a little harmless bug here: CellValueNeededDisabled does NOT get set back to
                    // false if we exit early because of no rows. That ends up working fine because CellValueNeeded
                    // won't be called if there are no rows anyway, and it gets set back to false whenever we do
                    // actually have more rows. So a try-finally would make it be set back to true always.
                    // Which would be "correct" but we'd just have to make sure it wouldn't break anything.
                    FMsDGV.SuspendDrawing();
                    // I think FMsDGV.SuspendDrawing() doesn't actually really work because it's a .NET control,
                    // not a direct wrapper around a Win32 one. So that's why we need this. It's possible we
                    // might not need this if we suspend/resume FMsDGV's parent control?
                    CellValueNeededDisabled = true;
                }

                try
                {
                    FMsDGV.SuppressSelectionEvent = true;

                    // Prevents:
                    // -a glitched row from being drawn at the end in certain situations
                    // -the subsequent row count set from being really slow
                    FMsDGV.Rows.Clear();

                    FMsDGV.RowCount = FMsDGV.FilterShownIndexList.Count;
                }
                finally
                {
                    FMsDGV.SuppressSelectionEvent = false;
                }

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
                        if (keepSelection != KeepSel.False)
                        {
                            // @TopLazy: Is this right? Ugh...
                            if (EventsDisabledCount == 0) EventsDisabledCount++;
                            // Old line
                            //EventsDisabled = true;
                        }
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
                        HashSet<FanMission> hash = multiSelectedFMs.ToHashSet();
                        DataGridViewRow? latestRow = null;
                        try
                        {
                            FMsDGV.SuppressSelectionEvent = true;

                            for (int i = 0; i < FMsDGV.FilterShownIndexList.Count; i++)
                            {
                                if (hash.Contains(FMsDGV.GetFMFromIndex(i)))
                                {
                                    latestRow = FMsDGV.Rows[i];
                                    latestRow.Selected = true;
                                }
                            }

                            // Match original behavior
                            if (latestRow != null) FMsDGV.MainSelectedRow = latestRow;
                        }
                        finally
                        {
                            FMsDGV.SuppressSelectionEvent = false;
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

        #endregion

        #region Top-right area

        private void TopRightTabControl_Selected(object sender, TabControlEventArgs e)
        {
            if (e.Action == TabControlAction.Selected && e.TabPage is Lazy_TabsBase lazyTab)
            {
                lazyTab.Construct();
            }
        }

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
                if (!Lazy_TopRightBlocker.Visible)
                {
                    TopRightTabControl.Enabled = true;
                }
            }

            TopRightCollapseButton.ArrowDirection = collapsed ? Direction.Left : Direction.Right;

            if (!collapsed && TopRightTabControl.SelectedTab is Lazy_TabsBase lazyTab)
            {
                lazyTab.ConstructWithSuspendResume();
            }
        }

        private void TopRightMenuButton_Click(object sender, EventArgs e)
        {
            ControlUtils.ShowMenu(TopRightLLMenu.Menu, TopRightMenuButton, ControlUtils.MenuPos.BottomLeft);
        }

        internal void TopRightMenu_MenuItems_Click(object sender, EventArgs e)
        {
            var s = (ToolStripMenuItemCustom)sender;

            TabPage tab = GetObjectFromMenuItem(TopRightLLMenu.Menu, s, _topRightTabs, TopRightTabsData.Count);

            if (!s.Checked && TopRightTabControl.TabCount == 1)
            {
                s.Checked = true;
                return;
            }

            TopRightTabControl.ShowTab(tab, s.Checked);
        }

        #endregion

        #region FMs list

        public FanMission? GetMainSelectedFMOrNull() => FMsDGV.RowSelected() ? FMsDGV.GetMainSelectedFM() : null;

        public FanMission? GetMainSelectedFMOrNull_Fast() => _displayedFM;

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

        public List<FanMission> GetSelectedFMs_InOrder_List() => FMsDGV.GetSelectedFMs_InOrder_List();

        public FanMission? GetFMFromIndex(int index) => FMsDGV.RowSelected() ? FMsDGV.GetFMFromIndex(index) : null;

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
                // So suspend FMsDGV's containing control instead and it works fine.
                TopSplitContainer.Panel1.SuspendDrawing();
                // Must suppress this or our context menu becomes wrong
                FMsDGV.SuppressSelectionEvent = true;

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
                var newF = new Font(f.FontFamily, fontSize, f.Style, f.Unit, f.GdiCharSet, f.GdiVerticalFont);

                // Set row height based on font plus some padding
                int rowHeight = type == ZoomFMsDGVType.ResetZoom ? _fmsListDefaultRowHeight : newF.Height + 9;

                // If we're on startup, then the widths will already have been restored (to zoomed size) from the
                // config
                bool heightOnly = type == ZoomFMsDGVType.ZoomToHeightOnly;

                // Must be done first, else we get wrong values
                var widthMul = new List<double>(ColumnsCount);
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
                int mainRowIndex = FMsDGV.MainSelectedRow?.Index ?? -1;
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
                        // Must come first because it deselects all other rows (sometimes?!)
                        if (mainRowIndex > -1)
                        {
                            try
                            {
                                FMsDGV.CurrentCell = FMsDGV.Rows[mainRowIndex].Cells[FMsDGV.FirstDisplayedCell?.ColumnIndex ?? 0];
                            }
                            catch
                            {
                                // ignore
                            }
                        }
                        for (int i = selIndices.Length - 1; i >= 0; i--)
                        {
                            // DON'T suppress the event, because if we do it'll resume right here again, but we
                            // already are suppressing it manually and we only want to resume at the end in the
                            // finally block down below
                            FMsDGV.SetRowSelected(selIndices[i], true, suppressEvent: false);
                        }

                        FMsDGV.SelectProperly(suspendResume: false);
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
                if (selIndices.Length > 0 && selFM != null)
                {
                    if (mainRowIndex > -1) FMsDGV.MainSelectedRow = FMsDGV.Rows[mainRowIndex];
                    CenterSelectedFM();
                }
            }
            finally
            {
                FMsDGV.SuppressSelectionEvent = false;
                TopSplitContainer.Panel1.ResumeDrawing();
            }
        }

        private void CenterSelectedFM()
        {
            try
            {
                // For this codepath only, we want to refresh instead of invalidate because invalidate leaves
                // the scroll bar in the previous position instead of the new position.
                FMsDGV.RefreshOnScrollHack = true;

                FMsDGV.FirstDisplayedScrollingRowIndex =
                    (FMsDGV.MainSelectedRow!.Index - (FMsDGV.DisplayedRowCount(true) / 2))
                    .Clamp(0, FMsDGV.RowCount - 1);
            }
            catch
            {
                // no room is available to display rows
            }
            finally
            {
                FMsDGV.RefreshOnScrollHack = false;
            }
        }

        public void SetPinnedMenuState(bool pinned) => FMsDGV_FM_LLMenu.SetPinOrUnpinMenuItemState(!pinned);

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

            var filterMatches = Core.SetFilter();

            // @VBL egregious
            if (landImmediate && FMsDGV.FilterShownIndexList.Count > 0)
            {
                bool foundUnTopped = false;
                if (Config.EnableFuzzySearch &&
                    (filterMatches.TitleExactMatch != null || filterMatches.AuthorExactMatch != null))
                {
                    // PERF_TODO(SortAndSetFilter()):
                    // We're looping through when we already have our FanMission object, but what we need is a
                    // SelectedFM object, but that requires getting the scroll position and such, so we can't
                    // just convert a FanMission to a SelectedFM. These loops are super stupid and wasteful but
                    // I can't think how to not have them at the moment. Look into this at some point.

                    if (filterMatches.TitleExactMatch != null)
                    {
                        for (int i = 0; i < FMsDGV.FilterShownIndexList.Count; i++)
                        {
                            FanMission fm = FMsDGV.GetFMFromIndex(i);
                            if (!fm.MarkedRecent && !fm.Pinned && fm == filterMatches.TitleExactMatch)
                            {
                                selectedFM = FMsDGV.GetFMPosInfoFromIndex(i);
                                keepSel = KeepSel.True;
                                foundUnTopped = true;
                                break;
                            }
                        }
                    }
                    if (filterMatches.AuthorExactMatch != null)
                    {
                        for (int i = 0; i < FMsDGV.FilterShownIndexList.Count; i++)
                        {
                            FanMission fm = FMsDGV.GetFMFromIndex(i);
                            if (!fm.MarkedRecent && !fm.Pinned && fm == filterMatches.AuthorExactMatch)
                            {
                                selectedFM = FMsDGV.GetFMPosInfoFromIndex(i);
                                keepSel = KeepSel.True;
                                foundUnTopped = true;
                                break;
                            }
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < FMsDGV.FilterShownIndexList.Count; i++)
                    {
                        FanMission fm = FMsDGV.GetFMFromIndex(i);
                        if (!fm.MarkedRecent && !fm.Pinned)
                        {
                            selectedFM = FMsDGV.GetFMPosInfoFromIndex(i);
                            keepSel = KeepSel.True;
                            foundUnTopped = true;
                            break;
                        }
                    }
                }

                bool foundExactTitleMatch = false;
                bool foundExactAuthorMatch = false;

                int titleMatchIndex = -1;
                int authorMatchIndex = -1;

                if (!foundUnTopped)
                {
                    string titleText = FilterTitleTextBox.Text;
                    string titleTextTrimmed = titleText.Trim();
                    string authorText = FilterAuthorTextBox.Text;

                    bool titleIsWhiteSpace = titleText.IsWhiteSpace();
                    bool authorIsWhiteSpace = authorText.IsWhiteSpace();

                    for (int i = 0; i < FMsDGV.FilterShownIndexList.Count; i++)
                    {
                        FanMission fm = FMsDGV.GetFMFromIndex(i);

                        if (!fm.MarkedRecent && !fm.Pinned)
                        {
                            break;
                        }

                        if (Config.EnableFuzzySearch)
                        {
                            if (!titleIsWhiteSpace)
                            {
                                (bool matched, bool exactMatch) = Core.FMTitleContains_AllTests(fm, titleText, titleTextTrimmed);
                                if (matched)
                                {
                                    titleMatchIndex = i;
                                    if (exactMatch)
                                    {
                                        selectedFM = FMsDGV.GetFMPosInfoFromIndex(i);
                                        keepSel = KeepSel.True;
                                        foundExactTitleMatch = true;
                                        break;
                                    }
                                }
                            }
                            if (!authorIsWhiteSpace)
                            {
                                (bool matched, bool exactMatch) = fm.Author.ContainsI_TextFilter(authorText);
                                if (matched)
                                {
                                    authorMatchIndex = i;
                                    if (exactMatch)
                                    {
                                        selectedFM = FMsDGV.GetFMPosInfoFromIndex(i);
                                        keepSel = KeepSel.True;
                                        foundExactAuthorMatch = true;
                                        break;
                                    }
                                }
                            }
                        }
                        else
                        {
                            if ((!titleIsWhiteSpace && Core.FMTitleContains_AllTests(fm, titleText, titleTextTrimmed).Matched) ||
                                (!authorIsWhiteSpace && fm.Author.ContainsI_TextFilter(authorText).Matched))
                            {
                                selectedFM = FMsDGV.GetFMPosInfoFromIndex(i);
                                keepSel = KeepSel.True;
                                break;
                            }
                        }
                    }

                    if (Config.EnableFuzzySearch)
                    {
                        if (!foundExactTitleMatch && titleMatchIndex > -1)
                        {
                            selectedFM = FMsDGV.GetFMPosInfoFromIndex(titleMatchIndex);
                            keepSel = KeepSel.True;
                        }
                        if (!foundExactAuthorMatch && authorMatchIndex > -1)
                        {
                            selectedFM = FMsDGV.GetFMPosInfoFromIndex(authorMatchIndex);
                            keepSel = KeepSel.True;
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

        // @MEM(CellValueNeeded): ehhh, you know...
        private void FMsDGV_CellValueNeeded(object sender, DataGridViewCellValueEventArgs e)
        {
            if (CellValueNeededDisabled) return;

            if (FMsDGV.FilterShownIndexList.Count == 0) return;

            FanMission fm = FMsDGV.GetFMFromIndex(e.RowIndex);

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

            bool fmShouldBePinned = fm.Pinned && !GetShowUnavailableFMsFilter();

            switch ((Column)e.ColumnIndex)
            {
                case Column.Game:
                    e.Value =
                        fm.Game.ConvertsToKnownAndSupported(out GameIndex gameIndex) ? Images.FMsList_GameIcons[(int)gameIndex] :
                        fm.Game == Game.Unsupported ? Images.RedQCircle :
                        // Can't say null, or else it sets an ugly red-x image
                        Images.Blank;
                    break;

                case Column.Installed:
                    e.Value = fm.Installed ? Images.GreenCheckCircle : Images.Blank;
                    break;

                case Column.MissionCount:
                    e.Value = fm.MisCount > 0 ? fm.MisCount.ToString(CultureInfo.CurrentCulture) : "";
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

                    if (TitleColumn.Visible && fmShouldBePinned)
                    {
                        finalTitle = pinChar + finalTitle;
                    }

                    e.Value = finalTitle;

                    break;

                case Column.Archive:
                    e.Value = !TitleColumn.Visible && ArchiveColumn.Visible && fmShouldBePinned
                        ? pinChar + fm.Archive
                        : fm.Archive;
                    break;

                case Column.Author:
                    e.Value = !TitleColumn.Visible && !ArchiveColumn.Visible && AuthorColumn.Visible && fmShouldBePinned
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

            SortDirection newSortDirection =
                e.ColumnIndex == (int)FMsDGV.CurrentSortedColumn
                    ? FMsDGV.CurrentSortDirection == SortDirection.Ascending
                        ? SortDirection.Descending
                        : SortDirection.Ascending
                    : ColumnDefaultSortDirections[e.ColumnIndex];

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

            DataGridView.HitTestInfo ht = FMsDGV.HitTest(e.X, e.Y);

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

        private void SetTopRightBlockerVisible()
        {
            // Always make sure the blocker is covering up the enabled changed work, to prevent flicker of it
            if (FMsDGV.MultipleFMsSelected())
            {
                Lazy_TopRightBlocker.Visible = true;
                if (!TopSplitContainer.FullScreen) TopRightTabControl.Enabled = false;
            }
            else
            {
                if (!TopSplitContainer.FullScreen) TopRightTabControl.Enabled = true;
                Lazy_TopRightBlocker.Visible = false;
            }
        }

        private async void FMsDGV_SelectionChanged(object sender, EventArgs e)
        {
            if (FMsDGV.SuppressSelectionEvent) return;

            if (EventsDisabled) return;

            // Don't run selection logic for extra selected rows, to prevent a possible cascade of heavy operations
            // from being run during multi-select (scanning, caching, who knows what)
            if (FMsDGV.MultipleFMsSelected()) return;

            int index = FMsDGV.MainSelectedRow?.Index ?? -1;

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
                else
                {
                    FMsDGV.SelectProperly();
                }

                _displayedFM = await Core.DisplayFM(index: index);
            }
        }

        private void FMsDGV_KeyDown(object sender, KeyEventArgs e)
        {
            if (ControlUtils.IsMenuKey(e))
            {
                FMsDGV.ContextMenuStrip = FMsDGV_FM_LLMenu.Menu;
            }
        }

        private async void FMsDGV_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex > -1 && Core.SelectedFMIsPlayable(out FanMission? fm))
            {
                await FMInstallAndPlay.InstallIfNeededAndPlay(fm, askConfIfRequired: true);
            }
        }

        #endregion

        public bool MultipleFMsSelected() => FMsDGV.MultipleFMsSelected();

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

            EditFMTabPage.UpdateRatingStrings(fmSelStyle);

            FMsDGV_FM_LLMenu.UpdateRatingList(fmSelStyle);

            #endregion

            #region Update rating column

            DataGridViewColumn newRatingColumn =
                Config.RatingDisplayStyle == RatingDisplayStyle.FMSel && Config.RatingUseStars
                    ? (DataGridViewColumn)RatingImageColumn
                    : RatingTextColumn;

            if (!startup)
            {
                DataGridViewColumn oldRatingColumn = FMsDGV.Columns[(int)Column.Rating];
                newRatingColumn.Width = newRatingColumn == RatingTextColumn
                    ? oldRatingColumn.Width
                    // To set the ratio back to exact on zoom reset
                    : FMsDGV.RowTemplate.Height == _fmsListDefaultRowHeight
                        ? _ratingImageColumnWidth
                        : (FMsDGV.DefaultCellStyle.Font.Height + 9) * (_ratingImageColumnWidth / _fmsListDefaultRowHeight);
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
                RefreshFMsListRowsOnlyKeepSelection();
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
                    CultureInfo currentCulture = CultureInfo.CurrentCulture;

                    string from = rFrom == -1 ? LText.Global.None : (ndl ? rFrom : rFrom / 2.0).ToString(currentCulture);
                    string to = rTo == -1 ? LText.Global.None : (ndl ? rTo : rTo / 2.0).ToString(currentCulture);

                    Lazy_ToolStripLabels.Show(Lazy_FilterLabel.Rating, from + " - " + to);
                }
                else
                {
                    Lazy_ToolStripLabels.Hide(Lazy_FilterLabel.Rating);
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

            int itemCount = ChooseReadmeLLPanel.ListBox.Items.Count;
            if (itemCount == 0 || ChooseReadmeLLPanel.ListBox.SelectedIndex == -1)
            {
                return;
            }

            FanMission fm = FMsDGV.GetMainSelectedFM();
            fm.SelectedReadme = ChooseReadmeLLPanel.ListBox.SelectedBackingItem();
            ChooseReadmeLLPanel.ShowPanel(false);

            List<string> list = itemCount > 1
                ? ChooseReadmeLLPanel.ListBox.BackingItems
                : new List<string>();

            ReadmeListFillAndSelect(list, fm.SelectedReadme);
            ShowReadmeControls(CursorOverReadmeArea());

            ChooseReadmeLLPanel.ListBox.ClearFullItems();

            Core.LoadReadme(fm);
        }

        private void ChooseReadmeComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (EventsDisabled) return;

            FanMission fm = FMsDGV.GetMainSelectedFM();
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
            IntPtr hWnd = Native.WindowFromPoint(Native.GetCursorPosition_Fast());
            if (Control.FromHandle(hWnd) == null) ShowReadmeControls(false);
        }

        [SuppressMessage("ReSharper", "MemberCanBeMadeStatic.Local")]
        private void ReadmeRichTextBox_LinkClicked(object sender, LinkClickedEventArgs e) => Core.OpenLink(e.LinkText, fixUpEmailLinks: true);

        private void ReadmeRichTextBox_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                ReadmeRichTextBox.ContextMenuStrip ??= Lazy_RTFBoxMenu.Menu;
            }
        }

        private void ReadmeRichTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (ControlUtils.IsMenuKey(e))
            {
                ReadmeRichTextBox.ContextMenuStrip ??= Lazy_RTFBoxMenu.Menu;
            }
        }

        private void ReadmeEncodingButton_Click(object sender, EventArgs e)
        {
            ControlUtils.ShowMenu(EncodingsLLMenu.Menu, ReadmeEncodingButton, ControlUtils.MenuPos.LeftDown);
        }

        public Encoding? ChangeReadmeEncoding(Encoding? encoding) => ReadmeRichTextBox.ChangeEncoding(encoding);

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
            if (enabled != _readmeControlsOtherThanComboBoxVisible)
            {
                foreach (DarkButton button in _readmeControlButtons)
                {
                    button.Visible = enabled;
                }
                _readmeControlsOtherThanComboBoxVisible = enabled;
            }
        }

        internal void ViewHTMLReadmeButton_Click(object sender, EventArgs e) => Core.ViewHTMLReadme(FMsDGV.GetMainSelectedFM());

        public void ChangeReadmeBoxFont(bool useFixed) => ReadmeRichTextBox.SetFontType(useFixed);

        #endregion

        #region Bottom bar

        #region Left side

        #region Install/Play buttons

        public void ShowInstallUninstallButton(bool enabled, bool startup = false)
        {
            try
            {
                InstallUninstallFMLLButton.SetVisible(enabled);
            }
            finally
            {
                // One time the stupid visuals didn't redraw but then they started working again even without this.
                // Argh.
                if (!startup) BottomLeftFLP.Refresh();
            }
        }

        #region Play without FM

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

        private static void ShowPerGameModsWindow(GameIndex gameIndex)
        {
            if (GameSupportsMods(gameIndex))
            {
                using var f = new OriginalGameModsForm(gameIndex);
                if (f.ShowDialogDark() != DialogResult.OK) return;
                Config.SetNewMantling(gameIndex, f.NewMantling);
                Config.SetDisabledMods(gameIndex, f.DisabledMods);
            }
            else
            {
                Core.Dialogs.ShowAlert(
                    GetLocalizedNoModSupportText(gameIndex),
                    LText.AlertMessages.Alert,
                    MBoxIcon.None);
            }
        }

        #region Single button

        // Because of the T2MP menu item breaking up the middle there, we can't array/index these menu items.
        // Just gonna have to leave this part as-is.
        internal void PlayOriginalGameButton_Click(object sender, EventArgs e)
        {
            PlayOriginalGameLLMenu.Construct();

            for (int i = 0, modI = 0; i < SupportedGameCount; i++)
            {
                GameIndex gameIndex = (GameIndex)i;
                bool gameSpecified = !Config.GetGameExe(gameIndex).IsEmpty();
                PlayOriginalGameLLMenu.GameMenuItems[i].Enabled = gameSpecified;
                if (GameIsDark(gameIndex))
                {
                    PlayOriginalGameLLMenu.ModsSubMenu.DropDownItems[modI].Enabled = gameSpecified;
                    modI++;
                }
            }
            PlayOriginalGameLLMenu.Thief2MPMenuItem.Visible = Config.T2MPDetected;

            ControlUtils.ShowMenu(PlayOriginalGameLLMenu.Menu, Lazy_PlayOriginalControls.ButtonSingle, ControlUtils.MenuPos.TopRight);
        }

        [SuppressMessage("ReSharper", "MemberCanBeMadeStatic.Global")]
        internal void PlayOriginalGameMenuItems_Click(object sender, EventArgs e)
        {
            GameIndex gameIndex = ((ToolStripMenuItemCustom)sender).GameIndex;

            bool playMP = sender == PlayOriginalGameLLMenu.Thief2MPMenuItem;

            FMInstallAndPlay.PlayOriginalGame(gameIndex, playMP);
        }

        internal void PlayOriginalGameModMenuItems_Click(object sender, EventArgs e)
        {
            ShowPerGameModsWindow(((ToolStripMenuItemCustom)sender).GameIndex);
        }

        #endregion

        #region Multiple buttons

        internal void PlayOriginalGameButtons_Click(object sender, EventArgs e)
        {
            FMInstallAndPlay.PlayOriginalGame(((DarkButton)sender).GameIndex);
        }

        internal void PlayOriginalGameButton_MouseUp(object sender, MouseEventArgs e)
        {
            if (CursorOverControl((Control)sender) && e.Button == MouseButtons.Right)
            {
                ShowPerGameModsWindow(((DarkButton)sender).GameIndex);
            }
        }

        internal void PlayOriginalT2MPButton_Click(object sender, EventArgs e)
        {
            ControlUtils.ShowMenu(PlayOriginalT2InMultiplayerLLMenu.Menu,
                Lazy_PlayOriginalControls.T2MPMenuButton, ControlUtils.MenuPos.TopRight);
        }

        internal void PlayT2MPMenuItem_Click(object sender, EventArgs e)
        {
            FMInstallAndPlay.PlayOriginalGame(GameIndex.Thief2, playMP: true);
        }

        #endregion

        // @GENGAMES (Play original game controls): End

        #endregion

        #endregion

        public void ShowWebSearchButton(bool enabled, bool startup = false)
        {
            try
            {
                Lazy_WebSearchButton.SetVisible(enabled);
            }
            finally
            {
                if (!startup) BottomLeftFLP.Refresh();
            }
        }

        internal void WebSearchButton_Click(object sender, EventArgs e) => Core.OpenWebSearchUrl(FMsDGV.GetMainSelectedFM().Title);

        #endregion

        #region Right side

        public void ShowExitButton(bool enabled, bool startup = false)
        {
            try
            {
                ExitLLButton.SetVisible(enabled);
            }
            finally
            {
                if (!startup) BottomRightFLP.Refresh();
            }
        }

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

            #endregion

            #region Bottom bar

            InstallUninstallFMLLButton.SetSayInstall(true);
            InstallUninstallFMLLButton.SetEnabled(false);

            PlayFMButton.Enabled = false;

            Lazy_WebSearchButton.SetEnabled(false);

            SetFMSelectedCountMessage(0);

            #endregion

            #region Readme area

            SetReadmeVisible(false);
            ReadmeRichTextBox.SetText("");

            ChooseReadmeLLPanel.ShowPanel(false);
            ViewHTMLReadmeLLButton.Hide();

            #endregion

            UpdateTopRightTabs();

            _displayedFM = null;

            SetTopRightBlockerVisible();
        }

        // PERF_TODO(Context menu sel state update): Since this runs always on selection change...
        // ... we might not need to call it on FM load.
        // NOTE(Context menu sel state update):
        // Keep this light and fast, because it gets called like 3 times every selection due to the @SEL_SYNC_HACK
        // for preventing "multi-select starts from top row even though our selection is not actually at the top
        // row"
        // @FM_CFG: Make even the option overrides hidden for Thief 3, as we really don't support anything for that
        // Put a label saying we don't support patch/customize stuff for Thief 3
        // @FM_CFG: Idea: Put a "Set FM option overrides..." thing in the menu that goes to the tab
        // (blink tab somehow to let the user know if the tab is already shown)
        internal void UpdateUIControlsForMultiSelectState(FanMission fm)
        {
            #region Get attributes that apply to all items

            // Crap-garbage code to loop through only once in case we have a large selection set

            int selRowsCount = 0;

            bool multiSelected,
                allAreInstalled,
                noneAreInstalled,
                allAreDark,
                noneAreDark,
                allAreAvailable,
                noneAreAvailable,
                allAreKnownAndSupported,
                allSelectedAreSameInstalledState,
                allAreSupportedAndAvailable,
                playShouldBeEnabled,
                installShouldBeEnabled;

            bool multiplePinnedStates = false;
            {
                int installedCount = 0,
                    markedUnavailableCount = 0,
                    gameIsDarkCount = 0,
                    knownAndSupportedCount = 0;

                bool atLeastOnePinned = false;
                bool atLeastOneUnpinned = false;

                // We iterate the whole list looking for selected FMs, rather than get SelectedRows every time.
                // Drag-selecting the whole ~1694 list, profiling shows that doing the full-iteration way is
                // about 2x faster than getting SelectedRows every time. It's less efficient when we have a big
                // list and small selection, but it's so fast that it doesn't really matter, and it doesn't cause
                // literally over a million allocations either.
                // But, do a special-case for when only one is selected, because then the selected rows collection
                // will be only one long for a negligible allocation, and _way_ faster in the single-select case.
                // Gets rid of lag from SortAndSetFilter() for example.
                // We don't need the loop if it's just 1, but keep it in case we want to tune this number.
                if (FMsDGV.GetRowSelectedCount() == 1)
                {
                    var selRows = FMsDGV.SelectedRows;
                    selRowsCount = selRows.Count;

                    for (int i = 0; i < selRowsCount; i++)
                    {
                        // @VBL(Multisel state updater):
                        // This whole stuff is business logic but it's the direct access of the DGV rows that
                        // prevents us from extracting it out without taking a large allocation hit
                        FanMission sFM = FMsDGV.GetFMFromIndex(selRows[i].Index);
                        if (sFM.Installed) installedCount++;
                        if (sFM.MarkedUnavailable) markedUnavailableCount++;
                        if (GameIsDark(sFM.Game)) gameIsDarkCount++;
                        if (GameIsKnownAndSupported(sFM.Game)) knownAndSupportedCount++;

                        if (!multiplePinnedStates)
                        {
                            if (sFM.Pinned)
                            {
                                atLeastOnePinned = true;
                            }
                            else
                            {
                                atLeastOneUnpinned = true;
                            }

                            if (atLeastOnePinned && atLeastOneUnpinned)
                            {
                                multiplePinnedStates = true;
                            }
                        }
                    }
                }
                else
                {
                    var rows = FMsDGV.Rows;
                    int rowCount = rows.Count;
                    for (int i = 0; i < rowCount; i++)
                    {
                        DataGridViewRow row = rows[i];
                        if (!row.Selected) continue;

                        selRowsCount++;

                        FanMission sFM = FMsDGV.GetFMFromIndex(row.Index);
                        if (sFM.Installed) installedCount++;
                        if (sFM.MarkedUnavailable) markedUnavailableCount++;
                        if (GameIsDark(sFM.Game)) gameIsDarkCount++;
                        if (GameIsKnownAndSupported(sFM.Game)) knownAndSupportedCount++;

                        if (!multiplePinnedStates)
                        {
                            if (sFM.Pinned)
                            {
                                atLeastOnePinned = true;
                            }
                            else
                            {
                                atLeastOneUnpinned = true;
                            }

                            if (atLeastOnePinned && atLeastOneUnpinned)
                            {
                                multiplePinnedStates = true;
                            }
                        }
                    }
                }

                allAreInstalled = installedCount == selRowsCount;
                noneAreInstalled = installedCount == 0;
                allAreDark = gameIsDarkCount == selRowsCount;
                noneAreDark = gameIsDarkCount == 0;
                allAreKnownAndSupported = knownAndSupportedCount == selRowsCount;
                allAreAvailable = markedUnavailableCount == 0;
                noneAreAvailable = markedUnavailableCount == selRowsCount;
                allSelectedAreSameInstalledState = allAreInstalled || noneAreInstalled;
                allAreSupportedAndAvailable = allAreKnownAndSupported && allAreAvailable;

                multiSelected = selRowsCount > 1;
                playShouldBeEnabled = !multiSelected && allAreSupportedAndAvailable;
                installShouldBeEnabled = allSelectedAreSameInstalledState &&
                                         ((multiSelected && !noneAreAvailable && allAreKnownAndSupported) || allAreSupportedAndAvailable);
            }

            // Exactly this order or we get the top-right tabs not being in a properly refreshed state
            try
            {
                SetTopRightBlockerVisible();

                Lazy_TopRightBlocker.SuspendDrawing();

                SetFMSelectedCountMessage(selRowsCount);
            }
            finally
            {
                Lazy_TopRightBlocker.ResumeDrawing();
            }

            #endregion

            FMsDGV_FM_LLMenu.SetPlayFMMenuItemEnabled(playShouldBeEnabled);
            PlayFMButton.Enabled = playShouldBeEnabled;

            FMsDGV_FM_LLMenu.SetPlayFMInMPMenuItemVisible(!multiSelected && fm.Game == Game.Thief2 && Config.T2MPDetected);
            FMsDGV_FM_LLMenu.SetPlayFMInMPMenuItemEnabled(!multiSelected && !fm.MarkedUnavailable);

            FMsDGV_FM_LLMenu.SetInstallUninstallMenuItemEnabled(installShouldBeEnabled);
            InstallUninstallFMLLButton.SetEnabled(installShouldBeEnabled);

            FMsDGV_FM_LLMenu.SetInstallUninstallMenuItemText(!fm.Installed, multiSelected);
            InstallUninstallFMLLButton.SetSayInstall(!fm.Installed);

            FMsDGV_FM_LLMenu.SetPinOrUnpinMenuItemState(!fm.Pinned);
            FMsDGV_FM_LLMenu.SetPinItemsMode(multiplePinnedStates);

            FMsDGV_FM_LLMenu.SetDeleteFMMenuItemEnabled(
                (multiSelected && !noneAreAvailable) || allAreAvailable
            );
            FMsDGV_FM_LLMenu.SetDeleteFMMenuItemVisible(!noneAreAvailable);
            FMsDGV_FM_LLMenu.SetDeleteFMMenuItemText(multiSelected);

            FMsDGV_FM_LLMenu.SetDeleteFromDBMenuItemVisible(noneAreAvailable);
            FMsDGV_FM_LLMenu.SetDeleteFromDBMenuItemText(multiSelected);

            FMsDGV_FM_LLMenu.SetOpenInDromEdMenuItemText(sayShockEd: fm.Game == Game.SS2);
            FMsDGV_FM_LLMenu.SetOpenInDromEdVisible(!multiSelected &&
                                                    fm.Game.ConvertsToDark(out GameIndex gameIndex)
                                                    && Config.GetGameEditorDetected(gameIndex));
            FMsDGV_FM_LLMenu.SetOpenInDromedEnabled(!multiSelected && !fm.MarkedUnavailable);

            FMsDGV_FM_LLMenu.SetOpenFMFolderVisible(!multiSelected && fm.Installed);

            FMsDGV_FM_LLMenu.SetScanFMMenuItemEnabled(!noneAreAvailable);
            FMsDGV_FM_LLMenu.SetScanFMText(multiSelected);

            FMsDGV_FM_LLMenu.SetConvertAudioRCSubMenuEnabled(
                !noneAreAvailable && !noneAreInstalled && !noneAreDark
            );

            FMsDGV_FM_LLMenu.SetGameSpecificFinishedOnMenuItemsText(fm.Game);

            FMsDGV_FM_LLMenu.SetWebSearchEnabled(!multiSelected);

            Lazy_WebSearchButton.SetEnabled(!multiSelected);
        }

        internal static string CreateMisCountLabelText(FanMission fm) => fm.MisCount switch
        {
            < 1 => "",
            1 => LText.StatisticsTab.MissionCount_Single,
            > 1 => LText.StatisticsTab.MissionCount_BeforeNumber +
                   fm.MisCount.ToString(CultureInfo.CurrentCulture) +
                   LText.StatisticsTab.MissionCount_AfterNumber
        };

        private void UpdateTopRightTabs()
        {
            using (new DisableEvents(this))
            {
                for (int i = 0; i < _topRightTabs.Length; i++)
                {
                    _topRightTabs[i].UpdatePage();
                }
            }
        }

        // @GENGAMES: Lots of game-specific code in here, but I don't see much to be done about it.
        // IMPORTANT(UpdateAllFMUIDataExceptReadme): ALWAYS call this when changing install state!
        // The Patch tab needs to change on install state change and you keep forgetting. So like reminder.
        public void UpdateAllFMUIDataExceptReadme(FanMission fm)
        {
            UpdateUIControlsForMultiSelectState(fm);

            // We should never get here when the view list count is 0, but hey
            MainLLMenu.SetScanAllFMsMenuItemEnabled(FMsViewList.Count > 0);

            FMsDGV_FM_LLMenu.SetFinishedOnMenuItemsChecked((Difficulty)fm.FinishedOn, fm.FinishedOnUnknown);

            UpdateTopRightTabs();
        }

        public void ClearReadmesList()
        {
            using (new DisableEvents(this))
            {
                ChooseReadmeComboBox.ClearFullItems();
            }
        }

        public void ReadmeListFillAndSelect(List<string> readmeFiles, string readme)
        {
            using (new DisableEvents(this))
            {
                if (readmeFiles.Count == 0)
                {
                    ChooseReadmeComboBox.ClearFullItems();
                    ChooseReadmeComboBox.Hide();
                }
                else
                {
                    FillReadmeListControl(ChooseReadmeComboBox, readmeFiles);
                    ChooseReadmeComboBox.SelectBackingIndexOf(readme);
                    ChooseReadmeComboBox.Show();
                }
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
                    FillReadmeListControl(ChooseReadmeLLPanel.ListBox, readmeFilesForChooser);
                    ShowReadmeControls(false);
                    ChooseReadmeLLPanel.ShowPanel(true);
                    break;
            }
        }

        private static void FillReadmeListControl(IListControlWithBackingItems readmeListControl, List<string> readmes)
        {
            try
            {
                readmeListControl.BeginUpdate();
                readmeListControl.ClearFullItems();

                foreach (string f in readmes)
                {
                    // @DIRSEP: To backslashes for each file, to prevent selection misses.
                    // I thought I accounted for this with backslashing the selected readme, but they all need to be.
                    readmeListControl.AddFullItem(f.ToBackSlashes(), f.GetFileNameFast());
                }
            }
            finally
            {
                readmeListControl.EndUpdate();
            }
        }

        public void ShowReadmeChooser(bool visible) => ChooseReadmeComboBox.Visible = visible;

        public void ShowInitialReadmeChooser(bool visible) => ChooseReadmeLLPanel.ShowPanel(visible);

        public Encoding? LoadReadmeContent(string path, ReadmeType fileType, Encoding? encoding)
        {
            return ReadmeRichTextBox.LoadContent(path, fileType, encoding);
        }

        public void SetReadmeLocalizableMessage(ReadmeLocalizableMessage messageType)
        {
            switch (messageType)
            {
                case ReadmeLocalizableMessage.NoReadmeFound:
                    ReadmeRichTextBox.SetText(LText.ReadmeArea.NoReadmeFound);
                    break;
                case ReadmeLocalizableMessage.UnableToLoadReadme:
                    ReadmeRichTextBox.SetText(LText.ReadmeArea.UnableToLoadReadme);
                    break;
            }
            ReadmeRichTextBox.LocalizableMessageType = messageType;
        }

        public void SetSelectedEncoding(Encoding encoding) => EncodingsLLMenu.SetEncodingMenuItemChecked(encoding);

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

        private void BottomLeftFLP_Paint(object sender, PaintEventArgs e) => Images.PaintControlSeparators(e, 2, items: _bottomLeftAreaSeparatedItems);

        private void BottomRightFLP_Paint(object sender, PaintEventArgs e) => Images.PaintControlSeparators(e, 2, items: _bottomRightAreaSeparatedItems);

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

        private void FilterBarFLP_Paint(object sender, PaintEventArgs e) => Images.PaintControlSeparators(e, -1, _filterLabels, 5, 20);

        private void PlayFMButton_Paint(object sender, PaintEventArgs e) => Images.PaintPlayFMButton(PlayFMButton, e);

        internal void PlayOriginalGameButton_Paint(object sender, PaintEventArgs e) => Images.PaintPlayOriginalButton(Lazy_PlayOriginalControls.ButtonSingle, e);

        internal void PlayOriginalGamesButtons_Paint(object sender, PaintEventArgs e)
        {
            DarkButton button = (DarkButton)sender;

            Image image = Images.GetPerGameImage(button.GameIndex).Primary.Large(button.Enabled);

            Images.PaintBitmapButton(button, e, image, x: 8);
        }

        private void SettingsButton_Paint(object sender, PaintEventArgs e) => Images.PaintBitmapButton(
            SettingsButton,
            e,
            SettingsButton.Enabled ? Images.Settings : Images.GetDisabledImage(Images.Settings),
            x: 10);

        private void TopRightMenuButton_Paint(object sender, PaintEventArgs e) => Images.PaintHamburgerMenuButton_TopRight(TopRightMenuButton, e);

        private void MainMenuButton_Paint(object sender, PaintEventArgs e) => Images.PaintHamburgerMenuButton24(MainMenuButton, e);

        private void ReadmeFullScreenButton_Paint(object sender, PaintEventArgs e) => Images.PaintReadmeFullScreenButton(ReadmeFullScreenButton, e);

        private void ReadmeEncodingButton_Paint(object sender, PaintEventArgs e) => Images.PaintReadmeEncodingButton(ReadmeEncodingButton, e);

        private void ResetLayoutButton_Paint(object sender, PaintEventArgs e) => Images.PaintResetLayoutButton(ResetLayoutButton, e);

        internal void ScanIconButtons_Paint(object sender, PaintEventArgs e) => Images.PaintScanButtons((Button)sender, e);

        private void ZoomInButtons_Paint(object sender, PaintEventArgs e) => Images.PaintZoomButtons((Button)sender, e, Zoom.In);

        private void ZoomOutButtons_Paint(object sender, PaintEventArgs e) => Images.PaintZoomButtons((Button)sender, e, Zoom.Out);

        private void ZoomResetButtons_Paint(object sender, PaintEventArgs e) => Images.PaintZoomButtons((Button)sender, e, Zoom.Reset);

        #endregion

        #region Helpers & misc

        private void CancelResizables()
        {
            FMsDGV.CancelColumnResize();
            MainSplitContainer.CancelResize();
            TopSplitContainer.CancelResize();
        }

        public void SetWaitCursor(bool value) => Cursor = value ? Cursors.WaitCursor : Cursors.Default;

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

        public void Block(bool block) => Invoke(() =>
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
        });

        private void UpdateConfig()
        {
            GameIndex gameTab = GameIndex.Thief1;
            if (Config.GameOrganization == GameOrganization.ByTab)
            {
                SaveCurrentTabSelectedFM(GamesTabControl.SelectedTab);
                var selGameTab = GamesTabControl.SelectedTab;
                for (int i = 0; i < SupportedGameCount; i++)
                {
                    if (_gameTabs[i] == selGameTab)
                    {
                        gameTab = (GameIndex)i;
                        break;
                    }
                }
            }

            SelectedFM selectedFM = FMsDGV.GetMainSelectedFMPosInfo();

            var topRightTabs = new TopRightTabsData
            {
                SelectedTab = (TopRightTab)Array.IndexOf(_topRightTabs.Cast<TabPage>().ToArray(), TopRightTabControl.SelectedTab)
            };

            for (int i = 0; i < TopRightTabsData.Count; i++)
            {
                topRightTabs.Tabs[i].DisplayIndex = TopRightTabControl.GetTabDisplayIndex(_topRightTabs[i]);
                topRightTabs.Tabs[i].Visible = TopRightTabControl.Contains(_topRightTabs[i]);
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

        public IContainer GetComponents() => components;

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
            return TagsTabPage.AddTagLLDropDownVisible() &&
                   // Check Size instead of ClientSize in order to support clicking the scroll bar
                   !TagsTabPage.CursorOverAddTagLLDropDown(fullArea: true) &&
                   !TagsTabPage.CursorOverAddTagTextBox() &&
                   !TagsTabPage.CursorOverAddTagButton();
        }

        internal bool CursorOverControl(Control control, bool fullArea = false)
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

        public void ActivateThisInstance()
        {
            if (WindowState == FormWindowState.Minimized) WindowState = _nominalWindowState;
            Activate();
        }

        private static T GetObjectFromMenuItem<T>(DarkContextMenu menu, ToolStripMenuItemCustom menuItem, T[] array, int count) where T : class
        {
            T? obj = null;
            for (int i = 0; i < count; i++)
            {
                if (menuItem == (ToolStripMenuItemCustom)menu.Items[i])
                {
                    obj = array[i];
                    break;
                }
            }

            AssertR(obj != null, nameof(obj) + " is null - object does not have a corresponding menu item");

            return obj!;
        }

        #endregion

        public bool GetUIEnabled() => EverythingPanel.Enabled;

        public void SetUIEnabled(bool enabled)
        {
            bool doFocus = !EverythingPanel.Enabled && enabled;

            EverythingPanel.Enabled = enabled;

            if (!doFocus) return;

            // The "mouse wheel scroll without needing to focus" thing stops working when no control is focused
            // (this happens when we disable and enable EverythingPanel). Therefore, we need to give focus to a
            // control here. One is as good as the next, but FMsDGV seems like a sensible choice.
            FMsDGV.Focus();
            FMsDGV.SelectProperly();
        }

        #region Drag & drop

        public bool AbleToAcceptDragDrop() => GetUIEnabled();

        private void EverythingPanel_DragEnter(object sender, DragEventArgs e)
        {
            if (Core.FilesDropped(e.Data.GetData(DataFormats.FileDrop), out _))
            {
                e.Effect = DragDropEffects.Copy;
            }
        }

        private async void EverythingPanel_DragDrop(object sender, DragEventArgs e)
        {
            if (Core.FilesDropped(e.Data.GetData(DataFormats.FileDrop), out string[]? droppedItems))
            {
                await FMArchives.Add(droppedItems.ToList());
            }
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

        #endregion

        #region Show dialogs

        public (bool Accepted, FMScanner.ScanOptions ScanOptions, bool NoneSelected)
        ShowScanAllFMsWindow(bool selected)
        {
            using var f = new ScanAllFMsForm(selected);
            return (f.ShowDialogDark(this) == DialogResult.OK, f.ScanOptions, f.NoneSelected);
        }

        public (bool Accepted,
            string IniFile,
            bool ImportFMData,
            bool ImportTitle,
            bool ImportSize,
            bool ImportComment,
            bool ImportReleaseDate,
            bool ImportLastPlayed,
            bool ImportFinishedOn,
            bool ImportSaves)
        ShowDarkLoaderImportWindow()
        {
            using var f = new ImportFromDarkLoaderForm();
            return (Accepted: f.ShowDialogDark(this) == DialogResult.OK,
                    IniFile: f.DarkLoaderIniFile,
                    ImportFMData: f.ImportFMData,
                    ImportTitle: f.ImportTitle,
                    ImportSize: f.ImportSize,
                    ImportComment: f.ImportComment,
                    ImportReleaseDate: f.ImportReleaseDate,
                    ImportLastPlayed: f.ImportLastPlayed,
                    ImportFinishedOn: f.ImportFinishedOn,
                    ImportSaves: f.ImportSaves
                );
        }

        public (bool Accepted,
            List<string> IniFiles,
            bool ImportTitle,
            bool ImportReleaseDate,
            bool ImportLastPlayed,
            bool ImportComment,
            bool ImportRating,
            bool ImportDisabledMods,
            bool ImportTags,
            bool ImportSelectedReadme,
            bool ImportFinishedOn,
            bool ImportSize)
            ShowImportFromMultipleInisForm(ImportType importType)
        {
            using var f = new ImportFromMultipleInisForm(importType);
            return (Accepted: f.ShowDialogDark(this) == DialogResult.OK,
                    IniFiles: f.IniFiles,
                    ImportTitle: f.ImportTitle,
                    ImportReleaseDate: f.ImportReleaseDate,
                    ImportLastPlayed: f.ImportLastPlayed,
                    ImportComment: f.ImportComment,
                    ImportRating: f.ImportRating,
                    ImportDisabledMods: f.ImportDisabledMods,
                    ImportTags: f.ImportTags,
                    ImportSelectedReadme: f.ImportSelectedReadme,
                    ImportFinishedOn: f.ImportFinishedOn,
                    ImportSize: f.ImportSize
                );
        }

        #endregion

        private void TopSplitContainer_FullScreenChanged(object sender, EventArgs e)
        {
            TopRightTabControl.Enabled = !TopSplitContainer.FullScreen;
        }

        private void MainSplitContainer_FullScreenChanged(object sender, EventArgs e)
        {
            MainSplitContainer.Panel1.Enabled = !MainSplitContainer.FullScreen;
        }

        #region FM selected stats

        private string _fmSelectedCountText = "";
        private string _fmCountText = "";

        // @vNext: Maybe only display it if the top-right area is hidden?
        private void SetFMSelectedCountMessage(int count)
        {
            string text =
                (count == 1 ? LText.FMSelectedStats.FMsSelected_Single_BeforeNumber : LText.FMSelectedStats.FMsSelected_Plural_BeforeNumber) +
                count.ToString(CultureInfo.CurrentCulture) +
                (count == 1 ? LText.FMSelectedStats.FMsSelected_Single_AfterNumber : LText.FMSelectedStats.FMsSelected_Plural_AfterNumber);

            _fmSelectedCountText = text;

            Lazy_TopRightBlocker.SetText(text);

            RefreshFMStatsLabel();
        }

        private void RefreshFMStatsLabel() => FMCountLabel.Text = _fmSelectedCountText + "\r\n" + _fmCountText;

        public void SetAvailableFMCount()
        {
            int count = 0;
            for (int i = 0; i < FMsViewList.Count; i++)
            {
                if (!FMsViewList[i].MarkedUnavailable) count++;
            }

            _fmCountText =
                (count == 1 ? LText.FMSelectedStats.FMsAvailable_Single_BeforeNumber : LText.FMSelectedStats.FMsAvailable_Plural_BeforeNumber) +
                count.ToString(CultureInfo.CurrentCulture) +
                (count == 1 ? LText.FMSelectedStats.FMsAvailable_Single_AfterNumber : LText.FMSelectedStats.FMsAvailable_Plural_AfterNumber);

            RefreshFMStatsLabel();
        }

        #endregion
    }
}
