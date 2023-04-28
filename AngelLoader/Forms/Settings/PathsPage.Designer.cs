#define FenGen_DesignerSource

namespace AngelLoader.Forms;

sealed partial class PathsPage
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

#if DEBUG
    /// <summary> 
    /// Required method for Designer support - do not modify 
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
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
        this.SteamExeBrowseButton = new AngelLoader.Forms.CustomControls.StandardButton();
        this.PathsToGameExesGroupBox = new AngelLoader.Forms.CustomControls.DarkGroupBox();
        this.GameRequirementsPanel = new System.Windows.Forms.Panel();
        this.GameRequirementsLabel = new AngelLoader.Forms.CustomControls.DarkLabel();
        this.SS2ExePathLabel = new AngelLoader.Forms.CustomControls.DarkLabel();
        this.Thief3ExePathLabel = new AngelLoader.Forms.CustomControls.DarkLabel();
        this.Thief2ExePathLabel = new AngelLoader.Forms.CustomControls.DarkLabel();
        this.Thief1ExePathLabel = new AngelLoader.Forms.CustomControls.DarkLabel();
        this.SS2ExePathBrowseButton = new AngelLoader.Forms.CustomControls.StandardButton();
        this.Thief3ExePathBrowseButton = new AngelLoader.Forms.CustomControls.StandardButton();
        this.Thief2ExePathBrowseButton = new AngelLoader.Forms.CustomControls.StandardButton();
        this.Thief1ExePathBrowseButton = new AngelLoader.Forms.CustomControls.StandardButton();
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
        this.PagePanel.Location = new System.Drawing.Point(0, 0);
        this.PagePanel.Name = "PagePanel";
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
        this.ActualPathsPanel.Location = new System.Drawing.Point(0, 0);
        this.ActualPathsPanel.MinimumSize = new System.Drawing.Size(440, 0);
        this.ActualPathsPanel.Name = "ActualPathsPanel";
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
        this.LayoutFLP.Name = "LayoutFLP";
        this.LayoutFLP.Padding = new System.Windows.Forms.Padding(5, 0, 0, 0);
        this.LayoutFLP.Size = new System.Drawing.Size(440, 392);
        this.LayoutFLP.TabIndex = 4;
        this.LayoutFLP.WrapContents = false;
        this.LayoutFLP.Layout += new System.Windows.Forms.LayoutEventHandler(this.LayoutFLP_Layout);
        // 
        // OtherGroupBox
        // 
        this.OtherGroupBox.Controls.Add(this.BackupPathPanel);
        this.OtherGroupBox.Location = new System.Drawing.Point(8, 3);
        this.OtherGroupBox.MinimumSize = new System.Drawing.Size(424, 0);
        this.OtherGroupBox.Name = "OtherGroupBox";
        this.OtherGroupBox.Size = new System.Drawing.Size(424, 117);
        this.OtherGroupBox.TabIndex = 2;
        this.OtherGroupBox.TabStop = false;
        this.OtherGroupBox.Text = "Other";
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
        this.BackupPathPanel.Location = new System.Drawing.Point(3, 16);
        this.BackupPathPanel.Margin = new System.Windows.Forms.Padding(0);
        this.BackupPathPanel.Name = "BackupPathPanel";
        this.BackupPathPanel.Size = new System.Drawing.Size(418, 98);
        this.BackupPathPanel.TabIndex = 4;
        // 
        // BackupHelpPictureBox
        // 
        this.BackupHelpPictureBox.Location = new System.Drawing.Point(13, 56);
        this.BackupHelpPictureBox.Name = "BackupHelpPictureBox";
        this.BackupHelpPictureBox.Size = new System.Drawing.Size(16, 16);
        this.BackupHelpPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
        this.BackupHelpPictureBox.TabIndex = 7;
        this.BackupHelpPictureBox.TabStop = false;
        // 
        // BackupPathHelpLabel
        // 
        this.BackupPathHelpLabel.AutoSize = true;
        this.BackupPathHelpLabel.Location = new System.Drawing.Point(32, 56);
        this.BackupPathHelpLabel.MaximumSize = new System.Drawing.Size(380, 0);
        this.BackupPathHelpLabel.Name = "BackupPathHelpLabel";
        this.BackupPathHelpLabel.Size = new System.Drawing.Size(121, 13);
        this.BackupPathHelpLabel.TabIndex = 6;
        this.BackupPathHelpLabel.Text = "[multi line help message]";
        this.BackupPathHelpLabel.TextChanged += new System.EventHandler(this.BackupPathHelpLabel_TextChanged);
        // 
        // BackupPathLabel
        // 
        this.BackupPathLabel.AutoSize = true;
        this.BackupPathLabel.Location = new System.Drawing.Point(13, 8);
        this.BackupPathLabel.Name = "BackupPathLabel";
        this.BackupPathLabel.Size = new System.Drawing.Size(174, 13);
        this.BackupPathLabel.TabIndex = 0;
        this.BackupPathLabel.Text = "Path to place new FM backup files:";
        // 
        // BackupPathTextBox
        // 
        this.BackupPathTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
        | System.Windows.Forms.AnchorStyles.Right)));
        this.BackupPathTextBox.Location = new System.Drawing.Point(13, 24);
        this.BackupPathTextBox.Name = "BackupPathTextBox";
        this.BackupPathTextBox.Size = new System.Drawing.Size(321, 20);
        this.BackupPathTextBox.TabIndex = 1;
        // 
        // BackupPathBrowseButton
        // 
        this.BackupPathBrowseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
        this.BackupPathBrowseButton.Location = new System.Drawing.Point(333, 23);
        this.BackupPathBrowseButton.Name = "BackupPathBrowseButton";
        this.BackupPathBrowseButton.TabIndex = 2;
        this.BackupPathBrowseButton.Text = "Browse...";
        // 
        // FMArchivePathsGroupBox
        // 
        this.FMArchivePathsGroupBox.Controls.Add(this.IncludeSubfoldersCheckBox);
        this.FMArchivePathsGroupBox.Controls.Add(this.AddFMArchivePathButton);
        this.FMArchivePathsGroupBox.Controls.Add(this.RemoveFMArchivePathButton);
        this.FMArchivePathsGroupBox.Controls.Add(this.FMArchivePathsListBox);
        this.FMArchivePathsGroupBox.Location = new System.Drawing.Point(8, 132);
        this.FMArchivePathsGroupBox.Margin = new System.Windows.Forms.Padding(3, 9, 3, 3);
        this.FMArchivePathsGroupBox.MinimumSize = new System.Drawing.Size(424, 0);
        this.FMArchivePathsGroupBox.Name = "FMArchivePathsGroupBox";
        this.FMArchivePathsGroupBox.Size = new System.Drawing.Size(424, 258);
        this.FMArchivePathsGroupBox.TabIndex = 3;
        this.FMArchivePathsGroupBox.TabStop = false;
        this.FMArchivePathsGroupBox.Text = "FM archive paths";
        // 
        // IncludeSubfoldersCheckBox
        // 
        this.IncludeSubfoldersCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
        this.IncludeSubfoldersCheckBox.AutoSize = true;
        this.IncludeSubfoldersCheckBox.Location = new System.Drawing.Point(16, 224);
        this.IncludeSubfoldersCheckBox.Name = "IncludeSubfoldersCheckBox";
        this.IncludeSubfoldersCheckBox.Size = new System.Drawing.Size(112, 17);
        this.IncludeSubfoldersCheckBox.TabIndex = 1;
        this.IncludeSubfoldersCheckBox.Text = "Include subfolders";
        // 
        // AddFMArchivePathButton
        // 
        this.AddFMArchivePathButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
        this.AddFMArchivePathButton.Location = new System.Drawing.Point(386, 224);
        this.AddFMArchivePathButton.Name = "AddFMArchivePathButton";
        this.AddFMArchivePathButton.Size = new System.Drawing.Size(23, 23);
        this.AddFMArchivePathButton.TabIndex = 3;
        this.AddFMArchivePathButton.PaintCustom += new System.Windows.Forms.PaintEventHandler(this.AddFMArchivePathButton_Paint);
        // 
        // RemoveFMArchivePathButton
        // 
        this.RemoveFMArchivePathButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
        this.RemoveFMArchivePathButton.Location = new System.Drawing.Point(362, 224);
        this.RemoveFMArchivePathButton.Name = "RemoveFMArchivePathButton";
        this.RemoveFMArchivePathButton.Size = new System.Drawing.Size(23, 23);
        this.RemoveFMArchivePathButton.TabIndex = 2;
        this.RemoveFMArchivePathButton.PaintCustom += new System.Windows.Forms.PaintEventHandler(this.RemoveFMArchivePathButton_Paint);
        // 
        // FMArchivePathsListBox
        // 
        this.FMArchivePathsListBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
        | System.Windows.Forms.AnchorStyles.Right)));
        this.FMArchivePathsListBox.Location = new System.Drawing.Point(16, 24);
        this.FMArchivePathsListBox.MultiSelect = false;
        this.FMArchivePathsListBox.Name = "FMArchivePathsListBox";
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
        this.SteamOptionsGroupBox.Name = "SteamOptionsGroupBox";
        this.SteamOptionsGroupBox.Size = new System.Drawing.Size(424, 176);
        this.SteamOptionsGroupBox.TabIndex = 1;
        this.SteamOptionsGroupBox.TabStop = false;
        this.SteamOptionsGroupBox.Text = "Steam options";
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
        this.LaunchTheseGamesThroughSteamPanel.Name = "LaunchTheseGamesThroughSteamPanel";
        this.LaunchTheseGamesThroughSteamPanel.Size = new System.Drawing.Size(392, 96);
        this.LaunchTheseGamesThroughSteamPanel.TabIndex = 7;
        // 
        // LaunchTheseGamesThroughSteamCheckBox
        // 
        this.LaunchTheseGamesThroughSteamCheckBox.AutoSize = true;
        this.LaunchTheseGamesThroughSteamCheckBox.Checked = true;
        this.LaunchTheseGamesThroughSteamCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
        this.LaunchTheseGamesThroughSteamCheckBox.Location = new System.Drawing.Point(0, 0);
        this.LaunchTheseGamesThroughSteamCheckBox.Name = "LaunchTheseGamesThroughSteamCheckBox";
        this.LaunchTheseGamesThroughSteamCheckBox.Size = new System.Drawing.Size(238, 17);
        this.LaunchTheseGamesThroughSteamCheckBox.TabIndex = 0;
        this.LaunchTheseGamesThroughSteamCheckBox.Text = "If Steam exists, use it to launch these games:";
        // 
        // Thief1UseSteamCheckBox
        // 
        this.Thief1UseSteamCheckBox.AutoSize = true;
        this.Thief1UseSteamCheckBox.Checked = true;
        this.Thief1UseSteamCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
        this.Thief1UseSteamCheckBox.Location = new System.Drawing.Point(8, 24);
        this.Thief1UseSteamCheckBox.Name = "Thief1UseSteamCheckBox";
        this.Thief1UseSteamCheckBox.Size = new System.Drawing.Size(59, 17);
        this.Thief1UseSteamCheckBox.TabIndex = 1;
        this.Thief1UseSteamCheckBox.Text = "Thief 1";
        // 
        // SS2UseSteamCheckBox
        // 
        this.SS2UseSteamCheckBox.AutoSize = true;
        this.SS2UseSteamCheckBox.Checked = true;
        this.SS2UseSteamCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
        this.SS2UseSteamCheckBox.Location = new System.Drawing.Point(8, 72);
        this.SS2UseSteamCheckBox.Name = "SS2UseSteamCheckBox";
        this.SS2UseSteamCheckBox.Size = new System.Drawing.Size(103, 17);
        this.SS2UseSteamCheckBox.TabIndex = 4;
        this.SS2UseSteamCheckBox.Text = "System Shock 2";
        // 
        // Thief3UseSteamCheckBox
        // 
        this.Thief3UseSteamCheckBox.AutoSize = true;
        this.Thief3UseSteamCheckBox.Checked = true;
        this.Thief3UseSteamCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
        this.Thief3UseSteamCheckBox.Location = new System.Drawing.Point(8, 56);
        this.Thief3UseSteamCheckBox.Name = "Thief3UseSteamCheckBox";
        this.Thief3UseSteamCheckBox.Size = new System.Drawing.Size(59, 17);
        this.Thief3UseSteamCheckBox.TabIndex = 3;
        this.Thief3UseSteamCheckBox.Text = "Thief 3";
        // 
        // Thief2UseSteamCheckBox
        // 
        this.Thief2UseSteamCheckBox.AutoSize = true;
        this.Thief2UseSteamCheckBox.Checked = true;
        this.Thief2UseSteamCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
        this.Thief2UseSteamCheckBox.Location = new System.Drawing.Point(8, 40);
        this.Thief2UseSteamCheckBox.Name = "Thief2UseSteamCheckBox";
        this.Thief2UseSteamCheckBox.Size = new System.Drawing.Size(59, 17);
        this.Thief2UseSteamCheckBox.TabIndex = 2;
        this.Thief2UseSteamCheckBox.Text = "Thief 2";
        // 
        // SteamExeLabel
        // 
        this.SteamExeLabel.AutoSize = true;
        this.SteamExeLabel.Location = new System.Drawing.Point(16, 24);
        this.SteamExeLabel.Name = "SteamExeLabel";
        this.SteamExeLabel.Size = new System.Drawing.Size(178, 13);
        this.SteamExeLabel.TabIndex = 0;
        this.SteamExeLabel.Text = "Path to Steam executable (optional):";
        // 
        // SteamExeTextBox
        // 
        this.SteamExeTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
        | System.Windows.Forms.AnchorStyles.Right)));
        this.SteamExeTextBox.Location = new System.Drawing.Point(16, 40);
        this.SteamExeTextBox.Name = "SteamExeTextBox";
        this.SteamExeTextBox.Size = new System.Drawing.Size(320, 20);
        this.SteamExeTextBox.TabIndex = 1;
        // 
        // SteamExeBrowseButton
        // 
        this.SteamExeBrowseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
        this.SteamExeBrowseButton.Location = new System.Drawing.Point(336, 39);
        this.SteamExeBrowseButton.Name = "SteamExeBrowseButton";
        this.SteamExeBrowseButton.TabIndex = 2;
        this.SteamExeBrowseButton.Text = "Browse...";
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
        this.PathsToGameExesGroupBox.Name = "PathsToGameExesGroupBox";
        this.PathsToGameExesGroupBox.Size = new System.Drawing.Size(424, 232);
        this.PathsToGameExesGroupBox.TabIndex = 0;
        this.PathsToGameExesGroupBox.TabStop = false;
        this.PathsToGameExesGroupBox.Text = "Paths to game executables";
        // 
        // GameRequirementsPanel
        // 
        this.GameRequirementsPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
        | System.Windows.Forms.AnchorStyles.Right)));
        this.GameRequirementsPanel.AutoScroll = true;
        this.GameRequirementsPanel.Controls.Add(this.GameRequirementsLabel);
        this.GameRequirementsPanel.Location = new System.Drawing.Point(16, 192);
        this.GameRequirementsPanel.Name = "GameRequirementsPanel";
        this.GameRequirementsPanel.Size = new System.Drawing.Size(392, 32);
        this.GameRequirementsPanel.TabIndex = 12;
        // 
        // GameRequirementsLabel
        // 
        this.GameRequirementsLabel.AutoSize = true;
        this.GameRequirementsLabel.Location = new System.Drawing.Point(0, 0);
        this.GameRequirementsLabel.Name = "GameRequirementsLabel";
        this.GameRequirementsLabel.Size = new System.Drawing.Size(273, 26);
        this.GameRequirementsLabel.TabIndex = 0;
        this.GameRequirementsLabel.Text = "* Thief 1, Thief 2 and System Shock 2 require NewDark.\r\n* Thief 3 requires the Sn" +
"eaky Upgrade 1.1.9.1 or above.";
        this.GameRequirementsLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        // 
        // SS2ExePathLabel
        // 
        this.SS2ExePathLabel.AutoSize = true;
        this.SS2ExePathLabel.Location = new System.Drawing.Point(16, 144);
        this.SS2ExePathLabel.Name = "SS2ExePathLabel";
        this.SS2ExePathLabel.Size = new System.Drawing.Size(87, 13);
        this.SS2ExePathLabel.TabIndex = 9;
        this.SS2ExePathLabel.Text = "System Shock 2:";
        // 
        // Thief3ExePathLabel
        // 
        this.Thief3ExePathLabel.AutoSize = true;
        this.Thief3ExePathLabel.Location = new System.Drawing.Point(16, 104);
        this.Thief3ExePathLabel.Name = "Thief3ExePathLabel";
        this.Thief3ExePathLabel.Size = new System.Drawing.Size(43, 13);
        this.Thief3ExePathLabel.TabIndex = 6;
        this.Thief3ExePathLabel.Text = "Thief 3:";
        // 
        // Thief2ExePathLabel
        // 
        this.Thief2ExePathLabel.AutoSize = true;
        this.Thief2ExePathLabel.Location = new System.Drawing.Point(16, 64);
        this.Thief2ExePathLabel.Name = "Thief2ExePathLabel";
        this.Thief2ExePathLabel.Size = new System.Drawing.Size(43, 13);
        this.Thief2ExePathLabel.TabIndex = 3;
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
        // SS2ExePathBrowseButton
        // 
        this.SS2ExePathBrowseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
        this.SS2ExePathBrowseButton.Location = new System.Drawing.Point(336, 159);
        this.SS2ExePathBrowseButton.Name = "SS2ExePathBrowseButton";
        this.SS2ExePathBrowseButton.TabIndex = 11;
        this.SS2ExePathBrowseButton.Text = "Browse...";
        // 
        // Thief3ExePathBrowseButton
        // 
        this.Thief3ExePathBrowseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
        this.Thief3ExePathBrowseButton.Location = new System.Drawing.Point(336, 119);
        this.Thief3ExePathBrowseButton.Name = "Thief3ExePathBrowseButton";
        this.Thief3ExePathBrowseButton.TabIndex = 8;
        this.Thief3ExePathBrowseButton.Text = "Browse...";
        // 
        // Thief2ExePathBrowseButton
        // 
        this.Thief2ExePathBrowseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
        this.Thief2ExePathBrowseButton.Location = new System.Drawing.Point(336, 79);
        this.Thief2ExePathBrowseButton.Name = "Thief2ExePathBrowseButton";
        this.Thief2ExePathBrowseButton.TabIndex = 5;
        this.Thief2ExePathBrowseButton.Text = "Browse...";
        // 
        // Thief1ExePathBrowseButton
        // 
        this.Thief1ExePathBrowseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
        this.Thief1ExePathBrowseButton.Location = new System.Drawing.Point(336, 39);
        this.Thief1ExePathBrowseButton.Name = "Thief1ExePathBrowseButton";
        this.Thief1ExePathBrowseButton.TabIndex = 2;
        this.Thief1ExePathBrowseButton.Text = "Browse...";
        // 
        // SS2ExePathTextBox
        // 
        this.SS2ExePathTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
        | System.Windows.Forms.AnchorStyles.Right)));
        this.SS2ExePathTextBox.Location = new System.Drawing.Point(16, 160);
        this.SS2ExePathTextBox.Name = "SS2ExePathTextBox";
        this.SS2ExePathTextBox.Size = new System.Drawing.Size(320, 20);
        this.SS2ExePathTextBox.TabIndex = 10;
        // 
        // Thief3ExePathTextBox
        // 
        this.Thief3ExePathTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
        | System.Windows.Forms.AnchorStyles.Right)));
        this.Thief3ExePathTextBox.Location = new System.Drawing.Point(16, 120);
        this.Thief3ExePathTextBox.Name = "Thief3ExePathTextBox";
        this.Thief3ExePathTextBox.Size = new System.Drawing.Size(320, 20);
        this.Thief3ExePathTextBox.TabIndex = 7;
        // 
        // Thief2ExePathTextBox
        // 
        this.Thief2ExePathTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
        | System.Windows.Forms.AnchorStyles.Right)));
        this.Thief2ExePathTextBox.Location = new System.Drawing.Point(16, 80);
        this.Thief2ExePathTextBox.Name = "Thief2ExePathTextBox";
        this.Thief2ExePathTextBox.Size = new System.Drawing.Size(320, 20);
        this.Thief2ExePathTextBox.TabIndex = 4;
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
#endif

    #endregion

    internal System.Windows.Forms.Panel PagePanel;
    internal AngelLoader.Forms.CustomControls.DarkGroupBox OtherGroupBox;
    internal AngelLoader.Forms.CustomControls.DarkLabel BackupPathLabel;
    internal AngelLoader.Forms.CustomControls.DarkButton BackupPathBrowseButton;
    internal AngelLoader.Forms.CustomControls.DarkTextBox BackupPathTextBox;
    internal AngelLoader.Forms.CustomControls.DarkGroupBox PathsToGameExesGroupBox;
    internal System.Windows.Forms.Panel GameRequirementsPanel;
    internal AngelLoader.Forms.CustomControls.DarkLabel GameRequirementsLabel;
    internal AngelLoader.Forms.CustomControls.DarkLabel Thief3ExePathLabel;
    internal AngelLoader.Forms.CustomControls.DarkLabel Thief2ExePathLabel;
    internal AngelLoader.Forms.CustomControls.DarkLabel Thief1ExePathLabel;
    internal AngelLoader.Forms.CustomControls.StandardButton Thief3ExePathBrowseButton;
    internal AngelLoader.Forms.CustomControls.StandardButton Thief2ExePathBrowseButton;
    internal AngelLoader.Forms.CustomControls.StandardButton Thief1ExePathBrowseButton;
    internal AngelLoader.Forms.CustomControls.DarkTextBox Thief3ExePathTextBox;
    internal AngelLoader.Forms.CustomControls.DarkTextBox Thief2ExePathTextBox;
    internal AngelLoader.Forms.CustomControls.DarkTextBox Thief1ExePathTextBox;
    internal AngelLoader.Forms.CustomControls.DarkGroupBox FMArchivePathsGroupBox;
    internal AngelLoader.Forms.CustomControls.DarkCheckBox IncludeSubfoldersCheckBox;
    internal AngelLoader.Forms.CustomControls.DarkButton AddFMArchivePathButton;
    internal AngelLoader.Forms.CustomControls.DarkButton RemoveFMArchivePathButton;
    internal AngelLoader.Forms.CustomControls.DarkListBox FMArchivePathsListBox;
    internal System.Windows.Forms.Panel ActualPathsPanel;
    internal System.Windows.Forms.Control DummyAutoScrollPanel;
    internal AngelLoader.Forms.CustomControls.DarkLabel SteamExeLabel;
    internal AngelLoader.Forms.CustomControls.DarkTextBox SteamExeTextBox;
    internal AngelLoader.Forms.CustomControls.StandardButton SteamExeBrowseButton;
    internal AngelLoader.Forms.CustomControls.DarkGroupBox SteamOptionsGroupBox;
    internal AngelLoader.Forms.CustomControls.DarkCheckBox Thief3UseSteamCheckBox;
    internal AngelLoader.Forms.CustomControls.DarkCheckBox Thief2UseSteamCheckBox;
    internal AngelLoader.Forms.CustomControls.DarkCheckBox Thief1UseSteamCheckBox;
    internal System.Windows.Forms.Panel LaunchTheseGamesThroughSteamPanel;
    internal AngelLoader.Forms.CustomControls.DarkCheckBox LaunchTheseGamesThroughSteamCheckBox;
    internal AngelLoader.Forms.CustomControls.DarkLabel SS2ExePathLabel;
    internal AngelLoader.Forms.CustomControls.StandardButton SS2ExePathBrowseButton;
    internal AngelLoader.Forms.CustomControls.DarkTextBox SS2ExePathTextBox;
    internal AngelLoader.Forms.CustomControls.DarkCheckBox SS2UseSteamCheckBox;
    internal System.Windows.Forms.Panel BackupPathPanel;
    internal System.Windows.Forms.FlowLayoutPanel LayoutFLP;
    internal AngelLoader.Forms.CustomControls.DarkLabel BackupPathHelpLabel;
    private System.Windows.Forms.PictureBox BackupHelpPictureBox;
}
