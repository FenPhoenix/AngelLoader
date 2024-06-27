using System;

// From Framework 4.8

namespace AL_Common;

// NumberBuffer is a partial wrapper around a stack pointer that maps on to
// the native NUMBER struct so that it can be passed to native directly. It
// must be initialized with a stack Byte * of size NumberBufferBytes.
// For performance, this structure should attempt to be completely inlined.
// 
// It should always be initialized like so:
//
// Byte * numberBufferBytes = stackalloc Byte[NumberBuffer.NumberBufferBytes];
// NumberBuffer number = new NumberBuffer(numberBufferBytes);
//
// For performance, when working on the buffer in managed we use the values in this
// structure, except for the digits, and pack those values into the byte buffer
// if called out to managed.
internal unsafe struct NumberBuffer
{
    // Enough space for NumberMaxDigit characters plus null and 3 32 bit integers and a pointer
    public static readonly int NumberBufferBytes = 12 + ((TryParseSpan.NumberMaxDigits + 1) * 2) + IntPtr.Size;

    private byte* baseAddress;
    public char* digits;
    public int precision;
    public int scale;
    public bool sign;

    public NumberBuffer(byte* stackBuffer)
    {
        baseAddress = stackBuffer;
        digits = (((char*)stackBuffer) + 6);
        precision = 0;
        scale = 0;
        sign = false;
    }

    public byte* PackForNative()
    {
        int* baseInteger = (int*)baseAddress;
        baseInteger[0] = precision;
        baseInteger[1] = scale;
        baseInteger[2] = sign ? 1 : 0;
        return baseAddress;
    }
}
