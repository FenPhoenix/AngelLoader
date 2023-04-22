namespace AngelLoader.Forms;

sealed partial class ScanAllFMsForm
{
    /// <summary>
    /// Custom generated component initializer with cruft removed.
    /// </summary>
    private void InitSlim()
    {
        this.TitleCheckBox = new AngelLoader.Forms.CustomControls.DarkCheckBox();
        this.AuthorCheckBox = new AngelLoader.Forms.CustomControls.DarkCheckBox();
        this.GameCheckBox = new AngelLoader.Forms.CustomControls.DarkCheckBox();
        this.CustomResourcesCheckBox = new AngelLoader.Forms.CustomControls.DarkCheckBox();
        this.SizeCheckBox = new AngelLoader.Forms.CustomControls.DarkCheckBox();
        this.ReleaseDateCheckBox = new AngelLoader.Forms.CustomControls.DarkCheckBox();
        this.TagsCheckBox = new AngelLoader.Forms.CustomControls.DarkCheckBox();
        this.SelectAllButton = new AngelLoader.Forms.CustomControls.DarkButton();
        this.SelectNoneButton = new AngelLoader.Forms.CustomControls.DarkButton();
        this.ScanButton = new AngelLoader.Forms.CustomControls.DarkButton();
        this.Cancel_Button = new AngelLoader.Forms.CustomControls.DarkButton();
        this.ScanAllFMsForLabel = new AngelLoader.Forms.CustomControls.DarkLabel();
        this.BottomFLP = new System.Windows.Forms.FlowLayoutPanel();
        this.SelectButtonsFLP = new System.Windows.Forms.FlowLayoutPanel();
        this.MissionCountCheckBox = new AngelLoader.Forms.CustomControls.DarkCheckBox();
        this.BottomFLP.SuspendLayout();
        this.SelectButtonsFLP.SuspendLayout();
        this.SuspendLayout();
        // 
        // TitleCheckBox
        // 
        this.TitleCheckBox.AutoSize = true;
        this.TitleCheckBox.Checked = true;
        this.TitleCheckBox.Location = new System.Drawing.Point(16, 40);
        this.TitleCheckBox.TabIndex = 2;
        this.TitleCheckBox.UseVisualStyleBackColor = true;
        // 
        // AuthorCheckBox
        // 
        this.AuthorCheckBox.AutoSize = true;
        this.AuthorCheckBox.Checked = true;
        this.AuthorCheckBox.Location = new System.Drawing.Point(16, 56);
        this.AuthorCheckBox.TabIndex = 3;
        this.AuthorCheckBox.UseVisualStyleBackColor = true;
        // 
        // GameCheckBox
        // 
        this.GameCheckBox.AutoSize = true;
        this.GameCheckBox.Checked = true;
        this.GameCheckBox.Location = new System.Drawing.Point(16, 72);
        this.GameCheckBox.TabIndex = 4;
        this.GameCheckBox.UseVisualStyleBackColor = true;
        // 
        // CustomResourcesCheckBox
        // 
        this.CustomResourcesCheckBox.AutoSize = true;
        this.CustomResourcesCheckBox.Checked = true;
        this.CustomResourcesCheckBox.Location = new System.Drawing.Point(16, 88);
        this.CustomResourcesCheckBox.TabIndex = 5;
        this.CustomResourcesCheckBox.UseVisualStyleBackColor = true;
        // 
        // SizeCheckBox
        // 
        this.SizeCheckBox.AutoSize = true;
        this.SizeCheckBox.Checked = true;
        this.SizeCheckBox.Location = new System.Drawing.Point(16, 104);
        this.SizeCheckBox.TabIndex = 6;
        this.SizeCheckBox.UseVisualStyleBackColor = true;
        // 
        // ReleaseDateCheckBox
        // 
        this.ReleaseDateCheckBox.AutoSize = true;
        this.ReleaseDateCheckBox.Checked = true;
        this.ReleaseDateCheckBox.Location = new System.Drawing.Point(16, 120);
        this.ReleaseDateCheckBox.TabIndex = 7;
        this.ReleaseDateCheckBox.UseVisualStyleBackColor = true;
        // 
        // TagsCheckBox
        // 
        this.TagsCheckBox.AutoSize = true;
        this.TagsCheckBox.Checked = true;
        this.TagsCheckBox.Location = new System.Drawing.Point(16, 136);
        this.TagsCheckBox.TabIndex = 8;
        this.TagsCheckBox.UseVisualStyleBackColor = true;
        // 
        // SelectAllButton
        // 
        this.SelectAllButton.AutoSize = true;
        this.SelectAllButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        this.SelectAllButton.Margin = new System.Windows.Forms.Padding(0, 3, 3, 3);
        this.SelectAllButton.MinimumSize = new System.Drawing.Size(0, 23);
        this.SelectAllButton.Padding = new System.Windows.Forms.Padding(6, 0, 6, 0);
        this.SelectAllButton.TabIndex = 0;
        this.SelectAllButton.UseVisualStyleBackColor = true;
        this.SelectAllButton.Click += new System.EventHandler(this.SelectAllButton_Click);
        // 
        // SelectNoneButton
        // 
        this.SelectNoneButton.AutoSize = true;
        this.SelectNoneButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        this.SelectNoneButton.MinimumSize = new System.Drawing.Size(0, 23);
        this.SelectNoneButton.Padding = new System.Windows.Forms.Padding(6, 0, 6, 0);
        this.SelectNoneButton.TabIndex = 1;
        this.SelectNoneButton.UseVisualStyleBackColor = true;
        this.SelectNoneButton.Click += new System.EventHandler(this.SelectNoneButton_Click);
        // 
        // ScanButton
        // 
        this.ScanButton.AutoSize = true;
        this.ScanButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        this.ScanButton.DialogResult = System.Windows.Forms.DialogResult.OK;
        this.ScanButton.Margin = new System.Windows.Forms.Padding(3, 8, 3, 3);
        this.ScanButton.MinimumSize = new System.Drawing.Size(75, 23);
        this.ScanButton.Padding = new System.Windows.Forms.Padding(6, 0, 6, 0);
        this.ScanButton.TabIndex = 1;
        this.ScanButton.UseVisualStyleBackColor = true;
        // 
        // Cancel_Button
        // 
        this.Cancel_Button.AutoSize = true;
        this.Cancel_Button.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        this.Cancel_Button.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        this.Cancel_Button.Margin = new System.Windows.Forms.Padding(3, 8, 9, 3);
        this.Cancel_Button.MinimumSize = new System.Drawing.Size(75, 23);
        this.Cancel_Button.Padding = new System.Windows.Forms.Padding(6, 0, 6, 0);
        this.Cancel_Button.TabIndex = 0;
        this.Cancel_Button.UseVisualStyleBackColor = true;
        // 
        // ScanAllFMsForLabel
        // 
        this.ScanAllFMsForLabel.AutoSize = true;
        this.ScanAllFMsForLabel.Location = new System.Drawing.Point(16, 16);
        // 
        // BottomFLP
        // 
        this.BottomFLP.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
        this.BottomFLP.Controls.Add(this.Cancel_Button);
        this.BottomFLP.Controls.Add(this.ScanButton);
        this.BottomFLP.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
        this.BottomFLP.Location = new System.Drawing.Point(0, 203);
        this.BottomFLP.Size = new System.Drawing.Size(416, 40);
        this.BottomFLP.TabIndex = 0;
        // 
        // SelectButtonsFLP
        // 
        this.SelectButtonsFLP.Controls.Add(this.SelectAllButton);
        this.SelectButtonsFLP.Controls.Add(this.SelectNoneButton);
        this.SelectButtonsFLP.Location = new System.Drawing.Point(15, 176);
        this.SelectButtonsFLP.Size = new System.Drawing.Size(401, 28);
        this.SelectButtonsFLP.TabIndex = 10;
        // 
        // MissionCountCheckBox
        // 
        this.MissionCountCheckBox.AutoSize = true;
        this.MissionCountCheckBox.Checked = true;
        this.MissionCountCheckBox.Location = new System.Drawing.Point(16, 152);
        this.MissionCountCheckBox.TabIndex = 9;
        this.MissionCountCheckBox.UseVisualStyleBackColor = true;
        // 
        // ScanAllFMsForm
        // 
        this.AcceptButton = this.ScanButton;
        this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.CancelButton = this.Cancel_Button;
        this.ClientSize = new System.Drawing.Size(416, 243);
        this.Controls.Add(this.SelectButtonsFLP);
        this.Controls.Add(this.BottomFLP);
        this.Controls.Add(this.ScanAllFMsForLabel);
        this.Controls.Add(this.MissionCountCheckBox);
        this.Controls.Add(this.TagsCheckBox);
        this.Controls.Add(this.ReleaseDateCheckBox);
        this.Controls.Add(this.SizeCheckBox);
        this.Controls.Add(this.CustomResourcesCheckBox);
        this.Controls.Add(this.GameCheckBox);
        this.Controls.Add(this.AuthorCheckBox);
        this.Controls.Add(this.TitleCheckBox);
        this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
        this.KeyPreview = true;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
        // Hack to prevent slow first render on some forms if Text is blank
        this.Text = " ";
        this.BottomFLP.ResumeLayout(false);
        this.BottomFLP.PerformLayout();
        this.SelectButtonsFLP.ResumeLayout(false);
        this.SelectButtonsFLP.PerformLayout();
        this.ResumeLayout(false);
        this.PerformLayout();
    }
}
