using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using JetBrains.Annotations;
using static AL_Common.Common;

namespace AL_Common
{
    public static class Logger
    {
        private static readonly ReaderWriterLockSlim _lock = new();

        private static string _logFile = "";
        public static void SetLogFile(string logFile) => _logFile = logFile;

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

        private static void ClearLogFile()
        {
            if (_logFile.IsEmpty()) return;

            try
            {
                _lock.EnterWriteLock();
                File.Delete(_logFile);
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
        public static void LogStartup(string message)
        {
            if (_logFile.IsEmpty()) return;

            try
            {
                using var sw = new StreamWriter(_logFile);
                sw.WriteLine(GetDateTimeStringFast() + " " + message + "\r\n");
            }
            catch (Exception logEx)
            {
                Debug.WriteLine(logEx);
            }
        }

        // TODO: Consider how to make the log not clear every startup and still have it be feasible for people to "post their log"
        public static void Log(
            string message = "",
            Exception? ex = null,
            bool stackTrace = false,
            [CallerMemberName] string callerMemberName = "")
        {
            if (_logFile.IsEmpty()) return;

            try
            {
                _lock.EnterReadLock();
                if (File.Exists(_logFile) && new FileInfo(_logFile).Length > ByteSize.MB * 50) ClearLogFile();
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

                using var sw = new StreamWriter(_logFile, append: true);

                string methodNameStr = callerMemberName + "\r\n";
                sw.WriteLine(GetDateTimeStringFast() + " " + methodNameStr + message);
                if (ex != null) sw.WriteLine("EXCEPTION:\r\n" + ex);
                if (stackTrace) sw.WriteLine("STACK TRACE:\r\n" + new StackTrace(1));
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
