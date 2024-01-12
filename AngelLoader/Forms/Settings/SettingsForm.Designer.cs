#define FenGen_DesignerSource

namespace AngelLoader.Forms;

partial class SettingsForm
{
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    #region Windows Form Designer generated code

#if DEBUG
    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
            this.components = new System.ComponentModel.Container();
            this.BottomFLP = new System.Windows.Forms.FlowLayoutPanel();
            this.Cancel_Button = new AngelLoader.Forms.CustomControls.StandardButton();
            this.OKButton = new AngelLoader.Forms.CustomControls.StandardButton();
            this.ErrorLabel = new AngelLoader.Forms.CustomControls.DarkLabel();
            this.ErrorIconPictureBox = new System.Windows.Forms.PictureBox();
            this.MainToolTip = new CustomControls.ToolTipCustom(this.components);
            this.MainSplitContainer = new AngelLoader.Forms.CustomControls.DarkSplitContainerCustom();
            this.ThiefBuddyRadioButton = new AngelLoader.Forms.CustomControls.DarkRadioButtonCustom();
            this.OtherRadioButton = new AngelLoader.Forms.CustomControls.DarkRadioButtonCustom();
            this.AppearanceRadioButton = new AngelLoader.Forms.CustomControls.DarkRadioButtonCustom();
            this.PathsRadioButton = new AngelLoader.Forms.CustomControls.DarkRadioButtonCustom();
            this.PagePanel = new System.Windows.Forms.Panel();
            this.BottomFLP.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ErrorIconPictureBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.MainSplitContainer)).BeginInit();
            this.MainSplitContainer.Panel1.SuspendLayout();
            this.MainSplitContainer.Panel2.SuspendLayout();
            this.MainSplitContainer.SuspendLayout();
            this.SuspendLayout();
            // 
            // BottomFLP
            // 
            this.BottomFLP.Controls.Add(this.Cancel_Button);
            this.BottomFLP.Controls.Add(this.OKButton);
            this.BottomFLP.Controls.Add(this.ErrorLabel);
            this.BottomFLP.Controls.Add(this.ErrorIconPictureBox);
            this.BottomFLP.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.BottomFLP.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.BottomFLP.Location = new System.Drawing.Point(0, 616);
            this.BottomFLP.Name = "BottomFLP";
            this.BottomFLP.Size = new System.Drawing.Size(694, 40);
            this.BottomFLP.TabIndex = 4;
            // 
            // Cancel_Button
            // 
            this.Cancel_Button.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Cancel_Button.Location = new System.Drawing.Point(610, 8);
            this.Cancel_Button.Margin = new System.Windows.Forms.Padding(3, 8, 9, 3);
            this.Cancel_Button.Name = "Cancel_Button";
            this.Cancel_Button.TabIndex = 0;
            this.Cancel_Button.Text = "Cancel";
            // 
            // OKButton
            // 
            this.OKButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.OKButton.Location = new System.Drawing.Point(529, 8);
            this.OKButton.Margin = new System.Windows.Forms.Padding(3, 8, 3, 3);
            this.OKButton.Name = "OKButton";
            this.OKButton.TabIndex = 1;
            this.OKButton.Text = "OK";
            // 
            // ErrorLabel
            // 
            this.ErrorLabel.AutoSize = true;
            this.ErrorLabel.Location = new System.Drawing.Point(462, 12);
            this.ErrorLabel.Margin = new System.Windows.Forms.Padding(3, 12, 3, 0);
            this.ErrorLabel.Name = "ErrorLabel";
            this.ErrorLabel.Size = new System.Drawing.Size(61, 13);
            this.ErrorLabel.TabIndex = 4;
            this.ErrorLabel.Text = "[ErrorLabel]";
            this.ErrorLabel.Visible = false;
            // 
            // ErrorIconPictureBox
            // 
            this.ErrorIconPictureBox.Location = new System.Drawing.Point(445, 12);
            this.ErrorIconPictureBox.Margin = new System.Windows.Forms.Padding(0, 12, 0, 0);
            this.ErrorIconPictureBox.Name = "ErrorIconPictureBox";
            this.ErrorIconPictureBox.Size = new System.Drawing.Size(14, 14);
            this.ErrorIconPictureBox.TabIndex = 5;
            this.ErrorIconPictureBox.TabStop = false;
            this.ErrorIconPictureBox.Visible = false;
            // 
            // MainSplitContainer
            // 
            this.MainSplitContainer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.MainSplitContainer.BackColor = System.Drawing.SystemColors.ControlDark;
            this.MainSplitContainer.Location = new System.Drawing.Point(0, 0);
            this.MainSplitContainer.Name = "MainSplitContainer";
            // 
            // MainSplitContainer.Panel1
            // 
            this.MainSplitContainer.Panel1.BackColor = System.Drawing.SystemColors.Control;
            this.MainSplitContainer.Panel1.Controls.Add(this.ThiefBuddyRadioButton);
            this.MainSplitContainer.Panel1.Controls.Add(this.OtherRadioButton);
            this.MainSplitContainer.Panel1.Controls.Add(this.AppearanceRadioButton);
            this.MainSplitContainer.Panel1.Controls.Add(this.PathsRadioButton);
            // 
            // MainSplitContainer.Panel2
            // 
            this.MainSplitContainer.Panel2.BackColor = System.Drawing.SystemColors.Control;
            this.MainSplitContainer.Panel2.Controls.Add(this.PagePanel);
            this.MainSplitContainer.Size = new System.Drawing.Size(694, 613);
            this.MainSplitContainer.SplitterDistance = 155;
            this.MainSplitContainer.TabIndex = 5;
            // 
            // ThiefBuddyRadioButton
            // 
            this.ThiefBuddyRadioButton.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ThiefBuddyRadioButton.Location = new System.Drawing.Point(8, 80);
            this.ThiefBuddyRadioButton.Name = "ThiefBuddyRadioButton";
            this.ThiefBuddyRadioButton.Size = new System.Drawing.Size(136, 23);
            this.ThiefBuddyRadioButton.TabIndex = 3;
            this.ThiefBuddyRadioButton.Text = "Thief Buddy";
            this.ThiefBuddyRadioButton.CheckedChanged += new System.EventHandler(this.PageRadioButtons_CheckedChanged);
            // 
            // OtherRadioButton
            // 
            this.OtherRadioButton.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.OtherRadioButton.Location = new System.Drawing.Point(8, 56);
            this.OtherRadioButton.Name = "OtherRadioButton";
            this.OtherRadioButton.Size = new System.Drawing.Size(136, 23);
            this.OtherRadioButton.TabIndex = 2;
            this.OtherRadioButton.Text = "Other";
            this.OtherRadioButton.CheckedChanged += new System.EventHandler(this.PageRadioButtons_CheckedChanged);
            // 
            // AppearanceRadioButton
            // 
            this.AppearanceRadioButton.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.AppearanceRadioButton.Location = new System.Drawing.Point(8, 32);
            this.AppearanceRadioButton.Name = "AppearanceRadioButton";
            this.AppearanceRadioButton.Size = new System.Drawing.Size(136, 23);
            this.AppearanceRadioButton.TabIndex = 1;
            this.AppearanceRadioButton.Text = "FM Display";
            this.AppearanceRadioButton.CheckedChanged += new System.EventHandler(this.PageRadioButtons_CheckedChanged);
            // 
            // PathsRadioButton
            // 
            this.PathsRadioButton.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.PathsRadioButton.Location = new System.Drawing.Point(8, 8);
            this.PathsRadioButton.Name = "PathsRadioButton";
            this.PathsRadioButton.Size = new System.Drawing.Size(136, 23);
            this.PathsRadioButton.TabIndex = 0;
            this.PathsRadioButton.Text = "Paths";
            this.PathsRadioButton.CheckedChanged += new System.EventHandler(this.PageRadioButtons_CheckedChanged);
            // 
            // PagePanel
            // 
            this.PagePanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PagePanel.Location = new System.Drawing.Point(0, 0);
            this.PagePanel.Name = "PagePanel";
            this.PagePanel.Size = new System.Drawing.Size(535, 613);
            this.PagePanel.TabIndex = 2;
            // 
            // SettingsForm
            // 
            this.AcceptButton = this.OKButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(694, 656);
            this.Controls.Add(this.MainSplitContainer);
            this.Controls.Add(this.BottomFLP);
            this.DoubleBuffered = true;
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(540, 320);
            this.Name = "SettingsForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "SettingsForm";
            this.BottomFLP.ResumeLayout(false);
            this.BottomFLP.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ErrorIconPictureBox)).EndInit();
            this.MainSplitContainer.Panel1.ResumeLayout(false);
            this.MainSplitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.MainSplitContainer)).EndInit();
            this.MainSplitContainer.ResumeLayout(false);
            this.ResumeLayout(false);

    }
#endif

    #endregion

    private System.Windows.Forms.Panel PagePanel;
    private System.Windows.Forms.FlowLayoutPanel BottomFLP;
    private AngelLoader.Forms.CustomControls.StandardButton Cancel_Button;
    private AngelLoader.Forms.CustomControls.StandardButton OKButton;
    private CustomControls.ToolTipCustom MainToolTip;
    private AngelLoader.Forms.CustomControls.DarkLabel ErrorLabel;
    private AngelLoader.Forms.CustomControls.DarkSplitContainerCustom MainSplitContainer;
    private AngelLoader.Forms.CustomControls.DarkRadioButtonCustom OtherRadioButton;
    private AngelLoader.Forms.CustomControls.DarkRadioButtonCustom AppearanceRadioButton;
    private AngelLoader.Forms.CustomControls.DarkRadioButtonCustom PathsRadioButton;
    private System.Windows.Forms.PictureBox ErrorIconPictureBox;
    private CustomControls.DarkRadioButtonCustom ThiefBuddyRadioButton;
}
