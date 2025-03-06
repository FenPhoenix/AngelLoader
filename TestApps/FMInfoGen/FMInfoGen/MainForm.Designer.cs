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
            this.FMScanProgressBar = new System.Windows.Forms.ProgressBar();
            this.CancelFMScanButton = new System.Windows.Forms.Button();
            this.ScanOptionsGroupBox = new System.Windows.Forms.GroupBox();
            this.ScanReleaseDateCheckBox = new System.Windows.Forms.CheckBox();
            this.ScanMissionCountCheckBox = new System.Windows.Forms.CheckBox();
            this.ScanTagsCheckBox = new System.Windows.Forms.CheckBox();
            this.ScanSizeCheckBox = new System.Windows.Forms.CheckBox();
            this.ScanTitleCheckBox = new System.Windows.Forms.CheckBox();
            this.ScanCustomResourcesCheckBox = new System.Windows.Forms.CheckBox();
            this.ScanAuthorCheckBox = new System.Windows.Forms.CheckBox();
            this.ScanGameTypeCheckBox = new System.Windows.Forms.CheckBox();
            this.Test2Button = new System.Windows.Forms.Button();
            this.TestButton = new System.Windows.Forms.Button();
            this.ScanControlsGroupBox = new System.Windows.Forms.GroupBox();
            this.GetOneFromFolderButton = new System.Windows.Forms.Button();
            this.GetFromZipsButton = new System.Windows.Forms.Button();
            this.GetFromFoldersButton = new System.Windows.Forms.Button();
            this.DebugLabel = new System.Windows.Forms.Label();
            this.MainTab_ListsPanel.SuspendLayout();
            this.MainTabControl.SuspendLayout();
            this.MainTabPage.SuspendLayout();
            this.ScanOptionsGroupBox.SuspendLayout();
            this.ScanControlsGroupBox.SuspendLayout();
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
            this.ExtractProgressBar.Location = new System.Drawing.Point(8, 584);
            this.ExtractProgressBar.Name = "ExtractProgressBar";
            this.ExtractProgressBar.Size = new System.Drawing.Size(744, 23);
            this.ExtractProgressBar.TabIndex = 7;
            this.ExtractProgressBar.Visible = false;
            // 
            // CancelExtractAllButton
            // 
            this.CancelExtractAllButton.Location = new System.Drawing.Point(633, 608);
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
            this.MainTab_ListsPanel.Size = new System.Drawing.Size(1220, 334);
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
            this.MainTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MainTabControl.Location = new System.Drawing.Point(0, 0);
            this.MainTabControl.Name = "MainTabControl";
            this.MainTabControl.SelectedIndex = 0;
            this.MainTabControl.Size = new System.Drawing.Size(1245, 670);
            this.MainTabControl.TabIndex = 10;
            // 
            // MainTabPage
            // 
            this.MainTabPage.BackColor = System.Drawing.SystemColors.Control;
            this.MainTabPage.Controls.Add(this.FMScanProgressBar);
            this.MainTabPage.Controls.Add(this.CancelFMScanButton);
            this.MainTabPage.Controls.Add(this.ScanOptionsGroupBox);
            this.MainTabPage.Controls.Add(this.Test2Button);
            this.MainTabPage.Controls.Add(this.TestButton);
            this.MainTabPage.Controls.Add(this.ScanControlsGroupBox);
            this.MainTabPage.Controls.Add(this.DebugLabel);
            this.MainTabPage.Controls.Add(this.MainTab_ListsPanel);
            this.MainTabPage.Controls.Add(this.ExtractProgressBar);
            this.MainTabPage.Controls.Add(this.CancelExtractAllButton);
            this.MainTabPage.Location = new System.Drawing.Point(4, 22);
            this.MainTabPage.Name = "MainTabPage";
            this.MainTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.MainTabPage.Size = new System.Drawing.Size(1237, 644);
            this.MainTabPage.TabIndex = 0;
            this.MainTabPage.Text = "Main";
            // 
            // FMScanProgressBar
            // 
            this.FMScanProgressBar.Location = new System.Drawing.Point(8, 528);
            this.FMScanProgressBar.Name = "FMScanProgressBar";
            this.FMScanProgressBar.Size = new System.Drawing.Size(744, 23);
            this.FMScanProgressBar.TabIndex = 18;
            // 
            // CancelFMScanButton
            // 
            this.CancelFMScanButton.Location = new System.Drawing.Point(658, 552);
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
            this.ScanOptionsGroupBox.Controls.Add(this.ScanMissionCountCheckBox);
            this.ScanOptionsGroupBox.Controls.Add(this.ScanTagsCheckBox);
            this.ScanOptionsGroupBox.Controls.Add(this.ScanSizeCheckBox);
            this.ScanOptionsGroupBox.Controls.Add(this.ScanTitleCheckBox);
            this.ScanOptionsGroupBox.Controls.Add(this.ScanCustomResourcesCheckBox);
            this.ScanOptionsGroupBox.Controls.Add(this.ScanAuthorCheckBox);
            this.ScanOptionsGroupBox.Controls.Add(this.ScanGameTypeCheckBox);
            this.ScanOptionsGroupBox.Location = new System.Drawing.Point(248, 344);
            this.ScanOptionsGroupBox.Name = "ScanOptionsGroupBox";
            this.ScanOptionsGroupBox.Size = new System.Drawing.Size(504, 168);
            this.ScanOptionsGroupBox.TabIndex = 16;
            this.ScanOptionsGroupBox.TabStop = false;
            this.ScanOptionsGroupBox.Text = "Scan options";
            // 
            // ScanReleaseDateCheckBox
            // 
            this.ScanReleaseDateCheckBox.AutoSize = true;
            this.ScanReleaseDateCheckBox.Checked = true;
            this.ScanReleaseDateCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ScanReleaseDateCheckBox.Location = new System.Drawing.Point(136, 48);
            this.ScanReleaseDateCheckBox.Name = "ScanReleaseDateCheckBox";
            this.ScanReleaseDateCheckBox.Size = new System.Drawing.Size(69, 17);
            this.ScanReleaseDateCheckBox.TabIndex = 18;
            this.ScanReleaseDateCheckBox.Text = "Rel. date";
            this.ScanReleaseDateCheckBox.UseVisualStyleBackColor = true;
            // 
            // ScanMissionCountCheckBox
            // 
            this.ScanMissionCountCheckBox.AutoSize = true;
            this.ScanMissionCountCheckBox.Checked = true;
            this.ScanMissionCountCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ScanMissionCountCheckBox.Location = new System.Drawing.Point(136, 96);
            this.ScanMissionCountCheckBox.Name = "ScanMissionCountCheckBox";
            this.ScanMissionCountCheckBox.Size = new System.Drawing.Size(91, 17);
            this.ScanMissionCountCheckBox.TabIndex = 18;
            this.ScanMissionCountCheckBox.Text = "Mission count";
            this.ScanMissionCountCheckBox.UseVisualStyleBackColor = true;
            // 
            // ScanTagsCheckBox
            // 
            this.ScanTagsCheckBox.AutoSize = true;
            this.ScanTagsCheckBox.Checked = true;
            this.ScanTagsCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ScanTagsCheckBox.Location = new System.Drawing.Point(136, 72);
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
            this.ScanSizeCheckBox.Location = new System.Drawing.Point(136, 24);
            this.ScanSizeCheckBox.Name = "ScanSizeCheckBox";
            this.ScanSizeCheckBox.Size = new System.Drawing.Size(46, 17);
            this.ScanSizeCheckBox.TabIndex = 17;
            this.ScanSizeCheckBox.Text = "Size";
            this.ScanSizeCheckBox.UseVisualStyleBackColor = true;
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
            this.ScanCustomResourcesCheckBox.Location = new System.Drawing.Point(16, 96);
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
            this.ScanAuthorCheckBox.Location = new System.Drawing.Point(16, 48);
            this.ScanAuthorCheckBox.Name = "ScanAuthorCheckBox";
            this.ScanAuthorCheckBox.Size = new System.Drawing.Size(57, 17);
            this.ScanAuthorCheckBox.TabIndex = 15;
            this.ScanAuthorCheckBox.Text = "Author";
            this.ScanAuthorCheckBox.UseVisualStyleBackColor = true;
            // 
            // ScanGameTypeCheckBox
            // 
            this.ScanGameTypeCheckBox.AutoSize = true;
            this.ScanGameTypeCheckBox.Checked = true;
            this.ScanGameTypeCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ScanGameTypeCheckBox.Location = new System.Drawing.Point(16, 72);
            this.ScanGameTypeCheckBox.Name = "ScanGameTypeCheckBox";
            this.ScanGameTypeCheckBox.Size = new System.Drawing.Size(77, 17);
            this.ScanGameTypeCheckBox.TabIndex = 15;
            this.ScanGameTypeCheckBox.Text = "Game type";
            this.ScanGameTypeCheckBox.UseVisualStyleBackColor = true;
            // 
            // Test2Button
            // 
            this.Test2Button.Location = new System.Drawing.Point(24, 456);
            this.Test2Button.Name = "Test2Button";
            this.Test2Button.Size = new System.Drawing.Size(97, 23);
            this.Test2Button.TabIndex = 14;
            this.Test2Button.Text = "Test 2";
            this.Test2Button.UseVisualStyleBackColor = true;
            this.Test2Button.Click += new System.EventHandler(this.Test2Button_Click);
            // 
            // TestButton
            // 
            this.TestButton.Location = new System.Drawing.Point(24, 432);
            this.TestButton.Name = "TestButton";
            this.TestButton.Size = new System.Drawing.Size(97, 23);
            this.TestButton.TabIndex = 14;
            this.TestButton.Text = "Test";
            this.TestButton.UseVisualStyleBackColor = true;
            this.TestButton.Click += new System.EventHandler(this.TestButton_Click);
            // 
            // ScanControlsGroupBox
            // 
            this.ScanControlsGroupBox.Controls.Add(this.GetOneFromZipButton);
            this.ScanControlsGroupBox.Controls.Add(this.GetOneFromFolderButton);
            this.ScanControlsGroupBox.Controls.Add(this.GetFromZipsButton);
            this.ScanControlsGroupBox.Controls.Add(this.GetFromFoldersButton);
            this.ScanControlsGroupBox.Location = new System.Drawing.Point(8, 344);
            this.ScanControlsGroupBox.Name = "ScanControlsGroupBox";
            this.ScanControlsGroupBox.Size = new System.Drawing.Size(232, 80);
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
            this.DebugLabel.Location = new System.Drawing.Point(9, 560);
            this.DebugLabel.Name = "DebugLabel";
            this.DebugLabel.Size = new System.Drawing.Size(71, 13);
            this.DebugLabel.TabIndex = 10;
            this.DebugLabel.Text = "[DebugLabel]";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1245, 670);
            this.Controls.Add(this.MainTabControl);
            this.Name = "MainForm";
            this.Text = "FMInfoGen";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MainForm_FormClosed);
            this.MainTab_ListsPanel.ResumeLayout(false);
            this.MainTab_ListsPanel.PerformLayout();
            this.MainTabControl.ResumeLayout(false);
            this.MainTabPage.ResumeLayout(false);
            this.MainTabPage.PerformLayout();
            this.ScanOptionsGroupBox.ResumeLayout(false);
            this.ScanOptionsGroupBox.PerformLayout();
            this.ScanControlsGroupBox.ResumeLayout(false);
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
        private System.Windows.Forms.Button GetOneFromZipButton;
        private System.Windows.Forms.Label DebugLabel;
        private System.Windows.Forms.GroupBox ScanControlsGroupBox;
        private System.Windows.Forms.Button GetFromZipsButton;
        private System.Windows.Forms.Button GetFromFoldersButton;
        private System.Windows.Forms.Button GetOneFromFolderButton;
        private System.Windows.Forms.Button TestButton;
        private System.Windows.Forms.CheckBox ScanTitleCheckBox;
        private System.Windows.Forms.CheckBox ScanCustomResourcesCheckBox;
        private System.Windows.Forms.CheckBox ScanGameTypeCheckBox;
        private System.Windows.Forms.CheckBox ScanAuthorCheckBox;
        private System.Windows.Forms.GroupBox ScanOptionsGroupBox;
        private System.Windows.Forms.Button CancelFMScanButton;
        private System.Windows.Forms.ProgressBar FMScanProgressBar;
        private System.Windows.Forms.Button Test2Button;
        private System.Windows.Forms.CheckBox ScanSizeCheckBox;
        private System.Windows.Forms.Label QuickSetLabel;
        private System.Windows.Forms.Button SetFMsFolderToT3Button;
        private System.Windows.Forms.Button SetFMsFolderToSS2Button;
        private System.Windows.Forms.Button SetFMsFolderTo1098Button;
        private System.Windows.Forms.CheckBox ScanReleaseDateCheckBox;
        private System.Windows.Forms.CheckBox ScanTagsCheckBox;
        private System.Windows.Forms.Button SetFMsFolderTo7zTestButton;
        private System.Windows.Forms.CheckBox ScanMissionCountCheckBox;
    }
}

