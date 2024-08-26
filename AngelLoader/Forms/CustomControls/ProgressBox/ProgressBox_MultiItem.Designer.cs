namespace AngelLoader.Forms.CustomControls.ProgressBox;

partial class ProgressBox_MultiItem
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

    /// <summary> 
    /// Required method for Designer support - do not modify 
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
            this.MainMessage1Label = new AngelLoader.Forms.CustomControls.DarkLabel();
            this.MainMessage2Label = new AngelLoader.Forms.CustomControls.DarkLabel();
            this.Cancel_Button = new AngelLoader.Forms.CustomControls.DarkButton();
            this.ItemsFLP = new AngelLoader.Forms.CustomControls.DarkFlowLayoutPanel();
            this.SuspendLayout();
            // 
            // MainMessage1Label
            // 
            this.MainMessage1Label.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.MainMessage1Label.Location = new System.Drawing.Point(4, 8);
            this.MainMessage1Label.Name = "MainMessage1Label";
            this.MainMessage1Label.Size = new System.Drawing.Size(418, 13);
            this.MainMessage1Label.TabIndex = 2;
            this.MainMessage1Label.Text = "Message1";
            this.MainMessage1Label.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // MainMessage2Label
            // 
            this.MainMessage2Label.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.MainMessage2Label.Location = new System.Drawing.Point(4, 24);
            this.MainMessage2Label.Name = "MainMessage2Label";
            this.MainMessage2Label.Size = new System.Drawing.Size(418, 13);
            this.MainMessage2Label.TabIndex = 3;
            this.MainMessage2Label.Text = "Message2";
            this.MainMessage2Label.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // Cancel_Button
            // 
            this.Cancel_Button.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.Cancel_Button.AutoSize = true;
            this.Cancel_Button.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.Cancel_Button.Location = new System.Drawing.Point(168, 48);
            this.Cancel_Button.MinimumSize = new System.Drawing.Size(88, 23);
            this.Cancel_Button.Name = "Cancel_Button";
            this.Cancel_Button.Padding = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.Cancel_Button.Size = new System.Drawing.Size(88, 23);
            this.Cancel_Button.TabIndex = 8;
            this.Cancel_Button.Text = "Cancel";
            // 
            // ItemsFLP
            // 
            this.ItemsFLP.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ItemsFLP.AutoScroll = true;
            this.ItemsFLP.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.ItemsFLP.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.ItemsFLP.Location = new System.Drawing.Point(8, 80);
            this.ItemsFLP.Name = "ItemsFLP";
            this.ItemsFLP.Size = new System.Drawing.Size(408, 104);
            this.ItemsFLP.TabIndex = 9;
            // 
            // ProgressBox_MultiItem
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Controls.Add(this.ItemsFLP);
            this.Controls.Add(this.Cancel_Button);
            this.Controls.Add(this.MainMessage1Label);
            this.Controls.Add(this.MainMessage2Label);
            this.Name = "ProgressBox_MultiItem";
            this.Size = new System.Drawing.Size(424, 192);
            this.ResumeLayout(false);
            this.PerformLayout();

    }

    #endregion

    private DarkLabel MainMessage1Label;
    private DarkLabel MainMessage2Label;
    private DarkButton Cancel_Button;
    private DarkFlowLayoutPanel ItemsFLP;
}
