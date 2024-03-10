namespace AngelLoader.Forms.CustomControls;

public sealed partial class Lazy_StatsPage
{
    /// <summary>
    /// Custom generated component initializer with cruft removed.
    /// </summary>
    private void InitSlim()
    {
        this.Stats_MisCountLabel = new AngelLoader.Forms.CustomControls.DarkLabel();
        this.StatsScanCustomResourcesButton = new AngelLoader.Forms.CustomControls.DarkButton();
        this.StatsCheckBoxesPanel = new AngelLoader.Forms.CustomControls.DrawnPanel();
        this.CustomResourcesLabel = new AngelLoader.Forms.CustomControls.DarkLabel();
        this.SuspendLayout();
        // 
        // Stats_MisCountLabel
        // 
        this.Stats_MisCountLabel.AutoSize = true;
        this.Stats_MisCountLabel.Location = new System.Drawing.Point(4, 8);
        // 
        // StatsScanCustomResourcesButton
        // 
        this.StatsScanCustomResourcesButton.AutoSize = true;
        this.StatsScanCustomResourcesButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        this.StatsScanCustomResourcesButton.Location = new System.Drawing.Point(6, 232);
        this.StatsScanCustomResourcesButton.MinimumSize = new System.Drawing.Size(0, 23);
        this.StatsScanCustomResourcesButton.Padding = new System.Windows.Forms.Padding(13, 0, 0, 0);
        this.StatsScanCustomResourcesButton.TabIndex = 17;
        // 
        // StatsCheckBoxesPanel
        // 
        this.StatsCheckBoxesPanel.AutoSize = true;
        this.StatsCheckBoxesPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        this.StatsCheckBoxesPanel.Location = new System.Drawing.Point(8, 64);
        // 
        // CustomResourcesLabel
        // 
        this.CustomResourcesLabel.AutoSize = true;
        this.CustomResourcesLabel.Location = new System.Drawing.Point(4, 42);
        // 
        // Lazy_StatsPage
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
        this.AutoScroll = true;
        this.Controls.Add(this.Stats_MisCountLabel);
        this.Controls.Add(this.StatsScanCustomResourcesButton);
        this.Controls.Add(this.StatsCheckBoxesPanel);
        this.Controls.Add(this.CustomResourcesLabel);
        this.Size = new System.Drawing.Size(527, 284);
        this.ResumeLayout(false);
        this.PerformLayout();
    }
}
