using System.Windows.Forms;
using AngelLoader.DataClasses;
using static AngelLoader.Misc;

namespace AngelLoader.Forms.CustomControls
{
    public sealed class CommentTabPage : Lazy_TabsBase
    {
        private Lazy_CommentPage? _page;

        public void Construct(MainForm owner)
        {
            if (_page != null) return;

            _owner = owner;
            _page = new Lazy_CommentPage
            {
                Dock = DockStyle.Fill,
                Tag = LoadType.Lazy
            };

            using (new DisableEvents(_owner))
            {
                SuspendLayout();

                Controls.Add(_page);

                ResumeLayout(false);
                PerformLayout();

                _page.CommentTextBox.Leave += _owner.CommentTextBox_Leave;
                _page.CommentTextBox.TextChanged += _owner.CommentTextBox_TextChanged;

                _constructed = true;

                UpdatePage();

                if (DarkModeEnabled) RefreshTheme();

                Localize();
            }
        }

        public void Localize()
        {
        }

        public void UpdatePage()
        {
            if (_page == null) return;
            FanMission? fm = _owner.GetMainSelectedFMOrNull();

            if (fm != null)
            {
                _page.CommentTextBox.Enabled = true;
                _page.CommentTextBox.Text = fm.Comment.FromRNEscapes();
            }
            else
            {
                _page.CommentTextBox.Text = "";
                _page.CommentTextBox.Enabled = false;
            }
        }

        internal string GetCommentBoxText()
        {
            Utils.AssertR(_page != null, nameof(_page) + " is null!");
            return _page!.CommentTextBox.Text;
        }
    }
}
