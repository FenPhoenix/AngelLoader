# AngelLoader

**TODO:** Write up instructions for getting it set up and compiling

**Things needed to build:**
- Run DllExport_Configure.bat. It will download the package automatically and eventually you'll see a window.
    - Choose AngelLoader.sln
    - Choose "Project files"
    - Check the Installed box for AngelLoader_Stub\AngelLoader_Stub.csproj. **Leave all the other Installed boxes unchecked.**
    - Click the x86 checkbox. This is absolutely required: AngelLoader_Stub is going to be called into by Thief (an x86 application), and therefore AngelLoader_Stub **must** be exported as x86 or it will simply not work.
    - When finished, the window should look something like this: [DllExport](https://www.dropbox.com/s/wabijv9on0h64ce/DllExport.png?dl=0)
    - Click Apply.
    - How the heck do you add an image to a github readme.
    - Sorry about this step, but there's absolutely nothing I can do about it: the author of DllExport refuses to use NuGet, opting instead for this apparently 100% custom thing. It's fine as far as it goes, but it does mean you have to write out a complicated-sounding explanation of how to set it up whenever you want other people to be able to use it. Meh. It is what it is.
 
- Download [FMScanner](https://github.com/FenPhoenix/FMScanner)
    - Add FMScanner.csproj to the AngelLoader solution in Visual Studio
    - Add a reference from AngelLoader.csproj to FMScanner.csproj in Visual Studio
    
- Download a 32-bit build of [FFmpeg](https://ffmpeg.zeranoe.com/builds/) (**must be 32-bit**)
    - Create a folder named "ffmpeg" in the solution base dir.
    - Extract the ffmpeg archive. It should have a bin folder in it. Copy all files from the bin folder to the ffmpeg folder you just created.
    - Although you can use any recent Windows build, I use a custom build with everything removed except mp3, ogg, and wav support (for size reduction - I don't want to be distributing a 40MB+ dependency). **TODO:** Host the custom build somewhere so others can use it
    - My custom build should be a NuGet package, but I can't figure out how to make a NuGet package consisting only of binaries, as no matter what I try it just gleefully vomits out an empty .dll along with the actual stuff

## Description
AngelLoader is a new fan mission loader for Thief 1, Thief 2, and Thief 3. Current loaders for those games (FMSel, NewDarkLoader) must be attached to each game individually, necessitating multiple installs, multiple setting of config options, the inability to manage all your missions in one place, etc. AngelLoader is a standalone one-stop shop for all your missions: every FM can be viewed, played, edited, installed, and uninstalled from one place.

The list of fan missions is filterable by game and many other criteria, and provides the option to either organize games by tab or to treat them as ordinary filters.

The interface is inspired by DarkLoader (by Björn Henke and Tom N. Harris) and NewDarkLoader (by Robin Collier). AngelLoader emulates the classic DarkLoader/NewDarkLoader UI design, with its simple "everything at your fingertips" layout making for a quick and intuitive experience. It also incorporates features from NewDarkLoader and FMSel, such as tags, filtering, rating, optional audio file conversion, etc.

FM loaders have traditionally had FM scanning functionality, and AngelLoader's scanner is second to none, detecting titles and authors from the trickiest of fan missions with a speed and accuracy rate not seen from any loader before. It also detects NewDark game types accurately, in contrast to DarkLoader which requires manual editing of its .ini file in order for NewDark Thief 1 missions to work.

In short, AngelLoader aims to be a complete successor to DarkLoader, being an all-in-one loader and manager with an intuitive interface, high performance, and many features both classic and modern.
