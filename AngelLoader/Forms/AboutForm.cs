using System;
using System.Drawing;
using System.Globalization;
using System.Reflection;
using System.Runtime.Versioning;
using System.Windows.Forms;
using AL_Common;
using AngelLoader.DataClasses;
using static AngelLoader.Misc;

namespace AngelLoader.Forms
{
    public sealed partial class AboutForm : Form
    {
        public AboutForm()
        {
#if DEBUG
            InitializeComponent();
#else
            InitComponentManual();
#endif

            Icon = Images.AngelLoader;

            // Just grab the largest frame (sub-icon) from the AL icon resource we have already, that way we don't
            // add any extra size to our executable.
            LogoPictureBox.Image = new Icon(Images.AngelLoader, 48, 48).ToBitmap();

            VersionLabel.Text = Application.ProductVersion;

            bool success = DateTime.TryParseExact(
                BuildDateSource.BuildDate,
                "yyyyMMddHHmmss",
                DateTimeFormatInfo.InvariantInfo,
                DateTimeStyles.AssumeUniversal,
                out DateTime result);

            BuildDateLabel.Text = success
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
                    BuildDateLabel.Text += "\r\n" + dotNetName;
                }
            }
            catch
            {
                // ignore
            }

            // Manually set the text here, because multiline texts are otherwise stored in resources and it's a
            // whole nasty thing that doesn't even work with our resx exclude system anyway.
            LicenseTextBox.Text =
                "MIT License\r\n\r\n" +
                "Copyright (c) 2018-2021 Brian Tobin (FenPhoenix)\r\n\r\n" +
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

            Localize();
        }

        private void LinkLabels_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            string link = "";
            if (sender == GitHubLinkLabel)
            {
                link = "https://github.com/FenPhoenix/AngelLoader";
            }
            else if (sender == SevenZipLinkLabel)
            {
                link = "https://www.7-zip.org/";
            }
            else if (sender == SevenZipSharpLinkLabel)
            {
                link = "https://github.com/squid-box/SevenZipSharp";
            }
            else if (sender == FFmpegLinkLabel)
            {
                link = "https://ffmpeg.org/";
            }
            else if (sender == FFmpegDotNetLinkLabel)
            {
                link = "https://github.com/cmxl/FFmpeg.NET";
            }
            else if (sender == SimpleHelpersDotNetLinkLabel)
            {
                link = "https://github.com/khalidsalomao/SimpleHelpers.Net/";
            }
            else if (sender == UdeNetStandardLinkLabel)
            {
                link = "https://github.com/yinyue200/ude";
            }
            else if (sender == OokiiDialogsLinkLabel)
            {
                link = "https://github.com/augustoproiete/ookii-dialogs-winforms";
            }
            else if (sender == NetCore3SysIOCompLinkLabel)
            {
                link = "https://github.com/dotnet/corefx/tree/release/3.0/src/System.IO.Compression";
            }
            else if (sender == DarkUILinkLabel)
            {
                link = "https://github.com/RobinPerris/DarkUI";
            }

            try
            {
                ProcessStart_UseShellExecute(link);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, LText.AlertMessages.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Localize()
        {
            Text = LText.AboutWindow.TitleText;
            AngelLoaderUsesLabel.Text = LText.AboutWindow.AngelLoaderUses;
            OKButton.Text = LText.Global.OK;
        }
    }
}
