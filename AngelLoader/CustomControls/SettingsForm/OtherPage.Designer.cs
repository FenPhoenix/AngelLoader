namespace AngelLoader.CustomControls.SettingsForm
{
    partial class OtherPage
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
            this.ShowOrHideUIElementsGroupBox = new System.Windows.Forms.GroupBox();
            this.HideFMListZoomButtonsCheckBox = new System.Windows.Forms.CheckBox();
            this.HideUninstallButtonCheckBox = new System.Windows.Forms.CheckBox();
            this.PlayFMOnDCOrEnterGroupBox = new System.Windows.Forms.GroupBox();
            this.ConfirmPlayOnDCOrEnterCheckBox = new System.Windows.Forms.CheckBox();
            this.LanguageGroupBox = new System.Windows.Forms.GroupBox();
            this.LanguageComboBox = new AngelLoader.CustomControls.ComboBoxCustom();
            this.WebSearchGroupBox = new System.Windows.Forms.GroupBox();
            this.WebSearchTitleExplanationLabel = new System.Windows.Forms.Label();
            this.WebSearchUrlTextBox = new System.Windows.Forms.TextBox();
            this.WebSearchUrlLabel = new System.Windows.Forms.Label();
            this.UninstallingFMsGroupBox = new System.Windows.Forms.GroupBox();
            this.ConfirmUninstallCheckBox = new System.Windows.Forms.CheckBox();
            this.WhatToBackUpLabel = new System.Windows.Forms.Label();
            this.BackupAlwaysAskCheckBox = new System.Windows.Forms.CheckBox();
            this.BackupAllChangedDataRadioButton = new System.Windows.Forms.RadioButton();
            this.BackupSavesAndScreensOnlyRadioButton = new System.Windows.Forms.RadioButton();
            this.FMFileConversionGroupBox = new System.Windows.Forms.GroupBox();
            this.ConvertOGGsToWAVsOnInstallCheckBox = new System.Windows.Forms.CheckBox();
            this.ConvertWAVsTo16BitOnInstallCheckBox = new System.Windows.Forms.CheckBox();
            this.WebSearchUrlResetButton = new System.Windows.Forms.Button();
            this.PagePanel.SuspendLayout();
            this.ShowOrHideUIElementsGroupBox.SuspendLayout();
            this.PlayFMOnDCOrEnterGroupBox.SuspendLayout();
            this.LanguageGroupBox.SuspendLayout();
            this.WebSearchGroupBox.SuspendLayout();
            this.UninstallingFMsGroupBox.SuspendLayout();
            this.FMFileConversionGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // PagePanel
            // 
            this.PagePanel.Controls.Add(this.ShowOrHideUIElementsGroupBox);
            this.PagePanel.Controls.Add(this.PlayFMOnDCOrEnterGroupBox);
            this.PagePanel.Controls.Add(this.LanguageGroupBox);
            this.PagePanel.Controls.Add(this.WebSearchGroupBox);
            this.PagePanel.Controls.Add(this.UninstallingFMsGroupBox);
            this.PagePanel.Controls.Add(this.FMFileConversionGroupBox);
            this.PagePanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PagePanel.Location = new System.Drawing.Point(0, 0);
            this.PagePanel.Name = "PagePanel";
            this.PagePanel.Size = new System.Drawing.Size(440, 584);
            this.PagePanel.TabIndex = 0;
            // 
            // ShowOrHideUIElementsGroupBox
            // 
            this.ShowOrHideUIElementsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ShowOrHideUIElementsGroupBox.Controls.Add(this.HideFMListZoomButtonsCheckBox);
            this.ShowOrHideUIElementsGroupBox.Controls.Add(this.HideUninstallButtonCheckBox);
            this.ShowOrHideUIElementsGroupBox.Location = new System.Drawing.Point(8, 492);
            this.ShowOrHideUIElementsGroupBox.Name = "ShowOrHideUIElementsGroupBox";
            this.ShowOrHideUIElementsGroupBox.Size = new System.Drawing.Size(424, 80);
            this.ShowOrHideUIElementsGroupBox.TabIndex = 11;
            this.ShowOrHideUIElementsGroupBox.TabStop = false;
            this.ShowOrHideUIElementsGroupBox.Text = "Show or hide interface elements";
            // 
            // HideFMListZoomButtonsCheckBox
            // 
            this.HideFMListZoomButtonsCheckBox.Location = new System.Drawing.Point(16, 44);
            this.HideFMListZoomButtonsCheckBox.Name = "HideFMListZoomButtonsCheckBox";
            this.HideFMListZoomButtonsCheckBox.Size = new System.Drawing.Size(392, 32);
            this.HideFMListZoomButtonsCheckBox.TabIndex = 2;
            this.HideFMListZoomButtonsCheckBox.Text = "Hide FM list zoom buttons";
            this.HideFMListZoomButtonsCheckBox.UseVisualStyleBackColor = true;
            // 
            // HideUninstallButtonCheckBox
            // 
            this.HideUninstallButtonCheckBox.Location = new System.Drawing.Point(16, 16);
            this.HideUninstallButtonCheckBox.Name = "HideUninstallButtonCheckBox";
            this.HideUninstallButtonCheckBox.Size = new System.Drawing.Size(392, 32);
            this.HideUninstallButtonCheckBox.TabIndex = 1;
            this.HideUninstallButtonCheckBox.Text = "Hide \"Install / Uninstall FM\" button (like FMSel)";
            this.HideUninstallButtonCheckBox.UseVisualStyleBackColor = true;
            // 
            // PlayFMOnDCOrEnterGroupBox
            // 
            this.PlayFMOnDCOrEnterGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.PlayFMOnDCOrEnterGroupBox.Controls.Add(this.ConfirmPlayOnDCOrEnterCheckBox);
            this.PlayFMOnDCOrEnterGroupBox.Location = new System.Drawing.Point(8, 428);
            this.PlayFMOnDCOrEnterGroupBox.Name = "PlayFMOnDCOrEnterGroupBox";
            this.PlayFMOnDCOrEnterGroupBox.Size = new System.Drawing.Size(424, 56);
            this.PlayFMOnDCOrEnterGroupBox.TabIndex = 9;
            this.PlayFMOnDCOrEnterGroupBox.TabStop = false;
            this.PlayFMOnDCOrEnterGroupBox.Text = "Play FM on double-click / Enter";
            // 
            // ConfirmPlayOnDCOrEnterCheckBox
            // 
            this.ConfirmPlayOnDCOrEnterCheckBox.AutoSize = true;
            this.ConfirmPlayOnDCOrEnterCheckBox.Checked = true;
            this.ConfirmPlayOnDCOrEnterCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ConfirmPlayOnDCOrEnterCheckBox.Location = new System.Drawing.Point(16, 24);
            this.ConfirmPlayOnDCOrEnterCheckBox.Name = "ConfirmPlayOnDCOrEnterCheckBox";
            this.ConfirmPlayOnDCOrEnterCheckBox.Size = new System.Drawing.Size(119, 17);
            this.ConfirmPlayOnDCOrEnterCheckBox.TabIndex = 0;
            this.ConfirmPlayOnDCOrEnterCheckBox.Text = "Ask for confirmation";
            this.ConfirmPlayOnDCOrEnterCheckBox.UseVisualStyleBackColor = true;
            // 
            // LanguageGroupBox
            // 
            this.LanguageGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.LanguageGroupBox.Controls.Add(this.LanguageComboBox);
            this.LanguageGroupBox.Location = new System.Drawing.Point(8, 256);
            this.LanguageGroupBox.Name = "LanguageGroupBox";
            this.LanguageGroupBox.Size = new System.Drawing.Size(424, 60);
            this.LanguageGroupBox.TabIndex = 10;
            this.LanguageGroupBox.TabStop = false;
            this.LanguageGroupBox.Text = "Language";
            // 
            // LanguageComboBox
            // 
            this.LanguageComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.LanguageComboBox.FormattingEnabled = true;
            this.LanguageComboBox.Location = new System.Drawing.Point(16, 24);
            this.LanguageComboBox.Name = "LanguageComboBox";
            this.LanguageComboBox.Size = new System.Drawing.Size(184, 21);
            this.LanguageComboBox.TabIndex = 0;
            // 
            // WebSearchGroupBox
            // 
            this.WebSearchGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.WebSearchGroupBox.Controls.Add(this.WebSearchUrlResetButton);
            this.WebSearchGroupBox.Controls.Add(this.WebSearchTitleExplanationLabel);
            this.WebSearchGroupBox.Controls.Add(this.WebSearchUrlTextBox);
            this.WebSearchGroupBox.Controls.Add(this.WebSearchUrlLabel);
            this.WebSearchGroupBox.Location = new System.Drawing.Point(8, 324);
            this.WebSearchGroupBox.Name = "WebSearchGroupBox";
            this.WebSearchGroupBox.Size = new System.Drawing.Size(424, 96);
            this.WebSearchGroupBox.TabIndex = 8;
            this.WebSearchGroupBox.TabStop = false;
            this.WebSearchGroupBox.Text = "Web search";
            // 
            // WebSearchTitleExplanationLabel
            // 
            this.WebSearchTitleExplanationLabel.AutoSize = true;
            this.WebSearchTitleExplanationLabel.Location = new System.Drawing.Point(16, 72);
            this.WebSearchTitleExplanationLabel.Name = "WebSearchTitleExplanationLabel";
            this.WebSearchTitleExplanationLabel.Size = new System.Drawing.Size(140, 13);
            this.WebSearchTitleExplanationLabel.TabIndex = 3;
            this.WebSearchTitleExplanationLabel.Text = "$TITLE$ : the title of the FM";
            // 
            // WebSearchUrlTextBox
            // 
            this.WebSearchUrlTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.WebSearchUrlTextBox.Location = new System.Drawing.Point(16, 48);
            this.WebSearchUrlTextBox.Name = "WebSearchUrlTextBox";
            this.WebSearchUrlTextBox.Size = new System.Drawing.Size(368, 20);
            this.WebSearchUrlTextBox.TabIndex = 1;
            // 
            // WebSearchUrlLabel
            // 
            this.WebSearchUrlLabel.Location = new System.Drawing.Point(16, 16);
            this.WebSearchUrlLabel.Name = "WebSearchUrlLabel";
            this.WebSearchUrlLabel.Size = new System.Drawing.Size(392, 32);
            this.WebSearchUrlLabel.TabIndex = 0;
            this.WebSearchUrlLabel.Text = "Full URL to use when searching for an FM title:";
            this.WebSearchUrlLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // UninstallingFMsGroupBox
            // 
            this.UninstallingFMsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.UninstallingFMsGroupBox.Controls.Add(this.ConfirmUninstallCheckBox);
            this.UninstallingFMsGroupBox.Controls.Add(this.WhatToBackUpLabel);
            this.UninstallingFMsGroupBox.Controls.Add(this.BackupAlwaysAskCheckBox);
            this.UninstallingFMsGroupBox.Controls.Add(this.BackupAllChangedDataRadioButton);
            this.UninstallingFMsGroupBox.Controls.Add(this.BackupSavesAndScreensOnlyRadioButton);
            this.UninstallingFMsGroupBox.Location = new System.Drawing.Point(8, 100);
            this.UninstallingFMsGroupBox.Name = "UninstallingFMsGroupBox";
            this.UninstallingFMsGroupBox.Size = new System.Drawing.Size(424, 148);
            this.UninstallingFMsGroupBox.TabIndex = 7;
            this.UninstallingFMsGroupBox.TabStop = false;
            this.UninstallingFMsGroupBox.Text = "Uninstalling FMs";
            // 
            // ConfirmUninstallCheckBox
            // 
            this.ConfirmUninstallCheckBox.AutoSize = true;
            this.ConfirmUninstallCheckBox.Checked = true;
            this.ConfirmUninstallCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ConfirmUninstallCheckBox.Location = new System.Drawing.Point(16, 24);
            this.ConfirmUninstallCheckBox.Name = "ConfirmUninstallCheckBox";
            this.ConfirmUninstallCheckBox.Size = new System.Drawing.Size(149, 17);
            this.ConfirmUninstallCheckBox.TabIndex = 0;
            this.ConfirmUninstallCheckBox.Text = "Confirm before uninstalling";
            this.ConfirmUninstallCheckBox.UseVisualStyleBackColor = true;
            // 
            // WhatToBackUpLabel
            // 
            this.WhatToBackUpLabel.AutoSize = true;
            this.WhatToBackUpLabel.Location = new System.Drawing.Point(16, 48);
            this.WhatToBackUpLabel.Name = "WhatToBackUpLabel";
            this.WhatToBackUpLabel.Size = new System.Drawing.Size(139, 13);
            this.WhatToBackUpLabel.TabIndex = 2;
            this.WhatToBackUpLabel.Text = "When uninstalling, back up:";
            // 
            // BackupAlwaysAskCheckBox
            // 
            this.BackupAlwaysAskCheckBox.AutoSize = true;
            this.BackupAlwaysAskCheckBox.Checked = true;
            this.BackupAlwaysAskCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.BackupAlwaysAskCheckBox.Location = new System.Drawing.Point(24, 120);
            this.BackupAlwaysAskCheckBox.Name = "BackupAlwaysAskCheckBox";
            this.BackupAlwaysAskCheckBox.Size = new System.Drawing.Size(79, 17);
            this.BackupAlwaysAskCheckBox.TabIndex = 5;
            this.BackupAlwaysAskCheckBox.Text = "Always ask";
            this.BackupAlwaysAskCheckBox.UseVisualStyleBackColor = true;
            // 
            // BackupAllChangedDataRadioButton
            // 
            this.BackupAllChangedDataRadioButton.AutoSize = true;
            this.BackupAllChangedDataRadioButton.Checked = true;
            this.BackupAllChangedDataRadioButton.Location = new System.Drawing.Point(24, 94);
            this.BackupAllChangedDataRadioButton.Name = "BackupAllChangedDataRadioButton";
            this.BackupAllChangedDataRadioButton.Size = new System.Drawing.Size(102, 17);
            this.BackupAllChangedDataRadioButton.TabIndex = 4;
            this.BackupAllChangedDataRadioButton.TabStop = true;
            this.BackupAllChangedDataRadioButton.Text = "All changed files";
            this.BackupAllChangedDataRadioButton.UseVisualStyleBackColor = true;
            // 
            // BackupSavesAndScreensOnlyRadioButton
            // 
            this.BackupSavesAndScreensOnlyRadioButton.AutoSize = true;
            this.BackupSavesAndScreensOnlyRadioButton.Location = new System.Drawing.Point(24, 70);
            this.BackupSavesAndScreensOnlyRadioButton.Name = "BackupSavesAndScreensOnlyRadioButton";
            this.BackupSavesAndScreensOnlyRadioButton.Size = new System.Drawing.Size(158, 17);
            this.BackupSavesAndScreensOnlyRadioButton.TabIndex = 3;
            this.BackupSavesAndScreensOnlyRadioButton.Text = "Saves and screenshots only";
            this.BackupSavesAndScreensOnlyRadioButton.UseVisualStyleBackColor = true;
            // 
            // FMFileConversionGroupBox
            // 
            this.FMFileConversionGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.FMFileConversionGroupBox.Controls.Add(this.ConvertOGGsToWAVsOnInstallCheckBox);
            this.FMFileConversionGroupBox.Controls.Add(this.ConvertWAVsTo16BitOnInstallCheckBox);
            this.FMFileConversionGroupBox.Location = new System.Drawing.Point(8, 8);
            this.FMFileConversionGroupBox.Name = "FMFileConversionGroupBox";
            this.FMFileConversionGroupBox.Size = new System.Drawing.Size(424, 84);
            this.FMFileConversionGroupBox.TabIndex = 6;
            this.FMFileConversionGroupBox.TabStop = false;
            this.FMFileConversionGroupBox.Text = "FM file conversion";
            // 
            // ConvertOGGsToWAVsOnInstallCheckBox
            // 
            this.ConvertOGGsToWAVsOnInstallCheckBox.Location = new System.Drawing.Point(16, 44);
            this.ConvertOGGsToWAVsOnInstallCheckBox.Name = "ConvertOGGsToWAVsOnInstallCheckBox";
            this.ConvertOGGsToWAVsOnInstallCheckBox.Size = new System.Drawing.Size(400, 32);
            this.ConvertOGGsToWAVsOnInstallCheckBox.TabIndex = 1;
            this.ConvertOGGsToWAVsOnInstallCheckBox.Text = "Convert .oggs to .wavs on install";
            this.ConvertOGGsToWAVsOnInstallCheckBox.UseVisualStyleBackColor = true;
            // 
            // ConvertWAVsTo16BitOnInstallCheckBox
            // 
            this.ConvertWAVsTo16BitOnInstallCheckBox.Checked = true;
            this.ConvertWAVsTo16BitOnInstallCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ConvertWAVsTo16BitOnInstallCheckBox.Location = new System.Drawing.Point(16, 16);
            this.ConvertWAVsTo16BitOnInstallCheckBox.Name = "ConvertWAVsTo16BitOnInstallCheckBox";
            this.ConvertWAVsTo16BitOnInstallCheckBox.Size = new System.Drawing.Size(400, 32);
            this.ConvertWAVsTo16BitOnInstallCheckBox.TabIndex = 0;
            this.ConvertWAVsTo16BitOnInstallCheckBox.Text = "Convert .wavs to 16 bit on install";
            this.ConvertWAVsTo16BitOnInstallCheckBox.UseVisualStyleBackColor = true;
            // 
            // WebSearchUrlResetButton
            // 
            this.WebSearchUrlResetButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.WebSearchUrlResetButton.BackgroundImage = global::AngelLoader.Properties.Resources.Refresh;
            this.WebSearchUrlResetButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.WebSearchUrlResetButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.WebSearchUrlResetButton.Location = new System.Drawing.Point(386, 48);
            this.WebSearchUrlResetButton.Name = "WebSearchUrlResetButton";
            this.WebSearchUrlResetButton.Size = new System.Drawing.Size(20, 20);
            this.WebSearchUrlResetButton.TabIndex = 2;
            this.WebSearchUrlResetButton.UseVisualStyleBackColor = true;
            // 
            // OtherPage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.PagePanel);
            this.Name = "OtherPage";
            this.Size = new System.Drawing.Size(440, 584);
            this.PagePanel.ResumeLayout(false);
            this.ShowOrHideUIElementsGroupBox.ResumeLayout(false);
            this.PlayFMOnDCOrEnterGroupBox.ResumeLayout(false);
            this.PlayFMOnDCOrEnterGroupBox.PerformLayout();
            this.LanguageGroupBox.ResumeLayout(false);
            this.WebSearchGroupBox.ResumeLayout(false);
            this.WebSearchGroupBox.PerformLayout();
            this.UninstallingFMsGroupBox.ResumeLayout(false);
            this.UninstallingFMsGroupBox.PerformLayout();
            this.FMFileConversionGroupBox.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel PagePanel;
        private System.Windows.Forms.GroupBox ShowOrHideUIElementsGroupBox;
        private System.Windows.Forms.CheckBox HideFMListZoomButtonsCheckBox;
        private System.Windows.Forms.CheckBox HideUninstallButtonCheckBox;
        private System.Windows.Forms.GroupBox PlayFMOnDCOrEnterGroupBox;
        private System.Windows.Forms.CheckBox ConfirmPlayOnDCOrEnterCheckBox;
        private System.Windows.Forms.GroupBox LanguageGroupBox;
        private ComboBoxCustom LanguageComboBox;
        private System.Windows.Forms.GroupBox WebSearchGroupBox;
        private System.Windows.Forms.Button WebSearchUrlResetButton;
        private System.Windows.Forms.Label WebSearchTitleExplanationLabel;
        private System.Windows.Forms.TextBox WebSearchUrlTextBox;
        private System.Windows.Forms.Label WebSearchUrlLabel;
        private System.Windows.Forms.GroupBox UninstallingFMsGroupBox;
        private System.Windows.Forms.CheckBox ConfirmUninstallCheckBox;
        private System.Windows.Forms.Label WhatToBackUpLabel;
        private System.Windows.Forms.CheckBox BackupAlwaysAskCheckBox;
        private System.Windows.Forms.RadioButton BackupAllChangedDataRadioButton;
        private System.Windows.Forms.RadioButton BackupSavesAndScreensOnlyRadioButton;
        private System.Windows.Forms.GroupBox FMFileConversionGroupBox;
        private System.Windows.Forms.CheckBox ConvertOGGsToWAVsOnInstallCheckBox;
        private System.Windows.Forms.CheckBox ConvertWAVsTo16BitOnInstallCheckBox;
    }
}
