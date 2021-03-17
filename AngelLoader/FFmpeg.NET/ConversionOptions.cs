using System;
using FFmpeg.NET.Enums;

namespace FFmpeg.NET
{
    public class ConversionOptions
    {

        /// <summary>
        ///     Hide Banner
        /// </summary>
        public bool HideBanner { get; set; } = false;

        /// <summary>
        ///     Set the number of threads to be used, in case the selected codec implementation supports multi-threading.
        ///     Possible values:
        ///         - 0 - automatically select the number of threads to set
        ///         - integer to max of cpu cores
        ///     Default value is ‘0’.
        /// </summary>
        public int Threads { get; set; } = 0;

        /// <summary>
        ///     Audio bit rate
        /// </summary>
        public int? AudioBitRate { get; set; } = null;

        /// <summary>
        ///     Remove Audio
        /// </summary>
        public bool RemoveAudio { get; set; } = false;

        /// <summary>
        ///     Audio sample rate
        /// </summary>
        public AudioSampleRate AudioSampleRate { get; set; } = AudioSampleRate.Default;

        /// <summary>
        ///     Map Metadata from INput to Output
        /// </summary>
        public bool MapMetadata { get; set; } = true;

        /// <summary>
        ///     Extra Arguments, such as  -movflags +faststart. Can be used to support missing features temporary
        /// </summary>
        public string ExtraArguments { get; set; }
    }
}
