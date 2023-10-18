using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using AngelLoader.DataClasses;
using static AL_Common.Common;
using static AngelLoader.GameSupport;
using static AngelLoader.Global;
using static AngelLoader.Utils;

namespace AngelLoader;

internal static class TDMWatchers
{
    // Don't put this one on an aggregate timer, because we want the marked-selected FM to change instantly
    private static readonly FileSystemWatcher TDMSelectedFMWatcher = new();

    private static readonly System.Timers.Timer _MissionInfoFileTimer = new(1000) { Enabled = false, AutoReset = false };
    private static readonly FileSystemWatcher TdmFMsListChangedWatcher = new();

    private static readonly System.Timers.Timer _pk4Timer = new(1000) { Enabled = false, AutoReset = false };
    private static readonly FileSystemWatcher TDM_PK4_Watcher = new();

    private static readonly object _tdmFMChangeLock = new();

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
        if (Core.View != null! && TdmFMSetChanged())
        {
            Core.View.QueueRefreshFromDisk();
        }
    }

    private static void TDM_PK4_Watcher_Changed(object sender, FileSystemEventArgs e) => _pk4Timer.Reset();

    private static void TDMSelectedFMWatcher_Changed(object sender, FileSystemEventArgs e) => UpdateTDMInstalledFMStatus(e.FullPath);

    private static void TdmFMsListChangedWatcher_Changed(object sender, FileSystemEventArgs e) => _MissionInfoFileTimer.Reset();

    private static void TDMSelectedFMWatcher_Deleted(object sender, FileSystemEventArgs e)
    {
        lock (_tdmFMChangeLock)
        {
            for (int i = 0; i < FMsViewList.Count; i++)
            {
                FanMission fm = FMsViewList[i];
                if (fm.Game == Game.TDM)
                {
                    fm.Installed = false;
                }
            }
        }

        if (Core.View != null!)
        {
            Core.View.QueueRefreshListOnly();
        }
    }

    #endregion

    internal static void UpdateTDMInstalledFMStatus(string? file = null)
    {
        lock (_tdmFMChangeLock)
        {
            string? fmName = null;
            if (file == null)
            {
                string fmInstallPath = Config.GetGamePath(GameIndex.TDM);
                if (!fmInstallPath.IsEmpty())
                {
                    try
                    {
                        file = Path.Combine(fmInstallPath, Paths.TDMCurrentFMFile);
                    }
                    catch
                    {
                        file = "";
                        fmName = null;
                    }
                }
                else
                {
                    file = "";
                    fmName = null;
                }
            }

            if (!file.IsEmpty())
            {
                if (File.Exists(file))
                {
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
                        fmName = lines[0];
                    }
                }
            }

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
        }

        if (Core.View != null!)
        {
            Core.View.QueueRefreshListOnly();
        }

        return;

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

    private static bool TdmFMSetChanged()
    {
        lock (_tdmFMChangeLock)
        {
            string fmsPath = Config.GetFMInstallPath(GameIndex.TDM);
            if (fmsPath.IsEmpty()) return false;

            try
            {
                List<string> internalTdmFMIds = new(FMDataIniListTDM.Count);
                foreach (FanMission fm in FMDataIniListTDM)
                {
                    if (!fm.MarkedUnavailable)
                    {
                        internalTdmFMIds.Add(fm.TDMInstalledDir);
                    }
                }

                List<string> fileTdmFMIds_Dirs = FastIO.GetDirsTopOnly(fmsPath, "*", returnFullPaths: false);
                List<string> fileTdmFMIds_PK4s = FastIO.GetFilesTopOnly(fmsPath, "*.pk4", returnFullPaths: false);
                HashSetI dirsHash = fileTdmFMIds_Dirs.ToHashSetI();

                var finalFilesList = new List<string>(fileTdmFMIds_Dirs.Count + fileTdmFMIds_PK4s.Count);

                finalFilesList.AddRange(fileTdmFMIds_Dirs);

                for (int i = 0; i < fileTdmFMIds_PK4s.Count; i++)
                {
                    string pk4 = fileTdmFMIds_PK4s[i];
                    string nameWithoutExt = pk4.RemoveExtension();
                    if (dirsHash.Add(nameWithoutExt))
                    {
                        finalFilesList.Add(nameWithoutExt);
                    }
                }

                for (int i = 0; i < finalFilesList.Count; i++)
                {
                    if (!IsValidTdmFM(finalFilesList[i]))
                    {
                        finalFilesList.RemoveAt(i);
                        break;
                    }
                }

                internalTdmFMIds.Sort();
                finalFilesList.Sort();

                // @TDM: Case sensitive comparison
                return !internalTdmFMIds.SequenceEqual(finalFilesList);
            }
            catch
            {
                return false;
            }
        }
    }
}
