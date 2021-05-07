namespace AngelLoader.Forms
{
    sealed partial class ImportFromDarkLoaderForm
    {
        /// <summary>
        /// Custom generated component initializer with cruft removed.
        /// </summary>
        private void InitializeComponentSlim()
        {
            this.OKButton = new AngelLoader.Forms.CustomControls.DarkButton();
            this.Cancel_Button = new AngelLoader.Forms.CustomControls.DarkButton();
            this.OKCancelFlowLayoutPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.ImportFinishedOnCheckBox = new AngelLoader.Forms.CustomControls.DarkCheckBox();
            this.ImportLastPlayedCheckBox = new AngelLoader.Forms.CustomControls.DarkCheckBox();
            this.ImportReleaseDateCheckBox = new AngelLoader.Forms.CustomControls.DarkCheckBox();
            this.ImportCommentCheckBox = new AngelLoader.Forms.CustomControls.DarkCheckBox();
            this.ImportSizeCheckBox = new AngelLoader.Forms.CustomControls.DarkCheckBox();
            this.ImportTitleCheckBox = new AngelLoader.Forms.CustomControls.DarkCheckBox();
            this.ImportFMDataCheckBox = new AngelLoader.Forms.CustomControls.DarkCheckBox();
            this.ImportSavesCheckBox = new AngelLoader.Forms.CustomControls.DarkCheckBox();
            this.ImportControls = new AngelLoader.Forms.User_DL_ImportControls();
            this.OKCancelFlowLayoutPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // OKButton
            // 
            this.OKButton.AutoSize = true;
            this.OKButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.OKButton.MinimumSize = new System.Drawing.Size(75, 23);
            this.OKButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.OKButton.Margin = new System.Windows.Forms.Padding(3, 8, 3, 3);
            this.OKButton.Padding = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.OKButton.TabIndex = 1;
            this.OKButton.UseVisualStyleBackColor = true;
            // 
            // Cancel_Button
            // 
            this.Cancel_Button.AutoSize = true;
            this.Cancel_Button.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.Cancel_Button.MinimumSize = new System.Drawing.Size(75, 23);
            this.Cancel_Button.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Cancel_Button.Margin = new System.Windows.Forms.Padding(3, 8, 9, 3);
            this.Cancel_Button.Padding = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.Cancel_Button.TabIndex = 0;
            this.Cancel_Button.UseVisualStyleBackColor = true;
            // 
            // OKCancelFlowLayoutPanel
            // 
            this.OKCancelFlowLayoutPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.OKCancelFlowLayoutPanel.Controls.Add(this.Cancel_Button);
            this.OKCancelFlowLayoutPanel.Controls.Add(this.OKButton);
            this.OKCancelFlowLayoutPanel.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.OKCancelFlowLayoutPanel.Location = new System.Drawing.Point(0, 245);
            this.OKCancelFlowLayoutPanel.Size = new System.Drawing.Size(547, 40);
            this.OKCancelFlowLayoutPanel.TabIndex = 9;
            // 
            // ImportFinishedOnCheckBox
            // 
            this.ImportFinishedOnCheckBox.AutoSize = true;
            this.ImportFinishedOnCheckBox.Checked = true;
            this.ImportFinishedOnCheckBox.Location = new System.Drawing.Point(32, 200);
            this.ImportFinishedOnCheckBox.TabIndex = 20;
            this.ImportFinishedOnCheckBox.UseVisualStyleBackColor = true;
            // 
            // ImportLastPlayedCheckBox
            // 
            this.ImportLastPlayedCheckBox.AutoSize = true;
            this.ImportLastPlayedCheckBox.Checked = true;
            this.ImportLastPlayedCheckBox.Location = new System.Drawing.Point(32, 184);
            this.ImportLastPlayedCheckBox.TabIndex = 21;
            this.ImportLastPlayedCheckBox.UseVisualStyleBackColor = true;
            // 
            // ImportReleaseDateCheckBox
            // 
            this.ImportReleaseDateCheckBox.AutoSize = true;
            this.ImportReleaseDateCheckBox.Checked = true;
            this.ImportReleaseDateCheckBox.Location = new System.Drawing.Point(32, 168);
            this.ImportReleaseDateCheckBox.TabIndex = 22;
            this.ImportReleaseDateCheckBox.UseVisualStyleBackColor = true;
            // 
            // ImportCommentCheckBox
            // 
            this.ImportCommentCheckBox.AutoSize = true;
            this.ImportCommentCheckBox.Checked = true;
            this.ImportCommentCheckBox.Location = new System.Drawing.Point(32, 152);
            this.ImportCommentCheckBox.TabIndex = 23;
            this.ImportCommentCheckBox.UseVisualStyleBackColor = true;
            // 
            // ImportSizeCheckBox
            // 
            this.ImportSizeCheckBox.AutoSize = true;
            this.ImportSizeCheckBox.Checked = true;
            this.ImportSizeCheckBox.Location = new System.Drawing.Point(32, 136);
            this.ImportSizeCheckBox.TabIndex = 24;
            this.ImportSizeCheckBox.UseVisualStyleBackColor = true;
            // 
            // ImportTitleCheckBox
            // 
            this.ImportTitleCheckBox.AutoSize = true;
            this.ImportTitleCheckBox.Checked = true;
            this.ImportTitleCheckBox.Location = new System.Drawing.Point(32, 120);
            this.ImportTitleCheckBox.TabIndex = 25;
            this.ImportTitleCheckBox.UseVisualStyleBackColor = true;
            // 
            // ImportFMDataCheckBox
            // 
            this.ImportFMDataCheckBox.AutoSize = true;
            this.ImportFMDataCheckBox.Checked = true;
            this.ImportFMDataCheckBox.Location = new System.Drawing.Point(16, 96);
            this.ImportFMDataCheckBox.TabIndex = 18;
            this.ImportFMDataCheckBox.UseVisualStyleBackColor = true;
            this.ImportFMDataCheckBox.CheckedChanged += new System.EventHandler(this.ImportFMDataCheckBox_CheckedChanged);
            // 
            // ImportSavesCheckBox
            // 
            this.ImportSavesCheckBox.AutoSize = true;
            this.ImportSavesCheckBox.Checked = true;
            this.ImportSavesCheckBox.Location = new System.Drawing.Point(16, 224);
            this.ImportSavesCheckBox.TabIndex = 19;
            this.ImportSavesCheckBox.UseVisualStyleBackColor = true;
            // 
            // ImportControls
            // 
            this.ImportControls.Location = new System.Drawing.Point(8, 8);
            this.ImportControls.Size = new System.Drawing.Size(545, 88);
            this.ImportControls.TabIndex = 10;
            // 
            // ImportFromDarkLoaderForm
            // 
            this.AcceptButton = this.OKButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.Cancel_Button;
            this.ClientSize = new System.Drawing.Size(547, 285);
            this.Controls.Add(this.ImportFinishedOnCheckBox);
            this.Controls.Add(this.ImportLastPlayedCheckBox);
            this.Controls.Add(this.ImportReleaseDateCheckBox);
            this.Controls.Add(this.ImportCommentCheckBox);
            this.Controls.Add(this.ImportSizeCheckBox);
            this.Controls.Add(this.ImportTitleCheckBox);
            this.Controls.Add(this.ImportFMDataCheckBox);
            this.Controls.Add(this.ImportSavesCheckBox);
            this.Controls.Add(this.ImportControls);
            this.Controls.Add(this.OKCancelFlowLayoutPanel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = AngelLoader.Forms.AL_Icon.AngelLoader;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            // Hack to prevent slow first render on some forms if Text is blank
            this.Text = " ";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ImportFromDarkLoaderForm_FormClosing);
            this.Load += new System.EventHandler(this.ImportFromDarkLoaderForm_Load);
            this.OKCancelFlowLayoutPanel.ResumeLayout(false);
            this.OKCancelFlowLayoutPanel.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}
