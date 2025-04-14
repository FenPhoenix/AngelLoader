using System;
using System.Runtime.InteropServices;

namespace AL_Common.DeviceIoControlLib.Utilities;

public sealed class UnmanagedMemory : IDisposable
{
    public nint Handle { get; }

    public UnmanagedMemory(int size)
    {
        Handle = Marshal.AllocHGlobal(size);
    }

    public static implicit operator nint(UnmanagedMemory mem)
    {
        return mem.Handle;
    }

    public void Dispose()
    {
        Marshal.FreeHGlobal(this);
    }
}
