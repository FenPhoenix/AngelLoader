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
    private static readonly FileSystemWatcher TDMSelectedFMWatcher = new();
    private static readonly FileSystemWatcher TdmFMsListChangedWatcher = new();

    internal static readonly object TdmFMChangeLock = new();

    internal static void Init()
    {
        TDMSelectedFMWatcher.Changed += TDMSelectedFMWatcher_Changed;
        TDMSelectedFMWatcher.Created += TDMSelectedFMWatcher_Changed;
        TDMSelectedFMWatcher.Deleted += TDMSelectedFMWatcher_Deleted;

        TdmFMsListChangedWatcher.Changed += TdmFMsListChangedWatcher_Changed;
        TdmFMsListChangedWatcher.Created += TdmFMsListChangedWatcher_Changed;
        TdmFMsListChangedWatcher.Deleted += TdmFMsListChangedWatcher_Changed;
    }

    #region Event handlers

    private static void TDMSelectedFMWatcher_Changed(object sender, FileSystemEventArgs e)
    {
        UpdateTDMInstalledFMStatus(e.FullPath);
    }

    private static void TdmFMsListChangedWatcher_Changed(object sender, FileSystemEventArgs e)
    {
        lock (TdmFMChangeLock)
        {
            if (Core.View != null! && GameConfigFiles.TdmFMSetChanged())
            {
                Core.View.QueueRefreshFromDisk();
            }
        }
    }

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

    internal static void UpdateTDMInstalledFMStatus(string? file = null)
    {
        lock (TdmFMChangeLock)
        {
            if (file == null)
            {
                string fmInstallPath = Global.Config.GetGamePath(GameIndex.TDM);
                if (fmInstallPath.IsEmpty()) return;
                try
                {
                    file = Path.Combine(fmInstallPath, Paths.TDMCurrentFMFile);
                }
                catch
                {
                    return;
                }
            }

            if (!File.Exists(file)) return;

            List<string>? lines = null;
            for (int tryIndex = 0; tryIndex < 3; tryIndex++)
            {
                if (TryGetLines(file, out lines))
                {
                    break;
                }
            }

            if (lines == null)
            {
                return;
            }

            if (lines.Count > 0)
            {
                string fmName = lines[0];
                for (int i = 0; i < FMsViewList.Count; i++)
                {
                    FanMission fm = FMsViewList[i];
                    if (fm.Game == Game.TDM)
                    {
                        fm.Installed = fm.TDMInstalledDir == fmName;
                    }
                }
            }

            if (Core.View != null!)
            {
                Core.View.QueueRefreshListOnly();
            }

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
        }
    }

    internal static void DeferredWatchersEnable(bool enableTDMWatchers)
    {
        string gamePath = Config.GetGamePath(GameIndex.TDM);
        string fmsPath = Config.GetFMInstallPath(GameIndex.TDM);
        if (enableTDMWatchers && !gamePath.IsEmpty() && !fmsPath.IsEmpty())
        {
            try
            {
                TDMSelectedFMWatcher.EnableRaisingEvents = false;
                TDMSelectedFMWatcher.Path = gamePath;
                TDMSelectedFMWatcher.Filter = Paths.TDMCurrentFMFile;
                TDMSelectedFMWatcher.NotifyFilter =
                    NotifyFilters.LastWrite |
                    NotifyFilters.CreationTime;
                TDMSelectedFMWatcher.EnableRaisingEvents = true;

            }
            catch
            {
                try
                {
                    TDMSelectedFMWatcher.EnableRaisingEvents = false;
                }
                catch
                {
                    // ignore
                }
            }

            try
            {
                TdmFMsListChangedWatcher.EnableRaisingEvents = false;
                TdmFMsListChangedWatcher.Path = fmsPath;
                TdmFMsListChangedWatcher.Filter = Paths.MissionsTdmInfo;
                TdmFMsListChangedWatcher.NotifyFilter =
                    NotifyFilters.LastWrite |
                    NotifyFilters.CreationTime;
                TdmFMsListChangedWatcher.EnableRaisingEvents = true;
            }
            catch
            {
                try
                {
                    TdmFMsListChangedWatcher.EnableRaisingEvents = false;
                }
                catch
                {
                    // ignore
                }
            }
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
    }
}
