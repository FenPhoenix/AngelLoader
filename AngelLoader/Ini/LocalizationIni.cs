using System.Collections.Generic;
using System.Reflection;
using AngelLoader.DataClasses;
using JetBrains.Annotations;

namespace AngelLoader
{
    internal static partial class Ini
    {
        [MustUseReturnValue]
        internal static LText_Class ReadLocalizationIni(string file)
        {
            var ret = new LText_Class();

            #region Dictionary setup

            const BindingFlags _bfLText = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

            var sectionFields = typeof(LText_Class).GetFields(_bfLText);
            var sections = new Dictionary<string, Dictionary<string, (FieldInfo FieldInfo, object Obj)>>(sectionFields.Length);
            for (int i = 0; i < sectionFields.Length; i++)
            {
                FieldInfo f = sectionFields[i];

                var fields = f.FieldType.GetFields(_bfLText);
                var dict = new Dictionary<string, (FieldInfo, object)>(fields.Length, new KeyComparer());
                foreach (var field in fields)
                {
                    dict[field.Name] = (field, f.GetValue(ret));
                }
                sections["[" + f.Name + "]"] = dict;
            }

            #endregion

            var lines = AL_Common.Common.File_ReadAllLines_List(file);
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

            return ret;
        }
    }
}
