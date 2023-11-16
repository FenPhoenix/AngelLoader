namespace SharpCompress.Compressors.Rar.VM;

internal sealed class VMPreparedOperand
{
    internal VMOpType Type { get; set; }
    internal int Data { get; set; }
    internal int Base { get; set; }
    internal int Offset { get; set; }
}
