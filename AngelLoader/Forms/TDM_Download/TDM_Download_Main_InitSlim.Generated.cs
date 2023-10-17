namespace AngelLoader.Forms;

sealed partial class TDM_Download_Main
{
    /// <summary>
    /// Custom generated component initializer with cruft removed.
    /// </summary>
    private void InitSlim()
    {
        this.MissionBasicInfoValuesLabel = new AngelLoader.Forms.CustomControls.DarkLabel();
        this.MissionBasicInfoKeysLabel = new AngelLoader.Forms.CustomControls.DarkLabel();
        this.DownloadButton = new AngelLoader.Forms.CustomControls.DarkButton();
        this.DownloadListBox = new AngelLoader.Forms.CustomControls.DarkListBoxWithBackingItems();
        this.ServerListBox = new AngelLoader.Forms.CustomControls.DarkListBoxWithBackingItems();
        this.SelectForDownloadButton = new AngelLoader.Forms.CustomControls.DarkButton();
        this.ProgressLabel = new AngelLoader.Forms.CustomControls.DarkLabel();
        this.SuspendLayout();
        // 
        // MissionBasicInfoValuesLabel
        // 
        this.MissionBasicInfoValuesLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
        this.MissionBasicInfoValuesLabel.AutoSize = true;
        this.MissionBasicInfoValuesLabel.Location = new System.Drawing.Point(104, 448);
        this.MissionBasicInfoValuesLabel.Size = new System.Drawing.Size(67, 13);
        // 
        // MissionBasicInfoKeysLabel
        // 
        this.MissionBasicInfoKeysLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
        this.MissionBasicInfoKeysLabel.AutoSize = true;
        this.MissionBasicInfoKeysLabel.Location = new System.Drawing.Point(32, 448);
        this.MissionBasicInfoKeysLabel.Size = new System.Drawing.Size(67, 13);
        // 
        // DownloadButton
        // 
        this.DownloadButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
        this.DownloadButton.Location = new System.Drawing.Point(726, 448);
        this.DownloadButton.Size = new System.Drawing.Size(75, 23);
        this.DownloadButton.TabIndex = 9;
        // 
        // DownloadListBox
        // 
        this.DownloadListBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
        | System.Windows.Forms.AnchorStyles.Left)));
        this.DownloadListBox.Location = new System.Drawing.Point(448, 40);
        this.DownloadListBox.Size = new System.Drawing.Size(352, 400);
        this.DownloadListBox.TabIndex = 7;
        // 
        // ServerListBox
        // 
        this.ServerListBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
        | System.Windows.Forms.AnchorStyles.Left)));
        this.ServerListBox.Location = new System.Drawing.Point(32, 40);
        this.ServerListBox.Size = new System.Drawing.Size(352, 400);
        this.ServerListBox.TabIndex = 8;
        // 
        // SelectForDownloadButton
        // 
        this.SelectForDownloadButton.Location = new System.Drawing.Point(392, 216);
        this.SelectForDownloadButton.Size = new System.Drawing.Size(48, 23);
        this.SelectForDownloadButton.TabIndex = 6;
        // 
        // ProgressLabel
        // 
        this.ProgressLabel.AutoSize = true;
        this.ProgressLabel.Location = new System.Drawing.Point(448, 448);
        // 
        // TDM_Download_Main
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
        this.Controls.Add(this.ProgressLabel);
        this.Controls.Add(this.MissionBasicInfoValuesLabel);
        this.Controls.Add(this.MissionBasicInfoKeysLabel);
        this.Controls.Add(this.DownloadButton);
        this.Controls.Add(this.DownloadListBox);
        this.Controls.Add(this.ServerListBox);
        this.Controls.Add(this.SelectForDownloadButton);
        this.Size = new System.Drawing.Size(830, 512);
        this.ResumeLayout(false);
        this.PerformLayout();
    }
}
