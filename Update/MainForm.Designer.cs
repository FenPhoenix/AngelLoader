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
        this.CopyProgressBar = new Update.DarkProgressBar();
        this.Message2Label = new Update.DarkLabel();
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
        // CopyProgressBar
        // 
        this.CopyProgressBar.Location = new System.Drawing.Point(32, 64);
        this.CopyProgressBar.Name = "CopyProgressBar";
        this.CopyProgressBar.Size = new System.Drawing.Size(504, 23);
        this.CopyProgressBar.TabIndex = 0;
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
        this.Controls.Add(this.CopyProgressBar);
        this.Controls.Add(this.Message2Label);
        this.Controls.Add(this.Message1Label);
        this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
        this.MaximizeBox = false;
        this.Name = "MainForm";
        this.ShowInTaskbar = true;
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
        this.Text = "AngelLoader Update";
        this.ResumeLayout(false);
        this.PerformLayout();

    }
#endif

    #endregion

    private DarkLabel Message1Label;
    private DarkProgressBar CopyProgressBar;
    private DarkLabel Message2Label;
}
