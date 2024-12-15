namespace AL_Common;

public static partial class Common
{
    // It's supposed to always be "{\rtf1", but some files have no number or some other number...
    // RichTextBox also only checks for "{\rtf", so we're fine here.
    public static readonly byte[] RTFHeaderBytes = @"{\rtf"u8.ToArray();

    public static readonly byte[] MAPPARAM = "MAPPARAM"u8.ToArray();

    // Perf, for passing to Split(), Trim() etc. so we don't allocate all the time
    public static readonly string[] SA_CRLF = { "\r\n" };
    public static readonly char[] CA_Comma = { ',' };
    public static readonly char[] CA_Colon = { ':' };
    public static readonly char[] CA_Semicolon = { ';' };
    public static readonly char[] CA_CommaSemicolon = { ',', ';' };
    public static readonly char[] CA_CommaSpace = { ',', ' ' };
    public static readonly char[] CA_Backslash = { '\\' };
    public static readonly char[] CA_BS_FS = { '\\', '/' };
    public static readonly char[] CA_Plus = { '+' };
    public static readonly char[] CA_DoubleQuote = { '\"' };
}
