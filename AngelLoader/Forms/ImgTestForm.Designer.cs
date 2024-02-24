namespace AngelLoader.Forms;

sealed partial class ImgTestForm
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

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
            this.ImageBox = new AngelLoader.Forms.CustomControls.ImagePanelCustom();
            this.GammaTrackBar = new System.Windows.Forms.TrackBar();
            this.CopyButton = new AngelLoader.Forms.CustomControls.DarkButton();
            ((System.ComponentModel.ISupportInitialize)(this.GammaTrackBar)).BeginInit();
            this.SuspendLayout();
            // 
            // ImageBox
            // 
            this.ImageBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ImageBox.Location = new System.Drawing.Point(8, 8);
            this.ImageBox.Name = "ImageBox";
            this.ImageBox.Size = new System.Drawing.Size(784, 384);
            this.ImageBox.TabIndex = 0;
            this.ImageBox.Paint += new System.Windows.Forms.PaintEventHandler(this.ImageBox_Paint);
            // 
            // GammaTrackBar
            // 
            this.GammaTrackBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.GammaTrackBar.Location = new System.Drawing.Point(16, 400);
            this.GammaTrackBar.Maximum = 20;
            this.GammaTrackBar.Name = "GammaTrackBar";
            this.GammaTrackBar.Size = new System.Drawing.Size(784, 45);
            this.GammaTrackBar.TabIndex = 0;
            this.GammaTrackBar.Value = 10;
            this.GammaTrackBar.Scroll += new System.EventHandler(this.GammaTrackBar_Scroll);
            // 
            // CopyButton
            // 
            this.CopyButton.Location = new System.Drawing.Point(336, 464);
            this.CopyButton.Name = "CopyButton";
            this.CopyButton.Size = new System.Drawing.Size(144, 24);
            this.CopyButton.TabIndex = 1;
            this.CopyButton.Text = "Copy to clipboard";
            this.CopyButton.Click += new System.EventHandler(this.CopyButton_Click);
            // 
            // ImgTestForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 522);
            this.Controls.Add(this.CopyButton);
            this.Controls.Add(this.GammaTrackBar);
            this.Controls.Add(this.ImageBox);
            this.Name = "ImgTestForm";
            this.Text = "ImgTestForm";
            ((System.ComponentModel.ISupportInitialize)(this.GammaTrackBar)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

    }

    #endregion

    private CustomControls.ImagePanelCustom ImageBox;
    private System.Windows.Forms.TrackBar GammaTrackBar;
    private CustomControls.DarkButton CopyButton;
}