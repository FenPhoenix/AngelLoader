using System.Drawing;
using System.Windows.Forms;

namespace AngelLoader.Forms
{
    public sealed partial class ImportFromMultipleInisForm
    {
        private void InitComponentManual()
        {
            OKButton = new Button();
            Cancel_Button = new Button();
            OKCancelFlowLayoutPanel = new FlowLayoutPanel();
            ImportControls = new User_FMSel_NDL_ImportControls();
            ImportSizeCheckBox = new CheckBox();
            ImportFinishedOnCheckBox = new CheckBox();
            ImportSelectedReadmeCheckBox = new CheckBox();
            ImportTagsCheckBox = new CheckBox();
            ImportDisabledModsCheckBox = new CheckBox();
            ImportRatingCheckBox = new CheckBox();
            ImportCommentCheckBox = new CheckBox();
            ImportLastPlayedCheckBox = new CheckBox();
            ImportReleaseDateCheckBox = new CheckBox();
            ImportTitleCheckBox = new CheckBox();
            OKCancelFlowLayoutPanel.SuspendLayout();
            SuspendLayout();
            // 
            // OKButton
            // 
            OKButton.AutoSize = true;
            OKButton.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            OKButton.MinimumSize = new Size(75, 23);
            OKButton.DialogResult = DialogResult.OK;
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
            Cancel_Button.Padding = new Padding(6, 0, 6, 0);
            Cancel_Button.TabIndex = 0;
            Cancel_Button.UseVisualStyleBackColor = true;
            // 
            // OKCancelFlowLayoutPanel
            // 
            OKCancelFlowLayoutPanel.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            OKCancelFlowLayoutPanel.AutoSize = true;
            OKCancelFlowLayoutPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            OKCancelFlowLayoutPanel.Controls.Add(Cancel_Button);
            OKCancelFlowLayoutPanel.Controls.Add(OKButton);
            OKCancelFlowLayoutPanel.FlowDirection = FlowDirection.RightToLeft;
            OKCancelFlowLayoutPanel.Location = new Point(386, 578);
            OKCancelFlowLayoutPanel.Size = new Size(162, 29);
            OKCancelFlowLayoutPanel.TabIndex = 0;
            // 
            // ImportControls
            // 
            ImportControls.Location = new Point(0, 0);
            ImportControls.Size = new Size(551, 408);
            ImportControls.TabIndex = 1;
            // 
            // ImportSizeCheckBox
            // 
            ImportSizeCheckBox.AutoSize = true;
            ImportSizeCheckBox.Checked = true;
            ImportSizeCheckBox.Location = new Point(16, 552);
            ImportSizeCheckBox.TabIndex = 19;
            ImportSizeCheckBox.UseVisualStyleBackColor = true;
            // 
            // ImportFinishedOnCheckBox
            // 
            ImportFinishedOnCheckBox.AutoSize = true;
            ImportFinishedOnCheckBox.Checked = true;
            ImportFinishedOnCheckBox.Location = new Point(16, 536);
            ImportFinishedOnCheckBox.TabIndex = 20;
            ImportFinishedOnCheckBox.UseVisualStyleBackColor = true;
            // 
            // ImportSelectedReadmeCheckBox
            // 
            ImportSelectedReadmeCheckBox.AutoSize = true;
            ImportSelectedReadmeCheckBox.Checked = true;
            ImportSelectedReadmeCheckBox.Location = new Point(16, 520);
            ImportSelectedReadmeCheckBox.TabIndex = 21;
            ImportSelectedReadmeCheckBox.UseVisualStyleBackColor = true;
            // 
            // ImportTagsCheckBox
            // 
            ImportTagsCheckBox.AutoSize = true;
            ImportTagsCheckBox.Checked = true;
            ImportTagsCheckBox.Location = new Point(16, 504);
            ImportTagsCheckBox.TabIndex = 22;
            ImportTagsCheckBox.UseVisualStyleBackColor = true;
            // 
            // ImportDisabledModsCheckBox
            // 
            ImportDisabledModsCheckBox.AutoSize = true;
            ImportDisabledModsCheckBox.Checked = true;
            ImportDisabledModsCheckBox.Location = new Point(16, 488);
            ImportDisabledModsCheckBox.TabIndex = 23;
            ImportDisabledModsCheckBox.UseVisualStyleBackColor = true;
            // 
            // ImportRatingCheckBox
            // 
            ImportRatingCheckBox.AutoSize = true;
            ImportRatingCheckBox.Checked = true;
            ImportRatingCheckBox.Location = new Point(16, 472);
            ImportRatingCheckBox.TabIndex = 24;
            ImportRatingCheckBox.UseVisualStyleBackColor = true;
            // 
            // ImportCommentCheckBox
            // 
            ImportCommentCheckBox.AutoSize = true;
            ImportCommentCheckBox.Checked = true;
            ImportCommentCheckBox.Location = new Point(16, 456);
            ImportCommentCheckBox.TabIndex = 25;
            ImportCommentCheckBox.UseVisualStyleBackColor = true;
            // 
            // ImportLastPlayedCheckBox
            // 
            ImportLastPlayedCheckBox.AutoSize = true;
            ImportLastPlayedCheckBox.Checked = true;
            ImportLastPlayedCheckBox.Location = new Point(16, 440);
            ImportLastPlayedCheckBox.TabIndex = 26;
            ImportLastPlayedCheckBox.UseVisualStyleBackColor = true;
            // 
            // ImportReleaseDateCheckBox
            // 
            ImportReleaseDateCheckBox.AutoSize = true;
            ImportReleaseDateCheckBox.Checked = true;
            ImportReleaseDateCheckBox.Location = new Point(16, 424);
            ImportReleaseDateCheckBox.TabIndex = 27;
            ImportReleaseDateCheckBox.UseVisualStyleBackColor = true;
            // 
            // ImportTitleCheckBox
            // 
            ImportTitleCheckBox.AutoSize = true;
            ImportTitleCheckBox.Checked = true;
            ImportTitleCheckBox.Location = new Point(16, 408);
            ImportTitleCheckBox.TabIndex = 28;
            ImportTitleCheckBox.UseVisualStyleBackColor = true;
            // 
            // ImportFromMultipleInisForm
            // 
            AcceptButton = OKButton;
            AutoScaleDimensions = new SizeF(6F, 13F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = Cancel_Button;
            ClientSize = new Size(553, 613);
            Controls.Add(ImportSizeCheckBox);
            Controls.Add(ImportFinishedOnCheckBox);
            Controls.Add(ImportSelectedReadmeCheckBox);
            Controls.Add(ImportTagsCheckBox);
            Controls.Add(ImportDisabledModsCheckBox);
            Controls.Add(ImportRatingCheckBox);
            Controls.Add(ImportCommentCheckBox);
            Controls.Add(ImportLastPlayedCheckBox);
            Controls.Add(ImportReleaseDateCheckBox);
            Controls.Add(ImportTitleCheckBox);
            Controls.Add(ImportControls);
            Controls.Add(OKCancelFlowLayoutPanel);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Icon = Images.AngelLoader;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            FormClosing += ImportFromMultipleInisForm_FormClosing;
            OKCancelFlowLayoutPanel.ResumeLayout(false);
            OKCancelFlowLayoutPanel.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }
    }
}
