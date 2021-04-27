using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using AngelLoader.Forms.CustomControls;

namespace AngelLoader.Forms
{
    internal partial class SettingsForm
    {
        private void InitComponentManual()
        {
            components = new Container();
            BottomFlowLayoutPanel = new FlowLayoutPanel();
            Cancel_Button = new DarkButton();
            OKButton = new DarkButton();
            ErrorLabel = new DarkLabel();
            ErrorIconPictureBox = new PictureBox();
            MainToolTip = new ToolTip(components);
            MainSplitContainer = new SplitContainerCustom();
            PagesTreeView = new DarkTreeView();
            PagePanel = new Panel();
            BottomFlowLayoutPanel.SuspendLayout();
            ((ISupportInitialize)ErrorIconPictureBox).BeginInit();
            ((ISupportInitialize)MainSplitContainer).BeginInit();
            MainSplitContainer.Panel1.SuspendLayout();
            MainSplitContainer.Panel2.SuspendLayout();
            MainSplitContainer.SuspendLayout();
            SuspendLayout();
            // 
            // BottomFlowLayoutPanel
            // 
            BottomFlowLayoutPanel.Controls.Add(Cancel_Button);
            BottomFlowLayoutPanel.Controls.Add(OKButton);
            BottomFlowLayoutPanel.Controls.Add(ErrorLabel);
            BottomFlowLayoutPanel.Controls.Add(ErrorIconPictureBox);
            BottomFlowLayoutPanel.Dock = DockStyle.Bottom;
            BottomFlowLayoutPanel.FlowDirection = FlowDirection.RightToLeft;
            BottomFlowLayoutPanel.Location = new Point(0, 616);
            BottomFlowLayoutPanel.Size = new Size(694, 40);
            BottomFlowLayoutPanel.TabIndex = 4;
            // 
            // Cancel_Button
            // 
            Cancel_Button.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            Cancel_Button.AutoSize = true;
            Cancel_Button.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            Cancel_Button.DialogResult = DialogResult.Cancel;
            Cancel_Button.Location = new Point(610, 8);
            Cancel_Button.Margin = new Padding(3, 8, 9, 3);
            Cancel_Button.MinimumSize = new Size(75, 23);
            Cancel_Button.Padding = new Padding(6, 0, 6, 0);
            Cancel_Button.TabIndex = 0;
            Cancel_Button.UseVisualStyleBackColor = true;
            // 
            // OKButton
            // 
            OKButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            OKButton.AutoSize = true;
            OKButton.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            OKButton.DialogResult = DialogResult.OK;
            OKButton.Location = new Point(529, 8);
            OKButton.Margin = new Padding(3, 8, 3, 3);
            OKButton.MinimumSize = new Size(75, 23);
            OKButton.Padding = new Padding(6, 0, 6, 0);
            OKButton.TabIndex = 1;
            OKButton.UseVisualStyleBackColor = true;
            // 
            // ErrorLabel
            // 
            ErrorLabel.AutoSize = true;
            ErrorLabel.ForeColor = SystemColors.ControlText;
            ErrorLabel.Location = new Point(462, 12);
            ErrorLabel.Margin = new Padding(3, 12, 3, 0);
            ErrorLabel.Size = new Size(61, 13);
            ErrorLabel.TabIndex = 4;
            ErrorLabel.Visible = false;
            // 
            // ErrorIconPictureBox
            // 
            ErrorIconPictureBox.Margin = new Padding(0, 12, 0, 0);
            ErrorIconPictureBox.Size = new Size(14, 14);
            ErrorIconPictureBox.TabIndex = 5;
            ErrorIconPictureBox.TabStop = false;
            ErrorIconPictureBox.Visible = false;
            // 
            // MainSplitContainer
            // 
            MainSplitContainer.Anchor = AnchorStyles.Top | AnchorStyles.Bottom
                                                         | AnchorStyles.Left
                                                         | AnchorStyles.Right;
            MainSplitContainer.BackColor = SystemColors.ControlDark;
            MainSplitContainer.Location = new Point(0, 0);
            // 
            // MainSplitContainer.Panel1
            // 
            MainSplitContainer.Panel1.BackColor = SystemColors.Control;
            MainSplitContainer.Panel1.Controls.Add(PagesTreeView);
            // 
            // MainSplitContainer.Panel2
            // 
            MainSplitContainer.Panel2.BackColor = SystemColors.Control;
            MainSplitContainer.Panel2.Controls.Add(PagePanel);
            MainSplitContainer.Size = new Size(694, 613);
            MainSplitContainer.SplitterDistance = 155;
            MainSplitContainer.TabIndex = 5;
            // 
            // PagesTreeView
            // 
            PagesTreeView.Dock = DockStyle.Fill;
            PagesTreeView.HideSelection = false;
            PagesTreeView.Location = new Point(0, 0);
            PagesTreeView.TabIndex = 0;
            PagesTreeView.AfterSelect += PagesTreeView_AfterSelect;
            // 
            // PagePanel
            // 
            PagePanel.Dock = DockStyle.Fill;
            PagePanel.Location = new Point(0, 0);
            PagePanel.Size = new Size(535, 613);
            PagePanel.TabIndex = 2;
            // 
            // SettingsForm
            // 
            AcceptButton = OKButton;
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(694, 656);
            Controls.Add(MainSplitContainer);
            Controls.Add(BottomFlowLayoutPanel);
            DoubleBuffered = true;
            Icon = AL_Icon.AngelLoader;
            KeyPreview = true;
            MaximizeBox = false;
            MinimizeBox = false;
            MinimumSize = new Size(540, 320);
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            // Un-obvious hack: If we DON'T set Text to something, anything, here, then first render (if paths tab
            // is the startup tab) is really slow. We just set a one-char blank space to prevent that(?!) Probably
            // something to do with this activating some kind of render routine beforehand... I guess... who knows...
            Text = " ";
            FormClosing += SettingsForm_FormClosing;
            Load += SettingsForm_Load;
            Shown += SettingsForm_Shown;
            KeyDown += SettingsForm_KeyDown;
            BottomFlowLayoutPanel.ResumeLayout(false);
            BottomFlowLayoutPanel.PerformLayout();
            ((ISupportInitialize)ErrorIconPictureBox).EndInit();
            MainSplitContainer.Panel1.ResumeLayout(false);
            MainSplitContainer.Panel2.ResumeLayout(false);
            ((ISupportInitialize)MainSplitContainer).EndInit();
            MainSplitContainer.ResumeLayout(false);
            ResumeLayout(false);
        }
    }
}
