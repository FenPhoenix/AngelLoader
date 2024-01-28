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
            this.CopyingLabel = new Update.DarkLabel();
            this.CopyingProgressBar = new Update.DarkProgressBar();
            this.CopyProgressBarOutlinePanel = new System.Windows.Forms.Panel();
            this.CopyProgressBarOutlinePanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // CopyingLabel
            // 
            this.CopyingLabel.AutoSize = true;
            this.CopyingLabel.Location = new System.Drawing.Point(200, 24);
            this.CopyingLabel.Name = "CopyingLabel";
            this.CopyingLabel.Size = new System.Drawing.Size(54, 13);
            this.CopyingLabel.TabIndex = 0;
            this.CopyingLabel.Text = "Copying...";
            this.CopyingLabel.UseMnemonic = false;
            // 
            // CopyingProgressBar
            // 
            this.CopyingProgressBar.Location = new System.Drawing.Point(0, 0);
            this.CopyingProgressBar.Name = "CopyingProgressBar";
            this.CopyingProgressBar.Size = new System.Drawing.Size(392, 23);
            this.CopyingProgressBar.TabIndex = 1;
            // 
            // CopyProgressBarOutlinePanel
            // 
            this.CopyProgressBarOutlinePanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.CopyProgressBarOutlinePanel.Controls.Add(this.CopyingProgressBar);
            this.CopyProgressBarOutlinePanel.Location = new System.Drawing.Point(31, 63);
            this.CopyProgressBarOutlinePanel.Name = "CopyProgressBarOutlinePanel";
            this.CopyProgressBarOutlinePanel.Size = new System.Drawing.Size(394, 25);
            this.CopyProgressBarOutlinePanel.TabIndex = 2;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(456, 115);
            this.Controls.Add(this.CopyProgressBarOutlinePanel);
            this.Controls.Add(this.CopyingLabel);
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

    private DarkLabel CopyingLabel;
    private DarkProgressBar CopyingProgressBar;
    private System.Windows.Forms.Panel CopyProgressBarOutlinePanel;
}
