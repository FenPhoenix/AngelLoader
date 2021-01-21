using System.Drawing;
using System.Windows.Forms;
using static AngelLoader.Misc;

namespace AngelLoader.Forms.CustomControls.Static_LazyLoaded
{
    internal static class ExitLLButton
    {
        private static bool _constructed;

        private static Button Button = null!;

        internal static void Localize()
        {
            if (_constructed) Button.Text = LText.MainMenu.Exit;
        }

        internal static void SetVisible(MainForm owner, bool enabled)
        {
            if (enabled)
            {
                if (!_constructed)
                {
                    var container = owner.BottomRightButtonsFLP;

                    Button = new Button();

                    container.Controls.Add(Button);
                    container.Controls.SetChildIndex(Button, 0);

                    Button.AutoSize = true;
                    Button.AutoSizeMode = AutoSizeMode.GrowAndShrink;
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
