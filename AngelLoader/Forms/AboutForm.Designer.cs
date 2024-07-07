#define FenGen_DesignerSource

namespace AngelLoader.Forms;

partial class AboutForm
{
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

#if DEBUG
    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
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
        LogoPictureBox.Name = "LogoPictureBox";
        LogoPictureBox.Size = new System.Drawing.Size(48, 48);
        LogoPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
        LogoPictureBox.TabIndex = 0;
        LogoPictureBox.TabStop = false;
        // 
        // VersionLabel
        // 
        VersionLabel.AutoSize = true;
        VersionLabel.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
        VersionLabel.Location = new System.Drawing.Point(411, 30);
        VersionLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
        VersionLabel.Name = "VersionLabel";
        VersionLabel.Size = new System.Drawing.Size(71, 21);
        VersionLabel.TabIndex = 1;
        VersionLabel.Text = "[version]";
        // 
        // GitHubLinkLabel
        // 
        GitHubLinkLabel.AutoSize = true;
        GitHubLinkLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
        GitHubLinkLabel.Location = new System.Drawing.Point(84, 78);
        GitHubLinkLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
        GitHubLinkLabel.Name = "GitHubLinkLabel";
        GitHubLinkLabel.Size = new System.Drawing.Size(268, 16);
        GitHubLinkLabel.TabIndex = 3;
        GitHubLinkLabel.TabStop = true;
        GitHubLinkLabel.Text = "https://github.com/FenPhoenix/AngelLoader";
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
        LicenseTextBox.Name = "LicenseTextBox";
        LicenseTextBox.ReadOnly = true;
        LicenseTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
        LicenseTextBox.Size = new System.Drawing.Size(541, 276);
        LicenseTextBox.TabIndex = 4;
        // 
        // OKButton
        // 
        OKButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        OKButton.Location = new System.Drawing.Point(519, 9);
        OKButton.Margin = new System.Windows.Forms.Padding(4, 9, 10, 3);
        OKButton.Name = "OKButton";
        OKButton.TabIndex = 0;
        OKButton.Text = "OK";
        // 
        // AngelLoaderUsesLabel
        // 
        AngelLoaderUsesLabel.AutoSize = true;
        AngelLoaderUsesLabel.Location = new System.Drawing.Point(37, 420);
        AngelLoaderUsesLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
        AngelLoaderUsesLabel.Name = "AngelLoaderUsesLabel";
        AngelLoaderUsesLabel.Size = new System.Drawing.Size(103, 15);
        AngelLoaderUsesLabel.TabIndex = 5;
        AngelLoaderUsesLabel.Text = "AngelLoader uses:";
        // 
        // LogoTextPictureBox
        // 
        LogoTextPictureBox.Location = new System.Drawing.Point(75, 18);
        LogoTextPictureBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
        LogoTextPictureBox.Name = "LogoTextPictureBox";
        LogoTextPictureBox.Size = new System.Drawing.Size(338, 58);
        LogoTextPictureBox.TabIndex = 7;
        LogoTextPictureBox.TabStop = false;
        // 
        // OK_FLP
        // 
        OK_FLP.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
        OK_FLP.Controls.Add(OKButton);
        OK_FLP.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
        OK_FLP.Location = new System.Drawing.Point(0, 603);
        OK_FLP.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
        OK_FLP.Name = "OK_FLP";
        OK_FLP.Size = new System.Drawing.Size(617, 46);
        OK_FLP.TabIndex = 0;
        // 
        // BuildDateLabel
        // 
        BuildDateLabel.AutoSize = true;
        BuildDateLabel.Location = new System.Drawing.Point(411, 58);
        BuildDateLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
        BuildDateLabel.Name = "BuildDateLabel";
        BuildDateLabel.Size = new System.Drawing.Size(38, 15);
        BuildDateLabel.TabIndex = 2;
        BuildDateLabel.Text = "[date]";
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
#endif

    #endregion

    private System.Windows.Forms.PictureBox LogoPictureBox;
    private System.Windows.Forms.PictureBox LogoTextPictureBox;
    private AngelLoader.Forms.CustomControls.DarkLabel VersionLabel;
    private AngelLoader.Forms.CustomControls.DarkLabel BuildDateLabel;
    private AngelLoader.Forms.CustomControls.DarkTextBox LicenseTextBox;
    private System.Windows.Forms.FlowLayoutPanel OK_FLP;
    private AngelLoader.Forms.CustomControls.StandardButton OKButton;
    private AngelLoader.Forms.CustomControls.DarkLabel AngelLoaderUsesLabel;
    private AngelLoader.Forms.CustomControls.DarkLinkLabel GitHubLinkLabel;
}
