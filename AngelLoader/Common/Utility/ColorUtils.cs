using System;
using System.Drawing;
using static AngelLoader.Misc;

namespace AngelLoader
{
    internal static class ColorUtils
    {
        #region Oklab color space

        // Public domain code ported/implemented from https://bottosson.github.io/posts/oklab/

        private readonly struct Lab
        {
            internal readonly float L;
            internal readonly float a;
            internal readonly float b;

            internal Lab(float L, float a, float b)
            {
                this.L = L;
                this.a = a;
                this.b = b;
            }
        }

        private readonly struct LCh
        {
            internal readonly float L;
            internal readonly float C;
            internal readonly float h;

            internal LCh(float L, float C, float h)
            {
                this.L = L;
                this.C = C;
                this.h = h;
            }
        }

        private static Lab ColorToOklab(Color color)
        {
            // Convert 0-255 to 0-1.0
            float r = color.R / 255.0f;
            float g = color.G / 255.0f;
            float b = color.B / 255.0f;

            // Convert to linear sRGB
            r = r > 0.04045 ? (float)Math.Pow((r + 0.055f) / 1.055f, 2.4f) : r / 12.92f;
            g = g > 0.04045 ? (float)Math.Pow((g + 0.055f) / 1.055f, 2.4f) : g / 12.92f;
            b = b > 0.04045 ? (float)Math.Pow((b + 0.055f) / 1.055f, 2.4f) : b / 12.92f;

            float l = 0.4122214708f * r + 0.5363325363f * g + 0.0514459929f * b;
            float m = 0.2119034982f * r + 0.6806995451f * g + 0.1073969566f * b;
            float s = 0.0883024619f * r + 0.2817188376f * g + 0.6299787005f * b;

            l = CubicRoot(l);
            m = CubicRoot(m);
            s = CubicRoot(s);

            var l2 = 0.2104542553f * l + 0.7936177850f * m - 0.0040720468f * s;
            var a2 = 1.9779984951f * l - 2.4285922050f * m + 0.4505937099f * s;
            var b2 = 0.0259040371f * l + 0.7827717662f * m - 0.8086757660f * s;

            var ret = new Lab(l2, a2, b2);

            return ret;
        }

        private static Color OklabToColor(Lab lab)
        {
            float l = lab.L + 0.3963377774f * lab.a + 0.2158037573f * lab.b;
            float m = lab.L - 0.1055613458f * lab.a - 0.0638541728f * lab.b;
            float s = lab.L - 0.0894841775f * lab.a - 1.2914855480f * lab.b;

            l = l * l * l;
            m = m * m * m;
            s = s * s * s;

            double r = +4.0767416621f * l - 3.3077115913f * m + 0.2309699292f * s;
            double g = -1.2684380046f * l + 2.6097574011f * m - 0.3413193965f * s;
            double b = -0.0041960863f * l - 0.7034186147f * m + 1.7076147010f * s;

            // Convert to regular RGB
            r = r > 0.0031308 ? 1.055 * Math.Pow(r, 1 / 2.4) - 0.055 : 12.92 * r;
            g = g > 0.0031308 ? 1.055 * Math.Pow(g, 1 / 2.4) - 0.055 : 12.92 * g;
            b = b > 0.0031308 ? 1.055 * Math.Pow(b, 1 / 2.4) - 0.055 : 12.92 * b;

            // Convert from 0-1.0 to 0-255
            // TODO: We have to clamp these, otherwise they're often outside the range.
            // The other implementations don't seem to do this. I checked and triple checked and couldn't find
            // any mistakes, and the results look fine visually, so I dunno...?
            int cr = (int)(r * 255f).Clamp(0f, 255f);
            int cg = (int)(g * 255f).Clamp(0f, 255f);
            int cb = (int)(b * 255f).Clamp(0f, 255f);

            return Color.FromArgb(cr, cg, cb);
        }

        private static LCh OklabToLCh(Lab lab)
        {
            float C = (float)(Math.Sqrt((lab.a * lab.a) + (lab.b * lab.b)));
            float h = (float)(Math.Atan2(lab.b, lab.a));

            return new LCh(lab.L, C, h);
        }

        private static Lab LChToOklab(LCh lch)
        {
            float a = (float)(lch.C * Math.Cos(lch.h));
            float b = (float)(lch.C * Math.Sin(lch.h));

            return new Lab(lch.L, a, b);
        }

        #endregion

        internal static Color InvertLightness(Color color, bool monochrome)
        {
            #region Special cases

            if (monochrome)
            {
                return DarkUI.Config.Colors.Fen_DarkForeground;
            }
            // Set pure black to custom-white (not pure white), otherwise it would invert around to pure white
            // and that's a bit too bright.
            else if (color.R == 0 && color.G == 0 && color.B == 0)
            {
                return DarkUI.Config.Colors.Fen_DarkForeground;
            }
            // One file (In These Enlightened Times) had some hidden (white-on-white) text, so make that match
            // our new background color to keep author intent (avoiding spoilers etc.)
            else if (color.R == 255 && color.G == 255 && color.B == 255)
            {
                return DarkUI.Config.Colors.Fen_DarkBackground;
            }

            #endregion

            #region Oklab lightness invert + slight boost

            // We unfortunately still need janky tuning of lightness and desaturation for good visibility, but
            // Oklab does give us a beautiful perceptual lightness scale (dark blue goes to light blue!) unlike
            // HSL, so awesome!

            var lab = ColorToOklab(color);

            #region Invert and global lightness boost

            float newL = lab.L < 0.5f
                ? ((1.0f - lab.L) + 0.10f).Clamp(0.0f, 1.0f)
                : (lab.L + 0.025f).Clamp(0.5f, 1.0f);
            //if (newL >= 0.5f && newL <= 0.7f)
            //{
            //    //newL = (float)(1.0f - (lab.L - 0.5)).Clamp(0.7f, 1.0f);
            //    newL = (lab.L + 0.25f).Clamp(0.5f, 0.7f);
            //}

            lab = new Lab(newL, lab.a, lab.b);

            #endregion

            var lch = OklabToLCh(lab);

            #region Tweaks for specific hue ranges

            bool redDesaturated = false;

            // Blue range
            if (lch.h >= -1.901099 && lch.h <= -1.448842)
            {
                lch = new LCh(lch.L.Clamp(0.7f, 1.0f), lch.C, lch.h);
            }
            // Green range
            else if (lch.h >= 2.382358 && lch.h <= 2.880215)
            {
                lch = new LCh(lch.L.Clamp(0.7f, 1.0f), lch.C, lch.h);
            }
            // Red range
            else if (lch.h >= -0.1025453 && lch.h <= 0.8673203)
            {
                lch = new LCh(lch.L.Clamp(0.6f, 1.0f), lch.C, lch.h);
                if (lch.L >= 0.5f && lch.L <= 0.7f)
                {
                    lch = new LCh(lch.L, lch.C - 0.025f, lch.h);
                    redDesaturated = true;
                }
            }

            #endregion

            // Slight global desaturation
            lab = LChToOklab(new LCh(lch.L, lch.C - (redDesaturated ? 0.015f : 0.04f), lch.h));

            #endregion

            // Almost done...
            Color retColor = OklabToColor(lab);

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
