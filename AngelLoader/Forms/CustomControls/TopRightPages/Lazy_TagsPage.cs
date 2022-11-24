using System.Windows.Forms;

namespace AngelLoader.Forms.CustomControls
{
    public partial class Lazy_TagsPage : UserControl
    {
        public Lazy_TagsPage()
        {
#if DEBUG
            InitializeComponent();
#else
            InitSlim();
#endif
        }
    }
}
