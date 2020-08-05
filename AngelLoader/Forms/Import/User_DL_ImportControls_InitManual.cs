using System.Drawing;
using System.Windows.Forms;

namespace AngelLoader.Forms.Import
{
    public sealed partial class User_DL_ImportControls
    {
        private void InitComponentManual()
        {
            AutodetectCheckBox = new CheckBox();
            ChooseDarkLoaderIniLabel = new Label();
            DarkLoaderIniTextBox = new TextBox();
            DarkLoaderIniBrowseButton = new Button();
            SuspendLayout();
            // 
            // AutodetectCheckBox
            // 
            AutodetectCheckBox.AutoSize = true;
            AutodetectCheckBox.Checked = true;
            AutodetectCheckBox.Location = new Point(8, 32);
            AutodetectCheckBox.TabIndex = 16;
            AutodetectCheckBox.UseVisualStyleBackColor = true;
            AutodetectCheckBox.CheckedChanged += AutodetectCheckBox_CheckedChanged;
            // 
            // ChooseDarkLoaderIniLabel
            // 
            ChooseDarkLoaderIniLabel.AutoSize = true;
            ChooseDarkLoaderIniLabel.Location = new Point(8, 8);
            ChooseDarkLoaderIniLabel.TabIndex = 11;
            // 
            // DarkLoaderIniTextBox
            // 
            DarkLoaderIniTextBox.Location = new Point(8, 56);
            DarkLoaderIniTextBox.ReadOnly = true;
            DarkLoaderIniTextBox.Size = new Size(440, 20);
            DarkLoaderIniTextBox.TabIndex = 12;
            // 
            // DarkLoaderIniBrowseButton
            // 
            DarkLoaderIniBrowseButton.AutoSize = true;
            DarkLoaderIniBrowseButton.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            DarkLoaderIniBrowseButton.MinimumSize = new Size(75, 23);
            DarkLoaderIniBrowseButton.Enabled = false;
            DarkLoaderIniBrowseButton.Location = new Point(448, 55);
            DarkLoaderIniBrowseButton.Padding = new Padding(6, 0, 6, 0);
            DarkLoaderIniBrowseButton.TabIndex = 13;
            DarkLoaderIniBrowseButton.UseVisualStyleBackColor = true;
            DarkLoaderIniBrowseButton.Click += DarkLoaderIniBrowseButton_Click;
            // 
            // User_DL_ImportControls
            // 
            AutoScaleDimensions = new SizeF(6F, 13F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(AutodetectCheckBox);
            Controls.Add(ChooseDarkLoaderIniLabel);
            Controls.Add(DarkLoaderIniTextBox);
            Controls.Add(DarkLoaderIniBrowseButton);
            Size = new Size(531, 88);
            ResumeLayout(false);
            PerformLayout();
        }
    }
}
