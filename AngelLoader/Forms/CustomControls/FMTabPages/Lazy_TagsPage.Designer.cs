#define FenGen_DesignerSource

namespace AngelLoader.Forms.CustomControls;

public sealed partial class Lazy_TagsPage
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
        this.AddTagButton = new AngelLoader.Forms.CustomControls.DarkButton();
        this.AddTagTextBox = new AngelLoader.Forms.CustomControls.DarkTextBoxCustom();
        this.AddRemoveTagFLP = new AngelLoader.Forms.CustomControls.DrawnFlowLayoutPanel();
        this.RemoveTagButton = new AngelLoader.Forms.CustomControls.DarkButton();
        this.AddTagFromListButton = new AngelLoader.Forms.CustomControls.DarkButton();
        this.TagsTreeView = new AngelLoader.Forms.CustomControls.DarkTreeView();
        this.AddRemoveTagFLP.SuspendLayout();
        this.SuspendLayout();
        // 
        // AddTagButton
        // 
        this.AddTagButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
        this.AddTagButton.AutoSize = true;
        this.AddTagButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        this.AddTagButton.Location = new System.Drawing.Point(455, 7);
        this.AddTagButton.MinimumSize = new System.Drawing.Size(0, 23);
        this.AddTagButton.Name = "AddTagButton";
        this.AddTagButton.Padding = new System.Windows.Forms.Padding(6, 0, 6, 0);
        this.AddTagButton.Size = new System.Drawing.Size(66, 23);
        this.AddTagButton.TabIndex = 7;
        this.AddTagButton.Text = "Add tag";
        // 
        // AddTagTextBox
        // 
        this.AddTagTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
        | System.Windows.Forms.AnchorStyles.Right)));
        this.AddTagTextBox.DisallowedCharacters = ",;";
        this.AddTagTextBox.Location = new System.Drawing.Point(8, 8);
        this.AddTagTextBox.Name = "AddTagTextBox";
        this.AddTagTextBox.Size = new System.Drawing.Size(442, 20);
        this.AddTagTextBox.StrictTextChangedEvent = false;
        this.AddTagTextBox.TabIndex = 6;
        // 
        // AddRemoveTagFLP
        // 
        this.AddRemoveTagFLP.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
        this.AddRemoveTagFLP.AutoSize = true;
        this.AddRemoveTagFLP.Controls.Add(this.RemoveTagButton);
        this.AddRemoveTagFLP.Controls.Add(this.AddTagFromListButton);
        this.AddRemoveTagFLP.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
        this.AddRemoveTagFLP.Location = new System.Drawing.Point(2, 248);
        this.AddRemoveTagFLP.Name = "AddRemoveTagFLP";
        this.AddRemoveTagFLP.Size = new System.Drawing.Size(525, 24);
        this.AddRemoveTagFLP.TabIndex = 9;
        // 
        // RemoveTagButton
        // 
        this.RemoveTagButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
        this.RemoveTagButton.AutoSize = true;
        this.RemoveTagButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        this.RemoveTagButton.Location = new System.Drawing.Point(432, 0);
        this.RemoveTagButton.Margin = new System.Windows.Forms.Padding(0, 0, 6, 0);
        this.RemoveTagButton.MinimumSize = new System.Drawing.Size(0, 23);
        this.RemoveTagButton.Name = "RemoveTagButton";
        this.RemoveTagButton.Padding = new System.Windows.Forms.Padding(6, 0, 6, 0);
        this.RemoveTagButton.Size = new System.Drawing.Size(87, 23);
        this.RemoveTagButton.TabIndex = 1;
        this.RemoveTagButton.Text = "Remove tag";
        // 
        // AddTagFromListButton
        // 
        this.AddTagFromListButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
        this.AddTagFromListButton.AutoSize = true;
        this.AddTagFromListButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        this.AddTagFromListButton.Location = new System.Drawing.Point(337, 0);
        this.AddTagFromListButton.Margin = new System.Windows.Forms.Padding(0);
        this.AddTagFromListButton.MinimumSize = new System.Drawing.Size(0, 23);
        this.AddTagFromListButton.Name = "AddTagFromListButton";
        this.AddTagFromListButton.Padding = new System.Windows.Forms.Padding(6, 0, 6, 0);
        this.AddTagFromListButton.Size = new System.Drawing.Size(95, 23);
        this.AddTagFromListButton.TabIndex = 0;
        this.AddTagFromListButton.Text = "Add from list...";
        // 
        // TagsTreeView
        // 
        this.TagsTreeView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
        | System.Windows.Forms.AnchorStyles.Left)
        | System.Windows.Forms.AnchorStyles.Right)));
        this.TagsTreeView.HideSelection = false;
        this.TagsTreeView.Location = new System.Drawing.Point(8, 32);
        this.TagsTreeView.Name = "TagsTreeView";
        this.TagsTreeView.Size = new System.Drawing.Size(512, 216);
        this.TagsTreeView.TabIndex = 8;
        // 
        // Lazy_TagsPage
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
        this.AutoScroll = true;
        this.AutoScrollMinSize = new System.Drawing.Size(240, 152);
        this.Controls.Add(this.AddTagButton);
        this.Controls.Add(this.AddTagTextBox);
        this.Controls.Add(this.AddRemoveTagFLP);
        this.Controls.Add(this.TagsTreeView);
        this.Name = "Lazy_TagsPage";
        this.Size = new System.Drawing.Size(527, 284);
        this.AddRemoveTagFLP.ResumeLayout(false);
        this.AddRemoveTagFLP.PerformLayout();
        this.ResumeLayout(false);
        this.PerformLayout();

    }
#endif

    #endregion

    internal DarkButton AddTagButton;
    internal DarkTextBoxCustom AddTagTextBox;
    internal DrawnFlowLayoutPanel AddRemoveTagFLP;
    internal DarkButton RemoveTagButton;
    internal DarkButton AddTagFromListButton;
    internal DarkTreeView TagsTreeView;
}
