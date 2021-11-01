using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using AL_Common;
using JetBrains.Annotations;
using static AngelLoader.Misc;

namespace AngelLoader.Forms.CustomControls
{
    public sealed class DarkCheckList : Panel, IDarkable
    {
        public sealed class CheckItem
        {
            public bool Checked;
            public string Text;
            public bool Caution;

            public CheckItem(bool @checked, string text, bool caution)
            {
                Checked = @checked;
                Text = text;
                Caution = caution;
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

        public DarkCheckList()
        {
            base.BackColor = BackColor;
            base.ForeColor = ForeColor;
        }

        [PublicAPI]
        public CheckItem[] CheckItems = Array.Empty<CheckItem>();

        private DarkCheckBox[] CheckBoxes = Array.Empty<DarkCheckBox>();

        [PublicAPI]
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new Color BackColor { get; set; } = SystemColors.Window;
        [PublicAPI]
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
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

                base.BackColor = DarkModeBackColor;
                base.ForeColor = DarkModeForeColor;
            }
            else
            {
                if (_origValuesStored)
                {
                    base.BackColor = (Color)_origBackColor!;
                    base.ForeColor = (Color)_origForeColor!;
                }
            }

            foreach (Control control in base.Controls)
            {
                if (control is IDarkable darkableControl)
                {
                    darkableControl.DarkModeEnabled = _darkModeEnabled;
                }
            }
        }

        internal void ClearList()
        {
            base.Controls.DisposeAndClear();
            CheckBoxes.DisposeAndClear();
            CheckBoxes = Array.Empty<DarkCheckBox>();
            CheckItems = Array.Empty<CheckItem>();
        }

        [PublicAPI]
        [Browsable(true)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        public event EventHandler<DarkCheckListEventArgs>? ItemCheckedChanged;

        internal void FillList(CheckItem[] items)
        {
            ClearList();
            CheckBoxes = new DarkCheckBox[items.Length];

            int x = 18;

            bool firstCautionDone = false;
            bool cautionsExist = false;

            int y = 0;
            int firstCautionY = 0;

            DarkLabel? cautionLabel = null;

            for (int i = 0; i < items.Length; i++, y += 20)
            {
                var item = items[i];

                if (!firstCautionDone && item.Caution)
                {
                    //cautionLabel = new DarkLabel
                    //{
                    //    AutoSize = true,
                    //    Text = "* " + LText.ModsTab.ImportantModsCaution,
                    //    ForeColor = Color.Maroon,
                    //    DarkModeForeColor = DarkColors.Fen_CautionText,
                    //    Location = new Point(x, 4 + y),
                    //    Padding = new Padding(0),
                    //    Margin = new Padding(0)
                    //};
                    //base.Controls.Add(cautionLabel);
                    firstCautionDone = true;
                    //i--;
                    firstCautionY = y;
                    //continue;
                }

                //if (item.Caution)
                //{
                //    cautionsExist = true;
                //    var label = new DarkLabel
                //    {
                //        AutoSize = true,
                //        Text = "*",
                //        //ForeColor = Color.Maroon,
                //        //DarkModeForeColor = DarkColors.Fen_CautionText,
                //        Location = new Point(x - 14, 4 + y),
                //        Padding = new Padding(0),
                //        Margin = new Padding(0)
                //    };
                //    base.Controls.Add(label);
                //}

                var cb = new DarkCheckBox
                {
                    AutoSize = true,
                    Text = item.Text + (item.Caution ? " *" : ""),
                    Location = new Point(x, 4 + y),
                    Checked = item.Checked,
                };
                if (item.Caution)
                {
                    var f = cb.Font;
                    cb.Font = new Font(f.FontFamily, f.Size, FontStyle.Italic, f.Unit, f.GdiCharSet,
                        f.GdiVerticalFont);
                }
                if (firstCautionDone)
                {
                    cb.DarkModeBackColor = DarkColors.Fen_RedHighlight;
                }
                //if (item.Caution)
                //{
                //    cb.ForeColor = Color.Maroon;
                //    cb.DarkModeForeColor = DarkColors.Fen_CautionText;
                //}
                base.Controls.Add(cb);
                CheckBoxes[i] = cb;
                cb.CheckedChanged += OnItemsCheckedChanged;
            }

            //if (cautionsExist)
            if (firstCautionDone)
            {
                var label = new DarkLabel
                {
                    AutoSize = true,
                    Text = "* " + LText.ModsTab.ImportantModsCaution,
                    ForeColor = Color.Maroon,
                    DarkModeForeColor = DarkColors.Fen_CautionText,
                    Location = new Point(4, 8 + y),
                    Padding = new Padding(0),
                    Margin = new Padding(0),
                };
                //var f = label.Font;
                //label.Font = new Font(f.FontFamily, f.Size, FontStyle.Italic, f.Unit, f.GdiCharSet,
                //    f.GdiVerticalFont);
                base.Controls.Add(label);
                cautionLabel?.SendToBack();
                var panel = new Panel
                {
                    Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right,
                    BackColor = DarkColors.Fen_RedHighlight,
                    Location = new Point(4, 4 + firstCautionY),
                    Size = new Size(ClientRectangle.Width - 8, (4 + y) - (4 + firstCautionY))
                };
                base.Controls.Add(panel);
                panel.SendToBack();
            }

            CheckItems = items;

            RefreshDarkMode();
        }

        private void OnItemsCheckedChanged(object sender, EventArgs e)
        {
            var s = (DarkCheckBox)sender;

            int checkBoxIndex = Array.IndexOf(CheckBoxes, s);

            CheckItems[checkBoxIndex].Checked = s.Checked;

            ItemCheckedChanged?.Invoke(this, new DarkCheckListEventArgs(checkBoxIndex, s.Checked, s.Text));
        }

        protected override void OnEnabledChanged(EventArgs e)
        {
            base.OnEnabledChanged(e);

            base.BackColor = _darkModeEnabled ? DarkModeBackColor : Enabled ? BackColor : SystemColors.Control;
        }
    }
}
