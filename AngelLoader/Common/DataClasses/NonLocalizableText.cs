using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;
using System.Runtime.Versioning;
using AL_Common;

namespace AngelLoader.DataClasses;

[SuppressMessage("ReSharper", "ConvertToConstant.Global")]
internal static partial class NonLocalizableText
{
    internal static readonly string DarkLoaderEllipses = "DarkLoader...";
    internal static readonly string FMSelEllipses = "FMSel...";
    internal static readonly string NewDarkLoaderEllipses = "NewDarkLoader...";

    internal static readonly string ThiefBuddy = "Thief Buddy";

    internal static readonly string License =
        "MIT License\r\n\r\n" +
        "Copyright (c) 2018-" + CurrentYear + " Brian Tobin (FenPhoenix)\r\n\r\n" +
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
            object[] attrs = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(TargetFrameworkAttribute), false);
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

    internal static readonly string AL_GitHub_Link = "https://github.com/FenPhoenix/AngelLoader";

    internal static readonly (string Text, string Link)[] Dependencies =
    {
        ("7-Zip", "https://www.7-zip.org/"),
        ("SharpCompress", "https://github.com/adamhathcock/sharpcompress"),
        ("ffmpeg", "https://ffmpeg.org/"),
        ("FFmpeg.NET", "https://github.com/cmxl/FFmpeg.NET"),
        ("SimpleHelpers.Net", "https://github.com/khalidsalomao/SimpleHelpers.Net/"),
        ("Ude.NetStandard", "https://github.com/yinyue200/ude"),
        ("Ookii Dialogs", "https://github.com/augustoproiete/ookii-dialogs-winforms"),
        (".NET Core 3 System.IO.Compression", "https://github.com/dotnet/corefx/tree/release/3.0/src/System.IO.Compression"),
        ("DarkUI", "https://github.com/RobinPerris/DarkUI"),
        ("EasyHook", "https://github.com/EasyHook/EasyHook"),
        ("Open Sans", "https://fonts.google.com/specimen/Open+Sans")
    };
    internal static readonly int DependenciesCount = Dependencies.Length;

    private static string[]? _percentStrings;
    internal static string[] PercentStrings
    {
        get
        {
            if (_percentStrings == null)
            {
                _percentStrings = new string[101];
                for (int i = 0; i < 101; i++)
                {
                    _percentStrings[i] = i.ToString(NumberFormatInfo.CurrentInfo) + "%";
                }
            }
            return _percentStrings;
        }
    }

    internal static readonly string ThiefBuddyLink = "https://drive.google.com/drive/folders/1plHN_9b819QfpL0UQX6s2NBM3gb9yYGV";
}
