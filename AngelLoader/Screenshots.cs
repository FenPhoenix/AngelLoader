using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using AngelLoader.DataClasses;
using static AL_Common.Common;
using static AngelLoader.GameSupport;
using static AngelLoader.Global;
using static AngelLoader.Misc;
using static AngelLoader.Utils;

namespace AngelLoader;

public sealed class ScreenshotWatcher(GameIndex gameIndex)
{
    private readonly GameIndex _gameIndex = gameIndex;

    private sealed class ScreenshotWatcherTimer(double interval) : System.Timers.Timer(interval)
    {
        internal string FullPath { get; private set; } = "";

        internal void ResetWith(string fullPath)
        {
            this.Reset();
            FullPath = fullPath;
        }
    }

    private bool _constructed;
    private FileSystemWatcher _watcher = null!;

    private ScreenshotWatcherTimer _timer = null!;

    private string _path = "";
    public string Path
    {
        get => _constructed ? _watcher.Path : _path;
        set
        {
            if (_constructed)
            {
                try
                {
                    _watcher.Path = value;
                }
                catch
                {
                    EnableWatching = false;
                }
            }
            else
            {
                _path = value;
            }
        }
    }

    private bool _enableWatching;
    public bool EnableWatching
    {
        get => _constructed ? _watcher.EnableRaisingEvents : _enableWatching;
        set
        {
            if (_constructed)
            {
                try
                {
                    _watcher.EnableRaisingEvents = value;
                }
                catch
                {
                    try
                    {
                        _watcher.EnableRaisingEvents = false;
                    }
                    catch
                    {
                        // ignore
                    }
                }
            }
            else
            {
                _enableWatching = value;
            }
        }
    }

    public void Construct()
    {
        if (_constructed) return;
        if (_path.IsEmpty()) return;

        try
        {
            _watcher = new FileSystemWatcher(Path);
            _watcher.Changed += Watcher_ChangedCreatedDeleted;
            _watcher.Created += Watcher_ChangedCreatedDeleted;
            _watcher.Deleted += Watcher_ChangedCreatedDeleted;
            _watcher.Renamed += Watcher_Renamed;
            _watcher.IncludeSubdirectories = true;
            _watcher.EnableRaisingEvents = EnableWatching;

            _timer = new ScreenshotWatcherTimer(1000) { Enabled = false, AutoReset = false };
            _timer.Elapsed += Timer_Elapsed;
        }
        catch
        {
            _constructed = false;
            return;
        }

        _constructed = true;
    }

    private void Watcher_ChangedCreatedDeleted(object sender, FileSystemEventArgs e)
    {
        _timer.ResetWith(e.FullPath);
    }

    private void Watcher_Renamed(object sender, RenamedEventArgs e)
    {
        _timer.ResetWith(e.OldFullPath);
    }

    private void HandleEvent(string fullPath) => Core.View.Invoke(() =>
    {
        if (!_constructed) return;
        if (!EnableWatching) return;

        FanMission? fm = Core.View.GetMainSelectedFMOrNull();
        if (fm == null) return;
        if (!fm.Game.ConvertsToKnownAndSupported(out GameIndex gameIndex)) return;
        if (gameIndex != _gameIndex) return;

        fullPath = fullPath.ToForwardSlashes_Net();

        string screenshotsParentDir;

        // @GENGAMES(Screenshots watcher)
        if (gameIndex == GameIndex.TDM)
        {
            string gamePath = Config.GetGamePath(GameIndex.TDM).ToForwardSlashes_Net();
            if (gamePath.IsEmpty()) return;
            screenshotsParentDir = gamePath.GetDirNameFast();
        }
        else
        {
            screenshotsParentDir = fm.RealInstalledDir;
        }

        string fmPlusScreenshotsPathSegment = "/" + screenshotsParentDir + "/screenshots";
        if (fullPath.ContainsI(fmPlusScreenshotsPathSegment + "/") ||
            fullPath.PathEndsWithI(fmPlusScreenshotsPathSegment))
        {
            /*
            @ScreenshotDisplay: If this is enabled, it's possible a non-matching change will override the matching one.
            Unlikely, but meh. But if we disable it, then any TDM FM will attempt refresh if anything in the
            central screenshots dir gets modified, even if it doesn't match our FM name. Also not a big deal. We
            could fix both cases by keeping a list of filenames and then checking if our name exists in it, and
            refreshing then.
            */
#if false
            if (gameIndex != GameIndex.TDM || ScreenshotFileMatchesTDMName(fm.TDMInstalledDir, fullPath.GetFileNameFast()))
#endif
            {
                Core.View.RefreshFMScreenshots(fm);
            }
        }
    });

    private void Timer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
    {
        if (Core.View == null!) return;
        HandleEvent(_timer.FullPath);
    }
}

public sealed class DisableScreenshotWatchers : IDisposable
{
    /*
    IMPORTANT! @THREADING(FMInstalledDirModificationScope):
    If we ever make it so that things can go in parallel (install/uninstall, scan, delete, etc.), this will
    no longer be safe! We're threading noobs so we don't know if volatile will solve the problem or what.
    Needs testing.
    */
    private static int _count;

    private readonly bool[] _originalValues = new bool[SupportedGameCount];

    public DisableScreenshotWatchers()
    {
        if (_count == 0)
        {
            for (int i = 0; i < SupportedGameCount; i++)
            {
                GameIndex gameIndex = (GameIndex)i;
                ScreenshotWatcher watcher = Screenshots.GetScreenshotWatcher(gameIndex);
                _originalValues[i] = watcher.EnableWatching;
                watcher.EnableWatching = false;
            }
        }
        _count++;
    }

    public void Dispose()
    {
        if (_count == 1)
        {
            for (int i = 0; i < SupportedGameCount; i++)
            {
                GameIndex gameIndex = (GameIndex)i;
                ScreenshotWatcher watcher = Screenshots.GetScreenshotWatcher(gameIndex);
                watcher.EnableWatching = _originalValues[i];
            }
        }
        _count = (_count - 1).ClampToZero();
    }
}

internal static class Screenshots
{
    private static readonly ScreenshotWatcher[] _screenshotWatchers = new ScreenshotWatcher[SupportedGameCount];
    internal static ScreenshotWatcher GetScreenshotWatcher(GameIndex gameIndex) => _screenshotWatchers[(int)gameIndex];

    static Screenshots()
    {
        for (int i = 0; i < SupportedGameCount; i++)
        {
            _screenshotWatchers[i] = new ScreenshotWatcher((GameIndex)i);
        }
    }

    /// <summary>
    /// If there are screenshots on disk, <paramref name="screenshotFileNames"/> will contain their full names.
    /// If there are none, or if <paramref name="fm"/> is <see langword="null"/>, <paramref name="screenshotFileNames"/> will be empty.
    /// </summary>
    /// <param name="fm"></param>
    /// <param name="screenshotFileNames"></param>
    internal static void PopulateScreenshotFileNames(FanMission? fm, List<string> screenshotFileNames)
    {
        screenshotFileNames.Clear();
        if (fm == null) return;
        if (!GameIsKnownAndSupported(fm.Game)) return;

        // @GENGAMES(Screenshots)
        if (fm.Game == Game.TDM)
        {
            /*
            TDM screenshot filename formats:

            Format 1 (1.08 - 2.10):
            mapname + "_%Y-%m-%d_%H.%M.%S." + extension
            Example: river1_1_2016-11-03_21.47.21.png
            Extracted fm name should be "river1_1"

            Format 2 (2.11+):
            mapname + " (%Y-%m-%d %H-%M-%S) (" + playerViewOriginStr + ")." + extension
            Example: written (2023-10-03 20-19-23) (889.44 -1464.35 174.68).jpg
            Extracted fm name should be "written"
            */

            string tdmGamePath = Config.GetGamePath(GameIndex.TDM);
            if (!tdmGamePath.IsEmpty() &&
                /*
                TDM stores all FMs' screenshots in the same dir. To prevent having to get the entire set (which
                could be very large), just get ones starting with our FM name and do a proper filter afterwards.
                */
                TryGetSortedScreenshotFileInfos(tdmGamePath, fm.TDMInstalledDir + "*.*", out FileInfo[]? files))
            {
                foreach (FileInfo fi in files)
                {
                    string fn = fi.Name;

                    if (ScreenshotFileMatchesTDMName(fm.TDMInstalledDir, fn))
                    {
                        AddIfValidFormat(screenshotFileNames, fi.FullName);
                    }
                }
            }
        }
        else
        {
            if (fm.Installed && FMIsReallyInstalled(fm, out string fmInstalledPath) &&
                TryGetSortedScreenshotFileInfos(fmInstalledPath, "*", out FileInfo[]? files))
            {
                foreach (FileInfo fi in files)
                {
                    AddIfValidFormat(screenshotFileNames, fi.FullName);
                }
            }
        }

        return;

        static bool TryGetSortedScreenshotFileInfos(
            string screenshotsDirParentPath,
            string pattern,
            [NotNullWhen(true)] out FileInfo[]? screenshots)
        {
            // @ScreenshotDisplay: Performance... we need a custom FileInfo getter without the 8.3 stuff
            try
            {
                /*
                @ScreenshotDisplay(Thief 3 central screenshots):
                These get put into one directory with the FM mission name (not overall FM name!) at the start of
                the file. For example, "All The World's A Stage" screenshots get prefixed "Bohn Street Theatre".
                These names are listed in \CONTENT\T3\Books\English\String_Tags\Misc.sch but also in other languages
                so we'd have to check every one of them, which also means we can't make one single search pattern,
                so we'd have a performance issue too. It's not even close to worth it to try to do this, so we're
                just not supporting central screenshots for Thief 3 right now.
                */
                string ssPath = Path.Combine(screenshotsDirParentPath, "screenshots");
                DirectoryInfo di = new(ssPath);
                // Standard practice is to let the GetFiles() call throw if the directory doesn't exist, but for
                // some reason that takes ~30ms whereas this check is <2ms (cold) to <.1ms (warm).
                if (!di.Exists)
                {
                    screenshots = null;
                    return false;
                }
                screenshots = di.GetFiles(pattern);
                Comparers.Screenshot.SortDirection = SortDirection.Ascending;
                Array.Sort(screenshots, Comparers.Screenshot);
                return true;
            }
            catch
            {
                screenshots = null;
                return false;
            }
        }

        static void AddIfValidFormat(List<string> screenshotFileNames, string filename)
        {
            if (filename.ExtIsUISupportedImage())
            {
                screenshotFileNames.Add(filename);
            }
        }
    }

    private static bool ScreenshotFileMatchesTDMName(string tdmInstalledDir, string fn)
    {
        int spaceIndex = fn.IndexOf(' ');
        // @TDM_CASE: Screenshot FM name comparison
        if (spaceIndex > -1 && fn.Substring(0, spaceIndex).Trim().EqualsI(tdmInstalledDir))
        {
            return true;
        }
        else
        {
            int underscoreIndex = fn.LastIndexOf('_');
            if (underscoreIndex == -1) return false;

            int secondToLastUnderscoreIndex = fn.LastIndexOf('_', (underscoreIndex - 1).ClampToZero());
            if (secondToLastUnderscoreIndex == -1) return false;

            // @TDM_CASE: Screenshot FM name comparison
            if (fn.Substring(0, secondToLastUnderscoreIndex).Trim().EqualsI(tdmInstalledDir))
            {
                return true;
            }
        }

        return false;
    }
}
