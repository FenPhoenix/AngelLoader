using System.Windows.Forms;
using AngelLoader.Forms.CustomControls;

namespace AngelLoader.Forms
{
    public partial class FilterDateForm
    {
        private void InitComponentManual()
        {
            Cancel_Button = new Button();
            OKButton = new Button();
            ResetButton = new Button();
            ToLabel = new Label();
            FromLabel = new Label();
            FromDateTimePicker = new DarkDateTimePicker();
            ToDateTimePicker = new DarkDateTimePicker();
            FromCheckBox = new CheckBox();
            ToCheckBox = new CheckBox();
            NoMinLabel = new TextBox();
            NoMaxLabel = new TextBox();
            SuspendLayout();
            // 
            // Cancel_Button
            // 
            Cancel_Button.DialogResult = DialogResult.Cancel;
            Cancel_Button.Location = new System.Drawing.Point(86, 128);
            Cancel_Button.Padding = new Padding(6, 0, 6, 0);
            Cancel_Button.Size = new System.Drawing.Size(75, 23);
            Cancel_Button.TabIndex = 0;
            Cancel_Button.UseVisualStyleBackColor = true;
            // 
            // OKButton
            // 
            OKButton.DialogResult = DialogResult.OK;
            OKButton.Location = new System.Drawing.Point(7, 128);
            OKButton.Padding = new Padding(6, 0, 6, 0);
            OKButton.Size = new System.Drawing.Size(75, 23);
            OKButton.TabIndex = 11;
            OKButton.UseVisualStyleBackColor = true;
            OKButton.Click += OKButton_Click;
            // 
            // ResetButton
            // 
            ResetButton.Location = new System.Drawing.Point(7, 88);
            ResetButton.Padding = new Padding(6, 0, 6, 0);
            ResetButton.Size = new System.Drawing.Size(154, 22);
            ResetButton.TabIndex = 10;
            ResetButton.UseVisualStyleBackColor = true;
            ResetButton.Click += ResetButton_Click;
            // 
            // ToLabel
            // 
            ToLabel.AutoSize = true;
            ToLabel.Location = new System.Drawing.Point(8, 48);
            ToLabel.Size = new System.Drawing.Size(23, 13);
            ToLabel.TabIndex = 6;
            // 
            // FromLabel
            // 
            FromLabel.AutoSize = true;
            FromLabel.Location = new System.Drawing.Point(8, 8);
            FromLabel.Size = new System.Drawing.Size(33, 13);
            FromLabel.TabIndex = 1;
            // 
            // FromDateTimePicker
            // 
            FromDateTimePicker.Format = DateTimePickerFormat.Short;
            FromDateTimePicker.Location = new System.Drawing.Point(28, 24);
            FromDateTimePicker.Size = new System.Drawing.Size(132, 20);
            FromDateTimePicker.TabIndex = 4;
            FromDateTimePicker.Visible = false;
            // 
            // ToDateTimePicker
            // 
            ToDateTimePicker.Format = DateTimePickerFormat.Short;
            ToDateTimePicker.Location = new System.Drawing.Point(28, 64);
            ToDateTimePicker.Size = new System.Drawing.Size(132, 20);
            ToDateTimePicker.TabIndex = 8;
            ToDateTimePicker.Visible = false;
            // 
            // FromCheckBox
            // 
            FromCheckBox.AutoSize = true;
            FromCheckBox.Location = new System.Drawing.Point(12, 27);
            FromCheckBox.Size = new System.Drawing.Size(15, 14);
            FromCheckBox.TabIndex = 3;
            FromCheckBox.UseVisualStyleBackColor = true;
            FromCheckBox.CheckedChanged += CheckBoxes_CheckedChanged;
            // 
            // ToCheckBox
            // 
            ToCheckBox.AutoSize = true;
            ToCheckBox.Location = new System.Drawing.Point(12, 67);
            ToCheckBox.Size = new System.Drawing.Size(15, 14);
            ToCheckBox.TabIndex = 7;
            ToCheckBox.UseVisualStyleBackColor = true;
            ToCheckBox.CheckedChanged += CheckBoxes_CheckedChanged;
            // 
            // NoMinLabel
            // 
            NoMinLabel.Enabled = false;
            NoMinLabel.Location = new System.Drawing.Point(56, 0);
            NoMinLabel.Size = new System.Drawing.Size(40, 20);
            NoMinLabel.TabIndex = 2;
            // 
            // NoMaxLabel
            // 
            NoMaxLabel.Enabled = false;
            NoMaxLabel.Location = new System.Drawing.Point(104, 0);
            NoMaxLabel.Size = new System.Drawing.Size(40, 20);
            NoMaxLabel.TabIndex = 5;
            // 
            // FilterDateForm
            // 
            AcceptButton = OKButton;
            AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = Cancel_Button;
            ClientSize = new System.Drawing.Size(170, 158);
            Controls.Add(NoMaxLabel);
            Controls.Add(NoMinLabel);
            Controls.Add(ToCheckBox);
            Controls.Add(FromCheckBox);
            Controls.Add(ToDateTimePicker);
            Controls.Add(FromDateTimePicker);
            Controls.Add(ResetButton);
            Controls.Add(ToLabel);
            Controls.Add(FromLabel);
            Controls.Add(Cancel_Button);
            Controls.Add(OKButton);
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            Icon = Images.AngelLoader;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.Manual;
            ResumeLayout(false);
            PerformLayout();
        }
    }
}
