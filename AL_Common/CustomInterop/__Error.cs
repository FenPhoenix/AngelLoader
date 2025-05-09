﻿// From Framework source and/or .NET 8 source or something

using System;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace AL_Common;
internal static class __Error
{
    /// <summary>
    /// Given a possible fully qualified path, ensure that we have path discovery permission
    /// to that path. If we do not, return just the file name. If we know it is a directory,
    /// then don't return the directory name.
    /// </summary>
    /// <param name="path"></param>
    /// <param name="isInvalidPath"></param>
    /// <returns></returns>
    private static string GetDisplayablePath(string path, bool isInvalidPath)
    {
        if (string.IsNullOrEmpty(path))
        {
            return path;
        }

        // Is it a fully qualified path?
        bool isFullyQualified = false;
        if (path.Length < 2)
        {
            return path;
        }

        if ((path[0] == Path.DirectorySeparatorChar) && (path[1] == Path.DirectorySeparatorChar))
        {
            isFullyQualified = true;
        }
        else if (path[1] == Path.VolumeSeparatorChar)
        {
            isFullyQualified = true;
        }

        if (!isFullyQualified && !isInvalidPath)
        {
            return path;
        }

        if (isInvalidPath)
        {
            path = path[^1] == Path.DirectorySeparatorChar
                ? SR.IO_NoPermissionToDirectoryName
                : Path.GetFileName(path);
        }

        return path;
    }

    // After calling GetLastWin32Error(), it clears the last error field, so you must save the
    // HResult and pass it to this method.  This method will determine the appropriate
    // exception to throw dependent on your error, and depending on the error, insert a string
    // into the message gotten from the ResourceManager.
    internal static void WinIOError(int errorCode, string maybeFullPath)
    {
        // This doesn't have to be perfect, but is a perf optimization.
        bool isInvalidPath = errorCode
            is Interop.Errors.ERROR_INVALID_NAME
            or Interop.Errors.ERROR_BAD_PATHNAME;

        string str = GetDisplayablePath(maybeFullPath, isInvalidPath);

        switch (errorCode)
        {
            case Interop.Errors.ERROR_FILE_NOT_FOUND:
                if (str.Length == 0)
                {
                    throw new FileNotFoundException(SR.FileNotFound);
                }
                else
                {
                    throw new FileNotFoundException(string.Format(CultureInfo.CurrentCulture, SR.FileNotFound_FileName, str), str);
                }

            case Interop.Errors.ERROR_PATH_NOT_FOUND:
                if (str.Length == 0)
                {
                    throw new DirectoryNotFoundException(SR.PathNotFound_NoPathName);
                }
                else
                {
                    throw new DirectoryNotFoundException(string.Format(CultureInfo.CurrentCulture, SR.PathNotFound_Path, str));
                }

            case Interop.Errors.ERROR_ACCESS_DENIED:
                if (str.Length == 0)
                {
                    throw new UnauthorizedAccessException(SR.UnauthorizedAccess_IODenied_NoPathName);
                }
                else
                {
                    throw new UnauthorizedAccessException(string.Format(CultureInfo.CurrentCulture, SR.UnauthorizedAccess_IODenied_Path, str));
                }

            case Interop.Errors.ERROR_ALREADY_EXISTS:
                if (str.Length == 0)
                {
                    goto default;
                }
                throw new IOException(string.Format(SR.IO_AlreadyExists_Name, str), MakeHRFromErrorCode(errorCode));

            case Interop.Errors.ERROR_FILENAME_EXCED_RANGE:
                throw new PathTooLongException(SR.PathTooLong);

            case Interop.Errors.ERROR_INVALID_DRIVE:
                throw new DriveNotFoundException(string.Format(CultureInfo.CurrentCulture, SR.DriveNotFound_Drive, str));

            case Interop.Errors.ERROR_INVALID_PARAMETER:
                throw new IOException(GetMessage(errorCode), MakeHRFromErrorCode(errorCode));

            case Interop.Errors.ERROR_SHARING_VIOLATION:
                if (str.Length == 0)
                {
                    throw new IOException(SR.IO_SharingViolation_NoFileName, MakeHRFromErrorCode(errorCode));
                }
                else
                {
                    throw new IOException(string.Format(SR.IO_SharingViolation_File, str), MakeHRFromErrorCode(errorCode));
                }

            case Interop.Errors.ERROR_FILE_EXISTS:
                if (str.Length == 0)
                {
                    goto default;
                }
                throw new IOException(string.Format(CultureInfo.CurrentCulture, SR.IO_FileExists_Name, str), MakeHRFromErrorCode(errorCode));

            case Interop.Errors.ERROR_OPERATION_ABORTED:
                throw new OperationCanceledException();

            default:
                throw new IOException(GetMessage(errorCode), MakeHRFromErrorCode(errorCode));
        }
    }

    // 0x80070006 for ERROR_INVALID_HANDLE
    private static int MakeHRFromErrorCode(int errorCode)
    {
        return unchecked(((int)0x80070000) | errorCode);
    }

    // for win32 error message formatting
    private const int FORMAT_MESSAGE_IGNORE_INSERTS = 0x00000200;
    private const int FORMAT_MESSAGE_FROM_SYSTEM = 0x00001000;
    private const int FORMAT_MESSAGE_ARGUMENT_ARRAY = 0x00002000;

    [DllImport("kernel32.dll", ExactSpelling = true, CharSet = CharSet.Unicode, BestFitMapping = false)]
    private static extern int FormatMessage(
        int dwFlags,
        nint lpSource,
        int dwMessageId,
        int dwLanguageId,
        StringBuilder lpBuffer,
        int nSize,
        nint va_list_arguments);

    // Gets an error message for a Win32 error code.
    private static string GetMessage(int errorCode)
    {
        StringBuilder sb = new(512);
        int result = FormatMessage(
            FORMAT_MESSAGE_IGNORE_INSERTS |
            FORMAT_MESSAGE_FROM_SYSTEM | FORMAT_MESSAGE_ARGUMENT_ARRAY,
            0, errorCode, 0, sb, sb.Capacity, 0);
        if (result != 0)
        {
            // result is the # of characters copied to the StringBuilder on NT,
            // but on Win9x, it appears to be the number of MBCS buffer.
            // Just give up and return the String as-is...
            string s = sb.ToString();
            return s;
        }
        else
        {
            return "UnknownError_Num " + errorCode;
        }
    }
}
