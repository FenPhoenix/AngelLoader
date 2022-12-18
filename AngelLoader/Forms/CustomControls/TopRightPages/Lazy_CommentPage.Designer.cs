#define FenGen_DesignerSource

namespace AngelLoader.Forms.CustomControls;

public sealed partial class Lazy_CommentPage
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
        this.CommentTextBox = new AngelLoader.Forms.CustomControls.DarkTextBox();
        this.SuspendLayout();
        // 
        // CommentTextBox
        // 
        this.CommentTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
        | System.Windows.Forms.AnchorStyles.Left)
        | System.Windows.Forms.AnchorStyles.Right)));
        this.CommentTextBox.Location = new System.Drawing.Point(8, 8);
        this.CommentTextBox.Multiline = true;
        this.CommentTextBox.Name = "CommentTextBox";
        this.CommentTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
        this.CommentTextBox.Size = new System.Drawing.Size(512, 266);
        this.CommentTextBox.TabIndex = 33;
        // 
        // Lazy_CommentPage
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
        this.Controls.Add(this.CommentTextBox);
        this.Name = "Lazy_CommentPage";
        this.Size = new System.Drawing.Size(527, 284);
        this.ResumeLayout(false);
        this.PerformLayout();

    }
#endif

    #endregion

    internal DarkTextBox CommentTextBox;
}
