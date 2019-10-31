using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using AngelLoader.Forms;

namespace AngelLoader.CustomControls.Static_LazyLoaded
{
    internal static class AddTagLLDropDown
    {
        internal static bool Constructed { get; private set; }

        private static ListBox? _listBox;
        internal static ListBox ListBox
        {
            get => _listBox!;
            private set => _listBox = value;
        }

        internal static void Construct(MainForm owner)
        {
            if (Constructed) return;

            ListBox = new ListBox();
            owner.EverythingPanel.Controls.Add(ListBox);
            ListBox.FormattingEnabled = true;
            ListBox.TabIndex = 3;
            ListBox.Visible = false;
            ListBox.SelectedIndexChanged += owner.AddTagListBox_SelectedIndexChanged;
            ListBox.KeyDown += owner.AddTagTextBoxOrListBox_KeyDown;
            ListBox.Leave += owner.AddTagTextBoxOrListBox_Leave;
            ListBox.MouseUp += owner.AddTagListBox_MouseUp;

            Constructed = true;
        }

        internal static void SetItemsAndShow(MainForm owner, List<string> list)
        {
            Construct(owner);
            
            ListBox.Items.Clear();
            foreach (var item in list) ListBox.Items.Add(item);

            var p = owner.PointToClient(owner.AddTagTextBox.PointToScreen(new Point(0, 0)));
            ListBox.Location = new Point(p.X, p.Y + owner.AddTagTextBox.Height);
            ListBox.Size = new Size(Math.Max(owner.AddTagTextBox.Width, 256), 225);

            ListBox.BringToFront();
            ListBox.Show();
        }

        internal static bool Visible => Constructed && ListBox.Visible;
        internal static bool Focused => Constructed && ListBox.Focused;

        internal static void HideAndClear()
        {
            if (!Constructed) return;

            ListBox.Hide();
            ListBox.Items.Clear();
        }
    }
}
