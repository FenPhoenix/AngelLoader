namespace AngelLoader.Forms;

sealed partial class TDMDownloadForm
{
    /// <summary>
    /// Custom generated component initializer with cruft removed.
    /// </summary>
    private void InitSlim()
    {
        this.CloseButton = new AngelLoader.Forms.CustomControls.StandardButton();
        this.MoreDetailsButton = new AngelLoader.Forms.CustomControls.DarkButton();
        this.SuspendLayout();
        // 
        // CloseButton
        // 
        this.CloseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
        this.CloseButton.DialogResult = System.Windows.Forms.DialogResult.OK;
        this.CloseButton.Location = new System.Drawing.Point(747, 504);
        this.CloseButton.TabIndex = 3;
        // 
        // MoreDetailsButton
        // 
        this.MoreDetailsButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
        this.MoreDetailsButton.Location = new System.Drawing.Point(32, 504);
        this.MoreDetailsButton.Size = new System.Drawing.Size(75, 23);
        this.MoreDetailsButton.TabIndex = 5;
        this.MoreDetailsButton.Visible = false;
        this.MoreDetailsButton.Click += new System.EventHandler(this.MoreDetailsButton_Click);
        // 
        // TDMDownloadForm
        // 
        this.AcceptButton = this.CloseButton;
        this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
        this.ClientSize = new System.Drawing.Size(830, 535);
        this.Controls.Add(this.MoreDetailsButton);
        this.Controls.Add(this.CloseButton);
        this.MinimumSize = new System.Drawing.Size(846, 574);
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
        // Hack to prevent slow first render on some forms if Text is blank
        this.Text = " ";
        this.Load += new System.EventHandler(this.TDMDownloadForm_Load);
        this.Shown += new System.EventHandler(this.TDMDownloadForm_Shown);
        this.ResumeLayout(false);
        this.PerformLayout();
    }
}
