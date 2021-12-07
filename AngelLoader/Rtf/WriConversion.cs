﻿using System;
using System.IO;
using System.Text;

namespace AngelLoader
{
    internal static class WriConversion
    {
        /// <summary>
        /// Checks if a file has the correct .wri format header.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns><see langword="true"/> if the file has the correct .wri format header,
        /// <see langword="false"/> if it doesn't or if there was an error.</returns>
        internal static bool IsWriFile(string fileName)
        {
            try
            {
                using var fs = File.OpenRead(fileName);
                (bool success, _, _) = ReadWriFileHeader(fs);
                return success;
            }
            catch
            {
                return false;
            }
        }

        private static (bool Success, uint PlainTextStart, uint PlainTextEnd)
        ReadWriFileHeader(Stream stream)
        {
            var fail = (false, (uint)0, (uint)stream.Length);

            const ushort WIDENT_VALUE = 48689;         // 0137061 octal
            const ushort WIDENT_NO_OLE_VALUE = 48690;  // 0137062 octal
            const ushort WTOOL_VALUE = 43776;          // 0125400 octal

            using var br = new BinaryReader(stream, Encoding.ASCII, leaveOpen: true);

            ushort wIdent = br.ReadUInt16();
            if (wIdent != WIDENT_VALUE && wIdent != WIDENT_NO_OLE_VALUE)
            {
                return fail;
            }

            if (br.ReadUInt16() != 0) return fail; // dty
            if (br.ReadUInt16() != WTOOL_VALUE) return fail; // wTool
            if (br.ReadUInt16() != 0) return fail; // Reserved 1
            if (br.ReadUInt16() != 0) return fail; // Reserved 2
            if (br.ReadUInt16() != 0) return fail; // Reserved 3
            if (br.ReadUInt16() != 0) return fail; // Reserved 4
            uint fcMac = br.ReadUInt32();
            br.ReadUInt16(); // pnPara
            br.ReadUInt16(); // pnFntb
            br.ReadUInt16(); // pnSep
            br.ReadUInt16(); // pnSetb
            br.ReadUInt16(); // pnPgtb
            br.ReadUInt16(); // pnFfntb
            br.BaseStream.Position += 66; // szSsht (not used)
            if (br.ReadUInt16() == 0) return fail; // pnMac: 0 means Word file, not Write file

            // Headers are always 128 bytes long I think?!
            return (true, 128, fcMac);
        }

        // Quick and dirty .wri plaintext loader. Lucrative Opportunity is the only known FM with a .wri readme.
        // For that particular file, we can just cut off the start and end junk chars and end up with a 100%
        // clean plaintext readme. For other .wri files, there could be junk chars in the middle too, and then
        // we would have to parse the format properly. But we only have the one file, so we don't bother.
        internal static (bool Success, byte[] Bytes, string Text)
        LoadWriFileAsPlainText(byte[] bytes)
        {
            try
            {
                using var ms = new MemoryStream(bytes);
                (bool success, uint plainTextStart, uint plainTextEnd) = ReadWriFileHeader(ms);
                if (success)
                {
                    // Lucrative Opportunity is Windows-1252 encoded, so just go ahead and assume that
                    // encoding. It's probably a reasonable assumption for .wri files anyway.
                    Encoding enc1252 = Encoding.GetEncoding(1252);
                    byte[] tempByte = new byte[1];
                    var sb = new StringBuilder(bytes.Length);
                    for (uint i = plainTextStart; i < plainTextEnd; i++)
                    {
                        byte b = bytes[i];
                        if (b is 9 or 10 or 13 || (b >= 32 && b != 127))
                        {
                            if (b <= 126)
                            {
                                sb.Append((char)b);
                            }
                            else
                            {
                                tempByte[0] = b;
                                sb.Append(enc1252.GetChars(tempByte));
                            }
                        }
                    }

                    string text = sb.ToString();
                    return (true, enc1252.GetBytes(text), text);
                }
            }
            catch
            {
                return (false, Array.Empty<byte>(), "");
            }

            return (false, Array.Empty<byte>(), "");
        }
    }
}
