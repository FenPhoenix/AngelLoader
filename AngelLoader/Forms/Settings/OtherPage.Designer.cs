#define FenGen_DesignerSource

namespace AngelLoader.Forms;

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
            this.PagePanel = new AngelLoader.Forms.CustomControls.PanelCustom();
            this.TagsGroupBox = new AngelLoader.Forms.CustomControls.DarkGroupBox();
            this.AlwaysShowPresetTagsCheckBox = new AngelLoader.Forms.CustomControls.DarkCheckBox();
            this.FilteringGroupBox = new AngelLoader.Forms.CustomControls.DarkGroupBox();
            this.EnableFuzzySearchCheckBox = new AngelLoader.Forms.CustomControls.DarkCheckBox();
            this.InstallingFMsGroupBox = new AngelLoader.Forms.CustomControls.DarkGroupBox();
            this.Install_ConfirmNeverRadioButton = new AngelLoader.Forms.CustomControls.DarkRadioButton();
            this.Install_ConfirmMultipleOnlyRadioButton = new AngelLoader.Forms.CustomControls.DarkRadioButton();
            this.Install_ConfirmAlwaysRadioButton = new AngelLoader.Forms.CustomControls.DarkRadioButton();
            this.ConfirmBeforeInstallLabel = new AngelLoader.Forms.CustomControls.DarkLabel();
            this.PlayFMOnDCOrEnterGroupBox = new AngelLoader.Forms.CustomControls.DarkGroupBox();
            this.ConfirmPlayOnDCOrEnterCheckBox = new AngelLoader.Forms.CustomControls.DarkCheckBox();
            this.WebSearchGroupBox = new AngelLoader.Forms.CustomControls.DarkGroupBox();
            this.TDMWebSearchUrlLabel = new AngelLoader.Forms.CustomControls.DarkLabel();
            this.SS2WebSearchUrlLabel = new AngelLoader.Forms.CustomControls.DarkLabel();
            this.T3WebSearchUrlLabel = new AngelLoader.Forms.CustomControls.DarkLabel();
            this.T2WebSearchUrlLabel = new AngelLoader.Forms.CustomControls.DarkLabel();
            this.T1WebSearchUrlLabel = new AngelLoader.Forms.CustomControls.DarkLabel();
            this.TDMWebSearchUrlResetButton = new AngelLoader.Forms.CustomControls.DarkButton();
            this.SS2WebSearchUrlResetButton = new AngelLoader.Forms.CustomControls.DarkButton();
            this.T3WebSearchUrlResetButton = new AngelLoader.Forms.CustomControls.DarkButton();
            this.T2WebSearchUrlResetButton = new AngelLoader.Forms.CustomControls.DarkButton();
            this.T1WebSearchUrlResetButton = new AngelLoader.Forms.CustomControls.DarkButton();
            this.WebSearchTitleExplanationLabel = new AngelLoader.Forms.CustomControls.DarkLabel();
            this.TDMWebSearchUrlTextBox = new AngelLoader.Forms.CustomControls.DarkTextBox();
            this.SS2WebSearchUrlTextBox = new AngelLoader.Forms.CustomControls.DarkTextBox();
            this.T3WebSearchUrlTextBox = new AngelLoader.Forms.CustomControls.DarkTextBox();
            this.T2WebSearchUrlTextBox = new AngelLoader.Forms.CustomControls.DarkTextBox();
            this.T1WebSearchUrlTextBox = new AngelLoader.Forms.CustomControls.DarkTextBox();
            this.WebSearchUrlLabel = new AngelLoader.Forms.CustomControls.DarkLabel();
            this.UninstallingFMsGroupBox = new AngelLoader.Forms.CustomControls.DarkGroupBox();
            this.ConfirmUninstallCheckBox = new AngelLoader.Forms.CustomControls.DarkCheckBox();
            this.WhatToBackUpLabel = new AngelLoader.Forms.CustomControls.DarkLabel();
            this.BackupAlwaysAskCheckBox = new AngelLoader.Forms.CustomControls.DarkCheckBox();
            this.BackupAllChangedDataRadioButton = new AngelLoader.Forms.CustomControls.DarkRadioButton();
            this.BackupSavesAndScreensOnlyRadioButton = new AngelLoader.Forms.CustomControls.DarkRadioButton();
            this.FMSettingsGroupBox = new AngelLoader.Forms.CustomControls.DarkGroupBox();
            this.OldMantleForOldDarkFMsCheckBox = new AngelLoader.Forms.CustomControls.DarkCheckBox();
            this.PagePanel.SuspendLayout();
            this.TagsGroupBox.SuspendLayout();
            this.FilteringGroupBox.SuspendLayout();
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
            this.PagePanel.AutoScrollMinSize = new System.Drawing.Size(432, 0);
            this.PagePanel.Controls.Add(this.TagsGroupBox);
            this.PagePanel.Controls.Add(this.FilteringGroupBox);
            this.PagePanel.Controls.Add(this.InstallingFMsGroupBox);
            this.PagePanel.Controls.Add(this.PlayFMOnDCOrEnterGroupBox);
            this.PagePanel.Controls.Add(this.WebSearchGroupBox);
            this.PagePanel.Controls.Add(this.UninstallingFMsGroupBox);
            this.PagePanel.Controls.Add(this.FMSettingsGroupBox);
            this.PagePanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PagePanel.Location = new System.Drawing.Point(0, 0);
            this.PagePanel.Name = "PagePanel";
            this.PagePanel.Size = new System.Drawing.Size(440, 985);
            this.PagePanel.TabIndex = 0;
            // 
            // TagsGroupBox
            // 
            this.TagsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.TagsGroupBox.Controls.Add(this.AlwaysShowPresetTagsCheckBox);
            this.TagsGroupBox.Location = new System.Drawing.Point(8, 812);
            this.TagsGroupBox.MinimumSize = new System.Drawing.Size(424, 0);
            this.TagsGroupBox.Name = "TagsGroupBox";
            this.TagsGroupBox.Size = new System.Drawing.Size(424, 56);
            this.TagsGroupBox.TabIndex = 6;
            this.TagsGroupBox.TabStop = false;
            this.TagsGroupBox.Text = "Tags";
            // 
            // AlwaysShowPresetTagsCheckBox
            // 
            this.AlwaysShowPresetTagsCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.AlwaysShowPresetTagsCheckBox.Checked = true;
            this.AlwaysShowPresetTagsCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.AlwaysShowPresetTagsCheckBox.Location = new System.Drawing.Point(16, 16);
            this.AlwaysShowPresetTagsCheckBox.Name = "AlwaysShowPresetTagsCheckBox";
            this.AlwaysShowPresetTagsCheckBox.Size = new System.Drawing.Size(400, 32);
            this.AlwaysShowPresetTagsCheckBox.TabIndex = 0;
            this.AlwaysShowPresetTagsCheckBox.Text = "Always show preset tags";
            // 
            // FilteringGroupBox
            // 
            this.FilteringGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.FilteringGroupBox.Controls.Add(this.EnableFuzzySearchCheckBox);
            this.FilteringGroupBox.Location = new System.Drawing.Point(8, 744);
            this.FilteringGroupBox.MinimumSize = new System.Drawing.Size(424, 0);
            this.FilteringGroupBox.Name = "FilteringGroupBox";
            this.FilteringGroupBox.Size = new System.Drawing.Size(424, 56);
            this.FilteringGroupBox.TabIndex = 5;
            this.FilteringGroupBox.TabStop = false;
            this.FilteringGroupBox.Text = "Filtering";
            // 
            // EnableFuzzySearchCheckBox
            // 
            this.EnableFuzzySearchCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.EnableFuzzySearchCheckBox.Location = new System.Drawing.Point(16, 16);
            this.EnableFuzzySearchCheckBox.Name = "EnableFuzzySearchCheckBox";
            this.EnableFuzzySearchCheckBox.Size = new System.Drawing.Size(400, 32);
            this.EnableFuzzySearchCheckBox.TabIndex = 0;
            this.EnableFuzzySearchCheckBox.Text = "Enable fuzzy search for title and author";
            // 
            // InstallingFMsGroupBox
            // 
            this.InstallingFMsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.InstallingFMsGroupBox.Controls.Add(this.Install_ConfirmNeverRadioButton);
            this.InstallingFMsGroupBox.Controls.Add(this.Install_ConfirmMultipleOnlyRadioButton);
            this.InstallingFMsGroupBox.Controls.Add(this.Install_ConfirmAlwaysRadioButton);
            this.InstallingFMsGroupBox.Controls.Add(this.ConfirmBeforeInstallLabel);
            this.InstallingFMsGroupBox.Location = new System.Drawing.Point(8, 76);
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
            // 
            // Install_ConfirmAlwaysRadioButton
            // 
            this.Install_ConfirmAlwaysRadioButton.AutoSize = true;
            this.Install_ConfirmAlwaysRadioButton.Location = new System.Drawing.Point(24, 48);
            this.Install_ConfirmAlwaysRadioButton.Name = "Install_ConfirmAlwaysRadioButton";
            this.Install_ConfirmAlwaysRadioButton.Size = new System.Drawing.Size(58, 17);
            this.Install_ConfirmAlwaysRadioButton.TabIndex = 1;
            this.Install_ConfirmAlwaysRadioButton.Text = "Always";
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
            this.PlayFMOnDCOrEnterGroupBox.Location = new System.Drawing.Point(8, 676);
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
            // 
            // WebSearchGroupBox
            // 
            this.WebSearchGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.WebSearchGroupBox.Controls.Add(this.TDMWebSearchUrlLabel);
            this.WebSearchGroupBox.Controls.Add(this.SS2WebSearchUrlLabel);
            this.WebSearchGroupBox.Controls.Add(this.T3WebSearchUrlLabel);
            this.WebSearchGroupBox.Controls.Add(this.T2WebSearchUrlLabel);
            this.WebSearchGroupBox.Controls.Add(this.T1WebSearchUrlLabel);
            this.WebSearchGroupBox.Controls.Add(this.TDMWebSearchUrlResetButton);
            this.WebSearchGroupBox.Controls.Add(this.SS2WebSearchUrlResetButton);
            this.WebSearchGroupBox.Controls.Add(this.T3WebSearchUrlResetButton);
            this.WebSearchGroupBox.Controls.Add(this.T2WebSearchUrlResetButton);
            this.WebSearchGroupBox.Controls.Add(this.T1WebSearchUrlResetButton);
            this.WebSearchGroupBox.Controls.Add(this.WebSearchTitleExplanationLabel);
            this.WebSearchGroupBox.Controls.Add(this.TDMWebSearchUrlTextBox);
            this.WebSearchGroupBox.Controls.Add(this.SS2WebSearchUrlTextBox);
            this.WebSearchGroupBox.Controls.Add(this.T3WebSearchUrlTextBox);
            this.WebSearchGroupBox.Controls.Add(this.T2WebSearchUrlTextBox);
            this.WebSearchGroupBox.Controls.Add(this.T1WebSearchUrlTextBox);
            this.WebSearchGroupBox.Controls.Add(this.WebSearchUrlLabel);
            this.WebSearchGroupBox.Location = new System.Drawing.Point(8, 372);
            this.WebSearchGroupBox.MinimumSize = new System.Drawing.Size(424, 0);
            this.WebSearchGroupBox.Name = "WebSearchGroupBox";
            this.WebSearchGroupBox.Size = new System.Drawing.Size(424, 292);
            this.WebSearchGroupBox.TabIndex = 3;
            this.WebSearchGroupBox.TabStop = false;
            this.WebSearchGroupBox.Text = "Web search";
            // 
            // TDMWebSearchUrlLabel
            // 
            this.TDMWebSearchUrlLabel.AutoSize = true;
            this.TDMWebSearchUrlLabel.Location = new System.Drawing.Point(16, 216);
            this.TDMWebSearchUrlLabel.Name = "TDMWebSearchUrlLabel";
            this.TDMWebSearchUrlLabel.Size = new System.Drawing.Size(79, 13);
            this.TDMWebSearchUrlLabel.TabIndex = 13;
            this.TDMWebSearchUrlLabel.Text = "The Dark Mod:";
            // 
            // SS2WebSearchUrlLabel
            // 
            this.SS2WebSearchUrlLabel.AutoSize = true;
            this.SS2WebSearchUrlLabel.Location = new System.Drawing.Point(16, 176);
            this.SS2WebSearchUrlLabel.Name = "SS2WebSearchUrlLabel";
            this.SS2WebSearchUrlLabel.Size = new System.Drawing.Size(87, 13);
            this.SS2WebSearchUrlLabel.TabIndex = 10;
            this.SS2WebSearchUrlLabel.Text = "System Shock 2:";
            // 
            // T3WebSearchUrlLabel
            // 
            this.T3WebSearchUrlLabel.AutoSize = true;
            this.T3WebSearchUrlLabel.Location = new System.Drawing.Point(16, 136);
            this.T3WebSearchUrlLabel.Name = "T3WebSearchUrlLabel";
            this.T3WebSearchUrlLabel.Size = new System.Drawing.Size(43, 13);
            this.T3WebSearchUrlLabel.TabIndex = 7;
            this.T3WebSearchUrlLabel.Text = "Thief 3:";
            // 
            // T2WebSearchUrlLabel
            // 
            this.T2WebSearchUrlLabel.AutoSize = true;
            this.T2WebSearchUrlLabel.Location = new System.Drawing.Point(16, 96);
            this.T2WebSearchUrlLabel.Name = "T2WebSearchUrlLabel";
            this.T2WebSearchUrlLabel.Size = new System.Drawing.Size(43, 13);
            this.T2WebSearchUrlLabel.TabIndex = 4;
            this.T2WebSearchUrlLabel.Text = "Thief 2:";
            // 
            // T1WebSearchUrlLabel
            // 
            this.T1WebSearchUrlLabel.AutoSize = true;
            this.T1WebSearchUrlLabel.Location = new System.Drawing.Point(16, 56);
            this.T1WebSearchUrlLabel.Name = "T1WebSearchUrlLabel";
            this.T1WebSearchUrlLabel.Size = new System.Drawing.Size(43, 13);
            this.T1WebSearchUrlLabel.TabIndex = 1;
            this.T1WebSearchUrlLabel.Text = "Thief 1:";
            // 
            // TDMWebSearchUrlResetButton
            // 
            this.TDMWebSearchUrlResetButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.TDMWebSearchUrlResetButton.Location = new System.Drawing.Point(393, 231);
            this.TDMWebSearchUrlResetButton.Name = "TDMWebSearchUrlResetButton";
            this.TDMWebSearchUrlResetButton.Size = new System.Drawing.Size(22, 22);
            this.TDMWebSearchUrlResetButton.TabIndex = 15;
            this.TDMWebSearchUrlResetButton.PaintCustom += new System.EventHandler<System.Windows.Forms.PaintEventArgs>(this.WebSearchUrlResetButton_Paint);
            // 
            // SS2WebSearchUrlResetButton
            // 
            this.SS2WebSearchUrlResetButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.SS2WebSearchUrlResetButton.Location = new System.Drawing.Point(393, 191);
            this.SS2WebSearchUrlResetButton.Name = "SS2WebSearchUrlResetButton";
            this.SS2WebSearchUrlResetButton.Size = new System.Drawing.Size(22, 22);
            this.SS2WebSearchUrlResetButton.TabIndex = 12;
            this.SS2WebSearchUrlResetButton.PaintCustom += new System.EventHandler<System.Windows.Forms.PaintEventArgs>(this.WebSearchUrlResetButton_Paint);
            // 
            // T3WebSearchUrlResetButton
            // 
            this.T3WebSearchUrlResetButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.T3WebSearchUrlResetButton.Location = new System.Drawing.Point(393, 151);
            this.T3WebSearchUrlResetButton.Name = "T3WebSearchUrlResetButton";
            this.T3WebSearchUrlResetButton.Size = new System.Drawing.Size(22, 22);
            this.T3WebSearchUrlResetButton.TabIndex = 9;
            this.T3WebSearchUrlResetButton.PaintCustom += new System.EventHandler<System.Windows.Forms.PaintEventArgs>(this.WebSearchUrlResetButton_Paint);
            // 
            // T2WebSearchUrlResetButton
            // 
            this.T2WebSearchUrlResetButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.T2WebSearchUrlResetButton.Location = new System.Drawing.Point(393, 111);
            this.T2WebSearchUrlResetButton.Name = "T2WebSearchUrlResetButton";
            this.T2WebSearchUrlResetButton.Size = new System.Drawing.Size(22, 22);
            this.T2WebSearchUrlResetButton.TabIndex = 6;
            this.T2WebSearchUrlResetButton.PaintCustom += new System.EventHandler<System.Windows.Forms.PaintEventArgs>(this.WebSearchUrlResetButton_Paint);
            // 
            // T1WebSearchUrlResetButton
            // 
            this.T1WebSearchUrlResetButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.T1WebSearchUrlResetButton.Location = new System.Drawing.Point(393, 71);
            this.T1WebSearchUrlResetButton.Name = "T1WebSearchUrlResetButton";
            this.T1WebSearchUrlResetButton.Size = new System.Drawing.Size(22, 22);
            this.T1WebSearchUrlResetButton.TabIndex = 3;
            this.T1WebSearchUrlResetButton.PaintCustom += new System.EventHandler<System.Windows.Forms.PaintEventArgs>(this.WebSearchUrlResetButton_Paint);
            // 
            // WebSearchTitleExplanationLabel
            // 
            this.WebSearchTitleExplanationLabel.AutoSize = true;
            this.WebSearchTitleExplanationLabel.Location = new System.Drawing.Point(16, 264);
            this.WebSearchTitleExplanationLabel.Name = "WebSearchTitleExplanationLabel";
            this.WebSearchTitleExplanationLabel.Size = new System.Drawing.Size(140, 13);
            this.WebSearchTitleExplanationLabel.TabIndex = 16;
            this.WebSearchTitleExplanationLabel.Text = "$TITLE$ : the title of the FM";
            // 
            // TDMWebSearchUrlTextBox
            // 
            this.TDMWebSearchUrlTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.TDMWebSearchUrlTextBox.Location = new System.Drawing.Point(16, 232);
            this.TDMWebSearchUrlTextBox.Name = "TDMWebSearchUrlTextBox";
            this.TDMWebSearchUrlTextBox.Size = new System.Drawing.Size(376, 20);
            this.TDMWebSearchUrlTextBox.TabIndex = 14;
            // 
            // SS2WebSearchUrlTextBox
            // 
            this.SS2WebSearchUrlTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.SS2WebSearchUrlTextBox.Location = new System.Drawing.Point(16, 192);
            this.SS2WebSearchUrlTextBox.Name = "SS2WebSearchUrlTextBox";
            this.SS2WebSearchUrlTextBox.Size = new System.Drawing.Size(376, 20);
            this.SS2WebSearchUrlTextBox.TabIndex = 11;
            // 
            // T3WebSearchUrlTextBox
            // 
            this.T3WebSearchUrlTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.T3WebSearchUrlTextBox.Location = new System.Drawing.Point(16, 152);
            this.T3WebSearchUrlTextBox.Name = "T3WebSearchUrlTextBox";
            this.T3WebSearchUrlTextBox.Size = new System.Drawing.Size(376, 20);
            this.T3WebSearchUrlTextBox.TabIndex = 8;
            // 
            // T2WebSearchUrlTextBox
            // 
            this.T2WebSearchUrlTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.T2WebSearchUrlTextBox.Location = new System.Drawing.Point(16, 112);
            this.T2WebSearchUrlTextBox.Name = "T2WebSearchUrlTextBox";
            this.T2WebSearchUrlTextBox.Size = new System.Drawing.Size(376, 20);
            this.T2WebSearchUrlTextBox.TabIndex = 5;
            // 
            // T1WebSearchUrlTextBox
            // 
            this.T1WebSearchUrlTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.T1WebSearchUrlTextBox.Location = new System.Drawing.Point(16, 72);
            this.T1WebSearchUrlTextBox.Name = "T1WebSearchUrlTextBox";
            this.T1WebSearchUrlTextBox.Size = new System.Drawing.Size(376, 20);
            this.T1WebSearchUrlTextBox.TabIndex = 2;
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
            this.UninstallingFMsGroupBox.Location = new System.Drawing.Point(8, 212);
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
            // 
            // BackupSavesAndScreensOnlyRadioButton
            // 
            this.BackupSavesAndScreensOnlyRadioButton.AutoSize = true;
            this.BackupSavesAndScreensOnlyRadioButton.Location = new System.Drawing.Point(24, 70);
            this.BackupSavesAndScreensOnlyRadioButton.Name = "BackupSavesAndScreensOnlyRadioButton";
            this.BackupSavesAndScreensOnlyRadioButton.Size = new System.Drawing.Size(158, 17);
            this.BackupSavesAndScreensOnlyRadioButton.TabIndex = 2;
            this.BackupSavesAndScreensOnlyRadioButton.Text = "Saves and screenshots only";
            // 
            // FMSettingsGroupBox
            // 
            this.FMSettingsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.FMSettingsGroupBox.Controls.Add(this.OldMantleForOldDarkFMsCheckBox);
            this.FMSettingsGroupBox.Location = new System.Drawing.Point(8, 8);
            this.FMSettingsGroupBox.MinimumSize = new System.Drawing.Size(424, 0);
            this.FMSettingsGroupBox.Name = "FMSettingsGroupBox";
            this.FMSettingsGroupBox.Size = new System.Drawing.Size(424, 56);
            this.FMSettingsGroupBox.TabIndex = 0;
            this.FMSettingsGroupBox.TabStop = false;
            this.FMSettingsGroupBox.Text = "FM settings";
            // 
            // OldMantleForOldDarkFMsCheckBox
            // 
            this.OldMantleForOldDarkFMsCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.OldMantleForOldDarkFMsCheckBox.Location = new System.Drawing.Point(16, 16);
            this.OldMantleForOldDarkFMsCheckBox.Name = "OldMantleForOldDarkFMsCheckBox";
            this.OldMantleForOldDarkFMsCheckBox.Size = new System.Drawing.Size(400, 32);
            this.OldMantleForOldDarkFMsCheckBox.TabIndex = 2;
            this.OldMantleForOldDarkFMsCheckBox.Text = "Use old mantling for OldDark FMs";
            // 
            // OtherPage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.PagePanel);
            this.Name = "OtherPage";
            this.Size = new System.Drawing.Size(440, 985);
            this.PagePanel.ResumeLayout(false);
            this.TagsGroupBox.ResumeLayout(false);
            this.FilteringGroupBox.ResumeLayout(false);
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

    internal AngelLoader.Forms.CustomControls.PanelCustom PagePanel;
    internal AngelLoader.Forms.CustomControls.DarkGroupBox PlayFMOnDCOrEnterGroupBox;
    internal AngelLoader.Forms.CustomControls.DarkCheckBox ConfirmPlayOnDCOrEnterCheckBox;
    internal AngelLoader.Forms.CustomControls.DarkGroupBox WebSearchGroupBox;
    internal AngelLoader.Forms.CustomControls.DarkButton T1WebSearchUrlResetButton;
    internal AngelLoader.Forms.CustomControls.DarkLabel WebSearchTitleExplanationLabel;
    internal AngelLoader.Forms.CustomControls.DarkTextBox T1WebSearchUrlTextBox;
    internal AngelLoader.Forms.CustomControls.DarkLabel WebSearchUrlLabel;
    internal AngelLoader.Forms.CustomControls.DarkGroupBox UninstallingFMsGroupBox;
    internal AngelLoader.Forms.CustomControls.DarkCheckBox ConfirmUninstallCheckBox;
    internal AngelLoader.Forms.CustomControls.DarkLabel WhatToBackUpLabel;
    internal AngelLoader.Forms.CustomControls.DarkCheckBox BackupAlwaysAskCheckBox;
    internal AngelLoader.Forms.CustomControls.DarkRadioButton BackupAllChangedDataRadioButton;
    internal AngelLoader.Forms.CustomControls.DarkRadioButton BackupSavesAndScreensOnlyRadioButton;
    internal AngelLoader.Forms.CustomControls.DarkGroupBox FMSettingsGroupBox;
    internal CustomControls.DarkGroupBox InstallingFMsGroupBox;
    internal CustomControls.DarkRadioButton Install_ConfirmNeverRadioButton;
    internal CustomControls.DarkRadioButton Install_ConfirmMultipleOnlyRadioButton;
    internal CustomControls.DarkRadioButton Install_ConfirmAlwaysRadioButton;
    internal CustomControls.DarkLabel ConfirmBeforeInstallLabel;
    internal CustomControls.DarkCheckBox OldMantleForOldDarkFMsCheckBox;
    internal CustomControls.DarkGroupBox FilteringGroupBox;
    internal CustomControls.DarkCheckBox EnableFuzzySearchCheckBox;
    internal CustomControls.DarkButton TDMWebSearchUrlResetButton;
    internal CustomControls.DarkButton SS2WebSearchUrlResetButton;
    internal CustomControls.DarkButton T3WebSearchUrlResetButton;
    internal CustomControls.DarkButton T2WebSearchUrlResetButton;
    internal CustomControls.DarkTextBox TDMWebSearchUrlTextBox;
    internal CustomControls.DarkTextBox SS2WebSearchUrlTextBox;
    internal CustomControls.DarkTextBox T3WebSearchUrlTextBox;
    internal CustomControls.DarkTextBox T2WebSearchUrlTextBox;
    internal CustomControls.DarkLabel T1WebSearchUrlLabel;
    internal CustomControls.DarkLabel TDMWebSearchUrlLabel;
    internal CustomControls.DarkLabel SS2WebSearchUrlLabel;
    internal CustomControls.DarkLabel T3WebSearchUrlLabel;
    internal CustomControls.DarkLabel T2WebSearchUrlLabel;
    internal CustomControls.DarkGroupBox TagsGroupBox;
    internal CustomControls.DarkCheckBox AlwaysShowPresetTagsCheckBox;
}
