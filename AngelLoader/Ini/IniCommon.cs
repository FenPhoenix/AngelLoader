using System;
using System.Reflection;
using AngelLoader.Common;
using AngelLoader.Common.DataClasses;
using AngelLoader.Common.Utility;

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

        private static ColumnData ConvertStringToColumnData(string str)
        {
            str = str.Trim().Trim(',');

            // DisplayIndex,Width,Visible
            // 0,100,True
            var commas = str.CountChars(',');

            if (commas == 0) return null;

            var cProps = str.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            if (cProps.Length == 0) return null;

            var ret = new ColumnData();
            for (int i = 0; i < cProps.Length; i++)
            {
                switch (i)
                {
                    case 0:
                        if (int.TryParse(cProps[i], out int di))
                        {
                            ret.DisplayIndex = di;
                        }
                        break;
                    case 1:
                        if (int.TryParse(cProps[i], out int width))
                        {
                            ret.Width = width > Defaults.MinColumnWidth ? width : Defaults.MinColumnWidth;
                        }
                        break;
                    case 2:
                        ret.Visible = cProps[i].EqualsTrue();
                        break;
                }
            }

            return ret;
        }
    }
}
