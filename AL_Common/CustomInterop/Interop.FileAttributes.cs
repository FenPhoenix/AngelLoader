//#define ENABLE_UNUSED

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace AL_Common;

internal static partial class Interop
{
    internal static partial class Kernel32
    {
        internal static class FileAttributes
        {
#if ENABLE_UNUSED
            internal const int FILE_ATTRIBUTE_NORMAL = 0x00000080;
            internal const int FILE_ATTRIBUTE_READONLY = 0x00000001;
#endif
            internal const int FILE_ATTRIBUTE_DIRECTORY = 0x00000010;
#if ENABLE_UNUSED
            internal const int FILE_ATTRIBUTE_REPARSE_POINT = 0x00000400;
#endif
        }
    }
}
