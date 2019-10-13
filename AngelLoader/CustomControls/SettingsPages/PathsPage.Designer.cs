namespace AngelLoader.CustomControls.SettingsPages
{
    partial class PathsPage
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.PagePanel = new System.Windows.Forms.Panel();
            this.ActualPathsPanel = new System.Windows.Forms.Panel();
            this.SteamGroupBox = new System.Windows.Forms.GroupBox();
            this.LaunchTheseGamesThroughSteamLabel = new System.Windows.Forms.Label();
            this.T3UseSteamCheckBox = new System.Windows.Forms.CheckBox();
            this.SteamExeLabel = new System.Windows.Forms.Label();
            this.T2UseSteamCheckBox = new System.Windows.Forms.CheckBox();
            this.SteamExeTextBox = new System.Windows.Forms.TextBox();
            this.T1UseSteamCheckBox = new System.Windows.Forms.CheckBox();
            this.SteamExeBrowseButton = new System.Windows.Forms.Button();
            this.PathsToGameExesGroupBox = new System.Windows.Forms.GroupBox();
            this.GameRequirementsPanel = new System.Windows.Forms.Panel();
            this.GameRequirementsLabel = new System.Windows.Forms.Label();
            this.Thief3ExePathLabel = new System.Windows.Forms.Label();
            this.Thief2ExePathLabel = new System.Windows.Forms.Label();
            this.Thief1ExePathLabel = new System.Windows.Forms.Label();
            this.Thief3ExePathBrowseButton = new System.Windows.Forms.Button();
            this.Thief2ExePathBrowseButton = new System.Windows.Forms.Button();
            this.Thief1ExePathBrowseButton = new System.Windows.Forms.Button();
            this.Thief3ExePathTextBox = new System.Windows.Forms.TextBox();
            this.Thief2ExePathTextBox = new System.Windows.Forms.TextBox();
            this.Thief1ExePathTextBox = new System.Windows.Forms.TextBox();
            this.FMArchivePathsGroupBox = new System.Windows.Forms.GroupBox();
            this.IncludeSubfoldersCheckBox = new System.Windows.Forms.CheckBox();
            this.AddFMArchivePathButton = new System.Windows.Forms.Button();
            this.RemoveFMArchivePathButton = new System.Windows.Forms.Button();
            this.FMArchivePathsListBox = new System.Windows.Forms.ListBox();
            this.OtherGroupBox = new System.Windows.Forms.GroupBox();
            this.BackupPathLabel = new System.Windows.Forms.Label();
            this.BackupPathBrowseButton = new System.Windows.Forms.Button();
            this.BackupPathTextBox = new System.Windows.Forms.TextBox();
            this.DummyAutoScrollPanel = new System.Windows.Forms.Panel();
            this.PagePanel.SuspendLayout();
            this.ActualPathsPanel.SuspendLayout();
            this.SteamGroupBox.SuspendLayout();
            this.PathsToGameExesGroupBox.SuspendLayout();
            this.GameRequirementsPanel.SuspendLayout();
            this.FMArchivePathsGroupBox.SuspendLayout();
            this.OtherGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // PagePanel
            // 
            this.PagePanel.AutoScroll = true;
            this.PagePanel.Controls.Add(this.ActualPathsPanel);
            this.PagePanel.Controls.Add(this.DummyAutoScrollPanel);
            this.PagePanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PagePanel.Location = new System.Drawing.Point(0, 0);
            this.PagePanel.Name = "PagePanel";
            this.PagePanel.Size = new System.Drawing.Size(440, 756);
            this.PagePanel.TabIndex = 3;
            // 
            // ActualPathsPanel
            // 
            this.ActualPathsPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ActualPathsPanel.Controls.Add(this.SteamGroupBox);
            this.ActualPathsPanel.Controls.Add(this.PathsToGameExesGroupBox);
            this.ActualPathsPanel.Controls.Add(this.FMArchivePathsGroupBox);
            this.ActualPathsPanel.Controls.Add(this.OtherGroupBox);
            this.ActualPathsPanel.Location = new System.Drawing.Point(0, 0);
            this.ActualPathsPanel.MinimumSize = new System.Drawing.Size(440, 0);
            this.ActualPathsPanel.Name = "ActualPathsPanel";
            this.ActualPathsPanel.Size = new System.Drawing.Size(440, 736);
            this.ActualPathsPanel.TabIndex = 4;
            // 
            // SteamGroupBox
            // 
            this.SteamGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.SteamGroupBox.Controls.Add(this.LaunchTheseGamesThroughSteamLabel);
            this.SteamGroupBox.Controls.Add(this.T3UseSteamCheckBox);
            this.SteamGroupBox.Controls.Add(this.SteamExeLabel);
            this.SteamGroupBox.Controls.Add(this.T2UseSteamCheckBox);
            this.SteamGroupBox.Controls.Add(this.SteamExeTextBox);
            this.SteamGroupBox.Controls.Add(this.T1UseSteamCheckBox);
            this.SteamGroupBox.Controls.Add(this.SteamExeBrowseButton);
            this.SteamGroupBox.Location = new System.Drawing.Point(8, 216);
            this.SteamGroupBox.Name = "SteamGroupBox";
            this.SteamGroupBox.Size = new System.Drawing.Size(424, 160);
            this.SteamGroupBox.TabIndex = 4;
            this.SteamGroupBox.TabStop = false;
            this.SteamGroupBox.Text = "Steam options";
            // 
            // LaunchTheseGamesThroughSteamLabel
            // 
            this.LaunchTheseGamesThroughSteamLabel.AutoSize = true;
            this.LaunchTheseGamesThroughSteamLabel.Location = new System.Drawing.Point(16, 72);
            this.LaunchTheseGamesThroughSteamLabel.Name = "LaunchTheseGamesThroughSteamLabel";
            this.LaunchTheseGamesThroughSteamLabel.Size = new System.Drawing.Size(181, 13);
            this.LaunchTheseGamesThroughSteamLabel.TabIndex = 11;
            this.LaunchTheseGamesThroughSteamLabel.Text = "Launch these games through Steam:";
            // 
            // T3UseSteamCheckBox
            // 
            this.T3UseSteamCheckBox.AutoSize = true;
            this.T3UseSteamCheckBox.Location = new System.Drawing.Point(24, 128);
            this.T3UseSteamCheckBox.Name = "T3UseSteamCheckBox";
            this.T3UseSteamCheckBox.Size = new System.Drawing.Size(59, 17);
            this.T3UseSteamCheckBox.TabIndex = 10;
            this.T3UseSteamCheckBox.Text = "Thief 3";
            this.T3UseSteamCheckBox.UseVisualStyleBackColor = true;
            // 
            // SteamExeLabel
            // 
            this.SteamExeLabel.AutoSize = true;
            this.SteamExeLabel.Location = new System.Drawing.Point(16, 24);
            this.SteamExeLabel.Name = "SteamExeLabel";
            this.SteamExeLabel.Size = new System.Drawing.Size(178, 13);
            this.SteamExeLabel.TabIndex = 8;
            this.SteamExeLabel.Text = "Path to Steam executable (optional):";
            // 
            // T2UseSteamCheckBox
            // 
            this.T2UseSteamCheckBox.AutoSize = true;
            this.T2UseSteamCheckBox.Location = new System.Drawing.Point(24, 112);
            this.T2UseSteamCheckBox.Name = "T2UseSteamCheckBox";
            this.T2UseSteamCheckBox.Size = new System.Drawing.Size(59, 17);
            this.T2UseSteamCheckBox.TabIndex = 10;
            this.T2UseSteamCheckBox.Text = "Thief 2";
            this.T2UseSteamCheckBox.UseVisualStyleBackColor = true;
            // 
            // SteamExeTextBox
            // 
            this.SteamExeTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.SteamExeTextBox.Location = new System.Drawing.Point(16, 40);
            this.SteamExeTextBox.Name = "SteamExeTextBox";
            this.SteamExeTextBox.Size = new System.Drawing.Size(320, 20);
            this.SteamExeTextBox.TabIndex = 5;
            // 
            // T1UseSteamCheckBox
            // 
            this.T1UseSteamCheckBox.AutoSize = true;
            this.T1UseSteamCheckBox.Location = new System.Drawing.Point(24, 96);
            this.T1UseSteamCheckBox.Name = "T1UseSteamCheckBox";
            this.T1UseSteamCheckBox.Size = new System.Drawing.Size(59, 17);
            this.T1UseSteamCheckBox.TabIndex = 10;
            this.T1UseSteamCheckBox.Text = "Thief 1";
            this.T1UseSteamCheckBox.UseVisualStyleBackColor = true;
            // 
            // SteamExeBrowseButton
            // 
            this.SteamExeBrowseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.SteamExeBrowseButton.AutoSize = true;
            this.SteamExeBrowseButton.Location = new System.Drawing.Point(336, 39);
            this.SteamExeBrowseButton.Name = "SteamExeBrowseButton";
            this.SteamExeBrowseButton.Padding = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.SteamExeBrowseButton.Size = new System.Drawing.Size(75, 23);
            this.SteamExeBrowseButton.TabIndex = 6;
            this.SteamExeBrowseButton.Text = "Browse...";
            this.SteamExeBrowseButton.UseVisualStyleBackColor = true;
            // 
            // PathsToGameExesGroupBox
            // 
            this.PathsToGameExesGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.PathsToGameExesGroupBox.Controls.Add(this.GameRequirementsPanel);
            this.PathsToGameExesGroupBox.Controls.Add(this.Thief3ExePathLabel);
            this.PathsToGameExesGroupBox.Controls.Add(this.Thief2ExePathLabel);
            this.PathsToGameExesGroupBox.Controls.Add(this.Thief1ExePathLabel);
            this.PathsToGameExesGroupBox.Controls.Add(this.Thief3ExePathBrowseButton);
            this.PathsToGameExesGroupBox.Controls.Add(this.Thief2ExePathBrowseButton);
            this.PathsToGameExesGroupBox.Controls.Add(this.Thief1ExePathBrowseButton);
            this.PathsToGameExesGroupBox.Controls.Add(this.Thief3ExePathTextBox);
            this.PathsToGameExesGroupBox.Controls.Add(this.Thief2ExePathTextBox);
            this.PathsToGameExesGroupBox.Controls.Add(this.Thief1ExePathTextBox);
            this.PathsToGameExesGroupBox.Location = new System.Drawing.Point(8, 8);
            this.PathsToGameExesGroupBox.MinimumSize = new System.Drawing.Size(424, 0);
            this.PathsToGameExesGroupBox.Name = "PathsToGameExesGroupBox";
            this.PathsToGameExesGroupBox.Size = new System.Drawing.Size(424, 192);
            this.PathsToGameExesGroupBox.TabIndex = 1;
            this.PathsToGameExesGroupBox.TabStop = false;
            this.PathsToGameExesGroupBox.Text = "Paths to game executables";
            // 
            // GameRequirementsPanel
            // 
            this.GameRequirementsPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.GameRequirementsPanel.AutoScroll = true;
            this.GameRequirementsPanel.Controls.Add(this.GameRequirementsLabel);
            this.GameRequirementsPanel.Location = new System.Drawing.Point(16, 144);
            this.GameRequirementsPanel.Name = "GameRequirementsPanel";
            this.GameRequirementsPanel.Size = new System.Drawing.Size(392, 32);
            this.GameRequirementsPanel.TabIndex = 9;
            // 
            // GameRequirementsLabel
            // 
            this.GameRequirementsLabel.AutoSize = true;
            this.GameRequirementsLabel.Location = new System.Drawing.Point(0, 0);
            this.GameRequirementsLabel.Name = "GameRequirementsLabel";
            this.GameRequirementsLabel.Size = new System.Drawing.Size(191, 26);
            this.GameRequirementsLabel.TabIndex = 1;
            this.GameRequirementsLabel.Text = "* Thief 1 and Thief 2 require NewDark.\r\n* Thief 3 requires the Sneaky Upgrade.";
            this.GameRequirementsLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // Thief3ExePathLabel
            // 
            this.Thief3ExePathLabel.AutoSize = true;
            this.Thief3ExePathLabel.Location = new System.Drawing.Point(16, 104);
            this.Thief3ExePathLabel.Name = "Thief3ExePathLabel";
            this.Thief3ExePathLabel.Size = new System.Drawing.Size(43, 13);
            this.Thief3ExePathLabel.TabIndex = 8;
            this.Thief3ExePathLabel.Text = "Thief 3:";
            // 
            // Thief2ExePathLabel
            // 
            this.Thief2ExePathLabel.AutoSize = true;
            this.Thief2ExePathLabel.Location = new System.Drawing.Point(16, 64);
            this.Thief2ExePathLabel.Name = "Thief2ExePathLabel";
            this.Thief2ExePathLabel.Size = new System.Drawing.Size(43, 13);
            this.Thief2ExePathLabel.TabIndex = 4;
            this.Thief2ExePathLabel.Text = "Thief 2:";
            // 
            // Thief1ExePathLabel
            // 
            this.Thief1ExePathLabel.AutoSize = true;
            this.Thief1ExePathLabel.Location = new System.Drawing.Point(16, 24);
            this.Thief1ExePathLabel.Name = "Thief1ExePathLabel";
            this.Thief1ExePathLabel.Size = new System.Drawing.Size(43, 13);
            this.Thief1ExePathLabel.TabIndex = 0;
            this.Thief1ExePathLabel.Text = "Thief 1:";
            // 
            // Thief3ExePathBrowseButton
            // 
            this.Thief3ExePathBrowseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.Thief3ExePathBrowseButton.AutoSize = true;
            this.Thief3ExePathBrowseButton.Location = new System.Drawing.Point(336, 119);
            this.Thief3ExePathBrowseButton.Name = "Thief3ExePathBrowseButton";
            this.Thief3ExePathBrowseButton.Padding = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.Thief3ExePathBrowseButton.Size = new System.Drawing.Size(75, 23);
            this.Thief3ExePathBrowseButton.TabIndex = 6;
            this.Thief3ExePathBrowseButton.Text = "Browse...";
            this.Thief3ExePathBrowseButton.UseVisualStyleBackColor = true;
            // 
            // Thief2ExePathBrowseButton
            // 
            this.Thief2ExePathBrowseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.Thief2ExePathBrowseButton.AutoSize = true;
            this.Thief2ExePathBrowseButton.Location = new System.Drawing.Point(336, 79);
            this.Thief2ExePathBrowseButton.Name = "Thief2ExePathBrowseButton";
            this.Thief2ExePathBrowseButton.Padding = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.Thief2ExePathBrowseButton.Size = new System.Drawing.Size(75, 23);
            this.Thief2ExePathBrowseButton.TabIndex = 4;
            this.Thief2ExePathBrowseButton.Text = "Browse...";
            this.Thief2ExePathBrowseButton.UseVisualStyleBackColor = true;
            // 
            // Thief1ExePathBrowseButton
            // 
            this.Thief1ExePathBrowseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.Thief1ExePathBrowseButton.AutoSize = true;
            this.Thief1ExePathBrowseButton.Location = new System.Drawing.Point(336, 39);
            this.Thief1ExePathBrowseButton.Name = "Thief1ExePathBrowseButton";
            this.Thief1ExePathBrowseButton.Padding = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.Thief1ExePathBrowseButton.Size = new System.Drawing.Size(75, 23);
            this.Thief1ExePathBrowseButton.TabIndex = 2;
            this.Thief1ExePathBrowseButton.Text = "Browse...";
            this.Thief1ExePathBrowseButton.UseVisualStyleBackColor = true;
            // 
            // Thief3ExePathTextBox
            // 
            this.Thief3ExePathTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.Thief3ExePathTextBox.Location = new System.Drawing.Point(16, 120);
            this.Thief3ExePathTextBox.Name = "Thief3ExePathTextBox";
            this.Thief3ExePathTextBox.Size = new System.Drawing.Size(320, 20);
            this.Thief3ExePathTextBox.TabIndex = 5;
            // 
            // Thief2ExePathTextBox
            // 
            this.Thief2ExePathTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.Thief2ExePathTextBox.Location = new System.Drawing.Point(16, 80);
            this.Thief2ExePathTextBox.Name = "Thief2ExePathTextBox";
            this.Thief2ExePathTextBox.Size = new System.Drawing.Size(320, 20);
            this.Thief2ExePathTextBox.TabIndex = 3;
            // 
            // Thief1ExePathTextBox
            // 
            this.Thief1ExePathTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.Thief1ExePathTextBox.Location = new System.Drawing.Point(16, 40);
            this.Thief1ExePathTextBox.Name = "Thief1ExePathTextBox";
            this.Thief1ExePathTextBox.Size = new System.Drawing.Size(320, 20);
            this.Thief1ExePathTextBox.TabIndex = 1;
            // 
            // FMArchivePathsGroupBox
            // 
            this.FMArchivePathsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.FMArchivePathsGroupBox.Controls.Add(this.IncludeSubfoldersCheckBox);
            this.FMArchivePathsGroupBox.Controls.Add(this.AddFMArchivePathButton);
            this.FMArchivePathsGroupBox.Controls.Add(this.RemoveFMArchivePathButton);
            this.FMArchivePathsGroupBox.Controls.Add(this.FMArchivePathsListBox);
            this.FMArchivePathsGroupBox.Location = new System.Drawing.Point(8, 472);
            this.FMArchivePathsGroupBox.MinimumSize = new System.Drawing.Size(424, 0);
            this.FMArchivePathsGroupBox.Name = "FMArchivePathsGroupBox";
            this.FMArchivePathsGroupBox.Size = new System.Drawing.Size(424, 256);
            this.FMArchivePathsGroupBox.TabIndex = 3;
            this.FMArchivePathsGroupBox.TabStop = false;
            this.FMArchivePathsGroupBox.Text = "FM archive paths";
            // 
            // IncludeSubfoldersCheckBox
            // 
            this.IncludeSubfoldersCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.IncludeSubfoldersCheckBox.AutoSize = true;
            this.IncludeSubfoldersCheckBox.Location = new System.Drawing.Point(16, 228);
            this.IncludeSubfoldersCheckBox.Name = "IncludeSubfoldersCheckBox";
            this.IncludeSubfoldersCheckBox.Size = new System.Drawing.Size(112, 17);
            this.IncludeSubfoldersCheckBox.TabIndex = 1;
            this.IncludeSubfoldersCheckBox.Text = "Include subfolders";
            this.IncludeSubfoldersCheckBox.UseVisualStyleBackColor = true;
            // 
            // AddFMArchivePathButton
            // 
            this.AddFMArchivePathButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.AddFMArchivePathButton.Location = new System.Drawing.Point(386, 224);
            this.AddFMArchivePathButton.Name = "AddFMArchivePathButton";
            this.AddFMArchivePathButton.Size = new System.Drawing.Size(23, 23);
            this.AddFMArchivePathButton.TabIndex = 3;
            this.AddFMArchivePathButton.UseVisualStyleBackColor = true;
            this.AddFMArchivePathButton.Paint += new System.Windows.Forms.PaintEventHandler(this.AddFMArchivePathButton_Paint);
            // 
            // RemoveFMArchivePathButton
            // 
            this.RemoveFMArchivePathButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.RemoveFMArchivePathButton.Location = new System.Drawing.Point(362, 224);
            this.RemoveFMArchivePathButton.Name = "RemoveFMArchivePathButton";
            this.RemoveFMArchivePathButton.Size = new System.Drawing.Size(23, 23);
            this.RemoveFMArchivePathButton.TabIndex = 2;
            this.RemoveFMArchivePathButton.UseVisualStyleBackColor = true;
            this.RemoveFMArchivePathButton.Paint += new System.Windows.Forms.PaintEventHandler(this.RemoveFMArchivePathButton_Paint);
            // 
            // FMArchivePathsListBox
            // 
            this.FMArchivePathsListBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.FMArchivePathsListBox.FormattingEnabled = true;
            this.FMArchivePathsListBox.Location = new System.Drawing.Point(16, 24);
            this.FMArchivePathsListBox.Name = "FMArchivePathsListBox";
            this.FMArchivePathsListBox.Size = new System.Drawing.Size(392, 199);
            this.FMArchivePathsListBox.TabIndex = 0;
            // 
            // OtherGroupBox
            // 
            this.OtherGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.OtherGroupBox.Controls.Add(this.BackupPathLabel);
            this.OtherGroupBox.Controls.Add(this.BackupPathBrowseButton);
            this.OtherGroupBox.Controls.Add(this.BackupPathTextBox);
            this.OtherGroupBox.Location = new System.Drawing.Point(8, 384);
            this.OtherGroupBox.MinimumSize = new System.Drawing.Size(424, 0);
            this.OtherGroupBox.Name = "OtherGroupBox";
            this.OtherGroupBox.Size = new System.Drawing.Size(424, 72);
            this.OtherGroupBox.TabIndex = 2;
            this.OtherGroupBox.TabStop = false;
            this.OtherGroupBox.Text = "Other";
            // 
            // BackupPathLabel
            // 
            this.BackupPathLabel.AutoSize = true;
            this.BackupPathLabel.Location = new System.Drawing.Point(16, 24);
            this.BackupPathLabel.Name = "BackupPathLabel";
            this.BackupPathLabel.Size = new System.Drawing.Size(88, 13);
            this.BackupPathLabel.TabIndex = 0;
            this.BackupPathLabel.Text = "FM backup path:";
            // 
            // BackupPathBrowseButton
            // 
            this.BackupPathBrowseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.BackupPathBrowseButton.AutoSize = true;
            this.BackupPathBrowseButton.Location = new System.Drawing.Point(336, 39);
            this.BackupPathBrowseButton.Name = "BackupPathBrowseButton";
            this.BackupPathBrowseButton.Padding = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.BackupPathBrowseButton.Size = new System.Drawing.Size(75, 23);
            this.BackupPathBrowseButton.TabIndex = 2;
            this.BackupPathBrowseButton.Text = "Browse...";
            this.BackupPathBrowseButton.UseVisualStyleBackColor = true;
            // 
            // BackupPathTextBox
            // 
            this.BackupPathTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.BackupPathTextBox.Location = new System.Drawing.Point(16, 40);
            this.BackupPathTextBox.Name = "BackupPathTextBox";
            this.BackupPathTextBox.Size = new System.Drawing.Size(320, 20);
            this.BackupPathTextBox.TabIndex = 1;
            // 
            // DummyAutoScrollPanel
            // 
            this.DummyAutoScrollPanel.Location = new System.Drawing.Point(8, 200);
            this.DummyAutoScrollPanel.Name = "DummyAutoScrollPanel";
            this.DummyAutoScrollPanel.Size = new System.Drawing.Size(424, 8);
            this.DummyAutoScrollPanel.TabIndex = 13;
            // 
            // PathsPage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.PagePanel);
            this.Name = "PathsPage";
            this.Size = new System.Drawing.Size(440, 756);
            this.PagePanel.ResumeLayout(false);
            this.ActualPathsPanel.ResumeLayout(false);
            this.SteamGroupBox.ResumeLayout(false);
            this.SteamGroupBox.PerformLayout();
            this.PathsToGameExesGroupBox.ResumeLayout(false);
            this.PathsToGameExesGroupBox.PerformLayout();
            this.GameRequirementsPanel.ResumeLayout(false);
            this.GameRequirementsPanel.PerformLayout();
            this.FMArchivePathsGroupBox.ResumeLayout(false);
            this.FMArchivePathsGroupBox.PerformLayout();
            this.OtherGroupBox.ResumeLayout(false);
            this.OtherGroupBox.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        internal System.Windows.Forms.Panel PagePanel;
        internal System.Windows.Forms.GroupBox OtherGroupBox;
        internal System.Windows.Forms.Label BackupPathLabel;
        internal System.Windows.Forms.Button BackupPathBrowseButton;
        internal System.Windows.Forms.TextBox BackupPathTextBox;
        internal System.Windows.Forms.GroupBox PathsToGameExesGroupBox;
        internal System.Windows.Forms.Panel GameRequirementsPanel;
        internal System.Windows.Forms.Label GameRequirementsLabel;
        internal System.Windows.Forms.Label Thief3ExePathLabel;
        internal System.Windows.Forms.Label Thief2ExePathLabel;
        internal System.Windows.Forms.Label Thief1ExePathLabel;
        internal System.Windows.Forms.Button Thief3ExePathBrowseButton;
        internal System.Windows.Forms.Button Thief2ExePathBrowseButton;
        internal System.Windows.Forms.Button Thief1ExePathBrowseButton;
        internal System.Windows.Forms.TextBox Thief3ExePathTextBox;
        internal System.Windows.Forms.TextBox Thief2ExePathTextBox;
        internal System.Windows.Forms.TextBox Thief1ExePathTextBox;
        internal System.Windows.Forms.GroupBox FMArchivePathsGroupBox;
        internal System.Windows.Forms.CheckBox IncludeSubfoldersCheckBox;
        internal System.Windows.Forms.Button AddFMArchivePathButton;
        internal System.Windows.Forms.Button RemoveFMArchivePathButton;
        internal System.Windows.Forms.ListBox FMArchivePathsListBox;
        internal System.Windows.Forms.Panel ActualPathsPanel;
        internal System.Windows.Forms.Panel DummyAutoScrollPanel;
        internal System.Windows.Forms.Label SteamExeLabel;
        internal System.Windows.Forms.TextBox SteamExeTextBox;
        internal System.Windows.Forms.Button SteamExeBrowseButton;
        internal System.Windows.Forms.GroupBox SteamGroupBox;
        internal System.Windows.Forms.Label LaunchTheseGamesThroughSteamLabel;
        internal System.Windows.Forms.CheckBox T3UseSteamCheckBox;
        internal System.Windows.Forms.CheckBox T2UseSteamCheckBox;
        internal System.Windows.Forms.CheckBox T1UseSteamCheckBox;
    }
}
