using System;
using System.Text;

namespace SharpCompress_7z.Common;

public sealed class ArchiveEncoding
{
    /// <summary>
    /// Default encoding to use when archive format doesn't specify one.
    /// </summary>
    public Encoding Default { get; }

    /// <summary>
    /// Set this when you want to use a custom method for all decoding operations.
    /// </summary>
    /// <returns>string Func(bytes, index, length)</returns>
    public Func<byte[], int, int, string>? CustomDecoder { get; set; }

    public ArchiveEncoding()
        : this(Encoding.Default) { }

    public ArchiveEncoding(Encoding def)
    {
        Default = def;
    }

#if !NETFRAMEWORK
    static ArchiveEncoding() => Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
#endif

    public string Decode(byte[] bytes) => Decode(bytes, 0, bytes.Length);

    public string Decode(byte[] bytes, int start, int length) =>
        GetDecoder().Invoke(bytes, start, length);

    public Encoding GetEncoding() => Default ?? Encoding.UTF8;

    public Func<byte[], int, int, string> GetDecoder() =>
        CustomDecoder ?? ((bytes, index, count) => GetEncoding().GetString(bytes, index, count));
}
