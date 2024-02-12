namespace SharpCompress.Compressors.Rar.UnpackV1.Decode;

internal class Decode
{
    protected Decode(int[] customDecodeNum)
    {
        DecodeLen = new int[16];
        DecodePos = new int[16];
        DecodeNum = customDecodeNum;
    }

    /// <summary> returns the decode Length array</summary>
    /// <returns> decodeLength
    /// </returns>
    internal readonly int[] DecodeLen;

    /// <summary> returns the decode num array</summary>
    /// <returns> decodeNum
    /// </returns>
    internal readonly int[] DecodeNum;

    /// <summary> returns the decodePos array</summary>
    /// <returns> decodePos
    /// </returns>
    internal readonly int[] DecodePos;

    internal int MaxNum;
}
