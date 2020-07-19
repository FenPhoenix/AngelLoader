using System.Windows.Forms;

namespace AngelLoader.Forms.CustomControls
{
    public partial class ProgressPanel
    {
        private void InitComponentManual()
        {
            ProgressCancelButton = new Button();
            ProgressPercentLabel = new Label();
            ProgressMessageLabel = new Label();
            CurrentThingLabel = new Label();
            ProgressBar = new ProgressBar();
            SuspendLayout();
            // 
            // ProgressCancelButton
            // 
            ProgressCancelButton.AutoSize = true;
            ProgressCancelButton.Location = new System.Drawing.Point(168, 88);
            ProgressCancelButton.Padding = new Padding(6, 0, 6, 0);
            ProgressCancelButton.Size = new System.Drawing.Size(88, 23);
            ProgressCancelButton.TabIndex = 4;
            ProgressCancelButton.UseVisualStyleBackColor = true;
            ProgressCancelButton.Click += ProgressCancelButton_Click;
            // 
            // ProgressPercentLabel
            // 
            ProgressPercentLabel.Location = new System.Drawing.Point(1, 40);
            ProgressPercentLabel.Size = new System.Drawing.Size(422, 13);
            ProgressPercentLabel.TabIndex = 2;
            ProgressPercentLabel.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // ProgressMessageLabel
            // 
            ProgressMessageLabel.Location = new System.Drawing.Point(1, 8);
            ProgressMessageLabel.Size = new System.Drawing.Size(422, 13);
            ProgressMessageLabel.TabIndex = 0;
            ProgressMessageLabel.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // CurrentThingLabel
            // 
            CurrentThingLabel.Location = new System.Drawing.Point(1, 24);
            CurrentThingLabel.Size = new System.Drawing.Size(422, 13);
            CurrentThingLabel.TabIndex = 1;
            CurrentThingLabel.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // ProgressBar
            // 
            ProgressBar.Location = new System.Drawing.Point(8, 56);
            ProgressBar.Size = new System.Drawing.Size(408, 23);
            ProgressBar.TabIndex = 3;
            // 
            // ProgressPanel
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            AutoScaleMode = AutoScaleMode.Font;
            BorderStyle = BorderStyle.FixedSingle;
            Controls.Add(ProgressCancelButton);
            Controls.Add(ProgressPercentLabel);
            Controls.Add(ProgressMessageLabel);
            Controls.Add(ProgressBar);
            Controls.Add(CurrentThingLabel);
            Name = "ProgressPanel";
            Size = new System.Drawing.Size(424, 128);
            ResumeLayout(false);
            PerformLayout();
        }
    }
}
