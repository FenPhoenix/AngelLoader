using System;
using System.Text;

namespace SharpCompress.Common;

public static class ArchiveEncoding
{
    static ArchiveEncoding() => Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

    public static string Decode(byte[] bytes) => Decode(bytes, 0, bytes.Length);

    private static string Decode(byte[] bytes, int start, int length) =>
        GetDecoder().Invoke(bytes, start, length);

    private static Func<byte[], int, int, string> GetDecoder() => static (bytes, index, count) =>
        Encoding.Default.GetString(bytes, index, count);
}
