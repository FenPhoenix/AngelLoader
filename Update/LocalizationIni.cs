using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Update;

internal static class Ini
{
    private sealed class KeyComparer : IEqualityComparer<string>
    {
        public bool Equals(string x, string y)
        {
            if (x == y) return true;

            // Intended: x == key in dict (no '='), y == incoming full line (with '=')
            // But assume we have no guarantee on which param is which, so swap them if they're wrong.

            int index = y.IndexOf('=');
            if (index == -1)
            {
                (y, x) = (x, y);
                index = y.IndexOf('=');
            }

            if (index != x.Length) return false;

            for (int i = 0; i < x.Length; i++)
            {
                if (x[i] != y[i]) return false;
            }

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint RotateLeft(uint value, int offset) => (value << offset) | (value >> (32 - offset));

        public unsafe int GetHashCode(string obj)
        {
            // From .NET 7 (but tweaked to stop at '=') - no separate 32/64 paths, and doesn't stop at nulls
            fixed (char* src = obj)
            {
                uint hash1 = (5381 << 16) + 5381;
                uint hash2 = hash1;

                uint* ptr = (uint*)src;

                int length = obj.IndexOf('=');
                length = length == -1 ? obj.Length : length + 1;
                int originalLength = length;

                while (length > 2 && *ptr < originalLength)
                {
                    length -= 4;
                    // Where length is 4n-1 (e.g. 3,7,11,15,19) this additionally consumes the null terminator
                    hash1 = (RotateLeft(hash1, 5) + hash1) ^ ptr[0];
                    hash2 = (RotateLeft(hash2, 5) + hash2) ^ ptr[1];
                    ptr += 2;
                }

                if (length > 0)
                {
                    // Where length is 4n-3 (e.g. 1,5,9,13,17) this additionally consumes the null terminator
                    hash2 = (RotateLeft(hash2, 5) + hash2) ^ ptr[0];
                }

                return (int)(hash1 + (hash2 * 1566083941));
            }
        }
    }

    internal static void ReadLocalizationIni(string file, LocalizationData lText)
    {
        #region Dictionary setup

        const BindingFlags _bfLText = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

        FieldInfo[] sectionFields = typeof(LocalizationData).GetFields(_bfLText);
        var sections = new Dictionary<string, Dictionary<string, (FieldInfo FieldInfo, object Obj)>>(sectionFields.Length, StringComparer.Ordinal);
        for (int i = 0; i < sectionFields.Length; i++)
        {
            FieldInfo f = sectionFields[i];

            FieldInfo[] fields = f.FieldType.GetFields(_bfLText);
            var dict = new Dictionary<string, (FieldInfo, object)>(fields.Length, new KeyComparer());
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
