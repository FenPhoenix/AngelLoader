// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Diagnostics;
using System.IO;
using JetBrains.Annotations;

namespace FMScanner.FastZipReader
{
    public static class ZipSize
    {
        /// <summary>
        /// Gets the total uncompressed size of all entries in the archive. Disposes the passed stream automatically.
        /// </summary>
        /// <returns>The total uncompressed size of all entries in the archive.</returns>
        /// <exception cref="InvalidDataException"></exception>
        [PublicAPI]
        public static long GetTotalUncompressedSize(Stream stream, ZipReusableBundle bundle)
        {
            Stream? extraTempStream = null;
            Stream? backingStream = null;

            try
            {
                long centralDirectoryStart, expectedNumberOfEntries;

                try
                {
                    backingStream = null;

                    if (!stream.CanRead)
                    {
                        throw new ArgumentException(SR.ReadModeCapabilities);
                    }
                    if (!stream.CanSeek)
                    {
                        backingStream = stream;
                        extraTempStream = stream = new MemoryStream();
                        backingStream.CopyTo(stream);
                        stream.Seek(0, SeekOrigin.Begin);
                    }

                    bundle.ArchiveSubReadStream.SetSuperStream(stream);

                    (centralDirectoryStart, expectedNumberOfEntries) = ReadEndOfCentralDirectory(stream, bundle);
                }
                catch
                {
                    extraTempStream?.Dispose();
                    throw;
                }

                long totalSize = 0;
                // assume ReadEndOfCentralDirectory has been called and has populated _centralDirectoryStart

                stream.Seek(centralDirectoryStart, SeekOrigin.Begin);

                long numberOfEntries = 0;

                //read the central directory
                while (ZipCentralDirectoryFileHeader.TryReadBlock(stream, sizeOnly: true, bundle, out var currentHeader))
                {
                    totalSize += currentHeader.UncompressedSize;
                    numberOfEntries++;
                }

                if (numberOfEntries != expectedNumberOfEntries)
                {
                    throw new InvalidDataException(SR.NumEntriesWrong);
                }

                return totalSize;
            }
            catch (EndOfStreamException ex)
            {
                throw new InvalidDataException(SR.Format(SR.CentralDirectoryInvalid, ex));
            }
            finally
            {
                stream.Dispose();
                bundle.ArchiveSubReadStream.SetSuperStream(null);

                backingStream?.Dispose();
                extraTempStream?.Dispose();
            }
        }

        // This function reads all the EOCD stuff it needs to find the offset to the start of the central directory
        // This offset gets put in _centralDirectoryStart and the number of this disk gets put in _numberOfThisDisk
        // Also does some verification that this isn't a split/spanned archive
        // Also checks that offset to CD isn't out of bounds
        private static (long CentralDirectoryStart, long ExpectedNumberOfEntries)
        ReadEndOfCentralDirectory(Stream stream, ZipReusableBundle bundle)
        {
            long centralDirectoryStart, expectedNumberOfEntries;

            try
            {
                // this seeks to the start of the end of central directory record
                stream.Seek(-ZipEndOfCentralDirectoryBlock.SizeOfBlockWithoutSignature, SeekOrigin.End);
                if (!ZipHelpers.SeekBackwardsToSignature(stream, ZipEndOfCentralDirectoryBlock.SignatureConstant, bundle))
                {
                    throw new InvalidDataException(SR.EOCDNotFound);
                }

                long eocdStart = stream.Position;

                // read the EOCD
                bool eocdProper = ZipEndOfCentralDirectoryBlock.TryReadBlock(stream, bundle, out ZipEndOfCentralDirectoryBlock eocd);
                Debug.Assert(eocdProper); // we just found this using the signature finder, so it should be okay

                if (eocd.NumberOfThisDisk != eocd.NumberOfTheDiskWithTheStartOfTheCentralDirectory)
                {
                    throw new InvalidDataException(SR.SplitSpanned);
                }

                centralDirectoryStart = eocd.OffsetOfStartOfCentralDirectoryWithRespectToTheStartingDiskNumber;
                if (eocd.NumberOfEntriesInTheCentralDirectory != eocd.NumberOfEntriesInTheCentralDirectoryOnThisDisk)
                {
                    throw new InvalidDataException(SR.SplitSpanned);
                }
                expectedNumberOfEntries = eocd.NumberOfEntriesInTheCentralDirectory;

                // only bother looking for zip64 EOCD stuff if we suspect it is needed because some value is FFFFFFFFF
                // because these are the only two values we need, we only worry about these
                // if we don't find the zip64 EOCD, we just give up and try to use the original values
                if (eocd.NumberOfThisDisk == ZipHelpers.Mask16Bit ||
                    eocd.OffsetOfStartOfCentralDirectoryWithRespectToTheStartingDiskNumber == ZipHelpers.Mask32Bit ||
                    eocd.NumberOfEntriesInTheCentralDirectory == ZipHelpers.Mask16Bit)
                {
                    // we need to look for zip 64 EOCD stuff
                    // seek to the zip 64 EOCD locator
                    stream.Seek(eocdStart - Zip64EndOfCentralDirectoryLocator.SizeOfBlockWithoutSignature, SeekOrigin.Begin);
                    // if we don't find it, assume it doesn't exist and use data from normal eocd
                    if (ZipHelpers.SeekBackwardsToSignature(stream, Zip64EndOfCentralDirectoryLocator.SignatureConstant, bundle))
                    {
                        // use locator to get to Zip64EOCD
                        bool zip64EOCDLocatorProper = Zip64EndOfCentralDirectoryLocator.TryReadBlock(stream, bundle, out Zip64EndOfCentralDirectoryLocator locator);
                        Debug.Assert(zip64EOCDLocatorProper); // we just found this using the signature finder, so it should be okay

                        if (locator.OffsetOfZip64EOCD > long.MaxValue)
                        {
                            throw new InvalidDataException(SR.FieldTooBigOffsetToZip64EOCD);
                        }
                        long zip64EOCDOffset = (long)locator.OffsetOfZip64EOCD;

                        stream.Seek(zip64EOCDOffset, SeekOrigin.Begin);

                        // read Zip64EOCD
                        if (!Zip64EndOfCentralDirectoryRecord.TryReadBlock(stream, bundle, out Zip64EndOfCentralDirectoryRecord record))
                        {
                            throw new InvalidDataException(SR.Zip64EOCDNotWhereExpected);
                        }

                        if (record.NumberOfEntriesTotal > long.MaxValue)
                        {
                            throw new InvalidDataException(SR.FieldTooBigNumEntries);
                        }
                        if (record.OffsetOfCentralDirectory > long.MaxValue)
                        {
                            throw new InvalidDataException(SR.FieldTooBigOffsetToCD);
                        }
                        if (record.NumberOfEntriesTotal != record.NumberOfEntriesOnThisDisk)
                        {
                            throw new InvalidDataException(SR.SplitSpanned);
                        }

                        expectedNumberOfEntries = (long)record.NumberOfEntriesTotal;
                        centralDirectoryStart = (long)record.OffsetOfCentralDirectory;
                    }
                }

                if (centralDirectoryStart > stream.Length)
                {
                    throw new InvalidDataException(SR.FieldTooBigOffsetToCD);
                }
            }
            catch (EndOfStreamException ex)
            {
                throw new InvalidDataException(SR.CDCorrupt, ex);
            }
            catch (IOException ex)
            {
                throw new InvalidDataException(SR.CDCorrupt, ex);
            }

            return (centralDirectoryStart, expectedNumberOfEntries);
        }
    }
}
