#if !Rar2017_64bit
using size_t = System.UInt32;
#else
using nint = System.Int64;
using nuint = System.UInt64;
using size_t = System.UInt64;
#endif
using System;
using System.Collections.Generic;
using static SharpCompress.Compressors.Rar.UnpackV2017.PackDef;
using static SharpCompress.Compressors.Rar.UnpackV2017.UnpackGlobal;
using int64 = System.Int64;

// TODO: REMOVE THIS... WIP
#pragma warning disable 169
#pragma warning disable 414

namespace SharpCompress.Compressors.Rar.UnpackV2017;

internal static class UnpackGlobal
{
    // Maximum allowed number of compressed bits processed in quick mode.
    public const int MAX_QUICK_DECODE_BITS = 10;

    // Maximum number of filters per entire data block. Must be at least
    // twice more than MAX_PACK_FILTERS to store filters from two data blocks.
    public const int MAX_UNPACK_FILTERS = 8192;

    // Maximum size of single filter block. We restrict it to limit memory
    // allocation. Must be equal or larger than MAX_ANALYZE_SIZE.
    public const int MAX_FILTER_BLOCK_SIZE = 0x400000;

    // Write data in 4 MB or smaller blocks. Must not exceed PACK_MAX_WRITE,
    // so we keep number of buffered filter in unpacker reasonable.
    public const int UNPACK_MAX_WRITE = 0x400000;
}

// Decode compressed bit fields to alphabet numbers.
internal sealed class DecodeTable
{
    // Real size of DecodeNum table.
    public uint MaxNum;

    // Left aligned start and upper limit codes defining code space
    // ranges for bit lengths. DecodeLen[BitLength-1] defines the start of
    // range for bit length and DecodeLen[BitLength] defines next code
    // after the end of range or in other words the upper limit code
    // for specified bit length.
    public readonly uint[] DecodeLen = new uint[16];

    // Every item of this array contains the sum of all preceding items.
    // So it contains the start position in code list for every bit length.
    public readonly uint[] DecodePos = new uint[16];

    // Number of compressed bits processed in quick mode.
    // Must not exceed MAX_QUICK_DECODE_BITS.
    public uint QuickBits;

    // Translates compressed bits (up to QuickBits length)
    // to bit length in quick mode.
    public readonly byte[] QuickLen = new byte[1 << MAX_QUICK_DECODE_BITS];

    // Translates compressed bits (up to QuickBits length)
    // to position in alphabet in quick mode.
    // 'ushort' saves some memory and even provides a little speed gain
    // comparting to 'uint' here.
    public readonly ushort[] QuickNum = new ushort[1 << MAX_QUICK_DECODE_BITS];

    // Translate the position in code list to position in alphabet.
    // We do not allocate it dynamically to avoid performance overhead
    // introduced by pointer, so we use the largest possible table size
    // as array dimension. Real size of this array is defined in MaxNum.
    // We use this array if compressed bit field is too lengthy
    // for QuickLen based translation.
    // 'ushort' saves some memory and even provides a little speed gain
    // comparting to 'uint' here.
    public readonly ushort[] DecodeNum = new ushort[LARGEST_TABLE_SIZE];
}

internal struct UnpackBlockHeader
{
    public int BlockSize;
    public int BlockBitSize;
    public int BlockStart;
    public int HeaderSize;
    public bool LastBlockInFile;
    public bool TablePresent;
}

internal struct UnpackBlockTables
{
    public DecodeTable LD;  // Decode literals.
    public DecodeTable DD;  // Decode distances.
    public DecodeTable LDD; // Decode lower bits of distances.
    public DecodeTable RD;  // Decode repeating distances.
    public DecodeTable BD;  // Decode bit lengths in Huffman table.

    public void Init()
    {
        LD = new DecodeTable();
        DD = new DecodeTable();
        LDD = new DecodeTable();
        RD = new DecodeTable();
        BD = new DecodeTable();
    }
}


#if RarV2017_RAR_SMP
enum UNP_DEC_TYPE {
UNPDT_LITERAL,UNPDT_MATCH,UNPDT_FULLREP,UNPDT_REP,UNPDT_FILTER
};

struct UnpackDecodedItem
{
UNP_DEC_TYPE Type;
ushort Length;
union
{
uint Distance;
byte Literal[4];
};
};


struct UnpackThreadData
{
Unpack *UnpackPtr;
BitInput Inp;
bool HeaderRead;
UnpackBlockHeader BlockHeader;
bool TableRead;
UnpackBlockTables BlockTables;
int DataSize;    // Data left in buffer. Can be less than block size.
bool DamagedData;
bool LargeBlock;
bool NoDataLeft; // 'true' if file is read completely.
bool Incomplete; // Not entire block was processed, need to read more data.

UnpackDecodedItem *Decoded;
uint DecodedSize;
uint DecodedAllocated;
uint ThreadNumber; // For debugging.

UnpackThreadData()
:Inp(false)
{
Decoded=NULL;
}
~UnpackThreadData()
{
if (Decoded!=NULL)
  free(Decoded);
}
};
#endif


//struct UnpackFilter
internal sealed class UnpackFilter
{
    public byte Type;
    public uint BlockStart;
    public uint BlockLength;
    public byte Channels;
    //  uint Width;
    //  byte PosR;
    public bool NextWindow;
}

internal sealed class AudioVariables // For RAR 2.0 archives only.
{
    public int K1, K2, K3, K4, K5;
    public int D1, D2, D3, D4;
    public int LastDelta;
    public readonly uint[] Dif = new uint[11];
    public uint ByteCount;
    public int LastChar;
}


// We can use the fragmented dictionary in case heap does not have the single
// large enough memory block. It is slower than normal dictionary.
internal sealed partial class FragmentedWindow
{
    private const int MAX_MEM_BLOCKS = 32;

    //void Reset();
    private readonly byte[][] Mem = new byte[MAX_MEM_BLOCKS][];
    private readonly size_t[] MemSize = new size_t[MAX_MEM_BLOCKS];

    //FragmentedWindow();
    //~FragmentedWindow();
    //void Init(size_t WinSize);
    //byte& operator [](size_t Item);
    //void CopyString(uint Length,uint Distance,size_t &UnpPtr,size_t MaxWinMask);
    //void CopyData(byte *Dest,size_t WinPos,size_t Size);
    //size_t GetBlockSize(size_t StartPos,size_t RequiredSize);
}


internal partial class Unpack
{

    //void Unpack5(bool Solid);
    //void Unpack5MT(bool Solid);
    //bool UnpReadBuf();
    //void UnpWriteBuf();
    //byte* ApplyFilter(byte *Data,uint DataSize,UnpackFilter *Flt);
    //void UnpWriteArea(size_t StartPtr,size_t EndPtr);
    //void UnpWriteData(byte *Data,size_t Size);
    //_forceinline uint SlotToLength(BitInput &Inp,uint Slot);
    //void UnpInitData50(bool Solid);
    //bool ReadBlockHeader(BitInput &Inp,UnpackBlockHeader &Header);
    //bool ReadTables(BitInput &Inp,UnpackBlockHeader &Header,UnpackBlockTables &Tables);
    //void MakeDecodeTables(byte *LengthTable,DecodeTable *Dec,uint Size);
    //_forceinline uint DecodeNumber(BitInput &Inp,DecodeTable *Dec);
    //void CopyString();
    //inline void InsertOldDist(uint Distance);
    //void UnpInitData(bool Solid);
    //_forceinline void CopyString(uint Length,uint Distance);
    //uint ReadFilterData(BitInput &Inp);
    //bool ReadFilter(BitInput &Inp,UnpackFilter &Filter);
    //bool AddFilter(UnpackFilter &Filter);
    //bool AddFilter();
    //void InitFilters();

    //ComprDataIO *UnpIO;
    //BitInput Inp;
    private BitInput Inp => this; // hopefully this gets inlined

#if RarV2017_RAR_SMP
void InitMT();
bool UnpackLargeBlock(UnpackThreadData &D);
bool ProcessDecoded(UnpackThreadData &D);

ThreadPool *UnpThreadPool;
UnpackThreadData *UnpThreadData;
uint MaxUserThreads;
byte *ReadBufMT;
#endif

    private byte[] FilterSrcMemory = Array.Empty<byte>();
    private byte[] FilterDstMemory = Array.Empty<byte>();

    // Filters code, one entry per filter.
    private readonly List<UnpackFilter> Filters = new List<UnpackFilter>();

    private readonly uint[] OldDist = new uint[4];
    private uint OldDistPtr;
    private uint LastLength;

    // LastDist is necessary only for RAR2 and older with circular OldDist
    // array. In RAR3 last distance is always stored in OldDist[0].
    private uint LastDist;

    private size_t UnpPtr, WrPtr;

    // Top border of read packed data.
    private int ReadTop;

    // Border to call UnpReadBuf. We use it instead of (ReadTop-C)
    // for optimization reasons. Ensures that we have C bytes in buffer
    // unless we are at the end of file.
    private int ReadBorder;

    private UnpackBlockHeader BlockHeader;
    private UnpackBlockTables BlockTables;

    private size_t WriteBorder;

    private byte[] Window;

    private readonly FragmentedWindow FragWindow = new FragmentedWindow();
    private bool Fragmented;

    private int64 DestUnpSize;

    //bool Suspended;
    private int64 WrittenFileSize;


    /***************************** Unpack v 1.5 *********************************/
    //void Unpack15(bool Solid);
    //void ShortLZ();
    //void LongLZ();
    //void HuffDecode();
    //void GetFlagsBuf();
    //void UnpInitData15(int Solid);
    //void InitHuff();
    //void CorrHuff(ushort *CharSet,byte *NumToPlace);
    //void CopyString15(uint Distance,uint Length);
    //uint DecodeNum(uint Num,uint StartPos,uint *DecTab,uint *PosTab);

    private readonly ushort[] ChSet = new ushort[256], ChSetA = new ushort[256], ChSetB = new ushort[256], ChSetC = new ushort[256];
    private readonly byte[] NToPl = new byte[256], NToPlB = new byte[256], NToPlC = new byte[256];
    private uint FlagBuf, AvrPlc, AvrPlcB, AvrLn1, AvrLn2, AvrLn3;
    private int Buf60, NumHuf, StMode, LCount, FlagsCnt;

    private uint Nhfb, Nlzb, MaxDist3;
    /***************************** Unpack v 1.5 *********************************/

    /***************************** Unpack v 2.0 *********************************/
    //void Unpack20(bool Solid);

    private DecodeTable[] MD = new DecodeTable[4]; // Decode multimedia data, up to 4 channels.

    private readonly byte[] UnpOldTable20 = new byte[MC20 * 4];
    private bool UnpAudioBlock;
    private uint UnpChannels, UnpCurChannel;

    private int UnpChannelDelta;
    //void CopyString20(uint Length,uint Distance);
    //bool ReadTables20();
    //void UnpWriteBuf20();
    //void UnpInitData20(int Solid);
    //void ReadLastTables();
    //byte DecodeAudio(int Delta);
    private AudioVariables[] AudV = new AudioVariables[4];
    /***************************** Unpack v 2.0 *********************************/

    /***************************** Unpack v 3.0 *********************************/

    /*#if !RarV2017_RAR5ONLY
        ModelPPM PPM;
    #endif*/
    private int PPMEscChar;

    // If we already read decoding tables for Unpack v2,v3,v5.
    // We should not use a single variable for all algorithm versions,
    // because we can have a corrupt archive with one algorithm file
    // followed by another algorithm file with "solid" flag and we do not
    // want to reuse tables from one algorithm in another.
    private bool TablesRead2, TablesRead5;

#if RarV2017_RAR_SMP
// More than 8 threads are unlikely to provide a noticeable gain
// for unpacking, but would use the additional memory.
void SetThreads(uint Threads) {MaxUserThreads=Min(Threads,8);}

void UnpackDecode(UnpackThreadData &D);
#endif

    private size_t MaxWinSize;
    private size_t MaxWinMask;
}
