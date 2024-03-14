using System;
using System.Collections.Generic;
using System.Diagnostics;
using AL_Common;
using AngelLoader.DataClasses;
using static AngelLoader.GameSupport;
using static AngelLoader.Global;

namespace AngelLoader;

/*
@PlayTimeTracking: We could get really fancy and switch the tracked FM when TDM changes FM in-game
@PlayTimeTracking: Remove Trace.WriteLines and other debug/test code
@PlayTimeTracking: Decide how to handle running through Steam:
Steam tracks the playtime itself, and furthermore if we tracked the runtime of the passed exe, we'd be tracking
Steam's runtime, which would very likely greatly exceed the game. Probably many people would just leave Steam
running indefinitely. Maybe we wait for the game to start and then link our Process object to it?
*/
public sealed class TimeTrackingProcess(GameIndex gameIndex)
{
    private readonly GameIndex _gameIndex = gameIndex;

    private string FMInstalledDir = "";

    // Processes have StartTime and EndTime properties, but those can cross timezones / DST and whatever else,
    // so let's just time it with a stopwatch.
    // @PlayTimeTracking: Could we fix this with converting to UTC / doing a special compare / whatever else?
    private readonly Stopwatch _stopwatch = new();
    private Process? _process;

    public void Start(ProcessStartInfo startInfo, FanMission fm)
    {
        try
        {
            startInfo.UseShellExecute = true;

            // Real installed dir even for TDM, because TDM's unique id installed dir might change after an FM find
            FMInstalledDir = fm.RealInstalledDir;

            _process?.Dispose();
            _process = new Process();
            _process.StartInfo = startInfo;
            _process.EnableRaisingEvents = true;

            _process.Exited += Process_Exited;
            _process.Start();
            _stopwatch.Restart();
        }
        catch
        {
            _stopwatch.Reset();
            _process?.Dispose();
            FMInstalledDir = "";
            _process = null;
            throw;
        }
    }

    private void Process_Exited(object sender, EventArgs e)
    {
        TimeSpan elapsed = _stopwatch.Elapsed;
        _stopwatch.Reset();
        _process?.Dispose();
        _process = null;
        Update(elapsed);
    }

    private void Update(TimeSpan elapsed) => Core.View.Invoke(() =>
    {
        List<FanMission> fmsList = _gameIndex == GameIndex.TDM ? FMDataIniListTDM : FMDataIniList;

        Trace.WriteLine(FMInstalledDir);

        FanMission? fm = fmsList.Find(x => x.RealInstalledDir.EqualsI(FMInstalledDir));
        if (fm == null)
        {
            Trace.WriteLine("null?!");
            return;
        }

        try
        {
            fm.PlayTime = fm.PlayTime.Add(elapsed);
        }
        catch
        {
            // Out of range... nothing we can do
        }

        Trace.WriteLine(fm.PlayTime);
    });
}

public static class PlayTimeTracking
{
    private static readonly TimeTrackingProcess[] _timeTrackingProcesses = new TimeTrackingProcess[SupportedGameCount];
    internal static TimeTrackingProcess GetTimeTrackingProcess(GameIndex gameIndex) => _timeTrackingProcesses[(int)gameIndex];

    static PlayTimeTracking()
    {
        for (int i = 0; i < SupportedGameCount; i++)
        {
            _timeTrackingProcesses[i] = new TimeTrackingProcess((GameIndex)i);
        }
    }
}