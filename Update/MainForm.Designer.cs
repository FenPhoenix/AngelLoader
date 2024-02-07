#define FenGen_DesignerSource

namespace Update;

sealed partial class MainForm
{
    #region Windows Form Designer generated code

#if DEBUG
    /// <summary>
    ///  Required method for Designer support - do not modify
    ///  the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
            this.Message1Label = new Update.DarkLabel();
            this.CopyingProgressBar = new Update.DarkProgressBar();
            this.CopyProgressBarOutlinePanel = new System.Windows.Forms.Panel();
            this.Message2Label = new Update.DarkLabel();
            this.CopyProgressBarOutlinePanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // Message1Label
            // 
            this.Message1Label.AutoSize = true;
            this.Message1Label.Location = new System.Drawing.Point(260, 24);
            this.Message1Label.Name = "Message1Label";
            this.Message1Label.Size = new System.Drawing.Size(54, 13);
            this.Message1Label.TabIndex = 0;
            this.Message1Label.Text = "Copying...";
            this.Message1Label.UseMnemonic = false;
            // 
            // CopyingProgressBar
            // 
            this.CopyingProgressBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.CopyingProgressBar.Location = new System.Drawing.Point(0, 0);
            this.CopyingProgressBar.Name = "CopyingProgressBar";
            this.CopyingProgressBar.Size = new System.Drawing.Size(504, 23);
            this.CopyingProgressBar.TabIndex = 1;
            // 
            // CopyProgressBarOutlinePanel
            // 
            this.CopyProgressBarOutlinePanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.CopyProgressBarOutlinePanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.CopyProgressBarOutlinePanel.Controls.Add(this.CopyingProgressBar);
            this.CopyProgressBarOutlinePanel.Location = new System.Drawing.Point(31, 63);
            this.CopyProgressBarOutlinePanel.Name = "CopyProgressBarOutlinePanel";
            this.CopyProgressBarOutlinePanel.Size = new System.Drawing.Size(506, 25);
            this.CopyProgressBarOutlinePanel.TabIndex = 2;
            // 
            // Message2Label
            // 
            this.Message2Label.AutoSize = true;
            this.Message2Label.Location = new System.Drawing.Point(260, 40);
            this.Message2Label.Name = "Message2Label";
            this.Message2Label.Size = new System.Drawing.Size(54, 13);
            this.Message2Label.TabIndex = 0;
            this.Message2Label.Text = "Copying...";
            this.Message2Label.UseMnemonic = false;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(568, 115);
            this.Controls.Add(this.CopyProgressBarOutlinePanel);
            this.Controls.Add(this.Message2Label);
            this.Controls.Add(this.Message1Label);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.ShowInTaskbar = true;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "AngelLoader Update";
            this.CopyProgressBarOutlinePanel.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

    }
#endif

    #endregion

    private DarkLabel Message1Label;
    private DarkProgressBar CopyingProgressBar;
    private System.Windows.Forms.Panel CopyProgressBarOutlinePanel;
    private DarkLabel Message2Label;
}
