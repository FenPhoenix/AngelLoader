namespace FMInfoGen
{
    internal sealed partial class MainForm
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.FMsListBox = new System.Windows.Forms.ListBox();
            this.SetFMsFolderButton = new System.Windows.Forms.Button();
            this.ExtractFMArchiveButton = new System.Windows.Forms.Button();
            this.TempPathTextBox = new System.Windows.Forms.TextBox();
            this.SetTempPathButton = new System.Windows.Forms.Button();
            this.ExtractedPathListBox = new System.Windows.Forms.ListBox();
            this.ExtractAllFMArchivesButton = new System.Windows.Forms.Button();
            this.ExtractProgressBar = new System.Windows.Forms.ProgressBar();
            this.CancelExtractAllButton = new System.Windows.Forms.Button();
            this.MainTab_ListsPanel = new System.Windows.Forms.Panel();
            this.QuickSetLabel = new System.Windows.Forms.Label();
            this.SetFMsFolderTo7zTestButton = new System.Windows.Forms.Button();
            this.SetFMsFolderToT3Button = new System.Windows.Forms.Button();
            this.SetFMsFolderToSS2Button = new System.Windows.Forms.Button();
            this.SetFMsFolderTo1098Button = new System.Windows.Forms.Button();
            this.OverwriteFoldersCheckBox = new System.Windows.Forms.CheckBox();
            this.GetOneFromZipButton = new System.Windows.Forms.Button();
            this.MainTabControl = new System.Windows.Forms.TabControl();
            this.MainTabPage = new System.Windows.Forms.TabPage();
            this.FastManualDiffButton = new System.Windows.Forms.Button();
            this.FMScanProgressBar = new System.Windows.Forms.ProgressBar();
            this.CancelFMScanButton = new System.Windows.Forms.Button();
            this.ScanOptionsGroupBox = new System.Windows.Forms.GroupBox();
            this.ScanReleaseDateCheckBox = new System.Windows.Forms.CheckBox();
            this.ScanDescriptionCheckBox = new System.Windows.Forms.CheckBox();
            this.ScanTagsCheckBox = new System.Windows.Forms.CheckBox();
            this.ScanSizeCheckBox = new System.Windows.Forms.CheckBox();
            this.ScanNewDarkRequiredCheckBox = new System.Windows.Forms.CheckBox();
            this.ScanTitleCheckBox = new System.Windows.Forms.CheckBox();
            this.ScanCustomResourcesCheckBox = new System.Windows.Forms.CheckBox();
            this.ScanAuthorCheckBox = new System.Windows.Forms.CheckBox();
            this.ScanNDMinVerCheckBox = new System.Windows.Forms.CheckBox();
            this.ScanIncludedMissionsCheckBox = new System.Windows.Forms.CheckBox();
            this.ScanGameTypeCheckBox = new System.Windows.Forms.CheckBox();
            this.ScanVersionCheckBox = new System.Windows.Forms.CheckBox();
            this.ScanLanguagesCheckBox = new System.Windows.Forms.CheckBox();
            this.Test2Button = new System.Windows.Forms.Button();
            this.TestButton = new System.Windows.Forms.Button();
            this.DebugLogTextBox = new System.Windows.Forms.TextBox();
            this.ScanControlsGroupBox = new System.Windows.Forms.GroupBox();
            this.GetOneFromFolderButton = new System.Windows.Forms.Button();
            this.GetFromZipsButton = new System.Windows.Forms.Button();
            this.GetFromFoldersButton = new System.Windows.Forms.Button();
            this.DebugLabel = new System.Windows.Forms.Label();
            this.AccuracyTabPage = new System.Windows.Forms.TabPage();
            this.AccuracyCheckPanel = new System.Windows.Forms.Panel();
            this.TitleCheckBox = new System.Windows.Forms.CheckBox();
            this.MoviesCheckBox = new System.Windows.Forms.CheckBox();
            this.AuthorCheckBox = new System.Windows.Forms.CheckBox();
            this.MapCheckBox = new System.Windows.Forms.CheckBox();
            this.VersionCheckBox = new System.Windows.Forms.CheckBox();
            this.AutomapCheckBox = new System.Windows.Forms.CheckBox();
            this.LanguagesCheckBox = new System.Windows.Forms.CheckBox();
            this.SubtitlesCheckBox = new System.Windows.Forms.CheckBox();
            this.GameCheckBox = new System.Windows.Forms.CheckBox();
            this.MotionsCheckBox = new System.Windows.Forms.CheckBox();
            this.NewDarkCheckBox = new System.Windows.Forms.CheckBox();
            this.CreaturesCheckBox = new System.Windows.Forms.CheckBox();
            this.NewDarkVerReqCheckBox = new System.Windows.Forms.CheckBox();
            this.ObjectsCheckBox = new System.Windows.Forms.CheckBox();
            this.TypeCheckBox = new System.Windows.Forms.CheckBox();
            this.SoundsCheckBox = new System.Windows.Forms.CheckBox();
            this.ScriptsCheckBox = new System.Windows.Forms.CheckBox();
            this.TexturesCheckBox = new System.Windows.Forms.CheckBox();
            this.CountButton = new System.Windows.Forms.Button();
            this.FMInfoTextBox = new System.Windows.Forms.TextBox();
            this.OpenFMFolderButton = new System.Windows.Forms.Button();
            this.OpenYamlButton = new System.Windows.Forms.Button();
            this.PopulateFMInfoListButton = new System.Windows.Forms.Button();
            this.FMInfoFilesListView = new System.Windows.Forms.ListView();
            this.TitleColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.ArchiveNameColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.AuthorColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.VersionColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.LanguagesColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.GameColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.NewDarkColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.NewDarkVersionRequiredColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.ReleaseDateColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.LastUpdatedColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.TypeColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.HasCustomScriptsColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.HasCustomTexturesColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.HasCustomSoundsColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.HasCustomObjectsColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.HasCustomCreaturesColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.HasCustomMotionsColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.HasAutomapColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.HasMapColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.HasMoviesColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.HasCustomSubtitlesColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.SizeColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.MainTab_ListsPanel.SuspendLayout();
            this.MainTabControl.SuspendLayout();
            this.MainTabPage.SuspendLayout();
            this.ScanOptionsGroupBox.SuspendLayout();
            this.ScanControlsGroupBox.SuspendLayout();
            this.AccuracyTabPage.SuspendLayout();
            this.AccuracyCheckPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // FMsListBox
            // 
            this.FMsListBox.FormattingEnabled = true;
            this.FMsListBox.Location = new System.Drawing.Point(0, 0);
            this.FMsListBox.MultiColumn = true;
            this.FMsListBox.Name = "FMsListBox";
            this.FMsListBox.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.FMsListBox.Size = new System.Drawing.Size(752, 277);
            this.FMsListBox.TabIndex = 0;
            // 
            // SetFMsFolderButton
            // 
            this.SetFMsFolderButton.Location = new System.Drawing.Point(-1, 277);
            this.SetFMsFolderButton.Name = "SetFMsFolderButton";
            this.SetFMsFolderButton.Size = new System.Drawing.Size(104, 23);
            this.SetFMsFolderButton.TabIndex = 2;
            this.SetFMsFolderButton.Text = "Set FMs folder...";
            this.SetFMsFolderButton.UseVisualStyleBackColor = true;
            this.SetFMsFolderButton.Click += new System.EventHandler(this.SetFMsFolderButton_Click);
            // 
            // ExtractFMArchiveButton
            // 
            this.ExtractFMArchiveButton.Location = new System.Drawing.Point(759, 277);
            this.ExtractFMArchiveButton.Name = "ExtractFMArchiveButton";
            this.ExtractFMArchiveButton.Size = new System.Drawing.Size(114, 23);
            this.ExtractFMArchiveButton.TabIndex = 3;
            this.ExtractFMArchiveButton.Text = "Extract FM archive";
            this.ExtractFMArchiveButton.UseVisualStyleBackColor = true;
            this.ExtractFMArchiveButton.Click += new System.EventHandler(this.ExtractFMArchiveButton_Click);
            // 
            // TempPathTextBox
            // 
            this.TempPathTextBox.Location = new System.Drawing.Point(0, 305);
            this.TempPathTextBox.Name = "TempPathTextBox";
            this.TempPathTextBox.Size = new System.Drawing.Size(655, 20);
            this.TempPathTextBox.TabIndex = 4;
            this.TempPathTextBox.TextChanged += new System.EventHandler(this.TempPathTextBox_TextChanged);
            // 
            // SetTempPathButton
            // 
            this.SetTempPathButton.Location = new System.Drawing.Point(655, 304);
            this.SetTempPathButton.Name = "SetTempPathButton";
            this.SetTempPathButton.Size = new System.Drawing.Size(98, 22);
            this.SetTempPathButton.TabIndex = 5;
            this.SetTempPathButton.Text = "Set temp folder...";
            this.SetTempPathButton.UseVisualStyleBackColor = true;
            this.SetTempPathButton.Click += new System.EventHandler(this.SetTempPathButton_Click);
            // 
            // ExtractedPathListBox
            // 
            this.ExtractedPathListBox.FormattingEnabled = true;
            this.ExtractedPathListBox.Location = new System.Drawing.Point(760, 0);
            this.ExtractedPathListBox.MultiColumn = true;
            this.ExtractedPathListBox.Name = "ExtractedPathListBox";
            this.ExtractedPathListBox.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.ExtractedPathListBox.Size = new System.Drawing.Size(456, 277);
            this.ExtractedPathListBox.TabIndex = 0;
            // 
            // ExtractAllFMArchivesButton
            // 
            this.ExtractAllFMArchivesButton.Location = new System.Drawing.Point(872, 277);
            this.ExtractAllFMArchivesButton.Name = "ExtractAllFMArchivesButton";
            this.ExtractAllFMArchivesButton.Size = new System.Drawing.Size(123, 23);
            this.ExtractAllFMArchivesButton.TabIndex = 6;
            this.ExtractAllFMArchivesButton.Text = "Extract all FM archives";
            this.ExtractAllFMArchivesButton.UseVisualStyleBackColor = true;
            this.ExtractAllFMArchivesButton.Click += new System.EventHandler(this.ExtractAllFMArchivesButton_Click);
            // 
            // ExtractProgressBar
            // 
            this.ExtractProgressBar.Location = new System.Drawing.Point(4, 608);
            this.ExtractProgressBar.Name = "ExtractProgressBar";
            this.ExtractProgressBar.Size = new System.Drawing.Size(752, 23);
            this.ExtractProgressBar.TabIndex = 7;
            this.ExtractProgressBar.Visible = false;
            // 
            // CancelExtractAllButton
            // 
            this.CancelExtractAllButton.Location = new System.Drawing.Point(637, 632);
            this.CancelExtractAllButton.Name = "CancelExtractAllButton";
            this.CancelExtractAllButton.Size = new System.Drawing.Size(120, 23);
            this.CancelExtractAllButton.TabIndex = 8;
            this.CancelExtractAllButton.Text = "Cancel extracting all";
            this.CancelExtractAllButton.UseVisualStyleBackColor = true;
            this.CancelExtractAllButton.Visible = false;
            this.CancelExtractAllButton.Click += new System.EventHandler(this.CancelExtractAllButton_Click);
            // 
            // MainTab_ListsPanel
            // 
            this.MainTab_ListsPanel.Controls.Add(this.QuickSetLabel);
            this.MainTab_ListsPanel.Controls.Add(this.SetFMsFolderTo7zTestButton);
            this.MainTab_ListsPanel.Controls.Add(this.SetFMsFolderToT3Button);
            this.MainTab_ListsPanel.Controls.Add(this.SetFMsFolderToSS2Button);
            this.MainTab_ListsPanel.Controls.Add(this.SetFMsFolderTo1098Button);
            this.MainTab_ListsPanel.Controls.Add(this.OverwriteFoldersCheckBox);
            this.MainTab_ListsPanel.Controls.Add(this.FMsListBox);
            this.MainTab_ListsPanel.Controls.Add(this.ExtractedPathListBox);
            this.MainTab_ListsPanel.Controls.Add(this.ExtractAllFMArchivesButton);
            this.MainTab_ListsPanel.Controls.Add(this.SetFMsFolderButton);
            this.MainTab_ListsPanel.Controls.Add(this.SetTempPathButton);
            this.MainTab_ListsPanel.Controls.Add(this.ExtractFMArchiveButton);
            this.MainTab_ListsPanel.Controls.Add(this.TempPathTextBox);
            this.MainTab_ListsPanel.Location = new System.Drawing.Point(4, 2);
            this.MainTab_ListsPanel.Name = "MainTab_ListsPanel";
            this.MainTab_ListsPanel.Size = new System.Drawing.Size(1408, 334);
            this.MainTab_ListsPanel.TabIndex = 9;
            // 
            // QuickSetLabel
            // 
            this.QuickSetLabel.AutoSize = true;
            this.QuickSetLabel.Location = new System.Drawing.Point(112, 282);
            this.QuickSetLabel.Name = "QuickSetLabel";
            this.QuickSetLabel.Size = new System.Drawing.Size(55, 13);
            this.QuickSetLabel.TabIndex = 9;
            this.QuickSetLabel.Text = "Quick set:";
            // 
            // SetFMsFolderTo7zTestButton
            // 
            this.SetFMsFolderTo7zTestButton.Location = new System.Drawing.Point(407, 277);
            this.SetFMsFolderTo7zTestButton.Name = "SetFMsFolderTo7zTestButton";
            this.SetFMsFolderTo7zTestButton.Size = new System.Drawing.Size(80, 23);
            this.SetFMsFolderTo7zTestButton.TabIndex = 8;
            this.SetFMsFolderTo7zTestButton.Text = "7z test";
            this.SetFMsFolderTo7zTestButton.UseVisualStyleBackColor = true;
            this.SetFMsFolderTo7zTestButton.Click += new System.EventHandler(this.SetFMsFolderTo7zTestButton_Click);
            // 
            // SetFMsFolderToT3Button
            // 
            this.SetFMsFolderToT3Button.Location = new System.Drawing.Point(328, 277);
            this.SetFMsFolderToT3Button.Name = "SetFMsFolderToT3Button";
            this.SetFMsFolderToT3Button.Size = new System.Drawing.Size(80, 23);
            this.SetFMsFolderToT3Button.TabIndex = 8;
            this.SetFMsFolderToT3Button.Text = "T3";
            this.SetFMsFolderToT3Button.UseVisualStyleBackColor = true;
            this.SetFMsFolderToT3Button.Click += new System.EventHandler(this.SetFMsFolderToT3Button_Click);
            // 
            // SetFMsFolderToSS2Button
            // 
            this.SetFMsFolderToSS2Button.Location = new System.Drawing.Point(249, 277);
            this.SetFMsFolderToSS2Button.Name = "SetFMsFolderToSS2Button";
            this.SetFMsFolderToSS2Button.Size = new System.Drawing.Size(80, 23);
            this.SetFMsFolderToSS2Button.TabIndex = 8;
            this.SetFMsFolderToSS2Button.Text = "SS2";
            this.SetFMsFolderToSS2Button.UseVisualStyleBackColor = true;
            this.SetFMsFolderToSS2Button.Click += new System.EventHandler(this.SetFMsFolderToSS2Button_Click);
            // 
            // SetFMsFolderTo1098Button
            // 
            this.SetFMsFolderTo1098Button.Location = new System.Drawing.Point(169, 277);
            this.SetFMsFolderTo1098Button.Name = "SetFMsFolderTo1098Button";
            this.SetFMsFolderTo1098Button.Size = new System.Drawing.Size(80, 23);
            this.SetFMsFolderTo1098Button.TabIndex = 8;
            this.SetFMsFolderTo1098Button.Text = "T1+T2 (1098)";
            this.SetFMsFolderTo1098Button.UseVisualStyleBackColor = true;
            this.SetFMsFolderTo1098Button.Click += new System.EventHandler(this.SetFMsFolderTo1098Button_Click);
            // 
            // OverwriteFoldersCheckBox
            // 
            this.OverwriteFoldersCheckBox.AutoSize = true;
            this.OverwriteFoldersCheckBox.Location = new System.Drawing.Point(1000, 280);
            this.OverwriteFoldersCheckBox.Name = "OverwriteFoldersCheckBox";
            this.OverwriteFoldersCheckBox.Size = new System.Drawing.Size(105, 17);
            this.OverwriteFoldersCheckBox.TabIndex = 7;
            this.OverwriteFoldersCheckBox.Text = "Overwrite folders";
            this.OverwriteFoldersCheckBox.UseVisualStyleBackColor = true;
            // 
            // GetOneFromZipButton
            // 
            this.GetOneFromZipButton.Location = new System.Drawing.Point(16, 48);
            this.GetOneFromZipButton.Name = "GetOneFromZipButton";
            this.GetOneFromZipButton.Size = new System.Drawing.Size(96, 23);
            this.GetOneFromZipButton.TabIndex = 10;
            this.GetOneFromZipButton.Text = "Get 1 from zip";
            this.GetOneFromZipButton.UseVisualStyleBackColor = true;
            this.GetOneFromZipButton.Click += new System.EventHandler(this.GetOneFromZipButton_Click);
            // 
            // MainTabControl
            // 
            this.MainTabControl.Controls.Add(this.MainTabPage);
            this.MainTabControl.Controls.Add(this.AccuracyTabPage);
            this.MainTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MainTabControl.Location = new System.Drawing.Point(0, 0);
            this.MainTabControl.Name = "MainTabControl";
            this.MainTabControl.SelectedIndex = 0;
            this.MainTabControl.Size = new System.Drawing.Size(1556, 740);
            this.MainTabControl.TabIndex = 10;
            // 
            // MainTabPage
            // 
            this.MainTabPage.BackColor = System.Drawing.SystemColors.Control;
            this.MainTabPage.Controls.Add(this.FastManualDiffButton);
            this.MainTabPage.Controls.Add(this.FMScanProgressBar);
            this.MainTabPage.Controls.Add(this.CancelFMScanButton);
            this.MainTabPage.Controls.Add(this.ScanOptionsGroupBox);
            this.MainTabPage.Controls.Add(this.Test2Button);
            this.MainTabPage.Controls.Add(this.TestButton);
            this.MainTabPage.Controls.Add(this.DebugLogTextBox);
            this.MainTabPage.Controls.Add(this.ScanControlsGroupBox);
            this.MainTabPage.Controls.Add(this.DebugLabel);
            this.MainTabPage.Controls.Add(this.MainTab_ListsPanel);
            this.MainTabPage.Controls.Add(this.ExtractProgressBar);
            this.MainTabPage.Controls.Add(this.CancelExtractAllButton);
            this.MainTabPage.Location = new System.Drawing.Point(4, 22);
            this.MainTabPage.Name = "MainTabPage";
            this.MainTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.MainTabPage.Size = new System.Drawing.Size(1548, 714);
            this.MainTabPage.TabIndex = 0;
            this.MainTabPage.Text = "Main";
            // 
            // FastManualDiffButton
            // 
            this.FastManualDiffButton.Location = new System.Drawing.Point(128, 480);
            this.FastManualDiffButton.Name = "FastManualDiffButton";
            this.FastManualDiffButton.Size = new System.Drawing.Size(112, 23);
            this.FastManualDiffButton.TabIndex = 19;
            this.FastManualDiffButton.Text = "Fast manual diff";
            this.FastManualDiffButton.UseVisualStyleBackColor = true;
            this.FastManualDiffButton.Click += new System.EventHandler(this.FastManualDiffButton_Click);
            // 
            // FMScanProgressBar
            // 
            this.FMScanProgressBar.Location = new System.Drawing.Point(352, 480);
            this.FMScanProgressBar.Name = "FMScanProgressBar";
            this.FMScanProgressBar.Size = new System.Drawing.Size(400, 23);
            this.FMScanProgressBar.TabIndex = 18;
            // 
            // CancelFMScanButton
            // 
            this.CancelFMScanButton.Location = new System.Drawing.Point(351, 456);
            this.CancelFMScanButton.Name = "CancelFMScanButton";
            this.CancelFMScanButton.Size = new System.Drawing.Size(96, 23);
            this.CancelFMScanButton.TabIndex = 17;
            this.CancelFMScanButton.Text = "Cancel FM scan";
            this.CancelFMScanButton.UseVisualStyleBackColor = true;
            this.CancelFMScanButton.Click += new System.EventHandler(this.CancelFMScanButton_Click);
            // 
            // ScanOptionsGroupBox
            // 
            this.ScanOptionsGroupBox.Controls.Add(this.ScanReleaseDateCheckBox);
            this.ScanOptionsGroupBox.Controls.Add(this.ScanDescriptionCheckBox);
            this.ScanOptionsGroupBox.Controls.Add(this.ScanTagsCheckBox);
            this.ScanOptionsGroupBox.Controls.Add(this.ScanSizeCheckBox);
            this.ScanOptionsGroupBox.Controls.Add(this.ScanNewDarkRequiredCheckBox);
            this.ScanOptionsGroupBox.Controls.Add(this.ScanTitleCheckBox);
            this.ScanOptionsGroupBox.Controls.Add(this.ScanCustomResourcesCheckBox);
            this.ScanOptionsGroupBox.Controls.Add(this.ScanAuthorCheckBox);
            this.ScanOptionsGroupBox.Controls.Add(this.ScanNDMinVerCheckBox);
            this.ScanOptionsGroupBox.Controls.Add(this.ScanIncludedMissionsCheckBox);
            this.ScanOptionsGroupBox.Controls.Add(this.ScanGameTypeCheckBox);
            this.ScanOptionsGroupBox.Controls.Add(this.ScanVersionCheckBox);
            this.ScanOptionsGroupBox.Controls.Add(this.ScanLanguagesCheckBox);
            this.ScanOptionsGroupBox.Location = new System.Drawing.Point(248, 344);
            this.ScanOptionsGroupBox.Name = "ScanOptionsGroupBox";
            this.ScanOptionsGroupBox.Size = new System.Drawing.Size(504, 104);
            this.ScanOptionsGroupBox.TabIndex = 16;
            this.ScanOptionsGroupBox.TabStop = false;
            this.ScanOptionsGroupBox.Text = "Scan options";
            // 
            // ScanReleaseDateCheckBox
            // 
            this.ScanReleaseDateCheckBox.AutoSize = true;
            this.ScanReleaseDateCheckBox.Checked = true;
            this.ScanReleaseDateCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ScanReleaseDateCheckBox.Location = new System.Drawing.Point(16, 72);
            this.ScanReleaseDateCheckBox.Name = "ScanReleaseDateCheckBox";
            this.ScanReleaseDateCheckBox.Size = new System.Drawing.Size(69, 17);
            this.ScanReleaseDateCheckBox.TabIndex = 18;
            this.ScanReleaseDateCheckBox.Text = "Rel. date";
            this.ScanReleaseDateCheckBox.UseVisualStyleBackColor = true;
            // 
            // ScanDescriptionCheckBox
            // 
            this.ScanDescriptionCheckBox.AutoSize = true;
            this.ScanDescriptionCheckBox.Checked = true;
            this.ScanDescriptionCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ScanDescriptionCheckBox.Enabled = false;
            this.ScanDescriptionCheckBox.Location = new System.Drawing.Point(224, 72);
            this.ScanDescriptionCheckBox.Name = "ScanDescriptionCheckBox";
            this.ScanDescriptionCheckBox.Size = new System.Drawing.Size(54, 17);
            this.ScanDescriptionCheckBox.TabIndex = 18;
            this.ScanDescriptionCheckBox.Text = "Descr";
            this.ScanDescriptionCheckBox.UseVisualStyleBackColor = true;
            // 
            // ScanTagsCheckBox
            // 
            this.ScanTagsCheckBox.AutoSize = true;
            this.ScanTagsCheckBox.Checked = true;
            this.ScanTagsCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ScanTagsCheckBox.Location = new System.Drawing.Point(96, 72);
            this.ScanTagsCheckBox.Name = "ScanTagsCheckBox";
            this.ScanTagsCheckBox.Size = new System.Drawing.Size(50, 17);
            this.ScanTagsCheckBox.TabIndex = 18;
            this.ScanTagsCheckBox.Text = "Tags";
            this.ScanTagsCheckBox.UseVisualStyleBackColor = true;
            // 
            // ScanSizeCheckBox
            // 
            this.ScanSizeCheckBox.AutoSize = true;
            this.ScanSizeCheckBox.Checked = true;
            this.ScanSizeCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ScanSizeCheckBox.Location = new System.Drawing.Point(416, 48);
            this.ScanSizeCheckBox.Name = "ScanSizeCheckBox";
            this.ScanSizeCheckBox.Size = new System.Drawing.Size(46, 17);
            this.ScanSizeCheckBox.TabIndex = 17;
            this.ScanSizeCheckBox.Text = "Size";
            this.ScanSizeCheckBox.UseVisualStyleBackColor = true;
            // 
            // ScanNewDarkRequiredCheckBox
            // 
            this.ScanNewDarkRequiredCheckBox.AutoSize = true;
            this.ScanNewDarkRequiredCheckBox.Checked = true;
            this.ScanNewDarkRequiredCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ScanNewDarkRequiredCheckBox.Enabled = false;
            this.ScanNewDarkRequiredCheckBox.Location = new System.Drawing.Point(96, 48);
            this.ScanNewDarkRequiredCheckBox.Name = "ScanNewDarkRequiredCheckBox";
            this.ScanNewDarkRequiredCheckBox.Size = new System.Drawing.Size(112, 17);
            this.ScanNewDarkRequiredCheckBox.TabIndex = 16;
            this.ScanNewDarkRequiredCheckBox.Text = "NewDark required";
            this.ScanNewDarkRequiredCheckBox.UseVisualStyleBackColor = true;
            // 
            // ScanTitleCheckBox
            // 
            this.ScanTitleCheckBox.AutoSize = true;
            this.ScanTitleCheckBox.Checked = true;
            this.ScanTitleCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ScanTitleCheckBox.Location = new System.Drawing.Point(16, 24);
            this.ScanTitleCheckBox.Name = "ScanTitleCheckBox";
            this.ScanTitleCheckBox.Size = new System.Drawing.Size(46, 17);
            this.ScanTitleCheckBox.TabIndex = 15;
            this.ScanTitleCheckBox.Text = "Title";
            this.ScanTitleCheckBox.UseVisualStyleBackColor = true;
            // 
            // ScanCustomResourcesCheckBox
            // 
            this.ScanCustomResourcesCheckBox.AutoSize = true;
            this.ScanCustomResourcesCheckBox.Checked = true;
            this.ScanCustomResourcesCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ScanCustomResourcesCheckBox.Location = new System.Drawing.Point(304, 48);
            this.ScanCustomResourcesCheckBox.Name = "ScanCustomResourcesCheckBox";
            this.ScanCustomResourcesCheckBox.Size = new System.Drawing.Size(110, 17);
            this.ScanCustomResourcesCheckBox.TabIndex = 15;
            this.ScanCustomResourcesCheckBox.Text = "Custom resources";
            this.ScanCustomResourcesCheckBox.UseVisualStyleBackColor = true;
            // 
            // ScanAuthorCheckBox
            // 
            this.ScanAuthorCheckBox.AutoSize = true;
            this.ScanAuthorCheckBox.Checked = true;
            this.ScanAuthorCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ScanAuthorCheckBox.Location = new System.Drawing.Point(224, 24);
            this.ScanAuthorCheckBox.Name = "ScanAuthorCheckBox";
            this.ScanAuthorCheckBox.Size = new System.Drawing.Size(57, 17);
            this.ScanAuthorCheckBox.TabIndex = 15;
            this.ScanAuthorCheckBox.Text = "Author";
            this.ScanAuthorCheckBox.UseVisualStyleBackColor = true;
            // 
            // ScanNDMinVerCheckBox
            // 
            this.ScanNDMinVerCheckBox.AutoSize = true;
            this.ScanNDMinVerCheckBox.Checked = true;
            this.ScanNDMinVerCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ScanNDMinVerCheckBox.Enabled = false;
            this.ScanNDMinVerCheckBox.Location = new System.Drawing.Point(224, 48);
            this.ScanNDMinVerCheckBox.Name = "ScanNDMinVerCheckBox";
            this.ScanNDMinVerCheckBox.Size = new System.Drawing.Size(73, 17);
            this.ScanNDMinVerCheckBox.TabIndex = 15;
            this.ScanNDMinVerCheckBox.Text = "ND min v.";
            this.ScanNDMinVerCheckBox.UseVisualStyleBackColor = true;
            // 
            // ScanIncludedMissionsCheckBox
            // 
            this.ScanIncludedMissionsCheckBox.AutoSize = true;
            this.ScanIncludedMissionsCheckBox.Checked = true;
            this.ScanIncludedMissionsCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ScanIncludedMissionsCheckBox.Enabled = false;
            this.ScanIncludedMissionsCheckBox.Location = new System.Drawing.Point(96, 24);
            this.ScanIncludedMissionsCheckBox.Name = "ScanIncludedMissionsCheckBox";
            this.ScanIncludedMissionsCheckBox.Size = new System.Drawing.Size(125, 17);
            this.ScanIncludedMissionsCheckBox.TabIndex = 15;
            this.ScanIncludedMissionsCheckBox.Text = "Campaign mis names";
            this.ScanIncludedMissionsCheckBox.UseVisualStyleBackColor = true;
            // 
            // ScanGameTypeCheckBox
            // 
            this.ScanGameTypeCheckBox.AutoSize = true;
            this.ScanGameTypeCheckBox.Checked = true;
            this.ScanGameTypeCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ScanGameTypeCheckBox.Location = new System.Drawing.Point(16, 48);
            this.ScanGameTypeCheckBox.Name = "ScanGameTypeCheckBox";
            this.ScanGameTypeCheckBox.Size = new System.Drawing.Size(77, 17);
            this.ScanGameTypeCheckBox.TabIndex = 15;
            this.ScanGameTypeCheckBox.Text = "Game type";
            this.ScanGameTypeCheckBox.UseVisualStyleBackColor = true;
            // 
            // ScanVersionCheckBox
            // 
            this.ScanVersionCheckBox.AutoSize = true;
            this.ScanVersionCheckBox.Checked = true;
            this.ScanVersionCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ScanVersionCheckBox.Enabled = false;
            this.ScanVersionCheckBox.Location = new System.Drawing.Point(304, 24);
            this.ScanVersionCheckBox.Name = "ScanVersionCheckBox";
            this.ScanVersionCheckBox.Size = new System.Drawing.Size(61, 17);
            this.ScanVersionCheckBox.TabIndex = 15;
            this.ScanVersionCheckBox.Text = "Version";
            this.ScanVersionCheckBox.UseVisualStyleBackColor = true;
            // 
            // ScanLanguagesCheckBox
            // 
            this.ScanLanguagesCheckBox.AutoSize = true;
            this.ScanLanguagesCheckBox.Checked = true;
            this.ScanLanguagesCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ScanLanguagesCheckBox.Enabled = false;
            this.ScanLanguagesCheckBox.Location = new System.Drawing.Point(416, 24);
            this.ScanLanguagesCheckBox.Name = "ScanLanguagesCheckBox";
            this.ScanLanguagesCheckBox.Size = new System.Drawing.Size(79, 17);
            this.ScanLanguagesCheckBox.TabIndex = 15;
            this.ScanLanguagesCheckBox.Text = "Languages";
            this.ScanLanguagesCheckBox.UseVisualStyleBackColor = true;
            // 
            // Test2Button
            // 
            this.Test2Button.Location = new System.Drawing.Point(247, 480);
            this.Test2Button.Name = "Test2Button";
            this.Test2Button.Size = new System.Drawing.Size(97, 23);
            this.Test2Button.TabIndex = 14;
            this.Test2Button.Text = "Test 2";
            this.Test2Button.UseVisualStyleBackColor = true;
            this.Test2Button.Click += new System.EventHandler(this.Test2Button_Click);
            // 
            // TestButton
            // 
            this.TestButton.Location = new System.Drawing.Point(247, 456);
            this.TestButton.Name = "TestButton";
            this.TestButton.Size = new System.Drawing.Size(97, 23);
            this.TestButton.TabIndex = 14;
            this.TestButton.Text = "Test";
            this.TestButton.UseVisualStyleBackColor = true;
            this.TestButton.Click += new System.EventHandler(this.TestButton_Click);
            // 
            // DebugLogTextBox
            // 
            this.DebugLogTextBox.Location = new System.Drawing.Point(760, 344);
            this.DebugLogTextBox.Multiline = true;
            this.DebugLogTextBox.Name = "DebugLogTextBox";
            this.DebugLogTextBox.ReadOnly = true;
            this.DebugLogTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.DebugLogTextBox.Size = new System.Drawing.Size(776, 360);
            this.DebugLogTextBox.TabIndex = 13;
            this.DebugLogTextBox.WordWrap = false;
            // 
            // ScanControlsGroupBox
            // 
            this.ScanControlsGroupBox.Controls.Add(this.GetOneFromZipButton);
            this.ScanControlsGroupBox.Controls.Add(this.GetOneFromFolderButton);
            this.ScanControlsGroupBox.Controls.Add(this.GetFromZipsButton);
            this.ScanControlsGroupBox.Controls.Add(this.GetFromFoldersButton);
            this.ScanControlsGroupBox.Location = new System.Drawing.Point(8, 344);
            this.ScanControlsGroupBox.Name = "ScanControlsGroupBox";
            this.ScanControlsGroupBox.Size = new System.Drawing.Size(232, 128);
            this.ScanControlsGroupBox.TabIndex = 12;
            this.ScanControlsGroupBox.TabStop = false;
            this.ScanControlsGroupBox.Text = "Scan controls";
            // 
            // GetOneFromFolderButton
            // 
            this.GetOneFromFolderButton.Location = new System.Drawing.Point(120, 48);
            this.GetOneFromFolderButton.Name = "GetOneFromFolderButton";
            this.GetOneFromFolderButton.Size = new System.Drawing.Size(96, 23);
            this.GetOneFromFolderButton.TabIndex = 13;
            this.GetOneFromFolderButton.Text = "Get 1 from folder";
            this.GetOneFromFolderButton.UseVisualStyleBackColor = true;
            this.GetOneFromFolderButton.Click += new System.EventHandler(this.GetOneFromFolderButton_Click);
            // 
            // GetFromZipsButton
            // 
            this.GetFromZipsButton.Location = new System.Drawing.Point(16, 24);
            this.GetFromZipsButton.Name = "GetFromZipsButton";
            this.GetFromZipsButton.Size = new System.Drawing.Size(96, 23);
            this.GetFromZipsButton.TabIndex = 11;
            this.GetFromZipsButton.Text = "Get from zips";
            this.GetFromZipsButton.UseVisualStyleBackColor = true;
            this.GetFromZipsButton.Click += new System.EventHandler(this.GetFromZipsButton_Click);
            // 
            // GetFromFoldersButton
            // 
            this.GetFromFoldersButton.Location = new System.Drawing.Point(120, 24);
            this.GetFromFoldersButton.Name = "GetFromFoldersButton";
            this.GetFromFoldersButton.Size = new System.Drawing.Size(96, 23);
            this.GetFromFoldersButton.TabIndex = 11;
            this.GetFromFoldersButton.Text = "Get from folders";
            this.GetFromFoldersButton.UseVisualStyleBackColor = true;
            this.GetFromFoldersButton.Click += new System.EventHandler(this.GetFromFoldersButton_Click);
            // 
            // DebugLabel
            // 
            this.DebugLabel.AutoSize = true;
            this.DebugLabel.Location = new System.Drawing.Point(352, 512);
            this.DebugLabel.Name = "DebugLabel";
            this.DebugLabel.Size = new System.Drawing.Size(71, 13);
            this.DebugLabel.TabIndex = 10;
            this.DebugLabel.Text = "[DebugLabel]";
            // 
            // AccuracyTabPage
            // 
            this.AccuracyTabPage.BackColor = System.Drawing.SystemColors.Control;
            this.AccuracyTabPage.Controls.Add(this.AccuracyCheckPanel);
            this.AccuracyTabPage.Controls.Add(this.CountButton);
            this.AccuracyTabPage.Controls.Add(this.FMInfoTextBox);
            this.AccuracyTabPage.Controls.Add(this.OpenFMFolderButton);
            this.AccuracyTabPage.Controls.Add(this.OpenYamlButton);
            this.AccuracyTabPage.Controls.Add(this.PopulateFMInfoListButton);
            this.AccuracyTabPage.Controls.Add(this.FMInfoFilesListView);
            this.AccuracyTabPage.Location = new System.Drawing.Point(4, 22);
            this.AccuracyTabPage.Name = "AccuracyTabPage";
            this.AccuracyTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.AccuracyTabPage.Size = new System.Drawing.Size(1548, 714);
            this.AccuracyTabPage.TabIndex = 1;
            this.AccuracyTabPage.Text = "Accuracy";
            // 
            // AccuracyCheckPanel
            // 
            this.AccuracyCheckPanel.Controls.Add(this.TitleCheckBox);
            this.AccuracyCheckPanel.Controls.Add(this.MoviesCheckBox);
            this.AccuracyCheckPanel.Controls.Add(this.AuthorCheckBox);
            this.AccuracyCheckPanel.Controls.Add(this.MapCheckBox);
            this.AccuracyCheckPanel.Controls.Add(this.VersionCheckBox);
            this.AccuracyCheckPanel.Controls.Add(this.AutomapCheckBox);
            this.AccuracyCheckPanel.Controls.Add(this.LanguagesCheckBox);
            this.AccuracyCheckPanel.Controls.Add(this.SubtitlesCheckBox);
            this.AccuracyCheckPanel.Controls.Add(this.GameCheckBox);
            this.AccuracyCheckPanel.Controls.Add(this.MotionsCheckBox);
            this.AccuracyCheckPanel.Controls.Add(this.NewDarkCheckBox);
            this.AccuracyCheckPanel.Controls.Add(this.CreaturesCheckBox);
            this.AccuracyCheckPanel.Controls.Add(this.NewDarkVerReqCheckBox);
            this.AccuracyCheckPanel.Controls.Add(this.ObjectsCheckBox);
            this.AccuracyCheckPanel.Controls.Add(this.TypeCheckBox);
            this.AccuracyCheckPanel.Controls.Add(this.SoundsCheckBox);
            this.AccuracyCheckPanel.Controls.Add(this.ScriptsCheckBox);
            this.AccuracyCheckPanel.Controls.Add(this.TexturesCheckBox);
            this.AccuracyCheckPanel.Location = new System.Drawing.Point(136, 360);
            this.AccuracyCheckPanel.Name = "AccuracyCheckPanel";
            this.AccuracyCheckPanel.Size = new System.Drawing.Size(1040, 72);
            this.AccuracyCheckPanel.TabIndex = 17;
            this.AccuracyCheckPanel.Visible = false;
            // 
            // TitleCheckBox
            // 
            this.TitleCheckBox.AutoCheck = false;
            this.TitleCheckBox.Checked = true;
            this.TitleCheckBox.CheckState = System.Windows.Forms.CheckState.Indeterminate;
            this.TitleCheckBox.Location = new System.Drawing.Point(8, 8);
            this.TitleCheckBox.Name = "TitleCheckBox";
            this.TitleCheckBox.Size = new System.Drawing.Size(48, 17);
            this.TitleCheckBox.TabIndex = 0;
            this.TitleCheckBox.Text = "Title";
            this.TitleCheckBox.ThreeState = true;
            this.TitleCheckBox.UseVisualStyleBackColor = true;
            this.TitleCheckBox.CheckStateChanged += new System.EventHandler(this.AccuracyCheckBoxes_CheckStateChanged);
            this.TitleCheckBox.Click += new System.EventHandler(this.AccuracyCheckBoxes_Click);
            // 
            // MoviesCheckBox
            // 
            this.MoviesCheckBox.AutoCheck = false;
            this.MoviesCheckBox.AutoSize = true;
            this.MoviesCheckBox.Checked = true;
            this.MoviesCheckBox.CheckState = System.Windows.Forms.CheckState.Indeterminate;
            this.MoviesCheckBox.Location = new System.Drawing.Point(616, 40);
            this.MoviesCheckBox.Name = "MoviesCheckBox";
            this.MoviesCheckBox.Size = new System.Drawing.Size(60, 17);
            this.MoviesCheckBox.TabIndex = 16;
            this.MoviesCheckBox.Text = "Movies";
            this.MoviesCheckBox.ThreeState = true;
            this.MoviesCheckBox.UseVisualStyleBackColor = true;
            this.MoviesCheckBox.CheckStateChanged += new System.EventHandler(this.AccuracyCheckBoxes_CheckStateChanged);
            this.MoviesCheckBox.Click += new System.EventHandler(this.AccuracyCheckBoxes_Click);
            // 
            // AuthorCheckBox
            // 
            this.AuthorCheckBox.AutoCheck = false;
            this.AuthorCheckBox.AutoSize = true;
            this.AuthorCheckBox.Checked = true;
            this.AuthorCheckBox.CheckState = System.Windows.Forms.CheckState.Indeterminate;
            this.AuthorCheckBox.Location = new System.Drawing.Point(72, 8);
            this.AuthorCheckBox.Name = "AuthorCheckBox";
            this.AuthorCheckBox.Size = new System.Drawing.Size(57, 17);
            this.AuthorCheckBox.TabIndex = 16;
            this.AuthorCheckBox.Text = "Author";
            this.AuthorCheckBox.ThreeState = true;
            this.AuthorCheckBox.UseVisualStyleBackColor = true;
            this.AuthorCheckBox.CheckStateChanged += new System.EventHandler(this.AccuracyCheckBoxes_CheckStateChanged);
            this.AuthorCheckBox.Click += new System.EventHandler(this.AccuracyCheckBoxes_Click);
            // 
            // MapCheckBox
            // 
            this.MapCheckBox.AutoCheck = false;
            this.MapCheckBox.AutoSize = true;
            this.MapCheckBox.Checked = true;
            this.MapCheckBox.CheckState = System.Windows.Forms.CheckState.Indeterminate;
            this.MapCheckBox.Location = new System.Drawing.Point(560, 40);
            this.MapCheckBox.Name = "MapCheckBox";
            this.MapCheckBox.Size = new System.Drawing.Size(47, 17);
            this.MapCheckBox.TabIndex = 16;
            this.MapCheckBox.Text = "Map";
            this.MapCheckBox.ThreeState = true;
            this.MapCheckBox.UseVisualStyleBackColor = true;
            this.MapCheckBox.CheckStateChanged += new System.EventHandler(this.AccuracyCheckBoxes_CheckStateChanged);
            this.MapCheckBox.Click += new System.EventHandler(this.AccuracyCheckBoxes_Click);
            // 
            // VersionCheckBox
            // 
            this.VersionCheckBox.AutoCheck = false;
            this.VersionCheckBox.AutoSize = true;
            this.VersionCheckBox.Checked = true;
            this.VersionCheckBox.CheckState = System.Windows.Forms.CheckState.Indeterminate;
            this.VersionCheckBox.Location = new System.Drawing.Point(152, 8);
            this.VersionCheckBox.Name = "VersionCheckBox";
            this.VersionCheckBox.Size = new System.Drawing.Size(61, 17);
            this.VersionCheckBox.TabIndex = 16;
            this.VersionCheckBox.Text = "Version";
            this.VersionCheckBox.ThreeState = true;
            this.VersionCheckBox.UseVisualStyleBackColor = true;
            this.VersionCheckBox.CheckStateChanged += new System.EventHandler(this.AccuracyCheckBoxes_CheckStateChanged);
            this.VersionCheckBox.Click += new System.EventHandler(this.AccuracyCheckBoxes_Click);
            // 
            // AutomapCheckBox
            // 
            this.AutomapCheckBox.AutoCheck = false;
            this.AutomapCheckBox.AutoSize = true;
            this.AutomapCheckBox.Checked = true;
            this.AutomapCheckBox.CheckState = System.Windows.Forms.CheckState.Indeterminate;
            this.AutomapCheckBox.Location = new System.Drawing.Point(472, 40);
            this.AutomapCheckBox.Name = "AutomapCheckBox";
            this.AutomapCheckBox.Size = new System.Drawing.Size(68, 17);
            this.AutomapCheckBox.TabIndex = 16;
            this.AutomapCheckBox.Text = "Automap";
            this.AutomapCheckBox.ThreeState = true;
            this.AutomapCheckBox.UseVisualStyleBackColor = true;
            this.AutomapCheckBox.CheckStateChanged += new System.EventHandler(this.AccuracyCheckBoxes_CheckStateChanged);
            this.AutomapCheckBox.Click += new System.EventHandler(this.AccuracyCheckBoxes_Click);
            // 
            // LanguagesCheckBox
            // 
            this.LanguagesCheckBox.AutoCheck = false;
            this.LanguagesCheckBox.AutoSize = true;
            this.LanguagesCheckBox.Checked = true;
            this.LanguagesCheckBox.CheckState = System.Windows.Forms.CheckState.Indeterminate;
            this.LanguagesCheckBox.Location = new System.Drawing.Point(224, 8);
            this.LanguagesCheckBox.Name = "LanguagesCheckBox";
            this.LanguagesCheckBox.Size = new System.Drawing.Size(79, 17);
            this.LanguagesCheckBox.TabIndex = 16;
            this.LanguagesCheckBox.Text = "Languages";
            this.LanguagesCheckBox.ThreeState = true;
            this.LanguagesCheckBox.UseVisualStyleBackColor = true;
            this.LanguagesCheckBox.CheckStateChanged += new System.EventHandler(this.AccuracyCheckBoxes_CheckStateChanged);
            this.LanguagesCheckBox.Click += new System.EventHandler(this.AccuracyCheckBoxes_Click);
            // 
            // SubtitlesCheckBox
            // 
            this.SubtitlesCheckBox.AutoCheck = false;
            this.SubtitlesCheckBox.AutoSize = true;
            this.SubtitlesCheckBox.Checked = true;
            this.SubtitlesCheckBox.CheckState = System.Windows.Forms.CheckState.Indeterminate;
            this.SubtitlesCheckBox.Location = new System.Drawing.Point(688, 40);
            this.SubtitlesCheckBox.Name = "SubtitlesCheckBox";
            this.SubtitlesCheckBox.Size = new System.Drawing.Size(66, 17);
            this.SubtitlesCheckBox.TabIndex = 16;
            this.SubtitlesCheckBox.Text = "Subtitles";
            this.SubtitlesCheckBox.ThreeState = true;
            this.SubtitlesCheckBox.UseVisualStyleBackColor = true;
            this.SubtitlesCheckBox.CheckStateChanged += new System.EventHandler(this.AccuracyCheckBoxes_CheckStateChanged);
            this.SubtitlesCheckBox.Click += new System.EventHandler(this.AccuracyCheckBoxes_Click);
            // 
            // GameCheckBox
            // 
            this.GameCheckBox.AutoCheck = false;
            this.GameCheckBox.AutoSize = true;
            this.GameCheckBox.Checked = true;
            this.GameCheckBox.CheckState = System.Windows.Forms.CheckState.Indeterminate;
            this.GameCheckBox.Location = new System.Drawing.Point(312, 8);
            this.GameCheckBox.Name = "GameCheckBox";
            this.GameCheckBox.Size = new System.Drawing.Size(54, 17);
            this.GameCheckBox.TabIndex = 16;
            this.GameCheckBox.Text = "Game";
            this.GameCheckBox.ThreeState = true;
            this.GameCheckBox.UseVisualStyleBackColor = true;
            this.GameCheckBox.CheckStateChanged += new System.EventHandler(this.AccuracyCheckBoxes_CheckStateChanged);
            this.GameCheckBox.Click += new System.EventHandler(this.AccuracyCheckBoxes_Click);
            // 
            // MotionsCheckBox
            // 
            this.MotionsCheckBox.AutoCheck = false;
            this.MotionsCheckBox.AutoSize = true;
            this.MotionsCheckBox.Checked = true;
            this.MotionsCheckBox.CheckState = System.Windows.Forms.CheckState.Indeterminate;
            this.MotionsCheckBox.Location = new System.Drawing.Point(392, 40);
            this.MotionsCheckBox.Name = "MotionsCheckBox";
            this.MotionsCheckBox.Size = new System.Drawing.Size(63, 17);
            this.MotionsCheckBox.TabIndex = 16;
            this.MotionsCheckBox.Text = "Motions";
            this.MotionsCheckBox.ThreeState = true;
            this.MotionsCheckBox.UseVisualStyleBackColor = true;
            this.MotionsCheckBox.CheckStateChanged += new System.EventHandler(this.AccuracyCheckBoxes_CheckStateChanged);
            this.MotionsCheckBox.Click += new System.EventHandler(this.AccuracyCheckBoxes_Click);
            // 
            // NewDarkCheckBox
            // 
            this.NewDarkCheckBox.AutoCheck = false;
            this.NewDarkCheckBox.AutoSize = true;
            this.NewDarkCheckBox.Checked = true;
            this.NewDarkCheckBox.CheckState = System.Windows.Forms.CheckState.Indeterminate;
            this.NewDarkCheckBox.Location = new System.Drawing.Point(392, 8);
            this.NewDarkCheckBox.Name = "NewDarkCheckBox";
            this.NewDarkCheckBox.Size = new System.Drawing.Size(71, 17);
            this.NewDarkCheckBox.TabIndex = 16;
            this.NewDarkCheckBox.Text = "NewDark";
            this.NewDarkCheckBox.ThreeState = true;
            this.NewDarkCheckBox.UseVisualStyleBackColor = true;
            this.NewDarkCheckBox.CheckStateChanged += new System.EventHandler(this.AccuracyCheckBoxes_CheckStateChanged);
            this.NewDarkCheckBox.Click += new System.EventHandler(this.AccuracyCheckBoxes_Click);
            // 
            // CreaturesCheckBox
            // 
            this.CreaturesCheckBox.AutoCheck = false;
            this.CreaturesCheckBox.AutoSize = true;
            this.CreaturesCheckBox.Checked = true;
            this.CreaturesCheckBox.CheckState = System.Windows.Forms.CheckState.Indeterminate;
            this.CreaturesCheckBox.Location = new System.Drawing.Point(312, 40);
            this.CreaturesCheckBox.Name = "CreaturesCheckBox";
            this.CreaturesCheckBox.Size = new System.Drawing.Size(71, 17);
            this.CreaturesCheckBox.TabIndex = 16;
            this.CreaturesCheckBox.Text = "Creatures";
            this.CreaturesCheckBox.ThreeState = true;
            this.CreaturesCheckBox.UseVisualStyleBackColor = true;
            this.CreaturesCheckBox.CheckStateChanged += new System.EventHandler(this.AccuracyCheckBoxes_CheckStateChanged);
            this.CreaturesCheckBox.Click += new System.EventHandler(this.AccuracyCheckBoxes_Click);
            // 
            // NewDarkVerReqCheckBox
            // 
            this.NewDarkVerReqCheckBox.AutoCheck = false;
            this.NewDarkVerReqCheckBox.AutoSize = true;
            this.NewDarkVerReqCheckBox.Checked = true;
            this.NewDarkVerReqCheckBox.CheckState = System.Windows.Forms.CheckState.Indeterminate;
            this.NewDarkVerReqCheckBox.Location = new System.Drawing.Point(472, 8);
            this.NewDarkVerReqCheckBox.Name = "NewDarkVerReqCheckBox";
            this.NewDarkVerReqCheckBox.Size = new System.Drawing.Size(81, 17);
            this.NewDarkVerReqCheckBox.TabIndex = 16;
            this.NewDarkVerReqCheckBox.Text = "ND ver req.";
            this.NewDarkVerReqCheckBox.ThreeState = true;
            this.NewDarkVerReqCheckBox.UseVisualStyleBackColor = true;
            this.NewDarkVerReqCheckBox.CheckStateChanged += new System.EventHandler(this.AccuracyCheckBoxes_CheckStateChanged);
            this.NewDarkVerReqCheckBox.Click += new System.EventHandler(this.AccuracyCheckBoxes_Click);
            // 
            // ObjectsCheckBox
            // 
            this.ObjectsCheckBox.AutoCheck = false;
            this.ObjectsCheckBox.AutoSize = true;
            this.ObjectsCheckBox.Checked = true;
            this.ObjectsCheckBox.CheckState = System.Windows.Forms.CheckState.Indeterminate;
            this.ObjectsCheckBox.Location = new System.Drawing.Point(224, 40);
            this.ObjectsCheckBox.Name = "ObjectsCheckBox";
            this.ObjectsCheckBox.Size = new System.Drawing.Size(62, 17);
            this.ObjectsCheckBox.TabIndex = 16;
            this.ObjectsCheckBox.Text = "Objects";
            this.ObjectsCheckBox.ThreeState = true;
            this.ObjectsCheckBox.UseVisualStyleBackColor = true;
            this.ObjectsCheckBox.CheckStateChanged += new System.EventHandler(this.AccuracyCheckBoxes_CheckStateChanged);
            this.ObjectsCheckBox.Click += new System.EventHandler(this.AccuracyCheckBoxes_Click);
            // 
            // TypeCheckBox
            // 
            this.TypeCheckBox.AutoCheck = false;
            this.TypeCheckBox.AutoSize = true;
            this.TypeCheckBox.Checked = true;
            this.TypeCheckBox.CheckState = System.Windows.Forms.CheckState.Indeterminate;
            this.TypeCheckBox.Location = new System.Drawing.Point(560, 8);
            this.TypeCheckBox.Name = "TypeCheckBox";
            this.TypeCheckBox.Size = new System.Drawing.Size(50, 17);
            this.TypeCheckBox.TabIndex = 16;
            this.TypeCheckBox.Text = "Type";
            this.TypeCheckBox.ThreeState = true;
            this.TypeCheckBox.UseVisualStyleBackColor = true;
            this.TypeCheckBox.CheckStateChanged += new System.EventHandler(this.AccuracyCheckBoxes_CheckStateChanged);
            this.TypeCheckBox.Click += new System.EventHandler(this.AccuracyCheckBoxes_Click);
            // 
            // SoundsCheckBox
            // 
            this.SoundsCheckBox.AutoCheck = false;
            this.SoundsCheckBox.AutoSize = true;
            this.SoundsCheckBox.Checked = true;
            this.SoundsCheckBox.CheckState = System.Windows.Forms.CheckState.Indeterminate;
            this.SoundsCheckBox.Location = new System.Drawing.Point(152, 40);
            this.SoundsCheckBox.Name = "SoundsCheckBox";
            this.SoundsCheckBox.Size = new System.Drawing.Size(62, 17);
            this.SoundsCheckBox.TabIndex = 16;
            this.SoundsCheckBox.Text = "Sounds";
            this.SoundsCheckBox.ThreeState = true;
            this.SoundsCheckBox.UseVisualStyleBackColor = true;
            this.SoundsCheckBox.CheckStateChanged += new System.EventHandler(this.AccuracyCheckBoxes_CheckStateChanged);
            this.SoundsCheckBox.Click += new System.EventHandler(this.AccuracyCheckBoxes_Click);
            // 
            // ScriptsCheckBox
            // 
            this.ScriptsCheckBox.AutoCheck = false;
            this.ScriptsCheckBox.AutoSize = true;
            this.ScriptsCheckBox.Checked = true;
            this.ScriptsCheckBox.CheckState = System.Windows.Forms.CheckState.Indeterminate;
            this.ScriptsCheckBox.Location = new System.Drawing.Point(8, 40);
            this.ScriptsCheckBox.Name = "ScriptsCheckBox";
            this.ScriptsCheckBox.Size = new System.Drawing.Size(58, 17);
            this.ScriptsCheckBox.TabIndex = 16;
            this.ScriptsCheckBox.Text = "Scripts";
            this.ScriptsCheckBox.ThreeState = true;
            this.ScriptsCheckBox.UseVisualStyleBackColor = true;
            this.ScriptsCheckBox.CheckStateChanged += new System.EventHandler(this.AccuracyCheckBoxes_CheckStateChanged);
            this.ScriptsCheckBox.Click += new System.EventHandler(this.AccuracyCheckBoxes_Click);
            // 
            // TexturesCheckBox
            // 
            this.TexturesCheckBox.AutoCheck = false;
            this.TexturesCheckBox.AutoSize = true;
            this.TexturesCheckBox.Checked = true;
            this.TexturesCheckBox.CheckState = System.Windows.Forms.CheckState.Indeterminate;
            this.TexturesCheckBox.Location = new System.Drawing.Point(72, 40);
            this.TexturesCheckBox.Name = "TexturesCheckBox";
            this.TexturesCheckBox.Size = new System.Drawing.Size(67, 17);
            this.TexturesCheckBox.TabIndex = 16;
            this.TexturesCheckBox.Text = "Textures";
            this.TexturesCheckBox.ThreeState = true;
            this.TexturesCheckBox.UseVisualStyleBackColor = true;
            this.TexturesCheckBox.CheckStateChanged += new System.EventHandler(this.AccuracyCheckBoxes_CheckStateChanged);
            this.TexturesCheckBox.Click += new System.EventHandler(this.AccuracyCheckBoxes_Click);
            // 
            // CountButton
            // 
            this.CountButton.Enabled = false;
            this.CountButton.Location = new System.Drawing.Point(1216, 355);
            this.CountButton.Name = "CountButton";
            this.CountButton.Size = new System.Drawing.Size(123, 23);
            this.CountButton.TabIndex = 15;
            this.CountButton.Text = "Count selected items";
            this.CountButton.UseVisualStyleBackColor = true;
            this.CountButton.Click += new System.EventHandler(this.CountButton_Click);
            // 
            // FMInfoTextBox
            // 
            this.FMInfoTextBox.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.FMInfoTextBox.Location = new System.Drawing.Point(3, 440);
            this.FMInfoTextBox.Multiline = true;
            this.FMInfoTextBox.Name = "FMInfoTextBox";
            this.FMInfoTextBox.Size = new System.Drawing.Size(1542, 271);
            this.FMInfoTextBox.TabIndex = 14;
            // 
            // OpenFMFolderButton
            // 
            this.OpenFMFolderButton.Enabled = false;
            this.OpenFMFolderButton.Location = new System.Drawing.Point(1434, 355);
            this.OpenFMFolderButton.Name = "OpenFMFolderButton";
            this.OpenFMFolderButton.Size = new System.Drawing.Size(112, 23);
            this.OpenFMFolderButton.TabIndex = 13;
            this.OpenFMFolderButton.Text = "Open FM folder";
            this.OpenFMFolderButton.UseVisualStyleBackColor = true;
            this.OpenFMFolderButton.Click += new System.EventHandler(this.OpenFMFolderButton_Click);
            // 
            // OpenYamlButton
            // 
            this.OpenYamlButton.Enabled = false;
            this.OpenYamlButton.Location = new System.Drawing.Point(1359, 355);
            this.OpenYamlButton.Name = "OpenYamlButton";
            this.OpenYamlButton.Size = new System.Drawing.Size(75, 23);
            this.OpenYamlButton.TabIndex = 13;
            this.OpenYamlButton.Text = "Open YAML";
            this.OpenYamlButton.UseVisualStyleBackColor = true;
            this.OpenYamlButton.Click += new System.EventHandler(this.OpenYamlButton_Click);
            // 
            // PopulateFMInfoListButton
            // 
            this.PopulateFMInfoListButton.Location = new System.Drawing.Point(2, 355);
            this.PopulateFMInfoListButton.Name = "PopulateFMInfoListButton";
            this.PopulateFMInfoListButton.Size = new System.Drawing.Size(128, 23);
            this.PopulateFMInfoListButton.TabIndex = 12;
            this.PopulateFMInfoListButton.Text = "Populate FM info list";
            this.PopulateFMInfoListButton.UseVisualStyleBackColor = true;
            this.PopulateFMInfoListButton.Click += new System.EventHandler(this.PopulateFMInfoListButton_Click);
            // 
            // FMInfoFilesListView
            // 
            this.FMInfoFilesListView.AllowColumnReorder = true;
            this.FMInfoFilesListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.TitleColumnHeader,
            this.ArchiveNameColumnHeader,
            this.AuthorColumnHeader,
            this.VersionColumnHeader,
            this.LanguagesColumnHeader,
            this.GameColumnHeader,
            this.NewDarkColumnHeader,
            this.NewDarkVersionRequiredColumnHeader,
            this.ReleaseDateColumnHeader,
            this.LastUpdatedColumnHeader,
            this.TypeColumnHeader,
            this.HasCustomScriptsColumnHeader,
            this.HasCustomTexturesColumnHeader,
            this.HasCustomSoundsColumnHeader,
            this.HasCustomObjectsColumnHeader,
            this.HasCustomCreaturesColumnHeader,
            this.HasCustomMotionsColumnHeader,
            this.HasAutomapColumnHeader,
            this.HasMapColumnHeader,
            this.HasMoviesColumnHeader,
            this.HasCustomSubtitlesColumnHeader,
            this.SizeColumnHeader});
            this.FMInfoFilesListView.Dock = System.Windows.Forms.DockStyle.Top;
            this.FMInfoFilesListView.FullRowSelect = true;
            this.FMInfoFilesListView.HideSelection = false;
            this.FMInfoFilesListView.Location = new System.Drawing.Point(3, 3);
            this.FMInfoFilesListView.Name = "FMInfoFilesListView";
            this.FMInfoFilesListView.Size = new System.Drawing.Size(1542, 352);
            this.FMInfoFilesListView.TabIndex = 11;
            this.FMInfoFilesListView.UseCompatibleStateImageBehavior = false;
            this.FMInfoFilesListView.View = System.Windows.Forms.View.Details;
            this.FMInfoFilesListView.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.FMInfoFilesListView_ColumnClick);
            this.FMInfoFilesListView.SelectedIndexChanged += new System.EventHandler(this.FMInfoFilesListView_SelectedIndexChanged);
            this.FMInfoFilesListView.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.FMInfoFilesListView_KeyPress);
            // 
            // TitleColumnHeader
            // 
            this.TitleColumnHeader.Text = "Title";
            this.TitleColumnHeader.Width = 269;
            // 
            // ArchiveNameColumnHeader
            // 
            this.ArchiveNameColumnHeader.Text = "Archive Name";
            this.ArchiveNameColumnHeader.Width = 255;
            // 
            // AuthorColumnHeader
            // 
            this.AuthorColumnHeader.DisplayIndex = 3;
            this.AuthorColumnHeader.Text = "Author";
            this.AuthorColumnHeader.Width = 144;
            // 
            // VersionColumnHeader
            // 
            this.VersionColumnHeader.DisplayIndex = 4;
            this.VersionColumnHeader.Text = "Version";
            // 
            // LanguagesColumnHeader
            // 
            this.LanguagesColumnHeader.DisplayIndex = 5;
            this.LanguagesColumnHeader.Text = "Languages";
            this.LanguagesColumnHeader.Width = 179;
            // 
            // GameColumnHeader
            // 
            this.GameColumnHeader.DisplayIndex = 6;
            this.GameColumnHeader.Text = "Game";
            this.GameColumnHeader.Width = 85;
            // 
            // NewDarkColumnHeader
            // 
            this.NewDarkColumnHeader.DisplayIndex = 7;
            this.NewDarkColumnHeader.Text = "NewDark";
            // 
            // NewDarkVersionRequiredColumnHeader
            // 
            this.NewDarkVersionRequiredColumnHeader.DisplayIndex = 8;
            this.NewDarkVersionRequiredColumnHeader.Text = "NewDark Version Required";
            this.NewDarkVersionRequiredColumnHeader.Width = 90;
            // 
            // ReleaseDateColumnHeader
            // 
            this.ReleaseDateColumnHeader.DisplayIndex = 9;
            this.ReleaseDateColumnHeader.Text = "Released";
            this.ReleaseDateColumnHeader.Width = 87;
            // 
            // LastUpdatedColumnHeader
            // 
            this.LastUpdatedColumnHeader.DisplayIndex = 10;
            this.LastUpdatedColumnHeader.Text = "Last Updated";
            this.LastUpdatedColumnHeader.Width = 103;
            // 
            // TypeColumnHeader
            // 
            this.TypeColumnHeader.DisplayIndex = 11;
            this.TypeColumnHeader.Text = "Type";
            this.TypeColumnHeader.Width = 70;
            // 
            // HasCustomScriptsColumnHeader
            // 
            this.HasCustomScriptsColumnHeader.DisplayIndex = 12;
            this.HasCustomScriptsColumnHeader.Text = "Scripts";
            // 
            // HasCustomTexturesColumnHeader
            // 
            this.HasCustomTexturesColumnHeader.DisplayIndex = 13;
            this.HasCustomTexturesColumnHeader.Text = "Textures";
            // 
            // HasCustomSoundsColumnHeader
            // 
            this.HasCustomSoundsColumnHeader.DisplayIndex = 14;
            this.HasCustomSoundsColumnHeader.Text = "Sounds";
            // 
            // HasCustomObjectsColumnHeader
            // 
            this.HasCustomObjectsColumnHeader.DisplayIndex = 15;
            this.HasCustomObjectsColumnHeader.Text = "Objects";
            // 
            // HasCustomCreaturesColumnHeader
            // 
            this.HasCustomCreaturesColumnHeader.DisplayIndex = 16;
            this.HasCustomCreaturesColumnHeader.Text = "Creatures";
            // 
            // HasCustomMotionsColumnHeader
            // 
            this.HasCustomMotionsColumnHeader.DisplayIndex = 17;
            this.HasCustomMotionsColumnHeader.Text = "Motions";
            // 
            // HasAutomapColumnHeader
            // 
            this.HasAutomapColumnHeader.DisplayIndex = 18;
            this.HasAutomapColumnHeader.Text = "Automap";
            // 
            // HasMapColumnHeader
            // 
            this.HasMapColumnHeader.DisplayIndex = 19;
            this.HasMapColumnHeader.Text = "Map";
            // 
            // HasMoviesColumnHeader
            // 
            this.HasMoviesColumnHeader.DisplayIndex = 20;
            this.HasMoviesColumnHeader.Text = "Movies";
            // 
            // HasCustomSubtitlesColumnHeader
            // 
            this.HasCustomSubtitlesColumnHeader.DisplayIndex = 21;
            this.HasCustomSubtitlesColumnHeader.Text = "Subtitles";
            // 
            // SizeColumnHeader
            // 
            this.SizeColumnHeader.DisplayIndex = 2;
            this.SizeColumnHeader.Text = "Size";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1556, 740);
            this.Controls.Add(this.MainTabControl);
            this.Name = "MainForm";
            this.Text = "FMInfoGen";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.MainTab_ListsPanel.ResumeLayout(false);
            this.MainTab_ListsPanel.PerformLayout();
            this.MainTabControl.ResumeLayout(false);
            this.MainTabPage.ResumeLayout(false);
            this.MainTabPage.PerformLayout();
            this.ScanOptionsGroupBox.ResumeLayout(false);
            this.ScanOptionsGroupBox.PerformLayout();
            this.ScanControlsGroupBox.ResumeLayout(false);
            this.AccuracyTabPage.ResumeLayout(false);
            this.AccuracyTabPage.PerformLayout();
            this.AccuracyCheckPanel.ResumeLayout(false);
            this.AccuracyCheckPanel.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListBox FMsListBox;
        private System.Windows.Forms.Button SetFMsFolderButton;
        private System.Windows.Forms.Button ExtractFMArchiveButton;
        private System.Windows.Forms.TextBox TempPathTextBox;
        private System.Windows.Forms.Button SetTempPathButton;
        private System.Windows.Forms.ListBox ExtractedPathListBox;
        private System.Windows.Forms.Button ExtractAllFMArchivesButton;
        private System.Windows.Forms.ProgressBar ExtractProgressBar;
        private System.Windows.Forms.Button CancelExtractAllButton;
        private System.Windows.Forms.Panel MainTab_ListsPanel;
        private System.Windows.Forms.CheckBox OverwriteFoldersCheckBox;
        private System.Windows.Forms.TabControl MainTabControl;
        private System.Windows.Forms.TabPage MainTabPage;
        private System.Windows.Forms.TabPage AccuracyTabPage;
        private System.Windows.Forms.ListView FMInfoFilesListView;
        private System.Windows.Forms.ColumnHeader ArchiveNameColumnHeader;
        private System.Windows.Forms.ColumnHeader TitleColumnHeader;
        private System.Windows.Forms.ColumnHeader AuthorColumnHeader;
        private System.Windows.Forms.ColumnHeader VersionColumnHeader;
        private System.Windows.Forms.ColumnHeader GameColumnHeader;
        private System.Windows.Forms.ColumnHeader LanguagesColumnHeader;
        private System.Windows.Forms.ColumnHeader NewDarkColumnHeader;
        private System.Windows.Forms.ColumnHeader NewDarkVersionRequiredColumnHeader;
        private System.Windows.Forms.ColumnHeader ReleaseDateColumnHeader;
        private System.Windows.Forms.ColumnHeader LastUpdatedColumnHeader;
        private System.Windows.Forms.Button PopulateFMInfoListButton;
        private System.Windows.Forms.Button OpenFMFolderButton;
        private System.Windows.Forms.Button OpenYamlButton;
        private System.Windows.Forms.Button GetOneFromZipButton;
        private System.Windows.Forms.ColumnHeader TypeColumnHeader;
        private System.Windows.Forms.TextBox FMInfoTextBox;
        private System.Windows.Forms.ColumnHeader HasCustomScriptsColumnHeader;
        private System.Windows.Forms.ColumnHeader HasCustomTexturesColumnHeader;
        private System.Windows.Forms.ColumnHeader HasCustomSoundsColumnHeader;
        private System.Windows.Forms.ColumnHeader HasCustomObjectsColumnHeader;
        private System.Windows.Forms.ColumnHeader HasCustomCreaturesColumnHeader;
        private System.Windows.Forms.ColumnHeader HasCustomMotionsColumnHeader;
        private System.Windows.Forms.ColumnHeader HasAutomapColumnHeader;
        private System.Windows.Forms.ColumnHeader HasMapColumnHeader;
        private System.Windows.Forms.ColumnHeader HasMoviesColumnHeader;
        private System.Windows.Forms.ColumnHeader HasCustomSubtitlesColumnHeader;
        private System.Windows.Forms.Button CountButton;
        private System.Windows.Forms.Label DebugLabel;
        private System.Windows.Forms.GroupBox ScanControlsGroupBox;
        private System.Windows.Forms.Button GetFromZipsButton;
        private System.Windows.Forms.Button GetFromFoldersButton;
        private System.Windows.Forms.Button GetOneFromFolderButton;
        private System.Windows.Forms.TextBox DebugLogTextBox;
        private System.Windows.Forms.Button TestButton;
        private System.Windows.Forms.CheckBox MoviesCheckBox;
        private System.Windows.Forms.CheckBox MapCheckBox;
        private System.Windows.Forms.CheckBox AutomapCheckBox;
        private System.Windows.Forms.CheckBox SubtitlesCheckBox;
        private System.Windows.Forms.CheckBox MotionsCheckBox;
        private System.Windows.Forms.CheckBox CreaturesCheckBox;
        private System.Windows.Forms.CheckBox ObjectsCheckBox;
        private System.Windows.Forms.CheckBox SoundsCheckBox;
        private System.Windows.Forms.CheckBox TexturesCheckBox;
        private System.Windows.Forms.CheckBox ScriptsCheckBox;
        private System.Windows.Forms.CheckBox TypeCheckBox;
        private System.Windows.Forms.CheckBox NewDarkVerReqCheckBox;
        private System.Windows.Forms.CheckBox NewDarkCheckBox;
        private System.Windows.Forms.CheckBox GameCheckBox;
        private System.Windows.Forms.CheckBox LanguagesCheckBox;
        private System.Windows.Forms.CheckBox VersionCheckBox;
        private System.Windows.Forms.CheckBox AuthorCheckBox;
        private System.Windows.Forms.CheckBox TitleCheckBox;
        private System.Windows.Forms.Panel AccuracyCheckPanel;
        private System.Windows.Forms.CheckBox ScanTitleCheckBox;
        private System.Windows.Forms.CheckBox ScanCustomResourcesCheckBox;
        private System.Windows.Forms.CheckBox ScanNDMinVerCheckBox;
        private System.Windows.Forms.CheckBox ScanGameTypeCheckBox;
        private System.Windows.Forms.CheckBox ScanLanguagesCheckBox;
        private System.Windows.Forms.CheckBox ScanVersionCheckBox;
        private System.Windows.Forms.CheckBox ScanIncludedMissionsCheckBox;
        private System.Windows.Forms.CheckBox ScanAuthorCheckBox;
        private System.Windows.Forms.GroupBox ScanOptionsGroupBox;
        private System.Windows.Forms.CheckBox ScanNewDarkRequiredCheckBox;
        private System.Windows.Forms.Button CancelFMScanButton;
        private System.Windows.Forms.ProgressBar FMScanProgressBar;
        private System.Windows.Forms.Button Test2Button;
        private System.Windows.Forms.CheckBox ScanSizeCheckBox;
        private System.Windows.Forms.ColumnHeader SizeColumnHeader;
        private System.Windows.Forms.Label QuickSetLabel;
        private System.Windows.Forms.Button SetFMsFolderToT3Button;
        private System.Windows.Forms.Button SetFMsFolderToSS2Button;
        private System.Windows.Forms.Button SetFMsFolderTo1098Button;
        private System.Windows.Forms.CheckBox ScanReleaseDateCheckBox;
        private System.Windows.Forms.CheckBox ScanTagsCheckBox;
        private System.Windows.Forms.CheckBox ScanDescriptionCheckBox;
        private System.Windows.Forms.Button SetFMsFolderTo7zTestButton;
        private System.Windows.Forms.Button FastManualDiffButton;
    }
}

