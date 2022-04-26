using System;
using AngelLoader.DataClasses;
using AngelLoader.Forms;

namespace AngelLoader
{
    // Slim wrapper around the splash screen form for cleanliness (and so we can translate a Dispose() call to a
    // ProgrammaticClose() call on the form).
    internal sealed class SplashScreen : IDisposable, ISplashScreen_Safe
    {
        private readonly SplashScreenForm _splashScreenForm;

        public SplashScreen()
        {
            // We don't show the form right away, because we want to handle showing manually for reasons of
            // setting the theme and whatever else
            _splashScreenForm = new SplashScreenForm();
        }

        internal void Show(VisualTheme theme) => _splashScreenForm.Show(theme);

        public void Hide() => _splashScreenForm.Hide();

        internal void SetMessage(string message)
        {
            if (_splashScreenForm.VisibleCached) _splashScreenForm.SetMessage(message);
        }

        internal void SetCheckMessageWidth(string message) => _splashScreenForm.SetCheckMessageWidth(message);

        internal void SetCheckAtStoredMessageWidth()
        {
            if (_splashScreenForm.VisibleCached) _splashScreenForm.SetCheckAtStoredMessageWidth();
        }

        public void Dispose()
        {
            _splashScreenForm.ProgrammaticClose();
            _splashScreenForm.Dispose();
        }
    }
}
