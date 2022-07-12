cd src\ClearRun.Console
set CURRENTPATH=%cd%

rem get the absolute path to the relative key file
CALL :NORMALIZEPATH "..\code_signing_key\ClearBible.pfx"

rem run the Inno Setup Compliler
rem "D:\Program Files (x86)\Inno Setup 6\ISCC.exe" "%CURRENTPATH%\DashboardInstaller.iss"


rem code sign the installer	
..\code_signing_key\signing_tool\signtool.exe ^
	sign /v /f %RETVAL% ^
	/p "9zm39UQ6kkr8GfkC" ^
	/t http://timestamp.comodoca.com/authenticode ^
	"%CURRENTPATH%\Output\ClearDashboard.exe"

pause

:: ========== FUNCTIONS ==========
EXIT /B

:NORMALIZEPATH
  SET RETVAL=%~f1
  EXIT /B

