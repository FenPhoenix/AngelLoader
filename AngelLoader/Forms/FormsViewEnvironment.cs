using System.Windows.Forms;

namespace AngelLoader.Forms
{
    public sealed class FormsViewEnvironment : IViewEnvironment
    {
        internal static bool ViewCreated;
        private static MainForm? _view;
        internal static MainForm ViewInternal
        {
            get
            {
                if (_view == null)
                {
                    _view = new MainForm();
                    ViewCreated = true;
                }
                return _view;
            }
        }

        public IView GetView() => ViewInternal;

        public IDialogs GetDialogs() => new Dialogs();

        public void ApplicationExit() => Application.Exit();
    }
}
