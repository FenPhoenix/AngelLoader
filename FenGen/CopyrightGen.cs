using System.Collections.Generic;
using System.IO;
using System.Xml;
using static FenGen.Misc;

namespace FenGen;

internal static class CopyrightGen
{
    private static readonly string[] LicenseTopLines =
    {
        "The full source code can be obtained from: https://github.com/FenPhoenix/AngelLoader",
        "------------------------------------------------------------------------------------",
        "",
    };

    private static readonly string[] MitLicenseLines =
    {
        "MIT License",
        "",
        "Copyright (c) 2018-" + Cache.CurrentYear + " Brian Tobin (FenPhoenix)",
        "",
        "Permission is hereby granted, free of charge, to any person obtaining a copy",
        "of this software and associated documentation files (the \"Software\"), to deal",
        "in the Software without restriction, including without limitation the rights",
        "to use, copy, modify, merge, publish, distribute, sublicense, and/or sell",
        "copies of the Software, and to permit persons to whom the Software is",
        "furnished to do so, subject to the following conditions:",
        "",
        "The above copyright notice and this permission notice shall be included in all",
        "copies or substantial portions of the Software.",
        "",
        "THE SOFTWARE IS PROVIDED \"AS IS\", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR",
        "IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,",
        "FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE",
        "AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER",
        "LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,",
        "OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE",
        "SOFTWARE.",
    };

    internal static void Generate(string destFile)
    {
        GenProjCopyright();
        GenCurrentYear(destFile);
        GenLicenses();
    }

    private static void GenProjCopyright()
    {
        var xml = new XmlDocument { PreserveWhitespace = true };
        xml.Load(Core.ALProjectFile);
        XmlElement copyrightNode = (XmlElement)xml.GetElementsByTagName("Copyright")[0];
        copyrightNode.InnerText = "Copyright © 2018 - " + Cache.CurrentYear;

        WriteXml(xml);
    }

    private static void GenCurrentYear(string destFile)
    {
        var w = GetWriterForClass(destFile, GenAttributes.FenGenCurrentYearDestClassAttribute);

        w.WL("internal const string CurrentYear = \"" + Cache.CurrentYear + "\";");
        w.CloseClassAndNamespace();

        File.WriteAllText(destFile, w.ToString());
    }

    private static void GenLicenses()
    {
        string solutionLicenseFile = Path.Combine(Core.ALSolutionPath, "LICENSE");
        string alProjectLicenseFile = Path.Combine(Core.ALProjectPath, "LICENSE");
        string alCommonLicenseFile = Path.Combine(Core.ALCommonProjectPath, "LICENSE");
        string fenGenLicenseFile = Path.Combine(Core.FenGenProjectPath, "LICENSE");
        string distLicenseFile = Path.Combine(Core.ALSolutionPath, "BinReleaseOnly", "Licenses", "AngelLoader license.txt");

        File.WriteAllLines(solutionLicenseFile, MitLicenseLines);
        File.WriteAllLines(alProjectLicenseFile, MitLicenseLines);
        File.WriteAllLines(alCommonLicenseFile, MitLicenseLines);
        File.WriteAllLines(fenGenLicenseFile, MitLicenseLines);

        var distLicenseLines = new List<string>();
        distLicenseLines.AddRange(LicenseTopLines);
        distLicenseLines.AddRange(MitLicenseLines);

        File.WriteAllLines(distLicenseFile, distLicenseLines);
    }
}
