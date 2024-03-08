namespace AngelLoader.Forms.CustomControls;

public sealed partial class Lazy_ModsPage
{
    /// <summary>
    /// Custom generated component initializer with cruft removed.
    /// </summary>
    private void InitSlim()
    {
        this.MainModsControl = new AngelLoader.Forms.CustomControls.ModsControl();
        this.SuspendLayout();
        // 
        // MainModsControl
        // 
        this.MainModsControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
        | System.Windows.Forms.AnchorStyles.Left)
        | System.Windows.Forms.AnchorStyles.Right)));
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
        this.Size = new System.Drawing.Size(527, 284);
        this.ResumeLayout(false);
    }
}
