using System;
using System.Text;

namespace FFmpeg.NET
{
    internal static class FFmpegArgumentBuilder
    {
        public static string Build(FFmpegParameters parameters)
        {
            if (parameters.HasCustomArguments)
                return parameters.CustomArguments;

            return Convert(parameters.InputFile, parameters.OutputFile, parameters.ConversionOptions);
        }

        private static string Convert(MediaFile inputFile, MediaFile outputFile, ConversionOptions conversionOptions)
        {
            var commandBuilder = new StringBuilder();

            // Default conversion
            if (conversionOptions == null)
                return commandBuilder.AppendFormat(" -i \"{0}\" \"{1}\" ", inputFile.FileInfo.FullName, outputFile.FileInfo.FullName).ToString();

            if (conversionOptions.HideBanner) commandBuilder.Append(" -hide_banner ");

            commandBuilder.AppendFormat(" -i \"{0}\" ", inputFile.FileInfo.FullName);

            #region Audio
            // Audio bit rate
            if (conversionOptions.AudioBitRate != null)
                commandBuilder.AppendFormat(" -ab {0}k", conversionOptions.AudioBitRate);

            #endregion

            if (conversionOptions.MapMetadata) commandBuilder.Append(" -map_metadata 0 ");

            // Extra arguments
            if (conversionOptions.ExtraArguments != null)
                commandBuilder.AppendFormat(" {0} ", conversionOptions.ExtraArguments);

            return commandBuilder.AppendFormat(" \"{0}\" ", outputFile.FileInfo.FullName).ToString();
        }
    }
}
