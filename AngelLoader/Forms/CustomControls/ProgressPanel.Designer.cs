#define FenGen_DesignerSource

namespace AngelLoader.Forms.CustomControls;

sealed partial class ProgressPanel
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
        SubProgressBar = new DarkProgressBar();
        Cancel_Button = new DarkButton();
        SubPercentLabel = new DarkLabel();
        MainPercentLabel = new DarkLabel();
        MainMessage1Label = new DarkLabel();
        MainProgressBar = new DarkProgressBar();
        SubMessageLabel = new DarkLabel();
        MainMessage2Label = new DarkLabel();
        SuspendLayout();
        // 
        // SubProgressBar
        // 
        SubProgressBar.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
        SubProgressBar.Location = new System.Drawing.Point(9, 138);
        SubProgressBar.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
        SubProgressBar.Name = "SubProgressBar";
        SubProgressBar.Size = new System.Drawing.Size(474, 18);
        SubProgressBar.TabIndex = 6;
        // 
        // Cancel_Button
        // 
        Cancel_Button.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
        Cancel_Button.AutoSize = true;
        Cancel_Button.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        Cancel_Button.Location = new System.Drawing.Point(196, 175);
        Cancel_Button.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
        Cancel_Button.MinimumSize = new System.Drawing.Size(103, 27);
        Cancel_Button.Name = "Cancel_Button";
        Cancel_Button.Padding = new System.Windows.Forms.Padding(7, 0, 7, 0);
        Cancel_Button.Size = new System.Drawing.Size(103, 27);
        Cancel_Button.TabIndex = 7;
        Cancel_Button.Text = "Cancel";
        Cancel_Button.Click += ProgressCancelButton_Click;
        // 
        // SubPercentLabel
        // 
        SubPercentLabel.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
        SubPercentLabel.Location = new System.Drawing.Point(5, 120);
        SubPercentLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
        SubPercentLabel.Name = "SubPercentLabel";
        SubPercentLabel.Size = new System.Drawing.Size(485, 15);
        SubPercentLabel.TabIndex = 5;
        SubPercentLabel.Text = "%";
        SubPercentLabel.TextAlign = System.Drawing.ContentAlignment.TopCenter;
        // 
        // MainPercentLabel
        // 
        MainPercentLabel.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
        MainPercentLabel.Location = new System.Drawing.Point(5, 46);
        MainPercentLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
        MainPercentLabel.Name = "MainPercentLabel";
        MainPercentLabel.Size = new System.Drawing.Size(485, 15);
        MainPercentLabel.TabIndex = 2;
        MainPercentLabel.Text = "%";
        MainPercentLabel.TextAlign = System.Drawing.ContentAlignment.TopCenter;
        // 
        // MainMessage1Label
        // 
        MainMessage1Label.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
        MainMessage1Label.Location = new System.Drawing.Point(5, 9);
        MainMessage1Label.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
        MainMessage1Label.Name = "MainMessage1Label";
        MainMessage1Label.Size = new System.Drawing.Size(485, 15);
        MainMessage1Label.TabIndex = 0;
        MainMessage1Label.Text = "Message";
        MainMessage1Label.TextAlign = System.Drawing.ContentAlignment.TopCenter;
        // 
        // MainProgressBar
        // 
        MainProgressBar.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
        MainProgressBar.Location = new System.Drawing.Point(9, 65);
        MainProgressBar.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
        MainProgressBar.Name = "MainProgressBar";
        MainProgressBar.Size = new System.Drawing.Size(474, 27);
        MainProgressBar.TabIndex = 3;
        // 
        // SubMessageLabel
        // 
        SubMessageLabel.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
        SubMessageLabel.Location = new System.Drawing.Point(5, 102);
        SubMessageLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
        SubMessageLabel.Name = "SubMessageLabel";
        SubMessageLabel.Size = new System.Drawing.Size(485, 15);
        SubMessageLabel.TabIndex = 4;
        SubMessageLabel.Text = "Current thing";
        SubMessageLabel.TextAlign = System.Drawing.ContentAlignment.TopCenter;
        // 
        // MainMessage2Label
        // 
        MainMessage2Label.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
        MainMessage2Label.Location = new System.Drawing.Point(5, 28);
        MainMessage2Label.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
        MainMessage2Label.Name = "MainMessage2Label";
        MainMessage2Label.Size = new System.Drawing.Size(485, 15);
        MainMessage2Label.TabIndex = 1;
        MainMessage2Label.Text = "Current thing";
        MainMessage2Label.TextAlign = System.Drawing.ContentAlignment.TopCenter;
        // 
        // ProgressPanel
        // 
        AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
        AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        Controls.Add(SubProgressBar);
        Controls.Add(Cancel_Button);
        Controls.Add(SubPercentLabel);
        Controls.Add(MainPercentLabel);
        Controls.Add(MainMessage1Label);
        Controls.Add(MainProgressBar);
        Controls.Add(SubMessageLabel);
        Controls.Add(MainMessage2Label);
        Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
        Name = "ProgressPanel";
        Size = new System.Drawing.Size(495, 222);
        ResumeLayout(false);
        PerformLayout();
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
