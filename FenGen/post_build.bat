echo ------------------------- START OF FenGen/post_build.bat

rem ~ strips surrounded quotes if they exist
rem batch file hell #9072: no spaces can exist around = sign for these lines
set ConfigurationName=%~1
set TargetDir=%~2
set ProjectDir=%~3
set SolutionDir=%~4

echo SolutionDir: %SolutionDir%

rem Autogenerate code
rem ---
rem batch file hell #21354: vars with spaces in the value must be entirely in quotes
set "FenGen=%TargetDir%FenGen.exe"

rem Run the resx exclude BEFORE AngelLoader even gets to its build (and AFTER FenGen has already been built).
rem That way we should end up with a proper project file and no "sometimes works, sometimes doesn't" mess.
set fenGenArgs=-resx_e

%FenGen% %fenGenArgs%
rem ---