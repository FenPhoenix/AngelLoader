using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using AL_Common;
using AngelLoader.DataClasses;
using AngelLoader.Forms.CustomControls;

namespace AngelLoader.Forms;

// We still want to be able to do UI-framework-dependent things when we may not have loaded our main view yet,
// so we have this concept of a "view environment" that will change implementations with the UI framework.
public sealed class FormsViewEnvironment : IViewEnvironment
{
    internal static bool ViewCreated;
    private static MainForm? _view;
    internal static MainForm ViewInternal
    {
        get
        {
            if (_view == null)
            {
                _view = new MainForm();
                ViewCreated = true;
            }
            return _view;
        }
    }

    public IView GetView() => ViewInternal;

    public IDialogs GetDialogs() => new Dialogs();

    public ISplashScreen GetSplashScreen() => new SplashScreenForm();

    public void ApplicationExit() => Application.Exit();

    public (bool Accepted, ConfigData OutConfig, bool AskForImport)
    ShowSettingsWindow(ISettingsChangeableView? view, ConfigData inConfig, SettingsWindowData.SettingsWindowState state)
    {
        using var f = new SettingsForm(view, inConfig, state);
        return (f.ShowDialogDark() == DialogResult.OK, f.OutConfig, f.AskForImport);
    }

    public string ProductVersion => Application.ProductVersion;

    public void PreloadScreenshot(ConfigData config, List<FanMission> fmsViewList)
    {
        // @ScreenshotDisplay: UI-specific preload conditions
        // If we allow moving the screenshots tab beside the readme or wherever else, we'll have to update
        // this code to match whatever the UI setup is.
        if (config.TopRightPanelCollapsed ||
            config.TopRightTabsData.SelectedTab != TopRightTab.Screenshots)
        {
            return;
        }

        SelectedFM selFM = config.GameOrganization == GameOrganization.OneList
            ? config.SelFM
            : config.GameTabsState.GetSelectedFM(config.GameTab);

        if (selFM.InstalledName.IsWhiteSpace()) return;

        FanMission? fm = fmsViewList.Find(x => x.InstalledDir.EqualsI(selFM.InstalledName));
        if (fm == null) return;
        if (!Utils.FMIsReallyInstalled(fm, out _)) return;

        // @ScreenshotDisplay: If we save the position in the screenshot cycle, we'll have to update this to match!
        List<string> screenshotFileNames = new();
        Screenshots.PopulateScreenshotFileNames(fm, screenshotFileNames);
        if (screenshotFileNames.Count == 0) return;

        string currentScreenshotFileName = screenshotFileNames[screenshotFileNames.Count - 1];

        ScreenshotsPreprocessing.Run(
            fm.InstalledDir,
            currentScreenshotFileName,
            screenshotFileNames);
    }

    public void PreprocessRTFReadme(ConfigData config, List<FanMission> fmsViewList, List<FanMission> fmsViewListUnscanned)
    {
        SelectedFM selFM = config.GameOrganization == GameOrganization.OneList
            ? config.SelFM
            : config.GameTabsState.GetSelectedFM(config.GameTab);

        if (selFM.InstalledName.IsWhiteSpace()) return;

        FanMission? fm = fmsViewList.Find(x => x.InstalledDir.EqualsI(selFM.InstalledName));
        if (fm == null) return;
        if (fmsViewListUnscanned.Contains(fm)) return;
        if (fm.SelectedReadme.IsWhiteSpace()) return;

        string readmeFile;
        try
        {
            (readmeFile, Misc.ReadmeType readmeType) = Core.GetReadmeFileAndType(fm);
            if (readmeType != Misc.ReadmeType.RichText || readmeFile.IsWhiteSpace())
            {
                return;
            }
        }
        catch
        {
            return;
        }

        byte[] bytes;
        try
        {
            bytes = File.ReadAllBytes(readmeFile);
        }
        catch
        {
            return;
        }

        RTFPreprocessing.PreloadRichFormat(readmeFile, bytes, config.DarkMode);
    }
}
