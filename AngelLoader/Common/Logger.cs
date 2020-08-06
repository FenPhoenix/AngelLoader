using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using JetBrains.Annotations;
using static AngelLoader.Misc;

namespace AngelLoader
{
    internal static class Logger
    {
        private static readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();

        #region Interop

        [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
        private struct SYSTEMTIME
        {
            internal ushort wYear;
            internal ushort wMonth;
            internal ushort wDayOfWeek;
            internal ushort wDay;
            internal ushort wHour;
            internal ushort wMinute;
            internal ushort wSecond;
            internal ushort wMilliseconds;
        }

        [DllImport("kernel32.dll")]
        private static extern void GetLocalTime(ref SYSTEMTIME systemTime);

        // For logging purposes: It takes an entire 5ms to get one DateTime.Now, but I don't really need hardcore
        // accuracy in logging dates, they're really just there for vague temporality. Because we log on startup,
        // this claws back some startup time.
        private static string GetDateTimeStringFast()
        {
            var dt = new SYSTEMTIME();
            GetLocalTime(ref dt);
            return dt.wYear + "/" + dt.wMonth + "/" + dt.wDay + " " +
                   dt.wHour + ":" + dt.wMinute + ":" + dt.wSecond;
        }

        #endregion

        /// <summary>
        /// A faster version without locking for running on startup.
        /// </summary>
        /// <param name="logFile"></param>
        internal static void ClearLogFileStartup(string logFile = "")
        {
            if (logFile.IsEmpty()) logFile = Paths.LogFile;

            try
            {
                File.Delete(logFile);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        private static void ClearLogFile(string logFile = "")
        {
            if (logFile.IsEmpty()) logFile = Paths.LogFile;

            try
            {
                _lock.EnterWriteLock();
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
                    _lock.ExitWriteLock();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }
            }
        }

        /// <summary>
        /// A faster version without locking or unnecessary options for running on startup.
        /// </summary>
        /// <param name="message"></param>
        internal static void LogStartup(string message)
        {
            try
            {
                using var sw = new StreamWriter(Paths.LogFile, append: false);
                sw.WriteLine(GetDateTimeStringFast() + " " + message + "\r\n");
            }
            catch (Exception logEx)
            {
                Debug.WriteLine(logEx);
            }
        }

        // TODO: Consider how to make the log not clear every startup and still have it be feasible for people to "post their log"
        internal static void Log(
            string message = "",
            Exception? ex = null,
            bool stackTrace = false,
            bool methodName = true,
            [CallerMemberName] string callerMemberName = "")
        {
            try
            {
                _lock.EnterReadLock();
                if (File.Exists(Paths.LogFile) && new FileInfo(Paths.LogFile).Length > ByteSize.MB * 50) ClearLogFile();
            }
            catch (Exception ex1)
            {
                Debug.WriteLine(ex1);
            }
            finally
            {
                try
                {
                    _lock.ExitReadLock();
                }
                catch (Exception logEx)
                {
                    Debug.WriteLine(logEx);
                }
            }

            try
            {
                _lock.EnterWriteLock();

                using var sw = new StreamWriter(Paths.LogFile, append: true);

                string methodNameStr = methodName ? callerMemberName + "\r\n" : "";
                sw.WriteLine(GetDateTimeStringFast() + " " + methodNameStr + message);
                if (stackTrace) sw.WriteLine("STACK TRACE:\r\n" + new StackTrace(1));
                if (ex != null) sw.WriteLine("EXCEPTION:\r\n" + ex);
                sw.WriteLine();
            }
            catch (Exception logEx)
            {
                Debug.WriteLine(logEx);
            }
            finally
            {
                try
                {
                    _lock.ExitWriteLock();
                }
                catch (Exception logEx)
                {
                    Debug.WriteLine(logEx);
                }
            }
        }
    }
}
