// Uncomment this define in all files it appears in to get all features (we use it for testing)
//#define FMScanner_FullCode

/*
@MEM(Scanner readme line splitting):
We could just get the full text and then allocate an array of int pairs for start and length of each line,
then just use that when we need to go line-by-line. It's still an array allocation per readme, but it should
be far less memory allocated than to essentially duplicate the entire readme in separate line form as we do now.

@RAR(Scanner): The rar stuff here is a total mess! It works, but we should clean it up...
*/

//#define ScanSynchronous

global using static AL_Common.FullyGlobal;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using AL_Common;
using AL_Common.FastZipReader;
using JetBrains.Annotations;
using SharpCompress.Archives.Rar;
using SharpCompress.Archives.SevenZip;
using SharpCompress.Readers.Rar;
using Ude.NetStandard;
using Ude.NetStandard.SimpleHelpers;
using static System.StringComparison;
using static AL_Common.Common;
using static AL_Common.LanguageSupport;
using static AL_Common.Logger;

namespace FMScanner;

public sealed partial class Scanner : IDisposable
{
#if DEBUG
    private readonly Stopwatch _overallTimer = new Stopwatch();
#endif

    #region Private fields

    private readonly ReadOnlyDataContext _ctx;

    private bool _titlesStrIsOEM850;

    #region Disposable

    private ZipArchiveFast _archive = null!;
    private ZipContext? _zipContext;
    private ZipContext ZipContext => _zipContext ??= new ZipContext();
    private readonly StreamReaderCustom _streamReaderCustom = new();

    private readonly MemoryStream _generalMemoryStream = new();

    private Stream _rarStream = null!;
    private RarArchive _rarArchive = null!;

    #endregion

    /// <summary>
    /// Hack to support scanning two different sets of fields depending on a bool, you pass in "full" scan
    /// fields here and "non-full" fields in the Scan* methods, and mark each passed FM with a bool.
    /// </summary>
    private readonly ScanOptions _fullScanOptions;

    private readonly SevenZipContext _sevenZipContext = new();

    private readonly string _sevenZipWorkingPath;
    private readonly string _sevenZipExePath;

    // Biggest known FM plaintext readme as of 2023/03/28 is 56KB, so 100KB is way more than enough to not reallocate
    private readonly FileEncoding _fileEncoding = new(ByteSize.KB * 100);

    private readonly ListFast<FileInfoCustom> _fmDirFileInfos = new(0);

    private ScanOptions _scanOptions = new();

    private RtfToTextConverter? _rtfConverter;
    private RtfToTextConverter RtfConverter => _rtfConverter ??= new RtfToTextConverter();

    private enum FMFormat
    {
        NotInArchive,
        Zip,
        SevenZip,
        Rar,
        RarSolid,
    }

    private FMFormat _fmFormat = FMFormat.NotInArchive;

    private string _fmWorkingPath = "";

    private readonly List<ReadmeInternal> _readmeFiles = new();

    private bool _ss2Fingerprinted;

    private bool SS2FingerprintRequiredAndNotDone() => (
#if FMScanner_FullCode
        _scanOptions.ScanNewDarkRequired ||
#endif
        _scanOptions.ScanGameType) && !_ss2Fingerprinted;

    private byte[]? _diskFileStreamBuffer;
    private byte[] DiskFileStreamBuffer => _diskFileStreamBuffer ??= new byte[4096];

    private DirectoryInfo? _fmWorkingPathDirInfo;
    private DirectoryInfo FMWorkingPathDirInfo => _fmWorkingPathDirInfo ??= new DirectoryInfo(_fmWorkingPath);

    private string? _fmWorkingPathDirName;
    private string FMWorkingPathDirName => _fmWorkingPathDirName ??= FMWorkingPathDirInfo.Name;

    // 50 entries is more than we're ever likely to need in this list, but still small enough not to be wasteful.
    private List<string>? _solidExtractedFilesList;
    private List<string> SolidExtractedFilesList => _solidExtractedFilesList ??= new List<string>(50);

    private List<string>? _solidExtractedFilesTempList;
    private List<string> SolidZipExtractedFilesTempList => _solidExtractedFilesTempList ??= new List<string>(50);

    private readonly List<string> _tempLines = new();
    private const int _maxTopLines = 5;
    private readonly List<string> _topLines = new(_maxTopLines);

    private ListFast<char>? _title1_TempNonWhitespaceChars;
    private ListFast<char> Title1_TempNonWhitespaceChars => _title1_TempNonWhitespaceChars ??= new ListFast<char>(50);

    private ListFast<char>? _title2_TempNonWhitespaceChars;
    private ListFast<char> Title2_TempNonWhitespaceChars => _title2_TempNonWhitespaceChars ??= new ListFast<char>(50);

    private readonly ListFast<char> _titleAcronymChars = new(10);
    private readonly ListFast<char> _altTitleAcronymChars = new(10);
    private readonly ListFast<char> _altTitleRomanToDecimalAcronymChars = new(10);

    private readonly List<NameAndIndex> _baseDirFiles = new(20);
    private readonly List<NameAndIndex> _misFiles = new(20);
    private readonly List<NameAndIndex> _usedMisFiles = new(20);
    private readonly List<NameAndIndex> _stringsDirFiles = new();
    private readonly List<NameAndIndex> _intrfaceDirFiles = new();
    private readonly List<NameAndIndex> _booksDirFiles = new();

    private readonly List<NameAndIndex> _readmeDirFiles = new(10);

    private List<NameAndIndex>? _t3FMExtrasDirFiles;
    private List<NameAndIndex> T3FMExtrasDirFiles => _t3FMExtrasDirFiles ??= new List<NameAndIndex>(10);

    private List<NameAndIndex>? _t3GmpFiles;
    private List<NameAndIndex> T3GmpFiles => _t3GmpFiles ??= new List<NameAndIndex>(20);

    private readonly ScannerTDMContext _tdmContext;

    #endregion

    #region Private classes

    private enum CopyReadmesToCacheResult
    {
        Success,
        NeedsHtmlRefExtract,
    }

    private enum DetectEncodingType
    {
        Standard,
        TitlesStr,
        NewGameStr,
    }

    private sealed class DetectedTitle(string value, bool temporary)
    {
        internal readonly string Value = value;
        internal bool Temporary = temporary;
    }

    private sealed class FileInfoCustom
    {
        internal string FullName;
        internal long Length;

        private SevenZipArchiveEntry? _archiveFileInfo;

        private bool _rar;

        private DateTime? _lastWriteTime;
        internal DateTime LastWriteTime
        {
            get
            {
                if (_rar)
                {
                    return (DateTime)_lastWriteTime!;
                }
                else
                {
                    if (_archiveFileInfo != null)
                    {
                        _lastWriteTime = _archiveFileInfo.LastModifiedTime ?? DateTime.MinValue;
                        _archiveFileInfo = null;
                    }
                }
                return (DateTime)_lastWriteTime!;
            }
        }

        internal FileInfoCustom(FileInfo fileInfo) => Set(fileInfo);

        /*
        @MEM(FileInfoCustom.FullName janky horrible inconsistency explanation)
        In this case, FullName will just be the entry name, so "readme.txt" for example, so not a full name AT ALL.
        I'm going to assume I did this to avoid allocations - if we passed the fm working path into here and
        combined it that's a string alloc for every entry, whereas we otherwise only need to do the combine for
        a small number of entries. I'm pretty sure that's why I would have done this, because it necessitates an
        fm-is-7zip check everywhere we do a name compare. UGH.
        */
        internal FileInfoCustom(SevenZipArchiveEntry archiveFileInfo) => Set(archiveFileInfo);

        internal FileInfoCustom(RarArchiveEntry entry) => Set(entry);

        [MemberNotNull(nameof(FullName))]
        internal void Set(FileInfo fileInfo)
        {
            _rar = false;
            FullName = fileInfo.FullName;
            Length = fileInfo.Length;
            _lastWriteTime = fileInfo.LastWriteTime;
            _archiveFileInfo = null;
        }

        [MemberNotNull(nameof(FullName))]
        internal void Set(SevenZipArchiveEntry archiveFileInfo)
        {
            _rar = false;
            FullName = archiveFileInfo.FileName;
            Length = archiveFileInfo.UncompressedSize;
            _lastWriteTime = null;
            _archiveFileInfo = archiveFileInfo;
        }

        [MemberNotNull(nameof(FullName))]
        internal void Set(RarArchiveEntry entry)
        {
            _rar = true;
            FullName = entry.Key;
            Length = entry.Size;
            _lastWriteTime = entry.LastModifiedTime ?? DateTime.MinValue;
            _archiveFileInfo = null;
        }
    }

    private sealed class ReadmeInternal
    {
        /// <summary>
        /// Check this bool to see if you want to scan the file or not. Currently false if readme is HTML or
        /// non-English.
        /// </summary>
        internal readonly bool Scan;
        internal readonly bool UseForDateDetect;
        internal readonly bool IsGlml;
        internal readonly ListFast<string> Lines = new(0);
        internal string Text = "";

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

        internal ReadmeInternal(bool isGlml, uint lastModifiedDateRaw, bool scan, bool useForDateDetect)
        {
            IsGlml = isGlml;
            _lastModifiedDateRaw = lastModifiedDateRaw;
            Scan = scan;
            UseForDateDetect = useForDateDetect;
        }

        internal ReadmeInternal(bool isGlml, DateTime lastModifiedDate, bool scan, bool useForDateDetect)
        {
            IsGlml = isGlml;
            _lastModifiedDate = lastModifiedDate;
            Scan = scan;
            UseForDateDetect = useForDateDetect;
        }
    }

    #endregion

    private enum SpecialLogic
    {
        Title,
        Author,
        ReleaseDate,
#if FMScanner_FullCode
        Version,
#endif
    }

    #region Constructors

#if FMScanner_FullCode
    [PublicAPI]
    public Scanner(string sevenZipExePath) : this(Path.GetDirectoryName(sevenZipExePath)!, sevenZipExePath, new ScanOptions(), new ReadOnlyDataContext(), new ScannerTDMContext(""))
    {
    }

    [PublicAPI]
    public Scanner(string sevenZipWorkingPath, string sevenZipExePath) : this(sevenZipWorkingPath, sevenZipExePath, new ScanOptions(), new ReadOnlyDataContext(), new ScannerTDMContext(""))
    {
    }
#endif

    [PublicAPI]
    public Scanner(
        string sevenZipWorkingPath,
        string sevenZipExePath,
        ScanOptions fullScanOptions,
        ReadOnlyDataContext readOnlyDataContext,
        ScannerTDMContext tdmContext)
    {
#if !NETFRAMEWORK
        System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
#endif

        _fullScanOptions = fullScanOptions;

        _ctx = readOnlyDataContext;

        _sevenZipWorkingPath = sevenZipWorkingPath;
        _sevenZipExePath = sevenZipExePath;

        _tdmContext = tdmContext;
    }

    #endregion

    public static List<ScannedFMDataAndError>
    ScanThreaded(
        string sevenZipWorkingPath,
        string sevenZipExePath,
        ScanOptions fullScanOptions,
        ScannerTDMContext tdmContext,
        int threadCount,
        List<FMToScan> fms,
        string tempPath,
        ScanOptions scanOptions,
        IProgress<ProgressReport> progress,
        CancellationToken cancellationToken)
    {
        ConcurrentQueue<FMToScan> cq = new(fms);
        ConcurrentBag<List<ScannedFMDataAndError>> returnLists = new();

        ReadOnlyDataContext ctx = new();
        try
        {
            ParallelOptions po = new()
            {
                CancellationToken = cancellationToken,
                MaxDegreeOfParallelism = threadCount,
            };

            _ = Parallel.For(0, threadCount, po, _ =>
            {
                using var scanner = new Scanner(
                    sevenZipWorkingPath: sevenZipWorkingPath,
                    sevenZipExePath: sevenZipExePath,
                    fullScanOptions: fullScanOptions,
                    readOnlyDataContext: ctx,
                    tdmContext: tdmContext);

                returnLists.Add(scanner.Scan(
                    cq,
                    tempPath: tempPath,
                    scanOptions: scanOptions,
                    progress: progress,
                    cancellationToken: cancellationToken));
            });
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            Log(ex: ex);
        }

        List<ScannedFMDataAndError> returnListFinal = new(fms.Count);

        foreach (List<ScannedFMDataAndError> list in returnLists)
        {
            returnListFinal.AddRange(list);
        }
        returnListFinal.Sort(new FMScanOriginalIndexComparer());

        return returnListFinal;
    }

    #region Scan synchronous

#if FMScanner_FullCode

    [PublicAPI]
    public ScannedFMDataAndError
    Scan(string mission, string tempPath, bool forceFullIfNew, string name, bool isArchive)
    {
        ConcurrentQueue<FMToScan> cq = new(new List<FMToScan>
        {
            new(path: mission, forceFullScan: forceFullIfNew, displayName: name, isTDM: false,
                isArchive: isArchive, originalIndex: 0),
        });
        return ScanMany(cq, tempPath, _scanOptions, null, CancellationToken.None)[0];
    }

    [PublicAPI]
    public ScannedFMDataAndError
    Scan(string mission, string tempPath, ScanOptions scanOptions, bool forceFullIfNew, string name, bool isArchive)
    {
        ConcurrentQueue<FMToScan> cq = new(new List<FMToScan>
        {
            new(path: mission, forceFullScan: forceFullIfNew, displayName: name, isTDM: false,
                isArchive: isArchive, originalIndex: 0),
        });
        return ScanMany(cq, tempPath, scanOptions, null, CancellationToken.None)[0];
    }

#endif

    // Debug should also use this - scan on UI thread so breaks will actually break where they're supposed to
    [PublicAPI]
    public List<ScannedFMDataAndError>
    Scan(ConcurrentQueue<FMToScan> missions, string tempPath, ScanOptions scanOptions,
         IProgress<ProgressReport> progress, CancellationToken cancellationToken)
    {
        return ScanMany(missions, tempPath, scanOptions, progress, cancellationToken);
    }

    #endregion

    #region Scan asynchronous

#if FMScanner_FullCode

    [PublicAPI]
    public Task<List<ScannedFMDataAndError>>
    ScanAsync(List<FMToScan> missions, string tempPath)
    {
        ConcurrentQueue<FMToScan> cq = new(missions);
        return Task.Run(() => ScanMany(cq, tempPath, _scanOptions, null, CancellationToken.None));
    }

    [PublicAPI]
    public Task<List<ScannedFMDataAndError>>
    ScanAsync(List<FMToScan> missions, string tempPath, ScanOptions scanOptions)
    {
        ConcurrentQueue<FMToScan> cq = new(missions);
        return Task.Run(() => ScanMany(cq, tempPath, scanOptions, null, CancellationToken.None));
    }

    [PublicAPI]
    public Task<List<ScannedFMDataAndError>>
    ScanAsync(List<FMToScan> missions, string tempPath, IProgress<ProgressReport> progress,
              CancellationToken cancellationToken)
    {
        ConcurrentQueue<FMToScan> cq = new(missions);
        return Task.Run(() => ScanMany(cq, tempPath, _scanOptions, progress, cancellationToken));
    }

#endif

    [PublicAPI]
    public Task<List<ScannedFMDataAndError>> ScanAsync(
        ConcurrentQueue<FMToScan> missions,
        string tempPath,
        ScanOptions scanOptions,
        IProgress<ProgressReport> progress,
        CancellationToken cancellationToken)
    {
        return Task.Run(() => ScanMany(missions, tempPath, scanOptions, progress, cancellationToken));
    }

    #endregion

    private void ResetCachedFields()
    {
        _titlesStrIsOEM850 = false;
        _tempLines.Clear();
        _topLines.Clear();
        _readmeFiles.Clear();
        _fmDirFileInfos.ClearFast();
        _ss2Fingerprinted = false;
        _fmWorkingPathDirName = null;
        _fmWorkingPathDirInfo = null;
        _fmFormat = FMFormat.NotInArchive;
        _solidExtractedFilesList?.Clear();
        _solidExtractedFilesTempList?.Clear();

        _title1_TempNonWhitespaceChars?.ClearFast();
        _title2_TempNonWhitespaceChars?.ClearFast();

        _baseDirFiles.Clear();
        _misFiles.Clear();
        _usedMisFiles.Clear();
        _stringsDirFiles.Clear();
        _intrfaceDirFiles.Clear();
        _booksDirFiles.Clear();

        _readmeDirFiles.Clear();

        _t3FMExtrasDirFiles?.Clear();
        _t3GmpFiles?.Clear();
    }

    private List<ScannedFMDataAndError>
    ScanMany(ConcurrentQueue<FMToScan> fms, string tempPath, ScanOptions scanOptions,
             IProgress<ProgressReport>? progress, CancellationToken cancellationToken)
    {
        // The try-catch blocks are to guarantee that the out-list will at least contain the same number of
        // entries as the in-list; this allows the calling app to not have to do a search to link up the FMs
        // and stuff

        #region Checks

        if (tempPath.IsEmpty())
        {
            Log("Argument is null or empty: " + nameof(tempPath));
            ThrowHelper.ArgumentException("Argument is null or empty.", nameof(tempPath));
        }

        if (fms == null) throw new ArgumentNullException(nameof(fms));

        // Deep-copy the scan options object because we might have to change its values in some cases, but we
        // don't want to modify the original because the caller will still have a reference to it and may
        // depend on it not changing.
        _scanOptions = scanOptions?.DeepCopy() ?? throw new ArgumentNullException(nameof(scanOptions));

        #endregion

        var scannedFMDataList = new List<ScannedFMDataAndError>(fms.Count);

        var progressReport = new ProgressReport();

        while (fms.TryDequeue(out FMToScan fm))
        {
            ResetCachedFields();

            // Random name for solid archive temp extract operations, to prevent possible file/folder name
            // clashes in parallelized scenario.
            string tempRandomName = Path.GetRandomFileName().Trim();

            bool nullAlreadyAdded = false;

            #region Init

            if (fm.Path.IsEmpty())
            {
                scannedFMDataList.Add(new ScannedFMDataAndError(fm.OriginalIndex));
                nullAlreadyAdded = true;
            }
            else
            {
                string fmPath = fm.Path;
                _fmFormat =
                    !fm.IsArchive
                        ? FMFormat.NotInArchive
                        : fmPath.ExtIsZip()
                            ? FMFormat.Zip
                            : fmPath.ExtIs7z()
                                ? FMFormat.SevenZip
                                : fmPath.ExtIsRar()
                                    ? FMFormat.Rar
                                    : FMFormat.NotInArchive;

                _archive?.Dispose();
                _rarArchive?.Dispose();
                _rarStream?.Dispose();

                if (_fmFormat > FMFormat.NotInArchive)
                {
                    try
                    {
                        _fmWorkingPath = Path.Combine(tempPath, tempRandomName);
                    }
                    catch (Exception ex)
                    {
                        Log(fmPath + ": Path.Combine error, paths are probably invalid", ex);
                        scannedFMDataList.Add(new ScannedFMDataAndError(fm.OriginalIndex));
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
                progressReport.FMName = fm.DisplayName;
                /*
                @MT_TASK: Race condition with fms.Count
                The count could have gone down between when we dequeued this fm and when we get the count here.
                Not sure if there's anything that can be done about it, there doesn't seem to be an atomic
                operation to dequeue AND get the count.
                Probably doesn't matter anyway because even if we could do that, another thread will report its
                progress anyway and there'll still be a race between us and it, even if we've removed the race
                between us and ourselves.
                @MT_TASK: We could pass in the original total count and use the FM's index field.
                */
                progressReport.FMsRemainingInQueue = fms.Count;

                progress.Report(progressReport);
            }

            #endregion

            // If there was an error then we already added null to the list. DON'T add any extra items!
            if (!nullAlreadyAdded)
            {
                var scannedFMAndError = new ScannedFMDataAndError(fm.OriginalIndex);
                ScanOptions? _tempScanOptions = null;
                try
                {
                    if (fm.ForceFullScan)
                    {
                        _tempScanOptions = _scanOptions.DeepCopy();
                        _scanOptions = _fullScanOptions.DeepCopy();
                    }

                    try
                    {
                        scannedFMAndError =
                            fm.IsTDM
                                ? ScanCurrentDarkModFM(fm)
                                : ScanCurrentFM(fm, tempPath, tempRandomName, cancellationToken);
                    }
                    catch (OperationCanceledException)
                    {
                        throw;
                    }
                    catch (Exception ex)
                    {
                        Log(fm.Path + ": Exception in FM scan", ex);
                        scannedFMAndError.ScannedFMData = null;
                        scannedFMAndError.Exception = ex;
                        scannedFMAndError.ErrorInfo = fm.Path + ": Exception in FM scan";
                    }
                    finally
                    {
                        if (fm.IsArchive && (fm.Path.ExtIs7z() || fm.Path.ExtIsRar()))
                        {
                            DeleteFMWorkingPath();
                        }
                    }

                    scannedFMDataList.Add(scannedFMAndError);
                }
                finally
                {
                    if (fm.ForceFullScan)
                    {
                        _scanOptions = _tempScanOptions!.DeepCopy();
                    }
                }
            }
        }

        return scannedFMDataList;
    }

    private void SetOrAddTitle(List<string> titles, string value)
    {
        value = CleanupTitle(value).Trim();

        if (value.IsEmpty()) return;

        if (!titles.ContainsI(value))
        {
            titles.Add(value);
        }
    }

    private void SetFMTitles(ScannedFMData fmData, List<string> titles, string? serverTitle = null)
    {
        OrderTitlesOptimally(titles, serverTitle);
        if (titles.Count > 0)
        {
            fmData.Title = titles[0];
            if (titles.Count > 1)
            {
                fmData.AlternateTitles = new string[titles.Count - 1];
                for (int i = 1; i < titles.Count; i++)
                {
                    fmData.AlternateTitles[i - 1] = titles[i].Trim();
                }
            }
        }
    }

    private bool SetupAuthorRequiredTitleScan()
    {
        // There's one author scan that depends on the title ("[title] by [author]"), so we need to scan
        // titles in that case, but we shouldn't actually set the title in the return object because the
        // caller didn't request it.
        bool scanTitleForAuthorPurposesOnly = false;
        if ((_scanOptions.ScanTags || _scanOptions.ScanAuthor) && !_scanOptions.ScanTitle)
        {
            _scanOptions.ScanTitle = true;
            scanTitleForAuthorPurposesOnly = true;
        }

        return scanTitleForAuthorPurposesOnly;
    }

    private void EndTitleScan(
        bool scanTitleForAuthorPurposesOnly,
        ScannedFMData fmData, List<string> titles, string? serverTitle = null)
    {
        List<string>? topOfReadmeTitles = GetTitlesFromTopOfReadmes();
        if (topOfReadmeTitles?.Count > 0)
        {
            for (int i = 0; i < topOfReadmeTitles.Count; i++)
            {
                SetOrAddTitle(titles, topOfReadmeTitles[i]);
            }
        }

        if (!scanTitleForAuthorPurposesOnly)
        {
            SetFMTitles(fmData, titles, serverTitle);
        }
        else
        {
            _scanOptions.ScanTitle = false;
        }
    }

    private ScannedFMDataAndError ScanCurrentDarkModFM(FMToScan fm)
    {
        string zipPath;

        /*
        @TDM_NOTE(Scan): The game rejects pk4s that don't have darkmod.txt in the base dir.
        If we matched this here, we'd still be counting them everywhere else (find, set-changed, etc). We really
        don't want to open zip files in all those codepaths, way too heavy on perf. So we're just going to say
        that garbage in the FMs folder is highly unlikely and leave it at that.

        @TDM_CASE: TDM uses OS case-sensitivity for darkmod.txt name
        */
        List<ZipArchiveFastEntry>? __zipEntries = null;
        List<ZipArchiveFastEntry> GetZipBaseDirEntries()
        {
            if (__zipEntries == null)
            {
                _archive?.Dispose();
                var zipResult = ConstructZipArchive(fm, zipPath, ZipContext, checkForZeroEntries: false);
                if (zipResult.Success)
                {
                    _archive = zipResult.Archive!;
                    __zipEntries = new List<ZipArchiveFastEntry>();
                    var entries = _archive.Entries;
                    for (int i = 0; i < entries.Count; i++)
                    {
                        ZipArchiveFastEntry entry = entries[i];
                        if (!entry.FullName.Rel_ContainsDirSep())
                        {
                            __zipEntries.Add(entry);
                        }
                    }
                }
                else
                {
                    __zipEntries = null;
                    throw zipResult.ScannedFMDataAndError?.Exception ?? new Exception("Zip reading failed");
                }
            }

            return __zipEntries;
        }

        TDM_ServerFMData? infoFromServer = null;
        if (_tdmContext.ServerFMData.TryGetValue(FMWorkingPathDirName, out TDM_ServerFMData serverFMData) &&
            _tdmContext.LocalFMData.TryGetValue(FMWorkingPathDirName, out TDM_LocalFMData localFMData) &&
            serverFMData.Version == localFMData.DownloadedVersion)
        {
            infoFromServer = serverFMData;
        }

        ScannedFMData fmData = new()
        {
            ArchiveName = FMWorkingPathDirName,
            Game = Game.TDM,
        };

        if (!Directory.Exists(_fmWorkingPath) &&
            _tdmContext.BaseFMsDirPK4Files.TryGetValue(FMWorkingPathDirName, out string realPK4))
        {
            if (_tdmContext.FMsPath.IsEmpty())
            {
                throw new Exception("TDM FM encountered, but passed fms path was empty!");
            }
            zipPath = Path.Combine(_tdmContext.FMsPath, realPK4);
        }
        else
        {
            // Matching game behavior: pk4 files inside FM folders can be named anything and still be game-loadable
            zipPath = Path.Combine(fm.Path, fmData.ArchiveName + ".pk4");
            if (!File.Exists(zipPath))
            {
                try
                {
                    zipPath = "";
                    string[] pk4FilesInFMFolder = Directory.GetFiles(fm.Path, "*.pk4", SearchOption.TopDirectoryOnly);
                    foreach (string fileName in pk4FilesInFMFolder)
                    {
                        // @TDM_CASE(Scanner: pk4 within fm folder - _l10n check)
                        if (!fileName.EndsWith("_l10n.pk4", OrdinalIgnoreCase))
                        {
                            zipPath = fileName;
                            break;
                        }
                    }
                }
                catch
                {
                    zipPath = "";
                }

                if (zipPath.IsEmpty())
                {
                    Log("Found a TDM FM directory with no pk4 in it. Invalid FM or empty FM directory. Returning 'Unsupported'.");
                    return UnsupportedTDM(
                        archivePath: fm.Path,
                        fen7zResult: null,
                        ex: null,
                        errorInfo: "FM directory: " + fm.Path,
                        originalIndex: fm.OriginalIndex
                    );
                }
            }
        }

        bool scanTitleForAuthorPurposesOnly = SetupAuthorRequiredTitleScan();

        if (_scanOptions.ScanSize)
        {
            try
            {
                FileInfo fi = new(zipPath);
                fmData.Size = (ulong)fi.Length;
            }
            catch
            {
                // ignore
            }
        }

        bool singleMission = true;

        if (_scanOptions.ScanMissionCount || _scanOptions.ScanTags)
        {
            int misCount = 0;

            try
            {
                List<ZipArchiveFastEntry> entries = GetZipBaseDirEntries();
                for (int i = 0; i < entries.Count; i++)
                {
                    ZipArchiveFastEntry entry = entries[i];
                    if (entry.FullName != FMFiles.TDM_MapSequence) continue;

                    using Stream es = _archive.OpenEntry(entry);
                    // Stupid micro-optimization: Don't call Dispose() method on stream twice
                    using var sr = new StreamReaderCustom.SRC_Wrapper(es, Encoding.UTF8, false, _streamReaderCustom, disposeStream: false);

                    bool inBlockComment = false;
                    while (sr.Reader.ReadLine() is { } line)
                    {
                        string lineT = line.Trim();

                        if (inBlockComment)
                        {
                            if (lineT.EndsWithO("*/"))
                            {
                                inBlockComment = false;
                            }
                        }
                        else if (lineT.StartsWithO("/*"))
                        {
                            inBlockComment = true;
                        }
                        else if (lineT.StartsWithO("//"))
                        {
                            // ReSharper disable once RedundantJumpStatement
                            continue;
                        }
                        /*
                        From https://wiki.thedarkmod.com/index.php?title=Tdm_mapsequence.txt:

                        --- snip ---

                        The syntax is:
                        Mission <N>: <mapname> [<mapname> ...]

                        N is the mission number, with the first mission carrying the number 1.

                        It's possible to define more than one map filename for a mission,
                        in case there are loading zones in it, but usually you won't need that.

                        --- snip ---

                        The way it's phrased makes it sound like multiple "maps" should still be considered
                        part of the same "mission" if they're used like this. So we're going to consider one
                        "Mission" line to be one mission, and if it has multiple maps then it's one mission
                        with loading zones.
                        */
                        else if (_ctx.DarkMod_TDM_MapSequence_MissionLine_Regex.Match(lineT).Success)
                        {
                            misCount++;
                        }
                    }
                }

                if (_scanOptions.ScanMissionCount)
                {
                    fmData.MissionCount = misCount == 0 ? 1 : misCount;
                }
                singleMission = misCount is 0 or 1;
            }
            catch
            {
                if (_scanOptions.ScanMissionCount)
                {
                    fmData.MissionCount = null;
                }
                singleMission = true;
            }

            if (_scanOptions.ScanMissionCount)
            {
                if (_scanOptions.GetOptionsEnum() == ScanOptionsEnum.MissionCount)
                {
                    // Early return for perf if we're not scanning anything else
                    return new ScannedFMDataAndError(fm.OriginalIndex) { ScannedFMData = fmData };
                }
            }
        }

        if (_scanOptions.ScanTitle || _scanOptions.ScanAuthor || _scanOptions.ScanReleaseDate)
        {
            // @TDM_NOTE(readme text & dates):
            // For best perf, I guess we would get dates from the pk4 but text from disk.
            // Still, these files are generally extremely small, and TDM scans are lightning-fast anyway, so
            // let's just leave it.

            // Sometimes the extracted readmes have different dates than the ones in the pk4.
            // The pk4's dates are to be considered canonical, as they won't have been modified by some weird
            // copying or who knows what with the on-disk ones.
            (ReadmeInternal? darkModTxtReadme, ReadmeInternal? readmeTxtReadme) =
                AddReadmeFromPK4(GetZipBaseDirEntries(), FMFiles.TDM_DarkModTxt, FMFiles.TDM_ReadmeTxt);

            List<string> titles = new();

            // The Dark Mod apparently picks key-value pairs out of darkmod.txt ignoring linebreaks (see Lords & Legacy).
            // That's _TERRIBLE_ but we want to match behavior.
            if (darkModTxtReadme != null)
            {
                MatchCollection matches = _ctx.DarkModTxtFieldsRegex.Matches(darkModTxtReadme.Text);
                int plus = 0;
                foreach (Match match in matches)
                {
                    if (match.Index > 0)
                    {
                        char c = darkModTxtReadme.Text[(match.Index + plus) - 1];
                        if (c is not '\r' and not '\n')
                        {
                            darkModTxtReadme.Text = darkModTxtReadme.Text.Insert(match.Index + plus, "\r\n");
                            plus += 2;
                        }
                    }
                }

                if (plus > 0)
                {
                    darkModTxtReadme.Lines.ClearFullAndAdd(darkModTxtReadme.Text.Split_String(_ctx.SA_Linebreaks, StringSplitOptions.None, _sevenZipContext.IntArrayPool));
                }
            }

            if (_scanOptions.ScanTitle)
            {
                if (darkModTxtReadme != null && readmeTxtReadme != null)
                {
                    SetOrAddTitle(titles, GetValueFromReadme(SpecialLogic.Title, _ctx.SA_TitleDetect, singleReadme: darkModTxtReadme));
                    SetOrAddTitle(titles, GetValueFromReadme(SpecialLogic.Title, _ctx.SA_TitleDetect, singleReadme: readmeTxtReadme));
                }
                else
                {
                    SetOrAddTitle(titles, GetValueFromReadme(SpecialLogic.Title, _ctx.SA_TitleDetect));
                }

                if (infoFromServer != null)
                {
                    SetOrAddTitle(titles, infoFromServer.Title);
                }

                EndTitleScan(scanTitleForAuthorPurposesOnly, fmData, titles, infoFromServer?.Title);
            }

            if (_scanOptions.ScanAuthor)
            {
                if (infoFromServer != null)
                {
                    // @TDM_NOTE: We're not running author cleanup on these, we're expecting them to be sane...
                    fmData.Author = infoFromServer.Author;
                }
                else
                {
                    GetAuthor(fmData, titles);
                }
            }

            if (_scanOptions.ScanReleaseDate)
            {
                if (infoFromServer != null)
                {
                    if (infoFromServer.ReleaseDateDT != null)
                    {
                        fmData.LastUpdateDate = new DateTimeOffset((DateTime)infoFromServer.ReleaseDateDT).DateTime;
                    }
                }
                else
                {
                    fmData.LastUpdateDate = GetReleaseDate();
                }
            }
        }

        if (_scanOptions.ScanTags && !singleMission)
        {
            AddCampaignTag(fmData);
        }

        return new ScannedFMDataAndError(fm.OriginalIndex) { ScannedFMData = fmData };

        (ReadmeInternal? DarkModTxtIndex, ReadmeInternal? ReadmeTxtIndex)
        AddReadmeFromPK4(List<ZipArchiveFastEntry> baseDirEntries, string readme1Name, string readme2Name)
        {
            ZipArchiveFastEntry? readme1entry = null;
            ZipArchiveFastEntry? readme2entry = null;
            for (int i = 0; i < baseDirEntries.Count; i++)
            {
                ZipArchiveFastEntry entry = baseDirEntries[i];
                // @TDM_CASE("darkmod.txt", "readme.txt" constants)
                if (entry.FullName.EqualsI(readme1Name))
                {
                    readme1entry = entry;
                }
                else if (entry.FullName.EqualsI(readme2Name))
                {
                    readme2entry = entry;
                }

                if (readme1entry != null && readme2entry != null)
                {
                    break;
                }
            }

            ReadmeInternal? readmeInternal1 = null;
            ReadmeInternal? readmeInternal2 = null;

            if (readme1entry != null)
            {
                CreateReadme(readme1entry, out readmeInternal1);
            }

            if (readme2entry != null)
            {
                CreateReadme(readme2entry, out readmeInternal2);
            }

            return (readmeInternal1, readmeInternal2);

            void CreateReadme(ZipArchiveFastEntry entry, out ReadmeInternal? readme)
            {
                try
                {
                    readme = new ReadmeInternal(
                        isGlml: false,
                        lastModifiedDateRaw: entry.LastWriteTime,
                        scan: true,
                        useForDateDetect: true);
                    Stream readmeStream = CreateSeekableStreamFromZipEntry(entry, (int)entry.Length);
                    readme.Text = ReadAllTextDetectEncoding(readmeStream);
                    readme.Lines.ClearFullAndAdd(readme.Text.Split_String(_ctx.SA_Linebreaks, StringSplitOptions.None, _sevenZipContext.IntArrayPool));
                    _readmeFiles.Add(readme);
                }
                catch
                {
                    readme = null;
                }
            }
        }
    }

    private static (bool Success, ScannedFMDataAndError? ScannedFMDataAndError, ZipArchiveFast? Archive)
    ConstructZipArchive(FMToScan fm, string path, ZipContext zipContext, bool checkForZeroEntries)
    {
        ZipArchiveFast? ret;

        try
        {
            ret = new ZipArchiveFast(
                FileStreamCustom.CreateRead(path, zipContext.FileStreamBuffer),
                zipContext,
                allowUnsupportedEntries: false);

            // Archive.Entries is lazy-loaded, so this will also trigger any exceptions that may be
            // thrown while loading them. If this passes, we're definitely good.
            int entriesCount = ret.Entries.Count;
            if (checkForZeroEntries && entriesCount == 0)
            {
                Log(fm.Path + ": fm is zip, no files in archive. Returning 'Unsupported' game type.", stackTrace: false);
                return (false, UnsupportedZip(fm.Path, null, null, "", fm.OriginalIndex), null);
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
                Log(fm.Path + $": fm is zip.{NL}" +
                    $"UNSUPPORTED COMPRESSION METHOD{NL}" +
                    "Zip contains one or more files compressed with an unsupported method. " +
                    $"Only the DEFLATE method is supported. Try manually extracting and re-creating the zip file.{NL}" +
                    "Returning 'Unknown' game type.", zipEx);
                return (false, UnknownZip(fm.Path, null, zipEx, "", fm.OriginalIndex), null);
            }
            else
            {
                Log(fm.Path + ": fm is zip, exception in " +
                    nameof(ZipArchiveFast) +
                    " construction or entries getting. Returning 'Unsupported' game type.", ex);
                return (false, UnsupportedZip(fm.Path, null, ex, "", fm.OriginalIndex), null);
            }
        }

        return (true, null, ret);
    }

    private void GetAuthor(ScannedFMData fmData, List<string> titles)
    {
        if (fmData.Author.IsEmpty())
        {
            List<string>? passTitles = titles.Count > 0 ? titles : null;
            string author = GetValueFromReadme(SpecialLogic.Author, _ctx.SA_AuthorDetect, passTitles);
            fmData.Author = CleanupValue(author).Trim();
        }

        if (!fmData.Author.IsEmpty())
        {
            Match match = _ctx.AuthorEmailRegex.Match(fmData.Author);
            if (match.Success)
            {
                fmData.Author = fmData.Author.Remove(match.Index, match.Length).Trim();
            }

            if (fmData.Author.StartsWithI_Local("By "))
            {
                fmData.Author = fmData.Author.Substring(2).Trim();
            }
        }
    }

    private ScannedFMDataAndError
    ScanCurrentFM(FMToScan fm, string tempPath, string tempRandomName, CancellationToken cancellationToken)
    {
#if DEBUG
        _overallTimer.Restart();
#endif

        // Sometimes we'll want to remove this from the start of a string to get a relative path, so it's
        // critical that we always know we have a dir separator on the end so we don't end up with a leading
        // one on the string when we remove this from the start of it

        if (!_fmWorkingPath.EndsWithDirSep()) _fmWorkingPath += "\\";

        ulong sevenZipSize = 0;

        #region Setup

        ListFast<SevenZipArchiveEntry> sevenZipEntries = null!;
        SharpCompress.LazyReadOnlyCollection<RarArchiveEntry> rarEntries = null!;

        if (_fmFormat == FMFormat.Rar)
        {
            _rarStream = FileStreamCustom.CreateRead(fm.Path, DiskFileStreamBuffer);
            _rarArchive = RarArchive.Open(_rarStream);
            rarEntries = _rarArchive.Entries;
            // Load the lazy-loaded list so it doesn't throw later
            _ = rarEntries.Count;

            if (_rarArchive.IsSolid)
            {
                _fmFormat = FMFormat.RarSolid;
                _rarArchive.Dispose();
                _rarStream.Dispose();
                _rarStream = FileStreamCustom.CreateRead(fm.Path, DiskFileStreamBuffer);
            }
        }

        bool needsHtmlRefExtract = false;

        if (_fmFormat is FMFormat.SevenZip or FMFormat.RarSolid)
        {
            #region Partial solid archive extract

            /*
            Rather than extracting everything, we only extract files we might need. We may still end up
            extracting more than we need, but it's WAY less than just dumbly doing the whole thing. Over
            my limited set of 45 7z files, this makes us about 4x faster on average. Certain individual
            FMs may still be about as slow depending on their structure and content, but meh. Improvement
            is improvement.

            IMPORTANT(Scanner partial solid archive extract):
            The logic for deciding which files to extract (taking files and then de-duping the list) needs
            to match the logic for using them. If we change the usage logic, we need to change this too!
            */

            try
            {
                // Stupid micro-optimization:
                // Init them both just once, avoiding repeated null checks on the properties
                var fileNamesList = SolidExtractedFilesList;
                var tempList = SolidZipExtractedFilesTempList;

                static bool EndsWithTitleFile(string fileName)
                {
                    return fileName.PathEndsWithI("/titles.str") ||
                           fileName.PathEndsWithI("/title.str");
                }

                Directory.CreateDirectory(_fmWorkingPath);

                cancellationToken.ThrowIfCancellationRequested();

                /*
                We use SharpCompress for getting the file names and metadata, as that doesn't involve any
                decompression and won't trigger any out-of-memory errors. We use this so we can get last write
                times in DateTime format and not have to parse possible localized text dates out of the output
                stream.
                */
                using (FileStreamCustom fs = FileStreamCustom.CreateRead(fm.Path, DiskFileStreamBuffer))
                {
                    sevenZipSize = (ulong)fs.Length;
                    int entriesCount;
                    cancellationToken.ThrowIfCancellationRequested();

                    if (_fmFormat == FMFormat.SevenZip)
                    {
                        var sevenZipArchive = new SevenZipArchive(fs, _sevenZipContext);
                        sevenZipEntries = sevenZipArchive.Entries;
                        entriesCount = sevenZipEntries.Count;
                    }
                    else
                    {
                        entriesCount = rarEntries.Count;
                    }

                    _fmDirFileInfos.SetRecycleState(entriesCount);

                    for (int i = 0; i < entriesCount; i++)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        SevenZipArchiveEntry sevenZipEntry = null!;
                        RarArchiveEntry rarEntry = null!;
                        string fn;
                        long uncompressedSize;
                        if (_fmFormat == FMFormat.SevenZip)
                        {
                            sevenZipEntry = sevenZipEntries[i];
                            if (sevenZipEntry.IsAnti) continue;
                            fn = sevenZipEntry.FileName;
                            uncompressedSize = sevenZipEntry.UncompressedSize;
                        }
                        else
                        {
                            rarEntry = rarEntries[i];
                            fn = rarEntry.Key;
                            uncompressedSize = rarEntry.Size;
                        }

                        int dirSeps;

                        // Always extract readmes no matter what, so our cache copying is always correct.
                        // Also maybe we would need to always extract them regardless for other reasons, but yeah.
                        if (fn.IsValidReadme() && uncompressedSize > 0 &&
                            (((dirSeps = fn.Rel_CountDirSepsUpToAmount(2)) == 1 &&
                              (fn.PathStartsWithI(FMDirs.T3FMExtras1S) ||
                               fn.PathStartsWithI(FMDirs.T3FMExtras2S))) ||
                             dirSeps == 0))
                        {
                            fileNamesList.Add(fn);
                        }
                        /*
                        @SharpCompress(.mis and gam file perf thoughts):
                        -We extract all .mis files because we don't know which will be used until we read missflag.str.
                         But we should see if there's some heuristic we could use. We could guess on a single .mis file
                         to extract, and then if we're wrong we go back and extract what we know to be a used .mis file.
                         If we could do something better than blind guess-and-hope, that would be good.

                        @SharpCompress(.mis and gam file extracting tests):
                        -With only blindly getting the first .mis file we find, it's ~18.5s
                        -With getting only .mis files but not .gam files, it's ~22s
                        -With getting all .mis and .gam files, it's ~22.3s

                        So if we're somehow able to only get the first .mis file, we come out noticeably ahead,
                        at least for the full scan.

                        -Many unused .mis files are ~177KB (180745, 180749, 181428 and similar)
                        -Many, but not all, unused .mis files are extremely smaller than the other ones, <1MB
                        -But at least one is 12MB so whatever
                        -Checking the miss** dirs in the intrface dir is a good heuristic, but not perfect
                         (TM20AC_TPOAIR.zip has an "unused" miss20.mis but it has an intrface subdir)
                         But also, that "unused" miss20.mis is still valid and correct, containing "RopeyArrow",
                         and "SKYOBJVAR" is at the expected NewDark place
                        -We should create 7z files from the known unused-mis-containing zip set to test accuracy
                         with.
                        It's nasty business, but 22.3 to 18.5 is a good savings if we can do it...
                        -UPDATE 2023/3/26:
                         The intrface miss** dir heuristic is not good after all, there's tons of FMs with valid
                         mis files that don't have a matching intrface subdir.
                        -UPDATE 2024/2/6:
                         Turns out DarkLoader actually just uses the first .mis file and doesn't check it for
                         used or anything. I completely misunderstood and wrongly assumed right from the start.
                         In fact, that's probably why unused .mis files exist: Authors probably put a small dummy
                         .mis file as the first one so that DarkLoader would scan it and be faster than scanning
                         the real, larger one. So, I could just take the first one and all would be fine...
                         Unless some newer missions are depending on our previous behavior...
                        */
                        else if ((_scanOptions.ScanGameType
#if FMScanner_FullCode
                                  || _scanOptions.ScanNewDarkRequired
#endif
                                 ) &&
                                 !fn.Rel_ContainsDirSep() &&
                                 (fn.ExtIsMis()
                                 // Solid scans don't use .gam files, so shave a small amount of time off here
                                 //|| fn.ExtIsGam()
                                 ))
                        {
                            fileNamesList.Add(fn);
                        }
                        else if (!fn.Rel_ContainsDirSep() &&
                                 (fn.EqualsI_Local(FMFiles.FMInfoXml) ||
                                  fn.EqualsI_Local(FMFiles.FMIni) ||
                                  fn.EqualsI_Local(FMFiles.ModIni)))
                        {
                            fileNamesList.Add(fn);
                        }
                        else if (fn.PathStartsWithI(FMDirs.StringsS) &&
                                 fn.PathEndsWithI(FMFiles.SMissFlag))
                        {
                            fileNamesList.Add(fn);
                        }
                        else if (fn.PathEndsWithI(FMFiles.SNewGameStr))
                        {
                            fileNamesList.Add(fn);
                        }
                        else if (EndsWithTitleFile(fn))
                        {
                            fileNamesList.Add(fn);
                        }

                        FileInfoCustom fileInfoCustom = _fmDirFileInfos[i];
                        if (fileInfoCustom != null!)
                        {
                            if (_fmFormat == FMFormat.SevenZip)
                            {
                                fileInfoCustom.Set(sevenZipEntry);
                            }
                            else
                            {
                                fileInfoCustom.Set(rarEntry);
                            }
                        }
                        else
                        {
                            fileInfoCustom = _fmFormat == FMFormat.SevenZip
                                ? new FileInfoCustom(sevenZipEntry)
                                : new FileInfoCustom(rarEntry);
                            _fmDirFileInfos[i] = fileInfoCustom;
                        }
                    }
                }

                #region De-duplicate list

                // Some files could have multiple copies in different folders, but we only want to extract
                // the one we're going to use. We separate out this more complex and self-dependent logic
                // here. Doing this nonsense is still faster than extracting to disk.

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

                foreach (string titlesFileLocation in _ctx.FMFiles_TitlesStrLocations)
                {
                    string? titlesFileToUse = tempList.Find(x => x.PathEqualsI(titlesFileLocation));
                    if (titlesFileToUse != null)
                    {
                        fileNamesList.Add(titlesFileToUse);
                        break;
                    }
                }

                #endregion

                if (_fmFormat == FMFormat.SevenZip)
                {
                    string listFile = Path.Combine(tempPath, tempRandomName + ".7zl");

                    Fen7z.Result result = Fen7z.Extract(
                        sevenZipWorkingPath: _sevenZipWorkingPath,
                        sevenZipPathAndExe: _sevenZipExePath,
                        archivePath: fm.Path,
                        outputPath: _fmWorkingPath,
                        cancellationToken: cancellationToken,
                        listFile: listFile,
                        fileNamesList: fileNamesList);

                    cancellationToken.ThrowIfCancellationRequested();

                    if (result.ErrorOccurred)
                    {
                        Log(fm.Path + $": fm is 7z{NL}" +
                            "7z.exe path: " + _sevenZipExePath + $"{NL}" +
                            result);

                        return UnsupportedZip(
                            archivePath: fm.Path,
                            fen7zResult: result,
                            ex: null,
                            errorInfo: "7z.exe path: " + _sevenZipExePath + $"{NL}" +
                                       fm.Path + $": fm is 7z{NL}",
                            originalIndex: fm.OriginalIndex);
                    }
                }
                else
                {
                    HashSetI fileNamesHash = fileNamesList.ToHashSetI();

                    using var rarReader = RarReader.Open(_rarStream);

                    while (rarReader.MoveToNextEntry())
                    {
                        string fn = rarReader.Entry.Key;
                        if (fileNamesHash.Contains(fn))
                        {
                            string finalFileName = GetExtractedNameOrThrowIfMalicious(_fmWorkingPath, fn);
                            string dir = Path.GetDirectoryName(finalFileName)!;
                            Directory.CreateDirectory(dir);
                            rarReader.ExtractToFile_Fast(finalFileName, overwrite: true);
                        }
                    }
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                string fmType = _fmFormat switch
                {
                    FMFormat.SevenZip => "7z",
                    FMFormat.RarSolid => "rar (solid)",
                    _ => "rar",
                };
                string exType = _fmFormat switch
                {
                    FMFormat.SevenZip => "7z.exe",
                    FMFormat.RarSolid => "rar (solid)",
                    _ => "rar",
                };
                Log(fm.Path + ": fm is " + fmType + ", exception in " + exType + " extraction", ex);
                return UnsupportedZip(
                    archivePath: fm.Path,
                    fen7zResult: null,
                    ex: ex,
                    errorInfo: "7z.exe path: " + _sevenZipExePath + $"{NL}" +
                               fm.Path + ": fm is " + fmType + ", exception in " + exType + " extraction",
                    originalIndex: fm.OriginalIndex
                );
            }

            #endregion

            if (!fm.CachePath.IsEmpty())
            {
                try
                {
                    CopyReadmesToCacheResult result = CopyReadmesToCacheDir(fm, sevenZipEntries, rarEntries);
                    if (result == CopyReadmesToCacheResult.NeedsHtmlRefExtract)
                    {
                        needsHtmlRefExtract = true;
                    }
                }
                catch
                {
                    // ignore for now
                }
                cancellationToken.ThrowIfCancellationRequested();
            }
        }
        else if (_fmFormat == FMFormat.Zip)
        {
            Debug.WriteLine("----------" + fm.Path);

            var zipResult = ConstructZipArchive(fm, fm.Path, ZipContext, checkForZeroEntries: true);
            if (zipResult.Success)
            {
                _archive = zipResult.Archive!;
            }
            else if (zipResult.ScannedFMDataAndError != null)
            {
                return zipResult.ScannedFMDataAndError;
            }
        }
        else if (_fmFormat == FMFormat.NotInArchive)
        {
            if (!Directory.Exists(_fmWorkingPath))
            {
                Log(fm.Path + ": fm is dir, but " + nameof(_fmWorkingPath) +
                    " (" + _fmWorkingPath + ") doesn't exist. Returning 'Unsupported' game type.", stackTrace: false);
                return UnsupportedDir(null, null, "", fm.OriginalIndex);
            }
            Debug.WriteLine("----------" + _fmWorkingPath);
        }

        #endregion

        var fmData = new ScannedFMData
        {
            ArchiveName = _fmFormat > FMFormat.NotInArchive
                ? Path.GetFileName(fm.Path)
                : FMWorkingPathDirName,
        };

        bool scanTitleForAuthorPurposesOnly = SetupAuthorRequiredTitleScan();

        #region Size

        if (_scanOptions.ScanSize)
        {
            switch (_fmFormat)
            {
                case FMFormat.Zip:
                    fmData.Size = (ulong)_archive.ArchiveStreamLength;
                    break;
                case FMFormat.Rar:
                case FMFormat.RarSolid:
                    fmData.Size = (ulong)_rarStream.Length;
                    break;
                case FMFormat.SevenZip:
                    fmData.Size = sevenZipSize;
                    break;
                default:
                {
                    // Getting the size is horrendously expensive for folders, but if we're doing it then we can
                    // save some time later by using the FileInfo list as a cache.
                    FileInfo[] fileInfos = FMWorkingPathDirInfo.GetFiles("*", SearchOption.AllDirectories);

                    ulong size = 0;
                    _fmDirFileInfos.SetRecycleState(fileInfos.Length);
                    for (int i = 0; i < fileInfos.Length; i++)
                    {
                        FileInfo fileInfo = fileInfos[i];
                        FileInfoCustom? fileInfoCustom = _fmDirFileInfos[i];
                        if (fileInfoCustom != null!)
                        {
                            fileInfoCustom.Set(fileInfo);
                        }
                        else
                        {
                            fileInfoCustom = new FileInfoCustom(fileInfo);
                            _fmDirFileInfos[i] = fileInfoCustom;
                        }
                        size += (ulong)fileInfo.Length;
                    }
                    fmData.Size = size;
                    break;
                }
            }
        }

        #endregion

        #region Cache FM data

        bool success = ReadAndCacheFMData(fm.Path, fmData);
        if (!success)
        {
            string ext = _fmFormat switch
            {
                FMFormat.Zip => "zip",
                FMFormat.SevenZip => "7z",
                FMFormat.Rar => "rar",
                FMFormat.RarSolid => "rar (solid)",
                _ => "dir",
            };
            Log(fm.Path + ": fm is " + ext + ", " +
                nameof(ReadAndCacheFMData) + " returned false. Returning 'Unsupported' game type.", stackTrace: false);

            return _fmFormat > FMFormat.NotInArchive
                ? UnsupportedZip(fm.Path, null, null, "", fm.OriginalIndex)
                : UnsupportedDir(null, null, "", fm.OriginalIndex);
        }

        #endregion

        bool fmIsT3 = fmData.Game == Game.Thief3;

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

        bool singleMission = _usedMisFiles.Count == 1;

#if FMScanner_FullCode
        fmData.Type = singleMission ? FMType.FanMission : FMType.Campaign;
#endif

        fmData.MissionCount = _usedMisFiles.Count;

        if (_scanOptions.GetOptionsEnum() == ScanOptionsEnum.MissionCount)
        {
            // Early return for perf if we're not scanning anything else
            return new ScannedFMDataAndError(fm.OriginalIndex) { ScannedFMData = fmData };
        }

        List<string> titles = new();

        bool fmIsSS2 = false;

        if (!fmIsT3)
        {
            #region NewDark/GameType checks

            if (
#if FMScanner_FullCode
                _scanOptions.ScanNewDarkRequired ||
#endif
                _scanOptions.ScanGameType)
            {
#if FMScanner_FullCode
                var (newDarkRequired, game)
#else
                Game game
#endif
                    = GetGameTypeAndEngine();
#if FMScanner_FullCode
                if (_scanOptions.ScanNewDarkRequired) fmData.NewDarkRequired = newDarkRequired;
#endif
                if (_scanOptions.ScanGameType)
                {
                    fmData.Game = game;
                    if (fmData.Game == Game.Unsupported)
                    {
                        return new ScannedFMDataAndError(fm.OriginalIndex) { ScannedFMData = fmData };
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
                for (int i = 0; i < _baseDirFiles.Count; i++)
                {
                    NameAndIndex f = _baseDirFiles[i];
                    if (f.Name.EqualsI_Local(FMFiles.FMInfoXml))
                    {
                        var (title, author
#if FMScanner_FullCode
                            , version
#endif
                            , releaseDate) = ReadFMInfoXml(f);
                        if (_scanOptions.ScanTitle) SetOrAddTitle(titles, title);
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
                for (int i = 0; i < _baseDirFiles.Count; i++)
                {
                    NameAndIndex f = _baseDirFiles[i];
                    if (f.Name.EqualsI_Local(FMFiles.FMIni))
                    {
                        var (title, author
#if FMScanner_FullCode
                            , description
#endif
                            , lastUpdateDate, tags) = ReadFMIni(f);
                        if (_scanOptions.ScanTitle) SetOrAddTitle(titles, title);
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
                for (int i = 0; i < _baseDirFiles.Count; i++)
                {
                    NameAndIndex f = _baseDirFiles[i];
                    if (f.Name.EqualsI_Local(FMFiles.ModIni))
                    {
                        var (title, author) = ReadModIni(f);
                        if (_scanOptions.ScanTitle) SetOrAddTitle(titles, title);
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

        foreach (NameAndIndex f in _baseDirFiles)
        {
            _readmeDirFiles.Add(f);
        }

        if (fmIsT3)
        {
            foreach (NameAndIndex f in T3FMExtrasDirFiles)
            {
                _readmeDirFiles.Add(f);
            }
        }

        ReadAndCacheReadmeFiles();

        #endregion

#if FMScanner_FullCode
        if (!fmIsT3)
        {
            // This is here because it needs to come after the readmes are cached
            #region NewDark minimum required version

            if (fmData.NewDarkRequired == true && _scanOptions.ScanNewDarkMinimumVersion)
            {
                fmData.NewDarkMinRequiredVersion = GetNewDarkVersion();
            }

            #endregion
        }
#endif

        #region Set release date

        if (_scanOptions.ScanReleaseDate && fmData.LastUpdateDate == null)
        {
            fmData.LastUpdateDate = GetReleaseDate();
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
                var (titleFrom0, titleFromN
#if FMScanner_FullCode
                        , cNames
#endif
                        )
                    = GetMissionNames();
                if (_scanOptions.ScanTitle)
                {
                    SetOrAddTitle(titles, titleFrom0);
                    SetOrAddTitle(titles, titleFromN);
                }
#if FMScanner_FullCode
                if (_scanOptions.ScanCampaignMissionNames && cNames != null && cNames.Count > 0)
                {
                    for (int i = 0; i < cNames.Count; i++)
                    {
                        cNames[i] = CleanupTitle(cNames[i]);
                    }
                    fmData.IncludedMissions = cNames.ToArray();
                }
#endif
            }
        }

        if (_scanOptions.ScanTitle)
        {
            SetOrAddTitle(titles, GetValueFromReadme(SpecialLogic.Title, _ctx.SA_TitleDetect));

            if (!fmIsT3) SetOrAddTitle(titles, GetTitleFromNewGameStrFile());

            EndTitleScan(scanTitleForAuthorPurposesOnly, fmData, titles);
        }

        #endregion

        #region Author

        if (_scanOptions.ScanAuthor || _scanOptions.ScanTags)
        {
            GetAuthor(fmData, titles);
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
                var getLangs = GetLanguages();
                if (getLangs.Langs > Language.Default) SetLangTags(fmData, getLangs.Langs, getLangs.EnglishIsUncertain);
#if FMScanner_FullCode
                if (!_scanOptions.ScanLanguages)
                {
                    fmData.Languages = Array.Empty<string>();
                }
                else
                {
                    var langsList = new List<string>(SupportedLanguageCount);

                    for (int i = 0; i < SupportedLanguageCount; i++)
                    {
                        Language language = LanguageIndexToLanguage((LanguageIndex)i);
                        if (getLangs.Langs.HasFlagFast(language))
                        {
                            langsList.Add(SupportedLanguages[i]);
                        }
                    }

                    fmData.Languages = langsList.ToArray();
                    Array.Sort(fmData.Languages);
                }
#endif
            }

            #endregion
        }

        if (_scanOptions.ScanTags)
        {
            if (!singleMission) AddCampaignTag(fmData);

            if (!fmData.Author.IsEmpty())
            {
                int ai = fmData.Author.IndexOf(' ');
                if (ai == -1) ai = fmData.Author.IndexOf('-');
                if (ai == -1) ai = fmData.Author.Length;
                string anonAuthor = fmData.Author.Substring(0, ai);
                if (anonAuthor.EqualsI_Local("Anon") ||
                    anonAuthor.EqualsI_Local("Withheld") ||
                    anonAuthor.EqualsI_Local("Unknown") ||
                    anonAuthor.SimilarityTo("Anonymous", _sevenZipContext) > 0.75)
                {
                    SetMiscTag(fmData, "unknown author");
                }
            }

            if (!_scanOptions.ScanAuthor) fmData.Author = "";
        }

#if DEBUG
        _overallTimer.Stop();
        Debug.WriteLine(@"This FM took:\r\n" + _overallTimer.Elapsed.ToString(@"hh\:mm\:ss\.fffffff"));
#endif

        return new ScannedFMDataAndError(fm.OriginalIndex) { ScannedFMData = fmData, NeedsHtmlRefExtract = needsHtmlRefExtract };
    }

    #region Fail return functions

    private static ScannedFMDataAndError UnsupportedTDM(
        string archivePath,
        Fen7z.Result? fen7zResult,
        Exception? ex,
        string errorInfo,
        int originalIndex) =>
        new(originalIndex)
        {
            ScannedFMData = new ScannedFMData
            {
                ArchiveName = Path.GetFileName(archivePath),
                Game = Game.Unsupported,
                MissionCount = 0,
            },
            Fen7zResult = fen7zResult,
            Exception = ex,
            ErrorInfo = errorInfo,
        };

    private static ScannedFMDataAndError UnsupportedZip(
        string archivePath,
        Fen7z.Result? fen7zResult,
        Exception? ex,
        string errorInfo,
        int originalIndex) =>
        new(originalIndex)
        {
            ScannedFMData = new ScannedFMData
            {
                ArchiveName = Path.GetFileName(archivePath),
                Game = Game.Unsupported,
                MissionCount = 0,
            },
            Fen7zResult = fen7zResult,
            Exception = ex,
            ErrorInfo = errorInfo,
        };

    private static ScannedFMDataAndError UnknownZip(
        string archivePath,
        Fen7z.Result? fen7zResult,
        Exception? ex,
        string errorInfo,
        int originalIndex) =>
        new(originalIndex)
        {
            ScannedFMData = new ScannedFMData
            {
                ArchiveName = Path.GetFileName(archivePath),
                Game = Game.Null,
                MissionCount = 0,
            },
            Fen7zResult = fen7zResult,
            Exception = ex,
            ErrorInfo = errorInfo,
        };

    private static ScannedFMDataAndError UnsupportedDir(
        Fen7z.Result? fen7zResult,
        Exception? ex,
        string errorInfo,
        int originalIndex) =>
        new(originalIndex)
        {
            ScannedFMData = null,
            Fen7zResult = fen7zResult,
            Exception = ex,
            ErrorInfo = errorInfo,
        };

    #endregion

    private CopyReadmesToCacheResult CopyReadmesToCacheDir(
        FMToScan fm,
        ListFast<SevenZipArchiveEntry> sevenZipEntries,
        SharpCompress.LazyReadOnlyCollection<RarArchiveEntry> rarEntries)
    {
        string cachePath = fm.CachePath;

        Directory.CreateDirectory(cachePath);

        var readmes = new List<(string Source, string Dest)>();

        foreach (string f in Directory.GetFiles(_fmWorkingPath, "*", SearchOption.TopDirectoryOnly))
        {
            if (f.IsValidReadme())
            {
                readmes.Add((f, Path.Combine(cachePath, Path.GetFileName(f))));
            }
        }

        for (int i = 0; i < 2; i++)
        {
            string readmeDir = i == 0 ? FMDirs.T3FMExtras1S : FMDirs.T3FMExtras2S;
            string readmePathFull = Path.Combine(_fmWorkingPath, readmeDir);

            if (Directory.Exists(readmePathFull))
            {
                string cachePathReadmeDir = Path.Combine(cachePath, readmeDir);

                Directory.CreateDirectory(cachePathReadmeDir);

                foreach (string f in Directory.GetFiles(readmePathFull, "*", SearchOption.TopDirectoryOnly))
                {
                    if (f.IsValidReadme())
                    {
                        readmes.Add((f, Path.Combine(cachePathReadmeDir, Path.GetFileName(f))));
                    }
                }
            }
        }

        if (readmes.Count > 0)
        {
            string[] readmeFileNames = new string[readmes.Count];
            for (int i = 0; i < readmes.Count; i++)
            {
                readmeFileNames[i] = readmes[i].Source;
            }

            List<string> archiveFileNames;
            if (_fmFormat == FMFormat.SevenZip)
            {
                archiveFileNames = new List<string>(sevenZipEntries.Count);
                for (int i = 0; i < sevenZipEntries.Count; i++)
                {
                    archiveFileNames.Add(Path.GetFileName(sevenZipEntries[i].FileName));
                }
            }
            else
            {
                archiveFileNames = new List<string>(rarEntries.Count);
                for (int i = 0; i < rarEntries.Count; i++)
                {
                    archiveFileNames.Add(Path.GetFileName(rarEntries[i].Key));
                }
            }

            if (HtmlNeedsReferenceExtract(readmeFileNames, archiveFileNames))
            {
                /*
                We don't want to handle HTML ref extracts here - it would complicate the code too much. So just
                send a message back telling the caller to handle it.
                We could just delete the cache directory here, but since it could be anything, we're considering
                that a potentially unsafe action.
                */
                return CopyReadmesToCacheResult.NeedsHtmlRefExtract;
            }

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

        return CopyReadmesToCacheResult.Success;
    }

    #region Dates

    [StructLayout(LayoutKind.Auto)]
    private readonly ref struct ParsedDateTime
    {
        private readonly bool _isAmbiguous;
        internal readonly DateTime? Date;

        [MemberNotNullWhen(true, nameof(Date))]
        internal bool IsAmbiguous => Date != null && _isAmbiguous;

        internal ParsedDateTime(DateTime? date, bool isAmbiguous)
        {
            _isAmbiguous = isAmbiguous;
            Date = date;
        }
    }

    [StructLayout(LayoutKind.Auto)]
    private readonly ref struct MisFileDateTime
    {
        private readonly bool _succeeded;
        internal readonly DateTime? Date;

        [MemberNotNullWhen(true, nameof(Date))]
        internal bool Succeeded => Date != null && _succeeded;

        internal MisFileDateTime(bool succeeded, DateTime? date)
        {
            _succeeded = succeeded;
            Date = date;
        }
    }

    private DateTime? GetReleaseDate()
    {
        ParsedDateTime GetReadmeParsedDateTime()
        {
            DateTime? topDT = GetReleaseDateFromTopOfReadmes(out bool topDtIsAmbiguous);

            // Search for updated dates FIRST, because they'll be the correct ones!
            string ds = GetValueFromReadme(SpecialLogic.ReleaseDate, _ctx.SA_LatestUpdateDateDetect);
            DateTime? dt = null;
            bool dtIsAmbiguous = false;
            if (!ds.IsEmpty()) StringToDate(ds, checkForAmbiguity: true, out dt, out dtIsAmbiguous);

            if (ds.IsEmpty() || dt == null)
            {
                ds = GetValueFromReadme(SpecialLogic.ReleaseDate, _ctx.SA_ReleaseDateDetect);
            }

            if (!ds.IsEmpty()) StringToDate(ds, checkForAmbiguity: true, out dt, out dtIsAmbiguous);

            if (topDT != null && dt != null)
            {
                // TODO: We don't check the ambiguous date against the file(s) in this case
                // So we just take the non-ambiguous one even if it may be older. We could fix that if we felt
                // like we needed to.
                if (!topDtIsAmbiguous && dtIsAmbiguous)
                {
                    return new ParsedDateTime(topDT, false);
                }
                else if (!dtIsAmbiguous && topDtIsAmbiguous)
                {
                    return new ParsedDateTime(dt, false);
                }
                else if (DateTime.Compare((DateTime)topDT, (DateTime)dt) > 0)
                {
                    return new ParsedDateTime(topDT, topDtIsAmbiguous);
                }
                else
                {
                    return new ParsedDateTime(dt, dtIsAmbiguous);
                }
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

        static MisFileDateTime GetMisFileDate(Scanner scanner, List<NameAndIndex> usedMisFiles)
        {
            if (usedMisFiles.Count > 0)
            {
                DateTime misFileDate;
                if (scanner._fmFormat == FMFormat.Zip)
                {
                    misFileDate = new DateTimeOffset(ZipHelpers.ZipTimeToDateTime(
                        scanner._archive.Entries[usedMisFiles[0].Index].LastWriteTime)).DateTime;
                }
                else if (scanner._fmFormat == FMFormat.SevenZip ||
                         scanner._fmFormat == FMFormat.RarSolid ||
                         scanner._fmDirFileInfos.Count > 0)
                {
                    misFileDate = new DateTimeOffset(scanner._fmDirFileInfos[usedMisFiles[0].Index].LastWriteTime).DateTime;
                }
                else
                {
                    var fi = new FileInfo(Path.Combine(scanner._fmWorkingPath, usedMisFiles[0].Name));
                    misFileDate = new DateTimeOffset(fi.LastWriteTime).DateTime;
                }

                return misFileDate.Year > 1998
                    ? new MisFileDateTime(true, misFileDate)
                    : new MisFileDateTime(false, null);
            }

            return new MisFileDateTime(false, null);
        }

        static DateTime? GetFileDateTime(DateTime fileLastModifiedDate, DateTime readmeParsedDate)
        {
            if (fileLastModifiedDate.Year == readmeParsedDate.Year)
            {
                if (fileLastModifiedDate.Month == readmeParsedDate.Month &&
                    fileLastModifiedDate.Day == readmeParsedDate.Day)
                {
                    return readmeParsedDate;
                }
                else if ((fileLastModifiedDate.Day == readmeParsedDate.Month &&
                          Math.Abs(fileLastModifiedDate.Month - readmeParsedDate.Day) <= 3) ||
                         (fileLastModifiedDate.Month == readmeParsedDate.Day &&
                          Math.Abs(fileLastModifiedDate.Day - readmeParsedDate.Month) <= 3))
                {
                    return new DateTime(
                        year: readmeParsedDate.Year,
                        month: readmeParsedDate.Day,
                        day: readmeParsedDate.Month,
                        hour: readmeParsedDate.Hour,
                        minute: readmeParsedDate.Minute,
                        second: readmeParsedDate.Second,
                        millisecond: readmeParsedDate.Millisecond,
                        kind: readmeParsedDate.Kind
                    );
                }
            }

            return null;
        }

        MisFileDateTime misFileDateTime = new(false, null);

        ParsedDateTime parsedDateTime = GetReadmeParsedDateTime();

        if (parsedDateTime.IsAmbiguous)
        {
            DateTime readmeParsedDate = (DateTime)parsedDateTime.Date;

            for (int i = 0; i < _readmeFiles.Count; i++)
            {
                ReadmeInternal readme = _readmeFiles[i];
                DateTime readmeLastModifiedDate = readme.LastModifiedDate;

                if (readme.UseForDateDetect && readmeLastModifiedDate.Year > 1998)
                {
                    DateTime? finalDate = GetFileDateTime(readmeLastModifiedDate, readmeParsedDate);
                    if (finalDate != null) return finalDate;
                }
            }

            misFileDateTime = GetMisFileDate(this, _usedMisFiles);
            if (misFileDateTime.Succeeded)
            {
                DateTime misFileLastModifiedDate = (DateTime)misFileDateTime.Date;

                if (misFileLastModifiedDate.Year > 1998)
                {
                    DateTime? finalDate = GetFileDateTime(misFileLastModifiedDate, readmeParsedDate);
                    if (finalDate != null) return finalDate;
                }
            }
        }

        if (parsedDateTime.Date != null) return parsedDateTime.Date;

        for (int i = 0; i < _readmeFiles.Count; i++)
        {
            ReadmeInternal readme = _readmeFiles[i];
            if (readme.LastModifiedDate.Year > 1998 && readme.UseForDateDetect)
            {
                return readme.LastModifiedDate;
            }
        }

        return misFileDateTime.Succeeded
            ? misFileDateTime.Date.Value
            : GetMisFileDate(this, _usedMisFiles).Date;
    }

    private DateTime? GetReleaseDateFromTopOfReadmes(out bool isAmbiguous)
    {
        // Always false for now, because we only return dates that have month names in them currently
        // (was I concerned about number-only dates having not enough context to be sure they're dates?)
        isAmbiguous = false;

        if (_readmeFiles.Count == 0) return null;

        foreach (ReadmeInternal r in _readmeFiles)
        {
            if (!r.Scan) continue;

            int topLineCount = 0;
            for (int i = 0; i < r.Lines.Count; i++)
            {
                string lineT = r.Lines[i].Trim();

                if (lineT.IsWhiteSpace()) continue;

                if (LineContainsMonthName(lineT, _ctx._monthNames))
                {
                    if (StringToDate(lineT, checkForAmbiguity: false, out DateTime? result, out _))
                    {
                        return result;
                    }
                }

                topLineCount++;
                if (topLineCount == _maxTopLines) break;
            }
        }

        return null;

        static bool LineContainsMonthName(string line, string[] monthNames)
        {
            foreach (string item in monthNames)
            {
                if (line.ContainsI(item))
                {
                    return true;
                }
            }
            return false;
        }
    }

    // TODO(Scanner/StringToDate()): Shouldn't we ALWAYS check for ambiguity...?
    private bool StringToDate(string dateString, bool checkForAmbiguity, [NotNullWhen(true)] out DateTime? dateTime, out bool isAmbiguous)
    {
        // If a date has dot separators, it's probably European format, so we can up our accuracy with regard
        // to guessing about day/month order.
        if (_ctx.EuropeanDateRegex.Match(dateString).Success)
        {
            string dateStringTemp = _ctx.PeriodWithOptionalSurroundingSpacesRegex.Replace(dateString, ".").Trim(_ctx.CA_Period);
            if (DateTime.TryParseExact(
                    dateStringTemp,
                    _ctx._dateFormatsEuropean,
                    DateTimeFormatInfo.InvariantInfo,
                    DateTimeStyles.None,
                    out DateTime eurDateResult))
            {
                dateTime = eurDateResult;
                isAmbiguous = eurDateResult.Month != eurDateResult.Day;
                return true;
            }
        }

        dateString = _ctx.DateSeparatorsRegex.Replace(dateString, " ");
        dateString = _ctx.DateOfSeparatorRegex.Replace(dateString, " ");
        dateString = _ctx.OneOrMoreWhiteSpaceCharsRegex.Replace(dateString, " ");

        dateString = _ctx.FebrRegex.Replace(dateString, "Feb ");
        dateString = _ctx.SeptRegex.Replace(dateString, "Sep ");
        dateString = _ctx.OktRegex.Replace(dateString, "Oct ");

        dateString = _ctx.HalloweenRegex.Replace(dateString, "Oct 31");
        dateString = _ctx.ChristmasRegex.Replace(dateString, "Dec 25");

        // Cute...
        dateString = _ctx.Y2KRegex.Replace(dateString, "2000");

        dateString = _ctx.JanuaryVariationsRegex.Replace(dateString, "Jan");
        dateString = _ctx.FebruaryVariationsRegex.Replace(dateString, "Feb");
        dateString = _ctx.MarchVariationsRegex.Replace(dateString, "Mar");
        dateString = _ctx.AprilVariationsRegex.Replace(dateString, "Apr");
        dateString = _ctx.MayVariationsRegex.Replace(dateString, "May");
        dateString = _ctx.JuneVariationsRegex.Replace(dateString, "Jun");
        dateString = _ctx.JulyVariationsRegex.Replace(dateString, "Jul");
        dateString = _ctx.AugustVariationsRegex.Replace(dateString, "Aug");
        dateString = _ctx.SeptemberVariationsRegex.Replace(dateString, "Sep");
        dateString = _ctx.OctoberVariationsRegex.Replace(dateString, "Oct");
        dateString = _ctx.NovemberVariationsRegex.Replace(dateString, "Nov");
        dateString = _ctx.DecemberVariationsRegex.Replace(dateString, "Dec");

        dateString = dateString.Trim(_ctx.CA_Period);
        dateString = dateString.Trim(_ctx.CA_Parens);
        dateString = dateString.Trim();

        // Remove "st", "nd", "rd, "th" if present, as DateTime.TryParse() will choke on them
        Match match = _ctx.DaySuffixesRegex.Match(dateString);
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
        foreach (var item in _ctx._dateFormats)
        {
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
            if (result is { } resultNotNull && resultNotNull.Month == resultNotNull.Day)
            {
                isAmbiguous = false;
                dateTime = resultNotNull;
                return true;
            }

            string[] nums = dateString.Split_Char(_ctx.CA_DateSeparators, StringSplitOptions.RemoveEmptyEntries, _sevenZipContext.IntArrayPool);
            if (nums.Length == 3)
            {
                bool unambiguousYearFound = false;
                bool unambiguousDayFound = false;

                foreach (string num in nums)
                {
                    if (Int_TryParseInv(num, out int numInt))
                    {
                        switch (numInt)
                        {
                            case 0 or (> 31 and <= 9999):
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

    private void SetLangTags(ScannedFMData fmData, Language langs, bool englishIsUncertain)
    {
        if (langs == Language.Default) return;

        if (fmData.TagsString.IsWhiteSpace()) fmData.TagsString = "";
        for (int i = 0; i < SupportedLanguageCount; i++)
        {
            LanguageIndex languageIndex = (LanguageIndex)i;

            if (!langs.HasFlagFast(LanguageIndexToLanguage(languageIndex)))
            {
                continue;
            }

            string lang = SupportedLanguages[i];

            Debug.Assert(lang == lang.ToLowerInvariant(),
                "lang != lang.ToLowerInvariant() - lang is not lowercase");

            if (englishIsUncertain && languageIndex == LanguageIndex.English) continue;

            if (fmData.TagsString.Contains(lang, Ordinal))
            {
                fmData.TagsString = Regex.Replace(fmData.TagsString, @":\s*" + lang, ":" + _ctx.LanguagesC[i]);
            }

            // PERF: 5ms over the whole 1098 set, whatever
            Match match = Regex.Match(fmData.TagsString, @"language:\s*" + lang, IgnoreCaseInvariant);
            if (match.Success) continue;

            if (fmData.TagsString != "") fmData.TagsString += ", ";
            fmData.TagsString += "language:" + _ctx.LanguagesC[i];
        }
    }

    private void AddCampaignTag(ScannedFMData fmData) => SetMiscTag(fmData, "campaign");

    private void SetMiscTag(ScannedFMData fmData, string tag)
    {
        if (fmData.TagsString.IsWhiteSpace()) fmData.TagsString = "";

        List<string> list = fmData.TagsString.Split_Char(CA_CommaSemicolon, StringSplitOptions.None, _sevenZipContext.IntArrayPool).ToList();
        bool tagFound = false;
        for (int i = 0; i < list.Count; i++)
        {
            list[i] = list[i].Trim();
            if (list[i].IsEmpty())
            {
                list.RemoveAt(i);
                i--;
            }
            else if (list[i].EqualsI_Local(tag))
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

    private bool ReadAndCacheFMData(string fmPath, ScannedFMData fmd)
    {
        #region Add BaseDirFiles

        bool t3Found = false;

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

        static bool FileExtensionFound(string fn, string[] extensions)
        {
            foreach (string extension in extensions)
            {
                if (Utility.EndsWithI_Local(fn.AsSpan(), extension))
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

        if (_fmFormat > FMFormat.NotInArchive || _fmDirFileInfos.Count > 0)
        {
            int filesCount = _fmFormat switch
            {
                FMFormat.Zip => _archive.Entries.Count,
                FMFormat.Rar => _rarArchive.Entries.Count,
                _ => _fmDirFileInfos.Count,
            };
            for (int i = 0; i < filesCount; i++)
            {
                string fn = _fmFormat switch
                {
                    FMFormat.Zip => _archive.Entries[i].FullName,
                    FMFormat.SevenZip or FMFormat.RarSolid => _fmDirFileInfos[i].FullName,
                    FMFormat.Rar => _rarArchive.Entries[i].Key,
                    _ => _fmDirFileInfos[i].FullName.Substring(_fmWorkingPath.Length),
                };

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
                                T3GmpFiles.Add(new NameAndIndex(Path.GetFileName(fn), i));
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
                                T3GmpFiles.Add(new NameAndIndex(Path.GetFileName(fn), i));
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
                    T3FMExtrasDirFiles.Add(new NameAndIndex(fn, i));
                    continue;
                }
                else if (!fn.Rel_ContainsDirSep() && fn.Contains('.'))
                {
                    _baseDirFiles.Add(new NameAndIndex(fn, i));
                    // Fallthrough so ScanCustomResources can use it
                }
                else if (!t3Found && fn.PathStartsWithI(FMDirs.StringsS))
                {
                    _stringsDirFiles.Add(new NameAndIndex(fn, i));
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
                    _intrfaceDirFiles.Add(new NameAndIndex(fn, i));
                    // Fallthrough so ScanCustomResources can use it
                }
                else if (!t3Found && fn.PathStartsWithI(FMDirs.BooksS))
                {
                    _booksDirFiles.Add(new NameAndIndex(fn, i));
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
                             FileExtensionFound(fn, _ctx.MotionFileExtensions))
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
                             FileExtensionFound(fn, _ctx.ImageFileExtensions))
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
                              FileExtensionFound(fn, _ctx.ScriptFileExtensions)) ||
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
                if (_baseDirFiles.Count == 0)
                {
                    Log(fmPath + ": 'fm is archive' or 'scanning size' codepath: No files in base dir. Returning false.", stackTrace: false);
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
        else // Dir only; solid is now handled up there as well
        {
            string t3DetectPath = Path.Combine(_fmWorkingPath, FMDirs.T3DetectS);
            if (Directory.Exists(t3DetectPath) &&
                FastIO.FilesExistSearchTop(t3DetectPath, _ctx.SA_T3DetectExtensions))
            {
                t3Found = true;
                if (_scanOptions.ScanMissionCount)
                {
                    foreach (string f in EnumFiles(FMDirs.T3DetectS, SearchOption.TopDirectoryOnly))
                    {
                        if (f.ExtIsGmp())
                        {
                            // We only want the filename; we already know it's in the right folder
                            T3GmpFiles.Add(new NameAndIndex(Path.GetFileName(f)));
                        }
                    }
                }
                fmd.Game = Game.Thief3;
            }

            foreach (string f in Directory.GetFiles(_fmWorkingPath, "*", SearchOption.TopDirectoryOnly))
            {
                _baseDirFiles.Add(new NameAndIndex(Path.GetFileName(f)));
            }

            if (t3Found)
            {
                foreach (string f in EnumFiles(FMDirs.T3FMExtras1S, SearchOption.TopDirectoryOnly))
                {
                    T3FMExtrasDirFiles.Add(new NameAndIndex(f.Substring(_fmWorkingPath.Length)));
                }

                foreach (string f in EnumFiles(FMDirs.T3FMExtras2S, SearchOption.TopDirectoryOnly))
                {
                    T3FMExtrasDirFiles.Add(new NameAndIndex(f.Substring(_fmWorkingPath.Length)));
                }
            }
            else
            {
                if (_baseDirFiles.Count == 0)
                {
                    Log(fmPath + ": 'fm is dir' codepath: No files in base dir. Returning false.", stackTrace: false);
                    return false;
                }

                foreach (string f in EnumFiles(FMDirs.StringsS, SearchOption.AllDirectories))
                {
                    _stringsDirFiles.Add(new NameAndIndex(f.Substring(_fmWorkingPath.Length)));
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
                    _intrfaceDirFiles.Add(new NameAndIndex(f.Substring(_fmWorkingPath.Length)));
                }

                foreach (string f in EnumFiles(FMDirs.BooksS, SearchOption.AllDirectories))
                {
                    _booksDirFiles.Add(new NameAndIndex(f.Substring(_fmWorkingPath.Length)));
                }

                if (SS2FingerprintRequiredAndNotDone() || _scanOptions.ScanCustomResources)
                {
                    // I tried getting rid of this GetDirectories call, but it made things more complicated
                    // for SS2 fingerprinting and didn't result in a clear perf win. At least not warm. Meh.
                    string[] baseDirFolders = Directory.GetDirectories(_fmWorkingPath, "*", SearchOption.TopDirectoryOnly);
                    for (int i = 0; i < baseDirFolders.Length; i++)
                    {
                        // @DIRSEP: Even for UNC paths, FM working path has to be at least like \\netPC\some_directory
                        // and we're getting dirs inside that, so it'll be at least \\netPC\some_directory\other
                        // so we'll always end up with "other" (for example). So we're safe here.
                        string folder = baseDirFolders[i];
                        baseDirFolders[i] = folder.Substring(folder.Rel_LastIndexOfDirSep() + 1);
                    }

                    if (_scanOptions.ScanCustomResources)
                    {
                        foreach (NameAndIndex f in _intrfaceDirFiles)
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
                            FastIO.FilesExistSearchAll(Path.Combine(_fmWorkingPath, FMDirs.Motions), _ctx.MotionFilePatterns);

                        fmd.HasMovies =
                            (baseDirFolders.ContainsI(FMDirs.Movies) &&
                             FastIO.FilesExistSearchAll(Path.Combine(_fmWorkingPath, FMDirs.Movies), _ctx.SA_AllFiles)) ||
                            (baseDirFolders.ContainsI(FMDirs.Cutscenes) &&
                             FastIO.FilesExistSearchAll(Path.Combine(_fmWorkingPath, FMDirs.Cutscenes), _ctx.SA_AllFiles));

                        fmd.HasCustomTextures =
                            baseDirFolders.ContainsI(FMDirs.Fam) &&
                            FastIO.FilesExistSearchAll(Path.Combine(_fmWorkingPath, FMDirs.Fam), _ctx.ImageFilePatterns);

                        fmd.HasCustomObjects =
                            baseDirFolders.ContainsI(FMDirs.Obj) &&
                            FastIO.FilesExistSearchAll(Path.Combine(_fmWorkingPath, FMDirs.Obj), _ctx.SA_AllBinFiles);

                        fmd.HasCustomCreatures =
                            baseDirFolders.ContainsI(FMDirs.Mesh) &&
                            FastIO.FilesExistSearchAll(Path.Combine(_fmWorkingPath, FMDirs.Mesh), _ctx.SA_AllBinFiles);

                        fmd.HasCustomScripts =
                            BaseDirScriptFileExtensions(_baseDirFiles, _ctx.ScriptFileExtensions) ||
                            (baseDirFolders.ContainsI(FMDirs.Scripts) &&
                             FastIO.FilesExistSearchAll(Path.Combine(_fmWorkingPath, FMDirs.Scripts), _ctx.SA_AllFiles));

                        fmd.HasCustomSounds =
                            (baseDirFolders.ContainsI(FMDirs.Snd) &&
                             FastIO.FilesExistSearchAll(Path.Combine(_fmWorkingPath, FMDirs.Snd), _ctx.SA_AllFiles)) ||
                            (baseDirFolders.ContainsI(FMDirs.Snd2) &&
                             FastIO.FilesExistSearchAll(Path.Combine(_fmWorkingPath, FMDirs.Snd2), _ctx.SA_AllFiles));

                        fmd.HasCustomSubtitles =
                            baseDirFolders.ContainsI(FMDirs.Subtitles) &&
                            FastIO.FilesExistSearchAll(Path.Combine(_fmWorkingPath, FMDirs.Subtitles), _ctx.SA_AllSubFiles);
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
                switch (T3GmpFiles.Count)
                {
                    case 1:
                        _usedMisFiles.Add(T3GmpFiles[0]);
                        break;
                    case > 1:
                        for (int i = 0; i < T3GmpFiles.Count; i++)
                        {
                            NameAndIndex item = T3GmpFiles[i];
                            if (!item.Name.EqualsI_Local(FMFiles.EntryGmp))
                            {
                                _usedMisFiles.Add(item);
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

        for (int i = 0; i < _baseDirFiles.Count; i++)
        {
            NameAndIndex f = _baseDirFiles[i];
            if (f.Name.ExtIsMis())
            {
                _misFiles.Add(new NameAndIndex(Path.GetFileName(f.Name), f.Index));
            }
        }

        if (_misFiles.Count == 0)
        {
            Log(fmPath + ": No .mis files in base dir. Returning false.", stackTrace: false);
            return false;
        }

        #endregion

        #region Cache list of used .mis files

        int missFlagIndex = -1;
        if (_stringsDirFiles.Count > 0)
        {
            // I don't remember if I need to search in this exact order, so uh... not rockin' the boat.
            for (int i = 0; i < _stringsDirFiles.Count; i++)
            {
                NameAndIndex item = _stringsDirFiles[i];
                if (item.Name.PathEqualsI(FMFiles.StringsMissFlag))
                {
                    missFlagIndex = i;
                    break;
                }
            }
            if (missFlagIndex == -1)
            {
                for (int i = 0; i < _stringsDirFiles.Count; i++)
                {
                    NameAndIndex item = _stringsDirFiles[i];
                    if (item.Name.PathEqualsI(FMFiles.StringsEnglishMissFlag))
                    {
                        missFlagIndex = i;
                        break;
                    }
                }
            }
            if (missFlagIndex == -1)
            {
                for (int i = 0; i < _stringsDirFiles.Count; i++)
                {
                    NameAndIndex item = _stringsDirFiles[i];
                    if (item.Name.PathEndsWithI(FMFiles.SMissFlag))
                    {
                        missFlagIndex = i;
                        break;
                    }
                }
            }
        }

        if (missFlagIndex > -1)
        {
            // missflag.str files are always ASCII / UTF8, so we can avoid an expensive encoding detect here
            ReadAllLinesUTF8(_stringsDirFiles[missFlagIndex], _tempLines);

            for (int mfI = 0; mfI < _misFiles.Count; mfI++)
            {
                NameAndIndex mf = _misFiles[mfI];

                // Obtuse nonsense to avoid string allocations (perf)
                if (mf.Name.StartsWithI_Local("miss") && mf.Name[4] != '.')
                {
                    // Since only files ending in .mis are in the misFiles list, we're guaranteed to find a .
                    // character and not get a -1 index. And since we know our file starts with "miss", the
                    // -4 is guaranteed not to take us negative either.
                    int count = mf.Name.IndexOf('.') - 4;
                    for (int mflI = 0; mflI < _tempLines.Count; mflI++)
                    {
                        string line = _tempLines[mflI];
                        if (line.StartsWithI_Local("miss_") && line.Length > 5 + count && line[5 + count] == ':')
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
                                    _usedMisFiles.Add(mf);
                                }
                            }
                        }
                    }
                }
            }
        }

        if (_usedMisFiles.Count == 0) _usedMisFiles.AddRange_Small(_misFiles);

        #endregion

        return true;
    }

    #region Read FM info files

    private (string Title, string Author
#if FMScanner_FullCode
        , string Version
#endif
        , DateTime? ReleaseDate)
    ReadFMInfoXml(NameAndIndex file)
    {
        string title = "";
        string author = "";
#if FMScanner_FullCode
        string version = "";
#endif
        DateTime? releaseDate = null;

        var fmInfoXml = new XmlDocument();

        #region Load XML

        if (_fmFormat == FMFormat.Zip)
        {
            using var es = _archive.OpenEntry(_archive.Entries[file.Index]);
            fmInfoXml.Load(es);
        }
        else if (_fmFormat == FMFormat.Rar)
        {
            using var es = _rarArchive.Entries[file.Index].OpenEntryStream();
            fmInfoXml.Load(es);
        }
        else
        {
            fmInfoXml.Load(Path.Combine(_fmWorkingPath, file.Name));
        }

        #endregion

        if (_scanOptions.ScanTitle)
        {
            using XmlNodeList xTitle = fmInfoXml.GetElementsByTagName("title");
            if (xTitle.Count > 0) title = xTitle[0].GetPlainInnerText();
        }

        if (_scanOptions.ScanTags || _scanOptions.ScanAuthor)
        {
            using XmlNodeList xAuthor = fmInfoXml.GetElementsByTagName("author");
            if (xAuthor.Count > 0) author = xAuthor[0].GetPlainInnerText();
        }

#if FMScanner_FullCode
        if (_scanOptions.ScanVersion)
        {
            using XmlNodeList xVersion = fmInfoXml.GetElementsByTagName("version");
            if (xVersion.Count > 0) version = xVersion[0].GetPlainInnerText();
        }
#endif

        using XmlNodeList xReleaseDate = fmInfoXml.GetElementsByTagName("releasedate");
        if (xReleaseDate.Count > 0)
        {
            string rdString = xReleaseDate[0].GetPlainInnerText();
            if (!rdString.IsEmpty()) releaseDate = StringToDate(rdString, checkForAmbiguity: false, out DateTime? dt, out _) ? dt : null;
        }

        // These files also specify languages and whether the mission has custom stuff, but we're not going
        // to trust what we're told - we're going to detect that stuff by looking at what's actually there.

        return (title, author
#if FMScanner_FullCode
            , version
#endif
            , releaseDate);
    }

    private (string Title, string Author
#if FMScanner_FullCode
        , string Description
#endif
        , DateTime? LastUpdateDate, string Tags)
    ReadFMIni(NameAndIndex file)
    {
        var ret = (
            Title: "",
            Author: ""
#if FMScanner_FullCode
            , Description: ""
#endif
            , LastUpdateDate: (DateTime?)null,
            Tags: ""
        );

        #region Load INI

        ReadAllLinesDetectEncoding(file, _tempLines);

        if (_tempLines.Count == 0)
        {
            return ("", ""
#if FMScanner_FullCode
                , ""
#endif
                , null, "");
        }

        (string NiceName, string ReleaseDate, string Tags, string Descr) fmIni = ("", "", "", "");

        #region Deserialize ini

        bool inDescr = false;

        foreach (string line in _tempLines)
        {
            if (line.StartsWithI_Local("NiceName="))
            {
                inDescr = false;
                fmIni.NiceName = line.Substring(9).Trim();
            }
            else if (line.StartsWithI_Local("ReleaseDate="))
            {
                inDescr = false;
                fmIni.ReleaseDate = line.Substring(12).Trim();
            }
            else if (line.StartsWithI_Local("Tags="))
            {
                inDescr = false;
                fmIni.Tags = line.Substring(5).Trim();
            }
            // Sometimes Descr values are literally multi-line. DON'T. DO. THAT. Use \n.
            // But I have to deal with it anyway.
            else if (line.StartsWithI_Local("Descr="))
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

            if (fmIni.Descr[0] == '\"' && fmIni.Descr[fmIni.Descr.Length - 1] == '\"' &&
                CountChars(fmIni.Descr, '\"') == 2)
            {
                fmIni.Descr = fmIni.Descr.Trim(CA_DoubleQuote);
            }
            if (fmIni.Descr[0] == LeftDoubleQuote && fmIni.Descr[fmIni.Descr.Length - 1] == RightDoubleQuote &&
                CountChars(fmIni.Descr, LeftDoubleQuote) + CountChars(fmIni.Descr, RightDoubleQuote) == 2)
            {
                fmIni.Descr = fmIni.Descr.Trim(_ctx.CA_UnicodeQuotes);
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
            string[] tagsArray = fmIni.Tags.Split_Char(CA_CommaSemicolon, StringSplitOptions.RemoveEmptyEntries, _sevenZipContext.IntArrayPool);

            string authorString = "";
            for (int i = 0, authorsFound = 0; i < tagsArray.Length; i++)
            {
                string tag = tagsArray[i];
                if (tag.StartsWithI_Local("author:"))
                {
                    if (authorsFound > 0 && !authorString.EndsWithO(", ")) authorString += ", ";
                    authorString += tag.Substring(tag.IndexOf(':') + 1).Trim();
                    authorsFound++;
                }
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

        ReadAllLinesDetectEncoding(file, _tempLines);

        if (_tempLines.Count == 0) return ret;

        for (int i = 0; i < _tempLines.Count; i++)
        {
            string lineT = _tempLines[i].Trim();
            if (lineT.EqualsI_Local("[modName]"))
            {
                while (i < _tempLines.Count - 1)
                {
                    string lt = _tempLines[i + 1].Trim();
                    if (lt.IsIniHeader())
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
            else if (lineT.EqualsI_Local("[authors]"))
            {
                while (i < _tempLines.Count - 1)
                {
                    string lt = _tempLines[i + 1].Trim();
                    if (lt.IsIniHeader())
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

    private void ReadAndCacheReadmeFiles()
    {
        // Note: .wri files look like they may be just plain text with garbage at the top. Shrug.
        // Treat 'em like plaintext and see how it goes.

        foreach (NameAndIndex readmeFile in _readmeDirFiles)
        {
            if (!readmeFile.Name.IsValidReadme()) continue;

            ZipArchiveFastEntry? zipReadmeEntry = null;
            RarArchiveEntry? rarReadmeEntry = null;

            int readmeFileLen;
            string readmeFileOnDisk = "";
            bool isGlml;
            DateTime? lastModifiedDate = null;

            if (_fmFormat == FMFormat.Zip)
            {
                zipReadmeEntry = _archive.Entries[readmeFile.Index];
                readmeFileLen = (int)zipReadmeEntry.Length;
                if (readmeFileLen == 0) continue;

                isGlml = zipReadmeEntry.FullName.ExtIsGlml();
            }
            else if (_fmFormat == FMFormat.Rar)
            {
                rarReadmeEntry = _rarArchive.Entries[readmeFile.Index];
                readmeFileLen = (int)rarReadmeEntry.Size;
                if (readmeFileLen == 0) continue;

                isGlml = rarReadmeEntry.Key.ExtIsGlml();
            }
            else
            {
                FileInfoCustom readmeFI;

                if (_fmDirFileInfos.Count > 0)
                {
                    readmeFI = _fmDirFileInfos[readmeFile.Index];
                    if (readmeFI.Length == 0) continue;

                    readmeFileOnDisk = Path.Combine(_fmWorkingPath, readmeFile.Name);
                }
                else
                {
                    /*
                    If the readme was 0 length, it won't have been extracted, so in that case just skip this
                    one and move on.
                    */
                    try
                    {
                        string fullReadmeFileName = Path.Combine(_fmWorkingPath, readmeFile.Name);

                        FileInfo fi = new(fullReadmeFileName);
                        if (fi.Length == 0) continue;

                        readmeFI = new FileInfoCustom(fi);

                        readmeFileOnDisk = fullReadmeFileName;
                    }
                    catch
                    {
                        continue;
                    }
                }

                readmeFileLen = (int)readmeFI.Length;

                isGlml = readmeFI.FullName.ExtIsGlml();
                lastModifiedDate = new DateTimeOffset(readmeFI.LastWriteTime).DateTime;
            }

            // Files containing these phrases are almost certain to be script info files, whose dates will be the
            // release date of their respective script package, and so should be ignored when detecting the FM's
            // release date
            bool useThisReadmeForDateDetect =
                !readmeFile.Name.ContainsI("copyright") &&
                !readmeFile.Name.ContainsI("tnhScript") &&
                !readmeFile.Name.ContainsI("nvscript") &&
                !readmeFile.Name.ContainsI("shtup");

            bool scanThisReadme =
                useThisReadmeForDateDetect &&
                !readmeFile.Name.ExtIsHtml() &&
                readmeFile.Name.IsEnglishReadme();

            // We still add the readme even if we're not going to store nor scan its contents, because we still
            // may need to look at its last modified date.
            if (_fmFormat == FMFormat.Zip)
            {
                _readmeFiles.Add(new ReadmeInternal(
                    isGlml: isGlml,
                    lastModifiedDateRaw: zipReadmeEntry!.LastWriteTime,
                    scan: scanThisReadme,
                    useForDateDetect: useThisReadmeForDateDetect
                ));
            }
            else if (_fmFormat == FMFormat.Rar)
            {
                _readmeFiles.Add(new ReadmeInternal(
                    isGlml: isGlml,
                    lastModifiedDate: rarReadmeEntry!.LastModifiedTime ?? DateTime.MinValue,
                    scan: scanThisReadme,
                    useForDateDetect: useThisReadmeForDateDetect
                ));
            }
            else
            {
                _readmeFiles.Add(new ReadmeInternal(
                    isGlml: isGlml,
                    lastModifiedDate: (DateTime)lastModifiedDate!,
                    scan: scanThisReadme,
                    useForDateDetect: useThisReadmeForDateDetect
                ));
            }

            if (!scanThisReadme) continue;

            Stream? readmeStream = null;
            try
            {
                // Saw one ".rtf" that was actually a plaintext file, and one vice versa. So detect by header
                // alone.
                readmeStream = _fmFormat switch
                {
                    FMFormat.Zip => _archive.OpenEntry(zipReadmeEntry!),
                    FMFormat.Rar => rarReadmeEntry!.OpenEntryStream(),
                    _ => FileStreamCustom.CreateRead(readmeFileOnDisk, DiskFileStreamBuffer),
                };

                int rtfHeaderBytesLength = RTFHeaderBytes.Length;

                _rtfHeaderBuffer.Clear();

                if (readmeFileLen >= rtfHeaderBytesLength)
                {
                    readmeStream.ReadAll(_rtfHeaderBuffer, 0, rtfHeaderBytesLength);
                }

                if (_fmFormat is FMFormat.Zip or FMFormat.Rar)
                {
                    readmeStream.Dispose();
                }
                else
                {
                    readmeStream.Seek(0, SeekOrigin.Begin);
                }

                ReadmeInternal last = _readmeFiles[^1];

                bool readmeIsRtf = _rtfHeaderBuffer.SequenceEqual(RTFHeaderBytes);
                if (readmeIsRtf)
                {
                    if (_fmFormat == FMFormat.Zip)
                    {
                        readmeStream = _archive.OpenEntry(zipReadmeEntry!);
                    }
                    else if (_fmFormat == FMFormat.Rar)
                    {
                        readmeStream = rarReadmeEntry!.OpenEntryStream();
                    }

                    // @MEM(RTF pooled byte arrays): This pool barely helps us
                    // Most of the arrays are used only once, a handful are used twice.
                    byte[] rtfBytes = _sevenZipContext.ByteArrayPool.Rent(readmeFileLen);
                    try
                    {
                        int bytesRead = readmeStream.ReadAll(rtfBytes, 0, readmeFileLen);
                        (bool success, string text) = RtfConverter.Convert(new ArrayWithLength<byte>(rtfBytes, bytesRead));
                        if (success)
                        {
                            last.Text = text;
                            last.Lines.ClearFullAndAdd(text.Split_String(_ctx.SA_Linebreaks, StringSplitOptions.None, _sevenZipContext.IntArrayPool));
                        }
                    }
                    finally
                    {
                        _sevenZipContext.ByteArrayPool.Return(rtfBytes);
                    }
                }
                else
                {
                    Stream stream = _fmFormat switch
                    {
                        FMFormat.Zip => CreateSeekableStreamFromZipEntry(zipReadmeEntry!, readmeFileLen),
                        FMFormat.Rar => CreateSeekableStreamFromRarEntry(rarReadmeEntry!, readmeFileLen),
                        _ => readmeStream,
                    };

                    last.Text = last.IsGlml
                        ? Utility.GLMLToPlainText(ReadAllTextUTF8(stream), Utf32CharBuffer)
                        : ReadAllTextDetectEncoding(stream);
                    last.Lines.ClearFullAndAdd(last.Text.Split_String(_ctx.SA_Linebreaks, StringSplitOptions.None, _sevenZipContext.IntArrayPool));
                }
            }
            finally
            {
                readmeStream?.Dispose();
            }
        }
    }

    private MemoryStream CreateSeekableStreamFromZipEntry(ZipArchiveFastEntry readmeEntry, int readmeFileLen)
    {
        _generalMemoryStream.SetLength(readmeFileLen);
        _generalMemoryStream.Position = 0;
        using var es = _archive.OpenEntry(readmeEntry);
        StreamCopyNoAlloc(es, _generalMemoryStream, StreamCopyBuffer);
        _generalMemoryStream.Position = 0;
        return _generalMemoryStream;
    }

    private MemoryStream CreateSeekableStreamFromRarEntry(RarArchiveEntry readmeEntry, int readmeFileLen)
    {
        _generalMemoryStream.SetLength(readmeFileLen);
        _generalMemoryStream.Position = 0;
        using var es = readmeEntry.OpenEntryStream();
        StreamCopyNoAlloc(es, _generalMemoryStream, StreamCopyBuffer);
        _generalMemoryStream.Position = 0;
        return _generalMemoryStream;
    }

    private string GetValueFromReadme(SpecialLogic specialLogic, string[] keys, List<string>? titles = null, ReadmeInternal? singleReadme = null)
    {
        string ret = "";

        for (int ri = 0; ri < _readmeFiles.Count; ri++)
        {
            ReadmeInternal file = _readmeFiles[ri];
            // @TDM_NOTE: Janky, but it works and a null check isn't going to be our bottleneck here, so whatever
            if (singleReadme != null && singleReadme != file) continue;

            if (!file.Scan) continue;

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
                    // output. Not enough to matter really, but meh...
                    ret = GetAuthorFromText(file.Text);
                    if (!ret.IsEmpty()) return ret;
                }
            }
            else
            {
                if (specialLogic == SpecialLogic.ReleaseDate)
                {
                    Match newDarkMatch = _ctx.NewDarkAndNumberRegex.Match(ret);
                    if (newDarkMatch.Success)
                    {
                        ret = ret.Substring(0, newDarkMatch.Index);
                    }

                    Match rtlNumberMatch = _ctx.AnyDateNumberRTLRegex.Match(ret);
                    if (rtlNumberMatch.Success)
                    {
                        ret = ret.Substring(0, rtlNumberMatch.Index + rtlNumberMatch.Length);
                    }
                }

                return ret;
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
            static string GetAuthorNextLine(ListFast<string> lines)
            {
                for (int i = 0; i < lines.Count; i++)
                {
                    string lineT = lines[i].Trim();
                    if (!lineT.EqualsI_Local("Author") &&
                        !lineT.EqualsI_Local("Author:") &&
                        !lineT.EqualsI_Local("Authors") &&
                        !lineT.EqualsI_Local("Authors:"))
                    {
                        continue;
                    }

                    if (i < lines.Count - 2)
                    {
                        string lineAfterNext = lines[i + 2].Trim();
                        int lanLen = lineAfterNext.Length;
                        if ((lanLen > 0 &&
                             (lineAfterNext.Contains(':') ||
                              // Overly-specific hack for the Dark Mod training mission
                              lineAfterNext == ".") &&
                             lanLen <= 50) ||
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

            /*
            @PERF_TODO(Scanner/GetValueFromReadme/GetAuthorFromTitleByAuthorLine() call section):
            This is last because it used to have a dynamic and constantly re-instantiated regex in it. I
            don't even know why I thought I needed that but it turns out I could just make it static like
            the rest, so I did. Re-evaluate this and maybe put it higher?
            Anything that can go before the full-text search probably should, because that's clearly the
            slowest by far.
            */
            ret = GetAuthorFromTitleByAuthorLine(titles);
        }

        return ret;
    }

    private string GetValueFromLines(SpecialLogic specialLogic, string[] keys, ListFast<string> lines)
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
                    lineStartTrimmed.StartsWithI_Local("Title & Description") ||
                    lineStartTrimmed.StartsWithGL("Title screen"):
#if false
                    // @Scanner: Enable these once we have more robust readme language logic
                    // Ugh
                    lineStartTrimmed.StartsWithGL("Titre  de la mission") ||
                    lineStartTrimmed.StartsWithGL("Titre original"):
#endif
                case SpecialLogic.ReleaseDate when
                    lineStartTrimmed.StartsWithI_Local("Release information") ||
                    lineStartTrimmed.StartsWithI_Local("Release version") ||
                    lineStartTrimmed.StartsWithI_Local("Release: version") ||
                    lineStartTrimmed.StartsWithI_Local("Released for"):
#if FMScanner_FullCode
                case SpecialLogic.Version when
                    lineStartTrimmed.StartsWithI_Local("Version History") ||
                    lineStartTrimmed.ContainsI("NewDark") ||
                    lineStartTrimmed.ContainsI("64 Cubed") ||
                    _ctx.VersionExclude1Regex.Match(lineStartTrimmed).Success:
#endif
                case SpecialLogic.Author when
                    lineStartTrimmed.StartsWithI_Local("Authors note"):
                    continue;
            }

            #endregion

            bool lineStartsWithKey = false;
            bool lineStartsWithKeyAndSeparatorChar = false;
            int indexAfterKey = -1;
            foreach (string key in keys)
            {
                // Either in given case or in all caps, but not in lowercase, because that's given me at least
                // one false positive
                if (lineStartTrimmed.StartsWithGU(key))
                {
                    lineStartsWithKey = true;

                    int keyLength = key.Length;

                    for (int afterKeyIndex = keyLength; afterKeyIndex < lineStartTrimmed.Length; afterKeyIndex++)
                    {
                        char c = lineStartTrimmed[afterKeyIndex];
                        if (!char.IsWhiteSpace(c))
                        {
                            if (c is ':' or '-' or '\u2013')
                            {
                                lineStartsWithKeyAndSeparatorChar = true;
                                indexAfterKey = keyLength;
                            }
                            break;
                        }
                    }

                    if (lineStartsWithKeyAndSeparatorChar)
                    {
                        break;
                    }
                }
            }
            if (!lineStartsWithKey) continue;

            if (lineStartsWithKeyAndSeparatorChar)
            {
                if (specialLogic == SpecialLogic.ReleaseDate)
                {
                    lineStartTrimmed = _ctx.MultipleColonsRegex.Replace(lineStartTrimmed, ":");
                    lineStartTrimmed = _ctx.MultipleDashesRegex.Replace(lineStartTrimmed, "-");
                    lineStartTrimmed = _ctx.MultipleUnicodeDashesRegex.Replace(lineStartTrimmed, "\u2013");
                }

                // Don't count these chars if they're part of a key
                int indexColon = lineStartTrimmed.IndexOf(':', indexAfterKey);
                int indexDash = lineStartTrimmed.IndexOf('-', indexAfterKey);
                int indexUnicodeDash = lineStartTrimmed.IndexOf('\u2013', indexAfterKey);

                int index = (indexColon | indexDash) > -1
                    ? Math.Min(indexColon, indexDash)
                    : Math.Max(indexColon, indexDash);

                if (index == -1) index = indexUnicodeDash;

                string finalValue = lineStartTrimmed.Substring(index + 1).Trim();
                if (!finalValue.IsEmpty())
                {
                    return finalValue;
                }
                else if (specialLogic == SpecialLogic.ReleaseDate && lineIndex < lines.Count - 1)
                {
                    return lines[lineIndex + 1].Trim();
                }
            }
            else
            {
#if FMScanner_FullCode
                // Don't detect "Version "; too many false positives
                // TODO: Can probably remove this check and then just sort out any false positives in GetVersion()
                if (specialLogic == SpecialLogic.Version) continue;
#endif

                foreach (string key in keys)
                {
                    if (!lineStartTrimmed.StartsWithI_Local(key)) continue;

                    // It's supposed to be finding a space after a key; this prevents it from finding the first
                    // space in the key itself if there is one.
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

        value = value.TrimEnd();

        if (value.IsEmpty()) return value;

        if (value[0] == '\"' && value[^1] == '\"') value = value.Trim(CA_DoubleQuote);

        if (value.IsEmpty()) return value;

        if ((value[0] == LeftDoubleQuote || value[0] == RightDoubleQuote) &&
            (value[^1] == LeftDoubleQuote || value[^1] == RightDoubleQuote))
        {
            value = value.Trim(_ctx.CA_UnicodeQuotes);
        }

        value = value.RemoveUnpairedLeadingOrTrailingQuotes();

        value = _ctx.MultipleWhiteSpaceRegex.Replace(value, " ");
        value = value.Replace('\t', ' ');

        #region Parentheses

        value = value.RemoveSurroundingParentheses();

        bool containsOpenParen = value.Contains('(');
        bool containsCloseParen = value.Contains(')');

        if (containsOpenParen) value = _ctx.OpenParenSpacesRegex.Replace(value, "(");
        if (containsCloseParen) value = _ctx.CloseParenSpacesRegex.Replace(value, ")");

        // If there's stuff like "(this an incomplete sentence and" at the end, chop it right off
        if (containsOpenParen && !containsCloseParen && value.CharAppearsExactlyOnce('('))
        {
            value = value.Substring(0, value.LastIndexOf('(')).TrimEnd();
        }

        #endregion

        // Remove trailing period unless it's at the end of a single letter (eg. "Robin G.")
        if (value.CharAppearsExactlyOnce('.') && value[^1] == '.' &&
            !((value.Length >= 3 && !char.IsWhiteSpace(value[^2]) && char.IsWhiteSpace(value[^3])) ||
              (value.Length == 2 && !char.IsWhiteSpace(value[^2]))))
        {
            value = value.Substring(0, value.Length - 1);
        }

        value = value.Trim(_ctx.CA_Asterisk);

        foreach (AsciiCharWithNonAsciiEquivalent item in _ctx._nonAsciiCharsWithAsciiEquivalents)
        {
            value = value.Replace(item.Original, item.Ascii);
        }

        return value;
    }

    #region Title(s) and mission names

    // This is kind of just an excuse to say that my scanner can catch the full proper title of Deceptive
    // Perception 2. :P
    // This is likely to be a bit loose with its accuracy, but since values caught here are almost certain to
    // end up as alternate titles, I can afford that.
    private List<string>? GetTitlesFromTopOfReadmes()
    {
        if (_readmeFiles.Count == 0) return null;

        List<string>? ret = null;

        foreach (ReadmeInternal r in _readmeFiles)
        {
            if (!r.Scan) continue;

            _topLines.Clear();

            for (int i = 0; i < r.Lines.Count; i++)
            {
                string line = r.Lines[i];
                if (!line.IsWhiteSpace()) _topLines.Add(line);
                if (_topLines.Count == _maxTopLines) break;
            }

            if (_topLines.Count < 2) continue;

            string titleConcat = "";

            for (int i = 0; i < _topLines.Count; i++)
            {
                string lineT = _topLines[i].Trim();
                if (i > 0 &&
                    (lineT.StartsWithI_Local("By ") || lineT.StartsWithI_Local("By: ") ||
                     lineT.StartsWithI_Local("Original concept by ") ||
                     lineT.StartsWithI_Local("Created by ") ||
                     _ctx.AThiefMissionRegex.Match(lineT).Success ||
                     lineT.StartsWithI_Local("A fan mission") ||
                     lineT.StartsWithI_Local("A Thief 3") ||
                     _ctx.AThief3MissionRegex.Match(lineT).Success ||
                     lineT.StartsWithI_Local("A System Shock") ||
                     lineT.StartsWithI_Local("An SS2")))
                {
                    for (int j = 0; j < i; j++)
                    {
                        if (j > 0) titleConcat += " ";
                        titleConcat += _topLines[j];
                    }
                    // Set a cutoff for the length so we don't end up with a huge string that's obviously more
                    // than a title
                    if (!titleConcat.IsWhiteSpace() && titleConcat.Length <= 50)
                    {
                        if (ret == null)
                        {
                            ret = _tempLines;
                            ret.Clear();
                        }
                        ret.Add(titleConcat);
                    }

                    break;
                }
            }
        }

        return ret;
    }

    private string GetTitleFromNewGameStrFile()
    {
        if (_intrfaceDirFiles.Count == 0) return "";

        int newGameStrFileIndex = -1;
        for (int i = 0; i < _intrfaceDirFiles.Count; i++)
        {
            NameAndIndex item = _intrfaceDirFiles[i];
            if (item.Name.PathEqualsI(FMFiles.IntrfaceEnglishNewGameStr))
            {
                newGameStrFileIndex = i;
                break;
            }
        }
        if (newGameStrFileIndex == -1)
        {
            for (int i = 0; i < _intrfaceDirFiles.Count; i++)
            {
                NameAndIndex item = _intrfaceDirFiles[i];
                if (item.Name.PathEqualsI(FMFiles.IntrfaceNewGameStr))
                {
                    newGameStrFileIndex = i;
                    break;
                }
            }
        }
        if (newGameStrFileIndex == -1)
        {
            for (int i = 0; i < _intrfaceDirFiles.Count; i++)
            {
                NameAndIndex item = _intrfaceDirFiles[i];
                if (item.Name.PathStartsWithI(FMDirs.IntrfaceS) &&
                    item.Name.PathEndsWithI(FMFiles.SNewGameStr))
                {
                    newGameStrFileIndex = i;
                    break;
                }
            }
        }

        if (newGameStrFileIndex == -1) return "";

        ReadAllLinesDetectEncoding(_intrfaceDirFiles[newGameStrFileIndex], _tempLines, type: DetectEncodingType.NewGameStr);

        for (int i = 0; i < _tempLines.Count; i++)
        {
            string lineT = _tempLines[i].Trim();
            if (lineT.StartsWithI_Local("skip_training:"))
            {
                string title = Utility.ExtractFromQuotedSection(lineT);
                if (title.IsEmpty()) continue;

                // Do our best to ignore things that aren't titles
                if (// first chars
                    title[0] != '{' && title[0] != '}' && title[0] != '-' && title[0] != '_' &&
                    title[0] != ':' && title[0] != ';' && title[0] != '!' && title[0] != '@' &&
                    title[0] != '#' && title[0] != '$' && title[0] != '%' && title[0] != '^' &&
                    title[0] != '&' && title[0] != '*' && title[0] != '(' && title[0] != ')' &&
                    // entire titles
                    !title.EqualsI_Local("Play") && !title.EqualsI_Local("Start") &&
                    !title.EqualsI_Local("Begin") && !title.EqualsI_Local("Begin...") &&
                    !title.EqualsI_Local("skip training") &&
                    // starting strings
                    !title.StartsWithI_Local("Let's go") && !title.StartsWithI_Local("Let's rock this boat") &&
                    !title.StartsWithI_Local("Play ") && !title.StartsWithI_Local("Continue") &&
                    !title.StartsWithI_Local("Start ") && !title.StartsWithI_Local("Begin "))
                {
                    return title;
                }
            }
        }

        return "";
    }

    private (string TitleFrom0, string TitleFromN
#if FMScanner_FullCode
        , List<string>? CampaignMissionNames
#endif
        )
    GetMissionNames()
    {
        List<string>? titlesStrLines = GetTitlesStrLines();
        if (titlesStrLines == null || titlesStrLines.Count == 0)
        {
            return ("", ""
#if FMScanner_FullCode
                    , null
#endif
                );
        }

        var ret =
            (TitleFrom0: "",
                TitleFromN: ""
#if FMScanner_FullCode
                , CampaignMissionNames: (List<string>?)null
#endif
            );

        static bool NameExistsInList(List<NameAndIndex> list, string value)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].Name.ContainsI(value)) return true;
            }
            return false;
        }

#if FMScanner_FullCode
        var titles = new List<string>(titlesStrLines.Count);
#else
        int titleFromTitlesFoundCount = 0;
        string firstTitleFromTitles = "";
#endif
        for (int lineIndex = 0; lineIndex < titlesStrLines.Count; lineIndex++)
        {
            string titleNum = "";
            string title = "";
            for (int umfIndex = 0; umfIndex < _usedMisFiles.Count; umfIndex++)
            {
                string line = titlesStrLines[lineIndex];
                int i;
                titleNum = line.Substring(i = line.IndexOf('_') + 1, line.IndexOf(':') - i).Trim();

                title = Utility.ExtractFromQuotedSection(line);
                if (title.IsEmpty()) continue;

                if (titleNum == "0") ret.TitleFrom0 = title;

                string umf = _usedMisFiles[umfIndex].Name;
                int umfDotIndex = umf.IndexOf('.');

                if (umfDotIndex > 4 && umf.StartsWithI_Local("miss") && titleNum == umf.Substring(4, umfDotIndex - 4))
                {
#if FMScanner_FullCode
                    titles.Add(title);
#else
                    if (titleFromTitlesFoundCount == 0)
                    {
                        firstTitleFromTitles = title;
                    }
                    titleFromTitlesFoundCount++;
#endif
                }
            }

            string missNumMis;

            if (_scanOptions.ScanTitle &&
                ret.TitleFromN.IsEmpty() &&
                lineIndex == titlesStrLines.Count - 1 &&
                !titleNum.IsEmpty() &&
                !title.IsEmpty() &&
                !NameExistsInList(_usedMisFiles, missNumMis = "miss" + titleNum + ".mis") &&
                NameExistsInList(_misFiles, missNumMis))
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

#if FMScanner_FullCode
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
#else
        if (_scanOptions.ScanTitle && titleFromTitlesFoundCount == 1)
        {
            ret.TitleFromN = firstTitleFromTitles;
        }
#endif

        return ret;
    }

    private List<string>? GetTitlesStrLines()
    {
        List<string>? titlesStrLines = null;

        #region Read title(s).str file

        foreach (string titlesFileLocation in _ctx.FMFiles_TitlesStrLocations)
        {
            int titlesFileIndex = -1;
            for (int i = 0; i < _stringsDirFiles.Count; i++)
            {
                NameAndIndex item = _stringsDirFiles[i];
                if (item.Name.PathEqualsI(titlesFileLocation))
                {
                    titlesFileIndex = i;
                    break;
                }
            }

            if (titlesFileIndex == -1) continue;

            ReadAllLinesDetectEncoding(_stringsDirFiles[titlesFileIndex], _tempLines, type: DetectEncodingType.TitlesStr);
            titlesStrLines = _tempLines;

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
                // Allocation avoidance

                string tfLineD = tfLinesD[i];

                ReadOnlySpan<char> tfLineDSpan = tfLineD.AsSpan();
                ReadOnlySpan<char> lineSpan = line.AsSpan()[..indexOfColon];

                int tfLineDSpanLen = tfLineDSpan.Length;
                int lineSpanLen = lineSpan.Length;

                if (tfLineDSpanLen >= lineSpanLen)
                {
                    Utility.StringCompareReturn strCmpResult = Utility.CompareToOrdinalIgnoreCase(tfLineDSpan[..lineSpanLen], lineSpan);
                    bool result;
                    if (strCmpResult.RequiresStringComparison)
                    {
                        // This path never gets hit in my ~1700 FM set, it's just a fallback in case it ever
                        // encounters a corner case. I think it would require non-ASCII chars.
                        result = tfLineD.StartsWith(line.Substring(0, indexOfColon), OrdinalIgnoreCase);
                    }
                    else
                    {
                        result = strCmpResult.Compare == 0;
                    }

                    if (result) return true;
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
                line.StartsWithI_Local("title_") &&
                (indexOfColon = line.IndexOf(':')) > -1 &&
                line.CharCountIsAtLeast('\"', 2) &&
                !TFLinesDAny(line, indexOfColon, tfLinesD))
            {
                tfLinesD.Add(line);
            }
        }

        tfLinesD.Sort(_ctx.TitlesStrNaturalNumericSort);

        #endregion

        return tfLinesD;
    }

    private string CleanupTitle(string value)
    {
        if (value.IsEmpty()) return value;

        // Some titles are clever and  A r e  W r i t t e n  L i k e  T h i s
        // But we want to leave titles that are supposed to be acronyms - ie, "U F O", "R G B"
        if (value.Contains(' ') && !Utility.AnyConsecutiveWordChars(value))
        {
            bool titleContainsLowerCaseAsciiChars = false;
            for (int i = 0; i < value.Length; i++)
            {
                // TODO: Only ASCII letters, so it won't catch lowercase other stuff
                if (value[i].IsAsciiLower())
                {
                    titleContainsLowerCaseAsciiChars = true;
                    break;
                }
            }

            if (titleContainsLowerCaseAsciiChars)
            {
                if (value.Contains("  ", Ordinal))
                {
                    string[] titleWords = value.Split_String(_ctx.SA_DoubleSpaces, StringSplitOptions.None, _sevenZipContext.IntArrayPool);
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
        }

        return CleanupValue(value).Trim();
    }

    /*
    @TDM: We could prefer titles with : in them?
    eg. "The Beleaguered Fence" vs. "Thomas Porter 2: Beleaguered Fence"
    We've fixed that one now, but for non-TDM the fix isn't active, so still a thought.
    */
    private void OrderTitlesOptimally(List<string> originalTitles, string? serverTitle = null)
    {
        if (originalTitles.Count == 0) return;

        List<DetectedTitle> titles = new();
        foreach (string title in originalTitles)
        {
            titles.Add(new DetectedTitle(title, temporary: false));
        }
        // Ultimate final attack against stubborn titles that just won't be caught
        foreach (ReadmeInternal readme in _readmeFiles)
        {
            if (readme.Lines.Count >= 2 && readme.Lines[1].IsWhiteSpace())
            {
                titles.Add(new DetectedTitle(readme.Lines[0], temporary: true));
            }
        }

        if (titles.Count < 2) return;

        DetectedTitle mainTitle = titles[0];

        if (mainTitle.Value.IsEmpty()) return;

        _titleAcronymChars.ClearFast();

        byte[] romanNumeralToDecimalTable = _ctx.RomanNumeralToDecimalTable;

        bool titleContainsAcronym = _ctx.AcronymRegex.Match(mainTitle.Value).Success;
        Utility.GetAcronym(mainTitle.Value, _titleAcronymChars, romanNumeralToDecimalTable);

        bool swapDone = false;

        ListFast<char> tempChars1 = Title1_TempNonWhitespaceChars;
        ListFast<char> tempChars2 = Title2_TempNonWhitespaceChars;

        if (titleContainsAcronym)
        {
            for (int i = 1; i < titles.Count; i++)
            {
                DetectedTitle altTitle = titles[i];
                _altTitleAcronymChars.ClearFast();
                _altTitleRomanToDecimalAcronymChars.ClearFast();

                Utility.GetAcronym(altTitle.Value, _altTitleAcronymChars, romanNumeralToDecimalTable);
                Utility.GetAcronym(altTitle.Value, _altTitleRomanToDecimalAcronymChars, romanNumeralToDecimalTable, convertRomanToDecimal: true);

                if (!mainTitle.Value.EqualsIgnoreCaseAndWhiteSpace(altTitle.Value, tempChars1, tempChars2) &&
                    (Utility.SequenceEqual(_titleAcronymChars, _altTitleAcronymChars) ||
                     Utility.SequenceEqual(_titleAcronymChars, _altTitleRomanToDecimalAcronymChars)))
                {
                    swapDone = SwapMainTitleWithTitleAtIndex(titles, i);
                    break;
                }
            }

            mainTitle = titles[0];
        }

        if (!mainTitle.Value.ContainsWhiteSpace() && mainTitle.Value.ContainsMultipleWords())
        {
            for (int i = 1; i < titles.Count; i++)
            {
                DetectedTitle altTitle = titles[i];
                if (altTitle.Value.ContainsWhiteSpace() &&
                    !(altTitle.Value.Length >= 2 && altTitle.Value[^1].IsAsciiUpper() &&
                      !altTitle.Value[^2].IsAsciiAlphanumeric()))
                {
                    swapDone = SwapMainTitleWithTitleAtIndex(titles, i);
                    break;
                }
            }
        }

        if (!swapDone &&
            !serverTitle.IsEmpty() &&
            !_ctx.AcronymRegex.Match(serverTitle).Success &&
            (titleContainsAcronym || serverTitle.Length > titles[0].Value.Length))
        {
            DoServerTitleSwap(titles, serverTitle);
            swapDone = true;
        }

        if (!swapDone &&
            titleContainsAcronym &&
            titles.Count == 2 &&
            titles[1].Value.Length > titles[0].Value.Length &&
            !titles[1].Temporary &&
            !titles[1].Value.StartsWithI_Local(titles[0].Value) &&
            !titles[1].Value.EqualsIgnoreCaseAndWhiteSpace(titles[0].Value, tempChars1, tempChars2))
        {
            SwapMainTitleWithTitleAtIndex(titles, 1);
        }

        originalTitles.Clear();
        for (int i = 0; i < titles.Count; i++)
        {
            DetectedTitle title = titles[i];
            if (i == 0 || !title.Temporary)
            {
                originalTitles.Add(title.Value);
            }
        }

        return;

        static bool SwapMainTitleWithTitleAtIndex(List<DetectedTitle> titles, int index)
        {
            (titles[0], titles[index]) = (titles[index], titles[0]);
            titles[0].Temporary = false;
            return true;
        }

        static void DoServerTitleSwap(List<DetectedTitle> titles, string serverTitle)
        {
            for (int i = 1; i < titles.Count; i++)
            {
                if (titles[i].Value == serverTitle)
                {
                    SwapMainTitleWithTitleAtIndex(titles, i);
                    return;
                }
            }
        }
    }

    #endregion

    #region Author

    private string GetAuthorFromTopOfReadme(ListFast<string> lines, List<string>? titles)
    {
        if (lines.Count == 0) return "";

        bool titleStartsWithBy = false;
        bool titleContainsBy = false;
        if (titles != null)
        {
            foreach (string title in titles)
            {
                if (title.StartsWithI_Local("by ")) titleStartsWithBy = true;
                if (title.ContainsI(" by ")) titleContainsBy = true;
            }
        }

        // Look for a "by [author]" in the first few lines. Looking for a line starting with "by" throughout
        // the whole text is asking for a cavalcade of false positives, hence why we only look near the top.
        _topLines.Clear();

        for (int i = 0; i < lines.Count; i++)
        {
            string line = lines[i];
            if (!line.IsWhiteSpace()) _topLines.Add(line);
            if (_topLines.Count == _maxTopLines) break;
        }

        if (_topLines.Count < 2) return "";

        for (int i = 0; i < _topLines.Count; i++)
        {
            if (i == 0 && titleStartsWithBy) continue;

            string lineT = _topLines[i].Trim();
            if (lineT.StartsWithI_Local("By ") || lineT.StartsWithI_Local("By: "))
            {
                string author = lineT.Substring(lineT.IndexOf(' ')).TrimStart();
                if (!author.IsEmpty()) return author;
            }
            else if (lineT.EqualsI_Local("By"))
            {
                if (!titleContainsBy && i < _topLines.Count - 1)
                {
                    return _topLines[i + 1].Trim();
                }
            }
            else
            {
                Match m = _ctx.AuthorGeneralCopyrightRegex.Match(lineT);
                if (!m.Success) continue;

                string author = CleanupCopyrightAuthor(m.Groups["Author"].Value);
                if (!author.IsEmpty()) return author;
            }
        }

        return "";
    }

    private string GetAuthorFromText(string text)
    {
        foreach (Regex regex in _ctx.AuthorRegexes)
        {
            Match match = regex.Match(text);
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

            for (int i = 0; i < rf.Lines.Count; i++)
            {
                string lineT = rf.Lines[i].Trim();

                if (!lineT.ContainsI(" by ")) continue;

                string titleCandidate = lineT.Substring(0, lineT.IndexOf(" by", OrdinalIgnoreCase)).Trim();

                bool fuzzyMatched = false;
                foreach (string title in titles)
                {
                    if (titleCandidate.SimilarityTo(title, _sevenZipContext) > 0.75)
                    {
                        fuzzyMatched = true;
                        break;
                    }
                }
                if (!fuzzyMatched) continue;

                string secondHalf = lineT.Substring(lineT.IndexOf(" by", OrdinalIgnoreCase));

                Match match = _ctx.TitleByAuthorRegex.Match(secondHalf);
                if (match.Success) return match.Groups["Author"].Value;
            }
        }

        return "";
    }

    private string GetAuthorFromCopyrightMessage()
    {
        static string AuthorCopyrightRegexesMatch(string line, Regex[] authorMissionCopyrightRegexes)
        {
            foreach (Regex regex in authorMissionCopyrightRegexes)
            {
                Match match = regex.Match(line);
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

            for (int i = 0; i < rf.Lines.Count; i++)
            {
                string line = rf.Lines[i];

                if (line.IsWhiteSpace()) continue;

                if (inCopyrightSection)
                {
                    // This whole nonsense is just to support the use of @ as a copyright symbol (used by some
                    // Theker missions); we want to be very specific about when we decide that "@" means "©".
                    Match m = !pastFirstLineOfCopyrightSection
                        ? _ctx.AuthorGeneralCopyrightIncludeAtSymbolRegex.Match(line)
                        : _ctx.AuthorGeneralCopyrightRegex.Match(line);
                    if (m.Success)
                    {
                        author = m.Groups["Author"].Value;
                        foundAuthor = true;
                        break;
                    }

                    pastFirstLineOfCopyrightSection = true;
                }

                author = AuthorCopyrightRegexesMatch(line, _ctx.AuthorMissionCopyrightRegexes);
                if (!author.IsEmpty())
                {
                    foundAuthor = true;
                    break;
                }

                string lineT = line.Trim(_ctx.CA_AsteriskHyphen).Trim();
                if (lineT.EqualsI_Local("Copyright Information") || lineT.EqualsI_Local("Copyright"))
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

        Match yearMatch = _ctx.CopyrightAuthorYearRegex.Match(author);
        if (yearMatch.Success) author = author.Substring(0, yearMatch.Index);

        if (author.Length >= 2 && author[^2] == ' ')
        {
            const string junkChars = "!@#$%^&*";
            char lastChar = author[^1];
            for (int i = 0; i < junkChars.Length; i++)
            {
                if (lastChar == junkChars[i])
                {
                    author = author.Substring(0, author.Length - 2);
                    break;
                }
            }
        }

        return author.TrimEnd(_ctx.CA_Period).Trim();
    }

    #endregion

#if FMScanner_FullCode
    private string GetVersion()
    {
        string version = GetValueFromReadme(SpecialLogic.Version, _ctx.SA_VersionDetect);

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
            Match match = _ctx.VersionFirstNumberRegex.Match(version);
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

    private (Language Langs, bool EnglishIsUncertain)
    GetLanguages()
    {
        Language langs = Language.Default;
        bool englishIsUncertain = false;

        for (int dirIndex = 0; dirIndex < 3; dirIndex++)
        {
            List<NameAndIndex> dirFiles = dirIndex switch
            {
                0 => _booksDirFiles,
                1 => _intrfaceDirFiles,
                _ => _stringsDirFiles,
            };

            for (int langIndex = 0; langIndex < SupportedLanguageCount; langIndex++)
            {
                Language language = LanguageIndexToLanguage((LanguageIndex)langIndex);
                for (int dfIndex = 0; dfIndex < dirFiles.Count; dfIndex++)
                {
                    NameAndIndex df = dirFiles[dfIndex];
                    // Directory separator agnostic & keeping perf reasonably high
                    string dfName = df.Name.ToForwardSlashes();

                    // We say HasFileExtension() because we only want to count lang dirs that have files in them
                    if (dfName.HasFileExtension() &&
                        (dfName.ContainsI(_ctx.Languages_FS_Lang_FS[langIndex]) ||
                         dfName.ContainsI(_ctx.Languages_FS_Lang_Language_FS[langIndex])))
                    {
                        langs |= language;
                        break;
                    }
                }
            }
        }

        if (!langs.HasFlagFast(Language.English))
        {
            langs |= Language.English;
            englishIsUncertain = true;
        }

        // Sometimes extra languages are in zip files inside the FM archive
        for (int baseDirFileIndex = 0; baseDirFileIndex < _baseDirFiles.Count; baseDirFileIndex++)
        {
            string fn = _baseDirFiles[baseDirFileIndex].Name;
            if (!fn.ExtIsZip() && !fn.ExtIs7z() && !fn.ExtIsRar()) continue;

            ReadOnlySpan<char> fnNoExt = fn.AsSpan(0, fn.LastIndexOf('.'));

            for (int langIndex = 0; langIndex < SupportedLanguageCount; langIndex++)
            {
                if (fn.StartsWithI_Local(SupportedLanguages[langIndex]))
                {
                    langs |= LanguageIndexToLanguage((LanguageIndex)langIndex);
                }
            }

            // "Italiano" will be caught by StartsWithI("italian")

            // Extra logic to account for whatever-style naming
            if (Utility.EqualsI_Local(fnNoExt, "rus") ||
                Utility.EndsWithI_Local(fnNoExt, "_ru") ||
                Utility.EndsWithI_Local(fnNoExt, "_rus") ||
                (fnNoExt.Length >= 4 && Utility.EndsWithI_Local(fnNoExt, "RUS") && fnNoExt[^4].IsAsciiLower()) ||
                fn.ContainsI("RusPack") || fn.ContainsI("RusText"))
            {
                langs |= Language.Russian;
            }
            else if (fn.ContainsI("Francais"))
            {
                langs |= Language.French;
            }
            else if (fn.ContainsI("Deutsch") || fn.ContainsI("Deutch"))
            {
                langs |= Language.German;
            }
            else if (fn.ContainsI("Espanol"))
            {
                langs |= Language.Spanish;
            }
            else if (fn.ContainsI("Nederlands"))
            {
                langs |= Language.Dutch;
            }
            else if (Utility.EqualsI_Local(fnNoExt, "huntext"))
            {
                langs |= Language.Hungarian;
            }
        }

        return langs > Language.Default
            ? (Langs: langs, EnglishIsUncertain: englishIsUncertain)
            : (Langs: Language.English, EnglishIsUncertain: true);
    }

#if FMScanner_FullCode
    private (bool? NewDarkRequired, Game Game)
#else
    private Game
#endif
    GetGameTypeAndEngine()
    {
#if FMScanner_FullCode
        var ret = (NewDarkRequired: (bool?)null, Game: Game.Null);
#else
        Game game = Game.Null;
#endif

        #region Choose smallest .gam file

        static ZipArchiveFastEntry? GetSmallestGamEntry_Zip(ZipArchiveFast _archive, List<NameAndIndex> _baseDirFiles)
        {
            int smallestSizeIndex = -1;
            long smallestSize = long.MaxValue;
            for (int i = 0; i < _baseDirFiles.Count; i++)
            {
                NameAndIndex item = _baseDirFiles[i];
                if (item.Name.ExtIsGam())
                {
                    ZipArchiveFastEntry gamFile = _archive.Entries[item.Index];
                    if (gamFile.Length <= smallestSize)
                    {
                        smallestSize = gamFile.Length;
                        smallestSizeIndex = item.Index;
                    }
                }
            }

            return smallestSizeIndex == -1 ? null : _archive.Entries[smallestSizeIndex];
        }

        static RarArchiveEntry? GetSmallestGamEntry_Rar(RarArchive _archive, List<NameAndIndex> _baseDirFiles)
        {
            int smallestSizeIndex = -1;
            long smallestSize = long.MaxValue;
            for (int i = 0; i < _baseDirFiles.Count; i++)
            {
                NameAndIndex item = _baseDirFiles[i];
                if (item.Name.ExtIsGam())
                {
                    RarArchiveEntry gamFile = _archive.Entries[item.Index];
                    if (gamFile.Size <= smallestSize)
                    {
                        smallestSize = gamFile.Size;
                        smallestSizeIndex = item.Index;
                    }
                }
            }

            return smallestSizeIndex == -1 ? null : _archive.Entries[smallestSizeIndex];
        }

        #endregion

        #region Choose smallest .mis file

        NameAndIndex smallestUsedMisFile;
        {
            if (_usedMisFiles.Count == 1)
            {
                smallestUsedMisFile = _usedMisFiles[0];
            }
            // We know we have at least 1 used mis file at this point because we early-return way before this if
            // we don't
            else
            {
                int smallestSizeIndex = -1;
                long smallestSize = long.MaxValue;
                for (int i = 0; i < _usedMisFiles.Count; i++)
                {
                    NameAndIndex mis = _usedMisFiles[i];
                    long length = _fmFormat == FMFormat.Zip
                        ? _archive.Entries[mis.Index].Length
                        : _fmFormat == FMFormat.Rar
                            ? _rarArchive.Entries[mis.Index].Size
                            : _fmFormat == FMFormat.SevenZip || _fmDirFileInfos.Count > 0
                                ? _fmDirFileInfos[mis.Index].Length
                                : new FileInfo(Path.Combine(_fmWorkingPath, mis.Name)).Length;

                    if (length <= smallestSize)
                    {
                        smallestSize = length;
                        smallestSizeIndex = i;
                    }
                }

                smallestUsedMisFile = _usedMisFiles[smallestSizeIndex];
            }
        }

        #endregion

        #region Setup

        ZipArchiveFastEntry misFileZipEntry = null!;
        RarArchiveEntry misFileRarEntry = null!;

        string misFileOnDisk = "";

        if (_fmFormat == FMFormat.Zip)
        {
            misFileZipEntry = _archive.Entries[smallestUsedMisFile.Index];
        }
        else if (_fmFormat == FMFormat.Rar)
        {
            misFileRarEntry = _rarArchive.Entries[smallestUsedMisFile.Index];
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

#if FMScanner_FullCode
        bool foundAtNewDarkLocation = false;
#endif
        bool foundAtOldDarkThief2Location = false;

        // We need to say "length - x" because for zips, the buffer will be full offset size rather than detection
        // string size
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

        Stream? misStream = null;
        try
        {
            misStream = _fmFormat switch
            {
                FMFormat.Zip => _archive.OpenEntry(misFileZipEntry),
                FMFormat.Rar => misFileRarEntry.OpenEntryStream(),
                _ => FileStreamCustom.CreateRead(misFileOnDisk, DiskFileStreamBuffer),
            };

            for (int i = 0; i < _ctx._locations.Length; i++)
            {
                if (
#if FMScanner_FullCode
                    !_scanOptions.ScanNewDarkRequired &&
#endif
                    (_ctx._locations[i] == _newDarkLoc1 || _ctx._locations[i] == _newDarkLoc2))
                {
                    break;
                }

                byte[] buffer;

                if (_fmFormat is FMFormat.Zip or FMFormat.Rar)
                {
                    buffer = _zipOffsetBuffers[i];
                    int length = _ctx._zipOffsets[i];
                    int bytesRead = misStream.ReadAll(buffer, 0, length);
                    if (bytesRead < length) break;
                }
                else
                {
                    buffer = _gameDetectStringBuffer;
                    misStream.Position = _ctx._locations[i];
                    int bytesRead = misStream.ReadAll(buffer, 0, _gameDetectStringBufferLength);
                    if (bytesRead < _gameDetectStringBufferLength) break;
                }

                if ((_ctx._locations[i] == _ss2MapParamNewDarkLoc ||
                     _ctx._locations[i] == _ss2MapParamOldDarkLoc) &&
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
#if FMScanner_FullCode
                    return (null, Game.SS2);
#else
                    return Game.SS2;
#endif
                }
                else if ((_ctx._locations[i] == _oldDarkT2Loc ||
                          _ctx._locations[i] == _newDarkLoc1 ||
                          _ctx._locations[i] == _newDarkLoc2) &&
                         EndsWithSKYOBJVAR(buffer))
                {
                    // Zip reading is going to check the NewDark locations the other way round, but fortunately
                    // they're interchangeable in meaning so we don't have to do anything
                    if (_ctx._locations[i] == _newDarkLoc1 || _ctx._locations[i] == _newDarkLoc2)
                    {
#if FMScanner_FullCode
                        ret.NewDarkRequired = true;
                        foundAtNewDarkLocation = true;
#endif
                        break;
                    }
                    else if (_ctx._locations[i] == _oldDarkT2Loc)
                    {
                        foundAtOldDarkThief2Location = true;
                        break;
                    }
                }
            }

#if FMScanner_FullCode
            if (!foundAtNewDarkLocation) ret.NewDarkRequired = false;
#endif
        }
        finally
        {
            misStream?.Dispose();
        }

        #endregion

        if (foundAtOldDarkThief2Location)
        {
#if FMScanner_FullCode
            return (
                _scanOptions.ScanNewDarkRequired ? (bool?)false : null,
                _scanOptions.ScanGameType ? Game.Thief2 : Game.Null);
#else
            return _scanOptions.ScanGameType ? Game.Thief2 : Game.Null;
#endif
        }

#if FMScanner_FullCode
        if (!_scanOptions.ScanGameType)
        {
            return (ret.NewDarkRequired, Game.Null);
        }
#endif

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
                for (int si = 0, ei = bufferSize; si < boundaryLen; si++, ei++)
                {
                    chunk[si] = chunk[ei];
                }
            }

            return false;
        }

        if (_fmFormat is FMFormat.Zip or FMFormat.Rar)
        {
            // For zips, since we can't seek within the stream, the fastest way to find our string is just to
            // brute-force straight through.
            // We only need the .gam file for non-solid FMs, so we can save extracting it otherwise.
            Stream? stream = null;
            try
            {
                if (_fmFormat == FMFormat.Zip)
                {
                    stream = _archive.OpenEntry(GetSmallestGamEntry_Zip(_archive, _baseDirFiles) ?? misFileZipEntry);
                }
                else
                {
                    RarArchiveEntry? entry = GetSmallestGamEntry_Rar(_rarArchive, _baseDirFiles);
                    stream = entry != null ? entry.OpenEntryStream() : misFileRarEntry.OpenEntryStream();
                }

#if FMScanner_FullCode
                ret.Game
#else
                game
#endif
                    = StreamContainsIdentString(
                        stream,
                        _ctx.RopeyArrow,
                        GameTypeBuffer_ChunkPlusRopeyArrow,
                        _gameTypeBufferSize)
                        ? Game.Thief2
                        : Game.Thief1;
            }
            finally
            {
                stream?.Dispose();
            }
        }
        else
        {
            // For uncompressed files on disk, we mercifully can just look at the TOC and then seek to the
            // OBJ_MAP chunk and search it for the string. Phew.
            using FileStreamCustom fs = FileStreamCustom.CreateRead(misFileOnDisk, DiskFileStreamBuffer);

            uint tocOffset = BinaryRead.ReadUInt32(fs, _binaryReadBuffer);

            fs.Position = tocOffset;

            uint invCount = BinaryRead.ReadUInt32(fs, _binaryReadBuffer);
            for (int i = 0; i < invCount; i++)
            {
                int bytesRead = fs.ReadAll(_misChunkHeaderBuffer, 0, 12);
                uint offset = BinaryRead.ReadUInt32(fs, _binaryReadBuffer);
                int length = (int)BinaryRead.ReadUInt32(fs, _binaryReadBuffer);

                if (bytesRead < 12 || !_misChunkHeaderBuffer.Contains(_ctx.OBJ_MAP)) continue;

                // Put us past the name (12), version high (4), version low (4), and the zero (4).
                // Length starts AFTER this 24-byte header! (thanks JayRude)
                fs.Position = offset + 24;

                byte[] content = _sevenZipContext.ByteArrayPool.Rent(length);
                try
                {
                    int objMapBytesRead = fs.ReadAll(content, 0, length);
#if FMScanner_FullCode
                    ret.Game
#else
                    game
#endif
                        = content.Contains(_ctx.RopeyArrow, objMapBytesRead)
                            ? Game.Thief2
                            : Game.Thief1;
                }
                finally
                {
                    _sevenZipContext.ByteArrayPool.Return(content);
                }
                break;
            }
        }

        #endregion

        #region SS2 slow-detect fallback

        /*
        Paranoid fallback. In case MAPPARAM ends up at a different byte location in a future version of
        NewDark, we run this check if we suspect we're dealing with an SS2 FM (we will have fingerprinted
        it earlier during ReadAndCacheFMData() and again here). For T2, we have a fallback scan if we don't
        find SKYOBJVAR at byte 772, so we're safe. But SS2 we should have a fallback in place as well. It's
        really slow, but better slow than incorrect. This way, if a new SS2 FM is released and has MAPPARAM
        in a different place, at least we're like 98% certain to still detect it correctly here. Then people
        can still at least have an accurate detection while I work on a new version that takes the new
        MAPPARAM location into account.
        */

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
        if (
#if FMScanner_FullCode
            ret.Game
#else
            game
#endif
            == Game.Thief1 && (_ss2Fingerprinted || SS2MisFilesPresent(_usedMisFiles, _ctx.FMFiles_SS2MisFiles)))
        {
            using Stream stream = _fmFormat switch
            {
                FMFormat.Zip => _archive.OpenEntry(misFileZipEntry),
                FMFormat.Rar => misFileRarEntry.OpenEntryStream(),
                _ => FileStreamCustom.CreateRead(misFileOnDisk, DiskFileStreamBuffer),
            };
            if (StreamContainsIdentString(
                    stream,
                    MAPPARAM,
                    GameTypeBuffer_ChunkPlusMAPPARAM,
                    _gameTypeBufferSize))
            {
#if FMScanner_FullCode
                ret.Game
#else
                game
#endif
                    = Game.SS2;
            }
        }

        #endregion

#if FMScanner_FullCode
        return ret;
#else
        return game;
#endif
    }

#if FMScanner_FullCode

    private string GetNewDarkVersion()
    {
        foreach (ReadmeInternal readme in _readmeFiles)
        {
            if (!readme.Scan) continue;

            string ndv = GetNewDarkVersionFromText(readme.Text);
            if (!ndv.IsEmpty()) return ndv;
        }

        return "";
    }

    private string GetNewDarkVersionFromText(string text)
    {
        string version = "";

        for (int i = 0; i < _ctx.NewDarkVersionRegexes.Length; i++)
        {
            Match match = _ctx.NewDarkVersionRegexes[i].Match(text);
            if (match.Success)
            {
                version = match.Groups["Version"].Value;
                break;
            }
        }

        if (version.IsEmpty()) return "";

        string ndv = version.Trim(_ctx.CA_Period);
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
            if ((_fmFormat is FMFormat.SevenZip or FMFormat.RarSolid) &&
                !_fmWorkingPath.IsEmpty() &&
                Directory.Exists(_fmWorkingPath))
            {
                DeleteDirectory(_fmWorkingPath);
            }
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
        for (int i = 0; i < value.Length; i++)
        {
            if (value[i] == character) count++;
        }

        return count;
    }

#endif

    /// <summary>
    /// Deletes a directory after first setting everything in it, and itself, to non-read-only.
    /// </summary>
    /// <param name="directory"></param>
    private static void DeleteDirectory(string directory)
    {
        try
        {
            // Assume no readonly files
            Directory.Delete(directory, recursive: true);
        }
        // Readonly file was found (one of the eight million possibilities of this exception anyway...)
        catch (IOException)
        {
            DirAndFileTree_UnSetReadOnly(directory);
            Directory.Delete(directory, recursive: true);
        }
    }

    #region Stream copy with buffer

    private byte[]? _streamCopyBuffer;
    private byte[] StreamCopyBuffer => _streamCopyBuffer ??= new byte[81920];

    #endregion

    #region Generic dir/file functions

    private string[] EnumFiles(string path, SearchOption searchOption)
    {
        try
        {
            return Directory.GetFiles(Path.Combine(_fmWorkingPath, path), "*", searchOption);
        }
        catch (DirectoryNotFoundException)
        {
            return Array.Empty<string>();
        }
    }

    #endregion

    #region Read plaintext

    #region Read all text

    private string ReadAllTextDetectEncoding(Stream stream)
    {
        Encoding encoding = _fileEncoding.DetectFileEncoding(stream) ?? Encoding.GetEncoding(1252);
        stream.Position = 0;

        using var sr = new StreamReaderCustom.SRC_Wrapper(stream, encoding, false, _streamReaderCustom, disposeStream: false);
        return sr.Reader.ReadToEnd();
    }

    private string ReadAllTextUTF8(Stream stream)
    {
        using var sr = new StreamReaderCustom.SRC_Wrapper(stream, Encoding.UTF8, false, _streamReaderCustom, disposeStream: false);
        return sr.Reader.ReadToEnd();
    }

    #endregion

    #region Read all lines

    // The general purpose character encoding detector can't detect OEM 850, so use some domain knowledge to
    // detect it ourselves.

    private static bool TryGetBufferFromStream(Stream stream, [NotNullWhen(true)] out byte[]? buffer, out int bufferLength)
    {
        try
        {
            if (stream is MemoryStream ms)
            {
                try
                {
                    buffer = ms.GetBuffer();
                    bufferLength = (int)stream.Length;
                    return true;
                }
                catch
                {
                    buffer = null;
                    bufferLength = 0;
                    return false;
                }
            }
            else
            {
                buffer = new byte[stream.Length];
                int bytesRead = stream.ReadAll(buffer, 0, buffer.Length);
                if (bytesRead != buffer.Length)
                {
                    buffer = null;
                    bufferLength = 0;
                    return false;
                }
                else
                {
                    bufferLength = buffer.Length;
                    return true;
                }
            }
        }
        finally
        {
            stream.Seek(0, SeekOrigin.Begin);
        }
    }

    private Encoding? GetEncodingFromSuspected(byte[] buffer, int bufferLength)
    {
        int suspected1252ByteCount = 0;
        int suspected850ByteCount = 0;

        for (int i = 0; i < bufferLength; i++)
        {
            byte b = buffer[i];
            suspected1252ByteCount += _ctx.Suspected1252Bytes[b];
            suspected850ByteCount += _ctx.Suspected850Bytes[b];
        }

        if (suspected1252ByteCount == 0 && suspected850ByteCount == 0)
        {
            return null;
        }
        else if (suspected1252ByteCount > suspected850ByteCount)
        {
            return Encoding.GetEncoding(1252);
        }
        else
        {
            return Encoding.GetEncoding(850);
        }
    }

    private bool TryDetectTitlesStrEncoding(Stream stream, [NotNullWhen(true)] out Encoding? encoding)
    {
        try
        {
            if (!TryGetBufferFromStream(stream, out byte[]? buffer, out int bufferLength))
            {
                encoding = null;
                return false;
            }

            Charset bomCharset = CharsetDetector.GetBOMCharset(buffer, bufferLength);
            if (bomCharset != Charset.Null)
            {
                encoding = CharsetDetector.CharsetToEncoding(bomCharset);
                return true;
            }

            // This looks bad for perf, but it takes negligible time over the full set, so optimization is not
            // urgent.
            foreach (byte[] item in _ctx.TitlesStrOEM850KeyPhrases)
            {
                if (buffer.Contains(item, bufferLength))
                {
                    encoding = Encoding.GetEncoding(850);
                    _titlesStrIsOEM850 = true;
                    return true;
                }
            }

            encoding = GetEncodingFromSuspected(buffer, bufferLength);
            if (encoding != null)
            {
                _titlesStrIsOEM850 = encoding.CodePage == 850;
                return true;
            }

            encoding = null;
            return false;
        }
        finally
        {
            stream.Seek(0, SeekOrigin.Begin);
        }
    }

    private bool TryDetectNewGameStrEncoding(Stream stream, [NotNullWhen(true)] out Encoding? encoding)
    {
        try
        {
            if (!TryGetBufferFromStream(stream, out byte[]? buffer, out int bufferLength))
            {
                encoding = null;
                return false;
            }

            Charset bomCharset = CharsetDetector.GetBOMCharset(buffer, bufferLength);
            if (bomCharset != Charset.Null)
            {
                encoding = CharsetDetector.CharsetToEncoding(bomCharset);
                return true;
            }

            if (_titlesStrIsOEM850)
            {
                encoding = Encoding.GetEncoding(850);
                return true;
            }

            encoding = GetEncodingFromSuspected(buffer, bufferLength);
            if (encoding != null)
            {
                return true;
            }

            encoding = null;
            return false;
        }
        finally
        {
            stream.Seek(0, SeekOrigin.Begin);
        }
    }

    private void ReadAllLinesDetectEncoding(
        NameAndIndex item, List<string> lines,
        DetectEncodingType type = DetectEncodingType.Standard)
    {
        lines.Clear();

        if (_fmFormat is FMFormat.Zip or FMFormat.Rar)
        {
            Stream? entryStream = null;
            try
            {
                long entryLength;
                if (_fmFormat == FMFormat.Zip)
                {
                    ZipArchiveFastEntry entry = _archive.Entries[item.Index];
                    entryStream = _archive.OpenEntry(entry);
                    entryLength = entry.Length;
                }
                else
                {
                    RarArchiveEntry entry = _rarArchive.Entries[item.Index];
                    entryStream = entry.OpenEntryStream();
                    entryLength = entry.Size;
                }

                // Detecting the encoding of a stream reads it forward some amount, and I can't seek backwards in
                // an archive stream, so I have to copy it to a seekable MemoryStream. Blah.
                _generalMemoryStream.SetLength(entryLength);
                _generalMemoryStream.Position = 0;

                StreamCopyNoAlloc(entryStream, _generalMemoryStream, StreamCopyBuffer);
                _generalMemoryStream.Position = 0;

                Encoding encoding = DetectEncoding(_generalMemoryStream);
                _generalMemoryStream.Position = 0;

                using var sr = new StreamReaderCustom.SRC_Wrapper(_generalMemoryStream, encoding, false,
                    _streamReaderCustom, disposeStream: false);
                while (sr.Reader.ReadLine() is { } line) lines.Add(line);
            }
            finally
            {
                entryStream?.Dispose();
            }
        }
        else
        {
            using FileStreamCustom stream = FileStreamCustom.CreateRead(Path.Combine(_fmWorkingPath, item.Name), DiskFileStreamBuffer);
            Encoding encoding = DetectEncoding(stream);
            stream.Seek(0, SeekOrigin.Begin);

            using var sr = new StreamReaderCustom.SRC_Wrapper(stream, encoding, false, _streamReaderCustom);
            while (sr.Reader.ReadLine() is { } line) lines.Add(line);
        }

        return;

        Encoding DetectEncoding(Stream stream) =>
            type == DetectEncodingType.NewGameStr &&
            TryDetectNewGameStrEncoding(stream, out Encoding? newGameStrEncoding)
                ? newGameStrEncoding
                : type == DetectEncodingType.TitlesStr &&
                  TryDetectTitlesStrEncoding(stream, out Encoding? titlesStrEncoding)
                    ? titlesStrEncoding
                    : _fileEncoding.DetectFileEncoding(stream) ?? Encoding.GetEncoding(1252);
    }

    private void ReadAllLinesUTF8(NameAndIndex item, List<string> lines)
    {
        lines.Clear();

        using Stream stream = _fmFormat switch
        {
            FMFormat.Zip => _archive.OpenEntry(_archive.Entries[item.Index]),
            FMFormat.Rar => _rarArchive.Entries[item.Index].OpenEntryStream(),
            _ => FileStreamCustom.CreateRead(Path.Combine(_fmWorkingPath, item.Name), DiskFileStreamBuffer),
        };

        // Stupid micro-optimization: Don't call Dispose() method on stream twice
        using var sr = new StreamReaderCustom.SRC_Wrapper(stream, Encoding.UTF8, false, _streamReaderCustom, disposeStream: false);
        while (sr.Reader.ReadLine() is { } line) lines.Add(line);
    }

    #endregion

    #endregion

    #endregion

    [PublicAPI]
    public void Dispose()
    {
        // ReSharper disable ConditionalAccessQualifierIsNonNullableAccordingToAPIContract
        _archive?.Dispose();
        _zipContext?.Dispose();
        _rarArchive?.Dispose();
        _rarStream?.Dispose();
        // ReSharper restore ConditionalAccessQualifierIsNonNullableAccordingToAPIContract
        _streamReaderCustom.DeInit();
        _generalMemoryStream.Dispose();
    }
}
