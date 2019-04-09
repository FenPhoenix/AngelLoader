using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading;

namespace AngelLoader.Common
{
    internal static class Logger
    {
        private static readonly ReaderWriterLockSlim Lock = new ReaderWriterLockSlim();
        internal static void Log(string message, bool stackTrace = true, Exception ex = null)
        {
            Lock.EnterWriteLock();
            try
            {
                using (var sw = new StreamWriter(Paths.LogFile, append: true))
                {
                    var sf = new StackFrame(1);
                    sw.WriteLine(
                        DateTime.Now.ToString(CultureInfo.InvariantCulture) + " " +
                        sf.GetMethod().Name + "\r\n" + message);
                    if (stackTrace) sw.WriteLine("STACK TRACE:\r\n" + sf.ToString());
                    if (ex != null) sw.WriteLine("EXCEPTION:\r\n" + ex);
                }
            }
            catch (Exception logEx)
            {
                Trace.WriteLine(logEx);
                // nothing else we can really do
            }
            finally
            {
                Lock.ExitWriteLock();
            }
        }
    }
}
