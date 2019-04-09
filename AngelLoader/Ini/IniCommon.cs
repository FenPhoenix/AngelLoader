using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using AngelLoader.Common.DataClasses;
using AngelLoader.Common.Utility;
using static AngelLoader.Common.Common;
using static AngelLoader.Common.Logger;

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
        internal static void ReadTranslatedLanguageName(string file)
        {
            StreamReader sr = null;
            try
            {
                sr = new StreamReader(file, Encoding.UTF8);

                bool inMeta = false;
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    var lineT = line.Trim();
                    if (inMeta && lineT.StartsWithFast_NoNullChecks(nameof(LText.Meta.TranslatedLanguageName) + "="))
                    {
                        var key = file.GetFileNameFast().RemoveExtension();
                        var value = line.TrimStart().Substring(nameof(LText.Meta.TranslatedLanguageName).Length + 1);
                        Config.LanguageNames[key] = value;
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
            catch (Exception ex)
            {
                Log("There was an error while reading " + file + ".", ex);
            }
            finally
            {
                sr?.Dispose();
            }
        }

        internal static DateTime? ReadNullableHexDate(string hexDate)
        {
            var success = long.TryParse(
                hexDate,
                NumberStyles.HexNumber,
                DateTimeFormatInfo.InvariantInfo,
                out long result);

            if (!success) return null;

            try
            {
                var dateTime = DateTimeOffset
                    .FromUnixTimeSeconds(result)
                    .DateTime
                    .ToLocalTime();

                return dateTime;
            }
            catch (ArgumentOutOfRangeException)
            {
                return null;
            }
        }
    }
}
