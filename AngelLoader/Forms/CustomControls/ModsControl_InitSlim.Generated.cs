namespace AngelLoader.Forms.CustomControls;

sealed partial class ModsControl
{
    /// <summary>
    /// Custom generated component initializer with cruft removed.
    /// </summary>
    private void InitSlim()
    {
        this.components = new System.ComponentModel.Container();
        this.HeaderLabel = new AngelLoader.Forms.CustomControls.DarkLabel();
        this.ResetFLP = new AngelLoader.Forms.CustomControls.DrawnFlowLayoutPanel();
        this.DisableNonImportantButton = new AngelLoader.Forms.CustomControls.DarkButton();
        this.EnableAllButton = new AngelLoader.Forms.CustomControls.DarkButton();
        this.ShowImportantCheckBox = new AngelLoader.Forms.CustomControls.DarkCheckBox();
        this.DisabledModsTextBox = new AngelLoader.Forms.CustomControls.DarkTextBox();
        this.DisabledModsLabel = new AngelLoader.Forms.CustomControls.DarkLabel();
        this.CheckList = new AngelLoader.Forms.CustomControls.ModsPanel();
        this.MainToolTip = new AngelLoader.Forms.CustomControls.ToolTipCustom(this.components);
        this.ResetFLP.SuspendLayout();
        this.SuspendLayout();
        // 
        // HeaderLabel
        // 
        this.HeaderLabel.AutoSize = true;
        this.HeaderLabel.Location = new System.Drawing.Point(7, 8);
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
        this.ResetFLP.Size = new System.Drawing.Size(513, 24);
        this.ResetFLP.TabIndex = 7;
        this.ResetFLP.WrapContents = false;
        // 
        // DisableNonImportantButton
        // 
        this.DisableNonImportantButton.AutoSize = true;
        this.DisableNonImportantButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        this.DisableNonImportantButton.Margin = new System.Windows.Forms.Padding(0);
        this.DisableNonImportantButton.MinimumSize = new System.Drawing.Size(75, 23);
        this.DisableNonImportantButton.TabIndex = 2;
        this.DisableNonImportantButton.Click += new System.EventHandler(this.DisableNonImportantButton_Click);
        // 
        // EnableAllButton
        // 
        this.EnableAllButton.AutoSize = true;
        this.EnableAllButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        this.EnableAllButton.Margin = new System.Windows.Forms.Padding(0);
        this.EnableAllButton.MinimumSize = new System.Drawing.Size(75, 23);
        this.EnableAllButton.TabIndex = 1;
        this.EnableAllButton.Click += new System.EventHandler(this.EnableAllButton_Click);
        // 
        // ShowImportantCheckBox
        // 
        this.ShowImportantCheckBox.AutoSize = true;
        this.ShowImportantCheckBox.TabIndex = 0;
        this.ShowImportantCheckBox.CheckedChanged += new System.EventHandler(this.ShowImportantCheckBox_CheckedChanged);
        // 
        // DisabledModsTextBox
        // 
        this.DisabledModsTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
        | System.Windows.Forms.AnchorStyles.Right)));
        this.DisabledModsTextBox.Location = new System.Drawing.Point(7, 256);
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
        this.DisabledModsLabel.Size = new System.Drawing.Size(79, 13);
        // 
        // CheckList
        // 
        this.CheckList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
        | System.Windows.Forms.AnchorStyles.Left)
        | System.Windows.Forms.AnchorStyles.Right)));
        this.CheckList.AutoScroll = true;
        this.CheckList.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        this.CheckList.Location = new System.Drawing.Point(7, 32);
        this.CheckList.Size = new System.Drawing.Size(512, 184);
        this.CheckList.TabIndex = 6;
        // 
        // ModsControl
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.AutoScroll = true;
        this.AutoScrollMinSize = new System.Drawing.Size(287, 216);
        this.Controls.Add(this.HeaderLabel);
        this.Controls.Add(this.ResetFLP);
        this.Controls.Add(this.DisabledModsTextBox);
        this.Controls.Add(this.DisabledModsLabel);
        this.Controls.Add(this.CheckList);
        this.Size = new System.Drawing.Size(527, 284);
        this.ResetFLP.ResumeLayout(false);
        this.ResetFLP.PerformLayout();
        this.ResumeLayout(false);
        this.PerformLayout();
    }
}
