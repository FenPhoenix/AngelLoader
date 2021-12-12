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

rem set fenGenArgs=-fmdata -language_t -add_build_date
rem set fenGenArgs=-fmdata -language_t -enable_lang_reflection_style_gen -add_build_date
set fenGenArgs=-fmdata -language_t -enable_lang_reflection_style_gen -add_build_date -gen_slim_designer_files
rem set fenGenArgs=-language_t -enable_lang_reflection_style_gen -add_build_date -gen_slim_designer_files

%FenGen% %fenGenArgs%
rem ---

"%system%xcopy" "%SolutionDir%temp_transfer_bin\*.*" "%TargetDir%" /y /i
