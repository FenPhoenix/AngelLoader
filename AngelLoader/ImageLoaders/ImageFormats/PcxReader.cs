using System;
using System.Buffers.Binary;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

/*

Decoder for ZSoft Paintbrush (.PCX) images.
Supports pretty much the full PCX specification (all bit
depths, etc).  At the very least, it decodes all PCX images that
I've found in the wild.  If you find one that it fails to decode,
let me know!

Copyright 2013-2023 Dmitry Brant
http://dmitrybrant.com

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

   http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.

----------------------------------------------------

*** Modified by FenPhoenix 2026:
-Support .NET Framework
-Reduce allocations
-Use stream.ReadAll()
-Use some internal, faster versions of things
-General cleanup
-Performance

*/

namespace DmitryBrant.ImageFormats;

/// <summary>
/// Handles reading ZSoft PCX images.
/// </summary>
public static class PcxReader
{
    private static readonly int[] _egaColors =
    {
        0x0, 0x0000AA, 0x00AA00, 0x00AAAA, 0xAA0000, 0xAA00AA, 0xAA5500, 0xAAAAAA,
        0x555555, 0x5555FF, 0x55FF55, 0x55FFFF, 0xFF5555, 0xFF55FF, 0xFFFF55, 0xFFFFFF,
    };

    private static int _currentPosition;
    private static int _currentByte;
    private static int _runLength;

    /// <summary>
    /// Reads a PCX image from a file.
    /// </summary>
    /// <param name="fileName">Name of the file to read.</param>
    /// <returns>Bitmap that contains the image that was read.</returns>
    public static Bitmap Load(string fileName)
    {
        byte[] bytes = File_ReadAllBytesFast(fileName);

        if (bytes.Length < 128)
            throw new InvalidDataException("PCX file isn't long enough to have a valid header.");

        byte tempByte = bytes[0];
        if (tempByte != 10)
            throw new InvalidDataException("This is not a valid PCX file.");

        byte version = bytes[1];
        if (version > 5)
            throw new InvalidDataException("Only Version 5 or lower PCX files are supported.");

        // This variable controls whether the bit plane values are interpreted as literal color states
        // instead of indices into the palette. In other words, this controls whether the palette is
        // used or ignored. As far as I can tell the only way to determine whether the palette is used
        // is by the version number of the file.
        // If the colors in your decoded picture look weird, try tweaking this variable.
        bool usePalette = version != 3 && version != 4; // PaintBrush 2.8 without palette information.

        tempByte = bytes[2];
        if (tempByte != 1)
            throw new InvalidDataException("Invalid PCX compression type.");

        int imgBpp = bytes[3];
        if (imgBpp != 8 && imgBpp != 4 && imgBpp != 2 && imgBpp != 1)
            throw new InvalidDataException("Only 8, 4, 2, and 1-bit PCX samples are supported.");

        ushort xmin = LittleEndian(Unsafe.ReadUnaligned<ushort>(ref bytes[4]));
        ushort ymin = LittleEndian(Unsafe.ReadUnaligned<ushort>(ref bytes[6]));
        ushort xmax = LittleEndian(Unsafe.ReadUnaligned<ushort>(ref bytes[8]));
        ushort ymax = LittleEndian(Unsafe.ReadUnaligned<ushort>(ref bytes[10]));

        int imgWidth = xmax - xmin + 1;
        int imgHeight = ymax - ymin + 1;

        if ((imgWidth < 1) || (imgHeight < 1) || (imgWidth > 32767) || (imgHeight > 32767))
            throw new InvalidDataException("This PCX file appears to have invalid dimensions.");

        byte[] colorPalette = new byte[48];
        Array.Copy(bytes, 16, colorPalette, 0, 48);

        int numPlanes = bytes[65];
        int bytesPerLine = (int)LittleEndian(Unsafe.ReadUnaligned<ushort>(ref bytes[66]));
        if (bytesPerLine == 0) bytesPerLine = xmax - xmin + 1;

        int paletteInfo = LittleEndian(Unsafe.ReadUnaligned<ushort>(ref bytes[68]));

        if (imgBpp == 8 && numPlanes == 1)
        {
            if (bytes.Length < 768)
                throw new InvalidDataException("PCX file not long enough to have 768-byte palette.");

            colorPalette = new byte[768];
            Array.Copy(bytes, bytes.Length - 768, colorPalette, 0, 768);
        }

        if (imgBpp == 1 && numPlanes == 1 && usePalette)
        {
            usePalette = false;

            // With 1-bpp images that claim to have palette information, we have a bit of a problem:
            // Images seen in the wild don't seem to be consistent about whether they actually use
            // the palette for 1-bit images.
            // This is a hacky way to detect whether the palette should be used. We look at the first
            // three RGB triplets, and if they are nonzero, *and* if the rest of the palette is zero,
            // then the palette will be used. Otherwise, the 1-bit image will be treated as black/white.
            bool remainingZeros = true;
            for (int c = 6; c < colorPalette.Length; c++)
            {
                if (colorPalette[c] != 0)
                {
                    remainingZeros = false;
                    break;
                }
            }
            if (remainingZeros && (colorPalette[0] != 0 || colorPalette[1] != 0 || colorPalette[2] != 0
                                   || colorPalette[3] != 0 || colorPalette[4] != 0 || colorPalette[5] != 0))
            {
                usePalette = true;
            }
        }

        if (!usePalette && imgBpp == 1 && (numPlanes == 3 || numPlanes == 4))
        {
            // Special handling for EGA images that don't contain palette information:
            // Pre-populate our palette with standard EGA colors.
            if (numPlanes == 3)
            {
                for (int c = 0; c < 8; c++)
                {
                    colorPalette[c * 3] = (byte)((_egaColors[c + 8] >> 16) & 0xFF);
                    colorPalette[c * 3 + 1] = (byte)((_egaColors[c + 8] >> 8) & 0xFF);
                    colorPalette[c * 3 + 2] = (byte)(_egaColors[c + 8] & 0xFF);
                }
                // Hack: make color 0 black instead of gray.
                colorPalette[0] = colorPalette[1] = colorPalette[2] = 0;
            }
            else if (numPlanes == 4)
            {
                for (int c = 0; c < 16; c++)
                {
                    colorPalette[c * 3] = (byte)((_egaColors[c] >> 16) & 0xFF);
                    colorPalette[c * 3 + 1] = (byte)((_egaColors[c] >> 8) & 0xFF);
                    colorPalette[c * 3 + 2] = (byte)(_egaColors[c] & 0xFF);
                }
            }
        }

        byte[] bmpData = new byte[(imgWidth + 1) * 4 * imgHeight];
        int x, y, i;

        _currentPosition = 128;
        _currentByte = 0;
        _runLength = 0;

        try
        {
            if (imgBpp == 1)
            {
                int b, p;
                byte val;
                byte[] scanline = new byte[bytesPerLine];
                byte[] realscanline = new byte[bytesPerLine * 8];

                for (y = 0; y < imgHeight; y++)
                {
                    //add together all the planes...
                    Array.Clear(realscanline, 0, realscanline.Length);
                    for (p = 0; p < numPlanes; p++)
                    {
                        x = 0;
                        for (i = 0; i < bytesPerLine; i++)
                        {
                            scanline[i] = (byte)ReadByte(bytes);

                            for (b = 7; b >= 0; b--)
                            {
                                if ((scanline[i] & (1 << b)) != 0) val = 1; else val = 0;
                                realscanline[x] |= (byte)(val << p);
                                x++;
                            }
                        }
                    }

                    for (x = 0; x < imgWidth; x++)
                    {
                        i = realscanline[x];

                        if (!usePalette && numPlanes == 1)
                        {
                            b = i != 0 ? 0xFF : 0;
                            bmpData[4 * (y * imgWidth + x)] = (byte)b;
                            bmpData[4 * (y * imgWidth + x) + 1] = (byte)b;
                            bmpData[4 * (y * imgWidth + x) + 2] = (byte)b;
                        }
                        else
                        {
                            bmpData[4 * (y * imgWidth + x)] = colorPalette[i * 3 + 2];
                            bmpData[4 * (y * imgWidth + x) + 1] = colorPalette[i * 3 + 1];
                            bmpData[4 * (y * imgWidth + x) + 2] = colorPalette[i * 3];
                        }
                    }
                }
            }
            else
            {
                if (numPlanes == 1)
                {
                    if (imgBpp == 8)
                    {
                        byte[] scanline = new byte[bytesPerLine];
                        for (y = 0; y < imgHeight; y++)
                        {
                            for (i = 0; i < bytesPerLine; i++)
                                scanline[i] = (byte)ReadByte(bytes);

                            for (x = 0; x < imgWidth; x++)
                            {
                                i = scanline[x];
                                bmpData[4 * (y * imgWidth + x)] = colorPalette[i * 3 + 2];
                                bmpData[4 * (y * imgWidth + x) + 1] = colorPalette[i * 3 + 1];
                                bmpData[4 * (y * imgWidth + x) + 2] = colorPalette[i * 3];
                            }
                        }
                    }
                    else if (imgBpp == 4)
                    {
                        byte[] scanline = new byte[bytesPerLine];
                        for (y = 0; y < imgHeight; y++)
                        {
                            for (i = 0; i < bytesPerLine; i++)
                                scanline[i] = (byte)ReadByte(bytes);

                            for (x = 0; x < imgWidth; x++)
                            {
                                i = scanline[x / 2];
                                bmpData[4 * (y * imgWidth + x)] = colorPalette[((i >> 4) & 0xF) * 3 + 2];
                                bmpData[4 * (y * imgWidth + x) + 1] = colorPalette[((i >> 4) & 0xF) * 3 + 1];
                                bmpData[4 * (y * imgWidth + x) + 2] = colorPalette[((i >> 4) & 0xF) * 3];
                                x++;
                                bmpData[4 * (y * imgWidth + x)] = colorPalette[(i & 0xF) * 3 + 2];
                                bmpData[4 * (y * imgWidth + x) + 1] = colorPalette[(i & 0xF) * 3 + 1];
                                bmpData[4 * (y * imgWidth + x) + 2] = colorPalette[(i & 0xF) * 3];
                            }
                        }
                    }
                    else if (imgBpp == 2)
                    {
                        byte[] scanline = new byte[bytesPerLine];
                        for (y = 0; y < imgHeight; y++)
                        {
                            for (i = 0; i < bytesPerLine; i++)
                                scanline[i] = (byte)ReadByte(bytes);

                            for (x = 0; x < imgWidth; x++)
                            {
                                i = scanline[x / 4];
                                bmpData[4 * (y * imgWidth + x)] = colorPalette[((i >> 6) & 0x3) * 3 + 2];
                                bmpData[4 * (y * imgWidth + x) + 1] = colorPalette[((i >> 6) & 0x3) * 3 + 1];
                                bmpData[4 * (y * imgWidth + x) + 2] = colorPalette[((i >> 6) & 0x3) * 3];
                                x++;
                                bmpData[4 * (y * imgWidth + x)] = colorPalette[((i >> 4) & 0x3) * 3 + 2];
                                bmpData[4 * (y * imgWidth + x) + 1] = colorPalette[((i >> 4) & 0x3) * 3 + 1];
                                bmpData[4 * (y * imgWidth + x) + 2] = colorPalette[((i >> 4) & 0x3) * 3];
                                x++;
                                bmpData[4 * (y * imgWidth + x)] = colorPalette[((i >> 2) & 0x3) * 3 + 2];
                                bmpData[4 * (y * imgWidth + x) + 1] = colorPalette[((i >> 2) & 0x3) * 3 + 1];
                                bmpData[4 * (y * imgWidth + x) + 2] = colorPalette[((i >> 2) & 0x3) * 3];
                                x++;
                                bmpData[4 * (y * imgWidth + x)] = colorPalette[(i & 0x3) * 3 + 2];
                                bmpData[4 * (y * imgWidth + x) + 1] = colorPalette[(i & 0x3) * 3 + 1];
                                bmpData[4 * (y * imgWidth + x) + 2] = colorPalette[(i & 0x3) * 3];
                            }
                        }
                    }
                }
                else if (numPlanes == 3)
                {
                    // *** This is the one it ends up at for our Thief-generated PCX images

                    byte[] scanlineR = new byte[bytesPerLine];
                    byte[] scanlineG = new byte[bytesPerLine];
                    byte[] scanlineB = new byte[bytesPerLine];
                    int bytePtr = 0;

                    for (y = 0; y < imgHeight; y++)
                    {
                        for (i = 0; i < bytesPerLine; i++)
                            scanlineR[i] = (byte)ReadByte(bytes);
                        for (i = 0; i < bytesPerLine; i++)
                            scanlineG[i] = (byte)ReadByte(bytes);
                        for (i = 0; i < bytesPerLine; i++)
                            scanlineB[i] = (byte)ReadByte(bytes);

                        for (int n = 0; n < imgWidth; n++)
                        {
                            bmpData[bytePtr++] = scanlineB[n];
                            bmpData[bytePtr++] = scanlineG[n];
                            bmpData[bytePtr++] = scanlineR[n];
                            bytePtr++;
                        }
                    }
                }

            }//bpp

        }
        catch (Exception e)
        {
            // return a partial image in case of unexpected end-of-file
            System.Diagnostics.Debug.WriteLine("Error while processing PCX file: " + e.Message);
        }

        Bitmap bmp = new(imgWidth, imgHeight, PixelFormat.Format32bppRgb);
        BitmapData bmpBits = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppRgb);
        Marshal.Copy(bmpData, 0, bmpBits.Scan0, imgWidth * 4 * imgHeight);
        bmp.UnlockBits(bmpBits);

        return bmp;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int ReadByte(byte[] bytes)
    {
        _runLength--;
        if (_runLength <= 0)
        {
            _currentByte = bytes[_currentPosition++];
            if (_currentByte > 191)
            {
                _runLength = _currentByte - 192;
                _currentByte = bytes[_currentPosition++];
            }
        }
        return _currentByte;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static ushort LittleEndian(ushort val)
    {
        return BitConverter.IsLittleEndian ? val : BinaryPrimitives.ReverseEndianness(val);
    }
}
