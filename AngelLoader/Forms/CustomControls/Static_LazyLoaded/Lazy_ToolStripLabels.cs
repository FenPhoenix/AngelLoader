﻿using System.Drawing;
using System.Windows.Forms;
using JetBrains.Annotations;
using static AngelLoader.Misc;
#pragma warning disable 8509 // Switch expression doesn't handle all possible inputs
#pragma warning disable CS8524 // The switch expression does not handle some values of its input type (it is not exhaustive) involving an unnamed enum value.

namespace AngelLoader.Forms.CustomControls.Static_LazyLoaded
{
    internal enum Lazy_ToolStripLabel
    {
        FilterByReleaseDate,
        FilterByLastPlayed,
        FilterByRating
    }

    internal static class Lazy_ToolStripLabels
    {
        private static bool _darkModeEnabled;
        [PublicAPI]
        internal static bool DarkModeEnabled
        {
            get => _darkModeEnabled;
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

        private static Color LabelForeColor => _darkModeEnabled ? DarkColors.LightText : Color.Maroon;

        private static readonly bool[] _constructed = new bool[3];

        // Inits to null, don't worry
        private static readonly ToolStripLabel[] _labels = new ToolStripLabel[3];

        internal static void Show(MainForm owner, Lazy_ToolStripLabel label, string text)
        {
            int li = (int)label;

            if (!_constructed[li])
            {
                _labels[li] = new ToolStripLabel { Tag = LazyLoaded.True };
                var _label = _labels[li];

                var container = owner.FilterIconButtonsToolStrip;
                var button = label switch
                {
                    Lazy_ToolStripLabel.FilterByReleaseDate => owner.FilterByReleaseDateButton,
                    Lazy_ToolStripLabel.FilterByLastPlayed => owner.FilterByLastPlayedButton,
                    Lazy_ToolStripLabel.FilterByRating => owner.FilterByRatingButton
                };

                for (int i = 0; i < container.Items.Count; i++)
                {
                    if (container.Items[i] == button)
                    {
                        if (i == container.Items.Count - 1)
                        {
                            container.Items.Add(_label);
                        }
                        else
                        {
                            container.Items.Insert(i + 1, _label);
                        }
                        break;
                    }
                }

                _label.ForeColor = LabelForeColor;
                _label.Margin = new Padding(4, 5, 0, 2);

                _constructed[li] = true;

                Localize(label);
            }

            _labels[li].Text = text;
            _labels[li].Visible = true;
        }

        internal static void Localize(Lazy_ToolStripLabel label)
        {
            int li = (int)label;

            if (_constructed[li])
            {
                _labels[li].ToolTipText = label switch
                {
                    Lazy_ToolStripLabel.FilterByReleaseDate => LText.FilterBar.ReleaseDateToolTip,
                    Lazy_ToolStripLabel.FilterByLastPlayed => LText.FilterBar.LastPlayedToolTip,
                    Lazy_ToolStripLabel.FilterByRating => LText.FilterBar.RatingToolTip
                };
            }
        }

        internal static void Hide(Lazy_ToolStripLabel label)
        {
            int li = (int)label;
            if (_constructed[li]) _labels[li].Visible = false;
        }
    }
}
