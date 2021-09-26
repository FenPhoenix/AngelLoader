using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using JetBrains.Annotations;

namespace AngelLoader.Forms.CustomControls
{
    public sealed class DarkCheckList : Panel, IDarkable
    {
        public sealed class CheckItem
        {
            public bool Checked;
            public string Text;

            public CheckItem(bool @checked, string text)
            {
                Checked = @checked;
                Text = text;
            }
        }

        public sealed class DarkCheckListEventArgs : EventArgs
        {
            public readonly int Index;
            public readonly bool Checked;
            public readonly string Text;

            public DarkCheckListEventArgs(int index, bool @checked, string text)
            {
                Index = index;
                Checked = @checked;
                Text = text;
            }
        }

        private bool _origValuesStored;
        private Color? _origBackColor;
        private Color? _origForeColor;

        [PublicAPI]
        public CheckItem[] CheckItems = Array.Empty<CheckItem>();

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new bool Controls { get; set; }

        [PublicAPI]
        public new Color BackColor { get; set; } = SystemColors.Control;
        [PublicAPI]
        public new Color ForeColor { get; set; } = SystemColors.ControlText;

        [PublicAPI]
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Color DarkModeBackColor { get; set; } = DarkColors.Fen_ControlBackground;
        [PublicAPI]
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Color DarkModeForeColor { get; set; } = DarkColors.LightText;

        private bool _darkModeEnabled;
        [PublicAPI]
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool DarkModeEnabled
        {
            get => _darkModeEnabled;
            set
            {
                if (_darkModeEnabled == value) return;
                _darkModeEnabled = value;

                RefreshDarkMode();
            }
        }

        internal void RefreshDarkMode()
        {
            if (_darkModeEnabled)
            {
                if (!_origValuesStored)
                {
                    _origBackColor = BackColor;
                    _origForeColor = ForeColor;
                    _origValuesStored = true;
                }

                BackColor = DarkModeBackColor;
                ForeColor = DarkModeForeColor;
            }
            else
            {
                if (_origValuesStored)
                {
                    BackColor = (Color)_origBackColor!;
                    ForeColor = (Color)_origForeColor!;
                }
            }

            foreach (Control control in base.Controls)
            {
                if (control is DarkCheckBox and IDarkable darkableControl)
                {
                    darkableControl.DarkModeEnabled = _darkModeEnabled;
                }
            }
        }

        internal void ClearList()
        {
            base.Controls.DisposeAndClear();
            CheckItems = Array.Empty<CheckItem>();
        }

        [PublicAPI]
        [Browsable(true)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        public event EventHandler<DarkCheckListEventArgs>? ItemCheckedChanged;

        internal void FillList(CheckItem[] items)
        {
            ClearList();

            for (int i = 0, y = 0; i < items.Length; i++, y += 20)
            {
                var item = items[i];
                var cb = new DarkCheckBox
                {
                    Text = item.Text,
                    Location = new Point(4, y),
                    Checked = item.Checked
                };
                base.Controls.Add(cb);
                cb.CheckedChanged += OnItemsCheckedChanged;
            }

            CheckItems = items;

            RefreshDarkMode();
        }

        private void OnItemsCheckedChanged(object sender, EventArgs e)
        {
            var s = (DarkCheckBox)sender;
            ItemCheckedChanged?.Invoke(this, new DarkCheckListEventArgs(base.Controls.IndexOf(s), s.Checked, s.Text));
        }
    }
}
