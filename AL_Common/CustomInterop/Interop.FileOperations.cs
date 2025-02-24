﻿//#define ENABLE_UNUSED

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace AL_Common;

internal static partial class Interop
{
    internal static partial class Kernel32
    {
        internal static class IOReparseOptions
        {
#if ENABLE_UNUSED
            internal const uint IO_REPARSE_TAG_FILE_PLACEHOLDER = 0x80000015;
#endif
            internal const uint IO_REPARSE_TAG_MOUNT_POINT = 0xA0000003;
            internal const uint IO_REPARSE_TAG_SYMLINK = 0xA000000C;
        }

        internal static class FileOperations
        {
            internal const int OPEN_EXISTING = 3;
#if ENABLE_UNUSED
            internal const int COPY_FILE_FAIL_IF_EXISTS = 0x00000001;
#endif

            internal const int FILE_FLAG_BACKUP_SEMANTICS = 0x02000000;
#if ENABLE_UNUSED
            internal const int FILE_FLAG_FIRST_PIPE_INSTANCE = 0x00080000;
#endif
            internal const int FILE_FLAG_OPEN_REPARSE_POINT = 0x00200000;
#if ENABLE_UNUSED
            internal const int FILE_FLAG_OVERLAPPED = 0x40000000;

            internal const int FILE_LIST_DIRECTORY = 0x0001;

            internal const int FILE_WRITE_ATTRIBUTES = 0x100;
#endif
        }
    }
}
