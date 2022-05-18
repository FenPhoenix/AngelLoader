using System.Windows.Forms;
using AngelLoader.DataClasses;

namespace AngelLoader.Forms
{
    // We still want to be able to do UI-framework-dependent things when we may not have loaded our main view yet,
    //so we have this concept of a "view environment" that will change implementations with the UI framework.
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

        public (bool Accepted, ConfigData OutConfig)
        ShowSettingsWindow(ISettingsChangeableWindow? view, ConfigData inConfig, bool startup, bool cleanStart)
        {
            using var f = new SettingsForm(view, inConfig, startup, cleanStart);
            return (f.ShowDialogDark() == DialogResult.OK, f.OutConfig);
        }

        public string ProductVersion => Application.ProductVersion;
    }
}
