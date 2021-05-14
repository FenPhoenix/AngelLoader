using System;
using System.Collections.Generic;
using System.Drawing;
using JetBrains.Annotations;
using static AngelLoader.Misc;

namespace AngelLoader.Forms.CustomControls.Static_LazyLoaded
{
    internal static class AddTagLLDropDown
    {
        internal static bool Constructed { get; private set; }

        internal static DarkListBox ListBox = null!;

        private static bool _darkModeEnabled;
        [PublicAPI]
        internal static bool DarkModeEnabled
        {
            get => _darkModeEnabled;
            set
            {
                if (_darkModeEnabled == value) return;
                _darkModeEnabled = value;

                if (!Constructed) return;

                ListBox.DarkModeEnabled = _darkModeEnabled;
            }
        }

        internal static void Construct(MainForm owner)
        {
            if (Constructed) return;

            ListBox = new DarkListBox { Tag = LazyLoaded.True };
            owner.EverythingPanel.Controls.Add(ListBox);
            ListBox.DarkModeEnabled = _darkModeEnabled;
            ListBox.Scrollable = true;
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

            ListBox.BeginUpdate();
            ListBox.Items.Clear();
            foreach (string item in list) ListBox.Items.Add(item);
            ListBox.EndUpdate();

            Point p = owner.PointToClient(owner.AddTagTextBox.PointToScreen(new Point(0, 0)));
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
