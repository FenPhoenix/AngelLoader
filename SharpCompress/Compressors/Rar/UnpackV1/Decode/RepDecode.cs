namespace SharpCompress.Compressors.Rar.UnpackV1.Decode;

internal sealed class RepDecode : Decode
{
    internal RepDecode()
        : base(new int[PackDef.RC]) { }
}
