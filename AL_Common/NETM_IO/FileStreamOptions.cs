// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using AL_Common.NETM_IO.Strategies;

namespace AL_Common.NETM_IO
{
    public sealed class FileStreamOptions
    {
        private FileMode _mode = FileMode.Open;
        private FileAccess _access = FileAccess.Read;
        private FileShare _share = FileStream_NET.DefaultShare;
        private FileOptions _options;
        private long _preallocationSize;
        private int _bufferSize = FileStream_NET.DefaultBufferSize;

        /// <summary>
        /// One of the enumeration values that determines how to open or create the file.
        /// </summary>
        /// <exception cref="T:System.ArgumentOutOfRangeException">When <paramref name="value" /> contains an invalid value.</exception>
        public FileMode Mode
        {
            get => _mode;
            set
            {
                if (value < FileMode.CreateNew || value > FileMode.Append)
                {
                    ThrowHelper.ArgumentOutOfRangeException_Enum_Value();
                }

                _mode = value;
            }
        }

        /// <summary>
        /// A bitwise combination of the enumeration values that determines how the file can be accessed by the <see cref="FileStream_NET" /> object. This also determines the values returned by the <see cref="FileStream_NET.CanRead" /> and <see cref="FileStream_NET.CanWrite" /> properties of the <see cref="FileStream_NET" /> object.
        /// </summary>
        /// <exception cref="T:System.ArgumentOutOfRangeException">When <paramref name="value" /> contains an invalid value.</exception>
        public FileAccess Access
        {
            get => _access;
            set
            {
                if (value < FileAccess.Read || value > FileAccess.ReadWrite)
                {
                    ThrowHelper.ArgumentOutOfRangeException_Enum_Value();
                }

                _access = value;
            }
        }

        /// <summary>
        /// A bitwise combination of the enumeration values that determines how the file will be shared by processes. The default value is <see cref="FileShare.Read" />.
        /// </summary>
        /// <exception cref="T:System.ArgumentOutOfRangeException">When <paramref name="value" /> contains an invalid value.</exception>
        public FileShare Share
        {
            get => _share;
            set
            {
                // don't include inheritable in our bounds check for share
                FileShare tempshare = value & ~FileShare.Inheritable;
                if (tempshare < FileShare.None || tempshare > (FileShare.ReadWrite | FileShare.Delete))
                {
                    ThrowHelper.ArgumentOutOfRangeException_Enum_Value();
                }

                _share = value;
            }
        }

        /// <summary>
        /// A bitwise combination of the enumeration values that specifies additional file options. The default value is <see cref="FileOptions.None" />, which indicates synchronous IO.
        /// </summary>
        /// <exception cref="T:System.ArgumentOutOfRangeException">When <paramref name="value" /> contains an invalid value.</exception>
        public FileOptions Options
        {
            get => _options;
            set
            {
                if (FileStreamHelpers.AreInvalid(value))
                {
                    ThrowHelper.ArgumentOutOfRangeException_Enum_Value();
                }

                _options = value;
            }
        }

        /// <summary>
        /// The initial allocation size in bytes for the file. A positive value is effective only when a regular file is being created, overwritten, or replaced.
        /// Negative values are not allowed.
        /// In other cases (including the default 0 value), it's ignored.
        /// </summary>
        /// <exception cref="T:System.ArgumentOutOfRangeException">When <paramref name="value" /> is negative.</exception>
        public long PreallocationSize
        {
            get => _preallocationSize;
            set
            {
                ArgumentOutOfRangeException.ThrowIfNegative(value);
                _preallocationSize = value;
            }
        }

        /// <summary>
        /// The size of the buffer used by <see cref="FileStream_NET" /> for buffering. The default buffer size is 4096.
        /// 0 or 1 means that buffering should be disabled. Negative values are not allowed.
        /// </summary>
        /// <exception cref="T:System.ArgumentOutOfRangeException">When <paramref name="value" /> is negative.</exception>
        public int BufferSize
        {
            get => _bufferSize;
            set
            {
                ArgumentOutOfRangeException.ThrowIfNegative(value);
                _bufferSize = value;
            }
        }
    }
}
