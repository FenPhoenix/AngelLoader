#define FenGen_DesignerSource

namespace AngelLoader.Forms.CustomControls;

sealed partial class ProgressBox
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
    #region Component Designer generated code

#if DEBUG
    /// <summary> 
    /// Required method for Designer support - do not modify 
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
            this.SubProgressBar = new AngelLoader.Forms.CustomControls.DarkProgressBar();
            this.Cancel_Button = new AngelLoader.Forms.CustomControls.DarkButton();
            this.SubPercentLabel = new AngelLoader.Forms.CustomControls.DarkLabel();
            this.MainPercentLabel = new AngelLoader.Forms.CustomControls.DarkLabel();
            this.MainMessage1Label = new AngelLoader.Forms.CustomControls.DarkLabel();
            this.MainProgressBar = new AngelLoader.Forms.CustomControls.DarkProgressBar();
            this.SubMessageLabel = new AngelLoader.Forms.CustomControls.DarkLabel();
            this.MainMessage2Label = new AngelLoader.Forms.CustomControls.DarkLabel();
            this.SuspendLayout();
            // 
            // SubProgressBar
            // 
            this.SubProgressBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.SubProgressBar.Location = new System.Drawing.Point(8, 120);
            this.SubProgressBar.Name = "SubProgressBar";
            this.SubProgressBar.Size = new System.Drawing.Size(406, 16);
            this.SubProgressBar.TabIndex = 6;
            // 
            // Cancel_Button
            // 
            this.Cancel_Button.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.Cancel_Button.AutoSize = true;
            this.Cancel_Button.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.Cancel_Button.Location = new System.Drawing.Point(168, 152);
            this.Cancel_Button.MinimumSize = new System.Drawing.Size(88, 23);
            this.Cancel_Button.Name = "Cancel_Button";
            this.Cancel_Button.Padding = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.Cancel_Button.Size = new System.Drawing.Size(88, 23);
            this.Cancel_Button.TabIndex = 7;
            this.Cancel_Button.Text = "Cancel";
            this.Cancel_Button.Click += new System.EventHandler(this.ProgressCancelButton_Click);
            // 
            // SubPercentLabel
            // 
            this.SubPercentLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.SubPercentLabel.Location = new System.Drawing.Point(4, 104);
            this.SubPercentLabel.Name = "SubPercentLabel";
            this.SubPercentLabel.Size = new System.Drawing.Size(416, 13);
            this.SubPercentLabel.TabIndex = 5;
            this.SubPercentLabel.Text = "%";
            this.SubPercentLabel.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // MainPercentLabel
            // 
            this.MainPercentLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.MainPercentLabel.Location = new System.Drawing.Point(4, 40);
            this.MainPercentLabel.Name = "MainPercentLabel";
            this.MainPercentLabel.Size = new System.Drawing.Size(416, 13);
            this.MainPercentLabel.TabIndex = 2;
            this.MainPercentLabel.Text = "%";
            this.MainPercentLabel.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // MainMessage1Label
            // 
            this.MainMessage1Label.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.MainMessage1Label.Location = new System.Drawing.Point(4, 8);
            this.MainMessage1Label.Name = "MainMessage1Label";
            this.MainMessage1Label.Size = new System.Drawing.Size(416, 13);
            this.MainMessage1Label.TabIndex = 0;
            this.MainMessage1Label.Text = "Message";
            this.MainMessage1Label.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // MainProgressBar
            // 
            this.MainProgressBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.MainProgressBar.Location = new System.Drawing.Point(8, 56);
            this.MainProgressBar.Name = "MainProgressBar";
            this.MainProgressBar.Size = new System.Drawing.Size(406, 23);
            this.MainProgressBar.TabIndex = 3;
            // 
            // SubMessageLabel
            // 
            this.SubMessageLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.SubMessageLabel.Location = new System.Drawing.Point(4, 88);
            this.SubMessageLabel.Name = "SubMessageLabel";
            this.SubMessageLabel.Size = new System.Drawing.Size(416, 13);
            this.SubMessageLabel.TabIndex = 4;
            this.SubMessageLabel.Text = "Current thing";
            this.SubMessageLabel.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // MainMessage2Label
            // 
            this.MainMessage2Label.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.MainMessage2Label.Location = new System.Drawing.Point(4, 24);
            this.MainMessage2Label.Name = "MainMessage2Label";
            this.MainMessage2Label.Size = new System.Drawing.Size(416, 13);
            this.MainMessage2Label.TabIndex = 1;
            this.MainMessage2Label.Text = "Current thing";
            this.MainMessage2Label.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // ProgressBox
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Controls.Add(this.SubProgressBar);
            this.Controls.Add(this.Cancel_Button);
            this.Controls.Add(this.SubPercentLabel);
            this.Controls.Add(this.MainPercentLabel);
            this.Controls.Add(this.MainMessage1Label);
            this.Controls.Add(this.MainProgressBar);
            this.Controls.Add(this.SubMessageLabel);
            this.Controls.Add(this.MainMessage2Label);
            this.Name = "ProgressBox";
            this.Size = new System.Drawing.Size(424, 192);
            this.ResumeLayout(false);
            this.PerformLayout();

    }
#endif

    #endregion
    private DarkButton Cancel_Button;
    private DarkLabel MainPercentLabel;
    private DarkLabel MainMessage1Label;
    private DarkLabel MainMessage2Label;
    private DarkProgressBar MainProgressBar;
    private DarkProgressBar SubProgressBar;
    private DarkLabel SubMessageLabel;
    private DarkLabel SubPercentLabel;
}
