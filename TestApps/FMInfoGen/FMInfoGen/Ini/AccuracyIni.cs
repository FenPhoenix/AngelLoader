using System.IO;
using System.Reflection;

namespace FMInfoGen;

internal static partial class Ini
{
    internal static void WriteAccuracyData(AccuracyData acc, string fileName)
    {
        using var sw = new StreamWriter(fileName, append: false);
        foreach (PropertyInfo p in acc.GetType().GetProperties())
        {
            sw.WriteLine(p.Name + "=" + (p.GetValue(acc) == null
                ? "null"
                : p.GetValue(acc).ToString()));
        }
    }

    internal static AccuracyData ReadAccuracyData(string fileName)
    {
        var acc = new AccuracyData();

        using var sr = new StreamReader(fileName);
        foreach (PropertyInfo p in acc.GetType().GetProperties())
        {
            string? line = sr.ReadLine();
            string value = line!.Substring(line.IndexOf('=') + 1);
            p.SetValue(acc, value == "null" ? null : (bool?)value.EqualsI(bool.TrueString));
        }

        return acc;
    }
}
