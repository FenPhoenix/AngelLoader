#nullable disable

using SharpCompress.IO;

namespace SharpCompress.Common.Rar.Headers;

internal sealed class ArchiveCryptHeader : RarHeader
{
    private const int CRYPT_VERSION = 0; // Supported encryption version.
    private const int SIZE_SALT50 = 16;
    private const int SIZE_PSWCHECK = 8;
    private const int SIZE_PSWCHECK_CSUM = 4;
    private const int CRYPT5_KDF_LG2_COUNT_MAX = 24; // LOG2 of maximum accepted iteration count.

    private bool _usePswCheck;
    private uint _lg2Count; // Log2 of PBKDF2 repetition count.

    public ArchiveCryptHeader(RarHeader header, RarCrcBinaryReader reader)
        : base(header, reader, HeaderType.Crypt) { }

    protected override void ReadFinish(MarkingBinaryReader reader)
    {
        var cryptVersion = reader.ReadRarVIntUInt32();
        if (cryptVersion > CRYPT_VERSION)
        {
            //error?
            return;
        }
        var encryptionFlags = reader.ReadRarVIntUInt32();
        _usePswCheck = FlagUtility.HasFlag(encryptionFlags, EncryptionFlagsV5.CHFL_CRYPT_PSWCHECK);
        _lg2Count = reader.ReadRarVIntByte(1);

        //UsePswCheck = HasHeaderFlag(EncryptionFlagsV5.CHFL_CRYPT_PSWCHECK);
        if (_lg2Count > CRYPT5_KDF_LG2_COUNT_MAX)
        {
            //error?
            return;
        }

        reader.ReadBytes(SIZE_SALT50);
        if (_usePswCheck)
        {
            reader.ReadBytes(SIZE_PSWCHECK);
            reader.ReadBytes(SIZE_PSWCHECK_CSUM);
        }
    }
}
