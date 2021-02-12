<h1 align="center">
AngelLoader
</h1>
<p align="center"><img src="https://fenphoenix.com/github/AngelLoader/MainWindow_1.4.5_600_v3.png" /></p>


<hr>
<h4 align="center">
AngelLoader is supported by JetBrains under their Open Source License.
</h4>
<p align="center">
<a href="https://www.jetbrains.com/?from=AngelLoader"><img src="https://fenphoenix.com/github/AngelLoader/jetbrains.svg" /></a>
</p>

## Description
AngelLoader is a modern, standalone fan mission loader for Thief 1, Thief 2, Thief 3, and System Shock 2. Current loaders for those games (FMSel, NewDarkLoader) must be attached to each game individually, necessitating multiple installs, multiple setting of config options, the inability to manage all your missions in one place, etc. AngelLoader is a one-stop shop for all your missions: every FM can be viewed, played, edited, installed, and uninstalled from one place.

The list of fan missions is filterable by game and many other criteria, and provides the option to either organize games by tab or to treat them as ordinary filters.

The interface is inspired by DarkLoader (by Björn Henke and Tom N. Harris) and NewDarkLoader (by Robin Collier). AngelLoader emulates the classic DarkLoader/NewDarkLoader UI design, with its simple "everything at your fingertips" layout making for a quick and intuitive experience. It also incorporates features from NewDarkLoader and FMSel, such as tags, filtering, rating, optional audio file conversion, etc.

FM loaders have traditionally had FM scanning functionality, and AngelLoader's scanner is second to none, detecting titles and authors from the trickiest of fan missions with a speed and accuracy rate not seen from any loader before. It also detects NewDark game types accurately, in contrast to DarkLoader which requires manual editing of its .ini file in order for NewDark Thief 1 missions to work.

In short, AngelLoader aims to be a complete successor to DarkLoader, being an all-in-one loader and manager with an intuitive interface, high performance, and many features both classic and modern.

## Building
- All dependencies now are either NuGet packages or are included in the bin_dependencies folder, so you should be able to just build with no fuss now.

## License
AngelLoader's code is released under the MIT license, except portions which are otherwise specified.
AngelLoader contains portions of code from the following:
- Ookii Dialogs by [Sven Groot](http://www.ookii.org/software/dialogs/), modified by [Caio Proiete](https://github.com/caioproiete/ookii-dialogs-winforms), and further modified and slimmed down by myself. This code is released under the BSD 3-clause license.
- DarkUI by [Robin Perris](https://github.com/RobinPerris/DarkUI)

FMScanner, which has now been forked to be part of AngelLoader, uses portions of code from the following:
- [SimpleHelpers.Net](https://github.com/khalidsalomao/SimpleHelpers.Net)
- Modified portions of the [.NET Core](https://github.com/dotnet/corefx) System.IO.Compression code (tuned for scanning performance)
