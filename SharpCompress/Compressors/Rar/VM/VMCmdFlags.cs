namespace SharpCompress.Compressors.Rar.VM;

internal static class VMCmdFlags
{
    internal const byte VMCF_OP0 = 0;
    internal const byte VMCF_OP1 = 1;
    internal const byte VMCF_OP2 = 2;
    internal const byte VMCF_OPMASK = 3;
    internal const byte VMCF_BYTEMODE = 4;
    internal const byte VMCF_JUMP = 8;
    internal const byte VMCF_PROC = 16;
    internal const byte VMCF_USEFLAGS = 32;
    internal const byte VMCF_CHFLAGS = 64;

    internal static readonly byte[] VM_CmdFlags =
    {
        VMCF_OP2 | VMCF_BYTEMODE,
        VMCF_OP2 | VMCF_BYTEMODE | VMCF_CHFLAGS,
        VMCF_OP2 | VMCF_BYTEMODE | VMCF_CHFLAGS,
        VMCF_OP2 | VMCF_BYTEMODE | VMCF_CHFLAGS,
        VMCF_OP1 | VMCF_JUMP | VMCF_USEFLAGS,
        VMCF_OP1 | VMCF_JUMP | VMCF_USEFLAGS,
        VMCF_OP1 | VMCF_BYTEMODE | VMCF_CHFLAGS,
        VMCF_OP1 | VMCF_BYTEMODE | VMCF_CHFLAGS,
        VMCF_OP1 | VMCF_JUMP,
        VMCF_OP2 | VMCF_BYTEMODE | VMCF_CHFLAGS,
        VMCF_OP2 | VMCF_BYTEMODE | VMCF_CHFLAGS,
        VMCF_OP2 | VMCF_BYTEMODE | VMCF_CHFLAGS,
        VMCF_OP2 | VMCF_BYTEMODE | VMCF_CHFLAGS,
        VMCF_OP1 | VMCF_JUMP | VMCF_USEFLAGS,
        VMCF_OP1 | VMCF_JUMP | VMCF_USEFLAGS,
        VMCF_OP1 | VMCF_JUMP | VMCF_USEFLAGS,
        VMCF_OP1 | VMCF_JUMP | VMCF_USEFLAGS,
        VMCF_OP1 | VMCF_JUMP | VMCF_USEFLAGS,
        VMCF_OP1 | VMCF_JUMP | VMCF_USEFLAGS,
        VMCF_OP1,
        VMCF_OP1,
        VMCF_OP1 | VMCF_PROC,
        VMCF_OP0 | VMCF_PROC,
        VMCF_OP1 | VMCF_BYTEMODE,
        VMCF_OP2 | VMCF_BYTEMODE | VMCF_CHFLAGS,
        VMCF_OP2 | VMCF_BYTEMODE | VMCF_CHFLAGS,
        VMCF_OP2 | VMCF_BYTEMODE | VMCF_CHFLAGS,
        VMCF_OP1 | VMCF_BYTEMODE | VMCF_CHFLAGS,
        VMCF_OP0,
        VMCF_OP0,
        VMCF_OP0 | VMCF_USEFLAGS,
        VMCF_OP0 | VMCF_CHFLAGS,
        VMCF_OP2,
        VMCF_OP2,
        VMCF_OP2 | VMCF_BYTEMODE,
        VMCF_OP2 | VMCF_BYTEMODE,
        VMCF_OP2 | VMCF_BYTEMODE,
        VMCF_OP2 | VMCF_BYTEMODE | VMCF_USEFLAGS | VMCF_CHFLAGS,
        VMCF_OP2 | VMCF_BYTEMODE | VMCF_USEFLAGS | VMCF_CHFLAGS,
        VMCF_OP0
    };
}
