using System.Linq;
using System.Windows.Forms;
using AngelLoader.Common.DataClasses;
using static AngelLoader.Common.Common;

namespace AngelLoader.CustomControls
{
    public sealed partial class DataGridViewCustom
    {
        private bool _columnHeaderMenuCreated;
        private readonly bool[] ColumnCheckedStates = { true, true, true, true, true, true, true, true, true, true, true, true };

        private bool _fmMenuCreated;
        private bool _installUninstallMenuEnabled = true;
        private bool _sayInstall;
        private bool _playFMMenuItemEnabled = true;
        private bool _scanFMMenuItemEnabled = true;
        private bool _openInDromEdSepVisible;
        private bool _openInDromEdMenuItemVisible;
        private bool _playFMInMPMenuItemVisible;
        private bool _convertAudioSubMenuEnabled = true;
        private int _rating = -1;
        private bool _finishedOnNormalChecked;
        private bool _finishedOnHardChecked;
        private bool _finishedOnExpertChecked;
        private bool _finishedOnExtremeChecked;
        private bool _finishedOnUnknownChecked;

        private void InitColumnHeaderContextMenu()
        {
            if (_columnHeaderMenuCreated) return;

            #region Instantiation

            ColumnHeaderContextMenu = new ContextMenuStripCustom
            {
                Name = nameof(ColumnHeaderContextMenu)
            };
            ResetColumnVisibilityMenuItem = new ToolStripMenuItem
            {
                Name = nameof(ResetColumnVisibilityMenuItem)
            };
            ResetAllColumnWidthsMenuItem = new ToolStripMenuItem
            {
                Name = nameof(ResetAllColumnWidthsMenuItem)
            };
            ResetColumnPositionsMenuItem = new ToolStripMenuItem
            {
                Name = nameof(ResetColumnPositionsMenuItem)
            };
            ColumnHeaderContextMenuSep1 = new ToolStripSeparator
            {
                Name = nameof(ColumnHeaderContextMenuSep1)
            };
            ShowGameMenuItem = new ToolStripMenuItem
            {
                CheckOnClick = true,
                Name = nameof(ShowGameMenuItem),
                Tag = Column.Game
            };
            ShowInstalledMenuItem = new ToolStripMenuItem
            {
                CheckOnClick = true,
                Name = nameof(ShowInstalledMenuItem),
                Tag = Column.Installed
            };
            ShowTitleMenuItem = new ToolStripMenuItem
            {
                CheckOnClick = true,
                Name = nameof(ShowTitleMenuItem),
                Tag = Column.Title
            };
            ShowArchiveMenuItem = new ToolStripMenuItem
            {
                CheckOnClick = true,
                Name = nameof(ShowArchiveMenuItem),
                Tag = Column.Archive
            };
            ShowAuthorMenuItem = new ToolStripMenuItem
            {
                CheckOnClick = true,
                Name = nameof(ShowAuthorMenuItem),
                Tag = Column.Author
            };
            ShowSizeMenuItem = new ToolStripMenuItem
            {
                CheckOnClick = true,
                Name = nameof(ShowSizeMenuItem),
                Tag = Column.Size
            };
            ShowRatingMenuItem = new ToolStripMenuItem
            {
                CheckOnClick = true,
                Name = nameof(ShowRatingMenuItem),
                Tag = Column.Rating
            };
            ShowFinishedMenuItem = new ToolStripMenuItem
            {
                CheckOnClick = true,
                Name = nameof(ShowFinishedMenuItem),
                Tag = Column.Finished
            };
            ShowReleaseDateMenuItem = new ToolStripMenuItem
            {
                CheckOnClick = true,
                Name = nameof(ShowReleaseDateMenuItem),
                Tag = Column.ReleaseDate
            };
            ShowLastPlayedMenuItem = new ToolStripMenuItem
            {
                CheckOnClick = true,
                Name = nameof(ShowLastPlayedMenuItem),
                Tag = Column.LastPlayed
            };
            ShowDisabledModsMenuItem = new ToolStripMenuItem
            {
                CheckOnClick = true,
                Name = nameof(ShowDisabledModsMenuItem),
                Tag = Column.DisabledMods
            };
            ShowCommentMenuItem = new ToolStripMenuItem
            {
                CheckOnClick = true,
                Name = nameof(ShowCommentMenuItem),
                Tag = Column.Comment
            };

            #endregion

            #region Fill ColumnHeaderCheckBoxMenuItems array

            ColumnHeaderCheckBoxMenuItems = new[]
            {
                ShowGameMenuItem,
                ShowInstalledMenuItem,
                ShowTitleMenuItem,
                ShowArchiveMenuItem,
                ShowAuthorMenuItem,
                ShowSizeMenuItem,
                ShowRatingMenuItem,
                ShowFinishedMenuItem,
                ShowReleaseDateMenuItem,
                ShowLastPlayedMenuItem,
                ShowDisabledModsMenuItem,
                ShowCommentMenuItem
            };

            for (int i = 0; i < ColumnHeaderCheckBoxMenuItems.Length; i++)
            {
                ColumnHeaderCheckBoxMenuItems[i].Checked = ColumnCheckedStates[i];
            }

            #endregion

            #region Add items to menu

            ColumnHeaderContextMenu.Items.AddRange(new ToolStripItem[]
            {
                ResetColumnVisibilityMenuItem,
                ResetAllColumnWidthsMenuItem,
                ResetColumnPositionsMenuItem,
                ColumnHeaderContextMenuSep1,
                ShowGameMenuItem,
                ShowInstalledMenuItem,
                ShowTitleMenuItem,
                ShowArchiveMenuItem,
                ShowAuthorMenuItem,
                ShowSizeMenuItem,
                ShowRatingMenuItem,
                ShowFinishedMenuItem,
                ShowReleaseDateMenuItem,
                ShowLastPlayedMenuItem,
                ShowDisabledModsMenuItem,
                ShowCommentMenuItem
            });

            #endregion

            ColumnHeaderContextMenu.SetPreventCloseOnClickItems(ColumnHeaderCheckBoxMenuItems);

            #region Event hookups

            ResetColumnVisibilityMenuItem.Click += ResetColumnVisibilityMenuItem_Click;
            ResetAllColumnWidthsMenuItem.Click += ResetAllColumnWidthsMenuItem_Click;
            ResetColumnPositionsMenuItem.Click += ResetColumnPositionsMenuItem_Click;

            ShowGameMenuItem.Click += CheckBoxMenuItem_Click;
            ShowInstalledMenuItem.Click += CheckBoxMenuItem_Click;
            ShowTitleMenuItem.Click += CheckBoxMenuItem_Click;
            ShowArchiveMenuItem.Click += CheckBoxMenuItem_Click;
            ShowAuthorMenuItem.Click += CheckBoxMenuItem_Click;
            ShowSizeMenuItem.Click += CheckBoxMenuItem_Click;
            ShowRatingMenuItem.Click += CheckBoxMenuItem_Click;
            ShowFinishedMenuItem.Click += CheckBoxMenuItem_Click;
            ShowReleaseDateMenuItem.Click += CheckBoxMenuItem_Click;
            ShowLastPlayedMenuItem.Click += CheckBoxMenuItem_Click;
            ShowDisabledModsMenuItem.Click += CheckBoxMenuItem_Click;
            ShowCommentMenuItem.Click += CheckBoxMenuItem_Click;

            #endregion

            _columnHeaderMenuCreated = true;

            SetColumnHeaderMenuItemTextToLocalized();
        }

        internal void InitFMContextMenu()
        {
            if (_fmMenuCreated) return;

            #region Instantiation

            FMContextMenu = new ContextMenuStrip { Name = nameof(FMContextMenu) };
            PlayFMMenuItem = new ToolStripMenuItem { Name = nameof(PlayFMMenuItem) };
            PlayFMInMPMenuItem = new ToolStripMenuItem { Name = nameof(PlayFMInMPMenuItem) };
            PlayFMAdvancedMenuItem = new ToolStripMenuItem { Name = nameof(PlayFMAdvancedMenuItem) };
            InstallUninstallMenuItem = new ToolStripMenuItem { Name = nameof(InstallUninstallMenuItem) };
            OpenInDromEdSep = new ToolStripSeparator { Name = nameof(OpenInDromEdSep) };
            OpenInDromEdMenuItem = new ToolStripMenuItem { Name = nameof(OpenInDromEdMenuItem) };
            FMContextMenuSep1 = new ToolStripSeparator { Name = nameof(FMContextMenuSep1) };
            ScanFMMenuItem = new ToolStripMenuItem { Name = nameof(ScanFMMenuItem) };
            ConvertAudioRCSubMenu = new ToolStripMenuItem { Name = nameof(ConvertAudioRCSubMenu) };
            ConvertWAVsTo16BitMenuItem = new ToolStripMenuItem { Name = nameof(ConvertWAVsTo16BitMenuItem) };
            ConvertOGGsToWAVsMenuItem = new ToolStripMenuItem { Name = nameof(ConvertOGGsToWAVsMenuItem) };
            FMContextMenuSep2 = new ToolStripSeparator { Name = nameof(FMContextMenuSep2) };
            RatingRCSubMenu = new ToolStripMenuItem
            {
                Name = nameof(RatingRCSubMenu)
            };
            RatingRCMenuUnrated = new ToolStripMenuItem
            {
                Name = nameof(RatingRCMenuUnrated),
                CheckOnClick = true
            };
            RatingRCMenu0 = new ToolStripMenuItem
            {
                Name = nameof(RatingRCMenu0),
                CheckOnClick = true
            };
            RatingRCMenu1 = new ToolStripMenuItem
            {
                Name = nameof(RatingRCMenu1),
                CheckOnClick = true
            };
            RatingRCMenu2 = new ToolStripMenuItem
            {
                Name = nameof(RatingRCMenu2),
                CheckOnClick = true
            };
            RatingRCMenu3 = new ToolStripMenuItem
            {
                Name = nameof(RatingRCMenu3),
                CheckOnClick = true
            };
            RatingRCMenu4 = new ToolStripMenuItem
            {
                Name = nameof(RatingRCMenu4),
                CheckOnClick = true
            };
            RatingRCMenu5 = new ToolStripMenuItem
            {
                Name = nameof(RatingRCMenu5),
                CheckOnClick = true
            };
            RatingRCMenu6 = new ToolStripMenuItem
            {
                Name = nameof(RatingRCMenu6),
                CheckOnClick = true
            };
            RatingRCMenu7 = new ToolStripMenuItem
            {
                Name = nameof(RatingRCMenu7),
                CheckOnClick = true
            };
            RatingRCMenu8 = new ToolStripMenuItem
            {
                Name = nameof(RatingRCMenu8),
                CheckOnClick = true
            };
            RatingRCMenu9 = new ToolStripMenuItem
            {
                Name = nameof(RatingRCMenu9),
                CheckOnClick = true
            };
            RatingRCMenu10 = new ToolStripMenuItem
            {
                Name = nameof(RatingRCMenu10),
                CheckOnClick = true
            };
            FinishedOnRCSubMenu = new ToolStripMenuItem
            {
                Name = nameof(FinishedOnRCSubMenu)
            };
            FinishedOnMenu = new ContextMenuStripCustom
            {
                Name = nameof(FinishedOnMenu)
            };
            FinishedOnNormalMenuItem = new ToolStripMenuItem
            {
                Name = nameof(FinishedOnNormalMenuItem),
                CheckOnClick = true
            };
            FinishedOnHardMenuItem = new ToolStripMenuItem
            {
                Name = nameof(FinishedOnHardMenuItem),
                CheckOnClick = true
            };
            FinishedOnExpertMenuItem = new ToolStripMenuItem
            {
                Name = nameof(FinishedOnExpertMenuItem),
                CheckOnClick = true
            };
            FinishedOnExtremeMenuItem = new ToolStripMenuItem
            {
                Name = nameof(FinishedOnExtremeMenuItem),
                CheckOnClick = true
            };
            FinishedOnUnknownMenuItem = new ToolStripMenuItem
            {
                Name = nameof(FinishedOnUnknownMenuItem),
                CheckOnClick = true
            };
            FMContextMenuSep3 = new ToolStripSeparator
            {
                Name = nameof(FMContextMenuSep3)
            };
            WebSearchMenuItem = new ToolStripMenuItem
            {
                Name = nameof(WebSearchMenuItem)
            };

            #endregion

            #region Add items to menu

            FMContextMenu.Items.AddRange(new ToolStripItem[]
            {
                PlayFMMenuItem,
                PlayFMInMPMenuItem,
                //PlayFMAdvancedMenuItem,
                InstallUninstallMenuItem,
                OpenInDromEdSep,
                OpenInDromEdMenuItem,
                FMContextMenuSep1,
                ScanFMMenuItem,
                ConvertAudioRCSubMenu,
                FMContextMenuSep2,
                RatingRCSubMenu,
                FinishedOnRCSubMenu,
                FMContextMenuSep3,
                WebSearchMenuItem
            });

            ConvertAudioRCSubMenu.DropDownItems.AddRange(new ToolStripItem[]
            {
                ConvertWAVsTo16BitMenuItem,
                ConvertOGGsToWAVsMenuItem
            });

            RatingRCSubMenu.DropDownItems.AddRange(new ToolStripItem[]
            {
                RatingRCMenuUnrated,
                RatingRCMenu0,
                RatingRCMenu1,
                RatingRCMenu2,
                RatingRCMenu3,
                RatingRCMenu4,
                RatingRCMenu5,
                RatingRCMenu6,
                RatingRCMenu7,
                RatingRCMenu8,
                RatingRCMenu9,
                RatingRCMenu10,
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
            FinishedOnRCSubMenu.DropDown = FinishedOnMenu;

            #endregion

            #region Event hookups

            FMContextMenu.Opening += FMContextMenu_Opening;
            PlayFMMenuItem.Click += PlayFMMenuItem_Click;
            PlayFMInMPMenuItem.Click += PlayFMInMPMenuItem_Click;
            InstallUninstallMenuItem.Click += InstallUninstallMenuItem_Click;
            OpenInDromEdMenuItem.Click += OpenInDromEdMenuItem_Click;
            ScanFMMenuItem.Click += ScanFMMenuItem_Click;
            ConvertWAVsTo16BitMenuItem.Click += ConvertWAVsTo16BitMenuItem_Click;
            ConvertOGGsToWAVsMenuItem.Click += ConvertOGGsToWAVsMenuItem_Click;

            foreach (ToolStripMenuItem item in RatingRCSubMenu.DropDownItems)
            {
                item.Click += RatingRCMenuItems_Click;
                item.CheckedChanged += RatingRCMenuItems_CheckedChanged;
            }

            RatingRCMenuUnrated.Click += RatingRCMenuItems_Click;
            RatingRCMenu0.Click += RatingRCMenuItems_Click;
            RatingRCMenu1.Click += RatingRCMenuItems_Click;
            RatingRCMenu2.Click += RatingRCMenuItems_Click;
            RatingRCMenu3.Click += RatingRCMenuItems_Click;
            RatingRCMenu4.Click += RatingRCMenuItems_Click;
            RatingRCMenu5.Click += RatingRCMenuItems_Click;
            RatingRCMenu6.Click += RatingRCMenuItems_Click;
            RatingRCMenu7.Click += RatingRCMenuItems_Click;
            RatingRCMenu8.Click += RatingRCMenuItems_Click;
            RatingRCMenu9.Click += RatingRCMenuItems_Click;
            RatingRCMenu10.Click += RatingRCMenuItems_Click;

            FinishedOnNormalMenuItem.Click += FinishedOnMenuItems_Click;
            FinishedOnHardMenuItem.Click += FinishedOnMenuItems_Click;
            FinishedOnExpertMenuItem.Click += FinishedOnMenuItems_Click;
            FinishedOnExtremeMenuItem.Click += FinishedOnMenuItems_Click;
            FinishedOnUnknownMenuItem.Click += FinishedOnMenuItems_Click;
            FinishedOnUnknownMenuItem.CheckedChanged += FinishedOnUnknownMenuItem_CheckedChanged;

            WebSearchMenuItem.Click += WebSearchMenuItem_Click;

            #endregion

            FinishedOnNormalMenuItem.Checked = _finishedOnNormalChecked;
            FinishedOnHardMenuItem.Checked = _finishedOnHardChecked;
            FinishedOnExpertMenuItem.Checked = _finishedOnExpertChecked;
            FinishedOnExtremeMenuItem.Checked = _finishedOnExtremeChecked;
            FinishedOnUnknownMenuItem.Checked = _finishedOnUnknownChecked;

            _fmMenuCreated = true;

            UpdateRatingList(Config.RatingDisplayStyle == RatingDisplayStyle.FMSel);

            SetRatingMenuItemChecked(_rating);

            InstallUninstallMenuItem.Enabled = _installUninstallMenuEnabled;
            InstallUninstallMenuItem.Text = _sayInstall
                    ? LText.FMsList.FMMenu_InstallFM
                    : LText.FMsList.FMMenu_UninstallFM;
            PlayFMMenuItem.Enabled = _playFMMenuItemEnabled;
            PlayFMInMPMenuItem.Visible = _playFMInMPMenuItemVisible;
            ScanFMMenuItem.Enabled = _scanFMMenuItemEnabled;
            OpenInDromEdSep.Visible = _openInDromEdSepVisible;
            OpenInDromEdMenuItem.Visible = _openInDromEdMenuItemVisible;
            ConvertAudioRCSubMenu.Enabled = _convertAudioSubMenuEnabled;

            SetFMMenuTextToLocalized();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                #region Column context menu

                ColumnHeaderContextMenu?.Dispose();
                ShowTitleMenuItem?.Dispose();
                ShowArchiveMenuItem?.Dispose();
                ShowSizeMenuItem?.Dispose();
                ShowRatingMenuItem?.Dispose();
                ShowReleaseDateMenuItem?.Dispose();
                ShowLastPlayedMenuItem?.Dispose();
                ShowCommentMenuItem?.Dispose();
                ShowDisabledModsMenuItem?.Dispose();
                ShowInstalledMenuItem?.Dispose();
                ShowGameMenuItem?.Dispose();
                ShowAuthorMenuItem?.Dispose();
                ResetColumnVisibilityMenuItem?.Dispose();
                ResetColumnPositionsMenuItem?.Dispose();
                ResetAllColumnWidthsMenuItem?.Dispose();
                ColumnHeaderContextMenuSep1?.Dispose();
                ShowFinishedMenuItem?.Dispose();

                #endregion

                #region FM context menu

                FMContextMenu?.Dispose();
                PlayFMMenuItem?.Dispose();
                PlayFMInMPMenuItem?.Dispose();
                PlayFMAdvancedMenuItem?.Dispose();
                InstallUninstallMenuItem?.Dispose();
                OpenInDromEdSep?.Dispose();
                OpenInDromEdMenuItem?.Dispose();
                FMContextMenuSep1?.Dispose();
                ScanFMMenuItem?.Dispose();
                ConvertAudioRCSubMenu?.Dispose();
                ConvertWAVsTo16BitMenuItem?.Dispose();
                ConvertOGGsToWAVsMenuItem?.Dispose();
                FMContextMenuSep2?.Dispose();
                RatingRCSubMenu?.Dispose();
                RatingRCMenuUnrated?.Dispose();
                RatingRCMenu0?.Dispose();
                RatingRCMenu1?.Dispose();
                RatingRCMenu2?.Dispose();
                RatingRCMenu3?.Dispose();
                RatingRCMenu4?.Dispose();
                RatingRCMenu5?.Dispose();
                RatingRCMenu6?.Dispose();
                RatingRCMenu7?.Dispose();
                RatingRCMenu8?.Dispose();
                RatingRCMenu9?.Dispose();
                RatingRCMenu10?.Dispose();
                FinishedOnRCSubMenu?.Dispose();
                FinishedOnMenu?.Dispose();
                FinishedOnNormalMenuItem?.Dispose();
                FinishedOnHardMenuItem?.Dispose();
                FinishedOnExpertMenuItem?.Dispose();
                FinishedOnExtremeMenuItem?.Dispose();
                FinishedOnUnknownMenuItem?.Dispose();
                FMContextMenuSep3?.Dispose();
                WebSearchMenuItem?.Dispose();

                #endregion
            }
            base.Dispose(disposing);
        }

        #region Column header context menu fields

        internal ContextMenuStripCustom ColumnHeaderContextMenu;

        private ToolStripMenuItem[] ColumnHeaderCheckBoxMenuItems;

        private ToolStripMenuItem ResetColumnVisibilityMenuItem;
        private ToolStripMenuItem ResetAllColumnWidthsMenuItem;
        private ToolStripMenuItem ResetColumnPositionsMenuItem;

        private ToolStripSeparator ColumnHeaderContextMenuSep1;

        private ToolStripMenuItem ShowGameMenuItem;
        private ToolStripMenuItem ShowInstalledMenuItem;
        private ToolStripMenuItem ShowTitleMenuItem;
        private ToolStripMenuItem ShowArchiveMenuItem;
        private ToolStripMenuItem ShowAuthorMenuItem;
        private ToolStripMenuItem ShowSizeMenuItem;
        private ToolStripMenuItem ShowRatingMenuItem;
        private ToolStripMenuItem ShowFinishedMenuItem;
        private ToolStripMenuItem ShowReleaseDateMenuItem;
        private ToolStripMenuItem ShowLastPlayedMenuItem;
        private ToolStripMenuItem ShowDisabledModsMenuItem;
        private ToolStripMenuItem ShowCommentMenuItem;

        #endregion

        #region FM context menu fields

        internal ContextMenuStrip FMContextMenu;

        internal ToolStripMenuItem PlayFMMenuItem;
        internal ToolStripMenuItem PlayFMInMPMenuItem;
        private ToolStripMenuItem PlayFMAdvancedMenuItem;
        internal ToolStripMenuItem InstallUninstallMenuItem;

        internal ToolStripSeparator OpenInDromEdSep;

        internal ToolStripMenuItem OpenInDromEdMenuItem;

        private ToolStripSeparator FMContextMenuSep1;

        internal ToolStripMenuItem ScanFMMenuItem;
        internal ToolStripMenuItem ConvertAudioRCSubMenu;
        private ToolStripMenuItem ConvertWAVsTo16BitMenuItem;
        private ToolStripMenuItem ConvertOGGsToWAVsMenuItem;

        private ToolStripSeparator FMContextMenuSep2;

        private ToolStripMenuItem RatingRCSubMenu;
        private ToolStripMenuItem RatingRCMenuUnrated;
        private ToolStripMenuItem RatingRCMenu0;
        private ToolStripMenuItem RatingRCMenu1;
        private ToolStripMenuItem RatingRCMenu2;
        private ToolStripMenuItem RatingRCMenu3;
        private ToolStripMenuItem RatingRCMenu4;
        private ToolStripMenuItem RatingRCMenu5;
        private ToolStripMenuItem RatingRCMenu6;
        private ToolStripMenuItem RatingRCMenu7;
        private ToolStripMenuItem RatingRCMenu8;
        private ToolStripMenuItem RatingRCMenu9;
        private ToolStripMenuItem RatingRCMenu10;

        private ToolStripMenuItem FinishedOnRCSubMenu;
        internal ContextMenuStripCustom FinishedOnMenu;
        internal ToolStripMenuItem FinishedOnNormalMenuItem;
        internal ToolStripMenuItem FinishedOnHardMenuItem;
        internal ToolStripMenuItem FinishedOnExpertMenuItem;
        internal ToolStripMenuItem FinishedOnExtremeMenuItem;
        internal ToolStripMenuItem FinishedOnUnknownMenuItem;

        private ToolStripSeparator FMContextMenuSep3;

        private ToolStripMenuItem WebSearchMenuItem;

        #endregion
    }
}
