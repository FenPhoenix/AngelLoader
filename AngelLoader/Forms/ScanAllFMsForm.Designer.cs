#define FenGen_DesignerSource

namespace AngelLoader.Forms;

sealed partial class ScanAllFMsForm
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
        this.TitleCheckBox = new AngelLoader.Forms.CustomControls.DarkCheckBox();
        this.AuthorCheckBox = new AngelLoader.Forms.CustomControls.DarkCheckBox();
        this.GameCheckBox = new AngelLoader.Forms.CustomControls.DarkCheckBox();
        this.CustomResourcesCheckBox = new AngelLoader.Forms.CustomControls.DarkCheckBox();
        this.SizeCheckBox = new AngelLoader.Forms.CustomControls.DarkCheckBox();
        this.ReleaseDateCheckBox = new AngelLoader.Forms.CustomControls.DarkCheckBox();
        this.TagsCheckBox = new AngelLoader.Forms.CustomControls.DarkCheckBox();
        this.SelectAllButton = new AngelLoader.Forms.CustomControls.DarkButton();
        this.SelectNoneButton = new AngelLoader.Forms.CustomControls.DarkButton();
        this.ScanButton = new AngelLoader.Forms.CustomControls.StandardButton();
        this.Cancel_Button = new AngelLoader.Forms.CustomControls.StandardButton();
        this.ScanAllFMsForLabel = new AngelLoader.Forms.CustomControls.DarkLabel();
        this.BottomFLP = new System.Windows.Forms.FlowLayoutPanel();
        this.SelectButtonsFLP = new System.Windows.Forms.FlowLayoutPanel();
        this.MissionCountCheckBox = new AngelLoader.Forms.CustomControls.DarkCheckBox();
        this.BottomFLP.SuspendLayout();
        this.SelectButtonsFLP.SuspendLayout();
        this.SuspendLayout();
        // 
        // TitleCheckBox
        // 
        this.TitleCheckBox.AutoSize = true;
        this.TitleCheckBox.Checked = true;
        this.TitleCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
        this.TitleCheckBox.Location = new System.Drawing.Point(16, 40);
        this.TitleCheckBox.Name = "TitleCheckBox";
        this.TitleCheckBox.Size = new System.Drawing.Size(46, 17);
        this.TitleCheckBox.TabIndex = 2;
        this.TitleCheckBox.Text = "Title";
        // 
        // AuthorCheckBox
        // 
        this.AuthorCheckBox.AutoSize = true;
        this.AuthorCheckBox.Checked = true;
        this.AuthorCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
        this.AuthorCheckBox.Location = new System.Drawing.Point(16, 56);
        this.AuthorCheckBox.Name = "AuthorCheckBox";
        this.AuthorCheckBox.Size = new System.Drawing.Size(57, 17);
        this.AuthorCheckBox.TabIndex = 3;
        this.AuthorCheckBox.Text = "Author";
        // 
        // GameCheckBox
        // 
        this.GameCheckBox.AutoSize = true;
        this.GameCheckBox.Checked = true;
        this.GameCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
        this.GameCheckBox.Location = new System.Drawing.Point(16, 72);
        this.GameCheckBox.Name = "GameCheckBox";
        this.GameCheckBox.Size = new System.Drawing.Size(54, 17);
        this.GameCheckBox.TabIndex = 4;
        this.GameCheckBox.Text = "Game";
        // 
        // CustomResourcesCheckBox
        // 
        this.CustomResourcesCheckBox.AutoSize = true;
        this.CustomResourcesCheckBox.Checked = true;
        this.CustomResourcesCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
        this.CustomResourcesCheckBox.Location = new System.Drawing.Point(16, 88);
        this.CustomResourcesCheckBox.Name = "CustomResourcesCheckBox";
        this.CustomResourcesCheckBox.Size = new System.Drawing.Size(110, 17);
        this.CustomResourcesCheckBox.TabIndex = 5;
        this.CustomResourcesCheckBox.Text = "Custom resources";
        // 
        // SizeCheckBox
        // 
        this.SizeCheckBox.AutoSize = true;
        this.SizeCheckBox.Checked = true;
        this.SizeCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
        this.SizeCheckBox.Location = new System.Drawing.Point(16, 104);
        this.SizeCheckBox.Name = "SizeCheckBox";
        this.SizeCheckBox.Size = new System.Drawing.Size(46, 17);
        this.SizeCheckBox.TabIndex = 6;
        this.SizeCheckBox.Text = "Size";
        // 
        // ReleaseDateCheckBox
        // 
        this.ReleaseDateCheckBox.AutoSize = true;
        this.ReleaseDateCheckBox.Checked = true;
        this.ReleaseDateCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
        this.ReleaseDateCheckBox.Location = new System.Drawing.Point(16, 120);
        this.ReleaseDateCheckBox.Name = "ReleaseDateCheckBox";
        this.ReleaseDateCheckBox.Size = new System.Drawing.Size(89, 17);
        this.ReleaseDateCheckBox.TabIndex = 7;
        this.ReleaseDateCheckBox.Text = "Release date";
        // 
        // TagsCheckBox
        // 
        this.TagsCheckBox.AutoSize = true;
        this.TagsCheckBox.Checked = true;
        this.TagsCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
        this.TagsCheckBox.Location = new System.Drawing.Point(16, 136);
        this.TagsCheckBox.Name = "TagsCheckBox";
        this.TagsCheckBox.Size = new System.Drawing.Size(50, 17);
        this.TagsCheckBox.TabIndex = 8;
        this.TagsCheckBox.Text = "Tags";
        // 
        // SelectAllButton
        // 
        this.SelectAllButton.AutoSize = true;
        this.SelectAllButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        this.SelectAllButton.Location = new System.Drawing.Point(0, 3);
        this.SelectAllButton.Margin = new System.Windows.Forms.Padding(0, 3, 3, 3);
        this.SelectAllButton.MinimumSize = new System.Drawing.Size(0, 23);
        this.SelectAllButton.Name = "SelectAllButton";
        this.SelectAllButton.Padding = new System.Windows.Forms.Padding(6, 0, 6, 0);
        this.SelectAllButton.Size = new System.Drawing.Size(72, 23);
        this.SelectAllButton.TabIndex = 0;
        this.SelectAllButton.Text = "Select all";
        this.SelectAllButton.Click += new System.EventHandler(this.SelectAllButton_Click);
        // 
        // SelectNoneButton
        // 
        this.SelectNoneButton.AutoSize = true;
        this.SelectNoneButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        this.SelectNoneButton.Location = new System.Drawing.Point(78, 3);
        this.SelectNoneButton.MinimumSize = new System.Drawing.Size(0, 23);
        this.SelectNoneButton.Name = "SelectNoneButton";
        this.SelectNoneButton.Padding = new System.Windows.Forms.Padding(6, 0, 6, 0);
        this.SelectNoneButton.Size = new System.Drawing.Size(86, 23);
        this.SelectNoneButton.TabIndex = 1;
        this.SelectNoneButton.Text = "Select none";
        this.SelectNoneButton.Click += new System.EventHandler(this.SelectNoneButton_Click);
        // 
        // ScanButton
        // 
        this.ScanButton.DialogResult = System.Windows.Forms.DialogResult.OK;
        this.ScanButton.Location = new System.Drawing.Point(251, 8);
        this.ScanButton.Margin = new System.Windows.Forms.Padding(3, 8, 3, 3);
        this.ScanButton.Name = "ScanButton";
        this.ScanButton.TabIndex = 1;
        this.ScanButton.Text = "Scan";
        // 
        // Cancel_Button
        // 
        this.Cancel_Button.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        this.Cancel_Button.Location = new System.Drawing.Point(332, 8);
        this.Cancel_Button.Margin = new System.Windows.Forms.Padding(3, 8, 9, 3);
        this.Cancel_Button.Name = "Cancel_Button";
        this.Cancel_Button.TabIndex = 0;
        this.Cancel_Button.Text = "Cancel";
        // 
        // ScanAllFMsForLabel
        // 
        this.ScanAllFMsForLabel.AutoSize = true;
        this.ScanAllFMsForLabel.Location = new System.Drawing.Point(16, 16);
        this.ScanAllFMsForLabel.Name = "ScanAllFMsForLabel";
        this.ScanAllFMsForLabel.Size = new System.Drawing.Size(86, 13);
        this.ScanAllFMsForLabel.TabIndex = 1;
        this.ScanAllFMsForLabel.Text = "Scan all FMs for:";
        // 
        // BottomFLP
        // 
        this.BottomFLP.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
        this.BottomFLP.Controls.Add(this.Cancel_Button);
        this.BottomFLP.Controls.Add(this.ScanButton);
        this.BottomFLP.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
        this.BottomFLP.Location = new System.Drawing.Point(0, 203);
        this.BottomFLP.Name = "BottomFLP";
        this.BottomFLP.Size = new System.Drawing.Size(416, 40);
        this.BottomFLP.TabIndex = 0;
        // 
        // SelectButtonsFLP
        // 
        this.SelectButtonsFLP.Controls.Add(this.SelectAllButton);
        this.SelectButtonsFLP.Controls.Add(this.SelectNoneButton);
        this.SelectButtonsFLP.Location = new System.Drawing.Point(15, 176);
        this.SelectButtonsFLP.Name = "SelectButtonsFLP";
        this.SelectButtonsFLP.Size = new System.Drawing.Size(401, 28);
        this.SelectButtonsFLP.TabIndex = 10;
        // 
        // MissionCountCheckBox
        // 
        this.MissionCountCheckBox.AutoSize = true;
        this.MissionCountCheckBox.Checked = true;
        this.MissionCountCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
        this.MissionCountCheckBox.Location = new System.Drawing.Point(16, 152);
        this.MissionCountCheckBox.Name = "MissionCountCheckBox";
        this.MissionCountCheckBox.Size = new System.Drawing.Size(91, 17);
        this.MissionCountCheckBox.TabIndex = 9;
        this.MissionCountCheckBox.Text = "Mission count";
        // 
        // ScanAllFMsForm
        // 
        this.AcceptButton = this.ScanButton;
        this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.CancelButton = this.Cancel_Button;
        this.ClientSize = new System.Drawing.Size(416, 243);
        this.Controls.Add(this.SelectButtonsFLP);
        this.Controls.Add(this.BottomFLP);
        this.Controls.Add(this.ScanAllFMsForLabel);
        this.Controls.Add(this.MissionCountCheckBox);
        this.Controls.Add(this.TagsCheckBox);
        this.Controls.Add(this.ReleaseDateCheckBox);
        this.Controls.Add(this.SizeCheckBox);
        this.Controls.Add(this.CustomResourcesCheckBox);
        this.Controls.Add(this.GameCheckBox);
        this.Controls.Add(this.AuthorCheckBox);
        this.Controls.Add(this.TitleCheckBox);
        this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
        this.KeyPreview = true;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.Name = "ScanAllFMsForm";
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
        this.Text = "Scan all FMs";
        this.BottomFLP.ResumeLayout(false);
        this.BottomFLP.PerformLayout();
        this.SelectButtonsFLP.ResumeLayout(false);
        this.SelectButtonsFLP.PerformLayout();
        this.ResumeLayout(false);
        this.PerformLayout();

    }
#endif

    #endregion

    private AngelLoader.Forms.CustomControls.DarkCheckBox TitleCheckBox;
    private AngelLoader.Forms.CustomControls.DarkCheckBox AuthorCheckBox;
    private AngelLoader.Forms.CustomControls.DarkCheckBox GameCheckBox;
    private AngelLoader.Forms.CustomControls.DarkCheckBox CustomResourcesCheckBox;
    private AngelLoader.Forms.CustomControls.DarkCheckBox SizeCheckBox;
    private AngelLoader.Forms.CustomControls.DarkCheckBox ReleaseDateCheckBox;
    private AngelLoader.Forms.CustomControls.DarkCheckBox TagsCheckBox;
    private AngelLoader.Forms.CustomControls.DarkButton SelectAllButton;
    private AngelLoader.Forms.CustomControls.DarkButton SelectNoneButton;
    private AngelLoader.Forms.CustomControls.StandardButton ScanButton;
    private AngelLoader.Forms.CustomControls.StandardButton Cancel_Button;
    private AngelLoader.Forms.CustomControls.DarkLabel ScanAllFMsForLabel;
    private System.Windows.Forms.FlowLayoutPanel BottomFLP;
    private System.Windows.Forms.FlowLayoutPanel SelectButtonsFLP;
    private CustomControls.DarkCheckBox MissionCountCheckBox;
}
