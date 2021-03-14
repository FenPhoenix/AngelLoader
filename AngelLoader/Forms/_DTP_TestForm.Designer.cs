
namespace AngelLoader.Forms
{
    partial class _DTP_TestForm
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
            this.button1 = new System.Windows.Forms.Button();
            this.darkDateTimePicker21 = new AngelLoader.Forms.CustomControls.DarkDateTimePicker2();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(680, 296);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 2;
            this.button1.Text = "button1";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // darkDateTimePicker21
            // 
            this.darkDateTimePicker21.Location = new System.Drawing.Point(272, 208);
            this.darkDateTimePicker21.MaximumSize = new System.Drawing.Size(65535, 20);
            this.darkDateTimePicker21.MinimumSize = new System.Drawing.Size(0, 20);
            this.darkDateTimePicker21.Name = "darkDateTimePicker21";
            this.darkDateTimePicker21.Size = new System.Drawing.Size(163, 20);
            this.darkDateTimePicker21.TabIndex = 0;
            // 
            // _DTP_TestForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.darkDateTimePicker21);
            this.Name = "_DTP_TestForm";
            this.Text = "_DTP_TestForm";
            this.ResumeLayout(false);

        }

        #endregion

        private CustomControls.DarkDateTimePicker2 darkDateTimePicker21;
        private System.Windows.Forms.Button button1;
    }
}