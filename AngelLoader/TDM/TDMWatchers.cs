using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using AngelLoader.DataClasses;
using static AL_Common.Common;
using static AngelLoader.GameSupport;
using static AngelLoader.Global;

namespace AngelLoader;

internal static class TDMWatchers
{
    // Don't put this one on an aggregate timer, because we want the marked-selected FM to change instantly
    private static readonly FileSystemWatcher TDMSelectedFMWatcher = new();

    private static readonly System.Timers.Timer _MissionInfoFileTimer = new(1000) { Enabled = false, AutoReset = false };
    private static readonly FileSystemWatcher TdmFMsListChangedWatcher = new();

    private static readonly System.Timers.Timer _pk4Timer = new(1000) { Enabled = false, AutoReset = false };
    private static readonly FileSystemWatcher TDM_PK4_Watcher = new();

    internal static readonly object TdmFMChangeLock = new();

    internal static void Init()
    {
        TDMSelectedFMWatcher.Changed += TDMSelectedFMWatcher_Changed;
        TDMSelectedFMWatcher.Created += TDMSelectedFMWatcher_Changed;
        TDMSelectedFMWatcher.Deleted += TDMSelectedFMWatcher_Deleted;

        TdmFMsListChangedWatcher.Changed += TdmFMsListChangedWatcher_Changed;
        TdmFMsListChangedWatcher.Created += TdmFMsListChangedWatcher_Changed;
        TdmFMsListChangedWatcher.Deleted += TdmFMsListChangedWatcher_Changed;

        TDM_PK4_Watcher.Changed += TDM_PK4_Watcher_Changed;
        TDM_PK4_Watcher.Created += TDM_PK4_Watcher_Changed;

        _pk4Timer.Elapsed += FileWatcherTimers_Elapsed;
        _MissionInfoFileTimer.Elapsed += FileWatcherTimers_Elapsed;
    }

    private static void Reset(this System.Timers.Timer timer)
    {
        timer.Stop();
        timer.Start();
    }

    #region Event handlers

    private static void FileWatcherTimers_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
    {
        lock (TdmFMChangeLock)
        {
            if (Core.View != null! && GameConfigFiles.TdmFMSetChanged())
            {
                Core.View.QueueRefreshFromDisk();
            }
        }
    }

    private static void TDM_PK4_Watcher_Changed(object sender, FileSystemEventArgs e) => _pk4Timer.Reset();

    private static void TDMSelectedFMWatcher_Changed(object sender, FileSystemEventArgs e) => UpdateTDMInstalledFMStatusWithLock(e.FullPath);

    private static void TdmFMsListChangedWatcher_Changed(object sender, FileSystemEventArgs e) => _MissionInfoFileTimer.Reset();

    private static void TDMSelectedFMWatcher_Deleted(object sender, FileSystemEventArgs e)
    {
        lock (TdmFMChangeLock)
        {
            for (int i = 0; i < FMsViewList.Count; i++)
            {
                FanMission fm = FMsViewList[i];
                if (fm.Game == Game.TDM)
                {
                    fm.Installed = false;
                }
            }

            if (Core.View != null!)
            {
                Core.View.QueueRefreshListOnly();
            }
        }
    }

    #endregion

    internal static void UpdateTDMInstalledFMStatusWithLock(string? file = null)
    {
        lock (TdmFMChangeLock)
        {
            UpdateTDMInstalledFMStatus(file);
        }
    }

    internal static bool UpdateTDMInstalledFMStatus(string? file = null)
    {
        if (file == null)
        {
            string fmInstallPath = Config.GetGamePath(GameIndex.TDM);
            if (fmInstallPath.IsEmpty()) return UpdateFMsList(null);
            try
            {
                file = Path.Combine(fmInstallPath, Paths.TDMCurrentFMFile);
            }
            catch
            {
                return UpdateFMsList(null);
            }
        }

        if (!File.Exists(file)) return UpdateFMsList(null);

        List<string>? lines = null;
        for (int tryIndex = 0; tryIndex < 3; tryIndex++)
        {
            if (TryGetLines(file, out lines))
            {
                break;
            }
        }

        if (lines?.Count > 0)
        {
            return UpdateFMsList(lines[0]);
        }

        return UpdateFMsList(null);

        static bool TryGetLines(string file, [NotNullWhen(true)] out List<string>? lines)
        {
            try
            {
                lines = File_ReadAllLines_List(file);
                return true;
            }
            catch
            {
                lines = null;
                return false;
            }
        }

        static bool UpdateFMsList(string? fmName)
        {
            for (int i = 0; i < FMsViewList.Count; i++)
            {
                FanMission fm = FMsViewList[i];
                if (fm.Game == Game.TDM)
                {
                    // @TDM(Case-sensitivity/UpdateTDMInstalledFMStatus): Case-sensitive compare
                    // Case-sensitive compare of the dir name from currentfm.txt and the dir name from our
                    // list.
                    fm.Installed = fmName != null && fm.TDMInstalledDir == fmName;
                }
            }

            if (Core.View != null!)
            {
                Core.View.QueueRefreshListOnly();
            }

            return true;
        }
    }

    internal static void DeferredWatchersEnable(bool enableTDMWatchers)
    {
        string gamePath = Config.GetGamePath(GameIndex.TDM);
        string fmsPath = Config.GetFMInstallPath(GameIndex.TDM);
        if (enableTDMWatchers && !gamePath.IsEmpty() && !fmsPath.IsEmpty())
        {
            SetWatcher(TDMSelectedFMWatcher, gamePath, Paths.TDMCurrentFMFile);
            SetWatcher(TdmFMsListChangedWatcher, fmsPath, Paths.MissionsTdmInfo);
            SetWatcher(TDM_PK4_Watcher, fmsPath, "*.pk4");
        }
        else
        {
            try
            {
                TDMSelectedFMWatcher.EnableRaisingEvents = false;
                TdmFMsListChangedWatcher.EnableRaisingEvents = false;
                TDM_PK4_Watcher.EnableRaisingEvents = false;
            }
            catch
            {
                // ignore
            }
        }

        return;

        static void SetWatcher(FileSystemWatcher watcher, string path, string filter)
        {
            try
            {
                watcher.EnableRaisingEvents = false;
                watcher.Path = path;
                watcher.Filter = filter;
                watcher.NotifyFilter =
                    NotifyFilters.LastWrite |
                    NotifyFilters.CreationTime;
                watcher.EnableRaisingEvents = true;
            }
            catch
            {
                try
                {
                    watcher.EnableRaisingEvents = false;
                }
                catch
                {
                    // ignore
                }
            }
        }
    }
}
