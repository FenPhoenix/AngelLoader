using System.Text;

namespace SharpCompress.Common.Rar.Headers;

/// <summary>
/// This is for the crazy Rar encoding that I don't understand
/// </summary>
internal static class FileNameDecoder
{
    internal static int GetChar(byte[] name, int pos) => name[pos] & 0xff;

    internal static string Decode(byte[] name, int encPos)
    {
        var decPos = 0;
        var flags = 0;
        var flagBits = 0;

        var highByte = GetChar(name, encPos++);
        var buf = new StringBuilder();
        while (encPos < name.Length)
        {
            if (flagBits == 0)
            {
                flags = GetChar(name, encPos++);
                flagBits = 8;
            }
            int low;
            switch (flags >> 6)
            {
                case 0:
                    buf.Append((char)(GetChar(name, encPos++)));
                    ++decPos;
                    break;

                case 1:
                    buf.Append((char)(GetChar(name, encPos++) + (highByte << 8)));
                    ++decPos;
                    break;

                case 2:
                    low = GetChar(name, encPos);
                    int high = GetChar(name, encPos + 1);
                    buf.Append((char)((high << 8) + low));
                    ++decPos;
                    encPos += 2;
                    break;

                case 3:
                    var length = GetChar(name, encPos++);
                    if ((length & 0x80) != 0)
                    {
                        var correction = GetChar(name, encPos++);
                        for (
                            length = (length & 0x7f) + 2;
                            length > 0 && decPos < name.Length;
                            length--, decPos++
                        )
                        {
                            low = (GetChar(name, decPos) + correction) & 0xff;
                            buf.Append((char)((highByte << 8) + low));
                        }
                    }
                    else
                    {
                        for (length += 2; length > 0 && decPos < name.Length; length--, decPos++)
                        {
                            buf.Append((char)(GetChar(name, decPos)));
                        }
                    }
                    break;
            }
            flags = (flags << 2) & 0xff;
            flagBits -= 2;
        }
        return buf.ToString();
    }
}
