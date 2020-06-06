#define FenGen_GameSupportSource

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngelLoader.FenGenSource
{
    internal class FenGenSource_GameSupport
    {
        internal enum Flags
        {
            Dark
        }

        internal sealed class GameSpec
        {
            internal string DisplayName = "";
            internal string InternalName = "";
            internal string ShortName = "";
            internal string IniPrefix = "";
            internal string LTextPrefix = "";
            internal string DifficultyNames = "Normal,Hard,Expert,Extreme";
            internal Flags Flags = Flags.Dark;
        }

        internal static void _()
        {

        }
    }
}
