namespace Update;

sealed partial class MainForm
{
    /// <summary>
    ///  Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    ///  Clean up any resources being used.
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
    ///  Required method for Designer support - do not modify
    ///  the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.CopyingLabel = new System.Windows.Forms.Label();
            this.CopyingProgressBar = new System.Windows.Forms.ProgressBar();
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
            this.CopyingLabel.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // CopyingProgressBar
            // 
            this.CopyingProgressBar.Location = new System.Drawing.Point(32, 64);
            this.CopyingProgressBar.Name = "CopyingProgressBar";
            this.CopyingProgressBar.Size = new System.Drawing.Size(392, 23);
            this.CopyingProgressBar.TabIndex = 1;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(456, 115);
            this.Controls.Add(this.CopyingProgressBar);
            this.Controls.Add(this.CopyingLabel);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "AngelLoader Update";
            this.ResumeLayout(false);
            this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Label CopyingLabel;
    private System.Windows.Forms.ProgressBar CopyingProgressBar;
}
