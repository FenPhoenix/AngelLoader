using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
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

    /// <summary>
    /// Reads a PCX image from a file.
    /// </summary>
    /// <param name="fileName">Name of the file to read.</param>
    /// <returns>Bitmap that contains the image that was read.</returns>
    public static Bitmap Load(string fileName)
    {
        using FileStream_NET f = File_OpenReadFast(fileName, FileStreamBufferSize);
        return Load(f);
    }

    /// <summary>
    /// Reads a PCX image from a stream.
    /// </summary>
    /// <param name="stream">Stream from which to read the image.</param>
    /// <param name="useCgaPalette">Whether to treat the image as having a CGA palette configuration.
    /// This is impossible to detect automatically from the file header, so this parameter can be
    /// specified explicitly if you expect this image to use CGA palette information, as defined in
    /// the PCX specification.</param>
    /// <returns>Bitmap that contains the image that was read.</returns>
    public static Bitmap Load(Stream stream, bool useCgaPalette = false)
    {
        BinaryBuffer binaryBuffer = new();

        byte tempByte = (byte)stream.ReadByte();
        if (tempByte != 10)
            throw new InvalidDataException("This is not a valid PCX file.");

        byte version = (byte)stream.ReadByte();
        if (version > 5)
            throw new InvalidDataException("Only Version 5 or lower PCX files are supported.");

        // This variable controls whether the bit plane values are interpreted as literal color states
        // instead of indices into the palette. In other words, this controls whether the palette is
        // used or ignored. As far as I can tell the only way to determine whether the palette is used
        // is by the version number of the file.
        // If the colors in your decoded picture look weird, try tweaking this variable.
        bool usePalette = version != 3 && version != 4; // PaintBrush 2.8 without palette information.

        tempByte = (byte)stream.ReadByte();
        if (tempByte != 1)
            throw new InvalidDataException("Invalid PCX compression type.");

        int imgBpp = stream.ReadByte();
        if (imgBpp != 8 && imgBpp != 4 && imgBpp != 2 && imgBpp != 1)
            throw new InvalidDataException("Only 8, 4, 2, and 1-bit PCX samples are supported.");

        ushort xmin = Util.LittleEndian(BinaryRead.ReadUInt16(stream, binaryBuffer));
        ushort ymin = Util.LittleEndian(BinaryRead.ReadUInt16(stream, binaryBuffer));
        ushort xmax = Util.LittleEndian(BinaryRead.ReadUInt16(stream, binaryBuffer));
        ushort ymax = Util.LittleEndian(BinaryRead.ReadUInt16(stream, binaryBuffer));

        int imgWidth = xmax - xmin + 1;
        int imgHeight = ymax - ymin + 1;

        if ((imgWidth < 1) || (imgHeight < 1) || (imgWidth > 32767) || (imgHeight > 32767))
            throw new InvalidDataException("This PCX file appears to have invalid dimensions.");

        Util.LittleEndian(BinaryRead.ReadUInt16(stream, binaryBuffer)); //hdpi
        Util.LittleEndian(BinaryRead.ReadUInt16(stream, binaryBuffer)); //vdpi

        byte[] colorPalette = new byte[48];
        stream.ReadAll(colorPalette, 0, 48);
        stream.ReadByte();

        int numPlanes = stream.ReadByte();
        int bytesPerLine = Util.LittleEndian(BinaryRead.ReadUInt16(stream, binaryBuffer));
        if (bytesPerLine == 0) bytesPerLine = xmax - xmin + 1;

        int paletteInfo = Util.LittleEndian(BinaryRead.ReadUInt16(stream, binaryBuffer));

        if (imgBpp == 8 && numPlanes == 1)
        {
            colorPalette = new byte[768];
            stream.Seek(-768, SeekOrigin.End);
            stream.ReadAll(colorPalette, 0, 768);
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

        if (useCgaPalette && usePalette)
        {
            int backgroundColor = colorPalette[0] >> 4;
            int flags = colorPalette[3] >> 5;
            int[] newColors = new int[4];

            if (imgBpp == 1)
            {
                // For monochrome images, the background color actually goes in palette index 1,
                // and index 0 is always black.
                newColors[0] = 0;
                newColors[1] = _egaColors[backgroundColor];
            }
            else if (imgBpp == 2)
            {
                bool intensity;
                bool palette0or1;
                if (paletteInfo == 0)
                {
                    palette0or1 = (flags & 0x2) != 0;
                    intensity = (flags & 0x1) != 0;
                }
                else
                {
                    palette0or1 = colorPalette[4] <= colorPalette[5];
                    intensity = colorPalette[4] > 200 || colorPalette[5] > 200;
                }

                newColors[0] = _egaColors[backgroundColor];
                if (!palette0or1)
                {
                    if (!intensity) { newColors[1] = 0x00AA00; newColors[2] = 0xAA0000; newColors[3] = 0xAA5500; }
                    else { newColors[1] = 0x55FF55; newColors[2] = 0xFF5555; newColors[3] = 0xFFFF55; }
                }
                else
                {
                    if (!intensity) { newColors[1] = 0x00AAAA; newColors[2] = 0xAA00AA; newColors[3] = 0xAAAAAA; }
                    else { newColors[1] = 0x55FFFF; newColors[2] = 0xFF55FF; newColors[3] = 0xFFFFFF; }
                }
            }
            for (int c = 0; c < 4; c++)
            {
                colorPalette[c * 3] = (byte)((newColors[c] >> 16) & 0xFF);
                colorPalette[c * 3 + 1] = (byte)((newColors[c] >> 8) & 0xFF);
                colorPalette[c * 3 + 2] = (byte)(newColors[c] & 0xFF);
            }
        }

        byte[] bmpData = new byte[(imgWidth + 1) * 4 * imgHeight];
        stream.Seek(128, SeekOrigin.Begin);
        int x, y, i;

        RleReader rleReader = new(stream);

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
                            scanline[i] = (byte)rleReader.ReadByte();

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
                                scanline[i] = (byte)rleReader.ReadByte();

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
                                scanline[i] = (byte)rleReader.ReadByte();

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
                                scanline[i] = (byte)rleReader.ReadByte();

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
                    byte[] scanlineR = new byte[bytesPerLine];
                    byte[] scanlineG = new byte[bytesPerLine];
                    byte[] scanlineB = new byte[bytesPerLine];
                    int bytePtr = 0;

                    for (y = 0; y < imgHeight; y++)
                    {
                        for (i = 0; i < bytesPerLine; i++)
                            scanlineR[i] = (byte)rleReader.ReadByte();
                        for (i = 0; i < bytesPerLine; i++)
                            scanlineG[i] = (byte)rleReader.ReadByte();
                        for (i = 0; i < bytesPerLine; i++)
                            scanlineB[i] = (byte)rleReader.ReadByte();

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

    private static class Util
    {
        public static ushort LittleEndian(ushort val)
        {
            return BitConverter.IsLittleEndian ? val : ConvEndian(val);
        }

        private static ushort ConvEndian(ushort val)
        {
            ushort temp = (ushort)(val << 8); temp &= 0xFF00; temp |= (ushort)((val >> 8) & 0xFF);
            return temp;
        }
    }

    /// <summary>
    /// Helper class for reading a run-length encoded stream in a PCX file.
    /// </summary>
    private sealed class RleReader
    {
        private int _currentByte;
        private int _runLength;
        private readonly Stream _stream;

        public RleReader(Stream stream) => _stream = stream;

        public int ReadByte()
        {
            _runLength--;
            if (_runLength <= 0)
            {
                _currentByte = _stream.ReadByte();
                if (_currentByte > 191)
                {
                    _runLength = _currentByte - 192;
                    _currentByte = _stream.ReadByte();
                }
            }
            return _currentByte;
        }
    }
}
