﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Runtime.InteropServices;

namespace AL_Common;

internal static partial class Interop
{
    internal static partial class Kernel32
    {
        /// <summary>
        /// WARNING: This method does not implicitly handle long paths. Use DeleteFile.
        /// </summary>
        [LibraryImport("kernel32", EntryPoint = "DeleteFileW", SetLastError = true, StringMarshalling = StringMarshalling.Utf16)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool DeleteFilePrivate(string path);

        internal static bool DeleteFile(string path)
        {
            path = PathInternal.EnsureExtendedPrefixIfNeeded(path);
            return DeleteFilePrivate(path);
        }
    }
}
