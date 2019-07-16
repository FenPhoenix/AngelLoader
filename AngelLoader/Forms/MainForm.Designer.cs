namespace AngelLoader.Forms
{
    partial class MainForm
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
            this.Test2Button = new System.Windows.Forms.Button();
            this.TestButton = new System.Windows.Forms.Button();
            this.ScanAllFMsButton = new System.Windows.Forms.Button();
            this.BottomPanel = new System.Windows.Forms.Panel();
            this.BottomRightButtonsFLP = new System.Windows.Forms.FlowLayoutPanel();
            this.SettingsButton = new System.Windows.Forms.Button();
            this.ImportButton = new System.Windows.Forms.Button();
            this.BottomLeftButtonsFLP = new System.Windows.Forms.FlowLayoutPanel();
            this.PlayFMButton = new System.Windows.Forms.Button();
            this.PlayOriginalGameButton = new System.Windows.Forms.Button();
            this.InstallUninstallFMButton = new System.Windows.Forms.Button();
            this.WebSearchButton = new System.Windows.Forms.Button();
            this.DebugLabel = new System.Windows.Forms.Label();
            this.DebugLabel2 = new System.Windows.Forms.Label();
            this.EverythingPanel = new System.Windows.Forms.Panel();
            this.AddTagListBox = new System.Windows.Forms.ListBox();
            this.MainSplitContainer = new AngelLoader.CustomControls.SplitContainerCustom();
            this.TopSplitContainer = new AngelLoader.CustomControls.SplitContainerCustom();
            this.FilterBarScrollRightButton = new AngelLoader.CustomControls.ArrowButton();
            this.FilterBarScrollLeftButton = new AngelLoader.CustomControls.ArrowButton();
            this.FMsDGV = new AngelLoader.CustomControls.DataGridViewCustom();
            this.GameTypeColumn = new System.Windows.Forms.DataGridViewImageColumn();
            this.InstalledColumn = new System.Windows.Forms.DataGridViewImageColumn();
            this.TitleColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ArchiveColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.AuthorColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.SizeColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.RatingTextColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.FinishedColumn = new System.Windows.Forms.DataGridViewImageColumn();
            this.ReleaseDateColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.LastPlayedColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.DisabledModsColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.CommentColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.FilterBarFLP = new System.Windows.Forms.FlowLayoutPanel();
            this.FilterGameButtonsToolStrip = new AngelLoader.CustomControls.ToolStripCustom();
            this.FilterByThief1Button = new AngelLoader.CustomControls.ToolStripButtonCustom();
            this.FilterByThief2Button = new AngelLoader.CustomControls.ToolStripButtonCustom();
            this.FilterByThief3Button = new AngelLoader.CustomControls.ToolStripButtonCustom();
            this.FilterTitleLabel = new System.Windows.Forms.Label();
            this.FilterTitleTextBox = new AngelLoader.CustomControls.TextBoxCustom();
            this.FilterAuthorLabel = new System.Windows.Forms.Label();
            this.FilterAuthorTextBox = new AngelLoader.CustomControls.TextBoxCustom();
            this.FilterIconButtonsToolStrip = new AngelLoader.CustomControls.ToolStripCustom();
            this.FilterByReleaseDateButton = new AngelLoader.CustomControls.ToolStripButtonCustom();
            this.FilterByReleaseDateLabel = new System.Windows.Forms.ToolStripLabel();
            this.FilterByLastPlayedButton = new AngelLoader.CustomControls.ToolStripButtonCustom();
            this.FilterByLastPlayedLabel = new System.Windows.Forms.ToolStripLabel();
            this.FilterByTagsButton = new AngelLoader.CustomControls.ToolStripButtonCustom();
            this.FilterByFinishedButton = new AngelLoader.CustomControls.ToolStripButtonCustom();
            this.FilterByUnfinishedButton = new AngelLoader.CustomControls.ToolStripButtonCustom();
            this.FilterByRatingButton = new AngelLoader.CustomControls.ToolStripButtonCustom();
            this.FilterByRatingLabel = new System.Windows.Forms.ToolStripLabel();
            this.FilterShowUnsupportedButton = new AngelLoader.CustomControls.ToolStripButtonCustom();
            this.RefreshAreaToolStrip = new AngelLoader.CustomControls.ToolStripCustom();
            this.FMsListZoomInButton = new AngelLoader.CustomControls.ToolStripButtonCustom();
            this.FMsListZoomOutButton = new AngelLoader.CustomControls.ToolStripButtonCustom();
            this.FMsListResetZoomButton = new AngelLoader.CustomControls.ToolStripButtonCustom();
            this.RefreshFromDiskButton = new AngelLoader.CustomControls.ToolStripButtonCustom();
            this.RefreshFiltersButton = new AngelLoader.CustomControls.ToolStripButtonCustom();
            this.ClearFiltersButton = new AngelLoader.CustomControls.ToolStripButtonCustom();
            this.ResetLayoutButton = new System.Windows.Forms.Button();
            this.GamesTabControl = new System.Windows.Forms.TabControl();
            this.Thief1TabPage = new System.Windows.Forms.TabPage();
            this.Thief2TabPage = new System.Windows.Forms.TabPage();
            this.Thief3TabPage = new System.Windows.Forms.TabPage();
            this.TopRightMenuButton = new System.Windows.Forms.Button();
            this.TopRightCollapseButton = new AngelLoader.CustomControls.ArrowButton();
            this.TopRightTabControl = new AngelLoader.CustomControls.TabControlCustom();
            this.StatisticsTabPage = new System.Windows.Forms.TabPage();
            this.StatsScanCustomResourcesButton = new System.Windows.Forms.Button();
            this.StatsCheckBoxesPanel = new System.Windows.Forms.Panel();
            this.CR_MapCheckBox = new System.Windows.Forms.CheckBox();
            this.CR_MoviesCheckBox = new System.Windows.Forms.CheckBox();
            this.CR_MotionsCheckBox = new System.Windows.Forms.CheckBox();
            this.CR_SoundsCheckBox = new System.Windows.Forms.CheckBox();
            this.CR_CreaturesCheckBox = new System.Windows.Forms.CheckBox();
            this.CR_TexturesCheckBox = new System.Windows.Forms.CheckBox();
            this.CR_AutomapCheckBox = new System.Windows.Forms.CheckBox();
            this.CR_ScriptsCheckBox = new System.Windows.Forms.CheckBox();
            this.CR_SubtitlesCheckBox = new System.Windows.Forms.CheckBox();
            this.CR_ObjectsCheckBox = new System.Windows.Forms.CheckBox();
            this.CustomResourcesLabel = new System.Windows.Forms.Label();
            this.EditFMTabPage = new System.Windows.Forms.TabPage();
            this.EditFMScanForReadmesButton = new System.Windows.Forms.Button();
            this.EditFMScanReleaseDateButton = new System.Windows.Forms.Button();
            this.EditFMScanAuthorButton = new System.Windows.Forms.Button();
            this.EditFMScanTitleButton = new System.Windows.Forms.Button();
            this.EditFMAltTitlesArrowButton = new AngelLoader.CustomControls.ArrowButton();
            this.EditFMTitleTextBox = new System.Windows.Forms.TextBox();
            this.EditFMFinishedOnButton = new System.Windows.Forms.Button();
            this.EditFMRatingComboBox = new System.Windows.Forms.ComboBox();
            this.EditFMRatingLabel = new System.Windows.Forms.Label();
            this.EditFMLastPlayedDateTimePicker = new System.Windows.Forms.DateTimePicker();
            this.EditFMReleaseDateDateTimePicker = new System.Windows.Forms.DateTimePicker();
            this.EditFMLastPlayedCheckBox = new System.Windows.Forms.CheckBox();
            this.EditFMReleaseDateCheckBox = new System.Windows.Forms.CheckBox();
            this.EditFMDisableAllModsCheckBox = new System.Windows.Forms.CheckBox();
            this.EditFMDisabledModsTextBox = new System.Windows.Forms.TextBox();
            this.EditFMDisabledModsLabel = new System.Windows.Forms.Label();
            this.EditFMAuthorTextBox = new System.Windows.Forms.TextBox();
            this.EditFMAuthorLabel = new System.Windows.Forms.Label();
            this.EditFMTitleLabel = new System.Windows.Forms.Label();
            this.CommentTabPage = new System.Windows.Forms.TabPage();
            this.CommentTextBox = new System.Windows.Forms.TextBox();
            this.TagsTabPage = new System.Windows.Forms.TabPage();
            this.AddTagButton = new System.Windows.Forms.Button();
            this.AddTagTextBox = new AngelLoader.CustomControls.TextBoxCustom();
            this.AddRemoveTagFLP = new System.Windows.Forms.FlowLayoutPanel();
            this.RemoveTagButton = new System.Windows.Forms.Button();
            this.AddTagFromListButton = new System.Windows.Forms.Button();
            this.TagsTreeView = new System.Windows.Forms.TreeView();
            this.PatchTabPage = new System.Windows.Forms.TabPage();
            this.PatchMainPanel = new System.Windows.Forms.Panel();
            this.PatchDMLsPanel = new System.Windows.Forms.Panel();
            this.PatchDMLPatchesLabel = new System.Windows.Forms.Label();
            this.PatchDMLsListBox = new System.Windows.Forms.ListBox();
            this.PatchRemoveDMLButton = new System.Windows.Forms.Button();
            this.PatchAddDMLButton = new System.Windows.Forms.Button();
            this.PatchOpenFMFolderButton = new System.Windows.Forms.Button();
            this.PatchFMNotInstalledLabel = new System.Windows.Forms.Label();
            this.ViewHTMLReadmeButton = new System.Windows.Forms.Button();
            this.ReadmeFullScreenButton = new System.Windows.Forms.Button();
            this.ZoomInButton = new System.Windows.Forms.Button();
            this.ZoomOutButton = new System.Windows.Forms.Button();
            this.ResetZoomButton = new System.Windows.Forms.Button();
            this.ChooseReadmeComboBox = new AngelLoader.CustomControls.ComboBoxCustom();
            this.ReadmeRichTextBox = new AngelLoader.CustomControls.RichTextBoxCustom();
            this.MainToolTip = new System.Windows.Forms.ToolTip(this.components);
            this.BottomPanel.SuspendLayout();
            this.BottomRightButtonsFLP.SuspendLayout();
            this.BottomLeftButtonsFLP.SuspendLayout();
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
            this.FilterGameButtonsToolStrip.SuspendLayout();
            this.FilterIconButtonsToolStrip.SuspendLayout();
            this.RefreshAreaToolStrip.SuspendLayout();
            this.GamesTabControl.SuspendLayout();
            this.TopRightTabControl.SuspendLayout();
            this.StatisticsTabPage.SuspendLayout();
            this.StatsCheckBoxesPanel.SuspendLayout();
            this.EditFMTabPage.SuspendLayout();
            this.CommentTabPage.SuspendLayout();
            this.TagsTabPage.SuspendLayout();
            this.AddRemoveTagFLP.SuspendLayout();
            this.PatchTabPage.SuspendLayout();
            this.PatchMainPanel.SuspendLayout();
            this.PatchDMLsPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // GameTabsImageList
            // 
            this.GameTabsImageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("GameTabsImageList.ImageStream")));
            this.GameTabsImageList.TransparentColor = System.Drawing.Color.Transparent;
            this.GameTabsImageList.Images.SetKeyName(0, "Thief1_16.png");
            this.GameTabsImageList.Images.SetKeyName(1, "Thief2_16.png");
            this.GameTabsImageList.Images.SetKeyName(2, "Thief3_16.png");
            // 
            // Test2Button
            // 
            this.Test2Button.Location = new System.Drawing.Point(632, 21);
            this.Test2Button.Name = "Test2Button";
            this.Test2Button.Size = new System.Drawing.Size(75, 22);
            this.Test2Button.TabIndex = 999;
            this.Test2Button.Text = "Test2";
            this.Test2Button.UseVisualStyleBackColor = true;
            this.Test2Button.Click += new System.EventHandler(this.Test2Button_Click);
            // 
            // TestButton
            // 
            this.TestButton.Location = new System.Drawing.Point(632, 0);
            this.TestButton.Name = "TestButton";
            this.TestButton.Size = new System.Drawing.Size(75, 22);
            this.TestButton.TabIndex = 999;
            this.TestButton.Text = "Test";
            this.TestButton.UseVisualStyleBackColor = true;
            this.TestButton.Click += new System.EventHandler(this.TestButton_Click);
            // 
            // ScanAllFMsButton
            // 
            this.ScanAllFMsButton.AutoSize = true;
            this.ScanAllFMsButton.Image = global::AngelLoader.Properties.Resources.Scan;
            this.ScanAllFMsButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.ScanAllFMsButton.Location = new System.Drawing.Point(365, 3);
            this.ScanAllFMsButton.Margin = new System.Windows.Forms.Padding(11, 3, 3, 3);
            this.ScanAllFMsButton.Name = "ScanAllFMsButton";
            this.ScanAllFMsButton.Padding = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.ScanAllFMsButton.Size = new System.Drawing.Size(123, 36);
            this.ScanAllFMsButton.TabIndex = 59;
            this.ScanAllFMsButton.Text = "Scan all FMs...";
            this.ScanAllFMsButton.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.ScanAllFMsButton.UseVisualStyleBackColor = true;
            this.ScanAllFMsButton.Click += new System.EventHandler(this.ScanAllFMsButton_Click);
            // 
            // BottomPanel
            // 
            this.BottomPanel.Controls.Add(this.BottomRightButtonsFLP);
            this.BottomPanel.Controls.Add(this.BottomLeftButtonsFLP);
            this.BottomPanel.Controls.Add(this.DebugLabel);
            this.BottomPanel.Controls.Add(this.DebugLabel2);
            this.BottomPanel.Controls.Add(this.Test2Button);
            this.BottomPanel.Controls.Add(this.TestButton);
            this.BottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.BottomPanel.Location = new System.Drawing.Point(0, 672);
            this.BottomPanel.Name = "BottomPanel";
            this.BottomPanel.Size = new System.Drawing.Size(1671, 44);
            this.BottomPanel.TabIndex = 1;
            // 
            // BottomRightButtonsFLP
            // 
            this.BottomRightButtonsFLP.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.BottomRightButtonsFLP.AutoSize = true;
            this.BottomRightButtonsFLP.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BottomRightButtonsFLP.Controls.Add(this.SettingsButton);
            this.BottomRightButtonsFLP.Controls.Add(this.ImportButton);
            this.BottomRightButtonsFLP.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.BottomRightButtonsFLP.Location = new System.Drawing.Point(1443, 0);
            this.BottomRightButtonsFLP.Name = "BottomRightButtonsFLP";
            this.BottomRightButtonsFLP.Size = new System.Drawing.Size(226, 42);
            this.BottomRightButtonsFLP.TabIndex = 37;
            // 
            // SettingsButton
            // 
            this.SettingsButton.AutoSize = true;
            this.SettingsButton.Image = global::AngelLoader.Properties.Resources.Settings_24;
            this.SettingsButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.SettingsButton.Location = new System.Drawing.Point(123, 3);
            this.SettingsButton.Name = "SettingsButton";
            this.SettingsButton.Padding = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.SettingsButton.Size = new System.Drawing.Size(100, 36);
            this.SettingsButton.TabIndex = 62;
            this.SettingsButton.Text = "Settings...";
            this.SettingsButton.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.SettingsButton.UseVisualStyleBackColor = true;
            this.SettingsButton.Click += new System.EventHandler(this.SettingsButton_Click);
            // 
            // ImportButton
            // 
            this.ImportButton.AutoSize = true;
            this.ImportButton.Image = global::AngelLoader.Properties.Resources.Import_24;
            this.ImportButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.ImportButton.Location = new System.Drawing.Point(3, 3);
            this.ImportButton.Name = "ImportButton";
            this.ImportButton.Padding = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.ImportButton.Size = new System.Drawing.Size(114, 36);
            this.ImportButton.TabIndex = 61;
            this.ImportButton.Text = "Import from...";
            this.ImportButton.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.ImportButton.UseVisualStyleBackColor = true;
            this.ImportButton.Click += new System.EventHandler(this.ImportButton_Click);
            // 
            // BottomLeftButtonsFLP
            // 
            this.BottomLeftButtonsFLP.AutoSize = true;
            this.BottomLeftButtonsFLP.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BottomLeftButtonsFLP.Controls.Add(this.PlayFMButton);
            this.BottomLeftButtonsFLP.Controls.Add(this.PlayOriginalGameButton);
            this.BottomLeftButtonsFLP.Controls.Add(this.InstallUninstallFMButton);
            this.BottomLeftButtonsFLP.Controls.Add(this.ScanAllFMsButton);
            this.BottomLeftButtonsFLP.Controls.Add(this.WebSearchButton);
            this.BottomLeftButtonsFLP.Location = new System.Drawing.Point(2, 0);
            this.BottomLeftButtonsFLP.Name = "BottomLeftButtonsFLP";
            this.BottomLeftButtonsFLP.Size = new System.Drawing.Size(621, 42);
            this.BottomLeftButtonsFLP.TabIndex = 36;
            this.BottomLeftButtonsFLP.Paint += new System.Windows.Forms.PaintEventHandler(this.BottomLeftButtonsFLP_Paint);
            // 
            // PlayFMButton
            // 
            this.PlayFMButton.AutoSize = true;
            this.PlayFMButton.Location = new System.Drawing.Point(3, 3);
            this.PlayFMButton.Name = "PlayFMButton";
            this.PlayFMButton.Padding = new System.Windows.Forms.Padding(28, 0, 6, 0);
            this.PlayFMButton.Size = new System.Drawing.Size(91, 36);
            this.PlayFMButton.TabIndex = 56;
            this.PlayFMButton.Text = "Play FM";
            this.PlayFMButton.UseVisualStyleBackColor = true;
            this.PlayFMButton.Click += new System.EventHandler(this.PlayFMButton_Click);
            this.PlayFMButton.Paint += new System.Windows.Forms.PaintEventHandler(this.PlayFMButton_Paint);
            // 
            // PlayOriginalGameButton
            // 
            this.PlayOriginalGameButton.AutoSize = true;
            this.PlayOriginalGameButton.Image = global::AngelLoader.Properties.Resources.Play_Original_24;
            this.PlayOriginalGameButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.PlayOriginalGameButton.Location = new System.Drawing.Point(100, 3);
            this.PlayOriginalGameButton.Name = "PlayOriginalGameButton";
            this.PlayOriginalGameButton.Padding = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.PlayOriginalGameButton.Size = new System.Drawing.Size(147, 36);
            this.PlayOriginalGameButton.TabIndex = 57;
            this.PlayOriginalGameButton.Text = "Play original game...";
            this.PlayOriginalGameButton.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.PlayOriginalGameButton.UseVisualStyleBackColor = true;
            this.PlayOriginalGameButton.Click += new System.EventHandler(this.PlayOriginalGameButton_Click);
            // 
            // InstallUninstallFMButton
            // 
            this.InstallUninstallFMButton.AutoSize = true;
            this.InstallUninstallFMButton.Image = global::AngelLoader.Properties.Resources.Install_24;
            this.InstallUninstallFMButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.InstallUninstallFMButton.Location = new System.Drawing.Point(253, 3);
            this.InstallUninstallFMButton.Name = "InstallUninstallFMButton";
            this.InstallUninstallFMButton.Padding = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.InstallUninstallFMButton.Size = new System.Drawing.Size(98, 36);
            this.InstallUninstallFMButton.TabIndex = 58;
            this.InstallUninstallFMButton.Text = "Install FM";
            this.InstallUninstallFMButton.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.InstallUninstallFMButton.UseVisualStyleBackColor = true;
            this.InstallUninstallFMButton.Click += new System.EventHandler(this.InstallUninstallFMButton_Click);
            // 
            // WebSearchButton
            // 
            this.WebSearchButton.AutoSize = true;
            this.WebSearchButton.Location = new System.Drawing.Point(502, 3);
            this.WebSearchButton.Margin = new System.Windows.Forms.Padding(11, 3, 3, 3);
            this.WebSearchButton.Name = "WebSearchButton";
            this.WebSearchButton.Padding = new System.Windows.Forms.Padding(33, 0, 6, 0);
            this.WebSearchButton.Size = new System.Drawing.Size(116, 36);
            this.WebSearchButton.TabIndex = 60;
            this.WebSearchButton.Text = "Web search";
            this.WebSearchButton.UseVisualStyleBackColor = true;
            this.WebSearchButton.Click += new System.EventHandler(this.WebSearchButton_Click);
            this.WebSearchButton.Paint += new System.Windows.Forms.PaintEventHandler(this.WebSearchButton_Paint);
            // 
            // DebugLabel
            // 
            this.DebugLabel.AutoSize = true;
            this.DebugLabel.Location = new System.Drawing.Point(712, 8);
            this.DebugLabel.Name = "DebugLabel";
            this.DebugLabel.Size = new System.Drawing.Size(71, 13);
            this.DebugLabel.TabIndex = 29;
            this.DebugLabel.Text = "[DebugLabel]";
            // 
            // DebugLabel2
            // 
            this.DebugLabel2.AutoSize = true;
            this.DebugLabel2.Location = new System.Drawing.Point(712, 24);
            this.DebugLabel2.Name = "DebugLabel2";
            this.DebugLabel2.Size = new System.Drawing.Size(77, 13);
            this.DebugLabel2.TabIndex = 32;
            this.DebugLabel2.Text = "[DebugLabel2]";
            // 
            // EverythingPanel
            // 
            this.EverythingPanel.Controls.Add(this.MainSplitContainer);
            this.EverythingPanel.Controls.Add(this.BottomPanel);
            this.EverythingPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.EverythingPanel.Location = new System.Drawing.Point(0, 0);
            this.EverythingPanel.Name = "EverythingPanel";
            this.EverythingPanel.Size = new System.Drawing.Size(1671, 716);
            this.EverythingPanel.TabIndex = 4;
            // 
            // MainSplitContainer
            // 
            this.MainSplitContainer.BackColor = System.Drawing.SystemColors.ActiveBorder;
            this.MainSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MainSplitContainer.Location = new System.Drawing.Point(0, 0);
            this.MainSplitContainer.MouseOverCrossSection = false;
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
            this.MainSplitContainer.Panel2.Controls.Add(this.ViewHTMLReadmeButton);
            this.MainSplitContainer.Panel2.Controls.Add(this.ReadmeFullScreenButton);
            this.MainSplitContainer.Panel2.Controls.Add(this.ZoomInButton);
            this.MainSplitContainer.Panel2.Controls.Add(this.ZoomOutButton);
            this.MainSplitContainer.Panel2.Controls.Add(this.ResetZoomButton);
            this.MainSplitContainer.Panel2.Controls.Add(this.ChooseReadmeComboBox);
            this.MainSplitContainer.Panel2.Controls.Add(this.ReadmeRichTextBox);
            this.MainSplitContainer.Panel2MinSize = 38;
            this.MainSplitContainer.Size = new System.Drawing.Size(1671, 672);
            this.MainSplitContainer.SplitterDistance = 309;
            this.MainSplitContainer.TabIndex = 0;
            // 
            // TopSplitContainer
            // 
            this.TopSplitContainer.BackColor = System.Drawing.SystemColors.ActiveBorder;
            this.TopSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.TopSplitContainer.Location = new System.Drawing.Point(0, 0);
            this.TopSplitContainer.MouseOverCrossSection = false;
            this.TopSplitContainer.Name = "TopSplitContainer";
            // 
            // TopSplitContainer.Panel1
            // 
            this.TopSplitContainer.Panel1.BackColor = System.Drawing.SystemColors.Control;
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
            this.TopSplitContainer.Panel2.Controls.Add(this.TopRightMenuButton);
            this.TopSplitContainer.Panel2.Controls.Add(this.TopRightCollapseButton);
            this.TopSplitContainer.Panel2.Controls.Add(this.TopRightTabControl);
            this.TopSplitContainer.Panel2.SizeChanged += new System.EventHandler(this.TopSplitContainer_Panel2_SizeChanged);
            this.TopSplitContainer.Size = new System.Drawing.Size(1671, 309);
            this.TopSplitContainer.SplitterDistance = 1116;
            this.TopSplitContainer.TabIndex = 0;
            // 
            // FilterBarScrollRightButton
            // 
            this.FilterBarScrollRightButton.ArrowDirection = AngelLoader.Common.Direction.Right;
            this.FilterBarScrollRightButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.FilterBarScrollRightButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.FilterBarScrollRightButton.Location = new System.Drawing.Point(1088, 56);
            this.FilterBarScrollRightButton.Name = "FilterBarScrollRightButton";
            this.FilterBarScrollRightButton.Size = new System.Drawing.Size(14, 24);
            this.FilterBarScrollRightButton.TabIndex = 10;
            this.FilterBarScrollRightButton.UseVisualStyleBackColor = true;
            this.FilterBarScrollRightButton.Visible = false;
            this.FilterBarScrollRightButton.EnabledChanged += new System.EventHandler(this.FilterBarScrollButtons_EnabledChanged);
            this.FilterBarScrollRightButton.VisibleChanged += new System.EventHandler(this.FilterBarScrollButtons_VisibleChanged);
            this.FilterBarScrollRightButton.Click += new System.EventHandler(this.FilterBarScrollButtons_Click);
            this.FilterBarScrollRightButton.MouseDown += new System.Windows.Forms.MouseEventHandler(this.FilterBarScrollButtons_MouseDown);
            this.FilterBarScrollRightButton.MouseUp += new System.Windows.Forms.MouseEventHandler(this.FilterBarScrollLeftButton_MouseUp);
            // 
            // FilterBarScrollLeftButton
            // 
            this.FilterBarScrollLeftButton.ArrowDirection = AngelLoader.Common.Direction.Left;
            this.FilterBarScrollLeftButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.FilterBarScrollLeftButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.FilterBarScrollLeftButton.Location = new System.Drawing.Point(1072, 56);
            this.FilterBarScrollLeftButton.Name = "FilterBarScrollLeftButton";
            this.FilterBarScrollLeftButton.Size = new System.Drawing.Size(14, 24);
            this.FilterBarScrollLeftButton.TabIndex = 2;
            this.FilterBarScrollLeftButton.UseVisualStyleBackColor = true;
            this.FilterBarScrollLeftButton.Visible = false;
            this.FilterBarScrollLeftButton.EnabledChanged += new System.EventHandler(this.FilterBarScrollButtons_EnabledChanged);
            this.FilterBarScrollLeftButton.VisibleChanged += new System.EventHandler(this.FilterBarScrollButtons_VisibleChanged);
            this.FilterBarScrollLeftButton.Click += new System.EventHandler(this.FilterBarScrollButtons_Click);
            this.FilterBarScrollLeftButton.MouseDown += new System.Windows.Forms.MouseEventHandler(this.FilterBarScrollButtons_MouseDown);
            this.FilterBarScrollLeftButton.MouseUp += new System.Windows.Forms.MouseEventHandler(this.FilterBarScrollLeftButton_MouseUp);
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
            this.TitleColumn,
            this.ArchiveColumn,
            this.AuthorColumn,
            this.SizeColumn,
            this.RatingTextColumn,
            this.FinishedColumn,
            this.ReleaseDateColumn,
            this.LastPlayedColumn,
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
            this.FMsDGV.MultiSelect = false;
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
            this.FMsDGV.CellValueNeeded += new System.Windows.Forms.DataGridViewCellValueEventHandler(this.FMsDGV_CellValueNeeded_Initial);
            this.FMsDGV.ColumnHeaderMouseClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.FMsDGV_ColumnHeaderMouseClick);
            this.FMsDGV.SelectionChanged += new System.EventHandler(this.FMsDGV_SelectionChanged);
            this.FMsDGV.KeyDown += new System.Windows.Forms.KeyEventHandler(this.FMsDGV_KeyDown);
            this.FMsDGV.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.FMsDGV_KeyPress);
            this.FMsDGV.MouseDown += new System.Windows.Forms.MouseEventHandler(this.FMsDGV_MouseDown);
            // 
            // GameTypeColumn
            // 
            this.GameTypeColumn.HeaderText = "Game";
            this.GameTypeColumn.ImageLayout = System.Windows.Forms.DataGridViewImageCellLayout.Zoom;
            this.GameTypeColumn.MinimumWidth = 25;
            this.GameTypeColumn.Name = "GameTypeColumn";
            this.GameTypeColumn.ReadOnly = true;
            this.GameTypeColumn.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.GameTypeColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Programmatic;
            // 
            // InstalledColumn
            // 
            this.InstalledColumn.HeaderText = "Installed";
            this.InstalledColumn.ImageLayout = System.Windows.Forms.DataGridViewImageCellLayout.Zoom;
            this.InstalledColumn.MinimumWidth = 25;
            this.InstalledColumn.Name = "InstalledColumn";
            this.InstalledColumn.ReadOnly = true;
            this.InstalledColumn.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.InstalledColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Programmatic;
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
            this.FilterBarFLP.Scroll += new System.Windows.Forms.ScrollEventHandler(this.FiltersFlowLayoutPanel_Scroll);
            this.FilterBarFLP.SizeChanged += new System.EventHandler(this.FiltersFlowLayoutPanel_SizeChanged);
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
            this.FilterGameButtonsToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.FilterByThief1Button,
            this.FilterByThief2Button,
            this.FilterByThief3Button});
            this.FilterGameButtonsToolStrip.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.Flow;
            this.FilterGameButtonsToolStrip.Location = new System.Drawing.Point(0, 0);
            this.FilterGameButtonsToolStrip.Name = "FilterGameButtonsToolStrip";
            this.FilterGameButtonsToolStrip.PaddingDrawNudge = 0;
            this.FilterGameButtonsToolStrip.Size = new System.Drawing.Size(76, 26);
            this.FilterGameButtonsToolStrip.TabIndex = 3;
            // 
            // FilterByThief1Button
            // 
            this.FilterByThief1Button.AutoSize = false;
            this.FilterByThief1Button.BackColor = System.Drawing.SystemColors.Control;
            this.FilterByThief1Button.CheckOnClick = true;
            this.FilterByThief1Button.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.FilterByThief1Button.Image = global::AngelLoader.Properties.Resources.Thief1_21;
            this.FilterByThief1Button.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.FilterByThief1Button.Margin = new System.Windows.Forms.Padding(0);
            this.FilterByThief1Button.Name = "FilterByThief1Button";
            this.FilterByThief1Button.Size = new System.Drawing.Size(25, 25);
            this.FilterByThief1Button.ToolTipText = "Thief 1";
            this.FilterByThief1Button.Click += new System.EventHandler(this.FilterByGameCheckButtons_Click);
            // 
            // FilterByThief2Button
            // 
            this.FilterByThief2Button.AutoSize = false;
            this.FilterByThief2Button.CheckOnClick = true;
            this.FilterByThief2Button.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.FilterByThief2Button.Image = global::AngelLoader.Properties.Resources.Thief2_21;
            this.FilterByThief2Button.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.FilterByThief2Button.Margin = new System.Windows.Forms.Padding(0);
            this.FilterByThief2Button.Name = "FilterByThief2Button";
            this.FilterByThief2Button.Size = new System.Drawing.Size(25, 25);
            this.FilterByThief2Button.ToolTipText = "Thief 2";
            this.FilterByThief2Button.Click += new System.EventHandler(this.FilterByGameCheckButtons_Click);
            // 
            // FilterByThief3Button
            // 
            this.FilterByThief3Button.AutoSize = false;
            this.FilterByThief3Button.CheckOnClick = true;
            this.FilterByThief3Button.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.FilterByThief3Button.Image = global::AngelLoader.Properties.Resources.Thief3_21;
            this.FilterByThief3Button.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.FilterByThief3Button.Margin = new System.Windows.Forms.Padding(0);
            this.FilterByThief3Button.Name = "FilterByThief3Button";
            this.FilterByThief3Button.Size = new System.Drawing.Size(25, 25);
            this.FilterByThief3Button.ToolTipText = "Thief 3";
            this.FilterByThief3Button.Click += new System.EventHandler(this.FilterByGameCheckButtons_Click);
            // 
            // FilterTitleLabel
            // 
            this.FilterTitleLabel.AutoSize = true;
            this.FilterTitleLabel.Location = new System.Drawing.Point(86, 6);
            this.FilterTitleLabel.Margin = new System.Windows.Forms.Padding(10, 6, 0, 0);
            this.FilterTitleLabel.Name = "FilterTitleLabel";
            this.FilterTitleLabel.Size = new System.Drawing.Size(30, 13);
            this.FilterTitleLabel.TabIndex = 5;
            this.FilterTitleLabel.Text = "Title:";
            // 
            // FilterTitleTextBox
            // 
            this.FilterTitleTextBox.DisallowedCharacters = "";
            this.FilterTitleTextBox.Location = new System.Drawing.Point(119, 3);
            this.FilterTitleTextBox.Name = "FilterTitleTextBox";
            this.FilterTitleTextBox.Size = new System.Drawing.Size(144, 20);
            this.FilterTitleTextBox.TabIndex = 6;
            this.FilterTitleTextBox.TextChanged += new System.EventHandler(this.FilterTextBoxes_TextChanged);
            // 
            // FilterAuthorLabel
            // 
            this.FilterAuthorLabel.AutoSize = true;
            this.FilterAuthorLabel.Location = new System.Drawing.Point(275, 6);
            this.FilterAuthorLabel.Margin = new System.Windows.Forms.Padding(9, 6, 0, 0);
            this.FilterAuthorLabel.Name = "FilterAuthorLabel";
            this.FilterAuthorLabel.Size = new System.Drawing.Size(41, 13);
            this.FilterAuthorLabel.TabIndex = 7;
            this.FilterAuthorLabel.Text = "Author:";
            // 
            // FilterAuthorTextBox
            // 
            this.FilterAuthorTextBox.DisallowedCharacters = "";
            this.FilterAuthorTextBox.Location = new System.Drawing.Point(319, 3);
            this.FilterAuthorTextBox.Name = "FilterAuthorTextBox";
            this.FilterAuthorTextBox.Size = new System.Drawing.Size(144, 20);
            this.FilterAuthorTextBox.TabIndex = 8;
            this.FilterAuthorTextBox.TextChanged += new System.EventHandler(this.FilterTextBoxes_TextChanged);
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
            this.FilterByReleaseDateLabel,
            this.FilterByLastPlayedButton,
            this.FilterByLastPlayedLabel,
            this.FilterByTagsButton,
            this.FilterByFinishedButton,
            this.FilterByUnfinishedButton,
            this.FilterByRatingButton,
            this.FilterByRatingLabel,
            this.FilterShowUnsupportedButton});
            this.FilterIconButtonsToolStrip.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.Flow;
            this.FilterIconButtonsToolStrip.Location = new System.Drawing.Point(466, 0);
            this.FilterIconButtonsToolStrip.Name = "FilterIconButtonsToolStrip";
            this.FilterIconButtonsToolStrip.PaddingDrawNudge = 0;
            this.FilterIconButtonsToolStrip.Size = new System.Drawing.Size(294, 26);
            this.FilterIconButtonsToolStrip.TabIndex = 3;
            this.FilterIconButtonsToolStrip.Paint += new System.Windows.Forms.PaintEventHandler(this.FilterIconButtonsToolStrip_Paint);
            // 
            // FilterByReleaseDateButton
            // 
            this.FilterByReleaseDateButton.AutoSize = false;
            this.FilterByReleaseDateButton.BackColor = System.Drawing.SystemColors.Control;
            this.FilterByReleaseDateButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.FilterByReleaseDateButton.Image = global::AngelLoader.Properties.Resources.FilterByReleaseDate;
            this.FilterByReleaseDateButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.FilterByReleaseDateButton.Margin = new System.Windows.Forms.Padding(6, 0, 0, 0);
            this.FilterByReleaseDateButton.Name = "FilterByReleaseDateButton";
            this.FilterByReleaseDateButton.Size = new System.Drawing.Size(25, 25);
            this.FilterByReleaseDateButton.ToolTipText = "Release date";
            this.FilterByReleaseDateButton.Click += new System.EventHandler(this.FilterByReleaseDateButton_Click);
            // 
            // FilterByReleaseDateLabel
            // 
            this.FilterByReleaseDateLabel.ForeColor = System.Drawing.Color.Maroon;
            this.FilterByReleaseDateLabel.Margin = new System.Windows.Forms.Padding(4, 5, 0, 2);
            this.FilterByReleaseDateLabel.Name = "FilterByReleaseDateLabel";
            this.FilterByReleaseDateLabel.Size = new System.Drawing.Size(26, 15);
            this.FilterByReleaseDateLabel.Text = "[rd]";
            this.FilterByReleaseDateLabel.ToolTipText = "Release date";
            // 
            // FilterByLastPlayedButton
            // 
            this.FilterByLastPlayedButton.AutoSize = false;
            this.FilterByLastPlayedButton.BackColor = System.Drawing.SystemColors.Control;
            this.FilterByLastPlayedButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.FilterByLastPlayedButton.Image = global::AngelLoader.Properties.Resources.FilterByLastPlayed;
            this.FilterByLastPlayedButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.FilterByLastPlayedButton.Margin = new System.Windows.Forms.Padding(6, 0, 0, 0);
            this.FilterByLastPlayedButton.Name = "FilterByLastPlayedButton";
            this.FilterByLastPlayedButton.Size = new System.Drawing.Size(25, 25);
            this.FilterByLastPlayedButton.ToolTipText = "Last played";
            this.FilterByLastPlayedButton.Click += new System.EventHandler(this.FilterByLastPlayedButton_Click);
            // 
            // FilterByLastPlayedLabel
            // 
            this.FilterByLastPlayedLabel.ForeColor = System.Drawing.Color.Maroon;
            this.FilterByLastPlayedLabel.Margin = new System.Windows.Forms.Padding(4, 5, 0, 2);
            this.FilterByLastPlayedLabel.Name = "FilterByLastPlayedLabel";
            this.FilterByLastPlayedLabel.Size = new System.Drawing.Size(25, 15);
            this.FilterByLastPlayedLabel.Text = "[lp]";
            this.FilterByLastPlayedLabel.ToolTipText = "Release date";
            // 
            // FilterByTagsButton
            // 
            this.FilterByTagsButton.AutoSize = false;
            this.FilterByTagsButton.BackColor = System.Drawing.SystemColors.Control;
            this.FilterByTagsButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.FilterByTagsButton.Image = global::AngelLoader.Properties.Resources.FilterByTags;
            this.FilterByTagsButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.FilterByTagsButton.Margin = new System.Windows.Forms.Padding(6, 0, 0, 0);
            this.FilterByTagsButton.Name = "FilterByTagsButton";
            this.FilterByTagsButton.Size = new System.Drawing.Size(25, 25);
            this.FilterByTagsButton.ToolTipText = "Tags";
            this.FilterByTagsButton.Click += new System.EventHandler(this.FilterByTagsButton_Click);
            // 
            // FilterByFinishedButton
            // 
            this.FilterByFinishedButton.AutoSize = false;
            this.FilterByFinishedButton.BackColor = System.Drawing.SystemColors.Control;
            this.FilterByFinishedButton.CheckOnClick = true;
            this.FilterByFinishedButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.FilterByFinishedButton.Image = global::AngelLoader.Properties.Resources.FilterByFinished;
            this.FilterByFinishedButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.FilterByFinishedButton.Margin = new System.Windows.Forms.Padding(6, 0, 0, 0);
            this.FilterByFinishedButton.Name = "FilterByFinishedButton";
            this.FilterByFinishedButton.Size = new System.Drawing.Size(25, 25);
            this.FilterByFinishedButton.ToolTipText = "Finished";
            this.FilterByFinishedButton.Click += new System.EventHandler(this.FilterByFinishedButton_Click);
            // 
            // FilterByUnfinishedButton
            // 
            this.FilterByUnfinishedButton.AutoSize = false;
            this.FilterByUnfinishedButton.BackColor = System.Drawing.SystemColors.Control;
            this.FilterByUnfinishedButton.CheckOnClick = true;
            this.FilterByUnfinishedButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.FilterByUnfinishedButton.Image = global::AngelLoader.Properties.Resources.FilterByUnfinished;
            this.FilterByUnfinishedButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.FilterByUnfinishedButton.Margin = new System.Windows.Forms.Padding(0);
            this.FilterByUnfinishedButton.Name = "FilterByUnfinishedButton";
            this.FilterByUnfinishedButton.Size = new System.Drawing.Size(25, 25);
            this.FilterByUnfinishedButton.ToolTipText = "Unfinished";
            this.FilterByUnfinishedButton.Click += new System.EventHandler(this.FilterByUnfinishedButton_Click);
            // 
            // FilterByRatingButton
            // 
            this.FilterByRatingButton.AutoSize = false;
            this.FilterByRatingButton.BackColor = System.Drawing.SystemColors.Control;
            this.FilterByRatingButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.FilterByRatingButton.Image = global::AngelLoader.Properties.Resources.FilterByRating;
            this.FilterByRatingButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.FilterByRatingButton.Margin = new System.Windows.Forms.Padding(6, 0, 0, 0);
            this.FilterByRatingButton.Name = "FilterByRatingButton";
            this.FilterByRatingButton.Size = new System.Drawing.Size(25, 25);
            this.FilterByRatingButton.ToolTipText = "Rating";
            this.FilterByRatingButton.Click += new System.EventHandler(this.FilterByRatingButton_Click);
            // 
            // FilterByRatingLabel
            // 
            this.FilterByRatingLabel.ForeColor = System.Drawing.Color.Maroon;
            this.FilterByRatingLabel.Margin = new System.Windows.Forms.Padding(4, 5, 0, 2);
            this.FilterByRatingLabel.Name = "FilterByRatingLabel";
            this.FilterByRatingLabel.Size = new System.Drawing.Size(19, 15);
            this.FilterByRatingLabel.Text = "[r]";
            this.FilterByRatingLabel.ToolTipText = "Rating";
            // 
            // FilterShowUnsupportedButton
            // 
            this.FilterShowUnsupportedButton.AutoSize = false;
            this.FilterShowUnsupportedButton.BackColor = System.Drawing.SystemColors.Control;
            this.FilterShowUnsupportedButton.CheckOnClick = true;
            this.FilterShowUnsupportedButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.FilterShowUnsupportedButton.Image = global::AngelLoader.Properties.Resources.Show_Unsupported;
            this.FilterShowUnsupportedButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.FilterShowUnsupportedButton.Margin = new System.Windows.Forms.Padding(6, 0, 0, 0);
            this.FilterShowUnsupportedButton.Name = "FilterShowUnsupportedButton";
            this.FilterShowUnsupportedButton.Size = new System.Drawing.Size(25, 25);
            this.FilterShowUnsupportedButton.ToolTipText = "Unfinished";
            this.FilterShowUnsupportedButton.Click += new System.EventHandler(this.FilterShowUnsupportedButton_Click);
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
            this.FMsListZoomInButton,
            this.FMsListZoomOutButton,
            this.FMsListResetZoomButton,
            this.RefreshFromDiskButton,
            this.RefreshFiltersButton,
            this.ClearFiltersButton});
            this.RefreshAreaToolStrip.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.Flow;
            this.RefreshAreaToolStrip.Location = new System.Drawing.Point(919, 0);
            this.RefreshAreaToolStrip.Name = "RefreshAreaToolStrip";
            this.RefreshAreaToolStrip.PaddingDrawNudge = 0;
            this.RefreshAreaToolStrip.Size = new System.Drawing.Size(166, 26);
            this.RefreshAreaToolStrip.TabIndex = 12;
            this.RefreshAreaToolStrip.Paint += new System.Windows.Forms.PaintEventHandler(this.RefreshAreaToolStrip_Paint);
            // 
            // FMsListZoomInButton
            // 
            this.FMsListZoomInButton.AutoSize = false;
            this.FMsListZoomInButton.BackColor = System.Drawing.SystemColors.Control;
            this.FMsListZoomInButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.FMsListZoomInButton.Image = global::AngelLoader.Properties.Resources.ZoomIn;
            this.FMsListZoomInButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.FMsListZoomInButton.Margin = new System.Windows.Forms.Padding(0);
            this.FMsListZoomInButton.Name = "FMsListZoomInButton";
            this.FMsListZoomInButton.Size = new System.Drawing.Size(25, 25);
            this.FMsListZoomInButton.Click += new System.EventHandler(this.FMsListZoomInButton_Click);
            // 
            // FMsListZoomOutButton
            // 
            this.FMsListZoomOutButton.AutoSize = false;
            this.FMsListZoomOutButton.BackColor = System.Drawing.SystemColors.Control;
            this.FMsListZoomOutButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.FMsListZoomOutButton.Image = global::AngelLoader.Properties.Resources.ZoomOut;
            this.FMsListZoomOutButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.FMsListZoomOutButton.Margin = new System.Windows.Forms.Padding(0);
            this.FMsListZoomOutButton.Name = "FMsListZoomOutButton";
            this.FMsListZoomOutButton.Size = new System.Drawing.Size(25, 25);
            this.FMsListZoomOutButton.Click += new System.EventHandler(this.FMsListZoomOutButton_Click);
            // 
            // FMsListResetZoomButton
            // 
            this.FMsListResetZoomButton.AutoSize = false;
            this.FMsListResetZoomButton.BackColor = System.Drawing.SystemColors.Control;
            this.FMsListResetZoomButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.FMsListResetZoomButton.Image = global::AngelLoader.Properties.Resources.ZoomReset;
            this.FMsListResetZoomButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.FMsListResetZoomButton.Margin = new System.Windows.Forms.Padding(0);
            this.FMsListResetZoomButton.Name = "FMsListResetZoomButton";
            this.FMsListResetZoomButton.Size = new System.Drawing.Size(25, 25);
            this.FMsListResetZoomButton.Click += new System.EventHandler(this.FMsListResetZoomButton_Click);
            // 
            // RefreshFromDiskButton
            // 
            this.RefreshFromDiskButton.AutoSize = false;
            this.RefreshFromDiskButton.BackColor = System.Drawing.SystemColors.Control;
            this.RefreshFromDiskButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.RefreshFromDiskButton.Image = global::AngelLoader.Properties.Resources.FindNewFMs_21;
            this.RefreshFromDiskButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.RefreshFromDiskButton.Margin = new System.Windows.Forms.Padding(6, 0, 0, 0);
            this.RefreshFromDiskButton.Name = "RefreshFromDiskButton";
            this.RefreshFromDiskButton.Size = new System.Drawing.Size(25, 25);
            this.RefreshFromDiskButton.ToolTipText = "Refresh from disk";
            this.RefreshFromDiskButton.Click += new System.EventHandler(this.RefreshFromDiskButton_Click);
            // 
            // RefreshFiltersButton
            // 
            this.RefreshFiltersButton.AutoSize = false;
            this.RefreshFiltersButton.BackColor = System.Drawing.SystemColors.Control;
            this.RefreshFiltersButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.RefreshFiltersButton.Image = global::AngelLoader.Properties.Resources.Refresh;
            this.RefreshFiltersButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.RefreshFiltersButton.Margin = new System.Windows.Forms.Padding(0);
            this.RefreshFiltersButton.Name = "RefreshFiltersButton";
            this.RefreshFiltersButton.Size = new System.Drawing.Size(25, 25);
            this.RefreshFiltersButton.ToolTipText = "Refresh filtered list";
            this.RefreshFiltersButton.Click += new System.EventHandler(this.RefreshFiltersButton_Click);
            // 
            // ClearFiltersButton
            // 
            this.ClearFiltersButton.AutoSize = false;
            this.ClearFiltersButton.BackColor = System.Drawing.SystemColors.Control;
            this.ClearFiltersButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.ClearFiltersButton.Image = global::AngelLoader.Properties.Resources.ClearFilters;
            this.ClearFiltersButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.ClearFiltersButton.Margin = new System.Windows.Forms.Padding(0, 0, 9, 1);
            this.ClearFiltersButton.Name = "ClearFiltersButton";
            this.ClearFiltersButton.Size = new System.Drawing.Size(25, 25);
            this.ClearFiltersButton.ToolTipText = "Clear filters";
            this.ClearFiltersButton.Click += new System.EventHandler(this.ClearFiltersButton_Click);
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
            this.ResetLayoutButton.UseVisualStyleBackColor = true;
            this.ResetLayoutButton.Click += new System.EventHandler(this.ResetLayoutButton_Click);
            this.ResetLayoutButton.Paint += new System.Windows.Forms.PaintEventHandler(this.ResetLayoutButton_Paint);
            // 
            // GamesTabControl
            // 
            this.GamesTabControl.Controls.Add(this.Thief1TabPage);
            this.GamesTabControl.Controls.Add(this.Thief2TabPage);
            this.GamesTabControl.Controls.Add(this.Thief3TabPage);
            this.GamesTabControl.ImageList = this.GameTabsImageList;
            this.GamesTabControl.Location = new System.Drawing.Point(1, 5);
            this.GamesTabControl.Name = "GamesTabControl";
            this.GamesTabControl.SelectedIndex = 0;
            this.GamesTabControl.Size = new System.Drawing.Size(1103, 24);
            this.GamesTabControl.TabIndex = 1;
            this.GamesTabControl.SelectedIndexChanged += new System.EventHandler(this.GamesTabControl_SelectedIndexChanged);
            this.GamesTabControl.Deselecting += new System.Windows.Forms.TabControlCancelEventHandler(this.GamesTabControl_Deselecting);
            // 
            // Thief1TabPage
            // 
            this.Thief1TabPage.BackColor = System.Drawing.SystemColors.Control;
            this.Thief1TabPage.ImageIndex = 0;
            this.Thief1TabPage.Location = new System.Drawing.Point(4, 23);
            this.Thief1TabPage.Name = "Thief1TabPage";
            this.Thief1TabPage.Padding = new System.Windows.Forms.Padding(3);
            this.Thief1TabPage.Size = new System.Drawing.Size(1095, 0);
            this.Thief1TabPage.TabIndex = 0;
            this.Thief1TabPage.Text = "Thief 1";
            // 
            // Thief2TabPage
            // 
            this.Thief2TabPage.ImageIndex = 1;
            this.Thief2TabPage.Location = new System.Drawing.Point(4, 23);
            this.Thief2TabPage.Name = "Thief2TabPage";
            this.Thief2TabPage.Padding = new System.Windows.Forms.Padding(3);
            this.Thief2TabPage.Size = new System.Drawing.Size(1095, 0);
            this.Thief2TabPage.TabIndex = 1;
            this.Thief2TabPage.Text = "Thief 2";
            this.Thief2TabPage.UseVisualStyleBackColor = true;
            // 
            // Thief3TabPage
            // 
            this.Thief3TabPage.ImageIndex = 2;
            this.Thief3TabPage.Location = new System.Drawing.Point(4, 23);
            this.Thief3TabPage.Name = "Thief3TabPage";
            this.Thief3TabPage.Padding = new System.Windows.Forms.Padding(3);
            this.Thief3TabPage.Size = new System.Drawing.Size(1095, 0);
            this.Thief3TabPage.TabIndex = 2;
            this.Thief3TabPage.Text = "Thief 3";
            this.Thief3TabPage.UseVisualStyleBackColor = true;
            // 
            // TopRightMenuButton
            // 
            this.TopRightMenuButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.TopRightMenuButton.FlatAppearance.BorderSize = 0;
            this.TopRightMenuButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.TopRightMenuButton.Location = new System.Drawing.Point(534, 2);
            this.TopRightMenuButton.Name = "TopRightMenuButton";
            this.TopRightMenuButton.Size = new System.Drawing.Size(16, 16);
            this.TopRightMenuButton.TabIndex = 13;
            this.TopRightMenuButton.UseVisualStyleBackColor = true;
            this.TopRightMenuButton.Click += new System.EventHandler(this.TopRightMenuButton_Click);
            this.TopRightMenuButton.Paint += new System.Windows.Forms.PaintEventHandler(this.TopRightMenuButton_Paint);
            // 
            // TopRightCollapseButton
            // 
            this.TopRightCollapseButton.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.TopRightCollapseButton.ArrowDirection = AngelLoader.Common.Direction.Right;
            this.TopRightCollapseButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.TopRightCollapseButton.FlatAppearance.BorderSize = 0;
            this.TopRightCollapseButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.TopRightCollapseButton.Location = new System.Drawing.Point(533, 20);
            this.TopRightCollapseButton.Name = "TopRightCollapseButton";
            this.TopRightCollapseButton.Size = new System.Drawing.Size(18, 287);
            this.TopRightCollapseButton.TabIndex = 14;
            this.TopRightCollapseButton.UseVisualStyleBackColor = true;
            this.TopRightCollapseButton.Click += new System.EventHandler(this.TopRightCollapseButton_Click);
            // 
            // TopRightTabControl
            // 
            this.TopRightTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.TopRightTabControl.Controls.Add(this.StatisticsTabPage);
            this.TopRightTabControl.Controls.Add(this.EditFMTabPage);
            this.TopRightTabControl.Controls.Add(this.CommentTabPage);
            this.TopRightTabControl.Controls.Add(this.TagsTabPage);
            this.TopRightTabControl.Controls.Add(this.PatchTabPage);
            this.TopRightTabControl.Location = new System.Drawing.Point(0, 0);
            this.TopRightTabControl.Name = "TopRightTabControl";
            this.TopRightTabControl.SelectedIndex = 0;
            this.TopRightTabControl.Size = new System.Drawing.Size(534, 310);
            this.TopRightTabControl.TabIndex = 15;
            // 
            // StatisticsTabPage
            // 
            this.StatisticsTabPage.AutoScroll = true;
            this.StatisticsTabPage.BackColor = System.Drawing.SystemColors.Control;
            this.StatisticsTabPage.Controls.Add(this.StatsScanCustomResourcesButton);
            this.StatisticsTabPage.Controls.Add(this.StatsCheckBoxesPanel);
            this.StatisticsTabPage.Controls.Add(this.CustomResourcesLabel);
            this.StatisticsTabPage.Location = new System.Drawing.Point(4, 22);
            this.StatisticsTabPage.Name = "StatisticsTabPage";
            this.StatisticsTabPage.Size = new System.Drawing.Size(526, 284);
            this.StatisticsTabPage.TabIndex = 0;
            this.StatisticsTabPage.Text = "Statistics";
            // 
            // StatsScanCustomResourcesButton
            // 
            this.StatsScanCustomResourcesButton.AutoSize = true;
            this.StatsScanCustomResourcesButton.Image = global::AngelLoader.Properties.Resources.ScanSmall;
            this.StatsScanCustomResourcesButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.StatsScanCustomResourcesButton.Location = new System.Drawing.Point(6, 200);
            this.StatsScanCustomResourcesButton.Name = "StatsScanCustomResourcesButton";
            this.StatsScanCustomResourcesButton.Size = new System.Drawing.Size(154, 23);
            this.StatsScanCustomResourcesButton.TabIndex = 12;
            this.StatsScanCustomResourcesButton.Text = "Rescan custom resources";
            this.StatsScanCustomResourcesButton.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.StatsScanCustomResourcesButton.UseVisualStyleBackColor = true;
            this.StatsScanCustomResourcesButton.Click += new System.EventHandler(this.RescanCustomResourcesButton_Click);
            // 
            // StatsCheckBoxesPanel
            // 
            this.StatsCheckBoxesPanel.AutoSize = true;
            this.StatsCheckBoxesPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.StatsCheckBoxesPanel.Controls.Add(this.CR_MapCheckBox);
            this.StatsCheckBoxesPanel.Controls.Add(this.CR_MoviesCheckBox);
            this.StatsCheckBoxesPanel.Controls.Add(this.CR_MotionsCheckBox);
            this.StatsCheckBoxesPanel.Controls.Add(this.CR_SoundsCheckBox);
            this.StatsCheckBoxesPanel.Controls.Add(this.CR_CreaturesCheckBox);
            this.StatsCheckBoxesPanel.Controls.Add(this.CR_TexturesCheckBox);
            this.StatsCheckBoxesPanel.Controls.Add(this.CR_AutomapCheckBox);
            this.StatsCheckBoxesPanel.Controls.Add(this.CR_ScriptsCheckBox);
            this.StatsCheckBoxesPanel.Controls.Add(this.CR_SubtitlesCheckBox);
            this.StatsCheckBoxesPanel.Controls.Add(this.CR_ObjectsCheckBox);
            this.StatsCheckBoxesPanel.Location = new System.Drawing.Point(8, 32);
            this.StatsCheckBoxesPanel.Name = "StatsCheckBoxesPanel";
            this.StatsCheckBoxesPanel.Size = new System.Drawing.Size(74, 164);
            this.StatsCheckBoxesPanel.TabIndex = 1;
            this.StatsCheckBoxesPanel.Visible = false;
            // 
            // CR_MapCheckBox
            // 
            this.CR_MapCheckBox.AutoCheck = false;
            this.CR_MapCheckBox.AutoSize = true;
            this.CR_MapCheckBox.Location = new System.Drawing.Point(0, 0);
            this.CR_MapCheckBox.Name = "CR_MapCheckBox";
            this.CR_MapCheckBox.Size = new System.Drawing.Size(47, 17);
            this.CR_MapCheckBox.TabIndex = 2;
            this.CR_MapCheckBox.Text = "Map";
            this.CR_MapCheckBox.UseVisualStyleBackColor = true;
            // 
            // CR_MoviesCheckBox
            // 
            this.CR_MoviesCheckBox.AutoCheck = false;
            this.CR_MoviesCheckBox.AutoSize = true;
            this.CR_MoviesCheckBox.Location = new System.Drawing.Point(0, 64);
            this.CR_MoviesCheckBox.Name = "CR_MoviesCheckBox";
            this.CR_MoviesCheckBox.Size = new System.Drawing.Size(60, 17);
            this.CR_MoviesCheckBox.TabIndex = 6;
            this.CR_MoviesCheckBox.Text = "Movies";
            this.CR_MoviesCheckBox.UseVisualStyleBackColor = true;
            // 
            // CR_MotionsCheckBox
            // 
            this.CR_MotionsCheckBox.AutoCheck = false;
            this.CR_MotionsCheckBox.AutoSize = true;
            this.CR_MotionsCheckBox.Location = new System.Drawing.Point(0, 112);
            this.CR_MotionsCheckBox.Name = "CR_MotionsCheckBox";
            this.CR_MotionsCheckBox.Size = new System.Drawing.Size(63, 17);
            this.CR_MotionsCheckBox.TabIndex = 9;
            this.CR_MotionsCheckBox.Text = "Motions";
            this.CR_MotionsCheckBox.UseVisualStyleBackColor = true;
            // 
            // CR_SoundsCheckBox
            // 
            this.CR_SoundsCheckBox.AutoCheck = false;
            this.CR_SoundsCheckBox.AutoSize = true;
            this.CR_SoundsCheckBox.Location = new System.Drawing.Point(0, 48);
            this.CR_SoundsCheckBox.Name = "CR_SoundsCheckBox";
            this.CR_SoundsCheckBox.Size = new System.Drawing.Size(62, 17);
            this.CR_SoundsCheckBox.TabIndex = 5;
            this.CR_SoundsCheckBox.Text = "Sounds";
            this.CR_SoundsCheckBox.UseVisualStyleBackColor = true;
            // 
            // CR_CreaturesCheckBox
            // 
            this.CR_CreaturesCheckBox.AutoCheck = false;
            this.CR_CreaturesCheckBox.AutoSize = true;
            this.CR_CreaturesCheckBox.Location = new System.Drawing.Point(0, 96);
            this.CR_CreaturesCheckBox.Name = "CR_CreaturesCheckBox";
            this.CR_CreaturesCheckBox.Size = new System.Drawing.Size(71, 17);
            this.CR_CreaturesCheckBox.TabIndex = 8;
            this.CR_CreaturesCheckBox.Text = "Creatures";
            this.CR_CreaturesCheckBox.UseVisualStyleBackColor = true;
            // 
            // CR_TexturesCheckBox
            // 
            this.CR_TexturesCheckBox.AutoCheck = false;
            this.CR_TexturesCheckBox.AutoSize = true;
            this.CR_TexturesCheckBox.Location = new System.Drawing.Point(0, 32);
            this.CR_TexturesCheckBox.Name = "CR_TexturesCheckBox";
            this.CR_TexturesCheckBox.Size = new System.Drawing.Size(67, 17);
            this.CR_TexturesCheckBox.TabIndex = 4;
            this.CR_TexturesCheckBox.Text = "Textures";
            this.CR_TexturesCheckBox.UseVisualStyleBackColor = true;
            // 
            // CR_AutomapCheckBox
            // 
            this.CR_AutomapCheckBox.AutoCheck = false;
            this.CR_AutomapCheckBox.AutoSize = true;
            this.CR_AutomapCheckBox.Location = new System.Drawing.Point(0, 16);
            this.CR_AutomapCheckBox.Name = "CR_AutomapCheckBox";
            this.CR_AutomapCheckBox.Size = new System.Drawing.Size(68, 17);
            this.CR_AutomapCheckBox.TabIndex = 3;
            this.CR_AutomapCheckBox.Text = "Automap";
            this.CR_AutomapCheckBox.UseVisualStyleBackColor = true;
            // 
            // CR_ScriptsCheckBox
            // 
            this.CR_ScriptsCheckBox.AutoCheck = false;
            this.CR_ScriptsCheckBox.AutoSize = true;
            this.CR_ScriptsCheckBox.Location = new System.Drawing.Point(0, 128);
            this.CR_ScriptsCheckBox.Name = "CR_ScriptsCheckBox";
            this.CR_ScriptsCheckBox.Size = new System.Drawing.Size(58, 17);
            this.CR_ScriptsCheckBox.TabIndex = 10;
            this.CR_ScriptsCheckBox.Text = "Scripts";
            this.CR_ScriptsCheckBox.UseVisualStyleBackColor = true;
            // 
            // CR_SubtitlesCheckBox
            // 
            this.CR_SubtitlesCheckBox.AutoCheck = false;
            this.CR_SubtitlesCheckBox.AutoSize = true;
            this.CR_SubtitlesCheckBox.Location = new System.Drawing.Point(0, 144);
            this.CR_SubtitlesCheckBox.Name = "CR_SubtitlesCheckBox";
            this.CR_SubtitlesCheckBox.Size = new System.Drawing.Size(66, 17);
            this.CR_SubtitlesCheckBox.TabIndex = 11;
            this.CR_SubtitlesCheckBox.Text = "Subtitles";
            this.CR_SubtitlesCheckBox.UseVisualStyleBackColor = true;
            // 
            // CR_ObjectsCheckBox
            // 
            this.CR_ObjectsCheckBox.AutoCheck = false;
            this.CR_ObjectsCheckBox.AutoSize = true;
            this.CR_ObjectsCheckBox.Location = new System.Drawing.Point(0, 80);
            this.CR_ObjectsCheckBox.Name = "CR_ObjectsCheckBox";
            this.CR_ObjectsCheckBox.Size = new System.Drawing.Size(62, 17);
            this.CR_ObjectsCheckBox.TabIndex = 7;
            this.CR_ObjectsCheckBox.Text = "Objects";
            this.CR_ObjectsCheckBox.UseVisualStyleBackColor = true;
            // 
            // CustomResourcesLabel
            // 
            this.CustomResourcesLabel.AutoSize = true;
            this.CustomResourcesLabel.Location = new System.Drawing.Point(4, 10);
            this.CustomResourcesLabel.Name = "CustomResourcesLabel";
            this.CustomResourcesLabel.Size = new System.Drawing.Size(156, 13);
            this.CustomResourcesLabel.TabIndex = 0;
            this.CustomResourcesLabel.Text = "Custom resources not scanned.";
            // 
            // EditFMTabPage
            // 
            this.EditFMTabPage.AutoScroll = true;
            this.EditFMTabPage.BackColor = System.Drawing.SystemColors.Control;
            this.EditFMTabPage.Controls.Add(this.EditFMScanForReadmesButton);
            this.EditFMTabPage.Controls.Add(this.EditFMScanReleaseDateButton);
            this.EditFMTabPage.Controls.Add(this.EditFMScanAuthorButton);
            this.EditFMTabPage.Controls.Add(this.EditFMScanTitleButton);
            this.EditFMTabPage.Controls.Add(this.EditFMAltTitlesArrowButton);
            this.EditFMTabPage.Controls.Add(this.EditFMTitleTextBox);
            this.EditFMTabPage.Controls.Add(this.EditFMFinishedOnButton);
            this.EditFMTabPage.Controls.Add(this.EditFMRatingComboBox);
            this.EditFMTabPage.Controls.Add(this.EditFMRatingLabel);
            this.EditFMTabPage.Controls.Add(this.EditFMLastPlayedDateTimePicker);
            this.EditFMTabPage.Controls.Add(this.EditFMReleaseDateDateTimePicker);
            this.EditFMTabPage.Controls.Add(this.EditFMLastPlayedCheckBox);
            this.EditFMTabPage.Controls.Add(this.EditFMReleaseDateCheckBox);
            this.EditFMTabPage.Controls.Add(this.EditFMDisableAllModsCheckBox);
            this.EditFMTabPage.Controls.Add(this.EditFMDisabledModsTextBox);
            this.EditFMTabPage.Controls.Add(this.EditFMDisabledModsLabel);
            this.EditFMTabPage.Controls.Add(this.EditFMAuthorTextBox);
            this.EditFMTabPage.Controls.Add(this.EditFMAuthorLabel);
            this.EditFMTabPage.Controls.Add(this.EditFMTitleLabel);
            this.EditFMTabPage.Location = new System.Drawing.Point(4, 22);
            this.EditFMTabPage.Name = "EditFMTabPage";
            this.EditFMTabPage.Size = new System.Drawing.Size(526, 284);
            this.EditFMTabPage.TabIndex = 2;
            this.EditFMTabPage.Text = "Edit FM";
            // 
            // EditFMScanForReadmesButton
            // 
            this.EditFMScanForReadmesButton.AutoSize = true;
            this.EditFMScanForReadmesButton.Image = global::AngelLoader.Properties.Resources.ScanSmall;
            this.EditFMScanForReadmesButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.EditFMScanForReadmesButton.Location = new System.Drawing.Point(8, 248);
            this.EditFMScanForReadmesButton.Name = "EditFMScanForReadmesButton";
            this.EditFMScanForReadmesButton.Size = new System.Drawing.Size(128, 23);
            this.EditFMScanForReadmesButton.TabIndex = 31;
            this.EditFMScanForReadmesButton.Text = "Rescan for readmes";
            this.EditFMScanForReadmesButton.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.EditFMScanForReadmesButton.UseVisualStyleBackColor = true;
            this.EditFMScanForReadmesButton.Click += new System.EventHandler(this.EditFMScanForReadmesButton_Click);
            // 
            // EditFMScanReleaseDateButton
            // 
            this.EditFMScanReleaseDateButton.BackgroundImage = global::AngelLoader.Properties.Resources.ScanSmall;
            this.EditFMScanReleaseDateButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.EditFMScanReleaseDateButton.Location = new System.Drawing.Point(136, 105);
            this.EditFMScanReleaseDateButton.Name = "EditFMScanReleaseDateButton";
            this.EditFMScanReleaseDateButton.Size = new System.Drawing.Size(22, 22);
            this.EditFMScanReleaseDateButton.TabIndex = 22;
            this.EditFMScanReleaseDateButton.UseVisualStyleBackColor = true;
            this.EditFMScanReleaseDateButton.Click += new System.EventHandler(this.EditFMScanReleaseDateButton_Click);
            // 
            // EditFMScanAuthorButton
            // 
            this.EditFMScanAuthorButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.EditFMScanAuthorButton.BackgroundImage = global::AngelLoader.Properties.Resources.ScanSmall;
            this.EditFMScanAuthorButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.EditFMScanAuthorButton.Location = new System.Drawing.Point(493, 63);
            this.EditFMScanAuthorButton.Name = "EditFMScanAuthorButton";
            this.EditFMScanAuthorButton.Size = new System.Drawing.Size(22, 22);
            this.EditFMScanAuthorButton.TabIndex = 19;
            this.EditFMScanAuthorButton.UseVisualStyleBackColor = true;
            this.EditFMScanAuthorButton.Click += new System.EventHandler(this.EditFMScanAuthorButton_Click);
            // 
            // EditFMScanTitleButton
            // 
            this.EditFMScanTitleButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.EditFMScanTitleButton.BackgroundImage = global::AngelLoader.Properties.Resources.ScanSmall;
            this.EditFMScanTitleButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.EditFMScanTitleButton.Location = new System.Drawing.Point(493, 23);
            this.EditFMScanTitleButton.Name = "EditFMScanTitleButton";
            this.EditFMScanTitleButton.Size = new System.Drawing.Size(22, 22);
            this.EditFMScanTitleButton.TabIndex = 16;
            this.EditFMScanTitleButton.UseVisualStyleBackColor = true;
            this.EditFMScanTitleButton.Click += new System.EventHandler(this.EditFMScanTitleButton_Click);
            // 
            // EditFMAltTitlesArrowButton
            // 
            this.EditFMAltTitlesArrowButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.EditFMAltTitlesArrowButton.ArrowDirection = AngelLoader.Common.Direction.Down;
            this.EditFMAltTitlesArrowButton.Location = new System.Drawing.Point(477, 23);
            this.EditFMAltTitlesArrowButton.Name = "EditFMAltTitlesArrowButton";
            this.EditFMAltTitlesArrowButton.Size = new System.Drawing.Size(17, 22);
            this.EditFMAltTitlesArrowButton.TabIndex = 15;
            this.EditFMAltTitlesArrowButton.UseVisualStyleBackColor = true;
            this.EditFMAltTitlesArrowButton.Click += new System.EventHandler(this.EditFMAltTitlesArrowButtonClick);
            // 
            // EditFMTitleTextBox
            // 
            this.EditFMTitleTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.EditFMTitleTextBox.Location = new System.Drawing.Point(8, 24);
            this.EditFMTitleTextBox.Name = "EditFMTitleTextBox";
            this.EditFMTitleTextBox.Size = new System.Drawing.Size(469, 20);
            this.EditFMTitleTextBox.TabIndex = 14;
            this.EditFMTitleTextBox.TextChanged += new System.EventHandler(this.EditFMTitleTextBox_TextChanged);
            this.EditFMTitleTextBox.Leave += new System.EventHandler(this.EditFMTitleTextBox_Leave);
            // 
            // EditFMFinishedOnButton
            // 
            this.EditFMFinishedOnButton.AutoSize = true;
            this.EditFMFinishedOnButton.Location = new System.Drawing.Point(184, 144);
            this.EditFMFinishedOnButton.Name = "EditFMFinishedOnButton";
            this.EditFMFinishedOnButton.Padding = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.EditFMFinishedOnButton.Size = new System.Drawing.Size(138, 23);
            this.EditFMFinishedOnButton.TabIndex = 27;
            this.EditFMFinishedOnButton.Text = "Finished on...";
            this.EditFMFinishedOnButton.UseVisualStyleBackColor = true;
            this.EditFMFinishedOnButton.Click += new System.EventHandler(this.EditFMFinishedOnButton_Click);
            // 
            // EditFMRatingComboBox
            // 
            this.EditFMRatingComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.EditFMRatingComboBox.FormattingEnabled = true;
            this.EditFMRatingComboBox.Items.AddRange(new object[] {
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
            this.EditFMRatingComboBox.Location = new System.Drawing.Point(185, 104);
            this.EditFMRatingComboBox.Name = "EditFMRatingComboBox";
            this.EditFMRatingComboBox.Size = new System.Drawing.Size(136, 21);
            this.EditFMRatingComboBox.TabIndex = 26;
            this.EditFMRatingComboBox.SelectedIndexChanged += new System.EventHandler(this.EditFMRatingComboBox_SelectedIndexChanged);
            // 
            // EditFMRatingLabel
            // 
            this.EditFMRatingLabel.AutoSize = true;
            this.EditFMRatingLabel.Location = new System.Drawing.Point(185, 87);
            this.EditFMRatingLabel.Name = "EditFMRatingLabel";
            this.EditFMRatingLabel.Size = new System.Drawing.Size(41, 13);
            this.EditFMRatingLabel.TabIndex = 25;
            this.EditFMRatingLabel.Text = "Rating:";
            // 
            // EditFMLastPlayedDateTimePicker
            // 
            this.EditFMLastPlayedDateTimePicker.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.EditFMLastPlayedDateTimePicker.Location = new System.Drawing.Point(8, 148);
            this.EditFMLastPlayedDateTimePicker.Name = "EditFMLastPlayedDateTimePicker";
            this.EditFMLastPlayedDateTimePicker.Size = new System.Drawing.Size(128, 20);
            this.EditFMLastPlayedDateTimePicker.TabIndex = 24;
            this.EditFMLastPlayedDateTimePicker.Visible = false;
            this.EditFMLastPlayedDateTimePicker.ValueChanged += new System.EventHandler(this.EditFMLastPlayedDateTimePicker_ValueChanged);
            // 
            // EditFMReleaseDateDateTimePicker
            // 
            this.EditFMReleaseDateDateTimePicker.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.EditFMReleaseDateDateTimePicker.Location = new System.Drawing.Point(8, 106);
            this.EditFMReleaseDateDateTimePicker.Name = "EditFMReleaseDateDateTimePicker";
            this.EditFMReleaseDateDateTimePicker.Size = new System.Drawing.Size(128, 20);
            this.EditFMReleaseDateDateTimePicker.TabIndex = 21;
            this.EditFMReleaseDateDateTimePicker.Visible = false;
            this.EditFMReleaseDateDateTimePicker.ValueChanged += new System.EventHandler(this.EditFMReleaseDateDateTimePicker_ValueChanged);
            // 
            // EditFMLastPlayedCheckBox
            // 
            this.EditFMLastPlayedCheckBox.AutoSize = true;
            this.EditFMLastPlayedCheckBox.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.EditFMLastPlayedCheckBox.Location = new System.Drawing.Point(8, 130);
            this.EditFMLastPlayedCheckBox.Name = "EditFMLastPlayedCheckBox";
            this.EditFMLastPlayedCheckBox.Size = new System.Drawing.Size(83, 17);
            this.EditFMLastPlayedCheckBox.TabIndex = 23;
            this.EditFMLastPlayedCheckBox.Text = "Last played:";
            this.EditFMLastPlayedCheckBox.UseVisualStyleBackColor = true;
            this.EditFMLastPlayedCheckBox.CheckedChanged += new System.EventHandler(this.EditFMLastPlayedCheckBox_CheckedChanged);
            // 
            // EditFMReleaseDateCheckBox
            // 
            this.EditFMReleaseDateCheckBox.AutoSize = true;
            this.EditFMReleaseDateCheckBox.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.EditFMReleaseDateCheckBox.Location = new System.Drawing.Point(8, 88);
            this.EditFMReleaseDateCheckBox.Name = "EditFMReleaseDateCheckBox";
            this.EditFMReleaseDateCheckBox.Size = new System.Drawing.Size(92, 17);
            this.EditFMReleaseDateCheckBox.TabIndex = 20;
            this.EditFMReleaseDateCheckBox.Text = "Release date:";
            this.EditFMReleaseDateCheckBox.UseVisualStyleBackColor = true;
            this.EditFMReleaseDateCheckBox.CheckedChanged += new System.EventHandler(this.EditFMReleaseDateCheckBox_CheckedChanged);
            // 
            // EditFMDisableAllModsCheckBox
            // 
            this.EditFMDisableAllModsCheckBox.AutoSize = true;
            this.EditFMDisableAllModsCheckBox.Location = new System.Drawing.Point(8, 216);
            this.EditFMDisableAllModsCheckBox.Name = "EditFMDisableAllModsCheckBox";
            this.EditFMDisableAllModsCheckBox.Size = new System.Drawing.Size(102, 17);
            this.EditFMDisableAllModsCheckBox.TabIndex = 30;
            this.EditFMDisableAllModsCheckBox.Text = "Disable all mods";
            this.EditFMDisableAllModsCheckBox.UseVisualStyleBackColor = true;
            this.EditFMDisableAllModsCheckBox.CheckedChanged += new System.EventHandler(this.EditFMDisableAllModsCheckBox_CheckedChanged);
            // 
            // EditFMDisabledModsTextBox
            // 
            this.EditFMDisabledModsTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.EditFMDisabledModsTextBox.Location = new System.Drawing.Point(8, 192);
            this.EditFMDisabledModsTextBox.Name = "EditFMDisabledModsTextBox";
            this.EditFMDisabledModsTextBox.Size = new System.Drawing.Size(502, 20);
            this.EditFMDisabledModsTextBox.TabIndex = 29;
            this.EditFMDisabledModsTextBox.TextChanged += new System.EventHandler(this.EditFMDisabledModsTextBox_TextChanged);
            this.EditFMDisabledModsTextBox.Leave += new System.EventHandler(this.EditFMDisabledModsTextBox_Leave);
            // 
            // EditFMDisabledModsLabel
            // 
            this.EditFMDisabledModsLabel.AutoSize = true;
            this.EditFMDisabledModsLabel.Location = new System.Drawing.Point(8, 176);
            this.EditFMDisabledModsLabel.Name = "EditFMDisabledModsLabel";
            this.EditFMDisabledModsLabel.Size = new System.Drawing.Size(79, 13);
            this.EditFMDisabledModsLabel.TabIndex = 28;
            this.EditFMDisabledModsLabel.Text = "Disabled mods:";
            // 
            // EditFMAuthorTextBox
            // 
            this.EditFMAuthorTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.EditFMAuthorTextBox.Location = new System.Drawing.Point(8, 64);
            this.EditFMAuthorTextBox.Name = "EditFMAuthorTextBox";
            this.EditFMAuthorTextBox.Size = new System.Drawing.Size(485, 20);
            this.EditFMAuthorTextBox.TabIndex = 18;
            this.EditFMAuthorTextBox.TextChanged += new System.EventHandler(this.EditFMAuthorTextBox_TextChanged);
            this.EditFMAuthorTextBox.Leave += new System.EventHandler(this.EditFMAuthorTextBox_Leave);
            // 
            // EditFMAuthorLabel
            // 
            this.EditFMAuthorLabel.AutoSize = true;
            this.EditFMAuthorLabel.Location = new System.Drawing.Point(8, 48);
            this.EditFMAuthorLabel.Name = "EditFMAuthorLabel";
            this.EditFMAuthorLabel.Size = new System.Drawing.Size(41, 13);
            this.EditFMAuthorLabel.TabIndex = 17;
            this.EditFMAuthorLabel.Text = "Author:";
            // 
            // EditFMTitleLabel
            // 
            this.EditFMTitleLabel.AutoSize = true;
            this.EditFMTitleLabel.Location = new System.Drawing.Point(8, 8);
            this.EditFMTitleLabel.Name = "EditFMTitleLabel";
            this.EditFMTitleLabel.Size = new System.Drawing.Size(30, 13);
            this.EditFMTitleLabel.TabIndex = 13;
            this.EditFMTitleLabel.Text = "Title:";
            // 
            // CommentTabPage
            // 
            this.CommentTabPage.BackColor = System.Drawing.SystemColors.Control;
            this.CommentTabPage.Controls.Add(this.CommentTextBox);
            this.CommentTabPage.Location = new System.Drawing.Point(4, 22);
            this.CommentTabPage.Name = "CommentTabPage";
            this.CommentTabPage.Size = new System.Drawing.Size(526, 284);
            this.CommentTabPage.TabIndex = 0;
            this.CommentTabPage.Text = "Comment";
            // 
            // CommentTextBox
            // 
            this.CommentTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.CommentTextBox.Location = new System.Drawing.Point(8, 8);
            this.CommentTextBox.Multiline = true;
            this.CommentTextBox.Name = "CommentTextBox";
            this.CommentTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.CommentTextBox.Size = new System.Drawing.Size(510, 266);
            this.CommentTextBox.TabIndex = 32;
            this.CommentTextBox.TextChanged += new System.EventHandler(this.CommentTextBox_TextChanged);
            this.CommentTextBox.Leave += new System.EventHandler(this.CommentTextBox_Leave);
            // 
            // TagsTabPage
            // 
            this.TagsTabPage.BackColor = System.Drawing.SystemColors.Control;
            this.TagsTabPage.Controls.Add(this.AddTagButton);
            this.TagsTabPage.Controls.Add(this.AddTagTextBox);
            this.TagsTabPage.Controls.Add(this.AddRemoveTagFLP);
            this.TagsTabPage.Controls.Add(this.TagsTreeView);
            this.TagsTabPage.Location = new System.Drawing.Point(4, 22);
            this.TagsTabPage.Name = "TagsTabPage";
            this.TagsTabPage.Size = new System.Drawing.Size(526, 284);
            this.TagsTabPage.TabIndex = 1;
            this.TagsTabPage.Text = "Tags";
            // 
            // AddTagButton
            // 
            this.AddTagButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.AddTagButton.AutoSize = true;
            this.AddTagButton.Location = new System.Drawing.Point(447, 7);
            this.AddTagButton.Name = "AddTagButton";
            this.AddTagButton.Padding = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.AddTagButton.Size = new System.Drawing.Size(72, 23);
            this.AddTagButton.TabIndex = 1;
            this.AddTagButton.Text = "Add tag";
            this.AddTagButton.UseVisualStyleBackColor = true;
            this.AddTagButton.Click += new System.EventHandler(this.AddTagButton_Click);
            // 
            // AddTagTextBox
            // 
            this.AddTagTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.AddTagTextBox.DisallowedCharacters = ",;";
            this.AddTagTextBox.Location = new System.Drawing.Point(8, 8);
            this.AddTagTextBox.Name = "AddTagTextBox";
            this.AddTagTextBox.Size = new System.Drawing.Size(440, 20);
            this.AddTagTextBox.TabIndex = 0;
            this.AddTagTextBox.TextChanged += new System.EventHandler(this.AddTagTextBox_TextChanged);
            this.AddTagTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.AddTagTextBoxOrListBox_KeyDown);
            this.AddTagTextBox.Leave += new System.EventHandler(this.AddTagTextBoxOrListBox_Leave);
            // 
            // AddRemoveTagFLP
            // 
            this.AddRemoveTagFLP.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.AddRemoveTagFLP.AutoSize = true;
            this.AddRemoveTagFLP.Controls.Add(this.RemoveTagButton);
            this.AddRemoveTagFLP.Controls.Add(this.AddTagFromListButton);
            this.AddRemoveTagFLP.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.AddRemoveTagFLP.Location = new System.Drawing.Point(-11, 248);
            this.AddRemoveTagFLP.Name = "AddRemoveTagFLP";
            this.AddRemoveTagFLP.Size = new System.Drawing.Size(536, 24);
            this.AddRemoveTagFLP.TabIndex = 3;
            // 
            // RemoveTagButton
            // 
            this.RemoveTagButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.RemoveTagButton.AutoSize = true;
            this.RemoveTagButton.Location = new System.Drawing.Point(386, 0);
            this.RemoveTagButton.Margin = new System.Windows.Forms.Padding(0, 0, 6, 0);
            this.RemoveTagButton.Name = "RemoveTagButton";
            this.RemoveTagButton.Padding = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.RemoveTagButton.Size = new System.Drawing.Size(144, 23);
            this.RemoveTagButton.TabIndex = 1;
            this.RemoveTagButton.Text = "Remove tag";
            this.RemoveTagButton.UseVisualStyleBackColor = true;
            this.RemoveTagButton.Click += new System.EventHandler(this.RemoveTagButton_Click);
            // 
            // AddTagFromListButton
            // 
            this.AddTagFromListButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.AddTagFromListButton.AutoSize = true;
            this.AddTagFromListButton.Location = new System.Drawing.Point(242, 0);
            this.AddTagFromListButton.Margin = new System.Windows.Forms.Padding(0);
            this.AddTagFromListButton.Name = "AddTagFromListButton";
            this.AddTagFromListButton.Padding = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.AddTagFromListButton.Size = new System.Drawing.Size(144, 23);
            this.AddTagFromListButton.TabIndex = 0;
            this.AddTagFromListButton.Text = "Add from list...";
            this.AddTagFromListButton.UseVisualStyleBackColor = true;
            this.AddTagFromListButton.Click += new System.EventHandler(this.AddTagFromListButton_Click);
            // 
            // TagsTreeView
            // 
            this.TagsTreeView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.TagsTreeView.HideSelection = false;
            this.TagsTreeView.Location = new System.Drawing.Point(8, 32);
            this.TagsTreeView.Name = "TagsTreeView";
            this.TagsTreeView.Size = new System.Drawing.Size(510, 216);
            this.TagsTreeView.TabIndex = 2;
            // 
            // PatchTabPage
            // 
            this.PatchTabPage.AutoScroll = true;
            this.PatchTabPage.BackColor = System.Drawing.SystemColors.Control;
            this.PatchTabPage.Controls.Add(this.PatchMainPanel);
            this.PatchTabPage.Controls.Add(this.PatchFMNotInstalledLabel);
            this.PatchTabPage.Location = new System.Drawing.Point(4, 22);
            this.PatchTabPage.Name = "PatchTabPage";
            this.PatchTabPage.Size = new System.Drawing.Size(526, 284);
            this.PatchTabPage.TabIndex = 3;
            this.PatchTabPage.Text = "Patch & Customize";
            // 
            // PatchMainPanel
            // 
            this.PatchMainPanel.AutoSize = true;
            this.PatchMainPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.PatchMainPanel.Controls.Add(this.PatchDMLsPanel);
            this.PatchMainPanel.Controls.Add(this.PatchOpenFMFolderButton);
            this.PatchMainPanel.Location = new System.Drawing.Point(0, 0);
            this.PatchMainPanel.Name = "PatchMainPanel";
            this.PatchMainPanel.Size = new System.Drawing.Size(175, 154);
            this.PatchMainPanel.TabIndex = 38;
            // 
            // PatchDMLsPanel
            // 
            this.PatchDMLsPanel.AutoSize = true;
            this.PatchDMLsPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.PatchDMLsPanel.Controls.Add(this.PatchDMLPatchesLabel);
            this.PatchDMLsPanel.Controls.Add(this.PatchDMLsListBox);
            this.PatchDMLsPanel.Controls.Add(this.PatchRemoveDMLButton);
            this.PatchDMLsPanel.Controls.Add(this.PatchAddDMLButton);
            this.PatchDMLsPanel.Location = new System.Drawing.Point(0, 0);
            this.PatchDMLsPanel.Name = "PatchDMLsPanel";
            this.PatchDMLsPanel.Size = new System.Drawing.Size(172, 120);
            this.PatchDMLsPanel.TabIndex = 39;
            // 
            // PatchDMLPatchesLabel
            // 
            this.PatchDMLPatchesLabel.AutoSize = true;
            this.PatchDMLPatchesLabel.Location = new System.Drawing.Point(8, 8);
            this.PatchDMLPatchesLabel.Name = "PatchDMLPatchesLabel";
            this.PatchDMLPatchesLabel.Size = new System.Drawing.Size(156, 13);
            this.PatchDMLPatchesLabel.TabIndex = 40;
            this.PatchDMLPatchesLabel.Text = ".dml patches applied to this FM:";
            // 
            // PatchDMLsListBox
            // 
            this.PatchDMLsListBox.FormattingEnabled = true;
            this.PatchDMLsListBox.Location = new System.Drawing.Point(8, 24);
            this.PatchDMLsListBox.Name = "PatchDMLsListBox";
            this.PatchDMLsListBox.Size = new System.Drawing.Size(160, 69);
            this.PatchDMLsListBox.TabIndex = 41;
            // 
            // PatchRemoveDMLButton
            // 
            this.PatchRemoveDMLButton.Location = new System.Drawing.Point(122, 94);
            this.PatchRemoveDMLButton.Name = "PatchRemoveDMLButton";
            this.PatchRemoveDMLButton.Size = new System.Drawing.Size(23, 23);
            this.PatchRemoveDMLButton.TabIndex = 42;
            this.PatchRemoveDMLButton.UseVisualStyleBackColor = true;
            this.PatchRemoveDMLButton.Click += new System.EventHandler(this.PatchRemoveDMLButton_Click);
            this.PatchRemoveDMLButton.Paint += new System.Windows.Forms.PaintEventHandler(this.PatchRemoveDMLButton_Paint);
            // 
            // PatchAddDMLButton
            // 
            this.PatchAddDMLButton.Location = new System.Drawing.Point(146, 94);
            this.PatchAddDMLButton.Name = "PatchAddDMLButton";
            this.PatchAddDMLButton.Size = new System.Drawing.Size(23, 23);
            this.PatchAddDMLButton.TabIndex = 43;
            this.PatchAddDMLButton.UseVisualStyleBackColor = true;
            this.PatchAddDMLButton.Click += new System.EventHandler(this.PatchAddDMLButton_Click);
            this.PatchAddDMLButton.Paint += new System.Windows.Forms.PaintEventHandler(this.PatchAddDMLButton_Paint);
            // 
            // PatchOpenFMFolderButton
            // 
            this.PatchOpenFMFolderButton.AutoSize = true;
            this.PatchOpenFMFolderButton.Location = new System.Drawing.Point(7, 128);
            this.PatchOpenFMFolderButton.Name = "PatchOpenFMFolderButton";
            this.PatchOpenFMFolderButton.Size = new System.Drawing.Size(162, 23);
            this.PatchOpenFMFolderButton.TabIndex = 44;
            this.PatchOpenFMFolderButton.Text = "Open FM folder";
            this.PatchOpenFMFolderButton.UseVisualStyleBackColor = true;
            this.PatchOpenFMFolderButton.Click += new System.EventHandler(this.PatchOpenFMFolderButton_Click);
            // 
            // PatchFMNotInstalledLabel
            // 
            this.PatchFMNotInstalledLabel.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.PatchFMNotInstalledLabel.AutoSize = true;
            this.PatchFMNotInstalledLabel.Location = new System.Drawing.Point(-9, 264);
            this.PatchFMNotInstalledLabel.Name = "PatchFMNotInstalledLabel";
            this.PatchFMNotInstalledLabel.Size = new System.Drawing.Size(232, 13);
            this.PatchFMNotInstalledLabel.TabIndex = 45;
            this.PatchFMNotInstalledLabel.Text = "FM must be installed in order to use this section.";
            // 
            // ViewHTMLReadmeButton
            // 
            this.ViewHTMLReadmeButton.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.ViewHTMLReadmeButton.AutoSize = true;
            this.ViewHTMLReadmeButton.Location = new System.Drawing.Point(776, 144);
            this.ViewHTMLReadmeButton.Name = "ViewHTMLReadmeButton";
            this.ViewHTMLReadmeButton.Padding = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.ViewHTMLReadmeButton.Size = new System.Drawing.Size(144, 23);
            this.ViewHTMLReadmeButton.TabIndex = 49;
            this.ViewHTMLReadmeButton.Text = "View HTML Readme";
            this.ViewHTMLReadmeButton.UseVisualStyleBackColor = true;
            this.ViewHTMLReadmeButton.Visible = false;
            this.ViewHTMLReadmeButton.Click += new System.EventHandler(this.ViewHTMLReadmeButton_Click);
            // 
            // ReadmeFullScreenButton
            // 
            this.ReadmeFullScreenButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ReadmeFullScreenButton.BackColor = System.Drawing.SystemColors.Window;
            this.ReadmeFullScreenButton.FlatAppearance.BorderSize = 0;
            this.ReadmeFullScreenButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ReadmeFullScreenButton.Location = new System.Drawing.Point(1616, 8);
            this.ReadmeFullScreenButton.Name = "ReadmeFullScreenButton";
            this.ReadmeFullScreenButton.Size = new System.Drawing.Size(20, 20);
            this.ReadmeFullScreenButton.TabIndex = 55;
            this.MainToolTip.SetToolTip(this.ReadmeFullScreenButton, "Fullscreen");
            this.ReadmeFullScreenButton.UseVisualStyleBackColor = false;
            this.ReadmeFullScreenButton.Visible = false;
            this.ReadmeFullScreenButton.Click += new System.EventHandler(this.ReadmeFullScreenButton_Click);
            this.ReadmeFullScreenButton.Paint += new System.Windows.Forms.PaintEventHandler(this.ReadmeFullScreenButton_Paint);
            // 
            // ZoomInButton
            // 
            this.ZoomInButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ZoomInButton.BackColor = System.Drawing.SystemColors.Window;
            this.ZoomInButton.BackgroundImage = global::AngelLoader.Properties.Resources.ZoomIn;
            this.ZoomInButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.ZoomInButton.FlatAppearance.BorderSize = 0;
            this.ZoomInButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ZoomInButton.Location = new System.Drawing.Point(1534, 8);
            this.ZoomInButton.Name = "ZoomInButton";
            this.ZoomInButton.Size = new System.Drawing.Size(20, 20);
            this.ZoomInButton.TabIndex = 52;
            this.MainToolTip.SetToolTip(this.ZoomInButton, "Zoom in");
            this.ZoomInButton.UseVisualStyleBackColor = false;
            this.ZoomInButton.Visible = false;
            this.ZoomInButton.Click += new System.EventHandler(this.ZoomInButton_Click);
            // 
            // ZoomOutButton
            // 
            this.ZoomOutButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ZoomOutButton.BackColor = System.Drawing.SystemColors.Window;
            this.ZoomOutButton.BackgroundImage = global::AngelLoader.Properties.Resources.ZoomOut;
            this.ZoomOutButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.ZoomOutButton.FlatAppearance.BorderSize = 0;
            this.ZoomOutButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ZoomOutButton.Location = new System.Drawing.Point(1558, 8);
            this.ZoomOutButton.Name = "ZoomOutButton";
            this.ZoomOutButton.Size = new System.Drawing.Size(20, 20);
            this.ZoomOutButton.TabIndex = 53;
            this.MainToolTip.SetToolTip(this.ZoomOutButton, "Zoom out");
            this.ZoomOutButton.UseVisualStyleBackColor = false;
            this.ZoomOutButton.Visible = false;
            this.ZoomOutButton.Click += new System.EventHandler(this.ZoomOutButton_Click);
            // 
            // ResetZoomButton
            // 
            this.ResetZoomButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ResetZoomButton.BackColor = System.Drawing.SystemColors.Window;
            this.ResetZoomButton.BackgroundImage = global::AngelLoader.Properties.Resources.ZoomReset;
            this.ResetZoomButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.ResetZoomButton.FlatAppearance.BorderSize = 0;
            this.ResetZoomButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ResetZoomButton.Location = new System.Drawing.Point(1582, 8);
            this.ResetZoomButton.Name = "ResetZoomButton";
            this.ResetZoomButton.Size = new System.Drawing.Size(20, 20);
            this.ResetZoomButton.TabIndex = 54;
            this.MainToolTip.SetToolTip(this.ResetZoomButton, "Reset zoom");
            this.ResetZoomButton.UseVisualStyleBackColor = false;
            this.ResetZoomButton.Visible = false;
            this.ResetZoomButton.Click += new System.EventHandler(this.ResetZoomButton_Click);
            // 
            // ChooseReadmeComboBox
            // 
            this.ChooseReadmeComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ChooseReadmeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ChooseReadmeComboBox.FormattingEnabled = true;
            this.ChooseReadmeComboBox.Location = new System.Drawing.Point(1350, 8);
            this.ChooseReadmeComboBox.Name = "ChooseReadmeComboBox";
            this.ChooseReadmeComboBox.Size = new System.Drawing.Size(170, 21);
            this.ChooseReadmeComboBox.TabIndex = 51;
            this.ChooseReadmeComboBox.Visible = false;
            this.ChooseReadmeComboBox.SelectedIndexChanged += new System.EventHandler(this.ChooseReadmeComboBox_SelectedIndexChanged);
            this.ChooseReadmeComboBox.DropDownClosed += new System.EventHandler(this.ChooseReadmeComboBox_DropDownClosed);
            // 
            // ReadmeRichTextBox
            // 
            this.ReadmeRichTextBox.BackColor = System.Drawing.SystemColors.Window;
            this.ReadmeRichTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ReadmeRichTextBox.Location = new System.Drawing.Point(0, 0);
            this.ReadmeRichTextBox.Name = "ReadmeRichTextBox";
            this.ReadmeRichTextBox.ReadOnly = true;
            this.ReadmeRichTextBox.Size = new System.Drawing.Size(1671, 359);
            this.ReadmeRichTextBox.TabIndex = 0;
            this.ReadmeRichTextBox.Text = "";
            this.ReadmeRichTextBox.LinkClicked += new System.Windows.Forms.LinkClickedEventHandler(this.ReadmeRichTextBox_LinkClicked);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(1671, 716);
            this.Controls.Add(this.EverythingPanel);
            this.DoubleBuffered = true;
            this.Icon = global::AngelLoader.Properties.Resources.AngelLoader;
            this.KeyPreview = true;
            this.MinimumSize = new System.Drawing.Size(894, 260);
            this.Name = "MainForm";
            this.Text = "AngelLoader";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.Deactivate += new System.EventHandler(this.MainForm_Deactivate);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.Shown += new System.EventHandler(this.MainForm_Shown);
            this.LocationChanged += new System.EventHandler(this.MainForm_LocationChanged);
            this.SizeChanged += new System.EventHandler(this.MainForm_SizeChanged);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.MainForm_KeyDown);
            this.BottomPanel.ResumeLayout(false);
            this.BottomPanel.PerformLayout();
            this.BottomRightButtonsFLP.ResumeLayout(false);
            this.BottomRightButtonsFLP.PerformLayout();
            this.BottomLeftButtonsFLP.ResumeLayout(false);
            this.BottomLeftButtonsFLP.PerformLayout();
            this.EverythingPanel.ResumeLayout(false);
            this.MainSplitContainer.Panel1.ResumeLayout(false);
            this.MainSplitContainer.Panel2.ResumeLayout(false);
            this.MainSplitContainer.Panel2.PerformLayout();
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
            this.FilterGameButtonsToolStrip.ResumeLayout(false);
            this.FilterGameButtonsToolStrip.PerformLayout();
            this.FilterIconButtonsToolStrip.ResumeLayout(false);
            this.FilterIconButtonsToolStrip.PerformLayout();
            this.RefreshAreaToolStrip.ResumeLayout(false);
            this.RefreshAreaToolStrip.PerformLayout();
            this.GamesTabControl.ResumeLayout(false);
            this.TopRightTabControl.ResumeLayout(false);
            this.StatisticsTabPage.ResumeLayout(false);
            this.StatisticsTabPage.PerformLayout();
            this.StatsCheckBoxesPanel.ResumeLayout(false);
            this.StatsCheckBoxesPanel.PerformLayout();
            this.EditFMTabPage.ResumeLayout(false);
            this.EditFMTabPage.PerformLayout();
            this.CommentTabPage.ResumeLayout(false);
            this.CommentTabPage.PerformLayout();
            this.TagsTabPage.ResumeLayout(false);
            this.TagsTabPage.PerformLayout();
            this.AddRemoveTagFLP.ResumeLayout(false);
            this.AddRemoveTagFLP.PerformLayout();
            this.PatchTabPage.ResumeLayout(false);
            this.PatchTabPage.PerformLayout();
            this.PatchMainPanel.ResumeLayout(false);
            this.PatchMainPanel.PerformLayout();
            this.PatchDMLsPanel.ResumeLayout(false);
            this.PatchDMLsPanel.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
#endif

        private AngelLoader.CustomControls.SplitContainerCustom MainSplitContainer;
        private System.Windows.Forms.Panel BottomPanel;
        private System.Windows.Forms.Button TestButton;
        private AngelLoader.CustomControls.DataGridViewCustom FMsDGV;
        private System.Windows.Forms.Button InstallUninstallFMButton;
        private System.Windows.Forms.Button PlayFMButton;
        private AngelLoader.CustomControls.RichTextBoxCustom ReadmeRichTextBox;
        private System.Windows.Forms.Button ScanAllFMsButton;
        private System.Windows.Forms.Label DebugLabel;
        private AngelLoader.CustomControls.SplitContainerCustom TopSplitContainer;
        private System.Windows.Forms.Panel EverythingPanel;
        private System.Windows.Forms.Button SettingsButton;
        private AngelLoader.CustomControls.ComboBoxCustom ChooseReadmeComboBox;
        private System.Windows.Forms.Button ResetZoomButton;
        private System.Windows.Forms.Button Test2Button;
        private System.Windows.Forms.Label FilterTitleLabel;
        private AngelLoader.CustomControls.TextBoxCustom FilterTitleTextBox;
        private System.Windows.Forms.Button PlayOriginalGameButton;
        private System.Windows.Forms.TabControl GamesTabControl;
        private System.Windows.Forms.TabPage Thief1TabPage;
        private System.Windows.Forms.TabPage Thief2TabPage;
        private System.Windows.Forms.ImageList GameTabsImageList;
        private AngelLoader.CustomControls.TabControlCustom TopRightTabControl;
        private System.Windows.Forms.TabPage StatisticsTabPage;
        private System.Windows.Forms.Label CustomResourcesLabel;
        private System.Windows.Forms.CheckBox CR_MotionsCheckBox;
        private System.Windows.Forms.CheckBox CR_CreaturesCheckBox;
        private System.Windows.Forms.CheckBox CR_MapCheckBox;
        private System.Windows.Forms.CheckBox CR_ScriptsCheckBox;
        private System.Windows.Forms.CheckBox CR_ObjectsCheckBox;
        private System.Windows.Forms.CheckBox CR_SubtitlesCheckBox;
        private System.Windows.Forms.CheckBox CR_AutomapCheckBox;
        private System.Windows.Forms.CheckBox CR_TexturesCheckBox;
        private System.Windows.Forms.CheckBox CR_SoundsCheckBox;
        private System.Windows.Forms.CheckBox CR_MoviesCheckBox;
        private System.Windows.Forms.TabPage CommentTabPage;
        private System.Windows.Forms.TextBox CommentTextBox;
        private System.Windows.Forms.TabPage TagsTabPage;
        private System.Windows.Forms.TreeView TagsTreeView;
        private System.Windows.Forms.Button ResetLayoutButton;
        private System.Windows.Forms.ToolTip MainToolTip;
        private System.Windows.Forms.Button RemoveTagButton;
        private System.Windows.Forms.Button AddTagButton;
        private System.Windows.Forms.Button AddTagFromListButton;
        private AngelLoader.CustomControls.TextBoxCustom AddTagTextBox;
        private AngelLoader.CustomControls.TextBoxCustom FilterAuthorTextBox;
        private System.Windows.Forms.Label FilterAuthorLabel;
        private System.Windows.Forms.TabPage EditFMTabPage;
        private System.Windows.Forms.DateTimePicker EditFMLastPlayedDateTimePicker;
        private System.Windows.Forms.DateTimePicker EditFMReleaseDateDateTimePicker;
        private System.Windows.Forms.TextBox EditFMAuthorTextBox;
        private System.Windows.Forms.Label EditFMAuthorLabel;
        private System.Windows.Forms.Label EditFMTitleLabel;
        private System.Windows.Forms.CheckBox EditFMDisableAllModsCheckBox;
        private System.Windows.Forms.TextBox EditFMDisabledModsTextBox;
        private System.Windows.Forms.Label EditFMDisabledModsLabel;
        private System.Windows.Forms.Label DebugLabel2;
        private System.Windows.Forms.FlowLayoutPanel FilterBarFLP;
        private AngelLoader.CustomControls.ToolStripCustom FilterGameButtonsToolStrip;
        private AngelLoader.CustomControls.ToolStripButtonCustom FilterByThief1Button;
        private AngelLoader.CustomControls.ToolStripButtonCustom FilterByThief2Button;
        private System.Windows.Forms.Panel StatsCheckBoxesPanel;
        private System.Windows.Forms.CheckBox EditFMLastPlayedCheckBox;
        private System.Windows.Forms.CheckBox EditFMReleaseDateCheckBox;
        private System.Windows.Forms.ComboBox EditFMRatingComboBox;
        private System.Windows.Forms.Label EditFMRatingLabel;
        private System.Windows.Forms.Button EditFMFinishedOnButton;
        private System.Windows.Forms.Button ReadmeFullScreenButton;
        private System.Windows.Forms.Button WebSearchButton;
        private AngelLoader.CustomControls.ArrowButton FilterBarScrollRightButton;
        private AngelLoader.CustomControls.ArrowButton FilterBarScrollLeftButton;
        private System.Windows.Forms.TextBox EditFMTitleTextBox;
        private CustomControls.ArrowButton EditFMAltTitlesArrowButton;
        private CustomControls.ToolStripCustom FilterIconButtonsToolStrip;
        private CustomControls.ToolStripButtonCustom FilterByFinishedButton;
        private CustomControls.ToolStripButtonCustom FilterByUnfinishedButton;
        private CustomControls.ToolStripButtonCustom FilterByRatingButton;
        private CustomControls.ToolStripButtonCustom FilterByTagsButton;
        private CustomControls.ToolStripButtonCustom FilterByReleaseDateButton;
        private System.Windows.Forms.ToolStripLabel FilterByReleaseDateLabel;
        private System.Windows.Forms.ToolStripLabel FilterByRatingLabel;
        private System.Windows.Forms.Button ImportButton;
        private CustomControls.ToolStripCustom RefreshAreaToolStrip;
        private CustomControls.ToolStripButtonCustom ClearFiltersButton;
        private CustomControls.ToolStripButtonCustom FilterByThief3Button;
        private System.Windows.Forms.TabPage Thief3TabPage;
        private CustomControls.ToolStripButtonCustom RefreshFiltersButton;
        private System.Windows.Forms.FlowLayoutPanel BottomLeftButtonsFLP;
        private System.Windows.Forms.FlowLayoutPanel BottomRightButtonsFLP;
        private System.Windows.Forms.Button ViewHTMLReadmeButton;
        private CustomControls.ToolStripButtonCustom FilterByLastPlayedButton;
        private System.Windows.Forms.ToolStripLabel FilterByLastPlayedLabel;
        private System.Windows.Forms.Button ZoomInButton;
        private System.Windows.Forms.Button ZoomOutButton;
        private System.Windows.Forms.FlowLayoutPanel AddRemoveTagFLP;
        private System.Windows.Forms.Button EditFMScanReleaseDateButton;
        private System.Windows.Forms.Button EditFMScanAuthorButton;
        private System.Windows.Forms.Button EditFMScanTitleButton;
        private System.Windows.Forms.Button EditFMScanForReadmesButton;
        private System.Windows.Forms.Button StatsScanCustomResourcesButton;
        private System.Windows.Forms.TabPage PatchTabPage;
        private System.Windows.Forms.ListBox PatchDMLsListBox;
        private System.Windows.Forms.Button PatchRemoveDMLButton;
        private System.Windows.Forms.Button PatchAddDMLButton;
        private System.Windows.Forms.Label PatchDMLPatchesLabel;
        private System.Windows.Forms.Panel PatchDMLsPanel;
        private System.Windows.Forms.Label PatchFMNotInstalledLabel;
        private System.Windows.Forms.Panel PatchMainPanel;
        private System.Windows.Forms.Button PatchOpenFMFolderButton;
        private AngelLoader.CustomControls.ArrowButton TopRightCollapseButton;
        private CustomControls.ToolStripButtonCustom RefreshFromDiskButton;
        private CustomControls.ToolStripButtonCustom FMsListZoomInButton;
        private CustomControls.ToolStripButtonCustom FMsListZoomOutButton;
        private CustomControls.ToolStripButtonCustom FMsListResetZoomButton;
        private System.Windows.Forms.DataGridViewImageColumn GameTypeColumn;
        private System.Windows.Forms.DataGridViewImageColumn InstalledColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn TitleColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn ArchiveColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn AuthorColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn SizeColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn RatingTextColumn;
        private System.Windows.Forms.DataGridViewImageColumn FinishedColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn ReleaseDateColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn LastPlayedColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn DisabledModsColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn CommentColumn;
        private CustomControls.ToolStripButtonCustom FilterShowUnsupportedButton;
        private System.Windows.Forms.Button TopRightMenuButton;
    }
}
