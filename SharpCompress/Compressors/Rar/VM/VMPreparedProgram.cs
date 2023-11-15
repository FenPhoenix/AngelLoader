using System.Collections.Generic;

namespace SharpCompress_7z.Compressors.Rar.VM;

internal sealed class VMPreparedProgram
{
    internal readonly List<VMPreparedCommand> Commands = new List<VMPreparedCommand>();
    internal List<VMPreparedCommand> AltCommands = new List<VMPreparedCommand>();

    public int CommandCount { get; set; }

    internal readonly List<byte> GlobalData = new List<byte>();
    internal List<byte> StaticData = new List<byte>();

    // static data contained in DB operators
    internal readonly int[] InitR = new int[7];

    internal int FilteredDataOffset { get; set; }
    internal int FilteredDataSize { get; set; }
}
