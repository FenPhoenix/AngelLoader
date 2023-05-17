using System;
using System.Threading.Tasks;
using AngelLoader.DataClasses;

namespace AngelLoader;

// Slim wrapper around the splash screen form for cleanliness (and so we can translate a Dispose() call to a
// ProgrammaticClose() call on the form).
internal sealed class SplashScreen : IDisposable, ISplashScreen_Safe
{
    private ISplashScreen _splashScreenView;

    public SplashScreen(ISplashScreen splashScreenView)
    {
        // We don't show the form right away, because we want to handle showing manually for reasons of
        // setting the theme and whatever else
        _splashScreenView = splashScreenView;
    }

    internal void Show(VisualTheme theme) => _splashScreenView.Show(theme);

    public void Hide() => _splashScreenView.Hide();

    internal void SetMessage(string message)
    {
        if (_splashScreenView.VisibleCached) _splashScreenView.SetMessage(message);
    }

    internal void SetCheckMessageWidth(string message) => _splashScreenView.SetCheckMessageWidth(message);

    internal void SetCheckAtStoredMessageWidth()
    {
        if (_splashScreenView.VisibleCached) _splashScreenView.SetCheckAtStoredMessageWidth();
    }

    internal void LockPainting(bool enabled) => _splashScreenView.LockPainting(enabled);

    public void Dispose()
    {
        _splashScreenView.ProgrammaticClose();
        _splashScreenView.Dispose();
        _splashScreenView = null!;
    }
}
