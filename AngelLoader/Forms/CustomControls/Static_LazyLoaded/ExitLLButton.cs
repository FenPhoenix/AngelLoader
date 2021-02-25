using System.Drawing;
using System.Windows.Forms;
using JetBrains.Annotations;
using static AngelLoader.Misc;

namespace AngelLoader.Forms.CustomControls.Static_LazyLoaded
{
    internal static class ExitLLButton
    {
        private static bool _constructed;

        private static DarkButton Button = null!;

        private static bool _darkModeEnabled;
        [PublicAPI]
        public static bool DarkModeEnabled
        {
            get => _darkModeEnabled;
            set
            {
                if (_darkModeEnabled == value) return;
                _darkModeEnabled = value;
                if (!_constructed) return;

                Button!.DarkModeEnabled = value;
            }
        }

        internal static void Localize()
        {
            if (!_constructed) return;
            Button.Text = LText.Global.Exit;
        }

        internal static void SetVisible(MainForm owner, bool enabled)
        {
            if (enabled)
            {
                if (!_constructed)
                {
                    var container = owner.BottomRightButtonsFLP;

                    Button = new DarkButton();

                    container.Controls.Add(Button);
                    container.Controls.SetChildIndex(Button, 0);

                    Button.AutoSize = true;
                    Button.AutoSizeMode = AutoSizeMode.GrowAndShrink;
                    Button.DarkModeEnabled = _darkModeEnabled;
                    Button.MinimumSize = new Size(36, 36);
                    Button.TabIndex = 63;
                    Button.UseVisualStyleBackColor = true;
                    Button.Click += (_, _) => owner.Close();

                    _constructed = true;

                    Localize();
                }

                Button.Show();
            }
            else
            {
                if (_constructed) Button.Hide();
            }
        }
    }
}
