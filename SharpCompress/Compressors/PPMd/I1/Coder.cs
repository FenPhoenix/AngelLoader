#region Using

#endregion

namespace SharpCompress.Compressors.PPMd.I1;

/// <summary>
/// A simple range coder.
/// </summary>
/// <remarks>
/// Note that in most cases fields are used rather than properties for performance reasons (for example,
/// <see cref="_scale"/> is a field rather than a property).
/// </remarks>
internal class Coder
{
    private uint _low;
    private uint _code;
    private uint _range;

    public uint _lowCount;
    public uint _highCount;
    public uint _scale;
}
