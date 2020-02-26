using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using AngelLoader.CustomControls;
using AngelLoader.Properties;
using static AngelLoader.Misc;

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
        private readonly System.Diagnostics.Stopwatch initT = new System.Diagnostics.Stopwatch();

        private void StartTimer()
        {
            initT.Restart();
        }

        private void StopTimer()
        {
            initT.Stop();
            System.Diagnostics.Trace.WriteLine(nameof(InitComponentManual) + "up to stop point: " + initT.Elapsed);
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
            WebSearchButton = new Button();
            EverythingPanel = new Panel();
            MainSplitContainer = new SplitContainerCustom();
            TopSplitContainer = new SplitContainerCustom();
            FilterBarScrollRightButton = new ArrowButton();
            FilterBarScrollLeftButton = new ArrowButton();
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
            FilterTitleLabel = new Label();
            FilterTitleTextBox = new TextBoxCustom();
            FilterAuthorLabel = new Label();
            FilterAuthorTextBox = new TextBoxCustom();
            FilterIconButtonsToolStrip = new ToolStripCustom();
            FilterByReleaseDateButton = new ToolStripButtonCustom();
            FilterByLastPlayedButton = new ToolStripButtonCustom();
            FilterByTagsButton = new ToolStripButtonCustom();
            FilterByFinishedButton = new ToolStripButtonCustom();
            FilterByUnfinishedButton = new ToolStripButtonCustom();
            FilterByRatingButton = new ToolStripButtonCustom();
            FilterShowUnsupportedButton = new ToolStripButtonCustom();
            FilterShowRecentAtTopButton = new ToolStripButtonCustom();
            RefreshAreaToolStrip = new ToolStripCustom();
            RefreshFromDiskButton = new ToolStripButtonCustom();
            RefreshFiltersButton = new ToolStripButtonCustom();
            ClearFiltersButton = new ToolStripButtonCustom();
            ResetLayoutButton = new Button();
            GamesTabControl = new TabControl();
            Thief1TabPage = new TabPage();
            Thief2TabPage = new TabPage();
            Thief3TabPage = new TabPage();
            TopRightMenuButton = new Button();
            TopRightCollapseButton = new ArrowButton();
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
            EditFMAltTitlesArrowButton = new ArrowButton();
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
            EditFMLanguageLabel = new Label();
            EditFMLanguageComboBox = new ComboBoxCustom();
            EditFMScanLanguagesButton = new Button();
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
            ReadmeFullScreenButton = new Button();
            ReadmeZoomInButton = new Button();
            ReadmeZoomOutButton = new Button();
            ReadmeResetZoomButton = new Button();
            ChooseReadmeComboBox = new ComboBoxCustom();
            ReadmeRichTextBox = new RichTextBoxCustom();
            MainToolTip = new ToolTip(components);
            FilterBySS2Button = new ToolStripButtonCustom();
            TagsTabAutoScrollMarker = new Control();
            SS2TabPage = new TabPage();

            #endregion

            // PERF_NOTE: SuspendLayouts: <1ms

            #region SuspendLayout()

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
            GameTabsImageList.Images.Add(Images.Thief1_16);
            GameTabsImageList.Images.Add(Images.Thief2_16);
            GameTabsImageList.Images.Add(Images.Thief3_16);
            GameTabsImageList.Images.Add(Images.Shock2_16);
            GameTabsImageList.ColorDepth = ColorDepth.Depth32Bit;
            GameTabsImageList.Images.SetKeyName(0, "Thief1_16.png");
            GameTabsImageList.Images.SetKeyName(1, "Thief2_16.png");
            GameTabsImageList.Images.SetKeyName(2, "Thief3_16.png");
            GameTabsImageList.Images.SetKeyName(3, "Shock2_16.png");
            // 
            // ScanAllFMsButton
            // 
            ScanAllFMsButton.AutoSize = true;
            ScanAllFMsButton.Margin = new Padding(11, 3, 3, 3);
            ScanAllFMsButton.Padding = new Padding(33, 0, 6, 0);
            ScanAllFMsButton.Height = 36;
            ScanAllFMsButton.TabIndex = 59;
            ScanAllFMsButton.UseVisualStyleBackColor = true;
            ScanAllFMsButton.Click += ScanAllFMsButton_Click;
            ScanAllFMsButton.Paint += ScanAllFMsButton_Paint;
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
            // Needs width to be anchored correctly
            BottomRightButtonsFLP.Width = 226;
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
            BottomLeftButtonsFLP.Controls.Add(ScanAllFMsButton);
            BottomLeftButtonsFLP.Controls.Add(WebSearchButton);
            BottomLeftButtonsFLP.Location = new Point(2, 0);
            BottomLeftButtonsFLP.TabIndex = 36;
            BottomLeftButtonsFLP.Paint += BottomLeftButtonsFLP_Paint;
            // 
            // PlayFMButton
            // 
            PlayFMButton.AutoSize = true;
            PlayFMButton.Padding = new Padding(28, 0, 6, 0);
            PlayFMButton.Height = 36;
            PlayFMButton.TabIndex = 56;
            PlayFMButton.UseVisualStyleBackColor = true;
            PlayFMButton.Click += PlayFMButton_Click;
            PlayFMButton.Paint += PlayFMButton_Paint;
            // 
            // PlayOriginalGameButton
            // 
            PlayOriginalGameButton.AutoSize = true;
            PlayOriginalGameButton.Image = Resources.Play_Original_24;
            PlayOriginalGameButton.ImageAlign = ContentAlignment.MiddleLeft;
            PlayOriginalGameButton.Padding = new Padding(6, 0, 6, 0);
            PlayOriginalGameButton.Height = 36;
            PlayOriginalGameButton.TabIndex = 57;
            PlayOriginalGameButton.TextImageRelation = TextImageRelation.ImageBeforeText;
            PlayOriginalGameButton.UseVisualStyleBackColor = true;
            PlayOriginalGameButton.Click += PlayOriginalGameButton_Click;
            // 
            // 
            // WebSearchButton
            // 
            WebSearchButton.AutoSize = true;
            WebSearchButton.Margin = new Padding(11, 3, 3, 3);
            WebSearchButton.Padding = new Padding(33, 0, 6, 0);
            WebSearchButton.Height = 36;
            WebSearchButton.TabIndex = 60;
            WebSearchButton.UseVisualStyleBackColor = true;
            WebSearchButton.Click += WebSearchButton_Click;
            WebSearchButton.Paint += WebSearchButton_Paint;
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
            MainSplitContainer.Panel2.Controls.Add(ReadmeFullScreenButton);
            MainSplitContainer.Panel2.Controls.Add(ReadmeZoomInButton);
            MainSplitContainer.Panel2.Controls.Add(ReadmeZoomOutButton);
            MainSplitContainer.Panel2.Controls.Add(ReadmeResetZoomButton);
            MainSplitContainer.Panel2.Controls.Add(ChooseReadmeComboBox);
            MainSplitContainer.Panel2.Controls.Add(ReadmeRichTextBox);
            MainSplitContainer.Panel2.MouseLeave += ReadmeArea_MouseLeave;
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
            TopSplitContainer.Size = new Size(1671, 309);
            TopSplitContainer.SplitterDistance = 1116;
            TopSplitContainer.TabIndex = 0;
            // 
            // FilterBarScrollRightButton
            // 
            FilterBarScrollRightButton.FlatStyle = FlatStyle.Flat;
            FilterBarScrollRightButton.ArrowDirection = Direction.Right;
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
            // I may want to enable this in the future - less contrasty background color for DataGridView
            /*
            FMsDGV.BackgroundColor = SystemColors.Control;
            FMsDGV.EnableHeadersVisualStyles = false;
            FMsDGV.ColumnHeadersDefaultCellStyle.SelectionBackColor = SystemColors.Menu;
            FMsDGV.ColumnHeadersDefaultCellStyle.SelectionForeColor = SystemColors.Menu;
            FMsDGV.ColumnHeadersDefaultCellStyle.Padding = new Padding(1);
            */
            FMsDGV.CellDoubleClick += FMsDGV_CellDoubleClick;
            FMsDGV.CellValueNeeded += FMsDGV_CellValueNeeded_Initial;
            FMsDGV.ColumnHeaderMouseClick += FMsDGV_ColumnHeaderMouseClick;
            FMsDGV.RowPrePaint += FMsDGV_RowPrePaint;
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
            FilterBarFLP.Controls.Add(FilterTitleLabel);
            FilterBarFLP.Controls.Add(FilterTitleTextBox);
            FilterBarFLP.Controls.Add(FilterAuthorLabel);
            FilterBarFLP.Controls.Add(FilterAuthorTextBox);
            FilterBarFLP.Controls.Add(FilterIconButtonsToolStrip);
            // PERF: The filter bar gets its x-pos and width set in GameTypeChange() and its y-pos is always 0.
            // Height is 100 so it goes behind the DataGridView and its actual scroll bars will be hidden but
            // they'll still function, and you can use your mousewheel or the custom arrow buttons to scroll.
            // 2019-07-15: It has to have its designer-generated width set here or else the left scroll button
            // doesn't appear. Meh?
            FilterBarFLP.Size = new Size(768, 100);

            FilterBarFLP.TabIndex = 11;
            FilterBarFLP.WrapContents = false;
            FilterBarFLP.Scroll += FiltersFlowLayoutPanel_Scroll;
            FilterBarFLP.SizeChanged += FiltersFlowLayoutPanel_SizeChanged;
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
            FilterByThief1Button.Image = Images.Thief1_21;
            FilterByThief1Button.Margin = new Padding(0);
            FilterByThief1Button.Size = new Size(25, 25);
            FilterByThief1Button.Click += FilterByGameCheckButtons_Click;
            // 
            // FilterByThief2Button
            // 
            FilterByThief2Button.AutoSize = false;
            FilterByThief2Button.CheckOnClick = true;
            FilterByThief2Button.DisplayStyle = ToolStripItemDisplayStyle.Image;
            FilterByThief2Button.Image = Images.Thief2_21;
            FilterByThief2Button.Margin = new Padding(0);
            FilterByThief2Button.Size = new Size(25, 25);
            FilterByThief2Button.Click += FilterByGameCheckButtons_Click;
            // 
            // FilterByThief3Button
            // 
            FilterByThief3Button.AutoSize = false;
            FilterByThief3Button.CheckOnClick = true;
            FilterByThief3Button.DisplayStyle = ToolStripItemDisplayStyle.Image;
            FilterByThief3Button.Image = Images.Thief3_21;
            FilterByThief3Button.Margin = new Padding(0);
            FilterByThief3Button.Size = new Size(25, 25);
            FilterByThief3Button.Click += FilterByGameCheckButtons_Click;
            // 
            // TagsTabAutoScrollMarker
            // 
            TagsTabAutoScrollMarker.Size = new Size(240, 152);
            // 
            // FilterBySS2Button
            // 
            FilterBySS2Button.AutoSize = false;
            FilterBySS2Button.CheckOnClick = true;
            FilterBySS2Button.DisplayStyle = ToolStripItemDisplayStyle.Image;
            FilterBySS2Button.Image = Images.Shock2_21;
            // Extra 2 padding on the right: Fix slight visual glitch on the right side
            FilterBySS2Button.Margin = new Padding(0, 0, 2, 0);
            FilterBySS2Button.Size = new Size(25, 25);
            FilterBySS2Button.Click += FilterByGameCheckButtons_Click;
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
            FilterTitleTextBox.TextChanged += FilterTextBoxes_TextChanged;
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
            FilterByReleaseDateButton,
            FilterByLastPlayedButton,
            FilterByTagsButton,
            FilterByFinishedButton,
            FilterByUnfinishedButton,
            FilterByRatingButton,
            FilterShowUnsupportedButton,
            FilterShowRecentAtTopButton});
            FilterIconButtonsToolStrip.LayoutStyle = ToolStripLayoutStyle.Flow;
            FilterIconButtonsToolStrip.TabIndex = 3;
            FilterIconButtonsToolStrip.Paint += FilterIconButtonsToolStrip_Paint;
            // 
            // FilterByReleaseDateButton
            // 
            FilterByReleaseDateButton.AutoSize = false;
            FilterByReleaseDateButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
            FilterByReleaseDateButton.Image = Resources.FilterByReleaseDate;
            FilterByReleaseDateButton.Margin = new Padding(6, 0, 0, 0);
            FilterByReleaseDateButton.Size = new Size(25, 25);
            FilterByReleaseDateButton.Click += FilterByReleaseDateButton_Click;
            // 
            // FilterByLastPlayedButton
            // 
            FilterByLastPlayedButton.AutoSize = false;
            FilterByLastPlayedButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
            FilterByLastPlayedButton.Image = Resources.FilterByLastPlayed;
            FilterByLastPlayedButton.Margin = new Padding(6, 0, 0, 0);
            FilterByLastPlayedButton.Size = new Size(25, 25);
            FilterByLastPlayedButton.Click += FilterByLastPlayedButton_Click;
            // 
            // FilterByTagsButton
            // 
            FilterByTagsButton.AutoSize = false;
            FilterByTagsButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
            FilterByTagsButton.Image = Resources.FilterByTags;
            FilterByTagsButton.Margin = new Padding(6, 0, 0, 0);
            FilterByTagsButton.Size = new Size(25, 25);
            FilterByTagsButton.Click += FilterByTagsButton_Click;
            // 
            // FilterByFinishedButton
            // 
            FilterByFinishedButton.AutoSize = false;
            FilterByFinishedButton.CheckOnClick = true;
            FilterByFinishedButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
            FilterByFinishedButton.Image = Resources.FilterByFinished;
            FilterByFinishedButton.Margin = new Padding(6, 0, 0, 0);
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
            // FilterByRatingButton
            // 
            FilterByRatingButton.AutoSize = false;
            FilterByRatingButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
            FilterByRatingButton.Image = Resources.FilterByRating;
            FilterByRatingButton.Margin = new Padding(6, 0, 0, 0);
            FilterByRatingButton.Size = new Size(25, 25);
            FilterByRatingButton.Click += FilterByRatingButton_Click;
            // 
            // FilterShowUnsupportedButton
            // 
            FilterShowUnsupportedButton.AutoSize = false;
            FilterShowUnsupportedButton.CheckOnClick = true;
            FilterShowUnsupportedButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
            FilterShowUnsupportedButton.Image = Resources.Show_Unsupported;
            FilterShowUnsupportedButton.Margin = new Padding(6, 0, 0, 0);
            FilterShowUnsupportedButton.Size = new Size(25, 25);
            FilterShowUnsupportedButton.Click += FilterShowUnsupportedButton_Click;
            // 
            // FilterShowRecentAtTopButton
            // 
            FilterShowRecentAtTopButton.AutoSize = false;
            FilterShowRecentAtTopButton.CheckOnClick = true;
            FilterShowRecentAtTopButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
            FilterShowRecentAtTopButton.Image = Resources.FilterShowRecentAtTop;
            FilterShowRecentAtTopButton.Margin = new Padding(6, 0, 2, 0);
            FilterShowRecentAtTopButton.Size = new Size(25, 25);
            FilterShowRecentAtTopButton.Click += FilterShowRecentAtTopButton_Click;
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
            RefreshAreaToolStrip.Location = new Point(919, 0);
            RefreshAreaToolStrip.Size = new Size(166, 26);
            RefreshAreaToolStrip.TabIndex = 12;
            RefreshAreaToolStrip.Paint += RefreshAreaToolStrip_Paint;
            // 
            // RefreshFromDiskButton
            // 
            RefreshFromDiskButton.AutoSize = false;
            RefreshFromDiskButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
            RefreshFromDiskButton.Image = Resources.FindNewFMs_21;
            RefreshFromDiskButton.Margin = new Padding(0);
            RefreshFromDiskButton.Size = new Size(25, 25);
            RefreshFromDiskButton.Click += RefreshFromDiskButton_Click;
            RefreshFromDiskButton.Margin = new Padding(6, 0, 0, 0);
            // 
            // RefreshFiltersButton
            // 
            RefreshFiltersButton.AutoSize = false;
            RefreshFiltersButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
            RefreshFiltersButton.Image = Images.Refresh;
            RefreshFiltersButton.Margin = new Padding(0);
            RefreshFiltersButton.Size = new Size(25, 25);
            RefreshFiltersButton.Click += RefreshFiltersButton_Click;
            // 
            // ClearFiltersButton
            // 
            ClearFiltersButton.AutoSize = false;
            ClearFiltersButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
            ClearFiltersButton.Image = Resources.ClearFilters;
            // 1 pixel to the bottom margin to prevent the bottom from getting cut off for some reason
            ClearFiltersButton.Margin = new Padding(0, 0, 9, 1);
            ClearFiltersButton.Size = new Size(25, 25);
            ClearFiltersButton.Click += ClearFiltersButton_Click;
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
            ResetLayoutButton.Paint += ResetLayoutButton_Paint;
            // 
            // GamesTabControl
            // 
            GamesTabControl.Controls.Add(Thief1TabPage);
            GamesTabControl.Controls.Add(Thief2TabPage);
            GamesTabControl.Controls.Add(Thief3TabPage);
            GamesTabControl.Controls.Add(SS2TabPage);
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
            TopRightMenuButton.Location = new Point(534, 2);
            TopRightMenuButton.Size = new Size(16, 16);
            TopRightMenuButton.TabIndex = 13;
            TopRightMenuButton.UseVisualStyleBackColor = true;
            TopRightMenuButton.Click += TopRightMenuButton_Click;
            TopRightMenuButton.Paint += TopRightMenuButton_Paint;
            // 
            // TopRightCollapseButton
            // 
            TopRightCollapseButton.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right;
            TopRightCollapseButton.ArrowDirection = Direction.Right;
            TopRightCollapseButton.BackgroundImageLayout = ImageLayout.None;
            TopRightCollapseButton.FlatAppearance.BorderSize = 0;
            TopRightCollapseButton.FlatStyle = FlatStyle.Flat;
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
            StatsScanCustomResourcesButton.Location = new Point(6, 200);
            StatsScanCustomResourcesButton.Padding = new Padding(13, 0, 0, 0);
            StatsScanCustomResourcesButton.Height = 23;
            StatsScanCustomResourcesButton.TabIndex = 12;
            StatsScanCustomResourcesButton.UseVisualStyleBackColor = true;
            StatsScanCustomResourcesButton.Click += RescanCustomResourcesButton_Click;
            StatsScanCustomResourcesButton.Paint += ScanIconButtons_Paint;
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
            // EditFMLanguageLabel
            // 
            EditFMLanguageLabel.AutoSize = true;
            EditFMLanguageLabel.Location = new Point(8, 246);
            EditFMLanguageLabel.TabIndex = 31;
            // 
            // EditFMLanguageComboBox
            // 
            EditFMLanguageComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            EditFMLanguageComboBox.FormattingEnabled = true;
            EditFMLanguageComboBox.Location = new Point(9, 262);
            EditFMLanguageComboBox.Size = new Size(128, 21);
            EditFMLanguageComboBox.TabIndex = 32;
            EditFMLanguageComboBox.SelectedIndexChanged += EditFMLanguageComboBox_SelectedIndexChanged;
            // 
            // EditFMScanLanguagesButton
            // 
            EditFMScanLanguagesButton.Location = new Point(137, 261);
            EditFMScanLanguagesButton.Size = new Size(22, 23);
            EditFMScanLanguagesButton.TabIndex = 33;
            EditFMScanLanguagesButton.UseVisualStyleBackColor = true;
            EditFMScanLanguagesButton.Click += EditFMScanLanguagesButton_Click;
            EditFMScanLanguagesButton.Paint += ScanIconButtons_Paint;
            // 
            // EditFMScanForReadmesButton
            // 
            EditFMScanForReadmesButton.AutoSize = true;
            EditFMScanForReadmesButton.Location = new Point(8, 299);
            EditFMScanForReadmesButton.Padding = new Padding(13, 0, 0, 0);
            EditFMScanForReadmesButton.Height = 23;
            EditFMScanForReadmesButton.TabIndex = 34;
            EditFMScanForReadmesButton.UseVisualStyleBackColor = true;
            EditFMScanForReadmesButton.Click += EditFMScanForReadmesButton_Click;
            EditFMScanForReadmesButton.Paint += ScanIconButtons_Paint;
            // 
            // EditFMScanReleaseDateButton
            // 
            EditFMScanReleaseDateButton.Location = new Point(136, 105);
            EditFMScanReleaseDateButton.Size = new Size(22, 22);
            EditFMScanReleaseDateButton.TabIndex = 22;
            EditFMScanReleaseDateButton.UseVisualStyleBackColor = true;
            EditFMScanReleaseDateButton.Click += EditFMScanReleaseDateButton_Click;
            EditFMScanReleaseDateButton.Paint += ScanIconButtons_Paint;
            // 
            // EditFMScanAuthorButton
            // 
            EditFMScanAuthorButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            EditFMScanAuthorButton.Location = new Point(493, 63);
            EditFMScanAuthorButton.Size = new Size(22, 22);
            EditFMScanAuthorButton.TabIndex = 19;
            EditFMScanAuthorButton.UseVisualStyleBackColor = true;
            EditFMScanAuthorButton.Click += EditFMScanAuthorButton_Click;
            EditFMScanAuthorButton.Paint += ScanIconButtons_Paint;
            // 
            // EditFMScanTitleButton
            // 
            EditFMScanTitleButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            EditFMScanTitleButton.Location = new Point(493, 23);
            EditFMScanTitleButton.Size = new Size(22, 22);
            EditFMScanTitleButton.TabIndex = 16;
            EditFMScanTitleButton.UseVisualStyleBackColor = true;
            EditFMScanTitleButton.Click += EditFMScanTitleButton_Click;
            EditFMScanTitleButton.Paint += ScanIconButtons_Paint;
            // 
            // EditFMAltTitlesDropDownButton
            // 
            EditFMAltTitlesArrowButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            EditFMAltTitlesArrowButton.ArrowDirection = Direction.Down;
            EditFMAltTitlesArrowButton.Location = new Point(477, 23);
            EditFMAltTitlesArrowButton.Size = new Size(17, 22);
            EditFMAltTitlesArrowButton.TabIndex = 15;
            EditFMAltTitlesArrowButton.UseVisualStyleBackColor = true;
            EditFMAltTitlesArrowButton.Click += EditFMAltTitlesArrowButtonClick;
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
            TagsTabPage.AutoScroll = true;
            TagsTabPage.BackColor = SystemColors.Control;
            TagsTabPage.Controls.Add(AddTagButton);
            TagsTabPage.Controls.Add(AddTagTextBox);
            TagsTabPage.Controls.Add(AddRemoveTagFLP);
            TagsTabPage.Controls.Add(TagsTreeView);
            TagsTabPage.Controls.Add(TagsTabAutoScrollMarker);
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
            AddRemoveTagFLP.Location = new Point(0, 248);
            AddRemoveTagFLP.Size = new Size(525, 24);
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
            PatchRemoveDMLButton.Location = new Point(122, 94);
            PatchRemoveDMLButton.Size = new Size(23, 23);
            PatchRemoveDMLButton.TabIndex = 42;
            PatchRemoveDMLButton.UseVisualStyleBackColor = true;
            PatchRemoveDMLButton.Click += PatchRemoveDMLButton_Click;
            PatchRemoveDMLButton.Paint += PatchRemoveDMLButton_Paint;
            // 
            // PatchAddDMLButton
            // 
            PatchAddDMLButton.Location = new Point(146, 94);
            PatchAddDMLButton.Size = new Size(23, 23);
            PatchAddDMLButton.TabIndex = 43;
            PatchAddDMLButton.UseVisualStyleBackColor = true;
            PatchAddDMLButton.Click += PatchAddDMLButton_Click;
            PatchAddDMLButton.Paint += PatchAddDMLButton_Paint;
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

            #region Readme control buttons

            // BackColor for these gets set in ShowReadme()

            // 
            // ReadmeFullScreenButton
            // 
            ReadmeFullScreenButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            ReadmeFullScreenButton.FlatAppearance.BorderSize = 0;
            ReadmeFullScreenButton.FlatStyle = FlatStyle.Flat;
            ReadmeFullScreenButton.Location = new Point(1616, 8);
            ReadmeFullScreenButton.Size = new Size(20, 20);
            ReadmeFullScreenButton.TabIndex = 55;
            ReadmeFullScreenButton.UseVisualStyleBackColor = false;
            ReadmeFullScreenButton.Visible = false;
            ReadmeFullScreenButton.Click += ReadmeFullScreenButton_Click;
            ReadmeFullScreenButton.Paint += ReadmeFullScreenButton_Paint;
            ReadmeFullScreenButton.MouseLeave += ReadmeArea_MouseLeave;
            // 
            // ZoomInButton
            // 
            ReadmeZoomInButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            ReadmeZoomInButton.BackgroundImage = Images.ZoomIn;
            ReadmeZoomInButton.BackgroundImageLayout = ImageLayout.Zoom;
            ReadmeZoomInButton.FlatAppearance.BorderSize = 0;
            ReadmeZoomInButton.FlatStyle = FlatStyle.Flat;
            ReadmeZoomInButton.Location = new Point(1534, 8);
            ReadmeZoomInButton.Size = new Size(20, 20);
            ReadmeZoomInButton.TabIndex = 52;
            ReadmeZoomInButton.UseVisualStyleBackColor = false;
            ReadmeZoomInButton.Visible = false;
            ReadmeZoomInButton.Click += ReadmeZoomInButton_Click;
            ReadmeZoomInButton.MouseLeave += ReadmeArea_MouseLeave;
            // 
            // ZoomOutButton
            // 
            ReadmeZoomOutButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            ReadmeZoomOutButton.BackColor = SystemColors.Window;
            ReadmeZoomOutButton.BackgroundImage = Images.ZoomOut;
            ReadmeZoomOutButton.BackgroundImageLayout = ImageLayout.Zoom;
            ReadmeZoomOutButton.FlatAppearance.BorderSize = 0;
            ReadmeZoomOutButton.FlatStyle = FlatStyle.Flat;
            ReadmeZoomOutButton.Location = new Point(1558, 8);
            ReadmeZoomOutButton.Size = new Size(20, 20);
            ReadmeZoomOutButton.TabIndex = 53;
            ReadmeZoomOutButton.UseVisualStyleBackColor = false;
            ReadmeZoomOutButton.Visible = false;
            ReadmeZoomOutButton.Click += ReadmeZoomOutButton_Click;
            ReadmeZoomOutButton.MouseLeave += ReadmeArea_MouseLeave;
            // 
            // ResetZoomButton
            // 
            ReadmeResetZoomButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            ReadmeResetZoomButton.BackgroundImage = Images.ZoomReset;
            ReadmeResetZoomButton.BackgroundImageLayout = ImageLayout.Zoom;
            ReadmeResetZoomButton.FlatAppearance.BorderSize = 0;
            ReadmeResetZoomButton.FlatStyle = FlatStyle.Flat;
            ReadmeResetZoomButton.Location = new Point(1582, 8);
            ReadmeResetZoomButton.Size = new Size(20, 20);
            ReadmeResetZoomButton.TabIndex = 54;
            ReadmeResetZoomButton.UseVisualStyleBackColor = false;
            ReadmeResetZoomButton.Visible = false;
            ReadmeResetZoomButton.Click += ReadmeResetZoomButton_Click;
            ReadmeResetZoomButton.MouseLeave += ReadmeArea_MouseLeave;

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
            ChooseReadmeComboBox.MouseLeave += ReadmeArea_MouseLeave;
            // 
            // ReadmeRichTextBox
            // 
            ReadmeRichTextBox.BackColor = SystemColors.Window;
            ReadmeRichTextBox.ReadOnly = true;
            ReadmeRichTextBox.Dock = DockStyle.Fill;
            ReadmeRichTextBox.TabIndex = 0;
            ReadmeRichTextBox.LinkClicked += ReadmeRichTextBox_LinkClicked;
            ReadmeRichTextBox.MouseLeave += ReadmeArea_MouseLeave;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            // NOTE: Keeping this in just in case... it takes <1ms to do anyway
            ClientSize = new Size(1671, 716);
            AutoScaleMode = AutoScaleMode.Dpi;
            Controls.Add(EverythingPanel);
            DoubleBuffered = true;
            Icon = Images.AngelLoader;
            MinimumSize = new Size(894, 260);
            Deactivate += MainForm_Deactivate;
            FormClosing += MainForm_FormClosing;
            Load += MainForm_Load;
            Shown += MainForm_Shown;
            LocationChanged += MainForm_LocationChanged;
            SizeChanged += MainForm_SizeChanged;
            KeyDown += MainForm_KeyDown;

            #endregion

            // PERF: Some of these we resume in Localize(), so we don't want to do duplicate work
            // PERF_NOTE: ResumeLayouts: ~4ms

            BottomPanel.ResumeLayout();

            /* Deferred
                BottomRightButtonsFLP.ResumeLayout();
                BottomLeftButtonsFLP.ResumeLayout();
            */

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
            FilterGameButtonsToolStrip.ResumeLayout();
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

            ResumeLayout();
        }
    }
}
