using System;
using System.IO;

namespace FFmpeg.NET
{
    public class MetaData
    {
        internal MetaData()
        {
        }

        public TimeSpan Duration { get; internal set; }
        public Audio AudioData { get; internal set; }
        public FileInfo FileInfo { get; internal set; }

        public override string ToString() 
            => $"Duration: {Duration}\nAudio MetaData:\n{AudioData}";

        public class Audio
        {
            internal Audio()
            {
            }

            public string Format { get; internal set; }
            public string SampleRate { get; internal set; }
            public string ChannelOutput { get; internal set; }
            public int BitRateKbs { get; internal set; }

            public override string ToString() 
                => $"Format: {Format}\nSampleRate: {SampleRate}\nChannelOuput: {ChannelOutput}\nBitRateKbs: {BitRateKbs}";
        }
    }
}