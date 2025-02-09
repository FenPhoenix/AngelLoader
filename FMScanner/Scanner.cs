// Now just surrounds a few ctors and public API methods
//#define FMScanner_FullCode
//#define INDIVIDUAL_FM_TIMING
//#define ScanSynchronous
//#define StoreCurrentFM

/*
@MEM(Scanner readme line splitting):
We could just get the full text and then allocate an array of int pairs for start and length of each line,
then just use that when we need to go line-by-line. It's still an array allocation per readme, but it should
be far less memory allocated than to essentially duplicate the entire readme in separate line form as we do now.

@RAR(Scanner): The rar stuff here is a total mess! It works, but we should clean it up...

@BLOCKS_NOTE: Tested: Solid RAR files work, just without the optimization, as designed

@BLOCKS_NOTE: Could SharpCompress (full) allow us to stream 7z entries to memory?
 Even though it's slower than native 7z.exe, if we have to extract a lot less, then maybe we'd still come out ahead.
 We could scan .mis and .gam files in the usual way, decompressing in chunks etc.
 UPDATE 2025-01-01: Tested this, and surprisingly we gain very little to nothing. SharpCompress is much slower
 at decompressing than native 7z.exe, enough so that it erases most of our time gained. It's probably for the
 best, as it made the code even more horrendously complicated than it already is.

@BLOCKS_NOTE: Non-solid 7z FMs work fine, but our solid-aware paths might be doing more work than necessary in that
 case. TBP non-solid scans very slightly slower than loader-friendly solid (like ~220ms vs ~190ms warm).
 This is not really urgent because it's unlikely anyone will make non-solid 7z FMs, but if we felt like looking
 into non-solid optimizations at some point we could.
*/

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using AL_Common.FastZipReader;
using JetBrains.Annotations;
using SharpCompress.Archives.Rar;
using SharpCompress.Archives.SevenZip;
using SharpCompress.Readers.Rar;
using Ude.NetStandard;
using Ude.NetStandard.SimpleHelpers;
using static System.StringComparison;
using static AL_Common.LanguageSupport;
using static AL_Common.Logger;
using static FMScanner.ReadOnlyDataContext;

namespace FMScanner;

internal enum FMFormat
{
    NotInArchive,
    Zip,
    SevenZip,
    Rar,
    RarSolid,
}

file static class FMFormatExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool IsStreamableArchive(this FMFormat fmFormat) => fmFormat is FMFormat.Zip or FMFormat.Rar;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool IsSolidArchive(this FMFormat fmFormat) => fmFormat is FMFormat.SevenZip or FMFormat.RarSolid;
}

public sealed class Scanner : IDisposable
{
    // Only safe to enable during single-threaded scans, otherwise all threads will hammer on this!
#if StoreCurrentFM
    public static FMToScan _CurrentFM = null!;
#endif

#if DEBUG
    private readonly Stopwatch _overallTimer = new Stopwatch();
#endif

    #region Private fields

    #region Buffers

#if X64
    private readonly FileNameCharBuffer _charBuffer = new();
#endif

    private readonly byte[] _rtfHeaderBuffer = new byte[RTFHeaderBytes.Length];

    private readonly byte[] _misChunkHeaderBuffer = new byte[12];

    private ListFast<char>? _utf32CharBuffer;
    private ListFast<char> Utf32CharBuffer => _utf32CharBuffer ??= new ListFast<char>(2);

    private readonly BinaryBuffer _binaryReadBuffer = new();

    #region Game detection

    private const int _gameTypeBufferSize = 81_920;

    private byte[]? _gameTypeBuffer_ChunkPlusRopeyArrow;
    private byte[] GameTypeBuffer_ChunkPlusRopeyArrow => _gameTypeBuffer_ChunkPlusRopeyArrow ??= new byte[_gameTypeBufferSize + _ctx.RopeyArrow.Length];

    private byte[]? _gameTypeBuffer_ChunkPlusMAPPARAM;
    private byte[] GameTypeBuffer_ChunkPlusMAPPARAM => _gameTypeBuffer_ChunkPlusMAPPARAM ??= new byte[_gameTypeBufferSize + MAPPARAM.Length];

    private byte[]? _gameTypeBuffer_ChunkPlusGAMEPARAM;
    private byte[] GameTypeBuffer_ChunkPlusGAMEPARAM => _gameTypeBuffer_ChunkPlusGAMEPARAM ??= new byte[_gameTypeBufferSize + _ctx.GAMEPARAM.Length];

    private readonly byte[][] _zipOffsetBuffers =
    {
        new byte[SS2_NewDark_MAPPARAM_Offset],
        new byte[T2_OldDark_SKYOBJVAR_Offset],
        new byte[SS2_OldDark_MAPPARAM_Offset],
    };

    // MAPPARAM is 8 bytes, so for that we just check the first 8 bytes and ignore the last, rather than
    // complicating things any further than they already are.
    private const int _gameDetectStringBufferLength = 9;
    private readonly byte[] _gameDetectStringBuffer = new byte[_gameDetectStringBufferLength];

    private byte[]? _streamCopyBuffer;
    private byte[] StreamCopyBuffer => _streamCopyBuffer ??= new byte[81920];

    #endregion

    #endregion

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

    private FMFormat _fmFormat = FMFormat.NotInArchive;

    private string _fmWorkingPath = "";

    private readonly ListFast<string> _titles = new(0);
    private readonly ListFast<string> _titlesTemp = new(0);

    private readonly ListFast<string> _titlesStrLines_Distinct = new(0);

    private readonly ListFast<ReadmeInternal> _readmeFiles = new(10);

    private bool _ss2Fingerprinted;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool SS2FingerprintRequiredAndNotDone() => _scanOptions.ScanGameType && !_ss2Fingerprinted;

    private byte[]? _diskFileStreamBuffer;
    private byte[] DiskFileStreamBuffer => _diskFileStreamBuffer ??= new byte[4096];

    private DirectoryInfo? _fmWorkingPathDirInfo;
    private DirectoryInfo FMWorkingPathDirInfo => _fmWorkingPathDirInfo ??= new DirectoryInfo(_fmWorkingPath);

    private string? _fmWorkingPathDirName;
    private string FMWorkingPathDirName => _fmWorkingPathDirName ??= FMWorkingPathDirInfo.Name;

    #region Solid entry lists

    // 50 entries is more than we're ever likely to need in these lists, but still small enough not to be wasteful.
    private ListFast<SolidEntry>? _solidExtractedEntriesList;
    private ListFast<SolidEntry> SolidExtractedEntriesList => _solidExtractedEntriesList ??= new ListFast<SolidEntry>(50);

    private ListFast<SolidEntry>? _solidExtractedEntriesTempList;
    private ListFast<SolidEntry> SolidZipExtractedEntriesTempList => _solidExtractedEntriesTempList ??= new ListFast<SolidEntry>(50);

    private ListFast<string>? _solidExtractedFilesList;
    private ListFast<string> SolidExtractedFilesList => _solidExtractedFilesList ??= new ListFast<string>(50);

    // @BLOCKS: Lazy-load these?
    private readonly ListFast<SolidEntry> _solid_MisFiles = new(20);
    private readonly ListFast<SolidEntry> _solid_GamFiles = new(10);
    private readonly ListFast<SolidEntry> _solid_MissFlagFiles = new(3);
    private readonly ListFast<SolidEntry> _solid_MisAndGamFiles = new(21);
    private readonly ListFast<NameAndIndex> _solid_MisFileItems = new(20);
    private readonly ListFast<NameAndIndex> _solid_UsedMisFileItems = new(20);
    private readonly ListFast<SolidEntry> _solid_FinalUsedMisFilesList = new(20);
    private readonly ListFast<string> _missFlagExtractList = new(1);

    #endregion

    private readonly ListFast<string> _tempLines = new(0);
    private const int _maxTopLines = 5;
    private readonly ListFast<string> _topLines = new(_maxTopLines);

    private ListFast<char>? _title1_TempNonWhitespaceChars;
    private ListFast<char> Title1_TempNonWhitespaceChars => _title1_TempNonWhitespaceChars ??= new ListFast<char>(50);

    private ListFast<char>? _title2_TempNonWhitespaceChars;
    private ListFast<char> Title2_TempNonWhitespaceChars => _title2_TempNonWhitespaceChars ??= new ListFast<char>(50);

    private readonly ListFast<char> _titleAcronymChars = new(10);
    private readonly ListFast<char> _altTitleAcronymChars = new(10);
    private readonly ListFast<char> _altTitleRomanToDecimalAcronymChars = new(10);

    private readonly ListFast<NameAndIndex> _baseDirFiles = new(20);
    private readonly ListFast<NameAndIndex> _misFiles = new(20);
    private readonly ListFast<NameAndIndex> _usedMisFiles = new(20);
    private readonly ListFast<NameAndIndex> _stringsDirFiles = new(0);
    private readonly ListFast<NameAndIndex> _intrfaceDirFiles = new(0);
    private readonly ListFast<NameAndIndex> _booksDirFiles = new(0);

    private readonly ListFast<NameAndIndex> _readmeDirFiles = new(10);

    private ListFast<NameAndIndex>? _t3FMExtrasDirFiles;
    private ListFast<NameAndIndex> T3FMExtrasDirFiles => _t3FMExtrasDirFiles ??= new ListFast<NameAndIndex>(10);

    private ListFast<NameAndIndex>? _t3GmpFiles;
    private ListFast<NameAndIndex> T3GmpFiles => _t3GmpFiles ??= new ListFast<NameAndIndex>(20);

    private readonly ListFast<DetectedTitle> _detectedTitles = new(6);

    private readonly ScannerTDMContext _tdmContext;

    private bool _missFlagAlreadyHandled;

    private NameAndIndex? _solidMissFlagFileToUse;
    private NameAndIndex? _solidMisFileToUse;
    private NameAndIndex? _solidGamFileToUse;

    #endregion

    #region Enums

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

    private enum SpecialLogic
    {
        Title,
        Author,
        ReleaseDate,
    }

    private enum GetLowestCostMisFileError
    {
        Success,
        SevenZipExtractError,
        NoUsedMisFile,
        Fallback,
    }

    #endregion

    #region Private classes

    private readonly struct SolidEntry
    {
        internal readonly string FullName;
        internal readonly int Index;
        internal readonly long TotalExtractionCost;

        public SolidEntry(string fullName, int index, long totalExtractionCost)
        {
            FullName = fullName;
            Index = index;
            TotalExtractionCost = totalExtractionCost;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal NameAndIndex ToNameAndIndex() => new(FullName, Index);
    }

    /// <summary>
    /// Abstracts over disposable streams vs. the reusable cached MemoryStream. The passed stream won't be disposed
    /// if it's the cached MemoryStream.
    /// </summary>
    [StructLayout(LayoutKind.Auto)]
    private readonly ref struct StreamScope
    {
        private readonly Scanner _scanner;
        internal readonly Stream Stream;

        public StreamScope(Scanner scanner, Stream stream)
        {
            _scanner = scanner;
            Stream = stream;
        }

        public void Dispose()
        {
            if (Stream != _scanner._generalMemoryStream)
            {
                Stream.Dispose();
            }
        }
    }

    [StructLayout(LayoutKind.Auto)]
    private readonly ref struct Entry
    {
        private enum EntryType : byte
        {
            Zip,
            Rar,
            FileInfoCached,
            OnDisk,
        }

        private readonly EntryType _entryType;

        private readonly Scanner _scanner;
        private readonly ZipArchiveFastEntry _zipEntry;
        private readonly RarArchiveEntry _rarEntry;
        private readonly FileInfoCustom _fileInfo;
        private readonly string _fileName;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private string GetFullFileName(string fileName) => Path.Combine(_scanner._fmWorkingPath, fileName);

        private string FullName => _entryType switch
        {
            EntryType.FileInfoCached => GetFullFileName(_fileInfo.FullName),
            EntryType.OnDisk => GetFullFileName(_fileName),
            _ => "",
        };

        internal uint LastModifiedDateRaw => _entryType switch
        {
            EntryType.Zip => _zipEntry.LastWriteTime,
            _ => 0,
        };

        internal DateTime LastModifiedDate => _entryType switch
        {
            EntryType.Rar => _rarEntry.LastModifiedTime ?? DateTime.MinValue,
            EntryType.FileInfoCached => _fileInfo.LastWriteTime,
            EntryType.OnDisk => new FileInfo(GetFullFileName(_fileName)).LastWriteTime,
            _ => default,
        };

        internal DateTime LastModifiedDate_UtcProcessed => _entryType switch
        {
            EntryType.Zip => new DateTimeOffset(ZipHelpers.ZipTimeToDateTime(LastModifiedDateRaw)).DateTime,
            _ => new DateTimeOffset(LastModifiedDate).DateTime,
        };

        internal long UncompressedSize => _entryType switch
        {
            EntryType.Zip => _zipEntry.Length,
            EntryType.Rar => _rarEntry.Size,
            EntryType.FileInfoCached => _fileInfo.Length,
            _ => new FileInfo(GetFullFileName(_fileName)).Length,
        };

        public Entry(Scanner scanner, ZipArchiveFastEntry entry)
        {
            _entryType = EntryType.Zip;
            _scanner = scanner;
            _zipEntry = entry;
            _rarEntry = null!;
            _fileInfo = null!;
            _fileName = "";
        }

        public Entry(Scanner scanner, RarArchiveEntry entry)
        {
            _entryType = EntryType.Rar;
            _scanner = scanner;
            _zipEntry = null!;
            _rarEntry = entry;
            _fileInfo = null!;
            _fileName = "";
        }

        public Entry(Scanner scanner, FileInfoCustom fi)
        {
            _entryType = EntryType.FileInfoCached;
            _scanner = scanner;
            _zipEntry = null!;
            _rarEntry = null!;
            _fileInfo = fi;
            _fileName = "";
        }

        public Entry(Scanner scanner, string fileName)
        {
            _entryType = EntryType.OnDisk;
            _scanner = scanner;
            _zipEntry = null!;
            _rarEntry = null!;
            _fileInfo = null!;
            _fileName = fileName;
        }

        internal Stream Open() => _entryType switch
        {
            EntryType.Zip => _scanner._archive.OpenEntry(_zipEntry),
            EntryType.Rar => _rarEntry.OpenEntryStream(),
            _ => GetReadModeFileStreamWithCachedBuffer(FullName, _scanner.DiskFileStreamBuffer),
        };

        internal StreamScope OpenSeekable()
        {
            Stream entryStream = Open();
            if (entryStream.CanSeek)
            {
                return new StreamScope(_scanner, entryStream);
            }
            else
            {
                try
                {
                    _scanner._generalMemoryStream.SetLength(UncompressedSize);
                    _scanner._generalMemoryStream.Position = 0;
                    StreamCopyNoAlloc(entryStream, _scanner._generalMemoryStream, _scanner.StreamCopyBuffer);
                    _scanner._generalMemoryStream.Position = 0;
                    return new StreamScope(_scanner, _scanner._generalMemoryStream);
                }
                finally
                {
                    entryStream.Dispose();
                }
            }
        }
    }

#if X64
    private sealed class FileNameCharBuffer
    {
        private const uint StartingArrayLength = 256;
        private char[] _array = new char[StartingArrayLength];
        // ulong so we can hold uint max without having to special-case uint max-1
        private ulong _arrayLength = StartingArrayLength;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public char[] GetArray(uint length)
        {
            if (length > _arrayLength)
            {
                _arrayLength = RoundUpToPowerOf2(length);
                _array = new char[_arrayLength];
            }
            return _array;
        }
    }
#endif

    private sealed class DetectedTitle(string value, bool temporary)
    {
        internal string Value = value;
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

        // @BLOCKS: Manual type switch
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
        internal bool Scan;
        internal bool UseForDateDetect;
        internal bool IsGlml;
        internal readonly ListFast<string> Lines = new(0);
        internal string Text = "";

        private uint? _lastModifiedDateRaw;
        private bool _lastModifiedDateConvertedToOffset;
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
                else if (!_lastModifiedDateConvertedToOffset)
                {
                    _lastModifiedDate = new DateTimeOffset(_lastModifiedDate!.Value).DateTime;
                    _lastModifiedDateConvertedToOffset = true;
                }
                return _lastModifiedDate!.Value;
            }
        }

        private ReadmeInternal(bool isGlml, uint lastModifiedDateRaw, bool scan, bool useForDateDetect)
        {
            Set(isGlml, lastModifiedDateRaw, scan, useForDateDetect);
        }

        private ReadmeInternal(bool isGlml, DateTime lastModifiedDate, bool scan, bool useForDateDetect)
        {
            Set(isGlml, lastModifiedDate, scan, useForDateDetect);
        }

        private void Set(bool isGlml, uint lastModifiedDateRaw, bool scan, bool useForDateDetect)
        {
            Text = "";
            Lines.ClearFast();
            IsGlml = isGlml;
            _lastModifiedDateRaw = lastModifiedDateRaw;
            _lastModifiedDate = null;
            _lastModifiedDateConvertedToOffset = false;
            Scan = scan;
            UseForDateDetect = useForDateDetect;
        }

        private void Set(bool isGlml, DateTime lastModifiedDate, bool scan, bool useForDateDetect)
        {
            Text = "";
            Lines.ClearFast();
            IsGlml = isGlml;
            _lastModifiedDateRaw = null;
            _lastModifiedDate = lastModifiedDate;
            _lastModifiedDateConvertedToOffset = false;
            Scan = scan;
            UseForDateDetect = useForDateDetect;
        }

        internal static ReadmeInternal AddReadme(ListFast<ReadmeInternal> readmes, bool isGlml, uint lastModifiedDateRaw, bool scan, bool useForDateDetect)
        {
            if (readmes.Count < readmes.Capacity)
            {
                ReadmeInternal item = readmes[readmes.Count];
                if (item != null!)
                {
                    item.Set(isGlml, lastModifiedDateRaw, scan, useForDateDetect);
                    readmes.Count++;
                    return item;
                }
            }

            ReadmeInternal readme = new(isGlml, lastModifiedDateRaw, scan, useForDateDetect);
            readmes.Add(readme);
            return readme;
        }

        internal static ReadmeInternal AddReadme(ListFast<ReadmeInternal> readmes, bool isGlml, DateTime lastModifiedDate, bool scan, bool useForDateDetect)
        {
            if (readmes.Count < readmes.Capacity)
            {
                ReadmeInternal item = readmes[readmes.Count];
                if (item != null!)
                {
                    item.Set(isGlml, lastModifiedDate, scan, useForDateDetect);
                    readmes.Count++;
                    return item;
                }
            }

            ReadmeInternal readme = new(isGlml, lastModifiedDate, scan, useForDateDetect);
            readmes.Add(readme);
            return readme;
        }

        internal static ReadmeInternal GetReadme(ListFast<ReadmeInternal> readmes, bool isGlml, uint lastModifiedDateRaw, bool scan, bool useForDateDetect)
        {
            return AddReadme(readmes, isGlml, lastModifiedDateRaw, scan, useForDateDetect);
        }
    }

    #endregion

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
        _fullScanOptions = fullScanOptions;

        _ctx = readOnlyDataContext;

        _sevenZipWorkingPath = sevenZipWorkingPath;
        _sevenZipExePath = sevenZipExePath;

        _tdmContext = tdmContext;
    }

    #endregion

    #region Public API

    [PublicAPI]
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
#if INDIVIDUAL_FM_TIMING
        TimingDataList.Clear();
#endif

        if (!TryGetParallelForData(threadCount, fms, cancellationToken, out var pd))
        {
            throw new ArgumentOutOfRangeException(nameof(ParallelOptions.MaxDegreeOfParallelism));
        }

        int fmNumber = 0;
        int fmsCount = fms.Count;

        ConcurrentBag<List<ScannedFMDataAndError>> returnLists = new();

        ReadOnlyDataContext ctx = new();
        try
        {
            _ = Parallel.For(0, threadCount, pd.PO, _ =>
            {
                using Scanner scanner = new(
                    sevenZipWorkingPath: sevenZipWorkingPath,
                    sevenZipExePath: sevenZipExePath,
                    fullScanOptions: fullScanOptions,
                    readOnlyDataContext: ctx,
                    tdmContext: tdmContext);

                returnLists.Add(scanner.ScanMany_Multithreaded(
                    pd.CQ,
                    tempPath: tempPath,
                    scanOptions: scanOptions,
                    progress: progress,
                    cancellationToken: pd.PO.CancellationToken,
                    fmNumber: ref fmNumber,
                    fmsCount: fmsCount,
                    listCapacity: threadCount <= 0 ? fmsCount : fmsCount / threadCount));
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
    Scan(string fmPath, string tempPath, bool forceFullIfNew, string name, bool isArchive)
    {
        List<FMToScan> fms = new()
        {
            new FMToScan(
                path: fmPath,
                forceFullScan: forceFullIfNew,
                displayName: name,
                isTDM: false,
                isArchive: isArchive,
                originalIndex: 0),
        };
        int fmNumber = 0;
        int fmsCount = fms.Count;
        return ScanMany_SingleThread(
            fms,
            tempPath,
            _scanOptions,
            null,
            CancellationToken.None,
            ref fmNumber,
            fmsCount)[0];
    }

    [PublicAPI]
    public ScannedFMDataAndError
    Scan(string fmPath, string tempPath, ScanOptions scanOptions, bool forceFullIfNew, string name, bool isArchive)
    {
        List<FMToScan> fms = new()
        {
            new FMToScan(
                path: fmPath,
                forceFullScan: forceFullIfNew,
                displayName: name,
                isTDM: false,
                isArchive: isArchive,
                originalIndex: 0),
        };
        int fmNumber = 0;
        int fmsCount = fms.Count;
        return ScanMany_SingleThread(
            fms,
            tempPath,
            scanOptions,
            null,
            CancellationToken.None,
            ref fmNumber,
            fmsCount)[0];
    }

    // Debug should also use this - scan on UI thread so breaks will actually break where they're supposed to
    [PublicAPI]
    public List<ScannedFMDataAndError>
    Scan(List<FMToScan> fms, string tempPath, ScanOptions scanOptions, IProgress<ProgressReport> progress, CancellationToken cancellationToken)
    {
        int fmNumber = 0;
        int fmsCount = fms.Count;
        return ScanMany_SingleThread(
            fms,
            tempPath,
            scanOptions,
            progress,
            cancellationToken,
            ref fmNumber,
            fmsCount);
    }

#endif

    #endregion

    #region Scan asynchronous

    [PublicAPI]
    public Task<List<ScannedFMDataAndError>> ScanAsync(
        List<FMToScan> fms,
        string tempPath,
        ScanOptions scanOptions,
        IProgress<ProgressReport> progress,
        CancellationToken cancellationToken)
    {
        int fmNumber = 0;
        int fmsCount = fms.Count;
        return Task.Run(
            () => ScanMany_SingleThread(
                fms,
                tempPath,
                scanOptions,
                progress,
                cancellationToken,
                ref fmNumber,
                fmsCount),
            cancellationToken);
    }

#if FMScanner_FullCode

    [PublicAPI]
    public Task<List<ScannedFMDataAndError>>
    ScanAsync(List<FMToScan> fms, string tempPath)
    {
        int fmNumber = 0;
        int fmsCount = fms.Count;
        return Task.Run(
            () => ScanMany_SingleThread(
                fms,
                tempPath,
                _scanOptions,
                null,
                CancellationToken.None,
                ref fmNumber,
                fmsCount)
        );
    }

    [PublicAPI]
    public Task<List<ScannedFMDataAndError>>
    ScanAsync(List<FMToScan> fms, string tempPath, ScanOptions scanOptions)
    {
        int fmNumber = 0;
        int fmsCount = fms.Count;
        return Task.Run(
            () => ScanMany_SingleThread(
                fms,
                tempPath,
                scanOptions,
                null,
                CancellationToken.None,
                ref fmNumber,
                fmsCount)
        );
    }

    [PublicAPI]
    public Task<List<ScannedFMDataAndError>>
    ScanAsync(List<FMToScan> fms, string tempPath, IProgress<ProgressReport> progress, CancellationToken cancellationToken)
    {
        int fmNumber = 0;
        int fmsCount = fms.Count;
        return Task.Run(
            () => ScanMany_SingleThread(
                fms,
                tempPath,
                _scanOptions,
                progress,
                cancellationToken,
                ref fmNumber,
                fmsCount),
            cancellationToken);
    }

#endif

    #endregion

    #endregion

    private void ResetCachedFields()
    {
        _titles.ClearFast();
        _titlesTemp.ClearFast();
        _titlesStrLines_Distinct.ClearFast();

        _titlesStrIsOEM850 = false;
        _tempLines.ClearFast();
        _topLines.ClearFast();
        _readmeFiles.ClearFast();
        _fmDirFileInfos.ClearFast();
        _ss2Fingerprinted = false;
        _fmWorkingPathDirName = null;
        _fmWorkingPathDirInfo = null;
        _fmFormat = FMFormat.NotInArchive;
        _solidExtractedEntriesList?.ClearFast();
        _solidExtractedEntriesTempList?.ClearFast();
        _solidExtractedFilesList?.ClearFast();

        _title1_TempNonWhitespaceChars?.ClearFast();
        _title2_TempNonWhitespaceChars?.ClearFast();

        _baseDirFiles.ClearFast();
        _misFiles.ClearFast();
        _usedMisFiles.ClearFast();
        _stringsDirFiles.ClearFast();
        _intrfaceDirFiles.ClearFast();
        _booksDirFiles.ClearFast();

        _readmeDirFiles.ClearFast();

        _t3FMExtrasDirFiles?.ClearFast();
        _t3GmpFiles?.ClearFast();

        _detectedTitles.ClearFast();

        _missFlagAlreadyHandled = false;
        _solidMissFlagFileToUse = null;
        _solidMisFileToUse = null;
        _solidGamFileToUse = null;

        _solid_MisFiles.ClearFast();
        _solid_GamFiles.ClearFast();
        _solid_MissFlagFiles.ClearFast();
        _solid_MisAndGamFiles.ClearFast();
        _solid_MisFileItems.ClearFast();
        _solid_UsedMisFileItems.ClearFast();
        _solid_FinalUsedMisFilesList.ClearFast();
        _missFlagExtractList.ClearFast();
    }

    #region Pre-scan

    private (List<ScannedFMDataAndError> ScannedFMDataList, ProgressReport ProgressReport)
    GetInitialScanData(string tempPath, ScanOptions scanOptions, int fmsCount, int listCapacity)
    {
        if (tempPath.IsEmpty())
        {
            Log("Argument is null or empty: " + nameof(tempPath));
            ThrowHelper.ArgumentException("Argument is null or empty.", nameof(tempPath));
        }

        // Deep-copy the scan options object because we might have to change its values in some cases, but we
        // don't want to modify the original because the caller will still have a reference to it and may
        // depend on it not changing.
        _scanOptions = scanOptions?.DeepCopy() ?? throw new ArgumentNullException(nameof(scanOptions));

        List<ScannedFMDataAndError> scannedFMDataList = new(listCapacity);

        ProgressReport progressReport = new()
        {
            FMsCount = fmsCount,
        };

        return (scannedFMDataList, progressReport);
    }

    private List<ScannedFMDataAndError> ScanMany_SingleThread(
        List<FMToScan> fms,
        string tempPath,
        ScanOptions scanOptions,
        IProgress<ProgressReport>? progress,
        CancellationToken cancellationToken,
        ref int fmNumber,
        int fmsCount)
    {
        if (fms == null) throw new ArgumentNullException(nameof(fms));

        (List<ScannedFMDataAndError> scannedFMDataList, ProgressReport progressReport) =
            GetInitialScanData(tempPath, scanOptions, fmsCount, fmsCount);

        for (int i = 0; i < fms.Count; i++)
        {
            FMToScan fm = fms[i];
            ScanFM(
                scannedFMDataList,
                progressReport,
                tempPath,
                fm,
                progress,
                cancellationToken,
                ref fmNumber);
        }

        return scannedFMDataList;
    }

    private List<ScannedFMDataAndError> ScanMany_Multithreaded(
        ConcurrentQueue<FMToScan> fms,
        string tempPath,
        ScanOptions scanOptions,
        IProgress<ProgressReport>? progress,
        CancellationToken cancellationToken,
        ref int fmNumber,
        int fmsCount,
        int listCapacity)
    {
        if (fms == null) throw new ArgumentNullException(nameof(fms));

        (List<ScannedFMDataAndError> scannedFMDataList, ProgressReport progressReport) =
            GetInitialScanData(tempPath, scanOptions, fmsCount, listCapacity);

        while (fms.TryDequeue(out FMToScan fm))
        {
            ScanFM(
                scannedFMDataList,
                progressReport,
                tempPath,
                fm,
                progress,
                cancellationToken,
                ref fmNumber);
        }

        return scannedFMDataList;
    }

    private void ScanFM(
        List<ScannedFMDataAndError> scannedFMDataList,
        ProgressReport progressReport,
        string tempPath,
        FMToScan fm,
        IProgress<ProgressReport>? progress,
        CancellationToken cancellationToken,
        ref int fmNumber)
    {
        // The try-catch blocks are to guarantee that the out-list will at least contain the same number of
        // entries as the in-list; this allows the calling app to not have to do a search to link up the FMs
        // and stuff

        ResetCachedFields();

        // Random name for solid archive temp extract operations, to prevent possible file/folder name
        // clashes in parallelized scenario.
        string tempRandomName = Path_GetRandomFileName().Trim();

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
            progressReport.FMNumber = Interlocked.Increment(ref fmNumber);

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
#if StoreCurrentFM
                    _CurrentFM = fm;
#endif
#if INDIVIDUAL_FM_TIMING
                    _fmTimer.Restart();
#endif
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
#if INDIVIDUAL_FM_TIMING
                    _fmTimer.Stop();
                    TimingDataList.Add(new TimingData(fm.Path, _fmTimer.Elapsed));
#endif

                    if (fm.IsArchive)
                    {
                        DeleteFMWorkingPathIfRequired();
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

    #endregion

#if INDIVIDUAL_FM_TIMING
    private static readonly Stopwatch _fmTimer = new();

    public sealed class TimingData
    {
        public readonly string Path;
        public readonly TimeSpan Time;

        public TimingData(string path, TimeSpan time)
        {
            Path = path;
            Time = time;
        }
    }

    public static List<TimingData> TimingDataList = new();
#endif

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
        ListFast<ZipArchiveFastEntry>? __zipEntries = null;
        ListFast<ZipArchiveFastEntry> GetZipBaseDirEntries()
        {
            if (__zipEntries == null)
            {
                _archive?.Dispose();
                var zipResult = ConstructZipArchive(fm, zipPath, ZipContext, checkForZeroEntries: false, darkModMode: true);
                if (zipResult.Success)
                {
                    _archive = zipResult.Archive!;
                    __zipEntries = _archive.Entries;
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
                ListFast<ZipArchiveFastEntry> entries = GetZipBaseDirEntries();
                for (int i = 0; i < entries.Count; i++)
                {
                    ZipArchiveFastEntry entry = entries[i];
                    if (entry.FullName != FMFiles.TDM_MapSequence) continue;

                    using Stream es = _archive.OpenEntry(entry);
                    // Stupid micro-optimization: Don't call Dispose() method on stream twice
                    using var sr = new StreamReaderCustom.SRC_Wrapper(es, Encoding.UTF8, _streamReaderCustom);

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

            ListFast<string> titles = _titles;

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
        AddReadmeFromPK4(ListFast<ZipArchiveFastEntry> baseDirEntries, string readme1Name, string readme2Name)
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

            void CreateReadme(ZipArchiveFastEntry zipEntry, out ReadmeInternal? readme)
            {
                try
                {
                    Entry entry = new(this, zipEntry);
                    readme = ReadmeInternal.GetReadme(
                        _readmeFiles,
                        isGlml: false,
                        lastModifiedDateRaw: entry.LastModifiedDateRaw,
                        scan: true,
                        useForDateDetect: true);
                    using StreamScope streamScope = entry.OpenSeekable();
                    readme.Text = ReadAllTextDetectEncoding(streamScope.Stream);
                    readme.Lines.AddRange_Large(readme.Text.Split_String(_ctx.SA_Linebreaks, StringSplitOptions.None, _sevenZipContext.IntArrayPool));
                }
                catch
                {
                    readme = null;
                }
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

        #region Setup

        ListFast<SevenZipArchiveEntry> sevenZipEntries = null!;
        SharpCompress.LazyReadOnlyCollection<RarArchiveEntry> rarEntries = null!;

        if (_fmFormat == FMFormat.Rar)
        {
            _rarStream = GetReadModeFileStreamWithCachedBuffer(fm.Path, DiskFileStreamBuffer);
            _rarArchive = RarArchive.Open(_rarStream);
            rarEntries = _rarArchive.Entries;
            // Load the lazy-loaded list so it doesn't throw later
            _ = rarEntries.Count;

            if (_rarArchive.IsSolid)
            {
                _fmFormat = FMFormat.RarSolid;
                _rarArchive.Dispose();
                _rarStream.Dispose();
                _rarStream = GetReadModeFileStreamWithCachedBuffer(fm.Path, DiskFileStreamBuffer);
            }
        }

        bool needsHtmlRefExtract = false;

        ulong sevenZipSize = 0;

        if (_fmFormat.IsSolidArchive())
        {
            (ScannedFMDataAndError? partialExtractError, ListFast<SolidEntry> entries) =
                DoPartialSolidExtract(
                    tempPath,
                    tempRandomName,
                    fm,
                    sevenZipEntries,
                    rarEntries,
                    cancellationToken,
                    out sevenZipSize);

            if (partialExtractError != null)
            {
                return partialExtractError;
            }

            if (!fm.CachePath.IsEmpty())
            {
                try
                {
                    CopyReadmesToCacheResult result = CopyReadmesToCacheDir(fm, entries);
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

        bool success = ReadAndCacheFMData(fm.Path, fmData, out int t3MisCount);
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

        bool singleMission =
            fmIsT3
                ? t3MisCount <= 1
                : _usedMisFiles.Count <= 1;

        fmData.MissionCount =
            fmIsT3
                ? t3MisCount
                : _usedMisFiles.Count;

        if (_scanOptions.GetOptionsEnum() == ScanOptionsEnum.MissionCount)
        {
            // Early return for perf if we're not scanning anything else
            return new ScannedFMDataAndError(fm.OriginalIndex) { ScannedFMData = fmData };
        }

        ListFast<string> titles = _titles;

        bool fmIsSS2 = false;

        if (!fmIsT3)
        {
            #region Game type checks

            if (_scanOptions.ScanGameType)
            {
                fmData.Game = GetGameType();
                if (fmData.Game == Game.Unsupported)
                {
                    return new ScannedFMDataAndError(fm.OriginalIndex) { ScannedFMData = fmData };
                }
            }

            fmIsSS2 = fmData.Game == Game.SS2;

            #endregion

            #region Check info files

            if (_scanOptions.ScanTitle || _scanOptions.ScanAuthor ||
                _scanOptions.ScanReleaseDate || _scanOptions.ScanTags)
            {
                for (int i = 0; i < _baseDirFiles.Count; i++)
                {
                    NameAndIndex f = _baseDirFiles[i];
                    if (f.Name.EqualsI_Local(FMFiles.FMInfoXml))
                    {
                        var (title, author, releaseDate) = ReadFMInfoXml(f);
                        if (_scanOptions.ScanTitle) SetOrAddTitle(titles, title);
                        if (_scanOptions.ScanTags || _scanOptions.ScanAuthor)
                        {
                            fmData.Author = author;
                        }
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
                        var (title, author, lastUpdateDate, tags) = ReadFMIni(f);
                        if (_scanOptions.ScanTitle) SetOrAddTitle(titles, title);
                        if ((_scanOptions.ScanTags || _scanOptions.ScanAuthor) && !author.IsEmpty())
                        {
                            fmData.Author = author;
                        }
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

        #region Set release date

        if (_scanOptions.ScanReleaseDate && fmData.LastUpdateDate == null)
        {
            fmData.LastUpdateDate = GetReleaseDate();
        }

        #endregion

        #region Title

        if (_scanOptions.ScanTitle)
        {
            // SS2 doesn't have a missions list or a titles list file
            if (!fmIsT3 && !fmIsSS2)
            {
                (string titleFrom0, string titleFromN) = GetTitleFromMissionNames();
                SetOrAddTitle(titles, titleFrom0);
                SetOrAddTitle(titles, titleFromN);
            }

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

        // Again, I don't know enough about Thief 3 to know how to detect its languages
        if (!fmIsT3)
        {
            #region Languages

            if (_scanOptions.ScanTags)
            {
                (Language langs, bool englishIsUncertain) = GetLanguages();
                if (langs > Language.Default) SetLangTags(fmData, langs, englishIsUncertain);
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

    #region Solid archive partial extract

    private (ScannedFMDataAndError? Error, ListFast<SolidEntry> Entries)
    DoPartialSolidExtract(
        string tempPath,
        string tempRandomName,
        FMToScan fm,
        ListFast<SevenZipArchiveEntry> sevenZipEntries,
        SharpCompress.LazyReadOnlyCollection<RarArchiveEntry> rarEntries,
        CancellationToken cancellationToken,
        out ulong sevenZipSize)
    {
        sevenZipSize = 0;

        /*
        We try to extract the absolute minimum, but if we can't determine for sure if our extraction cost is
        acceptable, then we fall back to the older method of extracting everything we might possibly need.

        IMPORTANT(Scanner partial solid archive extract):
        The logic for deciding which files to extract (taking files and then de-duping the list) needs
        to match the logic for using them. If we change the usage logic, we need to change this too!
        */

        // Stupid micro-optimization:
        // Init them both just once, avoiding repeated null checks on the properties
        ListFast<SolidEntry> entriesList = SolidExtractedEntriesList;
        ListFast<SolidEntry> tempList = SolidZipExtractedEntriesTempList;

        try
        {
            static bool EndsWithTitleFile(SolidEntry fileName)
            {
                return fileName.FullName.PathEndsWithI_AsciiSecond("/titles.str") ||
                       fileName.FullName.PathEndsWithI_AsciiSecond("/title.str");
            }

            Directory.CreateDirectory(_fmWorkingPath);

            cancellationToken.ThrowIfCancellationRequested();

            /*
            We use SharpCompress for getting the file names and metadata, as that doesn't involve any
            decompression and won't trigger any out-of-memory errors. We use this so we can get last write
            times in DateTime format and not have to parse possible localized text dates out of the output
            stream.
            */
            using (FileStream_NET fs = GetReadModeFileStreamWithCachedBuffer(fm.Path, DiskFileStreamBuffer))
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
                    SolidEntry solidEntry;
                    string fn;
                    long uncompressedSize;
                    // @BLOCKS: Manual type switch
                    if (_fmFormat == FMFormat.SevenZip)
                    {
                        sevenZipEntry = sevenZipEntries[i];
                        if (sevenZipEntry.IsAnti) continue;
                        fn = sevenZipEntry.FileName;
                        uncompressedSize = sevenZipEntry.UncompressedSize;
                        solidEntry = new SolidEntry(fn, i, sevenZipEntry.TotalExtractionCost);
                    }
                    else
                    {
                        rarEntry = rarEntries[i];
                        fn = rarEntry.Key;
                        uncompressedSize = rarEntry.Size;
                        /*
                        @BLOCKS_NOTE: For solid rar just say cost is always 0 for now, because we don't have
                         cost functionality for solid rar yet (and probably won't want to go into the guts of
                         the rar code to add it either).
                        */
                        solidEntry = new SolidEntry(rarEntry.Key, i, 0);
                    }

                    int dirSeps;

                    // Always extract readmes no matter what, so our cache copying is always correct.
                    // Also maybe we would need to always extract them regardless for other reasons, but yeah.
                    if (fn.IsValidReadme() && uncompressedSize > 0 &&
                        (((dirSeps = fn.Rel_CountDirSepsUpToAmount(2)) == 1 &&
                          (fn.PathStartsWithI_AsciiSecond(FMDirs.T3FMExtras1S) ||
                           fn.PathStartsWithI_AsciiSecond(FMDirs.T3FMExtras2S))) ||
                         dirSeps == 0))
                    {
                        entriesList.Add(solidEntry);
                    }
                    else if (fn.IsBaseDirMisOrGamFile())
                    {
                        // We always need to know about mis files to get the used ones for the titles.str
                        // scan, but we won't extract them if we don't actually need to scan them.
                        if (fn.ExtIsMis())
                        {
                            _solid_MisFiles.Add(solidEntry);
                        }
                        else if (_scanOptions.ScanGameType)
                        {
                            _solid_GamFiles.Add(solidEntry);
                        }
                    }
                    else if (!fn.Rel_ContainsDirSep() &&
                             (fn.EqualsI_Local(FMFiles.FMInfoXml) ||
                              fn.EqualsI_Local(FMFiles.FMIni) ||
                              fn.EqualsI_Local(FMFiles.ModIni)))
                    {
                        entriesList.Add(solidEntry);
                    }
                    else if (fn.PathStartsWithI_AsciiSecond(FMDirs.StringsS) &&
                             fn.PathEndsWithI_AsciiSecond(FMFiles.SMissFlag))
                    {
                        _solid_MissFlagFiles.Add(solidEntry);
                    }
                    else if (fn.PathEndsWithI_AsciiSecond(FMFiles.SNewGameStr))
                    {
                        entriesList.Add(solidEntry);
                    }
                    else if (EndsWithTitleFile(solidEntry))
                    {
                        entriesList.Add(solidEntry);
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

            if (!TryAddLowestCostSolidFiles(
                    entriesList,
                    tempPath,
                    tempRandomName,
                    fm,
                    cancellationToken,
                    out ScannedFMDataAndError? solidCostError))
            {
                return (solidCostError, entriesList);
            }

            #region De-duplicate list

            // Some files could have multiple copies in different folders, but we only want to extract
            // the one we're going to use. We separate out this more complex and self-dependent logic
            // here. Doing this nonsense is still faster than extracting to disk.

            static void PopulateTempList(
                ListFast<SolidEntry> fileNamesList,
                ListFast<SolidEntry> tempList,
                Func<SolidEntry, bool> predicate)
            {
                tempList.ClearFast();

                for (int i = 0; i < fileNamesList.Count; i++)
                {
                    SolidEntry fileName = fileNamesList[i];
                    if (predicate(fileName))
                    {
                        tempList.Add(fileName);
                        fileNamesList.RemoveAt(i);
                        i--;
                    }
                }
            }

            PopulateTempList(entriesList, tempList, static x => x.FullName.PathEndsWithI_AsciiSecond(FMFiles.SMissFlag));

            if (!_missFlagAlreadyHandled)
            {
                // TODO: We might be able to put these into a method that takes a predicate so they're not duplicated
                SolidEntry? missFlagToUse = null;
                if (_solid_MisFiles.Count > 1)
                {
                    foreach (var item in tempList)
                    {
                        if (item.FullName.PathEqualsI(FMFiles.StringsMissFlag))
                        {
                            missFlagToUse = item;
                            break;
                        }
                    }
                    if (missFlagToUse == null)
                    {
                        foreach (var item in tempList)
                        {
                            if (item.FullName.PathEqualsI(FMFiles.StringsEnglishMissFlag))
                            {
                                missFlagToUse = item;
                                break;
                            }
                        }
                    }
                    if (missFlagToUse == null)
                    {
                        foreach (var item in tempList)
                        {
                            if (item.FullName.PathEndsWithI_AsciiSecond(FMFiles.SMissFlag))
                            {
                                missFlagToUse = item;
                                break;
                            }
                        }
                    }
                }

                if (missFlagToUse is { } missFlagToUseNonNull)
                {
                    entriesList.Add(missFlagToUseNonNull);
                }
            }

            PopulateTempList(entriesList, tempList, static x => x.FullName.PathEndsWithI_AsciiSecond(FMFiles.SNewGameStr));

            SolidEntry? newGameStrToUse = null;
            foreach (var item in tempList)
            {
                if (item.FullName.PathEqualsI(FMFiles.IntrfaceEnglishNewGameStr))
                {
                    newGameStrToUse = item;
                    break;
                }
            }
            if (newGameStrToUse == null)
            {
                foreach (var item in tempList)
                {
                    if (item.FullName.PathEqualsI(FMFiles.IntrfaceNewGameStr))
                    {
                        newGameStrToUse = item;
                        break;
                    }
                }
            }
            if (newGameStrToUse == null)
            {
                foreach (var item in tempList)
                {
                    if (item.FullName.PathStartsWithI_AsciiSecond(FMDirs.IntrfaceS) &&
                        item.FullName.PathEndsWithI_AsciiSecond(FMFiles.SNewGameStr))
                    {
                        newGameStrToUse = item;
                        break;
                    }
                }
            }

            if (newGameStrToUse is { } newGameStrToUseNonNull)
            {
                entriesList.Add(newGameStrToUseNonNull);
            }

            PopulateTempList(entriesList, tempList, EndsWithTitleFile);

            foreach (string titlesFileLocation in _ctx.FMFiles_TitlesStrLocations)
            {
                SolidEntry? titlesFileToUse = null;
                foreach (SolidEntry item in tempList)
                {
                    if (item.FullName.PathEqualsI(titlesFileLocation))
                    {
                        titlesFileToUse = item;
                        break;
                    }
                }
                if (titlesFileToUse is { } titlesFileToUseNonNull)
                {
                    entriesList.Add(titlesFileToUseNonNull);
                    break;
                }
            }

            #endregion

            ListFast<string> fileNamesList = SolidExtractedFilesList;

            foreach (SolidEntry item in entriesList)
            {
                if (!_scanOptions.ScanGameType &&
                    item.FullName.IsBaseDirMisOrGamFile())
                {
                    continue;
                }
                fileNamesList.Add(item.FullName);
            }

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

                    return (UnsupportedZip(
                        archivePath: fm.Path,
                        fen7zResult: result,
                        ex: null,
                        errorInfo: "7z.exe path: " + _sevenZipExePath + $"{NL}" +
                                   fm.Path + $": fm is 7z{NL}",
                        originalIndex: fm.OriginalIndex), entriesList);
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
                        string finalFileName = ZipHelpers.GetExtractedNameOrThrowIfMalicious(_fmWorkingPath, fn);
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
            return (UnsupportedZip(
                    archivePath: fm.Path,
                    fen7zResult: null,
                    ex: ex,
                    errorInfo: "7z.exe path: " + _sevenZipExePath + $"{NL}" +
                               fm.Path + ": fm is " + fmType + ", exception in " + exType + " extraction",
                    originalIndex: fm.OriginalIndex),
                entriesList);
        }

        return (null, entriesList);
    }

    private bool TryAddLowestCostSolidFiles(
        ListFast<SolidEntry> entriesList,
        string tempPath,
        string tempRandomName,
        FMToScan fm,
        CancellationToken cancellationToken,
        [NotNullWhen(false)] out ScannedFMDataAndError? error)
    {
        error = null;

        /*
        @BLOCKS_NOTE: If a file is 0 length, it will go into block 0, even if other >0 length files are
        in that block. So if we want to check if a file is in a block by itself (for extraction cost purposes),
        we would have to ignore any files in its block that are 0 length. We don't need to do this currently,
        but just a note for the future.
        */

        // @BLOCKS: Implement solid RAR support later
        if (_fmFormat != FMFormat.SevenZip)
        {
            return FillOutNormalList();
        }

        SolidEntry? lowestCostGamFile = GetLowestExtractCostEntry(_solid_GamFiles);

        #region Fast gam file size check

        /*
        Quick check to see if the .gam file is smaller than ANY of the .mis files (used or unused).
        If it is, we don't have to do the expensive used .mis file check that has to extract missflag.str
        separately etc. Of course we'll still have to extract missflag.str later anyway, but it will at least
        be part of the main extract without requiring a separate 7z.exe call.
        */
        if (lowestCostGamFile != null)
        {
            _solid_MisAndGamFiles.AddRange(_solid_MisFiles, _solid_MisFiles.Count);
            _solid_MisAndGamFiles.Add(lowestCostGamFile.Value);

            SolidEntry? lowestCostMisOrGamFile = GetLowestExtractCostEntry(_solid_MisAndGamFiles);
            if (lowestCostMisOrGamFile != null &&
                lowestCostMisOrGamFile.Value.Index == lowestCostGamFile.Value.Index)
            {
                _solidGamFileToUse = CreateAndAdd(lowestCostGamFile);

                entriesList.AddRange(_solid_MissFlagFiles);

                return true;
            }
        }

        #endregion

        SolidEntry? lowestCostMissFlagFile = GetLowestExtractCostEntry(_solid_MissFlagFiles);

        var result =
            GetLowestCostUsedMisFile(
                lowestCostMissFlagFile,
                _solid_MisFiles,
                tempPath,
                tempRandomName,
                fm,
                cancellationToken);

        SolidEntry? lowestCostUsedMisFile = null;

        if (result.Result == GetLowestCostMisFileError.SevenZipExtractError)
        {
            error = UnsupportedZip(
                archivePath: fm.Path,
                fen7zResult: result.SevenZipResult,
                ex: null,
                errorInfo: "7z.exe path: " + _sevenZipExePath + $"{NL}" +
                           fm.Path + $": fm is 7z{NL}",
                originalIndex: fm.OriginalIndex);

            return false;
        }
        else if (result.Result == GetLowestCostMisFileError.Fallback)
        {
            return FillOutNormalList();
        }
        else if (result.Result == GetLowestCostMisFileError.Success)
        {
            lowestCostUsedMisFile = result.MisFile;
        }

        if (lowestCostGamFile is { } gamNonNull &&
            lowestCostUsedMisFile is { } usedMisNonNull)
        {
            if (gamNonNull.TotalExtractionCost <
                usedMisNonNull.TotalExtractionCost)
            {
                _solidGamFileToUse = CreateAndAdd(gamNonNull);
            }
            else
            {
                _solidMisFileToUse = CreateAndAdd(usedMisNonNull);
            }
        }
        else if (lowestCostGamFile != null)
        {
            _solidGamFileToUse = CreateAndAdd(lowestCostGamFile);
        }
        else if (lowestCostUsedMisFile != null)
        {
            _solidMisFileToUse = CreateAndAdd(lowestCostUsedMisFile);
        }
        else
        {
            return FillOutNormalList();
        }

        return true;

        NameAndIndex CreateAndAdd(SolidEntry? entry)
        {
            SolidEntry value = entry!.Value;
            entriesList.Add(value);
            return value.ToNameAndIndex();
        }

        bool FillOutNormalList()
        {
            if (_solid_MisFiles.Count > 1)
            {
                entriesList.AddRange(_solid_MissFlagFiles);
            }
            else
            {
                _solidMissFlagFileToUse = null;
            }

            entriesList.AddRange(_solid_MisFiles);
            entriesList.AddRange(_solid_GamFiles);

            return true;
        }
    }

    private static SolidEntry? GetLowestExtractCostEntry(ListFast<SolidEntry> list)
    {
        int lowestCostIndex = -1;
        long lowestCost = long.MaxValue;
        for (int i = 0; i < list.Count; i++)
        {
            SolidEntry item = list[i];
            if (item.TotalExtractionCost <= lowestCost)
            {
                lowestCost = item.TotalExtractionCost;
                lowestCostIndex = i;
            }
        }

        return lowestCostIndex == -1 ? null : list[lowestCostIndex];
    }

    private (GetLowestCostMisFileError Result, Fen7z.Result? SevenZipResult, SolidEntry MisFile)
    GetLowestCostUsedMisFile(
        SolidEntry? lowestCostMissFlagFile,
        ListFast<SolidEntry> misFiles,
        string tempPath,
        string tempRandomName,
        FMToScan fm,
        CancellationToken cancellationToken)
    {
        if (misFiles.Count == 1)
        {
            SolidEntry item = misFiles[0];
            _solid_UsedMisFileItems.Add(item.ToNameAndIndex());
            return (GetLowestCostMisFileError.Success, null, item);
        }

        if (lowestCostMissFlagFile is { } lowestCostMissFlagFileNonNull)
        {
            /*
            The largest known missflag.str file is 4040 bytes (due to a ton of custom comments, for the
            record). 64KB is small enough that extraction time will be negligible, but large enough to
            cover any missflag.str file that's likely to ever exist.
            */
            if (lowestCostMissFlagFileNonNull.TotalExtractionCost > ByteSize.KB * 64)
            {
                return (GetLowestCostMisFileError.Fallback, null, default);
            }

            string listFile = Path.Combine(tempPath, tempRandomName + ".7zl");

            _missFlagExtractList.Add(lowestCostMissFlagFileNonNull.FullName);

            Fen7z.Result result = Fen7z.Extract(
                sevenZipWorkingPath: _sevenZipWorkingPath,
                sevenZipPathAndExe: _sevenZipExePath,
                archivePath: fm.Path,
                outputPath: _fmWorkingPath,
                cancellationToken: cancellationToken,
                listFile: listFile,
                fileNamesList: _missFlagExtractList);

            if (result.ErrorOccurred)
            {
                Log(fm.Path + $": fm is 7z{NL}" +
                    "7z.exe path: " + _sevenZipExePath + $"{NL}" +
                    result);

                return (GetLowestCostMisFileError.SevenZipExtractError, result, default);
            }

            cancellationToken.ThrowIfCancellationRequested();

            for (int i = 0; i < misFiles.Count; i++)
            {
                SolidEntry item = misFiles[i];
                _solid_MisFileItems.Add(item.ToNameAndIndex());
            }

            NameAndIndex missFlagFile = lowestCostMissFlagFileNonNull.ToNameAndIndex();
            ReadAllLinesUTF8(missFlagFile, _tempLines);
            CacheUsedMisFiles(missFlagFile, _solid_MisFileItems, _solid_UsedMisFileItems, _tempLines);

            _solidMissFlagFileToUse = missFlagFile;
        }
        else
        {
            for (int i = 0; i < misFiles.Count; i++)
            {
                SolidEntry item = misFiles[i];
                _solid_UsedMisFileItems.Add(item.ToNameAndIndex());
            }
        }

        _missFlagAlreadyHandled = true;

        _usedMisFiles.ClearFast();
        foreach (NameAndIndex usedMisFile in _solid_UsedMisFileItems)
        {
            foreach (SolidEntry entry in misFiles)
            {
                if (entry.FullName == usedMisFile.Name)
                {
                    _solid_FinalUsedMisFilesList.Add(entry);
                }
            }
            _usedMisFiles.Add(usedMisFile);
        }

        SolidEntry? lowestCostUsedMisFile = GetLowestExtractCostEntry(_solid_FinalUsedMisFilesList);
        if (lowestCostUsedMisFile is { } lowestCostUsedMisFileNonNull)
        {
            return (GetLowestCostMisFileError.Success, null, lowestCostUsedMisFileNonNull);
        }
        else
        {
            return (GetLowestCostMisFileError.NoUsedMisFile, null, default);
        }
    }

    private CopyReadmesToCacheResult CopyReadmesToCacheDir(FMToScan fm, ListFast<SolidEntry> entries)
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

        if (readmes.Count == 0) return CopyReadmesToCacheResult.Success;

        bool anyHtmlReadmes = false;

        string[] readmeFileNames = new string[readmes.Count];
        for (int i = 0; i < readmes.Count; i++)
        {
            string readme = readmes[i].Source;
            readmeFileNames[i] = readme;
            if (readme.ExtIsHtml())
            {
                anyHtmlReadmes = true;
            }
        }

        if (anyHtmlReadmes)
        {
            List<string> archiveFileNames = new List<string>(entries.Count);
            for (int i = 0; i < entries.Count; i++)
            {
                archiveFileNames.Add(Path.GetFileName(entries[i].FullName));
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

        return CopyReadmesToCacheResult.Success;
    }

    #endregion

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

    // @BLOCKS: Could we merge the solid-extract loop into here, and just extract in between the main loop and
    //  the missflag read?
    private bool ReadAndCacheFMData(string fmPath, ScannedFMData fmd, out int t3MisCount)
    {
        t3MisCount = 0;

        #region Add BaseDirFiles

        bool t3Found = false;

        if (_fmFormat > FMFormat.NotInArchive || _fmDirFileInfos.Count > 0)
        {
            // @BLOCKS: Manual type switch
            int filesCount = _fmFormat switch
            {
                FMFormat.Zip => _archive.Entries.Count,
                FMFormat.Rar => _rarArchive.Entries.Count,
                _ => _fmDirFileInfos.Count,
            };
            for (int i = 0; i < filesCount; i++)
            {
                bool? pathIsIntrfaceDir = null;
                bool? pathIsCutscenes = null;
                bool? pathIsSnd2 = null;

                // @BLOCKS: Manual type switch
                string fn =
                    _fmFormat == FMFormat.Zip ? _archive.Entries[i].FullName :
                    _fmFormat.IsSolidArchive() ? _fmDirFileInfos[i].FullName :
                    _fmFormat == FMFormat.Rar ? _rarArchive.Entries[i].Key :
                    _fmDirFileInfos[i].FullName.Substring(_fmWorkingPath.Length);

                if (fn.PathStartsWithI_AsciiSecond(FMDirs.T3DetectS) &&
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
                else if (fn.PathStartsWithI_AsciiSecond(FMDirs.T3FMExtras1S) ||
                         fn.PathStartsWithI_AsciiSecond(FMDirs.T3FMExtras2S))
                {
                    T3FMExtrasDirFiles.Add(new NameAndIndex(fn, i));
                    continue;
                }
                else if (!fn.Rel_ContainsDirSep() && fn.Contains('.'))
                {
                    _baseDirFiles.Add(new NameAndIndex(fn, i));
                    // Fallthrough so ScanCustomResources can use it
                }
                else if (!t3Found && fn.PathStartsWithI_AsciiSecond(FMDirs.StringsS))
                {
                    _stringsDirFiles.Add(new NameAndIndex(fn, i));
                    if (SS2FingerprintRequiredAndNotDone() &&
                        (fn.PathEndsWithI_AsciiSecond(FMFiles.SS2Fingerprint1) ||
                         fn.PathEndsWithI_AsciiSecond(FMFiles.SS2Fingerprint2) ||
                         fn.PathEndsWithI_AsciiSecond(FMFiles.SS2Fingerprint3) ||
                         fn.PathEndsWithI_AsciiSecond(FMFiles.SS2Fingerprint4)))
                    {
                        _ss2Fingerprinted = true;
                    }
                    continue;
                }
                else if (!t3Found && (pathIsIntrfaceDir ??= fn.PathStartsWithI_AsciiSecond(FMDirs.IntrfaceS)))
                {
                    _intrfaceDirFiles.Add(new NameAndIndex(fn, i));
                    // Fallthrough so ScanCustomResources can use it
                }
                else if (!t3Found && fn.PathStartsWithI_AsciiSecond(FMDirs.BooksS))
                {
                    _booksDirFiles.Add(new NameAndIndex(fn, i));
                    continue;
                }
                else if (!t3Found && SS2FingerprintRequiredAndNotDone() &&
                         ((pathIsCutscenes ??= fn.PathStartsWithI_AsciiSecond(FMDirs.CutscenesS)) ||
                          (pathIsSnd2 ??= fn.PathStartsWithI_AsciiSecond(FMDirs.Snd2S))))
                {
                    _ss2Fingerprinted = true;
                    // Fallthrough so ScanCustomResources can use it
                }

                // Inlined for performance. We cut the time roughly in half by doing this.
                if (!t3Found && _scanOptions.ScanCustomResources)
                {
                    if (fmd.HasAutomap == null &&
                        (pathIsIntrfaceDir ??= fn.PathStartsWithI_AsciiSecond(FMDirs.IntrfaceS)) &&
                        AutomapFileExists(fn))
                    {
                        fmd.HasAutomap = true;
                    }
                    else if (fmd.HasMap == null &&
                             (pathIsIntrfaceDir ?? fn.PathStartsWithI_AsciiSecond(FMDirs.IntrfaceS)) &&
                             MapFileExists(fn))
                    {
                        fmd.HasMap = true;
                    }
                    else if (fmd.HasCustomMotions == null &&
                             fn.PathStartsWithI_AsciiSecond(FMDirs.MotionsS) &&
                             FileExtensionFound(fn, _ctx.MotionFileExtensions))
                    {
                        fmd.HasCustomMotions = true;
                    }
                    else if (fmd.HasMovies == null &&
                             (fn.PathStartsWithI_AsciiSecond(FMDirs.MoviesS) ||
                             (pathIsCutscenes ?? fn.PathStartsWithI_AsciiSecond(FMDirs.CutscenesS))) &&
                             fn.HasFileExtension())
                    {
                        fmd.HasMovies = true;
                    }
                    else if (fmd.HasCustomTextures == null &&
                             fn.PathStartsWithI_AsciiSecond(FMDirs.FamS) &&
                             FileExtensionFound(fn, _ctx.ImageFileExtensions))
                    {
                        fmd.HasCustomTextures = true;
                    }
                    else if (fmd.HasCustomObjects == null &&
                             fn.PathStartsWithI_AsciiSecond(FMDirs.ObjS) &&
                             fn.ExtIsBin())
                    {
                        fmd.HasCustomObjects = true;
                    }
                    else if (fmd.HasCustomCreatures == null &&
                             fn.PathStartsWithI_AsciiSecond(FMDirs.MeshS) &&
                             fn.ExtIsBin())
                    {
                        fmd.HasCustomCreatures = true;
                    }
                    else if ((fmd.HasCustomScripts == null &&
                              !fn.Rel_ContainsDirSep() &&
                              FileExtensionFound(fn, _ctx.ScriptFileExtensions)) ||
                             (fn.PathStartsWithI_AsciiSecond(FMDirs.ScriptsS) &&
                              fn.HasFileExtension()))
                    {
                        fmd.HasCustomScripts = true;
                    }
                    else if (fmd.HasCustomSounds == null &&
                             (fn.PathStartsWithI_AsciiSecond(FMDirs.SndS) ||
                              (pathIsSnd2 ?? fn.PathStartsWithI_AsciiSecond(FMDirs.Snd2S))) &&
                             fn.HasFileExtension())
                    {
                        fmd.HasCustomSounds = true;
                    }
                    else if (fmd.HasCustomSubtitles == null &&
                             fn.PathStartsWithI_AsciiSecond(FMDirs.SubtitlesS) &&
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
                        (f.PathEndsWithI_AsciiSecond(FMFiles.SS2Fingerprint1) ||
                         f.PathEndsWithI_AsciiSecond(FMFiles.SS2Fingerprint2) ||
                         f.PathEndsWithI_AsciiSecond(FMFiles.SS2Fingerprint3) ||
                         f.PathEndsWithI_AsciiSecond(FMFiles.SS2Fingerprint4)))
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
                            BaseDirScriptFileExtensionFound(_baseDirFiles, _ctx.ScriptFileExtensions) ||
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
                        t3MisCount++;
                        break;
                    case > 1:
                        for (int i = 0; i < T3GmpFiles.Count; i++)
                        {
                            NameAndIndex item = T3GmpFiles[i];
                            if (!item.Name.EqualsI_Local(FMFiles.EntryGmp))
                            {
                                t3MisCount++;
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

        if (_missFlagAlreadyHandled) return true;

        NameAndIndex? missFlagFile = null;
        if (_solidMissFlagFileToUse is { } solidMissFlagFileToUse)
        {
            missFlagFile = solidMissFlagFileToUse;
        }
        else
        {
            if (_misFiles.Count > 1 && _stringsDirFiles.Count > 0)
            {
                // I don't remember if I need to search in this exact order, so uh... not rockin' the boat.
                for (int i = 0; i < _stringsDirFiles.Count; i++)
                {
                    NameAndIndex item = _stringsDirFiles[i];
                    if (item.Name.PathEqualsI(FMFiles.StringsMissFlag))
                    {
                        missFlagFile = _stringsDirFiles[i];
                        break;
                    }
                }
                if (missFlagFile == null)
                {
                    for (int i = 0; i < _stringsDirFiles.Count; i++)
                    {
                        NameAndIndex item = _stringsDirFiles[i];
                        if (item.Name.PathEqualsI(FMFiles.StringsEnglishMissFlag))
                        {
                            missFlagFile = _stringsDirFiles[i];
                            break;
                        }
                    }
                }
                if (missFlagFile == null)
                {
                    for (int i = 0; i < _stringsDirFiles.Count; i++)
                    {
                        NameAndIndex item = _stringsDirFiles[i];
                        if (item.Name.PathEndsWithI_AsciiSecond(FMFiles.SMissFlag))
                        {
                            missFlagFile = _stringsDirFiles[i];
                            break;
                        }
                    }
                }
            }
        }

        if (missFlagFile is { } missFlagFileNonNull)
        {
            ReadAllLinesUTF8(missFlagFileNonNull, _tempLines);
        }
        CacheUsedMisFiles(missFlagFile, _misFiles, _usedMisFiles, _tempLines);

        #endregion

        return true;

        #region Local functions

        static bool MapFileExists(string path)
        {
            int lsi;
            return path.Rel_DirSepCountIsAtLeast(1, FMDirs.IntrfaceSLen) &&
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
            return path.Rel_DirSepCountIsAtLeast(1, FMDirs.IntrfaceSLen) &&
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

        static bool BaseDirScriptFileExtensionFound(ListFast<NameAndIndex> baseDirFiles, string[] scriptFileExtensions)
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

        #endregion
    }

    private static void CacheUsedMisFiles(
        NameAndIndex? missFlagFile,
        ListFast<NameAndIndex> misFiles,
        ListFast<NameAndIndex> usedMisFiles,
        ListFast<string> missFlagLines)
    {
        if (missFlagFile != null && misFiles.Count > 1)
        {
            for (int mfI = 0; mfI < misFiles.Count; mfI++)
            {
                NameAndIndex mf = misFiles[mfI];

                // Obtuse nonsense to avoid string allocations (perf)
                if (mf.Name.StartsWithI_Local("miss") && mf.Name[4] != '.')
                {
                    // Since only files ending in .mis are in the misFiles list, we're guaranteed to find a .
                    // character and not get a -1 index. And since we know our file starts with "miss", the
                    // -4 is guaranteed not to take us negative either.
                    int count = mf.Name.IndexOf('.') - 4;
                    for (int mflI = 0; mflI < missFlagLines.Count; mflI++)
                    {
                        string line = missFlagLines[mflI];
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
                                    usedMisFiles.Add(mf);
                                }
                            }
                        }
                    }
                }
            }
        }

        if (usedMisFiles.Count == 0) usedMisFiles.AddRange(misFiles, misFiles.Count);
    }

    #region Read FM info files

    private (string Title, string Author, DateTime? ReleaseDate)
    ReadFMInfoXml(NameAndIndex file)
    {
        string title = "";
        string author = "";
        DateTime? releaseDate = null;

        XmlDocument fmInfoXml = new();

        using (Stream es = GetEntry(file).Open())
        {
            fmInfoXml.Load(es);
        }

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

        using XmlNodeList xReleaseDate = fmInfoXml.GetElementsByTagName("releasedate");
        if (xReleaseDate.Count > 0)
        {
            string rdString = xReleaseDate[0].GetPlainInnerText();
            if (!rdString.IsEmpty()) releaseDate = StringToDate(rdString, checkForAmbiguity: false, out DateTime? dt, out _) ? dt : null;
        }

        // These files also specify languages and whether the mission has custom stuff, but we're not going
        // to trust what we're told - we're going to detect that stuff by looking at what's actually there.

        return (title, author, releaseDate);
    }

    private (string Title, string Author, DateTime? LastUpdateDate, string Tags)
    ReadFMIni(NameAndIndex file)
    {
        var ret = (
            Title: "",
            Author: "",
            LastUpdateDate: (DateTime?)null,
            Tags: ""
        );

        #region Load INI

        ReadAllLinesDetectEncoding(file, _tempLines);

        if (_tempLines.Count == 0)
        {
            return ("", "", null, "");
        }

        (string NiceName, string ReleaseDate, string Tags, string Descr) fmIni = ("", "", "", "");

        #region Deserialize ini

        foreach (string line in _tempLines)
        {
            if (line.StartsWithI_Local("NiceName="))
            {
                fmIni.NiceName = line.Substring(9).Trim();
            }
            else if (line.StartsWithI_Local("ReleaseDate="))
            {
                fmIni.ReleaseDate = line.Substring(12).Trim();
            }
            else if (line.StartsWithI_Local("Tags="))
            {
                fmIni.Tags = line.Substring(5).Trim();
            }
        }

        #endregion

        #endregion

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

        /*
        Notes:
        -fm.ini can specify a readme file, but it may not be the one we're looking for, as far as
         detecting values goes. Reading all .txt and .rtf files is slightly slower but more accurate.
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

            Entry readmeEntry = GetEntry(readmeFile);
            int readmeFileLen = (int)readmeEntry.UncompressedSize;
            if (readmeFileLen == 0) continue;

            bool isGlml = readmeFile.Name.ExtIsGlml();

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

            ReadmeInternal last;

            // We still add the readme even if we're not going to store nor scan its contents, because we still
            // may need to look at its last modified date.
            // @BLOCKS: Explicit archive type switch here, due to difficulty with date raw/not-raw optimization
            if (_fmFormat == FMFormat.Zip)
            {
                last = ReadmeInternal.AddReadme(
                    _readmeFiles,
                    isGlml: isGlml,
                    lastModifiedDateRaw: readmeEntry.LastModifiedDateRaw,
                    scan: scanThisReadme,
                    useForDateDetect: useThisReadmeForDateDetect
                );
            }
            else
            {
                last = ReadmeInternal.AddReadme(
                    _readmeFiles,
                    isGlml: isGlml,
                    lastModifiedDate: readmeEntry.LastModifiedDate,
                    scan: scanThisReadme,
                    useForDateDetect: useThisReadmeForDateDetect
                );
            }

            if (!scanThisReadme) continue;

            Stream? readmeStream = null;
            try
            {
                // Saw one ".rtf" that was actually a plaintext file, and one vice versa. So detect by header
                // alone.
                readmeStream = readmeEntry.Open();

                int rtfHeaderBytesLength = RTFHeaderBytes.Length;

                int rtfBytesRead = 0;
                if (readmeFileLen >= rtfHeaderBytesLength)
                {
                    rtfBytesRead = readmeStream.ReadAll(_rtfHeaderBuffer, 0, rtfHeaderBytesLength);
                }

                if (_fmFormat.IsStreamableArchive())
                {
                    readmeStream.Dispose();
                }
                else
                {
                    /*
                    @PERF_TODO: For seekable streams, we keep the stream open and just seek to the beginning.
                     This is a performance optimization, but it also creates a mess, as we now can't cleanly have
                     a using. How much performance this actually gains us is questionable; it'll be some, but I
                     doubt it's deal-breaking (but need to measure).
                    */
                    readmeStream.Seek(0, SeekOrigin.Begin);
                }

                bool readmeIsRtf = rtfBytesRead >= rtfHeaderBytesLength && _rtfHeaderBuffer.SequenceEqual(RTFHeaderBytes);
                if (readmeIsRtf)
                {
                    using Stream stream = _fmFormat.IsStreamableArchive()
                        ? readmeEntry.Open()
                        : readmeStream;

                    // @MEM(RTF pooled byte arrays): This pool barely helps us
                    // Most of the arrays are used only once, a handful are used twice.
                    byte[] rtfBytes = _sevenZipContext.ByteArrayPool.Rent(readmeFileLen);
                    try
                    {
                        int bytesRead = stream.ReadAll(rtfBytes, 0, readmeFileLen);
                        (bool success, string text) = RtfConverter.Convert(new ArrayWithLength<byte>(rtfBytes, bytesRead));
                        if (success)
                        {
                            last.Text = text;
                        }
                    }
                    finally
                    {
                        _sevenZipContext.ByteArrayPool.Return(rtfBytes);
                    }
                }
                else if (last.IsGlml)
                {
                    using Stream stream = _fmFormat.IsStreamableArchive()
                        ? readmeEntry.Open()
                        : readmeStream;
                    last.Text = Utility.GLMLToPlainText(ReadAllTextUTF8(stream), Utf32CharBuffer);
                }
                else
                {
                    using StreamScope streamScope = _fmFormat.IsStreamableArchive()
                        ? readmeEntry.OpenSeekable()
                        : new StreamScope(this, readmeStream);
                    last.Text = ReadAllTextDetectEncoding(streamScope.Stream);
                }

                last.Lines.AddRange_Large(last.Text.Split_String(_ctx.SA_Linebreaks, StringSplitOptions.None, _sevenZipContext.IntArrayPool));
            }
            finally
            {
                readmeStream?.Dispose();
            }
        }
    }

    #region Get value from readme

    private string GetValueFromReadme(SpecialLogic specialLogic, string[] keys, ListFast<string>? titles = null, ReadmeInternal? singleReadme = null)
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

        value = value.RemoveNonSemanticTrailingPeriod().Trim(_ctx.CA_Asterisk);

        foreach (NonAsciiCharWithAsciiEquivalent item in _ctx.NonAsciiCharsWithAsciiEquivalents)
        {
            value = value.Replace(item.Original, item.Ascii);
        }

        return value;
    }

    #endregion

    #region Title(s) and mission names

    private void SetOrAddTitle(ListFast<string> titles, string value)
    {
        value = CleanupTitle(value).Trim();

        if (value.IsEmpty()) return;

        if (!titles.ContainsI(value))
        {
            titles.Add(value);
        }
    }

    private void SetFMTitles(ScannedFMData fmData, ListFast<string> titles, string? serverTitle = null)
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
        ScannedFMData fmData, ListFast<string> titles, string? serverTitle = null)
    {
        ListFast<string>? topOfReadmeTitles = GetTitlesFromTopOfReadmes();
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

    // This is kind of just an excuse to say that my scanner can catch the full proper title of Deceptive
    // Perception 2. :P
    // This is likely to be a bit loose with its accuracy, but since values caught here are almost certain to
    // end up as alternate titles, I can afford that.
    private ListFast<string>? GetTitlesFromTopOfReadmes()
    {
        if (_readmeFiles.Count == 0) return null;

        ListFast<string>? ret = null;

        foreach (ReadmeInternal r in _readmeFiles)
        {
            if (!r.Scan) continue;

            _topLines.ClearFast();

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
                            ret.ClearFast();
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
                if (item.Name.PathEndsWithI_AsciiSecond(FMFiles.SNewGameStr))
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
                    !CharIsDisallowed(title[0], _ctx) &&
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static bool CharIsDisallowed(char ch, ReadOnlyDataContext ctx)
        {
            return ch < 256 && ctx.NewGameStrDisallowedTitleFirstChars[ch];
        }
    }

    private (string TitleFrom0, string TitleFromN)
    GetTitleFromMissionNames()
    {
        ListFast<string>? titlesStrLines = GetTitlesStrLines();
        if (titlesStrLines == null || titlesStrLines.Count == 0)
        {
            return ("", "");
        }

        var ret = (TitleFrom0: "", TitleFromN: "");

        int titleFromTitlesFoundCount = 0;
        string firstTitleFromTitles = "";
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
                    if (titleFromTitlesFoundCount == 0)
                    {
                        firstTitleFromTitles = title;
                    }
                    titleFromTitlesFoundCount++;
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
                break;
            }
        }

        if (_scanOptions.ScanTitle && titleFromTitlesFoundCount == 1)
        {
            ret.TitleFromN = firstTitleFromTitles;
        }

        return ret;

        static bool NameExistsInList(ListFast<NameAndIndex> list, string value)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].Name.ContainsI(value)) return true;
            }
            return false;
        }
    }

    private ListFast<string>? GetTitlesStrLines()
    {
        ListFast<string>? titlesStrLines = null;

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
        _titlesStrLines_Distinct.ClearFastAndEnsureCapacity(titlesStrLines.Count);

        static bool TitlesStrLinesContainsI(string line, int indexOfColon, ListFast<string> titlesStrLinesDistinct)
        {
            ReadOnlySpan<char> lineSpan = line.AsSpan()[..indexOfColon];
            int lineSpanLength = lineSpan.Length;

            for (int i = 0; i < titlesStrLinesDistinct.Count; i++)
            {
                // Allocation avoidance

                string titlesStrLineDistinct = titlesStrLinesDistinct[i];

                ReadOnlySpan<char> titlesStrLineDistinctSpan = titlesStrLineDistinct.AsSpan();

                int titlesStrLineDistinctSpanLength = titlesStrLineDistinctSpan.Length;

                if (titlesStrLineDistinctSpanLength >= lineSpanLength)
                {
                    Utility.StringCompareReturn strCmpResult = Utility.CompareToOrdinalIgnoreCase(titlesStrLineDistinctSpan[..lineSpanLength], lineSpan);
                    bool result;
                    if (strCmpResult.RequiresStringComparison)
                    {
                        // This path never gets hit in my ~1700 FM set, it's just a fallback in case it ever
                        // encounters a corner case. I think it would require non-ASCII chars.
                        result = titlesStrLineDistinct.StartsWith(line.Substring(0, indexOfColon), OrdinalIgnoreCase);
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
                !TitlesStrLinesContainsI(line, indexOfColon, _titlesStrLines_Distinct))
            {
                _titlesStrLines_Distinct.Add(line);
            }
        }

        _titlesStrLines_Distinct.Sort(_ctx.TitlesStrNaturalNumericSort);

        #endregion

        return _titlesStrLines_Distinct;
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
    private void OrderTitlesOptimally(ListFast<string> originalTitles, string? serverTitle = null)
    {
        if (originalTitles.Count == 0) return;

        ListFast<DetectedTitle> titles = _detectedTitles;
        foreach (string title in originalTitles)
        {
            AddDetectedTitle(titles, title, temporary: false);
        }
        // Ultimate final attack against stubborn titles that just won't be caught
        foreach (ReadmeInternal readme in _readmeFiles)
        {
            if (readme.Lines.Count >= 2 && readme.Lines[1].IsWhiteSpace())
            {
                AddDetectedTitle(titles, readme.Lines[0], temporary: true);
            }
        }
        if (titles.Count < 2) return;

        DetectedTitle mainTitle = titles[0];

        if (mainTitle.Value.IsEmpty()) return;

        _titleAcronymChars.ClearFast();

        byte[] romanNumeralToDecimalTable = _ctx.RomanNumeralToDecimalTable;

        bool titleContainsAcronym = _ctx.AcronymRegex.Match(mainTitle.Value).Success;
        Utility.GetAcronym(mainTitle.Value, _titleAcronymChars);

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

                Utility.GetAcronym(altTitle.Value, _altTitleAcronymChars);
                Utility.GetAcronym_SupportRomanNumerals(altTitle.Value, _altTitleRomanToDecimalAcronymChars, romanNumeralToDecimalTable);

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

        originalTitles.ClearFast();
        for (int i = 0; i < titles.Count; i++)
        {
            DetectedTitle title = titles[i];
            if (i == 0 || !title.Temporary)
            {
                originalTitles.Add(title.Value);
            }
        }

        return;

        static void AddDetectedTitle(ListFast<DetectedTitle> detectedTitles, string title, bool temporary)
        {
            if (detectedTitles.Count < detectedTitles.Capacity)
            {
                DetectedTitle item = detectedTitles[detectedTitles.Count];
                if (item != null!)
                {
                    item.Value = title;
                    item.Temporary = temporary;
                    detectedTitles.Count++;
                    return;
                }
            }

            detectedTitles.Add(new DetectedTitle(title, temporary));
        }

        static bool SwapMainTitleWithTitleAtIndex(ListFast<DetectedTitle> titles, int index)
        {
            (titles[0], titles[index]) = (titles[index], titles[0]);
            titles[0].Temporary = false;
            return true;
        }

        static void DoServerTitleSwap(ListFast<DetectedTitle> titles, string serverTitle)
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

    private void GetAuthor(ScannedFMData fmData, ListFast<string> titles)
    {
        if (fmData.Author.IsEmpty())
        {
            ListFast<string>? passTitles = titles.Count > 0 ? titles : null;
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

    private string GetAuthorFromTopOfReadme(ListFast<string> lines, ListFast<string>? titles)
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
        _topLines.ClearFast();

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

    private string GetAuthorFromTitleByAuthorLine(ListFast<string>? titlesIn)
    {
        if (titlesIn == null || titlesIn.Count == 0) return "";

        ListFast<string> titles = _titlesTemp;
        titles.ClearFastAndEnsureCapacity(titlesIn.Count);

        for (int i = 0; i < titlesIn.Count; i++)
        {
            string title = titlesIn[i];
            // With the new fuzzy match method, it might be possible for me to remove the need for this guard
            if (!title.ContainsI(" by "))
            {
                titles.Add(title);
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

        static string AuthorCopyrightRegexesMatch(string line, Regex[] authorMissionCopyrightRegexes)
        {
            foreach (Regex regex in authorMissionCopyrightRegexes)
            {
                Match match = regex.Match(line);
                if (match.Success) return match.Groups["Author"].Value;
            }
            return "";
        }
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
            author = author.TrimEnd(_ctx.CA_AuthorJunkChars);
        }

        return author.RemoveNonSemanticTrailingPeriod().Trim();
    }

    #endregion

    #region Languages

    private (Language Langs, bool EnglishIsUncertain)
    GetLanguages()
    {
        Language langs = Language.Default;
        bool englishIsUncertain = false;

        for (int dirIndex = 0; dirIndex < 3; dirIndex++)
        {
            ListFast<NameAndIndex> dirFiles = dirIndex switch
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
                    string dfName = df.Name;

                    // We say HasFileExtension() because we only want to count lang dirs that have files in them
                    if (!dfName.HasFileExtension()) continue;

                    string langNeedle1 = _ctx.Languages_FS_Lang_FS[langIndex];
                    if (dfName.Length < langNeedle1.Length) continue;

                    // Directory separator agnostic & keeping perf reasonably high
                    dfName = df.Name.ToForwardSlashes();
#if X64
                    ReadOnlySpan<char> langNeedle1Span = langNeedle1.AsSpan();
                    ReadOnlySpan<char> langNeedle2Span = _ctx.Languages_FS_Lang_Language_FS[langIndex].AsSpan();

                    Span<char> span = GetAsciiLowercaseSpan(dfName);

                    Span<char> spanSlice = span[..dfName.Length];
                    if (spanSlice.IndexOf(langNeedle1Span) > -1 ||
                        spanSlice.IndexOf(langNeedle2Span) > -1)
#else
                    if (dfName.ContainsI(_ctx.Languages_FS_Lang_FS[langIndex]) ||
                        dfName.ContainsI(_ctx.Languages_FS_Lang_Language_FS[langIndex]))
#endif
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

    #endregion

    #region Game type

    private Game GetGameType()
    {
        Game game;
        Entry misFileEntry = default;

        if (_solidGamFileToUse == null)
        {
            NameAndIndex smallestUsedMisFile = GameType_GetSmallestMisFile();
            misFileEntry = GetEntry(smallestUsedMisFile);

            game = GameType_DoQuickCheck(misFileEntry);
            if (game != Game.Null) return game;
        }

        game = GameType_DoMainCheck(misFileEntry);
        if (game != Game.Null) return game;

        game = GameType_DoSS2FallbackCheck(misFileEntry);
        if (game != Game.Null) return game;

        return Game.Thief1;
    }

    private Game GameType_DoQuickCheck(Entry misFileEntry)
    {
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

        * We skip this check because only a handful of OldDark Thief 2 missions have SKYOBJVAR in a wacky
          location, and it's faster and more reliable to simply carry on with the secondary check than to
          try to guess where SKYOBJVAR is in this case.

        For folder scans, we can seek to these positions directly, but for zip scans, we have to read
        through the stream sequentially until we hit each one.
        */

        Stream? misStream = null;
        try
        {
            misStream = misFileEntry.Open();

            for (int i = 0; i < _ctx.GameDetect_KeyPhraseLocations.Length; i++)
            {
                byte[] buffer;

                if (_fmFormat.IsStreamableArchive())
                {
                    buffer = _zipOffsetBuffers[i];
                    int length = _ctx.GameDetect_KeyPhraseZipOffsets[i];
                    int bytesRead = misStream.ReadAll(buffer, 0, length);
                    if (bytesRead < length) break;
                }
                else
                {
                    buffer = _gameDetectStringBuffer;
                    misStream.Position = _ctx.GameDetect_KeyPhraseLocations[i];
                    int bytesRead = misStream.ReadAll(buffer, 0, _gameDetectStringBufferLength);
                    if (bytesRead < _gameDetectStringBufferLength) break;
                }

                if ((_ctx.GameDetect_KeyPhraseLocations[i] == SS2_NewDark_MAPPARAM_Location ||
                     _ctx.GameDetect_KeyPhraseLocations[i] == SS2_OldDark_MAPPARAM_Location) &&
                    EndsWithMAPPARAM(buffer))
                {
                    return Game.SS2;
                }
                else if (_ctx.GameDetect_KeyPhraseLocations[i] == T2_OldDark_SKYOBJVAR_Location &&
                         EndsWithSKYOBJVAR(buffer))
                {
                    return Game.Thief2;
                }
            }

            return Game.Null;
        }
        finally
        {
            misStream?.Dispose();
        }

        // We need to say "length - x" because for zips, the buffer will be full offset size rather than detection
        // string size
        static bool EndsWithSKYOBJVAR(byte[] buffer)
        {
            const ulong SKYOBJVA_ULong = 0x41564A424F594B53;

            int len = buffer.Length;
            return Unsafe.ReadUnaligned<ulong>(ref buffer[len - _gameDetectStringBufferLength]) == SKYOBJVA_ULong &&
                   buffer[len - 1] == (byte)'R';
        }

        static bool EndsWithMAPPARAM(byte[] buffer)
        {
            const ulong MAPPARAM_ULong = 0x4D4152415050414D;

            int len = buffer.Length;
            return Unsafe.ReadUnaligned<ulong>(ref buffer[len - _gameDetectStringBufferLength]) == MAPPARAM_ULong;
        }
    }

    private Game GameType_DoMainCheck(Entry misFileEntry)
    {
        if (_fmFormat.IsStreamableArchive())
        {
            // For zips, since we can't seek within the stream, the fastest way to find our string is just to
            // brute-force straight through.
            Entry entry = GameType_TryGetSmallestGamFileEntry(out Entry gamFileEntry)
                ? gamFileEntry
                : misFileEntry;
            using Stream stream = entry.Open();

            return GameType_StreamContainsIdentString(
                stream,
                _ctx.RopeyArrow,
                GameTypeBuffer_ChunkPlusRopeyArrow,
                _gameTypeBufferSize)
                ? Game.Thief2
                : Game.Null;
        }
        else if (_solidGamFileToUse != null)
        {
            string gamFileOnDisk = Path.Combine(_fmWorkingPath, _solidGamFileToUse.Value.Name);
            using FileStream_NET fs = GetReadModeFileStreamWithCachedBuffer(gamFileOnDisk, DiskFileStreamBuffer);

            if (GAMEPARAM_At_Location(fs, _gameDetectStringBuffer, SS2_Gam_GAMEPARAM_Offset1) ||
                GAMEPARAM_At_Location(fs, _gameDetectStringBuffer, SS2_Gam_GAMEPARAM_Offset2))
            {
                return Game.SS2;
            }

            fs.Position = 0;

            return ChunkContainsPhrase(fs, SymName_First, SymName_Second, _ctx.RopeyArrow) ? Game.Thief2 : Game.Null;
        }
        else
        {
            // For uncompressed files on disk, we mercifully can just look at the TOC and then seek to the
            // OBJ_MAP chunk and search it for the string. Phew.
            using Stream fs = misFileEntry.Open();
            return ChunkContainsPhrase(fs, OBJ_MAP_First, OBJ_MAP_Second, _ctx.RopeyArrow) ? Game.Thief2 : Game.Null;
        }

        static bool GAMEPARAM_At_Location(Stream stream, byte[] buffer, int location)
        {
            if (stream.Length > location + _gameDetectStringBufferLength)
            {
                stream.Position = location;
                int bytesRead = stream.ReadAll(buffer, 0, _gameDetectStringBufferLength);
                if (bytesRead == _gameDetectStringBufferLength)
                {
                    const ulong GAMEPARA_Ulong = 0x41524150454D4147;

                    return Unsafe.ReadUnaligned<ulong>(ref buffer[0]) == GAMEPARA_Ulong &&
                           buffer[^1] == (byte)'M';
                }
            }

            return false;
        }

        bool ChunkContainsPhrase(Stream stream, ulong chunkNameFirst, uint chunkNameSecond, byte[] searchPhrase)
        {
            uint tocOffset = BinaryRead.ReadUInt32(stream, _binaryReadBuffer);

            stream.Position = tocOffset;

            uint invCount = BinaryRead.ReadUInt32(stream, _binaryReadBuffer);
            for (int i = 0; i < invCount; i++)
            {
                int bytesRead = stream.ReadAll(_misChunkHeaderBuffer, 0, _misChunkHeaderBuffer.Length);
                uint offset = BinaryRead.ReadUInt32(stream, _binaryReadBuffer);
                int length = (int)BinaryRead.ReadUInt32(stream, _binaryReadBuffer);

                // IMPORTANT: This MUST come AFTER the offset and length read, because those bump the stream forward!
                if (bytesRead < 12 || !HeaderEquals(_misChunkHeaderBuffer, chunkNameFirst, chunkNameSecond)) continue;

                // Put us past the name (12), version high (4), version low (4), and the zero (4).
                // Length starts AFTER this 24-byte header! (thanks JayRude)
                stream.Position = offset + 24;

                byte[] content = _sevenZipContext.ByteArrayPool.Rent(length);
                try
                {
                    int chunkBytesRead = stream.ReadAll(content, 0, length);
                    return content.Contains(searchPhrase, chunkBytesRead);
                }
                finally
                {
                    _sevenZipContext.ByteArrayPool.Return(content);
                }
            }

            return false;

            static bool HeaderEquals(byte[] header, ulong chunkNameFirst, uint chunkNameSecond)
            {
                ulong first = Unsafe.ReadUnaligned<ulong>(ref header[0]);
                if (first != chunkNameFirst) return false;
                uint second = Unsafe.ReadUnaligned<uint>(ref header[8]);
                return second == chunkNameSecond;
            }
        }
    }

    private Game GameType_DoSS2FallbackCheck(Entry misFileEntry)
    {
        /*
        Paranoid fallback. In case the ident string ends up at a different byte location in a future version of
        NewDark, we run this check if we suspect we're dealing with an SS2 FM (we will have fingerprinted it
        earlier during the FM data caching and again here). For T2, we have a fallback scan if we don't find
        SKYOBJVAR at byte 772, so we're safe. But SS2 we should have a fallback in place as well. It's really
        slow, but better slow than incorrect. This way, if a new SS2 FM is released and has the ident string
        in a different place, at least we're like 98% certain to still detect it correctly here. Then people
        can still at least have an accurate detection while I work on a new version that takes the new ident
        string location into account.
        */

        // Just check the bare ss2 fingerprinted value, because if we're here then we already know it's required
        if ((_ss2Fingerprinted || SS2MisFilesPresent(_usedMisFiles, _ctx.FMFiles_SS2MisFiles)))
        {
            using Stream stream =
                _solidGamFileToUse != null
                    ? GetReadModeFileStreamWithCachedBuffer(Path.Combine(_fmWorkingPath, _solidGamFileToUse.Value.Name), DiskFileStreamBuffer)
                    : misFileEntry.Open();

            (byte[] identifier, byte[] buffer) =
                _solidGamFileToUse != null
                    ? (_ctx.GAMEPARAM, GameTypeBuffer_ChunkPlusGAMEPARAM)
                    : (MAPPARAM, GameTypeBuffer_ChunkPlusMAPPARAM);

            if (GameType_StreamContainsIdentString(
                    stream,
                    identifier,
                    buffer,
                    _gameTypeBufferSize))
            {
                return Game.SS2;
            }
        }

        return Game.Null;

        static bool SS2MisFilesPresent(ListFast<NameAndIndex> misFiles, HashSetI ss2MisFiles)
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
    }

    private NameAndIndex GameType_GetSmallestMisFile()
    {
        if (_solidMisFileToUse is { } solidMisFileToUse)
        {
            return solidMisFileToUse;
        }
        else if (_usedMisFiles.Count == 1)
        {
            return _usedMisFiles[0];
        }
        // We know we have at least 1 used mis file at this point because we early-return way before this if
        // we don't
        else
        {
            int smallestSizeIndex = -1;
            long smallestSize = long.MaxValue;
            for (int i = 0; i < _usedMisFiles.Count; i++)
            {
                Entry misFile = GetEntry(_usedMisFiles[i]);
                long length = misFile.UncompressedSize;
                if (length <= smallestSize)
                {
                    smallestSize = length;
                    smallestSizeIndex = i;
                }
            }

            return _usedMisFiles[smallestSizeIndex];
        }
    }

    private bool GameType_TryGetSmallestGamFileEntry(out Entry result)
    {
        bool found = false;
        long smallestSize = long.MaxValue;
        Entry smallest = default;
        for (int i = 0; i < _baseDirFiles.Count; i++)
        {
            NameAndIndex item = _baseDirFiles[i];
            if (item.Name.ExtIsGam())
            {
                Entry gamFile = GetEntry(item);
                long gamSize = gamFile.UncompressedSize;
                if (gamSize <= smallestSize)
                {
                    found = true;
                    smallestSize = gamSize;
                    smallest = gamFile;
                }
            }
        }

        if (found)
        {
            result = smallest;
            return true;
        }
        else
        {
            result = default;
            return false;
        }
    }

    private static bool GameType_StreamContainsIdentString(
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

    #endregion

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

            misFileDateTime = GetMisFileDate(_usedMisFiles);
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
            : GetMisFileDate(_usedMisFiles).Date;

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

        MisFileDateTime GetMisFileDate(ListFast<NameAndIndex> usedMisFiles)
        {
            if (usedMisFiles.Count > 0)
            {
                DateTime misFileDate = GetEntry(usedMisFiles[0]).LastModifiedDate_UtcProcessed;

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

                if (LineContainsMonthName(lineT, _ctx.MonthNames))
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
    }

    private bool LineContainsMonthName(string line, (string Name, bool IsAscii)[] monthNames)
    {
        foreach (var item in monthNames)
        {
#if X64
            if (item.IsAscii)
            {
                Span<char> span = GetAsciiLowercaseSpan(line);
                if (span.IndexOf(item.Name.AsSpan()) > -1)
                {
                    return true;
                }
            }
            else
#endif
            {
                if (line.ContainsI(item.Name))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private readonly struct StringToDateResult
    {
        internal readonly bool FunctionResult;
        internal readonly DateTime? DateTime;
        internal readonly bool IsAmbiguous;

        public StringToDateResult(bool functionResult, DateTime? dateTime, bool isAmbiguous)
        {
            FunctionResult = functionResult;
            DateTime = dateTime;
            IsAmbiguous = isAmbiguous;
        }
    }

    private readonly Dictionary<string, StringToDateResult> _stringToDateResultCache = new(0);

    private bool StringToDate(string dateString, bool checkForAmbiguity, [NotNullWhen(true)] out DateTime? dateTime, out bool isAmbiguous)
    {
        string originalDateString = dateString;

        #region Early return

        // Believe it or not, there are quite a few instances of the exact same line across different FMs.
        // Anything we can do to avoid all the heavy work in this function is worth doing.
        if (_stringToDateResultCache.TryGetValue(originalDateString, out StringToDateResult stringToDateResult))
        {
            dateTime = stringToDateResult.DateTime;
            isAmbiguous = stringToDateResult.IsAmbiguous;
            return stringToDateResult.FunctionResult;
        }

        // There are two valid "word-only" dates: "Christmas Y2K" and "Halloween Y2K".
        // But we just barely get away with still having this number check work on account of the "2" in "Y2K".
        if (dateString.AsSpan().IndexOfAny("0123456789".AsSpan()) == -1)
        {
            isAmbiguous = false;
            dateTime = null;
            return DoReturn(_stringToDateResultCache, originalDateString, false, dateTime, isAmbiguous);
        }

        #endregion

        // If a date has dot separators, it's probably European format, so we can up our accuracy with regard
        // to guessing about day/month order.
        if (_ctx.EuropeanDateRegex.Match(dateString).Success)
        {
            string dateStringTemp = _ctx.PeriodWithOptionalSurroundingSpacesRegex.Replace(dateString, ".").Trim(_ctx.CA_Period);
            if (DateTime.TryParseExact(
                    dateStringTemp,
                    _ctx.DateFormatsEuropean,
                    DateTimeFormatInfo.InvariantInfo,
                    DateTimeStyles.None,
                    out DateTime eurDateResult))
            {
                dateTime = eurDateResult;
                isAmbiguous = eurDateResult.Month != eurDateResult.Day;
                return DoReturn(_stringToDateResultCache, originalDateString, true, dateTime, isAmbiguous);
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
        foreach (var item in _ctx.DateFormats)
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
            return DoReturn(_stringToDateResultCache, originalDateString, false, dateTime, isAmbiguous);
        }

        if (!checkForAmbiguity || !canBeAmbiguous)
        {
            isAmbiguous = false;
            dateTime = result;
            return DoReturn(_stringToDateResultCache, originalDateString, true, dateTime, isAmbiguous);
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
                return DoReturn(_stringToDateResultCache, originalDateString, true, dateTime, isAmbiguous);
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
        return DoReturn(_stringToDateResultCache, originalDateString, true, dateTime, isAmbiguous);

        static bool DoReturn(
            Dictionary<string, StringToDateResult> cache,
            string originalDateString,
            bool returnValue,
            DateTime? dateTime,
            bool isAmbiguous)
        {
            cache[originalDateString] = new StringToDateResult(returnValue, dateTime, isAmbiguous);
            return returnValue;
        }
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
            Match match = Regex.Match(fmData.TagsString, @"language:\s*" + lang, Regex_IgnoreCaseInvariant);
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

    #region Helpers

    private void DeleteFMWorkingPathIfRequired()
    {
        try
        {
            // IMPORTANT: _DO NOT_ delete the working path if we're a folder FM to start with!
            // That means our working path is NOT temporary!!!
            if (_fmFormat.IsSolidArchive() &&
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

    private string[] EnumFiles(string path, SearchOption searchOption)
    {
        try
        {
            string dir = Path.Combine(_fmWorkingPath, path);
            // Exists check is faster than letting GetFiles() fail
            return Directory.Exists(dir) ? Directory.GetFiles(dir, "*", searchOption) : Array.Empty<string>();
        }
        catch (DirectoryNotFoundException)
        {
            return Array.Empty<string>();
        }
    }

    #region Read plaintext

    #region Read all text

    private string ReadAllTextDetectEncoding(Stream stream)
    {
        Encoding encoding = _fileEncoding.DetectFileEncoding(stream) ?? Encoding.GetEncoding(1252);
        stream.Position = 0;

        using var sr = new StreamReaderCustom.SRC_Wrapper(stream, encoding, _streamReaderCustom);
        return sr.Reader.ReadToEnd();
    }

    private string ReadAllTextUTF8(Stream stream)
    {
        using var sr = new StreamReaderCustom.SRC_Wrapper(stream, Encoding.UTF8, _streamReaderCustom);
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
        NameAndIndex item,
        ListFast<string> lines,
        DetectEncodingType type = DetectEncodingType.Standard)
    {
        lines.ClearFast();

        Entry entry = GetEntry(item);
        using StreamScope streamScope = entry.OpenSeekable();

        Encoding encoding =
            type == DetectEncodingType.NewGameStr &&
            TryDetectNewGameStrEncoding(streamScope.Stream, out Encoding? newGameStrEncoding)
                ? newGameStrEncoding
                : type == DetectEncodingType.TitlesStr &&
                  TryDetectTitlesStrEncoding(streamScope.Stream, out Encoding? titlesStrEncoding)
                    ? titlesStrEncoding
                    : _fileEncoding.DetectFileEncoding(streamScope.Stream) ?? Encoding.GetEncoding(1252);

        streamScope.Stream.Seek(0, SeekOrigin.Begin);

        using StreamReaderCustom.SRC_Wrapper sr = new(streamScope.Stream, encoding, _streamReaderCustom);
        while (sr.Reader.ReadLine() is { } line) lines.Add(line);
    }

    private void ReadAllLinesUTF8(NameAndIndex item, ListFast<string> lines)
    {
        lines.ClearFast();

        Entry entry = GetEntry(item);
        using StreamScope streamScope = new(this, entry.Open());

        using StreamReaderCustom.SRC_Wrapper sr = new(streamScope.Stream, Encoding.UTF8, _streamReaderCustom);
        while (sr.Reader.ReadLine() is { } line) lines.Add(line);
    }

    #endregion

    #endregion

#if X64
    private Span<char> GetAsciiLowercaseSpan(string value)
    {
        /*
        .NET Framework's string.IndexOf(OrdinalIgnoreCase) is slow, so let's do a horrific hack to speed it up
        by 2.5-3x. We don't need any of this on modern .NET, as its string.IndexOf(OrdinalIgnoreCase) is bonkers
        fast as usual.
        */

        char[] array = _charBuffer.GetArray((uint)value.Length);
        Span<char> span = array.AsSpan(0, value.Length);
        value.AsSpan().CopyTo(span);

        /*
        I thought for sure I'd have to vectorize the ASCII to-lower conversion to get any sort of performance
        (which I couldn't figure out how to do anyway), but it turns out this dumbass scalar loop with a branch
        inside is still crazy fast compared to the old string.IndexOf(OrdinalIgnoreCase). Hey, I'll take it.
        */
        // Reverse for loop to eliminate bounds checking
        for (int i = value.Length - 1; i >= 0; i--)
        {
            char c = array[i];
            if (c.IsAsciiUpper())
            {
                c |= (char)0x20;
                array[i] = c;
            }
        }

        return span;
    }
#endif

    private Entry GetEntry(NameAndIndex item) => _fmFormat switch
    {
        FMFormat.Zip => new Entry(this, _archive.Entries[item.Index]),
        FMFormat.Rar => new Entry(this, _rarArchive.Entries[item.Index]),
        _ => _fmDirFileInfos.Count > 0
            ? new Entry(this, _fmDirFileInfos[item.Index])
            : new Entry(this, item.Name),
    };

    private static (bool Success, ScannedFMDataAndError? ScannedFMDataAndError, ZipArchiveFast? Archive)
    ConstructZipArchive(FMToScan fm, string path, ZipContext zipContext, bool checkForZeroEntries, bool darkModMode = false)
    {
        ZipArchiveFast? ret;

        try
        {
            ret = ZipArchiveFast.Create_Scan(
                stream: GetReadModeFileStreamWithCachedBuffer(path, zipContext.FileStreamBuffer),
                context: zipContext,
                darkMod: darkModMode);

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

    #endregion

    [PublicAPI]
    public void Dispose()
    {
        // ReSharper disable ConditionalAccessQualifierIsNonNullableAccordingToAPIContract
        _archive?.Dispose();
        _rarArchive?.Dispose();
        _rarStream?.Dispose();
        // ReSharper restore ConditionalAccessQualifierIsNonNullableAccordingToAPIContract
        _streamReaderCustom.DeInit();
        _generalMemoryStream.Dispose();
    }
}
