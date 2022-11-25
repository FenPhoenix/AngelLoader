﻿using System;
using System.Windows.Forms;
using AngelLoader.DataClasses;
using static AngelLoader.Misc;

namespace AngelLoader.Forms.CustomControls
{
    public sealed class CommentTabPage : Lazy_TabsBase
    {
        private Lazy_CommentPage _page = null!;

        #region Public common

        public override void SetOwner(MainForm owner) => _owner = owner;

        public override void Construct()
        {
            if (_constructed) return;

            _page = new Lazy_CommentPage
            {
                Dock = DockStyle.Fill,
                Tag = LoadType.Lazy
            };

            using (new DisableEvents(_owner))
            {
                Controls.Add(_page);

                _page.CommentTextBox.Leave += CommentTextBox_Leave;
                _page.CommentTextBox.TextChanged += CommentTextBox_TextChanged;

                _constructed = true;

                UpdatePage();

                if (DarkModeEnabled) RefreshTheme();
            }
        }

        public override void Localize() { }

        public override void UpdatePage()
        {
            if (!_constructed) return;
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

        #endregion

        #region Page

        private void CommentTextBox_TextChanged(object sender, EventArgs e)
        {
            if (_owner.EventsDisabled) return;
            FanMission? fm = _owner.GetMainSelectedFMOrNull_Fast();
            if (fm == null) return;

            string commentText = _page.CommentTextBox.Text;

            // Converting a multiline comment to single line:
            // DarkLoader copies up to the first linebreak or the 40 char mark, whichever comes first.
            // I'm doing the same, but bumping the cutoff point to 100 chars, which is still plenty fast.
            // fm.Comment.ToEscapes() is unbounded, but I measure tenths to hundredths of a millisecond even for
            // 25,000+ character strings with nothing but slashes and linebreaks in them.
            fm.Comment = commentText.ToRNEscapes();
            fm.CommentSingleLine = commentText.ToSingleLineComment(100);

            _owner.RefreshMainSelectedFMRow_Fast();
        }

        private void CommentTextBox_Leave(object sender, EventArgs e)
        {
            if (_owner.EventsDisabled) return;
            Ini.WriteFullFMDataIni();
        }

        #endregion
    }
}
