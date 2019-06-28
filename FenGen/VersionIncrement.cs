using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FenGen
{
    internal static class VersionIncrement
    {
        internal static void Generate(string fileName, VersionType verType)
        {
            var lines = File.ReadAllLines(fileName);
            var asmVerIndex = -1;
            var asmInfoVerIndex = -1;
            string asmVer = "";
            string asmInfoVer = "";
            for (int i = 0; i < lines.Length; i++)
            {
                var lineT = lines[i].Trim();
                if (lineT.StartsWith("[assembly: AssemblyVersion("))
                {
                    asmVer = Regex.Match(lineT, @"\""(?<Version>.+)\""").Groups["Version"].Value;
                    asmVerIndex = i;
                }
                else if (lineT.StartsWith("[assembly: AssemblyInformationalVersion("))
                {
                    asmInfoVer = Regex.Match(lineT, @"\""(?<Version>.+)\""").Groups["Version"].Value;
                    asmInfoVerIndex = i;
                }
            }

            if (asmVerIndex == -1 || asmInfoVerIndex == -1) return;

            var verFirstPart = asmVer.Substring(0, asmVer.LastIndexOf('.'));
            var rev = asmVer.Substring(asmVer.LastIndexOf('.') + 1);
            int.TryParse(rev, out int result);
            lines[asmVerIndex] = "[assembly: AssemblyVersion(\"" + verFirstPart + "." + (result + 1) + "\")]";
            var asmInfoVerString = verType == VersionType.Beta ? "beta build " + (result + 1) : "";
            lines[asmInfoVerIndex] = "[assembly: AssemblyInformationalVersion(\"" + asmInfoVerString + "\")]";

            File.WriteAllLines(fileName, lines);
        }
    }
}
