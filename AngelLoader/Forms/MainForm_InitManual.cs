using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using AngelLoader.Forms.CustomControls;
using AngelLoader.Properties;

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

namespace AngelLoader.Forms;

public sealed partial class MainForm
{
    private void InitComponentManual()
    {
        components = new Container();
        GameTabsImageList = new ImageList(components);
        BottomPanel = new Panel();
        BottomRightFLP = new FlowLayoutPanel();
        FMCountLabel = new DarkLabel();
        SettingsButton = new DarkButton();
        BottomLeftFLP = new FlowLayoutPanel();
        PlayFMButton = new DarkButton();
        PlayOriginalFLP = new FlowLayoutPanel();
        EverythingPanel = new Panel();
        MainSplitContainer = new DarkSplitContainerCustom();
        TopSplitContainer = new DarkSplitContainerCustom();
        MainMenuButton = new DarkButton();
        FilterBarScrollRightButton = new DarkArrowButton();
        FilterBarScrollLeftButton = new DarkArrowButton();
        FMsDGV = new DataGridViewCustom();
        GameTypeColumn = new DataGridViewImageColumn();
        InstalledColumn = new DataGridViewImageColumn();
        MisCountColumn = new DataGridViewTextBoxColumn();
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
        TopRightMenuButton = new DarkButton();
        TopRightCollapseButton = new DarkArrowButton();
        TopRightTabControl = new DarkTabControl();
        StatisticsTabPage = new StatsTabPage();
        EditFMTabPage = new EditFMTabPage();
        CommentTabPage = new CommentTabPage();
        TagsTabPage = new TagsTabPage();
        PatchTabPage = new PatchTabPage();
        ModsTabPage = new ModsTabPage();
        ReadmeEncodingButton = new DarkButton();
        ReadmeFullScreenButton = new DarkButton();
        ReadmeZoomInButton = new DarkButton();
        ReadmeZoomOutButton = new DarkButton();
        ReadmeResetZoomButton = new DarkButton();
        ChooseReadmeComboBox = new DarkComboBoxWithBackingItems();
        ReadmeRichTextBox = new RichTextBoxCustom();
        MainToolTip = new ToolTip(components);
        BottomPanel.SuspendLayout();
        BottomRightFLP.SuspendLayout();
        BottomLeftFLP.SuspendLayout();
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
        GameFilterControlsShowHideButtonToolStrip.SuspendLayout();
        FilterIconButtonsToolStrip.SuspendLayout();
        RefreshAreaToolStrip.SuspendLayout();
        TopRightTabControl.SuspendLayout();
        StatisticsTabPage.SuspendLayout();
        EditFMTabPage.SuspendLayout();
        CommentTabPage.SuspendLayout();
        TagsTabPage.SuspendLayout();
        PatchTabPage.SuspendLayout();
        ModsTabPage.SuspendLayout();
        SuspendLayout();
        // 
        // GameTabsImageList
        // 
        GameTabsImageList.ColorDepth = ColorDepth.Depth32Bit;
        // 
        // BottomPanel
        // 
        BottomPanel.Controls.Add(BottomRightFLP);
        BottomPanel.Controls.Add(BottomLeftFLP);
        BottomPanel.Dock = DockStyle.Bottom;
        BottomPanel.Size = new Size(1671, 44);
        BottomPanel.TabIndex = 1;
        // 
        // BottomRightFLP
        // 
        BottomRightFLP.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        BottomRightFLP.AutoSize = true;
        BottomRightFLP.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        BottomRightFLP.Controls.Add(SettingsButton);
        BottomRightFLP.Controls.Add(FMCountLabel);
        BottomRightFLP.FlowDirection = FlowDirection.RightToLeft;
        BottomRightFLP.Location = new Point(1490, 0);
        // Needs width to be anchored correctly
        BottomRightFLP.Width = 179;
        BottomRightFLP.TabIndex = 37;
        BottomRightFLP.Paint += BottomRightFLP_Paint;
        // 
        // SettingsButton
        // 
        SettingsButton.AutoSize = true;
        SettingsButton.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        SettingsButton.Margin = new Padding(15, 3, 3, 3);
        SettingsButton.MinimumSize = new Size(0, 36);
        SettingsButton.Padding = new Padding(30, 0, 6, 0);
        SettingsButton.TabIndex = 62;
        SettingsButton.UseVisualStyleBackColor = true;
        SettingsButton.PaintCustom += SettingsButton_Paint;
        SettingsButton.Click += Async_EventHandler_Main;
        // 
        // FMCountLabel
        // 
        FMCountLabel.AutoSize = true;
        FMCountLabel.Margin = new Padding(3, 1, 0, 0);
        // 
        // BottomLeftFLP
        // 
        BottomLeftFLP.AutoSize = true;
        BottomLeftFLP.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        BottomLeftFLP.Controls.Add(PlayFMButton);
        BottomLeftFLP.Controls.Add(PlayOriginalFLP);
        BottomLeftFLP.Location = new Point(2, 0);
        BottomLeftFLP.TabIndex = 36;
        BottomLeftFLP.Paint += BottomLeftFLP_Paint;
        // 
        // PlayFMButton
        // 
        PlayFMButton.AutoSize = true;
        PlayFMButton.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        PlayFMButton.Margin = new Padding(3, 3, 0, 3);
        PlayFMButton.MinimumSize = new Size(91, 36);
        PlayFMButton.Padding = new Padding(28, 0, 6, 0);
        PlayFMButton.TabIndex = 56;
        PlayFMButton.UseVisualStyleBackColor = true;
        PlayFMButton.PaintCustom += PlayFMButton_Paint;
        PlayFMButton.Click += Async_EventHandler_Main;
        // 
        // PlayOriginalFLP
        // 
        PlayOriginalFLP.AutoSize = true;
        PlayOriginalFLP.Margin = new Padding(6, 3, 0, 3);
        PlayOriginalFLP.TabIndex = 58;
        // 
        // EverythingPanel
        // 
        EverythingPanel.AllowDrop = true;
        EverythingPanel.Controls.Add(MainSplitContainer);
        EverythingPanel.Controls.Add(BottomPanel);
        EverythingPanel.Dock = DockStyle.Fill;
        EverythingPanel.Size = new Size(1671, 716);
        EverythingPanel.TabIndex = 4;
        EverythingPanel.DragDrop += EverythingPanel_DragDrop;
        EverythingPanel.DragEnter += EverythingPanel_DragEnter;
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
        MainSplitContainer.RefreshSiblingFirst = true;
        MainSplitContainer.Size = new Size(1671, 672);
        MainSplitContainer.SplitterDistance = 309;
        MainSplitContainer.TabIndex = 0;
        MainSplitContainer.FullScreenChanged += MainSplitContainer_FullScreenChanged;
        // 
        // TopSplitContainer
        // 
        TopSplitContainer.BackColor = SystemColors.ActiveBorder;
        TopSplitContainer.Dock = DockStyle.Fill;
        TopSplitContainer.FullScreenCollapsePanel = DarkSplitContainerCustom.Panel.Panel2;
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
        TopSplitContainer.FullScreenChanged += TopSplitContainer_FullScreenChanged;
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
            MisCountColumn,
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
        FMsDGV.MultiSelect = true;
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
        // MisCountColumn
        // 
        MisCountColumn.MinimumWidth = 25;
        MisCountColumn.ReadOnly = true;
        MisCountColumn.SortMode = DataGridViewColumnSortMode.Programmatic;
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
        FilterGameButtonsToolStrip.LayoutStyle = ToolStripLayoutStyle.Flow;
        FilterGameButtonsToolStrip.TabIndex = 3;
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
        // 
        // FilterTitleTextBox
        // 
        FilterTitleTextBox.Size = new Size(144, 20);
        FilterTitleTextBox.TabIndex = 6;
        FilterTitleTextBox.TextChanged += Async_EventHandler_Main;
        // 
        // FilterAuthorLabel
        // 
        FilterAuthorLabel.AutoSize = true;
        FilterAuthorLabel.Margin = new Padding(9, 6, 0, 0);
        // 
        // FilterAuthorTextBox
        // 
        FilterAuthorTextBox.Size = new Size(144, 20);
        FilterAuthorTextBox.TabIndex = 8;
        FilterAuthorTextBox.TextChanged += Async_EventHandler_Main;
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
        FilterByFinishedButton.Click += Async_EventHandler_Main;
        // 
        // FilterByUnfinishedButton
        // 
        FilterByUnfinishedButton.AutoSize = false;
        FilterByUnfinishedButton.CheckOnClick = true;
        FilterByUnfinishedButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
        FilterByUnfinishedButton.Margin = new Padding(0);
        FilterByUnfinishedButton.Size = new Size(25, 25);
        FilterByUnfinishedButton.Click += Async_EventHandler_Main;
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
        FilterShowUnsupportedButton.Image = Resources.ShowUnsupported;
        FilterShowUnsupportedButton.Margin = new Padding(6, 0, 0, 0);
        FilterShowUnsupportedButton.Size = new Size(25, 25);
        FilterShowUnsupportedButton.Click += Async_EventHandler_Main;
        // 
        // FilterShowUnavailableButton
        // 
        FilterShowUnavailableButton.AutoSize = false;
        FilterShowUnavailableButton.CheckOnClick = true;
        FilterShowUnavailableButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
        FilterShowUnavailableButton.Image = Resources.ShowUnavailable;
        FilterShowUnavailableButton.Margin = new Padding(0);
        FilterShowUnavailableButton.Size = new Size(25, 25);
        FilterShowUnavailableButton.Click += Async_EventHandler_Main;
        // 
        // FilterShowRecentAtTopButton
        // 
        FilterShowRecentAtTopButton.AutoSize = false;
        FilterShowRecentAtTopButton.CheckOnClick = true;
        FilterShowRecentAtTopButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
        FilterShowRecentAtTopButton.Margin = new Padding(6, 0, 2, 0);
        FilterShowRecentAtTopButton.Size = new Size(25, 25);
        FilterShowRecentAtTopButton.Click += Async_EventHandler_Main;
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
        RefreshFromDiskButton.Click += Async_EventHandler_Main;
        RefreshFromDiskButton.Margin = new Padding(6, 0, 0, 0);
        // 
        // RefreshFiltersButton
        // 
        RefreshFiltersButton.AutoSize = false;
        RefreshFiltersButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
        RefreshFiltersButton.Margin = new Padding(0);
        RefreshFiltersButton.Size = new Size(25, 25);
        RefreshFiltersButton.Click += Async_EventHandler_Main;
        // 
        // ClearFiltersButton
        // 
        ClearFiltersButton.AutoSize = false;
        ClearFiltersButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
        ClearFiltersButton.Margin = new Padding(0, 0, 9, 1);
        ClearFiltersButton.Size = new Size(25, 25);
        ClearFiltersButton.Click += Async_EventHandler_Main;
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
        GamesTabControl.ImageList = GameTabsImageList;
        GamesTabControl.Location = new Point(28, 5);
        GamesTabControl.Size = new Size(1075, 24);
        GamesTabControl.TabIndex = 1;
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
        StatisticsTabPage.BackColor = SystemColors.Control;
        // 
        // EditFMTabPage
        // 
        EditFMTabPage.BackColor = SystemColors.Control;
        // 
        // CommentTabPage
        // 
        CommentTabPage.BackColor = SystemColors.Control;
        // 
        // TagsTabPage
        // 
        TagsTabPage.BackColor = SystemColors.Control;
        // 
        // PatchTabPage
        // 
        PatchTabPage.BackColor = SystemColors.Control;
        // 
        // ModsTabPage
        // 
        ModsTabPage.BackColor = SystemColors.Control;
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
        ReadmeRichTextBox.KeyDown += ReadmeRichTextBox_KeyDown;
        ReadmeRichTextBox.MouseDown += ReadmeRichTextBox_MouseDown;
        ReadmeRichTextBox.MouseLeave += ReadmeArea_MouseLeave;
        // 
        // MainForm
        // 
        AutoScaleDimensions = new SizeF(96F, 96F);
        AutoScaleMode = AutoScaleMode.Dpi;
        ClientSize = new Size(1671, 716);
        Controls.Add(EverythingPanel);
        DoubleBuffered = true;
        KeyPreview = true;
        MinimumSize = new Size(894, 260);
        ShowInTaskbar = true;
        KeyDown += MainForm_KeyDown;
        BottomPanel.ResumeLayout(false);
        BottomPanel.PerformLayout();
        BottomRightFLP.ResumeLayout(false);
        BottomRightFLP.PerformLayout();
        BottomLeftFLP.ResumeLayout(false);
        BottomLeftFLP.PerformLayout();
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
        GameFilterControlsShowHideButtonToolStrip.ResumeLayout(false);
        GameFilterControlsShowHideButtonToolStrip.PerformLayout();
        FilterIconButtonsToolStrip.ResumeLayout(false);
        FilterIconButtonsToolStrip.PerformLayout();
        RefreshAreaToolStrip.ResumeLayout(false);
        RefreshAreaToolStrip.PerformLayout();
        TopRightTabControl.ResumeLayout(false);
        StatisticsTabPage.ResumeLayout(false);
        StatisticsTabPage.PerformLayout();
        EditFMTabPage.ResumeLayout(false);
        EditFMTabPage.PerformLayout();
        CommentTabPage.ResumeLayout(false);
        CommentTabPage.PerformLayout();
        TagsTabPage.ResumeLayout(false);
        TagsTabPage.PerformLayout();
        PatchTabPage.ResumeLayout(false);
        PatchTabPage.PerformLayout();
        ModsTabPage.ResumeLayout(false);
        ModsTabPage.PerformLayout();
        ResumeLayout(false);
    }
}
