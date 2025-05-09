﻿using System;
using System.Buffers;
using System.Diagnostics;
using System.IO;

namespace AL_Common;

public static partial class Common
{
    #region Classes

    public static FileStream_NET GetReadModeFileStreamWithCachedBuffer(string path, byte[] buffer)
    {
        FileStream_NET fs = new(path, FileMode.Open, FileAccess.Read, FileShare.Read, buffer, buffer.Length);
        return fs;
    }

    public static FileStream_NET GetWriteModeFileStreamWithCachedBuffer(string path, bool overwrite, byte[] buffer)
    {
        FileMode mode = overwrite ? FileMode.Create : FileMode.CreateNew;
        FileStream_NET fs = new(path, mode, FileAccess.Write, FileShare.Read, buffer, buffer.Length);
        return fs;
    }

    #endregion

    #region Methods

    public static int ReadAll(this Stream stream, byte[] buffer, int offset, int count)
    {
        int bytesReadRet = 0;
        int startPosThisRound = offset;
        while (true)
        {
            int bytesRead = stream.Read(buffer, startPosThisRound, count);
            if (bytesRead <= 0) break;
            bytesReadRet += bytesRead;
            startPosThisRound += bytesRead;
            count -= bytesRead;
        }

        return bytesReadRet;
    }

    public static void StreamCopyNoAlloc(Stream source, Stream destination, byte[] buffer)
    {
        int count;
        while ((count = source.Read(buffer, 0, buffer.Length)) != 0)
        {
            destination.Write(buffer, 0, count);
        }
    }

    #region ReadAllBytes .NET Modern

    // Licensed to the .NET Foundation under one or more agreements.
    // The .NET Foundation licenses this file to you under the MIT license.

    public static byte[] File_ReadAllBytesFast(string path)
    {
        // Fen: We're on Framework which is Windows-only, so we don't need to do an OS check.
        const FileOptions options = FileOptions.SequentialScan;

        // SequentialScan is a perf hint that requires extra sys-call on non-Windows OSes.
        //FileOptions options = OperatingSystem.IsWindows() ? FileOptions.SequentialScan : FileOptions.None;

        using AL_SafeFileHandle sfh = AL_SafeFileHandle.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read, options);

        long fileLength = 0;
        if (sfh.CanSeek && (fileLength = sfh.GetFileLength()) > MaxArrayLength)
        {
            throw new IOException(SR.IO_FileTooLong2GB);
        }

        if (fileLength == 0)
        {
            // Some file systems (e.g. procfs on Linux) return 0 for length even when there's content; also there are non-seekable files.
            // Thus we need to assume 0 doesn't mean empty.
            return ReadAllBytesUnknownLength(sfh);
        }

        int index = 0;
        int count = (int)fileLength;
        byte[] bytes = new byte[count];
        while (count > 0)
        {
            int n = RandomAccess.ReadAtOffset_Fast(sfh, bytes, index, count, index);
            if (n == 0)
            {
                ThrowHelper.EndOfFile();
            }

            index += n;
            count -= n;
        }
        return bytes;
    }

    private static byte[] ReadAllBytesUnknownLength(AL_SafeFileHandle sfh)
    {
        byte[]? rentedArray = null;
        Span<byte> buffer = stackalloc byte[512];
        try
        {
            int bytesRead = 0;
            while (true)
            {
                if (bytesRead == buffer.Length)
                {
                    uint newLength = (uint)buffer.Length * 2;
                    if (newLength > MaxArrayLength)
                    {
                        newLength = (uint)Math.Max(MaxArrayLength, buffer.Length + 1);
                    }

                    byte[] tmp = ArrayPool<byte>.Shared.Rent((int)newLength);
                    buffer.CopyTo(tmp);
                    byte[]? oldRentedArray = rentedArray;
                    buffer = rentedArray = tmp;
                    if (oldRentedArray != null)
                    {
                        ArrayPool<byte>.Shared.Return(oldRentedArray);
                    }
                }

                Debug.Assert(bytesRead < buffer.Length);
                int n = RandomAccess.ReadAtOffset(sfh, buffer[bytesRead..], bytesRead);
                if (n == 0)
                {
                    return buffer[..bytesRead].ToArray();
                }
                bytesRead += n;
            }
        }
        finally
        {
            if (rentedArray != null)
            {
                ArrayPool<byte>.Shared.Return(rentedArray);
            }
        }
    }

    #endregion

    #region Chainable StreamWriter methods

    public static StreamWriter Append(this StreamWriter sw, string value)
    {
        sw.Write(value);
        return sw;
    }

    public static StreamWriter Append(this StreamWriter sw, char value)
    {
        sw.Write(value);
        return sw;
    }

    public static StreamWriter Append(this StreamWriter sw, int value)
    {
        sw.Write(value);
        return sw;
    }

    public static StreamWriter Append(this StreamWriter sw, uint value)
    {
        sw.Write(value);
        return sw;
    }

    public static StreamWriter Append(this StreamWriter sw, long value)
    {
        sw.Write(value);
        return sw;
    }

    public static StreamWriter Append(this StreamWriter sw, ulong value)
    {
        sw.Write(value);
        return sw;
    }

    public static StreamWriter Append(this StreamWriter sw, float value)
    {
        sw.Write(value);
        return sw;
    }

    public static StreamWriter Append(this StreamWriter sw, double value)
    {
        sw.Write(value);
        return sw;
    }

    public static StreamWriter Append(this StreamWriter sw, decimal value)
    {
        sw.Write(value);
        return sw;
    }

    public static StreamWriter Append(this StreamWriter sw, bool value)
    {
        sw.Write(value);
        return sw;
    }

    public static StreamWriter Append(this StreamWriter sw, object? value)
    {
        sw.Write(value);
        return sw;
    }

    public static StreamWriter AppendLine(this StreamWriter sw, string value)
    {
        sw.WriteLine(value);
        return sw;
    }

    public static StreamWriter AppendLine(this StreamWriter sw, char value)
    {
        sw.WriteLine(value);
        return sw;
    }

    public static StreamWriter AppendLine(this StreamWriter sw, int value)
    {
        sw.WriteLine(value);
        return sw;
    }

    public static StreamWriter AppendLine(this StreamWriter sw, uint value)
    {
        sw.WriteLine(value);
        return sw;
    }

    public static StreamWriter AppendLine(this StreamWriter sw, long value)
    {
        sw.WriteLine(value);
        return sw;
    }

    public static StreamWriter AppendLine(this StreamWriter sw, ulong value)
    {
        sw.WriteLine(value);
        return sw;
    }

    public static StreamWriter AppendLine(this StreamWriter sw, float value)
    {
        sw.WriteLine(value);
        return sw;
    }

    public static StreamWriter AppendLine(this StreamWriter sw, double value)
    {
        sw.WriteLine(value);
        return sw;
    }

    public static StreamWriter AppendLine(this StreamWriter sw, decimal value)
    {
        sw.WriteLine(value);
        return sw;
    }

    public static StreamWriter AppendLine(this StreamWriter sw, bool value)
    {
        sw.WriteLine(value);
        return sw;
    }

    public static StreamWriter AppendLine(this StreamWriter sw, object? value)
    {
        sw.WriteLine(value);
        return sw;
    }

    public static StreamWriter AppendLine(this StreamWriter sw)
    {
        sw.WriteLine();
        return sw;
    }

    #endregion

    #endregion
}
