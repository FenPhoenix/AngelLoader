using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Reflection;
using System.Runtime.Versioning;
using System.Windows.Forms;
using AL_Common;
using AngelLoader.DataClasses;
using AngelLoader.Forms.CustomControls;
using AngelLoader.Properties;
using static AngelLoader.Misc;

namespace AngelLoader.Forms
{
    public sealed partial class AboutForm : DarkForm
    {
        private readonly List<KeyValuePair<Control, (Color ForeColor, Color BackColor)>> _controlColors = new();

        public AboutForm()
        {
#if DEBUG
            InitializeComponent();
#else
            InitializeComponentSlim();
#endif

#if !ReleasePublic && !NoAsserts
            static bool AnyLinkLabelHasNoText(Control control, int stackCounter = 0)
            {
                try
                {
                    stackCounter++;
                    if (stackCounter > 100) return false;

                    if (control is LinkLabel && control.Text.IsEmpty()) return true;

                    for (int i = 0; i < control.Controls.Count; i++)
                    {
                        if (AnyLinkLabelHasNoText(control.Controls[i], stackCounter)) return true;
                    }

                    return false;
                }
                finally
                {
                    stackCounter--;
                }
            }

            AssertR(!AnyLinkLabelHasNoText(this), "At least one link label has no text");
#endif

            // Just grab the largest frame (sub-icon) from the AL icon resource we have already, that way we don't
            // add any extra size to our executable.
            LogoPictureBox.Image = new Icon(AL_Icon.AngelLoader, 48, 48).ToBitmap();

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

            SetTheme(Config.VisualTheme);

            Localize();
        }

        private void SetTheme(VisualTheme theme)
        {
            if (theme == VisualTheme.Dark)
            {
                ControlUtils.ChangeFormThemeMode(theme, this, _controlColors, x => x == BuildDateLabel);
                BuildDateLabel.ForeColor = DarkColors.Fen_DarkForeground;
                LogoTextPictureBox.Image = Resources.About_Dark;
            }
            else
            {
                BuildDateLabel.ForeColor = SystemColors.ControlDarkDark;
                LogoTextPictureBox.Image = Resources.About;
            }
        }

        private void Localize()
        {
            Text = LText.AboutWindow.TitleText;
            AngelLoaderUsesLabel.Text = LText.AboutWindow.AngelLoaderUses;
            OKButton.Text = LText.Global.OK;
        }

        private void LinkLabels_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            string link =
                sender == GitHubLinkLabel ? "https://github.com/FenPhoenix/AngelLoader" :
                sender == SevenZipLinkLabel ? "https://www.7-zip.org/" :
                sender == SevenZipSharpLinkLabel ? "https://github.com/squid-box/SevenZipSharp" :
                sender == FFmpegLinkLabel ? "https://ffmpeg.org/" :
                sender == FFmpegDotNetLinkLabel ? "https://github.com/cmxl/FFmpeg.NET" :
                sender == SimpleHelpersDotNetLinkLabel ? "https://github.com/khalidsalomao/SimpleHelpers.Net/" :
                sender == UdeNetStandardLinkLabel ? "https://github.com/yinyue200/ude" :
                sender == OokiiDialogsLinkLabel ? "https://github.com/augustoproiete/ookii-dialogs-winforms" :
                sender == NetCore3SysIOCompLinkLabel ? "https://github.com/dotnet/corefx/tree/release/3.0/src/System.IO.Compression" :
                sender == DarkUILinkLabel ? "https://github.com/RobinPerris/DarkUI" :
                sender == EasyHookLinkLabel ? "https://github.com/EasyHook/EasyHook" :
                sender == OpenSansLinkLabel ? "https://fonts.google.com/specimen/Open+Sans" :
                "";

            AssertR(!link.IsEmpty(), nameof(link) + " is blank");

            if (link.IsEmpty()) return;

            try
            {
                ProcessStart_UseShellExecute(link);
            }
            catch (Exception ex)
            {
                Dialogs.ShowAlert(ex.Message, LText.AlertMessages.Error, MessageBoxIcon.Error);
            }
        }
    }
}
