namespace AngelLoader.Forms
{
    public sealed partial class ScanAllFMsForm
    {
        // Button widths are kept defined because we pass them as minimum values to the text autosizer
        private void InitComponentManual()
        {
            TitleCheckBox = new System.Windows.Forms.CheckBox();
            AuthorCheckBox = new System.Windows.Forms.CheckBox();
            GameCheckBox = new System.Windows.Forms.CheckBox();
            CustomResourcesCheckBox = new System.Windows.Forms.CheckBox();
            SizeCheckBox = new System.Windows.Forms.CheckBox();
            ReleaseDateCheckBox = new System.Windows.Forms.CheckBox();
            TagsCheckBox = new System.Windows.Forms.CheckBox();
            SelectAllButton = new System.Windows.Forms.Button();
            SelectNoneButton = new System.Windows.Forms.Button();
            ScanButton = new System.Windows.Forms.Button();
            Cancel_Button = new System.Windows.Forms.Button();
            ScanAllFMsForLabel = new System.Windows.Forms.Label();
            OKCancelButtonsFLP = new System.Windows.Forms.FlowLayoutPanel();
            SelectButtonsFLP = new System.Windows.Forms.FlowLayoutPanel();
            OKCancelButtonsFLP.SuspendLayout();
            SelectButtonsFLP.SuspendLayout();
            SuspendLayout();
            // 
            // TitleCheckBox
            // 
            TitleCheckBox.AutoSize = true;
            TitleCheckBox.Checked = true;
            TitleCheckBox.Location = new System.Drawing.Point(16, 40);
            TitleCheckBox.TabIndex = 2;
            TitleCheckBox.UseVisualStyleBackColor = true;
            // 
            // AuthorCheckBox
            // 
            AuthorCheckBox.AutoSize = true;
            AuthorCheckBox.Checked = true;
            AuthorCheckBox.Location = new System.Drawing.Point(16, 56);
            AuthorCheckBox.TabIndex = 3;
            AuthorCheckBox.UseVisualStyleBackColor = true;
            // 
            // GameCheckBox
            // 
            GameCheckBox.AutoSize = true;
            GameCheckBox.Checked = true;
            GameCheckBox.Location = new System.Drawing.Point(16, 72);
            GameCheckBox.TabIndex = 4;
            GameCheckBox.UseVisualStyleBackColor = true;
            // 
            // CustomResourcesCheckBox
            // 
            CustomResourcesCheckBox.AutoSize = true;
            CustomResourcesCheckBox.Checked = true;
            CustomResourcesCheckBox.Location = new System.Drawing.Point(16, 88);
            CustomResourcesCheckBox.TabIndex = 5;
            CustomResourcesCheckBox.UseVisualStyleBackColor = true;
            // 
            // SizeCheckBox
            // 
            SizeCheckBox.AutoSize = true;
            SizeCheckBox.Checked = true;
            SizeCheckBox.Location = new System.Drawing.Point(16, 104);
            SizeCheckBox.TabIndex = 6;
            SizeCheckBox.UseVisualStyleBackColor = true;
            // 
            // ReleaseDateCheckBox
            // 
            ReleaseDateCheckBox.AutoSize = true;
            ReleaseDateCheckBox.Checked = true;
            ReleaseDateCheckBox.Location = new System.Drawing.Point(16, 120);
            ReleaseDateCheckBox.TabIndex = 7;
            ReleaseDateCheckBox.UseVisualStyleBackColor = true;
            // 
            // TagsCheckBox
            // 
            TagsCheckBox.AutoSize = true;
            TagsCheckBox.Checked = true;
            TagsCheckBox.Location = new System.Drawing.Point(16, 136);
            TagsCheckBox.TabIndex = 8;
            TagsCheckBox.UseVisualStyleBackColor = true;
            // 
            // SelectAllButton
            // 
            SelectAllButton.AutoSize = true;
            SelectAllButton.Location = new System.Drawing.Point(0, 3);
            SelectAllButton.Margin = new System.Windows.Forms.Padding(0, 3, 3, 3);
            SelectAllButton.Padding = new System.Windows.Forms.Padding(6, 0, 6, 0);
            SelectAllButton.Width = 75;
            SelectAllButton.TabIndex = 0;
            SelectAllButton.UseVisualStyleBackColor = true;
            SelectAllButton.Click += SelectAllButton_Click;
            // 
            // SelectNoneButton
            // 
            SelectNoneButton.AutoSize = true;
            SelectNoneButton.Location = new System.Drawing.Point(81, 3);
            SelectNoneButton.Padding = new System.Windows.Forms.Padding(6, 0, 6, 0);
            SelectNoneButton.Width = 88;
            SelectNoneButton.TabIndex = 1;
            SelectNoneButton.UseVisualStyleBackColor = true;
            SelectNoneButton.Click += SelectNoneButton_Click;
            // 
            // ScanButton
            // 
            ScanButton.AutoSize = true;
            ScanButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            ScanButton.Location = new System.Drawing.Point(257, 3);
            ScanButton.Padding = new System.Windows.Forms.Padding(6, 0, 6, 0);
            ScanButton.Width = 75;
            ScanButton.TabIndex = 1;
            ScanButton.UseVisualStyleBackColor = true;
            // 
            // Cancel_Button
            // 
            Cancel_Button.AutoSize = true;
            Cancel_Button.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            Cancel_Button.Location = new System.Drawing.Point(338, 3);
            Cancel_Button.Padding = new System.Windows.Forms.Padding(6, 0, 6, 0);
            Cancel_Button.Width = 75;
            Cancel_Button.TabIndex = 0;
            Cancel_Button.UseVisualStyleBackColor = true;
            // 
            // ScanAllFMsForLabel
            // 
            ScanAllFMsForLabel.AutoSize = true;
            ScanAllFMsForLabel.Location = new System.Drawing.Point(16, 16);
            ScanAllFMsForLabel.TabIndex = 1;
            // 
            // OKCancelButtonsFLP
            // 
            OKCancelButtonsFLP.Controls.Add(Cancel_Button);
            OKCancelButtonsFLP.Controls.Add(ScanButton);
            OKCancelButtonsFLP.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            OKCancelButtonsFLP.Location = new System.Drawing.Point(0, 184);
            OKCancelButtonsFLP.Size = new System.Drawing.Size(416, 30);
            OKCancelButtonsFLP.TabIndex = 0;
            // 
            // SelectButtonsFLP
            // 
            SelectButtonsFLP.Controls.Add(SelectAllButton);
            SelectButtonsFLP.Controls.Add(SelectNoneButton);
            SelectButtonsFLP.Location = new System.Drawing.Point(15, 152);
            SelectButtonsFLP.Size = new System.Drawing.Size(401, 28);
            SelectButtonsFLP.TabIndex = 9;
            // 
            // ScanAllFMsForm
            // 
            AcceptButton = ScanButton;
            AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            CancelButton = Cancel_Button;
            ClientSize = new System.Drawing.Size(416, 214);
            Controls.Add(SelectButtonsFLP);
            Controls.Add(OKCancelButtonsFLP);
            Controls.Add(ScanAllFMsForLabel);
            Controls.Add(TagsCheckBox);
            Controls.Add(ReleaseDateCheckBox);
            Controls.Add(SizeCheckBox);
            Controls.Add(CustomResourcesCheckBox);
            Controls.Add(GameCheckBox);
            Controls.Add(AuthorCheckBox);
            Controls.Add(TitleCheckBox);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            Icon = Images.AngelLoader;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            FormClosing += ScanAllFMs_FormClosing;
            OKCancelButtonsFLP.ResumeLayout(false);
            OKCancelButtonsFLP.PerformLayout();
            SelectButtonsFLP.ResumeLayout(false);
            SelectButtonsFLP.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }
    }
}
