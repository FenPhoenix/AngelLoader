namespace AngelLoader.Forms.CustomControls;

public sealed partial class Lazy_ScreenshotsPage
{
    /// <summary>
    /// Custom generated component initializer with cruft removed.
    /// </summary>
    private void InitSlim()
    {
        this.NumberLabel = new AngelLoader.Forms.CustomControls.DarkLabel();
        this.ScreenshotsNextButton = new AngelLoader.Forms.CustomControls.DarkArrowButton();
        this.ScreenshotsPrevButton = new AngelLoader.Forms.CustomControls.DarkArrowButton();
        this.ScreenshotsPictureBox = new AngelLoader.Forms.CustomControls.DarkPictureBox();
        ((System.ComponentModel.ISupportInitialize)(this.ScreenshotsPictureBox)).BeginInit();
        this.SuspendLayout();
        // 
        // NumberLabel
        // 
        this.NumberLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
        this.NumberLabel.AutoSize = true;
        this.NumberLabel.Location = new System.Drawing.Point(8, 260);
        this.NumberLabel.Size = new System.Drawing.Size(0, 13);
        // 
        // ScreenshotsNextButton
        // 
        this.ScreenshotsNextButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
        this.ScreenshotsNextButton.ArrowDirection = AngelLoader.Forms.Direction.Right;
        this.ScreenshotsNextButton.Location = new System.Drawing.Point(446, 256);
        this.ScreenshotsNextButton.Size = new System.Drawing.Size(75, 23);
        this.ScreenshotsNextButton.TabIndex = 2;
        // 
        // ScreenshotsPrevButton
        // 
        this.ScreenshotsPrevButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
        this.ScreenshotsPrevButton.ArrowDirection = AngelLoader.Forms.Direction.Left;
        this.ScreenshotsPrevButton.Location = new System.Drawing.Point(371, 256);
        this.ScreenshotsPrevButton.Size = new System.Drawing.Size(75, 23);
        this.ScreenshotsPrevButton.TabIndex = 1;
        // 
        // ScreenshotsPictureBox
        // 
        this.ScreenshotsPictureBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
        | System.Windows.Forms.AnchorStyles.Left)
        | System.Windows.Forms.AnchorStyles.Right)));
        this.ScreenshotsPictureBox.Location = new System.Drawing.Point(8, 8);
        this.ScreenshotsPictureBox.Size = new System.Drawing.Size(512, 248);
        this.ScreenshotsPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
        // 
        // Lazy_ScreenshotsPage
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
        this.AutoScroll = true;
        this.AutoScrollMinSize = new System.Drawing.Size(220, 100);
        this.Controls.Add(this.NumberLabel);
        this.Controls.Add(this.ScreenshotsNextButton);
        this.Controls.Add(this.ScreenshotsPrevButton);
        this.Controls.Add(this.ScreenshotsPictureBox);
        this.Size = new System.Drawing.Size(527, 284);
        ((System.ComponentModel.ISupportInitialize)(this.ScreenshotsPictureBox)).EndInit();
        this.ResumeLayout(false);
        this.PerformLayout();
    }
}
