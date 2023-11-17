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

    public static FileStream GetReadModeFileStreamWithCachedBuffer(string path, byte[] buffer)
    {
        FileStream fs = new(path, FileMode.Open, FileAccess.Read, FileShare.Read, 4096);

        if (_fileStreamBufferFieldFound == true)
        {
            object? strategyInstance = _fileStreamStrategyFieldInfo!.GetValue(fs);
            _bufferField?.SetValue(strategyInstance, buffer);
        }
        else if (_fileStreamBufferFieldFound == null)
        {
            try
            {
                _fileStreamStrategyFieldInfo = typeof(FileStream)
                    .GetField(
                        "_strategy",
                        BindingFlags.NonPublic | BindingFlags.Instance);

                if (_fileStreamStrategyFieldInfo != null)
                {
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

        return fs;
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

    public static void StreamCopyNoAlloc(Stream source, Stream destination, byte[] buffer)
    {
        int count;
        while ((count = source.Read(buffer, 0, buffer.Length)) != 0)
        {
            destination.Write(buffer, 0, count);
        }
    }

    #endregion
}
