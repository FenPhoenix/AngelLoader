﻿//#define ENABLE_UNUSED

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace AL_Common;

internal static partial class Interop
{
    internal static class StatusOptions
    {
#if ENABLE_UNUSED
        // See the NT_SUCCESS macro in the Windows SDK, and
        // https://learn.microsoft.com/windows-hardware/drivers/kernel/using-ntstatus-values
        internal static bool NT_SUCCESS(uint ntStatus) => (int)ntStatus >= 0;
#endif

        // Error codes from ntstatus.h
        internal const uint STATUS_SUCCESS = 0x00000000;
#if ENABLE_UNUSED
        internal const uint STATUS_SOME_NOT_MAPPED = 0x00000107;
        internal const uint STATUS_NO_MORE_FILES = 0x80000006;
        internal const uint STATUS_INVALID_PARAMETER = 0xC000000D;
        internal const uint STATUS_FILE_NOT_FOUND = 0xC000000F;
        internal const uint STATUS_NO_MEMORY = 0xC0000017;
        internal const uint STATUS_ACCESS_DENIED = 0xC0000022;
        internal const uint STATUS_OBJECT_NAME_NOT_FOUND = 0xC0000034;
        internal const uint STATUS_QUOTA_EXCEEDED = 0xC0000044;
        internal const uint STATUS_ACCOUNT_RESTRICTION = 0xC000006E;
        internal const uint STATUS_NONE_MAPPED = 0xC0000073;
        internal const uint STATUS_INSUFFICIENT_RESOURCES = 0xC000009A;
        internal const uint STATUS_DISK_FULL = 0xC000007F;
        internal const uint STATUS_FILE_TOO_LARGE = 0xC0000904;
#endif
    }
}
