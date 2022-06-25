using System;
using System.Collections.Generic;
using System.Drawing;
using AngelLoader.Forms.WinFormsNative;
using JetBrains.Annotations;
using static AngelLoader.Misc;

namespace AngelLoader.Forms.CustomControls.LazyLoaded
{
    internal sealed class AddTagLLDropDown
    {
        private bool _constructed;

        private readonly MainForm _owner;

        private DarkListBox _listBox = null!;
        internal DarkListBox ListBox
        {
            get
            {
                Construct();
                return _listBox;
            }
        }

        private bool _darkModeEnabled;
        [PublicAPI]
        internal bool DarkModeEnabled
        {
            set
            {
                if (_darkModeEnabled == value) return;
                _darkModeEnabled = value;

                if (!_constructed) return;

                _listBox.DarkModeEnabled = _darkModeEnabled;
            }
        }

        internal AddTagLLDropDown(MainForm owner) => _owner = owner;

        private void Construct()
        {
            if (_constructed) return;

            _listBox = new DarkListBox
            {
                Tag = LoadType.Lazy,

                Scrollable = true,
                TabIndex = 3,
                Visible = false,

                DarkModeEnabled = _darkModeEnabled
            };

            _listBox.SelectedIndexChanged += _owner.AddTagListBox_SelectedIndexChanged;
            _listBox.KeyDown += _owner.AddTagTextBoxOrListBox_KeyDown;
            _listBox.Leave += _owner.AddTagTextBoxOrListBox_Leave;
            _listBox.MouseUp += _owner.AddTagListBox_MouseUp;

            _owner.EverythingPanel.Controls.Add(_listBox);

            _constructed = true;
        }

        internal void SetItemsAndShow(List<string> list)
        {
            Construct();

            _listBox.BeginUpdate();
            _listBox.Items.Clear();
            foreach (string item in list) _listBox.Items.Add(item);
            _listBox.EndUpdate();

            Point p = _owner.PointToClient_Fast(_owner.AddTagTextBox.PointToScreen_Fast(new Point(0, 0)));
            _listBox.Location = p with { Y = p.Y + _owner.AddTagTextBox.Height };
            _listBox.Size = new Size(Math.Max(_owner.AddTagTextBox.Width, 256), 225);

            _listBox.BringToFront();
            _listBox.Show();
        }

        internal bool Visible => _constructed && _listBox.Visible;
        internal bool Focused => _constructed && _listBox.Focused;

        internal void HideAndClear()
        {
            if (!_constructed) return;

            _listBox.Hide();
            _listBox.Items.Clear();
        }
    }
}
