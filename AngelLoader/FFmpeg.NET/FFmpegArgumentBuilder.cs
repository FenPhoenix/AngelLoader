using System;
using System.Text;
using FFmpeg.NET.Enums;

namespace FFmpeg.NET
{
    internal static class FFmpegArgumentBuilder
    {
        public static string Build(FFmpegParameters parameters)
        {
            if (parameters.HasCustomArguments)
                return parameters.CustomArguments;

            switch (parameters.Task)
            {
                case FFmpegTask.Convert:
                    return Convert(parameters.InputFile, parameters.OutputFile, parameters.ConversionOptions);

                case FFmpegTask.GetMetaData:
                    return GetMetadata(parameters.InputFile);

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static string GetMetadata(MediaFile inputFile) => $"-i \"{inputFile.FileInfo.FullName}\" -f ffmetadata -";

        private static string Convert(MediaFile inputFile, MediaFile outputFile, ConversionOptions conversionOptions)
        {
            var commandBuilder = new StringBuilder();

            // Default conversion
            if (conversionOptions == null)
                return commandBuilder.AppendFormat(" -i \"{0}\" \"{1}\" ", inputFile.FileInfo.FullName, outputFile.FileInfo.FullName).ToString();

            if (conversionOptions.HideBanner) commandBuilder.Append(" -hide_banner ");

            if (conversionOptions.Threads != 0)
            {
                commandBuilder.AppendFormat(" -threads {0} ", conversionOptions.Threads);
            }

            commandBuilder.AppendFormat(" -i \"{0}\" ", inputFile.FileInfo.FullName);

            #region Audio
            // Audio bit rate
            if (conversionOptions.AudioBitRate != null)
                commandBuilder.AppendFormat(" -ab {0}k", conversionOptions.AudioBitRate);

            // Audio sample rate
            if (conversionOptions.AudioSampleRate != AudioSampleRate.Default)
                commandBuilder.AppendFormat(" -ar {0} ", conversionOptions.AudioSampleRate.ToString().Replace("Hz", ""));

            // Remove Audio
            if (conversionOptions.RemoveAudio)
                commandBuilder.Append(" -an ");
            #endregion

            if (conversionOptions.MapMetadata) commandBuilder.Append(" -map_metadata 0 ");

            // Extra arguments
            if (conversionOptions.ExtraArguments != null)
                commandBuilder.AppendFormat(" {0} ", conversionOptions.ExtraArguments);

            return commandBuilder.AppendFormat(" \"{0}\" ", outputFile.FileInfo.FullName).ToString();
        }
    }
}
