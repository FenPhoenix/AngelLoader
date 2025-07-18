﻿using System.Drawing;
using System.Windows.Forms;
using AngelLoader.DataClasses;
using JetBrains.Annotations;
using static AngelLoader.Global;

namespace AngelLoader.Forms.CustomControls.LazyLoaded;

internal sealed class Lazy_ChooseReadmePanel : IDarkable
{
    private bool _constructed;

    private readonly MainForm _owner;

    private PanelCustom Panel = null!;

    private DarkListBoxWithBackingItems _listBox = null!;
    internal DarkListBoxWithBackingItems ListBox
    {
        get
        {
            Construct();
            return _listBox;
        }
    }

    private FlowLayoutPanelCustom OK_FLP = null!;
    private DarkButton OKButton = null!;

    private bool _darkModeEnabled;
    [PublicAPI]
    public bool DarkModeEnabled
    {
        set
        {
            if (_darkModeEnabled == value) return;
            _darkModeEnabled = value;

            if (!_constructed) return;

            Panel.BackColor = _darkModeEnabled ? DarkColors.Fen_DarkBackground : SystemColors.Control;
            _listBox.DarkModeEnabled = _darkModeEnabled;
            OK_FLP.BackColor = _darkModeEnabled ? DarkColors.Fen_DarkBackground : SystemColors.Control;
            OKButton.DarkModeEnabled = _darkModeEnabled;
        }
    }

    internal Lazy_ChooseReadmePanel(MainForm owner) => _owner = owner;

    private void Construct()
    {
        if (_constructed) return;

        OKButton = new StandardButton
        {
            Tag = LoadType.Lazy,

            Anchor = AnchorStyles.Top | AnchorStyles.Right,
            Margin = new Padding(0),
            TabIndex = 48,

            DarkModeEnabled = _darkModeEnabled,
        };
        OKButton.Click += _owner.ChooseReadmeButton_Click;

        OK_FLP = new FlowLayoutPanelCustom
        {
            Tag = LoadType.Lazy,

            FlowDirection = FlowDirection.RightToLeft,
            Location = new Point(1, 134),
            Size = new Size(320, 24),
            TabIndex = 3,

            BackColor = _darkModeEnabled ? DarkColors.Fen_DarkBackground : SystemColors.Control,
        };
        OK_FLP.Controls.Add(OKButton);

        _listBox = new DarkListBoxWithBackingItems
        {
            Tag = LoadType.Lazy,

            MultiSelect = false,
            Size = new Size(320, 134),
            TabIndex = 47,

            DarkModeEnabled = _darkModeEnabled,
        };

        Panel = new PanelCustom
        {
            Tag = LoadType.Lazy,

            Anchor = AnchorStyles.None,
            TabIndex = 46,
            Visible = false,
            Size = new Size(324, 161),

            BackColor = _darkModeEnabled ? DarkColors.Fen_DarkBackground : SystemColors.Control,
        };
        Panel.Controls.Add(_listBox);
        Panel.Controls.Add(OK_FLP);

        Control container = _owner.ReadmeContainer;
        container.Controls.Add(Panel);
        Panel.CenterHV(container);

        _constructed = true;

        Localize();
    }

    internal void Localize()
    {
        if (!_constructed) return;
        OKButton.Text = LText.Global.OK;
    }

    internal void ShowPanel(bool value)
    {
        if (value)
        {
            Construct();
            Panel.Show();
        }
        else if (_constructed)
        {
            Panel.Hide();
        }
    }
}
