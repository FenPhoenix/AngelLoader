using System;
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
        private bool _installUninstallMenuEnabled;
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

        private IDisposable[]? _fmContextMenuDisposables;

        #region FM context menu fields

#pragma warning disable IDE0069 // Disposable fields should be disposed

        // These are disposed by adding them to an array and iterating through it in Dispose()
        // TODO: This probably doesn't even need to happen, as they prolly get dumped with everything else on app exit

        private ContextMenuStrip? FMContextMenu;

        private ToolStripMenuItem? PlayFMMenuItem;
        private ToolStripMenuItem? PlayFMInMPMenuItem;
        private ToolStripMenuItem? InstallUninstallMenuItem;

        private ToolStripSeparator? DeleteFMSep;

        private ToolStripMenuItem? DeleteFMMenuItem;

        private ToolStripSeparator? OpenInDromEdSep;

        private ToolStripMenuItem? OpenInDromEdMenuItem;

        private ToolStripSeparator? FMContextMenuSep1;

        private ToolStripMenuItem? ScanFMMenuItem;
        private ToolStripMenuItem? ConvertAudioMenuItem;
        private ToolStripMenuItem? ConvertWAVsTo16BitMenuItem;
        private ToolStripMenuItem? ConvertOGGsToWAVsMenuItem;

        private ToolStripSeparator? FMContextMenuSep2;

        private ToolStripMenuItem? RatingMenuItem;
        private ToolStripMenuItem? RatingMenuUnrated;
        private ToolStripMenuItem? Rating0MenuItem;
        private ToolStripMenuItem? Rating1MenuItem;
        private ToolStripMenuItem? Rating2MenuItem;
        private ToolStripMenuItem? Rating3MenuItem;
        private ToolStripMenuItem? Rating4MenuItem;
        private ToolStripMenuItem? Rating5MenuItem;
        private ToolStripMenuItem? Rating6MenuItem;
        private ToolStripMenuItem? Rating7MenuItem;
        private ToolStripMenuItem? Rating8MenuItem;
        private ToolStripMenuItem? Rating9MenuItem;
        private ToolStripMenuItem? Rating10MenuItem;

        private ToolStripMenuItem? FinishedOnMenuItem;
        private ContextMenuStripCustom? FinishedOnMenu;
        private ToolStripMenuItem? FinishedOnNormalMenuItem;
        private ToolStripMenuItem? FinishedOnHardMenuItem;
        private ToolStripMenuItem? FinishedOnExpertMenuItem;
        private ToolStripMenuItem? FinishedOnExtremeMenuItem;
        private ToolStripMenuItem? FinishedOnUnknownMenuItem;

        private ToolStripSeparator? FMContextMenuSep3;

        private ToolStripMenuItem? WebSearchMenuItem;

#pragma warning restore IDE0069 // Disposable fields should be disposed

        #endregion

        #region Private methods

        private void ConstructFMContextMenu()
        {
            if (_fmMenuConstructed) return;

            #region Instantiation

            _fmContextMenuDisposables = new IDisposable[]
            {
                FMContextMenu = new ContextMenuStrip(),
                PlayFMMenuItem = new ToolStripMenuItem(),
                PlayFMInMPMenuItem = new ToolStripMenuItem(),

                InstallUninstallMenuItem = new ToolStripMenuItem(),

                DeleteFMSep = new ToolStripSeparator(),
                DeleteFMMenuItem = new ToolStripMenuItem { Image = Resources.Trash_16 },

                OpenInDromEdSep = new ToolStripSeparator(),
                OpenInDromEdMenuItem = new ToolStripMenuItem(),

                FMContextMenuSep1 = new ToolStripSeparator(),

                ScanFMMenuItem = new ToolStripMenuItem(),

                ConvertAudioMenuItem = new ToolStripMenuItem(),
                ConvertWAVsTo16BitMenuItem = new ToolStripMenuItem(),
                ConvertOGGsToWAVsMenuItem = new ToolStripMenuItem(),

                FMContextMenuSep2 = new ToolStripSeparator(),

                RatingMenuItem = new ToolStripMenuItem(),
                RatingMenuUnrated = new ToolStripMenuItem { CheckOnClick = true },
                Rating0MenuItem = new ToolStripMenuItem { CheckOnClick = true },
                Rating1MenuItem = new ToolStripMenuItem { CheckOnClick = true },
                Rating2MenuItem = new ToolStripMenuItem { CheckOnClick = true },
                Rating3MenuItem = new ToolStripMenuItem { CheckOnClick = true },
                Rating4MenuItem = new ToolStripMenuItem { CheckOnClick = true },
                Rating5MenuItem = new ToolStripMenuItem { CheckOnClick = true },
                Rating6MenuItem = new ToolStripMenuItem { CheckOnClick = true },
                Rating7MenuItem = new ToolStripMenuItem { CheckOnClick = true },
                Rating8MenuItem = new ToolStripMenuItem { CheckOnClick = true },
                Rating9MenuItem = new ToolStripMenuItem { CheckOnClick = true },
                Rating10MenuItem = new ToolStripMenuItem { CheckOnClick = true },

                FinishedOnMenuItem = new ToolStripMenuItem(),
                FinishedOnMenu = new ContextMenuStripCustom(),
                FinishedOnNormalMenuItem = new ToolStripMenuItem { CheckOnClick = true },
                FinishedOnHardMenuItem = new ToolStripMenuItem { CheckOnClick = true },
                FinishedOnExpertMenuItem = new ToolStripMenuItem { CheckOnClick = true },
                FinishedOnExtremeMenuItem = new ToolStripMenuItem { CheckOnClick = true },
                FinishedOnUnknownMenuItem = new ToolStripMenuItem { CheckOnClick = true },

                FMContextMenuSep3 = new ToolStripSeparator(),

                WebSearchMenuItem = new ToolStripMenuItem()
            };

            #endregion

            #region Add items to menu

            FMContextMenu.Items.AddRange(new ToolStripItem[]
            {
                PlayFMMenuItem,
                PlayFMInMPMenuItem,
                InstallUninstallMenuItem,
                DeleteFMSep,
                DeleteFMMenuItem,
                OpenInDromEdSep,
                OpenInDromEdMenuItem,
                FMContextMenuSep1,
                ScanFMMenuItem,
                ConvertAudioMenuItem,
                FMContextMenuSep2,
                RatingMenuItem,
                FinishedOnMenuItem,
                FMContextMenuSep3,
                WebSearchMenuItem
            });

            ConvertAudioMenuItem.DropDownItems.AddRange(new ToolStripItem[]
            {
                ConvertWAVsTo16BitMenuItem,
                ConvertOGGsToWAVsMenuItem
            });

            RatingMenuItem.DropDownItems.AddRange(new ToolStripItem[]
            {
                RatingMenuUnrated,
                Rating0MenuItem,
                Rating1MenuItem,
                Rating2MenuItem,
                Rating3MenuItem,
                Rating4MenuItem,
                Rating5MenuItem,
                Rating6MenuItem,
                Rating7MenuItem,
                Rating8MenuItem,
                Rating9MenuItem,
                Rating10MenuItem
            });

            FinishedOnMenu.Items.AddRange(new ToolStripItem[]
            {
                FinishedOnNormalMenuItem,
                FinishedOnHardMenuItem,
                FinishedOnExpertMenuItem,
                FinishedOnExtremeMenuItem,
                FinishedOnUnknownMenuItem
            });

            FinishedOnMenu.SetPreventCloseOnClickItems(FinishedOnMenu.Items.Cast<ToolStripMenuItem>().ToArray());
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

            foreach (ToolStripMenuItem item in RatingMenuItem.DropDownItems)
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

            InstallUninstallMenuItem.Enabled = _installUninstallMenuEnabled;
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

            PlayFMMenuItem!.Text = LText.FMsList.FMMenu_PlayFM.EscapeAmpersands();
            PlayFMInMPMenuItem!.Text = LText.FMsList.FMMenu_PlayFM_Multiplayer.EscapeAmpersands();

            #endregion

            SetInstallUninstallMenuItemText(sayInstall);

            DeleteFMMenuItem!.Text = LText.FMsList.FMMenu_DeleteFM.EscapeAmpersands();

            SetOpenInDromEdMenuItemText(sayShockEd);

            ScanFMMenuItem!.Text = LText.FMsList.FMMenu_ScanFM.EscapeAmpersands();

            #region Convert audio submenu

            ConvertAudioMenuItem!.Text = LText.FMsList.FMMenu_ConvertAudio.EscapeAmpersands();
            ConvertWAVsTo16BitMenuItem!.Text = LText.FMsList.ConvertAudioMenu_ConvertWAVsTo16Bit.EscapeAmpersands();
            ConvertOGGsToWAVsMenuItem!.Text = LText.FMsList.ConvertAudioMenu_ConvertOGGsToWAVs.EscapeAmpersands();

            #endregion

            #region Rating submenu

            RatingMenuItem!.Text = LText.FMsList.FMMenu_Rating.EscapeAmpersands();
            RatingMenuUnrated!.Text = LText.Global.Unrated.EscapeAmpersands();

            #endregion

            #region Finished On submenu

            FinishedOnMenuItem!.Text = LText.FMsList.FMMenu_FinishedOn.EscapeAmpersands();

            SetGameSpecificFinishedOnMenuItemsText(selFM?.Game ?? Game.Null);
            FinishedOnUnknownMenuItem!.Text = LText.Difficulties.Unknown.EscapeAmpersands();

            #endregion

            WebSearchMenuItem!.Text = LText.FMsList.FMMenu_WebSearch.EscapeAmpersands();
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
                _installUninstallMenuEnabled = value;
            }
        }

        internal void SetInstallUninstallMenuItemText(bool sayInstall)
        {
            if (!_fmMenuConstructed) return;

            InstallUninstallMenuItem!.Text = (sayInstall
                ? LText.FMsList.FMMenu_InstallFM
                : LText.FMsList.FMMenu_UninstallFM).EscapeAmpersands();
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

            OpenInDromEdMenuItem!.Text = (sayShockEd
                ? LText.FMsList.FMMenu_OpenInShockEd
                : LText.FMsList.FMMenu_OpenInDromEd).EscapeAmpersands();
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
                ((ToolStripMenuItem)RatingMenuItem!.DropDownItems[value + 1]).Checked = true;
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

            FinishedOnNormalMenuItem!.Text = GetLocalizedDifficultyName(game, Difficulty.Normal).EscapeAmpersands();
            FinishedOnHardMenuItem!.Text = GetLocalizedDifficultyName(game, Difficulty.Hard).EscapeAmpersands();
            FinishedOnExpertMenuItem!.Text = GetLocalizedDifficultyName(game, Difficulty.Expert).EscapeAmpersands();
            FinishedOnExtremeMenuItem!.Text = GetLocalizedDifficultyName(game, Difficulty.Extreme).EscapeAmpersands();
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
            var s = (ToolStripMenuItem)sender;
            if (!s.Checked) return;

            foreach (ToolStripMenuItem item in RatingMenuItem!.DropDownItems)
            {
                if (item != s) item.Checked = false;
            }
        }

        private void FinishedOnMenuItems_Click(object sender, EventArgs e)
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
                uint at = 1;
                foreach (ToolStripMenuItem item in FinishedOnMenu!.Items)
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

        private void DisposeFMContextMenu()
        {
            for (int i = 0; i < _fmContextMenuDisposables?.Length; i++)
            {
                _fmContextMenuDisposables?[i]?.Dispose();
            }
        }
    }
}
