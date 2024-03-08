#define FenGen_DesignerSource

namespace AngelLoader.Forms.CustomControls;

public sealed partial class Lazy_ModsPage
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
        this.MainModsControl = new AngelLoader.Forms.CustomControls.ModsControl();
        this.SuspendLayout();
        // 
        // MainModsControl
        // 
        this.MainModsControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
        | System.Windows.Forms.AnchorStyles.Left)
        | System.Windows.Forms.AnchorStyles.Right)));
        this.MainModsControl.Location = new System.Drawing.Point(0, 0);
        this.MainModsControl.Name = "MainModsControl";
        this.MainModsControl.Size = new System.Drawing.Size(527, 284);
        this.MainModsControl.TabIndex = 7;
        this.MainModsControl.Tag = AngelLoader.Forms.LoadType.Lazy;
        // 
        // Lazy_ModsPage
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
        this.AutoScroll = true;
        this.Controls.Add(this.MainModsControl);
        this.Name = "Lazy_ModsPage";
        this.Size = new System.Drawing.Size(527, 284);
        this.ResumeLayout(false);

    }
#endif

    #endregion

    internal ModsControl MainModsControl;
}
