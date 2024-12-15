using System.Collections.Generic;
using System.Threading;
using AngelLoader.DataClasses;

namespace AngelLoader.Forms;

internal static class ScreenshotsPreprocessing
{
    private static Thread? _thread;
    private static MemoryImage? _memoryImage;
    private static string _fmInstalledDir = "";
    private static string _currentScreenshotName = "";
    internal static readonly List<string> ScreenshotFileNames = new();
    internal static bool HasBeenActivated;

    internal static void Clear()
    {
        _thread = null;
        _memoryImage = null;
        _fmInstalledDir = "";
        _currentScreenshotName = "";
        ScreenshotFileNames.Clear();
        try
        {
            ScreenshotFileNames.Capacity = 0;
        }
        catch
        {
            // ignore
        }
    }

    internal static MemoryImage? GetMemoryImage(FanMission? fm, string currentScreenshotName)
    {
        WaitOnThread();

        if (fm == null) return null;

        if (!fm.InstalledDir.IsWhiteSpace() &&
            !_fmInstalledDir.IsWhiteSpace() &&
            currentScreenshotName.PathEqualsI(_currentScreenshotName) &&
            fm.InstalledDir.EqualsI(_fmInstalledDir))
        {
            return _memoryImage;
        }
        else
        {
            return null;
        }
    }

    internal static void WaitOnThread()
    {
        if (_thread == null) return;
        try
        {
            _thread.Join();
        }
        catch
        {
            // Not started or whatever
        }
    }

    // If we ran this in the startup work thread, it would stop before FinishInitAndShow(). But since users might
    // be loading 4K resolution pngs and whatnot, we want to overlap across as great a timespan as possible -
    // right up until the screenshot is due to be displayed.
    // We also make it a foreground thread so it doesn't keep the app alive if something goes wrong and we close.
    internal static void Run(
        string fmInstalledDir,
        string currentScreenshotFileName,
        List<string> screenshotFileNames)
    {
        HasBeenActivated = true;
        _fmInstalledDir = fmInstalledDir;
        _currentScreenshotName = currentScreenshotFileName;
        ScreenshotFileNames.ClearAndAdd(screenshotFileNames);

        try
        {
            _thread = new Thread(() =>
            {
                try
                {
                    _memoryImage = new MemoryImage(currentScreenshotFileName);
                }
                catch
                {
                    _memoryImage = null;
                }
            });

            _thread.IsBackground = false;
            _thread.Start();
        }
        catch
        {
            _memoryImage = null;
        }
    }
}
