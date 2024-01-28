﻿#define FenGen_DesignerSource

namespace Update;

sealed partial class DarkTaskDialog
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
        this.IconPictureBox = new System.Windows.Forms.PictureBox();
        this.BottomFLP = new System.Windows.Forms.FlowLayoutPanel();
        this.Cancel_Button = new DarkButton();
        this.NoButton = new DarkButton();
        this.YesButton = new DarkButton();
        this.MessageLabel = new DarkLabel();
        ((System.ComponentModel.ISupportInitialize)(this.IconPictureBox)).BeginInit();
        this.BottomFLP.SuspendLayout();
        this.SuspendLayout();
        // 
        // IconPictureBox
        // 
        this.IconPictureBox.Location = new System.Drawing.Point(10, 10);
        this.IconPictureBox.Name = "IconPictureBox";
        this.IconPictureBox.Size = new System.Drawing.Size(32, 32);
        this.IconPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
        this.IconPictureBox.TabIndex = 0;
        this.IconPictureBox.TabStop = false;
        // 
        // BottomFLP
        // 
        this.BottomFLP.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
        | System.Windows.Forms.AnchorStyles.Right)));
        this.BottomFLP.BackColor = System.Drawing.SystemColors.Control;
        this.BottomFLP.Controls.Add(this.Cancel_Button);
        this.BottomFLP.Controls.Add(this.NoButton);
        this.BottomFLP.Controls.Add(this.YesButton);
        this.BottomFLP.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
        this.BottomFLP.Location = new System.Drawing.Point(0, 169);
        this.BottomFLP.Margin = new System.Windows.Forms.Padding(0);
        this.BottomFLP.Name = "BottomFLP";
        this.BottomFLP.Padding = new System.Windows.Forms.Padding(0, 0, 7, 0);
        this.BottomFLP.Size = new System.Drawing.Size(532, 42);
        this.BottomFLP.TabIndex = 1;
        // 
        // Cancel_Button
        // 
        this.Cancel_Button.AutoSize = true;
        this.Cancel_Button.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        this.Cancel_Button.Location = new System.Drawing.Point(446, 9);
        this.Cancel_Button.Margin = new System.Windows.Forms.Padding(3, 9, 3, 3);
        this.Cancel_Button.MinimumSize = new System.Drawing.Size(76, 23);
        this.Cancel_Button.Name = "Cancel_Button";
        this.Cancel_Button.Padding = new System.Windows.Forms.Padding(10, 0, 10, 0);
        this.Cancel_Button.Size = new System.Drawing.Size(76, 23);
        this.Cancel_Button.TabIndex = 2;
        this.Cancel_Button.Text = "Cancel";
        // 
        // NoButton
        // 
        this.NoButton.AutoSize = true;
        this.NoButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        this.NoButton.Location = new System.Drawing.Point(364, 9);
        this.NoButton.Margin = new System.Windows.Forms.Padding(3, 9, 3, 3);
        this.NoButton.MinimumSize = new System.Drawing.Size(76, 23);
        this.NoButton.Name = "NoButton";
        this.NoButton.Padding = new System.Windows.Forms.Padding(10, 0, 10, 0);
        this.NoButton.Size = new System.Drawing.Size(76, 23);
        this.NoButton.TabIndex = 1;
        this.NoButton.Text = "No";
        // 
        // YesButton
        // 
        this.YesButton.AutoSize = true;
        this.YesButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        this.YesButton.Location = new System.Drawing.Point(282, 9);
        this.YesButton.Margin = new System.Windows.Forms.Padding(3, 9, 3, 3);
        this.YesButton.MinimumSize = new System.Drawing.Size(76, 23);
        this.YesButton.Name = "YesButton";
        this.YesButton.Padding = new System.Windows.Forms.Padding(10, 0, 10, 0);
        this.YesButton.Size = new System.Drawing.Size(76, 23);
        this.YesButton.TabIndex = 0;
        this.YesButton.Text = "Yes";
        // 
        // MessageLabel
        // 
        this.MessageLabel.AutoSize = true;
        this.MessageLabel.Location = new System.Drawing.Point(52, 15);
        this.MessageLabel.Name = "MessageLabel";
        this.MessageLabel.Size = new System.Drawing.Size(55, 13);
        this.MessageLabel.TabIndex = 0;
        this.MessageLabel.Text = "[message]";
        // 
        // DarkTaskDialog
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.BackColor = System.Drawing.SystemColors.Window;
        this.ClientSize = new System.Drawing.Size(532, 211);
        this.Controls.Add(this.BottomFLP);
        this.Controls.Add(this.MessageLabel);
        this.Controls.Add(this.IconPictureBox);
        this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.Name = "DarkTaskDialog";
        this.ShowIcon = false;
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
        this.Text = "DarkTaskDialog";
        ((System.ComponentModel.ISupportInitialize)(this.IconPictureBox)).EndInit();
        this.BottomFLP.ResumeLayout(false);
        this.BottomFLP.PerformLayout();
        this.ResumeLayout(false);
        this.PerformLayout();

    }
#endif

    #endregion

    private System.Windows.Forms.PictureBox IconPictureBox;
    private DarkLabel MessageLabel;
    private System.Windows.Forms.FlowLayoutPanel BottomFLP;
    private DarkButton Cancel_Button;
    private DarkButton NoButton;
    private DarkButton YesButton;
}
