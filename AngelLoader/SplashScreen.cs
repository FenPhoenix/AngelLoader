using System;
using AngelLoader.DataClasses;
using AngelLoader.Forms;

namespace AngelLoader
{
    // Slim wrapper around the splash screen form for cleanliness (and so we can translate a Dispose() call to a
    // ProgrammaticClose() call on the form).
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
            if (splashScreenForm.Visible) splashScreenForm.SetMessage(message);
        }

        public void Dispose() => splashScreenForm.ProgrammaticClose();
    }
}
