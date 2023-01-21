﻿using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using AL_Common;
using AngelLoader.DataClasses;
using JetBrains.Annotations;
using static AngelLoader.GameSupport;
using static AngelLoader.Global;
using static AngelLoader.Misc;

namespace AngelLoader.Forms.CustomControls.LazyLoaded;

internal sealed class FMsDGV_FM_LLMenu : IDarkable
{
    #region Backing fields

    private bool _constructed;
    private bool _playFMMenuItemEnabled;
    private bool _playFMInMPMenuItemVisible;
    private bool _playFMInMPMenuItemEnabled;
    private bool _installUninstallMenuItemEnabled;
    private bool _deleteFMMenuItemEnabled;
    private bool _deleteFMMenuItemVisible;
    private bool _deleteFromDBMenuItemVisible;
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

    private bool _multiplePinnedStates;

    private bool _sayPin = true;

    #endregion

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
    private ToolStripMenuItemCustom DeleteFromDBMenuItem = null!;
    private ToolStripSeparator OpenInDromEdSep = null!;
    private ToolStripMenuItemCustom OpenInDromEdMenuItem = null!;
    private ToolStripSeparator OpenFMFolderSep = null!;
    private ToolStripMenuItemCustom OpenFMFolderMenuItem = null!;
    // @vNext(Copy FM folder path):
    // We could allow copying this even if the FM is not installed, because we can still construct it.
    // This would be so you could paste it into Thief Buddy without having to split your install/play task, like
    // you'd otherwise have to install first, then copy-paste the folder, then play if you wanted to use Thief Buddy
    // all the time.
    private ToolStripMenuItemCustom CopyFMFolderPathMenuItem = null!;
    private ToolStripMenuItemCustom ScanFMMenuItem = null!;
    private ToolStripMenuItemCustom ConvertAudioMenuItem = null!;
    private ToolStripMenuItemCustom ConvertWAVsTo16BitMenuItem = null!;
    private ToolStripMenuItemCustom ConvertOGGsToWAVsMenuItem = null!;
    private ToolStripMenuItemCustom RatingMenuItem = null!;
    private ToolStripMenuItemCustom RatingMenuUnrated = null!;
    private ToolStripMenuItemCustom FinishedOnMenuItem = null!;
    private DarkContextMenu RatingMenu = null!;
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
        set
        {
            if (_darkModeEnabled == value) return;
            _darkModeEnabled = value;
            if (!_constructed) return;

            _menu.DarkModeEnabled = _darkModeEnabled;
            RatingMenu.DarkModeEnabled = _darkModeEnabled;
            FinishedOnMenu.DarkModeEnabled = _darkModeEnabled;

            DeleteFMMenuItem.Image = Images.Trash;
            DeleteFromDBMenuItem.Image = Images.DeleteFromDB;
            PinToTopMenuItem.Image = _sayPin ? Images.Pin : Images.Unpin;
            ExplicitPinToTopMenuItem.Image = Images.Pin;
            ExplicitUnpinFromTopMenuItem.Image = Images.Unpin;
            OpenFMFolderMenuItem.Image = Images.Folder;
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

        _menu = new DarkContextMenu(_owner) { Tag = LoadType.Lazy };
        RatingMenu = new DarkContextMenu(_owner) { Tag = LoadType.Lazy };
        FinishedOnMenu = new DarkContextMenu(_owner) { Tag = LoadType.Lazy };

        #endregion

        #region Add items to menu

        _menu.Items.AddRange(new ToolStripItem[]
        {
            PlayFMMenuItem = new ToolStripMenuItemCustom(),
            PlayFMInMPMenuItem = new ToolStripMenuItemCustom(),
            InstallUninstallMenuItem = new ToolStripMenuItemCustom(),
            new ToolStripSeparator(),
            PinToTopMenuItem = new ToolStripMenuItemCustom(),
            ExplicitPinToTopMenuItem = new ToolStripMenuItemCustom { Image = Images.Pin, Visible = false },
            ExplicitUnpinFromTopMenuItem = new ToolStripMenuItemCustom { Image = Images.Unpin, Visible = false },
            new ToolStripSeparator(),
            DeleteFMMenuItem = new ToolStripMenuItemCustom { Image = Images.Trash },
            DeleteFromDBMenuItem = new ToolStripMenuItemCustom { Image = Images.DeleteFromDB },
            OpenInDromEdSep = new ToolStripSeparator(),
            OpenInDromEdMenuItem = new ToolStripMenuItemCustom(),
            OpenFMFolderSep = new ToolStripSeparator(),
            OpenFMFolderMenuItem = new ToolStripMenuItemCustom { Image = Images.Folder },
            CopyFMFolderPathMenuItem = new ToolStripMenuItemCustom(),
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

        RatingMenu.Items.Add(RatingMenuUnrated = new ToolStripMenuItemCustom { CheckOnClick = true });
        for (int i = 0; i < 11; i++)
        {
            RatingMenu.Items.Add(new ToolStripMenuItemCustom { CheckOnClick = true });
        }

        RatingMenuItem.DropDown = RatingMenu;

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

        _menu.Opening += MenuOpening;
        PlayFMMenuItem.Click += AsyncMenuItems_Click;
        PlayFMInMPMenuItem.Click += AsyncMenuItems_Click;
        InstallUninstallMenuItem.Click += AsyncMenuItems_Click;
        PinToTopMenuItem.Click += AsyncMenuItems_Click;
        ExplicitPinToTopMenuItem.Click += AsyncMenuItems_Click;
        ExplicitUnpinFromTopMenuItem.Click += AsyncMenuItems_Click;
        DeleteFMMenuItem.Click += AsyncMenuItems_Click;
        DeleteFromDBMenuItem.Click += AsyncMenuItems_Click;
        OpenInDromEdMenuItem.Click += AsyncMenuItems_Click;
        OpenFMFolderMenuItem.Click += AsyncMenuItems_Click;
        CopyFMFolderPathMenuItem.Click += AsyncMenuItems_Click;
        ScanFMMenuItem.Click += AsyncMenuItems_Click;
        ConvertWAVsTo16BitMenuItem.Click += AsyncMenuItems_Click;
        ConvertOGGsToWAVsMenuItem.Click += AsyncMenuItems_Click;

        foreach (ToolStripMenuItemCustom item in RatingMenu.Items)
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
        DeleteFMMenuItem.Visible = _deleteFMMenuItemVisible;
        DeleteFromDBMenuItem.Visible = _deleteFromDBMenuItemVisible;

        OpenInDromEdSep.Visible = _openInDromEdMenuItemVisible;
        OpenInDromEdMenuItem.Visible = _openInDromEdMenuItemVisible;
        OpenInDromEdMenuItem.Enabled = _openInDromedMenuItemEnabled;

        OpenFMFolderSep.Visible = _openFMFolderMenuItemVisible;
        OpenFMFolderMenuItem.Visible = _openFMFolderMenuItemVisible;
        CopyFMFolderPathMenuItem.Visible = _openFMFolderMenuItemVisible;

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

        _menu.DarkModeEnabled = _darkModeEnabled;
        FinishedOnMenu.DarkModeEnabled = _darkModeEnabled;

        _constructed = true;

        // These must come after the constructed bool gets set to true
        SetPinItemsMode(_multiplePinnedStates);
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
        SetDeleteFromDBMenuItemText(multiSelected);

        SetOpenInDromEdMenuItemText(sayShockEd);

        OpenFMFolderMenuItem.Text = LText.FMsList.FMMenu_OpenFMFolder;
        CopyFMFolderPathMenuItem.Text = LText.FMsList.FMMenu_CopyFMFolderPath;

        SetScanFMText(multiSelected);

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

    internal void SetScanFMText(bool multiSelected)
    {
        if (!_constructed) return;

        ScanFMMenuItem.Text = multiSelected ? LText.FMsList.FMMenu_ScanFMs : LText.FMsList.FMMenu_ScanFM;
    }

    internal void UpdateRatingList(bool fmSelStyle)
    {
        if (!_constructed) return;

        for (int i = 0; i <= 10; i++)
        {
            string num = (fmSelStyle ? i / 2.0 : i).ToString(CultureInfo.CurrentCulture);
            RatingMenu.Items[i + 1].Text = num;
        }
    }

    internal ContextMenuStrip GetFinishedOnMenu()
    {
        Construct();
        return FinishedOnMenu;
    }

    internal ContextMenuStrip GetRatingMenu()
    {
        Construct();
        return RatingMenu;
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

        PinToTopMenuItem.Image = sayPin ? Images.Pin : Images.Unpin;
    }

    internal void SetPinItemsMode(bool multiplePinnedStates)
    {
        _multiplePinnedStates = multiplePinnedStates;

        if (!_constructed) return;

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

    internal void SetDeleteFMMenuItemVisible(bool value)
    {
        if (_constructed)
        {
            DeleteFMMenuItem.Visible = value;
        }
        else
        {
            _deleteFMMenuItemVisible = value;
        }
    }

    internal void SetDeleteFMMenuItemText(bool multiSelected)
    {
        if (!_constructed) return;

        DeleteFMMenuItem.Text = multiSelected
            ? LText.FMsList.FMMenu_DeleteFMs
            : LText.FMsList.FMMenu_DeleteFM;
    }

    internal void SetDeleteFromDBMenuItemVisible(bool value)
    {
        if (_constructed)
        {
            DeleteFromDBMenuItem.Visible = value;
        }
        else
        {
            _deleteFromDBMenuItemVisible = value;
        }
    }

    internal void SetDeleteFromDBMenuItemText(bool multiSelected)
    {
        if (!_constructed) return;

        DeleteFromDBMenuItem.Text = multiSelected
            ? LText.FMsList.FMMenu_DeleteFMsFromDB
            : LText.FMsList.FMMenu_DeleteFMFromDB;
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
            CopyFMFolderPathMenuItem.Visible = value;
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
        value = value.SetRatingClamped();

        if (_constructed)
        {
            ((ToolStripMenuItemCustom)RatingMenu.Items[value + 1]).Checked = true;
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
            !_owner.FMsDGV.RowSelected())
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
            await FMDelete.DeleteFMsFromDisk(_owner.FMsDGV.GetSelectedFMs_InOrder_List());
            _owner.SetAvailableFMCount();
        }
        else if (sender == DeleteFromDBMenuItem)
        {
            await FMDelete.DeleteFMsFromDB(_owner.GetSelectedFMs_InOrder_List());
            _owner.SetAvailableFMCount();
        }
        else if (sender == OpenInDromEdMenuItem)
        {
            FanMission fm = _owner.FMsDGV.GetMainSelectedFM();
            if (fm.Installed || await FMInstallAndPlay.Install(fm)) FMInstallAndPlay.OpenFMInEditor(fm);
        }
        else if (sender == OpenFMFolderMenuItem)
        {
            Core.OpenFMFolder(_owner.FMsDGV.GetMainSelectedFM());
        }
        else if (sender == CopyFMFolderPathMenuItem)
        {
            FanMission fm = _owner.FMsDGV.GetMainSelectedFM();
            if (Utils.FMIsReallyInstalled(fm, out string fmInstalledPath))
            {
                // @DIRSEP: Backslashes to be a good citizen, the user might paste this anywhere
                Clipboard.SetText(fmInstalledPath.ToBackSlashes());
            }
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
        for (int i = 0; i < RatingMenu.Items.Count; i++)
        {
            if (RatingMenu.Items[i] == sender)
            {
                int rating = i - 1;

                FanMission[] sFMs = _owner.FMsDGV.GetSelectedFMs();
                if (sFMs.Length > 0)
                {
                    foreach (FanMission sFM in sFMs)
                    {
                        sFM.Rating = rating;
                    }
                    _owner.RefreshFMsListRowsOnlyKeepSelection();
                }
                Ini.WriteFullFMDataIni();

                break;
            }
        }
    }

    private void RatingRCMenuItems_CheckedChanged(object sender, EventArgs e)
    {
        var s = (ToolStripMenuItemCustom)sender;
        if (!s.Checked) return;

        foreach (ToolStripMenuItemCustom item in RatingMenu.Items)
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
            (MBoxButton result, _) = Core.Dialogs.ShowMultiChoiceDialog(
                message: LText.AlertMessages.FinishedOnUnknown_MultiFMChange,
                title: LText.AlertMessages.Alert,
                icon: MBoxIcon.None,
                yes: LText.Global.Yes,
                no: LText.Global.No,
                defaultButton: MBoxButton.No
            );
            if (result == MBoxButton.No)
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

        _owner.RefreshFMsListRowsOnlyKeepSelection();

        Ini.WriteFullFMDataIni();
    }

    private void FinishedOnUnknownMenuItem_CheckedChanged(object sender, EventArgs e)
    {
        if (FinishedOnUnknownMenuItem.Checked) UncheckFinishedOnMenuItemsExceptUnknown();
    }

    private void WebSearchMenuItem_Click(object sender, EventArgs e) => Core.OpenWebSearchUrl(_owner.FMsDGV.GetMainSelectedFM().Title);

    #endregion
}