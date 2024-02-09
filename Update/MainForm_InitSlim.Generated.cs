namespace Update;

sealed partial class MainForm
{
    /// <summary>
    /// Custom generated component initializer with cruft removed.
    /// </summary>
    private void InitSlim()
    {
        this.Message1Label = new Update.DarkLabel();
        this.CopyProgressBar = new Update.DarkProgressBar();
        this.Message2Label = new Update.DarkLabel();
        this.SuspendLayout();
        // 
        // Message1Label
        // 
        this.Message1Label.AutoSize = true;
        this.Message1Label.Location = new System.Drawing.Point(260, 24);
        this.Message1Label.UseMnemonic = false;
        // 
        // CopyProgressBar
        // 
        this.CopyProgressBar.Location = new System.Drawing.Point(32, 64);
        this.CopyProgressBar.Size = new System.Drawing.Size(504, 23);
        // 
        // Message2Label
        // 
        this.Message2Label.AutoSize = true;
        this.Message2Label.Location = new System.Drawing.Point(260, 40);
        this.Message2Label.UseMnemonic = false;
        // 
        // MainForm
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
        this.ClientSize = new System.Drawing.Size(568, 115);
        this.Controls.Add(this.CopyProgressBar);
        this.Controls.Add(this.Message2Label);
        this.Controls.Add(this.Message1Label);
        this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
        this.MaximizeBox = false;
        this.ShowInTaskbar = true;
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
        // Hack to prevent slow first render on some forms if Text is blank
        this.Text = " ";
        this.ResumeLayout(false);
        this.PerformLayout();
    }
}
