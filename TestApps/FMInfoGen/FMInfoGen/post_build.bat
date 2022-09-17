echo ------------------------- START OF post_build.bat

rem ~ strips surrounded quotes if they exist
rem batch file hell #9072: no spaces can exist around = sign for these lines
set ConfigurationName=%~1
set TargetDir=%~2
set ProjectDir=%~3
set SolutionDir=%~4

rem batch file hell #21354: vars with spaces in the value must be entirely in quotes

rem FMInfoGen needs this, or else FMScanner will throw on a dll not found crapgarbage!
"%system%xcopy" "%TargetDir%x86\7z.dll" "%TargetDir%" /y