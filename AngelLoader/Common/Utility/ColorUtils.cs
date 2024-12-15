using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace AngelLoader;

internal static class ColorUtils
{
    #region Oklab color space

    // Public domain code ported/implemented from https://bottosson.github.io/posts/oklab/

    [StructLayout(LayoutKind.Auto)]
    private readonly ref struct Lab
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

    // Lightness, chroma, hue
    [StructLayout(LayoutKind.Auto)]
    private readonly ref struct LCh
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
        // Convert 0-255 -> 0-1.0
        float r = color.R / 255.0f;
        float g = color.G / 255.0f;
        float b = color.B / 255.0f;

        // Convert sRGB -> linear sRGB
        r = r > 0.04045 ? (float)Math.Pow((r + 0.055f) / 1.055f, 2.4f) : r / 12.92f;
        g = g > 0.04045 ? (float)Math.Pow((g + 0.055f) / 1.055f, 2.4f) : g / 12.92f;
        b = b > 0.04045 ? (float)Math.Pow((b + 0.055f) / 1.055f, 2.4f) : b / 12.92f;

        // Convert linear sRGB -> Oklab
        float l = (float)Math.Cbrt((0.4122214708f * r) + (0.5363325363f * g) + (0.0514459929f * b));
        float m = (float)Math.Cbrt((0.2119034982f * r) + (0.6806995451f * g) + (0.1073969566f * b));
        float s = (float)Math.Cbrt((0.0883024619f * r) + (0.2817188376f * g) + (0.6299787005f * b));

        float l_ = (0.2104542553f * l) + (0.7936177850f * m) - (0.0040720468f * s);
        float a_ = (1.9779984951f * l) - (2.4285922050f * m) + (0.4505937099f * s);
        float b_ = (0.0259040371f * l) + (0.7827717662f * m) - (0.8086757660f * s);

        return new Lab(l_, a_, b_);
    }

    private static Color OklabToColor(Lab lab)
    {
        // Convert Oklab -> linear sRGB
        float l = lab.L + (0.3963377774f * lab.a) + (0.2158037573f * lab.b);
        float m = lab.L - (0.1055613458f * lab.a) - (0.0638541728f * lab.b);
        float s = lab.L - (0.0894841775f * lab.a) - (1.2914855480f * lab.b);

        l = l * l * l;
        m = m * m * m;
        s = s * s * s;

        double r = (+4.0767416621f * l) - (3.3077115913f * m) + (0.2309699292f * s);
        double g = (-1.2684380046f * l) + (2.6097574011f * m) - (0.3413193965f * s);
        double b = (-0.0041960863f * l) - (0.7034186147f * m) + (1.7076147010f * s);

        // Convert linear sRGB -> sRGB
        r = r > 0.0031308 ? (1.055 * Math.Pow(r, 1 / 2.4)) - 0.055 : 12.92 * r;
        g = g > 0.0031308 ? (1.055 * Math.Pow(g, 1 / 2.4)) - 0.055 : 12.92 * g;
        b = b > 0.0031308 ? (1.055 * Math.Pow(b, 1 / 2.4)) - 0.055 : 12.92 * b;

        // Convert 0-1.0 -> 0-255, and clamp (clamping should be sufficient for our use case)
        int cr = (int)(r * 255f).Clamp(0f, 255f);
        int cg = (int)(g * 255f).Clamp(0f, 255f);
        int cb = (int)(b * 255f).Clamp(0f, 255f);

        return Color.FromArgb(cr, cg, cb);
    }

    private static LCh OklabToLCh(Lab lab)
    {
        float C = (float)Math.Sqrt((lab.a * lab.a) + (lab.b * lab.b));
        float h = (float)Math.Atan2(lab.b, lab.a);

        return new LCh(lab.L, C, h);
    }

    private static Lab LChToOklab(LCh lch)
    {
        float a = (float)(lch.C * Math.Cos(lch.h));
        float b = (float)(lch.C * Math.Sin(lch.h));

        return new Lab(lch.L, a, b);
    }

    #endregion

    internal static Color InvertLightness(Color color)
    {
        // We unfortunately still need janky tuning of lightness and desaturation for good visibility, but
        // Oklab does give us a beautiful perceptual lightness scale (dark blue goes to light blue!) unlike
        // HSL, so awesome!

        Lab lab = ColorToOklab(color);

        #region Invert and global lightness boost

        float newL = lab.L < 0.5f
            ? ((1.0f - lab.L) + 0.10f).ClampZeroToOne()
            : (lab.L + 0.025f).Clamp(0.5f, 1.0f);

        lab = new Lab(newL, lab.a, lab.b);

        #endregion

        LCh lch = OklabToLCh(lab);

        #region Tweaks for specific hue ranges

        bool redDesaturated = false;

        switch (lch.h)
        {
            // Blue range
            case >= -1.901099f and <= -1.448842f:
            // Green range
            case >= 2.382358f and <= 2.880215f:
                lch = new LCh(lch.L.Clamp(0.7f, 1.0f), lch.C, lch.h);
                break;
            // Red range
            case >= -0.1025453f and <= 0.8673203f:
                lch = new LCh(lch.L.Clamp(0.6f, 1.0f), lch.C, lch.h);
                if (lch.L is >= 0.5f and <= 0.7f)
                {
                    lch = new LCh(lch.L, (lch.C - 0.025f).ClampZeroToOne(), lch.h);
                    redDesaturated = true;
                }
                break;
        }

        #endregion

        // Slight global desaturation
        lab = LChToOklab(new LCh(lch.L, (lch.C - (redDesaturated ? 0.015f : 0.04f)).ClampZeroToOne(), lch.h));

        Color retColor = OklabToColor(lab);

        return retColor;
    }
}
