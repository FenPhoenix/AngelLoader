using System.Drawing;
using System.Windows.Forms;

namespace AngelLoader.Forms.CustomControls.SettingsPages
{
    public sealed partial class OtherPage
    {
        private void InitComponentManual()
        {
            PagePanel = new DarkPanel();
            PlayFMOnDCOrEnterGroupBox = new DarkGroupBox();
            ConfirmPlayOnDCOrEnterCheckBox = new DarkCheckBox();
            WebSearchGroupBox = new DarkGroupBox();
            WebSearchUrlResetButton = new DarkButton();
            WebSearchTitleExplanationLabel = new DarkLabel();
            WebSearchUrlTextBox = new DarkTextBox();
            WebSearchUrlLabel = new DarkLabel();
            UninstallingFMsGroupBox = new DarkGroupBox();
            ConfirmUninstallCheckBox = new DarkCheckBox();
            WhatToBackUpLabel = new DarkLabel();
            BackupAlwaysAskCheckBox = new DarkCheckBox();
            BackupAllChangedDataRadioButton = new DarkRadioButton();
            BackupSavesAndScreensOnlyRadioButton = new DarkRadioButton();
            FMFileConversionGroupBox = new DarkGroupBox();
            ConvertOGGsToWAVsOnInstallCheckBox = new DarkCheckBox();
            ConvertWAVsTo16BitOnInstallCheckBox = new DarkCheckBox();
            DummyAutoScrollPanel = new Control();
            PagePanel.SuspendLayout();
            PlayFMOnDCOrEnterGroupBox.SuspendLayout();
            WebSearchGroupBox.SuspendLayout();
            UninstallingFMsGroupBox.SuspendLayout();
            FMFileConversionGroupBox.SuspendLayout();
            SuspendLayout();
            // 
            // PagePanel
            // 
            PagePanel.AutoScroll = true;
            PagePanel.Controls.Add(PlayFMOnDCOrEnterGroupBox);
            PagePanel.Controls.Add(WebSearchGroupBox);
            PagePanel.Controls.Add(UninstallingFMsGroupBox);
            PagePanel.Controls.Add(FMFileConversionGroupBox);
            PagePanel.Controls.Add(DummyAutoScrollPanel);
            PagePanel.Dock = DockStyle.Fill;
            PagePanel.Location = new Point(0, 0);
            PagePanel.Size = new Size(440, 449);
            PagePanel.TabIndex = 0;
            // 
            // PlayFMOnDCOrEnterGroupBox
            // 
            PlayFMOnDCOrEnterGroupBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            PlayFMOnDCOrEnterGroupBox.Controls.Add(ConfirmPlayOnDCOrEnterCheckBox);
            PlayFMOnDCOrEnterGroupBox.Location = new Point(8, 384);
            PlayFMOnDCOrEnterGroupBox.MinimumSize = new Size(424, 0);
            PlayFMOnDCOrEnterGroupBox.Size = new Size(424, 56);
            PlayFMOnDCOrEnterGroupBox.TabIndex = 4;
            PlayFMOnDCOrEnterGroupBox.TabStop = false;
            // 
            // ConfirmPlayOnDCOrEnterCheckBox
            // 
            ConfirmPlayOnDCOrEnterCheckBox.AutoSize = true;
            ConfirmPlayOnDCOrEnterCheckBox.Checked = true;
            ConfirmPlayOnDCOrEnterCheckBox.Location = new Point(16, 24);
            ConfirmPlayOnDCOrEnterCheckBox.TabIndex = 0;
            ConfirmPlayOnDCOrEnterCheckBox.UseVisualStyleBackColor = true;
            //
            // WebSearchGroupBox
            // 
            WebSearchGroupBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            WebSearchGroupBox.Controls.Add(WebSearchUrlResetButton);
            WebSearchGroupBox.Controls.Add(WebSearchTitleExplanationLabel);
            WebSearchGroupBox.Controls.Add(WebSearchUrlTextBox);
            WebSearchGroupBox.Controls.Add(WebSearchUrlLabel);
            WebSearchGroupBox.Location = new Point(8, 264);
            WebSearchGroupBox.MinimumSize = new Size(424, 0);
            WebSearchGroupBox.Size = new Size(424, 108);
            WebSearchGroupBox.TabIndex = 3;
            WebSearchGroupBox.TabStop = false;
            // 
            // WebSearchUrlResetButton
            // 
            WebSearchUrlResetButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            WebSearchUrlResetButton.Location = new Point(393, 47);
            WebSearchUrlResetButton.Size = new Size(22, 22);
            WebSearchUrlResetButton.TabIndex = 2;
            WebSearchUrlResetButton.UseVisualStyleBackColor = true;
            WebSearchUrlResetButton.PaintCustom += WebSearchUrlResetButton_Paint;
            // 
            // WebSearchTitleExplanationLabel
            // 
            WebSearchTitleExplanationLabel.AutoSize = true;
            WebSearchTitleExplanationLabel.Location = new Point(16, 78);
            WebSearchTitleExplanationLabel.TabIndex = 3;
            // 
            // WebSearchUrlTextBox
            // 
            WebSearchUrlTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            WebSearchUrlTextBox.Location = new Point(16, 48);
            WebSearchUrlTextBox.Size = new Size(376, 20);
            WebSearchUrlTextBox.TabIndex = 1;
            // 
            // WebSearchUrlLabel
            // 
            WebSearchUrlLabel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            WebSearchUrlLabel.Location = new Point(16, 16);
            WebSearchUrlLabel.Size = new Size(400, 32);
            WebSearchUrlLabel.TabIndex = 0;
            WebSearchUrlLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // UninstallingFMsGroupBox
            // 
            UninstallingFMsGroupBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            UninstallingFMsGroupBox.Controls.Add(ConfirmUninstallCheckBox);
            UninstallingFMsGroupBox.Controls.Add(WhatToBackUpLabel);
            UninstallingFMsGroupBox.Controls.Add(BackupAlwaysAskCheckBox);
            UninstallingFMsGroupBox.Controls.Add(BackupAllChangedDataRadioButton);
            UninstallingFMsGroupBox.Controls.Add(BackupSavesAndScreensOnlyRadioButton);
            UninstallingFMsGroupBox.Location = new Point(8, 104);
            UninstallingFMsGroupBox.MinimumSize = new Size(424, 0);
            UninstallingFMsGroupBox.Size = new Size(424, 148);
            UninstallingFMsGroupBox.TabIndex = 2;
            UninstallingFMsGroupBox.TabStop = false;
            // 
            // ConfirmUninstallCheckBox
            // 
            ConfirmUninstallCheckBox.AutoSize = true;
            ConfirmUninstallCheckBox.Checked = true;
            ConfirmUninstallCheckBox.Location = new Point(16, 24);
            ConfirmUninstallCheckBox.TabIndex = 0;
            ConfirmUninstallCheckBox.UseVisualStyleBackColor = true;
            // 
            // WhatToBackUpLabel
            // 
            WhatToBackUpLabel.AutoSize = true;
            WhatToBackUpLabel.Location = new Point(16, 48);
            WhatToBackUpLabel.TabIndex = 2;
            // 
            // BackupAlwaysAskCheckBox
            // 
            BackupAlwaysAskCheckBox.AutoSize = true;
            BackupAlwaysAskCheckBox.Checked = true;
            BackupAlwaysAskCheckBox.Location = new Point(24, 120);
            BackupAlwaysAskCheckBox.TabIndex = 5;
            BackupAlwaysAskCheckBox.UseVisualStyleBackColor = true;
            // 
            // BackupAllChangedDataRadioButton
            // 
            BackupAllChangedDataRadioButton.AutoSize = true;
            BackupAllChangedDataRadioButton.Checked = true;
            BackupAllChangedDataRadioButton.Location = new Point(24, 94);
            BackupAllChangedDataRadioButton.TabIndex = 4;
            BackupAllChangedDataRadioButton.TabStop = true;
            BackupAllChangedDataRadioButton.UseVisualStyleBackColor = true;
            // 
            // BackupSavesAndScreensOnlyRadioButton
            // 
            BackupSavesAndScreensOnlyRadioButton.AutoSize = true;
            BackupSavesAndScreensOnlyRadioButton.Location = new Point(24, 70);
            BackupSavesAndScreensOnlyRadioButton.TabIndex = 3;
            BackupSavesAndScreensOnlyRadioButton.UseVisualStyleBackColor = true;
            // 
            // FMFileConversionGroupBox
            // 
            FMFileConversionGroupBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            FMFileConversionGroupBox.Controls.Add(ConvertOGGsToWAVsOnInstallCheckBox);
            FMFileConversionGroupBox.Controls.Add(ConvertWAVsTo16BitOnInstallCheckBox);
            FMFileConversionGroupBox.Location = new Point(8, 8);
            FMFileConversionGroupBox.MinimumSize = new Size(424, 0);
            FMFileConversionGroupBox.Size = new Size(424, 84);
            FMFileConversionGroupBox.TabIndex = 1;
            FMFileConversionGroupBox.TabStop = false;
            // 
            // ConvertOGGsToWAVsOnInstallCheckBox
            // 
            ConvertOGGsToWAVsOnInstallCheckBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            ConvertOGGsToWAVsOnInstallCheckBox.Location = new Point(16, 44);
            ConvertOGGsToWAVsOnInstallCheckBox.Size = new Size(400, 32);
            ConvertOGGsToWAVsOnInstallCheckBox.TabIndex = 1;
            ConvertOGGsToWAVsOnInstallCheckBox.UseVisualStyleBackColor = true;
            // 
            // ConvertWAVsTo16BitOnInstallCheckBox
            // 
            ConvertWAVsTo16BitOnInstallCheckBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            ConvertWAVsTo16BitOnInstallCheckBox.Checked = true;
            ConvertWAVsTo16BitOnInstallCheckBox.Location = new Point(16, 16);
            ConvertWAVsTo16BitOnInstallCheckBox.Size = new Size(400, 32);
            ConvertWAVsTo16BitOnInstallCheckBox.TabIndex = 0;
            ConvertWAVsTo16BitOnInstallCheckBox.UseVisualStyleBackColor = true;
            // 
            // DummyAutoScrollPanel
            // 
            DummyAutoScrollPanel.Location = new Point(8, 48);
            DummyAutoScrollPanel.Size = new Size(424, 8);
            DummyAutoScrollPanel.TabIndex = 12;
            // 
            // OtherPage
            // 
            AutoScaleDimensions = new SizeF(6F, 13F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(PagePanel);
            Dock = DockStyle.Fill;
            Size = new Size(440, 449);
            PagePanel.ResumeLayout(false);
            PlayFMOnDCOrEnterGroupBox.ResumeLayout(false);
            PlayFMOnDCOrEnterGroupBox.PerformLayout();
            WebSearchGroupBox.ResumeLayout(false);
            WebSearchGroupBox.PerformLayout();
            UninstallingFMsGroupBox.ResumeLayout(false);
            UninstallingFMsGroupBox.PerformLayout();
            FMFileConversionGroupBox.ResumeLayout(false);
            ResumeLayout(false);
        }
    }
}
