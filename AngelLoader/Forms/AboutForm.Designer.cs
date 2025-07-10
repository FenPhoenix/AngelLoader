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
            this.LogoPictureBox = new System.Windows.Forms.PictureBox();
            this.VersionLabel = new AngelLoader.Forms.CustomControls.DarkLabel();
            this.GitHubLinkLabel = new AngelLoader.Forms.CustomControls.DarkLinkLabel();
            this.LicenseTextBox = new AngelLoader.Forms.CustomControls.DarkTextBox();
            this.OKButton = new AngelLoader.Forms.CustomControls.StandardButton();
            this.AngelLoaderUsesLabel = new AngelLoader.Forms.CustomControls.DarkLabel();
            this.LogoTextPictureBox = new System.Windows.Forms.PictureBox();
            this.OK_FLP = new AngelLoader.Forms.CustomControls.FlowLayoutPanelCustom();
            this.BuildDateLabel = new AngelLoader.Forms.CustomControls.DarkLabel();
            ((System.ComponentModel.ISupportInitialize)(this.LogoPictureBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.LogoTextPictureBox)).BeginInit();
            this.OK_FLP.SuspendLayout();
            this.SuspendLayout();
            // 
            // LogoPictureBox
            // 
            this.LogoPictureBox.Location = new System.Drawing.Point(16, 16);
            this.LogoPictureBox.Name = "LogoPictureBox";
            this.LogoPictureBox.Size = new System.Drawing.Size(48, 48);
            this.LogoPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.LogoPictureBox.TabIndex = 0;
            this.LogoPictureBox.TabStop = false;
            // 
            // VersionLabel
            // 
            this.VersionLabel.AutoSize = true;
            this.VersionLabel.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.VersionLabel.Location = new System.Drawing.Point(352, 26);
            this.VersionLabel.Name = "VersionLabel";
            this.VersionLabel.Size = new System.Drawing.Size(71, 21);
            this.VersionLabel.TabIndex = 1;
            this.VersionLabel.Text = "[version]";
            // 
            // GitHubLinkLabel
            // 
            this.GitHubLinkLabel.AutoSize = true;
            this.GitHubLinkLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.GitHubLinkLabel.Location = new System.Drawing.Point(72, 68);
            this.GitHubLinkLabel.Name = "GitHubLinkLabel";
            this.GitHubLinkLabel.Size = new System.Drawing.Size(268, 16);
            this.GitHubLinkLabel.TabIndex = 3;
            this.GitHubLinkLabel.TabStop = true;
            this.GitHubLinkLabel.Text = "https://github.com/FenPhoenix/AngelLoader";
            this.GitHubLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LinkLabels_LinkClicked);
            // 
            // LicenseTextBox
            // 
            this.LicenseTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.LicenseTextBox.BackColor = System.Drawing.SystemColors.Window;
            this.LicenseTextBox.DarkModeReadOnlyColorsAreDefault = true;
            this.LicenseTextBox.Location = new System.Drawing.Point(32, 108);
            this.LicenseTextBox.Multiline = true;
            this.LicenseTextBox.Name = "LicenseTextBox";
            this.LicenseTextBox.ReadOnly = true;
            this.LicenseTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.LicenseTextBox.Size = new System.Drawing.Size(464, 240);
            this.LicenseTextBox.TabIndex = 4;
            // 
            // OKButton
            // 
            this.OKButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.OKButton.Location = new System.Drawing.Point(445, 8);
            this.OKButton.Margin = new System.Windows.Forms.Padding(3, 8, 9, 3);
            this.OKButton.Name = "OKButton";
            this.OKButton.TabIndex = 0;
            this.OKButton.Text = "OK";
            // 
            // AngelLoaderUsesLabel
            // 
            this.AngelLoaderUsesLabel.AutoSize = true;
            this.AngelLoaderUsesLabel.Location = new System.Drawing.Point(32, 364);
            this.AngelLoaderUsesLabel.Name = "AngelLoaderUsesLabel";
            this.AngelLoaderUsesLabel.Size = new System.Drawing.Size(95, 13);
            this.AngelLoaderUsesLabel.TabIndex = 5;
            this.AngelLoaderUsesLabel.Text = "AngelLoader uses:";
            // 
            // LogoTextPictureBox
            // 
            this.LogoTextPictureBox.Location = new System.Drawing.Point(64, 16);
            this.LogoTextPictureBox.Name = "LogoTextPictureBox";
            this.LogoTextPictureBox.Size = new System.Drawing.Size(290, 50);
            this.LogoTextPictureBox.TabIndex = 7;
            this.LogoTextPictureBox.TabStop = false;
            // 
            // OK_FLP
            // 
            this.OK_FLP.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.OK_FLP.Controls.Add(this.OKButton);
            this.OK_FLP.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.OK_FLP.Location = new System.Drawing.Point(0, 523);
            this.OK_FLP.Name = "OK_FLP";
            this.OK_FLP.Size = new System.Drawing.Size(529, 40);
            this.OK_FLP.TabIndex = 0;
            // 
            // BuildDateLabel
            // 
            this.BuildDateLabel.AutoSize = true;
            this.BuildDateLabel.Location = new System.Drawing.Point(352, 50);
            this.BuildDateLabel.Name = "BuildDateLabel";
            this.BuildDateLabel.Size = new System.Drawing.Size(34, 13);
            this.BuildDateLabel.TabIndex = 2;
            this.BuildDateLabel.Text = "[date]";
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
            this.Name = "AboutForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "About AngelLoader";
            ((System.ComponentModel.ISupportInitialize)(this.LogoPictureBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.LogoTextPictureBox)).EndInit();
            this.OK_FLP.ResumeLayout(false);
            this.OK_FLP.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

    }
#endif

    #endregion

    private System.Windows.Forms.PictureBox LogoPictureBox;
    private System.Windows.Forms.PictureBox LogoTextPictureBox;
    private AngelLoader.Forms.CustomControls.DarkLabel VersionLabel;
    private AngelLoader.Forms.CustomControls.DarkLabel BuildDateLabel;
    private AngelLoader.Forms.CustomControls.DarkTextBox LicenseTextBox;
    private AngelLoader.Forms.CustomControls.FlowLayoutPanelCustom OK_FLP;
    private AngelLoader.Forms.CustomControls.StandardButton OKButton;
    private AngelLoader.Forms.CustomControls.DarkLabel AngelLoaderUsesLabel;
    private AngelLoader.Forms.CustomControls.DarkLinkLabel GitHubLinkLabel;
}
