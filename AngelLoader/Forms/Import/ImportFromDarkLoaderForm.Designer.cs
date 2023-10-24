#define FenGen_DesignerSource

namespace AngelLoader.Forms;

sealed partial class ImportFromDarkLoaderForm
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
            this.OKButton = new AngelLoader.Forms.CustomControls.StandardButton();
            this.Cancel_Button = new AngelLoader.Forms.CustomControls.StandardButton();
            this.BottomFLP = new System.Windows.Forms.FlowLayoutPanel();
            this.ImportFinishedOnCheckBox = new AngelLoader.Forms.CustomControls.DarkCheckBox();
            this.ImportLastPlayedCheckBox = new AngelLoader.Forms.CustomControls.DarkCheckBox();
            this.ImportReleaseDateCheckBox = new AngelLoader.Forms.CustomControls.DarkCheckBox();
            this.ImportCommentCheckBox = new AngelLoader.Forms.CustomControls.DarkCheckBox();
            this.ImportSizeCheckBox = new AngelLoader.Forms.CustomControls.DarkCheckBox();
            this.ImportTitleCheckBox = new AngelLoader.Forms.CustomControls.DarkCheckBox();
            this.ImportFMDataCheckBox = new AngelLoader.Forms.CustomControls.DarkCheckBox();
            this.ImportSavesCheckBox = new AngelLoader.Forms.CustomControls.DarkCheckBox();
            this.ImportControls = new AngelLoader.Forms.User_DL_ImportControls();
            this.BackupPathRequiredLabel = new AngelLoader.Forms.CustomControls.DarkLabel();
            this.SetBackupPathLinkLabel = new AngelLoader.Forms.CustomControls.DarkLinkLabel();
            this.BottomFLP.SuspendLayout();
            this.SuspendLayout();
            // 
            // OKButton
            // 
            this.OKButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.OKButton.Location = new System.Drawing.Point(382, 8);
            this.OKButton.Margin = new System.Windows.Forms.Padding(3, 8, 3, 3);
            this.OKButton.Name = "OKButton";
            this.OKButton.TabIndex = 1;
            this.OKButton.Text = "OK";
            // 
            // Cancel_Button
            // 
            this.Cancel_Button.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Cancel_Button.Location = new System.Drawing.Point(463, 8);
            this.Cancel_Button.Margin = new System.Windows.Forms.Padding(3, 8, 9, 3);
            this.Cancel_Button.Name = "Cancel_Button";
            this.Cancel_Button.TabIndex = 0;
            this.Cancel_Button.Text = "Cancel";
            // 
            // BottomFLP
            // 
            this.BottomFLP.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.BottomFLP.Controls.Add(this.Cancel_Button);
            this.BottomFLP.Controls.Add(this.OKButton);
            this.BottomFLP.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.BottomFLP.Location = new System.Drawing.Point(0, 285);
            this.BottomFLP.Name = "BottomFLP";
            this.BottomFLP.Size = new System.Drawing.Size(547, 40);
            this.BottomFLP.TabIndex = 0;
            // 
            // ImportFinishedOnCheckBox
            // 
            this.ImportFinishedOnCheckBox.AutoSize = true;
            this.ImportFinishedOnCheckBox.Checked = true;
            this.ImportFinishedOnCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ImportFinishedOnCheckBox.Location = new System.Drawing.Point(32, 200);
            this.ImportFinishedOnCheckBox.Name = "ImportFinishedOnCheckBox";
            this.ImportFinishedOnCheckBox.Size = new System.Drawing.Size(80, 17);
            this.ImportFinishedOnCheckBox.TabIndex = 8;
            this.ImportFinishedOnCheckBox.Text = "Finished on";
            // 
            // ImportLastPlayedCheckBox
            // 
            this.ImportLastPlayedCheckBox.AutoSize = true;
            this.ImportLastPlayedCheckBox.Checked = true;
            this.ImportLastPlayedCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ImportLastPlayedCheckBox.Location = new System.Drawing.Point(32, 184);
            this.ImportLastPlayedCheckBox.Name = "ImportLastPlayedCheckBox";
            this.ImportLastPlayedCheckBox.Size = new System.Drawing.Size(80, 17);
            this.ImportLastPlayedCheckBox.TabIndex = 7;
            this.ImportLastPlayedCheckBox.Text = "Last played";
            // 
            // ImportReleaseDateCheckBox
            // 
            this.ImportReleaseDateCheckBox.AutoSize = true;
            this.ImportReleaseDateCheckBox.Checked = true;
            this.ImportReleaseDateCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ImportReleaseDateCheckBox.Location = new System.Drawing.Point(32, 168);
            this.ImportReleaseDateCheckBox.Name = "ImportReleaseDateCheckBox";
            this.ImportReleaseDateCheckBox.Size = new System.Drawing.Size(89, 17);
            this.ImportReleaseDateCheckBox.TabIndex = 6;
            this.ImportReleaseDateCheckBox.Text = "Release date";
            // 
            // ImportCommentCheckBox
            // 
            this.ImportCommentCheckBox.AutoSize = true;
            this.ImportCommentCheckBox.Checked = true;
            this.ImportCommentCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ImportCommentCheckBox.Location = new System.Drawing.Point(32, 152);
            this.ImportCommentCheckBox.Name = "ImportCommentCheckBox";
            this.ImportCommentCheckBox.Size = new System.Drawing.Size(70, 17);
            this.ImportCommentCheckBox.TabIndex = 5;
            this.ImportCommentCheckBox.Text = "Comment";
            // 
            // ImportSizeCheckBox
            // 
            this.ImportSizeCheckBox.AutoSize = true;
            this.ImportSizeCheckBox.Checked = true;
            this.ImportSizeCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ImportSizeCheckBox.Location = new System.Drawing.Point(32, 136);
            this.ImportSizeCheckBox.Name = "ImportSizeCheckBox";
            this.ImportSizeCheckBox.Size = new System.Drawing.Size(46, 17);
            this.ImportSizeCheckBox.TabIndex = 4;
            this.ImportSizeCheckBox.Text = "Size";
            // 
            // ImportTitleCheckBox
            // 
            this.ImportTitleCheckBox.AutoSize = true;
            this.ImportTitleCheckBox.Checked = true;
            this.ImportTitleCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ImportTitleCheckBox.Location = new System.Drawing.Point(32, 120);
            this.ImportTitleCheckBox.Name = "ImportTitleCheckBox";
            this.ImportTitleCheckBox.Size = new System.Drawing.Size(46, 17);
            this.ImportTitleCheckBox.TabIndex = 3;
            this.ImportTitleCheckBox.Text = "Title";
            // 
            // ImportFMDataCheckBox
            // 
            this.ImportFMDataCheckBox.AutoSize = true;
            this.ImportFMDataCheckBox.Checked = true;
            this.ImportFMDataCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ImportFMDataCheckBox.Location = new System.Drawing.Point(16, 96);
            this.ImportFMDataCheckBox.Name = "ImportFMDataCheckBox";
            this.ImportFMDataCheckBox.Size = new System.Drawing.Size(97, 17);
            this.ImportFMDataCheckBox.TabIndex = 2;
            this.ImportFMDataCheckBox.Text = "Import FM data";
            this.ImportFMDataCheckBox.CheckedChanged += new System.EventHandler(this.ImportFMDataCheckBox_CheckedChanged);
            // 
            // ImportSavesCheckBox
            // 
            this.ImportSavesCheckBox.AutoSize = true;
            this.ImportSavesCheckBox.Checked = true;
            this.ImportSavesCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ImportSavesCheckBox.Location = new System.Drawing.Point(16, 224);
            this.ImportSavesCheckBox.Name = "ImportSavesCheckBox";
            this.ImportSavesCheckBox.Size = new System.Drawing.Size(86, 17);
            this.ImportSavesCheckBox.TabIndex = 9;
            this.ImportSavesCheckBox.Text = "Import saves";
            // 
            // ImportControls
            // 
            this.ImportControls.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ImportControls.Location = new System.Drawing.Point(8, 8);
            this.ImportControls.Name = "ImportControls";
            this.ImportControls.Size = new System.Drawing.Size(536, 88);
            this.ImportControls.TabIndex = 1;
            // 
            // BackupPathRequiredLabel
            // 
            this.BackupPathRequiredLabel.AutoSize = true;
            this.BackupPathRequiredLabel.Location = new System.Drawing.Point(32, 244);
            this.BackupPathRequiredLabel.Name = "BackupPathRequiredLabel";
            this.BackupPathRequiredLabel.Size = new System.Drawing.Size(235, 13);
            this.BackupPathRequiredLabel.TabIndex = 0;
            this.BackupPathRequiredLabel.Text = "To import saves, an FM backup path is required.";
            // 
            // SetBackupPathLinkLabel
            // 
            this.SetBackupPathLinkLabel.AutoSize = true;
            this.SetBackupPathLinkLabel.Location = new System.Drawing.Point(32, 260);
            this.SetBackupPathLinkLabel.Name = "SetBackupPathLinkLabel";
            this.SetBackupPathLinkLabel.Size = new System.Drawing.Size(113, 13);
            this.SetBackupPathLinkLabel.TabIndex = 1;
            this.SetBackupPathLinkLabel.TabStop = true;
            this.SetBackupPathLinkLabel.Text = "Set FM backup path...";
            this.SetBackupPathLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.SetBackupPathLinkLabel_LinkClicked);
            // 
            // ImportFromDarkLoaderForm
            // 
            this.AcceptButton = this.OKButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.Cancel_Button;
            this.ClientSize = new System.Drawing.Size(547, 325);
            this.Controls.Add(this.BackupPathRequiredLabel);
            this.Controls.Add(this.SetBackupPathLinkLabel);
            this.Controls.Add(this.ImportFinishedOnCheckBox);
            this.Controls.Add(this.ImportLastPlayedCheckBox);
            this.Controls.Add(this.ImportReleaseDateCheckBox);
            this.Controls.Add(this.ImportCommentCheckBox);
            this.Controls.Add(this.ImportSizeCheckBox);
            this.Controls.Add(this.ImportTitleCheckBox);
            this.Controls.Add(this.ImportFMDataCheckBox);
            this.Controls.Add(this.ImportSavesCheckBox);
            this.Controls.Add(this.ImportControls);
            this.Controls.Add(this.BottomFLP);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ImportFromDarkLoaderForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Import from DarkLoader";
            this.BottomFLP.ResumeLayout(false);
            this.BottomFLP.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

    }
#endif

    #endregion

    private AngelLoader.Forms.CustomControls.StandardButton OKButton;
    private AngelLoader.Forms.CustomControls.StandardButton Cancel_Button;
    private System.Windows.Forms.FlowLayoutPanel BottomFLP;
    private User_DL_ImportControls ImportControls;
    private AngelLoader.Forms.CustomControls.DarkCheckBox ImportFinishedOnCheckBox;
    private AngelLoader.Forms.CustomControls.DarkCheckBox ImportLastPlayedCheckBox;
    private AngelLoader.Forms.CustomControls.DarkCheckBox ImportReleaseDateCheckBox;
    private AngelLoader.Forms.CustomControls.DarkCheckBox ImportCommentCheckBox;
    private AngelLoader.Forms.CustomControls.DarkCheckBox ImportSizeCheckBox;
    private AngelLoader.Forms.CustomControls.DarkCheckBox ImportTitleCheckBox;
    private AngelLoader.Forms.CustomControls.DarkCheckBox ImportFMDataCheckBox;
    private AngelLoader.Forms.CustomControls.DarkCheckBox ImportSavesCheckBox;
    private CustomControls.DarkLabel BackupPathRequiredLabel;
    private CustomControls.DarkLinkLabel SetBackupPathLinkLabel;
}
