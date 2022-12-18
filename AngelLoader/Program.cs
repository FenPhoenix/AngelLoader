﻿//#define WPF
//#define ENABLE_RTF_VISUAL_TEST_FORM

using System;
using System.Reflection;
using Microsoft.VisualBasic.ApplicationServices;
using static AL_Common.Logger;

namespace AngelLoader;

internal static class Program
{
    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    private static void Main(string[] args)
    {
#if ENABLE_RTF_VISUAL_TEST_FORM && (DEBUG || Release_Testing)
        Forms.RTF_Visual_Test_Form.LoadIfCommandLineArgsArePresent();
        return;
#endif

#if !WPF
        // Need to set these here, because the single-instance thing internally creates a window and message-
        // loop etc... that's also why we straight-up ditched our clever "init the ConfigurationManager in
        // the background" thing, because we're going to be creating a form as the very first thing we do now
        // anyway (even if we're the second instance), so such tricks won't help us. Oh well.
        System.Windows.Forms.Application.EnableVisualStyles();
        System.Windows.Forms.Application.SetCompatibleTextRenderingDefault(false);
#if NET6_0_OR_GREATER
        // It's not like we wouldn't choose a more modern font given a clean slate, but this at least gets
        // things working without having to redo the entire set of hardcoded UI assumptions based on the font.
        System.Windows.Forms.Application.SetDefaultFont(Utils.GetMicrosoftSansSerifDefault());
#endif
#endif
        new SingleInstanceManager().Run(args);
    }

    /*
    PERF_TODO: SingleInstanceManager style takes ~30-35ms to get to the top of OnStartup().
    The old way was faster, but we can't pass args the old way. We don't currently pass args right now,
    but eh...

    -The graphics (large image from icon and AngelLoader name image), loaded in the splash screen, combined
     take ~10ms (and that's including the creation of the dark mode version of the AL name image). Meh.

    -We could parallel-load the AngelLoader text image here, though we can't theme it because we won't know
     the theme until we read the config file.
    */
    private sealed class SingleInstanceManager : WindowsFormsApplicationBase
    {
        internal SingleInstanceManager() => IsSingleInstance = true;

        protected override bool OnStartup(StartupEventArgs eventArgs)
        {
#if !NETFRAMEWORK
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
#endif

#if !WPF
            IViewEnvironment viewEnv = new Forms.FormsViewEnvironment();
#else
#endif
            // IMPORTANT: Set this first thing or else we won't have a log file!
            SetLogFile(Paths.LogFile);

            // We don't need to clear this log because LogStartup overwrites (no append)
            LogStartup(viewEnv.ProductVersion + " Started session");

            // Do this after the startup log so we don't try to log something at the same time as the non-
            // lock-protected startup log
            AppDomain.CurrentDomain.FirstChanceException += static (_, e) =>
            {
                if (e.Exception.TargetSite.DeclaringType?.Assembly == Assembly.GetExecutingAssembly())
                {
                    Log(ex: e.Exception);
                }
            };

#if !WPF
            System.Windows.Forms.Application.Run(new AppContext(viewEnv));
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

#if !WPF
    private sealed class AppContext : System.Windows.Forms.ApplicationContext
    {
        internal AppContext(IViewEnvironment viewEnv) => Core.Init(viewEnv);
    }
#else
#endif
}