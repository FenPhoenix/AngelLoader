// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Runtime.InteropServices;

namespace AL_Common;

internal static partial class Interop
{
    internal static partial class Kernel32
    {
        /// <summary>
        /// WARNING: This method does not implicitly handle long paths. Use DeleteVolumeMountPoint.
        /// </summary>
        [LibraryImport("kernel32", EntryPoint = "DeleteVolumeMountPointW", SetLastError = true, StringMarshalling = StringMarshalling.Utf16)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool DeleteVolumeMountPointPrivate(string mountPoint);

        internal static bool DeleteVolumeMountPoint(string mountPoint)
        {
            mountPoint = PathInternal.EnsureExtendedPrefixIfNeeded(mountPoint);
            return DeleteVolumeMountPointPrivate(mountPoint);
        }
    }
}
