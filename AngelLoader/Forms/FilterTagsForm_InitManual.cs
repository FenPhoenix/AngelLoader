using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using AngelLoader.Forms.CustomControls;

namespace AngelLoader.Forms
{
    public sealed partial class FilterTagsForm
    {
        private void InitComponentManual()
        {
            components = new Container();
            OriginTreeView = new DarkTreeView();
            AndTreeView = new DarkTreeView();
            OrTreeView = new DarkTreeView();
            NotTreeView = new DarkTreeView();
            AndButton = new DarkButton();
            OrButton = new DarkButton();
            NotButton = new DarkButton();
            IncludeAllLabel = new DarkLabel();
            IncludeAnyLabel = new DarkLabel();
            ExcludeLabel = new DarkLabel();
            FilterLabelsPanel = new Panel();
            RemoveAllNotButton = new DarkButton();
            RemoveAllOrButton = new DarkButton();
            RemoveAllAndButton = new DarkButton();
            RemoveSelectedNotButton = new DarkButton();
            RemoveSelectedOrButton = new DarkButton();
            RemoveSelectedAndButton = new DarkButton();
            OKButton = new DarkButton();
            Cancel_Button = new DarkButton();
            ResetButton = new DarkButton();
            BottomButtonsFLP = new FlowLayoutPanel();
            MoveButtonsPanel = new Panel();
            MainToolTip = new ToolTip(components);
            FindTagTextBox = new DarkTextBox();
            FilterLabelsPanel.SuspendLayout();
            BottomButtonsFLP.SuspendLayout();
            MoveButtonsPanel.SuspendLayout();
            SuspendLayout();
            // 
            // OriginTreeView
            // 
            OriginTreeView.AlwaysDrawNodesFocused = true;
            OriginTreeView.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            OriginTreeView.HideSelection = false;
            OriginTreeView.Location = new Point(8, 32);
            OriginTreeView.Size = new Size(224, 624);
            OriginTreeView.TabIndex = 0;
            OriginTreeView.AfterSelect += OriginTreeView_AfterSelect;
            // 
            // AndTreeView
            // 
            AndTreeView.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            AndTreeView.HideSelection = false;
            AndTreeView.Location = new Point(344, 24);
            AndTreeView.Size = new Size(224, 632);
            AndTreeView.TabIndex = 3;
            // 
            // OrTreeView
            // 
            OrTreeView.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            OrTreeView.HideSelection = false;
            OrTreeView.Location = new Point(576, 24);
            OrTreeView.Size = new Size(224, 632);
            OrTreeView.TabIndex = 4;
            // 
            // NotTreeView
            // 
            NotTreeView.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            NotTreeView.HideSelection = false;
            NotTreeView.Location = new Point(808, 24);
            NotTreeView.Size = new Size(224, 632);
            NotTreeView.TabIndex = 5;
            // 
            // AndButton
            // 
            AndButton.AutoSize = true;
            AndButton.MinimumSize = new Size(80, 23);
            AndButton.Location = new Point(16, 0);
            AndButton.Padding = new Padding(7, 0, 0, 0);
            AndButton.TabIndex = 0;
            AndButton.TextAlign = ContentAlignment.MiddleLeft;
            AndButton.UseVisualStyleBackColor = true;
            AndButton.Click += AddTagsButtons_Click;
            AndButton.PaintCustom += ArrowButtons_Paint;
            // 
            // OrButton
            // 
            OrButton.AutoSize = true;
            OrButton.MinimumSize = new Size(80, 23);
            OrButton.Location = new Point(16, 23);
            OrButton.Padding = new Padding(7, 0, 0, 0);
            OrButton.TabIndex = 1;
            OrButton.TextAlign = ContentAlignment.MiddleLeft;
            OrButton.UseVisualStyleBackColor = true;
            OrButton.Click += AddTagsButtons_Click;
            OrButton.PaintCustom += ArrowButtons_Paint;
            // 
            // NotButton
            // 
            NotButton.AutoSize = true;
            NotButton.MinimumSize = new Size(80, 23);
            NotButton.Location = new Point(16, 46);
            NotButton.Padding = new Padding(7, 0, 0, 0);
            NotButton.TabIndex = 2;
            NotButton.TextAlign = ContentAlignment.MiddleLeft;
            NotButton.UseVisualStyleBackColor = true;
            NotButton.Click += AddTagsButtons_Click;
            NotButton.PaintCustom += ArrowButtons_Paint;
            // 
            // IncludeAllLabel
            // 
            IncludeAllLabel.AutoSize = true;
            IncludeAllLabel.Location = new Point(0, 8);
            IncludeAllLabel.TabIndex = 0;
            // 
            // IncludeAnyLabel
            // 
            IncludeAnyLabel.AutoSize = true;
            IncludeAnyLabel.Location = new Point(232, 8);
            IncludeAnyLabel.TabIndex = 3;
            // 
            // ExcludeLabel
            // 
            ExcludeLabel.AutoSize = true;
            ExcludeLabel.Location = new Point(464, 8);
            ExcludeLabel.TabIndex = 6;
            // 
            // FilterLabelsPanel
            // 
            FilterLabelsPanel.Controls.Add(RemoveAllNotButton);
            FilterLabelsPanel.Controls.Add(RemoveAllOrButton);
            FilterLabelsPanel.Controls.Add(RemoveAllAndButton);
            FilterLabelsPanel.Controls.Add(IncludeAllLabel);
            FilterLabelsPanel.Controls.Add(ExcludeLabel);
            FilterLabelsPanel.Controls.Add(RemoveSelectedNotButton);
            FilterLabelsPanel.Controls.Add(RemoveSelectedOrButton);
            FilterLabelsPanel.Controls.Add(RemoveSelectedAndButton);
            FilterLabelsPanel.Controls.Add(IncludeAnyLabel);
            FilterLabelsPanel.Location = new Point(344, 0);
            FilterLabelsPanel.Size = new Size(688, 24);
            FilterLabelsPanel.TabIndex = 2;
            // 
            // RemoveAllNotButton
            // 
            RemoveAllNotButton.Location = new Point(666, 0);
            RemoveAllNotButton.Size = new Size(23, 23);
            RemoveAllNotButton.TabIndex = 8;
            RemoveAllNotButton.UseVisualStyleBackColor = true;
            RemoveAllNotButton.Click += RemoveAllButtons_Click;
            RemoveAllNotButton.PaintCustom += RemoveAllButtons_Paint;
            // 
            // RemoveAllOrButton
            // 
            RemoveAllOrButton.Location = new Point(434, 0);
            RemoveAllOrButton.Size = new Size(23, 23);
            RemoveAllOrButton.TabIndex = 5;
            RemoveAllOrButton.UseVisualStyleBackColor = true;
            RemoveAllOrButton.Click += RemoveAllButtons_Click;
            RemoveAllOrButton.PaintCustom += RemoveAllButtons_Paint;
            // 
            // RemoveAllAndButton
            // 
            RemoveAllAndButton.Location = new Point(202, 0);
            RemoveAllAndButton.Size = new Size(23, 23);
            RemoveAllAndButton.TabIndex = 2;
            RemoveAllAndButton.UseVisualStyleBackColor = true;
            RemoveAllAndButton.Click += RemoveAllButtons_Click;
            RemoveAllAndButton.PaintCustom += RemoveAllButtons_Paint;
            // 
            // RemoveSelectedNotButton
            // 
            RemoveSelectedNotButton.Location = new Point(643, 0);
            RemoveSelectedNotButton.Size = new Size(23, 23);
            RemoveSelectedNotButton.TabIndex = 7;
            RemoveSelectedNotButton.UseVisualStyleBackColor = true;
            RemoveSelectedNotButton.Click += RemoveSelectedButtons_Click;
            RemoveSelectedNotButton.PaintCustom += RemoveButtons_Paint;
            // 
            // RemoveSelectedOrButton
            // 
            RemoveSelectedOrButton.Location = new Point(411, 0);
            RemoveSelectedOrButton.Size = new Size(23, 23);
            RemoveSelectedOrButton.TabIndex = 4;
            RemoveSelectedOrButton.UseVisualStyleBackColor = true;
            RemoveSelectedOrButton.Click += RemoveSelectedButtons_Click;
            RemoveSelectedOrButton.PaintCustom += RemoveButtons_Paint;
            // 
            // RemoveSelectedAndButton
            // 
            RemoveSelectedAndButton.Location = new Point(179, 0);
            RemoveSelectedAndButton.Size = new Size(23, 23);
            RemoveSelectedAndButton.TabIndex = 1;
            RemoveSelectedAndButton.UseVisualStyleBackColor = true;
            RemoveSelectedAndButton.Click += RemoveSelectedButtons_Click;
            RemoveSelectedAndButton.PaintCustom += RemoveButtons_Paint;
            // 
            // OKButton
            // 
            OKButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            OKButton.AutoSize = true;
            OKButton.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            OKButton.Margin = new Padding(13, 3, 3, 3);
            OKButton.MinimumSize = new Size(75, 23);
            OKButton.DialogResult = DialogResult.OK;
            OKButton.Padding = new Padding(6, 0, 6, 0);
            OKButton.TabIndex = 2;
            OKButton.UseVisualStyleBackColor = true;
            // 
            // Cancel_Button
            // 
            Cancel_Button.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            Cancel_Button.AutoSize = true;
            Cancel_Button.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            Cancel_Button.MinimumSize = new Size(75, 23);
            Cancel_Button.DialogResult = DialogResult.Cancel;
            Cancel_Button.Margin = new Padding(3, 3, 7, 3);
            Cancel_Button.Padding = new Padding(6, 0, 6, 0);
            Cancel_Button.TabIndex = 3;
            Cancel_Button.UseVisualStyleBackColor = true;
            // 
            // ResetButton
            // 
            ResetButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            ResetButton.AutoSize = true;
            ResetButton.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            ResetButton.MinimumSize = new Size(75, 23);
            ResetButton.Margin = new Padding(3, 3, 0, 3);
            ResetButton.Padding = new Padding(6, 0, 6, 0);
            ResetButton.TabIndex = 0;
            ResetButton.UseVisualStyleBackColor = true;
            ResetButton.Click += ResetButton_Click;
            // 
            // BottomButtonsFLP
            // 
            BottomButtonsFLP.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            BottomButtonsFLP.Controls.Add(Cancel_Button);
            BottomButtonsFLP.Controls.Add(OKButton);
            BottomButtonsFLP.Controls.Add(ResetButton);
            BottomButtonsFLP.FlowDirection = FlowDirection.RightToLeft;
            BottomButtonsFLP.Location = new Point(0, 660);
            BottomButtonsFLP.Size = new Size(1040, 32);
            BottomButtonsFLP.TabIndex = 6;
            BottomButtonsFLP.Paint += BottomButtonsFLP_Paint;
            // 
            // MoveButtonsPanel
            // 
            MoveButtonsPanel.Controls.Add(AndButton);
            MoveButtonsPanel.Controls.Add(OrButton);
            MoveButtonsPanel.Controls.Add(NotButton);
            MoveButtonsPanel.Location = new Point(232, 104);
            MoveButtonsPanel.Size = new Size(112, 552);
            MoveButtonsPanel.TabIndex = 1;
            // 
            // FindTagTextBox
            // 
            FindTagTextBox.Location = new Point(8, 8);
            FindTagTextBox.Size = new Size(224, 20);
            FindTagTextBox.TabIndex = 7;
            FindTagTextBox.TextChanged += FindTagTextBox_TextChanged;
            // 
            // FilterTagsForm
            // 
            AcceptButton = OKButton;
            AutoScaleDimensions = new SizeF(6F, 13F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = Cancel_Button;
            ClientSize = new Size(1040, 692);
            Controls.Add(FindTagTextBox);
            Controls.Add(MoveButtonsPanel);
            Controls.Add(BottomButtonsFLP);
            Controls.Add(FilterLabelsPanel);
            Controls.Add(NotTreeView);
            Controls.Add(OrTreeView);
            Controls.Add(AndTreeView);
            Controls.Add(OriginTreeView);
            FormBorderStyle = FormBorderStyle.SizableToolWindow;
            Icon = AL_Icon.AngelLoader;
            MaximizeBox = false;
            MaximumSize = new Size(1056, 32767);
            MinimizeBox = false;
            MinimumSize = new Size(1056, 242);
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            FilterLabelsPanel.ResumeLayout(false);
            FilterLabelsPanel.PerformLayout();
            BottomButtonsFLP.ResumeLayout(false);
            BottomButtonsFLP.PerformLayout();
            MoveButtonsPanel.ResumeLayout(false);
            MoveButtonsPanel.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }
    }
}
