//#define ENABLE_UNUSED

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace AL_Common;

internal static partial class Interop
{
    internal static partial class Kernel32
    {
#if ENABLE_UNUSED
        /// <summary>
        /// WARNING: This method does not implicitly handle long paths. Use GetFileAttributesEx.
        /// </summary>
        [System.Runtime.InteropServices.DllImport(
            "kernel32.dll",
            EntryPoint = "GetFileAttributesExW",
            SetLastError = true,
            CharSet = System.Runtime.InteropServices.CharSet.Unicode)]
        [return: System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.Bool)]
        private static extern bool GetFileAttributesExPrivate(
            string? name,
            GET_FILEEX_INFO_LEVELS fileInfoLevel,
            ref WIN32_FILE_ATTRIBUTE_DATA lpFileInformation);

        internal static bool GetFileAttributesEx(string? name, GET_FILEEX_INFO_LEVELS fileInfoLevel, ref WIN32_FILE_ATTRIBUTE_DATA lpFileInformation)
        {
            name = PathInternal.EnsureExtendedPrefixIfNeeded(name);
            return GetFileAttributesExPrivate(name, fileInfoLevel, ref lpFileInformation);
        }
#endif
    }
}
