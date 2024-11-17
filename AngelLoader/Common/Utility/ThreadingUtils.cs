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

    // @MT_TASK: Maybe make these take IEnumerable so as not to have to convert back and forth all the time
    // @MT_TASK: Thoroughly test all threadable-path codepaths now that we have this fully granular system
    internal static void FillThreadablePaths(List<ThreadablePath> paths)
    {
        ThreadablePath[] threadablePathsArray = paths.ToArray();
        DetectDriveTypes.GetAllDrivesType(threadablePathsArray, Config.DriveLettersAndTypes);
    }

    internal static ThreadingData GetLowestCommonThreadingData(
        List<ThreadablePath> paths
#if TESTING
        , [System.Runtime.CompilerServices.CallerMemberName] string caller = ""
#endif
        )
    {
        int? threadCount = null;

        if (Config.IOThreadingMode == IOThreadingMode.Custom)
        {
            threadCount = Config.CustomIOThreads;
        }

        ThreadingData threadingData;
        if (paths.Any(static x => x.DriveType == AL_DriveType.Other))
        {
            threadingData = new ThreadingData(threadCount ?? 1, IOThreadingLevel.Normal);
        }
        else if (paths.All(static x => x.DriveType == AL_DriveType.NVMe_SSD))
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
}
