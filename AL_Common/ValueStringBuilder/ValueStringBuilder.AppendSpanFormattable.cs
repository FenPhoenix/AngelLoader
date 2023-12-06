// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

//#define ENABLE_UNUSED

#if ENABLE_UNUSED

using System;

namespace AL_Common;

internal ref partial struct ValueStringBuilder
{
    internal void AppendSpanFormattable<T>(T value, string? format = null, IFormatProvider? provider = null) where T : ISpanFormattable
    {
        if (value.TryFormat(_chars.Slice(_pos), out int charsWritten, format, provider))
        {
            _pos += charsWritten;
        }
        else
        {
            Append(value.ToString(format, provider));
        }
    }
}
#endif
