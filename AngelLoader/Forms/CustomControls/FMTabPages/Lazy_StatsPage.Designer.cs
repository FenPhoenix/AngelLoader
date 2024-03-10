#define FenGen_DesignerSource

namespace AngelLoader.Forms.CustomControls;

public sealed partial class Lazy_StatsPage
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
        this.Stats_MisCountLabel.Name = "Stats_MisCountLabel";
        this.Stats_MisCountLabel.Size = new System.Drawing.Size(77, 13);
        this.Stats_MisCountLabel.TabIndex = 14;
        this.Stats_MisCountLabel.Text = "[mission count]";
        // 
        // StatsScanCustomResourcesButton
        // 
        this.StatsScanCustomResourcesButton.AutoSize = true;
        this.StatsScanCustomResourcesButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        this.StatsScanCustomResourcesButton.Location = new System.Drawing.Point(6, 232);
        this.StatsScanCustomResourcesButton.MinimumSize = new System.Drawing.Size(0, 23);
        this.StatsScanCustomResourcesButton.Name = "StatsScanCustomResourcesButton";
        this.StatsScanCustomResourcesButton.Padding = new System.Windows.Forms.Padding(13, 0, 0, 0);
        this.StatsScanCustomResourcesButton.Size = new System.Drawing.Size(110, 23);
        this.StatsScanCustomResourcesButton.TabIndex = 17;
        this.StatsScanCustomResourcesButton.Text = "Rescan statistics";
        // 
        // StatsCheckBoxesPanel
        // 
        this.StatsCheckBoxesPanel.AutoSize = true;
        this.StatsCheckBoxesPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        this.StatsCheckBoxesPanel.Location = new System.Drawing.Point(8, 64);
        this.StatsCheckBoxesPanel.Name = "StatsCheckBoxesPanel";
        this.StatsCheckBoxesPanel.Size = new System.Drawing.Size(0, 0);
        this.StatsCheckBoxesPanel.TabIndex = 16;
        // 
        // CustomResourcesLabel
        // 
        this.CustomResourcesLabel.AutoSize = true;
        this.CustomResourcesLabel.Location = new System.Drawing.Point(4, 42);
        this.CustomResourcesLabel.Name = "CustomResourcesLabel";
        this.CustomResourcesLabel.Size = new System.Drawing.Size(156, 13);
        this.CustomResourcesLabel.TabIndex = 15;
        this.CustomResourcesLabel.Text = "Custom resources not scanned.";
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
        this.Name = "Lazy_StatsPage";
        this.Size = new System.Drawing.Size(527, 284);
        this.ResumeLayout(false);
        this.PerformLayout();

    }
#endif

    #endregion

    internal DarkLabel Stats_MisCountLabel;
    internal DarkButton StatsScanCustomResourcesButton;
    internal DrawnPanel StatsCheckBoxesPanel;
    internal DarkLabel CustomResourcesLabel;
}
