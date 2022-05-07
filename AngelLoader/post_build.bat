echo ------------------------- START OF post_build.bat

rem ~ strips surrounded quotes if they exist
rem batch file hell #9072: no spaces can exist around = sign for these lines
set ConfigurationName=%~1
set TargetDir=%~2
set ProjectDir=%~3
set SolutionDir=%~4

rem batch file hell #21354: vars with spaces in the value must be entirely in quotes

rem Autogenerate code
rem ---
set "FenGen=%SolutionDir%FenGen\bin\Release\net472\FenGen.exe"
set fenGenArgs=-resx_r -bd_r

%FenGen% %fenGenArgs%
rem ---

rem Still copy this for SevenZipSharp's use
"%system%xcopy" "%TargetDir%x86\7z.dll" "%TargetDir%" /y

"%system%xcopy" "%SolutionDir%bin_dependencies\7z32" "%TargetDir%\7z32\" /y /i
"%system%xcopy" "%SolutionDir%bin_dependencies\7z64" "%TargetDir%\7z64\" /y /i
"%system%xcopy" "%SolutionDir%bin_dependencies\*.ttf" "%TargetDir%" /y /i

rem Dumb hack to get rid of extraneous dll files because ludicrously
rem xcopy requires you to make an entire file just to list excludes, rather than
rem specifying them on the command line like someone who is not clinically insane
del /F "%TargetDir%JetBrains.Annotations.dll"
del /F "%TargetDir%*xunit*.dll"
del /F "%TargetDir%*TestPlatform*.dll"
del /F "%TargetDir%testhost.dll"
del /F "%TargetDir%EasyLoad*.dll"
del /F "%TargetDir%EasyHook64.dll"
del /F "%TargetDir%EasyHook32Svc.exe"
del /F "%TargetDir%EasyHook64Svc.exe"
del /F "%TargetDir%.gitkeep"

"%system%xcopy" "%SolutionDir%bin_dependencies\ffmpeg" "%TargetDir%ffmpeg\" /y /i

if %ConfigurationName% == Release_Public (
"%system%xcopy" "%ProjectDir%Languages\English.ini" "%TargetDir%Data\Languages\" /y
) else if %ConfigurationName% == Release_Beta (
"%system%xcopy" "%ProjectDir%Languages\English.ini" "%TargetDir%Data\Languages\" /y
) else (
"%system%xcopy" "%ProjectDir%Languages\*.ini" "%TargetDir%Data\Languages\" /y /i /e
)

"%system%xcopy" "%ProjectDir%Resources\AngelLoader.ico" "%TargetDir%" /y /i

"%system%xcopy" "%SolutionDir%BinReleaseOnly" "%TargetDir%" /y /i /e

rem Exlude "history" dir without having to copy and delete it afterwards (it's large) or write out an excludes file
"%system%xcopy" "%SolutionDir%doc\*.html" "%TargetDir%doc\" /y /i
"%system%xcopy" "%SolutionDir%doc\images" "%TargetDir%doc\images" /y /i /e

rem Personal local-only file (git-ignored). It contains stuff that is only appropriate for my personal setup and
rem might well mess up someone else's. So don't worry about it.
if exist "%ProjectDir%post_build_fen_personal_dev.bat" (
	"%ProjectDir%post_build_fen_personal_dev.bat" "%ConfigurationName%" "%TargetDir%" "%ProjectDir%" "%SolutionDir%"
)