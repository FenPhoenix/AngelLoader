// @MT_TASK: Comment this out for final release
#define TESTING

using System;
using System.Collections.Generic;
using System.Linq;
using AL_Common;
using AngelLoader.DataClasses;
using static AngelLoader.Global;
using static AngelLoader.Misc;
namespace AngelLoader;

public static partial class Utils
{
    internal static int GetThreadCountForParallelOperation(int maxWorkItemsCount, ThreadingData threadingData)
    {
        return Math.Min(threadingData.Threads, maxWorkItemsCount);
    }

    internal static ThreadingData GetLowestCommonThreadingData(List<IOPath> paths)
    {
        int? threadCount = null;

        if (Config.IOThreadingMode == IOThreadingMode.Custom)
        {
            threadCount = Config.CustomIOThreads;
        }

        ThreadablePath[] threadablePaths = new ThreadablePath[paths.Count];
        for (int i = 0; i < paths.Count; i++)
        {
            threadablePaths[i] = new ThreadablePath(paths[i].Path, paths[i].Type);
        }

        DetectDriveTypes.GetAllDrivesType(threadablePaths, Config.DriveLettersAndTypes);

        List<ThreadablePath> threadablePathsList = new(threadablePaths.Length);
        for (int i = 0; i < threadablePaths.Length; i++)
        {
            ThreadablePath item = threadablePaths[i];
            if (!item.Root.IsEmpty())
            {
                threadablePathsList.Add(item);
            }
        }

        ThreadingData threadingData;
        if (threadablePathsList.Any(static x => x.DriveType == AL_DriveType.Other))
        {
            threadingData = new ThreadingData(threadCount ?? 1, IOThreadingLevel.Normal);
        }
        else if (threadablePathsList.All(static x => x.DriveType == AL_DriveType.NVMe_SSD))
        {
            threadingData = new ThreadingData(threadCount ?? CoreCount, IOThreadingLevel.Aggressive);
        }
        else
        {
            threadingData = new ThreadingData(threadCount ?? CoreCount, IOThreadingLevel.Normal);
        }

#if TESTING
        System.Diagnostics.Trace.WriteLine(nameof(GetLowestCommonThreadingData) + ": " + threadingData);
#endif

        return threadingData;
    }
}
