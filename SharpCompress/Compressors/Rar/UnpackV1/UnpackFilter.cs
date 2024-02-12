using SharpCompress.Compressors.Rar.VM;

namespace SharpCompress.Compressors.Rar.UnpackV1;

internal sealed class UnpackFilter
{
    public byte Type;
    public byte Channels;

    internal UnpackFilter() => Program = new VMPreparedProgram();

    internal int BlockStart;

    internal int BlockLength;

    internal int ExecCount;

    internal bool NextWindow;

    // position of parent filter in Filters array used as prototype for filter
    // in PrgStack array. Not defined for filters in Filters array.
    internal int ParentFilter;

    internal VMPreparedProgram Program;
}
