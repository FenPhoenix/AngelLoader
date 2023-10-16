#define FenGen_DesignerSource

namespace AngelLoader.Forms;

sealed partial class TDM_Download_Main
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
            this.MissionBasicInfoValuesLabel = new AngelLoader.Forms.CustomControls.DarkLabel();
            this.MissionBasicInfoKeysLabel = new AngelLoader.Forms.CustomControls.DarkLabel();
            this.DownloadButton = new AngelLoader.Forms.CustomControls.DarkButton();
            this.DownloadListBox = new AngelLoader.Forms.CustomControls.DarkListBoxWithBackingItems();
            this.ServerListBox = new AngelLoader.Forms.CustomControls.DarkListBoxWithBackingItems();
            this.SelectForDownloadButton = new AngelLoader.Forms.CustomControls.DarkButton();
            this.SuspendLayout();
            // 
            // MissionBasicInfoValuesLabel
            // 
            this.MissionBasicInfoValuesLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.MissionBasicInfoValuesLabel.AutoSize = true;
            this.MissionBasicInfoValuesLabel.Location = new System.Drawing.Point(104, 448);
            this.MissionBasicInfoValuesLabel.Name = "MissionBasicInfoValuesLabel";
            this.MissionBasicInfoValuesLabel.Size = new System.Drawing.Size(67, 13);
            this.MissionBasicInfoValuesLabel.TabIndex = 11;
            this.MissionBasicInfoValuesLabel.Text = "[mission info]";
            // 
            // MissionBasicInfoKeysLabel
            // 
            this.MissionBasicInfoKeysLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.MissionBasicInfoKeysLabel.AutoSize = true;
            this.MissionBasicInfoKeysLabel.Location = new System.Drawing.Point(32, 448);
            this.MissionBasicInfoKeysLabel.Name = "MissionBasicInfoKeysLabel";
            this.MissionBasicInfoKeysLabel.Size = new System.Drawing.Size(67, 13);
            this.MissionBasicInfoKeysLabel.TabIndex = 12;
            this.MissionBasicInfoKeysLabel.Text = "[mission info]";
            // 
            // DownloadButton
            // 
            this.DownloadButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.DownloadButton.Location = new System.Drawing.Point(726, 448);
            this.DownloadButton.Name = "DownloadButton";
            this.DownloadButton.Size = new System.Drawing.Size(75, 23);
            this.DownloadButton.TabIndex = 9;
            this.DownloadButton.Text = "Download";
            // 
            // DownloadListBox
            // 
            this.DownloadListBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.DownloadListBox.Location = new System.Drawing.Point(448, 40);
            this.DownloadListBox.Name = "DownloadListBox";
            this.DownloadListBox.Size = new System.Drawing.Size(352, 400);
            this.DownloadListBox.TabIndex = 7;
            // 
            // ServerListBox
            // 
            this.ServerListBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.ServerListBox.Location = new System.Drawing.Point(32, 40);
            this.ServerListBox.Name = "ServerListBox";
            this.ServerListBox.Size = new System.Drawing.Size(352, 400);
            this.ServerListBox.TabIndex = 8;
            // 
            // SelectForDownloadButton
            // 
            this.SelectForDownloadButton.Location = new System.Drawing.Point(392, 216);
            this.SelectForDownloadButton.Name = "SelectForDownloadButton";
            this.SelectForDownloadButton.Size = new System.Drawing.Size(48, 23);
            this.SelectForDownloadButton.TabIndex = 6;
            // 
            // TDM_Download_Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.Controls.Add(this.MissionBasicInfoValuesLabel);
            this.Controls.Add(this.MissionBasicInfoKeysLabel);
            this.Controls.Add(this.DownloadButton);
            this.Controls.Add(this.DownloadListBox);
            this.Controls.Add(this.ServerListBox);
            this.Controls.Add(this.SelectForDownloadButton);
            this.Name = "TDM_Download_Main";
            this.Size = new System.Drawing.Size(830, 512);
            this.ResumeLayout(false);
            this.PerformLayout();

    }
#endif

    #endregion
    internal Forms.CustomControls.DarkLabel MissionBasicInfoValuesLabel;
    internal Forms.CustomControls.DarkLabel MissionBasicInfoKeysLabel;
    internal Forms.CustomControls.DarkButton DownloadButton;
    internal Forms.CustomControls.DarkListBoxWithBackingItems DownloadListBox;
    internal Forms.CustomControls.DarkListBoxWithBackingItems ServerListBox;
    internal Forms.CustomControls.DarkButton SelectForDownloadButton;
}
