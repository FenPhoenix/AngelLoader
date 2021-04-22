using System;
using AngelLoader.DataClasses;
using AngelLoader.Forms;

namespace AngelLoader
{
    // Slim wrapper around the splash screen form for cleanliness (and so we can translate a Dispose() call to a
    // ProgrammaticClose() call on the form).
    internal sealed class SplashScreen : IDisposable
    {
        private readonly SplashScreenForm _splashScreenForm;

        public SplashScreen()
        {
            // We don't show the form right away, because we want to handle showing manually for reasons of
            // setting the theme and whatever else
            _splashScreenForm = new SplashScreenForm();
        }

        internal void Show(VisualTheme theme) => _splashScreenForm.Show(theme);

        internal void Hide() => _splashScreenForm.Hide();

        internal void SetMessage(string message)
        {
            // Small perf optimization
            if (_splashScreenForm.VisibleCached) _splashScreenForm.SetMessage(message);
        }

        public void Dispose() => _splashScreenForm.ProgrammaticClose();
    }
}
