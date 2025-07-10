#define FenGen_DesignerSource

namespace AngelLoader.Forms;

sealed partial class UpdateForm
{
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    #region Windows Form Designer generated code

#if DEBUG
    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
            this.BottomButtonsFLP = new AngelLoader.Forms.CustomControls.DrawnFlowLayoutPanel();
            this.Cancel_Button = new AngelLoader.Forms.CustomControls.StandardButton();
            this.UpdateButton = new AngelLoader.Forms.CustomControls.StandardButton();
            this.ReleaseNotesRichTextBox = new AngelLoader.Forms.CustomControls.RichTextBoxCustom();
            this.LoadingLabel = new AngelLoader.Forms.CustomControls.DarkLabel();
            this.BottomButtonsFLP.SuspendLayout();
            this.SuspendLayout();
            // 
            // BottomButtonsFLP
            // 
            this.BottomButtonsFLP.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.BottomButtonsFLP.Controls.Add(this.Cancel_Button);
            this.BottomButtonsFLP.Controls.Add(this.UpdateButton);
            this.BottomButtonsFLP.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.BottomButtonsFLP.Location = new System.Drawing.Point(0, 628);
            this.BottomButtonsFLP.Name = "BottomButtonsFLP";
            this.BottomButtonsFLP.Size = new System.Drawing.Size(800, 28);
            this.BottomButtonsFLP.TabIndex = 0;
            // 
            // Cancel_Button
            // 
            this.Cancel_Button.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Cancel_Button.Location = new System.Drawing.Point(722, 2);
            this.Cancel_Button.Margin = new System.Windows.Forms.Padding(3, 2, 3, 3);
            this.Cancel_Button.Name = "Cancel_Button";
            this.Cancel_Button.TabIndex = 0;
            this.Cancel_Button.Text = "Cancel";
            // 
            // UpdateButton
            // 
            this.UpdateButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.UpdateButton.Location = new System.Drawing.Point(599, 2);
            this.UpdateButton.Margin = new System.Windows.Forms.Padding(3, 2, 3, 3);
            this.UpdateButton.Name = "UpdateButton";
            this.UpdateButton.TabIndex = 1;
            this.UpdateButton.Text = "Update and restart";
            // 
            // ReleaseNotesRichTextBox
            // 
            this.ReleaseNotesRichTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ReleaseNotesRichTextBox.BackColor = System.Drawing.SystemColors.Window;
            this.ReleaseNotesRichTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.ReleaseNotesRichTextBox.Location = new System.Drawing.Point(2, 2);
            this.ReleaseNotesRichTextBox.Name = "ReleaseNotesRichTextBox";
            this.ReleaseNotesRichTextBox.ReadOnly = true;
            this.ReleaseNotesRichTextBox.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
            this.ReleaseNotesRichTextBox.Size = new System.Drawing.Size(796, 623);
            this.ReleaseNotesRichTextBox.TabIndex = 1;
            this.ReleaseNotesRichTextBox.Text = "";
            // 
            // LoadingLabel
            // 
            this.LoadingLabel.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.LoadingLabel.AutoSize = true;
            this.LoadingLabel.Location = new System.Drawing.Point(376, 296);
            this.LoadingLabel.Name = "LoadingLabel";
            this.LoadingLabel.Size = new System.Drawing.Size(47, 13);
            this.LoadingLabel.TabIndex = 2;
            this.LoadingLabel.Text = "[loading]";
            // 
            // UpdateForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.Cancel_Button;
            this.ClientSize = new System.Drawing.Size(800, 656);
            this.Controls.Add(this.LoadingLabel);
            this.Controls.Add(this.ReleaseNotesRichTextBox);
            this.Controls.Add(this.BottomButtonsFLP);
            this.KeyPreview = true;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(300, 200);
            this.Name = "UpdateForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Update available";
            this.BottomButtonsFLP.ResumeLayout(false);
            this.BottomButtonsFLP.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

    }
#endif

    #endregion

    private CustomControls.DrawnFlowLayoutPanel BottomButtonsFLP;
    private CustomControls.StandardButton Cancel_Button;
    private CustomControls.StandardButton UpdateButton;
    private CustomControls.RichTextBoxCustom ReleaseNotesRichTextBox;
    private CustomControls.DarkLabel LoadingLabel;
}
