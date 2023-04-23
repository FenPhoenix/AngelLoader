namespace AngelLoader.Forms;

sealed partial class ImportFromMultipleInisForm
{
    /// <summary>
    /// Custom generated component initializer with cruft removed.
    /// </summary>
    private void InitSlim()
    {
        this.OKButton = new AngelLoader.Forms.CustomControls.DarkButton();
        this.Cancel_Button = new AngelLoader.Forms.CustomControls.DarkButton();
        this.BottomFLP = new System.Windows.Forms.FlowLayoutPanel();
        this.ImportControls = new AngelLoader.Forms.User_FMSel_NDL_ImportControls();
        this.ImportSizeCheckBox = new AngelLoader.Forms.CustomControls.DarkCheckBox();
        this.ImportFinishedOnCheckBox = new AngelLoader.Forms.CustomControls.DarkCheckBox();
        this.ImportSelectedReadmeCheckBox = new AngelLoader.Forms.CustomControls.DarkCheckBox();
        this.ImportTagsCheckBox = new AngelLoader.Forms.CustomControls.DarkCheckBox();
        this.ImportDisabledModsCheckBox = new AngelLoader.Forms.CustomControls.DarkCheckBox();
        this.ImportRatingCheckBox = new AngelLoader.Forms.CustomControls.DarkCheckBox();
        this.ImportCommentCheckBox = new AngelLoader.Forms.CustomControls.DarkCheckBox();
        this.ImportLastPlayedCheckBox = new AngelLoader.Forms.CustomControls.DarkCheckBox();
        this.ImportReleaseDateCheckBox = new AngelLoader.Forms.CustomControls.DarkCheckBox();
        this.ImportTitleCheckBox = new AngelLoader.Forms.CustomControls.DarkCheckBox();
        this.BottomFLP.SuspendLayout();
        this.SuspendLayout();
        // 
        // OKButton
        // 
        this.OKButton.AutoSize = true;
        this.OKButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        this.OKButton.DialogResult = System.Windows.Forms.DialogResult.OK;
        this.OKButton.MinimumSize = new System.Drawing.Size(75, 23);
        this.OKButton.Padding = new System.Windows.Forms.Padding(6, 0, 6, 0);
        this.OKButton.TabIndex = 1;
        // 
        // Cancel_Button
        // 
        this.Cancel_Button.AutoSize = true;
        this.Cancel_Button.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        this.Cancel_Button.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        this.Cancel_Button.MinimumSize = new System.Drawing.Size(75, 23);
        this.Cancel_Button.Padding = new System.Windows.Forms.Padding(6, 0, 6, 0);
        this.Cancel_Button.TabIndex = 0;
        // 
        // BottomFLP
        // 
        this.BottomFLP.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
        this.BottomFLP.AutoSize = true;
        this.BottomFLP.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        this.BottomFLP.Controls.Add(this.Cancel_Button);
        this.BottomFLP.Controls.Add(this.OKButton);
        this.BottomFLP.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
        this.BottomFLP.Location = new System.Drawing.Point(386, 578);
        this.BottomFLP.Size = new System.Drawing.Size(162, 29);
        this.BottomFLP.TabIndex = 0;
        // 
        // ImportControls
        // 
        this.ImportControls.Size = new System.Drawing.Size(551, 408);
        this.ImportControls.TabIndex = 1;
        // 
        // ImportSizeCheckBox
        // 
        this.ImportSizeCheckBox.AutoSize = true;
        this.ImportSizeCheckBox.Checked = true;
        this.ImportSizeCheckBox.Location = new System.Drawing.Point(16, 552);
        this.ImportSizeCheckBox.TabIndex = 9;
        this.ImportSizeCheckBox.UseVisualStyleBackColor = true;
        // 
        // ImportFinishedOnCheckBox
        // 
        this.ImportFinishedOnCheckBox.AutoSize = true;
        this.ImportFinishedOnCheckBox.Checked = true;
        this.ImportFinishedOnCheckBox.Location = new System.Drawing.Point(16, 536);
        this.ImportFinishedOnCheckBox.TabIndex = 8;
        this.ImportFinishedOnCheckBox.UseVisualStyleBackColor = true;
        // 
        // ImportSelectedReadmeCheckBox
        // 
        this.ImportSelectedReadmeCheckBox.AutoSize = true;
        this.ImportSelectedReadmeCheckBox.Checked = true;
        this.ImportSelectedReadmeCheckBox.Location = new System.Drawing.Point(16, 520);
        this.ImportSelectedReadmeCheckBox.TabIndex = 7;
        this.ImportSelectedReadmeCheckBox.UseVisualStyleBackColor = true;
        // 
        // ImportTagsCheckBox
        // 
        this.ImportTagsCheckBox.AutoSize = true;
        this.ImportTagsCheckBox.Checked = true;
        this.ImportTagsCheckBox.Location = new System.Drawing.Point(16, 504);
        this.ImportTagsCheckBox.TabIndex = 6;
        this.ImportTagsCheckBox.UseVisualStyleBackColor = true;
        // 
        // ImportDisabledModsCheckBox
        // 
        this.ImportDisabledModsCheckBox.AutoSize = true;
        this.ImportDisabledModsCheckBox.Checked = true;
        this.ImportDisabledModsCheckBox.Location = new System.Drawing.Point(16, 488);
        this.ImportDisabledModsCheckBox.TabIndex = 5;
        this.ImportDisabledModsCheckBox.UseVisualStyleBackColor = true;
        // 
        // ImportRatingCheckBox
        // 
        this.ImportRatingCheckBox.AutoSize = true;
        this.ImportRatingCheckBox.Checked = true;
        this.ImportRatingCheckBox.Location = new System.Drawing.Point(16, 472);
        this.ImportRatingCheckBox.TabIndex = 4;
        this.ImportRatingCheckBox.UseVisualStyleBackColor = true;
        // 
        // ImportCommentCheckBox
        // 
        this.ImportCommentCheckBox.AutoSize = true;
        this.ImportCommentCheckBox.Checked = true;
        this.ImportCommentCheckBox.Location = new System.Drawing.Point(16, 456);
        this.ImportCommentCheckBox.TabIndex = 3;
        this.ImportCommentCheckBox.UseVisualStyleBackColor = true;
        // 
        // ImportLastPlayedCheckBox
        // 
        this.ImportLastPlayedCheckBox.AutoSize = true;
        this.ImportLastPlayedCheckBox.Checked = true;
        this.ImportLastPlayedCheckBox.Location = new System.Drawing.Point(16, 440);
        this.ImportLastPlayedCheckBox.TabIndex = 2;
        this.ImportLastPlayedCheckBox.UseVisualStyleBackColor = true;
        // 
        // ImportReleaseDateCheckBox
        // 
        this.ImportReleaseDateCheckBox.AutoSize = true;
        this.ImportReleaseDateCheckBox.Checked = true;
        this.ImportReleaseDateCheckBox.Location = new System.Drawing.Point(16, 424);
        this.ImportReleaseDateCheckBox.TabIndex = 1;
        this.ImportReleaseDateCheckBox.UseVisualStyleBackColor = true;
        // 
        // ImportTitleCheckBox
        // 
        this.ImportTitleCheckBox.AutoSize = true;
        this.ImportTitleCheckBox.Checked = true;
        this.ImportTitleCheckBox.Location = new System.Drawing.Point(16, 408);
        this.ImportTitleCheckBox.TabIndex = 0;
        this.ImportTitleCheckBox.UseVisualStyleBackColor = true;
        // 
        // ImportFromMultipleInisForm
        // 
        this.AcceptButton = this.OKButton;
        this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.CancelButton = this.Cancel_Button;
        this.ClientSize = new System.Drawing.Size(553, 613);
        this.Controls.Add(this.ImportSizeCheckBox);
        this.Controls.Add(this.ImportFinishedOnCheckBox);
        this.Controls.Add(this.ImportSelectedReadmeCheckBox);
        this.Controls.Add(this.ImportTagsCheckBox);
        this.Controls.Add(this.ImportDisabledModsCheckBox);
        this.Controls.Add(this.ImportRatingCheckBox);
        this.Controls.Add(this.ImportCommentCheckBox);
        this.Controls.Add(this.ImportLastPlayedCheckBox);
        this.Controls.Add(this.ImportReleaseDateCheckBox);
        this.Controls.Add(this.ImportTitleCheckBox);
        this.Controls.Add(this.ImportControls);
        this.Controls.Add(this.BottomFLP);
        this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
        // Hack to prevent slow first render on some forms if Text is blank
        this.Text = " ";
        this.BottomFLP.ResumeLayout(false);
        this.BottomFLP.PerformLayout();
        this.ResumeLayout(false);
        this.PerformLayout();
    }
}
