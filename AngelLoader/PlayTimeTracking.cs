//#define TESTING

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using AngelLoader.DataClasses;
using static AngelLoader.GameSupport;
using static AngelLoader.Global;
using static AngelLoader.Misc;
using static AngelLoader.NativeCommon;
using static AngelLoader.Utils;

namespace AngelLoader;

/*
@PlayTimeTracking: Should we put the play time on the Stats tab too?
*/
public sealed class TimeTrackingProcess(GameIndex gameIndex)
{
    private readonly GameIndex _gameIndex = gameIndex;

    internal string FMInstalledDir { get; private set; } = "";

    /*
    Processes have StartTime and ExitTime properties, but those can cross timezones / DST and whatever else, so
    let's just time it with a stopwatch.
    Also since we track TDM per-selected-FM rather than per-app-run, we can't use the Process start/exit times
    anyway.
    */
    private readonly Stopwatch _stopwatch = new();
    private Process? _process;

    public bool IsRunning { get; private set; }

    private CancellationTokenSource _steamGameStartCTS = new();
    private void CancelSteamWait() => _steamGameStartCTS.Cancel();

    public async Task<bool> Start(
        ProcessStartInfo startInfo,
        FanMission fm,
        bool steam,
        string gameExe)
    {
        try
        {
            InitStart(startInfo, fm);

            if (steam)
            {
                ProcessStart_UseShellExecute(startInfo);

                try
                {
                    _steamGameStartCTS = _steamGameStartCTS.Recreate();

                    Core.View.SetWaitCursor(false);

                    Core.View.ShowProgressBox_Single(
                        message1: LText.ProgressBox.WaitingForSteamToStartTheGame,
                        message2: LText.ProgressBox.WaitingForSteamToStartTheGame_Explanation,
                        progressType: ProgressType.Indeterminate,
                        cancelMessage: LText.Global.Cancel,
                        cancelAction: CancelSteamWait
                    );

                    _process = await WaitForAndReturnProcess(gameExe, _steamGameStartCTS.Token);

                    _process.EnableRaisingEvents = true;

                    _process.Exited += Process_Exited;
                    _stopwatch.Restart();
                    IsRunning = true;
                    return true;
                }
                catch (OperationCanceledException)
                {
                    HandleStartFailure();
                    // @PlayTimeTracking: Should we really return false on cancel though?
                    // If the user clicks Cancel and the game DOES end up starting, we won't even update the last
                    // played time. It's the least likely situation though.
                    return false;
                }
                finally
                {
                    Core.View.HideProgressBox();
                    _steamGameStartCTS.Dispose();
                }
            }
            else
            {
                StartProcessNonSteam(startInfo);
                _stopwatch.Restart();
                return true;
            }
        }
        catch
        {
            HandleStartFailure();
            throw;
        }
    }

    public void StartTdmWithNoFM(ProcessStartInfo startInfo)
    {
        InitStart(startInfo, null);

        try
        {
            StartProcessNonSteam(startInfo);
        }
        catch
        {
            HandleStartFailure();
            throw;
        }
    }

    private void InitStart(ProcessStartInfo startInfo, FanMission? fm)
    {
        startInfo.UseShellExecute = true;

        // Real installed dir even for TDM, because TDM's unique id installed dir might change after an FM find
        FMInstalledDir = fm?.RealInstalledDir ?? "";

        _process?.Dispose();
    }

    private void StartProcessNonSteam(ProcessStartInfo startInfo)
    {
        _process = new Process();
        _process.StartInfo = startInfo;
        _process.EnableRaisingEvents = true;

        _process.Exited += Process_Exited;
        _process.Start();
        IsRunning = true;
    }

    private void HandleStartFailure()
    {
        IsRunning = false;
        _stopwatch.Reset();
        _process?.Dispose();
        FMInstalledDir = "";
        _process = null;
    }

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
            FMInstalledDir = fmInstalledDir;
            _stopwatch.Restart();
        }
    }

    private static async Task<Process> WaitForAndReturnProcess(string fullPath, CancellationToken cancellationToken)
    {
        while (true)
        {
            Process[] processes = Process.GetProcesses();

            Process? returnProcess = null;
            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                foreach (Process proc in processes)
                {
                    try
                    {
                        string? fn = GetProcessPath(proc.Id);
                        if (!fn.IsEmpty() && fn.PathEqualsI(fullPath))
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
    }

    private void Process_Exited(object? sender, EventArgs e)
    {
#if TESTING
        Trace.WriteLine("Top of Process_Exited");
#endif

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
#if TESTING
        Trace.WriteLine("Top of Update()");
#endif

        if (FMInstalledDir.IsEmpty()) return;

        List<FanMission> fmsList = _gameIndex == GameIndex.TDM ? FMDataIniListTDM : FMDataIniList;

#if TESTING
        Trace.WriteLine(FMInstalledDir);
#endif

        FanMission? fm = fmsList.Find(x => x.RealInstalledDir.EqualsI(FMInstalledDir));
        if (fm == null)
        {
#if TESTING
            Trace.WriteLine("null?!");
#endif
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

#if TESTING
        Trace.WriteLine(fm.PlayTime);
#endif
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
