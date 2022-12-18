﻿using System;
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
    public int EventsDisabledCount { get; set; }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool EventsDisabled => EventsDisabledCount > 0;

    #region Private fields

    private Func<bool>? _predicate;

    private PictureBox? _errorPictureBox;
    private DarkLabel? _errorLabel;

    private DarkLabel? _cautionLabel;
    private DrawnPanel? _cautionPanel;

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

    [PublicAPI]
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
    public event EventHandler<DarkCheckListEventArgs>? ItemCheckedChanged;

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

    public bool InErrorState;

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
                TabStop = false,
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

        DarkLabel? cautionLabel = null;

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
            cautionLabel?.SendToBack();

            _cautionPanel = new DrawnPanel
            {
                Tag = ItemType.Caution,
                Visible = _predicate?.Invoke() ?? true,
                Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right,
                DrawnBackColor = Color.MistyRose,
                DarkModeDrawnBackColor = DarkColors.Fen_RedHighlight,
                Location = new Point(4, 4 + firstCautionY),
                Size = new Size(ClientRectangle.Width - 8, (4 + y) - (4 + firstCautionY))
            };
            Controls.Add(_cautionPanel);
            _cautionPanel.SendToBack();
        }

        CheckItems = items;

        RefreshDarkMode();
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
        if (EventsDisabled) return;

        var s = (DarkCheckBox)sender;

        int checkBoxIndex = Array.IndexOf(_checkBoxes, s);

        CheckItems[checkBoxIndex].Checked = s.Checked;

        ItemCheckedChanged?.Invoke(this, new DarkCheckListEventArgs(checkBoxIndex, s.Checked, s.Text));
    }

    protected override void OnEnabledChanged(EventArgs e)
    {
        base.OnEnabledChanged(e);

        base.BackColor = _darkModeEnabled ? DarkModeBackColor : Enabled ? BackColor : SystemColors.Control;
    }

    #endregion
}