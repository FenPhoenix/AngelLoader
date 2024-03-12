using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Update;

internal static class Ini
{
    internal static void ReadLocalizationIni(string file, LocalizationData lText)
    {
        #region Dictionary setup

        const BindingFlags _bfLText = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

        FieldInfo[] sectionFields = typeof(LocalizationData).GetFields(_bfLText);
        var sections = new Dictionary<string, Dictionary<string, (FieldInfo FieldInfo, object Obj)>>(sectionFields.Length, StringComparer.Ordinal);
        foreach (FieldInfo f in sectionFields)
        {
            FieldInfo[] fields = f.FieldType.GetFields(_bfLText);
            var dict = new Dictionary<string, (FieldInfo, object)>(fields.Length);
            foreach (FieldInfo field in fields)
            {
                dict[field.Name] = (field, f.GetValue(lText));
            }
            sections["[" + f.Name + "]"] = dict;
        }

        #endregion

        string[] lines = File.ReadAllLines(file);
        int linesLength = lines.Length;
        for (int i = 0; i < linesLength; i++)
        {
            string lineT = lines[i].Trim();
            if (lineT.Length > 0 && lineT[0] == '[' && sections.TryGetValue(lineT, out var fields))
            {
                while (i < linesLength - 1)
                {
                    string lt = lines[i + 1].TrimStart();
                    int eqIndex = lt.IndexOf('=');
                    if (eqIndex > -1)
                    {
                        if (fields.TryGetValue(lt.Substring(0, eqIndex), out var value))
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
