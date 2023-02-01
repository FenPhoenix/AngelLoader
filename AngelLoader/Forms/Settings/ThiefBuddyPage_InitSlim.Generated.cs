namespace AngelLoader.Forms;

partial class ThiefBuddyPage
{
    /// <summary>
    /// Custom generated component initializer with cruft removed.
    /// </summary>
    private void InitSlim()
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
        this.PagePanel.Size = new System.Drawing.Size(440, 692);
        this.PagePanel.TabIndex = 1;
        // 
        // DummyAutoScrollPanel
        // 
        this.DummyAutoScrollPanel.Location = new System.Drawing.Point(8, 48);
        this.DummyAutoScrollPanel.Size = new System.Drawing.Size(424, 8);
        this.DummyAutoScrollPanel.TabIndex = 12;
        // 
        // TBHelpPictureBox
        // 
        this.TBHelpPictureBox.Image = global::AngelLoader.Properties.Resources.Help;
        this.TBHelpPictureBox.Location = new System.Drawing.Point(8, 186);
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
        this.ThiefBuddyOptionsGroupBox.Size = new System.Drawing.Size(424, 168);
        this.ThiefBuddyOptionsGroupBox.TabIndex = 13;
        this.ThiefBuddyOptionsGroupBox.TabStop = false;
        // 
        // HorizDiv1
        // 
        this.HorizDiv1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
        | System.Windows.Forms.AnchorStyles.Right)));
        this.HorizDiv1.Location = new System.Drawing.Point(16, 40);
        this.HorizDiv1.Size = new System.Drawing.Size(392, 16);
        this.HorizDiv1.TabIndex = 7;
        // 
        // TBInstallStatusLabel
        // 
        this.TBInstallStatusLabel.AutoSize = true;
        this.TBInstallStatusLabel.Location = new System.Drawing.Point(16, 24);
        this.TBInstallStatusLabel.TabIndex = 6;
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
        this.RunTBPanel.Size = new System.Drawing.Size(392, 100);
        this.RunTBPanel.TabIndex = 5;
        // 
        // RunThiefBuddyWhenPlayingFMsLabel
        // 
        this.RunThiefBuddyWhenPlayingFMsLabel.AutoSize = true;
        this.RunThiefBuddyWhenPlayingFMsLabel.TabIndex = 5;
        // 
        // RunTBAlwaysRadioButton
        // 
        this.RunTBAlwaysRadioButton.AutoSize = true;
        this.RunTBAlwaysRadioButton.Location = new System.Drawing.Point(8, 24);
        this.RunTBAlwaysRadioButton.TabIndex = 4;
        this.RunTBAlwaysRadioButton.UseVisualStyleBackColor = true;
        // 
        // RunTBNeverRadioButton
        // 
        this.RunTBNeverRadioButton.AutoSize = true;
        this.RunTBNeverRadioButton.Location = new System.Drawing.Point(8, 72);
        this.RunTBNeverRadioButton.TabIndex = 4;
        this.RunTBNeverRadioButton.UseVisualStyleBackColor = true;
        // 
        // RunTBAskRadioButton
        // 
        this.RunTBAskRadioButton.AutoSize = true;
        this.RunTBAskRadioButton.Checked = true;
        this.RunTBAskRadioButton.Location = new System.Drawing.Point(8, 48);
        this.RunTBAskRadioButton.TabIndex = 4;
        this.RunTBAskRadioButton.TabStop = true;
        this.RunTBAskRadioButton.UseVisualStyleBackColor = true;
        // 
        // GetTBLinkLabel
        // 
        this.GetTBLinkLabel.AutoSize = true;
        this.GetTBLinkLabel.Location = new System.Drawing.Point(27, 208);
        this.GetTBLinkLabel.TabIndex = 10;
        this.GetTBLinkLabel.TabStop = true;
        // 
        // WhatIsTBHelpLabel
        // 
        this.WhatIsTBHelpLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
        | System.Windows.Forms.AnchorStyles.Right)));
        this.WhatIsTBHelpLabel.Location = new System.Drawing.Point(27, 180);
        this.WhatIsTBHelpLabel.MinimumSize = new System.Drawing.Size(0, 13);
        this.WhatIsTBHelpLabel.Size = new System.Drawing.Size(389, 28);
        this.WhatIsTBHelpLabel.TabIndex = 8;
        this.WhatIsTBHelpLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        // 
        // ThiefBuddyPage
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.Controls.Add(this.PagePanel);
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
}
