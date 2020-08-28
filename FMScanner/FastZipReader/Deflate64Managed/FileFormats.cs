// Fen's note: This is straight out of .NET Core 3 with no functional changes. I'm trusting it to be correct and
// working, and I'm not gonna touch it even for nullability.

#nullable disable

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace FMScanner.FastZipReader.Deflate64Managed
{
    internal interface IFileFormatReader
    {
        bool ReadHeader(InputBuffer input);
        bool ReadFooter(InputBuffer input);
        void UpdateWithBytesRead(byte[] buffer, int offset, int bytesToCopy);
        void Validate();
    }
}
