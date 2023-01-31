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
        this.ThiefBuddyOptionsGroupBox = new AngelLoader.Forms.CustomControls.DarkGroupBox();
        this.ThiefBuddyExeLabel = new AngelLoader.Forms.CustomControls.DarkLabel();
        this.ThiefBuddyExeBrowseButton = new AngelLoader.Forms.CustomControls.DarkButton();
        this.ThiefBuddyExeTextBox = new AngelLoader.Forms.CustomControls.DarkTextBox();
        this.UseThiefBuddyCheckBox = new AngelLoader.Forms.CustomControls.DarkCheckBox();
        this.PagePanel.SuspendLayout();
        this.ThiefBuddyOptionsGroupBox.SuspendLayout();
        this.SuspendLayout();
        // 
        // PagePanel
        // 
        this.PagePanel.AutoScroll = true;
        this.PagePanel.Controls.Add(this.ThiefBuddyOptionsGroupBox);
        this.PagePanel.Controls.Add(this.DummyAutoScrollPanel);
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
        // ThiefBuddyOptionsGroupBox
        // 
        this.ThiefBuddyOptionsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
        | System.Windows.Forms.AnchorStyles.Right)));
        this.ThiefBuddyOptionsGroupBox.Controls.Add(this.ThiefBuddyExeLabel);
        this.ThiefBuddyOptionsGroupBox.Controls.Add(this.ThiefBuddyExeBrowseButton);
        this.ThiefBuddyOptionsGroupBox.Controls.Add(this.ThiefBuddyExeTextBox);
        this.ThiefBuddyOptionsGroupBox.Controls.Add(this.UseThiefBuddyCheckBox);
        this.ThiefBuddyOptionsGroupBox.Location = new System.Drawing.Point(8, 8);
        this.ThiefBuddyOptionsGroupBox.MinimumSize = new System.Drawing.Size(424, 0);
        this.ThiefBuddyOptionsGroupBox.Size = new System.Drawing.Size(424, 104);
        this.ThiefBuddyOptionsGroupBox.TabIndex = 13;
        this.ThiefBuddyOptionsGroupBox.TabStop = false;
        // 
        // ThiefBuddyExeLabel
        // 
        this.ThiefBuddyExeLabel.AutoSize = true;
        this.ThiefBuddyExeLabel.Location = new System.Drawing.Point(16, 24);
        this.ThiefBuddyExeLabel.TabIndex = 3;
        // 
        // ThiefBuddyExeBrowseButton
        // 
        this.ThiefBuddyExeBrowseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
        this.ThiefBuddyExeBrowseButton.AutoSize = true;
        this.ThiefBuddyExeBrowseButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        this.ThiefBuddyExeBrowseButton.Location = new System.Drawing.Point(336, 39);
        this.ThiefBuddyExeBrowseButton.MinimumSize = new System.Drawing.Size(75, 23);
        this.ThiefBuddyExeBrowseButton.Padding = new System.Windows.Forms.Padding(6, 0, 6, 0);
        this.ThiefBuddyExeBrowseButton.Size = new System.Drawing.Size(75, 23);
        this.ThiefBuddyExeBrowseButton.TabIndex = 2;
        this.ThiefBuddyExeBrowseButton.UseVisualStyleBackColor = true;
        // 
        // ThiefBuddyExeTextBox
        // 
        this.ThiefBuddyExeTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
        | System.Windows.Forms.AnchorStyles.Right)));
        this.ThiefBuddyExeTextBox.Location = new System.Drawing.Point(16, 40);
        this.ThiefBuddyExeTextBox.Size = new System.Drawing.Size(320, 20);
        this.ThiefBuddyExeTextBox.TabIndex = 1;
        // 
        // UseThiefBuddyCheckBox
        // 
        this.UseThiefBuddyCheckBox.AutoSize = true;
        this.UseThiefBuddyCheckBox.Checked = true;
        this.UseThiefBuddyCheckBox.Location = new System.Drawing.Point(16, 72);
        this.UseThiefBuddyCheckBox.TabIndex = 0;
        this.UseThiefBuddyCheckBox.UseVisualStyleBackColor = true;
        // 
        // ThiefBuddyPage
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.Controls.Add(this.PagePanel);
        this.Size = new System.Drawing.Size(440, 692);
        this.PagePanel.ResumeLayout(false);
        this.ThiefBuddyOptionsGroupBox.ResumeLayout(false);
        this.ThiefBuddyOptionsGroupBox.PerformLayout();
        this.ResumeLayout(false);
    }
}
