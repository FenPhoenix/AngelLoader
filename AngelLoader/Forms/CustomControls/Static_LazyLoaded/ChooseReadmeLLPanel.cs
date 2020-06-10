using System.Drawing;
using System.Windows.Forms;
using JetBrains.Annotations;
using static AngelLoader.Misc;

namespace AngelLoader.Forms.CustomControls.Static_LazyLoaded
{
    internal static class ChooseReadmeLLPanel
    {
        private static bool _constructed;

        private static Panel? Panel;
        private static ListBoxCustom? _listBox;
        internal static ListBoxCustom ListBox
        {
            get => _listBox!;
            private set => _listBox = value;
        }
        private static FlowLayoutPanel? OKButtonFLP;
        private static Button? OKButton;

        internal static void Construct(MainForm form, Control container)
        {
            if (_constructed) return;

            OKButton = new Button
            {
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                AutoSize = true,
                Margin = new Padding(0),
                Padding = new Padding(6, 0, 6, 0),
                Height = 23,
                TabIndex = 48,
                UseVisualStyleBackColor = true
            };
            OKButton.Click += form.ChooseReadmeButton_Click;

            OKButtonFLP = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.RightToLeft,
                Location = new Point(1, 134),
                Size = new Size(320, 24),
                TabIndex = 3
            };
            OKButtonFLP.Controls.Add(OKButton);

            ListBox = new ListBoxCustom
            {
                FormattingEnabled = true,
                Size = new Size(320, 134),
                TabIndex = 47
            };

            Panel = new Panel
            {
                Anchor = AnchorStyles.None,
                TabIndex = 46,
                Visible = false,
                Size = new Size(324, 161)
            };
            Panel.Controls.Add(ListBox);
            Panel.Controls.Add(OKButtonFLP);

            Panel.CenterHV(container);
            container.Controls.Add(Panel);

            _constructed = true;

            Localize();
        }

        internal static void Localize()
        {
            if (_constructed) OKButton!.SetTextAutoSize(LText.Global.OK, 75);
        }

        [UsedImplicitly] // Actually used in an ifdef block
        internal static void SuspendPanelLayout()
        {
            if (_constructed) Panel!.SuspendLayout();
        }

        internal static void ResumePanelLayout()
        {
            if (_constructed) Panel!.ResumeLayout();
        }

        internal static void ShowPanel(bool value)
        {
            if (_constructed) Panel!.Visible = value;
        }
    }
}
