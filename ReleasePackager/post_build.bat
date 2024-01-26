echo ------------------------- ReleasePackager: START OF post_build.bat

rem ~ strips surrounded quotes if they exist
rem batch file hell #9072: no spaces can exist around = sign for these lines
set ConfigurationName=%~1
set TargetDir=%~2
set ProjectDir=%~3
set SolutionDir=%~4
set PlatformName=%~5
set TargetFramework=%~6

rem batch file hell #21354: vars with spaces in the value must be entirely in quotes

rem ---

"%system%xcopy" "%ProjectDir%7z\*.*" "%TargetDir%" /y /i
