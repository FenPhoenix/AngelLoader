using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using AngelLoader.DataClasses;
using AngelLoader.Forms;

namespace AngelLoader
{
    internal static class SplashScreen
    {
        private static SplashScreenForm? splashScreen;
        private static readonly EventWaitHandle _initWaitHandle = new EventWaitHandle(false, EventResetMode.ManualReset);
        private static readonly EventWaitHandle _disposeWaitHandle = new EventWaitHandle(false, EventResetMode.ManualReset);

        private static bool _initialized;
        private static bool _closed;

        internal static void InitSplashScreen()
        {
            if (_initialized) return;
            _initialized = true;

            Task.Run(() =>
            {
                splashScreen = new SplashScreenForm(_initWaitHandle, _disposeWaitHandle);
                // We use an app context so we don't show the form right away, because we want to handle showing
                // manually for reasons of setting the theme and whatever else
                Application.Run(new ApplicationContext());
            });

            _initWaitHandle.WaitOne();
        }

        internal static void SetSplashScreenMessage(string message)
        {
            if (_closed) return;

            _initWaitHandle.WaitOne();

            if (splashScreen != null && splashScreen.IsHandleCreated)
            {
                splashScreen.Invoke(new Action(() => splashScreen.SetMessageText(message)));
            }
        }

        internal static void HideSplashScreen()
        {
            if (_closed) return;

            _initWaitHandle.WaitOne();

            if (splashScreen != null && splashScreen.IsHandleCreated)
            {
                splashScreen.Invoke(new Action(() => splashScreen.Hide()));
            }
        }

        internal static void ShowSplashScreen(VisualTheme theme)
        {
            if (_closed) return;

            _initWaitHandle.WaitOne();

            if (splashScreen != null && splashScreen.IsHandleCreated)
            {
                splashScreen.Invoke(new Action(() => splashScreen.Show(theme)));
            }
        }

        internal static void CloseSplashScreen()
        {
            if (_closed) return;
            _closed = true;

            _initWaitHandle.WaitOne();

            if (splashScreen != null && splashScreen.IsHandleCreated)
            {
                splashScreen.Invoke(new Action(() =>
                {
                    splashScreen.Close();
                    splashScreen.Dispose();
                    _disposeWaitHandle.WaitOne();
                }));
            }
        }
    }
}
