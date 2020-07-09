//#define ScanSynchronous
//#define DEBUG_RANDOMIZE_DIR_SEPS
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
using FMScanner.SimpleHelpers;
using JetBrains.Annotations;
using SevenZip;
using static System.StringComparison;
using static FMScanner.Constants;
using static FMScanner.FMConstants;
using static FMScanner.Logger;
using static FMScanner.Regexes;
using static FMScanner.Utility;

namespace FMScanner
{
    [SuppressMessage("ReSharper", "ArrangeStaticMemberQualifier")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public class Scanner : IDisposable
    {
#if DEBUG
        private readonly Stopwatch _overallTimer = new Stopwatch();
#endif

        #region Public properties

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

        #endregion

        #region Private fields

#if DEBUG_RANDOMIZE_DIR_SEPS

        // Testing robustness of directory separator-agnostic code

        private readonly Random rnd = new Random();

        private string Debug_RandomizeDirSeps(string value)
        {
            for (int ri = 0; ri < value.Length; ri++)
            {
                if (value[ri].IsDirSep())
                {
                    value = value.Remove(ri, 1).Insert(ri, rnd.Next(0, 2) == 0 ? "\\" : "/");
                }
            }

            return value;
        }

#endif

        #region Disposable

        private ZipArchiveFast _archive;

        #endregion

        private readonly FileEncoding _fileEncoding = new FileEncoding();

        private readonly List<FileInfo> _fmDirFileInfos = new List<FileInfo>();

        private ScanOptions _scanOptions = new ScanOptions();

        private bool _fmIsZip;

        private string _archivePath;

        private string _fmWorkingPath;

        // Guess I'll leave this one global for reasons
        private readonly List<ReadmeInternal> _readmeFiles = new List<ReadmeInternal>();

        private readonly TitlesStrNaturalNumericSort _titlesStrNaturalNumericSort = new TitlesStrNaturalNumericSort();

        private bool _ss2Fingerprinted;

        #endregion

        #region Private classes

        private sealed class ReadmeInternal
        {
            /// <summary>
            /// Check this bool to see if you want to scan the file or not. Currently false if readme is HTML or
            /// non-English.
            /// </summary>
            internal bool Scan;
            internal string FileName;
            internal List<string> Lines;
            internal string Text;
            internal DateTime LastModifiedDate;
        }

        /// <summary>
        /// Stores a filename/index pair for quick lookups into a zip file.
        /// </summary>
        private sealed class NameAndIndex
        {
            internal string Name;
            internal int Index = -1;
        }

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
                tempPath, _scanOptions, null, CancellationToken.None)[0];
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
            return await Task.Run(() => ScanMany(missions, tempPath, _scanOptions, null, CancellationToken.None));
        }

        [PublicAPI]
        public async Task<List<ScannedFMData>>
        ScanAsync(List<FMToScan> missions, string tempPath, ScanOptions scanOptions)
        {
            return await Task.Run(() => ScanMany(missions, tempPath, scanOptions, null, CancellationToken.None));
        }

        [PublicAPI]
        public async Task<List<ScannedFMData>>
        ScanAsync(List<FMToScan> missions, string tempPath, IProgress<ProgressReport> progress,
                  CancellationToken cancellationToken)
        {
            return await Task.Run(() => ScanMany(missions, tempPath, _scanOptions, progress, cancellationToken));
        }

        [PublicAPI]
        public async Task<List<ScannedFMData>>
        ScanAsync(List<FMToScan> missions, string tempPath, ScanOptions scanOptions,
                  IProgress<ProgressReport> progress, CancellationToken cancellationToken)
        {
            return await Task.Run(() => ScanMany(missions, tempPath, scanOptions, progress, cancellationToken));
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

            if (tempPath.IsEmpty())
            {
                Log(LogFile, nameof(ScanMany) + ": Argument is null or empty: " + nameof(tempPath), methodName: false);
                throw new ArgumentException("Argument is null or empty.", nameof(tempPath));
            }

            if (missions == null) throw new ArgumentNullException(nameof(missions));
            if (missions.Count == 0 || (missions.Count == 1 && missions[0].Path.IsEmpty()))
            {
                Log(LogFile, nameof(ScanMany) + ": No mission(s) specified. tempPath: " + tempPath, methodName: false);
                throw new ArgumentException("No mission(s) specified.", nameof(missions));
            }

            _scanOptions = scanOptions ?? throw new ArgumentNullException(nameof(scanOptions));

            #endregion

            var scannedFMDataList = new List<ScannedFMData>();

            // Init and dispose rtfBox here to avoid cross-thread exceptions.
            // For performance, we only have one instance and we just change its content as needed.
            using var rtfBox = new RichTextBox();

            ProgressReport progressReport = new ProgressReport();

            for (int i = 0; i < missions.Count; i++)
            {
                _readmeFiles.Clear();
                _fmDirFileInfos.Clear();
                _ss2Fingerprinted = false;

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
                    string fm = missions[i].Path;
                    _fmIsZip = fm.ExtIsZip() || fm.ExtIs7z();

                    _archive?.Dispose();

                    if (_fmIsZip)
                    {
                        _archivePath = fm;
                        try
                        {
                            _fmWorkingPath = Path.Combine(tempPath, Path.GetFileNameWithoutExtension(_archivePath).Trim());
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
                        _fmWorkingPath = fm;
                    }
                }

                #endregion

                #region Report progress and handle cancellation

                cancellationToken.ThrowIfCancellationRequested();

                if (progress != null)
                {
                    // Recycle one object to minimize GC
                    progressReport.FMName = missions[i].Path;
                    progressReport.FMNumber = i + 1;
                    progressReport.FMsTotal = missions.Count;
                    progressReport.Percent = GetPercentFromValue(i + 1, missions.Count);
                    progressReport.Finished = false;

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
                            _tempScanOptions = _scanOptions.DeepCopy();
                            _scanOptions = FullScanOptions.DeepCopy();
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
                            _scanOptions = _tempScanOptions!.DeepCopy();
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

            return scannedFMDataList;
        }

        private ScannedFMData ScanCurrentFM(RichTextBox rtfBox)
        {
#if DEBUG
            _overallTimer.Restart();
#endif

            // Sometimes we'll want to remove this from the start of a string to get a relative path, so it's
            // critical that we always know we have a dir separator on the end so we don't end up with a leading
            // one on the string when we remove this from the start of it

            if (!_fmWorkingPath.EndsWithDirSep()) _fmWorkingPath += "\\";

#if DEBUG_RANDOMIZE_DIR_SEPS

            _fmWorkingPath = Debug_RandomizeDirSeps(_fmWorkingPath);

#endif

            static ScannedFMData UnsupportedZip(string archivePath) => new ScannedFMData
            {
                ArchiveName = Path.GetFileName(archivePath),
                Game = Game.Unsupported
            };

            static ScannedFMData UnsupportedDir() => null;

            long? sevenZipSize = null;

            #region Setup

            #region Check for and setup 7-Zip

            bool fmIsSevenZip = false;
            if (_fmIsZip && _archivePath.ExtIs7z())
            {
                _fmIsZip = false;
                fmIsSevenZip = true;

                try
                {
                    using var sze = new SevenZipExtractor(_archivePath) { PreserveDirectoryStructure = true };
                    sevenZipSize = sze.PackedSize;
                    sze.ExtractArchive(_fmWorkingPath);
                }
                catch (Exception)
                {
                    // Third party thing, doesn't tell you what exceptions it can throw, whatever
                    DeleteFMWorkingPath(_fmWorkingPath);
                    return UnsupportedZip(_archivePath);
                }
            }

            #endregion

            #region Check for and setup Zip

            if (_fmIsZip)
            {
                Debug.WriteLine(@"----------" + _archivePath);

                if (_archivePath.ExtIsZip())
                {
                    try
                    {
                        _archive = new ZipArchiveFast(new FileStream(_archivePath, FileMode.Open, FileAccess.Read),
                            leaveOpen: false, ZipEntryNameEncoding);

                        // Archive.Entries is lazy-loaded, so this will also trigger any exceptions that may be
                        // thrown while loading them. If this passes, we're definitely good.
                        if (_archive.Entries.Count == 0) return UnsupportedZip(_archivePath);
                    }
                    catch (Exception)
                    {
                        // Invalid zip file, whatever, move on
                        return UnsupportedZip(_archivePath);
                    }
                }
                else
                {
                    return UnsupportedZip(_archivePath);
                }
            }
            else
            {
                if (!Directory.Exists(_fmWorkingPath)) return UnsupportedDir();
                Debug.WriteLine(@"----------" + _fmWorkingPath);
            }

            #endregion

            #endregion

            var fmData = new ScannedFMData
            {
                ArchiveName = _fmIsZip || fmIsSevenZip
                    ? Path.GetFileName(_archivePath)
                    // PERF_TODO: Use fast string-only name getter
                    // Case against: Being built-in, this way is safe and complete. ArchiveName is monumentally
                    // important, we don't want to mess around.
                    : new DirectoryInfo(_fmWorkingPath).Name
            };

            #region Size

            // Getting the size is horrendously expensive for folders, but if we're doing it then we can save
            // some time later by using the FileInfo list as a cache.
            if (_scanOptions.ScanSize)
            {
                if (_fmIsZip)
                {
                    fmData.Size = _archive.ArchiveStream.Length;
                }
                else
                {
                    _fmDirFileInfos.Clear();
                    // PERF: AddRange() is a fat slug, do Add() in a loop instead
                    FileInfo[] fileInfos = new DirectoryInfo(_fmWorkingPath).GetFiles("*", SearchOption.AllDirectories);
                    // Don't reduce capacity, as that causes a reallocation
                    if (_fmDirFileInfos.Capacity < fileInfos.Length) _fmDirFileInfos.Capacity = fileInfos.Length;
                    for (int i = 0; i < fileInfos.Length; i++) _fmDirFileInfos.Add(fileInfos[i]);

                    if (fmIsSevenZip)
                    {
                        fmData.Size = sevenZipSize;
                    }
                    else
                    {
                        long size = 0;
                        foreach (FileInfo fi in _fmDirFileInfos) size += fi.Length;
                        fmData.Size = size;
                    }
                }
            }

            #endregion

            // PERF_TODO: Recycle these
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
                if (fmIsSevenZip) DeleteFMWorkingPath(_fmWorkingPath);
                return _fmIsZip || fmIsSevenZip ? UnsupportedZip(_archivePath) : UnsupportedDir();
            }

            #endregion

            bool fmIsT3 = fmData.Game == Game.Thief3;

            var altTitles = new List<string>();

            void SetOrAddTitle(string value)
            {
                value = CleanupTitle(value);

                if (value.IsEmpty()) return;

                if (fmData.Title.IsEmpty())
                {
                    fmData.Title = value;
                }
                else if (!fmData.Title.EqualsI(value) && !altTitles.ContainsI(value))
                {
                    altTitles.Add(value);
                }
            }

            bool fmIsSS2 = false;

            if (!fmIsT3)
            {
                // We check game type as early as possible because we used to want to reject SS2 FMs early for
                // speed. We support SS2 now, but this works fine and dandy where it is so not messing with it.

                #region NewDark/GameType checks

                if (_scanOptions.ScanNewDarkRequired || _scanOptions.ScanGameType)
                {
                    var (newDarkRequired, game) = GetGameTypeAndEngine(baseDirFiles, usedMisFiles);
                    if (_scanOptions.ScanNewDarkRequired) fmData.NewDarkRequired = newDarkRequired;
                    if (_scanOptions.ScanGameType)
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

                if (_scanOptions.ScanTitle || _scanOptions.ScanAuthor || _scanOptions.ScanVersion ||
                    _scanOptions.ScanReleaseDate || _scanOptions.ScanTags)
                {
                    for (int i = 0; i < baseDirFiles.Count; i++)
                    {
                        NameAndIndex f = baseDirFiles[i];
                        if (f.Name.EqualsI(FMFiles.FMInfoXml))
                        {
                            var (title, author, version, releaseDate) = ReadFMInfoXml(f);
                            if (_scanOptions.ScanTitle) SetOrAddTitle(title);
                            if (_scanOptions.ScanAuthor) fmData.Author = author;
                            if (_scanOptions.ScanVersion) fmData.Version = version;
                            if (_scanOptions.ScanReleaseDate && releaseDate != null) fmData.LastUpdateDate = releaseDate;
                            break;
                        }
                    }
                }
                // I think we need to always scan fm.ini even if we're not returning any of its fields, because
                // of tags, I think for some reason we're needing to read tags always?
                {
                    for (int i = 0; i < baseDirFiles.Count; i++)
                    {
                        NameAndIndex f = baseDirFiles[i];
                        if (f.Name.EqualsI(FMFiles.FMIni))
                        {
                            var (title, author, description, lastUpdateDate, tags) = ReadFMIni(f);
                            if (_scanOptions.ScanTitle) SetOrAddTitle(title);
                            if (_scanOptions.ScanAuthor && !author.IsEmpty()) fmData.Author = author;
                            if (_scanOptions.ScanDescription) fmData.Description = description;
                            if (_scanOptions.ScanReleaseDate && lastUpdateDate != null) fmData.LastUpdateDate = lastUpdateDate;
                            if (_scanOptions.ScanTags) fmData.TagsString = tags;
                            break;
                        }
                    }
                }
                if (_scanOptions.ScanTitle || _scanOptions.ScanAuthor)
                {
                    // SS2 file
                    // TODO: If we wanted to be sticklers, we could skip this for non-SS2 FMs
                    for (int i = 0; i < baseDirFiles.Count; i++)
                    {
                        NameAndIndex f = baseDirFiles[i];
                        if (f.Name.EqualsI(FMFiles.ModIni))
                        {
                            var (title, author) = ReadModIni(f);
                            if (_scanOptions.ScanTitle) SetOrAddTitle(title);
                            if (_scanOptions.ScanAuthor && !author.IsEmpty()) fmData.Author = author;
                            break;
                        }
                    }
                }

                #endregion
            }

            #region Read, cache, and set readme files

            // PERF_TODO: Recycle this
            var readmeDirFiles = new List<NameAndIndex>();

            foreach (NameAndIndex f in baseDirFiles) readmeDirFiles.Add(f);

            if (fmIsT3) foreach (NameAndIndex f in t3FMExtrasDirFiles) readmeDirFiles.Add(f);

            ReadAndCacheReadmeFiles(readmeDirFiles, rtfBox);

            #endregion

            if (!fmIsT3)
            {
                // This is here because it needs to come after the readmes are cached
                #region NewDark minimum required version

                if (fmData.NewDarkRequired == true && _scanOptions.ScanNewDarkMinimumVersion)
                {
                    fmData.NewDarkMinRequiredVersion = GetValueFromReadme(SpecialLogic.NewDarkMinimumVersion);
                }

                #endregion
            }

            #region Set release date

            if (_scanOptions.ScanReleaseDate && fmData.LastUpdateDate == null)
            {
                fmData.LastUpdateDate = GetReleaseDate(fmIsT3, usedMisFiles);
            }

            #endregion

            #region Title and IncludedMissions

            // SS2 doesn't have a missions list or a titles list file
            if (!fmIsT3 && !fmIsSS2)
            {
                if (_scanOptions.ScanTitle || _scanOptions.ScanCampaignMissionNames)
                {
                    var (titleFrom0, titleFromN, cNames) = GetMissionNames(stringsDirFiles, misFiles, usedMisFiles);
                    if (_scanOptions.ScanTitle)
                    {
                        SetOrAddTitle(titleFrom0);
                        SetOrAddTitle(titleFromN);
                    }

                    if (_scanOptions.ScanCampaignMissionNames && cNames != null && cNames.Count > 0)
                    {
                        for (int i = 0; i < cNames.Count; i++) cNames[i] = CleanupTitle(cNames[i]);
                        fmData.IncludedMissions = cNames.ToArray();
                    }
                }
            }

            if (_scanOptions.ScanTitle)
            {
                SetOrAddTitle(GetValueFromReadme(SpecialLogic.Title, titles: null, SA_TitleDetect));

                if (!fmIsT3) SetOrAddTitle(GetTitleFromNewGameStrFile(intrfaceDirFiles));

                var topOfReadmeTitles = GetTitlesFromTopOfReadmes(_readmeFiles);
                if (topOfReadmeTitles != null && topOfReadmeTitles.Count > 0)
                {
                    foreach (string title in topOfReadmeTitles) SetOrAddTitle(title);
                }

                fmData.AlternateTitles = altTitles.ToArray();
            }

            #endregion

            #region Author

            if (_scanOptions.ScanAuthor || _scanOptions.ScanTags)
            {
                if (fmData.Author.IsEmpty())
                {
                    var titles = !fmData.Title.IsEmpty() ? new List<string> { fmData.Title } : null;
                    if (titles != null && altTitles.Count > 0)
                    {
                        titles.AddRange(altTitles);
                    }

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

            if (_scanOptions.ScanVersion && fmData.Version.IsEmpty()) fmData.Version = GetVersion();

            #endregion

            // Again, I don't know enough about Thief 3 to know how to detect its languages
            if (!fmIsT3)
            {
                #region Languages

                if (_scanOptions.ScanLanguages || _scanOptions.ScanTags)
                {
                    var getLangs = GetLanguages(baseDirFiles, booksDirFiles, intrfaceDirFiles, stringsDirFiles);
                    fmData.Languages = getLangs.Langs.ToArray();
                    if (getLangs.Langs.Count > 0) SetLangTags(fmData, getLangs.EnglishIsUncertain);
                    if (!_scanOptions.ScanLanguages) fmData.Languages = null;
                }

                #endregion
            }

            if (_scanOptions.ScanTags)
            {
                if (fmData.Type == FMType.Campaign) SetMiscTag(fmData, "campaign");

                if (!fmData.Author.IsEmpty())
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

                if (!_scanOptions.ScanAuthor) fmData.Author = null;
            }

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

            if (fmIsSevenZip) DeleteFMWorkingPath(_fmWorkingPath);

#if DEBUG
            _overallTimer.Stop();
            Debug.WriteLine(@"This FM took:\r\n" + _overallTimer.Elapsed.ToString(@"hh\:mm\:ss\.fffffff"));
#endif

            return fmData;
        }

        private DateTime? GetReleaseDate(bool fmIsT3, List<NameAndIndex> usedMisFiles)
        {
            DateTime? ret = null;

            // Look in the readme
            string ds = GetValueFromReadme(SpecialLogic.None, null, SA_ReleaseDateDetect);
            if (!ds.IsEmpty() && StringToDate(ds, out DateTime dt))
            {
                ret = dt;
            }

            // Look for the first readme file's last modified date
            if (ret == null && _readmeFiles.Count > 0 && _readmeFiles[0].LastModifiedDate.Year > 1998)
            {
                ret = _readmeFiles[0].LastModifiedDate;
            }

            // No first used mission file for T3: no idea how to find such a thing
            if (fmIsT3) return ret;

            // Look for the first used .mis file's last modified date
            if (ret == null)
            {
                DateTime misFileDate;
                if (_fmIsZip)
                {
                    misFileDate = new DateTimeOffset(ZipHelpers.ZipTimeToDateTime(
                        _archive.Entries[usedMisFiles[0].Index].LastWriteTime)).DateTime;
                }
                else
                {
                    if (_scanOptions.ScanSize && _fmDirFileInfos.Count > 0)
                    {
                        string fn = _fmWorkingPath + usedMisFiles[0].Name;
                        // This loop is guaranteed to find something, because we will have quit early if we had
                        // no used .mis files.
                        FileInfo misFile = null;
                        for (int i = 0; i < _fmDirFileInfos.Count; i++)
                        {
                            FileInfo f = _fmDirFileInfos[i];
                            if (f.FullName.PathEqualsI(fn))
                            {
                                misFile = f;
                                break;
                            }
                        }
                        misFileDate = new DateTimeOffset(misFile!.LastWriteTime).DateTime;
                    }
                    else
                    {
                        var fi = new FileInfo(Path.Combine(_fmWorkingPath, usedMisFiles[0].Name));
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

        #region Set tags

        private static void SetLangTags(ScannedFMData fmData, bool englishIsUncertain)
        {
            if (fmData.TagsString.IsWhiteSpace()) fmData.TagsString = "";
            for (int i = 0; i < fmData.Languages.Length; i++)
            {
                string lang = fmData.Languages[i];

                // NOTE: This all depends on langs being lowercase!
                Debug.Assert(lang == lang.ToLowerInvariant(),
                            "lang != lang.ToLowerInvariant() - lang is not lowercase");

                if (englishIsUncertain && lang.EqualsI("english")) continue;

                if (fmData.TagsString.Contains(lang))
                {
                    fmData.TagsString = Regex.Replace(fmData.TagsString, @":\s*" + lang, ":" + LanguagesC[lang]);
                }

                // PERF: 5ms over the whole 1098 set, whatever
                Match match = Regex.Match(fmData.TagsString, @"language:\s*" + lang, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
                if (match.Success) continue;

                if (fmData.TagsString != "") fmData.TagsString += ", ";
                fmData.TagsString += "language:" + LanguagesC[lang];
            }

            // Compatibility with old behavior for painless diffing
            if (fmData.TagsString.IsEmpty()) fmData.TagsString = null;
        }

        private static void SetMiscTag(ScannedFMData fmData, string tag)
        {
            if (fmData.TagsString.IsWhiteSpace()) fmData.TagsString = "";

            var list = fmData.TagsString.Split(CA_CommaSemicolon).ToList();
            bool tagFound = false;
            for (int i = 0; i < list.Count; i++)
            {
                list[i] = list[i].Trim();
                if (list[i].IsEmpty())
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

        #endregion

        private bool ReadAndCacheFMData(ScannedFMData fmd, List<NameAndIndex> baseDirFiles,
                                        List<NameAndIndex> misFiles, List<NameAndIndex> usedMisFiles,
                                        List<NameAndIndex> stringsDirFiles, List<NameAndIndex> intrfaceDirFiles,
                                        List<NameAndIndex> booksDirFiles, List<NameAndIndex> t3FMExtrasDirFiles)
        {
            #region Add BaseDirFiles

            bool t3Found = false;

            // This is split out because of weird semantics with if(this && that) vs nested ifs (required in
            // order to have a var in the middle to avoid multiple LastIndexOf calls).
            static bool MapFileExists(string path)
            {
                if (path.PathStartsWithI(FMDirs.IntrfaceS) && path.DirSepCountIsAtLeast(1, FMDirs.IntrfaceSLen))
                {
                    int lsi = path.LastIndexOfDirSep();
                    if (path.Length > lsi + 5 &&
                        (path[lsi + 1] == 'p' || path[lsi + 1] == 'P') &&
                        (path[lsi + 2] == 'a' || path[lsi + 2] == 'A') &&
                        (path[lsi + 3] == 'g' || path[lsi + 3] == 'G') &&
                        (path[lsi + 4] == 'e' || path[lsi + 4] == 'E') &&
                        (path[lsi + 5] == '0') &&
                        path.LastIndexOf('.') > lsi)
                    {
                        return true;
                    }
                }

                return false;
            }

            if (_fmIsZip || _scanOptions.ScanSize)
            {
                int filesCount = _fmIsZip ? _archive.Entries.Count : _fmDirFileInfos.Count;
                for (int i = 0; i < filesCount; i++)
                {
                    string fn = _fmIsZip
                        ? _archive.Entries[i].FullName
                        : _fmDirFileInfos[i].FullName.Substring(_fmWorkingPath.Length);

#if DEBUG_RANDOMIZE_DIR_SEPS

                    fn = Debug_RandomizeDirSeps(fn);

#endif

                    int index = _fmIsZip ? i : -1;

                    if (!t3Found &&
                        fn.PathStartsWithI(FMDirs.T3DetectS) &&
                        fn.CountDirSeps(FMDirs.T3DetectSLen) == 0 &&
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
                    else if (fn.PathStartsWithI(FMDirs.T3FMExtras1S) ||
                             fn.PathStartsWithI(FMDirs.T3FMExtras2S))
                    {
                        t3FMExtrasDirFiles.Add(new NameAndIndex { Name = fn, Index = index });
                        continue;
                    }
                    else if (!fn.ContainsDirSep() && fn.Contains('.'))
                    {
                        baseDirFiles.Add(new NameAndIndex { Name = fn, Index = index });
                        // Fallthrough so ScanCustomResources can use it
                    }
                    else if (!t3Found && fn.PathStartsWithI(FMDirs.StringsS))
                    {
                        stringsDirFiles.Add(new NameAndIndex { Name = fn, Index = index });
                        if (!_ss2Fingerprinted &&
                            (fn.PathEndsWithI(FMFiles.SS2Fingerprint1) ||
                            fn.PathEndsWithI(FMFiles.SS2Fingerprint2) ||
                            fn.PathEndsWithI(FMFiles.SS2Fingerprint3) ||
                            fn.PathEndsWithI(FMFiles.SS2Fingerprint4)))
                        {
                            _ss2Fingerprinted = true;
                        }
                        continue;
                    }
                    else if (!t3Found && fn.PathStartsWithI(FMDirs.IntrfaceS))
                    {
                        intrfaceDirFiles.Add(new NameAndIndex { Name = fn, Index = index });
                        // Fallthrough so ScanCustomResources can use it
                    }
                    else if (!t3Found && fn.PathStartsWithI(FMDirs.BooksS))
                    {
                        booksDirFiles.Add(new NameAndIndex { Name = fn, Index = index });
                        continue;
                    }
                    else if (!t3Found && !_ss2Fingerprinted &&
                             (fn.PathStartsWithI(FMDirs.CutscenesS) ||
                              fn.PathStartsWithI(FMDirs.Snd2S)))
                    {
                        _ss2Fingerprinted = true;
                        // Fallthrough so ScanCustomResources can use it
                    }

                    // Inlined for performance. We cut the time roughly in half by doing this.
                    if (!t3Found && _scanOptions.ScanCustomResources)
                    {
                        if (fmd.HasAutomap == null &&
                            fn.PathStartsWithI(FMDirs.IntrfaceS) &&
                            fn.DirSepCountIsAtLeast(1, FMDirs.IntrfaceSLen) &&
                            fn.EndsWithRaDotBin())
                        {
                            fmd.HasAutomap = true;
                            // Definitely a clever deduction, definitely not a sneaky hack for GatB-T2
                            fmd.HasMap = true;
                        }
                        else if (fmd.HasMap == null && MapFileExists(fn))
                        {
                            fmd.HasMap = true;
                        }
                        else if (fmd.HasCustomMotions == null &&
                                 fn.PathStartsWithI(FMDirs.MotionsS) &&
                                 MotionFileExtensions.Any(fn.EndsWithI))
                        {
                            fmd.HasCustomMotions = true;
                        }
                        else if (fmd.HasMovies == null &&
                                 (fn.PathStartsWithI(FMDirs.MoviesS) || fn.PathStartsWithI(FMDirs.CutscenesS)) &&
                                 fn.HasFileExtension())
                        {
                            fmd.HasMovies = true;
                        }
                        else if (fmd.HasCustomTextures == null &&
                                 fn.PathStartsWithI(FMDirs.FamS) &&
                                 ImageFileExtensions.Any(fn.EndsWithI))
                        {
                            fmd.HasCustomTextures = true;
                        }
                        else if (fmd.HasCustomObjects == null &&
                                 fn.PathStartsWithI(FMDirs.ObjS) &&
                                 fn.ExtIsBin())
                        {
                            fmd.HasCustomObjects = true;
                        }
                        else if (fmd.HasCustomCreatures == null &&
                                 fn.PathStartsWithI(FMDirs.MeshS) &&
                                 fn.ExtIsBin())
                        {
                            fmd.HasCustomCreatures = true;
                        }
                        else if (fmd.HasCustomScripts == null &&
                                 (!fn.ContainsDirSep() &&
                                  ScriptFileExtensions.Any(fn.EndsWithI)) ||
                                 (fn.PathStartsWithI(FMDirs.ScriptsS) &&
                                  fn.HasFileExtension()))
                        {
                            fmd.HasCustomScripts = true;
                        }
                        else if (fmd.HasCustomSounds == null &&
                                 (fn.PathStartsWithI(FMDirs.SndS) || fn.PathStartsWithI(FMDirs.Snd2S)) &&
                                 fn.HasFileExtension())
                        {
                            fmd.HasCustomSounds = true;
                        }
                        else if (fmd.HasCustomSubtitles == null &&
                                 fn.PathStartsWithI(FMDirs.SubtitlesS) &&
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

                    if (_scanOptions.ScanCustomResources)
                    {
                        fmd.HasMap ??= false;
                        fmd.HasAutomap ??= false;
                        fmd.HasCustomMotions ??= false;
                        fmd.HasMovies ??= false;
                        fmd.HasCustomTextures ??= false;
                        fmd.HasCustomObjects ??= false;
                        fmd.HasCustomCreatures ??= false;
                        fmd.HasCustomScripts ??= false;
                        fmd.HasCustomSounds ??= false;
                        fmd.HasCustomSubtitles ??= false;
                    }
                }
            }
            else
            {
                string t3DetectPath = Path.Combine(_fmWorkingPath, FMDirs.T3DetectS);
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
                        t3FMExtrasDirFiles.Add(new NameAndIndex { Name = f.Substring(_fmWorkingPath.Length) });
                    }

                    foreach (string f in EnumFiles(FMDirs.T3FMExtras2, "*", SearchOption.TopDirectoryOnly))
                    {
                        t3FMExtrasDirFiles.Add(new NameAndIndex { Name = f.Substring(_fmWorkingPath.Length) });
                    }
                }
                else
                {
                    if (baseDirFiles.Count == 0) return false;

                    foreach (string f in EnumFiles(FMDirs.Strings, "*", SearchOption.AllDirectories))
                    {
                        stringsDirFiles.Add(new NameAndIndex { Name = f.Substring(_fmWorkingPath.Length) });
                        if (!_ss2Fingerprinted &&
                            (f.PathEndsWithI(FMFiles.SS2Fingerprint1) ||
                            f.PathEndsWithI(FMFiles.SS2Fingerprint2) ||
                            f.PathEndsWithI(FMFiles.SS2Fingerprint3) ||
                            f.PathEndsWithI(FMFiles.SS2Fingerprint4)))
                        {
                            _ss2Fingerprinted = true;
                        }
                    }

                    foreach (string f in EnumFiles(FMDirs.Intrface, "*", SearchOption.AllDirectories))
                    {
                        intrfaceDirFiles.Add(new NameAndIndex { Name = f.Substring(_fmWorkingPath.Length) });
                    }

                    foreach (string f in EnumFiles(FMDirs.Books, "*", SearchOption.AllDirectories))
                    {
                        booksDirFiles.Add(new NameAndIndex { Name = f.Substring(_fmWorkingPath.Length) });
                    }

                    // TODO: Maybe extract this again, but then I have to extract MapFileExists() too
                    if (!_ss2Fingerprinted || _scanOptions.ScanCustomResources)
                    {
                        // PERF_TODO: I'm getting all folders but only need to check if specific ones exist.
                        // Does FastIO code Just Work and return false if the path itself doesn't exist?
                        var baseDirFolders = new List<string>();
                        foreach (string f in Directory.GetDirectories(_fmWorkingPath, "*", SearchOption.TopDirectoryOnly))
                        {
                            baseDirFolders.Add(f.Substring(f.LastIndexOfDirSep() + 1));
                        }

                        if (_scanOptions.ScanCustomResources)
                        {
                            foreach (NameAndIndex f in intrfaceDirFiles)
                            {
                                if (fmd.HasAutomap == null &&
                                    f.Name.PathStartsWithI(FMDirs.IntrfaceS) &&
                                    f.Name.DirSepCountIsAtLeast(1, FMDirs.IntrfaceSLen) &&
                                    f.Name.EndsWithRaDotBin())
                                {
                                    fmd.HasAutomap = true;
                                    // Definitely a clever deduction, definitely not a sneaky hack for GatB-T2
                                    fmd.HasMap = true;
                                    break;
                                }

                                if (fmd.HasMap == null && MapFileExists(f.Name)) fmd.HasMap = true;
                            }

                            fmd.HasMap ??= false;
                            fmd.HasAutomap ??= false;

                            fmd.HasCustomMotions =
                                baseDirFolders.ContainsI(FMDirs.Motions) &&
                                FastIO.FilesExistSearchAll(Path.Combine(_fmWorkingPath, FMDirs.Motions), MotionFilePatterns);

                            fmd.HasMovies =
                                (baseDirFolders.ContainsI(FMDirs.Movies) &&
                                 FastIO.FilesExistSearchAll(Path.Combine(_fmWorkingPath, FMDirs.Movies), SA_AllFiles)) ||
                                (baseDirFolders.ContainsI(FMDirs.Cutscenes) &&
                                 FastIO.FilesExistSearchAll(Path.Combine(_fmWorkingPath, FMDirs.Cutscenes), SA_AllFiles));

                            fmd.HasCustomTextures =
                                baseDirFolders.ContainsI(FMDirs.Fam) &&
                                FastIO.FilesExistSearchAll(Path.Combine(_fmWorkingPath, FMDirs.Fam), ImageFilePatterns);

                            fmd.HasCustomObjects =
                                baseDirFolders.ContainsI(FMDirs.Obj) &&
                                FastIO.FilesExistSearchAll(Path.Combine(_fmWorkingPath, FMDirs.Obj), SA_AllBinFiles);

                            fmd.HasCustomCreatures =
                                baseDirFolders.ContainsI(FMDirs.Mesh) &&
                                FastIO.FilesExistSearchAll(Path.Combine(_fmWorkingPath, FMDirs.Mesh), SA_AllBinFiles);

                            fmd.HasCustomScripts =
                                baseDirFiles.Any(x => ScriptFileExtensions.ContainsI(Path.GetExtension(x.Name))) ||
                                (baseDirFolders.ContainsI(FMDirs.Scripts) &&
                                 FastIO.FilesExistSearchAll(Path.Combine(_fmWorkingPath, FMDirs.Scripts), SA_AllFiles));

                            fmd.HasCustomSounds =
                                (baseDirFolders.ContainsI(FMDirs.Snd) &&
                                 FastIO.FilesExistSearchAll(Path.Combine(_fmWorkingPath, FMDirs.Snd), SA_AllFiles)) ||
                                 (baseDirFolders.ContainsI(FMDirs.Snd2) &&
                                  FastIO.FilesExistSearchAll(Path.Combine(_fmWorkingPath, FMDirs.Snd2), SA_AllFiles));

                            fmd.HasCustomSubtitles =
                                baseDirFolders.ContainsI(FMDirs.Subtitles) &&
                                FastIO.FilesExistSearchAll(Path.Combine(_fmWorkingPath, FMDirs.Subtitles), SA_AllSubFiles);
                        }

                        if (!_ss2Fingerprinted &&
                            (baseDirFolders.ContainsI(FMDirs.Cutscenes) ||
                            baseDirFolders.ContainsI(FMDirs.Snd2)))
                        {
                            _ss2Fingerprinted = true;
                        }
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
                        x.Name.PathEqualsI(FMFiles.StringsMissFlag))
                    ?? stringsDirFiles.FirstOrDefault(x =>
                        x.Name.PathEqualsI(FMFiles.StringsEnglishMissFlag))
                    ?? stringsDirFiles.FirstOrDefault(x =>
                        x.Name.PathEndsWithI(FMFiles.SMissFlag));
            }

            if (missFlag != null)
            {
                List<string> mfLines;

                // missflag.str files are always ASCII / UTF8, so we can avoid an expensive encoding detect here
                if (_fmIsZip)
                {
                    var e = _archive.Entries[missFlag.Index];
                    using var es = e.Open();
                    mfLines = ReadAllLines(es, Encoding.UTF8);
                }
                else
                {
                    mfLines = ReadAllLines(Path.Combine(_fmWorkingPath, missFlag.Name), Encoding.UTF8);
                }

                for (int mfI = 0; mfI < misFiles.Count; mfI++)
                {
                    NameAndIndex mf = misFiles[mfI];

                    // Obtuse nonsense to avoid string allocations (perf)
                    if (mf.Name.StartsWithI("miss") && mf.Name[4] != '.')
                    {
                        // Since only files ending in .mis are in the misFiles list, we're guaranteed to find a .
                        // character and not get a -1 index. And since we know our file starts with "miss", the
                        // -4 is guaranteed not to take us negative either.
                        int count = mf.Name.IndexOf('.') - 4;
                        for (int mflI = 0; mflI < mfLines.Count; mflI++)
                        {
                            string line = mfLines[mflI];
                            if (line.StartsWithI("miss_") && line.Length > 5 + count && line[5 + count] == ':')
                            {
                                bool numsMatch = true;
                                for (int li = 4; li < 4 + count; li++)
                                {
                                    if (line[li + 1] != mf.Name[li])
                                    {
                                        numsMatch = false;
                                        break;
                                    }
                                }
                                int qIndex;
                                if (numsMatch && (qIndex = line.IndexOf('\"')) > -1)
                                {
                                    if (!(line.Length > qIndex + 5 &&
                                          // I don't think any files actually have "skip" in anything other than
                                          // lowercase, but I'm supporting any case anyway. You never know.
                                          (line[qIndex + 1] == 's' || line[qIndex + 1] == 'S') &&
                                          (line[qIndex + 2] == 'k' || line[qIndex + 2] == 'K') &&
                                          (line[qIndex + 3] == 'i' || line[qIndex + 3] == 'I') &&
                                          (line[qIndex + 4] == 'p' || line[qIndex + 4] == 'P') &&
                                          line[qIndex + 5] == '\"'))
                                    {
                                        usedMisFiles.Add(mf);
                                    }
                                }
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

        #region Read FM info files

        private (string Title, string Author, string Version, DateTime? ReleaseDate)
        ReadFMInfoXml(NameAndIndex file)
        {
            string title = null;
            string author = null;
            string version = null;
            DateTime? releaseDate = null;

            var fmInfoXml = new XmlDocument();

            #region Load XML

            if (_fmIsZip)
            {
                var e = _archive.Entries[file.Index];
                using var es = e.Open();
                fmInfoXml.Load(es);
            }
            else
            {
                fmInfoXml.Load(Path.Combine(_fmWorkingPath, file.Name));
            }

            #endregion

            if (_scanOptions.ScanTitle)
            {
                var xTitle = fmInfoXml.GetElementsByTagName("title");
                if (xTitle.Count > 0) title = xTitle[0].InnerText;
            }

            if (_scanOptions.ScanAuthor)
            {
                var xAuthor = fmInfoXml.GetElementsByTagName("author");
                if (xAuthor.Count > 0) author = xAuthor[0].InnerText;
            }

            if (_scanOptions.ScanVersion)
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
            var ret = (
                Title: (string)null,
                Author: (string)null,
                Description: (string)null,
                LastUpdateDate: (DateTime?)null,
                Tags: (string)null
            );

            #region Load INI

            List<string> iniLines;

            if (_fmIsZip)
            {
                var e = _archive.Entries[file.Index];
                using var es = e.Open();
                iniLines = ReadAllLinesE(es, e.Length);
            }
            else
            {
                iniLines = ReadAllLinesE(Path.Combine(_fmWorkingPath, file.Name));
            }

            if (iniLines == null || iniLines.Count == 0) return (null, null, null, null, null);

            (string NiceName, string ReleaseDate, string Tags, string Descr) fmIni = (null, null, null, null);

            #region Deserialize ini

            bool inDescr = false;

            // Quick n dirty, works fine and is fast
            foreach (string line in iniLines)
            {
                if (line.StartsWithI("NiceName="))
                {
                    inDescr = false;
                    fmIni.NiceName = line.Substring(9).Trim();
                }
                else if (line.StartsWithI("ReleaseDate="))
                {
                    inDescr = false;
                    fmIni.ReleaseDate = line.Substring(12).Trim();
                }
                else if (line.StartsWithI("Tags="))
                {
                    inDescr = false;
                    fmIni.Tags = line.Substring(5).Trim();
                }
                // Sometimes Descr values are literally multi-line. DON'T. DO. THAT. Use \n.
                // But I have to deal with it anyway.
                else if (line.StartsWithI("Descr="))
                {
                    inDescr = true;
                    if (_scanOptions.ScanDescription) fmIni.Descr = line.Substring(6).Trim();
                }
                else if (inDescr)
                {
                    if (_scanOptions.ScanDescription) fmIni.Descr += "\r\n" + line;
                }
            }

            if (_scanOptions.ScanDescription && !fmIni.Descr.IsEmpty()) fmIni.Descr = fmIni.Descr.Trim();

            #endregion

            #endregion

            #region Fixup description

            // Descr can be multiline. You're supposed to use \n for linebreaks. Most of the time people do that.
            // That's always nice when people do that. It's so much nicer than when they break an ini value into
            // multiple actual lines for some reason. God help us if any more keys get added to the spec and some
            // wise guy puts one of those keys after a multiline Descr.

            if (_scanOptions.ScanDescription && !fmIni.Descr.IsEmpty())
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
                if (fmIni.Descr.IsWhiteSpace()) fmIni.Descr = null;
            }

            #endregion

            #region Get author from tags

            if (_scanOptions.ScanAuthor)
            {
                if (fmIni.Tags != null)
                {
                    string[] tagsArray = fmIni.Tags.Split(CA_CommaSemicolon, StringSplitOptions.RemoveEmptyEntries);

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
            if (_scanOptions.ScanTags) ret.Tags = fmIni.Tags;

            if (_scanOptions.ScanTitle) ret.Title = fmIni.NiceName;

            if (_scanOptions.ScanReleaseDate)
            {
                string rd = fmIni.ReleaseDate;

                // The fm.ini Unix timestamp looks 32-bit, but FMSel's source code pegs it as int64. It must just
                // be writing only as many digits as it needs. That's good, because 32-bit will run out in 2038.
                // Anyway, we should parse it as long (NDL only does int, so it's in for a surprise in 20 years :P)
                if (long.TryParse(rd, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out long seconds))
                {
                    try
                    {
                        ret.LastUpdateDate = DateTimeOffset.FromUnixTimeSeconds(seconds).DateTime;
                    }
                    catch (ArgumentOutOfRangeException)
                    {
                        // Invalid date, leave blank
                    }
                }
                else if (!fmIni.ReleaseDate.IsEmpty())
                {
                    ret.LastUpdateDate = StringToDate(fmIni.ReleaseDate, out DateTime dt) ? (DateTime?)dt : null;
                }
            }

            if (_scanOptions.ScanDescription) ret.Description = fmIni.Descr;

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

            List<string> lines;

            if (_fmIsZip)
            {
                var e = _archive.Entries[file.Index];
                using var es = e.Open();
                lines = ReadAllLinesE(es, e.Length);
            }
            else
            {
                lines = ReadAllLinesE(Path.Combine(_fmWorkingPath, file.Name));
            }

            if (lines == null || lines.Count == 0) return ret;

            for (int i = 0; i < lines.Count; i++)
            {
                string lineT = lines[i].Trim();
                if (lineT.EqualsI("[modName]"))
                {
                    while (i < lines.Count - 1)
                    {
                        string lt = lines[i + 1].Trim();
                        if (!lt.IsEmpty() && lt[0] == '[' && lt[lt.Length - 1] == ']')
                        {
                            break;
                        }
                        else if (!lt.IsEmpty() && lt[0] != ';')
                        {
                            ret.Title = lt;
                            break;
                        }
                        i++;
                    }
                }
                else if (lineT.EqualsI("[authors]"))
                {
                    while (i < lines.Count - 1)
                    {
                        string lt = lines[i + 1].Trim();
                        if (!lt.IsEmpty() && lt[0] == '[' && lt[lt.Length - 1] == ']')
                        {
                            break;
                        }
                        else if (!lt.IsEmpty() && lt[0] != ';')
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

        #endregion

        // Because RTF files can have embedded images, their size can far exceed that normally expected of a
        // readme. To save time and memory, this method strips out such large data blocks before passing the
        // result to a WinForms RichTextBox for final conversion to plain text.
        private static bool GetRtfFileLinesAndText(Stream stream, int streamLength, RichTextBox rtfBox)
        {
            static bool ByteArrayStartsWith(byte[] first, byte[] second)
            {
                for (int i = 0; i < second.Length; i++) if (first[i] != second[i]) return false;
                return true;
            }

            if (stream.Position > 0) stream.Position = 0;

            // Don't parse files small enough to be unlikely to have embedded images; otherwise we're just parsing
            // it twice for nothing
            if (streamLength < ByteSize.KB * 256)
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

                        if (ByteArrayStartsWith(RtfTags.Bytes11, RtfTags.shppict) ||
                           ByteArrayStartsWith(RtfTags.Bytes11, RtfTags.objdata) ||
                           ByteArrayStartsWith(RtfTags.Bytes11, RtfTags.nonshppict) ||
                           ByteArrayStartsWith(RtfTags.Bytes11, RtfTags.pict))
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

                if (_fmIsZip) readmeEntry = _archive.Entries[readmeFile.Index];

                string fullReadmeFileName = null;

                FileInfo readmeFI = null;
                if (_fmDirFileInfos.Count > 0)
                {
                    for (int i = 0; i < _fmDirFileInfos.Count; i++)
                    {
                        FileInfo f = _fmDirFileInfos[i];
                        if (f.Name.PathEqualsI(fullReadmeFileName ??= Path.Combine(_fmWorkingPath, readmeFile.Name)))
                        {
                            readmeFI = f;
                            break;
                        }
                    }
                }

                int readmeFileLen =
                    _fmIsZip ? (int)readmeEntry.Length :
                    readmeFI != null ? (int)readmeFI.Length :
                    (int)new FileInfo(fullReadmeFileName ??= Path.Combine(_fmWorkingPath, readmeFile.Name)).Length;

                string readmeFileOnDisk = "";

                string fileName;
                DateTime lastModifiedDate;
                long readmeSize;

                if (_fmIsZip)
                {
                    fileName = readmeEntry.Name;
                    lastModifiedDate = new DateTimeOffset(ZipHelpers.ZipTimeToDateTime(readmeEntry.LastWriteTime)).DateTime;
                    readmeSize = readmeEntry.Length;
                }
                else
                {
                    readmeFileOnDisk = fullReadmeFileName ?? Path.Combine(_fmWorkingPath, readmeFile.Name);
                    FileInfo fi = readmeFI ?? new FileInfo(readmeFileOnDisk);
                    fileName = fi.Name;
                    lastModifiedDate = new DateTimeOffset(fi.LastWriteTime).DateTime;
                    readmeSize = fi.Length;
                }

                if (readmeSize == 0) continue;

                bool scanThisReadme = !readmeFile.Name.ExtIsHtml() && readmeFile.Name.IsEnglishReadme();

                _readmeFiles.Add(new ReadmeInternal
                {
                    FileName = fileName,
                    LastModifiedDate = lastModifiedDate,
                    Scan = scanThisReadme
                });

                if (!scanThisReadme) continue;

                // try-finally instead of using, because we only want to initialize the readme stream if FMIsZip
                Stream readmeStream = null;
                try
                {
                    if (_fmIsZip)
                    {
                        readmeStream = new MemoryStream(readmeFileLen);
                        using (var es = readmeEntry.Open()) es.CopyTo(readmeStream);

                        readmeStream.Position = 0;
                    }

                    // Saw one ".rtf" that was actually a plaintext file, and one vice versa. So detect by header
                    // alone.
                    byte[] rtfHeader;
                    using (var br = _fmIsZip
                        ? new BinaryReader(readmeStream, Encoding.ASCII, leaveOpen: true)
                        : new BinaryReader(new FileStream(readmeFileOnDisk, FileMode.Open, FileAccess.Read), Encoding.ASCII, leaveOpen: false))
                    {
                        // Null is a stupid micro-optimization so we don't waste a 6-byte alloc.
                        rtfHeader = br.BaseStream.Length >= RtfTags.HeaderBytesLength
                            ? br.ReadBytes(RtfTags.HeaderBytesLength)
                            : null;
                    }
                    if (_fmIsZip) readmeStream.Position = 0;

                    if (rtfHeader != null && rtfHeader.SequenceEqual(RtfTags.HeaderBytes))
                    {
                        bool success;
                        if (_fmIsZip)
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
                            ReadmeInternal last = _readmeFiles[_readmeFiles.Count - 1];
                            last.Lines = rtfBox.Lines.ToList();
                            last.Text = rtfBox.Text;
                        }
                    }
                    else
                    {
                        ReadmeInternal last = _readmeFiles[_readmeFiles.Count - 1];
                        last.Lines = _fmIsZip
                            ? ReadAllLinesE(readmeStream, readmeFileLen, streamIsSeekable: true)
                            : ReadAllLinesE(readmeFileOnDisk);

                        // Convert GLML files to plaintext by stripping the markup. Fortunately this is extremely
                        // easy as all tags are of the form [GLWHATEVER][/GLWHATEVER]. Very nice, very simple.
                        bool extIsGlml = last.FileName.ExtIsGlml();
                        if (extIsGlml)
                        {
                            for (int i = 0; i < last.Lines.Count; i++)
                            {
                                var matches = GLMLTagRegex.Matches(last.Lines[i]);
                                foreach (Match m in matches)
                                {
                                    // We put linebreaks in the middle of lines here, but see below for the fixup
                                    last.Lines[i] = last.Lines[i].Replace(m.Value, m.Value == "[GLNL]" ? "\r\n" : "");
                                }
                            }
                        }

                        last.Text = string.Join("\r\n", last.Lines);

                        if (extIsGlml)
                        {
                            // We may have inserted CRLFs into the middle of lines, so re-split the text into
                            // lines again to ensure no line contains CRLFs (ie. every CRLF will result in a
                            // separate line).
                            // This was working before out of sheer luck that [GLNL] tags were only occurring at
                            // the end of lines, but this isn't technically guaranteed.
                            last.Lines = last.Text.Split(SA_CRLF, StringSplitOptions.None).ToList();
                        }
                    }
                }
                finally
                {
                    readmeStream?.Dispose();
                }
            }
        }

        private string GetValueFromReadme(SpecialLogic specialLogic, List<string> titles = null, params string[] keys)
        {
            string ret = null;

            foreach (ReadmeInternal file in _readmeFiles)
            {
                if (!file.Scan) continue;

                if (specialLogic == SpecialLogic.NewDarkMinimumVersion)
                {
                    string ndv = GetNewDarkVersionFromText(file.Text);
                    if (!ndv.IsEmpty()) return ndv;
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
                        if (!ret.IsEmpty()) return ret;
                    }

                    ret = GetValueFromLines(specialLogic, keys, file.Lines);
                    if (ret.IsEmpty())
                    {
                        if (specialLogic == SpecialLogic.Author)
                        {
                            // PERF_TODO: We can move things around for perf here.
                            // We can put GetAuthorFromCopyrightMessage() here and put this down there, and be
                            // a little bit faster on average. But that causes a handful of differences in the
                            // output. Not enough to matter really, but the conceptual issue I have is that I
                            // don't quite like looking for copyright-section authors first. It feels like we
                            // should search other places first. I dunno.
                            ret = GetAuthorFromText(file.Text);
                            if (!ret.IsEmpty()) return ret;
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
            if (specialLogic == SpecialLogic.Author && ret.IsEmpty())
            {
                // We do this separately for performance and clarity; it's an uncommon case involving regex
                // searching and we don't want to run it unless we have to. Also, it's specific enough that we
                // don't really want to shoehorn it into the standard line search.
                ret = GetAuthorFromCopyrightMessage();

                if (!ret.IsEmpty()) return ret;

                // Finds eg.
                // Author:
                //      GORT (Shaun M.D. Morin)
                foreach (ReadmeInternal file in _readmeFiles)
                {
                    if (!file.Scan) continue;

                    ret = GetValueFromLines(SpecialLogic.AuthorNextLine, null, file.Lines);
                    if (!ret.IsEmpty()) return ret;
                }

                // PERF_TODO:
                // This is last because it used to have a dynamic and constantly re-instantiated regex in it. I
                // don't even know why I thought I needed that but it turns out I could just make it static like
                // the rest, so I did. Re-evaluate this and maybe put it higher?
                // Anything that can go before the full-text search probably should, because that's clearly the
                // slowest by far.
                ret = GetAuthorFromTitleByAuthorLine(titles);
            }

            return ret;
        }

        private static string GetValueFromLines(SpecialLogic specialLogic, string[] keys, List<string> lines)
        {
            if (specialLogic == SpecialLogic.AuthorNextLine)
            {
                for (int i = 0; i < lines.Count; i++)
                {
                    string lineT = lines[i].Trim();
                    if (!lineT.EqualsI("Author") && !lineT.EqualsI("Author:")) continue;

                    if (i < lines.Count - 2)
                    {
                        string lineAfterNext = lines[i + 2].Trim();
                        int lanLen = lineAfterNext.Length;
                        if ((lanLen > 0 && lineAfterNext[lanLen - 1] == ':' && lineAfterNext.Length <= 50) ||
                            lineAfterNext.IsWhiteSpace())
                        {
                            return lines[i + 1].Trim();
                        }
                    }
                }

                return null;
            }

            for (int lineIndex = 0; lineIndex < lines.Count; lineIndex++)
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

                    // Either in given case or in all caps, but not in lowercase, because that's given me at least
                    // one false positive
                    if (lineStartTrimmed.StartsWithGU(x))
                    {
                        lineStartsWithKey = true;
                        // Regex perf: fast enough not to worry about it
                        if (Regex.Match(lineStartTrimmed, @"^" + x + @"\s*(:|-|\u2013)",
                            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant).Success)
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
                    if (!finalValue.IsEmpty()) return finalValue;
                }
                else
                {
                    // Don't detect "Version "; too many false positives
                    // TODO: Can probably remove this check and then just sort out any false positives in GetVersion()
                    if (specialLogic == SpecialLogic.Version) continue;

                    for (int i = 0; i < keys.Length; i++)
                    {
                        string key = keys[i];
                        if (!lineStartTrimmed.StartsWithI(key)) continue;

                        // It's supposed to be finding a space after a key; this prevents it from finding the
                        // first space in the key itself if there is one.
                        string lineAfterKey = lineStartTrimmed.Remove(0, key.Length);

                        if (!lineAfterKey.IsEmpty() &&
                            (lineAfterKey[0] == ' ' || lineAfterKey[0] == '\t'))
                        {
                            string finalValue = lineAfterKey.TrimStart();
                            if (!finalValue.IsEmpty()) return finalValue;
                        }
                    }
                }
            }

            return null;
        }

        private static string CleanupValue(string value)
        {
            if (value.IsEmpty()) return value;

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

        #region Title(s) and mission names

        // This is kind of just an excuse to say that my scanner can catch the full proper title of Deceptive
        // Perception 2. :P
        // This is likely to be a bit loose with its accuracy, but since values caught here are almost certain to
        // end up as alternate titles, I can afford that.
        private List<string> GetTitlesFromTopOfReadmes(List<ReadmeInternal> readmes)
        {
            var ret = new List<string>();

            if (_readmeFiles == null || _readmeFiles.Count == 0) return ret;

            const int maxTopLines = 5;

            foreach (ReadmeInternal r in readmes)
            {
                if (r.FileName.ExtIsHtml() || r.Lines == null || r.Lines.Count == 0) continue;

                var lines = new List<string>();

                for (int i = 0; i < r.Lines.Count; i++)
                {
                    string line = r.Lines[i];
                    if (!line.IsWhiteSpace()) lines.Add(line);
                    if (lines.Count == maxTopLines) break;
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
                        lineT.StartsWithI("A fan mission") ||
                        lineT.StartsWithI("A Thief 3") ||
                        AThief3Mission.Match(lineT).Success ||
                        lineT.StartsWithI("A System Shock") ||
                        lineT.StartsWithI("An SS2"))
                    {
                        for (int j = 0; j < i; j++)
                        {
                            if (j > 0) titleConcat += " ";
                            titleConcat += lines[j];
                        }
                        // Set a cutoff for the length so we don't end up with a huge string that's obviously
                        // more than a title
                        if (!titleConcat.IsWhiteSpace() && titleConcat.Length <= 50)
                        {
                            ret.Add(CleanupTitle(titleConcat));
                        }

                        break;
                    }
                }
            }

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
                        x.Name.PathEqualsI(FMFiles.IntrfaceEnglishNewGameStrS))
                    ?? intrfaceDirFiles.FirstOrDefault(x =>
                        x.Name.PathEqualsI(FMFiles.IntrfaceNewGameStrS))
                    ?? intrfaceDirFiles.FirstOrDefault(x =>
                        x.Name.PathStartsWithI(FMDirs.IntrfaceS) &&
                        x.Name.PathEndsWithI(FMFiles.DscNewGameStrS));
            }

            if (newGameStrFile == null) return null;

            List<string> lines;

            if (_fmIsZip)
            {
                var e = _archive.Entries[newGameStrFile.Index];
                using var es = e.Open();
                lines = ReadAllLinesE(es, e.Length);
            }
            else
            {
                lines = ReadAllLinesE(Path.Combine(_fmWorkingPath, newGameStrFile.Name));
            }

            if (lines == null) return null;

            for (int i = 0; i < lines.Count; i++)
            {
                string lineT = lines[i].Trim();
                Match match = NewGameStrTitleRegex.Match(lineT);
                if (match.Success)
                {
                    string title = match.Groups["Title"].Value.Trim();
                    if (title.IsEmpty()) continue;

                    // Do our best to ignore things that aren't titles
                    if (// first chars
                        title[0] != '{' && title[0] != '}' && title[0] != '-' && title[0] != '_' &&
                        title[0] != ':' && title[0] != ';' && title[0] != '!' && title[0] != '@' &&
                        title[0] != '#' && title[0] != '$' && title[0] != '%' && title[0] != '^' &&
                        title[0] != '&' && title[0] != '*' && title[0] != '(' && title[0] != ')' &&
                        // entire titles
                        !title.EqualsI("Play") && !title.EqualsI("Start") &&
                        !title.EqualsI("Begin") && !title.EqualsI("Begin...") &&
                        !title.EqualsI("skip training") &&
                        // starting strings
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

        private (string TitleFrom0, string TitleFromN, List<string> CampaignMissionNames)
        GetMissionNames(List<NameAndIndex> stringsDirFiles, List<NameAndIndex> misFiles, List<NameAndIndex> usedMisFiles)
        {
            var titlesStrLines = GetTitlesStrLines(stringsDirFiles);
            if (titlesStrLines == null || titlesStrLines.Count == 0) return (null, null, null);

            var ret =
                (TitleFrom0: (string)null,
                TitleFromN: (string)null,
                CampaignMissionNames: (List<string>)null);

            static string ExtractFromQuotedSection(string line)
            {
                int i;
                return line.Substring(i = line.IndexOf('\"') + 1, line.IndexOf('\"', i) - i);
            }

            static bool NameExistsInList(List<NameAndIndex> list, string value)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    if (list[i].Name.ContainsI(value)) return true;
                }
                return false;
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
                    if (title.IsEmpty()) continue;

                    string umf = usedMisFiles[umfIndex].Name;
                    int umfDotIndex = umf.IndexOf('.');

                    if (umfDotIndex > 4 && umf.StartsWithI("miss") && titleNum == umf.Substring(4, umfDotIndex - 4))
                    {
                        titles.Add(title);
                    }
                }

                string missNumMis;

                if (_scanOptions.ScanTitle &&
                    ret.TitleFromN.IsEmpty() &&
                    lineIndex == titlesStrLines.Count - 1 &&
                    !titleNum.IsEmpty() &&
                    !title.IsEmpty() &&
                    !NameExistsInList(usedMisFiles, missNumMis = "miss" + titleNum + ".mis") &&
                    NameExistsInList(misFiles, missNumMis))
                {
                    ret.TitleFromN = title;
                    if (!_scanOptions.ScanCampaignMissionNames) break;
                }
            }

            if (titles.Count > 0)
            {
                if (_scanOptions.ScanTitle && titles.Count == 1)
                {
                    ret.TitleFromN = titles[0];
                }
                else if (_scanOptions.ScanCampaignMissionNames)
                {
                    ret.CampaignMissionNames = titles;
                }
            }

            return ret;
        }

        private List<string> GetTitlesStrLines(List<NameAndIndex> stringsDirFiles)
        {
            List<string> titlesStrLines = null;

            #region Read title(s).str file

            foreach (string titlesFileLocation in FMFiles.TitlesStrLocations)
            {
                NameAndIndex titlesFile = _fmIsZip
                    ? stringsDirFiles.FirstOrDefault(x => x.Name.PathEqualsI(titlesFileLocation))
                    : new NameAndIndex { Name = Path.Combine(_fmWorkingPath, titlesFileLocation) };

                if (titlesFile == null || !_fmIsZip && !File.Exists(titlesFile.Name)) continue;

                if (_fmIsZip)
                {
                    var e = _archive.Entries[titlesFile.Index];
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

            if (titlesStrLines == null || titlesStrLines.Count == 0) return null;

            #region Filter titlesStrLines

            // There's a way to do this with an IEqualityComparer, but no, for reasons
            var tfLinesD = new List<string>(titlesStrLines.Count);
            {
                for (int i = 0; i < titlesStrLines.Count; i++)
                {
                    int indexOfColon;
                    // Note: the Trim() is important, don't remove it
                    string line = titlesStrLines[i].Trim();
                    if (!line.IsEmpty() &&
                        line.StartsWithI("title_") &&
                        (indexOfColon = line.IndexOf(':')) > -1 &&
                        line.CharCountIsAtLeast('\"', 2) &&
                        !tfLinesD.Any(x => x.StartsWithI(line.Substring(0, indexOfColon))))
                    {
                        tfLinesD.Add(line);
                    }
                }
            }

            tfLinesD.Sort(_titlesStrNaturalNumericSort);

            #endregion

            return tfLinesD;
        }

        private static string CleanupTitle(string value)
        {
            if (value.IsEmpty()) return value;

            // Some titles are clever and  A r e  W r i t t e n  L i k e  T h i s
            // But we want to leave titles that are supposed to be acronyms - ie, "U F O", "R G B"
            if (value.Contains(' ') &&
                !TitleAnyConsecutiveLettersRegex.Match(value).Success &&
                TitleContainsLowerCaseCharsRegex.Match(value).Success)
            {
                if (value.Contains("  "))
                {
                    string[] titleWords = value.Split(SA_DoubleSpaces, StringSplitOptions.None);
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

        #endregion

        #region Author

        private static string GetAuthorFromTopOfReadme(List<string> lines, List<string> titles)
        {
            if (lines.Count == 0) return null;

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
            var topLines = new List<string>(maxTopLines);

            for (int i = 0; i < lines.Count; i++)
            {
                string line = lines[i];
                if (!line.IsWhiteSpace()) topLines.Add(line);
                if (topLines.Count == maxTopLines) break;
            }

            if (topLines.Count < 2) return null;

            for (int i = 0; i < topLines.Count; i++)
            {
                // Preemptive check
                if (i == 0 && titleStartsWithBy) continue;

                string lineT = topLines[i].Trim();
                if (lineT.StartsWithI("By ") || lineT.StartsWithI("By: "))
                {
                    string author = lineT.Substring(lineT.IndexOf(' ')).TrimStart();
                    if (!author.IsEmpty()) return author;
                }
                else if (lineT.EqualsI("By"))
                {
                    if (!titleContainsBy && i < topLines.Count - 1)
                    {
                        return topLines[i + 1].Trim();
                    }
                }
                else
                {
                    Match m = AuthorGeneralCopyrightRegex.Match(lineT);
                    if (!m.Success) continue;

                    string author = CleanupCopyrightAuthor(m.Groups["Author"].Value);
                    if (!author.IsEmpty()) return author;
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

            return !author.IsEmpty() ? author : null;
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

            // We DON'T just check the first five lines, because there might be another language section first
            // and this kind of author string might well be buried down in the file.
            foreach (ReadmeInternal rf in _readmeFiles)
            {
                if (!rf.Scan) continue;

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

                    Match match = TitleByAuthorRegex.Match(secondHalf);
                    if (match.Success) return match.Groups["Author"].Value;
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

            foreach (ReadmeInternal rf in _readmeFiles)
            {
                if (!rf.Scan) continue;

                bool inCopyrightSection = false;
                bool pastFirstLineOfCopyrightSection = false;

                foreach (string line in rf.Lines)
                {
                    if (line.IsWhiteSpace()) continue;

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
                    if (!author.IsEmpty())
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

            if (author.IsWhiteSpace()) return null;

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

            const string junkChars = "!@#$%^&*";
            bool authorLastCharIsJunk = false;
            char lastChar = author[author.Length - 1];
            for (int i = 0; i < junkChars.Length; i++)
            {
                if (lastChar == junkChars[i])
                {
                    authorLastCharIsJunk = true;
                    break;
                }
            }
            if (authorLastCharIsJunk && author[author.Length - 2] == ' ')
            {
                author = author.Substring(0, author.Length - 2);
            }

            author = author.TrimEnd(CA_Period).Trim();

            return author;
        }

        #endregion

        private string GetVersion()
        {
            string version = GetValueFromReadme(SpecialLogic.Version, null, SA_VersionDetect);

            if (version.IsEmpty()) return null;

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
        private static (List<string> Langs, bool EnglishIsUncertain)
        GetLanguages(List<NameAndIndex> baseDirFiles, List<NameAndIndex> booksDirFiles,
                     List<NameAndIndex> intrfaceDirFiles, List<NameAndIndex> stringsDirFiles)
        {
            var langs = new List<string>();
            bool englishIsUncertain = false;

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
                        // Directory separator agnostic & keeping perf reasonably high
                        string dfName = df.Name.Replace('\\', '/');

                        // We say HasFileExtension() because we only want to count lang dirs that have files in them
                        if (dfName.HasFileExtension() &&
                            (dfName.ContainsI(Languages_FS_Lang_FS[langIndex]) ||
                             dfName.ContainsI(Languages_FS_Lang_Language_FS[langIndex])))
                        {
                            langs.Add(lang);
                            break;
                        }
                    }
                }
            }

            if (!langs.ContainsI("english"))
            {
                langs.Add("english");
                englishIsUncertain = true;
            }

            // Sometimes extra languages are in zip files inside the FM archive
            for (int i = 0; i < baseDirFiles.Count; i++)
            {
                string fn = baseDirFiles[i].Name;
                if (!fn.ExtIsZip() && !fn.ExtIs7z() && !fn.ExtIsRar()) continue;

                // PERF_TODO: String allocation, but a large convenience
                fn = fn.RemoveExtension();

                // LINQ avoidance
                for (int j = 0; j < Languages.Length; j++)
                {
                    string lang = Languages[j];
                    if (fn.StartsWithI(lang)) langs.Add(lang);
                }

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
                var langsD = langs.Distinct().ToList();
                langsD.Sort();
                return (langsD, englishIsUncertain);
            }
            else
            {
                return (EnglishOnly, EnglishIsUncertain: true);
            }
        }

        private (bool? NewDarkRequired, Game Game)
        GetGameTypeAndEngine(List<NameAndIndex> baseDirFiles, List<NameAndIndex> usedMisFiles)
        {
            // PERF_TODO: Recycle the buffers in here

            var ret = (NewDarkRequired: (bool?)null, Game: Game.Null);

            #region Choose smallest .gam file

            NameAndIndex[] gamFiles = baseDirFiles.Where(x => x.Name.ExtIsGam()).ToArray();
            bool gamFileExists = gamFiles.Length > 0;

            var gamSizeList = new List<(string Name, int Index, long Size)>(gamFiles.Length);
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
                        if (_fmIsZip)
                        {
                            length = _archive.Entries[gam.Index].Length;
                        }
                        else
                        {
                            string gamFullPath = null;
                            FileInfo gamFI = _fmDirFileInfos.FirstOrDefault(x => x.FullName.PathEqualsI(gamFullPath ??= Path.Combine(_fmWorkingPath, gam.Name)));
                            length = gamFI?.Length ?? new FileInfo(gamFullPath ?? Path.Combine(_fmWorkingPath, gam.Name)).Length;
                        }
                        gamSizeList.Add((gam.Name, gam.Index, length));
                    }

                    var gamToUse = gamSizeList.OrderBy(x => x.Size).First();
                    smallestGamFile = new NameAndIndex { Name = gamToUse.Name, Index = gamToUse.Index };
                }
            }

            #endregion

            #region Choose smallest .mis file

            var misSizeList = new List<(string Name, int Index, long Size)>(usedMisFiles.Count);
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
                    if (_fmIsZip)
                    {
                        length = _archive.Entries[mis.Index].Length;
                    }
                    else
                    {
                        string misFullPath = null;
                        FileInfo misFI = _fmDirFileInfos.FirstOrDefault(x => x.FullName.PathEqualsI(misFullPath ??= Path.Combine(_fmWorkingPath, mis.Name)));
                        length = misFI?.Length ?? new FileInfo(misFullPath ?? Path.Combine(_fmWorkingPath, mis.Name)).Length;
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

            if (_fmIsZip)
            {
                if (gamFileExists) gamFileZipEntry = _archive.Entries[smallestGamFile.Index];
                misFileZipEntry = _archive.Entries[smallestUsedMisFile.Index];
            }
            else
            {
                misFileOnDisk = Path.Combine(_fmWorkingPath, smallestUsedMisFile.Name);
            }

            #endregion

            #region Check for SKYOBJVAR in .mis (determines OldDark/NewDark; determines game type for OldDark)

            /*
             SKYOBJVAR location key (byte position in file):
                 No SKYOBJVAR           - OldDark Thief 1/G
                 772                    - OldDark Thief 2                        Commonness: ~80%
                 7217                   - NewDark, could be either T1/G or T2    Commonness: ~14%
                 3093                   - NewDark, could be either T1/G or T2    Commonness: ~4%
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

            const int ss2MapParamLoc1 = 696;
            const int oldDarkT2Loc = 772;
            const int ss2MapParamLoc2 = 916;
            // Neither of these clash with SS2's SKYOBJVAR locations (3168, 7292).
            const int newDarkLoc1 = 7217;
            const int newDarkLoc2 = 3093;

            int[] locations = { ss2MapParamLoc1, oldDarkT2Loc, ss2MapParamLoc2, newDarkLoc1, newDarkLoc2 };

            const int ss2Offset1 = 705;      // 696+9 = 705
            const int t2OldDarkOffset = 76;  // (772+9)-705 = 76
            const int ss2Offset2 = 144;      // ((916+9)-76)-705 = 144
            const int newDarkOffset1 = 2177; // (((3093+9)-144)-76)-705 = 2177
            const int newDarkOffset2 = 4124; // ((((7217+9)-2177)-144)-76)-705 = 4124

            int[] zipOffsets = { ss2Offset1, t2OldDarkOffset, ss2Offset2, newDarkOffset1, newDarkOffset2 };

            // MAPPARAM is 8 bytes, so for that we just check the first 8 bytes and ignore the last, rather than
            // complicating things any further than they already are.
            const int locationBytesToRead = 9;
            bool foundAtNewDarkLocation = false;
            bool foundAtOldDarkThief2Location = false;

            // We need to say "length - x" because for zips, the buffer will be full offset size rather than
            // locationBytesToRead size
            static bool EndsWithSKYOBJVAR(byte[] buffer)
            {
                int len = buffer.Length;
                return buffer[len - 9] == 'S' &&
                       buffer[len - 8] == 'K' &&
                       buffer[len - 7] == 'Y' &&
                       buffer[len - 6] == 'O' &&
                       buffer[len - 5] == 'B' &&
                       buffer[len - 4] == 'J' &&
                       buffer[len - 3] == 'V' &&
                       buffer[len - 2] == 'A' &&
                       buffer[len - 1] == 'R';
            }

            static bool EndsWithMAPPARAM(byte[] buffer)
            {
                int len = buffer.Length;
                return buffer[len - 9] == 'M' &&
                       buffer[len - 8] == 'A' &&
                       buffer[len - 7] == 'P' &&
                       buffer[len - 6] == 'P' &&
                       buffer[len - 5] == 'A' &&
                       buffer[len - 4] == 'R' &&
                       buffer[len - 3] == 'A' &&
                       buffer[len - 2] == 'M';
            }

            using (var sr = _fmIsZip
                ? new BinaryReader(misFileZipEntry.Open(), Encoding.ASCII, false)
                : new BinaryReader(new FileStream(misFileOnDisk, FileMode.Open, FileAccess.Read), Encoding.ASCII, false))
            {
                for (int i = 0; i < locations.Length; i++)
                {
                    if (!_scanOptions.ScanNewDarkRequired &&
                        (locations[i] == newDarkLoc1 || locations[i] == newDarkLoc2))
                    {
                        break;
                    }

                    byte[] buffer;

                    if (_fmIsZip)
                    {
                        buffer = sr.ReadBytes(zipOffsets[i]);
                    }
                    else
                    {
                        sr.BaseStream.Position = locations[i];
                        buffer = sr.ReadBytes(locationBytesToRead);
                    }

                    if ((locations[i] == ss2MapParamLoc1 ||
                         locations[i] == ss2MapParamLoc2) &&
                        EndsWithMAPPARAM(buffer))
                    {
                        // TODO: @SS2: AngelLoader doesn't need to know if NewDark is required, but put that in eventually
                        return (null, Game.SS2);
                    }
                    else if ((locations[i] == oldDarkT2Loc ||
                              locations[i] == newDarkLoc1 ||
                              locations[i] == newDarkLoc2) &&
                             EndsWithSKYOBJVAR(buffer))
                    {
                        // Zip reading is going to check the NewDark locations the other way round, but fortunately
                        // they're interchangeable in meaning so we don't have to do anything
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
                return (_scanOptions.ScanNewDarkRequired ? (bool?)false : null,
                        _scanOptions.ScanGameType ? Game.Thief2 : Game.Null);
            }

            if (!_scanOptions.ScanGameType) return (ret.NewDarkRequired, Game.Null);

            #region Check for T2-unique value in .gam or .mis (determines game type for both OldDark and NewDark)

            static bool StreamContainsIdentString(Stream stream, byte[] identString)
            {
                try
                {
                    // To catch matches on a boundary between chunks, leave extra space at the start of each chunk
                    // for the last boundaryLen bytes of the previous chunk to go into, thus achieving a kind of
                    // quick-n-dirty "step back and re-read" type thing. Dunno man, it works.
                    int boundaryLen = identString.Length;
                    const int bufSize = 81_920;
                    byte[] chunk = new byte[boundaryLen + bufSize];

                    while (stream.Read(chunk, boundaryLen, bufSize) != 0)
                    {
                        if (chunk.Contains(identString)) return true;

                        // Copy the last boundaryLen bytes from chunk and put them at the beginning
                        for (int si = 0, ei = bufSize; si < boundaryLen; si++, ei++) chunk[si] = chunk[ei];
                    }

                    return false;
                }
                finally
                {
                    stream.Dispose();
                }
            }

            if (_fmIsZip)
            {
                // For zips, since we can't seek within the stream, the fastest way to find our string is just to
                // brute-force straight through.
                Stream stream = gamFileExists ? gamFileZipEntry.Open() : misFileZipEntry.Open();
                ret.Game = StreamContainsIdentString(stream, MisFileStrings.Thief2UniqueString)
                    ? Game.Thief2
                    : Game.Thief1;
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
                    byte[] header = br.ReadBytes(12);
                    uint offset = br.ReadUInt32();
                    uint length = br.ReadUInt32();

                    if (!header.Contains(MisFileStrings.OBJ_MAP)) continue;

                    br.BaseStream.Position = offset;

                    byte[] content = br.ReadBytes((int)length);
                    ret.Game = content.Contains(MisFileStrings.Thief2UniqueString)
                        ? Game.Thief2
                        : Game.Thief1;
                    break;
                }
            }

            #region SS2 slow-detect fallback

            // Paranoid fallback. In case MAPPARAM ends up at a different byte location in a future version of
            // NewDark, we run this check if we suspect we're dealing with an SS2 FM (we will have fingerprinted
            // it earlier during ReadAndCacheFMData() and again here). For T2, we have a fallback scan if we don't
            // find SKYOBJVAR at byte 772, so we're safe. But SS2 we should have a fallback in place as well. It's
            // really slow, but better slow than incorrect. This way, if a new SS2 FM is released and has MAPPARAM
            // in a different place, at least we're like 98% certain to still detect it correctly here. Then people
            // can still at least have an accurate detection while I work on a new version that takes the new
            // MAPPARAM location into account.

            static bool SS2MisFilesPresent(List<NameAndIndex> misFiles)
            {
                for (int mfI = 0; mfI < misFiles.Count; mfI++)
                {
                    string fn = misFiles[mfI].Name;
                    for (int ss2mfI = 0; ss2mfI < FMFiles.SS2MisFiles.Length; ss2mfI++)
                    {
                        if (fn.EqualsI(FMFiles.SS2MisFiles[ss2mfI]))
                        {
                            return true;
                        }
                    }
                }

                return false;
            }

            if (ret.Game == Game.Thief1 && (_ss2Fingerprinted || SS2MisFilesPresent(usedMisFiles)))
            {
                Stream stream = _fmIsZip
                    ? misFileZipEntry.Open()
                    : new FileStream(misFileOnDisk, FileMode.Open, FileAccess.Read);

                if (StreamContainsIdentString(stream, MisFileStrings.MAPPARAM))
                {
                    ret.Game = Game.SS2;
                }
            }

            #endregion

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

            if (version.IsEmpty()) return null;

            string ndv = version.Trim(CA_Period);
            int index = ndv.IndexOf('.');
            if (index > -1 && ndv.Substring(index + 1).Length < 2)
            {
                ndv += "0";
            }

            // Anything lower than 1.19 is OldDark; and cut it off at 2.0 to prevent that durn old time-travelling
            // Zealot's Hollow from claiming it was made with "NewDark Version 2.1"
            float ndvF = float.Parse(ndv);
            return ndvF >= 1.19 && ndvF < 2.0 ? ndv : null;
        }

        private static void DeleteFMWorkingPath(string fmWorkingPath)
        {
            try
            {
                var ds = Directory.GetDirectories(fmWorkingPath, "*", SearchOption.TopDirectoryOnly);
                for (int i = 0; i < ds.Length; i++) Directory.Delete(ds[i], true);

                Directory.Delete(fmWorkingPath, recursive: true);
            }
            catch (Exception)
            {
                // Don't care
            }
        }

        #region Helpers

        #region Generic dir/file functions

        private string[]
        EnumFiles(string searchPattern, SearchOption searchOption)
        {
            return EnumFiles("", searchPattern, searchOption, checkDirExists: false);
        }

        private string[]
        EnumFiles(string path, string searchPattern, SearchOption searchOption, bool checkDirExists = true)
        {
            string fullDir = Path.Combine(_fmWorkingPath, path);

            if (!checkDirExists || Directory.Exists(fullDir))
            {
                return Directory.GetFiles(fullDir, searchPattern, searchOption);
            }

            return new string[0];
        }

        #endregion

        #region ReadAllLines

        /// <summary>
        /// Reads all the lines in a stream, auto-detecting its encoding. Ensures non-ASCII characters show up
        /// correctly.
        /// </summary>
        /// <param name="stream">The stream to read.</param>
        /// <param name="length">The length of the stream in bytes.</param>
        /// <param name="streamIsSeekable">If true, the stream is used directly rather than copied, and is left
        /// open.</param>
        /// <returns></returns>
        private List<string> ReadAllLinesE(Stream stream, long length, bool streamIsSeekable = false)
        {
            var lines = new List<string>();

            // Quick hack
            if (streamIsSeekable)
            {
                stream.Position = 0;

                Encoding enc = _fileEncoding.DetectFileEncoding(stream);

                stream.Position = 0;

                // Code page 1252 = Western European (using instead of Encoding.Default)
                using var sr = new StreamReader(stream, enc ?? Encoding.GetEncoding(1252), false, 1024, leaveOpen: true);
                string line;
                while ((line = sr.ReadLine()) != null) lines.Add(line);
            }
            else
            {
                // Detecting the encoding of a stream reads it forward some amount, and I can't seek backwards in
                // an archive stream, so I have to copy it to a seekable MemoryStream. Blah.
                using var memStream = new MemoryStream((int)length);
                stream.CopyTo(memStream);
                stream.Dispose();
                memStream.Position = 0;
                Encoding enc = _fileEncoding.DetectFileEncoding(memStream);
                memStream.Position = 0;

                using var sr = new StreamReader(memStream, enc ?? Encoding.GetEncoding(1252), false);
                string line;
                while ((line = sr.ReadLine()) != null) lines.Add(line);
            }

            return lines;
        }

        private static List<string> ReadAllLines(Stream stream, Encoding encoding)
        {
            var lines = new List<string>();

            using var sr = new StreamReader(stream, encoding ?? Encoding.GetEncoding(1252), false);
            string line;
            while ((line = sr.ReadLine()) != null) lines.Add(line);

            return lines;
        }

        private static List<string> ReadAllLines(string file, Encoding encoding)
        {
            var lines = new List<string>();

            using var sr = new StreamReader(file, encoding, false);
            string line;
            while ((line = sr.ReadLine()) != null) lines.Add(line);

            return lines;
        }

        /// <summary>
        /// Reads all the lines in a file, auto-detecting its encoding. Ensures non-ASCII characters show up
        /// correctly.
        /// </summary>
        /// <param name="file">The file to read.</param>
        /// <returns></returns>
        private List<string> ReadAllLinesE(string file)
        {
            Encoding enc = _fileEncoding.DetectFileEncoding(file);

            var lines = new List<string>();
            using StreamReader streamReader = new StreamReader(file, enc ?? Encoding.GetEncoding(1252));
            string str;
            while ((str = streamReader.ReadLine()) != null) lines.Add(str);
            return lines;
        }

        #endregion

        #endregion

        public void Dispose() => _archive?.Dispose();
    }
}
