using System.Drawing;
using System.Windows.Forms;
using AngelLoader.Properties;

namespace AngelLoader.Forms
{
    public sealed partial class GameVersionsForm
    {
        private void InitComponentManual()
        {
            T1VersionLabel = new Label();
            T1VersionTextBox = new TextBox();
            T2VersionLabel = new Label();
            T2VersionTextBox = new TextBox();
            T3VersionLabel = new Label();
            T3VersionTextBox = new TextBox();
            SS2VersionLabel = new Label();
            SS2VersionTextBox = new TextBox();
            OKButton = new Button();
            SuspendLayout();
            // 
            // T1VersionLabel
            // 
            T1VersionLabel.AutoSize = true;
            T1VersionLabel.Location = new Point(11, 11);
            T1VersionLabel.TabIndex = 1;
            // 
            // T1VersionTextBox
            // 
            T1VersionTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            T1VersionTextBox.Location = new Point(147, 8);
            T1VersionTextBox.MaximumSize = new Size(224, 32767);
            T1VersionTextBox.MinimumSize = new Size(80, 4);
            T1VersionTextBox.ReadOnly = true;
            T1VersionTextBox.Size = new Size(224, 20);
            T1VersionTextBox.TabIndex = 2;
            // 
            // T2VersionLabel
            // 
            T2VersionLabel.AutoSize = true;
            T2VersionLabel.Location = new Point(11, 35);
            T2VersionLabel.TabIndex = 3;
            // 
            // T2VersionTextBox
            // 
            T2VersionTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            T2VersionTextBox.Location = new Point(147, 32);
            T2VersionTextBox.MaximumSize = new Size(224, 32767);
            T2VersionTextBox.MinimumSize = new Size(80, 4);
            T2VersionTextBox.ReadOnly = true;
            T2VersionTextBox.Size = new Size(224, 20);
            T2VersionTextBox.TabIndex = 4;
            // 
            // T3VersionLabel
            // 
            T3VersionLabel.AutoSize = true;
            T3VersionLabel.Location = new Point(11, 59);
            T3VersionLabel.TabIndex = 5;
            // 
            // T3VersionTextBox
            // 
            T3VersionTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            T3VersionTextBox.Location = new Point(147, 56);
            T3VersionTextBox.MaximumSize = new Size(224, 32767);
            T3VersionTextBox.MinimumSize = new Size(80, 4);
            T3VersionTextBox.ReadOnly = true;
            T3VersionTextBox.Size = new Size(224, 20);
            T3VersionTextBox.TabIndex = 6;
            // 
            // SS2VersionLabel
            // 
            SS2VersionLabel.AutoSize = true;
            SS2VersionLabel.Location = new Point(11, 83);
            SS2VersionLabel.TabIndex = 7;
            // 
            // SS2VersionTextBox
            // 
            SS2VersionTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            SS2VersionTextBox.Location = new Point(147, 80);
            SS2VersionTextBox.MaximumSize = new Size(224, 32767);
            SS2VersionTextBox.MinimumSize = new Size(80, 4);
            SS2VersionTextBox.ReadOnly = true;
            SS2VersionTextBox.Size = new Size(224, 20);
            SS2VersionTextBox.TabIndex = 8;
            // 
            // OKButton
            // 
            OKButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            OKButton.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            OKButton.DialogResult = DialogResult.Cancel;
            OKButton.Location = new Point(297, 107);
            OKButton.MinimumSize = new Size(75, 23);
            OKButton.TabIndex = 0;
            OKButton.UseVisualStyleBackColor = true;
            // 
            // GameVersionsForm
            // 
            AutoScaleDimensions = new SizeF(6F, 13F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = OKButton;
            ClientSize = new Size(380, 137);
            Controls.Add(OKButton);
            Controls.Add(SS2VersionTextBox);
            Controls.Add(SS2VersionLabel);
            Controls.Add(T3VersionTextBox);
            Controls.Add(T3VersionLabel);
            Controls.Add(T2VersionTextBox);
            Controls.Add(T2VersionLabel);
            Controls.Add(T1VersionTextBox);
            Controls.Add(T1VersionLabel);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Icon = Resources.AngelLoader;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            ResumeLayout(false);
            PerformLayout();
        }
    }
}
