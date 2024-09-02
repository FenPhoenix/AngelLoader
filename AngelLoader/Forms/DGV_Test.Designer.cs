namespace AngelLoader.Forms;

sealed partial class DGV_Test
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

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
            this.dgV_ProgressItem1 = new AngelLoader.Forms.CustomControls.DGV_ProgressItem();
            this.Column1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.dgV_ProgressItem1)).BeginInit();
            this.SuspendLayout();
            // 
            // dgV_ProgressItem1
            // 
            this.dgV_ProgressItem1.AllowUserToAddRows = false;
            this.dgV_ProgressItem1.AllowUserToDeleteRows = false;
            this.dgV_ProgressItem1.AllowUserToResizeRows = false;
            this.dgV_ProgressItem1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            this.dgV_ProgressItem1.ColumnHeadersVisible = false;
            this.dgV_ProgressItem1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Column1});
            this.dgV_ProgressItem1.Location = new System.Drawing.Point(104, 88);
            this.dgV_ProgressItem1.MultiSelect = false;
            this.dgV_ProgressItem1.Name = "dgV_ProgressItem1";
            this.dgV_ProgressItem1.ReadOnly = true;
            this.dgV_ProgressItem1.RowHeadersVisible = false;
            this.dgV_ProgressItem1.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            this.dgV_ProgressItem1.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgV_ProgressItem1.Size = new System.Drawing.Size(544, 240);
            this.dgV_ProgressItem1.TabIndex = 0;
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
            // DGV_Test
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.dgV_ProgressItem1);
            this.Name = "DGV_Test";
            this.Text = "DGV_Test";
            ((System.ComponentModel.ISupportInitialize)(this.dgV_ProgressItem1)).EndInit();
            this.ResumeLayout(false);

    }

    #endregion

    private CustomControls.DGV_ProgressItem dgV_ProgressItem1;
    private System.Windows.Forms.DataGridViewTextBoxColumn Column1;
}