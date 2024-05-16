/*
Fen's note: I mean, well, it's barely recognizable as FFmpeg.NET, but I did start with it as a base, so...

The MIT License (MIT)

Copyright (c) [2018] [Tobias Haimerl (cmxl)]

Permission is hereby granted, free of charge, to any person obtaining a copy of
this software and associated documentation files (the "Software"), to deal in
the Software without restriction, including without limitation the rights to
use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of
the Software, and to permit persons to whom the Software is furnished to do so,
subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using AL_Common;
using AngelLoader.DataClasses;
using static AL_Common.Logger;

namespace AngelLoader.FFmpeg.NET;

internal enum ConvertType
{
    AudioBitRateTo16Bit,
    FormatConvert,
}

internal static class Engine
{
    internal static async Task ConvertAsync(string input, string output, ConvertType convertType)
    {
        if (!File.Exists(Paths.FFmpegExe))
        {
            var ex = new ArgumentException("FFmpeg executable not found", Paths.FFmpegExe);
            LogInfo("", ex, input, output, convertType);
            throw ex;
        }

        // -y overwrite output files
        string arguments = convertType == ConvertType.FormatConvert
            ? "-y -i \"" + input + "\" \"" + output + "\""
            : "-y -i \"" + input + "\" -ab 16k -map_metadata 0 \"" + output + "\"";

        var startInfo = new ProcessStartInfo
        {
            Arguments = arguments,
            FileName = Paths.FFmpegExe,
            CreateNoWindow = true,
            UseShellExecute = false,
        };

        using var ffmpegProcess = new Process();
        ffmpegProcess.StartInfo = startInfo;
        try
        {
            await WaitForExitAsync(ffmpegProcess);
        }
        catch (Exception ex)
        {
            LogInfo(ErrorText.ExTry + "run or exit FFmpeg", ex, input, output, convertType);
            throw;
        }
        return;

        #region Local functions

        static void LogInfo(string topMsg, Exception ex, string input, string output, ConvertType convertType)
        {
            Log((!topMsg.IsEmpty() ? topMsg + "\r\n" : "") +
                nameof(input) + ": " + input + "\r\n" +
                nameof(output) + ": " + output + "\r\n" +
                nameof(convertType) + ": " + convertType,
                ex);
        }

        #endregion
    }

    private static Task<int> WaitForExitAsync(Process process)
    {
        var tcs = new TaskCompletionSource<int>();

        process.EnableRaisingEvents = true;
        process.Exited += ProcessOnExited;

        bool started = process.Start();
        if (!started)
        {
            tcs.TrySetException(new InvalidOperationException("Could not start process " + process));
        }

        return tcs.Task;

        void ProcessOnExited(object? sender, EventArgs e)
        {
            process.WaitForExit();
            tcs.TrySetResult(process.ExitCode);
            process.Exited -= ProcessOnExited;
        }
    }
}
