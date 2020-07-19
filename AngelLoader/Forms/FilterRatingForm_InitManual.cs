using System.Windows.Forms;

namespace AngelLoader.Forms
{
    public sealed partial class FilterRatingForm
    {
        private void InitComponentManual()
        {
            FromLabel = new Label();
            ToLabel = new Label();
            FromComboBox = new ComboBox();
            ToComboBox = new ComboBox();
            OKButton = new Button();
            Cancel_Button = new Button();
            ResetButton = new Button();
            SuspendLayout();
            // 
            // FromLabel
            // 
            FromLabel.AutoSize = true;
            FromLabel.Location = new System.Drawing.Point(8, 8);
            FromLabel.Size = new System.Drawing.Size(33, 13);
            FromLabel.TabIndex = 1;
            // 
            // ToLabel
            // 
            ToLabel.AutoSize = true;
            ToLabel.Location = new System.Drawing.Point(8, 48);
            ToLabel.Size = new System.Drawing.Size(23, 13);
            ToLabel.TabIndex = 3;
            // 
            // FromComboBox
            // 
            FromComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            FromComboBox.FormattingEnabled = true;
            FromComboBox.Location = new System.Drawing.Point(8, 24);
            FromComboBox.Size = new System.Drawing.Size(152, 21);
            FromComboBox.TabIndex = 2;
            FromComboBox.SelectedIndexChanged += ComboBoxes_SelectedIndexChanged;
            // 
            // ToComboBox
            // 
            ToComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            ToComboBox.FormattingEnabled = true;
            ToComboBox.Location = new System.Drawing.Point(8, 64);
            ToComboBox.Size = new System.Drawing.Size(152, 21);
            ToComboBox.TabIndex = 4;
            ToComboBox.SelectedIndexChanged += ComboBoxes_SelectedIndexChanged;
            // 
            // OKButton
            // 
            OKButton.DialogResult = DialogResult.OK;
            OKButton.Location = new System.Drawing.Point(7, 128);
            OKButton.Padding = new Padding(6, 0, 6, 0);
            OKButton.Size = new System.Drawing.Size(75, 23);
            OKButton.TabIndex = 6;
            OKButton.UseVisualStyleBackColor = true;
            OKButton.Click += OKButton_Click;
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
            // ResetButton
            // 
            ResetButton.Location = new System.Drawing.Point(7, 88);
            ResetButton.Padding = new Padding(6, 0, 6, 0);
            ResetButton.Size = new System.Drawing.Size(154, 22);
            ResetButton.TabIndex = 5;
            ResetButton.UseVisualStyleBackColor = true;
            ResetButton.Click += ResetButton_Click;
            // 
            // FilterRatingForm
            // 
            AcceptButton = OKButton;
            AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = Cancel_Button;
            ClientSize = new System.Drawing.Size(170, 158);
            Controls.Add(ResetButton);
            Controls.Add(Cancel_Button);
            Controls.Add(OKButton);
            Controls.Add(ToComboBox);
            Controls.Add(FromComboBox);
            Controls.Add(ToLabel);
            Controls.Add(FromLabel);
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
