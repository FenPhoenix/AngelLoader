namespace AngelLoader.Forms.Import
{
    partial class ImportAllSelectiveForm
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
            this.user_DL_ImportControls1 = new AngelLoader.Forms.Import.User_DL_ImportControls();
            this.user_FMSel_NDL_ImportControls1 = new AngelLoader.Forms.Import.User_FMSel_NDL_ImportControls();
            this.DL_GroupBox = new System.Windows.Forms.GroupBox();
            this.FMSel_GroupBox = new System.Windows.Forms.GroupBox();
            this.NDL_GroupBox = new System.Windows.Forms.GroupBox();
            this.user_FMSel_NDL_ImportControls2 = new AngelLoader.Forms.Import.User_FMSel_NDL_ImportControls();
            this.DL_GroupBox.SuspendLayout();
            this.FMSel_GroupBox.SuspendLayout();
            this.NDL_GroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // user_DL_ImportControls1
            // 
            this.user_DL_ImportControls1.Location = new System.Drawing.Point(8, 16);
            this.user_DL_ImportControls1.Name = "user_DL_ImportControls1";
            this.user_DL_ImportControls1.Size = new System.Drawing.Size(540, 245);
            this.user_DL_ImportControls1.TabIndex = 0;
            // 
            // user_FMSel_NDL_ImportControls1
            // 
            this.user_FMSel_NDL_ImportControls1.Location = new System.Drawing.Point(8, 16);
            this.user_FMSel_NDL_ImportControls1.Name = "user_FMSel_NDL_ImportControls1";
            this.user_FMSel_NDL_ImportControls1.Size = new System.Drawing.Size(551, 494);
            this.user_FMSel_NDL_ImportControls1.TabIndex = 1;
            // 
            // DL_GroupBox
            // 
            this.DL_GroupBox.Controls.Add(this.user_DL_ImportControls1);
            this.DL_GroupBox.Location = new System.Drawing.Point(8, 8);
            this.DL_GroupBox.Name = "DL_GroupBox";
            this.DL_GroupBox.Size = new System.Drawing.Size(560, 272);
            this.DL_GroupBox.TabIndex = 2;
            this.DL_GroupBox.TabStop = false;
            this.DL_GroupBox.Text = "DarkLoader";
            // 
            // FMSel_GroupBox
            // 
            this.FMSel_GroupBox.Controls.Add(this.user_FMSel_NDL_ImportControls1);
            this.FMSel_GroupBox.Location = new System.Drawing.Point(8, 288);
            this.FMSel_GroupBox.Name = "FMSel_GroupBox";
            this.FMSel_GroupBox.Size = new System.Drawing.Size(568, 520);
            this.FMSel_GroupBox.TabIndex = 3;
            this.FMSel_GroupBox.TabStop = false;
            this.FMSel_GroupBox.Text = "FMSel";
            // 
            // NDL_GroupBox
            // 
            this.NDL_GroupBox.Controls.Add(this.user_FMSel_NDL_ImportControls2);
            this.NDL_GroupBox.Location = new System.Drawing.Point(8, 816);
            this.NDL_GroupBox.Name = "NDL_GroupBox";
            this.NDL_GroupBox.Size = new System.Drawing.Size(568, 520);
            this.NDL_GroupBox.TabIndex = 4;
            this.NDL_GroupBox.TabStop = false;
            this.NDL_GroupBox.Text = "NewDarkLoader";
            // 
            // user_FMSel_NDL_ImportControls2
            // 
            this.user_FMSel_NDL_ImportControls2.Location = new System.Drawing.Point(8, 16);
            this.user_FMSel_NDL_ImportControls2.Name = "user_FMSel_NDL_ImportControls2";
            this.user_FMSel_NDL_ImportControls2.Size = new System.Drawing.Size(551, 494);
            this.user_FMSel_NDL_ImportControls2.TabIndex = 1;
            // 
            // ImportAllSelectiveForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 1093);
            this.Controls.Add(this.NDL_GroupBox);
            this.Controls.Add(this.FMSel_GroupBox);
            this.Controls.Add(this.DL_GroupBox);
            this.Name = "ImportAllSelectiveForm";
            this.Text = "ImportAllSelectiveForm";
            this.DL_GroupBox.ResumeLayout(false);
            this.FMSel_GroupBox.ResumeLayout(false);
            this.NDL_GroupBox.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private User_DL_ImportControls user_DL_ImportControls1;
        private User_FMSel_NDL_ImportControls user_FMSel_NDL_ImportControls1;
        private System.Windows.Forms.GroupBox DL_GroupBox;
        private System.Windows.Forms.GroupBox FMSel_GroupBox;
        private System.Windows.Forms.GroupBox NDL_GroupBox;
        private User_FMSel_NDL_ImportControls user_FMSel_NDL_ImportControls2;
    }
}