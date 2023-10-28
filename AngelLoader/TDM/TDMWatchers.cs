using System.IO;
using System.Threading.Tasks;
using AL_Common;
using static AngelLoader.GameSupport;
using static AngelLoader.Global;
using static AngelLoader.Misc;

namespace AngelLoader;

internal static class TDMWatchers
{
    // Don't put this one on an aggregate timer, because we want the marked-selected FM to change instantly
    private static readonly FileSystemWatcher TDMSelectedFMWatcher = new();

    private static readonly System.Timers.Timer _MissionInfoFileTimer = new(1000) { Enabled = false, AutoReset = false };
    private static readonly FileSystemWatcher TdmFMsListChangedWatcher = new();

    internal static void Init()
    {
        TDMSelectedFMWatcher.Changed += TDMSelectedFMWatcher_Changed;
        TDMSelectedFMWatcher.Created += TDMSelectedFMWatcher_Changed;
        TDMSelectedFMWatcher.Deleted += TDMSelectedFMWatcher_Changed;

        TdmFMsListChangedWatcher.Changed += TdmFMsListChangedWatcher_Changed;
        TdmFMsListChangedWatcher.Created += TdmFMsListChangedWatcher_Changed;
        TdmFMsListChangedWatcher.Deleted += TdmFMsListChangedWatcher_Changed;

        _MissionInfoFileTimer.Elapsed += FileWatcherTimers_Elapsed;
    }

    private static async Task DoFMDataChanged()
    {
        if (TDM.TdmFMSetChanged())
        {
            if (!Core.View.ModalDialogUp())
            {
                await Core.RefreshFMsListFromDisk();
            }
        }
        else
        {
            TDM.SetTDMMissionInfoData();
        }

        TDM.ViewRefreshAfterAutoUpdate();
    }

    private static void DoCurrentFMChanged()
    {
        TDM.UpdateTDMInstalledFMStatus();
        TDM.ViewRefreshAfterAutoUpdate();
    }

    private static Task RefreshIfAllowed(TDM_FileChanged refresh) => (Task)Core.View.Invoke(async () =>
    {
        if (!Core.View.RefreshAllowed()) return;

        switch (refresh)
        {
            case TDM_FileChanged.MissionInfo:
                try
                {
                    Core.View.SetWaitCursor(true);
                    await DoFMDataChanged();
                    DoCurrentFMChanged();
                }
                finally
                {
                    Core.View.SetWaitCursor(false);
                }
                break;
            case TDM_FileChanged.CurrentFM:
                try
                {
                    Core.View.SetWaitCursor(true);
                    DoCurrentFMChanged();
                }
                finally
                {
                    Core.View.SetWaitCursor(false);
                }
                break;
        }
    });

    #region Event handlers

    private static async void FileWatcherTimers_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
    {
        if (Core.View == null!) return;
        await RefreshIfAllowed(TDM_FileChanged.MissionInfo);
    }

    private static void TdmFMsListChangedWatcher_Changed(object sender, FileSystemEventArgs e) => _MissionInfoFileTimer.Reset();

    private static async void TDMSelectedFMWatcher_Changed(object sender, FileSystemEventArgs e)
    {
        if (Core.View == null!) return;
        await RefreshIfAllowed(TDM_FileChanged.CurrentFM);
    }

    #endregion

    /*
    @TDM_NOTE(Watchers): If we don't have an fms dir, all watchers are disabled.
    -currentfm.txt is fine, the watcher works even if there isn't a currentfm.txt and then one is created.
    -missions.tdminfo is also fine, if missing the watcher will pick it back up when it gets created and modified.
    -A fresh TDM install puts the fms dir there already as it comes with a few FMs, so we can probably just
     ignore the edge case of no fms dir. It's highly unlikely to occur, and all it results in is auto-refresh
     shutting off until the next app run.
    */
    internal static void DeferredWatchersEnable(bool enableTDMWatchers)
    {
        string gamePath = Config.GetGamePath(GameIndex.TDM);
        string fmsPath = Config.GetFMInstallPath(GameIndex.TDM);
        if (enableTDMWatchers && !gamePath.IsEmpty() && !fmsPath.IsEmpty())
        {
            SetWatcher(TDMSelectedFMWatcher, gamePath, Paths.TDMCurrentFMFile);
            SetWatcher(TdmFMsListChangedWatcher, fmsPath, Paths.MissionsTdmInfo);
        }
        else
        {
            try
            {
                TDMSelectedFMWatcher.EnableRaisingEvents = false;
                TdmFMsListChangedWatcher.EnableRaisingEvents = false;
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
