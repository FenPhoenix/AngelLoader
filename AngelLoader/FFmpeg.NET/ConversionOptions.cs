namespace FFmpeg.NET
{
    public class ConversionOptions
    {
        /// <summary>
        ///     Hide Banner
        /// </summary>
        public bool HideBanner { get; set; } = false;

        /// <summary>
        ///     Audio bit rate
        /// </summary>
        public int? AudioBitRate { get; set; } = null;

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
