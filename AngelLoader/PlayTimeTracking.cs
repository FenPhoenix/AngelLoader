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

                    // @PlayTimeTracking: Put some explanation that the wait is needed to track FM play time
                    Core.View.ShowProgressBox_Single(
                        message1: LText.ProgressBox.WaitingForSteamToStartTheGame,
                        progressType: Misc.ProgressType.Indeterminate,
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
        var buffer = new StringBuilder(1024);

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