#define FenGen_DesignerSource

namespace AngelLoader.Forms.CustomControls;

public sealed partial class Lazy_ScreenshotsPage
{
    /// <summary> 
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary> 
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    #region Component Designer generated code

#if DEBUG
    /// <summary> 
    /// Required method for Designer support - do not modify 
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
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
            this.ScreenshotsPictureBox.Name = "ScreenshotsPictureBox";
            this.ScreenshotsPictureBox.Size = new System.Drawing.Size(328, 192);
            this.ScreenshotsPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.ScreenshotsPictureBox.TabIndex = 0;
            this.ScreenshotsPictureBox.TabStop = false;
            // 
            // ScreenshotsPrevButton
            // 
            this.ScreenshotsPrevButton.Location = new System.Drawing.Point(187, 200);
            this.ScreenshotsPrevButton.Name = "ScreenshotsPrevButton";
            this.ScreenshotsPrevButton.Size = new System.Drawing.Size(75, 23);
            this.ScreenshotsPrevButton.TabIndex = 1;
            this.ScreenshotsPrevButton.Text = "Prev";
            // 
            // ScreenshotsNextButton
            // 
            this.ScreenshotsNextButton.Location = new System.Drawing.Point(262, 200);
            this.ScreenshotsNextButton.Name = "ScreenshotsNextButton";
            this.ScreenshotsNextButton.Size = new System.Drawing.Size(75, 23);
            this.ScreenshotsNextButton.TabIndex = 1;
            this.ScreenshotsNextButton.Text = "Next";
            // 
            // Lazy_ScreenshotsPage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoScroll = true;
            this.Controls.Add(this.ScreenshotsNextButton);
            this.Controls.Add(this.ScreenshotsPrevButton);
            this.Controls.Add(this.ScreenshotsPictureBox);
            this.Name = "Lazy_ScreenshotsPage";
            this.Size = new System.Drawing.Size(527, 284);
            ((System.ComponentModel.ISupportInitialize)(this.ScreenshotsPictureBox)).EndInit();
            this.ResumeLayout(false);

    }
#endif

    #endregion

    internal System.Windows.Forms.PictureBox ScreenshotsPictureBox;
    internal DarkButton ScreenshotsPrevButton;
    internal DarkButton ScreenshotsNextButton;
}
