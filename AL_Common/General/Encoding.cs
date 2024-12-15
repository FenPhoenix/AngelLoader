using System;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;

namespace AL_Common;

public static partial class Common
{
    // Immediately static init for thread safety
    public static readonly Encoding UTF8NoBOM = new UTF8Encoding(false, true);

    // .NET Core changed the Encoding.Default return value from legacy ANSI to UTF8. This is disastrous for us
    // because we need to read and write certain files that are written in Framework Encoding.Default (ANSI).
    // So implement a manual version here...

    [LibraryImport("kernel32.dll")]
    private static partial int GetACP();

    public static Encoding GetLegacyDefaultEncoding()
    {
        try
        {
            // .NET Framework simply calls GetACP() for Encoding.Default, so let's try that first.
            return Encoding.GetEncoding(GetACP());
        }
        catch
        {
            try
            {
                // @NET5(Encoding.Default): I think InstalledUICulture is the actual one we want?
                // CurrentCulture will normally match it I think, but to be safe we should be explicit...
                // Unless I'm wrong, but hopefully someone will report the bug again if I am?
                return Encoding.GetEncoding(CultureInfo.InstalledUICulture.TextInfo.ANSICodePage);
            }
            catch (Exception ex)
            {
                // @NET5(Encoding.Default): If we get here, we should maybe put up a dialog, because this is a bad situation.
                // It will lead to a recurrence of the bug where cam_mod.ini is written with the wrong encoding
                // and non-ASCII game paths won't be read and FMs can't be played.
                Logger.Log(
                    $"Unable to get the system default ANSI encoding, which is required for reading and writing certain files correctly.{NL}" +
                    "Returning .NET default encoding, which will probably be UTF8 and may cause issues for locales outside North America.",
                    ex);
                return Encoding.Default;
            }
        }
    }

    public static Encoding GetOEMCodePageOrFallback(Encoding fallback)
    {
        Encoding enc;
        try
        {
            enc = Encoding.GetEncoding(CultureInfo.CurrentCulture.TextInfo.OEMCodePage);
        }
        catch
        {
            enc = fallback;
        }

        return enc;
    }
}
