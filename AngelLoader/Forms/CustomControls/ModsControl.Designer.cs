#define FenGen_DesignerSource

namespace AngelLoader.Forms.CustomControls;

sealed partial class ModsControl
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
        this.components = new System.ComponentModel.Container();
        this.HeaderLabel = new AngelLoader.Forms.CustomControls.DarkLabel();
        this.ResetFLP = new DarkFlowLayoutPanel();
        this.DisableNonImportantButton = new AngelLoader.Forms.CustomControls.DarkButton();
        this.EnableAllButton = new AngelLoader.Forms.CustomControls.DarkButton();
        this.ShowImportantCheckBox = new AngelLoader.Forms.CustomControls.DarkCheckBox();
        this.DisabledModsTextBox = new AngelLoader.Forms.CustomControls.DarkTextBox();
        this.DisabledModsLabel = new AngelLoader.Forms.CustomControls.DarkLabel();
        this.CheckList = new AngelLoader.Forms.CustomControls.DarkCheckList();
        this.AutoScrollDummyPanel = new DrawnPanel();
        this.MainToolTip = new System.Windows.Forms.ToolTip(this.components);
        this.ResetFLP.SuspendLayout();
        this.SuspendLayout();
        // 
        // HeaderLabel
        // 
        this.HeaderLabel.AutoSize = true;
        this.HeaderLabel.Location = new System.Drawing.Point(7, 8);
        this.HeaderLabel.Name = "HeaderLabel";
        this.HeaderLabel.Size = new System.Drawing.Size(174, 13);
        this.HeaderLabel.TabIndex = 10;
        this.HeaderLabel.Text = "Enable or disable mods for this FM: ";
        // 
        // ResetFLP
        // 
        this.ResetFLP.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
        | System.Windows.Forms.AnchorStyles.Right)));
        this.ResetFLP.Controls.Add(this.DisableNonImportantButton);
        this.ResetFLP.Controls.Add(this.EnableAllButton);
        this.ResetFLP.Controls.Add(this.ShowImportantCheckBox);
        this.ResetFLP.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
        this.ResetFLP.Location = new System.Drawing.Point(7, 216);
        this.ResetFLP.Name = "ResetFLP";
        this.ResetFLP.Size = new System.Drawing.Size(513, 24);
        this.ResetFLP.TabIndex = 7;
        this.ResetFLP.WrapContents = false;
        // 
        // DisableNonImportantButton
        // 
        this.DisableNonImportantButton.Location = new System.Drawing.Point(438, 0);
        this.DisableNonImportantButton.Margin = new System.Windows.Forms.Padding(0);
        this.DisableNonImportantButton.MinimumSize = new System.Drawing.Size(75, 23);
        this.DisableNonImportantButton.Name = "DisableNonImportantButton";
        this.DisableNonImportantButton.Size = new System.Drawing.Size(75, 23);
        this.DisableNonImportantButton.TabIndex = 2;
        this.DisableNonImportantButton.Text = "Disable all";
        this.DisableNonImportantButton.Click += new System.EventHandler(this.DisableNonImportantButton_Click);
        // 
        // EnableAllButton
        // 
        this.EnableAllButton.Location = new System.Drawing.Point(363, 0);
        this.EnableAllButton.Margin = new System.Windows.Forms.Padding(0);
        this.EnableAllButton.MinimumSize = new System.Drawing.Size(75, 23);
        this.EnableAllButton.Name = "EnableAllButton";
        this.EnableAllButton.Size = new System.Drawing.Size(75, 23);
        this.EnableAllButton.TabIndex = 1;
        this.EnableAllButton.Text = "Enable all";
        this.EnableAllButton.Click += new System.EventHandler(this.EnableAllButton_Click);
        // 
        // ShowImportantCheckBox
        // 
        this.ShowImportantCheckBox.AutoSize = true;
        this.ShowImportantCheckBox.Location = new System.Drawing.Point(261, 3);
        this.ShowImportantCheckBox.Name = "ShowImportantCheckBox";
        this.ShowImportantCheckBox.Size = new System.Drawing.Size(99, 17);
        this.ShowImportantCheckBox.TabIndex = 0;
        this.ShowImportantCheckBox.Text = "Show important";
        this.ShowImportantCheckBox.UseVisualStyleBackColor = true;
        this.ShowImportantCheckBox.CheckedChanged += new System.EventHandler(this.ShowImportantCheckBox_CheckedChanged);
        // 
        // DisabledModsTextBox
        // 
        this.DisabledModsTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
        | System.Windows.Forms.AnchorStyles.Right)));
        this.DisabledModsTextBox.Location = new System.Drawing.Point(7, 256);
        this.DisabledModsTextBox.Name = "DisabledModsTextBox";
        this.DisabledModsTextBox.Size = new System.Drawing.Size(512, 20);
        this.DisabledModsTextBox.TabIndex = 9;
        this.DisabledModsTextBox.TextChanged += new System.EventHandler(this.DisabledModsTextBox_TextChanged);
        this.DisabledModsTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.DisabledModsTextBox_KeyDown);
        this.DisabledModsTextBox.Leave += new System.EventHandler(this.DisabledModsTextBox_Leave);
        // 
        // DisabledModsLabel
        // 
        this.DisabledModsLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
        this.DisabledModsLabel.AutoSize = true;
        this.DisabledModsLabel.Location = new System.Drawing.Point(7, 240);
        this.DisabledModsLabel.Name = "DisabledModsLabel";
        this.DisabledModsLabel.Size = new System.Drawing.Size(79, 13);
        this.DisabledModsLabel.TabIndex = 8;
        this.DisabledModsLabel.Text = "Disabled mods:";
        // 
        // CheckList
        // 
        this.CheckList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
        | System.Windows.Forms.AnchorStyles.Left)
        | System.Windows.Forms.AnchorStyles.Right)));
        this.CheckList.AutoScroll = true;
        this.CheckList.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        this.CheckList.Location = new System.Drawing.Point(7, 32);
        this.CheckList.Name = "CheckList";
        this.CheckList.Size = new System.Drawing.Size(512, 184);
        this.CheckList.TabIndex = 6;
        this.CheckList.ItemCheckedChanged += new System.EventHandler<AngelLoader.Forms.CustomControls.DarkCheckList.DarkCheckListEventArgs>(this.CheckList_ItemCheckedChanged);
        // 
        // AutoScrollDummyPanel
        // 
        this.AutoScrollDummyPanel.Location = new System.Drawing.Point(7, 8);
        this.AutoScrollDummyPanel.Name = "AutoScrollDummyPanel";
        this.AutoScrollDummyPanel.Size = new System.Drawing.Size(280, 208);
        this.AutoScrollDummyPanel.TabIndex = 11;
        // 
        // ModsControl
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.AutoScroll = true;
        this.Controls.Add(this.HeaderLabel);
        this.Controls.Add(this.ResetFLP);
        this.Controls.Add(this.DisabledModsTextBox);
        this.Controls.Add(this.DisabledModsLabel);
        this.Controls.Add(this.CheckList);
        this.Controls.Add(this.AutoScrollDummyPanel);
        this.Name = "ModsControl";
        this.Size = new System.Drawing.Size(527, 284);
        this.ResetFLP.ResumeLayout(false);
        this.ResetFLP.PerformLayout();
        this.ResumeLayout(false);
        this.PerformLayout();

    }
#endif

    #endregion

    internal DarkLabel HeaderLabel;
    internal DarkFlowLayoutPanel ResetFLP;
    internal DarkButton DisableNonImportantButton;
    internal DarkButton EnableAllButton;
    internal DarkCheckBox ShowImportantCheckBox;
    internal DarkTextBox DisabledModsTextBox;
    internal DarkLabel DisabledModsLabel;
    internal DarkCheckList CheckList;
    internal DrawnPanel AutoScrollDummyPanel;
    internal System.Windows.Forms.ToolTip MainToolTip;
}
