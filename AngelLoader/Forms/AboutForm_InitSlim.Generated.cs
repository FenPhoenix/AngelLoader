﻿namespace AngelLoader.Forms;

partial class AboutForm
{
    /// <summary>
    /// Custom generated component initializer with cruft removed.
    /// </summary>
    private void InitSlim()
    {
        LogoPictureBox = new System.Windows.Forms.PictureBox();
        VersionLabel = new CustomControls.DarkLabel();
        GitHubLinkLabel = new CustomControls.DarkLinkLabel();
        LicenseTextBox = new CustomControls.DarkTextBox();
        OKButton = new CustomControls.StandardButton();
        AngelLoaderUsesLabel = new CustomControls.DarkLabel();
        LogoTextPictureBox = new System.Windows.Forms.PictureBox();
        OK_FLP = new System.Windows.Forms.FlowLayoutPanel();
        BuildDateLabel = new CustomControls.DarkLabel();
        ((System.ComponentModel.ISupportInitialize)LogoPictureBox).BeginInit();
        ((System.ComponentModel.ISupportInitialize)LogoTextPictureBox).BeginInit();
        OK_FLP.SuspendLayout();
        SuspendLayout();
        // 
        // LogoPictureBox
        // 
        LogoPictureBox.Location = new System.Drawing.Point(19, 18);
        LogoPictureBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
        LogoPictureBox.Size = new System.Drawing.Size(48, 48);
        LogoPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
        // 
        // VersionLabel
        // 
        VersionLabel.AutoSize = true;
        VersionLabel.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
        VersionLabel.Location = new System.Drawing.Point(411, 30);
        VersionLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
        // 
        // GitHubLinkLabel
        // 
        GitHubLinkLabel.AutoSize = true;
        GitHubLinkLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
        GitHubLinkLabel.Location = new System.Drawing.Point(84, 78);
        GitHubLinkLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
        GitHubLinkLabel.TabIndex = 3;
        GitHubLinkLabel.TabStop = true;
        GitHubLinkLabel.LinkClicked += LinkLabels_LinkClicked;
        // 
        // LicenseTextBox
        // 
        LicenseTextBox.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
        LicenseTextBox.BackColor = System.Drawing.SystemColors.Window;
        LicenseTextBox.DarkModeReadOnlyColorsAreDefault = true;
        LicenseTextBox.Location = new System.Drawing.Point(37, 125);
        LicenseTextBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
        LicenseTextBox.Multiline = true;
        LicenseTextBox.ReadOnly = true;
        LicenseTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
        LicenseTextBox.Size = new System.Drawing.Size(541, 276);
        LicenseTextBox.TabIndex = 4;
        // 
        // OKButton
        // 
        OKButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        OKButton.Margin = new System.Windows.Forms.Padding(4, 9, 10, 3);
        OKButton.TabIndex = 0;
        // 
        // AngelLoaderUsesLabel
        // 
        AngelLoaderUsesLabel.AutoSize = true;
        AngelLoaderUsesLabel.Location = new System.Drawing.Point(37, 420);
        AngelLoaderUsesLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
        // 
        // LogoTextPictureBox
        // 
        LogoTextPictureBox.Location = new System.Drawing.Point(75, 18);
        LogoTextPictureBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
        LogoTextPictureBox.Size = new System.Drawing.Size(338, 58);
        // 
        // OK_FLP
        // 
        OK_FLP.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
        OK_FLP.Controls.Add(OKButton);
        OK_FLP.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
        OK_FLP.Location = new System.Drawing.Point(0, 603);
        OK_FLP.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
        OK_FLP.Size = new System.Drawing.Size(617, 46);
        OK_FLP.TabIndex = 0;
        // 
        // BuildDateLabel
        // 
        BuildDateLabel.AutoSize = true;
        BuildDateLabel.Location = new System.Drawing.Point(411, 58);
        BuildDateLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
        // 
        // AboutForm
        // 
        AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
        AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        BackColor = System.Drawing.SystemColors.Window;
        CancelButton = OKButton;
        ClientSize = new System.Drawing.Size(617, 650);
        Controls.Add(BuildDateLabel);
        Controls.Add(VersionLabel);
        Controls.Add(OK_FLP);
        Controls.Add(LogoTextPictureBox);
        Controls.Add(AngelLoaderUsesLabel);
        Controls.Add(LicenseTextBox);
        Controls.Add(GitHubLinkLabel);
        Controls.Add(LogoPictureBox);
        FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
        Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
        MaximizeBox = false;
        MinimizeBox = false;
        Name = "AboutForm";
        StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
        Text = "About AngelLoader";
        ((System.ComponentModel.ISupportInitialize)LogoPictureBox).EndInit();
        ((System.ComponentModel.ISupportInitialize)LogoTextPictureBox).EndInit();
        OK_FLP.ResumeLayout(false);
        OK_FLP.PerformLayout();
        ResumeLayout(false);
        PerformLayout();
    }
}
