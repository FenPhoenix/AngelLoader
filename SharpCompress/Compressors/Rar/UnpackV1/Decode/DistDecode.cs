namespace SharpCompress.Compressors.Rar.UnpackV1.Decode;

internal sealed class DistDecode : Decode
{
    internal DistDecode()
        : base(new int[PackDef.DC]) { }
}
