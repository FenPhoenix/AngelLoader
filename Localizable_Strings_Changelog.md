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

### v1.4:

#### English.ini:  

```diff
[FilterBar]
...
ShowJunk=Show FMs marked as "unsupported game or non-FM archive"
+ ShowRecentAtTop=Show recently added FMs at the top of the list
```

```diff
GameColumn=Game
InstalledColumn=Installed
TitleColumn=Title
ArchiveColumn=Archive
AuthorColumn=Author
SizeColumn=Size
RatingColumn=Rating
FinishedColumn=Finished
ReleaseDateColumn=Release Date
LastPlayedColumn=Last Played
+ ; The date an FM was added to the list. Basically means the date you downloaded it and put it into your archives folder.
+ DateAddedColumn=Date Added
DisabledModsColumn=Disabled Mods
CommentColumn=Comment
```

```diff
[EditFMTab]
TabText=Edit FM

Title=Title:
Author=Author:
ReleaseDate=Release date:
LastPlayed=Last played:
Rating=Rating:
FinishedOn=Finished on...
DisabledMods=Disabled mods:
DisableAllMods=Disable all mods
+ PlayFMInThisLanguage=Play FM in this language:
+ DefaultLanguage=Default

RescanTitleToolTip=Rescan title
RescanAuthorToolTip=Rescan author
RescanReleaseDateToolTip=Rescan release date
+ RescanLanguages=Rescan for supported languages
RescanForReadmes=Rescan for readmes
```

```diff
[SettingsWindow]
...
FMDisplay_DateFormat=Date format
FMDisplay_CurrentCultureShort=Current culture short
FMDisplay_CurrentCultureLong=Current culture long
FMDisplay_Custom=Custom:

FMDisplay_ErrorInvalidDateFormat=Invalid date format.
FMDisplay_ErrorDateOutOfRange=The date and time is outside the range of dates supported by the calendar used by the current culture.

+ FMDisplay_RecentFMs=Recent FMs
+ FMDisplay_RecentFMs_MaxDays=Maximum number of days to consider an FM "recent":
```

#### Documentation:

#### Images:
The following images have been **updated** to show new text and features:  
`main_window_full_960.png`  
`fms_list_960.png`  
`edit_fm_tab.png`  
`setup_fm_display_tab.png`  

#### AngelLoader documentation.html:

```diff
<h2><a name="main_window" />Main window</h2>

<p><img src="images/main_window_full_960.png" /></p>

<h4>Startup scan</h4>
- Whenever new FMs are detected, they will be quick-scanned for game type only. If you cancel the scan, then the game types will remain blank and will be scanned when selected, similar to DarkLoader. It's recommended that you let the scan finish, especially if you're using game tabs, as it will result in FMs being categorized properly. You can perform a more detailed scan later (see <a href="#scan_all_fms_button">Scan all FMs</a>).
+ Whenever new FMs are detected, they will be automatically scanned. If you cancel the scan, then they will be scanned when manually selected, similar to DarkLoader. It's recommended that you let the scan finish, especially if you're using game tabs, as it will result in FMs being categorized properly.

<h4>Filter bar</h4>
Here you can choose to filter your FM list by game, title, author, release date, last played date, tags, finished status, and rating. If you've chosen to organize your games by tab, then these tabs will take the place of the game filter buttons.

- <h4><a name="show_junk" />Show unsupported</h4>
+ <h4><a name="show_junk" />Show FMs marked as "unsupported game or non-FM archive"</h4>
This allows archives marked as Unknown (<img src="images/red_circle_question_mark_21.png" />) (archives that were rejected as not being FMs) to be displayed in the list. If support for new games is added in the future, you can use this to show previously unsupported FMs so you can re-scan them individually if you wish.

+ <h4><a name="show_recent_at_top" />Show recently added FMs at the top of the list</h4>
+ This will cause recently added FMs to be highlighted and displayed at the top of the list. This makes it easier to find FMs that you've just downloaded, for example. The number of days to consider an FM "recent" can be changed in the <a href="#settings_days_recent">Settings window</a>. The default is 15 days.
+
<h4>Refresh from disk button</h4>
Reloads the list of FMs from disk. This will always occur on startup, but this button is useful if you've added new FMs since starting AngelLoader.
<br>The list can also be refreshed from disk by pressing <code>Shift-F5</code> when the <a href="#mission_list">mission list</a> is focused.
```

```diff
<h4>Finished</h4>
Displays an icon representing which difficulty or difficulties you've finished an FM on.
<p>
<img src="images/Finished_Normal.png" /> - <b>Normal</b> (<b>Easy</b> for Thief: Deadly Shadows and System Shock 2)<br>
<img src="images/Finished_Hard.png" /> - <b>Hard</b> (<b>Normal</b> for Thief: Deadly Shadows and System Shock 2)<br>
<img src="images/Finished_Expert.png" /> - <b>Expert</b> (<b>Hard</b> for Thief: Deadly Shadows and System Shock 2)<br>
<img src="images/Finished_Extreme.png" /> - <b>Extreme</b> (<b>Expert</b> for Thief: Deadly Shadows, <b>Impossible</b> for System Shock 2)<br>
<img src="images/Finished_Unknown.png" /> - <b>Unknown</b>
</p>
See the <a href="#finished_on">Finished On submenu</a> for more information about difficulty levels.

<h4>Release Date</h4>
Displays the FM's release date in the <a href="#settings_date_format">specified format</a>.

<h4>Last Played</h4>
Displays the FM's last played date in the <a href="#settings_date_format">specified format</a>.

+ <h4>Date Added</h4>
+ Displays the date the FM was added to the list.
+
<h4>Disabled Mods</h4>
Displays the disabled mods, if any, for the FM. If all mods are disabled for the FM, it will display "* [All]".

<h4>Comment</h4>
Displays the FM's comment, if any, up to the first 100 characters or the first linebreak, whichever comes first.
```

```diff
<h4><a name="edit_fm_tab" />Edit FM tab</h4>

<p><img src="images/edit_fm_tab.png" /></p>

Here you can edit an FM's data. If you want to re-scan for a certain field, click the <img src="images/scan_14.png" /> icon beside the field.

<h4>Alternate titles button</h4>
Sometimes, multiple different titles will be detected during a scan. If the default title doesn't look correct, try clicking this dropdown button to see if another is available. Clicking an alternate title will change the FM's title to the one that you've selected.

<h4>Release date and Last played</h4>
If a date hasn't been scanned for or cannot be detected, its checkbox will be unchecked and no date will be shown.

<h4>Disabled mods</h4>
You can disable certain mods per-mission here. To see which mods you have installed, look at <code>cam_mod.ini</code> in your game folder. This string must be in the format <code>modname1+modname2+modname3</code> etc. So to disable the Enhancement Pack 2, it should be:

<p><code>ep2</code></p>

<p>To disable the Enhancement Pack 2 and the HD mod, it would be:</p>

<p><code>ep2+hdmod</code></p>

<p>To disable all mods for the current FM, check the <b>Disable all mods</b> checkbox.</p>

+ <h4>Language selection</h4>
+ Here you can choose to play an FM in a particular language. Only languages the FM supports will be available.
+
<h4>Comment tab</h4>

<p><img src="images/comment_tab.png" /></p>

Here you can enter a comment for the FM. This comment will also be displayed in the Comments column (up to the first 100 characters or the first linebreak, whichever comes first), and will update as you type.
```

```diff
<h3><a name="settings_fm_display_section" />FM Display section</h3>

<p><img src="images/setup_fm_display_tab.png" /></p>

<h4>Game organization</h4>
Here you can choose to either organize games into their own tabs, or to display your FMs as one list and allow filtering by game. When game tabs are enabled, each game will have its own selected FM and set of filters that will be retained between tab switches. Organizing games by tab can make things cleaner, but if you want to filter without regard to game (say, to find all missions by a single author who has released missions for multiple games), then having one list with game filters will work better.

<p>
  If you select the <b>Use short names on game tabs</b> checkbox, game tabs will be displayed with abbreviated names to save screen space.
</p>

<h4><a name="settings_sorting" />Sorting</h4>
Here you can choose to ignore leading articles when sorting FMs. For example, the FM "The Seven Sisters" will be considered to start with an "S". If you choose to move articles to the end of names when displaying them, then "The Seven Sisters" will be displayed as "Seven Sisters, The". The default set of articles is "a, an, the", but you can add more (for example to support other languages). These articles are not part of the normal localization functionality, because they apply to fan mission names, which can be any language; therefore the app-wide language setting doesn't apply to them.

<h4><a name="settings_rating_display_style" />Rating display style</h4>
Here you can choose the style in which to display an FM's rating (0-10, 0-5, or 0-5 with stars).

<h4><a name="settings_date_format" />Date format</h4>
Here you can choose how to display dates: either in the short or long form of your PC's current culture, or a custom format.

+ <h4><a name="settings_days_recent" />Recent FMs</h4>
+ When choosing to <a href="#show_recent_at_top">show recently added FMs at the top of the list</a>, only FMs added within the selected number of days will be included.
+
<h3><a name="settings_other_section" />Other section</h3>

<p><img src="images/setup_other_tab.png" /></p>
```

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
