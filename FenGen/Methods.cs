using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FenGen
{
    class Methods
    {
        internal const string Tab = "    ";

        internal static string Indent(int num)
        {
            var ret = "";
            for (int i = 0; i < num; i++) ret += Tab;
            return ret;
        }
    }
}
