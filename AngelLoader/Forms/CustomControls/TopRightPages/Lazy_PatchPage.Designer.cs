#define FenGen_DesignerSource

namespace AngelLoader.Forms.CustomControls;

public sealed partial class Lazy_PatchPage
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
        this.Patch_PerFMValues_Label = new AngelLoader.Forms.CustomControls.DarkLabel();
        this.Patch_NDSubs_CheckBox = new AngelLoader.Forms.CustomControls.DarkCheckBox();
        this.Patch_PostProc_CheckBox = new AngelLoader.Forms.CustomControls.DarkCheckBox();
        this.Patch_NewMantle_CheckBox = new AngelLoader.Forms.CustomControls.DarkCheckBox();
        this.PatchMainPanel = new AngelLoader.Forms.CustomControls.DrawnPanel();
        this.PatchDMLsPanel = new AngelLoader.Forms.CustomControls.DrawnPanel();
        this.PatchDMLPatchesLabel = new AngelLoader.Forms.CustomControls.DarkLabel();
        this.PatchDMLsListBox = new AngelLoader.Forms.CustomControls.DarkListBox();
        this.PatchRemoveDMLButton = new AngelLoader.Forms.CustomControls.DarkButton();
        this.PatchAddDMLButton = new AngelLoader.Forms.CustomControls.DarkButton();
        this.PatchOpenFMFolderButton = new AngelLoader.Forms.CustomControls.DarkButton();
        this.PatchMainPanel.SuspendLayout();
        this.PatchDMLsPanel.SuspendLayout();
        this.SuspendLayout();
        // 
        // Patch_PerFMValues_Label
        // 
        this.Patch_PerFMValues_Label.AutoSize = true;
        this.Patch_PerFMValues_Label.Location = new System.Drawing.Point(6, 8);
        this.Patch_PerFMValues_Label.Name = "Patch_PerFMValues_Label";
        this.Patch_PerFMValues_Label.Size = new System.Drawing.Size(87, 13);
        this.Patch_PerFMValues_Label.TabIndex = 39;
        this.Patch_PerFMValues_Label.Text = "Option overrides:";
        // 
        // Patch_NDSubs_CheckBox
        // 
        this.Patch_NDSubs_CheckBox.AutoSize = true;
        this.Patch_NDSubs_CheckBox.Checked = true;
        this.Patch_NDSubs_CheckBox.CheckState = System.Windows.Forms.CheckState.Indeterminate;
        this.Patch_NDSubs_CheckBox.Location = new System.Drawing.Point(8, 80);
        this.Patch_NDSubs_CheckBox.Name = "Patch_NDSubs_CheckBox";
        this.Patch_NDSubs_CheckBox.Size = new System.Drawing.Size(125, 17);
        this.Patch_NDSubs_CheckBox.TabIndex = 42;
        this.Patch_NDSubs_CheckBox.Text = "Subtitles (if available)";
        this.Patch_NDSubs_CheckBox.ThreeState = true;
        this.Patch_NDSubs_CheckBox.UseVisualStyleBackColor = true;
        // 
        // Patch_PostProc_CheckBox
        // 
        this.Patch_PostProc_CheckBox.AutoSize = true;
        this.Patch_PostProc_CheckBox.Checked = true;
        this.Patch_PostProc_CheckBox.CheckState = System.Windows.Forms.CheckState.Indeterminate;
        this.Patch_PostProc_CheckBox.Location = new System.Drawing.Point(8, 56);
        this.Patch_PostProc_CheckBox.Name = "Patch_PostProc_CheckBox";
        this.Patch_PostProc_CheckBox.Size = new System.Drawing.Size(159, 17);
        this.Patch_PostProc_CheckBox.TabIndex = 41;
        this.Patch_PostProc_CheckBox.Text = "Post-processing (bloom etc.)";
        this.Patch_PostProc_CheckBox.ThreeState = true;
        this.Patch_PostProc_CheckBox.UseVisualStyleBackColor = true;
        // 
        // Patch_NewMantle_CheckBox
        // 
        this.Patch_NewMantle_CheckBox.AutoSize = true;
        this.Patch_NewMantle_CheckBox.Checked = true;
        this.Patch_NewMantle_CheckBox.CheckState = System.Windows.Forms.CheckState.Indeterminate;
        this.Patch_NewMantle_CheckBox.Location = new System.Drawing.Point(8, 32);
        this.Patch_NewMantle_CheckBox.Name = "Patch_NewMantle_CheckBox";
        this.Patch_NewMantle_CheckBox.Size = new System.Drawing.Size(82, 17);
        this.Patch_NewMantle_CheckBox.TabIndex = 40;
        this.Patch_NewMantle_CheckBox.Text = "New mantle";
        this.Patch_NewMantle_CheckBox.ThreeState = true;
        this.Patch_NewMantle_CheckBox.UseVisualStyleBackColor = true;
        // 
        // PatchMainPanel
        // 
        this.PatchMainPanel.AutoSize = true;
        this.PatchMainPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        this.PatchMainPanel.Controls.Add(this.PatchDMLsPanel);
        this.PatchMainPanel.Controls.Add(this.PatchOpenFMFolderButton);
        this.PatchMainPanel.Location = new System.Drawing.Point(0, 104);
        this.PatchMainPanel.Name = "PatchMainPanel";
        this.PatchMainPanel.Size = new System.Drawing.Size(307, 250);
        this.PatchMainPanel.TabIndex = 43;
        // 
        // PatchDMLsPanel
        // 
        this.PatchDMLsPanel.AutoSize = true;
        this.PatchDMLsPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        this.PatchDMLsPanel.Controls.Add(this.PatchDMLPatchesLabel);
        this.PatchDMLsPanel.Controls.Add(this.PatchDMLsListBox);
        this.PatchDMLsPanel.Controls.Add(this.PatchRemoveDMLButton);
        this.PatchDMLsPanel.Controls.Add(this.PatchAddDMLButton);
        this.PatchDMLsPanel.Location = new System.Drawing.Point(-2, 0);
        this.PatchDMLsPanel.Name = "PatchDMLsPanel";
        this.PatchDMLsPanel.Size = new System.Drawing.Size(306, 218);
        this.PatchDMLsPanel.TabIndex = 39;
        // 
        // PatchDMLPatchesLabel
        // 
        this.PatchDMLPatchesLabel.AutoSize = true;
        this.PatchDMLPatchesLabel.Location = new System.Drawing.Point(6, 8);
        this.PatchDMLPatchesLabel.Name = "PatchDMLPatchesLabel";
        this.PatchDMLPatchesLabel.Size = new System.Drawing.Size(156, 13);
        this.PatchDMLPatchesLabel.TabIndex = 40;
        this.PatchDMLPatchesLabel.Text = ".dml patches applied to this FM:";
        // 
        // PatchDMLsListBox
        // 
        this.PatchDMLsListBox.Location = new System.Drawing.Point(6, 24);
        this.PatchDMLsListBox.MultiSelect = false;
        this.PatchDMLsListBox.Name = "PatchDMLsListBox";
        this.PatchDMLsListBox.Size = new System.Drawing.Size(296, 168);
        this.PatchDMLsListBox.TabIndex = 41;
        // 
        // PatchRemoveDMLButton
        // 
        this.PatchRemoveDMLButton.Location = new System.Drawing.Point(256, 192);
        this.PatchRemoveDMLButton.Name = "PatchRemoveDMLButton";
        this.PatchRemoveDMLButton.Size = new System.Drawing.Size(23, 23);
        this.PatchRemoveDMLButton.TabIndex = 42;
        // 
        // PatchAddDMLButton
        // 
        this.PatchAddDMLButton.Location = new System.Drawing.Point(280, 192);
        this.PatchAddDMLButton.Name = "PatchAddDMLButton";
        this.PatchAddDMLButton.Size = new System.Drawing.Size(23, 23);
        this.PatchAddDMLButton.TabIndex = 43;
        // 
        // PatchOpenFMFolderButton
        // 
        this.PatchOpenFMFolderButton.AutoSize = true;
        this.PatchOpenFMFolderButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        this.PatchOpenFMFolderButton.Location = new System.Drawing.Point(5, 224);
        this.PatchOpenFMFolderButton.MinimumSize = new System.Drawing.Size(162, 23);
        this.PatchOpenFMFolderButton.Name = "PatchOpenFMFolderButton";
        this.PatchOpenFMFolderButton.Size = new System.Drawing.Size(162, 23);
        this.PatchOpenFMFolderButton.TabIndex = 44;
        this.PatchOpenFMFolderButton.Text = "Open FM folder";
        // 
        // Lazy_PatchPage
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
        this.AutoScroll = true;
        this.Controls.Add(this.Patch_PerFMValues_Label);
        this.Controls.Add(this.Patch_NDSubs_CheckBox);
        this.Controls.Add(this.Patch_PostProc_CheckBox);
        this.Controls.Add(this.Patch_NewMantle_CheckBox);
        this.Controls.Add(this.PatchMainPanel);
        this.Name = "Lazy_PatchPage";
        this.Size = new System.Drawing.Size(527, 362);
        this.PatchMainPanel.ResumeLayout(false);
        this.PatchMainPanel.PerformLayout();
        this.PatchDMLsPanel.ResumeLayout(false);
        this.PatchDMLsPanel.PerformLayout();
        this.ResumeLayout(false);
        this.PerformLayout();

    }
#endif

    #endregion

    internal DarkLabel Patch_PerFMValues_Label;
    internal DarkCheckBox Patch_NDSubs_CheckBox;
    internal DarkCheckBox Patch_PostProc_CheckBox;
    internal DarkCheckBox Patch_NewMantle_CheckBox;
    internal DrawnPanel PatchMainPanel;
    internal DrawnPanel PatchDMLsPanel;
    internal DarkLabel PatchDMLPatchesLabel;
    internal DarkListBox PatchDMLsListBox;
    internal DarkButton PatchRemoveDMLButton;
    internal DarkButton PatchAddDMLButton;
    internal DarkButton PatchOpenFMFolderButton;
}
