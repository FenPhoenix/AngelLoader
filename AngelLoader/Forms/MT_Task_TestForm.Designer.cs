namespace AngelLoader.Forms;

sealed partial class MT_Task_TestForm
{
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.drawnPanel1 = new AngelLoader.Forms.CustomControls.DrawnPanel();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // pictureBox1
            // 
            this.pictureBox1.Location = new System.Drawing.Point(208, 208);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(380, 13);
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;
            // 
            // drawnPanel1
            // 
            this.drawnPanel1.Location = new System.Drawing.Point(208, 248);
            this.drawnPanel1.Name = "drawnPanel1";
            this.drawnPanel1.Size = new System.Drawing.Size(380, 13);
            this.drawnPanel1.TabIndex = 1;
            // 
            // MT_Task_TestForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.drawnPanel1);
            this.Controls.Add(this.pictureBox1);
            this.Name = "MT_Task_TestForm";
            this.Text = "MT_Task_TestForm";
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.PictureBox pictureBox1;
    private CustomControls.DrawnPanel drawnPanel1;
}