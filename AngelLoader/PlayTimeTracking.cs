using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AL_Common;
using AngelLoader.DataClasses;
using static AngelLoader.GameSupport;
using static AngelLoader.Global;
using static AngelLoader.NativeCommon;
using static AngelLoader.Utils;

namespace AngelLoader;

/*
@PlayTimeTracking: Remove Trace.WriteLines and other debug/test code
@PlayTimeTracking: Decide how to handle running through Steam:
Steam tracks the playtime itself, and furthermore if we tracked the runtime of the passed exe, we'd be tracking
Steam's runtime, which would very likely greatly exceed the game. Probably many people would just leave Steam
running indefinitely. Maybe we wait for the game to start and then link our Process object to it?
*/
public sealed class TimeTrackingProcess(GameIndex gameIndex)
{
    private readonly GameIndex _gameIndex = gameIndex;

    internal string FMInstalledDir { get; private set; } = "";

    /*
    Processes have StartTime and EndTime properties, but those can cross timezones / DST and whatever else, so
    let's just time it with a stopwatch.
    Also since we track TDM per-selected-FM rather than per-app-run, we can't use the Process start/end times
    anyway.
    */
    private readonly Stopwatch _stopwatch = new();
    private Process? _process;

    public bool IsRunning { get; private set; }

    public async Task Start(
        ProcessStartInfo startInfo,
        FanMission fm,
        bool steam,
        string gameExe)
    {
        try
        {
            startInfo.UseShellExecute = true;

            // Real installed dir even for TDM, because TDM's unique id installed dir might change after an FM find
            FMInstalledDir = fm.RealInstalledDir;

            _process?.Dispose();

            if (steam)
            {
                ProcessStart_UseShellExecute(startInfo);
                _process = await WaitForProcessAndReturn(gameExe, CancellationToken.None);
                _process.EnableRaisingEvents = true;

                _process.Exited += Process_Exited;
                _stopwatch.Restart();
                IsRunning = true;
            }
            else
            {
                _process = new Process();
                _process.StartInfo = startInfo;
                _process.EnableRaisingEvents = true;

                _process.Exited += Process_Exited;
                _process.Start();
                _stopwatch.Restart();
                IsRunning = true;
            }
        }
        catch
        {
            IsRunning = false;
            _stopwatch.Reset();
            _process?.Dispose();
            FMInstalledDir = "";
            _process = null;
            throw;
        }
    }

    // @PlayTimeTracking(Switch TDM FM): Make it work None->FM
    // Works FM->FM, FM->None, and FM->None->FM
    public void SwitchTdmFM(string? fmInstalledDir)
    {
        _stopwatch.Stop();
        Update(_stopwatch.Elapsed);
        if (fmInstalledDir.IsEmpty())
        {
            // Do not set running to false - we're still running, just with no FM!
            _stopwatch.Reset();
            FMInstalledDir = "";
        }
        else
        {
            IsRunning = true;
            FMInstalledDir = fmInstalledDir;
            _stopwatch.Restart();
        }
    }

    private static async Task<Process> WaitForProcessAndReturn(string fullPath, CancellationToken cancellationToken)
    {
        var buffer = new StringBuilder(1024);

        /*
        @PlayTimeTracking: Dumb infinite loop - give it a timeout and robustify etc.
        Notes on this:
        -A timeout is difficult because Steam could start and then update for a reasonably substantial amount of
         time. Also, in theory, Steam could start, not have the requested game in the user's library, and then
         the game never starts but we're still just waiting forever.
        -We could make the timeout be fairly long, like a minute or two, and then pop up a dialog asking the user
         if they want to continue waiting (telling them that we need to wait for the game to track play time).
         We'd also need to use the cancellation token if we're about to start the game while still waiting for
         the last game start request, and make sure we wait for it to finish canceling before going further.
        */
        while (true)
        {
            Process[] processes = Process.GetProcesses();

            cancellationToken.ThrowIfCancellationRequested();

            Process? returnProcess = null;
            try
            {
                foreach (Process proc in processes)
                {
                    try
                    {
                        string fn = GetProcessPath(proc.Id, buffer);
                        if (!string.IsNullOrEmpty(fn) && fn.PathEqualsI(fullPath))
                        {
                            returnProcess = proc;
                            return proc;
                        }
                    }
                    catch
                    {
                        // ignore
                    }

                    cancellationToken.ThrowIfCancellationRequested();
                }
            }
            finally
            {
                foreach (Process process in processes)
                {
                    if (!process.EqualsIfNotNull(returnProcess))
                    {
                        process.Dispose();
                    }
                }
            }
            await Task.Delay(1000, cancellationToken);
        }

        static string GetProcessPath(int procId, StringBuilder buffer)
        {
            buffer.Clear();

            using var hProc = OpenProcess(QUERY_LIMITED_INFORMATION, false, procId);
            if (!hProc.IsInvalid)
            {
                int size = buffer.Capacity;
                if (QueryFullProcessImageNameW(hProc, 0, buffer, ref size)) return buffer.ToString();
            }
            return "";
        }
    }

    private void Process_Exited(object sender, EventArgs e)
    {
        Trace.WriteLine("Top of Process_Exited");

        IsRunning = false;
        TimeSpan elapsed = _stopwatch.Elapsed;
        _stopwatch.Reset();
        _process?.Dispose();
        _process = null;
        Update(elapsed);
        FMInstalledDir = "";
    }

    private void Update(TimeSpan elapsed) => Core.View.Invoke(() =>
    {
        Trace.WriteLine("Top of Update()");

        if (FMInstalledDir.IsEmpty()) return;

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

        Core.View.RefreshFMsListRowsOnlyKeepSelection();

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