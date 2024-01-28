namespace Update;

sealed partial class MainForm
{
    /// <summary>
    /// Custom generated component initializer with cruft removed.
    /// </summary>
    private void InitSlim()
    {
        this.CopyingLabel = new Update.DarkLabel();
        this.CopyingProgressBar = new Update.DarkProgressBar();
        this.CopyProgressBarOutlinePanel = new System.Windows.Forms.Panel();
        this.CopyProgressBarOutlinePanel.SuspendLayout();
        this.SuspendLayout();
        // 
        // CopyingLabel
        // 
        this.CopyingLabel.AutoSize = true;
        this.CopyingLabel.Location = new System.Drawing.Point(200, 24);
        this.CopyingLabel.UseMnemonic = false;
        // 
        // CopyingProgressBar
        // 
        this.CopyingProgressBar.Size = new System.Drawing.Size(392, 23);
        // 
        // CopyProgressBarOutlinePanel
        // 
        this.CopyProgressBarOutlinePanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        this.CopyProgressBarOutlinePanel.Controls.Add(this.CopyingProgressBar);
        this.CopyProgressBarOutlinePanel.Location = new System.Drawing.Point(31, 63);
        this.CopyProgressBarOutlinePanel.Size = new System.Drawing.Size(394, 25);
        this.CopyProgressBarOutlinePanel.TabIndex = 2;
        // 
        // MainForm
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
        this.ClientSize = new System.Drawing.Size(456, 115);
        this.Controls.Add(this.CopyProgressBarOutlinePanel);
        this.Controls.Add(this.CopyingLabel);
        this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
        this.MaximizeBox = false;
        this.ShowInTaskbar = true;
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
        // Hack to prevent slow first render on some forms if Text is blank
        this.Text = " ";
        this.CopyProgressBarOutlinePanel.ResumeLayout(false);
        this.ResumeLayout(false);
        this.PerformLayout();
    }
}
