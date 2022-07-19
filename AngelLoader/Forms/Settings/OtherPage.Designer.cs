﻿#define FenGen_DesignerSource

namespace AngelLoader.Forms
{
    sealed partial class OtherPage
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
            this.InstallingFMsGroupBox = new AngelLoader.Forms.CustomControls.DarkGroupBox();
            this.Install_ConfirmNeverRadioButton = new AngelLoader.Forms.CustomControls.DarkRadioButton();
            this.Install_ConfirmMultipleOnlyRadioButton = new AngelLoader.Forms.CustomControls.DarkRadioButton();
            this.Install_ConfirmAlwaysRadioButton = new AngelLoader.Forms.CustomControls.DarkRadioButton();
            this.ConfirmBeforeInstallLabel = new AngelLoader.Forms.CustomControls.DarkLabel();
            this.PlayFMOnDCOrEnterGroupBox = new AngelLoader.Forms.CustomControls.DarkGroupBox();
            this.ConfirmPlayOnDCOrEnterCheckBox = new AngelLoader.Forms.CustomControls.DarkCheckBox();
            this.WebSearchGroupBox = new AngelLoader.Forms.CustomControls.DarkGroupBox();
            this.WebSearchUrlResetButton = new AngelLoader.Forms.CustomControls.DarkButton();
            this.WebSearchTitleExplanationLabel = new AngelLoader.Forms.CustomControls.DarkLabel();
            this.WebSearchUrlTextBox = new AngelLoader.Forms.CustomControls.DarkTextBox();
            this.WebSearchUrlLabel = new AngelLoader.Forms.CustomControls.DarkLabel();
            this.UninstallingFMsGroupBox = new AngelLoader.Forms.CustomControls.DarkGroupBox();
            this.ConfirmUninstallCheckBox = new AngelLoader.Forms.CustomControls.DarkCheckBox();
            this.WhatToBackUpLabel = new AngelLoader.Forms.CustomControls.DarkLabel();
            this.BackupAlwaysAskCheckBox = new AngelLoader.Forms.CustomControls.DarkCheckBox();
            this.BackupAllChangedDataRadioButton = new AngelLoader.Forms.CustomControls.DarkRadioButton();
            this.BackupSavesAndScreensOnlyRadioButton = new AngelLoader.Forms.CustomControls.DarkRadioButton();
            this.FMSettingsGroupBox = new AngelLoader.Forms.CustomControls.DarkGroupBox();
            this.ConvertOGGsToWAVsOnInstallCheckBox = new AngelLoader.Forms.CustomControls.DarkCheckBox();
            this.ConvertWAVsTo16BitOnInstallCheckBox = new AngelLoader.Forms.CustomControls.DarkCheckBox();
            this.DummyAutoScrollPanel = new System.Windows.Forms.Control();
            this.OldMantleForOldDarkFMsCheckBox = new AngelLoader.Forms.CustomControls.DarkCheckBox();
            this.PagePanel.SuspendLayout();
            this.InstallingFMsGroupBox.SuspendLayout();
            this.PlayFMOnDCOrEnterGroupBox.SuspendLayout();
            this.WebSearchGroupBox.SuspendLayout();
            this.UninstallingFMsGroupBox.SuspendLayout();
            this.FMSettingsGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // PagePanel
            // 
            this.PagePanel.AutoScroll = true;
            this.PagePanel.Controls.Add(this.InstallingFMsGroupBox);
            this.PagePanel.Controls.Add(this.PlayFMOnDCOrEnterGroupBox);
            this.PagePanel.Controls.Add(this.WebSearchGroupBox);
            this.PagePanel.Controls.Add(this.UninstallingFMsGroupBox);
            this.PagePanel.Controls.Add(this.FMSettingsGroupBox);
            this.PagePanel.Controls.Add(this.DummyAutoScrollPanel);
            this.PagePanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PagePanel.Location = new System.Drawing.Point(0, 0);
            this.PagePanel.Name = "PagePanel";
            this.PagePanel.Size = new System.Drawing.Size(440, 623);
            this.PagePanel.TabIndex = 0;
            // 
            // InstallingFMsGroupBox
            // 
            this.InstallingFMsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.InstallingFMsGroupBox.Controls.Add(this.Install_ConfirmNeverRadioButton);
            this.InstallingFMsGroupBox.Controls.Add(this.Install_ConfirmMultipleOnlyRadioButton);
            this.InstallingFMsGroupBox.Controls.Add(this.Install_ConfirmAlwaysRadioButton);
            this.InstallingFMsGroupBox.Controls.Add(this.ConfirmBeforeInstallLabel);
            this.InstallingFMsGroupBox.Location = new System.Drawing.Point(8, 140);
            this.InstallingFMsGroupBox.MinimumSize = new System.Drawing.Size(424, 0);
            this.InstallingFMsGroupBox.Name = "InstallingFMsGroupBox";
            this.InstallingFMsGroupBox.Size = new System.Drawing.Size(424, 124);
            this.InstallingFMsGroupBox.TabIndex = 1;
            this.InstallingFMsGroupBox.TabStop = false;
            this.InstallingFMsGroupBox.Text = "Installing FMs";
            // 
            // Install_ConfirmNeverRadioButton
            // 
            this.Install_ConfirmNeverRadioButton.AutoSize = true;
            this.Install_ConfirmNeverRadioButton.Location = new System.Drawing.Point(24, 96);
            this.Install_ConfirmNeverRadioButton.Name = "Install_ConfirmNeverRadioButton";
            this.Install_ConfirmNeverRadioButton.Size = new System.Drawing.Size(54, 17);
            this.Install_ConfirmNeverRadioButton.TabIndex = 3;
            this.Install_ConfirmNeverRadioButton.Text = "Never";
            this.Install_ConfirmNeverRadioButton.UseVisualStyleBackColor = true;
            // 
            // Install_ConfirmMultipleOnlyRadioButton
            // 
            this.Install_ConfirmMultipleOnlyRadioButton.AutoSize = true;
            this.Install_ConfirmMultipleOnlyRadioButton.Checked = true;
            this.Install_ConfirmMultipleOnlyRadioButton.Location = new System.Drawing.Point(24, 72);
            this.Install_ConfirmMultipleOnlyRadioButton.Name = "Install_ConfirmMultipleOnlyRadioButton";
            this.Install_ConfirmMultipleOnlyRadioButton.Size = new System.Drawing.Size(122, 17);
            this.Install_ConfirmMultipleOnlyRadioButton.TabIndex = 2;
            this.Install_ConfirmMultipleOnlyRadioButton.TabStop = true;
            this.Install_ConfirmMultipleOnlyRadioButton.Text = "Only for multiple FMs";
            this.Install_ConfirmMultipleOnlyRadioButton.UseVisualStyleBackColor = true;
            // 
            // Install_ConfirmAlwaysRadioButton
            // 
            this.Install_ConfirmAlwaysRadioButton.AutoSize = true;
            this.Install_ConfirmAlwaysRadioButton.Location = new System.Drawing.Point(24, 48);
            this.Install_ConfirmAlwaysRadioButton.Name = "Install_ConfirmAlwaysRadioButton";
            this.Install_ConfirmAlwaysRadioButton.Size = new System.Drawing.Size(58, 17);
            this.Install_ConfirmAlwaysRadioButton.TabIndex = 1;
            this.Install_ConfirmAlwaysRadioButton.Text = "Always";
            this.Install_ConfirmAlwaysRadioButton.UseVisualStyleBackColor = true;
            // 
            // ConfirmBeforeInstallLabel
            // 
            this.ConfirmBeforeInstallLabel.AutoSize = true;
            this.ConfirmBeforeInstallLabel.Location = new System.Drawing.Point(16, 24);
            this.ConfirmBeforeInstallLabel.Name = "ConfirmBeforeInstallLabel";
            this.ConfirmBeforeInstallLabel.Size = new System.Drawing.Size(121, 13);
            this.ConfirmBeforeInstallLabel.TabIndex = 0;
            this.ConfirmBeforeInstallLabel.Text = "Confirm before installing:";
            // 
            // PlayFMOnDCOrEnterGroupBox
            // 
            this.PlayFMOnDCOrEnterGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.PlayFMOnDCOrEnterGroupBox.Controls.Add(this.ConfirmPlayOnDCOrEnterCheckBox);
            this.PlayFMOnDCOrEnterGroupBox.Location = new System.Drawing.Point(8, 556);
            this.PlayFMOnDCOrEnterGroupBox.MinimumSize = new System.Drawing.Size(424, 0);
            this.PlayFMOnDCOrEnterGroupBox.Name = "PlayFMOnDCOrEnterGroupBox";
            this.PlayFMOnDCOrEnterGroupBox.Size = new System.Drawing.Size(424, 56);
            this.PlayFMOnDCOrEnterGroupBox.TabIndex = 4;
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
            // WebSearchGroupBox
            // 
            this.WebSearchGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.WebSearchGroupBox.Controls.Add(this.WebSearchUrlResetButton);
            this.WebSearchGroupBox.Controls.Add(this.WebSearchTitleExplanationLabel);
            this.WebSearchGroupBox.Controls.Add(this.WebSearchUrlTextBox);
            this.WebSearchGroupBox.Controls.Add(this.WebSearchUrlLabel);
            this.WebSearchGroupBox.Location = new System.Drawing.Point(8, 436);
            this.WebSearchGroupBox.MinimumSize = new System.Drawing.Size(424, 0);
            this.WebSearchGroupBox.Name = "WebSearchGroupBox";
            this.WebSearchGroupBox.Size = new System.Drawing.Size(424, 108);
            this.WebSearchGroupBox.TabIndex = 3;
            this.WebSearchGroupBox.TabStop = false;
            this.WebSearchGroupBox.Text = "Web search";
            // 
            // WebSearchUrlResetButton
            // 
            this.WebSearchUrlResetButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.WebSearchUrlResetButton.Location = new System.Drawing.Point(393, 47);
            this.WebSearchUrlResetButton.Name = "WebSearchUrlResetButton";
            this.WebSearchUrlResetButton.Size = new System.Drawing.Size(22, 22);
            this.WebSearchUrlResetButton.TabIndex = 2;
            this.WebSearchUrlResetButton.UseVisualStyleBackColor = true;
            this.WebSearchUrlResetButton.PaintCustom += new System.Windows.Forms.PaintEventHandler(this.WebSearchUrlResetButton_Paint);
            // 
            // WebSearchTitleExplanationLabel
            // 
            this.WebSearchTitleExplanationLabel.AutoSize = true;
            this.WebSearchTitleExplanationLabel.Location = new System.Drawing.Point(16, 78);
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
            this.WebSearchUrlTextBox.Size = new System.Drawing.Size(376, 20);
            this.WebSearchUrlTextBox.TabIndex = 1;
            // 
            // WebSearchUrlLabel
            // 
            this.WebSearchUrlLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.WebSearchUrlLabel.Location = new System.Drawing.Point(16, 16);
            this.WebSearchUrlLabel.Name = "WebSearchUrlLabel";
            this.WebSearchUrlLabel.Size = new System.Drawing.Size(400, 32);
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
            this.UninstallingFMsGroupBox.Location = new System.Drawing.Point(8, 276);
            this.UninstallingFMsGroupBox.MinimumSize = new System.Drawing.Size(424, 0);
            this.UninstallingFMsGroupBox.Name = "UninstallingFMsGroupBox";
            this.UninstallingFMsGroupBox.Size = new System.Drawing.Size(424, 148);
            this.UninstallingFMsGroupBox.TabIndex = 2;
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
            this.WhatToBackUpLabel.TabIndex = 1;
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
            this.BackupAlwaysAskCheckBox.TabIndex = 4;
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
            this.BackupAllChangedDataRadioButton.TabIndex = 3;
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
            this.BackupSavesAndScreensOnlyRadioButton.TabIndex = 2;
            this.BackupSavesAndScreensOnlyRadioButton.Text = "Saves and screenshots only";
            this.BackupSavesAndScreensOnlyRadioButton.UseVisualStyleBackColor = true;
            // 
            // FMSettingsGroupBox
            // 
            this.FMSettingsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.FMSettingsGroupBox.Controls.Add(this.OldMantleForOldDarkFMsCheckBox);
            this.FMSettingsGroupBox.Controls.Add(this.ConvertOGGsToWAVsOnInstallCheckBox);
            this.FMSettingsGroupBox.Controls.Add(this.ConvertWAVsTo16BitOnInstallCheckBox);
            this.FMSettingsGroupBox.Location = new System.Drawing.Point(8, 8);
            this.FMSettingsGroupBox.MinimumSize = new System.Drawing.Size(424, 0);
            this.FMSettingsGroupBox.Name = "FMSettingsGroupBox";
            this.FMSettingsGroupBox.Size = new System.Drawing.Size(424, 120);
            this.FMSettingsGroupBox.TabIndex = 0;
            this.FMSettingsGroupBox.TabStop = false;
            this.FMSettingsGroupBox.Text = "FM settings";
            // 
            // ConvertOGGsToWAVsOnInstallCheckBox
            // 
            this.ConvertOGGsToWAVsOnInstallCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ConvertOGGsToWAVsOnInstallCheckBox.Location = new System.Drawing.Point(16, 44);
            this.ConvertOGGsToWAVsOnInstallCheckBox.Name = "ConvertOGGsToWAVsOnInstallCheckBox";
            this.ConvertOGGsToWAVsOnInstallCheckBox.Size = new System.Drawing.Size(400, 32);
            this.ConvertOGGsToWAVsOnInstallCheckBox.TabIndex = 1;
            this.ConvertOGGsToWAVsOnInstallCheckBox.Text = "Convert .oggs to .wavs on install";
            this.ConvertOGGsToWAVsOnInstallCheckBox.UseVisualStyleBackColor = true;
            // 
            // ConvertWAVsTo16BitOnInstallCheckBox
            // 
            this.ConvertWAVsTo16BitOnInstallCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ConvertWAVsTo16BitOnInstallCheckBox.Checked = true;
            this.ConvertWAVsTo16BitOnInstallCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ConvertWAVsTo16BitOnInstallCheckBox.Location = new System.Drawing.Point(16, 16);
            this.ConvertWAVsTo16BitOnInstallCheckBox.Name = "ConvertWAVsTo16BitOnInstallCheckBox";
            this.ConvertWAVsTo16BitOnInstallCheckBox.Size = new System.Drawing.Size(400, 32);
            this.ConvertWAVsTo16BitOnInstallCheckBox.TabIndex = 0;
            this.ConvertWAVsTo16BitOnInstallCheckBox.Text = "Convert .wavs to 16 bit on install";
            this.ConvertWAVsTo16BitOnInstallCheckBox.UseVisualStyleBackColor = true;
            // 
            // DummyAutoScrollPanel
            // 
            this.DummyAutoScrollPanel.Location = new System.Drawing.Point(8, 48);
            this.DummyAutoScrollPanel.Name = "DummyAutoScrollPanel";
            this.DummyAutoScrollPanel.Size = new System.Drawing.Size(424, 8);
            this.DummyAutoScrollPanel.TabIndex = 12;
            // 
            // OldMantleForOldDarkFMsCheckBox
            // 
            this.OldMantleForOldDarkFMsCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.OldMantleForOldDarkFMsCheckBox.Location = new System.Drawing.Point(16, 80);
            this.OldMantleForOldDarkFMsCheckBox.Name = "OldMantleForOldDarkFMsCheckBox";
            this.OldMantleForOldDarkFMsCheckBox.Size = new System.Drawing.Size(400, 32);
            this.OldMantleForOldDarkFMsCheckBox.TabIndex = 2;
            this.OldMantleForOldDarkFMsCheckBox.Text = "Use old mantling for OldDark FMs";
            this.OldMantleForOldDarkFMsCheckBox.UseVisualStyleBackColor = true;
            // 
            // OtherPage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.PagePanel);
            this.Name = "OtherPage";
            this.Size = new System.Drawing.Size(440, 623);
            this.PagePanel.ResumeLayout(false);
            this.InstallingFMsGroupBox.ResumeLayout(false);
            this.InstallingFMsGroupBox.PerformLayout();
            this.PlayFMOnDCOrEnterGroupBox.ResumeLayout(false);
            this.PlayFMOnDCOrEnterGroupBox.PerformLayout();
            this.WebSearchGroupBox.ResumeLayout(false);
            this.WebSearchGroupBox.PerformLayout();
            this.UninstallingFMsGroupBox.ResumeLayout(false);
            this.UninstallingFMsGroupBox.PerformLayout();
            this.FMSettingsGroupBox.ResumeLayout(false);
            this.ResumeLayout(false);

        }
#endif

        #endregion

        internal System.Windows.Forms.Panel PagePanel;
        internal AngelLoader.Forms.CustomControls.DarkGroupBox PlayFMOnDCOrEnterGroupBox;
        internal AngelLoader.Forms.CustomControls.DarkCheckBox ConfirmPlayOnDCOrEnterCheckBox;
        internal AngelLoader.Forms.CustomControls.DarkGroupBox WebSearchGroupBox;
        internal AngelLoader.Forms.CustomControls.DarkButton WebSearchUrlResetButton;
        internal AngelLoader.Forms.CustomControls.DarkLabel WebSearchTitleExplanationLabel;
        internal AngelLoader.Forms.CustomControls.DarkTextBox WebSearchUrlTextBox;
        internal AngelLoader.Forms.CustomControls.DarkLabel WebSearchUrlLabel;
        internal AngelLoader.Forms.CustomControls.DarkGroupBox UninstallingFMsGroupBox;
        internal AngelLoader.Forms.CustomControls.DarkCheckBox ConfirmUninstallCheckBox;
        internal AngelLoader.Forms.CustomControls.DarkLabel WhatToBackUpLabel;
        internal AngelLoader.Forms.CustomControls.DarkCheckBox BackupAlwaysAskCheckBox;
        internal AngelLoader.Forms.CustomControls.DarkRadioButton BackupAllChangedDataRadioButton;
        internal AngelLoader.Forms.CustomControls.DarkRadioButton BackupSavesAndScreensOnlyRadioButton;
        internal AngelLoader.Forms.CustomControls.DarkGroupBox FMSettingsGroupBox;
        internal AngelLoader.Forms.CustomControls.DarkCheckBox ConvertOGGsToWAVsOnInstallCheckBox;
        internal AngelLoader.Forms.CustomControls.DarkCheckBox ConvertWAVsTo16BitOnInstallCheckBox;
        internal System.Windows.Forms.Control DummyAutoScrollPanel;
        internal CustomControls.DarkGroupBox InstallingFMsGroupBox;
        internal CustomControls.DarkRadioButton Install_ConfirmNeverRadioButton;
        internal CustomControls.DarkRadioButton Install_ConfirmMultipleOnlyRadioButton;
        internal CustomControls.DarkRadioButton Install_ConfirmAlwaysRadioButton;
        internal CustomControls.DarkLabel ConfirmBeforeInstallLabel;
        internal CustomControls.DarkCheckBox OldMantleForOldDarkFMsCheckBox;
    }
}
