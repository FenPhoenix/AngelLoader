using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Update;

internal static class Ini
{
    private sealed class MemoryStringComparer : IEqualityComparer<ReadOnlyMemory<char>>
    {
        public bool Equals(ReadOnlyMemory<char> x, ReadOnlyMemory<char> y) => x.Span.Equals(y.Span, StringComparison.Ordinal);

        public int GetHashCode(ReadOnlyMemory<char> obj) => string.GetHashCode(obj.Span);
    }

    internal static void ReadLocalizationIni(string file, LocalizationData lText)
    {
        #region Dictionary setup

        const BindingFlags _bfLText = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

        FieldInfo[] sectionFields = typeof(LocalizationData).GetFields(_bfLText);
        var sections = new Dictionary<ReadOnlyMemory<char>, Dictionary<ReadOnlyMemory<char>, (FieldInfo FieldInfo, object Obj)>>(sectionFields.Length, new MemoryStringComparer());
        foreach (FieldInfo f in sectionFields)
        {
            FieldInfo[] fields = f.FieldType.GetFields(_bfLText);
            var dict = new Dictionary<ReadOnlyMemory<char>, (FieldInfo, object)>(fields.Length, new MemoryStringComparer());
            foreach (FieldInfo field in fields)
            {
                dict[field.Name.AsMemory()] = (field, f.GetValue(lText)!);
            }
            sections[("[" + f.Name + "]").AsMemory()] = dict;
        }

        #endregion

        string[] lines = File.ReadAllLines(file);
        int linesLength = lines.Length;
        for (int i = 0; i < linesLength; i++)
        {
            var lineT = lines[i].AsMemory().Trim();
            if (lineT.Length > 0 && lineT.Span[0] == '[' && sections.TryGetValue(lineT, out var fields))
            {
                while (i < linesLength - 1)
                {
                    string lt = lines[i + 1].TrimStart();
                    int eqIndex = lt.IndexOf('=');
                    if (eqIndex > -1)
                    {
                        if (fields.TryGetValue(lt.AsMemory()[..eqIndex], out var value))
                        {
                            value.FieldInfo.SetValue(value.Obj, lt.Substring(eqIndex + 1));
                        }
                    }
                    else if (lt.Length > 0 && lt[0] == '[')
                    {
                        break;
                    }
                    i++;
                }
            }
        }
    }
}
