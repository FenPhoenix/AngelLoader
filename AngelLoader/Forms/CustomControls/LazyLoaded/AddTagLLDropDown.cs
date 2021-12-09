﻿using System;
using System.Collections.Generic;
using System.Drawing;
using JetBrains.Annotations;
using static AngelLoader.Misc;

namespace AngelLoader.Forms.CustomControls.LazyLoaded
{
    internal sealed class AddTagLLDropDown
    {
        internal bool Constructed { get; private set; }

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
            get => _darkModeEnabled;
            set
            {
                if (_darkModeEnabled == value) return;
                _darkModeEnabled = value;

                if (!Constructed) return;

                _listBox.DarkModeEnabled = _darkModeEnabled;
            }
        }

        internal AddTagLLDropDown(MainForm owner) => _owner = owner;

        private void Construct()
        {
            if (Constructed) return;

            _listBox = new DarkListBox { Tag = LoadType.Lazy };
            _owner.EverythingPanel.Controls.Add(_listBox);
            _listBox.DarkModeEnabled = _darkModeEnabled;
            _listBox.Scrollable = true;
            _listBox.TabIndex = 3;
            _listBox.Visible = false;
            _listBox.SelectedIndexChanged += _owner.AddTagListBox_SelectedIndexChanged;
            _listBox.KeyDown += _owner.AddTagTextBoxOrListBox_KeyDown;
            _listBox.Leave += _owner.AddTagTextBoxOrListBox_Leave;
            _listBox.MouseUp += _owner.AddTagListBox_MouseUp;

            Constructed = true;
        }

        internal void SetItemsAndShow(List<string> list)
        {
            Construct();

            _listBox.BeginUpdate();
            _listBox.Items.Clear();
            foreach (string item in list) _listBox.Items.Add(item);
            _listBox.EndUpdate();

            Point p = _owner.PointToClient(_owner.AddTagTextBox.PointToScreen(new Point(0, 0)));
            _listBox.Location = new Point(p.X, p.Y + _owner.AddTagTextBox.Height);
            _listBox.Size = new Size(Math.Max(_owner.AddTagTextBox.Width, 256), 225);

            _listBox.BringToFront();
            _listBox.Show();
        }

        internal bool Visible => Constructed && _listBox.Visible;
        internal bool Focused => Constructed && _listBox.Focused;

        internal void HideAndClear()
        {
            if (!Constructed) return;

            _listBox.Hide();
            _listBox.Items.Clear();
        }
    }
}
