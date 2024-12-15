using System.Globalization;
using System.Text;

namespace AL_Common;

public static partial class Common
{
    // Immediately static init for thread safety
    public static readonly Encoding UTF8NoBOM = new UTF8Encoding(false, true);

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
