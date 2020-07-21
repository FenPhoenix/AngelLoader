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
set FenGen="%SolutionDir%FenGen\bin\Release\net472\FenGen.exe"

set fenGenArgs=-fmdata -language_t
rem set fenGenArgs=-fmdata -language_t -enable_lang_reflection_style_gen

%FenGen% %fenGenArgs%
rem ---

"%system%xcopy" "%SolutionDir%temp_transfer_bin\*.*" "%TargetDir%" /y /i
