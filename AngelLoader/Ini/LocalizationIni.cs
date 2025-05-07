using System;
using System.Collections.Generic;
using System.Reflection;
using AngelLoader.DataClasses;

namespace AngelLoader;

internal static partial class Ini
{
    internal static Dictionary<string, Dictionary<string, (FieldInfo FieldInfo, object Obj)>> GetLocalizationDictionary(LText_Class lText)
    {
        const BindingFlags _bfLText = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

        FieldInfo[] sectionFields = typeof(LText_Class).GetFields(_bfLText);
        var sections = new Dictionary<string, Dictionary<string, (FieldInfo FieldInfo, object Obj)>>(sectionFields.Length, StringComparer.Ordinal);
        foreach (FieldInfo f in sectionFields)
        {
            FieldInfo[] fields = f.FieldType.GetFields(_bfLText);
            var dict = new Dictionary<string, (FieldInfo, object)>(fields.Length, new KeyComparer());
            foreach (FieldInfo field in fields)
            {
                dict[field.Name] = (field, f.GetValue(lText));
            }
            sections["[" + f.Name + "]"] = dict;
        }

        return sections;
    }

    internal static void ReadLocalizationIni(string file, LText_Class lText, Dictionary<string, Dictionary<string, (FieldInfo FieldInfo, object Obj)>>? sections = null)
    {
        sections ??= GetLocalizationDictionary(lText);

        List<string> lines = File_ReadAllLines_List(file);
        int linesLength = lines.Count;
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
                        if (fields.TryGetValue(lt, out var value))
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
