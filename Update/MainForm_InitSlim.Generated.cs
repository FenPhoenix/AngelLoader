namespace Update;

sealed partial class MainForm
{
    /// <summary>
    /// Custom generated component initializer with cruft removed.
    /// </summary>
    private void InitSlim()
    {
        System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
        this.CopyingLabel = new System.Windows.Forms.Label();
        this.CopyingProgressBar = new System.Windows.Forms.ProgressBar();
        this.SuspendLayout();
        // 
        // CopyingLabel
        // 
        this.CopyingLabel.AutoSize = true;
        this.CopyingLabel.Location = new System.Drawing.Point(200, 24);
        this.CopyingLabel.TextAlign = System.Drawing.ContentAlignment.TopCenter;
        // 
        // CopyingProgressBar
        // 
        this.CopyingProgressBar.Location = new System.Drawing.Point(32, 64);
        this.CopyingProgressBar.Size = new System.Drawing.Size(392, 23);
        // 
        // MainForm
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
        this.ClientSize = new System.Drawing.Size(456, 115);
        this.Controls.Add(this.CopyingProgressBar);
        this.Controls.Add(this.CopyingLabel);
        this.MaximizeBox = false;
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
        // Hack to prevent slow first render on some forms if Text is blank
        this.Text = " ";
        this.ResumeLayout(false);
        this.PerformLayout();
    }
}
