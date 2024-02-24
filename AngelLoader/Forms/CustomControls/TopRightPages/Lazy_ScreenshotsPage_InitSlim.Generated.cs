namespace AngelLoader.Forms.CustomControls;

public sealed partial class Lazy_ScreenshotsPage
{
    /// <summary>
    /// Custom generated component initializer with cruft removed.
    /// </summary>
    private void InitSlim()
    {
        this.NumberLabel = new AngelLoader.Forms.CustomControls.DarkLabel();
        this.NextButton = new AngelLoader.Forms.CustomControls.DarkArrowButton();
        this.PrevButton = new AngelLoader.Forms.CustomControls.DarkArrowButton();
        this.ScreenshotsPictureBox = new AngelLoader.Forms.CustomControls.DarkPictureBox();
        this.OpenScreenshotsFolderButton = new AngelLoader.Forms.CustomControls.DarkButton();
        ((System.ComponentModel.ISupportInitialize)(this.ScreenshotsPictureBox)).BeginInit();
        this.SuspendLayout();
        // 
        // NumberLabel
        // 
        this.NumberLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
        this.NumberLabel.AutoSize = true;
        this.NumberLabel.Location = new System.Drawing.Point(472, 238);
        this.NumberLabel.Size = new System.Drawing.Size(48, 13);
        // 
        // NextButton
        // 
        this.NextButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
        this.NextButton.ArrowDirection = AngelLoader.Forms.Direction.Right;
        this.NextButton.Location = new System.Drawing.Point(446, 256);
        this.NextButton.Size = new System.Drawing.Size(75, 23);
        this.NextButton.TabIndex = 2;
        // 
        // PrevButton
        // 
        this.PrevButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
        this.PrevButton.ArrowDirection = AngelLoader.Forms.Direction.Left;
        this.PrevButton.Location = new System.Drawing.Point(371, 256);
        this.PrevButton.Size = new System.Drawing.Size(75, 23);
        this.PrevButton.TabIndex = 1;
        // 
        // ScreenshotsPictureBox
        // 
        this.ScreenshotsPictureBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
        | System.Windows.Forms.AnchorStyles.Left)
        | System.Windows.Forms.AnchorStyles.Right)));
        this.ScreenshotsPictureBox.Location = new System.Drawing.Point(8, 8);
        this.ScreenshotsPictureBox.Size = new System.Drawing.Size(512, 224);
        this.ScreenshotsPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
        // 
        // OpenScreenshotsFolderButton
        // 
        this.OpenScreenshotsFolderButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
        this.OpenScreenshotsFolderButton.Location = new System.Drawing.Point(336, 256);
        this.OpenScreenshotsFolderButton.Size = new System.Drawing.Size(35, 23);
        this.OpenScreenshotsFolderButton.TabIndex = 0;
        // 
        // Lazy_ScreenshotsPage
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
        this.AutoScroll = true;
        this.AutoScrollMinSize = new System.Drawing.Size(220, 100);
        this.Controls.Add(this.OpenScreenshotsFolderButton);
        this.Controls.Add(this.NumberLabel);
        this.Controls.Add(this.NextButton);
        this.Controls.Add(this.PrevButton);
        this.Controls.Add(this.ScreenshotsPictureBox);
        this.Size = new System.Drawing.Size(527, 284);
        ((System.ComponentModel.ISupportInitialize)(this.ScreenshotsPictureBox)).EndInit();
        this.ResumeLayout(false);
        this.PerformLayout();
    }
}
