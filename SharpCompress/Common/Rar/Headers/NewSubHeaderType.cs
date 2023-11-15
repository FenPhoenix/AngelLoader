using System;

namespace SharpCompress.Common.Rar.Headers;

internal sealed class NewSubHeaderType : IEquatable<NewSubHeaderType>
{
    internal static readonly NewSubHeaderType SUBHEAD_TYPE_RR = new('R', 'R');

    private readonly byte[] _bytes;

    private NewSubHeaderType(params char[] chars)
    {
        _bytes = new byte[chars.Length];
        for (var i = 0; i < chars.Length; ++i)
        {
            _bytes[i] = (byte)chars[i];
        }
    }

    internal bool Equals(byte[] bytes)
    {
        if (_bytes.Length != bytes.Length)
        {
            return false;
        }

        return _bytes.AsSpan().SequenceEqual(bytes);
    }

    public bool Equals(NewSubHeaderType? other) => other is not null && Equals(other._bytes);
}
