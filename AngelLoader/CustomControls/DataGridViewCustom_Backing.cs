using System.Windows.Forms;
using AngelLoader.Common.DataClasses;

namespace AngelLoader.CustomControls
{
    public sealed partial class DataGridViewCustom
    {
        private bool _columnHeaderMenuCreated;
        private readonly bool[] ColumnCheckedStates = { true, true, true, true, true, true, true, true, true, true, true, true };

        internal bool FMMenuCreated { get; private set; }

        private void InitColumnHeaderContextMenu()
        {
            #region Instantiation

            FMColumnHeaderContextMenu = new ContextMenuStripCustom
            {
                Name = nameof(FMColumnHeaderContextMenu)
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

            FMColumnHeaderContextMenu.Items.AddRange(new ToolStripItem[]
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

            FMColumnHeaderContextMenu.SetPreventCloseOnClickItems(ColumnHeaderCheckBoxMenuItems);

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

        private void InitFMContextMenu()
        {
            #region Instantiation

            FMContextMenu = new ContextMenuStrip
            {
                Name = nameof(FMContextMenu)
            };
            PlayFMMenuItem = new ToolStripMenuItem
            {
                Name = nameof(PlayFMMenuItem)
            };
            PlayFMInMPMenuItem = new ToolStripMenuItem
            {
                Name = nameof(PlayFMInMPMenuItem)
            };
            PlayFMAdvancedMenuItem = new ToolStripMenuItem
            {
                Name = nameof(PlayFMAdvancedMenuItem)
            };
            InstallUninstallMenuItem = new ToolStripMenuItem
            {
                Name = nameof(InstallUninstallMenuItem)
            };
            OpenInDromEdSep = new ToolStripMenuItem
            {
                Name = nameof(OpenInDromEdSep)
            };
            OpenInDromEdMenuItem = new ToolStripMenuItem
            {
                Name = nameof(OpenInDromEdMenuItem)
            };
            FMContextMenuSep1 = new ToolStripMenuItem
            {
                Name = nameof(FMContextMenuSep1)
            };
            ScanFMMenuItem = new ToolStripMenuItem
            {
                Name = nameof(ScanFMMenuItem)
            };
            ConvertAudioRCSubMenu = new ToolStripMenuItem
            {
                Name = nameof(ConvertAudioRCSubMenu)
            };
            ConvertWAVsTo16BitMenuItem = new ToolStripMenuItem
            {
                Name = nameof(ConvertWAVsTo16BitMenuItem)
            };
            ConvertOGGsToWAVsMenuItem = new ToolStripMenuItem
            {
                Name = nameof(ConvertOGGsToWAVsMenuItem)
            };
            FMContextMenuSep2 = new ToolStripMenuItem
            {
                Name = nameof(FMContextMenuSep2)
            };
            RatingRCSubMenu = new ToolStripMenuItem
            {
                Name = nameof(RatingRCSubMenu)
            };
            RatingRCMenuUnrated = new ToolStripMenuItem
            {
                Name = nameof(RatingRCMenuUnrated)
            };
            RatingRCMenu0 = new ToolStripMenuItem
            {
                Name = nameof(RatingRCMenu0)
            };
            RatingRCMenu1 = new ToolStripMenuItem
            {
                Name = nameof(RatingRCMenu1)
            };
            RatingRCMenu2 = new ToolStripMenuItem
            {
                Name = nameof(RatingRCMenu2)
            };
            RatingRCMenu3 = new ToolStripMenuItem
            {
                Name = nameof(RatingRCMenu3)
            };
            RatingRCMenu4 = new ToolStripMenuItem
            {
                Name = nameof(RatingRCMenu4)
            };
            RatingRCMenu5 = new ToolStripMenuItem
            {
                Name = nameof(RatingRCMenu5)
            };
            RatingRCMenu6 = new ToolStripMenuItem
            {
                Name = nameof(RatingRCMenu6)
            };
            RatingRCMenu7 = new ToolStripMenuItem
            {
                Name = nameof(RatingRCMenu7)
            };
            RatingRCMenu8 = new ToolStripMenuItem
            {
                Name = nameof(RatingRCMenu8)
            };
            RatingRCMenu9 = new ToolStripMenuItem
            {
                Name = nameof(RatingRCMenu9)
            };
            RatingRCMenu10 = new ToolStripMenuItem
            {
                Name = nameof(RatingRCMenu10)
            };
            FinishedOnRCSubMenu = new ToolStripMenuItem
            {
                Name = nameof(FinishedOnRCSubMenu)
            };
            FinishedOnNormalMenuItem = new ToolStripMenuItem
            {
                Name = nameof(FinishedOnNormalMenuItem)
            };
            FinishedOnHardMenuItem = new ToolStripMenuItem
            {
                Name = nameof(FinishedOnHardMenuItem)
            };
            FinishedOnExpertMenuItem = new ToolStripMenuItem
            {
                Name = nameof(FinishedOnExpertMenuItem)
            };
            FinishedOnExtremeMenuItem = new ToolStripMenuItem
            {
                Name = nameof(FinishedOnExtremeMenuItem)
            };
            FinishedOnUnknownMenuItem = new ToolStripMenuItem
            {
                Name = nameof(FinishedOnUnknownMenuItem)
            };
            FMContextMenuSep3 = new ToolStripMenuItem
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
                PlayFMAdvancedMenuItem,
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

            FinishedOnRCSubMenu.DropDownItems.AddRange(new ToolStripItem[]
            {
                FinishedOnNormalMenuItem,
                FinishedOnHardMenuItem,
                FinishedOnExpertMenuItem,
                FinishedOnExtremeMenuItem,
                FinishedOnUnknownMenuItem
            });

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

            WebSearchMenuItem.Click += WebSearchMenuItem_Click;

            #endregion

            FMMenuCreated = true;

            SetFMMenuTextToLocalized();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                OriginalContextMenu?.Dispose();

                #region Column context menu

                FMColumnHeaderContextMenu?.Dispose();
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

        internal ContextMenuStripCustom FMColumnHeaderContextMenu;

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

        private ContextMenuStrip FMContextMenu;

        private ToolStripMenuItem PlayFMMenuItem;
        private ToolStripMenuItem PlayFMInMPMenuItem;
        private ToolStripMenuItem PlayFMAdvancedMenuItem;
        private ToolStripMenuItem InstallUninstallMenuItem;

        private ToolStripMenuItem OpenInDromEdSep;

        private ToolStripMenuItem OpenInDromEdMenuItem;

        private ToolStripMenuItem FMContextMenuSep1;

        private ToolStripMenuItem ScanFMMenuItem;
        private ToolStripMenuItem ConvertAudioRCSubMenu;
        private ToolStripMenuItem ConvertWAVsTo16BitMenuItem;
        private ToolStripMenuItem ConvertOGGsToWAVsMenuItem;

        private ToolStripMenuItem FMContextMenuSep2;

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
        private ToolStripMenuItem FinishedOnNormalMenuItem;
        private ToolStripMenuItem FinishedOnHardMenuItem;
        private ToolStripMenuItem FinishedOnExpertMenuItem;
        private ToolStripMenuItem FinishedOnExtremeMenuItem;
        private ToolStripMenuItem FinishedOnUnknownMenuItem;

        private ToolStripMenuItem FMContextMenuSep3;

        private ToolStripMenuItem WebSearchMenuItem;

        #endregion
    }
}
