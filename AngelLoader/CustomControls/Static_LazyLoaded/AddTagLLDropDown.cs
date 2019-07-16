using System.Windows.Forms;
using AngelLoader.Forms;

namespace AngelLoader.CustomControls.Static_LazyLoaded
{
    internal static class AddTagLLDropDown
    {
        internal static bool Constructed { get; private set; }

        internal static ListBox ListBox;

        internal static void Construct(MainForm form, Control container)
        {
            if (Constructed) return;

            ListBox = new ListBox();
            container.Controls.Add(ListBox);
            ListBox.FormattingEnabled = true;
            ListBox.TabIndex = 3;
            ListBox.Visible = false;
            ListBox.SelectedIndexChanged += form.AddTagListBox_SelectedIndexChanged;
            ListBox.KeyDown += form.AddTagTextBoxOrListBox_KeyDown;
            ListBox.Leave += form.AddTagTextBoxOrListBox_Leave;
            ListBox.MouseUp += form.AddTagListBox_MouseUp;

            Constructed = true;
        }

        internal static bool Visible => Constructed && ListBox.Visible;
        internal static bool Focused => Constructed && ListBox.Focused;

        internal static void Hide()
        {
            if (Constructed) ListBox.Hide();
        }

        internal static void Clear()
        {
            if (Constructed) ListBox.Items.Clear();
        }
    }
}
