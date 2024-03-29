using SharpCompress.IO;

namespace SharpCompress.Common.Rar.Headers;

internal sealed class ArchiveHeader : RarHeader
{
    public ArchiveHeader(RarHeader header, RarCrcBinaryReader reader)
        : base(header, reader, HeaderType.Archive) { }

    protected override void ReadFinish(MarkingBinaryReader reader)
    {
        if (IsRar5)
        {
            Flags = reader.ReadRarVIntUInt16();
            if (HasFlag(ArchiveFlagsV5.HAS_VOLUME_NUMBER))
            {
                _ = (int)reader.ReadRarVIntUInt32();
            }
            // later: we may have a locator record if we need it
            //if (ExtraSize != 0) {
            //    ReadLocator(reader);
            //}
        }
        else
        {
            Flags = HeaderFlags;
            reader.ReadInt16();
            reader.ReadInt32();
            if (HasFlag(ArchiveFlagsV4.ENCRYPT_VER))
            {
                reader.ReadByte();
            }
        }
    }

    private ushort Flags;

    private bool HasFlag(ushort flag) => (Flags & flag) == flag;

    public bool? IsEncrypted => IsRar5 ? null : HasFlag(ArchiveFlagsV4.PASSWORD);

    public bool IsVolume => HasFlag(IsRar5 ? ArchiveFlagsV5.VOLUME : ArchiveFlagsV4.VOLUME);

    public bool IsSolid => HasFlag(IsRar5 ? ArchiveFlagsV5.SOLID : ArchiveFlagsV4.SOLID);
}
