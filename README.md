<h1 align="center">
AngelLoader

## This is the modern .NET version. It is believed to work fine (but doesn't receive much field testing), and you can compile and use it if you're interested in comparing.

There are no new features compared to the standard .NET Framework version, and the code is almost entirely the same, differing only to work around breaking changes or to take advantage of higher-performance language features.

### The good:
- General performance increase - scans get faster, anything related to zip files gets faster (installing etc). 7z stuff does _not_ get faster because it's handled by the external native 7z.exe.
- Potential to properly support high DPI, due to WinForms receiving ongoing development in that regard on modern .NET (but this is not implemented currently).
- Potential to use SIMD to really crank performance in bottlenecky areas (but again, not implemented).

### The bad:
- **_Horrendous_** cold startup time - this is endemic to all WinForms apps on modern .NET. It takes several seconds merely just to get to the splash screen. Personally, I find this to be an unacceptable user experience. Your tolerance for it may vary. Note that cold startup doesn't just happen on a fresh boot; it happens whenever Windows decides to invalidate whatever cache is keeping it fast. So every time you go to start the app you wince in anticipation of it maybe being cold this time. Like I said, truly awful UX.
- This version uses SpanExtensions.Net which (as of v1.3.0) contains a potential stack overflow risk from recursion if asked to split a string into too many parts. Unlikely to be hit in practice as you'd need a _lot_ of parts, but meh, just a heads up.

### The mediocre:
- The UI does not get any faster, unfortunately, and that's probably the place you would most notice a performance difference if there was one.
- While the performance increases appear large in the profiler, they're not that noticeable in actual use, at least not to me.

If the cold startup time were solved (either directly or through finishing of WinForms AOT support (assuming that would fix it)), I would just tweak SpanExtensions.Net myself and switch AngelLoader to modern .NET. As it stands, though, I don't find the moderate general perf increase to be an acceptable trade for the infuriating, decades-regressed cold startup performance. Seriously I'm pretty sure my 386 started apps faster. 🤷‍♂️

<hr/>

</h1>
<p align="center"><img src="https://github.com/FenPhoenix/AngelLoader/blob/master/docs/images/main_window_v194_900w.png" /></p>

<hr>
<h4 align="center">
Thanks to JetBrains for providing their tools to AngelLoader under their Open Source License.
</h4>
<p align="center">
<a href="https://jb.gg/OpenSourceSupport"><img src="https://resources.jetbrains.com/storage/products/company/brand/logos/jb_beam.svg" width="128" height="128"/></a>
</p>

## Description
AngelLoader is a standalone fan mission loader for Thief 1, Thief 2, Thief 3, System Shock 2, and The Dark Mod. Unlike FMSel, which requires a separate copy for each game, AngelLoader allows you to manage and play all your FMs in one place. The interface is inspired by DarkLoader (by Björn Henke and Tom N. Harris) and NewDarkLoader (by Robin Collier).

## Features
- Manage all your FMs in one place
- Automatically detects metadata for every FM: title, author, game, release date, etc. No need to type it in!
- Select multiple FMs at a time to quickly mark them all finished, install them in bulk, etc.
- Search and filter your FM collection by many criteria
- Organize your games by tab or treat them as filters
- Disable visual or other mods on a per-FM basis with a simple visual list (no more typing an arcane string into a textbox!)
- Choose which language to play with on a per-FM basis
- Import your FM information from DarkLoader, NewDarkLoader, or FMSel
- Automatically fixes common problems, such as:
  - Non-16-bit audio causing static
  - Custom palettes not being applied properly to FMs (Ranstall Keep etc.)
  - Local values left in global config files by NewDark (character_detail)
- Option to automatically use old mantling for OldDark missions
- Supports light and dark themes
- Plays nice with other loaders: AngelLoader doesn't store any .dlls or data files in your game folders, making it truly portable and non-intrusive

## Installing
Simply download the [latest release](https://github.com/FenPhoenix/AngelLoader/releases) and unzip it to a folder of your choice. For example, `C:\AngelLoader`. New versions can be extracted right on top of old ones: your data files will not be overwritten.

*Note: Some folders are considered "protected" (`Program Files` / `Program Files (x86)` are examples) and AngelLoader should not be placed anywhere in these folders or it may not (probably won't) work.*

.NET Framework 4.7.2 or above is required. All modern versions of Windows should come with this already.

## Building
- Use Visual Studio 2022, .NET Framework 4.7.2 targeting, also going to need the C++ workload installed.
- Use Release_Public for the standard AngelLoader build.
- All dependencies now are either NuGet packages or are included in the bin_dependencies folder, so you should be able to just build with no fuss now.

## License
AngelLoader's code is released under the MIT license, except portions which are otherwise specified.
AngelLoader contains portions of code from the following:
- Ookii Dialogs by [Sven Groot](http://www.ookii.org/software/dialogs/), modified by [Caio Proiete](https://github.com/caioproiete/ookii-dialogs-winforms), and further modified and slimmed down by myself. This code is released under the BSD 3-clause license.
- DarkUI by [Robin Perris](https://github.com/RobinPerris/DarkUI)
- FFmpeg.NET by [Tobias Haimerl](https://github.com/cmxl/FFmpeg.NET)
- SharpCompress (stripped down to the bare minimum for 7z archive entry enumeration) by [Adam Hathcock](https://github.com/adamhathcock/sharpcompress)

FMScanner, which has now been forked to be part of AngelLoader, uses portions of code from the following:
- [SimpleHelpers.Net](https://github.com/khalidsalomao/SimpleHelpers.Net)
- Modified portions of the [.NET Core](https://github.com/dotnet/corefx) System.IO.Compression code (tuned for scanning performance)
