namespace SharpCompress.Compressors.Rar.UnpackV1.Decode;

internal sealed class AudioVariables
{
    internal AudioVariables() => Dif = new int[11];

    internal readonly int[] Dif;
    internal int ByteCount;
    internal int D1;

    internal int D2;
    internal int D3;
    internal int D4;

    internal int K1;
    internal int K2;
    internal int K3;
    internal int K4;
    internal int K5;
    internal int LastChar;
    internal int LastDelta;
}
