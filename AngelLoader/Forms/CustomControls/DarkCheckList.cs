using System;
using System.ComponentModel;
using System.Drawing;
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

    private PictureBox? _errorPictureBox;
    private DarkLabel? _errorLabel;

    private DarkLabel? _cautionLabel;
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
    public bool InErrorState { get; private set; }

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

        if (_errorPictureBox != null)
        {
            _errorPictureBox.Image = Images.RedExclCircle;
        }
    }

    #endregion

    #region Public methods

    internal static bool IsControlCaution(Control control) => control.Tag is ItemType.Caution;

    /*
    @MEM/@Mods(Mods panel checkbox list):
    -Make a control to handle the recycling/dark mode syncing of these
    -Now that we only change the mods list on game data set, we can just have one set of mods per supported
     game, and only rebuild them on game change.
    */
    internal void ClearList()
    {
        Controls.DisposeAndClear();
        _checkBoxes.DisposeAll();
        _checkBoxes = Array.Empty<DarkCheckBox>();
        CheckItems = Array.Empty<CheckItem>();
    }

    internal void SetErrorText(string text)
    {
        if (!text.IsEmpty())
        {
            ClearList();

            _errorPictureBox?.Dispose();
            _errorLabel?.Dispose();

            _errorPictureBox = new PictureBox
            {
                Location = new Point(16, 8),
                Size = new Size(14, 14),
                Visible = false
            };
            _errorLabel = new DarkLabel
            {
                AutoSize = true,
                Location = new Point(_errorPictureBox.Right + 4, 8),
                Visible = false
            };

            _errorPictureBox.Click += static (_, _) => Core.OpenLogFile();
            _errorLabel.Click += static (_, _) => Core.OpenLogFile();

            _errorPictureBox.Image = Images.RedExclCircle;
            _errorLabel.Text = text;

            Controls.Add(_errorPictureBox);
            Controls.Add(_errorLabel);

            _errorPictureBox.Show();
            _errorLabel.Show();

            InErrorState = true;
        }
        else
        {
            _errorPictureBox?.Hide();
            if (_errorLabel != null)
            {
                _errorLabel.Hide();
                _errorLabel.Text = "";
            }

            InErrorState = false;
        }
    }

    internal void FillList(CheckItem[] items, string cautionText)
    {
        SetErrorText("");

        ClearList();
        _checkBoxes = new DarkCheckBox[items.Length];

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

            var cb = new DarkCheckBox
            {
                AutoSize = true,
                Text = item.Text + (item.Caution ? " *" : ""),
                Location = new Point(x, 4 + y),
                Checked = item.Checked
            };
            if (item.Caution)
            {
                cb.Tag = ItemType.Caution;
                cb.Visible = _predicate?.Invoke() ?? true;
                cb.SetFontStyle(FontStyle.Italic);
                cb.BackColor = Color.MistyRose;
            }
            if (firstCautionDone)
            {
                cb.DarkModeBackColor = DarkColors.Fen_RedHighlight;
            }
            Controls.Add(cb);
            _checkBoxes[i] = cb;
            cb.CheckedChanged += OnItemsCheckedChanged;
        }

        if (firstCautionDone)
        {
            _cautionLabel = new DarkLabel
            {
                Tag = ItemType.Caution,
                Visible = _predicate?.Invoke() ?? true,
                AutoSize = true,
                ForeColor = Color.Maroon,
                DarkModeForeColor = DarkColors.Fen_CautionText,
                Location = new Point(4, 8 + y)
            };
            RefreshCautionLabelText(cautionText);
            Controls.Add(_cautionLabel);

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

        RefreshDarkMode();
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

        int checkBoxIndex = Array.IndexOf(_checkBoxes, s);

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
