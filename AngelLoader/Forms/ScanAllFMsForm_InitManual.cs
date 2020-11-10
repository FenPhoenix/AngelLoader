using System.Drawing;
using System.Windows.Forms;

namespace AngelLoader.Forms
{
    public sealed partial class ScanAllFMsForm
    {
        private void InitComponentManual()
        {
            TitleCheckBox = new CheckBox();
            AuthorCheckBox = new CheckBox();
            GameCheckBox = new CheckBox();
            CustomResourcesCheckBox = new CheckBox();
            SizeCheckBox = new CheckBox();
            ReleaseDateCheckBox = new CheckBox();
            TagsCheckBox = new CheckBox();
            SelectAllButton = new Button();
            SelectNoneButton = new Button();
            ScanButton = new Button();
            Cancel_Button = new Button();
            ScanAllFMsForLabel = new Label();
            OKCancelButtonsFLP = new FlowLayoutPanel();
            SelectButtonsFLP = new FlowLayoutPanel();
            OKCancelButtonsFLP.SuspendLayout();
            SelectButtonsFLP.SuspendLayout();
            SuspendLayout();
            // 
            // TitleCheckBox
            // 
            TitleCheckBox.AutoSize = true;
            TitleCheckBox.Checked = true;
            TitleCheckBox.Location = new Point(16, 40);
            TitleCheckBox.TabIndex = 2;
            TitleCheckBox.UseVisualStyleBackColor = true;
            // 
            // AuthorCheckBox
            // 
            AuthorCheckBox.AutoSize = true;
            AuthorCheckBox.Checked = true;
            AuthorCheckBox.Location = new Point(16, 56);
            AuthorCheckBox.TabIndex = 3;
            AuthorCheckBox.UseVisualStyleBackColor = true;
            // 
            // GameCheckBox
            // 
            GameCheckBox.AutoSize = true;
            GameCheckBox.Checked = true;
            GameCheckBox.Location = new Point(16, 72);
            GameCheckBox.TabIndex = 4;
            GameCheckBox.UseVisualStyleBackColor = true;
            // 
            // CustomResourcesCheckBox
            // 
            CustomResourcesCheckBox.AutoSize = true;
            CustomResourcesCheckBox.Checked = true;
            CustomResourcesCheckBox.Location = new Point(16, 88);
            CustomResourcesCheckBox.TabIndex = 5;
            CustomResourcesCheckBox.UseVisualStyleBackColor = true;
            // 
            // SizeCheckBox
            // 
            SizeCheckBox.AutoSize = true;
            SizeCheckBox.Checked = true;
            SizeCheckBox.Location = new Point(16, 104);
            SizeCheckBox.TabIndex = 6;
            SizeCheckBox.UseVisualStyleBackColor = true;
            // 
            // ReleaseDateCheckBox
            // 
            ReleaseDateCheckBox.AutoSize = true;
            ReleaseDateCheckBox.Checked = true;
            ReleaseDateCheckBox.Location = new Point(16, 120);
            ReleaseDateCheckBox.TabIndex = 7;
            ReleaseDateCheckBox.UseVisualStyleBackColor = true;
            // 
            // TagsCheckBox
            // 
            TagsCheckBox.AutoSize = true;
            TagsCheckBox.Checked = true;
            TagsCheckBox.Location = new Point(16, 136);
            TagsCheckBox.TabIndex = 8;
            TagsCheckBox.UseVisualStyleBackColor = true;
            // 
            // SelectAllButton
            // 
            SelectAllButton.AutoSize = true;
            SelectAllButton.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            SelectAllButton.MinimumSize = new Size(0, 23);
            SelectAllButton.Margin = new Padding(0, 3, 3, 3);
            SelectAllButton.Padding = new Padding(6, 0, 6, 0);
            SelectAllButton.TabIndex = 0;
            SelectAllButton.UseVisualStyleBackColor = true;
            SelectAllButton.Click += SelectAllButton_Click;
            // 
            // SelectNoneButton
            // 
            SelectNoneButton.AutoSize = true;
            SelectNoneButton.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            SelectNoneButton.MinimumSize = new Size(0, 23);
            SelectNoneButton.Padding = new Padding(6, 0, 6, 0);
            SelectNoneButton.TabIndex = 1;
            SelectNoneButton.UseVisualStyleBackColor = true;
            SelectNoneButton.Click += SelectNoneButton_Click;
            // 
            // ScanButton
            // 
            ScanButton.AutoSize = true;
            ScanButton.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            ScanButton.MinimumSize = new Size(75, 23);
            ScanButton.DialogResult = DialogResult.OK;
            ScanButton.Margin = new Padding(3, 8, 3, 3);
            ScanButton.Padding = new Padding(6, 0, 6, 0);
            ScanButton.TabIndex = 1;
            ScanButton.UseVisualStyleBackColor = true;
            // 
            // Cancel_Button
            // 
            Cancel_Button.AutoSize = true;
            Cancel_Button.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            Cancel_Button.MinimumSize = new Size(75, 23);
            Cancel_Button.DialogResult = DialogResult.Cancel;
            this.Cancel_Button.Margin = new Padding(3, 8, 9, 3);
            Cancel_Button.Padding = new Padding(6, 0, 6, 0);
            Cancel_Button.TabIndex = 0;
            Cancel_Button.UseVisualStyleBackColor = true;
            // 
            // ScanAllFMsForLabel
            // 
            ScanAllFMsForLabel.AutoSize = true;
            ScanAllFMsForLabel.Location = new Point(16, 16);
            ScanAllFMsForLabel.TabIndex = 1;
            // 
            // OKCancelButtonsFLP
            // 
            OKCancelButtonsFLP.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            OKCancelButtonsFLP.Controls.Add(Cancel_Button);
            OKCancelButtonsFLP.Controls.Add(ScanButton);
            OKCancelButtonsFLP.FlowDirection = FlowDirection.RightToLeft;
            OKCancelButtonsFLP.Location = new Point(0, 179);
            OKCancelButtonsFLP.Size = new Size(416, 40);
            OKCancelButtonsFLP.TabIndex = 0;
            // 
            // SelectButtonsFLP
            // 
            SelectButtonsFLP.Controls.Add(SelectAllButton);
            SelectButtonsFLP.Controls.Add(SelectNoneButton);
            SelectButtonsFLP.Location = new Point(15, 152);
            SelectButtonsFLP.Size = new Size(401, 28);
            SelectButtonsFLP.TabIndex = 9;
            // 
            // ScanAllFMsForm
            // 
            AcceptButton = ScanButton;
            AutoScaleDimensions = new SizeF(6F, 13F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = Cancel_Button;
            ClientSize = new Size(416, 219);
            Controls.Add(SelectButtonsFLP);
            Controls.Add(OKCancelButtonsFLP);
            Controls.Add(ScanAllFMsForLabel);
            Controls.Add(TagsCheckBox);
            Controls.Add(ReleaseDateCheckBox);
            Controls.Add(SizeCheckBox);
            Controls.Add(CustomResourcesCheckBox);
            Controls.Add(GameCheckBox);
            Controls.Add(AuthorCheckBox);
            Controls.Add(TitleCheckBox);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Icon = Images.AngelLoader;
            KeyPreview = true;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            FormClosing += ScanAllFMs_FormClosing;
            KeyDown += ScanAllFMsForm_KeyDown;
            OKCancelButtonsFLP.ResumeLayout(false);
            OKCancelButtonsFLP.PerformLayout();
            SelectButtonsFLP.ResumeLayout(false);
            SelectButtonsFLP.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }
    }
}
