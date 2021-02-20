using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using AL_Common;
using AngelLoader.DataClasses;
using AngelLoader.Properties;
using DarkUI.Controls;
using static AngelLoader.GameSupport;
using static AngelLoader.Misc;

namespace AngelLoader.Forms.CustomControls.Static_LazyLoaded
{
    internal static class FMsDGV_FM_LLMenu
    {
        #region Backing fields

        private static bool _constructed;
        private static bool _installUninstallMenuItemEnabled;
        private static bool _playFMMenuItemEnabled;
        private static bool _scanFMMenuItemEnabled;
        private static bool _openInDromEdSepVisible;
        private static bool _openInDromEdMenuItemVisible;
        private static bool _openInDromedMenuItemEnabled;
        private static bool _playFMInMPMenuItemVisible;
        private static bool _playFMInMPMenuItemEnabled;
        private static bool _convertAudioSubMenuEnabled;
        private static bool _deleteFMMenuItemEnabled;
        private static int _rating = -1;
        private static bool _finishedOnNormalChecked;
        private static bool _finishedOnHardChecked;
        private static bool _finishedOnExpertChecked;
        private static bool _finishedOnExtremeChecked;
        private static bool _finishedOnUnknownChecked;

        #endregion

        #region FM context menu fields

        private static MainForm _owner = null!;

        internal static DarkContextMenu? FMContextMenu;

        private static ToolStripMenuItemCustom? PlayFMMenuItem;
        private static ToolStripMenuItemCustom? PlayFMInMPMenuItem;
        private static ToolStripMenuItemCustom? InstallUninstallMenuItem;
        private static ToolStripMenuItemCustom? DeleteFMMenuItem;
        private static ToolStripSeparator? OpenInDromEdSep;
        private static ToolStripMenuItemCustom? OpenInDromEdMenuItem;
        private static ToolStripMenuItemCustom? ScanFMMenuItem;
        private static ToolStripMenuItemCustom? ConvertAudioMenuItem;
        private static ToolStripMenuItemCustom? ConvertWAVsTo16BitMenuItem;
        private static ToolStripMenuItemCustom? ConvertOGGsToWAVsMenuItem;
        private static ToolStripMenuItemCustom? RatingMenuItem;
        private static ToolStripMenuItemCustom? RatingMenuUnrated;
        private static ToolStripMenuItemCustom? FinishedOnMenuItem;
        private static ContextMenuStripCustom? FinishedOnMenu;
        private static ToolStripMenuItemCustom? FinishedOnNormalMenuItem;
        private static ToolStripMenuItemCustom? FinishedOnHardMenuItem;
        private static ToolStripMenuItemCustom? FinishedOnExpertMenuItem;
        private static ToolStripMenuItemCustom? FinishedOnExtremeMenuItem;
        private static ToolStripMenuItemCustom? FinishedOnUnknownMenuItem;
        private static ToolStripMenuItemCustom? WebSearchMenuItem;

        #endregion

        #region Private methods

        private static bool _darkModeEnabled;
        public static bool DarkModeEnabled
        {
            get => _darkModeEnabled;
            set
            {
                _darkModeEnabled = value;
                if (!_constructed) return;

                FMContextMenu!.DarkModeEnabled = _darkModeEnabled;
                FinishedOnMenu!.DarkModeEnabled = _darkModeEnabled;
            }
        }

        internal static void Construct(MainForm owner)
        {
            if (_constructed) return;

            _owner = owner;

            #region Instantiation

            FMContextMenu = new DarkContextMenu(_darkModeEnabled, _owner.GetComponents());
            FinishedOnMenu = new ContextMenuStripCustom(_darkModeEnabled, _owner.GetComponents());

            #endregion

            #region Add items to menu

            FMContextMenu.Items.AddRange(new ToolStripItem[]
            {
                PlayFMMenuItem = new ToolStripMenuItemCustom(),
                PlayFMInMPMenuItem = new ToolStripMenuItemCustom(),
                InstallUninstallMenuItem = new ToolStripMenuItemCustom(),
                new ToolStripSeparator(),
                DeleteFMMenuItem = new ToolStripMenuItemCustom { Image = Resources.Trash_16 },
                OpenInDromEdSep = new ToolStripSeparator(),
                OpenInDromEdMenuItem = new ToolStripMenuItemCustom(),
                new ToolStripSeparator(),
                ScanFMMenuItem = new ToolStripMenuItemCustom(),
                ConvertAudioMenuItem = new ToolStripMenuItemCustom(),
                new ToolStripSeparator(),
                RatingMenuItem = new ToolStripMenuItemCustom(),
                FinishedOnMenuItem = new ToolStripMenuItemCustom(),
                new ToolStripSeparator(),
                WebSearchMenuItem = new ToolStripMenuItemCustom()
            });

            ConvertAudioMenuItem.DropDownItems.AddRange(new ToolStripItem[]
            {
                ConvertWAVsTo16BitMenuItem = new ToolStripMenuItemCustom(),
                ConvertOGGsToWAVsMenuItem = new ToolStripMenuItemCustom()
            });

            RatingMenuItem.DropDownItems.Add(RatingMenuUnrated = new ToolStripMenuItemCustom { CheckOnClick = true });
            for (int i = 0; i < 11; i++)
            {
                RatingMenuItem.DropDownItems.Add(new ToolStripMenuItemCustom { CheckOnClick = true });
            }

            FinishedOnMenu.Items.AddRange(new ToolStripItem[]
            {
                FinishedOnNormalMenuItem = new ToolStripMenuItemCustom { CheckOnClick = true },
                FinishedOnHardMenuItem = new ToolStripMenuItemCustom { CheckOnClick = true },
                FinishedOnExpertMenuItem = new ToolStripMenuItemCustom { CheckOnClick = true },
                FinishedOnExtremeMenuItem = new ToolStripMenuItemCustom { CheckOnClick = true },
                FinishedOnUnknownMenuItem = new ToolStripMenuItemCustom { CheckOnClick = true }
            });

            FinishedOnMenu.SetPreventCloseOnClickItems(FinishedOnMenu.Items.Cast<ToolStripMenuItemCustom>().ToArray());
            FinishedOnMenuItem.DropDown = FinishedOnMenu;

            #endregion

            #region Event hookups

            FMContextMenu.Opening += FMContextMenu_Opening;
            PlayFMMenuItem.Click += AsyncMenuItems_Click;
            PlayFMInMPMenuItem.Click += AsyncMenuItems_Click;
            InstallUninstallMenuItem.Click += AsyncMenuItems_Click;
            DeleteFMMenuItem.Click += AsyncMenuItems_Click;
            OpenInDromEdMenuItem.Click += AsyncMenuItems_Click;
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

            InstallUninstallMenuItem.Enabled = _installUninstallMenuItemEnabled;
            DeleteFMMenuItem.Enabled = _deleteFMMenuItemEnabled;
            PlayFMMenuItem.Enabled = _playFMMenuItemEnabled;
            PlayFMInMPMenuItem.Visible = _playFMInMPMenuItemVisible;
            PlayFMInMPMenuItem.Enabled = _playFMInMPMenuItemEnabled;
            ScanFMMenuItem.Enabled = _scanFMMenuItemEnabled;
            OpenInDromEdSep.Visible = _openInDromEdSepVisible;
            OpenInDromEdMenuItem.Visible = _openInDromEdMenuItemVisible;
            OpenInDromEdMenuItem.Enabled = _openInDromedMenuItemEnabled;
            ConvertAudioMenuItem.Enabled = _convertAudioSubMenuEnabled;

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
            UpdateRatingList(Config.RatingDisplayStyle == RatingDisplayStyle.FMSel);
            SetRatingMenuItemChecked(_rating);
            SetFMMenuTextToLocalized();
        }

        private static void UncheckFinishedOnMenuItemsExceptUnknown()
        {
            if (_constructed)
            {
                FinishedOnNormalMenuItem!.Checked = false;
                FinishedOnHardMenuItem!.Checked = false;
                FinishedOnExpertMenuItem!.Checked = false;
                FinishedOnExtremeMenuItem!.Checked = false;
            }
            else
            {
                _finishedOnNormalChecked = false;
                _finishedOnHardChecked = false;
                _finishedOnExpertChecked = false;
                _finishedOnExtremeChecked = false;
            }
        }

        internal static void SetFMMenuTextToLocalized()
        {
            if (!_constructed) return;

            #region Get current FM info

            // Some menu items' text depends on FM state. Because this could be run after startup, we need to
            // make sure those items' text is set correctly.
            FanMission? selFM = _owner.FMsDGV.SelectedRows.Count > 0 ? _owner.FMsDGV.GetSelectedFM() : null;
            bool sayInstall = selFM == null || !selFM.Installed;
            // @GENGAMES - Localize FM context menu - "sayShockEd"
            bool sayShockEd = selFM != null && selFM.Game == Game.SS2;

            #endregion

            #region Play

            PlayFMMenuItem!.Text = LText.FMsList.FMMenu_PlayFM;
            PlayFMInMPMenuItem!.Text = LText.FMsList.FMMenu_PlayFM_Multiplayer;

            #endregion

            SetInstallUninstallMenuItemText(sayInstall);

            DeleteFMMenuItem!.Text = LText.FMsList.FMMenu_DeleteFM;

            SetOpenInDromEdMenuItemText(sayShockEd);

            ScanFMMenuItem!.Text = LText.FMsList.FMMenu_ScanFM;

            #region Convert audio submenu

            ConvertAudioMenuItem!.Text = LText.FMsList.FMMenu_ConvertAudio;
            ConvertWAVsTo16BitMenuItem!.Text = LText.FMsList.ConvertAudioMenu_ConvertWAVsTo16Bit;
            ConvertOGGsToWAVsMenuItem!.Text = LText.FMsList.ConvertAudioMenu_ConvertOGGsToWAVs;

            #endregion

            #region Rating submenu

            RatingMenuItem!.Text = LText.FMsList.FMMenu_Rating;
            RatingMenuUnrated!.Text = LText.Global.Unrated;

            #endregion

            #region Finished On submenu

            FinishedOnMenuItem!.Text = LText.FMsList.FMMenu_FinishedOn;

            SetGameSpecificFinishedOnMenuItemsText(selFM?.Game ?? Game.Null);
            FinishedOnUnknownMenuItem!.Text = LText.Difficulties.Unknown;

            #endregion

            WebSearchMenuItem!.Text = LText.FMsList.FMMenu_WebSearch;
        }

        private static void SetFinishedOnUnknownMenuItemChecked(bool value)
        {
            if (_constructed)
            {
                FinishedOnUnknownMenuItem!.Checked = value;
            }
            else
            {
                _finishedOnUnknownChecked = value;
            }

            if (value) UncheckFinishedOnMenuItemsExceptUnknown();
        }

        private static void SetFinishedOnMenuItemChecked(Difficulty difficulty, bool value)
        {
            if (value && !_constructed) _finishedOnUnknownChecked = false;

            switch (difficulty)
            {
                case Difficulty.Normal:
                    if (_constructed)
                    {
                        FinishedOnNormalMenuItem!.Checked = value;
                    }
                    else
                    {
                        _finishedOnNormalChecked = value;
                    }
                    break;
                case Difficulty.Hard:
                    if (_constructed)
                    {
                        FinishedOnHardMenuItem!.Checked = value;
                    }
                    else
                    {
                        _finishedOnHardChecked = value;
                    }
                    break;
                case Difficulty.Expert:
                    if (_constructed)
                    {
                        FinishedOnExpertMenuItem!.Checked = value;
                    }
                    else
                    {
                        _finishedOnExpertChecked = value;
                    }
                    break;
                case Difficulty.Extreme:
                    if (_constructed)
                    {
                        FinishedOnExtremeMenuItem!.Checked = value;
                    }
                    else
                    {
                        _finishedOnExtremeChecked = value;
                    }
                    break;
            }
        }

        #endregion

        #region API methods

        internal static bool Visible => _constructed && FMContextMenu?.Visible == true;

        internal static void UpdateRatingList(bool fmSelStyle)
        {
            if (!_constructed) return;

            for (int i = 0; i <= 10; i++)
            {
                string num = (fmSelStyle ? i / 2.0 : i).ToString(CultureInfo.CurrentCulture);
                RatingMenuItem!.DropDownItems[i + 1].Text = num;
            }
        }

        internal static ContextMenuStrip GetFinishedOnMenu(MainForm owner)
        {
            Construct(owner);
            return FinishedOnMenu!;
        }

        internal static void SetPlayFMMenuItemEnabled(bool value)
        {
            if (_constructed)
            {
                PlayFMMenuItem!.Enabled = value;
            }
            else
            {
                _playFMMenuItemEnabled = value;
            }
        }

        internal static void SetPlayFMInMPMenuItemVisible(bool value)
        {
            if (_constructed)
            {
                PlayFMInMPMenuItem!.Visible = value;
            }
            else
            {
                _playFMInMPMenuItemVisible = value;
            }
        }

        internal static void SetPlayFMInMPMenuItemEnabled(bool value)
        {
            if (_constructed)
            {
                PlayFMInMPMenuItem!.Enabled = value;
            }
            else
            {
                _playFMInMPMenuItemEnabled = value;
            }
        }

        internal static void SetInstallUninstallMenuItemEnabled(bool value)
        {
            if (_constructed)
            {
                InstallUninstallMenuItem!.Enabled = value;
            }
            else
            {
                _installUninstallMenuItemEnabled = value;
            }
        }

        internal static void SetInstallUninstallMenuItemText(bool sayInstall)
        {
            if (!_constructed) return;

            InstallUninstallMenuItem!.Text = sayInstall
                ? LText.FMsList.FMMenu_InstallFM
                : LText.FMsList.FMMenu_UninstallFM;
        }

        internal static void SetDeleteFMMenuItemEnabled(bool value)
        {
            if (_constructed)
            {
                DeleteFMMenuItem!.Enabled = value;
            }
            else
            {
                _deleteFMMenuItemEnabled = value;
            }
        }

        internal static void SetOpenInDromEdVisible(bool value)
        {
            if (_constructed)
            {
                OpenInDromEdSep!.Visible = value;
                OpenInDromEdMenuItem!.Visible = value;
            }
            else
            {
                _openInDromEdSepVisible = value;
                _openInDromEdMenuItemVisible = value;
            }
        }

        internal static void SetOpenInDromedEnabled(bool value)
        {
            if (_constructed)
            {
                OpenInDromEdMenuItem!.Enabled = value;
            }
            else
            {
                _openInDromedMenuItemEnabled = value;
            }
        }

        internal static void SetOpenInDromEdMenuItemText(bool sayShockEd)
        {
            if (!_constructed) return;

            OpenInDromEdMenuItem!.Text = sayShockEd
                ? LText.FMsList.FMMenu_OpenInShockEd
                : LText.FMsList.FMMenu_OpenInDromEd;
        }

        internal static void SetScanFMMenuItemEnabled(bool value)
        {
            if (_constructed)
            {
                ScanFMMenuItem!.Enabled = value;
            }
            else
            {
                _scanFMMenuItemEnabled = value;
            }
        }

        internal static void SetConvertAudioRCSubMenuEnabled(bool value)
        {
            if (_constructed)
            {
                ConvertAudioMenuItem!.Enabled = value;
            }
            else
            {
                _convertAudioSubMenuEnabled = value;
            }
        }

        internal static void SetRatingMenuItemChecked(int value)
        {
            value = value.Clamp(-1, 10);

            if (_constructed)
            {
                ((ToolStripMenuItemCustom)RatingMenuItem!.DropDownItems[value + 1]).Checked = true;
            }
            else
            {
                _rating = value;
            }
        }

        internal static void SetFinishedOnMenuItemsChecked(Difficulty difficulty, bool finishedOnUnknown)
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
        internal static void SetGameSpecificFinishedOnMenuItemsText(Game game)
        {
            if (!_constructed) return;

            FinishedOnNormalMenuItem!.Text = GetLocalizedDifficultyName(game, Difficulty.Normal);
            FinishedOnHardMenuItem!.Text = GetLocalizedDifficultyName(game, Difficulty.Hard);
            FinishedOnExpertMenuItem!.Text = GetLocalizedDifficultyName(game, Difficulty.Expert);
            FinishedOnExtremeMenuItem!.Text = GetLocalizedDifficultyName(game, Difficulty.Extreme);
        }

        internal static void ClearFinishedOnMenuItemChecks()
        {
            SetFinishedOnUnknownMenuItemChecked(false);
            UncheckFinishedOnMenuItemsExceptUnknown();
        }

        #endregion

        #region Event handlers

        private static void FMContextMenu_Opening(object sender, CancelEventArgs e)
        {
            // Fix for a corner case where the user could press the right mouse button, hold it, keyboard-switch
            // to an empty tab, then let up the mouse and a menu would come up even though no FM was selected.
            if (_owner.FMsDGV.RowCount == 0 || _owner.FMsDGV.SelectedRows.Count == 0) e.Cancel = true;
        }

        // Extra async/await avoidance
        private static async void AsyncMenuItems_Click(object sender, EventArgs e)
        {
            if (sender == PlayFMMenuItem || sender == PlayFMInMPMenuItem)
            {
                await FMInstallAndPlay.InstallIfNeededAndPlay(_owner.FMsDGV.GetSelectedFM(), playMP: sender == PlayFMInMPMenuItem);
            }
            else if (sender == InstallUninstallMenuItem)
            {
                await FMInstallAndPlay.InstallOrUninstall(_owner.FMsDGV.GetSelectedFM());
            }
            else if (sender == DeleteFMMenuItem)
            {
                await FMArchives.Delete(_owner.FMsDGV.GetSelectedFM());
            }
            else if (sender == OpenInDromEdMenuItem)
            {
                var fm = _owner.FMsDGV.GetSelectedFM();
                if (fm.Installed || await FMInstallAndPlay.InstallFM(fm)) FMInstallAndPlay.OpenFMInEditor(fm);
            }
            else if (sender == ScanFMMenuItem)
            {
                if (await FMScan.ScanFMs(new List<FanMission> { _owner.FMsDGV.GetSelectedFM() }, hideBoxIfZip: true))
                {
                    _owner.RefreshSelectedFM();
                }
            }
            else if (sender == ConvertWAVsTo16BitMenuItem || sender == ConvertOGGsToWAVsMenuItem)
            {
                var convertType = sender == ConvertWAVsTo16BitMenuItem ? AudioConvert.WAVToWAV16 : AudioConvert.OGGToWAV;
                await FMAudio.ConvertToWAVs(_owner.FMsDGV.GetSelectedFM(), convertType, true);
            }
        }

        private static void RatingMenuItems_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < RatingMenuItem!.DropDownItems.Count; i++)
            {
                if (RatingMenuItem.DropDownItems[i] == sender)
                {
                    int rating = i - 1;
                    _owner.FMsDGV.GetSelectedFM().Rating = rating;
                    _owner.RefreshSelectedFM(rowOnly: true);
                    _owner.UpdateRatingMenus(rating, disableEvents: true);
                    Ini.WriteFullFMDataIni();
                    break;
                }
            }
        }

        private static void RatingRCMenuItems_CheckedChanged(object sender, EventArgs e)
        {
            var s = (ToolStripMenuItemCustom)sender;
            if (!s.Checked) return;

            foreach (ToolStripMenuItemCustom item in RatingMenuItem!.DropDownItems)
            {
                if (item != s) item.Checked = false;
            }
        }

        private static void FinishedOnMenuItems_Click(object sender, EventArgs e)
        {
            var senderItem = (ToolStripMenuItemCustom)sender;

            var fm = _owner.FMsDGV.GetSelectedFM();

            fm.FinishedOn = 0;
            fm.FinishedOnUnknown = false;

            if (senderItem == FinishedOnUnknownMenuItem)
            {
                fm.FinishedOnUnknown = senderItem.Checked;
            }
            else
            {
                uint at = 1;
                foreach (ToolStripMenuItemCustom item in FinishedOnMenu!.Items)
                {
                    if (item == FinishedOnUnknownMenuItem) continue;

                    if (item.Checked) fm.FinishedOn |= at;
                    at <<= 1;
                }
                if (fm.FinishedOn > 0)
                {
                    FinishedOnUnknownMenuItem!.Checked = false;
                    fm.FinishedOnUnknown = false;
                }
            }

            _owner.RefreshSelectedFM(rowOnly: true);
            Ini.WriteFullFMDataIni();
        }

        private static void FinishedOnUnknownMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            if (FinishedOnUnknownMenuItem!.Checked) UncheckFinishedOnMenuItemsExceptUnknown();
        }

        private static void WebSearchMenuItem_Click(object sender, EventArgs e) => Core.OpenWebSearchUrl(_owner.FMsDGV.GetSelectedFM().Title);

        #endregion
    }
}
