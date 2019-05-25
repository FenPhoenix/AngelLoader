namespace AngelLoader.Forms
{
    partial class SettingsForm2
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
            this.SectionsListBox = new System.Windows.Forms.ListBox();
            this.PathsPanel = new System.Windows.Forms.Panel();
            this.PathsToGameExesGroupBox = new System.Windows.Forms.GroupBox();
            this.T3NeedsSULabel = new System.Windows.Forms.Label();
            this.T1T2NeedNewDarkLabel = new System.Windows.Forms.Label();
            this.Thief3ExePathLabel = new System.Windows.Forms.Label();
            this.Thief2ExePathLabel = new System.Windows.Forms.Label();
            this.Thief1ExePathLabel = new System.Windows.Forms.Label();
            this.Thief3ExePathBrowseButton = new System.Windows.Forms.Button();
            this.Thief2ExePathBrowseButton = new System.Windows.Forms.Button();
            this.Thief1ExePathBrowseButton = new System.Windows.Forms.Button();
            this.Thief3ExePathTextBox = new System.Windows.Forms.TextBox();
            this.Thief2ExePathTextBox = new System.Windows.Forms.TextBox();
            this.Thief1ExePathTextBox = new System.Windows.Forms.TextBox();
            this.OtherGroupBox = new System.Windows.Forms.GroupBox();
            this.BackupPathLabel = new System.Windows.Forms.Label();
            this.BackupPathBrowseButton = new System.Windows.Forms.Button();
            this.BackupPathTextBox = new System.Windows.Forms.TextBox();
            this.PathsPanel.SuspendLayout();
            this.PathsToGameExesGroupBox.SuspendLayout();
            this.OtherGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // SectionsListBox
            // 
            this.SectionsListBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.SectionsListBox.FormattingEnabled = true;
            this.SectionsListBox.IntegralHeight = false;
            this.SectionsListBox.Items.AddRange(new object[] {
            "Paths",
            "FM Display",
            "Other"});
            this.SectionsListBox.Location = new System.Drawing.Point(0, 0);
            this.SectionsListBox.Name = "SectionsListBox";
            this.SectionsListBox.Size = new System.Drawing.Size(232, 550);
            this.SectionsListBox.TabIndex = 1;
            // 
            // PathsPanel
            // 
            this.PathsPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.PathsPanel.AutoScroll = true;
            this.PathsPanel.Controls.Add(this.OtherGroupBox);
            this.PathsPanel.Controls.Add(this.PathsToGameExesGroupBox);
            this.PathsPanel.Location = new System.Drawing.Point(232, 0);
            this.PathsPanel.Name = "PathsPanel";
            this.PathsPanel.Size = new System.Drawing.Size(480, 550);
            this.PathsPanel.TabIndex = 2;
            // 
            // PathsToGameExesGroupBox
            // 
            this.PathsToGameExesGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.PathsToGameExesGroupBox.Controls.Add(this.T3NeedsSULabel);
            this.PathsToGameExesGroupBox.Controls.Add(this.T1T2NeedNewDarkLabel);
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
            this.PathsToGameExesGroupBox.Name = "PathsToGameExesGroupBox";
            this.PathsToGameExesGroupBox.Size = new System.Drawing.Size(464, 216);
            this.PathsToGameExesGroupBox.TabIndex = 1;
            this.PathsToGameExesGroupBox.TabStop = false;
            this.PathsToGameExesGroupBox.Text = "Paths to game executables";
            // 
            // T3NeedsSULabel
            // 
            this.T3NeedsSULabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.T3NeedsSULabel.Location = new System.Drawing.Point(16, 176);
            this.T3NeedsSULabel.Name = "T3NeedsSULabel";
            this.T3NeedsSULabel.Size = new System.Drawing.Size(440, 32);
            this.T3NeedsSULabel.TabIndex = 2;
            this.T3NeedsSULabel.Text = "* Thief 3 requires the Sneaky Upgrade.";
            this.T3NeedsSULabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // T1T2NeedNewDarkLabel
            // 
            this.T1T2NeedNewDarkLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.T1T2NeedNewDarkLabel.Location = new System.Drawing.Point(16, 144);
            this.T1T2NeedNewDarkLabel.Name = "T1T2NeedNewDarkLabel";
            this.T1T2NeedNewDarkLabel.Size = new System.Drawing.Size(440, 32);
            this.T1T2NeedNewDarkLabel.TabIndex = 1;
            this.T1T2NeedNewDarkLabel.Text = "* Thief 1 and Thief 2 require NewDark.";
            this.T1T2NeedNewDarkLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
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
            this.Thief3ExePathBrowseButton.Location = new System.Drawing.Point(376, 119);
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
            this.Thief2ExePathBrowseButton.Location = new System.Drawing.Point(376, 79);
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
            this.Thief1ExePathBrowseButton.Location = new System.Drawing.Point(376, 39);
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
            this.Thief3ExePathTextBox.Size = new System.Drawing.Size(360, 20);
            this.Thief3ExePathTextBox.TabIndex = 5;
            // 
            // Thief2ExePathTextBox
            // 
            this.Thief2ExePathTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.Thief2ExePathTextBox.Location = new System.Drawing.Point(16, 80);
            this.Thief2ExePathTextBox.Name = "Thief2ExePathTextBox";
            this.Thief2ExePathTextBox.Size = new System.Drawing.Size(360, 20);
            this.Thief2ExePathTextBox.TabIndex = 3;
            // 
            // Thief1ExePathTextBox
            // 
            this.Thief1ExePathTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.Thief1ExePathTextBox.Location = new System.Drawing.Point(16, 40);
            this.Thief1ExePathTextBox.Name = "Thief1ExePathTextBox";
            this.Thief1ExePathTextBox.Size = new System.Drawing.Size(360, 20);
            this.Thief1ExePathTextBox.TabIndex = 1;
            // 
            // OtherGroupBox
            // 
            this.OtherGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.OtherGroupBox.Controls.Add(this.BackupPathLabel);
            this.OtherGroupBox.Controls.Add(this.BackupPathBrowseButton);
            this.OtherGroupBox.Controls.Add(this.BackupPathTextBox);
            this.OtherGroupBox.Location = new System.Drawing.Point(8, 232);
            this.OtherGroupBox.Name = "OtherGroupBox";
            this.OtherGroupBox.Size = new System.Drawing.Size(464, 72);
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
            this.BackupPathBrowseButton.Location = new System.Drawing.Point(376, 39);
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
            this.BackupPathTextBox.Size = new System.Drawing.Size(360, 20);
            this.BackupPathTextBox.TabIndex = 1;
            // 
            // SettingsForm2
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(712, 550);
            this.Controls.Add(this.PathsPanel);
            this.Controls.Add(this.SectionsListBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(540, 320);
            this.Name = "SettingsForm2";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "SettingsForm2";
            this.PathsPanel.ResumeLayout(false);
            this.PathsToGameExesGroupBox.ResumeLayout(false);
            this.PathsToGameExesGroupBox.PerformLayout();
            this.OtherGroupBox.ResumeLayout(false);
            this.OtherGroupBox.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListBox SectionsListBox;
        private System.Windows.Forms.Panel PathsPanel;
        private System.Windows.Forms.GroupBox PathsToGameExesGroupBox;
        private System.Windows.Forms.Label T3NeedsSULabel;
        private System.Windows.Forms.Label T1T2NeedNewDarkLabel;
        private System.Windows.Forms.Label Thief3ExePathLabel;
        private System.Windows.Forms.Label Thief2ExePathLabel;
        private System.Windows.Forms.Label Thief1ExePathLabel;
        private System.Windows.Forms.Button Thief3ExePathBrowseButton;
        private System.Windows.Forms.Button Thief2ExePathBrowseButton;
        private System.Windows.Forms.Button Thief1ExePathBrowseButton;
        private System.Windows.Forms.TextBox Thief3ExePathTextBox;
        private System.Windows.Forms.TextBox Thief2ExePathTextBox;
        private System.Windows.Forms.TextBox Thief1ExePathTextBox;
        private System.Windows.Forms.GroupBox OtherGroupBox;
        private System.Windows.Forms.Label BackupPathLabel;
        private System.Windows.Forms.Button BackupPathBrowseButton;
        private System.Windows.Forms.TextBox BackupPathTextBox;
    }
}