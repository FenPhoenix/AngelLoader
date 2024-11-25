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
    [DllImport("Kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern bool DeviceIoControl(
        SafeFileHandle hDevice,
        IOControlCode IoControlCode,
        [In] STORAGE_PROPERTY_QUERY InBuffer,
        uint nInBufferSize,
        [Out] byte[] OutBuffer,
        uint nOutBufferSize,
        ref uint pBytesReturned,
        [In] IntPtr Overlapped
    );

    /// <summary>
    /// Repeatedly invokes InvokeIoControl with the specified input, as long as it gets return code 234 ("More data available") from the method.
    /// </summary>
    public static byte[] InvokeIoControlUnknownSize(SafeFileHandle handle, IOControlCode controlCode, STORAGE_PROPERTY_QUERY input, uint increment = 128, uint inputSizeOverride = 0)
    {
        uint returnedBytes = 0;

        uint outputLength = increment;

        uint inputSize = inputSizeOverride > 0
            ? inputSizeOverride
            : MarshalHelper.SizeOf<STORAGE_PROPERTY_QUERY>();

        do
        {
            byte[] output = new byte[outputLength];
            bool success = DeviceIoControl(handle, controlCode, input, inputSize, output, outputLength, ref returnedBytes, IntPtr.Zero);

            if (!success)
            {
                int lastError = Marshal.GetLastWin32Error();

                if (lastError == 234)
                {
                    // More data
                    outputLength += increment;
                    continue;
                }

                throw new Win32Exception(lastError, "Couldn't invoke DeviceIoControl for " + controlCode + ". LastError: " + Utils.GetWin32ErrorMessage(lastError));
            }

            // Return the result
            if (output.Length == returnedBytes)
            {
                return output;
            }

            byte[] res = new byte[returnedBytes];
            Array.Copy(output, res, (int)returnedBytes);

            return res;
        } while (true);
    }
}
