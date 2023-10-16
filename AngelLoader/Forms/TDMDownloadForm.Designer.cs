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
            this.DownloadButton = new AngelLoader.Forms.CustomControls.DarkButton();
            this.DownloadListBox = new AngelLoader.Forms.CustomControls.DarkListBoxWithBackingItems();
            this.ServerListBox = new AngelLoader.Forms.CustomControls.DarkListBoxWithBackingItems();
            this.SelectForDownloadButton = new AngelLoader.Forms.CustomControls.DarkButton();
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
            // DownloadButton
            // 
            this.DownloadButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.DownloadButton.Location = new System.Drawing.Point(726, 448);
            this.DownloadButton.Name = "DownloadButton";
            this.DownloadButton.Size = new System.Drawing.Size(75, 23);
            this.DownloadButton.TabIndex = 2;
            this.DownloadButton.Text = "Download";
            this.DownloadButton.Click += new System.EventHandler(this.DownloadButton_Click);
            // 
            // DownloadListBox
            // 
            this.DownloadListBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.DownloadListBox.Location = new System.Drawing.Point(448, 40);
            this.DownloadListBox.Name = "DownloadListBox";
            this.DownloadListBox.Size = new System.Drawing.Size(352, 400);
            this.DownloadListBox.TabIndex = 1;
            // 
            // ServerListBox
            // 
            this.ServerListBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.ServerListBox.Location = new System.Drawing.Point(32, 40);
            this.ServerListBox.Name = "ServerListBox";
            this.ServerListBox.Size = new System.Drawing.Size(352, 400);
            this.ServerListBox.TabIndex = 1;
            // 
            // SelectForDownloadButton
            // 
            this.SelectForDownloadButton.Location = new System.Drawing.Point(392, 216);
            this.SelectForDownloadButton.Name = "SelectForDownloadButton";
            this.SelectForDownloadButton.Size = new System.Drawing.Size(48, 23);
            this.SelectForDownloadButton.TabIndex = 0;
            this.SelectForDownloadButton.Click += new System.EventHandler(this.SelectForDownloadButton_Click);
            // 
            // TDMDownloadForm
            // 
            this.AcceptButton = this.CloseButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(830, 535);
            this.Controls.Add(this.CloseButton);
            this.Controls.Add(this.DownloadButton);
            this.Controls.Add(this.DownloadListBox);
            this.Controls.Add(this.ServerListBox);
            this.Controls.Add(this.SelectForDownloadButton);
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

    private CustomControls.DarkButton SelectForDownloadButton;
    private CustomControls.DarkListBoxWithBackingItems ServerListBox;
    private CustomControls.DarkListBoxWithBackingItems DownloadListBox;
    private CustomControls.DarkButton DownloadButton;
    private CustomControls.StandardButton CloseButton;
}
