using System.Drawing;
using System.Windows.Forms;

namespace AngelLoader.Forms.Import
{
    public sealed partial class User_FMSel_NDL_ImportControls
    {
        private void InitComponentManual()
        {
            Thief3GroupBox = new GroupBox();
            Thief3AutodetectCheckBox = new CheckBox();
            Thief3IniBrowseButton = new Button();
            Thief3IniTextBox = new TextBox();
            Thief2GroupBox = new GroupBox();
            Thief2AutodetectCheckBox = new CheckBox();
            Thief2IniBrowseButton = new Button();
            Thief2IniTextBox = new TextBox();
            Thief1GroupBox = new GroupBox();
            Thief1AutodetectCheckBox = new CheckBox();
            Thief1IniBrowseButton = new Button();
            Thief1IniTextBox = new TextBox();
            ChooseIniFilesLabel = new Label();
            SS2GroupBox = new GroupBox();
            SS2AutodetectCheckBox = new CheckBox();
            SS2IniBrowseButton = new Button();
            SS2IniTextBox = new TextBox();
            Thief3GroupBox.SuspendLayout();
            Thief2GroupBox.SuspendLayout();
            Thief1GroupBox.SuspendLayout();
            SS2GroupBox.SuspendLayout();
            SuspendLayout();
            // 
            // Thief3GroupBox
            // 
            Thief3GroupBox.Controls.Add(Thief3AutodetectCheckBox);
            Thief3GroupBox.Controls.Add(Thief3IniBrowseButton);
            Thief3GroupBox.Controls.Add(Thief3IniTextBox);
            Thief3GroupBox.Location = new Point(8, 216);
            Thief3GroupBox.Size = new Size(536, 88);
            Thief3GroupBox.TabIndex = 9;
            Thief3GroupBox.TabStop = false;
            // 
            // Thief3AutodetectCheckBox
            // 
            Thief3AutodetectCheckBox.AutoSize = true;
            Thief3AutodetectCheckBox.Checked = true;
            Thief3AutodetectCheckBox.Location = new Point(16, 24);
            Thief3AutodetectCheckBox.TabIndex = 4;
            Thief3AutodetectCheckBox.UseVisualStyleBackColor = true;
            Thief3AutodetectCheckBox.CheckedChanged += AutodetectCheckBoxes_CheckedChanged;
            // 
            // Thief3IniBrowseButton
            // 
            Thief3IniBrowseButton.AutoSize = true;
            Thief3IniBrowseButton.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            Thief3IniBrowseButton.MinimumSize = new Size(75, 23);
            Thief3IniBrowseButton.Enabled = false;
            Thief3IniBrowseButton.Location = new Point(447, 48);
            Thief3IniBrowseButton.Padding = new Padding(6, 0, 6, 0);
            Thief3IniBrowseButton.TabIndex = 1;
            Thief3IniBrowseButton.UseVisualStyleBackColor = true;
            Thief3IniBrowseButton.Click += ThiefIniBrowseButtons_Click;
            // 
            // Thief3IniTextBox
            // 
            Thief3IniTextBox.Location = new Point(15, 49);
            Thief3IniTextBox.ReadOnly = true;
            Thief3IniTextBox.Size = new Size(432, 20);
            Thief3IniTextBox.TabIndex = 0;
            // 
            // Thief2GroupBox
            // 
            Thief2GroupBox.Controls.Add(Thief2AutodetectCheckBox);
            Thief2GroupBox.Controls.Add(Thief2IniBrowseButton);
            Thief2GroupBox.Controls.Add(Thief2IniTextBox);
            Thief2GroupBox.Location = new Point(8, 120);
            Thief2GroupBox.Size = new Size(536, 88);
            Thief2GroupBox.TabIndex = 8;
            Thief2GroupBox.TabStop = false;
            // 
            // Thief2AutodetectCheckBox
            // 
            Thief2AutodetectCheckBox.AutoSize = true;
            Thief2AutodetectCheckBox.Checked = true;
            Thief2AutodetectCheckBox.Location = new Point(16, 24);
            Thief2AutodetectCheckBox.TabIndex = 3;
            Thief2AutodetectCheckBox.UseVisualStyleBackColor = true;
            Thief2AutodetectCheckBox.CheckedChanged += AutodetectCheckBoxes_CheckedChanged;
            // 
            // Thief2IniBrowseButton
            // 
            Thief2IniBrowseButton.AutoSize = true;
            Thief2IniBrowseButton.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            Thief2IniBrowseButton.MinimumSize = new Size(75, 23);
            Thief2IniBrowseButton.Enabled = false;
            Thief2IniBrowseButton.Location = new Point(447, 47);
            Thief2IniBrowseButton.Padding = new Padding(6, 0, 6, 0);
            Thief2IniBrowseButton.TabIndex = 1;
            Thief2IniBrowseButton.UseVisualStyleBackColor = true;
            Thief2IniBrowseButton.Click += ThiefIniBrowseButtons_Click;
            // 
            // Thief2IniTextBox
            // 
            Thief2IniTextBox.Location = new Point(15, 48);
            Thief2IniTextBox.ReadOnly = true;
            Thief2IniTextBox.Size = new Size(432, 20);
            Thief2IniTextBox.TabIndex = 0;
            // 
            // Thief1GroupBox
            // 
            Thief1GroupBox.Controls.Add(Thief1AutodetectCheckBox);
            Thief1GroupBox.Controls.Add(Thief1IniBrowseButton);
            Thief1GroupBox.Controls.Add(Thief1IniTextBox);
            Thief1GroupBox.Location = new Point(8, 32);
            Thief1GroupBox.Size = new Size(536, 80);
            Thief1GroupBox.TabIndex = 7;
            Thief1GroupBox.TabStop = false;
            // 
            // Thief1AutodetectCheckBox
            // 
            Thief1AutodetectCheckBox.AutoSize = true;
            Thief1AutodetectCheckBox.Checked = true;
            Thief1AutodetectCheckBox.Location = new Point(16, 24);
            Thief1AutodetectCheckBox.TabIndex = 2;
            Thief1AutodetectCheckBox.UseVisualStyleBackColor = true;
            Thief1AutodetectCheckBox.CheckedChanged += AutodetectCheckBoxes_CheckedChanged;
            // 
            // Thief1IniBrowseButton
            // 
            Thief1IniBrowseButton.AutoSize = true;
            Thief1IniBrowseButton.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            Thief1IniBrowseButton.MinimumSize = new Size(75, 23);
            Thief1IniBrowseButton.Enabled = false;
            Thief1IniBrowseButton.Location = new Point(448, 47);
            Thief1IniBrowseButton.Padding = new Padding(6, 0, 6, 0);
            Thief1IniBrowseButton.TabIndex = 1;
            Thief1IniBrowseButton.UseVisualStyleBackColor = true;
            Thief1IniBrowseButton.Click += ThiefIniBrowseButtons_Click;
            // 
            // Thief1IniTextBox
            // 
            Thief1IniTextBox.Location = new Point(16, 48);
            Thief1IniTextBox.ReadOnly = true;
            Thief1IniTextBox.Size = new Size(432, 20);
            Thief1IniTextBox.TabIndex = 0;
            // 
            // ChooseIniFilesLabel
            // 
            ChooseIniFilesLabel.AutoSize = true;
            ChooseIniFilesLabel.Location = new Point(16, 8);
            ChooseIniFilesLabel.TabIndex = 6;
            // 
            // SS2GroupBox
            // 
            SS2GroupBox.Controls.Add(SS2AutodetectCheckBox);
            SS2GroupBox.Controls.Add(SS2IniBrowseButton);
            SS2GroupBox.Controls.Add(SS2IniTextBox);
            SS2GroupBox.Location = new Point(8, 312);
            SS2GroupBox.Size = new Size(536, 88);
            SS2GroupBox.TabIndex = 9;
            SS2GroupBox.TabStop = false;
            // 
            // SS2AutodetectCheckBox
            // 
            SS2AutodetectCheckBox.AutoSize = true;
            SS2AutodetectCheckBox.Checked = true;
            SS2AutodetectCheckBox.Location = new Point(16, 24);
            SS2AutodetectCheckBox.TabIndex = 4;
            SS2AutodetectCheckBox.UseVisualStyleBackColor = true;
            SS2AutodetectCheckBox.CheckedChanged += AutodetectCheckBoxes_CheckedChanged;
            // 
            // SS2IniBrowseButton
            // 
            SS2IniBrowseButton.AutoSize = true;
            SS2IniBrowseButton.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            SS2IniBrowseButton.MinimumSize = new Size(75, 23);
            SS2IniBrowseButton.Enabled = false;
            SS2IniBrowseButton.Location = new Point(447, 48);
            SS2IniBrowseButton.Padding = new Padding(6, 0, 6, 0);
            SS2IniBrowseButton.TabIndex = 1;
            SS2IniBrowseButton.UseVisualStyleBackColor = true;
            SS2IniBrowseButton.Click += ThiefIniBrowseButtons_Click;
            // 
            // SS2IniTextBox
            // 
            SS2IniTextBox.Location = new Point(15, 49);
            SS2IniTextBox.ReadOnly = true;
            SS2IniTextBox.Size = new Size(432, 20);
            SS2IniTextBox.TabIndex = 0;
            // 
            // User_FMSel_NDL_ImportControls
            // 
            AutoScaleDimensions = new SizeF(6F, 13F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(SS2GroupBox);
            Controls.Add(Thief3GroupBox);
            Controls.Add(Thief2GroupBox);
            Controls.Add(Thief1GroupBox);
            Controls.Add(ChooseIniFilesLabel);
            Size = new Size(551, 410);
            Thief3GroupBox.ResumeLayout(false);
            Thief3GroupBox.PerformLayout();
            Thief2GroupBox.ResumeLayout(false);
            Thief2GroupBox.PerformLayout();
            Thief1GroupBox.ResumeLayout(false);
            Thief1GroupBox.PerformLayout();
            SS2GroupBox.ResumeLayout(false);
            SS2GroupBox.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }
    }
}
