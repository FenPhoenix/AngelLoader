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
            // 
            // ImportSavesCheckBox
            // 
            this.ImportSavesCheckBox.AutoSize = true;
            this.ImportSavesCheckBox.Checked = true;
            this.ImportSavesCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ImportSavesCheckBox.Location = new System.Drawing.Point(16, 104);
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
            // User_DL_ImportControls
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.AutodetectCheckBox);
            this.Controls.Add(this.ImportFMDataCheckBox);
            this.Controls.Add(this.ImportSavesCheckBox);
            this.Controls.Add(this.ChooseDarkLoaderIniLabel);
            this.Controls.Add(this.DarkLoaderIniTextBox);
            this.Controls.Add(this.DarkLoaderIniBrowseButton);
            this.Name = "User_DL_ImportControls";
            this.Size = new System.Drawing.Size(540, 131);
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
    }
}
