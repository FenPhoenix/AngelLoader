﻿#if true
using System;
using System.Collections.Generic;
using SharpCompress.Compressors.Rar.UnpackV1.Decode;

namespace SharpCompress.Compressors.Rar.UnpackV1;

internal partial class Unpack
{
    // Maximum number of filters per entire data block. Must be at least
    // twice more than MAX_PACK_FILTERS to store filters from two data blocks.
    private const int MAX_UNPACK_FILTERS = 8192;

    // Maximum size of single filter block. We restrict it to limit memory
    // allocation. Must be equal or larger than MAX_ANALYZE_SIZE.
    private const int MAX_FILTER_BLOCK_SIZE = 0x400000;

    // Write data in 4 MB or smaller blocks. Must not exceed PACK_MAX_WRITE,
    // so we keep number of buffered filter in unpacker reasonable.
    private const int UNPACK_MAX_WRITE = 0x400000;

    private readonly bool TablesRead5;
    private int WriteBorder;

    // TODO: see logic in unpack.cpp Unpack::Init()
    private const int MaxWinSize = PackDef.MAXWINSIZE;
    private const int MaxWinMask = PackDef.MAXWINMASK;

    // TODO: rename var
    private int UnpPtr
    {
        get => unpPtr;
        set => unpPtr = value;
    }
    private int ReadBorder
    {
        get => readBorder;
        set => readBorder = value;
    }
    private long DestUnpSize => destUnpSize;
    private long WrittenFileSize => writtenFileSize;
    private byte[] Window => window;

    private uint LastLength
    {
        get => (uint)lastLength;
        set => lastLength = (int)value;
    }

    private uint OldDistN(int i) => (uint)oldDist[i];

    private void SetOldDistN(int i, uint value) => oldDist[i] = (int)value;

    private int WrPtr => wrPtr;
    private Unpack BlockHeader => this;

    private Unpack Header => this;

    private int ReadTop
    {
        get => readTop;
        set => readTop = value;
    }
    private List<UnpackFilter> Filters => filters;

    // TODO: make sure these aren't already somewhere else
    private int BlockSize;
    private int BlockBitSize;
    private int BlockStart;
    private bool LastBlockInFile;

    private void Unpack5(bool Solid)
    {
        FileExtracted = true;

        if (!Suspended)
        {
            UnpInitData(Solid);
            if (!UnpReadBuf())
            {
                return;
            }

            // Check TablesRead5 to be sure that we read tables at least once
            // regardless of current block header TablePresent flag.
            // So we can safely use these tables below.
            if (!ReadBlockHeader() || !ReadTables() || !TablesRead5)
            {
                return;
            }
        }

        while (true)
        {
            UnpPtr &= MaxWinMask;

            if (Inp.InAddr >= ReadBorder)
            {
                var FileDone = false;

                // We use 'while', because for empty block containing only Huffman table,
                // we'll be on the block border once again just after reading the table.
                while (
                    Inp.InAddr > BlockHeader.BlockStart + BlockHeader.BlockSize - 1
                    || Inp.InAddr == BlockHeader.BlockStart + BlockHeader.BlockSize - 1
                        && Inp.InBit >= BlockHeader.BlockBitSize
                )
                {
                    if (BlockHeader.LastBlockInFile)
                    {
                        FileDone = true;
                        break;
                    }
                    if (!ReadBlockHeader() || !ReadTables())
                    {
                        return;
                    }
                }
                if (FileDone || !UnpReadBuf())
                {
                    break;
                }
            }

            if (
                ((WriteBorder - UnpPtr) & MaxWinMask) < PackDef.MAX_LZ_MATCH + 3
                && WriteBorder != UnpPtr
            )
            {
                UnpWriteBuf();
                if (WrittenFileSize > DestUnpSize)
                {
                    return;
                }

                if (Suspended)
                {
                    FileExtracted = false;
                    return;
                }
            }

            //uint MainSlot=DecodeNumber(Inp,LD);
            var MainSlot = this.DecodeNumber(LD);
            if (MainSlot < 256)
            {
                //              if (Fragmented)
                //                FragWindow[UnpPtr++]=(byte)MainSlot;
                //              else
                Window[UnpPtr++] = (byte)MainSlot;
                continue;
            }
            if (MainSlot >= 262)
            {
                var Length = SlotToLength(MainSlot - 262);

                //uint DBits,Distance=1,DistSlot=DecodeNumber(Inp,&BlockTables.DD);
                int DBits;
                uint Distance = 1,
                    DistSlot = this.DecodeNumber(DD);
                if (DistSlot < 4)
                {
                    DBits = 0;
                    Distance += DistSlot;
                }
                else
                {
                    //DBits=DistSlot/2 - 1;
                    DBits = (int)((DistSlot / 2) - 1);
                    Distance += (2 | (DistSlot & 1)) << DBits;
                }

                if (DBits > 0)
                {
                    if (DBits >= 4)
                    {
                        if (DBits > 4)
                        {
                            Distance += ((Inp.getbits() >> (36 - DBits)) << 4);
                            Inp.AddBits(DBits - 4);
                        }
                        //uint LowDist=DecodeNumber(Inp,&BlockTables.LDD);
                        var LowDist = this.DecodeNumber(LDD);
                        Distance += LowDist;
                    }
                    else
                    {
                        Distance += Inp.getbits() >> (32 - DBits);
                        Inp.AddBits(DBits);
                    }
                }

                if (Distance > 0x100)
                {
                    Length++;
                    if (Distance > 0x2000)
                    {
                        Length++;
                        if (Distance > 0x40000)
                        {
                            Length++;
                        }
                    }
                }

                InsertOldDist(Distance);
                LastLength = Length;
                //              if (Fragmented)
                //                FragWindow.CopyString(Length,Distance,UnpPtr,MaxWinMask);
                //              else
                CopyString(Length, Distance);
                continue;
            }
            if (MainSlot == 256)
            {
                var Filter = new UnpackFilter();
                if (!ReadFilter(Filter) || !AddFilter(Filter))
                {
                    break;
                }

                continue;
            }
            if (MainSlot == 257)
            {
                if (LastLength != 0)
                //                if (Fragmented)
                //                  FragWindow.CopyString(LastLength,OldDist[0],UnpPtr,MaxWinMask);
                //                else
                //CopyString(LastLength,OldDist[0]);
                {
                    CopyString(LastLength, OldDistN(0));
                }

                continue;
            }
            if (MainSlot < 262)
            {
                //uint DistNum=MainSlot-258;
                var DistNum = (int)(MainSlot - 258);
                //uint Distance=OldDist[DistNum];
                var Distance = OldDistN(DistNum);
                //for (uint I=DistNum;I>0;I--)
                for (var I = DistNum; I > 0; I--)
                //OldDistN[I]=OldDistN(I-1);
                {
                    SetOldDistN(I, OldDistN(I - 1));
                }

                //OldDistN[0]=Distance;
                SetOldDistN(0, Distance);

                var LengthSlot = this.DecodeNumber(RD);
                var Length = SlotToLength(LengthSlot);
                LastLength = Length;
                //              if (Fragmented)
                //                FragWindow.CopyString(Length,Distance,UnpPtr,MaxWinMask);
                //              else
                CopyString(Length, Distance);
                continue;
            }
        }
        UnpWriteBuf();
    }

    private uint ReadFilterData()
    {
        var ByteCount = (Inp.fgetbits() >> 14) + 1;
        Inp.AddBits(2);

        uint Data = 0;
        //for (uint I=0;I<ByteCount;I++)
        for (var I = 0; I < ByteCount; I++)
        {
            Data += (Inp.fgetbits() >> 8) << (I * 8);
            Inp.AddBits(8);
        }
        return Data;
    }

    private bool ReadFilter(UnpackFilter Filter)
    {
        if (!Inp.ExternalBuffer && Inp.InAddr > ReadTop - 16)
        {
            if (!UnpReadBuf())
            {
                return false;
            }
        }

        Filter.BlockStart = (int)ReadFilterData();
        Filter.BlockLength = (int)ReadFilterData();
        if (Filter.BlockLength > MAX_FILTER_BLOCK_SIZE)
        {
            Filter.BlockLength = 0;
        }

        //Filter.Type=Inp.fgetbits()>>13;
        Filter.Type = (byte)(Inp.fgetbits() >> 13);
        Inp.faddbits(3);

        if (Filter.Type == (byte)FilterType.FILTER_DELTA)
        {
            //Filter.Channels=(Inp.fgetbits()>>11)+1;
            Filter.Channels = (byte)((Inp.fgetbits() >> 11) + 1);
            Inp.faddbits(5);
        }

        return true;
    }

    private bool AddFilter(UnpackFilter Filter)
    {
        if (Filters.Count >= MAX_UNPACK_FILTERS)
        {
            UnpWriteBuf(); // Write data, apply and flush filters.
            if (Filters.Count >= MAX_UNPACK_FILTERS)
            {
                InitFilters(); // Still too many filters, prevent excessive memory use.
            }
        }

        // If distance to filter start is that large that due to circular dictionary
        // mode now it points to old not written yet data, then we set 'NextWindow'
        // flag and process this filter only after processing that older data.
        Filter.NextWindow = WrPtr != UnpPtr && ((WrPtr - UnpPtr) & MaxWinMask) <= Filter.BlockStart;

        Filter.BlockStart = ((Filter.BlockStart + UnpPtr) & MaxWinMask);
        Filters.Add(Filter);
        return true;
    }

    private bool UnpReadBuf()
    {
        var DataSize = ReadTop - Inp.InAddr; // Data left to process.
        if (DataSize < 0)
        {
            return false;
        }

        BlockHeader.BlockSize -= Inp.InAddr - BlockHeader.BlockStart;
        if (Inp.InAddr > MAX_SIZE / 2)
        {
            // If we already processed more than half of buffer, let's move
            // remaining data into beginning to free more space for new data
            // and ensure that calling function does not cross the buffer border
            // even if we did not read anything here. Also it ensures that read size
            // is not less than CRYPT_BLOCK_SIZE, so we can align it without risk
            // to make it zero.
            if (DataSize > 0)
            //memmove(Inp.InBuf,Inp.InBuf+Inp.InAddr,DataSize);
            {
                Array.Copy(InBuf, inAddr, InBuf, 0, DataSize);
            }

            // TODO: perf
            //Buffer.BlockCopy(InBuf, inAddr, InBuf, 0, DataSize);

            Inp.InAddr = 0;
            ReadTop = DataSize;
        }
        else
        {
            DataSize = ReadTop;
        }

        var ReadCode = 0;
        if (MAX_SIZE != DataSize)
        //ReadCode=UnpIO->UnpRead(Inp.InBuf+DataSize,BitInput.MAX_SIZE-DataSize);
        {
            ReadCode = readStream.Read(InBuf, DataSize, MAX_SIZE - DataSize);
        }

        if (ReadCode > 0) // Can be also -1.
        {
            ReadTop += ReadCode;
        }

        ReadBorder = ReadTop - 30;
        BlockHeader.BlockStart = Inp.InAddr;
        if (BlockHeader.BlockSize != -1) // '-1' means not defined yet.
        {
            // We may need to quit from main extraction loop and read new block header
            // and trees earlier than data in input buffer ends.
            ReadBorder = Math.Min(ReadBorder, BlockHeader.BlockStart + BlockHeader.BlockSize - 1);
        }
        return ReadCode != -1;
    }

    //?
    //        void UnpWriteBuf()
    //        {
    //          size_t WrittenBorder=WrPtr;
    //          size_t FullWriteSize=(UnpPtr-WrittenBorder)&MaxWinMask;
    //          size_t WriteSizeLeft=FullWriteSize;
    //          bool NotAllFiltersProcessed=false;
    //          for (size_t I=0;I<Filters.Size();I++)
    //          {
    //            // Here we apply filters to data which we need to write.
    //            // We always copy data to another memory block before processing.
    //            // We cannot process them just in place in Window buffer, because
    //            // these data can be used for future string matches, so we must
    //            // preserve them in original form.
    //
    //            UnpackFilter *flt=&Filters[I];
    //            if (flt->Type==FilterType.FILTER_NONE)
    //              continue;
    //            if (flt->NextWindow)
    //            {
    //              // Here we skip filters which have block start in current data range
    //              // due to address wrap around in circular dictionary, but actually
    //              // belong to next dictionary block. If such filter start position
    //              // is included to current write range, then we reset 'NextWindow' flag.
    //              // In fact we can reset it even without such check, because current
    //              // implementation seems to guarantee 'NextWindow' flag reset after
    //              // buffer writing for all existing filters. But let's keep this check
    //              // just in case. Compressor guarantees that distance between
    //              // filter block start and filter storing position cannot exceed
    //              // the dictionary size. So if we covered the filter block start with
    //              // our write here, we can safely assume that filter is applicable
    //              // to next block on no further wrap arounds is possible.
    //              if (((flt->BlockStart-WrPtr)&MaxWinMask)<=FullWriteSize)
    //                flt->NextWindow=false;
    //              continue;
    //            }
    //            uint BlockStart=flt->BlockStart;
    //            uint BlockLength=flt->BlockLength;
    //            if (((BlockStart-WrittenBorder)&MaxWinMask)<WriteSizeLeft)
    //            {
    //              if (WrittenBorder!=BlockStart)
    //              {
    //                UnpWriteArea(WrittenBorder,BlockStart);
    //                WrittenBorder=BlockStart;
    //                WriteSizeLeft=(UnpPtr-WrittenBorder)&MaxWinMask;
    //              }
    //              if (BlockLength<=WriteSizeLeft)
    //              {
    //                if (BlockLength>0) // We set it to 0 also for invalid filters.
    //                {
    //                  uint BlockEnd=(BlockStart+BlockLength)&MaxWinMask;
    //
    //                  FilterSrcMemory.Alloc(BlockLength);
    //                  byte *Mem=&FilterSrcMemory[0];
    //                  if (BlockStart<BlockEnd || BlockEnd==0)
    //                  {
    //                    if (Fragmented)
    //                      FragWindow.CopyData(Mem,BlockStart,BlockLength);
    //                    else
    //                      memcpy(Mem,Window+BlockStart,BlockLength);
    //                  }
    //                  else
    //                  {
    //                    size_t FirstPartLength=size_t(MaxWinSize-BlockStart);
    //                    if (Fragmented)
    //                    {
    //                      FragWindow.CopyData(Mem,BlockStart,FirstPartLength);
    //                      FragWindow.CopyData(Mem+FirstPartLength,0,BlockEnd);
    //                    }
    //                    else
    //                    {
    //                      memcpy(Mem,Window+BlockStart,FirstPartLength);
    //                      memcpy(Mem+FirstPartLength,Window,BlockEnd);
    //                    }
    //                  }
    //
    //                  byte *OutMem=ApplyFilter(Mem,BlockLength,flt);
    //
    //                  Filters[I].Type=FilterType.FILTER_NONE;
    //
    //                  if (OutMem!=NULL)
    //                    UnpIO->UnpWrite(OutMem,BlockLength);
    //
    //                  UnpSomeRead=true;
    //                  WrittenFileSize+=BlockLength;
    //                  WrittenBorder=BlockEnd;
    //                  WriteSizeLeft=(UnpPtr-WrittenBorder)&MaxWinMask;
    //                }
    //              }
    //              else
    //              {
    //                // Current filter intersects the window write border, so we adjust
    //                // the window border to process this filter next time, not now.
    //                WrPtr=WrittenBorder;
    //
    //                // Since Filter start position can only increase, we quit processing
    //                // all following filters for this data block and reset 'NextWindow'
    //                // flag for them.
    //                for (size_t J=I;J<Filters.Size();J++)
    //                {
    //                  UnpackFilter *flt=&Filters[J];
    //                  if (flt->Type!=FilterType.FILTER_NONE)
    //                    flt->NextWindow=false;
    //                }
    //
    //                // Do not write data left after current filter now.
    //                NotAllFiltersProcessed=true;
    //                break;
    //              }
    //            }
    //          }
    //
    //          // Remove processed filters from queue.
    //          size_t EmptyCount=0;
    //          for (size_t I=0;I<Filters.Size();I++)
    //          {
    //            if (EmptyCount>0)
    //              Filters[I-EmptyCount]=Filters[I];
    //            if (Filters[I].Type==FilterType.FILTER_NONE)
    //              EmptyCount++;
    //          }
    //          if (EmptyCount>0)
    //            Filters.Alloc(Filters.Size()-EmptyCount);
    //
    //          if (!NotAllFiltersProcessed) // Only if all filters are processed.
    //          {
    //            // Write data left after last filter.
    //            UnpWriteArea(WrittenBorder,UnpPtr);
    //            WrPtr=UnpPtr;
    //          }
    //
    //          // We prefer to write data in blocks not exceeding UNPACK_MAX_WRITE
    //          // instead of potentially huge MaxWinSize blocks. It also allows us
    //          // to keep the size of Filters array reasonable.
    //          WriteBorder=(UnpPtr+Min(MaxWinSize,UNPACK_MAX_WRITE))&MaxWinMask;
    //
    //          // Choose the nearest among WriteBorder and WrPtr actual written border.
    //          // If border is equal to UnpPtr, it means that we have MaxWinSize data ahead.
    //          if (WriteBorder==UnpPtr ||
    //              WrPtr!=UnpPtr && ((WrPtr-UnpPtr)&MaxWinMask)<((WriteBorder-UnpPtr)&MaxWinMask))
    //            WriteBorder=WrPtr;
    //        }


    // unused
    //x byte* ApplyFilter(byte *Data,uint DataSize,UnpackFilter *Flt)
    //        byte[] ApplyFilter(byte []Data, uint DataSize, UnpackFilter Flt)
    //        {
    //          //x byte *SrcData=Data;
    //          byte []SrcData=Data;
    //          switch(Flt.Type)
    //          {
    //            case (byte)FilterType.FILTER_E8:
    //            case (byte)FilterType.FILTER_E8E9:
    //              {
    //                uint FileOffset=(uint)WrittenFileSize;
    //
    //                const uint FileSize=0x1000000;
    //                byte CmpByte2=Flt.Type==(byte)FilterType.FILTER_E8E9 ? (byte)0xe9 : (byte)0xe8;
    //                // DataSize is unsigned, so we use "CurPos+4" and not "DataSize-4"
    //                // to avoid overflow for DataSize<4.
    //                for (uint CurPos=0;CurPos+4<DataSize;)
    //                {
    //                  byte CurByte=*(Data++);
    //                  CurPos++;
    //                  if (CurByte==0xe8 || CurByte==CmpByte2)
    //                  {
    //                    uint Offset=(CurPos+FileOffset)%FileSize;
    //                    uint Addr=RawGet4(Data);
    //
    //                    // We check 0x80000000 bit instead of '< 0' comparison
    //                    // not assuming int32 presence or uint size and endianness.
    //                    if ((Addr & 0x80000000)!=0)              // Addr<0
    //                    {
    //                      if (((Addr+Offset) & 0x80000000)==0)   // Addr+Offset>=0
    //                        RawPut4(Addr+FileSize,Data);
    //                    }
    //                    else
    //                      if (((Addr-FileSize) & 0x80000000)!=0) // Addr<FileSize
    //                        RawPut4(Addr-Offset,Data);
    //
    //                    Data+=4;
    //                    CurPos+=4;
    //                  }
    //                }
    //              }
    //              return SrcData;
    //            case (byte)FilterType.FILTER_ARM:
    //              {
    //                uint FileOffset=(uint)WrittenFileSize;
    //                // DataSize is unsigned, so we use "CurPos+3" and not "DataSize-3"
    //                // to avoid overflow for DataSize<3.
    //                for (uint CurPos=0;CurPos+3<DataSize;CurPos+=4)
    //                {
    //                  byte *D=Data+CurPos;
    //                  if (D[3]==0xeb) // BL command with '1110' (Always) condition.
    //                  {
    //                    uint Offset=D[0]+uint(D[1])*0x100+uint(D[2])*0x10000;
    //                    Offset-=(FileOffset+CurPos)/4;
    //                    D[0]=(byte)Offset;
    //                    D[1]=(byte)(Offset>>8);
    //                    D[2]=(byte)(Offset>>16);
    //                  }
    //                }
    //              }
    //              return SrcData;
    //            case (byte)FilterType.FILTER_DELTA:
    //              {
    //                // Unlike RAR3, we do not need to reject excessive channel
    //                // values here, since RAR5 uses only 5 bits to store channel.
    //                uint Channels=Flt->Channels,SrcPos=0;
    //
    //                FilterDstMemory.Alloc(DataSize);
    //                byte *DstData=&FilterDstMemory[0];
    //
    //                // Bytes from same channels are grouped to continual data blocks,
    //                // so we need to place them back to their interleaving positions.
    //                for (uint CurChannel=0;CurChannel<Channels;CurChannel++)
    //                {
    //                  byte PrevByte=0;
    //                  for (uint DestPos=CurChannel;DestPos<DataSize;DestPos+=Channels)
    //                    DstData[DestPos]=(PrevByte-=Data[SrcPos++]);
    //                }
    //                return DstData;
    //              }
    //
    //          }
    //          return null;
    //        }

    // unused
    //        void UnpWriteArea(size_t StartPtr,size_t EndPtr)
    //        {
    //          if (EndPtr!=StartPtr)
    //            UnpSomeRead=true;
    //          if (EndPtr<StartPtr)
    //            UnpAllBuf=true;
    //
    ////          if (Fragmented)
    ////          {
    ////            size_t SizeToWrite=(EndPtr-StartPtr) & MaxWinMask;
    ////            while (SizeToWrite>0)
    ////            {
    ////              size_t BlockSize=FragWindow.GetBlockSize(StartPtr,SizeToWrite);
    ////              UnpWriteData(&FragWindow[StartPtr],BlockSize);
    ////              SizeToWrite-=BlockSize;
    ////              StartPtr=(StartPtr+BlockSize) & MaxWinMask;
    ////            }
    ////          }
    ////          else
    //            if (EndPtr<StartPtr)
    //            {
    //              UnpWriteData(Window+StartPtr,MaxWinSize-StartPtr);
    //              UnpWriteData(Window,EndPtr);
    //            }
    //            else
    //              UnpWriteData(Window+StartPtr,EndPtr-StartPtr);
    //        }

    // unused
    //        void UnpWriteData(byte *Data,size_t Size)
    //        {
    //          if (WrittenFileSize>=DestUnpSize)
    //            return;
    //          size_t WriteSize=Size;
    //          long LeftToWrite=DestUnpSize-WrittenFileSize;
    //          if ((long)WriteSize>LeftToWrite)
    //            WriteSize=(size_t)LeftToWrite;
    //          UnpIO->UnpWrite(Data,WriteSize);
    //          WrittenFileSize+=Size;
    //        }

    private bool ReadBlockHeader()
    {
        if (!Inp.ExternalBuffer && Inp.InAddr > ReadTop - 7)
        {
            if (!UnpReadBuf())
            {
                return false;
            }
        }

        //Inp.faddbits((8-Inp.InBit)&7);
        Inp.faddbits((uint)((8 - Inp.InBit) & 7));

        var BlockFlags = (byte)(Inp.fgetbits() >> 8);
        Inp.faddbits(8);
        //uint ByteCount=((BlockFlags>>3)&3)+1; // Block size byte count.
        var ByteCount = (uint)(((BlockFlags >> 3) & 3) + 1); // Block size byte count.

        if (ByteCount == 4)
        {
            return false;
        }

        //Header.HeaderSize=2+ByteCount;

        Header.BlockBitSize = (BlockFlags & 7) + 1;

        var SavedCheckSum = (byte)(Inp.fgetbits() >> 8);
        Inp.faddbits(8);

        var BlockSize = 0;
        //for (uint I=0;I<ByteCount;I++)
        for (var I = 0; I < ByteCount; I++)
        {
            //BlockSize+=(Inp.fgetbits()>>8)<<(I*8);
            BlockSize += (int)(Inp.fgetbits() >> 8) << (I * 8);
            Inp.AddBits(8);
        }

        Header.BlockSize = BlockSize;
        var CheckSum = (byte)(0x5a ^ BlockFlags ^ BlockSize ^ (BlockSize >> 8) ^ (BlockSize >> 16));
        if (CheckSum != SavedCheckSum)
        {
            return false;
        }

        Header.BlockStart = Inp.InAddr;
        ReadBorder = Math.Min(ReadBorder, Header.BlockStart + Header.BlockSize - 1);

        Header.LastBlockInFile = (BlockFlags & 0x40) != 0;
        return true;
    }

    //?
    //        bool ReadTables(BitInput Inp, ref UnpackBlockHeader Header, ref UnpackBlockTables Tables)
    //        {
    //          if (!Header.TablePresent)
    //            return true;
    //
    //          if (!Inp.ExternalBuffer && Inp.InAddr>ReadTop-25)
    //            if (!UnpReadBuf())
    //              return false;
    //
    //          byte BitLength[BC];
    //          for (uint I=0;I<BC;I++)
    //          {
    //            uint Length=(byte)(Inp.fgetbits() >> 12);
    //            Inp.faddbits(4);
    //            if (Length==15)
    //            {
    //              uint ZeroCount=(byte)(Inp.fgetbits() >> 12);
    //              Inp.faddbits(4);
    //              if (ZeroCount==0)
    //                BitLength[I]=15;
    //              else
    //              {
    //                ZeroCount+=2;
    //                while (ZeroCount-- > 0 && I<ASIZE(BitLength))
    //                  BitLength[I++]=0;
    //                I--;
    //              }
    //            }
    //            else
    //              BitLength[I]=Length;
    //          }
    //
    //          MakeDecodeTables(BitLength,&Tables.BD,BC);
    //
    //          byte Table[HUFF_TABLE_SIZE];
    //          const uint TableSize=HUFF_TABLE_SIZE;
    //          for (uint I=0;I<TableSize;)
    //          {
    //            if (!Inp.ExternalBuffer && Inp.InAddr>ReadTop-5)
    //              if (!UnpReadBuf())
    //                return false;
    //            uint Number=DecodeNumber(Inp,&Tables.BD);
    //            if (Number<16)
    //            {
    //              Table[I]=Number;
    //              I++;
    //            }
    //            else
    //              if (Number<18)
    //              {
    //                uint N;
    //                if (Number==16)
    //                {
    //                  N=(Inp.fgetbits() >> 13)+3;
    //                  Inp.faddbits(3);
    //                }
    //                else
    //                {
    //                  N=(Inp.fgetbits() >> 9)+11;
    //                  Inp.faddbits(7);
    //                }
    //                if (I==0)
    //                {
    //                  // We cannot have "repeat previous" code at the first position.
    //                  // Multiple such codes would shift Inp position without changing I,
    //                  // which can lead to reading beyond of Inp boundary in mutithreading
    //                  // mode, where Inp.ExternalBuffer disables bounds check and we just
    //                  // reserve a lot of buffer space to not need such check normally.
    //                  return false;
    //                }
    //                else
    //                  while (N-- > 0 && I<TableSize)
    //                  {
    //                    Table[I]=Table[I-1];
    //                    I++;
    //                  }
    //              }
    //              else
    //              {
    //                uint N;
    //                if (Number==18)
    //                {
    //                  N=(Inp.fgetbits() >> 13)+3;
    //                  Inp.faddbits(3);
    //                }
    //                else
    //                {
    //                  N=(Inp.fgetbits() >> 9)+11;
    //                  Inp.faddbits(7);
    //                }
    //                while (N-- > 0 && I<TableSize)
    //                  Table[I++]=0;
    //              }
    //          }
    //          TablesRead5=true;
    //          if (!Inp.ExternalBuffer && Inp.InAddr>ReadTop)
    //            return false;
    //          MakeDecodeTables(&Table[0],&Tables.LD,NC);
    //          MakeDecodeTables(&Table[NC],&Tables.DD,DC);
    //          MakeDecodeTables(&Table[NC+DC],&Tables.LDD,LDC);
    //          MakeDecodeTables(&Table[NC+DC+LDC],&Tables.RD,RC);
    //          return true;
    //        }

    //?
    //        void InitFilters()
    //        {
    //          Filters.SoftReset();
    //        }
}
#endif
