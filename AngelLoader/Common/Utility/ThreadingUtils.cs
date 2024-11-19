//#define TESTING

using System;
using System.Collections.Generic;
using System.Linq;
using AngelLoader.DataClasses;
using static AL_Common.Common;
using static AngelLoader.GameSupport;
using static AngelLoader.Global;
using static AngelLoader.Misc;

namespace AngelLoader;

public static partial class Utils
{
    internal static int GetThreadCountForParallelOperation(int maxWorkItemsCount, ThreadingData threadingData)
    {
        return Math.Min(threadingData.Threads, maxWorkItemsCount);
    }

    internal static ThreadingData GetLowestCommonThreadingData(
        List<ThreadablePath> paths
#if TESTING
        , [System.Runtime.CompilerServices.CallerMemberName] string caller = ""
#endif
        )
    {
        int? threadCount = null;

        if (Config.IOThreadsMode == IOThreadsMode.Custom)
        {
            threadCount = Config.CustomIOThreadCount;
        }

        ThreadingData threadingData;
        if (paths.Any(static x => x.DriveThreadability == DriveThreadability.Single))
        {
            threadingData = new ThreadingData(threadCount ?? 1, IOThreadingLevel.Normal);
        }
        else if (paths.All(static x => x.DriveThreadability == DriveThreadability.Aggressive))
        {
            threadingData = new ThreadingData(threadCount ?? CoreCount, IOThreadingLevel.Aggressive);
        }
        else
        {
            threadingData = new ThreadingData(threadCount ?? CoreCount, IOThreadingLevel.Normal);
        }

#if TESTING
        System.Diagnostics.Trace.WriteLine(
            nameof(GetLowestCommonThreadingData) + ": " + threadingData + $"{NL}" +
            "---- Caller: " + caller);
#endif

        return threadingData;
    }

    #region Task-relevant paths

    #region Audio conversion

    internal static List<ThreadablePath> GetAudioConversionRelevantPaths(List<ValidAudioConvertibleFM> fms)
    {
        List<ThreadablePath> ret = new(SupportedGameCount);

        bool[] fmInstalledDirsRequired = new bool[SupportedGameCount];
        for (int i = 0; i < fms.Count; i++)
        {
            fmInstalledDirsRequired[(int)fms[i].GameIndex] = true;
        }

        for (int i = 0; i < SupportedGameCount; i++)
        {
            if (fmInstalledDirsRequired[i])
            {
                GameIndex gameIndex = (GameIndex)i;
                ret.Add(new ThreadablePath(
                    Config.GetFMInstallPath(gameIndex),
                    IOPathType.Directory,
                    ThreadablePathType.FMInstallPath,
                    gameIndex));
            }
        }

        FillThreadablePaths(ret);
        return ret;
    }

    internal static int GetAudioConversionThreadCount(int maxWorkItemsCount, ThreadingData threadingData) =>
        threadingData.Level == IOThreadingLevel.Aggressive
            ? GetThreadCountForParallelOperation(maxWorkItemsCount, threadingData)
            : 1;

    #endregion

    #region Install/uninstall

    internal static List<ThreadablePath> GetInstallUninstallRelevantPaths(
        HashSetI usedArchivePaths,
        bool[] fmInstalledDirsRequired)
    {
        List<ThreadablePath> ret = new(Config.FMArchivePaths.Count + SupportedGameCount + 1);

        foreach (string item in usedArchivePaths)
        {
            ret.Add(new ThreadablePath(
                item,
                IOPathType.Directory,
                ThreadablePathType.ArchivePath));
        }

        for (int i = 0; i < SupportedGameCount; i++)
        {
            if (fmInstalledDirsRequired[i])
            {
                GameIndex gameIndex = (GameIndex)i;
                ret.Add(new ThreadablePath(
                    Config.GetFMInstallPath(gameIndex),
                    IOPathType.Directory,
                    ThreadablePathType.FMInstallPath,
                    gameIndex));
            }
        }
        ret.Add(new ThreadablePath(
            Config.FMsBackupPath,
            IOPathType.Directory,
            ThreadablePathType.BackupPath));

#if TESTING
        System.Diagnostics.Trace.WriteLine("--------- " + nameof(GetInstallUninstallRelevantPaths) + "():");
        foreach (ThreadablePath item in ret)
        {
            System.Diagnostics.Trace.WriteLine(item.OriginalPath + ", " + item.IOPathType);
        }
        System.Diagnostics.Trace.WriteLine("---------");
#endif

        FillThreadablePaths(ret);
        return ret;
    }

    internal static List<ThreadablePath> FilterToZipFMInstallRelevant(this List<ThreadablePath> paths, FMInstallAndPlay.FMData fmData)
    {
        return paths
            .Where(x =>
                (x.ThreadablePathType == ThreadablePathType.FMInstallPath &&
                 x.GameIndex == fmData.GameIndex &&
                 x.OriginalPath.PathEqualsI_Dir(fmData.InstBasePath)) ||
                (x.ThreadablePathType == ThreadablePathType.ArchivePath &&
                 x.OriginalPath.PathEqualsI_Dir(fmData.ArchiveDirectoryPath)))
            .ToList();
    }

    internal static List<ThreadablePath> FilterToPostInstallWorkRelevant(this List<ThreadablePath> paths, FMInstallAndPlay.FMData fmData)
    {
        return paths
            .Where(x =>
                x.ThreadablePathType == ThreadablePathType.FMInstallPath &&
                x.GameIndex == fmData.GameIndex &&
                x.OriginalPath.PathEqualsI_Dir(fmData.InstBasePath))
            .ToList();
    }

    internal static List<ThreadablePath> GetDeleteInstalledDirRelevantPaths(string path, GameIndex gameIndex)
    {
        List<ThreadablePath> paths = new()
        {
            new ThreadablePath(path, IOPathType.Directory, ThreadablePathType.FMInstallPath, gameIndex),
        };
        FillThreadablePaths(paths);

        return paths;
    }

    #endregion

    #region Scan

    internal static List<ThreadablePath> GetScanRelevantPaths(
        HashSetI usedArchivePaths,
        bool[] fmInstalledDirsRequired,
        bool atLeastOneSolidArchiveInSet)
    {
        List<ThreadablePath> ret = new(usedArchivePaths.Count + SupportedGameCount + 2);
        foreach (var item in usedArchivePaths)
        {
            ret.Add(new ThreadablePath(
                item,
                IOPathType.Directory,
                ThreadablePathType.ArchivePath));
        }
        for (int i = 0; i < SupportedGameCount; i++)
        {
            if (fmInstalledDirsRequired[i])
            {
                GameIndex gameIndex = (GameIndex)i;
                ret.Add(new ThreadablePath(
                    Config.GetFMInstallPath(gameIndex),
                    IOPathType.Directory,
                    ThreadablePathType.FMInstallPath,
                    gameIndex));
            }
        }
        if (atLeastOneSolidArchiveInSet)
        {
            ret.Add(new ThreadablePath(Paths.BaseTemp, IOPathType.Directory, ThreadablePathType.TempPath));
            ret.Add(new ThreadablePath(Paths.FMsCache, IOPathType.Directory, ThreadablePathType.FMCachePath));
        }

#if TESTING
        System.Diagnostics.Trace.WriteLine("--------- " + nameof(GetScanRelevantPaths) + "():");
        foreach (ThreadablePath item in ret)
        {
            System.Diagnostics.Trace.WriteLine(item.OriginalPath + ", " + item.OriginalPath);
        }
        System.Diagnostics.Trace.WriteLine("---------");
#endif

        FillThreadablePaths(ret);
        return ret;
    }

    #endregion

    private static void FillThreadablePaths(List<ThreadablePath> paths)
    {
        DetectDriveData.GetAllDriveThreadabilities(paths, Config.DriveLettersAndTypes);
    }

    #endregion
}
