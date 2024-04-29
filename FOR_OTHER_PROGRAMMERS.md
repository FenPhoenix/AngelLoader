# Notes for other programmers

AngelLoader sometimes has weird code for the sake of reducing bloat, increasing performance, or because I just felt OCD about some stupid thing that ultimately doesn't matter much.

I'm going to document stuff that might be weird here so people might have a slightly less confusing time of it iunno.

## Solution configurations

- **Debug**: Standard debug config, and also the only config that allows editing forms with the designer (because the designer codepaths will be activated, and the manual and "init-slim" codepaths will be dummied out, with ifdefs). See form designer section below. Also, test UI buttons, labels, key commands, and various pieces of test code will be enabled. Asserts are enabled.
- **Release**: This is the "private" release build, which may contain features and things that I myself use but that I don't consider polished enough, or relevant enough, for general release. Test code mentioned above will **not** be enabled. Asserts are enabled.
- **Release_Beta**: Same as **Release_Public**, except activates a single RELEASE_BETA ifdef relating to displaying the beta version after the window title (with the version string being editable manually). Asserts are enabled.
- **Release_Public**: The public release profile. "Private" feature ifdefs will be deactivated (removed from the build) and the build will be in a state 100% ready for public release. Asserts are disabled.
- **Release_Testing**: Like **Release** except test code is enabled. Asserts are enabled.
- **Release_Testing_NoAsserts**: Like **Release_Testing** but asserts are disabled. Good for testing performance where asserts might mess up the measurement.
- **RT_StartupOnly**: Like **ReleaseTesting**, but will shut down immediately after the main window is shown. Good for measuring startup time only, guaranteeing that nothing after startup will being measured. Asserts are enabled.

In the **Debug** config, FenGen and FMScanner will still be in Release configs. If you want them to be in **Debug**, you'll have to change it.

FenGen has a special mode where if you put it in **Debug** config, it will show a window with a test button. If you set FenGen as the startup project, you can set a breakpoint, build the solution, and then click the button in the window to debug new FenGen code. That's why FenGen is by default in **Release** config even when the solution is in **Debug**. It's all kinda awkward, but meh. Once you're done debugging, set FenGen back to **Release** when solution is in **Debug**.

FMScanner is in **Release** always for reasons I can't remember. At some point in development that may have been important, or maybe it still is for weird code reasons, I dunno...

## FenGen (the code generator)

This is a separate project that runs before compilation of all the other projects. It generates code into various files based on various other files. This is kinda terrible in terms of understandability, but, it prevents errors from manual modification of files, and is _EXTREMELY HELPFUL_ in making localizable string additions a breeze.

FenGen will always run _**if**_ "Rebuild Solution" is chosen when building. Sometimes it won't build and/or run if some other options are chosen. I don't really remember which ones cause it to not build and/or run; I always just choose "Rebuild Solution". AngelLoader is small enough that full solution builds take only a few seconds (on my 3950x anyway) so it's not a big deal, at least to me.

Newer versions of C#/.NET (or whichever it is) now support official Code Generators, but see the notes at the top of FenGen/Core.cs for why I don't use them.
Short answer, because a) I need to modify .resx files which aren't "normal code" and Official Generators(tm) don't let you modify existing files I don't think, and b) at least last time I checked which was a while ago, the generated code "file" only gets updated ONCE PER VISUAL STUDIO RUN, and if you want to see further updates, you have to restart VS. Constantly. They may have fixed that by now, but I'm tired of dealing with MS introducing new things with great fanfare but then it taking like eight years before they actually function in any usable way. And we need a custom generator anyway for the above reason, so whatever.

FenGen is run from the pre_build.bat and post_build.bat files. It takes command line arguments to tell it which tasks to perform. See the FenGen code for what the args do.

## FenGen attributes and define headers

FenGen uses two different kinds of helpers: attributes, and define headers.

FenGen attributes are located in AngelLoader/Common/FenGenAttributes.cs. They aren't compiled with the code, and are strictly for FenGen to read textually to configure its behavior. Most of them have comments explaining them. If you add a new one, you also have to add it in FenGen/Core.cs and then add the implementation code in the appropriate place.

FenGen "define headers" are simply #define lines that go at the very top of FenGen-relevant .cs files to help it find those files faster. The attributes that are meant to be placed on "source" or "destination" enums or classes are also there to cut down FenGen's processing time by directing it straight to the appropriate class.

Using Roslyn to perform a full-solution analysis to find files "properly" is hilariously, unacceptably slow. Whereas letting FenGen simply loop through all .cs files, read the first line for a define header, and then move on, is gargantuanly faster.

Again, if you need to add a new define header, you have to add it to FenGen in addition to putting it at the top of the appropriate file(s).

## On an unsuccessful build, there are modified project files and maybe others like BuildDate.Generated.cs

FenGen runs once before solution build, and once AFTER solution build to remove certain temporary changes it's made. If the build fails, it doesn't get to do its second run to clean up the changes. You can simply revert the modifications from the files it's touched, or just fix the error and build again and it will clean up the changes itself.

## The form designers don't work, display an error page, or display an empty form?

Designing forms is a pain in the ass due to me trying to be clever (and succeeding, sorta, but also causing pain for anyone else).

Short answer: Close the designer, switch the solution configuration to Debug, rebuild, then open the form again. You should see the designer now. _**But,**_ if you're trying to design the _main_ form, you're in for a ton of manual work, see below!

Long answer:

Windows Forms lets you put text on controls in the designer, and you should, to understand how it will look and to make sure any text-based autosizing looks right etc. However, it's a massive amount of duplicated bloat to store the designer-entered text in the code-behind when it's just going to be replaced immediately with text from the chosen language file. So for MainForm.cs, I created a copy of the MainForm.Designer.cs code-behind file (MainForm_InitManual.cs) and manually removed all the '(whatever-control).Text = "Whatever text"' lines. Then I realized I didn't need the Name strings either, so I removed those too. Then I got drunk on the power of WinForms bloat removal, and went and also removed Size properties if it was AutoSize etc., and a crapton of other redundant things.

I did this manual de-bloating for all other forms for a while too, until I added an auto-Designer.cs-debloater to FenGen, so that most forms can now just be designed as normal and will have a (FormName)\_InitSlim.Generated.cs added automatically (as long as their .Designer.cs file has the line "#define FenGen_DesignerSource" at the top!). But, if you're making a new form, you're still going to have to add this bit manually to the constructor:

```
#if DEBUG
            InitializeComponent();
#else
            InitSlim();
#endif
```

replacing the single "InitializeComponent();" line that normally gets put there automatically.

Now, the main form. The main form is _not_ subject to this auto-debloater, because:
- It's extremely complex and the debloater isn't able to handle every possible debloat-requiring situation in it.
- It has certain modifications that aren't to do with debloating necessarily, but rather performance, like assigning images from the Images class rather than Resources. The debloater doesn't and can't really know about this easily.
- It has other custom code that's to do with ifdeffing out certain control declarations for public builds and such, and the debloater is not set up to be able to handle that either.

So... if you want to modify the main form through the designer, then you're going to have to visually diff and manually copy the modifications (debloating by hand) to MainForm_InitManual.cs. There will be a LOT of changes, but most of them will just be stupid crap like a width being 55 instead of 54 or whatever, for some reason. So you can just copy-and-paste all those as is, or leave them, whatever.

So, in conclusion, my recommendation is just to tear this whole system out if we cared about staying sane. Downside: the executable will be larger. Meh.

PS. FenGen also temporarily modifies AngelLoader.proj to exclude all forms' .resx files (so everything except the global Resources.resx, which is required) because we don't use them and they're fricking bloat. So yeah.

## A few forms don't even have a designer file?

These forms are based mainly around the set of supported games, and displaying a set of controls related to such. They're fully automated in their display of game-related controls, so that no manual work has to be done if new games were to be added. Not that that's ever really going to happen, but, yeah. Also it reduces bloat a bit.

## For MainForm, in the designer, a lot of stuff is missing images or text or just looks incomplete

These controls have their images, text, or other contents or attributes set programmatically (in MainForm.cs code file) for one reason or another (debloating the code-behind, putting game-related controls into a loop so we don't have duplicate code for each one, etc.)

## Why aren't we on .NET 5 or greater? Why are we still on Framework?

(for notes about .NET 5+ in the code, search it for "@NET5")

We've fixed the hook problem described below now, so that's no longer preventing us from switching to .NET 5+.

The reason we don't use it now is because .NET 5+ WinForms apps have a cold startup time of like 5 seconds, whereas on Framework cold starts are near-instant. This is an unacceptable user experience that will make you want to put your fist through your screen every time you start the app and then have to wait an eternity even just for the splash screen to be able to show up (which also makes the splash screen into a joke, because the whole point of a splash screen is to come up FAST when the app itself can't).

It's true that once running, the .NET version is faster, but it's not something you'll probably notice much if at all, but what you definitely will notice is an eight billion year startup time, so that's why I haven't switched yet. Native AOT may fix this (don't know for sure), but WinForms doesn't support Native AOT yet at the time of this writing, so that's moot.

~~We're prevented from moving to modern .NET for one seemingly tiny yet showstopping reason: the GetSysColor hook.~~

~~For dark mode, certain parts of the UI can only be themed by using "hooks", which is to say overriding certain Windows theme- or color-related functions and redirecting them to our own, where we hand it back custom colors. We use four:~~

~~- GetSysColor~~
~~- GetSysColorBrush~~
~~- DrawThemeBackground~~
~~- GetThemeColor~~

~~On Framework, all four work fine. On modern .NET, the last three work fine, but GetSysColor crashes with an ExecutionEngineException when it returns. I've tried everything under the sun, but it stubbornly refuses to work.~~

~~Update 2022-11-09: GetSysColor fails because the new runtimes have the SuppressGCTransition attribute on it. So... that's the end of the line for that.~~

~~GetSysColor is reponsible for the following:~~

~~- TextBox (and RichTextBox) selection color~~
~~- DateTimePicker~~
~~- RichTextBox default text color~~

~~If you simply turned off the GetSysColor hook, then DateTimePickers would appear light themed; selected text would be the default medium blue rather than our custom light blue; and RTF readmes would often have large swaths of their text black instead of light grey (this all in dark mode; light mode would look fine).~~

If we were feeling spicy, we could try to port AngelLoader to WinUI 3 or MAUI or whatever. I've tried but constantly run into obstacles that eventually disappear in version updates but then some other one comes up. Currently, the obstacle is that MAUI apps run fine within Visual Studio, but fail to run whatsoever OUTSIDE of Visual Studio. The WinUI 3 RichTextBox seems good and fast but we'd have to parse and convert embedded .wmf images to .png and then insert them back into the stream, cause it won't display .wmf images at all.

## Why Framework 4.7.2, not 4.8?

4.7.2 was the latest version at the time I started development. 4.8 is an overall minor upgrade and contains nothing that would substantially benefit AngelLoader, so I saw no reason to retarget. There's also a 4.8.1, but that's mostly an ARM64-related upgrade and thus irrelevant for us, and it also cuts off older OS support.

It's unlikely anyone would not have 4.8 at this point, so we could target it if we wanted and probably cause minimal to no disruption. But meh.

## Could AngelLoader be switched to 64-bit?

It's available in 64-bit and 32-bit versions now.

~~As far as I know, yeah, I haven't done any serious testing but I've made efforts to support 64-bit in places in the code that would have differences relating to such. Search for "@X64" to find notes on it.~~

~~Update 2022-09-17: With 64-bit we lose the GetSysColorBrush hook too (crashes "hooking near conditional jump not supported"), but this one - I _think_ - is literally only used as a fallback for drawing dark vertical/horizontal scroll bar corners on Windows 7 when Aero is disabled. Niche and everything else seems to look fine, so I just put a runtime 64-bit check around that hook so it just won't be enabled on x64.~~

~~Other than that, x64 appears to work fine from some light testing...~~

~~The reason it's 32-bit is that originally, I had it as a dll that the game calls into, FMSel-style. In that case, loaders are _required_ to be 32-bit because the game is 32-bit. However, then I made AngelLoader standalone, but never switched to x64 because I was like "meh, there's no need to and maybe one person in the world is still using 32-bit Windows or something, meh".~~

~~Due to the above point, note that AngelLoader_Stub (the C++ project) can _**not**_ be made 64-bit, because that's the part that the game calls into. Not that it needs to be anyway, it barely does anything but very slightly format some data and then pass it to the game.~~

## Could AngelLoader be made to run (or run better) on Linux?

~~AL can't really be a _native_ Linux app for reasons relating to it having to interface with a Windows game that's going to be running on Wine. For example, the game would have to run AngelLoader_Stub.dll, which must be a Windows dll, but then that dll is normally placed in AL's install directory, which would be on the Linux side, not Wine. Further, AL has to temporarily modify certain game config files, which are on the Wine side. I guess it might be able to be worked around, but it wouldn't work as it is now anyway.~~

~~But I'm not an expert on Linux or Wine (in fact I don't know much at all), so maybe native Linux and Wine apps can connect in harmony. I just don't know. You'd have to ask someone more knowledgeable or research it if you're interested.~~

Turns out, it looks like native apps can interop with Wine after all, according to the pinned comment [here](https://www.youtube.com/watch?v=eef4aE0XjVU). So if we had a cross-platform UI that would let us show rtf in some way or another, we could just make it native on .NET 5+ and write some Wine interop code.

As for having AL just run better on Wine, that's kinda on Wine unfortunately. We're using WinForms and .NET Framework (not modern .NET, for reasons stated above), and neither of those work particularly great on Wine. In my testing, I couldn't get AL to run usably at all on Linux, but others have, but the UI won't be perfect and especially dark mode will be _very_ not perfect.

## On modifying the FMData class, and the ConfigData class

FMData can be modified and FenGen will automatically generate the ini read/write code on next build. If you're adding a field whose type is of a new class that FenGen doesn't know about, you'll have to add code for that class to FenGen (FMDataGen.cs).

FenGen does NOT, however, generate anything from the ConfigData class. It's just too large and complex with too many special requirements (or so I judged at the time I made the decision anyway) and so if you add a field, you'll have to add the reader/writer code manually. Look at ConfigIni.cs to see how the code generally goes, you should be able to easily figure out the pattern for adding the reader/writer code by looking at what's there.

## Why use fields everywhere instead of properties lol?

Fields are:
- _Slightly_ faster (only really matters in tight loops like the RTF parser etc.)
- _Slightly_ smaller (like they make for a smaller executable)
- _Slightly_ shorter to type

Downside: They don't have CodeSense so you have to clunkily click "Show uses" or whatever

I basically just use them because logic-less properties aren't needed, and the idea of using something that isn't needed just because some faux-expert design patternsy website guy said so annoys me. Use properties if they're public-facing so you can change the implementation without breaking binary compatibility, sure, absolutely. I mean unless your dll is going to be recompiled with your exe always, in which case you don't even need to do that. But internally? Meh! Use the more efficient thing! >:(

## Properties/Resources.resx only contains some of the images? Where are the rest?

The rest are generated (drawn, painted) programmatically and cached (Images.cs) for less bloat.

## What's the deal with FMScanner's "#define FMScanner_FullCode" thing and then a bunch of ifdefs in the code?

FMScanner was originally made before AngelLoader, and was made to scan all kinds of things AngelLoader didn't end up needing. I wanted to keep that code in for other purposes, but AL didn't need it, so I ifdeffed it all out to... reduce bloat. I really really like reducing bloat.

## Why use that weird custom I/O stuff when GetFiles() etc. works perfectly fine?

At least on Framework, GetFiles() (and its ilk) use a slow version of getting the file info, where it asks Windows for stuff it doesn't need (or more precisely, fails to explicitly tell Windows NOT to get stuff it doesn't need), and so making a custom version ended up being a _lot_ faster. The other reason is that for the scanner, it sometimes needs to know if any file with a given extension exists in a directory. With GetFiles() (and its ilk), if you told it to get "\*.mc" it would grab _all_ \*.mc files even though you only wanted to know if there was _at least one_. Our custom version can just walk the directory and return true as soon as it finds the first file. Also, we needed to use the custom version for the above reason anyway, so yeah.

When you see GetFiles() (or its ilk) being used, it's probably because we want to search the entire subdirectory, and our custom version can't do that (because I just never added that capability).

## Import code is horrific

Import code is fixed now.

~~Yeah... it is. It's some of the oldest code still in there, and I was a lot noobier back then and didn't even know what a hash table was. I honestly just haven't wanted to even look at it, but you can see comments with me admitting it doesn't do a very good job technically (though in practice it works well in most cases). Uh... so yeah, sorry about that.~~

## "#define WPF"?

I was experimenting with whether I could switch to WPF for AngelLoader. Turns out not (because of the explosively slow RichTextBox). But, I've removed all UI-specific knowledge from the main business logic in case I find a UI framework I can feasibly switch to later.

## Business logic in the UI code?

Yeah, some, but it's mostly gone. Hopefully it isn't too bad and can be worked with.

## "TransparentPanel"?

That's just a cheap hack to block input to the UI because I couldn't figure out how to block all of it using just the message filter. It's silly but works, so whatever.

## No unit tests?!

Uh, yeah, kinda not really. I do use a few semi-automated tests internally (separate testing apps, now on github in the TestApps folder).

There's also a bit of test code ifdeffed out in the codebase, for when I need to test something I don't change often. I test the accuracy of the scanner and of the rtf-to-plaintext converter and such with external test apps. There's some ifdeffed test code for testing RTF visual accuracy that's a bit more involved too. So yeah, some things ARE semi-auto tested.

## Lazy-loaded controls?

For controls that may not actually ever be visible on a given run (mostly context menus, but some other things too), I keep them lazy-loaded (in other words, loaded only right before they'll be displayed) to keep startup time down. This does actually help because there's a lot of them and the Win32 UI is deadly slow in Win10 (it was pretty fast in Win7).

Lazy loaded control classes all follow a similar pattern, mostly having Construct(), Localize(), backing fields for the constructed bool and any relevant state, etc. But they're also mostly different enough in the details that they can't really share much. So, the pattern is basically just self imposed and not mandated by interface or whatever.

## Dark mode implementation is nightmarish

Yeah... every control has a different way you have to theme it, and sometimes you need to use the sledgehammer of a Win32 hook. ListBox couldn't be themed to an acceptable level at all, so I had to imitate a ListBox with a ListView. Sorry, but I really did my best. WinForms isn't meant to be themed.

Unfortunately, if you want to create a new control, or use one that isn't currently used, you'll have to figure out how to theme it anew. Fortunately, I think I've only had to do that like once since I introduced dark mode?

## Windows-specific code

There's _tons_ of p/invokes all over the place, but the vast majority are for the UI. The ones that are in the non-UI logic are mostly just the custom fast I/O and a handful of miscellaneous stragglers. Assuming we switched out the UI, there'd be hardly any Windows-specific code left, easy enough to deal with.

There is also a ProcessUtils class which contains wrappers to always start Process objects with UseShellExecute = true. We'd have to figure those out if we wanted to go cross-platform.

## await eliding?

Because I love removing bloat way too much, I've elided await where reasonable, or just written code in a way that doesn't need so many links in the await chain. It actually helps quite a lot with exe size reduction.

## ReSharper

I use ReSharper which includes a "To-do Explorer" which lets you set certain phrases that will be highlighted and added to a list where you can browse them. It's awesome, but if you don't have ReSharper then you'll have a harder time finding notes on certain things. Most of the "todo" phrases start with an @ (except "TODO:" itself and a couple others), so if you regex search for like "@\w+" or something you can prolly find them.
