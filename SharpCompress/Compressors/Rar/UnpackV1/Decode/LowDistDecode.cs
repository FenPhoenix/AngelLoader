namespace SharpCompress.Compressors.Rar.UnpackV1.Decode;

internal sealed class LowDistDecode : Decode
{
    internal LowDistDecode()
        : base(new int[PackDef.LDC]) { }
}
