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

    internal static ThreadingData GetLowestCommonThreadingData(List<string> paths)
    {
        ThreadingData threadingData;
        // @MT_TASK: We can't just have one "Custom" anymore... we have different scenarios: install, audio conv, scan, etc...
        if (Config.IOThreadingLevel == IOThreadingLevel.Custom)
        {
            threadingData = new ThreadingData(Config.CustomIOThreads, Config.CustomIOThreadingMode);
        }
        else
        {
            List<AL_DriveType> types = DetectDriveTypes.GetAllDrivesType(paths);

            if (types.Any(static x => x == AL_DriveType.Other))
            {
                threadingData = new ThreadingData(1, IOThreadingMode.Normal);
            }
            else if (types.All(static x => x == AL_DriveType.NVMe_SSD))
            {
                threadingData = new ThreadingData(CoreCount, IOThreadingMode.Aggressive);
            }
            else
            {
                threadingData = new ThreadingData(CoreCount, IOThreadingMode.Normal);
            }
        }

#if TESTING
        System.Diagnostics.Trace.WriteLine(nameof(GetLowestCommonThreadingData) + ": " + threadingData);
#endif

        return threadingData;
    }
}
