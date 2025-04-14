// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Runtime.InteropServices;

namespace AL_Common;

internal static partial class Interop
{
    internal static partial class BCrypt
    {
        internal const int BCRYPT_USE_SYSTEM_PREFERRED_RNG = 0x00000002;

        [DllImport("BCrypt.dll")]
        internal static extern unsafe NTSTATUS BCryptGenRandom(nint hAlgorithm, byte* pbBuffer, int cbBuffer, int dwFlags);
    }
}