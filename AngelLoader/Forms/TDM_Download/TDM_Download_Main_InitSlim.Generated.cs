namespace AngelLoader.Forms;

sealed partial class TDM_Download_Main
{
    /// <summary>
    /// Custom generated component initializer with cruft removed.
    /// </summary>
    private void InitSlim()
    {
        this.darkFlowLayoutPanel2 = new AngelLoader.Forms.CustomControls.DarkFlowLayoutPanel();
        this.DownloadButton = new AngelLoader.Forms.CustomControls.DarkButton();
        this.darkFlowLayoutPanel1 = new AngelLoader.Forms.CustomControls.DarkFlowLayoutPanel();
        this.SortByLabel = new AngelLoader.Forms.CustomControls.DarkLabel();
        this.SortByDateCheckBox = new AngelLoader.Forms.CustomControls.DarkRadioButton();
        this.SortByTitleCheckBox = new AngelLoader.Forms.CustomControls.DarkRadioButton();
        this.SelectedForDownloadLabel = new AngelLoader.Forms.CustomControls.DarkLabel();
        this.DownloadableMissionsLabel = new AngelLoader.Forms.CustomControls.DarkLabel();
        this.MissionBasicInfoValuesLabel = new AngelLoader.Forms.CustomControls.DarkLabel();
        this.MissionBasicInfoKeysLabel = new AngelLoader.Forms.CustomControls.DarkLabel();
        this.DownloadListBox = new AngelLoader.Forms.CustomControls.DarkListBoxWithBackingItems();
        this.ServerListBox = new AngelLoader.Forms.CustomControls.DarkListBoxWithBackingItems();
        this.UnselectForDownloadButton = new AngelLoader.Forms.CustomControls.DarkArrowButton();
        this.SelectForDownloadButton = new AngelLoader.Forms.CustomControls.DarkArrowButton();
        this.darkFlowLayoutPanel2.SuspendLayout();
        this.darkFlowLayoutPanel1.SuspendLayout();
        this.SuspendLayout();
        // 
        // darkFlowLayoutPanel2
        // 
        this.darkFlowLayoutPanel2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
        this.darkFlowLayoutPanel2.Controls.Add(this.DownloadButton);
        this.darkFlowLayoutPanel2.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
        this.darkFlowLayoutPanel2.Location = new System.Drawing.Point(448, 504);
        this.darkFlowLayoutPanel2.Size = new System.Drawing.Size(353, 24);
        this.darkFlowLayoutPanel2.TabIndex = 17;
        // 
        // DownloadButton
        // 
        this.DownloadButton.Margin = new System.Windows.Forms.Padding(0);
        this.DownloadButton.Size = new System.Drawing.Size(75, 23);
        this.DownloadButton.TabIndex = 9;
        // 
        // darkFlowLayoutPanel1
        // 
        this.darkFlowLayoutPanel1.Controls.Add(this.SortByLabel);
        this.darkFlowLayoutPanel1.Controls.Add(this.SortByDateCheckBox);
        this.darkFlowLayoutPanel1.Controls.Add(this.SortByTitleCheckBox);
        this.darkFlowLayoutPanel1.Location = new System.Drawing.Point(32, 32);
        this.darkFlowLayoutPanel1.Size = new System.Drawing.Size(352, 24);
        this.darkFlowLayoutPanel1.TabIndex = 16;
        // 
        // SortByLabel
        // 
        this.SortByLabel.AutoSize = true;
        this.SortByLabel.Margin = new System.Windows.Forms.Padding(3, 4, 3, 0);
        // 
        // SortByDateCheckBox
        // 
        this.SortByDateCheckBox.AutoSize = true;
        this.SortByDateCheckBox.Checked = true;
        this.SortByDateCheckBox.TabIndex = 0;
        this.SortByDateCheckBox.TabStop = true;
        // 
        // SortByTitleCheckBox
        // 
        this.SortByTitleCheckBox.AutoSize = true;
        this.SortByTitleCheckBox.TabIndex = 0;
        // 
        // SelectedForDownloadLabel
        // 
        this.SelectedForDownloadLabel.AutoSize = true;
        this.SelectedForDownloadLabel.Location = new System.Drawing.Point(448, 24);
        // 
        // DownloadableMissionsLabel
        // 
        this.DownloadableMissionsLabel.AutoSize = true;
        this.DownloadableMissionsLabel.Location = new System.Drawing.Point(32, 16);
        // 
        // MissionBasicInfoValuesLabel
        // 
        this.MissionBasicInfoValuesLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
        this.MissionBasicInfoValuesLabel.AutoSize = true;
        this.MissionBasicInfoValuesLabel.Location = new System.Drawing.Point(104, 504);
        this.MissionBasicInfoValuesLabel.Size = new System.Drawing.Size(67, 13);
        // 
        // MissionBasicInfoKeysLabel
        // 
        this.MissionBasicInfoKeysLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
        this.MissionBasicInfoKeysLabel.AutoSize = true;
        this.MissionBasicInfoKeysLabel.Location = new System.Drawing.Point(32, 504);
        this.MissionBasicInfoKeysLabel.Size = new System.Drawing.Size(67, 13);
        // 
        // DownloadListBox
        // 
        this.DownloadListBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
        | System.Windows.Forms.AnchorStyles.Left)));
        this.DownloadListBox.Location = new System.Drawing.Point(448, 56);
        this.DownloadListBox.MultiSelect = false;
        this.DownloadListBox.Size = new System.Drawing.Size(352, 440);
        this.DownloadListBox.TabIndex = 7;
        // 
        // ServerListBox
        // 
        this.ServerListBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
        | System.Windows.Forms.AnchorStyles.Left)));
        this.ServerListBox.Location = new System.Drawing.Point(32, 56);
        this.ServerListBox.MultiSelect = false;
        this.ServerListBox.Size = new System.Drawing.Size(352, 440);
        this.ServerListBox.TabIndex = 8;
        // 
        // UnselectForDownloadButton
        // 
        this.UnselectForDownloadButton.ArrowDirection = AngelLoader.Forms.Direction.Left;
        this.UnselectForDownloadButton.Location = new System.Drawing.Point(392, 248);
        this.UnselectForDownloadButton.Size = new System.Drawing.Size(48, 23);
        this.UnselectForDownloadButton.TabIndex = 6;
        // 
        // SelectForDownloadButton
        // 
        this.SelectForDownloadButton.ArrowDirection = AngelLoader.Forms.Direction.Right;
        this.SelectForDownloadButton.Location = new System.Drawing.Point(392, 224);
        this.SelectForDownloadButton.Size = new System.Drawing.Size(48, 23);
        this.SelectForDownloadButton.TabIndex = 6;
        // 
        // TDM_Download_Main
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
        this.Controls.Add(this.darkFlowLayoutPanel2);
        this.Controls.Add(this.darkFlowLayoutPanel1);
        this.Controls.Add(this.SelectedForDownloadLabel);
        this.Controls.Add(this.DownloadableMissionsLabel);
        this.Controls.Add(this.MissionBasicInfoValuesLabel);
        this.Controls.Add(this.MissionBasicInfoKeysLabel);
        this.Controls.Add(this.DownloadListBox);
        this.Controls.Add(this.ServerListBox);
        this.Controls.Add(this.UnselectForDownloadButton);
        this.Controls.Add(this.SelectForDownloadButton);
        this.Size = new System.Drawing.Size(830, 566);
        this.darkFlowLayoutPanel2.ResumeLayout(false);
        this.darkFlowLayoutPanel1.ResumeLayout(false);
        this.darkFlowLayoutPanel1.PerformLayout();
        this.ResumeLayout(false);
        this.PerformLayout();
    }
}
