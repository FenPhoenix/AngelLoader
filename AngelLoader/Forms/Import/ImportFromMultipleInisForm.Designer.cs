#define FenGen_DesignerSource

namespace AngelLoader.Forms;

sealed partial class ImportFromMultipleInisForm
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
        this.BottomFLP = new System.Windows.Forms.FlowLayoutPanel();
        this.ImportControls = new AngelLoader.Forms.User_FMSel_NDL_ImportControls();
        this.ImportSizeCheckBox = new AngelLoader.Forms.CustomControls.DarkCheckBox();
        this.ImportFinishedOnCheckBox = new AngelLoader.Forms.CustomControls.DarkCheckBox();
        this.ImportSelectedReadmeCheckBox = new AngelLoader.Forms.CustomControls.DarkCheckBox();
        this.ImportTagsCheckBox = new AngelLoader.Forms.CustomControls.DarkCheckBox();
        this.ImportDisabledModsCheckBox = new AngelLoader.Forms.CustomControls.DarkCheckBox();
        this.ImportRatingCheckBox = new AngelLoader.Forms.CustomControls.DarkCheckBox();
        this.ImportCommentCheckBox = new AngelLoader.Forms.CustomControls.DarkCheckBox();
        this.ImportLastPlayedCheckBox = new AngelLoader.Forms.CustomControls.DarkCheckBox();
        this.ImportReleaseDateCheckBox = new AngelLoader.Forms.CustomControls.DarkCheckBox();
        this.ImportTitleCheckBox = new AngelLoader.Forms.CustomControls.DarkCheckBox();
        this.BottomFLP.SuspendLayout();
        this.SuspendLayout();
        // 
        // OKButton
        // 
        this.OKButton.AutoSize = true;
        this.OKButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        this.OKButton.DialogResult = System.Windows.Forms.DialogResult.OK;
        this.OKButton.Location = new System.Drawing.Point(3, 3);
        this.OKButton.MinimumSize = new System.Drawing.Size(75, 23);
        this.OKButton.Name = "OKButton";
        this.OKButton.Padding = new System.Windows.Forms.Padding(6, 0, 6, 0);
        this.OKButton.Size = new System.Drawing.Size(75, 23);
        this.OKButton.TabIndex = 1;
        this.OKButton.Text = "OK";
        // 
        // Cancel_Button
        // 
        this.Cancel_Button.AutoSize = true;
        this.Cancel_Button.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        this.Cancel_Button.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        this.Cancel_Button.Location = new System.Drawing.Point(84, 3);
        this.Cancel_Button.MinimumSize = new System.Drawing.Size(75, 23);
        this.Cancel_Button.Name = "Cancel_Button";
        this.Cancel_Button.Padding = new System.Windows.Forms.Padding(6, 0, 6, 0);
        this.Cancel_Button.Size = new System.Drawing.Size(75, 23);
        this.Cancel_Button.TabIndex = 0;
        this.Cancel_Button.Text = "Cancel";
        // 
        // BottomFLP
        // 
        this.BottomFLP.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
        this.BottomFLP.AutoSize = true;
        this.BottomFLP.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        this.BottomFLP.Controls.Add(this.Cancel_Button);
        this.BottomFLP.Controls.Add(this.OKButton);
        this.BottomFLP.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
        this.BottomFLP.Location = new System.Drawing.Point(386, 578);
        this.BottomFLP.Name = "BottomFLP";
        this.BottomFLP.Size = new System.Drawing.Size(162, 29);
        this.BottomFLP.TabIndex = 0;
        // 
        // ImportControls
        // 
        this.ImportControls.Location = new System.Drawing.Point(0, 0);
        this.ImportControls.Name = "ImportControls";
        this.ImportControls.Size = new System.Drawing.Size(551, 408);
        this.ImportControls.TabIndex = 1;
        // 
        // ImportSizeCheckBox
        // 
        this.ImportSizeCheckBox.AutoSize = true;
        this.ImportSizeCheckBox.Checked = true;
        this.ImportSizeCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
        this.ImportSizeCheckBox.Location = new System.Drawing.Point(16, 552);
        this.ImportSizeCheckBox.Name = "ImportSizeCheckBox";
        this.ImportSizeCheckBox.Size = new System.Drawing.Size(46, 17);
        this.ImportSizeCheckBox.TabIndex = 9;
        this.ImportSizeCheckBox.Text = "Size";
        // 
        // ImportFinishedOnCheckBox
        // 
        this.ImportFinishedOnCheckBox.AutoSize = true;
        this.ImportFinishedOnCheckBox.Checked = true;
        this.ImportFinishedOnCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
        this.ImportFinishedOnCheckBox.Location = new System.Drawing.Point(16, 536);
        this.ImportFinishedOnCheckBox.Name = "ImportFinishedOnCheckBox";
        this.ImportFinishedOnCheckBox.Size = new System.Drawing.Size(80, 17);
        this.ImportFinishedOnCheckBox.TabIndex = 8;
        this.ImportFinishedOnCheckBox.Text = "Finished on";
        // 
        // ImportSelectedReadmeCheckBox
        // 
        this.ImportSelectedReadmeCheckBox.AutoSize = true;
        this.ImportSelectedReadmeCheckBox.Checked = true;
        this.ImportSelectedReadmeCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
        this.ImportSelectedReadmeCheckBox.Location = new System.Drawing.Point(16, 520);
        this.ImportSelectedReadmeCheckBox.Name = "ImportSelectedReadmeCheckBox";
        this.ImportSelectedReadmeCheckBox.Size = new System.Drawing.Size(106, 17);
        this.ImportSelectedReadmeCheckBox.TabIndex = 7;
        this.ImportSelectedReadmeCheckBox.Text = "Selected readme";
        // 
        // ImportTagsCheckBox
        // 
        this.ImportTagsCheckBox.AutoSize = true;
        this.ImportTagsCheckBox.Checked = true;
        this.ImportTagsCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
        this.ImportTagsCheckBox.Location = new System.Drawing.Point(16, 504);
        this.ImportTagsCheckBox.Name = "ImportTagsCheckBox";
        this.ImportTagsCheckBox.Size = new System.Drawing.Size(50, 17);
        this.ImportTagsCheckBox.TabIndex = 6;
        this.ImportTagsCheckBox.Text = "Tags";
        // 
        // ImportDisabledModsCheckBox
        // 
        this.ImportDisabledModsCheckBox.AutoSize = true;
        this.ImportDisabledModsCheckBox.Checked = true;
        this.ImportDisabledModsCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
        this.ImportDisabledModsCheckBox.Location = new System.Drawing.Point(16, 488);
        this.ImportDisabledModsCheckBox.Name = "ImportDisabledModsCheckBox";
        this.ImportDisabledModsCheckBox.Size = new System.Drawing.Size(95, 17);
        this.ImportDisabledModsCheckBox.TabIndex = 5;
        this.ImportDisabledModsCheckBox.Text = "Disabled mods";
        // 
        // ImportRatingCheckBox
        // 
        this.ImportRatingCheckBox.AutoSize = true;
        this.ImportRatingCheckBox.Checked = true;
        this.ImportRatingCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
        this.ImportRatingCheckBox.Location = new System.Drawing.Point(16, 472);
        this.ImportRatingCheckBox.Name = "ImportRatingCheckBox";
        this.ImportRatingCheckBox.Size = new System.Drawing.Size(57, 17);
        this.ImportRatingCheckBox.TabIndex = 4;
        this.ImportRatingCheckBox.Text = "Rating";
        // 
        // ImportCommentCheckBox
        // 
        this.ImportCommentCheckBox.AutoSize = true;
        this.ImportCommentCheckBox.Checked = true;
        this.ImportCommentCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
        this.ImportCommentCheckBox.Location = new System.Drawing.Point(16, 456);
        this.ImportCommentCheckBox.Name = "ImportCommentCheckBox";
        this.ImportCommentCheckBox.Size = new System.Drawing.Size(70, 17);
        this.ImportCommentCheckBox.TabIndex = 3;
        this.ImportCommentCheckBox.Text = "Comment";
        // 
        // ImportLastPlayedCheckBox
        // 
        this.ImportLastPlayedCheckBox.AutoSize = true;
        this.ImportLastPlayedCheckBox.Checked = true;
        this.ImportLastPlayedCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
        this.ImportLastPlayedCheckBox.Location = new System.Drawing.Point(16, 440);
        this.ImportLastPlayedCheckBox.Name = "ImportLastPlayedCheckBox";
        this.ImportLastPlayedCheckBox.Size = new System.Drawing.Size(80, 17);
        this.ImportLastPlayedCheckBox.TabIndex = 2;
        this.ImportLastPlayedCheckBox.Text = "Last played";
        // 
        // ImportReleaseDateCheckBox
        // 
        this.ImportReleaseDateCheckBox.AutoSize = true;
        this.ImportReleaseDateCheckBox.Checked = true;
        this.ImportReleaseDateCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
        this.ImportReleaseDateCheckBox.Location = new System.Drawing.Point(16, 424);
        this.ImportReleaseDateCheckBox.Name = "ImportReleaseDateCheckBox";
        this.ImportReleaseDateCheckBox.Size = new System.Drawing.Size(89, 17);
        this.ImportReleaseDateCheckBox.TabIndex = 1;
        this.ImportReleaseDateCheckBox.Text = "Release date";
        // 
        // ImportTitleCheckBox
        // 
        this.ImportTitleCheckBox.AutoSize = true;
        this.ImportTitleCheckBox.Checked = true;
        this.ImportTitleCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
        this.ImportTitleCheckBox.Location = new System.Drawing.Point(16, 408);
        this.ImportTitleCheckBox.Name = "ImportTitleCheckBox";
        this.ImportTitleCheckBox.Size = new System.Drawing.Size(46, 17);
        this.ImportTitleCheckBox.TabIndex = 0;
        this.ImportTitleCheckBox.Text = "Title";
        // 
        // ImportFromMultipleInisForm
        // 
        this.AcceptButton = this.OKButton;
        this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.CancelButton = this.Cancel_Button;
        this.ClientSize = new System.Drawing.Size(553, 613);
        this.Controls.Add(this.ImportSizeCheckBox);
        this.Controls.Add(this.ImportFinishedOnCheckBox);
        this.Controls.Add(this.ImportSelectedReadmeCheckBox);
        this.Controls.Add(this.ImportTagsCheckBox);
        this.Controls.Add(this.ImportDisabledModsCheckBox);
        this.Controls.Add(this.ImportRatingCheckBox);
        this.Controls.Add(this.ImportCommentCheckBox);
        this.Controls.Add(this.ImportLastPlayedCheckBox);
        this.Controls.Add(this.ImportReleaseDateCheckBox);
        this.Controls.Add(this.ImportTitleCheckBox);
        this.Controls.Add(this.ImportControls);
        this.Controls.Add(this.BottomFLP);
        this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.Name = "ImportFromMultipleInisForm";
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
        this.Text = "[Import From Multiple]";
        this.BottomFLP.ResumeLayout(false);
        this.BottomFLP.PerformLayout();
        this.ResumeLayout(false);
        this.PerformLayout();

    }
#endif

    #endregion

    private AngelLoader.Forms.CustomControls.DarkButton OKButton;
    private AngelLoader.Forms.CustomControls.DarkButton Cancel_Button;
    private System.Windows.Forms.FlowLayoutPanel BottomFLP;
    private User_FMSel_NDL_ImportControls ImportControls;
    private AngelLoader.Forms.CustomControls.DarkCheckBox ImportSizeCheckBox;
    private AngelLoader.Forms.CustomControls.DarkCheckBox ImportFinishedOnCheckBox;
    private AngelLoader.Forms.CustomControls.DarkCheckBox ImportSelectedReadmeCheckBox;
    private AngelLoader.Forms.CustomControls.DarkCheckBox ImportTagsCheckBox;
    private AngelLoader.Forms.CustomControls.DarkCheckBox ImportDisabledModsCheckBox;
    private AngelLoader.Forms.CustomControls.DarkCheckBox ImportRatingCheckBox;
    private AngelLoader.Forms.CustomControls.DarkCheckBox ImportCommentCheckBox;
    private AngelLoader.Forms.CustomControls.DarkCheckBox ImportLastPlayedCheckBox;
    private AngelLoader.Forms.CustomControls.DarkCheckBox ImportReleaseDateCheckBox;
    private AngelLoader.Forms.CustomControls.DarkCheckBox ImportTitleCheckBox;
}
