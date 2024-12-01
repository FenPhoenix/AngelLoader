// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.IO;

namespace AL_Common.NETM_IO.Strategies
{
    internal sealed class SyncWindowsFileStreamStrategy : OSFileStreamStrategy
    {
        internal SyncWindowsFileStreamStrategy(AL_SafeFileHandle handle, FileAccess access) : base(handle, access)
        {
        }

        internal SyncWindowsFileStreamStrategy(string path, FileMode mode, FileAccess access, FileShare share, FileOptions options, long preallocationSize)
            : base(path, mode, access, share, options, preallocationSize)
        {
        }

        internal override bool IsAsync => false;
    }
}
