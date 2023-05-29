using System;
using System.Drawing;
using System.Windows.Forms;
using AngelLoader.DataClasses;
using AngelLoader.Forms.CustomControls;
using static AngelLoader.Global;

namespace AngelLoader.Forms;

public sealed partial class AboutForm : DarkFormBase
{
    private readonly DarkLinkLabel[] _linkLabels = new DarkLinkLabel[NonLocalizableText.DependenciesCount];

    public AboutForm()
    {
#if DEBUG
        InitializeComponent();
#else
        InitSlim();
#endif

        SuspendLayout();

        int x = 32;
        int y = 388;
        int tabIndex = 6;
        for (int i = 0; i < NonLocalizableText.DependenciesCount; i++, y += 16, tabIndex++)
        {
            if (i == 8)
            {
                x = 232;
                y = 388;
            }

            var label = new DarkLinkLabel
            {
                AutoSize = true,
                Location = new Point(x, y),
                TabIndex = tabIndex,
                TabStop = true
            };
            label.LinkClicked += LinkLabels_LinkClicked;
            _linkLabels[i] = label;
            Controls.Add(label);
        }

        ResumeLayout(false);
        PerformLayout();

        LogoPictureBox.Image = Preload.AL_Icon_Bmp;

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
            LogoTextPictureBox.Image = Preload.AboutDark;
        }
        else
        {
            LogoTextPictureBox.Image = Preload.About;
        }
    }

    private void Localize()
    {
        Text = LText.AboutWindow.TitleText;
        AngelLoaderUsesLabel.Text = LText.AboutWindow.AngelLoaderUses;
        OKButton.Text = LText.Global.OK;

        GitHubLinkLabel.Text = NonLocalizableText.AL_GitHub_Link;

        for (int i = 0; i < NonLocalizableText.DependenciesCount; i++)
        {
            _linkLabels[i].Text = NonLocalizableText.Dependencies[i].Text;
        }
    }

    private void LinkLabels_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
        string link = sender == GitHubLinkLabel
            ? NonLocalizableText.AL_GitHub_Link
            : NonLocalizableText.Dependencies[Array.IndexOf(_linkLabels, (DarkLinkLabel)sender)].Link;

        Core.OpenLink(link);
    }
}
