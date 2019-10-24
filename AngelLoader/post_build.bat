echo ------------------------- START OF post_build.bat

rem ~ strips surrounded quotes if they exist
rem batch file hell #9072: no spaces can exist around = sign for these lines
set ConfigurationName=%~1
set TargetDir=%~2
set ProjectDir=%~3
set SolutionDir=%~4

if %ConfigurationName% == Release_Beta (
set destDir="C:\AngelLoader_Beta_Package\"
) else if %ConfigurationName% == Release_Public (
set destDir="C:\AngelLoader_Public_Package\"
) else (
set destDir="C:\AngelLoader\"
)

"%system%xcopy" "%TargetDir%AngelLoader.exe" "%destDir%" /y
rem dll.config is for .NET Core 3
rem "%system%xcopy" "%TargetDir%AngelLoader.dll.config" "%destDir%" /y
rem exe.config is for .NET Framework
"%system%xcopy" "%TargetDir%AngelLoader.exe.config" "%destDir%" /y

"%system%xcopy" "%TargetDir%*.dll" "%destDir%" /y

rem Dumb hack to get rid of extraneous dll files because ludicrously
rem xcopy requires you to make an entire file just to list excludes, rather than
rem specifying them on the command line like someone who is not clinically insane
del /F "%destDir%JetBrains.Annotations.dll"
del /F "%destDir%*xunit*.dll"
del /F "%destDir%*TestPlatform*.dll"
del /F "%destDir%testhost.dll"

rem "%system%xcopy" "%SolutionDir%libs\x86\7z.dll" "%destDir%" /y
rem "%system%xcopy" "%SolutionDir%\libs\x86\7z.dll" "%TargetDir%" /y

rem Inexplicably this doesn't work the first time. You have to build twice to get
rem the stupid file to copy.
rem Maybe there's some sort of "DependsOn" thing you can do. I dunno.
"%system%xcopy" "%TargetDir%x86\7z.dll" "%destDir%" /y
"%system%xcopy" "%TargetDir%x86\7z.dll" "%TargetDir%" /y

"%system%xcopy" "%SolutionDir%ffmpeg" "%destDir%ffmpeg\" /y /i

if %ConfigurationName% == Release_Public (
"%system%xcopy" "%ProjectDir%Languages\English.ini" "%destDir%Data\Languages\" /y
) else (
"%system%xcopy" "%ProjectDir%Languages" "%destDir%Data\Languages\" /y /i /e
)

"%system%xcopy" "%SolutionDir%BinReleaseOnly" "%destDir%" /y /i /e

"%system%xcopy" "%SolutionDir%doc" "%destDir%doc\" /y /i /e
