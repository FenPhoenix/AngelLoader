//#define logEnabled
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;

namespace FMScanner
{
    internal static class Logger
    {
        private static readonly ReaderWriterLockSlim Lock = new ReaderWriterLockSlim();

        private static void ClearLogFile(string logFile)
        {
            Lock.EnterWriteLock();
            try
            {
                File.Delete(logFile);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
            finally
            {
                try
                {
                    Lock.ExitWriteLock();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }
            }
        }

        internal static void Log(string logFile,
            string message, Exception ex = null, bool stackTrace = false, bool methodName = true,
            [CallerMemberName] string callerMemberName = "")
        {
#if !logEnabled
            return;
#endif

            if (logFile.IsEmpty()) return;

            try
            {
                Lock.EnterReadLock();
                if (File.Exists(logFile) && new FileInfo(logFile).Length > ByteSize.MB * 50) ClearLogFile(logFile);
            }
            catch (Exception logEx)
            {
                Debug.WriteLine(logEx);
            }
            finally
            {
                Lock.ExitReadLock();
            }

            try
            {
                Lock.EnterWriteLock();
                using (var sw = new StreamWriter(logFile, append: true))
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
                Debug.WriteLine(logEx);
            }
            finally
            {
                try
                {
                    Lock.ExitWriteLock();
                }
                catch (Exception logEx)
                {
                    Debug.WriteLine(logEx);
                }
            }
        }
    }
}
