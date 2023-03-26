namespace SharpCompress.Common.SevenZip;

internal readonly struct CMethodId
{
    public const ulong K_AES_ID = 0x06F10701;

    public readonly ulong _id;

    public CMethodId(ulong id) => _id = id;

    public override int GetHashCode() => _id.GetHashCode();

    public override bool Equals(object? obj) => obj is CMethodId other && Equals(other);

    public bool Equals(CMethodId other) => _id == other._id;

    public static bool operator ==(CMethodId left, CMethodId right) => left._id == right._id;

    public static bool operator !=(CMethodId left, CMethodId right) => left._id != right._id;
}
