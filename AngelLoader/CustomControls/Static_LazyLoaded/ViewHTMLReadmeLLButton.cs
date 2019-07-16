using System.Windows.Forms;
using AngelLoader.Common.DataClasses;
using AngelLoader.Common.Utility;
using AngelLoader.Forms;

namespace AngelLoader.CustomControls.Static_LazyLoaded
{
    internal static class ViewHTMLReadmeLLButton
    {
        private static bool _constructed;
        private static Button Button;

        internal static void Construct(MainForm form, Control container)
        {
            if (_constructed) return;

            Button = new Button();
            container.Controls.Add(Button);
            Button.Anchor = AnchorStyles.None;
            Button.AutoSize = true;
            // This thing gets centered later so no location is specified here
            Button.Padding = new Padding(6, 0, 6, 0);
            Button.Height = 23;
            Button.TabIndex = 49;
            Button.UseVisualStyleBackColor = true;
            Button.Visible = false;
            Button.Click += form.ViewHTMLReadmeButton_Click;

            _constructed = true;

            Localize();
            Button.CenterHV(container);
        }

        internal static void Localize()
        {
            if (_constructed) Button.SetTextAutoSize(LText.ReadmeArea.ViewHTMLReadme);
        }

        internal static void Center(Control parent)
        {
            if (_constructed) Button.CenterHV(parent);
        }

        internal static bool Visible => _constructed && Button.Visible;

        internal static void Hide()
        {
            if (_constructed) Button.Hide();
        }

        internal static void Show() => Button.Show();
    }
}
