namespace AngelLoader.Forms;

partial class UpdatePage
{
    /// <summary>
    /// Custom generated component initializer with cruft removed.
    /// </summary>
    private void InitSlim()
    {
        this.PagePanel = new System.Windows.Forms.Panel();
        this.UpdateOptionsGroupBox = new AngelLoader.Forms.CustomControls.DarkGroupBox();
        this.CheckForUpdatesOnStartupCheckBox = new AngelLoader.Forms.CustomControls.DarkCheckBox();
        this.DummyAutoScrollPanel = new System.Windows.Forms.Control();
        this.PagePanel.SuspendLayout();
        this.UpdateOptionsGroupBox.SuspendLayout();
        this.SuspendLayout();
        // 
        // PagePanel
        // 
        this.PagePanel.AutoScroll = true;
        this.PagePanel.Controls.Add(this.UpdateOptionsGroupBox);
        this.PagePanel.Controls.Add(this.DummyAutoScrollPanel);
        this.PagePanel.Dock = System.Windows.Forms.DockStyle.Fill;
        this.PagePanel.Size = new System.Drawing.Size(440, 692);
        this.PagePanel.TabIndex = 0;
        // 
        // UpdateOptionsGroupBox
        // 
        this.UpdateOptionsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
        | System.Windows.Forms.AnchorStyles.Right)));
        this.UpdateOptionsGroupBox.Controls.Add(this.CheckForUpdatesOnStartupCheckBox);
        this.UpdateOptionsGroupBox.Location = new System.Drawing.Point(8, 8);
        this.UpdateOptionsGroupBox.MinimumSize = new System.Drawing.Size(424, 0);
        this.UpdateOptionsGroupBox.Size = new System.Drawing.Size(424, 56);
        this.UpdateOptionsGroupBox.TabIndex = 1;
        this.UpdateOptionsGroupBox.TabStop = false;
        // 
        // CheckForUpdatesOnStartupCheckBox
        // 
        this.CheckForUpdatesOnStartupCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
        | System.Windows.Forms.AnchorStyles.Right)));
        this.CheckForUpdatesOnStartupCheckBox.Location = new System.Drawing.Point(16, 16);
        this.CheckForUpdatesOnStartupCheckBox.Size = new System.Drawing.Size(392, 32);
        this.CheckForUpdatesOnStartupCheckBox.TabIndex = 0;
        // 
        // DummyAutoScrollPanel
        // 
        this.DummyAutoScrollPanel.Location = new System.Drawing.Point(8, 48);
        this.DummyAutoScrollPanel.Size = new System.Drawing.Size(424, 8);
        // 
        // UpdatePage
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.Controls.Add(this.PagePanel);
        this.Size = new System.Drawing.Size(440, 692);
        this.PagePanel.ResumeLayout(false);
        this.UpdateOptionsGroupBox.ResumeLayout(false);
        this.ResumeLayout(false);
    }
}
