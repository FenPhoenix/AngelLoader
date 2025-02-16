// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Runtime.InteropServices;

namespace AL_Common;

internal static partial class Interop
{
    internal static partial class Kernel32
    {
        /// <summary>
        /// WARNING: This method does not implicitly handle long paths. Use GetFileAttributesEx.
        /// </summary>
        [DllImport(
            "kernel32.dll",
            EntryPoint = "GetFileAttributesExW",
            ExactSpelling = true,
            SetLastError = true,
            CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetFileAttributesExPrivate(
            string? name,
            GET_FILEEX_INFO_LEVELS fileInfoLevel,
            ref WIN32_FILE_ATTRIBUTE_DATA lpFileInformation);

        internal static bool GetFileAttributesEx(string? name, GET_FILEEX_INFO_LEVELS fileInfoLevel, ref WIN32_FILE_ATTRIBUTE_DATA lpFileInformation)
        {
            name = PathInternal.EnsureExtendedPrefixIfNeeded(name);
            return GetFileAttributesExPrivate(name, fileInfoLevel, ref lpFileInformation);
        }
    }
}
