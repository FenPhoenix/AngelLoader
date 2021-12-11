### Windows Forms (currently used!):
Pros:
- Fast startup
- Virtualizable (read: usable) DataGridView
- Has RichTextBox (wrapped Win32 one)
- Can be themed, but with great difficulty

Cons:
- Not cross-platform
- High DPI not fully supported (read: not usable)
- RichTextBox leaks memory
- Theming relies on a ton of jank and also hooks, which cause friction for creating new controls and may not be future-proof
- Looks old, is laggy, can flicker, lacks modern niceties, and is generally clunky and not smooth

### WPF:
Pros:
- Fast startup
- Fully supports high DPI
- Internally implemented RichTextBox (ie. _not_ a wrapper around the Win32 one) that doesn't leak
- Can be themed, and in a better and more supported way than Windows Forms

Cons:
- Not cross-platform
- Unusable DataGrid - not virtualizable (and no, "Virtual Mode" is not what we need despite it being named like it is)
- Probably dead-end tech

### WinUI 3:
Pros:
- Can be themed in a completely supported and dynamically-switchable way
- Has RichTextBox
- DataGrid - is it virtualizable for high performance with large numbers of rows? **(need to confirm!)**

Cons:
- Not cross-platform
- Horrendously laggy and in a jagged and inconsistent way which makes it even _more_ distracting and awful than WinForms, at least WinForms is _even_ in its slowness

### MAUI:
Pros:
- Cross-platform

Cons:
- I think it uses WinUI 3 for Windows so we're right back to that
- I don't know much about this one. I don't even know how you would develop apps, I try to make a MAUI project and there's nothing, no designer and not even any reference documentation that would even tell me how to create a UI by hand. Ugh.

### Avalonia
Pros:
- Cross-platform
- Markets itself as "spiritual successor" to WPF, so it could be like WPF but not dead-end
- Fully themeable

Cons:
- Horrendously slow startup time on the order of multiple seconds for a simple blank window
- Still at an early version and doesn't have a rich text box of any description
- Bloats up AngelLoader's distribution size

### Qt with .NET wrapper
Pros:
- Cross-platform
- The Qt UI seems fast and responsive from trying out a few apps
- No RTF-load-capable RichTextBox as such, but there is a "rich text" box that I can probably use the WPF RTF reader to fill out instead of its normal FlowDocument(?)

Cons:
- License is dumb and annoying and I can't include its source without making my code GPL but I can include .dlls without doing so
- Requires using a proprietary IDE that's like a dozen GB in size and a steep learning curve
- Bloats up AngelLoader's distribution size

### Electron with .NET wrapper:
Pros:
- Cross-platform
- Potential for really nice looking UI I guess

Cons:
- Bloats up distribution size by >100MB
- How is startup time?

### Dear ImGui with .NET wrapper:
Pros:
- Sane way to do GUI finally
- No bloat whatsoever in the UI lib itself
- As fast as it gets for both startup and use
- Has most of what we need
- Could presumably be made cross-platform with appropriate render backends

Cons:
- Still bloats up AngelLoader's distribution size due to the need to carry actual renderer dlls with it
- No rich text box capable of handling what we need - we would have to implement it ourselves
- We would be reliant on a wrapper that may be several versions behind Dear ImGui
