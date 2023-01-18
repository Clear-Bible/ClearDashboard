
robocopy $(TargetDir) $(SolutionDir)src\ClearDashboard.Wpf.Application\bin\Debug\net7.0-windows $(TargetName).*

exit 0

rem each robocopy statement and then underneath have the error check.
rem if %ERRORLEVEL% GEQ 8 goto failed

rem end of batch file
rem GOTO success

rem :failed
rem do not pause as it will pause msbuild.
rem exit 1

rem :success
rem exit 0    