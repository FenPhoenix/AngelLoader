﻿using System.IO;
using System.Reflection;
using System.Text;
using AngelLoader.Common.DataClasses;
using AngelLoader.Common.Utility;
using static AngelLoader.Common.Common;

namespace AngelLoader.Ini
{
    internal static partial class Ini
    {
        internal static bool StartsWithFast_NoNullChecks(this string str, string value)
        {
            if (str.Length < value.Length) return false;

            for (int i = 0; i < value.Length; i++)
            {
                if (str[i] != value[i]) return false;
            }

            return true;
        }

        #region BindingFlags

        private const BindingFlags BFlagsInstance = BindingFlags.IgnoreCase |
                                                    BindingFlags.Public |
                                                    BindingFlags.NonPublic |
                                                    BindingFlags.Instance;

        private const BindingFlags BFlagsStatic = BindingFlags.IgnoreCase |
                                                  BindingFlags.Public |
                                                  BindingFlags.NonPublic |
                                                  BindingFlags.Static;

        private const BindingFlags BFlagsEnum = BindingFlags.Instance |
                                                BindingFlags.Static |
                                                BindingFlags.Public |
                                                BindingFlags.NonPublic;

        #endregion

        // This kinda belongs in LanguageIni.cs, but it's separated to prevent it from being removed when that
        // file is re-generated. I could make it so it doesn't get removed, but meh.
        internal static void ReadLanguageName(string file)
        {
            using (var sr = new StreamReader(file, Encoding.UTF8))
            {
                bool inMeta = false;
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    var lineT = line.Trim();
                    if (inMeta && lineT.StartsWithFast_NoNullChecks(nameof(LText.Meta.LanguageName) + "="))
                    {
                        var key = file.GetFileNameFast().RemoveExtension();
                        var value = line.TrimStart().Substring(nameof(LText.Meta.LanguageName).Length + 1);
                        Config.LanguageNames.Add(key, value);
                        return;
                    }
                    else if (lineT == "[" + nameof(LText.Meta) + "]")
                    {
                        inMeta = true;
                    }
                    else if (!lineT.IsEmpty() && lineT[0] == '[' && lineT[lineT.Length - 1] == ']')
                    {
                        inMeta = false;
                    }
                }
            }
        }
    }
}
