namespace AngelLoader.Forms
{
    partial class ImportFromDarkLoaderForm
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.ImportFMDataCheckBox = new System.Windows.Forms.CheckBox();
            this.ImportSavesCheckBox = new System.Windows.Forms.CheckBox();
            this.ChooseDarkLoaderIniLabel = new System.Windows.Forms.Label();
            this.DarkLoaderIniTextBox = new System.Windows.Forms.TextBox();
            this.DarkLoaderIniBrowseButton = new System.Windows.Forms.Button();
            this.OKButton = new System.Windows.Forms.Button();
            this.Cancel_Button = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // ImportFMDataCheckBox
            // 
            this.ImportFMDataCheckBox.AutoSize = true;
            this.ImportFMDataCheckBox.Checked = true;
            this.ImportFMDataCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ImportFMDataCheckBox.Location = new System.Drawing.Point(16, 56);
            this.ImportFMDataCheckBox.Name = "ImportFMDataCheckBox";
            this.ImportFMDataCheckBox.Size = new System.Drawing.Size(97, 17);
            this.ImportFMDataCheckBox.TabIndex = 3;
            this.ImportFMDataCheckBox.Text = "Import FM data";
            this.ImportFMDataCheckBox.UseVisualStyleBackColor = true;
            // 
            // ImportSavesCheckBox
            // 
            this.ImportSavesCheckBox.AutoSize = true;
            this.ImportSavesCheckBox.Checked = true;
            this.ImportSavesCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ImportSavesCheckBox.Location = new System.Drawing.Point(16, 72);
            this.ImportSavesCheckBox.Name = "ImportSavesCheckBox";
            this.ImportSavesCheckBox.Size = new System.Drawing.Size(86, 17);
            this.ImportSavesCheckBox.TabIndex = 4;
            this.ImportSavesCheckBox.Text = "Import saves";
            this.ImportSavesCheckBox.UseVisualStyleBackColor = true;
            // 
            // ChooseDarkLoaderIniLabel
            // 
            this.ChooseDarkLoaderIniLabel.AutoSize = true;
            this.ChooseDarkLoaderIniLabel.Location = new System.Drawing.Point(16, 16);
            this.ChooseDarkLoaderIniLabel.Name = "ChooseDarkLoaderIniLabel";
            this.ChooseDarkLoaderIniLabel.Size = new System.Drawing.Size(118, 13);
            this.ChooseDarkLoaderIniLabel.TabIndex = 8;
            this.ChooseDarkLoaderIniLabel.Text = "Choose DarkLoader.ini:";
            // 
            // DarkLoaderIniTextBox
            // 
            this.DarkLoaderIniTextBox.Location = new System.Drawing.Point(16, 32);
            this.DarkLoaderIniTextBox.Name = "DarkLoaderIniTextBox";
            this.DarkLoaderIniTextBox.Size = new System.Drawing.Size(440, 20);
            this.DarkLoaderIniTextBox.TabIndex = 1;
            // 
            // DarkLoaderIniBrowseButton
            // 
            this.DarkLoaderIniBrowseButton.Location = new System.Drawing.Point(456, 31);
            this.DarkLoaderIniBrowseButton.Name = "DarkLoaderIniBrowseButton";
            this.DarkLoaderIniBrowseButton.Size = new System.Drawing.Size(75, 23);
            this.DarkLoaderIniBrowseButton.TabIndex = 2;
            this.DarkLoaderIniBrowseButton.Text = "Browse...";
            this.DarkLoaderIniBrowseButton.UseVisualStyleBackColor = true;
            this.DarkLoaderIniBrowseButton.Click += new System.EventHandler(this.DarkLoaderIniBrowseButton_Click);
            // 
            // OKButton
            // 
            this.OKButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.OKButton.Location = new System.Drawing.Point(384, 144);
            this.OKButton.Name = "OKButton";
            this.OKButton.Size = new System.Drawing.Size(75, 23);
            this.OKButton.TabIndex = 5;
            this.OKButton.Text = "OK";
            this.OKButton.UseVisualStyleBackColor = true;
            // 
            // Cancel_Button
            // 
            this.Cancel_Button.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Cancel_Button.Location = new System.Drawing.Point(464, 144);
            this.Cancel_Button.Name = "Cancel_Button";
            this.Cancel_Button.Size = new System.Drawing.Size(75, 23);
            this.Cancel_Button.TabIndex = 0;
            this.Cancel_Button.Text = "Cancel";
            this.Cancel_Button.UseVisualStyleBackColor = true;
            // 
            // ImportFromDarkLoaderForm
            // 
            this.AcceptButton = this.OKButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.Cancel_Button;
            this.ClientSize = new System.Drawing.Size(547, 175);
            this.Controls.Add(this.Cancel_Button);
            this.Controls.Add(this.OKButton);
            this.Controls.Add(this.ImportFMDataCheckBox);
            this.Controls.Add(this.ImportSavesCheckBox);
            this.Controls.Add(this.ChooseDarkLoaderIniLabel);
            this.Controls.Add(this.DarkLoaderIniTextBox);
            this.Controls.Add(this.DarkLoaderIniBrowseButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ImportFromDarkLoaderForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "ImportFromDarkLoaderForm";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ImportFromDarkLoaderForm_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.CheckBox ImportFMDataCheckBox;
        private System.Windows.Forms.CheckBox ImportSavesCheckBox;
        private System.Windows.Forms.Label ChooseDarkLoaderIniLabel;
        private System.Windows.Forms.TextBox DarkLoaderIniTextBox;
        private System.Windows.Forms.Button DarkLoaderIniBrowseButton;
        private System.Windows.Forms.Button OKButton;
        private System.Windows.Forms.Button Cancel_Button;
    }
}