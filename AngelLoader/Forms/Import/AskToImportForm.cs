using System;
using System.Windows.Forms;
using AngelLoader.DataClasses;
using static AngelLoader.Global;
using static AngelLoader.Misc;

namespace AngelLoader.Forms;
public sealed partial class AskToImportForm : DarkFormBase
{
    public ImportType SelectedImportType;

    public AskToImportForm()
    {
#if DEBUG
        InitializeComponent();
#else
        InitSlim();
#endif

        if (Config.DarkMode) SetThemeBase(Config.VisualTheme);

        Localize();
    }

    public override void RespondToSystemThemeChange() => SetThemeBase(Config.VisualTheme);

    private void Localize()
    {
        Text = LText.Importing.AskToImport_Title;

        MessageLabel.Text = LText.Importing.AskToImport_Message;
        DarkLoaderButton.Text = NonLocalizableText.DarkLoaderEllipses;
        FMSelButton.Text = NonLocalizableText.FMSelEllipses;
        NewDarkLoaderButton.Text = NonLocalizableText.NewDarkLoaderEllipses;
        DontImportButton.Text = LText.Importing.AskToImport_DontImport;

        int maxButtonLength = 0;

        if (DarkLoaderButton.Width > maxButtonLength) maxButtonLength = DarkLoaderButton.Width;
        if (FMSelButton.Width > maxButtonLength) maxButtonLength = FMSelButton.Width;
        if (NewDarkLoaderButton.Width > maxButtonLength) maxButtonLength = NewDarkLoaderButton.Width;
        if (DontImportButton.Width > maxButtonLength) maxButtonLength = DontImportButton.Width;

        DarkLoaderButton.Width = maxButtonLength;
        FMSelButton.Width = maxButtonLength;
        NewDarkLoaderButton.Width = maxButtonLength;
        DontImportButton.Width = maxButtonLength;

        int maxLength = 0;

        if (MessageLabel.Width > maxLength) maxLength = MessageLabel.Width;
        if (DarkLoaderButton.Width > maxLength) maxLength = DarkLoaderButton.Width;
        if (FMSelButton.Width > maxLength) maxLength = FMSelButton.Width;
        if (NewDarkLoaderButton.Width > maxLength) maxLength = NewDarkLoaderButton.Width;
        if (DontImportButton.Width > maxLength) maxLength = DontImportButton.Width;

        if (ClientSize.Width < maxLength + 48) ClientSize = ClientSize with { Width = maxLength + 48 };

        MessageLabel.CenterH(this, clientSize: true);
        DarkLoaderButton.CenterH(this, clientSize: true);
        FMSelButton.CenterH(this, clientSize: true);
        NewDarkLoaderButton.CenterH(this, clientSize: true);
        DontImportButton.CenterH(this, clientSize: true);
    }

    private void ImportButtons_Click(object sender, EventArgs e)
    {
        SelectedImportType =
            sender == DarkLoaderButton ? ImportType.DarkLoader :
            sender == FMSelButton ? ImportType.FMSel :
            ImportType.NewDarkLoader;
        DialogResult = DialogResult.OK;
        Close();
    }

    private void DontImportButton_Click(object sender, EventArgs e)
    {
        DialogResult = DialogResult.Cancel;
        Close();
    }
}
