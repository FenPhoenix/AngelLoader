namespace AngelLoader.Forms;

/*
// 
// GameTabsImageList
// 
this.GameTabsImageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("GameTabsImageList.ImageStream")));
this.GameTabsImageList.TransparentColor = System.Drawing.Color.Transparent;
// IMPORTANT: WinForms does the above by default, but that results in images being 8-bit-looking, even
// though the ImageStream is set to 32-bit and so are all the images within. But we need to keep it
// there for the designer to work at all, so just remember to test in non-debug mode so you can see
// the images properly.
*/

sealed partial class MainForm
{
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

#if DEBUG
    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        this.components = new System.ComponentModel.Container();
        System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
        System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
        System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
        System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
        System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
        this.GameTabsImageList = new System.Windows.Forms.ImageList(this.components);
        this.BottomRightFLP = new System.Windows.Forms.FlowLayoutPanel();
        this.SettingsButton = new AngelLoader.Forms.CustomControls.DarkButton();
        this.FMCountLabel = new AngelLoader.Forms.CustomControls.DarkLabel();
        this.BottomLeftFLP = new System.Windows.Forms.FlowLayoutPanel();
        this.PlayFMButton = new AngelLoader.Forms.CustomControls.DarkButton();
        this.PlayOriginalFLP = new System.Windows.Forms.FlowLayoutPanel();
        this.EverythingPanel = new System.Windows.Forms.Panel();
        this.MainSplitContainer = new AngelLoader.Forms.CustomControls.DarkSplitContainerCustom();
        this.TopSplitContainer = new AngelLoader.Forms.CustomControls.DarkSplitContainerCustom();
        this.MainMenuButton = new AngelLoader.Forms.CustomControls.DarkButton();
        this.FilterBarScrollRightButton = new AngelLoader.Forms.CustomControls.DarkArrowButton();
        this.FilterBarScrollLeftButton = new AngelLoader.Forms.CustomControls.DarkArrowButton();
        this.FMsDGV = new AngelLoader.Forms.CustomControls.DataGridViewCustom();
        this.GameTypeColumn = new System.Windows.Forms.DataGridViewImageColumn();
        this.InstalledColumn = new System.Windows.Forms.DataGridViewImageColumn();
        this.MisCountColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
        this.TitleColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
        this.ArchiveColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
        this.AuthorColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
        this.SizeColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
        this.RatingTextColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
        this.FinishedColumn = new System.Windows.Forms.DataGridViewImageColumn();
        this.ReleaseDateColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
        this.LastPlayedColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
        this.DateAddedColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
        this.DisabledModsColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
        this.CommentColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
        this.FilterBarFLP = new System.Windows.Forms.FlowLayoutPanel();
        this.FilterGameButtonsToolStrip = new AngelLoader.Forms.CustomControls.ToolStripCustom();
        this.GameFilterControlsShowHideButtonToolStrip = new AngelLoader.Forms.CustomControls.ToolStripCustom();
        this.GameFilterControlsShowHideButton = new AngelLoader.Forms.CustomControls.ToolStripArrowButton();
        this.FilterTitleLabel = new AngelLoader.Forms.CustomControls.DarkLabel();
        this.FilterTitleTextBox = new AngelLoader.Forms.CustomControls.DarkTextBoxCustom();
        this.FilterAuthorLabel = new AngelLoader.Forms.CustomControls.DarkLabel();
        this.FilterAuthorTextBox = new AngelLoader.Forms.CustomControls.DarkTextBoxCustom();
        this.FilterIconButtonsToolStrip = new AngelLoader.Forms.CustomControls.ToolStripCustom();
        this.FilterByReleaseDateButton = new AngelLoader.Forms.CustomControls.ToolStripButtonCustom();
        this.FilterByLastPlayedButton = new AngelLoader.Forms.CustomControls.ToolStripButtonCustom();
        this.FilterByTagsButton = new AngelLoader.Forms.CustomControls.ToolStripButtonCustom();
        this.FilterByFinishedButton = new AngelLoader.Forms.CustomControls.ToolStripButtonCustom();
        this.FilterByUnfinishedButton = new AngelLoader.Forms.CustomControls.ToolStripButtonCustom();
        this.FilterByRatingButton = new AngelLoader.Forms.CustomControls.ToolStripButtonCustom();
        this.FilterShowUnsupportedButton = new AngelLoader.Forms.CustomControls.ToolStripButtonCustom();
        this.FilterShowUnavailableButton = new AngelLoader.Forms.CustomControls.ToolStripButtonCustom();
        this.FilterShowRecentAtTopButton = new AngelLoader.Forms.CustomControls.ToolStripButtonCustom();
        this.FilterControlsShowHideButton = new AngelLoader.Forms.CustomControls.ToolStripArrowButton();
        this.RefreshAreaToolStrip = new AngelLoader.Forms.CustomControls.ToolStripCustom();
        this.RefreshFromDiskButton = new AngelLoader.Forms.CustomControls.ToolStripButtonCustom();
        this.RefreshFiltersButton = new AngelLoader.Forms.CustomControls.ToolStripButtonCustom();
        this.ClearFiltersButton = new AngelLoader.Forms.CustomControls.ToolStripButtonCustom();
        this.ResetLayoutButton = new AngelLoader.Forms.CustomControls.DarkButton();
        this.GamesTabControl = new AngelLoader.Forms.CustomControls.DarkTabControl();
        this.TopFMTabsMenuButton = new AngelLoader.Forms.CustomControls.DarkButton();
        this.TopFMTabsCollapseButton = new AngelLoader.Forms.CustomControls.DarkArrowButton();
        this.TopFMTabControl = new AngelLoader.Forms.CustomControls.DarkTabControl();
        this.StatisticsTabPage = new AngelLoader.Forms.CustomControls.StatsTabPage();
        this.EditFMTabPage = new AngelLoader.Forms.CustomControls.EditFMTabPage();
        this.CommentTabPage = new AngelLoader.Forms.CustomControls.CommentTabPage();
        this.TagsTabPage = new AngelLoader.Forms.CustomControls.TagsTabPage();
        this.PatchTabPage = new AngelLoader.Forms.CustomControls.PatchTabPage();
        this.ModsTabPage = new AngelLoader.Forms.CustomControls.ModsTabPage();
        this.ScreenshotsTabPage = new AngelLoader.Forms.CustomControls.ScreenshotsTabPage();
        this.LowerSplitContainer = new AngelLoader.Forms.CustomControls.DarkSplitContainerCustom();
        this.ReadmeEncodingButton = new AngelLoader.Forms.CustomControls.DarkButton();
        this.ReadmeFullScreenButton = new AngelLoader.Forms.CustomControls.DarkButton();
        this.ReadmeZoomInButton = new AngelLoader.Forms.CustomControls.DarkButton();
        this.ReadmeZoomOutButton = new AngelLoader.Forms.CustomControls.DarkButton();
        this.ReadmeResetZoomButton = new AngelLoader.Forms.CustomControls.DarkButton();
        this.ChooseReadmeComboBox = new AngelLoader.Forms.CustomControls.DarkComboBoxWithBackingItems();
        this.ReadmeRichTextBox = new AngelLoader.Forms.CustomControls.RichTextBoxCustom();
        this.MainToolTip = new AngelLoader.Forms.CustomControls.ToolTipCustom(this.components);
        this.BottomRightFLP.SuspendLayout();
        this.BottomLeftFLP.SuspendLayout();
        this.EverythingPanel.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.MainSplitContainer)).BeginInit();
        this.MainSplitContainer.Panel1.SuspendLayout();
        this.MainSplitContainer.Panel2.SuspendLayout();
        this.MainSplitContainer.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.TopSplitContainer)).BeginInit();
        this.TopSplitContainer.Panel1.SuspendLayout();
        this.TopSplitContainer.Panel2.SuspendLayout();
        this.TopSplitContainer.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.FMsDGV)).BeginInit();
        this.FilterBarFLP.SuspendLayout();
        this.GameFilterControlsShowHideButtonToolStrip.SuspendLayout();
        this.FilterIconButtonsToolStrip.SuspendLayout();
        this.RefreshAreaToolStrip.SuspendLayout();
        this.TopFMTabControl.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.LowerSplitContainer)).BeginInit();
        this.LowerSplitContainer.Panel1.SuspendLayout();
        this.LowerSplitContainer.SuspendLayout();
        this.SuspendLayout();
        // 
        // GameTabsImageList
        // 
        this.GameTabsImageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("GameTabsImageList.ImageStream")));
        this.GameTabsImageList.TransparentColor = System.Drawing.Color.Transparent;
        this.GameTabsImageList.Images.SetKeyName(0, "Thief1_16.png");
        this.GameTabsImageList.Images.SetKeyName(1, "Thief2_16.png");
        this.GameTabsImageList.Images.SetKeyName(2, "Thief3_16.png");
        this.GameTabsImageList.Images.SetKeyName(3, "Shock2_16.png");
        // 
        // BottomRightFLP
        // 
        this.BottomRightFLP.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
        this.BottomRightFLP.AutoSize = true;
        this.BottomRightFLP.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        this.BottomRightFLP.Controls.Add(this.SettingsButton);
        this.BottomRightFLP.Controls.Add(this.FMCountLabel);
        this.BottomRightFLP.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
        this.BottomRightFLP.Location = new System.Drawing.Point(1490, 672);
        this.BottomRightFLP.Name = "BottomRightFLP";
        this.BottomRightFLP.Size = new System.Drawing.Size(179, 42);
        this.BottomRightFLP.TabIndex = 37;
        this.BottomRightFLP.Paint += new System.Windows.Forms.PaintEventHandler(this.BottomRightFLP_Paint);
        // 
        // SettingsButton
        // 
        this.SettingsButton.AutoSize = true;
        this.SettingsButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        this.SettingsButton.Location = new System.Drawing.Point(76, 3);
        this.SettingsButton.Margin = new System.Windows.Forms.Padding(15, 3, 3, 3);
        this.SettingsButton.MinimumSize = new System.Drawing.Size(0, 36);
        this.SettingsButton.Name = "SettingsButton";
        this.SettingsButton.Padding = new System.Windows.Forms.Padding(30, 0, 6, 0);
        this.SettingsButton.Size = new System.Drawing.Size(100, 36);
        this.SettingsButton.TabIndex = 2;
        this.SettingsButton.Text = "Settings...";
        this.SettingsButton.PaintCustom += new System.EventHandler<System.Windows.Forms.PaintEventArgs>(this.SettingsButton_Paint);
        this.SettingsButton.Click += new System.EventHandler(this.Async_EventHandler_Main);
        // 
        // FMCountLabel
        // 
        this.FMCountLabel.AutoSize = true;
        this.FMCountLabel.Location = new System.Drawing.Point(3, 1);
        this.FMCountLabel.Margin = new System.Windows.Forms.Padding(3, 1, 0, 0);
        this.FMCountLabel.Name = "FMCountLabel";
        this.FMCountLabel.Size = new System.Drawing.Size(58, 13);
        this.FMCountLabel.TabIndex = 1;
        this.FMCountLabel.Text = "[FM count]";
        // 
        // BottomLeftFLP
        // 
        this.BottomLeftFLP.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
        this.BottomLeftFLP.AutoSize = true;
        this.BottomLeftFLP.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        this.BottomLeftFLP.Controls.Add(this.PlayFMButton);
        this.BottomLeftFLP.Controls.Add(this.PlayOriginalFLP);
        this.BottomLeftFLP.Location = new System.Drawing.Point(2, 672);
        this.BottomLeftFLP.Name = "BottomLeftFLP";
        this.BottomLeftFLP.Size = new System.Drawing.Size(100, 42);
        this.BottomLeftFLP.TabIndex = 36;
        this.BottomLeftFLP.Paint += new System.Windows.Forms.PaintEventHandler(this.BottomLeftFLP_Paint);
        // 
        // PlayFMButton
        // 
        this.PlayFMButton.AutoSize = true;
        this.PlayFMButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        this.PlayFMButton.Location = new System.Drawing.Point(3, 3);
        this.PlayFMButton.Margin = new System.Windows.Forms.Padding(3, 3, 0, 3);
        this.PlayFMButton.MinimumSize = new System.Drawing.Size(91, 36);
        this.PlayFMButton.Name = "PlayFMButton";
        this.PlayFMButton.Padding = new System.Windows.Forms.Padding(28, 0, 6, 0);
        this.PlayFMButton.Size = new System.Drawing.Size(91, 36);
        this.PlayFMButton.TabIndex = 56;
        this.PlayFMButton.Text = "Play FM";
        this.PlayFMButton.PaintCustom += new System.EventHandler<System.Windows.Forms.PaintEventArgs>(this.PlayFMButton_Paint);
        this.PlayFMButton.Click += new System.EventHandler(this.Async_EventHandler_Main);
        // 
        // PlayOriginalFLP
        // 
        this.PlayOriginalFLP.AutoSize = true;
        this.PlayOriginalFLP.Location = new System.Drawing.Point(100, 3);
        this.PlayOriginalFLP.Margin = new System.Windows.Forms.Padding(6, 3, 0, 3);
        this.PlayOriginalFLP.Name = "PlayOriginalFLP";
        this.PlayOriginalFLP.Size = new System.Drawing.Size(0, 0);
        this.PlayOriginalFLP.TabIndex = 57;
        // 
        // EverythingPanel
        // 
        this.EverythingPanel.AllowDrop = true;
        this.EverythingPanel.Controls.Add(this.MainSplitContainer);
        this.EverythingPanel.Controls.Add(this.BottomRightFLP);
        this.EverythingPanel.Controls.Add(this.BottomLeftFLP);
        this.EverythingPanel.Dock = System.Windows.Forms.DockStyle.Fill;
        this.EverythingPanel.Location = new System.Drawing.Point(0, 0);
        this.EverythingPanel.Name = "EverythingPanel";
        this.EverythingPanel.Size = new System.Drawing.Size(1671, 716);
        this.EverythingPanel.TabIndex = 4;
        this.EverythingPanel.DragDrop += new System.Windows.Forms.DragEventHandler(this.EverythingPanel_DragDrop);
        this.EverythingPanel.DragEnter += new System.Windows.Forms.DragEventHandler(this.EverythingPanel_DragEnter);
        // 
        // MainSplitContainer
        // 
        this.MainSplitContainer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
        | System.Windows.Forms.AnchorStyles.Left)
        | System.Windows.Forms.AnchorStyles.Right)));
        this.MainSplitContainer.BackColor = System.Drawing.SystemColors.ActiveBorder;
        this.MainSplitContainer.Location = new System.Drawing.Point(0, 0);
        this.MainSplitContainer.Name = "MainSplitContainer";
        this.MainSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
        // 
        // MainSplitContainer.Panel1
        // 
        this.MainSplitContainer.Panel1.BackColor = System.Drawing.SystemColors.Control;
        this.MainSplitContainer.Panel1.Controls.Add(this.TopSplitContainer);
        this.MainSplitContainer.Panel1MinSize = 100;
        // 
        // MainSplitContainer.Panel2
        // 
        this.MainSplitContainer.Panel2.BackColor = System.Drawing.SystemColors.Control;
        this.MainSplitContainer.Panel2.Controls.Add(this.LowerSplitContainer);
        this.MainSplitContainer.Panel2.Padding = new System.Windows.Forms.Padding(1, 1, 2, 2);
        this.MainSplitContainer.Panel2.Paint += new System.Windows.Forms.PaintEventHandler(this.ReadmeContainer_Paint);
        this.MainSplitContainer.Panel2.MouseLeave += new System.EventHandler(this.ReadmeArea_MouseLeave);
        this.MainSplitContainer.Panel2MinSize = 38;
        this.MainSplitContainer.RefreshSiblingFirst = true;
        this.MainSplitContainer.Size = new System.Drawing.Size(1671, 672);
        this.MainSplitContainer.SplitterDistance = 309;
        this.MainSplitContainer.TabIndex = 0;
        this.MainSplitContainer.FullScreenBeforeChanged += new System.EventHandler(this.MainSplitContainer_FullScreenBeforeChanged);
        this.MainSplitContainer.FullScreenChanged += new System.EventHandler(this.MainSplitContainer_FullScreenChanged);
        // 
        // TopSplitContainer
        // 
        this.TopSplitContainer.BackColor = System.Drawing.SystemColors.ActiveBorder;
        this.TopSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
        this.TopSplitContainer.FullScreenCollapsePanel = AngelLoader.Forms.CustomControls.DarkSplitContainerCustom.Panel.Panel2;
        this.TopSplitContainer.Location = new System.Drawing.Point(0, 0);
        this.TopSplitContainer.Name = "TopSplitContainer";
        // 
        // TopSplitContainer.Panel1
        // 
        this.TopSplitContainer.Panel1.BackColor = System.Drawing.SystemColors.Control;
        this.TopSplitContainer.Panel1.Controls.Add(this.MainMenuButton);
        this.TopSplitContainer.Panel1.Controls.Add(this.FilterBarScrollRightButton);
        this.TopSplitContainer.Panel1.Controls.Add(this.FilterBarScrollLeftButton);
        this.TopSplitContainer.Panel1.Controls.Add(this.FMsDGV);
        this.TopSplitContainer.Panel1.Controls.Add(this.FilterBarFLP);
        this.TopSplitContainer.Panel1.Controls.Add(this.RefreshAreaToolStrip);
        this.TopSplitContainer.Panel1.Controls.Add(this.ResetLayoutButton);
        this.TopSplitContainer.Panel1.Controls.Add(this.GamesTabControl);
        // 
        // TopSplitContainer.Panel2
        // 
        this.TopSplitContainer.Panel2.BackColor = System.Drawing.SystemColors.Control;
        this.TopSplitContainer.Panel2.Controls.Add(this.TopFMTabsMenuButton);
        this.TopSplitContainer.Panel2.Controls.Add(this.TopFMTabsCollapseButton);
        this.TopSplitContainer.Panel2.Controls.Add(this.TopFMTabControl);
        this.TopSplitContainer.Size = new System.Drawing.Size(1671, 309);
        this.TopSplitContainer.SplitterDistance = 1116;
        this.TopSplitContainer.TabIndex = 0;
        this.TopSplitContainer.FullScreenChanged += new System.EventHandler(this.TopSplitContainer_FullScreenChanged);
        // 
        // MainMenuButton
        // 
        this.MainMenuButton.FlatAppearance.BorderSize = 0;
        this.MainMenuButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.MainMenuButton.Location = new System.Drawing.Point(0, 0);
        this.MainMenuButton.Name = "MainMenuButton";
        this.MainMenuButton.Size = new System.Drawing.Size(24, 24);
        this.MainMenuButton.TabIndex = 14;
        this.MainMenuButton.PaintCustom += new System.EventHandler<System.Windows.Forms.PaintEventArgs>(this.MainMenuButton_Paint);
        this.MainMenuButton.Click += new System.EventHandler(this.MainMenuButton_Click);
        this.MainMenuButton.Enter += new System.EventHandler(this.MainMenuButton_Enter);
        // 
        // FilterBarScrollRightButton
        // 
        this.FilterBarScrollRightButton.ArrowDirection = AngelLoader.Forms.Direction.Right;
        this.FilterBarScrollRightButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.FilterBarScrollRightButton.Location = new System.Drawing.Point(1088, 56);
        this.FilterBarScrollRightButton.Name = "FilterBarScrollRightButton";
        this.FilterBarScrollRightButton.Size = new System.Drawing.Size(14, 24);
        this.FilterBarScrollRightButton.TabIndex = 10;
        this.FilterBarScrollRightButton.Visible = false;
        this.FilterBarScrollRightButton.EnabledChanged += new System.EventHandler(this.FilterBarScrollButtons_EnabledChanged);
        this.FilterBarScrollRightButton.VisibleChanged += new System.EventHandler(this.FilterBarScrollButtons_VisibleChanged);
        this.FilterBarScrollRightButton.Click += new System.EventHandler(this.FilterBarScrollButtons_Click);
        this.FilterBarScrollRightButton.MouseDown += new System.Windows.Forms.MouseEventHandler(this.FilterBarScrollButtons_MouseDown);
        this.FilterBarScrollRightButton.MouseUp += new System.Windows.Forms.MouseEventHandler(this.FilterBarScrollButtons_MouseUp);
        // 
        // FilterBarScrollLeftButton
        // 
        this.FilterBarScrollLeftButton.ArrowDirection = AngelLoader.Forms.Direction.Left;
        this.FilterBarScrollLeftButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.FilterBarScrollLeftButton.Location = new System.Drawing.Point(1072, 56);
        this.FilterBarScrollLeftButton.Name = "FilterBarScrollLeftButton";
        this.FilterBarScrollLeftButton.Size = new System.Drawing.Size(14, 24);
        this.FilterBarScrollLeftButton.TabIndex = 2;
        this.FilterBarScrollLeftButton.Visible = false;
        this.FilterBarScrollLeftButton.EnabledChanged += new System.EventHandler(this.FilterBarScrollButtons_EnabledChanged);
        this.FilterBarScrollLeftButton.VisibleChanged += new System.EventHandler(this.FilterBarScrollButtons_VisibleChanged);
        this.FilterBarScrollLeftButton.Click += new System.EventHandler(this.FilterBarScrollButtons_Click);
        this.FilterBarScrollLeftButton.MouseDown += new System.Windows.Forms.MouseEventHandler(this.FilterBarScrollButtons_MouseDown);
        this.FilterBarScrollLeftButton.MouseUp += new System.Windows.Forms.MouseEventHandler(this.FilterBarScrollButtons_MouseUp);
        // 
        // FMsDGV
        // 
        this.FMsDGV.AllowUserToAddRows = false;
        this.FMsDGV.AllowUserToDeleteRows = false;
        this.FMsDGV.AllowUserToOrderColumns = true;
        this.FMsDGV.AllowUserToResizeRows = false;
        this.FMsDGV.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
        | System.Windows.Forms.AnchorStyles.Left)
        | System.Windows.Forms.AnchorStyles.Right)));
        this.FMsDGV.BackgroundColor = System.Drawing.SystemColors.ControlDark;
        dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
        dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
        dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
        dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
        dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
        dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
        this.FMsDGV.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
        this.FMsDGV.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        this.FMsDGV.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.GameTypeColumn,
            this.InstalledColumn,
            this.MisCountColumn,
            this.TitleColumn,
            this.ArchiveColumn,
            this.AuthorColumn,
            this.SizeColumn,
            this.RatingTextColumn,
            this.FinishedColumn,
            this.ReleaseDateColumn,
            this.LastPlayedColumn,
            this.DateAddedColumn,
            this.DisabledModsColumn,
            this.CommentColumn});
        dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
        dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Window;
        dataGridViewCellStyle3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.ControlText;
        dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
        dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
        dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
        this.FMsDGV.DefaultCellStyle = dataGridViewCellStyle3;
        this.FMsDGV.Location = new System.Drawing.Point(1, 26);
        this.FMsDGV.Name = "FMsDGV";
        this.FMsDGV.ReadOnly = true;
        dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
        dataGridViewCellStyle4.BackColor = System.Drawing.SystemColors.Control;
        dataGridViewCellStyle4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        dataGridViewCellStyle4.ForeColor = System.Drawing.SystemColors.WindowText;
        dataGridViewCellStyle4.SelectionBackColor = System.Drawing.SystemColors.Highlight;
        dataGridViewCellStyle4.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
        dataGridViewCellStyle4.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
        this.FMsDGV.RowHeadersDefaultCellStyle = dataGridViewCellStyle4;
        this.FMsDGV.RowHeadersVisible = false;
        this.FMsDGV.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
        this.FMsDGV.Size = new System.Drawing.Size(1109, 282);
        this.FMsDGV.StandardTab = true;
        this.FMsDGV.TabIndex = 0;
        this.FMsDGV.VirtualMode = true;
        this.FMsDGV.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.FMsDGV_CellDoubleClick);
        this.FMsDGV.CellValueNeeded += new System.Windows.Forms.DataGridViewCellValueEventHandler(this.FMsDGV_CellValueNeeded);
        this.FMsDGV.ColumnHeaderMouseClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.FMsDGV_ColumnHeaderMouseClick);
        this.FMsDGV.SelectionChanged += new System.EventHandler(this.FMsDGV_SelectionChanged);
        this.FMsDGV.KeyDown += new System.Windows.Forms.KeyEventHandler(this.FMsDGV_KeyDown);
        this.FMsDGV.MouseDown += new System.Windows.Forms.MouseEventHandler(this.FMsDGV_MouseDown);
        // 
        // GameTypeColumn
        // 
        this.GameTypeColumn.HeaderText = "Game";
        this.GameTypeColumn.ImageLayout = System.Windows.Forms.DataGridViewImageCellLayout.Zoom;
        this.GameTypeColumn.MinimumWidth = 25;
        this.GameTypeColumn.Name = "GameTypeColumn";
        this.GameTypeColumn.ReadOnly = true;
        this.GameTypeColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Programmatic;
        // 
        // InstalledColumn
        // 
        this.InstalledColumn.HeaderText = "Installed";
        this.InstalledColumn.ImageLayout = System.Windows.Forms.DataGridViewImageCellLayout.Zoom;
        this.InstalledColumn.MinimumWidth = 25;
        this.InstalledColumn.Name = "InstalledColumn";
        this.InstalledColumn.ReadOnly = true;
        this.InstalledColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Programmatic;
        // 
        // MisCountColumn
        // 
        this.MisCountColumn.HeaderText = "Mission count";
        this.MisCountColumn.MinimumWidth = 25;
        this.MisCountColumn.Name = "MisCountColumn";
        this.MisCountColumn.ReadOnly = true;
        this.MisCountColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Programmatic;
        // 
        // TitleColumn
        // 
        this.TitleColumn.HeaderText = "Title";
        this.TitleColumn.MinimumWidth = 25;
        this.TitleColumn.Name = "TitleColumn";
        this.TitleColumn.ReadOnly = true;
        this.TitleColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Programmatic;
        // 
        // ArchiveColumn
        // 
        this.ArchiveColumn.HeaderText = "Archive";
        this.ArchiveColumn.MinimumWidth = 25;
        this.ArchiveColumn.Name = "ArchiveColumn";
        this.ArchiveColumn.ReadOnly = true;
        this.ArchiveColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Programmatic;
        // 
        // AuthorColumn
        // 
        this.AuthorColumn.HeaderText = "Author";
        this.AuthorColumn.MinimumWidth = 25;
        this.AuthorColumn.Name = "AuthorColumn";
        this.AuthorColumn.ReadOnly = true;
        this.AuthorColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Programmatic;
        // 
        // SizeColumn
        // 
        dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
        this.SizeColumn.DefaultCellStyle = dataGridViewCellStyle2;
        this.SizeColumn.HeaderText = "Size";
        this.SizeColumn.MinimumWidth = 25;
        this.SizeColumn.Name = "SizeColumn";
        this.SizeColumn.ReadOnly = true;
        this.SizeColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Programmatic;
        // 
        // RatingTextColumn
        // 
        this.RatingTextColumn.HeaderText = "Rating";
        this.RatingTextColumn.MinimumWidth = 25;
        this.RatingTextColumn.Name = "RatingTextColumn";
        this.RatingTextColumn.ReadOnly = true;
        this.RatingTextColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Programmatic;
        // 
        // FinishedColumn
        // 
        this.FinishedColumn.HeaderText = "Finished";
        this.FinishedColumn.ImageLayout = System.Windows.Forms.DataGridViewImageCellLayout.Zoom;
        this.FinishedColumn.Name = "FinishedColumn";
        this.FinishedColumn.ReadOnly = true;
        this.FinishedColumn.Resizable = System.Windows.Forms.DataGridViewTriState.False;
        this.FinishedColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Programmatic;
        this.FinishedColumn.Width = 71;
        // 
        // ReleaseDateColumn
        // 
        this.ReleaseDateColumn.HeaderText = "Release Date";
        this.ReleaseDateColumn.MinimumWidth = 25;
        this.ReleaseDateColumn.Name = "ReleaseDateColumn";
        this.ReleaseDateColumn.ReadOnly = true;
        this.ReleaseDateColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Programmatic;
        // 
        // LastPlayedColumn
        // 
        this.LastPlayedColumn.HeaderText = "Last Played";
        this.LastPlayedColumn.MinimumWidth = 25;
        this.LastPlayedColumn.Name = "LastPlayedColumn";
        this.LastPlayedColumn.ReadOnly = true;
        this.LastPlayedColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Programmatic;
        // 
        // DateAddedColumn
        // 
        this.DateAddedColumn.HeaderText = "Date Added";
        this.DateAddedColumn.MinimumWidth = 25;
        this.DateAddedColumn.Name = "DateAddedColumn";
        this.DateAddedColumn.ReadOnly = true;
        this.DateAddedColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Programmatic;
        // 
        // DisabledModsColumn
        // 
        this.DisabledModsColumn.HeaderText = "Disabled Mods";
        this.DisabledModsColumn.MinimumWidth = 25;
        this.DisabledModsColumn.Name = "DisabledModsColumn";
        this.DisabledModsColumn.ReadOnly = true;
        this.DisabledModsColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Programmatic;
        // 
        // CommentColumn
        // 
        this.CommentColumn.HeaderText = "Comment";
        this.CommentColumn.MinimumWidth = 25;
        this.CommentColumn.Name = "CommentColumn";
        this.CommentColumn.ReadOnly = true;
        this.CommentColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Programmatic;
        // 
        // FilterBarFLP
        // 
        this.FilterBarFLP.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
        | System.Windows.Forms.AnchorStyles.Right)));
        this.FilterBarFLP.AutoScroll = true;
        this.FilterBarFLP.Controls.Add(this.FilterGameButtonsToolStrip);
        this.FilterBarFLP.Controls.Add(this.GameFilterControlsShowHideButtonToolStrip);
        this.FilterBarFLP.Controls.Add(this.FilterTitleLabel);
        this.FilterBarFLP.Controls.Add(this.FilterTitleTextBox);
        this.FilterBarFLP.Controls.Add(this.FilterAuthorLabel);
        this.FilterBarFLP.Controls.Add(this.FilterAuthorTextBox);
        this.FilterBarFLP.Controls.Add(this.FilterIconButtonsToolStrip);
        this.FilterBarFLP.Location = new System.Drawing.Point(144, 0);
        this.FilterBarFLP.Name = "FilterBarFLP";
        this.FilterBarFLP.Size = new System.Drawing.Size(768, 100);
        this.FilterBarFLP.TabIndex = 11;
        this.FilterBarFLP.WrapContents = false;
        this.FilterBarFLP.Scroll += new System.Windows.Forms.ScrollEventHandler(this.FilterBarFLP_Scroll);
        this.FilterBarFLP.SizeChanged += new System.EventHandler(this.FilterBarFLP_SizeChanged);
        this.FilterBarFLP.Paint += new System.Windows.Forms.PaintEventHandler(this.FilterBarFLP_Paint);
        // 
        // FilterGameButtonsToolStrip
        // 
        this.FilterGameButtonsToolStrip.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
        | System.Windows.Forms.AnchorStyles.Left)));
        this.FilterGameButtonsToolStrip.BackColor = System.Drawing.SystemColors.Control;
        this.FilterGameButtonsToolStrip.CanOverflow = false;
        this.FilterGameButtonsToolStrip.Dock = System.Windows.Forms.DockStyle.None;
        this.FilterGameButtonsToolStrip.GripMargin = new System.Windows.Forms.Padding(0);
        this.FilterGameButtonsToolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
        this.FilterGameButtonsToolStrip.ImageScalingSize = new System.Drawing.Size(22, 22);
        this.FilterGameButtonsToolStrip.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.Flow;
        this.FilterGameButtonsToolStrip.Location = new System.Drawing.Point(0, 0);
        this.FilterGameButtonsToolStrip.Name = "FilterGameButtonsToolStrip";
        this.FilterGameButtonsToolStrip.Size = new System.Drawing.Size(1, 26);
        this.FilterGameButtonsToolStrip.TabIndex = 3;
        // 
        // GameFilterControlsShowHideButtonToolStrip
        // 
        this.GameFilterControlsShowHideButtonToolStrip.AutoSize = false;
        this.GameFilterControlsShowHideButtonToolStrip.BackColor = System.Drawing.SystemColors.Control;
        this.GameFilterControlsShowHideButtonToolStrip.CanOverflow = false;
        this.GameFilterControlsShowHideButtonToolStrip.Dock = System.Windows.Forms.DockStyle.None;
        this.GameFilterControlsShowHideButtonToolStrip.GripMargin = new System.Windows.Forms.Padding(0);
        this.GameFilterControlsShowHideButtonToolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
        this.GameFilterControlsShowHideButtonToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.GameFilterControlsShowHideButton});
        this.GameFilterControlsShowHideButtonToolStrip.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.Flow;
        this.GameFilterControlsShowHideButtonToolStrip.Location = new System.Drawing.Point(1, 0);
        this.GameFilterControlsShowHideButtonToolStrip.Name = "GameFilterControlsShowHideButtonToolStrip";
        this.GameFilterControlsShowHideButtonToolStrip.Padding = new System.Windows.Forms.Padding(0);
        this.GameFilterControlsShowHideButtonToolStrip.Size = new System.Drawing.Size(13, 26);
        this.GameFilterControlsShowHideButtonToolStrip.TabIndex = 0;
        // 
        // GameFilterControlsShowHideButton
        // 
        this.GameFilterControlsShowHideButton.ArrowDirection = AngelLoader.Forms.Direction.Down;
        this.GameFilterControlsShowHideButton.AutoSize = false;
        this.GameFilterControlsShowHideButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.None;
        this.GameFilterControlsShowHideButton.ImageTransparentColor = System.Drawing.Color.Magenta;
        this.GameFilterControlsShowHideButton.Name = "GameFilterControlsShowHideButton";
        this.GameFilterControlsShowHideButton.Size = new System.Drawing.Size(11, 23);
        this.GameFilterControlsShowHideButton.Click += new System.EventHandler(this.GameFilterControlsShowHideButton_Click);
        // 
        // FilterTitleLabel
        // 
        this.FilterTitleLabel.AutoSize = true;
        this.FilterTitleLabel.Location = new System.Drawing.Point(24, 6);
        this.FilterTitleLabel.Margin = new System.Windows.Forms.Padding(10, 6, 0, 0);
        this.FilterTitleLabel.Name = "FilterTitleLabel";
        this.FilterTitleLabel.Size = new System.Drawing.Size(30, 13);
        this.FilterTitleLabel.TabIndex = 5;
        this.FilterTitleLabel.Text = "Title:";
        // 
        // FilterTitleTextBox
        // 
        this.FilterTitleTextBox.Location = new System.Drawing.Point(57, 3);
        this.FilterTitleTextBox.Name = "FilterTitleTextBox";
        this.FilterTitleTextBox.Size = new System.Drawing.Size(144, 20);
        this.FilterTitleTextBox.TabIndex = 6;
        this.FilterTitleTextBox.TextChanged += new System.EventHandler(this.Async_EventHandler_Main);
        // 
        // FilterAuthorLabel
        // 
        this.FilterAuthorLabel.AutoSize = true;
        this.FilterAuthorLabel.Location = new System.Drawing.Point(213, 6);
        this.FilterAuthorLabel.Margin = new System.Windows.Forms.Padding(9, 6, 0, 0);
        this.FilterAuthorLabel.Name = "FilterAuthorLabel";
        this.FilterAuthorLabel.Size = new System.Drawing.Size(41, 13);
        this.FilterAuthorLabel.TabIndex = 7;
        this.FilterAuthorLabel.Text = "Author:";
        // 
        // FilterAuthorTextBox
        // 
        this.FilterAuthorTextBox.Location = new System.Drawing.Point(257, 3);
        this.FilterAuthorTextBox.Name = "FilterAuthorTextBox";
        this.FilterAuthorTextBox.Size = new System.Drawing.Size(144, 20);
        this.FilterAuthorTextBox.TabIndex = 8;
        this.FilterAuthorTextBox.TextChanged += new System.EventHandler(this.Async_EventHandler_Main);
        // 
        // FilterIconButtonsToolStrip
        // 
        this.FilterIconButtonsToolStrip.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
        | System.Windows.Forms.AnchorStyles.Left)));
        this.FilterIconButtonsToolStrip.BackColor = System.Drawing.SystemColors.Control;
        this.FilterIconButtonsToolStrip.CanOverflow = false;
        this.FilterIconButtonsToolStrip.Dock = System.Windows.Forms.DockStyle.None;
        this.FilterIconButtonsToolStrip.GripMargin = new System.Windows.Forms.Padding(0);
        this.FilterIconButtonsToolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
        this.FilterIconButtonsToolStrip.ImageScalingSize = new System.Drawing.Size(22, 22);
        this.FilterIconButtonsToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.FilterByReleaseDateButton,
            this.FilterByLastPlayedButton,
            this.FilterByTagsButton,
            this.FilterByFinishedButton,
            this.FilterByUnfinishedButton,
            this.FilterByRatingButton,
            this.FilterShowUnsupportedButton,
            this.FilterShowUnavailableButton,
            this.FilterShowRecentAtTopButton,
            this.FilterControlsShowHideButton});
        this.FilterIconButtonsToolStrip.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.Flow;
        this.FilterIconButtonsToolStrip.Location = new System.Drawing.Point(404, 0);
        this.FilterIconButtonsToolStrip.Name = "FilterIconButtonsToolStrip";
        this.FilterIconButtonsToolStrip.Size = new System.Drawing.Size(285, 26);
        this.FilterIconButtonsToolStrip.TabIndex = 3;
        this.FilterIconButtonsToolStrip.Paint += new System.Windows.Forms.PaintEventHandler(this.FilterIconButtonsToolStrip_Paint);
        // 
        // FilterByReleaseDateButton
        // 
        this.FilterByReleaseDateButton.AutoSize = false;
        this.FilterByReleaseDateButton.BackColor = System.Drawing.SystemColors.Control;
        this.FilterByReleaseDateButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
        this.FilterByReleaseDateButton.Image = Images.FilterByReleaseDate;
        this.FilterByReleaseDateButton.ImageTransparentColor = System.Drawing.Color.Magenta;
        this.FilterByReleaseDateButton.Margin = new System.Windows.Forms.Padding(6, 0, 0, 0);
        this.FilterByReleaseDateButton.Name = "FilterByReleaseDateButton";
        this.FilterByReleaseDateButton.Size = new System.Drawing.Size(25, 25);
        this.FilterByReleaseDateButton.ToolTipText = "Release date";
        this.FilterByReleaseDateButton.Click += new System.EventHandler(this.FilterWindowOpenButtons_Click);
        // 
        // FilterByLastPlayedButton
        // 
        this.FilterByLastPlayedButton.AutoSize = false;
        this.FilterByLastPlayedButton.BackColor = System.Drawing.SystemColors.Control;
        this.FilterByLastPlayedButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
        this.FilterByLastPlayedButton.Image = Images.FilterByLastPlayed;
        this.FilterByLastPlayedButton.ImageTransparentColor = System.Drawing.Color.Magenta;
        this.FilterByLastPlayedButton.Margin = new System.Windows.Forms.Padding(6, 0, 0, 0);
        this.FilterByLastPlayedButton.Name = "FilterByLastPlayedButton";
        this.FilterByLastPlayedButton.Size = new System.Drawing.Size(25, 25);
        this.FilterByLastPlayedButton.ToolTipText = "Last played";
        this.FilterByLastPlayedButton.Click += new System.EventHandler(this.FilterWindowOpenButtons_Click);
        // 
        // FilterByTagsButton
        // 
        this.FilterByTagsButton.AutoSize = false;
        this.FilterByTagsButton.BackColor = System.Drawing.SystemColors.Control;
        this.FilterByTagsButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
        this.FilterByTagsButton.Image = Images.FilterByTags;
        this.FilterByTagsButton.ImageTransparentColor = System.Drawing.Color.Magenta;
        this.FilterByTagsButton.Margin = new System.Windows.Forms.Padding(6, 0, 0, 0);
        this.FilterByTagsButton.Name = "FilterByTagsButton";
        this.FilterByTagsButton.Size = new System.Drawing.Size(25, 25);
        this.FilterByTagsButton.ToolTipText = "Tags";
        this.FilterByTagsButton.Click += new System.EventHandler(this.FilterWindowOpenButtons_Click);
        // 
        // FilterByFinishedButton
        // 
        this.FilterByFinishedButton.AutoSize = false;
        this.FilterByFinishedButton.BackColor = System.Drawing.SystemColors.Control;
        this.FilterByFinishedButton.CheckOnClick = true;
        this.FilterByFinishedButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
        this.FilterByFinishedButton.Image = Images.FilterByFinished;
        this.FilterByFinishedButton.ImageTransparentColor = System.Drawing.Color.Magenta;
        this.FilterByFinishedButton.Margin = new System.Windows.Forms.Padding(6, 0, 0, 0);
        this.FilterByFinishedButton.Name = "FilterByFinishedButton";
        this.FilterByFinishedButton.Size = new System.Drawing.Size(25, 25);
        this.FilterByFinishedButton.ToolTipText = "Finished";
        this.FilterByFinishedButton.Click += new System.EventHandler(this.Async_EventHandler_Main);
        // 
        // FilterByUnfinishedButton
        // 
        this.FilterByUnfinishedButton.AutoSize = false;
        this.FilterByUnfinishedButton.BackColor = System.Drawing.SystemColors.Control;
        this.FilterByUnfinishedButton.CheckOnClick = true;
        this.FilterByUnfinishedButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
        this.FilterByUnfinishedButton.Image = Images.FilterByUnfinished;
        this.FilterByUnfinishedButton.ImageTransparentColor = System.Drawing.Color.Magenta;
        this.FilterByUnfinishedButton.Margin = new System.Windows.Forms.Padding(0);
        this.FilterByUnfinishedButton.Name = "FilterByUnfinishedButton";
        this.FilterByUnfinishedButton.Size = new System.Drawing.Size(25, 25);
        this.FilterByUnfinishedButton.ToolTipText = "Unfinished";
        this.FilterByUnfinishedButton.Click += new System.EventHandler(this.Async_EventHandler_Main);
        // 
        // FilterByRatingButton
        // 
        this.FilterByRatingButton.AutoSize = false;
        this.FilterByRatingButton.BackColor = System.Drawing.SystemColors.Control;
        this.FilterByRatingButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
        this.FilterByRatingButton.Image = Images.FilterByRating;
        this.FilterByRatingButton.ImageTransparentColor = System.Drawing.Color.Magenta;
        this.FilterByRatingButton.Margin = new System.Windows.Forms.Padding(6, 0, 0, 0);
        this.FilterByRatingButton.Name = "FilterByRatingButton";
        this.FilterByRatingButton.Size = new System.Drawing.Size(25, 25);
        this.FilterByRatingButton.ToolTipText = "Rating";
        this.FilterByRatingButton.Click += new System.EventHandler(this.FilterWindowOpenButtons_Click);
        // 
        // FilterShowUnsupportedButton
        // 
        this.FilterShowUnsupportedButton.AutoSize = false;
        this.FilterShowUnsupportedButton.BackColor = System.Drawing.SystemColors.Control;
        this.FilterShowUnsupportedButton.CheckOnClick = true;
        this.FilterShowUnsupportedButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
        this.FilterShowUnsupportedButton.Image = global::AngelLoader.Properties.Resources.ShowUnsupported;
        this.FilterShowUnsupportedButton.ImageTransparentColor = System.Drawing.Color.Magenta;
        this.FilterShowUnsupportedButton.Margin = new System.Windows.Forms.Padding(6, 0, 0, 0);
        this.FilterShowUnsupportedButton.Name = "FilterShowUnsupportedButton";
        this.FilterShowUnsupportedButton.Size = new System.Drawing.Size(25, 25);
        this.FilterShowUnsupportedButton.ToolTipText = "Show FMs marked as \"unsupported game or non-FM archive\"";
        this.FilterShowUnsupportedButton.Click += new System.EventHandler(this.Async_EventHandler_Main);
        // 
        // FilterShowUnavailableButton
        // 
        this.FilterShowUnavailableButton.AutoSize = false;
        this.FilterShowUnavailableButton.CheckOnClick = true;
        this.FilterShowUnavailableButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
        this.FilterShowUnavailableButton.Image = global::AngelLoader.Properties.Resources.ShowUnavailable;
        this.FilterShowUnavailableButton.ImageTransparentColor = System.Drawing.Color.Magenta;
        this.FilterShowUnavailableButton.Margin = new System.Windows.Forms.Padding(0);
        this.FilterShowUnavailableButton.Name = "FilterShowUnavailableButton";
        this.FilterShowUnavailableButton.Size = new System.Drawing.Size(25, 25);
        this.FilterShowUnavailableButton.ToolTipText = "Show unavailable FMs";
        this.FilterShowUnavailableButton.Click += new System.EventHandler(this.Async_EventHandler_Main);
        // 
        // FilterShowRecentAtTopButton
        // 
        this.FilterShowRecentAtTopButton.AutoSize = false;
        this.FilterShowRecentAtTopButton.CheckOnClick = true;
        this.FilterShowRecentAtTopButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
        this.FilterShowRecentAtTopButton.Image = Images.FilterShowRecentAtTop;
        this.FilterShowRecentAtTopButton.ImageTransparentColor = System.Drawing.Color.Magenta;
        this.FilterShowRecentAtTopButton.Margin = new System.Windows.Forms.Padding(6, 0, 2, 0);
        this.FilterShowRecentAtTopButton.Name = "FilterShowRecentAtTopButton";
        this.FilterShowRecentAtTopButton.Size = new System.Drawing.Size(25, 25);
        this.FilterShowRecentAtTopButton.ToolTipText = "Show recent at top";
        this.FilterShowRecentAtTopButton.Click += new System.EventHandler(this.Async_EventHandler_Main);
        // 
        // FilterControlsShowHideButton
        // 
        this.FilterControlsShowHideButton.ArrowDirection = AngelLoader.Forms.Direction.Down;
        this.FilterControlsShowHideButton.AutoSize = false;
        this.FilterControlsShowHideButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.None;
        this.FilterControlsShowHideButton.ImageTransparentColor = System.Drawing.Color.Magenta;
        this.FilterControlsShowHideButton.Margin = new System.Windows.Forms.Padding(4, 1, 0, 2);
        this.FilterControlsShowHideButton.Name = "FilterControlsShowHideButton";
        this.FilterControlsShowHideButton.Size = new System.Drawing.Size(11, 23);
        this.FilterControlsShowHideButton.Click += new System.EventHandler(this.FilterControlsShowHideButton_Click);
        // 
        // RefreshAreaToolStrip
        // 
        this.RefreshAreaToolStrip.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
        | System.Windows.Forms.AnchorStyles.Right)));
        this.RefreshAreaToolStrip.BackColor = System.Drawing.SystemColors.Control;
        this.RefreshAreaToolStrip.CanOverflow = false;
        this.RefreshAreaToolStrip.Dock = System.Windows.Forms.DockStyle.None;
        this.RefreshAreaToolStrip.GripMargin = new System.Windows.Forms.Padding(0);
        this.RefreshAreaToolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
        this.RefreshAreaToolStrip.ImageScalingSize = new System.Drawing.Size(22, 22);
        this.RefreshAreaToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.RefreshFromDiskButton,
            this.RefreshFiltersButton,
            this.ClearFiltersButton});
        this.RefreshAreaToolStrip.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.Flow;
        this.RefreshAreaToolStrip.Location = new System.Drawing.Point(994, 0);
        this.RefreshAreaToolStrip.Name = "RefreshAreaToolStrip";
        this.RefreshAreaToolStrip.Size = new System.Drawing.Size(91, 26);
        this.RefreshAreaToolStrip.TabIndex = 12;
        this.RefreshAreaToolStrip.Paint += new System.Windows.Forms.PaintEventHandler(this.RefreshAreaToolStrip_Paint);
        // 
        // RefreshFromDiskButton
        // 
        this.RefreshFromDiskButton.AutoSize = false;
        this.RefreshFromDiskButton.BackColor = System.Drawing.SystemColors.Control;
        this.RefreshFromDiskButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
        this.RefreshFromDiskButton.Image = Images.Refresh;
        this.RefreshFromDiskButton.ImageTransparentColor = System.Drawing.Color.Magenta;
        this.RefreshFromDiskButton.Margin = new System.Windows.Forms.Padding(6, 0, 0, 0);
        this.RefreshFromDiskButton.Name = "RefreshFromDiskButton";
        this.RefreshFromDiskButton.Size = new System.Drawing.Size(25, 25);
        this.RefreshFromDiskButton.ToolTipText = "Refresh from disk";
        this.RefreshFromDiskButton.Click += new System.EventHandler(this.Async_EventHandler_Main);
        // 
        // RefreshFiltersButton
        // 
        this.RefreshFiltersButton.AutoSize = false;
        this.RefreshFiltersButton.BackColor = System.Drawing.SystemColors.Control;
        this.RefreshFiltersButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
        this.RefreshFiltersButton.Image = Images.RefreshFilters;
        this.RefreshFiltersButton.ImageTransparentColor = System.Drawing.Color.Magenta;
        this.RefreshFiltersButton.Margin = new System.Windows.Forms.Padding(0);
        this.RefreshFiltersButton.Name = "RefreshFiltersButton";
        this.RefreshFiltersButton.Size = new System.Drawing.Size(25, 25);
        this.RefreshFiltersButton.ToolTipText = "Refresh filtered list";
        this.RefreshFiltersButton.Click += new System.EventHandler(this.Async_EventHandler_Main);
        // 
        // ClearFiltersButton
        // 
        this.ClearFiltersButton.AutoSize = false;
        this.ClearFiltersButton.BackColor = System.Drawing.SystemColors.Control;
        this.ClearFiltersButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
        this.ClearFiltersButton.Image = Images.ClearFilters;
        this.ClearFiltersButton.ImageTransparentColor = System.Drawing.Color.Magenta;
        this.ClearFiltersButton.Margin = new System.Windows.Forms.Padding(0, 0, 9, 1);
        this.ClearFiltersButton.Name = "ClearFiltersButton";
        this.ClearFiltersButton.Size = new System.Drawing.Size(25, 25);
        this.ClearFiltersButton.ToolTipText = "Clear filters";
        this.ClearFiltersButton.Click += new System.EventHandler(this.Async_EventHandler_Main);
        // 
        // ResetLayoutButton
        // 
        this.ResetLayoutButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
        this.ResetLayoutButton.FlatAppearance.BorderSize = 0;
        this.ResetLayoutButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.ResetLayoutButton.Location = new System.Drawing.Point(1090, 2);
        this.ResetLayoutButton.Name = "ResetLayoutButton";
        this.ResetLayoutButton.Size = new System.Drawing.Size(21, 21);
        this.ResetLayoutButton.TabIndex = 13;
        this.MainToolTip.SetToolTip(this.ResetLayoutButton, "Reset layout");
        this.ResetLayoutButton.PaintCustom += new System.EventHandler<System.Windows.Forms.PaintEventArgs>(this.ResetLayoutButton_Paint);
        this.ResetLayoutButton.Click += new System.EventHandler(this.ResetLayoutButton_Click);
        // 
        // GamesTabControl
        // 
        this.GamesTabControl.ImageList = this.GameTabsImageList;
        this.GamesTabControl.Location = new System.Drawing.Point(28, 5);
        this.GamesTabControl.Name = "GamesTabControl";
        this.GamesTabControl.Size = new System.Drawing.Size(1075, 24);
        this.GamesTabControl.TabIndex = 1;
        // 
        // TopFMTabsMenuButton
        // 
        this.TopFMTabsMenuButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
        this.TopFMTabsMenuButton.FlatAppearance.BorderSize = 0;
        this.TopFMTabsMenuButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.TopFMTabsMenuButton.Location = new System.Drawing.Point(533, 0);
        this.TopFMTabsMenuButton.Name = "TopFMTabsMenuButton";
        this.TopFMTabsMenuButton.Size = new System.Drawing.Size(18, 20);
        this.TopFMTabsMenuButton.TabIndex = 13;
        this.TopFMTabsMenuButton.PaintCustom += new System.EventHandler<System.Windows.Forms.PaintEventArgs>(this.FMTabsMenuButton_Paint);
        this.TopFMTabsMenuButton.Click += new System.EventHandler(this.TopFMTabsMenuButton_Click);
        // 
        // TopFMTabsCollapseButton
        // 
        this.TopFMTabsCollapseButton.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
        | System.Windows.Forms.AnchorStyles.Right)));
        this.TopFMTabsCollapseButton.ArrowDirection = AngelLoader.Forms.Direction.Right;
        this.TopFMTabsCollapseButton.FlatAppearance.BorderSize = 0;
        this.TopFMTabsCollapseButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.TopFMTabsCollapseButton.Location = new System.Drawing.Point(533, 20);
        this.TopFMTabsCollapseButton.Name = "TopFMTabsCollapseButton";
        this.TopFMTabsCollapseButton.Size = new System.Drawing.Size(18, 289);
        this.TopFMTabsCollapseButton.TabIndex = 14;
        this.TopFMTabsCollapseButton.Click += new System.EventHandler(this.TopFMTabsCollapseButton_Click);
        // 
        // TopFMTabControl
        // 
        this.TopFMTabControl.AllowReordering = true;
        this.TopFMTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
        | System.Windows.Forms.AnchorStyles.Left)
        | System.Windows.Forms.AnchorStyles.Right)));
        this.TopFMTabControl.Controls.Add(this.StatisticsTabPage);
        this.TopFMTabControl.Controls.Add(this.EditFMTabPage);
        this.TopFMTabControl.Controls.Add(this.CommentTabPage);
        this.TopFMTabControl.Controls.Add(this.TagsTabPage);
        this.TopFMTabControl.Controls.Add(this.PatchTabPage);
        this.TopFMTabControl.Controls.Add(this.ModsTabPage);
        this.TopFMTabControl.Controls.Add(this.ScreenshotsTabPage);
        this.TopFMTabControl.EnableScrollButtonsRefreshHack = true;
        this.TopFMTabControl.Location = new System.Drawing.Point(0, 0);
        this.TopFMTabControl.Name = "TopFMTabControl";
        this.TopFMTabControl.SelectedIndex = 0;
        this.TopFMTabControl.Size = new System.Drawing.Size(535, 310);
        this.TopFMTabControl.TabIndex = 15;
        // 
        // StatisticsTabPage
        // 
        this.StatisticsTabPage.BackColor = System.Drawing.SystemColors.Control;
        this.StatisticsTabPage.Location = new System.Drawing.Point(4, 22);
        this.StatisticsTabPage.Name = "StatisticsTabPage";
        this.StatisticsTabPage.Size = new System.Drawing.Size(527, 284);
        this.StatisticsTabPage.TabIndex = 0;
        this.StatisticsTabPage.Text = "Statistics";
        // 
        // EditFMTabPage
        // 
        this.EditFMTabPage.BackColor = System.Drawing.SystemColors.Control;
        this.EditFMTabPage.Location = new System.Drawing.Point(4, 22);
        this.EditFMTabPage.Name = "EditFMTabPage";
        this.EditFMTabPage.Size = new System.Drawing.Size(527, 284);
        this.EditFMTabPage.TabIndex = 2;
        this.EditFMTabPage.Text = "Edit FM";
        // 
        // CommentTabPage
        // 
        this.CommentTabPage.BackColor = System.Drawing.SystemColors.Control;
        this.CommentTabPage.Location = new System.Drawing.Point(4, 22);
        this.CommentTabPage.Name = "CommentTabPage";
        this.CommentTabPage.Size = new System.Drawing.Size(527, 284);
        this.CommentTabPage.TabIndex = 0;
        this.CommentTabPage.Text = "Comment";
        // 
        // TagsTabPage
        // 
        this.TagsTabPage.BackColor = System.Drawing.SystemColors.Control;
        this.TagsTabPage.Location = new System.Drawing.Point(4, 22);
        this.TagsTabPage.Name = "TagsTabPage";
        this.TagsTabPage.Size = new System.Drawing.Size(527, 284);
        this.TagsTabPage.TabIndex = 1;
        this.TagsTabPage.Text = "Tags";
        // 
        // PatchTabPage
        // 
        this.PatchTabPage.BackColor = System.Drawing.SystemColors.Control;
        this.PatchTabPage.Location = new System.Drawing.Point(4, 22);
        this.PatchTabPage.Name = "PatchTabPage";
        this.PatchTabPage.Size = new System.Drawing.Size(527, 284);
        this.PatchTabPage.TabIndex = 3;
        this.PatchTabPage.Text = "Patch & Customize";
        // 
        // ModsTabPage
        // 
        this.ModsTabPage.BackColor = System.Drawing.SystemColors.Control;
        this.ModsTabPage.Location = new System.Drawing.Point(4, 22);
        this.ModsTabPage.Name = "ModsTabPage";
        this.ModsTabPage.Size = new System.Drawing.Size(527, 284);
        this.ModsTabPage.TabIndex = 4;
        this.ModsTabPage.Text = "Mods";
        // 
        // ScreenshotsTabPage
        // 
        this.ScreenshotsTabPage.BackColor = System.Drawing.SystemColors.Control;
        this.ScreenshotsTabPage.Location = new System.Drawing.Point(4, 22);
        this.ScreenshotsTabPage.Name = "ScreenshotsTabPage";
        this.ScreenshotsTabPage.Size = new System.Drawing.Size(527, 284);
        this.ScreenshotsTabPage.TabIndex = 5;
        this.ScreenshotsTabPage.Text = "Screenshots";
        // 
        // LowerSplitContainer
        // 
        this.LowerSplitContainer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
        | System.Windows.Forms.AnchorStyles.Left)
        | System.Windows.Forms.AnchorStyles.Right)));
        this.LowerSplitContainer.BackColor = System.Drawing.SystemColors.ActiveBorder;
        this.LowerSplitContainer.Location = new System.Drawing.Point(0, 0);
        this.LowerSplitContainer.Name = "LowerSplitContainer";
        // 
        // LowerSplitContainer.Panel1
        // 
        this.LowerSplitContainer.Panel1.BackColor = System.Drawing.SystemColors.Control;
        this.LowerSplitContainer.Panel1.Controls.Add(this.ReadmeEncodingButton);
        this.LowerSplitContainer.Panel1.Controls.Add(this.ReadmeFullScreenButton);
        this.LowerSplitContainer.Panel1.Controls.Add(this.ReadmeZoomInButton);
        this.LowerSplitContainer.Panel1.Controls.Add(this.ReadmeZoomOutButton);
        this.LowerSplitContainer.Panel1.Controls.Add(this.ReadmeResetZoomButton);
        this.LowerSplitContainer.Panel1.Controls.Add(this.ChooseReadmeComboBox);
        this.LowerSplitContainer.Panel1.Controls.Add(this.ReadmeRichTextBox);
        // 
        // LowerSplitContainer.Panel2
        // 
        this.LowerSplitContainer.Panel2.BackColor = System.Drawing.SystemColors.Control;
        this.LowerSplitContainer.Panel2Collapsed = true;
        this.LowerSplitContainer.Size = new System.Drawing.Size(1671, 357);
        this.LowerSplitContainer.SplitterDistance = 1613;
        this.LowerSplitContainer.TabIndex = 0;
        // 
        // ReadmeEncodingButton
        // 
        this.ReadmeEncodingButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
        this.ReadmeEncodingButton.BackColor = System.Drawing.SystemColors.Window;
        this.ReadmeEncodingButton.FlatAppearance.BorderSize = 0;
        this.ReadmeEncodingButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.ReadmeEncodingButton.Location = new System.Drawing.Point(1502, 8);
        this.ReadmeEncodingButton.Name = "ReadmeEncodingButton";
        this.ReadmeEncodingButton.Size = new System.Drawing.Size(21, 21);
        this.ReadmeEncodingButton.TabIndex = 2;
        this.MainToolTip.SetToolTip(this.ReadmeEncodingButton, "Character encoding");
        this.ReadmeEncodingButton.Visible = false;
        this.ReadmeEncodingButton.PaintCustom += new System.EventHandler<System.Windows.Forms.PaintEventArgs>(this.ReadmeButtons_Paint);
        this.ReadmeEncodingButton.Click += new System.EventHandler(this.ReadmeButtons_Click);
        this.ReadmeEncodingButton.MouseLeave += new System.EventHandler(this.ReadmeArea_MouseLeave);
        // 
        // ReadmeFullScreenButton
        // 
        this.ReadmeFullScreenButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
        this.ReadmeFullScreenButton.BackColor = System.Drawing.SystemColors.Window;
        this.ReadmeFullScreenButton.FlatAppearance.BorderSize = 0;
        this.ReadmeFullScreenButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.ReadmeFullScreenButton.Location = new System.Drawing.Point(1616, 8);
        this.ReadmeFullScreenButton.Name = "ReadmeFullScreenButton";
        this.ReadmeFullScreenButton.Size = new System.Drawing.Size(21, 21);
        this.ReadmeFullScreenButton.TabIndex = 6;
        this.MainToolTip.SetToolTip(this.ReadmeFullScreenButton, "Fullscreen");
        this.ReadmeFullScreenButton.Visible = false;
        this.ReadmeFullScreenButton.PaintCustom += new System.EventHandler<System.Windows.Forms.PaintEventArgs>(this.ReadmeButtons_Paint);
        this.ReadmeFullScreenButton.Click += new System.EventHandler(this.ReadmeButtons_Click);
        this.ReadmeFullScreenButton.MouseLeave += new System.EventHandler(this.ReadmeArea_MouseLeave);
        // 
        // ReadmeZoomInButton
        // 
        this.ReadmeZoomInButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
        this.ReadmeZoomInButton.BackColor = System.Drawing.SystemColors.Window;
        this.ReadmeZoomInButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
        this.ReadmeZoomInButton.FlatAppearance.BorderSize = 0;
        this.ReadmeZoomInButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.ReadmeZoomInButton.Location = new System.Drawing.Point(1534, 8);
        this.ReadmeZoomInButton.Name = "ReadmeZoomInButton";
        this.ReadmeZoomInButton.Size = new System.Drawing.Size(21, 21);
        this.ReadmeZoomInButton.TabIndex = 3;
        this.MainToolTip.SetToolTip(this.ReadmeZoomInButton, "Zoom in");
        this.ReadmeZoomInButton.Visible = false;
        this.ReadmeZoomInButton.PaintCustom += new System.EventHandler<System.Windows.Forms.PaintEventArgs>(this.ReadmeButtons_Paint);
        this.ReadmeZoomInButton.Click += new System.EventHandler(this.ReadmeButtons_Click);
        this.ReadmeZoomInButton.MouseLeave += new System.EventHandler(this.ReadmeArea_MouseLeave);
        // 
        // ReadmeZoomOutButton
        // 
        this.ReadmeZoomOutButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
        this.ReadmeZoomOutButton.BackColor = System.Drawing.SystemColors.Window;
        this.ReadmeZoomOutButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
        this.ReadmeZoomOutButton.FlatAppearance.BorderSize = 0;
        this.ReadmeZoomOutButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.ReadmeZoomOutButton.Location = new System.Drawing.Point(1559, 8);
        this.ReadmeZoomOutButton.Name = "ReadmeZoomOutButton";
        this.ReadmeZoomOutButton.Size = new System.Drawing.Size(21, 21);
        this.ReadmeZoomOutButton.TabIndex = 4;
        this.MainToolTip.SetToolTip(this.ReadmeZoomOutButton, "Zoom out");
        this.ReadmeZoomOutButton.Visible = false;
        this.ReadmeZoomOutButton.PaintCustom += new System.EventHandler<System.Windows.Forms.PaintEventArgs>(this.ReadmeButtons_Paint);
        this.ReadmeZoomOutButton.Click += new System.EventHandler(this.ReadmeButtons_Click);
        this.ReadmeZoomOutButton.MouseLeave += new System.EventHandler(this.ReadmeArea_MouseLeave);
        // 
        // ReadmeResetZoomButton
        // 
        this.ReadmeResetZoomButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
        this.ReadmeResetZoomButton.BackColor = System.Drawing.SystemColors.Window;
        this.ReadmeResetZoomButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
        this.ReadmeResetZoomButton.FlatAppearance.BorderSize = 0;
        this.ReadmeResetZoomButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.ReadmeResetZoomButton.Location = new System.Drawing.Point(1584, 8);
        this.ReadmeResetZoomButton.Name = "ReadmeResetZoomButton";
        this.ReadmeResetZoomButton.Size = new System.Drawing.Size(21, 21);
        this.ReadmeResetZoomButton.TabIndex = 5;
        this.MainToolTip.SetToolTip(this.ReadmeResetZoomButton, "Reset zoom");
        this.ReadmeResetZoomButton.Visible = false;
        this.ReadmeResetZoomButton.PaintCustom += new System.EventHandler<System.Windows.Forms.PaintEventArgs>(this.ReadmeButtons_Paint);
        this.ReadmeResetZoomButton.Click += new System.EventHandler(this.ReadmeButtons_Click);
        this.ReadmeResetZoomButton.MouseLeave += new System.EventHandler(this.ReadmeArea_MouseLeave);
        // 
        // ChooseReadmeComboBox
        // 
        this.ChooseReadmeComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
        this.ChooseReadmeComboBox.FireMouseLeaveOnLeaveWindow = true;
        this.ChooseReadmeComboBox.FormattingEnabled = true;
        this.ChooseReadmeComboBox.Location = new System.Drawing.Point(1321, 8);
        this.ChooseReadmeComboBox.Name = "ChooseReadmeComboBox";
        this.ChooseReadmeComboBox.Size = new System.Drawing.Size(170, 21);
        this.ChooseReadmeComboBox.TabIndex = 1;
        this.ChooseReadmeComboBox.Visible = false;
        this.ChooseReadmeComboBox.SelectedIndexChanged += new System.EventHandler(this.ChooseReadmeComboBox_SelectedIndexChanged);
        this.ChooseReadmeComboBox.DropDownClosed += new System.EventHandler(this.ChooseReadmeComboBox_DropDownClosed);
        this.ChooseReadmeComboBox.MouseLeave += new System.EventHandler(this.ReadmeArea_MouseLeave);
        // 
        // ReadmeRichTextBox
        // 
        this.ReadmeRichTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
        | System.Windows.Forms.AnchorStyles.Left)
        | System.Windows.Forms.AnchorStyles.Right)));
        this.ReadmeRichTextBox.BackColor = System.Drawing.SystemColors.Window;
        this.ReadmeRichTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
        this.ReadmeRichTextBox.Location = new System.Drawing.Point(1, 1);
        this.ReadmeRichTextBox.Name = "ReadmeRichTextBox";
        this.ReadmeRichTextBox.ReadOnly = true;
        this.ReadmeRichTextBox.Size = new System.Drawing.Size(1668, 356);
        this.ReadmeRichTextBox.TabIndex = 0;
        this.ReadmeRichTextBox.Text = "";
        this.ReadmeRichTextBox.MouseLeave += new System.EventHandler(this.ReadmeArea_MouseLeave);
        // 
        // MainForm
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
        this.ClientSize = new System.Drawing.Size(1671, 716);
        this.Controls.Add(this.EverythingPanel);
        this.DoubleBuffered = true;
        this.KeyPreview = true;
        this.MinimumSize = new System.Drawing.Size(894, 260);
        this.Name = "MainForm";
        this.ShowInTaskbar = true;
        this.Text = "AngelLoader";
        this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
        this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.MainForm_KeyDown);
        this.BottomRightFLP.ResumeLayout(false);
        this.BottomRightFLP.PerformLayout();
        this.BottomLeftFLP.ResumeLayout(false);
        this.BottomLeftFLP.PerformLayout();
        this.EverythingPanel.ResumeLayout(false);
        this.EverythingPanel.PerformLayout();
        this.MainSplitContainer.Panel1.ResumeLayout(false);
        this.MainSplitContainer.Panel2.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)(this.MainSplitContainer)).EndInit();
        this.MainSplitContainer.ResumeLayout(false);
        this.TopSplitContainer.Panel1.ResumeLayout(false);
        this.TopSplitContainer.Panel1.PerformLayout();
        this.TopSplitContainer.Panel2.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)(this.TopSplitContainer)).EndInit();
        this.TopSplitContainer.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)(this.FMsDGV)).EndInit();
        this.FilterBarFLP.ResumeLayout(false);
        this.FilterBarFLP.PerformLayout();
        this.GameFilterControlsShowHideButtonToolStrip.ResumeLayout(false);
        this.GameFilterControlsShowHideButtonToolStrip.PerformLayout();
        this.FilterIconButtonsToolStrip.ResumeLayout(false);
        this.FilterIconButtonsToolStrip.PerformLayout();
        this.RefreshAreaToolStrip.ResumeLayout(false);
        this.RefreshAreaToolStrip.PerformLayout();
        this.TopFMTabControl.ResumeLayout(false);
        this.LowerSplitContainer.Panel1.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)(this.LowerSplitContainer)).EndInit();
        this.LowerSplitContainer.ResumeLayout(false);
        this.ResumeLayout(false);

    }

    #endregion
#endif

#if DEBUG || (Release_Testing && !RT_StartupOnly)
    internal CustomControls.DarkButton TestButton;
    internal CustomControls.DarkButton Test2Button;
    internal CustomControls.DarkButton Test3Button;
    internal CustomControls.DarkButton Test4Button;
    internal CustomControls.DarkLabel DebugLabel;
    internal CustomControls.DarkLabel DebugLabel2;
#endif

#if !ReleaseBeta && !ReleasePublic
    internal CustomControls.DarkCheckBox ForceWindowedCheckBox;
    internal CustomControls.DarkCheckBox T1ScreenShotModeCheckBox;
    internal CustomControls.DarkCheckBox T2ScreenShotModeCheckBox;
    internal CustomControls.DarkCheckBox T1TitaniumModeCheckBox;
    internal CustomControls.DarkCheckBox T2TitaniumModeCheckBox;
#endif

    internal System.Windows.Forms.Panel EverythingPanel;
    internal CustomControls.DarkSplitContainerCustom MainSplitContainer;
    internal CustomControls.DarkSplitContainerCustom TopSplitContainer;

    internal CustomControls.ToolTipCustom MainToolTip;

    #region Top bar

    internal CustomControls.DarkButton MainMenuButton;

    internal System.Windows.Forms.FlowLayoutPanel FilterBarFLP;

    internal CustomControls.DarkArrowButton FilterBarScrollLeftButton;
    internal CustomControls.DarkArrowButton FilterBarScrollRightButton;

    internal CustomControls.ToolStripCustom FilterGameButtonsToolStrip;
    internal CustomControls.DarkTabControl GamesTabControl;
    internal System.Windows.Forms.ImageList GameTabsImageList;
    internal CustomControls.ToolStripCustom GameFilterControlsShowHideButtonToolStrip;
    internal CustomControls.ToolStripArrowButton GameFilterControlsShowHideButton;

    internal CustomControls.ToolStripCustom FilterIconButtonsToolStrip;
    internal CustomControls.DarkLabel FilterTitleLabel;
    internal CustomControls.DarkTextBoxCustom FilterTitleTextBox;
    internal CustomControls.DarkLabel FilterAuthorLabel;
    internal CustomControls.DarkTextBoxCustom FilterAuthorTextBox;

    internal CustomControls.ToolStripButtonCustom FilterByReleaseDateButton;
    internal CustomControls.ToolStripButtonCustom FilterByLastPlayedButton;

    internal CustomControls.ToolStripButtonCustom FilterByTagsButton;

    internal CustomControls.ToolStripButtonCustom FilterByFinishedButton;
    internal CustomControls.ToolStripButtonCustom FilterByUnfinishedButton;

    internal CustomControls.ToolStripButtonCustom FilterByRatingButton;

    internal CustomControls.ToolStripButtonCustom FilterShowUnsupportedButton;
    internal CustomControls.ToolStripButtonCustom FilterShowUnavailableButton;
    internal CustomControls.ToolStripButtonCustom FilterShowRecentAtTopButton;

    internal CustomControls.ToolStripArrowButton FilterControlsShowHideButton;

    internal CustomControls.ToolStripCustom RefreshAreaToolStrip;

    internal CustomControls.ToolStripButtonCustom RefreshFromDiskButton;
    internal CustomControls.ToolStripButtonCustom RefreshFiltersButton;
    internal CustomControls.ToolStripButtonCustom ClearFiltersButton;

    internal CustomControls.DarkButton ResetLayoutButton;

    #endregion

    #region FMsDGV

    internal CustomControls.DataGridViewCustom FMsDGV;

#if DateAccTest
    internal System.Windows.Forms.DataGridViewImageColumn DateAccuracyColumn;
#endif
    internal System.Windows.Forms.DataGridViewImageColumn GameTypeColumn;
    internal System.Windows.Forms.DataGridViewImageColumn InstalledColumn;
    internal System.Windows.Forms.DataGridViewTextBoxColumn MisCountColumn;
    internal System.Windows.Forms.DataGridViewTextBoxColumn TitleColumn;
    internal System.Windows.Forms.DataGridViewTextBoxColumn ArchiveColumn;
    internal System.Windows.Forms.DataGridViewTextBoxColumn AuthorColumn;
    internal System.Windows.Forms.DataGridViewTextBoxColumn SizeColumn;
    internal System.Windows.Forms.DataGridViewTextBoxColumn RatingTextColumn;
    internal System.Windows.Forms.DataGridViewImageColumn RatingImageColumn;
    internal System.Windows.Forms.DataGridViewImageColumn FinishedColumn;
    internal System.Windows.Forms.DataGridViewTextBoxColumn ReleaseDateColumn;
    internal System.Windows.Forms.DataGridViewTextBoxColumn LastPlayedColumn;
    internal System.Windows.Forms.DataGridViewTextBoxColumn DateAddedColumn;
    internal System.Windows.Forms.DataGridViewTextBoxColumn DisabledModsColumn;
    internal System.Windows.Forms.DataGridViewTextBoxColumn CommentColumn;

    #endregion

    #region Top-right

    internal CustomControls.DarkTabControl TopFMTabControl;
    internal CustomControls.StatsTabPage StatisticsTabPage;
    internal CustomControls.EditFMTabPage EditFMTabPage;
    internal CustomControls.CommentTabPage CommentTabPage;
    internal CustomControls.TagsTabPage TagsTabPage;
    internal CustomControls.PatchTabPage PatchTabPage;
    internal CustomControls.ModsTabPage ModsTabPage;
    internal CustomControls.ScreenshotsTabPage ScreenshotsTabPage;

    internal CustomControls.DarkButton TopFMTabsMenuButton;
    internal CustomControls.DarkArrowButton TopFMTabsCollapseButton;

    #endregion

    #region Readme

    internal CustomControls.RichTextBoxCustom ReadmeRichTextBox;

    internal CustomControls.DarkComboBoxWithBackingItems ChooseReadmeComboBox;
    internal CustomControls.DarkButton ReadmeEncodingButton;
    internal CustomControls.DarkButton ReadmeZoomInButton;
    internal CustomControls.DarkButton ReadmeZoomOutButton;
    internal CustomControls.DarkButton ReadmeResetZoomButton;
    internal CustomControls.DarkButton ReadmeFullScreenButton;
    internal CustomControls.DarkSplitContainerCustom LowerSplitContainer;

    #endregion

    #region Bottom

    internal System.Windows.Forms.FlowLayoutPanel BottomLeftFLP;
    internal CustomControls.DarkButton PlayFMButton;
    internal System.Windows.Forms.FlowLayoutPanel PlayOriginalFLP;

    internal System.Windows.Forms.FlowLayoutPanel BottomRightFLP;
    internal CustomControls.DarkLabel FMCountLabel;
    internal CustomControls.DarkButton SettingsButton;

    #endregion
}
