using System.Drawing;
using System.Windows.Forms;
using AL_Common;
using AngelLoader.DataClasses;
using AngelLoader.Properties;
using static AngelLoader.Global;
using static AngelLoader.Utils;

namespace AngelLoader.Forms;

public sealed partial class AboutForm : DarkFormBase
{
    public AboutForm()
    {
#if DEBUG
        InitializeComponent();
#else
        InitSlim();
#endif

        // Just grab the largest frame (sub-icon) from the AL icon resource we have already, that way we don't
        // add any extra size to our executable.
        LogoPictureBox.Image = new Icon(AL_Icon.AngelLoader, 48, 48).ToBitmap();

        VersionLabel.Text = Application.ProductVersion;

        BuildDateLabel.Text = NonLocalizableText.GetBuildDateText();

        // Manually set the text here, because multiline texts are otherwise stored in resources and it's a
        // whole nasty thing that doesn't even work with our resx exclude system anyway.
        LicenseTextBox.Text = NonLocalizableText.License;

        SetTheme(Config.VisualTheme);

        Localize();
    }

    private void SetTheme(VisualTheme theme)
    {
        if (theme == VisualTheme.Dark)
        {
            SetThemeBase(theme);
            LogoTextPictureBox.Image = DarkModeImageConversion.CreateDarkModeVersion(Resources.About);
        }
        else
        {
            LogoTextPictureBox.Image = Resources.About;
        }
    }

    private void Localize()
    {
        Text = LText.AboutWindow.TitleText;
        AngelLoaderUsesLabel.Text = LText.AboutWindow.AngelLoaderUses;
        OKButton.Text = LText.Global.OK;

        GitHubLinkLabel.Text = NonLocalizableText.AL_GitHub_Link;
        SevenZipLinkLabel.Text = NonLocalizableText.SevenZip_Link_Text;
        SharpCompressLinkLabel.Text = NonLocalizableText.SharpCompress_Link_Text;
        FFmpegLinkLabel.Text = NonLocalizableText.FFmpeg_Link_Text;
        FFmpegDotNetLinkLabel.Text = NonLocalizableText.FFmpegDotNet_Link_Text;
        SimpleHelpersDotNetLinkLabel.Text = NonLocalizableText.SimpleHelpersDotNet_Link_Text;
        UdeNetStandardLinkLabel.Text = NonLocalizableText.UdeNetStandard_Link_Text;
        OokiiDialogsLinkLabel.Text = NonLocalizableText.OokiiDialogs_Link_Text;
        NetCore3SysIOCompLinkLabel.Text = NonLocalizableText.netCore3SysIOComp_Link_Text;
        DarkUILinkLabel.Text = NonLocalizableText.DarkUI_Link_Text;
        EasyHookLinkLabel.Text = NonLocalizableText.EasyHook_Link_Text;
        OpenSansLinkLabel.Text = NonLocalizableText.OpenSans_Link_Text;

#if !ReleasePublic && !NoAsserts
        static bool AnyLinkLabelHasNoText(Control control, int stackCounter = 0)
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

        AssertR(!AnyLinkLabelHasNoText(this), "At least one link label has no text");
#endif
    }

    private void LinkLabels_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
        string link =
            sender == GitHubLinkLabel ? NonLocalizableText.AL_GitHub_Link :
            sender == SevenZipLinkLabel ? NonLocalizableText.SevenZip_Link :
            sender == SharpCompressLinkLabel ? NonLocalizableText.SharpCompress_Link :
            sender == FFmpegLinkLabel ? NonLocalizableText.FFmpeg_Link :
            sender == FFmpegDotNetLinkLabel ? NonLocalizableText.FFmpegDotNet_Link :
            sender == SimpleHelpersDotNetLinkLabel ? NonLocalizableText.SimpleHelpersDotNet_Link :
            sender == UdeNetStandardLinkLabel ? NonLocalizableText.UdeNetStandard_Link :
            sender == OokiiDialogsLinkLabel ? NonLocalizableText.OokiiDialogs_Link :
            sender == NetCore3SysIOCompLinkLabel ? NonLocalizableText.NetCore3SysIOComp_Link :
            sender == DarkUILinkLabel ? NonLocalizableText.DarkUI_Link :
            sender == EasyHookLinkLabel ? NonLocalizableText.EasyHook_Link :
            sender == OpenSansLinkLabel ? NonLocalizableText.OpenSans_Link :
            "";

        AssertR(!link.IsEmpty(), nameof(link) + " is blank");

        if (link.IsEmpty()) return;

        Core.OpenLink(link);
    }
}
