namespace AngelLoader.Forms.CustomControls;

public sealed partial class Lazy_PatchPage
{
    /// <summary>
    /// Custom generated component initializer with cruft removed.
    /// </summary>
    private void InitSlim()
    {
        this.Patch_PerFMValues_Label = new AngelLoader.Forms.CustomControls.DarkLabel();
        this.Patch_NDSubs_CheckBox = new AngelLoader.Forms.CustomControls.DarkCheckBox();
        this.Patch_PostProc_CheckBox = new AngelLoader.Forms.CustomControls.DarkCheckBox();
        this.Patch_NewMantle_CheckBox = new AngelLoader.Forms.CustomControls.DarkCheckBox();
        this.PatchMainPanel = new AngelLoader.Forms.CustomControls.DrawnPanel();
        this.PatchDMLsPanel = new AngelLoader.Forms.CustomControls.DrawnPanel();
        this.AddRemoveDMLButtonsFLP = new AngelLoader.Forms.CustomControls.DrawnFlowLayoutPanel();
        this.PatchAddDMLButton = new AngelLoader.Forms.CustomControls.DarkButton();
        this.PatchRemoveDMLButton = new AngelLoader.Forms.CustomControls.DarkButton();
        this.PatchDMLPatchesLabel = new AngelLoader.Forms.CustomControls.DarkLabel();
        this.PatchDMLsListBox = new AngelLoader.Forms.CustomControls.DarkListBox();
        this.PatchOpenFMFolderButton = new AngelLoader.Forms.CustomControls.DarkButton();
        this.PatchMainPanel.SuspendLayout();
        this.PatchDMLsPanel.SuspendLayout();
        this.AddRemoveDMLButtonsFLP.SuspendLayout();
        this.SuspendLayout();
        // 
        // Patch_PerFMValues_Label
        // 
        this.Patch_PerFMValues_Label.AutoSize = true;
        this.Patch_PerFMValues_Label.Location = new System.Drawing.Point(6, 8);
        // 
        // Patch_NDSubs_CheckBox
        // 
        this.Patch_NDSubs_CheckBox.AutoSize = true;
        this.Patch_NDSubs_CheckBox.Checked = true;
        this.Patch_NDSubs_CheckBox.CheckState = System.Windows.Forms.CheckState.Indeterminate;
        this.Patch_NDSubs_CheckBox.Location = new System.Drawing.Point(8, 80);
        this.Patch_NDSubs_CheckBox.TabIndex = 42;
        this.Patch_NDSubs_CheckBox.ThreeState = true;
        // 
        // Patch_PostProc_CheckBox
        // 
        this.Patch_PostProc_CheckBox.AutoSize = true;
        this.Patch_PostProc_CheckBox.Checked = true;
        this.Patch_PostProc_CheckBox.CheckState = System.Windows.Forms.CheckState.Indeterminate;
        this.Patch_PostProc_CheckBox.Location = new System.Drawing.Point(8, 56);
        this.Patch_PostProc_CheckBox.TabIndex = 41;
        this.Patch_PostProc_CheckBox.ThreeState = true;
        // 
        // Patch_NewMantle_CheckBox
        // 
        this.Patch_NewMantle_CheckBox.AutoSize = true;
        this.Patch_NewMantle_CheckBox.Checked = true;
        this.Patch_NewMantle_CheckBox.CheckState = System.Windows.Forms.CheckState.Indeterminate;
        this.Patch_NewMantle_CheckBox.Location = new System.Drawing.Point(8, 32);
        this.Patch_NewMantle_CheckBox.TabIndex = 40;
        this.Patch_NewMantle_CheckBox.ThreeState = true;
        // 
        // PatchMainPanel
        // 
        this.PatchMainPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
        | System.Windows.Forms.AnchorStyles.Left)
        | System.Windows.Forms.AnchorStyles.Right)));
        this.PatchMainPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        this.PatchMainPanel.Controls.Add(this.PatchDMLsPanel);
        this.PatchMainPanel.Controls.Add(this.PatchOpenFMFolderButton);
        this.PatchMainPanel.Location = new System.Drawing.Point(0, 104);
        this.PatchMainPanel.Size = new System.Drawing.Size(527, 250);
        this.PatchMainPanel.TabIndex = 43;
        // 
        // PatchDMLsPanel
        // 
        this.PatchDMLsPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
        | System.Windows.Forms.AnchorStyles.Left)
        | System.Windows.Forms.AnchorStyles.Right)));
        this.PatchDMLsPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        this.PatchDMLsPanel.Controls.Add(this.AddRemoveDMLButtonsFLP);
        this.PatchDMLsPanel.Controls.Add(this.PatchDMLPatchesLabel);
        this.PatchDMLsPanel.Controls.Add(this.PatchDMLsListBox);
        this.PatchDMLsPanel.Location = new System.Drawing.Point(-2, 0);
        this.PatchDMLsPanel.Size = new System.Drawing.Size(527, 218);
        this.PatchDMLsPanel.TabIndex = 39;
        // 
        // AddRemoveDMLButtonsFLP
        // 
        this.AddRemoveDMLButtonsFLP.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
        | System.Windows.Forms.AnchorStyles.Right)));
        this.AddRemoveDMLButtonsFLP.Controls.Add(this.PatchAddDMLButton);
        this.AddRemoveDMLButtonsFLP.Controls.Add(this.PatchRemoveDMLButton);
        this.AddRemoveDMLButtonsFLP.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
        this.AddRemoveDMLButtonsFLP.Location = new System.Drawing.Point(6, 193);
        this.AddRemoveDMLButtonsFLP.Size = new System.Drawing.Size(519, 24);
        this.AddRemoveDMLButtonsFLP.TabIndex = 44;
        // 
        // PatchAddDMLButton
        // 
        this.PatchAddDMLButton.Margin = new System.Windows.Forms.Padding(1, 0, 0, 0);
        this.PatchAddDMLButton.Size = new System.Drawing.Size(23, 23);
        this.PatchAddDMLButton.TabIndex = 43;
        // 
        // PatchRemoveDMLButton
        // 
        this.PatchRemoveDMLButton.Margin = new System.Windows.Forms.Padding(1, 0, 0, 0);
        this.PatchRemoveDMLButton.Size = new System.Drawing.Size(23, 23);
        this.PatchRemoveDMLButton.TabIndex = 42;
        // 
        // PatchDMLPatchesLabel
        // 
        this.PatchDMLPatchesLabel.AutoSize = true;
        this.PatchDMLPatchesLabel.Location = new System.Drawing.Point(6, 8);
        // 
        // PatchDMLsListBox
        // 
        this.PatchDMLsListBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
        | System.Windows.Forms.AnchorStyles.Left)
        | System.Windows.Forms.AnchorStyles.Right)));
        this.PatchDMLsListBox.Location = new System.Drawing.Point(6, 24);
        this.PatchDMLsListBox.MultiSelect = false;
        this.PatchDMLsListBox.Size = new System.Drawing.Size(518, 168);
        this.PatchDMLsListBox.TabIndex = 41;
        // 
        // PatchOpenFMFolderButton
        // 
        this.PatchOpenFMFolderButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
        this.PatchOpenFMFolderButton.AutoSize = true;
        this.PatchOpenFMFolderButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        this.PatchOpenFMFolderButton.Location = new System.Drawing.Point(5, 224);
        this.PatchOpenFMFolderButton.MinimumSize = new System.Drawing.Size(162, 23);
        this.PatchOpenFMFolderButton.TabIndex = 44;
        // 
        // Lazy_PatchPage
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
        this.AutoScroll = true;
        this.AutoScrollMinSize = new System.Drawing.Size(200, 300);
        this.Controls.Add(this.Patch_PerFMValues_Label);
        this.Controls.Add(this.Patch_NDSubs_CheckBox);
        this.Controls.Add(this.Patch_PostProc_CheckBox);
        this.Controls.Add(this.Patch_NewMantle_CheckBox);
        this.Controls.Add(this.PatchMainPanel);
        this.Size = new System.Drawing.Size(527, 362);
        this.PatchMainPanel.ResumeLayout(false);
        this.PatchMainPanel.PerformLayout();
        this.PatchDMLsPanel.ResumeLayout(false);
        this.PatchDMLsPanel.PerformLayout();
        this.AddRemoveDMLButtonsFLP.ResumeLayout(false);
        this.ResumeLayout(false);
        this.PerformLayout();
    }
}
