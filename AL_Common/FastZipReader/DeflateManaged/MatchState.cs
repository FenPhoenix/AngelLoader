// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace AL_Common.FastZipReader.DeflateManaged
{
    internal enum MatchState
    {
        HasSymbol = 1,
        HasMatch = 2,
        HasSymbolAndMatch = 3,
    }
}
