using System.Drawing;
using System.Windows.Forms;
using AngelLoader.DataClasses;
using JetBrains.Annotations;
using static AngelLoader.Global;
#pragma warning disable 8509 // Switch expression doesn't handle all possible inputs
#pragma warning disable CS8524 // The switch expression does not handle some values of its input type (it is not exhaustive) involving an unnamed enum value.

namespace AngelLoader.Forms.CustomControls.LazyLoaded;

internal enum Lazy_FilterLabel
{
    ReleaseDate,
    LastPlayed,
    Rating,
}

internal sealed class Lazy_ToolStripLabels : IDarkable
{
    private const int FilterLabelCount = 3;

    private readonly MainForm _owner;

    private bool _darkModeEnabled;
    [PublicAPI]
    public bool DarkModeEnabled
    {
        set
        {
            if (_darkModeEnabled == value) return;
            _darkModeEnabled = value;

            for (int i = 0; i < _labels.Length; i++)
            {
                if (_constructed[i])
                {
                    ToolStripLabel label = _labels[i];
                    label.ForeColor = LabelForeColor;
                }
            }
        }
    }

    internal Lazy_ToolStripLabels(MainForm owner) => _owner = owner;

    private Color LabelForeColor => _darkModeEnabled ? DarkColors.LightText : Color.Maroon;

    private readonly bool[] _constructed = new bool[FilterLabelCount];

    // Inits to null, don't worry
    private readonly ToolStripLabel[] _labels = new ToolStripLabel[FilterLabelCount];

    internal void Show(Lazy_FilterLabel label, string text)
    {
        int li = (int)label;

        if (!_constructed[li])
        {
            ToolStripLabel _label = new()
            {
                ForeColor = LabelForeColor,
                Margin = new Padding(4, 5, 0, 2),
            };

            _labels[li] = _label;

            var container = _owner.FilterIconButtonsToolStrip;
            ToolStripButtonCustom button = label switch
            {
                Lazy_FilterLabel.ReleaseDate => _owner.FilterByReleaseDateButton,
                Lazy_FilterLabel.LastPlayed => _owner.FilterByLastPlayedButton,
                Lazy_FilterLabel.Rating => _owner.FilterByRatingButton,
            };

            for (int i = 0; i < container.Items.Count; i++)
            {
                if (container.Items[i] == button)
                {
                    container.Items.Insert(i + 1, _label);
                    break;
                }
            }

            _constructed[li] = true;

            Localize(label);
        }

        _labels[li].Text = text;
        _labels[li].Visible = true;
    }

    internal void Localize(Lazy_FilterLabel label)
    {
        int li = (int)label;

        if (!_constructed[li]) return;

        _labels[li].ToolTipText = label switch
        {
            Lazy_FilterLabel.ReleaseDate => LText.FilterBar.ReleaseDateToolTip,
            Lazy_FilterLabel.LastPlayed => LText.FilterBar.LastPlayedToolTip,
            Lazy_FilterLabel.Rating => LText.FilterBar.RatingToolTip,
        };
    }

    internal void Hide(Lazy_FilterLabel label)
    {
        int li = (int)label;
        if (!_constructed[li]) return;
        _labels[li].Visible = false;
    }
}
