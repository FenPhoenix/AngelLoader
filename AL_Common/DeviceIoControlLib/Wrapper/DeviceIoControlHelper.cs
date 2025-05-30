using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using AL_Common.DeviceIoControlLib.Objects.Enums;
using AL_Common.DeviceIoControlLib.Objects.Storage;
using AL_Common.DeviceIoControlLib.Utilities;
using Microsoft.Win32.SafeHandles;

namespace AL_Common.DeviceIoControlLib.Wrapper;

public static class DeviceIoControlHelper
{
    // Use manual marshalling rather than UnmanagedType.AsAny for future-proofing, and also make it even more
    // manual to prevent crashing in 32-bit mode.
    [DllImport("Kernel32.dll", ExactSpelling = true, SetLastError = true, CharSet = CharSet.Unicode)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern unsafe bool DeviceIoControl(
        SafeFileHandle hDevice,
        IOControlCode dwIoControlCode,
        [In] nint lpInBuffer,
        uint nInBufferSize,
        [Out] void* lpOutBuffer,
        uint nOutBufferSize,
        out uint lpBytesReturned,
        [In] nint lpOverlapped
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
            nint inputPtr = Marshal.AllocHGlobal(Marshal.SizeOf(input));
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
                        lpOverlapped: 0);

                    if (!success)
                    {
                        int lastError = Marshal.GetLastWin32Error();

                        if (lastError == Interop.Errors.ERROR_MORE_DATA)
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
