using System;
using System.IO;
using System.Reflection;

namespace AL_Common;

public static partial class Common
{
    private static bool? _fileStreamBufferFieldFound;
    private static FieldInfo? _fileStreamStrategyFieldInfo;
    private static Type? _fileStreamBufferedStrategyType;
    private static FieldInfo? _bufferField;

    #region Methods

    /*
    @NET5(FileStream cached buffer):
    We currently pass a buffer all around a million times. We would like to have this just use a rented array.
    However, it's very tricky to do so:
    -We can't inherit from FileStream because then the strategy ends up being DerivedFileStreamStrategy with the
     original strategy as a field. Then we have to get two instances and all the frigging reflection crap just to
     set the value. We can't cache instances of course because there'll be new ones for every new FileStream.
    -We can't use a wrapper struct because many of the callers need a Stream-derived type.
    -We could do a Stream-derived type and have the buffer-cached FileStream as a member and override everything
     and redirect to the member FileStream. Maybe this would prevent the JIT from devirtualizing. Would it matter?
    -We could use a wrapper struct and make the callsites messier.
    */
    public static FileStream GetReadModeFileStreamWithCachedBuffer(string path, byte[] buffer)
    {
        FileStream fs = new(path, FileMode.Open, FileAccess.Read, FileShare.Read, 4096);

        SetBuffer(fs, buffer);

        return fs;
    }

    public static FileStream GetWriteModeFileStreamWithCachedBuffer(string path, bool overwrite, byte[] buffer)
    {
        FileMode mode = overwrite ? FileMode.Create : FileMode.CreateNew;

        FileStream fs = new(path, mode, FileAccess.Write, FileShare.Read, 4096);

        SetBuffer(fs, buffer);

        return fs;
    }

    static Common()
    {
        try
        {
            _fileStreamStrategyFieldInfo = typeof(FileStream)
                .GetField(
                    "_strategy",
                    BindingFlags.NonPublic | BindingFlags.Instance);

            if (_fileStreamStrategyFieldInfo != null)
            {
                using FileStream fs = File.Create(
                    Path.Combine(Path.GetTempPath(), Path.GetRandomFileName()),
                    1,
                    FileOptions.DeleteOnClose);
                object? strategyInstance = _fileStreamStrategyFieldInfo.GetValue(fs);
                if (strategyInstance != null)
                {
                    _fileStreamBufferedStrategyType = strategyInstance.GetType();
                    _bufferField = _fileStreamBufferedStrategyType
                        .GetField(
                            "_buffer",
                            BindingFlags.NonPublic | BindingFlags.Instance);
                    _fileStreamBufferFieldFound = _bufferField != null;
                }
            }
        }
        catch
        {
            _fileStreamBufferFieldFound = false;
            _fileStreamStrategyFieldInfo = null;
            _fileStreamBufferedStrategyType = null;
            _bufferField = null;
        }
    }

    private static void SetBuffer(FileStream fs, byte[] buffer)
    {
        if (_fileStreamBufferFieldFound == true)
        {
            object? strategyInstance = _fileStreamStrategyFieldInfo!.GetValue(fs);
            _bufferField?.SetValue(strategyInstance, buffer);
        }
    }

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

    public static int ReadAll(this Stream stream, Span<byte> buffer)
    {
        return stream.ReadAtLeast(buffer, buffer.Length, throwOnEndOfStream: false);
    }

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
