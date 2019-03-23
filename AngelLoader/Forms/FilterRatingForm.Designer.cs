namespace AngelLoader.Forms
{
    partial class FilterRatingForm
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
            this.FromLabel = new System.Windows.Forms.Label();
            this.ToLabel = new System.Windows.Forms.Label();
            this.FromComboBox = new System.Windows.Forms.ComboBox();
            this.ToComboBox = new System.Windows.Forms.ComboBox();
            this.OKButton = new System.Windows.Forms.Button();
            this.Cancel_Button = new System.Windows.Forms.Button();
            this.ResetButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // FromLabel
            // 
            this.FromLabel.AutoSize = true;
            this.FromLabel.Location = new System.Drawing.Point(8, 8);
            this.FromLabel.Name = "FromLabel";
            this.FromLabel.Size = new System.Drawing.Size(33, 13);
            this.FromLabel.TabIndex = 0;
            this.FromLabel.Text = "From:";
            // 
            // ToLabel
            // 
            this.ToLabel.AutoSize = true;
            this.ToLabel.Location = new System.Drawing.Point(8, 48);
            this.ToLabel.Name = "ToLabel";
            this.ToLabel.Size = new System.Drawing.Size(23, 13);
            this.ToLabel.TabIndex = 2;
            this.ToLabel.Text = "To:";
            // 
            // FromComboBox
            // 
            this.FromComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.FromComboBox.FormattingEnabled = true;
            this.FromComboBox.Location = new System.Drawing.Point(8, 24);
            this.FromComboBox.Name = "FromComboBox";
            this.FromComboBox.Size = new System.Drawing.Size(152, 21);
            this.FromComboBox.TabIndex = 1;
            this.FromComboBox.SelectedIndexChanged += new System.EventHandler(this.ComboBoxes_SelectedIndexChanged);
            // 
            // ToComboBox
            // 
            this.ToComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ToComboBox.FormattingEnabled = true;
            this.ToComboBox.Location = new System.Drawing.Point(8, 64);
            this.ToComboBox.Name = "ToComboBox";
            this.ToComboBox.Size = new System.Drawing.Size(152, 21);
            this.ToComboBox.TabIndex = 3;
            this.ToComboBox.SelectedIndexChanged += new System.EventHandler(this.ComboBoxes_SelectedIndexChanged);
            // 
            // OKButton
            // 
            this.OKButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.OKButton.Location = new System.Drawing.Point(7, 128);
            this.OKButton.Name = "OKButton";
            this.OKButton.Padding = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.OKButton.Size = new System.Drawing.Size(75, 23);
            this.OKButton.TabIndex = 5;
            this.OKButton.Text = "OK";
            this.OKButton.UseVisualStyleBackColor = true;
            this.OKButton.Click += new System.EventHandler(this.OKButton_Click);
            // 
            // Cancel_Button
            // 
            this.Cancel_Button.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Cancel_Button.Location = new System.Drawing.Point(86, 128);
            this.Cancel_Button.Name = "Cancel_Button";
            this.Cancel_Button.Padding = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.Cancel_Button.Size = new System.Drawing.Size(75, 23);
            this.Cancel_Button.TabIndex = 6;
            this.Cancel_Button.Text = "Cancel";
            this.Cancel_Button.UseVisualStyleBackColor = true;
            // 
            // ResetButton
            // 
            this.ResetButton.Location = new System.Drawing.Point(7, 88);
            this.ResetButton.Name = "ResetButton";
            this.ResetButton.Padding = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.ResetButton.Size = new System.Drawing.Size(154, 22);
            this.ResetButton.TabIndex = 4;
            this.ResetButton.Text = "Reset";
            this.ResetButton.UseVisualStyleBackColor = true;
            this.ResetButton.Click += new System.EventHandler(this.ResetButton_Click);
            // 
            // FilterRatingForm
            // 
            this.AcceptButton = this.OKButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.Cancel_Button;
            this.ClientSize = new System.Drawing.Size(170, 158);
            this.Controls.Add(this.ResetButton);
            this.Controls.Add(this.Cancel_Button);
            this.Controls.Add(this.OKButton);
            this.Controls.Add(this.ToComboBox);
            this.Controls.Add(this.FromComboBox);
            this.Controls.Add(this.ToLabel);
            this.Controls.Add(this.FromLabel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FilterRatingForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Set rating filter";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label FromLabel;
        private System.Windows.Forms.Label ToLabel;
        private System.Windows.Forms.ComboBox FromComboBox;
        private System.Windows.Forms.ComboBox ToComboBox;
        private System.Windows.Forms.Button OKButton;
        private System.Windows.Forms.Button Cancel_Button;
        private System.Windows.Forms.Button ResetButton;
    }
}