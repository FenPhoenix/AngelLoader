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
- DataGrid apparently CAN be made fast like WinForms - [see](https://stackoverflow.com/questions/55245962/use-data-virtualization-when-binding-to-wpf-datagrid-and-support-sorting)

Cons:
- Not cross-platform
- ~Unusable DataGrid - not virtualizable (and no, "Virtual Mode" is not what we need despite it being named like it is)~
- RichTextBox is slow as ass if there's more than a trivial amount of "runs" / "blocks" or whatever
- Probably dead-end tech

### WPF with WebView2 to display rtf-to-HTML converted readmes:

Pros:
- A full-blown web browser _definitely_ covers the Win32 RichEdit control's capabilites.
- Preliminary testing shows good results, can have back-and-forward disabled, context menu disabled, can zoom/set zoom programatically, etc.

Cons:
- WebView2 is Edge, and it's subject to updates. This exposes us to a _huge_ liability in that a) a third-party can break us at any time and there's nothing we can do, and b) every user might have a different version of Edge on their system, so if there's a problem, good luck us being able to diagnose/reproduce.
- WebView2 can also be deployed in a fixed version alongside the app, which dodges the issues above, but then we're talking a _300MB+_ package. It makes Electron look positively svelte in comparison.
- Load performance acceptability is uncertain - I measured ~20-30ms to load AL's doc page, but then later it was over 100ms, not sure what happened there. The doc page is very large and with many images in comparison to what's expected of an average FM readme, so we could still be good. Need more testing. Can't test multiple readme load speed at present because I don't have a working rtf-to-html converter.

### WinUI 3:
Pros:
- Can be themed in a completely supported and dynamically-switchable way
- Has RichTextBox - fast, doesn't support WMF images but we can pre-convert
- DataGrid - is it virtualizable for high performance with large numbers of rows? **(need to confirm!)**
- Latest demo doesn't seem slow anymore? Need more thorough testing

Cons:
- Not cross-platform
- ~Horrendously laggy and in a jagged and inconsistent way which makes it even _more_ distracting and awful than WinForms, at least WinForms is _even_ in its slowness~
- Appears to dump masses of dlls in build dir - suggesting we need to package them with the app (but not sure)

### MAUI:
Pros:
- Cross-platform
- Uses WinUI 3 so has the nice RichTextBox

Cons:
- I think it uses WinUI 3 for Windows so we're right back to that
- ~I don't know much about this one. I don't even know how you would develop apps, I try to make a MAUI project and there's nothing, no designer and not even any reference documentation that would even tell me how to create a UI by hand. Ugh.~
- ~Can't get MAUI projects to work whatsoever, all references are just errors and docs say nothing about this situation and google provides no (working) answers either~
- Now there's a new problem, MAUI apps run in Visual Studio but they fail and exit immediately when run outside. Google is no help as always, giving irrelevant answers that don't apply.
- "Publish" app asks for Windows Store crap, so are we or are we not required to make MAUI apps Windows Store crap?!?!? I don't know! One would hope not?!

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
- Rich text control HTML subset may or may not cover all Win32 RichEdit control's capabilities. Basics are there, font family/size/color, bold/italic, links, tables, etc. Can't do base64 embedded images, so that's a hard requirement for an on-disk converted-file cache to hold external images.

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
- Doesn't seem to have disabled-control functionality, probably also lacking other things we might want
- We would be reliant on a wrapper that may be several versions behind Dear ImGui
