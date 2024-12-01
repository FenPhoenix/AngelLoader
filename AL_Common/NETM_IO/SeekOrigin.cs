// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace AL_Common.NETM_IO
{
    // Provides seek reference points.  To seek to the end of a stream,
    // call stream.Seek(0, SeekOrigin.End).
    public enum SeekOrigin
    {
        // These constants match Win32's FILE_BEGIN, FILE_CURRENT, and FILE_END
        Begin = 0,
        Current = 1,
        End = 2,
    }
}
