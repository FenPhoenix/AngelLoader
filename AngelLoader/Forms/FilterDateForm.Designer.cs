namespace AngelLoader.Forms
{
    partial class FilterDateForm
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
            this.Cancel_Button = new System.Windows.Forms.Button();
            this.OKButton = new System.Windows.Forms.Button();
            this.ResetButton = new System.Windows.Forms.Button();
            this.ToLabel = new System.Windows.Forms.Label();
            this.FromLabel = new System.Windows.Forms.Label();
            this.FromDateTimePicker = new AngelLoader.Forms.CustomControls.DarkDateTimePicker();
            this.ToDateTimePicker = new AngelLoader.Forms.CustomControls.DarkDateTimePicker();
            this.FromCheckBox = new System.Windows.Forms.CheckBox();
            this.ToCheckBox = new System.Windows.Forms.CheckBox();
            this.NoMinLabel = new System.Windows.Forms.TextBox();
            this.NoMaxLabel = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // Cancel_Button
            // 
            this.Cancel_Button.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Cancel_Button.Location = new System.Drawing.Point(86, 128);
            this.Cancel_Button.Name = "Cancel_Button";
            this.Cancel_Button.Padding = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.Cancel_Button.Size = new System.Drawing.Size(75, 23);
            this.Cancel_Button.TabIndex = 0;
            this.Cancel_Button.Text = "Cancel";
            this.Cancel_Button.UseVisualStyleBackColor = true;
            // 
            // OKButton
            // 
            this.OKButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.OKButton.Location = new System.Drawing.Point(7, 128);
            this.OKButton.Name = "OKButton";
            this.OKButton.Padding = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.OKButton.Size = new System.Drawing.Size(75, 23);
            this.OKButton.TabIndex = 11;
            this.OKButton.Text = "OK";
            this.OKButton.UseVisualStyleBackColor = true;
            this.OKButton.Click += new System.EventHandler(this.OKButton_Click);
            // 
            // ResetButton
            // 
            this.ResetButton.Location = new System.Drawing.Point(7, 88);
            this.ResetButton.Name = "ResetButton";
            this.ResetButton.Padding = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.ResetButton.Size = new System.Drawing.Size(154, 22);
            this.ResetButton.TabIndex = 10;
            this.ResetButton.Text = "Reset";
            this.ResetButton.UseVisualStyleBackColor = true;
            this.ResetButton.Click += new System.EventHandler(this.ResetButton_Click);
            // 
            // ToLabel
            // 
            this.ToLabel.AutoSize = true;
            this.ToLabel.Location = new System.Drawing.Point(8, 48);
            this.ToLabel.Name = "ToLabel";
            this.ToLabel.Size = new System.Drawing.Size(23, 13);
            this.ToLabel.TabIndex = 6;
            this.ToLabel.Text = "To:";
            // 
            // FromLabel
            // 
            this.FromLabel.AutoSize = true;
            this.FromLabel.Location = new System.Drawing.Point(8, 8);
            this.FromLabel.Name = "FromLabel";
            this.FromLabel.Size = new System.Drawing.Size(33, 13);
            this.FromLabel.TabIndex = 1;
            this.FromLabel.Text = "From:";
            // 
            // FromDateTimePicker
            // 
            this.FromDateTimePicker.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.FromDateTimePicker.Location = new System.Drawing.Point(28, 24);
            this.FromDateTimePicker.Name = "FromDateTimePicker";
            this.FromDateTimePicker.Size = new System.Drawing.Size(132, 20);
            this.FromDateTimePicker.TabIndex = 4;
            this.FromDateTimePicker.Visible = false;
            // 
            // ToDateTimePicker
            // 
            this.ToDateTimePicker.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.ToDateTimePicker.Location = new System.Drawing.Point(28, 64);
            this.ToDateTimePicker.Name = "ToDateTimePicker";
            this.ToDateTimePicker.Size = new System.Drawing.Size(132, 20);
            this.ToDateTimePicker.TabIndex = 8;
            this.ToDateTimePicker.Visible = false;
            // 
            // FromCheckBox
            // 
            this.FromCheckBox.AutoSize = true;
            this.FromCheckBox.Location = new System.Drawing.Point(12, 27);
            this.FromCheckBox.Name = "FromCheckBox";
            this.FromCheckBox.Size = new System.Drawing.Size(15, 14);
            this.FromCheckBox.TabIndex = 3;
            this.FromCheckBox.UseVisualStyleBackColor = true;
            this.FromCheckBox.CheckedChanged += new System.EventHandler(this.CheckBoxes_CheckedChanged);
            // 
            // ToCheckBox
            // 
            this.ToCheckBox.AutoSize = true;
            this.ToCheckBox.Location = new System.Drawing.Point(12, 67);
            this.ToCheckBox.Name = "ToCheckBox";
            this.ToCheckBox.Size = new System.Drawing.Size(15, 14);
            this.ToCheckBox.TabIndex = 7;
            this.ToCheckBox.UseVisualStyleBackColor = true;
            this.ToCheckBox.CheckedChanged += new System.EventHandler(this.CheckBoxes_CheckedChanged);
            // 
            // NoMinLabel
            // 
            this.NoMinLabel.Enabled = false;
            this.NoMinLabel.Location = new System.Drawing.Point(56, 0);
            this.NoMinLabel.Name = "NoMinLabel";
            this.NoMinLabel.Size = new System.Drawing.Size(40, 20);
            this.NoMinLabel.TabIndex = 2;
            this.NoMinLabel.Text = "(no minimum)";
            // 
            // NoMaxLabel
            // 
            this.NoMaxLabel.Enabled = false;
            this.NoMaxLabel.Location = new System.Drawing.Point(104, 0);
            this.NoMaxLabel.Name = "NoMaxLabel";
            this.NoMaxLabel.Size = new System.Drawing.Size(40, 20);
            this.NoMaxLabel.TabIndex = 5;
            this.NoMaxLabel.Text = "(no maximum)";
            // 
            // FilterDateForm
            // 
            this.AcceptButton = this.OKButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.Cancel_Button;
            this.ClientSize = new System.Drawing.Size(170, 158);
            this.Controls.Add(this.NoMaxLabel);
            this.Controls.Add(this.NoMinLabel);
            this.Controls.Add(this.ToCheckBox);
            this.Controls.Add(this.FromCheckBox);
            this.Controls.Add(this.ToDateTimePicker);
            this.Controls.Add(this.FromDateTimePicker);
            this.Controls.Add(this.ResetButton);
            this.Controls.Add(this.ToLabel);
            this.Controls.Add(this.FromLabel);
            this.Controls.Add(this.Cancel_Button);
            this.Controls.Add(this.OKButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Icon = global::AngelLoader.Properties.Resources.AngelLoader;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FilterDateForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Set release date filter";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
#endif

        private System.Windows.Forms.Button Cancel_Button;
        private System.Windows.Forms.Button OKButton;
        private System.Windows.Forms.Button ResetButton;
        private System.Windows.Forms.Label ToLabel;
        private System.Windows.Forms.Label FromLabel;
        private AngelLoader.Forms.CustomControls.DarkDateTimePicker FromDateTimePicker;
        private AngelLoader.Forms.CustomControls.DarkDateTimePicker ToDateTimePicker;
        private System.Windows.Forms.CheckBox FromCheckBox;
        private System.Windows.Forms.CheckBox ToCheckBox;
        private System.Windows.Forms.TextBox NoMinLabel;
        private System.Windows.Forms.TextBox NoMaxLabel;
    }
}