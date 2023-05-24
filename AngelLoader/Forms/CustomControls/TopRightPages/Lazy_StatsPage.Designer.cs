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
        this.CR_MapCheckBox = new AngelLoader.Forms.CustomControls.DarkCheckBox();
        this.CR_MoviesCheckBox = new AngelLoader.Forms.CustomControls.DarkCheckBox();
        this.CR_MotionsCheckBox = new AngelLoader.Forms.CustomControls.DarkCheckBox();
        this.CR_SoundsCheckBox = new AngelLoader.Forms.CustomControls.DarkCheckBox();
        this.CR_CreaturesCheckBox = new AngelLoader.Forms.CustomControls.DarkCheckBox();
        this.CR_TexturesCheckBox = new AngelLoader.Forms.CustomControls.DarkCheckBox();
        this.CR_AutomapCheckBox = new AngelLoader.Forms.CustomControls.DarkCheckBox();
        this.CR_ScriptsCheckBox = new AngelLoader.Forms.CustomControls.DarkCheckBox();
        this.CR_SubtitlesCheckBox = new AngelLoader.Forms.CustomControls.DarkCheckBox();
        this.CR_ObjectsCheckBox = new AngelLoader.Forms.CustomControls.DarkCheckBox();
        this.CustomResourcesLabel = new AngelLoader.Forms.CustomControls.DarkLabel();
        this.StatsCheckBoxesPanel.SuspendLayout();
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
        this.StatsCheckBoxesPanel.Controls.Add(this.CR_MapCheckBox);
        this.StatsCheckBoxesPanel.Controls.Add(this.CR_MoviesCheckBox);
        this.StatsCheckBoxesPanel.Controls.Add(this.CR_MotionsCheckBox);
        this.StatsCheckBoxesPanel.Controls.Add(this.CR_SoundsCheckBox);
        this.StatsCheckBoxesPanel.Controls.Add(this.CR_CreaturesCheckBox);
        this.StatsCheckBoxesPanel.Controls.Add(this.CR_TexturesCheckBox);
        this.StatsCheckBoxesPanel.Controls.Add(this.CR_AutomapCheckBox);
        this.StatsCheckBoxesPanel.Controls.Add(this.CR_ScriptsCheckBox);
        this.StatsCheckBoxesPanel.Controls.Add(this.CR_SubtitlesCheckBox);
        this.StatsCheckBoxesPanel.Controls.Add(this.CR_ObjectsCheckBox);
        this.StatsCheckBoxesPanel.Location = new System.Drawing.Point(8, 64);
        this.StatsCheckBoxesPanel.Name = "StatsCheckBoxesPanel";
        this.StatsCheckBoxesPanel.Size = new System.Drawing.Size(74, 164);
        this.StatsCheckBoxesPanel.TabIndex = 16;
        // 
        // CR_MapCheckBox
        // 
        this.CR_MapCheckBox.AutoCheck = false;
        this.CR_MapCheckBox.AutoSize = true;
        this.CR_MapCheckBox.Location = new System.Drawing.Point(0, 0);
        this.CR_MapCheckBox.Name = "CR_MapCheckBox";
        this.CR_MapCheckBox.Size = new System.Drawing.Size(47, 17);
        this.CR_MapCheckBox.TabIndex = 2;
        this.CR_MapCheckBox.Text = "Map";
        // 
        // CR_MoviesCheckBox
        // 
        this.CR_MoviesCheckBox.AutoCheck = false;
        this.CR_MoviesCheckBox.AutoSize = true;
        this.CR_MoviesCheckBox.Location = new System.Drawing.Point(0, 64);
        this.CR_MoviesCheckBox.Name = "CR_MoviesCheckBox";
        this.CR_MoviesCheckBox.Size = new System.Drawing.Size(60, 17);
        this.CR_MoviesCheckBox.TabIndex = 6;
        this.CR_MoviesCheckBox.Text = "Movies";
        // 
        // CR_MotionsCheckBox
        // 
        this.CR_MotionsCheckBox.AutoCheck = false;
        this.CR_MotionsCheckBox.AutoSize = true;
        this.CR_MotionsCheckBox.Location = new System.Drawing.Point(0, 112);
        this.CR_MotionsCheckBox.Name = "CR_MotionsCheckBox";
        this.CR_MotionsCheckBox.Size = new System.Drawing.Size(63, 17);
        this.CR_MotionsCheckBox.TabIndex = 9;
        this.CR_MotionsCheckBox.Text = "Motions";
        // 
        // CR_SoundsCheckBox
        // 
        this.CR_SoundsCheckBox.AutoCheck = false;
        this.CR_SoundsCheckBox.AutoSize = true;
        this.CR_SoundsCheckBox.Location = new System.Drawing.Point(0, 48);
        this.CR_SoundsCheckBox.Name = "CR_SoundsCheckBox";
        this.CR_SoundsCheckBox.Size = new System.Drawing.Size(62, 17);
        this.CR_SoundsCheckBox.TabIndex = 5;
        this.CR_SoundsCheckBox.Text = "Sounds";
        // 
        // CR_CreaturesCheckBox
        // 
        this.CR_CreaturesCheckBox.AutoCheck = false;
        this.CR_CreaturesCheckBox.AutoSize = true;
        this.CR_CreaturesCheckBox.Location = new System.Drawing.Point(0, 96);
        this.CR_CreaturesCheckBox.Name = "CR_CreaturesCheckBox";
        this.CR_CreaturesCheckBox.Size = new System.Drawing.Size(71, 17);
        this.CR_CreaturesCheckBox.TabIndex = 8;
        this.CR_CreaturesCheckBox.Text = "Creatures";
        // 
        // CR_TexturesCheckBox
        // 
        this.CR_TexturesCheckBox.AutoCheck = false;
        this.CR_TexturesCheckBox.AutoSize = true;
        this.CR_TexturesCheckBox.Location = new System.Drawing.Point(0, 32);
        this.CR_TexturesCheckBox.Name = "CR_TexturesCheckBox";
        this.CR_TexturesCheckBox.Size = new System.Drawing.Size(67, 17);
        this.CR_TexturesCheckBox.TabIndex = 4;
        this.CR_TexturesCheckBox.Text = "Textures";
        // 
        // CR_AutomapCheckBox
        // 
        this.CR_AutomapCheckBox.AutoCheck = false;
        this.CR_AutomapCheckBox.AutoSize = true;
        this.CR_AutomapCheckBox.Location = new System.Drawing.Point(0, 16);
        this.CR_AutomapCheckBox.Name = "CR_AutomapCheckBox";
        this.CR_AutomapCheckBox.Size = new System.Drawing.Size(68, 17);
        this.CR_AutomapCheckBox.TabIndex = 3;
        this.CR_AutomapCheckBox.Text = "Automap";
        // 
        // CR_ScriptsCheckBox
        // 
        this.CR_ScriptsCheckBox.AutoCheck = false;
        this.CR_ScriptsCheckBox.AutoSize = true;
        this.CR_ScriptsCheckBox.Location = new System.Drawing.Point(0, 128);
        this.CR_ScriptsCheckBox.Name = "CR_ScriptsCheckBox";
        this.CR_ScriptsCheckBox.Size = new System.Drawing.Size(58, 17);
        this.CR_ScriptsCheckBox.TabIndex = 10;
        this.CR_ScriptsCheckBox.Text = "Scripts";
        // 
        // CR_SubtitlesCheckBox
        // 
        this.CR_SubtitlesCheckBox.AutoCheck = false;
        this.CR_SubtitlesCheckBox.AutoSize = true;
        this.CR_SubtitlesCheckBox.Location = new System.Drawing.Point(0, 144);
        this.CR_SubtitlesCheckBox.Name = "CR_SubtitlesCheckBox";
        this.CR_SubtitlesCheckBox.Size = new System.Drawing.Size(66, 17);
        this.CR_SubtitlesCheckBox.TabIndex = 11;
        this.CR_SubtitlesCheckBox.Text = "Subtitles";
        // 
        // CR_ObjectsCheckBox
        // 
        this.CR_ObjectsCheckBox.AutoCheck = false;
        this.CR_ObjectsCheckBox.AutoSize = true;
        this.CR_ObjectsCheckBox.Location = new System.Drawing.Point(0, 80);
        this.CR_ObjectsCheckBox.Name = "CR_ObjectsCheckBox";
        this.CR_ObjectsCheckBox.Size = new System.Drawing.Size(62, 17);
        this.CR_ObjectsCheckBox.TabIndex = 7;
        this.CR_ObjectsCheckBox.Text = "Objects";
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
        this.StatsCheckBoxesPanel.ResumeLayout(false);
        this.StatsCheckBoxesPanel.PerformLayout();
        this.ResumeLayout(false);
        this.PerformLayout();

    }
#endif

    #endregion

    internal DarkLabel Stats_MisCountLabel;
    internal DarkButton StatsScanCustomResourcesButton;
    internal DrawnPanel StatsCheckBoxesPanel;
    internal DarkCheckBox CR_MapCheckBox;
    internal DarkCheckBox CR_MoviesCheckBox;
    internal DarkCheckBox CR_MotionsCheckBox;
    internal DarkCheckBox CR_SoundsCheckBox;
    internal DarkCheckBox CR_CreaturesCheckBox;
    internal DarkCheckBox CR_TexturesCheckBox;
    internal DarkCheckBox CR_AutomapCheckBox;
    internal DarkCheckBox CR_ScriptsCheckBox;
    internal DarkCheckBox CR_SubtitlesCheckBox;
    internal DarkCheckBox CR_ObjectsCheckBox;
    internal DarkLabel CustomResourcesLabel;
}
