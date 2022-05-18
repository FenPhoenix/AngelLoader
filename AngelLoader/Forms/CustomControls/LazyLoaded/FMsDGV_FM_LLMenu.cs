using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using AL_Common;
using AngelLoader.DataClasses;
using JetBrains.Annotations;
using static AngelLoader.GameSupport;
using static AngelLoader.Misc;

namespace AngelLoader.Forms.CustomControls.LazyLoaded
{
    internal sealed class FMsDGV_FM_LLMenu
    {
        #region Backing fields

        private bool _constructed;
        private bool _playFMMenuItemEnabled;
        private bool _playFMInMPMenuItemVisible;
        private bool _playFMInMPMenuItemEnabled;
        private bool _installUninstallMenuItemEnabled;
        private bool _deleteFMMenuItemEnabled;
        private bool _openInDromEdMenuItemVisible;
        private bool _openInDromedMenuItemEnabled;
        private bool _openFMFolderMenuItemVisible;
        private bool _scanFMMenuItemEnabled;
        private bool _convertAudioSubMenuEnabled;
        private int _rating = -1;
        private bool _finishedOnNormalChecked;
        private bool _finishedOnHardChecked;
        private bool _finishedOnExpertChecked;
        private bool _finishedOnExtremeChecked;
        private bool _finishedOnUnknownChecked;
        private bool _webSearchMenuItemEnabled;

        #endregion

        private bool _sayPin = true;

        private readonly MainForm _owner;

        #region Menu item fields

        private DarkContextMenu _menu = null!;
        internal DarkContextMenu Menu
        {
            get
            {
                Construct();
                return _menu;
            }
        }

        private ToolStripMenuItemCustom PlayFMMenuItem = null!;
        private ToolStripMenuItemCustom PlayFMInMPMenuItem = null!;
        private ToolStripMenuItemCustom InstallUninstallMenuItem = null!;
        private ToolStripMenuItemCustom PinToTopMenuItem = null!;
        private ToolStripMenuItemCustom ExplicitPinToTopMenuItem = null!;
        private ToolStripMenuItemCustom ExplicitUnpinFromTopMenuItem = null!;
        private ToolStripMenuItemCustom DeleteFMMenuItem = null!;
        private ToolStripSeparator OpenInDromEdSep = null!;
        private ToolStripMenuItemCustom OpenInDromEdMenuItem = null!;
        private ToolStripSeparator OpenFMFolderSep = null!;
        private ToolStripMenuItemCustom OpenFMFolderMenuItem = null!;
        private ToolStripMenuItemCustom ScanFMMenuItem = null!;
        private ToolStripMenuItemCustom ConvertAudioMenuItem = null!;
        private ToolStripMenuItemCustom ConvertWAVsTo16BitMenuItem = null!;
        private ToolStripMenuItemCustom ConvertOGGsToWAVsMenuItem = null!;
        private ToolStripMenuItemCustom RatingMenuItem = null!;
        private ToolStripMenuItemCustom RatingMenuUnrated = null!;
        private ToolStripMenuItemCustom FinishedOnMenuItem = null!;
        private DarkContextMenu FinishedOnMenu = null!;
        private ToolStripMenuItemCustom FinishedOnNormalMenuItem = null!;
        private ToolStripMenuItemCustom FinishedOnHardMenuItem = null!;
        private ToolStripMenuItemCustom FinishedOnExpertMenuItem = null!;
        private ToolStripMenuItemCustom FinishedOnExtremeMenuItem = null!;
        private ToolStripMenuItemCustom FinishedOnUnknownMenuItem = null!;
        private ToolStripMenuItemCustom WebSearchMenuItem = null!;

        #endregion

        private bool _darkModeEnabled;
        [PublicAPI]
        public bool DarkModeEnabled
        {
            get => _darkModeEnabled;
            set
            {
                if (_darkModeEnabled == value) return;
                _darkModeEnabled = value;
                if (!_constructed) return;

                _menu.DarkModeEnabled = _darkModeEnabled;
                FinishedOnMenu.DarkModeEnabled = _darkModeEnabled;

                DeleteFMMenuItem.Image = Images.Trash_16;
                PinToTopMenuItem.Image = _sayPin ? Images.Pin_16 : Images.Unpin_16;
                ExplicitPinToTopMenuItem.Image = Images.Pin_16;
                ExplicitUnpinFromTopMenuItem.Image = Images.Unpin_16;
            }
        }

        internal FMsDGV_FM_LLMenu(MainForm owner) => _owner = owner;

        #region Private methods

        private void UncheckFinishedOnMenuItemsExceptUnknown()
        {
            if (_constructed)
            {
                FinishedOnNormalMenuItem.Checked = false;
                FinishedOnHardMenuItem.Checked = false;
                FinishedOnExpertMenuItem.Checked = false;
                FinishedOnExtremeMenuItem.Checked = false;
            }
            else
            {
                _finishedOnNormalChecked = false;
                _finishedOnHardChecked = false;
                _finishedOnExpertChecked = false;
                _finishedOnExtremeChecked = false;
            }
        }

        private void SetFinishedOnUnknownMenuItemChecked(bool value)
        {
            if (_constructed)
            {
                FinishedOnUnknownMenuItem.Checked = value;
            }
            else
            {
                _finishedOnUnknownChecked = value;
            }

            if (value) UncheckFinishedOnMenuItemsExceptUnknown();
        }

        private void SetFinishedOnMenuItemChecked(Difficulty difficulty, bool value)
        {
            if (value && !_constructed) _finishedOnUnknownChecked = false;

            switch (difficulty)
            {
                case Difficulty.Normal:
                    if (_constructed)
                    {
                        FinishedOnNormalMenuItem.Checked = value;
                    }
                    else
                    {
                        _finishedOnNormalChecked = value;
                    }
                    break;
                case Difficulty.Hard:
                    if (_constructed)
                    {
                        FinishedOnHardMenuItem.Checked = value;
                    }
                    else
                    {
                        _finishedOnHardChecked = value;
                    }
                    break;
                case Difficulty.Expert:
                    if (_constructed)
                    {
                        FinishedOnExpertMenuItem.Checked = value;
                    }
                    else
                    {
                        _finishedOnExpertChecked = value;
                    }
                    break;
                case Difficulty.Extreme:
                    if (_constructed)
                    {
                        FinishedOnExtremeMenuItem.Checked = value;
                    }
                    else
                    {
                        _finishedOnExtremeChecked = value;
                    }
                    break;
            }
        }

        #endregion

        #region Public methods

        internal bool Visible => _constructed && _menu.Visible;

        private void Construct()
        {
            if (_constructed) return;

            #region Instantiation

            _menu = new DarkContextMenu(_darkModeEnabled, _owner.GetComponents()) { Tag = LoadType.Lazy };
            FinishedOnMenu = new DarkContextMenu(_darkModeEnabled, _owner.GetComponents()) { Tag = LoadType.Lazy };

            #endregion

            #region Add items to menu

            _menu.Items.AddRange(new ToolStripItem[]
            {
                PlayFMMenuItem = new ToolStripMenuItemCustom { Tag = LoadType.Lazy },
                PlayFMInMPMenuItem = new ToolStripMenuItemCustom { Tag = LoadType.Lazy },
                InstallUninstallMenuItem = new ToolStripMenuItemCustom { Tag = LoadType.Lazy },
                new ToolStripSeparator { Tag = LoadType.Lazy },
                PinToTopMenuItem = new ToolStripMenuItemCustom { Tag = LoadType.Lazy },
                ExplicitPinToTopMenuItem = new ToolStripMenuItemCustom { Tag = LoadType.Lazy, Image = Images.Pin_16, Visible = false },
                ExplicitUnpinFromTopMenuItem = new ToolStripMenuItemCustom { Tag = LoadType.Lazy, Image = Images.Unpin_16, Visible = false },
                new ToolStripSeparator { Tag = LoadType.Lazy },
                DeleteFMMenuItem = new ToolStripMenuItemCustom { Image = Images.Trash_16, Tag = LoadType.Lazy },
                OpenInDromEdSep = new ToolStripSeparator { Tag = LoadType.Lazy },
                OpenInDromEdMenuItem = new ToolStripMenuItemCustom { Tag = LoadType.Lazy },
                OpenFMFolderSep = new ToolStripSeparator { Tag = LoadType.Lazy },
                OpenFMFolderMenuItem = new ToolStripMenuItemCustom { Tag = LoadType.Lazy },
                new ToolStripSeparator { Tag = LoadType.Lazy },
                ScanFMMenuItem = new ToolStripMenuItemCustom { Tag = LoadType.Lazy },
                ConvertAudioMenuItem = new ToolStripMenuItemCustom { Tag = LoadType.Lazy },
                new ToolStripSeparator { Tag = LoadType.Lazy },
                RatingMenuItem = new ToolStripMenuItemCustom { Tag = LoadType.Lazy },
                FinishedOnMenuItem = new ToolStripMenuItemCustom { Tag = LoadType.Lazy },
                new ToolStripSeparator { Tag = LoadType.Lazy },
                WebSearchMenuItem = new ToolStripMenuItemCustom { Tag = LoadType.Lazy }
            });

            ConvertAudioMenuItem.DropDownItems.AddRange(new ToolStripItem[]
            {
                ConvertWAVsTo16BitMenuItem = new ToolStripMenuItemCustom{ Tag = LoadType.Lazy },
                ConvertOGGsToWAVsMenuItem = new ToolStripMenuItemCustom{ Tag = LoadType.Lazy }
            });

            RatingMenuItem.DropDownItems.Add(RatingMenuUnrated = new ToolStripMenuItemCustom { CheckOnClick = true, Tag = LoadType.Lazy });
            for (int i = 0; i < 11; i++)
            {
                RatingMenuItem.DropDownItems.Add(new ToolStripMenuItemCustom { CheckOnClick = true, Tag = LoadType.Lazy });
            }

            FinishedOnMenu.Items.AddRange(new ToolStripItem[]
            {
                FinishedOnNormalMenuItem = new ToolStripMenuItemCustom { CheckOnClick = true, Tag = LoadType.Lazy },
                FinishedOnHardMenuItem = new ToolStripMenuItemCustom { CheckOnClick = true, Tag = LoadType.Lazy },
                FinishedOnExpertMenuItem = new ToolStripMenuItemCustom { CheckOnClick = true, Tag = LoadType.Lazy },
                FinishedOnExtremeMenuItem = new ToolStripMenuItemCustom { CheckOnClick = true, Tag = LoadType.Lazy },
                FinishedOnUnknownMenuItem = new ToolStripMenuItemCustom { CheckOnClick = true, Tag = LoadType.Lazy }
            });

            FinishedOnMenu.SetPreventCloseOnClickItems(FinishedOnMenu.Items.Cast<ToolStripMenuItemCustom>().ToArray());
            FinishedOnMenuItem.DropDown = FinishedOnMenu;

            #endregion

            #region Event hookups

            _menu.Opening += MenuOpening;
            PlayFMMenuItem.Click += AsyncMenuItems_Click;
            PlayFMInMPMenuItem.Click += AsyncMenuItems_Click;
            InstallUninstallMenuItem.Click += AsyncMenuItems_Click;
            PinToTopMenuItem.Click += AsyncMenuItems_Click;
            ExplicitPinToTopMenuItem.Click += AsyncMenuItems_Click;
            ExplicitUnpinFromTopMenuItem.Click += AsyncMenuItems_Click;
            DeleteFMMenuItem.Click += AsyncMenuItems_Click;
            OpenInDromEdMenuItem.Click += AsyncMenuItems_Click;
            OpenFMFolderMenuItem.Click += AsyncMenuItems_Click;
            ScanFMMenuItem.Click += AsyncMenuItems_Click;
            ConvertWAVsTo16BitMenuItem.Click += AsyncMenuItems_Click;
            ConvertOGGsToWAVsMenuItem.Click += AsyncMenuItems_Click;

            foreach (ToolStripMenuItemCustom item in RatingMenuItem.DropDownItems)
            {
                item.Click += RatingMenuItems_Click;
                item.CheckedChanged += RatingRCMenuItems_CheckedChanged;
            }

            FinishedOnNormalMenuItem.Click += FinishedOnMenuItems_Click;
            FinishedOnHardMenuItem.Click += FinishedOnMenuItems_Click;
            FinishedOnExpertMenuItem.Click += FinishedOnMenuItems_Click;
            FinishedOnExtremeMenuItem.Click += FinishedOnMenuItems_Click;
            FinishedOnUnknownMenuItem.Click += FinishedOnMenuItems_Click;
            FinishedOnUnknownMenuItem.CheckedChanged += FinishedOnUnknownMenuItem_CheckedChanged;

            WebSearchMenuItem.Click += WebSearchMenuItem_Click;

            #endregion

            #region Set main menu item values

            PlayFMMenuItem.Enabled = _playFMMenuItemEnabled;
            PlayFMInMPMenuItem.Visible = _playFMInMPMenuItemVisible;
            PlayFMInMPMenuItem.Enabled = _playFMInMPMenuItemEnabled;

            InstallUninstallMenuItem.Enabled = _installUninstallMenuItemEnabled;

            DeleteFMMenuItem.Enabled = _deleteFMMenuItemEnabled;

            OpenInDromEdSep.Visible = _openInDromEdMenuItemVisible;
            OpenInDromEdMenuItem.Visible = _openInDromEdMenuItemVisible;
            OpenInDromEdMenuItem.Enabled = _openInDromedMenuItemEnabled;

            OpenFMFolderSep.Visible = _openFMFolderMenuItemVisible;
            OpenFMFolderMenuItem.Visible = _openFMFolderMenuItemVisible;

            ScanFMMenuItem.Enabled = _scanFMMenuItemEnabled;

            ConvertAudioMenuItem.Enabled = _convertAudioSubMenuEnabled;

            WebSearchMenuItem.Enabled = _webSearchMenuItemEnabled;

            #endregion

            #region Set Finished On checked values

            FinishedOnNormalMenuItem.Checked = _finishedOnNormalChecked;
            FinishedOnHardMenuItem.Checked = _finishedOnHardChecked;
            FinishedOnExpertMenuItem.Checked = _finishedOnExpertChecked;
            FinishedOnExtremeMenuItem.Checked = _finishedOnExtremeChecked;
            FinishedOnUnknownMenuItem.Checked = _finishedOnUnknownChecked;

            #endregion

            _constructed = true;

            // These must come after the constructed bool gets set to true
            SetPinItemsMode();
            UpdateRatingList(Config.RatingDisplayStyle == RatingDisplayStyle.FMSel);
            SetRatingMenuItemChecked(_rating);
            Localize();
        }

        internal void Localize()
        {
            if (!_constructed) return;

            #region Get current FM info

            // Some menu items' text depends on FM state. Because this could be run after startup, we need to
            // make sure those items' text is set correctly.
            FanMission? selFM = _owner.GetMainSelectedFMOrNull();
            bool sayInstall = selFM is not { Installed: true };
            // @GENGAMES - Localize FM context menu - "sayShockEd"
            bool sayShockEd = selFM is { Game: Game.SS2 };
            bool sayPin = selFM is not { Pinned: true };

            bool multiSelected = _owner.FMsDGV.MultipleFMsSelected();

            #endregion

            #region Play

            PlayFMMenuItem.Text = LText.Global.PlayFM;
            PlayFMInMPMenuItem.Text = LText.FMsList.FMMenu_PlayFM_Multiplayer;

            #endregion

            SetInstallUninstallMenuItemText(sayInstall, multiSelected);

            SetPinOrUnpinMenuItemState(sayPin);
            ExplicitPinToTopMenuItem.Text = LText.FMsList.FMMenu_PinFM;
            ExplicitUnpinFromTopMenuItem.Text = LText.FMsList.FMMenu_UnpinFM;

            SetDeleteFMMenuItemText(multiSelected);

            SetOpenInDromEdMenuItemText(sayShockEd);

            OpenFMFolderMenuItem.Text = LText.FMsList.FMMenu_OpenFMFolder;

            SetScanFMText();

            #region Convert audio submenu

            ConvertAudioMenuItem.Text = LText.FMsList.FMMenu_ConvertAudio;
            ConvertWAVsTo16BitMenuItem.Text = LText.FMsList.ConvertAudioMenu_ConvertWAVsTo16Bit;
            ConvertOGGsToWAVsMenuItem.Text = LText.FMsList.ConvertAudioMenu_ConvertOGGsToWAVs;

            #endregion

            #region Rating submenu

            RatingMenuItem.Text = LText.FMsList.FMMenu_Rating;
            RatingMenuUnrated.Text = LText.Global.Unrated;

            #endregion

            #region Finished On submenu

            FinishedOnMenuItem.Text = LText.FMsList.FMMenu_FinishedOn;

            SetGameSpecificFinishedOnMenuItemsText(selFM?.Game ?? Game.Null);
            FinishedOnUnknownMenuItem.Text = LText.Difficulties.Unknown;

            #endregion

            WebSearchMenuItem.Text = LText.FMsList.FMMenu_WebSearch;
        }

        internal void SetScanFMText()
        {
            if (!_constructed) return;

            bool multiSelected = _owner.FMsDGV.MultipleFMsSelected();
            ScanFMMenuItem.Text = multiSelected ? LText.FMsList.FMMenu_ScanFMs : LText.FMsList.FMMenu_ScanFM;
        }

        internal void UpdateRatingList(bool fmSelStyle)
        {
            if (!_constructed) return;

            for (int i = 0; i <= 10; i++)
            {
                string num = (fmSelStyle ? i / 2.0 : i).ToString(CultureInfo.CurrentCulture);
                RatingMenuItem.DropDownItems[i + 1].Text = num;
            }
        }

        internal ContextMenuStrip GetFinishedOnMenu()
        {
            Construct();
            return FinishedOnMenu;
        }

        internal void SetPlayFMMenuItemEnabled(bool value)
        {
            if (_constructed)
            {
                PlayFMMenuItem.Enabled = value;
            }
            else
            {
                _playFMMenuItemEnabled = value;
            }
        }

        internal void SetPlayFMInMPMenuItemVisible(bool value)
        {
            if (_constructed)
            {
                PlayFMInMPMenuItem.Visible = value;
            }
            else
            {
                _playFMInMPMenuItemVisible = value;
            }
        }

        internal void SetPlayFMInMPMenuItemEnabled(bool value)
        {
            if (_constructed)
            {
                PlayFMInMPMenuItem.Enabled = value;
            }
            else
            {
                _playFMInMPMenuItemEnabled = value;
            }
        }

        internal void SetInstallUninstallMenuItemEnabled(bool value)
        {
            if (_constructed)
            {
                InstallUninstallMenuItem.Enabled = value;
            }
            else
            {
                _installUninstallMenuItemEnabled = value;
            }
        }

        internal void SetInstallUninstallMenuItemText(bool sayInstall, bool multiSelected)
        {
            if (!_constructed) return;

            InstallUninstallMenuItem.Text =
                sayInstall
                    ? multiSelected
                        ? LText.Global.InstallFMs
                        : LText.Global.InstallFM
                    : multiSelected
                        ? LText.Global.UninstallFMs
                        : LText.Global.UninstallFM;
        }

        internal void SetPinOrUnpinMenuItemState(bool sayPin)
        {
            if (!_constructed) return;

            _sayPin = sayPin;

            PinToTopMenuItem.Text = sayPin
                ? LText.FMsList.FMMenu_PinFM
                : LText.FMsList.FMMenu_UnpinFM;

            PinToTopMenuItem.Image = sayPin ? Images.Pin_16 : Images.Unpin_16;
        }

        internal void SetPinItemsMode()
        {
            if (!_constructed) return;

            bool atLeastOnePinned = false;
            bool atLeastOneUnpinned = false;

            bool multiplePinnedStates = false;

            var selectedFMs = _owner.FMsDGV.GetSelectedFMs();
            if (selectedFMs.Length > 1)
            {
                for (int i = 0; i < selectedFMs.Length; i++)
                {
                    if (selectedFMs[i].Pinned)
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
                        break;
                    }
                }
            }

            if (multiplePinnedStates)
            {
                PinToTopMenuItem.Visible = false;
                ExplicitPinToTopMenuItem.Visible = true;
                ExplicitUnpinFromTopMenuItem.Visible = true;
            }
            else
            {
                PinToTopMenuItem.Visible = true;
                ExplicitPinToTopMenuItem.Visible = false;
                ExplicitUnpinFromTopMenuItem.Visible = false;
            }
        }

        internal void SetDeleteFMMenuItemEnabled(bool value)
        {
            if (_constructed)
            {
                DeleteFMMenuItem.Enabled = value;
            }
            else
            {
                _deleteFMMenuItemEnabled = value;
            }
        }

        internal void SetDeleteFMMenuItemText(bool multiSelected)
        {
            if (!_constructed) return;

            DeleteFMMenuItem.Text = multiSelected
                ? LText.FMsList.FMMenu_DeleteFMs
                : LText.FMsList.FMMenu_DeleteFM;
        }

        internal void SetOpenInDromEdVisible(bool value)
        {
            if (_constructed)
            {
                OpenInDromEdSep.Visible = value;
                OpenInDromEdMenuItem.Visible = value;
            }
            else
            {
                _openInDromEdMenuItemVisible = value;
            }
        }

        internal void SetOpenInDromedEnabled(bool value)
        {
            if (_constructed)
            {
                OpenInDromEdMenuItem.Enabled = value;
            }
            else
            {
                _openInDromedMenuItemEnabled = value;
            }
        }

        internal void SetOpenInDromEdMenuItemText(bool sayShockEd)
        {
            if (!_constructed) return;

            OpenInDromEdMenuItem.Text = sayShockEd
                ? LText.FMsList.FMMenu_OpenInShockEd
                : LText.FMsList.FMMenu_OpenInDromEd;
        }

        internal void SetOpenFMFolderVisible(bool value)
        {
            if (_constructed)
            {
                OpenFMFolderSep.Visible = value;
                OpenFMFolderMenuItem.Visible = value;
            }
            else
            {
                _openFMFolderMenuItemVisible = value;
            }
        }

        internal void SetScanFMMenuItemEnabled(bool value)
        {
            if (_constructed)
            {
                ScanFMMenuItem.Enabled = value;
            }
            else
            {
                _scanFMMenuItemEnabled = value;
            }
        }

        internal void SetConvertAudioRCSubMenuEnabled(bool value)
        {
            if (_constructed)
            {
                ConvertAudioMenuItem.Enabled = value;
            }
            else
            {
                _convertAudioSubMenuEnabled = value;
            }
        }

        internal void SetRatingMenuItemChecked(int value)
        {
            value = value.Clamp(-1, 10);

            if (_constructed)
            {
                ((ToolStripMenuItemCustom)RatingMenuItem.DropDownItems[value + 1]).Checked = true;
            }
            else
            {
                _rating = value;
            }
        }

        internal void SetFinishedOnMenuItemsChecked(Difficulty difficulty, bool finishedOnUnknown)
        {
            if (finishedOnUnknown)
            {
                SetFinishedOnUnknownMenuItemChecked(true);
            }
            else
            {
                // I don't have to disable events because I'm only wired up to Click, not Checked
                SetFinishedOnMenuItemChecked(Difficulty.Normal, difficulty.HasFlagFast(Difficulty.Normal));
                SetFinishedOnMenuItemChecked(Difficulty.Hard, difficulty.HasFlagFast(Difficulty.Hard));
                SetFinishedOnMenuItemChecked(Difficulty.Expert, difficulty.HasFlagFast(Difficulty.Expert));
                SetFinishedOnMenuItemChecked(Difficulty.Extreme, difficulty.HasFlagFast(Difficulty.Extreme));
                SetFinishedOnUnknownMenuItemChecked(false);
            }
        }

        // Thief 1+2 difficulties: Normal, Hard, Expert, Extreme ("Extreme" is for DarkLoader compatibility)
        // Thief 3 difficulties: Easy, Normal, Hard, Expert
        // SS2 difficulties: Easy, Normal, Hard, Impossible
        internal void SetGameSpecificFinishedOnMenuItemsText(Game game)
        {
            if (!_constructed) return;

            FinishedOnNormalMenuItem.Text = GetLocalizedDifficultyName(game, Difficulty.Normal);
            FinishedOnHardMenuItem.Text = GetLocalizedDifficultyName(game, Difficulty.Hard);
            FinishedOnExpertMenuItem.Text = GetLocalizedDifficultyName(game, Difficulty.Expert);
            FinishedOnExtremeMenuItem.Text = GetLocalizedDifficultyName(game, Difficulty.Extreme);
        }

        internal void ClearFinishedOnMenuItemChecks()
        {
            SetFinishedOnUnknownMenuItemChecked(false);
            UncheckFinishedOnMenuItemsExceptUnknown();
        }

        internal void SetWebSearchEnabled(bool value)
        {
            if (_constructed)
            {
                WebSearchMenuItem.Enabled = value;
            }
            else
            {
                _webSearchMenuItemEnabled = value;
            }
        }

        #endregion

        #region Event handlers

        private void MenuOpening(object sender, CancelEventArgs e)
        {
            // Fix for a corner case where the user could press the right mouse button, hold it, keyboard-switch
            // to an empty tab, then let up the mouse and a menu would come up even though no FM was selected.
            if (_owner.FMsDGV.RowCount == 0 ||
                !_owner.FMsDGV.RowSelected() ||
                _owner.ViewBlocked)
            {
                e.Cancel = true;
            }
        }

        // Extra async/await avoidance
        private async void AsyncMenuItems_Click(object sender, EventArgs e)
        {
            if (sender == PlayFMMenuItem || sender == PlayFMInMPMenuItem)
            {
                await FMInstallAndPlay.InstallIfNeededAndPlay(_owner.FMsDGV.GetMainSelectedFM(), playMP: sender == PlayFMInMPMenuItem);
            }
            else if (sender == InstallUninstallMenuItem)
            {
                await FMInstallAndPlay.InstallOrUninstall(_owner.GetSelectedFMs_InOrder());
            }
            else if (sender == DeleteFMMenuItem)
            {
                if (_owner.FMsDGV.MultipleFMsSelected())
                {
                    await FMArchives.Delete(_owner.FMsDGV.GetSelectedFMs_InOrder_List());
                }
                else
                {
                    await FMArchives.Delete(_owner.FMsDGV.GetMainSelectedFM());
                }
            }
            else if (sender == OpenInDromEdMenuItem)
            {
                var fm = _owner.FMsDGV.GetMainSelectedFM();
                if (fm.Installed || await FMInstallAndPlay.Install(fm)) FMInstallAndPlay.OpenFMInEditor(fm);
            }
            else if (sender == OpenFMFolderMenuItem)
            {
                Core.OpenFMFolder(_owner.FMsDGV.GetMainSelectedFM());
            }
            else if (sender == ScanFMMenuItem)
            {
                await FMScan.ScanSelectedFMs();
            }
            else if (sender == ConvertWAVsTo16BitMenuItem || sender == ConvertOGGsToWAVsMenuItem)
            {
                await FMAudio.ConvertSelected(sender == ConvertWAVsTo16BitMenuItem
                    ? AudioConvert.WAVToWAV16
                    : AudioConvert.OGGToWAV);
            }
            else if (sender == PinToTopMenuItem ||
                     sender == ExplicitPinToTopMenuItem ||
                     sender == ExplicitUnpinFromTopMenuItem)
            {
                await Core.PinOrUnpinFM(pin: sender == PinToTopMenuItem
                    ? !_owner.FMsDGV.GetMainSelectedFM().Pinned
                    : sender == ExplicitPinToTopMenuItem);
            }
        }

        private void RatingMenuItems_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < RatingMenuItem.DropDownItems.Count; i++)
            {
                if (RatingMenuItem.DropDownItems[i] == sender)
                {
                    _owner.UpdateRatingForSelectedFMs(rating: i - 1);
                    break;
                }
            }
        }

        private void RatingRCMenuItems_CheckedChanged(object sender, EventArgs e)
        {
            var s = (ToolStripMenuItemCustom)sender;
            if (!s.Checked) return;

            foreach (ToolStripMenuItemCustom item in RatingMenuItem.DropDownItems)
            {
                if (item != s) item.Checked = false;
            }
        }

        private void FinishedOnMenuItems_Click(object sender, EventArgs e)
        {
            var senderItem = (ToolStripMenuItemCustom)sender;

            FanMission[] selFMs = _owner.FMsDGV.GetSelectedFMs();
            FanMission mainFM = _owner.FMsDGV.GetMainSelectedFM();

            if (selFMs.Length > 1 &&
                senderItem == FinishedOnUnknownMenuItem &&
                FinishedOnUnknownMenuItem.Checked)
            {
                bool doFinishedOnUnknown = Core.Dialogs.AskToContinue(
                    LText.AlertMessages.FinishedOnUnknown_MultiFMChange,
                    LText.AlertMessages.Alert,
                    defaultButton: MBoxButton.No
                );
                if (!doFinishedOnUnknown)
                {
                    SetFinishedOnMenuItemsChecked((Difficulty)mainFM.FinishedOn, mainFM.FinishedOnUnknown);
                    return;
                }
            }

            mainFM.FinishedOn = 0;
            mainFM.FinishedOnUnknown = false;

            if (senderItem == FinishedOnUnknownMenuItem)
            {
                mainFM.FinishedOnUnknown = senderItem.Checked;
            }
            else
            {
                uint at = 1;
                foreach (ToolStripMenuItemCustom item in FinishedOnMenu.Items)
                {
                    if (item == FinishedOnUnknownMenuItem) continue;

                    if (item.Checked) mainFM.FinishedOn |= at;
                    at <<= 1;
                }
                if (mainFM.FinishedOn > 0)
                {
                    FinishedOnUnknownMenuItem.Checked = false;
                    mainFM.FinishedOnUnknown = false;
                }
            }

            foreach (FanMission fm in selFMs)
            {
                if (fm == mainFM) continue;
                uint at = 1;
                foreach (ToolStripMenuItemCustom item in FinishedOnMenu.Items)
                {
                    if (item == FinishedOnUnknownMenuItem)
                    {
                        if (item.Checked)
                        {
                            fm.FinishedOn = 0;
                            fm.FinishedOnUnknown = true;
                        }
                        else
                        {
                            fm.FinishedOnUnknown = false;
                        }
                    }
                    else if (item == senderItem)
                    {
                        fm.FinishedOnUnknown = false;
                        if (item.Checked)
                        {
                            fm.FinishedOn |= at;
                        }
                        else
                        {
                            fm.FinishedOn &= ~at;
                        }
                        break;
                    }
                    at <<= 1;
                }
            }

            _owner.RefreshAllSelectedFMRows();

            Ini.WriteFullFMDataIni();
        }

        private void FinishedOnUnknownMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            if (FinishedOnUnknownMenuItem.Checked) UncheckFinishedOnMenuItemsExceptUnknown();
        }

        private void WebSearchMenuItem_Click(object sender, EventArgs e) => Core.OpenWebSearchUrl(_owner.FMsDGV.GetMainSelectedFM().Title);

        #endregion
    }
}
