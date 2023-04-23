namespace AngelLoader.Forms;

sealed partial class FilterRatingForm
{
    /// <summary>
    /// Custom generated component initializer with cruft removed.
    /// </summary>
    private void InitSlim()
    {
        this.FromLabel = new AngelLoader.Forms.CustomControls.DarkLabel();
        this.ToLabel = new AngelLoader.Forms.CustomControls.DarkLabel();
        this.FromComboBox = new AngelLoader.Forms.CustomControls.DarkComboBox();
        this.ToComboBox = new AngelLoader.Forms.CustomControls.DarkComboBox();
        this.OKButton = new AngelLoader.Forms.CustomControls.DarkButton();
        this.Cancel_Button = new AngelLoader.Forms.CustomControls.DarkButton();
        this.ResetButton = new AngelLoader.Forms.CustomControls.DarkButton();
        this.BottomFLP = new System.Windows.Forms.FlowLayoutPanel();
        this.BottomFLP.SuspendLayout();
        this.SuspendLayout();
        // 
        // FromLabel
        // 
        this.FromLabel.AutoSize = true;
        this.FromLabel.Location = new System.Drawing.Point(8, 8);
        // 
        // ToLabel
        // 
        this.ToLabel.AutoSize = true;
        this.ToLabel.Location = new System.Drawing.Point(8, 48);
        // 
        // FromComboBox
        // 
        this.FromComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
        | System.Windows.Forms.AnchorStyles.Right)));
        this.FromComboBox.FormattingEnabled = true;
        this.FromComboBox.Location = new System.Drawing.Point(8, 24);
        this.FromComboBox.Size = new System.Drawing.Size(154, 21);
        this.FromComboBox.TabIndex = 2;
        this.FromComboBox.SelectedIndexChanged += new System.EventHandler(this.ComboBoxes_SelectedIndexChanged);
        // 
        // ToComboBox
        // 
        this.ToComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
        | System.Windows.Forms.AnchorStyles.Right)));
        this.ToComboBox.FormattingEnabled = true;
        this.ToComboBox.Location = new System.Drawing.Point(8, 64);
        this.ToComboBox.Size = new System.Drawing.Size(154, 21);
        this.ToComboBox.TabIndex = 4;
        this.ToComboBox.SelectedIndexChanged += new System.EventHandler(this.ComboBoxes_SelectedIndexChanged);
        // 
        // OKButton
        // 
        this.OKButton.AutoSize = true;
        this.OKButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        this.OKButton.DialogResult = System.Windows.Forms.DialogResult.OK;
        this.OKButton.MinimumSize = new System.Drawing.Size(75, 23);
        this.OKButton.Padding = new System.Windows.Forms.Padding(6, 0, 6, 0);
        this.OKButton.TabIndex = 1;
        // 
        // Cancel_Button
        // 
        this.Cancel_Button.AutoSize = true;
        this.Cancel_Button.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        this.Cancel_Button.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        this.Cancel_Button.MinimumSize = new System.Drawing.Size(75, 23);
        this.Cancel_Button.Padding = new System.Windows.Forms.Padding(6, 0, 6, 0);
        this.Cancel_Button.TabIndex = 0;
        // 
        // ResetButton
        // 
        this.ResetButton.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
        | System.Windows.Forms.AnchorStyles.Right)));
        this.ResetButton.Location = new System.Drawing.Point(7, 88);
        this.ResetButton.Padding = new System.Windows.Forms.Padding(6, 0, 6, 0);
        this.ResetButton.Size = new System.Drawing.Size(156, 22);
        this.ResetButton.TabIndex = 5;
        this.ResetButton.Click += new System.EventHandler(this.ResetButton_Click);
        // 
        // BottomFLP
        // 
        this.BottomFLP.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
        | System.Windows.Forms.AnchorStyles.Right)));
        this.BottomFLP.Controls.Add(this.Cancel_Button);
        this.BottomFLP.Controls.Add(this.OKButton);
        this.BottomFLP.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
        this.BottomFLP.Location = new System.Drawing.Point(0, 125);
        this.BottomFLP.Padding = new System.Windows.Forms.Padding(0, 0, 4, 0);
        this.BottomFLP.Size = new System.Drawing.Size(170, 33);
        this.BottomFLP.TabIndex = 0;
        // 
        // FilterRatingForm
        // 
        this.AcceptButton = this.OKButton;
        this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.CancelButton = this.Cancel_Button;
        this.ClientSize = new System.Drawing.Size(170, 158);
        this.Controls.Add(this.BottomFLP);
        this.Controls.Add(this.ResetButton);
        this.Controls.Add(this.ToComboBox);
        this.Controls.Add(this.FromComboBox);
        this.Controls.Add(this.ToLabel);
        this.Controls.Add(this.FromLabel);
        this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
        // Hack to prevent slow first render on some forms if Text is blank
        this.Text = " ";
        this.BottomFLP.ResumeLayout(false);
        this.BottomFLP.PerformLayout();
        this.ResumeLayout(false);
        this.PerformLayout();
    }
}
