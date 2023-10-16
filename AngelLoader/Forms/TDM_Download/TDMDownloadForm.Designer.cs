#define FenGen_DesignerSource

namespace AngelLoader.Forms;

sealed partial class TDMDownloadForm
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

#if DEBUG
    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
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
            this.CloseButton.Name = "CloseButton";
            this.CloseButton.TabIndex = 3;
            this.CloseButton.Text = "Close";
            // 
            // MoreDetailsButton
            // 
            this.MoreDetailsButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.MoreDetailsButton.Location = new System.Drawing.Point(32, 504);
            this.MoreDetailsButton.Name = "MoreDetailsButton";
            this.MoreDetailsButton.Size = new System.Drawing.Size(75, 23);
            this.MoreDetailsButton.TabIndex = 5;
            this.MoreDetailsButton.Text = "More...";
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
            this.Name = "TDMDownloadForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Dark Mod Downloader";
            this.Load += new System.EventHandler(this.TDMDownloadForm_Load);
            this.Shown += new System.EventHandler(this.TDMDownloadForm_Shown);
            this.ResumeLayout(false);
            this.PerformLayout();

    }
#endif

    #endregion
    internal CustomControls.StandardButton CloseButton;
    internal CustomControls.DarkButton MoreDetailsButton;
}
