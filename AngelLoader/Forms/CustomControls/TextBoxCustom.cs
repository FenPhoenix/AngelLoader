using System;
using System.ComponentModel;
using System.Windows.Forms;
using AL_Common;

namespace AngelLoader.Forms.CustomControls
{
    // Also lifted straight from Autovid but with a couple improvements
    public sealed class TextBoxCustom : DarkTextBox
    {
        [Browsable(true)] public string DisallowedCharacters { get; set; } = "";

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
                foreach (char c in DisallowedCharacters) newText = newText.Replace(c.ToString(), "");

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
            if (oldBackingText != Text) base.OnTextChanged(e);
        }
    }
}
