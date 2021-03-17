using FFmpeg.NET.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace FFmpeg.NET
{
    /// <summary>
    ///     Contains all Regex tasks
    /// </summary>
    internal static class RegexEngine
    {
        /// <summary>
        ///     Dictionary containing every Regex test.
        /// </summary>
        internal static readonly Dictionary<Find, Regex> _index = new Dictionary<Find, Regex>
        {
            {Find.BitRate, new Regex(@"([0-9]*)\s*kb/s")},
            {Find.Duration, new Regex(@"Duration: ([^,]*), ")},
            {Find.ConvertProgressFrame, new Regex(@"frame=\s*([0-9]*)")},
            {Find.ConvertProgressFps, new Regex(@"fps=\s*([0-9]*\.?[0-9]*?)")},
            {Find.ConvertProgressSize, new Regex(@"size=\s*([0-9]*)kB")},
            {Find.ConvertProgressFinished, new Regex(@"Lsize=\s*([0-9]*)kB")},
            {Find.ConvertProgressTime, new Regex(@"time=\s*([^ ]*)")},
            {Find.ConvertProgressBitrate, new Regex(@"bitrate=\s*([0-9]*\.?[0-9]*?)kbits/s")},
            {Find.MetaAudio, new Regex(@"(Stream\s*#[0-9]*:[0-9]*\(?[^\)]*?\)?: Audio:.*)")},
            {Find.AudioFormatHzChannel, new Regex(@"Audio:\s*([^,]*),\s([^,]*),\s([^,]*)")},
        };

        /// <summary>
        ///     <para> ---- </para>
        ///     <para>Establishes whether the data contains progress information.</para>
        /// </summary>
        /// <param name="data">Event data from the FFmpeg console.</param>
        /// <param name="progressData">
        ///     <para>If successful, outputs a <see cref="ProgressData" /> which is </para>
        ///     <para>generated from the data. </para>
        /// </param>
        internal static bool IsProgressData(string data, out ProgressData progressData)
        {
            progressData = null;

            var matchFrame = _index[Find.ConvertProgressFrame].Match(data);
            var matchFps = _index[Find.ConvertProgressFps].Match(data);
            var matchSize = _index[Find.ConvertProgressSize].Match(data);
            var matchTime = _index[Find.ConvertProgressTime].Match(data);
            var matchBitrate = _index[Find.ConvertProgressBitrate].Match(data);

            if (!matchTime.Success)
                return false;

            TimeSpanLargeTryParse(matchTime.Groups[1].Value, out var processedDuration);

            var frame = GetLongValue(matchFrame);
            var fps = GetDoubleValue(matchFps);
            var sizeKb = GetIntValue(matchSize);
            var bitrate = GetDoubleValue(matchBitrate);

            progressData = new ProgressData(processedDuration, TimeSpan.Zero, frame, fps, sizeKb, bitrate);

            return true;
        }

		// Parse timespan string as returned by ffmpeg. No days, allow hours to
		// exceed 23.
		internal static bool TimeSpanLargeTryParse(string str, out TimeSpan result)
		{
			result = TimeSpan.Zero;

            // Process hours.
            int hours = 0;
            int start = 0;
            int end = str.IndexOf(':', start);
            if (end < 0)
                return false;
            if (!int.TryParse(str.Substring(start, end - start), out hours))
                return false;

            // Process minutes
            int minutes = 0;
            start = end + 1;
            end = str.IndexOf(':', start);
            if (end < 0)
                return false;
            if (!int.TryParse(str.Substring(start, end - start), out minutes))
                return false;

            // Process seconds
            double seconds = 0.0;
            start = end + 1;
            // ffmpeg doesnt respect the computers culture
            if (!double.TryParse(str.Substring(start), NumberStyles.Number, CultureInfo.InvariantCulture, out seconds))
                return false;

            result = new TimeSpan(0, hours, minutes, 0, (int)Math.Round(seconds * 1000.0));
            return true;
		}

        private static long? GetLongValue(Match match)
            => long.TryParse(match.Groups[1].Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var result)
                ? result
                : (long?)null;

        private static double? GetDoubleValue(Match match)
            => double.TryParse(match.Groups[1].Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var result)
                ? result
                : (double?)null;

        private static int? GetIntValue(Match match)
            => int.TryParse(match.Groups[1].Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var result)
                ? result
                : (int?)null;

        internal static void TestAudio(string data, FFmpegParameters engine)
        {
            var matchMetaAudio = _index[Find.MetaAudio].Match(data);

            if (!matchMetaAudio.Success)
                return;

            var fullMetadata = matchMetaAudio.Groups[1].ToString();

            var matchAudioFormatHzChannel = _index[Find.AudioFormatHzChannel].Match(fullMetadata).Groups;
            var matchAudioBitRate = _index[Find.BitRate].Match(fullMetadata).Groups;

            if (engine.InputFile.MetaData == null)
                engine.InputFile.MetaData = new MetaData();

            if (engine.InputFile.MetaData.AudioData == null)
                engine.InputFile.MetaData.AudioData = new MetaData.Audio
                {
                    Format = matchAudioFormatHzChannel[1].ToString(),
                    SampleRate = matchAudioFormatHzChannel[2].ToString(),
                    ChannelOutput = matchAudioFormatHzChannel[3].ToString(),
                    BitRateKbs = !string.IsNullOrWhiteSpace(matchAudioBitRate[1].ToString()) ? Convert.ToInt32(matchAudioBitRate[1].ToString()) : 0
                };
        }

        internal enum Find
        {
            AudioFormatHzChannel,
            ConvertProgressBitrate,
            ConvertProgressFps,
            ConvertProgressFrame,
            ConvertProgressSize,
            ConvertProgressFinished,
            ConvertProgressTime,
            Duration,
            MetaAudio,
            BitRate,
        }
    }
}
