using System.IO;
using System.Reflection;

namespace AL_Common;

public static partial class Common
{
    #region Classes

    /// <summary>
    /// A read-mode file stream with performance/allocation improvements.
    /// </summary>
    public sealed class FileStreamReadFast : FileStream
    {
        private static bool _fieldStreamBufferFieldFound;
        private static FieldInfo? _fieldStreamBufferFieldInfo;

        private long _length = -1;
        public override long Length
        {
            get
            {
                if (_length == -1)
                {
                    _length = base.Length;
                }
                return _length;
            }
        }

        // Init reflection stuff in static ctor for thread safety
        static FileStreamReadFast()
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

                _fieldStreamBufferFieldFound =
                    _fieldStreamBufferFieldInfo != null &&
                    _fieldStreamBufferFieldInfo.FieldType == typeof(byte[]);
            }
            catch
            {
                _fieldStreamBufferFieldFound = false;
                _fieldStreamBufferFieldInfo = null;
            }
        }

        public FileStreamReadFast(string path,
            FileShare share,
            int bufferSize)
            : base(path, FileMode.Open, FileAccess.Read, share, bufferSize)
        {
        }

        public FileStreamReadFast(string path,
            FileShare share)
            : base(path, FileMode.Open, FileAccess.Read, share)
        {
        }

        public static FileStreamReadFast Create(string path, byte[] buffer)
        {
            FileStreamReadFast fs =
                _fieldStreamBufferFieldFound
                    ? new FileStreamReadFast(path, FileShare.Read, buffer.Length)
                    : new FileStreamReadFast(path, FileShare.Read);

            if (_fieldStreamBufferFieldFound)
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
