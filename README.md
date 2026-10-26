<h1 align="center">
AngelLoader

## This is the modern .NET version. It is believed to work fine (but doesn't receive much field testing), and you can compile and use it if you're interested in comparing.

### *IMPORTANT!*
If you want to change the targeted .NET version (eg. to upgrade), you **must** [follow the instructions for generating a new custom System.Drawing.dll here](https://github.com/FenPhoenix/AngelLoader/blob/NET_Modern/BUILDING.md) or it won't compile!

<hr>

There are no new features compared to the standard .NET Framework version, and the code is almost entirely the same, differing only to work around breaking changes or to take advantage of higher-performance language features.

### The good:
- General performance increase - scans get faster, anything related to zip files gets faster (installing etc). 7z stuff does _not_ get faster because it's handled by the external native 7z.exe.
- Potential to properly support high DPI, due to WinForms receiving ongoing development in that regard on modern .NET (but this is not implemented currently).
- Potential to use SIMD to really crank performance in bottlenecky areas (but again, not implemented).

### The bad:
- **_Horrendous_** cold startup time - this is endemic to all WinForms apps on modern .NET. It takes several seconds merely just to get to the splash screen. Personally, I find this to be an unacceptable user experience. Your tolerance for it may vary. Note that cold startup doesn't just happen on a fresh boot; it happens whenever Windows decides to invalidate whatever cache is keeping it fast. So every time you go to start the app you wince in anticipation of it maybe being cold this time. Like I said, truly awful UX.

### The mediocre:
- The UI does not get any faster, unfortunately, and that's probably the place you would most notice a performance difference if there was one.
- While the performance increases appear large in the profiler, they're not that noticeable in actual use, at least not to me.

If the cold startup time were solved (either directly or through finishing of WinForms AOT support (assuming that would fix it)), I would just switch AngelLoader to modern .NET. As it stands, though, I don't find the moderate general perf increase to be an acceptable trade for the infuriating, decades-regressed cold startup performance. Seriously I'm pretty sure my 386 started apps faster. 🤷‍♂️

<hr/>

</h1>
<p align="center"><img src="https://github.com/FenPhoenix/AngelLoader/blob/master/docs/images/main_window_v194_900w.png" /></p>

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

### Running on Linux:
I haven't tried or tested running this modern .NET version on Linux yet, sorry.

## Building
- Use Visual Studio 2026 with .NET and C++ desktop workloads installed.
- Use Release_Public for the standard AngelLoader build.
- All dependencies now are either NuGet packages or are included in the bin_dependencies folder, so you should be able to just build with no fuss now.
- To repeat, for this modern .NET version, you must [follow the instructions for generating a new custom System.Drawing.dll here](https://github.com/FenPhoenix/AngelLoader/blob/NET_Modern/BUILDING.md) or it won't compile once the targeted version has been changed.

## License
AngelLoader's code is released under the MIT license, except portions which are otherwise specified in the About window and in the [Licenses directory](https://github.com/FenPhoenix/AngelLoader/tree/master/BinReleaseOnly/Licenses).

## Statement on the Non-Use of AI
I do not use AI to generate any of my code, and in fact I don't use it in any area of my programming workflow, period. All my code is written by me. I can't vouch that third-party dependencies don't contain any AI generated code, but thankfully most of them are old enough that they probably don't, so there's that.
