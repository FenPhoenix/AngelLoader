using System.Drawing;
using System.Windows.Forms;
using JetBrains.Annotations;
using static AngelLoader.Misc;

namespace AngelLoader.Forms.CustomControls.Static_LazyLoaded
{
    internal static class ChooseReadmeLLPanel
    {
        private static bool _constructed;

        private static Panel Panel = null!;
        internal static ListBoxCustom ListBox = null!;

        private static FlowLayoutPanel OKButtonFLP = null!;
        private static DarkButton OKButton = null!;

        private static bool _darkModeEnabled;
        internal static bool DarkModeEnabled
        {
            get => _darkModeEnabled;
            set
            {
                if (_darkModeEnabled == value) return;
                _darkModeEnabled = value;

                if (!_constructed) return;

                Panel!.BackColor = _darkModeEnabled ? DarkColors.Fen_DarkBackground : SystemColors.Control;
                ListBox!.DarkModeEnabled = _darkModeEnabled;
                OKButtonFLP.BackColor = _darkModeEnabled ? DarkColors.Fen_DarkBackground : SystemColors.Control;
                OKButton!.DarkModeEnabled = _darkModeEnabled;
            }
        }

        internal static void Construct(MainForm form, Control container)
        {
            if (_constructed) return;

            OKButton = new DarkButton
            {
                Tag = LazyLoaded.True,
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                AutoSize = true,
                Margin = new Padding(0),
                Padding = new Padding(6, 0, 6, 0),
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                MinimumSize = new Size(75, 23),
                TabIndex = 48,
                UseVisualStyleBackColor = true,
                DarkModeEnabled = _darkModeEnabled
            };
            OKButton.Click += form.ChooseReadmeButton_Click;

            OKButtonFLP = new FlowLayoutPanel
            {
                Tag = LazyLoaded.True,
                FlowDirection = FlowDirection.RightToLeft,
                Location = new Point(1, 134),
                Size = new Size(320, 24),
                TabIndex = 3,
                BackColor = _darkModeEnabled ? DarkColors.Fen_DarkBackground : SystemColors.Control
            };
            OKButtonFLP.Controls.Add(OKButton);

            ListBox = new ListBoxCustom
            {
                Tag = LazyLoaded.True,
                MultiSelect = false,
                Size = new Size(320, 134),
                TabIndex = 47,
                DarkModeEnabled = _darkModeEnabled
            };

            Panel = new Panel
            {
                Tag = LazyLoaded.True,
                Anchor = AnchorStyles.None,
                TabIndex = 46,
                Visible = false,
                Size = new Size(324, 161),
                BackColor = _darkModeEnabled ? DarkColors.Fen_DarkBackground : SystemColors.Control
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
            if (_constructed) OKButton.Text = LText.Global.OK;
        }

        [UsedImplicitly] // Actually used in an ifdef block
        internal static void SuspendPanelLayout()
        {
            if (_constructed) Panel.SuspendLayout();
        }

        internal static void ResumePanelLayout()
        {
            if (_constructed) Panel.ResumeLayout();
        }

        internal static void ShowPanel(bool value)
        {
            if (_constructed) Panel.Visible = value;
        }
    }
}
