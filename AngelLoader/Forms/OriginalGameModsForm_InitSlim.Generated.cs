namespace AngelLoader.Forms;

partial class OriginalGameModsForm
{
    /// <summary>
    /// Custom generated component initializer with cruft removed.
    /// </summary>
    private void InitSlim()
    {
        this.components = new System.ComponentModel.Container();
        this.OrigGameModsControl = new AngelLoader.Forms.CustomControls.ModsControl();
        this.OKButton = new AngelLoader.Forms.CustomControls.DarkButton();
        this.BottomFLP = new System.Windows.Forms.FlowLayoutPanel();
        this.Cancel_Button = new AngelLoader.Forms.CustomControls.DarkButton();
        this.NewMantleCheckBox = new AngelLoader.Forms.CustomControls.DarkCheckBox();
        this.MainToolTip = new System.Windows.Forms.ToolTip(this.components);
        this.BottomFLP.SuspendLayout();
        this.SuspendLayout();
        // 
        // OrigGameModsControl
        // 
        this.OrigGameModsControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
        | System.Windows.Forms.AnchorStyles.Left)
        | System.Windows.Forms.AnchorStyles.Right)));
        this.OrigGameModsControl.Location = new System.Drawing.Point(0, 64);
        this.OrigGameModsControl.Size = new System.Drawing.Size(532, 404);
        this.OrigGameModsControl.TabIndex = 3;
        // 
        // OKButton
        // 
        this.OKButton.DialogResult = System.Windows.Forms.DialogResult.OK;
        this.OKButton.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
        this.OKButton.MinimumSize = new System.Drawing.Size(75, 23);
        this.OKButton.TabIndex = 0;
        this.OKButton.UseVisualStyleBackColor = true;
        // 
        // BottomFLP
        // 
        this.BottomFLP.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
        | System.Windows.Forms.AnchorStyles.Right)));
        this.BottomFLP.Controls.Add(this.Cancel_Button);
        this.BottomFLP.Controls.Add(this.OKButton);
        this.BottomFLP.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
        this.BottomFLP.Location = new System.Drawing.Point(8, 469);
        this.BottomFLP.Size = new System.Drawing.Size(517, 23);
        this.BottomFLP.TabIndex = 0;
        // 
        // Cancel_Button
        // 
        this.Cancel_Button.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        this.Cancel_Button.Margin = new System.Windows.Forms.Padding(0);
        this.Cancel_Button.MinimumSize = new System.Drawing.Size(75, 23);
        this.Cancel_Button.TabIndex = 1;
        this.Cancel_Button.UseVisualStyleBackColor = true;
        // 
        // NewMantleCheckBox
        // 
        this.NewMantleCheckBox.AutoSize = true;
        this.NewMantleCheckBox.Checked = true;
        this.NewMantleCheckBox.CheckState = System.Windows.Forms.CheckState.Indeterminate;
        this.NewMantleCheckBox.Location = new System.Drawing.Point(8, 16);
        this.NewMantleCheckBox.TabIndex = 1;
        this.NewMantleCheckBox.ThreeState = true;
        this.NewMantleCheckBox.UseVisualStyleBackColor = true;
        // 
        // OriginalGameModsForm
        // 
        this.AcceptButton = this.OKButton;
        this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.CancelButton = this.Cancel_Button;
        this.ClientSize = new System.Drawing.Size(532, 500);
        this.Controls.Add(this.NewMantleCheckBox);
        this.Controls.Add(this.BottomFLP);
        this.Controls.Add(this.OrigGameModsControl);
        this.KeyPreview = true;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.MinimumSize = new System.Drawing.Size(200, 200);
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
        // Hack to prevent slow first render on some forms if Text is blank
        this.Text = " ";
        this.Paint += new System.Windows.Forms.PaintEventHandler(this.OriginalGameModsForm_Paint);
        this.BottomFLP.ResumeLayout(false);
        this.ResumeLayout(false);
        this.PerformLayout();
    }
}
