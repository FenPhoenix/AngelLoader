using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;

namespace AL_Common.DeviceIoControlLib.Utilities;

internal static class Utils
{
    public static string GetWin32ErrorMessage(int errorCode)
    {
        return new Win32Exception(errorCode).Message;
    }

    public static T ByteArrayToStruct<T>(byte[] data, int index) where T : struct
    {
        using UnmanagedMemory mem = new(data.Length - index);
        Marshal.Copy(data, index, mem.Handle, data.Length - index);
        return mem.Handle.ToStructure<T>();
    }

    public static string ReadNullTerminatedAsciiString(byte[] br, int index)
    {
        byte[] nameBytes = br;
        for (int i = index; i < nameBytes.Length; i++)
        {
            if (nameBytes[i] == 0) // \0
            {
                return Encoding.ASCII.GetString(nameBytes, index, i - index);
            }
        }

        return string.Empty;
    }
}
