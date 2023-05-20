using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using AL_Common;
using AngelLoader.DataClasses;
using JetBrains.Annotations;

namespace AngelLoader.Forms.CustomControls;

public sealed class DarkCheckList : Panel, IDarkable, IEventDisabler
{
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public int EventsDisabled { get; set; }

    #region Private fields

    private Func<bool>? _predicate;

    private DarkLabel? _cautionLabel;
    private DarkLabel CautionLabel
    {
        get
        {
            if (_cautionLabel == null)
            {
                _cautionLabel = new DarkLabel
                {
                    Tag = ItemType.Caution,
                    AutoSize = true,
                    ForeColor = Color.Maroon,
                    DarkModeForeColor = DarkColors.Fen_CautionText,
                };
                Controls.Add(_cautionLabel);
                _cautionLabel.DarkModeEnabled = _darkModeEnabled;
            }

            return _cautionLabel;
        }
    }

    private Rectangle _cautionRectangle = Rectangle.Empty;

    private bool _origValuesStored;
    private Color? _origBackColor;
    private Color? _origForeColor;

    private DarkCheckBox[] _checkBoxes = Array.Empty<DarkCheckBox>();

    private enum ItemType { Caution }

    #endregion

    #region Public classes

    [PublicAPI]
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

    #endregion

    #region Public fields and properties

    [PublicAPI]
    public CheckItem[] CheckItems = Array.Empty<CheckItem>();

    [PublicAPI]
    public new Color BackColor = SystemColors.Window;

    [PublicAPI]
    public new Color ForeColor = SystemColors.ControlText;

    [PublicAPI]
    public Color DarkModeBackColor = DarkColors.Fen_ControlBackground;

    [PublicAPI]
    public Color DarkModeForeColor = DarkColors.LightText;

    private bool _darkModeEnabled;
    [PublicAPI]
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool DarkModeEnabled
    {
        set
        {
            if (_darkModeEnabled == value) return;
            _darkModeEnabled = value;

            RefreshDarkMode();
        }
    }

    #endregion

    #region Public events

    [PublicAPI]
    [Browsable(true)]
    [EditorBrowsable(EditorBrowsableState.Always)]
    public event EventHandler? ItemCheckedChanged;

    #endregion

    public DarkCheckList()
    {
        base.BackColor = BackColor;
        base.ForeColor = ForeColor;
    }

    #region Private methods

    private void RefreshDarkMode()
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

        foreach (Control control in Controls)
        {
            if (control is IDarkable darkableControl)
            {
                darkableControl.DarkModeEnabled = _darkModeEnabled;
            }
        }
    }

    #endregion

    #region Public methods

    internal static bool IsControlCaution(Control control) => control.Tag is ItemType.Caution;

    internal void SoftClearList()
    {
        foreach (DarkCheckBox cb in _checkBoxes)
        {
            cb.Hide();
        }

        _cautionLabel?.Hide();
        _cautionRectangle = Rectangle.Empty;
    }

    internal void SetList(CheckItem[] items, string cautionText)
    {
        const int x = 18;

        bool firstCautionDone = false;

        int y = 0;
        int firstCautionY = 0;

        for (int i = 0; i < items.Length; i++, y += 20)
        {
            CheckItem item = items[i];

            if (!firstCautionDone && item.Caution)
            {
                firstCautionDone = true;
                firstCautionY = y;
            }

            DarkCheckBox cb = _checkBoxes[i];
            cb.Text = item.Text + (item.Caution ? " *" : "");
            cb.Location = new Point(x, 4 + y);
            using (new DisableEvents(this))
            {
                cb.Checked = item.Checked;
            }

            if (item.Caution)
            {
                cb.Tag = ItemType.Caution;
                cb.Visible = _predicate?.Invoke() ?? true;
                cb.SetFontStyle(FontStyle.Italic);
                cb.BackColor = Color.MistyRose;
                cb.DarkModeBackColor = DarkColors.Fen_RedHighlight;
            }
            else
            {
                cb.Tag = null;
                cb.Visible = true;
                cb.SetFontStyle(FontStyle.Regular);
                cb.BackColor = SystemColors.Window;
                cb.DarkModeBackColor = null;
            }
        }

        for (int i = items.Length; i < _checkBoxes.Length; i++)
        {
            DarkCheckBox cb = _checkBoxes[i];
            cb.Visible = false;
            cb.Tag = null;
        }

        if (firstCautionDone)
        {
            CautionLabel.Visible = _predicate?.Invoke() ?? true;
            CautionLabel.Location = new Point(4, 8 + y);

            RefreshCautionLabelText(cautionText);

            _cautionRectangle = new Rectangle(
                4,
                4 + firstCautionY,
                0, // Width will be set on draw for manual "anchoring"
                (4 + y) - (4 + firstCautionY)
            );
        }
        else
        {
            _cautionRectangle = Rectangle.Empty;
        }

        CheckItems = items;
    }

    internal void RecreateList(int maxCheckBoxCount)
    {
        try
        {
            SuspendLayout();

            Controls.DisposeAndClear();
            _checkBoxes.DisposeAll();
            CheckItems = Array.Empty<CheckItem>();

            _checkBoxes = new DarkCheckBox[maxCheckBoxCount];
            for (int i = 0; i < _checkBoxes.Length; i++)
            {
                DarkCheckBox cb = new()
                {
                    AutoSize = true
                };
                _checkBoxes[i] = cb;
                Controls.Add(cb);
                cb.CheckedChanged += OnItemsCheckedChanged;
            }

            _cautionLabel = null;

            RefreshDarkMode();
        }
        finally
        {
            ResumeLayout(true);
        }
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        if (_cautionRectangle != Rectangle.Empty && (_predicate?.Invoke() ?? true))
        {
            _cautionRectangle.Width = ClientRectangle.Width - 8;
            e.Graphics.FillRectangle(_darkModeEnabled ? DarkColors.Fen_RedHighlightBrush : Brushes.MistyRose, _cautionRectangle);
        }
        base.OnPaint(e);
    }

    internal void SetCautionVisiblePredicate(Func<bool> predicate) => _predicate = predicate;

    internal void RefreshCautionLabelText(string text)
    {
        if (_cautionLabel != null)
        {
            _cautionLabel.Text = "* " + text;
        }
    }

    internal void ShowCautionSection(bool show)
    {
        if (!_checkBoxes.Any(static x => x.Visible))
        {
            _cautionLabel?.Hide();
            return;
        }

        foreach (Control c in Controls)
        {
            if (c.Tag is ItemType.Caution)
            {
                c.Visible = show;
            }
        }

        Refresh();
    }

    internal void SetItemCheckedStates(bool[] checkedStates)
    {
        if (checkedStates.Length != CheckItems.Length) return;

        using (new DisableEvents(this))
        {
            for (int i = 0; i < checkedStates.Length; i++)
            {
                bool checkedState = checkedStates[i];
                CheckItems[i].Checked = checkedState;
                _checkBoxes[i].Checked = checkedState;
            }
        }
    }

    #endregion

    #region Event handlers

    private void OnItemsCheckedChanged(object sender, EventArgs e)
    {
        if (EventsDisabled > 0) return;

        var s = (DarkCheckBox)sender;

        int checkBoxIndex = Array.IndexOf(_checkBoxes, s, 0, CheckItems.Length);
        if (checkBoxIndex == -1) return;

        CheckItems[checkBoxIndex].Checked = s.Checked;

        ItemCheckedChanged?.Invoke(this, EventArgs.Empty);
    }

    protected override void OnEnabledChanged(EventArgs e)
    {
        base.OnEnabledChanged(e);

        base.BackColor = _darkModeEnabled ? DarkModeBackColor : Enabled ? BackColor : SystemColors.Control;
    }

    #endregion
}
