#define FenGen_DesignerSource

namespace AngelLoader.Forms;

partial class AudioFilesPage
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
            this.PagePanel = new AngelLoader.Forms.CustomControls.PanelCustom();
            this.ND128_ConvertMP3sToWAVsOnInstallCheckBox = new AngelLoader.Forms.CustomControls.DarkCheckBox();
            this.GlobalGroupBox = new AngelLoader.Forms.CustomControls.DarkGroupBox();
            this.ConvertOGGsToWAVsOnInstallCheckBox = new AngelLoader.Forms.CustomControls.DarkCheckBox();
            this.ND127_ConvertWAVsTo16BitOnInstallCheckBox = new AngelLoader.Forms.CustomControls.DarkCheckBox();
            this.ND128GroupBox = new AngelLoader.Forms.CustomControls.DarkGroupBox();
            this.ND127GroupBox = new AngelLoader.Forms.CustomControls.DarkGroupBox();
            this.ND127_ConvertMP3sToWAVsOnInstallCheckBox = new AngelLoader.Forms.CustomControls.DarkCheckBox();
            this.ND128_ConvertWAVsTo16BitOnInstallCheckBox = new AngelLoader.Forms.CustomControls.DarkCheckBox();
            this.PagePanel.SuspendLayout();
            this.GlobalGroupBox.SuspendLayout();
            this.ND128GroupBox.SuspendLayout();
            this.ND127GroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // PagePanel
            // 
            this.PagePanel.AutoScroll = true;
            this.PagePanel.AutoScrollMinSize = new System.Drawing.Size(432, 0);
            this.PagePanel.Controls.Add(this.ND128GroupBox);
            this.PagePanel.Controls.Add(this.GlobalGroupBox);
            this.PagePanel.Controls.Add(this.ND127GroupBox);
            this.PagePanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PagePanel.Location = new System.Drawing.Point(0, 0);
            this.PagePanel.Name = "PagePanel";
            this.PagePanel.Size = new System.Drawing.Size(440, 479);
            this.PagePanel.TabIndex = 0;
            // 
            // ND128_ConvertMP3sToWAVsOnInstallCheckBox
            // 
            this.ND128_ConvertMP3sToWAVsOnInstallCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ND128_ConvertMP3sToWAVsOnInstallCheckBox.Location = new System.Drawing.Point(16, 48);
            this.ND128_ConvertMP3sToWAVsOnInstallCheckBox.Name = "ND128_ConvertMP3sToWAVsOnInstallCheckBox";
            this.ND128_ConvertMP3sToWAVsOnInstallCheckBox.Size = new System.Drawing.Size(400, 32);
            this.ND128_ConvertMP3sToWAVsOnInstallCheckBox.TabIndex = 0;
            this.ND128_ConvertMP3sToWAVsOnInstallCheckBox.Text = "Convert .mp3s to .wavs on install";
            // 
            // GlobalGroupBox
            // 
            this.GlobalGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.GlobalGroupBox.Controls.Add(this.ConvertOGGsToWAVsOnInstallCheckBox);
            this.GlobalGroupBox.Location = new System.Drawing.Point(8, 8);
            this.GlobalGroupBox.MinimumSize = new System.Drawing.Size(424, 0);
            this.GlobalGroupBox.Name = "GlobalGroupBox";
            this.GlobalGroupBox.Size = new System.Drawing.Size(424, 56);
            this.GlobalGroupBox.TabIndex = 0;
            this.GlobalGroupBox.TabStop = false;
            this.GlobalGroupBox.Text = "Global";
            // 
            // ConvertOGGsToWAVsOnInstallCheckBox
            // 
            this.ConvertOGGsToWAVsOnInstallCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ConvertOGGsToWAVsOnInstallCheckBox.Location = new System.Drawing.Point(16, 16);
            this.ConvertOGGsToWAVsOnInstallCheckBox.Name = "ConvertOGGsToWAVsOnInstallCheckBox";
            this.ConvertOGGsToWAVsOnInstallCheckBox.Size = new System.Drawing.Size(400, 32);
            this.ConvertOGGsToWAVsOnInstallCheckBox.TabIndex = 0;
            this.ConvertOGGsToWAVsOnInstallCheckBox.Text = "Convert .oggs to .wavs on install";
            // 
            // ND127_ConvertWAVsTo16BitOnInstallCheckBox
            // 
            this.ND127_ConvertWAVsTo16BitOnInstallCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ND127_ConvertWAVsTo16BitOnInstallCheckBox.Checked = true;
            this.ND127_ConvertWAVsTo16BitOnInstallCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ND127_ConvertWAVsTo16BitOnInstallCheckBox.Location = new System.Drawing.Point(16, 16);
            this.ND127_ConvertWAVsTo16BitOnInstallCheckBox.Name = "ND127_ConvertWAVsTo16BitOnInstallCheckBox";
            this.ND127_ConvertWAVsTo16BitOnInstallCheckBox.Size = new System.Drawing.Size(400, 32);
            this.ND127_ConvertWAVsTo16BitOnInstallCheckBox.TabIndex = 0;
            this.ND127_ConvertWAVsTo16BitOnInstallCheckBox.Text = "Convert .wavs to 16 bit on install";
            // 
            // ND128GroupBox
            // 
            this.ND128GroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ND128GroupBox.Controls.Add(this.ND128_ConvertWAVsTo16BitOnInstallCheckBox);
            this.ND128GroupBox.Controls.Add(this.ND128_ConvertMP3sToWAVsOnInstallCheckBox);
            this.ND128GroupBox.Location = new System.Drawing.Point(8, 176);
            this.ND128GroupBox.MinimumSize = new System.Drawing.Size(424, 0);
            this.ND128GroupBox.Name = "ND128GroupBox";
            this.ND128GroupBox.Size = new System.Drawing.Size(424, 88);
            this.ND128GroupBox.TabIndex = 2;
            this.ND128GroupBox.TabStop = false;
            this.ND128GroupBox.Text = "NewDark 1.28 and newer";
            // 
            // ND127GroupBox
            // 
            this.ND127GroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ND127GroupBox.Controls.Add(this.ND127_ConvertMP3sToWAVsOnInstallCheckBox);
            this.ND127GroupBox.Controls.Add(this.ND127_ConvertWAVsTo16BitOnInstallCheckBox);
            this.ND127GroupBox.Location = new System.Drawing.Point(8, 76);
            this.ND127GroupBox.MinimumSize = new System.Drawing.Size(424, 0);
            this.ND127GroupBox.Name = "ND127GroupBox";
            this.ND127GroupBox.Size = new System.Drawing.Size(424, 88);
            this.ND127GroupBox.TabIndex = 1;
            this.ND127GroupBox.TabStop = false;
            this.ND127GroupBox.Text = "NewDark 1.27 and older";
            // 
            // ND127_ConvertMP3sToWAVsOnInstallCheckBox
            // 
            this.ND127_ConvertMP3sToWAVsOnInstallCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ND127_ConvertMP3sToWAVsOnInstallCheckBox.Checked = true;
            this.ND127_ConvertMP3sToWAVsOnInstallCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ND127_ConvertMP3sToWAVsOnInstallCheckBox.Enabled = false;
            this.ND127_ConvertMP3sToWAVsOnInstallCheckBox.Location = new System.Drawing.Point(16, 48);
            this.ND127_ConvertMP3sToWAVsOnInstallCheckBox.Name = "ND127_ConvertMP3sToWAVsOnInstallCheckBox";
            this.ND127_ConvertMP3sToWAVsOnInstallCheckBox.Size = new System.Drawing.Size(400, 32);
            this.ND127_ConvertMP3sToWAVsOnInstallCheckBox.TabIndex = 1;
            this.ND127_ConvertMP3sToWAVsOnInstallCheckBox.Text = "Convert .mp3s to .wavs on install (required)";
            // 
            // ND128_ConvertWAVsTo16BitOnInstallCheckBox
            // 
            this.ND128_ConvertWAVsTo16BitOnInstallCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ND128_ConvertWAVsTo16BitOnInstallCheckBox.Enabled = false;
            this.ND128_ConvertWAVsTo16BitOnInstallCheckBox.Location = new System.Drawing.Point(16, 16);
            this.ND128_ConvertWAVsTo16BitOnInstallCheckBox.Name = "ND128_ConvertWAVsTo16BitOnInstallCheckBox";
            this.ND128_ConvertWAVsTo16BitOnInstallCheckBox.Size = new System.Drawing.Size(400, 32);
            this.ND128_ConvertWAVsTo16BitOnInstallCheckBox.TabIndex = 1;
            this.ND128_ConvertWAVsTo16BitOnInstallCheckBox.Text = "Convert .wavs to 16 bit on install (not required)";
            // 
            // AudioFilesPage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.PagePanel);
            this.Name = "AudioFilesPage";
            this.Size = new System.Drawing.Size(440, 479);
            this.PagePanel.ResumeLayout(false);
            this.GlobalGroupBox.ResumeLayout(false);
            this.ND128GroupBox.ResumeLayout(false);
            this.ND127GroupBox.ResumeLayout(false);
            this.ResumeLayout(false);

    }
#endif

    #endregion

    internal CustomControls.DarkGroupBox GlobalGroupBox;
    internal CustomControls.DarkCheckBox ConvertOGGsToWAVsOnInstallCheckBox;
    internal AngelLoader.Forms.CustomControls.PanelCustom PagePanel;
    internal CustomControls.DarkCheckBox ND128_ConvertMP3sToWAVsOnInstallCheckBox;
    internal CustomControls.DarkCheckBox ND127_ConvertWAVsTo16BitOnInstallCheckBox;
    internal CustomControls.DarkGroupBox ND128GroupBox;
    internal CustomControls.DarkCheckBox ND128_ConvertWAVsTo16BitOnInstallCheckBox;
    internal CustomControls.DarkGroupBox ND127GroupBox;
    internal CustomControls.DarkCheckBox ND127_ConvertMP3sToWAVsOnInstallCheckBox;
}
