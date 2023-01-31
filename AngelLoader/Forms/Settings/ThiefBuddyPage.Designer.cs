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
            this.ThiefBuddyOptionsGroupBox = new AngelLoader.Forms.CustomControls.DarkGroupBox();
            this.GetTBLinkLabel = new AngelLoader.Forms.CustomControls.DarkLinkLabel();
            this.TBHelpPictureBox = new System.Windows.Forms.PictureBox();
            this.WhatIsTBHelpLabel = new AngelLoader.Forms.CustomControls.DarkLabel();
            this.AutodetectFLP = new System.Windows.Forms.FlowLayoutPanel();
            this.AutodetectButton = new AngelLoader.Forms.CustomControls.DarkButton();
            this.RunTBPanel = new System.Windows.Forms.Panel();
            this.RunThiefBuddyWhenPlayingFMsLabel = new AngelLoader.Forms.CustomControls.DarkLabel();
            this.RunTBAlwaysRadioButton = new AngelLoader.Forms.CustomControls.DarkRadioButton();
            this.RunTBNeverRadioButton = new AngelLoader.Forms.CustomControls.DarkRadioButton();
            this.RunTBAskRadioButton = new AngelLoader.Forms.CustomControls.DarkRadioButton();
            this.ThiefBuddyExeLabel = new AngelLoader.Forms.CustomControls.DarkLabel();
            this.ThiefBuddyExeBrowseButton = new AngelLoader.Forms.CustomControls.DarkButton();
            this.ThiefBuddyExeTextBox = new AngelLoader.Forms.CustomControls.DarkTextBox();
            this.PagePanel.SuspendLayout();
            this.ThiefBuddyOptionsGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.TBHelpPictureBox)).BeginInit();
            this.AutodetectFLP.SuspendLayout();
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
            // ThiefBuddyOptionsGroupBox
            // 
            this.ThiefBuddyOptionsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ThiefBuddyOptionsGroupBox.Controls.Add(this.AutodetectFLP);
            this.ThiefBuddyOptionsGroupBox.Controls.Add(this.RunTBPanel);
            this.ThiefBuddyOptionsGroupBox.Controls.Add(this.ThiefBuddyExeLabel);
            this.ThiefBuddyOptionsGroupBox.Controls.Add(this.ThiefBuddyExeBrowseButton);
            this.ThiefBuddyOptionsGroupBox.Controls.Add(this.ThiefBuddyExeTextBox);
            this.ThiefBuddyOptionsGroupBox.Location = new System.Drawing.Point(8, 8);
            this.ThiefBuddyOptionsGroupBox.MinimumSize = new System.Drawing.Size(424, 0);
            this.ThiefBuddyOptionsGroupBox.Name = "ThiefBuddyOptionsGroupBox";
            this.ThiefBuddyOptionsGroupBox.Size = new System.Drawing.Size(424, 200);
            this.ThiefBuddyOptionsGroupBox.TabIndex = 13;
            this.ThiefBuddyOptionsGroupBox.TabStop = false;
            this.ThiefBuddyOptionsGroupBox.Text = "Thief Buddy options";
            // 
            // GetTBLinkLabel
            // 
            this.GetTBLinkLabel.AutoSize = true;
            this.GetTBLinkLabel.Location = new System.Drawing.Point(27, 240);
            this.GetTBLinkLabel.Name = "GetTBLinkLabel";
            this.GetTBLinkLabel.Size = new System.Drawing.Size(84, 13);
            this.GetTBLinkLabel.TabIndex = 10;
            this.GetTBLinkLabel.TabStop = true;
            this.GetTBLinkLabel.Text = "Get Thief Buddy";
            // 
            // TBHelpPictureBox
            // 
            this.TBHelpPictureBox.Image = global::AngelLoader.Properties.Resources.Help;
            this.TBHelpPictureBox.Location = new System.Drawing.Point(8, 220);
            this.TBHelpPictureBox.Name = "TBHelpPictureBox";
            this.TBHelpPictureBox.Size = new System.Drawing.Size(16, 16);
            this.TBHelpPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.TBHelpPictureBox.TabIndex = 9;
            this.TBHelpPictureBox.TabStop = false;
            // 
            // WhatIsTBHelpLabel
            // 
            this.WhatIsTBHelpLabel.AutoSize = true;
            this.WhatIsTBHelpLabel.Location = new System.Drawing.Point(27, 220);
            this.WhatIsTBHelpLabel.Name = "WhatIsTBHelpLabel";
            this.WhatIsTBHelpLabel.Size = new System.Drawing.Size(229, 13);
            this.WhatIsTBHelpLabel.TabIndex = 8;
            this.WhatIsTBHelpLabel.Text = "Thief Buddy is a quicksave backup helper tool.";
            // 
            // AutodetectFLP
            // 
            this.AutodetectFLP.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.AutodetectFLP.Controls.Add(this.AutodetectButton);
            this.AutodetectFLP.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.AutodetectFLP.Location = new System.Drawing.Point(16, 61);
            this.AutodetectFLP.Name = "AutodetectFLP";
            this.AutodetectFLP.Size = new System.Drawing.Size(395, 24);
            this.AutodetectFLP.TabIndex = 7;
            // 
            // AutodetectButton
            // 
            this.AutodetectButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.AutodetectButton.AutoSize = true;
            this.AutodetectButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.AutodetectButton.Location = new System.Drawing.Point(320, 0);
            this.AutodetectButton.Margin = new System.Windows.Forms.Padding(0);
            this.AutodetectButton.MinimumSize = new System.Drawing.Size(75, 23);
            this.AutodetectButton.Name = "AutodetectButton";
            this.AutodetectButton.Size = new System.Drawing.Size(75, 23);
            this.AutodetectButton.TabIndex = 6;
            this.AutodetectButton.Text = "Autodetect";
            this.AutodetectButton.UseVisualStyleBackColor = true;
            // 
            // RunTBPanel
            // 
            this.RunTBPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.RunTBPanel.Controls.Add(this.RunThiefBuddyWhenPlayingFMsLabel);
            this.RunTBPanel.Controls.Add(this.RunTBAlwaysRadioButton);
            this.RunTBPanel.Controls.Add(this.RunTBNeverRadioButton);
            this.RunTBPanel.Controls.Add(this.RunTBAskRadioButton);
            this.RunTBPanel.Location = new System.Drawing.Point(16, 96);
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
            // ThiefBuddyExeLabel
            // 
            this.ThiefBuddyExeLabel.AutoSize = true;
            this.ThiefBuddyExeLabel.Location = new System.Drawing.Point(16, 24);
            this.ThiefBuddyExeLabel.Name = "ThiefBuddyExeLabel";
            this.ThiefBuddyExeLabel.Size = new System.Drawing.Size(205, 13);
            this.ThiefBuddyExeLabel.TabIndex = 3;
            this.ThiefBuddyExeLabel.Text = "Path to Thief Buddy executable (optional):";
            // 
            // ThiefBuddyExeBrowseButton
            // 
            this.ThiefBuddyExeBrowseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ThiefBuddyExeBrowseButton.AutoSize = true;
            this.ThiefBuddyExeBrowseButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ThiefBuddyExeBrowseButton.Location = new System.Drawing.Point(336, 39);
            this.ThiefBuddyExeBrowseButton.MinimumSize = new System.Drawing.Size(75, 23);
            this.ThiefBuddyExeBrowseButton.Name = "ThiefBuddyExeBrowseButton";
            this.ThiefBuddyExeBrowseButton.Padding = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.ThiefBuddyExeBrowseButton.Size = new System.Drawing.Size(75, 23);
            this.ThiefBuddyExeBrowseButton.TabIndex = 2;
            this.ThiefBuddyExeBrowseButton.Text = "Browse...";
            this.ThiefBuddyExeBrowseButton.UseVisualStyleBackColor = true;
            // 
            // ThiefBuddyExeTextBox
            // 
            this.ThiefBuddyExeTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ThiefBuddyExeTextBox.Location = new System.Drawing.Point(16, 40);
            this.ThiefBuddyExeTextBox.Name = "ThiefBuddyExeTextBox";
            this.ThiefBuddyExeTextBox.Size = new System.Drawing.Size(320, 20);
            this.ThiefBuddyExeTextBox.TabIndex = 1;
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
            this.ThiefBuddyOptionsGroupBox.ResumeLayout(false);
            this.ThiefBuddyOptionsGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.TBHelpPictureBox)).EndInit();
            this.AutodetectFLP.ResumeLayout(false);
            this.AutodetectFLP.PerformLayout();
            this.RunTBPanel.ResumeLayout(false);
            this.RunTBPanel.PerformLayout();
            this.ResumeLayout(false);

    }
#endif

    #endregion

    internal System.Windows.Forms.Panel PagePanel;
    internal System.Windows.Forms.Control DummyAutoScrollPanel;
    internal CustomControls.DarkGroupBox ThiefBuddyOptionsGroupBox;
    internal CustomControls.DarkLabel ThiefBuddyExeLabel;
    internal CustomControls.DarkButton ThiefBuddyExeBrowseButton;
    internal CustomControls.DarkTextBox ThiefBuddyExeTextBox;
    internal CustomControls.DarkRadioButton RunTBNeverRadioButton;
    internal CustomControls.DarkRadioButton RunTBAskRadioButton;
    internal CustomControls.DarkRadioButton RunTBAlwaysRadioButton;
    internal System.Windows.Forms.Panel RunTBPanel;
    internal CustomControls.DarkLabel RunThiefBuddyWhenPlayingFMsLabel;
    internal CustomControls.DarkButton AutodetectButton;
    internal System.Windows.Forms.FlowLayoutPanel AutodetectFLP;
    internal System.Windows.Forms.PictureBox TBHelpPictureBox;
    internal CustomControls.DarkLabel WhatIsTBHelpLabel;
    internal CustomControls.DarkLinkLabel GetTBLinkLabel;
}
