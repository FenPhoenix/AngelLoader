#define FenGen_DesignerSource

namespace AngelLoader.Forms;

partial class FilterDateForm
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
        this.Cancel_Button = new AngelLoader.Forms.CustomControls.DarkButton();
        this.OKButton = new AngelLoader.Forms.CustomControls.DarkButton();
        this.ResetButton = new AngelLoader.Forms.CustomControls.DarkButton();
        this.ToLabel = new AngelLoader.Forms.CustomControls.DarkLabel();
        this.FromLabel = new AngelLoader.Forms.CustomControls.DarkLabel();
        this.FromDateTimePicker = new AngelLoader.Forms.CustomControls.DarkDateTimePicker();
        this.ToDateTimePicker = new AngelLoader.Forms.CustomControls.DarkDateTimePicker();
        this.FromCheckBox = new AngelLoader.Forms.CustomControls.DarkCheckBox();
        this.ToCheckBox = new AngelLoader.Forms.CustomControls.DarkCheckBox();
        this.NoMinLabel = new AngelLoader.Forms.CustomControls.DarkTextBox();
        this.NoMaxLabel = new AngelLoader.Forms.CustomControls.DarkTextBox();
        this.BottomFLP = new System.Windows.Forms.FlowLayoutPanel();
        this.BottomFLP.SuspendLayout();
        this.SuspendLayout();
        // 
        // Cancel_Button
        // 
        this.Cancel_Button.AutoSize = true;
        this.Cancel_Button.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        this.Cancel_Button.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        this.Cancel_Button.Location = new System.Drawing.Point(88, 3);
        this.Cancel_Button.MinimumSize = new System.Drawing.Size(75, 23);
        this.Cancel_Button.Name = "Cancel_Button";
        this.Cancel_Button.Padding = new System.Windows.Forms.Padding(6, 0, 6, 0);
        this.Cancel_Button.Size = new System.Drawing.Size(75, 23);
        this.Cancel_Button.TabIndex = 0;
        this.Cancel_Button.Text = "Cancel";
        // 
        // OKButton
        // 
        this.OKButton.AutoSize = true;
        this.OKButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        this.OKButton.DialogResult = System.Windows.Forms.DialogResult.OK;
        this.OKButton.Location = new System.Drawing.Point(7, 3);
        this.OKButton.MinimumSize = new System.Drawing.Size(75, 23);
        this.OKButton.Name = "OKButton";
        this.OKButton.Padding = new System.Windows.Forms.Padding(6, 0, 6, 0);
        this.OKButton.Size = new System.Drawing.Size(75, 23);
        this.OKButton.TabIndex = 1;
        this.OKButton.Text = "OK";
        // 
        // ResetButton
        // 
        this.ResetButton.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
        | System.Windows.Forms.AnchorStyles.Right)));
        this.ResetButton.Location = new System.Drawing.Point(7, 88);
        this.ResetButton.Name = "ResetButton";
        this.ResetButton.Padding = new System.Windows.Forms.Padding(6, 0, 6, 0);
        this.ResetButton.Size = new System.Drawing.Size(156, 22);
        this.ResetButton.TabIndex = 9;
        this.ResetButton.Text = "Reset";
        this.ResetButton.Click += new System.EventHandler(this.ResetButton_Click);
        // 
        // ToLabel
        // 
        this.ToLabel.AutoSize = true;
        this.ToLabel.Location = new System.Drawing.Point(8, 48);
        this.ToLabel.Name = "ToLabel";
        this.ToLabel.Size = new System.Drawing.Size(23, 13);
        this.ToLabel.TabIndex = 5;
        this.ToLabel.Text = "To:";
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
        // FromDateTimePicker
        // 
        this.FromDateTimePicker.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
        | System.Windows.Forms.AnchorStyles.Right)));
        this.FromDateTimePicker.Format = System.Windows.Forms.DateTimePickerFormat.Short;
        this.FromDateTimePicker.Location = new System.Drawing.Point(28, 24);
        this.FromDateTimePicker.Name = "FromDateTimePicker";
        this.FromDateTimePicker.Size = new System.Drawing.Size(134, 20);
        this.FromDateTimePicker.TabIndex = 3;
        this.FromDateTimePicker.Visible = false;
        // 
        // ToDateTimePicker
        // 
        this.ToDateTimePicker.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
        | System.Windows.Forms.AnchorStyles.Right)));
        this.ToDateTimePicker.Format = System.Windows.Forms.DateTimePickerFormat.Short;
        this.ToDateTimePicker.Location = new System.Drawing.Point(28, 64);
        this.ToDateTimePicker.Name = "ToDateTimePicker";
        this.ToDateTimePicker.Size = new System.Drawing.Size(134, 20);
        this.ToDateTimePicker.TabIndex = 7;
        this.ToDateTimePicker.Visible = false;
        // 
        // FromCheckBox
        // 
        this.FromCheckBox.AutoSize = true;
        this.FromCheckBox.Location = new System.Drawing.Point(12, 27);
        this.FromCheckBox.Name = "FromCheckBox";
        this.FromCheckBox.Size = new System.Drawing.Size(15, 14);
        this.FromCheckBox.TabIndex = 2;
        this.FromCheckBox.UseVisualStyleBackColor = true;
        this.FromCheckBox.CheckedChanged += new System.EventHandler(this.CheckBoxes_CheckedChanged);
        // 
        // ToCheckBox
        // 
        this.ToCheckBox.AutoSize = true;
        this.ToCheckBox.Location = new System.Drawing.Point(12, 67);
        this.ToCheckBox.Name = "ToCheckBox";
        this.ToCheckBox.Size = new System.Drawing.Size(15, 14);
        this.ToCheckBox.TabIndex = 6;
        this.ToCheckBox.UseVisualStyleBackColor = true;
        this.ToCheckBox.CheckedChanged += new System.EventHandler(this.CheckBoxes_CheckedChanged);
        // 
        // NoMinLabel
        // 
        this.NoMinLabel.Enabled = false;
        this.NoMinLabel.Location = new System.Drawing.Point(56, 0);
        this.NoMinLabel.Name = "NoMinLabel";
        this.NoMinLabel.Size = new System.Drawing.Size(40, 20);
        this.NoMinLabel.TabIndex = 4;
        this.NoMinLabel.Text = "(no minimum)";
        // 
        // NoMaxLabel
        // 
        this.NoMaxLabel.Enabled = false;
        this.NoMaxLabel.Location = new System.Drawing.Point(104, 0);
        this.NoMaxLabel.Name = "NoMaxLabel";
        this.NoMaxLabel.Size = new System.Drawing.Size(40, 20);
        this.NoMaxLabel.TabIndex = 8;
        this.NoMaxLabel.Text = "(no maximum)";
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
        // FilterDateForm
        // 
        this.AcceptButton = this.OKButton;
        this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.CancelButton = this.Cancel_Button;
        this.ClientSize = new System.Drawing.Size(170, 158);
        this.Controls.Add(this.BottomFLP);
        this.Controls.Add(this.NoMaxLabel);
        this.Controls.Add(this.NoMinLabel);
        this.Controls.Add(this.ToCheckBox);
        this.Controls.Add(this.FromCheckBox);
        this.Controls.Add(this.ToDateTimePicker);
        this.Controls.Add(this.FromDateTimePicker);
        this.Controls.Add(this.ResetButton);
        this.Controls.Add(this.ToLabel);
        this.Controls.Add(this.FromLabel);
        this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.Name = "FilterDateForm";
        this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
        this.Text = "Set release date filter";
        this.BottomFLP.ResumeLayout(false);
        this.BottomFLP.PerformLayout();
        this.ResumeLayout(false);
        this.PerformLayout();

    }
#endif

    #endregion

    private AngelLoader.Forms.CustomControls.DarkButton Cancel_Button;
    private AngelLoader.Forms.CustomControls.DarkButton OKButton;
    private AngelLoader.Forms.CustomControls.DarkButton ResetButton;
    private AngelLoader.Forms.CustomControls.DarkLabel ToLabel;
    private AngelLoader.Forms.CustomControls.DarkLabel FromLabel;
    private AngelLoader.Forms.CustomControls.DarkDateTimePicker FromDateTimePicker;
    private AngelLoader.Forms.CustomControls.DarkDateTimePicker ToDateTimePicker;
    private AngelLoader.Forms.CustomControls.DarkCheckBox FromCheckBox;
    private AngelLoader.Forms.CustomControls.DarkCheckBox ToCheckBox;
    private AngelLoader.Forms.CustomControls.DarkTextBox NoMinLabel;
    private AngelLoader.Forms.CustomControls.DarkTextBox NoMaxLabel;
    private System.Windows.Forms.FlowLayoutPanel BottomFLP;
}
