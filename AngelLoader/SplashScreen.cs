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
        private static SplashScreenForm? splashScreenForm;
        private static readonly EventWaitHandle _initWaitHandle = new EventWaitHandle(false, EventResetMode.ManualReset);
        private static readonly EventWaitHandle _disposeWaitHandle = new EventWaitHandle(false, EventResetMode.ManualReset);

        private static bool _initialized;
        private static bool _closed;

        internal static void Init()
        {
            if (_initialized) return;
            _initialized = true;

            Task.Run(() =>
            {
                splashScreenForm = new SplashScreenForm(_initWaitHandle, _disposeWaitHandle);
                // We use an app context so we don't show the form right away, because we want to handle showing
                // manually for reasons of setting the theme and whatever else
                Application.Run(new ApplicationContext());
            });

            _initWaitHandle.WaitOne();
        }

        internal static void SetMessage(string message)
        {
            if (_closed) return;

            _initWaitHandle.WaitOne();

            if (splashScreenForm != null && splashScreenForm.IsHandleCreated)
            {
                splashScreenForm.Invoke(new Action(() => splashScreenForm.SetMessageText(message)));
            }
        }

        internal static void Show(VisualTheme theme)
        {
            if (_closed) return;

            _initWaitHandle.WaitOne();

            if (splashScreenForm != null && splashScreenForm.IsHandleCreated)
            {
                splashScreenForm.Invoke(new Action(() =>
                {
                    splashScreenForm.Show(theme);
                    splashScreenForm.Activate();
                }));
            }
        }

        internal static void Hide()
        {
            if (_closed) return;

            _initWaitHandle.WaitOne();

            if (splashScreenForm != null && splashScreenForm.IsHandleCreated)
            {
                splashScreenForm.Invoke(new Action(() => splashScreenForm.Hide()));
            }
        }

        internal static void Close()
        {
            if (_closed) return;
            _closed = true;

            _initWaitHandle.WaitOne();

            if (splashScreenForm != null && splashScreenForm.IsHandleCreated)
            {
                splashScreenForm.Invoke(new Action(() =>
                {
                    splashScreenForm.ProgrammaticClose();
                    _disposeWaitHandle.WaitOne();
                }));
            }
        }
    }
}
