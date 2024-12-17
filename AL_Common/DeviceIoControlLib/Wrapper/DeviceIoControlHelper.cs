using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using AL_Common.DeviceIoControlLib.Objects.Enums;
using AL_Common.DeviceIoControlLib.Objects.Storage;
using AL_Common.DeviceIoControlLib.Utilities;
using Microsoft.Win32.SafeHandles;

namespace AL_Common.DeviceIoControlLib.Wrapper;

public static partial class DeviceIoControlHelper
{
    // Use manual marshalling rather than UnmanagedType.AsAny for future-proofing, and also make it even more
    // manual to prevent crashing in 32-bit mode (although we don't currently do 32-bit in .NET modern).
    [LibraryImport("Kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static unsafe partial bool DeviceIoControl(
        SafeFileHandle hDevice,
        IOControlCode dwIoControlCode,
        IntPtr lpInBuffer,
        uint nInBufferSize,
        void* lpOutBuffer,
        uint nOutBufferSize,
        out uint lpBytesReturned,
        IntPtr lpOverlapped
    );

    /// <summary>
    /// Repeatedly invokes InvokeIoControl with the specified input, as long as it gets return code 234 ("More data available") from the method.
    /// </summary>
    public static unsafe byte[] InvokeIoControlUnknownSize(
        SafeFileHandle handle,
        IOControlCode controlCode,
        STORAGE_PROPERTY_QUERY input,
        uint increment = 128,
        uint inputSizeOverride = 0)
    {
        uint outputLength = increment;

        uint inputSize = inputSizeOverride > 0
            ? inputSizeOverride
            : MarshalHelper.SizeOf<STORAGE_PROPERTY_QUERY>();

        do
        {
            IntPtr inputPtr = Marshal.AllocHGlobal(Marshal.SizeOf(input));
            try
            {
                Marshal.StructureToPtr(input, inputPtr, true);

                byte[] output = new byte[outputLength];
                fixed (byte* outputPtr = output)
                {
                    bool success = DeviceIoControl(
                        hDevice: handle,
                        dwIoControlCode: controlCode,
                        lpInBuffer: inputPtr,
                        nInBufferSize: inputSize,
                        lpOutBuffer: outputPtr,
                        nOutBufferSize: outputLength,
                        lpBytesReturned: out uint returnedBytes,
                        lpOverlapped: IntPtr.Zero);

                    if (!success)
                    {
                        int lastError = Marshal.GetLastWin32Error();

                        if (lastError == 234)
                        {
                            // More data
                            outputLength += increment;
                            continue;
                        }

                        throw new Win32Exception(lastError,
                            "Couldn't invoke DeviceIoControl for " + controlCode + ". LastError: " +
                            Utils.GetWin32ErrorMessage(lastError));
                    }

                    // Return the result
                    if (output.Length == returnedBytes)
                    {
                        return output;
                    }

                    byte[] res = new byte[returnedBytes];
                    Array.Copy(output, res, (int)returnedBytes);

                    return res;
                }
            }
            finally
            {
                Marshal.FreeHGlobal(inputPtr);
            }
        } while (true);
    }
}
