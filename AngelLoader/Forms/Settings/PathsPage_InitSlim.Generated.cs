namespace AngelLoader.Forms;

sealed partial class PathsPage
{
    /// <summary>
    /// Custom generated component initializer with cruft removed.
    /// </summary>
    private void InitSlim()
    {
        this.PagePanel = new System.Windows.Forms.Panel();
        this.ActualPathsPanel = new System.Windows.Forms.Panel();
        this.LayoutFLP = new System.Windows.Forms.FlowLayoutPanel();
        this.OtherGroupBox = new AngelLoader.Forms.CustomControls.DarkGroupBox();
        this.BackupPathPanel = new System.Windows.Forms.Panel();
        this.BackupHelpPictureBox = new System.Windows.Forms.PictureBox();
        this.BackupPathHelpLabel = new AngelLoader.Forms.CustomControls.DarkLabel();
        this.BackupPathLabel = new AngelLoader.Forms.CustomControls.DarkLabel();
        this.BackupPathTextBox = new AngelLoader.Forms.CustomControls.DarkTextBox();
        this.BackupPathBrowseButton = new AngelLoader.Forms.CustomControls.DarkButton();
        this.FMArchivePathsGroupBox = new AngelLoader.Forms.CustomControls.DarkGroupBox();
        this.IncludeSubfoldersCheckBox = new AngelLoader.Forms.CustomControls.DarkCheckBox();
        this.AddFMArchivePathButton = new AngelLoader.Forms.CustomControls.DarkButton();
        this.RemoveFMArchivePathButton = new AngelLoader.Forms.CustomControls.DarkButton();
        this.FMArchivePathsListBox = new AngelLoader.Forms.CustomControls.DarkListBox();
        this.SteamOptionsGroupBox = new AngelLoader.Forms.CustomControls.DarkGroupBox();
        this.LaunchTheseGamesThroughSteamPanel = new System.Windows.Forms.Panel();
        this.LaunchTheseGamesThroughSteamCheckBox = new AngelLoader.Forms.CustomControls.DarkCheckBox();
        this.Thief1UseSteamCheckBox = new AngelLoader.Forms.CustomControls.DarkCheckBox();
        this.SS2UseSteamCheckBox = new AngelLoader.Forms.CustomControls.DarkCheckBox();
        this.Thief3UseSteamCheckBox = new AngelLoader.Forms.CustomControls.DarkCheckBox();
        this.Thief2UseSteamCheckBox = new AngelLoader.Forms.CustomControls.DarkCheckBox();
        this.SteamExeLabel = new AngelLoader.Forms.CustomControls.DarkLabel();
        this.SteamExeTextBox = new AngelLoader.Forms.CustomControls.DarkTextBox();
        this.SteamExeBrowseButton = new AngelLoader.Forms.CustomControls.DarkButton();
        this.PathsToGameExesGroupBox = new AngelLoader.Forms.CustomControls.DarkGroupBox();
        this.GameRequirementsPanel = new System.Windows.Forms.Panel();
        this.GameRequirementsLabel = new AngelLoader.Forms.CustomControls.DarkLabel();
        this.SS2ExePathLabel = new AngelLoader.Forms.CustomControls.DarkLabel();
        this.Thief3ExePathLabel = new AngelLoader.Forms.CustomControls.DarkLabel();
        this.Thief2ExePathLabel = new AngelLoader.Forms.CustomControls.DarkLabel();
        this.Thief1ExePathLabel = new AngelLoader.Forms.CustomControls.DarkLabel();
        this.SS2ExePathBrowseButton = new AngelLoader.Forms.CustomControls.DarkButton();
        this.Thief3ExePathBrowseButton = new AngelLoader.Forms.CustomControls.DarkButton();
        this.Thief2ExePathBrowseButton = new AngelLoader.Forms.CustomControls.DarkButton();
        this.Thief1ExePathBrowseButton = new AngelLoader.Forms.CustomControls.DarkButton();
        this.SS2ExePathTextBox = new AngelLoader.Forms.CustomControls.DarkTextBox();
        this.Thief3ExePathTextBox = new AngelLoader.Forms.CustomControls.DarkTextBox();
        this.Thief2ExePathTextBox = new AngelLoader.Forms.CustomControls.DarkTextBox();
        this.Thief1ExePathTextBox = new AngelLoader.Forms.CustomControls.DarkTextBox();
        this.DummyAutoScrollPanel = new System.Windows.Forms.Control();
        this.PagePanel.SuspendLayout();
        this.ActualPathsPanel.SuspendLayout();
        this.LayoutFLP.SuspendLayout();
        this.OtherGroupBox.SuspendLayout();
        this.BackupPathPanel.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.BackupHelpPictureBox)).BeginInit();
        this.FMArchivePathsGroupBox.SuspendLayout();
        this.SteamOptionsGroupBox.SuspendLayout();
        this.LaunchTheseGamesThroughSteamPanel.SuspendLayout();
        this.PathsToGameExesGroupBox.SuspendLayout();
        this.GameRequirementsPanel.SuspendLayout();
        this.SuspendLayout();
        // 
        // PagePanel
        // 
        this.PagePanel.AutoScroll = true;
        this.PagePanel.Controls.Add(this.ActualPathsPanel);
        this.PagePanel.Controls.Add(this.DummyAutoScrollPanel);
        this.PagePanel.Dock = System.Windows.Forms.DockStyle.Fill;
        this.PagePanel.Size = new System.Drawing.Size(440, 847);
        this.PagePanel.TabIndex = 3;
        // 
        // ActualPathsPanel
        // 
        this.ActualPathsPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
        | System.Windows.Forms.AnchorStyles.Right)));
        this.ActualPathsPanel.Controls.Add(this.LayoutFLP);
        this.ActualPathsPanel.Controls.Add(this.SteamOptionsGroupBox);
        this.ActualPathsPanel.Controls.Add(this.PathsToGameExesGroupBox);
        this.ActualPathsPanel.MinimumSize = new System.Drawing.Size(440, 0);
        this.ActualPathsPanel.Size = new System.Drawing.Size(440, 824);
        this.ActualPathsPanel.TabIndex = 4;
        // 
        // LayoutFLP
        // 
        this.LayoutFLP.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
        | System.Windows.Forms.AnchorStyles.Right)));
        this.LayoutFLP.Controls.Add(this.OtherGroupBox);
        this.LayoutFLP.Controls.Add(this.FMArchivePathsGroupBox);
        this.LayoutFLP.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
        this.LayoutFLP.Location = new System.Drawing.Point(0, 437);
        this.LayoutFLP.Margin = new System.Windows.Forms.Padding(0, 3, 0, 3);
        this.LayoutFLP.Padding = new System.Windows.Forms.Padding(5, 0, 0, 0);
        this.LayoutFLP.Size = new System.Drawing.Size(440, 392);
        this.LayoutFLP.TabIndex = 4;
        this.LayoutFLP.WrapContents = false;
        this.LayoutFLP.Layout += new System.Windows.Forms.LayoutEventHandler(this.LayoutFLP_Layout);
        // 
        // OtherGroupBox
        // 
        this.OtherGroupBox.Controls.Add(this.BackupPathPanel);
        this.OtherGroupBox.MinimumSize = new System.Drawing.Size(424, 0);
        this.OtherGroupBox.Size = new System.Drawing.Size(424, 117);
        this.OtherGroupBox.TabIndex = 2;
        this.OtherGroupBox.TabStop = false;
        // 
        // BackupPathPanel
        // 
        this.BackupPathPanel.AutoScroll = true;
        this.BackupPathPanel.Controls.Add(this.BackupHelpPictureBox);
        this.BackupPathPanel.Controls.Add(this.BackupPathHelpLabel);
        this.BackupPathPanel.Controls.Add(this.BackupPathLabel);
        this.BackupPathPanel.Controls.Add(this.BackupPathTextBox);
        this.BackupPathPanel.Controls.Add(this.BackupPathBrowseButton);
        this.BackupPathPanel.Dock = System.Windows.Forms.DockStyle.Fill;
        this.BackupPathPanel.Margin = new System.Windows.Forms.Padding(0);
        this.BackupPathPanel.Size = new System.Drawing.Size(418, 98);
        this.BackupPathPanel.TabIndex = 4;
        // 
        // BackupHelpPictureBox
        // 
        this.BackupHelpPictureBox.Location = new System.Drawing.Point(13, 56);
        this.BackupHelpPictureBox.Size = new System.Drawing.Size(16, 16);
        this.BackupHelpPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
        // 
        // BackupPathHelpLabel
        // 
        this.BackupPathHelpLabel.AutoSize = true;
        this.BackupPathHelpLabel.Location = new System.Drawing.Point(32, 56);
        this.BackupPathHelpLabel.MaximumSize = new System.Drawing.Size(380, 0);
        this.BackupPathHelpLabel.TextChanged += new System.EventHandler(this.BackupPathHelpLabel_TextChanged);
        // 
        // BackupPathLabel
        // 
        this.BackupPathLabel.AutoSize = true;
        this.BackupPathLabel.Location = new System.Drawing.Point(13, 8);
        // 
        // BackupPathTextBox
        // 
        this.BackupPathTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
        | System.Windows.Forms.AnchorStyles.Right)));
        this.BackupPathTextBox.Location = new System.Drawing.Point(13, 24);
        this.BackupPathTextBox.Size = new System.Drawing.Size(321, 20);
        this.BackupPathTextBox.TabIndex = 1;
        // 
        // BackupPathBrowseButton
        // 
        this.BackupPathBrowseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
        this.BackupPathBrowseButton.AutoSize = true;
        this.BackupPathBrowseButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        this.BackupPathBrowseButton.Location = new System.Drawing.Point(333, 23);
        this.BackupPathBrowseButton.MinimumSize = new System.Drawing.Size(75, 23);
        this.BackupPathBrowseButton.Padding = new System.Windows.Forms.Padding(6, 0, 6, 0);
        this.BackupPathBrowseButton.TabIndex = 2;
        this.BackupPathBrowseButton.UseVisualStyleBackColor = true;
        // 
        // FMArchivePathsGroupBox
        // 
        this.FMArchivePathsGroupBox.Controls.Add(this.IncludeSubfoldersCheckBox);
        this.FMArchivePathsGroupBox.Controls.Add(this.AddFMArchivePathButton);
        this.FMArchivePathsGroupBox.Controls.Add(this.RemoveFMArchivePathButton);
        this.FMArchivePathsGroupBox.Controls.Add(this.FMArchivePathsListBox);
        this.FMArchivePathsGroupBox.Margin = new System.Windows.Forms.Padding(3, 9, 3, 3);
        this.FMArchivePathsGroupBox.MinimumSize = new System.Drawing.Size(424, 0);
        this.FMArchivePathsGroupBox.Size = new System.Drawing.Size(424, 258);
        this.FMArchivePathsGroupBox.TabIndex = 3;
        this.FMArchivePathsGroupBox.TabStop = false;
        // 
        // IncludeSubfoldersCheckBox
        // 
        this.IncludeSubfoldersCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
        this.IncludeSubfoldersCheckBox.AutoSize = true;
        this.IncludeSubfoldersCheckBox.Location = new System.Drawing.Point(16, 224);
        this.IncludeSubfoldersCheckBox.Size = new System.Drawing.Size(112, 17);
        this.IncludeSubfoldersCheckBox.TabIndex = 1;
        this.IncludeSubfoldersCheckBox.UseVisualStyleBackColor = true;
        // 
        // AddFMArchivePathButton
        // 
        this.AddFMArchivePathButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
        this.AddFMArchivePathButton.Location = new System.Drawing.Point(386, 224);
        this.AddFMArchivePathButton.Size = new System.Drawing.Size(23, 23);
        this.AddFMArchivePathButton.TabIndex = 3;
        this.AddFMArchivePathButton.UseVisualStyleBackColor = true;
        this.AddFMArchivePathButton.PaintCustom += new System.Windows.Forms.PaintEventHandler(this.AddFMArchivePathButton_Paint);
        // 
        // RemoveFMArchivePathButton
        // 
        this.RemoveFMArchivePathButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
        this.RemoveFMArchivePathButton.Location = new System.Drawing.Point(362, 224);
        this.RemoveFMArchivePathButton.Size = new System.Drawing.Size(23, 23);
        this.RemoveFMArchivePathButton.TabIndex = 2;
        this.RemoveFMArchivePathButton.UseVisualStyleBackColor = true;
        this.RemoveFMArchivePathButton.PaintCustom += new System.Windows.Forms.PaintEventHandler(this.RemoveFMArchivePathButton_Paint);
        // 
        // FMArchivePathsListBox
        // 
        this.FMArchivePathsListBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
        | System.Windows.Forms.AnchorStyles.Right)));
        this.FMArchivePathsListBox.Location = new System.Drawing.Point(16, 24);
        this.FMArchivePathsListBox.MultiSelect = false;
        this.FMArchivePathsListBox.Size = new System.Drawing.Size(392, 199);
        this.FMArchivePathsListBox.TabIndex = 0;
        // 
        // SteamOptionsGroupBox
        // 
        this.SteamOptionsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
        | System.Windows.Forms.AnchorStyles.Right)));
        this.SteamOptionsGroupBox.Controls.Add(this.LaunchTheseGamesThroughSteamPanel);
        this.SteamOptionsGroupBox.Controls.Add(this.SteamExeLabel);
        this.SteamOptionsGroupBox.Controls.Add(this.SteamExeTextBox);
        this.SteamOptionsGroupBox.Controls.Add(this.SteamExeBrowseButton);
        this.SteamOptionsGroupBox.Location = new System.Drawing.Point(8, 252);
        this.SteamOptionsGroupBox.Size = new System.Drawing.Size(424, 176);
        this.SteamOptionsGroupBox.TabIndex = 1;
        this.SteamOptionsGroupBox.TabStop = false;
        // 
        // LaunchTheseGamesThroughSteamPanel
        // 
        this.LaunchTheseGamesThroughSteamPanel.Controls.Add(this.LaunchTheseGamesThroughSteamCheckBox);
        this.LaunchTheseGamesThroughSteamPanel.Controls.Add(this.Thief1UseSteamCheckBox);
        this.LaunchTheseGamesThroughSteamPanel.Controls.Add(this.SS2UseSteamCheckBox);
        this.LaunchTheseGamesThroughSteamPanel.Controls.Add(this.Thief3UseSteamCheckBox);
        this.LaunchTheseGamesThroughSteamPanel.Controls.Add(this.Thief2UseSteamCheckBox);
        this.LaunchTheseGamesThroughSteamPanel.Enabled = false;
        this.LaunchTheseGamesThroughSteamPanel.Location = new System.Drawing.Point(16, 72);
        this.LaunchTheseGamesThroughSteamPanel.Size = new System.Drawing.Size(392, 96);
        this.LaunchTheseGamesThroughSteamPanel.TabIndex = 7;
        // 
        // LaunchTheseGamesThroughSteamCheckBox
        // 
        this.LaunchTheseGamesThroughSteamCheckBox.AutoSize = true;
        this.LaunchTheseGamesThroughSteamCheckBox.Checked = true;
        this.LaunchTheseGamesThroughSteamCheckBox.TabIndex = 0;
        this.LaunchTheseGamesThroughSteamCheckBox.UseVisualStyleBackColor = true;
        // 
        // Thief1UseSteamCheckBox
        // 
        this.Thief1UseSteamCheckBox.AutoSize = true;
        this.Thief1UseSteamCheckBox.Checked = true;
        this.Thief1UseSteamCheckBox.Location = new System.Drawing.Point(8, 24);
        this.Thief1UseSteamCheckBox.TabIndex = 1;
        this.Thief1UseSteamCheckBox.UseVisualStyleBackColor = true;
        // 
        // SS2UseSteamCheckBox
        // 
        this.SS2UseSteamCheckBox.AutoSize = true;
        this.SS2UseSteamCheckBox.Checked = true;
        this.SS2UseSteamCheckBox.Location = new System.Drawing.Point(8, 72);
        this.SS2UseSteamCheckBox.TabIndex = 4;
        this.SS2UseSteamCheckBox.UseVisualStyleBackColor = true;
        // 
        // Thief3UseSteamCheckBox
        // 
        this.Thief3UseSteamCheckBox.AutoSize = true;
        this.Thief3UseSteamCheckBox.Checked = true;
        this.Thief3UseSteamCheckBox.Location = new System.Drawing.Point(8, 56);
        this.Thief3UseSteamCheckBox.TabIndex = 3;
        this.Thief3UseSteamCheckBox.UseVisualStyleBackColor = true;
        // 
        // Thief2UseSteamCheckBox
        // 
        this.Thief2UseSteamCheckBox.AutoSize = true;
        this.Thief2UseSteamCheckBox.Checked = true;
        this.Thief2UseSteamCheckBox.Location = new System.Drawing.Point(8, 40);
        this.Thief2UseSteamCheckBox.TabIndex = 2;
        this.Thief2UseSteamCheckBox.UseVisualStyleBackColor = true;
        // 
        // SteamExeLabel
        // 
        this.SteamExeLabel.AutoSize = true;
        this.SteamExeLabel.Location = new System.Drawing.Point(16, 24);
        // 
        // SteamExeTextBox
        // 
        this.SteamExeTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
        | System.Windows.Forms.AnchorStyles.Right)));
        this.SteamExeTextBox.Location = new System.Drawing.Point(16, 40);
        this.SteamExeTextBox.Size = new System.Drawing.Size(320, 20);
        this.SteamExeTextBox.TabIndex = 1;
        // 
        // SteamExeBrowseButton
        // 
        this.SteamExeBrowseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
        this.SteamExeBrowseButton.AutoSize = true;
        this.SteamExeBrowseButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        this.SteamExeBrowseButton.Location = new System.Drawing.Point(336, 39);
        this.SteamExeBrowseButton.MinimumSize = new System.Drawing.Size(75, 23);
        this.SteamExeBrowseButton.Padding = new System.Windows.Forms.Padding(6, 0, 6, 0);
        this.SteamExeBrowseButton.TabIndex = 2;
        this.SteamExeBrowseButton.UseVisualStyleBackColor = true;
        // 
        // PathsToGameExesGroupBox
        // 
        this.PathsToGameExesGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
        | System.Windows.Forms.AnchorStyles.Right)));
        this.PathsToGameExesGroupBox.Controls.Add(this.GameRequirementsPanel);
        this.PathsToGameExesGroupBox.Controls.Add(this.SS2ExePathLabel);
        this.PathsToGameExesGroupBox.Controls.Add(this.Thief3ExePathLabel);
        this.PathsToGameExesGroupBox.Controls.Add(this.Thief2ExePathLabel);
        this.PathsToGameExesGroupBox.Controls.Add(this.Thief1ExePathLabel);
        this.PathsToGameExesGroupBox.Controls.Add(this.SS2ExePathBrowseButton);
        this.PathsToGameExesGroupBox.Controls.Add(this.Thief3ExePathBrowseButton);
        this.PathsToGameExesGroupBox.Controls.Add(this.Thief2ExePathBrowseButton);
        this.PathsToGameExesGroupBox.Controls.Add(this.Thief1ExePathBrowseButton);
        this.PathsToGameExesGroupBox.Controls.Add(this.SS2ExePathTextBox);
        this.PathsToGameExesGroupBox.Controls.Add(this.Thief3ExePathTextBox);
        this.PathsToGameExesGroupBox.Controls.Add(this.Thief2ExePathTextBox);
        this.PathsToGameExesGroupBox.Controls.Add(this.Thief1ExePathTextBox);
        this.PathsToGameExesGroupBox.Location = new System.Drawing.Point(8, 8);
        this.PathsToGameExesGroupBox.MinimumSize = new System.Drawing.Size(424, 0);
        this.PathsToGameExesGroupBox.Size = new System.Drawing.Size(424, 232);
        this.PathsToGameExesGroupBox.TabIndex = 0;
        this.PathsToGameExesGroupBox.TabStop = false;
        // 
        // GameRequirementsPanel
        // 
        this.GameRequirementsPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
        | System.Windows.Forms.AnchorStyles.Right)));
        this.GameRequirementsPanel.AutoScroll = true;
        this.GameRequirementsPanel.Controls.Add(this.GameRequirementsLabel);
        this.GameRequirementsPanel.Location = new System.Drawing.Point(16, 192);
        this.GameRequirementsPanel.Size = new System.Drawing.Size(392, 32);
        this.GameRequirementsPanel.TabIndex = 12;
        // 
        // GameRequirementsLabel
        // 
        this.GameRequirementsLabel.AutoSize = true;
        this.GameRequirementsLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        // 
        // SS2ExePathLabel
        // 
        this.SS2ExePathLabel.AutoSize = true;
        this.SS2ExePathLabel.Location = new System.Drawing.Point(16, 144);
        // 
        // Thief3ExePathLabel
        // 
        this.Thief3ExePathLabel.AutoSize = true;
        this.Thief3ExePathLabel.Location = new System.Drawing.Point(16, 104);
        // 
        // Thief2ExePathLabel
        // 
        this.Thief2ExePathLabel.AutoSize = true;
        this.Thief2ExePathLabel.Location = new System.Drawing.Point(16, 64);
        // 
        // Thief1ExePathLabel
        // 
        this.Thief1ExePathLabel.AutoSize = true;
        this.Thief1ExePathLabel.Location = new System.Drawing.Point(16, 24);
        // 
        // SS2ExePathBrowseButton
        // 
        this.SS2ExePathBrowseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
        this.SS2ExePathBrowseButton.AutoSize = true;
        this.SS2ExePathBrowseButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        this.SS2ExePathBrowseButton.Location = new System.Drawing.Point(336, 159);
        this.SS2ExePathBrowseButton.MinimumSize = new System.Drawing.Size(75, 23);
        this.SS2ExePathBrowseButton.Padding = new System.Windows.Forms.Padding(6, 0, 6, 0);
        this.SS2ExePathBrowseButton.TabIndex = 11;
        this.SS2ExePathBrowseButton.UseVisualStyleBackColor = true;
        // 
        // Thief3ExePathBrowseButton
        // 
        this.Thief3ExePathBrowseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
        this.Thief3ExePathBrowseButton.AutoSize = true;
        this.Thief3ExePathBrowseButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        this.Thief3ExePathBrowseButton.Location = new System.Drawing.Point(336, 119);
        this.Thief3ExePathBrowseButton.MinimumSize = new System.Drawing.Size(75, 23);
        this.Thief3ExePathBrowseButton.Padding = new System.Windows.Forms.Padding(6, 0, 6, 0);
        this.Thief3ExePathBrowseButton.TabIndex = 8;
        this.Thief3ExePathBrowseButton.UseVisualStyleBackColor = true;
        // 
        // Thief2ExePathBrowseButton
        // 
        this.Thief2ExePathBrowseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
        this.Thief2ExePathBrowseButton.AutoSize = true;
        this.Thief2ExePathBrowseButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        this.Thief2ExePathBrowseButton.Location = new System.Drawing.Point(336, 79);
        this.Thief2ExePathBrowseButton.MinimumSize = new System.Drawing.Size(75, 23);
        this.Thief2ExePathBrowseButton.Padding = new System.Windows.Forms.Padding(6, 0, 6, 0);
        this.Thief2ExePathBrowseButton.TabIndex = 5;
        this.Thief2ExePathBrowseButton.UseVisualStyleBackColor = true;
        // 
        // Thief1ExePathBrowseButton
        // 
        this.Thief1ExePathBrowseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
        this.Thief1ExePathBrowseButton.AutoSize = true;
        this.Thief1ExePathBrowseButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        this.Thief1ExePathBrowseButton.Location = new System.Drawing.Point(336, 39);
        this.Thief1ExePathBrowseButton.MinimumSize = new System.Drawing.Size(75, 23);
        this.Thief1ExePathBrowseButton.Padding = new System.Windows.Forms.Padding(6, 0, 6, 0);
        this.Thief1ExePathBrowseButton.TabIndex = 2;
        this.Thief1ExePathBrowseButton.UseVisualStyleBackColor = true;
        // 
        // SS2ExePathTextBox
        // 
        this.SS2ExePathTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
        | System.Windows.Forms.AnchorStyles.Right)));
        this.SS2ExePathTextBox.Location = new System.Drawing.Point(16, 160);
        this.SS2ExePathTextBox.Size = new System.Drawing.Size(320, 20);
        this.SS2ExePathTextBox.TabIndex = 10;
        // 
        // Thief3ExePathTextBox
        // 
        this.Thief3ExePathTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
        | System.Windows.Forms.AnchorStyles.Right)));
        this.Thief3ExePathTextBox.Location = new System.Drawing.Point(16, 120);
        this.Thief3ExePathTextBox.Size = new System.Drawing.Size(320, 20);
        this.Thief3ExePathTextBox.TabIndex = 7;
        // 
        // Thief2ExePathTextBox
        // 
        this.Thief2ExePathTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
        | System.Windows.Forms.AnchorStyles.Right)));
        this.Thief2ExePathTextBox.Location = new System.Drawing.Point(16, 80);
        this.Thief2ExePathTextBox.Size = new System.Drawing.Size(320, 20);
        this.Thief2ExePathTextBox.TabIndex = 4;
        // 
        // Thief1ExePathTextBox
        // 
        this.Thief1ExePathTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
        | System.Windows.Forms.AnchorStyles.Right)));
        this.Thief1ExePathTextBox.Location = new System.Drawing.Point(16, 40);
        this.Thief1ExePathTextBox.Size = new System.Drawing.Size(320, 20);
        this.Thief1ExePathTextBox.TabIndex = 1;
        // 
        // DummyAutoScrollPanel
        // 
        this.DummyAutoScrollPanel.Location = new System.Drawing.Point(8, 200);
        this.DummyAutoScrollPanel.Size = new System.Drawing.Size(424, 8);
        // 
        // PathsPage
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.Controls.Add(this.PagePanel);
        this.Size = new System.Drawing.Size(440, 847);
        this.PagePanel.ResumeLayout(false);
        this.ActualPathsPanel.ResumeLayout(false);
        this.LayoutFLP.ResumeLayout(false);
        this.OtherGroupBox.ResumeLayout(false);
        this.BackupPathPanel.ResumeLayout(false);
        this.BackupPathPanel.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)(this.BackupHelpPictureBox)).EndInit();
        this.FMArchivePathsGroupBox.ResumeLayout(false);
        this.FMArchivePathsGroupBox.PerformLayout();
        this.SteamOptionsGroupBox.ResumeLayout(false);
        this.SteamOptionsGroupBox.PerformLayout();
        this.LaunchTheseGamesThroughSteamPanel.ResumeLayout(false);
        this.LaunchTheseGamesThroughSteamPanel.PerformLayout();
        this.PathsToGameExesGroupBox.ResumeLayout(false);
        this.PathsToGameExesGroupBox.PerformLayout();
        this.GameRequirementsPanel.ResumeLayout(false);
        this.GameRequirementsPanel.PerformLayout();
        this.ResumeLayout(false);
    }
}
