using System;
using System.IO;

namespace FMInfoGen;

internal static class Ini
{
    internal static void ReadConfigIni(ConfigData config)
    {
        string[] lines = File.ReadAllLines(Paths.ConfigFile);
        for (int i = 0; i < lines.Length; i++)
        {
            string lineTS = lines[i].TrimStart();

            if (IsKey(nameof(config.TempPath)))
            {
                config.TempPath = lineTS.Substring(nameof(config.TempPath).Length + 1);
            }
            else if (IsKey(nameof(config.FMsPath)))
            {
                config.FMsPath = lineTS.Substring(nameof(config.FMsPath).Length + 1);
            }

            continue;

            bool IsKey(string key) => lineTS.StartsWith(key + "=", StringComparison.Ordinal);
        }
    }

    internal static void WriteConfigIni(ConfigData config)
    {
        using StreamWriter sw = new(Paths.ConfigFile);
        sw.WriteLine(nameof(config.TempPath) + "=" + config.TempPath);
        sw.WriteLine(nameof(config.FMsPath) + "=" + config.FMsPath);
    }
}
