using System.IO;
using System.Reflection;

namespace AL_Common;

public static partial class Common
{
    #region Classes

    /// <summary>
    /// A file stream with performance/allocation improvements.
    /// </summary>
    public sealed class FileStreamFast : FileStream
    {
        private static bool _fieldStreamBufferFieldFound;
        private static FieldInfo? _fieldStreamBufferFieldInfo;

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
                _fieldStreamBufferFieldFound
                    ? new FileStreamFast(path, FileMode.Open, FileAccess.Read, FileShare.Read, writeMode: false, buffer.Length)
                    : new FileStreamFast(path, FileMode.Open, FileAccess.Read, FileShare.Read, writeMode: false);

            SetBuffer(fs, buffer);

            return fs;
        }

        public static FileStreamFast CreateWrite(string path, bool overwrite, byte[] buffer)
        {
            FileMode mode = overwrite ? FileMode.Create : FileMode.CreateNew;

            FileStreamFast fs =
                _fieldStreamBufferFieldFound
                    ? new FileStreamFast(path, mode, FileAccess.Write, FileShare.Read, writeMode: true, buffer.Length)
                    : new FileStreamFast(path, mode, FileAccess.Write, FileShare.Read, writeMode: true);

            SetBuffer(fs, buffer);

            return fs;
        }

        private static void SetBuffer(FileStreamFast fs, byte[] buffer)
        {
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
