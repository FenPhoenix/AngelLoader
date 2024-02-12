namespace SharpCompress.Compressors.Rar.VM;

internal sealed class VMStandardFilterSignature
{
    internal VMStandardFilterSignature(int length, uint crc, VMStandardFilters type)
    {
        Length = length;
        CRC = crc;
        Type = type;
    }

    internal readonly int Length;

    internal readonly uint CRC;

    internal readonly VMStandardFilters Type;
}
