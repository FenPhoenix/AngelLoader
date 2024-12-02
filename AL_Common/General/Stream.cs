using System;
using System.Buffers;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using AL_Common.NETM_IO;

namespace AL_Common;

public static partial class Common
{
    #region Classes

    /// <summary>
    /// A file stream with performance/allocation improvements.
    /// </summary>
    public sealed class FileStreamFast : FileStream
    {
        private static bool _fileStreamBufferFieldFound;
        private static FieldInfo? _fileStreamBufferFieldInfo;

        private readonly bool _writeMode;
        private long _length = -1;
        public override long Length
        {
            get
            {
                if (_writeMode)
                {
                    return base.Length;
                }
                else
                {
                    if (_length == -1)
                    {
                        _length = base.Length;
                    }
                    return _length;
                }
            }
        }

        // Init reflection stuff in static ctor for thread safety
        static FileStreamFast()
        {
            try
            {
                // @NET5(FileStream buffering): Newer .NETs (since the FileStream "strategy" additions) are totally different
                // We'd have to see if they added a way to pass in a buffer, and if not, we'd have to write totally
                // different code to get at the buffer here for newer .NETs.
                // typeof(FileStream) (base type) because that's the type where the buffer field is
                _fileStreamBufferFieldInfo = typeof(FileStream)
                    .GetField(
                        "_buffer",
                        BindingFlags.NonPublic | BindingFlags.Instance);

                _fileStreamBufferFieldFound =
                    _fileStreamBufferFieldInfo != null &&
                    _fileStreamBufferFieldInfo.FieldType == typeof(byte[]);
            }
            catch
            {
                _fileStreamBufferFieldFound = false;
                _fileStreamBufferFieldInfo = null;
            }
        }

        public FileStreamFast(
            string path,
            FileMode mode,
            FileAccess access,
            FileShare share,
            bool writeMode,
            int bufferSize)
            : base(path, mode, access, share, bufferSize)
        {
            _writeMode = writeMode;
        }

        public FileStreamFast(
            string path,
            FileMode mode,
            FileAccess access,
            FileShare share,
            bool writeMode)
            : base(path, mode, access, share)
        {
            _writeMode = writeMode;
        }

        public static FileStreamFast CreateRead(string path, byte[] buffer)
        {
            FileStreamFast fs =
                _fileStreamBufferFieldFound
                    ? new FileStreamFast(path, FileMode.Open, FileAccess.Read, FileShare.Read, writeMode: false, buffer.Length)
                    : new FileStreamFast(path, FileMode.Open, FileAccess.Read, FileShare.Read, writeMode: false);

            SetBuffer(fs, buffer);

            return fs;
        }

        public static FileStreamFast CreateWrite(string path, bool overwrite, byte[] buffer)
        {
            FileMode mode = overwrite ? FileMode.Create : FileMode.CreateNew;

            FileStreamFast fs =
                _fileStreamBufferFieldFound
                    ? new FileStreamFast(path, mode, FileAccess.Write, FileShare.Read, writeMode: true, buffer.Length)
                    : new FileStreamFast(path, mode, FileAccess.Write, FileShare.Read, writeMode: true);

            SetBuffer(fs, buffer);

            return fs;
        }

        private static void SetBuffer(FileStreamFast fs, byte[] buffer)
        {
            if (_fileStreamBufferFieldFound)
            {
                try
                {
                    _fileStreamBufferFieldInfo?.SetValue(fs, buffer);
                }
                catch
                {
                    _fileStreamBufferFieldFound = false;
                    _fileStreamBufferFieldInfo = null;
                }
            }
        }
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
        if (sfh.CanSeek && (fileLength = sfh.GetFileLength()) > 2146435071)
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
            int n = RandomAccess.ReadAtOffset(sfh, bytes.AsSpan(index, count), index);
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
                    if (newLength > 2146435071)
                    {
                        newLength = (uint)Math.Max(2146435071, buffer.Length + 1);
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
