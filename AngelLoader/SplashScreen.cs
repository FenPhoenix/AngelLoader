using AngelLoader.DataClasses;
using AngelLoader.Forms;

namespace AngelLoader
{
    // TODO(SplashScreen): Even with our hacks, the main window activation still sometimes doesn't work.
    // We should either try to un-thread this and maybe that'll work, or we just give up and remove it.
    // We could try starting the splash screen in the main thread and shoving the main app into a secondary
    // thread. Maybe that would be the "right way around" for it?!
    internal static class SplashScreen
    {
        private static SplashScreenForm? splashScreenForm;
        private static bool _initialized;
        private static bool _closed;

        internal static void Init()
        {
            if (_initialized || _closed) return;
            _initialized = true;

            // We don't show the form right away, because we want to handle showing manually for reasons of
            // setting the theme and whatever else
            splashScreenForm = new SplashScreenForm();
        }

        internal static void SetMessage(string message)
        {
            if (!_initialized || _closed) return;

            if (splashScreenForm != null && splashScreenForm.IsHandleCreated)
            {
                splashScreenForm.SetMessage(message);
            }
        }

        internal static void Show(VisualTheme theme)
        {
            if (!_initialized || _closed) return;

            if (splashScreenForm != null && splashScreenForm.IsHandleCreated)
            {
                splashScreenForm.Show(theme);
            }
        }

        internal static void Hide()
        {
            if (!_initialized || _closed) return;

            splashScreenForm?.Hide();
        }

        internal static void Close()
        {
            if (!_initialized || _closed) return;
            _closed = true;

            splashScreenForm?.ProgrammaticClose();
        }
    }
}
