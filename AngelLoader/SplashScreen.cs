using System;
using AngelLoader.DataClasses;
using AngelLoader.Forms;

namespace AngelLoader
{
    // TODO(SplashScreen): Even with our hacks, the main window activation still sometimes doesn't work.
    // We should either try to un-thread this and maybe that'll work, or we just give up and remove it.
    // We could try starting the splash screen in the main thread and shoving the main app into a secondary
    // thread. Maybe that would be the "right way around" for it?!
    internal sealed class SplashScreen : IDisposable
    {
        private readonly SplashScreenForm splashScreenForm;

        public SplashScreen()
        {
            // We don't show the form right away, because we want to handle showing manually for reasons of
            // setting the theme and whatever else
            splashScreenForm = new SplashScreenForm();
        }

        internal void Show(VisualTheme theme) => splashScreenForm.Show(theme);

        internal void Hide() => splashScreenForm.Hide();

        internal void SetMessage(string message)
        {
            // Small perf optimization
            if (splashScreenForm.Visible)
            {
                splashScreenForm.SetMessage(message);
            }
        }

        public void Dispose() => splashScreenForm.ProgrammaticClose();
    }
}
