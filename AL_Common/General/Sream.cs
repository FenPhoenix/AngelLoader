using System.IO;
using System.Reflection;

namespace AL_Common;

public static partial class Common
{
    private static bool? _fieldStreamBufferFieldFound;
    private static FieldInfo? _fieldStreamBufferFieldInfo;

    #region Methods

    public static FileStream GetReadModeFileStreamWithCachedBuffer(string path, byte[] buffer)
    {
        buffer.Clear();

        if (_fieldStreamBufferFieldFound == null)
        {
            try
            {
                // @NET5(FileStream buffering): Newer .NETs (since the FileStream "strategy" additions) are totally different
                // We'd have to see if they added a way to pass in a buffer, and if not, we'd have to write totally
                // different code to get at the buffer here for newer .NETs.
                // typeof(FileStream) (base type) because that's the type where the buffer field is
                _fieldStreamBufferFieldInfo = typeof(FileStream)
                    .GetField(
                        "_buffer",
                        BindingFlags.NonPublic | BindingFlags.Instance);

                _fieldStreamBufferFieldFound = _fieldStreamBufferFieldInfo != null &&
                                               _fieldStreamBufferFieldInfo.FieldType == typeof(byte[]);
            }
            catch
            {
                _fieldStreamBufferFieldFound = false;
                _fieldStreamBufferFieldInfo = null;
            }
        }

        var fs =
            _fieldStreamBufferFieldFound == true
                ? new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, buffer.Length)
                : new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);

        if (_fieldStreamBufferFieldFound == true)
        {
            try
            {
                _fieldStreamBufferFieldInfo?.SetValue(fs, buffer);
            }
            catch
            {
                _fieldStreamBufferFieldFound = false;
                _fieldStreamBufferFieldInfo = null;
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
