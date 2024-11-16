// @MT_TASK: Comment this out for final release
#define TESTING

using System;
using System.Collections.Generic;
using System.Linq;
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

        List<AL_DriveType> types = DetectDriveTypes.GetAllDrivesType(paths, Config.DriveLettersAndTypes);

        ThreadingData threadingData;
        if (types.Any(static x => x == AL_DriveType.Other))
        {
            threadingData = new ThreadingData(threadCount ?? 1, IOThreadingLevel.Normal);
        }
        else if (types.All(static x => x == AL_DriveType.NVMe_SSD))
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
