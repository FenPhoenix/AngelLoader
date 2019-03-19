using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngelLoader.Ini
{
    internal static partial class Ini
    {
        internal static void WriteLanguageIni()
        {
            const string file = @"C:\Language_test.ini";
            using (var sw = new StreamWriter(file, append: false, Encoding.UTF8))
            {
                sw.WriteLine("[Global]");
                sw.WriteLine("OK=OK");
                sw.WriteLine("Cancel=Cancel");
                sw.WriteLine("BrowseEllipses=Browse...");
                sw.WriteLine("Add=Add");
                sw.WriteLine("AddEllipses=Add...");
                sw.WriteLine("Remove=Remove");
                sw.WriteLine("RemoveEllipses=Remove...");
                sw.WriteLine("Reset=Reset");
                sw.WriteLine("Unrated=Unrated");
                sw.WriteLine("None=None");
                sw.WriteLine("CustomTagInCategory=<custom>");
                sw.WriteLine("KilobyteShort=KB");
                sw.WriteLine("MegabyteShort=MB");
                sw.WriteLine("GigabyteShort=GB");
            }
        }
    }
}
