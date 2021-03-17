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

namespace AngelLoader.FFmpeg.NET
{
    public enum ConvertType
    {
        AudioBitRateTo16Bit,
        FormatConvert
    }

    public sealed class Engine
    {
        private readonly string _ffmpegPath;

        public Engine(string ffmpegPath)
        {
            if (!File.Exists(ffmpegPath))
            {
                throw new ArgumentException("FFmpeg executable not found", ffmpegPath);
            }

            _ffmpegPath = ffmpegPath;
        }

        public async Task ConvertAsync(string input, string output, ConvertType convertType)
        {
            string arguments = convertType == ConvertType.FormatConvert
                ? " -i \"" + input + "\" \"" + output + "\" "
                : " -i \"" + input + "\" -ab 16k -map_metadata 0 \"" + output + "\" ";

            var startInfo = new ProcessStartInfo
            {
                // -y overwrite output files
                Arguments = "-y " + arguments,
                FileName = _ffmpegPath,
                CreateNoWindow = true,
                UseShellExecute = false
            };

            using var ffmpegProcess = new Process { StartInfo = startInfo };
            await WaitForExitAsync(ffmpegProcess);
        }

        private static Task<int> WaitForExitAsync(Process process)
        {
            var tcs = new TaskCompletionSource<int>();

            void processOnExited(object sender, EventArgs e)
            {
                process.WaitForExit();
                tcs.TrySetResult(process.ExitCode);
                process.Exited -= processOnExited;
            }

            process.EnableRaisingEvents = true;
            process.Exited += processOnExited;

            bool started = process.Start();
            if (!started)
            {
                tcs.TrySetException(new InvalidOperationException("Could not start process " + process));
            }

            return tcs.Task;
        }
    }
}
