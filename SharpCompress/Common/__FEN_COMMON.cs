namespace SharpCompress.Common;

internal static class FEN_COMMON
{
    /*
    @SharpCompress: Crappy static array for now, until we can figure out how to pass it in for the batch scan
    
    @SharpCompress: There's plenty of performance optimization we could still do here.
    Lots of byte[] allocations, stream recreations, etc. However, we're already pretty fast, so it's not urgent.

    @SharpCompress: Also make sure we're not reading uncached FileStream.Length multiple times.
    */
    internal static readonly byte[] Byte1 = new byte[1];
}
