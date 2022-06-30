namespace AngelLoader.Forms.CustomControls
{
    sealed partial class ModsControl
    {
        /// <summary>
        /// Custom generated component initializer with cruft removed.
        /// </summary>
        private void InitSlim()
        {
            this.components = new System.ComponentModel.Container();
            this.HeaderLabel = new AngelLoader.Forms.CustomControls.DarkLabel();
            this.ResetFLP = new System.Windows.Forms.FlowLayoutPanel();
            this.DisableNonImportantButton = new AngelLoader.Forms.CustomControls.DarkButton();
            this.EnableAllButton = new AngelLoader.Forms.CustomControls.DarkButton();
            this.ShowUberCheckBox = new AngelLoader.Forms.CustomControls.DarkCheckBox();
            this.ModsDisabledModsTextBox = new AngelLoader.Forms.CustomControls.DarkTextBox();
            this.DisabledModsLabel = new AngelLoader.Forms.CustomControls.DarkLabel();
            this.CheckList = new AngelLoader.Forms.CustomControls.DarkCheckList();
            this.AutoScrollDummyPanel = new System.Windows.Forms.Panel();
            this.MainToolTip = new System.Windows.Forms.ToolTip(this.components);
            this.ResetFLP.SuspendLayout();
            this.SuspendLayout();
            // 
            // HeaderLabel
            // 
            this.HeaderLabel.AutoSize = true;
            this.HeaderLabel.Location = new System.Drawing.Point(7, 8);
            this.HeaderLabel.TabIndex = 10;
            // 
            // ResetFLP
            // 
            this.ResetFLP.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ResetFLP.Controls.Add(this.DisableNonImportantButton);
            this.ResetFLP.Controls.Add(this.EnableAllButton);
            this.ResetFLP.Controls.Add(this.ShowUberCheckBox);
            this.ResetFLP.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.ResetFLP.Location = new System.Drawing.Point(7, 216);
            this.ResetFLP.Size = new System.Drawing.Size(513, 24);
            this.ResetFLP.TabIndex = 7;
            this.ResetFLP.WrapContents = false;
            // 
            // DisableNonImportantButton
            // 
            this.DisableNonImportantButton.Margin = new System.Windows.Forms.Padding(0);
            this.DisableNonImportantButton.MinimumSize = new System.Drawing.Size(75, 23);
            this.DisableNonImportantButton.TabIndex = 2;
            this.DisableNonImportantButton.UseVisualStyleBackColor = true;
            this.DisableNonImportantButton.Click += new System.EventHandler(this.DisableNonImportantButton_Click);
            // 
            // EnableAllButton
            // 
            this.EnableAllButton.Margin = new System.Windows.Forms.Padding(0);
            this.EnableAllButton.MinimumSize = new System.Drawing.Size(75, 23);
            this.EnableAllButton.TabIndex = 1;
            this.EnableAllButton.UseVisualStyleBackColor = true;
            this.EnableAllButton.Click += new System.EventHandler(this.EnableAllButton_Click);
            // 
            // ShowUberCheckBox
            // 
            this.ShowUberCheckBox.AutoSize = true;
            this.ShowUberCheckBox.TabIndex = 0;
            this.ShowUberCheckBox.UseVisualStyleBackColor = true;
            this.ShowUberCheckBox.CheckedChanged += new System.EventHandler(this.ShowUberCheckBox_CheckedChanged);
            // 
            // DisabledModsTextBox
            // 
            this.ModsDisabledModsTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ModsDisabledModsTextBox.Location = new System.Drawing.Point(7, 256);
            this.ModsDisabledModsTextBox.Size = new System.Drawing.Size(512, 20);
            this.ModsDisabledModsTextBox.TabIndex = 9;
            this.ModsDisabledModsTextBox.TextChanged += new System.EventHandler(this.DisabledModsTextBox_TextChanged);
            this.ModsDisabledModsTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.DisabledModsTextBox_KeyDown);
            this.ModsDisabledModsTextBox.Leave += new System.EventHandler(this.DisabledModsTextBox_Leave);
            // 
            // DisabledModsLabel
            // 
            this.DisabledModsLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.DisabledModsLabel.AutoSize = true;
            this.DisabledModsLabel.Location = new System.Drawing.Point(7, 240);
            this.DisabledModsLabel.Size = new System.Drawing.Size(79, 13);
            this.DisabledModsLabel.TabIndex = 8;
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
            this.CheckList.ItemCheckedChanged += new System.EventHandler<AngelLoader.Forms.CustomControls.DarkCheckList.DarkCheckListEventArgs>(this.CheckList_ItemCheckedChanged);
            // 
            // AutoScrollDummyPanel
            // 
            this.AutoScrollDummyPanel.Location = new System.Drawing.Point(7, 8);
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
            this.Controls.Add(this.ModsDisabledModsTextBox);
            this.Controls.Add(this.DisabledModsLabel);
            this.Controls.Add(this.CheckList);
            this.Controls.Add(this.AutoScrollDummyPanel);
            this.Size = new System.Drawing.Size(527, 284);
            this.ResetFLP.ResumeLayout(false);
            this.ResetFLP.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}
