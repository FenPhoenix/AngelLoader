namespace AngelLoader.Forms;

partial class DarkTaskDialog
{
    /// <summary>
    /// Custom generated component initializer with cruft removed.
    /// </summary>
    private void InitSlim()
    {
        this.IconPictureBox = new System.Windows.Forms.PictureBox();
        this.BottomFLP = new System.Windows.Forms.FlowLayoutPanel();
        this.VerificationCheckBox = new AngelLoader.Forms.CustomControls.DarkCheckBox();
        this.Cancel_Button = new AngelLoader.Forms.CustomControls.DarkButton();
        this.NoButton = new AngelLoader.Forms.CustomControls.DarkButton();
        this.YesButton = new AngelLoader.Forms.CustomControls.DarkButton();
        this.MessageLabel = new AngelLoader.Forms.CustomControls.DarkLabel();
        ((System.ComponentModel.ISupportInitialize)(this.IconPictureBox)).BeginInit();
        this.BottomFLP.SuspendLayout();
        this.SuspendLayout();
        // 
        // IconPictureBox
        // 
        this.IconPictureBox.Location = new System.Drawing.Point(10, 10);
        this.IconPictureBox.Size = new System.Drawing.Size(32, 32);
        this.IconPictureBox.TabIndex = 0;
        this.IconPictureBox.TabStop = false;
        // 
        // BottomFLP
        // 
        this.BottomFLP.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
        | System.Windows.Forms.AnchorStyles.Right)));
        this.BottomFLP.BackColor = System.Drawing.SystemColors.Control;
        this.BottomFLP.Controls.Add(this.Cancel_Button);
        this.BottomFLP.Controls.Add(this.NoButton);
        this.BottomFLP.Controls.Add(this.YesButton);
        this.BottomFLP.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
        this.BottomFLP.Location = new System.Drawing.Point(0, 169);
        this.BottomFLP.Margin = new System.Windows.Forms.Padding(0);
        this.BottomFLP.Padding = new System.Windows.Forms.Padding(0, 0, 7, 0);
        this.BottomFLP.Size = new System.Drawing.Size(532, 42);
        this.BottomFLP.TabIndex = 1;
        // 
        // VerificationCheckBox
        // 
        this.VerificationCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
        this.VerificationCheckBox.AutoSize = true;
        this.VerificationCheckBox.Location = new System.Drawing.Point(13, 184);
        this.VerificationCheckBox.Margin = new System.Windows.Forms.Padding(5, 13, 5, 3);
        this.VerificationCheckBox.Size = new System.Drawing.Size(57, 17);
        this.VerificationCheckBox.TabIndex = 3;
        this.VerificationCheckBox.UseVisualStyleBackColor = true;
        // 
        // Cancel_Button
        // 
        this.Cancel_Button.AutoSize = true;
        this.Cancel_Button.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        this.Cancel_Button.Margin = new System.Windows.Forms.Padding(3, 9, 3, 3);
        this.Cancel_Button.MinimumSize = new System.Drawing.Size(76, 23);
        this.Cancel_Button.Padding = new System.Windows.Forms.Padding(10, 0, 10, 0);
        this.Cancel_Button.TabIndex = 2;
        this.Cancel_Button.UseVisualStyleBackColor = true;
        // 
        // NoButton
        // 
        this.NoButton.AutoSize = true;
        this.NoButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        this.NoButton.Margin = new System.Windows.Forms.Padding(3, 9, 3, 3);
        this.NoButton.MinimumSize = new System.Drawing.Size(76, 23);
        this.NoButton.Padding = new System.Windows.Forms.Padding(10, 0, 10, 0);
        this.NoButton.TabIndex = 1;
        this.NoButton.UseVisualStyleBackColor = true;
        // 
        // YesButton
        // 
        this.YesButton.AutoSize = true;
        this.YesButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        this.YesButton.Margin = new System.Windows.Forms.Padding(3, 9, 3, 3);
        this.YesButton.MinimumSize = new System.Drawing.Size(76, 23);
        this.YesButton.Padding = new System.Windows.Forms.Padding(10, 0, 10, 0);
        this.YesButton.TabIndex = 0;
        this.YesButton.UseVisualStyleBackColor = true;
        // 
        // MessageLabel
        // 
        this.MessageLabel.AutoSize = true;
        this.MessageLabel.Location = new System.Drawing.Point(52, 15);
        // 
        // DarkTaskDialog
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.BackColor = System.Drawing.SystemColors.Window;
        this.ClientSize = new System.Drawing.Size(532, 211);
        this.Controls.Add(this.VerificationCheckBox);
        this.Controls.Add(this.BottomFLP);
        this.Controls.Add(this.MessageLabel);
        this.Controls.Add(this.IconPictureBox);
        this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.ShowIcon = false;
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
        // Hack to prevent slow first render on some forms if Text is blank
        this.Text = " ";
        ((System.ComponentModel.ISupportInitialize)(this.IconPictureBox)).EndInit();
        this.BottomFLP.ResumeLayout(false);
        this.BottomFLP.PerformLayout();
        this.ResumeLayout(false);
        this.PerformLayout();
    }
}
