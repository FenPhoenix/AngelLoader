using System.Collections.Generic;

namespace SharpCompress.Compressors.Rar.VM;

internal sealed class VMPreparedProgram
{
    internal readonly List<VMPreparedCommand> Commands = new List<VMPreparedCommand>();
    internal List<VMPreparedCommand> AltCommands = new List<VMPreparedCommand>();

    public int CommandCount;

    internal readonly List<byte> GlobalData = new List<byte>();
    internal List<byte> StaticData = new List<byte>();

    // static data contained in DB operators
    internal readonly int[] InitR = new int[7];

    internal int FilteredDataOffset;
    internal int FilteredDataSize;
}
