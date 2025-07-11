#define WinForms
//#define ENABLE_RTF_VISUAL_TEST_FORM
//#define HIGH_DPI
//#define TRACE_WRITE_TO_FILE

using System;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.VisualBasic.ApplicationServices;
using static AL_Common.Logger;

namespace AngelLoader;

internal sealed class SplashScreenPreloadState
{
    internal PrivateFontCollection? FontCollection;
    internal Font? MessageFont;

    internal readonly Task SplashScreenPreloadTask;

    internal SplashScreenPreloadState()
    {
        SplashScreenPreloadTask = Task.Run(() =>
        {
#if WinForms
            _ = Forms.Preload.AL_Icon_Bmp;
            _ = Forms.Preload.About;
            _ = Forms.Preload.AboutDark;

            // For some reason getting a built-in font is godawful slow (270+ ms), so we literally just fricking
            // bundle Open Sans and use that. It takes like 6ms. Sheesh.
            try
            {
                FontCollection = new PrivateFontCollection();
                FontCollection.AddFontFile(Path.Combine(Paths.Startup, "OpenSans-Regular.ttf"));
                MessageFont = new Font(FontCollection.Families[0], 12.0f, FontStyle.Regular);
            }
            catch
            {
                // Godawful slow as stated, but if we don't find our font, then we have to fall back to something.
                MessageFont = new Font(FontFamily.GenericSansSerif, 12.0f, FontStyle.Regular);
            }
#endif
        });
    }
}

internal static class Program
{
    internal static SplashScreenPreloadState SplashScreenPreloadState = null!;

    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    private static void Main(string[] args)
    {
#if TRACE_WRITE_TO_FILE
        using FileStream fs = File.Open(Path.Combine(Paths.Startup, "_TRACE_WRITELINE.txt"), FileMode.Create, FileAccess.Write, FileShare.Read);
        using System.Diagnostics.TextWriterTraceListener fl = new(fs);
        System.Diagnostics.Trace.Listeners.Add(fl);
#endif

        /*
        @Wine: We include this dependency solely for Wine, which requires it unless the user were to install real
        .NET. Windows doesn't require this, but it does no harm - except bloating us up by 700k, but such are the
        breaks. Other tradeoffs are substantially worse for user experience and reliability.
        */
        System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

#if ENABLE_RTF_VISUAL_TEST_FORM && (DEBUG || Release_Testing)
        Forms.RTF_Visual_Test_Form.LoadIfCommandLineArgsArePresent();
        return;
#endif

#if WinForms
        SplashScreenPreloadState = new SplashScreenPreloadState();

        // Need to set these here, because the single-instance thing internally creates a window and message-
        // loop etc... that's also why we straight-up ditched our clever "init the ConfigurationManager in
        // the background" thing, because we're going to be creating a form as the very first thing we do now
        // anyway (even if we're the second instance), so such tricks won't help us. Oh well.
        System.Windows.Forms.Application.EnableVisualStyles();
        System.Windows.Forms.Application.SetCompatibleTextRenderingDefault(false);
#if HIGH_DPI
        System.Windows.Forms.Application.SetHighDpiMode(System.Windows.Forms.HighDpiMode.PerMonitorV2);
#endif
#if NET6_0_OR_GREATER
        // It's not like we wouldn't choose a more modern font given a clean slate, but this at least gets
        // things working without having to redo the entire set of hardcoded UI assumptions based on the font.
        System.Windows.Forms.Application.SetDefaultFont(Utils.GetMicrosoftSansSerifDefault());
#endif
#endif
        new SingleInstanceManager().Run(args);
    }

    /*
    @PERF_TODO: SingleInstanceManager style takes ~30-35ms to get to the top of OnStartup().
    The old way was faster, but we can't pass args the old way. We don't currently pass args right now, but eh...
    */
    private sealed class SingleInstanceManager : WindowsFormsApplicationBase
    {
        internal SingleInstanceManager() => IsSingleInstance = true;

        protected override bool OnStartup(StartupEventArgs eventArgs)
        {
#if WinForms
            IViewEnvironment viewEnv = new Forms.FormsViewEnvironment();
#else
#endif
            // IMPORTANT: Set this first thing or else we won't have a log file!
            SetLogFile(Paths.LogFile);

            // We don't need to clear this log because LogStartup overwrites (no append)
#if X64
            const string appType = "Framework x64";
#else
            const string appType = "Framework x86";
#endif
            LogStartup(viewEnv.ProductVersion + " (" + appType + ") Started session");

            // Do this after the startup log so we don't try to log something at the same time as the non-lock-
            // protected startup log
            AppDomain.CurrentDomain.UnhandledException += static (_, e) =>
            {
                Exception ex = (Exception)e.ExceptionObject;
                if (ex.TargetSite.DeclaringType?.Assembly == Assembly.GetExecutingAssembly())
                {
                    Log("*** Unhandled exception: ", ex);
                }
            };

#if WinForms
            System.Windows.Forms.Application.Run(new AL_AppContext(viewEnv, doUpdateCleanup: eventArgs.CommandLine.Contains("-after_update_cleanup")));
#else
#endif

            return false;
        }

        protected override void OnStartupNextInstance(StartupNextInstanceEventArgs eventArgs)
        {
            // The official Microsoft example puts the base call line first, so I guess I will too?
            // https://github.com/microsoft/wpf-samples/tree/main/Application%20Management/SingleInstanceDetection
            base.OnStartupNextInstance(eventArgs);
            // We're supposed to be able to set BringToForeground = true, but it doesn't work for us. It
            // could be because we're not directly running a form, but running it later through an app context?
            // Who knows the frigging reason, but we have to do it manually for it to work, so meh.
            Core.ActivateMainView();
        }
    }

#if WinForms
    private sealed class AL_AppContext : System.Windows.Forms.ApplicationContext
    {
        internal AL_AppContext(IViewEnvironment viewEnv, bool doUpdateCleanup) => Core.Init(viewEnv, doUpdateCleanup);
    }
#else
#endif
}
