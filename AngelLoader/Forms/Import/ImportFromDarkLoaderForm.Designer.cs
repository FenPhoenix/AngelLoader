﻿#define FenGen_DesignerSource

namespace AngelLoader.Forms
{
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
            this.OKButton = new AngelLoader.Forms.CustomControls.DarkButton();
            this.Cancel_Button = new AngelLoader.Forms.CustomControls.DarkButton();
            this.OKCancelFlowLayoutPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.ImportFinishedOnCheckBox = new AngelLoader.Forms.CustomControls.DarkCheckBox();
            this.ImportLastPlayedCheckBox = new AngelLoader.Forms.CustomControls.DarkCheckBox();
            this.ImportReleaseDateCheckBox = new AngelLoader.Forms.CustomControls.DarkCheckBox();
            this.ImportCommentCheckBox = new AngelLoader.Forms.CustomControls.DarkCheckBox();
            this.ImportSizeCheckBox = new AngelLoader.Forms.CustomControls.DarkCheckBox();
            this.ImportTitleCheckBox = new AngelLoader.Forms.CustomControls.DarkCheckBox();
            this.ImportFMDataCheckBox = new AngelLoader.Forms.CustomControls.DarkCheckBox();
            this.ImportSavesCheckBox = new AngelLoader.Forms.CustomControls.DarkCheckBox();
            this.ImportControls = new AngelLoader.Forms.User_DL_ImportControls();
            this.OKCancelFlowLayoutPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // OKButton
            // 
            this.OKButton.AutoSize = true;
            this.OKButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.OKButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.OKButton.Location = new System.Drawing.Point(382, 8);
            this.OKButton.Margin = new System.Windows.Forms.Padding(3, 8, 3, 3);
            this.OKButton.MinimumSize = new System.Drawing.Size(75, 23);
            this.OKButton.Name = "OKButton";
            this.OKButton.Padding = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.OKButton.Size = new System.Drawing.Size(75, 23);
            this.OKButton.TabIndex = 1;
            this.OKButton.Text = "OK";
            this.OKButton.UseVisualStyleBackColor = true;
            // 
            // Cancel_Button
            // 
            this.Cancel_Button.AutoSize = true;
            this.Cancel_Button.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.Cancel_Button.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Cancel_Button.Location = new System.Drawing.Point(463, 8);
            this.Cancel_Button.Margin = new System.Windows.Forms.Padding(3, 8, 9, 3);
            this.Cancel_Button.MinimumSize = new System.Drawing.Size(75, 23);
            this.Cancel_Button.Name = "Cancel_Button";
            this.Cancel_Button.Padding = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.Cancel_Button.Size = new System.Drawing.Size(75, 23);
            this.Cancel_Button.TabIndex = 0;
            this.Cancel_Button.Text = "Cancel";
            this.Cancel_Button.UseVisualStyleBackColor = true;
            // 
            // OKCancelFlowLayoutPanel
            // 
            this.OKCancelFlowLayoutPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.OKCancelFlowLayoutPanel.Controls.Add(this.Cancel_Button);
            this.OKCancelFlowLayoutPanel.Controls.Add(this.OKButton);
            this.OKCancelFlowLayoutPanel.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.OKCancelFlowLayoutPanel.Location = new System.Drawing.Point(0, 245);
            this.OKCancelFlowLayoutPanel.Name = "OKCancelFlowLayoutPanel";
            this.OKCancelFlowLayoutPanel.Size = new System.Drawing.Size(547, 40);
            this.OKCancelFlowLayoutPanel.TabIndex = 0;
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
            this.ImportFinishedOnCheckBox.UseVisualStyleBackColor = true;
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
            this.ImportLastPlayedCheckBox.UseVisualStyleBackColor = true;
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
            this.ImportReleaseDateCheckBox.UseVisualStyleBackColor = true;
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
            this.ImportCommentCheckBox.UseVisualStyleBackColor = true;
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
            this.ImportSizeCheckBox.UseVisualStyleBackColor = true;
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
            this.ImportTitleCheckBox.UseVisualStyleBackColor = true;
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
            this.ImportFMDataCheckBox.UseVisualStyleBackColor = true;
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
            this.ImportSavesCheckBox.UseVisualStyleBackColor = true;
            // 
            // ImportControls
            // 
            this.ImportControls.Location = new System.Drawing.Point(8, 8);
            this.ImportControls.Name = "ImportControls";
            this.ImportControls.Size = new System.Drawing.Size(545, 88);
            this.ImportControls.TabIndex = 1;
            // 
            // ImportFromDarkLoaderForm
            // 
            this.AcceptButton = this.OKButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.Cancel_Button;
            this.ClientSize = new System.Drawing.Size(547, 285);
            this.Controls.Add(this.ImportFinishedOnCheckBox);
            this.Controls.Add(this.ImportLastPlayedCheckBox);
            this.Controls.Add(this.ImportReleaseDateCheckBox);
            this.Controls.Add(this.ImportCommentCheckBox);
            this.Controls.Add(this.ImportSizeCheckBox);
            this.Controls.Add(this.ImportTitleCheckBox);
            this.Controls.Add(this.ImportFMDataCheckBox);
            this.Controls.Add(this.ImportSavesCheckBox);
            this.Controls.Add(this.ImportControls);
            this.Controls.Add(this.OKCancelFlowLayoutPanel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ImportFromDarkLoaderForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Import from DarkLoader";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ImportFromDarkLoaderForm_FormClosing);
            this.Load += new System.EventHandler(this.ImportFromDarkLoaderForm_Load);
            this.OKCancelFlowLayoutPanel.ResumeLayout(false);
            this.OKCancelFlowLayoutPanel.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }
#endif

        #endregion

        private AngelLoader.Forms.CustomControls.DarkButton OKButton;
        private AngelLoader.Forms.CustomControls.DarkButton Cancel_Button;
        private System.Windows.Forms.FlowLayoutPanel OKCancelFlowLayoutPanel;
        private User_DL_ImportControls ImportControls;
        private AngelLoader.Forms.CustomControls.DarkCheckBox ImportFinishedOnCheckBox;
        private AngelLoader.Forms.CustomControls.DarkCheckBox ImportLastPlayedCheckBox;
        private AngelLoader.Forms.CustomControls.DarkCheckBox ImportReleaseDateCheckBox;
        private AngelLoader.Forms.CustomControls.DarkCheckBox ImportCommentCheckBox;
        private AngelLoader.Forms.CustomControls.DarkCheckBox ImportSizeCheckBox;
        private AngelLoader.Forms.CustomControls.DarkCheckBox ImportTitleCheckBox;
        private AngelLoader.Forms.CustomControls.DarkCheckBox ImportFMDataCheckBox;
        private AngelLoader.Forms.CustomControls.DarkCheckBox ImportSavesCheckBox;
    }
}
