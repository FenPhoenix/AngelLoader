## Localizable text changelog for the benefit of translators

### Key:
Red lines with a `-` in front of them are lines that have been **removed** for the listed version.
```diff
- removed line
```
Green lines with a `+` in front of them are lines that have been **added** for the listed version.
```diff
+ added line
```
Black lines with no `-` or `+` in front of them are lines that have not changed, and are just there to give you context on where the changed text is located.
```diff
AngelLoader is awesome.
- You can use it to load your fan missions and become fantastigreat.
+ You can use it to load, play, and manage your fan missions and become tafferiffic.
Thanks for checking it out!
```

## List of new strings by version

### v1.3.2:

No localizable text changes.

### v1.3.1:

No localizable text changes.

### v1.3:

No localizable text changes.

### v1.2:

#### Documentation:

#### Images:

The following image has been **added**:  
`Shock2_21.png`  

The following images have been **updated** to show System Shock 2 support:  
`main_window_full_960.png`  
`fms_list_960.png`  
`initial_setup.png`  
`setup_paths_tab.png`  

The following image has been **updated** to show short game tab name support:  
`setup_fm_display_tab.png`  

#### AngelLoader documentation.html:

```diff
<h1><img src="images/AngelLoader_Icon_48.png" style="vertical-align:middle" /> AngelLoader</h1>
- <h4>A fan mission loader for Thief Gold, Thief II: The Metal Age, and Thief: Deadly Shadows</h4>
+ <h4>A fan mission loader for Thief Gold, Thief II: The Metal Age, Thief: Deadly Shadows, and System Shock 2</h4>
```

```diff
<h4><a name="paths_to_game_exes" />Paths to game executables</h4>
- Here you can choose the executable files for the Thief games you have installed. These fields are optional - you can set some, all, or none. You will still be able to see and manage all of your fan missions even if you haven't set their corresponding executable, but of course you won't be able to install or play any of them unless their executable is set.
- <p><b>Thief 1</b> and <b>Thief 2</b> must be patched with NewDark in order for AngelLoader to be able to use them. <b>Thief 3</b> must be patched with the Sneaky Upgrade. Version 1.1.9.1 or above is recommended; while versions as far back as 1.1.3 may work, they haven't been tested and are not officially supported.</p>
+ Here you can choose the executable files for the supported games you have installed. These fields are optional - you can set some, all, or none. You will still be able to see and manage all of your fan missions even if you haven't set their corresponding executable, but of course you won't be able to install or play any of them unless their executable is set.
+ <p><b>Thief 1</b>, <b>Thief 2</b> and <b>System Shock 2</b> must be patched with NewDark in order for AngelLoader to be able to use them. <b>Thief 3</b> must be patched with the Sneaky Upgrade. Version 1.1.9.1 or above is recommended; while versions as far back as 1.1.3 may work, they haven't been tested and are not officially supported.</p>

<h4>Steam options</h4>
- If you own one or more Thief games on Steam, you can specify the location of <code>steam.exe</code> and choose which games should be launched through it. Launching a game in this way enables access to Steam features such as the in-game overlay, play time counter, etc.
+ If you own one or more supported games on Steam, you can specify the location of <code>steam.exe</code> and choose which games should be launched through it. Launching a game in this way enables access to Steam features such as the in-game overlay, play time counter, etc.
```

```diff
<h4>Game</h4>
Displays an icon based on which game the mission is for. If unknown, it will be blank. The icons are:
<p>
<img src="images/Thief1_21.png" /> - Thief<br>
<img src="images/Thief2_21.png" /> - Thief II<br>
<img src="images/Thief3_21.png" /> - Thief: Deadly Shadows<br>
+ <img src="images/Shock2_21.png" /> - System Shock 2<br>
<img src="images/red_circle_question_mark_21.png" /> - Unsupported mission or non-mission archive (these only appear when the <a href="#show_junk">Show Unsupported</a> filter is enabled).</p>
```

```diff
<h4>Finished</h4>
Displays an icon representing which difficulty or difficulties you've finished an FM on.
<p>
- <img src="images/Finished_Normal.png" /> - <b>Normal</b> (<b>Easy</b> for Thief: Deadly Shadows)<br>
- <img src="images/Finished_Hard.png" /> - <b>Hard</b> (<b>Normal</b> for Thief: Deadly Shadows)<br>
- <img src="images/Finished_Expert.png" /> - <b>Expert</b> (<b>Hard</b> for Thief: Deadly Shadows)<br>
- <img src="images/Finished_Extreme.png" /> - <b>Extreme</b> (<b>Expert</b> for Thief: Deadly Shadows)<br>
+ <img src="images/Finished_Normal.png" /> - <b>Normal</b> (<b>Easy</b> for Thief: Deadly Shadows and System Shock 2)<br>
+ <img src="images/Finished_Hard.png" /> - <b>Hard</b> (<b>Normal</b> for Thief: Deadly Shadows and System Shock 2)<br>
+ <img src="images/Finished_Expert.png" /> - <b>Expert</b> (<b>Hard</b> for Thief: Deadly Shadows and System Shock 2)<br>
+ <img src="images/Finished_Extreme.png" /> - <b>Extreme</b> (<b>Expert</b> for Thief: Deadly Shadows, <b>Impossible</b> for System Shock 2)<br>
<img src="images/Finished_Unknown.png" /> - <b>Unknown</b>
</p>
See the <a href="#finished_on">Finished On submenu</a> for more information about difficulty levels.
```

```diff
<h4>Open FM in DromEd</h4>
- This item will only appear if DromEd.exe was found in the game directory. Clicking it will open the currently selected FM in DromEd, installing it first if necessary.<br>
+ This item will only appear if DromEd.exe (or ShockEd.exe for System Shock 2) was found in the game directory. Clicking it will open the currently selected FM in DromEd or ShockEd, installing it first if necessary.<br>
<b>This option does not apply to Thief: Deadly Shadows.</b>
```

```diff
<h4><a name="finished_on" />Finished On submenu</h4>
Here you can set which difficulty or difficulties you've finished the selected mission on.<br>
<b>Unknown</b> - this is mainly for compatibility with imported FMSel data, which doesn't mark difficulties for its Finished value.<br>
For Thief 1 and Thief 2, the other difficulties are <b>Normal</b>, <b>Hard</b>, <b>Expert</b>, and <b>Extreme</b>.<br>
For Thief: Deadly Shadows, they are <b>Easy</b>, <b>Normal</b>, <b>Hard</b>, and <b>Expert</b>.<br>
+ For System Shock 2, they are <b>Easy</b>, <b>Normal</b>, <b>Hard</b>, and <b>Impossible</b>.<br>
"<b>Extreme</b>" is not an official Thief difficulty, but is provided for compatibility with imported DarkLoader data, or to use as you see fit (to denote Ghost, etc).
```

```diff
<h4>Disabled mods</h4>
-You can disable certain mods per-mission here. To see which mods you have installed, look at <code>cam_mod.ini</code> in your Thief game folder. This string must be in the format <code>modname1+modname2+modname3</code> etc. So to disable the Enhancement Pack 2, it should be:
+ You can disable certain mods per-mission here. To see which mods you have installed, look at <code>cam_mod.ini</code> in your game folder. This string must be in the format <code>modname1+modname2+modname3</code> etc. So to disable the Enhancement Pack 2, it should be:

<p><code>ep2</code></p>

<p>To disable the Enhancement Pack 2 and the HD mod, it would be:</p>

<p><code>ep2+hdmod</code></p>

<p>To disable all mods for the current FM, check the <b>Disable all mods</b> checkbox.</p>
```

```diff
<h4>Game organization</h4>
Here you can choose to either organize games into their own tabs, or to display your FMs as one list and allow filtering by game. When game tabs are enabled, each game will have its own selected FM and set of filters that will be retained between tab switches. Organizing games by tab can make things cleaner, but if you want to filter without regard to game (say, to find all missions by a single author who has released missions for multiple games), then having one list with game filters will work better.

+ <p>
+  If you select the <b>Use short names on game tabs</b> checkbox, game tabs will be displayed with abbreviated names to save screen space.
+ </p>
```

```diff
<h4><a name="settings_audio_conversion" />Convert .wavs to 16 bit on install</h4>
- Depending on your setup, .wav files that are higher than 16 bit may cause audio problems when played by Thief 1 or Thief 2, such as intermittent or constant static noise. Converting all .wav files to 16 bit will solve this issue, and does not result in a perceptible loss of fidelity. Therefore, this option is turned on by default.<br>
+ Depending on your setup, .wav files that are higher than 16 bit may cause audio problems when played by Dark Engine games, such as intermittent or constant static noise. Converting all .wav files to 16 bit will solve this issue, and does not result in a perceptible loss of fidelity. Therefore, this option is turned on by default.<br>
<b>This option has no effect for Thief: Deadly Shadows.</b>
```

```diff
<p><b>AngelLoader uses the following libraries:</b></p>
<a href="https://www.7-zip.org/">7z.dll</a><br>
<a href="https://github.com/squid-box/SevenZipSharp">SquidBox.SevenZipSharp</a><br>
<a href="https://ffmpeg.org/">ffmpeg</a><br>
<a href="https://github.com/cmxl/FFmpeg.NET">FFmpeg.NET</a><br>
- <a href="https://github.com/gmamaladze/globalmousekeyhook">GlobalMouseKeyHook</a><br>
<a href="https://github.com/khalidsalomao/SimpleHelpers.Net/">SimpleHelpers.Net</a><br>
<a href="https://github.com/yinyue200/ude">UDE.NetStandard</a><br>
+ <p><b>and portions of code from the following:</b></p>
<a href="https://github.com/caioproiete/ookii-dialogs-winforms">Ookii Dialogs</a><br>
<a href="https://github.com/dotnet/corefx">Modified portions of .NET Core's System.IO.Compression code</a> (tuned for scanning performance)<br>
```
#### English.ini:  

```diff
[Global]
Thief1=Thief 1
Thief2=Thief 2
Thief3=Thief 3
+ SystemShock2=System Shock 2

+ Thief1_Short=T1
+ Thief2_Short=T2
+ Thief3_Short=T3
+ SystemShock2_Short=SS2

Thief1_Colon=Thief 1:
Thief2_Colon=Thief 2:
Thief3_Colon=Thief 3:
+ SystemShock2_Colon=System Shock 2:
```

```diff
[AlertMessages]
Play_ExecutableNotFound=Executable file not specified or not found. Unable to play.
Play_GamePathNotFound=Game path not found. Unable to play.
Play_ExecutableNotFoundFM=Executable file not specified or not found. Unable to play FM.
Play_GameIsRunning=Game is already running. Exit it first!
- Play_AnyGameIsRunning=One or more Thief games are already running. Please exit them first.
+ Play_AnyGameIsRunning=One or more supported games are already running. Please exit them first.
Play_UnknownGameType=Selected FM's game type is not known. The FM is either not scanned or is not an FM. Unable to play.
Play_ConfirmMessage=Play FM?

DromEd_ExecutableNotFound=DromEd.exe was not found in the game directory. Unable to open FM.
+ ShockEd_ExecutableNotFound=ShockEd.exe was not found in the game directory. Unable to open FM.
DromEd_UnknownGameType=Selected FM's game type is not known. The FM is either not scanned or is not an FM. Unable to open FM.
```

```diff
[Difficulties]
Easy=Easy
Normal=Normal
Hard=Hard
Expert=Expert
Extreme=Extreme
+ Impossible=Impossible
Unknown=Unknown
```

```diff
[FMsList]
FMMenu_PlayFM_Multiplayer=Play FM (multiplayer)
FMMenu_InstallFM=Install FM
FMMenu_UninstallFM=Uninstall FM
FMMenu_OpenInDromEd=Open FM in DromEd
+ FMMenu_OpenInShockEd=Open FM in ShockEd
FMMenu_Rating=Rating
FMMenu_FinishedOn=Finished on
FMMenu_ConvertAudio=Convert audio
FMMenu_ScanFM=Scan FM
FMMenu_WebSearch=Web search
```

```diff
[SettingsWindow]
- Paths_Thief1AndThief2RequireNewDark=* Thief 1 and Thief 2 require NewDark.
+ Paths_DarkEngineGamesRequireNewDark=* Thief 1, Thief 2 and System Shock 2 require NewDark.

FMDisplay_GameOrganization=Game organization
FMDisplay_GameOrganizationByTab=Each game in its own tab
+ FMDisplay_UseShortGameTabNames=Use short names on game tabs
FMDisplay_GameOrganizationOneList=Everything in one list, and games are filters
```

### v1.1.6:

#### English.ini:

The following strings have been **added** under the **\[SettingsWindow\]** section:

`Paths_SteamOptions=Steam options`  
`Paths_PathToSteamExecutable=Path to Steam executable (optional):`  
`Paths_LaunchTheseGamesThroughSteam=If Steam exists, use it to launch these games:`

In order to reduce duplication, the following strings have been **added** under the **\[Global\]** header:

`Thief1=Thief 1`  
`Thief2=Thief 2`  
`Thief3=Thief 3`  

`Thief1_Colon=Thief 1:`  
`Thief2_Colon=Thief 2:`  
`Thief3_Colon=Thief 3:`  

The following section and all its listed strings (listed below) has been **removed**:

`[GameTabs]`  
`Thief1=Thief 1`  
`Thief2=Thief 2`  
`Thief3=Thief 3`  

The following strings have been **removed** from the **\[FilterBar\]** section:

`Thief1ToolTip=Thief 1`  
`Thief2ToolTip=Thief 2`  
`Thief3ToolTip=Thief 3`  

The following strings have been **removed** from the **\[PlayOriginalGameMenu\]** section:

`Thief1=Thief 1`  
`Thief2=Thief 2`  
`Thief3=Thief 3`  

And the following strings have been **removed** from the **\[SettingsWindow\]** section:

`Paths_Thief1=Thief 1:`  
`Paths_Thief2=Thief 2:`  
`Paths_Thief3=Thief 3:`  

#### Documentation:

#### AngelLoader documentation.html:

The following text has been **added** under the **Getting Started** -> **Initial Setup** section:

`<h4>Steam options</h4>`  
`If you own one or more Thief games on Steam, you can specify the location of <code>steam.exe</code> and choose which games should be launched through it. Launching a game in this way enables access to Steam features such as the in-game overlay, play time counter, etc.`

#### images:

The following image files have been **changed**:

- updated `initial_setup.png` to add "Steam options" section
- updated `setup_paths_tab.png` to add "Steam options" section

### v1.1.5:

#### English.ini:

The following strings have been **added** under the **\[Importing\]** section:

`ImportData_Title=Title`  
`ImportData_ReleaseDate=Release date`  
`ImportData_LastPlayed=Last played`  
`ImportData_Finished=Finished`  
`ImportData_Comment=Comment`  
`ImportData_Rating=Rating`  
`ImportData_DisabledMods=Disabled mods`  
`ImportData_Tags=Tags`  
`ImportData_SelectedReadme=Selected readme`  
`ImportData_Size=Size`  
