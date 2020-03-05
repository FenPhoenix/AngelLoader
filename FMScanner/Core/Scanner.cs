/*
FMScanner - A fast, thorough, accurate scanner for Thief 1 and Thief 2 fan missions.

Written in 2017-2020 by FenPhoenix.

To the extent possible under law, the author(s) have dedicated all copyright and related and neighboring rights
to this software to the public domain worldwide. This software is distributed without any warranty.

You should have received a copy of the CC0 Public Domain Dedication along with this software.
If not, see <http://creativecommons.org/publicdomain/zero/1.0/>.
*/

//#define ScanSynchronous
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using FMScanner.FastZipReader;
using JetBrains.Annotations;
using SevenZip;
using static System.StringComparison;
using static FMScanner.Constants;
using static FMScanner.FMConstants;
using static FMScanner.Logger;
using static FMScanner.Methods;
using static FMScanner.Regexes;

namespace FMScanner
{
    #region Classes

    internal sealed class ReadmeInternal
    {
        internal string FileName { get; set; }
        internal string[] Lines { get; set; }
        internal string Text { get; set; }
        internal DateTime LastModifiedDate { get; set; }
    }

    /// <summary>
    /// Stores a filename/index pair for quick lookups into a zip file.
    /// </summary>
    internal sealed class NameAndIndex
    {
        internal string Name { get; set; }
        internal int Index { get; set; } = -1;
    }

    #endregion

    [SuppressMessage("ReSharper", "ArrangeStaticMemberQualifier")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public class Scanner : IDisposable
    {
        private Stopwatch OverallTimer { get; } = new Stopwatch();

        #region Properties

        /// <summary>
        /// The encoding to use when reading entry names in FM archives. Use this when you need absolute
        /// consistency (like when you're grabbing file names and then looking them back up within the archive
        /// later). Default: <see cref="Encoding.UTF8"/>
        /// </summary>
        [PublicAPI]
        public Encoding ZipEntryNameEncoding { get; set; } = Encoding.UTF8;

        [PublicAPI]
        public string LogFile { get; set; } = "";

        /// <summary>
        /// Hack to support scanning two different sets of fields depending on a bool, you pass in "full" scan
        /// fields here and "non-full" fields in the Scan* methods, and mark each passed FM with a bool.
        /// </summary>
        [PublicAPI]
        public ScanOptions FullScanOptions { get; set; } = new ScanOptions();

        #region Disposable

        private ZipArchiveFast Archive { get; set; }

        #endregion

        private List<FileInfo> FMDirFiles { get; set; } = new List<FileInfo>();

        private char dsc { get; set; }

        private ScanOptions ScanOptions { get; set; } = new ScanOptions();

        private bool FMIsZip { get; set; }

        private string ArchivePath { get; set; }

        private string FMWorkingPath { get; set; }

        // Guess I'll leave this one global for reasons
        private List<ReadmeInternal> ReadmeFiles { get; set; } = new List<ReadmeInternal>();

        #endregion

        private enum SpecialLogic
        {
            None,
            Title,
            Version,
            NewDarkMinimumVersion,
            Author,
            AuthorNextLine
        }

        #region Scan synchronous

        public ScannedFMData
        Scan(string mission, string tempPath, bool forceFullIfNew)
        {
            return ScanMany(
                new List<FMToScan> { new FMToScan { Path = mission, ForceFullScan = forceFullIfNew } },
                tempPath, this.ScanOptions, null, CancellationToken.None)[0];
        }

        public ScannedFMData
        Scan(string mission, string tempPath, ScanOptions scanOptions, bool forceFullIfNew)
        {
            return ScanMany(
                new List<FMToScan> { new FMToScan { Path = mission, ForceFullScan = forceFullIfNew } },
                tempPath, scanOptions, null, CancellationToken.None)[0];
        }

        // Debug - scan on UI thread so breaks will actually break where they're supposed to
        // (test frontend use only)
#if DEBUG || ScanSynchronous
        [PublicAPI]
        public List<ScannedFMData>
        Scan(List<FMToScan> missions, string tempPath, ScanOptions scanOptions,
                IProgress<ProgressReport> progress, CancellationToken cancellationToken)
        {
            return ScanMany(missions, tempPath, scanOptions, progress, cancellationToken);
        }
#endif

        #endregion

        #region Scan asynchronous

        [PublicAPI]
        public async Task<List<ScannedFMData>>
        ScanAsync(List<FMToScan> missions, string tempPath)
        {
            return await Task.Run(() =>
                ScanMany(missions, tempPath, this.ScanOptions, null, CancellationToken.None));
        }

        [PublicAPI]
        public async Task<List<ScannedFMData>>
        ScanAsync(List<FMToScan> missions, string tempPath, ScanOptions scanOptions)
        {
            return await Task.Run(() =>
                ScanMany(missions, tempPath, scanOptions, null, CancellationToken.None));
        }

        [PublicAPI]
        public async Task<List<ScannedFMData>>
        ScanAsync(List<FMToScan> missions, string tempPath, IProgress<ProgressReport> progress,
            CancellationToken cancellationToken)
        {
            return await Task.Run(() =>
                ScanMany(missions, tempPath, this.ScanOptions, progress, cancellationToken));
        }

        [PublicAPI]
        public async Task<List<ScannedFMData>>
        ScanAsync(List<FMToScan> missions, string tempPath, ScanOptions scanOptions,
            IProgress<ProgressReport> progress, CancellationToken cancellationToken)
        {
            return await Task.Run(() =>
                ScanMany(missions, tempPath, scanOptions, progress, cancellationToken));
        }

        #endregion

        private List<ScannedFMData>
        ScanMany(List<FMToScan> missions, string tempPath, ScanOptions scanOptions,
            IProgress<ProgressReport> progress, CancellationToken cancellationToken)
        {
            // The try-catch blocks are to guarantee that the out-list will at least contain the same number of
            // entries as the in-list; this allows the calling app to not have to do a search to link up the FMs
            // and stuff

            #region Checks

            if (string.IsNullOrEmpty(tempPath))
            {
                Log(LogFile, nameof(ScanMany) + ": Argument is null or empty: " + nameof(tempPath), methodName: false);
                throw new ArgumentException("Argument is null or empty.", nameof(tempPath));
            }

            if (missions == null) throw new ArgumentNullException(nameof(missions));
            if (missions.Count == 0 || (missions.Count == 1 && string.IsNullOrEmpty(missions[0].Path)))
            {
                Log(LogFile, nameof(ScanMany) + ": No mission(s) specified. tempPath: " + tempPath, methodName: false);
                throw new ArgumentException("No mission(s) specified.", nameof(missions));
            }

            this.ScanOptions = scanOptions ?? throw new ArgumentNullException(nameof(scanOptions));

            #endregion

            tempPath = tempPath.Replace('/', '\\');

            var scannedFMDataList = new List<ScannedFMData>();

            // Init and dispose rtfBox here to avoid cross-thread exceptions.
            // For performance, we only have one instance and we just change its content as needed.
            using (var rtfBox = new RichTextBox())
            {
                ProgressReport progressReport = null;

                for (int i = 0; i < missions.Count; i++)
                {
                    ReadmeFiles.Clear();
                    FMDirFiles.Clear();

                    bool nullAlreadyAdded = false;

                    #region Init

                    if (missions[i].Path.IsEmpty())
                    {
                        missions[i].Path = "";
                        scannedFMDataList.Add(null);
                        nullAlreadyAdded = true;
                    }
                    else
                    {
                        string fm = missions[i].Path.Replace('/', '\\');
                        FMIsZip = fm.ExtIsZip() || fm.ExtIs7z();

                        Archive?.Dispose();

                        if (FMIsZip)
                        {
                            ArchivePath = fm;
                            try
                            {
                                FMWorkingPath = Path.Combine(tempPath, Path.GetFileNameWithoutExtension(ArchivePath).Trim());
                            }
                            catch (Exception ex)
                            {
                                Log(LogFile, "Path.Combine error, paths are probably invalid", ex);
                                scannedFMDataList.Add(null);
                                nullAlreadyAdded = true;
                            }
                        }
                        else
                        {
                            FMWorkingPath = fm;
                        }
                    }

                    #endregion

                    #region Report progress and handle cancellation

                    cancellationToken.ThrowIfCancellationRequested();

                    if (progress != null)
                    {
                        progressReport = new ProgressReport
                        {
                            FMName = missions[i].Path,
                            FMNumber = i + 1,
                            FMsTotal = missions.Count,
                            Percent = (100 * (i + 1)) / missions.Count,
                            Finished = false
                        };

                        progress.Report(progressReport);
                    }

                    #endregion

                    Log(LogFile, "About to scan " + missions[i], methodName: false);

                    // If there was an error then we already added null to the list. DON'T add any extra items!
                    if (!nullAlreadyAdded)
                    {
                        ScannedFMData scannedFM = null;
                        ScanOptions _tempScanOptions = null;
                        try
                        {
                            if (missions[i].ForceFullScan)
                            {
                                _tempScanOptions = ScanOptions.DeepCopy();
                                ScanOptions = FullScanOptions.DeepCopy();
                            }
                            try
                            {
                                scannedFM = ScanCurrentFM(rtfBox);
                            }
                            catch (Exception ex)
                            {
                                Log(LogFile, "Exception in FM scan", ex);
                            }
                            scannedFMDataList.Add(scannedFM);
                        }
                        finally
                        {
                            if (missions[i].ForceFullScan)
                            {
                                ScanOptions = _tempScanOptions!.DeepCopy();
                            }
                        }
                    }

                    Log(LogFile, "Finished scanning " + missions[i], methodName: false);

                    if (progress != null && i == missions.Count - 1)
                    {
                        progressReport.Finished = true;
                        progress.Report(progressReport);
                    }
                }
            }

            return scannedFMDataList;
        }

        private ScannedFMData ScanCurrentFM(RichTextBox rtfBox)
        {
            OverallTimer.Restart();

            dsc = FMIsZip ? '/' : '\\';

            // Sometimes we'll want to remove this from the start of a string to get a relative path, so it's
            // critical that we always know we have a dir separator on the end so we don't end up with a leading
            // one on the string when we remove this from the start of it
            if (FMWorkingPath[FMWorkingPath.Length - 1] != dsc) FMWorkingPath += dsc;

            static ScannedFMData UnsupportedZip(string archivePath) => new ScannedFMData
            {
                ArchiveName = Path.GetFileName(archivePath),
                Game = Game.Unsupported
            };

            static ScannedFMData UnsupportedDir() => null;

            long? sevenZipSize = null;

            #region Check for and setup 7-Zip

            bool fmIsSevenZip = false;
            if (FMIsZip && ArchivePath.ExtIs7z())
            {
                FMIsZip = false;
                dsc = '\\';
                FMWorkingPath = FMWorkingPath.Replace('/', '\\');
                fmIsSevenZip = true;

                try
                {
                    using var sze = new SevenZipExtractor(ArchivePath) { PreserveDirectoryStructure = true };
                    sevenZipSize = sze.PackedSize;
                    sze.ExtractArchive(FMWorkingPath);
                }
                catch (Exception)
                {
                    // Third party thing, doesn't tell you what exceptions it can throw, whatever
                    DeleteFMWorkingPath(FMWorkingPath);
                    return UnsupportedZip(ArchivePath);
                }
            }

            #endregion

            #region Check for and setup Zip

            if (FMIsZip)
            {
                Debug.WriteLine(@"----------" + ArchivePath);

                if (ArchivePath.ExtIsZip())
                {
                    try
                    {
                        Archive = new ZipArchiveFast(new FileStream(ArchivePath, FileMode.Open, FileAccess.Read),
                            leaveOpen: false, ZipEntryNameEncoding);

                        // Archive.Entries is lazy-loaded, so this will also trigger any exceptions that may be
                        // thrown while loading them. If this passes, we're definitely good.
                        if (Archive.Entries.Count == 0) return UnsupportedZip(ArchivePath);
                    }
                    catch (Exception)
                    {
                        // Invalid zip file, whatever, move on
                        return UnsupportedZip(ArchivePath);
                    }
                }
                else
                {
                    return UnsupportedZip(ArchivePath);
                }
            }
            else
            {
                if (!Directory.Exists(FMWorkingPath)) return UnsupportedDir();
                Debug.WriteLine(@"----------" + FMWorkingPath);
            }

            #endregion

            var fmData = new ScannedFMData
            {
                ArchiveName = FMIsZip || fmIsSevenZip
                    ? Path.GetFileName(ArchivePath)
                    : new DirectoryInfo(FMWorkingPath).Name
            };

            #region Size

            // Getting the size is horrendously expensive for folders, but if we're doing it then we can save
            // some time later by using the FileInfo list as a cache.
            if (ScanOptions.ScanSize)
            {
                if (FMIsZip)
                {
                    fmData.Size = Archive.ArchiveStream.Length;
                }
                else
                {
                    FMDirFiles = new DirectoryInfo(FMWorkingPath).EnumerateFiles("*", SearchOption.AllDirectories).ToList();

                    if (fmIsSevenZip)
                    {
                        fmData.Size = sevenZipSize;
                    }
                    else
                    {
                        long size = 0;
                        foreach (FileInfo fi in FMDirFiles) size += fi.Length;
                        fmData.Size = size;
                    }
                }
            }

            #endregion

            var baseDirFiles = new List<NameAndIndex>();
            var misFiles = new List<NameAndIndex>();
            var usedMisFiles = new List<NameAndIndex>();
            var stringsDirFiles = new List<NameAndIndex>();
            var intrfaceDirFiles = new List<NameAndIndex>();
            var booksDirFiles = new List<NameAndIndex>();
            var t3FMExtrasDirFiles = new List<NameAndIndex>();

            #region Cache FM data

            bool success =
                ReadAndCacheFMData(fmData, baseDirFiles, misFiles, usedMisFiles, stringsDirFiles,
                    intrfaceDirFiles, booksDirFiles, t3FMExtrasDirFiles);

            if (!success)
            {
                if (fmIsSevenZip) DeleteFMWorkingPath(FMWorkingPath);
                return FMIsZip || fmIsSevenZip ? UnsupportedZip(ArchivePath) : UnsupportedDir();
            }

            #endregion

            bool fmIsT3 = fmData.Game == Game.Thief3;

            void SetOrAddTitle(string value)
            {
                value = CleanupTitle(value);

                if (string.IsNullOrEmpty(value)) return;

                if (string.IsNullOrEmpty(fmData.Title))
                {
                    fmData.Title = value;
                }
                else if (!fmData.Title.EqualsI(value) && !fmData.AlternateTitles.ContainsI(value))
                {
                    fmData.AlternateTitles.Add(value);
                }
            }

            bool fmIsSS2 = false;

            if (!fmIsT3)
            {
                // We check game type as early as possible because we used to want to reject SS2 FMs early for
                // speed. We support SS2 now, but this works fine and dandy where it is so not messing with it.

                #region NewDark/GameType checks

                if (ScanOptions.ScanNewDarkRequired || ScanOptions.ScanGameType)
                {
                    var (newDarkRequired, game) = GetGameTypeAndEngine(baseDirFiles, usedMisFiles);
                    if (ScanOptions.ScanNewDarkRequired) fmData.NewDarkRequired = newDarkRequired;
                    if (ScanOptions.ScanGameType)
                    {
                        fmData.Game = game;
                        if (fmData.Game == Game.Unsupported) return fmData;
                    }
                }

                fmIsSS2 = fmData.Game == Game.SS2;

                #endregion

                // If we're Thief 3, we just skip figuring this out - I don't know how to detect if a T3 mission
                // is a campaign, and I'm not even sure any T3 campaigns have been released (not counting ones
                // that are just a series of separate FMs, of course).
                // TODO: @SS2: Force SS2 to single mission for now
                // Until I can figure out how to detect which .mis files are used without there being an actual
                // list...
                fmData.Type = fmIsSS2 || usedMisFiles.Count <= 1 ? FMType.FanMission : FMType.Campaign;

                #region Check info files

                if (ScanOptions.ScanTitle || ScanOptions.ScanAuthor || ScanOptions.ScanVersion ||
                    ScanOptions.ScanReleaseDate || ScanOptions.ScanTags)
                {
                    NameAndIndex fmInfoXml = baseDirFiles.FirstOrDefault(x => x.Name.EqualsI(FMFiles.FMInfoXml));
                    if (fmInfoXml != null)
                    {
                        var t = ReadFMInfoXml(fmInfoXml);
                        if (ScanOptions.ScanTitle) SetOrAddTitle(t.Title);
                        if (ScanOptions.ScanAuthor) fmData.Author = t.Author;
                        if (ScanOptions.ScanVersion) fmData.Version = t.Version;
                        if (ScanOptions.ScanReleaseDate && t.ReleaseDate != null) fmData.LastUpdateDate = t.ReleaseDate;
                    }
                }
                // I think we need to always scan fm.ini even if we're not returning any of its fields, because
                // of tags, I think for some reason we're needing to read tags always?
                {
                    NameAndIndex fmIni = baseDirFiles.FirstOrDefault(x => x.Name.EqualsI(FMFiles.FMIni));
                    if (fmIni != null)
                    {
                        var t = ReadFMIni(fmIni);
                        if (ScanOptions.ScanTitle) SetOrAddTitle(t.Title);
                        if (ScanOptions.ScanAuthor && !t.Author.IsEmpty()) fmData.Author = t.Author;
                        fmData.Description = t.Description;
                        if (ScanOptions.ScanReleaseDate && t.LastUpdateDate != null) fmData.LastUpdateDate = t.LastUpdateDate;
                        if (ScanOptions.ScanTags) fmData.TagsString = t.Tags;
                    }
                }
                if (ScanOptions.ScanTitle || ScanOptions.ScanAuthor)
                {
                    // SS2 file
                    // TODO: If we wanted to be sticklers, we could skip this for non-SS2 FMs
                    NameAndIndex modIni = baseDirFiles.FirstOrDefault(x => x.Name.EqualsI(FMFiles.ModIni));
                    if (modIni != null)
                    {
                        var t = ReadModIni(modIni);
                        if (ScanOptions.ScanTitle) SetOrAddTitle(t.Title);
                        if (ScanOptions.ScanAuthor && !t.Author.IsEmpty()) fmData.Author = t.Author;
                    }
                }

                #endregion
            }

            #region Read, cache, and set readme files

            var readmeDirFiles = new List<NameAndIndex>();

            foreach (NameAndIndex f in baseDirFiles) readmeDirFiles.Add(f);

            if (fmIsT3) foreach (NameAndIndex f in t3FMExtrasDirFiles) readmeDirFiles.Add(f);

            ReadAndCacheReadmeFiles(readmeDirFiles, rtfBox);

            #endregion

            if (!fmIsT3)
            {
                // This is here because it needs to come after the readmes are cached
                #region NewDark minimum required version

                if (fmData.NewDarkRequired == true && ScanOptions.ScanNewDarkMinimumVersion)
                {
                    fmData.NewDarkMinRequiredVersion = GetValueFromReadme(SpecialLogic.NewDarkMinimumVersion);
                }

                #endregion
            }

            #region Set release date

            if (ScanOptions.ScanReleaseDate && fmData.LastUpdateDate == null)
            {
                fmData.LastUpdateDate = GetReleaseDate(fmIsT3, usedMisFiles);
            }

            #endregion

            #region Title and IncludedMissions

            // SS2 doesn't have a missions list or a titles list file
            if (!fmIsT3 && !fmIsSS2)
            {
                if (ScanOptions.ScanTitle || ScanOptions.ScanCampaignMissionNames)
                {
                    var (titleFrom0, titleFromN, cNames) = GetMissionNames(stringsDirFiles, misFiles, usedMisFiles);
                    if (ScanOptions.ScanTitle)
                    {
                        SetOrAddTitle(titleFrom0);
                        SetOrAddTitle(titleFromN);
                    }

                    if (ScanOptions.ScanCampaignMissionNames && cNames != null && cNames.Length > 0)
                    {
                        for (int i = 0; i < cNames.Length; i++) cNames[i] = CleanupTitle(cNames[i]);
                        fmData.IncludedMissions = cNames;
                    }
                }
            }

            if (ScanOptions.ScanTitle)
            {
                SetOrAddTitle(GetValueFromReadme(SpecialLogic.Title, titles: null, SA_TitleDetect));

                if (!fmIsT3) SetOrAddTitle(GetTitleFromNewGameStrFile(intrfaceDirFiles));

                var topOfReadmeTitles = GetTitlesFromTopOfReadmes(ReadmeFiles);
                if (topOfReadmeTitles != null && topOfReadmeTitles.Count > 0)
                {
                    foreach (string title in topOfReadmeTitles) SetOrAddTitle(title);
                }
            }

            #endregion

            #region Author

            if (ScanOptions.ScanAuthor || ScanOptions.ScanTags)
            {
                if (fmData.Author.IsEmpty())
                {
                    var titles = !string.IsNullOrEmpty(fmData.Title)
                        ? new List<string> { fmData.Title }
                        : null;
                    if (titles != null && fmData.AlternateTitles?.Count > 0)
                    {
                        titles.AddRange(fmData.AlternateTitles);
                    }

                    // TODO: Do I want to check AlternateTitles for StartsWithI("By ") as well?
                    string author = GetValueFromReadme(SpecialLogic.Author, titles, SA_AuthorDetect);

                    fmData.Author = CleanupValue(author);
                }

                if (!fmData.Author.IsEmpty())
                {
                    // Remove email addresses from the end of author names
                    Match match = AuthorEmailRegex.Match(fmData.Author);
                    if (match.Success)
                    {
                        fmData.Author = fmData.Author.Remove(match.Index, match.Length).Trim();
                    }
                }
            }

            #endregion

            #region Version

            if (ScanOptions.ScanVersion && fmData.Version.IsEmpty()) fmData.Version = GetVersion();

            #endregion

            // Again, I don't know enough about Thief 3 to know how to detect its languages
            if (!fmIsT3)
            {
                #region Languages

                if (ScanOptions.ScanLanguages || ScanOptions.ScanTags)
                {
                    var getLangs = GetLanguages(baseDirFiles, booksDirFiles, intrfaceDirFiles, stringsDirFiles);
                    fmData.Languages = getLangs.Langs;
                    if (getLangs.Langs?.Length > 0)
                    {
                        SetLangTags(fmData, getLangs.UncertainLangs);
                    }
                    if (!ScanOptions.ScanLanguages) fmData.Languages = null;
                }

                #endregion
            }

            if (ScanOptions.ScanTags)
            {
                if (fmData.Type == FMType.Campaign) SetMiscTag(fmData, "campaign");

                if (!string.IsNullOrEmpty(fmData.Author))
                {
                    int ai = fmData.Author.IndexOf(' ');
                    if (ai == -1) ai = fmData.Author.IndexOf('-');
                    if (ai == -1) ai = fmData.Author.Length - 1;
                    string anonAuthor = fmData.Author.Substring(0, ai);
                    if (anonAuthor.EqualsI("Anon") || anonAuthor.EqualsI("Withheld") ||
                        anonAuthor.SimilarityTo("Anonymous", OrdinalIgnoreCase) > 0.75)
                    {
                        SetMiscTag(fmData, "unknown author");
                    }
                }

                if (!ScanOptions.ScanAuthor) fmData.Author = null;
            }

            if (fmIsSevenZip) DeleteFMWorkingPath(FMWorkingPath);

            OverallTimer.Stop();
            Debug.WriteLine(@"This FM took:\r\n" + OverallTimer.Elapsed.ToString(@"hh\:mm\:ss\.fffffff"));

            // Due to the Thief 3 detection being done in the same place as the custom resources check, it's
            // theoretically possible to end up with some of these set. There's no way around it, so just unset
            // them all here for consistency.
            if (fmIsT3)
            {
                fmData.HasMap = null;
                fmData.HasAutomap = null;
                fmData.HasCustomCreatures = null;
                fmData.HasCustomScripts = null;
                fmData.HasCustomTextures = null;
                fmData.HasCustomSounds = null;
                fmData.HasCustomObjects = null;
                fmData.HasCustomMotions = null;
                fmData.HasCustomSubtitles = null;
                fmData.HasMovies = null;
            }

            return fmData;
        }

        private DateTime? GetReleaseDate(bool fmIsT3, List<NameAndIndex> usedMisFiles)
        {
            DateTime? ret = null;

            // Look in the readme
            string ds = GetValueFromReadme(SpecialLogic.None, null, SA_ReleaseDateDetect);
            if (!string.IsNullOrEmpty(ds) && StringToDate(ds, out DateTime dt))
            {
                ret = dt;
            }

            // Look for the first readme file's last modified date
            if (ret == null && ReadmeFiles.Count > 0 && ReadmeFiles[0].LastModifiedDate.Year > 1998)
            {
                ret = ReadmeFiles[0].LastModifiedDate;
            }

            // No first used mission file for T3: no idea how to find such a thing
            if (fmIsT3) return ret;

            // Look for the first used .mis file's last modified date
            if (ret == null)
            {
                DateTime misFileDate;
                if (FMIsZip)
                {
                    misFileDate =
                        new DateTimeOffset(ZipHelpers.ZipTimeToDateTime(
                            Archive.Entries[usedMisFiles[0].Index].LastWriteTime)).DateTime;
                }
                else
                {
                    if (ScanOptions.ScanSize && FMDirFiles.Count > 0)
                    {
                        FileInfo misFile = FMDirFiles.First(x => x.FullName.EqualsI(FMWorkingPath + usedMisFiles[0].Name));
                        misFileDate = new DateTimeOffset(misFile.LastWriteTime).DateTime;
                    }
                    else
                    {
                        var fi = new FileInfo(Path.Combine(FMWorkingPath, usedMisFiles[0].Name));
                        misFileDate = new DateTimeOffset(fi.LastWriteTime).DateTime;
                    }
                }

                if (misFileDate.Year > 1998)
                {
                    ret = misFileDate;
                }
            }

            // If we still don't have anything, give up: we've made a good-faith effort.
            return ret;
        }

        private static void SetLangTags(ScannedFMData fmData, string[] uncertainLangs)
        {
            if (string.IsNullOrWhiteSpace(fmData.TagsString)) fmData.TagsString = "";
            for (int i = 0; i < fmData.Languages.Length; i++)
            {
                string lang = fmData.Languages[i];

                // NOTE: This all depends on langs being lowercase!
                Debug.Assert(lang == lang.ToLowerInvariant(),
                            "lang != lang.ToLowerInvariant() - lang is not lowercase");

                if (uncertainLangs.Contains(lang)) continue;

                if (fmData.TagsString.Contains(lang))
                {
                    fmData.TagsString = Regex.Replace(fmData.TagsString, @":\s*" + lang, ":" + LanguagesC[lang]);
                }

                // PERF: 5ms over the whole 1098 set, whatever
                Match match = Regex.Match(fmData.TagsString, @"language:\s*" + lang, RegexOptions.IgnoreCase);
                if (match.Success) continue;

                if (fmData.TagsString != "") fmData.TagsString += ", ";
                fmData.TagsString += "language:" + LanguagesC[lang];
            }

            // Compatibility with old behavior for painless diffing
            if (string.IsNullOrEmpty(fmData.TagsString)) fmData.TagsString = null;
        }

        private static void SetMiscTag(ScannedFMData fmData, string tag)
        {
            if (string.IsNullOrWhiteSpace(fmData.TagsString)) fmData.TagsString = "";

            var list = fmData.TagsString.Split(CA_CommaSemicolon).ToList();
            bool tagFound = false;
            for (int i = 0; i < list.Count; i++)
            {
                list[i] = list[i].Trim();
                if (string.IsNullOrEmpty(list[i]))
                {
                    list.RemoveAt(i);
                    i--;
                }
                else if (list[i].EqualsI(tag))
                {
                    if (tagFound)
                    {
                        list.RemoveAt(i);
                        i--;
                    }
                    else
                    {
                        list[i] = tag;
                        tagFound = true;
                    }
                }
            }
            if (tagFound) return;

            list.Add(tag);

            string tagsString = "";
            for (int i = 0; i < list.Count; i++)
            {
                if (i > 0) tagsString += ", ";
                tagsString += list[i];
            }
            fmData.TagsString = tagsString;
        }

        private bool ReadAndCacheFMData(ScannedFMData fmd, List<NameAndIndex> baseDirFiles,
            List<NameAndIndex> misFiles, List<NameAndIndex> usedMisFiles, List<NameAndIndex> stringsDirFiles,
            List<NameAndIndex> intrfaceDirFiles, List<NameAndIndex> booksDirFiles,
            List<NameAndIndex> t3FMExtrasDirFiles)
        {
            #region Add BaseDirFiles

            bool t3Found = false;

            // This is split out because of weird semantics with if(this && that) vs nested ifs (required in
            // order to have a var in the middle to avoid multiple LastIndexOf calls).
            static bool MapFileExists(string path, char _dsc)
            {
                if (path.StartsWithI(FMDirs.IntrfaceS(_dsc)) &&
                    path.CountChars(_dsc) >= 2)
                {
                    int lsi = path.LastIndexOf(_dsc);
                    if (path.Length > lsi + 5 &&
                        path.Substring(lsi + 1, 5).EqualsI("page0") &&
                        path.LastIndexOf('.') > lsi)
                    {
                        return true;
                    }
                }

                return false;
            }

            if (FMIsZip || ScanOptions.ScanSize)
            {
                for (int i = 0; i < (FMIsZip ? Archive.Entries.Count : FMDirFiles.Count); i++)
                {
                    string fn = FMIsZip
                        ? Archive.Entries[i].FullName
                        : FMDirFiles[i].FullName.Substring(FMWorkingPath.Length);

                    int index = FMIsZip ? i : -1;

                    if (!t3Found &&
                        fn.StartsWithI(FMDirs.T3DetectS(dsc)) &&
                        fn.CountChars(dsc) == 3 &&
                        (fn.ExtIsIbt() ||
                        fn.ExtIsCbt() ||
                        fn.ExtIsGmp() ||
                        fn.ExtIsNed() ||
                        fn.ExtIsUnr()))
                    {
                        fmd.Game = Game.Thief3;
                        t3Found = true;
                        continue;
                    }
                    // We can't early-out if !t3Found here because if we find it after this point, we'll be
                    // missing however many of these we skipped before we detected Thief 3
                    else if (fn.StartsWithI(FMDirs.T3FMExtras1S(dsc)) ||
                             fn.StartsWithI(FMDirs.T3FMExtras2S(dsc)))
                    {
                        t3FMExtrasDirFiles.Add(new NameAndIndex { Name = fn, Index = index });
                        continue;
                    }
                    else if (!fn.Contains(dsc) && fn.Contains('.'))
                    {
                        baseDirFiles.Add(new NameAndIndex { Name = fn, Index = index });
                        // Fallthrough so ScanCustomResources can use it
                    }
                    else if (!t3Found && fn.StartsWithI(FMDirs.StringsS(dsc)))
                    {
                        stringsDirFiles.Add(new NameAndIndex { Name = fn, Index = index });
                        continue;
                    }
                    else if (!t3Found && fn.StartsWithI(FMDirs.IntrfaceS(dsc)))
                    {
                        intrfaceDirFiles.Add(new NameAndIndex { Name = fn, Index = index });
                        // Fallthrough so ScanCustomResources can use it
                    }
                    else if (!t3Found && fn.StartsWithI(FMDirs.BooksS(dsc)))
                    {
                        booksDirFiles.Add(new NameAndIndex { Name = fn, Index = index });
                        continue;
                    }

                    // Inlined for performance. We cut the time roughly in half by doing this.
                    if (!t3Found && ScanOptions.ScanCustomResources)
                    {
                        if (fmd.HasAutomap == null &&
                            fn.StartsWithI(FMDirs.IntrfaceS(dsc)) &&
                            fn.CountChars(dsc) >= 2 &&
                            fn.EndsWithRaDotBin())
                        {
                            fmd.HasAutomap = true;
                            // Definitely a clever deduction, definitely not a sneaky hack for GatB-T2
                            fmd.HasMap = true;
                        }
                        else if (fmd.HasMap == null && MapFileExists(fn, dsc))
                        {
                            fmd.HasMap = true;
                        }
                        else if (fmd.HasCustomMotions == null &&
                                 fn.StartsWithI(FMDirs.MotionsS(dsc)) &&
                                 MotionFileExtensions.Any(fn.EndsWithI))
                        {
                            fmd.HasCustomMotions = true;
                        }
                        else if (fmd.HasMovies == null &&
                                 (fn.StartsWithI(FMDirs.MoviesS(dsc)) || fn.StartsWithI(FMDirs.CutscenesS(dsc))) &&
                                 fn.HasFileExtension())
                        {
                            fmd.HasMovies = true;
                        }
                        else if (fmd.HasCustomTextures == null &&
                                 fn.StartsWithI(FMDirs.FamS(dsc)) &&
                                 ImageFileExtensions.Any(fn.EndsWithI))
                        {
                            fmd.HasCustomTextures = true;
                        }
                        else if (fmd.HasCustomObjects == null &&
                                 fn.StartsWithI(FMDirs.ObjS(dsc)) &&
                                 fn.ExtIsBin())
                        {
                            fmd.HasCustomObjects = true;
                        }
                        else if (fmd.HasCustomCreatures == null &&
                                 fn.StartsWithI(FMDirs.MeshS(dsc)) &&
                                 fn.ExtIsBin())
                        {
                            fmd.HasCustomCreatures = true;
                        }
                        else if (fmd.HasCustomScripts == null &&
                                 (!fn.Contains(dsc) &&
                                  ScriptFileExtensions.Any(fn.EndsWithI)) ||
                                 (fn.StartsWithI(FMDirs.ScriptsS(dsc)) &&
                                  fn.HasFileExtension()))
                        {
                            fmd.HasCustomScripts = true;
                        }
                        else if (fmd.HasCustomSounds == null &&
                                 (fn.StartsWithI(FMDirs.SndS(dsc)) || fn.StartsWithI(FMDirs.Snd2S(dsc))) &&
                                 fn.HasFileExtension())
                        {
                            fmd.HasCustomSounds = true;
                        }
                        else if (fmd.HasCustomSubtitles == null &&
                                 fn.StartsWithI(FMDirs.SubtitlesS(dsc)) &&
                                 fn.ExtIsSub())
                        {
                            fmd.HasCustomSubtitles = true;
                        }
                    }
                }

                // Thief 3 FMs can have empty base dirs, and we don't scan for custom resources for T3
                if (!t3Found)
                {
                    if (baseDirFiles.Count == 0) return false;

                    if (ScanOptions.ScanCustomResources)
                    {
                        if (fmd.HasMap == null) fmd.HasMap = false;
                        if (fmd.HasAutomap == null) fmd.HasAutomap = false;
                        if (fmd.HasCustomMotions == null) fmd.HasCustomMotions = false;
                        if (fmd.HasMovies == null) fmd.HasMovies = false;
                        if (fmd.HasCustomTextures == null) fmd.HasCustomTextures = false;
                        if (fmd.HasCustomObjects == null) fmd.HasCustomObjects = false;
                        if (fmd.HasCustomCreatures == null) fmd.HasCustomCreatures = false;
                        if (fmd.HasCustomScripts == null) fmd.HasCustomScripts = false;
                        if (fmd.HasCustomSounds == null) fmd.HasCustomSounds = false;
                        if (fmd.HasCustomSubtitles == null) fmd.HasCustomSubtitles = false;
                    }
                }
            }
            else
            {
                string t3DetectPath = Path.Combine(FMWorkingPath, FMDirs.T3Detect);
                if (Directory.Exists(t3DetectPath) &&
                    FastIO.FilesExistSearchTop(t3DetectPath, SA_T3DetectExtensions))
                {
                    t3Found = true;
                    fmd.Game = Game.Thief3;
                }

                foreach (string f in EnumFiles("*", SearchOption.TopDirectoryOnly))
                {
                    baseDirFiles.Add(new NameAndIndex { Name = Path.GetFileName(f) });
                }

                if (t3Found)
                {
                    foreach (string f in EnumFiles(FMDirs.T3FMExtras1, "*", SearchOption.TopDirectoryOnly))
                    {
                        t3FMExtrasDirFiles.Add(new NameAndIndex { Name = f.Substring(FMWorkingPath.Length) });
                    }

                    foreach (string f in EnumFiles(FMDirs.T3FMExtras2, "*", SearchOption.TopDirectoryOnly))
                    {
                        t3FMExtrasDirFiles.Add(new NameAndIndex { Name = f.Substring(FMWorkingPath.Length) });
                    }
                }
                else
                {
                    if (baseDirFiles.Count == 0) return false;

                    foreach (string f in EnumFiles(FMDirs.Strings, "*", SearchOption.AllDirectories))
                    {
                        stringsDirFiles.Add(new NameAndIndex { Name = f.Substring(FMWorkingPath.Length) });
                    }

                    foreach (string f in EnumFiles(FMDirs.Intrface, "*", SearchOption.AllDirectories))
                    {
                        intrfaceDirFiles.Add(new NameAndIndex { Name = f.Substring(FMWorkingPath.Length) });
                    }

                    foreach (string f in EnumFiles(FMDirs.Books, "*", SearchOption.AllDirectories))
                    {
                        booksDirFiles.Add(new NameAndIndex { Name = f.Substring(FMWorkingPath.Length) });
                    }

                    // TODO: Maybe extract this again, but then I have to extract MapFileExists() too
                    if (ScanOptions.ScanCustomResources)
                    {
                        // TODO: I already have baseDirFiles; see if this EnumerateDirectories can be removed
                        // Even a janky scan through baseDirFiles would probably be faster than hitting the disk
                        string[] baseDirFolders = (
                            from f in Directory.EnumerateDirectories(FMWorkingPath, "*",
                                SearchOption.TopDirectoryOnly)
                            select f.Substring(f.LastIndexOf(dsc) + 1)).ToArray();

                        foreach (NameAndIndex f in intrfaceDirFiles)
                        {
                            if (fmd.HasAutomap == null &&
                                f.Name.StartsWithI(FMDirs.IntrfaceS(dsc)) &&
                                f.Name.CountChars(dsc) >= 2 &&
                                f.Name.EndsWithRaDotBin())
                            {
                                fmd.HasAutomap = true;
                                // Definitely a clever deduction, definitely not a sneaky hack for GatB-T2
                                fmd.HasMap = true;
                                break;
                            }

                            if (fmd.HasMap == null && MapFileExists(f.Name, dsc)) fmd.HasMap = true;
                        }

                        if (fmd.HasMap == null) fmd.HasMap = false;
                        if (fmd.HasAutomap == null) fmd.HasAutomap = false;

                        fmd.HasCustomMotions =
                            baseDirFolders.ContainsI(FMDirs.Motions) &&
                            FastIO.FilesExistSearchAll(Path.Combine(FMWorkingPath, FMDirs.Motions),
                                MotionFilePatterns);

                        fmd.HasMovies =
                            (baseDirFolders.ContainsI(FMDirs.Movies) &&
                             FastIO.FilesExistSearchAll(Path.Combine(FMWorkingPath, FMDirs.Movies), SA_AllFiles)) ||
                            (baseDirFolders.ContainsI(FMDirs.Cutscenes) &&
                             FastIO.FilesExistSearchAll(Path.Combine(FMWorkingPath, FMDirs.Cutscenes), SA_AllFiles));

                        fmd.HasCustomTextures =
                            baseDirFolders.ContainsI(FMDirs.Fam) &&
                            FastIO.FilesExistSearchAll(Path.Combine(FMWorkingPath, FMDirs.Fam),
                                ImageFilePatterns);

                        fmd.HasCustomObjects =
                            baseDirFolders.ContainsI(FMDirs.Obj) &&
                            FastIO.FilesExistSearchAll(Path.Combine(FMWorkingPath, FMDirs.Obj), SA_AllBinFiles);

                        fmd.HasCustomCreatures =
                            baseDirFolders.ContainsI(FMDirs.Mesh) &&
                            FastIO.FilesExistSearchAll(Path.Combine(FMWorkingPath, FMDirs.Mesh), SA_AllBinFiles);

                        fmd.HasCustomScripts =
                            baseDirFiles.Any(x => ScriptFileExtensions.ContainsI(Path.GetExtension(x.Name))) ||
                            (baseDirFolders.ContainsI(FMDirs.Scripts) &&
                             FastIO.FilesExistSearchAll(Path.Combine(FMWorkingPath, FMDirs.Scripts), SA_AllFiles));

                        fmd.HasCustomSounds =
                            (baseDirFolders.ContainsI(FMDirs.Snd) &&
                             FastIO.FilesExistSearchAll(Path.Combine(FMWorkingPath, FMDirs.Snd), SA_AllFiles)) ||
                             (baseDirFolders.ContainsI(FMDirs.Snd2) &&
                              FastIO.FilesExistSearchAll(Path.Combine(FMWorkingPath, FMDirs.Snd2), SA_AllFiles));

                        fmd.HasCustomSubtitles =
                            baseDirFolders.ContainsI(FMDirs.Subtitles) &&
                            FastIO.FilesExistSearchAll(Path.Combine(FMWorkingPath, FMDirs.Subtitles), SA_AllSubFiles);
                    }
                }
            }

            // Cut it right here for Thief 3: we don't need anything else
            if (t3Found) return true;

            #endregion

            #region Add MisFiles and check for none

            for (int i = 0; i < baseDirFiles.Count; i++)
            {
                NameAndIndex f = baseDirFiles[i];
                if (f.Name.ExtIsMis())
                {
                    misFiles.Add(new NameAndIndex { Name = Path.GetFileName(f.Name), Index = f.Index });
                }
            }

            if (misFiles.Count == 0) return false;

            #endregion

            #region Cache list of used .mis files

            NameAndIndex missFlag = null;

            if (stringsDirFiles.Count > 0)
            {
                // I don't remember if I need to search in this exact order, so uh... not rockin' the boat.
                missFlag =
                    stringsDirFiles.FirstOrDefault(x =>
                        x.Name.EqualsI(FMDirs.StringsS(dsc) + FMFiles.MissFlag))
                    ?? stringsDirFiles.FirstOrDefault(x =>
                        x.Name.EqualsI(FMDirs.StringsS(dsc) + "english" + dsc + FMFiles.MissFlag))
                    ?? stringsDirFiles.FirstOrDefault(x =>
                        x.Name.EndsWithI(dsc + FMFiles.MissFlag));
            }

            if (missFlag != null)
            {
                string[] mfLines;

                if (FMIsZip)
                {
                    var e = Archive.Entries[missFlag.Index];
                    using var es = e.Open();
                    mfLines = ReadAllLinesE(es, e.Length);
                }
                else
                {
                    mfLines = ReadAllLinesE(Path.Combine(FMWorkingPath, missFlag.Name));
                }

                for (int i = 0; i < misFiles.Count; i++)
                {
                    NameAndIndex mf = misFiles[i];
                    string mfNoExt = mf.Name.RemoveExtension();
                    if (mfNoExt.StartsWithI("miss") && mfNoExt.Length > 4)
                    {
                        for (int j = 0; j < mfLines.Length; j++)
                        {
                            string line = mfLines[j];
                            if (line.StartsWithI("miss_" + mfNoExt.Substring(4) + ":") &&
                                line.IndexOf('\"') > -1 &&
                                !line.Substring(line.IndexOf('\"')).StartsWithI("\"skip\""))
                            {
                                usedMisFiles.Add(mf);
                            }
                        }
                    }
                }
            }

            // Fallback we hope never happens, but... sometimes it does
            if (usedMisFiles.Count == 0) usedMisFiles.AddRange(misFiles);

            #endregion

            return true;
        }

        private (string Title, string Author, string Version, DateTime? ReleaseDate)
        ReadFMInfoXml(NameAndIndex file)
        {
            string title = null;
            string author = null;
            string version = null;
            DateTime? releaseDate = null;

            var fmInfoXml = new XmlDocument();

            #region Load XML

            if (FMIsZip)
            {
                var e = Archive.Entries[file.Index];
                using var es = e.Open();
                fmInfoXml.Load(es);
            }
            else
            {
                fmInfoXml.Load(Path.Combine(FMWorkingPath, file.Name));
            }

            #endregion

            if (ScanOptions.ScanTitle)
            {
                var xTitle = fmInfoXml.GetElementsByTagName("title");
                if (xTitle.Count > 0) title = xTitle[0].InnerText;
            }

            if (ScanOptions.ScanAuthor)
            {
                var xAuthor = fmInfoXml.GetElementsByTagName("author");
                if (xAuthor.Count > 0) author = xAuthor[0].InnerText;
            }

            if (ScanOptions.ScanVersion)
            {
                var xVersion = fmInfoXml.GetElementsByTagName("version");
                if (xVersion.Count > 0) version = xVersion[0].InnerText;
            }

            var xReleaseDate = fmInfoXml.GetElementsByTagName("releasedate");
            if (xReleaseDate.Count > 0)
            {
                string rdString = xReleaseDate[0].InnerText;
                releaseDate = StringToDate(rdString, out DateTime dt) ? (DateTime?)dt : null;
            }

            // These files also specify languages and whether the mission has custom stuff, but we're not going
            // to trust what we're told - we're going to detect that stuff by looking at what's actually there.

            return (title, author, version, releaseDate);
        }

        private (string Title, string Author, string Description, DateTime? LastUpdateDate, string Tags)
        ReadFMIni(NameAndIndex file)
        {
            var ret = (Title: (string)null, Author: (string)null, Description: (string)null,
                LastUpdateDate: (DateTime?)null, Tags: (string)null);

            #region Load INI

            string[] iniLines;

            if (FMIsZip)
            {
                var e = Archive.Entries[file.Index];
                using var es = e.Open();
                iniLines = ReadAllLinesE(es, e.Length);
            }
            else
            {
                iniLines = ReadAllLinesE(Path.Combine(FMWorkingPath, file.Name));
            }

            if (iniLines == null || iniLines.Length == 0) return (null, null, null, null, null);

            FMIniData fmIni = Ini.DeserializeFMIniLines(iniLines);

            #endregion

            #region Descr

            // Descr can be multiline. You're supposed to use \n for linebreaks. Most of the time people do that.
            // That's always nice when people do that. It's so much nicer than when they break an ini value into
            // multiple actual lines for some reason. God help us if any more keys get added to the spec and some
            // wise guy puts one of those keys after a multiline Descr.

            if (!string.IsNullOrEmpty(fmIni.Descr))
            {
                fmIni.Descr = fmIni.Descr
                    .Replace(@"\t", "\t")
                    .Replace(@"\r\n", "\r\n")
                    .Replace(@"\r", "\r\n")
                    .Replace(@"\n", "\r\n")
                    .Replace(@"\""", "\"");

                // Remove surrounding quotes
                if (fmIni.Descr[0] == '\"' && fmIni.Descr[fmIni.Descr.Length - 1] == '\"' &&
                    fmIni.Descr.CountChars('\"') == 2)
                {
                    fmIni.Descr = fmIni.Descr.Trim(CA_DoubleQuote);
                }
                if (fmIni.Descr[0] == uldq && fmIni.Descr[fmIni.Descr.Length - 1] == urdq &&
                    fmIni.Descr.CountChars(uldq) + fmIni.Descr.CountChars(urdq) == 2)
                {
                    fmIni.Descr = fmIni.Descr.Trim(CA_UnicodeQuotes);
                }

                fmIni.Descr = fmIni.Descr.RemoveUnpairedLeadingOrTrailingQuotes();

                // Normalize to just LF for now. Otherwise it just doesn't work right for reasons confusing and
                // senseless. It can easily be converted later.
                fmIni.Descr = fmIni.Descr.Replace("\r\n", "\n");
                if (string.IsNullOrWhiteSpace(fmIni.Descr)) fmIni.Descr = null;
            }

            #endregion

            #region Get author from tags

            if (ScanOptions.ScanAuthor)
            {
                if (fmIni.Tags != null)
                {
                    string[] tagsArray = fmIni.Tags.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);

                    // LINQ avoidance
                    var authors = new List<string>();
                    for (int i = 0; i < tagsArray.Length; i++)
                    {
                        string tag = tagsArray[i];
                        if (tag.StartsWithI("author:")) authors.Add(tag);
                    }

                    string authorString = "";
                    for (int i = 0; i < authors.Count; i++)
                    {
                        string a = authors[i];
                        if (i > 0 && !authorString.EndsWith(", ")) authorString += ", ";
                        authorString += a.Substring(a.IndexOf(':') + 1).Trim();
                    }

                    ret.Author = authorString;
                }
            }

            #endregion

            // Return the raw string and let the caller decide what to do with it
            if (ScanOptions.ScanTags) ret.Tags = fmIni.Tags;

            if (ScanOptions.ScanTitle) ret.Title = fmIni.NiceName;

            if (ScanOptions.ScanReleaseDate)
            {
                string rd = fmIni.ReleaseDate;

                // The fm.ini Unix timestamp looks 32-bit, but FMSel's source code pegs it as int64. It must just
                // be writing only as many digits as it needs. That's good, because 32-bit will run out in 2038.
                // Anyway, we should parse it as long (NDL only does int, so it's in for a surprise in 20 years :P)
                if (long.TryParse(rd, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out long seconds))
                {
                    try
                    {
                        var newDate = DateTimeOffset.FromUnixTimeSeconds(seconds).DateTime;
                        ret.LastUpdateDate = newDate;
                    }
                    catch (ArgumentOutOfRangeException)
                    {
                        // Invalid date, leave blank
                    }
                }
                else if (!string.IsNullOrEmpty(fmIni.ReleaseDate))
                {
                    ret.LastUpdateDate = StringToDate(fmIni.ReleaseDate, out DateTime dt) ? (DateTime?)dt : null;
                }
            }

            ret.Description = fmIni.Descr;

            /*
               Notes:
                - fm.ini can specify a readme file, but it may not be the one we're looking for, as far as
                  detecting values goes. Reading all .txt and .rtf files is slightly slower but more accurate.

                - Although fm.ini wasn't used before NewDark, its presence doesn't necessarily mean the mission
                  is NewDark-only. Sturmdrang Peak has it but doesn't require NewDark, for instance.
            */

            return ret;
        }

        private (string Title, string Author)
        ReadModIni(NameAndIndex file)
        {
            var ret = (Title: (string)null, Author: (string)null);

            string[] lines;

            if (FMIsZip)
            {
                var e = Archive.Entries[file.Index];
                using var es = e.Open();
                lines = ReadAllLinesE(es, e.Length);
            }
            else
            {
                lines = ReadAllLinesE(Path.Combine(FMWorkingPath, file.Name));
            }

            if (lines == null || lines.Length == 0) return ret;

            for (int i = 0; i < lines.Length; i++)
            {
                string lineT = lines[i].Trim();
                if (lineT.EqualsI("[modName]"))
                {
                    while (i < lines.Length - 1)
                    {
                        string lt = lines[i + 1].Trim();
                        if (!string.IsNullOrEmpty(lt) && lt[0] == '[' && lt[lt.Length - 1] == ']')
                        {
                            break;
                        }
                        else if (!string.IsNullOrEmpty(lt) && lt[0] != ';')
                        {
                            ret.Title = lt;
                            break;
                        }
                        i++;
                    }
                }
                else if (lineT.EqualsI("[authors]"))
                {
                    while (i < lines.Length - 1)
                    {
                        string lt = lines[i + 1].Trim();
                        if (!string.IsNullOrEmpty(lt) && lt[0] == '[' && lt[lt.Length - 1] == ']')
                        {
                            break;
                        }
                        else if (!string.IsNullOrEmpty(lt) && lt[0] != ';')
                        {
                            ret.Author = lt;
                            break;
                        }
                        i++;
                    }
                }
            }

            return ret;
        }

        // Because RTF files can have embedded images, their size can far exceed that normally expected of a
        // readme. To save time and memory, this method strips out such large data blocks before passing the
        // result to a WinForms RichTextBox for final conversion to plain text.
        private static bool GetRtfFileLinesAndText(Stream stream, int streamLength, RichTextBox rtfBox)
        {
            if (stream.Position > 0) stream.Position = 0;

            // Don't parse files small enough to be unlikely to have embedded images; otherwise we're just
            // parsing it twice for nothing
            if (streamLength < 262_144)
            {
                rtfBox.LoadFile(stream, RichTextBoxStreamType.RichText);
                stream.Position = 0;
                return true;
            }

            var byteList = new List<byte>();
            byte stack = 0;
            for (long i = 0; i < streamLength; i++)
            {
                // Just in case there's a malformed file or something
                if (stack > 100)
                {
                    stream.Position = 0;
                    return false;
                }

                int b = stream.ReadByte();
                if (b == '{')
                {
                    if (i < streamLength - 11)
                    {
                        stream.Read(RtfTags.Bytes11, 0, RtfTags.Bytes11.Length);

                        Array.Copy(RtfTags.Bytes11, RtfTags.Bytes10, 10);
                        Array.Copy(RtfTags.Bytes11, RtfTags.Bytes5, 5);

                        if (RtfTags.Bytes10.SequenceEqual(RtfTags.shppict) ||
                            RtfTags.Bytes10.SequenceEqual(RtfTags.objdata) ||
                            RtfTags.Bytes11.SequenceEqual(RtfTags.nonshppict) ||
                            RtfTags.Bytes5.SequenceEqual(RtfTags.pict))
                        {
                            stack++;
                            stream.Position -= RtfTags.Bytes11.Length;
                            continue;
                        }

                        if (stack > 0) stack++;
                        stream.Position -= RtfTags.Bytes11.Length;
                    }
                }
                else if (b == '}' && stack > 0)
                {
                    stack--;
                    continue;
                }

                if (stack == 0) byteList.Add((byte)b);
            }

            using var trimmedMemStream = new MemoryStream(byteList.ToArray());
            rtfBox.LoadFile(trimmedMemStream, RichTextBoxStreamType.RichText);
            stream.Position = 0;
            return true;
        }

        private void ReadAndCacheReadmeFiles(List<NameAndIndex> readmeDirFiles, RichTextBox rtfBox)
        {
            // Note: .wri files look like they may be just plain text with garbage at the top. Shrug.
            // Treat 'em like plaintext and see how it goes.

            foreach (NameAndIndex readmeFile in readmeDirFiles)
            {
                if (!readmeFile.Name.IsValidReadme()) continue;

                ZipArchiveEntry readmeEntry = null;

                if (FMIsZip) readmeEntry = Archive.Entries[readmeFile.Index];

                FileInfo readmeFI = FMDirFiles.Count > 0
                    ? FMDirFiles.FirstOrDefault(x => x.Name.EqualsI(Path.Combine(FMWorkingPath, readmeFile.Name)))
                    : null;

                int readmeFileLen =
                    FMIsZip ? (int)readmeEntry.Length :
                    readmeFI != null ? (int)readmeFI.Length :
                    (int)new FileInfo(Path.Combine(FMWorkingPath, readmeFile.Name)).Length;

                string readmeFileOnDisk = "";

                string fileName;
                DateTime lastModifiedDate;
                long readmeSize;

                if (FMIsZip)
                {
                    fileName = readmeEntry.Name;
                    lastModifiedDate =
                        new DateTimeOffset(ZipHelpers.ZipTimeToDateTime(readmeEntry.LastWriteTime)).DateTime;
                    readmeSize = readmeEntry.Length;
                }
                else
                {
                    readmeFileOnDisk = Path.Combine(FMWorkingPath, readmeFile.Name);
                    FileInfo fi = readmeFI ?? new FileInfo(readmeFileOnDisk);
                    fileName = fi.Name;
                    lastModifiedDate = new DateTimeOffset(fi.LastWriteTime).DateTime;
                    readmeSize = fi.Length;
                }

                if (readmeSize == 0) continue;

                ReadmeFiles.Add(new ReadmeInternal
                {
                    FileName = fileName,
                    LastModifiedDate = lastModifiedDate
                });

                if (readmeFile.Name.ExtIsHtml() || !readmeFile.Name.IsEnglishReadme()) continue;

                // try-finally instead of using, because we only want to initialize the readme stream if FMIsZip
                Stream readmeStream = null;
                try
                {
                    if (FMIsZip)
                    {
                        readmeStream = new MemoryStream(readmeFileLen);
                        using (var es = readmeEntry.Open()) es.CopyTo(readmeStream);

                        readmeStream.Position = 0;
                    }

                    // Saw one ".rtf" that was actually a plaintext file, and one vice versa. So detect by
                    // header alone.
                    char[] rtfHeader = new char[6];
                    using (var sr = FMIsZip
                        ? new StreamReader(readmeStream, Encoding.ASCII, false, 6, true)
                        : new StreamReader(readmeFileOnDisk, Encoding.ASCII, false, 6))
                    {
                        sr.ReadBlock(rtfHeader, 0, 6);
                    }
                    if (FMIsZip) readmeStream.Position = 0;

                    if (string.Concat(rtfHeader).EqualsI(@"{\rtf1"))
                    {
                        bool success;
                        if (FMIsZip)
                        {
                            success = GetRtfFileLinesAndText(readmeStream, readmeFileLen, rtfBox);
                        }
                        else
                        {
                            using var fs = new FileStream(readmeFileOnDisk, FileMode.Open, FileAccess.Read);
                            success = GetRtfFileLinesAndText(fs, readmeFileLen, rtfBox);
                        }

                        if (success)
                        {
                            ReadmeInternal last = ReadmeFiles[ReadmeFiles.Count - 1];
                            last.Lines = rtfBox.Lines;
                            last.Text = rtfBox.Text;
                        }
                    }
                    else
                    {
                        ReadmeInternal last = ReadmeFiles[ReadmeFiles.Count - 1];
                        last.Lines = FMIsZip
                            ? ReadAllLinesE(readmeStream, readmeFileLen, streamIsSeekable: true)
                            : ReadAllLinesE(readmeFileOnDisk);

                        // Convert GLML files to plaintext by stripping the markup. Fortunately this is extremely
                        // easy as all tags are of the form [GLWHATEVER][/GLWHATEVER]. Very nice, very simple.
                        if (last.FileName.ExtIsGlml())
                        {
                            for (int i = 0; i < last.Lines.Length; i++)
                            {
                                var matches = GLMLTagRegex.Matches(last.Lines[i]);
                                foreach (Match m in matches)
                                {
                                    last.Lines[i] = last.Lines[i].Replace(m.Value, m.Value == "[GLNL]" ? "\r\n" : "");
                                }
                            }
                        }

                        last.Text = string.Join("\r\n", last.Lines);
                    }
                }
                finally
                {
                    readmeStream?.Dispose();
                }
            }
        }

        private List<string> GetTitlesStrLines(List<NameAndIndex> stringsDirFiles)
        {
            string[] titlesStrLines = null;

            #region Read title(s).str file

            // Do not change search order: strings/english, strings, strings/[any other language]
            var titlesStrDirs = new List<string>
            {
                FMDirs.StringsS(dsc) + "english" + dsc + FMFiles.TitlesStr,
                FMDirs.StringsS(dsc) + "english" + dsc + FMFiles.TitleStr,
                FMDirs.StringsS(dsc) + FMFiles.TitlesStr,
                FMDirs.StringsS(dsc) + FMFiles.TitleStr
            };
            foreach (string lang in Languages)
            {
                if (lang == "english") continue;

                titlesStrDirs.Add(FMDirs.StringsS(dsc) + lang + dsc + FMFiles.TitlesStr);
                titlesStrDirs.Add(FMDirs.StringsS(dsc) + lang + dsc + FMFiles.TitleStr);
            }

            foreach (string titlesFileLocation in titlesStrDirs)
            {
                NameAndIndex titlesFile = FMIsZip
                    ? stringsDirFiles.FirstOrDefault(x => x.Name.EqualsI(titlesFileLocation))
                    : new NameAndIndex { Name = Path.Combine(FMWorkingPath, titlesFileLocation) };

                if (titlesFile == null || !FMIsZip && !File.Exists(titlesFile.Name)) continue;

                if (FMIsZip)
                {
                    var e = Archive.Entries[titlesFile.Index];
                    using var es = e.Open();
                    titlesStrLines = ReadAllLinesE(es, e.Length);
                }
                else
                {
                    titlesStrLines = ReadAllLinesE(titlesFile.Name);
                }

                break;
            }

            #endregion

            if (titlesStrLines == null || titlesStrLines.Length == 0) return null;

            #region Filter titlesStrLines

            // There's a way to do this with an IEqualityComparer, but no, for reasons
            var tfLinesD = new List<string>(titlesStrLines.Length);
            {
                for (int i = 0; i < titlesStrLines.Length; i++)
                {
                    // Note: the Trim() is important, don't remove it
                    string line = titlesStrLines[i].Trim();
                    if (!string.IsNullOrEmpty(line) &&
                        line.Contains(':') &&
                        line.CountChars('\"') > 1 &&
                        line.StartsWithI("title_") &&
                        !tfLinesD.Any(x => x.StartsWithI(line.Substring(0, line.IndexOf(':')))))
                    {
                        tfLinesD.Add(line);
                    }
                }
            }

            tfLinesD.Sort(new TitlesStrNaturalNumericSort());

            #endregion

            return tfLinesD;
        }

        private (string TitleFrom0, string TitleFromNumbered, string[] CampaignMissionNames)
        GetMissionNames(List<NameAndIndex> stringsDirFiles, List<NameAndIndex> misFiles, List<NameAndIndex> usedMisFiles)
        {
            var titlesStrLines = GetTitlesStrLines(stringsDirFiles);
            if (titlesStrLines == null || titlesStrLines.Count == 0) return (null, null, null);

            var ret =
                (TitleFrom0: (string)null,
                TitleFromNumbered: (string)null,
                CampaignMissionNames: (string[])null);

            static string ExtractFromQuotedSection(string line)
            {
                int i;
                return line.Substring(i = line.IndexOf('\"') + 1, line.IndexOf('\"', i) - i);
            }

            var titles = new List<string>(titlesStrLines.Count);
            for (int lineIndex = 0; lineIndex < titlesStrLines.Count; lineIndex++)
            {
                string titleNum = null;
                string title = null;
                for (int umfIndex = 0; umfIndex < usedMisFiles.Count; umfIndex++)
                {
                    string line = titlesStrLines[lineIndex];
                    int i;
                    titleNum = line.Substring(i = line.IndexOf('_') + 1, line.IndexOf(':') - i).Trim();
                    if (titleNum == "0") ret.TitleFrom0 = ExtractFromQuotedSection(line);

                    title = ExtractFromQuotedSection(line);
                    if (string.IsNullOrEmpty(title)) continue;

                    string umfNoExt = usedMisFiles[umfIndex].Name.RemoveExtension();
                    if (umfNoExt != null && umfNoExt.StartsWithI("miss") && umfNoExt.Length > 4 &&
                        titleNum == umfNoExt.Substring(4))
                    {
                        titles.Add(title);
                    }
                }

                if (ScanOptions.ScanTitle &&
                    ret.TitleFromNumbered.IsEmpty() &&
                    lineIndex == titlesStrLines.Count - 1 &&
                    !string.IsNullOrEmpty(titleNum) &&
                    !string.IsNullOrEmpty(title) &&
                    !usedMisFiles.Any(x => x.Name.ContainsI("miss" + titleNum + ".mis")) &&
                    misFiles.Any(x => x.Name.ContainsI("miss" + titleNum + ".mis")))
                {
                    ret.TitleFromNumbered = title;
                    if (!ScanOptions.ScanCampaignMissionNames) break;
                }
            }

            if (titles.Count > 0)
            {
                if (ScanOptions.ScanTitle && titles.Count == 1)
                {
                    ret.TitleFromNumbered = titles[0];
                }
                else if (ScanOptions.ScanCampaignMissionNames)
                {
                    ret.CampaignMissionNames = titles.ToArray();
                }
            }

            return ret;
        }

        // This is kind of just an excuse to say that my scanner can catch the full proper title of Deceptive
        // Perception 2. :P
        // This is likely to be a bit loose with its accuracy, but since values caught here are almost certain to
        // end up as alternate titles, I can afford that.
        private List<string> GetTitlesFromTopOfReadmes(List<ReadmeInternal> readmes)
        {
            var ret = new List<string>();

            if (ReadmeFiles == null || ReadmeFiles.Count == 0) return ret;

            const int maxTopLines = 5;

            foreach (ReadmeInternal r in readmes)
            {
                if (r.FileName.ExtIsHtml() || r.Lines == null || r.Lines.Length == 0) continue;

                var lines = new List<string>();

                int fullLineCount = 0;
                for (int i = 0; i < r.Lines.Length; i++)
                {
                    string line = r.Lines[i];
                    if (!string.IsNullOrWhiteSpace(line))
                    {
                        lines.Add(line);
                        fullLineCount++;
                    }
                    if (fullLineCount == maxTopLines) break;
                }

                if (lines.Count < 2) continue;

                string titleConcat = "";

                for (int i = 0; i < lines.Count; i++)
                {
                    string lineT = lines[i].Trim();
                    if (i > 0 &&
                        lineT.StartsWithI("By ") || lineT.StartsWithI("By: ") ||
                        lineT.StartsWithI("Original concept by ") ||
                        lineT.StartsWithI("Created by ") ||
                        lineT.StartsWithI("A Thief 2 fan") ||
                        lineT.StartsWithI("A Thief Gold fan") ||
                        lineT.StartsWithI("A Thief 1 fan") ||
                        lineT.StartsWithI("A Thief fan") ||
                        lineT.StartsWithI("A fan mission"))
                    {
                        for (int j = 0; j < i; j++)
                        {
                            if (j > 0) titleConcat += " ";
                            titleConcat += lines[j];
                        }
                        // Set a cutoff for the length so we don't end up with a huge string that's obviously
                        // more than a title
                        if (!string.IsNullOrWhiteSpace(titleConcat) && titleConcat.Length <= 50)
                        {
                            ret.Add(CleanupTitle(titleConcat));
                        }

                        break;
                    }
                }
            }

            return ret;
        }

        private string
        GetValueFromReadme(SpecialLogic specialLogic, List<string> titles = null, params string[] keys)
        {
            string ret = null;

            foreach (ReadmeInternal file in ReadmeFiles.Where(x => !x.FileName.ExtIsHtml() && x.FileName.IsEnglishReadme()))
            {
                if (specialLogic == SpecialLogic.NewDarkMinimumVersion)
                {
                    string ndv = GetNewDarkVersionFromText(file.Text);
                    if (!string.IsNullOrEmpty(ndv)) return ndv;
                }
                else
                {
                    if (specialLogic == SpecialLogic.Author)
                    {
                        /*
                            Check this first so as to avoid:

                            Briefing Movie
                            Created by Yandros using VideoPad by NCH Software
                        */
                        ret = GetAuthorFromTopOfReadme(file.Lines, titles);
                        if (!string.IsNullOrEmpty(ret)) return ret;
                    }

                    ret = GetValueFromLines(specialLogic, keys, file.Lines);
                    if (string.IsNullOrEmpty(ret))
                    {
                        if (specialLogic == SpecialLogic.Author)
                        {
                            ret = GetAuthorFromText(file.Text);
                            if (!string.IsNullOrEmpty(ret)) return ret;
                        }
                    }
                    else
                    {
                        return ret;
                    }
                }
            }

            // Do the less common cases separately so as not to slow down the main ones with checks that are
            // statistically unlikely to find anything
            if (specialLogic == SpecialLogic.Author && string.IsNullOrEmpty(ret))
            {
                // We do this separately for performance and clarity; it's an uncommon case involving regex
                // searching and we don't want to run it unless we have to. Also, it's specific enough that
                // we don't really want to shoehorn it into the standard line search.
                ret = GetAuthorFromCopyrightMessage();

                if (!string.IsNullOrEmpty(ret)) return ret;

                // Finds eg.
                // Author:
                //      GORT (Shaun M.D. Morin)
                foreach (ReadmeInternal file in ReadmeFiles.Where(x => !x.FileName.ExtIsHtml() && x.FileName.IsEnglishReadme()))
                {
                    ret = GetValueFromLines(SpecialLogic.AuthorNextLine, null, file.Lines);
                    if (!string.IsNullOrEmpty(ret)) return ret;
                }

                // Very last resort, because it has a dynamic regex in it
                ret = GetAuthorFromTitleByAuthorLine(titles);
            }

            return ret;
        }

        private static string GetValueFromLines(SpecialLogic specialLogic, string[] keys, string[] lines)
        {
            if (specialLogic == SpecialLogic.AuthorNextLine)
            {
                for (int i = 0; i < lines.Length; i++)
                {
                    string lineT = lines[i].Trim();
                    if (!lineT.EqualsI("Author") && !lineT.EqualsI("Author:")) continue;

                    if (i < lines.Length - 2)
                    {
                        string lineAfterNext = lines[i + 2].Trim();
                        int lanLen = lineAfterNext.Length;
                        if ((lanLen > 0 && lineAfterNext[lanLen - 1] == ':' && lineAfterNext.Length <= 50) ||
                            string.IsNullOrWhiteSpace(lineAfterNext))
                        {
                            return lines[i + 1].Trim();
                        }
                    }
                }

                return null;
            }

            for (int lineIndex = 0; lineIndex < lines.Length; lineIndex++)
            {
                string line = lines[lineIndex];
                string lineStartTrimmed = line.TrimStart();

                #region Excludes

                switch (specialLogic)
                {
                    // I can't believe fallthrough is actually useful (for visual purposes only, but still!)
                    case SpecialLogic.Title when
                        lineStartTrimmed.StartsWithI("Title & Description") ||
                        lineStartTrimmed.StartsWithGL("Title screen"):
                    case SpecialLogic.Version when
                        lineStartTrimmed.StartsWithI("Version History") ||
                        lineStartTrimmed.ContainsI("NewDark") ||
                        lineStartTrimmed.ContainsI("64 Cubed") ||
                        VersionExclude1Regex.Match(lineStartTrimmed).Success:
                    case SpecialLogic.Author when
                        lineStartTrimmed.StartsWithI("Authors note"):
                        continue;
                }

                #endregion

                bool lineStartsWithKey = false;
                bool lineStartsWithKeyAndSeparatorChar = false;
                for (int i = 0; i < keys.Length; i++)
                {
                    string x = keys[i];

                    // Either in given case or in all caps, but not in lowercase, because that's given me at
                    // least one false positive
                    if (lineStartTrimmed.StartsWithGU(x))
                    {
                        lineStartsWithKey = true;
                        // Regex perf: fast enough not to worry about it
                        if (Regex.Match(lineStartTrimmed, @"^" + x + @"\s*(:|-|\u2013)", RegexOptions.IgnoreCase)
                            .Success)
                        {
                            lineStartsWithKeyAndSeparatorChar = true;
                            break;
                        }
                    }
                }
                if (!lineStartsWithKey) continue;

                if (lineStartsWithKeyAndSeparatorChar)
                {
                    int indexColon = lineStartTrimmed.IndexOf(':');
                    int indexDash = lineStartTrimmed.IndexOf('-');
                    int indexUnicodeDash = lineStartTrimmed.IndexOf('\u2013');

                    int index = indexColon > -1 && indexDash > -1
                        ? Math.Min(indexColon, indexDash)
                        : Math.Max(indexColon, indexDash);

                    if (index == -1) index = indexUnicodeDash;

                    string finalValue = lineStartTrimmed.Substring(index + 1).Trim();
                    if (!string.IsNullOrEmpty(finalValue)) return finalValue;
                }
                else
                {
                    // Don't detect "Version "; too many false positives
                    // TODO: Can probably remove this check and then just sort out any false positives in
                    // TODO: GetVersion()
                    if (specialLogic == SpecialLogic.Version) continue;

                    for (int i = 0; i < keys.Length; i++)
                    {
                        string key = keys[i];
                        if (!lineStartTrimmed.StartsWithI(key)) continue;

                        // It's supposed to be finding a space after a key; this prevents it from finding the
                        // first space in the key itself if there is one.
                        string lineAfterKey = lineStartTrimmed.Remove(0, key.Length);

                        if (!string.IsNullOrEmpty(lineAfterKey) &&
                            (lineAfterKey[0] == ' ' || lineAfterKey[0] == '\t'))
                        {
                            string finalValue = lineAfterKey.TrimStart();
                            if (!string.IsNullOrEmpty(finalValue)) return finalValue;
                        }
                    }
                }
            }

            return null;
        }

        private static string CleanupValue(string value)
        {
            if (string.IsNullOrEmpty(value)) return value;

            string ret = value.TrimEnd();

            // Remove surrounding quotes
            if (ret[0] == '\"' && ret[ret.Length - 1] == '\"') ret = ret.Trim(CA_DoubleQuote);
            if ((ret[0] == uldq || ret[0] == urdq) &&
                (ret[ret.Length - 1] == uldq || ret[ret.Length - 1] == urdq))
            {
                ret = ret.Trim(CA_UnicodeQuotes);
            }

            ret = ret.RemoveUnpairedLeadingOrTrailingQuotes();

            // Remove duplicate spaces
            ret = Regex.Replace(ret, @"\s{2,}", " ");
            ret = ret.Replace('\t', ' ');

            #region Parentheses

            ret = ret.RemoveSurroundingParentheses();

            bool containsOpenParen = ret.Contains('(');
            bool containsCloseParen = ret.Contains(')');

            // Remove extraneous whitespace within parentheses
            if (containsOpenParen) ret = OpenParenSpacesRegex.Replace(ret, "(");
            if (containsCloseParen) ret = CloseParenSpacesRegex.Replace(ret, ")");

            // If there's stuff like "(this an incomplete sentence and" at the end, chop it right off
            if (containsOpenParen && ret.CountChars('(') == 1 && !containsCloseParen)
            {
                ret = ret.Substring(0, ret.LastIndexOf('(')).TrimEnd();
            }

            #endregion

            return ret;
        }

        private string GetTitleFromNewGameStrFile(List<NameAndIndex> intrfaceDirFiles)
        {
            if (intrfaceDirFiles.Count == 0) return null;
            var newGameStrFile = new NameAndIndex();

            if (intrfaceDirFiles.Count > 0)
            {
                newGameStrFile =
                    intrfaceDirFiles.FirstOrDefault(x =>
                        x.Name.EqualsI(FMDirs.IntrfaceS(dsc) + "english" + dsc + FMFiles.NewGameStr))
                    ?? intrfaceDirFiles.FirstOrDefault(x =>
                        x.Name.EqualsI(FMDirs.IntrfaceS(dsc) + FMFiles.NewGameStr))
                    ?? intrfaceDirFiles.FirstOrDefault(x =>
                        x.Name.StartsWithI(FMDirs.IntrfaceS(dsc)) &&
                        x.Name.EndsWithI(dsc + FMFiles.NewGameStr));
            }

            if (newGameStrFile == null) return null;

            string[] lines;

            if (FMIsZip)
            {
                var e = Archive.Entries[newGameStrFile.Index];
                using var es = e.Open();
                lines = ReadAllLinesE(es, e.Length);
            }
            else
            {
                lines = ReadAllLinesE(Path.Combine(FMWorkingPath, newGameStrFile.Name));
            }

            if (lines == null) return null;

            for (int i = 0; i < lines.Length; i++)
            {
                string lineT = lines[i].Trim();
                Match match = NewGameStrTitleRegex.Match(lineT);
                if (match.Success)
                {
                    string title = match.Groups["Title"].Value.Trim();
                    if (string.IsNullOrEmpty(title)) continue;

                    // Do our best to ignore things that aren't titles
                    if ("{}-_:;!@#$%^&*()".All(x => title[0] != x) &&
                        !title.EqualsI("Play") && !title.EqualsI("Start") &&
                        !title.EqualsI("Begin") && !title.EqualsI("Begin...") &&
                        !title.EqualsI("skip training") &&
                        !title.StartsWithI("Let's go") && !title.StartsWithI("Let's rock this boat") &&
                        !title.StartsWithI("Play ") && !title.StartsWithI("Continue") &&
                        !title.StartsWithI("Start ") && !title.StartsWithI("Begin "))
                    {
                        return title;
                    }
                }
            }

            return null;
        }

        private static string CleanupTitle(string value)
        {
            if (string.IsNullOrEmpty(value)) return value;

            // Some titles are clever and  A r e  W r i t t e n  L i k e  T h i s
            // But we want to leave titles that are supposed to be acronyms - ie, "U F O", "R G B"
            if (value.Contains(' ') &&
                !TitleAnyConsecutiveLettersRegex.Match(value).Success &&
                TitleContainsLowerCaseCharsRegex.Match(value).Success)
            {
                if (value.Contains("  "))
                {
                    string[] titleWords = value.Split(new[] { "  " }, StringSplitOptions.None);
                    for (int i = 0; i < titleWords.Length; i++)
                    {
                        titleWords[i] = titleWords[i].Replace(" ", "");
                    }

                    value = string.Join(" ", titleWords);
                }
                else
                {
                    value = value.Replace(" ", "");
                }
            }

            value = CleanupValue(value);

            return value;
        }

        private static string GetAuthorFromTopOfReadme(string[] linesArray, List<string> titles)
        {
            if (linesArray.Length == 0) return null;

            bool titleStartsWithBy = false;
            bool titleContainsBy = false;
            if (titles != null)
            {
                foreach (string title in titles)
                {
                    if (title.StartsWithI("by ")) titleStartsWithBy = true;
                    if (title.ContainsI(" by ")) titleContainsBy = true;
                }
            }

            const int maxTopLines = 5;

            // Look for a "by [author]" in the first few lines. Looking for a line starting with "by" throughout
            // the whole text is asking for a cavalcade of false positives, hence why we only look near the top.
            var lines = new List<string>();

            int fullLineCount = 0;
            for (int i = 0; i < linesArray.Length; i++)
            {
                string line = linesArray[i];
                if (!string.IsNullOrWhiteSpace(line))
                {
                    lines.Add(line);
                    fullLineCount++;
                }
                if (fullLineCount == maxTopLines) break;
            }

            if (lines.Count < 2) return null;

            for (int i = 0; i < lines.Count; i++)
            {
                // Preemptive check
                if (i == 0 && titleStartsWithBy) continue;

                string lineT = lines[i].Trim();
                if (lineT.StartsWithI("By ") || lineT.StartsWithI("By: "))
                {
                    string author = lineT.Substring(lineT.IndexOf(' ')).TrimStart();
                    if (!string.IsNullOrEmpty(author)) return author;
                }
                else if (lineT.EqualsI("By"))
                {
                    if (!titleContainsBy && i < lines.Count - 1)
                    {
                        return lines[i + 1].Trim();
                    }
                }
                else
                {
                    Match m = AuthorGeneralCopyrightRegex.Match(lineT);
                    if (!m.Success) continue;

                    string author = CleanupCopyrightAuthor(m.Groups["Author"].Value);
                    if (!string.IsNullOrEmpty(author)) return author;
                }
            }

            return null;
        }

        private static string GetAuthorFromText(string text)
        {
            string author = null;

            for (int i = 0; i < AuthorRegexes.Length; i++)
            {
                Match match = AuthorRegexes[i].Match(text);
                if (match.Success)
                {
                    author = match.Groups["Author"].Value;
                    break;
                }
            }

            return !string.IsNullOrEmpty(author) ? author : null;
        }

        private string GetAuthorFromTitleByAuthorLine(List<string> titles)
        {
            if (titles == null || titles.Count == 0) return null;

            // With the new fuzzy match method, it might be possible for me to remove the need for this guard
            for (int i = 0; i < titles.Count; i++)
            {
                if (titles[i].ContainsI(" by "))
                {
                    titles.RemoveAt(i);
                    i--;
                }
            }

            if (titles.Count == 0) return null;

            var titleByAuthorRegex = new Regex(@"(\s+|\s*(:|-|\u2013|,)\s*)by(\s+|\s*(:|-|\u2013)\s*)(?<Author>.+)",
                RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);

            // We DON'T just check the first five lines, because there might be another language section first
            // and this kind of author string might well be buried down in the file.
            foreach (ReadmeInternal rf in ReadmeFiles.Where(x => !x.FileName.ExtIsHtml() && x.FileName.IsEnglishReadme()))
            {
                foreach (string line in rf.Lines)
                {
                    string lineT = line.Trim();

                    if (!lineT.ContainsI(" by ")) continue;

                    string titleCandidate = lineT.Substring(0, lineT.IndexOf(" by", OrdinalIgnoreCase)).Trim();

                    bool fuzzyMatched = false;
                    foreach (string title in titles)
                    {
                        if (titleCandidate.SimilarityTo(title, OrdinalIgnoreCase) > 0.75)
                        {
                            fuzzyMatched = true;
                            break;
                        }
                    }
                    if (!fuzzyMatched) continue;

                    string secondHalf = lineT.Substring(lineT.IndexOf(" by", OrdinalIgnoreCase));

                    Match match = titleByAuthorRegex.Match(secondHalf);
                    if (match.Success)
                    {
                        return match.Groups["Author"].Value;
                    }
                }
            }

            return null;
        }

        private string GetAuthorFromCopyrightMessage()
        {
            static string AuthorCopyrightRegexesMatch(string line)
            {
                for (int i = 0; i < AuthorMissionCopyrightRegexes.Length; i++)
                {
                    Match match = AuthorMissionCopyrightRegexes[i].Match(line);
                    if (match.Success) return match.Groups["Author"].Value;
                }
                return null;
            }

            string author = null;

            bool foundAuthor = false;

            foreach (ReadmeInternal rf in ReadmeFiles.Where(x => !x.FileName.ExtIsHtml() && x.FileName.IsEnglishReadme()))
            {
                bool inCopyrightSection = false;
                bool pastFirstLineOfCopyrightSection = false;

                foreach (string line in rf.Lines)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    if (inCopyrightSection)
                    {
                        // This whole nonsense is just to support the use of @ as a copyright symbol (used by some
                        // Theker missions); we want to be very specific about when we decide that "@" means "©".
                        Match m = !pastFirstLineOfCopyrightSection
                            ? AuthorGeneralCopyrightIncludeAtSymbolRegex.Match(line)
                            : AuthorGeneralCopyrightRegex.Match(line);
                        if (m.Success)
                        {
                            author = m.Groups["Author"].Value;
                            foundAuthor = true;
                            break;
                        }

                        pastFirstLineOfCopyrightSection = true;
                    }

                    author = AuthorCopyrightRegexesMatch(line);
                    if (!string.IsNullOrEmpty(author))
                    {
                        foundAuthor = true;
                        break;
                    }

                    string lineT = line.Trim(CA_Asterisk).Trim(CA_Hyphen).Trim();
                    if (lineT.EqualsI("Copyright Information") || lineT.EqualsI("Copyright"))
                    {
                        inCopyrightSection = true;
                    }
                }

                if (foundAuthor) break;
            }

            if (string.IsNullOrWhiteSpace(author)) return null;

            author = CleanupCopyrightAuthor(author);

            return author;
        }

        private static string CleanupCopyrightAuthor(string author)
        {
            author = author.Trim().RemoveSurroundingParentheses();

            int index = author.IndexOf(',');
            if (index > -1) author = author.Substring(0, index);

            index = author.IndexOf(". ", Ordinal);
            if (index > -1) author = author.Substring(0, index);

            Match yearMatch = CopyrightAuthorYearRegex.Match(author);
            if (yearMatch.Success) author = author.Substring(0, yearMatch.Index);

            if ("!@#$%^&*".Any(x => author[author.Length - 1] == x) &&
                author.ElementAt(author.Length - 2) == ' ')
            {
                author = author.Substring(0, author.Length - 2);
            }

            author = author.TrimEnd(CA_Period).Trim();

            return author;
        }

        private string GetVersion()
        {
            string version = GetValueFromReadme(SpecialLogic.Version, null, SA_VersionDetect);

            if (string.IsNullOrEmpty(version)) return null;

            Debug.WriteLine(@"GetVersion() top:");
            Debug.WriteLine(version);

            const string numbers = "0123456789.";
            if (numbers.Any(x => version[0] == x))
            {
                int indexSpace = version.IndexOf(' ');
                int indexTab = version.IndexOf('\t');

                int index = indexSpace > -1 && indexTab > -1
                    ? Math.Min(indexSpace, indexTab)
                    : Math.Max(indexSpace, indexTab);

                if (index > -1)
                {
                    version = version.Substring(0, index);
                }
            }
            else // Starts with non-numbers
            {
                // Find index of the first numeric character
                Match match = VersionFirstNumberRegex.Match(version);
                if (match.Success)
                {
                    version = version.Substring(match.Index);

                    int indexSpace = version.IndexOf(' ');
                    int indexTab = version.IndexOf('\t');

                    int index = indexSpace > -1 && indexTab > -1
                        ? Math.Min(indexSpace, indexTab)
                        : Math.Max(indexSpace, indexTab);

                    if (index > -1)
                    {
                        version = version.Substring(0, index);
                    }
                }
            }

            Debug.WriteLine(@"GetVersion() bottom:");
            Debug.WriteLine(version);

            return version;
        }

        // TODO: Add all missing languages, and implement language detection for non-folder-specified FMs
        private (string[] Langs, string[] UncertainLangs)
        GetLanguages(List<NameAndIndex> baseDirFiles, List<NameAndIndex> booksDirFiles,
            List<NameAndIndex> intrfaceDirFiles, List<NameAndIndex> stringsDirFiles)
        {
            var langs = new List<string>();
            var uncertainLangs = new List<string>();

            for (int dirIndex = 0; dirIndex < 3; dirIndex++)
            {
                var dirFiles = dirIndex switch
                {
                    0 => booksDirFiles,
                    1 => intrfaceDirFiles,
                    _ => stringsDirFiles
                };

                for (int langIndex = 0; langIndex < Languages.Length; langIndex++)
                {
                    string lang = Languages[langIndex];
                    for (int dfIndex = 0; dfIndex < dirFiles.Count; dfIndex++)
                    {
                        NameAndIndex df = dirFiles[dfIndex];
                        if (df.Name.HasFileExtension() &&
                            (df.Name.ContainsI(dsc + lang + dsc) ||
                             df.Name.ContainsI(dsc + lang + " Language" + dsc)))
                        {
                            langs.Add(lang);
                        }
                    }
                }
            }

            if (!langs.ContainsI("english"))
            {
                langs.Add("english");
                uncertainLangs.Add("english");
            }

            // Sometimes extra languages are in zip files inside the FM archive
            for (int i = 0; i < baseDirFiles.Count; i++)
            {
                string fn = baseDirFiles[i].Name;
                if (!fn.ExtIsZip() && !fn.ExtIs7z() && !fn.ExtIsRar())
                {
                    continue;
                }

                fn = fn.RemoveExtension();

                langs.AddRange(
                    from lang in Languages
                    where fn.StartsWithI(lang)
                    select lang);

                // "Italiano" will be caught by StartsWithI("italian")

                // Extra logic to account for whatever-style naming
                if (fn.EqualsI("rus") ||
                    fn.EndsWithI("_ru") ||
                    fn.EndsWithI("_rus") ||
                    Regex.Match(fn, @"[a-z]+RUS$").Success ||
                    fn.ContainsI("RusPack") || fn.ContainsI("RusText"))
                {
                    langs.Add("russian");
                }
                else if (fn.ContainsI("Francais"))
                {
                    langs.Add("french");
                }
                else if (fn.ContainsI("Deutsch") || fn.ContainsI("Deutch"))
                {
                    langs.Add("german");
                }
                else if (fn.ContainsI("Espanol"))
                {
                    langs.Add("spanish");
                }
                else if (fn.ContainsI("Nederlands"))
                {
                    langs.Add("dutch");
                }
                else if (fn.EqualsI("huntext"))
                {
                    langs.Add("hungarian");
                }
            }

            if (langs.Count > 0)
            {
                string[] langsD = langs.Distinct().ToArray();
                Array.Sort(langsD);
                return (langsD, uncertainLangs.ToArray());
            }
            else
            {
                return (new[] { "english" }, new[] { "english" });
            }
        }

        private (bool? NewDarkRequired, Game Game)
        GetGameTypeAndEngine(List<NameAndIndex> baseDirFiles, List<NameAndIndex> usedMisFiles)
        {
            var ret = (NewDarkRequired: (bool?)null, Game: Game.Null);

            #region Choose smallest .gam file

            NameAndIndex[] gamFiles = baseDirFiles.Where(x => x.Name.ExtIsGam()).ToArray();
            bool gamFileExists = gamFiles.Length > 0;

            var gamSizeList = new List<(string Name, int Index, long Size)>();
            NameAndIndex smallestGamFile = null;

            if (gamFileExists)
            {
                if (gamFiles.Length == 1)
                {
                    smallestGamFile = gamFiles[0];
                }
                else
                {
                    foreach (NameAndIndex gam in gamFiles)
                    {
                        long length;
                        if (FMIsZip)
                        {
                            length = Archive.Entries[gam.Index].Length;
                        }
                        else
                        {
                            FileInfo gamFI = FMDirFiles.FirstOrDefault(x => x.FullName.EqualsI(Path.Combine(FMWorkingPath, gam.Name)));
                            length = gamFI?.Length ?? new FileInfo(Path.Combine(FMWorkingPath, gam.Name)).Length;
                        }
                        gamSizeList.Add((gam.Name, gam.Index, length));
                    }

                    var gamToUse = gamSizeList.OrderBy(x => x.Size).First();
                    smallestGamFile = new NameAndIndex { Name = gamToUse.Name, Index = gamToUse.Index };
                }
            }

            #endregion

            #region Choose smallest .mis file

            var misSizeList = new List<(string Name, int Index, long Size)>();
            NameAndIndex smallestUsedMisFile;

            if (usedMisFiles.Count == 1)
            {
                smallestUsedMisFile = usedMisFiles[0];
            }
            else if (usedMisFiles.Count > 1)
            {
                foreach (NameAndIndex mis in usedMisFiles)
                {
                    long length;
                    if (FMIsZip)
                    {
                        length = Archive.Entries[mis.Index].Length;
                    }
                    else
                    {
                        FileInfo misFI = FMDirFiles.FirstOrDefault(x => x.FullName.EqualsI(Path.Combine(FMWorkingPath, mis.Name)));
                        length = misFI?.Length ?? new FileInfo(Path.Combine(FMWorkingPath, mis.Name)).Length;
                    }
                    misSizeList.Add((mis.Name, mis.Index, length));
                }

                var misToUse = misSizeList.OrderBy(x => x.Size).First();
                smallestUsedMisFile = new NameAndIndex { Name = misToUse.Name, Index = misToUse.Index };
            }
            else
            {
                // We know usedMisFiles can never be empty at this point because we early-return way before this
                // if it is, but the code analysis doesn't know that, so we put this in to prevent a couple of
                // null-reference warnings.
                throw new Exception(nameof(usedMisFiles) + ".Count is 0");
            }

            #endregion

            #region Setup

            ZipArchiveEntry gamFileZipEntry = null;
            ZipArchiveEntry misFileZipEntry = null;

            string misFileOnDisk = null;

            if (FMIsZip)
            {
                if (gamFileExists) gamFileZipEntry = Archive.Entries[smallestGamFile.Index];
                misFileZipEntry = Archive.Entries[smallestUsedMisFile.Index];
            }
            else
            {
                misFileOnDisk = Path.Combine(FMWorkingPath, smallestUsedMisFile.Name);
            }

            #endregion

            #region Check for SKYOBJVAR in .mis (determines OldDark/NewDark; determines game type for OldDark)

            /*
             SKYOBJVAR location key:
                 No SKYOBJVAR           - OldDark Thief 1/G
                 ~770                   - OldDark Thief 2                        Commonness: ~80%
                 ~7216                  - NewDark, could be either T1/G or T2    Commonness: ~14%
                 ~3092                  - NewDark, could be either T1/G or T2    Commonness: ~4%
                 Any other location*    - OldDark Thief2

            System Shock 2 .mis files can (but may not) have the SKYOBJVAR string. If they do, it'll be at 3168
            or 7292.
            System Shock 2 .mis files all have the MAPPARAM string. It will be at either 696 or 916. One or the
            other may correspond to NewDark but I dunno cause I haven't looked into it that far yet.
            (we don't detect OldDark/NewDark for SS2 yet)

             * We skip this check because only a handful of OldDark Thief 2 missions have SKYOBJVAR in a wacky
               location, and it's faster and more reliable to simply carry on with the secondary check than to
               try to guess where SKYOBJVAR is in this case.
            */

            // For folder scans, we can seek to these positions directly, but for zip scans, we have to read
            // through the stream sequentially until we hit each one.
            const int oldDarkT2Loc = 750;

            // These two locations just narrowly avoid the places where an SS2 SKYOBJVAR can be
            // (when read length is 100 bytes)
            const int newDarkLoc1 = 7180;
            const int newDarkLoc2 = 3050;

            const int ss2MapParamLoc1 = 670;
            const int ss2MapParamLoc2 = 870;
            int[] locations = { ss2MapParamLoc1, ss2MapParamLoc2, oldDarkT2Loc, newDarkLoc1, newDarkLoc2 };

            // 750+100 = 850
            // (3050+100)-850 = 2300
            // ((7180+100)-2300)-850 = 4130
            // For SS2, SKYOBJVAR is located after position 2300, so we can get away without explicitly accounting
            // for it here.
            // Extra dummy values to make its length match locations[]
            int[] zipOffsets = { -1, -1, 850, 2300, 4130 };

            const int locationBytesToRead = 100;
            bool foundAtNewDarkLocation = false;
            bool foundAtOldDarkThief2Location = false;

            char[] zipBuf = null;
            char[] dirBuf = new char[locationBytesToRead];

            using (var sr = FMIsZip
                ? new BinaryReader(misFileZipEntry.Open(), Encoding.ASCII, false)
                : new BinaryReader(new FileStream(misFileOnDisk, FileMode.Open, FileAccess.Read), Encoding.ASCII, false))
            {
                for (int i = 0; i < locations.Length; i++)
                {
                    if (FMIsZip)
                    {
                        if (zipOffsets[i] == -1) continue;
                        zipBuf = sr.ReadChars(zipOffsets[i]);
                    }
                    else
                    {
                        sr.BaseStream.Position = locations[i];
                        dirBuf = sr.ReadChars(locationBytesToRead);
                    }

                    if ((FMIsZip && i < 4 && zipBuf.Contains(MisFileStrings.MapParam)) ||
                        (!FMIsZip && i < 2 && dirBuf.Contains(MisFileStrings.MapParam)))
                    {
                        // TODO: @SS2: AngelLoader doesn't need to know if NewDark is required, but put that in eventually
                        return (null, Game.SS2);
                    }

                    if (locations[i] == ss2MapParamLoc1 || locations[i] == ss2MapParamLoc2)
                    {
                        continue;
                    }

                    // We avoid string.Concat() in favor of directly searching char arrays, as that's WAY faster
                    if ((FMIsZip ? zipBuf : dirBuf).Contains(MisFileStrings.SkyObjVar))
                    {
                        // Zip reading is going to check the NewDark locations the other way round, but
                        // fortunately they're interchangeable in meaning so we don't have to do anything
                        if (locations[i] == newDarkLoc1 || locations[i] == newDarkLoc2)
                        {
                            ret.NewDarkRequired = true;
                            foundAtNewDarkLocation = true;
                            break;
                        }
                        else if (locations[i] == oldDarkT2Loc)
                        {
                            foundAtOldDarkThief2Location = true;
                            break;
                        }
                    }
                }

                if (!foundAtNewDarkLocation) ret.NewDarkRequired = false;
            }

            #endregion

            if (foundAtOldDarkThief2Location)
            {
                return (ScanOptions.ScanNewDarkRequired ? (bool?)false : null,
                        ScanOptions.ScanGameType ? Game.Thief2 : Game.Null);
            }

            if (!ScanOptions.ScanGameType) return (ret.NewDarkRequired, Game.Null);

            #region Check for T2-unique value in .gam or .mis (determines game type for both OldDark and NewDark)

            if (FMIsZip)
            {
                // For zips, since we can't seek within the stream, the fastest way to find our string is just to
                // brute-force straight through.
                using var zipEntryStream = gamFileExists ? gamFileZipEntry.Open() : misFileZipEntry.Open();
                byte[] identString = gamFileExists
                    ? MisFileStrings.Thief2UniqueStringGam
                    : MisFileStrings.Thief2UniqueStringMis;

                // To catch matches on a boundary between chunks, leave extra space at the start of each
                // chunk for the last boundaryLen bytes of the previous chunk to go into, thus achieving a
                // kind of quick-n-dirty "step back and re-read" type thing. Dunno man, it works.
                int boundaryLen = identString.Length;
                const int bufSize = 81_920;
                byte[] chunk = new byte[boundaryLen + bufSize];

                while (zipEntryStream.Read(chunk, boundaryLen, bufSize) != 0)
                {
                    if (chunk.Contains(identString))
                    {
                        ret.Game = Game.Thief2;
                        break;
                    }

                    // Copy the last boundaryLen bytes from chunk and put them at the beginning
                    for (int si = 0, ei = bufSize; si < boundaryLen; si++, ei++) chunk[si] = chunk[ei];
                }

                if (ret.Game == Game.Null) ret.Game = Game.Thief1;
            }
            else
            {
                // For uncompressed files on disk, we mercifully can just look at the TOC and then seek to the
                // OBJ_MAP chunk and search it for the string. Phew.
                using var br = new BinaryReader(File.Open(misFileOnDisk, FileMode.Open, FileAccess.Read),
                    Encoding.ASCII, leaveOpen: false);
                uint tocOffset = br.ReadUInt32();

                br.BaseStream.Position = tocOffset;

                uint invCount = br.ReadUInt32();
                for (int i = 0; i < invCount; i++)
                {
                    char[] header = br.ReadChars(12);
                    uint offset = br.ReadUInt32();
                    uint length = br.ReadUInt32();

                    if (!header.Contains(MisFileStrings.ObjMap)) continue;

                    br.BaseStream.Position = offset;

                    byte[] content = br.ReadBytes((int)length);
                    ret.Game = content.Contains(MisFileStrings.Thief2UniqueStringMis)
                        ? Game.Thief2
                        : Game.Thief1;
                    break;
                }
            }

            #endregion

            return ret;
        }

        private static string GetNewDarkVersionFromText(string text)
        {
            string version = null;

            for (int i = 0; i < NewDarkVersionRegexes.Length; i++)
            {
                Match match = NewDarkVersionRegexes[i].Match(text);
                if (match.Success)
                {
                    version = match.Groups["Version"].Value;
                    break;
                }
            }

            if (string.IsNullOrEmpty(version)) return null;

            string ndv = version.Trim(CA_Period);
            int index = ndv.IndexOf('.');
            if (index > -1 && ndv.Substring(index + 1).Length < 2)
            {
                ndv += "0";
            }

            // Anything lower than 1.19 is OldDark; and cut it off at 2.0 to prevent that durn old time-
            // travelling Zealot's Hollow from claiming it was made with "NewDark Version 2.1"
            float ndvF = float.Parse(ndv);
            return ndvF >= 1.19 && ndvF < 2.0 ? ndv : null;
        }

        private static void DeleteFMWorkingPath(string fmWorkingPath)
        {
            try
            {
                var ds = Directory.GetDirectories(fmWorkingPath, "*", SearchOption.TopDirectoryOnly);
                for (var i = 0; i < ds.Length; i++) Directory.Delete(ds[i], true);

                Directory.Delete(fmWorkingPath, recursive: true);
            }
            catch (Exception)
            {
                // Don't care
            }
        }

        #region Generic dir/file functions

        private string[]
        EnumFiles(string searchPattern, SearchOption searchOption)
        {
            return EnumFiles("", searchPattern, searchOption, checkDirExists: false);
        }

        private string[]
        EnumFiles(string path, string searchPattern, SearchOption searchOption, bool checkDirExists = true)
        {
            string fullDir = Path.Combine(FMWorkingPath, path);

            if (!checkDirExists || Directory.Exists(fullDir))
            {
                return Directory.GetFiles(fullDir, searchPattern, searchOption);
            }

            return new string[0];
        }

        #endregion

        public void Dispose() => Archive?.Dispose();
    }
}
