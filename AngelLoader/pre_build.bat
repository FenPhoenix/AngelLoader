echo ------------------------- START OF pre_build.bat

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
set "FenGen=%SolutionDir%FenGen\bin\Release\net472\FenGen.exe"

set fenGenArgs=-fmd -lang_t -bd -des -game -cr

%FenGen% %fenGenArgs%
rem ---

"%system%xcopy" "%SolutionDir%temp_transfer_bin\*.*" "%TargetDir%" /y /i
