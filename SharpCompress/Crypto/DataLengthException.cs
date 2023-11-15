namespace SharpCompress.Crypto;

public sealed class DataLengthException : CryptoException
{
    /**
    * base constructor.
    */

#if false
    public DataLengthException() { }

    /**
     * create a DataLengthException with the given message.
     *
     * @param message the message to be carried with the exception.
     */

    public DataLengthException(string message, Exception exception)
        : base(message, exception) { }
#endif

    public DataLengthException(string message)
        : base(message) { }

}
