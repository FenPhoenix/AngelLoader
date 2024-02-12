namespace SharpCompress.Compressors.Rar.VM;

internal sealed class VMPreparedCommand
{
    internal VMPreparedCommand()
    {
        Op1 = new VMPreparedOperand();
        Op2 = new VMPreparedOperand();
    }

    internal VMCommands OpCode;
    internal bool IsByteMode;
    internal readonly VMPreparedOperand Op1;

    internal readonly VMPreparedOperand Op2;
}
