
namespace AngelLoader.Forms
{
    partial class DarkListBox2_Test_Form
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle8 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle9 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle7 = new System.Windows.Forms.DataGridViewCellStyle();
            this.listView1 = new System.Windows.Forms.ListView();
            this.darkListBox21 = new AngelLoader.Forms.CustomControls.DarkListBox();
            this.Column1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.button1 = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.darkListBox21)).BeginInit();
            this.SuspendLayout();
            // 
            // listView1
            // 
            this.listView1.HideSelection = false;
            this.listView1.Location = new System.Drawing.Point(560, 176);
            this.listView1.Name = "listView1";
            this.listView1.Size = new System.Drawing.Size(136, 88);
            this.listView1.TabIndex = 1;
            this.listView1.UseCompatibleStateImageBehavior = false;
            // 
            // darkListBox21
            // 
            this.darkListBox21.AllowUserToAddRows = false;
            this.darkListBox21.AllowUserToDeleteRows = false;
            this.darkListBox21.AllowUserToOrderColumns = true;
            this.darkListBox21.AllowUserToResizeColumns = false;
            this.darkListBox21.AllowUserToResizeRows = false;
            this.darkListBox21.BackgroundColor = System.Drawing.SystemColors.Window;
            this.darkListBox21.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.None;
            this.darkListBox21.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
            this.darkListBox21.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.darkListBox21.ColumnHeadersVisible = false;
            this.darkListBox21.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Column1});
            this.darkListBox21.Location = new System.Drawing.Point(192, 112);
            this.darkListBox21.Name = "darkListBox21";
            this.darkListBox21.RowHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
            dataGridViewCellStyle8.Alignment = System.Windows.Forms.DataGridViewContentAlignment.TopLeft;
            dataGridViewCellStyle8.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle8.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle8.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle8.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle8.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle8.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.darkListBox21.RowHeadersDefaultCellStyle = dataGridViewCellStyle8;
            this.darkListBox21.RowHeadersVisible = false;
            this.darkListBox21.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            dataGridViewCellStyle9.BackColor = System.Drawing.SystemColors.Window;
            this.darkListBox21.RowsDefaultCellStyle = dataGridViewCellStyle9;
            this.darkListBox21.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
            this.darkListBox21.Size = new System.Drawing.Size(240, 150);
            this.darkListBox21.TabIndex = 0;
            // 
            // Column1
            // 
            this.Column1.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            dataGridViewCellStyle7.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.Column1.DefaultCellStyle = dataGridViewCellStyle7;
            this.Column1.HeaderText = "Column1";
            this.Column1.Name = "Column1";
            this.Column1.ReadOnly = true;
            this.Column1.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.Column1.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(360, 264);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 2;
            this.button1.Text = "button1";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // DarkListBox2_Test_Form
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.listView1);
            this.Controls.Add(this.darkListBox21);
            this.Name = "DarkListBox2_Test_Form";
            this.Text = "DarkListBox2_Test_Form";
            this.Load += new System.EventHandler(this.DarkListBox2_Test_Form_Load);
            this.Shown += new System.EventHandler(this.DarkListBox2_Test_Form_Shown);
            ((System.ComponentModel.ISupportInitialize)(this.darkListBox21)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private CustomControls.DarkListBox darkListBox21;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column1;
        private System.Windows.Forms.ListView listView1;
        private System.Windows.Forms.Button button1;
    }
}