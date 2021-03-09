using System.Drawing;
using System.Windows.Forms;
using AngelLoader.Forms.CustomControls;

namespace AngelLoader.Forms
{
    public sealed partial class PathsPage
    {
        private void InitComponentManual()
        {
            PagePanel = new DarkPanel();
            ActualPathsPanel = new Panel();
            FlowLayoutPanel1 = new FlowLayoutPanel();
            OtherGroupBox = new DarkGroupBox();
            BackupPathPanel = new Panel();
            BackupHelpPictureBox = new PictureBox();
            BackupPathHelpLabel = new DarkLabel();
            BackupPathLabel = new DarkLabel();
            BackupPathTextBox = new DarkTextBox();
            BackupPathBrowseButton = new DarkButton();
            FMArchivePathsGroupBox = new DarkGroupBox();
            IncludeSubfoldersCheckBox = new DarkCheckBox();
            AddFMArchivePathButton = new DarkButton();
            RemoveFMArchivePathButton = new DarkButton();
            FMArchivePathsListBox = new DarkListBox();
            SteamOptionsGroupBox = new DarkGroupBox();
            LaunchTheseGamesThroughSteamPanel = new Panel();
            LaunchTheseGamesThroughSteamCheckBox = new DarkCheckBox();
            Thief1UseSteamCheckBox = new DarkCheckBox();
            SS2UseSteamCheckBox = new DarkCheckBox();
            Thief3UseSteamCheckBox = new DarkCheckBox();
            Thief2UseSteamCheckBox = new DarkCheckBox();
            SteamExeLabel = new DarkLabel();
            SteamExeTextBox = new DarkTextBox();
            SteamExeBrowseButton = new DarkButton();
            PathsToGameExesGroupBox = new DarkGroupBox();
            GameRequirementsPanel = new Panel();
            GameRequirementsLabel = new DarkLabel();
            SS2ExePathLabel = new DarkLabel();
            Thief3ExePathLabel = new DarkLabel();
            Thief2ExePathLabel = new DarkLabel();
            Thief1ExePathLabel = new DarkLabel();
            SS2ExePathBrowseButton = new DarkButton();
            Thief3ExePathBrowseButton = new DarkButton();
            Thief2ExePathBrowseButton = new DarkButton();
            Thief1ExePathBrowseButton = new DarkButton();
            SS2ExePathTextBox = new DarkTextBox();
            Thief3ExePathTextBox = new DarkTextBox();
            Thief2ExePathTextBox = new DarkTextBox();
            Thief1ExePathTextBox = new DarkTextBox();
            DummyAutoScrollPanel = new Control();
            PagePanel.SuspendLayout();
            ActualPathsPanel.SuspendLayout();
            FlowLayoutPanel1.SuspendLayout();
            OtherGroupBox.SuspendLayout();
            BackupPathPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)BackupHelpPictureBox).BeginInit();
            FMArchivePathsGroupBox.SuspendLayout();
            SteamOptionsGroupBox.SuspendLayout();
            LaunchTheseGamesThroughSteamPanel.SuspendLayout();
            PathsToGameExesGroupBox.SuspendLayout();
            GameRequirementsPanel.SuspendLayout();
            SuspendLayout();
            // 
            // PagePanel
            // 
            PagePanel.AutoScroll = true;
            PagePanel.Controls.Add(ActualPathsPanel);
            PagePanel.Controls.Add(DummyAutoScrollPanel);
            PagePanel.Dock = DockStyle.Fill;
            PagePanel.Location = new Point(0, 0);
            PagePanel.Size = new Size(440, 910);
            PagePanel.TabIndex = 3;
            // 
            // ActualPathsPanel
            // 
            ActualPathsPanel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            ActualPathsPanel.Controls.Add(FlowLayoutPanel1);
            ActualPathsPanel.Controls.Add(SteamOptionsGroupBox);
            ActualPathsPanel.Controls.Add(PathsToGameExesGroupBox);
            ActualPathsPanel.Location = new Point(0, 0);
            ActualPathsPanel.MinimumSize = new Size(440, 0);
            ActualPathsPanel.Size = new Size(440, 824);
            ActualPathsPanel.TabIndex = 4;
            // 
            // FlowLayoutPanel1
            // 
            FlowLayoutPanel1.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            FlowLayoutPanel1.Controls.Add(OtherGroupBox);
            FlowLayoutPanel1.Controls.Add(FMArchivePathsGroupBox);
            FlowLayoutPanel1.FlowDirection = FlowDirection.TopDown;
            FlowLayoutPanel1.Location = new Point(0, 432);
            FlowLayoutPanel1.Margin = new Padding(0, 3, 0, 3);
            FlowLayoutPanel1.Padding = new Padding(5, 0, 0, 0);
            FlowLayoutPanel1.Size = new Size(440, 392);
            FlowLayoutPanel1.TabIndex = 4;
            FlowLayoutPanel1.WrapContents = false;
            FlowLayoutPanel1.Layout += FlowLayoutPanel1_Layout;
            // 
            // OtherGroupBox
            // 
            OtherGroupBox.Controls.Add(BackupPathPanel);
            OtherGroupBox.Location = new Point(8, 3);
            OtherGroupBox.MinimumSize = new Size(424, 0);
            OtherGroupBox.Size = new Size(424, 117);
            OtherGroupBox.TabIndex = 2;
            OtherGroupBox.TabStop = false;
            // 
            // BackupPathPanel
            // 
            BackupPathPanel.AutoScroll = true;
            BackupPathPanel.Controls.Add(BackupHelpPictureBox);
            BackupPathPanel.Controls.Add(BackupPathHelpLabel);
            BackupPathPanel.Controls.Add(BackupPathLabel);
            BackupPathPanel.Controls.Add(BackupPathTextBox);
            BackupPathPanel.Controls.Add(BackupPathBrowseButton);
            BackupPathPanel.Dock = DockStyle.Fill;
            BackupPathPanel.Location = new Point(3, 16);
            BackupPathPanel.Margin = new Padding(0);
            BackupPathPanel.Size = new Size(418, 98);
            BackupPathPanel.TabIndex = 4;
            // 
            // BackupHelpPictureBox
            // 
            BackupHelpPictureBox.Image = Properties.Resources.Help_16;
            BackupHelpPictureBox.Location = new Point(13, 56);
            BackupHelpPictureBox.Size = new Size(16, 16);
            BackupHelpPictureBox.SizeMode = PictureBoxSizeMode.AutoSize;
            BackupHelpPictureBox.TabIndex = 7;
            BackupHelpPictureBox.TabStop = false;
            // 
            // BackupPathHelpLabel
            // 
            BackupPathHelpLabel.AutoSize = true;
            BackupPathHelpLabel.Location = new Point(32, 56);
            BackupPathHelpLabel.MaximumSize = new Size(380, 0);
            BackupPathHelpLabel.TabIndex = 6;
            BackupPathHelpLabel.TextChanged += BackupPathHelpLabel_TextChanged;
            // 
            // BackupPathLabel
            // 
            BackupPathLabel.AutoSize = true;
            BackupPathLabel.Location = new Point(13, 8);
            BackupPathLabel.TabIndex = 0;
            // 
            // BackupPathTextBox
            // 
            BackupPathTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            BackupPathTextBox.Location = new Point(13, 24);
            BackupPathTextBox.Size = new Size(321, 20);
            BackupPathTextBox.TabIndex = 1;
            // 
            // BackupPathBrowseButton
            // 
            BackupPathBrowseButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            BackupPathBrowseButton.AutoSize = true;
            BackupPathBrowseButton.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            BackupPathBrowseButton.MinimumSize = new Size(75, 23);
            BackupPathBrowseButton.Location = new Point(333, 23);
            BackupPathBrowseButton.Padding = new Padding(6, 0, 6, 0);
            BackupPathBrowseButton.TabIndex = 2;
            BackupPathBrowseButton.UseVisualStyleBackColor = true;
            // 
            // FMArchivePathsGroupBox
            // 
            FMArchivePathsGroupBox.Controls.Add(IncludeSubfoldersCheckBox);
            FMArchivePathsGroupBox.Controls.Add(AddFMArchivePathButton);
            FMArchivePathsGroupBox.Controls.Add(RemoveFMArchivePathButton);
            FMArchivePathsGroupBox.Controls.Add(FMArchivePathsListBox);
            FMArchivePathsGroupBox.Location = new Point(8, 126);
            FMArchivePathsGroupBox.MinimumSize = new Size(424, 0);
            FMArchivePathsGroupBox.Size = new Size(424, 258);
            FMArchivePathsGroupBox.TabIndex = 3;
            FMArchivePathsGroupBox.TabStop = false;
            // 
            // IncludeSubfoldersCheckBox
            // 
            IncludeSubfoldersCheckBox.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            IncludeSubfoldersCheckBox.AutoSize = true;
            IncludeSubfoldersCheckBox.Location = new Point(16, 224);
            IncludeSubfoldersCheckBox.TabIndex = 1;
            IncludeSubfoldersCheckBox.UseVisualStyleBackColor = true;
            // 
            // AddFMArchivePathButton
            // 
            AddFMArchivePathButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            AddFMArchivePathButton.Location = new Point(386, 224);
            AddFMArchivePathButton.Size = new Size(23, 23);
            AddFMArchivePathButton.TabIndex = 3;
            AddFMArchivePathButton.UseVisualStyleBackColor = true;
            AddFMArchivePathButton.PaintCustom += AddFMArchivePathButton_Paint;
            // 
            // RemoveFMArchivePathButton
            // 
            RemoveFMArchivePathButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            RemoveFMArchivePathButton.Location = new Point(362, 224);
            RemoveFMArchivePathButton.Size = new Size(23, 23);
            RemoveFMArchivePathButton.TabIndex = 2;
            RemoveFMArchivePathButton.UseVisualStyleBackColor = true;
            RemoveFMArchivePathButton.PaintCustom += RemoveFMArchivePathButton_Paint;
            // 
            // FMArchivePathsListBox
            // 
            FMArchivePathsListBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            FMArchivePathsListBox.Location = new Point(16, 24);
            FMArchivePathsListBox.MultiSelect = false;
            FMArchivePathsListBox.Scrollable = true;
            FMArchivePathsListBox.Size = new Size(392, 199);
            FMArchivePathsListBox.TabIndex = 0;
            // 
            // SteamOptionsGroupBox
            // 
            SteamOptionsGroupBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            SteamOptionsGroupBox.Controls.Add(LaunchTheseGamesThroughSteamPanel);
            SteamOptionsGroupBox.Controls.Add(SteamExeLabel);
            SteamOptionsGroupBox.Controls.Add(SteamExeTextBox);
            SteamOptionsGroupBox.Controls.Add(SteamExeBrowseButton);
            SteamOptionsGroupBox.Location = new Point(8, 248);
            SteamOptionsGroupBox.Size = new Size(424, 176);
            SteamOptionsGroupBox.TabIndex = 1;
            SteamOptionsGroupBox.TabStop = false;
            // 
            // LaunchTheseGamesThroughSteamPanel
            // 
            LaunchTheseGamesThroughSteamPanel.Controls.Add(LaunchTheseGamesThroughSteamCheckBox);
            LaunchTheseGamesThroughSteamPanel.Controls.Add(Thief1UseSteamCheckBox);
            LaunchTheseGamesThroughSteamPanel.Controls.Add(SS2UseSteamCheckBox);
            LaunchTheseGamesThroughSteamPanel.Controls.Add(Thief3UseSteamCheckBox);
            LaunchTheseGamesThroughSteamPanel.Controls.Add(Thief2UseSteamCheckBox);
            LaunchTheseGamesThroughSteamPanel.Enabled = false;
            LaunchTheseGamesThroughSteamPanel.Location = new Point(16, 72);
            LaunchTheseGamesThroughSteamPanel.Size = new Size(392, 96);
            LaunchTheseGamesThroughSteamPanel.TabIndex = 7;
            // 
            // LaunchTheseGamesThroughSteamCheckBox
            // 
            LaunchTheseGamesThroughSteamCheckBox.AutoSize = true;
            LaunchTheseGamesThroughSteamCheckBox.Checked = true;
            LaunchTheseGamesThroughSteamCheckBox.Location = new Point(0, 0);
            LaunchTheseGamesThroughSteamCheckBox.TabIndex = 0;
            LaunchTheseGamesThroughSteamCheckBox.UseVisualStyleBackColor = true;
            // 
            // Thief1UseSteamCheckBox
            // 
            Thief1UseSteamCheckBox.AutoSize = true;
            Thief1UseSteamCheckBox.Checked = true;
            Thief1UseSteamCheckBox.Location = new Point(8, 24);
            Thief1UseSteamCheckBox.TabIndex = 1;
            Thief1UseSteamCheckBox.UseVisualStyleBackColor = true;
            // 
            // SS2UseSteamCheckBox
            // 
            SS2UseSteamCheckBox.AutoSize = true;
            SS2UseSteamCheckBox.Checked = true;
            SS2UseSteamCheckBox.Location = new Point(8, 72);
            SS2UseSteamCheckBox.TabIndex = 4;
            SS2UseSteamCheckBox.UseVisualStyleBackColor = true;
            // 
            // Thief3UseSteamCheckBox
            // 
            Thief3UseSteamCheckBox.AutoSize = true;
            Thief3UseSteamCheckBox.Checked = true;
            Thief3UseSteamCheckBox.Location = new Point(8, 56);
            Thief3UseSteamCheckBox.TabIndex = 3;
            Thief3UseSteamCheckBox.UseVisualStyleBackColor = true;
            // 
            // Thief2UseSteamCheckBox
            // 
            Thief2UseSteamCheckBox.AutoSize = true;
            Thief2UseSteamCheckBox.Checked = true;
            Thief2UseSteamCheckBox.Location = new Point(8, 40);
            Thief2UseSteamCheckBox.TabIndex = 2;
            Thief2UseSteamCheckBox.UseVisualStyleBackColor = true;
            // 
            // SteamExeLabel
            // 
            SteamExeLabel.AutoSize = true;
            SteamExeLabel.Location = new Point(16, 24);
            SteamExeLabel.TabIndex = 0;
            // 
            // SteamExeTextBox
            // 
            SteamExeTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            SteamExeTextBox.Location = new Point(16, 40);
            SteamExeTextBox.Size = new Size(320, 20);
            SteamExeTextBox.TabIndex = 1;
            // 
            // SteamExeBrowseButton
            // 
            SteamExeBrowseButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            SteamExeBrowseButton.AutoSize = true;
            SteamExeBrowseButton.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            SteamExeBrowseButton.MinimumSize = new Size(75, 23);
            SteamExeBrowseButton.Location = new Point(336, 39);
            SteamExeBrowseButton.Padding = new Padding(6, 0, 6, 0);
            SteamExeBrowseButton.TabIndex = 2;
            SteamExeBrowseButton.UseVisualStyleBackColor = true;
            // 
            // PathsToGameExesGroupBox
            // 
            PathsToGameExesGroupBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            PathsToGameExesGroupBox.Controls.Add(GameRequirementsPanel);
            PathsToGameExesGroupBox.Controls.Add(SS2ExePathLabel);
            PathsToGameExesGroupBox.Controls.Add(Thief3ExePathLabel);
            PathsToGameExesGroupBox.Controls.Add(Thief2ExePathLabel);
            PathsToGameExesGroupBox.Controls.Add(Thief1ExePathLabel);
            PathsToGameExesGroupBox.Controls.Add(SS2ExePathBrowseButton);
            PathsToGameExesGroupBox.Controls.Add(Thief3ExePathBrowseButton);
            PathsToGameExesGroupBox.Controls.Add(Thief2ExePathBrowseButton);
            PathsToGameExesGroupBox.Controls.Add(Thief1ExePathBrowseButton);
            PathsToGameExesGroupBox.Controls.Add(SS2ExePathTextBox);
            PathsToGameExesGroupBox.Controls.Add(Thief3ExePathTextBox);
            PathsToGameExesGroupBox.Controls.Add(Thief2ExePathTextBox);
            PathsToGameExesGroupBox.Controls.Add(Thief1ExePathTextBox);
            PathsToGameExesGroupBox.Location = new Point(8, 8);
            PathsToGameExesGroupBox.MinimumSize = new Size(424, 0);
            PathsToGameExesGroupBox.Size = new Size(424, 232);
            PathsToGameExesGroupBox.TabIndex = 0;
            PathsToGameExesGroupBox.TabStop = false;
            // 
            // GameRequirementsPanel
            // 
            GameRequirementsPanel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            GameRequirementsPanel.AutoScroll = true;
            GameRequirementsPanel.Controls.Add(GameRequirementsLabel);
            GameRequirementsPanel.Location = new Point(16, 192);
            GameRequirementsPanel.Size = new Size(392, 32);
            GameRequirementsPanel.TabIndex = 12;
            // 
            // GameRequirementsLabel
            // 
            GameRequirementsLabel.AutoSize = true;
            GameRequirementsLabel.Location = new Point(0, 0);
            GameRequirementsLabel.TabIndex = 0;
            GameRequirementsLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // SS2ExePathLabel
            // 
            SS2ExePathLabel.AutoSize = true;
            SS2ExePathLabel.Location = new Point(16, 144);
            SS2ExePathLabel.TabIndex = 9;
            // 
            // Thief3ExePathLabel
            // 
            Thief3ExePathLabel.AutoSize = true;
            Thief3ExePathLabel.Location = new Point(16, 104);
            Thief3ExePathLabel.TabIndex = 6;
            // 
            // Thief2ExePathLabel
            // 
            Thief2ExePathLabel.AutoSize = true;
            Thief2ExePathLabel.Location = new Point(16, 64);
            Thief2ExePathLabel.TabIndex = 3;
            // 
            // Thief1ExePathLabel
            // 
            Thief1ExePathLabel.AutoSize = true;
            Thief1ExePathLabel.Location = new Point(16, 24);
            Thief1ExePathLabel.TabIndex = 0;
            // 
            // SS2ExePathBrowseButton
            // 
            SS2ExePathBrowseButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            SS2ExePathBrowseButton.AutoSize = true;
            SS2ExePathBrowseButton.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            SS2ExePathBrowseButton.MinimumSize = new Size(75, 23);
            SS2ExePathBrowseButton.Location = new Point(336, 159);
            SS2ExePathBrowseButton.Padding = new Padding(6, 0, 6, 0);
            SS2ExePathBrowseButton.TabIndex = 11;
            SS2ExePathBrowseButton.UseVisualStyleBackColor = true;
            // 
            // Thief3ExePathBrowseButton
            // 
            Thief3ExePathBrowseButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            Thief3ExePathBrowseButton.AutoSize = true;
            Thief3ExePathBrowseButton.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            Thief3ExePathBrowseButton.MinimumSize = new Size(75, 23);
            Thief3ExePathBrowseButton.Location = new Point(336, 119);
            Thief3ExePathBrowseButton.Padding = new Padding(6, 0, 6, 0);
            Thief3ExePathBrowseButton.TabIndex = 8;
            Thief3ExePathBrowseButton.UseVisualStyleBackColor = true;
            // 
            // Thief2ExePathBrowseButton
            // 
            Thief2ExePathBrowseButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            Thief2ExePathBrowseButton.AutoSize = true;
            Thief2ExePathBrowseButton.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            Thief2ExePathBrowseButton.MinimumSize = new Size(75, 23);
            Thief2ExePathBrowseButton.Location = new Point(336, 79);
            Thief2ExePathBrowseButton.Padding = new Padding(6, 0, 6, 0);
            Thief2ExePathBrowseButton.TabIndex = 5;
            Thief2ExePathBrowseButton.UseVisualStyleBackColor = true;
            // 
            // Thief1ExePathBrowseButton
            // 
            Thief1ExePathBrowseButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            Thief1ExePathBrowseButton.AutoSize = true;
            Thief1ExePathBrowseButton.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            Thief1ExePathBrowseButton.MinimumSize = new Size(75, 23);
            Thief1ExePathBrowseButton.Location = new Point(336, 39);
            Thief1ExePathBrowseButton.Padding = new Padding(6, 0, 6, 0);
            Thief1ExePathBrowseButton.TabIndex = 2;
            Thief1ExePathBrowseButton.UseVisualStyleBackColor = true;
            // 
            // SS2ExePathTextBox
            // 
            SS2ExePathTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            SS2ExePathTextBox.Location = new Point(16, 160);
            SS2ExePathTextBox.Size = new Size(320, 20);
            SS2ExePathTextBox.TabIndex = 10;
            // 
            // Thief3ExePathTextBox
            // 
            Thief3ExePathTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            Thief3ExePathTextBox.Location = new Point(16, 120);
            Thief3ExePathTextBox.Size = new Size(320, 20);
            Thief3ExePathTextBox.TabIndex = 7;
            // 
            // Thief2ExePathTextBox
            // 
            Thief2ExePathTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            Thief2ExePathTextBox.Location = new Point(16, 80);
            Thief2ExePathTextBox.Size = new Size(320, 20);
            Thief2ExePathTextBox.TabIndex = 4;
            // 
            // Thief1ExePathTextBox
            // 
            Thief1ExePathTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            Thief1ExePathTextBox.Location = new Point(16, 40);
            Thief1ExePathTextBox.Size = new Size(320, 20);
            Thief1ExePathTextBox.TabIndex = 1;
            // 
            // DummyAutoScrollPanel
            // 
            DummyAutoScrollPanel.Location = new Point(8, 200);
            DummyAutoScrollPanel.Size = new Size(424, 8);
            DummyAutoScrollPanel.TabIndex = 13;
            // 
            // PathsPage
            // 
            AutoScaleDimensions = new SizeF(6F, 13F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(PagePanel);
            Size = new Size(440, 910);
            PagePanel.ResumeLayout(false);
            ActualPathsPanel.ResumeLayout(false);
            FlowLayoutPanel1.ResumeLayout(false);
            OtherGroupBox.ResumeLayout(false);
            BackupPathPanel.ResumeLayout(false);
            BackupPathPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)BackupHelpPictureBox).EndInit();
            FMArchivePathsGroupBox.ResumeLayout(false);
            FMArchivePathsGroupBox.PerformLayout();
            SteamOptionsGroupBox.ResumeLayout(false);
            SteamOptionsGroupBox.PerformLayout();
            LaunchTheseGamesThroughSteamPanel.ResumeLayout(false);
            LaunchTheseGamesThroughSteamPanel.PerformLayout();
            PathsToGameExesGroupBox.ResumeLayout(false);
            PathsToGameExesGroupBox.PerformLayout();
            GameRequirementsPanel.ResumeLayout(false);
            GameRequirementsPanel.PerformLayout();
            ResumeLayout(false);
        }
    }
}
