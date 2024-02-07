using System.IO;
using System.Windows.Forms;

namespace Update;

internal static class Paths
{
    // These are manual duplicates from the main app, but it's highly unlikely any of them will change, for
    // backward compatibility reasons if nothing else.
    internal static readonly string LogFile = Path.Combine(Application.StartupPath, "AngelLoader_log.txt");

    internal static readonly string ConfigIni = Path.Combine(Application.StartupPath, "Data", "Config.ini");

    private static readonly string _baseTemp = Path.Combine(Path.GetTempPath(), "AngelLoader");
    internal static readonly string UpdateTemp = Path.Combine(_baseTemp, "Update");
    internal static readonly string UpdateBakTemp = Path.Combine(_baseTemp, "UpdateBak");
}
