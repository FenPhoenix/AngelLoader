﻿namespace AngelLoader.Forms
{
    partial class FilterDateForm
    {
        /// <summary>
        /// Custom generated component initializer with cruft removed.
        /// </summary>
        private void InitializeComponentSlim()
        {
            this.Cancel_Button = new AngelLoader.Forms.CustomControls.DarkButton();
            this.OKButton = new AngelLoader.Forms.CustomControls.DarkButton();
            this.ResetButton = new AngelLoader.Forms.CustomControls.DarkButton();
            this.ToLabel = new AngelLoader.Forms.CustomControls.DarkLabel();
            this.FromLabel = new AngelLoader.Forms.CustomControls.DarkLabel();
            this.FromDateTimePicker = new AngelLoader.Forms.CustomControls.DarkDateTimePicker();
            this.ToDateTimePicker = new AngelLoader.Forms.CustomControls.DarkDateTimePicker();
            this.FromCheckBox = new AngelLoader.Forms.CustomControls.DarkCheckBox();
            this.ToCheckBox = new AngelLoader.Forms.CustomControls.DarkCheckBox();
            this.NoMinLabel = new AngelLoader.Forms.CustomControls.DarkTextBox();
            this.NoMaxLabel = new AngelLoader.Forms.CustomControls.DarkTextBox();
            this.SuspendLayout();
            // 
            // Cancel_Button
            // 
            this.Cancel_Button.AutoSize = true;
            this.Cancel_Button.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.Cancel_Button.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Cancel_Button.Location = new System.Drawing.Point(86, 128);
            this.Cancel_Button.MinimumSize = new System.Drawing.Size(75, 23);
            this.Cancel_Button.Padding = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.Cancel_Button.TabIndex = 0;
            this.Cancel_Button.UseVisualStyleBackColor = true;
            // 
            // OKButton
            // 
            this.OKButton.AutoSize = true;
            this.OKButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.OKButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.OKButton.Location = new System.Drawing.Point(7, 128);
            this.OKButton.MinimumSize = new System.Drawing.Size(75, 23);
            this.OKButton.Padding = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.OKButton.TabIndex = 11;
            this.OKButton.UseVisualStyleBackColor = true;
            this.OKButton.Click += new System.EventHandler(this.OKButton_Click);
            // 
            // ResetButton
            // 
            this.ResetButton.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ResetButton.Location = new System.Drawing.Point(7, 88);
            this.ResetButton.Padding = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.ResetButton.Size = new System.Drawing.Size(154, 22);
            this.ResetButton.TabIndex = 10;
            this.ResetButton.UseVisualStyleBackColor = true;
            this.ResetButton.Click += new System.EventHandler(this.ResetButton_Click);
            // 
            // ToLabel
            // 
            this.ToLabel.AutoSize = true;
            this.ToLabel.Location = new System.Drawing.Point(8, 48);
            this.ToLabel.TabIndex = 6;
            // 
            // FromLabel
            // 
            this.FromLabel.AutoSize = true;
            this.FromLabel.Location = new System.Drawing.Point(8, 8);
            this.FromLabel.TabIndex = 1;
            // 
            // FromDateTimePicker
            // 
            this.FromDateTimePicker.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.FromDateTimePicker.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.FromDateTimePicker.Location = new System.Drawing.Point(28, 24);
            this.FromDateTimePicker.Size = new System.Drawing.Size(132, 20);
            this.FromDateTimePicker.TabIndex = 4;
            this.FromDateTimePicker.Visible = false;
            // 
            // ToDateTimePicker
            // 
            this.ToDateTimePicker.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ToDateTimePicker.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.ToDateTimePicker.Location = new System.Drawing.Point(28, 64);
            this.ToDateTimePicker.Size = new System.Drawing.Size(132, 20);
            this.ToDateTimePicker.TabIndex = 8;
            this.ToDateTimePicker.Visible = false;
            // 
            // FromCheckBox
            // 
            this.FromCheckBox.AutoSize = true;
            this.FromCheckBox.Location = new System.Drawing.Point(12, 27);
            this.FromCheckBox.TabIndex = 3;
            this.FromCheckBox.UseVisualStyleBackColor = true;
            this.FromCheckBox.CheckedChanged += new System.EventHandler(this.CheckBoxes_CheckedChanged);
            // 
            // ToCheckBox
            // 
            this.ToCheckBox.AutoSize = true;
            this.ToCheckBox.Location = new System.Drawing.Point(12, 67);
            this.ToCheckBox.TabIndex = 7;
            this.ToCheckBox.UseVisualStyleBackColor = true;
            this.ToCheckBox.CheckedChanged += new System.EventHandler(this.CheckBoxes_CheckedChanged);
            // 
            // NoMinLabel
            // 
            this.NoMinLabel.Enabled = false;
            this.NoMinLabel.Location = new System.Drawing.Point(56, 0);
            this.NoMinLabel.Size = new System.Drawing.Size(40, 20);
            this.NoMinLabel.TabIndex = 2;
            // 
            // NoMaxLabel
            // 
            this.NoMaxLabel.Enabled = false;
            this.NoMaxLabel.Location = new System.Drawing.Point(104, 0);
            this.NoMaxLabel.Size = new System.Drawing.Size(40, 20);
            this.NoMaxLabel.TabIndex = 5;
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
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            // Hack to prevent slow first render on some forms if Text is blank
            this.Text = " ";
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}
