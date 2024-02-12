using SharpCompress.IO;

namespace SharpCompress.Common.Rar.Headers;

internal sealed class EndArchiveHeader : RarHeader
{
    public EndArchiveHeader(RarHeader header, RarCrcBinaryReader reader)
        : base(header, reader, HeaderType.EndArchive) { }

    protected override void ReadFinish(MarkingBinaryReader reader)
    {
        if (IsRar5)
        {
            Flags = reader.ReadRarVIntUInt16();
        }
        else
        {
            Flags = HeaderFlags;
            if (HasFlag(EndArchiveFlagsV4.DATA_CRC))
            {
                reader.ReadInt32();
            }
            if (HasFlag(EndArchiveFlagsV4.VOLUME_NUMBER))
            {
                reader.ReadInt16();
            }
        }
    }

    private ushort Flags;

    private bool HasFlag(ushort flag) => (Flags & flag) == flag;
}
