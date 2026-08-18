/*
 * MIT License
 * 
 * Copyright (c) 2024-2026 Brian Tobin
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
*/

using System;
using System.Globalization;
using ReasonableRTF.Enums;

namespace ReasonableRTF.Models;

/// <summary>
/// The conversion result.
/// If an error occurred, <see cref="Error"/> will be set to the error code, and <see cref="Exception"/> might be set to an <see cref="System.Exception"/>.
/// If the conversion was successful, <see cref="Error"/> will be set to <see cref="RtfError.OK"/> and <see cref="Exception"/> will be <see langword="null"/>.
/// </summary>
public readonly struct RtfResult
{
	/// <summary>
	/// Initializes the <see cref="RtfResult"/>.
	/// </summary>
	/// <param name="error">The <see cref="RtfError"/>.</param>
	/// <param name="bytePositionOfError">The Position where the <paramref name="error"/> or <paramref name="exception"/> occured.</param>
	/// <param name="exception">The <see cref="System.Exception"/>, which occured.</param>
	internal RtfResult(RtfError error, int bytePositionOfError, Exception? exception)
    {
        Text = "";
        Error = error;
        BytePositionOfError = bytePositionOfError;
        Exception = exception;
    }

	/// <summary>
	/// Initializes the <see cref="RtfResult"/>.
	/// </summary>
	/// <param name="text">The extracted Text.</param>
	internal RtfResult(string text)
    {
        Text = text;
        Error = RtfError.OK;
        BytePositionOfError = -1;
        Exception = null;
    }

    /// <summary>
    /// The converted plain text.
    /// </summary>
    public string Text { get; }

    /// <summary>
    /// The error code.
    /// </summary>
    public RtfError Error { get; }

    /// <summary>
    /// The approximate position in the data stream where the error occurred, or -1 if no error.
    /// </summary>
    public int BytePositionOfError { get; }

    /// <summary>
    /// The caught exception, or <see langword="null"/> if no exception occurred.
    /// </summary>
    public Exception? Exception { get; }

	/// <summary>
	/// Gets the <see cref="RtfResult"/> as a <see cref="string"/>.
	/// If the conversion was not successful, the error message will be included.
	/// </summary>
	/// <returns>Returns the <see cref="RtfResult"/> as <see cref="string"/>.</returns>
	public override string ToString()
    {
        string error = Error == RtfError.OK ? "Success" : "Error: " + Error;

        string errorDescription = Error == RtfError.OK
            ? ""
            : "Error description: " + Error switch
            {
                RtfError.NotAnRtfFile => "The file did not have a valid rtf header.",
                RtfError.StackUnderflow => "Unmatched '}'.",
                RtfError.UnmatchedBrace => "Unmatched '{'.",
                RtfError.UnexpectedEndOfFile => "End of file was unexpectedly encountered while parsing.",
                RtfError.KeywordTooLong => "A keyword longer than 32 characters was encountered.",
                RtfError.ParameterOutOfRange => "A keyword parameter was outside the range of -2147483648 to 2147483647, or was longer than 10 characters.",
                RtfError.AbortedForSafety => "The rtf was malformed in such a way that it might have been unsafe to continue parsing it (infinite loops, stack overflows, etc.)",
                _ => "An unexpected error occurred.",
            } + Environment.NewLine;

        string lastPosition =
            Error == RtfError.OK
                ? ""
                : "Byte position of error (approximate): " + BytePositionOfError.ToString(CultureInfo.InvariantCulture) + Environment.NewLine;

        return "RTF to plaintext conversion result:" + Environment.NewLine +
               "-----------------------------------" + Environment.NewLine +
               error + Environment.NewLine +
               errorDescription +
               lastPosition +
               "Exception: " + (Exception != null ? Environment.NewLine + Exception : "none");
    }
}
