# AngelLoader

**TODO:** Write up instructions for getting it set up and compiling

- Needs FFmpeg in "ffmpeg" folder in solution base dir. Can use any recent Windows build, but I use a custom build with everything removed except mp3, ogg, and wav support (for size reduction - I don't want to be distributing a 40MB+ dependency). **TODO:** Host the custom build somewhere so others can use it
- Needs to have DllExport stuff run manually; explain required settings for that

## Description:
AngelLoader is a new fan mission loader for Thief 1, Thief 2, and Thief 3. Current loaders for those games (FMSel, NewDarkLoader) must be attached to each game individually, necessitating multiple installs, multiple setting of config options, the inability to manage all your missions in one place, etc. AngelLoader is a standalone one-stop shop for all your missions: every FM can be viewed, played, edited, installed, and uninstalled from one place.

The list of fan missions is filterable by game and many other criteria, and provides the option to either organize games by tab or to treat them as ordinary filters.

The interface is inspired by DarkLoader (by Björn Henke and Tom N. Harris) and NewDarkLoader (by Robin Collier). AngelLoader emulates the classic DarkLoader/NewDarkLoader UI design, with its simple "everything at your fingertips" layout making for a quick and intuitive experience. It also incorporates features from NewDarkLoader and FMSel, such as tags, filtering, rating, optional audio file conversion, etc.

FM loaders have traditionally had FM scanning functionality, and AngelLoader's scanner is second to none, detecting titles and authors from the trickiest of fan missions with a speed and accuracy rate not seen from any loader before. It also detects NewDark game types accurately, in contrast to DarkLoader which requires manual editing of its .ini file in order for NewDark Thief 1 missions to work.

In short, AngelLoader strives to be a complete successor to DarkLoader, being an all-in-one loader and manager with an intuitive interface, high performance, and many features both classic and modern.
