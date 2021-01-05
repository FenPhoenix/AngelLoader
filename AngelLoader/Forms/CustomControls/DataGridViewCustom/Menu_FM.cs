﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using AngelLoader.DataClasses;
using AngelLoader.Properties;
using static AngelLoader.GameSupport;
using static AngelLoader.Misc;

namespace AngelLoader.Forms.CustomControls
{
    public sealed partial class DataGridViewCustom
    {
        #region Backing fields

        private bool _fmMenuConstructed;
        private bool _installUninstallMenuItemEnabled;
        private bool _playFMMenuItemEnabled;
        private bool _scanFMMenuItemEnabled;
        private bool _openInDromEdSepVisible;
        private bool _openInDromEdMenuItemVisible;
        private bool _playFMInMPMenuItemVisible;
        private bool _convertAudioSubMenuEnabled;
        private bool _deleteFMMenuItemEnabled;
        private int _rating = -1;
        private bool _finishedOnNormalChecked;
        private bool _finishedOnHardChecked;
        private bool _finishedOnExpertChecked;
        private bool _finishedOnExtremeChecked;
        private bool _finishedOnUnknownChecked;

        #endregion

        #region FM context menu fields

        private ContextMenuStrip? FMContextMenu;

        private ToolStripMenuItemCustom? PlayFMMenuItem;
        private ToolStripMenuItemCustom? PlayFMInMPMenuItem;
        private ToolStripMenuItemCustom? InstallUninstallMenuItem;
        private ToolStripMenuItemCustom? DeleteFMMenuItem;
        private ToolStripSeparator? OpenInDromEdSep;
        private ToolStripMenuItemCustom? OpenInDromEdMenuItem;
        private ToolStripMenuItemCustom? ScanFMMenuItem;
        private ToolStripMenuItemCustom? ConvertAudioMenuItem;
        private ToolStripMenuItemCustom? ConvertWAVsTo16BitMenuItem;
        private ToolStripMenuItemCustom? ConvertOGGsToWAVsMenuItem;
        private ToolStripMenuItemCustom? RatingMenuItem;
        private ToolStripMenuItemCustom? RatingMenuUnrated;
        private ToolStripMenuItemCustom? FinishedOnMenuItem;
        private ContextMenuStripCustom? FinishedOnMenu;
        private ToolStripMenuItemCustom? FinishedOnNormalMenuItem;
        private ToolStripMenuItemCustom? FinishedOnHardMenuItem;
        private ToolStripMenuItemCustom? FinishedOnExpertMenuItem;
        private ToolStripMenuItemCustom? FinishedOnExtremeMenuItem;
        private ToolStripMenuItemCustom? FinishedOnUnknownMenuItem;
        private ToolStripMenuItemCustom? WebSearchMenuItem;

        #endregion

        #region Private methods

        private void ConstructFMContextMenu()
        {
            if (_fmMenuConstructed) return;

            #region Instantiation

            FMContextMenu = new ContextMenuStrip(_owner.GetComponents());
            FinishedOnMenu = new ContextMenuStripCustom(_owner.GetComponents());

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
            ScanFMMenuItem.Enabled = _scanFMMenuItemEnabled;
            OpenInDromEdSep.Visible = _openInDromEdSepVisible;
            OpenInDromEdMenuItem.Visible = _openInDromEdMenuItemVisible;
            ConvertAudioMenuItem.Enabled = _convertAudioSubMenuEnabled;

            #endregion

            #region Set Finished On checked values

            FinishedOnNormalMenuItem.Checked = _finishedOnNormalChecked;
            FinishedOnHardMenuItem.Checked = _finishedOnHardChecked;
            FinishedOnExpertMenuItem.Checked = _finishedOnExpertChecked;
            FinishedOnExtremeMenuItem.Checked = _finishedOnExtremeChecked;
            FinishedOnUnknownMenuItem.Checked = _finishedOnUnknownChecked;

            #endregion

            _fmMenuConstructed = true;

            // These must come after the constructed bool gets set to true
            UpdateRatingList(Config.RatingDisplayStyle == RatingDisplayStyle.FMSel);
            SetRatingMenuItemChecked(_rating);
            SetFMMenuTextToLocalized();
        }

        private void UncheckFinishedOnMenuItemsExceptUnknown()
        {
            if (_fmMenuConstructed)
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

        private void SetFMMenuTextToLocalized()
        {
            if (!_fmMenuConstructed) return;

            #region Get current FM info

            // Some menu items' text depends on FM state. Because this could be run after startup, we need to
            // make sure those items' text is set correctly.
            FanMission? selFM = SelectedRows.Count > 0 ? GetSelectedFM() : null;
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

        private void SetFinishedOnUnknownMenuItemChecked(bool value)
        {
            if (_fmMenuConstructed)
            {
                FinishedOnUnknownMenuItem!.Checked = value;
            }
            else
            {
                _finishedOnUnknownChecked = value;
            }

            if (value) UncheckFinishedOnMenuItemsExceptUnknown();
        }

        private void SetFinishedOnMenuItemChecked(Difficulty difficulty, bool value)
        {
            if (value && !_fmMenuConstructed) _finishedOnUnknownChecked = false;

            switch (difficulty)
            {
                case Difficulty.Normal:
                    if (_fmMenuConstructed)
                    {
                        FinishedOnNormalMenuItem!.Checked = value;
                    }
                    else
                    {
                        _finishedOnNormalChecked = value;
                    }
                    break;
                case Difficulty.Hard:
                    if (_fmMenuConstructed)
                    {
                        FinishedOnHardMenuItem!.Checked = value;
                    }
                    else
                    {
                        _finishedOnHardChecked = value;
                    }
                    break;
                case Difficulty.Expert:
                    if (_fmMenuConstructed)
                    {
                        FinishedOnExpertMenuItem!.Checked = value;
                    }
                    else
                    {
                        _finishedOnExpertChecked = value;
                    }
                    break;
                case Difficulty.Extreme:
                    if (_fmMenuConstructed)
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

        internal void UpdateRatingList(bool fmSelStyle)
        {
            if (!_fmMenuConstructed) return;

            for (int i = 0; i <= 10; i++)
            {
                string num = (fmSelStyle ? i / 2.0 : i).ToString(CultureInfo.CurrentCulture);
                RatingMenuItem!.DropDownItems[i + 1].Text = num;
            }
        }

        internal ContextMenuStrip GetFinishedOnMenu()
        {
            ConstructFMContextMenu();
            return FinishedOnMenu!;
        }

        internal void SetPlayFMMenuItemEnabled(bool value)
        {
            if (_fmMenuConstructed)
            {
                PlayFMMenuItem!.Enabled = value;
            }
            else
            {
                _playFMMenuItemEnabled = value;
            }
        }

        internal void SetPlayFMInMPMenuItemVisible(bool value)
        {
            if (_fmMenuConstructed)
            {
                PlayFMInMPMenuItem!.Visible = value;
            }
            else
            {
                _playFMInMPMenuItemVisible = value;
            }
        }

        internal void SetInstallUninstallMenuItemEnabled(bool value)
        {
            if (_fmMenuConstructed)
            {
                InstallUninstallMenuItem!.Enabled = value;
            }
            else
            {
                _installUninstallMenuItemEnabled = value;
            }
        }

        internal void SetInstallUninstallMenuItemText(bool sayInstall)
        {
            if (!_fmMenuConstructed) return;

            InstallUninstallMenuItem!.Text = sayInstall
                ? LText.FMsList.FMMenu_InstallFM
                : LText.FMsList.FMMenu_UninstallFM;
        }

        internal void SetDeleteFMMenuItemEnabled(bool value)
        {
            if (_fmMenuConstructed)
            {
                DeleteFMMenuItem!.Enabled = value;
            }
            else
            {
                _deleteFMMenuItemEnabled = value;
            }
        }

        internal void SetOpenInDromEdVisible(bool value)
        {
            if (_fmMenuConstructed)
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

        internal void SetOpenInDromEdMenuItemText(bool sayShockEd)
        {
            if (!_fmMenuConstructed) return;

            OpenInDromEdMenuItem!.Text = sayShockEd
                ? LText.FMsList.FMMenu_OpenInShockEd
                : LText.FMsList.FMMenu_OpenInDromEd;
        }

        internal void SetScanFMMenuItemEnabled(bool value)
        {
            if (_fmMenuConstructed)
            {
                ScanFMMenuItem!.Enabled = value;
            }
            else
            {
                _scanFMMenuItemEnabled = value;
            }
        }

        internal void SetConvertAudioRCSubMenuEnabled(bool value)
        {
            if (_fmMenuConstructed)
            {
                ConvertAudioMenuItem!.Enabled = value;
            }
            else
            {
                _convertAudioSubMenuEnabled = value;
            }
        }

        internal void SetRatingMenuItemChecked(int value)
        {
            value = value.Clamp(-1, 10);

            if (_fmMenuConstructed)
            {
                ((ToolStripMenuItemCustom)RatingMenuItem!.DropDownItems[value + 1]).Checked = true;
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
            if (!_fmMenuConstructed) return;

            FinishedOnNormalMenuItem!.Text = GetLocalizedDifficultyName(game, Difficulty.Normal);
            FinishedOnHardMenuItem!.Text = GetLocalizedDifficultyName(game, Difficulty.Hard);
            FinishedOnExpertMenuItem!.Text = GetLocalizedDifficultyName(game, Difficulty.Expert);
            FinishedOnExtremeMenuItem!.Text = GetLocalizedDifficultyName(game, Difficulty.Extreme);
        }

        internal void ClearFinishedOnMenuItemChecks()
        {
            SetFinishedOnUnknownMenuItemChecked(false);
            UncheckFinishedOnMenuItemsExceptUnknown();
        }

        #endregion

        #region Event handlers

        private void FMContextMenu_Opening(object sender, CancelEventArgs e)
        {
            // Fix for a corner case where the user could press the right mouse button, hold it, keyboard-switch
            // to an empty tab, then let up the mouse and a menu would come up even though no FM was selected.
            if (RowCount == 0 || SelectedRows.Count == 0) e.Cancel = true;
        }

        // Extra async/await avoidance
        private async void AsyncMenuItems_Click(object sender, EventArgs e)
        {
            if (sender == PlayFMMenuItem || sender == PlayFMInMPMenuItem)
            {
                await FMInstallAndPlay.InstallIfNeededAndPlay(GetSelectedFM(), playMP: sender == PlayFMInMPMenuItem);
            }
            else if (sender == InstallUninstallMenuItem)
            {
                await FMInstallAndPlay.InstallOrUninstall(GetSelectedFM());
            }
            else if (sender == DeleteFMMenuItem)
            {
                await FMArchives.Delete(GetSelectedFM());
            }
            else if (sender == OpenInDromEdMenuItem)
            {
                var fm = GetSelectedFM();
                if (fm.Installed || await FMInstallAndPlay.InstallFM(fm)) FMInstallAndPlay.OpenFMInEditor(fm);
            }
            else if (sender == ScanFMMenuItem)
            {
                if (await FMScan.ScanFMs(new List<FanMission> { GetSelectedFM() }, hideBoxIfZip: true))
                {
                    _owner.RefreshSelectedFM();
                }
            }
            else if (sender == ConvertWAVsTo16BitMenuItem || sender == ConvertOGGsToWAVsMenuItem)
            {
                var convertType = sender == ConvertWAVsTo16BitMenuItem ? AudioConvert.WAVToWAV16 : AudioConvert.OGGToWAV;
                await FMAudio.ConvertToWAVs(GetSelectedFM(), convertType, true);
            }
        }

        private void RatingMenuItems_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < RatingMenuItem!.DropDownItems.Count; i++)
            {
                if (RatingMenuItem.DropDownItems[i] == sender)
                {
                    int rating = i - 1;
                    GetSelectedFM().Rating = rating;
                    _owner.RefreshSelectedFM(rowOnly: true);
                    _owner.UpdateRatingMenus(rating, disableEvents: true);
                    Ini.WriteFullFMDataIni();
                    break;
                }
            }
        }

        private void RatingRCMenuItems_CheckedChanged(object sender, EventArgs e)
        {
            var s = (ToolStripMenuItemCustom)sender;
            if (!s.Checked) return;

            foreach (ToolStripMenuItemCustom item in RatingMenuItem!.DropDownItems)
            {
                if (item != s) item.Checked = false;
            }
        }

        private void FinishedOnMenuItems_Click(object sender, EventArgs e)
        {
            var senderItem = (ToolStripMenuItemCustom)sender;

            var fm = GetSelectedFM();

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

        private void FinishedOnUnknownMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            if (FinishedOnUnknownMenuItem!.Checked) UncheckFinishedOnMenuItemsExceptUnknown();
        }

        private void WebSearchMenuItem_Click(object sender, EventArgs e) => Core.OpenWebSearchUrl(GetSelectedFM().Title);

        #endregion
    }
}
