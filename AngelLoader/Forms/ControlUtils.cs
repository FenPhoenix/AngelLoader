﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using JetBrains.Annotations;
using static AngelLoader.Misc;
using static AngelLoader.WinAPI.InteropMisc;

namespace AngelLoader.Forms
{
    internal static class ControlUtils
    {
        #region Suspend/resume drawing

        internal static void SuspendDrawing(this Control control)
        {
            if (!control.IsHandleCreated || !control.Visible) return;
            SendMessage(control.Handle, WM_SETREDRAW, false, 0);
        }

        internal static void ResumeDrawing(this Control control)
        {
            if (!control.IsHandleCreated || !control.Visible) return;
            SendMessage(control.Handle, WM_SETREDRAW, true, 0);
            control.Refresh();
        }

        #endregion

        /// <summary>
        /// Sets the progress bar's value instantly. Avoids the la-dee-dah catch-up-when-I-feel-like-it nature of
        /// the progress bar that makes it look annoying and unprofessional.
        /// </summary>
        /// <param name="pb"></param>
        /// <param name="value"></param>
        public static void SetValueInstant(this ProgressBar pb, int value)
        {
            value = value.Clamp(pb.Minimum, pb.Maximum);

            if (value == pb.Maximum)
            {
                pb.Value = pb.Maximum;
            }
            else
            {
                pb.Value = (value + 1).Clamp(pb.Minimum, pb.Maximum);
                pb.Value = value;
            }
        }

        #region Centering

        [PublicAPI]
        internal static void CenterH(this Control control, Control parent)
        {
            control.Location = new Point((parent.Width / 2) - (control.Width / 2), control.Location.Y);
        }

        /*
        [PublicAPI]
        internal static void CenterV(this Control control, Control parent)
        {
            control.Location = new Point(control.Location.X, (parent.Height / 2) - (control.Height / 2));
        }
        */

        [PublicAPI]
        internal static void CenterHV(this Control control, Control parent, bool clientSize = false)
        {
            int pWidth = clientSize ? parent.ClientSize.Width : parent.Width;
            int pHeight = clientSize ? parent.ClientSize.Height : parent.Height;
            control.Location = new Point((pWidth / 2) - (control.Width / 2), (pHeight / 2) - (control.Height / 2));
        }

        #endregion

        #region Autosizing

        // PERF_TODO: These are relatively expensive operations (10ms to make 3 calls from SettingsForm)
        // See if we can manually calculate some or all of this and end up with the same result as if we let the
        // layout do the work as we do now.

        /// <summary>
        /// Special case for buttons that need to be autosized but then have their width adjusted after the fact,
        /// and so are unable to have GrowAndShrink set.
        /// </summary>
        /// <param name="button"></param>
        /// <param name="text"></param>
        internal static void SetTextAutoSize(this Button button, string text)
        {
            button.Text = "";
            button.Width = 2;
            button.Text = text;
        }

        /// <summary>
        /// Sets a <see cref="Button"/>'s text, and autosizes and repositions the <see cref="Button"/> and a
        /// <see cref="TextBox"/> horizontally together to accommodate it.
        /// </summary>
        /// <param name="button"></param>
        /// <param name="textBox"></param>
        /// <param name="text"></param>
        internal static void SetTextForTextBoxButtonCombo(this Button button, TextBox textBox, string text)
        {
            // Quick fix for this not working if layout is suspended.
            // This will then cause any other controls within the same parent to do their full layout.
            // If this becomes a problem, come up with a better solution here.
            button.Parent.ResumeLayout();

            AnchorStyles oldAnchor = button.Anchor;
            button.Anchor = AnchorStyles.Top | AnchorStyles.Left;

            int oldWidth = button.Width;

            button.Text = text;

            int diff =
                button.Width > oldWidth ? -(button.Width - oldWidth) :
                button.Width < oldWidth ? oldWidth - button.Width : 0;

            button.Left += diff;
            // For some reason the diff doesn't work when scaling is > 100% so, yeah
            textBox.Width = button.Left > textBox.Left ? (button.Left - textBox.Left) - 1 : 0;

            button.Anchor = oldAnchor;
        }

        #endregion

        internal static void RemoveAndSelectNearest(this ListBox listBox)
        {
            if (listBox.SelectedIndex == -1) return;

            int oldSelectedIndex = listBox.SelectedIndex;

            listBox.Items.RemoveAt(listBox.SelectedIndex);

            if (oldSelectedIndex < listBox.Items.Count && listBox.Items.Count > 1)
            {
                listBox.SelectedIndex = oldSelectedIndex;
            }
            else if (listBox.Items.Count > 1)
            {
                listBox.SelectedIndex = oldSelectedIndex - 1;
            }
            else if (listBox.Items.Count == 1)
            {
                listBox.SelectedIndex = 0;
            }
        }

        internal static bool EqualsIfNotNull(this object? sender, object? equals) => sender != null && equals != null && sender == equals;

        internal static void HideFocusRectangle(this Control control) => SendMessage(
            control.Handle,
            WM_CHANGEUISTATE,
            new IntPtr(SetControlFocusToHidden),
            new IntPtr(0));

        internal static void MakeColumnVisible(DataGridViewColumn column, bool visible)
        {
            column.Visible = visible;
            // Fix for zero-height glitch when Rating column gets swapped out when all columns are hidden
            try
            {
                column.Width++;
                column.Width--;
            }
            // stupid OCD check in case adding 1 would take us over 65536
            catch (ArgumentOutOfRangeException)
            {
                column.Width--;
                column.Width++;
            }
        }

        internal static void FillControlDict(
            Control control,
            Dictionary<Control, (Color ForeColor, Color BackColor)> controlColors,
            int stackCounter = 0)
        {
            const int maxStackCount = 100;

            if (!controlColors.ContainsKey(control))
            {
                controlColors[control] = (control.ForeColor, control.BackColor);
            }

            stackCounter++;

            AssertR(
                stackCounter <= maxStackCount,
                nameof(FillControlDict) + "(): stack overflow (" + nameof(stackCounter) + " == " + stackCounter + ", should be <= " + maxStackCount + ")");

            for (int i = 0; i < control.Controls.Count; i++)
            {
                FillControlDict(control.Controls[i], controlColors, stackCounter);
            }
        }

        // Because color perception is complicated and not uniform, we just use a precomputed table of minimum
        // lightness values per hue value, to try and get a readable and pleasant-looking result for all possible
        // colors.
        // TODO: @DarkMode: Make one for max saturation too, then we should be good
        private static readonly int[] HueMinLightnessValues =
        {
            145,
            146,
            147,
            149,
            150,
            151,
            152,
            153,
            155,
            156,
            157,
            158,
            159,
            160,
            161,
            162,
            163,
            164,
            165,
            166,
            167,
            169,
            170,
            171,
            172,
            173,
            174,
            175,
            176,
            177,
            178,
            179,
            179,
            180,
            181,
            182,
            182,
            183,
            183,
            184,
            184,
            185,
            185,
            185,
            185,
            185,
            184,
            184,
            184,
            184,
            183,
            183,
            182,
            182,
            182,
            181,
            181,
            181,
            181,
            180,
            180,
            180,
            180,
            180,
            180,
            180,
            180,
            180,
            181,
            181,
            181,
            181,
            181,
            182,
            182,
            182,
            182,
            183,
            183,
            183,
            183,
            184,
            184,
            184,
            184,
            184,
            185,
            185,
            185,
            185,
            185,
            186,
            186,
            186,
            186,
            186,
            186,
            186,
            186,
            186,
            186,
            186,
            186,
            186,
            185,
            185,
            185,
            184,
            184,
            183,
            183,
            182,
            182,
            181,
            180,
            180,
            179,
            178,
            177,
            177,
            176,
            175,
            175,
            174,
            174,
            173,
            173,
            173,
            172,
            172,
            172,
            172,
            172,
            172,
            172,
            173,
            173,
            173,
            174,
            174,
            175,
            175,
            176,
            176,
            177,
            177,
            178,
            178,
            179,
            179,
            180,
            180,
            180,
            181,
            181,
            181,
            182,
            182,
            182,
            182,
            183,
            183,
            183,
            183,
            183,
            183,
            183,
            183,
            183,
            182,
            182,
            182,
            181,
            180,
            180,
            179,
            178,
            178,
            177,
            177,
            176,
            176,
            176,
            175,
            175,
            175,
            175,
            175,
            175,
            175,
            175,
            175,
            175,
            175,
            175,
            175,
            175,
            175,
            175,
            175,
            175,
            175,
            175,
            175,
            175,
            175,
            175,
            175,
            175,
            175,
            175,
            175,
            174,
            174,
            174,
            173,
            173,
            172,
            172,
            171,
            170,
            169,
            168,
            167,
            166,
            165,
            164,
            163,
            161,
            160,
            159,
            157,
            156,
            155,
            153,
            152,
            150,
            149,
            147,
            146
        };

        internal static Color InvertBrightness(Color color, bool monochrome)
        {
            // NOTE: HSL values are 0-240, not 0-255 as RGB colors are.

            // Set pure black to custom-white (not pure white), otherwise it would invert around to pure white
            // and that's a bit too bright.
            if (color.R == 0 && color.G == 0 && color.B == 0)
            {
                return DarkUI.Config.Colors.Fen_DarkForeground;
            }
            // One file (In These Enlightened Times) had some hidden (white-on-white) text, so make that match
            // our new background color to keep author intent (avoiding spoilers etc.)
            else if (color.R == 255 && color.G == 255 && color.B == 255)
            {
                return DarkUI.Config.Colors.Fen_DarkBackground;
            }

            if (monochrome)
            {
                return DarkUI.Config.Colors.Fen_DarkForeground;
            }

            // ReSharper disable once JoinDeclarationAndInitializer
            Color retColor;

            // Option 1:
            #region RGB shift low-contrast (https://github.com/vn971/linux-color-inversion)

            // This one doesn't provide the "blue boost" we need... so using the other formula for now

            /*
            const float whiteBias = 0.17f;

            float r = color.R / 255f;
            float g = color.G / 255f;
            float b = color.B / 255f;
            float a = color.A / 255f;

            const float m = 1.0f + whiteBias;
            float shift = whiteBias + a - Math.Min(r, Math.Min(g, b)) - Math.Max(r, Math.Max(g, b));
            float fr = (shift + r) / m;
            float fg = (shift + g) / m;
            float fb = (shift + b) / m;
            float fa = a;

            retColor = Color.FromArgb(
                (int)Math.Round(fa * 255).Clamp(0, 255),
                (int)Math.Round(fr * 255).Clamp(0, 255),
                (int)Math.Round(fg * 255).Clamp(0, 255),
                (int)Math.Round(fb * 255).Clamp(0, 255)
            );
            */

            #endregion

            // Option 2:
            #region RGB->HSL, luminance invert + bg luminance, manual blue boost

            int h = 0, l = 0, s = 0;
            ColorRGBToHLS(ColorTranslator.ToWin32(color), ref h, ref l, ref s);

            l = 240 - l;

            int origS = s;

            s = (s - 80).Clamp(0,240);

            // Blue is the only color that appears to change hue when lightness-inverted - it looks more like a
            // light purple. So shift it more into the sky-blue range, which looks more intuitively "right" for
            // a bright version of blue.
            if ((h > 150 && h < 171) &&
                (origS > 190) &&
                (l > 110 && l < 130))
            {
                h -= 20;
            }

            // TODO: @DarkMode: We clamp to a bit less than 240 in order to keep brightness from being blinding
            // Test if this looks good across all readmes
            int finalL = l.Clamp(HueMinLightnessValues[h], 200);

            retColor = ColorTranslator.FromWin32(ColorHLSToRGB(h, finalL, s));

            #endregion

            // For some reason RTF doesn't accept a \cfN if the color is 255 all around, it has to be 254 or
            // less... don't ask me
            if (retColor.R == 255 && retColor.G == 255 && retColor.B == 255)
            {
                retColor = Color.FromArgb(254, 254, 254);
            }

            return retColor;
        }
    }
}
