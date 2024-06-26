﻿using System;
using AngelLoader.DataClasses;

namespace AngelLoader.Forms.CustomControls;

public sealed class CommentTabPage : Lazy_TabsBase
{
    private Lazy_CommentPage _page = null!;

    #region Public common

    public override void Construct()
    {
        if (_constructed) return;

        _page = ConstructPage<Lazy_CommentPage>();

        using (new DisableEvents(_owner))
        {
            Controls.Add(_page);

            _page.CommentTextBox.Leave += _owner.TextBoxLeave_Save;
            _page.CommentTextBox.TextChanged += CommentTextBox_TextChanged;

            FinishConstruct();
        }

        _page.Show();
    }

    public override void UpdatePage()
    {
        if (!_constructed) return;
        FanMission? fm = _owner.GetMainSelectedFMOrNull();

        if (fm != null)
        {
            _page.CommentTextBox.Enabled = true;
            // Clear to set scroll position back to top - only applies if comments are exactly the same between
            // FMs (extremely unlikely but hey)
            _page.CommentTextBox.Clear();
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
        if (_owner.EventsDisabled > 0) return;
        FanMission? fm = _owner._displayedFM;
        if (fm == null) return;

        string commentText = _page.CommentTextBox.Text;

        /*
        Converting a multiline comment to single line:
        DarkLoader copies up to the first linebreak or the 40 char mark, whichever comes first. I'm doing the
        same, but bumping the cutoff point to 100 chars, which is still plenty fast. Escaping is unbounded, but
        I measure tenths to hundredths of a millisecond even for 25,000+ character strings with nothing but
        slashes and linebreaks in them.
        */
        fm.Comment = commentText.ToRNEscapes();
        // @MEM/@PERF_TODO: Can we test if the FM comment already matches the first 100 chars and avoid an alloc?
        fm.CommentSingleLine = commentText.ToSingleLineComment();

        _owner.RefreshSelectedRowCell(Column.Comment);
    }

    #endregion
}
