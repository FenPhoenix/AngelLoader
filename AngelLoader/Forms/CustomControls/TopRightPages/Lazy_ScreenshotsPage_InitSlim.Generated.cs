namespace AngelLoader.Forms.CustomControls;

public sealed partial class Lazy_ScreenshotsPage
{
    /// <summary>
    /// Custom generated component initializer with cruft removed.
    /// </summary>
    private void InitSlim()
    {
        this.ScreenshotsPictureBox = new System.Windows.Forms.PictureBox();
        this.ScreenshotsPrevButton = new AngelLoader.Forms.CustomControls.DarkButton();
        this.ScreenshotsNextButton = new AngelLoader.Forms.CustomControls.DarkButton();
        ((System.ComponentModel.ISupportInitialize)(this.ScreenshotsPictureBox)).BeginInit();
        this.SuspendLayout();
        // 
        // ScreenshotsPictureBox
        // 
        this.ScreenshotsPictureBox.Location = new System.Drawing.Point(8, 8);
        this.ScreenshotsPictureBox.Size = new System.Drawing.Size(328, 192);
        this.ScreenshotsPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
        // 
        // ScreenshotsPrevButton
        // 
        this.ScreenshotsPrevButton.Location = new System.Drawing.Point(187, 200);
        this.ScreenshotsPrevButton.Size = new System.Drawing.Size(75, 23);
        this.ScreenshotsPrevButton.TabIndex = 1;
        // 
        // ScreenshotsNextButton
        // 
        this.ScreenshotsNextButton.Location = new System.Drawing.Point(262, 200);
        this.ScreenshotsNextButton.Size = new System.Drawing.Size(75, 23);
        this.ScreenshotsNextButton.TabIndex = 1;
        // 
        // Lazy_ScreenshotsPage
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
        this.AutoScroll = true;
        this.Controls.Add(this.ScreenshotsNextButton);
        this.Controls.Add(this.ScreenshotsPrevButton);
        this.Controls.Add(this.ScreenshotsPictureBox);
        this.Size = new System.Drawing.Size(527, 284);
        ((System.ComponentModel.ISupportInitialize)(this.ScreenshotsPictureBox)).EndInit();
        this.ResumeLayout(false);
    }
}
