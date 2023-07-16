using System;
using System.IO;

namespace FMInfoGen;

internal static partial class Ini
{
    internal static void ReadConfigIni(ConfigData config)
    {
        string[] lines = File.ReadAllLines(Paths.ConfigFile);
        for (int i = 0; i < lines.Length; i++)
        {
            string lineTS = lines[i].TrimStart();

            bool IsKey(string key) => lineTS.StartsWith(key + "=", StringComparison.Ordinal);

            if (IsKey(nameof(config.TempPath)))
            {
                config.TempPath = lineTS.Substring(nameof(config.TempPath).Length + 1);
            }
            else if (IsKey(nameof(config.FMsPath)))
            {
                config.FMsPath = lineTS.Substring(nameof(config.FMsPath).Length + 1);
            }
        }
    }

    internal static void WriteConfigIni(ConfigData config)
    {
        using var sw = new StreamWriter(Paths.ConfigFile);
        sw.WriteLine(nameof(config.TempPath) + "=" + config.TempPath);
        sw.WriteLine(nameof(config.FMsPath) + "=" + config.FMsPath);
    }
}
