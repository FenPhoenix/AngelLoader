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

    /// <summary>
    /// This is to work around a quirk when CodePagesEncodingProvider is registered.
    /// If we call Encoding.GetEncoding() with a codepage corresponding to one of the encodings that .NET supports
    /// by default, then CodePagesEncodingProvider will fail to find it in its own list, but in the process of
    /// failing to find it, it will heap-allocate a 40-byte array. And since nothing will have been cached, it
    /// will go through this fail-to-find process - and the 40-byte alloc - every single time it gets one of these
    /// codepages. So call through this custom method to intercept such codepages and redirect them to the
    /// built-in .NET static cached versions.
    /// <para/>
    /// This is not necessary on Framework, as there the codepages are all built-in so there isn't this layering
    /// quirk.
    /// </summary>
    /// <param name="codePage"></param>
    /// <returns></returns>
    public static Encoding GetEncoding_Arbitrary(int codePage)
    {
        return codePage switch
        {
            // Order of roughly expected commonness
            65001 => Encoding.UTF8,
            1200 => Encoding.Unicode,
            20127 => Encoding.ASCII,
            28591 => Encoding.Latin1,
            1201 => Encoding.BigEndianUnicode,
            12000 => Encoding.UTF32,
            // This one, alone, is private. Sure why the hell not.
            // Fortunately it's almost certainly never going to occur in practice in our use case, so meh.
            //case 12001:
            //    return Encoding.BigEndianUTF32;
            _ => Encoding.GetEncoding(codePage),
        };
    }
}
