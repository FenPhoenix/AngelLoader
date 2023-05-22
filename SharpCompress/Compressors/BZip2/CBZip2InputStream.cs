#nullable disable

using System;
using System.IO;

/*
 * Copyright 2001,2004-2005 The Apache Software Foundation
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

/*
 * This package is based on the work done by Keiron Liddle, Aftex Software
 * <keiron@aftexsw.com> to whom the Ant project is very grateful for his
 * great code.
 */

namespace SharpCompress.Compressors.BZip2;

/**
  * An input stream that decompresses from the BZip2 format (with the file
  * header chars) to be read as any other stream.
  *
  * @author <a href="mailto:keiron@aftexsw.com">Keiron Liddle</a>
  *
  * <b>NB:</b> note this class has been modified to read the leading BZ from the
  * start of the BZIP2 stream to make it compatible with other PGP programs.
  */

internal sealed class CBZip2InputStream : Stream
{
    private static void Cadvise()
    {
        //System.out.Println("CRC Error");
        //throw new CCoruptionError();
    }

    private static void CompressedStreamEOF() => Cadvise();

    private void MakeMaps()
    {
        int i;
        _nInUse = 0;
        for (i = 0; i < 256; i++)
        {
            if (_inUse[i])
            {
                _seqToUnseq[_nInUse] = (char)i;
                _unseqToSeq[i] = (char)_nInUse;
                _nInUse++;
            }
        }
    }

    /*
    index of the last char in the block, so
    the block size == last + 1.
    */
    private int _last;

    /*
    index in zptr[] of original string after sorting.
    */
    private int _origPtr;

    /*
    always: in the range 0 .. 9.
    The current block size is 100000 * this number.
    */
    private int _blockSize100k;

    private bool _blockRandomised;

    private int _bsBuff;
    private int _bsLive;
    private readonly CRC _mCrc = new CRC();

    private readonly bool[] _inUse = new bool[256];
    private int _nInUse;

    private readonly char[] _seqToUnseq = new char[256];
    private readonly char[] _unseqToSeq = new char[256];

    private readonly char[] _selector = new char[BZip2Constants.MAX_SELECTORS];
    private readonly char[] _selectorMtf = new char[BZip2Constants.MAX_SELECTORS];

    private int[] _tt;
    private char[] _ll8;

    /*
    freq table collected to save a pass over the data
    during decompression.
    */
    private readonly int[] _unzftab = new int[256];

    private readonly int[][] _limit = InitIntArray(
        BZip2Constants.N_GROUPS,
        BZip2Constants.MAX_ALPHA_SIZE
    );
    private readonly int[][] _basev = InitIntArray(
        BZip2Constants.N_GROUPS,
        BZip2Constants.MAX_ALPHA_SIZE
    );
    private readonly int[][] _perm = InitIntArray(
        BZip2Constants.N_GROUPS,
        BZip2Constants.MAX_ALPHA_SIZE
    );
    private readonly int[] _minLens = new int[BZip2Constants.N_GROUPS];

    private Stream _bsStream;

    private bool _streamEnd;

    private int _currentChar = -1;

    private const int START_BLOCK_STATE = 1;
    private const int RAND_PART_A_STATE = 2;
    private const int RAND_PART_B_STATE = 3;
    private const int RAND_PART_C_STATE = 4;
    private const int NO_RAND_PART_A_STATE = 5;
    private const int NO_RAND_PART_B_STATE = 6;
    private const int NO_RAND_PART_C_STATE = 7;

    private int _currentState = START_BLOCK_STATE;

    private int _storedBlockCRC,
        _storedCombinedCRC;
    private int _computedBlockCRC,
        _computedCombinedCRC;

    private int _i2,
        _count,
        _chPrev,
        _ch2;
    private int _i,
        _tPos;
    private int _rNToGo;
    private int _rTPos;
    private int _j2;
    private char _z;
    private bool _isDisposed;

    public CBZip2InputStream(Stream zStream)
    {
        _ll8 = null;
        _tt = null;
        BsSetStream(zStream);
        Initialize(true);
        InitBlock();
        SetupBlock();
    }

    protected override void Dispose(bool disposing)
    {
        if (_isDisposed)
        {
            return;
        }
        _isDisposed = true;
        base.Dispose(disposing);
        _bsStream?.Dispose();
    }

    private static int[][] InitIntArray(int n1, int n2)
    {
        int[][] a = new int[n1][];
        for (int k = 0; k < n1; ++k)
        {
            a[k] = new int[n2];
        }
        return a;
    }

    private static char[][] InitCharArray(int n1, int n2)
    {
        char[][] a = new char[n1][];
        for (int k = 0; k < n1; ++k)
        {
            a[k] = new char[n2];
        }
        return a;
    }

    public override int ReadByte()
    {
        if (_streamEnd)
        {
            return -1;
        }
        int retChar = _currentChar;
        switch (_currentState)
        {
            case START_BLOCK_STATE:
                break;
            case RAND_PART_A_STATE:
                break;
            case RAND_PART_B_STATE:
                SetupRandPartB();
                break;
            case RAND_PART_C_STATE:
                SetupRandPartC();
                break;
            case NO_RAND_PART_A_STATE:
                break;
            case NO_RAND_PART_B_STATE:
                SetupNoRandPartB();
                break;
            case NO_RAND_PART_C_STATE:
                SetupNoRandPartC();
                break;
        }
        return retChar;
    }

    private bool Initialize(bool isFirstStream)
    {
        int magic0 = _bsStream.ReadByte();
        int magic1 = _bsStream.ReadByte();
        int magic2 = _bsStream.ReadByte();
        if (magic0 == -1 && !isFirstStream)
        {
            return false;
        }
        if (magic0 != 'B' || magic1 != 'Z' || magic2 != 'h')
        {
            throw new IOException("Not a BZIP2 marked stream");
        }
        int magic3 = _bsStream.ReadByte();
        if (magic3 < '1' || magic3 > '9')
        {
            BsFinishedWithStream();
            _streamEnd = true;
            return false;
        }

        SetDecompressStructureSizes(magic3 - '0');
        _bsLive = 0;
        _computedCombinedCRC = 0;
        return true;
    }

    private void InitBlock()
    {
        char magic1,
            magic2,
            magic3,
            magic4;
        char magic5,
            magic6;

        while (true)
        {
            magic1 = BsGetUChar();
            magic2 = BsGetUChar();
            magic3 = BsGetUChar();
            magic4 = BsGetUChar();
            magic5 = BsGetUChar();
            magic6 = BsGetUChar();
            if (
                magic1 != 0x17
                || magic2 != 0x72
                || magic3 != 0x45
                || magic4 != 0x38
                || magic5 != 0x50
                || magic6 != 0x90
            )
            {
                break;
            }

            if (Complete())
            {
                return;
            }
        }

        if (
            magic1 != 0x31
            || magic2 != 0x41
            || magic3 != 0x59
            || magic4 != 0x26
            || magic5 != 0x53
            || magic6 != 0x59
        )
        {
            BadBlockHeader();
            _streamEnd = true;
            return;
        }

        _storedBlockCRC = BsGetInt32();

        if (BsR(1) == 1)
        {
            _blockRandomised = true;
        }
        else
        {
            _blockRandomised = false;
        }

        //        currBlockNo++;
        GetAndMoveToFrontDecode();

        _mCrc.InitialiseCRC();
        _currentState = START_BLOCK_STATE;
    }

    private void EndBlock()
    {
        _computedBlockCRC = _mCrc.GetFinalCRC();
        /* A bad CRC is considered a fatal error. */
        if (_storedBlockCRC != _computedBlockCRC)
        {
            CrcError();
        }

        _computedCombinedCRC = (_computedCombinedCRC << 1) | _computedCombinedCRC >>> 31;
        _computedCombinedCRC ^= _computedBlockCRC;
    }

    private bool Complete()
    {
        _storedCombinedCRC = BsGetInt32();
        if (_storedCombinedCRC != _computedCombinedCRC)
        {
            CrcError();
        }

        bool complete = !Initialize(false);
        if (complete)
        {
            BsFinishedWithStream();
            _streamEnd = true;
        }

        // Look for the next .bz2 stream if decompressing
        // concatenated files.
        return complete;
    }

    private static void BlockOverrun() => Cadvise();

    private static void BadBlockHeader() => Cadvise();

    private static void CrcError() => Cadvise();

    private void BsFinishedWithStream()
    {
        _bsStream?.Dispose();
        _bsStream = null;
    }

    private void BsSetStream(Stream f)
    {
        _bsStream = f;
        _bsLive = 0;
        _bsBuff = 0;
    }

    private int BsR(int n)
    {
        int v;
        while (_bsLive < n)
        {
            int zzi;
            int thech = '\0';
            try
            {
                thech = (char)_bsStream.ReadByte();
            }
            catch (IOException)
            {
                CompressedStreamEOF();
            }
            if (thech == '\uffff')
            {
                CompressedStreamEOF();
            }
            zzi = thech;
            _bsBuff = (_bsBuff << 8) | (zzi & 0xff);
            _bsLive += 8;
        }

        v = (_bsBuff >> (_bsLive - n)) & ((1 << n) - 1);
        _bsLive -= n;
        return v;
    }

    private char BsGetUChar() => (char)BsR(8);

    private int BsGetint()
    {
        int u = 0;
        u = (u << 8) | BsR(8);
        u = (u << 8) | BsR(8);
        u = (u << 8) | BsR(8);
        u = (u << 8) | BsR(8);
        return u;
    }

    private int BsGetIntVS(int numBits) => BsR(numBits);

    private int BsGetInt32() => BsGetint();

    private static void HbCreateDecodeTables(
        int[] limit,
        int[] basev,
        int[] perm,
        char[] length,
        int minLen,
        int maxLen,
        int alphaSize
    )
    {
        int pp,
            i,
            j,
            vec;

        pp = 0;
        for (i = minLen; i <= maxLen; i++)
        {
            for (j = 0; j < alphaSize; j++)
            {
                if (length[j] == i)
                {
                    perm[pp] = j;
                    pp++;
                }
            }
        }

        for (i = 0; i < BZip2Constants.MAX_CODE_LEN; i++)
        {
            basev[i] = 0;
        }
        for (i = 0; i < alphaSize; i++)
        {
            basev[length[i] + 1]++;
        }

        for (i = 1; i < BZip2Constants.MAX_CODE_LEN; i++)
        {
            basev[i] += basev[i - 1];
        }

        for (i = 0; i < BZip2Constants.MAX_CODE_LEN; i++)
        {
            limit[i] = 0;
        }
        vec = 0;

        for (i = minLen; i <= maxLen; i++)
        {
            vec += (basev[i + 1] - basev[i]);
            limit[i] = vec - 1;
            vec <<= 1;
        }
        for (i = minLen + 1; i <= maxLen; i++)
        {
            basev[i] = ((limit[i - 1] + 1) << 1) - basev[i];
        }
    }

    private void RecvDecodingTables()
    {
        char[][] len = InitCharArray(BZip2Constants.N_GROUPS, BZip2Constants.MAX_ALPHA_SIZE);
        int i,
            j,
            t,
            nGroups,
            nSelectors,
            alphaSize;
        int minLen,
            maxLen;
        bool[] inUse16 = new bool[16];

        /* Receive the mapping table */
        for (i = 0; i < 16; i++)
        {
            if (BsR(1) == 1)
            {
                inUse16[i] = true;
            }
            else
            {
                inUse16[i] = false;
            }
        }

        for (i = 0; i < 256; i++)
        {
            _inUse[i] = false;
        }

        for (i = 0; i < 16; i++)
        {
            if (inUse16[i])
            {
                for (j = 0; j < 16; j++)
                {
                    if (BsR(1) == 1)
                    {
                        _inUse[(i * 16) + j] = true;
                    }
                }
            }
        }

        MakeMaps();
        alphaSize = _nInUse + 2;

        /* Now the selectors */
        nGroups = BsR(3);
        nSelectors = BsR(15);
        for (i = 0; i < nSelectors; i++)
        {
            j = 0;
            while (BsR(1) == 1)
            {
                j++;
            }
            _selectorMtf[i] = (char)j;
        }

        /* Undo the MTF values for the selectors. */
        {
            char[] pos = new char[BZip2Constants.N_GROUPS];
            char tmp,
                v;
            for (v = '\0'; v < nGroups; v++)
            {
                pos[v] = v;
            }

            for (i = 0; i < nSelectors; i++)
            {
                v = _selectorMtf[i];
                tmp = pos[v];
                while (v > 0)
                {
                    pos[v] = pos[v - 1];
                    v--;
                }
                pos[0] = tmp;
                _selector[i] = tmp;
            }
        }

        /* Now the coding tables */
        for (t = 0; t < nGroups; t++)
        {
            int curr = BsR(5);
            for (i = 0; i < alphaSize; i++)
            {
                while (BsR(1) == 1)
                {
                    if (BsR(1) == 0)
                    {
                        curr++;
                    }
                    else
                    {
                        curr--;
                    }
                }
                len[t][i] = (char)curr;
            }
        }

        /* Create the Huffman decoding tables */
        for (t = 0; t < nGroups; t++)
        {
            minLen = 32;
            maxLen = 0;
            for (i = 0; i < alphaSize; i++)
            {
                if (len[t][i] > maxLen)
                {
                    maxLen = len[t][i];
                }
                if (len[t][i] < minLen)
                {
                    minLen = len[t][i];
                }
            }
            HbCreateDecodeTables(_limit[t], _basev[t], _perm[t], len[t], minLen, maxLen, alphaSize);
            _minLens[t] = minLen;
        }
    }

    private void GetAndMoveToFrontDecode()
    {
        char[] yy = new char[256];
        int i,
            j,
            nextSym,
            limitLast;
        int EOB,
            groupNo,
            groupPos;

        limitLast = BZip2Constants.baseBlockSize * _blockSize100k;
        _origPtr = BsGetIntVS(24);

        RecvDecodingTables();
        EOB = _nInUse + 1;
        groupNo = -1;
        groupPos = 0;

        /*
        Setting up the unzftab entries here is not strictly
        necessary, but it does save having to do it later
        in a separate pass, and so saves a block's worth of
        cache misses.
        */
        for (i = 0; i <= 255; i++)
        {
            _unzftab[i] = 0;
        }

        for (i = 0; i <= 255; i++)
        {
            yy[i] = (char)i;
        }

        _last = -1;

        {
            int zt,
                zn,
                zvec,
                zj;
            if (groupPos == 0)
            {
                groupNo++;
                groupPos = BZip2Constants.G_SIZE;
            }
            groupPos--;
            zt = _selector[groupNo];
            zn = _minLens[zt];
            zvec = BsR(zn);
            while (zvec > _limit[zt][zn])
            {
                zn++;
                {
                    {
                        while (_bsLive < 1)
                        {
                            int zzi;
                            char thech = '\0';
                            try
                            {
                                thech = (char)_bsStream.ReadByte();
                            }
                            catch (IOException)
                            {
                                CompressedStreamEOF();
                            }
                            if (thech == '\uffff')
                            {
                                CompressedStreamEOF();
                            }
                            zzi = thech;
                            _bsBuff = (_bsBuff << 8) | (zzi & 0xff);
                            _bsLive += 8;
                        }
                    }
                    zj = (_bsBuff >> (_bsLive - 1)) & 1;
                    _bsLive--;
                }
                zvec = (zvec << 1) | zj;
            }
            nextSym = _perm[zt][zvec - _basev[zt][zn]];
        }

        while (true)
        {
            if (nextSym == EOB)
            {
                break;
            }

            if (nextSym == BZip2Constants.RUNA || nextSym == BZip2Constants.RUNB)
            {
                char ch;
                int s = -1;
                int N = 1;
                do
                {
                    if (nextSym == BZip2Constants.RUNA)
                    {
                        s += (0 + 1) * N;
                    }
                    else if (nextSym == BZip2Constants.RUNB)
                    {
                        s += (1 + 1) * N;
                    }
                    N *= 2;
                    {
                        int zt,
                            zn,
                            zvec,
                            zj;
                        if (groupPos == 0)
                        {
                            groupNo++;
                            groupPos = BZip2Constants.G_SIZE;
                        }
                        groupPos--;
                        zt = _selector[groupNo];
                        zn = _minLens[zt];
                        zvec = BsR(zn);
                        while (zvec > _limit[zt][zn])
                        {
                            zn++;
                            {
                                {
                                    while (_bsLive < 1)
                                    {
                                        int zzi;
                                        char thech = '\0';
                                        try
                                        {
                                            thech = (char)_bsStream.ReadByte();
                                        }
                                        catch (IOException)
                                        {
                                            CompressedStreamEOF();
                                        }
                                        if (thech == '\uffff')
                                        {
                                            CompressedStreamEOF();
                                        }
                                        zzi = thech;
                                        _bsBuff = (_bsBuff << 8) | (zzi & 0xff);
                                        _bsLive += 8;
                                    }
                                }
                                zj = (_bsBuff >> (_bsLive - 1)) & 1;
                                _bsLive--;
                            }
                            zvec = (zvec << 1) | zj;
                        }
                        nextSym = _perm[zt][zvec - _basev[zt][zn]];
                    }
                } while (nextSym == BZip2Constants.RUNA || nextSym == BZip2Constants.RUNB);

                s++;
                ch = _seqToUnseq[yy[0]];
                _unzftab[ch] += s;

                while (s > 0)
                {
                    _last++;
                    _ll8[_last] = ch;
                    s--;
                }

                if (_last >= limitLast)
                {
                    BlockOverrun();
                }
            }
            else
            {
                char tmp;
                _last++;
                if (_last >= limitLast)
                {
                    BlockOverrun();
                }

                tmp = yy[nextSym - 1];
                _unzftab[_seqToUnseq[tmp]]++;
                _ll8[_last] = _seqToUnseq[tmp];

                /*
                This loop is hammered during decompression,
                hence the unrolling.

                for (j = nextSym-1; j > 0; j--) yy[j] = yy[j-1];
                */

                j = nextSym - 1;
                for (; j > 3; j -= 4)
                {
                    yy[j] = yy[j - 1];
                    yy[j - 1] = yy[j - 2];
                    yy[j - 2] = yy[j - 3];
                    yy[j - 3] = yy[j - 4];
                }
                for (; j > 0; j--)
                {
                    yy[j] = yy[j - 1];
                }

                yy[0] = tmp;
                {
                    int zt,
                        zn,
                        zvec,
                        zj;
                    if (groupPos == 0)
                    {
                        groupNo++;
                        groupPos = BZip2Constants.G_SIZE;
                    }
                    groupPos--;
                    zt = _selector[groupNo];
                    zn = _minLens[zt];
                    zvec = BsR(zn);
                    while (zvec > _limit[zt][zn])
                    {
                        zn++;
                        {
                            {
                                while (_bsLive < 1)
                                {
                                    int zzi;
                                    char thech = '\0';
                                    try
                                    {
                                        thech = (char)_bsStream.ReadByte();
                                    }
                                    catch (IOException)
                                    {
                                        CompressedStreamEOF();
                                    }
                                    zzi = thech;
                                    _bsBuff = (_bsBuff << 8) | (zzi & 0xff);
                                    _bsLive += 8;
                                }
                            }
                            zj = (_bsBuff >> (_bsLive - 1)) & 1;
                            _bsLive--;
                        }
                        zvec = (zvec << 1) | zj;
                    }
                    nextSym = _perm[zt][zvec - _basev[zt][zn]];
                }
            }
        }
    }

    private void SetupBlock()
    {
        Span<int> cftab = stackalloc int[257];
        char ch;

        cftab[0] = 0;
        for (_i = 1; _i <= 256; _i++)
        {
            cftab[_i] = _unzftab[_i - 1];
        }
        for (_i = 1; _i <= 256; _i++)
        {
            cftab[_i] += cftab[_i - 1];
        }

        for (_i = 0; _i <= _last; _i++)
        {
            ch = _ll8[_i];
            _tt[cftab[ch]] = _i;
            cftab[ch]++;
        }

        _tPos = _tt[_origPtr];

        _count = 0;
        _i2 = 0;
        _ch2 = 256; /* not a char and not EOF */

        if (_blockRandomised)
        {
            _rNToGo = 0;
            _rTPos = 0;
            SetupRandPartA();
        }
        else
        {
            SetupNoRandPartA();
        }
    }

    private void SetupRandPartA()
    {
        if (_i2 <= _last)
        {
            _chPrev = _ch2;
            _ch2 = _ll8[_tPos];
            _tPos = _tt[_tPos];
            if (_rNToGo == 0)
            {
                _rNToGo = BZip2Constants.rNums[_rTPos];
                _rTPos++;
                if (_rTPos == 512)
                {
                    _rTPos = 0;
                }
            }
            _rNToGo--;
            _ch2 ^= (_rNToGo == 1) ? 1 : 0;
            _i2++;

            _currentChar = _ch2;
            _currentState = RAND_PART_B_STATE;
            _mCrc.UpdateCRC(_ch2);
        }
        else
        {
            EndBlock();
            InitBlock();
            SetupBlock();
        }
    }

    private void SetupNoRandPartA()
    {
        if (_i2 <= _last)
        {
            _chPrev = _ch2;
            _ch2 = _ll8[_tPos];
            _tPos = _tt[_tPos];
            _i2++;

            _currentChar = _ch2;
            _currentState = NO_RAND_PART_B_STATE;
            _mCrc.UpdateCRC(_ch2);
        }
        else
        {
            EndBlock();
            InitBlock();
            SetupBlock();
        }
    }

    private void SetupRandPartB()
    {
        if (_ch2 != _chPrev)
        {
            _currentState = RAND_PART_A_STATE;
            _count = 1;
            SetupRandPartA();
        }
        else
        {
            _count++;
            if (_count >= 4)
            {
                _z = _ll8[_tPos];
                _tPos = _tt[_tPos];
                if (_rNToGo == 0)
                {
                    _rNToGo = BZip2Constants.rNums[_rTPos];
                    _rTPos++;
                    if (_rTPos == 512)
                    {
                        _rTPos = 0;
                    }
                }
                _rNToGo--;
                _z ^= (char)((_rNToGo == 1) ? 1 : 0);
                _j2 = 0;
                _currentState = RAND_PART_C_STATE;
                SetupRandPartC();
            }
            else
            {
                _currentState = RAND_PART_A_STATE;
                SetupRandPartA();
            }
        }
    }

    private void SetupRandPartC()
    {
        if (_j2 < _z)
        {
            _currentChar = _ch2;
            _mCrc.UpdateCRC(_ch2);
            _j2++;
        }
        else
        {
            _currentState = RAND_PART_A_STATE;
            _i2++;
            _count = 0;
            SetupRandPartA();
        }
    }

    private void SetupNoRandPartB()
    {
        if (_ch2 != _chPrev)
        {
            _currentState = NO_RAND_PART_A_STATE;
            _count = 1;
            SetupNoRandPartA();
        }
        else
        {
            _count++;
            if (_count >= 4)
            {
                _z = _ll8[_tPos];
                _tPos = _tt[_tPos];
                _currentState = NO_RAND_PART_C_STATE;
                _j2 = 0;
                SetupNoRandPartC();
            }
            else
            {
                _currentState = NO_RAND_PART_A_STATE;
                SetupNoRandPartA();
            }
        }
    }

    private void SetupNoRandPartC()
    {
        if (_j2 < _z)
        {
            _currentChar = _ch2;
            _mCrc.UpdateCRC(_ch2);
            _j2++;
        }
        else
        {
            _currentState = NO_RAND_PART_A_STATE;
            _i2++;
            _count = 0;
            SetupNoRandPartA();
        }
    }

    private void SetDecompressStructureSizes(int newSize100k)
    {
        if (!(0 <= newSize100k && newSize100k <= 9 && 0 <= _blockSize100k && _blockSize100k <= 9))
        {
            // throw new IOException("Invalid block size");
        }

        _blockSize100k = newSize100k;

        if (newSize100k == 0)
        {
            return;
        }

        int n = BZip2Constants.baseBlockSize * newSize100k;
        _ll8 = new char[n];
        _tt = new int[n];
    }

    public override void Flush() { }

    public override int Read(byte[] buffer, int offset, int count)
    {
        int k;
        for (k = 0; k < count; ++k)
        {
            int c = ReadByte();
            if (c == -1)
            {
                break;
            }
            buffer[k + offset] = (byte)c;
        }
        return k;
    }

    public override long Seek(long offset, SeekOrigin origin) => 0;

    public override void SetLength(long value) { }

    public override void Write(byte[] buffer, int offset, int count) { }

    public override void WriteByte(byte value) { }

    public override bool CanRead => true;

    public override bool CanSeek => false;

    public override bool CanWrite => false;

    public override long Length => 0;

    public override long Position
    {
        get => 0;
        set { }
    }
}
