using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using AngelLoader.Common.Utility;

namespace AngelLoader.Common
{
    internal static class Logger
    {
        private static readonly ReaderWriterLockSlim Lock = new ReaderWriterLockSlim();

        internal static void ClearLogFile(string logFile = "")
        {
            if (logFile.IsEmpty()) logFile = Paths.LogFile;

            Lock.EnterWriteLock();
            try
            {
                File.Delete(logFile);
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
            }
            finally
            {
                Lock.ExitWriteLock();
            }
        }

        internal static void Log(string message, Exception ex = null, bool stackTrace = false, bool methodName = true,
            [CallerMemberName] string callerMemberName = "")
        {
            Lock.EnterReadLock();
            try
            {
                if (new FileInfo(Paths.LogFile).Length > ByteSize.MB * 50) ClearLogFile();
            }
            catch (Exception ex1)
            {
                Trace.WriteLine(ex1);
            }
            Lock.ExitReadLock();

            Lock.EnterWriteLock();
            try
            {
                using (var sw = new StreamWriter(Paths.LogFile, append: true))
                {
                    var st = new StackTrace(1);
                    var methodNameStr = methodName ? callerMemberName + "\r\n" : "";
                    sw.WriteLine(
                        DateTime.Now.ToString(CultureInfo.InvariantCulture) + " " +
                        methodNameStr + message);
                    if (stackTrace) sw.WriteLine("STACK TRACE:\r\n" + st);
                    if (ex != null) sw.WriteLine("EXCEPTION:\r\n" + ex);
                    sw.WriteLine();
                }
            }
            catch (Exception logEx)
            {
                Trace.WriteLine(logEx);
            }
            finally
            {
                Lock.ExitWriteLock();
            }
        }
    }
}
