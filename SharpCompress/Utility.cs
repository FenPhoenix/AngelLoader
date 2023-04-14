namespace SharpCompress;

internal static class Utility
{
    /// <summary>
    /// Performs an unsigned bitwise right shift with the specified number
    /// </summary>
    /// <param name="number">Number to operate on</param>
    /// <param name="bits">Amount of bits to shift</param>
    /// <returns>The resulting number from the shift operation</returns>
    internal static int URShift(int number, int bits)
    {
        if (number >= 0)
        {
            return number >> bits;
        }
        return (number >> bits) + (2 << ~bits);
    }

    /// <summary>
    /// Performs an unsigned bitwise right shift with the specified number
    /// </summary>
    /// <param name="number">Number to operate on</param>
    /// <param name="bits">Amount of bits to shift</param>
    /// <returns>The resulting number from the shift operation</returns>
    internal static long URShift(long number, int bits)
    {
        if (number >= 0)
        {
            return number >> bits;
        }
        return (number >> bits) + (2L << ~bits);
    }
}
