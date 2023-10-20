﻿#define FenGen_DesignerSource

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
            this.darkFlowLayoutPanel2.Name = "darkFlowLayoutPanel2";
            this.darkFlowLayoutPanel2.Size = new System.Drawing.Size(353, 24);
            this.darkFlowLayoutPanel2.TabIndex = 17;
            // 
            // DownloadButton
            // 
            this.DownloadButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.DownloadButton.Location = new System.Drawing.Point(278, 0);
            this.DownloadButton.Margin = new System.Windows.Forms.Padding(0);
            this.DownloadButton.Name = "DownloadButton";
            this.DownloadButton.Size = new System.Drawing.Size(75, 23);
            this.DownloadButton.TabIndex = 9;
            this.DownloadButton.Text = "Download";
            // 
            // darkFlowLayoutPanel1
            // 
            this.darkFlowLayoutPanel1.Controls.Add(this.SortByLabel);
            this.darkFlowLayoutPanel1.Controls.Add(this.SortByDateCheckBox);
            this.darkFlowLayoutPanel1.Controls.Add(this.SortByTitleCheckBox);
            this.darkFlowLayoutPanel1.Location = new System.Drawing.Point(32, 32);
            this.darkFlowLayoutPanel1.Name = "darkFlowLayoutPanel1";
            this.darkFlowLayoutPanel1.Size = new System.Drawing.Size(352, 24);
            this.darkFlowLayoutPanel1.TabIndex = 16;
            // 
            // SortByLabel
            // 
            this.SortByLabel.AutoSize = true;
            this.SortByLabel.Location = new System.Drawing.Point(3, 4);
            this.SortByLabel.Margin = new System.Windows.Forms.Padding(3, 4, 3, 0);
            this.SortByLabel.Name = "SortByLabel";
            this.SortByLabel.Size = new System.Drawing.Size(43, 13);
            this.SortByLabel.TabIndex = 1;
            this.SortByLabel.Text = "Sort by:";
            // 
            // SortByDateCheckBox
            // 
            this.SortByDateCheckBox.AutoSize = true;
            this.SortByDateCheckBox.Checked = true;
            this.SortByDateCheckBox.Location = new System.Drawing.Point(52, 3);
            this.SortByDateCheckBox.Name = "SortByDateCheckBox";
            this.SortByDateCheckBox.Size = new System.Drawing.Size(48, 17);
            this.SortByDateCheckBox.TabIndex = 0;
            this.SortByDateCheckBox.TabStop = true;
            this.SortByDateCheckBox.Text = "Date";
            // 
            // SortByTitleCheckBox
            // 
            this.SortByTitleCheckBox.AutoSize = true;
            this.SortByTitleCheckBox.Location = new System.Drawing.Point(106, 3);
            this.SortByTitleCheckBox.Name = "SortByTitleCheckBox";
            this.SortByTitleCheckBox.Size = new System.Drawing.Size(45, 17);
            this.SortByTitleCheckBox.TabIndex = 0;
            this.SortByTitleCheckBox.Text = "Title";
            // 
            // SelectedForDownloadLabel
            // 
            this.SelectedForDownloadLabel.AutoSize = true;
            this.SelectedForDownloadLabel.Location = new System.Drawing.Point(448, 24);
            this.SelectedForDownloadLabel.Name = "SelectedForDownloadLabel";
            this.SelectedForDownloadLabel.Size = new System.Drawing.Size(116, 13);
            this.SelectedForDownloadLabel.TabIndex = 15;
            this.SelectedForDownloadLabel.Text = "Selected for download:";
            // 
            // DownloadableMissionsLabel
            // 
            this.DownloadableMissionsLabel.AutoSize = true;
            this.DownloadableMissionsLabel.Location = new System.Drawing.Point(32, 16);
            this.DownloadableMissionsLabel.Name = "DownloadableMissionsLabel";
            this.DownloadableMissionsLabel.Size = new System.Drawing.Size(120, 13);
            this.DownloadableMissionsLabel.TabIndex = 15;
            this.DownloadableMissionsLabel.Text = "Downloadable missions:";
            // 
            // MissionBasicInfoValuesLabel
            // 
            this.MissionBasicInfoValuesLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.MissionBasicInfoValuesLabel.AutoSize = true;
            this.MissionBasicInfoValuesLabel.Location = new System.Drawing.Point(104, 504);
            this.MissionBasicInfoValuesLabel.Name = "MissionBasicInfoValuesLabel";
            this.MissionBasicInfoValuesLabel.Size = new System.Drawing.Size(67, 13);
            this.MissionBasicInfoValuesLabel.TabIndex = 11;
            this.MissionBasicInfoValuesLabel.Text = "[mission info]";
            // 
            // MissionBasicInfoKeysLabel
            // 
            this.MissionBasicInfoKeysLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.MissionBasicInfoKeysLabel.AutoSize = true;
            this.MissionBasicInfoKeysLabel.Location = new System.Drawing.Point(32, 504);
            this.MissionBasicInfoKeysLabel.Name = "MissionBasicInfoKeysLabel";
            this.MissionBasicInfoKeysLabel.Size = new System.Drawing.Size(67, 13);
            this.MissionBasicInfoKeysLabel.TabIndex = 12;
            this.MissionBasicInfoKeysLabel.Text = "[mission info]";
            // 
            // DownloadListBox
            // 
            this.DownloadListBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.DownloadListBox.Location = new System.Drawing.Point(448, 56);
            this.DownloadListBox.MultiSelect = false;
            this.DownloadListBox.Name = "DownloadListBox";
            this.DownloadListBox.Size = new System.Drawing.Size(352, 440);
            this.DownloadListBox.TabIndex = 7;
            // 
            // ServerListBox
            // 
            this.ServerListBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.ServerListBox.Location = new System.Drawing.Point(32, 56);
            this.ServerListBox.MultiSelect = false;
            this.ServerListBox.Name = "ServerListBox";
            this.ServerListBox.Size = new System.Drawing.Size(352, 440);
            this.ServerListBox.TabIndex = 8;
            // 
            // UnselectForDownloadButton
            // 
            this.UnselectForDownloadButton.ArrowDirection = AngelLoader.Forms.Direction.Left;
            this.UnselectForDownloadButton.Location = new System.Drawing.Point(392, 248);
            this.UnselectForDownloadButton.Name = "UnselectForDownloadButton";
            this.UnselectForDownloadButton.Size = new System.Drawing.Size(48, 23);
            this.UnselectForDownloadButton.TabIndex = 6;
            // 
            // SelectForDownloadButton
            // 
            this.SelectForDownloadButton.ArrowDirection = AngelLoader.Forms.Direction.Right;
            this.SelectForDownloadButton.Location = new System.Drawing.Point(392, 224);
            this.SelectForDownloadButton.Name = "SelectForDownloadButton";
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
            this.Name = "TDM_Download_Main";
            this.Size = new System.Drawing.Size(830, 566);
            this.darkFlowLayoutPanel2.ResumeLayout(false);
            this.darkFlowLayoutPanel1.ResumeLayout(false);
            this.darkFlowLayoutPanel1.PerformLayout();
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
    internal Forms.CustomControls.DarkArrowButton SelectForDownloadButton;
    internal CustomControls.DarkArrowButton UnselectForDownloadButton;
    internal CustomControls.DarkRadioButton SortByDateCheckBox;
    internal CustomControls.DarkRadioButton SortByTitleCheckBox;
    internal CustomControls.DarkLabel SortByLabel;
    private CustomControls.DarkFlowLayoutPanel darkFlowLayoutPanel1;
    internal CustomControls.DarkLabel DownloadableMissionsLabel;
    internal CustomControls.DarkLabel SelectedForDownloadLabel;
    private CustomControls.DarkFlowLayoutPanel darkFlowLayoutPanel2;
}
