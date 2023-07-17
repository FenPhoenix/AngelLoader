using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using AL_Common;
using AngelLoader.DataClasses;
using JetBrains.Annotations;
using static AL_Common.Common;
using static AL_Common.LanguageSupport;
using static AL_Common.Logger;
using static AngelLoader.GameSupport;
using static AngelLoader.Global;
using static AngelLoader.NativeCommon;

namespace AngelLoader;

public static partial class Utils
{
    // Depend on non-defined symbol if we're in public release profile, to prevent the bloat of calls to this
#if ReleasePublic || NoAsserts
    [Conditional("_AngelLoader_In_Release_Public_Mode")]
#endif
    [PublicAPI]
    [AssertionMethod]
    public static void AssertR(
        [AssertionCondition(AssertionConditionType.IS_TRUE)]
        bool condition,
        string message,
        string detailedMessage = "")
        => Trace.Assert(condition, message, detailedMessage);

    #region Numeric

    #region Clamping

    internal static float ClampToRichTextBoxZoomMinMax(this float value) => value.Clamp(0.1f, 5.0f);

    internal static float ClampToFMsDGVFontSizeMinMax(this float value)
    {
        if (value < Math.Round(1.00f, 2)) value = 1.00f;
        if (value > Math.Round(41.25f, 2)) value = 41.25f;
        return (float)Math.Round(value, 2);
    }

    internal static int SetRatingClamped(this int rating) => rating.Clamp(-1, 10);

    #endregion

    internal static int MathMax3(int num1, int num2, int num3) => Math.Max(Math.Max(num1, num2), num3);

    internal static int MathMax4(int num1, int num2, int num3, int num4) => Math.Max(Math.Max(Math.Max(num1, num2), num3), num4);

    internal static float CubicRoot(float x) => (float)Math.Pow(x, 1f / 3f);

    #endregion

    #region FM utils

    /// <summary>
    /// If FM's archive name is non-blank, returns it; otherwise, returns FM's installed dir name.
    /// </summary>
    /// <param name="fm"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static string GetFMId(FanMission fm) => !fm.Archive.IsEmpty() ? fm.Archive : fm.InstalledDir;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void SetFMResource(FanMission fm, CustomResources resource, bool value)
    {
        if (value) { fm.Resources |= resource; } else { fm.Resources &= ~resource; }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool FMHasResource(FanMission fm, CustomResources resource) => (fm.Resources & resource) == resource;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool FMNeedsScan(FanMission fm) => !fm.MarkedUnavailable && (fm.Game == Game.Null ||
        (fm.Game != Game.Unsupported && !fm.MarkedScanned));

    /// <summary>
    /// Returns true if <paramref name="fm"/> is actually installed on disk. Specifically, it checks only if
    /// the FM's installed folder exists in the expected place, and not whether the folder contains a complete
    /// and validly structured FM or even if it contains anything at all. Validity is assumed.
    /// </summary>
    /// <param name="fm"></param>
    /// <param name="fmInstalledPath">If the FM exists on disk, the FM's installed path; otherwise, the empty string.</param>
    /// <returns></returns>
    internal static bool FMIsReallyInstalled(FanMission fm, out string fmInstalledPath)
    {
        fmInstalledPath = "";

        if (!fm.Game.ConvertsToKnownAndSupported(out GameIndex gameIndex) || !fm.Installed)
        {
            return false;
        }

        string instPath = Config.GetFMInstallPath(gameIndex);
        return !instPath.IsEmpty() &&
               TryCombineDirectoryPathAndCheckExistence(instPath, fm.InstalledDir, out fmInstalledPath);
    }

    #endregion

    #region Enum HasFlagFast

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool HasFlagFast(this FinishedState @enum, FinishedState flag) => (@enum & flag) == flag;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool HasFlagFast(this Game @enum, Game flag) => (@enum & flag) == flag;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool HasFlagFast(this Language @enum, Language flag) => (@enum & flag) == flag;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool HasFlagFast(this Difficulty @enum, Difficulty flag) => (@enum & flag) == flag;

    #endregion

    /// <summary>
    /// Converts a 32-bit or 64-bit Unix date string in hex format to a nullable DateTime object.
    /// </summary>
    /// <param name="unixDate"></param>
    /// <returns>A DateTime object, or null if the string couldn't be converted to a valid date for any reason.</returns>
    internal static DateTime? ConvertHexUnixDateToDateTime(string unixDate)
    {
        bool success = long.TryParse(
            unixDate,
            NumberStyles.HexNumber,
            DateTimeFormatInfo.InvariantInfo,
            out long result);

        if (success)
        {
            try
            {
                return DateTimeOffset.FromUnixTimeSeconds(result).DateTime.ToLocalTime();
            }
            catch (ArgumentOutOfRangeException)
            {
                return null;
            }
        }

        return null;
    }

    internal static bool GameIsRunning(string gameExe, bool checkAllGames = false)
    {
        #region Local functions

        string t2MPExe = "";
        string T2MPExe()
        {
            if (!t2MPExe.IsEmpty()) return t2MPExe;
            if (Config.GetGameExe(GameIndex.Thief2).IsEmpty()) return "";
            string t2Path = Config.GetGamePath(GameIndex.Thief2);
            return t2MPExe = t2Path.IsEmpty() ? "" : Path.Combine(t2Path, Paths.T2MPExe);
        }

        static bool AnyGameRunning(string path)
        {
            for (int i = 0; i < SupportedGameCount; i++)
            {
                string exe = Config.GetGameExe((GameIndex)i);
                if (!exe.IsEmpty() && path.PathEqualsI(exe)) return true;
            }

            return false;
        }

        static string GetProcessPath(int procId, StringBuilder buffer)
        {
            buffer.Clear();

            using var hProc = OpenProcess(QUERY_LIMITED_INFORMATION, false, procId);
            if (!hProc.IsInvalid)
            {
                int size = buffer.Capacity;
                if (QueryFullProcessImageName(hProc, 0, buffer, ref size)) return buffer.ToString();
            }
            return "";
        }

        #endregion

        var buffer = new StringBuilder(1024);

        // We're doing this whole rigamarole because the game might have been started by someone other than
        // us. Otherwise, we could just persist our process object and then we wouldn't have to do this check.

        Process[] processes = Process.GetProcesses();
        try
        {
            foreach (Process proc in processes)
            {
                try
                {
                    string fn = GetProcessPath(proc.Id, buffer);
                    if (!fn.IsEmpty() &&
                        ((checkAllGames &&
                          (AnyGameRunning(fn) ||
                           (!T2MPExe().IsEmpty() && fn.PathEqualsI(T2MPExe())))) ||
                         (!checkAllGames &&
                          !gameExe.IsEmpty() && fn.PathEqualsI(gameExe))))
                    {
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    // Even if this were to be one of our games, if .NET won't let us find out then all we can do
                    // is shrug and move on.
                    Log(ex: ex);
                }
            }
        }
        finally
        {
            // Are we serious? Do we have this nasty of a gotcha going on here?!
            processes.DisposeAll();
        }

        return false;
    }

    internal static void CancelIfNotDisposed(this CancellationTokenSource value)
    {
        try { value.Cancel(); } catch (ObjectDisposedException) { }
    }

    /// <summary>
    /// Disposes and assigns a new one.
    /// </summary>
    /// <param name="cts"></param>
    /// <returns></returns>
    [MustUseReturnValue]
    internal static CancellationTokenSource Recreate(this CancellationTokenSource cts)
    {
        cts.Dispose();
        return new CancellationTokenSource();
    }

    internal static bool WinVersionIs7OrAbove()
    {
        try
        {
            return Environment.OSVersion.Platform == PlatformID.Win32NT &&
                   Environment.OSVersion.Version >= new Version(6, 1);

            // Win8 check: same but version is 6, 2
        }
        catch
        {
            return false;
        }
    }

    internal static ZipArchive GetZipArchiveCharEnc(string fileName)
    {
        // One user was getting "1 is not a supported code page" with this(?!) so fall back in that case...
        Encoding enc;
        try
        {
            enc = Encoding.GetEncoding(CultureInfo.CurrentCulture.TextInfo.OEMCodePage);
        }
        catch
        {
            enc = Encoding.UTF8;
        }

        return new ZipArchive(File_OpenReadFast(fileName), ZipArchiveMode.Read, leaveOpen: false, enc);
    }

    internal static void LogFMInfo(
        FanMission fm,
        string topMessage,
        Exception? ex = null,
        bool stackTrace = false,
        [CallerMemberName] string callerMemberName = "")
    {
        Log("Caller: " + callerMemberName + "\r\n\r\n" +
            topMessage + "\r\n" +
            "fm." + nameof(fm.Game) + ": " + fm.Game + "\r\n" +
            "fm." + nameof(fm.Archive) + ": " + fm.Archive + "\r\n" +
            "fm." + nameof(fm.InstalledDir) + ": " + fm.InstalledDir + "\r\n" +
            "fm." + nameof(fm.Installed) + ": " + fm.Installed + "\r\n" +
            (fm.Game.ConvertsToKnownAndSupported(out GameIndex gameIndex)
                ? "Base directory for installed FMs: " + Config.GetFMInstallPath(gameIndex)
                : "Game type is not known or not supported.") +
            (ex != null ? "\r\nException:\r\n" + ex : ""), stackTrace: stackTrace);
    }

    internal static bool TryReadAllLines(string file, [NotNullWhen(true)] out List<string>? lines)
    {
        try
        {
            lines = File_ReadAllLines_List(file);
            return true;
        }
        catch (Exception ex)
        {
            Log(ErrorText.ExRead + file, ex);
            lines = null;
            return false;
        }
    }

    internal static bool TryWriteAllLines(string file, List<string> lines)
    {
        try
        {
            File.WriteAllLines(file, lines);
            return true;
        }
        catch (Exception ex)
        {
            Log(ErrorText.ExWrite + file, ex);
            return false;
        }
    }

    internal static Font GetMicrosoftSansSerifDefault() => new("Microsoft Sans Serif", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 0);

#if DateAccTest
    internal static string DateAccuracy_Serialize(DateAccuracy da) => da switch
    {
        DateAccuracy.Green => "Green",
        DateAccuracy.Yellow => "Yellow",
        DateAccuracy.Red => "Red",
        _ => "Null"
    };

    internal static DateAccuracy DateAccuracy_Deserialize(string str) => str switch
    {
        "Green" => DateAccuracy.Green,
        "Yellow" => DateAccuracy.Yellow,
        "Red" => DateAccuracy.Red,
        _ => DateAccuracy.Null
    };
#endif
}
