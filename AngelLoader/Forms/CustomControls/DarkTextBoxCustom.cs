using System;
using System.ComponentModel;
using System.Windows.Forms;
using AL_Common;
using JetBrains.Annotations;

namespace AngelLoader.Forms.CustomControls;

public sealed class DarkTextBoxCustom : DarkTextBox
{
    [PublicAPI]
    [DefaultValue("")]
    public string DisallowedCharacters { get; set; } = "";

    /// <summary>
    /// Set to true to block TextChanged from firing unless the text really did change. Otherwise it's the
    /// default fast-and-loose behavior.
    /// </summary>
    [PublicAPI]
    [DefaultValue(true)]
    public bool StrictTextChangedEvent { get; set; } = true;

    private string _backingText = "";

    public void SetTextAndMoveCursorToEnd(string text)
    {
        Text = text;
        Focus();
        if (Text.Length > 0) SelectionStart = Text.Length;
    }

    protected override void OnKeyPress(KeyPressEventArgs e)
    {
        if (!DisallowedCharacters.IsEmpty() && DisallowedCharacters.Contains(e.KeyChar))
        {
            e.Handled = true;
        }

        base.OnKeyPress(e);
    }

    protected override void OnTextChanged(EventArgs e)
    {
        string oldBackingText = _backingText;

        if (!DisallowedCharacters.IsEmpty())
        {
            string newText = Text;
            foreach (char c in DisallowedCharacters)
            {
                newText = newText.Replace(c.ToString(), "");
            }

            // Prevents control-key combinations (Ctrl+A for example) from breaking, since they also fire
            // this event even though the text doesn't actually end up changing in that case.
            if (newText != Text)
            {
                int oldCaretPosition = SelectionStart;
                int oldTextLength = Text.Length;

                Text = newText;

                int newCaretPosition = oldCaretPosition - (oldTextLength - newText.Length);
                Select(newCaretPosition < 0 ? 0 : newCaretPosition, 0);
            }
        }

        _backingText = Text;

        // Prevents non-text key combinations from firing the TextChanged event.
        // How in the hell does "text changed" mean "key pressed but literally no text changed at all".
        // Microsoft...
        // 2021-03-09:
        // I guess I finally figured out why they did this. It's probably so that you can select a character,
        // overwrite it with the same one, and now your text is the same but you can still run things that
        // react to "text entry" (like I'm having to do with my search drop-down).
        if (!StrictTextChangedEvent || oldBackingText != Text)
        {
            base.OnTextChanged(e);
        }
    }
}
