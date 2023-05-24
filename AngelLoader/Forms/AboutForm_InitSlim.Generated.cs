namespace AngelLoader.Forms;

partial class AboutForm
{
    /// <summary>
    /// Custom generated component initializer with cruft removed.
    /// </summary>
    private void InitSlim()
    {
        this.LogoPictureBox = new System.Windows.Forms.PictureBox();
        this.VersionLabel = new AngelLoader.Forms.CustomControls.DarkLabel();
        this.GitHubLinkLabel = new AngelLoader.Forms.CustomControls.DarkLinkLabel();
        this.LicenseTextBox = new AngelLoader.Forms.CustomControls.DarkTextBox();
        this.OKButton = new AngelLoader.Forms.CustomControls.StandardButton();
        this.AngelLoaderUsesLabel = new AngelLoader.Forms.CustomControls.DarkLabel();
        this.LogoTextPictureBox = new System.Windows.Forms.PictureBox();
        this.OK_FLP = new System.Windows.Forms.FlowLayoutPanel();
        this.BuildDateLabel = new AngelLoader.Forms.CustomControls.DarkLabel();
        ((System.ComponentModel.ISupportInitialize)(this.LogoPictureBox)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this.LogoTextPictureBox)).BeginInit();
        this.OK_FLP.SuspendLayout();
        this.SuspendLayout();
        // 
        // LogoPictureBox
        // 
        this.LogoPictureBox.Location = new System.Drawing.Point(16, 16);
        this.LogoPictureBox.Size = new System.Drawing.Size(48, 48);
        this.LogoPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
        // 
        // VersionLabel
        // 
        this.VersionLabel.AutoSize = true;
        this.VersionLabel.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        this.VersionLabel.Location = new System.Drawing.Point(352, 26);
        // 
        // GitHubLinkLabel
        // 
        this.GitHubLinkLabel.AutoSize = true;
        this.GitHubLinkLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        this.GitHubLinkLabel.Location = new System.Drawing.Point(72, 68);
        this.GitHubLinkLabel.TabIndex = 3;
        this.GitHubLinkLabel.TabStop = true;
        this.GitHubLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LinkLabels_LinkClicked);
        // 
        // LicenseTextBox
        // 
        this.LicenseTextBox.BackColor = System.Drawing.SystemColors.Window;
        this.LicenseTextBox.DarkModeReadOnlyColorsAreDefault = true;
        this.LicenseTextBox.Location = new System.Drawing.Point(32, 108);
        this.LicenseTextBox.Multiline = true;
        this.LicenseTextBox.ReadOnly = true;
        this.LicenseTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
        this.LicenseTextBox.Size = new System.Drawing.Size(464, 240);
        this.LicenseTextBox.TabIndex = 4;
        // 
        // OKButton
        // 
        this.OKButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        this.OKButton.Margin = new System.Windows.Forms.Padding(3, 8, 9, 3);
        this.OKButton.TabIndex = 0;
        // 
        // AngelLoaderUsesLabel
        // 
        this.AngelLoaderUsesLabel.AutoSize = true;
        this.AngelLoaderUsesLabel.Location = new System.Drawing.Point(32, 364);
        // 
        // LogoTextPictureBox
        // 
        this.LogoTextPictureBox.Location = new System.Drawing.Point(64, 16);
        this.LogoTextPictureBox.Size = new System.Drawing.Size(290, 50);
        // 
        // OK_FLP
        // 
        this.OK_FLP.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
        this.OK_FLP.Controls.Add(this.OKButton);
        this.OK_FLP.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
        this.OK_FLP.Location = new System.Drawing.Point(0, 523);
        this.OK_FLP.Size = new System.Drawing.Size(529, 40);
        this.OK_FLP.TabIndex = 0;
        // 
        // BuildDateLabel
        // 
        this.BuildDateLabel.AutoSize = true;
        this.BuildDateLabel.Location = new System.Drawing.Point(352, 50);
        // 
        // AboutForm
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.BackColor = System.Drawing.SystemColors.Window;
        this.CancelButton = this.OKButton;
        this.ClientSize = new System.Drawing.Size(529, 563);
        this.Controls.Add(this.BuildDateLabel);
        this.Controls.Add(this.VersionLabel);
        this.Controls.Add(this.OK_FLP);
        this.Controls.Add(this.LogoTextPictureBox);
        this.Controls.Add(this.AngelLoaderUsesLabel);
        this.Controls.Add(this.LicenseTextBox);
        this.Controls.Add(this.GitHubLinkLabel);
        this.Controls.Add(this.LogoPictureBox);
        this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
        // Hack to prevent slow first render on some forms if Text is blank
        this.Text = " ";
        ((System.ComponentModel.ISupportInitialize)(this.LogoPictureBox)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(this.LogoTextPictureBox)).EndInit();
        this.OK_FLP.ResumeLayout(false);
        this.OK_FLP.PerformLayout();
        this.ResumeLayout(false);
        this.PerformLayout();
    }
}
