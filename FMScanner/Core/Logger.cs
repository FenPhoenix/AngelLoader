﻿using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using static AL_Common.Utils;

namespace FMScanner
{
    internal static class Logger
    {
        private static readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();

        private static void ClearLogFile(string logFile)
        {
            if (logFile.IsEmpty()) return;

            _lock.EnterWriteLock();
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
                    _lock.ExitWriteLock();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }
            }
        }

        internal static void Log(string logFile,
            string message, Exception? ex = null, bool stackTrace = false, bool methodName = true,
            [CallerMemberName] string callerMemberName = "")
        {
            if (logFile.IsEmpty()) return;

            try
            {
                _lock.EnterReadLock();
                if (File.Exists(logFile) && new FileInfo(logFile).Length > ByteSize.MB * 50) ClearLogFile(logFile);
            }
            catch (Exception logEx)
            {
                Debug.WriteLine(logEx);
            }
            finally
            {
                _lock.ExitReadLock();
            }

            try
            {
                _lock.EnterWriteLock();
                using var sw = new StreamWriter(logFile, append: true);
                var st = new StackTrace(1);
                string methodNameStr = methodName ? callerMemberName + "\r\n" : "";
                sw.WriteLine(
                    DateTime.Now.ToString(CultureInfo.InvariantCulture) + " " +
                    methodNameStr + message);
                if (stackTrace) sw.WriteLine("STACK TRACE:\r\n" + st);
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
