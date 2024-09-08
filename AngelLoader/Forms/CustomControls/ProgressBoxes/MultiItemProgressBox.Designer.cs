#define FenGen_DesignerSource

namespace AngelLoader.Forms.CustomControls;

sealed partial class MultiItemProgressBox
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            this.Message1Label = new AngelLoader.Forms.CustomControls.DarkLabel();
            this.Cancel_Button = new AngelLoader.Forms.CustomControls.DarkButton();
            this.ItemsDGV = new AngelLoader.Forms.CustomControls.DGV_ProgressItem();
            this.Column1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.MainProgressBar = new AngelLoader.Forms.CustomControls.DarkProgressBar();
            this.MainPercentLabel = new AngelLoader.Forms.CustomControls.DarkLabel();
            ((System.ComponentModel.ISupportInitialize)(this.ItemsDGV)).BeginInit();
            this.SuspendLayout();
            // 
            // Message1Label
            // 
            this.Message1Label.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.Message1Label.Location = new System.Drawing.Point(4, 8);
            this.Message1Label.Name = "Message1Label";
            this.Message1Label.Size = new System.Drawing.Size(416, 13);
            this.Message1Label.TabIndex = 0;
            this.Message1Label.Text = "Message";
            this.Message1Label.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // Cancel_Button
            // 
            this.Cancel_Button.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.Cancel_Button.AutoSize = true;
            this.Cancel_Button.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.Cancel_Button.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.Cancel_Button.Location = new System.Drawing.Point(168, 72);
            this.Cancel_Button.MinimumSize = new System.Drawing.Size(88, 23);
            this.Cancel_Button.Name = "Cancel_Button";
            this.Cancel_Button.Padding = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.Cancel_Button.Size = new System.Drawing.Size(88, 23);
            this.Cancel_Button.TabIndex = 3;
            this.Cancel_Button.Text = "Cancel";
            // 
            // ItemsDGV
            // 
            this.ItemsDGV.AllowUserToAddRows = false;
            this.ItemsDGV.AllowUserToDeleteRows = false;
            this.ItemsDGV.AllowUserToResizeColumns = false;
            this.ItemsDGV.AllowUserToResizeRows = false;
            this.ItemsDGV.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ItemsDGV.BackgroundColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.ItemsDGV.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.ItemsDGV.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            this.ItemsDGV.ColumnHeadersVisible = false;
            this.ItemsDGV.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Column1});
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.TopLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.ItemsDGV.DefaultCellStyle = dataGridViewCellStyle2;
            this.ItemsDGV.Location = new System.Drawing.Point(8, 104);
            this.ItemsDGV.MultiSelect = false;
            this.ItemsDGV.Name = "ItemsDGV";
            this.ItemsDGV.ReadOnly = true;
            this.ItemsDGV.RowHeadersVisible = false;
            this.ItemsDGV.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            this.ItemsDGV.RowTemplate.Height = 51;
            this.ItemsDGV.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.ItemsDGV.Size = new System.Drawing.Size(406, 284);
            this.ItemsDGV.StandardTab = true;
            this.ItemsDGV.TabIndex = 4;
            this.ItemsDGV.VirtualMode = true;
            // 
            // Column1
            // 
            this.Column1.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.Column1.HeaderText = "Column1";
            this.Column1.Name = "Column1";
            this.Column1.ReadOnly = true;
            this.Column1.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.Column1.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Programmatic;
            // 
            // MainProgressBar
            // 
            this.MainProgressBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.MainProgressBar.Location = new System.Drawing.Point(8, 40);
            this.MainProgressBar.Name = "MainProgressBar";
            this.MainProgressBar.Size = new System.Drawing.Size(406, 23);
            this.MainProgressBar.TabIndex = 2;
            // 
            // MainPercentLabel
            // 
            this.MainPercentLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.MainPercentLabel.Location = new System.Drawing.Point(4, 24);
            this.MainPercentLabel.Name = "MainPercentLabel";
            this.MainPercentLabel.Size = new System.Drawing.Size(416, 13);
            this.MainPercentLabel.TabIndex = 1;
            this.MainPercentLabel.Text = "%";
            this.MainPercentLabel.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // MultiItemProgressBox
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Controls.Add(this.MainPercentLabel);
            this.Controls.Add(this.MainProgressBar);
            this.Controls.Add(this.ItemsDGV);
            this.Controls.Add(this.Cancel_Button);
            this.Controls.Add(this.Message1Label);
            this.Name = "MultiItemProgressBox";
            this.Size = new System.Drawing.Size(424, 398);
            ((System.ComponentModel.ISupportInitialize)(this.ItemsDGV)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

    }
#endif

    #endregion

    private DarkLabel Message1Label;
    private DarkButton Cancel_Button;
    private DGV_ProgressItem ItemsDGV;
    private DarkProgressBar MainProgressBar;
    private DarkLabel MainPercentLabel;
    private System.Windows.Forms.DataGridViewTextBoxColumn Column1;
}
