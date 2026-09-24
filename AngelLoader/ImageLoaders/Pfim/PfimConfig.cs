namespace Pfim
{
    public sealed class PfimConfig
    {
        public PfimConfig(
            int bufferSize = 0x8000,
            bool applyColorMap = true)
        {
            BufferSize = bufferSize;
            ApplyColorMap = applyColorMap;
        }

        public bool ApplyColorMap { get; }

        public int BufferSize { get; }
    }
}
