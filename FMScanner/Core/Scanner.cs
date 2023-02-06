﻿// Uncomment this define in all files it appears in to get all features (we use it for testing)
//#define FMScanner_FullCode
#define Enable7zReadmeCacheCode

// Null notes:
// -Lists are nullable because we want to avoid allocating new Lists all over the place.
// -Arrays are non-nullable because assigning them Array.Empty<T> is basically free.
// -Strings are non-nullable because assigning them the empty string "" is basically free.
// -A few temporary function-level strings are nullable for easier empty-check semantics etc., but returned strings
//  are non-nullable.

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
using System.Xml;
using AL_Common;
using AL_Common.FastZipReader;
using FMScanner.ScannerZipReader;
using FMScanner.SimpleHelpers;
using JetBrains.Annotations;
using SevenZip;
using static System.StringComparison;
using static AL_Common.Common;
using static AL_Common.Logger;

namespace FMScanner;

public sealed partial class Scanner : IDisposable
{
#if DEBUG
    private readonly Stopwatch _overallTimer = new Stopwatch();
#endif

    #region Public properties

    /// <summary>
    /// Hack to support scanning two different sets of fields depending on a bool, you pass in "full" scan
    /// fields here and "non-full" fields in the Scan* methods, and mark each passed FM with a bool.
    /// </summary>
    [PublicAPI]
    public ScanOptions FullScanOptions = new();

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

    private ZipArchiveS _archive = null!;
    private readonly ZipReusableBundleS _zipBundle;

    #endregion

    private readonly string _sevenZipExePath;

    private readonly FileEncoding _fileEncoding = new();

    private readonly List<FileInfoCustom> _fmDirFileInfos = new();

    private ScanOptions _scanOptions = new();

    // The custom RTF converter is designed to be instantiated once and run many times, recycling its own
    // fields as much as possible for performance.
    private RtfToTextConverter? _rtfConverter;
    private RtfToTextConverter RtfConverter => _rtfConverter ??= new RtfToTextConverter();

    private bool _fmIsZip;
    private bool _fmIsSevenZip;

    private string _fmWorkingPath = "";

    // Guess I'll leave this one global for reasons
    private readonly List<ReadmeInternal> _readmeFiles = new();

    private readonly TitlesStrNaturalNumericSort _titlesStrNaturalNumericSort = new();

    private bool _ss2Fingerprinted;

    private bool SS2FingerprintRequiredAndNotDone() => (
#if FMScanner_FullCode
        _scanOptions.ScanNewDarkRequired ||
#endif
        _scanOptions.ScanGameType) && !_ss2Fingerprinted;

    private byte[]? _diskFileStreamBuffer;
    private byte[] DiskFileStreamBuffer => _diskFileStreamBuffer ??= new byte[4096];

    #endregion

    #region Private classes

    private sealed class FileInfoCustom
    {
        internal readonly string FullName;
        internal readonly long Length;

        private ArchiveFileInfo? _archiveFileInfo;

        private DateTime? _lastWriteTime;
        internal DateTime LastWriteTime
        {
            get
            {
                if (_archiveFileInfo != null)
                {
                    _lastWriteTime = ((ArchiveFileInfo)_archiveFileInfo).LastWriteTime;
                    _archiveFileInfo = null;
                }
                return (DateTime)_lastWriteTime!;
            }
        }

        internal FileInfoCustom(FileInfo fileInfo)
        {
            FullName = fileInfo.FullName;
            Length = fileInfo.Length;
            _lastWriteTime = fileInfo.LastWriteTime;
        }

        internal FileInfoCustom(ArchiveFileInfo archiveFileInfo)
        {
            FullName = archiveFileInfo.FileName;
            Length = (long)archiveFileInfo.Size;
            _archiveFileInfo = archiveFileInfo;
        }
    }

    private sealed class ReadmeInternal
    {
        /// <summary>
        /// Check this bool to see if you want to scan the file or not. Currently false if readme is HTML or
        /// non-English.
        /// </summary>
        internal readonly bool Scan;
        internal readonly bool IsGlml;
        internal readonly List<string> Lines = new();
        internal string Text = "";

        // For zips, we store this simple uint until we need it, only then do we expand it into a full DateTime
        private uint? _lastModifiedDateRaw;
        private DateTime? _lastModifiedDate;
        internal DateTime LastModifiedDate
        {
            get
            {
                if (_lastModifiedDateRaw != null)
                {
                    _lastModifiedDate = new DateTimeOffset(ZipHelpers.ZipTimeToDateTime((uint)_lastModifiedDateRaw)).DateTime;
                    _lastModifiedDateRaw = null;
                }
                return (DateTime)_lastModifiedDate!;
            }
        }

        public ReadmeInternal(bool isGlml, uint lastModifiedDateRaw, bool scan)
        {
            IsGlml = isGlml;
            _lastModifiedDateRaw = lastModifiedDateRaw;
            Scan = scan;
        }

        public ReadmeInternal(bool isGlml, DateTime lastModifiedDate, bool scan)
        {
            IsGlml = isGlml;
            _lastModifiedDate = lastModifiedDate;
            Scan = scan;
        }
    }

    #endregion

    private enum SpecialLogic
    {
        None,
        Title,
        Author,
#if FMScanner_FullCode
        Version,
        NewDarkMinimumVersion
#endif
    }

    [PublicAPI]
    public Scanner(string sevenZipExePath)
    {
#if !NETFRAMEWORK
        System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
#endif

        _zipBundle = new ZipReusableBundleS();

        _sevenZipExePath = sevenZipExePath;

        #region Array construction

        int langsCount = Languages.Length;
        Languages_FS_Lang_FS = new string[langsCount];
        Languages_FS_Lang_Language_FS = new string[langsCount];
        LanguagesC = new DictionaryI<string>(langsCount);

        #region FMFiles_TitlesStrLocations

        // Do not change search order: strings/english, strings, strings/[any other language]
        FMFiles_TitlesStrLocations[0] = "strings/english/titles.str";
        FMFiles_TitlesStrLocations[1] = "strings/english/title.str";
        FMFiles_TitlesStrLocations[2] = "strings/titles.str";
        FMFiles_TitlesStrLocations[3] = "strings/title.str";

        for (int i = 1; i < langsCount; i++)
        {
            string lang = Languages[i];
            FMFiles_TitlesStrLocations[(i - 1) + 4] = "strings/" + lang + "/titles.str";
            FMFiles_TitlesStrLocations[(i - 1) + 4 + (langsCount - 1)] = "strings/" + lang + "/title.str";
        }

        #endregion

        #region Languages

        for (int i = 0; i < langsCount; i++)
        {
            string lang = Languages[i];
            Languages_FS_Lang_FS[i] = "/" + lang + "/";
            Languages_FS_Lang_Language_FS[i] = "/" + lang + " Language/";

            // Lowercase to first-char-uppercase dict: Cheesy hack because it wasn't designed this way.
            // All lang first chars are lowercase ASCII letters, so just subtract 32 to uppercase them.
            LanguagesC.Add(lang, (char)(lang[0] - 32) + lang.Substring(1));
        }

        #endregion

        #endregion
    }

    #region Scan synchronous

#if FMScanner_FullCode

    [PublicAPI]
    public ScannedFMDataAndError
    Scan(string mission, string tempPath, bool forceFullIfNew)
    {
        return ScanMany(
            new List<FMToScan> { new() { Path = mission, ForceFullScan = forceFullIfNew } },
            tempPath, _scanOptions, null, CancellationToken.None)[0];
    }

    [PublicAPI]
    public ScannedFMDataAndError
    Scan(string mission, string tempPath, ScanOptions scanOptions, bool forceFullIfNew)
    {
        return ScanMany(
            new List<FMToScan> { new() { Path = mission, ForceFullScan = forceFullIfNew } },
            tempPath, scanOptions, null, CancellationToken.None)[0];
    }

#endif

    // Debug - scan on UI thread so breaks will actually break where they're supposed to
    // (test frontend use only)
#if DEBUG || ScanSynchronous
    [PublicAPI]
    public List<ScannedFMDataAndError>
    Scan(List<FMToScan> missions, string tempPath, ScanOptions scanOptions,
         IProgress<ProgressReport> progress, CancellationToken cancellationToken)
    {
        return ScanMany(missions, tempPath, scanOptions, progress, cancellationToken);
    }
#endif

    #endregion

    #region Scan asynchronous

#if FMScanner_FullCode

    [PublicAPI]
    public Task<List<ScannedFMDataAndError>>
    ScanAsync(List<FMToScan> missions, string tempPath)
    {
        return Task.Run(() => ScanMany(missions, tempPath, _scanOptions, null, CancellationToken.None));
    }

    [PublicAPI]
    public Task<List<ScannedFMDataAndError>>
    ScanAsync(List<FMToScan> missions, string tempPath, ScanOptions scanOptions)
    {
        return Task.Run(() => ScanMany(missions, tempPath, scanOptions, null, CancellationToken.None));
    }

    [PublicAPI]
    public Task<List<ScannedFMDataAndError>>
    ScanAsync(List<FMToScan> missions, string tempPath, IProgress<ProgressReport> progress,
              CancellationToken cancellationToken)
    {
        return Task.Run(() => ScanMany(missions, tempPath, _scanOptions, progress, cancellationToken));
    }

#endif

    [PublicAPI]
    public Task<List<ScannedFMDataAndError>>
    ScanAsync(List<FMToScan> missions, string tempPath, ScanOptions scanOptions,
              IProgress<ProgressReport> progress, CancellationToken cancellationToken)
    {
        return Task.Run(() => ScanMany(missions, tempPath, scanOptions, progress, cancellationToken));
    }

    #endregion

    private List<ScannedFMDataAndError>
    ScanMany(List<FMToScan> missions, string tempPath, ScanOptions scanOptions,
             IProgress<ProgressReport>? progress, CancellationToken cancellationToken)
    {
        // The try-catch blocks are to guarantee that the out-list will at least contain the same number of
        // entries as the in-list; this allows the calling app to not have to do a search to link up the FMs
        // and stuff

        #region Checks

        if (tempPath.IsEmpty())
        {
            Log("Argument is null or empty: " + nameof(tempPath));
            throw new ArgumentException("Argument is null or empty.", nameof(tempPath));
        }

        if (missions == null) throw new ArgumentNullException(nameof(missions));
        if (missions.Count == 0 || (missions.Count == 1 && missions[0].Path.IsEmpty()))
        {
            Log("No mission(s) specified. tempPath: " + tempPath);
            throw new ArgumentException("No mission(s) specified.", nameof(missions));
        }

        // Deep-copy the scan options object because we might have to change its values in some cases, but we
        // don't want to modify the original because the caller will still have a reference to it and may
        // depend on it not changing.
        _scanOptions = scanOptions?.DeepCopy() ?? throw new ArgumentNullException(nameof(scanOptions));

        #endregion

        var scannedFMDataList = new List<ScannedFMDataAndError>(missions.Count);

        var progressReport = new ProgressReport();

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
                scannedFMDataList.Add(new ScannedFMDataAndError());
                nullAlreadyAdded = true;
            }
            else
            {
                string fmPath = missions[i].Path;
                _fmIsZip = fmPath.ExtIsZip() || fmPath.ExtIs7z();

                _archive?.Dispose();

                if (_fmIsZip)
                {
                    try
                    {
                        _fmWorkingPath = Path.Combine(tempPath, Path.GetFileNameWithoutExtension(fmPath).Trim());
                    }
                    catch (Exception ex)
                    {
                        Log(missions[i].Path + ": Path.Combine error, paths are probably invalid", ex);
                        scannedFMDataList.Add(new ScannedFMDataAndError());
                        nullAlreadyAdded = true;
                    }
                }
                else
                {
                    _fmWorkingPath = fmPath;
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
                progressReport.Percent = GetPercentFromValue_Int(missions.Count == 1 ? 0 : i + 1, missions.Count);

                progress.Report(progressReport);
            }

            #endregion

            // If there was an error then we already added null to the list. DON'T add any extra items!
            if (!nullAlreadyAdded)
            {
                var scannedFMAndError = new ScannedFMDataAndError();
                ScanOptions? _tempScanOptions = null;
                try
                {
                    if (missions[i].ForceFullScan)
                    {
                        _tempScanOptions = _scanOptions.DeepCopy();
                        _scanOptions = FullScanOptions.DeepCopy();
                    }

                    try
                    {
                        scannedFMAndError = ScanCurrentFM(missions[i], tempPath, cancellationToken);
                    }
                    catch (OperationCanceledException)
                    {
                        throw;
                    }
                    catch (Exception ex)
                    {
                        Log(missions[i].Path + ": Exception in FM scan", ex);
                        scannedFMAndError.ScannedFMData = null;
                        scannedFMAndError.Exception = ex;
                        scannedFMAndError.ErrorInfo = missions[i].Path + ": Exception in FM scan";
                        // Okay, we don't want to throw because that would stop the whole scan, but we want
                        // to continue scanning any further FMs that may be in the list. So just log.
                    }
                    finally
                    {
                        if (missions[i].Path.ExtIs7z()) DeleteFMWorkingPath();
                    }

                    scannedFMDataList.Add(scannedFMAndError);
                }
                finally
                {
                    if (missions[i].ForceFullScan)
                    {
                        _scanOptions = _tempScanOptions!.DeepCopy();
                    }
                }
            }

            if (progress != null && i == missions.Count - 1)
            {
                progressReport.Percent = 100;
                progress.Report(progressReport);
            }
        }

        return scannedFMDataList;
    }

    private ScannedFMDataAndError
    ScanCurrentFM(FMToScan fm, string tempPath, CancellationToken cancellationToken)
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

        static ScannedFMDataAndError UnsupportedZip(string archivePath, Fen7z.Result? fen7zResult, Exception? ex, string errorInfo) => new()
        {
            ScannedFMData = new ScannedFMData
            {
                ArchiveName = Path.GetFileName(archivePath),
                Game = Game.Unsupported,
                MissionCount = 0
            },
            Fen7zResult = fen7zResult,
            Exception = ex,
            ErrorInfo = errorInfo
        };

        static ScannedFMDataAndError UnknownZip(string archivePath, Fen7z.Result? fen7zResult, Exception? ex, string errorInfo) => new()
        {
            ScannedFMData = new ScannedFMData
            {
                ArchiveName = Path.GetFileName(archivePath),
                Game = Game.Null,
                MissionCount = 0
            },
            Fen7zResult = fen7zResult,
            Exception = ex,
            ErrorInfo = errorInfo
        };

        static ScannedFMDataAndError UnsupportedDir(Fen7z.Result? fen7zResult, Exception? ex, string errorInfo) => new()
        {
            ScannedFMData = null,
            Fen7zResult = fen7zResult,
            Exception = ex,
            ErrorInfo = errorInfo
        };

        ulong? sevenZipSize = null;

        #region Setup

        #region Check for and setup 7-Zip

        if (_fmIsZip && fm.Path.ExtIs7z())
        {
            _fmIsZip = false;
            _fmIsSevenZip = true;

            #region Partial 7z extract

            /*
            Rather than extracting everything, we only extract files we might need. We may still end up
            extracting more than we need, but it's WAY less than just dumbly doing the whole thing. Over
            my limited set of 45 7z files, this makes us about 4x faster on average. Certain individual
            FMs may still be about as slow depending on their structure and content, but meh. Improvement
            is improvement.

            IMPORTANT(Scanner partial 7z extract):
            The logic for deciding which files to extract (taking files and then de-duping the list) needs
            to match the logic for using them. If we change the usage logic, we need to change this too!
            */

            try
            {
                static bool EndsWithTitleFile(string fileName)
                {
                    return fileName.PathEndsWithI("/titles.str") ||
                           fileName.PathEndsWithI("/title.str");
                }

                Directory.CreateDirectory(_fmWorkingPath);

                cancellationToken.ThrowIfCancellationRequested();

                /*
                We still use SevenZipSharp for merely getting the file names and metadata, as that doesn't
                involve any decompression and shouldn't trigger any out-of-memory errors. We use this so
                we can get last write times in DateTime format and not have to parse possible localized
                text dates out of the output stream.
                */
                var fileNamesList = new List<string>();
                uint extractorFilesCount;
                using (var _sevenZipArchive = new SevenZipExtractor(fm.Path) { PreserveDirectoryStructure = true })
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    sevenZipSize = (ulong)_sevenZipArchive.PackedSize;
                    extractorFilesCount = _sevenZipArchive.FilesCount;
                    for (int i = 0; i < extractorFilesCount; i++)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        ArchiveFileInfo entry = _sevenZipArchive.ArchiveFileData[i];
                        string fn = entry.FileName;
                        int dirSeps;

                        // Always extract readmes no matter what, so our .7z caching is always correct.
                        // Also maybe we would need to always extract them regardless for other reasons, but
                        // yeah.
                        if (entry.FileName.IsValidReadme() && entry.Size > 0 &&
                            (((dirSeps = fn.Rel_CountDirSepsUpToAmount(2)) == 1 &&
                              (fn.PathStartsWithI(FMDirs.T3FMExtras1S) ||
                               fn.PathStartsWithI(FMDirs.T3FMExtras2S))) ||
                             dirSeps == 0))
                        {
                            fileNamesList.Add(entry.FileName);
                        }
                        // Only extract these if we need them!
                        else if ((_scanOptions.ScanGameType
#if FMScanner_FullCode
                                  || _scanOptions.ScanNewDarkRequired
#endif
                                 ) &&
                                 !entry.FileName.Rel_ContainsDirSep() &&
                                 (entry.FileName.ExtIsMis() ||
                                  entry.FileName.ExtIsGam()))
                        {
                            fileNamesList.Add(entry.FileName);
                        }
                        else if (!entry.FileName.Rel_ContainsDirSep() &&
                                 (entry.FileName.EqualsI(FMFiles.FMInfoXml) ||
                                  entry.FileName.EqualsI(FMFiles.FMIni) ||
                                  entry.FileName.EqualsI(FMFiles.ModIni)))
                        {
                            fileNamesList.Add(entry.FileName);
                        }
                        else if (entry.FileName.PathStartsWithI(FMDirs.StringsS) &&
                                 entry.FileName.PathEndsWithI(FMFiles.SMissFlag))
                        {
                            fileNamesList.Add(entry.FileName);
                        }
                        else if (entry.FileName.PathEndsWithI(FMFiles.SNewGameStr))
                        {
                            fileNamesList.Add(entry.FileName);
                        }
                        else if (EndsWithTitleFile(entry.FileName))
                        {
                            fileNamesList.Add(entry.FileName);
                        }

                        _fmDirFileInfos.Add(new FileInfoCustom(entry));
                    }
                }

                #region De-duplicate list

                // Some files could have multiple copies in different folders, but we only want to extract
                // the one we're going to use. We separate out this more complex and self-dependent logic
                // here. Doing this nonsense is still faster than extracting to disk.

                var tempList = new List<string>(fileNamesList.Count);

                static void PopulateTempList(
                    List<string> fileNamesList,
                    List<string> tempList,
                    Func<string, bool> predicate)
                {
                    tempList.Clear();

                    for (int i = 0; i < fileNamesList.Count; i++)
                    {
                        string fileName = fileNamesList[i];
                        if (predicate(fileName))
                        {
                            tempList.Add(fileName);
                            fileNamesList.RemoveAt(i);
                            i--;
                        }
                    }
                }

                PopulateTempList(fileNamesList, tempList, static x => x.PathEndsWithI(FMFiles.SMissFlag));

                // TODO: We might be able to put these into a method that takes a predicate so they're not duplicated
                // (from the normal logic way down there somewhere)
                string? missFlagToUse =
                    tempList.Find(static x =>
                        x.PathEqualsI(FMFiles.StringsMissFlag))
                    ?? tempList.Find(static x =>
                        x.PathEqualsI(FMFiles.StringsEnglishMissFlag))
                    ?? tempList.Find(static x =>
                        x.PathEndsWithI(FMFiles.SMissFlag));

                if (missFlagToUse != null)
                {
                    fileNamesList.Add(missFlagToUse);
                }

                PopulateTempList(fileNamesList, tempList, static x => x.PathEndsWithI(FMFiles.SNewGameStr));

                string? newGameStrToUse =
                    tempList.Find(static x =>
                        x.PathEqualsI(FMFiles.IntrfaceEnglishNewGameStr))
                    ?? tempList.Find(static x =>
                        x.PathEqualsI(FMFiles.IntrfaceNewGameStr))
                    ?? tempList.Find(static x =>
                        x.PathStartsWithI(FMDirs.IntrfaceS) &&
                        x.PathEndsWithI(FMFiles.SNewGameStr));

                if (newGameStrToUse != null)
                {
                    fileNamesList.Add(newGameStrToUse);
                }

                PopulateTempList(fileNamesList, tempList, EndsWithTitleFile);

                foreach (string titlesFileLocation in FMFiles_TitlesStrLocations)
                {
                    string? titlesFileToUse = tempList.Find(x => x.PathEqualsI(titlesFileLocation));
                    if (titlesFileToUse != null)
                    {
                        fileNamesList.Add(titlesFileToUse);
                        break;
                    }
                }

                #endregion

                string listFile = Path.Combine(tempPath, new DirectoryInfo(_fmWorkingPath).Name + ".7zl");

                Fen7z.Result result = Fen7z.Extract(
                    sevenZipWorkingPath: Path.GetDirectoryName(_sevenZipExePath)!,
                    sevenZipPathAndExe: _sevenZipExePath,
                    archivePath: fm.Path,
                    outputPath: _fmWorkingPath,
                    entriesCount: (int)extractorFilesCount,
                    listFile: listFile,
                    fileNamesList: fileNamesList,
                    cancellationToken: cancellationToken);

                cancellationToken.ThrowIfCancellationRequested();

                if (result.ErrorOccurred)
                {
                    Log(fm.Path + ": fm is 7z\r\n" +
                        "7z.exe path: " + _sevenZipExePath + "\r\n" +
                        result);

                    return UnsupportedZip(
                        archivePath: fm.Path,
                        fen7zResult: result,
                        ex: null,
                        errorInfo: "7z.exe path: " + _sevenZipExePath + "\r\n" +
                                   fm.Path + ": fm is 7z\r\n");
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                Log(fm.Path + ": fm is 7z, exception in 7z.exe extraction", ex);
                return UnsupportedZip(
                    archivePath: fm.Path,
                    fen7zResult: null,
                    ex: ex,
                    errorInfo: "7z.exe path: " + _sevenZipExePath + "\r\n" +
                               fm.Path + ": fm is 7z, exception in 7z.exe extraction"
                );
            }

            #endregion

#if Enable7zReadmeCacheCode
            if (!fm.CachePath.IsEmpty())
            {
                try
                {
                    CopySevenZipReadmesToCacheDir(fm);
                }
                catch
                {
                    // ignore for now
                }
                cancellationToken.ThrowIfCancellationRequested();
            }
#endif
        }

        #endregion

        #region Check for and setup Zip

        if (_fmIsZip)
        {
            Debug.WriteLine("----------" + fm.Path);

            if (fm.Path.ExtIsZip())
            {
                try
                {
                    _archive = new ZipArchiveS(GetFileStreamFast(fm.Path, _zipBundle.FileStreamBuffer), _zipBundle, allowUnsupportedEntries: false);

                    // Archive.Entries is lazy-loaded, so this will also trigger any exceptions that may be
                    // thrown while loading them. If this passes, we're definitely good.
                    if (_archive.Entries.Count == 0)
                    {
                        Log(fm.Path + ": fm is zip, no files in archive. Returning 'Unsupported' game type.", stackTrace: false);
                        return UnsupportedZip(fm.Path, null, null, "");
                    }
                }
                catch (Exception ex)
                {
                    #region Notes about semi-broken zips
                    /*
                    Semi-broken but still workable zip files throw on open (FMSel can work with them, but we can't)
                    
                    Known semi-broken files:
                    Uguest.zip (https://archive.org/download/ThiefMissions/) (Part 3.zip)
                    1999-08-11_UninvitedGuests.zip (https://mega.nz/folder/QfZG0AZA#cGHPc2Fu708Uuo4itvMARQ)

                    Both files are byte-identical but just with different names.

                    Note that my version of the second file (same name) is not broken, I got it from
                    http://ladyjo1.free.fr/ back in like 2018 or whenever I got that big pack to test the
                    scanner with.

                    These files throw with "The archive entry was compressed using an unsupported compression method."
                    They throw on both ZipArchiveFast() and regular built-in ZipArchive().

                    The compression method for each file in the archive is:

                    MISS15.MIS:                6
                    UGUEST.TXT:                6
                    INTRFACE/NEWGAME.STR:      1
                    INTRFACE/UGUEST/GOALS.STR: 1
                    STRINGS/MISSFLAG.STR:      1
                    STRINGS/TITLES.STR:        6

                    1 = Shrink
                    6 = Implode

                    (according to https://pkware.cachefly.net/webdocs/casestudies/APPNOTE.TXT)

                   .NET zip (all versions AFAIK) only supports Deflate.

                    There also seem to be other errors, involving "headers" and "data past the end of archive".
                    I don't really know enough about the zip format to understand these that much.

                    7z.exe can handle these files, so we could use that as a fallback, but:

                    7z.exe reports:

                    ERRORS:
                    Headers Error

                    WARNINGS:
                    There are data after the end of archive

                    And it considers the error to be "fatal" even though it succeeds in this case (the
                    extracted dir diffs identical with the extracted dir of the working one).
                    But if we're going to attempt to sometimes allow fatal errors to count as "success", I
                    dunno how we would tell the difference between that and an ACTUAL fatal (ie. extract did
                    not result in intact files on disk) error. If we just match by "Headers error" and/or
                    "data past end" who knows if sometimes those might actually result in bad output and not
                    others. I don't know. So we're going to continue to fail in this case, but at least tell
                    the user what's wrong and give them an actionable suggestion.
                    */
                    #endregion
                    if (ex is ZipCompressionMethodException zipEx)
                    {
                        Log(fm.Path + ": fm is zip.\r\n" +
                            "UNSUPPORTED COMPRESSION METHOD\r\n" +
                            "Zip contains one or more files compressed with an unsupported method. " +
                            "Only the DEFLATE method is supported. Try manually extracting and re-creating the zip file.\r\n" +
                            "Returning 'Unknown' game type.", zipEx);
                        return UnknownZip(fm.Path, null, zipEx, "");
                    }
                    // Invalid zip file, whatever, move on
                    else
                    {
                        Log(fm.Path + ": fm is zip, exception in " +
                            nameof(ZipArchiveS) +
                            " construction or entries getting. Returning 'Unsupported' game type.", ex);
                        return UnsupportedZip(fm.Path, null, ex, "");
                    }
                }
            }
            else
            {
                Log(fm.Path + ": " + nameof(_fmIsZip) +
                    " == true, but extension was not .zip. Returning 'Unsupported' game type.", stackTrace: false);
                return UnsupportedZip(fm.Path, null, null, "");
            }
        }
        else
        {
            if (!Directory.Exists(_fmWorkingPath))
            {
                Log(fm.Path + ": fm is dir, but " + nameof(_fmWorkingPath) +
                    " (" + _fmWorkingPath + ") doesn't exist. Returning 'Unsupported' game type.", stackTrace: false);
                return UnsupportedDir(null, null, "");
            }
            Debug.WriteLine("----------" + _fmWorkingPath);
        }

        #endregion

        #endregion

        var fmData = new ScannedFMData
        {
            ArchiveName = _fmIsZip || _fmIsSevenZip
                ? Path.GetFileName(fm.Path)
                // @PERF_TODO: Use fast string-only name getter
                // Case against: Being built-in, this way is safe and complete. ArchiveName is monumentally
                // important, we don't want to mess around.
                : new DirectoryInfo(_fmWorkingPath).Name
        };

        // There's one author scan that depends on the title ("[title] by [author]"), so we need to scan
        // titles in that case, but we shouldn't actually set the title in the return object because the
        // caller didn't request it.
        bool scanTitleForAuthorPurposesOnly = false;
        if ((_scanOptions.ScanTags || _scanOptions.ScanAuthor) && !_scanOptions.ScanTitle)
        {
            _scanOptions.ScanTitle = true;
            scanTitleForAuthorPurposesOnly = true;
        }

        #region Size

        // Getting the size is horrendously expensive for folders, but if we're doing it then we can save
        // some time later by using the FileInfo list as a cache.
        if (_scanOptions.ScanSize)
        {
            if (_fmIsZip)
            {
                fmData.Size = (ulong)_archive.ArchiveStreamLength;
            }
            else
            {
                if (_fmDirFileInfos.Count == 0)
                {
                    // PERF: AddRange() is a fat slug, do Add() in a loop instead
                    FileInfo[] fileInfos = new DirectoryInfo(_fmWorkingPath).GetFiles("*", SearchOption.AllDirectories);
                    // Don't reduce capacity, as that causes a reallocation
                    if (_fmDirFileInfos.Capacity < fileInfos.Length) _fmDirFileInfos.Capacity = fileInfos.Length;
                    for (int i = 0; i < fileInfos.Length; i++)
                    {
                        _fmDirFileInfos.Add(new FileInfoCustom(fileInfos[i]));
                    }
                }

                if (_fmIsSevenZip)
                {
                    fmData.Size = sevenZipSize;
                }
                else
                {
                    long size = 0;
                    foreach (FileInfoCustom fi in _fmDirFileInfos) size += fi.Length;
                    fmData.Size = (ulong)size;
                }
            }
        }

        #endregion

        // @PERF_TODO: Recycle these
        var baseDirFiles = new List<NameAndIndex>();
        var misFiles = new List<NameAndIndex>();
        var usedMisFiles = new List<NameAndIndex>();
        var stringsDirFiles = new List<NameAndIndex>();
        var intrfaceDirFiles = new List<NameAndIndex>();
        var booksDirFiles = new List<NameAndIndex>();
        var t3FMExtrasDirFiles = new List<NameAndIndex>();

        #region Cache FM data

        bool success = ReadAndCacheFMData(
            fm.Path,
            fmData,
            baseDirFiles,
            misFiles,
            usedMisFiles,
            stringsDirFiles,
            intrfaceDirFiles,
            booksDirFiles,
            t3FMExtrasDirFiles);

        if (!success)
        {
            string ext = _fmIsZip ? "zip" : _fmIsSevenZip ? "7z" : "dir";
            Log(fm.Path + ": fm is " + ext + ", " +
                nameof(ReadAndCacheFMData) + " returned false. Returning 'Unsupported' game type.", stackTrace: false);

            return _fmIsZip || _fmIsSevenZip ? UnsupportedZip(fm.Path, null, null, "") : UnsupportedDir(null, null, "");
        }

        #endregion

        bool fmIsT3 = fmData.Game == Game.Thief3;

        fmData.Type = usedMisFiles.Count > 1 ? FMType.Campaign : FMType.FanMission;

        fmData.MissionCount = usedMisFiles.Count;

        if (_scanOptions.GetOptionsEnum() == ScanOptionsEnum.MissionCount)
        {
            // Early return for perf if we're not scanning anything else
            return new ScannedFMDataAndError { ScannedFMData = fmData };
        }

        var altTitles = new List<string>();

        string finalTitle = "";

        void SetOrAddTitle(string value)
        {
            value = CleanupTitle(value).Trim();

            if (value.IsEmpty()) return;

            if (finalTitle.IsEmpty())
            {
                finalTitle = value;
            }
            else if (!finalTitle.EqualsI(value) && !altTitles.ContainsI(value))
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

            if (
#if FMScanner_FullCode
                _scanOptions.ScanNewDarkRequired ||
#endif
                _scanOptions.ScanGameType)
            {
                var (newDarkRequired, game) = GetGameTypeAndEngine(baseDirFiles, usedMisFiles);
#if FMScanner_FullCode
                if (_scanOptions.ScanNewDarkRequired) fmData.NewDarkRequired = newDarkRequired;
#endif
                if (_scanOptions.ScanGameType)
                {
                    fmData.Game = game;
                    if (fmData.Game == Game.Unsupported)
                    {
                        return new ScannedFMDataAndError { ScannedFMData = fmData };
                    }
                }
            }

            fmIsSS2 = fmData.Game == Game.SS2;

            #endregion

            #region Check info files

            if (_scanOptions.ScanTitle || _scanOptions.ScanAuthor ||
#if FMScanner_FullCode
                _scanOptions.ScanVersion ||
#endif
                _scanOptions.ScanReleaseDate || _scanOptions.ScanTags)
            {
                for (int i = 0; i < baseDirFiles.Count; i++)
                {
                    NameAndIndex f = baseDirFiles[i];
                    if (f.Name.EqualsI(FMFiles.FMInfoXml))
                    {
                        var (title, author, version, releaseDate) = ReadFMInfoXml(f);
                        if (_scanOptions.ScanTitle) SetOrAddTitle(title);
                        if (_scanOptions.ScanTags || _scanOptions.ScanAuthor)
                        {
                            fmData.Author = author;
                        }
#if FMScanner_FullCode
                        if (_scanOptions.ScanVersion) fmData.Version = version;
#endif
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
                        if ((_scanOptions.ScanTags || _scanOptions.ScanAuthor) && !author.IsEmpty())
                        {
                            fmData.Author = author;
                        }
#if FMScanner_FullCode
                        if (_scanOptions.ScanDescription) fmData.Description = description;
#endif
                        if (_scanOptions.ScanReleaseDate && lastUpdateDate != null) fmData.LastUpdateDate = lastUpdateDate;
                        if (_scanOptions.ScanTags) fmData.TagsString = tags;
                        break;
                    }
                }
            }
            if (_scanOptions.ScanTitle || _scanOptions.ScanTags || _scanOptions.ScanAuthor)
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
                        if ((_scanOptions.ScanTags || _scanOptions.ScanAuthor) && !author.IsEmpty())
                        {
                            fmData.Author = author;
                        }
                        break;
                    }
                }
            }

            #endregion
        }

        #region Read, cache, and set readme files

        // @PERF_TODO: Recycle this
        var readmeDirFiles = new List<NameAndIndex>();

        foreach (NameAndIndex f in baseDirFiles) readmeDirFiles.Add(f);

        if (fmIsT3) foreach (NameAndIndex f in t3FMExtrasDirFiles) readmeDirFiles.Add(f);

        ReadAndCacheReadmeFiles(readmeDirFiles);

        #endregion

#if FMScanner_FullCode
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
#endif

        #region Set release date

        if (_scanOptions.ScanReleaseDate && fmData.LastUpdateDate == null)
        {
            fmData.LastUpdateDate = GetReleaseDate(usedMisFiles);
        }

        #endregion

        #region Title and IncludedMissions

        // SS2 doesn't have a missions list or a titles list file
        if (!fmIsT3 && !fmIsSS2)
        {
            if (_scanOptions.ScanTitle
#if FMScanner_FullCode
                || _scanOptions.ScanCampaignMissionNames
#endif
               )
            {
                var (titleFrom0, titleFromN, cNames) = GetMissionNames(stringsDirFiles, misFiles, usedMisFiles);
                if (_scanOptions.ScanTitle)
                {
                    SetOrAddTitle(titleFrom0);
                    SetOrAddTitle(titleFromN);
                }
#if FMScanner_FullCode
                if (_scanOptions.ScanCampaignMissionNames && cNames != null && cNames.Count > 0)
                {
                    for (int i = 0; i < cNames.Count; i++) cNames[i] = CleanupTitle(cNames[i]);
                    fmData.IncludedMissions = cNames.ToArray();
                }
#endif
            }
        }

        if (_scanOptions.ScanTitle)
        {
            SetOrAddTitle(GetValueFromReadme(SpecialLogic.Title, titles: null, SA_TitleDetect));

            if (!fmIsT3) SetOrAddTitle(GetTitleFromNewGameStrFile(intrfaceDirFiles));

            List<string>? topOfReadmeTitles = GetTitlesFromTopOfReadmes(_readmeFiles);
            if (topOfReadmeTitles?.Count > 0)
            {
                for (int i = 0; i < topOfReadmeTitles.Count; i++) SetOrAddTitle(topOfReadmeTitles[i]);
            }

            if (!scanTitleForAuthorPurposesOnly)
            {
                fmData.Title = finalTitle;
                fmData.AlternateTitles = altTitles.ToArray();
                for (int i = 0; i < fmData.AlternateTitles.Length; i++)
                {
                    fmData.AlternateTitles[i] = fmData.AlternateTitles[i].Trim();
                }
            }
            else
            {
                _scanOptions.ScanTitle = false;
            }
        }

        #endregion

        #region Author

        if (_scanOptions.ScanAuthor || _scanOptions.ScanTags)
        {
            if (fmData.Author.IsEmpty())
            {
                List<string>? titles = !finalTitle.IsEmpty() ? new List<string> { finalTitle } : null;
                if (titles != null && altTitles.Count > 0)
                {
                    titles.AddRange(altTitles);
                }

                string author = GetValueFromReadme(SpecialLogic.Author, titles, SA_AuthorDetect);

                fmData.Author = CleanupValue(author).Trim();
            }

            if (!fmData.Author.IsEmpty())
            {
                // Remove email addresses from the end of author names
                Match match = AuthorEmailRegex.Match(fmData.Author);
                if (match.Success)
                {
                    fmData.Author = fmData.Author.Remove(match.Index, match.Length).Trim();
                }

                if (fmData.Author.StartsWithI("By "))
                {
                    fmData.Author = fmData.Author.Substring(2).Trim();
                }
            }
        }

        #endregion

#if FMScanner_FullCode
        #region Version

        if (_scanOptions.ScanVersion && fmData.Version.IsEmpty()) fmData.Version = GetVersion();

        #endregion
#endif

        // Again, I don't know enough about Thief 3 to know how to detect its languages
        if (!fmIsT3)
        {
            #region Languages

            if (
#if FMScanner_FullCode
                _scanOptions.ScanLanguages ||
#endif
                _scanOptions.ScanTags)
            {
                var getLangs = GetLanguages(baseDirFiles, booksDirFiles, intrfaceDirFiles, stringsDirFiles);
                fmData.Languages = getLangs.Langs.ToArray();
                if (getLangs.Langs.Count > 0) SetLangTags(fmData, getLangs.EnglishIsUncertain);
#if FMScanner_FullCode
                if (!_scanOptions.ScanLanguages) fmData.Languages = Array.Empty<string>();
#endif
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
                if (ai == -1) ai = fmData.Author.Length;
                string anonAuthor = fmData.Author.Substring(0, ai);
                if (anonAuthor.EqualsI("Anon") ||
                    anonAuthor.EqualsI("Withheld") ||
                    anonAuthor.EqualsI("Unknown") ||
                    anonAuthor.SimilarityTo("Anonymous", OrdinalIgnoreCase) > 0.75)
                {
                    SetMiscTag(fmData, "unknown author");
                }
            }

            if (!_scanOptions.ScanAuthor) fmData.Author = "";
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

#if DEBUG
        _overallTimer.Stop();
        Debug.WriteLine(@"This FM took:\r\n" + _overallTimer.Elapsed.ToString(@"hh\:mm\:ss\.fffffff"));
#endif

        return new ScannedFMDataAndError { ScannedFMData = fmData };
    }

#if Enable7zReadmeCacheCode
    private void CopySevenZipReadmesToCacheDir(FMToScan fm)
    {
        string cachePath = fm.CachePath;

        Directory.CreateDirectory(cachePath);

        var readmes = new List<(string Source, string Dest)>();

        foreach (string f in Directory.GetFiles(_fmWorkingPath, "*", SearchOption.TopDirectoryOnly))
        {
            if (f.IsValidReadme())
            {
                // If any HTML files found, give up - we don't have the facility to do a recursive
                // linked-file scan here at the moment
                if (f.ExtIsHtml()) return;

                readmes.Add((f, Path.Combine(cachePath, Path.GetFileName(f))));
            }
        }

        for (int i = 0; i < 2; i++)
        {
            string readmeDir = i == 0 ? FMDirs.T3FMExtras1S : FMDirs.T3FMExtras2S;
            string readmePathFull = Path.Combine(_fmWorkingPath, readmeDir);

            if (Directory.Exists(readmePathFull))
            {
                Directory.CreateDirectory(Path.Combine(cachePath, readmeDir));

                foreach (string f in Directory.GetFiles(readmePathFull, "*", SearchOption.TopDirectoryOnly))
                {
                    if (f.IsValidReadme())
                    {
                        // Ditto the above
                        if (f.ExtIsHtml()) return;

                        readmes.Add((f, Path.Combine(cachePath, readmeDir, Path.GetFileName(f))));
                    }
                }
            }
        }

        if (readmes.Count > 0)
        {
            try
            {
                foreach (var (source, dest) in readmes)
                {
                    File.Copy(source, dest, overwrite: true);
                }
            }
            catch (Exception ex)
            {
                Log("Exception copying files to cache during scan", ex);
                try
                {
                    DeleteDirectory(cachePath);
                }
                catch (Exception ex2)
                {
                    Log("Exception deleting cache path '" + cachePath + "' as part of exception handling for cache copy failure", ex2);
                }
            }
        }
    }
#endif

    #region Dates

    private readonly ref struct ParsedDateTime
    {
        private readonly bool _isAmbiguous;
        [MemberNotNullWhen(true, nameof(Date))]
        internal bool IsAmbiguous => Date != null && _isAmbiguous;
        internal readonly DateTime? Date;

        internal ParsedDateTime(DateTime? date, bool isAmbiguous)
        {
            _isAmbiguous = isAmbiguous;
            Date = date;
        }
    }

    /*
    TODO: Do some bullshit where we compare the readme date vs. the date in the readme
    So we can do some guesswork on whether the readme dates are near enough to correct or not

    If we wanted, we could also fuzzy-match the swapped day to within like 5 days each direction for example.
    */
    private DateTime? GetReleaseDate(List<NameAndIndex> usedMisFiles)
    {
        ParsedDateTime GetReadmeParsedDateTime()
        {
            // Look in the readme
            DateTime? topDT = GetReleaseDateFromTopOfReadmes(out bool topDtIsAmbiguous);

            // Search for updated dates FIRST, because they'll be the correct ones!
            string ds = GetValueFromReadme(SpecialLogic.None, null, SA_LatestUpdateDateDetect);
            DateTime? dt = null;
            bool dtIsAmbiguous = false;
            if (!ds.IsEmpty()) StringToDate(ds, checkForAmbiguity: true, out dt, out dtIsAmbiguous);

            if (ds.IsEmpty() || dt == null)
            {
                ds = GetValueFromReadme(SpecialLogic.None, null, SA_ReleaseDateDetect);
            }

            if (!ds.IsEmpty()) StringToDate(ds, checkForAmbiguity: true, out dt, out dtIsAmbiguous);

            if (topDT != null && dt != null)
            {
                return !topDtIsAmbiguous && dtIsAmbiguous
                    ? new ParsedDateTime(topDT, false)
                    : !dtIsAmbiguous && topDtIsAmbiguous
                        ? new ParsedDateTime(dt, false)
                        : DateTime.Compare((DateTime)topDT, (DateTime)dt) > 0
                            ? new ParsedDateTime(topDT, topDtIsAmbiguous)
                            : new ParsedDateTime(dt, dtIsAmbiguous);
            }
            else if (topDT != null)
            {
                return new ParsedDateTime(topDT, topDtIsAmbiguous);
            }
            else if (dt != null)
            {
                return new ParsedDateTime(dt, dtIsAmbiguous);
            }

            return new ParsedDateTime(null, false);
        }

        ParsedDateTime parsedDateTime = GetReadmeParsedDateTime();

        if (parsedDateTime.IsAmbiguous)
        {
            for (int i = 0; i < _readmeFiles.Count; i++)
            {
                DateTime readmeDate = _readmeFiles[i].LastModifiedDate;

                DateTime pdt = (DateTime)parsedDateTime.Date;

                if (readmeDate.Year > 1998 && readmeDate.Year == pdt.Year)
                {
                    if (readmeDate.Month == pdt.Month &&
                        readmeDate.Day == pdt.Day)
                    {
                        return pdt;
                    }
                    else if (readmeDate.Day == pdt.Month &&
                             readmeDate.Month == pdt.Day)
                    {
                        // Should just use the readme date for max perf I guess, but this keeps all other fields
                        // the same. BUG/TODO: These dates may get double-DateTimeOffsetUnixWhatever-converted...
                        // Thus bumping them by a few hours. Look into this at some point.
                        return new DateTime(
                            pdt.Year,
                            pdt.Day,
                            pdt.Month,
                            pdt.Hour,
                            pdt.Minute,
                            pdt.Second,
                            pdt.Millisecond
                        );
                    }
                }
            }
        }

        if (parsedDateTime.Date != null) return parsedDateTime.Date;

        // Look for the first readme file's last modified date
        if (_readmeFiles.Count > 0 && _readmeFiles[0].LastModifiedDate.Year > 1998)
        {
            return _readmeFiles[0].LastModifiedDate;
        }

        // Look for the first used .mis file's last modified date
        if (usedMisFiles.Count > 0)
        {
            DateTime misFileDate;
            if (_fmIsZip)
            {
                misFileDate = new DateTimeOffset(ZipHelpers.ZipTimeToDateTime(
                    _archive.Entries[usedMisFiles[0].Index].LastWriteTime)).DateTime;
            }
            else
            {
                if (_fmDirFileInfos.Count > 0)
                {
                    string fn = _fmIsSevenZip ? usedMisFiles[0].Name : _fmWorkingPath + usedMisFiles[0].Name;
                    // This loop is guaranteed to find something, because we will have quit early if we had no
                    // used .mis files.
                    FileInfoCustom? misFile = null;
                    for (int i = 0; i < _fmDirFileInfos.Count; i++)
                    {
                        FileInfoCustom f = _fmDirFileInfos[i];
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
                return misFileDate;
            }
        }

        // If we still don't have anything, give up: we've made a good-faith effort.
        return null;
    }

    private DateTime? GetReleaseDateFromTopOfReadmes(out bool isAmbiguous)
    {
        // Always false for now, because we only return dates that have month names in them currently
        // (was I concerned about number-only dates having not enough context to be sure they're dates?)
        isAmbiguous = false;

        if (_readmeFiles.Count == 0) return null;

        const int maxTopLines = 5;

        foreach (ReadmeInternal r in _readmeFiles)
        {
            if (!r.Scan) continue;

            var lines = new List<string>(maxTopLines);

            for (int i = 0; i < r.Lines.Count; i++)
            {
                string line = r.Lines[i];
                if (!line.IsWhiteSpace()) lines.Add(line);
                if (lines.Count == maxTopLines) break;
            }

            for (int i = 0; i < lines.Count; i++)
            {
                string lineT = lines[i].Trim();
                foreach (string item in _monthNamesEnglish)
                {
                    if (lineT.ContainsI(item) && StringToDate(lineT, checkForAmbiguity: false, out DateTime? result, out _))
                    {
                        return result;
                    }
                }
            }
        }

        return null;
    }

    // TODO(Scanner/StringToDate()): Shouldn't we ALWAYS check for ambiguity...?
    // TODO(Scanner/StringToDate()): We should consider identical month/day numbers non-ambiguous
    // (eg. 2/2/2000, and only if they're both between 1 and 12 of course)
    private bool StringToDate(string dateString, bool checkForAmbiguity, [NotNullWhen(true)] out DateTime? dateTime, out bool isAmbiguous)
    {
        // If a date has dot separators, it's probably European format, so we can up our accuracy with regard
        // to guessing about day/month order.
        if (Regex.Match(dateString, @"[0123456789]{1,2}\.[0123456789]{1,2}\.([0123456789]{4}|[0123456789]{2})").Success)
        {
            if (DateTime.TryParseExact(
                    dateString,
                    _dateFormatsEuropean,
                    DateTimeFormatInfo.InvariantInfo,
                    DateTimeStyles.None,
                    out DateTime eurDateResult))
            {
                dateTime = eurDateResult;
                isAmbiguous = true;
                return true;
            }
        }

        dateString = Regex.Replace(dateString, @"\s*,\s*", " ");
        dateString = Regex.Replace(dateString, @"\s+", " ");
        // Auldale Chess Tournament saying "March ~8, 2006"
        dateString = Regex.Replace(dateString, @"\s*~\s*", " ");
        dateString = Regex.Replace(dateString, @"\s+-\s+", "-");
        dateString = Regex.Replace(dateString, @"\s+/\s+", "/");
        dateString = Regex.Replace(dateString, @"\s+of\s+", " ");
        dateString = Regex.Replace(dateString, @"\s*\.\s*", "/");

        // Remove "st", "nd", "rd, "th" if present, as DateTime.TryParse() will choke on them
        Match match = DaySuffixesRegex.Match(dateString);
        if (match.Success)
        {
            Group suffix = match.Groups["Suffix"];
            dateString = dateString.Substring(0, suffix.Index) +
                         dateString.Substring(suffix.Index + suffix.Length);
        }

        // We pass specific date formats to ensure that no field will be inferred: if there's no year, we
        // want to fail, and not assume the current year.
        bool success = false;
        bool canBeAmbiguous = false;
        DateTime? result = null!;
        for (int i = 0; i < _dateFormats.Length; i++)
        {
            var item = _dateFormats[i];
            success = DateTime.TryParseExact(
                dateString,
                item.Format,
                DateTimeFormatInfo.InvariantInfo,
                DateTimeStyles.None,
                out DateTime result_);
            if (success)
            {
                canBeAmbiguous = item.CanBeAmbiguous;
                result = result_;
                break;
            }
        }

        if (!success)
        {
            isAmbiguous = false;
            dateTime = null;
            return false;
        }

        if (!checkForAmbiguity || !canBeAmbiguous)
        {
            isAmbiguous = false;
            dateTime = result;
            return true;
        }

        isAmbiguous = true;
        foreach (char c in dateString)
        {
            if (c.IsAsciiAlpha())
            {
                isAmbiguous = false;
                break;
            }
        }

        if (isAmbiguous)
        {
            string[] nums = dateString.Split(CA_DateSeparators, StringSplitOptions.RemoveEmptyEntries);
            if (nums.Length == 3)
            {
                bool unambiguousYearFound = false;
                bool unambiguousDayFound = false;

                for (int i = 0; i < nums.Length; i++)
                {
                    if (int.TryParse(nums[i], out int numInt))
                    {
                        switch (numInt)
                        {
                            case > 31 and <= 9999:
                                unambiguousYearFound = true;
                                break;
                            case > 12 and <= 31:
                                unambiguousDayFound = true;
                                break;
                        }
                    }
                }

                if (unambiguousYearFound && unambiguousDayFound)
                {
                    isAmbiguous = false;
                }
            }
        }

        dateTime = result;
        return true;
    }

    #endregion

    #region Set tags

    private void SetLangTags(ScannedFMData fmData, bool englishIsUncertain)
    {
        if (fmData.Languages.Length == 0) return;

        if (fmData.TagsString.IsWhiteSpace()) fmData.TagsString = "";
        for (int i = 0; i < fmData.Languages.Length; i++)
        {
            string lang = fmData.Languages[i];

            // This all depends on langs being lowercase!
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
    }

    private static void SetMiscTag(ScannedFMData fmData, string tag)
    {
        if (fmData.TagsString.IsWhiteSpace()) fmData.TagsString = "";

        List<string> list = fmData.TagsString.Split(CA_CommaSemicolon).ToList();
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

    private bool ReadAndCacheFMData(
        string fmPath,
        ScannedFMData fmd,
        List<NameAndIndex> baseDirFiles,
        List<NameAndIndex> misFiles,
        List<NameAndIndex> usedMisFiles,
        List<NameAndIndex> stringsDirFiles,
        List<NameAndIndex> intrfaceDirFiles,
        List<NameAndIndex> booksDirFiles,
        List<NameAndIndex> t3FMExtrasDirFiles)
    {
        #region Add BaseDirFiles

        bool t3Found = false;
        var t3GmpFiles = new List<NameAndIndex>();

        static bool MapFileExists(string path)
        {
            int lsi;
            return path.PathStartsWithI(FMDirs.IntrfaceS) &&
                   path.Rel_DirSepCountIsAtLeast(1, FMDirs.IntrfaceSLen) &&
                   path.Length > (lsi = path.Rel_LastIndexOfDirSep()) + 5 &&
                   (path[lsi + 1] == 'p' || path[lsi + 1] == 'P') &&
                   (path[lsi + 2] == 'a' || path[lsi + 2] == 'A') &&
                   (path[lsi + 3] == 'g' || path[lsi + 3] == 'G') &&
                   (path[lsi + 4] == 'e' || path[lsi + 4] == 'E') &&
                   (path[lsi + 5] == '0') &&
                   path.LastIndexOf('.') > lsi;
        }

        static bool AutomapFileExists(string path)
        {
            int len = path.Length;
            return path.PathStartsWithI(FMDirs.IntrfaceS) &&
                   path.Rel_DirSepCountIsAtLeast(1, FMDirs.IntrfaceSLen) &&
                   // We don't need to check the length because we only need length == 6 but by virtue of
                   // starting with "intrface/", our length is guaranteed to be at least 9
                   (path[len - 6] == 'r' || path[len - 6] == 'R') &&
                   (path[len - 5] == 'a' || path[len - 5] == 'A') &&
                   path[len - 4] == '.' &&
                   (path[len - 3] == 'b' || path[len - 3] == 'B') &&
                   (path[len - 2] == 'i' || path[len - 2] == 'I') &&
                   (path[len - 1] == 'n' || path[len - 1] == 'N');
        }

        static bool FileExtensionFound(string fn, string[] array)
        {
            for (int i = 0; i < array.Length; i++)
            {
                if (fn.EndsWithI(array[i]))
                {
                    return true;
                }
            }
            return false;
        }

        static bool BaseDirScriptFileExtensions(List<NameAndIndex> baseDirFiles, string[] scriptFileExtensions)
        {
            for (int i = 0; i < baseDirFiles.Count; i++)
            {
                if (scriptFileExtensions.ContainsI(Path.GetExtension(baseDirFiles[i].Name)))
                {
                    return true;
                }
            }
            return false;
        }

        if (_fmIsZip || _fmDirFileInfos.Count > 0)
        {
            int filesCount = _fmIsZip ? _archive.Entries.Count : _fmDirFileInfos.Count;
            for (int i = 0; i < filesCount; i++)
            {
                string fn = _fmIsZip
                    ? _archive.Entries[i].FullName
                    : _fmIsSevenZip
                        ? _fmDirFileInfos[i].FullName
                        : _fmDirFileInfos[i].FullName.Substring(_fmWorkingPath.Length);

#if DEBUG_RANDOMIZE_DIR_SEPS

                fn = Debug_RandomizeDirSeps(fn);

#endif

                int index = _fmIsZip ? i : -1;

                if (fn.PathStartsWithI(FMDirs.T3DetectS) &&
                    fn.Rel_CountDirSeps(FMDirs.T3DetectSLen) == 0)
                {
                    if (t3Found)
                    {
                        if (fn.ExtIsGmp())
                        {
                            if (_scanOptions.ScanMissionCount)
                            {
                                // We only want the filename; we already know it's in the right folder
                                t3GmpFiles.Add(new NameAndIndex(Path.GetFileName(fn), index));
                            }
                            continue;
                        }
                    }
                    else
                    {
                        if (fn.ExtIsIbt() ||
                            fn.ExtIsCbt() ||
                            fn.ExtIsNed() ||
                            fn.ExtIsUnr())
                        {
                            fmd.Game = Game.Thief3;
                            t3Found = true;
                            continue;
                        }
                        else if (fn.ExtIsGmp())
                        {
                            fmd.Game = Game.Thief3;
                            t3Found = true;
                            if (_scanOptions.ScanMissionCount)
                            {
                                // We only want the filename; we already know it's in the right folder
                                t3GmpFiles.Add(new NameAndIndex(Path.GetFileName(fn), index));
                            }
                            continue;
                        }
                    }
                }
                // We can't early-out if !t3Found here because if we find it after this point, we'll be
                // missing however many of these we skipped before we detected Thief 3
                else if (fn.PathStartsWithI(FMDirs.T3FMExtras1S) ||
                         fn.PathStartsWithI(FMDirs.T3FMExtras2S))
                {
                    t3FMExtrasDirFiles.Add(new NameAndIndex(fn, index));
                    continue;
                }
                else if (!fn.Rel_ContainsDirSep() && fn.Contains('.'))
                {
                    baseDirFiles.Add(new NameAndIndex(fn, index));
                    // Fallthrough so ScanCustomResources can use it
                }
                else if (!t3Found && fn.PathStartsWithI(FMDirs.StringsS))
                {
                    stringsDirFiles.Add(new NameAndIndex(fn, index));
                    if (SS2FingerprintRequiredAndNotDone() &&
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
                    intrfaceDirFiles.Add(new NameAndIndex(fn, index));
                    // Fallthrough so ScanCustomResources can use it
                }
                else if (!t3Found && fn.PathStartsWithI(FMDirs.BooksS))
                {
                    booksDirFiles.Add(new NameAndIndex(fn, index));
                    continue;
                }
                else if (!t3Found && SS2FingerprintRequiredAndNotDone() &&
                         (fn.PathStartsWithI(FMDirs.CutscenesS) ||
                          fn.PathStartsWithI(FMDirs.Snd2S)))
                {
                    _ss2Fingerprinted = true;
                    // Fallthrough so ScanCustomResources can use it
                }

                // Inlined for performance. We cut the time roughly in half by doing this.
                if (!t3Found && _scanOptions.ScanCustomResources)
                {
                    if (fmd.HasAutomap == null && AutomapFileExists(fn))
                    {
                        fmd.HasAutomap = true;
                    }
                    else if (fmd.HasMap == null && MapFileExists(fn))
                    {
                        fmd.HasMap = true;
                    }
                    else if (fmd.HasCustomMotions == null &&
                             fn.PathStartsWithI(FMDirs.MotionsS) &&
                             FileExtensionFound(fn, MotionFileExtensions))
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
                             FileExtensionFound(fn, ImageFileExtensions))
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
                    else if ((fmd.HasCustomScripts == null &&
                              !fn.Rel_ContainsDirSep() &&
                              FileExtensionFound(fn, ScriptFileExtensions)) ||
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
                if (baseDirFiles.Count == 0)
                {
                    Log(fmPath + ": 'fm is zip' or 'scanning size' codepath: No files in base dir. Returning false.", stackTrace: false);
                    return false;
                }

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
        else // Dir only; 7z is now handled up there as well
        {
            string t3DetectPath = Path.Combine(_fmWorkingPath, FMDirs.T3DetectS);
            if (Directory.Exists(t3DetectPath) &&
                FastIO.FilesExistSearchTop(t3DetectPath, SA_T3DetectExtensions))
            {
                t3Found = true;
                if (_scanOptions.ScanMissionCount)
                {
                    foreach (string f in EnumFiles(FMDirs.T3DetectS, SearchOption.TopDirectoryOnly))
                    {
                        if (f.ExtIsGmp())
                        {
                            // We only want the filename; we already know it's in the right folder
                            t3GmpFiles.Add(new NameAndIndex(Path.GetFileName(f)));
                        }
                    }
                }
                fmd.Game = Game.Thief3;
            }

            foreach (string f in EnumFiles(SearchOption.TopDirectoryOnly))
            {
                baseDirFiles.Add(new NameAndIndex(Path.GetFileName(f)));
            }

            if (t3Found)
            {
                foreach (string f in EnumFiles(FMDirs.T3FMExtras1S, SearchOption.TopDirectoryOnly))
                {
                    t3FMExtrasDirFiles.Add(new NameAndIndex(f.Substring(_fmWorkingPath.Length)));
                }

                foreach (string f in EnumFiles(FMDirs.T3FMExtras2S, SearchOption.TopDirectoryOnly))
                {
                    t3FMExtrasDirFiles.Add(new NameAndIndex(f.Substring(_fmWorkingPath.Length)));
                }
            }
            else
            {
                if (baseDirFiles.Count == 0)
                {
                    Log(fmPath + ": 'fm is dir' codepath: No files in base dir. Returning false.", stackTrace: false);
                    return false;
                }

                foreach (string f in EnumFiles(FMDirs.StringsS, SearchOption.AllDirectories))
                {
                    stringsDirFiles.Add(new NameAndIndex(f.Substring(_fmWorkingPath.Length)));
                    if (SS2FingerprintRequiredAndNotDone() &&
                        (f.PathEndsWithI(FMFiles.SS2Fingerprint1) ||
                         f.PathEndsWithI(FMFiles.SS2Fingerprint2) ||
                         f.PathEndsWithI(FMFiles.SS2Fingerprint3) ||
                         f.PathEndsWithI(FMFiles.SS2Fingerprint4)))
                    {
                        _ss2Fingerprinted = true;
                    }
                }

                foreach (string f in EnumFiles(FMDirs.IntrfaceS, SearchOption.AllDirectories))
                {
                    intrfaceDirFiles.Add(new NameAndIndex(f.Substring(_fmWorkingPath.Length)));
                }

                foreach (string f in EnumFiles(FMDirs.BooksS, SearchOption.AllDirectories))
                {
                    booksDirFiles.Add(new NameAndIndex(f.Substring(_fmWorkingPath.Length)));
                }

                if (SS2FingerprintRequiredAndNotDone() || _scanOptions.ScanCustomResources)
                {
                    // I tried getting rid of this GetDirectories call, but it made things more complicated
                    // for SS2 fingerprinting and didn't result in a clear perf win. At least not warm. Meh.
                    var baseDirFolders = new List<string>();
                    foreach (string dir in Directory.GetDirectories(_fmWorkingPath, "*", SearchOption.TopDirectoryOnly))
                    {
                        // @DIRSEP: Even for UNC paths, FM working path has to be at least like \\netPC\some_directory
                        // and we're getting dirs inside that, so it'll be at least \\netPC\some_directory\other
                        // so we'll always end up with "other" (for example). So we're safe here.
                        baseDirFolders.Add(dir.Substring(dir.Rel_LastIndexOfDirSep() + 1));
                    }

                    if (_scanOptions.ScanCustomResources)
                    {
                        foreach (NameAndIndex f in intrfaceDirFiles)
                        {
                            if (fmd.HasAutomap == null && AutomapFileExists(f.Name))
                            {
                                fmd.HasAutomap = true;
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
                            BaseDirScriptFileExtensions(baseDirFiles, ScriptFileExtensions) ||
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

                    if (SS2FingerprintRequiredAndNotDone() &&
                        (baseDirFolders.ContainsI(FMDirs.Cutscenes) ||
                         baseDirFolders.ContainsI(FMDirs.Snd2)))
                    {
                        _ss2Fingerprinted = true;
                    }
                }
            }
        }

        if (t3Found)
        {
            if (_scanOptions.ScanMissionCount)
            {
                switch (t3GmpFiles.Count)
                {
                    case 1:
                        usedMisFiles.Add(t3GmpFiles[0]);
                        break;
                    case > 1:
                        for (int i = 0; i < t3GmpFiles.Count; i++)
                        {
                            NameAndIndex item = t3GmpFiles[i];
                            if (!item.Name.EqualsI(FMFiles.EntryGmp))
                            {
                                usedMisFiles.Add(item);
                            }
                        }
                        break;
                }
            }

            // Cut it right here for Thief 3: we don't need anything else
            return true;
        }

        #endregion

        #region Add MisFiles and check for none

        for (int i = 0; i < baseDirFiles.Count; i++)
        {
            NameAndIndex f = baseDirFiles[i];
            if (f.Name.ExtIsMis())
            {
                misFiles.Add(new NameAndIndex(Path.GetFileName(f.Name), f.Index));
            }
        }

        if (misFiles.Count == 0)
        {
            Log(fmPath + ": No .mis files in base dir. Returning false.", stackTrace: false);
            return false;
        }

        #endregion

        #region Cache list of used .mis files

        NameAndIndex? missFlag = null;
        if (stringsDirFiles.Count > 0)
        {
            // I don't remember if I need to search in this exact order, so uh... not rockin' the boat.
            for (int i = 0; i < stringsDirFiles.Count; i++)
            {
                NameAndIndex item = stringsDirFiles[i];
                if (item.Name.PathEqualsI(FMFiles.StringsMissFlag))
                {
                    missFlag = item;
                    break;
                }
            }
            if (missFlag == null)
            {
                for (int i = 0; i < stringsDirFiles.Count; i++)
                {
                    NameAndIndex item = stringsDirFiles[i];
                    if (item.Name.PathEqualsI(FMFiles.StringsEnglishMissFlag))
                    {
                        missFlag = item;
                        break;
                    }
                }
            }
            if (missFlag == null)
            {
                for (int i = 0; i < stringsDirFiles.Count; i++)
                {
                    NameAndIndex item = stringsDirFiles[i];
                    if (item.Name.PathEndsWithI(FMFiles.SMissFlag))
                    {
                        missFlag = item;
                        break;
                    }
                }
            }
        }

        if (missFlag != null)
        {
            List<string> mfLines;

            // missflag.str files are always ASCII / UTF8, so we can avoid an expensive encoding detect here
            if (_fmIsZip)
            {
                using var es = _archive.OpenEntry(_archive.Entries[missFlag.Index]);
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
        string title = "";
        string author = "";
        string version = "";
        DateTime? releaseDate = null;

        var fmInfoXml = new XmlDocument();

        #region Load XML

        if (_fmIsZip)
        {
            using var es = _archive.OpenEntry(_archive.Entries[file.Index]);
            fmInfoXml.Load(es);
        }
        else
        {
            fmInfoXml.Load(Path.Combine(_fmWorkingPath, file.Name));
        }

        #endregion

        if (_scanOptions.ScanTitle)
        {
            XmlNodeList xTitle = fmInfoXml.GetElementsByTagName("title");
            if (xTitle.Count > 0) title = xTitle[0]?.InnerText ?? "";
        }

        if (_scanOptions.ScanTags || _scanOptions.ScanAuthor)
        {
            XmlNodeList xAuthor = fmInfoXml.GetElementsByTagName("author");
            if (xAuthor.Count > 0) author = xAuthor[0]?.InnerText ?? "";
        }

#if FMScanner_FullCode
        if (_scanOptions.ScanVersion)
        {
            XmlNodeList xVersion = fmInfoXml.GetElementsByTagName("version");
            if (xVersion.Count > 0) version = xVersion[0]?.InnerText ?? "";
        }
#endif

        XmlNodeList xReleaseDate = fmInfoXml.GetElementsByTagName("releasedate");
        if (xReleaseDate.Count > 0)
        {
            string rdString = xReleaseDate[0]?.InnerText ?? "";
            if (!rdString.IsEmpty()) releaseDate = StringToDate(rdString, checkForAmbiguity: false, out DateTime? dt, out _) ? dt : null;
        }

        // These files also specify languages and whether the mission has custom stuff, but we're not going
        // to trust what we're told - we're going to detect that stuff by looking at what's actually there.

        return (title, author, version, releaseDate);
    }

    private (string Title, string Author, string Description, DateTime? LastUpdateDate, string Tags)
    ReadFMIni(NameAndIndex file)
    {
        var ret = (
            Title: "",
            Author: "",
            Description: "",
            LastUpdateDate: (DateTime?)null,
            Tags: ""
        );

        #region Load INI

        List<string> iniLines;

        if (_fmIsZip)
        {
            ZipArchiveSEntry e = _archive.Entries[file.Index];
            using var es = _archive.OpenEntry(e);
            iniLines = ReadAllLinesE(es, e.Length);
        }
        else
        {
            iniLines = ReadAllLinesE(Path.Combine(_fmWorkingPath, file.Name));
        }

        if (iniLines.Count == 0) return ("", "", "", null, "");

        (string NiceName, string ReleaseDate, string Tags, string Descr) fmIni = ("", "", "", "");

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
#if FMScanner_FullCode
                if (_scanOptions.ScanDescription) fmIni.Descr = line.Substring(6).Trim();
#endif
            }
            else if (inDescr)
            {
#if FMScanner_FullCode
                if (_scanOptions.ScanDescription) fmIni.Descr += "\r\n" + line;
#endif
            }
        }

#if FMScanner_FullCode
        if (_scanOptions.ScanDescription && !fmIni.Descr.IsEmpty()) fmIni.Descr = fmIni.Descr.Trim();
#endif

        #endregion

        #endregion

#if FMScanner_FullCode
        #region Fixup description

        // Descr can be multiline, and you're supposed to use \n for linebreaks, but sometimes this value
        // is multiple actual lines. Despite this being a horrific violation of the .ini format, we still
        // have to handle it.

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
                CountChars(fmIni.Descr, '\"') == 2)
            {
                fmIni.Descr = fmIni.Descr.Trim(CA_DoubleQuote);
            }
            if (fmIni.Descr[0] == LeftDoubleQuote && fmIni.Descr[fmIni.Descr.Length - 1] == RightDoubleQuote &&
                CountChars(fmIni.Descr, LeftDoubleQuote) + CountChars(fmIni.Descr, RightDoubleQuote) == 2)
            {
                fmIni.Descr = fmIni.Descr.Trim(CA_UnicodeQuotes);
            }

            fmIni.Descr = fmIni.Descr.RemoveUnpairedLeadingOrTrailingQuotes();

            // Normalize to just LF for now. Otherwise it just doesn't work right for reasons confusing and
            // senseless. It can easily be converted later.
            fmIni.Descr = fmIni.Descr.Replace("\r\n", "\n");
            if (fmIni.Descr.IsWhiteSpace()) fmIni.Descr = "";
        }

        #endregion
#endif

        #region Get author from tags

        if ((_scanOptions.ScanTags || _scanOptions.ScanAuthor) && !fmIni.Tags.IsEmpty())
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
                if (i > 0 && !authorString.EndsWithO(", ")) authorString += ", ";
                authorString += a.Substring(a.IndexOf(':') + 1).Trim();
            }

            ret.Author = authorString;
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
            // Anyway, we should parse it as long.
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
                ret.LastUpdateDate = StringToDate(fmIni.ReleaseDate, checkForAmbiguity: false, out DateTime? dt, out _) ? dt : null;
            }
        }

#if FMScanner_FullCode
        if (_scanOptions.ScanDescription) ret.Description = fmIni.Descr;
#endif

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
        var ret = (Title: "", Author: "");

        List<string> lines;

        if (_fmIsZip)
        {
            ZipArchiveSEntry e = _archive.Entries[file.Index];
            using var es = _archive.OpenEntry(e);
            lines = ReadAllLinesE(es, e.Length);
        }
        else
        {
            lines = ReadAllLinesE(Path.Combine(_fmWorkingPath, file.Name));
        }

        if (lines.Count == 0) return ret;

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

    private void ReadAndCacheReadmeFiles(List<NameAndIndex> readmeDirFiles)
    {
        // Note: .wri files look like they may be just plain text with garbage at the top. Shrug.
        // Treat 'em like plaintext and see how it goes.

        foreach (NameAndIndex readmeFile in readmeDirFiles)
        {
            if (!readmeFile.Name.IsValidReadme()) continue;

            ZipArchiveSEntry? readmeEntry = null;

            if (_fmIsZip) readmeEntry = _archive.Entries[readmeFile.Index];

            string? fullReadmeFileName = null;
            FileInfoCustom? readmeFI = null;
            if (!_fmIsZip && _fmDirFileInfos.Count > 0)
            {
                for (int i = 0; i < _fmDirFileInfos.Count; i++)
                {
                    FileInfoCustom f = _fmDirFileInfos[i];
                    if (f.FullName.PathEqualsI(fullReadmeFileName ??= Path.Combine(_fmWorkingPath, readmeFile.Name)))
                    {
                        readmeFI = f;
                        break;
                    }
                }
            }

            int readmeFileLen =
                _fmIsZip ? (int)readmeEntry!.Length :
                readmeFI != null ? (int)readmeFI.Length :
                (int)new FileInfo(fullReadmeFileName ??= Path.Combine(_fmWorkingPath, readmeFile.Name)).Length;

            string readmeFileOnDisk = "";

            bool isGlml;
            DateTime? lastModifiedDate = null;
            long readmeSize;

            if (_fmIsZip)
            {
                isGlml = readmeEntry!.FullName.ExtIsGlml();
                readmeSize = readmeEntry.Length;
            }
            else
            {
                readmeFileOnDisk = fullReadmeFileName ?? Path.Combine(_fmWorkingPath, readmeFile.Name);
                FileInfoCustom fi = readmeFI ?? new FileInfoCustom(new FileInfo(readmeFileOnDisk));
                isGlml = fi.FullName.ExtIsGlml();
                lastModifiedDate = new DateTimeOffset(fi.LastWriteTime).DateTime;
                readmeSize = fi.Length;
            }

            if (readmeSize == 0) continue;

            bool scanThisReadme = !readmeFile.Name.ExtIsHtml() && readmeFile.Name.IsEnglishReadme();

            // We still add the readme even if we're not going to store nor scan its contents, because we
            // still may need to look at its last modified date.
            if (_fmIsZip)
            {
                _readmeFiles.Add(new ReadmeInternal(
                    isGlml: isGlml,
                    lastModifiedDateRaw: readmeEntry!.LastWriteTime,
                    scan: scanThisReadme
                ));
            }
            else
            {
                _readmeFiles.Add(new ReadmeInternal(
                    isGlml: isGlml,
                    lastModifiedDate: (DateTime)lastModifiedDate!,
                    scan: scanThisReadme
                ));
            }

            if (!scanThisReadme) continue;

            // try-finally instead of using, because we only want to initialize the readme stream if _fmIsZip
            Stream? readmeStream = null;
            try
            {
                if (_fmIsZip)
                {
                    /*
                    We used to copy the entire stream into memory here first, because we needed to seek.
                    With the new custom RTF converter, we don't need to seek anymore.

                    With the new converter, copying the stream to memory first results in the fastest
                    performance, but slightly more memory use than the old RichTextBox method.

                    We've instead chosen to go with the buffered read here, which is slightly slower - but
                    still vastly faster than the old RichTextBox-based converter - and saves a substantial
                    amount of memory. Any other time I would choose ultimate speed, but RTF files can be
                    extremely large (due to often containing images), so I'm erring on the side of caution.
                    */
                    readmeStream = _archive.OpenEntry(readmeEntry!);
                }

                _rtfHeaderBuffer.Clear();

                // Saw one ".rtf" that was actually a plaintext file, and one vice versa. So detect by header
                // alone.
                Stream? readmeHeaderStream = null;
                try
                {
                    readmeHeaderStream = _fmIsZip
                        ? readmeStream!
                        : GetFileStreamFast(readmeFileOnDisk, DiskFileStreamBuffer);

                    // stupid micro-optimization
                    const int rtfHeaderBytesLength = 6;

                    if (readmeFileLen >= rtfHeaderBytesLength)
                    {
                        readmeHeaderStream.ReadAll(_rtfHeaderBuffer, 0, rtfHeaderBytesLength);
                    }
                }
                finally
                {
                    if (!_fmIsZip)
                    {
                        readmeHeaderStream?.Dispose();
                    }
                }

                ReadmeInternal last = _readmeFiles[_readmeFiles.Count - 1];

                // file is rtf
                if (_rtfHeaderBuffer.SequenceEqual(RTFHeaderBytes))
                {
                    bool success;
                    string text;
                    if (_fmIsZip)
                    {
                        readmeStream?.Dispose();
                        readmeStream = _archive.OpenEntry(readmeEntry!);
                        (success, text) = RtfConverter.Convert(readmeStream, readmeFileLen);
                    }
                    else
                    {
                        using var fs = GetFileStreamFast(readmeFileOnDisk, DiskFileStreamBuffer);
                        (success, text) = RtfConverter.Convert(fs, readmeFileLen);
                    }

                    if (success)
                    {
                        last.Text = text;
                        last.Lines.ClearAndAdd(text.Split(CRLF_CR_LF, StringSplitOptions.None));
                    }
                }
                else // file is plain text
                {
                    if (_fmIsZip)
                    {
                        // Plain text, so load the whole thing in one go
                        readmeStream?.Dispose();
                        readmeStream = new MemoryStream(readmeFileLen);
                        using var es = _archive.OpenEntry(readmeEntry!);
                        StreamCopyNoAlloc(es, readmeStream);
                    }

                    last.Text = _fmIsZip
                        ? ReadAllTextE(readmeStream!)
                        : ReadAllTextE(readmeFileOnDisk);

                    if (last.IsGlml) last.Text = Utility.GLMLToText(last.Text);

                    last.Lines.ClearAndAdd(last.Text.Split(CRLF_CR_LF, StringSplitOptions.None));
                }
            }
            finally
            {
                readmeStream?.Dispose();
            }
        }
    }

    private string GetValueFromReadme(SpecialLogic specialLogic, List<string>? titles = null, params string[] keys)
    {
        string ret = "";

        foreach (ReadmeInternal file in _readmeFiles)
        {
            if (!file.Scan) continue;

#if FMScanner_FullCode
            if (specialLogic == SpecialLogic.NewDarkMinimumVersion)
            {
                string ndv = GetNewDarkVersionFromText(file.Text);
                if (!ndv.IsEmpty()) return ndv;
            }
            else
#endif
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
                        // @PERF_TODO: We can move things around for perf here.
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
            static string GetAuthorNextLine(List<string> lines)
            {
                for (int i = 0; i < lines.Count; i++)
                {
                    string lineT = lines[i].Trim();
                    if (!lineT.EqualsI("Author") && !lineT.EqualsI("Author:")) continue;

                    if (i < lines.Count - 2)
                    {
                        string lineAfterNext = lines[i + 2].Trim();
                        int lanLen = lineAfterNext.Length;
                        if ((lanLen > 0 && lineAfterNext[lanLen - 1] == ':' && lanLen <= 50) ||
                            lineAfterNext.IsWhiteSpace())
                        {
                            return lines[i + 1].Trim();
                        }
                    }
                }

                return "";
            }

            foreach (ReadmeInternal file in _readmeFiles)
            {
                if (!file.Scan) continue;

                ret = GetAuthorNextLine(file.Lines);

                if (!ret.IsEmpty()) return ret;
            }

            // @PERF_TODO(Scanner/GetValueFromReadme/GetAuthorFromTitleByAuthorLine() call section):
            // This is last because it used to have a dynamic and constantly re-instantiated regex in it. I
            // don't even know why I thought I needed that but it turns out I could just make it static like
            // the rest, so I did. Re-evaluate this and maybe put it higher?
            // Anything that can go before the full-text search probably should, because that's clearly the
            // slowest by far.
            ret = GetAuthorFromTitleByAuthorLine(titles);
        }

        return ret;
    }

    private
#if !FMScanner_FullCode
    static
#endif
    string GetValueFromLines(SpecialLogic specialLogic, string[] keys, List<string> lines)
    {
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
#if FMScanner_FullCode
                case SpecialLogic.Version when
                    lineStartTrimmed.StartsWithI("Version History") ||
                    lineStartTrimmed.ContainsI("NewDark") ||
                    lineStartTrimmed.ContainsI("64 Cubed") ||
                    VersionExclude1Regex.Match(lineStartTrimmed).Success:
#endif
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
                    if (Regex.Match(lineStartTrimmed, "^" + x + @"\s*(:|-|\u2013)",
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
#if FMScanner_FullCode
                // Don't detect "Version "; too many false positives
                // TODO: Can probably remove this check and then just sort out any false positives in GetVersion()
                if (specialLogic == SpecialLogic.Version) continue;
#endif

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

        return "";
    }

    private string CleanupValue(string value)
    {
        if (value.IsEmpty()) return value;

        string ret = value.TrimEnd();

        // Remove surrounding quotes
        if (ret[0] == '\"' && ret[ret.Length - 1] == '\"') ret = ret.Trim(CA_DoubleQuote);
        if ((ret[0] == LeftDoubleQuote || ret[0] == RightDoubleQuote) &&
            (ret[ret.Length - 1] == LeftDoubleQuote || ret[ret.Length - 1] == RightDoubleQuote))
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
        if (containsOpenParen && ret.CountCharsUpToAmount('(', 2) == 1 && !containsCloseParen)
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
    private List<string>? GetTitlesFromTopOfReadmes(List<ReadmeInternal> readmes)
    {
        if (_readmeFiles.Count == 0) return null;

        List<string>? ret = null;

        const int maxTopLines = 5;

        foreach (ReadmeInternal r in readmes)
        {
            if (!r.Scan) continue;

            var lines = new List<string>(maxTopLines);

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
                    (lineT.StartsWithI("By ") || lineT.StartsWithI("By: ") ||
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
                     lineT.StartsWithI("An SS2")))
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
                        ret ??= new List<string>();
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
        if (intrfaceDirFiles.Count == 0) return "";

        NameAndIndex? newGameStrFile =
            intrfaceDirFiles.Find(static x =>
                x.Name.PathEqualsI(FMFiles.IntrfaceEnglishNewGameStr))
            ?? intrfaceDirFiles.Find(static x =>
                x.Name.PathEqualsI(FMFiles.IntrfaceNewGameStr))
            ?? intrfaceDirFiles.Find(static x =>
                x.Name.PathStartsWithI(FMDirs.IntrfaceS) &&
                x.Name.PathEndsWithI(FMFiles.SNewGameStr));

        if (newGameStrFile == null) return "";

        List<string> lines;

        if (_fmIsZip)
        {
            ZipArchiveSEntry e = _archive.Entries[newGameStrFile.Index];
            using var es = _archive.OpenEntry(e);
            lines = ReadAllLinesE(es, e.Length);
        }
        else
        {
            lines = ReadAllLinesE(Path.Combine(_fmWorkingPath, newGameStrFile.Name));
        }

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

        return "";
    }

    private (string TitleFrom0, string TitleFromN, List<string>? CampaignMissionNames)
    GetMissionNames(List<NameAndIndex> stringsDirFiles, List<NameAndIndex> misFiles, List<NameAndIndex> usedMisFiles)
    {
        List<string>? titlesStrLines = GetTitlesStrLines(stringsDirFiles);
        if (titlesStrLines == null || titlesStrLines.Count == 0) return ("", "", null);

        var ret =
            (TitleFrom0: "",
                TitleFromN: "",
                CampaignMissionNames: (List<string>?)null);

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
            string titleNum = "";
            string title = "";
            for (int umfIndex = 0; umfIndex < usedMisFiles.Count; umfIndex++)
            {
                string line = titlesStrLines[lineIndex];
                int i;
                titleNum = line.Substring(i = line.IndexOf('_') + 1, line.IndexOf(':') - i).Trim();

                title = ExtractFromQuotedSection(line);
                if (title.IsEmpty()) continue;

                if (titleNum == "0") ret.TitleFrom0 = title;

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
#if FMScanner_FullCode
                if (!_scanOptions.ScanCampaignMissionNames)
#endif
                {
                    break;
                }
            }
        }

        if (titles.Count > 0)
        {
            if (_scanOptions.ScanTitle && titles.Count == 1)
            {
                ret.TitleFromN = titles[0];
            }
#if FMScanner_FullCode
            else if (_scanOptions.ScanCampaignMissionNames)
            {
                ret.CampaignMissionNames = titles;
            }
#endif
        }

        return ret;
    }

    private List<string>? GetTitlesStrLines(List<NameAndIndex> stringsDirFiles)
    {
        List<string>? titlesStrLines = null;

        #region Read title(s).str file

        foreach (string titlesFileLocation in FMFiles_TitlesStrLocations)
        {
            NameAndIndex? titlesFile = null;
            if (_fmIsZip)
            {
                for (int i = 0; i < stringsDirFiles.Count; i++)
                {
                    var item = stringsDirFiles[i];
                    if (item.Name.PathEqualsI(titlesFileLocation))
                    {
                        titlesFile = item;
                        break;
                    }
                }
            }
            else
            {
                titlesFile = new NameAndIndex(Path.Combine(_fmWorkingPath, titlesFileLocation));
            }

            if (titlesFile == null || (!_fmIsZip && !File.Exists(titlesFile.Name))) continue;

            if (_fmIsZip)
            {
                ZipArchiveSEntry e = _archive.Entries[titlesFile.Index];
                using var es = _archive.OpenEntry(e);
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

        static bool TFLinesDAny(string line, int indexOfColon, List<string> tfLinesD)
        {
            for (int i = 0; i < tfLinesD.Count; i++)
            {
                if (tfLinesD[i].StartsWithI(line.Substring(0, indexOfColon)))
                {
                    return true;
                }
            }
            return false;
        }

        for (int i = 0; i < titlesStrLines.Count; i++)
        {
            int indexOfColon;
            // Note: the Trim() is important, don't remove it
            string line = titlesStrLines[i].Trim();
            if (!line.IsEmpty() &&
                line.StartsWithI("title_") &&
                (indexOfColon = line.IndexOf(':')) > -1 &&
                line.CharCountIsAtLeast('\"', 2) &&
                !TFLinesDAny(line, indexOfColon, tfLinesD))
            {
                tfLinesD.Add(line);
            }
        }

        tfLinesD.Sort(_titlesStrNaturalNumericSort);

        #endregion

        return tfLinesD;
    }

    private string CleanupTitle(string value)
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

        return CleanupValue(value).Trim();
    }

    #endregion

    #region Author

    private string GetAuthorFromTopOfReadme(List<string> lines, List<string>? titles)
    {
        if (lines.Count == 0) return "";

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

        if (topLines.Count < 2) return "";

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

        return "";
    }

    private string GetAuthorFromText(string text)
    {
        for (int i = 0; i < AuthorRegexes.Length; i++)
        {
            Match match = AuthorRegexes[i].Match(text);
            if (match.Success) return match.Groups["Author"].Value;
        }

        return "";
    }

    private string GetAuthorFromTitleByAuthorLine(List<string>? titles)
    {
        if (titles == null || titles.Count == 0) return "";

        // With the new fuzzy match method, it might be possible for me to remove the need for this guard
        for (int i = 0; i < titles.Count; i++)
        {
            if (titles[i].ContainsI(" by "))
            {
                titles.RemoveAt(i);
                i--;
            }
        }

        if (titles.Count == 0) return "";

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

        return "";
    }

    private string GetAuthorFromCopyrightMessage()
    {
        static string AuthorCopyrightRegexesMatch(string line, Regex[] authorMissionCopyrightRegexes)
        {
            for (int i = 0; i < authorMissionCopyrightRegexes.Length; i++)
            {
                Match match = authorMissionCopyrightRegexes[i].Match(line);
                if (match.Success) return match.Groups["Author"].Value;
            }
            return "";
        }

        string author = "";

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

                author = AuthorCopyrightRegexesMatch(line, AuthorMissionCopyrightRegexes);
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

        return author.IsWhiteSpace() ? "" : CleanupCopyrightAuthor(author);
    }

    private string CleanupCopyrightAuthor(string author)
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

        return author.TrimEnd(CA_Period).Trim();
    }

    #endregion

#if FMScanner_FullCode
    private string GetVersion()
    {
        string version = GetValueFromReadme(SpecialLogic.Version, null, SA_VersionDetect);

        if (version.IsEmpty()) return "";

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
#endif

    private (List<string> Langs, bool EnglishIsUncertain)
    GetLanguages(List<NameAndIndex> baseDirFiles, List<NameAndIndex> booksDirFiles,
        List<NameAndIndex> intrfaceDirFiles, List<NameAndIndex> stringsDirFiles)
    {
        var langs = new List<string>();
        bool englishIsUncertain = false;

        for (int dirIndex = 0; dirIndex < 3; dirIndex++)
        {
            List<NameAndIndex> dirFiles = dirIndex switch
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
                    string dfName = df.Name.ToForwardSlashes();

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

            // @PERF_TODO: String allocation, but a large convenience
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
                Regex.Match(fn, "[a-z]+RUS$").Success ||
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
            List<string> langsD = langs.Distinct().ToList();
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
        var ret = (NewDarkRequired: (bool?)null, Game: Game.Null);

        #region Choose smallest .gam file

        NameAndIndex[] gamFiles = baseDirFiles.Where(static x => x.Name.ExtIsGam()).ToArray();
        NameAndIndex? smallestGamFile = null;

        if (gamFiles.Length > 0)
        {
            if (gamFiles.Length == 1)
            {
                smallestGamFile = gamFiles[0];
            }
            else
            {
                var gamSizeList = new List<(NameAndIndex Gam, long Size)>(gamFiles.Length);
                foreach (NameAndIndex gam in gamFiles)
                {
                    long length;
                    if (_fmIsZip)
                    {
                        length = _archive.Entries[gam.Index].Length;
                    }
                    else
                    {
                        string? gamFullPath = null;
                        FileInfoCustom? gamFI = _fmDirFileInfos.Find(x => x.FullName.PathEqualsI(gamFullPath ??= Path.Combine(_fmWorkingPath, gam.Name)));
                        length = gamFI?.Length ?? new FileInfo(gamFullPath ?? Path.Combine(_fmWorkingPath, gam.Name)).Length;
                    }
                    gamSizeList.Add((gam, length));
                }

                smallestGamFile = gamSizeList.OrderBy(static x => x.Size).First().Gam;
            }
        }

        #endregion

        #region Choose smallest .mis file

        var misSizeList = new List<(NameAndIndex Mis, long Size)>(usedMisFiles.Count);
        NameAndIndex smallestUsedMisFile;

        if (usedMisFiles.Count == 1)
        {
            smallestUsedMisFile = usedMisFiles[0];
        }
        // We know usedMisFiles can never be empty at this point because we early-return way before this if
        // it is
        else // usedMisFiles.Count > 1
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
                    string? misFullPath = null;
                    FileInfoCustom? misFI = _fmDirFileInfos.Find(x => x.FullName.PathEqualsI(misFullPath ??= Path.Combine(_fmWorkingPath, mis.Name)));
                    length = misFI?.Length ?? new FileInfo(misFullPath ?? Path.Combine(_fmWorkingPath, mis.Name)).Length;
                }
                misSizeList.Add((mis, length));
            }

            smallestUsedMisFile = misSizeList.OrderBy(static x => x.Size).First().Mis;
        }

        #endregion

        #region Setup

        ZipArchiveSEntry gamFileZipEntry = null!;
        ZipArchiveSEntry misFileZipEntry = null!;

        string misFileOnDisk = null!;

        if (_fmIsZip)
        {
            if (smallestGamFile != null) gamFileZipEntry = _archive.Entries[smallestGamFile.Index];
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
        System Shock 2 .mis files all have the MAPPARAM string. It will be at either 696 or 916.
        696 = NewDark, 916 = OldDark.
        (but we don't detect OldDark/NewDark for SS2 yet, see below)

        * We skip this check because only a handful of OldDark Thief 2 missions have SKYOBJVAR in a wacky
          location, and it's faster and more reliable to simply carry on with the secondary check than to
          try to guess where SKYOBJVAR is in this case.

        For folder scans, we can seek to these positions directly, but for zip scans, we have to read
        through the stream sequentially until we hit each one.
        */

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

        using (var br = _fmIsZip
                   ? new BinaryReader(_archive.OpenEntry(misFileZipEntry), Encoding.ASCII, false)
                   : new BinaryReader(GetFileStreamFast(misFileOnDisk, DiskFileStreamBuffer), Encoding.ASCII, false))
        {
            for (int i = 0; i < _locations.Length; i++)
            {
                if (
#if FMScanner_FullCode
                    !_scanOptions.ScanNewDarkRequired &&
#endif
                    (_locations[i] == _newDarkLoc1 || _locations[i] == _newDarkLoc2))
                {
                    break;
                }

                byte[] buffer;

                if (_fmIsZip)
                {
                    buffer = _zipOffsetBuffers[i];
                    int length = _zipOffsets[i];
                    int bytesRead = br.BaseStream.ReadAll(buffer, 0, length);
                    if (bytesRead < length) break;
                }
                else
                {
                    buffer = _gameDetectStringBuffer;
                    br.BaseStream.Position = _locations[i];
                    int bytesRead = br.BaseStream.ReadAll(buffer, 0, _gameDetectStringBufferLength);
                    if (bytesRead < _gameDetectStringBufferLength) break;
                }

                if ((_locations[i] == _ss2MapParamNewDarkLoc ||
                     _locations[i] == _ss2MapParamOldDarkLoc) &&
                    EndsWithMAPPARAM(buffer))
                {
                    /*
                    TODO: @SS2: AngelLoader doesn't need to know if NewDark is required, but put that in eventually
                    How to detect NewDark/OldDark for SS2:
                    If MAPPARAM is at either OldDark location or NewDark location, then loop through all
                    .mis files and check for MAPPARAM at NewDark location in each. If any found at NewDark
                    location, we're NewDark. Unfortunately we have to do that because SS2 missions sometimes
                    combine OldDark and NewDark .mis files.
                    So we're leaving this disabled for now because adding a .mis read loop in here would be
                    a pain, but there's how we would add it.
                    Also, we can detect NewDark for T1/T2 by checking for DARKMISS at byte 612 if we wanted
                    (DARKMISS at byte 612 = NewDark) although our SKYOBJVAR position checking method works
                    fine currently.
                    SS2 .mis files don't have DARKMISS in them at all.
                    */
                    return (null, Game.SS2);
                }
                else if ((_locations[i] == _oldDarkT2Loc ||
                          _locations[i] == _newDarkLoc1 ||
                          _locations[i] == _newDarkLoc2) &&
                         EndsWithSKYOBJVAR(buffer))
                {
                    // Zip reading is going to check the NewDark locations the other way round, but fortunately
                    // they're interchangeable in meaning so we don't have to do anything
                    if (_locations[i] == _newDarkLoc1 || _locations[i] == _newDarkLoc2)
                    {
                        ret.NewDarkRequired = true;
                        foundAtNewDarkLocation = true;
                        break;
                    }
                    else if (_locations[i] == _oldDarkT2Loc)
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
            return (
#if FMScanner_FullCode
                _scanOptions.ScanNewDarkRequired ? (bool?)false :
#endif
                null,
                _scanOptions.ScanGameType ? Game.Thief2 : Game.Null);
        }

        if (!_scanOptions.ScanGameType) return (ret.NewDarkRequired, Game.Null);

        #region Check for T2-unique value in .gam or .mis (determines game type for both OldDark and NewDark)

        static bool StreamContainsIdentString(
            Stream stream,
            byte[] identString,
            byte[] chunk,
            int bufferSize)
        {
            // To catch matches on a boundary between chunks, leave extra space at the start of each chunk
            // for the last boundaryLen bytes of the previous chunk to go into, thus achieving a kind of
            // quick-n-dirty "step back and re-read" type thing. Dunno man, it works.
            int boundaryLen = identString.Length;

            chunk.Clear();

            int bytesRead;
            while ((bytesRead = stream.ReadAll(chunk, boundaryLen, bufferSize)) != 0)
            {
                // Zero out all bytes after the end of the read data if there are any, in the ludicrously
                // unlikely case that the end of this read data combines with the data that was already in
                // there and gives a false match. Literally not gonna happen but like yeah I noticed so yeah.
                if (bytesRead < bufferSize)
                {
                    Array.Clear(chunk, boundaryLen + bytesRead, bufferSize - (boundaryLen + bytesRead));
                }

                if (chunk.Contains(identString)) return true;

                // Copy the last boundaryLen bytes from chunk and put them at the beginning
                for (int si = 0, ei = bufferSize; si < boundaryLen; si++, ei++) chunk[si] = chunk[ei];
            }

            return false;
        }

        if (_fmIsZip)
        {
            // For zips, since we can't seek within the stream, the fastest way to find our string is just to
            // brute-force straight through.
            using Stream stream = smallestGamFile != null ? _archive.OpenEntry(gamFileZipEntry) : _archive.OpenEntry(misFileZipEntry);
            ret.Game = StreamContainsIdentString(
                stream,
                RopeyArrow,
                GameTypeBuffer_ChunkPlusRopeyArrow,
                _gameTypeBufferSize)
                ? Game.Thief2
                : Game.Thief1;
        }
        else
        {
            // For uncompressed files on disk, we mercifully can just look at the TOC and then seek to the
            // OBJ_MAP chunk and search it for the string. Phew.
            using var br = new BinaryReader(GetFileStreamFast(misFileOnDisk, DiskFileStreamBuffer), Encoding.ASCII, leaveOpen: false);

            uint tocOffset = br.ReadUInt32();

            br.BaseStream.Position = tocOffset;

            uint invCount = br.ReadUInt32();
            for (int i = 0; i < invCount; i++)
            {
                int bytesRead = br.BaseStream.ReadAll(_misChunkHeaderBuffer, 0, 12);
                uint offset = br.ReadUInt32();
                uint length = br.ReadUInt32();

                if (bytesRead < 12 || !_misChunkHeaderBuffer.Contains(OBJ_MAP)) continue;

                // Put us past the name (12), version high (4), version low (4), and the zero (4).
                // Length starts AFTER this 24-byte header! (thanks JayRude)
                br.BaseStream.Position = offset + 24;

                byte[] content = br.ReadBytes((int)length);
                ret.Game = content.Contains(RopeyArrow)
                    ? Game.Thief2
                    : Game.Thief1;
                break;
            }
        }

        #endregion

        #region SS2 slow-detect fallback

        // Paranoid fallback. In case MAPPARAM ends up at a different byte location in a future version of
        // NewDark, we run this check if we suspect we're dealing with an SS2 FM (we will have fingerprinted
        // it earlier during ReadAndCacheFMData() and again here). For T2, we have a fallback scan if we don't
        // find SKYOBJVAR at byte 772, so we're safe. But SS2 we should have a fallback in place as well. It's
        // really slow, but better slow than incorrect. This way, if a new SS2 FM is released and has MAPPARAM
        // in a different place, at least we're like 98% certain to still detect it correctly here. Then people
        // can still at least have an accurate detection while I work on a new version that takes the new
        // MAPPARAM location into account.

        static bool SS2MisFilesPresent(List<NameAndIndex> misFiles, HashSetI ss2MisFiles)
        {
            for (int mfI = 0; mfI < misFiles.Count; mfI++)
            {
                if (ss2MisFiles.Contains(misFiles[mfI].Name))
                {
                    return true;
                }
            }

            return false;
        }

        // Just check the bare ss2 fingerprinted value, because if we're here then we already know it's required
        if (ret.Game == Game.Thief1 && (_ss2Fingerprinted || SS2MisFilesPresent(usedMisFiles, FMFiles_SS2MisFiles)))
        {
            using Stream stream = _fmIsZip ? _archive.OpenEntry(misFileZipEntry) : GetFileStreamFast(misFileOnDisk, DiskFileStreamBuffer);
            if (StreamContainsIdentString(
                    stream,
                    MAPPARAM,
                    GameTypeBuffer_ChunkPlusMAPPARAM,
                    _gameTypeBufferSize))
            {
                ret.Game = Game.SS2;
            }
        }

        #endregion

        return ret;
    }

#if FMScanner_FullCode
    private string GetNewDarkVersionFromText(string text)
    {
        string version = "";

        for (int i = 0; i < NewDarkVersionRegexes.Length; i++)
        {
            Match match = NewDarkVersionRegexes[i].Match(text);
            if (match.Success)
            {
                version = match.Groups["Version"].Value;
                break;
            }
        }

        if (version.IsEmpty()) return "";

        string ndv = version.Trim(CA_Period);
        int index = ndv.IndexOf('.');
        if (index > -1 && ndv.Substring(index + 1).Length < 2)
        {
            ndv += "0";
        }

        // Anything lower than 1.19 is OldDark; and cut it off at 2.0 to prevent that durn old time-travelling
        // Zealot's Hollow from claiming it was made with "NewDark Version 2.1"
        return Float_TryParseInv(ndv, out float ndvF) && ndvF >= 1.19 && ndvF < 2.0 ? ndv : "";
    }
#endif

    private void DeleteFMWorkingPath()
    {
        try
        {
            // IMPORTANT: _DO NOT_ delete the working path if we're a folder FM to start with!
            // That means our working path is NOT temporary!!!
            if (!_fmIsSevenZip ||
                _fmWorkingPath.IsEmpty() ||
                !Directory.Exists(_fmWorkingPath))
            {
                return;
            }
            DeleteDirectory(_fmWorkingPath);
        }
        catch
        {
            // Don't care
        }
    }

    #region Helpers

#if FMScanner_FullCode

    // So we don't bloat up AL_Common with this when we don't use it there

    /// <summary>
    /// Returns the number of times a character appears in a string.
    /// Avoids whatever silly overhead junk Count(predicate) is doing.
    /// </summary>
    /// <param name="value"></param>
    /// <param name="character"></param>
    /// <returns></returns>
    private static int CountChars(string value, char character)
    {
        int count = 0;
        for (int i = 0; i < value.Length; i++) if (value[i] == character) count++;

        return count;
    }

#endif

    /// <summary>
    /// Deletes a directory after first setting everything in it, and itself, to non-read-only.
    /// </summary>
    /// <param name="directory"></param>
    private static void DeleteDirectory(string directory)
    {
        DirAndFileTree_UnSetReadOnly(directory);

        string[] dirs = Directory.GetDirectories(directory, "*", SearchOption.TopDirectoryOnly);
        for (int i = 0; i < dirs.Length; i++)
        {
            Directory.Delete(dirs[i], recursive: true);
        }

        Directory.Delete(directory, recursive: true);
    }

    #region Stream copy with buffer

    private byte[]? _streamCopyBuffer;
    private byte[] StreamCopyBuffer => _streamCopyBuffer ??= new byte[81920];

    private void StreamCopyNoAlloc(Stream source, Stream destination)
    {
        byte[] buffer = StreamCopyBuffer;

        buffer.Clear();
        int count;
        while ((count = source.Read(buffer, 0, buffer.Length)) != 0)
        {
            destination.Write(buffer, 0, count);
        }
    }

    #endregion

    #region Generic dir/file functions

    private string[]
    EnumFiles(SearchOption searchOption) => EnumFiles("", searchOption, checkDirExists: false);

    private string[]
    EnumFiles(string path, SearchOption searchOption, bool checkDirExists = true)
    {
        string fullDir = Path.Combine(_fmWorkingPath, path);

        return !checkDirExists || Directory.Exists(fullDir)
            ? Directory.GetFiles(fullDir, "*", searchOption)
            : Array.Empty<string>();
    }

    #endregion

    #region Read plaintext

    #region ReadAllText (detect encoding)

    /// <summary>
    /// Reads all the text in a stream, auto-detecting its encoding. Ensures non-ASCII characters show up
    /// correctly.
    /// </summary>
    /// <param name="stream"></param>
    /// <returns></returns>
    private string ReadAllTextE(Stream stream)
    {
        Encoding encoding = _fileEncoding.DetectFileEncoding(stream) ?? Encoding.GetEncoding(1252);

        stream.Position = 0;

        using var sr = new StreamReader(stream, encoding);
        return sr.ReadToEnd();
    }

    /// <summary>
    /// Reads all the text in a file, auto-detecting its encoding. Ensures non-ASCII characters show up
    /// correctly.
    /// </summary>
    /// <param name="file"></param>
    /// <returns></returns>
    private string ReadAllTextE(string file)
    {
        Encoding encoding = _fileEncoding.DetectFileEncoding(file) ?? Encoding.GetEncoding(1252);

        using var sr = GetStreamReaderFast(file, encoding, DiskFileStreamBuffer);
        return sr.ReadToEnd();
    }

    #endregion

    #region ReadAllLines (detect encoding)

    /// <summary>
    /// Reads all the lines in a stream, auto-detecting its encoding. Ensures non-ASCII characters show up
    /// correctly.
    /// </summary>
    /// <param name="stream">The stream to read.</param>
    /// <param name="length">The length of the stream in bytes.</param>
    /// <returns></returns>
    private List<string> ReadAllLinesE(Stream stream, long length)
    {
        var lines = new List<string>();

        // Detecting the encoding of a stream reads it forward some amount, and I can't seek backwards in
        // an archive stream, so I have to copy it to a seekable MemoryStream. Blah.
        using var memStream = new MemoryStream((int)length);
        StreamCopyNoAlloc(stream, memStream);
        stream.Dispose();
        memStream.Position = 0;
        Encoding encoding = _fileEncoding.DetectFileEncoding(memStream) ?? Encoding.GetEncoding(1252);
        memStream.Position = 0;

        using var sr = new StreamReader(memStream, encoding, false);
        while (sr.ReadLine() is { } line) lines.Add(line);

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
        Encoding encoding = _fileEncoding.DetectFileEncoding(file) ?? Encoding.GetEncoding(1252);

        var lines = new List<string>();

        using var sr = GetStreamReaderFast(file, encoding, DiskFileStreamBuffer);
        while (sr.ReadLine() is { } line) lines.Add(line);

        return lines;
    }

    #endregion

    #region ReadAllLines (as is)

    private static List<string> ReadAllLines(Stream stream, Encoding encoding)
    {
        var lines = new List<string>();

        using var sr = new StreamReader(stream, encoding, false);
        while (sr.ReadLine() is { } line) lines.Add(line);

        return lines;
    }

    private List<string> ReadAllLines(string file, Encoding encoding)
    {
        var lines = new List<string>();

        using var sr = GetStreamReaderFast(file, encoding, false, DiskFileStreamBuffer);
        while (sr.ReadLine() is { } line) lines.Add(line);

        return lines;
    }

    #endregion

    #endregion

    #endregion

    [PublicAPI]
    public void Dispose()
    {
        // ReSharper disable once ConditionalAccessQualifierIsNonNullableAccordingToAPIContract
        _archive?.Dispose();
        _zipBundle.Dispose();
    }
}