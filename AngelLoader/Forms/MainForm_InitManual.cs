using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using AngelLoader.Forms.CustomControls;
using AngelLoader.Properties;
using static AngelLoader.Misc;

/*
 Notes:
    -Some Anchor / Dock combos have been kept despite the docs saying they're mutually exclusive, because
     they currently work and I'm not 100% certain which one I should keep. Lowest priority.
    -We need to always add the tab pages to the Controls collection on init, because otherwise in dark
     mode the tabs have wrong sizes until you set it again after the UI is already shown(?!)
    -The filter bar gets its x-pos and width set in GameTypeChange() and its y-pos is always 0.
     Height is 100 so it goes behind the DataGridView and its actual scroll bars will be hidden but
     they'll still function, and you can use your mousewheel or the custom arrow buttons to scroll.
     2019-07-15: It has to have its designer-generated width set here or else the left scroll button
     doesn't appear. Meh?
    -Readme control buttons:
     BackColor for these gets set when the readme box is shown or hidden (for classic mode)
*/

namespace AngelLoader.Forms
{
    public sealed partial class MainForm
    {
        private void InitComponentManual()
        {
            components = new Container();
            GameTabsImageList = new ImageList(components);
            BottomPanel = new Panel();
            BottomRightButtonsFLP = new FlowLayoutPanel();
            SettingsButton = new DarkButton();
            BottomLeftButtonsFLP = new FlowLayoutPanel();
            PlayFMButton = new DarkButton();
            PlayOriginalGameButton = new DarkButton();
            WebSearchButton = new DarkButton();
            EverythingPanel = new Panel();
            MainSplitContainer = new DarkSplitContainerCustom();
            TopSplitContainer = new DarkSplitContainerCustom();
            MainMenuButton = new DarkButton();
            FilterBarScrollRightButton = new DarkArrowButton();
            FilterBarScrollLeftButton = new DarkArrowButton();
            FMsDGV = new DataGridViewCustom();
            GameTypeColumn = new DataGridViewImageColumn();
            InstalledColumn = new DataGridViewImageColumn();
            TitleColumn = new DataGridViewTextBoxColumn();
            ArchiveColumn = new DataGridViewTextBoxColumn();
            AuthorColumn = new DataGridViewTextBoxColumn();
            SizeColumn = new DataGridViewTextBoxColumn();
            RatingTextColumn = new DataGridViewTextBoxColumn();
            FinishedColumn = new DataGridViewImageColumn();
            ReleaseDateColumn = new DataGridViewTextBoxColumn();
            LastPlayedColumn = new DataGridViewTextBoxColumn();
            DateAddedColumn = new DataGridViewTextBoxColumn();
            DisabledModsColumn = new DataGridViewTextBoxColumn();
            CommentColumn = new DataGridViewTextBoxColumn();
            FilterBarFLP = new FlowLayoutPanel();
            FilterGameButtonsToolStrip = new ToolStripCustom();
            FilterByThief1Button = new ToolStripButtonCustom();
            FilterByThief2Button = new ToolStripButtonCustom();
            FilterByThief3Button = new ToolStripButtonCustom();
            FilterBySS2Button = new ToolStripButtonCustom();
            GameFilterControlsShowHideButtonToolStrip = new ToolStripCustom();
            GameFilterControlsShowHideButton = new ToolStripArrowButton();
            FilterTitleLabel = new DarkLabel();
            FilterTitleTextBox = new DarkTextBoxCustom();
            FilterAuthorLabel = new DarkLabel();
            FilterAuthorTextBox = new DarkTextBoxCustom();
            FilterIconButtonsToolStrip = new ToolStripCustom();
            FilterByReleaseDateButton = new ToolStripButtonCustom();
            FilterByLastPlayedButton = new ToolStripButtonCustom();
            FilterByTagsButton = new ToolStripButtonCustom();
            FilterByFinishedButton = new ToolStripButtonCustom();
            FilterByUnfinishedButton = new ToolStripButtonCustom();
            FilterByRatingButton = new ToolStripButtonCustom();
            FilterShowUnsupportedButton = new ToolStripButtonCustom();
            FilterShowUnavailableButton = new ToolStripButtonCustom();
            FilterShowRecentAtTopButton = new ToolStripButtonCustom();
            FilterControlsShowHideButton = new ToolStripArrowButton();
            RefreshAreaToolStrip = new ToolStripCustom();
            RefreshFromDiskButton = new ToolStripButtonCustom();
            RefreshFiltersButton = new ToolStripButtonCustom();
            ClearFiltersButton = new ToolStripButtonCustom();
            ResetLayoutButton = new DarkButton();
            GamesTabControl = new DarkTabControl();
            Thief1TabPage = new DarkTabPageCustom();
            Thief2TabPage = new DarkTabPageCustom();
            Thief3TabPage = new DarkTabPageCustom();
            SS2TabPage = new DarkTabPageCustom();
            TopRightMenuButton = new DarkButton();
            TopRightCollapseButton = new DarkArrowButton();
            TopRightTabControl = new DarkTabControl();
            StatisticsTabPage = new DarkTabPageCustom();
            StatsScanCustomResourcesButton = new DarkButton();
            StatsCheckBoxesPanel = new Panel();
            CR_MapCheckBox = new DarkCheckBox();
            CR_MoviesCheckBox = new DarkCheckBox();
            CR_MotionsCheckBox = new DarkCheckBox();
            CR_SoundsCheckBox = new DarkCheckBox();
            CR_CreaturesCheckBox = new DarkCheckBox();
            CR_TexturesCheckBox = new DarkCheckBox();
            CR_AutomapCheckBox = new DarkCheckBox();
            CR_ScriptsCheckBox = new DarkCheckBox();
            CR_SubtitlesCheckBox = new DarkCheckBox();
            CR_ObjectsCheckBox = new DarkCheckBox();
            CustomResourcesLabel = new DarkLabel();
            EditFMTabPage = new DarkTabPageCustom();
            EditFMScanLanguagesButton = new DarkButton();
            EditFMLanguageLabel = new DarkLabel();
            EditFMLanguageComboBox = new DarkComboBoxWithBackingItems();
            EditFMScanForReadmesButton = new DarkButton();
            EditFMScanReleaseDateButton = new DarkButton();
            EditFMScanAuthorButton = new DarkButton();
            EditFMScanTitleButton = new DarkButton();
            EditFMAltTitlesArrowButton = new DarkArrowButton();
            EditFMTitleTextBox = new DarkTextBox();
            EditFMFinishedOnButton = new DarkButton();
            EditFMRatingComboBox = new DarkComboBoxWithBackingItems();
            EditFMRatingLabel = new DarkLabel();
            EditFMLastPlayedDateTimePicker = new DarkDateTimePicker();
            EditFMReleaseDateDateTimePicker = new DarkDateTimePicker();
            EditFMLastPlayedCheckBox = new DarkCheckBox();
            EditFMReleaseDateCheckBox = new DarkCheckBox();
            EditFMAuthorTextBox = new DarkTextBox();
            EditFMAuthorLabel = new DarkLabel();
            EditFMTitleLabel = new DarkLabel();
            CommentTabPage = new DarkTabPageCustom();
            CommentTextBox = new DarkTextBox();
            TagsTabPage = new DarkTabPageCustom();
            AddTagButton = new DarkButton();
            AddTagTextBox = new DarkTextBoxCustom();
            AddRemoveTagFLP = new FlowLayoutPanel();
            RemoveTagButton = new DarkButton();
            AddTagFromListButton = new DarkButton();
            TagsTreeView = new DarkTreeView();
            TagsTabAutoScrollMarker = new Control();
            PatchTabPage = new DarkTabPageCustom();
            PatchMainPanel = new Panel();
            PatchDMLsPanel = new Panel();
            PatchDMLPatchesLabel = new DarkLabel();
            PatchDMLsListBox = new DarkListBox();
            PatchRemoveDMLButton = new DarkButton();
            PatchAddDMLButton = new DarkButton();
            PatchOpenFMFolderButton = new DarkButton();
            PatchFMNotInstalledLabel = new DarkLabel();
            ModsTabPage = new DarkTabPageCustom();
            ModsDisabledModsTextBox = new DarkTextBox();
            ModsDisabledModsLabel = new DarkLabel();
            ModsCheckList = new DarkCheckList();
            ReadmeEncodingButton = new DarkButton();
            ReadmeFullScreenButton = new DarkButton();
            ReadmeZoomInButton = new DarkButton();
            ReadmeZoomOutButton = new DarkButton();
            ReadmeResetZoomButton = new DarkButton();
            ChooseReadmeComboBox = new DarkComboBoxWithBackingItems();
            ReadmeRichTextBox = new RichTextBoxCustom();
            MainToolTip = new ToolTip(components);
            BottomPanel.SuspendLayout();
            BottomRightButtonsFLP.SuspendLayout();
            BottomLeftButtonsFLP.SuspendLayout();
            EverythingPanel.SuspendLayout();
            MainSplitContainer.BeginInit();
            MainSplitContainer.Panel1.SuspendLayout();
            MainSplitContainer.Panel2.SuspendLayout();
            MainSplitContainer.SuspendLayout();
            TopSplitContainer.BeginInit();
            TopSplitContainer.Panel1.SuspendLayout();
            TopSplitContainer.Panel2.SuspendLayout();
            TopSplitContainer.SuspendLayout();
            ((ISupportInitialize)FMsDGV).BeginInit();
            FilterBarFLP.SuspendLayout();
            FilterGameButtonsToolStrip.SuspendLayout();
            GameFilterControlsShowHideButtonToolStrip.SuspendLayout();
            FilterIconButtonsToolStrip.SuspendLayout();
            RefreshAreaToolStrip.SuspendLayout();
            GamesTabControl.SuspendLayout();
            TopRightTabControl.SuspendLayout();
            StatisticsTabPage.SuspendLayout();
            StatsCheckBoxesPanel.SuspendLayout();
            EditFMTabPage.SuspendLayout();
            CommentTabPage.SuspendLayout();
            TagsTabPage.SuspendLayout();
            AddRemoveTagFLP.SuspendLayout();
            PatchTabPage.SuspendLayout();
            PatchMainPanel.SuspendLayout();
            PatchDMLsPanel.SuspendLayout();
            ModsTabPage.SuspendLayout();
            SuspendLayout();
            // 
            // GameTabsImageList
            // 
            GameTabsImageList.ColorDepth = ColorDepth.Depth32Bit;
            // 
            // BottomPanel
            // 
            BottomPanel.Controls.Add(BottomRightButtonsFLP);
            BottomPanel.Controls.Add(BottomLeftButtonsFLP);
            BottomPanel.Dock = DockStyle.Bottom;
            BottomPanel.Size = new Size(1671, 44);
            BottomPanel.TabIndex = 1;
            // 
            // BottomRightButtonsFLP
            // 
            BottomRightButtonsFLP.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            BottomRightButtonsFLP.AutoSize = true;
            BottomRightButtonsFLP.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            BottomRightButtonsFLP.Controls.Add(SettingsButton);
            BottomRightButtonsFLP.FlowDirection = FlowDirection.RightToLeft;
            BottomRightButtonsFLP.Location = new Point(1563, 0);
            // Needs width to be anchored correctly
            BottomRightButtonsFLP.Width = 106;
            BottomRightButtonsFLP.TabIndex = 37;
            // 
            // SettingsButton
            // 
            SettingsButton.AutoSize = true;
            SettingsButton.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            SettingsButton.MinimumSize = new Size(0, 36);
            SettingsButton.Padding = new Padding(30, 0, 6, 0);
            SettingsButton.TabIndex = 62;
            SettingsButton.UseVisualStyleBackColor = true;
            SettingsButton.PaintCustom += SettingsButton_Paint;
            SettingsButton.Click += Settings_Click;
            // 
            // BottomLeftButtonsFLP
            // 
            BottomLeftButtonsFLP.AutoSize = true;
            BottomLeftButtonsFLP.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            BottomLeftButtonsFLP.Controls.Add(PlayFMButton);
            BottomLeftButtonsFLP.Controls.Add(PlayOriginalGameButton);
            BottomLeftButtonsFLP.Controls.Add(WebSearchButton);
            BottomLeftButtonsFLP.Location = new Point(2, 0);
            BottomLeftButtonsFLP.TabIndex = 36;
            BottomLeftButtonsFLP.Paint += BottomLeftButtonsFLP_Paint;
            // 
            // PlayFMButton
            // 
            PlayFMButton.AutoSize = true;
            PlayFMButton.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            PlayFMButton.MinimumSize = new Size(91, 36);
            PlayFMButton.Padding = new Padding(28, 0, 6, 0);
            PlayFMButton.TabIndex = 56;
            PlayFMButton.UseVisualStyleBackColor = true;
            PlayFMButton.PaintCustom += PlayFMButton_Paint;
            PlayFMButton.Click += InstallUninstall_Play_Buttons_Click;
            // 
            // PlayOriginalGameButton
            // 
            PlayOriginalGameButton.AutoSize = true;
            PlayOriginalGameButton.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            PlayOriginalGameButton.Margin = new Padding(3, 3, 0, 3);
            PlayOriginalGameButton.MinimumSize = new Size(0, 36);
            PlayOriginalGameButton.Padding = new Padding(33, 0, 6, 0);
            PlayOriginalGameButton.TabIndex = 57;
            PlayOriginalGameButton.UseVisualStyleBackColor = true;
            PlayOriginalGameButton.PaintCustom += PlayOriginalGameButton_Paint;
            PlayOriginalGameButton.Click += PlayOriginalGameButton_Click;
            // 
            // WebSearchButton
            // 
            WebSearchButton.AutoSize = true;
            WebSearchButton.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            WebSearchButton.Margin = new Padding(15, 3, 3, 3);
            WebSearchButton.MinimumSize = new Size(116, 36);
            WebSearchButton.Padding = new Padding(33, 0, 6, 0);
            WebSearchButton.TabIndex = 60;
            WebSearchButton.UseVisualStyleBackColor = true;
            WebSearchButton.PaintCustom += WebSearchButton_Paint;
            WebSearchButton.Click += WebSearchButton_Click;
            // 
            // EverythingPanel
            // 
            EverythingPanel.Controls.Add(MainSplitContainer);
            EverythingPanel.Controls.Add(BottomPanel);
            EverythingPanel.Dock = DockStyle.Fill;
            EverythingPanel.Size = new Size(1671, 716);
            EverythingPanel.TabIndex = 4;
            // 
            // MainSplitContainer
            // 
            MainSplitContainer.BackColor = SystemColors.ActiveBorder;
            MainSplitContainer.Dock = DockStyle.Fill;
            MainSplitContainer.Orientation = Orientation.Horizontal;
            // 
            // MainSplitContainer.Panel1
            // 
            MainSplitContainer.Panel1.BackColor = SystemColors.Control;
            MainSplitContainer.Panel1.Controls.Add(TopSplitContainer);
            MainSplitContainer.Panel1MinSize = 100;
            // 
            // MainSplitContainer.Panel2
            // 
            MainSplitContainer.Panel2.BackColor = SystemColors.Control;
            MainSplitContainer.Panel2.Controls.Add(ReadmeEncodingButton);
            MainSplitContainer.Panel2.Controls.Add(ReadmeFullScreenButton);
            MainSplitContainer.Panel2.Controls.Add(ReadmeZoomInButton);
            MainSplitContainer.Panel2.Controls.Add(ReadmeZoomOutButton);
            MainSplitContainer.Panel2.Controls.Add(ReadmeResetZoomButton);
            MainSplitContainer.Panel2.Controls.Add(ChooseReadmeComboBox);
            MainSplitContainer.Panel2.Controls.Add(ReadmeRichTextBox);
            MainSplitContainer.Panel2.MouseLeave += ReadmeArea_MouseLeave;
            MainSplitContainer.Panel2.Paint += MainSplitContainer_Panel2_Paint;
            MainSplitContainer.Panel2MinSize = 38;
            MainSplitContainer.Panel2.Padding = new Padding(1, 1, 2, 2);
            MainSplitContainer.Size = new Size(1671, 672);
            MainSplitContainer.SplitterDistance = 309;
            MainSplitContainer.TabIndex = 0;
            // 
            // TopSplitContainer
            // 
            TopSplitContainer.BackColor = SystemColors.ActiveBorder;
            TopSplitContainer.Dock = DockStyle.Fill;
            // 
            // TopSplitContainer.Panel1
            // 
            TopSplitContainer.Panel1.BackColor = SystemColors.Control;
            TopSplitContainer.Panel1.Controls.Add(MainMenuButton);
            TopSplitContainer.Panel1.Controls.Add(FilterBarScrollRightButton);
            TopSplitContainer.Panel1.Controls.Add(FilterBarScrollLeftButton);
            TopSplitContainer.Panel1.Controls.Add(FMsDGV);
            TopSplitContainer.Panel1.Controls.Add(FilterBarFLP);
            TopSplitContainer.Panel1.Controls.Add(RefreshAreaToolStrip);
            TopSplitContainer.Panel1.Controls.Add(ResetLayoutButton);
            TopSplitContainer.Panel1.Controls.Add(GamesTabControl);
            // 
            // TopSplitContainer.Panel2
            // 
            TopSplitContainer.Panel2.BackColor = SystemColors.Control;
            TopSplitContainer.Panel2.Controls.Add(TopRightMenuButton);
            TopSplitContainer.Panel2.Controls.Add(TopRightCollapseButton);
            TopSplitContainer.Panel2.Controls.Add(TopRightTabControl);
            TopSplitContainer.Size = new Size(1671, 309);
            TopSplitContainer.SplitterDistance = 1116;
            TopSplitContainer.TabIndex = 0;
            // 
            // MainMenuButton
            // 
            MainMenuButton.FlatAppearance.BorderSize = 0;
            MainMenuButton.FlatStyle = FlatStyle.Flat;
            MainMenuButton.Size = new Size(24, 24);
            MainMenuButton.TabIndex = 14;
            MainMenuButton.UseVisualStyleBackColor = true;
            MainMenuButton.PaintCustom += MainMenuButton_Paint;
            MainMenuButton.Click += MainMenuButton_Click;
            MainMenuButton.Enter += MainMenuButton_Enter;
            // 
            // FilterBarScrollRightButton
            // 
            FilterBarScrollRightButton.ArrowDirection = Direction.Right;
            FilterBarScrollRightButton.FlatStyle = FlatStyle.Flat;
            FilterBarScrollRightButton.Size = new Size(14, 24);
            FilterBarScrollRightButton.TabIndex = 10;
            FilterBarScrollRightButton.UseVisualStyleBackColor = true;
            FilterBarScrollRightButton.Visible = false;
            FilterBarScrollRightButton.EnabledChanged += FilterBarScrollButtons_EnabledChanged;
            FilterBarScrollRightButton.VisibleChanged += FilterBarScrollButtons_VisibleChanged;
            FilterBarScrollRightButton.Click += FilterBarScrollButtons_Click;
            FilterBarScrollRightButton.MouseDown += FilterBarScrollButtons_MouseDown;
            FilterBarScrollRightButton.MouseUp += FilterBarScrollLeftButton_MouseUp;
            // 
            // FilterBarScrollLeftButton
            // 
            FilterBarScrollLeftButton.FlatStyle = FlatStyle.Flat;
            FilterBarScrollLeftButton.ArrowDirection = Direction.Left;
            FilterBarScrollLeftButton.Size = new Size(14, 24);
            FilterBarScrollLeftButton.TabIndex = 2;
            FilterBarScrollLeftButton.UseVisualStyleBackColor = true;
            FilterBarScrollLeftButton.Visible = false;
            FilterBarScrollLeftButton.EnabledChanged += FilterBarScrollButtons_EnabledChanged;
            FilterBarScrollLeftButton.VisibleChanged += FilterBarScrollButtons_VisibleChanged;
            FilterBarScrollLeftButton.Click += FilterBarScrollButtons_Click;
            FilterBarScrollLeftButton.MouseDown += FilterBarScrollButtons_MouseDown;
            FilterBarScrollLeftButton.MouseUp += FilterBarScrollLeftButton_MouseUp;
            // 
            // FMsDGV
            // 
            FMsDGV.AllowUserToAddRows = false;
            FMsDGV.AllowUserToDeleteRows = false;
            FMsDGV.AllowUserToOrderColumns = true;
            FMsDGV.AllowUserToResizeRows = false;
            FMsDGV.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            FMsDGV.ColumnHeadersDefaultCellStyle.WrapMode = DataGridViewTriState.False;
            FMsDGV.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            FMsDGV.Columns.AddRange(
                GameTypeColumn,
                InstalledColumn,
                TitleColumn,
                ArchiveColumn,
                AuthorColumn,
                SizeColumn,
                RatingTextColumn,
                FinishedColumn,
                ReleaseDateColumn,
                LastPlayedColumn,
                DateAddedColumn,
                DisabledModsColumn,
                CommentColumn);
            FMsDGV.BackgroundColor = SystemColors.ControlDark;
            FMsDGV.DefaultCellStyle.WrapMode = DataGridViewTriState.False;
            FMsDGV.Location = new Point(1, 26);
            FMsDGV.MultiSelect = false;
            FMsDGV.ReadOnly = true;
            FMsDGV.RowHeadersVisible = false;
            FMsDGV.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            FMsDGV.Size = new Size(1109, 282);
            FMsDGV.StandardTab = true;
            FMsDGV.TabIndex = 0;
            FMsDGV.VirtualMode = true;
            FMsDGV.CellDoubleClick += FMsDGV_CellDoubleClick;
            FMsDGV.CellValueNeeded += FMsDGV_CellValueNeeded;
            FMsDGV.ColumnHeaderMouseClick += FMsDGV_ColumnHeaderMouseClick;
            FMsDGV.SelectionChanged += FMsDGV_SelectionChanged;
            FMsDGV.KeyDown += FMsDGV_KeyDown;
            FMsDGV.KeyPress += FMsDGV_KeyPress;
            FMsDGV.MouseDown += FMsDGV_MouseDown;
            // 
            // GameTypeColumn
            // 
            GameTypeColumn.ImageLayout = DataGridViewImageCellLayout.Zoom;
            GameTypeColumn.MinimumWidth = 25;
            GameTypeColumn.ReadOnly = true;
            GameTypeColumn.Resizable = DataGridViewTriState.True;
            GameTypeColumn.SortMode = DataGridViewColumnSortMode.Programmatic;
            // 
            // InstalledColumn
            // 
            InstalledColumn.ImageLayout = DataGridViewImageCellLayout.Zoom;
            InstalledColumn.MinimumWidth = 25;
            InstalledColumn.ReadOnly = true;
            InstalledColumn.Resizable = DataGridViewTriState.True;
            InstalledColumn.SortMode = DataGridViewColumnSortMode.Programmatic;
            // 
            // TitleColumn
            // 
            TitleColumn.MinimumWidth = 25;
            TitleColumn.ReadOnly = true;
            TitleColumn.SortMode = DataGridViewColumnSortMode.Programmatic;
            // 
            // ArchiveColumn
            // 
            ArchiveColumn.MinimumWidth = 25;
            ArchiveColumn.ReadOnly = true;
            ArchiveColumn.SortMode = DataGridViewColumnSortMode.Programmatic;
            // 
            // AuthorColumn
            // 
            AuthorColumn.MinimumWidth = 25;
            AuthorColumn.ReadOnly = true;
            AuthorColumn.SortMode = DataGridViewColumnSortMode.Programmatic;
            // 
            // SizeColumn
            // 
            SizeColumn.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            SizeColumn.MinimumWidth = 25;
            SizeColumn.ReadOnly = true;
            SizeColumn.SortMode = DataGridViewColumnSortMode.Programmatic;
            // 
            // RatingTextColumn
            // 
            RatingTextColumn.MinimumWidth = 25;
            RatingTextColumn.ReadOnly = true;
            RatingTextColumn.SortMode = DataGridViewColumnSortMode.Programmatic;
            // 
            // FinishedColumn
            // 
            FinishedColumn.ImageLayout = DataGridViewImageCellLayout.Zoom;
            FinishedColumn.ReadOnly = true;
            FinishedColumn.Resizable = DataGridViewTriState.False;
            FinishedColumn.SortMode = DataGridViewColumnSortMode.Programmatic;
            FinishedColumn.Width = 71;
            // 
            // ReleaseDateColumn
            // 
            ReleaseDateColumn.MinimumWidth = 25;
            ReleaseDateColumn.ReadOnly = true;
            ReleaseDateColumn.SortMode = DataGridViewColumnSortMode.Programmatic;
            // 
            // LastPlayedColumn
            // 
            LastPlayedColumn.MinimumWidth = 25;
            LastPlayedColumn.ReadOnly = true;
            LastPlayedColumn.SortMode = DataGridViewColumnSortMode.Programmatic;
            //
            // DateAddedColumn
            // 
            DateAddedColumn.MinimumWidth = 25;
            DateAddedColumn.ReadOnly = true;
            DateAddedColumn.SortMode = DataGridViewColumnSortMode.Programmatic;
            // 
            // DisabledModsColumn
            // 
            DisabledModsColumn.MinimumWidth = 25;
            DisabledModsColumn.ReadOnly = true;
            DisabledModsColumn.SortMode = DataGridViewColumnSortMode.Programmatic;
            // 
            // CommentColumn
            // 
            CommentColumn.MinimumWidth = 25;
            CommentColumn.ReadOnly = true;
            CommentColumn.SortMode = DataGridViewColumnSortMode.Programmatic;
            // 
            // FilterBarFLP
            // 
            FilterBarFLP.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            FilterBarFLP.AutoScroll = true;
            FilterBarFLP.Controls.Add(FilterGameButtonsToolStrip);
            FilterBarFLP.Controls.Add(GameFilterControlsShowHideButtonToolStrip);
            FilterBarFLP.Controls.Add(FilterTitleLabel);
            FilterBarFLP.Controls.Add(FilterTitleTextBox);
            FilterBarFLP.Controls.Add(FilterAuthorLabel);
            FilterBarFLP.Controls.Add(FilterAuthorTextBox);
            FilterBarFLP.Controls.Add(FilterIconButtonsToolStrip);
            FilterBarFLP.Size = new Size(768, 100);
            FilterBarFLP.TabIndex = 11;
            FilterBarFLP.WrapContents = false;
            FilterBarFLP.Scroll += FilterBarFLP_Scroll;
            FilterBarFLP.SizeChanged += FilterBarFLP_SizeChanged;
            FilterBarFLP.Paint += FilterBarFLP_Paint;
            // 
            // FilterGameButtonsToolStrip
            // 
            FilterGameButtonsToolStrip.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            FilterGameButtonsToolStrip.BackColor = SystemColors.Control;
            FilterGameButtonsToolStrip.CanOverflow = false;
            FilterGameButtonsToolStrip.Dock = DockStyle.None;
            FilterGameButtonsToolStrip.GripMargin = new Padding(0);
            FilterGameButtonsToolStrip.GripStyle = ToolStripGripStyle.Hidden;
            FilterGameButtonsToolStrip.ImageScalingSize = new Size(22, 22);
            FilterGameButtonsToolStrip.Items.AddRange(new ToolStripItem[] {
            FilterByThief1Button,
            FilterByThief2Button,
            FilterByThief3Button,
            FilterBySS2Button});
            FilterGameButtonsToolStrip.LayoutStyle = ToolStripLayoutStyle.Flow;
            FilterGameButtonsToolStrip.TabIndex = 3;
            // 
            // FilterByThief1Button
            // 
            FilterByThief1Button.AutoSize = false;
            FilterByThief1Button.CheckOnClick = true;
            FilterByThief1Button.DisplayStyle = ToolStripItemDisplayStyle.Image;
            FilterByThief1Button.Margin = new Padding(1, 0, 0, 0);
            FilterByThief1Button.Size = new Size(25, 25);
            FilterByThief1Button.Click += Filters_Changed;
            // 
            // FilterByThief2Button
            // 
            FilterByThief2Button.AutoSize = false;
            FilterByThief2Button.CheckOnClick = true;
            FilterByThief2Button.DisplayStyle = ToolStripItemDisplayStyle.Image;
            FilterByThief2Button.Margin = new Padding(0);
            FilterByThief2Button.Size = new Size(25, 25);
            FilterByThief2Button.Click += Filters_Changed;
            // 
            // FilterByThief3Button
            // 
            FilterByThief3Button.AutoSize = false;
            FilterByThief3Button.CheckOnClick = true;
            FilterByThief3Button.DisplayStyle = ToolStripItemDisplayStyle.Image;
            FilterByThief3Button.Margin = new Padding(0);
            FilterByThief3Button.Size = new Size(25, 25);
            FilterByThief3Button.Click += Filters_Changed;
            // 
            // FilterBySS2Button
            // 
            FilterBySS2Button.AutoSize = false;
            FilterBySS2Button.CheckOnClick = true;
            FilterBySS2Button.DisplayStyle = ToolStripItemDisplayStyle.Image;
            FilterBySS2Button.Margin = new Padding(0, 0, 2, 0);
            FilterBySS2Button.Size = new Size(25, 25);
            FilterBySS2Button.Click += Filters_Changed;
            // 
            // GameFilterControlsShowHideButtonToolStrip
            // 
            GameFilterControlsShowHideButtonToolStrip.AutoSize = false;
            GameFilterControlsShowHideButtonToolStrip.BackColor = SystemColors.Control;
            GameFilterControlsShowHideButtonToolStrip.CanOverflow = false;
            GameFilterControlsShowHideButtonToolStrip.Dock = DockStyle.None;
            GameFilterControlsShowHideButtonToolStrip.GripMargin = new Padding(0);
            GameFilterControlsShowHideButtonToolStrip.GripStyle = ToolStripGripStyle.Hidden;
            GameFilterControlsShowHideButtonToolStrip.Items.Add(GameFilterControlsShowHideButton);
            GameFilterControlsShowHideButtonToolStrip.LayoutStyle = ToolStripLayoutStyle.Flow;
            GameFilterControlsShowHideButtonToolStrip.Padding = new Padding(0);
            GameFilterControlsShowHideButtonToolStrip.Size = new Size(13, 26);
            GameFilterControlsShowHideButtonToolStrip.TabIndex = 0;
            // 
            // GameFilterControlsShowHideButton
            // 
            GameFilterControlsShowHideButton.ArrowDirection = Direction.Down;
            GameFilterControlsShowHideButton.AutoSize = false;
            GameFilterControlsShowHideButton.DisplayStyle = ToolStripItemDisplayStyle.None;
            GameFilterControlsShowHideButton.Size = new Size(11, 23);
            GameFilterControlsShowHideButton.Click += GameFilterControlsShowHideButton_Click;
            // 
            // FilterTitleLabel
            // 
            FilterTitleLabel.AutoSize = true;
            FilterTitleLabel.Margin = new Padding(10, 6, 0, 0);
            FilterTitleLabel.TabIndex = 5;
            // 
            // FilterTitleTextBox
            // 
            FilterTitleTextBox.Size = new Size(144, 20);
            FilterTitleTextBox.TabIndex = 6;
            FilterTitleTextBox.TextChanged += Filters_Changed;
            // 
            // FilterAuthorLabel
            // 
            FilterAuthorLabel.AutoSize = true;
            FilterAuthorLabel.Margin = new Padding(9, 6, 0, 0);
            FilterAuthorLabel.TabIndex = 7;
            // 
            // FilterAuthorTextBox
            // 
            FilterAuthorTextBox.Size = new Size(144, 20);
            FilterAuthorTextBox.TabIndex = 8;
            FilterAuthorTextBox.TextChanged += Filters_Changed;
            // 
            // FilterIconButtonsToolStrip
            // 
            FilterIconButtonsToolStrip.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            FilterIconButtonsToolStrip.BackColor = SystemColors.Control;
            FilterIconButtonsToolStrip.CanOverflow = false;
            FilterIconButtonsToolStrip.Dock = DockStyle.None;
            FilterIconButtonsToolStrip.GripMargin = new Padding(0);
            FilterIconButtonsToolStrip.GripStyle = ToolStripGripStyle.Hidden;
            FilterIconButtonsToolStrip.ImageScalingSize = new Size(22, 22);
            FilterIconButtonsToolStrip.Items.AddRange(new ToolStripItem[] {
            FilterByReleaseDateButton,
            FilterByLastPlayedButton,
            FilterByTagsButton,
            FilterByFinishedButton,
            FilterByUnfinishedButton,
            FilterByRatingButton,
            FilterShowUnsupportedButton,
            FilterShowUnavailableButton,
            FilterShowRecentAtTopButton,
            FilterControlsShowHideButton});
            FilterIconButtonsToolStrip.LayoutStyle = ToolStripLayoutStyle.Flow;
            FilterIconButtonsToolStrip.TabIndex = 3;
            FilterIconButtonsToolStrip.Paint += FilterIconButtonsToolStrip_Paint;
            // 
            // FilterByReleaseDateButton
            // 
            FilterByReleaseDateButton.AutoSize = false;
            FilterByReleaseDateButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
            FilterByReleaseDateButton.Margin = new Padding(6, 0, 0, 0);
            FilterByReleaseDateButton.Size = new Size(25, 25);
            FilterByReleaseDateButton.Click += FilterWindowOpenButtons_Click;
            // 
            // FilterByLastPlayedButton
            // 
            FilterByLastPlayedButton.AutoSize = false;
            FilterByLastPlayedButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
            FilterByLastPlayedButton.Margin = new Padding(6, 0, 0, 0);
            FilterByLastPlayedButton.Size = new Size(25, 25);
            FilterByLastPlayedButton.Click += FilterWindowOpenButtons_Click;
            // 
            // FilterByTagsButton
            // 
            FilterByTagsButton.AutoSize = false;
            FilterByTagsButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
            FilterByTagsButton.Margin = new Padding(6, 0, 0, 0);
            FilterByTagsButton.Size = new Size(25, 25);
            FilterByTagsButton.Click += FilterWindowOpenButtons_Click;
            // 
            // FilterByFinishedButton
            // 
            FilterByFinishedButton.AutoSize = false;
            FilterByFinishedButton.CheckOnClick = true;
            FilterByFinishedButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
            FilterByFinishedButton.Margin = new Padding(6, 0, 0, 0);
            FilterByFinishedButton.Size = new Size(25, 25);
            FilterByFinishedButton.Click += Filters_Changed;
            // 
            // FilterByUnfinishedButton
            // 
            FilterByUnfinishedButton.AutoSize = false;
            FilterByUnfinishedButton.CheckOnClick = true;
            FilterByUnfinishedButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
            FilterByUnfinishedButton.Margin = new Padding(0);
            FilterByUnfinishedButton.Size = new Size(25, 25);
            FilterByUnfinishedButton.Click += Filters_Changed;
            // 
            // FilterByRatingButton
            // 
            FilterByRatingButton.AutoSize = false;
            FilterByRatingButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
            FilterByRatingButton.Margin = new Padding(6, 0, 0, 0);
            FilterByRatingButton.Size = new Size(25, 25);
            FilterByRatingButton.Click += FilterWindowOpenButtons_Click;
            // 
            // FilterShowUnsupportedButton
            // 
            FilterShowUnsupportedButton.AutoSize = false;
            FilterShowUnsupportedButton.CheckOnClick = true;
            FilterShowUnsupportedButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
            FilterShowUnsupportedButton.Image = Resources.Show_Unsupported;
            FilterShowUnsupportedButton.Margin = new Padding(6, 0, 0, 0);
            FilterShowUnsupportedButton.Size = new Size(25, 25);
            FilterShowUnsupportedButton.Click += Filters_Changed;
            // 
            // FilterShowUnavailableButton
            // 
            FilterShowUnavailableButton.AutoSize = false;
            FilterShowUnavailableButton.CheckOnClick = true;
            FilterShowUnavailableButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
            FilterShowUnavailableButton.Image = Resources.Show_Unavailable;
            FilterShowUnavailableButton.Margin = new Padding(0);
            FilterShowUnavailableButton.Size = new Size(25, 25);
            FilterShowUnavailableButton.Click += Filters_Changed;
            // 
            // FilterShowRecentAtTopButton
            // 
            FilterShowRecentAtTopButton.AutoSize = false;
            FilterShowRecentAtTopButton.CheckOnClick = true;
            FilterShowRecentAtTopButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
            FilterShowRecentAtTopButton.Margin = new Padding(6, 0, 2, 0);
            FilterShowRecentAtTopButton.Size = new Size(25, 25);
            FilterShowRecentAtTopButton.Click += Filters_Changed;
            // 
            // FilterControlsShowHideButton
            // 
            FilterControlsShowHideButton.ArrowDirection = Direction.Down;
            FilterControlsShowHideButton.AutoSize = false;
            FilterControlsShowHideButton.DisplayStyle = ToolStripItemDisplayStyle.None;
            FilterControlsShowHideButton.Margin = new Padding(4, 1, 0, 2);
            FilterControlsShowHideButton.Size = new Size(11, 23);
            FilterControlsShowHideButton.Click += FilterControlsShowHideButton_Click;
            // 
            // RefreshAreaToolStrip
            // 
            RefreshAreaToolStrip.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right;
            RefreshAreaToolStrip.BackColor = SystemColors.Control;
            RefreshAreaToolStrip.CanOverflow = false;
            RefreshAreaToolStrip.Dock = DockStyle.None;
            RefreshAreaToolStrip.GripMargin = new Padding(0);
            RefreshAreaToolStrip.GripStyle = ToolStripGripStyle.Hidden;
            RefreshAreaToolStrip.ImageScalingSize = new Size(22, 22);
            RefreshAreaToolStrip.Items.AddRange(new ToolStripItem[] {
            RefreshFromDiskButton,
            RefreshFiltersButton,
            ClearFiltersButton});
            RefreshAreaToolStrip.LayoutStyle = ToolStripLayoutStyle.Flow;
            RefreshAreaToolStrip.Location = new Point(994, 0);
            RefreshAreaToolStrip.Size = new Size(91, 26);
            RefreshAreaToolStrip.TabIndex = 12;
            RefreshAreaToolStrip.Paint += RefreshAreaToolStrip_Paint;
            // 
            // RefreshFromDiskButton
            // 
            RefreshFromDiskButton.AutoSize = false;
            RefreshFromDiskButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
            RefreshFromDiskButton.Margin = new Padding(0);
            RefreshFromDiskButton.Size = new Size(25, 25);
            RefreshFromDiskButton.Click += Filters_Changed;
            RefreshFromDiskButton.Margin = new Padding(6, 0, 0, 0);
            // 
            // RefreshFiltersButton
            // 
            RefreshFiltersButton.AutoSize = false;
            RefreshFiltersButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
            RefreshFiltersButton.Margin = new Padding(0);
            RefreshFiltersButton.Size = new Size(25, 25);
            RefreshFiltersButton.Click += Filters_Changed;
            // 
            // ClearFiltersButton
            // 
            ClearFiltersButton.AutoSize = false;
            ClearFiltersButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
            ClearFiltersButton.Margin = new Padding(0, 0, 9, 1);
            ClearFiltersButton.Size = new Size(25, 25);
            ClearFiltersButton.Click += Filters_Changed;
            // 
            // ResetLayoutButton
            // 
            ResetLayoutButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            ResetLayoutButton.FlatAppearance.BorderSize = 0;
            ResetLayoutButton.FlatStyle = FlatStyle.Flat;
            ResetLayoutButton.Location = new Point(1090, 2);
            ResetLayoutButton.Size = new Size(21, 21);
            ResetLayoutButton.TabIndex = 13;
            ResetLayoutButton.UseVisualStyleBackColor = true;
            ResetLayoutButton.Click += ResetLayoutButton_Click;
            ResetLayoutButton.PaintCustom += ResetLayoutButton_Paint;
            // 
            // GamesTabControl
            // 
            GamesTabControl.Controls.Add(Thief1TabPage);
            GamesTabControl.Controls.Add(Thief2TabPage);
            GamesTabControl.Controls.Add(Thief3TabPage);
            GamesTabControl.Controls.Add(SS2TabPage);
            GamesTabControl.ImageList = GameTabsImageList;
            GamesTabControl.Location = new Point(28, 5);
            GamesTabControl.SelectedIndex = 0;
            GamesTabControl.Size = new Size(1075, 24);
            GamesTabControl.TabIndex = 1;
            GamesTabControl.SelectedIndexChanged += GamesTabControl_SelectedIndexChanged;
            GamesTabControl.Deselecting += GamesTabControl_Deselecting;
            // 
            // Thief1TabPage
            // 
            Thief1TabPage.ImageIndex = 0;
            Thief1TabPage.TabIndex = 0;
            // 
            // Thief2TabPage
            // 
            Thief2TabPage.ImageIndex = 1;
            Thief2TabPage.TabIndex = 1;
            // 
            // Thief3TabPage
            // 
            Thief3TabPage.ImageIndex = 2;
            Thief3TabPage.TabIndex = 2;
            //
            // SS2TabPage
            //
            SS2TabPage.ImageIndex = 3;
            SS2TabPage.TabIndex = 3;
            // 
            // TopRightMenuButton
            // 
            TopRightMenuButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            TopRightMenuButton.FlatAppearance.BorderSize = 0;
            TopRightMenuButton.FlatStyle = FlatStyle.Flat;
            TopRightMenuButton.Location = new Point(533, 0);
            TopRightMenuButton.Size = new Size(18, 20);
            TopRightMenuButton.TabIndex = 13;
            TopRightMenuButton.UseVisualStyleBackColor = true;
            TopRightMenuButton.Click += TopRightMenuButton_Click;
            TopRightMenuButton.PaintCustom += TopRightMenuButton_Paint;
            // 
            // TopRightCollapseButton
            // 
            TopRightCollapseButton.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right;
            TopRightCollapseButton.ArrowDirection = Direction.Right;
            TopRightCollapseButton.BackgroundImageLayout = ImageLayout.None;
            TopRightCollapseButton.FlatAppearance.BorderSize = 0;
            TopRightCollapseButton.FlatStyle = FlatStyle.Flat;
            TopRightCollapseButton.Location = new Point(533, 20);
            TopRightCollapseButton.Size = new Size(18, 289);
            TopRightCollapseButton.TabIndex = 14;
            TopRightCollapseButton.UseVisualStyleBackColor = true;
            TopRightCollapseButton.Click += TopRightCollapseButton_Click;
            // 
            // TopRightTabControl
            // 
            TopRightTabControl.AllowReordering = true;
            TopRightTabControl.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            TopRightTabControl.Controls.Add(StatisticsTabPage);
            TopRightTabControl.Controls.Add(EditFMTabPage);
            TopRightTabControl.Controls.Add(CommentTabPage);
            TopRightTabControl.Controls.Add(TagsTabPage);
            TopRightTabControl.Controls.Add(PatchTabPage);
            TopRightTabControl.Controls.Add(ModsTabPage);
            TopRightTabControl.Size = new Size(535, 310);
            TopRightTabControl.TabIndex = 15;
            // 
            // StatisticsTabPage
            // 
            StatisticsTabPage.AutoScroll = true;
            StatisticsTabPage.BackColor = SystemColors.Control;
            StatisticsTabPage.Controls.Add(StatsScanCustomResourcesButton);
            StatisticsTabPage.Controls.Add(StatsCheckBoxesPanel);
            StatisticsTabPage.Controls.Add(CustomResourcesLabel);
            StatisticsTabPage.Size = new Size(526, 284);
            StatisticsTabPage.TabIndex = 0;
            // 
            // StatsScanCustomResourcesButton
            // 
            StatsScanCustomResourcesButton.AutoSize = true;
            StatsScanCustomResourcesButton.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            StatsScanCustomResourcesButton.Location = new Point(6, 200);
            StatsScanCustomResourcesButton.MinimumSize = new Size(0, 23);
            StatsScanCustomResourcesButton.Padding = new Padding(13, 0, 0, 0);
            StatsScanCustomResourcesButton.TabIndex = 12;
            StatsScanCustomResourcesButton.UseVisualStyleBackColor = true;
            StatsScanCustomResourcesButton.PaintCustom += ScanIconButtons_Paint;
            StatsScanCustomResourcesButton.Click += FieldScanButtons_Click;
            // 
            // StatsCheckBoxesPanel
            // 
            StatsCheckBoxesPanel.AutoSize = true;
            StatsCheckBoxesPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            StatsCheckBoxesPanel.Controls.Add(CR_MapCheckBox);
            StatsCheckBoxesPanel.Controls.Add(CR_MoviesCheckBox);
            StatsCheckBoxesPanel.Controls.Add(CR_MotionsCheckBox);
            StatsCheckBoxesPanel.Controls.Add(CR_SoundsCheckBox);
            StatsCheckBoxesPanel.Controls.Add(CR_CreaturesCheckBox);
            StatsCheckBoxesPanel.Controls.Add(CR_TexturesCheckBox);
            StatsCheckBoxesPanel.Controls.Add(CR_AutomapCheckBox);
            StatsCheckBoxesPanel.Controls.Add(CR_ScriptsCheckBox);
            StatsCheckBoxesPanel.Controls.Add(CR_SubtitlesCheckBox);
            StatsCheckBoxesPanel.Controls.Add(CR_ObjectsCheckBox);
            StatsCheckBoxesPanel.Location = new Point(8, 32);
            StatsCheckBoxesPanel.TabIndex = 1;
            StatsCheckBoxesPanel.Visible = false;
            // 
            // CR_MapCheckBox
            // 
            CR_MapCheckBox.AutoCheck = false;
            CR_MapCheckBox.AutoSize = true;
            CR_MapCheckBox.TabIndex = 2;
            CR_MapCheckBox.UseVisualStyleBackColor = true;
            // 
            // CR_MoviesCheckBox
            // 
            CR_MoviesCheckBox.AutoCheck = false;
            CR_MoviesCheckBox.AutoSize = true;
            CR_MoviesCheckBox.Location = new Point(0, 64);
            CR_MoviesCheckBox.TabIndex = 6;
            CR_MoviesCheckBox.UseVisualStyleBackColor = true;
            // 
            // CR_MotionsCheckBox
            // 
            CR_MotionsCheckBox.AutoCheck = false;
            CR_MotionsCheckBox.AutoSize = true;
            CR_MotionsCheckBox.Location = new Point(0, 112);
            CR_MotionsCheckBox.TabIndex = 9;
            CR_MotionsCheckBox.UseVisualStyleBackColor = true;
            // 
            // CR_SoundsCheckBox
            // 
            CR_SoundsCheckBox.AutoCheck = false;
            CR_SoundsCheckBox.AutoSize = true;
            CR_SoundsCheckBox.Location = new Point(0, 48);
            CR_SoundsCheckBox.TabIndex = 5;
            CR_SoundsCheckBox.UseVisualStyleBackColor = true;
            // 
            // CR_CreaturesCheckBox
            // 
            CR_CreaturesCheckBox.AutoCheck = false;
            CR_CreaturesCheckBox.AutoSize = true;
            CR_CreaturesCheckBox.Location = new Point(0, 96);
            CR_CreaturesCheckBox.TabIndex = 8;
            CR_CreaturesCheckBox.UseVisualStyleBackColor = true;
            // 
            // CR_TexturesCheckBox
            // 
            CR_TexturesCheckBox.AutoCheck = false;
            CR_TexturesCheckBox.AutoSize = true;
            CR_TexturesCheckBox.Location = new Point(0, 32);
            CR_TexturesCheckBox.TabIndex = 4;
            CR_TexturesCheckBox.UseVisualStyleBackColor = true;
            // 
            // CR_AutomapCheckBox
            // 
            CR_AutomapCheckBox.AutoCheck = false;
            CR_AutomapCheckBox.AutoSize = true;
            CR_AutomapCheckBox.Location = new Point(0, 16);
            CR_AutomapCheckBox.TabIndex = 3;
            CR_AutomapCheckBox.UseVisualStyleBackColor = true;
            // 
            // CR_ScriptsCheckBox
            // 
            CR_ScriptsCheckBox.AutoCheck = false;
            CR_ScriptsCheckBox.AutoSize = true;
            CR_ScriptsCheckBox.Location = new Point(0, 128);
            CR_ScriptsCheckBox.TabIndex = 10;
            CR_ScriptsCheckBox.UseVisualStyleBackColor = true;
            // 
            // CR_SubtitlesCheckBox
            // 
            CR_SubtitlesCheckBox.AutoCheck = false;
            CR_SubtitlesCheckBox.AutoSize = true;
            CR_SubtitlesCheckBox.Location = new Point(0, 144);
            CR_SubtitlesCheckBox.TabIndex = 11;
            CR_SubtitlesCheckBox.UseVisualStyleBackColor = true;
            // 
            // CR_ObjectsCheckBox
            // 
            CR_ObjectsCheckBox.AutoCheck = false;
            CR_ObjectsCheckBox.AutoSize = true;
            CR_ObjectsCheckBox.Location = new Point(0, 80);
            CR_ObjectsCheckBox.TabIndex = 7;
            CR_ObjectsCheckBox.UseVisualStyleBackColor = true;
            // 
            // CustomResourcesLabel
            // 
            CustomResourcesLabel.AutoSize = true;
            CustomResourcesLabel.Location = new Point(4, 10);
            CustomResourcesLabel.TabIndex = 0;
            // 
            // EditFMTabPage
            // 
            EditFMTabPage.AutoScroll = true;
            EditFMTabPage.BackColor = SystemColors.Control;
            EditFMTabPage.Controls.Add(EditFMScanLanguagesButton);
            EditFMTabPage.Controls.Add(EditFMLanguageLabel);
            EditFMTabPage.Controls.Add(EditFMLanguageComboBox);
            EditFMTabPage.Controls.Add(EditFMScanForReadmesButton);
            EditFMTabPage.Controls.Add(EditFMScanReleaseDateButton);
            EditFMTabPage.Controls.Add(EditFMScanAuthorButton);
            EditFMTabPage.Controls.Add(EditFMScanTitleButton);
            EditFMTabPage.Controls.Add(EditFMAltTitlesArrowButton);
            EditFMTabPage.Controls.Add(EditFMTitleTextBox);
            EditFMTabPage.Controls.Add(EditFMFinishedOnButton);
            EditFMTabPage.Controls.Add(EditFMRatingComboBox);
            EditFMTabPage.Controls.Add(EditFMRatingLabel);
            EditFMTabPage.Controls.Add(EditFMLastPlayedDateTimePicker);
            EditFMTabPage.Controls.Add(EditFMReleaseDateDateTimePicker);
            EditFMTabPage.Controls.Add(EditFMLastPlayedCheckBox);
            EditFMTabPage.Controls.Add(EditFMReleaseDateCheckBox);
            EditFMTabPage.Controls.Add(EditFMAuthorTextBox);
            EditFMTabPage.Controls.Add(EditFMAuthorLabel);
            EditFMTabPage.Controls.Add(EditFMTitleLabel);
            EditFMTabPage.Size = new Size(526, 284);
            EditFMTabPage.TabIndex = 2;
            // 
            // EditFMScanLanguagesButton
            // 
            EditFMScanLanguagesButton.Location = new Point(137, 200);
            EditFMScanLanguagesButton.Size = new Size(22, 23);
            EditFMScanLanguagesButton.TabIndex = 33;
            EditFMScanLanguagesButton.UseVisualStyleBackColor = true;
            EditFMScanLanguagesButton.PaintCustom += ScanIconButtons_Paint;
            EditFMScanLanguagesButton.Click += EditFMScanLanguagesButton_Click;
            // 
            // EditFMLanguageLabel
            // 
            EditFMLanguageLabel.AutoSize = true;
            EditFMLanguageLabel.Location = new Point(8, 185);
            EditFMLanguageLabel.TabIndex = 31;
            // 
            // EditFMLanguageComboBox
            // 
            EditFMLanguageComboBox.FormattingEnabled = true;
            EditFMLanguageComboBox.Location = new Point(9, 201);
            EditFMLanguageComboBox.Size = new Size(128, 21);
            EditFMLanguageComboBox.TabIndex = 32;
            EditFMLanguageComboBox.SelectedIndexChanged += EditFMLanguageComboBox_SelectedIndexChanged;
            // 
            // EditFMScanForReadmesButton
            // 
            EditFMScanForReadmesButton.AutoSize = true;
            EditFMScanForReadmesButton.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            EditFMScanForReadmesButton.Location = new Point(8, 238);
            EditFMScanForReadmesButton.MinimumSize = new Size(130, 23);
            EditFMScanForReadmesButton.Padding = new Padding(13, 0, 0, 0);
            EditFMScanForReadmesButton.TabIndex = 34;
            EditFMScanForReadmesButton.UseVisualStyleBackColor = true;
            EditFMScanForReadmesButton.PaintCustom += ScanIconButtons_Paint;
            EditFMScanForReadmesButton.Click += FieldScanButtons_Click;
            // 
            // EditFMScanReleaseDateButton
            // 
            EditFMScanReleaseDateButton.Location = new Point(136, 105);
            EditFMScanReleaseDateButton.Size = new Size(22, 22);
            EditFMScanReleaseDateButton.TabIndex = 22;
            EditFMScanReleaseDateButton.UseVisualStyleBackColor = true;
            EditFMScanReleaseDateButton.PaintCustom += ScanIconButtons_Paint;
            EditFMScanReleaseDateButton.Click += FieldScanButtons_Click;
            // 
            // EditFMScanAuthorButton
            // 
            EditFMScanAuthorButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            EditFMScanAuthorButton.Location = new Point(493, 63);
            EditFMScanAuthorButton.Size = new Size(22, 22);
            EditFMScanAuthorButton.TabIndex = 19;
            EditFMScanAuthorButton.UseVisualStyleBackColor = true;
            EditFMScanAuthorButton.PaintCustom += ScanIconButtons_Paint;
            EditFMScanAuthorButton.Click += FieldScanButtons_Click;
            // 
            // EditFMScanTitleButton
            // 
            EditFMScanTitleButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            EditFMScanTitleButton.Location = new Point(493, 23);
            EditFMScanTitleButton.Size = new Size(22, 22);
            EditFMScanTitleButton.TabIndex = 16;
            EditFMScanTitleButton.UseVisualStyleBackColor = true;
            EditFMScanTitleButton.PaintCustom += ScanIconButtons_Paint;
            EditFMScanTitleButton.Click += FieldScanButtons_Click;
            // 
            // EditFMAltTitlesArrowButton
            // 
            EditFMAltTitlesArrowButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            EditFMAltTitlesArrowButton.ArrowDirection = Direction.Down;
            EditFMAltTitlesArrowButton.Location = new Point(477, 23);
            EditFMAltTitlesArrowButton.Size = new Size(17, 22);
            EditFMAltTitlesArrowButton.TabIndex = 15;
            EditFMAltTitlesArrowButton.UseVisualStyleBackColor = true;
            EditFMAltTitlesArrowButton.Click += EditFMAltTitlesArrowButton_Click;
            // 
            // EditFMTitleTextBox
            // 
            EditFMTitleTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            EditFMTitleTextBox.Location = new Point(8, 24);
            EditFMTitleTextBox.Size = new Size(469, 20);
            EditFMTitleTextBox.TabIndex = 14;
            EditFMTitleTextBox.TextChanged += EditFMTitleTextBox_TextChanged;
            EditFMTitleTextBox.Leave += EditFMTitleTextBox_Leave;
            // 
            // EditFMFinishedOnButton
            // 
            EditFMFinishedOnButton.AutoSize = true;
            EditFMFinishedOnButton.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            EditFMFinishedOnButton.Location = new Point(184, 144);
            EditFMFinishedOnButton.MinimumSize = new Size(138, 23);
            EditFMFinishedOnButton.Padding = new Padding(6, 0, 6, 0);
            EditFMFinishedOnButton.TabIndex = 27;
            EditFMFinishedOnButton.UseVisualStyleBackColor = true;
            EditFMFinishedOnButton.Click += EditFMFinishedOnButton_Click;
            // 
            // EditFMRatingComboBox
            // 
            EditFMRatingComboBox.FormattingEnabled = true;
            EditFMRatingComboBox.Items.AddRange(new object[] {
            "Unrated",
            "0",
            "1",
            "2",
            "3",
            "4",
            "5",
            "6",
            "7",
            "8",
            "9",
            "10"});
            EditFMRatingComboBox.Location = new Point(185, 104);
            EditFMRatingComboBox.Size = new Size(136, 21);
            EditFMRatingComboBox.TabIndex = 26;
            EditFMRatingComboBox.SelectedIndexChanged += EditFMRatingComboBox_SelectedIndexChanged;
            // 
            // EditFMRatingLabel
            // 
            EditFMRatingLabel.AutoSize = true;
            EditFMRatingLabel.Location = new Point(185, 87);
            EditFMRatingLabel.TabIndex = 25;
            // 
            // EditFMLastPlayedDateTimePicker
            // 
            EditFMLastPlayedDateTimePicker.Format = DateTimePickerFormat.Short;
            EditFMLastPlayedDateTimePicker.Location = new Point(8, 148);
            EditFMLastPlayedDateTimePicker.Size = new Size(128, 20);
            EditFMLastPlayedDateTimePicker.TabIndex = 24;
            EditFMLastPlayedDateTimePicker.Visible = false;
            EditFMLastPlayedDateTimePicker.ValueChanged += EditFMLastPlayedDateTimePicker_ValueChanged;
            // 
            // EditFMReleaseDateDateTimePicker
            // 
            EditFMReleaseDateDateTimePicker.Format = DateTimePickerFormat.Short;
            EditFMReleaseDateDateTimePicker.Location = new Point(8, 106);
            EditFMReleaseDateDateTimePicker.Size = new Size(128, 20);
            EditFMReleaseDateDateTimePicker.TabIndex = 21;
            EditFMReleaseDateDateTimePicker.Visible = false;
            EditFMReleaseDateDateTimePicker.ValueChanged += EditFMReleaseDateDateTimePicker_ValueChanged;
            // 
            // EditFMLastPlayedCheckBox
            // 
            EditFMLastPlayedCheckBox.AutoSize = true;
            EditFMLastPlayedCheckBox.Location = new Point(8, 130);
            EditFMLastPlayedCheckBox.TabIndex = 23;
            EditFMLastPlayedCheckBox.UseVisualStyleBackColor = true;
            EditFMLastPlayedCheckBox.CheckedChanged += EditFMLastPlayedCheckBox_CheckedChanged;
            // 
            // EditFMReleaseDateCheckBox
            // 
            EditFMReleaseDateCheckBox.AutoSize = true;
            EditFMReleaseDateCheckBox.Location = new Point(8, 88);
            EditFMReleaseDateCheckBox.TabIndex = 20;
            EditFMReleaseDateCheckBox.UseVisualStyleBackColor = true;
            EditFMReleaseDateCheckBox.CheckedChanged += EditFMReleaseDateCheckBox_CheckedChanged;
            // 
            // EditFMAuthorTextBox
            // 
            EditFMAuthorTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            EditFMAuthorTextBox.Location = new Point(8, 64);
            EditFMAuthorTextBox.Size = new Size(485, 20);
            EditFMAuthorTextBox.TabIndex = 18;
            EditFMAuthorTextBox.TextChanged += EditFMAuthorTextBox_TextChanged;
            EditFMAuthorTextBox.Leave += EditFMAuthorTextBox_Leave;
            // 
            // EditFMAuthorLabel
            // 
            EditFMAuthorLabel.AutoSize = true;
            EditFMAuthorLabel.Location = new Point(8, 48);
            EditFMAuthorLabel.TabIndex = 17;
            // 
            // EditFMTitleLabel
            // 
            EditFMTitleLabel.AutoSize = true;
            EditFMTitleLabel.Location = new Point(8, 8);
            EditFMTitleLabel.TabIndex = 13;
            // 
            // CommentTabPage
            // 
            CommentTabPage.BackColor = SystemColors.Control;
            CommentTabPage.Controls.Add(CommentTextBox);
            CommentTabPage.Size = new Size(526, 284);
            CommentTabPage.TabIndex = 0;
            // 
            // CommentTextBox
            // 
            CommentTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            CommentTextBox.Location = new Point(8, 8);
            CommentTextBox.Multiline = true;
            CommentTextBox.ScrollBars = ScrollBars.Vertical;
            CommentTextBox.Size = new Size(510, 266);
            CommentTextBox.TabIndex = 32;
            CommentTextBox.TextChanged += CommentTextBox_TextChanged;
            CommentTextBox.Leave += CommentTextBox_Leave;
            // 
            // TagsTabPage
            // 
            TagsTabPage.AutoScroll = true;
            TagsTabPage.BackColor = SystemColors.Control;
            TagsTabPage.Controls.Add(AddTagButton);
            TagsTabPage.Controls.Add(AddTagTextBox);
            TagsTabPage.Controls.Add(AddRemoveTagFLP);
            TagsTabPage.Controls.Add(TagsTreeView);
            TagsTabPage.Controls.Add(TagsTabAutoScrollMarker);
            TagsTabPage.Size = new Size(526, 284);
            TagsTabPage.TabIndex = 1;
            // 
            // AddTagButton
            // 
            AddTagButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            AddTagButton.AutoSize = true;
            AddTagButton.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            AddTagButton.Location = new Point(453, 7);
            AddTagButton.MinimumSize = new Size(0, 23);
            AddTagButton.Padding = new Padding(6, 0, 6, 0);
            AddTagButton.Size = new Size(66, 23);
            AddTagButton.TabIndex = 1;
            AddTagButton.UseVisualStyleBackColor = true;
            AddTagButton.Click += AddTagButton_Click;
            // 
            // AddTagTextBox
            // 
            AddTagTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            AddTagTextBox.DisallowedCharacters = ",;";
            AddTagTextBox.Location = new Point(8, 8);
            AddTagTextBox.Size = new Size(440, 20);
            AddTagTextBox.StrictTextChangedEvent = false;
            AddTagTextBox.TabIndex = 0;
            AddTagTextBox.TextChanged += AddTagTextBox_TextChanged;
            AddTagTextBox.KeyDown += AddTagTextBoxOrListBox_KeyDown;
            AddTagTextBox.Leave += AddTagTextBoxOrListBox_Leave;
            // 
            // AddRemoveTagFLP
            // 
            AddRemoveTagFLP.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            AddRemoveTagFLP.AutoSize = true;
            AddRemoveTagFLP.Controls.Add(RemoveTagButton);
            AddRemoveTagFLP.Controls.Add(AddTagFromListButton);
            AddRemoveTagFLP.FlowDirection = FlowDirection.RightToLeft;
            AddRemoveTagFLP.Location = new Point(0, 248);
            AddRemoveTagFLP.Size = new Size(525, 24);
            AddRemoveTagFLP.TabIndex = 3;
            // 
            // RemoveTagButton
            // 
            RemoveTagButton.AutoSize = true;
            RemoveTagButton.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            RemoveTagButton.Margin = new Padding(0, 0, 6, 0);
            RemoveTagButton.MinimumSize = new Size(0, 23);
            RemoveTagButton.Padding = new Padding(6, 0, 6, 0);
            RemoveTagButton.TabIndex = 1;
            RemoveTagButton.UseVisualStyleBackColor = true;
            RemoveTagButton.Click += RemoveTagButton_Click;
            // 
            // AddTagFromListButton
            // 
            AddTagFromListButton.AutoSize = true;
            AddTagFromListButton.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            AddTagFromListButton.Margin = new Padding(0);
            AddTagFromListButton.MinimumSize = new Size(0, 23);
            AddTagFromListButton.Padding = new Padding(6, 0, 6, 0);
            AddTagFromListButton.TabIndex = 0;
            AddTagFromListButton.UseVisualStyleBackColor = true;
            AddTagFromListButton.Click += AddTagFromListButton_Click;
            // 
            // TagsTreeView
            // 
            TagsTreeView.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            TagsTreeView.HideSelection = false;
            TagsTreeView.Location = new Point(8, 32);
            TagsTreeView.Size = new Size(510, 216);
            TagsTreeView.TabIndex = 2;
            // 
            // TagsTabAutoScrollMarker
            // 
            TagsTabAutoScrollMarker.Size = new Size(240, 152);
            // 
            // PatchTabPage
            // 
            PatchTabPage.AutoScroll = true;
            PatchTabPage.BackColor = SystemColors.Control;
            PatchTabPage.Controls.Add(PatchMainPanel);
            PatchTabPage.Controls.Add(PatchFMNotInstalledLabel);
            PatchTabPage.Size = new Size(526, 284);
            PatchTabPage.TabIndex = 3;
            // 
            // PatchMainPanel
            // 
            PatchMainPanel.AutoSize = true;
            PatchMainPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            PatchMainPanel.Controls.Add(PatchDMLsPanel);
            PatchMainPanel.Controls.Add(PatchOpenFMFolderButton);
            PatchMainPanel.Size = new Size(175, 154);
            PatchMainPanel.TabIndex = 38;
            // 
            // PatchDMLsPanel
            // 
            PatchDMLsPanel.AutoSize = true;
            PatchDMLsPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            PatchDMLsPanel.Controls.Add(PatchDMLPatchesLabel);
            PatchDMLsPanel.Controls.Add(PatchDMLsListBox);
            PatchDMLsPanel.Controls.Add(PatchRemoveDMLButton);
            PatchDMLsPanel.Controls.Add(PatchAddDMLButton);
            PatchDMLsPanel.Size = new Size(172, 120);
            PatchDMLsPanel.TabIndex = 39;
            // 
            // PatchDMLPatchesLabel
            // 
            PatchDMLPatchesLabel.AutoSize = true;
            PatchDMLPatchesLabel.Location = new Point(8, 8);
            PatchDMLPatchesLabel.TabIndex = 40;
            // 
            // PatchDMLsListBox
            // 
            PatchDMLsListBox.Location = new Point(8, 24);
            PatchDMLsListBox.MultiSelect = false;
            PatchDMLsListBox.Size = new Size(160, 69);
            PatchDMLsListBox.TabIndex = 41;
            // 
            // PatchRemoveDMLButton
            // 
            PatchRemoveDMLButton.Location = new Point(122, 94);
            PatchRemoveDMLButton.Size = new Size(23, 23);
            PatchRemoveDMLButton.TabIndex = 42;
            PatchRemoveDMLButton.UseVisualStyleBackColor = true;
            PatchRemoveDMLButton.PaintCustom += PatchRemoveDMLButton_Paint;
            PatchRemoveDMLButton.Click += PatchRemoveDMLButton_Click;
            // 
            // PatchAddDMLButton
            // 
            PatchAddDMLButton.Location = new Point(146, 94);
            PatchAddDMLButton.Size = new Size(23, 23);
            PatchAddDMLButton.TabIndex = 43;
            PatchAddDMLButton.UseVisualStyleBackColor = true;
            PatchAddDMLButton.PaintCustom += PatchAddDMLButton_Paint;
            PatchAddDMLButton.Click += PatchAddDMLButton_Click;
            // 
            // PatchOpenFMFolderButton
            // 
            PatchOpenFMFolderButton.AutoSize = true;
            PatchOpenFMFolderButton.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            PatchOpenFMFolderButton.Location = new Point(7, 128);
            PatchOpenFMFolderButton.MinimumSize = new Size(162, 23);
            PatchOpenFMFolderButton.TabIndex = 44;
            PatchOpenFMFolderButton.UseVisualStyleBackColor = true;
            PatchOpenFMFolderButton.Click += PatchOpenFMFolderButton_Click;
            // 
            // PatchFMNotInstalledLabel
            // 
            PatchFMNotInstalledLabel.Anchor = AnchorStyles.None;
            PatchFMNotInstalledLabel.AutoSize = true;
            // This thing gets centered later so no location is specified here
            PatchFMNotInstalledLabel.TabIndex = 45;
            // 
            // ModsTabPage
            // 
            ModsTabPage.AutoScroll = true;
            ModsTabPage.BackColor = SystemColors.Control;
            ModsTabPage.Controls.Add(ModsDisabledModsTextBox);
            ModsTabPage.Controls.Add(ModsDisabledModsLabel);
            ModsTabPage.Controls.Add(ModsCheckList);
            ModsTabPage.Size = new Size(527, 284);
            ModsTabPage.TabIndex = 4;
            // 
            // ModsDisabledModsTextBox
            // 
            ModsDisabledModsTextBox.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            ModsDisabledModsTextBox.Location = new Point(8, 216);
            ModsDisabledModsTextBox.Size = new Size(512, 20);
            ModsDisabledModsTextBox.TabIndex = 32;
            ModsDisabledModsTextBox.TextChanged += ModsDisabledModsTextBox_TextChanged;
            ModsDisabledModsTextBox.Leave += ModsDisabledModsTextBox_Leave;
            // 
            // ModsDisabledModsLabel
            // 
            ModsDisabledModsLabel.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            ModsDisabledModsLabel.AutoSize = true;
            ModsDisabledModsLabel.Location = new Point(8, 200);
            ModsDisabledModsLabel.TabIndex = 31;
            // 
            // ModsCheckList
            // 
            ModsCheckList.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            ModsCheckList.AutoScroll = true;
            ModsCheckList.BorderStyle = BorderStyle.FixedSingle;
            ModsCheckList.Location = new Point(8, 8);
            ModsCheckList.Size = new Size(512, 184);
            ModsCheckList.TabIndex = 0;
            ModsCheckList.ItemCheckedChanged += ModsCheckList_ItemCheckedChanged;
            // 
            // ReadmeEncodingButton
            // 
            ReadmeEncodingButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            ReadmeEncodingButton.FlatAppearance.BorderSize = 0;
            ReadmeEncodingButton.FlatStyle = FlatStyle.Flat;
            ReadmeEncodingButton.Location = new Point(1502, 8);
            ReadmeEncodingButton.Size = new Size(21, 21);
            ReadmeEncodingButton.TabIndex = 2;
            ReadmeEncodingButton.UseVisualStyleBackColor = false;
            ReadmeEncodingButton.Visible = false;
            ReadmeEncodingButton.PaintCustom += ReadmeEncodingButton_Paint;
            ReadmeEncodingButton.Click += ReadmeEncodingButton_Click;
            ReadmeEncodingButton.MouseLeave += ReadmeArea_MouseLeave;
            // 
            // ReadmeFullScreenButton
            // 
            ReadmeFullScreenButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            ReadmeFullScreenButton.FlatAppearance.BorderSize = 0;
            ReadmeFullScreenButton.FlatStyle = FlatStyle.Flat;
            ReadmeFullScreenButton.Location = new Point(1616, 8);
            ReadmeFullScreenButton.Size = new Size(21, 21);
            ReadmeFullScreenButton.TabIndex = 6;
            ReadmeFullScreenButton.UseVisualStyleBackColor = false;
            ReadmeFullScreenButton.Visible = false;
            ReadmeFullScreenButton.PaintCustom += ReadmeFullScreenButton_Paint;
            ReadmeFullScreenButton.Click += ReadmeFullScreenButton_Click;
            ReadmeFullScreenButton.MouseLeave += ReadmeArea_MouseLeave;
            // 
            // ReadmeZoomInButton
            // 
            ReadmeZoomInButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            ReadmeZoomInButton.FlatAppearance.BorderSize = 0;
            ReadmeZoomInButton.FlatStyle = FlatStyle.Flat;
            ReadmeZoomInButton.Location = new Point(1534, 8);
            ReadmeZoomInButton.Size = new Size(21, 21);
            ReadmeZoomInButton.TabIndex = 3;
            ReadmeZoomInButton.UseVisualStyleBackColor = false;
            ReadmeZoomInButton.Visible = false;
            ReadmeZoomInButton.PaintCustom += ZoomInButtons_Paint;
            ReadmeZoomInButton.Click += ReadmeZoomInButton_Click;
            ReadmeZoomInButton.MouseLeave += ReadmeArea_MouseLeave;
            // 
            // ReadmeZoomOutButton
            // 
            ReadmeZoomOutButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            ReadmeZoomOutButton.BackColor = SystemColors.Window;
            ReadmeZoomOutButton.FlatAppearance.BorderSize = 0;
            ReadmeZoomOutButton.FlatStyle = FlatStyle.Flat;
            ReadmeZoomOutButton.Location = new Point(1559, 8);
            ReadmeZoomOutButton.Size = new Size(21, 21);
            ReadmeZoomOutButton.TabIndex = 4;
            ReadmeZoomOutButton.UseVisualStyleBackColor = false;
            ReadmeZoomOutButton.Visible = false;
            ReadmeZoomOutButton.PaintCustom += ZoomOutButtons_Paint;
            ReadmeZoomOutButton.Click += ReadmeZoomOutButton_Click;
            ReadmeZoomOutButton.MouseLeave += ReadmeArea_MouseLeave;
            // 
            // ReadmeResetZoomButton
            // 
            ReadmeResetZoomButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            ReadmeResetZoomButton.FlatAppearance.BorderSize = 0;
            ReadmeResetZoomButton.FlatStyle = FlatStyle.Flat;
            ReadmeResetZoomButton.Location = new Point(1584, 8);
            ReadmeResetZoomButton.Size = new Size(21, 21);
            ReadmeResetZoomButton.TabIndex = 5;
            ReadmeResetZoomButton.UseVisualStyleBackColor = false;
            ReadmeResetZoomButton.Visible = false;
            ReadmeResetZoomButton.PaintCustom += ZoomResetButtons_Paint;
            ReadmeResetZoomButton.Click += ReadmeResetZoomButton_Click;
            ReadmeResetZoomButton.MouseLeave += ReadmeArea_MouseLeave;
            // 
            // ChooseReadmeComboBox
            // 
            ChooseReadmeComboBox.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            ChooseReadmeComboBox.FireMouseLeaveOnLeaveWindow = true;
            ChooseReadmeComboBox.FormattingEnabled = true;
            ChooseReadmeComboBox.Location = new Point(1321, 8);
            ChooseReadmeComboBox.Size = new Size(170, 21);
            ChooseReadmeComboBox.TabIndex = 1;
            ChooseReadmeComboBox.Visible = false;
            ChooseReadmeComboBox.SelectedIndexChanged += ChooseReadmeComboBox_SelectedIndexChanged;
            ChooseReadmeComboBox.DropDownClosed += ChooseReadmeComboBox_DropDownClosed;
            ChooseReadmeComboBox.MouseLeave += ReadmeArea_MouseLeave;
            // 
            // ReadmeRichTextBox
            // 
            ReadmeRichTextBox.BackColor = SystemColors.Window;
            ReadmeRichTextBox.BorderStyle = BorderStyle.None;
            ReadmeRichTextBox.Dock = DockStyle.Fill;
            ReadmeRichTextBox.ReadOnly = true;
            ReadmeRichTextBox.TabIndex = 0;
            ReadmeRichTextBox.LinkClicked += ReadmeRichTextBox_LinkClicked;
            ReadmeRichTextBox.MouseLeave += ReadmeArea_MouseLeave;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(1671, 716);
            Controls.Add(EverythingPanel);
            DoubleBuffered = true;
            Icon = AL_Icon.AngelLoader;
            KeyPreview = true;
            MinimumSize = new Size(894, 260);
            Deactivate += MainForm_Deactivate;
            FormClosing += MainForm_FormClosing;
            Load += MainForm_Load;
            Shown += MainForm_Shown;
            LocationChanged += MainForm_LocationChanged;
            SizeChanged += MainForm_SizeChanged;
            KeyDown += MainForm_KeyDown;
            BottomPanel.ResumeLayout(false);
            BottomPanel.PerformLayout();
            BottomRightButtonsFLP.ResumeLayout(false);
            BottomRightButtonsFLP.PerformLayout();
            BottomLeftButtonsFLP.ResumeLayout(false);
            BottomLeftButtonsFLP.PerformLayout();
            EverythingPanel.ResumeLayout(false);
            MainSplitContainer.Panel1.ResumeLayout(false);
            MainSplitContainer.Panel2.ResumeLayout(false);
            ((ISupportInitialize)MainSplitContainer).EndInit();
            MainSplitContainer.ResumeLayout(false);
            TopSplitContainer.Panel1.ResumeLayout(false);
            TopSplitContainer.Panel1.PerformLayout();
            TopSplitContainer.Panel2.ResumeLayout(false);
            ((ISupportInitialize)TopSplitContainer).EndInit();
            TopSplitContainer.ResumeLayout(false);
            ((ISupportInitialize)FMsDGV).EndInit();
            FilterBarFLP.ResumeLayout(false);
            FilterBarFLP.PerformLayout();
            FilterGameButtonsToolStrip.ResumeLayout(false);
            FilterGameButtonsToolStrip.PerformLayout();
            GameFilterControlsShowHideButtonToolStrip.ResumeLayout(false);
            GameFilterControlsShowHideButtonToolStrip.PerformLayout();
            FilterIconButtonsToolStrip.ResumeLayout(false);
            FilterIconButtonsToolStrip.PerformLayout();
            RefreshAreaToolStrip.ResumeLayout(false);
            RefreshAreaToolStrip.PerformLayout();
            GamesTabControl.ResumeLayout(false);
            TopRightTabControl.ResumeLayout(false);
            StatisticsTabPage.ResumeLayout(false);
            StatisticsTabPage.PerformLayout();
            StatsCheckBoxesPanel.ResumeLayout(false);
            StatsCheckBoxesPanel.PerformLayout();
            EditFMTabPage.ResumeLayout(false);
            EditFMTabPage.PerformLayout();
            CommentTabPage.ResumeLayout(false);
            CommentTabPage.PerformLayout();
            TagsTabPage.ResumeLayout(false);
            TagsTabPage.PerformLayout();
            AddRemoveTagFLP.ResumeLayout(false);
            AddRemoveTagFLP.PerformLayout();
            PatchTabPage.ResumeLayout(false);
            PatchTabPage.PerformLayout();
            PatchMainPanel.ResumeLayout(false);
            PatchMainPanel.PerformLayout();
            PatchDMLsPanel.ResumeLayout(false);
            PatchDMLsPanel.PerformLayout();
            ModsTabPage.ResumeLayout(false);
            ModsTabPage.PerformLayout();
            ResumeLayout(false);
        }
    }
}
