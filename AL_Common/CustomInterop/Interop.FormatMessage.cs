// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Runtime.InteropServices;

namespace AL_Common;

internal static partial class Interop
{
    internal static partial class Kernel32
    {
        private const int FORMAT_MESSAGE_IGNORE_INSERTS = 0x00000200;
        private const int FORMAT_MESSAGE_FROM_HMODULE = 0x00000800;
        private const int FORMAT_MESSAGE_FROM_SYSTEM = 0x00001000;
        private const int FORMAT_MESSAGE_ARGUMENT_ARRAY = 0x00002000;
        private const int FORMAT_MESSAGE_ALLOCATE_BUFFER = 0x00000100;
        private const int ERROR_INSUFFICIENT_BUFFER = 0x7A;

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = true)]
        private static extern unsafe int FormatMessageW(
            int dwFlags,
            nint lpSource,
            uint dwMessageId,
            int dwLanguageId,
            void* lpBuffer,
            int nSize,
            nint arguments);

        /// <summary>
        ///     Returns a string message for the specified Win32 error code.
        /// </summary>
        internal static string GetMessage(int errorCode) =>
            GetMessage(errorCode, 0);

        internal static unsafe string GetMessage(int errorCode, nint moduleHandle)
        {
            int flags = FORMAT_MESSAGE_IGNORE_INSERTS | FORMAT_MESSAGE_FROM_SYSTEM | FORMAT_MESSAGE_ARGUMENT_ARRAY;
            if (moduleHandle != 0)
            {
                flags |= FORMAT_MESSAGE_FROM_HMODULE;
            }

            // First try to format the message into the stack based buffer.  Most error messages willl fit.
            Span<char> stackBuffer = stackalloc char[256]; // arbitrary stack limit
            fixed (char* bufferPtr = stackBuffer)
            {
                int length = FormatMessageW(flags, moduleHandle, unchecked((uint)errorCode), 0, bufferPtr, stackBuffer.Length, 0);
                if (length > 0)
                {
                    return GetAndTrimString(stackBuffer[..length]);
                }
            }

            // We got back an error.  If the error indicated that there wasn't enough room to store
            // the error message, then call FormatMessage again, but this time rather than passing in
            // a buffer, have the method allocate one, which we then need to free.
            if (Marshal.GetLastWin32Error() == ERROR_INSUFFICIENT_BUFFER)
            {
                nint nativeMsgPtr = default;
                try
                {
                    int length = FormatMessageW(flags | FORMAT_MESSAGE_ALLOCATE_BUFFER, moduleHandle, unchecked((uint)errorCode), 0, &nativeMsgPtr, 0, 0);
                    if (length > 0)
                    {
                        return GetAndTrimString(new ReadOnlySpan<char>((char*)nativeMsgPtr, length));
                    }
                }
                finally
                {
                    Marshal.FreeHGlobal(nativeMsgPtr);
                }
            }

            // Couldn't get a message, so manufacture one.
            return $"Unknown error (0x{errorCode:x})";
        }

        private static string GetAndTrimString(ReadOnlySpan<char> buffer)
        {
            int length = buffer.Length;
            while (length > 0 && buffer[length - 1] <= 32)
            {
                length--; // trim off spaces and non-printable ASCII chars at the end of the resource
            }
            return buffer[..length].ToString();
        }
    }
}
