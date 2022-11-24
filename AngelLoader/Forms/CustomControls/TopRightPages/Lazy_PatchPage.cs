using System.Windows.Forms;

namespace AngelLoader.Forms.CustomControls
{
    public partial class Lazy_PatchPage : UserControl
    {
        public Lazy_PatchPage()
        {
#if DEBUG
            InitializeComponent();
#else
            InitSlim();
#endif
        }
    }
}
