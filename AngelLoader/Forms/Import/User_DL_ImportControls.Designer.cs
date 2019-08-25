namespace AngelLoader.Forms.Import
{
    partial class User_DL_ImportControls
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

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.AutodetectCheckBox = new System.Windows.Forms.CheckBox();
            this.ImportFMDataCheckBox = new System.Windows.Forms.CheckBox();
            this.ImportSavesCheckBox = new System.Windows.Forms.CheckBox();
            this.ChooseDarkLoaderIniLabel = new System.Windows.Forms.Label();
            this.DarkLoaderIniTextBox = new System.Windows.Forms.TextBox();
            this.DarkLoaderIniBrowseButton = new System.Windows.Forms.Button();
            this.ImportTitleCheckBox = new System.Windows.Forms.CheckBox();
            this.ImportSizeCheckBox = new System.Windows.Forms.CheckBox();
            this.ImportCommentCheckBox = new System.Windows.Forms.CheckBox();
            this.ImportReleaseDateCheckBox = new System.Windows.Forms.CheckBox();
            this.ImportLastPlayedCheckBox = new System.Windows.Forms.CheckBox();
            this.ImportFinishedOnCheckBox = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // AutodetectCheckBox
            // 
            this.AutodetectCheckBox.AutoSize = true;
            this.AutodetectCheckBox.Checked = true;
            this.AutodetectCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.AutodetectCheckBox.Location = new System.Drawing.Point(16, 40);
            this.AutodetectCheckBox.Name = "AutodetectCheckBox";
            this.AutodetectCheckBox.Size = new System.Drawing.Size(78, 17);
            this.AutodetectCheckBox.TabIndex = 16;
            this.AutodetectCheckBox.Text = "Autodetect";
            this.AutodetectCheckBox.UseVisualStyleBackColor = true;
            this.AutodetectCheckBox.CheckedChanged += new System.EventHandler(this.AutodetectCheckBox_CheckedChanged);
            // 
            // ImportFMDataCheckBox
            // 
            this.ImportFMDataCheckBox.AutoSize = true;
            this.ImportFMDataCheckBox.Checked = true;
            this.ImportFMDataCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ImportFMDataCheckBox.Location = new System.Drawing.Point(16, 88);
            this.ImportFMDataCheckBox.Name = "ImportFMDataCheckBox";
            this.ImportFMDataCheckBox.Size = new System.Drawing.Size(97, 17);
            this.ImportFMDataCheckBox.TabIndex = 14;
            this.ImportFMDataCheckBox.Text = "Import FM data";
            this.ImportFMDataCheckBox.UseVisualStyleBackColor = true;
            this.ImportFMDataCheckBox.CheckedChanged += new System.EventHandler(this.ImportFMDataCheckBox_CheckedChanged);
            // 
            // ImportSavesCheckBox
            // 
            this.ImportSavesCheckBox.AutoSize = true;
            this.ImportSavesCheckBox.Checked = true;
            this.ImportSavesCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ImportSavesCheckBox.Location = new System.Drawing.Point(16, 216);
            this.ImportSavesCheckBox.Name = "ImportSavesCheckBox";
            this.ImportSavesCheckBox.Size = new System.Drawing.Size(86, 17);
            this.ImportSavesCheckBox.TabIndex = 15;
            this.ImportSavesCheckBox.Text = "Import saves";
            this.ImportSavesCheckBox.UseVisualStyleBackColor = true;
            // 
            // ChooseDarkLoaderIniLabel
            // 
            this.ChooseDarkLoaderIniLabel.AutoSize = true;
            this.ChooseDarkLoaderIniLabel.Location = new System.Drawing.Point(16, 16);
            this.ChooseDarkLoaderIniLabel.Name = "ChooseDarkLoaderIniLabel";
            this.ChooseDarkLoaderIniLabel.Size = new System.Drawing.Size(118, 13);
            this.ChooseDarkLoaderIniLabel.TabIndex = 11;
            this.ChooseDarkLoaderIniLabel.Text = "Choose DarkLoader.ini:";
            // 
            // DarkLoaderIniTextBox
            // 
            this.DarkLoaderIniTextBox.Location = new System.Drawing.Point(16, 64);
            this.DarkLoaderIniTextBox.Name = "DarkLoaderIniTextBox";
            this.DarkLoaderIniTextBox.ReadOnly = true;
            this.DarkLoaderIniTextBox.Size = new System.Drawing.Size(440, 20);
            this.DarkLoaderIniTextBox.TabIndex = 12;
            // 
            // DarkLoaderIniBrowseButton
            // 
            this.DarkLoaderIniBrowseButton.AutoSize = true;
            this.DarkLoaderIniBrowseButton.Enabled = false;
            this.DarkLoaderIniBrowseButton.Location = new System.Drawing.Point(456, 63);
            this.DarkLoaderIniBrowseButton.Name = "DarkLoaderIniBrowseButton";
            this.DarkLoaderIniBrowseButton.Padding = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.DarkLoaderIniBrowseButton.Size = new System.Drawing.Size(75, 23);
            this.DarkLoaderIniBrowseButton.TabIndex = 13;
            this.DarkLoaderIniBrowseButton.Text = "Browse...";
            this.DarkLoaderIniBrowseButton.UseVisualStyleBackColor = true;
            this.DarkLoaderIniBrowseButton.Click += new System.EventHandler(this.DarkLoaderIniBrowseButton_Click);
            // 
            // ImportTitleCheckBox
            // 
            this.ImportTitleCheckBox.AutoSize = true;
            this.ImportTitleCheckBox.Checked = true;
            this.ImportTitleCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ImportTitleCheckBox.Location = new System.Drawing.Point(32, 112);
            this.ImportTitleCheckBox.Name = "ImportTitleCheckBox";
            this.ImportTitleCheckBox.Size = new System.Drawing.Size(46, 17);
            this.ImportTitleCheckBox.TabIndex = 17;
            this.ImportTitleCheckBox.Text = "Title";
            this.ImportTitleCheckBox.UseVisualStyleBackColor = true;
            // 
            // ImportSizeCheckBox
            // 
            this.ImportSizeCheckBox.AutoSize = true;
            this.ImportSizeCheckBox.Checked = true;
            this.ImportSizeCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ImportSizeCheckBox.Location = new System.Drawing.Point(32, 128);
            this.ImportSizeCheckBox.Name = "ImportSizeCheckBox";
            this.ImportSizeCheckBox.Size = new System.Drawing.Size(46, 17);
            this.ImportSizeCheckBox.TabIndex = 17;
            this.ImportSizeCheckBox.Text = "Size";
            this.ImportSizeCheckBox.UseVisualStyleBackColor = true;
            // 
            // ImportCommentCheckBox
            // 
            this.ImportCommentCheckBox.AutoSize = true;
            this.ImportCommentCheckBox.Checked = true;
            this.ImportCommentCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ImportCommentCheckBox.Location = new System.Drawing.Point(32, 144);
            this.ImportCommentCheckBox.Name = "ImportCommentCheckBox";
            this.ImportCommentCheckBox.Size = new System.Drawing.Size(70, 17);
            this.ImportCommentCheckBox.TabIndex = 17;
            this.ImportCommentCheckBox.Text = "Comment";
            this.ImportCommentCheckBox.UseVisualStyleBackColor = true;
            // 
            // ImportReleaseDateCheckBox
            // 
            this.ImportReleaseDateCheckBox.AutoSize = true;
            this.ImportReleaseDateCheckBox.Checked = true;
            this.ImportReleaseDateCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ImportReleaseDateCheckBox.Location = new System.Drawing.Point(32, 160);
            this.ImportReleaseDateCheckBox.Name = "ImportReleaseDateCheckBox";
            this.ImportReleaseDateCheckBox.Size = new System.Drawing.Size(89, 17);
            this.ImportReleaseDateCheckBox.TabIndex = 17;
            this.ImportReleaseDateCheckBox.Text = "Release date";
            this.ImportReleaseDateCheckBox.UseVisualStyleBackColor = true;
            // 
            // ImportLastPlayedCheckBox
            // 
            this.ImportLastPlayedCheckBox.AutoSize = true;
            this.ImportLastPlayedCheckBox.Checked = true;
            this.ImportLastPlayedCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ImportLastPlayedCheckBox.Location = new System.Drawing.Point(32, 176);
            this.ImportLastPlayedCheckBox.Name = "ImportLastPlayedCheckBox";
            this.ImportLastPlayedCheckBox.Size = new System.Drawing.Size(80, 17);
            this.ImportLastPlayedCheckBox.TabIndex = 17;
            this.ImportLastPlayedCheckBox.Text = "Last played";
            this.ImportLastPlayedCheckBox.UseVisualStyleBackColor = true;
            // 
            // ImportFinishedOnCheckBox
            // 
            this.ImportFinishedOnCheckBox.AutoSize = true;
            this.ImportFinishedOnCheckBox.Checked = true;
            this.ImportFinishedOnCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ImportFinishedOnCheckBox.Location = new System.Drawing.Point(32, 192);
            this.ImportFinishedOnCheckBox.Name = "ImportFinishedOnCheckBox";
            this.ImportFinishedOnCheckBox.Size = new System.Drawing.Size(80, 17);
            this.ImportFinishedOnCheckBox.TabIndex = 17;
            this.ImportFinishedOnCheckBox.Text = "Finished on";
            this.ImportFinishedOnCheckBox.UseVisualStyleBackColor = true;
            // 
            // User_DL_ImportControls
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.ImportFinishedOnCheckBox);
            this.Controls.Add(this.ImportLastPlayedCheckBox);
            this.Controls.Add(this.ImportReleaseDateCheckBox);
            this.Controls.Add(this.ImportCommentCheckBox);
            this.Controls.Add(this.ImportSizeCheckBox);
            this.Controls.Add(this.ImportTitleCheckBox);
            this.Controls.Add(this.AutodetectCheckBox);
            this.Controls.Add(this.ImportFMDataCheckBox);
            this.Controls.Add(this.ImportSavesCheckBox);
            this.Controls.Add(this.ChooseDarkLoaderIniLabel);
            this.Controls.Add(this.DarkLoaderIniTextBox);
            this.Controls.Add(this.DarkLoaderIniBrowseButton);
            this.Name = "User_DL_ImportControls";
            this.Size = new System.Drawing.Size(540, 245);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox AutodetectCheckBox;
        private System.Windows.Forms.CheckBox ImportFMDataCheckBox;
        private System.Windows.Forms.CheckBox ImportSavesCheckBox;
        private System.Windows.Forms.Label ChooseDarkLoaderIniLabel;
        private System.Windows.Forms.TextBox DarkLoaderIniTextBox;
        private System.Windows.Forms.Button DarkLoaderIniBrowseButton;
        private System.Windows.Forms.CheckBox ImportTitleCheckBox;
        private System.Windows.Forms.CheckBox ImportSizeCheckBox;
        private System.Windows.Forms.CheckBox ImportCommentCheckBox;
        private System.Windows.Forms.CheckBox ImportReleaseDateCheckBox;
        private System.Windows.Forms.CheckBox ImportLastPlayedCheckBox;
        private System.Windows.Forms.CheckBox ImportFinishedOnCheckBox;
    }
}
