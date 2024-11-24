//#define ENABLE_UNUSED

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
#if ENABLE_UNUSED
            internal const uint IO_REPARSE_TAG_SYMLINK = 0xA000000C;
#endif
        }
    }
}
