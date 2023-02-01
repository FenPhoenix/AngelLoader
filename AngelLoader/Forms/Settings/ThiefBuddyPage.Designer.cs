﻿#define FenGen_DesignerSource

namespace AngelLoader.Forms;

partial class ThiefBuddyPage
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
            this.DummyAutoScrollPanel = new System.Windows.Forms.Control();
            this.TBHelpPictureBox = new System.Windows.Forms.PictureBox();
            this.ThiefBuddyOptionsGroupBox = new AngelLoader.Forms.CustomControls.DarkGroupBox();
            this.HorizDiv1 = new AngelLoader.Forms.CustomControls.DarkHorizontalDivider();
            this.TBInstallStatusLabel = new AngelLoader.Forms.CustomControls.DarkLabel();
            this.RunTBPanel = new System.Windows.Forms.Panel();
            this.RunThiefBuddyWhenPlayingFMsLabel = new AngelLoader.Forms.CustomControls.DarkLabel();
            this.RunTBAlwaysRadioButton = new AngelLoader.Forms.CustomControls.DarkRadioButton();
            this.RunTBNeverRadioButton = new AngelLoader.Forms.CustomControls.DarkRadioButton();
            this.RunTBAskRadioButton = new AngelLoader.Forms.CustomControls.DarkRadioButton();
            this.GetTBLinkLabel = new AngelLoader.Forms.CustomControls.DarkLinkLabel();
            this.WhatIsTBHelpLabel = new AngelLoader.Forms.CustomControls.DarkLabel();
            this.PagePanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.TBHelpPictureBox)).BeginInit();
            this.ThiefBuddyOptionsGroupBox.SuspendLayout();
            this.RunTBPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // PagePanel
            // 
            this.PagePanel.AutoScroll = true;
            this.PagePanel.Controls.Add(this.ThiefBuddyOptionsGroupBox);
            this.PagePanel.Controls.Add(this.GetTBLinkLabel);
            this.PagePanel.Controls.Add(this.DummyAutoScrollPanel);
            this.PagePanel.Controls.Add(this.TBHelpPictureBox);
            this.PagePanel.Controls.Add(this.WhatIsTBHelpLabel);
            this.PagePanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PagePanel.Location = new System.Drawing.Point(0, 0);
            this.PagePanel.Name = "PagePanel";
            this.PagePanel.Size = new System.Drawing.Size(440, 692);
            this.PagePanel.TabIndex = 1;
            // 
            // DummyAutoScrollPanel
            // 
            this.DummyAutoScrollPanel.Location = new System.Drawing.Point(8, 48);
            this.DummyAutoScrollPanel.Name = "DummyAutoScrollPanel";
            this.DummyAutoScrollPanel.Size = new System.Drawing.Size(424, 8);
            this.DummyAutoScrollPanel.TabIndex = 12;
            // 
            // TBHelpPictureBox
            // 
            this.TBHelpPictureBox.Image = global::AngelLoader.Properties.Resources.Help;
            this.TBHelpPictureBox.Location = new System.Drawing.Point(8, 188);
            this.TBHelpPictureBox.Name = "TBHelpPictureBox";
            this.TBHelpPictureBox.Size = new System.Drawing.Size(16, 16);
            this.TBHelpPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.TBHelpPictureBox.TabIndex = 9;
            this.TBHelpPictureBox.TabStop = false;
            // 
            // ThiefBuddyOptionsGroupBox
            // 
            this.ThiefBuddyOptionsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ThiefBuddyOptionsGroupBox.Controls.Add(this.HorizDiv1);
            this.ThiefBuddyOptionsGroupBox.Controls.Add(this.TBInstallStatusLabel);
            this.ThiefBuddyOptionsGroupBox.Controls.Add(this.RunTBPanel);
            this.ThiefBuddyOptionsGroupBox.Location = new System.Drawing.Point(8, 8);
            this.ThiefBuddyOptionsGroupBox.MinimumSize = new System.Drawing.Size(424, 0);
            this.ThiefBuddyOptionsGroupBox.Name = "ThiefBuddyOptionsGroupBox";
            this.ThiefBuddyOptionsGroupBox.Size = new System.Drawing.Size(424, 168);
            this.ThiefBuddyOptionsGroupBox.TabIndex = 13;
            this.ThiefBuddyOptionsGroupBox.TabStop = false;
            this.ThiefBuddyOptionsGroupBox.Text = "Thief Buddy options";
            // 
            // HorizDiv1
            // 
            this.HorizDiv1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.HorizDiv1.Location = new System.Drawing.Point(16, 40);
            this.HorizDiv1.Name = "HorizDiv1";
            this.HorizDiv1.Size = new System.Drawing.Size(392, 16);
            this.HorizDiv1.TabIndex = 7;
            // 
            // TBInstallStatusLabel
            // 
            this.TBInstallStatusLabel.AutoSize = true;
            this.TBInstallStatusLabel.Location = new System.Drawing.Point(16, 24);
            this.TBInstallStatusLabel.Name = "TBInstallStatusLabel";
            this.TBInstallStatusLabel.Size = new System.Drawing.Size(136, 13);
            this.TBInstallStatusLabel.TabIndex = 6;
            this.TBInstallStatusLabel.Text = "Thief Buddy is not installed.";
            // 
            // RunTBPanel
            // 
            this.RunTBPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.RunTBPanel.Controls.Add(this.RunThiefBuddyWhenPlayingFMsLabel);
            this.RunTBPanel.Controls.Add(this.RunTBAlwaysRadioButton);
            this.RunTBPanel.Controls.Add(this.RunTBNeverRadioButton);
            this.RunTBPanel.Controls.Add(this.RunTBAskRadioButton);
            this.RunTBPanel.Location = new System.Drawing.Point(16, 60);
            this.RunTBPanel.Name = "RunTBPanel";
            this.RunTBPanel.Size = new System.Drawing.Size(392, 100);
            this.RunTBPanel.TabIndex = 5;
            // 
            // RunThiefBuddyWhenPlayingFMsLabel
            // 
            this.RunThiefBuddyWhenPlayingFMsLabel.AutoSize = true;
            this.RunThiefBuddyWhenPlayingFMsLabel.Location = new System.Drawing.Point(0, 0);
            this.RunThiefBuddyWhenPlayingFMsLabel.Name = "RunThiefBuddyWhenPlayingFMsLabel";
            this.RunThiefBuddyWhenPlayingFMsLabel.Size = new System.Drawing.Size(178, 13);
            this.RunThiefBuddyWhenPlayingFMsLabel.TabIndex = 5;
            this.RunThiefBuddyWhenPlayingFMsLabel.Text = "Run Thief Buddy when playing FMs:";
            // 
            // RunTBAlwaysRadioButton
            // 
            this.RunTBAlwaysRadioButton.AutoSize = true;
            this.RunTBAlwaysRadioButton.Location = new System.Drawing.Point(8, 24);
            this.RunTBAlwaysRadioButton.Name = "RunTBAlwaysRadioButton";
            this.RunTBAlwaysRadioButton.Size = new System.Drawing.Size(58, 17);
            this.RunTBAlwaysRadioButton.TabIndex = 4;
            this.RunTBAlwaysRadioButton.Text = "Always";
            this.RunTBAlwaysRadioButton.UseVisualStyleBackColor = true;
            // 
            // RunTBNeverRadioButton
            // 
            this.RunTBNeverRadioButton.AutoSize = true;
            this.RunTBNeverRadioButton.Location = new System.Drawing.Point(8, 72);
            this.RunTBNeverRadioButton.Name = "RunTBNeverRadioButton";
            this.RunTBNeverRadioButton.Size = new System.Drawing.Size(54, 17);
            this.RunTBNeverRadioButton.TabIndex = 4;
            this.RunTBNeverRadioButton.Text = "Never";
            this.RunTBNeverRadioButton.UseVisualStyleBackColor = true;
            // 
            // RunTBAskRadioButton
            // 
            this.RunTBAskRadioButton.AutoSize = true;
            this.RunTBAskRadioButton.Checked = true;
            this.RunTBAskRadioButton.Location = new System.Drawing.Point(8, 48);
            this.RunTBAskRadioButton.Name = "RunTBAskRadioButton";
            this.RunTBAskRadioButton.Size = new System.Drawing.Size(94, 17);
            this.RunTBAskRadioButton.TabIndex = 4;
            this.RunTBAskRadioButton.TabStop = true;
            this.RunTBAskRadioButton.Text = "Ask every time";
            this.RunTBAskRadioButton.UseVisualStyleBackColor = true;
            // 
            // GetTBLinkLabel
            // 
            this.GetTBLinkLabel.AutoSize = true;
            this.GetTBLinkLabel.Location = new System.Drawing.Point(27, 208);
            this.GetTBLinkLabel.Name = "GetTBLinkLabel";
            this.GetTBLinkLabel.Size = new System.Drawing.Size(84, 13);
            this.GetTBLinkLabel.TabIndex = 10;
            this.GetTBLinkLabel.TabStop = true;
            this.GetTBLinkLabel.Text = "Get Thief Buddy";
            // 
            // WhatIsTBHelpLabel
            // 
            this.WhatIsTBHelpLabel.AutoSize = true;
            this.WhatIsTBHelpLabel.Location = new System.Drawing.Point(27, 188);
            this.WhatIsTBHelpLabel.Name = "WhatIsTBHelpLabel";
            this.WhatIsTBHelpLabel.Size = new System.Drawing.Size(229, 13);
            this.WhatIsTBHelpLabel.TabIndex = 8;
            this.WhatIsTBHelpLabel.Text = "Thief Buddy is a quicksave backup helper tool.";
            // 
            // ThiefBuddyPage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.PagePanel);
            this.Name = "ThiefBuddyPage";
            this.Size = new System.Drawing.Size(440, 692);
            this.PagePanel.ResumeLayout(false);
            this.PagePanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.TBHelpPictureBox)).EndInit();
            this.ThiefBuddyOptionsGroupBox.ResumeLayout(false);
            this.ThiefBuddyOptionsGroupBox.PerformLayout();
            this.RunTBPanel.ResumeLayout(false);
            this.RunTBPanel.PerformLayout();
            this.ResumeLayout(false);

    }
#endif

    #endregion

    internal System.Windows.Forms.Panel PagePanel;
    internal System.Windows.Forms.Control DummyAutoScrollPanel;
    internal CustomControls.DarkGroupBox ThiefBuddyOptionsGroupBox;
    internal CustomControls.DarkRadioButton RunTBNeverRadioButton;
    internal CustomControls.DarkRadioButton RunTBAskRadioButton;
    internal CustomControls.DarkRadioButton RunTBAlwaysRadioButton;
    internal System.Windows.Forms.Panel RunTBPanel;
    internal CustomControls.DarkLabel RunThiefBuddyWhenPlayingFMsLabel;
    internal System.Windows.Forms.PictureBox TBHelpPictureBox;
    internal CustomControls.DarkLabel WhatIsTBHelpLabel;
    internal CustomControls.DarkLinkLabel GetTBLinkLabel;
    internal CustomControls.DarkLabel TBInstallStatusLabel;
    private CustomControls.DarkHorizontalDivider HorizDiv1;
}
