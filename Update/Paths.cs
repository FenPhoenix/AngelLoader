using System.IO;
using System.Windows.Forms;

namespace Update;

internal static class Paths
{
    // @Update: We could/maybe should generate AL's constants into a file in this project.
    // That way we'll automatically update them here without having to have a dll dependency (which is a no-go).
    internal static readonly string LogFile = Path.Combine(Application.StartupPath, "AngelLoader_log.txt");

    internal static readonly string ConfigIni = Path.Combine(Application.StartupPath, "Data", "Config.ini");

    internal static readonly string _baseTemp = Path.Combine(Path.GetTempPath(), "AngelLoader");
    internal static readonly string UpdateTemp = Path.Combine(_baseTemp, "Update");
    internal static readonly string UpdateBakTemp = Path.Combine(_baseTemp, "UpdateBak");
}
