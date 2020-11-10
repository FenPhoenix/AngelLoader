using System.Drawing;
using System.Windows.Forms;

namespace AngelLoader.Forms
{
    public sealed partial class ImportFromDarkLoaderForm
    {
        private void InitComponentManual()
        {
            OKButton = new Button();
            Cancel_Button = new Button();
            OKCancelFlowLayoutPanel = new FlowLayoutPanel();
            ImportFinishedOnCheckBox = new CheckBox();
            ImportLastPlayedCheckBox = new CheckBox();
            ImportReleaseDateCheckBox = new CheckBox();
            ImportCommentCheckBox = new CheckBox();
            ImportSizeCheckBox = new CheckBox();
            ImportTitleCheckBox = new CheckBox();
            ImportFMDataCheckBox = new CheckBox();
            ImportSavesCheckBox = new CheckBox();
            ImportControls = new User_DL_ImportControls();
            OKCancelFlowLayoutPanel.SuspendLayout();
            SuspendLayout();
            // 
            // OKButton
            // 
            OKButton.AutoSize = true;
            OKButton.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            OKButton.MinimumSize = new Size(75, 23);
            OKButton.DialogResult = DialogResult.OK;
            OKButton.Margin = new Padding(3, 8, 3, 3);
            OKButton.Padding = new Padding(6, 0, 6, 0);
            OKButton.TabIndex = 1;
            OKButton.UseVisualStyleBackColor = true;
            // 
            // Cancel_Button
            // 
            Cancel_Button.AutoSize = true;
            Cancel_Button.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            Cancel_Button.MinimumSize = new Size(75, 23);
            Cancel_Button.DialogResult = DialogResult.Cancel;
            Cancel_Button.Margin = new Padding(3, 8, 9, 3);
            Cancel_Button.Padding = new Padding(6, 0, 6, 0);
            Cancel_Button.TabIndex = 0;
            Cancel_Button.UseVisualStyleBackColor = true;
            // 
            // OKCancelFlowLayoutPanel
            // 
            OKCancelFlowLayoutPanel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            OKCancelFlowLayoutPanel.Controls.Add(Cancel_Button);
            OKCancelFlowLayoutPanel.Controls.Add(OKButton);
            OKCancelFlowLayoutPanel.FlowDirection = FlowDirection.RightToLeft;
            OKCancelFlowLayoutPanel.Location = new Point(0, 245);
            OKCancelFlowLayoutPanel.Size = new Size(547, 40);
            OKCancelFlowLayoutPanel.TabIndex = 9;
            // 
            // ImportFinishedOnCheckBox
            // 
            ImportFinishedOnCheckBox.AutoSize = true;
            ImportFinishedOnCheckBox.Checked = true;
            ImportFinishedOnCheckBox.Location = new Point(32, 200);
            ImportFinishedOnCheckBox.TabIndex = 20;
            ImportFinishedOnCheckBox.UseVisualStyleBackColor = true;
            // 
            // ImportLastPlayedCheckBox
            // 
            ImportLastPlayedCheckBox.AutoSize = true;
            ImportLastPlayedCheckBox.Checked = true;
            ImportLastPlayedCheckBox.Location = new Point(32, 184);
            ImportLastPlayedCheckBox.TabIndex = 21;
            ImportLastPlayedCheckBox.UseVisualStyleBackColor = true;
            // 
            // ImportReleaseDateCheckBox
            // 
            ImportReleaseDateCheckBox.AutoSize = true;
            ImportReleaseDateCheckBox.Checked = true;
            ImportReleaseDateCheckBox.Location = new Point(32, 168);
            ImportReleaseDateCheckBox.TabIndex = 22;
            ImportReleaseDateCheckBox.UseVisualStyleBackColor = true;
            // 
            // ImportCommentCheckBox
            // 
            ImportCommentCheckBox.AutoSize = true;
            ImportCommentCheckBox.Checked = true;
            ImportCommentCheckBox.Location = new Point(32, 152);
            ImportCommentCheckBox.TabIndex = 23;
            ImportCommentCheckBox.UseVisualStyleBackColor = true;
            // 
            // ImportSizeCheckBox
            // 
            ImportSizeCheckBox.AutoSize = true;
            ImportSizeCheckBox.Checked = true;
            ImportSizeCheckBox.Location = new Point(32, 136);
            ImportSizeCheckBox.TabIndex = 24;
            ImportSizeCheckBox.UseVisualStyleBackColor = true;
            // 
            // ImportTitleCheckBox
            // 
            ImportTitleCheckBox.AutoSize = true;
            ImportTitleCheckBox.Checked = true;
            ImportTitleCheckBox.Location = new Point(32, 120);
            ImportTitleCheckBox.TabIndex = 25;
            ImportTitleCheckBox.UseVisualStyleBackColor = true;
            // 
            // ImportFMDataCheckBox
            // 
            ImportFMDataCheckBox.AutoSize = true;
            ImportFMDataCheckBox.Checked = true;
            ImportFMDataCheckBox.Location = new Point(16, 96);
            ImportFMDataCheckBox.TabIndex = 18;
            ImportFMDataCheckBox.UseVisualStyleBackColor = true;
            ImportFMDataCheckBox.CheckedChanged += ImportFMDataCheckBox_CheckedChanged;
            // 
            // ImportSavesCheckBox
            // 
            ImportSavesCheckBox.AutoSize = true;
            ImportSavesCheckBox.Checked = true;
            ImportSavesCheckBox.Location = new Point(16, 224);
            ImportSavesCheckBox.TabIndex = 19;
            ImportSavesCheckBox.UseVisualStyleBackColor = true;
            // 
            // ImportControls
            // 
            ImportControls.Location = new Point(8, 8);
            ImportControls.Size = new Size(545, 88);
            ImportControls.TabIndex = 10;
            // 
            // ImportFromDarkLoaderForm
            // 
            AcceptButton = OKButton;
            AutoScaleDimensions = new SizeF(6F, 13F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = Cancel_Button;
            ClientSize = new Size(547, 285);
            Controls.Add(ImportFinishedOnCheckBox);
            Controls.Add(ImportLastPlayedCheckBox);
            Controls.Add(ImportReleaseDateCheckBox);
            Controls.Add(ImportCommentCheckBox);
            Controls.Add(ImportSizeCheckBox);
            Controls.Add(ImportTitleCheckBox);
            Controls.Add(ImportFMDataCheckBox);
            Controls.Add(ImportSavesCheckBox);
            Controls.Add(ImportControls);
            Controls.Add(OKCancelFlowLayoutPanel);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Icon = Images.AngelLoader;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            FormClosing += ImportFromDarkLoaderForm_FormClosing;
            Load += ImportFromDarkLoaderForm_Load;
            OKCancelFlowLayoutPanel.ResumeLayout(false);
            OKCancelFlowLayoutPanel.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }
    }
}
