using System;
using System.Globalization;
using System.Reflection;
using System.Runtime.Versioning;
using AL_Common;

namespace AngelLoader.DataClasses
{
    internal static class NonLocalizableText
    {
        internal const string DarkLoaderEllipses = "DarkLoader...";
        internal const string FMSelEllipses = "FMSel...";
        internal const string NewDarkLoaderEllipses = "NewDarkLoader...";

        internal const string License =
            "MIT License\r\n\r\n" +
            "Copyright (c) 2018-2022 Brian Tobin (FenPhoenix)\r\n\r\n" +
            "Permission is hereby granted, free of charge, to any person obtaining a copy " +
            "of this software and associated documentation files (the \"Software\"), to deal " +
            "in the Software without restriction, including without limitation the rights " +
            "to use, copy, modify, merge, publish, distribute, sublicense, and/or sell " +
            "copies of the Software, and to permit persons to whom the Software is " +
            "furnished to do so, subject to the following conditions:\r\n\r\n" +
            "The above copyright notice and this permission notice shall be included in all " +
            "copies or substantial portions of the Software.\r\n\r\n" +
            "THE SOFTWARE IS PROVIDED \"AS IS\", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR " +
            "IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, " +
            "FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL " +
            "THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER " +
            "LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, " +
            "OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE " +
            "SOFTWARE.";

        internal static string GetBuildDateText()
        {
            bool success = DateTime.TryParseExact(
                BuildDateSource.BuildDate,
                "yyyyMMddHHmmss",
                DateTimeFormatInfo.InvariantInfo,
                DateTimeStyles.AssumeUniversal,
                out DateTime result);

            string ret = success
                ? result.ToLocalTime().ToString("yyyy MMM dd, HH:mm:ss", CultureInfo.CurrentCulture)
                : "";

            try
            {
                var attrs = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(TargetFrameworkAttribute), false);
                if (attrs.Length == 1)
                {
                    var fn = new FrameworkName(((TargetFrameworkAttribute)attrs[0]).FrameworkName);
                    string dotNetName = fn.Identifier.ContainsI("Framework")
                        ? ".NET Framework " + fn.Version
                        : ".NET " + fn.Version;
                    ret += "\r\n" + dotNetName;
                }
            }
            catch
            {
                // ignore
            }

            return ret;
        }

        internal const string AL_GitHub_Link = "https://github.com/FenPhoenix/AngelLoader";

        internal const string SevenZip_Link_Text = "7-Zip";
        internal const string SevenZip_Link = "https://www.7-zip.org/";

        internal const string SevenZipSharp_Link_Text = "SquidBox.SevenZipSharp";
        internal const string SevenZipSharp_Link = "https://github.com/squid-box/SevenZipSharp";

        internal const string FFmpeg_Link_Text = "ffmpeg";
        internal const string FFmpeg_Link = "https://ffmpeg.org/";

        internal const string FFmpegDotNet_Link_Text = "FFmpeg.NET";
        internal const string FFmpegDotNet_Link = "https://github.com/cmxl/FFmpeg.NET";

        internal const string SimpleHelpersDotNet_Link_Text = "SimpleHelpers.Net";
        internal const string SimpleHelpersDotNet_Link = "https://github.com/khalidsalomao/SimpleHelpers.Net/";

        internal const string UdeNetStandard_Link_Text = "Ude.NetStandard";
        internal const string UdeNetStandard_Link = "https://github.com/yinyue200/ude";

        internal const string OokiiDialogs_Link_Text = "Ookii Dialogs";
        internal const string OokiiDialogs_Link = "https://github.com/augustoproiete/ookii-dialogs-winforms";

        internal const string netCore3SysIOComp_Link_Text = ".NET Core 3 System.IO.Compression";
        internal const string NetCore3SysIOComp_Link = "https://github.com/dotnet/corefx/tree/release/3.0/src/System.IO.Compression";

        internal const string DarkUI_Link_Text = "DarkUI";
        internal const string DarkUI_Link = "https://github.com/RobinPerris/DarkUI";

        internal const string EasyHook_Link_Text = "EasyHook";
        internal const string EasyHook_Link = "https://github.com/EasyHook/EasyHook";

        internal const string OpenSans_Link_Text = "Open Sans";
        internal const string OpenSans_Link = "https://fonts.google.com/specimen/Open+Sans";
    }
}
