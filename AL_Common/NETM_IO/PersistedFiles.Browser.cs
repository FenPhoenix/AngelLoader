// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace AL_Common.NETM_IO
{
    internal static partial class PersistedFiles
    {
        internal static string? GetHomeDirectory() => Environment.GetEnvironmentVariable("HOME");
    }
}
