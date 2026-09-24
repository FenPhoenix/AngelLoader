#define FenGen_DesignerSource

namespace AngelLoader.Forms;

sealed partial class FilterRatingForm
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
        this.FromLabel = new AngelLoader.Forms.CustomControls.DarkLabel();
        this.ToLabel = new AngelLoader.Forms.CustomControls.DarkLabel();
        this.FromComboBox = new AngelLoader.Forms.CustomControls.DarkComboBox();
        this.ToComboBox = new AngelLoader.Forms.CustomControls.DarkComboBox();
        this.OKButton = new AngelLoader.Forms.CustomControls.StandardButton();
        this.Cancel_Button = new AngelLoader.Forms.CustomControls.StandardButton();
        this.ResetButton = new AngelLoader.Forms.CustomControls.DarkButton();
        this.BottomFLP = new AngelLoader.Forms.CustomControls.FlowLayoutPanelCustom();
        this.BottomFLP.SuspendLayout();
        this.SuspendLayout();
        // 
        // FromLabel
        // 
        this.FromLabel.AutoSize = true;
        this.FromLabel.Location = new System.Drawing.Point(8, 8);
        this.FromLabel.Name = "FromLabel";
        this.FromLabel.Size = new System.Drawing.Size(33, 13);
        this.FromLabel.TabIndex = 1;
        this.FromLabel.Text = "From:";
        // 
        // ToLabel
        // 
        this.ToLabel.AutoSize = true;
        this.ToLabel.Location = new System.Drawing.Point(8, 48);
        this.ToLabel.Name = "ToLabel";
        this.ToLabel.Size = new System.Drawing.Size(23, 13);
        this.ToLabel.TabIndex = 3;
        this.ToLabel.Text = "To:";
        // 
        // FromComboBox
        // 
        this.FromComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
        | System.Windows.Forms.AnchorStyles.Right)));
        this.FromComboBox.FormattingEnabled = true;
        this.FromComboBox.Location = new System.Drawing.Point(8, 24);
        this.FromComboBox.Name = "FromComboBox";
        this.FromComboBox.Size = new System.Drawing.Size(154, 21);
        this.FromComboBox.TabIndex = 2;
        this.FromComboBox.SelectedIndexChanged += new System.EventHandler(this.ComboBoxes_SelectedIndexChanged);
        // 
        // ToComboBox
        // 
        this.ToComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
        | System.Windows.Forms.AnchorStyles.Right)));
        this.ToComboBox.FormattingEnabled = true;
        this.ToComboBox.Location = new System.Drawing.Point(8, 64);
        this.ToComboBox.Name = "ToComboBox";
        this.ToComboBox.Size = new System.Drawing.Size(154, 21);
        this.ToComboBox.TabIndex = 4;
        this.ToComboBox.SelectedIndexChanged += new System.EventHandler(this.ComboBoxes_SelectedIndexChanged);
        // 
        // OKButton
        // 
        this.OKButton.DialogResult = System.Windows.Forms.DialogResult.OK;
        this.OKButton.Location = new System.Drawing.Point(7, 3);
        this.OKButton.Name = "OKButton";
        this.OKButton.TabIndex = 1;
        this.OKButton.Text = "OK";
        // 
        // Cancel_Button
        // 
        this.Cancel_Button.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        this.Cancel_Button.Location = new System.Drawing.Point(88, 3);
        this.Cancel_Button.Name = "Cancel_Button";
        this.Cancel_Button.TabIndex = 0;
        this.Cancel_Button.Text = "Cancel";
        // 
        // ResetButton
        // 
        this.ResetButton.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
        | System.Windows.Forms.AnchorStyles.Right)));
        this.ResetButton.Location = new System.Drawing.Point(7, 88);
        this.ResetButton.Name = "ResetButton";
        this.ResetButton.Padding = new System.Windows.Forms.Padding(6, 0, 6, 0);
        this.ResetButton.Size = new System.Drawing.Size(156, 22);
        this.ResetButton.TabIndex = 5;
        this.ResetButton.Text = "Reset";
        this.ResetButton.Click += new System.EventHandler(this.ResetButton_Click);
        // 
        // BottomFLP
        // 
        this.BottomFLP.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
        | System.Windows.Forms.AnchorStyles.Right)));
        this.BottomFLP.Controls.Add(this.Cancel_Button);
        this.BottomFLP.Controls.Add(this.OKButton);
        this.BottomFLP.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
        this.BottomFLP.Location = new System.Drawing.Point(0, 125);
        this.BottomFLP.Name = "BottomFLP";
        this.BottomFLP.Padding = new System.Windows.Forms.Padding(0, 0, 4, 0);
        this.BottomFLP.Size = new System.Drawing.Size(170, 33);
        this.BottomFLP.TabIndex = 0;
        // 
        // FilterRatingForm
        // 
        this.AcceptButton = this.OKButton;
        this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.CancelButton = this.Cancel_Button;
        this.ClientSize = new System.Drawing.Size(170, 158);
        this.Controls.Add(this.BottomFLP);
        this.Controls.Add(this.ResetButton);
        this.Controls.Add(this.ToComboBox);
        this.Controls.Add(this.FromComboBox);
        this.Controls.Add(this.ToLabel);
        this.Controls.Add(this.FromLabel);
        this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.Name = "FilterRatingForm";
        this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
        this.Text = "Set rating filter";
        this.BottomFLP.ResumeLayout(false);
        this.BottomFLP.PerformLayout();
        this.ResumeLayout(false);
        this.PerformLayout();

    }
#endif

    #endregion

    private AngelLoader.Forms.CustomControls.DarkLabel FromLabel;
    private AngelLoader.Forms.CustomControls.DarkLabel ToLabel;
    private AngelLoader.Forms.CustomControls.DarkComboBox FromComboBox;
    private AngelLoader.Forms.CustomControls.DarkComboBox ToComboBox;
    private AngelLoader.Forms.CustomControls.StandardButton OKButton;
    private AngelLoader.Forms.CustomControls.StandardButton Cancel_Button;
    private AngelLoader.Forms.CustomControls.DarkButton ResetButton;
    private AngelLoader.Forms.CustomControls.FlowLayoutPanelCustom BottomFLP;
}
