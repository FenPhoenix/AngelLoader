using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using AngelLoader.Common;
using AngelLoader.Common.DataClasses;
using AngelLoader.Common.Utility;
using AngelLoader.CustomControls;
using FFmpeg.NET;
using FMScanner;
using SevenZip;
using static AngelLoader.Ini.Ini;
using static AngelLoader.Common.Utility.Methods;
using CompressionLevel = SevenZip.CompressionLevel;
using CompressionMode = SevenZip.CompressionMode;

namespace AngelLoader
{
    internal sealed class BackupAndRestore
    {
        private readonly FanMission FM;
        private readonly string FMInstallsBasePath;
        private readonly string FMsBackupPath;

        internal BackupAndRestore(FanMission fm, string fmInstallsBasePath, string fmsBackupPath)
        {
            FM = fm;
            FMInstallsBasePath = fmInstallsBasePath;
            FMsBackupPath = fmsBackupPath;
        }

        internal async Task BackupSavesAndScreenshots()
        {
            // TODO: Implement Thief 3 save backups
            if (FM.Game != Game.Thief1 && FM.Game != Game.Thief2) return;

            await Task.Run(() =>
            {
                if (FM.InstalledDir.IsEmpty()) return;

                var savesPath = Path.Combine(FMInstallsBasePath, FM.InstalledDir, "saves");
                var screensPath = Path.Combine(FMInstallsBasePath, FM.InstalledDir, "screenshots");

                bool savesPathExists = Directory.Exists(savesPath);
                bool screensPathExists = Directory.Exists(screensPath);

                if (!savesPathExists && !screensPathExists) return;

                var bakFile =
                    Path.Combine(FMsBackupPath,
                        (FM.Archive.RemoveExtension() ?? FM.InstalledDir) + Paths.FMBackupSuffix);

                Paths.PrepareTempPath(Paths.CompressorTemp);
                try
                {
                    var compressor = new SevenZipCompressor(Paths.CompressorTemp)
                    {
                        ArchiveFormat = OutArchiveFormat.Zip,
                        CompressionLevel = CompressionLevel.Normal,
                        PreserveDirectoryRoot = true
                    };

                    if (savesPathExists &&
                        Directory.GetFiles(savesPath, "*", SearchOption.AllDirectories).Length > 0)
                    {
                        compressor.CompressDirectory(savesPath, bakFile);
                        compressor.CompressionMode = CompressionMode.Append;
                    }

                    if (screensPathExists &&
                        Directory.GetFiles(screensPath, "*", SearchOption.AllDirectories).Length > 0)
                    {
                        compressor.CompressDirectory(screensPath, bakFile);
                    }
                }
                finally
                {
                    // Clean up after ourselves, just in case something went wrong and SevenZipCompressor didn't.
                    // We don't want to be like some apps that pile junk in the temp folder and never delete it.
                    // We're a good temp folder citizen.
                    Paths.PrepareTempPath(Paths.CompressorTemp);
                }
            });
        }

        internal async Task RestoreSavesAndScreenshots()
        {
            // TODO: Implement Thief 3 save restore here
            if (FM.Game != Game.Thief1 && FM.Game != Game.Thief2) return;

            await Task.Run(() =>
            {
                var bakFiles = new List<FileInfo>();

                // Our backup path, separate to avoid creating any more ambiguity
                var bakFile =
                    Path.Combine(FMsBackupPath, FM.Archive.RemoveExtension() + Paths.FMBackupSuffix);

                if (File.Exists(bakFile)) bakFiles.Add(new FileInfo(bakFile));

                // But also search all archive dirs for compatibility with FMSel / NDL backups
                foreach (var path in GetFMArchivePaths())
                {
                    bakFile = Path.Combine(path, FM.Archive.RemoveExtension() + Paths.FMBackupSuffix);
                    if (File.Exists(bakFile)) bakFiles.Add(new FileInfo(bakFile));
                }

                if (bakFiles.Count == 0) return;

                // Just use the newest file
                bakFiles = bakFiles.OrderByDescending(x => x.LastWriteTime).ToList();
                var newestBakFile = bakFiles[0].FullName;

                using (var extractor = new SevenZipExtractor(newestBakFile))
                {
                    extractor.ExtractArchive(Path.Combine(FMInstallsBasePath, FM.InstalledDir));
                }
            });
        }
    }
}
