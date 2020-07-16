namespace AngelLoader.Forms
{
    sealed partial class ScanAllFMsForm
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

#if DEBUG
        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.TitleCheckBox = new System.Windows.Forms.CheckBox();
            this.AuthorCheckBox = new System.Windows.Forms.CheckBox();
            this.GameCheckBox = new System.Windows.Forms.CheckBox();
            this.CustomResourcesCheckBox = new System.Windows.Forms.CheckBox();
            this.SizeCheckBox = new System.Windows.Forms.CheckBox();
            this.ReleaseDateCheckBox = new System.Windows.Forms.CheckBox();
            this.TagsCheckBox = new System.Windows.Forms.CheckBox();
            this.SelectAllButton = new System.Windows.Forms.Button();
            this.SelectNoneButton = new System.Windows.Forms.Button();
            this.ScanButton = new System.Windows.Forms.Button();
            this.Cancel_Button = new System.Windows.Forms.Button();
            this.ScanAllFMsForLabel = new System.Windows.Forms.Label();
            this.OKCancelButtonsFLP = new System.Windows.Forms.FlowLayoutPanel();
            this.SelectButtonsFLP = new System.Windows.Forms.FlowLayoutPanel();
            this.OKCancelButtonsFLP.SuspendLayout();
            this.SelectButtonsFLP.SuspendLayout();
            this.SuspendLayout();
            // 
            // TitleCheckBox
            // 
            this.TitleCheckBox.AutoSize = true;
            this.TitleCheckBox.Checked = true;
            this.TitleCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.TitleCheckBox.Location = new System.Drawing.Point(16, 40);
            this.TitleCheckBox.Name = "TitleCheckBox";
            this.TitleCheckBox.Size = new System.Drawing.Size(46, 17);
            this.TitleCheckBox.TabIndex = 2;
            this.TitleCheckBox.Text = "Title";
            this.TitleCheckBox.UseVisualStyleBackColor = true;
            // 
            // AuthorCheckBox
            // 
            this.AuthorCheckBox.AutoSize = true;
            this.AuthorCheckBox.Checked = true;
            this.AuthorCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.AuthorCheckBox.Location = new System.Drawing.Point(16, 56);
            this.AuthorCheckBox.Name = "AuthorCheckBox";
            this.AuthorCheckBox.Size = new System.Drawing.Size(57, 17);
            this.AuthorCheckBox.TabIndex = 3;
            this.AuthorCheckBox.Text = "Author";
            this.AuthorCheckBox.UseVisualStyleBackColor = true;
            // 
            // GameCheckBox
            // 
            this.GameCheckBox.AutoSize = true;
            this.GameCheckBox.Checked = true;
            this.GameCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.GameCheckBox.Location = new System.Drawing.Point(16, 72);
            this.GameCheckBox.Name = "GameCheckBox";
            this.GameCheckBox.Size = new System.Drawing.Size(54, 17);
            this.GameCheckBox.TabIndex = 4;
            this.GameCheckBox.Text = "Game";
            this.GameCheckBox.UseVisualStyleBackColor = true;
            // 
            // CustomResourcesCheckBox
            // 
            this.CustomResourcesCheckBox.AutoSize = true;
            this.CustomResourcesCheckBox.Checked = true;
            this.CustomResourcesCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.CustomResourcesCheckBox.Location = new System.Drawing.Point(16, 88);
            this.CustomResourcesCheckBox.Name = "CustomResourcesCheckBox";
            this.CustomResourcesCheckBox.Size = new System.Drawing.Size(110, 17);
            this.CustomResourcesCheckBox.TabIndex = 5;
            this.CustomResourcesCheckBox.Text = "Custom resources";
            this.CustomResourcesCheckBox.UseVisualStyleBackColor = true;
            // 
            // SizeCheckBox
            // 
            this.SizeCheckBox.AutoSize = true;
            this.SizeCheckBox.Checked = true;
            this.SizeCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.SizeCheckBox.Location = new System.Drawing.Point(16, 104);
            this.SizeCheckBox.Name = "SizeCheckBox";
            this.SizeCheckBox.Size = new System.Drawing.Size(46, 17);
            this.SizeCheckBox.TabIndex = 6;
            this.SizeCheckBox.Text = "Size";
            this.SizeCheckBox.UseVisualStyleBackColor = true;
            // 
            // ReleaseDateCheckBox
            // 
            this.ReleaseDateCheckBox.AutoSize = true;
            this.ReleaseDateCheckBox.Checked = true;
            this.ReleaseDateCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ReleaseDateCheckBox.Location = new System.Drawing.Point(16, 120);
            this.ReleaseDateCheckBox.Name = "ReleaseDateCheckBox";
            this.ReleaseDateCheckBox.Size = new System.Drawing.Size(89, 17);
            this.ReleaseDateCheckBox.TabIndex = 7;
            this.ReleaseDateCheckBox.Text = "Release date";
            this.ReleaseDateCheckBox.UseVisualStyleBackColor = true;
            // 
            // TagsCheckBox
            // 
            this.TagsCheckBox.AutoSize = true;
            this.TagsCheckBox.Checked = true;
            this.TagsCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.TagsCheckBox.Location = new System.Drawing.Point(16, 136);
            this.TagsCheckBox.Name = "TagsCheckBox";
            this.TagsCheckBox.Size = new System.Drawing.Size(50, 17);
            this.TagsCheckBox.TabIndex = 8;
            this.TagsCheckBox.Text = "Tags";
            this.TagsCheckBox.UseVisualStyleBackColor = true;
            // 
            // SelectAllButton
            // 
            this.SelectAllButton.AutoSize = true;
            this.SelectAllButton.Location = new System.Drawing.Point(0, 3);
            this.SelectAllButton.Margin = new System.Windows.Forms.Padding(0, 3, 3, 3);
            this.SelectAllButton.Name = "SelectAllButton";
            this.SelectAllButton.Padding = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.SelectAllButton.Size = new System.Drawing.Size(75, 23);
            this.SelectAllButton.TabIndex = 0;
            this.SelectAllButton.Text = "Select all";
            this.SelectAllButton.UseVisualStyleBackColor = true;
            this.SelectAllButton.Click += new System.EventHandler(this.SelectAllButton_Click);
            // 
            // SelectNoneButton
            // 
            this.SelectNoneButton.AutoSize = true;
            this.SelectNoneButton.Location = new System.Drawing.Point(81, 3);
            this.SelectNoneButton.Name = "SelectNoneButton";
            this.SelectNoneButton.Padding = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.SelectNoneButton.Size = new System.Drawing.Size(88, 23);
            this.SelectNoneButton.TabIndex = 1;
            this.SelectNoneButton.Text = "Select none";
            this.SelectNoneButton.UseVisualStyleBackColor = true;
            this.SelectNoneButton.Click += new System.EventHandler(this.SelectNoneButton_Click);
            // 
            // ScanButton
            // 
            this.ScanButton.AutoSize = true;
            this.ScanButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.ScanButton.Location = new System.Drawing.Point(257, 3);
            this.ScanButton.Name = "ScanButton";
            this.ScanButton.Padding = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.ScanButton.Size = new System.Drawing.Size(75, 23);
            this.ScanButton.TabIndex = 1;
            this.ScanButton.Text = "Scan";
            this.ScanButton.UseVisualStyleBackColor = true;
            // 
            // Cancel_Button
            // 
            this.Cancel_Button.AutoSize = true;
            this.Cancel_Button.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Cancel_Button.Location = new System.Drawing.Point(338, 3);
            this.Cancel_Button.Name = "Cancel_Button";
            this.Cancel_Button.Padding = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.Cancel_Button.Size = new System.Drawing.Size(75, 23);
            this.Cancel_Button.TabIndex = 0;
            this.Cancel_Button.Text = "Cancel";
            this.Cancel_Button.UseVisualStyleBackColor = true;
            // 
            // ScanAllFMsForLabel
            // 
            this.ScanAllFMsForLabel.AutoSize = true;
            this.ScanAllFMsForLabel.Location = new System.Drawing.Point(16, 16);
            this.ScanAllFMsForLabel.Name = "ScanAllFMsForLabel";
            this.ScanAllFMsForLabel.Size = new System.Drawing.Size(86, 13);
            this.ScanAllFMsForLabel.TabIndex = 1;
            this.ScanAllFMsForLabel.Text = "Scan all FMs for:";
            // 
            // OKCancelButtonsFLP
            // 
            this.OKCancelButtonsFLP.Controls.Add(this.Cancel_Button);
            this.OKCancelButtonsFLP.Controls.Add(this.ScanButton);
            this.OKCancelButtonsFLP.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.OKCancelButtonsFLP.Location = new System.Drawing.Point(0, 184);
            this.OKCancelButtonsFLP.Name = "OKCancelButtonsFLP";
            this.OKCancelButtonsFLP.Size = new System.Drawing.Size(416, 30);
            this.OKCancelButtonsFLP.TabIndex = 0;
            // 
            // SelectButtonsFLP
            // 
            this.SelectButtonsFLP.Controls.Add(this.SelectAllButton);
            this.SelectButtonsFLP.Controls.Add(this.SelectNoneButton);
            this.SelectButtonsFLP.Location = new System.Drawing.Point(15, 152);
            this.SelectButtonsFLP.Name = "SelectButtonsFLP";
            this.SelectButtonsFLP.Size = new System.Drawing.Size(401, 28);
            this.SelectButtonsFLP.TabIndex = 9;
            // 
            // ScanAllFMsForm
            // 
            this.AcceptButton = this.ScanButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.Cancel_Button;
            this.ClientSize = new System.Drawing.Size(416, 214);
            this.Controls.Add(this.SelectButtonsFLP);
            this.Controls.Add(this.OKCancelButtonsFLP);
            this.Controls.Add(this.ScanAllFMsForLabel);
            this.Controls.Add(this.TagsCheckBox);
            this.Controls.Add(this.ReleaseDateCheckBox);
            this.Controls.Add(this.SizeCheckBox);
            this.Controls.Add(this.CustomResourcesCheckBox);
            this.Controls.Add(this.GameCheckBox);
            this.Controls.Add(this.AuthorCheckBox);
            this.Controls.Add(this.TitleCheckBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = global::AngelLoader.Properties.Resources.AngelLoader;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ScanAllFMsForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Scan all FMs";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ScanAllFMs_FormClosing);
            this.OKCancelButtonsFLP.ResumeLayout(false);
            this.OKCancelButtonsFLP.PerformLayout();
            this.SelectButtonsFLP.ResumeLayout(false);
            this.SelectButtonsFLP.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
#endif

        private System.Windows.Forms.CheckBox TitleCheckBox;
        private System.Windows.Forms.CheckBox AuthorCheckBox;
        private System.Windows.Forms.CheckBox GameCheckBox;
        private System.Windows.Forms.CheckBox CustomResourcesCheckBox;
        private System.Windows.Forms.CheckBox SizeCheckBox;
        private System.Windows.Forms.CheckBox ReleaseDateCheckBox;
        private System.Windows.Forms.CheckBox TagsCheckBox;
        private System.Windows.Forms.Button SelectAllButton;
        private System.Windows.Forms.Button SelectNoneButton;
        private System.Windows.Forms.Button ScanButton;
        private System.Windows.Forms.Button Cancel_Button;
        private System.Windows.Forms.Label ScanAllFMsForLabel;
        private System.Windows.Forms.FlowLayoutPanel OKCancelButtonsFLP;
        private System.Windows.Forms.FlowLayoutPanel SelectButtonsFLP;
    }
}