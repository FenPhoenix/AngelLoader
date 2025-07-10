#define FenGen_DesignerSource

namespace AngelLoader.Forms;

partial class OriginalGameModsForm
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
        this.components = new System.ComponentModel.Container();
        this.OrigGameModsControl = new AngelLoader.Forms.CustomControls.ModsControl();
        this.OKButton = new AngelLoader.Forms.CustomControls.StandardButton();
        this.BottomFLP = new AngelLoader.Forms.CustomControls.FlowLayoutPanelCustom();
        this.Cancel_Button = new AngelLoader.Forms.CustomControls.StandardButton();
        this.NewMantleCheckBox = new AngelLoader.Forms.CustomControls.DarkCheckBox();
        this.MainToolTip = new CustomControls.ToolTipCustom(this.components);
        this.BottomFLP.SuspendLayout();
        this.SuspendLayout();
        // 
        // OrigGameModsControl
        // 
        this.OrigGameModsControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
        | System.Windows.Forms.AnchorStyles.Left)
        | System.Windows.Forms.AnchorStyles.Right)));
        this.OrigGameModsControl.Location = new System.Drawing.Point(0, 64);
        this.OrigGameModsControl.Name = "OrigGameModsControl";
        this.OrigGameModsControl.Size = new System.Drawing.Size(532, 404);
        this.OrigGameModsControl.TabIndex = 3;
        // 
        // OKButton
        // 
        this.OKButton.DialogResult = System.Windows.Forms.DialogResult.OK;
        this.OKButton.Location = new System.Drawing.Point(364, 0);
        this.OKButton.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
        this.OKButton.Name = "OKButton";
        this.OKButton.TabIndex = 0;
        this.OKButton.Text = "OK";
        // 
        // BottomFLP
        // 
        this.BottomFLP.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
        | System.Windows.Forms.AnchorStyles.Right)));
        this.BottomFLP.Controls.Add(this.Cancel_Button);
        this.BottomFLP.Controls.Add(this.OKButton);
        this.BottomFLP.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
        this.BottomFLP.Location = new System.Drawing.Point(8, 469);
        this.BottomFLP.Name = "BottomFLP";
        this.BottomFLP.Size = new System.Drawing.Size(517, 23);
        this.BottomFLP.TabIndex = 0;
        // 
        // Cancel_Button
        // 
        this.Cancel_Button.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        this.Cancel_Button.Location = new System.Drawing.Point(442, 0);
        this.Cancel_Button.Margin = new System.Windows.Forms.Padding(0);
        this.Cancel_Button.Name = "Cancel_Button";
        this.Cancel_Button.TabIndex = 1;
        this.Cancel_Button.Text = "Cancel";
        // 
        // NewMantleCheckBox
        // 
        this.NewMantleCheckBox.AutoSize = true;
        this.NewMantleCheckBox.Checked = true;
        this.NewMantleCheckBox.CheckState = System.Windows.Forms.CheckState.Indeterminate;
        this.NewMantleCheckBox.Location = new System.Drawing.Point(8, 16);
        this.NewMantleCheckBox.Name = "NewMantleCheckBox";
        this.NewMantleCheckBox.Size = new System.Drawing.Size(82, 17);
        this.NewMantleCheckBox.TabIndex = 1;
        this.NewMantleCheckBox.Text = "New mantle";
        this.NewMantleCheckBox.ThreeState = true;
        // 
        // OriginalGameModsForm
        // 
        this.AcceptButton = this.OKButton;
        this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.CancelButton = this.Cancel_Button;
        this.ClientSize = new System.Drawing.Size(532, 500);
        this.Controls.Add(this.NewMantleCheckBox);
        this.Controls.Add(this.BottomFLP);
        this.Controls.Add(this.OrigGameModsControl);
        this.KeyPreview = true;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.MinimumSize = new System.Drawing.Size(200, 200);
        this.Name = "OriginalGameModsForm";
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
        this.Text = "Manage mods for [Game]";
        this.BottomFLP.ResumeLayout(false);
        this.BottomFLP.PerformLayout();
        this.ResumeLayout(false);
        this.PerformLayout();

    }
#endif

    #endregion

    private CustomControls.ModsControl OrigGameModsControl;
    private CustomControls.StandardButton OKButton;
    private AngelLoader.Forms.CustomControls.FlowLayoutPanelCustom BottomFLP;
    private CustomControls.StandardButton Cancel_Button;
    private CustomControls.DarkCheckBox NewMantleCheckBox;
    private CustomControls.ToolTipCustom MainToolTip;
}
