using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using AngelLoader.CustomControls;
using AngelLoader.Properties;

namespace AngelLoader.Forms
{
    partial class MainForm
    {
        /*
         Notes:
            -All unused Name assignments have been removed, except those for the top-right tab pages because
             they're used for debug purposes.
            -Some Anchor / Dock combos have been kept despite the docs saying they're mutually exclusive, because
             they currently work and I'm not 100% certain which one I should keep. Lowest priority.
        */

#if DEBUG || Release_Testing
        private readonly System.Diagnostics.Stopwatch t = new System.Diagnostics.Stopwatch();

        private void StartTimer()
        {
            t.Restart();
        }

        private void StopTimer()
        {
            t.Stop();
            System.Diagnostics.Trace.WriteLine(nameof(InitComponentManual) + "up to stop point: " + t.Elapsed);
            System.Environment.Exit(1);
        }
#endif

        private void InitComponentManual()
        {
            components = new Container();

            // PERF_NOTE: Instantiation: ~15ms (suspicion confirmed - worth it to lazy-load everything that can be!)

            #region Instantiation

            GameTabsImageList = new ImageList(components);
            ScanAllFMsButton = new Button();
            BottomPanel = new Panel();
            BottomRightButtonsFLP = new FlowLayoutPanel();
            SettingsButton = new Button();
            ImportButton = new Button();
            BottomLeftButtonsFLP = new FlowLayoutPanel();
            PlayFMButton = new Button();
            PlayOriginalGameButton = new Button();
            InstallUninstallFMButton = new Button();
            BottomScanAllFMsSepToolStrip = new ToolStripCustom();
            BottomScanAllFMsLeftSep = new ToolStripSeparatorCustom();
            BottomWebSearchLeftSepToolStrip = new ToolStripCustom();
            BottomWebSearchLeftSep = new ToolStripSeparatorCustom();
            WebSearchButton = new Button();
            EverythingPanel = new Panel();
            AddTagListBox = new ListBox();
            MainSplitContainer = new SplitContainerCustom();
            TopSplitContainer = new SplitContainerCustom();
            FilterBarScrollRightButton = new Button();
            FilterBarScrollLeftButton = new Button();
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
            DisabledModsColumn = new DataGridViewTextBoxColumn();
            CommentColumn = new DataGridViewTextBoxColumn();
            FilterBarFLP = new FlowLayoutPanel();
            FilterGamesLeftSepToolStrip = new ToolStripCustom();
            FilterGamesLeftSep = new ToolStripSeparatorCustom();
            FilterGameButtonsToolStrip = new ToolStripCustom();
            FilterByThief1Button = new ToolStripButtonCustom();
            FilterByThief2Button = new ToolStripButtonCustom();
            FilterByThief3Button = new ToolStripButtonCustom();
            FilterTitleLeftSep = new ToolStripSeparatorCustom();
            FilterTitleLabel = new Label();
            FilterTitleTextBox = new TextBoxCustom();
            FilterAuthorLeftSepToolStrip = new ToolStripCustom();
            FilterAuthorLeftSep = new ToolStripSeparatorCustom();
            FilterAuthorLabel = new Label();
            FilterAuthorTextBox = new TextBoxCustom();
            FilterIconButtonsToolStrip = new ToolStripCustom();
            FilterReleaseDateLeftSep = new ToolStripSeparatorCustom();
            FilterByReleaseDateButton = new ToolStripButtonCustom();
            FilterByReleaseDateLabel = new ToolStripLabel();
            FilterLastPlayedLeftSep = new ToolStripSeparatorCustom();
            FilterByLastPlayedButton = new ToolStripButtonCustom();
            FilterByLastPlayedLabel = new ToolStripLabel();
            FilterTagsLeftSep = new ToolStripSeparatorCustom();
            FilterByTagsButton = new ToolStripButtonCustom();
            FilterFinishedLeftSep = new ToolStripSeparatorCustom();
            FilterByFinishedButton = new ToolStripButtonCustom();
            FilterByUnfinishedButton = new ToolStripButtonCustom();
            FilterRatingLeftSep = new ToolStripSeparatorCustom();
            FilterByRatingButton = new ToolStripButtonCustom();
            FilterByRatingLabel = new ToolStripLabel();
            FilterShowUnsupportedLeftSep = new ToolStripSeparatorCustom();
            FilterShowUnsupportedButton = new ToolStripButtonCustom();
            RefreshAreaToolStrip = new ToolStripCustom();
            FMsListZoomInButton = new ToolStripButtonCustom();
            FMsListZoomOutButton = new ToolStripButtonCustom();
            FMsListResetZoomButton = new ToolStripButtonCustom();
            RefreshAreaLeftSep = new ToolStripSeparatorCustom();
            RefreshFromDiskButton = new ToolStripButtonCustom();
            RefreshFiltersButton = new ToolStripButtonCustom();
            ClearFiltersButton = new ToolStripButtonCustom();
            ResetLayoutLeftSep = new ToolStripSeparatorCustom();
            ResetLayoutButton = new Button();
            GamesTabControl = new TabControl();
            Thief1TabPage = new TabPage();
            Thief2TabPage = new TabPage();
            Thief3TabPage = new TabPage();
            TopRightMenuButton = new Button();
            TopRightCollapseButton = new Button();
            TopRightTabControl = new TabControlCustom();
            StatisticsTabPage = new TabPage();
            StatsScanCustomResourcesButton = new Button();
            StatsCheckBoxesPanel = new Panel();
            CR_MapCheckBox = new CheckBox();
            CR_MoviesCheckBox = new CheckBox();
            CR_MotionsCheckBox = new CheckBox();
            CR_SoundsCheckBox = new CheckBox();
            CR_CreaturesCheckBox = new CheckBox();
            CR_TexturesCheckBox = new CheckBox();
            CR_AutomapCheckBox = new CheckBox();
            CR_ScriptsCheckBox = new CheckBox();
            CR_SubtitlesCheckBox = new CheckBox();
            CR_ObjectsCheckBox = new CheckBox();
            CustomResourcesLabel = new Label();
            EditFMTabPage = new TabPage();
            EditFMScanForReadmesButton = new Button();
            EditFMScanReleaseDateButton = new Button();
            EditFMScanAuthorButton = new Button();
            EditFMScanTitleButton = new Button();
            EditFMAltTitlesDropDownButton = new DropDownButton();
            EditFMTitleTextBox = new TextBox();
            EditFMFinishedOnButton = new Button();
            EditFMRatingComboBox = new ComboBox();
            EditFMRatingLabel = new Label();
            EditFMLastPlayedDateTimePicker = new DateTimePicker();
            EditFMReleaseDateDateTimePicker = new DateTimePicker();
            EditFMLastPlayedCheckBox = new CheckBox();
            EditFMReleaseDateCheckBox = new CheckBox();
            EditFMDisableAllModsCheckBox = new CheckBox();
            EditFMDisabledModsTextBox = new TextBox();
            EditFMDisabledModsLabel = new Label();
            EditFMAuthorTextBox = new TextBox();
            EditFMAuthorLabel = new Label();
            EditFMTitleLabel = new Label();
            CommentTabPage = new TabPage();
            CommentTextBox = new TextBox();
            TagsTabPage = new TabPage();
            AddTagButton = new Button();
            AddTagTextBox = new TextBoxCustom();
            AddRemoveTagFLP = new FlowLayoutPanel();
            RemoveTagButton = new Button();
            AddTagFromListButton = new Button();
            TagsTreeView = new TreeView();
            PatchTabPage = new TabPage();
            PatchMainPanel = new Panel();
            PatchDMLsPanel = new Panel();
            PatchDMLPatchesLabel = new Label();
            PatchDMLsListBox = new ListBox();
            PatchRemoveDMLButton = new Button();
            PatchAddDMLButton = new Button();
            PatchOpenFMFolderButton = new Button();
            PatchFMNotInstalledLabel = new Label();
            ViewHTMLReadmeButton = new Button();
            ReadmeFullScreenButton = new Button();
            ZoomInButton = new Button();
            ZoomOutButton = new Button();
            ResetZoomButton = new Button();
            ChooseReadmeComboBox = new ComboBoxCustom();
            ChooseReadmePanel = new Panel();
            ChooseReadmeOKFLP = new FlowLayoutPanel();
            ChooseReadmeButton = new Button();
            ChooseReadmeListBox = new ListBoxCustom();
            ReadmeRichTextBox = new RichTextBoxCustom();
            MainToolTip = new ToolTip(components);
            AddTagMenu = new ContextMenuStrip(components);

            #endregion

            // PERF_NOTE: SuspendLayouts: <1ms

            #region SuspendLayout()

            BottomPanel.SuspendLayout();
            BottomRightButtonsFLP.SuspendLayout();
            BottomLeftButtonsFLP.SuspendLayout();
            BottomScanAllFMsSepToolStrip.SuspendLayout();
            BottomWebSearchLeftSepToolStrip.SuspendLayout();
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
            FilterGamesLeftSepToolStrip.SuspendLayout();
            FilterGameButtonsToolStrip.SuspendLayout();
            FilterAuthorLeftSepToolStrip.SuspendLayout();
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
            ChooseReadmePanel.SuspendLayout();
            ChooseReadmeOKFLP.SuspendLayout();
            SuspendLayout();

            #endregion

            // PERF_NOTE: Property sets: 15~17ms

            #region Property sets

            // 
            // GameTabsImageList
            // 
            // PERF: WinForms does this by default:
            // GameTabsImageList.ImageStream = ((ImageListStreamer)(resources.GetObject("GameTabsImageList.ImageStream")));
            // But it's way faster to do this (saves ~8ms):
            GameTabsImageList.Images.Add(Resources.Thief1_16);
            GameTabsImageList.Images.Add(Resources.Thief2_16);
            GameTabsImageList.Images.Add(Resources.Thief3_16);
            GameTabsImageList.ColorDepth = ColorDepth.Depth32Bit;
            GameTabsImageList.Images.SetKeyName(0, "Thief1_16.png");
            GameTabsImageList.Images.SetKeyName(1, "Thief2_16.png");
            GameTabsImageList.Images.SetKeyName(2, "Thief3_16.png");
            // 
            // ScanAllFMsButton
            // 
            ScanAllFMsButton.AutoSize = true;
            ScanAllFMsButton.Image = Resources.Scan;
            ScanAllFMsButton.ImageAlign = ContentAlignment.MiddleLeft;
            ScanAllFMsButton.Padding = new Padding(6, 0, 6, 0);
            ScanAllFMsButton.Height = 36;
            ScanAllFMsButton.TabIndex = 59;
            ScanAllFMsButton.TextImageRelation = TextImageRelation.ImageBeforeText;
            ScanAllFMsButton.UseVisualStyleBackColor = true;
            ScanAllFMsButton.Click += ScanAllFMsButton_Click;
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
            BottomRightButtonsFLP.Controls.Add(ImportButton);
            BottomRightButtonsFLP.FlowDirection = FlowDirection.RightToLeft;
            BottomRightButtonsFLP.Location = new Point(1443, 0);
            BottomRightButtonsFLP.Size = new Size(226, 42);
            BottomRightButtonsFLP.TabIndex = 37;
            // 
            // SettingsButton
            // 
            SettingsButton.AutoSize = true;
            SettingsButton.Image = Resources.Settings_24;
            SettingsButton.ImageAlign = ContentAlignment.MiddleLeft;
            SettingsButton.Padding = new Padding(6, 0, 6, 0);
            SettingsButton.Height = 36;
            SettingsButton.TabIndex = 62;
            SettingsButton.TextImageRelation = TextImageRelation.ImageBeforeText;
            SettingsButton.UseVisualStyleBackColor = true;
            SettingsButton.Click += SettingsButton_Click;
            // 
            // ImportButton
            // 
            ImportButton.AutoSize = true;
            ImportButton.Image = Resources.Import_24;
            ImportButton.ImageAlign = ContentAlignment.MiddleLeft;
            ImportButton.Padding = new Padding(6, 0, 6, 0);
            ImportButton.Height = 36;
            ImportButton.TabIndex = 61;
            ImportButton.TextImageRelation = TextImageRelation.ImageBeforeText;
            ImportButton.UseVisualStyleBackColor = true;
            ImportButton.Click += ImportButton_Click;
            // 
            // BottomLeftButtonsFLP
            // 
            BottomLeftButtonsFLP.AutoSize = true;
            BottomLeftButtonsFLP.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            BottomLeftButtonsFLP.Controls.Add(PlayFMButton);
            BottomLeftButtonsFLP.Controls.Add(PlayOriginalGameButton);
            BottomLeftButtonsFLP.Controls.Add(InstallUninstallFMButton);
            BottomLeftButtonsFLP.Controls.Add(BottomScanAllFMsSepToolStrip);
            BottomLeftButtonsFLP.Controls.Add(ScanAllFMsButton);
            BottomLeftButtonsFLP.Controls.Add(BottomWebSearchLeftSepToolStrip);
            BottomLeftButtonsFLP.Controls.Add(WebSearchButton);
            BottomLeftButtonsFLP.Location = new Point(2, 0);
            BottomLeftButtonsFLP.Size = new Size(616, 42);
            BottomLeftButtonsFLP.TabIndex = 36;
            // 
            // PlayFMButton
            // 
            PlayFMButton.AutoSize = true;
            PlayFMButton.Image = Resources.PlayArrow_24;
            PlayFMButton.ImageAlign = ContentAlignment.MiddleLeft;
            PlayFMButton.Padding = new Padding(6, 0, 6, 0);
            PlayFMButton.Height = 36;
            PlayFMButton.TabIndex = 56;
            PlayFMButton.TextImageRelation = TextImageRelation.ImageBeforeText;
            PlayFMButton.UseVisualStyleBackColor = true;
            PlayFMButton.Click += PlayFMButton_Click;
            // 
            // PlayOriginalGameButton
            // 
            PlayOriginalGameButton.AutoSize = true;
            PlayOriginalGameButton.Image = Resources.Play_original_24;
            PlayOriginalGameButton.ImageAlign = ContentAlignment.MiddleLeft;
            PlayOriginalGameButton.Padding = new Padding(6, 0, 6, 0);
            PlayOriginalGameButton.Height = 36;
            PlayOriginalGameButton.TabIndex = 57;
            PlayOriginalGameButton.TextImageRelation = TextImageRelation.ImageBeforeText;
            PlayOriginalGameButton.UseVisualStyleBackColor = true;
            PlayOriginalGameButton.Click += PlayOriginalGameButton_Click;
            // 
            // InstallUninstallFMButton
            // 
            InstallUninstallFMButton.AutoSize = true;
            InstallUninstallFMButton.Image = Resources.Install_24;
            InstallUninstallFMButton.ImageAlign = ContentAlignment.MiddleLeft;
            InstallUninstallFMButton.Padding = new Padding(6, 0, 6, 0);
            InstallUninstallFMButton.Height = 36;
            InstallUninstallFMButton.TabIndex = 58;
            InstallUninstallFMButton.TextImageRelation = TextImageRelation.ImageBeforeText;
            InstallUninstallFMButton.UseVisualStyleBackColor = true;
            InstallUninstallFMButton.Click += InstallUninstallFMButton_Click;
            // 
            // BottomScanAllFMsSepToolStrip
            // 
            BottomScanAllFMsSepToolStrip.GripMargin = new Padding(0);
            BottomScanAllFMsSepToolStrip.GripStyle = ToolStripGripStyle.Hidden;
            BottomScanAllFMsSepToolStrip.Items.AddRange(new ToolStripItem[] {
            BottomScanAllFMsLeftSep});
            BottomScanAllFMsSepToolStrip.Padding = new Padding(0);
            BottomScanAllFMsSepToolStrip.Size = new Size(8, 42);
            BottomScanAllFMsSepToolStrip.TabIndex = 32;
            // 
            // BottomScanAllFMsLeftSep
            // 
            BottomScanAllFMsLeftSep.AutoSize = false;
            BottomScanAllFMsLeftSep.Size = new Size(6, 42);
            // 
            // BottomWebSearchLeftSepToolStrip
            // 
            BottomWebSearchLeftSepToolStrip.GripMargin = new Padding(0);
            BottomWebSearchLeftSepToolStrip.GripStyle = ToolStripGripStyle.Hidden;
            BottomWebSearchLeftSepToolStrip.Items.AddRange(new ToolStripItem[] {
            BottomWebSearchLeftSep});
            BottomWebSearchLeftSepToolStrip.Padding = new Padding(0);
            BottomWebSearchLeftSepToolStrip.Size = new Size(8, 42);
            BottomWebSearchLeftSepToolStrip.TabIndex = 35;
            // 
            // BottomWebSearchLeftSep
            // 
            BottomWebSearchLeftSep.AutoSize = false;
            BottomWebSearchLeftSep.Size = new Size(6, 42);
            // 
            // WebSearchButton
            // 
            WebSearchButton.AutoSize = true;
            WebSearchButton.Image = Resources.WebSearch_24;
            WebSearchButton.ImageAlign = ContentAlignment.MiddleLeft;
            WebSearchButton.Padding = new Padding(6, 0, 6, 0);
            WebSearchButton.Height = 36;
            WebSearchButton.TabIndex = 60;
            WebSearchButton.TextImageRelation = TextImageRelation.ImageBeforeText;
            WebSearchButton.UseVisualStyleBackColor = true;
            WebSearchButton.Click += WebSearchButton_Click;
            // 
            // EverythingPanel
            // 
            EverythingPanel.Controls.Add(AddTagListBox);
            EverythingPanel.Controls.Add(MainSplitContainer);
            EverythingPanel.Controls.Add(BottomPanel);
            EverythingPanel.Dock = DockStyle.Fill;
            EverythingPanel.Size = new Size(1671, 716);
            EverythingPanel.TabIndex = 4;
            // 
            // AddTagListBox
            // 
            AddTagListBox.FormattingEnabled = true;
            AddTagListBox.TabIndex = 3;
            AddTagListBox.Visible = false;
            AddTagListBox.SelectedIndexChanged += AddTagListBox_SelectedIndexChanged;
            AddTagListBox.KeyDown += AddTagTextBoxOrListBox_KeyDown;
            AddTagListBox.Leave += AddTagTextBoxOrListBox_Leave;
            AddTagListBox.MouseUp += AddTagListBox_MouseUp;
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
            MainSplitContainer.Panel2.Controls.Add(ViewHTMLReadmeButton);
            MainSplitContainer.Panel2.Controls.Add(ReadmeFullScreenButton);
            MainSplitContainer.Panel2.Controls.Add(ZoomInButton);
            MainSplitContainer.Panel2.Controls.Add(ZoomOutButton);
            MainSplitContainer.Panel2.Controls.Add(ResetZoomButton);
            MainSplitContainer.Panel2.Controls.Add(ChooseReadmeComboBox);
            MainSplitContainer.Panel2.Controls.Add(ChooseReadmePanel);
            MainSplitContainer.Panel2.Controls.Add(ReadmeRichTextBox);
            MainSplitContainer.Panel2MinSize = 38;
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
            TopSplitContainer.Panel2.SizeChanged += TopSplitContainer_Panel2_SizeChanged;
            TopSplitContainer.Size = new Size(1671, 309);
            TopSplitContainer.SplitterDistance = 1116;
            TopSplitContainer.TabIndex = 0;
            // 
            // FilterBarScrollRightButton
            // 
            FilterBarScrollRightButton.BackgroundImage = Resources.ArrowRightSmall;
            FilterBarScrollRightButton.BackgroundImageLayout = ImageLayout.Center;
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
            FilterBarScrollLeftButton.BackgroundImage = Resources.ArrowLeftSmall;
            FilterBarScrollLeftButton.BackgroundImageLayout = ImageLayout.Center;
            FilterBarScrollLeftButton.FlatStyle = FlatStyle.Flat;
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
                DisabledModsColumn,
                CommentColumn);
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
            FMsDGV.CellValueNeeded += FMsDGV_CellValueNeeded_Initial;
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
            FilterBarFLP.Controls.Add(FilterGamesLeftSepToolStrip);
            FilterBarFLP.Controls.Add(FilterGameButtonsToolStrip);
            FilterBarFLP.Controls.Add(FilterTitleLabel);
            FilterBarFLP.Controls.Add(FilterTitleTextBox);
            FilterBarFLP.Controls.Add(FilterAuthorLeftSepToolStrip);
            FilterBarFLP.Controls.Add(FilterAuthorLabel);
            FilterBarFLP.Controls.Add(FilterAuthorTextBox);
            FilterBarFLP.Controls.Add(FilterIconButtonsToolStrip);

            // PERF: The filter bar gets its x-pos and width set in GameTypeChange() and its y-pos is always 0.
            // Height is 100 so it goes behind the DataGridView and its actual scroll bars will be hidden but
            // they'll still function, and you can use your mousewheel or the custom arrow buttons to scroll.
            FilterBarFLP.Height = 100;

            FilterBarFLP.TabIndex = 11;
            FilterBarFLP.WrapContents = false;
            FilterBarFLP.Scroll += FiltersFlowLayoutPanel_Scroll;
            FilterBarFLP.SizeChanged += FiltersFlowLayoutPanel_SizeChanged;
            // 
            // FilterGamesLeftSepToolStrip
            // 
            FilterGamesLeftSepToolStrip.GripMargin = new Padding(0);
            FilterGamesLeftSepToolStrip.GripStyle = ToolStripGripStyle.Hidden;
            FilterGamesLeftSepToolStrip.Items.AddRange(new ToolStripItem[] {
            FilterGamesLeftSep});
            FilterGamesLeftSepToolStrip.Padding = new Padding(0);
            FilterGamesLeftSepToolStrip.PaddingDrawNudge = 1;
            FilterGamesLeftSepToolStrip.Size = new Size(6, 26);
            FilterGamesLeftSepToolStrip.TabIndex = 2;
            // 
            // FilterGamesLeftSep
            // 
            FilterGamesLeftSep.AutoSize = false;
            FilterGamesLeftSep.Margin = new Padding(0, 0, -2, 0);
            FilterGamesLeftSep.Size = new Size(6, 26);
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
            FilterTitleLeftSep});
            FilterGameButtonsToolStrip.LayoutStyle = ToolStripLayoutStyle.Flow;
            FilterGameButtonsToolStrip.Location = new Point(6, 0);
            FilterGameButtonsToolStrip.PaddingDrawNudge = 0;
            FilterGameButtonsToolStrip.Size = new Size(85, 26);
            FilterGameButtonsToolStrip.TabIndex = 3;
            // 
            // FilterByThief1Button
            // 
            FilterByThief1Button.AutoSize = false;
            FilterByThief1Button.CheckOnClick = true;
            FilterByThief1Button.DisplayStyle = ToolStripItemDisplayStyle.Image;
            FilterByThief1Button.Image = Resources.Thief1_21;
            FilterByThief1Button.Margin = new Padding(0);
            FilterByThief1Button.Size = new Size(25, 25);
            FilterByThief1Button.Click += FilterByGameCheckButtons_Click;
            // 
            // FilterByThief2Button
            // 
            FilterByThief2Button.AutoSize = false;
            FilterByThief2Button.CheckOnClick = true;
            FilterByThief2Button.DisplayStyle = ToolStripItemDisplayStyle.Image;
            FilterByThief2Button.Image = Resources.Thief2_21;
            FilterByThief2Button.Margin = new Padding(0);
            FilterByThief2Button.Size = new Size(25, 25);
            FilterByThief2Button.Click += FilterByGameCheckButtons_Click;
            // 
            // FilterByThief3Button
            // 
            FilterByThief3Button.AutoSize = false;
            FilterByThief3Button.CheckOnClick = true;
            FilterByThief3Button.DisplayStyle = ToolStripItemDisplayStyle.Image;
            FilterByThief3Button.Image = Resources.Thief3_21;
            FilterByThief3Button.Margin = new Padding(0);
            FilterByThief3Button.Size = new Size(25, 25);
            FilterByThief3Button.Click += FilterByGameCheckButtons_Click;
            // 
            // FilterTitleLeftSep
            // 
            FilterTitleLeftSep.AutoSize = false;
            FilterTitleLeftSep.Margin = new Padding(3, 0, 0, 0);
            FilterTitleLeftSep.Size = new Size(6, 26);
            // 
            // FilterTitleLabel
            // 
            FilterTitleLabel.AutoSize = true;
            FilterTitleLabel.Margin = new Padding(0, 6, 0, 0);
            FilterTitleLabel.TabIndex = 5;
            // 
            // FilterTitleTextBox
            // 
            FilterTitleTextBox.Size = new Size(144, 20);
            FilterTitleTextBox.TabIndex = 6;
            FilterTitleTextBox.TextChanged += FilterTextBoxes_TextChanged;
            // 
            // FilterAuthorLeftSepToolStrip
            // 
            FilterAuthorLeftSepToolStrip.GripMargin = new Padding(0);
            FilterAuthorLeftSepToolStrip.GripStyle = ToolStripGripStyle.Hidden;
            FilterAuthorLeftSepToolStrip.Items.AddRange(new ToolStripItem[] {
            FilterAuthorLeftSep});
            FilterAuthorLeftSepToolStrip.Padding = new Padding(0);
            FilterAuthorLeftSepToolStrip.PaddingDrawNudge = 1;
            FilterAuthorLeftSepToolStrip.Size = new Size(6, 26);
            FilterAuthorLeftSepToolStrip.TabIndex = 45;
            // 
            // FilterAuthorLeftSep
            // 
            FilterAuthorLeftSep.AutoSize = false;
            FilterAuthorLeftSep.Margin = new Padding(0, 0, -2, 0);
            FilterAuthorLeftSep.Size = new Size(6, 26);
            // 
            // FilterAuthorLabel
            // 
            FilterAuthorLabel.AutoSize = true;
            FilterAuthorLabel.Margin = new Padding(3, 6, 0, 0);
            FilterAuthorLabel.TabIndex = 7;
            // 
            // FilterAuthorTextBox
            // 
            FilterAuthorTextBox.Size = new Size(144, 20);
            FilterAuthorTextBox.TabIndex = 8;
            FilterAuthorTextBox.TextChanged += FilterTextBoxes_TextChanged;
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
            FilterReleaseDateLeftSep,
            FilterByReleaseDateButton,
            FilterByReleaseDateLabel,
            FilterLastPlayedLeftSep,
            FilterByLastPlayedButton,
            FilterByLastPlayedLabel,
            FilterTagsLeftSep,
            FilterByTagsButton,
            FilterFinishedLeftSep,
            FilterByFinishedButton,
            FilterByUnfinishedButton,
            FilterRatingLeftSep,
            FilterByRatingButton,
            FilterByRatingLabel,
            FilterShowUnsupportedLeftSep,
            FilterShowUnsupportedButton});
            FilterIconButtonsToolStrip.LayoutStyle = ToolStripLayoutStyle.Flow;
            FilterIconButtonsToolStrip.PaddingDrawNudge = 0;
            FilterIconButtonsToolStrip.Size = new Size(297, 26);
            FilterIconButtonsToolStrip.TabIndex = 3;
            // 
            // FilterReleaseDateLeftSep
            // 
            FilterReleaseDateLeftSep.AutoSize = false;
            FilterReleaseDateLeftSep.Margin = new Padding(0, 0, 3, 0);
            FilterReleaseDateLeftSep.Size = new Size(6, 26);
            // 
            // FilterByReleaseDateButton
            // 
            FilterByReleaseDateButton.AutoSize = false;
            FilterByReleaseDateButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
            FilterByReleaseDateButton.Image = Resources.FilterByReleaseDate;
            FilterByReleaseDateButton.Margin = new Padding(0);
            FilterByReleaseDateButton.Size = new Size(25, 25);
            FilterByReleaseDateButton.Click += FilterByReleaseDateButton_Click;
            // 
            // FilterByReleaseDateLabel
            // 
            FilterByReleaseDateLabel.ForeColor = Color.Maroon;
            FilterByReleaseDateLabel.Margin = new Padding(4, 5, 0, 2);
            FilterByReleaseDateLabel.Size = new Size(26, 15);
            // 
            // FilterLastPlayedLeftSep
            // 
            FilterLastPlayedLeftSep.AutoSize = false;
            FilterLastPlayedLeftSep.Size = new Size(6, 26);
            // 
            // FilterByLastPlayedButton
            // 
            FilterByLastPlayedButton.AutoSize = false;
            FilterByLastPlayedButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
            FilterByLastPlayedButton.Image = Resources.FilterByLastPlayed;
            FilterByLastPlayedButton.Margin = new Padding(0);
            FilterByLastPlayedButton.Size = new Size(25, 25);
            FilterByLastPlayedButton.Click += FilterByLastPlayedButton_Click;
            // 
            // FilterByLastPlayedLabel
            // 
            FilterByLastPlayedLabel.ForeColor = Color.Maroon;
            FilterByLastPlayedLabel.Margin = new Padding(4, 5, 0, 2);
            FilterByLastPlayedLabel.Size = new Size(25, 15);
            // 
            // FilterTagsLeftSep
            // 
            FilterTagsLeftSep.AutoSize = false;
            FilterTagsLeftSep.Size = new Size(6, 26);
            // 
            // FilterByTagsButton
            // 
            FilterByTagsButton.AutoSize = false;
            FilterByTagsButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
            FilterByTagsButton.Image = Resources.FilterByTags;
            FilterByTagsButton.Margin = new Padding(0);
            FilterByTagsButton.Size = new Size(25, 25);
            FilterByTagsButton.Click += FilterByTagsButton_Click;
            // 
            // FilterFinishedLeftSep
            // 
            FilterFinishedLeftSep.AutoSize = false;
            FilterFinishedLeftSep.Size = new Size(6, 26);
            // 
            // FilterByFinishedButton
            // 
            FilterByFinishedButton.AutoSize = false;
            FilterByFinishedButton.CheckOnClick = true;
            FilterByFinishedButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
            FilterByFinishedButton.Image = Resources.FilterByFinished;
            FilterByFinishedButton.Margin = new Padding(0);
            FilterByFinishedButton.Size = new Size(25, 25);
            FilterByFinishedButton.Click += FilterByFinishedButton_Click;
            // 
            // FilterByUnfinishedButton
            // 
            FilterByUnfinishedButton.AutoSize = false;
            FilterByUnfinishedButton.CheckOnClick = true;
            FilterByUnfinishedButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
            FilterByUnfinishedButton.Image = Resources.FilterByUnfinished;
            FilterByUnfinishedButton.Margin = new Padding(0);
            FilterByUnfinishedButton.Size = new Size(25, 25);
            FilterByUnfinishedButton.Click += FilterByUnfinishedButton_Click;
            // 
            // FilterRatingLeftSep
            // 
            FilterRatingLeftSep.AutoSize = false;
            FilterRatingLeftSep.Size = new Size(6, 26);
            // 
            // FilterByRatingButton
            // 
            FilterByRatingButton.AutoSize = false;
            FilterByRatingButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
            FilterByRatingButton.Image = Resources.FilterByRating;
            FilterByRatingButton.Margin = new Padding(0);
            FilterByRatingButton.Size = new Size(25, 25);
            FilterByRatingButton.Click += FilterByRatingButton_Click;
            // 
            // FilterByRatingLabel
            // 
            FilterByRatingLabel.ForeColor = Color.Maroon;
            FilterByRatingLabel.Margin = new Padding(4, 5, 0, 2);
            FilterByRatingLabel.Size = new Size(19, 15);
            // 
            // FilterShowUnsupportedLeftSep
            // 
            FilterShowUnsupportedLeftSep.AutoSize = false;
            FilterShowUnsupportedLeftSep.Size = new Size(6, 26);
            // 
            // FilterShowUnsupportedButton
            // 
            FilterShowUnsupportedButton.AutoSize = false;
            FilterShowUnsupportedButton.CheckOnClick = true;
            FilterShowUnsupportedButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
            FilterShowUnsupportedButton.Image = Resources.Show_Unsupported;
            FilterShowUnsupportedButton.Margin = new Padding(0);
            FilterShowUnsupportedButton.Size = new Size(25, 25);
            FilterShowUnsupportedButton.Click += FilterShowUnsupportedButton_Click;
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
            FMsListZoomInButton,
            FMsListZoomOutButton,
            FMsListResetZoomButton,
            RefreshAreaLeftSep,
            RefreshFromDiskButton,
            RefreshFiltersButton,
            ClearFiltersButton,
            ResetLayoutLeftSep});
            RefreshAreaToolStrip.LayoutStyle = ToolStripLayoutStyle.Flow;
            RefreshAreaToolStrip.Location = new Point(916, 0);
            RefreshAreaToolStrip.Size = new Size(169, 26);
            RefreshAreaToolStrip.TabIndex = 12;
            // 
            // FMsListZoomInButton
            // 
            FMsListZoomInButton.AutoSize = false;
            FMsListZoomInButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
            FMsListZoomInButton.Image = Resources.ZoomIn;
            FMsListZoomInButton.Margin = new Padding(0);
            FMsListZoomInButton.Size = new Size(25, 25);
            FMsListZoomInButton.Click += FMsListZoomInButton_Click;
            // 
            // FMsListZoomOutButton
            // 
            FMsListZoomOutButton.AutoSize = false;
            FMsListZoomOutButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
            FMsListZoomOutButton.Image = Resources.ZoomOut;
            FMsListZoomOutButton.Margin = new Padding(0);
            FMsListZoomOutButton.Size = new Size(25, 25);
            FMsListZoomOutButton.Click += FMsListZoomOutButton_Click;
            // 
            // FMsListResetZoomButton
            // 
            FMsListResetZoomButton.AutoSize = false;
            FMsListResetZoomButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
            FMsListResetZoomButton.Image = Resources.ZoomReset;
            FMsListResetZoomButton.Margin = new Padding(0);
            FMsListResetZoomButton.Size = new Size(25, 25);
            FMsListResetZoomButton.Click += FMsListResetZoomButton_Click;
            // 
            // RefreshAreaLeftSep
            // 
            RefreshAreaLeftSep.AutoSize = false;
            RefreshAreaLeftSep.Margin = new Padding(3, 0, 0, 0);
            RefreshAreaLeftSep.Size = new Size(6, 26);
            // 
            // RefreshFromDiskButton
            // 
            RefreshFromDiskButton.AutoSize = false;
            RefreshFromDiskButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
            RefreshFromDiskButton.Image = Resources.FindNewFMs_21;
            RefreshFromDiskButton.Margin = new Padding(0);
            RefreshFromDiskButton.Size = new Size(25, 25);
            RefreshFromDiskButton.Click += RefreshFromDiskButton_Click;
            // 
            // RefreshFiltersButton
            // 
            RefreshFiltersButton.AutoSize = false;
            RefreshFiltersButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
            RefreshFiltersButton.Image = Resources.Refresh;
            RefreshFiltersButton.Margin = new Padding(0);
            RefreshFiltersButton.Size = new Size(25, 25);
            RefreshFiltersButton.Click += RefreshFiltersButton_Click;
            // 
            // ClearFiltersButton
            // 
            ClearFiltersButton.AutoSize = false;
            ClearFiltersButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
            ClearFiltersButton.Image = Resources.ClearFilters;
            ClearFiltersButton.Margin = new Padding(0);
            ClearFiltersButton.Size = new Size(25, 25);
            ClearFiltersButton.Click += ClearFiltersButton_Click;
            // 
            // ResetLayoutLeftSep
            // 
            ResetLayoutLeftSep.AutoSize = false;
            ResetLayoutLeftSep.Margin = new Padding(3, 0, 0, 0);
            ResetLayoutLeftSep.Size = new Size(6, 26);
            // 
            // ResetLayoutButton
            // 
            ResetLayoutButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            ResetLayoutButton.BackgroundImage = Resources.ResetLayout;
            ResetLayoutButton.BackgroundImageLayout = ImageLayout.Zoom;
            ResetLayoutButton.FlatAppearance.BorderSize = 0;
            ResetLayoutButton.FlatStyle = FlatStyle.Flat;
            ResetLayoutButton.Location = new Point(1090, 2);
            ResetLayoutButton.Size = new Size(21, 21);
            ResetLayoutButton.TabIndex = 13;
            ResetLayoutButton.UseVisualStyleBackColor = true;
            ResetLayoutButton.Click += ResetLayoutButton_Click;
            // 
            // GamesTabControl
            // 
            GamesTabControl.Controls.Add(Thief1TabPage);
            GamesTabControl.Controls.Add(Thief2TabPage);
            GamesTabControl.Controls.Add(Thief3TabPage);
            GamesTabControl.ImageList = GameTabsImageList;
            GamesTabControl.Location = new Point(1, 5);
            GamesTabControl.SelectedIndex = 0;
            GamesTabControl.Size = new Size(1103, 24);
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
            // TopRightMenuButton
            // 
            TopRightMenuButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            TopRightMenuButton.BackgroundImage = Resources.Hamburger_16;
            TopRightMenuButton.BackgroundImageLayout = ImageLayout.Zoom;
            TopRightMenuButton.FlatAppearance.BorderSize = 0;
            TopRightMenuButton.FlatStyle = FlatStyle.Flat;
            TopRightMenuButton.Location = new Point(534, 2);
            TopRightMenuButton.Size = new Size(16, 16);
            TopRightMenuButton.TabIndex = 13;
            TopRightMenuButton.UseVisualStyleBackColor = true;
            TopRightMenuButton.Click += TopRightMenuButton_Click;
            // 
            // TopRightCollapseButton
            // 
            TopRightCollapseButton.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right;
            TopRightCollapseButton.BackgroundImageLayout = ImageLayout.None;
            TopRightCollapseButton.FlatAppearance.BorderSize = 0;
            TopRightCollapseButton.FlatStyle = FlatStyle.Flat;
            TopRightCollapseButton.Image = Resources.ArrowRightSmall;
            TopRightCollapseButton.Location = new Point(533, 20);
            TopRightCollapseButton.Size = new Size(18, 287);
            TopRightCollapseButton.TabIndex = 14;
            TopRightCollapseButton.UseVisualStyleBackColor = true;
            TopRightCollapseButton.Click += TopRightCollapseButton_Click;
            // 
            // TopRightTabControl
            // 
            TopRightTabControl.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            /* These tab pages get added as appropriate on startup
            TopRightTabControl.Controls.Add(StatisticsTabPage);
            TopRightTabControl.Controls.Add(EditFMTabPage);
            TopRightTabControl.Controls.Add(CommentTabPage);
            TopRightTabControl.Controls.Add(TagsTabPage);
            TopRightTabControl.Controls.Add(PatchTabPage);
            */
            TopRightTabControl.Location = new Point(0, 0);
            TopRightTabControl.Size = new Size(534, 310);
            TopRightTabControl.TabIndex = 15;
            // 
            // StatisticsTabPage
            // 
            StatisticsTabPage.AutoScroll = true;
            StatisticsTabPage.BackColor = SystemColors.Control;
            StatisticsTabPage.Controls.Add(StatsScanCustomResourcesButton);
            StatisticsTabPage.Controls.Add(StatsCheckBoxesPanel);
            StatisticsTabPage.Controls.Add(CustomResourcesLabel);
            StatisticsTabPage.Name = nameof(StatisticsTabPage);
            StatisticsTabPage.Size = new Size(526, 284);
            StatisticsTabPage.TabIndex = 0;
            // 
            // StatsScanCustomResourcesButton
            // 
            StatsScanCustomResourcesButton.AutoSize = true;
            StatsScanCustomResourcesButton.Image = Resources.ScanSmall;
            StatsScanCustomResourcesButton.ImageAlign = ContentAlignment.MiddleLeft;
            StatsScanCustomResourcesButton.Location = new Point(6, 200);
            StatsScanCustomResourcesButton.Size = new Size(154, 23);
            StatsScanCustomResourcesButton.TabIndex = 12;
            StatsScanCustomResourcesButton.TextImageRelation = TextImageRelation.ImageBeforeText;
            StatsScanCustomResourcesButton.UseVisualStyleBackColor = true;
            StatsScanCustomResourcesButton.Click += RescanCustomResourcesButton_Click;
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
            CR_MapCheckBox.Location = new Point(0, 0);
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
            EditFMTabPage.Controls.Add(EditFMScanForReadmesButton);
            EditFMTabPage.Controls.Add(EditFMScanReleaseDateButton);
            EditFMTabPage.Controls.Add(EditFMScanAuthorButton);
            EditFMTabPage.Controls.Add(EditFMScanTitleButton);
            EditFMTabPage.Controls.Add(EditFMAltTitlesDropDownButton);
            EditFMTabPage.Controls.Add(EditFMTitleTextBox);
            EditFMTabPage.Controls.Add(EditFMFinishedOnButton);
            EditFMTabPage.Controls.Add(EditFMRatingComboBox);
            EditFMTabPage.Controls.Add(EditFMRatingLabel);
            EditFMTabPage.Controls.Add(EditFMLastPlayedDateTimePicker);
            EditFMTabPage.Controls.Add(EditFMReleaseDateDateTimePicker);
            EditFMTabPage.Controls.Add(EditFMLastPlayedCheckBox);
            EditFMTabPage.Controls.Add(EditFMReleaseDateCheckBox);
            EditFMTabPage.Controls.Add(EditFMDisableAllModsCheckBox);
            EditFMTabPage.Controls.Add(EditFMDisabledModsTextBox);
            EditFMTabPage.Controls.Add(EditFMDisabledModsLabel);
            EditFMTabPage.Controls.Add(EditFMAuthorTextBox);
            EditFMTabPage.Controls.Add(EditFMAuthorLabel);
            EditFMTabPage.Controls.Add(EditFMTitleLabel);
            EditFMTabPage.Name = nameof(EditFMTabPage);
            EditFMTabPage.Size = new Size(526, 284);
            EditFMTabPage.TabIndex = 2;
            // 
            // EditFMScanForReadmesButton
            // 
            EditFMScanForReadmesButton.AutoSize = true;
            EditFMScanForReadmesButton.Image = Resources.ScanSmall;
            EditFMScanForReadmesButton.ImageAlign = ContentAlignment.MiddleLeft;
            EditFMScanForReadmesButton.Location = new Point(8, 248);
            EditFMScanForReadmesButton.Height = 23;
            EditFMScanForReadmesButton.TabIndex = 31;
            EditFMScanForReadmesButton.TextImageRelation = TextImageRelation.ImageBeforeText;
            EditFMScanForReadmesButton.UseVisualStyleBackColor = true;
            EditFMScanForReadmesButton.Click += EditFMScanForReadmesButton_Click;
            // 
            // EditFMScanReleaseDateButton
            // 
            EditFMScanReleaseDateButton.BackgroundImage = Resources.ScanSmall;
            EditFMScanReleaseDateButton.BackgroundImageLayout = ImageLayout.Zoom;
            EditFMScanReleaseDateButton.Location = new Point(136, 105);
            EditFMScanReleaseDateButton.Size = new Size(22, 22);
            EditFMScanReleaseDateButton.TabIndex = 22;
            EditFMScanReleaseDateButton.UseVisualStyleBackColor = true;
            EditFMScanReleaseDateButton.Click += EditFMScanReleaseDateButton_Click;
            // 
            // EditFMScanAuthorButton
            // 
            EditFMScanAuthorButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            EditFMScanAuthorButton.BackgroundImage = Resources.ScanSmall;
            EditFMScanAuthorButton.BackgroundImageLayout = ImageLayout.Zoom;
            EditFMScanAuthorButton.Location = new Point(493, 63);
            EditFMScanAuthorButton.Size = new Size(22, 22);
            EditFMScanAuthorButton.TabIndex = 19;
            EditFMScanAuthorButton.UseVisualStyleBackColor = true;
            EditFMScanAuthorButton.Click += EditFMScanAuthorButton_Click;
            // 
            // EditFMScanTitleButton
            // 
            EditFMScanTitleButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            EditFMScanTitleButton.BackgroundImage = Resources.ScanSmall;
            EditFMScanTitleButton.BackgroundImageLayout = ImageLayout.Zoom;
            EditFMScanTitleButton.Location = new Point(493, 23);
            EditFMScanTitleButton.Size = new Size(22, 22);
            EditFMScanTitleButton.TabIndex = 16;
            EditFMScanTitleButton.UseVisualStyleBackColor = true;
            EditFMScanTitleButton.Click += EditFMScanTitleButton_Click;
            // 
            // EditFMAltTitlesDropDownButton
            // 
            EditFMAltTitlesDropDownButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            EditFMAltTitlesDropDownButton.Location = new Point(477, 23);
            EditFMAltTitlesDropDownButton.Size = new Size(17, 22);
            EditFMAltTitlesDropDownButton.TabIndex = 15;
            EditFMAltTitlesDropDownButton.UseVisualStyleBackColor = true;
            EditFMAltTitlesDropDownButton.Click += EditFMAltTitlesDropDownButton_Click;
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
            EditFMFinishedOnButton.Location = new Point(184, 144);
            EditFMFinishedOnButton.Padding = new Padding(6, 0, 6, 0);
            EditFMFinishedOnButton.Height = 23;
            EditFMFinishedOnButton.TabIndex = 27;
            EditFMFinishedOnButton.UseVisualStyleBackColor = true;
            EditFMFinishedOnButton.Click += EditFMFinishedOnButton_Click;
            // 
            // EditFMRatingComboBox
            // 
            EditFMRatingComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
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
            EditFMLastPlayedCheckBox.CheckAlign = ContentAlignment.MiddleRight;
            EditFMLastPlayedCheckBox.Location = new Point(8, 130);
            EditFMLastPlayedCheckBox.TabIndex = 23;
            EditFMLastPlayedCheckBox.UseVisualStyleBackColor = true;
            EditFMLastPlayedCheckBox.CheckedChanged += EditFMLastPlayedCheckBox_CheckedChanged;
            // 
            // EditFMReleaseDateCheckBox
            // 
            EditFMReleaseDateCheckBox.AutoSize = true;
            EditFMReleaseDateCheckBox.CheckAlign = ContentAlignment.MiddleRight;
            EditFMReleaseDateCheckBox.Location = new Point(8, 88);
            EditFMReleaseDateCheckBox.TabIndex = 20;
            EditFMReleaseDateCheckBox.UseVisualStyleBackColor = true;
            EditFMReleaseDateCheckBox.CheckedChanged += EditFMReleaseDateCheckBox_CheckedChanged;
            // 
            // EditFMDisableAllModsCheckBox
            // 
            EditFMDisableAllModsCheckBox.AutoSize = true;
            EditFMDisableAllModsCheckBox.Location = new Point(8, 216);
            EditFMDisableAllModsCheckBox.TabIndex = 30;
            EditFMDisableAllModsCheckBox.UseVisualStyleBackColor = true;
            EditFMDisableAllModsCheckBox.CheckedChanged += EditFMDisableAllModsCheckBox_CheckedChanged;
            // 
            // EditFMDisabledModsTextBox
            // 
            EditFMDisabledModsTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            EditFMDisabledModsTextBox.Location = new Point(8, 192);
            EditFMDisabledModsTextBox.Size = new Size(502, 20);
            EditFMDisabledModsTextBox.TabIndex = 29;
            EditFMDisabledModsTextBox.TextChanged += EditFMDisabledModsTextBox_TextChanged;
            EditFMDisabledModsTextBox.Leave += EditFMDisabledModsTextBox_Leave;
            // 
            // EditFMDisabledModsLabel
            // 
            EditFMDisabledModsLabel.AutoSize = true;
            EditFMDisabledModsLabel.Location = new Point(8, 176);
            EditFMDisabledModsLabel.TabIndex = 28;
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
            CommentTabPage.Name = nameof(CommentTabPage);
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
            TagsTabPage.BackColor = SystemColors.Control;
            TagsTabPage.Controls.Add(AddTagButton);
            TagsTabPage.Controls.Add(AddTagTextBox);
            TagsTabPage.Controls.Add(AddRemoveTagFLP);
            TagsTabPage.Controls.Add(TagsTreeView);
            TagsTabPage.Name = nameof(TagsTabPage);
            TagsTabPage.Size = new Size(526, 284);
            TagsTabPage.TabIndex = 1;
            // 
            // AddTagButton
            // 
            AddTagButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            AddTagButton.AutoSize = true;
            AddTagButton.Location = new Point(444, 7);
            AddTagButton.Padding = new Padding(6, 0, 6, 0);
            AddTagButton.Height = 23;
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
            AddRemoveTagFLP.Location = new Point(-11, 248);
            AddRemoveTagFLP.Size = new Size(536, 24);
            AddRemoveTagFLP.TabIndex = 3;
            // 
            // RemoveTagButton
            // 
            RemoveTagButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            RemoveTagButton.AutoSize = true;
            RemoveTagButton.Location = new Point(392, 0);
            RemoveTagButton.Margin = new Padding(0, 0, 6, 0);
            RemoveTagButton.Padding = new Padding(6, 0, 6, 0);
            RemoveTagButton.Height = 23;
            RemoveTagButton.TabIndex = 1;
            RemoveTagButton.UseVisualStyleBackColor = true;
            RemoveTagButton.Click += RemoveTagButton_Click;
            // 
            // AddTagFromListButton
            // 
            AddTagFromListButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            AddTagFromListButton.AutoSize = true;
            AddTagFromListButton.Location = new Point(248, 0);
            AddTagFromListButton.Margin = new Padding(0);
            AddTagFromListButton.Padding = new Padding(6, 0, 6, 0);
            AddTagFromListButton.Height = 23;
            AddTagFromListButton.TabIndex = 0;
            AddTagFromListButton.UseVisualStyleBackColor = true;
            AddTagFromListButton.Click += TagPresetsButton_Click;
            // 
            // TagsTreeView
            // 
            TagsTreeView.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            TagsTreeView.HideSelection = false;
            TagsTreeView.Location = new Point(8, 32);
            TagsTreeView.Size = new Size(510, 216);
            TagsTreeView.TabIndex = 2;
            // 
            // PatchTabPage
            // 
            PatchTabPage.AutoScroll = true;
            PatchTabPage.BackColor = SystemColors.Control;
            PatchTabPage.Controls.Add(PatchMainPanel);
            PatchTabPage.Controls.Add(PatchFMNotInstalledLabel);
            PatchTabPage.Name = nameof(PatchTabPage);
            PatchTabPage.Size = new Size(526, 284);
            PatchTabPage.TabIndex = 3;
            // 
            // PatchMainPanel
            // 
            PatchMainPanel.AutoSize = true;
            PatchMainPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            PatchMainPanel.Controls.Add(PatchDMLsPanel);
            PatchMainPanel.Controls.Add(PatchOpenFMFolderButton);
            PatchMainPanel.Location = new Point(0, 0);
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
            PatchDMLsPanel.Location = new Point(0, 0);
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
            PatchDMLsListBox.FormattingEnabled = true;
            PatchDMLsListBox.Location = new Point(8, 24);
            PatchDMLsListBox.Size = new Size(160, 69);
            PatchDMLsListBox.TabIndex = 41;
            // 
            // PatchRemoveDMLButton
            // 
            PatchRemoveDMLButton.BackgroundImage = Resources.Minus;
            PatchRemoveDMLButton.BackgroundImageLayout = ImageLayout.Zoom;
            PatchRemoveDMLButton.Location = new Point(122, 94);
            PatchRemoveDMLButton.Size = new Size(23, 23);
            PatchRemoveDMLButton.TabIndex = 42;
            PatchRemoveDMLButton.UseVisualStyleBackColor = true;
            PatchRemoveDMLButton.Click += PatchRemoveDMLButton_Click;
            // 
            // PatchAddDMLButton
            // 
            PatchAddDMLButton.BackgroundImage = Resources.Add;
            PatchAddDMLButton.BackgroundImageLayout = ImageLayout.Zoom;
            PatchAddDMLButton.Location = new Point(146, 94);
            PatchAddDMLButton.Size = new Size(23, 23);
            PatchAddDMLButton.TabIndex = 43;
            PatchAddDMLButton.UseVisualStyleBackColor = true;
            PatchAddDMLButton.Click += PatchAddDMLButton_Click;
            // 
            // PatchOpenFMFolderButton
            // 
            PatchOpenFMFolderButton.AutoSize = true;
            PatchOpenFMFolderButton.Location = new Point(7, 128);
            PatchOpenFMFolderButton.Height = 23;
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
            // ViewHTMLReadmeButton
            // 
            ViewHTMLReadmeButton.Anchor = AnchorStyles.None;
            ViewHTMLReadmeButton.AutoSize = true;
            // This thing gets centered later so no location is specified here
            ViewHTMLReadmeButton.Padding = new Padding(6, 0, 6, 0);
            ViewHTMLReadmeButton.Height = 23;
            ViewHTMLReadmeButton.TabIndex = 49;
            ViewHTMLReadmeButton.UseVisualStyleBackColor = true;
            ViewHTMLReadmeButton.Visible = false;
            ViewHTMLReadmeButton.Click += ViewHTMLReadmeButton_Click;

            #region Readme control buttons

            // BackColor for these gets set in ShowReadme()

            // 
            // ReadmeFullScreenButton
            // 
            ReadmeFullScreenButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            ReadmeFullScreenButton.BackgroundImage = Resources.Fullscreen;
            ReadmeFullScreenButton.BackgroundImageLayout = ImageLayout.Zoom;
            ReadmeFullScreenButton.FlatAppearance.BorderSize = 0;
            ReadmeFullScreenButton.FlatStyle = FlatStyle.Flat;
            ReadmeFullScreenButton.Location = new Point(1616, 8);
            ReadmeFullScreenButton.Size = new Size(20, 20);
            ReadmeFullScreenButton.TabIndex = 55;
            ReadmeFullScreenButton.UseVisualStyleBackColor = false;
            ReadmeFullScreenButton.Visible = false;
            ReadmeFullScreenButton.Click += ReadmeFullScreenButton_Click;
            // 
            // ZoomInButton
            // 
            ZoomInButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            ZoomInButton.BackgroundImage = Resources.ZoomIn;
            ZoomInButton.BackgroundImageLayout = ImageLayout.Zoom;
            ZoomInButton.FlatAppearance.BorderSize = 0;
            ZoomInButton.FlatStyle = FlatStyle.Flat;
            ZoomInButton.Location = new Point(1534, 8);
            ZoomInButton.Size = new Size(20, 20);
            ZoomInButton.TabIndex = 52;
            ZoomInButton.UseVisualStyleBackColor = false;
            ZoomInButton.Visible = false;
            ZoomInButton.Click += ZoomInButton_Click;
            // 
            // ZoomOutButton
            // 
            ZoomOutButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            ZoomOutButton.BackColor = SystemColors.Window;
            ZoomOutButton.BackgroundImage = Resources.ZoomOut;
            ZoomOutButton.BackgroundImageLayout = ImageLayout.Zoom;
            ZoomOutButton.FlatAppearance.BorderSize = 0;
            ZoomOutButton.FlatStyle = FlatStyle.Flat;
            ZoomOutButton.Location = new Point(1558, 8);
            ZoomOutButton.Size = new Size(20, 20);
            ZoomOutButton.TabIndex = 53;
            ZoomOutButton.UseVisualStyleBackColor = false;
            ZoomOutButton.Visible = false;
            ZoomOutButton.Click += ZoomOutButton_Click;
            // 
            // ResetZoomButton
            // 
            ResetZoomButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            ResetZoomButton.BackgroundImage = Resources.ZoomReset;
            ResetZoomButton.BackgroundImageLayout = ImageLayout.Zoom;
            ResetZoomButton.FlatAppearance.BorderSize = 0;
            ResetZoomButton.FlatStyle = FlatStyle.Flat;
            ResetZoomButton.Location = new Point(1582, 8);
            ResetZoomButton.Size = new Size(20, 20);
            ResetZoomButton.TabIndex = 54;
            ResetZoomButton.UseVisualStyleBackColor = false;
            ResetZoomButton.Visible = false;
            ResetZoomButton.Click += ResetZoomButton_Click;

            #endregion
            // 
            // ChooseReadmeComboBox
            // 
            ChooseReadmeComboBox.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            ChooseReadmeComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            ChooseReadmeComboBox.FormattingEnabled = true;
            ChooseReadmeComboBox.Location = new Point(1350, 8);
            ChooseReadmeComboBox.Size = new Size(170, 21);
            ChooseReadmeComboBox.TabIndex = 51;
            ChooseReadmeComboBox.Visible = false;
            ChooseReadmeComboBox.SelectedIndexChanged += ChooseReadmeComboBox_SelectedIndexChanged;
            ChooseReadmeComboBox.DropDownClosed += ChooseReadmeComboBox_DropDownClosed;
            // 
            // ChooseReadmePanel
            // 
            // PERF_TODO: AutoSizeMode is GrowAndShrink, so can we get rid of the size set?
            // Should it even be autosizing at all...? (I don't think so?!)
            ChooseReadmePanel.Anchor = AnchorStyles.None;
            ChooseReadmePanel.AutoSize = true;
            ChooseReadmePanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            ChooseReadmePanel.Controls.Add(ChooseReadmeOKFLP);
            ChooseReadmePanel.Controls.Add(ChooseReadmeListBox);
            // This gets centered later so no location is specified here
            ChooseReadmePanel.Size = new Size(324, 161);
            ChooseReadmePanel.TabIndex = 46;
            ChooseReadmePanel.Visible = false;
            // 
            // ChooseReadmeOKFLP
            // 
            ChooseReadmeOKFLP.Controls.Add(ChooseReadmeButton);
            ChooseReadmeOKFLP.FlowDirection = FlowDirection.RightToLeft;
            ChooseReadmeOKFLP.Location = new Point(1, 134);
            ChooseReadmeOKFLP.Size = new Size(320, 24);
            ChooseReadmeOKFLP.TabIndex = 3;
            // 
            // ChooseReadmeButton
            // 
            ChooseReadmeButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            ChooseReadmeButton.AutoSize = true;
            ChooseReadmeButton.Location = new Point(245, 0);
            ChooseReadmeButton.Margin = new Padding(0);
            ChooseReadmeButton.Padding = new Padding(6, 0, 6, 0);
            ChooseReadmeButton.Height = 23;
            ChooseReadmeButton.TabIndex = 48;
            ChooseReadmeButton.UseVisualStyleBackColor = true;
            ChooseReadmeButton.Click += ChooseReadmeButton_Click;
            // 
            // ChooseReadmeListBox
            // 
            ChooseReadmeListBox.FormattingEnabled = true;
            ChooseReadmeListBox.Location = new Point(0, 0);
            ChooseReadmeListBox.Size = new Size(320, 134);
            ChooseReadmeListBox.TabIndex = 47;
            // 
            // ReadmeRichTextBox
            // 
            ReadmeRichTextBox.BackColor = SystemColors.Window;
            ReadmeRichTextBox.ReadOnly = true;
            ReadmeRichTextBox.Dock = DockStyle.Fill;
            ReadmeRichTextBox.TabIndex = 0;
            ReadmeRichTextBox.LinkClicked += ReadmeRichTextBox_LinkClicked;
            // 
            // AddTagMenu
            // 
            AddTagMenu.Closed += AddTagMenu_Closed;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            // NOTE: Keeping this in just in case... it takes <1ms to do anyway
            ClientSize = new Size(1671, 716);
            AutoScaleMode = AutoScaleMode.Dpi;
            Controls.Add(EverythingPanel);
            DoubleBuffered = true;
            Icon = Resources.AngelLoader;
            MinimumSize = new Size(894, 260);
            Deactivate += MainForm_Deactivate;
            FormClosing += MainForm_FormClosing;
            Load += MainForm_Load;
            Shown += MainForm_Shown;
            LocationChanged += MainForm_LocationChanged;
            SizeChanged += MainForm_SizeChanged;
            KeyDown += MainForm_KeyDown;

            #endregion

            // PERF: Some of these we resume in SetUITextToLocalized(), so we don't want to do duplicate work
            // PERF_NOTE: ResumeLayouts: ~4ms

            BottomPanel.ResumeLayout();

            /* Deferred
                BottomRightButtonsFLP.ResumeLayout();
                BottomLeftButtonsFLP.ResumeLayout();
            */

            BottomScanAllFMsSepToolStrip.ResumeLayout();
            BottomWebSearchLeftSepToolStrip.ResumeLayout();
            EverythingPanel.ResumeLayout();
            MainSplitContainer.Panel1.ResumeLayout();

            /* Deferred
                MainSplitContainer.Panel2.ResumeLayout();
            */

            MainSplitContainer.EndInit();
            MainSplitContainer.ResumeLayout();
            TopSplitContainer.Panel1.ResumeLayout();
            TopSplitContainer.Panel2.ResumeLayout();
            TopSplitContainer.EndInit();
            TopSplitContainer.ResumeLayout();
            ((ISupportInitialize)FMsDGV).EndInit();
            FilterBarFLP.ResumeLayout();
            FilterGamesLeftSepToolStrip.ResumeLayout();
            FilterGameButtonsToolStrip.ResumeLayout();
            FilterAuthorLeftSepToolStrip.ResumeLayout();
            FilterIconButtonsToolStrip.ResumeLayout();
            RefreshAreaToolStrip.ResumeLayout();
            GamesTabControl.ResumeLayout();
            TopRightTabControl.ResumeLayout();

            /* Deferred
                StatisticsTabPage.ResumeLayout();
                StatsCheckBoxesPanel.ResumeLayout();
                EditFMTabPage.ResumeLayout();
                CommentTabPage.ResumeLayout();
                TagsTabPage.ResumeLayout();
                AddRemoveTagFLP.ResumeLayout();
            */

            PatchTabPage.ResumeLayout();

            /* Deferred
                PatchMainPanel.ResumeLayout();
            */

            PatchDMLsPanel.ResumeLayout();

            /* Deferred
                ChooseReadmePanel.ResumeLayout();
            */

            ChooseReadmeOKFLP.ResumeLayout();
            ResumeLayout();
        }
    }
}
