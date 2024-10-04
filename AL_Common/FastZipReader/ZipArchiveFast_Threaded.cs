// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

// Zip Spec here: http://www.pkware.com/documents/casestudies/APPNOTE.TXT

using System.IO;
using static AL_Common.Common;
using static AL_Common.FastZipReader.ZipArchiveFast_Common;

namespace AL_Common.FastZipReader;

public static class ZipArchiveFast_Threaded
{
    private static Stream OpenEntry(
        ZipArchiveFastEntry entry,
        ZipContext_Threaded context)
    {
        if (!IsOpenable(entry, context.ArchiveStream, context.ArchiveStreamLength, context.BinaryReadBuffer, out string message))
        {
            ThrowHelper.InvalidData(message);
        }

        // _storedOffsetOfCompressedData will never be null, since we know IsOpenable is true

        context.ArchiveSubReadStream.Set((long)entry.StoredOffsetOfCompressedData!, entry.CompressedLength);

        return GetDataDecompressor(entry, context.ArchiveSubReadStream);
    }

    public static void ExtractToFile_Fast(
        ZipArchiveFastEntry entry,
        string fileName,
        bool overwrite,
        bool unSetReadOnly,
        ZipContext_Threaded context,
        byte[] fileStreamWriteBuffer)
    {
        using (FileStreamFast destination = FileStreamFast.CreateWrite(fileName, overwrite, fileStreamWriteBuffer))
        using (Stream source = OpenEntry(entry, context))
        {
            StreamCopyNoAlloc(source, destination, context.TempBuffer);
        }

        SetLastWriteTime_Fast(fileName, ZipHelpers.ZipTimeToDateTime(entry.LastWriteTime));

        if (unSetReadOnly)
        {
            File_UnSetReadOnly(fileName);
        }
    }
}
