using System.Drawing;
using System.Windows.Forms;

namespace AngelLoader.Forms.CustomControls.SettingsPages
{
    public sealed partial class OtherPage
    {
        private void InitComponentManual()
        {
            PagePanel = new Panel();
            ReadmeGroupBox = new GroupBox();
            ReadmeFixedWidthFontCheckBox = new CheckBox();
            ShowOrHideUIElementsGroupBox = new GroupBox();
            HideExitButtonCheckBox = new CheckBox();
            HideFMListZoomButtonsCheckBox = new CheckBox();
            HideUninstallButtonCheckBox = new CheckBox();
            PlayFMOnDCOrEnterGroupBox = new GroupBox();
            ConfirmPlayOnDCOrEnterCheckBox = new CheckBox();
            LanguageGroupBox = new GroupBox();
            LanguageComboBox = new ComboBoxCustom();
            WebSearchGroupBox = new GroupBox();
            WebSearchUrlResetButton = new Button();
            WebSearchTitleExplanationLabel = new Label();
            WebSearchUrlTextBox = new TextBox();
            WebSearchUrlLabel = new Label();
            UninstallingFMsGroupBox = new GroupBox();
            ConfirmUninstallCheckBox = new CheckBox();
            WhatToBackUpLabel = new Label();
            BackupAlwaysAskCheckBox = new CheckBox();
            BackupAllChangedDataRadioButton = new RadioButton();
            BackupSavesAndScreensOnlyRadioButton = new RadioButton();
            FMFileConversionGroupBox = new GroupBox();
            ConvertOGGsToWAVsOnInstallCheckBox = new CheckBox();
            ConvertWAVsTo16BitOnInstallCheckBox = new CheckBox();
            DummyAutoScrollPanel = new Control();
            PagePanel.SuspendLayout();
            ReadmeGroupBox.SuspendLayout();
            ShowOrHideUIElementsGroupBox.SuspendLayout();
            PlayFMOnDCOrEnterGroupBox.SuspendLayout();
            LanguageGroupBox.SuspendLayout();
            WebSearchGroupBox.SuspendLayout();
            UninstallingFMsGroupBox.SuspendLayout();
            FMFileConversionGroupBox.SuspendLayout();
            SuspendLayout();
            // 
            // PagePanel
            // 
            PagePanel.AutoScroll = true;
            PagePanel.Controls.Add(ReadmeGroupBox);
            PagePanel.Controls.Add(ShowOrHideUIElementsGroupBox);
            PagePanel.Controls.Add(PlayFMOnDCOrEnterGroupBox);
            PagePanel.Controls.Add(LanguageGroupBox);
            PagePanel.Controls.Add(WebSearchGroupBox);
            PagePanel.Controls.Add(UninstallingFMsGroupBox);
            PagePanel.Controls.Add(FMFileConversionGroupBox);
            PagePanel.Controls.Add(DummyAutoScrollPanel);
            PagePanel.Dock = DockStyle.Fill;
            PagePanel.Location = new Point(0, 0);
            PagePanel.Size = new Size(440, 713);
            PagePanel.TabIndex = 0;
            // 
            // ReadmeGroupBox
            // 
            ReadmeGroupBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            ReadmeGroupBox.Controls.Add(ReadmeFixedWidthFontCheckBox);
            ReadmeGroupBox.Location = new Point(8, 640);
            ReadmeGroupBox.Size = new Size(424, 64);
            ReadmeGroupBox.TabIndex = 13;
            ReadmeGroupBox.TabStop = false;
            // 
            // ReadmeFixedWidthFontCheckBox
            // 
            ReadmeFixedWidthFontCheckBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            ReadmeFixedWidthFontCheckBox.Checked = true;
            ReadmeFixedWidthFontCheckBox.Location = new Point(16, 24);
            ReadmeFixedWidthFontCheckBox.Size = new Size(400, 32);
            ReadmeFixedWidthFontCheckBox.TabIndex = 0;
            ReadmeFixedWidthFontCheckBox.UseVisualStyleBackColor = true;
            // 
            // ShowOrHideUIElementsGroupBox
            // 
            ShowOrHideUIElementsGroupBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            ShowOrHideUIElementsGroupBox.Controls.Add(HideExitButtonCheckBox);
            ShowOrHideUIElementsGroupBox.Controls.Add(HideFMListZoomButtonsCheckBox);
            ShowOrHideUIElementsGroupBox.Controls.Add(HideUninstallButtonCheckBox);
            ShowOrHideUIElementsGroupBox.Location = new Point(8, 525);
            ShowOrHideUIElementsGroupBox.MinimumSize = new Size(424, 0);
            ShowOrHideUIElementsGroupBox.Size = new Size(424, 107);
            ShowOrHideUIElementsGroupBox.TabIndex = 5;
            ShowOrHideUIElementsGroupBox.TabStop = false;
            // 
            // HideExitButtonCheckBox
            // 
            HideExitButtonCheckBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            HideExitButtonCheckBox.Checked = true;
            HideExitButtonCheckBox.Location = new Point(16, 72);
            HideExitButtonCheckBox.Size = new Size(400, 32);
            HideExitButtonCheckBox.TabIndex = 3;
            HideExitButtonCheckBox.UseVisualStyleBackColor = true;
            // 
            // HideFMListZoomButtonsCheckBox
            // 
            HideFMListZoomButtonsCheckBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            HideFMListZoomButtonsCheckBox.Location = new Point(16, 44);
            HideFMListZoomButtonsCheckBox.Size = new Size(400, 32);
            HideFMListZoomButtonsCheckBox.TabIndex = 2;
            HideFMListZoomButtonsCheckBox.UseVisualStyleBackColor = true;
            // 
            // HideUninstallButtonCheckBox
            // 
            HideUninstallButtonCheckBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            HideUninstallButtonCheckBox.Location = new Point(16, 16);
            HideUninstallButtonCheckBox.Size = new Size(400, 32);
            HideUninstallButtonCheckBox.TabIndex = 1;
            HideUninstallButtonCheckBox.UseVisualStyleBackColor = true;
            // 
            // PlayFMOnDCOrEnterGroupBox
            // 
            PlayFMOnDCOrEnterGroupBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            PlayFMOnDCOrEnterGroupBox.Controls.Add(ConfirmPlayOnDCOrEnterCheckBox);
            PlayFMOnDCOrEnterGroupBox.Location = new Point(8, 456);
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
            // LanguageGroupBox
            // 
            LanguageGroupBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            LanguageGroupBox.Controls.Add(LanguageComboBox);
            LanguageGroupBox.Location = new Point(8, 8);
            LanguageGroupBox.MinimumSize = new Size(424, 0);
            LanguageGroupBox.Size = new Size(424, 60);
            LanguageGroupBox.TabIndex = 0;
            LanguageGroupBox.TabStop = false;
            // 
            // LanguageComboBox
            // 
            LanguageComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            LanguageComboBox.FormattingEnabled = true;
            LanguageComboBox.Location = new Point(16, 24);
            LanguageComboBox.Size = new Size(184, 21);
            LanguageComboBox.TabIndex = 0;
            //
            // WebSearchGroupBox
            // 
            WebSearchGroupBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            WebSearchGroupBox.Controls.Add(WebSearchUrlResetButton);
            WebSearchGroupBox.Controls.Add(WebSearchTitleExplanationLabel);
            WebSearchGroupBox.Controls.Add(WebSearchUrlTextBox);
            WebSearchGroupBox.Controls.Add(WebSearchUrlLabel);
            WebSearchGroupBox.Location = new Point(8, 336);
            WebSearchGroupBox.MinimumSize = new Size(424, 0);
            WebSearchGroupBox.Size = new Size(424, 108);
            WebSearchGroupBox.TabIndex = 3;
            WebSearchGroupBox.TabStop = false;
            // 
            // WebSearchUrlResetButton
            // 
            WebSearchUrlResetButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            WebSearchUrlResetButton.BackgroundImage = Images.Refresh;
            WebSearchUrlResetButton.BackgroundImageLayout = ImageLayout.Zoom;
            WebSearchUrlResetButton.FlatStyle = FlatStyle.Flat;
            WebSearchUrlResetButton.Location = new Point(394, 48);
            WebSearchUrlResetButton.Size = new Size(20, 20);
            WebSearchUrlResetButton.TabIndex = 2;
            WebSearchUrlResetButton.UseVisualStyleBackColor = true;
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
            UninstallingFMsGroupBox.Location = new Point(8, 176);
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
            FMFileConversionGroupBox.Location = new Point(8, 80);
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
            DummyAutoScrollPanel.Location = new Point(8, 120);
            DummyAutoScrollPanel.Size = new Size(424, 8);
            DummyAutoScrollPanel.TabIndex = 12;
            // 
            // OtherPage
            // 
            AutoScaleDimensions = new SizeF(6F, 13F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(PagePanel);
            Size = new Size(440, 713);
            PagePanel.ResumeLayout(false);
            ReadmeGroupBox.ResumeLayout(false);
            ShowOrHideUIElementsGroupBox.ResumeLayout(false);
            PlayFMOnDCOrEnterGroupBox.ResumeLayout(false);
            PlayFMOnDCOrEnterGroupBox.PerformLayout();
            LanguageGroupBox.ResumeLayout(false);
            WebSearchGroupBox.ResumeLayout(false);
            WebSearchGroupBox.PerformLayout();
            UninstallingFMsGroupBox.ResumeLayout(false);
            UninstallingFMsGroupBox.PerformLayout();
            FMFileConversionGroupBox.ResumeLayout(false);
            ResumeLayout(false);
        }
    }
}
