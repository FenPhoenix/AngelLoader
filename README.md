<h1 align="center">
AngelLoader
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

### Running on Linux:
The latest version of AngelLoader runs well under Linux using Wine or Proton, with some tweaking:

- For now, the **32-bit** version is easier to get up and running and recommended. Make sure to download the version with `x86` in the name from [latest release](https://github.com/FenPhoenix/AngelLoader/releases).
- You will need to install the following dependencies into your AngelLoader prefix: `gdiplus`, `msftedit` and `msls31`. Use either winetricks for Wine, protontricks for Proton, or add them as dependencies in Bottles if using that.

That's it! Launch AngelLoader and you're good to go.

If you did want to use the **64-bit** version of AngelLoader, a few more steps are required. We need a 64-bit version of `msls31.dll`. The easiest way is to get it from the official IE8 installer directly from Microsoft.

  - Download [IE8-WindowsServer2003-x64-ENU.exe](https://download.microsoft.com/download/7/5/4/754D6601-662D-4E39-9788-6F90D8E5C097/IE8-WindowsServer2003-x64-ENU.exe)
  - Extract the exe using Ark, 7-zip or whatever archive program your system has.
  - Copy `msls31.dll` from the extracted files to `drive_c/windows/system32` inside your prefix, overwriting what's there.

  Alternatively, if you have access to an actual Windows 10/11 install, you can grab the dll from `C:\Windows\System32\msls31.dll` and copy it over.

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
